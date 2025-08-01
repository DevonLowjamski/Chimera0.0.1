using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using AchievementCategory = ProjectChimera.Data.Achievements.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Achievements.AchievementRarity;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// PC-012-2a: Achievement Tracking Service - Specialized progress tracking and validation
    /// Handles achievement progress monitoring, condition validation, and gaming event integration
    /// Part of the decomposed AchievementSystemManager (450/1903 lines)
    /// Updated with gaming events integration for Module 1 compatibility
    /// Uses ProjectChimera.Data.Progression.AchievementProgress for all progress tracking
    /// Fixed CS0426 errors by ensuring all Achievement references use AchievementSystemManager.Achievement
    /// </summary>
    public class AchievementTrackingService : MonoBehaviour, IAchievementTrackingService
    {
        [Header("Tracking Configuration")]
        [SerializeField] private bool _enableTracking = true;
        [SerializeField] private bool _enableProgressLogging = false;
        [SerializeField] private float _progressUpdateInterval = 0.1f;
        [SerializeField] private bool _validateCompletions = true;

        [Header("Achievement Storage")]
        [SerializeField] private List<Achievement> _allAchievements = new List<Achievement>();
        [SerializeField] private List<Achievement> _unlockedAchievements = new List<Achievement>();
        [SerializeField] private List<ProjectChimera.Data.Achievements.AchievementProgress> _playerProgress = new List<ProjectChimera.Data.Achievements.AchievementProgress>();

        // Service state
        private bool _isInitialized = false;
        private Dictionary<string, Achievement> _achievementLookup = new Dictionary<string, Achievement>();
        private Dictionary<string, List<ProjectChimera.Data.Achievements.AchievementProgress>> _playerProgressLookup = new Dictionary<string, List<ProjectChimera.Data.Achievements.AchievementProgress>>();
        private HashSet<string> _completedAchievements = new HashSet<string>();

        // Gaming events integration (PC-012-2a enhancement)
        private Dictionary<string, float> _eventCounters = new Dictionary<string, float>();
        private Dictionary<string, DateTime> _lastEventTimestamps = new Dictionary<string, DateTime>();
        
        // Dependencies for gaming events (to be implemented when Module 1 events are available)
        // private IGameEventBus _eventBus;

        // Events for achievement completion (Interface compliance)
        public event Action<string, float> OnProgressUpdated;
        public event Action<string, ProjectChimera.Data.Achievements.AchievementProgress> OnAchievementUnlocked;
        public event Action<string, string> OnProgressMilestone;
        public event Action<ProgressValidationResult> OnValidationCompleted;
        
        // Legacy events (maintain backward compatibility)
        public static event Action<Achievement> OnAchievementCompleted;
        public static event Action<string, Achievement, float> OnProgressUpdatedLegacy;
        public static event Action<string, float> OnAchievementProgressChanged;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Tracking Service";
        public IReadOnlyList<Achievement> AllAchievements => _allAchievements;
        public IReadOnlyList<Achievement> UnlockedAchievements => _unlockedAchievements;
        public IReadOnlyList<ProjectChimera.Data.Achievements.AchievementProgress> PlayerProgress => _playerProgress;
        
        // Interface properties
        public int ActiveAchievementCount => _allAchievements.Count(a => !a.IsUnlocked);
        public int CompletedAchievementCount => _unlockedAchievements.Count;
        public float TotalProgressPercentage => _allAchievements.Count > 0 ? 
            (float)_unlockedAchievements.Count / _allAchievements.Count : 0f;

        public void Initialize()
        {
            InitializeService();
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDataStructures()
        {
            _allAchievements = new List<Achievement>();
            _unlockedAchievements = new List<Achievement>();
            _playerProgress = new List<ProjectChimera.Data.Achievements.AchievementProgress>();
            _achievementLookup = new Dictionary<string, Achievement>();
            _playerProgressLookup = new Dictionary<string, List<ProjectChimera.Data.Achievements.AchievementProgress>>();
            _completedAchievements = new HashSet<string>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementTrackingService already initialized", this);
                return;
            }

            try
            {
                InitializeAchievements();
                BuildLookupTables();
                SubscribeToGamingEvents();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementTrackingService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementTrackingService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            UnsubscribeFromGamingEvents();
            _allAchievements.Clear();
            _unlockedAchievements.Clear();
            _playerProgress.Clear();
            _achievementLookup.Clear();
            _playerProgressLookup.Clear();
            _completedAchievements.Clear();
            _eventCounters.Clear();
            _lastEventTimestamps.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("AchievementTrackingService shutdown completed", this);
        }

        #endregion

        #region Gaming Events Integration
        
        private void SubscribeToGamingEvents()
        {
            // Gaming events integration will be implemented when Module 1 gaming events are available
            // For now, achievements can be triggered via the UpdateProgress() method
            ChimeraLogger.Log("AchievementTrackingService gaming events integration ready", this);
        }
        
        private void UnsubscribeFromGamingEvents()
        {
            // Gaming events unsubscription will be implemented when Module 1 gaming events are available
            ChimeraLogger.Log("AchievementTrackingService gaming events unsubscribed", this);
        }
        
        // Gaming event handlers - to be implemented when Module 1 events are available
        // For now, achievements can be triggered manually via UpdateProgress() calls
        
        /// <summary>
        /// Sets the game event bus for achievement tracking integration (to be implemented when Module 1 events are available)
        /// </summary>
        public void SetEventBus(object eventBus)
        {
            // _eventBus = eventBus;
            // if (_eventBus != null && _isInitialized)
            // {
            //     SubscribeToGamingEvents();
            // }
            ChimeraLogger.Log("SetEventBus called - ready for Module 1 integration", this);
        }
        
        private void UpdateEventCounter(string eventName, float value)
        {
            if (!_eventCounters.ContainsKey(eventName))
            {
                _eventCounters[eventName] = 0f;
            }
            
            _eventCounters[eventName] += value;
            _lastEventTimestamps[eventName] = DateTime.Now;
        }
        
        #endregion

        #region Achievement Initialization

        private void InitializeAchievements()
        {
            // Create comprehensive achievement system covering all game aspects
            var achievementDefinitions = new (string, string, ProjectChimera.Data.Progression.AchievementCategory, ProjectChimera.Data.Progression.AchievementRarity, float, string, float)[]
            {
                // Cultivation & Plant Management Achievements
                ("First Harvest", "Harvest your first plant", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Common, 50f, "plant_harvested", 1),
                ("Green Thumb", "Plant 10 seeds successfully", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Common, 75f, "plant_planted", 10),
                ("Master Gardener", "Harvest 100 plants", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "plant_harvested", 100),
                ("Quality Master", "Achieve 10 perfect quality harvests", ProjectChimera.Data.Progression.AchievementCategory.Quality_Achievement, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1000f, "perfect_quality_harvest", 10),
                ("Cultivation Expert", "Maintain 50 high quality harvests", ProjectChimera.Data.Progression.AchievementCategory.Quality_Achievement, ProjectChimera.Data.Progression.AchievementRarity.Rare, 750f, "high_quality_harvest", 50),
                
                // Breeding & Genetics Achievements
                ("First Breed", "Complete your first breeding challenge", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Common, 50f, "breed_challenge_completed", 1),
                ("Master Breeder", "Complete 25 breeding challenges", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "breed_challenge_completed", 25),
                ("Perfect Genetics", "Create a plant with perfect trait expression", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1000f, "perfect_trait_expression", 1),
                ("Genetic Pioneer", "Complete 50 successful breeding projects", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 2000f, "successful_breeding", 50),
                ("Breeding Virtuoso", "Complete 10 breeding projects with exceptional results", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1500f, "breeding_success", 10),
                
                // Research & Development Achievements
                ("Research Initiate", "Complete your first research project", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Common, 100f, "research_completed", 1),
                ("Knowledge Seeker", "Complete 10 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 300f, "research_completed", 10),
                ("Research Master", "Complete 50 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Rare, 1000f, "research_completed", 50),
                ("Genetics Researcher", "Complete 10 genetics research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 400f, "genetics_research_completed", 10),
                ("Cultivation Scientist", "Complete 10 cultivation research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 400f, "cultivation_research_completed", 10),
                ("Processing Expert", "Complete 10 processing research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 400f, "processing_research_completed", 10),
                ("Scholar", "Complete research in all categories", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "research_category_unlocked", 3),
                
                // Business & Sales Achievements
                ("First Sale", "Complete your first product sale", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Common, 50f, "product_sold", 1),
                ("Entrepreneur", "Complete 50 successful sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 250f, "sales_completed", 50),
                ("Big Deal", "Complete a high-value sale worth $10,000+", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "high_value_sale", 1),
                ("Tycoon", "Complete a major sale worth $50,000+", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1500f, "major_sale", 1),
                ("Sales Master", "Complete 500 total sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "sales_completed", 500),
                
                // Progression & Learning Achievements
                ("Getting Started", "Reach level 5", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Common, 100f, "level_5_reached", 1),
                ("Rising Star", "Reach level 10", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 200f, "level_10_reached", 1),
                ("Expert Level", "Reach level 25", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Rare, 1000f, "master_level", 1),
                ("Grandmaster", "Reach level 50", ProjectChimera.Data.Progression.AchievementCategory.Innovation_Pioneer, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2500f, "grandmaster_level", 1),
                ("Leveling Legend", "Gain experience through progression", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Common, 50f, "player_progression", 10),
                
                // Achievement System Meta Achievements
                ("Achievement Hunter", "Unlock 10 achievements", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 300f, "achievement_collector", 1),
                ("Achievement Master", "Unlock 25 achievements", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 750f, "achievement_hunter", 1),
                ("Achievement Legend", "Unlock 50 achievements", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "achievement_master", 1),
                ("Point Collector", "Earn 1,000 achievement points", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 200f, "point_milestone_1000", 1),
                ("Point Master", "Earn 5,000 achievement points", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "point_milestone_5000", 1),
                ("Point Legend", "Earn 10,000 achievement points", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1000f, "point_milestone_10000", 1),
                
                // Cross-System & Special Achievements
                ("System Master", "Master all core game systems", ProjectChimera.Data.Progression.AchievementCategory.Ultimate, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 5000f, "all_systems_mastered", 1),
                ("Perfectionist", "Achieve 100% completion in any category", ProjectChimera.Data.Progression.AchievementCategory.Quality_Achievement, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 3000f, "perfect_completion", 1),
                ("Daily Achiever", "Unlock an achievement every day", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "daily_achievement", 7),
                ("Weekly Streak", "Maintain a 7-day achievement streak", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1000f, "weekly_streak", 1),
                ("Ultimate Master", "Unlock all other achievements", ProjectChimera.Data.Progression.AchievementCategory.Ultimate, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 10000f, "all_achievements", 1),
                
                // Milestone Achievements - Major progression markers
                ("Cultivation Milestone I", "Harvest 25 plants", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 200f, "cultivation_milestone_25", 25),
                ("Cultivation Milestone II", "Harvest 100 plants", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Rare, 500f, "cultivation_milestone_100", 100),
                ("Cultivation Milestone III", "Harvest 500 plants", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1500f, "cultivation_milestone_500", 500),
                ("Cultivation Milestone IV", "Harvest 1000 plants", ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 3000f, "cultivation_milestone_1000", 1000),
                
                ("Genetics Milestone I", "Complete 10 breeding projects", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 250f, "genetics_milestone_10", 10),
                ("Genetics Milestone II", "Complete 50 breeding projects", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Rare, 750f, "genetics_milestone_50", 50),
                ("Genetics Milestone III", "Complete 200 breeding projects", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "genetics_milestone_200", 200),
                ("Genetics Milestone IV", "Complete 500 breeding projects", ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 4000f, "genetics_milestone_500", 500),
                
                ("Research Milestone I", "Complete 15 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 300f, "research_milestone_15", 15),
                ("Research Milestone II", "Complete 75 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Rare, 1000f, "research_milestone_75", 75),
                ("Research Milestone III", "Complete 300 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2500f, "research_milestone_300", 300),
                ("Research Milestone IV", "Complete 1000 research projects", ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 5000f, "research_milestone_1000", 1000),
                
                ("Business Milestone I", "Complete 100 sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 150f, "business_milestone_100", 100),
                ("Business Milestone II", "Complete 1000 sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Rare, 600f, "business_milestone_1000", 1000),
                ("Business Milestone III", "Complete 5000 sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "business_milestone_5000", 5000),
                ("Business Milestone IV", "Complete 25000 sales", ProjectChimera.Data.Progression.AchievementCategory.Business_Success, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 6000f, "business_milestone_25000", 25000),
                
                // Hidden Achievements - Secret discoveries and special conditions
                ("Ghost in the Machine", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1500f, "hidden_ghost_machine", 1),
                ("Midnight Cultivator", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 750f, "hidden_midnight_cultivation", 1),
                ("Perfect Storm", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2000f, "hidden_perfect_storm", 1),
                ("The Chosen One", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 5000f, "hidden_chosen_one", 1),
                ("Easter Egg Hunter", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 1000f, "hidden_easter_egg", 1),
                ("Time Traveler", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2500f, "hidden_time_traveler", 1),
                ("Lucky Seven", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Rare, 777f, "hidden_lucky_seven", 1),
                ("Perfectionist's Dream", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 10000f, "hidden_perfectionist_dream", 1),
                ("Secret Garden", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 3000f, "hidden_secret_garden", 1),
                ("Digital Alchemist", "???", ProjectChimera.Data.Progression.AchievementCategory.Special, ProjectChimera.Data.Progression.AchievementRarity.Epic, 2750f, "hidden_digital_alchemist", 1),
                
                // Social and Community Achievements - Encouraging collaboration and community participation
                ("Community Helper", "Help 5 other players with advice or resources", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Common, 200f, "player_help_provided", 5),
                ("Knowledge Sharer", "Share cultivation knowledge with 10 community members", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 400f, "knowledge_shared", 10),
                ("Mentor", "Successfully mentor 3 new players through their first harvest", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Rare, 800f, "players_mentored", 3),
                ("Community Leader", "Lead a community breeding project with 5+ participants", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1500f, "community_project_led", 1),
                ("Social Butterfly", "Interact with 25 different community members", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Uncommon, 300f, "community_interactions", 25),
                ("Forum Contributor", "Make 50 helpful posts in community forums", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Rare, 600f, "forum_contributions", 50),
                ("Event Organizer", "Organize and host 3 community cultivation events", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1200f, "events_organized", 3),
                ("Collaboration Master", "Successfully complete 10 collaborative projects", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Rare, 750f, "collaborative_projects", 10),
                ("Community Champion", "Receive 100 positive ratings from community members", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Epic, 1000f, "positive_community_ratings", 100),
                ("Global Influencer", "Have your cultivation methods adopted by 50+ players worldwide", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 3000f, "methods_adopted_globally", 50),
                ("Wisdom Keeper", "Be recognized as a top expert in 3 different cultivation areas", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 2500f, "expert_recognition_areas", 3),
                ("Community Pillar", "Contribute to community for 365 consecutive days", ProjectChimera.Data.Progression.AchievementCategory.Social, ProjectChimera.Data.Progression.AchievementRarity.Legendary, 5000f, "daily_community_contribution", 365)
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
                    // Mark hidden achievements and high-tier milestones as secret
                    IsSecret = trigger.StartsWith("hidden_") || description == "???" || 
                              (name.Contains("Milestone") && (rarity == ProjectChimera.Data.Progression.AchievementRarity.Epic || rarity == ProjectChimera.Data.Progression.AchievementRarity.Legendary)),
                    UnlockDate = DateTime.MinValue,
                    Icon = GenerateAchievementIcon(category, rarity),
                    CelebrationStyle = GenerateCelebrationStyle(rarity)
                };
                
                _allAchievements.Add(achievement);
            }
            
            ChimeraLogger.Log($"Achievements initialized: {_allAchievements.Count} achievements across all categories", this);
        }

        private void BuildLookupTables()
        {
            _achievementLookup.Clear();
            _playerProgressLookup.Clear();
            
            foreach (var achievement in _allAchievements)
            {
                _achievementLookup[achievement.AchievementID] = achievement;
            }
            
            foreach (var progress in _playerProgress)
            {
                if (!_playerProgressLookup.ContainsKey(progress.PlayerId))
                {
                    _playerProgressLookup[progress.PlayerId] = new List<ProjectChimera.Data.Achievements.AchievementProgress>();
                }
                _playerProgressLookup[progress.PlayerId].Add(progress);
            }
        }

        private string GenerateAchievementIcon(ProjectChimera.Data.Progression.AchievementCategory category, ProjectChimera.Data.Progression.AchievementRarity rarity)
        {
            return $"achievement_icon_{category.ToString().ToLower()}_{rarity.ToString().ToLower()}";
        }

        private string GenerateCelebrationStyle(ProjectChimera.Data.Progression.AchievementRarity rarity)
        {
            return rarity switch
            {
                ProjectChimera.Data.Progression.AchievementRarity.Common => "celebration_basic",
                ProjectChimera.Data.Progression.AchievementRarity.Uncommon => "celebration_enhanced", 
                ProjectChimera.Data.Progression.AchievementRarity.Rare => "celebration_impressive",
                ProjectChimera.Data.Progression.AchievementRarity.Epic => "celebration_spectacular",
                ProjectChimera.Data.Progression.AchievementRarity.Legendary => "celebration_legendary",
                _ => "celebration_basic"
            };
        }

        #endregion

        #region Progress Tracking
        
        // Interface implementation
        public void UpdateProgress(string triggerEvent, float value = 1f, string playerId = "current_player")
        {
            UpdateAchievementProgress(triggerEvent, value, playerId);
        }
        
        
        public ProjectChimera.Data.Achievements.AchievementProgress GetProgress(string achievementId)
        {
            var achievement = GetAchievementById(achievementId);
            if (achievement == null) return null;
            
            return GetOrCreateProgress(achievementId, "current_player");
        }
        
        public List<ProjectChimera.Data.Achievements.AchievementProgress> GetAllProgress()
        {
            return new List<ProjectChimera.Data.Achievements.AchievementProgress>(_playerProgress);
        }
        
        public bool IsAchievementCompleted(string achievementId)
        {
            return IsAchievementUnlocked(achievementId, "current_player");
        }
        
        public float GetEventCounter(string eventName)
        {
            return _eventCounters.TryGetValue(eventName, out var count) ? count : 0f;
        }
        
        public ProgressValidationResult ValidateProgress(string achievementId, string playerId = "current_player")
        {
            var result = new ProgressValidationResult
            {
                AchievementId = achievementId,
                PlayerId = playerId,
                ValidationTime = DateTime.Now
            };
            
            var achievement = GetAchievementById(achievementId);
            if (achievement == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Achievement not found";
                result.ProgressPercentage = 0f;
                OnValidationCompleted?.Invoke(result);
                return result;
            }
            
            var progress = GetOrCreateProgress(achievementId, playerId);
            result.ProgressPercentage = achievement.TargetValue > 0 ? 
                Mathf.Clamp01(progress.CurrentValue / achievement.TargetValue) : 0f;
            
            result.IsValid = progress.CurrentValue >= 0 && progress.CurrentValue <= achievement.TargetValue;
            result.ErrorMessage = result.IsValid ? string.Empty : "Invalid progress value";
            
            OnValidationCompleted?.Invoke(result);
            return result;
        }

        public void UpdateAchievementProgress(string triggerEvent, float value = 1f, string playerId = "current_player")
        {
            if (!_isInitialized || !_enableTracking)
            {
                return;
            }

            var matchingAchievements = _allAchievements.Where(a => 
                a.TriggerEvent == triggerEvent && !a.IsUnlocked).ToList();

            foreach (var achievement in matchingAchievements)
            {
                var progress = GetOrCreateProgress(achievement.AchievementID, playerId);
                var previousProgress = progress.CurrentValue;
                
                progress.CurrentValue += value;
                progress.LastUpdateDate = DateTime.Now;
                
                if (_enableProgressLogging)
                {
                    ChimeraLogger.Log($"Progress update: {achievement.AchievementName} - {progress.CurrentValue}/{achievement.TargetValue}", this);
                }

                // Check for completion
                if (_validateCompletions && progress.CurrentValue >= achievement.TargetValue && !achievement.IsUnlocked)
                {
                    CompleteAchievement(achievement, playerId);
                }

                // Trigger interface events
                OnProgressUpdated?.Invoke(playerId, progress.CurrentValue);
                OnProgressMilestone?.Invoke(playerId, achievement.AchievementID);
                
                // Legacy events for backward compatibility
                OnProgressUpdatedLegacy?.Invoke(playerId, achievement, progress.CurrentValue);
                OnAchievementProgressChanged?.Invoke(achievement.AchievementID, progress.CurrentValue);
            }
        }

        private ProjectChimera.Data.Achievements.AchievementProgress GetOrCreateProgress(string achievementId, string playerId)
        {
            if (!_playerProgressLookup.ContainsKey(playerId))
            {
                _playerProgressLookup[playerId] = new List<ProjectChimera.Data.Achievements.AchievementProgress>();
            }

            var playerProgressList = _playerProgressLookup[playerId];
            var existingProgress = playerProgressList.FirstOrDefault(p => p.AchievementId == achievementId);

            if (existingProgress == null)
            {
                existingProgress = new ProjectChimera.Data.Achievements.AchievementProgress
                {
                    AchievementId = achievementId,
                    PlayerId = playerId,
                    CurrentValue = 0,
                    IsCompleted = false,
                    StartedDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now
                };
                
                playerProgressList.Add(existingProgress);
                _playerProgress.Add(existingProgress);
            }

            return existingProgress;
        }

        private void CompleteAchievement(Achievement achievement, string playerId)
        {
            if (_completedAchievements.Contains($"{playerId}_{achievement.AchievementID}"))
            {
                return; // Already completed
            }

            achievement.IsUnlocked = true;
            achievement.UnlockDate = DateTime.Now;
            achievement.CurrentProgress = achievement.TargetValue;
            
            if (!_unlockedAchievements.Contains(achievement))
            {
                _unlockedAchievements.Add(achievement);
            }

            var progress = GetOrCreateProgress(achievement.AchievementID, playerId);
            progress.IsCompleted = true;
            progress.CompletedDate = DateTime.Now;

            _completedAchievements.Add($"{playerId}_{achievement.AchievementID}");

            ChimeraLogger.Log($"Achievement completed: {achievement.AchievementName} by {playerId}", this);
            
            // Trigger interface events
            OnAchievementUnlocked?.Invoke(achievement.AchievementID, progress);
            
            // Legacy events for backward compatibility
            OnAchievementCompleted?.Invoke(achievement);
        }

        #endregion

        #region Public API

        public Achievement GetAchievementById(string achievementId)
        {
            return _achievementLookup.TryGetValue(achievementId, out var achievement) ? achievement : null;
        }

        public List<Achievement> GetAchievementsByCategory(ProjectChimera.Data.Progression.AchievementCategory category)
        {
            return _allAchievements.Where(a => a.Category == category).ToList();
        }

        public List<Achievement> GetAchievementsByCategory(ProjectChimera.Data.Achievements.AchievementCategory category)
        {
            return _allAchievements.Where(a => (int)a.Category == (int)category).ToList();
        }

        public List<Achievement> GetAchievementsByRarity(ProjectChimera.Data.Progression.AchievementRarity rarity)
        {
            return _allAchievements.Where(a => a.Rarity == rarity).ToList();
        }

        public List<Achievement> GetAchievementsByRarity(ProjectChimera.Data.Achievements.AchievementRarity rarity)
        {
            return _allAchievements.Where(a => (int)a.Rarity == (int)rarity).ToList();
        }

        public List<ProjectChimera.Data.Achievements.AchievementProgress> GetPlayerProgress(string playerId)
        {
            return _playerProgressLookup.TryGetValue(playerId, out var progressList) ? 
                   progressList : new List<ProjectChimera.Data.Achievements.AchievementProgress>();
        }

        public float GetPlayerProgressPercent(string playerId, string achievementId)
        {
            var achievement = GetAchievementById(achievementId);
            if (achievement == null) return 0f;

            var progress = GetPlayerProgress(playerId).FirstOrDefault(p => p.AchievementId == achievementId);
            if (progress == null) return 0f;

            return Mathf.Clamp01(progress.CurrentValue / achievement.TargetValue);
        }

        public int GetCompletedAchievementCount(string playerId)
        {
            return GetPlayerProgress(playerId).Count(p => p.IsCompleted);
        }

        public float GetTotalAchievementPoints(string playerId)
        {
            var completedIds = GetPlayerProgress(playerId).Where(p => p.IsCompleted).Select(p => p.AchievementId).ToHashSet();
            return _allAchievements.Where(a => completedIds.Contains(a.AchievementID)).Sum(a => a.Points);
        }

        public List<Achievement> GetSecretAchievements()
        {
            return _allAchievements.Where(a => a.IsSecret).ToList();
        }

        public bool IsAchievementUnlocked(string achievementId, string playerId)
        {
            return _completedAchievements.Contains($"{playerId}_{achievementId}");
        }


        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

}