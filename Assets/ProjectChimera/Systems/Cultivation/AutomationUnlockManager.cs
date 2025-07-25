using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Manages automation unlocks based on player progress and burden thresholds
    /// </summary>
    public class AutomationUnlockManager : MonoBehaviour
    {
        [Header("Unlock Configuration")]
        [SerializeField] private AutomationUnlockLibrarySO _unlockLibrary;
        [SerializeField] private float _unlockCooldownTime = 5.0f;
        
        private bool _isInitialized = false;
        private Dictionary<CultivationTaskType, float> _lastUnlockTimes = new Dictionary<CultivationTaskType, float>();
        private Dictionary<AutomationSystemType, bool> _unlockedSystems = new Dictionary<AutomationSystemType, bool>();
        
        public void Initialize()
        {
            Initialize(null);
        }
        
        public void Initialize(AutomationUnlockLibrarySO unlockLibrary = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AutomationUnlockManager already initialized", this);
                return;
            }
            
            _unlockLibrary = unlockLibrary ?? _unlockLibrary;
            
            if (_unlockLibrary == null)
            {
                ChimeraLogger.LogError("AutomationUnlockLibrarySO is required for initialization", this);
                return;
            }
            
            InitializeUnlockStates();
            
            _isInitialized = true;
            ChimeraLogger.Log("AutomationUnlockManager initialized successfully", this);
        }
        
        private void InitializeUnlockStates()
        {
            // Initialize all automation systems as locked
            foreach (AutomationSystemType systemType in System.Enum.GetValues(typeof(AutomationSystemType)))
            {
                _unlockedSystems[systemType] = false;
            }
            
            // Initialize unlock times
            foreach (CultivationTaskType taskType in System.Enum.GetValues(typeof(CultivationTaskType)))
            {
                _lastUnlockTimes[taskType] = 0f;
            }
        }
        
        /// <summary>
        /// Check if automation can be unlocked for a specific task type
        /// </summary>
        public bool CanUnlockAutomation(CultivationTaskType taskType, float currentBurden, float playerSkill)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("AutomationUnlockManager not initialized", this);
                return false;
            }
            
            // Check cooldown
            if (Time.time - _lastUnlockTimes[taskType] < _unlockCooldownTime)
            {
                return false;
            }
            
            // Check burden threshold
            float requiredBurden = GetRequiredBurdenThreshold(taskType);
            if (currentBurden < requiredBurden)
            {
                return false;
            }
            
            // Check skill requirements
            float requiredSkill = GetRequiredSkillLevel(taskType);
            if (playerSkill < requiredSkill)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Attempt to unlock automation for a task type
        /// </summary>
        public AutomationUnlockResult TryUnlockAutomation(CultivationTaskType taskType, float currentBurden, float playerSkill)
        {
            var result = new AutomationUnlockResult
            {
                TaskType = taskType,
                Success = false,
                Reason = "Unknown error"
            };
            
            if (!CanUnlockAutomation(taskType, currentBurden, playerSkill))
            {
                result.Reason = "Requirements not met";
                return result;
            }
            
            // Determine which systems to unlock
            var systemsToUnlock = GetSystemsForTaskType(taskType);
            var unlockedSystems = new List<AutomationSystemType>();
            
            foreach (var systemType in systemsToUnlock)
            {
                if (!_unlockedSystems[systemType])
                {
                    _unlockedSystems[systemType] = true;
                    unlockedSystems.Add(systemType);
                }
            }
            
            if (unlockedSystems.Count > 0)
            {
                result.Success = true;
                result.Reason = $"Unlocked {unlockedSystems.Count} automation systems";
                result.UnlockedSystems = unlockedSystems;
                _lastUnlockTimes[taskType] = Time.time;
                
                ChimeraLogger.Log($"Unlocked automation systems for {taskType}: {string.Join(", ", unlockedSystems)}", this);
            }
            else
            {
                result.Reason = "No new systems to unlock";
            }
            
            return result;
        }
        
        /// <summary>
        /// Check if a specific automation system is unlocked
        /// </summary>
        public bool IsSystemUnlocked(AutomationSystemType systemType)
        {
            return _unlockedSystems.ContainsKey(systemType) && _unlockedSystems[systemType];
        }
        
        /// <summary>
        /// Get all unlocked automation systems
        /// </summary>
        public List<AutomationSystemType> GetUnlockedSystems()
        {
            var unlockedSystems = new List<AutomationSystemType>();
            
            foreach (var kvp in _unlockedSystems)
            {
                if (kvp.Value)
                {
                    unlockedSystems.Add(kvp.Key);
                }
            }
            
            return unlockedSystems;
        }
        
        /// <summary>
        /// Get required burden threshold for unlocking automation
        /// </summary>
        private float GetRequiredBurdenThreshold(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => 15.0f,
                CultivationTaskType.Feeding => 20.0f,
                CultivationTaskType.Pruning => 25.0f,
                CultivationTaskType.Training => 30.0f,
                CultivationTaskType.Harvesting => 18.0f,
                CultivationTaskType.Monitoring => 12.0f,
                CultivationTaskType.EnvironmentalControl => 22.0f,
                CultivationTaskType.PestControl => 28.0f,
                CultivationTaskType.Transplanting => 35.0f,
                CultivationTaskType.Defoliation => 20.0f,
                _ => 20.0f
            };
        }
        
        /// <summary>
        /// Get required skill level for unlocking automation
        /// </summary>
        private float GetRequiredSkillLevel(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => 3.0f,
                CultivationTaskType.Feeding => 4.0f,
                CultivationTaskType.Pruning => 6.0f,
                CultivationTaskType.Training => 7.0f,
                CultivationTaskType.Harvesting => 5.0f,
                CultivationTaskType.Monitoring => 2.0f,
                CultivationTaskType.EnvironmentalControl => 5.5f,
                CultivationTaskType.PestControl => 6.5f,
                CultivationTaskType.Transplanting => 8.0f,
                CultivationTaskType.Defoliation => 4.5f,
                _ => 4.0f
            };
        }
        
        /// <summary>
        /// Process automation unlock from skill node state - called by TreeSkillProgressionSystem
        /// </summary>
        public void ProcessAutomationUnlock(SkillNodeState nodeState)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("AutomationUnlockManager not initialized", this);
                return;
            }

            // Determine task type from node ID or type
            var taskType = GetTaskTypeFromNode(nodeState);
            if (taskType == CultivationTaskType.None)
            {
                ChimeraLogger.LogWarning($"Could not determine task type for node: {nodeState.NodeId}", this);
                return;
            }

            // Unlock automation with reduced requirements since it's skill-based
            var result = TryUnlockAutomation(taskType, GetRequiredBurdenThreshold(taskType) * 0.5f, GetRequiredSkillLevel(taskType) * 0.7f);
            
            if (result.Success)
            {
                ChimeraLogger.Log($"Automation unlocked via skill node {nodeState.NodeId}: {result.Reason}", this);
            }
        }

        /// <summary>
        /// Get task type from skill node state
        /// </summary>
        private CultivationTaskType GetTaskTypeFromNode(SkillNodeState nodeState)
        {
            // Map node IDs to task types (this would be configurable in a real implementation)
            return nodeState.NodeId.ToLowerInvariant() switch
            {
                var id when id.Contains("watering") || id.Contains("irrigation") => CultivationTaskType.Watering,
                var id when id.Contains("feeding") || id.Contains("nutrient") => CultivationTaskType.Feeding,
                var id when id.Contains("pruning") || id.Contains("trim") => CultivationTaskType.Pruning,
                var id when id.Contains("training") || id.Contains("lst") => CultivationTaskType.Training,
                var id when id.Contains("harvest") => CultivationTaskType.Harvesting,
                var id when id.Contains("monitor") || id.Contains("sensor") => CultivationTaskType.Monitoring,
                var id when id.Contains("environment") || id.Contains("climate") => CultivationTaskType.EnvironmentalControl,
                var id when id.Contains("pest") || id.Contains("ipm") => CultivationTaskType.PestControl,
                var id when id.Contains("transplant") => CultivationTaskType.Transplanting,
                var id when id.Contains("defoliat") => CultivationTaskType.Defoliation,
                _ => CultivationTaskType.None
            };
        }

        /// <summary>
        /// Get automation systems associated with a task type
        /// </summary>
        private List<AutomationSystemType> GetSystemsForTaskType(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => new List<AutomationSystemType> { AutomationSystemType.IrrigationSystem },
                CultivationTaskType.Feeding => new List<AutomationSystemType> { AutomationSystemType.NutrientDelivery },
                CultivationTaskType.Monitoring => new List<AutomationSystemType> { AutomationSystemType.SensorNetwork, AutomationSystemType.DataCollection },
                CultivationTaskType.EnvironmentalControl => new List<AutomationSystemType> { AutomationSystemType.ClimateControl, AutomationSystemType.VentilationControl },
                CultivationTaskType.PestControl => new List<AutomationSystemType> { AutomationSystemType.IPM },
                _ => new List<AutomationSystemType>()
            };
        }
        
        /// <summary>
        /// Evaluate unlock eligibility for a specific task type
        /// </summary>
        public void EvaluateUnlockEligibility(CultivationTaskType taskType)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("AutomationUnlockManager not initialized", this);
                return;
            }
            
            // Use default values for evaluation
            float defaultBurden = GetRequiredBurdenThreshold(taskType);
            float defaultSkill = GetRequiredSkillLevel(taskType);
            
            var result = TryUnlockAutomation(taskType, defaultBurden, defaultSkill);
            
            if (result.Success)
            {
                ChimeraLogger.Log($"Automation eligibility evaluation resulted in unlock for {taskType}: {result.Reason}", this);
            }
            else
            {
                ChimeraLogger.Log($"Automation not eligible for {taskType}: {result.Reason}", this);
            }
        }
        
        /// <summary>
        /// Get total number of unlocked systems
        /// </summary>
        public int GetTotalUnlockedSystems()
        {
            if (!_isInitialized) return 0;
            
            int count = 0;
            foreach (var kvp in _unlockedSystems)
            {
                if (kvp.Value) count++;
            }
            return count;
        }
    }
    
    /// <summary>
    /// Result of an automation unlock attempt
    /// </summary>
    [System.Serializable]
    public class AutomationUnlockResult
    {
        public CultivationTaskType TaskType;
        public bool Success;
        public string Reason;
        public List<AutomationSystemType> UnlockedSystems = new List<AutomationSystemType>();
    }
} 