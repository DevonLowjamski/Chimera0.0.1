using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    // Leaderboard system enums
    public enum LeaderboardType
    {
        Overall,
        Speed,
        Efficiency,
        Innovation,
        Environmental,
        Strategy,
        Regional,
        Tournament,
        Seasonal,
        Weekly,
        Monthly,
        AllTime
    }
    
    public enum TrendDirection
    {
        Rising,
        Falling,
        Stable,
        Volatile,
        NewPlayer
    }
    
    public enum RankTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        Grandmaster,
        Legend
    }
    
    // Core leaderboard structures
    [Serializable]
    public class Leaderboard
    {
        public LeaderboardType Type;
        public string Title;
        public string Description;
        public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
        public DateTime CreatedDate;
        public DateTime LastUpdated;
        public int MaxEntries = 1000;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public bool IsActive = true;
        public string Region;
        public string Category;
    }
    
    [Serializable]
    public class LeaderboardEntry
    {
        public int Rank;
        public string PlayerId;
        public string PlayerName;
        public float Score;
        public int GamesPlayed;
        public float WinRate;
        public DateTime LastActive;
        public string CountryCode;
        public string Region;
        public RankTier Tier;
        public Dictionary<string, object> AdditionalStats = new Dictionary<string, object>();
        public int RankChange; // Positive = rank improved, Negative = rank dropped
        public bool IsProvisional;
        public float RatingConfidence;
        public List<string> Badges = new List<string>();
    }
    
    [Serializable]
    public class LeaderboardData
    {
        public LeaderboardType LeaderboardType;
        public string Title;
        public string Description;
        public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
        public DateTime LastUpdated;
        public int TotalEntries;
        public SeasonInfo SeasonInfo;
        public float UpdateInterval;
        public Dictionary<string, object> Statistics = new Dictionary<string, object>();
        public LeaderboardFilters AppliedFilters;
    }
    
    // Player ranking and profile structures
    [Serializable]
    public class PlayerRankingProfile
    {
        public string PlayerId;
        public string PlayerName;
        public float OverallRating;
        public float PeakRating;
        public int CurrentRank;
        public int PeakRank;
        public int TotalGamesPlayed;
        public int GamesWon;
        public int GamesLost;
        public float WinRate;
        public DateTime LastActive;
        public DateTime CreatedDate;
        public bool IsProvisional;
        public RankTier CurrentTier;
        public Dictionary<string, float> CategoryRatings = new Dictionary<string, float>();
        public int RegionalRank;
        public string CountryCode;
        public string Region;
        public List<Achievement> Achievements = new List<Achievement>();
        public PlayerStatistics Statistics;
        public float RatingConfidence;
        public int ConsecutiveWins;
        public int ConsecutiveLosses;
        public int LongestWinStreak;
    }
    
    [Serializable]
    public class PlayerPerformanceData
    {
        public string PlayerId;
        public string PlayerName;
        public string TournamentId;
        public bool GameWon;
        public float FinalScore;
        public float OpponentRating;
        public float PerformanceRating;
        public TimeSpan GameDuration;
        public StrategyType StrategyUsed;
        public Dictionary<string, float> DetailedMetrics = new Dictionary<string, float>();
        public string CountryCode;
        public string Region;
        public DateTime GameDate;
        public float DifficultyMultiplier;
        public bool WasRankedMatch;
    }
    
    [Serializable]
    public class PlayerStatistics
    {
        public float AverageScore;
        public float HighestScore;
        public float AverageGameTime;
        public float FastestWin;
        public int TotalPestsEliminated;
        public float TotalEfficiencyScore;
        public Dictionary<StrategyType, int> StrategyUsage = new Dictionary<StrategyType, int>();
        public Dictionary<TournamentType, PlayerTournamentStats> TournamentStats = new Dictionary<TournamentType, PlayerTournamentStats>();
        public int TotalTournaments;
        public int TournamentWins;
        public float AveragePlacement;
        public float TotalPrizeMoney;
    }
    
    [Serializable]
    public class PlayerTournamentStats
    {
        public int TournamentsPlayed;
        public int TournamentsWon;
        public float AverageScore;
        public float BestScore;
        public int BestPlacement;
        public float AveragePlacement;
        public float WinRate;
    }
    
    // Ranking history and tracking
    [Serializable]
    public class RankingSnapshot
    {
        public DateTime Timestamp;
        public float Rating;
        public int Rank;
        public RankTier Tier;
        public string TournamentId;
        public string Reason; // "Tournament Win", "Rating Decay", "Season Reset", etc.
        public float RatingChange;
        public int RankChange;
    }
    
    [Serializable]
    public class PlayerRankingHistory
    {
        public string PlayerId;
        public List<RankingSnapshot> Snapshots = new List<RankingSnapshot>();
        public float PeakRating;
        public float LowestRating;
        public float RatingChange;
        public TrendDirection TrendDirection;
        public float Volatility;
        public DateTime FirstRanked;
        public int TotalRankChanges;
    }
    
    [Serializable]
    public class RankChange
    {
        public DateTime Date;
        public int OldRank;
        public int NewRank;
        public int RankDifference;
        public string Reason;
        public LeaderboardType LeaderboardType;
    }
    
    // Leaderboard status and display
    [Serializable]
    public class PlayerLeaderboardStatus
    {
        public string PlayerId;
        public string PlayerName;
        public int OverallRank;
        public float OverallRating;
        public RankTier CurrentTier;
        public int RegionalRank;
        public Dictionary<string, int> CategoryRankings = new Dictionary<string, int>();
        public List<RankChange> RecentRankChanges = new List<RankChange>();
        public string SeasonHighlight;
        public string NextMilestone;
        public float ProgressToNextTier;
        public List<Achievement> RecentAchievements = new List<Achievement>();
    }
    
    // Filtering and search
    [Serializable]
    public class LeaderboardFilters
    {
        public string Region;
        public string CountryCode;
        public RankTier? MinTier;
        public RankTier? MaxTier;
        public int? MinGames;
        public int? MaxGames;
        public bool OnlyActive; // Active in last X days
        public int ActiveDays = 30;
        public bool ExcludeProvisional;
        public StrategyType? PreferredStrategy;
        public TournamentType? TournamentType;
        public DateTime? DateFrom;
        public DateTime? DateTo;
    }
    
    // Regional and specialized leaderboards
    [Serializable]
    public class RegionalLeaderboard : Leaderboard
    {
        public string RegionCode;
        public string RegionName;
        public int TotalPlayers;
        public float AverageRating;
        public string TopPlayer;
        public Dictionary<string, int> CountryDistribution = new Dictionary<string, int>();
    }
    
    // Season management
    [Serializable]
    public class SeasonInfo
    {
        public int SeasonNumber;
        public string SeasonName;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public int TotalPlayers;
        public string CurrentChampion;
        public float ProgressPercentage;
        public Dictionary<string, object> SeasonGoals = new Dictionary<string, object>();
        public List<SeasonMilestone> Milestones = new List<SeasonMilestone>();
    }
    
    [Serializable]
    public class SeasonRecord
    {
        public int SeasonNumber;
        public DateTime StartDate;
        public DateTime EndDate;
        public int TotalPlayers;
        public string ChampionId;
        public List<LeaderboardEntry> TopPlayers = new List<LeaderboardEntry>();
        public Dictionary<string, object> SeasonHighlights = new Dictionary<string, object>();
        public Dictionary<string, float> AverageRatings = new Dictionary<string, float>();
        public int TotalGamesPlayed;
        public List<SeasonAchievement> SeasonAchievements = new List<SeasonAchievement>();
    }
    
    [Serializable]
    public class SeasonMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public float ProgressRequired;
        public float CurrentProgress;
        public bool IsCompleted;
        public DateTime? CompletionDate;
        public List<string> Rewards = new List<string>();
    }
    
    [Serializable]
    public class SeasonAchievement
    {
        public string AchievementId;
        public string PlayerId;
        public string PlayerName;
        public string AchievementName;
        public string Description;
        public DateTime EarnedDate;
        public Dictionary<string, object> AchievementData = new Dictionary<string, object>();
    }
    
    // System metrics and reporting
    [Serializable]
    public class LeaderboardSystemMetrics
    {
        public int TotalPlayers;
        public int ActivePlayers;
        public int RankedPlayers;
        public int ProvisionalPlayers;
        public float AverageRating;
        public float RatingStandardDeviation;
        public int TotalRankingUpdates;
        public int CurrentSeason;
        public float SeasonProgress;
        public Dictionary<string, int> RegionalDistribution = new Dictionary<string, int>();
        public Dictionary<RankTier, int> TierDistribution = new Dictionary<RankTier, int>();
        public DateTime LastUpdateTime;
        public float SystemHealth; // 0-1 score indicating system performance
        public List<string> SystemAlerts = new List<string>();
    }
    
    // Competition and rivalry tracking
    [Serializable]
    public class PlayerRivalry
    {
        public string Player1Id;
        public string Player2Id;
        public int Player1Wins;
        public int Player2Wins;
        public int TotalMatches;
        public DateTime LastMatch;
        public float RatingGap;
        public List<RivalryMatch> MatchHistory = new List<RivalryMatch>();
        public string CurrentLeader;
        public bool IsActiveRivalry;
    }
    
    [Serializable]
    public class RivalryMatch
    {
        public DateTime MatchDate;
        public string WinnerId;
        public float WinnerScore;
        public float LoserScore;
        public string TournamentId;
        public TournamentType TournamentType;
    }
    
    // Leaderboard events and notifications
    [Serializable]
    public class LeaderboardEvent
    {
        public string EventId;
        public string EventType; // "RankChange", "NewRecord", "TierPromotion", etc.
        public string PlayerId;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public bool IsPublic;
        public string Description;
        public int Priority; // 1 = Low, 5 = Critical
    }
    
    [Serializable]
    public class RankingNotification
    {
        public string NotificationId;
        public string PlayerId;
        public string NotificationType;
        public string Title;
        public string Message;
        public DateTime CreatedTime;
        public bool IsRead;
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
        public string IconUrl;
        public bool RequiresAction;
    }
    
    // Hall of Fame and records
    [Serializable]
    public class HallOfFameEntry
    {
        public string PlayerId;
        public string PlayerName;
        public string RecordType;
        public string RecordDescription;
        public float RecordValue;
        public DateTime RecordDate;
        public int SeasonNumber;
        public bool IsCurrentRecord;
        public string TournamentId;
        public Dictionary<string, object> RecordDetails = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class LeaderboardRecord
    {
        public string RecordId;
        public string RecordName;
        public string PlayerId;
        public string PlayerName;
        public float Value;
        public DateTime Date;
        public string Category;
        public bool IsGlobalRecord;
        public bool IsSeasonRecord;
        public Dictionary<string, object> Context = new Dictionary<string, object>();
    }
    
    // Analytics and insights
    [Serializable]
    public class RankingAnalytics
    {
        public DateTime AnalysisDate;
        public Dictionary<string, float> RatingDistribution = new Dictionary<string, float>();
        public Dictionary<string, float> ActivityPatterns = new Dictionary<string, float>();
        public Dictionary<string, float> PerformanceTrends = new Dictionary<string, float>();
        public List<string> Insights = new List<string>();
        public Dictionary<string, object> MetricsSnapshot = new Dictionary<string, object>();
        public float SystemStability;
        public float CompetitiveBalance;
    }
    
    // Tournaments and special events impact on rankings
    [Serializable]
    public class RankingEvent
    {
        public string EventId;
        public string EventName;
        public DateTime StartDate;
        public DateTime EndDate;
        public float RatingMultiplier;
        public bool AffectsRankings;
        public Dictionary<string, object> SpecialRules = new Dictionary<string, object>();
        public List<string> ParticipantIds = new List<string>();
        public Dictionary<string, float> EventResults = new Dictionary<string, float>();
    }
}