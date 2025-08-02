using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Additional progression data structures for service decomposition
    /// Only contains new types not already defined in existing files
    /// </summary>

    #region New Service-Specific Classes

    [System.Serializable]
    public class PlayerProgressionProfile
    {
        public string PlayerId;
        public string PlayerName;
        public int CurrentLevel;
        public float TotalExperience;
        public Dictionary<string, float> SystemExperience = new Dictionary<string, float>();
        public List<string> CompletedMilestones = new List<string>();
        public List<string> UnlockedContent = new List<string>();
        public Dictionary<string, DateTime> RecentSystemActivity = new Dictionary<string, DateTime>();
        public DateTime ProfileCreatedDate;
        public DateTime LastActivityDate;
        public ProgressionStats Stats;
    }

    [System.Serializable]
    public class ProgressionStats
    {
        public int TotalLevelsGained;
        public float TotalExperienceEarned;
        public int TotalMilestonesCompleted;
        public int TotalContentUnlocked;
        public int TotalAchievementsUnlocked;
        public TimeSpan TotalPlayTime;
        public Dictionary<string, float> SystemStats = new Dictionary<string, float>();
    }

    [System.Serializable]
    public class CrossSystemBonus
    {
        public string BonusID;
        public string BonusName;
        public string Description;
        public List<string> RequiredSystems = new List<string>();
        public float ExperienceMultiplier;
        public bool IsActive;
        public int ActivationCount;
        public DateTime LastActivation;
        public TimeSpan Duration;
    }

    [System.Serializable]
    public class ExperienceCalculationResult
    {
        public float BaseExperience;
        public float QualityBonus;
        public float DifficultyBonus;
        public float EfficiencyBonus;
        public float CrossSystemMultiplier;
        public Dictionary<string, float> ContextualBonuses = new Dictionary<string, float>();
        public float FinalExperience;
        public string CalculationBreakdown;
    }

    [System.Serializable]
    public class LevelUpResult
    {
        public int OldLevel;
        public int NewLevel;
        public int SkillPointsAwarded;
        public List<CleanProgressionReward> LevelRewards = new List<CleanProgressionReward>();
        public List<string> UnlockedContent = new List<string>();
        public bool HasMilestoneCompletion;
        public List<string> CompletedMilestones = new List<string>();
    }

    [System.Serializable]
    public class SkillTreeNode
    {
        public string SkillId;
        public string SkillName;
        public string Description;
        public SkillCategory Category;
        public int MaxLevel;
        public int CurrentLevel;
        public int RequiredSkillPoints;
        public List<string> Prerequisites = new List<string>();
        public List<string> EffectIds = new List<string>(); // Reference to existing SkillEffect definitions
        public bool IsUnlocked;
        public DateTime UnlockDate;
    }

    // SkillEffect already defined in SkillNodeSO.cs - using existing definition

    [System.Serializable]
    public class SkillPath
    {
        public string PathId;
        public string PathName;
        public List<string> SkillSequence = new List<string>();
        public int TotalSkillPointsCost;
        public float EstimatedCompletionTime;
        public PathDifficulty Difficulty;
        public List<string> Benefits = new List<string>();
    }

    [System.Serializable]
    public class LongTermGoal
    {
        public string GoalId;
        public string GoalName;
        public string Description;
        public GoalCategory Category;
        public float TargetValue;
        public float CurrentValue;
        public DateTime StartDate;
        public DateTime TargetDate;
        public GoalPriority Priority;
        public List<string> Milestones = new List<string>();
        public bool IsActive;
        public bool IsCompleted;
    }

    [System.Serializable]
    public class GoalProgress
    {
        public string GoalId;
        public float CompletionPercentage;
        public TimeSpan EstimatedTimeToCompletion;
        public float AverageProgressRate;
        public List<ProgressSnapshot> History = new List<ProgressSnapshot>();
        public string CurrentPhase;
        public List<string> NextMilestones = new List<string>();
    }

    [System.Serializable]
    public class ProgressSnapshot
    {
        public DateTime Timestamp;
        public float Value;
        public string Context;
        public string TriggerEvent;
    }

    [System.Serializable]
    public class ProgressionTrend
    {
        public string TrendId;
        public string TrendName;
        public TrendType Type;
        public float CurrentValue;
        public float PreviousValue;
        public float ChangeRate;
        public TrendDirection Direction;
        public TimeSpan AnalysisPeriod;
    }

    [System.Serializable]
    public class ProgressionEfficiency
    {
        public float OverallEfficiency;
        public Dictionary<string, float> SystemEfficiencies = new Dictionary<string, float>();
        public float ExperiencePerHour;
        public float MilestonesPerSession;
        public float UnlocksPerLevel;
        public EfficiencyRating Rating;
    }

    [System.Serializable]
    public class PlayerRanking
    {
        public int GlobalRank;
        public int TotalPlayers;
        public float Percentile;
        public Dictionary<string, int> SystemRankings = new Dictionary<string, int>();
        public List<RankingCategory> Categories = new List<RankingCategory>();
        public DateTime LastRankingUpdate;
    }

    [System.Serializable]
    public class RankingCategory
    {
        public string CategoryName;
        public int Rank;
        public float Score;
        public string Description;
    }

    [System.Serializable]
    public class ProgressionMetric
    {
        public string MetricId;
        public string MetricName;
        public MetricType Type;
        public float Value;
        public DateTime Timestamp;
        public string Context;
        public TimeSpan Period;
    }

    [System.Serializable]
    public class ProgressionInsight
    {
        public string InsightId;
        public string Title;
        public string Description;
        public InsightType Type;
        public InsightPriority Priority;
        public List<string> Recommendations = new List<string>();
        public float Confidence;
        public DateTime GeneratedDate;
        public Dictionary<string, object> SupportingData = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class ProgressionRecommendation
    {
        public string RecommendationId;
        public string Title;
        public string Description;
        public RecommendationType Type;
        public float PotentialBenefit;
        public float ImplementationDifficulty;
        public TimeSpan EstimatedTimeToResult;
        public List<string> ActionSteps = new List<string>();
        public RecommendationPriority Priority;
    }

    [System.Serializable]
    public class ProgressionComparison
    {
        public string ComparisonId;
        public string PlayerId;
        public string PeerGroup;
        public Dictionary<string, float> RelativePerformance = new Dictionary<string, float>();
        public List<ComparisonMetric> Metrics = new List<ComparisonMetric>();
        public string Summary;
        public DateTime ComparisonDate;
    }

    [System.Serializable]
    public class ComparisonMetric
    {
        public string MetricName;
        public float PlayerValue;
        public float PeerAverage;
        public float PercentileDifference;
        public ComparisonResult Result;
    }

    [System.Serializable]
    public class ProgressionBenchmark
    {
        public string BenchmarkId;
        public string BenchmarkName;
        public BenchmarkCategory Category;
        public float TargetValue;
        public float AverageValue;
        public float TopPercentileValue;
        public string Description;
        public DateTime LastUpdated;
    }

    [System.Serializable]
    public class ExperienceSourceData
    {
        public string SystemName;
        public float BaseMultiplier = 1.0f;
        public float QualityMultiplier = 1.0f;
        public float DifficultyMultiplier = 1.0f;
        public bool IsActive = true;
        public string Description;
    }

    [System.Serializable]
    public class ExperienceMultiplier
    {
        public float BaseMultiplier = 1.0f;
        public float CrossSystemMultiplier = 1.0f;
        public float LevelBonusMultiplier = 1.0f;
        public float TotalMultiplier = 1.0f;
    }

    [System.Serializable]
    public class LevelReward
    {
        public string RewardId;
        public string RewardName;
        public CleanProgressionRewardType Type;
        public float Value;
        public string Description;
    }

    [System.Serializable]
    public class Skill
    {
        public string SkillId;
        public string SkillName;
        public string Description;
        public int Level;
        public int MaxLevel;
        public bool IsUnlocked;
    }

    // AchievementReward already defined in ProgressionDataStructures.cs

    #endregion

    #region New Enums for Service Decomposition
    // Only enums that don't exist in other progression files

    public enum EffectType
    {
        Multiplier,
        Additive,
        Unlock,
        Threshold,
        Conditional
    }

    public enum PathDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master
    }

    public enum GoalCategory
    {
        Skill,
        Achievement,
        System,
        Social,
        Mastery,
        Collection
    }

    public enum GoalPriority
    {
        Low,
        Medium,
        High,
        Ultimate
    }

    public enum TrendType
    {
        Experience,
        Level,
        System,
        Efficiency,
        Social
    }

    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable,
        Volatile
    }

    public enum EfficiencyRating
    {
        Poor,
        Below_Average,
        Average,
        Above_Average,
        Excellent,
        Exceptional
    }

    public enum MetricType
    {
        Experience,
        Level,
        Time,
        Count,
        Ratio,
        Rate
    }

    public enum InsightType
    {
        Performance,
        Opportunity,
        Warning,
        Achievement,
        Recommendation
    }

    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum RecommendationType
    {
        Skill,
        System,
        Goal,
        Strategy,
        Social
    }

    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum ComparisonResult
    {
        Much_Below,
        Below,
        Average,
        Above,
        Much_Above
    }

    public enum BenchmarkCategory
    {
        Experience,
        Level,
        Achievement,
        Skill,
        System,
        Social
    }


    public enum MilestoneType
    {
        Tutorial,
        Level,
        Achievement,
        System,
        Social,
        Research,
        Competition
    }

    #endregion
}