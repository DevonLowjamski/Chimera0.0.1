using UnityEngine;
using System.Collections.Generic;
using System;
using AutomationUnlockEventData = ProjectChimera.Core.Events.AutomationUnlockEventData;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Skill Node Event Data - Defines event data structures for skill progression system
    /// Contains event payload data for skill node unlocks, progress updates, and achievements
    /// </summary>
    
    [System.Serializable]
    public class SkillNodeEventData
    {
        [Header("Event Identity")]
        public string EventId;
        public SkillNodeEventType EventType;
        public DateTime Timestamp;
        public string PlayerId;
        
        [Header("Skill Node Information")]
        public string NodeId;
        public string NodeName;
        public SkillBranch Branch;
        public SkillNodeType NodeType;
        
        [Header("Progress Data")]
        public float PreviousProgress;
        public float NewProgress;
        public float ExperienceGained;
        public float TotalExperience;
        
        [Header("Context")]
        public string TriggeringAction;
        public CultivationTaskType RelatedTask;
        public float PlayerSkillLevel;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
        
        public SkillNodeEventData()
        {
            EventId = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
        }
        
        public SkillNodeEventData(SkillNodeEventType eventType, string nodeId, string playerId = "default")
        {
            EventId = Guid.NewGuid().ToString();
            EventType = eventType;
            NodeId = nodeId;
            PlayerId = playerId;
            Timestamp = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class SkillProgressEventData
    {
        [Header("Progress Information")]
        public string NodeId;
        public float ProgressDelta;
        public float NewProgressValue;
        public ExperienceSource ExperienceSource;
        public float SourceMultiplier;
        
        [Header("Skill Context")]
        public CultivationTaskType TaskPerformed;
        public float TaskQuality;
        public float TaskDifficulty;
        public bool WasAutomated;
        
        [Header("Player State")]
        public float PlayerSkillLevel;
        public float PlayerEngagement;
        public TimeSpan SessionDuration;
        
        public SkillProgressEventData()
        {
            // Default constructor
        }
        
        public SkillProgressEventData(string nodeId, float progressDelta, ExperienceSource source)
        {
            NodeId = nodeId;
            ProgressDelta = progressDelta;
            ExperienceSource = source;
        }
    }
    
    [System.Serializable]
    public class SkillUnlockEventData
    {
        [Header("Unlock Information")]
        public string UnlockedNodeId;
        public string UnlockedNodeName;
        public SkillBranch Branch;
        public SkillNodeType NodeType;
        
        [Header("Unlock Context")]
        public string[] PrerequisitesMet;
        public float FinalExperienceAmount;
        public bool WasInstantUnlock;
        public string UnlockMethod; // "progression", "purchase", "achievement", etc.
        
        [Header("Rewards")]
        public string[] UnlockedCapabilities;
        public string[] UnlockedEquipment;
        public SkillBonus[] SkillBonuses;
        
        [Header("Achievement Data")]
        public bool TriggeredAchievement;
        public string AchievementId;
        public bool IsFirstInBranch;
        public bool IsFirstOfType;
        
        public SkillUnlockEventData()
        {
            // Default constructor
        }
        
        public SkillUnlockEventData(string nodeId, string nodeName, SkillBranch branch)
        {
            UnlockedNodeId = nodeId;
            UnlockedNodeName = nodeName;
            Branch = branch;
        }
    }
    
    [System.Serializable]
    public class BranchProgressEventData
    {
        [Header("Branch Information")]
        public SkillBranch Branch;
        public string BranchName;
        public int NodesUnlocked;
        public int TotalNodesInBranch;
        public float BranchCompletionPercentage;
        
        [Header("Progress Details")]
        public float BranchVibrancy;
        public TreeGrowthLevel BranchGrowthLevel;
        public float TotalBranchExperience;
        public DateTime LastProgressTime;
        
        [Header("Milestones")]
        public BranchMilestone[] ReachedMilestones;
        public BranchMilestone NextMilestone;
        public bool BranchCompleted;
        
        [Header("Synergy Effects")]
        public float BranchSynergyBonus;
        public SkillBranch[] SynergyBranches;
        public bool TriggeredSynergyUnlock;
        
        public BranchProgressEventData()
        {
            // Default constructor
        }
        
        public BranchProgressEventData(SkillBranch branch, int nodesUnlocked, int totalNodes)
        {
            Branch = branch;
            NodesUnlocked = nodesUnlocked;
            TotalNodesInBranch = totalNodes;
            BranchCompletionPercentage = totalNodes > 0 ? (float)nodesUnlocked / totalNodes : 0f;
        }
    }
    
    [System.Serializable]
    public class TreeGrowthEventData
    {
        [Header("Tree State")]
        public TreeGrowthLevel PreviousGrowthLevel;
        public TreeGrowthLevel NewGrowthLevel;
        public float TreeVibrancy;
        public int TotalNodesUnlocked;
        
        [Header("Growth Metrics")]
        public float GrowthRate;
        public TimeSpan TimeSinceLastGrowth;
        public float ExperienceVelocity;
        public Dictionary<SkillBranch, int> BranchProgress = new Dictionary<SkillBranch, int>();
        
        [Header("Visual Changes")]
        public bool RequiresVisualizationUpdate;
        public float NewTreeScale;
        public Color NewTreeColor;
        public bool EnableSpecialEffects;
        
        [Header("Unlocks")]
        public string[] NewCapabilitiesUnlocked;
        public string[] NewFeaturesUnlocked;
        public bool UnlockedNewBranch;
        public SkillBranch NewlyUnlockedBranch;
        
        public TreeGrowthEventData()
        {
            // Default constructor
        }
        
        public TreeGrowthEventData(TreeGrowthLevel previousLevel, TreeGrowthLevel newLevel, int totalNodes)
        {
            PreviousGrowthLevel = previousLevel;
            NewGrowthLevel = newLevel;
            TotalNodesUnlocked = totalNodes;
        }
    }
    
    
    [System.Serializable]
    public class SkillSynergyEventData
    {
        [Header("Synergy Information")]
        public SkillBranch[] InvolvedBranches;
        public string SynergyName;
        public string SynergyDescription;
        public SynergyType SynergyType;
        
        [Header("Synergy Effects")]
        public float SynergyMultiplier;
        public string[] UnlockedCombinations;
        public string[] EnhancedCapabilities;
        public float BonusExperienceMultiplier;
        
        [Header("Requirements Met")]
        public Dictionary<SkillBranch, int> RequiredProgress = new Dictionary<SkillBranch, int>();
        public Dictionary<SkillBranch, int> CurrentProgress = new Dictionary<SkillBranch, int>();
        public bool AllRequirementsMet;
        
        [Header("Impact")]
        public float ProductivityBonus;
        public float QualityBonus;
        public float CostReductionBonus;
        
        public SkillSynergyEventData()
        {
            // Default constructor
        }
        
        public SkillSynergyEventData(SkillBranch[] branches, SynergyType synergyType)
        {
            InvolvedBranches = branches;
            SynergyType = synergyType;
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class SkillBonus
    {
        public SkillBonusType BonusType;
        public float BonusValue;
        public string BonusDescription;
        public bool IsPermanent = true;
        public DateTime ExpirationDate;
        
        public SkillBonus(SkillBonusType type, float value, string description)
        {
            BonusType = type;
            BonusValue = value;
            BonusDescription = description;
        }
    }
    
    [System.Serializable]
    public class BranchMilestone
    {
        public string MilestoneId;
        public string MilestoneName;
        public string Description;
        public int RequiredNodes;
        public float RequiredExperience;
        public bool IsAchieved;
        public DateTime AchievementDate;
        public MilestoneReward[] Rewards;
        
        public BranchMilestone(string id, string name, int requiredNodes)
        {
            MilestoneId = id;
            MilestoneName = name;
            RequiredNodes = requiredNodes;
        }
    }
    
    // Note: SkillNodeState, BranchProgressionState, and BranchDefinition are defined in SkillTreeDataStructures.cs
    
    #endregion
    
    #region Enums
    
    public enum SkillNodeEventType
    {
        ProgressGained,
        NodeUnlocked,
        ConceptIntroduced,
        EquipmentUnlocked,
        BonusActivated,
        PrerequisiteMet,
        NodeActivated,
        SkillUsed
    }
    
    // Note: ExperienceSource enum is defined in SkillTreeDataStructures.cs
    
    public enum SkillBonusType
    {
        ExperienceGain,
        EfficiencyBonus,
        CostReduction,
        QualityBonus,
        SpeedBonus,
        AutomationBonus,
        SynergyBonus
    }
    
    public enum SynergyType
    {
        BranchCombination,
        SkillChain,
        AutomationIntegration,
        KnowledgeApplication,
        MasteryBonus
    }
    
    public enum TaskType
    {
        Watering,
        Nutrient_Application,
        Pruning,
        Training,
        Harvesting,
        Environmental_Control,
        Monitoring,
        Equipment_Maintenance,
        Quality_Assessment,
        Documentation
    }
    
    public enum UnlockedSystems
    {
        AutoWatering,
        NutrientInjection,
        ClimateControl,
        LightingAutomation,
        SecuritySystem,
        MonitoringSystem,
        AlertSystem,
        DataLogging,
        RemoteControl,
        AIOptimization
    }
    
    #endregion
}