using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.IPM
{
    #region Missing Enums for Architecture Types
    
    public enum IPMWeaponType
    {
        Organic,
        Chemical,
        Biological,
        Physical,
        Ultimate
    }
    
    #endregion
    
    #region System Configuration Types
    
    [Serializable]
    public class IPMSystemConfiguration
    {
        public IPMBattleConfiguration BattleConfig;
        public IPMProgressionConfiguration ProgressionConfig;
        public IPMAnalyticsConfiguration AnalyticsConfig;
        public IPMTournamentConfiguration TournamentConfig;
        public IPMNotificationConfiguration NotificationConfig;
    }
    
    [Serializable]
    public class IPMBattleConfiguration
    {
        public int MaxConcurrentBattles = 5;
        public bool EnableAdvancedBattles = true;
        public float BattleUpdateInterval = 0.1f;
        public bool EnableBattleLogging = true;
    }
    
    [Serializable]
    public class IPMProgressionConfiguration
    {
        public bool EnableProgressionSystem = true;
        public int MaxHistoryEntries = 100;
        public float ExperienceMultiplier = 1.0f;
        public int MaxPlayerLevel = 100;
    }
    
    [Serializable]
    public class IPMAnalyticsConfiguration
    {
        public bool EnableAnalytics = true;
        public int DataRetentionDays = 30;
        public bool EnableRealTimeAnalytics = true;
        public float AnalyticsUpdateInterval = 5.0f;
    }
    
    [Serializable]
    public class IPMTournamentConfiguration
    {
        public bool EnableTournaments = true;
        public int MaxTournamentParticipants = 32;
        public float TournamentDurationHours = 24.0f;
        public bool EnableRankedTournaments = true;
    }
    
    [Serializable]
    public class IPMNotificationConfiguration
    {
        public bool EnableNotifications = true;
        public float NotificationCooldownSeconds = 5.0f;
        public int MaxNotificationsPerMinute = 10;
        public bool EnablePushNotifications = false;
    }
    
    #endregion
    
    #region Battle System Types
    
    [Serializable]
    public class IPMBattleRequest
    {
        public string PlayerId;
        public string BattleMode;
        public int Difficulty;
        public List<string> SelectedWeapons = new List<string>();
        public List<string> SelectedStrategies = new List<string>();
        public Dictionary<string, object> CustomParameters = new Dictionary<string, object>();
    }
    
    
    [Serializable]
    public class IPMEnhancedBattleSession
    {
        public string BattleId;
        public string PlayerId;
        public string BattleMode;
        public int Difficulty;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        public bool IsCompleted;
        public IPMBattleState CurrentState;
        public List<IPMPestEnemy> EnemyPests = new List<IPMPestEnemy>();
        public List<string> WeaponsUsed = new List<string>();
        public List<string> StrategiesUsed = new List<string>();
        public float PlayerHealth = 100f;
        public float CurrentScore = 0f;
        public int TurnCount = 0;
        public Dictionary<string, object> SessionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class IPMBattleActionRequest
    {
        public string BattleId;
        public string PlayerId;
        public IPMBattleActionType ActionType;
        public string TargetId;
        public string WeaponId;
        public string StrategyId;
        public Dictionary<string, object> ActionParameters = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class IPMBattleActionResult
    {
        public string ActionId;
        public bool IsSuccessful;
        public float DamageDealt;
        public float ScoreGained;
        public bool IsCriticalHit;
        public bool IsBattleCompleted;
        public IPMBattleResult BattleResult;
        public List<string> StatusEffects = new List<string>();
        public string ResultMessage;
        public Dictionary<string, object> ResultData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class IPMBattleResult
    {
        public string BattleId;
        public string PlayerId;
        public IPMBattleOutcome Outcome;
        public float FinalScore;
        public int PestsDefeated;
        public TimeSpan BattleDuration;
        public List<string> WeaponsUsed = new List<string>();
        public List<string> StrategiesUsed = new List<string>();
        public List<IPMBattleAchievement> AchievementsEarned = new List<IPMBattleAchievement>();
        public DateTime CompletedAt;
        public Dictionary<string, float> BattleStatistics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMBattleValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public List<string> ValidationWarnings = new List<string>();
    }
    
    public enum IPMBattleState
    {
        Initializing,
        WaitingForPlayer,
        InProgress,
        PlayerTurn,
        EnemyTurn,
        Paused,
        Completed,
        Cancelled,
        Error
    }
    
    public enum IPMBattleActionType
    {
        Attack,
        UseStrategy,
        UseItem,
        Defend,
        Retreat,
        Special,
        Wait
    }
    
    public enum IPMBattleOutcome
    {
        Victory,
        Defeat,
        Draw,
        Timeout,
        Cancelled,
        Error
    }
    
    #endregion
    
    #region Player System Types
    
    [Serializable]
    public class IPMEnhancedPlayerProfile
    {
        public string PlayerId;
        public string PlayerName;
        public int Level;
        public int Experience;
        public int ExperienceToNextLevel;
        public int TotalBattlesWon;
        public int TotalBattlesLost;
        public int TotalPestsDefeated;
        public IPMWeaponType FavoriteWeaponType;
        public string PreferredBattleMode;
        public DateTime CreatedAt;
        public DateTime LastActiveAt;
        public List<string> UnlockedWeapons = new List<string>();
        public List<string> UnlockedStrategies = new List<string>();
        public List<string> Achievements = new List<string>();
        public Dictionary<string, float> Statistics = new Dictionary<string, float>();
        public IPMPlayerRank CurrentRank;
        public int RankingPoints;
    }
    
    [Serializable]
    public class IPMProgressionUpdate
    {
        public string PlayerId;
        public IPMBattleResult BattleResult;
        public int ExperienceGained;
        public List<string> WeaponsUsed = new List<string>();
        public List<string> StrategiesUsed = new List<string>();
        public List<string> NewAchievements = new List<string>();
        public DateTime UpdatedAt;
    }
    
    [Serializable]
    public class IPMProgressionResult
    {
        public bool LeveledUp;
        public int PreviousLevel;
        public int NewLevel;
        public int ExperienceGained;
        public List<string> UnlockedContent = new List<string>();
        public List<string> NewAchievements = new List<string>();
        public IPMPlayerRank NewRank;
    }
    
    [Serializable]
    public class IPMPlayerAchievement
    {
        public string AchievementId;
        public string PlayerId;
        public string AchievementType;
        public string Title;
        public string Description;
        public DateTime UnlockedAt;
        public Dictionary<string, object> AchievementData = new Dictionary<string, object>();
    }
    
    public enum IPMPlayerRank
    {
        Novice,
        Apprentice,
        Journeyman,
        Expert,
        Master,
        Grandmaster,
        Legend
    }
    
    #endregion
    
    #region Tournament System Types
    
    [Serializable]
    public class IPMTournamentRequest
    {
        public string TournamentName;
        public string CreatorId;
        public IPMTournamentType TournamentType;
        public int MaxParticipants;
        public DateTime StartTime;
        public TimeSpan Duration;
        public List<string> AllowedBattleModes = new List<string>();
        public Dictionary<string, object> TournamentRules = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class IPMTournament
    {
        public string TournamentId;
        public string TournamentName;
        public string CreatorId;
        public IPMTournamentType TournamentType;
        public IPMTournamentStatus Status;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public int MaxParticipants;
        public List<string> Participants = new List<string>();
        public List<IPMTournamentMatch> Matches = new List<IPMTournamentMatch>();
        public List<IPMTournamentRanking> Rankings = new List<IPMTournamentRanking>();
        public Dictionary<string, object> TournamentData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class IPMTournamentMatch
    {
        public string MatchId;
        public string TournamentId;
        public string Player1Id;
        public string Player2Id;
        public string WinnerId;
        public IPMTournamentMatchStatus Status;
        public DateTime ScheduledTime;
        public DateTime CompletedTime;
        public List<string> BattleIds = new List<string>();
        public Dictionary<string, float> MatchStatistics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMTournamentRanking
    {
        public string PlayerId;
        public string PlayerName;
        public int Position;
        public int Wins;
        public int Losses;
        public float Score;
        public Dictionary<string, float> DetailedStats = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMTournamentUpdate
    {
        public string TournamentId;
        public string UpdateType;
        public string Message;
        public DateTime Timestamp;
        public Dictionary<string, object> UpdateData = new Dictionary<string, object>();
    }
    
    public enum IPMTournamentType
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Swiss,
        KingOfTheHill,
        BattleRoyale
    }
    
    public enum IPMTournamentStatus
    {
        Created,
        Registering,
        Starting,
        InProgress,
        Completed,
        Cancelled,
        Paused
    }
    
    public enum IPMTournamentMatchStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        NoShow
    }
    
    #endregion
    
    #region Leaderboard System Types
    
    [Serializable]
    public class IPMLeaderboardQuery
    {
        public IPMLeaderboardType LeaderboardType;
        public IPMLeaderboardTimeframe Timeframe;
        public int MaxResults = 50;
        public int Offset = 0;
        public string PlayerId; // Optional - for finding player's rank
    }
    
    [Serializable]
    public class IPMLeaderboardResult
    {
        public IPMLeaderboardType LeaderboardType;
        public IPMLeaderboardTimeframe Timeframe;
        public List<IPMLeaderboardEntry> Players = new List<IPMLeaderboardEntry>();
        public IPMLeaderboardEntry PlayerRank; // Player's own ranking if requested
        public DateTime LastUpdated;
    }
    
    public enum IPMLeaderboardType
    {
        OverallRanking,
        BattlesWon,
        PestsDefeated,
        TournamentWins,
        ExperiencePoints,
        WinRate,
        AverageScore,
        LongestWinStreak
    }
    
    public enum IPMLeaderboardTimeframe
    {
        AllTime,
        ThisWeek,
        ThisMonth,
        ThisSeason,
        Today
    }
    
    #endregion
    
    #region Analytics System Types
    
    [Serializable]
    public class IPMPlayerAnalytics
    {
        public string PlayerId;
        public int TotalGamesPlayed;
        public int TotalGamesWon;
        public float WinRate;
        public float AverageScore;
        public int TotalPestsDefeated;
        public TimeSpan TotalPlayTime;
        public Dictionary<string, int> WeaponUsage = new Dictionary<string, int>();
        public Dictionary<string, int> StrategyUsage = new Dictionary<string, int>();
        public Dictionary<string, float> BattleModePerformance = new Dictionary<string, float>();
        public List<IPMPerformanceTrend> PerformanceTrends = new List<IPMPerformanceTrend>();
        public DateTime LastUpdated;
    }
    
    [Serializable]
    public class IPMSystemAnalytics
    {
        public int TotalRegisteredPlayers;
        public int ActivePlayersToday;
        public int TotalBattlesPlayed;
        public int ActiveBattlesNow;
        public float AverageSessionDuration;
        public Dictionary<string, int> PopularBattleModes = new Dictionary<string, int>();
        public Dictionary<string, int> PopularWeapons = new Dictionary<string, int>();
        public List<IPMSystemTrend> SystemTrends = new List<IPMSystemTrend>();
        public DateTime LastUpdated;
    }
    
    [Serializable]
    public class IPMPerformanceTrend
    {
        public DateTime Date;
        public float WinRate;
        public float AverageScore;
        public int GamesPlayed;
    }
    
    [Serializable]
    public class IPMSystemTrend
    {
        public DateTime Date;
        public int ActivePlayers;
        public int BattlesPlayed;
        public float AverageSessionDuration;
    }
    
    #endregion
    
    #region Event System Types
    
    [Serializable]
    public class IPMBattleStartedEvent
    {
        public string BattleId;
        public string PlayerId;
        public string BattleMode;
        public int Difficulty;
        public DateTime StartTime;
    }
    
    [Serializable]
    public class IPMBattleCompletedEvent
    {
        public string BattleId;
        public string PlayerId;
        public IPMBattleResult BattleResult;
        public DateTime CompletedAt;
    }
    
    [Serializable]
    public class IPMPlayerLevelUpEvent
    {
        public string PlayerId;
        public int PreviousLevel;
        public int NewLevel;
        public List<string> UnlockedContent = new List<string>();
        public DateTime LevelUpTime;
    }
    
    [Serializable]
    public class IPMTournamentStartedEvent
    {
        public string TournamentId;
        public string TournamentName;
        public int ParticipantCount;
        public DateTime StartTime;
    }
    
    [Serializable]
    public class IPMSystemErrorEvent
    {
        public string ErrorId;
        public string ErrorMessage;
        public string StackTrace;
        public IPMErrorSeverity Severity;
        public string Component;
        public DateTime ErrorTime;
    }
    
    public enum IPMErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    #endregion
    
    #region System Health and Monitoring Types
    
    [Serializable]
    public class IPMEnhancedSystemMetrics
    {
        public DateTime SystemStartTime = DateTime.Now;
        public DateTime LastUpdateTime = DateTime.Now;
        public TimeSpan SystemUptime;
        public int ActiveBattlesCount;
        public int TotalPlayersCount;
        public int TotalBattlesStarted;
        public int TotalBattlesCompleted;
        public int TotalLevelUps;
        public int TotalTournamentsStarted;
        public int TotalErrors;
        public Dictionary<string, float> PerformanceCounters = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMSystemHealthCheck
    {
        public DateTime Timestamp;
        public IPMSystemHealth OverallHealth;
        public Dictionary<string, bool> ComponentStatuses = new Dictionary<string, bool>();
        public List<string> ErrorMessages = new List<string>();
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMSystemAlert
    {
        public string AlertType;
        public string Message;
        public IPMErrorSeverity Severity;
        public DateTime Timestamp;
        public Dictionary<string, object> AlertData = new Dictionary<string, object>();
    }
    
    public enum IPMSystemHealth
    {
        Healthy,
        Degraded,
        Critical,
        Unknown
    }
    
    #endregion
    
    #region Combat and Game Mechanics Types
    
    [Serializable]
    public class IPMPestEnemy
    {
        public string PestId;
        public string PestName;
        public PestType PestType;
        public int DifficultyLevel;
        public float MaxHealth;
        public float CurrentHealth;
        public float AttackPower;
        public float DefensePower;
        public List<string> Abilities = new List<string>();
        public List<IPMWeaponType> Weaknesses = new List<IPMWeaponType>();
        public List<IPMWeaponType> Resistances = new List<IPMWeaponType>();
        public List<string> StatusEffects = new List<string>();
        public bool IsDefeated;
        public Dictionary<string, float> CombatStats = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class IPMWeaponDefinition
    {
        public string WeaponId;
        public string WeaponName;
        public IPMWeaponType WeaponType;
        public float BaseDamage;
        public float CriticalChance;
        public int RequiredLevel;
        public string Description;
        public List<string> EffectiveAgainst = new List<string>();
        public List<string> SpecialEffects = new List<string>();
        public Dictionary<string, float> WeaponStats = new Dictionary<string, float>();
        public bool IsUnlocked;
    }
    
    [Serializable]
    public class IPMStrategyDefinition
    {
        public string StrategyId;
        public string StrategyName;
        public string Description;
        public float AttackModifier;
        public float DefenseModifier;
        public float CriticalModifier;
        public TimeSpan Duration;
        public TimeSpan Cooldown;
        public int RequiredLevel;
        public List<string> RequiredWeapons = new List<string>();
        public Dictionary<string, float> StrategyEffects = new Dictionary<string, float>();
        public bool IsUnlocked;
    }
    
    [Serializable]
    public class IPMBattleAchievement
    {
        public string AchievementId;
        public string Title;
        public string Description;
        public IPMBattleAchievementType Type;
        public Dictionary<string, object> Criteria = new Dictionary<string, object>();
        public int ExperienceReward;
        public List<string> UnlockRewards = new List<string>();
    }
    
    public enum IPMBattleAchievementType
    {
        FirstVictory,
        PestSlayer,
        Perfectionist,
        Strategist,
        SpeedRunner,
        Survivor,
        WeaponMaster,
        CriticalHitter,
        Combo,
        Endurance
    }
    
    #endregion
}