using UnityEngine;
using System;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using SkillNodeType = ProjectChimera.Data.Cultivation.SkillNodeType;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Cultivation-specific events for the Project Chimera event system
    /// Located in Events assembly to access both Core and Data types
    /// </summary>
    
    #region Plant Care Events
    
    [System.Serializable]
    public class PlantCareEvent : BaseEvent
    {
        public int PlantInstanceID { get; set; }
        public CultivationTaskType TaskType { get; set; }
        public float CareQuality { get; set; }
        public float PlayerSkillLevel { get; set; }
        public bool WasAutomated { get; set; }
        
        public PlantCareEvent(int plantId, CultivationTaskType taskType, float quality, float skillLevel, bool automated = false)
        {
            PlantInstanceID = plantId;
            TaskType = taskType;
            CareQuality = quality;
            PlayerSkillLevel = skillLevel;
            WasAutomated = automated;
        }
    }
    
    [System.Serializable]
    public class PlantHealthChangedEvent : BaseEvent
    {
        public int PlantInstanceID { get; set; }
        public float PreviousHealth { get; set; }
        public float NewHealth { get; set; }
        public string HealthChangeReason { get; set; }
        public bool IsCritical { get; set; }
        
        public PlantHealthChangedEvent(int plantId, float previousHealth, float newHealth, string reason)
        {
            PlantInstanceID = plantId;
            PreviousHealth = previousHealth;
            NewHealth = newHealth;
            HealthChangeReason = reason;
            IsCritical = newHealth < 20f;
        }
    }
    
    [System.Serializable]
    public class PlantGrowthStageChangedEvent : BaseEvent
    {
        public int PlantInstanceID { get; set; }
        public PlantGrowthStage PreviousStage { get; set; }
        public PlantGrowthStage NewStage { get; set; }
        public float GrowthProgress { get; set; }
        
        public PlantGrowthStageChangedEvent(int plantId, PlantGrowthStage previousStage, PlantGrowthStage newStage, float progress)
        {
            PlantInstanceID = plantId;
            PreviousStage = previousStage;
            NewStage = newStage;
            GrowthProgress = progress;
        }
    }
    
    #endregion
    
    #region Skill System Events
    
    [System.Serializable]
    public class SkillExperienceGainedEvent : BaseEvent
    {
        public SkillBranch Branch { get; set; }
        public float ExperienceGained { get; set; }
        public float TotalExperience { get; set; }
        public string Source { get; set; }
        public CultivationTaskType RelatedTask { get; set; }
        
        public SkillExperienceGainedEvent(SkillBranch branch, float gained, float total, string source, CultivationTaskType task)
        {
            Branch = branch;
            ExperienceGained = gained;
            TotalExperience = total;
            Source = source;
            RelatedTask = task;
        }
    }
    
    [System.Serializable]
    public class SkillNodeUnlockedEvent : BaseEvent
    {
        public string NodeId { get; set; }
        public string NodeName { get; set; }
        public SkillBranch Branch { get; set; }
        public SkillNodeType NodeType { get; set; }
        public string[] UnlockedCapabilities { get; set; }
        public bool IsFirstInBranch { get; set; }
        
        public SkillNodeUnlockedEvent(string nodeId, string nodeName, SkillBranch branch, SkillNodeType nodeType, string[] capabilities)
        {
            NodeId = nodeId;
            NodeName = nodeName;
            Branch = branch;
            NodeType = nodeType;
            UnlockedCapabilities = capabilities ?? new string[0];
        }
    }
    
    [System.Serializable]
    public class SkillTreeGrowthEvent : BaseEvent
    {
        public TreeGrowthLevel PreviousLevel { get; set; }
        public TreeGrowthLevel NewLevel { get; set; }
        public int TotalNodesUnlocked { get; set; }
        public float TreeVibrancy { get; set; }
        public SkillBranch[] ActiveBranches { get; set; }
        
        public SkillTreeGrowthEvent(TreeGrowthLevel previousLevel, TreeGrowthLevel newLevel, int totalNodes, float vibrancy)
        {
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            TotalNodesUnlocked = totalNodes;
            TreeVibrancy = vibrancy;
        }
    }
    
    #endregion
    
    #region Automation Events
    
    [System.Serializable]
    public class AutomationSystemUnlockedEvent : BaseEvent
    {
        public AutomationSystemType SystemType { get; set; }
        public string SystemName { get; set; }
        public CultivationTaskType[] SupportedTasks { get; set; }
        public float BurdenReduction { get; set; }
        public float UnlockCost { get; set; }
        
        public AutomationSystemUnlockedEvent(AutomationSystemType systemType, string systemName, CultivationTaskType[] supportedTasks, float burdenReduction, float cost)
        {
            SystemType = systemType;
            SystemName = systemName;
            SupportedTasks = supportedTasks ?? new CultivationTaskType[0];
            BurdenReduction = burdenReduction;
            UnlockCost = cost;
        }
    }
    
    [System.Serializable]
    public class AutomationSystemActivatedEvent : BaseEvent
    {
        public AutomationSystemType SystemType { get; set; }
        public int PlantInstanceID { get; set; }
        public CultivationTaskType TaskType { get; set; }
        public float SystemEfficiency { get; set; }
        public bool WasSuccessful { get; set; }
        
        public AutomationSystemActivatedEvent(AutomationSystemType systemType, int plantId, CultivationTaskType taskType, float efficiency, bool successful)
        {
            SystemType = systemType;
            PlantInstanceID = plantId;
            TaskType = taskType;
            SystemEfficiency = efficiency;
            WasSuccessful = successful;
        }
    }
    
    #endregion
    
    #region Time Acceleration Events
    
    [System.Serializable]
    public class TimeScaleChangedEvent : BaseEvent
    {
        public GameTimeScale PreviousScale { get; set; }
        public GameTimeScale NewScale { get; set; }
        public float PreviousMultiplier { get; set; }
        public float NewMultiplier { get; set; }
        public string ChangeReason { get; set; }
        
        public TimeScaleChangedEvent(GameTimeScale previousScale, GameTimeScale newScale, float previousMultiplier, float newMultiplier, string reason)
        {
            PreviousScale = previousScale;
            NewScale = newScale;
            PreviousMultiplier = previousMultiplier;
            NewMultiplier = newMultiplier;
            ChangeReason = reason;
        }
    }
    
    [System.Serializable]
    public class PlayerEngagementChangedEvent : BaseEvent
    {
        public float PreviousEngagement { get; set; }
        public float NewEngagement { get; set; }
        public string EngagementFactor { get; set; }
        public bool IsOptimal { get; set; }
        
        public PlayerEngagementChangedEvent(float previousEngagement, float newEngagement, string factor, bool isOptimal)
        {
            PreviousEngagement = previousEngagement;
            NewEngagement = newEngagement;
            EngagementFactor = factor;
            IsOptimal = isOptimal;
        }
    }
    
    #endregion
    
    #region Achievement Events
    
    [System.Serializable]
    public class AchievementUnlockedEvent : BaseEvent
    {
        public string AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string Description { get; set; }
        public AchievementType Type { get; set; }
        public float RewardValue { get; set; }
        public string RewardType { get; set; }
        
        public AchievementUnlockedEvent(string achievementId, string name, string description, AchievementType type)
        {
            AchievementId = achievementId;
            AchievementName = name;
            Description = description;
            Type = type;
        }
    }
    
    [System.Serializable]
    public class MilestoneReachedEvent : BaseEvent
    {
        public string MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public float ProgressValue { get; set; }
        public string ProgressMetric { get; set; }
        public string[] UnlockedRewards { get; set; }
        
        public MilestoneReachedEvent(string milestoneId, string name, float progress, string metric)
        {
            MilestoneId = milestoneId;
            MilestoneName = name;
            ProgressValue = progress;
            ProgressMetric = metric;
        }
    }
    
    #endregion
    
    #region Environmental Events
    
    [System.Serializable]
    public class EnvironmentalConditionChangedEvent : BaseEvent
    {
        public EnvironmentalFactor Factor { get; set; }
        public float PreviousValue { get; set; }
        public float NewValue { get; set; }
        public string ChangeSource { get; set; }
        public bool IsOptimal { get; set; }
        
        public EnvironmentalConditionChangedEvent(EnvironmentalFactor factor, float previousValue, float newValue, string source)
        {
            Factor = factor;
            PreviousValue = previousValue;
            NewValue = newValue;
            ChangeSource = source;
            IsOptimal = Mathf.Abs(newValue - 1.0f) < 0.1f; // Assuming 1.0 is optimal
        }
    }
    
    [System.Serializable]
    public class EnvironmentalAlertEvent : BaseEvent
    {
        public EnvironmentalFactor Factor { get; set; }
        public float CurrentValue { get; set; }
        public float OptimalValue { get; set; }
        public AlertSeverity Severity { get; set; }
        public string AlertMessage { get; set; }
        public string[] RecommendedActions { get; set; }
        
        public EnvironmentalAlertEvent(EnvironmentalFactor factor, float currentValue, float optimalValue, AlertSeverity severity, string message)
        {
            Factor = factor;
            CurrentValue = currentValue;
            OptimalValue = optimalValue;
            Severity = severity;
            AlertMessage = message;
        }
    }
    
    #endregion
    
    #region Enums
    
    public enum AchievementType
    {
        SkillMastery,
        PlantCare,
        Automation,
        Efficiency,
        Discovery,
        Milestone,
        Special
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }
    
    #endregion
} 