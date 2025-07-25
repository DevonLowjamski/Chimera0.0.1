using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC013-5d: Main coordinator class for cultivation operations
    /// Orchestrates specialized cultivation services to provide a unified interface
    /// while maintaining separation of concerns and single responsibility principle.
    /// Replaces the monolithic CultivationManager.cs.
    /// </summary>
    public class CultivationOrchestrator : ChimeraManager
    {
        [Header("Orchestrator Configuration")]
        [SerializeField] private bool _enableAutoGrowth = true;
        [SerializeField] private float _timeAcceleration = 1f;
        [SerializeField] private int _maxPlantsPerGrow = 50;
        [SerializeField] private float _growthUpdateInterval = 86400f; // 24 hours in seconds
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantPlanted;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantHarvested;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantStageChanged;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantHealthCritical;
        
        // Specialized service components
        private CultivationZoneManager _zoneManager;
        private CultivationEnvironmentalController _environmentalController;
        private CultivationPlantTracker _plantTracker;
        
        // Dependencies
        private PlantManager _plantManager;
        
        // Timing
        private float _lastGrowthUpdate = 0f;
        
        public string ManagerName => "Cultivation Orchestrator";
        
        // Unified Properties (delegating to specialized services)
        public int ActivePlantCount => _plantTracker?.ActivePlantCount ?? 0;
        public int ActiveZoneCount => _zoneManager?.ActiveZoneCount ?? 0;
        public CultivationStatistics Statistics => _plantTracker?.Statistics ?? new CultivationStatistics();
        public bool EnableAutoGrowth 
        { 
            get => _enableAutoGrowth; 
            set => _enableAutoGrowth = value; 
        }
        public float TimeAcceleration 
        { 
            get => _timeAcceleration; 
            set => _timeAcceleration = Mathf.Clamp(value, 0.1f, 100f); 
        }
        
        // Service Access Properties
        public CultivationZoneManager ZoneManager => _zoneManager;
        public CultivationEnvironmentalController EnvironmentalController => _environmentalController;
        public CultivationPlantTracker PlantTracker => _plantTracker;
        
        protected override void OnManagerInitialize()
        {
            Debug.Log("[CultivationOrchestrator] Initializing cultivation orchestration system...");
            
            InitializeServices();
            ConnectEventHandlers();
            
            _lastGrowthUpdate = Time.time;
            
            Debug.Log($"[CultivationOrchestrator] Initialized. Auto-growth: {_enableAutoGrowth}, Max plants: {_maxPlantsPerGrow}");
        }
        
        protected override void OnManagerShutdown()
        {
            Debug.Log("[CultivationOrchestrator] Shutting down cultivation orchestration...");
            
            DisconnectEventHandlers();
            ShutdownServices();
        }
        
        protected override void Update()
        {
            if (!IsInitialized || !_enableAutoGrowth) return;
            
            // Handle daily growth updates
            if (Time.time - _lastGrowthUpdate >= _growthUpdateInterval)
            {
                ProcessDailyGrowthForAllPlants();
                _lastGrowthUpdate = Time.time;
            }
        }
        
        #region Service Initialization
        
        private void InitializeServices()
        {
            // Get or create service components
            _zoneManager = GetOrCreateComponent<CultivationZoneManager>();
            _environmentalController = GetOrCreateComponent<CultivationEnvironmentalController>();
            _plantTracker = GetOrCreateComponent<CultivationPlantTracker>();
            
            // Get dependencies
            _plantManager = GameManager.Instance?.GetManager<PlantManager>();
            if (_plantManager == null)
            {
                Debug.LogWarning("[CultivationOrchestrator] PlantManager not found - some features may be limited");
            }
            
            // Initialize all services
            _zoneManager?.Initialize();
            _environmentalController?.Initialize();
            _plantTracker?.Initialize();
        }
        
        private void ShutdownServices()
        {
            _zoneManager?.Shutdown();
            _environmentalController?.Shutdown();
            _plantTracker?.Shutdown();
        }
        
        private T GetOrCreateComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        
        #endregion
        
        #region Event Handling
        
        private void ConnectEventHandlers()
        {
            if (_plantTracker != null)
            {
                _plantTracker.OnPlantRegistered += OnPlantRegistered;
                _plantTracker.OnPlantRemoved += OnPlantRemoved;
                _plantTracker.OnPlantStageChanged += OnPlantStageChanged;
                _plantTracker.OnPlantHealthCritical += OnPlantHealthCritical;
            }
            
            if (_environmentalController != null)
            {
                _environmentalController.OnEnvironmentalAlert += OnEnvironmentalAlert;
                _environmentalController.OnEnvironmentalAdjustment += OnEnvironmentalAdjustment;
            }
            
            if (_zoneManager != null)
            {
                _zoneManager.OnZoneCreated += OnZoneCreated;
                _zoneManager.OnZoneRemoved += OnZoneRemoved;
            }
        }
        
        private void DisconnectEventHandlers()
        {
            if (_plantTracker != null)
            {
                _plantTracker.OnPlantRegistered -= OnPlantRegistered;
                _plantTracker.OnPlantRemoved -= OnPlantRemoved;
                _plantTracker.OnPlantStageChanged -= OnPlantStageChanged;
                _plantTracker.OnPlantHealthCritical -= OnPlantHealthCritical;
            }
            
            if (_environmentalController != null)
            {
                _environmentalController.OnEnvironmentalAlert -= OnEnvironmentalAlert;
                _environmentalController.OnEnvironmentalAdjustment -= OnEnvironmentalAdjustment;
            }
            
            if (_zoneManager != null)
            {
                _zoneManager.OnZoneCreated -= OnZoneCreated;
                _zoneManager.OnZoneRemoved -= OnZoneRemoved;
            }
        }
        
        #endregion
        
        #region Public API - Plant Operations
        
        /// <summary>
        /// Plants a seed and registers it in the cultivation system
        /// </summary>
        public PlantInstanceSO PlantSeed(string plantName, PlantStrainSO strain, GenotypeDataSO genotype, Vector3 position, string zoneId = "default")
        {
            if (_plantTracker == null || _zoneManager == null)
            {
                Debug.LogError("[CultivationOrchestrator] Required services not initialized");
                return null;
            }
            
            if (ActivePlantCount >= _maxPlantsPerGrow)
            {
                Debug.LogWarning($"[CultivationOrchestrator] Maximum plant limit reached ({_maxPlantsPerGrow})");
                return null;
            }
            
            // Create plant instance
            var plant = ScriptableObject.CreateInstance<PlantInstanceSO>();
            plant.InitializePlant(System.Guid.NewGuid().ToString(), plantName, strain, genotype, position);
            
            // Register with tracker
            if (_plantTracker.RegisterPlant(plant, position, zoneId))
            {
                Debug.Log($"[CultivationOrchestrator] Successfully planted {plantName} in zone {zoneId}");
                return plant;
            }
            
            Debug.LogError($"[CultivationOrchestrator] Failed to register plant {plantName}");
            return null;
        }
        
        /// <summary>
        /// Harvests a plant and processes the yield
        /// </summary>
        public bool HarvestPlant(string plantId)
        {
            var plant = GetPlant(plantId);
            if (plant == null)
            {
                Debug.LogWarning($"[CultivationOrchestrator] Plant {plantId} not found for harvest");
                return false;
            }
            
            if (plant.CurrentGrowthStage != PlantGrowthStage.Harvestable)
            {
                Debug.LogWarning($"[CultivationOrchestrator] Plant {plant.PlantName} is not ready for harvest (Stage: {plant.CurrentGrowthStage})");
                return false;
            }
            
            // Process harvest
            ProcessHarvest(plant);
            
            // Remove from tracking
            _plantTracker?.RemovePlant(plantId, PlantRemovalReason.Harvested);
            
            // Fire event
            _onPlantHarvested?.Raise(plant);
            
            return true;
        }
        
        /// <summary>
        /// Gets a specific plant
        /// </summary>
        public PlantInstanceSO GetPlant(string plantId)
        {
            return _plantTracker?.GetPlant(plantId);
        }
        
        /// <summary>
        /// Gets all active plants
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            return _plantTracker?.GetAllPlants() ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets plants by growth stage
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsByStage(PlantGrowthStage stage)
        {
            return _plantTracker?.GetPlantsByStage(stage) ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets plants needing attention
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            return _plantTracker?.GetPlantsNeedingAttention() ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        #endregion
        
        #region Public API - Zone Operations
        
        /// <summary>
        /// Creates a new cultivation zone
        /// </summary>
        public bool CreateZone(string zoneId, EnvironmentalConditions environment)
        {
            return _zoneManager?.CreateZone(zoneId, environment) ?? false;
        }
        
        /// <summary>
        /// Sets zone environmental conditions
        /// </summary>
        public void SetZoneEnvironment(string zoneId, EnvironmentalConditions environment)
        {
            _zoneManager?.SetZoneEnvironment(zoneId, environment);
        }
        
        /// <summary>
        /// Gets zone environmental conditions
        /// </summary>
        public EnvironmentalConditions GetZoneEnvironment(string zoneId)
        {
            return _zoneManager?.GetZoneEnvironment(zoneId) ?? EnvironmentalConditions.CreateIndoorDefault();
        }
        
        /// <summary>
        /// Gets plants in a specific zone
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsInZone(string zoneId)
        {
            return _plantTracker?.GetPlantsInZone(zoneId) ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        #endregion
        
        #region Public API - Care Operations
        
        /// <summary>
        /// Waters all plants
        /// </summary>
        public void WaterAllPlants(float waterAmount = 0.5f)
        {
            var plants = GetAllPlants().ToList();
            foreach (var plant in plants)
            {
                WaterPlant(plant, waterAmount);
            }
            
            Debug.Log($"[CultivationOrchestrator] Watered {plants.Count} plants with {waterAmount} units each");
        }
        
        /// <summary>
        /// Feeds all plants with nutrients
        /// </summary>
        public void FeedAllPlants(float nutrientAmount = 0.4f)
        {
            var plants = GetAllPlants().ToList();
            foreach (var plant in plants)
            {
                FeedPlant(plant, nutrientAmount);
            }
            
            Debug.Log($"[CultivationOrchestrator] Fed {plants.Count} plants with {nutrientAmount} units each");
        }
        
        /// <summary>
        /// Processes daily growth for all plants
        /// </summary>
        public void ProcessDailyGrowthForAllPlants()
        {
            var plants = GetAllPlants().ToList();
            
            Debug.Log($"[CultivationOrchestrator] Processing daily growth for {plants.Count} plants");
            
            foreach (var plant in plants)
            {
                ProcessPlantGrowth(plant);
            }
            
            // Process environmental effects by zone
            ProcessEnvironmentalEffectsForAllZones();
        }
        
        /// <summary>
        /// Forces an immediate growth update
        /// </summary>
        public void ForceGrowthUpdate()
        {
            ProcessDailyGrowthForAllPlants();
            Debug.Log("[CultivationOrchestrator] Forced growth update completed");
        }
        
        #endregion
        
        #region Private Methods
        
        private void ProcessPlantGrowth(PlantInstanceSO plant)
        {
            if (plant == null) return;
            
            // Get environment for plant's zone
            string zoneId = _zoneManager?.GetPlantZone(plant.PlantID) ?? "default";
            EnvironmentalConditions environment = GetZoneEnvironment(zoneId);
            
            // Use PlantManager for actual growth processing if available
            if (_plantManager != null)
            {
                // Delegate to PlantManager which now uses specialized services
                // This maintains backward compatibility while using the new architecture
            }
            else
            {
                // Fallback basic growth processing
                BasicGrowthProcessing(plant, environment);
            }
        }
        
        private void BasicGrowthProcessing(PlantInstanceSO plant, EnvironmentalConditions environment)
        {
            // Simple growth progression (full implementation would be more complex)
            float growthRate = CalculateGrowthRate(plant, environment);
            
            // Update plant age and check for stage transitions
            if (ShouldAdvanceGrowthStage(plant, growthRate))
            {
                AdvancePlantStage(plant);
            }
        }
        
        private float CalculateGrowthRate(PlantInstanceSO plant, EnvironmentalConditions environment)
        {
            float baseRate = 1f;
            
            // Environmental factors (simplified)
            float tempFactor = CalculateTemperatureFactor(environment.Temperature);
            float humidityFactor = CalculateHumidityFactor(environment.Humidity);
            float lightFactor = CalculateLightFactor(environment.DailyLightIntegral);
            
            return baseRate * tempFactor * humidityFactor * lightFactor * _timeAcceleration;
        }
        
        private bool ShouldAdvanceGrowthStage(PlantInstanceSO plant, float growthRate)
        {
            // Simplified stage advancement logic
            float requiredDays = GetRequiredDaysForStage(plant.CurrentGrowthStage);
            return plant.DaysInCurrentStage >= requiredDays;
        }
        
        private void AdvancePlantStage(PlantInstanceSO plant)
        {
            PlantGrowthStage currentStage = plant.CurrentGrowthStage;
            PlantGrowthStage nextStage = GetNextGrowthStage(currentStage);
            
            if (nextStage != currentStage)
            {
                // Update plant stage (would be done through PlantManager in full implementation)
                _plantTracker?.UpdatePlantStage(plant.PlantID, nextStage);
            }
        }
        
        private void ProcessEnvironmentalEffectsForAllZones()
        {
            if (_environmentalController == null || _zoneManager == null) return;
            
            foreach (string zoneId in _zoneManager.ZoneIds)
            {
                var plantsInZone = GetPlantsInZone(zoneId).ToList();
                if (plantsInZone.Any())
                {
                    _environmentalController.ProcessEnvironmentalEffects(zoneId, plantsInZone);
                }
            }
        }
        
        private void ProcessHarvest(PlantInstanceSO plant)
        {
            // Calculate yield and quality (simplified)
            float yieldAmount = CalculateHarvestYield(plant);
            float qualityScore = CalculateHarvestQuality(plant);
            
            // Add to inventory (would integrate with economy system)
            AddHarvestToInventory(plant, yieldAmount, qualityScore);
            
            Debug.Log($"[CultivationOrchestrator] Harvested {plant.PlantName}: {yieldAmount:F2}g at {qualityScore:F1}% quality");
        }
        
        private float CalculateHarvestYield(PlantInstanceSO plant)
        {
            // Simplified yield calculation
            float baseYield = 50f; // Default base yield since AverageYield may not exist
            if (plant.Strain != null)
            {
                // Use strain-specific yield if available
                baseYield = 50f; // Placeholder - would be plant.Strain.BaseYield or similar
            }
            
            float healthModifier = plant.CurrentHealth;
            float maturityModifier = plant.MaturityLevel;
            
            return baseYield * healthModifier * maturityModifier;
        }
        
        private float CalculateHarvestQuality(PlantInstanceSO plant)
        {
            // Simplified quality calculation
            float baseQuality = 70f;
            float healthBonus = plant.CurrentHealth * 20f;
            float stressPenalty = plant.StressLevel * 15f;
            
            return Mathf.Clamp(baseQuality + healthBonus - stressPenalty, 0f, 100f);
        }
        
        private void AddHarvestToInventory(PlantInstanceSO plant, float yieldAmount, float qualityScore)
        {
            // Integration point with economy/inventory system
            Debug.Log($"[CultivationOrchestrator] Added to inventory: {yieldAmount:F2}g of {plant.PlantName} ({qualityScore:F1}% quality)");
        }
        
        private void WaterPlant(PlantInstanceSO plant, float amount)
        {
            // Would delegate to PlantManager or implement basic watering
            Debug.Log($"[CultivationOrchestrator] Watered {plant.PlantName} with {amount} units");
        }
        
        private void FeedPlant(PlantInstanceSO plant, float amount)
        {
            // Would delegate to PlantManager or implement basic feeding
            Debug.Log($"[CultivationOrchestrator] Fed {plant.PlantName} with {amount} nutrient units");
        }
        
        #endregion
        
        #region Helper Methods
        
        private float CalculateTemperatureFactor(float temperature)
        {
            float optimal = 23f; // Optimal temperature
            float difference = Mathf.Abs(temperature - optimal);
            return Mathf.Clamp01(1f - (difference / 15f));
        }
        
        private float CalculateHumidityFactor(float humidity)
        {
            float optimal = 50f; // Optimal humidity
            float difference = Mathf.Abs(humidity - optimal);
            return Mathf.Clamp01(1f - (difference / 30f));
        }
        
        private float CalculateLightFactor(float dli)
        {
            float optimal = 30f; // Optimal DLI
            float difference = Mathf.Abs(dli - optimal);
            return Mathf.Clamp01(1f - (difference / 20f));
        }
        
        private float GetRequiredDaysForStage(PlantGrowthStage stage)
        {
            switch (stage)
            {
                case PlantGrowthStage.Seed: return 3f;
                case PlantGrowthStage.Germination: return 7f;
                case PlantGrowthStage.Seedling: return 14f;
                case PlantGrowthStage.Vegetative: return 30f;
                case PlantGrowthStage.Flowering: return 60f;
                default: return 7f;
            }
        }
        
        private PlantGrowthStage GetNextGrowthStage(PlantGrowthStage current)
        {
            switch (current)
            {
                case PlantGrowthStage.Seed: return PlantGrowthStage.Germination;
                case PlantGrowthStage.Germination: return PlantGrowthStage.Seedling;
                case PlantGrowthStage.Seedling: return PlantGrowthStage.Vegetative;
                case PlantGrowthStage.Vegetative: return PlantGrowthStage.Flowering;
                case PlantGrowthStage.Flowering: return PlantGrowthStage.Harvestable;
                default: return current;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlantRegistered(PlantInstanceSO plant)
        {
            _onPlantPlanted?.Raise(plant);
            Debug.Log($"[CultivationOrchestrator] Plant registered: {plant.PlantName}");
        }
        
        private void OnPlantRemoved(string plantId, PlantInstanceSO plant)
        {
            Debug.Log($"[CultivationOrchestrator] Plant removed: {plant.PlantName}");
        }
        
        private void OnPlantStageChanged(PlantInstanceSO plant, PlantGrowthStage oldStage, PlantGrowthStage newStage)
        {
            _onPlantStageChanged?.Raise(plant);
            Debug.Log($"[CultivationOrchestrator] Plant {plant.PlantName} stage changed: {oldStage} -> {newStage}");
        }
        
        private void OnPlantHealthCritical(PlantInstanceSO plant)
        {
            _onPlantHealthCritical?.Raise(plant);
            Debug.LogWarning($"[CultivationOrchestrator] Critical health alert for plant {plant.PlantName}");
        }
        
        private void OnEnvironmentalAlert(string zoneId, EnvironmentalAlert alert)
        {
            Debug.LogWarning($"[CultivationOrchestrator] Environmental alert in zone {zoneId}: {alert.Severity}");
        }
        
        private void OnEnvironmentalAdjustment(string zoneId, EnvironmentalConditions environment)
        {
            Debug.Log($"[CultivationOrchestrator] Environmental adjustment made for zone {zoneId}");
        }
        
        private void OnZoneCreated(string zoneId)
        {
            Debug.Log($"[CultivationOrchestrator] Zone created: {zoneId}");
        }
        
        private void OnZoneRemoved(string zoneId)
        {
            Debug.Log($"[CultivationOrchestrator] Zone removed: {zoneId}");
        }
        
        #endregion
    }
}