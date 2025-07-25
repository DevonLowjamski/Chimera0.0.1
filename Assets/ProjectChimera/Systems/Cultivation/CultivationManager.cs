using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Economy;
using ProjectChimera.Systems.Economy;
using DataEnvironmental = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Central manager for all cultivation operations in Project Chimera.
    /// Handles plant lifecycle management, environmental control, and resource management.
    /// </summary>
    public class CultivationManager : ChimeraManager
    {
        [Header("Cultivation Configuration")]
        [SerializeField] private bool _enableAutoGrowth = true;
        [SerializeField] private float _timeAcceleration = 1f;
        [SerializeField] private int _maxPlantsPerGrow = 50;
        
        [Header("Default Environmental Settings")]
        [SerializeField] private DataEnvironmental _defaultEnvironment;
        
        [Header("Events")]
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantPlanted;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantHarvested;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantStageChanged;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onPlantHealthCritical;
        
        // Runtime data
        private Dictionary<string, PlantInstanceSO> _activePlants = new Dictionary<string, PlantInstanceSO>();
        private Dictionary<string, Vector3> _plantPositions = new Dictionary<string, Vector3>();
        private Dictionary<string, DataEnvironmental> _zoneEnvironments = new Dictionary<string, DataEnvironmental>();
        
        // Cultivation statistics
        private int _totalPlantsGrown = 0;
        private int _totalPlantsHarvested = 0;
        private float _totalYieldHarvested = 0f;
        private float _averagePlantHealth = 0f;
        
        // Time management
        private float _lastGrowthUpdate = 0f;
        private const float GROWTH_UPDATE_INTERVAL = 86400f; // 24 hours in seconds (real-time)
        
        public string ManagerName => "Cultivation Manager";
        
        // Properties
        public int ActivePlantCount => _activePlants.Count;
        public int TotalPlantsGrown => _totalPlantsGrown;
        public int TotalPlantsHarvested => _totalPlantsHarvested;
        public float TotalYieldHarvested => _totalYieldHarvested;
        public float AveragePlantHealth => _averagePlantHealth;
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
        
        protected override void OnManagerInitialize()
        {
            Debug.Log("[CultivationManager] Initializing cultivation systems...");
            
            // Initialize default environment if not set
            if (_defaultEnvironment.Temperature == 0f)
            {
                _defaultEnvironment = DataEnvironmental.CreateIndoorDefault();
            }
            
            // Set up default growing zone
            _zoneEnvironments["default"] = _defaultEnvironment;
            
            _lastGrowthUpdate = Time.time;
            
            Debug.Log($"[CultivationManager] Initialized. Max plants: {_maxPlantsPerGrow}, Auto-growth: {_enableAutoGrowth}");
        }
        
        protected override void OnManagerShutdown()
        {
            Debug.Log("[CultivationManager] Shutting down cultivation systems...");
            
            // Save current plant states before shutdown
            SaveAllPlantStates();
            
            _activePlants.Clear();
            _plantPositions.Clear();
            _zoneEnvironments.Clear();
        }
        
        protected override void Update()
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
        /// Plants a new plant instance in the cultivation system.
        /// </summary>
        public PlantInstanceSO PlantSeed(string plantName, PlantStrainSO strain, GenotypeDataSO genotype, Vector3 position, string zoneId = "default")
        {
            if (_activePlants.Count >= _maxPlantsPerGrow)
            {
                Debug.LogWarning($"[CultivationManager] Cannot plant '{plantName}': Maximum plant limit ({_maxPlantsPerGrow}) reached.");
                return null;
            }
            
            if (strain == null)
            {
                Debug.LogError($"[CultivationManager] Cannot plant '{plantName}': No strain specified.");
                return null;
            }
            
            // Generate unique plant ID
            string plantId = GenerateUniquePlantId();
            
            // Create new plant instance
            PlantInstanceSO newPlant = ScriptableObject.CreateInstance<PlantInstanceSO>();
            newPlant.name = $"Plant_{plantId}_{plantName}";
            newPlant.InitializePlant(plantId, plantName, strain, genotype, position);
            
            // Add to active plants
            _activePlants[plantId] = newPlant;
            _plantPositions[plantId] = position;
            
            // Set initial environment for the zone
            if (!_zoneEnvironments.ContainsKey(zoneId))
            {
                _zoneEnvironments[zoneId] = _defaultEnvironment;
            }
            
            _totalPlantsGrown++;
            
            // Raise planting event
            _onPlantPlanted?.Raise(newPlant);
            
            Debug.Log($"[CultivationManager] Planted '{plantName}' (ID: {plantId}) at position {position}");
            
            return newPlant;
        }
        
        /// <summary>
        /// Removes a plant from the cultivation system (harvest, death, etc.).
        /// </summary>
        public bool RemovePlant(string plantId, bool isHarvest = false)
        {
            if (!_activePlants.ContainsKey(plantId))
            {
                Debug.LogWarning($"[CultivationManager] Cannot remove plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            PlantInstanceSO plant = _activePlants[plantId];
            
            if (isHarvest)
            {
                ProcessHarvest(plant);
            }
            
            // Clean up
            _activePlants.Remove(plantId);
            _plantPositions.Remove(plantId);
            
            // Destroy the ScriptableObject instance
            if (Application.isPlaying)
            {
                Destroy(plant);
            }
            else
            {
                DestroyImmediate(plant);
            }
            
            Debug.Log($"[CultivationManager] Removed plant '{plantId}' (Harvest: {isHarvest})");
            
            return true;
        }
        
        /// <summary>
        /// Gets a plant instance by its ID.
        /// </summary>
        public PlantInstanceSO GetPlant(string plantId)
        {
            _activePlants.TryGetValue(plantId, out PlantInstanceSO plant);
            return plant;
        }
        
        /// <summary>
        /// Gets all active plants.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            return _activePlants.Values;
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsByStage(PlantGrowthStage stage)
        {
            return _activePlants.Values.Where(plant => plant.CurrentGrowthStage == stage);
        }
        
        /// <summary>
        /// Gets all plants that need attention (low health, resources, etc.).
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            return _activePlants.Values.Where(plant => 
                plant.OverallHealth < 0.5f || 
                plant.WaterLevel < 0.3f || 
                plant.NutrientLevel < 0.3f ||
                plant.StressLevel > 0.7f
            );
        }
        
        /// <summary>
        /// Updates environmental conditions for a specific zone.
        /// </summary>
        public void SetZoneEnvironment(string zoneId, DataEnvironmental environment)
        {
            _zoneEnvironments[zoneId] = environment;
            Debug.Log($"[CultivationManager] Updated environment for zone '{zoneId}': {environment}");
        }
        
        /// <summary>
        /// Gets environmental conditions for a specific zone.
        /// </summary>
        public DataEnvironmental GetZoneEnvironment(string zoneId)
        {
            return _zoneEnvironments.TryGetValue(zoneId, out DataEnvironmental environment) 
                ? environment 
                : _defaultEnvironment;
        }
        
        /// <summary>
        /// Waters a specific plant.
        /// </summary>
        public bool WaterPlant(string plantId, float waterAmount = 0.5f)
        {
            if (!_activePlants.TryGetValue(plantId, out PlantInstanceSO plant))
            {
                Debug.LogWarning($"[CultivationManager] Cannot water plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            plant.Water(waterAmount, System.DateTime.Now);
            Debug.Log($"[CultivationManager] Watered plant '{plantId}' with {waterAmount * 100f}% water.");
            
            return true;
        }
        
        /// <summary>
        /// Feeds nutrients to a specific plant.
        /// </summary>
        public bool FeedPlant(string plantId, float nutrientAmount = 0.4f)
        {
            if (!_activePlants.TryGetValue(plantId, out PlantInstanceSO plant))
            {
                Debug.LogWarning($"[CultivationManager] Cannot feed plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            plant.Feed(nutrientAmount, System.DateTime.Now);
            Debug.Log($"[CultivationManager] Fed plant '{plantId}' with {nutrientAmount * 100f}% nutrients.");
            
            return true;
        }
        
        /// <summary>
        /// Applies training to a specific plant.
        /// </summary>
        public bool TrainPlant(string plantId, string trainingType)
        {
            if (!_activePlants.TryGetValue(plantId, out PlantInstanceSO plant))
            {
                Debug.LogWarning($"[CultivationManager] Cannot train plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            plant.ApplyTraining(trainingType, System.DateTime.Now);
            Debug.Log($"[CultivationManager] Applied '{trainingType}' training to plant '{plantId}'.");
            
            return true;
        }
        
        /// <summary>
        /// Waters all plants in the cultivation system.
        /// </summary>
        public void WaterAllPlants(float waterAmount = 0.5f)
        {
            int wateredCount = 0;
            foreach (var plant in _activePlants.Values)
            {
                if (plant.WaterLevel < 0.8f) // Only water if needed
                {
                    plant.Water(waterAmount, System.DateTime.Now);
                    wateredCount++;
                }
            }
            
            Debug.Log($"[CultivationManager] Auto-watered {wateredCount}/{_activePlants.Count} plants.");
        }
        
        /// <summary>
        /// Feeds all plants in the cultivation system.
        /// </summary>
        public void FeedAllPlants(float nutrientAmount = 0.4f)
        {
            int fedCount = 0;
            foreach (var plant in _activePlants.Values)
            {
                if (plant.NutrientLevel < 0.7f) // Only feed if needed
                {
                    plant.Feed(nutrientAmount, System.DateTime.Now);
                    fedCount++;
                }
            }
            
            Debug.Log($"[CultivationManager] Auto-fed {fedCount}/{_activePlants.Count} plants.");
        }
        
        /// <summary>
        /// Processes daily growth for all active plants.
        /// </summary>
        public void ProcessDailyGrowthForAllPlants()
        {
            Debug.Log($"[CultivationManager] Processing daily growth for {_activePlants.Count} plants...");
            
            List<string> plantsToRemove = new List<string>();
            float totalHealth = 0f;
            int healthyPlants = 0;
            
            foreach (var kvp in _activePlants)
            {
                string plantId = kvp.Key;
                PlantInstanceSO plant = kvp.Value;
                
                // Get environment for this plant's zone
                DataEnvironmental environment = GetEnvironmentForPlant(plantId);
                
                // Track health before growth
                PlantGrowthStage previousStage = plant.CurrentGrowthStage;
                
                // Process daily growth
                plant.ProcessDailyGrowth(environment, _timeAcceleration);
                
                // Check for stage transition
                if (plant.CurrentGrowthStage != previousStage)
                {
                    Debug.Log($"[CultivationManager] Plant '{plantId}' transitioned from {previousStage} to {plant.CurrentGrowthStage}");
                    _onPlantStageChanged?.Raise(plant);
                }
                
                // Check for critical health
                if (plant.OverallHealth < 0.2f)
                {
                    Debug.LogWarning($"[CultivationManager] Plant '{plantId}' has critical health: {plant.OverallHealth:F2}");
                    _onPlantHealthCritical?.Raise(plant);
                }
                
                // Check if plant died
                if (plant.OverallHealth <= 0f)
                {
                    Debug.LogWarning($"[CultivationManager] Plant '{plantId}' has died.");
                    plantsToRemove.Add(plantId);
                }
                // Check if plant is ready for harvest
                else if (plant.CurrentGrowthStage == PlantGrowthStage.Harvest)
                {
                    Debug.Log($"[CultivationManager] Plant '{plantId}' is ready for harvest!");
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
                RemovePlant(plantId, false);
            }
            
            // Update average health
            _averagePlantHealth = healthyPlants > 0 ? totalHealth / healthyPlants : 0f;
            
            Debug.Log($"[CultivationManager] Growth processed. Average health: {_averagePlantHealth:F2}, Dead plants removed: {plantsToRemove.Count}");
        }
        
        /// <summary>
        /// Forces an immediate growth update for testing purposes.
        /// </summary>
        public void ForceGrowthUpdate()
        {
            ProcessDailyGrowthForAllPlants();
        }
        
        /// <summary>
        /// Gets cultivation statistics.
        /// </summary>
        public (int active, int grown, int harvested, float yield, float avgHealth) GetCultivationStats()
        {
            return (_activePlants.Count, _totalPlantsGrown, _totalPlantsHarvested, _totalYieldHarvested, _averagePlantHealth);
        }
        
        private string GenerateUniquePlantId()
        {
            string baseId;
            int counter = 1;
            
            do
            {
                baseId = $"plant_{System.DateTime.Now:yyyyMMdd}_{counter:D3}";
                counter++;
            }
            while (_activePlants.ContainsKey(baseId));
            
            return baseId;
        }
        
        private DataEnvironmental GetEnvironmentForPlant(string plantId)
        {
            // For now, use default zone. Later can implement per-plant zone assignment
            return _zoneEnvironments.TryGetValue("default", out DataEnvironmental environment) 
                ? environment 
                : _defaultEnvironment;
        }
        
        private void ProcessHarvest(PlantInstanceSO plant)
        {
            float yieldAmount = plant.CalculateYieldPotential() * 100f; // Convert to grams
            float potency = plant.CalculatePotencyPotential();
            
            _totalPlantsHarvested++;
            _totalYieldHarvested += yieldAmount;
            
            Debug.Log($"[CultivationManager] Harvested plant '{plant.PlantID}': {yieldAmount:F1}g at {potency:F1}% potency");
            
            // PC-009-2: Add harvested product to player inventory
            AddHarvestToInventory(plant, yieldAmount, potency);
            
            // Raise harvest event
            _onPlantHarvested?.Raise(plant);
        }
        
        /// <summary>
        /// PC-009-2: Adds harvested plant product to player inventory
        /// </summary>
        private void AddHarvestToInventory(PlantInstanceSO plant, float yieldAmount, float qualityScore)
        {
            try
            {
                // Get TradingManager for inventory access
                var tradingManager = GameManager.Instance?.GetManager<TradingManager>();
                if (tradingManager?.PlayerInventory == null)
                {
                    Debug.LogWarning("[CultivationManager] TradingManager or PlayerInventory not available - harvest not added to inventory");
                    return;
                }
                
                // Create default cannabis flower product if none exists
                // In a full implementation, this would reference an actual MarketProductSO asset
                MarketProductSO flowerProduct = null; // Will be null for now - indicates generic harvested flower
                
                // Generate unique batch ID from plant data
                string batchId = $"{plant.Strain?.DisplayName ?? "Unknown"}_{plant.PlantID}_{System.DateTime.Now:yyyyMMdd_HHmm}";
                
                // Add to inventory
                bool added = tradingManager.PlayerInventory.AddHarvestedProduct(
                    product: flowerProduct, // Will create generic product entry
                    quantity: yieldAmount,
                    qualityScore: qualityScore,
                    batchId: batchId
                );
                
                if (added)
                {
                    Debug.Log($"[CultivationManager] Successfully added {yieldAmount:F1}g to inventory (Batch: {batchId}, Quality: {qualityScore:F1}%)");
                }
                else
                {
                    Debug.LogWarning($"[CultivationManager] Failed to add harvest to inventory - possibly over capacity");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CultivationManager] Error adding harvest to inventory: {ex.Message}");
            }
        }
        
        private void SaveAllPlantStates()
        {
            // This would typically save to persistent storage
            // For now, just log the save operation
            Debug.Log($"[CultivationManager] Saving states for {_activePlants.Count} plants...");
        }
    }
}