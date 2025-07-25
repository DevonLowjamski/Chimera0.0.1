using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.ServiceContainer;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using System;
using ProjectChimera.Systems.Genetics; // Added for proper GeneticPerformanceMonitor
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions; // Explicit alias
using TraitExpressionResult = ProjectChimera.Systems.Genetics.TraitExpressionResult; // Resolve ambiguous reference
using StressResponse = ProjectChimera.Systems.Genetics.StressResponse; // Resolve ambiguous reference
using StressFactor = ProjectChimera.Systems.Genetics.StressFactor; // Resolve ambiguous reference
using GeneticPerformanceStats = ProjectChimera.Systems.Cultivation.GeneticPerformanceStats; // Use Systems version
using GeneticsPerformanceStats = ProjectChimera.Systems.Genetics.GeneticsPerformanceStats; // Use Genetics version
using SystemsHarvestResults = ProjectChimera.Systems.Cultivation.SystemsHarvestResults; // Use Systems version
using HarvestResults = ProjectChimera.Systems.Cultivation.HarvestResults; // Use Systems version
using CultivationHarvestResults = ProjectChimera.Systems.Cultivation.HarvestResults; // Use Systems version

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Core plant management system for Project Chimera.
    /// Handles plant lifecycle, growth simulation, harvest mechanics, and cultivation achievements.
    /// Uses event-driven integration with progression systems.
    /// </summary>
    public class PlantManager : ChimeraManager
    {
        [Header("Plant Management Configuration")]
        [SerializeField] private bool _enablePlantAI = true;
        [SerializeField] private bool _enableAutoHarvest = false;
        [SerializeField] private bool _enableQualityTracking = true;
        [SerializeField] private float _updateInterval = 1f;

        [Header("Growth Settings")]
        [SerializeField] private bool _enableRealisticGrowthCycles = true;
        [SerializeField] private float _growthRateMultiplier = 1f;
        [SerializeField] private bool _enableEnvironmentalStress = true;
        [SerializeField] private float _stressRecoveryRate = 0.1f;

        [Header("Harvest Configuration")]
        [SerializeField] private bool _enableYieldVariability = true;
        [SerializeField] private float _harvestQualityMultiplier = 1f;
        [SerializeField] private bool _enablePostHarvestProcessing = true;

        [Header("Achievement Events")]
        [SerializeField] private SimpleGameEventSO _onPlantCreated;
        [SerializeField] private SimpleGameEventSO _onPlantHarvested;
        [SerializeField] private SimpleGameEventSO _onQualityHarvest;
        [SerializeField] private SimpleGameEventSO _onPerfectQuality;
        [SerializeField] private SimpleGameEventSO _onHighYieldAchieved;
        [SerializeField] private SimpleGameEventSO _onPotencyRecord;
        [SerializeField] private SimpleGameEventSO _onTerpeneProfile;

        [Header("Plant Management")]
        [SerializeField] private float _plantUpdateInterval = 1f; // seconds
        [SerializeField] private int _maxPlantsPerUpdate = 10;
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Growth Configuration")]
        [SerializeField] private AnimationCurve _defaultGrowthCurve;
        [SerializeField] private float _globalGrowthModifier = 1f;
        [SerializeField] private bool _enableStressSystem = true;
        [SerializeField] private bool _enableGxEInteractions = true;
        
        [Header("Phase 3.1: Advanced Genetics Integration")]
        [SerializeField] private bool _enableAdvancedGenetics = true;
        [SerializeField] private bool _enableGeneticPerformanceMonitoring = true;
        [SerializeField] private float _traitExpressionCacheCleanupInterval = 60f; // Clear cache every minute
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<PlantInstance> _onPlantGrowthStageChanged;
        [SerializeField] private GameEventSO<PlantInstance> _onPlantHealthChanged;
        [SerializeField] private GameEventSO<PlantInstance> _onPlantDied;
        
        [Header("Achievement Integration")]
        [SerializeField] private bool _enableAchievementTracking = true;
        
        // Private fields - CultivationManager is the single source of truth for plant data
        private Dictionary<string, PlantInstance> _activePlants = new Dictionary<string, PlantInstance>();
        private List<PlantInstance> _plantsToUpdate = new List<PlantInstance>();
        private int _currentUpdateIndex = 0;
        private float _lastUpdateTime = 0f;
        private PlantUpdateProcessor _updateProcessor;
        
        // Phase 3.1: Advanced genetics integration
        private float _lastCacheCleanupTime = 0f;
        private GeneticPerformanceMonitor _geneticPerformanceMonitor;
        
        // PC014-2a: Dependency injection - services injected via service container
        [Inject] private CultivationManager _cultivationManager;
        [Inject] private IPlantLifecycleService _lifecycleService;
        [Inject] private IPlantGrowthService _growthService;
        [Inject] private IPlantEnvironmentalProcessingService _environmentalService;
        [Inject] private IPlantYieldCalculationService _yieldService;
        [Inject] private IPlantGeneticsService _geneticsService;
        [Inject] private IPlantHarvestService _harvestService;
        [Inject] private IPlantStatisticsService _statisticsService;
        [Inject] private IPlantAchievementService _achievementService;
        [Inject] private IPlantProcessingService _processingService;
        
        // Manager references removed to prevent cyclic assembly dependencies

        // Achievement tracking - now event-based
        private CultivationEventTracker _eventTracker;
        
        // Events for other systems to subscribe to
        public System.Action<PlantInstance> OnPlantAdded;
        public System.Action<PlantInstance> OnPlantHarvested;
        public System.Action<PlantInstance> OnPlantStageChanged;
        public System.Action<PlantInstance> OnPlantWatered;
        public System.Action<PlantInstance> OnPlantHealthUpdated;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties - delegate to CultivationManager
        public int ActivePlantCount => _cultivationManager?.ActivePlantCount ?? 0;
        public float GlobalGrowthModifier 
        { 
            get => _globalGrowthModifier; 
            set => _globalGrowthModifier = Mathf.Clamp(value, 0.1f, 10f); 
        }
        
        protected override void OnManagerInitialize()
        {
            // PC014-2a: Use dependency injection for service resolution
            ServiceInjector.InjectDependencies(this);
            
            // Validate critical dependencies
            if (_cultivationManager == null)
            {
                LogWarning("CultivationManager not injected! PlantManager will operate in standalone testing mode.");
                // Continue initialization for testing scenarios
            }
            
            if (_growthService == null || _environmentalService == null || _yieldService == null)
            {
                LogWarning("One or more cultivation services not injected. Some features may be limited.");
            }
            
            // Initialize enhanced update processor with advanced genetics support
            _updateProcessor = new PlantUpdateProcessor(_enableStressSystem, _enableGxEInteractions, _enableAdvancedGenetics);
            
            // Initialize genetic performance monitoring
            if (_enableGeneticPerformanceMonitoring)
            {
                _geneticPerformanceMonitor = new GeneticPerformanceMonitor();
            }
            
            // PC013-4: Initialize specialized services with dependency injection
            InitializeSpecializedServices();
            
            // Initialize achievement tracking
            if (_enableAchievementTracking)
            {
                InitializeAchievementTracking();
            }
            
            LogInfo($"PlantManager initialized - delegating plant data to CultivationManager (Advanced Genetics: {_enableAdvancedGenetics})");
            
            // Initialize growth curve if not set
            if (_defaultGrowthCurve == null || _defaultGrowthCurve.keys.Length == 0)
            {
                InitializeDefaultGrowthCurve();
            }
        }

        protected override void OnManagerShutdown()
        {
            LogInfo("PlantManager shutting down - CultivationManager handles plant cleanup");
            
            // PC013-4: Shutdown specialized services
            _growthService?.Shutdown();
            _environmentalService?.Shutdown();
            _yieldService?.Shutdown();
            
            // Clear update tracking only - CultivationManager handles plant cleanup
            _plantsToUpdate.Clear();
            _updateProcessor = null;
            _cultivationManager = null;
        }
        
        protected override void OnManagerUpdate()
        {
            if (Time.time - _lastUpdateTime >= _plantUpdateInterval)
            {
                UpdatePlants();
                _lastUpdateTime = Time.time;
            }
            
            // Phase 3.1: Periodic cache cleanup for performance optimization
            if (_enableAdvancedGenetics && Time.time - _lastCacheCleanupTime >= _traitExpressionCacheCleanupInterval)
            {
                PerformCacheCleanup();
                _lastCacheCleanupTime = Time.time;
            }
        }
        
        /// <summary>
        /// Creates a new plant instance from a strain definition.
        /// </summary>
        public PlantInstance CreatePlant(PlantStrainSO strain, Vector3 position, Transform parent = null)
        {
            if (strain == null)
            {
                LogError("Cannot create plant: strain is null");
                return null;
            }
            
            var plantInstance = PlantInstance.CreateFromStrain(strain, position, parent);
            RegisterPlant(plantInstance);
            
            LogInfo($"Created plant: {plantInstance.PlantID} (Strain: {strain.StrainName})");
            
            // Track plant creation achievements
            if (_enableAchievementTracking && _eventTracker != null)
            {
                _eventTracker.OnPlantCreated(plantInstance);
            }
            
            // Trigger plant created event for progression system
            _onPlantCreated?.Raise();
            
            return plantInstance;
        }
        
        /// <summary>
        /// Creates multiple plants from the same strain.
        /// </summary>
        public List<PlantInstance> CreatePlants(PlantStrainSO strain, List<Vector3> positions, Transform parent = null)
        {
            var plants = new List<PlantInstance>();
            
            foreach (var position in positions)
            {
                var plant = CreatePlant(strain, position, parent);
                if (plant != null)
                    plants.Add(plant);
            }
            
            LogInfo($"Created {plants.Count} plants from strain: {strain.StrainName}");
            return plants;
        }
        
        /// <summary>
        /// Registers an existing plant instance with the manager.
        /// PlantManager now handles UI/processing aspects while CultivationManager manages data.
        /// </summary>
        public void RegisterPlant(PlantInstance plant)
        {
            if (plant == null)
            {
                LogError("Cannot register null plant");
                return;
            }
            
            if (_cultivationManager == null)
            {
                LogError("Cannot register plant: CultivationManager reference is null");
                return;
            }
            
            // Check if plant is already tracked by CultivationManager
            var existingPlant = _cultivationManager.GetPlant(plant.PlantID);
            if (existingPlant != null)
            {
                LogWarning($"Plant {plant.PlantID} already registered in CultivationManager, skipping");
                return;
            }
            
            // Add to processing update list only
            if (!_plantsToUpdate.Contains(plant))
            {
                _plantsToUpdate.Add(plant);
            }
            
            // Subscribe to plant events
            plant.OnGrowthStageChanged += OnPlantGrowthStageChanged;
            plant.OnHealthChanged += OnPlantHealthChanged;
            plant.OnPlantDied += OnPlantDied;
            
            // Invoke OnPlantAdded event for other systems
            OnPlantAdded?.Invoke(plant);
            
            if (_enableDetailedLogging)
                LogInfo($"Registered plant for processing: {plant.PlantID}");
        }
        
        /// <summary>
        /// Registers a plant instance (alternative signature for compatibility).
        /// </summary>
        public void RegisterPlantInstance(PlantInstance plantInstance)
        {
            RegisterPlant(plantInstance);
        }

        /// <summary>
        /// Registers a SpeedTree plant instance (compatibility overload).
        /// NOTE: SpeedTree integration commented out until proper assembly references are configured
        /// </summary>
        public void RegisterPlantInstance(object speedTreeInstance)
        {
            // Generic object parameter to avoid SpeedTree dependency issues
            LogWarning("SpeedTree plant registration attempted but SpeedTree integration is disabled");
        }

        /// <summary>
        /// Registers an interactive plant component (compatibility overload).
        /// </summary>
        public void RegisterPlantInstance(ProjectChimera.Systems.Cultivation.InteractivePlantComponent interactivePlant)
        {
            if (interactivePlant == null)
            {
                LogError("Cannot register null interactive plant component");
                return;
            }

            // Try to get the underlying PlantInstance from the component
            var plantInstance = interactivePlant.GetComponent<PlantInstance>();
            if (plantInstance != null)
            {
                RegisterPlant(plantInstance);
                LogInfo($"Registered interactive plant component: {interactivePlant.name}");
            }
            else
            {
                LogWarning($"Interactive plant component {interactivePlant.name} has no PlantInstance component");
            }
        }

        /// <summary>
        /// Unregisters a SpeedTree plant instance.
        /// NOTE: SpeedTree integration commented out until proper assembly references are configured
        /// </summary>
        /*
        public void UnregisterPlantInstance(ProjectChimera.Systems.SpeedTree.SpeedTreePlantInstance plantInstance)
        {
            if (plantInstance == null)
            {
                LogError("Cannot unregister null SpeedTree plant instance");
                return;
            }

            var plantId = plantInstance.PlantId;
            UnregisterPlant(plantId, PlantRemovalReason.Removed);
        }
        */

        /// <summary>
        /// Unregisters a plant instance by ID (compatibility method).
        /// </summary>
        public void UnregisterPlantInstance(string plantID)
        {
            UnregisterPlant(plantID, PlantRemovalReason.Removed);
        }

        /// <summary>
        /// Unregisters a plant instance by object (compatibility method).
        /// </summary>
        public void UnregisterPlantInstance(PlantInstance plantInstance)
        {
            if (plantInstance == null)
            {
                LogError("Cannot unregister null plant instance");
                return;
            }
            
            UnregisterPlant(plantInstance.PlantID, PlantRemovalReason.Removed);
        }

        /// <summary>
        /// Unregisters a plant instance by instance ID (compatibility method).
        /// </summary>
        public void UnregisterPlantInstance(int instanceId)
        {
            // Find plant by Unity instance ID
            var plant = _plantsToUpdate.FirstOrDefault(p => p.GetInstanceID() == instanceId);
            if (plant != null)
            {
                UnregisterPlant(plant.PlantID, PlantRemovalReason.Removed);
            }
            else
            {
                LogWarning($"No plant found with instance ID: {instanceId}");
            }
        }

        /// <summary>
        /// Updates environmental adaptation for all plants based on current conditions.
        /// </summary>
        public void UpdateEnvironmentalAdaptation(ProjectChimera.Data.Cultivation.EnvironmentalConditions conditions)
        {
            foreach (var plant in _plantsToUpdate)
            {
                if (plant != null)
                {
                    // Update plant's environmental adaptation
                    plant.UpdateEnvironmentalAdaptation(conditions);
                }
            }
            
            LogInfo($"Updated environmental adaptation for {_plantsToUpdate.Count} plants");
        }

        /// <summary>
        /// Updates environmental adaptation for a specific plant.
        /// </summary>
        public void UpdateEnvironmentalAdaptation(string plantID, ProjectChimera.Data.Cultivation.EnvironmentalConditions conditions)
        {
            var plant = _plantsToUpdate.FirstOrDefault(p => p.PlantID == plantID);
            if (plant != null)
            {
                plant.UpdateEnvironmentalAdaptation(conditions);
                LogInfo($"Updated environmental adaptation for plant {plantID}");
            }
            else
            {
                LogWarning($"Plant {plantID} not found for environmental adaptation update");
            }
        }

        /// <summary>
        /// Removes a plant from management (for harvesting, death, etc.).
        /// PlantManager only handles processing cleanup - CultivationManager handles data removal.
        /// </summary>
        public void UnregisterPlant(string plantID, PlantRemovalReason reason = PlantRemovalReason.Other)
        {
            // Find plant in processing list
            var plant = _plantsToUpdate.FirstOrDefault(p => p.PlantID == plantID);
            if (plant == null)
            {
                LogWarning($"Attempted to unregister unknown plant from processing: {plantID}");
                return;
            }
            
            // Unsubscribe from events
            plant.OnGrowthStageChanged -= OnPlantGrowthStageChanged;
            plant.OnHealthChanged -= OnPlantHealthChanged;
            plant.OnPlantDied -= OnPlantDied;
            
            // Remove from processing list only
            _plantsToUpdate.Remove(plant);
            
            // Raise appropriate event
            switch (reason)
            {
                case PlantRemovalReason.Harvested:
                    _onPlantHarvested?.Raise();
                    break;
                case PlantRemovalReason.Died:
                    _onPlantDied?.Raise(plant);
                    break;
            }
            
            LogInfo($"Unregistered plant from processing: {plantID} (Reason: {reason})");
        }
        
        /// <summary>
        /// Gets a plant instance by ID - delegates to CultivationManager.
        /// Note: Returns PlantInstanceSO from CultivationManager, not PlantInstance.
        /// </summary>
        public PlantInstanceSO GetPlant(string plantID)
        {
            return _cultivationManager?.GetPlant(plantID);
        }
        
        /// <summary>
        /// Gets a plant instance by ID (alternative method name for compatibility).
        /// Returns PlantInstanceSO from CultivationManager.
        /// </summary>
        public PlantInstanceSO GetPlantInstance(string plantID)
        {
            return GetPlant(plantID);
        }
        
        /// <summary>
        /// Gets all active plants - delegates to CultivationManager.
        /// Returns PlantInstanceSO objects from CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            return _cultivationManager?.GetAllPlants() ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all active plant instances (alternative method name for compatibility).
        /// Returns PlantInstanceSO objects from CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlantInstances()
        {
            return GetAllPlants();
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsInStage(PlantGrowthStage stage)
        {
            return _cultivationManager?.GetPlantsByStage(stage) ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants ready for harvest - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetHarvestablePlants()
        {
            return GetPlantsInStage(PlantGrowthStage.Harvest);
        }
        
        /// <summary>
        /// Gets all plants that need attention (low health, stress, etc.) - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            return _cultivationManager?.GetPlantsNeedingAttention() ?? Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Updates the environmental conditions for all plants - delegates to CultivationManager.
        /// </summary>
        public void UpdateEnvironmentalConditions(ProjectChimera.Data.Cultivation.EnvironmentalConditions newConditions)
        {
            if (_cultivationManager != null)
            {
                _cultivationManager.SetZoneEnvironment("default", newConditions);
                LogInfo($"Updated environmental conditions via CultivationManager");
            }
            else
            {
                LogError("Cannot update environmental conditions: CultivationManager is null");
            }
        }
        
        /// <summary>
        /// Applies environmental stress to all plants.
        /// </summary>
        public void ApplyEnvironmentalStress(EnvironmentalStressSO stressSource, float intensity)
        {
            if (!_enableStressSystem || stressSource == null)
                return;
            
            int affectedPlants = 0;
            foreach (var plant in _plantsToUpdate)
            {
                if (plant != null && plant.ApplyStress(stressSource, intensity))
                    affectedPlants++;
            }
            
            LogInfo($"Applied stress '{stressSource.StressName}' to {affectedPlants} plants");
        }
        
        /// <summary>
        /// Harvests a plant and returns the harvest results.
        /// PC013-4: Refactored to use specialized YieldCalculationService
        /// </summary>
        public SystemsHarvestResults HarvestPlant(string plantID)
        {
            // PC013-4: Delegate to specialized yield calculation service
            if (_yieldService != null)
            {
                var harvestResults = _yieldService.HarvestPlant(plantID);
                if (harvestResults != null)
                {
                    // Get the PlantInstance for event tracking
                    var plantInstance = _plantsToUpdate.FirstOrDefault(p => p.PlantID == plantID);
                    
                    // Track harvest achievements before unregistering
                    if (_enableAchievementTracking && _eventTracker != null && plantInstance != null)
                    {
                        _eventTracker.OnPlantHarvested(plantInstance, harvestResults);
                    }
                    
                    // Invoke OnPlantHarvested event for other systems
                    if (plantInstance != null)
                    {
                        OnPlantHarvested?.Invoke(plantInstance);
                    }
                    
                    // Trigger harvest events for progression system to listen to
                    _onPlantHarvested?.Raise();
                    
                    // Quality-based events
                    if (harvestResults.QualityScore >= 0.9f)
                    {
                        _onQualityHarvest?.Raise();
                    }
                    if (harvestResults.QualityScore >= 0.95f)
                    {
                        _onPerfectQuality?.Raise();
                    }
                    
                    // Yield-based events
                    if (harvestResults.TotalYieldGrams >= 50f)
                    {
                        _onHighYieldAchieved?.Raise();
                    }
                    
                    UnregisterPlant(plantID, PlantRemovalReason.Harvested);
                    
                    LogInfo($"Harvested plant {plantID}: {harvestResults.TotalYieldGrams}g yield, {harvestResults.QualityScore:F2} quality");
                    
                    return harvestResults;
                }
            }
            
            // Fallback to legacy implementation if service is not available
            LogWarning("YieldCalculationService not available - using legacy harvest implementation");
            return HarvestPlantLegacy(plantID);
        }
        
        /// <summary>
        /// Legacy harvest implementation for fallback
        /// </summary>
        private SystemsHarvestResults HarvestPlantLegacy(string plantID)
        {
            var plant = GetPlant(plantID);
            if (plant == null)
            {
                // Use warning instead of error for testing scenarios
                if (GameManager.Instance == null || GameManager.Instance.GetManager<CultivationManager>() == null)
                {
                    LogWarning($"Cannot harvest unknown plant: {plantID} (PlantManager in testing mode)");
                }
                else
                {
                    LogError($"Cannot harvest unknown plant: {plantID}");
                }
                return null;
            }
            
            if (plant.CurrentGrowthStage != PlantGrowthStage.Harvest)
            {
                LogWarning($"Plant {plantID} is not ready for harvest (Stage: {plant.CurrentGrowthStage})");
                return null;
            }
            
            var dataHarvestResults = plant.Harvest();
            
            // Convert Data layer HarvestResults to Systems layer HarvestResults
            var harvestResults = new SystemsHarvestResults
            {
                PlantID = dataHarvestResults.PlantID,
                TotalYieldGrams = dataHarvestResults.TotalYieldGrams,
                QualityScore = dataHarvestResults.QualityScore,
                Cannabinoids = new CannabinoidProfile(), // Create empty profile
                Terpenes = new TerpeneProfile(), // Create empty profile
                FloweringDays = (int)plant.AgeInDays,
                FinalHealth = plant.OverallHealth,
                HarvestDate = dataHarvestResults.HarvestDate
            };
            
            return harvestResults;
        }
        
        /// <summary>
        /// Gets comprehensive statistics about all managed plants - delegates to CultivationManager.
        /// </summary>
        public PlantManagerStatistics GetStatistics()
        {
            var stats = new PlantManagerStatistics();
            
            if (_cultivationManager == null)
            {
                LogWarning("Cannot get statistics: CultivationManager is null");
                return stats;
            }
            
            var cultivationStats = _cultivationManager.GetCultivationStats();
            stats.TotalPlants = cultivationStats.active;
            
            // Get detailed plant data from CultivationManager
            var allPlants = _cultivationManager.GetAllPlants();
            foreach (var plant in allPlants)
            {
                if (plant != null)
                {
                    stats.PlantsByStage[(int)plant.CurrentGrowthStage]++;
                    stats.AverageHealth += plant.OverallHealth;
                    stats.AverageStress += plant.StressLevel;
                    
                    if (plant.OverallHealth < 0.5f)
                        stats.UnhealthyPlants++;
                    
                    if (plant.StressLevel > 0.7f)
                        stats.HighStressPlants++;
                }
            }
            
            if (stats.TotalPlants > 0)
            {
                stats.AverageHealth /= stats.TotalPlants;
                stats.AverageStress /= stats.TotalPlants;
            }
            
            return stats;
        }
        
        /// <summary>
        /// Phase 3.2: Enhanced plant update system with batch processing optimization.
        /// Automatically selects optimal processing method based on plant count and performance.
        /// </summary>
        private void UpdatePlants()
        {
            if (_plantsToUpdate.Count == 0)
                return;
            
            var timeManager = GameManager.Instance.GetManager<TimeManager>();
            float deltaTime = timeManager.GetScaledDeltaTime();
            
            // Determine optimal batch size based on plant count and performance
            int optimalBatchSize = CalculateOptimalBatchSize();
            int plantsToProcess = Mathf.Min(optimalBatchSize, _plantsToUpdate.Count);
            
            // Get the next batch of plants to process
            var plantsToProcessThisFrame = GetNextPlantsToProcess(plantsToProcess);
            
            if (plantsToProcessThisFrame.Count == 0)
                return;
            
            // PC013-4: Use specialized services with batch processing for improved performance
            if (_enableAdvancedGenetics && plantsToProcessThisFrame.Count > 10)
            {
                // Use specialized services for batch processing
                if (_growthService != null)
                {
                    _growthService.UpdatePlantGrowthBatch(plantsToProcessThisFrame, deltaTime);
                }
                
                if (_environmentalService != null)
                {
                    _environmentalService.UpdatePlantEnvironmentalProcessingBatch(plantsToProcessThisFrame, deltaTime);
                }
                
                // Fallback to update processor for other systems
                _updateProcessor.UpdatePlantsBatch(plantsToProcessThisFrame, deltaTime, _globalGrowthModifier);
                
                // Update genetic performance monitoring
                if (_enableGeneticPerformanceMonitoring && _geneticPerformanceMonitor != null)
                {
                    var performanceStats = _updateProcessor.GetPerformanceMetrics();
                    // Convert GeneticPerformanceStats to GeneticsPerformanceStats
                    var geneticsStats = new GeneticsPerformanceStats
                    {
                        TotalCalculations = performanceStats.TotalCalculations,
                        AverageCalculationTimeMs = performanceStats.AverageCalculationTimeMs,
                        CacheHitRatio = performanceStats.CacheHitRatio,
                        BatchCalculations = performanceStats.BatchCalculations,
                        AverageBatchTimeMs = performanceStats.AverageBatchTimeMs
                    };
                    _geneticPerformanceMonitor.RecordBatchUpdate(plantsToProcessThisFrame.Count, geneticsStats);
                }
            }
            else
            {
                // PC013-4: Use specialized services for individual processing
                foreach (var plant in plantsToProcessThisFrame)
                {
                    if (plant != null && plant.IsActive)
                    {
                        // Use specialized services for individual processing
                        if (_growthService != null)
                        {
                            _growthService.UpdatePlantGrowth(plant, deltaTime);
                        }
                        
                        if (_environmentalService != null)
                        {
                            _environmentalService.UpdatePlantEnvironmentalProcessing(plant, deltaTime);
                        }
                        
                        // Fallback to update processor for other systems
                        _updateProcessor.UpdatePlant(plant, deltaTime, _globalGrowthModifier);
                    }
                }
            }
            
            // Advance update index
            _currentUpdateIndex += plantsToProcess;
            
            // Reset index when we've processed all plants
            if (_currentUpdateIndex >= _plantsToUpdate.Count)
            {
                _currentUpdateIndex = 0;
                
                // Remove any inactive plants from the update list
                _plantsToUpdate.RemoveAll(p => p == null || !p.IsActive);
                
                // Perform periodic performance optimization
                if (_enableGeneticPerformanceMonitoring && UnityEngine.Random.Range(0f, 1f) < 0.1f) // 10% chance per cycle
                {
                    PerformPerformanceOptimization();
                }
            }
        }
        
        /// <summary>
        /// Phase 3.2: Calculate optimal batch size based on performance metrics and system resources.
        /// </summary>
        private int CalculateOptimalBatchSize()
        {
            int baseBatchSize = _maxPlantsPerUpdate;
            
            if (_enableGeneticPerformanceMonitoring && _geneticPerformanceMonitor != null)
            {
                var performanceData = _geneticPerformanceMonitor.GetPerformanceData();
                
                // Adjust batch size based on recent performance
                if (performanceData.AverageUpdateTimeMs > 16.0f) // Target 60 FPS
                {
                    baseBatchSize = Mathf.Max(5, baseBatchSize / 2); // Reduce batch size
                }
                else if (performanceData.AverageUpdateTimeMs < 5.0f)
                {
                    baseBatchSize = Mathf.Min(50, baseBatchSize * 2); // Increase batch size
                }
            }
            
            // Consider system memory and GPU availability
            if (_enableAdvancedGenetics && SystemInfo.supportsComputeShaders && SystemInfo.systemMemorySize > 8192)
            {
                baseBatchSize = Mathf.Min(100, baseBatchSize * 2); // Larger batches for powerful systems
            }
            
            return baseBatchSize;
        }
        
        /// <summary>
        /// Get the next batch of plants to process in this frame.
        /// </summary>
        private List<PlantInstance> GetNextPlantsToProcess(int count)
        {
            var plantsToProcess = new List<PlantInstance>();
            int endIndex = Mathf.Min(_currentUpdateIndex + count, _plantsToUpdate.Count);
            
            for (int i = _currentUpdateIndex; i < endIndex; i++)
            {
                var plant = _plantsToUpdate[i];
                if (plant != null && plant.IsActive)
                {
                    plantsToProcess.Add(plant);
                }
            }
            
            return plantsToProcess;
        }
        
        /// <summary>
        /// Phase 3.2: Perform cache cleanup and performance optimization.
        /// </summary>
        private void PerformCacheCleanup()
        {
            if (_updateProcessor != null)
            {
                _updateProcessor.ClearTraitExpressionCache();
            }
            
            if (_enableDetailedLogging)
            {
                LogInfo("Performed cache cleanup for genetic calculations");
            }
        }
        
        /// <summary>
        /// Phase 3.2: Perform comprehensive performance optimization.
        /// </summary>
        private void PerformPerformanceOptimization()
        {
            if (_updateProcessor != null)
            {
                _updateProcessor.OptimizePerformance();
            }
            
            // Log performance statistics
            if (_enableGeneticPerformanceMonitoring && _geneticPerformanceMonitor != null)
            {
                var stats = _updateProcessor.GetPerformanceMetrics();
                LogInfo($"Performance Stats: {stats}");
            }
        }
        
        private void OnPlantGrowthStageChanged(PlantInstance plant)
        {
            LogInfo($"Plant {plant.PlantID} advanced to {plant.CurrentGrowthStage}");
            _onPlantGrowthStageChanged?.Raise(plant);
            
            // Invoke OnPlantStageChanged event for other systems
            OnPlantStageChanged?.Invoke(plant);
            
            // Track achievement progress
            if (_enableAchievementTracking && _eventTracker != null)
            {
                _eventTracker.OnPlantGrowthStageChanged(plant);
            }
        }
        
        private void OnPlantHealthChanged(PlantInstance plant)
        {
            if (_enableDetailedLogging)
                LogInfo($"Plant {plant.PlantID} health changed to {plant.CurrentHealth:F2}");
            
            _onPlantHealthChanged?.Raise(plant);
            
            // Invoke OnPlantHealthUpdated event for other systems
            OnPlantHealthUpdated?.Invoke(plant);
            
            // Track health-related achievements
            if (_enableAchievementTracking && _eventTracker != null)
            {
                _eventTracker.OnPlantHealthChanged(plant);
            }
        }
        
        private void OnPlantDied(PlantInstance plant)
        {
            LogInfo($"Plant {plant.PlantID} died (Health: {plant.CurrentHealth:F2})");
            
            // Track plant death for achievements
            if (_enableAchievementTracking && _eventTracker != null)
            {
                _eventTracker.OnPlantDied(plant);
            }
            
            UnregisterPlant(plant.PlantID, PlantRemovalReason.Died);
        }
        
        private void InitializeDefaultGrowthCurve()
        {
            _defaultGrowthCurve = new AnimationCurve();
            _defaultGrowthCurve.AddKey(0f, 0f);      // Start
            _defaultGrowthCurve.AddKey(0.25f, 0.1f); // Slow initial growth
            _defaultGrowthCurve.AddKey(0.5f, 0.4f);  // Accelerating growth
            _defaultGrowthCurve.AddKey(0.75f, 0.8f); // Peak growth
            _defaultGrowthCurve.AddKey(1f, 1f);      // Mature
            
            LogInfo("Initialized default growth curve");
        }
        
        /// <summary>
        /// PC013-4: Initialize specialized services with dependency injection
        /// </summary>
        private void InitializeSpecializedServices()
        {
            LogInfo("Initializing specialized plant services...");
            
            // Initialize growth service
            _growthService = new PlantGrowthService();
            _growthService.GlobalGrowthModifier = _globalGrowthModifier;
            _growthService.Initialize();
            
            // Initialize environmental processing service
            _environmentalService = new PlantEnvironmentalProcessingService();
            _environmentalService.EnableStressSystem = _enableStressSystem;
            _environmentalService.EnableGxEInteractions = _enableGxEInteractions;
            _environmentalService.StressRecoveryRate = _stressRecoveryRate;
            _environmentalService.Initialize();
            
            // Initialize yield calculation service with dependency injection
            _yieldService = new PlantYieldCalculationService(_environmentalService);
            _yieldService.EnableYieldVariability = _enableYieldVariability;
            _yieldService.EnablePostHarvestProcessing = _enablePostHarvestProcessing;
            _yieldService.HarvestQualityMultiplier = _harvestQualityMultiplier;
            _yieldService.Initialize();
            
            LogInfo("Specialized plant services initialized successfully");
        }

        private void InitializeAchievementTracking()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // Event-based progression tracking enabled
            }
            else
            {
                LogWarning("GameManager not found - achievement tracking disabled");
                _enableAchievementTracking = false;
            }
        }

        /// <summary>
        /// Calculate expected yield for a plant instance based on its current state and growth conditions
        /// PC013-4: Refactored to use specialized YieldCalculationService
        /// </summary>
        public float CalculateExpectedYield(PlantInstance plantInstance)
        {
            // PC013-4: Delegate to specialized yield calculation service
            if (_yieldService != null)
            {
                return _yieldService.CalculateExpectedYield(plantInstance);
            }
            
            // Fallback to legacy implementation if service is not available
            LogWarning("YieldCalculationService not available - using legacy yield calculation");
            return CalculateExpectedYieldLegacy(plantInstance);
        }
        
        /// <summary>
        /// Legacy yield calculation implementation for fallback
        /// </summary>
        private float CalculateExpectedYieldLegacy(PlantInstance plantInstance)
        {
            if (plantInstance == null)
            {
                LogWarning("Cannot calculate expected yield for null plant instance");
                return 0f;
            }

            // Get base yield from strain data
            float baseYield = plantInstance.Strain?.BaseYieldGrams ?? 50f; // Default 50g if strain data missing
            
            // Apply health modifier
            float healthModifier = plantInstance.Health / 100f;
            
            // Apply growth stage modifier
            float stageModifier = GetStageYieldModifier(plantInstance.CurrentGrowthStage);
            
            // Apply environmental stress modifier
            float stressModifier = 1f - (plantInstance.StressLevel / 100f);
            
            // Apply global growth modifier
            float globalModifier = _globalGrowthModifier;
            
            // Calculate final expected yield
            float expectedYield = baseYield * healthModifier * stageModifier * stressModifier * globalModifier;
            
            // Apply harvest quality multiplier if enabled
            if (_enableYieldVariability)
            {
                expectedYield *= _harvestQualityMultiplier;
            }
            
            return Mathf.Max(0f, expectedYield);
        }
        
        /// <summary>
        /// Get yield modifier based on growth stage
        /// PC013-4: Refactored to use specialized YieldCalculationService
        /// </summary>
        private float GetStageYieldModifier(PlantGrowthStage stage)
        {
            // PC013-4: Delegate to specialized yield calculation service
            if (_yieldService != null)
            {
                return _yieldService.GetStageYieldModifier(stage);
            }
            
            // Fallback to legacy implementation if service is not available
            return GetStageYieldModifierLegacy(stage);
        }
        
        /// <summary>
        /// Legacy stage yield modifier implementation for fallback
        /// </summary>
        private float GetStageYieldModifierLegacy(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seed => 0f,
                PlantGrowthStage.Germination => 0f,
                PlantGrowthStage.Seedling => 0.1f,
                PlantGrowthStage.Vegetative => 0.3f,
                PlantGrowthStage.PreFlowering => 0.5f,
                PlantGrowthStage.Flowering => 0.8f,
                PlantGrowthStage.Ripening => 0.95f,
                PlantGrowthStage.Harvest => 1f,
                PlantGrowthStage.Harvestable => 1f,
                PlantGrowthStage.Harvested => 0f,
                PlantGrowthStage.Drying => 0f,
                PlantGrowthStage.Curing => 0f,
                _ => 0.5f // Default fallback
            };
        }
        
        // Phase 3.1: Advanced Genetics Integration Methods
        
        /// <summary>
        /// Get genetic performance statistics for monitoring and optimization.
        /// </summary>
        public GeneticPerformanceStats GetGeneticPerformanceStats()
        {
            if (_geneticPerformanceMonitor == null)
            {
                return new GeneticPerformanceStats
                {
                    TotalCalculations = 0,
                    AverageCalculationTimeMs = 0.0,
                    CacheHitRatio = 0.0,
                    BatchCalculations = 0,
                    AverageBatchTimeMs = 0.0,
                    AverageUpdateTimeMs = 0.0
                };
            }
            
            var geneticsStats = _geneticPerformanceMonitor.GetPerformanceStats();
            // Convert GeneticsPerformanceStats to GeneticPerformanceStats
            return new GeneticPerformanceStats
            {
                TotalCalculations = geneticsStats.TotalCalculations,
                AverageCalculationTimeMs = geneticsStats.AverageCalculationTimeMs,
                CacheHitRatio = geneticsStats.CacheHitRatio,
                BatchCalculations = geneticsStats.BatchCalculations,
                AverageBatchTimeMs = geneticsStats.AverageBatchTimeMs,
                AverageUpdateTimeMs = 0.0 // Default value since this might not exist in GeneticsPerformanceStats
            };
        }
        
        /// <summary>
        /// Enable or disable advanced genetics during runtime.
        /// </summary>
        public void SetAdvancedGeneticsEnabled(bool enabled)
        {
            if (_enableAdvancedGenetics != enabled)
            {
                _enableAdvancedGenetics = enabled;
                
                // Reinitialize update processor with new setting
                _updateProcessor = new PlantUpdateProcessor(_enableStressSystem, _enableGxEInteractions, _enableAdvancedGenetics);
                
                LogInfo($"Advanced genetics {(enabled ? "enabled" : "disabled")}");
            }
        }
        
        /// <summary>
        /// Get comprehensive statistics including genetic performance data.
        /// </summary>
        public EnhancedPlantManagerStatistics GetEnhancedStatistics()
        {
            var basicStats = GetStatistics();
            var enhancedStats = new EnhancedPlantManagerStatistics
            {
                // Copy basic statistics
                TotalPlants = basicStats.TotalPlants,
                PlantsByStage = basicStats.PlantsByStage,
                AverageHealth = basicStats.AverageHealth,
                AverageStress = basicStats.AverageStress,
                UnhealthyPlants = basicStats.UnhealthyPlants,
                HighStressPlants = basicStats.HighStressPlants,
                
                // Add genetic statistics
                AdvancedGeneticsEnabled = _enableAdvancedGenetics,
                GeneticStats = GetGeneticPerformanceStats()
            };
            
            // Calculate genetic diversity statistics
            if (_enableAdvancedGenetics)
            {
                enhancedStats.GeneticDiversityStats = CalculateGeneticDiversityStats();
            }
            
            return enhancedStats;
        }
        
        /// <summary>
        /// Calculate genetic diversity statistics across all plants - uses CultivationManager data.
        /// </summary>
        private GeneticDiversityStats CalculateGeneticDiversityStats()
        {
            var stats = new GeneticDiversityStats();
            
            if (_cultivationManager == null)
            {
                LogWarning("Cannot calculate genetic diversity: CultivationManager is null");
                return stats;
            }
            
            var strainCounts = new Dictionary<string, int>();
            var allPlants = _cultivationManager.GetAllPlants();
            
            foreach (var plant in allPlants)
            {
                if (plant != null)
                {
                    // Count strain diversity based on PlantInstanceSO data
                    var strainName = plant.StrainName ?? "Unknown";
                    strainCounts[strainName] = strainCounts.GetValueOrDefault(strainName, 0) + 1;
                }
            }
            
            stats.StrainDiversity = strainCounts.Count;
            stats.MostCommonStrain = strainCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "None";
            
            // Note: TraitExpression data may not be available in PlantInstanceSO
            // This would need to be implemented when advanced genetics are integrated
            stats.AverageGeneticFitness = 1.0f; // Default value
            stats.TraitExpressionVariance = 0.0f; // Default value
            
            return stats;
        }
        
        /// <summary>
        /// Calculate variance in trait expression across plants.
        /// </summary>
        private float CalculateTraitVariance(List<TraitExpressionResult> traitExpressions)
        {
            if (traitExpressions.Count == 0)
                return 0f;
            
            var heightVariances = new List<float>();
            var thcVariances = new List<float>();
            var cbdVariances = new List<float>();
            var yieldVariances = new List<float>();
            
            foreach (var expression in traitExpressions)
            {
                heightVariances.Add(expression.HeightExpression);
                thcVariances.Add(expression.THCExpression);
                cbdVariances.Add(expression.CBDExpression);
                yieldVariances.Add(expression.YieldExpression);
            }
            
            // Calculate combined variance across all traits
            float heightVar = CalculateVariance(heightVariances);
            float thcVar = CalculateVariance(thcVariances);
            float cbdVar = CalculateVariance(cbdVariances);
            float yieldVar = CalculateVariance(yieldVariances);
            
            return (heightVar + thcVar + cbdVar + yieldVar) / 4f;
        }
        
        /// <summary>
        /// Calculate statistical variance for a list of values.
        /// </summary>
        private float CalculateVariance(List<float> values)
        {
            if (values.Count == 0)
                return 0f;
            
            float mean = values.Average();
            float variance = values.Sum(v => Mathf.Pow(v - mean, 2)) / values.Count;
            return variance;
        }
    }
    
    // Note: Common types moved to IPlantService.cs to avoid duplication
}