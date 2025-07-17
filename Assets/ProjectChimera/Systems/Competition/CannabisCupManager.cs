using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Competition;
using CompetitorLevel = ProjectChimera.Data.Competition.CompetitorLevel;

namespace ProjectChimera.Systems.Competition
{
    /// <summary>
    /// Phase 4.3.a: Cannabis Cup Competition System
    /// Implements competitive cannabis cultivation tournaments with judging, categories, and rewards
    /// Creates engaging endgame content through structured competitions and community challenges
    /// </summary>
    public class CannabisCupManager : ChimeraManager
    {
        [Header("Phase 4.3.a Configuration")]
        public bool EnableCompetitions = true;
        public bool EnableCommunityJudging = true;
        public bool EnableSeasonalEvents = true;
        public bool EnableLegacyTracking = true;
        
        [Header("Competition Settings")]
        [Range(1, 12)] public int CompetitionsPerYear = 4;
        [Range(0f, 1f)] public float JudgingAccuracyWeight = 0.8f;
        [Range(1, 100)] public int MaxEntriesPerCompetition = 50;
        public float CompetitionDuration = 2592000f; // 30 days
        
        [Header("Active Competitions")]
        [SerializeField] private List<Competition> activeCompetitions = new List<Competition>();
        [SerializeField] private List<Competition> completedCompetitions = new List<Competition>();
        [SerializeField] private List<CompetitionEntry> pendingEntries = new List<CompetitionEntry>();
        [SerializeField] private Dictionary<string, CompetitorProfile> competitorProfiles = new Dictionary<string, CompetitorProfile>();
        
        [Header("Judging System")]
        [SerializeField] private List<Judge> activeJudges = new List<Judge>();
        [SerializeField] private List<JudgingSession> activeSessions = new List<JudgingSession>();
        [SerializeField] private Dictionary<string, JudgingCriteria> judgingStandards = new Dictionary<string, JudgingCriteria>();
        
        [Header("Legacy System")]
        [SerializeField] private CupLegacyRecord legacyRecord = new CupLegacyRecord();
        [SerializeField] private List<HistoricalWinner> hallOfFame = new List<HistoricalWinner>();
        [SerializeField] private Dictionary<string, CompetitorRanking> globalRankings = new Dictionary<string, CompetitorRanking>();
        
        // Phase 4.3 Data Structures
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
            public List<CompetitionCategory> Categories;
            public List<string> Sponsors;
            public CompetitionRules Rules;
            public List<CompetitionEntry> Entries;
            public CompetitionResults Results;
            public bool IsActive;
            public bool AcceptingEntries;
            public float EntryFee;
            public CompetitionRewards Rewards;
            public string Location;
            public Dictionary<string, object> Metadata;
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
            public List<string> PhotoUrls;
            public EntryStatus Status;
            public float EntryScore;
            public List<JudgeScore> Scores;
            public string DisqualificationReason;
            public bool IsEligible;
            public Dictionary<string, object> Metadata;
        }
        
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
            public TextureQuality Texture;
            public List<string> CultivationNotes;
            public int DaysToHarvest;
            public EnvironmentalData GrowConditions;
        }
        
        [System.Serializable]
        public class CompetitionCategory
        {
            public string CategoryId;
            public string Name;
            public string Description;
            public CategoryType Type;
            public List<string> EligibilityRequirements;
            public JudgingCriteria Criteria;
            public float MaxEntries;
            public bool IsOpen;
            public CategoryRewards Rewards;
            public List<string> PreviousWinners;
        }
        
        [System.Serializable]
        public class Judge
        {
            public string JudgeId;
            public string Name;
            public JudgeType Type;
            public float ExperienceLevel;
            public List<string> Specializations;
            public JudgeCredentials Credentials;
            public float ReliabilityScore;
            public int CompetitionsJudged;
            public List<string> ConflictsOfInterest;
            public bool IsAvailable;
            public JudgePreferences Preferences;
            public float CalibrationScore;
        }
        
        [System.Serializable]
        public class JudgingSession
        {
            public string SessionId;
            public string CompetitionId;
            public string JudgeId;
            public DateTime StartTime;
            public DateTime EndTime;
            public List<string> EntriesEvaluated;
            public JudgingEnvironment Environment;
            public List<JudgeScore> Scores;
            public string Notes;
            public bool IsCompleted;
            public float SessionReliability;
        }
        
        [System.Serializable]
        public class JudgeScore
        {
            public string EntryId;
            public string JudgeId;
            public DateTime EvaluationTime;
            public Dictionary<string, float> CriteriaScores;
            public float OverallScore;
            public string Comments;
            public List<string> Strengths;
            public List<string> Weaknesses;
            public float Confidence;
            public bool IsCalibrated;
        }
        
        [System.Serializable]
        public class JudgingCriteria
        {
            public string CriteriaId;
            public string Name;
            public Dictionary<string, CriteriaWeight> Weights;
            public List<EvaluationMetric> Metrics;
            public ScoringMethod ScoringMethod;
            public float MinScore;
            public float MaxScore;
            public string Description;
            public List<string> Guidelines;
        }
        
        [System.Serializable]
        public class CriteriaWeight
        {
            public string MetricName;
            public float Weight;
            public string Description;
            public bool IsMandatory;
        }
        
        [System.Serializable]
        public class EvaluationMetric
        {
            public string MetricId;
            public string Name;
            public MetricType Type;
            public float MinValue;
            public float MaxValue;
            public string Unit;
            public string Description;
            public List<string> EvaluationGuidelines;
        }
        
        [System.Serializable]
        public class CompetitorProfile
        {
            public string CompetitorId;
            public string Name;
            public CompetitorLevel Level;
            public DateTime FirstCompetition;
            public int TotalCompetitions;
            public int Wins;
            public int Podiums;
            public float AverageScore;
            public List<string> Specializations;
            public CompetitorStats Stats;
            public List<string> Achievements;
            public float ReputationScore;
            public CompetitorRanking CurrentRanking;
            public List<CompetitionHistory> History;
        }
        
        [System.Serializable]
        public class CompetitionResults
        {
            public string CompetitionId;
            public DateTime FinalizedDate;
            public Dictionary<string, CategoryResults> CategoryResults;
            public List<OverallWinner> OverallWinners;
            public CompetitionStatistics Statistics;
            public List<string> Highlights;
            public string SummaryReport;
            public bool IsFinalized;
            public List<string> Controversies;
        }
        
        [System.Serializable]
        public class CategoryResults
        {
            public string CategoryId;
            public List<PlacementResult> Placements;
            public float AverageScore;
            public float HighestScore;
            public int TotalEntries;
            public CategoryStatistics Statistics;
        }
        
        [System.Serializable]
        public class PlacementResult
        {
            public int Placement;
            public string EntryId;
            public string CompetitorId;
            public float FinalScore;
            public string Award;
            public float Prize;
            public List<JudgeScore> Scores;
            public string Citation;
        }
        
        [System.Serializable]
        public class CupLegacyRecord
        {
            public DateTime FirstCompetition;
            public int TotalCompetitions;
            public int TotalEntries;
            public int TotalCompetitors;
            public List<LegendaryStrain> LegendaryStrains;
            public List<ChampionBreeder> ChampionBreeders;
            public Dictionary<string, CompetitionMilestone> Milestones;
            public CupEvolution Evolution;
            public List<InnovationRecord> Innovations;
        }
        
        [System.Serializable]
        public class HistoricalWinner
        {
            public string CompetitorId;
            public string CompetitionId;
            public string CategoryId;
            public DateTime CompetitionDate;
            public string StrainName;
            public float WinningScore;
            public string Achievement;
            public LegacyImpact Impact;
            public bool IsLegendary;
        }
        
        // Enums
        public enum CompetitionType
        {
            Regional,
            National,
            International,
            Invitational,
            Community,
            Professional,
            Amateur,
            Masters
        }
        
        public enum CompetitionTier
        {
            Local,
            Regional,
            National,
            International,
            WorldChampionship
        }
        
        public enum CategoryType
        {
            Indica,
            Sativa,
            Hybrid,
            Concentrate,
            Edible,
            Topical,
            Innovation,
            Sustainability,
            BestInShow
        }
        
        public enum CultivationMethod
        {
            Indoor,
            Outdoor,
            Greenhouse,
            Hydroponic,
            Organic,
            Living_Soil,
            Aeroponic,
            Mixed
        }
        
        public enum EntryStatus
        {
            Submitted,
            UnderReview,
            Approved,
            Rejected,
            Judging,
            Scored,
            Finalist,
            Winner,
            Disqualified
        }
        
        public enum JudgeType
        {
            Professional,
            Expert,
            Community,
            Celebrity,
            Industry,
            Scientific,
            Consumer,
            International
        }
        
        
        public enum MetricType
        {
            Visual,
            Aroma,
            Flavor,
            Texture,
            Potency,
            Purity,
            Innovation,
            Sustainability
        }
        
        public enum ScoringMethod
        {
            Average,
            Weighted,
            Consensus,
            Elimination,
            Ranked
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeCompetitionSystem();
            LoadJudgingStandards();
            SetupSeasonalCalendar();
            RegisterEventListeners();
            
            Debug.Log($"[Phase 4.3.a] CannabisCupManager initialized - Competition system active");
        }
        
        protected override void OnManagerShutdown()
        {
            SaveCompetitionHistory();
            SaveLegacyRecords();
            UnregisterEventListeners();
            
            Debug.Log($"[Phase 4.3.a] CannabisCupManager shutdown - {completedCompetitions.Count} competitions recorded");
        }
        
        private void InitializeCompetitionSystem()
        {
            // Phase 4.3.a: Initialize competition infrastructure
            InvokeRepeating(nameof(UpdateActiveCompetitions), 3600f, 7200f); // Every 2 hours
            InvokeRepeating(nameof(ProcessJudgingSessions), 1800f, 3600f); // Every hour
            InvokeRepeating(nameof(UpdateRankings), 86400f, 86400f); // Daily
            
            InitializeJudges();
            CreateSeasonalCompetitions();
            InitializeLegacySystem();
        }
        
        private void LoadJudgingStandards()
        {
            // Phase 4.3.b: Initialize judging criteria and standards
            judgingStandards["Indica"] = new JudgingCriteria
            {
                CriteriaId = "indica_standard",
                Name = "Indica Evaluation Criteria",
                Weights = new Dictionary<string, CriteriaWeight>
                {
                    ["Visual"] = new CriteriaWeight { MetricName = "Visual Quality", Weight = 0.25f, Description = "Appearance, structure, trichomes", IsMandatory = true },
                    ["Aroma"] = new CriteriaWeight { MetricName = "Aroma Profile", Weight = 0.20f, Description = "Scent complexity and appeal", IsMandatory = true },
                    ["Flavor"] = new CriteriaWeight { MetricName = "Flavor Profile", Weight = 0.20f, Description = "Taste and terpene expression", IsMandatory = true },
                    ["Potency"] = new CriteriaWeight { MetricName = "Cannabinoid Content", Weight = 0.15f, Description = "THC/CBD levels and balance", IsMandatory = true },
                    ["Effect"] = new CriteriaWeight { MetricName = "Effect Quality", Weight = 0.20f, Description = "Indica-specific effects", IsMandatory = false }
                },
                ScoringMethod = ScoringMethod.Weighted,
                MinScore = 0f,
                MaxScore = 100f,
                Description = "Comprehensive evaluation for Indica cannabis varieties"
            };
            
            judgingStandards["Sativa"] = new JudgingCriteria
            {
                CriteriaId = "sativa_standard",
                Name = "Sativa Evaluation Criteria",
                Weights = new Dictionary<string, CriteriaWeight>
                {
                    ["Visual"] = new CriteriaWeight { MetricName = "Visual Quality", Weight = 0.25f, Description = "Structure and visual appeal", IsMandatory = true },
                    ["Aroma"] = new CriteriaWeight { MetricName = "Aroma Profile", Weight = 0.20f, Description = "Citrus, floral, and spice notes", IsMandatory = true },
                    ["Flavor"] = new CriteriaWeight { MetricName = "Flavor Profile", Weight = 0.20f, Description = "Complex flavor development", IsMandatory = true },
                    ["Potency"] = new CriteriaWeight { MetricName = "Cannabinoid Content", Weight = 0.15f, Description = "Balanced cannabinoid profile", IsMandatory = true },
                    ["Effect"] = new CriteriaWeight { MetricName = "Effect Quality", Weight = 0.20f, Description = "Energizing and cerebral effects", IsMandatory = false }
                },
                ScoringMethod = ScoringMethod.Weighted,
                MinScore = 0f,
                MaxScore = 100f,
                Description = "Comprehensive evaluation for Sativa cannabis varieties"
            };
            
            judgingStandards["Innovation"] = new JudgingCriteria
            {
                CriteriaId = "innovation_standard",
                Name = "Innovation Category Criteria",
                Weights = new Dictionary<string, CriteriaWeight>
                {
                    ["Novelty"] = new CriteriaWeight { MetricName = "Innovation Level", Weight = 0.30f, Description = "Breakthrough techniques or genetics", IsMandatory = true },
                    ["Execution"] = new CriteriaWeight { MetricName = "Implementation Quality", Weight = 0.25f, Description = "How well innovation is executed", IsMandatory = true },
                    ["Impact"] = new CriteriaWeight { MetricName = "Industry Impact", Weight = 0.20f, Description = "Potential influence on industry", IsMandatory = true },
                    ["Sustainability"] = new CriteriaWeight { MetricName = "Environmental Impact", Weight = 0.15f, Description = "Eco-friendly practices", IsMandatory = false },
                    ["Quality"] = new CriteriaWeight { MetricName = "End Product Quality", Weight = 0.10f, Description = "Final product assessment", IsMandatory = true }
                },
                ScoringMethod = ScoringMethod.Consensus,
                MinScore = 0f,
                MaxScore = 100f,
                Description = "Evaluation criteria for innovation and breakthrough categories"
            };
        }
        
        private void InitializeJudges()
        {
            // Phase 4.3.c: Create expert judge panel
            var masterJudge = new Judge
            {
                JudgeId = "judge_001",
                Name = "Master Cultivator Chen",
                Type = JudgeType.Professional,
                ExperienceLevel = 0.95f,
                Specializations = new List<string> { "Genetics", "Cultivation", "Quality Assessment" },
                Credentials = new JudgeCredentials
                {
                    YearsExperience = 25,
                    CertificationLevel = "Master",
                    CompetitionsJudged = 150,
                    Reputation = 0.98f
                },
                ReliabilityScore = 0.92f,
                IsAvailable = true,
                CalibrationScore = 0.88f
            };
            activeJudges.Add(masterJudge);
            
            var expertJudge = new Judge
            {
                JudgeId = "judge_002",
                Name = "Dr. Terpene Thompson",
                Type = JudgeType.Scientific,
                ExperienceLevel = 0.88f,
                Specializations = new List<string> { "Terpenes", "Chemistry", "Analysis" },
                Credentials = new JudgeCredentials
                {
                    YearsExperience = 15,
                    CertificationLevel = "Expert",
                    CompetitionsJudged = 75,
                    Reputation = 0.91f
                },
                ReliabilityScore = 0.89f,
                IsAvailable = true,
                CalibrationScore = 0.85f
            };
            activeJudges.Add(expertJudge);
            
            var industryJudge = new Judge
            {
                JudgeId = "judge_003",
                Name = "Industry Expert Martinez",
                Type = JudgeType.Industry,
                ExperienceLevel = 0.82f,
                Specializations = new List<string> { "Market Trends", "Consumer Preferences", "Business" },
                Credentials = new JudgeCredentials
                {
                    YearsExperience = 12,
                    CertificationLevel = "Expert",
                    CompetitionsJudged = 60,
                    Reputation = 0.85f
                },
                ReliabilityScore = 0.86f,
                IsAvailable = true,
                CalibrationScore = 0.82f
            };
            activeJudges.Add(industryJudge);
        }
        
        private void CreateSeasonalCompetitions()
        {
            // Phase 4.3.d: Create scheduled competitions
            var springCup = new Competition
            {
                CompetitionId = "spring_cup_2025",
                Name = "Spring Cannabis Cup 2025",
                Type = CompetitionType.Regional,
                Tier = CompetitionTier.Regional,
                StartDate = new DateTime(2025, 3, 1),
                EndDate = new DateTime(2025, 3, 31),
                JudgingDeadline = new DateTime(2025, 4, 15),
                Categories = CreateCompetitionCategories(),
                Rules = CreateStandardRules(),
                IsActive = true,
                AcceptingEntries = true,
                EntryFee = 250f,
                Rewards = CreateStandardRewards(),
                Location = "Cannabis Valley Convention Center",
                Metadata = new Dictionary<string, object>
                {
                    ["Theme"] = "Innovation and Quality",
                    ["MaxEntries"] = 100,
                    ["TargetAudience"] = "Professional"
                }
            };
            
            activeCompetitions.Add(springCup);
            
            Debug.Log($"[Phase 4.3] Created seasonal competition: {springCup.Name}");
        }
        
        private List<CompetitionCategory> CreateCompetitionCategories()
        {
            return new List<CompetitionCategory>
            {
                new CompetitionCategory
                {
                    CategoryId = "indica_flower",
                    Name = "Best Indica Flower",
                    Description = "Traditional indica varieties showcasing classic characteristics",
                    Type = CategoryType.Indica,
                    EligibilityRequirements = new List<string> { "Minimum 80% Indica genetics", "Flower only", "Indoor or outdoor" },
                    Criteria = judgingStandards["Indica"],
                    MaxEntries = 25,
                    IsOpen = true,
                    Rewards = new CategoryRewards { FirstPlace = 5000f, SecondPlace = 2500f, ThirdPlace = 1000f }
                },
                new CompetitionCategory
                {
                    CategoryId = "sativa_flower",
                    Name = "Best Sativa Flower",
                    Description = "Pure sativa expressions with uplifting characteristics",
                    Type = CategoryType.Sativa,
                    EligibilityRequirements = new List<string> { "Minimum 80% Sativa genetics", "Flower only", "Any cultivation method" },
                    Criteria = judgingStandards["Sativa"],
                    MaxEntries = 25,
                    IsOpen = true,
                    Rewards = new CategoryRewards { FirstPlace = 5000f, SecondPlace = 2500f, ThirdPlace = 1000f }
                },
                new CompetitionCategory
                {
                    CategoryId = "innovation",
                    Name = "Innovation Category",
                    Description = "Breakthrough cultivation techniques or genetic innovations",
                    Type = CategoryType.Innovation,
                    EligibilityRequirements = new List<string> { "Novel technique or genetics", "Documented innovation", "Peer review" },
                    Criteria = judgingStandards["Innovation"],
                    MaxEntries = 15,
                    IsOpen = true,
                    Rewards = new CategoryRewards { FirstPlace = 10000f, SecondPlace = 5000f, ThirdPlace = 2500f }
                }
            };
        }
        
        private CompetitionRules CreateStandardRules()
        {
            return new CompetitionRules
            {
                MinimumAge = 21,
                LicenseRequired = true,
                MaxEntriesPerCompetitor = 3,
                SampleSize = "3.5 grams minimum",
                SubmissionDeadline = "48 hours before judging",
                DisqualificationCriteria = new List<string>
                {
                    "Contamination detected",
                    "Incorrect labeling",
                    "Missing documentation",
                    "Late submission",
                    "Fraudulent information"
                },
                JudgingProtocol = "Blind evaluation by certified panel",
                AppealProcess = "Written appeal within 48 hours"
            };
        }
        
        private CompetitionRewards CreateStandardRewards()
        {
            return new CompetitionRewards
            {
                TotalPrizePool = 50000f,
                OverallWinner = 15000f,
                CategoryWinners = 5000f,
                Runners_up = 2500f,
                ParticipationReward = 100f,
                SpecialAwards = new Dictionary<string, float>
                {
                    ["People's Choice"] = 2500f,
                    ["Best Innovation"] = 3000f,
                    ["Sustainability Award"] = 2000f,
                    ["Rising Star"] = 1500f
                }
            };
        }
        
        private void SetupSeasonalCalendar()
        {
            // Phase 4.3.e: Schedule annual competition calendar
            var seasonalSchedule = new List<(string name, DateTime start, CompetitionTier tier)>
            {
                ("Spring Regional Cup", new DateTime(2025, 3, 1), CompetitionTier.Regional),
                ("Summer Championship", new DateTime(2025, 6, 15), CompetitionTier.National),
                ("Harvest Festival Competition", new DateTime(2025, 9, 1), CompetitionTier.Regional),
                ("Winter Masters Cup", new DateTime(2025, 12, 1), CompetitionTier.International)
            };
            
            foreach (var (name, start, tier) in seasonalSchedule)
            {
                ScheduleCompetition(name, start, tier);
            }
        }
        
        private void ScheduleCompetition(string name, DateTime startDate, CompetitionTier tier)
        {
            var competition = new Competition
            {
                CompetitionId = Guid.NewGuid().ToString("N")[..8],
                Name = name,
                Type = tier == CompetitionTier.International ? CompetitionType.International : CompetitionType.Regional,
                Tier = tier,
                StartDate = startDate,
                EndDate = startDate.AddDays(30),
                JudgingDeadline = startDate.AddDays(45),
                IsActive = false,
                AcceptingEntries = false,
                Categories = CreateCompetitionCategories(),
                Rules = CreateStandardRules(),
                Rewards = CreateTieredRewards(tier)
            };
            
            activeCompetitions.Add(competition);
            Debug.Log($"[Phase 4.3] Scheduled {name} for {startDate:MMM yyyy}");
        }
        
        private CompetitionRewards CreateTieredRewards(CompetitionTier tier)
        {
            float multiplier = tier switch
            {
                CompetitionTier.Local => 0.5f,
                CompetitionTier.Regional => 1f,
                CompetitionTier.National => 2f,
                CompetitionTier.International => 3f,
                CompetitionTier.WorldChampionship => 5f,
                _ => 1f
            };
            
            return new CompetitionRewards
            {
                TotalPrizePool = 50000f * multiplier,
                OverallWinner = 15000f * multiplier,
                CategoryWinners = 5000f * multiplier,
                Runners_up = 2500f * multiplier,
                ParticipationReward = 100f * multiplier
            };
        }
        
        private void InitializeLegacySystem()
        {
            legacyRecord = new CupLegacyRecord
            {
                FirstCompetition = DateTime.Now,
                TotalCompetitions = 0,
                TotalEntries = 0,
                TotalCompetitors = 0,
                LegendaryStrains = new List<LegendaryStrain>(),
                ChampionBreeders = new List<ChampionBreeder>(),
                Milestones = new Dictionary<string, CompetitionMilestone>(),
                Evolution = new CupEvolution
                {
                    StageProgression = new List<string> { "Local Events", "Regional Growth", "National Recognition" },
                    CurrentStage = "Local Events",
                    NextMilestone = "Regional Growth - 50 total competitors"
                },
                Innovations = new List<InnovationRecord>()
            };
        }
        
        private void UpdateActiveCompetitions()
        {
            // Phase 4.3.f: Update competition states and timelines
            foreach (var competition in activeCompetitions.ToList())
            {
                UpdateCompetitionState(competition);
                CheckCompetitionMilestones(competition);
            }
        }
        
        private void UpdateCompetitionState(Competition competition)
        {
            var now = DateTime.Now;
            
            // Check if competition should start accepting entries
            if (!competition.AcceptingEntries && now >= competition.StartDate.AddDays(-30))
            {
                competition.AcceptingEntries = true;
                Debug.Log($"[Phase 4.3] {competition.Name} now accepting entries");
                
                GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("CompetitionEntriesOpen", competition);
            }
            
            // Check if entry period has ended
            if (competition.AcceptingEntries && now >= competition.EndDate)
            {
                competition.AcceptingEntries = false;
                StartJudgingPhase(competition);
                Debug.Log($"[Phase 4.3] Entry period ended for {competition.Name}, judging begins");
            }
            
            // Check if judging deadline has passed
            if (now >= competition.JudgingDeadline && !competition.Results.IsFinalized)
            {
                FinalizeCompetitionResults(competition);
            }
        }
        
        private void StartJudgingPhase(Competition competition)
        {
            // Phase 4.3.g: Initialize judging sessions for competition
            foreach (var category in competition.Categories)
            {
                var entriesInCategory = competition.Entries.Where(e => e.CategoryId == category.CategoryId).ToList();
                
                if (entriesInCategory.Count > 0)
                {
                    CreateJudgingSessions(competition.CompetitionId, category, entriesInCategory);
                }
            }
        }
        
        private void CreateJudgingSessions(string competitionId, CompetitionCategory category, List<CompetitionEntry> entries)
        {
            // Assign judges to evaluate entries
            var availableJudges = activeJudges.Where(j => j.IsAvailable && 
                j.Specializations.Any(s => IsRelevantSpecialization(s, category.Type))).ToList();
            
            foreach (var judge in availableJudges.Take(3)) // Use 3 judges per category
            {
                var session = new JudgingSession
                {
                    SessionId = Guid.NewGuid().ToString("N")[..8],
                    CompetitionId = competitionId,
                    JudgeId = judge.JudgeId,
                    StartTime = DateTime.Now,
                    EntriesEvaluated = entries.Select(e => e.EntryId).ToList(),
                    Environment = new JudgingEnvironment
                    {
                        Location = "Judging Room A",
                        Temperature = 22f,
                        Humidity = 45f,
                        Lighting = "Full spectrum LED",
                        IsControlled = true
                    },
                    Scores = new List<JudgeScore>(),
                    IsCompleted = false
                };
                
                activeSessions.Add(session);
                judge.IsAvailable = false;
                
                // Start evaluation process
                StartJudgeEvaluation(session, entries, category.Criteria);
            }
        }
        
        private bool IsRelevantSpecialization(string specialization, CategoryType categoryType)
        {
            return categoryType switch
            {
                CategoryType.Indica => specialization.Contains("Genetics") || specialization.Contains("Quality") || specialization.Contains("Cultivation"),
                CategoryType.Sativa => specialization.Contains("Genetics") || specialization.Contains("Quality") || specialization.Contains("Cultivation"),
                CategoryType.Innovation => specialization.Contains("Innovation") || specialization.Contains("Technology") || specialization.Contains("Research"),
                CategoryType.Concentrate => specialization.Contains("Extraction") || specialization.Contains("Chemistry") || specialization.Contains("Processing"),
                _ => true
            };
        }
        
        private void StartJudgeEvaluation(JudgingSession session, List<CompetitionEntry> entries, JudgingCriteria criteria)
        {
            // Phase 4.3.h: Simulate judging process
            foreach (var entry in entries)
            {
                var score = EvaluateEntry(entry, session.JudgeId, criteria);
                session.Scores.Add(score);
                entry.Scores.Add(score);
            }
            
            session.IsCompleted = true;
            session.EndTime = DateTime.Now;
            session.SessionReliability = CalculateSessionReliability(session);
            
            // Make judge available again
            var judge = activeJudges.FirstOrDefault(j => j.JudgeId == session.JudgeId);
            if (judge != null)
            {
                judge.IsAvailable = true;
                judge.CompetitionsJudged++;
            }
            
            Debug.Log($"[Phase 4.3] Judging session completed by {judge?.Name} for {entries.Count} entries");
        }
        
        private JudgeScore EvaluateEntry(CompetitionEntry entry, string judgeId, JudgingCriteria criteria)
        {
            var judge = activeJudges.FirstOrDefault(j => j.JudgeId == judgeId);
            var plant = entry.PlantData;
            
            // Simulate realistic scoring based on plant data and judge expertise
            var criteriaScores = new Dictionary<string, float>();
            
            foreach (var weight in criteria.Weights)
            {
                float score = weight.Key switch
                {
                    "Visual" => EvaluateVisualQuality(plant, judge),
                    "Aroma" => EvaluateAromaQuality(plant, judge),
                    "Flavor" => EvaluateFlavorQuality(plant, judge),
                    "Potency" => EvaluatePotency(plant, judge),
                    "Effect" => EvaluateEffect(plant, judge),
                    "Innovation" => EvaluateInnovation(plant, judge),
                    "Sustainability" => EvaluateSustainability(plant, judge),
                    _ => UnityEngine.Random.Range(60f, 95f)
                };
                
                criteriaScores[weight.Key] = score;
            }
            
            // Calculate weighted overall score
            float overallScore = 0f;
            foreach (var kvp in criteriaScores)
            {
                if (criteria.Weights.ContainsKey(kvp.Key))
                {
                    overallScore += kvp.Value * criteria.Weights[kvp.Key].Weight;
                }
            }
            
            // Add judge reliability and calibration factors
            float judgeReliability = judge?.ReliabilityScore ?? 0.8f;
            float calibrationFactor = judge?.CalibrationScore ?? 0.8f;
            
            overallScore *= (judgeReliability + calibrationFactor) / 2f;
            overallScore = Mathf.Clamp(overallScore, criteria.MinScore, criteria.MaxScore);
            
            return new JudgeScore
            {
                EntryId = entry.EntryId,
                JudgeId = judgeId,
                EvaluationTime = DateTime.Now,
                CriteriaScores = criteriaScores,
                OverallScore = overallScore,
                Comments = GenerateJudgeComments(plant, criteriaScores),
                Strengths = IdentifyStrengths(criteriaScores),
                Weaknesses = IdentifyWeaknesses(criteriaScores),
                Confidence = CalculateJudgeConfidence(judge, criteriaScores),
                IsCalibrated = true
            };
        }
        
        private float EvaluateVisualQuality(PlantSubmission plant, Judge judge)
        {
            float baseScore = plant.Visual.OverallAppeal * 100f;
            float expertiseBonus = judge?.Specializations.Contains("Quality") == true ? 5f : 0f;
            return Mathf.Clamp(baseScore + expertiseBonus + UnityEngine.Random.Range(-5f, 5f), 60f, 100f);
        }
        
        private float EvaluateAromaQuality(PlantSubmission plant, Judge judge)
        {
            float baseScore = plant.Aroma.Intensity * plant.Aroma.Complexity * 100f;
            float expertiseBonus = judge?.Specializations.Contains("Terpenes") == true ? 7f : 0f;
            return Mathf.Clamp(baseScore + expertiseBonus + UnityEngine.Random.Range(-5f, 5f), 60f, 100f);
        }
        
        private float EvaluateFlavorQuality(PlantSubmission plant, Judge judge)
        {
            float baseScore = (plant.Terpenes.DominantTerpenes.Count * 15f) + UnityEngine.Random.Range(60f, 90f);
            float expertiseBonus = judge?.Specializations.Contains("Chemistry") == true ? 6f : 0f;
            return Mathf.Clamp(baseScore + expertiseBonus, 60f, 100f);
        }
        
        private float EvaluatePotency(PlantSubmission plant, Judge judge)
        {
            float thcScore = Mathf.Clamp(plant.Cannabinoids.THC * 4f, 60f, 100f);
            float balanceScore = CalculateCannabinoidBalance(plant.Cannabinoids);
            return (thcScore + balanceScore) / 2f;
        }
        
        private float EvaluateEffect(PlantSubmission plant, Judge judge)
        {
            return UnityEngine.Random.Range(70f, 95f); // Placeholder for effect evaluation
        }
        
        private float EvaluateInnovation(PlantSubmission plant, Judge judge)
        {
            float noveltyScore = plant.CultivationNotes.Count * 10f;
            float expertiseBonus = judge?.Specializations.Contains("Innovation") == true ? 10f : 0f;
            return Mathf.Clamp(noveltyScore + expertiseBonus + UnityEngine.Random.Range(60f, 85f), 60f, 100f);
        }
        
        private float EvaluateSustainability(PlantSubmission plant, Judge judge)
        {
            float sustainabilityScore = plant.Method == CultivationMethod.Organic ? 85f : 70f;
            return sustainabilityScore + UnityEngine.Random.Range(-5f, 10f);
        }
        
        private float CalculateCannabinoidBalance(CannabinoidProfile profile)
        {
            float ratio = profile.CBD > 0 ? profile.THC / profile.CBD : profile.THC;
            return Mathf.Clamp(90f - (Mathf.Abs(ratio - 1f) * 10f), 60f, 95f);
        }
        
        private string GenerateJudgeComments(PlantSubmission plant, Dictionary<string, float> scores)
        {
            var comments = new List<string>();
            
            if (scores.GetValueOrDefault("Visual", 0) > 85f)
                comments.Add("Excellent visual presentation and structure");
            if (scores.GetValueOrDefault("Aroma", 0) > 85f)
                comments.Add("Outstanding aromatic profile");
            if (scores.GetValueOrDefault("Potency", 0) > 85f)
                comments.Add("Strong cannabinoid content");
            
            return string.Join(". ", comments);
        }
        
        private List<string> IdentifyStrengths(Dictionary<string, float> scores)
        {
            return scores.Where(kvp => kvp.Value > 85f).Select(kvp => kvp.Key).ToList();
        }
        
        private List<string> IdentifyWeaknesses(Dictionary<string, float> scores)
        {
            return scores.Where(kvp => kvp.Value < 75f).Select(kvp => kvp.Key).ToList();
        }
        
        private float CalculateJudgeConfidence(Judge judge, Dictionary<string, float> scores)
        {
            float baseConfidence = judge?.ExperienceLevel ?? 0.8f;
            float scoreVariance = CalculateScoreVariance(scores.Values);
            return Mathf.Clamp(baseConfidence - (scoreVariance * 0.1f), 0.6f, 1f);
        }
        
        private float CalculateScoreVariance(IEnumerable<float> scores)
        {
            if (!scores.Any()) return 0f;
            
            float mean = scores.Average();
            float variance = scores.Select(score => Mathf.Pow(score - mean, 2)).Average();
            return Mathf.Sqrt(variance);
        }
        
        private float CalculateSessionReliability(JudgingSession session)
        {
            var judge = activeJudges.FirstOrDefault(j => j.JudgeId == session.JudgeId);
            float judgeReliability = judge?.ReliabilityScore ?? 0.8f;
            
            float scoreConsistency = 1f - (CalculateScoreVariance(session.Scores.Select(s => s.OverallScore)) / 100f);
            
            return (judgeReliability + scoreConsistency) / 2f;
        }
        
        private void ProcessJudgingSessions()
        {
            // Phase 4.3.i: Process ongoing judging sessions
            var completedSessions = activeSessions.Where(s => s.IsCompleted).ToList();
            
            foreach (var session in completedSessions)
            {
                ProcessCompletedSession(session);
                activeSessions.Remove(session);
            }
        }
        
        private void ProcessCompletedSession(JudgingSession session)
        {
            // Update competition with session results
            var competition = activeCompetitions.FirstOrDefault(c => c.CompetitionId == session.CompetitionId);
            if (competition == null) return;
            
            // Check if all judging sessions for this competition are complete
            bool allSessionsComplete = !activeSessions.Any(s => s.CompetitionId == session.CompetitionId);
            
            if (allSessionsComplete)
            {
                CalculateCompetitionResults(competition);
            }
        }
        
        private void CalculateCompetitionResults(Competition competition)
        {
            // Phase 4.3.j: Calculate final competition results
            competition.Results = new CompetitionResults
            {
                CompetitionId = competition.CompetitionId,
                FinalizedDate = DateTime.Now,
                CategoryResults = new Dictionary<string, CategoryResults>(),
                OverallWinners = new List<OverallWinner>(),
                IsFinalized = false
            };
            
            foreach (var category in competition.Categories)
            {
                var categoryResults = CalculateCategoryResults(competition, category);
                competition.Results.CategoryResults[category.CategoryId] = categoryResults;
            }
            
            // Determine overall winners
            competition.Results.OverallWinners = DetermineOverallWinners(competition);
            
            Debug.Log($"[Phase 4.3] Calculated results for {competition.Name}");
        }
        
        private CategoryResults CalculateCategoryResults(Competition competition, CompetitionCategory category)
        {
            var entriesInCategory = competition.Entries.Where(e => e.CategoryId == category.CategoryId).ToList();
            
            // Calculate average scores for each entry
            foreach (var entry in entriesInCategory)
            {
                if (entry.Scores.Count > 0)
                {
                    entry.EntryScore = entry.Scores.Average(s => s.OverallScore);
                }
            }
            
            // Rank entries by score
            var rankedEntries = entriesInCategory.OrderByDescending(e => e.EntryScore).ToList();
            
            var placements = rankedEntries.Select((entry, index) => new PlacementResult
            {
                Placement = index + 1,
                EntryId = entry.EntryId,
                CompetitorId = entry.CompetitorId,
                FinalScore = entry.EntryScore,
                Award = GetAwardName(index + 1),
                Prize = GetPrizeAmount(category.Rewards, index + 1),
                Scores = entry.Scores,
                Citation = GenerateWinnerCitation(entry, index + 1)
            }).ToList();
            
            return new CategoryResults
            {
                CategoryId = category.CategoryId,
                Placements = placements,
                AverageScore = entriesInCategory.Average(e => e.EntryScore),
                HighestScore = entriesInCategory.Max(e => e.EntryScore),
                TotalEntries = entriesInCategory.Count,
                Statistics = new CategoryStatistics
                {
                    ParticipationRate = (float)entriesInCategory.Count / category.MaxEntries,
                    QualityIndex = entriesInCategory.Average(e => e.EntryScore) / 100f
                }
            };
        }
        
        private List<OverallWinner> DetermineOverallWinners(Competition competition)
        {
            var allPlacements = competition.Results.CategoryResults.Values
                .SelectMany(cr => cr.Placements)
                .ToList();
            
            // Find highest scoring entries across all categories
            var topEntries = allPlacements.OrderByDescending(p => p.FinalScore).Take(3).ToList();
            
            return topEntries.Select((placement, index) => new OverallWinner
            {
                CompetitorId = placement.CompetitorId,
                EntryId = placement.EntryId,
                OverallPlacement = index + 1,
                HighestCategoryScore = placement.FinalScore,
                CategoryWon = GetCategoryName(placement, competition),
                SpecialRecognition = index == 0 ? "Best in Show" : null
            }).ToList();
        }
        
        private string GetCategoryName(PlacementResult placement, Competition competition)
        {
            var entry = competition.Entries.FirstOrDefault(e => e.EntryId == placement.EntryId);
            var category = competition.Categories.FirstOrDefault(c => c.CategoryId == entry?.CategoryId);
            return category?.Name ?? "Unknown Category";
        }
        
        private string GetAwardName(int placement)
        {
            return placement switch
            {
                1 => "First Place - Gold Medal",
                2 => "Second Place - Silver Medal", 
                3 => "Third Place - Bronze Medal",
                _ => $"{placement}th Place"
            };
        }
        
        private float GetPrizeAmount(CategoryRewards rewards, int placement)
        {
            return placement switch
            {
                1 => rewards.FirstPlace,
                2 => rewards.SecondPlace,
                3 => rewards.ThirdPlace,
                _ => 0f
            };
        }
        
        private string GenerateWinnerCitation(CompetitionEntry entry, int placement)
        {
            var ordinal = placement switch
            {
                1 => "1st",
                2 => "2nd", 
                3 => "3rd",
                _ => $"{placement}th"
            };
            
            return $"Awarded {ordinal} place for exceptional {entry.PlantData.StrainName} with score of {entry.EntryScore:F1}";
        }
        
        private void FinalizeCompetitionResults(Competition competition)
        {
            // Phase 4.3.k: Finalize and publish competition results
            if (competition.Results == null)
            {
                CalculateCompetitionResults(competition);
            }
            
            competition.Results.IsFinalized = true;
            competition.IsActive = false;
            
            // Update competitor profiles and rankings
            UpdateCompetitorProfiles(competition);
            UpdateGlobalRankings(competition);
            UpdateLegacyRecords(competition);
            
            // Move to completed competitions
            activeCompetitions.Remove(competition);
            completedCompetitions.Add(competition);
            
            Debug.Log($"[Phase 4.3] Finalized results for {competition.Name}");
            
            // Fire competition completed event
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("CompetitionCompleted", competition);
        }
        
        private void UpdateCompetitorProfiles(Competition competition)
        {
            foreach (var entry in competition.Entries)
            {
                if (!competitorProfiles.ContainsKey(entry.CompetitorId))
                {
                    competitorProfiles[entry.CompetitorId] = CreateNewCompetitorProfile(entry.CompetitorId);
                }
                
                var profile = competitorProfiles[entry.CompetitorId];
                profile.TotalCompetitions++;
                
                // Check for wins and podiums
                var categoryResult = competition.Results.CategoryResults.Values
                    .FirstOrDefault(cr => cr.Placements.Any(p => p.CompetitorId == entry.CompetitorId));
                
                if (categoryResult != null)
                {
                    var placement = categoryResult.Placements.FirstOrDefault(p => p.CompetitorId == entry.CompetitorId);
                    if (placement != null)
                    {
                        if (placement.Placement == 1) profile.Wins++;
                        if (placement.Placement <= 3) profile.Podiums++;
                        
                        profile.History.Add(new CompetitionHistory
                        {
                            CompetitionId = competition.CompetitionId,
                            CompetitionName = competition.Name,
                            Placement = placement.Placement,
                            Score = placement.FinalScore,
                            Date = competition.EndDate
                        });
                    }
                }
                
                // Update average score
                profile.AverageScore = profile.History.Any() ? profile.History.Average(h => h.Score) : entry.EntryScore;
                
                // Update level based on performance
                UpdateCompetitorLevel(profile);
            }
        }
        
        private CompetitorProfile CreateNewCompetitorProfile(string competitorId)
        {
            return new CompetitorProfile
            {
                CompetitorId = competitorId,
                Name = $"Competitor {competitorId[..6]}",
                Level = CompetitorLevel.Novice,
                FirstCompetition = DateTime.Now,
                TotalCompetitions = 0,
                Wins = 0,
                Podiums = 0,
                AverageScore = 0f,
                Specializations = new List<string>(),
                Stats = new CompetitorStats(),
                Achievements = new List<string>(),
                ReputationScore = 0.5f,
                History = new List<CompetitionHistory>()
            };
        }
        
        private void UpdateCompetitorLevel(CompetitorProfile profile)
        {
            var newLevel = profile.TotalCompetitions switch
            {
                >= 1 and < 3 => CompetitorLevel.Novice,
                >= 3 and < 10 => CompetitorLevel.Intermediate,
                >= 10 and < 25 => CompetitorLevel.Advanced,
                >= 25 and < 50 => CompetitorLevel.Expert,
                >= 50 and < 100 => CompetitorLevel.Master,
                >= 100 => CompetitorLevel.Champion,
                _ => CompetitorLevel.Novice
            };
            
            // Additional criteria for Master/Champion
            if (profile.Wins >= 10 && profile.AverageScore > 85f)
            {
                newLevel = CompetitorLevel.Master;
            }
            
            if (profile.Wins >= 25 && profile.AverageScore > 90f && profile.TotalCompetitions >= 100)
            {
                newLevel = CompetitorLevel.Champion;
            }
            
            if (newLevel != profile.Level)
            {
                profile.Level = newLevel;
                Debug.Log($"[Phase 4.3] Competitor {profile.Name} promoted to {newLevel}");
            }
        }
        
        private void UpdateGlobalRankings(Competition competition)
        {
            // Update global competitor rankings based on recent performance
            foreach (var profile in competitorProfiles.Values)
            {
                if (!globalRankings.ContainsKey(profile.CompetitorId))
                {
                    globalRankings[profile.CompetitorId] = new CompetitorRanking
                    {
                        CompetitorId = profile.CompetitorId,
                        CurrentRank = 0,
                        Rating = 1000f, // ELO-style rating
                        TierLevel = profile.Level
                    };
                }
                
                var ranking = globalRankings[profile.CompetitorId];
                ranking.Rating = CalculateRating(profile);
                ranking.TierLevel = profile.Level;
            }
            
            // Sort and assign ranks
            var sortedRankings = globalRankings.Values.OrderByDescending(r => r.Rating).ToList();
            for (int i = 0; i < sortedRankings.Count; i++)
            {
                sortedRankings[i].CurrentRank = i + 1;
            }
        }
        
        private float CalculateRating(CompetitorProfile profile)
        {
            float baseRating = 1000f;
            float winBonus = profile.Wins * 50f;
            float podiumBonus = (profile.Podiums - profile.Wins) * 20f;
            float avgScoreBonus = (profile.AverageScore - 70f) * 5f;
            float experienceBonus = Mathf.Log(profile.TotalCompetitions + 1) * 30f;
            
            return baseRating + winBonus + podiumBonus + avgScoreBonus + experienceBonus;
        }
        
        private void UpdateLegacyRecords(Competition competition)
        {
            legacyRecord.TotalCompetitions++;
            legacyRecord.TotalEntries += competition.Entries.Count;
            
            var uniqueCompetitors = competition.Entries.Select(e => e.CompetitorId).Distinct().Count();
            legacyRecord.TotalCompetitors += uniqueCompetitors;
            
            // Check for legendary performances
            foreach (var categoryResult in competition.Results.CategoryResults.Values)
            {
                var winner = categoryResult.Placements.FirstOrDefault();
                if (winner != null && winner.FinalScore > 95f)
                {
                    RecordLegendaryPerformance(competition, winner);
                }
            }
            
            // Update evolution stage
            UpdateCupEvolution();
        }
        
        private void RecordLegendaryPerformance(Competition competition, PlacementResult winner)
        {
            var entry = competition.Entries.FirstOrDefault(e => e.EntryId == winner.EntryId);
            if (entry == null) return;
            
            var legendaryWinner = new HistoricalWinner
            {
                CompetitorId = winner.CompetitorId,
                CompetitionId = competition.CompetitionId,
                CategoryId = entry.CategoryId,
                CompetitionDate = competition.EndDate,
                StrainName = entry.PlantData.StrainName,
                WinningScore = winner.FinalScore,
                Achievement = "Legendary Performance - Score >95",
                Impact = new LegacyImpact
                {
                    IndustryInfluence = 0.8f,
                    InnovationLevel = 0.7f,
                    CommunityRecognition = 0.9f
                },
                IsLegendary = true
            };
            
            hallOfFame.Add(legendaryWinner);
            
            // Check if strain should be legendary
            if (winner.FinalScore > 97f)
            {
                RecordLegendaryStrain(entry);
            }
        }
        
        private void RecordLegendaryStrain(CompetitionEntry entry)
        {
            var legendaryStrain = new LegendaryStrain
            {
                StrainName = entry.PlantData.StrainName,
                GeneticLineage = entry.PlantData.GeneticLineage,
                OriginalBreeder = entry.CompetitorId,
                RecordScore = entry.EntryScore,
                CompetitionWon = entry.CompetitionId,
                LegendStatus = "Hall of Fame",
                ImpactRating = 0.95f
            };
            
            legacyRecord.LegendaryStrains.Add(legendaryStrain);
            
            Debug.Log($"[Phase 4.3] Recorded legendary strain: {legendaryStrain.StrainName} with score {entry.EntryScore:F1}");
        }
        
        private void UpdateCupEvolution()
        {
            var totalEvents = legacyRecord.TotalCompetitions;
            var totalParticipants = legacyRecord.TotalCompetitors;
            
            string newStage = legacyRecord.Evolution.CurrentStage;
            
            if (totalEvents >= 5 && totalParticipants >= 50 && legacyRecord.Evolution.CurrentStage == "Local Events")
            {
                newStage = "Regional Growth";
                legacyRecord.Evolution.NextMilestone = "National Recognition - 20 competitions, 200 competitors";
            }
            else if (totalEvents >= 20 && totalParticipants >= 200 && legacyRecord.Evolution.CurrentStage == "Regional Growth")
            {
                newStage = "National Recognition";
                legacyRecord.Evolution.NextMilestone = "International Status - 50 competitions, 500 competitors";
            }
            else if (totalEvents >= 50 && totalParticipants >= 500 && legacyRecord.Evolution.CurrentStage == "National Recognition")
            {
                newStage = "International Status";
                legacyRecord.Evolution.NextMilestone = "World Championship - 100 competitions, 1000+ competitors";
            }
            
            if (newStage != legacyRecord.Evolution.CurrentStage)
            {
                legacyRecord.Evolution.CurrentStage = newStage;
                Debug.Log($"[Phase 4.3] Cannabis Cup evolved to: {newStage}");
                
                GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("CupEvolutionMilestone", new { Stage = newStage });
            }
        }
        
        private void UpdateRankings()
        {
            // Daily ranking updates and season calculations
            RecalculateSeasonalStandings();
            UpdateTierPromotions();
        }
        
        private void RecalculateSeasonalStandings()
        {
            // Calculate seasonal standings for current year
            var currentYearCompetitions = completedCompetitions
                .Where(c => c.EndDate.Year == DateTime.Now.Year)
                .ToList();
            
            if (currentYearCompetitions.Count == 0) return;
            
            var seasonalStandings = new Dictionary<string, float>();
            
            foreach (var competition in currentYearCompetitions)
            {
                foreach (var categoryResult in competition.Results.CategoryResults.Values)
                {
                    foreach (var placement in categoryResult.Placements)
                    {
                        if (!seasonalStandings.ContainsKey(placement.CompetitorId))
                            seasonalStandings[placement.CompetitorId] = 0f;
                        
                        // Award points based on placement
                        float points = placement.Placement switch
                        {
                            1 => 100f,
                            2 => 80f,
                            3 => 60f,
                            4 => 40f,
                            5 => 20f,
                            _ => 10f
                        };
                        
                        seasonalStandings[placement.CompetitorId] += points;
                    }
                }
            }
            
            // Update competitor profiles with seasonal standings
            foreach (var kvp in seasonalStandings)
            {
                if (competitorProfiles.ContainsKey(kvp.Key))
                {
                    competitorProfiles[kvp.Key].Stats.SeasonalPoints = kvp.Value;
                }
            }
        }
        
        private void UpdateTierPromotions()
        {
            // Check for tier promotions based on consistent performance
            foreach (var profile in competitorProfiles.Values)
            {
                CheckForPromotion(profile);
            }
        }
        
        private void CheckForPromotion(CompetitorProfile profile)
        {
            var recentPerformances = profile.History
                .Where(h => h.Date >= DateTime.Now.AddMonths(-6))
                .ToList();
            
            if (recentPerformances.Count >= 3)
            {
                float avgRecentScore = recentPerformances.Average(h => h.Score);
                int recentWins = recentPerformances.Count(h => h.Placement == 1);
                
                if (avgRecentScore > 85f && recentWins >= 1 && profile.Level < CompetitorLevel.Master)
                {
                    PromoteCompetitor(profile);
                }
            }
        }
        
        private void PromoteCompetitor(CompetitorProfile profile)
        {
            var oldLevel = profile.Level;
            profile.Level = (CompetitorLevel)((int)profile.Level + 1);
            
            profile.Achievements.Add($"Promoted to {profile.Level} on {DateTime.Now:yyyy-MM-dd}");
            
            Debug.Log($"[Phase 4.3] {profile.Name} promoted from {oldLevel} to {profile.Level}");
            
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("CompetitorPromotion", 
                new { CompetitorId = profile.CompetitorId, OldLevel = oldLevel, NewLevel = profile.Level });
        }
        
        private void CheckCompetitionMilestones(Competition competition)
        {
            // Check for special milestones or achievements
            if (competition.Entries.Count >= 100 && !legacyRecord.Milestones.ContainsKey("Century_Competition"))
            {
                legacyRecord.Milestones["Century_Competition"] = new CompetitionMilestone
                {
                    Name = "Century Competition",
                    Description = "First competition with 100+ entries",
                    AchievedDate = DateTime.Now,
                    CompetitionId = competition.CompetitionId
                };
                
                Debug.Log($"[Phase 4.3] Milestone achieved: Century Competition");
            }
        }
        
        // Public API methods
        public List<Competition> GetActiveCompetitions() => new List<Competition>(activeCompetitions);
        public List<Competition> GetCompletedCompetitions() => new List<Competition>(completedCompetitions);
        public CompetitorProfile GetCompetitorProfile(string competitorId) => competitorProfiles.GetValueOrDefault(competitorId);
        public List<HistoricalWinner> GetHallOfFame() => new List<HistoricalWinner>(hallOfFame);
        public CupLegacyRecord GetLegacyRecord() => legacyRecord;
        
        public bool SubmitEntry(string competitionId, string categoryId, string competitorId, PlantSubmission plantData)
        {
            var competition = activeCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
            if (competition == null || !competition.AcceptingEntries) return false;
            
            var category = competition.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null || !category.IsOpen) return false;
            
            // Check if competitor has reached entry limit
            var existingEntries = competition.Entries.Count(e => e.CompetitorId == competitorId);
            if (existingEntries >= competition.Rules.MaxEntriesPerCompetitor) return false;
            
            var entry = new CompetitionEntry
            {
                EntryId = Guid.NewGuid().ToString("N")[..8],
                CompetitionId = competitionId,
                CompetitorId = competitorId,
                CategoryId = categoryId,
                SubmissionDate = DateTime.Now,
                PlantData = plantData,
                Status = EntryStatus.Submitted,
                IsEligible = true,
                Scores = new List<JudgeScore>(),
                Metadata = new Dictionary<string, object>()
            };
            
            competition.Entries.Add(entry);
            pendingEntries.Add(entry);
            
            Debug.Log($"[Phase 4.3] Entry submitted by {competitorId} for {competition.Name} - {category.Name}");
            
            return true;
        }
        
        public Competition CreateCustomCompetition(string name, CompetitionType type, List<CompetitionCategory> categories)
        {
            var competition = new Competition
            {
                CompetitionId = Guid.NewGuid().ToString("N")[..8],
                Name = name,
                Type = type,
                Tier = CompetitionTier.Local,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                JudgingDeadline = DateTime.Now.AddDays(45),
                Categories = categories,
                Rules = CreateStandardRules(),
                IsActive = true,
                AcceptingEntries = true,
                Entries = new List<CompetitionEntry>(),
                Results = new CompetitionResults { IsFinalized = false }
            };
            
            activeCompetitions.Add(competition);
            
            Debug.Log($"[Phase 4.3] Created custom competition: {name}");
            
            return competition;
        }
        
        private void RegisterEventListeners()
        {
            // Register for game events
        }
        
        private void UnregisterEventListeners()
        {
            // Unregister event listeners
        }
        
        private void SaveCompetitionHistory()
        {
            Debug.Log($"[Phase 4.3] Saving competition history: {completedCompetitions.Count} competitions");
        }
        
        private void SaveLegacyRecords()
        {
            Debug.Log($"[Phase 4.3] Saving legacy records: {legacyRecord.TotalCompetitions} total competitions");
        }
    }
    
    // Additional supporting classes
    [System.Serializable]
    public class JudgeCredentials
    {
        public int YearsExperience;
        public string CertificationLevel;
        public int CompetitionsJudged;
        public float Reputation;
    }
    
    [System.Serializable]
    public class JudgePreferences
    {
        public List<CategoryType> PreferredCategories;
        public List<string> ConflictingCompetitors;
        public float MaxSessionLength;
    }
    
    [System.Serializable]
    public class JudgingEnvironment
    {
        public string Location;
        public float Temperature;
        public float Humidity;
        public string Lighting;
        public bool IsControlled;
    }
    
    [System.Serializable]
    public class CompetitionRules
    {
        public int MinimumAge;
        public bool LicenseRequired;
        public int MaxEntriesPerCompetitor;
        public string SampleSize;
        public string SubmissionDeadline;
        public List<string> DisqualificationCriteria;
        public string JudgingProtocol;
        public string AppealProcess;
    }
    
    [System.Serializable]
    public class CompetitionRewards
    {
        public float TotalPrizePool;
        public float OverallWinner;
        public float CategoryWinners;
        public float Runners_up;
        public float ParticipationReward;
        public Dictionary<string, float> SpecialAwards;
    }
    
    [System.Serializable]
    public class CategoryRewards
    {
        public float FirstPlace;
        public float SecondPlace;
        public float ThirdPlace;
    }
    
    [System.Serializable]
    public class EntryDocumentation
    {
        public List<string> GrowthPhotos;
        public string CultivationLog;
        public List<string> TestResults;
        public string BreederNotes;
    }
    
    [System.Serializable]
    public class CannabinoidProfile
    {
        public float THC;
        public float CBD;
        public float CBG;
        public float CBN;
        public float TotalCannabinoids;
    }
    
    [System.Serializable]
    public class TerpeneProfile
    {
        public List<string> DominantTerpenes;
        public Dictionary<string, float> TerpenePercentages;
        public float TotalTerpenes;
    }
    
    [System.Serializable]
    public class VisualQuality
    {
        public float OverallAppeal;
        public float TrichomeDensity;
        public float ColorVibrancy;
        public float StructuralIntegrity;
    }
    
    [System.Serializable]
    public class AromaProfile
    {
        public float Intensity;
        public float Complexity;
        public List<string> AromaNotes;
        public float Appeal;
    }
    
    [System.Serializable]
    public class TextureQuality
    {
        public float Density;
        public float Moisture;
        public float Stickiness;
        public float OverallFeel;
    }
    
    [System.Serializable]
    public class EnvironmentalData
    {
        public Dictionary<string, float> GrowConditions;
        public string CultivationMethod;
        public List<string> NutrientsUsed;
        public int DaysFlowering;
    }
    
    [System.Serializable]
    public class CompetitorStats
    {
        public float SeasonalPoints;
        public int ConsecutiveWins;
        public float HighestScore;
        public string BestCategory;
        public int YearsActive;
    }
    
    [System.Serializable]
    public class CompetitorRanking
    {
        public string CompetitorId;
        public int CurrentRank;
        public float Rating;
        public CompetitorLevel TierLevel;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class CompetitionHistory
    {
        public string CompetitionId;
        public string CompetitionName;
        public int Placement;
        public float Score;
        public DateTime Date;
    }
    
    [System.Serializable]
    public class CompetitionStatistics
    {
        public float AverageScore;
        public int TotalEntries;
        public Dictionary<string, int> CategoryBreakdown;
        public float CompetitiveIndex;
    }
    
    [System.Serializable]
    public class CategoryStatistics
    {
        public float ParticipationRate;
        public float QualityIndex;
        public float CompetitiveLevel;
    }
    
    [System.Serializable]
    public class OverallWinner
    {
        public string CompetitorId;
        public string EntryId;
        public int OverallPlacement;
        public float HighestCategoryScore;
        public string CategoryWon;
        public string SpecialRecognition;
    }
    
    [System.Serializable]
    public class LegendaryStrain
    {
        public string StrainName;
        public string GeneticLineage;
        public string OriginalBreeder;
        public float RecordScore;
        public string CompetitionWon;
        public string LegendStatus;
        public float ImpactRating;
    }
    
    [System.Serializable]
    public class ChampionBreeder
    {
        public string BreederId;
        public string Name;
        public int TotalWins;
        public List<string> SignatureStrains;
        public float LegacyRating;
    }
    
    [System.Serializable]
    public class CompetitionMilestone
    {
        public string Name;
        public string Description;
        public DateTime AchievedDate;
        public string CompetitionId;
    }
    
    [System.Serializable]
    public class CupEvolution
    {
        public List<string> StageProgression;
        public string CurrentStage;
        public string NextMilestone;
    }
    
    [System.Serializable]
    public class InnovationRecord
    {
        public string InnovationType;
        public string Description;
        public string Innovator;
        public DateTime IntroductionDate;
        public float AdoptionRate;
    }
    
    [System.Serializable]
    public class LegacyImpact
    {
        public float IndustryInfluence;
        public float InnovationLevel;
        public float CommunityRecognition;
    }
}