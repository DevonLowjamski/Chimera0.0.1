using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Competition
{
    /// <summary>
    /// Comprehensive competition data structures for decomposed services
    /// </summary>

    #region Core Competition Classes

    [System.Serializable]
    public class Competition
    {
        public string CompetitionId;
        public string Name;
        public CompetitionType Type;
        public CompetitionTier Tier;
        public DateTime StartDate;
        public DateTime EndDate;
        public DateTime JudgingDeadline;
        public List<CompetitionCategory> Categories = new List<CompetitionCategory>();
        public List<string> Sponsors = new List<string>();
        public CompetitionRules Rules;
        public List<CompetitionEntry> Entries = new List<CompetitionEntry>();
        public CompetitionResults Results;
        public bool IsActive;
        public bool AcceptingEntries;
        public float EntryFee;
        public CompetitionRewards Rewards;
        public string Location;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class CompetitionCategory
    {
        public string CategoryId;
        public string Name;
        public string Description;
        public CategoryType Type;
        public List<string> RequiredCriteria = new List<string>();
        public int MaxEntries;
        public bool IsActive = true;
    }

    [System.Serializable]
    public class CompetitionRules
    {
        public int MaxPlantsPerEntry = 1;
        public List<string> RequiredDocumentation = new List<string>();
        public List<string> JudgingCriteria = new List<string>();
        public List<string> DisqualificationReasons = new List<string>();
        public bool AllowMultipleEntries = false;
        public int MinimumPlantAge = 60;
        public float MinimumYield = 10f;
    }

    [System.Serializable]
    public class CompetitionEntry
    {
        public string EntryId;
        public string CompetitionId;
        public string CompetitorId;
        public string CategoryId;
        public DateTime SubmissionDate;
        public PlantSubmission PlantData;
        public EntryDocumentation Documentation;
        public List<string> PhotoUrls = new List<string>();
        public EntryStatus Status;
        public float EntryScore;
        public List<JudgeScore> Scores = new List<JudgeScore>();
        public string DisqualificationReason;
        public bool IsEligible = true;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public string RegistrationId;
        public string PlayerId;
        public string PlantId;
    }

    [System.Serializable]
    public class CompetitionResults
    {
        public string CompetitionId;
        public DateTime CalculationDate;
        public List<PlantRanking> Rankings = new List<PlantRanking>();
        public Dictionary<string, string> CategoryWinners = new Dictionary<string, string>();
        public string OverallWinner;
        public bool IsFinalized;
        public bool IsValidated;
    }

    [System.Serializable]
    public class CompetitionRewards
    {
        public List<Prize> Prizes = new List<Prize>();
        public Dictionary<int, float> PlacementRewards = new Dictionary<int, float>();
        public List<string> AchievementIds = new List<string>(); // Reference achievements by ID instead
    }

    [System.Serializable]
    public class CompetitionFormat
    {
        public string FormatId;
        public string Name;
        public string Description;
        public TimeSpan Duration;
        public int MaxEntries;
        public List<string> Categories = new List<string>();
        public bool IsActive = true;
    }

    #endregion

    #region Plant and Submission Classes

    [System.Serializable]
    public class PlantSubmission
    {
        public string StrainName;
        public string GeneticLineage;
        public CultivationMethod Method;
        public float TotalYield;
        public float QualityScore;
        public CannabinoidProfile Cannabinoids;
        public TerpeneProfile Terpenes;
        public VisualQuality Visual;
        public AromaProfile Aroma;
        public DateTime HarvestDate;
        public int DaysToHarvest;
        public List<string> CultivationNotes = new List<string>();
    }

    [System.Serializable]
    public class EntryDocumentation
    {
        public List<string> PhotoUrls = new List<string>();
        public List<string> DocumentUrls = new List<string>();
        public string GrowLog;
        public string NutrientSchedule;
        public List<string> TestResults = new List<string>();
        public bool IsComplete;
    }

    [System.Serializable]
    public class CannabinoidProfile
    {
        public float THC;
        public float CBD;
        public float CBG;
        public float CBN;
        public float TotalCannabinoids;
        public DateTime TestDate;
        public string TestLab;
    }

    [System.Serializable]
    public class TerpeneProfile
    {
        public float Myrcene;
        public float Limonene;
        public float Pinene;
        public float Linalool;
        public float Caryophyllene;
        public float TotalTerpenes;
        public DateTime TestDate;
        public string TestLab;
    }

    [System.Serializable]
    public class VisualQuality
    {
        public float TrichomeData;
        public float ColorQuality;
        public float StructuralIntegrity;
        public float TrimQuality;
        public float OverallAppearance;
    }

    [System.Serializable]
    public class AromaProfile
    {
        public float Intensity;
        public float Complexity;
        public float Appeal;
        public List<string> AromaNotes = new List<string>();
    }

    #endregion

    #region Judging Classes

    [System.Serializable]
    public class Judge
    {
        public string JudgeId;
        public string Name;
        public bool IsCertified;
        public int YearsExperience;
        public int CompetitionsJudged;
        public List<string> AssignedCompetitions = new List<string>();
        public int MaxConcurrentCompetitions = 3;
        public float ReliabilityScore = 1.0f;
    }

    [System.Serializable]
    public class JudgingSession
    {
        public string SessionId;
        public string CompetitionId;
        public string JudgeId;
        public List<string> EntriesToJudge = new List<string>();
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsCompleted;
    }

    [System.Serializable]
    public class JudgingCriteria
    {
        public float VisualWeight = 0.25f;
        public float AromaWeight = 0.25f;
        public float PotencyWeight = 0.30f;
        public float OverallWeight = 0.20f;
        public float MaxScore = 100f;
        public bool RequiredCertification = true;
    }

    [System.Serializable]
    public class JudgeScore
    {
        public string JudgeId;
        public string PlantId;
        public float VisualScore;
        public float AromaScore;
        public float PotencyScore;
        public float OverallScore;
        public float TotalScore;
        public string Comments;
        public DateTime SubmissionTime;
        public bool IsValidated;
    }

    [System.Serializable]
    public class JudgeScorecard
    {
        public float VisualScore;
        public float AromaScore;
        public float PotencyScore;
        public float OverallScore;
        public string Comments;
    }

    [System.Serializable]
    public class ScoreBreakdown
    {
        public string PlantId;
        public string JudgeId;
        public float VisualScore;
        public float AromaScore;
        public float PotencyScore;
        public float OverallScore;
        public float TotalScore;
        public string Comments;
        public DateTime SubmissionTime;
    }

    [System.Serializable]
    public class PlantRanking
    {
        public string PlantId;
        public int Rank;
        public float Score;
        public string CompetitionId;
    }

    [System.Serializable]
    public class WinnerSelection
    {
        public string CompetitionId;
        public string OverallWinner;
        public Dictionary<string, string> CategoryWinners = new Dictionary<string, string>();
        public List<string> TopThree = new List<string>();
        public DateTime SelectionDate;
    }

    #endregion

    #region Registration Classes

    [System.Serializable]
    public class ParticipantRegistration
    {
        public string RegistrationId;
        public string PlayerId;
        public string CompetitionId;
        public DateTime RegistrationDate;
        public RegistrationStatus Status;
        public PlantSubmission PlantSubmission;
        public bool IsValidated;
        public DateTime ValidationDate;
        public List<string> ValidationErrors = new List<string>();
        public string EntryId;
        public List<string> NotificationHistory = new List<string>();
    }

    [System.Serializable]
    public class ParticipantProfile
    {
        public string PlayerId;
        public DateTime CreatedDate;
        public int CompetitionsEntered;
        public int SkillLevel;
        public int Age;
        public string Region;
        public DateTime LastCompetitionDate;
        public List<string> CompetitionHistory = new List<string>();
        public List<string> Certifications = new List<string>();
    }

    [System.Serializable]
    public class CompetitionRequirements
    {
        public int MinimumCompetitions;
        public int MinimumSkillLevel;
        public int MinimumAge;
        public bool RegionRestricted;
        public List<string> EligibleRegions = new List<string>();
        public List<string> RequiredCertifications = new List<string>();
    }

    [System.Serializable]
    public class QualificationResult
    {
        public string PlayerId;
        public string CompetitionId;
        public bool IsQualified;
        public string FailureReason;
        public CompetitionRequirements CheckedRequirements;
        public DateTime CheckDate = DateTime.Now;
    }

    [System.Serializable]
    public class PlantEntry
    {
        public string EntryId;
        public string CategoryId;
        public PlantSubmission PlantSubmission;
        public DateTime SubmissionDate;
        public EntryStatus Status;
    }

    #endregion

    #region Rewards Classes

    [System.Serializable]
    public class Prize
    {
        public string PrizeId;
        public string Name;
        public string Description;
        public float MonetaryValue;
        public List<string> Items = new List<string>();
        public PrizeRarity Rarity;
        public DateTime ExpiryDate;
    }

    [System.Serializable]
    public class PrizeStructure
    {
        public Prize FirstPlacePrize;
        public Prize SecondPlacePrize;
        public Prize ThirdPlacePrize;
        public Prize HonorableMentionPrize;
        public List<Prize> CategoryPrizes = new List<Prize>();
        public List<Prize> SpecialAwards = new List<Prize>();
    }

    [System.Serializable]
    public class DistributedPrize
    {
        public string PrizeId;
        public string WinnerId;
        public string CompetitionId;
        public Prize Prize;
        public DateTime DistributionDate;
        public bool IsClaimed;
        public DateTime ClaimDate;
    }

    [System.Serializable]
    public class WinnerProfile
    {
        public string WinnerId;
        public string CompetitionId;
        public string CompetitionName;
        public PlacementPosition PlacementPosition;
        public float Score;
        public DateTime WinDate;
        public string Category;
    }

    [System.Serializable]
    public class RewardHistory
    {
        public string PlayerId;
        public string PrizeName;
        public float PrizeValue;
        public string CompetitionId;
        public DateTime AwardDate;
    }

    [System.Serializable]
    public class RewardStatistics
    {
        public string PlayerId;
        public int TotalPrizesWon;
        public float TotalValueAwarded;
        public int AchievementsUnlocked;
        public int FirstPlaceWins;
        public int SecondPlaceWins;
        public int ThirdPlaceWins;
        public int CompetitionsParticipated;
        public DateTime LastUpdate = DateTime.Now;
    }

    #endregion

    #region Profile Classes

    [System.Serializable]
    public class CompetitorProfile
    {
        public string CompetitorId;
        public string Name;
        public CompetitorLevel Level;
        public int CompetitionsEntered;
        public int Wins;
        public int PlacementTrophies;
        public float AverageScore;
        public DateTime LastCompetition;
        public List<string> Specialties = new List<string>();
        public Dictionary<string, float> CategoryStats = new Dictionary<string, float>();
    }

    [System.Serializable]
    public class CompetitorRanking
    {
        public string CompetitorId;
        public int GlobalRank;
        public float RankingScore;
        public CompetitorLevel Tier;
        public int SeasonWins;
        public DateTime LastUpdate;
    }

    [System.Serializable]
    public class HistoricalWinner
    {
        public string WinnerId;
        public string CompetitionId;
        public string CompetitionName;
        public DateTime WinDate;
        public string Category;
        public float WinningScore;
        public PlacementPosition Position;
    }

    [System.Serializable]
    public class CupLegacyRecord
    {
        public int TotalCompetitions;
        public int TotalParticipants;
        public DateTime FirstCompetition;
        public DateTime LastCompetition;
        public List<HistoricalWinner> ChampionHistory = new List<HistoricalWinner>();
        public Dictionary<string, int> CategoryWinCounts = new Dictionary<string, int>();
    }

    #endregion

    #region Enums

    public enum CultivationMethod
    {
        Soil,
        Hydroponic,
        Aeroponic,
        Coco,
        Organic,
        Mixed
    }

    public enum RegistrationStatus
    {
        Pending,
        Validated,
        ValidationFailed,
        Submitted,
        Accepted,
        Rejected,
        Cancelled
    }

    public enum EntryStatus
    {
        NotFound,
        Draft,
        Submitted,
        UnderReview,
        Accepted,
        Rejected,
        Disqualified
    }

    public enum JudgeQualificationLevel
    {
        Unqualified,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master
    }

    public enum PlacementPosition
    {
        First,
        Second,
        Third,
        HonorableMention
    }

    public enum PrizeRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum CompetitionStatus
    {
        NotFound,
        Scheduled,
        Active,
        Judging,
        Complete,
        Ended,
        Cancelled
    }

    #endregion
}