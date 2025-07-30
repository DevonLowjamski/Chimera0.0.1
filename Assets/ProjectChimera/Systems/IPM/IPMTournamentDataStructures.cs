using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    // Tournament system enums
    public enum TournamentType
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Swiss,
        KingOfTheHill,
        LeaguePlay,
        Ladder,
        CustomBracket
    }
    
    public enum TournamentStatus
    {
        Planning,
        Registration,
        WaitingToStart,
        InProgress,
        Paused,
        Completed,
        Cancelled,
        Postponed
    }
    
    public enum ParticipantStatus
    {
        Registered,
        Confirmed,
        Active,
        Eliminated,
        Withdrawn,
        AdvancedByBye,
        Disqualified
    }
    
    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        Postponed,
        WaitingForPlayers,
        UnderReview
    }
    
    public enum MatchType
    {
        Elimination,
        GroupStage,
        Semifinal,
        Final,
        ThirdPlace,
        Qualifier,
        Regular
    }
    
    // Core tournament structures
    [Serializable]
    public class Tournament
    {
        public string TournamentId;
        public string Name;
        public string Description;
        public TournamentType TournamentType;
        public IPMDifficultyLevel DifficultyLevel;
        public int MaxParticipants;
        public float EntryFee;
        public float PrizePool;
        public DateTime RegistrationStartTime;
        public DateTime RegistrationEndTime;
        public DateTime StartTime;
        public DateTime EndTime;
        public TournamentStatus Status;
        public List<TournamentParticipant> Participants = new List<TournamentParticipant>();
        public List<TournamentMatch> Matches = new List<TournamentMatch>();
        public TournamentBracket Bracket;
        public Dictionary<string, object> Rules = new Dictionary<string, object>();
        public Dictionary<string, object> Restrictions = new Dictionary<string, object>();
        public string CreatedBy;
        public DateTime CreationTime;
        public int CurrentRound;
        public int TotalRounds;
        public Dictionary<string, float> PrizeDistribution = new Dictionary<string, float>();
        public List<string> Sponsors = new List<string>();
        public bool IsRanked = true;
    }
    
    [Serializable]
    public class TournamentConfiguration
    {
        public string Name;
        public string Description;
        public TournamentType TournamentType;
        public IPMDifficultyLevel DifficultyLevel;
        public int MaxParticipants;
        public float EntryFee;
        public float PrizePool;
        public Dictionary<string, object> Rules = new Dictionary<string, object>();
        public Dictionary<string, object> Restrictions = new Dictionary<string, object>();
        public string CreatedBy;
        public bool AllowSpectators = true;
        public bool IsPrivate = false;
        public string Password;
        public List<string> InvitedPlayers = new List<string>();
    }
    
    [Serializable]
    public class TournamentParticipant
    {
        public string ParticipantId;
        public string PlayerId;
        public string PlayerName;
        public int PlayerLevel;
        public float CurrentRating;
        public DateTime RegistrationTime;
        public ParticipantStatus Status;
        public int Seed;
        public int Wins;
        public int Losses;
        public float CurrentScore;
        public TournamentStatistics Statistics;
        public Dictionary<string, object> PlayerData = new Dictionary<string, object>();
        public List<string> Achievements = new List<string>();
        public float TotalPrizeMoney;
    }
    
    [Serializable]
    public class TournamentMatch
    {
        public string MatchId;
        public string TournamentId;
        public int Round;
        public int MatchNumber;
        public string Player1Id;
        public string Player2Id;
        public MatchStatus Status;
        public DateTime ScheduledTime;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan EstimatedDuration;
        public MatchType MatchType;
        public MatchResult Result;
        public string RefereeId;
        public List<string> SpectatorIds = new List<string>();
        public Dictionary<string, object> MatchData = new Dictionary<string, object>();
        public bool IsStreamed;
        public string StreamUrl;
    }
    
    [Serializable]
    public class MatchResult
    {
        public string ResultId;
        public string MatchId;
        public string WinnerId;
        public string LoserId;
        public float WinnerScore;
        public float LoserScore;
        public TimeSpan MatchDuration;
        public string ResultType; // "Normal", "Forfeit", "Disqualification", etc.
        public Dictionary<string, float> DetailedScores = new Dictionary<string, float>();
        public List<MatchPerformance> Performances = new List<MatchPerformance>();
        public string Notes;
        public DateTime ResultTime;
        public bool IsDisputed;
        public string DisputeReason;
    }
    
    [Serializable]
    public class MatchPerformance
    {
        public string PlayerId;
        public float FinalScore;
        public float CompletionTime;
        public float OptimalTime;
        public float ResourceEfficiency;
        public float InnovationScore;
        public float EnvironmentalScore;
        public float PenaltyMultiplier;
        public IPMDifficultyLevel DifficultyLevel;
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public List<string> StrategiesUsed = new List<string>();
        public int PestsEliminated;
        public float AccuracyRating;
        public float SpeedRating;
    }
    
    // Player information and ratings
    [Serializable]
    public class PlayerInfo
    {
        public string PlayerId;
        public string PlayerName;
        public int PlayerLevel;
        public string Country;
        public string Region;
        public DateTime JoinDate;
        public bool IsVerified;
        public Dictionary<string, object> Profile = new Dictionary<string, object>();
        public List<string> Preferences = new List<string>();
    }
    
    [Serializable]
    public class PlayerRating
    {
        public string PlayerId;
        public float CurrentRating;
        public float PeakRating;
        public float LowestRating;
        public int TournamentCount;
        public int WinCount;
        public int LossCount;
        public float WinRate;
        public DateTime LastUpdated;
        public List<RatingHistory> RatingHistory = new List<RatingHistory>();
        public Dictionary<TournamentType, float> TypeRatings = new Dictionary<TournamentType, float>();
        public int CurrentStreak;
        public int LongestWinStreak;
    }
    
    [Serializable]
    public class RatingHistory
    {
        public DateTime Date;
        public float Rating;
        public float Change;
        public string TournamentId;
        public string Reason;
    }
    
    // Tournament bracket and structure
    [Serializable]
    public class TournamentBracket
    {
        public string BracketId;
        public TournamentType BracketType;
        public List<BracketRound> Rounds = new List<BracketRound>();
        public Dictionary<string, BracketPosition> PlayerPositions = new Dictionary<string, BracketPosition>();
        public bool IsFinalized;
        public DateTime CreationTime;
        public string CreatedBy;
    }
    
    [Serializable]
    public class BracketRound
    {
        public int RoundNumber;
        public string RoundName;
        public List<string> MatchIds = new List<string>();
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsCompleted;
        public int ParticipantCount;
    }
    
    [Serializable]
    public class BracketPosition
    {
        public string PlayerId;
        public int Seed;
        public int CurrentRound;
        public string CurrentMatchId;
        public bool IsEliminated;
        public int FinalPlacement;
    }
    
    // Statistics and reporting
    [Serializable]
    public class TournamentStatistics
    {
        public int MatchesPlayed;
        public int MatchesWon;
        public int MatchesLost;
        public float AverageScore;
        public float HighestScore;
        public float TotalScore;
        public TimeSpan TotalPlayTime;
        public TimeSpan AverageMatchTime;
        public int PestsEliminatedTotal;
        public float AverageEfficiency;
        public Dictionary<StrategyType, int> StrategiesUsed = new Dictionary<StrategyType, int>();
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
        public List<string> AchievementsEarned = new List<string>();
    }
    
    [Serializable]
    public class TournamentResult
    {
        public string PlayerId;
        public string TournamentId;
        public int Placement;
        public int TotalParticipants;
        public float FinalScore;
        public int Wins;
        public int Losses;
        public float PrizeMoney;
        public DateTime TournamentEndTime;
        public List<string> AchievementsEarned = new List<string>();
        public Dictionary<string, float> CategoryRankings = new Dictionary<string, float>();
        public bool IsPersonalBest;
    }
    
    [Serializable]
    public class TournamentHistory
    {
        public string TournamentId;
        public string TournamentName;
        public TournamentType TournamentType;
        public DateTime StartTime;
        public DateTime EndTime;
        public int TotalParticipants;
        public List<TournamentResult> Results = new List<TournamentResult>();
        public string ChampionId;
        public float TotalPrizePool;
        public Dictionary<string, object> Highlights = new Dictionary<string, object>();
    }
    
    // Reporting and information structures
    [Serializable]
    public class TournamentInfo
    {
        public Tournament Tournament;
        public List<TournamentParticipant> CurrentStandings = new List<TournamentParticipant>();
        public List<TournamentMatch> NextMatches = new List<TournamentMatch>();
        public List<TournamentMatch> CompletedMatches = new List<TournamentMatch>();
        public TournamentStatistics Statistics;
        public Dictionary<string, object> AdditionalInfo = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class PlayerTournamentRecord
    {
        public string PlayerId;
        public int TotalTournaments;
        public int TotalWins;
        public int TotalLosses;
        public int BestPlacement;
        public float AverageScore;
        public StrategyType FavoriteStrategy;
        public List<TournamentResult> RecentForm = new List<TournamentResult>();
        public Dictionary<TournamentType, int> TypeParticipation = new Dictionary<TournamentType, int>();
        public float TotalEarnings;
        public List<string> Titles = new List<string>();
    }
    
    [Serializable]
    public class TournamentSystemMetrics
    {
        public int TotalTournamentsHosted;
        public int ActiveTournaments;
        public int TotalRegisteredPlayers;
        public int TotalMatches;
        public float AverageTournamentDuration;
        public Dictionary<TournamentType, int> TournamentTypeDistribution = new Dictionary<TournamentType, int>();
        public float AverageParticipantsPerTournament;
        public DateTime LastUpdateTime;
        public Dictionary<string, float> SystemPerformance = new Dictionary<string, float>();
        public float PlayerSatisfactionScore;
        public int ActiveSpectators;
    }
    
    // Supporting engine classes
    [Serializable]
    public class ScoringEngine
    {
        public float BaseScoreMultiplier;
        public float DifficultyMultiplier;
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
        public DateTime LastCalibration;
        
        public ScoringEngine(float baseMultiplier, float difficultyMultiplier)
        {
            BaseScoreMultiplier = baseMultiplier;
            DifficultyMultiplier = difficultyMultiplier;
            LastCalibration = DateTime.Now;
        }
        
        public float CalculateScore(MatchPerformance performance)
        {
            // Placeholder scoring calculation
            return performance.FinalScore * BaseScoreMultiplier;
        }
    }
    
    [Serializable]
    public class RankingSystem
    {
        public string SystemId;
        public Dictionary<string, float> RankingFactors = new Dictionary<string, float>();
        public DateTime LastUpdate;
        
        public List<PlayerRating> GetTopPlayers(int count)
        {
            // Placeholder implementation
            return new List<PlayerRating>();
        }
    }
    
    [Serializable]
    public class MatchmakingEngine
    {
        public string EngineId;
        public Dictionary<string, float> MatchmakingWeights = new Dictionary<string, float>();
        public float RatingVarianceThreshold = 200.0f;
        
        public List<TournamentMatch> GenerateMatches(List<TournamentParticipant> participants)
        {
            // Placeholder implementation
            return new List<TournamentMatch>();
        }
    }
    
    [Serializable]
    public class TournamentBracketGenerator
    {
        public string GeneratorId;
        public Dictionary<TournamentType, BracketTemplate> Templates = new Dictionary<TournamentType, BracketTemplate>();
        
        public TournamentBracket GenerateBracket(TournamentType type, int maxParticipants)
        {
            return new TournamentBracket
            {
                BracketId = Guid.NewGuid().ToString(),
                BracketType = type,
                IsFinalized = false,
                CreationTime = DateTime.Now
            };
        }
    }
    
    [Serializable]
    public class BracketTemplate
    {
        public TournamentType TournamentType;
        public int MinParticipants;
        public int MaxParticipants;
        public int RoundCount;
        public Dictionary<string, object> Configuration = new Dictionary<string, object>();
    }
    
    // Event and notification structures
    [Serializable]
    public class TournamentEvent
    {
        public string EventId;
        public string TournamentId;
        public string EventType;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public List<string> AffectedPlayers = new List<string>();
        public string Description;
        public bool IsPublic = true;
    }
    
    [Serializable]
    public class TournamentNotification
    {
        public string NotificationId;
        public string TournamentId;
        public string PlayerId;
        public string NotificationType;
        public string Title;
        public string Message;
        public DateTime CreatedTime;
        public bool IsRead;
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
        public int Priority; // 1 = Low, 5 = Critical
    }
    
    // Prize and reward structures
    [Serializable]
    public class PrizeStructure
    {
        public string PrizeId;
        public string TournamentId;
        public Dictionary<int, Prize> PlacementPrizes = new Dictionary<int, Prize>(); // Placement -> Prize
        public List<Achievement> SpecialPrizes = new List<Achievement>();
        public float TotalValue;
        public string Currency;
        public bool IsDistributed;
    }
    
    [Serializable]
    public class Prize
    {
        public string PrizeId;
        public string Name;
        public string Description;
        public float MonetaryValue;
        public Dictionary<string, object> Items = new Dictionary<string, object>();
        public bool IsTransferable;
        public DateTime ExpiryDate;
    }
    
    [Serializable]
    public class Achievement
    {
        public string AchievementId;
        public string Name;
        public string Description;
        public string IconPath;
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
        public Prize Reward;
        public bool IsSecret;
        public int Points;
    }
}