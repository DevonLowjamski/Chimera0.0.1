using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Earned Automation Configuration - Configuration for automation systems earned through progression
    /// Defines automation unlock conditions, capabilities, and efficiency parameters
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Earned Automation Config", menuName = "Project Chimera/Cultivation/Earned Automation Config")]
    public class EarnedAutomationConfigSO : ChimeraConfigSO
    {
        [Header("Automation Progression")]
        public List<AutomationTierConfig> AutomationTiers = new List<AutomationTierConfig>();
        
        [Header("Unlock Requirements")]
        public List<AutomationUnlockConfig> UnlockConfigurations = new List<AutomationUnlockConfig>();
        
        [Header("Efficiency Settings")]
        [Range(0.1f, 2.0f)] public float BaseAutomationEfficiency = 0.8f;
        [Range(0.01f, 0.1f)] public float EfficiencyGainPerLevel = 0.02f;
        [Range(0.5f, 1.0f)] public float MaxAutomationEfficiency = 0.95f;
        
        [Header("Cost Settings")]
        [Range(0.1f, 5.0f)] public float AutomationCostMultiplier = 1.5f;
        [Range(0.1f, 2.0f)] public float MaintenanceCostFactor = 0.3f;
        
        [Header("Burden Threshold Settings")]
        [Range(1.0f, 10.0f)] public float BaseBurdenThreshold = 5.0f;
        [Range(0.5f, 3.0f)] public float BurdenThresholdMultiplier = 1.2f;
        [Range(5.0f, 60.0f)] public float BurdenDecayDelay = 10.0f;
        [Range(0.1f, 2.0f)] public float BurdenDecayRate = 0.5f;
        
        [Header("Skill and Complexity Settings")]
        [Range(1.0f, 10.0f)] public float MaxSkillLevel = 10.0f;
        
        [Header("Precision and Quality Settings")]
        [Range(0.1f, 2.0f)] public float BaseScaleReference = 1.0f;
        
        [Header("Burden Weight Settings")]
        public BurdenWeights BurdenWeights = new BurdenWeights();
        
        [Header("Benefit Weight Settings")]
        public BenefitWeights BenefitWeights = new BenefitWeights();
        
        [Header("Burden Threshold Settings")]
        public BurdenThresholds BurdenThresholds = new BurdenThresholds();
        
        public AutomationTierConfig GetTierConfig(AutomationTier tier)
        {
            return AutomationTiers.Find(t => t.Tier == tier);
        }
        
        public bool CanUnlockAutomation(AutomationType automationType, int playerLevel, float experience)
        {
            var unlockConfig = UnlockConfigurations.Find(u => u.AutomationType == automationType);
            if (unlockConfig == null) return false;
            
            return playerLevel >= unlockConfig.RequiredLevel && experience >= unlockConfig.RequiredExperience;
        }
        
        public float GetBurdenThreshold(CultivationTaskType taskType)
        {
            // Calculate burden threshold based on task type complexity
            float baseBurdenThreshold = 5.0f;
            float multiplier = taskType switch
            {
                CultivationTaskType.Watering => 1.0f,
                CultivationTaskType.Feeding => 1.2f,
                CultivationTaskType.Monitoring => 0.8f,
                CultivationTaskType.PestControl => 1.5f,
                CultivationTaskType.Transplanting => 1.8f,
                CultivationTaskType.Defoliation => 1.3f,
                CultivationTaskType.Pruning => 1.4f,
                CultivationTaskType.Training => 1.6f,
                CultivationTaskType.Harvesting => 1.1f,
                CultivationTaskType.Cleaning => 0.9f,
                CultivationTaskType.Maintenance => 1.7f,
                CultivationTaskType.Research => 2.0f,
                _ => 1.0f
            };
            
            return baseBurdenThreshold * multiplier;
        }
        
        public float GetPrecisionRequirement(CultivationTaskType taskType)
        {
            // Return precision requirement for different task types
            return taskType switch
            {
                CultivationTaskType.None => 0.1f,
                CultivationTaskType.Watering => 0.6f,
                CultivationTaskType.Feeding => 0.8f,
                CultivationTaskType.EnvironmentalControl => 0.9f,
                CultivationTaskType.Transplanting => 0.95f,
                CultivationTaskType.Defoliation => 0.85f,
                CultivationTaskType.Pruning => 0.9f,
                CultivationTaskType.Training => 0.8f,
                CultivationTaskType.Monitoring => 0.7f,
                CultivationTaskType.Harvesting => 0.75f,
                CultivationTaskType.Cleaning => 0.5f,
                CultivationTaskType.Maintenance => 0.8f,
                CultivationTaskType.Research => 0.95f,
                CultivationTaskType.PestControl => 0.9f,
                _ => 0.7f
            };
        }
        
        public float GetErrorTolerance(CultivationTaskType taskType)
        {
            // Return error tolerance for different task types (inverse of precision requirement)
            return 1.0f - GetPrecisionRequirement(taskType);
        }
        
        public float GetTaskComplexity(CultivationTaskType taskType)
        {
            // Return complexity rating for different task types
            return taskType switch
            {
                CultivationTaskType.None => 0.0f,
                CultivationTaskType.Watering => 0.5f,
                CultivationTaskType.Feeding => 0.7f,
                CultivationTaskType.Fertilizing => 0.6f,
                CultivationTaskType.EnvironmentalControl => 1.2f,
                CultivationTaskType.Transplanting => 1.8f,
                CultivationTaskType.Defoliation => 1.3f,
                CultivationTaskType.Pruning => 1.4f,
                CultivationTaskType.Training => 1.6f,
                CultivationTaskType.Monitoring => 0.8f,
                CultivationTaskType.Harvesting => 1.1f,
                CultivationTaskType.Cleaning => 0.4f,
                CultivationTaskType.Maintenance => 1.7f,
                CultivationTaskType.Research => 2.0f,
                CultivationTaskType.PestControl => 1.5f,
                _ => 1.0f
            };
        }
        
        public float GetRequiredMastery(AutomationSystemType systemType)
        {
            // Return required mastery level for different automation systems
            return systemType switch
            {
                AutomationSystemType.IrrigationSystem => 5.0f,
                AutomationSystemType.NutrientDelivery => 7.0f,
                AutomationSystemType.ClimateControl => 8.0f,
                AutomationSystemType.LightingControl => 6.0f,
                AutomationSystemType.LightingSchedule => 7.5f,
                AutomationSystemType.MonitoringSensors => 4.0f,
                AutomationSystemType.HarvestAssist => 6.5f,
                AutomationSystemType.SensorNetwork => 8.5f,
                AutomationSystemType.DataCollection => 9.0f,
                AutomationSystemType.VentilationControl => 7.5f,
                AutomationSystemType.IPM => 9.5f,
                _ => 5.0f
            };
        }
        
        public float GetMaintenanceInterval(AutomationSystemType systemType)
        {
            // Return maintenance interval in hours for different automation systems
            return systemType switch
            {
                AutomationSystemType.IrrigationSystem => 168.0f, // 1 week
                AutomationSystemType.NutrientDelivery => 72.0f, // 3 days
                AutomationSystemType.ClimateControl => 336.0f, // 2 weeks
                AutomationSystemType.LightingControl => 720.0f, // 1 month
                AutomationSystemType.LightingSchedule => 720.0f, // 1 month
                AutomationSystemType.MonitoringSensors => 168.0f, // 1 week
                AutomationSystemType.HarvestAssist => 240.0f, // 10 days
                AutomationSystemType.SensorNetwork => 504.0f, // 3 weeks
                AutomationSystemType.DataCollection => 720.0f, // 1 month
                AutomationSystemType.VentilationControl => 336.0f, // 2 weeks
                AutomationSystemType.IPM => 120.0f, // 5 days
                _ => 168.0f
            };
        }
        
        public float GetExperienceThreshold(CultivationTaskType taskType)
        {
            // Return experience threshold for automation unlock for different task types
            return taskType switch
            {
                CultivationTaskType.Watering => 100.0f,
                CultivationTaskType.Feeding => 200.0f,
                CultivationTaskType.Fertilizing => 150.0f,
                CultivationTaskType.Monitoring => 80.0f,
                CultivationTaskType.DataCollection => 300.0f,
                CultivationTaskType.EnvironmentalControl => 400.0f,
                CultivationTaskType.Maintenance => 250.0f,
                _ => 100.0f
            };
        }
    }
    
    [System.Serializable]
    public class AutomationTierConfig
    {
        public string TierName;
        public AutomationTier Tier;
        [Range(0.1f, 1.0f)] public float EfficiencyMultiplier = 1.0f;
        [Range(1, 100)] public int MaxAutomatedActions = 10;
        public List<AutomationType> AvailableAutomations = new List<AutomationType>();
        public string Description;
    }
    
    [System.Serializable]
    public class AutomationUnlockConfig
    {
        public string UnlockName;
        public AutomationType AutomationType;
        [Range(1, 100)] public int RequiredLevel = 1;
        [Range(0f, 10000f)] public float RequiredExperience = 0f;
        public List<string> RequiredAchievements = new List<string>();
        public List<string> PrerequisiteAutomations = new List<string>();
        [Range(0.1f, 10.0f)] public float UnlockCost = 1.0f;
    }
    
    // AutomationTier enum is defined in AutomationUnlockLibrarySO.cs
    
    public enum AutomationType
    {
        AutoWatering,
        AutoFeeding,
        AutoPruning,
        AutoHarvesting,
        AutoMonitoring,
        AutoClimateControl,
        AutoLighting,
        AutoNutrientMixing
    }
    
    [System.Serializable]
    public class BurdenWeights
    {
        [Range(0.0f, 1.0f)] public float CognitiveLoadWeight = 0.25f;
        [Range(0.0f, 1.0f)] public float TimeInvestmentWeight = 0.2f;
        [Range(0.0f, 1.0f)] public float ConsistencyChallengeWeight = 0.15f;
        [Range(0.0f, 1.0f)] public float ScalePressureWeight = 0.2f;
        [Range(0.0f, 1.0f)] public float QualityRiskWeight = 0.2f;
    }
    
    [System.Serializable]
    public class BenefitWeights
    {
        [Range(0.0f, 1.0f)] public float ConsistencyImprovementWeight = 0.3f;
        [Range(0.0f, 1.0f)] public float EfficiencyGainWeight = 0.25f;
        [Range(0.0f, 1.0f)] public float ScalabilityWeight = 0.2f;
        [Range(0.0f, 1.0f)] public float QualityOptimizationWeight = 0.15f;
        [Range(0.0f, 1.0f)] public float TimeLiberationWeight = 0.1f;
        
        // Additional properties referenced in gaming systems
        [Range(0.0f, 1.0f)] public float ConsistencyWeight = 0.3f;
        [Range(0.0f, 1.0f)] public float EfficiencyWeight = 0.25f;
        [Range(0.0f, 1.0f)] public float QualityWeight = 0.15f;
        [Range(0.0f, 1.0f)] public float TimeWeight = 0.1f;
    }
    
    [System.Serializable]
    public class BurdenThresholds
    {
        [Range(1.0f, 10.0f)] public float LowBurdenThreshold = 2.0f;
        [Range(1.0f, 10.0f)] public float MediumBurdenThreshold = 5.0f;
        [Range(1.0f, 10.0f)] public float HighBurdenThreshold = 8.0f;
        [Range(1.0f, 10.0f)] public float CriticalBurdenThreshold = 10.0f;
        
        // Properties with shorter names for easier access
        public float LowThreshold => LowBurdenThreshold;
        public float MediumThreshold => MediumBurdenThreshold;
        public float HighThreshold => HighBurdenThreshold;
        public float CriticalThreshold => CriticalBurdenThreshold;
    }
    
}