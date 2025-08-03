using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Achievements
{
    /// <summary>
    /// Comprehensive data structures for Project Chimera's Achievement & Milestone system.
    /// Defines all achievement types, progression systems, reward structures, and social recognition
    /// components for the most sophisticated achievement system ever designed for cultivation simulation.
    /// </summary>
    
    #region Core Achievement Enums
    
    /// <summary>
    /// Categories of achievements in Project Chimera
    /// </summary>
    public enum AchievementCategory
    {
        Cultivation,        // Growing and harvesting achievements
        Genetics,          // Breeding and strain development
        Business,          // Economic and market achievements
        Community,         // Social and mentorship achievements
        Education,         // Learning and knowledge achievements
        Discovery,         // Exploration and secret finding
        Cultural,          // Heritage and tradition achievements
        Technical,         // Equipment and facility achievements
        Environmental,     // Sustainability achievements
        Innovation,        // Research and development achievements
        Seasonal,          // Time-based and event achievements
        Hidden,            // Secret and easter egg achievements
        Legacy,            // Long-term impact achievements
        Competition        // Competition and tournament achievements
    }
    
    /// <summary>
    /// Rarity levels for achievements affecting rewards and recognition
    /// </summary>
    public enum AchievementRarity
    {
        Common,           // Basic achievements, frequent unlocks
        Uncommon,         // Moderate challenge, regular recognition
        Rare,             // Significant effort, community attention
        Epic,             // Major accomplishment, regional recognition
        Legendary,        // Exceptional achievement, global recognition
        Mythical,         // Ultra-rare, historical significance
        Divine           // One-of-a-kind, legendary status
    }
    
    /// <summary>
    /// Difficulty levels for achievement progression
    /// </summary>
    public enum AchievementDifficulty
    {
        Trivial,          // Automatic or very easy
        Easy,             // Minimal effort required
        Normal,           // Standard gameplay achievement
        Medium,           // Moderate difficulty level
        Hard,             // Requires dedication and skill
        Expert,           // Master-level accomplishment
        Legendary,        // Extreme difficulty and dedication
        Impossible       // Theoretical maximum difficulty
    }
    
    /// <summary>
    /// Types of achievement progression
    /// </summary>
    public enum AchievementType
    {
        Standard,         // Single unlock achievement
        Progressive,      // Multiple tiers with increasing requirements
        Cumulative,       // Accumulative progress over time
        Sequential,       // Must be completed in order
        Conditional,      // Requires specific conditions
        Timed,           // Time-limited availability
        Hidden,          // Secret achievements with discovery mechanics
        Community,       // Requires community participation
        Competitive,     // Ranking-based achievements
        Milestone,       // Major progression markers
        Easter_Egg,      // Special discovery achievements
        Cultural,        // Heritage and tradition based
        Educational,     // Learning objective achievements
        Progression,     // General progression-based achievements
        Skills          // Skill-related achievements
    }
    
    /// <summary>
    /// Social recognition levels for achievements
    /// </summary>
    public enum SocialRecognitionLevel
    {
        None,             // No social recognition
        Personal,         // Personal milestone only
        Friends,          // Visible to friends network
        Community,        // Local community recognition
        Regional,         // Regional/server recognition
        Global,           // Global community recognition
        Historical       // Permanent historical record
    }
    
    /// <summary>
    /// Trigger types for hidden achievements
    /// </summary>
    public enum HiddenTriggerType
    {
        Action,           // Specific player action
        Sequence,         // Series of actions in order
        Time,             // Time-based conditions
        Location,         // Geographic/coordinate based
        Combination,      // Multiple simultaneous conditions
        Social,           // Multiplayer interaction
        Environmental,    // Game state conditions
        Random,           // Chance-based discovery
        Contextual       // Complex conditional logic
    }
    
    #endregion
    
    #region Core Achievement Structures
    
    /// <summary>
    /// Base achievement definition with comprehensive metadata
    /// </summary>
    [Serializable]
    public class BaseAchievement
    {
        [Header("Basic Information")]
        public string Id = "";
        public string AchievementId = ""; // Compatibility property
        public string Name = "";
        public string Description = "";
        public string DetailedDescription = "";
        public AchievementCategory Category = AchievementCategory.Cultivation;
        public AchievementType Type = AchievementType.Standard;
        public AchievementRarity Rarity = AchievementRarity.Common;
        public AchievementDifficulty Difficulty = AchievementDifficulty.Normal;
        
        [Header("Visual Assets")]
        public Sprite Icon;
        public Sprite IconLocked;
        public Sprite IconProgression;
        public Sprite Badge;
        public Color ThemeColor = Color.white;
        public List<string> AdditionalAssets = new List<string>();
        
        [Header("Progress and Requirements")]
        public float RequiredValue = 1f;
        public string RequiredValueDescription = "";
        public List<AchievementRequirement> Prerequisites = new List<AchievementRequirement>();
        public List<string> RequiredAchievements = new List<string>();
        public bool IsRepeatable = false;
        public TimeSpan? TimeLimit;
        
        [Header("Rewards and Recognition")]
        public int BaseExperienceReward = 100;
        public int BaseCurrencyReward = 0;
        public int BaseReputationReward = 0;
        public List<string> ItemRewards = new List<string>();
        public List<string> FeatureUnlocks = new List<string>();
        public SocialRecognitionLevel SocialRecognition = SocialRecognitionLevel.Personal;
        
        [Header("Educational Content")]
        public bool HasEducationalContent = false;
        public List<string> LearningObjectives = new List<string>();
        public List<string> CultivationFacts = new List<string>();
        public string ScientificBasis = "";
        public float EducationalWeight = 0f;
        
        [Header("Cultural and Historical")]
        public bool HasCulturalSignificance = false;
        public string CulturalContext = "";
        public string HistoricalBackground = "";
        public List<string> CulturalTags = new List<string>();
        
        [Header("System Integration")]
        public bool IsActive = true;
        public DateTime? AvailableFrom;
        public DateTime? AvailableUntil;
        public List<string> IntegrationTags = new List<string>();
        public Dictionary<string, object> CustomProperties = new Dictionary<string, object>();
        
        [Header("Metadata")]
        public DateTime CreationDate = DateTime.Now;
        public string CreatedBy = "";
        public int Version = 1;
        public List<string> UpdateNotes = new List<string>();
    }
    
    /// <summary>
    /// Progressive achievement with multiple tiers and escalating rewards
    /// </summary>
    [Serializable]
    public class ProgressiveAchievement : BaseAchievement
    {
        [Header("Progressive Tiers")]
        public List<AchievementTier> Tiers = new List<AchievementTier>();
        public bool EnableTierBonuses = true;
        public float TierBonusMultiplier = 1.2f;
        public string ProgressionType = "Linear"; // Linear, Exponential, Custom
        
        [Header("Progression Display")]
        public string ProgressFormat = "{0}/{1}";
        public string CompletionMessage = "Tier {0} completed!";
        public bool ShowNextTierPreview = true;
        public bool ShowOverallProgress = true;
    }
    
    /// <summary>
    /// Hidden achievement with complex discovery mechanics
    /// </summary>
    [Serializable]
    public class HiddenAchievement : BaseAchievement
    {
        [Header("Discovery Mechanics")]
        public List<HiddenTrigger> Triggers = new List<HiddenTrigger>();
        public List<HiddenCondition> Conditions = new List<HiddenCondition>();
        public bool RequireAllConditions = true;
        public bool RequireSequentialTriggers = false;
        
        [Header("Hint System")]
        public List<string> DiscoveryHints = new List<string>();
        public int HintsUnlockedByProgress = 0;
        public bool EnableHintProgression = true;
        public string CrypticDescription = "";
        
        [Header("Discovery Rewards")]
        public float DiscoveryBonusMultiplier = 2.0f;
        public List<string> UniqueDiscoveryRewards = new List<string>();
        public bool GrantExplorerStatus = false;
        public string DiscoveryTitle = "";
        
        [Header("Rarity and Exclusivity")]
        public int MaxDiscoverers = -1; // -1 for unlimited
        public bool IsGloballyUnique = false;
        public bool RequiresCommunityEffort = false;
        public int CommunityDiscoveryThreshold = 1;
    }
    
    /// <summary>
    /// Community achievement requiring collaborative effort
    /// </summary>
    [Serializable]
    public class CommunityAchievement : BaseAchievement
    {
        [Header("Community Requirements")]
        public int MinimumParticipants = 1;
        public int MaximumParticipants = -1; // -1 for unlimited
        public bool RequiresDiverseSkills = false;
        public List<string> RequiredRoles = new List<string>();
        
        [Header("Collaboration Mechanics")]
        public bool EnableProgressSharing = true;
        public bool RequiresCoordination = false;
        public float IndividualContributionWeight = 1.0f;
        public float TeamworkBonusMultiplier = 1.5f;
        
        [Header("Community Impact")]
        public string CommunityBenefit = "";
        public bool GeneratesPublicResource = false;
        public string PublicResourceType = "";
        public float CommunityReputationImpact = 0f;
        
        [Header("Recognition")]
        public bool RecognizeAllParticipants = true;
        public bool RecognizeTopContributors = true;
        public int TopContributorCount = 3;
        public List<string> LeadershipRewards = new List<string>();
    }
    
    #endregion
    
    #region Achievement Progression and Tracking
    
    /// <summary>
    /// Individual tier within a progressive achievement
    /// </summary>
    [Serializable]
    public class AchievementTier
    {
        public int TierLevel = 1;
        public string TierName = "";
        public string TierDescription = "";
        public float RequiredValue = 1f;
        public int ExperienceReward = 100;
        public int CurrencyReward = 0;
        public int ReputationReward = 0;
        public List<string> TierItemRewards = new List<string>();
        public List<string> TierUnlocks = new List<string>();
        public string TierTitle = "";
        public Sprite TierBadge;
        public Color TierColor = Color.white;
        public bool IsCurrentTier = false;
        public DateTime? UnlockedDate;
    }
    
    /// <summary>
    /// Player's progress towards a specific achievement
    /// </summary>
    [Serializable]
    public class AchievementProgress
    {
        public string AchievementId = "";
        public string PlayerId = "";
        public float CurrentValue = 0f;
        public float RequiredValue = 1f;
        public float ProgressPercentage = 0f;
        public int CurrentTier = 0;
        public bool IsCompleted = false;
        public bool IsActive = true;
        
        [Header("Timing")]
        public DateTime StartedDate = DateTime.Now;
        public DateTime? CompletedDate;
        public DateTime LastUpdateDate = DateTime.Now;
        public TimeSpan? TimeToComplete;
        public TimeSpan? RemainingTime;
        
        [Header("Progress Tracking")]
        public List<ProgressMilestone> Milestones = new List<ProgressMilestone>();
        public Dictionary<string, float> SubProgress = new Dictionary<string, float>();
        public List<string> CompletedRequirements = new List<string>();
        public float DailyProgress = 0f;
        public float WeeklyProgress = 0f;
        public float AverageProgressRate = 0f;
        
        [Header("Context")]
        public Dictionary<string, object> ProgressContext = new Dictionary<string, object>();
        public List<string> ProgressNotes = new List<string>();
        public string LastProgressAction = "";
        public DateTime LastSignificantProgress = DateTime.Now;
    }
    
    /// <summary>
    /// Milestone within achievement progression
    /// </summary>
    [Serializable]
    public class ProgressMilestone
    {
        public string MilestoneId = "";
        public string MilestoneName = "";
        public float ThresholdValue = 0f;
        public bool IsReached = false;
        public DateTime? ReachedDate;
        public List<string> MilestoneRewards = new List<string>();
        public string MilestoneMessage = "";
    }
    
    /// <summary>
    /// Completed achievement record
    /// </summary>
    [Serializable]
    public class CompletedAchievement
    {
        public string AchievementId = "";
        public string PlayerId = "";
        public BaseAchievement Achievement;
        public DateTime CompletionDate = DateTime.Now;
        public int CompletedTier = 1;
        public float FinalValue = 0f;
        public TimeSpan TimeToComplete;
        
        [Header("Completion Context")]
        public string CompletionMethod = "";
        public Dictionary<string, object> CompletionContext = new Dictionary<string, object>();
        public List<string> AssistingPlayers = new List<string>();
        public bool WasCommunityEffort = false;
        
        [Header("Rewards Received")]
        public int ExperienceAwarded = 0;
        public int CurrencyAwarded = 0;
        public int ReputationAwarded = 0;
        public List<string> ItemsAwarded = new List<string>();
        public List<string> FeaturesUnlocked = new List<string>();
        public string TitleAwarded = "";
        
        [Header("Recognition")]
        public SocialRecognitionLevel RecognitionReceived = SocialRecognitionLevel.Personal;
        public bool WasGloballyAnnounced = false;
        public int CommunityLikes = 0;
        public List<string> CommunityComments = new List<string>();
        public bool IsFeatured = false;
        public DateTime? FeaturedUntil;
    }
    
    #endregion
    
    #region Reward and Recognition Systems
    
    /// <summary>
    /// Comprehensive reward package for achievement completion
    /// </summary>
    [Serializable]
    public class AchievementReward
    {
        [Header("Base Rewards")]
        public string Name = "";
        public long Value = 0;
        public int ExperiencePoints = 0;
        public int CurrencyAmount = 0;
        public int ReputationPoints = 0;
        public List<ItemReward> Items = new List<ItemReward>();
        
        [Header("Progression Rewards")]
        public List<string> FeatureUnlocks = new List<string>();
        public List<string> ContentUnlocks = new List<string>();
        public List<string> ToolUnlocks = new List<string>();
        public string PlayerTitle = "";
        public string PlayerBadge = "";
        
        [Header("Social Rewards")]
        public int SocialInfluencePoints = 0;
        public int MentorshipCredits = 0;
        public bool GrantsLeadershipStatus = false;
        public List<string> CommunityPrivileges = new List<string>();
        
        [Header("Bonus Multipliers")]
        public float ExperienceMultiplier = 1.0f;
        public float CurrencyMultiplier = 1.0f;
        public float ReputationMultiplier = 1.0f;
        public float SocialInfluenceMultiplier = 1.0f;
        
        [Header("Time-Limited Bonuses")]
        public List<TimeLimitedBonus> TimedBonuses = new List<TimeLimitedBonus>();
        public TimeSpan? BonusDuration;
        public bool StacksWithOtherBonuses = true;
        
        [Header("Unique Rewards")]
        public bool IsUniqueReward = false;
        public string UniqueRewardType = "";
        public Dictionary<string, object> CustomRewardData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Individual item reward
    /// </summary>
    [Serializable]
    public class ItemReward
    {
        public string ItemId = "";
        public string ItemName = "";
        public int Quantity = 1;
        public float Quality = 1.0f;
        public List<string> ItemModifiers = new List<string>();
        public bool IsRare = false;
        public bool IsUnique = false;
        public string CustomDescription = "";
    }
    
    /// <summary>
    /// Time-limited bonus effect
    /// </summary>
    [Serializable]
    public class TimeLimitedBonus
    {
        public string BonusType = "";
        public float BonusValue = 0f;
        public TimeSpan Duration;
        public DateTime StartTime = DateTime.Now;
        public bool IsActive = true;
        public bool StacksWithSimilar = false;
        public string DisplayName = "";
        public string Description = "";
    }
    
    #endregion
    
    #region Hidden Achievement Systems
    
    /// <summary>
    /// Trigger condition for hidden achievements
    /// </summary>
    [Serializable]
    public class HiddenTrigger
    {
        public string TriggerId = "";
        public HiddenTriggerType TriggerType = HiddenTriggerType.Action;
        public string TriggerAction = "";
        public Dictionary<string, object> TriggerParameters = new Dictionary<string, object>();
        public int RequiredOccurrences = 1;
        public TimeSpan? TimeWindow;
        public bool MustBeConsecutive = false;
        public List<string> RequiredContext = new List<string>();
    }
    
    /// <summary>
    /// Complex condition for hidden achievement discovery
    /// </summary>
    [Serializable]
    public class HiddenCondition
    {
        public string ConditionId = "";
        public string ConditionType = "";
        public object RequiredValue;
        public string ComparisonOperator = "equals"; // equals, greater, less, contains, etc.
        public bool IsOptional = false;
        public float Weight = 1.0f;
        public string FailureMessage = "";
        public string SuccessMessage = "";
        public Dictionary<string, object> ConditionData = new Dictionary<string, object>();
    }
    
    #endregion
    
    #region Social and Community Systems
    
    /// <summary>
    /// Social recognition record
    /// </summary>
    [Serializable]
    public class SocialRecognition
    {
        public string PlayerId = "";
        public string AchievementId = "";
        public SocialRecognitionLevel Level = SocialRecognitionLevel.Personal;
        public DateTime RecognitionDate = DateTime.Now;
        public int CommunityVotes = 0;
        public int GlobalRanking = 0;
        public bool IsFeatured = false;
        public DateTime? FeaturedUntil;
        public List<string> CommunityComments = new List<string>();
        public Dictionary<string, int> ReactionCounts = new Dictionary<string, int>();
        public float SocialImpactScore = 0f;
    }
    
    /// <summary>
    /// Community contribution tracking
    /// </summary>
    [Serializable]
    public class CommunityContribution
    {
        public string ContributorId = "";
        public string AchievementId = "";
        public float ContributionValue = 0f;
        public float ContributionPercentage = 0f;
        public string ContributionType = "";
        public DateTime ContributionDate = DateTime.Now;
        public bool IsSignificantContribution = false;
        public List<string> ContributionActions = new List<string>();
        public Dictionary<string, object> ContributionContext = new Dictionary<string, object>();
    }
    
    #endregion
    
    #region Achievement System Requirements and Validation
    
    /// <summary>
    /// Requirement for achievement unlock or progression
    /// </summary>
    [Serializable]
    public class AchievementRequirement
    {
        public string RequirementId = "";
        public string RequirementType = "";
        public object RequiredValue;
        public string Description = "";
        public bool IsOptional = false;
        public float Weight = 1.0f;
        public bool IsMet = false;
        public DateTime? MetDate;
        public string ValidationMethod = "";
        public Dictionary<string, object> ValidationData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Player statistics for achievement tracking
    /// </summary>
    [Serializable]
    public class PlayerAchievementStatistics
    {
        [Header("Basic Statistics")]
        public string PlayerId = "";
        public int TotalAchievementsUnlocked = 0;
        public int TotalAchievementPoints = 0;
        public float AchievementCompletionRate = 0f;
        public int CurrentStreak = 0;
        public int LongestStreak = 0;
        
        [Header("Category Statistics")]
        public Dictionary<AchievementCategory, int> CategoryCompletions = new Dictionary<AchievementCategory, int>();
        public Dictionary<AchievementCategory, float> CategoryProgress = new Dictionary<AchievementCategory, float>();
        public Dictionary<AchievementRarity, int> RarityUnlocks = new Dictionary<AchievementRarity, int>();
        
        [Header("Time-Based Statistics")]
        public DateTime FirstAchievement = DateTime.Now;
        public DateTime LastAchievement = DateTime.Now;
        public TimeSpan TotalTimeToAchievements;
        public float AverageTimeToCompletion = 0f;
        public int AchievementsThisWeek = 0;
        public int AchievementsThisMonth = 0;
        
        [Header("Social Statistics")]
        public int GlobalRecognitions = 0;
        public int CommunityAchievements = 0;
        public int HiddenAchievementsFound = 0;
        public float SocialInfluenceScore = 0f;
        public int MentorshipCount = 0;
        public int CommunityHelpCount = 0;
        
        [Header("Advanced Metrics")]
        public float EngagementScore = 0f;
        public float DifficultyPreference = 0f;
        public List<string> FavoriteCategories = new List<string>();
        public Dictionary<string, float> SkillProgression = new Dictionary<string, float>();
        public List<string> PreferredRewardTypes = new List<string>();
    }
    
    #endregion
}