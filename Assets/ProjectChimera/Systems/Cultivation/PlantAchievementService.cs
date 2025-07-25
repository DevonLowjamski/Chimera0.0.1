using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-3: Plant Achievement Service - Handles achievement tracking, event handling, and progression integration
    /// Extracted from monolithic PlantManager for Single Responsibility Principle
    /// Focuses solely on tracking plant-related achievements and progression events
    /// </summary>
    public class PlantAchievementService : IPlantAchievementService
    {
        [Header("Achievement Configuration")]
        [SerializeField] private bool _enableAchievementTracking = true;
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Achievement Event Channels")]
        [SerializeField] private SimpleGameEventSO _onPlantCreated;
        [SerializeField] private SimpleGameEventSO _onPlantHarvested;
        [SerializeField] private SimpleGameEventSO _onQualityHarvest;
        [SerializeField] private SimpleGameEventSO _onPerfectQuality;
        [SerializeField] private SimpleGameEventSO _onHighYieldAchieved;
        [SerializeField] private SimpleGameEventSO _onPotencyRecord;
        [SerializeField] private SimpleGameEventSO _onTerpeneProfile;
        
        // Achievement tracking data
        private CultivationEventTracker _eventTracker;
        private Dictionary<string, int> _plantCounts = new Dictionary<string, int>();
        private Dictionary<string, float> _harvestTotals = new Dictionary<string, float>();
        private List<PlantInstance> _healthyPlants = new List<PlantInstance>();
        private int _totalPlantsCreated = 0;
        private int _totalPlantsHarvested = 0;
        private int _totalPlantDeaths = 0;
        private float _totalYieldHarvested = 0f;
        private float _highestQualityAchieved = 0f;
        
        public bool IsInitialized { get; private set; }
        
        public bool EnableAchievementTracking
        {
            get => _enableAchievementTracking;
            set => _enableAchievementTracking = value;
        }
        
        // Achievement statistics properties
        public int TotalPlantsCreated => _totalPlantsCreated;
        public int TotalPlantsHarvested => _totalPlantsHarvested;
        public float TotalYieldHarvested => _totalYieldHarvested;
        public float HighestQualityAchieved => _highestQualityAchieved;
        public int HealthyPlantsCount => _healthyPlants.Count;
        public int StrainDiversity => _plantCounts.Count;
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("[PlantAchievementService] Initializing achievement tracking system...");
            
            if (_enableAchievementTracking)
            {
                InitializeAchievementTracking();
            }
            
            IsInitialized = true;
            Debug.Log($"[PlantAchievementService] Achievement tracking initialized (Enabled: {_enableAchievementTracking})");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("[PlantAchievementService] Shutting down achievement tracking system...");
            
            _plantCounts.Clear();
            _harvestTotals.Clear();
            _healthyPlants.Clear();
            _eventTracker = null;
            
            IsInitialized = false;
        }
        
        /// <summary>
        /// Tracks plant creation for achievement purposes.
        /// </summary>
        public void TrackPlantCreation(PlantInstance plant)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null) return;
            
            _totalPlantsCreated++;
            
            // Track strain diversity
            if (plant.Strain != null)
            {
                string strainName = plant.Strain.StrainName;
                if (_plantCounts.ContainsKey(strainName))
                    _plantCounts[strainName]++;
                else
                    _plantCounts[strainName] = 1;
            }
            
            // Trigger achievement events
            _onPlantCreated?.Raise();
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                _eventTracker.OnPlantCreated(plant);
            }
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[PlantAchievementService] Tracked plant creation: {plant.PlantID} (Total: {_totalPlantsCreated})");
            }
            
            // Check for creation milestones
            CheckCreationMilestones();
        }
        
        /// <summary>
        /// Tracks plant harvest for achievement purposes.
        /// </summary>
        public void TrackPlantHarvest(PlantInstance plant, SystemsHarvestResults harvestResults)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null || harvestResults == null) return;
            
            _totalPlantsHarvested++;
            _totalYieldHarvested += harvestResults.TotalYieldGrams;
            
            // Track quality records
            if (harvestResults.QualityScore > _highestQualityAchieved)
            {
                _highestQualityAchieved = harvestResults.QualityScore;
                
                // Check for quality milestones
                CheckQualityMilestones(harvestResults.QualityScore);
            }
            
            // Track strain-specific harvest totals
            if (plant.Strain != null)
            {
                string strainName = plant.Strain.StrainName;
                if (_harvestTotals.ContainsKey(strainName))
                    _harvestTotals[strainName] += harvestResults.TotalYieldGrams;
                else
                    _harvestTotals[strainName] = harvestResults.TotalYieldGrams;
            }
            
            // Trigger achievement events
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
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                _eventTracker.OnPlantHarvested(plant, harvestResults);
            }
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[PlantAchievementService] Tracked plant harvest: {plant.PlantID} - Yield: {harvestResults.TotalYieldGrams}g, Quality: {harvestResults.QualityScore:F2}");
            }
            
            // Check for harvest milestones
            CheckHarvestMilestones();
        }
        
        /// <summary>
        /// Tracks plant death for achievement purposes.
        /// </summary>
        public void TrackPlantDeath(PlantInstance plant)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null) return;
            
            _totalPlantDeaths++;
            _healthyPlants.Remove(plant);
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                _eventTracker.OnPlantDied(plant);
            }
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[PlantAchievementService] Tracked plant death: {plant.PlantID} (Total deaths: {_totalPlantDeaths})");
            }
            
            // Check for survival-related achievements (inverse tracking)
            CheckSurvivalMilestones();
        }
        
        /// <summary>
        /// Tracks plant health changes for achievement purposes.
        /// </summary>
        public void TrackPlantHealthChange(PlantInstance plant)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null) return;
            
            // Track healthy plants
            bool wasHealthy = _healthyPlants.Contains(plant);
            bool isHealthy = plant.CurrentHealth > 0.8f;
            
            if (isHealthy && !wasHealthy)
            {
                _healthyPlants.Add(plant);
            }
            else if (!isHealthy && wasHealthy)
            {
                _healthyPlants.Remove(plant);
            }
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                _eventTracker.OnPlantHealthChanged(plant);
            }
            
            if (_enableDetailedLogging && wasHealthy != isHealthy)
            {
                Debug.Log($"[PlantAchievementService] Health change tracked: {plant.PlantID} - Health: {plant.CurrentHealth:F2} (Healthy plants: {_healthyPlants.Count})");
            }
            
            // Check for health-related achievements
            CheckHealthMilestones();
        }
        
        /// <summary>
        /// Tracks plant growth stage changes for achievement purposes.
        /// </summary>
        public void TrackPlantGrowthStageChange(PlantInstance plant)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null) return;
            
            // Track growth milestones
            if (plant.CurrentGrowthStage == PlantGrowthStage.Flowering && plant.CurrentHealth > 0.9f)
            {
                // High-health flowering achievement
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] High-health flowering tracked: {plant.PlantID}");
                }
            }
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                _eventTracker.OnPlantGrowthStageChanged(plant);
            }
            
            // Check for growth-related achievements
            CheckGrowthMilestones(plant);
        }
        
        /// <summary>
        /// Gets comprehensive achievement statistics.
        /// </summary>
        public PlantAchievementStats GetAchievementStats()
        {
            if (!IsInitialized)
                return new PlantAchievementStats();
            
            return new PlantAchievementStats
            {
                TotalPlantsCreated = _totalPlantsCreated,
                TotalPlantsHarvested = _totalPlantsHarvested,
                TotalPlantDeaths = _totalPlantDeaths,
                TotalYieldHarvested = _totalYieldHarvested,
                HighestQualityAchieved = _highestQualityAchieved,
                HealthyPlantsCount = _healthyPlants.Count,
                StrainDiversity = _plantCounts.Count,
                SurvivalRate = _totalPlantsCreated > 0 ? (float)(_totalPlantsCreated - _totalPlantDeaths) / _totalPlantsCreated : 0f,
                AverageYieldPerPlant = _totalPlantsHarvested > 0 ? _totalYieldHarvested / _totalPlantsHarvested : 0f
            };
        }
        
        /// <summary>
        /// Resets all achievement tracking data.
        /// </summary>
        public void ResetAchievementData()
        {
            if (!IsInitialized) return;
            
            _plantCounts.Clear();
            _harvestTotals.Clear();
            _healthyPlants.Clear();
            _totalPlantsCreated = 0;
            _totalPlantsHarvested = 0;
            _totalPlantDeaths = 0;
            _totalYieldHarvested = 0f;
            _highestQualityAchieved = 0f;
            
            Debug.Log("[PlantAchievementService] Achievement tracking data reset");
        }
        
        /// <summary>
        /// Process plant-related achievements for a specific plant
        /// </summary>
        public void ProcessPlantAchievements(PlantInstance plant)
        {
            if (!IsInitialized || !_enableAchievementTracking || plant == null)
            {
                return;
            }
            
            var startTime = System.DateTime.Now;
            
            // Check health-based achievements
            if (plant.CurrentHealth > 90f)
            {
                if (!_healthyPlants.Contains(plant))
                {
                    _healthyPlants.Add(plant);
                    CheckHealthMilestones();
                }
            }
            else
            {
                _healthyPlants.Remove(plant);
            }
            
            // Check growth stage achievements
            CheckGrowthMilestones(plant);
            
            // Check survival achievements
            CheckSurvivalMilestones();
            
            // Track with event tracker
            if (_eventTracker != null)
            {
                // Process any pending achievement events
                _eventTracker.ProcessPlantAchievements(plant);
            }
            
            var processTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            
            if (_enableDetailedLogging && processTime > 1.0)
            {
                Debug.Log($"[PlantAchievementService] Processed achievements for {plant.PlantID} in {processTime:F1}ms");
            }
        }
        
        /// <summary>
        /// Initializes achievement tracking system.
        /// </summary>
        private void InitializeAchievementTracking()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                _eventTracker = new CultivationEventTracker();
                Debug.Log("[PlantAchievementService] Achievement tracking system initialized");
            }
            else
            {
                Debug.LogWarning("[PlantAchievementService] GameManager not found - achievement tracking disabled");
                _enableAchievementTracking = false;
            }
        }
        
        /// <summary>
        /// Checks for plant creation milestones.
        /// </summary>
        private void CheckCreationMilestones()
        {
            // Check for creation milestones (10, 50, 100, 500, 1000 plants)
            if (_totalPlantsCreated == 10 || _totalPlantsCreated == 50 || _totalPlantsCreated == 100 || 
                _totalPlantsCreated == 500 || _totalPlantsCreated == 1000)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] Creation milestone reached: {_totalPlantsCreated} plants");
                }
                
                // Trigger milestone event
                _onPlantCreated?.Raise();
            }
        }
        
        /// <summary>
        /// Checks for harvest milestones.
        /// </summary>
        private void CheckHarvestMilestones()
        {
            // Check for harvest milestones
            if (_totalPlantsHarvested == 10 || _totalPlantsHarvested == 50 || _totalPlantsHarvested == 100 || 
                _totalPlantsHarvested == 500 || _totalPlantsHarvested == 1000)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] Harvest milestone reached: {_totalPlantsHarvested} plants");
                }
            }
            
            // Check for yield milestones (1kg, 5kg, 10kg, 50kg, 100kg)
            if (_totalYieldHarvested >= 1000f || _totalYieldHarvested >= 5000f || _totalYieldHarvested >= 10000f || 
                _totalYieldHarvested >= 50000f || _totalYieldHarvested >= 100000f)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] Yield milestone reached: {_totalYieldHarvested:F0}g");
                }
                
                _onHighYieldAchieved?.Raise();
            }
        }
        
        /// <summary>
        /// Checks for quality milestones.
        /// </summary>
        private void CheckQualityMilestones(float qualityScore)
        {
            // Check for quality milestones
            if (qualityScore >= 0.95f)
            {
                _onPerfectQuality?.Raise();
            }
            else if (qualityScore >= 0.9f)
            {
                _onQualityHarvest?.Raise();
            }
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[PlantAchievementService] Quality milestone: {qualityScore:F2} (Record: {_highestQualityAchieved:F2})");
            }
        }
        
        /// <summary>
        /// Checks for health-related milestones.
        /// </summary>
        private void CheckHealthMilestones()
        {
            // Check for healthy plant milestones
            if (_healthyPlants.Count >= 10 || _healthyPlants.Count >= 50 || _healthyPlants.Count >= 100)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] Healthy plants milestone: {_healthyPlants.Count} plants");
                }
            }
        }
        
        /// <summary>
        /// Checks for growth-related milestones.
        /// </summary>
        private void CheckGrowthMilestones(PlantInstance plant)
        {
            // Check for specific growth achievements
            if (plant.CurrentGrowthStage == PlantGrowthStage.Harvest && plant.CurrentHealth > 0.95f)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] Perfect health harvest: {plant.PlantID}");
                }
            }
        }
        
        /// <summary>
        /// Checks for survival-related milestones.
        /// </summary>
        private void CheckSurvivalMilestones()
        {
            // Calculate survival rate
            float survivalRate = _totalPlantsCreated > 0 ? (float)(_totalPlantsCreated - _totalPlantDeaths) / _totalPlantsCreated : 0f;
            
            // Check for high survival rate achievements
            if (survivalRate >= 0.95f && _totalPlantsCreated >= 50)
            {
                if (_enableDetailedLogging)
                {
                    Debug.Log($"[PlantAchievementService] High survival rate achievement: {survivalRate:F2}");
                }
            }
        }
    }
    
}