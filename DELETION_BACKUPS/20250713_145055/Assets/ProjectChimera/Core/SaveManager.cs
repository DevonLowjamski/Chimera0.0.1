using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Manages save/load operations for Project Chimera using Data Transfer Objects (DTOs).
    /// Handles multiple save slots, auto-save, and data serialization/deserialization.
    /// </summary>
    public class SaveManager : ChimeraManager, IGameStateListener
    {
        [Header("Save Configuration")]
        [SerializeField] private SaveManagerConfigSO _config;
        [SerializeField] private string _saveFilePrefix = "ProjectChimera_Save_";
        [SerializeField] private string _saveFileExtension = ".json";
        [SerializeField] private bool _compresseSaveFiles = true;
        [SerializeField] private bool _encryptSaveFiles = false;

        [Header("Auto-Save Settings")]
        [SerializeField] private bool _enableAutoSave = true;
        [SerializeField] private float _autoSaveInterval = 300.0f; // 5 minutes
        [SerializeField] private int _maxAutoSaves = 5;

        [Header("Save Events")]
        [SerializeField] private StringGameEventSO _onSaveStarted;
        [SerializeField] private StringGameEventSO _onSaveCompleted;
        [SerializeField] private StringGameEventSO _onLoadStarted;
        [SerializeField] private StringGameEventSO _onLoadCompleted;
        [SerializeField] private StringGameEventSO _onSaveError;

        // Save system state
        private string _saveDirectory;
        private readonly Dictionary<int, SaveSlotInfo> _saveSlots = new Dictionary<int, SaveSlotInfo>();
        private readonly List<ISaveable> _saveableComponents = new List<ISaveable>();
        private Coroutine _autoSaveCoroutine;
        private bool _isSaving = false;
        private bool _isLoading = false;

        // Performance tracking
        private float _lastSaveTime = 0.0f;
        private float _lastLoadTime = 0.0f;
        private int _totalSaves = 0;
        private int _totalLoads = 0;

        /// <summary>
        /// Whether a save operation is currently in progress.
        /// </summary>
        public bool IsSaving => _isSaving;

        /// <summary>
        /// Whether a load operation is currently in progress.
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Directory where save files are stored.
        /// </summary>
        public string SaveDirectory => _saveDirectory;

        /// <summary>
        /// Number of registered saveable components.
        /// </summary>
        public int SaveableComponentCount => _saveableComponents.Count;

        /// <summary>
        /// Time when the last save operation completed.
        /// </summary>
        public float LastSaveTime => _lastSaveTime;

        /// <summary>
        /// Time when the last load operation completed.
        /// </summary>
        public float LastLoadTime => _lastLoadTime;

        /// <summary>
        /// Total number of save operations performed.
        /// </summary>
        public int TotalSaves => _totalSaves;

        /// <summary>
        /// Total number of load operations performed.
        /// </summary>
        public int TotalLoads => _totalLoads;

        protected override void OnManagerInitialize()
        {
            LogDebug("Initializing Save Manager");

            // Load configuration
            if (_config != null)
            {
                _saveFilePrefix = _config.SaveFilePrefix;
                _saveFileExtension = _config.SaveFileExtension;
                _compresseSaveFiles = _config.CompressSaveFiles;
                _encryptSaveFiles = _config.EncryptSaveFiles;
                _enableAutoSave = _config.EnableAutoSave;
                _autoSaveInterval = _config.AutoSaveInterval;
                _maxAutoSaves = _config.MaxAutoSaves;
            }

            // Set up save directory
            _saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            CreateSaveDirectory();

            // Discover existing save files
            DiscoverSaveSlots();

            // Auto-discover saveable components
            DiscoverSaveableComponents();

            // Start auto-save if enabled
            if (_enableAutoSave)
            {
                _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }

            LogDebug($"Save Manager initialized - {_saveSlots.Count} save slots found, {_saveableComponents.Count} saveable components registered");
        }

        protected override void OnManagerShutdown()
        {
            LogDebug("Shutting down Save Manager");

            // Stop auto-save
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
                _autoSaveCoroutine = null;
            }

            // Clear registries
            _saveSlots.Clear();
            _saveableComponents.Clear();

            _isSaving = false;
            _isLoading = false;
        }

        /// <summary>
        /// Creates the save directory if it doesn't exist.
        /// </summary>
        private void CreateSaveDirectory()
        {
            try
            {
                if (!Directory.Exists(_saveDirectory))
                {
                    Directory.CreateDirectory(_saveDirectory);
                    LogDebug($"Created save directory: {_saveDirectory}");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to create save directory: {e.Message}");
            }
        }

        /// <summary>
        /// Discovers existing save files and populates save slot information.
        /// </summary>
        private void DiscoverSaveSlots()
        {
            try
            {
                string[] saveFiles = Directory.GetFiles(_saveDirectory, $"{_saveFilePrefix}*{_saveFileExtension}");
                
                foreach (string filePath in saveFiles)
                {
                    if (TryParseSaveSlotFromFileName(Path.GetFileName(filePath), out int slotNumber))
                    {
                        var fileInfo = new FileInfo(filePath);
                        var slotInfo = new SaveSlotInfo
                        {
                            SlotNumber = slotNumber,
                            FilePath = filePath,
                            LastModified = fileInfo.LastWriteTime,
                            FileSize = fileInfo.Length,
                            IsAutoSave = filePath.Contains("AutoSave")
                        };

                        // Try to read save metadata
                        if (TryReadSaveMetadata(filePath, out SaveMetadata metadata))
                        {
                            slotInfo.GameVersion = metadata.GameVersion;
                            slotInfo.SaveTime = metadata.SaveTime;
                            slotInfo.PlayTime = metadata.PlayTime;
                            slotInfo.Description = metadata.Description;
                        }

                        _saveSlots[slotNumber] = slotInfo;
                    }
                }

                LogDebug($"Discovered {_saveSlots.Count} save slots");
            }
            catch (Exception e)
            {
                LogError($"Error discovering save slots: {e.Message}");
            }
        }

        /// <summary>
        /// Discovers and registers all saveable components in the scene.
        /// </summary>
        private void DiscoverSaveableComponents()
        {
            var saveableObjects = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
            foreach (var saveable in saveableObjects)
            {
                RegisterSaveableComponent(saveable);
            }
        }

        /// <summary>
        /// Registers a saveable component with the save system.
        /// </summary>
        public void RegisterSaveableComponent(ISaveable saveable)
        {
            if (saveable != null && !_saveableComponents.Contains(saveable))
            {
                _saveableComponents.Add(saveable);
                LogDebug($"Registered saveable component: {saveable.GetType().Name}");
            }
        }

        /// <summary>
        /// Unregisters a saveable component from the save system.
        /// </summary>
        public void UnregisterSaveableComponent(ISaveable saveable)
        {
            if (_saveableComponents.Remove(saveable))
            {
                LogDebug($"Unregistered saveable component: {saveable.GetType().Name}");
            }
        }

        /// <summary>
        /// Saves the game to the specified slot.
        /// </summary>
        public void SaveGame(int slotNumber, string description = "")
        {
            if (_isSaving)
            {
                LogWarning("Save operation already in progress");
                return;
            }

            StartCoroutine(SaveGameCoroutine(slotNumber, description, false));
        }

        /// <summary>
        /// Performs an auto-save operation.
        /// </summary>
        public void AutoSave()
        {
            if (_isSaving || _isLoading)
            {
                LogDebug("Skipping auto-save - operation in progress");
                return;
            }

            int autoSaveSlot = GetNextAutoSaveSlot();
            StartCoroutine(SaveGameCoroutine(autoSaveSlot, "Auto Save", true));
        }

        /// <summary>
        /// Loads the game from the specified slot.
        /// </summary>
        public void LoadGame(int slotNumber)
        {
            if (_isLoading)
            {
                LogWarning("Load operation already in progress");
                return;
            }

            if (!_saveSlots.ContainsKey(slotNumber))
            {
                LogError($"Save slot {slotNumber} does not exist");
                return;
            }

            StartCoroutine(LoadGameCoroutine(slotNumber));
        }

        /// <summary>
        /// Coroutine that handles the save operation.
        /// </summary>
        private IEnumerator SaveGameCoroutine(int slotNumber, string description, bool isAutoSave)
        {
            _isSaving = true;
            string operation = isAutoSave ? "Auto Save" : "Manual Save";
            
            LogDebug($"Starting {operation} to slot {slotNumber}");
            _onSaveStarted?.Raise($"Slot {slotNumber}: {description}");

            GameSaveData saveData = null;
            string filePath = null;
            bool saveSuccessful = false;

            try
            {
                // Create save data object
                saveData = new GameSaveData
                {
                    Metadata = new SaveMetadata
                    {
                        SlotNumber = slotNumber,
                        Description = description,
                        SaveTime = DateTime.Now,
                        GameVersion = Application.version,
                        PlayTime = GameManager.Instance?.TotalGameTime.TotalHours ?? 0.0,
                        IsAutoSave = isAutoSave
                    },
                    ComponentData = new Dictionary<string, object>()
                };

                // Collect data from all saveable components
                foreach (var saveable in _saveableComponents)
                {
                    try
                    {
                        string componentId = saveable.GetSaveId();
                        object componentData = saveable.GetSaveData();
                        
                        if (!string.IsNullOrEmpty(componentId) && componentData != null)
                        {
                            saveData.ComponentData[componentId] = componentData;
                        }
                    }
                    catch (Exception e)
                    {
                        LogError($"Error collecting save data from {saveable.GetType().Name}: {e.Message}");
                    }
                }

                filePath = GetSaveFilePath(slotNumber, isAutoSave);
                saveSuccessful = true;
            }
            catch (Exception e)
            {
                LogError($"{operation} failed: {e.Message}");
                _onSaveError?.Raise($"Save failed: {e.Message}");
                saveSuccessful = false;
            }
            finally
            {
                _isSaving = false;
            }

            // Yield after processing all components (outside try-catch)
            yield return null;

            // Serialize and write to file (outside try-catch)
            if (saveSuccessful && saveData != null && !string.IsNullOrEmpty(filePath))
            {
                yield return StartCoroutine(WriteSaveFileCoroutine(filePath, saveData));

                // Update slot information
                var slotInfo = new SaveSlotInfo
                {
                    SlotNumber = slotNumber,
                    FilePath = filePath,
                    LastModified = DateTime.Now,
                    FileSize = new FileInfo(filePath).Length,
                    IsAutoSave = isAutoSave,
                    GameVersion = saveData.Metadata.GameVersion,
                    SaveTime = saveData.Metadata.SaveTime,
                    PlayTime = saveData.Metadata.PlayTime,
                    Description = description
                };

                _saveSlots[slotNumber] = slotInfo;
                _lastSaveTime = Time.time;
                _totalSaves++;

                LogDebug($"{operation} completed successfully");
                _onSaveCompleted?.Raise($"Slot {slotNumber}: {description}");
            }
        }

        /// <summary>
        /// Coroutine that handles the load operation.
        /// </summary>
        private IEnumerator LoadGameCoroutine(int slotNumber)
        {
            _isLoading = true;
            
            LogDebug($"Starting load from slot {slotNumber}");
            _onLoadStarted?.Raise($"Slot {slotNumber}");

            string filePath = null;
            GameSaveData saveData = null;
            bool loadSuccessful = false;

            try
            {
                filePath = _saveSlots[slotNumber].FilePath;
                loadSuccessful = true;
            }
            catch (Exception e)
            {
                LogError($"Load failed: {e.Message}");
                _onSaveError?.Raise($"Load failed: {e.Message}");
                loadSuccessful = false;
            }
            finally
            {
                _isLoading = false;
            }

            // Read and deserialize save file (outside try-catch)
            if (loadSuccessful && !string.IsNullOrEmpty(filePath))
            {
                yield return StartCoroutine(ReadSaveFileCoroutine(filePath, data => saveData = data));

                if (saveData == null)
                {
                    LogError("Failed to read save file");
                    _onSaveError?.Raise($"Load failed: Failed to read save file");
                }
                else
                {
                    // Apply data to all saveable components
                    foreach (var saveable in _saveableComponents)
                    {
                        try
                        {
                            string componentId = saveable.GetSaveId();
                            
                            if (saveData.ComponentData.TryGetValue(componentId, out object componentData))
                            {
                                saveable.LoadSaveData(componentData);
                            }
                        }
                        catch (Exception e)
                        {
                            LogError($"Error loading save data to {saveable.GetType().Name}: {e.Message}");
                        }
                    }

                    // Yield after processing all components (outside try-catch)
                    yield return null;

                    _lastLoadTime = Time.time;
                    _totalLoads++;

                    LogDebug("Load completed successfully");
                    _onLoadCompleted?.Raise($"Slot {slotNumber}");
                }
            }
        }

        /// <summary>
        /// Coroutine that writes save data to a file.
        /// </summary>
        private IEnumerator WriteSaveFileCoroutine(string filePath, GameSaveData saveData)
        {
            yield return null; // Yield before starting file operations

            try
            {
                string jsonData = JsonUtility.ToJson(saveData, true);
                
                // Write to temporary file first
                string tempPath = filePath + ".tmp";
                File.WriteAllText(tempPath, jsonData);
                
                // Move temporary file to final location (atomic operation)
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);

                LogDebug($"Save file written: {filePath}");
            }
            catch (Exception e)
            {
                LogError($"Error writing save file: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Coroutine that reads save data from a file.
        /// </summary>
        private IEnumerator ReadSaveFileCoroutine(string filePath, Action<GameSaveData> callback)
        {
            yield return null; // Yield before starting file operations

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Save file not found: {filePath}");
                }

                string jsonData = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                callback(saveData);
                LogDebug($"Save file read: {filePath}");
            }
            catch (Exception e)
            {
                LogError($"Error reading save file: {e.Message}");
                callback(null);
            }
        }

        /// <summary>
        /// Auto-save coroutine that runs continuously.
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveInterval);
                
                if (GameManager.Instance?.CurrentGameState == GameState.Running && !_isSaving && !_isLoading)
                {
                    AutoSave();
                }
            }
        }

        /// <summary>
        /// Gets the next available auto-save slot number.
        /// </summary>
        private int GetNextAutoSaveSlot()
        {
            int baseSlot = 9000; // Auto-save slots start at 9000
            for (int i = 0; i < _maxAutoSaves; i++)
            {
                int slotNumber = baseSlot + i;
                if (!_saveSlots.ContainsKey(slotNumber))
                {
                    return slotNumber;
                }
            }

            // If all auto-save slots are full, overwrite the oldest
            var autoSaves = _saveSlots.Values.Where(s => s.IsAutoSave).OrderBy(s => s.LastModified);
            return autoSaves.First().SlotNumber;
        }

        /// <summary>
        /// Gets the file path for a save slot.
        /// </summary>
        private string GetSaveFilePath(int slotNumber, bool isAutoSave)
        {
            string fileName = isAutoSave ? 
                $"{_saveFilePrefix}AutoSave_{slotNumber:D4}{_saveFileExtension}" :
                $"{_saveFilePrefix}{slotNumber:D4}{_saveFileExtension}";
            
            return Path.Combine(_saveDirectory, fileName);
        }

        /// <summary>
        /// Tries to parse a slot number from a save file name.
        /// </summary>
        private bool TryParseSaveSlotFromFileName(string fileName, out int slotNumber)
        {
            slotNumber = -1;

            if (!fileName.StartsWith(_saveFilePrefix) || !fileName.EndsWith(_saveFileExtension))
            {
                return false;
            }

            string numberPart = fileName.Substring(_saveFilePrefix.Length);
            numberPart = numberPart.Substring(0, numberPart.Length - _saveFileExtension.Length);

            // Handle auto-save naming
            if (numberPart.StartsWith("AutoSave_"))
            {
                numberPart = numberPart.Substring("AutoSave_".Length);
            }

            return int.TryParse(numberPart, out slotNumber);
        }

        /// <summary>
        /// Tries to read save metadata from a file.
        /// </summary>
        private bool TryReadSaveMetadata(string filePath, out SaveMetadata metadata)
        {
            metadata = default;

            try
            {
                string jsonData = File.ReadAllText(filePath);
                var saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                metadata = saveData.Metadata;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets information about all save slots.
        /// </summary>
        public List<SaveSlotInfo> GetSaveSlots()
        {
            return new List<SaveSlotInfo>(_saveSlots.Values);
        }

        /// <summary>
        /// Gets information about a specific save slot.
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(int slotNumber)
        {
            return _saveSlots.TryGetValue(slotNumber, out SaveSlotInfo info) ? info : default;
        }

        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        public void DeleteSaveSlot(int slotNumber)
        {
            if (!_saveSlots.TryGetValue(slotNumber, out SaveSlotInfo slotInfo))
            {
                LogWarning($"Cannot delete save slot {slotNumber} - slot not found");
                return;
            }

            try
            {
                if (File.Exists(slotInfo.FilePath))
                {
                    File.Delete(slotInfo.FilePath);
                }

                _saveSlots.Remove(slotNumber);
                LogDebug($"Deleted save slot {slotNumber}");
            }
            catch (Exception e)
            {
                LogError($"Error deleting save slot {slotNumber}: {e.Message}");
            }
        }

        /// <summary>
        /// Handles game state changes.
        /// </summary>
        public void OnGameStateChanged(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Shutdown:
                    // Perform final auto-save on shutdown
                    if (_enableAutoSave && !_isSaving && !_isLoading)
                    {
                        AutoSave();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Interface for components that can be saved and loaded.
    /// </summary>
    public interface ISaveable
    {
        string GetSaveId();
        object GetSaveData();
        void LoadSaveData(object data);
    }

    /// <summary>
    /// Information about a save slot.
    /// </summary>
    [Serializable]
    public struct SaveSlotInfo
    {
        public int SlotNumber;
        public string FilePath;
        public DateTime LastModified;
        public long FileSize;
        public bool IsAutoSave;
        public string GameVersion;
        public DateTime SaveTime;
        public double PlayTime;
        public string Description;
    }

    /// <summary>
    /// Metadata stored with each save file.
    /// </summary>
    [Serializable]
    public struct SaveMetadata
    {
        public int SlotNumber;
        public string Description;
        public DateTime SaveTime;
        public string GameVersion;
        public double PlayTime;
        public bool IsAutoSave;
    }

    /// <summary>
    /// Complete save data structure.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public SaveMetadata Metadata;
        public Dictionary<string, object> ComponentData;
    }

    /// <summary>
    /// Configuration for Save Manager behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "Save Manager Config", menuName = "Project Chimera/Core/Save Manager Config")]
    public class SaveManagerConfigSO : ChimeraConfigSO
    {
        [Header("File Settings")]
        public string SaveFilePrefix = "ProjectChimera_Save_";
        public string SaveFileExtension = ".json";
        public bool CompressSaveFiles = true;
        public bool EncryptSaveFiles = false;

        [Header("Auto-Save Settings")]
        public bool EnableAutoSave = true;
        
        [Range(30.0f, 1800.0f)]
        public float AutoSaveInterval = 300.0f;
        
        [Range(1, 20)]
        public int MaxAutoSaves = 5;

        [Header("Performance Settings")]
        public bool UseAsyncSave = true;
        public bool ValidateSaveData = true;
        
        [Range(1, 100)]
        public int MaxSaveSlots = 50;
    }
}