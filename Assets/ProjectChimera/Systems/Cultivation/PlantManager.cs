using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using System;

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
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<PlantInstance> _onPlantGrowthStageChanged;
        [SerializeField] private GameEventSO<PlantInstance> _onPlantHealthChanged;
        [SerializeField] private GameEventSO<PlantInstance> _onPlantDied;
        
        [Header("Achievement Integration")]
        [SerializeField] private bool _enableAchievementTracking = true;
        
        // Private fields
        private Dictionary<string, PlantInstance> _activePlants = new Dictionary<string, PlantInstance>();
        private List<PlantInstance> _plantsToUpdate = new List<PlantInstance>();
        private int _currentUpdateIndex = 0;
        private float _lastUpdateTime = 0f;
        private PlantUpdateProcessor _updateProcessor;
        
        // Manager references removed to prevent cyclic assembly dependencies

        // Achievement tracking - now event-based
        private CultivationEventTracker _eventTracker;
        
        // Events for other systems to subscribe to
        public System.Action<PlantInstance> OnPlantAdded;
        public System.Action<PlantInstance> OnPlantHarvested;
        public System.Action<PlantInstance> OnPlantStageChanged;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public int ActivePlantCount => _activePlants.Count;
        public float GlobalGrowthModifier 
        { 
            get => _globalGrowthModifier; 
            set => _globalGrowthModifier = Mathf.Clamp(value, 0.1f, 10f); 
        }
        
        protected override void OnManagerInitialize()
        {
            _updateProcessor = new PlantUpdateProcessor(_enableStressSystem, _enableGxEInteractions);
            
            // Initialize achievement tracking
            if (_enableAchievementTracking)
            {
                InitializeAchievementTracking();
            }
            
            LogInfo($"PlantManager initialized with {_activePlants.Count} plants");
            
            // Initialize growth curve if not set
            if (_defaultGrowthCurve == null || _defaultGrowthCurve.keys.Length == 0)
            {
                InitializeDefaultGrowthCurve();
            }
        }

        protected override void OnManagerShutdown()
        {
            LogInfo("PlantManager shutting down");
            
            // Clean up all plants
            foreach (var plant in _activePlants.Values)
            {
                if (plant != null)
                {
                    Destroy(plant.gameObject);
                }
            }
            
            _activePlants.Clear();
            _plantsToUpdate.Clear();
            _updateProcessor = null;
        }
        
        protected override void OnManagerUpdate()
        {
            if (Time.time - _lastUpdateTime >= _plantUpdateInterval)
            {
                UpdatePlants();
                _lastUpdateTime = Time.time;
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
        /// </summary>
        public void RegisterPlant(PlantInstance plant)
        {
            if (plant == null)
            {
                LogError("Cannot register null plant");
                return;
            }
            
            if (_activePlants.ContainsKey(plant.PlantID))
            {
                LogWarning($"Plant {plant.PlantID} already registered, skipping");
                return;
            }
            
            _activePlants[plant.PlantID] = plant;
            _plantsToUpdate.Add(plant);
            
            // Subscribe to plant events
            plant.OnGrowthStageChanged += OnPlantGrowthStageChanged;
            plant.OnHealthChanged += OnPlantHealthChanged;
            plant.OnPlantDied += OnPlantDied;
            
            // Invoke OnPlantAdded event for other systems
            OnPlantAdded?.Invoke(plant);
            
            if (_enableDetailedLogging)
                LogInfo($"Registered plant: {plant.PlantID}");
        }
        
        /// <summary>
        /// Removes a plant from management (for harvesting, death, etc.).
        /// </summary>
        public void UnregisterPlant(string plantID, PlantRemovalReason reason = PlantRemovalReason.Other)
        {
            if (!_activePlants.TryGetValue(plantID, out var plant))
            {
                LogWarning($"Attempted to unregister unknown plant: {plantID}");
                return;
            }
            
            // Unsubscribe from events
            plant.OnGrowthStageChanged -= OnPlantGrowthStageChanged;
            plant.OnHealthChanged -= OnPlantHealthChanged;
            plant.OnPlantDied -= OnPlantDied;
            
            _activePlants.Remove(plantID);
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
            
            LogInfo($"Unregistered plant: {plantID} (Reason: {reason})");
        }
        
        /// <summary>
        /// Gets a plant instance by ID.
        /// </summary>
        public PlantInstance GetPlant(string plantID)
        {
            _activePlants.TryGetValue(plantID, out var plant);
            return plant;
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage.
        /// </summary>
        public List<PlantInstance> GetPlantsInStage(PlantGrowthStage stage)
        {
            return _activePlants.Values.Where(p => p.CurrentGrowthStage == stage).ToList();
        }
        
        /// <summary>
        /// Gets all plants ready for harvest.
        /// </summary>
        public List<PlantInstance> GetHarvestablePlants()
        {
            return GetPlantsInStage(PlantGrowthStage.Harvest);
        }
        
        /// <summary>
        /// Gets all plants that need attention (low health, stress, etc.).
        /// </summary>
        public List<PlantInstance> GetPlantsNeedingAttention()
        {
            return _activePlants.Values.Where(p => 
                p.CurrentHealth < 0.7f || 
                p.StressLevel > 0.3f || 
                p.HasActiveStressors()
            ).ToList();
        }
        
        /// <summary>
        /// Updates the environmental conditions for all plants.
        /// </summary>
        public void UpdateEnvironmentalConditions(EnvironmentalConditions newConditions)
        {
            foreach (var plant in _activePlants.Values)
            {
                plant.UpdateEnvironmentalConditions(newConditions);
            }
            
            LogInfo($"Updated environmental conditions for {_activePlants.Count} plants");
        }
        
        /// <summary>
        /// Applies environmental stress to plants based on conditions.
        /// </summary>
        public void ApplyEnvironmentalStress(EnvironmentalStressSO stressSource, float intensity)
        {
            if (!_enableStressSystem || stressSource == null)
                return;
            
            int affectedPlants = 0;
            foreach (var plant in _activePlants.Values)
            {
                if (plant.ApplyStress(stressSource, intensity))
                    affectedPlants++;
            }
            
            LogInfo($"Applied stress '{stressSource.StressName}' to {affectedPlants} plants");
        }
        
        /// <summary>
        /// Harvests a plant and returns the harvest results.
        /// </summary>
        public HarvestResults HarvestPlant(string plantID)
        {
            var plant = GetPlant(plantID);
            if (plant == null)
            {
                LogError($"Cannot harvest unknown plant: {plantID}");
                return null;
            }
            
            if (plant.CurrentGrowthStage != PlantGrowthStage.Harvest)
            {
                LogWarning($"Plant {plantID} is not ready for harvest (Stage: {plant.CurrentGrowthStage})");
                return null;
            }
            
            var harvestResults = plant.Harvest();
            
            // Track harvest achievements before unregistering
            if (_enableAchievementTracking && _eventTracker != null)
            {
                _eventTracker.OnPlantHarvested(plant, harvestResults);
            }
            
            // Invoke OnPlantHarvested event for other systems
            OnPlantHarvested?.Invoke(plant);
            
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
        
        /// <summary>
        /// Gets comprehensive statistics about all managed plants.
        /// </summary>
        public PlantManagerStatistics GetStatistics()
        {
            var stats = new PlantManagerStatistics();
            
            foreach (var plant in _activePlants.Values)
            {
                stats.TotalPlants++;
                stats.PlantsByStage[(int)plant.CurrentGrowthStage]++;
                stats.AverageHealth += plant.CurrentHealth;
                stats.AverageStress += plant.StressLevel;
                
                if (plant.CurrentHealth < 0.5f)
                    stats.UnhealthyPlants++;
                
                if (plant.StressLevel > 0.7f)
                    stats.HighStressPlants++;
            }
            
            if (stats.TotalPlants > 0)
            {
                stats.AverageHealth /= stats.TotalPlants;
                stats.AverageStress /= stats.TotalPlants;
            }
            
            return stats;
        }
        
        private void UpdatePlants()
        {
            if (_plantsToUpdate.Count == 0)
                return;
            
            var timeManager = GameManager.Instance.GetManager<TimeManager>();
            float deltaTime = timeManager.GetScaledDeltaTime();
            
            int plantsToProcess = Mathf.Min(_maxPlantsPerUpdate, _plantsToUpdate.Count);
            int endIndex = Mathf.Min(_currentUpdateIndex + plantsToProcess, _plantsToUpdate.Count);
            
            for (int i = _currentUpdateIndex; i < endIndex; i++)
            {
                var plant = _plantsToUpdate[i];
                if (plant != null && plant.IsActive)
                {
                    _updateProcessor.UpdatePlant(plant, deltaTime, _globalGrowthModifier);
                }
            }
            
            _currentUpdateIndex = endIndex;
            
            // Reset index when we've processed all plants
            if (_currentUpdateIndex >= _plantsToUpdate.Count)
            {
                _currentUpdateIndex = 0;
                
                // Remove any inactive plants from the update list
                _plantsToUpdate.RemoveAll(p => p == null || !p.IsActive);
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

    }
    
    /// <summary>
    /// Statistics about all plants managed by the PlantManager.
    /// </summary>
    [System.Serializable]
    public class PlantManagerStatistics
    {
        public int TotalPlants;
        public int[] PlantsByStage = new int[System.Enum.GetValues(typeof(PlantGrowthStage)).Length];
        public float AverageHealth;
        public float AverageStress;
        public int UnhealthyPlants;
        public int HighStressPlants;
    }
    
    public enum PlantRemovalReason
    {
        Harvested,
        Died,
        Removed,
        Other
    }
    
    /// <summary>
    /// Event-based cultivation achievement tracking.
    /// Triggers events that progression system can listen to.
    /// </summary>
    public class CultivationEventTracker
    {
        private Dictionary<string, int> _plantCounts = new Dictionary<string, int>();
        private Dictionary<string, float> _harvestTotals = new Dictionary<string, float>();
        private List<PlantInstance> _healthyPlants = new List<PlantInstance>();
        private int _totalPlantsCreated = 0;
        private int _totalPlantsHarvested = 0;
        private int _totalPlantDeaths = 0;
        private float _totalYieldHarvested = 0f;
        private float _highestQualityAchieved = 0f;
        
        public void OnPlantCreated(PlantInstance plant)
        {
            _totalPlantsCreated++;
            
            // Track by strain
            string strainName = plant.Strain?.StrainName ?? "Unknown";
            if (!_plantCounts.ContainsKey(strainName))
                _plantCounts[strainName] = 0;
            _plantCounts[strainName]++;
            
            // Trigger events for progression system to listen to
            // The progression system will handle achievements
            UnityEngine.Debug.Log($"🌱 Plant Created Event: {plant.PlantName} (Total: {_totalPlantsCreated})");
        }
        
        public void OnPlantHarvested(PlantInstance plant, HarvestResults harvestResults)
        {
            _totalPlantsHarvested++;
            _totalYieldHarvested += harvestResults.TotalYieldGrams;
            
            if (harvestResults.QualityScore > _highestQualityAchieved)
                _highestQualityAchieved = harvestResults.QualityScore;
            
            // Track harvest by strain
            string strainName = plant.Strain?.StrainName ?? "Unknown";
            if (!_harvestTotals.ContainsKey(strainName))
                _harvestTotals[strainName] = 0f;
            _harvestTotals[strainName] += harvestResults.TotalYieldGrams;
            
            UnityEngine.Debug.Log($"🌾 Plant Harvested Event: {plant.PlantName} ({harvestResults.TotalYieldGrams}g, Quality: {harvestResults.QualityScore:F2})");
        }
        
        public void OnPlantDied(PlantInstance plant)
        {
            _totalPlantDeaths++;
            _healthyPlants.Remove(plant);
            
            UnityEngine.Debug.Log($"💀 Plant Death Event: {plant.PlantName} (Total Deaths: {_totalPlantDeaths})");
        }
        
        public void OnPlantHealthChanged(PlantInstance plant)
        {
            // Track plants with consistently high health
            if (plant.CurrentHealth >= 0.9f && !_healthyPlants.Contains(plant))
            {
                _healthyPlants.Add(plant);
            }
            else if (plant.CurrentHealth < 0.9f && _healthyPlants.Contains(plant))
            {
                _healthyPlants.Remove(plant);
            }
            
            // Events can be triggered based on health milestones
            if (_healthyPlants.Count >= 5)
            {
                UnityEngine.Debug.Log($"💚 Health Achievement Event: {_healthyPlants.Count} healthy plants maintained");
            }
        }
        
        public void OnPlantGrowthStageChanged(PlantInstance plant)
        {
            // Events for growth stage progression
            if (plant.CurrentGrowthStage == PlantGrowthStage.Flowering)
            {
                UnityEngine.Debug.Log($"🌸 Growth Achievement Event: {plant.PlantName} reached flowering stage");
            }
            if (plant.CurrentGrowthStage == PlantGrowthStage.Harvest)
            {
                UnityEngine.Debug.Log($"🎯 Maturity Achievement Event: {plant.PlantName} ready for harvest");
            }
        }
        
        // Public getters for progression system to access data
        public int TotalPlantsCreated => _totalPlantsCreated;
        public int TotalPlantsHarvested => _totalPlantsHarvested;
        public float TotalYieldHarvested => _totalYieldHarvested;
        public float HighestQualityAchieved => _highestQualityAchieved;
        public int HealthyPlantsCount => _healthyPlants.Count;
        public int StrainDiversity => _plantCounts.Count;
    }
}