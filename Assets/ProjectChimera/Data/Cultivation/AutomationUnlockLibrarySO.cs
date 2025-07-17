using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Automation Unlock Library - ScriptableObject containing all automation system unlock data
    /// Defines progression paths, requirements, and unlockable automation systems
    /// </summary>
    [CreateAssetMenu(fileName = "AutomationUnlockLibrary", menuName = "Project Chimera/Cultivation/Automation Unlock Library")]
    public class AutomationUnlockLibrarySO : ScriptableObject
    {
        [Header("Automation System Definitions")]
        [SerializeField] private AutomationSystemDefinition[] _automationSystems = new AutomationSystemDefinition[0];
        
        [Header("Unlock Progression")]
        [SerializeField] private AutomationUnlockPath[] _unlockPaths = new AutomationUnlockPath[0];
        
        [Header("Burden Thresholds")]
        [Range(0.1f, 10f)] public float MinimumBurdenForUnlock = 2.0f;
        [Range(0.5f, 20f)] public float MaximumBurdenThreshold = 15.0f;
        [Range(0.1f, 5f)] public float BurdenReductionFactor = 0.7f;
        
        [Header("Skill Requirements")]
        [Range(1, 100)] public int MinimumSkillLevelForAutomation = 10;
        [Range(1, 100)] public int MaximumSkillLevelRequired = 75;
        [Range(0.1f, 2f)] public float SkillProgressionMultiplier = 1.0f;
        
        [Header("Unlock Costs")]
        [Range(10f, 10000f)] public float BaseUnlockCost = 100f;
        [Range(1f, 5f)] public float CostScalingFactor = 1.5f;
        [Range(0.1f, 2f)] public float SkillDiscountFactor = 0.8f;
        
        // Public Properties
        public AutomationSystemDefinition[] AutomationSystems => _automationSystems;
        public AutomationUnlockPath[] UnlockPaths => _unlockPaths;
        
        /// <summary>
        /// Get automation system by type
        /// </summary>
        public AutomationSystemDefinition GetAutomationSystem(AutomationSystemType systemType)
        {
            return _automationSystems.FirstOrDefault(s => s.SystemType == systemType);
        }
        
        /// <summary>
        /// Get all automation systems for a specific task type
        /// </summary>
        public AutomationSystemDefinition[] GetAutomationSystemsForTask(CultivationTaskType taskType)
        {
            return _automationSystems.Where(s => s.SupportedTasks.Contains(taskType)).ToArray();
        }
        
        /// <summary>
        /// Check if automation system can be unlocked
        /// </summary>
        public bool CanUnlockSystem(AutomationSystemType systemType, float playerSkillLevel, float taskBurden, List<AutomationSystemType> unlockedSystems)
        {
            var system = GetAutomationSystem(systemType);
            if (system == null) return false;
            
            // Check skill requirements
            if (playerSkillLevel < system.RequiredSkillLevel) return false;
            
            // Check burden threshold
            if (taskBurden < system.MinimumBurdenRequired) return false;
            
            // Check prerequisites
            if (system.Prerequisites != null && system.Prerequisites.Length > 0)
            {
                foreach (var prerequisite in system.Prerequisites)
                {
                    if (!unlockedSystems.Contains(prerequisite)) return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculate unlock cost for automation system
        /// </summary>
        public float CalculateUnlockCost(AutomationSystemType systemType, float playerSkillLevel)
        {
            var system = GetAutomationSystem(systemType);
            if (system == null) return 0f;
            
            var baseCost = BaseUnlockCost * Mathf.Pow(CostScalingFactor, (int)system.Tier);
            var skillDiscount = Mathf.Clamp01(playerSkillLevel / 100f) * SkillDiscountFactor;
            
            return baseCost * (1f - skillDiscount);
        }
        
        /// <summary>
        /// Get burden reduction from automation system
        /// </summary>
        public float GetBurdenReduction(AutomationSystemType systemType, float systemEfficiency = 1f)
        {
            var system = GetAutomationSystem(systemType);
            if (system == null) return 0f;
            
            return system.BurdenReduction * BurdenReductionFactor * systemEfficiency;
        }
        
        /// <summary>
        /// Get next available unlock for player
        /// </summary>
        public AutomationSystemDefinition GetNextAvailableUnlock(float playerSkillLevel, float taskBurden, List<AutomationSystemType> unlockedSystems, CultivationTaskType taskType)
        {
            var availableSystems = GetAutomationSystemsForTask(taskType)
                .Where(s => !unlockedSystems.Contains(s.SystemType))
                .Where(s => CanUnlockSystem(s.SystemType, playerSkillLevel, taskBurden, unlockedSystems))
                .OrderBy(s => s.RequiredSkillLevel);
            
            return availableSystems.FirstOrDefault();
        }
        
        /// <summary>
        /// Get unlock path for specific system
        /// </summary>
        public AutomationUnlockPath GetUnlockPath(AutomationSystemType systemType)
        {
            return _unlockPaths.FirstOrDefault(p => p.TargetSystem == systemType);
        }
        
        /// <summary>
        /// Validate library data
        /// </summary>
        private void OnValidate()
        {
            ValidateSystemDefinitions();
            ValidateUnlockPaths();
        }
        
        private void ValidateSystemDefinitions()
        {
            for (int i = 0; i < _automationSystems.Length; i++)
            {
                var system = _automationSystems[i];
                
                // Ensure required fields are set
                if (string.IsNullOrEmpty(system.SystemName))
                    system.SystemName = $"Automation System {i + 1}";
                
                if (system.SupportedTasks == null || system.SupportedTasks.Length == 0)
                    Debug.LogWarning($"Automation system '{system.SystemName}' has no supported tasks defined");
                
                // Validate burden reduction
                system.BurdenReduction = Mathf.Clamp(system.BurdenReduction, 0.1f, 1f);
                
                // Validate skill requirements
                system.RequiredSkillLevel = Mathf.Clamp(system.RequiredSkillLevel, 1, 100);
            }
        }
        
        private void ValidateUnlockPaths()
        {
            foreach (var path in _unlockPaths)
            {
                // Ensure path has valid milestones
                if (path.Milestones == null || path.Milestones.Length == 0)
                {
                    Debug.LogWarning($"Unlock path for '{path.TargetSystem}' has no milestones defined");
                }
            }
        }
    }
    
    [System.Serializable]
    public class AutomationSystemDefinition
    {
        [Header("System Identity")]
        public AutomationSystemType SystemType;
        public string SystemName;
        [TextArea(2, 4)] public string Description;
        public AutomationTier Tier = AutomationTier.Basic;
        
        [Header("Capabilities")]
        public CultivationTaskType[] SupportedTasks = new CultivationTaskType[0];
        [Range(0.1f, 1f)] public float BurdenReduction = 0.5f;
        [Range(0.1f, 2f)] public float EfficiencyMultiplier = 1.2f;
        [Range(0.5f, 1f)] public float ReliabilityRating = 0.9f;
        
        [Header("Requirements")]
        [Range(1, 100)] public int RequiredSkillLevel = 10;
        [Range(0.1f, 20f)] public float MinimumBurdenRequired = 2f;
        public AutomationSystemType[] Prerequisites = new AutomationSystemType[0];
        
        [Header("Costs")]
        [Range(10f, 10000f)] public float UnlockCost = 100f;
        [Range(1f, 1000f)] public float MaintenanceCost = 10f;
        [Range(0.1f, 100f)] public float OperationalCost = 5f;
        
        [Header("Visual")]
        public Sprite SystemIcon;
        public GameObject SystemPrefab;
        public Color SystemColor = Color.white;
    }
    
    [System.Serializable]
    public class AutomationUnlockPath
    {
        [Header("Path Definition")]
        public AutomationSystemType TargetSystem;
        public string PathName;
        [TextArea(2, 3)] public string PathDescription;
        
        [Header("Progression")]
        public UnlockMilestone[] Milestones = new UnlockMilestone[0];
        [Range(1f, 100f)] public float TotalProgressRequired = 50f;
        
        [Header("Rewards")]
        public UnlockReward[] IntermediateRewards = new UnlockReward[0];
        public UnlockReward FinalReward;
    }
    
    [System.Serializable]
    public class UnlockMilestone
    {
        public string MilestoneName;
        [TextArea(1, 2)] public string Description;
        [Range(0f, 100f)] public float ProgressRequired;
        public MilestoneType Type = MilestoneType.SkillLevel;
        public float TargetValue;
        public bool IsCompleted = false;
    }
    
    [System.Serializable]
    public class UnlockReward
    {
        public RewardType Type = RewardType.SkillBonus;
        public float Value;
        public string Description;
    }
    
    public enum AutomationSystemType
    {
        BasicWatering,
        AdvancedWatering,
        IrrigationSystem,
        NutrientDelivery,
        EnvironmentalControl,
        ClimateControl,
        LightingAutomation,
        LightingControl,
        LightingSchedule,
        MonitoringSystem,
        MonitoringSensors,
        SensorNetwork,
        AlertSystem,
        DataLogging,
        DataCollection,
        PredictiveAnalytics,
        HarvestAssist,
        VentilationControl,
        IPM,
        FullAutomation
    }
    
    public enum AutomationTier
    {
        Basic,
        Intermediate,
        Advanced,
        Professional,
        Expert
    }
    
    public enum MilestoneType
    {
        SkillLevel,
        TaskCount,
        BurdenThreshold,
        TimeSpent,
        QualityAchieved
    }
    
    // RewardType enum is defined in CultivationPathData.cs to avoid duplicates
}