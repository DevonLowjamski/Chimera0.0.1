using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

// Use type alias to match the PlantInstance type used by cultivation services
using PlantInstance = ProjectChimera.Systems.Cultivation.PlantInstance;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Integration layer that connects existing plant services with the gaming event bus.
    /// Part of Module 1: Gaming Experience Core - Deliverable 1.3
    /// 
    /// This service acts as a bridge between the cultivation systems and gaming events,
    /// ensuring that significant cultivation actions trigger appropriate gaming events
    /// for player feedback, achievements, and analytics.
    /// </summary>
    public class ServiceEventIntegration : ChimeraManager, IServiceEventIntegration
    {
        [Header("Integration Configuration")]
        [SerializeField] private bool _enablePlantLifecycleEvents = true;
        [SerializeField] private bool _enableEnvironmentalEvents = true;
        [SerializeField] private bool _enableHarvestEvents = true;
        [SerializeField] private bool _enableAchievementEvents = true;
        [SerializeField] private bool _enableGeneticsEvents = true;
        
        [Header("Event Filtering")]
        [SerializeField] private float _minimumHealthChangeThreshold = 0.05f;
        [SerializeField] private float _minimumStressChangeThreshold = 0.1f;
        [SerializeField] private bool _enableDebugLogging = false;
        
        [Header("Dependencies")]
        [SerializeField] private GameEventBus _gameEventBus;
        
        // Service references - will be injected or found at runtime
        private IPlantLifecycleService _plantLifecycleService;
        private IPlantGrowthService _plantGrowthService;
        private IPlantEnvironmentalService _plantEnvironmentalService;
        private IPlantHarvestService _plantHarvestService;
        private IPlantAchievementService _plantAchievementService;
        private IPlantGeneticsService _plantGeneticsService;
        
        // Event tracking for filtering duplicate events
        private PlantEventTracker _eventTracker;
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeServiceReferences();
            InitializeEventTracker();
            SubscribeToServiceEvents();
            
            Debug.Log($"[ServiceEventIntegration] Service event integration initialized");
            Debug.Log($"   - Plant Lifecycle Events: {_enablePlantLifecycleEvents}");
            Debug.Log($"   - Environmental Events: {_enableEnvironmentalEvents}");
            Debug.Log($"   - Harvest Events: {_enableHarvestEvents}");
            Debug.Log($"   - Achievement Events: {_enableAchievementEvents}");
            Debug.Log($"   - Genetics Events: {_enableGeneticsEvents}");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            // Update event tracker for filtering
            _eventTracker?.Update();
        }
        
        protected override void OnManagerShutdown()
        {
            UnsubscribeFromServiceEvents();
            _eventTracker?.Cleanup();
            
            Debug.Log("[ServiceEventIntegration] Service event integration shutdown complete");
        }
        
        #endregion
        
        #region Service Integration
        
        /// <summary>
        /// Initialize references to existing cultivation services
        /// </summary>
        private void InitializeServiceReferences()
        {
            // Find GameEventBus if not assigned
            if (_gameEventBus == null)
            {
                _gameEventBus = FindObjectOfType<GameEventBus>();
                if (_gameEventBus == null)
                {
                    Debug.LogError("[ServiceEventIntegration] GameEventBus not found! Gaming events will not be processed.");
                    return;
                }
            }
            
            // Find existing cultivation services
            // Note: In a proper DI system, these would be injected
            FindCultivationServices();
        }
        
        /// <summary>
        /// Find existing cultivation services in the scene
        /// </summary>
        private void FindCultivationServices()
        {
            // Try to find services via common patterns
            var cultivationManager = FindObjectOfType<CultivationManager>();
            if (cultivationManager != null)
            {
                // Access services through CultivationManager if available
                TryGetServicesFromCultivationManager(cultivationManager);
            }
            else
            {
                // Fallback: look for individual service components
                TryFindIndividualServices();
            }
        }
        
        /// <summary>
        /// Get service references from CultivationManager
        /// </summary>
        private void TryGetServicesFromCultivationManager(CultivationManager cultivationManager)
        {
            // CultivationManager may have direct references or factory methods
            // This is a placeholder for the actual integration pattern
            Debug.Log("[ServiceEventIntegration] Found CultivationManager, attempting service integration");
            
            // TODO: Implement actual service retrieval from CultivationManager
            // This would depend on how services are exposed by CultivationManager
        }
        
        /// <summary>
        /// Find individual service components as fallback
        /// </summary>
        private void TryFindIndividualServices()
        {
            // Look for service components in the scene
            var services = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var service in services)
            {
                if (service is IPlantLifecycleService lifecycleService)
                    _plantLifecycleService = lifecycleService;
                else if (service is IPlantGrowthService growthService)
                    _plantGrowthService = growthService;
                else if (service is IPlantEnvironmentalService environmentalService)
                    _plantEnvironmentalService = environmentalService;
                else if (service is IPlantHarvestService harvestService)
                    _plantHarvestService = harvestService;
                else if (service is IPlantAchievementService achievementService)
                    _plantAchievementService = achievementService;
                else if (service is IPlantGeneticsService geneticsService)
                    _plantGeneticsService = geneticsService;
            }
            
            Debug.Log($"[ServiceEventIntegration] Found services - Lifecycle: {_plantLifecycleService != null}, " +
                     $"Growth: {_plantGrowthService != null}, Environmental: {_plantEnvironmentalService != null}, " +
                     $"Harvest: {_plantHarvestService != null}, Achievement: {_plantAchievementService != null}, " +
                     $"Genetics: {_plantGeneticsService != null}");
        }
        
        #endregion
        
        #region Event Subscription
        
        /// <summary>
        /// Subscribe to events from existing cultivation services
        /// </summary>
        private void SubscribeToServiceEvents()
        {
            // Use polling approach instead of direct event subscription to avoid type compatibility issues
            // Start coroutine for periodic service monitoring
            StartCoroutine(MonitorServiceChanges());
            
            if (_enableDebugLogging)
            {
                Debug.Log("[ServiceEventIntegration] Service monitoring started (polling mode)");
            }
        }
        
        /// <summary>
        /// Unsubscribe from service events
        /// </summary>
        private void UnsubscribeFromServiceEvents()
        {
            // Stop the monitoring coroutine
            StopAllCoroutines();
            
            if (_enableDebugLogging)
            {
                Debug.Log("[ServiceEventIntegration] Service monitoring stopped");
            }
        }
        
        /// <summary>
        /// Monitor service changes using coroutine-based polling
        /// </summary>
        private System.Collections.IEnumerator MonitorServiceChanges()
        {
            var lastPlantCounts = new Dictionary<string, int>();
            var lastHealthValues = new Dictionary<string, float>();
            var lastStageValues = new Dictionary<string, string>();
            
            while (enabled && gameObject.activeInHierarchy)
            {
                try
                {
                    if (_plantLifecycleService != null && _plantLifecycleService.IsInitialized)
                    {
                        // Monitor tracked plants for changes
                        var currentPlants = _plantLifecycleService.GetTrackedPlants();
                        
                        foreach (var plant in currentPlants)
                        {
                            if (plant == null || !plant.IsActive) continue;
                            
                            var plantId = GetPlantId(plant);
                            var currentHealth = GetPlantHealth(plant);
                            var currentStage = GetPlantGrowthStage(plant);
                            
                            // Check for new plants
                            if (!lastPlantCounts.ContainsKey(plantId))
                            {
                                lastPlantCounts[plantId] = 1;
                                lastHealthValues[plantId] = currentHealth;
                                lastStageValues[plantId] = currentStage;
                                
                                // Trigger plant added event
                                OnPlantAdded(plant);
                                continue;
                            }
                            
                            // Check for health changes
                            if (lastHealthValues.TryGetValue(plantId, out var lastHealth))
                            {
                                var healthChange = Mathf.Abs(currentHealth - lastHealth);
                                if (healthChange >= _minimumHealthChangeThreshold)
                                {
                                    lastHealthValues[plantId] = currentHealth;
                                    OnPlantHealthUpdated(plant);
                                }
                            }
                            
                            // Check for stage changes
                            if (lastStageValues.TryGetValue(plantId, out var lastStage))
                            {
                                if (lastStage != currentStage)
                                {
                                    lastStageValues[plantId] = currentStage;
                                    OnPlantStageChanged(plant);
                                }
                            }
                        }
                        
                        // Clean up tracking for removed plants
                        var currentPlantIds = new HashSet<string>(currentPlants.Where(p => p != null).Select(p => GetPlantId(p)));
                        var keysToRemove = lastPlantCounts.Keys.Where(id => !currentPlantIds.Contains(id)).ToList();
                        
                        foreach (var keyToRemove in keysToRemove)
                        {
                            lastPlantCounts.Remove(keyToRemove);
                            lastHealthValues.Remove(keyToRemove);
                            lastStageValues.Remove(keyToRemove);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ServiceEventIntegration] Error during service monitoring: {ex.Message}");
                }
                
                // Wait before next check (adjust frequency as needed)
                yield return new WaitForSeconds(1.0f); // Check every second
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle plant creation events - using generic object parameter to avoid type conflicts
        /// </summary>
        private void OnPlantAdded(object plant)
        {
            if (!_enablePlantLifecycleEvents || plant == null) return;
            
            // Create plant creation event - using safe property access
            var plantCreatedEvent = new PlantCreatedEvent
            {
                PlantId = GetPlantId(plant),
                PlantStrain = GetPlantStrain(plant),
                PlayerId = "PLAYER", // TODO: Get actual player ID
                CreationLocation = GetPlantPosition(plant),
                InitialHealth = GetPlantHealth(plant),
                GrowthStage = GetPlantGrowthStage(plant)
            };
            
            _gameEventBus.PublishImmediate(plantCreatedEvent);
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[ServiceEventIntegration] Plant created: {plantCreatedEvent.PlantId} ({plantCreatedEvent.PlantStrain})");
            }
        }
        
        /// <summary>
        /// Handle plant growth stage changes - using generic object parameter to avoid type conflicts
        /// </summary>
        private void OnPlantStageChanged(object plant)
        {
            if (!_enablePlantLifecycleEvents || plant == null) return;
            
            var plantId = GetPlantId(plant);
            var currentStage = GetPlantGrowthStage(plant);
            
            // Get previous stage from event tracker
            var previousStage = _eventTracker.GetPreviousStage(plantId);
            
            var growthEvent = new PlantGrowthEvent
            {
                PlantId = plantId,
                PreviousStage = previousStage,
                NewStage = currentStage,
                GrowthProgress = CalculateGrowthProgress(plant),
                ShouldCelebrate = IsSignificantGrowthMilestone(currentStage),
                PlantHealth = GetPlantHealth(plant),
                PlayerId = "PLAYER" // TODO: Get actual player ID
            };
            
            // Add environmental conditions if available
            AddEnvironmentalConditions(growthEvent, plant);
            
            _gameEventBus.PublishImmediate(growthEvent);
            
            // Update event tracker
            _eventTracker.UpdatePlantStage(plantId, currentStage);
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[ServiceEventIntegration] Plant growth: {plantId} {previousStage} -> {currentStage}");
            }
        }
        
        /// <summary>
        /// Handle plant health updates - using generic object parameter to avoid type conflicts
        /// </summary>
        private void OnPlantHealthUpdated(object plant)
        {
            if (!_enablePlantLifecycleEvents || plant == null) return;
            
            var plantId = GetPlantId(plant);
            var currentHealth = GetPlantHealth(plant);
            var previousHealth = _eventTracker.GetPreviousHealth(plantId);
            var healthChange = Mathf.Abs(currentHealth - previousHealth);
            
            // Only trigger event if health change is significant
            if (healthChange >= _minimumHealthChangeThreshold)
            {
                var healthEvent = new PlantHealthEvent
                {
                    PlantId = plantId,
                    PreviousHealth = previousHealth,
                    CurrentHealth = currentHealth,
                    HealthChangeReason = DetermineHealthChangeReason(plant, previousHealth),
                    RequiresPlayerAttention = currentHealth < 0.5f,
                    RecommendedAction = GetHealthRecommendation(plant),
                    PlayerId = "PLAYER" // TODO: Get actual player ID
                };
                
                _gameEventBus.PublishImmediate(healthEvent);
                
                // Update event tracker
                _eventTracker.UpdatePlantHealth(plantId, currentHealth);
                
                if (_enableDebugLogging)
                {
                    Debug.Log($"[ServiceEventIntegration] Plant health change: {plantId} {previousHealth:F2} -> {currentHealth:F2}");
                }
            }
        }
        
        /// <summary>
        /// Handle plant harvest events - using generic object parameter to avoid type conflicts
        /// </summary>
        private void OnPlantHarvested(object plant)
        {
            if (!_enableHarvestEvents || plant == null) return;
            
            var plantId = GetPlantId(plant);
            var plantStrain = GetPlantStrain(plant);
            
            // TODO: Get actual harvest results from the harvest service
            var harvestEvent = new PlantHarvestedEvent
            {
                PlantId = plantId,
                PlantStrain = plantStrain,
                ActualYield = 0f, // TODO: Get from harvest results
                QualityGrade = GetPlantHealth(plant), // Placeholder
                IsRecordBreaking = false, // TODO: Implement record tracking
                PlayerId = "PLAYER" // TODO: Get actual player ID
            };
            
            _gameEventBus.PublishImmediate(harvestEvent);
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[ServiceEventIntegration] Plant harvested: {plantId} ({plantStrain})");
            }
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Manually trigger a plant interaction event
        /// </summary>
        public void TriggerPlantInteraction(string plantId, string interactionType, bool wasSuccessful, string feedbackMessage = "")
        {
            var interactionEvent = new PlantInteractionEvent
            {
                PlantId = plantId,
                InteractionType = interactionType,
                WasSuccessful = wasSuccessful,
                FeedbackMessage = feedbackMessage,
                PlayerId = "PLAYER" // TODO: Get actual player ID
            };
            
            _gameEventBus.PublishImmediate(interactionEvent);
        }
        
        /// <summary>
        /// Manually trigger an environmental change event
        /// </summary>
        public void TriggerEnvironmentalChange(string zone, string parameter, float previousValue, float newValue, float optimalValue)
        {
            if (!_enableEnvironmentalEvents) return;
            
            var environmentalEvent = new EnvironmentalChangeEvent
            {
                EnvironmentalZone = zone,
                ParameterChanged = parameter,
                PreviousValue = previousValue,
                NewValue = newValue,
                OptimalValue = optimalValue,
                IsWithinOptimalRange = Mathf.Abs(newValue - optimalValue) < 0.1f,
                ChangeSource = "Player",
                PlayerId = "PLAYER" // TODO: Get actual player ID
            };
            
            _gameEventBus.Publish(environmentalEvent);
        }
        
        /// <summary>
        /// Get integration status and diagnostics
        /// </summary>
        public ServiceIntegrationStatus GetIntegrationStatus()
        {
            return new ServiceIntegrationStatus
            {
                GameEventBusConnected = _gameEventBus != null,
                PlantLifecycleServiceConnected = _plantLifecycleService != null,
                PlantGrowthServiceConnected = _plantGrowthService != null,
                PlantEnvironmentalServiceConnected = _plantEnvironmentalService != null,
                PlantHarvestServiceConnected = _plantHarvestService != null,
                PlantAchievementServiceConnected = _plantAchievementService != null,
                PlantGeneticsServiceConnected = _plantGeneticsService != null,
                EventsPublishedCount = _eventTracker?.TotalEventsPublished ?? 0
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        private void InitializeEventTracker()
        {
            _eventTracker = new PlantEventTracker();
        }
        
        private float CalculateGrowthProgress(object plant)
        {
            // TODO: Implement actual growth progress calculation
            // This would depend on the plant's stage duration and time in current stage
            return 1.0f; // Placeholder - assume stage completed
        }
        
        private bool IsSignificantGrowthMilestone(string stage)
        {
            // Determine which growth stages should trigger celebrations
            return stage == "Flowering" || stage == "Harvest" || stage == "Flowering" || stage == "Mature";
        }
        
        private void AddEnvironmentalConditions(PlantGrowthEvent growthEvent, object plant)
        {
            // TODO: Get actual environmental conditions from environmental service
            // For now, add placeholder conditions
            growthEvent.EnvironmentalConditions.Add("Temperature", 24.0f);
            growthEvent.EnvironmentalConditions.Add("Humidity", 0.65f);
            growthEvent.EnvironmentalConditions.Add("Light", 800f);
        }
        
        private string DetermineHealthChangeReason(object plant, float previousHealth)
        {
            // TODO: Implement logic to determine why health changed
            var currentHealth = GetPlantHealth(plant);
            if (currentHealth > previousHealth)
                return "Recovery";
            else
                return "Environmental Stress";
        }
        
        private string GetHealthRecommendation(object plant)
        {
            // TODO: Implement intelligent health recommendations
            var currentHealth = GetPlantHealth(plant);
            if (currentHealth < 0.3f)
                return "Check environmental conditions and consider treatment";
            else if (currentHealth < 0.7f)
                return "Monitor plant closely and adjust care";
            else
                return "Plant is healthy";
        }
        
        /// <summary>
        /// Safe property accessor methods for any PlantInstance type
        /// These methods handle the different PlantInstance implementations that may exist
        /// </summary>
        private string GetPlantId(object plant)
        {
            if (plant == null) return "Unknown";
            
            // Try different property names that might exist
            var type = plant.GetType();
            
            // Try PlantID
            var plantIdProperty = type.GetProperty("PlantID");
            if (plantIdProperty != null)
                return plantIdProperty.GetValue(plant)?.ToString() ?? "Unknown";
                
            // Try PlantId
            var plantIdField = type.GetField("PlantId");
            if (plantIdField != null)
                return plantIdField.GetValue(plant)?.ToString() ?? "Unknown";
                
            // Fallback
            return plant.GetHashCode().ToString();
        }
        
        private string GetPlantStrain(object plant)
        {
            if (plant == null) return "Unknown";
            
            var type = plant.GetType();
            
            // Try Strain property
            var strainProperty = type.GetProperty("Strain");
            if (strainProperty != null)
            {
                var strain = strainProperty.GetValue(plant);
                if (strain != null)
                {
                    var strainNameProperty = strain.GetType().GetProperty("StrainName");
                    if (strainNameProperty != null)
                        return strainNameProperty.GetValue(strain)?.ToString() ?? "Unknown";
                }
            }
            
            // Try StrainName field
            var strainNameField = type.GetField("StrainName");
            if (strainNameField != null)
                return strainNameField.GetValue(plant)?.ToString() ?? "Unknown";
                
            return "Unknown";
        }
        
        private float GetPlantHealth(object plant)
        {
            if (plant == null) return 1.0f;
            
            var type = plant.GetType();
            
            // Try CurrentHealth property
            var currentHealthProperty = type.GetProperty("CurrentHealth");
            if (currentHealthProperty != null)
            {
                var value = currentHealthProperty.GetValue(plant);
                if (value is float floatValue)
                    return floatValue;
            }
            
            // Try Health field
            var healthField = type.GetField("Health");
            if (healthField != null)
            {
                var value = healthField.GetValue(plant);
                if (value is float floatValue)
                    return floatValue;
            }
            
            return 1.0f; // Default healthy
        }
        
        private string GetPlantGrowthStage(object plant)
        {
            if (plant == null) return "Unknown";
            
            var type = plant.GetType();
            
            // Try CurrentGrowthStage property
            var currentStageProperty = type.GetProperty("CurrentGrowthStage");
            if (currentStageProperty != null)
                return currentStageProperty.GetValue(plant)?.ToString() ?? "Unknown";
                
            // Try GrowthStage field
            var stageField = type.GetField("GrowthStage");
            if (stageField != null)
                return stageField.GetValue(plant)?.ToString() ?? "Unknown";
                
            return "Unknown";
        }
        
        private Vector3 GetPlantPosition(object plant)
        {
            if (plant == null) return Vector3.zero;
            
            var type = plant.GetType();
            
            // Try Position field first
            var positionField = type.GetField("Position");
            if (positionField != null)
            {
                var value = positionField.GetValue(plant);
                if (value is Vector3 vectorValue)
                    return vectorValue;
            }
            
            // Try transform property if this derives from MonoBehaviour
            var transformProperty = type.GetProperty("transform");
            if (transformProperty != null)
            {
                var transform = transformProperty.GetValue(plant) as Transform;
                if (transform != null)
                    return transform.position;
            }
            
            return Vector3.zero;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for service event integration
    /// </summary>
    public interface IServiceEventIntegration
    {
        void TriggerPlantInteraction(string plantId, string interactionType, bool wasSuccessful, string feedbackMessage = "");
        void TriggerEnvironmentalChange(string zone, string parameter, float previousValue, float newValue, float optimalValue);
        ServiceIntegrationStatus GetIntegrationStatus();
    }
    
    /// <summary>
    /// Plant creation event (new event type for integration)
    /// </summary>
    [Serializable]
    public class PlantCreatedEvent : GameEvent
    {
        public string PlantId { get; set; }
        public string PlantStrain { get; set; }
        public Vector3 CreationLocation { get; set; }
        public float InitialHealth { get; set; }
        public string GrowthStage { get; set; }
        
        public PlantCreatedEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "PlantLifecycle";
        }
    }
    
    /// <summary>
    /// Status of service integration connections
    /// </summary>
    [Serializable]
    public struct ServiceIntegrationStatus
    {
        public bool GameEventBusConnected;
        public bool PlantLifecycleServiceConnected;
        public bool PlantGrowthServiceConnected;
        public bool PlantEnvironmentalServiceConnected;
        public bool PlantHarvestServiceConnected;
        public bool PlantAchievementServiceConnected;
        public bool PlantGeneticsServiceConnected;
        public int EventsPublishedCount;
        
        public bool IsFullyConnected => GameEventBusConnected && 
                                       PlantLifecycleServiceConnected && 
                                       PlantGrowthServiceConnected;
        
        public float ConnectionPercentage
        {
            get
            {
                int connectedServices = 0;
                int totalServices = 7;
                
                if (GameEventBusConnected) connectedServices++;
                if (PlantLifecycleServiceConnected) connectedServices++;
                if (PlantGrowthServiceConnected) connectedServices++;
                if (PlantEnvironmentalServiceConnected) connectedServices++;
                if (PlantHarvestServiceConnected) connectedServices++;
                if (PlantAchievementServiceConnected) connectedServices++;
                if (PlantGeneticsServiceConnected) connectedServices++;
                
                return (float)connectedServices / totalServices;
            }
        }
    }
    
    /// <summary>
    /// Event tracker for filtering and state management
    /// </summary>
    internal class PlantEventTracker
    {
        private System.Collections.Generic.Dictionary<string, string> _previousStages = new System.Collections.Generic.Dictionary<string, string>();
        private System.Collections.Generic.Dictionary<string, float> _previousHealth = new System.Collections.Generic.Dictionary<string, float>();
        
        public int TotalEventsPublished { get; private set; }
        
        public void Update()
        {
            // Periodic cleanup of old plant data
            // TODO: Implement cleanup logic for removed plants
        }
        
        public void Cleanup()
        {
            _previousStages.Clear();
            _previousHealth.Clear();
        }
        
        public string GetPreviousStage(string plantId)
        {
            return _previousStages.TryGetValue(plantId, out string stage) ? stage : "Unknown";
        }
        
        public void UpdatePlantStage(string plantId, string stage)
        {
            _previousStages[plantId] = stage;
            TotalEventsPublished++;
        }
        
        public float GetPreviousHealth(string plantId)
        {
            return _previousHealth.TryGetValue(plantId, out float health) ? health : 1.0f;
        }
        
        public void UpdatePlantHealth(string plantId, float health)
        {
            _previousHealth[plantId] = health;
            TotalEventsPublished++;
        }
    }
}