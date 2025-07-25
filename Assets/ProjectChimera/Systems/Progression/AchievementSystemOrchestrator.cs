using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;
using ProgressionAchievementProgress = ProjectChimera.Data.Progression.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement System Orchestrator - Central coordination and manager interface
    /// Coordinates all achievement services and provides unified interface for the achievement system
    /// Manages service lifecycle, inter-service communication, and external API exposure
    /// Replaces the original monolithic AchievementSystemManager with a clean orchestration pattern
    /// </summary>
    public class AchievementSystemOrchestrator : MonoBehaviour
    {
        [Header("Service Configuration")]
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private bool _enableAllServices = true;
        [SerializeField] private bool _enableServiceLogging = false;
        [SerializeField] private float _serviceHealthCheckInterval = 30f;

        [Header("Service References")]
        [SerializeField] private AchievementTrackingService _trackingService;
        [SerializeField] private AchievementCelebrationService _celebrationService;
        [SerializeField] private PlayerRecognitionService _recognitionService;
        [SerializeField] private AchievementDataService _dataService;
        [SerializeField] private AchievementEventService _eventService;

        [Header("Orchestration Settings")]
        [SerializeField] private bool _enableServiceSync = true;
        [SerializeField] private bool _enableFailover = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private int _maxRetryAttempts = 3;

        // Service state management
        private bool _isInitialized = false;
        private bool _isShuttingDown = false;
        private float _lastHealthCheck = 0f;
        private Dictionary<string, bool> _serviceHealth = new Dictionary<string, bool>();
        private Dictionary<string, DateTime> _serviceLastActivity = new Dictionary<string, DateTime>();
        private List<string> _initializationOrder = new List<string>();

        // Service communication
        private Dictionary<string, List<Action<object>>> _serviceCallbacks = new Dictionary<string, List<Action<object>>>();
        private Queue<ServiceCommand> _commandQueue = new Queue<ServiceCommand>();
        private AchievementSystemMetrics _systemMetrics = new AchievementSystemMetrics();

        // Events for system coordination
        public static event Action<string> OnServiceInitialized;
        public static event Action<string> OnServiceShutdown;
        public static event Action<Achievement> OnAchievementUnlocked;
        public static event Action<string, string, float> OnProgressUpdate;
        public static event Action<AchievementSystemMetrics> OnSystemMetricsUpdate;

        #region Orchestrator Interface

        public bool IsInitialized => _isInitialized;
        public string SystemName => "Achievement System Orchestrator";
        public int ActiveServices => _serviceHealth.Count(kvp => kvp.Value);
        public int TotalServices => 5; // Number of achievement services
        public AchievementSystemMetrics SystemMetrics => _systemMetrics;

        public void Initialize()
        {
            InitializeOrchestrator();
        }

        public void Shutdown()
        {
            ShutdownOrchestrator();
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeServiceReferences();
            SetupServiceCallbacks();
        }

        private void Start()
        {
            if (_initializeOnStart)
            {
                InitializeOrchestrator();
            }
        }

        private void Update()
        {
            if (_isInitialized && !_isShuttingDown)
            {
                ProcessCommandQueue();
                PerformHealthChecks();
                UpdateSystemMetrics();
            }
        }

        private void OnDestroy()
        {
            ShutdownOrchestrator();
        }

        #endregion

        #region Orchestrator Lifecycle

        private void InitializeServiceReferences()
        {
            // Find services if not assigned in inspector
            if (_trackingService == null)
                _trackingService = FindObjectOfType<AchievementTrackingService>();
            if (_celebrationService == null)
                _celebrationService = FindObjectOfType<AchievementCelebrationService>();
            if (_recognitionService == null)
                _recognitionService = FindObjectOfType<PlayerRecognitionService>();
            if (_dataService == null)
                _dataService = FindObjectOfType<AchievementDataService>();
            if (_eventService == null)
                _eventService = FindObjectOfType<AchievementEventService>();

            // Log service discovery
            if (_enableServiceLogging)
            {
                ChimeraLogger.Log($"Service references initialized: Tracking={_trackingService != null}, Celebration={_celebrationService != null}, Recognition={_recognitionService != null}, Data={_dataService != null}, Event={_eventService != null}", this);
            }
        }

        private void SetupServiceCallbacks()
        {
            // Initialize service callback dictionaries
            _serviceCallbacks["tracking"] = new List<Action<object>>();
            _serviceCallbacks["celebration"] = new List<Action<object>>();
            _serviceCallbacks["recognition"] = new List<Action<object>>();
            _serviceCallbacks["data"] = new List<Action<object>>();
            _serviceCallbacks["event"] = new List<Action<object>>();

            // Set up initialization order
            _initializationOrder = new List<string> { "data", "tracking", "event", "recognition", "celebration" };
        }

        public void InitializeOrchestrator()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementSystemOrchestrator already initialized", this);
                return;
            }

            try
            {
                ChimeraLogger.Log("Starting Achievement System Orchestrator initialization", this);
                
                InitializeServicesInOrder();
                SetupServiceIntegration();
                StartSystemMonitoring();
                
                _isInitialized = true;
                ChimeraLogger.Log("Achievement System Orchestrator initialized successfully", this);
                
                OnServiceInitialized?.Invoke("orchestrator");
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize Achievement System Orchestrator: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownOrchestrator()
        {
            if (!_isInitialized || _isShuttingDown) return;

            try
            {
                _isShuttingDown = true;
                ChimeraLogger.Log("Starting Achievement System Orchestrator shutdown", this);
                
                SaveSystemState();
                ShutdownServicesInOrder();
                CleanupResources();
                
                _isInitialized = false;
                _isShuttingDown = false;
                
                ChimeraLogger.Log("Achievement System Orchestrator shutdown completed", this);
                OnServiceShutdown?.Invoke("orchestrator");
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error during Achievement System Orchestrator shutdown: {ex.Message}", this);
            }
        }

        #endregion

        #region Service Management

        private void InitializeServicesInOrder()
        {
            foreach (string serviceName in _initializationOrder)
            {
                if (!_enableAllServices)
                {
                    continue;
                }

                try
                {
                    bool success = InitializeService(serviceName);
                    _serviceHealth[serviceName] = success;
                    _serviceLastActivity[serviceName] = DateTime.Now;
                    
                    if (success)
                    {
                        OnServiceInitialized?.Invoke(serviceName);
                        if (_enableServiceLogging)
                        {
                            ChimeraLogger.Log($"Service '{serviceName}' initialized successfully", this);
                        }
                    }
                    else
                    {
                        ChimeraLogger.LogWarning($"Service '{serviceName}' failed to initialize", this);
                    }
                }
                catch (System.Exception ex)
                {
                    ChimeraLogger.LogError($"Exception initializing service '{serviceName}': {ex.Message}", this);
                    _serviceHealth[serviceName] = false;
                }
            }
        }

        private bool InitializeService(string serviceName)
        {
            switch (serviceName)
            {
                case "data":
                    if (_dataService != null)
                    {
                        _dataService.Initialize();
                        return _dataService.IsInitialized;
                    }
                    break;
                    
                case "tracking":
                    if (_trackingService != null)
                    {
                        _trackingService.Initialize();
                        return _trackingService.IsInitialized;
                    }
                    break;
                    
                case "event":
                    if (_eventService != null)
                    {
                        _eventService.Initialize();
                        return _eventService.IsInitialized;
                    }
                    break;
                    
                case "recognition":
                    if (_recognitionService != null)
                    {
                        _recognitionService.Initialize();
                        return _recognitionService.IsInitialized;
                    }
                    break;
                    
                case "celebration":
                    if (_celebrationService != null)
                    {
                        _celebrationService.Initialize();
                        return _celebrationService.IsInitialized;
                    }
                    break;
            }
            
            return false;
        }

        private void ShutdownServicesInOrder()
        {
            // Shutdown in reverse order
            var shutdownOrder = _initializationOrder.AsEnumerable().Reverse().ToList();
            
            foreach (string serviceName in shutdownOrder)
            {
                try
                {
                    ShutdownService(serviceName);
                    OnServiceShutdown?.Invoke(serviceName);
                    
                    if (_enableServiceLogging)
                    {
                        ChimeraLogger.Log($"Service '{serviceName}' shutdown completed", this);
                    }
                }
                catch (System.Exception ex)
                {
                    ChimeraLogger.LogError($"Exception shutting down service '{serviceName}': {ex.Message}", this);
                }
            }
        }

        private void ShutdownService(string serviceName)
        {
            switch (serviceName)
            {
                case "celebration":
                    _celebrationService?.Shutdown();
                    break;
                case "recognition":
                    _recognitionService?.Shutdown();
                    break;
                case "event":
                    _eventService?.Shutdown();
                    break;
                case "tracking":
                    _trackingService?.Shutdown();
                    break;
                case "data":
                    _dataService?.Shutdown();
                    break;
            }
            
            _serviceHealth[serviceName] = false;
        }

        #endregion

        #region Service Integration

        private void SetupServiceIntegration()
        {
            // Connect tracking service events to other services
            if (_trackingService != null && _eventService != null)
            {
                // Event service feeds progress updates to tracking service
                AchievementEventService.OnProgressUpdated += OnEventServiceProgressUpdate;
            }

            if (_trackingService != null && _celebrationService != null)
            {
                // Tracking service achievement completions trigger celebrations
                AchievementTrackingService.OnAchievementCompleted += OnAchievementCompleted;
            }

            if (_trackingService != null && _recognitionService != null)
            {
                // Achievement completions update player recognition
                AchievementTrackingService.OnAchievementCompleted += OnAchievementForRecognition;
            }

            if (_dataService != null)
            {
                // All services can trigger data saves
                SetupDataServiceIntegration();
            }

            ChimeraLogger.Log("Service integration setup completed", this);
        }

        private void SetupDataServiceIntegration()
        {
            // Connect data events from all services
            if (_trackingService != null)
            {
                AchievementTrackingService.OnAchievementCompleted += OnDataUpdateRequired;
            }
            
            if (_recognitionService != null)
            {
                PlayerRecognitionService.OnPlayerRecognition += OnPlayerDataUpdateRequired;
            }
        }

        #endregion

        #region Event Handlers

        private void OnEventServiceProgressUpdate(string playerId, string eventType, float progress)
        {
            if (_trackingService != null)
            {
                _trackingService.UpdateAchievementProgress(eventType, progress, playerId);
            }
            
            OnProgressUpdate?.Invoke(playerId, eventType, progress);
            _serviceLastActivity["event"] = DateTime.Now;
        }

        private void OnAchievementCompleted(Achievement achievement)
        {
            // Trigger celebration
            if (_celebrationService != null)
            {
                _celebrationService.QueueCelebration(achievement);
            }
            
            OnAchievementUnlocked?.Invoke(achievement);
            _systemMetrics.TotalAchievementsUnlocked++;
            _serviceLastActivity["tracking"] = DateTime.Now;
        }

        private void OnAchievementForRecognition(Achievement achievement)
        {
            // Update player recognition
            if (_recognitionService != null)
            {
                string playerId = "current_player"; // Would get from achievement context
                _recognitionService.UpdatePlayerProfile(playerId, achievement);
                _recognitionService.CheckForBadgeUnlocks(playerId);
                _recognitionService.CheckForTierAdvancement(playerId);
            }
            
            _serviceLastActivity["recognition"] = DateTime.Now;
        }

        private void OnDataUpdateRequired(Achievement achievement)
        {
            if (_dataService != null)
            {
                // Mark data as dirty to trigger save
                _dataService.ForceDataSave();
            }
            
            _serviceLastActivity["data"] = DateTime.Now;
        }

        private void OnPlayerDataUpdateRequired(PlayerAchievementProfile profile)
        {
            if (_dataService != null && profile != null)
            {
                var playerData = _dataService.GetPlayerData(profile.PlayerID);
                if (playerData != null)
                {
                    playerData.TotalPoints = profile.TotalPoints;
                    playerData.CompletionPercentage = profile.CompletionPercentage;
                    _dataService.SavePlayerData(profile.PlayerID, playerData);
                }
            }
            
            _serviceLastActivity["data"] = DateTime.Now;
        }

        #endregion

        #region Command Processing

        private void ProcessCommandQueue()
        {
            int processedCommands = 0;
            int maxCommandsPerFrame = 10;

            while (_commandQueue.Count > 0 && processedCommands < maxCommandsPerFrame)
            {
                var command = _commandQueue.Dequeue();
                ProcessServiceCommand(command);
                processedCommands++;
            }
        }

        private void ProcessServiceCommand(ServiceCommand command)
        {
            try
            {
                switch (command.Type)
                {
                    case "achievement_progress":
                        HandleAchievementProgressCommand(command);
                        break;
                    case "celebration_trigger":
                        HandleCelebrationCommand(command);
                        break;
                    case "data_save":
                        HandleDataSaveCommand(command);
                        break;
                    case "system_health_check":
                        HandleHealthCheckCommand(command);
                        break;
                    default:
                        if (_enableServiceLogging)
                        {
                            ChimeraLogger.LogWarning($"Unknown command type: {command.Type}", this);
                        }
                        break;
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error processing command {command.Type}: {ex.Message}", this);
            }
        }

        private void HandleAchievementProgressCommand(ServiceCommand command)
        {
            if (command.Data.TryGetValue("eventType", out var eventType) &&
                command.Data.TryGetValue("playerId", out var playerId) &&
                command.Data.TryGetValue("value", out var value))
            {
                _eventService?.ProcessGameEvent(eventType.ToString(), playerId.ToString(), Convert.ToSingle(value));
            }
        }

        private void HandleCelebrationCommand(ServiceCommand command)
        {
            if (command.Data.TryGetValue("achievement", out var achievement) && achievement is Achievement ach)
            {
                _celebrationService?.QueueCelebration(ach);
            }
        }

        private void HandleDataSaveCommand(ServiceCommand command)
        {
            _dataService?.ForceDataSave();
        }

        private void HandleHealthCheckCommand(ServiceCommand command)
        {
            PerformImmediateHealthCheck();
        }

        #endregion

        #region Health Monitoring

        private void PerformHealthChecks()
        {
            if (Time.time - _lastHealthCheck < _serviceHealthCheckInterval)
            {
                return;
            }

            foreach (string serviceName in _initializationOrder)
            {
                bool currentHealth = CheckServiceHealth(serviceName);
                bool previousHealth = _serviceHealth.GetValueOrDefault(serviceName, false);
                
                _serviceHealth[serviceName] = currentHealth;
                
                if (currentHealth != previousHealth)
                {
                    if (currentHealth)
                    {
                        ChimeraLogger.Log($"Service '{serviceName}' health restored", this);
                    }
                    else
                    {
                        ChimeraLogger.LogWarning($"Service '{serviceName}' health degraded", this);
                        
                        if (_enableFailover)
                        {
                            AttemptServiceRecovery(serviceName);
                        }
                    }
                }
            }

            _lastHealthCheck = Time.time;
        }

        private bool CheckServiceHealth(string serviceName)
        {
            switch (serviceName)
            {
                case "data":
                    return _dataService != null && _dataService.IsInitialized;
                case "tracking":
                    return _trackingService != null && _trackingService.IsInitialized;
                case "event":
                    return _eventService != null && _eventService.IsInitialized;
                case "recognition":
                    return _recognitionService != null && _recognitionService.IsInitialized;
                case "celebration":
                    return _celebrationService != null && _celebrationService.IsInitialized;
                default:
                    return false;
            }
        }

        private void PerformImmediateHealthCheck()
        {
            foreach (string serviceName in _initializationOrder)
            {
                _serviceHealth[serviceName] = CheckServiceHealth(serviceName);
            }
        }

        private void AttemptServiceRecovery(string serviceName)
        {
            ChimeraLogger.Log($"Attempting recovery for service: {serviceName}", this);
            
            try
            {
                // Attempt to reinitialize the service
                bool recovered = InitializeService(serviceName);
                
                if (recovered)
                {
                    ChimeraLogger.Log($"Service '{serviceName}' recovery successful", this);
                    _serviceHealth[serviceName] = true;
                    _serviceLastActivity[serviceName] = DateTime.Now;
                }
                else
                {
                    ChimeraLogger.LogError($"Service '{serviceName}' recovery failed", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Exception during service recovery for '{serviceName}': {ex.Message}", this);
            }
        }

        #endregion

        #region System Metrics

        private void UpdateSystemMetrics()
        {
            _systemMetrics.LastUpdate = DateTime.Now;
            _systemMetrics.ActiveServices = ActiveServices;
            _systemMetrics.ServiceHealth = new Dictionary<string, bool>(_serviceHealth);
            
            // Update service-specific metrics
            if (_trackingService != null)
            {
                _systemMetrics.TotalAchievements = _trackingService.AllAchievements.Count;
                _systemMetrics.UnlockedAchievements = _trackingService.UnlockedAchievements.Count;
            }
            
            if (_eventService != null)
            {
                _systemMetrics.QueuedEvents = _eventService.QueuedEvents;
                _systemMetrics.TrackedPlayers = _eventService.TrackedPlayers;
            }
            
            if (_celebrationService != null)
            {
                _systemMetrics.PendingCelebrations = _celebrationService.PendingCelebrations;
            }
            
            OnSystemMetricsUpdate?.Invoke(_systemMetrics);
        }

        private void StartSystemMonitoring()
        {
            _systemMetrics = new AchievementSystemMetrics
            {
                InitializationTime = DateTime.Now,
                LastUpdate = DateTime.Now,
                ServiceHealth = new Dictionary<string, bool>(_serviceHealth)
            };
        }

        #endregion

        #region Utility Methods

        private void SaveSystemState()
        {
            // Force save all data before shutdown
            _dataService?.ForceDataSave();
            
            if (_enableServiceLogging)
            {
                ChimeraLogger.Log("System state saved before shutdown", this);
            }
        }

        private void CleanupResources()
        {
            _serviceCallbacks.Clear();
            _commandQueue.Clear();
            _serviceHealth.Clear();
            _serviceLastActivity.Clear();
            
            // Unsubscribe from events
            AchievementEventService.OnProgressUpdated -= OnEventServiceProgressUpdate;
            AchievementTrackingService.OnAchievementCompleted -= OnAchievementCompleted;
            AchievementTrackingService.OnAchievementCompleted -= OnAchievementForRecognition;
            AchievementTrackingService.OnAchievementCompleted -= OnDataUpdateRequired;
            PlayerRecognitionService.OnPlayerRecognition -= OnPlayerDataUpdateRequired;
        }

        #endregion

        #region Public API

        public void TriggerAchievementProgress(string eventType, string playerId = "current_player", float value = 1f)
        {
            if (!_isInitialized)
            {
                return;
            }

            var command = new ServiceCommand
            {
                Type = "achievement_progress",
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    ["eventType"] = eventType,
                    ["playerId"] = playerId,
                    ["value"] = value
                }
            };
            
            _commandQueue.Enqueue(command);
        }

        public void TriggerCelebration(Achievement achievement)
        {
            if (!_isInitialized || achievement == null)
            {
                return;
            }

            var command = new ServiceCommand
            {
                Type = "celebration_trigger",
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    ["achievement"] = achievement
                }
            };
            
            _commandQueue.Enqueue(command);
        }

        public void ForceSaveData()
        {
            if (!_isInitialized)
            {
                return;
            }

            var command = new ServiceCommand
            {
                Type = "data_save",
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>()
            };
            
            _commandQueue.Enqueue(command);
        }

        public List<Achievement> GetAllAchievements()
        {
            return _trackingService?.AllAchievements.ToList() ?? new List<Achievement>();
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            return _trackingService?.UnlockedAchievements.ToList() ?? new List<Achievement>();
        }

        public PlayerAchievementProfile GetPlayerProfile(string playerId)
        {
            return _recognitionService?.GetOrCreatePlayerProfile(playerId);
        }

        public Dictionary<string, float> GetPlayerProgress(string playerId)
        {
            return _eventService?.GetPlayerProgress(playerId) ?? new Dictionary<string, float>();
        }

        public bool IsServiceHealthy(string serviceName)
        {
            return _serviceHealth.GetValueOrDefault(serviceName, false);
        }

        public void UpdateSystemSettings(bool enableAllServices, bool enableLogging, float healthCheckInterval)
        {
            _enableAllServices = enableAllServices;
            _enableServiceLogging = enableLogging;
            _serviceHealthCheckInterval = healthCheckInterval;
            
            ChimeraLogger.Log($"System settings updated: allServices={enableAllServices}, logging={enableLogging}, healthInterval={healthCheckInterval}", this);
        }

        #endregion
    }

    /// <summary>
    /// Service command for orchestrator command queue
    /// </summary>
    [System.Serializable]
    public class ServiceCommand
    {
        public string Type = "";
        public DateTime Timestamp = DateTime.Now;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    /// <summary>
    /// Achievement system metrics for monitoring
    /// </summary>
    [System.Serializable]
    public class AchievementSystemMetrics
    {
        public DateTime InitializationTime = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
        public int ActiveServices = 0;
        public Dictionary<string, bool> ServiceHealth = new Dictionary<string, bool>();
        public int TotalAchievements = 0;
        public int UnlockedAchievements = 0;
        public int TotalAchievementsUnlocked = 0;
        public int QueuedEvents = 0;
        public int TrackedPlayers = 0;
        public int PendingCelebrations = 0;
    }
}