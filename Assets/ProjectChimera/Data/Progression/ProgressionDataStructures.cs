using System;
using UnityEngine;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Additional data structures for the progression system that extend the existing framework.
    /// Includes achievement definitions, experience tracking, and specialized progression mechanics.
    /// </summary>
    
    [System.Serializable]
    public class AchievementDefinition
    {
        public string AchievementId;
        public string AchievementName;
        public AchievementCategory Category;
        public AchievementType Type;
        [TextArea(2, 4)] public string Description;
        public Sprite Icon;
        
        [Header("Requirements")]
        public AchievementRequirements Requirements;
        public bool IsHidden = false;
        public bool IsRepeatable = false;
        
        [Header("Rewards")]
        public List<AchievementReward> Rewards = new List<AchievementReward>();
        public float ExperienceReward = 100f;
        public int SkillPointReward = 1;
        
        [Header("Progress Tracking")]
        public List<ProgressCondition> ProgressConditions = new List<ProgressCondition>();
        public bool TrackProgress = true;
    }
    
    [System.Serializable]
    public class AchievementRequirements
    {
        public int MinimumPlayerLevel = 1;
        public List<SkillNodeSO> RequiredSkills = new List<SkillNodeSO>();
        public List<string> RequiredAchievements = new List<string>();
        public List<ResearchProjectSO> RequiredResearch = new List<ResearchProjectSO>();
        public bool RequiresSpecificFacility = false;
        public string RequiredFacilityType;
        
        // Compatibility property for ComprehensiveProgressionManager
        public List<AchievementRequirement> Requirements = new List<AchievementRequirement>();
    }
    
    [System.Serializable]
    public class AchievementReward
    {
        public string RewardID;
        public string AchievementID;
        public AchievementRewardType RewardType;
        public RewardType Type; // Additional type field for compatibility
        public string RewardDescription;
        public string Description; // Alias for RewardDescription
        public float RewardValue;
        public float Amount { get => RewardValue; set => RewardValue = value; } // Writable alias for RewardValue for compatibility
        public DateTime DateAwarded;
        public SkillNodeSO UnlockedSkill;
        public ResearchProjectSO UnlockedResearch;
        public EquipmentDataSO UnlockedEquipment;
        public string UnlockedFeature;

        // Default constructor
        public AchievementReward() { }

        // Copy constructor for the AchievementBaseClasses.cs usage
        public AchievementReward(AchievementReward other)
        {
            RewardType = other.RewardType;
            Type = other.Type;
            RewardDescription = other.RewardDescription;
            Description = other.Description;
            RewardValue = other.RewardValue;
            UnlockedSkill = other.UnlockedSkill;
            UnlockedResearch = other.UnlockedResearch;
            UnlockedEquipment = other.UnlockedEquipment;
            UnlockedFeature = other.UnlockedFeature;
        }
    }
    
    [System.Serializable]
    public class ProgressCondition
    {
        public ProgressType ProgressType;
        public string TargetId;
        public float RequiredValue;
        public ComparisonType Comparison;
        public string Description;
    }
    
    /// <summary>
    /// Player progression data tracking all advancement across the game
    /// </summary>
    [System.Serializable]
    public class PlayerProgressionData
    {
        [Header("Player Identity")]
        public string PlayerId;
        public int PlayerLevel = 1;
        public float TotalExperience = 0f;
        public int AvailableSkillPoints = 0;
        public float PlayTimeHours = 0f;
        public System.DateTime CreationDate;
        public System.DateTime LastPlayDate;
        
        [Header("Completed Content")]
        public List<string> CompletedAchievements = new List<string>();
        public List<string> CompletedResearch = new List<string>();
        public List<string> UnlockedSkills = new List<string>();
        public List<string> UnlockedContent = new List<string>();
        public List<string> CompletedMilestones = new List<string>();
        public List<string> CompletedObjectives = new List<string>();
        
        [Header("Campaign Progress")]
        public CampaignProgressionData CampaignProgress = new CampaignProgressionData();
        public CampaignProgressionData CampaignData = new CampaignProgressionData(); // Alias for compatibility
        
        [Header("Statistics")]
        public PlayerStatTracker StatTracker = new PlayerStatTracker();
        public int FacilitiesBuilt = 0;
        public float TotalProfit = 0f;
        public int ResearchCompleted = 0;
        public int AchievementsUnlocked = 0;
        
        [Header("Skill Levels")]
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
        public Dictionary<string, float> SkillExperience = new Dictionary<string, float>();
        
        [Header("Preferences")]
        public ProgressionPreferences Preferences = new ProgressionPreferences();
        
        // Progression version for save compatibility
        public string ProgressionVersion = "1.0";
        public int OverallLevel = 1;
        
        public int GetSkillLevel(string skillId)
        {
            return SkillLevels.ContainsKey(skillId) ? SkillLevels[skillId] : 1;
        }
        
        public float GetSkillExperience(string skillId)
        {
            return SkillExperience.ContainsKey(skillId) ? SkillExperience[skillId] : 0f;
        }
        
        public bool HasCompletedAchievement(string achievementId)
        {
            return CompletedAchievements.Contains(achievementId);
        }
        
        public bool HasUnlockedSkill(string skillId)
        {
            return UnlockedSkills.Contains(skillId);
        }
        
        public bool HasCompletedResearch(string researchId)
        {
            return CompletedResearch.Contains(researchId);
        }
        
        public bool HasUnlockedContent(string contentId)
        {
            return UnlockedContent.Contains(contentId);
        }
    }
    
    /// <summary>
    /// Campaign progression data for tracking story and phase progress
    /// </summary>
    [System.Serializable]
    public class CampaignProgressionData
    {
        public CampaignPhase CurrentPhase = CampaignPhase.Tutorial;
        public List<string> CompletedMilestones = new List<string>();
        public List<string> UnlockedStoryContent = new List<string>();
        public float CampaignProgress = 0f;
        public System.DateTime LastCampaignUpdate;
        public Dictionary<string, bool> PhaseCompletionStatus = new Dictionary<string, bool>();
        
        // Additional properties needed by CampaignManager
        public float PhaseProgress = 0f;
        public System.DateTime PhaseStartTime;
        public List<CampaignPhase> CompletedPhases = new List<CampaignPhase>();
        public List<string> ReachedMilestones = new List<string>();
    }
    
    /// <summary>
    /// Player statistics tracker for detailed progression analytics
    /// </summary>
    [System.Serializable]
    public class PlayerStatTracker
    {
        [Header("Cultivation Stats")]
        public int PlantsGrown = 0;
        public int PlantsHarvested = 0;
        public int SuccessfulBreeds = 0;
        public float TotalYield = 0f;
        public float BestQualityAchieved = 0f;
        
        [Header("Research Stats")]
        public int ResearchProjectsCompleted = 0;
        public float TotalResearchTime = 0f;
        public int InnovationsDiscovered = 0;
        public int PatentsEarned = 0;
        
        [Header("Business Stats")]
        public float TotalRevenue = 0f;
        public float TotalProfit = 0f; // Alias for compatibility
        public float TotalExpenses = 0f;
        public int SuccessfulSales = 0;
        public float BestSalePrice = 0f;
        public int FacilitiesConstructed = 0;
        public int FacilitiesBuilt = 0; // Alias for compatibility
        
        [Header("Teaching Stats")]
        public int StudentsTeaching = 0;
        public int LessonsGiven = 0;
        public float TeachingEffectiveness = 0f;
        public int MentorshipPrograms = 0;
        
        [Header("Time Stats")]
        public float TotalPlayTime = 0f;
        public int SessionsPlayed = 0;
        public System.DateTime FirstPlayDate;
        public System.DateTime LastActiveDate;
    }
    
    /// <summary>
    /// Player progression preferences and settings
    /// </summary>
    [System.Serializable]
    public class ProgressionPreferences
    {
        [Header("Notification Preferences")]
        public bool ShowSkillLevelUpNotifications = true;
        public bool ShowAchievementNotifications = true;
        public bool ShowResearchCompletionNotifications = true;
        public bool ShowMilestoneNotifications = true;
        
        [Header("Progression Settings")]
        public bool AutoSaveProgression = true;
        public bool EnableProgressionAnalytics = true;
        public bool ShowProgressionTips = true;
        public bool EnableProgressionEffects = true;
        
        [Header("Difficulty Preferences")]
        public float ExperienceMultiplier = 1f;
        public float ProgressionDifficulty = 1f;
        public bool EnableHardcoreMode = false;
        public bool EnableChallengeMode = false;
    }
    
    /// <summary>
    /// Campaign milestone data structure
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
        public System.DateTime CompletionDate;
        public System.DateTime EstimatedCompletionTime;
        public float EstimatedDurationHours = 1f; // Duration in hours for this milestone
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
    }
    
    /// <summary>
    /// Milestone reward structure
    /// </summary>
    [System.Serializable]
    public class MilestoneReward
    {
        public MilestoneRewardType RewardType;
        public float RewardValue;
        public string RewardDescription;
        public SkillNodeSO UnlockedSkillNode;
        public string UnlockedFeature;
        public bool IsPermanentBonus = true;
        
        // Compatibility properties for ComprehensiveProgressionManager
        public MilestoneRewardType Type => RewardType; // Compatibility alias
        public string TargetId => UnlockedFeature ?? UnlockedSkillNode?.SkillId ?? ""; // Compatibility alias
        public float Amount => RewardValue; // Compatibility alias
    }
    
    // Enumerations
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
    
    public enum AchievementType
    {
        One_Time,
        Progressive,
        Repeatable,
        Seasonal,
        Hidden,
        Legendary,
        SkillLevel
    }
    
    public enum AchievementRewardType
    {
        Experience_Bonus,
        Skill_Points,
        Unlock_Skill,
        Unlock_Research,
        Unlock_Equipment,
        Unlock_Feature,
        Permanent_Bonus,
        Cosmetic_Reward
    }
    
    public enum ProgressType
    {
        Count,
        Accumulation,
        Achievement,
        Ratio,
        Quality_Threshold,
        Time_Based,
        Skill_Level,
        Research_Completion
    }
    
    public enum ComparisonType
    {
        Equals,
        Greater_Than,
        Greater_Equal,
        Less_Than,
        Less_Equal,
        Not_Equals
    }
    
    public enum CampaignPhase
    {
        Tutorial,
        EarlyGrowth,
        Expansion,
        Specialization,
        Innovation,
        Mastery,
        Legacy
    }
    
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
    
    public enum MilestoneRewardType
    {
        Permanent_Bonus,
        Unlock_Feature,
        Unlock_Skill,
        Title_Recognition,
        Special_Access,
        Multiplier_Bonus,
        Unique_Opportunity,
        Legacy_Achievement
    }
    
    public enum ExperienceSource
    {
        Plant_Harvest,
        Breeding_Success,
        Research_Completion,
        Quality_Achievement,
        Facility_Completion,
        Skill_Usage,
        Achievement_Unlock,
        Quest_Completion,
        Passive_Time,
        PlantCare,
        Teaching_Others,
        Collaboration,
        Gameplay,
        Research,
        Teaching,
        Innovation,
        Achievement,
        Challenge,
        Mentorship,
        Tutorial_Completion
    }
    
    public enum RewardType
    {
        Experience,
        Currency,
        AchievementPoints,
        SkillPoints,
        UnlockFeature,
        Title,
        Item,
        Items,
        Multiplier,
        ContentUnlock,
        Skill_Point,
        Research_Unlock
    }
    
    public enum SynergyType
    {
        Domain_Synergy,
        Category_Synergy,
        Cross_Domain,
        Complementary,
        Reinforcing,
        Multiplicative
    }
    
    public enum LearningPathType
    {
        Beginner_Path,
        Specialist_Path,
        Generalist_Path,
        Expert_Path,
        Innovation_Path,
        Efficiency_Path,
        Quality_Path,
        Research_Path
    }
    
    /// <summary>
    /// Collaboration opportunity for research and learning
    /// </summary>
    [System.Serializable]
    public class CollaborationOpportunity
    {
        public string OpportunityId;
        public string Title;
        public string OpportunityName; // Compatibility alias for Title
        public string Description;
        public CollaborationType Type;
        public CollaborationType CollaborationType; // Compatibility alias for Type
        public List<string> RequiredSkills = new List<string>();
        public List<SkillCategory> RequiredExpertise = new List<SkillCategory>(); // For ResearchManager compatibility
        public List<string> RequiredResearch = new List<string>();
        public int MinPlayerLevel = 1;
        public int MinimumSkillLevel = 3; // For ResearchManager compatibility
        public float DurationDays = 30f;
        public List<CollaborationBenefit> Benefits = new List<CollaborationBenefit>();
        public float SuccessChance = 0.8f;
        public bool IsAvailable = true;
        public System.DateTime AvailableUntil;
    }
    
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
        public System.DateTime UnlockDate;
        public string IconPath;
        public int UnlockCost = 0;
    }
    
    /// <summary>
    /// Learning accelerator for skill progression
    /// </summary>
    [System.Serializable]
    public class LearningAccelerator
    {
        public string AcceleratorId;
        public string Name;
        public string Description;
        public AcceleratorType Type;
        public AcceleratorType AcceleratorType; // Compatibility alias
        public float ExperienceMultiplier = 1.5f;
        public float SpeedMultiplier = 1.5f; // For SkillTreeManager compatibility
        public float DurationHours = 24f;
        public float DurationDays = 1f; // DurationHours / 24
        public List<string> ApplicableSkills = new List<string>();
        public List<SkillCategory> ApplicableCategories = new List<SkillCategory>(); // For SkillTreeManager compatibility
        public List<AcceleratorRequirement> Requirements = new List<AcceleratorRequirement>();
        public int Cost = 100;
        public bool IsReusable = false;
    }
    
    /// <summary>
    /// Synergy effect definition
    /// </summary>
    [System.Serializable]
    public class SynergyEffect
    {
        public string EffectName;
        public SkillEffectType EffectType;
        public float BonusMultiplier = 1.2f;
        public string Description;
        
        // Additional properties for SkillTreeManager compatibility
        public bool RequiresBothSkills = true; // Whether both skills are required for this effect
        public int MinimumCombinedLevel = 5; // Minimum combined level for this effect
    }
    
    /// <summary>
    /// Skill synergy for bonus effects
    /// </summary>
    [System.Serializable]
    public class SkillSynergy
    {
        public string SynergyId;
        public string Name;
        public string Description;
        public List<string> RequiredSkills = new List<string>();
        public SynergyType SynergyType;
        public float BonusMultiplier = 1.2f;
        public List<string> AffectedSkills = new List<string>();
        public List<SynergyEffect> SynergyEffects = new List<SynergyEffect>();
        public bool IsActive = false;
        
        // Additional properties for SkillTreeManager compatibility
        public SkillNodeSO PrimarySkill; // Primary skill for synergy
        public SkillNodeSO SecondarySkill; // Secondary skill for synergy
        public float SynergyStrength = 1.0f; // Strength of the synergy effect
        
        // Method for checking if both skills are required
        public bool RequiresBothSkills()
        {
            return PrimarySkill != null && SecondarySkill != null;
        }
        
        public int MinimumCombinedLevel = 5; // Minimum combined level for synergy activation
    }
    
    /// <summary>
    /// Expertise area definition
    /// </summary>
    [System.Serializable]
    public class ExpertiseArea
    {
        public string AreaId;
        public string Name;
        public string Description;
        public List<SkillCategory> RelevantCategories = new List<SkillCategory>();
        public int RequiredSkillCount = 5;
        public int RequiredMasteryCount = 3;
        public float ExpertiseThreshold = 0.8f;
        public List<ExpertiseBenefit> Benefits = new List<ExpertiseBenefit>();
    }
    
    /// <summary>
    /// Learning path definition
    /// </summary>
    [System.Serializable]
    public class LearningPath
    {
        public string PathId;
        public string Name;
        public string Description;
        public LearningPathType PathType;
        public List<string> SkillSequence = new List<string>();
        public List<PathMilestone> Milestones = new List<PathMilestone>();
        public float EstimatedDurationDays = 30f;
        public int DifficultyLevel = 1;
    }
    
    // Supporting enums and classes
    // CollaborationType is defined in ResearchDataStructures.cs
    
    [System.Serializable]
    public class SkillTreeBranch
    {
        public string BranchId;
        public string BranchName;
        public string Description;
        public SkillCategory Category;
        public SkillDomain Domain;
        public List<SkillNodeSO> Skills = new List<SkillNodeSO>();
        public Vector2 BranchPosition; // For UI positioning
        public Color BranchColor = Color.white;
        public string IconPath;
        public List<string> PrerequisiteBranches = new List<string>();
        public int UnlockLevel = 1;
        public bool IsVisible = true;
    }
    
    public enum UnlockableContentType
    {
        Skill_Node,
        Research_Project,
        Equipment,
        Facility_Type,
        Feature,
        Achievement,
        Cosmetic
    }
    
    public enum AcceleratorType
    {
        Experience_Boost,
        Learning_Speed,
        Skill_Focus,
        Research_Boost,
        Collaboration_Bonus,
        Innovation_Catalyst,
        Time_Acceleration
    }

    public enum ExpertiseBenefitType
    {
        Experience_Multiplier,
        Skill_Efficiency,
        Research_Speed,
        Quality_Bonus,
        Cost_Reduction,
        Time_Reduction,
        Success_Rate_Bonus,
        Innovation_Rate
    }

    public enum AcceleratorRequirementType
    {
        Money_Investment,
        Skill_Level,
        Reputation_Level,
        Research_Completion,
        Achievement_Unlock,
        Player_Level,
        Time_Investment
    }
    
    [System.Serializable]
    public class CollaborationBenefit
    {
        public string BenefitName;
        public CollaborationBenefitType BenefitType;
        public float BenefitValue;
        public string BenefitDescription;
        public string Description; // Compatibility alias
    }
    
    // UnlockRequirement is defined in UnlockConfigSO.cs
    
    [System.Serializable]
    public class AcceleratorRequirement
    {
        public AcceleratorRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
    }
    
    [System.Serializable]
    public class ExpertiseBenefit
    {
        public ExpertiseBenefitType BenefitType;
        public float BenefitValue;
        public string Description;
        public bool IsPermanent = true;
    }
    
    [System.Serializable]
    public class PathMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public int RequiredSkillsCompleted;
        public List<string> Rewards = new List<string>();
    }

    /// <summary>
    /// Progression metrics for analytics and monitoring
    /// </summary>
    [System.Serializable]
    public class ProgressionMetrics
    {
        public int CurrentSkillLevels;
        public int CompletedAchievements;
        public int ActiveResearchProjects;
        public int CompletedResearchProjects;
        public int UnlockedContent;
        public float TotalExperience;
        public int OverallLevel;
        public System.DateTime LastUpdate;
        public System.DateTime SessionStartTime;
        public double SessionDuration;
        public float ExperiencePerHour;
        public float AchievementRate;
        public float ResearchCompletionRate;
        
        // Compatibility properties for ComprehensiveProgressionManager
        public int TotalSkills => CurrentSkillLevels;
        public int TotalAchievements => CompletedAchievements;
        public int TotalResearchProjects => ActiveResearchProjects + CompletedResearchProjects;
    }

    /// <summary>
    /// Passive bonus from research or achievements
    /// </summary>
    [System.Serializable]
    public class PassiveBonus
    {
        public string BonusId;
        public string BonusName;
        public string Description;
        public PassiveBonusType BonusType;
        public float BonusValue;
        public string TargetSystem;
        public bool IsPermanent = true;
        public float DurationHours = 0f;
        public System.DateTime ExpirationDate;
        
        // Compatibility property
        public float Value => BonusValue; // Compatibility alias
    }

    /// <summary>
    /// Achievement requirement structure (singular)
    /// </summary>
    [System.Serializable]
    public class AchievementRequirement
    {
        public AchievementRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
        public ComparisonType Comparison = ComparisonType.Greater_Equal;
        
        // Compatibility property
        public AchievementRequirementType Type => RequirementType; // Compatibility alias
    }

    /// <summary>
    /// Progression event for event-driven system
    /// </summary>
    [System.Serializable]
    public class ProgressionEvent
    {
        public string EventId;
        public ProgressionEventType EventType;
        public string SourceSystem;
        public string TargetId;
        public float EventValue;
        public string EventData;
        public System.DateTime EventTime;
        public bool IsProcessed = false;
        
        // Additional properties for ComprehensiveProgressionManager
        public string SkillId;
        public float Amount;
        public string Source;
        public System.DateTime Timestamp;
        public int Level;
        public string ResearchId;
        public string AchievementId;
        public string ContentId;
        
        // Compatibility properties for ComprehensiveProgressionManager
        public ProgressionEventType Type => EventType;
        public float ExperienceGained => EventValue;
    }

    /// <summary>
    /// Progression notification for UI
    /// </summary>
    [System.Serializable]
    public class ProgressionNotification
    {
        public string NotificationId;
        public ProgressionNotificationType NotificationType;
        public string Title;
        public string Message;
        public string IconPath;
        public float DisplayDuration = 3f;
        public System.DateTime CreationTime;
        public System.DateTime StartTime; // For ComprehensiveProgressionManager compatibility
        public float Duration = 3f; // For ComprehensiveProgressionManager compatibility
        public bool IsActive = true;
    }

    /// <summary>
    /// Progression session tracking
    /// </summary>
    [System.Serializable]
    public class ProgressionSession
    {
        public string SessionId;
        public System.DateTime StartTime;
        public System.DateTime EndTime;
        public float ExperienceGained;
        public int SkillLevelsGained;
        public int AchievementsUnlocked;
        public int ResearchCompleted;
        public List<string> ActivitiesPerformed = new List<string>();
    }

    /// <summary>
    /// Research progress tracking
    /// </summary>
    [System.Serializable]
    public class ResearchProgress
    {
        public string ResearchId;
        public float Progress = 0f;
        public bool IsActive = false;
        public System.DateTime StartDate;
        public System.DateTime StartTime; // Compatibility alias
        public System.DateTime EstimatedCompletion;
        public System.DateTime CompletionTime; // Compatibility alias
        public float ResearchRate = 1f;
        public List<string> AssignedResearchers = new List<string>();
        public Dictionary<string, float> ResourcesAllocated = new Dictionary<string, float>();
        
        // Additional properties for compatibility
        public float PhaseProgress = 0f;
        public float MilestoneProgress = 0f;
        public float OverallProgress => (PhaseProgress * 0.7f) + (MilestoneProgress * 0.3f);
        public float EstimatedTimeRemaining = 0f;
    }

    /// <summary>
    /// Progression system report
    /// </summary>
    [System.Serializable]
    public class ProgressionSystemReport
    {
        public ProgressionMetrics Metrics;
        public PlayerProgressionData PlayerProgression;
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
        public HashSet<string> UnlockedContent = new HashSet<string>();
        public HashSet<string> CompletedAchievements = new HashSet<string>();
        public List<string> RecentAchievements = new List<string>();
        public List<string> ActiveResearch = new List<string>();
        public List<string> AvailableUnlocks = new List<string>();
        public List<string> RecommendedActions = new List<string>();
        public System.DateTime ReportGenerated;
    }

    /// <summary>
    /// Progression analytics data
    /// </summary>
    [System.Serializable]
    public class ProgressionAnalytics
    {
        public Dictionary<string, float> SkillUsageStats = new Dictionary<string, float>();
        public Dictionary<string, int> AchievementAttempts = new Dictionary<string, int>();
        public Dictionary<string, float> ResearchEfficiency = new Dictionary<string, float>();
        public List<ProgressionSession> SessionHistory = new List<ProgressionSession>();
        public float AverageSessionLength;
        public float TotalPlayTime;
        
        /// <summary>
        /// Update analytics data - called by ComprehensiveProgressionManager
        /// </summary>
        public void Update()
        {
            // Update session tracking
            if (SessionHistory.Count > 0)
            {
                var currentSession = SessionHistory.LastOrDefault();
                if (currentSession != null && currentSession.EndTime == default)
                {
                    // Update current session duration
                    var sessionDuration = (System.DateTime.Now - currentSession.StartTime).TotalHours;
                    TotalPlayTime += (float)sessionDuration;
                }
            }
            
            // Calculate average session length
            if (SessionHistory.Count > 0)
            {
                var completedSessions = SessionHistory.Where(s => s.EndTime != default).ToList();
                if (completedSessions.Count > 0)
                {
                    AverageSessionLength = completedSessions.Average(s => (float)(s.EndTime - s.StartTime).TotalHours);
                }
            }
        }
        
        /// <summary>
        /// Cleanup analytics data - called by ComprehensiveProgressionManager on shutdown
        /// </summary>
        public void Cleanup()
        {
            // End current session if active
            if (SessionHistory.Count > 0)
            {
                var currentSession = SessionHistory.LastOrDefault();
                if (currentSession != null && currentSession.EndTime == default)
                {
                    currentSession.EndTime = System.DateTime.Now;
                }
            }
            
            // Clear temporary data but keep persistent analytics
            // Note: We don't clear the dictionaries as they contain valuable analytics data
            // that should persist between sessions
        }
        
        /// <summary>
        /// Record a progression event for analytics
        /// </summary>
        public void RecordEvent(ProgressionEvent progressionEvent)
        {
            if (progressionEvent == null) return;
            
            // Update relevant analytics based on event type
            switch (progressionEvent.EventType)
            {
                case ProgressionEventType.Experience_Gained:
                    if (!string.IsNullOrEmpty(progressionEvent.SkillId))
                    {
                        if (!SkillUsageStats.ContainsKey(progressionEvent.SkillId))
                            SkillUsageStats[progressionEvent.SkillId] = 0f;
                        SkillUsageStats[progressionEvent.SkillId] += progressionEvent.Amount;
                    }
                    break;
                    
                case ProgressionEventType.Achievement_Unlocked:
                    if (!string.IsNullOrEmpty(progressionEvent.AchievementId))
                    {
                        if (!AchievementAttempts.ContainsKey(progressionEvent.AchievementId))
                            AchievementAttempts[progressionEvent.AchievementId] = 0;
                        AchievementAttempts[progressionEvent.AchievementId]++;
                    }
                    break;
                    
                case ProgressionEventType.Research_Completed:
                    if (!string.IsNullOrEmpty(progressionEvent.ResearchId))
                    {
                        if (!ResearchEfficiency.ContainsKey(progressionEvent.ResearchId))
                            ResearchEfficiency[progressionEvent.ResearchId] = 1f;
                        // Could calculate efficiency based on completion time vs expected time
                    }
                    break;
            }
        }
    }

    // Additional Enums
    public enum PassiveBonusType
    {
        Experience_Multiplier,
        Skill_Efficiency,
        Research_Speed,
        Quality_Bonus,
        Yield_Increase,
        Cost_Reduction,
        Time_Reduction,
        Success_Rate_Bonus
    }

    public enum AchievementRequirementType
    {
        SkillLevel,
        ResearchCompleted,
        PlantsHarvested,
        FacilitiesBuilt,
        TotalProfit,
        PlayTime,
        AchievementUnlock,
        ContentUnlock,
        PlayerLevel,
        ExperienceGained
    }

    public enum ProgressCalculationType
    {
        Linear,
        Logarithmic,
        Exponential,
        Threshold
    }

    public enum AchievementTriggerType
    {
        Immediate,
        Threshold,
        Accumulative,
        Conditional,
        Timed,
        Event_Based,
        Milestone_Based,
        Cultivation,
        Economic,
        Environmental,
        Facility,
        Educational,
        Community,
        Seasonal,
        Challenge,
        Mastery
    }

    public enum AchievementProgressionType
    {
        Linear,
        Exponential,
        Stepped,
        Tiered,
        Milestone_Based
    }

    /// <summary>
    /// Context information for achievement processing
    /// </summary>
    [System.Serializable]
    public class AchievementContext
    {
        public string PlayerId = "";
        public string SessionId = "";
        public string EventType = "";
        public string SourceSystem = "";
        public string TargetObject = "";
        public float EventValue = 0f;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public Dictionary<string, float> EnvironmentalFactors = new Dictionary<string, float>();
        public System.DateTime EventTime = System.DateTime.Now;
        public bool IsMultiplayer = false;
        public List<string> ActiveModifiers = new List<string>();
        
        // Additional context properties for specific achievement types
        public string FacilityId = "";
        public string PlantId = "";
        public string ResearchId = "";
        public string SkillId = "";
        public float QualityScore = 0f;
        public float EfficiencyScore = 0f;
    }

    /// <summary>
    /// Player's achievement profile and progress
    /// </summary>
    [System.Serializable]
    public class PlayerAchievementProfile
    {
        [Header("Player Identity")]
        public string PlayerId = "";
        public string PlayerName = "";
        public int Level = 1;
        public float TotalExperience = 0f;
        public int AchievementPoints = 0;
        public System.DateTime CreationDate = System.DateTime.Now;
        public System.DateTime LastPlayTime = System.DateTime.Now;
        
        [Header("Achievement Progress")]
        public List<string> UnlockedAchievements = new List<string>();
        public List<string> InProgressAchievements = new List<string>();
        public Dictionary<string, float> AchievementProgress = new Dictionary<string, float>();
        public Dictionary<AchievementCategory, int> CategoryProgress = new Dictionary<AchievementCategory, int>();
        public List<string> CompletedMilestones = new List<string>();
        
        [Header("Statistics")]
        public int TotalAchievementsUnlocked = 0;
        public int CurrentStreak = 0;
        public int LongestStreak = 0;
        public float AchievementCompletionRate = 0f;
        public System.DateTime FirstAchievement = System.DateTime.Now;
        public System.DateTime LastAchievement = System.DateTime.Now;
        
        [Header("Preferences")]
        public bool HasPremiumStatus = false;
        public bool ShowAchievementNotifications = true;
        public bool EnableProgressTracking = true;
        public AchievementDifficulty PreferredDifficulty = AchievementDifficulty.Normal;
        
        [Header("Social Features")]
        public int GlobalRecognitions = 0;
        public int CommunityAchievements = 0;
        public int HiddenAchievementsFound = 0;
        public float SocialInfluenceScore = 0f;
        public int MentorshipCount = 0;
        
        // PC-012-11: Social and Community Achievement Tracking
        public List<string> CommunityInteractions = new List<string>();
        public int ConsecutiveCommunityDays = 0;
        public System.DateTime LastCommunityContribution = System.DateTime.MinValue;
        
        // Properties for PlayerShowcase compatibility
        public float CompletionPercentage = 0f;
        public float TotalScore = 0f;
        
        // Additional properties for PlayerRecognitionService compatibility
        public string PlayerID { get => PlayerId; set => PlayerId = value; } // Alias for PlayerId
        public int TotalAchievements { get => TotalAchievementsUnlocked; set => TotalAchievementsUnlocked = value; } // Alias
        public float TotalPoints { get => TotalScore; set => TotalScore = value; } // Alias for TotalScore
        public string CurrentTier = "Novice";
        public AchievementCategory FavoriteCategory = AchievementCategory.Cultivation_Mastery;
        public System.DateTime LastUnlock { get => LastAchievement; set => LastAchievement = value; } // Alias
        public List<string> UnlockHistory = new List<string>();
        public List<string> EarnedBadges = new List<string>();
        public int Prestige = 0;
        public int SocialRecognition = 0;
        public int CommunityContributions = 0;
        public System.DateTime ProfileCreatedDate { get => CreationDate; set => CreationDate = value; } // Alias
        public System.DateTime LastUpdateDate { get => LastPlayTime; set => LastPlayTime = value; } // Alias
        public List<string> MentoredPlayers = new List<string>();
        public int HelpfulContributions = 0;
        
        public bool HasUnlockedAchievement(string achievementId)
        {
            return UnlockedAchievements.Contains(achievementId);
        }
        
        public float GetAchievementProgress(string achievementId)
        {
            return AchievementProgress.GetValueOrDefault(achievementId, 0f);
        }
        
        public void AddAchievementProgress(string achievementId, float progress)
        {
            AchievementProgress[achievementId] = progress;
            if (!InProgressAchievements.Contains(achievementId) && progress > 0f && progress < 1f)
            {
                InProgressAchievements.Add(achievementId);
            }
        }
        
        public void UnlockAchievement(string achievementId)
        {
            if (!UnlockedAchievements.Contains(achievementId))
            {
                UnlockedAchievements.Add(achievementId);
                InProgressAchievements.Remove(achievementId);
                AchievementProgress[achievementId] = 1f;
                TotalAchievementsUnlocked++;
                LastAchievement = System.DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Record of an unlocked achievement
    /// </summary>
    [System.Serializable]
    public class UnlockedAchievement
    {
        public string AchievementId = "";
        public AchievementDefinition Achievement;
        public string PlayerId = "";
        public System.DateTime UnlockTime = System.DateTime.Now;
        public AchievementContext Context = new AchievementContext();
        public float CompletionTime = 0f;
        public float QualityScore = 1f;
        public List<string> Witnesses = new List<string>();
        public bool IsShared = false;
        public string CustomMessage = "";
        public Dictionary<string, object> UnlockData = new Dictionary<string, object>();
        
        // Additional properties for milestone tracking
        public bool WasFirstTime = true;
        public bool WasPersonalBest = false;
        public int AttemptNumber = 1;
        public float DifficultyMultiplier = 1f;
        
        // Point value for achievement showcase
        public float PointValue = 0f;
    }

    // Additional missing enums referenced in the codebase
    public enum AchievementDifficulty
    {
        Trivial,
        Easy,
        Medium,
        Normal,
        Hard,
        Expert,
        Master,
        Grandmaster,
        Legendary
    }

    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public enum MilestoneCalculationType
    {
        AllRequired,
        AnyRequired,
        Percentage,
        Weighted
    }

    public enum ProgressionEventType
    {
        Skill_Level_Up,
        SkillLevelUp,
        Research_Completed,
        ResearchCompleted,
        Achievement_Unlocked,
        AchievementUnlocked,
        Content_Unlocked,
        Experience_Gained,
        Milestone_Reached,
        Plant_Harvested,
        Construction_Completed,
        Sale_Completed
    }

    public enum ProgressionNotificationType
    {
        Skill_Level_Up,
        Achievement_Unlocked,
        Research_Completed,
        Content_Unlocked,
        Milestone_Reached,
        Warning,
        Information
    }

    /// <summary>
    /// Represents an achievement trigger event
    /// </summary>
    [System.Serializable]
    public class AchievementTrigger
    {
        public string TriggerId = "";
        public AchievementTriggerType TriggerType;
        public string SourceSystem = "";
        public string EventType = "";
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public float EventValue = 0f;
        public System.DateTime TriggerTime = System.DateTime.Now;
        public string PlayerId = "";
        public string SessionId = "";
        
        // Additional context for specific trigger types
        public string TargetId = "";
        public string FacilityId = "";
        public string PlantId = "";
        public string ResearchId = "";
        public string SkillId = "";
        public float QualityScore = 0f;
        public float EfficiencyScore = 0f;
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// Represents gameplay state data for achievement detection
    /// </summary>
    [System.Serializable]
    public class GameplayStateData
    {
        public Dictionary<string, object> SystemStates = new Dictionary<string, object>();
        public Dictionary<string, float> SystemMetrics = new Dictionary<string, float>();
        public List<string> ActiveSystems = new List<string>();
        public System.DateTime StateTimestamp = System.DateTime.Now;
        public string SessionId = "";
        public string PlayerId = "";
        
        // Cultivation system state
        public int ActivePlants = 0;
        public float TotalYield = 0f;
        public float AverageQuality = 0f;
        
        // Economic system state
        public float CurrentBalance = 0f;
        public float MonthlyProfit = 0f;
        public int TotalSales = 0;
        
        // Research system state
        public int ActiveResearch = 0;
        public int CompletedResearch = 0;
        public float ResearchEfficiency = 0f;
        
        // Facility system state
        public int TotalFacilities = 0;
        public float FacilityUtilization = 0f;
        public float OperationalEfficiency = 0f;
        
        // Player progression state
        public int PlayerLevel = 1;
        public float TotalExperience = 0f;
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
        public List<string> CompletedAchievements = new List<string>();
    }

    /// <summary>
    /// Definition for hidden achievements
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementDefinition
    {
        public string AchievementId = "";
        public string Name = "";
        public string Description = "";
        public AchievementCategory Category;
        public AchievementRarity Rarity = AchievementRarity.Rare;
        public List<ComplexCondition> Conditions = new List<ComplexCondition>();
        public List<string> PrerequisiteAchievements = new List<string>();
        public int MinimumPlayerLevel = 1;
        public bool RequiresSpecificSequence = false;
        public float TimeWindowHours = 24f;
        public Dictionary<string, object> MetadataRequirements = new Dictionary<string, object>();
    }

    /// <summary>
    /// Complex condition for hidden achievements
    /// </summary>
    [System.Serializable]
    public class ComplexCondition
    {
        public string ConditionId = "";
        public ComplexConditionType ConditionType;
        public string TargetSystem = "";
        public string TargetMetric = "";
        public float RequiredValue = 0f;
        public ComparisonType Comparison = ComparisonType.Greater_Equal;
        public float TimeWindowHours = 1f;
        public bool RequiresSequence = false;
        public List<string> RequiredSequence = new List<string>();
        public Dictionary<string, object> AdditionalParameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// Hint for discovering hidden achievements
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementHint
    {
        public string HintId = "";
        public string AchievementId = "";
        public string HintText = "";
        public HintType Type = HintType.Subtle;
        public float RevealLevel = 0.1f; // How much to reveal (0.0 to 1.0)
        public List<string> TriggerConditions = new List<string>();
        public bool IsUnlocked = false;
        public System.DateTime UnlockTime = System.DateTime.Now;
    }

    // Additional enums for complex conditions and hints
    public enum ComplexConditionType
    {
        Statistical,
        Temporal,
        Behavioral,
        SystemState,
        CrossSystem,
        Sequential,
        Environmental
    }

    public enum HintType
    {
        Subtle,
        Direct,
        Cryptic,
        Progressive,
        Contextual
    }

    /// <summary>
    /// Achievement progress data for individual player achievement tracking
    /// Contains all required properties: AchievementId, PlayerId, CurrentValue, IsCompleted, StartDate, LastUpdated, CompletionDate
    /// </summary>
    [System.Serializable]
    public class AchievementProgress
    {
        public string AchievementId = "";
        public string PlayerId = "";
        public float CurrentValue = 0f;
        public bool IsCompleted = false;
        public System.DateTime StartDate = System.DateTime.Now;
        public System.DateTime LastUpdated = System.DateTime.Now;
        public System.DateTime CompletionDate = System.DateTime.MinValue;
        
        // Constructor for compatibility
        public AchievementProgress() { }
        
        public AchievementProgress(string achievementId, string playerId, float currentValue = 0f)
        {
            AchievementId = achievementId;
            PlayerId = playerId;
            CurrentValue = currentValue;
            StartDate = System.DateTime.Now;
            LastUpdated = System.DateTime.Now;
        }
    }

    /// <summary>
    /// Achievement progress information for tracking and display
    /// </summary>
    [System.Serializable]
    public class AchievementProgressInfo
    {
        public string AchievementId = "";
        public float CurrentProgress = 0f;
        public float RequiredProgress = 1f;
        public float RequiredValue = 1f; // Alias for RequiredProgress
        public bool IsCompleted = false;
        public bool IsComplete { get => IsCompleted; set => IsCompleted = value; } // Writable alias for IsCompleted
        public System.DateTime LastUpdated = System.DateTime.Now;
        public System.DateTime LastUpdate { get => LastUpdated; set => LastUpdated = value; } // Writable alias for LastUpdated
        public string ProgressDescription = "";
        public string DisplayText { get => ProgressDescription; set => ProgressDescription = value; } // Writable alias for ProgressDescription
        public AchievementTierInfo TierInfo;
        public List<string> CompletedSteps = new List<string>();
        public List<string> RemainingSteps = new List<string>();
        public float EstimatedTimeToComplete = 0f;
        public ProgressCalculationType CalculationType = ProgressCalculationType.Linear;
        
        // Writable property for progress percentage with automatic calculation fallback
        private float _progressPercentage = 0f;
        public float ProgressPercentage 
        { 
            get => _progressPercentage > 0f ? _progressPercentage : (RequiredProgress > 0f ? (CurrentProgress / RequiredProgress) * 100f : 0f); 
            set => _progressPercentage = value; 
        }
        
        // Constructor for compatibility
        public AchievementProgressInfo() { }
        
        public AchievementProgressInfo(string achievementId, float currentProgress, float requiredProgress)
        {
            AchievementId = achievementId;
            CurrentProgress = currentProgress;
            RequiredProgress = requiredProgress;
            RequiredValue = requiredProgress;
            LastUpdated = System.DateTime.Now;
        }
    }

    /// <summary>
    /// Achievement tier information for progressive achievements
    /// </summary>
    [System.Serializable]
    public class AchievementTierInfo
    {
        public int CurrentTier = 1;
        public int MaxTiers = 1;
        public float TierProgress = 0f;
        public string TierName = "";
        public string TierDescription = "";
        public List<AchievementReward> TierRewards = new List<AchievementReward>();
        
        // Additional properties for BaseAchievementTrigger compatibility - made writable
        private int _totalTiers = 1;
        public int TotalTiers { get => _totalTiers > 0 ? _totalTiers : MaxTiers; set => _totalTiers = value; }
        
        private AchievementTierRequirements _nextTierRequirements;
        public AchievementTierRequirements NextTierRequirements 
        { 
            get => _nextTierRequirements ?? GetNextTierRequirements(); 
            set => _nextTierRequirements = value; 
        }
        
        /// <summary>
        /// Gets the requirements for the next tier
        /// </summary>
        public AchievementTierRequirements GetNextTierRequirements()
        {
            return new AchievementTierRequirements
            {
                NextTierLevel = CurrentTier + 1,
                TierIndex = CurrentTier,
                TierName = $"Tier {CurrentTier + 1}",
                RequiredProgress = TierProgress + 100f, // Default increment
                RequiredValue = TierProgress + 100f,
                CurrentValue = TierProgress,
                RemainingValue = 100f,
                ProgressPercentage = TierProgress / (TierProgress + 100f) * 100f
            };
        }
    }

    /// <summary>
    /// Milestone context for milestone completion events
    /// </summary>
    [System.Serializable]
    public class MilestoneContext
    {
        public string PlayerId = "";
        public string SessionId = "";
        public string TriggerEvent = "";
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public System.DateTime CompletionTime = System.DateTime.Now;
        public float TimeTaken = 0f;
        public List<string> ContributingAchievements = new List<string>();
        public bool IsFirstCompletion = true;
        public float DifficultyMultiplier = 1f;
    }

    /// <summary>
    /// Configuration for reward calculation systems
    /// </summary>
    [System.Serializable]
    public class RewardCalculationConfig
    {
        public float BaseExperienceReward = 100f;
        public float ExperienceMultiplier = 1f;
        public float DifficultyBonus = 0.1f;
        public float FirstTimeBonus = 0.5f;
        public float StreakBonus = 0.05f;
        public float QualityBonus = 0.2f;
        public float TimeBonus = 0.1f;
        public float SynergyBonus = 0.15f;
        public bool EnableRandomBonuses = true;
        public float RandomBonusRange = 0.1f;
        public Dictionary<AchievementCategory, float> CategoryMultipliers = new Dictionary<AchievementCategory, float>();
        public Dictionary<AchievementRarity, float> RarityMultipliers = new Dictionary<AchievementRarity, float>();
        
        // Additional properties for BaseRewardCalculator compatibility
        public float MilestoneRewardMultiplier = 2f; // Milestone rewards are typically higher
        public float LevelMultiplierIncrement = 0.05f; // 5% increase per level
        public float StreakMultiplierIncrement = 0.02f; // 2% increase per streak achievement
        public float PremiumStatusMultiplier = 1.5f; // 50% bonus for premium players
        
        // Maximum reward limits
        public int MaxExperienceReward = 10000;
        public int MaxAchievementPointsReward = 1000;
        public int MaxCurrencyReward = 50000;
        public int MaxItemReward = 100;
    }

    /// <summary>
    /// Achievement tier for progressive achievements
    /// </summary>
    [System.Serializable]
    public class AchievementTier
    {
        public int TierLevel = 1;
        public string TierName = "";
        public string TierDescription = "";
        public float RequiredProgress = 100f;
        public float RequiredValue => RequiredProgress; // Alias for RequiredProgress for compatibility
        public List<AchievementReward> TierRewards = new List<AchievementReward>();
        public string TierIcon = ""; // String icon for PlayerRecognitionService compatibility  
        public Sprite TierIconSprite; // Sprite version for UI compatibility
        public Color TierColor = Color.white;
        public bool IsUnlocked = false;
        public System.DateTime UnlockTime = System.DateTime.Now;
        public AchievementRarity TierRarity = AchievementRarity.Common;
        
        // Additional properties for PlayerRecognitionService compatibility
        public string TierID = "";
        public string Description { get => TierDescription; set => TierDescription = value; } // Alias
        public float RequiredPoints { get => RequiredProgress; set => RequiredProgress = value; } // Alias
        public List<string> Benefits = new List<string>();
        public int Prestige = 0;
        public int Level { get => TierLevel; set => TierLevel = value; } // Alias
        public System.DateTime UnlockDate { get => UnlockTime; set => UnlockTime = value; } // Alias
    }

    /// <summary>
    /// Requirements for unlocking the next achievement tier
    /// </summary>
    [System.Serializable]
    public class AchievementTierRequirements
    {
        public int NextTierLevel = 2;
        public int TierIndex = 1; // Index of the tier being referenced
        public string TierName = ""; // Name of the next tier
        public float RequiredProgress = 200f;
        public float RequiredValue = 200f; // Alias for RequiredProgress
        public float CurrentValue = 0f; // Current progress value
        public float RemainingValue = 200f; // Remaining progress needed
        public float ProgressPercentage = 0f; // Progress percentage towards next tier
        public List<string> RequiredAchievements = new List<string>();
        public int MinimumPlayerLevel = 1;
        public float EstimatedTimeToUnlock = 0f;
        public string RequirementDescription = "";
        public List<AchievementRequirement> AdditionalRequirements = new List<AchievementRequirement>();
    }

    /// <summary>
    /// ScriptableObject for milestone definitions - placeholder for SO references
    /// Note: The actual SO should be created separately, this is for type compatibility
    /// </summary>
    public abstract class MilestoneSO : ScriptableObject
    {
        public string MilestoneId = "";
        public string MilestoneName = "";
        public string Description = "";
        public List<string> RequiredAchievements = new List<string>();
        public List<string> UnlockedAchievements = new List<string>();
        public float RequiredProgress = 100f;
        public AchievementCategory Category;
        public bool IsHidden = false;
        
        // Rewards property for BaseRewardCalculator compatibility
        public List<AchievementReward> Rewards = new List<AchievementReward>();
    }

    // Missing Event Data Types for Achievement System

    /// <summary>
    /// Social recognition data for community achievements
    /// </summary>
    [System.Serializable]
    public class SocialRecognitionData
    {
        public string PlayerId = "";
        public string RecognitionType = "";
        public float RecognitionValue = 0f;
        public string SourceCommunity = "";
        public System.DateTime RecognitionTime = System.DateTime.Now;
        public List<string> Witnesses = new List<string>();
        public string RecognitionMessage = "";
        public bool IsPublic = true;
    }

    /// <summary>
    /// Leaderboard position data for competitive achievements
    /// </summary>
    [System.Serializable]
    public class LeaderboardPosition
    {
        public string PlayerId = "";
        public string LeaderboardType = "";
        public int CurrentPosition = 0;
        public int PreviousPosition = 0;
        public float Score = 0f;
        public System.DateTime LastUpdate = System.DateTime.Now;
        public string Category = "";
        public bool IsTopPercentile = false;
    }

    /// <summary>
    /// Achievement prediction data for analytics
    /// </summary>
    [System.Serializable]
    public class AchievementPrediction
    {
        public string AchievementId = "";
        public string PlayerId = "";
        public float CompletionProbability = 0f;
        public float EstimatedTimeToComplete = 0f;
        public List<string> RecommendedActions = new List<string>();
        public System.DateTime PredictionTime = System.DateTime.Now;
        public string PredictionMethod = "";
        public float ConfidenceLevel = 0f;
    }

    /// <summary>
    /// Achievement analytics report data
    /// </summary>
    [System.Serializable]
    public class AchievementAnalyticsReport
    {
        public string ReportId = "";
        public string PlayerId = "";
        public System.DateTime ReportPeriodStart = System.DateTime.Now;
        public System.DateTime ReportPeriodEnd = System.DateTime.Now;
        public int AchievementsCompleted = 0;
        public float TotalExperienceGained = 0f;
        public List<string> TopCategories = new List<string>();
        public float CompletionRate = 0f;
        public float AverageTimePerAchievement = 0f;
        public Dictionary<string, int> CategoryBreakdown = new Dictionary<string, int>();
    }

    /// <summary>
    /// Plant lifecycle event data for cultivation achievements
    /// </summary>
    [System.Serializable]
    public class PlantLifecycleEventData
    {
        public string PlantId = "";
        public string EventType = "";
        public string GrowthStage = "";
        public float PlantAge = 0f;
        public float HealthScore = 1f;
        public float QualityScore = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string FacilityId = "";
        public Dictionary<string, float> EnvironmentalConditions = new Dictionary<string, float>();
    }

    /// <summary>
    /// Irrigation event data for facility management achievements
    /// </summary>
    [System.Serializable]
    public class IrrigationEventData
    {
        public string SystemId = "";
        public string EventType = "";
        public float WaterAmount = 0f;
        public float NutrientConcentration = 0f;
        public float PHLevel = 7f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string FacilityZone = "";
        public bool IsAutomated = true;
        public float SystemEfficiency = 1f;
    }

    /// <summary>
    /// Environmental event data for cultivation achievements
    /// </summary>
    [System.Serializable]
    public class EnvironmentalEventData
    {
        public string FacilityId = "";
        public string EventType = "";
        public float Temperature = 22f;
        public float Humidity = 60f;
        public float CO2Level = 400f;
        public float LightIntensity = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string Zone = "";
        public bool IsWithinOptimalRange = true;
    }

    /// <summary>
    /// PM (Preventive Maintenance) event data for facility achievements
    /// </summary>
    [System.Serializable]
    public class PMEventData
    {
        public string EquipmentId = "";
        public string MaintenanceType = "";
        public System.DateTime ScheduledTime = System.DateTime.Now;
        public System.DateTime CompletedTime = System.DateTime.Now;
        public string TechnicianId = "";
        public bool WasSuccessful = true;
        public float DowntimeHours = 0f;
        public string Notes = "";
        public float CostIncurred = 0f;
    }

    /// <summary>
    /// IPM (Integrated Pest Management) event data - alias for PMEventData for compatibility
    /// </summary>
    [System.Serializable]
    public class IPMEventData : PMEventData
    {
        public string PestType = "";
        public string TreatmentMethod = "";
        public float TreatmentEffectiveness = 1f;
        public bool IsOrganicTreatment = true;
        public string AffectedArea = "";
    }

    /// <summary>
    /// Harvest event data for cultivation achievements
    /// </summary>
    [System.Serializable]
    public class HarvestEventData
    {
        public string PlantId = "";
        public string BatchId = "";
        public float YieldAmount = 0f;
        public float QualityGrade = 0f;
        public System.DateTime HarvestTime = System.DateTime.Now;
        public string HarvesterPlayerId = "";
        public string FacilityId = "";
        public float DryWeight = 0f;
        public float WetWeight = 0f;
        public Dictionary<string, float> QualityMetrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// Construction event data for facility achievements
    /// </summary>
    [System.Serializable]
    public class ConstructionEventData
    {
        public string ProjectId = "";
        public string ConstructionType = "";
        public string FacilityId = "";
        public float ProgressPercentage = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public float CostIncurred = 0f;
        public string ContractorId = "";
        public bool IsCompleted = false;
        public float QualityScore = 1f;
    }

    /// <summary>
    /// Equipment event data for facility achievements
    /// </summary>
    [System.Serializable]
    public class EquipmentEventData
    {
        public string EquipmentId = "";
        public string EventType = "";
        public string OperationalStatus = "";
        public float EfficiencyRating = 1f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string OperatorId = "";
        public float PowerConsumption = 0f;
        public bool RequiresMaintenance = false;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// UI event data for interface achievements
    /// </summary>
    [System.Serializable]
    public class UIEventData
    {
        public string EventType = "";
        public string UIElement = "";
        public string ScreenName = "";
        public System.DateTime EventTime = System.DateTime.Now;
        public string PlayerId = "";
        public float InteractionDuration = 0f;
        public bool WasSuccessful = true;
        public Dictionary<string, object> EventParameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// Teaching state data for education achievements
    /// </summary>
    [System.Serializable]
    public class TeachingStateData
    {
        public string TeacherId = "";
        public string StudentId = "";
        public string LessonType = "";
        public float ProgressPercentage = 0f;
        public System.DateTime SessionTime = System.DateTime.Now;
        public float EffectivenessScore = 0f;
        public string TopicCovered = "";
        public bool IsCompleted = false;
        public float DurationMinutes = 0f;
    }

    /// <summary>
    /// Financial event data for business achievements
    /// </summary>
    [System.Serializable]
    public class FinancialEventData
    {
        public string TransactionId = "";
        public string TransactionType = "";
        public float Amount = 0f;
        public string Currency = "USD";
        public System.DateTime TransactionTime = System.DateTime.Now;
        public string PlayerId = "";
        public string Description = "";
        public string Category = "";
        public float RunningBalance = 0f;
    }

    /// <summary>
    /// Research event data for research achievements
    /// </summary>
    [System.Serializable]
    public class ResearchEventData
    {
        public string ResearchId = "";
        public string EventType = "";
        public float ProgressPercentage = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string ResearcherId = "";
        public string ResearchPhase = "";
        public float BreakthroughProbability = 0f;
        public bool IsCompleted = false;
        public Dictionary<string, float> ResearchMetrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// Economic state data for market achievements
    /// </summary>
    [System.Serializable]
    public class EconomicStateData
    {
        public string MarketId = "";
        public float MarketPrice = 0f;
        public float Demand = 1f;
        public float Supply = 1f;
        public System.DateTime StateTime = System.DateTime.Now;
        public string CommodityType = "";
        public float Volatility = 0f;
        public float TrendDirection = 0f;
        public Dictionary<string, float> MarketFactors = new Dictionary<string, float>();
    }

    /// <summary>
    /// Community contribution data for social achievements
    /// </summary>
    [System.Serializable]
    public class CommunityContributionData
    {
        public string PlayerId = "";
        public string ContributionType = "";
        public float ContributionValue = 0f;
        public System.DateTime ContributionTime = System.DateTime.Now;
        public string CommunityId = "";
        public string Description = "";
        public bool IsRecognized = false;
        public float ImpactScore = 0f;
    }

    /// <summary>
    /// Community state data for social achievements
    /// </summary>
    [System.Serializable]
    public class CommunityStateData
    {
        public string CommunityId = "";
        public int MemberCount = 0;
        public float ActivityLevel = 0f;
        public System.DateTime StateTime = System.DateTime.Now;
        public float CommunityHealth = 1f;
        public Dictionary<string, float> CommunityMetrics = new Dictionary<string, float>();
        public List<string> ActiveMembers = new List<string>();
        public string CommunityType = "";
    }

    /// <summary>
    /// Community rank data for competitive social achievements
    /// </summary>
    [System.Serializable]
    public class CommunityRankData
    {
        public string PlayerId = "";
        public string CommunityId = "";
        public int CurrentRank = 0;
        public int PreviousRank = 0;
        public float RankScore = 0f;
        public System.DateTime LastRankUpdate = System.DateTime.Now;
        public string RankCategory = "";
        public bool IsTopTier = false;
        public float PercentilePosition = 0f;
    }

    /// <summary>
    /// Utility event data for infrastructure achievements
    /// </summary>
    [System.Serializable]
    public class UtilityEventData
    {
        public string UtilityType = ""; // Electrical, Plumbing, HVAC, etc.
        public string SystemId = "";
        public string EventType = "";
        public float CapacityUtilization = 0f;
        public float EfficiencyRating = 1f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string FacilityZone = "";
        public bool IsOperational = true;
        public float PowerConsumption = 0f;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// Facility state data for comprehensive facility achievements
    /// </summary>
    [System.Serializable]
    public class FacilityStateData
    {
        public string FacilityId = "";
        public string FacilityType = "";
        public float OverallEfficiency = 1f;
        public float CapacityUtilization = 0f;
        public System.DateTime StateTime = System.DateTime.Now;
        public int TotalRooms = 0;
        public int ActiveRooms = 0;
        public float TotalArea = 0f;
        public float UsableArea = 0f;
        public Dictionary<string, float> ZoneEfficiencies = new Dictionary<string, float>();
        public Dictionary<string, int> EquipmentCounts = new Dictionary<string, int>();
        public bool IsCompliant = true;
        public float ComplianceScore = 1f;
    }

    /// <summary>
    /// Resource event data for resource management achievements
    /// </summary>
    [System.Serializable]
    public class ResourceEventData
    {
        public string ResourceType = ""; // Water, Nutrients, Energy, etc.
        public string EventType = "";
        public float ResourceAmount = 0f;
        public float CostPerUnit = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string FacilityId = "";
        public string UsageCategory = "";
        public float EfficiencyRating = 1f;
        public bool IsWaste = false;
        public float WasteAmount = 0f;
        public Dictionary<string, float> ResourceMetrics = new Dictionary<string, float>();
    }

    /// <summary>
    /// Market event data for economic and trading achievements
    /// </summary>
    [System.Serializable]
    public class MarketEventData
    {
        public string MarketId = "";
        public string EventType = "";
        public string CommodityType = "";
        public float Price = 0f;
        public float Volume = 0f;
        public System.DateTime EventTime = System.DateTime.Now;
        public string PlayerId = "";
        public string TransactionId = "";
        public bool IsBuyOrder = true;
        public float ProfitMargin = 0f;
        public string MarketCondition = "";
        public Dictionary<string, float> MarketFactors = new Dictionary<string, float>();
    }

    // Note: AchievementSO class is defined in separate AchievementSO.cs file

    #region Hidden Achievement System
    
    /// <summary>
    /// Data structure for hidden achievement discovery events
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementDiscovery
    {
        public string AchievementId;
        public string DiscoveryMethod;
        public System.DateTime DiscoveryTime;
        public string PlayerId;
        public Dictionary<string, object> DiscoveryContext = new Dictionary<string, object>();
        public float ConfidenceScore;
        public bool IsValidated;
        
        public HiddenAchievementDiscovery()
        {
            DiscoveryTime = System.DateTime.Now;
            ConfidenceScore = 1.0f;
            IsValidated = false;
        }
    }
    
    /// <summary>
    /// Pattern matching data for behavior analysis
    /// </summary>
    [System.Serializable]
    public class PatternMatchData
    {
        public string PatternId;
        public string PatternType;
        public float MatchStrength;
        public List<string> MatchedActions = new List<string>();
        public System.DateTime FirstOccurrence;
        public System.DateTime LastOccurrence;
        public int Frequency;
        public Dictionary<string, float> PatternParameters = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Results from behavior analysis system
    /// </summary>
    [System.Serializable]
    public class BehaviorAnalysisResult
    {
        public string PlayerId;
        public string BehaviorType;
        public float AnalysisScore;
        public List<PatternMatchData> DetectedPatterns = new List<PatternMatchData>();
        public Dictionary<string, float> BehaviorMetrics = new Dictionary<string, float>();
        public System.DateTime AnalysisTime;
        public string AnalysisMethod;
    }
    
    #endregion
    
    #region Reward System Data Types
    
    /// <summary>
    /// Data for tracking reward streaks
    /// </summary>
    [System.Serializable]
    public class RewardStreakData
    {
        public string StreakId;
        public string PlayerId;
        public string StreakType;
        public int CurrentStreakLength;
        public int MaxStreakLength;
        public System.DateTime StreakStartTime;
        public System.DateTime LastRewardTime;
        public float StreakMultiplier;
        public List<string> RewardHistory = new List<string>();
        public bool IsActive;
    }
    
    /// <summary>
    /// Behavior-based bonus calculations
    /// </summary>
    [System.Serializable]
    public class BehaviorBonus
    {
        public string BonusType;
        public float BonusMultiplier;
        public string BehaviorTrigger;
        public Dictionary<string, float> BonusParameters = new Dictionary<string, float>();
        public System.DateTime ExpirationTime;
        public bool IsStackable;
        public int MaxStacks;
    }
    
    /// <summary>
    /// Economic reward metrics and tracking
    /// </summary>
    [System.Serializable]
    public class EconomicRewardMetrics
    {
        public float TotalRewardsDistributed;
        public float AverageRewardValue;
        public Dictionary<string, float> RewardsByType = new Dictionary<string, float>();
        public System.DateTime LastUpdate;
        public float InflationRate;
        public float PlayerEconomicImpact;
        public Dictionary<string, int> RewardFrequency = new Dictionary<string, int>();
    }
    
    #endregion
    
    #region Social Recognition Data Types
    
    /// <summary>
    /// Peer endorsement data for social recognition
    /// </summary>
    [System.Serializable]
    public class PeerEndorsement
    {
        public string EndorsementId;
        public string EndorserPlayerId;
        public string EndorsedPlayerId;
        public string EndorsementType;
        public string EndorsementReason;
        public float EndorsementWeight;
        public System.DateTime EndorsementTime;
        public bool IsValidated;
        public Dictionary<string, object> EndorsementData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Mentorship relationship tracking
    /// </summary>
    [System.Serializable]
    public class MentorshipRelationship
    {
        public string RelationshipId;
        public string MentorPlayerId;
        public string MenteePlayerId;
        public System.DateTime RelationshipStartTime;
        public System.DateTime LastInteraction;
        public string MentorshipType;
        public float MentorshipEffectiveness;
        public List<string> SharedAchievements = new List<string>();
        public bool IsActive;
        public Dictionary<string, float> ProgressMetrics = new Dictionary<string, float>();
    }
    
    #endregion
    
    #region Achievement System Data Types
    
    /// <summary>
    /// Achievement update data for system manager
    /// </summary>
    [System.Serializable]
    public class AchievementUpdateData
    {
        public string AchievementId;
        public string PlayerId;
        public float ProgressDelta;
        public string UpdateSource;
        public System.DateTime UpdateTime;
        public Dictionary<string, object> UpdateContext = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Cultivation achievement data
    /// </summary>
    [System.Serializable]
    public class CultivationAchievementData
    {
        public string PlantStrainId;
        public int PlantsGrown;
        public float TotalYield;
        public float AverageQuality;
        public List<string> TechniquesUsed = new List<string>();
        public Dictionary<string, float> CultivationMetrics = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Economic achievement data
    /// </summary>
    [System.Serializable]
    public class EconomicAchievementData
    {
        public float TotalRevenue;
        public float ProfitMargin;
        public int SuccessfulTrades;
        public Dictionary<string, float> MarketMetrics = new Dictionary<string, float>();
        public List<string> EconomicMilestones = new List<string>();
    }
    
    /// <summary>
    /// Educational achievement data for tracking learning progress
    /// </summary>
    [System.Serializable]
    public class EducationalAchievementData
    {
        public string PlayerId;
        public int CoursesCompleted;
        public float AverageTestScore;
        public List<string> CertificationsEarned = new List<string>();
        public Dictionary<string, float> LearningMetrics = new Dictionary<string, float>();
        public System.DateTime LastEducationalActivity;
    }
    
    /// <summary>
    /// Progressive achievement data
    /// </summary>
    [System.Serializable]
    public class ProgressiveAchievementData
    {
        public int CurrentTier;
        public float TierProgress;
        public List<string> CompletedTiers = new List<string>();
        public Dictionary<string, float> ProgressionMetrics = new Dictionary<string, float>();
        public System.DateTime NextTierUnlockTime;
    }
    
    /// <summary>
    /// Community achievement data
    /// </summary>
    [System.Serializable]
    public class CommunityAchievementData
    {
        public int CommunityContributions;
        public float CommunityImpactScore;
        public List<string> CommunityRoles = new List<string>();
        public Dictionary<string, float> SocialMetrics = new Dictionary<string, float>();
        public System.DateTime LastCommunityActivity;
    }
    
    /// <summary>
    /// Seasonal achievement data
    /// </summary>
    [System.Serializable]
    public class SeasonalAchievementData
    {
        public string CurrentSeason;
        public int SeasonalEventsParticipated;
        public float SeasonalScore;
        public List<string> SeasonalMilestones = new List<string>();
        public Dictionary<string, float> SeasonalMetrics = new Dictionary<string, float>();
    }
    
    #endregion

    #region Analytics Data Types
    
    [Serializable]
    public class PlayerAnalyticsProfile
    {
        public string PlayerId;
        public Dictionary<string, object> AnalyticsData = new Dictionary<string, object>();
        public List<BehaviorPattern> BehaviorPatterns = new List<BehaviorPattern>();
        public PlayerSegmentationReport SegmentationData;
        public PersonalizationProfile PersonalizationSettings;
        public System.DateTime LastAnalyticsUpdate = System.DateTime.Now;
    }

    [Serializable]
    public class ABTestResult
    {
        public string TestId;
        public string VariantId;
        public string TestName;
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
        public bool IsWinner;
        public float ConfidenceLevel = 0.95f;
        public System.DateTime TestStartTime;
        public System.DateTime TestEndTime;
    }

    [Serializable]
    public class AnalyticsReport
    {
        public string ReportId;
        public string ReportName;
        public ReportType Type;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public System.DateTime GeneratedAt = System.DateTime.Now;
        public string GeneratedBy;
    }

    [Serializable]
    public class ABTest
    {
        public string TestId;
        public string TestName;
        public string Description;
        public List<string> Variants = new List<string>();
        public ABTestStatus Status = ABTestStatus.Draft;
        public float TrafficAllocation = 0.5f;
        public Dictionary<string, object> Configuration = new Dictionary<string, object>();
        public System.DateTime StartDate;
        public System.DateTime EndDate;
    }

    [Serializable]
    public class PersonalizationProfile
    {
        public string ProfileId;
        public string PlayerId;
        public Dictionary<string, float> Preferences = new Dictionary<string, float>();
        public List<string> RecommendedContent = new List<string>();
        public PersonalizationStrategy Strategy = PersonalizationStrategy.Collaborative;
        public System.DateTime LastUpdated = System.DateTime.Now;
    }

    [Serializable]
    public class PlayerSegmentationReport
    {
        public string PlayerId;
        public PlayerSegment PrimarySegment;
        public List<PlayerSegment> SecondarySegments = new List<PlayerSegment>();
        public float SegmentConfidence = 0.8f;
        public Dictionary<string, float> SegmentScores = new Dictionary<string, float>();
        public System.DateTime LastSegmentation = System.DateTime.Now;
    }

    [Serializable]
    public class BehaviorPattern
    {
        public string PatternId;
        public string PatternName;
        public BehaviorType Type;
        public float Frequency;
        public float Intensity;
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();
        public System.DateTime FirstDetected;
        public System.DateTime LastOccurrence;
    }

    [Serializable]
    public class AchievementAnalyticsData
    {
        public string AchievementId;
        public int CompletionCount;
        public float AverageCompletionTime;
        public float DifficultyRating;
        public Dictionary<string, float> PlayerSegmentPerformance = new Dictionary<string, float>();
        public List<string> CommonFailurePoints = new List<string>();
        public System.DateTime LastAnalyzed = System.DateTime.Now;
    }

    [Serializable]
    public class EngagementTracker
    {
        public string PlayerId;
        public float EngagementScore = 0.5f;
        public Dictionary<string, float> ActivityMetrics = new Dictionary<string, float>();
        public List<EngagementEvent> RecentEvents = new List<EngagementEvent>();
        public EngagementTrend Trend = EngagementTrend.Stable;
        public System.DateTime LastActivity = System.DateTime.Now;
    }

    [Serializable]
    public class EngagementEvent
    {
        public string EventId;
        public string EventType;
        public float EngagementValue;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public System.DateTime Timestamp = System.DateTime.Now;
    }

    [Serializable]
    public class AnalyticsEvent
    {
        public string EventId;
        public string EventName;
        public string Category;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public System.DateTime Timestamp = System.DateTime.Now;
        public string PlayerId;
        public string SessionId;
    }

    // Enums for Analytics
    public enum ReportType
    {
        Achievement,
        Engagement,
        Retention,
        Monetization,
        Performance,
        Segmentation,
        ABTest
    }

    public enum ABTestStatus
    {
        Draft,
        Running,
        Paused,
        Completed,
        Cancelled
    }

    public enum PersonalizationStrategy
    {
        ContentBased,
        Collaborative,
        Hybrid,
        RuleBased,
        MachineLearning
    }

    public enum PlayerSegment
    {
        Casual,
        Hardcore,
        Competitive,
        Social,
        Explorer,
        Achiever,
        Spender,
        Retainer
    }

    public enum BehaviorType
    {
        PlaySession,
        Achievement,
        Social,
        Economic,
        Progression,
        Engagement,
        Retention
    }

    public enum EngagementTrend
    {
        Increasing,
        Decreasing,
        Stable,
        Volatile,
        Seasonal
    }

    #endregion

    /// <summary>
    /// Represents a player's achievement showcase display
    /// </summary>
    [System.Serializable]
    public class PlayerShowcase
    {
        [Header("Showcase Identity")]
        public string PlayerId;
        public string PlayerName;
        public System.DateTime LastUpdated;
        
        [Header("Featured Achievements")]
        public List<string> FeaturedAchievementIds = new List<string>();
        public List<UnlockedAchievement> RecentAchievements = new List<UnlockedAchievement>();
        public UnlockedAchievement HighlightedAchievement;
        
        [Header("Showcase Settings")]
        public bool IsPublic = true;
        public bool ShowRecentActivity = true;
        public bool ShowProgress = true;
        public int MaxDisplayedAchievements = 10;
        
        [Header("Social Stats")]
        public int TotalAchievements;
        public float CompletionPercentage;
        public List<AchievementCategory> Specializations = new List<AchievementCategory>();
        public float OverallScore;
        
        [Header("Customization")]
        public string ShowcaseTheme = "Default";
        public List<string> CustomTags = new List<string>();
        public string PersonalMessage;
        
        /// <summary>
        /// Add an achievement to the showcase
        /// </summary>
        public void AddAchievement(UnlockedAchievement achievement)
        {
            if (achievement == null) return;
            
            // Add to recent achievements (keep only latest)
            RecentAchievements.Insert(0, achievement);
            if (RecentAchievements.Count > 5)
            {
                RecentAchievements.RemoveAt(RecentAchievements.Count - 1);
            }
            
            // Update stats
            TotalAchievements++;
            LastUpdated = System.DateTime.Now;
            
            // Check if this should be the highlighted achievement
            if (HighlightedAchievement == null || 
                achievement.PointValue > HighlightedAchievement.PointValue)
            {
                HighlightedAchievement = achievement;
            }
        }
        
        /// <summary>
        /// Update showcase statistics
        /// </summary>
        public void UpdateStats(PlayerAchievementProfile profile)
        {
            if (profile == null) return;
            
            TotalAchievements = profile.UnlockedAchievements?.Count ?? 0;
            CompletionPercentage = profile.CompletionPercentage;
            OverallScore = profile.TotalScore;
            LastUpdated = System.DateTime.Now;
        }
        
        /// <summary>
        /// Get display-ready achievement list
        /// </summary>
        public List<UnlockedAchievement> GetDisplayAchievements()
        {
            var displayList = new List<UnlockedAchievement>();
            
            // Add highlighted achievement first
            if (HighlightedAchievement != null)
            {
                displayList.Add(HighlightedAchievement);
            }
            
            // Add recent achievements (avoiding duplicates)
            foreach (var achievement in RecentAchievements)
            {
                if (!displayList.Contains(achievement) && displayList.Count < MaxDisplayedAchievements)
                {
                    displayList.Add(achievement);
                }
            }
            
            return displayList;
        }
    }

    /// <summary>
    /// Data structure for achievement-related events
    /// </summary>
    [System.Serializable]
    public class AchievementEventData
    {
        public string EventId;
        public string PlayerId;
        public string AchievementId;
        public System.DateTime Timestamp;
        public float ProgressValue;
        public bool IsCompleted;
        public Dictionary<string, object> EventMetadata = new Dictionary<string, object>();
        public string EventType;
        public float ImpactScore;
    }

    /// <summary>
    /// Data structure for milestone-related events
    /// </summary>
    [System.Serializable]
    public class MilestoneEventData
    {
        public string EventId;
        public string PlayerId;
        public string MilestoneId;
        public System.DateTime Timestamp;
        public float CompletionPercentage;
        public bool IsAchieved;
        public Dictionary<string, object> MilestoneMetadata = new Dictionary<string, object>();
        public string MilestoneType;
        public List<string> Prerequisites = new List<string>();
        public float RewardValue;
    }

    /// <summary>
    /// Represents a dynamically generated milestone
    /// </summary>
    [System.Serializable]
    public class DynamicMilestone
    {
        public string MilestoneId;
        public string MilestoneName;
        public string Description;
        public AchievementCategory Category;
        public List<MilestoneRequirement> Requirements = new List<MilestoneRequirement>();
        public List<MilestoneReward> Rewards = new List<MilestoneReward>();
        public float DifficultyMultiplier = 1.0f;
        public System.DateTime GeneratedTime;
        public System.DateTime ExpirationTime;
        public bool IsActive = true;
        public string GenerationReason;
        public float CompletionBonus = 1.2f;
    }

    /// <summary>
    /// Represents a progression pathway
    /// </summary>
    [System.Serializable]
    public class ProgressionPathway
    {
        public string PathwayId;
        public string PathwayName;
        public string Description;
        public List<string> RequiredMilestones = new List<string>();
        public List<string> UnlockedMilestones = new List<string>();
        public List<MilestoneReward> PathwayRewards = new List<MilestoneReward>();
        public int RequiredPlayerLevel = 1;
        public bool IsUnlocked = false;
        public System.DateTime UnlockTime;
        public float ProgressionBonus = 1.1f;
        public string PathwayType;
    }

    /// <summary>
    /// Represents mastery in a specific category
    /// </summary>
    [System.Serializable]
    public class CategoryMastery
    {
        public AchievementCategory Category;
        public MasteryLevel MasteryLevel;
        public int ProgressionPoints;
        public System.DateTime LastUpdate;
        public List<string> MasteredAchievements = new List<string>();
        public float MasteryPercentage;
        public List<string> UnlockedBenefits = new List<string>();
        public int RequiredPointsForNextLevel;
    }

    /// <summary>
    /// Configuration for milestone system
    /// </summary>
    [System.Serializable]
    public class MilestoneConfigSO : ScriptableObject
    {
        [Header("Milestone Settings")]
        public int MaxMilestonesPerUpdate = 5;
        public int MaxCachedMilestones = 100;
        public int MaxActiveSynergies = 10;
        public float SynergyDecayRate = 0.95f;
        public int MaxDynamicMilestones = 15;
        public float GenerationThreshold = 0.75f;
        public float AdaptationRate = 0.1f;
        
        [Header("Performance Settings")]
        public bool EnablePerformanceOptimization = true;
        public int BatchProcessingSize = 10;
        public float UpdateFrequency = 1.0f;
        
        [Header("AI Generation")]
        public bool EnableAIGeneration = false;
        public float AIGenerationThreshold = 0.8f;
        public int MaxAIGeneratedMilestones = 5;
    }

    /// <summary>
    /// System metrics for milestone progression
    /// </summary>
    [System.Serializable]
    public class MilestoneSystemMetrics
    {
        public int TotalActiveMilestones;
        public int TotalCompletedMilestones;
        public int TotalGeneratedMilestones;
        public float AverageCompletionTime;
        public float SystemPerformance = 1.0f;
        public System.DateTime LastUpdate;
        public Dictionary<AchievementCategory, int> CategoryBreakdown = new Dictionary<AchievementCategory, int>();
        public float PlayerEngagementScore = 0.5f;
        public int ActiveSynergies;
    }

    /// <summary>
    /// Mastery level enumeration
    /// </summary>
    public enum MasteryLevel
    {
        Novice,
        Apprentice,
        Skilled,
        Expert,
        Master,
        Grandmaster,
        Legendary
    }

    /// <summary>
    /// Progress tracking for pathways
    /// </summary>
    [System.Serializable]
    public class PathwayProgress
    {
        public string PathwayId;
        public ProgressionPathway Pathway;
        public List<string> CompletedMilestones = new List<string>();
        public float CompletionPercentage;
        public System.DateTime StartTime;
        public System.DateTime LastUpdate;
        public bool IsCompleted = false;
        public System.DateTime CompletionTime;
        public List<string> UnlockedRewards = new List<string>();
        public int CurrentStep;
        public int TotalSteps;
    }

    // Missing progression system types
    [System.Serializable]
    public class ProgressionContext
    {
        public string PlayerId;
        public Dictionary<string, object> CurrentStats = new Dictionary<string, object>();
        public List<string> ActiveMilestones = new List<string>();
        public List<string> CompletedAchievements = new List<string>();
        public float SessionTime;
        public System.DateTime SessionStart;
        public Dictionary<string, float> SkillLevels = new Dictionary<string, float>();
        public Dictionary<string, object> GameState = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class MilestoneTracker
    {
        public Dictionary<string, MilestoneProgress> ActiveMilestones = new Dictionary<string, MilestoneProgress>();
        public List<string> CompletedMilestones = new List<string>();
        public Dictionary<string, float> ProgressValues = new Dictionary<string, float>();
        public System.DateTime LastUpdate;
    }

    [System.Serializable]
    public class MilestoneProgress
    {
        public string MilestoneId;
        public float CurrentProgress;
        public float TargetProgress;
        public System.DateTime StartTime;
        public bool IsCompleted;
    }

    [System.Serializable]
    public class CrossCategorySynergyEngine
    {
        public Dictionary<string, float> SynergyMultipliers = new Dictionary<string, float>();
        public List<string> ActiveSynergies = new List<string>();
        public float SynergyDecayRate = 0.95f;
    }

    [System.Serializable]
    public class DynamicMilestoneGenerator
    {
        public bool EnableGeneration = true;
        public float GenerationThreshold = 0.7f;
        public int MaxDynamicMilestones = 10;
        public List<string> GeneratedMilestones = new List<string>();
    }

    [System.Serializable]
    public class AdaptivePathwayManager
    {
        public Dictionary<string, ProgressionPathway> ActivePathways = new Dictionary<string, ProgressionPathway>();
        public float AdaptationRate = 0.1f;
        public List<string> RecommendedPathways = new List<string>();
    }

    [System.Serializable]
    public class MasteryProgressionTracker
    {
        public Dictionary<string, MasteryLevel> MasteryLevels = new Dictionary<string, MasteryLevel>();
        public Dictionary<string, float> MasteryProgress = new Dictionary<string, float>();
        public List<string> MasteredSkills = new List<string>();
    }

    [System.Serializable]
    public class LegacyAchievementCoordinator
    {
        public List<string> LegacyGoals = new List<string>();
        public Dictionary<string, float> LegacyProgress = new Dictionary<string, float>();
        public bool EnableLegacyTracking = true;
    }

    [System.Serializable]
    public class ProgressionEventData
    {
        public string EventId;
        public string EventType;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public System.DateTime Timestamp;
        public string PlayerId;
    }

    [System.Serializable]
    public class RewardEventData
    {
        public string RewardId;
        public string RewardType;
        public object RewardValue;
        public string Source;
        public System.DateTime Timestamp;
    }

    [System.Serializable]
    public class AchievementTracker
    {
        public Dictionary<string, float> AchievementProgress = new Dictionary<string, float>();
        public List<string> CompletedAchievements = new List<string>();
        public List<string> HiddenAchievements = new List<string>();
    }

    [System.Serializable]
    public class ProgressionCache
    {
        public Dictionary<string, object> CachedData = new Dictionary<string, object>();
        public System.DateTime LastCacheUpdate;
        public bool IsCacheValid = true;
    }

    [System.Serializable]
    public class BatchMilestoneProcessor
    {
        public List<string> PendingMilestones = new List<string>();
        public float ProcessingInterval = 1.0f;
        public bool EnableBatchProcessing = true;
    }

    [System.Serializable]
    public class CategoryBalanceAnalyzer
    {
        public Dictionary<AchievementCategory, float> CategoryWeights = new Dictionary<AchievementCategory, float>();
        public List<string> RecommendedCategories = new List<string>();
        public float BalanceThreshold = 0.8f;
    }

    [System.Serializable]
    public class MilestoneAnalyticsEngine
    {
        public Dictionary<string, int> CompletionCounts = new Dictionary<string, int>();
        public Dictionary<string, float> AverageCompletionTimes = new Dictionary<string, float>();
        public float OverallEngagement = 0.5f;
    }

    [System.Serializable]
    public class PlayerBehaviorProfiler
    {
        public Dictionary<string, float> BehaviorMetrics = new Dictionary<string, float>();
        public List<string> PreferredCategories = new List<string>();
        public string PlayStyle;
    }

    [System.Serializable]
    public class ProgressionPredictionEngine
    {
        public Dictionary<string, float> PredictedProgress = new Dictionary<string, float>();
        public List<string> RecommendedMilestones = new List<string>();
        public float PredictionAccuracy = 0.7f;
    }

    [System.Serializable]
    public class SynergyCalculator
    {
        public Dictionary<string, float> SynergyStrengths = new Dictionary<string, float>();
        public List<string> ActiveSynergyPairs = new List<string>();
        public float MaxSynergyMultiplier = 2.0f;
    }

    /// <summary>
    /// Achievement badge for player recognition
    /// Enhanced version with additional properties for PlayerRecognitionService
    /// Updated: Fixed CS0117 errors by ensuring all required properties exist
    /// </summary>
    [System.Serializable]
    public class AchievementBadge
    {
        public string BadgeID = "";
        public string BadgeName = "";
        public string Description = "";
        public BadgeType BadgeType = BadgeType.Milestone;
        public int RequiredAchievements = 0;
        public bool IsEarned = false;
        public System.DateTime EarnDate = System.DateTime.MinValue;
        public int Prestige = 0;
        public string Color = "Silver";
        
        // Enhanced properties for PlayerRecognitionService
        public string Icon = "";
        public AchievementRarity Rarity = AchievementRarity.Common;
        public List<string> Requirements = new List<string>();
        public float CompletionRate = 0f;
    }

    /// <summary>
    /// Badge type enumeration for achievement badges
    /// </summary>
    [System.Serializable]
    public enum BadgeType
    {
        Milestone,
        Category, 
        Special,
        Social,
        Ultimate
    }
}