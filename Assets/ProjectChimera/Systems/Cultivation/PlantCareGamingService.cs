using UnityEngine;
using System;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Construction; // For SkillLevel enum
using ProjectChimera.Core.Events; // For cultivation gaming event data
using ProjectChimera.Data.Events; // For non-generic GameEventChannelSO
using GameEventChannel = ProjectChimera.Data.Events.GameEventChannelSO;
using ProjectChimera.Data.Progression; // For ExperienceSource enum
using ExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Plant Care Gaming Service - Handles all interactive plant care mechanics and feedback systems
    /// Extracted from EnhancedCultivationGamingManager to provide focused plant care functionality
    /// Implements immediate visual/audio feedback, skill progression, and tool management
    /// </summary>
    public class PlantCareGamingService : MonoBehaviour
    {
        [Header("Interactive Plant Care Components")]
        [SerializeField] private InteractivePlantCareSystem _plantCareSystem;
        [SerializeField] private PlantInteractionController _interactionController;
        [SerializeField] private CareToolManager _careToolManager;
        
        [Header("Configuration")]
        [SerializeField] private EnhancedCultivationGamingConfigSO _cultivationGamingConfig;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventChannel _onPlantCarePerformed;
        
        // Dependencies
        private TreeSkillProgressionSystem _skillTreeSystem;
        private ManualTaskBurdenCalculator _burdenCalculator;
        private AutomationUnlockManager _automationUnlocks;
        
        // Performance Metrics
        private int _careActionsPerformed = 0;
        private float _sessionStartTime;
        
        // State
        private bool _isInitialized = false;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the plant care gaming service with dependencies
        /// </summary>
        public void Initialize(
            EnhancedCultivationGamingConfigSO config,
            TreeSkillProgressionSystem skillTreeSystem = null,
            ManualTaskBurdenCalculator burdenCalculator = null,
            AutomationUnlockManager automationUnlocks = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("PlantCareGamingService already initialized");
                return;
            }
            
            try
            {
                _cultivationGamingConfig = config;
                _skillTreeSystem = skillTreeSystem;
                _burdenCalculator = burdenCalculator;
                _automationUnlocks = automationUnlocks;
                
                ValidateConfiguration();
                InitializeInteractivePlantCare();
                RegisterEventHandlers();
                
                _sessionStartTime = Time.time;
                _isInitialized = true;
                
                Debug.Log("PlantCareGamingService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize PlantCareGamingService: {ex.Message}");
                throw;
            }
        }
        
        private void ValidateConfiguration()
        {
            if (_cultivationGamingConfig == null)
                throw new System.NullReferenceException("EnhancedCultivationGamingConfigSO is required");
                
            // Create plant care systems if not assigned
            if (_plantCareSystem == null) CreateInteractivePlantCareSystem();
        }
        
        private void InitializeInteractivePlantCare()
        {
            Debug.Log("Initializing Interactive Plant Care Systems...");
            
            _plantCareSystem?.Initialize(_cultivationGamingConfig?.InteractivePlantCareConfig);
            _interactionController?.Initialize(_cultivationGamingConfig?.PlantInteractionConfig);
            _careToolManager?.Initialize(_cultivationGamingConfig?.CareToolLibrary);
            
            Debug.Log("Interactive Plant Care Systems initialized");
        }
        
        private void CreateInteractivePlantCareSystem()
        {
            var systemGO = new GameObject("InteractivePlantCareSystem");
            systemGO.transform.SetParent(transform);
            _plantCareSystem = systemGO.AddComponent<InteractivePlantCareSystem>();
            
            var controllerGO = new GameObject("PlantInteractionController");
            controllerGO.transform.SetParent(systemGO.transform);
            _interactionController = controllerGO.AddComponent<PlantInteractionController>();
            
            var toolManagerGO = new GameObject("CareToolManager");
            toolManagerGO.transform.SetParent(systemGO.transform);
            _careToolManager = toolManagerGO.AddComponent<CareToolManager>();
        }
        
        #endregion
        
        #region Event System
        
        private void RegisterEventHandlers()
        {
            if (_onPlantCarePerformed != null)
                _onPlantCarePerformed.OnEventRaisedWithData.AddListener(OnPlantCarePerformed);
        }
        
        private void UnregisterEventHandlers()
        {
            if (_onPlantCarePerformed != null)
                _onPlantCarePerformed.OnEventRaisedWithData.RemoveListener(OnPlantCarePerformed);
        }
        
        private void OnPlantCarePerformed(object eventData)
        {
            _careActionsPerformed++;
            
            if (eventData is PlantCareEventData careData)
            {
                ProcessCareActionFeedback(careData);
                UpdateSkillProgression(careData);
                EvaluateAutomationDesire(careData);
            }
        }
        
        #endregion
        
        #region Core Plant Care Mechanics
        
        /// <summary>
        /// Perform a care action on a plant with specified tool and technique
        /// </summary>
        public PlantCareResult PerformPlantCare(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            if (!_isInitialized || plant == null || action == null)
                return PlantCareResult.Failed;
                
            var result = _plantCareSystem?.ProcessCareAction(plant, action) ?? PlantCareResult.Failed;
            
            // Create care event data for feedback processing
            var careEventData = new PlantCareEventData
            {
                PlantInstance = plant,
                CareType = action.TaskType.ToString(),
                CareQuality = ConvertResultToQuality(result).ToString(),
                Timestamp = Time.time
            };
            
            // Trigger care performed event
            _onPlantCarePerformed?.RaiseEvent(careEventData);
            
            return result;
        }
        
        private void ProcessCareActionFeedback(PlantCareEventData careData)
        {
            // Provide immediate visual and audio feedback for care actions
            var careQuality = ParseCareQuality(careData.CareQuality);
            var careType = ParseCultivationTaskType(careData.CareType);
            var feedbackIntensity = CalculateFeedbackIntensity(careQuality);
            
            // Safe cast for PlantInstance
            if (careData.PlantInstance is InteractivePlant plant)
            {
                TriggerVisualFeedback(plant, feedbackIntensity);
            }
            TriggerAudioFeedback(careType, feedbackIntensity);
        }
        
        private void UpdateSkillProgression(PlantCareEventData careData)
        {
            if (_skillTreeSystem == null) return;
            
            var careQuality = ParseCareQuality(careData.CareQuality);
            var careType = ParseCultivationTaskType(careData.CareType);
            var skillGain = CalculateSkillGain(careType, careQuality);
            
            _skillTreeSystem.AddSkillExperience(careData.CareType, skillGain, ExperienceSource.PlantCare);
        }
        
        private void EvaluateAutomationDesire(PlantCareEventData careData)
        {
            if (_burdenCalculator == null || _automationUnlocks == null) return;
            
            var currentBurden = _burdenCalculator.CalculateCurrentBurden(careData);
            if (currentBurden == AutomationDesireLevel.High || currentBurden == AutomationDesireLevel.Critical)
            {
                var careType = ParseCultivationTaskType(careData.CareType);
                _automationUnlocks.EvaluateUnlockEligibility(careType);
            }
        }
        
        #endregion
        
        #region Visual and Audio Feedback
        
        private void TriggerVisualFeedback(InteractivePlant plant, float intensity)
        {
            // Implementation for visual feedback system
            // This would integrate with plant visualization and effects systems
            
            // Example implementation:
            // - Particle effects for successful care actions
            // - Color changes based on care quality
            // - Animation triggers for plant response
            
            if (plant == null) return;
            
            var plantRenderer = plant.GetComponent<Renderer>();
            if (plantRenderer != null)
            {
                // Apply visual feedback based on intensity
                var feedbackColor = GetFeedbackColor(intensity);
                // Could implement temporary color overlay or particle effects here
            }
            
            // Log visual feedback for debugging
            Debug.Log($"Visual feedback triggered for plant {plant.name} with intensity {intensity:F2}");
        }
        
        private void TriggerAudioFeedback(CultivationTaskType taskType, float intensity)
        {
            // Implementation for audio feedback system
            // This would play appropriate sounds based on care action and quality
            
            // Example implementation:
            // - Watering sounds for watering actions
            // - Pruning sounds for trimming actions
            // - Success/failure audio cues based on intensity
            
            // Log audio feedback for debugging
            Debug.Log($"Audio feedback triggered for {taskType} with intensity {intensity:F2}");
        }
        
        private Color GetFeedbackColor(float intensity)
        {
            // Convert intensity to color for visual feedback
            if (intensity >= 0.8f) return Color.green;      // Excellent care
            if (intensity >= 0.6f) return Color.yellow;     // Good care
            if (intensity >= 0.4f) return Color.white;      // Average care
            if (intensity >= 0.2f) return Color.orange;     // Poor care
            return Color.red;                                // Failed care
        }
        
        #endregion
        
        #region Tool Management
        
        /// <summary>
        /// Get available care tools for specified task type
        /// </summary>
        public CareAction[] GetAvailableTools(CultivationTaskType taskType)
        {
            return _careToolManager?.GetAvailableTools(taskType) ?? new CareAction[0];
        }
        
        /// <summary>
        /// Check if a specific tool is available and unlocked
        /// </summary>
        public bool IsToolAvailable(CareAction tool)
        {
            return _careToolManager?.IsToolAvailable(tool) ?? false;
        }
        
        /// <summary>
        /// Unlock new mechanics based on skill progression
        /// </summary>
        public void UnlockNewMechanics(SkillNodeType nodeType)
        {
            _plantCareSystem?.UnlockNewMechanics(nodeType);
        }
        
        /// <summary>
        /// Adjust task complexity based on game state
        /// </summary>
        public void AdjustTaskComplexity(float complexityMultiplier)
        {
            _plantCareSystem?.AdjustTaskComplexity(complexityMultiplier);
        }
        
        #endregion
        
        #region Performance Metrics
        
        /// <summary>
        /// Get plant care performance metrics for current session
        /// </summary>
        public PlantCareMetrics GetCareMetrics()
        {
            return new PlantCareMetrics
            {
                SessionDuration = Time.time - _sessionStartTime,
                CareActionsPerformed = _careActionsPerformed,
                ActionsPerMinute = CalculateActionsPerMinute(),
                EngagementLevel = CalculateEngagementLevel()
            };
        }
        
        private float CalculateActionsPerMinute()
        {
            var sessionDuration = Time.time - _sessionStartTime;
            return sessionDuration > 0 ? (_careActionsPerformed / (sessionDuration / 60f)) : 0;
        }
        
        private float CalculateEngagementLevel()
        {
            var actionsPerMinute = CalculateActionsPerMinute();
            return Mathf.Clamp01(actionsPerMinute / 10f); // Normalize to 0-1 based on 10 actions per minute target
        }
        
        #endregion
        
        #region Utility Methods
        
        private float CalculateFeedbackIntensity(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Perfect => 1.0f,
                CareQuality.Excellent => 0.8f,
                CareQuality.Good => 0.6f,
                CareQuality.Average => 0.4f,
                CareQuality.Poor => 0.2f,
                _ => 0.1f
            };
        }
        
        private float CalculateSkillGain(CultivationTaskType taskType, CareQuality quality)
        {
            var baseGain = _cultivationGamingConfig?.SkillProgressionRate ?? 1.0f;
            var qualityMultiplier = CalculateFeedbackIntensity(quality);
            return baseGain * qualityMultiplier;
        }
        
        private CareQuality ConvertResultToQuality(PlantCareResult result)
        {
            return result switch
            {
                PlantCareResult.Perfect => CareQuality.Perfect,
                PlantCareResult.Successful => CareQuality.Excellent,
                PlantCareResult.Adequate => CareQuality.Average,
                PlantCareResult.Suboptimal => CareQuality.Poor,
                PlantCareResult.Failed => CareQuality.Failed,
                _ => CareQuality.Average
            };
        }
        
        // Helper methods to convert string properties to enums for compatibility
        private CareQuality ParseCareQuality(string careQualityString)
        {
            if (System.Enum.TryParse<CareQuality>(careQualityString, true, out var result))
            {
                return result;
            }
            return CareQuality.Average; // Default fallback
        }
        
        private CultivationTaskType ParseCultivationTaskType(string taskTypeString)
        {
            if (System.Enum.TryParse<CultivationTaskType>(taskTypeString, true, out var result))
            {
                return result;
            }
            return CultivationTaskType.Watering; // Default fallback
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!_isInitialized) return;
            
            // Update plant care system
            _plantCareSystem?.UpdateSystem(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            UnregisterEventHandlers();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Plant care performance metrics
    /// </summary>
    [System.Serializable]
    public class PlantCareMetrics
    {
        public float SessionDuration;
        public int CareActionsPerformed;
        public float ActionsPerMinute;
        public float EngagementLevel;
    }
    
    /// <summary>
    /// Plant care result enum for action outcomes
    /// </summary>
    public enum PlantCareResult
    {
        Perfect,
        Successful,
        Adequate,
        Suboptimal,
        Failed
    }
    
    /// <summary>
    /// Care quality enum for skill progression and feedback
    /// </summary>
    public enum CareQuality
    {
        Failed,
        Poor,
        Adequate,
        Average,
        Good,
        Excellent,
        Perfect
    }
    
    #endregion
}