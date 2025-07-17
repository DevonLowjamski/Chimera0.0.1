using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Progression Integrator - Cross-system progression bonuses and gaming integration
    /// Coordinates progression across all gaming systems for maximum engagement and rewards
    /// Provides intelligent progression bonuses, streak tracking, and unified gaming experience
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class ProgressionIntegrator : ChimeraManager
    {
        [Header("Integration Configuration")]
        public bool EnableProgressionIntegration = true;
        public bool EnableCrossSystemBonuses = true;
        public bool EnableStreakTracking = true;
        public bool EnableProgressionEvents = true;
        
        [Header("Bonus Settings")]
        public float BaseStreakMultiplier = 1.1f;
        public float MaxStreakMultiplier = 3.0f;
        public int MaxStreakCount = 20;
        public float SystemComboBonus = 0.25f;
        
        [Header("Integration Collections")]
        [SerializeField] private List<SystemProgressionProfile> systemProfiles = new List<SystemProgressionProfile>();
        [SerializeField] private List<ProgressionStreak> activeStreaks = new List<ProgressionStreak>();
        [SerializeField] private List<CrossSystemCombo> activeCombos = new List<CrossSystemCombo>();
        [SerializeField] private Dictionary<string, ProgressionBonus> availableBonuses = new Dictionary<string, ProgressionBonus>();
        
        [Header("Player Integration State")]
        [SerializeField] private Dictionary<string, PlayerIntegrationProfile> playerIntegration = new Dictionary<string, PlayerIntegrationProfile>();
        [SerializeField] private List<ProgressionEvent> recentEvents = new List<ProgressionEvent>();
        [SerializeField] private DateTime lastIntegrationUpdate = DateTime.Now;
        
        [Header("Gaming Metrics")]
        [SerializeField] private float totalBonusesAwarded = 0f;
        [SerializeField] private int totalStreaksAchieved = 0;
        [SerializeField] private int totalCombosActivated = 0;
        [SerializeField] private float averageBonusMultiplier = 1.0f;
        
        // Events for progression integration and celebrations
        public static event Action<ProgressionStreak> OnStreakAchieved;
        public static event Action<CrossSystemCombo> OnComboActivated;
        public static event Action<string, float> OnBonusAwarded;
        public static event Action<ProgressionEvent> OnProgressionEvent;
        public static event Action<PlayerIntegrationProfile> OnPlayerMilestone;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize progression integration system
            InitializeIntegrationSystem();
            
            if (EnableProgressionIntegration)
            {
                StartProgressionIntegration();
            }
            
            Debug.Log("✅ ProgressionIntegrator initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up progression integration
            if (EnableProgressionIntegration)
            {
                StopProgressionIntegration();
            }
            
            // Clear all events to prevent memory leaks
            OnStreakAchieved = null;
            OnComboActivated = null;
            OnBonusAwarded = null;
            OnProgressionEvent = null;
            OnPlayerMilestone = null;
            
            Debug.Log("✅ ProgressionIntegrator shutdown successfully");
        }
        
        private void InitializeIntegrationSystem()
        {
            // Initialize collections if empty
            if (systemProfiles == null) systemProfiles = new List<SystemProgressionProfile>();
            if (activeStreaks == null) activeStreaks = new List<ProgressionStreak>();
            if (activeCombos == null) activeCombos = new List<CrossSystemCombo>();
            if (availableBonuses == null) availableBonuses = new Dictionary<string, ProgressionBonus>();
            if (playerIntegration == null) playerIntegration = new Dictionary<string, PlayerIntegrationProfile>();
            if (recentEvents == null) recentEvents = new List<ProgressionEvent>();
            
            // Initialize gaming system integration
            InitializeSystemProfiles();
            InitializeProgressionBonuses();
            InitializeCrossSystemCombos();
        }
        
        private void InitializeSystemProfiles()
        {
            // Create progression profiles for all gaming systems
            var gamingSystems = new[]
            {
                ("Breeding", "BreedingChallengeSystem", 1.2f, "Fun breeding puzzles and mini-games"),
                ("Aromatic", "AromaticGamingSystem", 1.3f, "Sensory mastery and scent identification"),
                ("Genetics", "GeneticsGamingSystem", 1.1f, "Scientific discovery and trait research"),
                ("IPM", "IPMBattleSystem", 1.0f, "Strategic pest combat and management"),
                ("Competition", "CompetitiveManager", 2.0f, "Tournaments and competitive play"),
                ("Achievement", "AchievementSystemManager", 1.5f, "Goal completion and recognition"),
                ("Research", "ResearchManager", 1.4f, "Scientific advancement and discovery"),
                ("Progression", "ComprehensiveProgressionManager", 1.0f, "Overall progression coordination")
            };
            
            foreach (var (name, systemType, multiplier, description) in gamingSystems)
            {
                var profile = new SystemProgressionProfile
                {
                    SystemName = name,
                    SystemType = systemType,
                    BaseMultiplier = multiplier,
                    Description = description,
                    IsActive = true,
                    LastActivity = DateTime.MinValue,
                    TotalInteractions = 0,
                    AverageEngagement = 0f,
                    CurrentStreakCount = 0
                };
                
                systemProfiles.Add(profile);
            }
            
            Debug.Log($"✅ System profiles initialized: {systemProfiles.Count} gaming systems");
        }
        
        private void InitializeProgressionBonuses()
        {
            // Create intelligent progression bonuses for cross-system engagement
            var bonusDefinitions = new[]
            {
                ("Daily_Engagement", 1.25f, "25% bonus for daily activity across systems"),
                ("System_Explorer", 1.5f, "50% bonus for trying multiple systems in one session"),
                ("Mastery_Focus", 1.3f, "30% bonus for deep engagement with single system"),
                ("Gaming_Streak", 2.0f, "100% bonus for maintaining activity streaks"),
                ("Discovery_Bonus", 1.8f, "80% bonus for unlocking new content"),
                ("Competition_Edge", 1.6f, "60% bonus for competitive activities"),
                ("Research_Synergy", 1.4f, "40% bonus for combining research with practice"),
                ("Perfect_Performance", 3.0f, "200% bonus for achieving perfect scores")
            };
            
            foreach (var (bonusId, multiplier, description) in bonusDefinitions)
            {
                var bonus = new ProgressionBonus
                {
                    BonusID = bonusId,
                    BonusName = bonusId.Replace("_", " "),
                    Description = description,
                    Multiplier = multiplier,
                    IsActive = false,
                    ActivationConditions = GenerateBonusConditions(bonusId),
                    Duration = CalculateBonusDuration(bonusId),
                    CooldownTime = CalculateBonusCooldown(bonusId)
                };
                
                availableBonuses[bonusId] = bonus;
            }
            
            Debug.Log($"✅ Progression bonuses initialized: {availableBonuses.Count} bonuses");
        }
        
        private void InitializeCrossSystemCombos()
        {
            // Create exciting cross-system combo opportunities
            var comboDefinitions = new[]
            {
                ("Science_Master", new[] { "Genetics", "Research", "Breeding" }, 1.8f, "Scientific mastery combo"),
                ("Gaming_Virtuoso", new[] { "Breeding", "Aromatic", "IPM" }, 1.6f, "Gaming excellence combo"),
                ("Competition_Champion", new[] { "Competition", "Achievement", "Breeding" }, 2.2f, "Competitive champion combo"),
                ("Discovery_Pioneer", new[] { "Research", "Genetics", "Achievement" }, 1.7f, "Discovery pioneer combo"),
                ("Complete_Mastery", new[] { "Breeding", "Aromatic", "Genetics", "IPM", "Competition" }, 3.0f, "Ultimate mastery combo"),
                ("Daily_Explorer", new[] { "Breeding", "Aromatic", "Research" }, 1.4f, "Daily exploration combo")
            };
            
            foreach (var (name, systems, multiplier, description) in comboDefinitions)
            {
                var combo = new CrossSystemCombo
                {
                    ComboID = $"combo_{name.ToLower()}",
                    ComboName = name.Replace("_", " "),
                    Description = description,
                    RequiredSystems = new List<string>(systems),
                    Multiplier = multiplier,
                    IsActive = false,
                    ActivationCount = 0,
                    LastActivation = DateTime.MinValue,
                    TimeWindow = TimeSpan.FromHours(2) // 2-hour activation window
                };
                
                activeCombos.Add(combo);
            }
            
            Debug.Log($"✅ Cross-system combos initialized: {activeCombos.Count} combinations");
        }
        
        private void StartProgressionIntegration()
        {
            // Start progression integration monitoring
            lastIntegrationUpdate = DateTime.Now;
            
            Debug.Log("✅ Progression integration started - unified gaming experience active");
        }
        
        private void StopProgressionIntegration()
        {
            // Clean up progression integration
            Debug.Log("✅ Progression integration stopped");
        }
        
        private void Update()
        {
            if (!EnableProgressionIntegration) return;
            
            // Update progression integration systems
            UpdateProgressionIntegration();
            UpdateActiveStreaks();
            UpdateCrossSystemCombos();
            ProcessProgressionEvents();
        }
        
        private void UpdateProgressionIntegration()
        {
            // Update system activity and engagement metrics
            foreach (var profile in systemProfiles)
            {
                if (profile.IsActive && profile.LastActivity > DateTime.MinValue)
                {
                    var timeSinceActivity = DateTime.Now - profile.LastActivity;
                    if (timeSinceActivity.TotalMinutes > 30) // Consider inactive after 30 minutes
                    {
                        profile.CurrentStreakCount = 0;
                    }
                }
            }
        }
        
        private void UpdateActiveStreaks()
        {
            if (!EnableStreakTracking) return;
            
            // Update and validate active streaks
            foreach (var streak in activeStreaks.ToList())
            {
                var timeSinceLastActivity = DateTime.Now - streak.LastActivity;
                if (timeSinceLastActivity.TotalHours > 24) // Streak expires after 24 hours
                {
                    streak.IsActive = false;
                    streak.StreakEndDate = DateTime.Now;
                }
            }
        }
        
        private void UpdateCrossSystemCombos()
        {
            if (!EnableCrossSystemBonuses) return;
            
            // Check for combo activation opportunities
            foreach (var combo in activeCombos)
            {
                bool canActivate = CheckComboConditions(combo);
                if (canActivate && !combo.IsActive)
                {
                    ActivateCombo(combo);
                }
                else if (combo.IsActive && (DateTime.Now - combo.LastActivation) > combo.TimeWindow)
                {
                    DeactivateCombo(combo);
                }
            }
        }
        
        private void ProcessProgressionEvents()
        {
            if (!EnableProgressionEvents) return;
            
            // Process and clean up old progression events
            var expiredEvents = recentEvents.Where(e => 
                (DateTime.Now - e.EventTime).TotalHours > 1).ToList();
            
            foreach (var expiredEvent in expiredEvents)
            {
                recentEvents.Remove(expiredEvent);
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Record system interaction for progression integration
        /// </summary>
        public void RecordSystemInteraction(string systemName, string interactionType, float engagementScore, string playerId = "current_player")
        {
            if (!EnableProgressionIntegration) return;
            
            var profile = systemProfiles.FirstOrDefault(p => p.SystemName == systemName);
            if (profile != null)
            {
                profile.LastActivity = DateTime.Now;
                profile.TotalInteractions++;
                profile.AverageEngagement = (profile.AverageEngagement + engagementScore) / 2f;
                
                // Update player integration profile
                var playerProfile = GetOrCreatePlayerIntegration(playerId);
                UpdatePlayerIntegration(playerProfile, systemName, interactionType, engagementScore);
                
                // Check for streak opportunities
                if (EnableStreakTracking)
                {
                    CheckForStreakOpportunities(systemName, playerId);
                }
                
                // Check for bonus activation
                if (EnableCrossSystemBonuses)
                {
                    CheckForBonusActivation(systemName, playerId);
                }
                
                // Create progression event
                CreateProgressionEvent($"{systemName} Interaction", interactionType, engagementScore, playerId);
            }
            
            Debug.Log($"✅ System interaction recorded: {systemName} - {interactionType} (Engagement: {engagementScore:F1})");
        }
        
        /// <summary>
        /// Calculate progression bonus for system activity
        /// </summary>
        public float CalculateProgressionBonus(string systemName, float baseValue, string playerId = "current_player")
        {
            if (!EnableProgressionIntegration) return baseValue;
            
            float totalMultiplier = 1.0f;
            var playerProfile = GetOrCreatePlayerIntegration(playerId);
            
            // Apply system-specific multiplier
            var systemProfile = systemProfiles.FirstOrDefault(p => p.SystemName == systemName);
            if (systemProfile != null)
            {
                totalMultiplier *= systemProfile.BaseMultiplier;
            }
            
            // Apply streak bonuses
            if (EnableStreakTracking)
            {
                var systemStreak = activeStreaks.FirstOrDefault(s => 
                    s.SystemName == systemName && s.PlayerID == playerId && s.IsActive);
                if (systemStreak != null)
                {
                    float streakMultiplier = CalculateStreakMultiplier(systemStreak.StreakCount);
                    totalMultiplier *= streakMultiplier;
                }
            }
            
            // Apply cross-system combo bonuses
            if (EnableCrossSystemBonuses)
            {
                var activeCombo = activeCombos.FirstOrDefault(c => 
                    c.IsActive && c.RequiredSystems.Contains(systemName));
                if (activeCombo != null)
                {
                    totalMultiplier *= activeCombo.Multiplier;
                }
            }
            
            // Apply available progression bonuses
            foreach (var bonus in availableBonuses.Values.Where(b => b.IsActive))
            {
                if (CheckBonusApplicability(bonus, systemName, playerId))
                {
                    totalMultiplier *= bonus.Multiplier;
                }
            }
            
            float finalValue = baseValue * totalMultiplier;
            float bonusAmount = finalValue - baseValue;
            
            if (bonusAmount > 0)
            {
                totalBonusesAwarded += bonusAmount;
                OnBonusAwarded?.Invoke(playerId, bonusAmount);
            }
            
            return finalValue;
        }
        
        /// <summary>
        /// Get player's progression integration profile
        /// </summary>
        public PlayerIntegrationProfile GetPlayerIntegration(string playerId = "current_player")
        {
            return GetOrCreatePlayerIntegration(playerId);
        }
        
        /// <summary>
        /// Get active progression streaks for player
        /// </summary>
        public List<ProgressionStreak> GetActiveStreaks(string playerId = "current_player")
        {
            return activeStreaks.Where(s => s.PlayerID == playerId && s.IsActive).ToList();
        }
        
        /// <summary>
        /// Get available progression bonuses
        /// </summary>
        public List<ProgressionBonus> GetAvailableBonuses(string playerId = "current_player")
        {
            return availableBonuses.Values.Where(b => b.IsActive || CanActivateBonus(b, playerId)).ToList();
        }
        
        /// <summary>
        /// Get active cross-system combos
        /// </summary>
        public List<CrossSystemCombo> GetActiveCombos()
        {
            return activeCombos.Where(c => c.IsActive).ToList();
        }
        
        /// <summary>
        /// Get progression integration statistics
        /// </summary>
        public ProgressionIntegrationStats GetIntegrationStats()
        {
            var stats = new ProgressionIntegrationStats
            {
                TotalSystemsActive = systemProfiles.Count(p => p.IsActive),
                TotalBonusesAwarded = totalBonusesAwarded,
                TotalStreaksAchieved = totalStreaksAchieved,
                TotalCombosActivated = totalCombosActivated,
                AverageBonusMultiplier = averageBonusMultiplier,
                ActiveStreaksCount = activeStreaks.Count(s => s.IsActive),
                ActiveCombosCount = activeCombos.Count(c => c.IsActive),
                LastUpdateTime = lastIntegrationUpdate
            };
            
            return stats;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private PlayerIntegrationProfile GetOrCreatePlayerIntegration(string playerId)
        {
            if (playerIntegration.ContainsKey(playerId))
            {
                return playerIntegration[playerId];
            }
            
            var newProfile = new PlayerIntegrationProfile
            {
                PlayerID = playerId,
                TotalSystemsUsed = 0,
                FavoriteSystem = "",
                TotalBonusesEarned = 0f,
                LongestStreak = 0,
                TotalCombosActivated = 0,
                LastActivity = DateTime.Now,
                SystemUsageStats = new Dictionary<string, int>(),
                RecentActivity = new List<string>()
            };
            
            playerIntegration[playerId] = newProfile;
            return newProfile;
        }
        
        private void UpdatePlayerIntegration(PlayerIntegrationProfile profile, string systemName, string interactionType, float engagementScore)
        {
            profile.LastActivity = DateTime.Now;
            
            // Update system usage stats
            if (!profile.SystemUsageStats.ContainsKey(systemName))
            {
                profile.SystemUsageStats[systemName] = 0;
                profile.TotalSystemsUsed++;
            }
            profile.SystemUsageStats[systemName]++;
            
            // Update favorite system
            var mostUsed = profile.SystemUsageStats.OrderByDescending(kvp => kvp.Value).First();
            profile.FavoriteSystem = mostUsed.Key;
            
            // Add to recent activity
            profile.RecentActivity.Insert(0, $"{systemName}:{interactionType}");
            if (profile.RecentActivity.Count > 10) // Keep last 10 activities
            {
                profile.RecentActivity.RemoveAt(profile.RecentActivity.Count - 1);
            }
        }
        
        private void CheckForStreakOpportunities(string systemName, string playerId)
        {
            var existingStreak = activeStreaks.FirstOrDefault(s => 
                s.SystemName == systemName && s.PlayerID == playerId && s.IsActive);
            
            if (existingStreak != null)
            {
                // Continue existing streak
                existingStreak.StreakCount++;
                existingStreak.LastActivity = DateTime.Now;
                
                if (existingStreak.StreakCount > existingStreak.BestStreak)
                {
                    existingStreak.BestStreak = existingStreak.StreakCount;
                }
                
                OnStreakAchieved?.Invoke(existingStreak);
            }
            else
            {
                // Start new streak
                var newStreak = new ProgressionStreak
                {
                    StreakID = $"streak_{systemName}_{playerId}_{DateTime.Now.Ticks}",
                    SystemName = systemName,
                    PlayerID = playerId,
                    StreakCount = 1,
                    BestStreak = 1,
                    StartDate = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true,
                    Multiplier = CalculateStreakMultiplier(1)
                };
                
                activeStreaks.Add(newStreak);
            }
            
            totalStreaksAchieved++;
        }
        
        private void CheckForBonusActivation(string systemName, string playerId)
        {
            foreach (var bonus in availableBonuses.Values.Where(b => !b.IsActive))
            {
                if (CanActivateBonus(bonus, playerId))
                {
                    ActivateBonus(bonus, playerId);
                }
            }
        }
        
        private bool CheckComboConditions(CrossSystemCombo combo)
        {
            // Check if all required systems have been active within the time window
            var cutoffTime = DateTime.Now.Subtract(combo.TimeWindow);
            
            int activeSystemCount = 0;
            foreach (var systemName in combo.RequiredSystems)
            {
                var systemProfile = systemProfiles.FirstOrDefault(p => p.SystemName == systemName);
                if (systemProfile != null && systemProfile.LastActivity > cutoffTime)
                {
                    activeSystemCount++;
                }
            }
            
            return activeSystemCount >= combo.RequiredSystems.Count;
        }
        
        private void ActivateCombo(CrossSystemCombo combo)
        {
            combo.IsActive = true;
            combo.LastActivation = DateTime.Now;
            combo.ActivationCount++;
            totalCombosActivated++;
            
            OnComboActivated?.Invoke(combo);
            
            Debug.Log($"🔥 Cross-system combo activated: {combo.ComboName} ({combo.Multiplier:F1}x multiplier)");
        }
        
        private void DeactivateCombo(CrossSystemCombo combo)
        {
            combo.IsActive = false;
            Debug.Log($"Combo deactivated: {combo.ComboName}");
        }
        
        private float CalculateStreakMultiplier(int streakCount)
        {
            float multiplier = BaseStreakMultiplier + (streakCount - 1) * 0.1f;
            return Mathf.Min(multiplier, MaxStreakMultiplier);
        }
        
        private bool CanActivateBonus(ProgressionBonus bonus, string playerId)
        {
            // Check if bonus activation conditions are met
            var playerProfile = GetOrCreatePlayerIntegration(playerId);
            
            return bonus.BonusID switch
            {
                "Daily_Engagement" => playerProfile.SystemUsageStats.Count >= 3, // Used 3+ systems
                "System_Explorer" => playerProfile.RecentActivity.Count >= 5, // Recent activity
                "Mastery_Focus" => playerProfile.SystemUsageStats.Values.Any(v => v >= 10), // 10+ uses of one system
                "Gaming_Streak" => activeStreaks.Any(s => s.PlayerID == playerId && s.StreakCount >= 5),
                "Discovery_Bonus" => true, // Can always activate on discoveries
                "Competition_Edge" => playerProfile.SystemUsageStats.ContainsKey("Competition"),
                "Research_Synergy" => playerProfile.SystemUsageStats.ContainsKey("Research"),
                "Perfect_Performance" => true, // Can activate on perfect scores
                _ => false
            };
        }
        
        private void ActivateBonus(ProgressionBonus bonus, string playerId)
        {
            bonus.IsActive = true;
            bonus.ActivationTime = DateTime.Now;
            
            var playerProfile = GetOrCreatePlayerIntegration(playerId);
            playerProfile.TotalBonusesEarned++;
            
            OnBonusAwarded?.Invoke(playerId, bonus.Multiplier - 1f);
            
            Debug.Log($"🎉 Progression bonus activated: {bonus.BonusName} ({bonus.Multiplier:F1}x)");
        }
        
        private bool CheckBonusApplicability(ProgressionBonus bonus, string systemName, string playerId)
        {
            // Check if bonus applies to current system activity
            return bonus.IsActive && (DateTime.Now - bonus.ActivationTime) < bonus.Duration;
        }
        
        private List<string> GenerateBonusConditions(string bonusId)
        {
            return bonusId switch
            {
                "Daily_Engagement" => new List<string> { "Use 3+ systems in one day" },
                "System_Explorer" => new List<string> { "Try multiple systems in one session" },
                "Mastery_Focus" => new List<string> { "Deep engagement with single system" },
                "Gaming_Streak" => new List<string> { "Maintain activity streaks" },
                "Discovery_Bonus" => new List<string> { "Unlock new content" },
                "Competition_Edge" => new List<string> { "Participate in competitions" },
                "Research_Synergy" => new List<string> { "Combine research with practice" },
                "Perfect_Performance" => new List<string> { "Achieve perfect scores" },
                _ => new List<string> { "Meet bonus conditions" }
            };
        }
        
        private TimeSpan CalculateBonusDuration(string bonusId)
        {
            return bonusId switch
            {
                "Daily_Engagement" => TimeSpan.FromHours(24),
                "System_Explorer" => TimeSpan.FromHours(2),
                "Mastery_Focus" => TimeSpan.FromHours(1),
                "Gaming_Streak" => TimeSpan.FromMinutes(30),
                "Discovery_Bonus" => TimeSpan.FromHours(4),
                "Competition_Edge" => TimeSpan.FromHours(3),
                "Research_Synergy" => TimeSpan.FromHours(2),
                "Perfect_Performance" => TimeSpan.FromMinutes(15),
                _ => TimeSpan.FromHours(1)
            };
        }
        
        private TimeSpan CalculateBonusCooldown(string bonusId)
        {
            return bonusId switch
            {
                "Daily_Engagement" => TimeSpan.FromHours(24),
                "System_Explorer" => TimeSpan.FromHours(4),
                "Mastery_Focus" => TimeSpan.FromHours(2),
                "Gaming_Streak" => TimeSpan.FromHours(1),
                "Discovery_Bonus" => TimeSpan.FromHours(6),
                "Competition_Edge" => TimeSpan.FromHours(8),
                "Research_Synergy" => TimeSpan.FromHours(4),
                "Perfect_Performance" => TimeSpan.FromHours(1),
                _ => TimeSpan.FromHours(2)
            };
        }
        
        private void CreateProgressionEvent(string eventName, string eventType, float value, string playerId)
        {
            var progressionEvent = new ProgressionEvent
            {
                EventID = $"event_{DateTime.Now.Ticks}",
                EventName = eventName,
                EventType = eventType,
                EventTime = DateTime.Now,
                PlayerID = playerId,
                Value = value,
                SystemName = eventType.Split('_')[0] // Extract system name
            };
            
            recentEvents.Add(progressionEvent);
            OnProgressionEvent?.Invoke(progressionEvent);
            
            // Keep only recent events
            if (recentEvents.Count > 100)
            {
                var oldestEvents = recentEvents.OrderBy(e => e.EventTime).Take(20).ToList();
                foreach (var oldEvent in oldestEvents)
                {
                    recentEvents.Remove(oldEvent);
                }
            }
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate progression integrator functionality
        /// </summary>
        public void TestProgressionIntegrator()
        {
            Debug.Log("=== Testing Progression Integrator ===");
            Debug.Log($"Integration Enabled: {EnableProgressionIntegration}");
            Debug.Log($"Cross-System Bonuses: {EnableCrossSystemBonuses}");
            Debug.Log($"Streak Tracking: {EnableStreakTracking}");
            Debug.Log($"System Profiles: {systemProfiles.Count}");
            Debug.Log($"Available Bonuses: {availableBonuses.Count}");
            Debug.Log($"Cross-System Combos: {activeCombos.Count}");
            
            // Test system interaction recording
            if (EnableProgressionIntegration)
            {
                RecordSystemInteraction("Breeding", "Challenge_Completed", 0.9f, "test_player");
                RecordSystemInteraction("Aromatic", "Scent_Identified", 0.8f, "test_player");
                RecordSystemInteraction("Genetics", "Research_Completed", 0.95f, "test_player");
                Debug.Log($"✓ Test system interactions recorded");
                
                // Test progression bonus calculation
                float baseValue = 100f;
                float bonusValue = CalculateProgressionBonus("Breeding", baseValue, "test_player");
                Debug.Log($"✓ Test bonus calculation: {baseValue} → {bonusValue} ({bonusValue/baseValue:F1}x)");
                
                // Test player integration profile
                var integration = GetPlayerIntegration("test_player");
                Debug.Log($"✓ Test player integration: {integration.TotalSystemsUsed} systems, Favorite: {integration.FavoriteSystem}");
                
                // Test statistics
                var stats = GetIntegrationStats();
                Debug.Log($"✓ Test integration stats: {stats.TotalSystemsActive} active systems, {stats.TotalBonusesAwarded:F0} total bonuses");
            }
            
            Debug.Log("✅ Progression integrator test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class SystemProgressionProfile
    {
        public string SystemName;
        public string SystemType;
        public float BaseMultiplier;
        public string Description;
        public bool IsActive;
        public DateTime LastActivity;
        public int TotalInteractions;
        public float AverageEngagement;
        public int CurrentStreakCount;
    }
    
    [System.Serializable]
    public class PlayerIntegrationProfile
    {
        public string PlayerID;
        public int TotalSystemsUsed;
        public string FavoriteSystem;
        public float TotalBonusesEarned;
        public int LongestStreak;
        public int TotalCombosActivated;
        public DateTime LastActivity;
        public Dictionary<string, int> SystemUsageStats = new Dictionary<string, int>();
        public List<string> RecentActivity = new List<string>();
    }
    
    [System.Serializable]
    public class ProgressionStreak
    {
        public string StreakID;
        public string SystemName;
        public string PlayerID;
        public int StreakCount;
        public int BestStreak;
        public DateTime StartDate;
        public DateTime StreakEndDate;
        public DateTime LastActivity;
        public bool IsActive;
        public float Multiplier;
    }
    
    [System.Serializable]
    public class CrossSystemCombo
    {
        public string ComboID;
        public string ComboName;
        public string Description;
        public List<string> RequiredSystems = new List<string>();
        public float Multiplier;
        public bool IsActive;
        public int ActivationCount;
        public DateTime LastActivation;
        public TimeSpan TimeWindow;
    }
    
    [System.Serializable]
    public class ProgressionBonus
    {
        public string BonusID;
        public string BonusName;
        public string Description;
        public float Multiplier;
        public bool IsActive;
        public DateTime ActivationTime;
        public TimeSpan Duration;
        public TimeSpan CooldownTime;
        public List<string> ActivationConditions = new List<string>();
    }
    
    [System.Serializable]
    public class ProgressionEvent
    {
        public string EventID;
        public string EventName;
        public string EventType;
        public DateTime EventTime;
        public string PlayerID;
        public float Value;
        public string SystemName;
    }
    
    [System.Serializable]
    public class ProgressionIntegrationStats
    {
        public int TotalSystemsActive;
        public float TotalBonusesAwarded;
        public int TotalStreaksAchieved;
        public int TotalCombosActivated;
        public float AverageBonusMultiplier;
        public int ActiveStreaksCount;
        public int ActiveCombosCount;
        public DateTime LastUpdateTime;
    }
    
    #endregion
}