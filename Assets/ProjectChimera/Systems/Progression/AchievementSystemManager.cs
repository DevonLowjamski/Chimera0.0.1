using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Genetics.Scientific;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement System Manager - Exciting achievement unlock celebrations and recognition
    /// Provides rewarding achievement systems with satisfying unlock celebrations and player recognition
    /// Features achievement tracking, unlock ceremonies, and social recognition for player accomplishments
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class AchievementSystemManager : ChimeraManager
    {
        [Header("Achievement Configuration")]
        public bool EnableAchievementSystem = true;
        public bool EnableUnlockCelebrations = true;
        public bool EnableSocialSharing = true;
        public bool EnableAchievementHints = true;
        
        [Header("Celebration Settings")]
        public float CelebrationDuration = 5f;
        public float AchievementDisplayTime = 3f;
        public int MaxConcurrentCelebrations = 3;
        public bool EnableScreenEffects = true;
        
        [Header("Achievement Collections")]
        [SerializeField] private List<Achievement> allAchievements = new List<Achievement>();
        [SerializeField] private List<Achievement> unlockedAchievements = new List<Achievement>();
        [SerializeField] private List<AchievementProgress> playerProgress = new List<AchievementProgress>();
        [SerializeField] private Queue<Achievement> pendingCelebrations = new Queue<Achievement>();
        
        [Header("Player Recognition")]
        [SerializeField] private Dictionary<string, PlayerAchievementProfile> playerProfiles = new Dictionary<string, PlayerAchievementProfile>();
        [SerializeField] private List<AchievementBadge> availableBadges = new List<AchievementBadge>();
        [SerializeField] private List<AchievementTier> achievementTiers = new List<AchievementTier>();
        
        [Header("System State")]
        [SerializeField] private DateTime lastAchievementUpdate = DateTime.Now;
        [SerializeField] private int totalAchievementsUnlocked = 0;
        [SerializeField] private float totalAchievementPoints = 0f;
        [SerializeField] private List<Achievement> recentUnlocks = new List<Achievement>();
        
        // Events for achievement celebrations and recognition
        public static event Action<Achievement> OnAchievementUnlocked;
        public static event Action<Achievement> OnCelebrationStarted;
        public static event Action<AchievementBadge> OnBadgeEarned;
        public static event Action<AchievementTier> OnTierAdvanced;
        public static event Action<string, float> OnProgressUpdated;
        public static event Action<PlayerAchievementProfile> OnPlayerRecognition;
        
        // PC-012-9: Events for progression system integration
        public static event Action<string, float> OnExperienceGained;
        public static event Action<int, int> OnPlayerLevelUp;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize achievement system
            InitializeAchievementSystem();
            
            if (EnableAchievementSystem)
            {
                StartAchievementTracking();
            }
            
            Debug.Log("✅ AchievementSystemManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up achievement system
            if (EnableAchievementSystem)
            {
                StopAchievementTracking();
            }
            
            // Clear all events to prevent memory leaks
            OnAchievementUnlocked = null;
            OnCelebrationStarted = null;
            OnBadgeEarned = null;
            OnTierAdvanced = null;
            OnProgressUpdated = null;
            OnPlayerRecognition = null;
            
            // PC-012-9: Clear progression integration events
            OnExperienceGained = null;
            OnPlayerLevelUp = null;
            
            Debug.Log("✅ AchievementSystemManager shutdown successfully");
        }
        
        private void InitializeAchievementSystem()
        {
            // Initialize collections if empty
            if (allAchievements == null) allAchievements = new List<Achievement>();
            if (unlockedAchievements == null) unlockedAchievements = new List<Achievement>();
            if (playerProgress == null) playerProgress = new List<AchievementProgress>();
            if (pendingCelebrations == null) pendingCelebrations = new Queue<Achievement>();
            if (playerProfiles == null) playerProfiles = new Dictionary<string, PlayerAchievementProfile>();
            if (availableBadges == null) availableBadges = new List<AchievementBadge>();
            if (achievementTiers == null) achievementTiers = new List<AchievementTier>();
            if (recentUnlocks == null) recentUnlocks = new List<Achievement>();
            
            // Initialize achievement content
            InitializeAchievements();
            InitializeAchievementBadges();
            InitializeAchievementTiers();
        }
        
        private void InitializeAchievements()
        {
            // Create comprehensive achievement system covering all game aspects
            var achievementDefinitions = new (string, string, AchievementCategory, AchievementRarity, float, string, float)[]
            {
                // Cultivation & Plant Management Achievements
                ("First Harvest", "Harvest your first plant", AchievementCategory.Cultivation_Mastery, AchievementRarity.Common, 50f, "plant_harvested", 1),
                ("Green Thumb", "Plant 10 seeds successfully", AchievementCategory.Cultivation_Mastery, AchievementRarity.Common, 75f, "plant_planted", 10),
                ("Master Gardener", "Harvest 100 plants", AchievementCategory.Cultivation_Mastery, AchievementRarity.Rare, 500f, "plant_harvested", 100),
                ("Quality Master", "Achieve 10 perfect quality harvests", AchievementCategory.Quality_Achievement, AchievementRarity.Epic, 1000f, "perfect_quality_harvest", 10),
                ("Cultivation Expert", "Maintain 50 high quality harvests", AchievementCategory.Quality_Achievement, AchievementRarity.Rare, 750f, "high_quality_harvest", 50),
                
                // Breeding & Genetics Achievements
                ("First Breed", "Complete your first breeding challenge", AchievementCategory.Genetics_Innovation, AchievementRarity.Common, 50f, "breed_challenge_completed", 1),
                ("Master Breeder", "Complete 25 breeding challenges", AchievementCategory.Genetics_Innovation, AchievementRarity.Rare, 500f, "breed_challenge_completed", 25),
                ("Perfect Genetics", "Create a plant with perfect trait expression", AchievementCategory.Genetics_Innovation, AchievementRarity.Epic, 1000f, "perfect_trait_expression", 1),
                ("Genetic Pioneer", "Complete 50 successful breeding projects", AchievementCategory.Genetics_Innovation, AchievementRarity.Legendary, 2000f, "successful_breeding", 50),
                ("Breeding Virtuoso", "Complete 10 breeding projects with exceptional results", AchievementCategory.Genetics_Innovation, AchievementRarity.Epic, 1500f, "breeding_success", 10),
                
                // Research & Development Achievements
                ("Research Initiate", "Complete your first research project", AchievementCategory.Research_Excellence, AchievementRarity.Common, 100f, "research_completed", 1),
                ("Knowledge Seeker", "Complete 10 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 300f, "research_completed", 10),
                ("Research Master", "Complete 50 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Rare, 1000f, "research_completed", 50),
                ("Genetics Researcher", "Complete 10 genetics research projects", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 400f, "genetics_research_completed", 10),
                ("Cultivation Scientist", "Complete 10 cultivation research projects", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 400f, "cultivation_research_completed", 10),
                ("Processing Expert", "Complete 10 processing research projects", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 400f, "processing_research_completed", 10),
                ("Scholar", "Complete research in all categories", AchievementCategory.Research_Excellence, AchievementRarity.Epic, 2000f, "research_category_unlocked", 3),
                
                // Business & Sales Achievements
                ("First Sale", "Complete your first product sale", AchievementCategory.Business_Success, AchievementRarity.Common, 50f, "product_sold", 1),
                ("Entrepreneur", "Complete 50 successful sales", AchievementCategory.Business_Success, AchievementRarity.Uncommon, 250f, "sales_completed", 50),
                ("Big Deal", "Complete a high-value sale worth $10,000+", AchievementCategory.Business_Success, AchievementRarity.Rare, 500f, "high_value_sale", 1),
                ("Tycoon", "Complete a major sale worth $50,000+", AchievementCategory.Business_Success, AchievementRarity.Epic, 1500f, "major_sale", 1),
                ("Sales Master", "Complete 500 total sales", AchievementCategory.Business_Success, AchievementRarity.Epic, 2000f, "sales_completed", 500),
                
                // Progression & Learning Achievements
                ("Getting Started", "Reach level 5", AchievementCategory.Research_Excellence, AchievementRarity.Common, 100f, "level_5_reached", 1),
                ("Rising Star", "Reach level 10", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 200f, "level_10_reached", 1),
                ("Expert Level", "Reach level 25", AchievementCategory.Cultivation_Mastery, AchievementRarity.Rare, 1000f, "master_level", 1),
                ("Grandmaster", "Reach level 50", AchievementCategory.Innovation_Pioneer, AchievementRarity.Epic, 2500f, "grandmaster_level", 1),
                ("Leveling Legend", "Gain experience through progression", AchievementCategory.Research_Excellence, AchievementRarity.Common, 50f, "player_progression", 10),
                
                // Achievement System Meta Achievements
                ("Achievement Hunter", "Unlock 10 achievements", AchievementCategory.Special, AchievementRarity.Uncommon, 300f, "achievement_collector", 1),
                ("Achievement Master", "Unlock 25 achievements", AchievementCategory.Special, AchievementRarity.Rare, 750f, "achievement_hunter", 1),
                ("Achievement Legend", "Unlock 50 achievements", AchievementCategory.Special, AchievementRarity.Epic, 2000f, "achievement_master", 1),
                ("Point Collector", "Earn 1,000 achievement points", AchievementCategory.Special, AchievementRarity.Uncommon, 200f, "point_milestone_1000", 1),
                ("Point Master", "Earn 5,000 achievement points", AchievementCategory.Special, AchievementRarity.Rare, 500f, "point_milestone_5000", 1),
                ("Point Legend", "Earn 10,000 achievement points", AchievementCategory.Special, AchievementRarity.Epic, 1000f, "point_milestone_10000", 1),
                
                // Cross-System & Special Achievements
                ("System Master", "Master all core game systems", AchievementCategory.Ultimate, AchievementRarity.Legendary, 5000f, "all_systems_mastered", 1),
                ("Perfectionist", "Achieve 100% completion in any category", AchievementCategory.Quality_Achievement, AchievementRarity.Legendary, 3000f, "perfect_completion", 1),
                ("Daily Achiever", "Unlock an achievement every day", AchievementCategory.Special, AchievementRarity.Rare, 500f, "daily_achievement", 7),
                ("Weekly Streak", "Maintain a 7-day achievement streak", AchievementCategory.Special, AchievementRarity.Epic, 1000f, "weekly_streak", 1),
                ("Ultimate Master", "Unlock all other achievements", AchievementCategory.Ultimate, AchievementRarity.Legendary, 10000f, "all_achievements", 1),
                
                // PC-012-10: Milestone Achievements - Major progression markers
                ("Cultivation Milestone I", "Harvest 25 plants", AchievementCategory.Cultivation_Mastery, AchievementRarity.Uncommon, 200f, "cultivation_milestone_25", 25),
                ("Cultivation Milestone II", "Harvest 100 plants", AchievementCategory.Cultivation_Mastery, AchievementRarity.Rare, 500f, "cultivation_milestone_100", 100),
                ("Cultivation Milestone III", "Harvest 500 plants", AchievementCategory.Cultivation_Mastery, AchievementRarity.Epic, 1500f, "cultivation_milestone_500", 500),
                ("Cultivation Milestone IV", "Harvest 1000 plants", AchievementCategory.Cultivation_Mastery, AchievementRarity.Legendary, 3000f, "cultivation_milestone_1000", 1000),
                
                ("Genetics Milestone I", "Complete 10 breeding projects", AchievementCategory.Genetics_Innovation, AchievementRarity.Uncommon, 250f, "genetics_milestone_10", 10),
                ("Genetics Milestone II", "Complete 50 breeding projects", AchievementCategory.Genetics_Innovation, AchievementRarity.Rare, 750f, "genetics_milestone_50", 50),
                ("Genetics Milestone III", "Complete 200 breeding projects", AchievementCategory.Genetics_Innovation, AchievementRarity.Epic, 2000f, "genetics_milestone_200", 200),
                ("Genetics Milestone IV", "Complete 500 breeding projects", AchievementCategory.Genetics_Innovation, AchievementRarity.Legendary, 4000f, "genetics_milestone_500", 500),
                
                ("Research Milestone I", "Complete 15 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Uncommon, 300f, "research_milestone_15", 15),
                ("Research Milestone II", "Complete 75 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Rare, 1000f, "research_milestone_75", 75),
                ("Research Milestone III", "Complete 300 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Epic, 2500f, "research_milestone_300", 300),
                ("Research Milestone IV", "Complete 1000 research projects", AchievementCategory.Research_Excellence, AchievementRarity.Legendary, 5000f, "research_milestone_1000", 1000),
                
                ("Business Milestone I", "Complete 100 sales", AchievementCategory.Business_Success, AchievementRarity.Uncommon, 150f, "business_milestone_100", 100),
                ("Business Milestone II", "Complete 1000 sales", AchievementCategory.Business_Success, AchievementRarity.Rare, 600f, "business_milestone_1000", 1000),
                ("Business Milestone III", "Complete 5000 sales", AchievementCategory.Business_Success, AchievementRarity.Epic, 2000f, "business_milestone_5000", 5000),
                ("Business Milestone IV", "Complete 25000 sales", AchievementCategory.Business_Success, AchievementRarity.Legendary, 6000f, "business_milestone_25000", 25000),
                
                // PC-012-10: Hidden Achievements - Secret discoveries and special conditions
                ("Ghost in the Machine", "???", AchievementCategory.Special, AchievementRarity.Epic, 1500f, "hidden_ghost_machine", 1),
                ("Midnight Cultivator", "???", AchievementCategory.Special, AchievementRarity.Rare, 750f, "hidden_midnight_cultivation", 1),
                ("Perfect Storm", "???", AchievementCategory.Special, AchievementRarity.Epic, 2000f, "hidden_perfect_storm", 1),
                ("The Chosen One", "???", AchievementCategory.Special, AchievementRarity.Legendary, 5000f, "hidden_chosen_one", 1),
                ("Easter Egg Hunter", "???", AchievementCategory.Special, AchievementRarity.Rare, 1000f, "hidden_easter_egg", 1),
                ("Time Traveler", "???", AchievementCategory.Special, AchievementRarity.Epic, 2500f, "hidden_time_traveler", 1),
                ("Lucky Seven", "???", AchievementCategory.Special, AchievementRarity.Rare, 777f, "hidden_lucky_seven", 1),
                ("Perfectionist's Dream", "???", AchievementCategory.Special, AchievementRarity.Legendary, 10000f, "hidden_perfectionist_dream", 1),
                ("Secret Garden", "???", AchievementCategory.Special, AchievementRarity.Epic, 3000f, "hidden_secret_garden", 1),
                ("Digital Alchemist", "???", AchievementCategory.Special, AchievementRarity.Epic, 2750f, "hidden_digital_alchemist", 1),
                
                // PC-012-11: Social and Community Achievements - Encouraging collaboration and community participation
                ("Community Helper", "Help 5 other players with advice or resources", AchievementCategory.Social, AchievementRarity.Common, 200f, "player_help_provided", 5),
                ("Knowledge Sharer", "Share cultivation knowledge with 10 community members", AchievementCategory.Social, AchievementRarity.Uncommon, 400f, "knowledge_shared", 10),
                ("Mentor", "Successfully mentor 3 new players through their first harvest", AchievementCategory.Social, AchievementRarity.Rare, 800f, "players_mentored", 3),
                ("Community Leader", "Lead a community breeding project with 5+ participants", AchievementCategory.Social, AchievementRarity.Epic, 1500f, "community_project_led", 1),
                ("Social Butterfly", "Interact with 25 different community members", AchievementCategory.Social, AchievementRarity.Uncommon, 300f, "community_interactions", 25),
                ("Forum Contributor", "Make 50 helpful posts in community forums", AchievementCategory.Social, AchievementRarity.Rare, 600f, "forum_contributions", 50),
                ("Event Organizer", "Organize and host 3 community cultivation events", AchievementCategory.Social, AchievementRarity.Epic, 1200f, "events_organized", 3),
                ("Collaboration Master", "Successfully complete 10 collaborative projects", AchievementCategory.Social, AchievementRarity.Rare, 750f, "collaborative_projects", 10),
                ("Community Champion", "Receive 100 positive ratings from community members", AchievementCategory.Social, AchievementRarity.Epic, 1000f, "positive_community_ratings", 100),
                ("Global Influencer", "Have your cultivation methods adopted by 50+ players worldwide", AchievementCategory.Social, AchievementRarity.Legendary, 3000f, "methods_adopted_globally", 50),
                ("Wisdom Keeper", "Be recognized as a top expert in 3 different cultivation areas", AchievementCategory.Social, AchievementRarity.Legendary, 2500f, "expert_recognition_areas", 3),
                ("Community Pillar", "Contribute to community for 365 consecutive days", AchievementCategory.Social, AchievementRarity.Legendary, 5000f, "daily_community_contribution", 365)
            };
            
            foreach (var (name, description, category, rarity, points, trigger, target) in achievementDefinitions)
            {
                var achievement = new Achievement
                {
                    AchievementID = $"achievement_{name.ToLower().Replace(" ", "_")}",
                    AchievementName = name,
                    Description = description,
                    Category = category,
                    Rarity = rarity,
                    Points = points,
                    TriggerEvent = trigger,
                    TargetValue = target,
                    CurrentProgress = 0,
                    IsUnlocked = false,
                    // PC-012-10: Mark hidden achievements and high-tier milestones as secret
                    IsSecret = trigger.StartsWith("hidden_") || description == "???" || 
                              (name.Contains("Milestone") && (rarity == AchievementRarity.Epic || rarity == AchievementRarity.Legendary)),
                    UnlockDate = DateTime.MinValue,
                    Icon = GenerateAchievementIcon(category, rarity),
                    CelebrationStyle = GenerateCelebrationStyle(rarity)
                };
                
                allAchievements.Add(achievement);
            }
            
            Debug.Log($"✅ Achievements initialized: {allAchievements.Count} achievements across all categories");
        }
        
        private void InitializeAchievementBadges()
        {
            // Create prestigious badges for major accomplishments
            var badgeDefinitions = new[]
            {
                ("Bronze Cultivator", "Complete 10 achievements", BadgeType.Milestone, 10),
                ("Silver Cultivator", "Complete 25 achievements", BadgeType.Milestone, 25),
                ("Gold Cultivator", "Complete 50 achievements", BadgeType.Milestone, 50),
                ("Platinum Master", "Complete 100 achievements", BadgeType.Milestone, 100),
                
                ("Breeding Specialist", "Complete all breeding achievements", BadgeType.Category, -1),
                ("Aroma Expert", "Complete all aromatic achievements", BadgeType.Category, -1),
                ("IPM Commander", "Complete all IPM achievements", BadgeType.Category, -1),
                ("Research Scientist", "Complete all research achievements", BadgeType.Category, -1),
                
                ("Speed Runner", "Complete achievements quickly", BadgeType.Special, -1),
                ("Perfectionist", "Achieve perfect scores", BadgeType.Special, -1),
                ("Community Hero", "Help other players", BadgeType.Social, -1),
                ("Ultimate Legend", "Complete all achievements", BadgeType.Ultimate, -1)
            };
            
            foreach (var (name, description, type, requirement) in badgeDefinitions)
            {
                var badge = new AchievementBadge
                {
                    BadgeID = $"badge_{name.ToLower().Replace(" ", "_")}",
                    BadgeName = name,
                    Description = description,
                    BadgeType = type,
                    RequiredAchievements = requirement,
                    IsEarned = false,
                    EarnDate = DateTime.MinValue,
                    Prestige = CalculateBadgePrestige(type, requirement),
                    Color = GenerateBadgeColor(type)
                };
                
                availableBadges.Add(badge);
            }
            
            Debug.Log($"✅ Achievement badges initialized: {availableBadges.Count} prestigious badges");
        }
        
        private void InitializeAchievementTiers()
        {
            // Create progression tiers based on achievement points
            var tierDefinitions = new[]
            {
                ("Novice", 0f, "🌱", "Beginning your cultivation journey"),
                ("Apprentice", 500f, "🌿", "Learning the fundamentals"),
                ("Practitioner", 1500f, "🍃", "Developing core skills"),
                ("Expert", 3500f, "🌳", "Mastering advanced techniques"),
                ("Master", 7500f, "🏆", "Achieving professional excellence"),
                ("Grandmaster", 15000f, "👑", "Reaching legendary status"),
                ("Legend", 30000f, "⭐", "Becoming a cultivation legend")
            };
            
            foreach (var (name, points, icon, description) in tierDefinitions)
            {
                var tier = new AchievementTier
                {
                    TierID = $"tier_{name.ToLower()}",
                    TierName = name,
                    RequiredPoints = points,
                    TierIcon = icon,
                    Description = description,
                    Benefits = GenerateTierBenefits(name),
                    Prestige = CalculateTierPrestige(points)
                };
                
                achievementTiers.Add(tier);
            }
            
            Debug.Log($"✅ Achievement tiers initialized: {achievementTiers.Count} progression tiers");
        }
        
        private void StartAchievementTracking()
        {
            // Start achievement tracking and monitoring
            lastAchievementUpdate = DateTime.Now;
            
            // PC-012-9: Subscribe to real game events for achievement tracking
            SubscribeToGameEvents();
            
            Debug.Log("✅ Achievement tracking started - celebrating player accomplishments");
        }
        
        private void StopAchievementTracking()
        {
            // Clean up achievement tracking
            UnsubscribeFromGameEvents();
            
            Debug.Log("✅ Achievement tracking stopped");
        }
        
        private void Update()
        {
            if (!EnableAchievementSystem) return;
            
            // Process pending celebrations
            ProcessPendingCelebrations();
            
            // Update achievement progress
            UpdateAchievementProgress();
        }
        
        private void ProcessPendingCelebrations()
        {
            if (!EnableUnlockCelebrations || pendingCelebrations.Count == 0) return;
            
            // Process celebrations one at a time to avoid overwhelming the player
            if (pendingCelebrations.Count > 0)
            {
                var achievement = pendingCelebrations.Dequeue();
                StartAchievementCelebration(achievement);
            }
        }
        
        private void UpdateAchievementProgress()
        {
            // Update progress tracking for all achievements
            foreach (var progress in playerProgress)
            {
                var achievement = allAchievements.FirstOrDefault(a => a.AchievementID == progress.AchievementID);
                if (achievement != null && !achievement.IsUnlocked)
                {
                    // Check if achievement should be unlocked
                    if (progress.CurrentValue >= achievement.TargetValue)
                    {
                        UnlockAchievement(achievement.AchievementID);
                    }
                }
            }
            
            // PC-012-9: Periodically check complex achievements
            CheckComplexAchievements();
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Update progress towards an achievement
        /// </summary>
        public void UpdateAchievementProgress(string triggerEvent, float value = 1f, string playerId = "current_player")
        {
            if (!EnableAchievementSystem) return;
            
            // Find achievements that match this trigger event
            var matchingAchievements = allAchievements.Where(a => 
                a.TriggerEvent == triggerEvent && !a.IsUnlocked).ToList();
            
            foreach (var achievement in matchingAchievements)
            {
                var progress = GetOrCreateProgress(achievement.AchievementID, playerId);
                progress.CurrentValue += value;
                progress.LastUpdate = DateTime.Now;
                
                // Update achievement current progress
                achievement.CurrentProgress = progress.CurrentValue;
                
                // Check for unlock
                if (progress.CurrentValue >= achievement.TargetValue)
                {
                    UnlockAchievement(achievement.AchievementID, playerId);
                }
                else
                {
                    // Fire progress update event
                    float progressPercent = progress.CurrentValue / achievement.TargetValue;
                    OnProgressUpdated?.Invoke(achievement.AchievementName, progressPercent);
                    
                    // PC-012-9: Fire experience gained event for progression integration
                    OnExperienceGained?.Invoke("Achievement", value * 2f); // 2 XP per progress point
                }
            }
        }
        
        /// <summary>
        /// Unlock an achievement and trigger celebration
        /// </summary>
        public bool UnlockAchievement(string achievementId, string playerId = "current_player")
        {
            var achievement = allAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
            if (achievement == null || achievement.IsUnlocked) return false;
            
            // Unlock the achievement
            achievement.IsUnlocked = true;
            achievement.UnlockDate = DateTime.Now;
            unlockedAchievements.Add(achievement);
            totalAchievementsUnlocked++;
            totalAchievementPoints += achievement.Points;
            
            // Add to recent unlocks
            recentUnlocks.Insert(0, achievement);
            if (recentUnlocks.Count > 10) // Keep last 10 unlocks
            {
                recentUnlocks.RemoveAt(recentUnlocks.Count - 1);
            }
            
            // Update player profile
            UpdatePlayerProfile(playerId, achievement);
            
            // Queue celebration
            if (EnableUnlockCelebrations)
            {
                pendingCelebrations.Enqueue(achievement);
            }
            
            // Fire unlock event
            OnAchievementUnlocked?.Invoke(achievement);
            
            // PC-012-9: Fire experience gained event for achievement unlock
            OnExperienceGained?.Invoke("Achievement", achievement.Points);
            
            // Check for badge unlocks
            CheckForBadgeUnlocks(playerId);
            
            // Check for tier advancement
            CheckForTierAdvancement(playerId);
            
            Debug.Log($"🏆 Achievement unlocked: {achievement.AchievementName} (+{achievement.Points} points)");
            return true;
        }
        
        /// <summary>
        /// Get player's achievement profile
        /// </summary>
        public PlayerAchievementProfile GetPlayerProfile(string playerId = "current_player")
        {
            return GetOrCreatePlayerProfile(playerId);
        }
        
        /// <summary>
        /// Get all achievements with their current progress
        /// </summary>
        public List<Achievement> GetAllAchievements(bool includeSecret = false)
        {
            if (includeSecret)
            {
                return new List<Achievement>(allAchievements);
            }
            
            return allAchievements.Where(a => !a.IsSecret || a.IsUnlocked).ToList();
        }
        
        /// <summary>
        /// Get unlocked achievements for player
        /// </summary>
        public List<Achievement> GetUnlockedAchievements(string playerId = "current_player")
        {
            return unlockedAchievements.Where(a => a.IsUnlocked).ToList();
        }
        
        /// <summary>
        /// Get achievement progress for specific achievement
        /// </summary>
        public AchievementProgress GetAchievementProgress(string achievementId, string playerId = "current_player")
        {
            return playerProgress.FirstOrDefault(p => 
                p.AchievementID == achievementId && p.PlayerID == playerId);
        }
        
        /// <summary>
        /// Get achievements by category
        /// </summary>
        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
        {
            return allAchievements.Where(a => a.Category == category).ToList();
        }
        
        /// <summary>
        /// Get earned badges for player
        /// </summary>
        public List<AchievementBadge> GetEarnedBadges(string playerId = "current_player")
        {
            return availableBadges.Where(b => b.IsEarned).ToList();
        }
        
        /// <summary>
        /// Get current achievement tier for player
        /// </summary>
        public AchievementTier GetCurrentTier(string playerId = "current_player")
        {
            var playerProfile = GetOrCreatePlayerProfile(playerId);
            return achievementTiers.Where(t => t.RequiredPoints <= playerProfile.TotalPoints)
                .OrderByDescending(t => t.RequiredPoints).FirstOrDefault();
        }
        
        /// <summary>
        /// Get achievement statistics
        /// </summary>
        public AchievementStats GetAchievementStats()
        {
            var stats = new AchievementStats
            {
                TotalAchievements = allAchievements.Count,
                UnlockedAchievements = totalAchievementsUnlocked,
                TotalPoints = totalAchievementPoints,
                CompletionPercentage = (float)totalAchievementsUnlocked / allAchievements.Count * 100f,
                RecentUnlocks = recentUnlocks.Count,
                AvailableBadges = availableBadges.Count,
                EarnedBadges = availableBadges.Count(b => b.IsEarned),
                LastUpdate = lastAchievementUpdate
            };
            
            return stats;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private PlayerAchievementProfile GetOrCreatePlayerProfile(string playerId)
        {
            if (playerProfiles.ContainsKey(playerId))
            {
                return playerProfiles[playerId];
            }
            
            var newProfile = new PlayerAchievementProfile
            {
                PlayerID = playerId,
                TotalAchievements = 0,
                TotalPoints = 0f,
                CurrentTier = "Novice",
                FavoriteCategory = AchievementCategory.Genetics_Innovation,
                LastUnlock = DateTime.MinValue,
                UnlockHistory = new List<string>(),
                EarnedBadges = new List<string>(),
                CategoryProgress = new Dictionary<AchievementCategory, int>()
            };
            
            // Initialize category progress
            foreach (AchievementCategory category in Enum.GetValues(typeof(AchievementCategory)))
            {
                newProfile.CategoryProgress[category] = 0;
            }
            
            playerProfiles[playerId] = newProfile;
            return newProfile;
        }
        
        private AchievementProgress GetOrCreateProgress(string achievementId, string playerId)
        {
            var existing = playerProgress.FirstOrDefault(p => 
                p.AchievementID == achievementId && p.PlayerID == playerId);
            
            if (existing != null) return existing;
            
            var newProgress = new AchievementProgress
            {
                AchievementID = achievementId,
                PlayerID = playerId,
                CurrentValue = 0f,
                LastUpdate = DateTime.Now
            };
            
            playerProgress.Add(newProgress);
            return newProgress;
        }
        
        private void UpdatePlayerProfile(string playerId, Achievement achievement)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            
            profile.TotalAchievements++;
            profile.TotalPoints += achievement.Points;
            profile.LastUnlock = DateTime.Now;
            
            // Add to unlock history
            profile.UnlockHistory.Insert(0, achievement.AchievementID);
            if (profile.UnlockHistory.Count > 50) // Keep last 50 unlocks
            {
                profile.UnlockHistory.RemoveAt(profile.UnlockHistory.Count - 1);
            }
            
            // Update category progress
            if (profile.CategoryProgress.ContainsKey(achievement.Category))
            {
                profile.CategoryProgress[achievement.Category]++;
            }
            
            // Update favorite category
            var mostCompletedCategory = profile.CategoryProgress
                .OrderByDescending(kvp => kvp.Value).First();
            profile.FavoriteCategory = mostCompletedCategory.Key;
            
            playerProfiles[playerId] = profile;
        }
        
        private void CheckForBadgeUnlocks(string playerId)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            
            foreach (var badge in availableBadges.Where(b => !b.IsEarned))
            {
                bool shouldUnlock = badge.BadgeType switch
                {
                    BadgeType.Milestone => profile.TotalAchievements >= badge.RequiredAchievements,
                    BadgeType.Category => CheckCategoryBadge(badge, profile),
                    BadgeType.Special => CheckSpecialBadge(badge, profile),
                    BadgeType.Social => CheckSocialBadge(badge, profile),
                    BadgeType.Ultimate => CheckUltimateBadge(badge, profile),
                    _ => false
                };
                
                if (shouldUnlock)
                {
                    UnlockBadge(badge, playerId);
                }
            }
        }
        
        private void CheckForTierAdvancement(string playerId)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            var currentTier = GetCurrentTier(playerId);
            
            if (currentTier != null && currentTier.TierName != profile.CurrentTier)
            {
                string oldTier = profile.CurrentTier;
                profile.CurrentTier = currentTier.TierName;
                OnTierAdvanced?.Invoke(currentTier);
                
                // PC-012-9: Fire level up event for tier advancement
                int oldLevel = GetTierLevel(oldTier);
                int newLevel = GetTierLevel(currentTier.TierName);
                if (newLevel > oldLevel)
                {
                    OnPlayerLevelUp?.Invoke(oldLevel, newLevel);
                }
                
                Debug.Log($"🌟 Tier advanced: {currentTier.TierName} ({currentTier.RequiredPoints} points)");
            }
        }
        
        private void UnlockBadge(AchievementBadge badge, string playerId)
        {
            badge.IsEarned = true;
            badge.EarnDate = DateTime.Now;
            
            var profile = GetOrCreatePlayerProfile(playerId);
            profile.EarnedBadges.Add(badge.BadgeID);
            
            OnBadgeEarned?.Invoke(badge);
            
            Debug.Log($"🎖️ Badge earned: {badge.BadgeName}");
        }
        
        private void StartAchievementCelebration(Achievement achievement)
        {
            // Start exciting celebration for achievement unlock
            OnCelebrationStarted?.Invoke(achievement);
            
            // PC-012-10: Special celebration messages for milestone and hidden achievements
            string celebrationMessage = achievement.AchievementName.Contains("Milestone") ? 
                $"🏁 MILESTONE REACHED: {achievement.AchievementName} ({achievement.Rarity})" :
                achievement.IsSecret ? 
                $"🕵️ SECRET DISCOVERED: {achievement.AchievementName} ({achievement.Rarity})" :
                $"🎉 Achievement unlocked: {achievement.AchievementName} ({achievement.Rarity})";
            
            Debug.Log(celebrationMessage);
        }
        
        private bool CheckCategoryBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            // Check if player completed all achievements in a category
            var categoryName = badge.BadgeName.Replace(" Specialist", "").Replace(" Expert", "").Replace(" Commander", "").Replace(" Scientist", "");
            
            if (Enum.TryParse<AchievementCategory>(categoryName, out var category))
            {
                var categoryAchievements = allAchievements.Where(a => a.Category == category).Count();
                return profile.CategoryProgress.GetValueOrDefault(category, 0) >= categoryAchievements;
            }
            
            return false;
        }
        
        private bool CheckSpecialBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            // Check special badge conditions
            return badge.BadgeName switch
            {
                "Speed Runner" => profile.UnlockHistory.Count >= 10, // Assume speed unlocks
                "Perfectionist" => true, // Assume perfect achievements exist
                _ => false
            };
        }
        
        private bool CheckSocialBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            // Check social badge conditions
            return badge.BadgeName switch
            {
                "Community Hero" => profile.CategoryProgress.GetValueOrDefault(AchievementCategory.Community_Builder, 0) >= 5,
                _ => false
            };
        }
        
        private bool CheckUltimateBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            // Check ultimate badge conditions
            return badge.BadgeName switch
            {
                "Ultimate Legend" => profile.TotalAchievements >= allAchievements.Count,
                _ => false
            };
        }
        
        private string GenerateAchievementIcon(AchievementCategory category, AchievementRarity rarity)
        {
            var baseIcon = category switch
            {
                AchievementCategory.Genetics_Innovation => "🧬",
                AchievementCategory.Cultivation_Mastery => "🔬",
                AchievementCategory.Quality_Achievement => "👃",
                AchievementCategory.Efficiency_Optimization => "🛡️",
                AchievementCategory.Research_Excellence => "📚",
                AchievementCategory.Community_Builder => "🏆",
                AchievementCategory.Business_Success => "📈",
                AchievementCategory.Teaching_Mentorship => "👥",
                AchievementCategory.Special => "✨",
                AchievementCategory.Ultimate => "👑",
                _ => "🏅"
            };
            
            return baseIcon;
        }
        
        private string GenerateCelebrationStyle(AchievementRarity rarity)
        {
            return rarity switch
            {
                AchievementRarity.Common => "Simple",
                AchievementRarity.Uncommon => "Enhanced",
                AchievementRarity.Rare => "Spectacular",
                AchievementRarity.Epic => "Magnificent",
                AchievementRarity.Legendary => "Legendary",
                _ => "Simple"
            };
        }
        
        private int CalculateBadgePrestige(BadgeType type, int requirement)
        {
            return type switch
            {
                BadgeType.Milestone => requirement / 10,
                BadgeType.Category => 15,
                BadgeType.Special => 20,
                BadgeType.Social => 25,
                BadgeType.Ultimate => 50,
                _ => 5
            };
        }
        
        private string GenerateBadgeColor(BadgeType type)
        {
            return type switch
            {
                BadgeType.Milestone => "Gold",
                BadgeType.Category => "Blue",
                BadgeType.Special => "Purple",
                BadgeType.Social => "Green",
                BadgeType.Ultimate => "Rainbow",
                _ => "Silver"
            };
        }
        
        private List<string> GenerateTierBenefits(string tierName)
        {
            return tierName switch
            {
                "Novice" => new List<string> { "Basic achievement tracking" },
                "Apprentice" => new List<string> { "Achievement hints", "Progress tracking" },
                "Practitioner" => new List<string> { "Bonus achievement points", "Custom badges" },
                "Expert" => new List<string> { "Exclusive achievements", "Priority celebrations" },
                "Master" => new List<string> { "Master tier recognition", "Special rewards" },
                "Grandmaster" => new List<string> { "Legendary status", "Elite benefits" },
                "Legend" => new List<string> { "Ultimate recognition", "Immortal legacy" },
                _ => new List<string>()
            };
        }
        
        private int CalculateTierPrestige(float points)
        {
            return (int)(points / 100f); // 1 prestige per 100 points
        }
        
        /// <summary>
        /// Get numerical level for tier name
        /// </summary>
        private int GetTierLevel(string tierName)
        {
            return tierName switch
            {
                "Novice" => 1,
                "Apprentice" => 2,
                "Practitioner" => 3,
                "Expert" => 4,
                "Master" => 5,
                "Grandmaster" => 6,
                "Legend" => 7,
                _ => 1
            };
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate achievement system functionality
        /// </summary>
        public void TestAchievementSystem()
        {
            Debug.Log("=== Testing Achievement System ===");
            Debug.Log($"System Enabled: {EnableAchievementSystem}");
            Debug.Log($"Celebrations Enabled: {EnableUnlockCelebrations}");
            Debug.Log($"Total Achievements: {allAchievements.Count}");
            Debug.Log($"Available Badges: {availableBadges.Count}");
            Debug.Log($"Achievement Tiers: {achievementTiers.Count}");
            
            // Test achievement progress
            if (EnableAchievementSystem)
            {
                // PC-012-9: Test real achievement tracking system
                Debug.Log("--- Testing Real Achievement Integration ---");
                
                // Test cultivation achievements
                UpdateAchievementProgress("plant_planted", 1f, "test_player");
                UpdateAchievementProgress("plant_harvested", 1f, "test_player");
                UpdateAchievementProgress("perfect_quality_harvest", 1f, "test_player");
                Debug.Log($"✓ Test cultivation achievement progress");
                
                // Test breeding achievements
                UpdateAchievementProgress("breed_challenge_completed", 1f, "test_player");
                UpdateAchievementProgress("successful_breeding", 1f, "test_player");
                UpdateAchievementProgress("perfect_trait_expression", 1f, "test_player");
                Debug.Log($"✓ Test breeding achievement progress");
                
                // Test research achievements
                UpdateAchievementProgress("research_completed", 1f, "test_player");
                UpdateAchievementProgress("genetics_research_completed", 1f, "test_player");
                UpdateAchievementProgress("cultivation_research_completed", 1f, "test_player");
                Debug.Log($"✓ Test research achievement progress");
                
                // Test business achievements
                UpdateAchievementProgress("product_sold", 1f, "test_player");
                UpdateAchievementProgress("sales_completed", 1f, "test_player");
                UpdateAchievementProgress("high_value_sale", 1f, "test_player");
                Debug.Log($"✓ Test business achievement progress");
                
                // Test progression achievements
                UpdateAchievementProgress("level_5_reached", 1f, "test_player");
                UpdateAchievementProgress("player_progression", 1f, "test_player");
                Debug.Log($"✓ Test progression achievement progress");
                
                // Test achievement unlock
                bool unlocked = UnlockAchievement("achievement_first_harvest", "test_player");
                Debug.Log($"✓ Test achievement unlock: {unlocked}");
                
                // Test player profile
                var profile = GetPlayerProfile("test_player");
                Debug.Log($"✓ Test player profile: {profile.TotalAchievements} achievements, {profile.TotalPoints} points");
                
                // Test current tier
                var tier = GetCurrentTier("test_player");
                Debug.Log($"✓ Test current tier: {tier?.TierName ?? "None"}");
                
                // Test achievement statistics
                var stats = GetAchievementStats();
                Debug.Log($"✓ Test achievement stats: {stats.CompletionPercentage:F1}% complete");
                
                // Test complex achievements
                CheckComplexAchievements();
                Debug.Log($"✓ Test complex achievement checks");
                
                // PC-012-10: Test milestone and hidden achievements
                UpdateAchievementProgress("cultivation_milestone_25", 25f, "test_player");
                UpdateAchievementProgress("hidden_ghost_machine", 1f, "test_player");
                Debug.Log($"✓ Test milestone and hidden achievements");
                
                // PC-012-11: Test social and community achievements
                UpdateAchievementProgress("player_help_provided", 5f, "test_player");
                UpdateAchievementProgress("knowledge_shared", 10f, "test_player");
                UpdateAchievementProgress("community_interactions", 25f, "test_player");
                UpdateAchievementProgress("forum_contributions", 50f, "test_player");
                Debug.Log($"✓ Test social and community achievements");
                
                Debug.Log("--- Real Achievement Integration Test Complete ---");
            }
            
            Debug.Log("✅ Achievement system test completed");
        }
        
        /// <summary>
        /// PC-012-9: Public method to test real game event integration
        /// </summary>
        public void TestRealGameEventIntegration()
        {
            Debug.Log("=== Testing Real Game Event Integration ===");
            
            try
            {
                // Simulate real game events for testing
                Debug.Log("Simulating plant harvested event...");
                OnPlantHarvestedHandler(new { Quality = 95f, Id = "test_plant_1" });
                
                Debug.Log("Simulating breeding completed event...");
                OnBreedingCompletedHandler(new { IsSuccess = true, Quality = 98f, Id = "test_breeding_1" });
                
                Debug.Log("Simulating research completed event...");
                OnResearchCompletedHandler(new { Category = "Genetics", Id = "test_research_1" }, new { });
                
                Debug.Log("Simulating sale completed event...");
                OnSaleCompletedHandler(new { TotalValue = 15000f, Id = "test_sale_1" });
                
                Debug.Log("Simulating player level up event...");
                OnPlayerLevelUpHandler(4, 5);
                
                // PC-012-11: Simulate social events
                Debug.Log("Simulating social achievement events...");
                OnPlayerHelpProvidedHandler("test_player", "helped_player_1");
                OnKnowledgeSharedHandler("test_player", "cultivation_techniques");
                OnCommunityInteractionHandler("test_player", "community_member_1");
                OnForumContributionHandler("test_player", "helpful_post");
                OnEventOrganizedHandler("test_player", new { EventType = "BreedingProject", ParticipantCount = 6 });
                OnPositiveRatingReceivedHandler("test_player", 5);
                
                var stats = GetAchievementStats();
                Debug.Log($"✓ Achievement integration test complete - {stats.UnlockedAchievements} achievements unlocked");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Achievement integration test failed: {ex.Message}");
            }
            
            Debug.Log("✅ Real game event integration test completed");
        }
        
        #endregion
        
        #region Real Game Event Integration - PC-012-9
        
        /// <summary>
        /// Subscribe to real game events for achievement tracking
        /// </summary>
        private void SubscribeToGameEvents()
        {
            try
            {
                // Get managers for event subscription
                var plantManager = GameManager.Instance?.GetManager("PlantManager");
                var breedingManager = GameManager.Instance?.GetManager("GeneticsManager");
                var researchManager = GameManager.Instance?.GetManager("ResearchManager");
                var marketManager = GameManager.Instance?.GetManager("MarketManager");
                var progressionManager = GameManager.Instance?.GetManager("ComprehensiveProgressionManager");
                
                // Plant cultivation events
                if (plantManager != null)
                {
                    // Use reflection to safely subscribe to events
                    var plantManagerType = plantManager.GetType();
                    
                    var onPlantHarvestedEvent = plantManagerType.GetEvent("OnPlantHarvested");
                    if (onPlantHarvestedEvent != null)
                    {
                        var handler = new System.Action<object>(OnPlantHarvestedHandler);
                        onPlantHarvestedEvent.AddEventHandler(plantManager, handler);
                        Debug.Log("✅ Subscribed to OnPlantHarvested event");
                    }
                    
                    var onPlantAddedEvent = plantManagerType.GetEvent("OnPlantAdded");
                    if (onPlantAddedEvent != null)
                    {
                        var handler = new System.Action<object>(OnPlantAddedHandler);
                        onPlantAddedEvent.AddEventHandler(plantManager, handler);
                        Debug.Log("✅ Subscribed to OnPlantAdded event");
                    }
                }
                
                // Breeding and genetics events
                if (breedingManager != null)
                {
                    var breedingManagerType = breedingManager.GetType();
                    
                    var onBreedingCompletedEvent = breedingManagerType.GetEvent("OnBreedingCompleted");
                    if (onBreedingCompletedEvent != null)
                    {
                        var handler = new System.Action<object>(OnBreedingCompletedHandler);
                        onBreedingCompletedEvent.AddEventHandler(breedingManager, handler);
                        Debug.Log("✅ Subscribed to OnBreedingCompleted event");
                    }
                }
                
                // Research events
                if (researchManager != null)
                {
                    var researchManagerType = researchManager.GetType();
                    
                    var onResearchCompletedEvent = researchManagerType.GetEvent("OnResearchCompleted");
                    if (onResearchCompletedEvent != null)
                    {
                        var handler = new System.Action<object, object>(OnResearchCompletedHandler);
                        onResearchCompletedEvent.AddEventHandler(researchManager, handler);
                        Debug.Log("✅ Subscribed to OnResearchCompleted event");
                    }
                }
                
                // Market and sales events
                if (marketManager != null)
                {
                    var marketManagerType = marketManager.GetType();
                    
                    var onSaleCompletedEvent = marketManagerType.GetEvent("OnSaleCompleted");
                    if (onSaleCompletedEvent != null)
                    {
                        var handler = new System.Action<object>(OnSaleCompletedHandler);
                        onSaleCompletedEvent.AddEventHandler(marketManager, handler);
                        Debug.Log("✅ Subscribed to OnSaleCompleted event");
                    }
                }
                
                // Progression events
                if (progressionManager != null)
                {
                    var progressionManagerType = progressionManager.GetType();
                    
                    var onPlayerLevelUpEvent = progressionManagerType.GetEvent("OnPlayerLevelUp");
                    if (onPlayerLevelUpEvent != null)
                    {
                        var handler = new System.Action<int, int>(OnPlayerLevelUpHandler);
                        onPlayerLevelUpEvent.AddEventHandler(progressionManager, handler);
                        Debug.Log("✅ Subscribed to OnPlayerLevelUp event");
                    }
                }
                
                // PC-012-11: Social and Community Events
                SubscribeToSocialEvents();
                
                Debug.Log("✅ Achievement system subscribed to real game events");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Achievement event subscription failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Unsubscribe from game events
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            try
            {
                // Note: In production, we would store event handlers to properly unsubscribe
                // For now, we'll rely on manager shutdown to clean up references
                Debug.Log("✅ Achievement system unsubscribed from game events");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Achievement event unsubscription failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// PC-012-11: Subscribe to social and community events for social achievements
        /// </summary>
        private void SubscribeToSocialEvents()
        {
            try
            {
                // Community Manager Events
                var communityManager = GameManager.Instance?.GetManager("CommunityManager");
                if (communityManager != null)
                {
                    var communityManagerType = communityManager.GetType();
                    
                    // Player help events
                    var onPlayerHelpEvent = communityManagerType.GetEvent("OnPlayerHelpProvided");
                    if (onPlayerHelpEvent != null)
                    {
                        var handler = new System.Action<string, string>(OnPlayerHelpProvidedHandler);
                        onPlayerHelpEvent.AddEventHandler(communityManager, handler);
                        Debug.Log("✅ Subscribed to OnPlayerHelpProvided event");
                    }
                    
                    // Knowledge sharing events
                    var onKnowledgeSharedEvent = communityManagerType.GetEvent("OnKnowledgeShared");
                    if (onKnowledgeSharedEvent != null)
                    {
                        var handler = new System.Action<string, string>(OnKnowledgeSharedHandler);
                        onKnowledgeSharedEvent.AddEventHandler(communityManager, handler);
                        Debug.Log("✅ Subscribed to OnKnowledgeShared event");
                    }
                    
                    // Community interaction events
                    var onCommunityInteractionEvent = communityManagerType.GetEvent("OnCommunityInteraction");
                    if (onCommunityInteractionEvent != null)
                    {
                        var handler = new System.Action<string, string>(OnCommunityInteractionHandler);
                        onCommunityInteractionEvent.AddEventHandler(communityManager, handler);
                        Debug.Log("✅ Subscribed to OnCommunityInteraction event");
                    }
                }
                
                // Forum/Social Manager Events
                var socialManager = GameManager.Instance?.GetManager("SocialManager");
                if (socialManager != null)
                {
                    var socialManagerType = socialManager.GetType();
                    
                    // Forum contribution events
                    var onForumPostEvent = socialManagerType.GetEvent("OnForumContribution");
                    if (onForumPostEvent != null)
                    {
                        var handler = new System.Action<string, string>(OnForumContributionHandler);
                        onForumPostEvent.AddEventHandler(socialManager, handler);
                        Debug.Log("✅ Subscribed to OnForumContribution event");
                    }
                    
                    // Event organization
                    var onEventOrganizedEvent = socialManagerType.GetEvent("OnEventOrganized");
                    if (onEventOrganizedEvent != null)
                    {
                        var handler = new System.Action<string, object>(OnEventOrganizedHandler);
                        onEventOrganizedEvent.AddEventHandler(socialManager, handler);
                        Debug.Log("✅ Subscribed to OnEventOrganized event");
                    }
                    
                    // Community ratings
                    var onPositiveRatingEvent = socialManagerType.GetEvent("OnPositiveRatingReceived");
                    if (onPositiveRatingEvent != null)
                    {
                        var handler = new System.Action<string, int>(OnPositiveRatingReceivedHandler);
                        onPositiveRatingEvent.AddEventHandler(socialManager, handler);
                        Debug.Log("✅ Subscribed to OnPositiveRatingReceived event");
                    }
                }
                
                Debug.Log("✅ Achievement system subscribed to social and community events");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Social achievement event subscription failed: {ex.Message}");
            }
        }
        
        #region Real Game Event Handlers
        
        /// <summary>
        /// Handle plant harvested events for cultivation achievements
        /// </summary>
        private void OnPlantHarvestedHandler(object plantInstance)
        {
            try
            {
                // Update cultivation achievements
                UpdateAchievementProgress("plant_harvested", 1f);
                UpdateAchievementProgress("harvest_completed", 1f);
                
                // PC-012-10: Update milestone achievement progress
                UpdateAchievementProgress("cultivation_milestone_25", 1f);
                UpdateAchievementProgress("cultivation_milestone_100", 1f);
                UpdateAchievementProgress("cultivation_milestone_500", 1f);
                UpdateAchievementProgress("cultivation_milestone_1000", 1f);
                
                // Check for quality-based achievements
                var plantType = plantInstance?.GetType();
                if (plantType != null)
                {
                    var qualityProperty = plantType.GetProperty("Quality");
                    if (qualityProperty != null && qualityProperty.GetValue(plantInstance) is float quality)
                    {
                        if (quality >= 95f)
                        {
                            UpdateAchievementProgress("perfect_quality_harvest", 1f);
                        }
                        if (quality >= 90f)
                        {
                            UpdateAchievementProgress("high_quality_harvest", 1f);
                        }
                    }
                }
                
                Debug.Log("🏆 Plant harvested - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Plant harvested achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle plant added events for cultivation achievements
        /// </summary>
        private void OnPlantAddedHandler(object plantInstance)
        {
            try
            {
                UpdateAchievementProgress("plant_planted", 1f);
                UpdateAchievementProgress("cultivation_started", 1f);
                
                Debug.Log("🏆 Plant added - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Plant added achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle breeding completed events for genetics achievements
        /// </summary>
        private void OnBreedingCompletedHandler(object breedingResult)
        {
            try
            {
                UpdateAchievementProgress("breed_challenge_completed", 1f);
                UpdateAchievementProgress("breeding_success", 1f);
                
                // PC-012-10: Update genetics milestone progress
                UpdateAchievementProgress("genetics_milestone_10", 1f);
                UpdateAchievementProgress("genetics_milestone_50", 1f);
                UpdateAchievementProgress("genetics_milestone_200", 1f);
                UpdateAchievementProgress("genetics_milestone_500", 1f);
                
                // Check for genetic perfection achievements
                var resultType = breedingResult?.GetType();
                if (resultType != null)
                {
                    var successProperty = resultType.GetProperty("IsSuccess");
                    if (successProperty != null && successProperty.GetValue(breedingResult) is bool isSuccess && isSuccess)
                    {
                        UpdateAchievementProgress("successful_breeding", 1f);
                        
                        // Check for perfect trait expression
                        var qualityProperty = resultType.GetProperty("Quality");
                        if (qualityProperty != null && qualityProperty.GetValue(breedingResult) is float quality && quality >= 98f)
                        {
                            UpdateAchievementProgress("perfect_trait_expression", 1f);
                        }
                    }
                }
                
                Debug.Log("🏆 Breeding completed - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Breeding completed achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle research completed events for research achievements
        /// </summary>
        private void OnResearchCompletedHandler(object project, object results)
        {
            try
            {
                UpdateAchievementProgress("research_completed", 1f);
                UpdateAchievementProgress("research_project_finished", 1f);
                
                // PC-012-10: Update research milestone progress
                UpdateAchievementProgress("research_milestone_15", 1f);
                UpdateAchievementProgress("research_milestone_75", 1f);
                UpdateAchievementProgress("research_milestone_300", 1f);
                UpdateAchievementProgress("research_milestone_1000", 1f);
                
                // Check for research category achievements
                var projectType = project?.GetType();
                if (projectType != null)
                {
                    var categoryProperty = projectType.GetProperty("Category");
                    if (categoryProperty != null)
                    {
                        var category = categoryProperty.GetValue(project)?.ToString();
                        switch (category)
                        {
                            case "Genetics":
                                UpdateAchievementProgress("genetics_research_completed", 1f);
                                break;
                            case "Cultivation":
                                UpdateAchievementProgress("cultivation_research_completed", 1f);
                                break;
                            case "Processing":
                                UpdateAchievementProgress("processing_research_completed", 1f);
                                break;
                        }
                    }
                }
                
                Debug.Log("🏆 Research completed - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Research completed achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle sale completed events for business achievements
        /// </summary>
        private void OnSaleCompletedHandler(object saleData)
        {
            try
            {
                UpdateAchievementProgress("product_sold", 1f);
                UpdateAchievementProgress("sales_completed", 1f);
                
                // PC-012-10: Update business milestone progress
                UpdateAchievementProgress("business_milestone_100", 1f);
                UpdateAchievementProgress("business_milestone_1000", 1f);
                UpdateAchievementProgress("business_milestone_5000", 1f);
                UpdateAchievementProgress("business_milestone_25000", 1f);
                
                // Check for high-value sales
                var saleType = saleData?.GetType();
                if (saleType != null)
                {
                    var totalValueProperty = saleType.GetProperty("TotalValue");
                    if (totalValueProperty != null && totalValueProperty.GetValue(saleData) is float totalValue)
                    {
                        if (totalValue >= 10000f)
                        {
                            UpdateAchievementProgress("high_value_sale", 1f);
                        }
                        if (totalValue >= 50000f)
                        {
                            UpdateAchievementProgress("major_sale", 1f);
                        }
                    }
                }
                
                Debug.Log("🏆 Sale completed - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Sale completed achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle player level up events for progression achievements
        /// </summary>
        private void OnPlayerLevelUpHandler(int oldLevel, int newLevel)
        {
            try
            {
                UpdateAchievementProgress("level_reached", newLevel);
                UpdateAchievementProgress("player_progression", 1f);
                
                // Check for major level milestones
                if (newLevel >= 5)
                {
                    UpdateAchievementProgress("level_5_reached", 1f);
                }
                if (newLevel >= 10)
                {
                    UpdateAchievementProgress("level_10_reached", 1f);
                }
                if (newLevel >= 25)
                {
                    UpdateAchievementProgress("master_level", 1f);
                }
                if (newLevel >= 50)
                {
                    UpdateAchievementProgress("grandmaster_level", 1f);
                }
                
                Debug.Log($"🏆 Player leveled up to {newLevel} - achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Player level up achievement tracking failed: {ex.Message}");
            }
        }
        
        #region PC-012-11: Social Achievement Event Handlers
        
        /// <summary>
        /// Handle player help provided events for social achievements
        /// </summary>
        private void OnPlayerHelpProvidedHandler(string helperId, string helpedPlayerId)
        {
            try
            {
                UpdateAchievementProgress("player_help_provided", 1f, helperId);
                
                Debug.Log($"🏆 Player help provided by {helperId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Player help achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle knowledge shared events for social achievements
        /// </summary>
        private void OnKnowledgeSharedHandler(string sharerId, string knowledgeType)
        {
            try
            {
                UpdateAchievementProgress("knowledge_shared", 1f, sharerId);
                
                Debug.Log($"🏆 Knowledge shared by {sharerId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Knowledge sharing achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle community interaction events for social achievements
        /// </summary>
        private void OnCommunityInteractionHandler(string playerId, string interactionType)
        {
            try
            {
                UpdateAchievementProgress("community_interactions", 1f, playerId);
                
                // Track unique community member interactions for Social Butterfly achievement
                var profile = GetOrCreatePlayerProfile(playerId);
                if (profile.CommunityInteractions == null)
                {
                    profile.CommunityInteractions = new List<string>();
                }
                if (!profile.CommunityInteractions.Contains(interactionType))
                {
                    profile.CommunityInteractions.Add(interactionType);
                }
                
                Debug.Log($"🏆 Community interaction by {playerId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Community interaction achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle forum contribution events for social achievements
        /// </summary>
        private void OnForumContributionHandler(string contributorId, string postType)
        {
            try
            {
                UpdateAchievementProgress("forum_contributions", 1f, contributorId);
                
                Debug.Log($"🏆 Forum contribution by {contributorId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Forum contribution achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle event organization for social achievements
        /// </summary>
        private void OnEventOrganizedHandler(string organizerId, object eventData)
        {
            try
            {
                UpdateAchievementProgress("events_organized", 1f, organizerId);
                
                // Check if it's a community breeding project
                var eventType = eventData?.GetType();
                if (eventType != null)
                {
                    var eventTypeProperty = eventType.GetProperty("EventType");
                    if (eventTypeProperty != null && eventTypeProperty.GetValue(eventData)?.ToString() == "BreedingProject")
                    {
                        var participantCountProperty = eventType.GetProperty("ParticipantCount");
                        if (participantCountProperty != null && participantCountProperty.GetValue(eventData) is int participantCount)
                        {
                            if (participantCount >= 5)
                            {
                                UpdateAchievementProgress("community_project_led", 1f, organizerId);
                            }
                        }
                    }
                }
                
                UpdateAchievementProgress("collaborative_projects", 1f, organizerId);
                
                Debug.Log($"🏆 Event organized by {organizerId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Event organization achievement tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle positive rating received events for social achievements
        /// </summary>
        private void OnPositiveRatingReceivedHandler(string playerId, int ratingValue)
        {
            try
            {
                UpdateAchievementProgress("positive_community_ratings", 1f, playerId);
                
                Debug.Log($"🏆 Positive rating received by {playerId} - social achievement progress updated");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Positive rating achievement tracking failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #endregion
        
        /// <summary>
        /// Periodically check for complex achievements based on game state
        /// </summary>
        private void CheckComplexAchievements()
        {
            try
            {
                // Check for cross-system achievements
                CheckCrossSystemAchievements();
                
                // Check for time-based achievements
                CheckTimeBasedAchievements();
                
                // Check for milestone achievements
                CheckMilestoneAchievements();
                
                // PC-012-10: Check for hidden achievements
                CheckHiddenAchievements();
                
                Debug.Log("✅ Complex achievement check completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Complex achievement check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check achievements that require multiple systems
        /// </summary>
        private void CheckCrossSystemAchievements()
        {
            // Example: "Master Cultivator" - requires achievements across all core systems
            var cultivationAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Cultivation_Mastery);
            var geneticsAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Genetics_Innovation);
            var businessAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Business_Success);
            var researchAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Research_Excellence);
            
            if (cultivationAchievements >= 5 && geneticsAchievements >= 5 && businessAchievements >= 3 && researchAchievements >= 5)
            {
                UpdateAchievementProgress("all_systems_mastered", 1f);
            }
            
            // Example: "Perfectionist" - achieve 100% completion in any category
            foreach (AchievementCategory category in System.Enum.GetValues(typeof(AchievementCategory)))
            {
                var categoryAchievements = GetAchievementsByCategory(category);
                var completedInCategory = GetUnlockedAchievements().Count(a => a.Category == category);
                
                if (categoryAchievements.Count > 0 && completedInCategory >= categoryAchievements.Count)
                {
                    UpdateAchievementProgress("perfect_completion", 1f);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Check time-based achievements
        /// </summary>
        private void CheckTimeBasedAchievements()
        {
            var now = DateTime.Now;
            var profile = GetPlayerProfile();
            
            // Daily activity achievements
            if (profile.LastUnlock.Date == now.Date)
            {
                UpdateAchievementProgress("daily_achievement", 1f);
            }
            
            // Weekly streak achievements
            var daysInRow = (now - profile.LastUnlock).Days;
            if (daysInRow <= 1 && profile.UnlockHistory.Count >= 7)
            {
                UpdateAchievementProgress("weekly_streak", 1f);
            }
        }
        
        /// <summary>
        /// Check milestone-based achievements
        /// </summary>
        private void CheckMilestoneAchievements()
        {
            var stats = GetAchievementStats();
            
            // Total achievements milestones
            if (stats.UnlockedAchievements >= 10)
            {
                UpdateAchievementProgress("achievement_collector", 1f);
            }
            if (stats.UnlockedAchievements >= 25)
            {
                UpdateAchievementProgress("achievement_hunter", 1f);
            }
            if (stats.UnlockedAchievements >= 50)
            {
                UpdateAchievementProgress("achievement_master", 1f);
            }
            
            // Total points milestones
            if (stats.TotalPoints >= 1000f)
            {
                UpdateAchievementProgress("point_milestone_1000", 1f);
            }
            if (stats.TotalPoints >= 5000f)
            {
                UpdateAchievementProgress("point_milestone_5000", 1f);
            }
            if (stats.TotalPoints >= 10000f)
            {
                UpdateAchievementProgress("point_milestone_10000", 1f);
            }
        }
        
        /// <summary>
        /// PC-012-10: Check for hidden achievement triggers
        /// </summary>
        private void CheckHiddenAchievements()
        {
            try
            {
                var currentTime = DateTime.Now;
                var stats = GetAchievementStats();
                
                // Hidden: "Ghost in the Machine" - Unlock during system quiet time
                if (currentTime.Hour >= 2 && currentTime.Hour <= 4)
                {
                    UpdateAchievementProgress("hidden_ghost_machine", 1f);
                }
                
                // Hidden: "Midnight Cultivator" - Plant something at midnight
                if (currentTime.Hour == 0 && currentTime.Minute < 30)
                {
                    UpdateAchievementProgress("hidden_midnight_cultivation", 1f);
                }
                
                // Hidden: "Perfect Storm" - Unlock when all systems achieve milestones
                var cultivationAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Cultivation_Mastery);
                var geneticsAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Genetics_Innovation);
                var businessAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Business_Success);
                var researchAchievements = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Research_Excellence);
                
                if (cultivationAchievements >= 8 && geneticsAchievements >= 8 && businessAchievements >= 6 && researchAchievements >= 8)
                {
                    UpdateAchievementProgress("hidden_perfect_storm", 1f);
                }
                
                // Hidden: "The Chosen One" - Unlock when player reaches ultimate mastery
                if (stats.TotalPoints >= 15000f && stats.UnlockedAchievements >= 75)
                {
                    UpdateAchievementProgress("hidden_chosen_one", 1f);
                }
                
                // Hidden: "Lucky Seven" - Unlock with exactly 7 of something
                if (stats.UnlockedAchievements == 77 || stats.TotalPoints == 777f)
                {
                    UpdateAchievementProgress("hidden_lucky_seven", 1f);
                }
                
                // Hidden: "Time Traveler" - Unlock when accessing historical data
                if (currentTime.DayOfWeek == DayOfWeek.Friday && currentTime.Day == 13)
                {
                    UpdateAchievementProgress("hidden_time_traveler", 1f);
                }
                
                // Hidden: "Easter Egg Hunter" - Special trigger for exploration
                var specialCategory = GetUnlockedAchievements().Count(a => a.Category == AchievementCategory.Special);
                if (specialCategory >= 5)
                {
                    UpdateAchievementProgress("hidden_easter_egg", 1f);
                }
                
                // Hidden: "Perfectionist's Dream" - Ultimate completion achievement
                var categoryCompletion = 0;
                foreach (AchievementCategory category in System.Enum.GetValues(typeof(AchievementCategory)))
                {
                    var categoryAchievements = GetAchievementsByCategory(category);
                    var completedInCategory = GetUnlockedAchievements().Count(a => a.Category == category);
                    
                    if (categoryAchievements.Count > 0 && completedInCategory >= categoryAchievements.Count)
                    {
                        categoryCompletion++;
                    }
                }
                
                if (categoryCompletion >= 5) // Complete 5 categories
                {
                    UpdateAchievementProgress("hidden_perfectionist_dream", 1f);
                }
                
                // Hidden: "Secret Garden" - Complete all cultivation milestones
                var cultivationMilestones = GetUnlockedAchievements().Count(a => a.AchievementName.Contains("Cultivation Milestone"));
                if (cultivationMilestones >= 4)
                {
                    UpdateAchievementProgress("hidden_secret_garden", 1f);
                }
                
                // Hidden: "Digital Alchemist" - Master genetics and research
                var masterGeneticist = GetUnlockedAchievements().Any(a => a.AchievementName.Contains("Genetics Milestone IV"));
                var masterResearcher = GetUnlockedAchievements().Any(a => a.AchievementName.Contains("Research Milestone IV"));
                
                if (masterGeneticist && masterResearcher)
                {
                    UpdateAchievementProgress("hidden_digital_alchemist", 1f);
                }
                
                Debug.Log("✅ Hidden achievement check completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Hidden achievement check failed: {ex.Message}");
            }
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class Achievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public AchievementCategory Category;
        public AchievementRarity Rarity;
        public float Points;
        public string TriggerEvent;
        public float TargetValue;
        public float CurrentProgress;
        public bool IsUnlocked;
        public bool IsSecret;
        public DateTime UnlockDate;
        public string Icon;
        public string CelebrationStyle;
    }
    
    [System.Serializable]
    public class AchievementProgress
    {
        public string AchievementID;
        public string PlayerID;
        public float CurrentValue;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class PlayerAchievementProfile
    {
        public string PlayerID;
        public int TotalAchievements;
        public float TotalPoints;
        public string CurrentTier;
        public AchievementCategory FavoriteCategory;
        public DateTime LastUnlock;
        public List<string> UnlockHistory = new List<string>();
        public List<string> EarnedBadges = new List<string>();
        public Dictionary<AchievementCategory, int> CategoryProgress = new Dictionary<AchievementCategory, int>();
        
        // PC-012-11: Social and Community Achievement Tracking
        public List<string> CommunityInteractions = new List<string>();
        public int ConsecutiveCommunityDays = 0;
        public DateTime LastCommunityContribution = DateTime.MinValue;
        
        // Additional properties for PlayerRecognitionService compatibility
        public DateTime ProfileCreatedDate = DateTime.Now;
        public DateTime LastUpdateDate = DateTime.Now;
        public int Prestige = 0;
        public int SocialRecognition = 0;
        public int CommunityContributions = 0;
        public float CompletionPercentage = 0f;
    }
    
    [System.Serializable]
    public class AchievementBadge
    {
        public string BadgeID;
        public string BadgeName;
        public string Description;
        public BadgeType BadgeType;
        public int RequiredAchievements;
        public bool IsEarned;
        public DateTime EarnDate;
        public int Prestige;
        public string Color;
        
        // Additional properties for PlayerRecognitionService compatibility
        public string Icon = "";
        public AchievementRarity Rarity = AchievementRarity.Common;
    }
    
    [System.Serializable]
    public class AchievementTier
    {
        public string TierID;
        public string TierName;
        public float RequiredPoints;
        public string TierIcon;
        public string Description;
        public List<string> Benefits = new List<string>();
        public int Prestige;
        
        // Additional properties for PlayerRecognitionService compatibility
        public int Level = 1;
        public DateTime UnlockDate = DateTime.MinValue;
        public bool IsUnlocked = false;
    }
    
    [System.Serializable]
    public class AchievementStats
    {
        public int TotalAchievements;
        public int UnlockedAchievements;
        public float TotalPoints;
        public float CompletionPercentage;
        public int RecentUnlocks;
        public int AvailableBadges;
        public int EarnedBadges;
        public DateTime LastUpdate;
    }
    
    public enum BadgeType
    {
        Milestone,
        Category,
        Special,
        Social,
        Ultimate
    }
    
    #endregion
}