using UnityEngine;
using System.Collections.Generic;
using System;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Tournament data structures for breeding tournament systems.
    /// </summary>
    
    [System.Serializable]
    public class TournamentParticipant
    {
        public string PlayerId = "";
        public string PlayerName = "";
        public DateTime RegistrationTime = DateTime.Now;
        public int Ranking = 0;
        public float Score = 0f;
        public bool IsActive = true;
        public List<string> Submissions = new List<string>();
        public string TournamentId = "";
    }
    
    [System.Serializable]
    public class TournamentData
    {
        public string TournamentId = "";
        public string TournamentName = "";
        public string Description = "";
        public DateTime StartTime = DateTime.Now;
        public DateTime EndTime = DateTime.Now.AddDays(7);
        public int MaxParticipants = 50;
        public List<string> CurrentParticipants = new List<string>();
        public TournamentStatus Status = TournamentStatus.Active;
        public Dictionary<string, float> Rewards = new Dictionary<string, float>();
        public List<string> Rules = new List<string>();
        
        // Missing properties for Systems layer
        public TournamentType TournamentType = TournamentType.Weekly;
        public string DifficultyLevel = "";
        public string BreedingGoal = "";
        public DateTime RegistrationStartTime = DateTime.Now;
        public DateTime RegistrationEndTime = DateTime.Now.AddDays(1);
        public DateTime CompetitionStartTime = DateTime.Now.AddDays(1);
        public DateTime CompetitionEndTime = DateTime.Now.AddDays(8);
        public TournamentEntryRequirements EntryRequirements;
        public DateTime CreatedAt = DateTime.Now;
        public List<TournamentResult> Results = new List<TournamentResult>();
    }
    
    [System.Serializable]
    public class TournamentSubmission
    {
        public string SubmissionId = "";
        public string TournamentId = "";
        public string PlayerId = "";
        public string PlayerName = "";
        public DateTime SubmissionTime = DateTime.Now;
        public PlantStrainSO SubmittedStrain;
        public float Score = 0f;
        public bool IsValidated = false;
        public string Notes = "";
        public List<string> JudgeComments = new List<string>();
        public TournamentSubmissionData SubmissionData;
    }
    
    public enum TournamentStatus
    {
        Planned,
        Registration,
        Active,
        Judging,
        Completed,
        Cancelled
    }
    
    public enum TournamentType
    {
        Weekly,
        Championship,
        GrandPrix,
        Seasonal,
        Special
    }
    
    public enum TournamentDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public class BreedingGoalFactory
    {
        public static string CreateSimpleYieldGoal()
        {
            return "Maximize yield potential above 800g per plant";
        }
        
        public static string CreateQualityGoal()
        {
            return "Achieve THC content above 20% with balanced terpene profile";
        }
        
        public static string CreateBalancedGoal()
        {
            return "Balance yield, potency, and resistance traits";
        }
        
        // Missing methods for tournament manager
        public static string CreateHighTHCGoal()
        {
            return "Achieve maximum THC content above 25% with optimal potency";
        }
        
        public static string CreateBalancedCannabinoidGoal()
        {
            return "Achieve balanced THC:CBD ratio with diverse cannabinoid profile";
        }
        
        public static string CreateHighYieldGoal()
        {
            return "Maximize yield potential above 1000g per plant with quality retention";
        }
    }
    
    [System.Serializable]
    public class TournamentResult
    {
        public string TournamentId = "";
        public string PlayerId = "";
        public string PlayerName = "";
        public int FinalRanking = 0;
        public float FinalScore = 0f;
        public List<string> Achievements = new List<string>();
        public Dictionary<string, float> Rewards = new Dictionary<string, float>();
        public DateTime CompletionTime = DateTime.Now;
        
        // Missing properties for Systems layer
        public string TournamentName = "";
        public int TotalParticipants = 0;
        public int TotalSubmissions = 0;
        public List<TournamentRanking> Rankings = new List<TournamentRanking>();
    }
    
    [System.Serializable]
    public class TournamentConfiguration
    {
        public string ConfigurationId = "";
        public string TournamentName = "";
        public string Description = "";
        public int MaxParticipants = 50;
        public int MinParticipants = 5;
        public TimeSpan Duration = TimeSpan.FromDays(7);
        public List<string> Rules = new List<string>();
        public Dictionary<string, float> RewardStructure = new Dictionary<string, float>();
        public TournamentEntryRequirements EntryRequirements;
        
        // Missing properties for Systems layer
        public TournamentType TournamentType = TournamentType.Weekly;
        public string DifficultyLevel = "";
        public string BreedingGoal = "";
        public DateTime RegistrationStartTime = DateTime.Now;
        public DateTime RegistrationEndTime = DateTime.Now.AddDays(1);
        public DateTime CompetitionStartTime = DateTime.Now.AddDays(1);
        public DateTime CompetitionEndTime = DateTime.Now.AddDays(8);
    }
    
    [System.Serializable]
    public class TournamentSubmissionData
    {
        public string SubmissionId = "";
        public string TournamentId = "";
        public string PlayerId = "";
        public PlantStrainSO SubmittedStrain;
        public DateTime SubmissionTime = DateTime.Now;
        public Dictionary<string, float> EvaluationScores = new Dictionary<string, float>();
        public float TotalScore = 0f;
        public bool IsDisqualified = false;
        public string DisqualificationReason = "";
        public List<string> JudgeNotes = new List<string>();
        
        // Missing properties for Systems layer
        public PlantStrainSO ParentStrain;
        public PlantStrainSO Parent1Strain;
        public PlantStrainSO Parent2Strain;
        public bool IsInnovativeApproach = false;
    }
    
    [System.Serializable]
    public class TournamentReward
    {
        public string RewardId = "";
        public string RewardName = "";
        public string Description = "";
        public TournamentRewardTier Tier = TournamentRewardTier.Bronze;
        public float Value = 0f;
        public string RewardType = "";
        public bool IsExclusive = false;
        public int Quantity = 1;
        public string PlayerId = "";
        
        // Missing properties for Systems layer
        public string TierName = "";
        public float ReputationReward = 0f;
        public float CurrencyReward = 0f;
        public List<string> ExclusiveUnlocks = new List<string>();
    }
    
    [System.Serializable]
    public class TournamentRanking
    {
        public string TournamentId = "";
        public List<PlayerRankingData> Rankings = new List<PlayerRankingData>();
        public DateTime LastUpdated = DateTime.Now;
        public bool IsFinal = false;
        
        // Missing properties for Systems layer
        public int Rank = 0;
        public string PlayerId = "";
        public string PlayerName = "";
        public float Score = 0f;
        public TournamentSubmissionData SubmissionData;
    }
    
    [System.Serializable]
    public class TournamentEntryRequirements
    {
        public int MinimumLevel = 1;
        public float MinimumReputation = 0f;
        public List<string> RequiredAchievements = new List<string>();
        public bool RequiresPremiumMembership = false;
        public int EntryFee = 0;
        public List<string> RestrictedTraits = new List<string>();
    }
    
    [System.Serializable]
    public class PlayerRankingData
    {
        public string PlayerId = "";
        public string PlayerName = "";
        public int CurrentRank = 0;
        public int PreviousRank = 0;
        public float Score = 0f;
        public float Progress = 0f;
        public DateTime LastUpdate = DateTime.Now;
        public List<string> RecentAchievements = new List<string>();
        
        // Missing properties for Systems layer
        public int TotalTournaments = 0;
        public float TotalScore = 0f;
        public float AverageScore = 0f;
        public int Wins = 0;
        public int TopFivePlacements = 0;
        public DateTime LastTournamentDate = DateTime.Now;
        public int GlobalRank = 0;
    }
    
    [System.Serializable]
    public class TournamentStatistics
    {
        public string TournamentId = "";
        public int TotalParticipants = 0;
        public int ActiveParticipants = 0;
        public int TotalSubmissions = 0;
        public float AverageScore = 0f;
        public float HighestScore = 0f;
        public float LowestScore = 0f;
        public DateTime StartTime = DateTime.Now;
        public DateTime EndTime = DateTime.Now;
        public Dictionary<string, int> ParticipantsByRegion = new Dictionary<string, int>();
        public int TotalTournaments = 0;
    }
    
    public enum TournamentRewardTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Legend
    }
    
    [System.Serializable]
    public class TournamentRewardTierData
    {
        public TournamentRewardTier Tier = TournamentRewardTier.Bronze;
        public string TierName = "";
        public int MinimumRank = 1;
        public int MaximumRank = 10;
        public float ReputationReward = 0f;
        public float CurrencyReward = 0f;
        public List<string> ExclusiveUnlocks = new List<string>();
        public float Multiplier = 1f;
        public bool IsExclusive = false;
    }
}