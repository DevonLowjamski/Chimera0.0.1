using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-2: Growth Processor - Handles automated plant growth and time management
    /// Extracted from monolithic CultivationManager for Single Responsibility Principle
    /// </summary>
    public class GrowthProcessor : IGrowthProcessor
    {
        [Header("Growth Processing Configuration")]
        [SerializeField] private bool _enableAutoGrowth = true;
        [SerializeField] private float _timeAcceleration = 1f;
        
        // Time management
        private float _lastGrowthUpdate = 0f;
        private const float GROWTH_UPDATE_INTERVAL = 86400f; // 24 hours in seconds (real-time)
        
        // Statistics
        private float _averagePlantHealth = 0f;
        
        // Dependencies
        private IPlantLifecycleManager _plantLifecycleManager;
        private IEnvironmentalManager _environmentalManager;
        
        public bool IsInitialized { get; private set; }
        
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
        
        public float AveragePlantHealth => _averagePlantHealth;
        
        public GrowthProcessor(IPlantLifecycleManager plantLifecycleManager, IEnvironmentalManager environmentalManager)
        {
            _plantLifecycleManager = plantLifecycleManager;
            _environmentalManager = environmentalManager;
        }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("[GrowthProcessor] Initializing growth processing...");
            
            _lastGrowthUpdate = Time.time;
            _averagePlantHealth = 0f;
            
            IsInitialized = true;
            Debug.Log($"[GrowthProcessor] Initialized. Auto-growth: {_enableAutoGrowth}, Time acceleration: {_timeAcceleration}x");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("[GrowthProcessor] Shutting down growth processing...");
            IsInitialized = false;
        }
        
        /// <summary>
        /// Updates growth processing - should be called from main manager's Update
        /// </summary>
        public void Update()
        {
            if (!IsInitialized || !_enableAutoGrowth) return;
            
            // Check if it's time for daily growth update
            float timeSinceLastUpdate = Time.time - _lastGrowthUpdate;
            float adjustedInterval = GROWTH_UPDATE_INTERVAL / _timeAcceleration;
            
            if (timeSinceLastUpdate >= adjustedInterval)
            {
                ProcessDailyGrowthForAllPlants();
                _lastGrowthUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// Processes daily growth for all active plants.
        /// </summary>
        public void ProcessDailyGrowthForAllPlants()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[GrowthProcessor] Cannot process growth: Manager not initialized.");
                return;
            }
            
            var allPlants = _plantLifecycleManager.GetAllPlants();
            int plantCount = _plantLifecycleManager.ActivePlantCount;
            
            Debug.Log($"[GrowthProcessor] Processing daily growth for {plantCount} plants...");
            
            List<string> plantsToRemove = new List<string>();
            float totalHealth = 0f;
            int healthyPlants = 0;
            
            foreach (var plant in allPlants)
            {
                string plantId = plant.PlantID;
                
                // Get environment for this plant
                var environmentData = _environmentalManager.GetEnvironmentForPlant(plantId);
                
                // Convert to cultivation environmental conditions
                var environment = ConvertToCultivationEnvironmentalConditions(environmentData);
                
                // Track health before growth
                PlantGrowthStage previousStage = plant.CurrentGrowthStage;
                
                // Process daily growth
                plant.ProcessDailyGrowth(environment, _timeAcceleration);
                
                // Check for stage transition
                if (plant.CurrentGrowthStage != previousStage)
                {
                    Debug.Log($"[GrowthProcessor] Plant '{plantId}' transitioned from {previousStage} to {plant.CurrentGrowthStage}");
                    
                    // Use PlantLifecycleManager to trigger event
                    if (_plantLifecycleManager is PlantLifecycleManager lifecycleManager)
                    {
                        lifecycleManager.TriggerStageChangeEvent(plant);
                    }
                }
                
                // Check for critical health
                if (plant.OverallHealth < 0.2f)
                {
                    Debug.LogWarning($"[GrowthProcessor] Plant '{plantId}' has critical health: {plant.OverallHealth:F2}");
                    
                    // Use PlantLifecycleManager to trigger event
                    if (_plantLifecycleManager is PlantLifecycleManager lifecycleManager)
                    {
                        lifecycleManager.TriggerHealthCriticalEvent(plant);
                    }
                }
                
                // Check if plant died
                if (plant.OverallHealth <= 0f)
                {
                    Debug.LogWarning($"[GrowthProcessor] Plant '{plantId}' has died.");
                    plantsToRemove.Add(plantId);
                }
                // Check if plant is ready for harvest
                else if (plant.CurrentGrowthStage == PlantGrowthStage.Harvest)
                {
                    Debug.Log($"[GrowthProcessor] Plant '{plantId}' is ready for harvest!");
                }
                else
                {
                    totalHealth += plant.OverallHealth;
                    healthyPlants++;
                }
            }
            
            // Remove dead plants
            foreach (string plantId in plantsToRemove)
            {
                _plantLifecycleManager.RemovePlant(plantId, false);
            }
            
            // Update average health
            _averagePlantHealth = healthyPlants > 0 ? totalHealth / healthyPlants : 0f;
            
            Debug.Log($"[GrowthProcessor] Growth processed. Average health: {_averagePlantHealth:F2}, Dead plants removed: {plantsToRemove.Count}");
        }
        
        /// <summary>
        /// Forces an immediate growth update for testing purposes.
        /// </summary>
        public void ForceGrowthUpdate()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[GrowthProcessor] Cannot force growth update: Manager not initialized.");
                return;
            }
            
            ProcessDailyGrowthForAllPlants();
        }
        
        /// <summary>
        /// Gets growth statistics
        /// </summary>
        public (float avgHealth, int readyToHarvest, int critical) GetGrowthStatistics()
        {
            if (!IsInitialized) return (0f, 0, 0);
            
            var harvestReady = _plantLifecycleManager.GetPlantsByStage(PlantGrowthStage.Harvest);
            var allPlants = _plantLifecycleManager.GetAllPlants();
            
            int readyToHarvest = 0;
            int critical = 0;
            
            foreach (var plant in harvestReady)
            {
                readyToHarvest++;
            }
            
            foreach (var plant in allPlants)
            {
                if (plant.OverallHealth < 0.2f)
                {
                    critical++;
                }
            }
            
            return (_averagePlantHealth, readyToHarvest, critical);
        }
        
        /// <summary>
        /// Sets custom growth parameters for testing
        /// </summary>
        public void SetGrowthParameters(float timeAcceleration, bool enableAutoGrowth)
        {
            TimeAcceleration = timeAcceleration;
            EnableAutoGrowth = enableAutoGrowth;
            
            Debug.Log($"[GrowthProcessor] Updated growth parameters: Time acceleration: {_timeAcceleration}x, Auto-growth: {_enableAutoGrowth}");
        }
        
        /// <summary>
        /// Resets growth timing (useful for testing)
        /// </summary>
        public void ResetGrowthTiming()
        {
            _lastGrowthUpdate = Time.time;
            Debug.Log("[GrowthProcessor] Reset growth timing.");
        }
        
        /// <summary>
        /// Gets next growth update time
        /// </summary>
        public float GetTimeUntilNextGrowthUpdate()
        {
            if (!IsInitialized || !_enableAutoGrowth) return -1f;
            
            float timeSinceLastUpdate = Time.time - _lastGrowthUpdate;
            float adjustedInterval = GROWTH_UPDATE_INTERVAL / _timeAcceleration;
            
            return Mathf.Max(0f, adjustedInterval - timeSinceLastUpdate);
        }
        
        /// <summary>
        /// Converts Environment.EnvironmentalConditions to Cultivation.EnvironmentalConditions
        /// </summary>
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions ConvertToCultivationEnvironmentalConditions(ProjectChimera.Data.Environment.EnvironmentalConditions envData)
        {
            var cultivationEnv = new ProjectChimera.Data.Cultivation.EnvironmentalConditions();
            
            // Map common properties
            cultivationEnv.Temperature = envData.Temperature;
            cultivationEnv.Humidity = envData.Humidity;
            cultivationEnv.CO2Level = envData.CO2Level;
            cultivationEnv.LightIntensity = envData.LightIntensity;
            cultivationEnv.AirFlow = envData.AirFlow;
            
            // Map additional properties if they exist
            if (envData.DailyLightIntegral > 0)
            {
                cultivationEnv.PhotoperiodHours = envData.DailyLightIntegral / (envData.LightIntensity / 100f); // Approximate conversion
            }
            else
            {
                cultivationEnv.PhotoperiodHours = 18f; // Default for vegetative growth
            }
            
            cultivationEnv.WaterAvailability = 80f; // Default value
            cultivationEnv.ElectricalConductivity = 1200f; // Default EC
            cultivationEnv.pH = 6.5f; // Default pH
            
            return cultivationEnv;
        }
    }
}