using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-2: Refactored Cultivation Manager - Orchestrates modular cultivation components
    /// Replaced monolithic 515-line class with composition-based architecture
    /// Adheres to Single Responsibility Principle and Dependency Injection patterns
    /// </summary>
    public class RefactoredCultivationManager : ChimeraManager
    {
        [Header("Cultivation Manager Configuration")]
        [SerializeField] private bool _enableCultivationSystem = true;
        
        // Modular components
        private IPlantLifecycleManager _plantLifecycleManager;
        private IPlantCareManager _plantCareManager;
        private IEnvironmentalManager _environmentalManager;
        private IGrowthProcessor _growthProcessor;
        private IHarvestManager _harvestManager;
        
        public string ManagerName => "Refactored Cultivation Manager";
        
        // Delegate properties to components
        public int ActivePlantCount => _plantLifecycleManager?.ActivePlantCount ?? 0;
        public int TotalPlantsGrown => _plantLifecycleManager?.TotalPlantsGrown ?? 0;
        public int TotalPlantsHarvested => _plantLifecycleManager?.TotalPlantsHarvested ?? 0;
        public float TotalYieldHarvested => _plantLifecycleManager?.TotalYieldHarvested ?? 0f;
        public float AveragePlantHealth => _growthProcessor?.AveragePlantHealth ?? 0f;
        public bool EnableAutoGrowth 
        { 
            get => _growthProcessor?.EnableAutoGrowth ?? false; 
            set { if (_growthProcessor != null) _growthProcessor.EnableAutoGrowth = value; }
        }
        public float TimeAcceleration 
        { 
            get => _growthProcessor?.TimeAcceleration ?? 1f; 
            set { if (_growthProcessor != null) _growthProcessor.TimeAcceleration = value; }
        }
        
        protected override void OnManagerInitialize()
        {
            Debug.Log("[RefactoredCultivationManager] Initializing modular cultivation system...");
            
            if (!_enableCultivationSystem)
            {
                Debug.Log("[RefactoredCultivationManager] Cultivation system disabled.");
                return;
            }
            
            // Initialize components with dependency injection
            InitializeComponents();
            
            // Register with GameManager
            GameManager.Instance?.RegisterManager(this);
            
            Debug.Log("[RefactoredCultivationManager] Modular cultivation system initialized successfully.");
        }
        
        protected override void OnManagerShutdown()
        {
            Debug.Log("[RefactoredCultivationManager] Shutting down modular cultivation system...");
            
            // Shutdown components in reverse order
            ShutdownComponents();
            
            Debug.Log("[RefactoredCultivationManager] Modular cultivation system shutdown complete.");
        }
        
        protected override void Update()
        {
            if (!IsInitialized || !_enableCultivationSystem) return;
            
            // Update growth processor (handles timing and auto-growth)
            if (_growthProcessor is GrowthProcessor processor)
            {
                processor.Update();
            }
        }
        
        private void InitializeComponents()
        {
            // Initialize in dependency order
            _plantLifecycleManager = new PlantLifecycleManager(null, null); // Will set dependencies after creation
            _environmentalManager = new CultivationEnvironmentalManager(_plantLifecycleManager);
            _harvestManager = new HarvestManager(_plantLifecycleManager);
            _plantCareManager = new PlantCareManager(_plantLifecycleManager);
            _growthProcessor = new GrowthProcessor(_plantLifecycleManager, _environmentalManager);
            
            // Update dependencies for PlantLifecycleManager
            if (_plantLifecycleManager is PlantLifecycleManager lifecycleManager)
            {
                // Set dependencies via constructor replacement or setter injection
                // For now, we'll use a temporary approach - in full DI implementation, this would be handled by container
                System.Reflection.FieldInfo envField = typeof(PlantLifecycleManager).GetField("_environmentalManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Reflection.FieldInfo harvestField = typeof(PlantLifecycleManager).GetField("_harvestManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                envField?.SetValue(lifecycleManager, _environmentalManager);
                harvestField?.SetValue(lifecycleManager, _harvestManager);
            }
            
            // Initialize all components
            _plantLifecycleManager.Initialize();
            _environmentalManager.Initialize();
            _harvestManager.Initialize();
            _plantCareManager.Initialize();
            _growthProcessor.Initialize();
            
            Debug.Log("[RefactoredCultivationManager] All components initialized successfully.");
        }
        
        private void ShutdownComponents()
        {
            // Shutdown in reverse order
            _growthProcessor?.Shutdown();
            _plantCareManager?.Shutdown();
            _harvestManager?.Shutdown();
            _environmentalManager?.Shutdown();
            _plantLifecycleManager?.Shutdown();
            
            Debug.Log("[RefactoredCultivationManager] All components shutdown successfully.");
        }
        
        #region Public API - Delegates to appropriate components
        
        /// <summary>
        /// Plants a new plant instance in the cultivation system.
        /// </summary>
        public PlantInstanceSO PlantSeed(string plantName, PlantStrainSO strain, GenotypeDataSO genotype, Vector3 position, string zoneId = "default")
        {
            return _plantLifecycleManager?.PlantSeed(plantName, strain, genotype, position, zoneId);
        }
        
        /// <summary>
        /// Removes a plant from the cultivation system.
        /// </summary>
        public bool RemovePlant(string plantId, bool isHarvest = false)
        {
            return _plantLifecycleManager?.RemovePlant(plantId, isHarvest) ?? false;
        }
        
        /// <summary>
        /// Gets a plant instance by its ID.
        /// </summary>
        public PlantInstanceSO GetPlant(string plantId)
        {
            return _plantLifecycleManager?.GetPlant(plantId);
        }
        
        /// <summary>
        /// Gets all active plants.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            return _plantLifecycleManager?.GetAllPlants() ?? new PlantInstanceSO[0];
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PlantInstanceSO> GetPlantsByStage(PlantGrowthStage stage)
        {
            return _plantLifecycleManager?.GetPlantsByStage(stage) ?? new PlantInstanceSO[0];
        }
        
        /// <summary>
        /// Gets all plants that need attention.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            return _plantLifecycleManager?.GetPlantsNeedingAttention() ?? new PlantInstanceSO[0];
        }
        
        /// <summary>
        /// Waters a specific plant.
        /// </summary>
        public bool WaterPlant(string plantId, float waterAmount = 0.5f)
        {
            return _plantCareManager?.WaterPlant(plantId, waterAmount) ?? false;
        }
        
        /// <summary>
        /// Feeds nutrients to a specific plant.
        /// </summary>
        public bool FeedPlant(string plantId, float nutrientAmount = 0.4f)
        {
            return _plantCareManager?.FeedPlant(plantId, nutrientAmount) ?? false;
        }
        
        /// <summary>
        /// Applies training to a specific plant.
        /// </summary>
        public bool TrainPlant(string plantId, string trainingType)
        {
            return _plantCareManager?.TrainPlant(plantId, trainingType) ?? false;
        }
        
        /// <summary>
        /// Waters all plants in the cultivation system.
        /// </summary>
        public void WaterAllPlants(float waterAmount = 0.5f)
        {
            _plantCareManager?.WaterAllPlants(waterAmount);
        }
        
        /// <summary>
        /// Feeds all plants in the cultivation system.
        /// </summary>
        public void FeedAllPlants(float nutrientAmount = 0.4f)
        {
            _plantCareManager?.FeedAllPlants(nutrientAmount);
        }
        
        /// <summary>
        /// Updates environmental conditions for a specific zone.
        /// </summary>
        public void SetZoneEnvironment(string zoneId, ProjectChimera.Data.Environment.EnvironmentalConditions environment)
        {
            _environmentalManager?.SetZoneEnvironment(zoneId, environment);
        }
        
        /// <summary>
        /// Gets environmental conditions for a specific zone.
        /// </summary>
        public ProjectChimera.Data.Environment.EnvironmentalConditions GetZoneEnvironment(string zoneId)
        {
            return _environmentalManager?.GetZoneEnvironment(zoneId) ?? new ProjectChimera.Data.Environment.EnvironmentalConditions();
        }
        
        /// <summary>
        /// Processes daily growth for all active plants.
        /// </summary>
        public void ProcessDailyGrowthForAllPlants()
        {
            _growthProcessor?.ProcessDailyGrowthForAllPlants();
        }
        
        /// <summary>
        /// Forces an immediate growth update for testing purposes.
        /// </summary>
        public void ForceGrowthUpdate()
        {
            _growthProcessor?.ForceGrowthUpdate();
        }
        
        /// <summary>
        /// Harvests a plant by ID
        /// </summary>
        public bool HarvestPlant(string plantId)
        {
            return _harvestManager?.HarvestPlant(plantId) ?? false;
        }
        
        /// <summary>
        /// Gets cultivation statistics.
        /// </summary>
        public (int active, int grown, int harvested, float yield, float avgHealth) GetCultivationStats()
        {
            return (ActivePlantCount, TotalPlantsGrown, TotalPlantsHarvested, TotalYieldHarvested, AveragePlantHealth);
        }
        
        #endregion
        
        #region Component Access (for advanced usage)
        
        /// <summary>
        /// Gets the plant lifecycle manager component
        /// </summary>
        public IPlantLifecycleManager GetPlantLifecycleManager() => _plantLifecycleManager;
        
        /// <summary>
        /// Gets the plant care manager component
        /// </summary>
        public IPlantCareManager GetPlantCareManager() => _plantCareManager;
        
        /// <summary>
        /// Gets the environmental manager component
        /// </summary>
        public IEnvironmentalManager GetEnvironmentalManager() => _environmentalManager;
        
        /// <summary>
        /// Gets the growth processor component
        /// </summary>
        public IGrowthProcessor GetGrowthProcessor() => _growthProcessor;
        
        /// <summary>
        /// Gets the harvest manager component
        /// </summary>
        public IHarvestManager GetHarvestManager() => _harvestManager;
        
        #endregion
    }
}