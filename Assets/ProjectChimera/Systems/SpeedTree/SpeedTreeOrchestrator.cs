using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.SpeedTree;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Cultivation;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8f: SpeedTreeOrchestrator - Main coordination class for SpeedTree system
    /// Orchestrates all SpeedTree services: Rendering, Growth Animation, Environmental, Wind, and Performance.
    /// Replaces the monolithic AdvancedSpeedTreeManager with a coordinated service architecture.
    /// </summary>
    public class SpeedTreeOrchestrator : ChimeraManager
    {
        [Header("SpeedTree Configuration")]
        [SerializeField] private SpeedTreeLibrarySO _speedTreeLibrary;
        [SerializeField] private SpeedTreeShaderConfigSO _shaderConfig;
        [SerializeField] private SpeedTreeLODConfigSO _lodConfig;
        [SerializeField] private SpeedTreeGeneticsConfigSO _geneticsConfig;
        [SerializeField] private SpeedTreeGrowthConfigSO _growthConfig;
        
        [Header("System Settings")]
        [SerializeField] private bool _enableSpeedTreeSystem = true;
        [SerializeField] private bool _enableGrowthAnimation = true;
        [SerializeField] private bool _enableEnvironmentalEffects = true;
        [SerializeField] private bool _enableWindSimulation = true;
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxVisiblePlants = 1000;
        [SerializeField] private float _systemUpdateFrequency = 0.1f;
        
        [Header("Performance Targets")]
        [SerializeField] private float _targetFPS = 60f;
        [SerializeField] private SpeedTreeQualityLevel _defaultQuality = SpeedTreeQualityLevel.High;
        
        // Service references
        private SpeedTreeRenderingService _renderingService;
        private SpeedTreeGrowthAnimationService _growthAnimationService;
        private SpeedTreeEnvironmentalService _environmentalService;
        private SpeedTreeWindService _windService;
        private SpeedTreePerformanceService _performanceService;
        
        // Plant management
        private Dictionary<int, SpeedTreePlantData> _activePlants;
        private List<CannabisStrainAsset> _cannabisStrains;
        
        // System state
        private bool _systemInitialized;
        private SpeedTreeSystemStats _systemStats;
        
        // Events for system coordination
        public System.Action<SpeedTreePlantData> OnPlantCreated;
        public System.Action<int> OnPlantDestroyed;
        public System.Action<SpeedTreeSystemStats> OnSystemStatsUpdated;
        public System.Action<bool> OnSystemEnabledChanged;
        
        protected override void OnManagerInitialize()
        {
            InitializeSpeedTreeSystem();
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_systemInitialized || !_enableSpeedTreeSystem) return;
            
            UpdateSystemStats();
            
            // Services handle their own updates via coroutines
            // This is just for high-level coordination
        }
        
        protected override void OnManagerShutdown()
        {
            CleanupSpeedTreeSystem();
        }
        
        /// <summary>
        /// Creates a new SpeedTree plant instance with full service integration.
        /// </summary>
        public SpeedTreePlantData CreatePlantInstance(InteractivePlantComponent plantComponent, string strainId = "")
        {
            if (!_systemInitialized || !_enableSpeedTreeSystem)
            {
                LogWarning("SpeedTree system not initialized or disabled");
                return null;
            }
            
            if (plantComponent == null)
            {
                LogError("Cannot create plant instance: plantComponent is null");
                return null;
            }
            
            try
            {
                // Create plant instance through rendering service
                var plantInstance = _renderingService?.CreatePlantInstance(plantComponent, strainId);
                if (plantInstance == null)
                {
                    LogError($"Failed to create plant instance for {plantComponent.name}");
                    return null;
                }
                
                // Register with all services
                RegisterPlantWithServices(plantInstance);
                
                // Track the plant
                _activePlants[plantInstance.InstanceId] = plantInstance;
                
                OnPlantCreated?.Invoke(plantInstance);
                
                LogInfo($"Created SpeedTree plant instance: {plantInstance.InstanceId}");
                return plantInstance;
            }
            catch (System.Exception ex)
            {
                LogError($"Exception creating plant instance: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Destroys a SpeedTree plant instance and cleans up all service registrations.
        /// </summary>
        public bool DestroyPlantInstance(int instanceId)
        {
            if (!_activePlants.TryGetValue(instanceId, out var plantInstance))
            {
                LogWarning($"Plant instance not found: {instanceId}");
                return false;
            }
            
            try
            {
                // Unregister from all services
                UnregisterPlantFromServices(instanceId);
                
                // Destroy through rendering service
                var success = _renderingService?.DestroyPlantInstance(instanceId) ?? false;
                
                // Remove from tracking
                _activePlants.Remove(instanceId);
                
                OnPlantDestroyed?.Invoke(instanceId);
                
                LogInfo($"Destroyed SpeedTree plant instance: {instanceId}");
                return success;
            }
            catch (System.Exception ex)
            {
                LogError($"Exception destroying plant instance {instanceId}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a plant instance by ID.
        /// </summary>
        public SpeedTreePlantData GetPlantInstance(int instanceId)
        {
            return _activePlants.TryGetValue(instanceId, out var plant) ? plant : null;
        }
        
        /// <summary>
        /// Gets all active plant instances.
        /// </summary>
        public IReadOnlyDictionary<int, SpeedTreePlantData> GetAllPlantInstances()
        {
            return _activePlants;
        }
        
        /// <summary>
        /// Updates environmental conditions for all plants.
        /// </summary>
        public void UpdateEnvironmentalConditions(ProjectChimera.Data.Environment.EnvironmentalConditions newConditions)
        {
            _environmentalService?.UpdateEnvironmentalConditions(newConditions);
            LogInfo("Updated environmental conditions for SpeedTree system");
        }
        
        /// <summary>
        /// Updates wind settings for all plants.
        /// </summary>
        public void UpdateWindSettings(SpeedTreeWindSettings newWindSettings)
        {
            _windService?.UpdateWindSettings(newWindSettings);
            LogInfo($"Updated wind settings: Strength={newWindSettings.WindStrength}");
        }
        
        /// <summary>
        /// Updates performance settings for optimization.
        /// </summary>
        public void UpdatePerformanceSettings(SpeedTreePerformanceMetrics newMetrics)
        {
            _performanceService?.UpdatePerformanceSettings(newMetrics);
            LogInfo($"Updated performance settings: Quality={newMetrics.QualityLevel}");
        }
        
        /// <summary>
        /// Forces an immediate system update cycle.
        /// </summary>
        public void ForceSystemUpdate()
        {
            _performanceService?.ForcePerformanceUpdate();
            UpdateSystemStats();
            LogInfo("Forced SpeedTree system update");
        }
        
        /// <summary>
        /// Gets comprehensive system statistics.
        /// </summary>
        public SpeedTreeSystemStats GetSystemStats()
        {
            UpdateSystemStats();
            return _systemStats;
        }
        
        /// <summary>
        /// Enables or disables the entire SpeedTree system.
        /// </summary>
        public void SetSystemEnabled(bool enabled)
        {
            _enableSpeedTreeSystem = enabled;
            
            if (enabled && !_systemInitialized)
            {
                InitializeSpeedTreeSystem();
            }
            else if (!enabled && _systemInitialized)
            {
                DisableAllServices();
            }
            
            OnSystemEnabledChanged?.Invoke(enabled);
            LogInfo($"SpeedTree system {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Gets the rendering service for direct access.
        /// </summary>
        public SpeedTreeRenderingService GetRenderingService() => _renderingService;
        
        /// <summary>
        /// Gets the growth animation service for direct access.
        /// </summary>
        public SpeedTreeGrowthAnimationService GetGrowthAnimationService() => _growthAnimationService;
        
        /// <summary>
        /// Gets the environmental service for direct access.
        /// </summary>
        public SpeedTreeEnvironmentalService GetEnvironmentalService() => _environmentalService;
        
        /// <summary>
        /// Gets the wind service for direct access.
        /// </summary>
        public SpeedTreeWindService GetWindService() => _windService;
        
        /// <summary>
        /// Gets the performance service for direct access.
        /// </summary>
        public SpeedTreePerformanceService GetPerformanceService() => _performanceService;
        
        // Private methods
        private void InitializeSpeedTreeSystem()
        {
            if (_systemInitialized)
            {
                LogWarning("SpeedTree system already initialized");
                return;
            }
            
            LogInfo("Initializing SpeedTree system...");
            
            try
            {
                // Initialize data structures
                _activePlants = new Dictionary<int, SpeedTreePlantData>();
                _cannabisStrains = LoadCannabisStrains();
                _systemStats = new SpeedTreeSystemStats();
                
                // Validate configuration
                if (!ValidateConfiguration())
                {
                    LogError("SpeedTree configuration validation failed");
                    return;
                }
                
                // Initialize services
                InitializeServices();
                
                // Setup service event handlers
                SetupServiceEventHandlers();
                
                _systemInitialized = true;
                LogInfo("SpeedTree system initialized successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to initialize SpeedTree system: {ex.Message}");
                _systemInitialized = false;
            }
        }
        
        private bool ValidateConfiguration()
        {
            var isValid = true;
            
            if (_speedTreeLibrary == null)
            {
                LogError("SpeedTree library configuration is missing");
                isValid = false;
            }
            
            if (_shaderConfig == null)
            {
                LogWarning("SpeedTree shader configuration is missing - using defaults");
            }
            
            if (_lodConfig == null)
            {
                LogWarning("SpeedTree LOD configuration is missing - using defaults");
            }
            
            if (_geneticsConfig == null)
            {
                LogWarning("SpeedTree genetics configuration is missing - using defaults");
            }
            
            if (_growthConfig == null)
            {
                LogWarning("SpeedTree growth configuration is missing - using defaults");
            }
            
            return isValid;
        }
        
        private void InitializeServices()
        {
            var mainCamera = Camera.main;
            
            // Initialize rendering service
            _renderingService = new SpeedTreeRenderingService(
                _speedTreeLibrary, _shaderConfig, _lodConfig, _geneticsConfig, _cannabisStrains);
            
            // Initialize growth animation service
            if (_enableGrowthAnimation)
            {
                _growthAnimationService = new SpeedTreeGrowthAnimationService(
                    _growthConfig, this, _enableGrowthAnimation, 1f);
            }
            
            // Initialize environmental service
            if (_enableEnvironmentalEffects)
            {
                _environmentalService = new SpeedTreeEnvironmentalService(
                    this, _enableEnvironmentalEffects, true, true, _systemUpdateFrequency);
            }
            
            // Initialize wind service
            if (_enableWindSimulation)
            {
                var defaultWindSettings = new SpeedTreeWindSettings();
                _windService = new SpeedTreeWindService(
                    defaultWindSettings, this, _enableWindSimulation, _systemUpdateFrequency);
            }
            
            // Initialize performance service
            if (_enablePerformanceOptimization)
            {
                _performanceService = new SpeedTreePerformanceService(
                    _lodConfig, mainCamera, this, _maxVisiblePlants, 
                    _enablePerformanceOptimization, _systemUpdateFrequency);
                    
                _performanceService.SetTargetFPS(_targetFPS);
            }
            
            LogInfo("All SpeedTree services initialized");
        }
        
        private void SetupServiceEventHandlers()
        {
            // Rendering service events
            if (_renderingService != null)
            {
                _renderingService.OnPlantInstanceCreated += HandlePlantInstanceCreated;
                _renderingService.OnPlantInstanceDestroyed += HandlePlantInstanceDestroyed;
            }
            
            // Growth animation service events
            if (_growthAnimationService != null)
            {
                _growthAnimationService.OnGrowthStageChanged += HandleGrowthStageChanged;
                _growthAnimationService.OnGrowthProgressUpdated += HandleGrowthProgressUpdated;
            }
            
            // Performance service events
            if (_performanceService != null)
            {
                _performanceService.OnFPSChanged += HandleFPSChanged;
                _performanceService.OnQualityLevelChanged += HandleQualityLevelChanged;
            }
        }
        
        private void RegisterPlantWithServices(SpeedTreePlantData plantInstance)
        {
            // Register with growth animation service
            _growthAnimationService?.StartGrowthAnimation(plantInstance);
            
            // Register with environmental service
            _environmentalService?.RegisterPlant(plantInstance);
            
            // Register with wind service
            _windService?.RegisterPlantForWind(plantInstance);
            
            // Register with performance service
            _performanceService?.RegisterPlant(plantInstance);
        }
        
        private void UnregisterPlantFromServices(int instanceId)
        {
            // Unregister from all services
            _growthAnimationService?.StopGrowthAnimation(instanceId);
            _environmentalService?.UnregisterPlant(instanceId);
            _windService?.UnregisterPlantFromWind(instanceId);
            _performanceService?.UnregisterPlant(instanceId);
        }
        
        private void UpdateSystemStats()
        {
            if (_systemStats == null) return;
            
            _systemStats.TotalPlants = _activePlants.Count;
            _systemStats.RenderingStats = _renderingService?.GetRenderingStats();
            _systemStats.PerformanceStats = _performanceService?.GetPerformanceStats();
            _systemStats.WindStats = _windService?.GetWindStats();
            _systemStats.SystemEnabled = _enableSpeedTreeSystem;
            _systemStats.LastUpdateTime = Time.time;
            
            OnSystemStatsUpdated?.Invoke(_systemStats);
        }
        
        private void DisableAllServices()
        {
            _growthAnimationService?.Cleanup();
            _environmentalService?.Cleanup();
            _windService?.Cleanup();
            _performanceService?.Cleanup();
        }
        
        private void CleanupSpeedTreeSystem()
        {
            LogInfo("Cleaning up SpeedTree system...");
            
            // Cleanup all services
            _renderingService = null;
            _growthAnimationService?.Cleanup();
            _growthAnimationService = null;
            _environmentalService?.Cleanup();
            _environmentalService = null;
            _windService?.Cleanup();
            _windService = null;
            _performanceService?.Cleanup();
            _performanceService = null;
            
            // Clear plant data
            _activePlants?.Clear();
            _cannabisStrains?.Clear();
            
            _systemInitialized = false;
            LogInfo("SpeedTree system cleanup completed");
        }
        
        private List<CannabisStrainAsset> LoadCannabisStrains()
        {
            // Load cannabis strain assets from resources or configuration
            var strains = new List<CannabisStrainAsset>();
            
            // Add default strains if none configured
            if (strains.Count == 0)
            {
                strains.Add(new CannabisStrainAsset
                {
                    StrainId = "default_indica",
                    StrainName = "Default Indica",
                    DefaultGenetics = new CannabisGeneticData { StrainId = "default_indica" }
                });
            }
            
            return strains;
        }
        
        // Event handlers
        private void HandlePlantInstanceCreated(SpeedTreePlantData plantData)
        {
            LogInfo($"Plant instance created: {plantData.InstanceId}");
        }
        
        private void HandlePlantInstanceDestroyed(int instanceId)
        {
            LogInfo($"Plant instance destroyed: {instanceId}");
        }
        
        private void HandleGrowthStageChanged(int instanceId, PlantGrowthStage newStage)
        {
            LogInfo($"Plant {instanceId} growth stage changed to: {newStage}");
        }
        
        private void HandleGrowthProgressUpdated(int instanceId, float progress)
        {
            // Update can be frequent, only log significant changes
            if (progress % 0.25f < 0.01f) // Log at 25% intervals
            {
                LogInfo($"Plant {instanceId} growth progress: {progress:P0}");
            }
        }
        
        private void HandleFPSChanged(float newFPS)
        {
            if (newFPS < _targetFPS * 0.8f)
            {
                LogWarning($"Performance warning: FPS dropped to {newFPS:F1} (target: {_targetFPS})");
            }
        }
        
        private void HandleQualityLevelChanged(SpeedTreeQualityLevel newQuality)
        {
            LogInfo($"SpeedTree quality level changed to: {newQuality.QualityName}");
        }
    }
    
    /// <summary>
    /// Comprehensive system statistics for SpeedTree orchestrator.
    /// </summary>
    [System.Serializable]
    public class SpeedTreeSystemStats
    {
        public int TotalPlants;
        public bool SystemEnabled;
        public float LastUpdateTime;
        public SpeedTreeRenderingStats RenderingStats;
        public SpeedTreePerformanceStats PerformanceStats;
        public WindSimulationStats WindStats;
    }
}