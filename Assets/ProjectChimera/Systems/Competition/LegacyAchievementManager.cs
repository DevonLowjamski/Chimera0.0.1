using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Achievements;
using AchievementRarity = ProjectChimera.Data.Achievements.AchievementRarity;
using AchievementReward = ProjectChimera.Data.Achievements.AchievementReward;
using AchievementDifficulty = ProjectChimera.Data.Achievements.AchievementDifficulty;
using AchievementCategory = ProjectChimera.Data.Achievements.AchievementCategory;
using AchievementType = ProjectChimera.Data.Achievements.AchievementType;
using Achievement = ProjectChimera.Data.Achievements.BaseAchievement;

namespace ProjectChimera.Systems.Competition
{
    /// <summary>
    /// Phase 4.3.b: Legacy and Achievement System
    /// Manages long-term player progression, achievements, and legacy tracking
    /// Creates meaningful endgame progression and community recognition systems
    /// </summary>
    public class LegacyAchievementManager : ChimeraManager
    {
        [Header("Phase 4.3.b Configuration")]
        public bool EnableAchievements = true;
        public bool EnableLegacyTracking = true;
        public bool EnableCommunityRecognition = true;
        public bool EnableMilestoneRewards = true;
        
        [Header("Achievement Settings")]
        [Range(1, 1000)] public int MaxAchievements = 500;
        [Range(0f, 100f)] public float DailyProgressBonus = 1.5f;
        [Range(1, 50)] public int MaxConcurrentChallenges = 10;
        public float AchievementCooldown = 86400f; // 24 hours
        
        [Header("Active Achievements")]
        [SerializeField] private List<BaseAchievement> unlockedAchievements = new List<BaseAchievement>();
        [SerializeField] private List<BaseAchievement> availableAchievements = new List<BaseAchievement>();
        [SerializeField] private List<AchievementProgress> activeProgress = new List<AchievementProgress>();
        [SerializeField] private Dictionary<string, LegacyMilestone> legacyMilestones = new Dictionary<string, LegacyMilestone>();
        
        [Header("Legacy System")]
        [SerializeField] private PlayerLegacy playerLegacy = new PlayerLegacy();
        [SerializeField] private List<LegacyContribution> contributions = new List<LegacyContribution>();
        [SerializeField] private Dictionary<string, CommunityRecognition> recognitions = new Dictionary<string, CommunityRecognition>();
        
        [Header("Milestone Rewards")]
        [SerializeField] private List<MilestoneReward> pendingRewards = new List<MilestoneReward>();
        [SerializeField] private List<UnlockableContent> unlockedContent = new List<UnlockableContent>();
        [SerializeField] private Dictionary<string, ReputationLevel> reputationLevels = new Dictionary<string, ReputationLevel>();
        
        // Phase 4.3.b Data Structures
        
        [System.Serializable]
        public class AchievementProgress
        {
            public string AchievementId;
            public Dictionary<string, float> RequirementProgress;
            public DateTime StartDate;
            public DateTime LastUpdate;
            public bool IsActive;
            public float TotalProgress;
            public List<string> CompletedRequirements;
            public string CurrentStage;
            public int AttemptsCount;
            public float BestScore;
            public Dictionary<string, object> SessionData;
        }
        
        
        [System.Serializable]
        public class PlayerLegacy
        {
            public string PlayerId;
            public string LegacyName;
            public DateTime StartDate;
            public LegacyRank CurrentRank;
            public long TotalExperience;
            public long LegacyPoints;
            public List<string> MajorAchievements;
            public List<string> Specializations;
            public Dictionary<string, float> MasteryLevels;
            public CommunityImpact Impact;
            public List<string> Mentorships;
            public List<string> Innovations;
            public Dictionary<string, LegacyMetric> Metrics;
            public LegacyStatus Status;
        }
        
        [System.Serializable]
        public class LegacyMilestone
        {
            public string MilestoneId;
            public string Title;
            public string Description;
            public MilestoneType Type;
            public DateTime AchievedDate;
            public List<string> RequiredAchievements;
            public long RequiredExperience;
            public Dictionary<string, float> RequiredMasteries;
            public List<MilestoneReward> Rewards;
            public bool IsAchieved;
            public float ProgressPercentage;
            public MilestoneTier Tier;
            public CommunityVisibility Visibility;
        }
        
        [System.Serializable]
        public class CommunityRecognition
        {
            public string RecognitionId;
            public string Title;
            public RecognitionType Type;
            public DateTime AwardedDate;
            public string AwardedBy;
            public string Reason;
            public RecognitionLevel Level;
            public List<string> Endorsements;
            public float CommunityScore;
            public bool IsPublic;
            public Dictionary<string, object> Metadata;
        }
        
        [System.Serializable]
        public class LegacyContribution
        {
            public string ContributionId;
            public ContributionType Type;
            public DateTime Date;
            public string Description;
            public float ImpactScore;
            public List<string> Beneficiaries;
            public ContributionScope Scope;
            public bool IsVerified;
            public List<string> Evidence;
            public Dictionary<string, object> Details;
        }
        
        [System.Serializable]
        public class MilestoneReward
        {
            public string RewardId;
            public string Name;
            public RewardType Type;
            public RewardRarity Rarity;
            public long Value;
            public List<string> UnlockableFeatures;
            public List<string> UnlockableContent;
            public Dictionary<string, float> StatBoosts;
            public bool IsExclusive;
            public DateTime ExpirationDate;
            public RewardCategory Category;
        }
        
        [System.Serializable]
        public class UnlockableContent
        {
            public string ContentId;
            public string Name;
            public ContentType Type;
            public string Description;
            public List<string> Requirements;
            public bool IsUnlocked;
            public DateTime UnlockDate;
            public ContentRarity Rarity;
            public List<string> Features;
            public Dictionary<string, object> Properties;
        }
        
        [System.Serializable]
        public class CommunityImpact
        {
            public float TotalImpactScore;
            public int PeopleMentored;
            public int InnovationsShared;
            public int KnowledgeContributions;
            public float CommunityReputation;
            public List<string> RecognizedContributions;
            public Dictionary<string, float> CategoryScores;
            public DateTime LastUpdate;
        }
        
        [System.Serializable]
        public class LegacyMetric
        {
            public string MetricName;
            public float CurrentValue;
            public float MaxValue;
            public float AverageValue;
            public DateTime LastUpdate;
            public List<float> HistoricalValues;
            public MetricTrend Trend;
            public float ImportanceWeight;
        }
        
        [System.Serializable]
        public class ReputationLevel
        {
            public string LevelId;
            public string Name;
            public int Level;
            public long RequiredExperience;
            public List<string> Benefits;
            public Dictionary<string, float> Bonuses;
            public bool IsAchieved;
            public DateTime AchievedDate;
            public ReputationType Type;
        }
        
        // Enums for Phase 4.3.b
        
        public enum LegacyRank
        {
            Novice, Apprentice, Journeyman, Expert, Master, Grandmaster, Legend
        }
        
        public enum MilestoneType
        {
            Experience, Achievement, Mastery, Innovation, Community, Competition
        }
        
        public enum MilestoneTier
        {
            Personal, Local, Regional, National, Global, Legendary
        }
        
        public enum RecognitionType
        {
            Peer, Community, Expert, Official, Achievement, Innovation
        }
        
        public enum RecognitionLevel
        {
            Acknowledgment, Appreciation, Excellence, Distinguished, Outstanding, Legendary
        }
        
        public enum ContributionType
        {
            Knowledge, Innovation, Mentorship, Community, Competition, Research
        }
        
        public enum ContributionScope
        {
            Individual, Team, Community, Industry, Global
        }
        
        public enum RewardType
        {
            Experience, Currency, Item, Feature, Access, Recognition, Boost
        }
        
        public enum RewardRarity
        {
            Common, Uncommon, Rare, Epic, Legendary, Exclusive
        }
        
        public enum RewardCategory
        {
            Progression, Cosmetic, Functional, Social, Economic, Special
        }
        
        public enum ContentType
        {
            Feature, Tool, Customization, Area, Mode, Social, Educational
        }
        
        public enum ContentRarity
        {
            Standard, Premium, Exclusive, Limited, Legendary, Unique
        }
        
        public enum LegacyStatus
        {
            Building, Established, Distinguished, Renowned, Legendary, Immortal
        }
        
        public enum CommunityVisibility
        {
            Private, Friends, Community, Public, Featured
        }
        
        public enum MetricTrend
        {
            Declining, Stable, Growing, Accelerating, Peak
        }
        
        public enum ReputationType
        {
            General, Cultivation, Genetics, Business, Community, Competition
        }
        
        protected override void OnManagerInitialize()
        {
            if (EnableAchievements)
            {
                InitializeAchievementSystem();
            }
            
            if (EnableLegacyTracking)
            {
                InitializeLegacySystem();
            }
            
            if (EnableCommunityRecognition)
            {
                InitializeCommunitySystem();
            }
            
            StartAchievementTracking();
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up achievement tracking
            CancelInvoke(nameof(UpdateAchievementProgress));
        }
        
        protected override void OnManagerUpdate()
        {
            if (EnableAchievements)
            {
                UpdateAchievementProgress();
                ProcessCompletedAchievements();
            }
            
            if (EnableLegacyTracking)
            {
                UpdateLegacyMetrics();
                CheckLegacyMilestones();
            }
            
            ProcessPendingRewards();
        }
        
        private void InitializeAchievementSystem()
        {
            // Phase 4.3.b: Initialize comprehensive achievement system
            LoadAchievementDefinitions();
            InitializePlayerProgress();
            SetupAchievementTrackers();
            
            Debug.Log($"Phase 4.3.b: Achievement system initialized with {availableAchievements.Count} achievements");
        }
        
        private void LoadAchievementDefinitions()
        {
            // Load achievement definitions from ScriptableObject database
            var achievementDatabase = Resources.Load<AchievementDatabaseSO>("AchievementDatabase");
            if (achievementDatabase != null)
            {
                availableAchievements.AddRange(achievementDatabase.AllAchievements);
            }
            
            // Create sample achievements for demonstration
            CreateSampleAchievements();
        }
        
        private void CreateSampleAchievements()
        {
            // Cultivation Achievements
            var firstHarvest = new BaseAchievement
            {
                Id = "first_harvest",
                AchievementId = "first_harvest",
                Name = "First Harvest",
                Description = "Successfully harvest your first cannabis plant",
                Category = AchievementCategory.Cultivation,
                Rarity = AchievementRarity.Common,
                Type = AchievementType.Standard,
                BaseExperienceReward = 100,
                Difficulty = AchievementDifficulty.Easy
            };
            
            var masterCultivator = new BaseAchievement
            {
                Id = "master_cultivator",
                AchievementId = "master_cultivator",
                Name = "Master Cultivator",
                Description = "Harvest 1000 plants with 95%+ quality rating",
                Category = AchievementCategory.Cultivation,
                Rarity = AchievementRarity.Legendary,
                Type = AchievementType.Progressive,
                BaseExperienceReward = 10000,
                Difficulty = AchievementDifficulty.Legendary,
                RequiredValue = 1000f
            };
            
            // Genetics Achievements
            var firstBreeding = new BaseAchievement
            {
                Id = "first_breeding",
                AchievementId = "first_breeding",
                Name = "Genetic Pioneer",
                Description = "Successfully breed your first new cannabis strain",
                Category = AchievementCategory.Genetics,
                Rarity = AchievementRarity.Uncommon,
                Type = AchievementType.Standard,
                BaseExperienceReward = 500,
                Difficulty = AchievementDifficulty.Normal
            };
            
            // Competition Achievements
            var cupWinner = new BaseAchievement
            {
                Id = "cup_winner",
                AchievementId = "cup_winner",
                Name = "Cannabis Cup Champion",
                Description = "Win first place in a Cannabis Cup competition",
                Category = AchievementCategory.Community, // Using Community since Competition doesn't exist in the enum
                Rarity = AchievementRarity.Epic,
                Type = AchievementType.Standard,
                BaseExperienceReward = 5000,
                Difficulty = AchievementDifficulty.Expert
            };
            
            // Community Achievements
            var mentor = new BaseAchievement
            {
                Id = "mentor_master",
                AchievementId = "mentor_master",
                Name = "Master Mentor",
                Description = "Successfully mentor 50 new cultivators",
                Category = AchievementCategory.Community,
                Rarity = AchievementRarity.Rare,
                Type = AchievementType.Progressive,
                BaseExperienceReward = 7500,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 50f
            };
            
            availableAchievements.AddRange(new[] { firstHarvest, masterCultivator, firstBreeding, cupWinner, mentor });
        }
        
        private void InitializePlayerProgress()
        {
            // Initialize progress tracking for all available achievements
            foreach (var achievement in availableAchievements)
            {
                if (!activeProgress.Any(p => p.AchievementId == achievement.Id))
                {
                    var progress = new AchievementProgress
                    {
                        AchievementId = achievement.Id,
                        RequirementProgress = new Dictionary<string, float>(),
                        StartDate = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        IsActive = true,
                        TotalProgress = 0f,
                        CompletedRequirements = new List<string>(),
                        SessionData = new Dictionary<string, object>()
                    };
                    
                    // Initialize requirement progress (simplified for BaseAchievement)
                    progress.RequirementProgress["main_progress"] = 0f;
                    
                    activeProgress.Add(progress);
                }
            }
        }
        
        private void SetupAchievementTrackers()
        {
            // Set up event listeners for achievement tracking
            // This would connect to game events like plant harvested, strain bred, competition won, etc.
        }
        
        private void InitializeLegacySystem()
        {
            // Phase 4.3.b: Initialize legacy tracking system
            if (playerLegacy.PlayerId == null)
            {
                playerLegacy = new PlayerLegacy
                {
                    PlayerId = SystemInfo.deviceUniqueIdentifier,
                    LegacyName = "Cultivation Legacy",
                    StartDate = DateTime.Now,
                    CurrentRank = LegacyRank.Novice,
                    TotalExperience = 0,
                    LegacyPoints = 0,
                    MajorAchievements = new List<string>(),
                    Specializations = new List<string>(),
                    MasteryLevels = new Dictionary<string, float>(),
                    Impact = new CommunityImpact
                    {
                        TotalImpactScore = 0f,
                        CategoryScores = new Dictionary<string, float>(),
                        LastUpdate = DateTime.Now
                    },
                    Metrics = new Dictionary<string, LegacyMetric>(),
                    Status = LegacyStatus.Building
                };
            }
            
            InitializeLegacyMilestones();
            InitializeMasteryTracking();
            
            Debug.Log("Phase 4.3.b: Legacy system initialized");
        }
        
        private void InitializeLegacyMilestones()
        {
            // Create sample legacy milestones
            var noviceComplete = new LegacyMilestone
            {
                MilestoneId = "novice_complete",
                Title = "Novice Cultivator",
                Description = "Complete all novice-level achievements",
                Type = MilestoneType.Achievement,
                RequiredExperience = 1000,
                Tier = MilestoneTier.Personal,
                Visibility = CommunityVisibility.Private,
                Rewards = new List<MilestoneReward>
                {
                    new MilestoneReward
                    {
                        RewardId = "novice_badge",
                        Name = "Novice Badge",
                        Type = RewardType.Recognition,
                        Rarity = RewardRarity.Common,
                        Category = RewardCategory.Social
                    }
                }
            };
            
            var masterBreeder = new LegacyMilestone
            {
                MilestoneId = "master_breeder",
                Title = "Master Breeder",
                Description = "Achieve mastery in genetic cultivation",
                Type = MilestoneType.Mastery,
                RequiredMasteries = new Dictionary<string, float> { { "genetics", 0.9f } },
                Tier = MilestoneTier.Regional,
                Visibility = CommunityVisibility.Community
            };
            
            legacyMilestones["novice_complete"] = noviceComplete;
            legacyMilestones["master_breeder"] = masterBreeder;
        }
        
        private void InitializeMasteryTracking()
        {
            // Initialize mastery levels for different cultivation areas
            string[] masteryAreas = { "cultivation", "genetics", "economics", "competition", "community", "innovation" };
            
            foreach (var area in masteryAreas)
            {
                if (!playerLegacy.MasteryLevels.ContainsKey(area))
                {
                    playerLegacy.MasteryLevels[area] = 0f;
                }
                
                playerLegacy.Metrics[area] = new LegacyMetric
                {
                    MetricName = area,
                    CurrentValue = 0f,
                    MaxValue = 1f,
                    AverageValue = 0f,
                    LastUpdate = DateTime.Now,
                    HistoricalValues = new List<float>(),
                    Trend = MetricTrend.Stable,
                    ImportanceWeight = 1f
                };
            }
        }
        
        private void InitializeCommunitySystem()
        {
            // Phase 4.3.b: Initialize community recognition system
            InitializeReputationLevels();
            
            Debug.Log("Phase 4.3.b: Community recognition system initialized");
        }
        
        private void InitializeReputationLevels()
        {
            // Initialize reputation levels for different areas
            var reputationAreas = new Dictionary<string, ReputationType>
            {
                { "general", ReputationType.General },
                { "cultivation", ReputationType.Cultivation },
                { "genetics", ReputationType.Genetics },
                { "business", ReputationType.Business },
                { "community", ReputationType.Community },
                { "competition", ReputationType.Competition }
            };
            
            foreach (var area in reputationAreas)
            {
                for (int level = 1; level <= 10; level++)
                {
                    var reputationLevel = new ReputationLevel
                    {
                        LevelId = $"{area.Key}_level_{level}",
                        Name = $"{area.Key.ToTitleCase()} Level {level}",
                        Level = level,
                        RequiredExperience = level * 1000,
                        Benefits = new List<string>(),
                        Bonuses = new Dictionary<string, float>(),
                        IsAchieved = false,
                        Type = area.Value
                    };
                    
                    reputationLevels[reputationLevel.LevelId] = reputationLevel;
                }
            }
        }
        
        private void StartAchievementTracking()
        {
            // Start background achievement tracking
            InvokeRepeating(nameof(UpdateAchievementProgress), 1f, 5f); // Update every 5 seconds
        }
        
        private void UpdateAchievementProgress()
        {
            if (!EnableAchievements) return;
            
            foreach (var progress in activeProgress)
            {
                if (!progress.IsActive) continue;
                
                var achievement = availableAchievements.FirstOrDefault(a => a.Id == progress.AchievementId);
                if (achievement == null) continue;
                
                UpdateIndividualAchievementProgress(achievement, progress);
            }
        }
        
        private void UpdateIndividualAchievementProgress(BaseAchievement achievement, AchievementProgress progress)
        {
            // Simplified progress tracking for demonstration
            float currentValue = GetCurrentValueForAchievement(achievement);
            progress.TotalProgress = Mathf.Clamp01(currentValue / achievement.RequiredValue);
            progress.LastUpdate = DateTime.Now;
            
            // Check if achievement is completed
            if (progress.TotalProgress >= 1f && !unlockedAchievements.Any(a => a.Id == achievement.Id))
            {
                CompleteAchievement(achievement);
            }
        }
        
        private float GetCurrentValueForAchievement(BaseAchievement achievement)
        {
            // This would interface with game statistics to get current values
            // For now, return placeholder values based on achievement ID
            switch (achievement.Id)
            {
                case "first_harvest":
                    return 0f; // Get from cultivation manager
                case "master_cultivator":
                    return 0f; // Get from harvest data
                case "first_breeding":
                    return 0f; // Get from genetics manager
                case "cup_winner":
                    return 0f; // Get from competition manager
                case "mentor_master":
                    return 0f; // Get from community manager
                default:
                    return 0f;
            }
        }
        
        private void CompleteAchievement(BaseAchievement achievement)
        {
            unlockedAchievements.Add(achievement);
            
            // Award experience and rewards
            playerLegacy.TotalExperience += achievement.BaseExperienceReward;
            playerLegacy.LegacyPoints += CalculateLegacyPoints(achievement);
            
            // Update legacy milestones
            UpdateLegacyProgress(achievement);
            
            // Trigger achievement unlocked event
            OnAchievementUnlocked?.Invoke(achievement);
            
            Debug.Log($"Achievement unlocked: {achievement.Name}");
        }
        
        private long CalculateLegacyPoints(BaseAchievement achievement)
        {
            long basePoints = achievement.BaseExperienceReward / 10;
            float rarityMultiplier = GetRarityMultiplier(achievement.Rarity);
            float difficultyMultiplier = GetDifficultyMultiplier(achievement.Difficulty);
            
            return (long)(basePoints * rarityMultiplier * difficultyMultiplier);
        }
        
        private float GetRarityMultiplier(AchievementRarity rarity)
        {
            return rarity switch
            {
                AchievementRarity.Common => 1f,
                AchievementRarity.Uncommon => 1.5f,
                AchievementRarity.Rare => 2f,
                AchievementRarity.Epic => 3f,
                AchievementRarity.Legendary => 5f,
                AchievementRarity.Mythical => 8f,
                AchievementRarity.Divine => 10f,
                _ => 1f
            };
        }
        
        private float GetDifficultyMultiplier(AchievementDifficulty difficulty)
        {
            return difficulty switch
            {
                AchievementDifficulty.Trivial => 0.5f,
                AchievementDifficulty.Easy => 0.8f,
                AchievementDifficulty.Normal => 1f,
                AchievementDifficulty.Hard => 1.5f,
                AchievementDifficulty.Expert => 2f,
                AchievementDifficulty.Legendary => 3f,
                AchievementDifficulty.Impossible => 5f,
                _ => 1f
            };
        }
        
        private void ProcessAchievementReward(AchievementReward reward)
        {
            // Process different types of achievement rewards
            var milestoneReward = new MilestoneReward
            {
                RewardId = Guid.NewGuid().ToString(),
                Name = reward.Name,
                Type = RewardType.Experience, // Map from achievement reward type
                Value = reward.Value,
                Category = RewardCategory.Progression
            };
            
            pendingRewards.Add(milestoneReward);
        }
        
        private void UpdateLegacyProgress(BaseAchievement achievement)
        {
            // Update specializations based on achievement category
            string specialization = achievement.Category.ToString().ToLower();
            if (!playerLegacy.Specializations.Contains(specialization))
            {
                // Check if enough achievements in this category to consider it a specialization
                int categoryAchievements = unlockedAchievements.Count(a => a.Category == achievement.Category);
                if (categoryAchievements >= 5) // Threshold for specialization
                {
                    playerLegacy.Specializations.Add(specialization);
                }
            }
            
            // Update mastery levels
            if (playerLegacy.MasteryLevels.ContainsKey(specialization))
            {
                float masteryIncrease = CalculateMasteryIncrease(achievement);
                playerLegacy.MasteryLevels[specialization] = Mathf.Clamp01(
                    playerLegacy.MasteryLevels[specialization] + masteryIncrease
                );
            }
            
            // Add to major achievements if it's significant
            if (achievement.Rarity >= AchievementRarity.Epic)
            {
                playerLegacy.MajorAchievements.Add(achievement.Id);
            }
            
            // Update legacy rank based on total experience
            UpdateLegacyRank();
        }
        
        private float CalculateMasteryIncrease(BaseAchievement achievement)
        {
            float baseIncrease = 0.01f; // 1% base increase
            float rarityMultiplier = GetRarityMultiplier(achievement.Rarity);
            float difficultyMultiplier = GetDifficultyMultiplier(achievement.Difficulty);
            
            return baseIncrease * rarityMultiplier * difficultyMultiplier;
        }
        
        private void UpdateLegacyRank()
        {
            var newRank = playerLegacy.TotalExperience switch
            {
                < 1000 => LegacyRank.Novice,
                < 5000 => LegacyRank.Apprentice,
                < 15000 => LegacyRank.Journeyman,
                < 50000 => LegacyRank.Expert,
                < 150000 => LegacyRank.Master,
                < 500000 => LegacyRank.Grandmaster,
                _ => LegacyRank.Legend
            };
            
            if (newRank > playerLegacy.CurrentRank)
            {
                playerLegacy.CurrentRank = newRank;
                OnLegacyRankUp?.Invoke(newRank);
                Debug.Log($"Legacy rank increased to: {newRank}");
            }
        }
        
        private void ProcessCompletedAchievements()
        {
            // Process any post-completion logic for achievements
            foreach (var achievement in unlockedAchievements.Where(a => a.IsRepeatable))
            {
                // Reset repeatable achievements if conditions are met
                if (ShouldResetRepeatable(achievement))
                {
                    ResetRepeatableAchievement(achievement);
                }
            }
        }
        
        private bool ShouldResetRepeatable(BaseAchievement achievement)
        {
            // Check if enough time has passed for repeatable achievements
            // Since BaseAchievement doesn't have UnlockDate, we'll use a simple time-based check
            return true; // Simplified for demonstration
        }
        
        private void ResetRepeatableAchievement(BaseAchievement achievement)
        {
            var progress = activeProgress.FirstOrDefault(p => p.AchievementId == achievement.Id);
            if (progress != null)
            {
                progress.TotalProgress = 0f;
                progress.CompletedRequirements.Clear();
                progress.IsActive = true;
                progress.StartDate = DateTime.Now;
                progress.LastUpdate = DateTime.Now;
                
                foreach (var reqId in progress.RequirementProgress.Keys.ToList())
                {
                    progress.RequirementProgress[reqId] = 0f;
                }
            }
        }
        
        private void UpdateLegacyMetrics()
        {
            if (!EnableLegacyTracking) return;
            
            foreach (var metric in playerLegacy.Metrics.Values)
            {
                UpdateIndividualMetric(metric);
            }
            
            // Update community impact
            UpdateCommunityImpact();
        }
        
        private void UpdateIndividualMetric(LegacyMetric metric)
        {
            // Get current value for this metric
            float newValue = GetCurrentMetricValue(metric.MetricName);
            
            // Store historical value
            metric.HistoricalValues.Add(metric.CurrentValue);
            if (metric.HistoricalValues.Count > 100) // Keep last 100 values
            {
                metric.HistoricalValues.RemoveAt(0);
            }
            
            // Update values
            metric.CurrentValue = newValue;
            metric.MaxValue = Mathf.Max(metric.MaxValue, newValue);
            metric.AverageValue = metric.HistoricalValues.Average();
            metric.LastUpdate = DateTime.Now;
            
            // Calculate trend
            if (metric.HistoricalValues.Count >= 5)
            {
                var recentValues = metric.HistoricalValues.TakeLast(5).ToList();
                var trend = CalculateTrend(recentValues);
                metric.Trend = trend;
            }
        }
        
        private float GetCurrentMetricValue(string metricName)
        {
            // This would interface with various game systems to get current metric values
            return metricName switch
            {
                "cultivation" => playerLegacy.MasteryLevels.GetValueOrDefault("cultivation", 0f),
                "genetics" => playerLegacy.MasteryLevels.GetValueOrDefault("genetics", 0f),
                "economics" => playerLegacy.MasteryLevels.GetValueOrDefault("economics", 0f),
                "competition" => playerLegacy.MasteryLevels.GetValueOrDefault("competition", 0f),
                "community" => playerLegacy.MasteryLevels.GetValueOrDefault("community", 0f),
                "innovation" => playerLegacy.MasteryLevels.GetValueOrDefault("innovation", 0f),
                _ => 0f
            };
        }
        
        private MetricTrend CalculateTrend(List<float> values)
        {
            if (values.Count < 2) return MetricTrend.Stable;
            
            float firstHalf = values.Take(values.Count / 2).Average();
            float secondHalf = values.Skip(values.Count / 2).Average();
            float change = secondHalf - firstHalf;
            
            return change switch
            {
                < -0.1f => MetricTrend.Declining,
                < -0.01f => MetricTrend.Stable,
                < 0.1f => MetricTrend.Growing,
                < 0.2f => MetricTrend.Accelerating,
                _ => MetricTrend.Peak
            };
        }
        
        private void UpdateCommunityImpact()
        {
            // Calculate total community impact based on various factors
            float knowledgeScore = contributions.Where(c => c.Type == ContributionType.Knowledge).Sum(c => c.ImpactScore);
            float mentorshipScore = contributions.Where(c => c.Type == ContributionType.Mentorship).Sum(c => c.ImpactScore);
            float innovationScore = contributions.Where(c => c.Type == ContributionType.Innovation).Sum(c => c.ImpactScore);
            float communityScore = contributions.Where(c => c.Type == ContributionType.Community).Sum(c => c.ImpactScore);
            
            playerLegacy.Impact.CategoryScores["knowledge"] = knowledgeScore;
            playerLegacy.Impact.CategoryScores["mentorship"] = mentorshipScore;
            playerLegacy.Impact.CategoryScores["innovation"] = innovationScore;
            playerLegacy.Impact.CategoryScores["community"] = communityScore;
            
            playerLegacy.Impact.TotalImpactScore = playerLegacy.Impact.CategoryScores.Values.Sum();
            playerLegacy.Impact.LastUpdate = DateTime.Now;
        }
        
        private void CheckLegacyMilestones()
        {
            foreach (var milestone in legacyMilestones.Values.Where(m => !m.IsAchieved))
            {
                if (EvaluateMilestoneRequirements(milestone))
                {
                    CompleteMilestone(milestone);
                }
                else
                {
                    UpdateMilestoneProgress(milestone);
                }
            }
        }
        
        private bool EvaluateMilestoneRequirements(LegacyMilestone milestone)
        {
            // Check experience requirements
            if (milestone.RequiredExperience > 0 && playerLegacy.TotalExperience < milestone.RequiredExperience)
            {
                return false;
            }
            
            // Check mastery requirements
            if (milestone.RequiredMasteries != null)
            {
                foreach (var mastery in milestone.RequiredMasteries)
                {
                    if (!playerLegacy.MasteryLevels.ContainsKey(mastery.Key) ||
                        playerLegacy.MasteryLevels[mastery.Key] < mastery.Value)
                    {
                        return false;
                    }
                }
            }
            
            // Check required achievements
            if (milestone.RequiredAchievements != null)
            {
                foreach (var requiredAchievement in milestone.RequiredAchievements)
                {
                    if (!unlockedAchievements.Any(a => a.AchievementId == requiredAchievement))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private void UpdateMilestoneProgress(LegacyMilestone milestone)
        {
            float totalProgress = 0f;
            int requirementCount = 0;
            
            // Experience progress
            if (milestone.RequiredExperience > 0)
            {
                totalProgress += Mathf.Clamp01((float)playerLegacy.TotalExperience / milestone.RequiredExperience);
                requirementCount++;
            }
            
            // Mastery progress
            if (milestone.RequiredMasteries != null)
            {
                foreach (var mastery in milestone.RequiredMasteries)
                {
                    float currentMastery = playerLegacy.MasteryLevels.GetValueOrDefault(mastery.Key, 0f);
                    totalProgress += Mathf.Clamp01(currentMastery / mastery.Value);
                    requirementCount++;
                }
            }
            
            // Achievement progress
            if (milestone.RequiredAchievements != null)
            {
                int completedAchievements = milestone.RequiredAchievements.Count(
                    req => unlockedAchievements.Any(a => a.AchievementId == req)
                );
                totalProgress += (float)completedAchievements / milestone.RequiredAchievements.Count;
                requirementCount++;
            }
            
            milestone.ProgressPercentage = requirementCount > 0 ? totalProgress / requirementCount : 0f;
        }
        
        private void CompleteMilestone(LegacyMilestone milestone)
        {
            milestone.IsAchieved = true;
            milestone.AchievedDate = DateTime.Now;
            milestone.ProgressPercentage = 1f;
            
            // Process milestone rewards
            if (milestone.Rewards != null)
            {
                pendingRewards.AddRange(milestone.Rewards);
            }
            
            OnMilestoneCompleted?.Invoke(milestone);
            Debug.Log($"Legacy milestone completed: {milestone.Title}");
        }
        
        private void ProcessPendingRewards()
        {
            var rewardsToProcess = pendingRewards.ToList();
            pendingRewards.Clear();
            
            foreach (var reward in rewardsToProcess)
            {
                ProcessMilestoneReward(reward);
            }
        }
        
        private void ProcessMilestoneReward(MilestoneReward reward)
        {
            switch (reward.Type)
            {
                case RewardType.Experience:
                    playerLegacy.TotalExperience += reward.Value;
                    break;
                    
                case RewardType.Currency:
                    // Award currency through economic system
                    break;
                    
                case RewardType.Feature:
                    // Unlock new features
                    if (reward.UnlockableFeatures != null)
                    {
                        foreach (var feature in reward.UnlockableFeatures)
                        {
                            UnlockFeature(feature);
                        }
                    }
                    break;
                    
                case RewardType.Access:
                    // Grant access to new areas or content
                    if (reward.UnlockableContent != null)
                    {
                        foreach (var content in reward.UnlockableContent)
                        {
                            UnlockContent(content);
                        }
                    }
                    break;
            }
            
            OnRewardProcessed?.Invoke(reward);
        }
        
        private void UnlockFeature(string featureId)
        {
            var feature = new UnlockableContent
            {
                ContentId = featureId,
                Name = featureId.ToTitleCase(),
                Type = ContentType.Feature,
                IsUnlocked = true,
                UnlockDate = DateTime.Now,
                Rarity = ContentRarity.Standard
            };
            
            unlockedContent.Add(feature);
            Debug.Log($"Feature unlocked: {feature.Name}");
        }
        
        private void UnlockContent(string contentId)
        {
            var content = new UnlockableContent
            {
                ContentId = contentId,
                Name = contentId.ToTitleCase(),
                Type = ContentType.Area,
                IsUnlocked = true,
                UnlockDate = DateTime.Now,
                Rarity = ContentRarity.Standard
            };
            
            unlockedContent.Add(content);
            Debug.Log($"Content unlocked: {content.Name}");
        }
        
        // Public API for other systems
        public void RecordPlayerAction(string actionType, string target, float value, Dictionary<string, object> metadata = null)
        {
            // This method would be called by other systems to record player actions for achievement tracking
            // Implementation would update relevant achievement progress based on the action
        }
        
        public void AddCommunityContribution(ContributionType type, string description, float impactScore, List<string> beneficiaries = null)
        {
            var contribution = new LegacyContribution
            {
                ContributionId = Guid.NewGuid().ToString(),
                Type = type,
                Date = DateTime.Now,
                Description = description,
                ImpactScore = impactScore,
                Beneficiaries = beneficiaries ?? new List<string>(),
                Scope = DetermineContributionScope(impactScore, beneficiaries?.Count ?? 0),
                IsVerified = true
            };
            
            contributions.Add(contribution);
            UpdateCommunityImpact();
        }
        
        private ContributionScope DetermineContributionScope(float impactScore, int beneficiaryCount)
        {
            return (impactScore, beneficiaryCount) switch
            {
                (< 10, < 5) => ContributionScope.Individual,
                (< 50, < 20) => ContributionScope.Team,
                (< 200, < 100) => ContributionScope.Community,
                (< 1000, < 500) => ContributionScope.Industry,
                _ => ContributionScope.Global
            };
        }
        
        public void AwardCommunityRecognition(RecognitionType type, string title, string reason, RecognitionLevel level)
        {
            var recognition = new CommunityRecognition
            {
                RecognitionId = Guid.NewGuid().ToString(),
                Title = title,
                Type = type,
                AwardedDate = DateTime.Now,
                Reason = reason,
                Level = level,
                Endorsements = new List<string>(),
                CommunityScore = CalculateCommunityScore(level),
                IsPublic = true
            };
            
            recognitions[recognition.RecognitionId] = recognition;
            OnRecognitionAwarded?.Invoke(recognition);
        }
        
        private float CalculateCommunityScore(RecognitionLevel level)
        {
            return level switch
            {
                RecognitionLevel.Acknowledgment => 10f,
                RecognitionLevel.Appreciation => 25f,
                RecognitionLevel.Excellence => 50f,
                RecognitionLevel.Distinguished => 100f,
                RecognitionLevel.Outstanding => 200f,
                RecognitionLevel.Legendary => 500f,
                _ => 0f
            };
        }
        
        // Events for other systems to subscribe to
        public System.Action<BaseAchievement> OnAchievementUnlocked;
        public System.Action<LegacyMilestone> OnMilestoneCompleted;
        public System.Action<LegacyRank> OnLegacyRankUp;
        public System.Action<MilestoneReward> OnRewardProcessed;
        public System.Action<CommunityRecognition> OnRecognitionAwarded;
        
        // Query methods for UI and other systems
        public List<BaseAchievement> GetUnlockedAchievements() => unlockedAchievements;
        public List<BaseAchievement> GetAvailableAchievements() => availableAchievements;
        public PlayerLegacy GetPlayerLegacy() => playerLegacy;
        public List<LegacyMilestone> GetCompletedMilestones() => legacyMilestones.Values.Where(m => m.IsAchieved).ToList();
        public List<LegacyMilestone> GetAvailableMilestones() => legacyMilestones.Values.Where(m => !m.IsAchieved).ToList();
        public List<UnlockableContent> GetUnlockedContent() => unlockedContent;
        public Dictionary<string, CommunityRecognition> GetCommunityRecognitions() => recognitions;
        
        public float GetAchievementProgress(string achievementId)
        {
            return activeProgress.FirstOrDefault(p => p.AchievementId == achievementId)?.TotalProgress ?? 0f;
        }
        
        public AchievementProgress GetDetailedProgress(string achievementId)
        {
            return activeProgress.FirstOrDefault(p => p.AchievementId == achievementId);
        }
        
        public Dictionary<string, float> GetMasteryLevels() => playerLegacy.MasteryLevels;
        
        public float GetOverallProgress()
        {
            if (availableAchievements.Count == 0) return 0f;
            return (float)unlockedAchievements.Count / availableAchievements.Count;
        }
    }

    // Extension method for string formatting
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input[1..].ToLower();
        }
    }
    
}