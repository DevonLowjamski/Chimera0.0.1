using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Calculates the burden associated with manual cultivation tasks
    /// </summary>
    public class ManualTaskBurdenCalculator : MonoBehaviour
    {
        [Header("Burden Calculation Settings")]
        [SerializeField] private float _baseBurdenMultiplier = 1.0f;
        [SerializeField] private float _skillLevelReduction = 0.1f;
        [SerializeField] private float _frequencyMultiplier = 1.2f;
        [SerializeField] private float _complexityWeight = 0.8f;
        
        private bool _isInitialized = false;
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("ManualTaskBurdenCalculator already initialized", this);
                return;
            }
            
            _isInitialized = true;
            ChimeraLogger.Log("ManualTaskBurdenCalculator initialized successfully", this);
        }
        
        /// <summary>
        /// Calculate the burden for a specific manual task
        /// </summary>
        public float CalculateTaskBurden(CultivationTaskType taskType, float skillLevel, float frequency, float complexity)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("ManualTaskBurdenCalculator not initialized", this);
                return 0f;
            }
            
            // Base burden calculation
            float baseBurden = GetBaseBurden(taskType);
            
            // Apply skill level reduction
            float skillReduction = Mathf.Clamp01(skillLevel * _skillLevelReduction);
            
            // Apply frequency multiplier
            float frequencyBurden = frequency * _frequencyMultiplier;
            
            // Apply complexity weight
            float complexityBurden = complexity * _complexityWeight;
            
            // Calculate total burden
            float totalBurden = (baseBurden + frequencyBurden + complexityBurden) * (1f - skillReduction) * _baseBurdenMultiplier;
            
            return Mathf.Max(0f, totalBurden);
        }
        
        /// <summary>
        /// Get the base burden value for a task type
        /// </summary>
        private float GetBaseBurden(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => 2.0f,
                CultivationTaskType.Feeding => 3.0f,
                CultivationTaskType.Pruning => 4.0f,
                CultivationTaskType.Training => 3.5f,
                CultivationTaskType.Harvesting => 2.5f,
                CultivationTaskType.Monitoring => 1.5f,
                CultivationTaskType.EnvironmentalControl => 3.0f,
                CultivationTaskType.PestControl => 4.5f,
                CultivationTaskType.Transplanting => 3.5f,
                CultivationTaskType.Defoliation => 2.8f,
                _ => 2.0f
            };
        }
        
        /// <summary>
        /// Calculate burden reduction from automation
        /// </summary>
        public float CalculateAutomationBurdenReduction(CultivationTaskType taskType, float automationLevel)
        {
            float maxReduction = GetMaxAutomationReduction(taskType);
            return maxReduction * Mathf.Clamp01(automationLevel);
        }
        
        /// <summary>
        /// Get maximum possible burden reduction from automation for a task type
        /// </summary>
        private float GetMaxAutomationReduction(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => 0.9f,
                CultivationTaskType.Feeding => 0.8f,
                CultivationTaskType.Pruning => 0.3f,
                CultivationTaskType.Training => 0.2f,
                CultivationTaskType.Harvesting => 0.4f,
                CultivationTaskType.Monitoring => 0.95f,
                CultivationTaskType.EnvironmentalControl => 0.9f,
                CultivationTaskType.PestControl => 0.6f,
                CultivationTaskType.Transplanting => 0.1f,
                CultivationTaskType.Defoliation => 0.2f,
                _ => 0.5f
            };
        }
        
        /// <summary>
        /// Calculate current burden for plant care event data
        /// </summary>
        public AutomationDesireLevel CalculateCurrentBurden(PlantCareEventData careData)
        {
            if (!_isInitialized || careData == null)
                return AutomationDesireLevel.None;
                
            var careType = ParseCultivationTaskType(careData.CareType);
            float burden = CalculateTaskBurden(careType, 1.0f, 1.0f, 1.0f);
            
            if (burden > 0.8f) return AutomationDesireLevel.Critical;
            if (burden > 0.6f) return AutomationDesireLevel.High;
            if (burden > 0.4f) return AutomationDesireLevel.Medium;
            if (burden > 0.2f) return AutomationDesireLevel.Low;
            return AutomationDesireLevel.None;
        }
        
        /// <summary>
        /// Get automation desire for specific task type
        /// </summary>
        public AutomationDesireLevel GetAutomationDesire(CultivationTaskType taskType)
        {
            if (!_isInitialized)
                return AutomationDesireLevel.None;
                
            float burden = CalculateTaskBurden(taskType, 1.0f, 1.0f, 1.0f);
            
            if (burden > 0.8f) return AutomationDesireLevel.Critical;
            if (burden > 0.6f) return AutomationDesireLevel.High;
            if (burden > 0.4f) return AutomationDesireLevel.Medium;
            if (burden > 0.2f) return AutomationDesireLevel.Low;
            return AutomationDesireLevel.None;
        }
        
        /// <summary>
        /// Update burden calculations with complexity multiplier
        /// </summary>
        public void UpdateBurdenCalculations(float complexityMultiplier)
        {
            if (!_isInitialized) return;
            
            _complexityWeight = Mathf.Clamp(complexityMultiplier, 0.1f, 3.0f);
            ChimeraLogger.Log($"Updated burden calculations with complexity multiplier: {complexityMultiplier}", this);
        }
        
        /// <summary>
        /// Helper method to convert string to CultivationTaskType enum
        /// </summary>
        private CultivationTaskType ParseCultivationTaskType(string taskTypeString)
        {
            if (System.Enum.TryParse<CultivationTaskType>(taskTypeString, true, out var result))
            {
                return result;
            }
            return CultivationTaskType.Watering; // Default fallback
        }
    }
} 