using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Genetics;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using GeneticPerformanceStats = ProjectChimera.Systems.Cultivation.GeneticPerformanceStats;
using SystemsHarvestResults = ProjectChimera.Systems.Cultivation.SystemsHarvestResults;
using HarvestResults = ProjectChimera.Systems.Cultivation.HarvestResults;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-3: Refactored Plant Manager - Orchestrates specialized plant management services
    /// Replaces monolithic 1,207-line PlantManager with composition-based architecture
    /// Adheres to Single Responsibility Principle and Dependency Injection patterns
    /// </summary>
    public class RefactoredPlantManager : ChimeraManager
    {
        [Header("Plant Manager Configuration")]
        [SerializeField] private bool _enablePlantManagement = true;
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Service Configuration")]
        [SerializeField] private bool _enableLifecycleService = true;
        [SerializeField] private bool _enableProcessingService = true;
        [SerializeField] private bool _enableAchievementService = true;
        [SerializeField] private bool _enableGeneticsService = true;
        [SerializeField] private bool _enableHarvestService = true;
        [SerializeField] private bool _enableStatisticsService = true;
        [SerializeField] private bool _enableEnvironmentalService = true;
        
        // Specialized services
        private IPlantLifecycleService _lifecycleService;
        private IPlantProcessingService _processingService;
        private IPlantAchievementService _achievementService;
        private IPlantGeneticsService _geneticsService;
        private IPlantHarvestService _harvestService;
        private IPlantStatisticsService _statisticsService;
        private IPlantEnvironmentalService _environmentalService;
        
        // Dependencies
        private CultivationManager _cultivationManager;
        
        public string ManagerName => "Refactored Plant Manager";
        
        // Delegate properties to services for backward compatibility
        public int ActivePlantCount => _statisticsService?.ActivePlantCount ?? 0;
        public float GlobalGrowthModifier
        {
            get => _processingService?.GlobalGrowthModifier ?? 1f;
            set { if (_processingService != null) _processingService.GlobalGrowthModifier = value; }
        }
        
        // Public events for system integration
        public System.Action<PlantInstance> OnPlantAdded;
        public System.Action<PlantInstance> OnPlantHarvested;
        public System.Action<PlantInstance> OnPlantStageChanged;
        public System.Action<PlantInstance> OnPlantHealthUpdated;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        protected override void OnManagerInitialize()
        {
            Debug.Log("[RefactoredPlantManager] Initializing modular plant management system...");
            
            if (!_enablePlantManagement)
            {
                Debug.Log("[RefactoredPlantManager] Plant management system disabled.");
                return;
            }
            
            // Get reference to CultivationManager
            _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            if (_cultivationManager == null)
            {
                Debug.LogError("[RefactoredPlantManager] CultivationManager not found! RefactoredPlantManager requires CultivationManager.");
                return;
            }
            
            // Initialize services with dependency injection
            InitializeServices();
            
            // Wire up inter-service communication
            WireServiceEvents();
            
            // Register with GameManager
            GameManager.Instance?.RegisterManager(this);
            
            Debug.Log("[RefactoredPlantManager] Modular plant management system initialized successfully.");
        }
        
        protected override void OnManagerShutdown()
        {
            Debug.Log("[RefactoredPlantManager] Shutting down modular plant management system...");
            
            // Unwire service events
            UnwireServiceEvents();
            
            // Shutdown services in reverse order
            ShutdownServices();
            
            Debug.Log("[RefactoredPlantManager] Modular plant management system shutdown complete.");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_enablePlantManagement) return;
            
            // Update processing service (handles plant updates)
            if (_processingService != null && _enableProcessingService)
            {
                _processingService.UpdatePlants();
            }
            
            // Update statistics service periodically
            if (_statisticsService != null && _enableStatisticsService)
            {
                // Update statistics every few seconds
                if (Time.frameCount % 180 == 0) // ~3 seconds at 60 FPS
                {
                    _statisticsService.UpdateStatistics();
                }
            }
        }
        
        /// <summary>
        /// Initializes all plant management services.
        /// </summary>
        private void InitializeServices()
        {
            // Initialize services in dependency order
            
            // 1. Lifecycle Service (foundational)
            if (_enableLifecycleService)
            {
                _lifecycleService = new PlantLifecycleService(_cultivationManager);
                _lifecycleService.Initialize();
                Debug.Log("[RefactoredPlantManager] Lifecycle service initialized");
            }
            
            // 2. Processing Service (depends on lifecycle)
            if (_enableProcessingService && _lifecycleService != null)
            {
                _processingService = new PlantProcessingService(_lifecycleService);
                _processingService.Initialize();
                Debug.Log("[RefactoredPlantManager] Processing service initialized");
            }
            
            // 3. Achievement Service (independent)
            if (_enableAchievementService)
            {
                _achievementService = new PlantAchievementService();
                _achievementService.Initialize();
                Debug.Log("[RefactoredPlantManager] Achievement service initialized");
            }
            
            // 4. Genetics Service (independent)
            if (_enableGeneticsService)
            {
                _geneticsService = new PlantGeneticsService(_cultivationManager);
                _geneticsService.Initialize();
                Debug.Log("[RefactoredPlantManager] Genetics service initialized");
            }
            
            // 5. Harvest Service (depends on lifecycle)
            if (_enableHarvestService && _lifecycleService != null)
            {
                _harvestService = new PlantHarvestService(_lifecycleService, _cultivationManager);
                _harvestService.Initialize();
                Debug.Log("[RefactoredPlantManager] Harvest service initialized");
            }
            
            // 6. Statistics Service (depends on other services)
            if (_enableStatisticsService)
            {
                _statisticsService = new PlantStatisticsService(_cultivationManager, _lifecycleService, _achievementService, _geneticsService);
                _statisticsService.Initialize();
                Debug.Log("[RefactoredPlantManager] Statistics service initialized");
            }
            
            // 7. Environmental Service (depends on lifecycle)
            if (_enableEnvironmentalService && _lifecycleService != null)
            {
                _environmentalService = new PlantEnvironmentalService(_lifecycleService, _cultivationManager);
                _environmentalService.Initialize();
                Debug.Log("[RefactoredPlantManager] Environmental service initialized");
            }
        }
        
        /// <summary>
        /// Wires up inter-service communication events.
        /// </summary>
        private void WireServiceEvents()
        {
            // Wire lifecycle service events
            if (_lifecycleService != null)
            {
                _lifecycleService.OnPlantAdded += OnPlantAddedHandler;
                _lifecycleService.OnPlantStageChanged += OnPlantStageChangedHandler;
                _lifecycleService.OnPlantHealthUpdated += OnPlantHealthUpdatedHandler;
            }
            
            // Wire harvest service events
            if (_harvestService != null)
            {
                _harvestService.OnPlantHarvested += OnPlantHarvestedHandler;
            }
            
            // Wire achievement service to track events
            if (_achievementService != null)
            {
                if (_lifecycleService != null)
                {
                    _lifecycleService.OnPlantAdded += _achievementService.TrackPlantCreation;
                    _lifecycleService.OnPlantStageChanged += _achievementService.TrackPlantGrowthStageChange;
                    _lifecycleService.OnPlantHealthUpdated += _achievementService.TrackPlantHealthChange;
                }
                
                if (_harvestService != null)
                {
                    _harvestService.OnPlantHarvested += (plant) => {
                        // Create harvest results for achievement tracking
                        var harvestResults = new SystemsHarvestResults
                        {
                            PlantID = plant.PlantID,
                            TotalYieldGrams = plant.Strain?.BaseYieldGrams ?? 50f,
                            QualityScore = plant.CurrentHealth / 100f,
                            HarvestDate = System.DateTime.Now
                        };
                        _achievementService.TrackPlantHarvest(plant, harvestResults);
                    };
                }
            }
            
            Debug.Log("[RefactoredPlantManager] Service events wired successfully");
        }
        
        /// <summary>
        /// Unwires service events.
        /// </summary>
        private void UnwireServiceEvents()
        {
            // Unwire lifecycle service events
            if (_lifecycleService != null)
            {
                _lifecycleService.OnPlantAdded -= OnPlantAddedHandler;
                _lifecycleService.OnPlantStageChanged -= OnPlantStageChangedHandler;
                _lifecycleService.OnPlantHealthUpdated -= OnPlantHealthUpdatedHandler;
            }
            
            // Unwire harvest service events
            if (_harvestService != null)
            {
                _harvestService.OnPlantHarvested -= OnPlantHarvestedHandler;
            }
            
            // Clear achievement service event handlers
            if (_achievementService != null && _lifecycleService != null)
            {
                _lifecycleService.OnPlantAdded -= _achievementService.TrackPlantCreation;
                _lifecycleService.OnPlantStageChanged -= _achievementService.TrackPlantGrowthStageChange;
                _lifecycleService.OnPlantHealthUpdated -= _achievementService.TrackPlantHealthChange;
            }
        }
        
        /// <summary>
        /// Shuts down all services.
        /// </summary>
        private void ShutdownServices()
        {
            // Shutdown in reverse order
            _environmentalService?.Shutdown();
            _statisticsService?.Shutdown();
            _harvestService?.Shutdown();
            _geneticsService?.Shutdown();
            _achievementService?.Shutdown();
            _processingService?.Shutdown();
            _lifecycleService?.Shutdown();
            
            Debug.Log("[RefactoredPlantManager] All services shutdown successfully");
        }
        
        #region Backward Compatible API - Delegates to Services
        
        /// <summary>
        /// Creates a new plant instance from a strain definition.
        /// </summary>
        public PlantInstance CreatePlant(PlantStrainSO strain, Vector3 position, Transform parent = null)
        {
            return _lifecycleService?.CreatePlant(strain, position, parent);
        }
        
        /// <summary>
        /// Creates multiple plants from the same strain.
        /// </summary>
        public List<PlantInstance> CreatePlants(PlantStrainSO strain, List<Vector3> positions, Transform parent = null)
        {
            return _lifecycleService?.CreatePlants(strain, positions, parent) ?? new List<PlantInstance>();
        }
        
        /// <summary>
        /// Registers a plant instance with the manager.
        /// </summary>
        public void RegisterPlant(PlantInstance plant)
        {
            _lifecycleService?.RegisterPlant(plant);
        }
        
        /// <summary>
        /// Unregisters a plant from management.
        /// </summary>
        public void UnregisterPlant(string plantID, PlantRemovalReason reason = PlantRemovalReason.Other)
        {
            _lifecycleService?.UnregisterPlant(plantID, reason);
        }
        
        /// <summary>
        /// Gets a plant instance by ID.
        /// </summary>
        public PlantInstanceSO GetPlant(string plantID)
        {
            return _lifecycleService?.GetPlant(plantID);
        }
        
        /// <summary>
        /// Gets all active plants.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            return _lifecycleService?.GetAllPlants() ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsInStage(PlantGrowthStage stage)
        {
            return _lifecycleService?.GetPlantsInStage(stage) ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants ready for harvest.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetHarvestablePlants()
        {
            return _lifecycleService?.GetHarvestablePlants() ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants that need attention.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            return _lifecycleService?.GetPlantsNeedingAttention() ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Harvests a plant and returns the harvest results.
        /// </summary>
        public SystemsHarvestResults HarvestPlant(string plantID)
        {
            return _harvestService?.HarvestPlant(plantID);
        }
        
        /// <summary>
        /// Calculates expected yield for a plant instance.
        /// </summary>
        public float CalculateExpectedYield(PlantInstance plantInstance)
        {
            return _harvestService?.CalculateExpectedYield(plantInstance) ?? 0f;
        }
        
        /// <summary>
        /// Updates environmental conditions for all plants.
        /// </summary>
        public void UpdateEnvironmentalConditions(EnvironmentalConditions newConditions)
        {
            _environmentalService?.UpdateEnvironmentalConditions(newConditions);
        }
        
        /// <summary>
        /// Applies environmental stress to all plants.
        /// </summary>
        public void ApplyEnvironmentalStress(ProjectChimera.Data.Environment.EnvironmentalStressSO stressSource, float intensity)
        {
            _environmentalService?.ApplyEnvironmentalStress(stressSource, intensity);
        }
        
        /// <summary>
        /// Gets comprehensive plant manager statistics.
        /// </summary>
        public PlantManagerStatistics GetStatistics()
        {
            return _statisticsService?.GetStatistics() ?? new PlantManagerStatistics();
        }
        
        /// <summary>
        /// Gets enhanced statistics including genetic performance data.
        /// </summary>
        public EnhancedPlantManagerStatistics GetEnhancedStatistics()
        {
            return _statisticsService?.GetEnhancedStatistics() ?? new EnhancedPlantManagerStatistics();
        }
        
        /// <summary>
        /// Gets genetic performance statistics.
        /// </summary>
        public GeneticPerformanceStats GetGeneticPerformanceStats()
        {
            return _geneticsService?.GetGeneticPerformanceStats() ?? new GeneticPerformanceStats();
        }
        
        /// <summary>
        /// Enables or disables advanced genetics.
        /// </summary>
        public void SetAdvancedGeneticsEnabled(bool enabled)
        {
            _geneticsService?.SetAdvancedGeneticsEnabled(enabled);
        }
        
        #endregion
        
        #region Service Access (for advanced usage)
        
        /// <summary>
        /// Gets the lifecycle service.
        /// </summary>
        public IPlantLifecycleService GetLifecycleService() => _lifecycleService;
        
        /// <summary>
        /// Gets the processing service.
        /// </summary>
        public IPlantProcessingService GetProcessingService() => _processingService;
        
        /// <summary>
        /// Gets the achievement service.
        /// </summary>
        public IPlantAchievementService GetAchievementService() => _achievementService;
        
        /// <summary>
        /// Gets the genetics service.
        /// </summary>
        public IPlantGeneticsService GetGeneticsService() => _geneticsService;
        
        /// <summary>
        /// Gets the harvest service.
        /// </summary>
        public IPlantHarvestService GetHarvestService() => _harvestService;
        
        /// <summary>
        /// Gets the statistics service.
        /// </summary>
        public IPlantStatisticsService GetStatisticsService() => _statisticsService;
        
        /// <summary>
        /// Gets the environmental service.
        /// </summary>
        public IPlantEnvironmentalService GetEnvironmentalService() => _environmentalService;
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlantAddedHandler(PlantInstance plant)
        {
            OnPlantAdded?.Invoke(plant);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[RefactoredPlantManager] Plant added: {plant.PlantID}");
            }
        }
        
        private void OnPlantStageChangedHandler(PlantInstance plant)
        {
            OnPlantStageChanged?.Invoke(plant);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[RefactoredPlantManager] Plant stage changed: {plant.PlantID} -> {plant.CurrentGrowthStage}");
            }
        }
        
        private void OnPlantHealthUpdatedHandler(PlantInstance plant)
        {
            OnPlantHealthUpdated?.Invoke(plant);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[RefactoredPlantManager] Plant health updated: {plant.PlantID} -> {plant.CurrentHealth:F1}");
            }
        }
        
        private void OnPlantHarvestedHandler(PlantInstance plant)
        {
            OnPlantHarvested?.Invoke(plant);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[RefactoredPlantManager] Plant harvested: {plant.PlantID}");
            }
        }
        
        #endregion
    }
}