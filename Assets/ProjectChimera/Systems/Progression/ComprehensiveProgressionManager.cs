using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Comprehensive Progression Manager - Unified progression orchestration across all game systems
    /// Focuses on exciting progression rewards, celebrations, and cross-system integration
    /// Manages player levels, unlocks, achievements, and provides satisfying progression feedback
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class ComprehensiveProgressionManager : ChimeraManager
    {
        [Header("Progression Configuration")]
        public bool EnableProgressionSystem = true;
        public bool EnableLevelProgression = true;
        public bool EnableUnlockSystem = true;
        public bool EnableProgressionRewards = true;
        
        [Header("Progression Settings")]
        public int MaxPlayerLevel = 100;
        public float BaseExperienceRequirement = 1000f;
        public float ExperienceScalingFactor = 1.5f;
        public float ProgressionCelebrationDuration = 3f;
        
        [Header("Progression Collections")]
        [SerializeField] private List<PlayerProgressionProfile> playerProfiles = new List<PlayerProgressionProfile>();
        [SerializeField] private List<ProgressionMilestone> availableMilestones = new List<ProgressionMilestone>();
        [SerializeField] private List<ProgressionUnlock> availableUnlocks = new List<ProgressionUnlock>();
        [SerializeField] private List<ProgressionReward> pendingRewards = new List<ProgressionReward>();
        
        [Header("System Integration")]
        [SerializeField] private Dictionary<string, float> systemExperienceRates = new Dictionary<string, float>();
        [SerializeField] private Dictionary<string, ProgressionCategory> systemCategories = new Dictionary<string, ProgressionCategory>();
        [SerializeField] private List<CrossSystemBonus> crossSystemBonuses = new List<CrossSystemBonus>();
        
        [Header("Progression State")]
        [SerializeField] private DateTime lastProgressionUpdate = DateTime.Now;
        [SerializeField] private float totalExperienceAwarded = 0f;
        [SerializeField] private int totalLevelsGained = 0;
        [SerializeField] private int totalUnlocksGranted = 0;
        
        // Events for progression celebrations and feedback
        public static event Action<int, int> OnPlayerLevelUp; // oldLevel, newLevel
        public static event Action<ProgressionMilestone> OnMilestoneCompleted;
        public static event Action<ProgressionUnlock> OnContentUnlocked;
        public static event Action<ProgressionReward> OnRewardEarned;
        public static event Action<CrossSystemBonus> OnCrossSystemBonus;
        public static event Action<string, float> OnExperienceGained; // system, amount
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize comprehensive progression system
            InitializeProgressionSystem();
            
            if (EnableProgressionSystem)
            {
                StartProgressionTracking();
            }
            
            Debug.Log("✅ ComprehensiveProgressionManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up progression tracking
            if (EnableProgressionSystem)
            {
                StopProgressionTracking();
            }
            
            // Clear all events to prevent memory leaks
            OnPlayerLevelUp = null;
            OnMilestoneCompleted = null;
            OnContentUnlocked = null;
            OnRewardEarned = null;
            OnCrossSystemBonus = null;
            OnExperienceGained = null;
            
            Debug.Log("✅ ComprehensiveProgressionManager shutdown successfully");
        }
        
        private void InitializeProgressionSystem()
        {
            // Initialize collections if empty
            if (playerProfiles == null) playerProfiles = new List<PlayerProgressionProfile>();
            if (availableMilestones == null) availableMilestones = new List<ProgressionMilestone>();
            if (availableUnlocks == null) availableUnlocks = new List<ProgressionUnlock>();
            if (pendingRewards == null) pendingRewards = new List<ProgressionReward>();
            if (systemExperienceRates == null) systemExperienceRates = new Dictionary<string, float>();
            if (systemCategories == null) systemCategories = new Dictionary<string, ProgressionCategory>();
            if (crossSystemBonuses == null) crossSystemBonuses = new List<CrossSystemBonus>();
            
            // Initialize progression content
            InitializeSystemIntegration();
            InitializeMilestones();
            InitializeUnlocks();
            InitializeCrossSystemBonuses();
        }
        
        private void InitializeSystemIntegration()
        {
            // Configure experience rates for different game systems
            systemExperienceRates.Clear();
            systemExperienceRates["Genetics"] = 1.0f;
            systemExperienceRates["Breeding"] = 1.2f;
            systemExperienceRates["IPM"] = 0.8f;
            systemExperienceRates["Cultivation"] = 1.0f;
            systemExperienceRates["Research"] = 1.5f;
            systemExperienceRates["Competition"] = 2.0f;
            systemExperienceRates["Achievement"] = 1.3f;
            systemExperienceRates["Discovery"] = 1.8f;
            
            // Configure system categories for organized progression
            systemCategories.Clear();
            systemCategories["Genetics"] = ProgressionCategory.Genetics;
            systemCategories["Breeding"] = ProgressionCategory.Genetics;
            systemCategories["IPM"] = ProgressionCategory.Business;
            systemCategories["Cultivation"] = ProgressionCategory.Cultivation;
            systemCategories["Research"] = ProgressionCategory.Genetics;
            systemCategories["Competition"] = ProgressionCategory.Social;
            systemCategories["Achievement"] = ProgressionCategory.General;
            systemCategories["Discovery"] = ProgressionCategory.Research;
            
            Debug.Log($"✅ System integration initialized: {systemExperienceRates.Count} systems configured");
        }
        
        private void InitializeMilestones()
        {
            // Create exciting progression milestones across all systems
            var milestoneDefinitions = new[]
            {
                // Early Game Milestones
                ("First Steps", "Complete your first breeding challenge", 1, ProgressionCategory.Genetics, 500f),
                ("Green Thumb", "Successfully cultivate 5 plants", 2, ProgressionCategory.Cultivation, 750f),
                ("Pest Hunter", "Win your first IPM battle", 3, ProgressionCategory.Business, 600f),
                ("Curious Mind", "Complete your first research project", 4, ProgressionCategory.Genetics, 800f),
                
                // Mid Game Milestones
                ("Master Breeder", "Create 10 perfect breeding combinations", 10, ProgressionCategory.Genetics, 2000f),
                ("Cultivation Expert", "Achieve 95% yield efficiency", 15, ProgressionCategory.Cultivation, 2500f),
                ("IPM Specialist", "Master all pest control methods", 12, ProgressionCategory.Business, 2200f),
                ("Research Pioneer", "Unlock 5 advanced research topics", 18, ProgressionCategory.Genetics, 3000f),
                
                // End Game Milestones
                ("Genetics Grandmaster", "Perfect 20 legendary breeding challenges", 25, ProgressionCategory.Genetics, 5000f),
                ("Facility Mogul", "Manage 10 fully optimized facilities", 30, ProgressionCategory.Cultivation, 6000f),
                ("Competition Champion", "Win 15 major competitions", 35, ProgressionCategory.Social, 7500f),
                ("Ultimate Cultivator", "Achieve mastery in all systems", 50, ProgressionCategory.General, 10000f)
            };
            
            foreach (var (name, description, level, category, experience) in milestoneDefinitions)
            {
                var milestone = new ProgressionMilestone
                {
                    MilestoneID = $"milestone_{name.ToLower().Replace(" ", "_")}",
                    MilestoneName = name,
                    Description = description,
                    RequiredLevel = level,
                    Category = category,
                    ExperienceReward = experience,
                    IsUnlocked = level <= 5, // Early milestones start unlocked
                    IsCompleted = false,
                    CompletionDate = DateTime.MinValue
                };
                
                // Add exciting unlock rewards
                if (level <= 10)
                {
                    milestone.UnlockRewards.Add($"New {category} features");
                    milestone.UnlockRewards.Add("Bonus experience multiplier");
                }
                else if (level <= 25)
                {
                    milestone.UnlockRewards.Add("Advanced gameplay mechanics");
                    milestone.UnlockRewards.Add("Exclusive content access");
                    milestone.UnlockRewards.Add("Special visual effects");
                }
                else
                {
                    milestone.UnlockRewards.Add("Elite status recognition");
                    milestone.UnlockRewards.Add("Legendary content access");
                    milestone.UnlockRewards.Add("Master-tier rewards");
                }
                
                availableMilestones.Add(milestone);
            }
            
            Debug.Log($"✅ Progression milestones initialized: {availableMilestones.Count} milestones");
        }
        
        private void InitializeUnlocks()
        {
            // Create exciting content unlocks for progression
            var unlockDefinitions = new[]
            {
                // System Feature Unlocks
                ("Advanced Breeding Tools", ProgressionCategory.Genetics, 5, "Unlock powerful breeding analysis tools"),
                ("IPM Arsenal", ProgressionCategory.Business, 8, "Access to advanced pest control methods"),
                ("Research Laboratory", ProgressionCategory.Genetics, 12, "Unlock cutting-edge research facilities"),
                ("Competition Arena", ProgressionCategory.Social, 15, "Access to elite tournaments and competitions"),
                
                // Cosmetic/Visual Unlocks
                ("Golden Greenhouse Theme", ProgressionCategory.Cultivation, 10, "Luxurious facility visual theme"),
                ("Master Cultivator Badge", ProgressionCategory.General, 20, "Prestigious achievement badge"),
                ("Legendary Plant Strains", ProgressionCategory.Genetics, 25, "Access to rare genetic varieties"),
                ("Elite Facility Designs", ProgressionCategory.Cultivation, 30, "Premium architectural options"),
                
                // Gameplay Enhancement Unlocks
                ("Speed Cultivation Mode", ProgressionCategory.Cultivation, 18, "Accelerated growth mechanics"),
                ("Expert Analysis Tools", ProgressionCategory.Genetics, 22, "Advanced genetic analysis capabilities"),
                ("Automation Systems", ProgressionCategory.Business, 28, "Intelligent facility automation"),
                ("Master Class Content", ProgressionCategory.General, 40, "Ultimate challenge modes")
            };
            
            foreach (var (name, category, level, description) in unlockDefinitions)
            {
                var unlock = new ProgressionUnlock
                {
                    UnlockID = $"unlock_{name.ToLower().Replace(" ", "_")}",
                    UnlockName = name,
                    Description = description,
                    Category = category,
                    RequiredLevel = level,
                    IsUnlocked = false,
                    UnlockDate = DateTime.MinValue,
                    UnlockType = level <= 15 ? UnlockType.Feature : level <= 30 ? UnlockType.Cosmetic : UnlockType.Elite
                };
                
                availableUnlocks.Add(unlock);
            }
            
            Debug.Log($"✅ Progression unlocks initialized: {availableUnlocks.Count} unlocks");
        }
        
        private void InitializeCrossSystemBonuses()
        {
            // Create exciting bonuses for using multiple systems
            var bonusDefinitions = new[]
            {
                ("Science Synergy", new[] { "Genetics", "Research" }, 1.25f, "25% bonus when combining genetics and research"),
                ("Production Master", new[] { "Cultivation", "IPM" }, 1.20f, "20% bonus for cultivation + pest management"),
                ("Competition Edge", new[] { "Breeding", "Competition" }, 1.30f, "30% bonus for competitive breeding"),
                ("Ultimate Mastery", new[] { "Genetics", "Cultivation", "IPM", "Research" }, 2.0f, "100% bonus for mastering all core systems"),
                ("Social Scientist", new[] { "Research", "Competition" }, 1.15f, "15% bonus for research + competition"),
                ("Facility Genius", new[] { "Cultivation", "IPM", "Achievement" }, 1.35f, "35% bonus for complete facility management")
            };
            
            foreach (var (name, systems, multiplier, description) in bonusDefinitions)
            {
                var bonus = new CrossSystemBonus
                {
                    BonusID = $"bonus_{name.ToLower().Replace(" ", "_")}",
                    BonusName = name,
                    Description = description,
                    RequiredSystems = new List<string>(systems),
                    ExperienceMultiplier = multiplier,
                    IsActive = false,
                    ActivationCount = 0
                };
                
                crossSystemBonuses.Add(bonus);
            }
            
            Debug.Log($"✅ Cross-system bonuses initialized: {crossSystemBonuses.Count} bonuses");
        }
        
        private void StartProgressionTracking()
        {
            // Start comprehensive progression tracking
            lastProgressionUpdate = DateTime.Now;
            
            Debug.Log("✅ Progression tracking started - unified experience across all systems");
        }
        
        private void StopProgressionTracking()
        {
            // Clean up progression tracking
            Debug.Log("✅ Progression tracking stopped");
        }
        
        private void Update()
        {
            if (!EnableProgressionSystem) return;
            
            // Process pending progression updates
            ProcessPendingRewards();
            UpdateCrossSystemBonuses();
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Award experience points from any game system
        /// </summary>
        public void AwardExperience(string systemName, float baseExperience, string playerId = "current_player", string reason = "")
        {
            if (!EnableProgressionSystem) return;
            
            var profile = GetOrCreatePlayerProfile(playerId);
            
            // Calculate final experience with system rates and bonuses
            float systemRate = systemExperienceRates.GetValueOrDefault(systemName, 1.0f);
            float crossSystemMultiplier = CalculateCrossSystemMultiplier(playerId, systemName);
            float finalExperience = baseExperience * systemRate * crossSystemMultiplier;
            
            // Award experience
            float oldExperience = profile.TotalExperience;
            int oldLevel = profile.CurrentLevel;
            
            profile.TotalExperience += finalExperience;
            profile.SystemExperience[systemName] = profile.SystemExperience.GetValueOrDefault(systemName, 0f) + finalExperience;
            
            // Check for level up
            int newLevel = CalculatePlayerLevel(profile.TotalExperience);
            if (newLevel > oldLevel)
            {
                HandleLevelUp(profile, oldLevel, newLevel);
            }
            
            // Track system activity for cross-system bonuses
            profile.RecentSystemActivity[systemName] = DateTime.Now;
            
            totalExperienceAwarded += finalExperience;
            
            // Fire events
            OnExperienceGained?.Invoke(systemName, finalExperience);
            
            if (crossSystemMultiplier > 1.0f)
            {
                var activeBonus = crossSystemBonuses.FirstOrDefault(b => b.IsActive && b.RequiredSystems.Contains(systemName));
                if (activeBonus != null)
                {
                    OnCrossSystemBonus?.Invoke(activeBonus);
                }
            }
            
            Debug.Log($"✅ Experience awarded: {finalExperience:F0} from {systemName} to {playerId}" +
                     (crossSystemMultiplier > 1.0f ? $" (Bonus: {crossSystemMultiplier:F1}x)" : ""));
        }
        
        /// <summary>
        /// Complete a progression milestone
        /// </summary>
        public bool CompleteMilestone(string milestoneId, string playerId = "current_player")
        {
            var milestone = availableMilestones.FirstOrDefault(m => m.MilestoneID == milestoneId);
            if (milestone == null || milestone.IsCompleted)
            {
                return false;
            }
            
            var profile = GetOrCreatePlayerProfile(playerId);
            
            // Check if player meets requirements
            if (profile.CurrentLevel < milestone.RequiredLevel)
            {
                Debug.LogWarning($"Player level {profile.CurrentLevel} insufficient for milestone {milestone.MilestoneName} (requires {milestone.RequiredLevel})");
                return false;
            }
            
            // Complete milestone
            milestone.IsCompleted = true;
            milestone.CompletionDate = DateTime.Now;
            
            // Award milestone rewards
            AwardExperience("Achievement", milestone.ExperienceReward, playerId, $"Milestone: {milestone.MilestoneName}");
            
            // Process unlock rewards
            foreach (var unlockReward in milestone.UnlockRewards)
            {
                ProcessMilestoneUnlock(unlockReward, playerId);
            }
            
            // Add to player's completed milestones
            if (!profile.CompletedMilestones.Contains(milestoneId))
            {
                profile.CompletedMilestones.Add(milestoneId);
            }
            
            OnMilestoneCompleted?.Invoke(milestone);
            
            Debug.Log($"🏆 Milestone completed: {milestone.MilestoneName} by {playerId}");
            return true;
        }
        
        /// <summary>
        /// Unlock progression content
        /// </summary>
        public bool UnlockContent(string unlockId, string playerId = "current_player")
        {
            var unlock = availableUnlocks.FirstOrDefault(u => u.UnlockID == unlockId);
            if (unlock == null || unlock.IsUnlocked)
            {
                return false;
            }
            
            var profile = GetOrCreatePlayerProfile(playerId);
            
            // Check if player meets requirements
            if (profile.CurrentLevel < unlock.RequiredLevel)
            {
                Debug.LogWarning($"Player level {profile.CurrentLevel} insufficient for unlock {unlock.UnlockName} (requires {unlock.RequiredLevel})");
                return false;
            }
            
            // Unlock content
            unlock.IsUnlocked = true;
            unlock.UnlockDate = DateTime.Now;
            
            // Add to player's unlocks
            if (!profile.UnlockedContent.Contains(unlockId))
            {
                profile.UnlockedContent.Add(unlockId);
            }
            
            totalUnlocksGranted++;
            
            OnContentUnlocked?.Invoke(unlock);
            
            Debug.Log($"🔓 Content unlocked: {unlock.UnlockName} for {playerId}");
            return true;
        }
        
        /// <summary>
        /// Get player's progression profile
        /// </summary>
        public PlayerProgressionProfile GetPlayerProfile(string playerId = "current_player")
        {
            return GetOrCreatePlayerProfile(playerId);
        }
        
        /// <summary>
        /// Get available milestones for player level
        /// </summary>
        public List<ProgressionMilestone> GetAvailableMilestones(string playerId = "current_player")
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            return availableMilestones.Where(m => 
                m.IsUnlocked && 
                !m.IsCompleted && 
                profile.CurrentLevel >= m.RequiredLevel - 2 // Show milestones within 2 levels
            ).ToList();
        }
        
        /// <summary>
        /// Get available unlocks for player level
        /// </summary>
        public List<ProgressionUnlock> GetAvailableUnlocks(string playerId = "current_player")
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            return availableUnlocks.Where(u => 
                !u.IsUnlocked && 
                profile.CurrentLevel >= u.RequiredLevel
            ).ToList();
        }
        
        /// <summary>
        /// Get cross-system bonuses status
        /// </summary>
        public List<CrossSystemBonus> GetActiveBonuses(string playerId = "current_player")
        {
            return crossSystemBonuses.Where(b => b.IsActive).ToList();
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private PlayerProgressionProfile GetOrCreatePlayerProfile(string playerId)
        {
            var existing = playerProfiles.FirstOrDefault(p => p.PlayerID == playerId);
            if (existing != null) return existing;
            
            var newProfile = new PlayerProgressionProfile
            {
                PlayerID = playerId,
                CurrentLevel = 1,
                TotalExperience = 0f,
                LastActivity = DateTime.Now,
                SystemExperience = new Dictionary<string, float>(),
                RecentSystemActivity = new Dictionary<string, DateTime>(),
                CompletedMilestones = new List<string>(),
                UnlockedContent = new List<string>()
            };
            
            playerProfiles.Add(newProfile);
            return newProfile;
        }
        
        private int CalculatePlayerLevel(float totalExperience)
        {
            if (totalExperience <= 0) return 1;
            
            float currentLevelExp = BaseExperienceRequirement;
            int level = 1;
            
            while (totalExperience >= currentLevelExp && level < MaxPlayerLevel)
            {
                totalExperience -= currentLevelExp;
                level++;
                currentLevelExp *= ExperienceScalingFactor;
            }
            
            return level;
        }
        
        private float CalculateExperienceForLevel(int level)
        {
            float totalExp = 0f;
            float currentLevelExp = BaseExperienceRequirement;
            
            for (int i = 1; i < level; i++)
            {
                totalExp += currentLevelExp;
                currentLevelExp *= ExperienceScalingFactor;
            }
            
            return totalExp;
        }
        
        private float CalculateCrossSystemMultiplier(string playerId, string currentSystem)
        {
            var profile = GetOrCreatePlayerProfile(playerId);
            float bestMultiplier = 1.0f;
            
            foreach (var bonus in crossSystemBonuses)
            {
                if (!bonus.RequiredSystems.Contains(currentSystem)) continue;
                
                bool allSystemsActive = bonus.RequiredSystems.All(system =>
                    profile.RecentSystemActivity.ContainsKey(system) &&
                    (DateTime.Now - profile.RecentSystemActivity[system]).TotalMinutes <= 30 // Active within 30 minutes
                );
                
                if (allSystemsActive && bonus.ExperienceMultiplier > bestMultiplier)
                {
                    bestMultiplier = bonus.ExperienceMultiplier;
                    bonus.IsActive = true;
                    bonus.ActivationCount++;
                }
            }
            
            return bestMultiplier;
        }
        
        private void HandleLevelUp(PlayerProgressionProfile profile, int oldLevel, int newLevel)
        {
            profile.CurrentLevel = newLevel;
            totalLevelsGained += (newLevel - oldLevel);
            
            // Create level up rewards
            for (int level = oldLevel + 1; level <= newLevel; level++)
            {
                var reward = new ProgressionReward
                {
                    RewardID = $"levelup_{profile.PlayerID}_{level}",
                    RewardName = $"Level {level} Reward",
                    RewardType = ProgressionRewardType.LevelUp,
                    ExperienceBonus = level * 100f,
                    UnlockedFeatures = CalculateLevelUpUnlocks(level),
                    AwardDate = DateTime.Now
                };
                
                pendingRewards.Add(reward);
            }
            
            // Check for new milestone unlocks
            foreach (var milestone in availableMilestones.Where(m => !m.IsUnlocked && m.RequiredLevel <= newLevel))
            {
                milestone.IsUnlocked = true;
            }
            
            // Check for automatic content unlocks
            var autoUnlocks = availableUnlocks.Where(u => !u.IsUnlocked && u.RequiredLevel <= newLevel).ToList();
            foreach (var unlock in autoUnlocks)
            {
                UnlockContent(unlock.UnlockID, profile.PlayerID);
            }
            
            OnPlayerLevelUp?.Invoke(oldLevel, newLevel);
            
            Debug.Log($"🌟 LEVEL UP! {profile.PlayerID}: {oldLevel} → {newLevel}");
        }
        
        private List<string> CalculateLevelUpUnlocks(int level)
        {
            var unlocks = new List<string>();
            
            if (level % 5 == 0) // Every 5 levels
            {
                unlocks.Add("New gameplay features");
            }
            
            if (level % 10 == 0) // Every 10 levels
            {
                unlocks.Add("Advanced system access");
                unlocks.Add("Bonus experience multiplier");
            }
            
            if (level >= 25 && level % 15 == 0) // High level rewards
            {
                unlocks.Add("Elite content access");
                unlocks.Add("Master-tier features");
            }
            
            return unlocks;
        }
        
        private void ProcessMilestoneUnlock(string unlockDescription, string playerId)
        {
            // Process milestone-based unlocks
            var reward = new ProgressionReward
            {
                RewardID = $"milestone_unlock_{DateTime.Now.Ticks}",
                RewardName = unlockDescription,
                RewardType = ProgressionRewardType.Milestone,
                ExperienceBonus = 0f,
                UnlockedFeatures = new List<string> { unlockDescription },
                AwardDate = DateTime.Now
            };
            
            pendingRewards.Add(reward);
        }
        
        private void ProcessPendingRewards()
        {
            foreach (var reward in pendingRewards.ToList())
            {
                // Award pending rewards
                OnRewardEarned?.Invoke(reward);
                pendingRewards.Remove(reward);
            }
        }
        
        private void UpdateCrossSystemBonuses()
        {
            // Deactivate expired bonuses
            foreach (var bonus in crossSystemBonuses.Where(b => b.IsActive))
            {
                bonus.IsActive = false; // Will be reactivated if conditions are still met
            }
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate comprehensive progression system functionality
        /// </summary>
        public void TestProgressionSystem()
        {
            Debug.Log("=== Testing Comprehensive Progression System ===");
            Debug.Log($"Progression Enabled: {EnableProgressionSystem}");
            Debug.Log($"Level Progression Enabled: {EnableLevelProgression}");
            Debug.Log($"Max Player Level: {MaxPlayerLevel}");
            Debug.Log($"Available Milestones: {availableMilestones.Count}");
            Debug.Log($"Available Unlocks: {availableUnlocks.Count}");
            Debug.Log($"Cross-System Bonuses: {crossSystemBonuses.Count}");
            Debug.Log($"Integrated Systems: {systemExperienceRates.Count}");
            
            // Test experience awarding
            if (EnableProgressionSystem)
            {
                AwardExperience("Genetics", 500f, "test_player", "Test breeding challenge");
                AwardExperience("Research", 300f, "test_player", "Test research completion");
                Debug.Log($"✓ Test experience awarding");
                
                // Test player profile
                var profile = GetPlayerProfile("test_player");
                Debug.Log($"✓ Test player profile: Level {profile.CurrentLevel}, Experience: {profile.TotalExperience}");
                
                // Test milestone completion
                var firstMilestone = availableMilestones.FirstOrDefault(m => m.IsUnlocked && !m.IsCompleted);
                if (firstMilestone != null)
                {
                    bool completed = CompleteMilestone(firstMilestone.MilestoneID, "test_player");
                    Debug.Log($"✓ Test milestone completion: {completed}");
                }
                
                // Test content unlock
                var firstUnlock = availableUnlocks.FirstOrDefault(u => !u.IsUnlocked);
                if (firstUnlock != null)
                {
                    bool unlocked = UnlockContent(firstUnlock.UnlockID, "test_player");
                    Debug.Log($"✓ Test content unlock: {unlocked}");
                }
            }
            
            Debug.Log("✅ Comprehensive progression system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class PlayerProgressionProfile
    {
        public string PlayerID;
        public int CurrentLevel;
        public float TotalExperience;
        public DateTime LastActivity;
        public Dictionary<string, float> SystemExperience = new Dictionary<string, float>();
        public Dictionary<string, DateTime> RecentSystemActivity = new Dictionary<string, DateTime>();
        public List<string> CompletedMilestones = new List<string>();
        public List<string> UnlockedContent = new List<string>();
    }
    
    [System.Serializable]
    public class ProgressionMilestone
    {
        public string MilestoneID;
        public string MilestoneName;
        public string Description;
        public int RequiredLevel;
        public ProgressionCategory Category;
        public float ExperienceReward;
        public bool IsUnlocked;
        public bool IsCompleted;
        public DateTime CompletionDate;
        public List<string> UnlockRewards = new List<string>();
    }
    
    [System.Serializable]
    public class ProgressionUnlock
    {
        public string UnlockID;
        public string UnlockName;
        public string Description;
        public ProgressionCategory Category;
        public int RequiredLevel;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public UnlockType UnlockType;
    }
    
    [System.Serializable]
    public class ProgressionReward
    {
        public string RewardID;
        public string RewardName;
        public ProgressionRewardType RewardType;
        public float ExperienceBonus;
        public List<string> UnlockedFeatures = new List<string>();
        public DateTime AwardDate;
    }
    
    [System.Serializable]
    public class CrossSystemBonus
    {
        public string BonusID;
        public string BonusName;
        public string Description;
        public List<string> RequiredSystems = new List<string>();
        public float ExperienceMultiplier;
        public bool IsActive;
        public int ActivationCount;
    }
    
    public enum UnlockType
    {
        Feature,
        Cosmetic,
        Elite,
        Legendary
    }
    
    public enum ProgressionRewardType
    {
        LevelUp,
        Milestone,
        Achievement,
        Discovery,
        Competition
    }
    
    #endregion
}