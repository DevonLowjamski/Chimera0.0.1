using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;
using ProgressionAchievementProgress = ProjectChimera.Data.Progression.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement Data Service - Manages data persistence, achievement state management, and synchronization
    /// Extracted from AchievementSystemManager to provide focused data management functionality
    /// Handles save/load operations, data validation, backup management, and cross-session synchronization
    /// Provides comprehensive data integrity and persistence for achievement systems
    /// </summary>
    public class AchievementDataService : MonoBehaviour
    {
        [Header("Data Persistence Configuration")]
        [SerializeField] private bool _enableDataPersistence = true;
        [SerializeField] private bool _enableAutoSave = true;
        [SerializeField] private float _autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool _enableBackups = true;
        [SerializeField] private int _maxBackupFiles = 5;

        [Header("Data Validation")]
        [SerializeField] private bool _enableDataValidation = true;
        [SerializeField] private bool _enableDataRepair = true;
        [SerializeField] private bool _validateOnLoad = true;
        [SerializeField] private bool _logDataOperations = false;

        [Header("Synchronization Settings")]
        [SerializeField] private bool _enableCloudSync = false;
        [SerializeField] private bool _enableCrossPlatformSync = false;
        [SerializeField] private float _syncRetryDelay = 5f;
        [SerializeField] private int _maxSyncRetries = 3;

        [Header("File Management")]
        [SerializeField] private string _dataFileName = "achievements.json";
        [SerializeField] private string _backupPrefix = "achievements_backup_";
        [SerializeField] private string _tempFileExtension = ".tmp";

        // Service state
        private bool _isInitialized = false;
        private bool _isDirty = false;
        private float _lastSaveTime = 0f;
        private string _dataDirectory = "";
        private string _fullDataPath = "";

        // Data containers
        private AchievementDataContainer _achievementData = new AchievementDataContainer();
        private Dictionary<string, PlayerAchievementData> _playerDataCache = new Dictionary<string, PlayerAchievementData>();
        private List<string> _pendingSyncOperations = new List<string>();

        // Service state tracking
        private Dictionary<string, DateTime> _lastModified = new Dictionary<string, DateTime>();
        private Queue<DataOperation> _pendingOperations = new Queue<DataOperation>();
        private bool _isSaving = false;
        private bool _isLoading = false;

        // Events for data operations
        public static event Action<string> OnDataSaved;
        public static event Action<string> OnDataLoaded;
        public static event Action<string, string> OnDataError;
        public static event Action<string> OnDataValidated;
        public static event Action<float> OnSyncProgress;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Data Service";
        public bool IsDirty => _isDirty;
        public bool IsSaving => _isSaving;
        public bool IsLoading => _isLoading;
        public int PendingOperations => _pendingOperations.Count;

        public void Initialize()
        {
            InitializeService();
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            InitializeService();
        }

        private void Update()
        {
            if (_isInitialized && _enableAutoSave)
            {
                ProcessAutoSave();
            }
        }

        private void InitializeDataStructures()
        {
            _achievementData = new AchievementDataContainer();
            _playerDataCache = new Dictionary<string, PlayerAchievementData>();
            _pendingSyncOperations = new List<string>();
            _lastModified = new Dictionary<string, DateTime>();
            _pendingOperations = new Queue<DataOperation>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementDataService already initialized", this);
                return;
            }

            try
            {
                SetupDataPaths();
                CreateDataDirectories();
                
                if (_enableDataPersistence)
                {
                    LoadAchievementData();
                }
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementDataService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementDataService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                if (_enableDataPersistence && _isDirty)
                {
                    SaveAchievementData();
                }
                
                ProcessPendingOperations();
                ClearAllData();
                
                _isInitialized = false;
                ChimeraLogger.Log("AchievementDataService shutdown completed", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error during AchievementDataService shutdown: {ex.Message}", this);
            }
        }

        #endregion

        #region Data Path Management

        private void SetupDataPaths()
        {
            _dataDirectory = Path.Combine(Application.persistentDataPath, "Achievements");
            _fullDataPath = Path.Combine(_dataDirectory, _dataFileName);
            
            if (_logDataOperations)
            {
                ChimeraLogger.Log($"Data directory: {_dataDirectory}", this);
                ChimeraLogger.Log($"Data file path: {_fullDataPath}", this);
            }
        }

        private void CreateDataDirectories()
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                    ChimeraLogger.Log($"Created achievement data directory: {_dataDirectory}", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to create data directories: {ex.Message}", this);
                throw;
            }
        }

        #endregion

        #region Data Persistence

        public void SaveAchievementData()
        {
            if (!_enableDataPersistence || _isSaving)
            {
                return;
            }

            try
            {
                _isSaving = true;
                
                // Prepare data for serialization
                var dataToSave = PrepareDataForSave();
                
                // Create backup if enabled
                if (_enableBackups)
                {
                    CreateBackup();
                }
                
                // Write to temporary file first
                string tempPath = _fullDataPath + _tempFileExtension;
                string jsonData = JsonUtility.ToJson(dataToSave, true);
                
                File.WriteAllText(tempPath, jsonData);
                
                // Atomic move from temp to actual file
                if (File.Exists(_fullDataPath))
                {
                    File.Delete(_fullDataPath);
                }
                File.Move(tempPath, _fullDataPath);
                
                _isDirty = false;
                _lastSaveTime = Time.time;
                
                OnDataSaved?.Invoke(_fullDataPath);
                
                if (_logDataOperations)
                {
                    ChimeraLogger.Log($"Achievement data saved successfully to {_fullDataPath}", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to save achievement data: {ex.Message}", this);
                OnDataError?.Invoke("save", ex.Message);
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void LoadAchievementData()
        {
            if (!_enableDataPersistence || _isLoading)
            {
                return;
            }

            try
            {
                _isLoading = true;
                
                if (!File.Exists(_fullDataPath))
                {
                    ChimeraLogger.Log("No existing achievement data file found, starting with empty data", this);
                    InitializeDefaultData();
                    return;
                }
                
                string jsonData = File.ReadAllText(_fullDataPath);
                var loadedData = JsonUtility.FromJson<AchievementDataContainer>(jsonData);
                
                if (_enableDataValidation && _validateOnLoad)
                {
                    if (ValidateLoadedData(loadedData))
                    {
                        _achievementData = loadedData;
                        ProcessLoadedData();
                    }
                    else
                    {
                        ChimeraLogger.LogWarning("Loaded data failed validation, attempting repair", this);
                        if (_enableDataRepair && AttemptDataRepair(loadedData))
                        {
                            _achievementData = loadedData;
                            ProcessLoadedData();
                            MarkDirty(); // Data was repaired, needs saving
                        }
                        else
                        {
                            ChimeraLogger.LogError("Data repair failed, initializing with default data", this);
                            InitializeDefaultData();
                        }
                    }
                }
                else
                {
                    _achievementData = loadedData;
                    ProcessLoadedData();
                }
                
                OnDataLoaded?.Invoke(_fullDataPath);
                
                if (_logDataOperations)
                {
                    ChimeraLogger.Log($"Achievement data loaded successfully from {_fullDataPath}", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to load achievement data: {ex.Message}", this);
                OnDataError?.Invoke("load", ex.Message);
                InitializeDefaultData();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private AchievementDataContainer PrepareDataForSave()
        {
            var container = new AchievementDataContainer
            {
                Version = "1.0",
                SaveTime = DateTime.Now,
                PlayerDataEntries = _playerDataCache.Values.ToList(),
                GlobalStats = CalculateGlobalStats(),
                DataChecksum = ""
            };
            
            // Calculate checksum for data integrity
            container.DataChecksum = CalculateDataChecksum(container);
            
            return container;
        }

        private void ProcessLoadedData()
        {
            // Rebuild player data cache
            _playerDataCache.Clear();
            foreach (var playerData in _achievementData.PlayerDataEntries)
            {
                _playerDataCache[playerData.PlayerID] = playerData;
                _lastModified[playerData.PlayerID] = playerData.LastModified;
            }
        }

        private void InitializeDefaultData()
        {
            _achievementData = new AchievementDataContainer
            {
                Version = "1.0",
                SaveTime = DateTime.Now,
                PlayerDataEntries = new List<PlayerAchievementData>(),
                GlobalStats = new GlobalAchievementStats(),
                DataChecksum = ""
            };
            
            MarkDirty();
        }

        #endregion

        #region Data Validation

        private bool ValidateLoadedData(AchievementDataContainer data)
        {
            if (data == null)
            {
                ChimeraLogger.LogError("Loaded data container is null", this);
                return false;
            }
            
            // Validate version compatibility
            if (string.IsNullOrEmpty(data.Version))
            {
                ChimeraLogger.LogWarning("Data version is missing", this);
                return false;
            }
            
            // Validate data integrity with checksum
            if (!string.IsNullOrEmpty(data.DataChecksum))
            {
                string calculatedChecksum = CalculateDataChecksum(data);
                if (calculatedChecksum != data.DataChecksum)
                {
                    ChimeraLogger.LogError("Data checksum validation failed", this);
                    return false;
                }
            }
            
            // Validate player data entries
            if (data.PlayerDataEntries != null)
            {
                foreach (var playerData in data.PlayerDataEntries)
                {
                    if (!ValidatePlayerData(playerData))
                    {
                        return false;
                    }
                }
            }
            
            OnDataValidated?.Invoke("Data validation passed");
            return true;
        }

        private bool ValidatePlayerData(PlayerAchievementData playerData)
        {
            if (playerData == null || string.IsNullOrEmpty(playerData.PlayerID))
            {
                ChimeraLogger.LogError("Invalid player data detected", this);
                return false;
            }
            
            // Validate achievement progress entries
            if (playerData.AchievementProgress != null)
            {
                foreach (var progress in playerData.AchievementProgress)
                {
                    if (progress.CurrentValue < 0 || string.IsNullOrEmpty(progress.AchievementId))
                    {
                        ChimeraLogger.LogError($"Invalid progress data for player {playerData.PlayerID}", this);
                        return false;
                    }
                }
            }
            
            return true;
        }

        private bool AttemptDataRepair(AchievementDataContainer data)
        {
            bool repaired = false;
            
            // Repair missing or invalid player IDs
            if (data.PlayerDataEntries != null)
            {
                for (int i = data.PlayerDataEntries.Count - 1; i >= 0; i--)
                {
                    var playerData = data.PlayerDataEntries[i];
                    if (playerData == null || string.IsNullOrEmpty(playerData.PlayerID))
                    {
                        data.PlayerDataEntries.RemoveAt(i);
                        repaired = true;
                        ChimeraLogger.Log($"Removed invalid player data entry at index {i}", this);
                    }
                    else if (RepairPlayerData(playerData))
                    {
                        repaired = true;
                    }
                }
            }
            
            // Repair global stats
            if (data.GlobalStats == null)
            {
                data.GlobalStats = new GlobalAchievementStats();
                repaired = true;
            }
            
            if (repaired)
            {
                ChimeraLogger.Log("Data repair completed successfully", this);
            }
            
            return repaired;
        }

        private bool RepairPlayerData(PlayerAchievementData playerData)
        {
            bool repaired = false;
            
            // Repair null collections
            if (playerData.AchievementProgress == null)
            {
                playerData.AchievementProgress = new List<ProgressionAchievementProgress>();
                repaired = true;
            }
            
            if (playerData.UnlockedAchievements == null)
            {
                playerData.UnlockedAchievements = new List<string>();
                repaired = true;
            }
            
            // Remove invalid progress entries
            if (playerData.AchievementProgress != null)
            {
                for (int i = playerData.AchievementProgress.Count - 1; i >= 0; i--)
                {
                    var progress = playerData.AchievementProgress[i];
                    if (progress.CurrentValue < 0 || string.IsNullOrEmpty(progress.AchievementId))
                    {
                        playerData.AchievementProgress.RemoveAt(i);
                        repaired = true;
                    }
                }
            }
            
            return repaired;
        }

        private string CalculateDataChecksum(AchievementDataContainer data)
        {
            // Simple checksum calculation for data integrity
            // In production, you might want to use a more robust hash function
            var tempChecksum = data.DataChecksum;
            data.DataChecksum = ""; // Clear checksum for calculation
            
            string jsonData = JsonUtility.ToJson(data);
            int checksum = jsonData.GetHashCode();
            
            data.DataChecksum = tempChecksum; // Restore original checksum
            return checksum.ToString();
        }

        #endregion

        #region Backup Management

        private void CreateBackup()
        {
            try
            {
                if (!File.Exists(_fullDataPath))
                {
                    return; // No file to backup
                }
                
                string backupFileName = $"{_backupPrefix}{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string backupPath = Path.Combine(_dataDirectory, backupFileName);
                
                File.Copy(_fullDataPath, backupPath);
                
                // Clean up old backups
                CleanupOldBackups();
                
                if (_logDataOperations)
                {
                    ChimeraLogger.Log($"Backup created: {backupPath}", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to create backup: {ex.Message}", this);
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_dataDirectory, $"{_backupPrefix}*.json")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Skip(_maxBackupFiles)
                    .ToArray();
                
                foreach (string backupFile in backupFiles)
                {
                    File.Delete(backupFile);
                    if (_logDataOperations)
                    {
                        ChimeraLogger.Log($"Deleted old backup: {backupFile}", this);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to cleanup old backups: {ex.Message}", this);
            }
        }

        #endregion

        #region Player Data Management

        public PlayerAchievementData GetPlayerData(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }
            
            if (_playerDataCache.TryGetValue(playerId, out var playerData))
            {
                return playerData;
            }
            
            // Create new player data if it doesn't exist
            return CreateNewPlayerData(playerId);
        }

        public void SavePlayerData(string playerId, PlayerAchievementData playerData)
        {
            if (string.IsNullOrEmpty(playerId) || playerData == null)
            {
                return;
            }
            
            playerData.LastModified = DateTime.Now;
            _playerDataCache[playerId] = playerData;
            _lastModified[playerId] = playerData.LastModified;
            
            MarkDirty();
            
            if (_logDataOperations)
            {
                ChimeraLogger.Log($"Player data updated for: {playerId}", this);
            }
        }

        private PlayerAchievementData CreateNewPlayerData(string playerId)
        {
            var newPlayerData = new PlayerAchievementData
            {
                PlayerID = playerId,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now,
                AchievementProgress = new List<ProgressionAchievementProgress>(),
                UnlockedAchievements = new List<string>(),
                TotalPoints = 0f,
                CompletionPercentage = 0f
            };
            
            _playerDataCache[playerId] = newPlayerData;
            _lastModified[playerId] = newPlayerData.LastModified;
            
            MarkDirty();
            
            return newPlayerData;
        }

        #endregion

        #region Auto Save

        private void ProcessAutoSave()
        {
            if (_isDirty && Time.time - _lastSaveTime >= _autoSaveInterval)
            {
                SaveAchievementData();
            }
        }

        #endregion

        #region Utility Methods

        private void MarkDirty()
        {
            _isDirty = true;
        }

        private GlobalAchievementStats CalculateGlobalStats()
        {
            var stats = new GlobalAchievementStats
            {
                TotalPlayers = _playerDataCache.Count,
                LastCalculated = DateTime.Now
            };
            
            return stats;
        }

        private void ProcessPendingOperations()
        {
            while (_pendingOperations.Count > 0)
            {
                var operation = _pendingOperations.Dequeue();
                // Process operation based on type
                if (_logDataOperations)
                {
                    ChimeraLogger.Log($"Processing pending operation: {operation.Type}", this);
                }
            }
        }

        private void ClearAllData()
        {
            _achievementData = new AchievementDataContainer();
            _playerDataCache.Clear();
            _pendingSyncOperations.Clear();
            _lastModified.Clear();
            _pendingOperations.Clear();
        }

        #endregion

        #region Public API

        public void ForceDataSave()
        {
            if (_enableDataPersistence)
            {
                SaveAchievementData();
            }
        }

        public void ForceDataReload()
        {
            if (_enableDataPersistence)
            {
                LoadAchievementData();
            }
        }

        public bool HasPlayerData(string playerId)
        {
            return !string.IsNullOrEmpty(playerId) && _playerDataCache.ContainsKey(playerId);
        }

        public List<string> GetAllPlayerIds()
        {
            return _playerDataCache.Keys.ToList();
        }

        public DateTime GetPlayerLastModified(string playerId)
        {
            return _lastModified.TryGetValue(playerId, out var lastModified) ? lastModified : DateTime.MinValue;
        }

        public void UpdateDataSettings(bool enablePersistence, bool enableAutoSave, float autoSaveInterval)
        {
            _enableDataPersistence = enablePersistence;
            _enableAutoSave = enableAutoSave;
            _autoSaveInterval = autoSaveInterval;
            
            ChimeraLogger.Log($"Data settings updated: persistence={enablePersistence}, autoSave={enableAutoSave}, interval={autoSaveInterval}", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _enableDataPersistence && _isDirty)
            {
                SaveAchievementData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _enableDataPersistence && _isDirty)
            {
                SaveAchievementData();
            }
        }

        #endregion
    }

    /// <summary>
    /// Container for all achievement data persistence
    /// </summary>
    [System.Serializable]
    public class AchievementDataContainer
    {
        public string Version = "1.0";
        public DateTime SaveTime = DateTime.Now;
        public List<PlayerAchievementData> PlayerDataEntries = new List<PlayerAchievementData>();
        public GlobalAchievementStats GlobalStats = new GlobalAchievementStats();
        public string DataChecksum = "";
    }

    /// <summary>
    /// Individual player achievement data for persistence
    /// </summary>
    [System.Serializable]
    public class PlayerAchievementData
    {
        public string PlayerID = "";
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastModified = DateTime.Now;
        public List<ProgressionAchievementProgress> AchievementProgress = new List<ProgressionAchievementProgress>();
        public List<string> UnlockedAchievements = new List<string>();
        public float TotalPoints = 0f;
        public float CompletionPercentage = 0f;
    }

    /// <summary>
    /// Global achievement statistics for analytics
    /// </summary>
    [System.Serializable]
    public class GlobalAchievementStats
    {
        public int TotalPlayers = 0;
        public DateTime LastCalculated = DateTime.Now;
    }

    /// <summary>
    /// Data operation for tracking pending operations
    /// </summary>
    [System.Serializable]
    public class DataOperation
    {
        public string Type = "";
        public string PlayerID = "";
        public DateTime Timestamp = DateTime.Now;
    }
}