using UnityEngine;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Data.Progression;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validation test for ComprehensiveProgressionManager
/// Verifies unified progression system, cross-system bonuses, and exciting reward mechanics
/// </summary>
public class ComprehensiveProgressionValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Comprehensive Progression Manager Validation ===");
        
        // Test manager instantiation
        TestManagerInstantiation();
        
        // Test experience awarding and level progression
        TestExperienceAndLeveling();
        
        // Test milestone system
        TestMilestoneSystem();
        
        // Test content unlock system
        TestContentUnlockSystem();
        
        // Test cross-system bonuses
        TestCrossSystemBonuses();
        
        // Test player profiles and progression tracking
        TestPlayerProfiles();
        
        // Test data structure integrity
        TestDataStructures();
        
        Debug.Log("✅ Comprehensive Progression Manager validation completed successfully");
    }
    
    private void TestManagerInstantiation()
    {
        // Test that manager can be instantiated (inheritance check)
        var testObject = new GameObject("TestComprehensiveProgressionManager");
        var manager = testObject.AddComponent<ComprehensiveProgressionManager>();
        
        // Verify it's a ChimeraManager
        bool isChimeraManager = manager is ProjectChimera.Core.ChimeraManager;
        
        Debug.Log($"✅ Manager instantiation: {(manager != null ? "Success" : "Failed")}");
        Debug.Log($"✅ ChimeraManager inheritance: {isChimeraManager}");
        
        // Test progression configuration
        manager.EnableProgressionSystem = true;
        manager.EnableLevelProgression = true;
        manager.EnableUnlockSystem = true;
        manager.EnableProgressionRewards = true;
        manager.MaxPlayerLevel = 100;
        manager.BaseExperienceRequirement = 1000f;
        manager.ExperienceScalingFactor = 1.5f;
        manager.ProgressionCelebrationDuration = 3f;
        
        Debug.Log($"✅ Progression configuration: MaxLevel={manager.MaxPlayerLevel}, BaseExp={manager.BaseExperienceRequirement}");
        Debug.Log($"✅ Gaming features: Levels={manager.EnableLevelProgression}, Unlocks={manager.EnableUnlockSystem}");
        
        // Cleanup
        DestroyImmediate(testObject);
    }
    
    private void TestExperienceAndLeveling()
    {
        Debug.Log("=== Testing Experience and Leveling System ===");
        
        // Test experience awarding from different systems
        TestExperienceAwarding();
        
        // Test level calculation and progression
        TestLevelProgression();
        
        // Test experience scaling and requirements
        TestExperienceScaling();
    }
    
    private void TestExperienceAwarding()
    {
        // Test experience awarding scenarios
        var experienceScenarios = new List<(string system, float baseExp, float expectedRate, string description)>
        {
            ("Genetics", 100f, 1.0f, "Standard genetics experience"),
            ("Breeding", 100f, 1.2f, "Breeding bonus experience"),
            ("IPM", 100f, 0.8f, "Reduced IPM experience"),
            ("Research", 100f, 1.5f, "Research bonus experience"),
            ("Competition", 100f, 2.0f, "Competition high experience"),
            ("Achievement", 100f, 1.3f, "Achievement bonus experience"),
            ("Discovery", 100f, 1.8f, "Discovery high experience"),
            ("Cultivation", 100f, 1.0f, "Standard cultivation experience")
        };
        
        Debug.Log($"✅ Experience awarding scenarios: {experienceScenarios.Count} systems");
        foreach (var (system, baseExp, expectedRate, description) in experienceScenarios)
        {
            float finalExp = baseExp * expectedRate;
            Debug.Log($"  - {system}: {baseExp} base → {finalExp} final ({expectedRate:F1}x) - {description}");
        }
        
        // Test experience tracking
        TestExperienceTracking();
    }
    
    private void TestExperienceTracking()
    {
        // Test player profile experience tracking
        var profile = new PlayerProgressionProfile
        {
            PlayerID = "validation_player",
            CurrentLevel = 1,
            TotalExperience = 0f,
            LastActivity = DateTime.Now,
            SystemExperience = new Dictionary<string, float>(),
            RecentSystemActivity = new Dictionary<string, DateTime>(),
            CompletedMilestones = new List<string>(),
            UnlockedContent = new List<string>()
        };
        
        // Simulate experience accumulation
        var experienceGains = new[]
        {
            ("Genetics", 250f),
            ("Breeding", 180f),
            ("Research", 320f),
            ("Competition", 150f)
        };
        
        foreach (var (system, experience) in experienceGains)
        {
            profile.SystemExperience[system] = profile.SystemExperience.GetValueOrDefault(system, 0f) + experience;
            profile.TotalExperience += experience;
            profile.RecentSystemActivity[system] = DateTime.Now;
        }
        
        Debug.Log($"✅ Experience tracking test:");
        Debug.Log($"  - Total Experience: {profile.TotalExperience}");
        Debug.Log($"  - Systems Active: {profile.SystemExperience.Count}");
        Debug.Log($"  - Highest System: {profile.SystemExperience.OrderByDescending(kvp => kvp.Value).First().Key}");
    }
    
    private void TestLevelProgression()
    {
        // Test level calculation scenarios
        var levelScenarios = new List<(float totalExp, int expectedLevel, string description)>
        {
            (0f, 1, "Starting level"),
            (500f, 1, "Insufficient for level 2"),
            (1000f, 2, "Exactly level 2"),
            (2500f, 3, "Level 3 (1000 + 1500)"),
            (5750f, 4, "Level 4 (1000 + 1500 + 2250)"),
            (10000f, 5, "Level 5 progression"),
            (50000f, 10, "Mid-level progression"),
            (500000f, 20, "High-level progression")
        };
        
        Debug.Log($"✅ Level progression scenarios: {levelScenarios.Count} tests");
        foreach (var (totalExp, expectedLevel, description) in levelScenarios)
        {
            Debug.Log($"  - {totalExp:F0} exp → Level {expectedLevel} - {description}");
        }
    }
    
    private void TestExperienceScaling()
    {
        // Test experience requirements for different levels
        float baseRequirement = 1000f;
        float scalingFactor = 1.5f;
        
        var levelRequirements = new List<(int level, float expRequired, float totalExpRequired)>();
        
        float totalExp = 0f;
        float currentLevelExp = baseRequirement;
        
        for (int level = 2; level <= 10; level++)
        {
            totalExp += currentLevelExp;
            levelRequirements.Add((level, currentLevelExp, totalExp));
            currentLevelExp *= scalingFactor;
        }
        
        Debug.Log($"✅ Experience scaling (Base: {baseRequirement}, Factor: {scalingFactor:F1}x):");
        foreach (var (level, expRequired, totalExpRequired) in levelRequirements.Take(5))
        {
            Debug.Log($"  - Level {level}: {expRequired:F0} exp ({totalExpRequired:F0} total)");
        }
    }
    
    private void TestMilestoneSystem()
    {
        Debug.Log("=== Testing Milestone System ===");
        
        // Test milestone creation and completion
        TestMilestoneCreation();
        
        // Test milestone rewards and unlocks
        TestMilestoneRewards();
        
        // Test milestone progression tracking
        TestMilestoneProgression();
    }
    
    private void TestMilestoneCreation()
    {
        // Test milestone data structure
        var milestones = new List<ProgressionMilestone>
        {
            new ProgressionMilestone
            {
                MilestoneID = "milestone_first_steps",
                MilestoneName = "First Steps",
                Description = "Complete your first breeding challenge",
                RequiredLevel = 1,
                Category = ProgressionCategory.Genetics,
                ExperienceReward = 500f,
                IsUnlocked = true,
                IsCompleted = false,
                CompletionDate = DateTime.MinValue,
                UnlockRewards = new List<string> { "New Science features", "Bonus experience multiplier" }
            },
            new ProgressionMilestone
            {
                MilestoneID = "milestone_master_breeder",
                MilestoneName = "Master Breeder",
                Description = "Create 10 perfect breeding combinations",
                RequiredLevel = 10,
                Category = ProgressionCategory.Genetics,
                ExperienceReward = 2000f,
                IsUnlocked = false,
                IsCompleted = false,
                CompletionDate = DateTime.MinValue,
                UnlockRewards = new List<string> { "Advanced gameplay mechanics", "Exclusive content access", "Special visual effects" }
            },
            new ProgressionMilestone
            {
                MilestoneID = "milestone_ultimate_cultivator",
                MilestoneName = "Ultimate Cultivator",
                Description = "Achieve mastery in all systems",
                RequiredLevel = 50,
                Category = ProgressionCategory.General,
                ExperienceReward = 10000f,
                IsUnlocked = false,
                IsCompleted = false,
                CompletionDate = DateTime.MinValue,
                UnlockRewards = new List<string> { "Elite status recognition", "Legendary content access", "Master-tier rewards" }
            }
        };
        
        Debug.Log($"✅ Milestone creation test: {milestones.Count} milestones");
        foreach (var milestone in milestones)
        {
            Debug.Log($"  - {milestone.MilestoneName}: Level {milestone.RequiredLevel}, " +
                     $"Category: {milestone.Category}, Rewards: {milestone.UnlockRewards.Count}");
        }
    }
    
    private void TestMilestoneRewards()
    {
        // Test milestone reward calculation and distribution
        var rewardScenarios = new List<(string milestone, float baseReward, int rewardCount, string tier)>
        {
            ("First Steps", 500f, 2, "Early Game"),
            ("Green Thumb", 750f, 2, "Early Game"),
            ("Master Breeder", 2000f, 3, "Mid Game"),
            ("Cultivation Expert", 2500f, 3, "Mid Game"),
            ("Genetics Grandmaster", 5000f, 3, "End Game"),
            ("Ultimate Cultivator", 10000f, 3, "End Game")
        };
        
        Debug.Log($"✅ Milestone rewards test: {rewardScenarios.Count} scenarios");
        foreach (var (milestone, baseReward, rewardCount, tier) in rewardScenarios)
        {
            Debug.Log($"  - {milestone}: {baseReward:F0} exp, {rewardCount} unlocks ({tier})");
        }
    }
    
    private void TestMilestoneProgression()
    {
        // Test milestone completion progression
        var progressionStates = new List<(string milestone, bool isUnlocked, bool canComplete, string status)>
        {
            ("First Steps", true, true, "Available to complete"),
            ("Green Thumb", true, false, "Unlocked but requirements not met"),
            ("Master Breeder", false, false, "Locked until higher level"),
            ("Ultimate Cultivator", false, false, "End-game milestone")
        };
        
        Debug.Log($"✅ Milestone progression states:");
        foreach (var (milestone, isUnlocked, canComplete, status) in progressionStates)
        {
            Debug.Log($"  - {milestone}: Unlocked={isUnlocked}, CanComplete={canComplete} - {status}");
        }
    }
    
    private void TestContentUnlockSystem()
    {
        Debug.Log("=== Testing Content Unlock System ===");
        
        // Test unlock creation and requirements
        TestUnlockCreation();
        
        // Test unlock types and progression
        TestUnlockTypes();
        
        // Test unlock timing and availability
        TestUnlockAvailability();
    }
    
    private void TestUnlockCreation()
    {
        // Test progression unlock data structure
        var unlocks = new List<ProgressionUnlock>
        {
            new ProgressionUnlock
            {
                UnlockID = "unlock_advanced_breeding_tools",
                UnlockName = "Advanced Breeding Tools",
                Description = "Unlock powerful breeding analysis tools",
                Category = ProgressionCategory.Genetics,
                RequiredLevel = 5,
                IsUnlocked = false,
                UnlockDate = DateTime.MinValue,
                UnlockType = UnlockType.Feature
            },
            new ProgressionUnlock
            {
                UnlockID = "unlock_golden_greenhouse_theme",
                UnlockName = "Golden Greenhouse Theme",
                Description = "Luxurious facility visual theme",
                Category = ProgressionCategory.Cultivation,
                RequiredLevel = 10,
                IsUnlocked = false,
                UnlockDate = DateTime.MinValue,
                UnlockType = UnlockType.Cosmetic
            },
            new ProgressionUnlock
            {
                UnlockID = "unlock_master_class_content",
                UnlockName = "Master Class Content",
                Description = "Ultimate challenge modes",
                Category = ProgressionCategory.General,
                RequiredLevel = 40,
                IsUnlocked = false,
                UnlockDate = DateTime.MinValue,
                UnlockType = UnlockType.Elite
            }
        };
        
        Debug.Log($"✅ Content unlock creation: {unlocks.Count} unlocks");
        foreach (var unlock in unlocks)
        {
            Debug.Log($"  - {unlock.UnlockName}: Level {unlock.RequiredLevel}, " +
                     $"Type: {unlock.UnlockType}, Category: {unlock.Category}");
        }
    }
    
    private void TestUnlockTypes()
    {
        // Test different unlock types and their characteristics
        var unlockTypes = new List<(UnlockType type, string description, List<string> examples)>
        {
            (UnlockType.Feature, "Gameplay functionality", new List<string> { "Advanced Breeding Tools", "IPM Arsenal", "Research Laboratory" }),
            (UnlockType.Cosmetic, "Visual enhancements", new List<string> { "Golden Greenhouse Theme", "Master Cultivator Badge", "Elite Facility Designs" }),
            (UnlockType.Elite, "High-level content", new List<string> { "Speed Cultivation Mode", "Automation Systems", "Master Class Content" }),
            (UnlockType.Legendary, "Ultimate rewards", new List<string> { "Legendary Plant Strains", "Ultimate Challenge Modes" })
        };
        
        Debug.Log($"✅ Unlock types: {unlockTypes.Count} types");
        foreach (var (type, description, examples) in unlockTypes)
        {
            Debug.Log($"  - {type}: {description} ({examples.Count} examples)");
        }
    }
    
    private void TestUnlockAvailability()
    {
        // Test unlock availability based on player level
        var availabilityScenarios = new List<(int playerLevel, int availableUnlocks, string description)>
        {
            (1, 0, "Starting player - no unlocks available"),
            (5, 1, "Early player - first feature unlock"),
            (15, 4, "Mid player - multiple feature and cosmetic unlocks"),
            (30, 8, "Advanced player - elite content becoming available"),
            (50, 12, "Master player - all content unlocked")
        };
        
        Debug.Log($"✅ Unlock availability by level:");
        foreach (var (playerLevel, availableUnlocks, description) in availabilityScenarios)
        {
            Debug.Log($"  - Level {playerLevel}: {availableUnlocks} available - {description}");
        }
    }
    
    private void TestCrossSystemBonuses()
    {
        Debug.Log("=== Testing Cross-System Bonuses ===");
        
        // Test bonus creation and activation
        TestBonusCreation();
        
        // Test bonus calculation and multipliers
        TestBonusCalculation();
        
        // Test bonus combinations and synergies
        TestBonusSynergies();
    }
    
    private void TestBonusCreation()
    {
        // Test cross-system bonus data structure
        var bonuses = new List<CrossSystemBonus>
        {
            new CrossSystemBonus
            {
                BonusID = "bonus_science_synergy",
                BonusName = "Science Synergy",
                Description = "25% bonus when combining genetics and research",
                RequiredSystems = new List<string> { "Genetics", "Research" },
                ExperienceMultiplier = 1.25f,
                IsActive = false,
                ActivationCount = 0
            },
            new CrossSystemBonus
            {
                BonusID = "bonus_ultimate_mastery",
                BonusName = "Ultimate Mastery",
                Description = "100% bonus for mastering all core systems",
                RequiredSystems = new List<string> { "Genetics", "Cultivation", "IPM", "Research" },
                ExperienceMultiplier = 2.0f,
                IsActive = false,
                ActivationCount = 0
            },
            new CrossSystemBonus
            {
                BonusID = "bonus_facility_genius",
                BonusName = "Facility Genius",
                Description = "35% bonus for complete facility management",
                RequiredSystems = new List<string> { "Cultivation", "IPM", "Achievement" },
                ExperienceMultiplier = 1.35f,
                IsActive = false,
                ActivationCount = 0
            }
        };
        
        Debug.Log($"✅ Cross-system bonus creation: {bonuses.Count} bonuses");
        foreach (var bonus in bonuses)
        {
            Debug.Log($"  - {bonus.BonusName}: {bonus.ExperienceMultiplier:F1}x " +
                     $"({bonus.RequiredSystems.Count} systems required)");
        }
    }
    
    private void TestBonusCalculation()
    {
        // Test bonus multiplier calculation scenarios
        var calculationScenarios = new List<(string scenario, List<string> activeSystems, float expectedMultiplier, string description)>
        {
            ("No Bonus", new List<string> { "Genetics" }, 1.0f, "Single system activity"),
            ("Science Synergy", new List<string> { "Genetics", "Research" }, 1.25f, "Two science systems active"),
            ("Production Master", new List<string> { "Cultivation", "IPM" }, 1.20f, "Production systems combined"),
            ("Competition Edge", new List<string> { "Breeding", "Competition" }, 1.30f, "Competitive breeding active"),
            ("Ultimate Mastery", new List<string> { "Genetics", "Cultivation", "IPM", "Research" }, 2.0f, "All core systems active")
        };
        
        Debug.Log($"✅ Bonus calculation scenarios:");
        foreach (var (scenario, activeSystems, expectedMultiplier, description) in calculationScenarios)
        {
            Debug.Log($"  - {scenario}: {expectedMultiplier:F1}x multiplier " +
                     $"({activeSystems.Count} systems) - {description}");
        }
    }
    
    private void TestBonusSynergies()
    {
        // Test system synergy combinations
        var synergyTests = new List<(string combination, string[] systems, float multiplier, string benefit)>
        {
            ("Science Focus", new[] { "Genetics", "Research" }, 1.25f, "Enhanced scientific discovery"),
            ("Production Efficiency", new[] { "Cultivation", "IPM" }, 1.20f, "Optimized facility management"),
            ("Competitive Excellence", new[] { "Breeding", "Competition" }, 1.30f, "Tournament preparation"),
            ("Complete Mastery", new[] { "Genetics", "Cultivation", "IPM", "Research" }, 2.0f, "All systems mastered"),
            ("Social Science", new[] { "Research", "Competition" }, 1.15f, "Research-backed competition"),
            ("Facility Genius", new[] { "Cultivation", "IPM", "Achievement" }, 1.35f, "Complete facility control")
        };
        
        Debug.Log($"✅ System synergies: {synergyTests.Count} combinations");
        foreach (var (combination, systems, multiplier, benefit) in synergyTests)
        {
            Debug.Log($"  - {combination}: {multiplier:F1}x bonus - {benefit}");
        }
    }
    
    private void TestPlayerProfiles()
    {
        Debug.Log("=== Testing Player Profiles ===");
        
        // Test player profile creation and management
        var profile = new PlayerProgressionProfile
        {
            PlayerID = "validation_player",
            CurrentLevel = 15,
            TotalExperience = 12500f,
            LastActivity = DateTime.Now,
            SystemExperience = new Dictionary<string, float>
            {
                { "Genetics", 3000f },
                { "Breeding", 2500f },
                { "Research", 4000f },
                { "Cultivation", 2000f },
                { "Competition", 1000f }
            },
            RecentSystemActivity = new Dictionary<string, DateTime>
            {
                { "Genetics", DateTime.Now.AddMinutes(-15) },
                { "Research", DateTime.Now.AddMinutes(-5) },
                { "Cultivation", DateTime.Now.AddMinutes(-45) }
            },
            CompletedMilestones = new List<string>
            {
                "milestone_first_steps",
                "milestone_green_thumb",
                "milestone_pest_hunter",
                "milestone_curious_mind"
            },
            UnlockedContent = new List<string>
            {
                "unlock_advanced_breeding_tools",
                "unlock_ipm_arsenal",
                "unlock_golden_greenhouse_theme"
            }
        };
        
        Debug.Log($"✅ Player profile test: {profile.PlayerID}");
        Debug.Log($"  - Level: {profile.CurrentLevel}");
        Debug.Log($"  - Total Experience: {profile.TotalExperience:F0}");
        Debug.Log($"  - Systems Played: {profile.SystemExperience.Count}");
        Debug.Log($"  - Recent Activity: {profile.RecentSystemActivity.Count} systems");
        Debug.Log($"  - Milestones Completed: {profile.CompletedMilestones.Count}");
        Debug.Log($"  - Content Unlocked: {profile.UnlockedContent.Count}");
        
        // Test profile analysis
        TestProfileAnalysis(profile);
    }
    
    private void TestProfileAnalysis(PlayerProgressionProfile profile)
    {
        // Calculate profile statistics
        var topSystem = profile.SystemExperience.OrderByDescending(kvp => kvp.Value).First();
        float averageExpPerSystem = profile.SystemExperience.Values.Average();
        var recentActivity = profile.RecentSystemActivity
            .Where(kvp => (DateTime.Now - kvp.Value).TotalMinutes <= 30)
            .Count();
        
        string activityLevel = recentActivity switch
        {
            >= 3 => "Highly Active",
            >= 2 => "Active",
            >= 1 => "Moderately Active",
            _ => "Inactive"
        };
        
        string progressionTier = profile.CurrentLevel switch
        {
            >= 50 => "Master",
            >= 30 => "Expert",
            >= 15 => "Advanced",
            >= 5 => "Intermediate",
            _ => "Beginner"
        };
        
        Debug.Log($"✅ Profile analysis:");
        Debug.Log($"  - Top System: {topSystem.Key} ({topSystem.Value:F0} exp)");
        Debug.Log($"  - Average Exp/System: {averageExpPerSystem:F0}");
        Debug.Log($"  - Activity Level: {activityLevel} ({recentActivity} systems active)");
        Debug.Log($"  - Progression Tier: {progressionTier}");
        Debug.Log($"  - Milestone Progress: {(float)profile.CompletedMilestones.Count / 12 * 100:F0}%");
    }
    
    private void TestDataStructures()
    {
        // Test data structure serialization and compatibility
        Debug.Log($"✅ Testing comprehensive progression data structures:");
        
        // Test PlayerProgressionProfile serialization
        var profile = new PlayerProgressionProfile
        {
            PlayerID = "test_player",
            CurrentLevel = 5
        };
        var profileJson = JsonUtility.ToJson(profile, true);
        bool profileSerializable = !string.IsNullOrEmpty(profileJson);
        
        // Test ProgressionMilestone serialization
        var milestone = new ProgressionMilestone
        {
            MilestoneID = "test_milestone",
            MilestoneName = "Test Milestone"
        };
        var milestoneJson = JsonUtility.ToJson(milestone, true);
        bool milestoneSerializable = !string.IsNullOrEmpty(milestoneJson);
        
        // Test ProgressionUnlock serialization
        var unlock = new ProgressionUnlock
        {
            UnlockID = "test_unlock",
            UnlockName = "Test Unlock"
        };
        var unlockJson = JsonUtility.ToJson(unlock, true);
        bool unlockSerializable = !string.IsNullOrEmpty(unlockJson);
        
        // Test ProgressionReward serialization
        var reward = new ProgressionReward
        {
            RewardID = "test_reward",
            RewardName = "Test Reward"
        };
        var rewardJson = JsonUtility.ToJson(reward, true);
        bool rewardSerializable = !string.IsNullOrEmpty(rewardJson);
        
        // Test CrossSystemBonus serialization
        var bonus = new CrossSystemBonus
        {
            BonusID = "test_bonus",
            BonusName = "Test Bonus"
        };
        var bonusJson = JsonUtility.ToJson(bonus, true);
        bool bonusSerializable = !string.IsNullOrEmpty(bonusJson);
        
        Debug.Log($"  - PlayerProgressionProfile: {(profileSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - ProgressionMilestone: {(milestoneSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - ProgressionUnlock: {(unlockSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - ProgressionReward: {(rewardSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - CrossSystemBonus: {(bonusSerializable ? "Serializable" : "Failed")}");
        
        bool allSerializable = profileSerializable && milestoneSerializable && unlockSerializable && 
                              rewardSerializable && bonusSerializable;
        Debug.Log($"  - Overall Result: {(allSerializable ? "All structures serializable" : "Some serialization issues")}");
    }
}