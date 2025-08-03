using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Data.Progression.Achievements
{
    /// <summary>
    /// Achievement system data structures extracted from ProgressionDataStructures.cs
    /// Contains achievement definitions, progress tracking, rewards, and analytics
    /// Part of Phase 1 Foundation Data Structures refactoring
    /// </summary>

    #region Core Achievement Types

    /// <summary>
    /// Achievement category enumeration
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

    /// <summary>
    /// Achievement type enumeration
    /// </summary>
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

    /// <summary>
    /// Achievement reward type enumeration
    /// </summary>
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

    /// <summary>
    /// Achievement difficulty levels
    /// </summary>
    public enum AchievementDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        Legendary
    }

    /// <summary>
    /// Achievement rarity classification
    /// </summary>
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    #endregion

    #region Achievement Definitions

    /// <summary>
    /// Core achievement definition
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

    /// <summary>
    /// Achievement requirements definition
    /// </summary>
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

    /// <summary>
    /// Individual achievement requirement
    /// </summary>
    [System.Serializable]
    public class AchievementRequirement
    {
        public string RequirementId;
        public AchievementRequirementType RequirementType;
        public string TargetId; // ID of target skill, research, etc.
        public float RequiredValue;
        public ComparisonType Comparison;
        public string Description;
        public bool IsOptional = false;
        public float Weight = 1f; // For weighted progress calculations
    }

    /// <summary>
    /// Achievement reward definition
    /// </summary>
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

    /// <summary>
    /// Progress condition for achievement tracking
    /// </summary>
    [System.Serializable]
    public class ProgressCondition
    {
        public ProgressType ProgressType;
        public string TargetId;
        public float RequiredValue;
        public ComparisonType Comparison;
        public string Description;
    }

    #endregion

    #region Hidden Achievements

    /// <summary>
    /// Hidden achievement definition
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementDefinition
    {
        public string AchievementId;
        public string InternalName; // For development reference
        public AchievementCategory Category;
        public List<ComplexCondition> UnlockConditions = new List<ComplexCondition>();
        public List<AchievementReward> Rewards = new List<AchievementReward>();
        public float ExperienceReward = 200f;
        public bool RequiresDiscovery = true;
        public int MaxDiscoveries = 1;
        public List<HiddenAchievementHint> Hints = new List<HiddenAchievementHint>();
    }

    /// <summary>
    /// Complex condition for hidden achievements
    /// </summary>
    [System.Serializable]
    public class ComplexCondition
    {
        public string ConditionId;
        public ComplexConditionType ConditionType;
        public List<string> RequiredActions = new List<string>();
        public Dictionary<string, float> RequiredValues = new Dictionary<string, float>();
        public bool RequiresSequence = false;
        public float TimeWindow = -1f; // Time window in hours, -1 = unlimited
        public string Description;
    }

    /// <summary>
    /// Hint for discovering hidden achievements
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementHint
    {
        public string HintId;
        public HintType HintType;
        public string HintText;
        public bool IsUnlocked = false;
        public List<string> UnlockConditions = new List<string>();
        public int HintLevel = 1; // 1 = vague, 5 = specific
    }

    /// <summary>
    /// Hidden achievement discovery record
    /// </summary>
    [System.Serializable]
    public class HiddenAchievementDiscovery
    {
        public string PlayerId;
        public string AchievementId;
        public DateTime DiscoveryTime;
        public string DiscoveryMethod; // How it was discovered
        public Dictionary<string, object> DiscoveryContext = new Dictionary<string, object>();
        public bool WasHinted = false;
        public int AttemptsBeforeDiscovery = 0;
        public float TimeToDiscovery = 0f; // Hours since first hint
    }

    #endregion

    #region Player Achievement Progress

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
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastPlayTime = DateTime.Now;
        
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
        public DateTime FirstAchievement = DateTime.Now;
        public DateTime LastAchievement = DateTime.Now;
        
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
        public DateTime LastCommunityContribution = DateTime.MinValue;
        
        // Properties for PlayerShowcase compatibility
        public float CompletionPercentage = 0f;
        public float TotalScore = 0f;
        
        // Additional properties for PlayerRecognitionService compatibility
        public string PlayerID { get => PlayerId; set => PlayerId = value; } // Alias for PlayerId
        public int TotalAchievements { get => TotalAchievementsUnlocked; set => TotalAchievementsUnlocked = value; } // Alias
        public float TotalPoints { get => TotalScore; set => TotalScore = value; } // Alias for TotalScore
        public string CurrentTier = "Novice";
        public AchievementCategory FavoriteCategory = AchievementCategory.Cultivation_Mastery;
        public DateTime LastUnlock { get => LastAchievement; set => LastAchievement = value; } // Alias
        public List<string> UnlockHistory = new List<string>();
        public List<string> EarnedBadges = new List<string>();
        public int Prestige = 0;
        public int SocialRecognition = 0;
        public int CommunityContributions = 0;
        public DateTime ProfileCreatedDate { get => CreationDate; set => CreationDate = value; } // Alias
        public DateTime LastUpdateDate { get => LastPlayTime; set => LastPlayTime = value; } // Alias
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
                LastAchievement = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Record of an unlocked achievement
    /// </summary>
    [System.Serializable]
    public class UnlockedAchievement
    {
        public string AchievementId;
        public string PlayerId;
        public DateTime UnlockTime;
        public AchievementContext UnlockContext;
        public float CompletionValue; // The value that triggered completion
        public bool WasShared = false;
        public List<string> WitnessPlayers = new List<string>(); // Other players who witnessed the unlock
        public float DifficultyAtUnlock = 1f;
        public string UnlockMethod; // How the achievement was unlocked
        public Dictionary<string, object> UnlockData = new Dictionary<string, object>();
        
        // Additional properties for achievement showcase
        public bool IsFeatured = false;
        public int LikesReceived = 0;
        public List<string> Comments = new List<string>();
        public bool IsPublic = true;
        
        // Properties for rarity and prestige calculation
        public float GlobalRarity = 1f; // Percentage of players who have this achievement
        public int PrestigeValue = 1;
        public bool IsFirstEver = false; // First player globally to unlock this
    }

    /// <summary>
    /// Achievement context for tracking unlock conditions
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
        public DateTime EventTime = DateTime.Now;
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

    #endregion

    #region Achievement Progress Tracking

    /// <summary>
    /// Achievement progress tracking
    /// </summary>
    [System.Serializable]
    public class AchievementProgress
    {
        public string AchievementId;
        public string PlayerId;
        public float CurrentProgress = 0f;
        public float RequiredProgress = 1f;
        public bool IsCompleted = false;
        public DateTime StartTime;
        public DateTime LastUpdateTime;
        public List<ProgressStep> ProgressHistory = new List<ProgressStep>();
        public Dictionary<string, float> SubProgress = new Dictionary<string, float>(); // For multi-part achievements
        public bool IsTracking = true;
        public float EstimatedTimeToComplete = -1f; // In hours, -1 = unknown
    }

    /// <summary>
    /// Individual progress step
    /// </summary>
    [System.Serializable]
    public class ProgressStep
    {
        public DateTime Timestamp;
        public float ProgressDelta;
        public string ActionType;
        public string Description;
        public Dictionary<string, object> StepData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Achievement progress info for UI display
    /// </summary>
    [System.Serializable]
    public class AchievementProgressInfo
    {
        public string AchievementId;
        public string AchievementName;
        public string Description;
        public Sprite Icon;
        public float ProgressPercentage = 0f;
        public string ProgressText;
        public bool IsVisible = true;
        public bool IsCompleted = false;
        public bool IsNew = false;
        public AchievementCategory Category;
        public AchievementDifficulty Difficulty;
        public List<ProgressMilestone> Milestones = new List<ProgressMilestone>();
        public DateTime EstimatedCompletionTime;
        public string NextStepHint;
    }

    /// <summary>
    /// Progress milestone within an achievement
    /// </summary>
    [System.Serializable]
    public class ProgressMilestone
    {
        public string MilestoneName;
        public float ProgressThreshold; // 0.0 to 1.0
        public bool IsCompleted = false;
        public DateTime CompletionTime;
        public string Description;
        public List<AchievementReward> MilestoneRewards = new List<AchievementReward>();
    }

    #endregion

    #region Achievement Tiers and Ranking

    /// <summary>
    /// Achievement tier information
    /// </summary>
    [System.Serializable]
    public class AchievementTierInfo
    {
        public string TierId;
        public string TierName;
        public int RequiredAchievements;
        public int RequiredPoints;
        public List<AchievementCategory> RequiredCategories = new List<AchievementCategory>();
        public AchievementDifficulty MinimumDifficulty;
        public Color TierColor = Color.white;
        public Sprite TierIcon;
        public List<TierBenefit> TierBenefits = new List<TierBenefit>();
        public bool IsPrestigeTier = false;
        public string Description;
    }

    /// <summary>
    /// Benefit granted by achievement tier
    /// </summary>
    [System.Serializable]
    public class TierBenefit
    {
        public string BenefitName;
        public TierBenefitType BenefitType;
        public float BenefitValue;
        public string Description;
        public bool IsActive = true;
    }

    /// <summary>
    /// Achievement tier definition
    /// </summary>
    [System.Serializable]
    public class AchievementTier
    {
        public string TierId;
        public string TierName;
        public string Description;
        public AchievementTierRequirements Requirements;
        public List<TierBenefit> Benefits = new List<TierBenefit>();
        public Color TierColor = Color.white;
        public Sprite TierBadge;
        public int TierLevel = 1;
        public bool IsPrestigeTier = false;
        public bool RequiresNomination = false;
    }

    /// <summary>
    /// Requirements for achievement tier
    /// </summary>
    [System.Serializable]
    public class AchievementTierRequirements
    {
        public int MinimumAchievements = 10;
        public int MinimumPoints = 1000;
        public List<AchievementCategory> RequiredCategories = new List<AchievementCategory>();
        public List<string> SpecificAchievements = new List<string>(); // Must have these specific achievements
        public AchievementDifficulty MinimumDifficulty = AchievementDifficulty.Normal;
        public float MinimumCompletionRate = 0.7f; // 70%
        public int CommunityEndorsements = 0;
        public bool RequiresGoodStanding = true;
        public List<string> ExcludedPlayers = new List<string>();
    }

    #endregion

    #region Achievement Analytics and Predictions

    /// <summary>
    /// Achievement prediction system
    /// </summary>
    [System.Serializable]
    public class AchievementPrediction
    {
        public string PlayerId;
        public List<string> RecommendedAchievements = new List<string>();
        public Dictionary<string, float> PredictedCompletionTimes = new Dictionary<string, float>();
        public Dictionary<string, float> SuccessProbabilities = new Dictionary<string, float>();
        public DateTime PredictionDate;
        public string PredictionMethod;
        public float PredictionConfidence = 0.8f;
    }

    /// <summary>
    /// Achievement analytics report
    /// </summary>
    [System.Serializable]
    public class AchievementAnalyticsReport
    {
        public DateTime ReportDate;
        public string PlayerId;
        public int TotalAchievements;
        public int CompletedAchievements;
        public float CompletionRate;
        public Dictionary<AchievementCategory, int> CategoryBreakdown = new Dictionary<AchievementCategory, int>();
        public Dictionary<AchievementDifficulty, int> DifficultyBreakdown = new Dictionary<AchievementDifficulty, int>();
        public float AverageTimeToComplete;
        public List<string> MostProgressedAchievements = new List<string>();
        public List<string> RecommendedNext = new List<string>();
        public float EngagementScore;
        public string PlayerType; // Based on achievement patterns
    }

    /// <summary>
    /// Achievement analytics data
    /// </summary>
    [System.Serializable]
    public class AchievementAnalyticsData
    {
        public string PlayerId;
        public Dictionary<string, float> CompletionTimes = new Dictionary<string, float>();
        public Dictionary<string, int> AttemptCounts = new Dictionary<string, int>();
        public List<string> AbandonedAchievements = new List<string>();
        public Dictionary<AchievementCategory, float> CategoryPreferences = new Dictionary<AchievementCategory, float>();
        public float OverallEngagement;
        public DateTime LastAnalysisUpdate;
    }

    #endregion

    #region Specialized Achievement Types

    /// <summary>
    /// Cultivation-specific achievement data
    /// </summary>
    [System.Serializable]
    public class CultivationAchievementData
    {
        public int PlantsGrown = 0;
        public float TotalYield = 0f;
        public float BestQuality = 0f;
        public int StrainsMastered = 0;
        public Dictionary<string, int> StrainCounts = new Dictionary<string, int>();
    }

    /// <summary>
    /// Economic achievement data
    /// </summary>
    [System.Serializable]
    public class EconomicAchievementData
    {
        public float TotalProfit = 0f;
        public float BestSingleSale = 0f;
        public int SuccessfulTrades = 0;
        public float MarketShareAchieved = 0f;
    }

    /// <summary>
    /// Educational achievement data
    /// </summary>
    [System.Serializable]
    public class EducationalAchievementData
    {
        public int PlayersHelped = 0;
        public int TutorialsCompleted = 0;
        public int ResourcesShared = 0;
        public float TeachingRating = 0f;
    }

    /// <summary>
    /// Progressive achievement data
    /// </summary>
    [System.Serializable]
    public class ProgressiveAchievementData
    {
        public int CurrentLevel = 1;
        public int MaxLevel = 10;
        public float CurrentLevelProgress = 0f;
        public List<float> LevelThresholds = new List<float>();
        public List<AchievementReward> LevelRewards = new List<AchievementReward>();
    }

    /// <summary>
    /// Community achievement data
    /// </summary>
    [System.Serializable]
    public class CommunityAchievementData
    {
        public int ParticipantsRequired = 10;
        public int CurrentParticipants = 0;
        public List<string> ParticipantIds = new List<string>();
        public DateTime EventStartTime;
        public DateTime EventEndTime;
        public bool IsGlobalEvent = false;
    }

    /// <summary>
    /// Seasonal achievement data
    /// </summary>
    [System.Serializable]
    public class SeasonalAchievementData
    {
        public string SeasonId;
        public DateTime SeasonStart;
        public DateTime SeasonEnd;
        public bool IsActive = false;
        public List<string> SeasonalRewards = new List<string>();
        public float SeasonMultiplier = 1f;
    }

    #endregion

    #region Achievement Triggers and Events

    /// <summary>
    /// Achievement trigger system
    /// </summary>
    [System.Serializable]
    public class AchievementTrigger
    {
        public string TriggerId;
        public string AchievementId;
        public AchievementTriggerType TriggerType;
        public string EventType;
        public Dictionary<string, object> TriggerConditions = new Dictionary<string, object>();
        public bool IsActive = true;
        public float CooldownTime = 0f; // Seconds between triggers
        public DateTime LastTriggered;
        public int MaxTriggers = -1; // -1 = unlimited
        public int CurrentTriggers = 0;
    }

    /// <summary>
    /// Achievement event data
    /// </summary>
    [System.Serializable]
    public class AchievementEventData
    {
        public string EventId;
        public string EventType;
        public string PlayerId;
        public string AchievementId;
        public DateTime EventTime;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public AchievementContext Context;
        public bool ProcessedSuccessfully = false;
    }

    #endregion

    #region Achievement Badges and Display

    /// <summary>
    /// Achievement badge system
    /// </summary>
    [System.Serializable]
    public class AchievementBadge
    {
        public string BadgeId;
        public string BadgeName;
        public string Description;
        public Sprite BadgeIcon;
        public BadgeType BadgeType;
        public AchievementRarity Rarity;
        public Color BadgeColor = Color.white;
        public bool IsAnimated = false;
        public bool IsDisplayed = true;
        public DateTime EarnedDate;
        public List<string> RequiredAchievements = new List<string>();
    }

    #endregion

    #region Supporting Enums

    public enum ProgressType
    {
        Count,
        Accumulation,
        Achievement,
        Ratio,
        Quality_Threshold,
        Skill_Level
    }

    public enum ComparisonType
    {
        Equal,
        Greater_Than,
        Less_Than,
        Greater_Equal,
        Less_Equal,
        Not_Equal
    }

    public enum AchievementRequirementType
    {
        Player_Level,
        Skill_Level,
        Achievement_Count,
        Research_Complete,
        Facility_Built,
        Plant_Harvested,
        Profit_Earned,
        Quality_Achieved,
        Time_Played,
        Social_Action
    }

    public enum AchievementTriggerType
    {
        Immediate,
        Delayed,
        Conditional,
        Threshold,
        Sequence,
        Time_Based,
        Event_Based,
        Social_Trigger
    }

    public enum ComplexConditionType
    {
        Sequence_Required,
        Simultaneous_Actions,
        Time_Window,
        Location_Specific,
        Context_Dependent,
        Multi_Player,
        Hidden_Pattern,
        Easter_Egg
    }

    public enum HintType
    {
        Text_Hint,
        Visual_Cue,
        Audio_Hint,
        Environmental,
        Interactive,
        Progressive,
        Community_Hint,
        Meta_Hint
    }

    public enum RewardType
    {
        Experience,
        Skill_Points,
        Money,
        Items,
        Unlocks,
        Bonuses,
        Cosmetics,
        Achievements
    }

    public enum TierBenefitType
    {
        Experience_Bonus,
        Skill_Point_Bonus,
        Unlock_Access,
        Social_Status,
        UI_Customization,
        Priority_Support,
        Beta_Access,
        Special_Events
    }

    public enum BadgeType
    {
        Achievement,
        Tier,
        Special_Event,
        Community,
        Developer,
        Limited_Edition,
        Prestige,
        Legacy
    }

    #endregion
}