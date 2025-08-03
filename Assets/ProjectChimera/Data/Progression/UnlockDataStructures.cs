using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Data.Progression.Unlocks
{
    /// <summary>
    /// Unlock and progression gate data structures extracted from ProgressionDataStructures.cs
    /// Contains unlockable content, milestones, requirements, and dynamic progression systems
    /// Part of Phase 1 Foundation Data Structures refactoring
    /// </summary>

    #region Core Unlock Types

    /// <summary>
    /// Unlockable content types
    /// </summary>
    public enum UnlockableContentType
    {
        Skill_Node,
        Research_Project,
        Equipment,
        Facility_Type,
        Feature,
        Achievement,
        Cosmetic,
        Recipe,
        Blueprint,
        Area,
        Mode,
        Tutorial,
        Tool,
        Process
    }

    /// <summary>
    /// Milestone requirement types
    /// </summary>
    public enum MilestoneRequirementType
    {
        SkillLevel,
        Player_Level,
        Skill_Level,
        ResearchCompletion,
        Research_Completion,
        AchievementUnlock,
        Achievement_Unlock,
        FacilityConstruction,
        Facility_Construction,
        ProfitGeneration,
        Profit_Generation,
        QualityThreshold,
        Quality_Achievement,
        TimeInGame,
        Time_Played,
        ContentUnlock,
        Achievement,
        Category,
        Points,
        Level,
        Custom
    }

    /// <summary>
    /// Milestone reward types
    /// </summary>
    public enum MilestoneRewardType
    {
        Permanent_Bonus,
        Unlock_Feature,
        Unlock_Skill,
        Unlock_Research,
        Unlock_Equipment,
        Experience_Bonus,
        Currency_Reward,
        Skill_Points,
        Achievement_Points,
        Cosmetic_Unlock,
        Title_Unlock,
        Badge_Unlock
    }

    /// <summary>
    /// Unlock requirement types
    /// </summary>
    public enum UnlockRequirementType
    {
        Level_Requirement,
        Skill_Requirement,
        Achievement_Requirement,
        Research_Requirement,
        Currency_Requirement,
        Time_Requirement,
        Quality_Requirement,
        Quantity_Requirement,
        Social_Requirement,
        Event_Requirement
    }

    /// <summary>
    /// Milestone calculation types
    /// </summary>
    public enum MilestoneCalculationType
    {
        Simple_Count,
        Weighted_Sum,
        Percentage_Based,
        Threshold_Based,
        Time_Based,
        Quality_Based,
        Efficiency_Based,
        Custom_Formula
    }

    /// <summary>
    /// Unlock condition types
    /// </summary>
    public enum UnlockConditionType
    {
        Single_Requirement,
        All_Requirements,
        Any_Requirement,
        Weighted_Requirements,
        Sequential_Requirements,
        Conditional_Requirements,
        Time_Gated,
        Event_Triggered
    }

    #endregion

    #region Unlockable Content System

    /// <summary>
    /// Unlockable content definition
    /// </summary>
    [System.Serializable]
    public class UnlockableContent
    {
        public string ContentId;
        public string ContentName;
        public string Description;
        public UnlockableContentType ContentType;
        public List<UnlockRequirement> Requirements = new List<UnlockRequirement>();
        public bool IsUnlocked = false;
        public DateTime UnlockDate;
        public string IconPath;
        public int UnlockCost = 0;
        public UnlockConditionType ConditionType = UnlockConditionType.All_Requirements;
        public bool IsVisible = true;
        public bool IsRepeatable = false;
        public int Priority = 0;
        public List<string> Tags = new List<string>();
        public Dictionary<string, object> ContentData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Unlock requirement definition
    /// </summary>
    [System.Serializable]
    public class UnlockRequirement
    {
        public string RequirementId;
        public UnlockRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
        public bool IsOptional = false;
        public float Weight = 1f;
        public bool IsCompleted = false;
        public DateTime CompletionDate;
        public Dictionary<string, object> RequirementData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Content unlock event
    /// </summary>
    [System.Serializable]
    public class ContentUnlockEvent
    {
        public string EventId;
        public string PlayerId;
        public string ContentId;
        public DateTime UnlockTime;
        public List<string> TriggeringSources = new List<string>();
        public bool WasCelebrated = false;
        public Dictionary<string, object> UnlockContext = new Dictionary<string, object>();
        public float PlayerProgressAtUnlock = 0f;
        public bool IsFirstTimeUnlock = true;
    }

    /// <summary>
    /// Unlock progress tracker
    /// </summary>
    [System.Serializable]
    public class UnlockProgressTracker
    {
        public string PlayerId;
        public Dictionary<string, float> ContentProgress = new Dictionary<string, float>();
        public Dictionary<string, bool> ContentUnlocked = new Dictionary<string, bool>();
        public Dictionary<string, DateTime> UnlockDates = new Dictionary<string, DateTime>();
        public List<string> RecentUnlocks = new List<string>();
        public DateTime LastProgressUpdate;
        
        public float GetProgress(string contentId)
        {
            return ContentProgress.GetValueOrDefault(contentId, 0f);
        }
        
        public bool IsUnlocked(string contentId)
        {
            return ContentUnlocked.GetValueOrDefault(contentId, false);
        }
        
        public void UnlockContent(string contentId)
        {
            ContentUnlocked[contentId] = true;
            ContentProgress[contentId] = 1f;
            UnlockDates[contentId] = DateTime.Now;
            RecentUnlocks.Add(contentId);
            
            // Keep only recent unlocks (last 10)
            if (RecentUnlocks.Count > 10)
            {
                RecentUnlocks.RemoveAt(0);
            }
        }
    }

    #endregion

    #region Milestone System

    /// <summary>
    /// Campaign milestone definition
    /// </summary>
    [System.Serializable]
    public class CampaignMilestone
    {
        public string MilestoneId;
        public string MilestoneName;
        public string Description;
        public CampaignPhase RequiredPhase;
        public List<MilestoneRequirement> Requirements = new List<MilestoneRequirement>();
        public List<MilestoneReward> Rewards = new List<MilestoneReward>();
        public bool IsCompleted = false;
        public DateTime CompletionDate;
        public DateTime EstimatedCompletionTime;
        public float EstimatedDurationHours = 1f; // Duration in hours for this milestone
        public MilestoneCalculationType CalculationType = MilestoneCalculationType.Simple_Count;
        public bool IsOptional = false;
        public int SortOrder = 0;
        public List<string> PrerequisiteMilestones = new List<string>();
    }

    /// <summary>
    /// Milestone requirement structure
    /// </summary>
    [System.Serializable]
    public class MilestoneRequirement
    {
        public MilestoneRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
        
        // Additional properties for BaseAchievementTrigger compatibility
        public AchievementCategory CategoryTarget; // For category-based requirements
        public int RequiredCount; // Number of items required (achievements, etc.)
        public float Weight = 1f; // Weight for weighted progress calculations
        public bool IsOptional = false;
        public float CurrentProgress = 0f;
        public DateTime LastProgressUpdate;
    }

    /// <summary>
    /// Milestone reward structure
    /// </summary>
    [System.Serializable]
    public class MilestoneReward
    {
        public string RewardID = "";
        public string MilestoneID = "";
        public MilestoneRewardType RewardType;
        public float RewardValue;
        public string RewardDescription;
        public string Description; // Make it a regular property instead of computed
        public SkillNodeSO UnlockedSkillNode;
        public string UnlockedFeature;
        public bool IsPermanentBonus = true;
        public bool IsAutomatic = true;
        public bool RequiresChoice = false;
        public List<string> RewardOptions = new List<string>();
        
        // Compatibility properties for ComprehensiveProgressionManager
        public MilestoneRewardType Type => RewardType; // Compatibility alias
        public string TargetId => UnlockedFeature ?? UnlockedSkillNode?.SkillId ?? ""; // Compatibility alias
    }

    /// <summary>
    /// Dynamic milestone that adapts to player behavior
    /// </summary>
    [System.Serializable]
    public class DynamicMilestone
    {
        public string MilestoneId;
        public string BaseName;
        public string Description;
        public List<DynamicParameter> Parameters = new List<DynamicParameter>();
        public DateTime CreationTime;
        public DateTime ExpirationTime;
        public bool IsActive = true;
        public float DifficultyMultiplier = 1f;
        public List<MilestoneRequirement> GeneratedRequirements = new List<MilestoneRequirement>();
        public List<MilestoneReward> GeneratedRewards = new List<MilestoneReward>();
        public string GenerationReason;
    }

    /// <summary>
    /// Dynamic parameter for milestone generation
    /// </summary>
    [System.Serializable]
    public class DynamicParameter
    {
        public string ParameterName;
        public ParameterType ParameterType;
        public float BaseValue;
        public float CurrentValue;
        public float MinValue;
        public float MaxValue;
        public bool IsAdaptive = true;
        public float AdaptationRate = 0.1f;
    }

    /// <summary>
    /// Milestone context for tracking conditions
    /// </summary>
    [System.Serializable]
    public class MilestoneContext
    {
        public string PlayerId;
        public string SessionId;
        public DateTime StartTime;
        public Dictionary<string, float> ProgressValues = new Dictionary<string, float>();
        public Dictionary<string, object> ContextData = new Dictionary<string, object>();
        public List<string> ActiveMilestones = new List<string>();
        public bool IsMultiplayer = false;
        public string CurrentActivity;
        public float SessionDuration = 0f;
    }

    /// <summary>
    /// Milestone progress tracking
    /// </summary>
    [System.Serializable]
    public class MilestoneProgress
    {
        public string MilestoneId;
        public string PlayerId;
        public float CurrentProgress = 0f;
        public float RequiredProgress = 1f;
        public bool IsCompleted = false;
        public DateTime StartTime;
        public DateTime LastUpdateTime;
        public List<ProgressStep> ProgressSteps = new List<ProgressStep>();
        public Dictionary<string, float> SubProgress = new Dictionary<string, float>();
        public float EstimatedTimeToComplete = -1f;
    }

    /// <summary>
    /// Progress step within a milestone
    /// </summary>
    [System.Serializable]
    public class ProgressStep
    {
        public DateTime Timestamp;
        public float ProgressDelta;
        public string ActionType;
        public string Description;
        public Dictionary<string, object> StepData = new Dictionary<string, object>();
        public string RequirementId;
        public float RequirementProgress;
    }

    #endregion

    #region Progression Gates

    /// <summary>
    /// Progression gate that blocks access until conditions are met
    /// </summary>
    [System.Serializable]
    public class ProgressionGate
    {
        public string GateId;
        public string GateName;
        public string Description;
        public GateType GateType;
        public List<GateCondition> Conditions = new List<GateCondition>();
        public bool IsOpen = false;
        public DateTime OpenTime;
        public string BlockedContent; // What this gate blocks access to
        public List<string> BlockedContentIds = new List<string>();
        public bool IsVisible = true;
        public string HintText;
        public int Priority = 0;
    }

    /// <summary>
    /// Gate condition definition
    /// </summary>
    [System.Serializable]
    public class GateCondition
    {
        public string ConditionId;
        public GateConditionType ConditionType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
        public bool IsSatisfied = false;
        public DateTime SatisfiedTime;
        public float CurrentValue = 0f;
        public bool IsOptional = false;
        public float Weight = 1f;
    }

    /// <summary>
    /// Gate event for tracking gate state changes
    /// </summary>
    [System.Serializable]
    public class GateEvent
    {
        public string EventId;
        public string GateId;
        public string PlayerId;
        public GateEventType EventType;
        public DateTime EventTime;
        public string TriggerReason;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public bool WasNotified = false;
    }

    #endregion

    #region Progression Pathways

    /// <summary>
    /// Represents a progression pathway
    /// </summary>
    [System.Serializable]
    public class ProgressionPathway
    {
        public string PathwayId;
        public string PathwayName;
        public string Description;
        public PathwayType PathwayType;
        public List<PathwayNode> Nodes = new List<PathwayNode>();
        public List<string> Prerequisites = new List<string>();
        public bool IsUnlocked = false;
        public bool IsCompleted = false;
        public float CompletionPercentage = 0f;
        public float ProgressionBonus = 1.1f;
        public int EstimatedDurationDays = 30;
        public PathwayDifficulty Difficulty = PathwayDifficulty.Normal;
        public List<PathwayReward> CompletionRewards = new List<PathwayReward>();
    }

    /// <summary>
    /// Node within a progression pathway
    /// </summary>
    [System.Serializable]
    public class PathwayNode
    {
        public string NodeId;
        public string NodeName;
        public string Description;
        public NodeType NodeType;
        public List<string> Prerequisites = new List<string>();
        public List<NodeRequirement> Requirements = new List<NodeRequirement>();
        public List<NodeReward> Rewards = new List<NodeReward>();
        public bool IsCompleted = false;
        public DateTime CompletionTime;
        public Vector2 NodePosition; // For UI layout
        public bool IsOptional = false;
    }

    /// <summary>
    /// Requirement for pathway node
    /// </summary>
    [System.Serializable]
    public class NodeRequirement
    {
        public string RequirementId;
        public NodeRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
        public bool IsCompleted = false;
        public float CurrentProgress = 0f;
    }

    /// <summary>
    /// Reward for completing pathway node
    /// </summary>
    [System.Serializable]
    public class NodeReward
    {
        public string RewardId;
        public NodeRewardType RewardType;
        public float RewardValue;
        public string Description;
        public bool IsAutomatic = true;
        public Dictionary<string, object> RewardData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Reward for completing entire pathway
    /// </summary>
    [System.Serializable]
    public class PathwayReward
    {
        public string RewardId;
        public PathwayRewardType RewardType;
        public float RewardValue;
        public string Description;
        public bool IsPrestige = false;
        public bool IsUnique = true;
    }

    /// <summary>
    /// Progress tracking for pathways
    /// </summary>
    [System.Serializable]
    public class PathwayProgress
    {
        public string PathwayId;
        public string PlayerId;
        public List<string> CompletedNodes = new List<string>();
        public Dictionary<string, float> NodeProgress = new Dictionary<string, float>();
        public float OverallProgress = 0f;
        public DateTime StartTime;
        public DateTime LastProgressTime;
        public bool IsActive = true;
        public int CurrentNodeIndex = 0;
        public string CurrentNodeId;
    }

    #endregion

    #region Category Mastery System

    /// <summary>
    /// Category mastery tracking
    /// </summary>
    [System.Serializable]
    public class CategoryMastery
    {
        public AchievementCategory Category;
        public int CompletedAchievements = 0;
        public int TotalAchievements = 0;
        public float MasteryPercentage = 0f;
        public MasteryLevel MasteryLevel = MasteryLevel.Novice;
        public int ProgressionPoints;
        public List<MasteryBonus> MasteryBonuses = new List<MasteryBonus>();
        public DateTime LastProgressUpdate;
        public bool IsElite = false;
        public float EliteThreshold = 0.95f;
    }

    /// <summary>
    /// Mastery bonus for category completion
    /// </summary>
    [System.Serializable]
    public class MasteryBonus
    {
        public string BonusId;
        public string BonusName;
        public MasteryBonusType BonusType;
        public float BonusValue;
        public MasteryLevel RequiredLevel;
        public string Description;
        public bool IsActive = true;
        public bool IsStackable = false;
        public DateTime UnlockTime;
    }

    #endregion

    #region Analytics and Management

    /// <summary>
    /// Milestone system metrics
    /// </summary>
    [System.Serializable]
    public class MilestoneSystemMetrics
    {
        public int TotalMilestones = 0;
        public int CompletedMilestones = 0;
        public int ActiveMilestones = 0;
        public float AverageCompletionTime = 0f;
        public float CompletionRate = 0f;
        public Dictionary<MilestoneRequirementType, int> RequirementTypeDistribution = new Dictionary<MilestoneRequirementType, int>();
        public Dictionary<MilestoneRewardType, int> RewardTypeDistribution = new Dictionary<MilestoneRewardType, int>();
        public DateTime LastMetricsUpdate;
        public float PlayerEngagementScore = 0f;
        public List<string> MostCompletedMilestones = new List<string>();
        public List<string> LeastCompletedMilestones = new List<string>();
    }

    /// <summary>
    /// Milestone tracker for progress management
    /// </summary>
    [System.Serializable]
    public class MilestoneTracker
    {
        public string PlayerId;
        public Dictionary<string, MilestoneProgress> TrackedMilestones = new Dictionary<string, MilestoneProgress>();
        public List<string> CompletedMilestones = new List<string>();
        public DateTime LastUpdate;
        public bool IsActive = true;
    }

    /// <summary>
    /// Dynamic milestone generator
    /// </summary>
    [System.Serializable]
    public class DynamicMilestoneGenerator
    {
        public string GeneratorId;
        public List<MilestoneTemplate> Templates = new List<MilestoneTemplate>();
        public Dictionary<string, float> PlayerBehaviorData = new Dictionary<string, float>();
        public DateTime LastGeneration;
        public int MaxActiveDynamicMilestones = 5;
        public bool IsEnabled = true;
    }

    /// <summary>
    /// Template for generating dynamic milestones
    /// </summary>
    [System.Serializable]
    public class MilestoneTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public string DescriptionTemplate;
        public List<RequirementTemplate> RequirementTemplates = new List<RequirementTemplate>();
        public List<RewardTemplate> RewardTemplates = new List<RewardTemplate>();
        public float BaseWeight = 1f;
        public List<string> TriggerConditions = new List<string>();
    }

    /// <summary>
    /// Template for milestone requirements
    /// </summary>
    [System.Serializable]
    public class RequirementTemplate
    {
        public MilestoneRequirementType RequirementType;
        public string TargetPattern; // Pattern for generating target IDs
        public float BaseValue;
        public float ValueMultiplier = 1f;
        public bool IsScalable = true;
    }

    /// <summary>
    /// Template for milestone rewards
    /// </summary>
    [System.Serializable]
    public class RewardTemplate
    {
        public MilestoneRewardType RewardType;
        public float BaseValue;
        public float ValueMultiplier = 1f;
        public bool IsScalable = true;
        public List<string> RewardOptions = new List<string>();
    }

    /// <summary>
    /// Milestone analytics engine
    /// </summary>
    [System.Serializable]
    public class MilestoneAnalyticsEngine
    {
        public Dictionary<string, float> MilestoneCompletionRates = new Dictionary<string, float>();
        public Dictionary<string, float> AverageCompletionTimes = new Dictionary<string, float>();
        public List<MilestoneInsight> GeneratedInsights = new List<MilestoneInsight>();
        public DateTime LastAnalysis;
        public bool IsEnabled = true;
    }

    /// <summary>
    /// Insight generated from milestone analytics
    /// </summary>
    [System.Serializable]
    public class MilestoneInsight
    {
        public string InsightId;
        public InsightType InsightType;
        public string InsightText;
        public float Confidence;
        public DateTime GeneratedTime;
        public List<string> SupportingData = new List<string>();
        public bool IsActionable = true;
    }

    #endregion

    #region Event Data Structures

    /// <summary>
    /// Milestone event data
    /// </summary>
    [System.Serializable]
    public class MilestoneEventData
    {
        public string EventId;
        public string MilestoneId;
        public string PlayerId;
        public MilestoneEventType EventType;
        public DateTime EventTime;
        public float ProgressBefore;
        public float ProgressAfter;
        public string TriggerSource;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }

    #endregion

    #region Supporting Enums

    /// <summary>
    /// Campaign phase enumeration
    /// </summary>
    public enum CampaignPhase
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Endgame,
        Expansion,
        Custom
    }

    /// <summary>
    /// Achievement category enumeration (for compatibility)
    /// </summary>
    public enum AchievementCategory
    {
        Cultivation_Mastery,
        Genetics_Innovation,
        Research_Excellence,
        Business_Success,
        Teaching_Mentorship,
        Collaboration_Leadership,
        Quality_Achievement,
        Efficiency_Optimization,
        Innovation_Pioneer,
        Community_Builder,
        Breeding,
        Social,
        Genetics,
        Aromatic,
        Ultimate,
        Special
    }

    public enum GateType
    {
        Level_Gate,
        Achievement_Gate,
        Skill_Gate,
        Progress_Gate,
        Time_Gate,
        Event_Gate,
        Social_Gate,
        Custom_Gate
    }

    public enum GateConditionType
    {
        Level_Requirement,
        Skill_Level,
        Achievement_Count,
        Research_Complete,
        Time_Played,
        Quality_Achieved,
        Progress_Percentage,
        Custom_Condition
    }

    public enum GateEventType
    {
        Gate_Opened,
        Gate_Closed,
        Condition_Met,
        Condition_Failed,
        Progress_Updated
    }

    public enum PathwayType
    {
        Linear,
        Branching,
        Circular,
        Hub_And_Spoke,
        Free_Form,
        Guided,
        Challenge_Based
    }

    public enum PathwayDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        Master
    }

    public enum NodeType
    {
        Start_Node,
        Progress_Node,
        Choice_Node,
        Challenge_Node,
        Reward_Node,
        Gate_Node,
        End_Node
    }

    public enum NodeRequirementType
    {
        Level_Requirement,
        Skill_Requirement,
        Achievement_Requirement,
        Time_Requirement,
        Quality_Requirement,
        Quantity_Requirement
    }

    public enum NodeRewardType
    {
        Experience_Points,
        Skill_Points,
        Currency,
        Item_Unlock,
        Feature_Unlock,
        Bonus_Multiplier
    }

    public enum PathwayRewardType
    {
        Mastery_Badge,
        Prestige_Points,
        Exclusive_Content,
        Title_Unlock,
        Permanent_Bonus,
        Special_Recognition
    }

    public enum MasteryLevel
    {
        Novice,
        Apprentice,
        Journeyman,
        Expert,
        Master,
        Grandmaster
    }

    public enum MasteryBonusType
    {
        Experience_Multiplier,
        Efficiency_Bonus,
        Quality_Bonus,
        Speed_Bonus,
        Cost_Reduction,
        Unlock_Access,
        Prestige_Benefit
    }

    public enum ParameterType
    {
        Integer,
        Float,
        Boolean,
        String,
        Enum,
        List,
        Dictionary
    }

    public enum InsightType
    {
        Performance_Insight,
        Engagement_Insight,
        Difficulty_Insight,
        Progress_Insight,
        Recommendation,
        Warning,
        Opportunity
    }

    public enum MilestoneEventType
    {
        Milestone_Started,
        Milestone_Progress,
        Milestone_Completed,
        Milestone_Failed,
        Milestone_Abandoned,
        Requirement_Met,
        Reward_Claimed
    }

    #endregion
}