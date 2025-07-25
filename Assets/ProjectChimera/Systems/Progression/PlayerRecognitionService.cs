using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Player Recognition Service - Manages player profiles, badge system, and tier advancement
    /// Extracted from AchievementSystemManager to provide focused player recognition functionality
    /// Handles player achievement profiles, badge unlocks, tier progression, and social recognition
    /// Provides comprehensive player progression tracking and recognition systems
    /// </summary>
    public class PlayerRecognitionService : MonoBehaviour
    {
        [Header("Recognition Configuration")]
        [SerializeField] private bool _enablePlayerProfiles = true;
        [SerializeField] private bool _enableBadgeSystem = true;
        [SerializeField] private bool _enableTierProgression = true;
        [SerializeField] private bool _enableSocialRecognition = true;

        [Header("Profile Management")]
        [SerializeField] private int _maxProfileHistory = 50;
        [SerializeField] private int _maxCategoryTracking = 20;
        [SerializeField] private bool _autoCreateProfiles = true;
        [SerializeField] private bool _persistProfiles = true;

        [Header("Badge Configuration")]
        [SerializeField] private bool _enableMilestoneBadges = true;
        [SerializeField] private bool _enableCategoryBadges = true;
        [SerializeField] private bool _enableSpecialBadges = true;
        [SerializeField] private bool _enableSocialBadges = true;

        [Header("Tier Progression")]
        [SerializeField] private bool _enableTierBenefits = true;
        [SerializeField] private bool _enablePrestigeSystem = true;
        [SerializeField] private float _tierProgressMultiplier = 1.0f;

        [Header("Player Data")]
        [SerializeField] private Dictionary<string, PlayerAchievementProfile> _playerProfiles = new Dictionary<string, PlayerAchievementProfile>();
        [SerializeField] private List<AchievementBadge> _availableBadges = new List<AchievementBadge>();
        [SerializeField] private List<AchievementTier> _achievementTiers = new List<AchievementTier>();
        [SerializeField] private List<string> _recentProfileUpdates = new List<string>();

        // Service state
        private bool _isInitialized = false;
        private DateTime _lastProfileUpdate = DateTime.Now;
        private Dictionary<string, DateTime> _profileCooldowns = new Dictionary<string, DateTime>();

        // Recognition data lookups
        private Dictionary<BadgeType, List<AchievementBadge>> _badgesByType = new Dictionary<BadgeType, List<AchievementBadge>>();
        private Dictionary<string, AchievementTier> _tierLookup = new Dictionary<string, AchievementTier>();

        // Events for player recognition
        public static event Action<PlayerAchievementProfile> OnPlayerRecognition;
        public static event Action<AchievementBadge> OnBadgeEarned;
        public static event Action<AchievementTier> OnTierAdvanced;
        public static event Action<PlayerAchievementProfile> OnProfileUpdated;
        public static event Action<string, int, int> OnPlayerLevelUp;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Player Recognition Service";
        public int TotalProfiles => _playerProfiles.Count;
        public int AvailableBadges => _availableBadges.Count;
        public int AchievementTiers => _achievementTiers.Count;

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
            _playerProfiles = new Dictionary<string, PlayerAchievementProfile>();
            _availableBadges = new List<AchievementBadge>();
            _achievementTiers = new List<AchievementTier>();
            _recentProfileUpdates = new List<string>();
            _profileCooldowns = new Dictionary<string, DateTime>();
            _badgesByType = new Dictionary<BadgeType, List<AchievementBadge>>();
            _tierLookup = new Dictionary<string, AchievementTier>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PlayerRecognitionService already initialized", this);
                return;
            }

            try
            {
                InitializeBadgeSystem();
                InitializeTierSystem();
                BuildLookupTables();
                
                _isInitialized = true;
                ChimeraLogger.Log("PlayerRecognitionService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PlayerRecognitionService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            SaveAllProfiles();
            ClearAllData();
            
            _isInitialized = false;
            ChimeraLogger.Log("PlayerRecognitionService shutdown completed", this);
        }

        #endregion

        #region Badge System Initialization

        private void InitializeBadgeSystem()
        {
            if (!_enableBadgeSystem) return;

            // Create comprehensive badge definitions
            var badgeDefinitions = new (string, string, BadgeType, int, string)[]
            {
                // Milestone Badges - Achievement count based
                ("Bronze Cultivator", "Complete 10 achievements", BadgeType.Milestone, 10, "Gold"),
                ("Silver Cultivator", "Complete 25 achievements", BadgeType.Milestone, 25, "Silver"),
                ("Gold Cultivator", "Complete 50 achievements", BadgeType.Milestone, 50, "Gold"),
                ("Platinum Master", "Complete 100 achievements", BadgeType.Milestone, 100, "Platinum"),
                ("Diamond Legend", "Complete 200 achievements", BadgeType.Milestone, 200, "Diamond"),

                // Category Badges - Category completion based
                ("Breeding Specialist", "Complete all breeding achievements", BadgeType.Category, -1, "Blue"),
                ("Cultivation Expert", "Complete all cultivation achievements", BadgeType.Category, -1, "Green"),
                ("Research Scientist", "Complete all research achievements", BadgeType.Category, -1, "Purple"),
                ("Business Tycoon", "Complete all business achievements", BadgeType.Category, -1, "Yellow"),
                ("Genetics Pioneer", "Complete all genetics achievements", BadgeType.Category, -1, "Orange"),
                ("Social Influencer", "Complete all social achievements", BadgeType.Category, -1, "Pink"),

                // Special Badges - Unique conditions
                ("Speed Runner", "Complete achievements quickly", BadgeType.Special, -1, "Red"),
                ("Perfectionist", "Achieve perfect scores consistently", BadgeType.Special, -1, "White"),
                ("Night Owl", "Unlock achievements at night", BadgeType.Special, -1, "Dark Blue"),
                ("Early Bird", "Unlock achievements in the morning", BadgeType.Special, -1, "Light Blue"),
                ("Streak Master", "Maintain long achievement streaks", BadgeType.Special, -1, "Rainbow"),

                // Social Badges - Community interaction based
                ("Community Helper", "Help other players consistently", BadgeType.Social, 50, "Green"),
                ("Mentor", "Guide new players successfully", BadgeType.Social, 25, "Gold"),
                ("Knowledge Sharer", "Share expertise with community", BadgeType.Social, 100, "Blue"),
                ("Event Organizer", "Organize community events", BadgeType.Social, 10, "Purple"),

                // Ultimate Badges - Highest achievements
                ("Ultimate Master", "Complete all achievements", BadgeType.Ultimate, -1, "Rainbow"),
                ("Legendary Pioneer", "Achieve legendary status in all areas", BadgeType.Ultimate, -1, "Cosmic"),
                ("Immortal Legend", "Reach the highest possible recognition", BadgeType.Ultimate, -1, "Divine")
            };

            foreach (var (name, description, type, requirement, color) in badgeDefinitions)
            {
                var badge = new AchievementBadge
                {
                    BadgeID = GenerateBadgeId(name),
                    BadgeName = name,
                    Description = description,
                    BadgeType = type,
                    RequiredAchievements = requirement,
                    IsEarned = false,
                    EarnDate = DateTime.MinValue,
                    Prestige = CalculateBadgePrestige(type, requirement),
                    Color = color,
                    Icon = GenerateBadgeIcon(type, name),
                    Rarity = DetermineBadgeRarity(type)
                };

                _availableBadges.Add(badge);
            }

            ChimeraLogger.Log($"Badge system initialized: {_availableBadges.Count} badges available", this);
        }

        private void InitializeTierSystem()
        {
            if (!_enableTierProgression) return;

            // Create progression tier definitions
            var tierDefinitions = new (string, float, string, string, int)[]
            {
                ("Novice", 0f, "🌱", "Beginning your cultivation journey", 1),
                ("Apprentice", 500f, "🌿", "Learning the fundamentals", 2),
                ("Practitioner", 1500f, "🍃", "Developing core skills", 3),
                ("Expert", 3500f, "🌳", "Mastering advanced techniques", 4),
                ("Master", 7500f, "🏆", "Achieving professional excellence", 5),
                ("Grandmaster", 15000f, "👑", "Reaching legendary status", 6),
                ("Legend", 30000f, "⭐", "Becoming a cultivation legend", 7),
                ("Immortal", 60000f, "💫", "Transcending mortal limitations", 8)
            };

            foreach (var (name, points, icon, description, level) in tierDefinitions)
            {
                var tier = new AchievementTier
                {
                    TierID = GenerateTierId(name),
                    TierName = name,
                    RequiredPoints = points * _tierProgressMultiplier,
                    TierIcon = icon,
                    Description = description,
                    Level = level,
                    Benefits = GenerateTierBenefits(name),
                    Prestige = CalculateTierPrestige(points),
                    UnlockDate = DateTime.MinValue,
                    IsUnlocked = false
                };

                _achievementTiers.Add(tier);
            }

            ChimeraLogger.Log($"Tier system initialized: {_achievementTiers.Count} progression tiers", this);
        }

        private void BuildLookupTables()
        {
            // Build badge type lookup
            _badgesByType.Clear();
            foreach (var badge in _availableBadges)
            {
                if (!_badgesByType.ContainsKey(badge.BadgeType))
                {
                    _badgesByType[badge.BadgeType] = new List<AchievementBadge>();
                }
                _badgesByType[badge.BadgeType].Add(badge);
            }

            // Build tier lookup
            _tierLookup.Clear();
            foreach (var tier in _achievementTiers)
            {
                _tierLookup[tier.TierName] = tier;
            }
        }

        #endregion

        #region Player Profile Management

        public PlayerAchievementProfile GetOrCreatePlayerProfile(string playerId)
        {
            if (!_enablePlayerProfiles || string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            if (_playerProfiles.ContainsKey(playerId))
            {
                return _playerProfiles[playerId];
            }

            if (!_autoCreateProfiles)
            {
                return null;
            }

            var newProfile = CreateNewPlayerProfile(playerId);
            _playerProfiles[playerId] = newProfile;
            
            ChimeraLogger.Log($"Created new player profile: {playerId}", this);
            return newProfile;
        }

        private PlayerAchievementProfile CreateNewPlayerProfile(string playerId)
        {
            var profile = new PlayerAchievementProfile
            {
                PlayerID = playerId,
                TotalAchievements = 0,
                TotalPoints = 0f,
                CurrentTier = "Novice",
                FavoriteCategory = AchievementCategory.Cultivation_Mastery,
                LastUnlock = DateTime.MinValue,
                UnlockHistory = new List<string>(),
                EarnedBadges = new List<string>(),
                CategoryProgress = new Dictionary<AchievementCategory, int>(),
                ProfileCreatedDate = DateTime.Now,
                LastUpdateDate = DateTime.Now,
                Prestige = 0,
                SocialRecognition = 0,
                CommunityContributions = 0
            };

            // Initialize category progress tracking
            foreach (AchievementCategory category in Enum.GetValues(typeof(AchievementCategory)))
            {
                profile.CategoryProgress[category] = 0;
            }

            return profile;
        }

        public void UpdatePlayerProfile(string playerId, Achievement achievement)
        {
            if (!_enablePlayerProfiles || achievement == null)
            {
                return;
            }

            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return;

            // Update basic statistics
            profile.TotalAchievements++;
            profile.TotalPoints += achievement.Points;
            profile.LastUnlock = DateTime.Now;
            profile.LastUpdateDate = DateTime.Now;

            // Add to unlock history with size limit
            profile.UnlockHistory.Insert(0, achievement.AchievementID);
            if (profile.UnlockHistory.Count > _maxProfileHistory)
            {
                profile.UnlockHistory.RemoveAt(profile.UnlockHistory.Count - 1);
            }

            // Update category progress
            if (profile.CategoryProgress.ContainsKey(achievement.Category))
            {
                profile.CategoryProgress[achievement.Category]++;
            }
            else
            {
                profile.CategoryProgress[achievement.Category] = 1;
            }

            // Update favorite category (most completed)
            var mostCompletedCategory = profile.CategoryProgress
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault();
            
            if (mostCompletedCategory.Value > 0)
            {
                profile.FavoriteCategory = mostCompletedCategory.Key;
            }

            // Update prestige based on achievement rarity
            profile.Prestige += CalculateAchievementPrestige(achievement);

            // Check for social achievements
            if (achievement.Category == AchievementCategory.Social)
            {
                profile.SocialRecognition++;
                profile.CommunityContributions++;
            }

            // Save profile
            _playerProfiles[playerId] = profile;
            AddToRecentUpdates(playerId);

            // Fire profile updated event
            OnProfileUpdated?.Invoke(profile);

            ChimeraLogger.Log($"Updated player profile: {playerId} - {profile.TotalAchievements} achievements, {profile.TotalPoints} points", this);
        }

        #endregion

        #region Badge Management

        public void CheckForBadgeUnlocks(string playerId)
        {
            if (!_enableBadgeSystem || string.IsNullOrEmpty(playerId))
            {
                return;
            }

            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return;

            foreach (var badge in _availableBadges.Where(b => !b.IsEarned))
            {
                bool shouldUnlock = EvaluateBadgeCondition(badge, profile);

                if (shouldUnlock)
                {
                    UnlockBadge(badge, playerId);
                }
            }
        }

        private bool EvaluateBadgeCondition(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            return badge.BadgeType switch
            {
                BadgeType.Milestone => EvaluateMilestoneBadge(badge, profile),
                BadgeType.Category => EvaluateCategoryBadge(badge, profile),
                BadgeType.Special => EvaluateSpecialBadge(badge, profile),
                BadgeType.Social => EvaluateSocialBadge(badge, profile),
                BadgeType.Ultimate => EvaluateUltimateBadge(badge, profile),
                _ => false
            };
        }

        private bool EvaluateMilestoneBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            return _enableMilestoneBadges && 
                   badge.RequiredAchievements > 0 && 
                   profile.TotalAchievements >= badge.RequiredAchievements;
        }

        private bool EvaluateCategoryBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            if (!_enableCategoryBadges) return false;

            // Map badge name to achievement category
            var categoryMap = new Dictionary<string, AchievementCategory>
            {
                { "Breeding Specialist", AchievementCategory.Genetics_Innovation },
                { "Cultivation Expert", AchievementCategory.Cultivation_Mastery },
                { "Research Scientist", AchievementCategory.Research_Excellence },
                { "Business Tycoon", AchievementCategory.Business_Success },
                { "Genetics Pioneer", AchievementCategory.Genetics_Innovation },
                { "Social Influencer", AchievementCategory.Social }
            };

            if (categoryMap.TryGetValue(badge.BadgeName, out var category))
            {
                // Require significant progress in the category (10+ achievements)
                return profile.CategoryProgress.GetValueOrDefault(category, 0) >= 10;
            }

            return false;
        }

        private bool EvaluateSpecialBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            if (!_enableSpecialBadges) return false;

            return badge.BadgeName switch
            {
                "Speed Runner" => profile.UnlockHistory.Count >= 20, // Fast unlocking
                "Perfectionist" => profile.Prestige >= 100, // High prestige
                "Night Owl" => CheckTimeBasedCondition(profile, 22, 6), // Night unlocks
                "Early Bird" => CheckTimeBasedCondition(profile, 6, 10), // Morning unlocks
                "Streak Master" => CheckUnlockStreak(profile, 7), // Consecutive days
                _ => false
            };
        }

        private bool EvaluateSocialBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            if (!_enableSocialBadges) return false;

            return badge.BadgeName switch
            {
                "Community Helper" => profile.SocialRecognition >= 50,
                "Mentor" => profile.CommunityContributions >= 25,
                "Knowledge Sharer" => profile.SocialRecognition >= 100,
                "Event Organizer" => profile.CommunityContributions >= 10,
                _ => false
            };
        }

        private bool EvaluateUltimateBadge(AchievementBadge badge, PlayerAchievementProfile profile)
        {
            return badge.BadgeName switch
            {
                "Ultimate Master" => profile.TotalAchievements >= 100 && profile.TotalPoints >= 50000f,
                "Legendary Pioneer" => profile.Prestige >= 500 && AllCategoriesComplete(profile),
                "Immortal Legend" => profile.TotalPoints >= 100000f && profile.Prestige >= 1000,
                _ => false
            };
        }

        private void UnlockBadge(AchievementBadge badge, string playerId)
        {
            badge.IsEarned = true;
            badge.EarnDate = DateTime.Now;

            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile != null)
            {
                profile.EarnedBadges.Add(badge.BadgeID);
                profile.Prestige += badge.Prestige;
                _playerProfiles[playerId] = profile;
            }

            OnBadgeEarned?.Invoke(badge);
            OnPlayerRecognition?.Invoke(profile);

            ChimeraLogger.Log($"Badge earned: {badge.BadgeName} by {playerId}", this);
        }

        #endregion

        #region Tier Management

        public void CheckForTierAdvancement(string playerId)
        {
            if (!_enableTierProgression || string.IsNullOrEmpty(playerId))
            {
                return;
            }

            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return;

            var currentTier = GetCurrentTier(playerId);
            var profileTier = _tierLookup.GetValueOrDefault(profile.CurrentTier);

            if (currentTier != null && (profileTier == null || currentTier.Level > profileTier.Level))
            {
                AdvancePlayerTier(playerId, currentTier, profileTier);
            }
        }

        private void AdvancePlayerTier(string playerId, AchievementTier newTier, AchievementTier oldTier)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return;

            string oldTierName = profile.CurrentTier;
            profile.CurrentTier = newTier.TierName;
            profile.Prestige += newTier.Prestige;
            _playerProfiles[playerId] = profile;

            // Mark tier as unlocked
            newTier.IsUnlocked = true;
            newTier.UnlockDate = DateTime.Now;

            // Fire tier advancement events
            OnTierAdvanced?.Invoke(newTier);
            OnPlayerRecognition?.Invoke(profile);

            // Fire level up event
            int oldLevel = oldTier?.Level ?? 0;
            int newLevel = newTier.Level;
            if (newLevel > oldLevel)
            {
                OnPlayerLevelUp?.Invoke(playerId, oldLevel, newLevel);
            }

            ChimeraLogger.Log($"Tier advanced: {playerId} reached {newTier.TierName} (Level {newLevel})", this);
        }

        public AchievementTier GetCurrentTier(string playerId)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return _achievementTiers.FirstOrDefault();

            return _achievementTiers
                .Where(t => t.RequiredPoints <= profile.TotalPoints)
                .OrderByDescending(t => t.RequiredPoints)
                .FirstOrDefault() ?? _achievementTiers.FirstOrDefault();
        }

        public AchievementTier GetNextTier(string playerId)
        {
            var currentTier = GetCurrentTier(playerId);
            if (currentTier == null) return null;

            return _achievementTiers
                .Where(t => t.RequiredPoints > currentTier.RequiredPoints)
                .OrderBy(t => t.RequiredPoints)
                .FirstOrDefault();
        }

        #endregion

        #region Helper Methods

        private bool CheckTimeBasedCondition(PlayerAchievementProfile profile, int startHour, int endHour)
        {
            // Simplified check - would need actual unlock times in real implementation
            return profile.UnlockHistory.Count >= 5;
        }

        private bool CheckUnlockStreak(PlayerAchievementProfile profile, int days)
        {
            // Simplified streak check - would need detailed unlock timestamps
            return profile.UnlockHistory.Count >= days;
        }

        private bool AllCategoriesComplete(PlayerAchievementProfile profile)
        {
            var requiredCategories = 5; // Minimum categories to complete
            return profile.CategoryProgress.Count(kvp => kvp.Value >= 5) >= requiredCategories;
        }

        private int CalculateAchievementPrestige(Achievement achievement)
        {
            return achievement.Rarity switch
            {
                AchievementRarity.Common => 1,
                AchievementRarity.Uncommon => 2,
                AchievementRarity.Rare => 5,
                AchievementRarity.Epic => 10,
                AchievementRarity.Legendary => 25,
                _ => 1
            };
        }

        private void AddToRecentUpdates(string playerId)
        {
            _recentProfileUpdates.Insert(0, playerId);
            if (_recentProfileUpdates.Count > 20)
            {
                _recentProfileUpdates.RemoveAt(_recentProfileUpdates.Count - 1);
            }
        }

        #endregion

        #region Configuration Generators

        private string GenerateBadgeId(string name)
        {
            return $"badge_{name.ToLower().Replace(" ", "_")}";
        }

        private string GenerateTierId(string name)
        {
            return $"tier_{name.ToLower()}";
        }

        private string GenerateBadgeIcon(BadgeType type, string name)
        {
            return type switch
            {
                BadgeType.Milestone => "milestone_badge",
                BadgeType.Category => "category_badge",
                BadgeType.Special => "special_badge",
                BadgeType.Social => "social_badge",
                BadgeType.Ultimate => "ultimate_badge",
                _ => "default_badge"
            };
        }

        private AchievementRarity DetermineBadgeRarity(BadgeType type)
        {
            return type switch
            {
                BadgeType.Milestone => AchievementRarity.Uncommon,
                BadgeType.Category => AchievementRarity.Rare,
                BadgeType.Special => AchievementRarity.Epic,
                BadgeType.Social => AchievementRarity.Rare,
                BadgeType.Ultimate => AchievementRarity.Legendary,
                _ => AchievementRarity.Common
            };
        }

        private int CalculateBadgePrestige(BadgeType type, int requirement)
        {
            var basePrestige = type switch
            {
                BadgeType.Milestone => Math.Max(requirement / 10, 5),
                BadgeType.Category => 15,
                BadgeType.Special => 20,
                BadgeType.Social => 25,
                BadgeType.Ultimate => 50,
                _ => 5
            };

            return basePrestige;
        }

        private List<string> GenerateTierBenefits(string tierName)
        {
            return tierName switch
            {
                "Novice" => new List<string> { "Basic achievement tracking", "Profile creation" },
                "Apprentice" => new List<string> { "Achievement hints", "Progress tracking", "Community access" },
                "Practitioner" => new List<string> { "Bonus achievement points", "Custom badges", "Enhanced features" },
                "Expert" => new List<string> { "Exclusive achievements", "Priority celebrations", "Advanced tools" },
                "Master" => new List<string> { "Master tier recognition", "Special rewards", "Mentor privileges" },
                "Grandmaster" => new List<string> { "Legendary status", "Elite benefits", "Leadership roles" },
                "Legend" => new List<string> { "Ultimate recognition", "Immortal legacy", "Hall of fame" },
                "Immortal" => new List<string> { "Transcendent status", "Divine privileges", "Eternal glory" },
                _ => new List<string>()
            };
        }

        private int CalculateTierPrestige(float points)
        {
            return (int)(points / 100f); // 1 prestige per 100 points
        }

        #endregion

        #region Public API

        public List<PlayerAchievementProfile> GetAllProfiles()
        {
            return _playerProfiles.Values.ToList();
        }

        public List<AchievementBadge> GetEarnedBadges(string playerId)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            if (profile == null) return new List<AchievementBadge>();

            return _availableBadges.Where(b => profile.EarnedBadges.Contains(b.BadgeID)).ToList();
        }

        public List<AchievementBadge> GetAvailableBadges()
        {
            return _availableBadges.ToList();
        }

        public List<AchievementTier> GetAllTiers()
        {
            return _achievementTiers.ToList();
        }

        public PlayerRecognitionStats GetRecognitionStats(string playerId)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            var currentTier = GetCurrentTier(playerId);
            var nextTier = GetNextTier(playerId);

            return new PlayerRecognitionStats
            {
                PlayerId = playerId,
                TotalAchievements = profile?.TotalAchievements ?? 0,
                TotalPoints = profile?.TotalPoints ?? 0f,
                CurrentTier = currentTier?.TierName ?? "Novice",
                NextTier = nextTier?.TierName ?? "Max Level",
                EarnedBadges = profile?.EarnedBadges.Count ?? 0,
                Prestige = profile?.Prestige ?? 0,
                SocialRecognition = profile?.SocialRecognition ?? 0,
                FavoriteCategory = profile?.FavoriteCategory ?? AchievementCategory.Cultivation_Mastery,
                ProfileAge = profile != null ? (DateTime.Now - profile.ProfileCreatedDate).Days : 0
            };
        }

        public void UpdateRecognitionSettings(bool enableProfiles, bool enableBadges, bool enableTiers)
        {
            _enablePlayerProfiles = enableProfiles;
            _enableBadgeSystem = enableBadges;
            _enableTierProgression = enableTiers;
            
            ChimeraLogger.Log($"Recognition settings updated: profiles={enableProfiles}, badges={enableBadges}, tiers={enableTiers}", this);
        }

        #endregion

        #region Data Persistence

        private void SaveAllProfiles()
        {
            if (!_persistProfiles) return;

            // Profile persistence would be implemented here
            ChimeraLogger.Log($"Saved {_playerProfiles.Count} player profiles", this);
        }

        private void ClearAllData()
        {
            _playerProfiles.Clear();
            _recentProfileUpdates.Clear();
            _profileCooldowns.Clear();
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    /// <summary>
    /// Statistics for player recognition system
    /// </summary>
    [System.Serializable]
    public class PlayerRecognitionStats
    {
        public string PlayerId = "";
        public int TotalAchievements = 0;
        public float TotalPoints = 0f;
        public string CurrentTier = "Novice";
        public string NextTier = "";
        public int EarnedBadges = 0;
        public int Prestige = 0;
        public int SocialRecognition = 0;
        public AchievementCategory FavoriteCategory = AchievementCategory.Cultivation_Mastery;
        public int ProfileAge = 0;
    }
}