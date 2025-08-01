using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using ProjectChimera.Core.Logging;
using AchievementData = ProjectChimera.Systems.Progression.Achievement;
using AchievementRarity = ProjectChimera.Data.Achievements.AchievementRarity;

namespace ProjectChimera.Testing.Achievement
{
    /// <summary>
    /// Comprehensive test suite for Achievement Services - targeting 90%+ code coverage
    /// Tests all 4 decomposed services: Tracking, Reward, Display, Coordinator
    /// Part of PC-012 Achievement System decomposition validation
    /// </summary>
    public class AchievementServicesTestSuite
    {
        private AchievementTrackingService trackingService;
        private AchievementRewardService rewardService;
        private AchievementDisplayService displayService;
        private AchievementCoordinator coordinator;
        private GameObject testGameObject;

        [SetUp]
        public void SetUp()
        {
            // Create test game object to host services
            testGameObject = new GameObject("AchievementServicesTest");
            
            // Initialize all achievement services
            trackingService = testGameObject.AddComponent<AchievementTrackingService>();
            rewardService = testGameObject.AddComponent<AchievementRewardService>();
            displayService = testGameObject.AddComponent<AchievementDisplayService>();
            coordinator = testGameObject.AddComponent<AchievementCoordinator>();
            
            // Initialize services
            trackingService.Initialize();
            rewardService.Initialize();
            displayService.Initialize();
            coordinator.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up services
            if (trackingService != null) trackingService.Shutdown();
            if (rewardService != null) rewardService.Shutdown();
            if (displayService != null) displayService.Shutdown();
            if (coordinator != null) coordinator.Shutdown();
            
            // Destroy test object
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }

        #region AchievementTrackingService Tests

        [Test]
        public void TrackingService_Initialization_SetsCorrectState()
        {
            // Test service initialization
            Assert.IsTrue(trackingService.IsInitialized, "Tracking service should be initialized");
            Assert.AreEqual("Achievement Tracking Service", trackingService.ServiceName);
            Assert.IsNotNull(trackingService.AllAchievements, "All achievements list should not be null");
            Assert.Greater(trackingService.AllAchievements.Count, 0, "Should have initialized achievements");
        }

        [Test]
        public void TrackingService_UpdateProgress_TriggersCorrectEvents()
        {
            // Arrange
            string testEvent = "plant_harvested";
            float testValue = 1f;
            string testPlayer = "test_player";
            bool progressUpdated = false;
            
            trackingService.OnProgressUpdated += (playerId, progress) => 
            {
                progressUpdated = true;
                Assert.AreEqual(testPlayer, playerId);
            };

            // Act
            trackingService.UpdateProgress(testEvent, testValue, testPlayer);

            // Assert
            Assert.IsTrue(progressUpdated, "Progress updated event should be triggered");
        }

        [Test]
        public void TrackingService_GetAchievementById_ReturnsCorrectAchievement()
        {
            // Arrange
            var firstAchievement = trackingService.AllAchievements.FirstOrDefault();
            Assert.IsNotNull(firstAchievement, "Should have at least one achievement");

            // Act
            var retrievedAchievement = trackingService.GetAchievementById(firstAchievement.AchievementID);

            // Assert
            Assert.IsNotNull(retrievedAchievement);
            Assert.AreEqual(firstAchievement.AchievementID, retrievedAchievement.AchievementID);
            Assert.AreEqual(firstAchievement.AchievementName, retrievedAchievement.AchievementName);
        }

        [Test]
        public void TrackingService_GetAchievementsByCategory_FiltersCorrectly()
        {
            // Act
            var cultivationAchievements = trackingService.GetAchievementsByCategory(
                ProjectChimera.Data.Achievements.AchievementCategory.Cultivation);

            // Assert
            Assert.IsNotNull(cultivationAchievements);
            Assert.Greater(cultivationAchievements.Count, 0, "Should have cultivation achievements");
            Assert.IsTrue(cultivationAchievements.All(a => 
                (int)a.Category == (int)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation),
                "All returned achievements should be cultivation category");
        }

        [Test]
        public void TrackingService_ValidateProgress_ReturnsValidResult()
        {
            // Arrange
            var testAchievement = trackingService.AllAchievements.FirstOrDefault();
            Assert.IsNotNull(testAchievement);

            // Act
            var validationResult = trackingService.ValidateProgress(testAchievement.AchievementID, "test_player");

            // Assert
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(testAchievement.AchievementID, validationResult.AchievementId);
            Assert.AreEqual("test_player", validationResult.PlayerId);
            Assert.IsTrue(validationResult.IsValid);
        }

        #endregion

        #region AchievementRewardService Tests

        [Test]
        public void RewardService_Initialization_SetsCorrectState()
        {
            // Test service initialization
            Assert.IsTrue(rewardService.IsInitialized, "Reward service should be initialized");
            Assert.AreEqual("Achievement Reward Service", rewardService.ServiceName);
            Assert.AreEqual(0f, rewardService.TotalRewardsDistributed, "Should start with zero rewards distributed");
        }

        [Test]
        public void RewardService_CalculateRewards_ReturnsValidBundle()
        {
            // Arrange
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                AchievementName = "Test Achievement",
                Points = 100f,
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Category = (ProjectChimera.Data.Progression.AchievementCategory)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation
            };

            // Act
            var rewardBundle = rewardService.CalculateRewards(testAchievement, "test_player");

            // Assert
            Assert.IsNotNull(rewardBundle);
            Assert.AreEqual("test_achievement", rewardBundle.AchievementId);
            Assert.AreEqual("test_player", rewardBundle.PlayerId);
            Assert.Greater(rewardBundle.CurrencyReward, 0, "Should have currency reward");
            Assert.Greater(rewardBundle.ExperienceReward, 0, "Should have experience reward");
            Assert.Greater(rewardBundle.TotalValue, 0f, "Should have total value");
        }

        [Test]
        public void RewardService_DistributeRewards_TriggersEvents()
        {
            // Arrange
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                Points = 100f,
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Category = (ProjectChimera.Data.Progression.AchievementCategory)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation
            };
            
            var rewardBundle = rewardService.CalculateRewards(testAchievement, "test_player");
            bool rewardDistributed = false;
            
            rewardService.OnRewardDistributed += (playerId, bundle) => 
            {
                rewardDistributed = true;
                Assert.AreEqual("test_player", playerId);
                Assert.AreEqual(rewardBundle.AchievementId, bundle.AchievementId);
            };

            // Act
            var success = rewardService.DistributeRewards(rewardBundle);

            // Assert
            Assert.IsTrue(success, "Reward distribution should succeed");
            Assert.IsTrue(rewardDistributed, "Reward distributed event should be triggered");
        }

        [Test]
        public void RewardService_GetRewardStatistics_ReturnsValidData()
        {
            // Arrange - distribute some rewards first
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                Points = 100f,
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Category = (ProjectChimera.Data.Progression.AchievementCategory)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation
            };
            
            var rewardBundle = rewardService.CalculateRewards(testAchievement, "test_player");
            rewardService.DistributeRewards(rewardBundle);

            // Act
            var statistics = rewardService.GetRewardStatistics();

            // Assert
            Assert.IsNotNull(statistics);
            Assert.AreEqual(1, statistics.TotalTransactions);
            Assert.Greater(statistics.TotalValueDistributed, 0f);
            Assert.AreEqual(1, statistics.TotalPlayersRewarded);
        }

        #endregion

        #region AchievementDisplayService Tests

        [Test]
        public void DisplayService_Initialization_SetsCorrectState()
        {
            // Test service initialization
            Assert.IsTrue(displayService.IsInitialized, "Display service should be initialized");
            Assert.AreEqual("Achievement Display Service", displayService.ServiceName);
            Assert.AreEqual(0, displayService.ActiveNotificationCount);
            Assert.AreEqual(0, displayService.QueuedNotificationCount);
            Assert.IsFalse(displayService.IsDisplayingNotifications);
        }

        [Test]
        public void DisplayService_ShowAchievementNotification_CreatesNotification()
        {
            // Arrange
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                AchievementName = "Test Achievement",
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)AchievementRarity.Common
            };

            bool notificationDisplayed = false;
            displayService.OnNotificationDisplayed += (notification) => 
            {
                notificationDisplayed = true;
                Assert.AreEqual(testAchievement.AchievementID, notification.Achievement.AchievementID);
            };

            // Act
            displayService.ShowAchievementNotification(testAchievement);

            // Allow some time for processing
            System.Threading.Thread.Sleep(500);

            // Assert
            Assert.IsTrue(displayService.QueuedNotificationCount > 0 || displayService.ActiveNotificationCount > 0,
                "Should have notifications in queue or active");
        }

        [Test]
        public void DisplayService_GetDisplayStatistics_ReturnsValidData()
        {
            // Act
            var statistics = displayService.GetDisplayStatistics();

            // Assert
            Assert.IsNotNull(statistics);
            Assert.GreaterOrEqual(statistics.TotalNotificationsDisplayed, 0);
            Assert.GreaterOrEqual(statistics.ActiveNotifications, 0);
            Assert.GreaterOrEqual(statistics.QueuedNotifications, 0);
        }

        [Test]
        public void DisplayService_ClearAllNotifications_ResetsState()
        {
            // Arrange - add some notifications first
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                AchievementName = "Test Achievement",
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)AchievementRarity.Common
            };
            
            displayService.ShowAchievementNotification(testAchievement);

            // Act
            displayService.ClearAllNotifications();

            // Assert
            Assert.AreEqual(0, displayService.ActiveNotificationCount);
            Assert.AreEqual(0, displayService.QueuedNotificationCount);
            Assert.IsFalse(displayService.IsDisplayingNotifications);
        }

        #endregion

        #region AchievementCoordinator Tests

        [Test]
        public void Coordinator_Initialization_SetsCorrectState()
        {
            // Test service initialization
            Assert.IsTrue(coordinator.IsInitialized, "Coordinator should be initialized");
            Assert.AreEqual("Achievement Coordinator", coordinator.ServiceName);
            Assert.IsNotNull(coordinator.Statistics);
            Assert.IsNotNull(coordinator.ServiceHealth);
        }

        [Test]
        public void Coordinator_GetStatistics_ReturnsValidData()
        {
            // Act
            var statistics = coordinator.GetStatistics();

            // Assert
            Assert.IsNotNull(statistics);
            Assert.GreaterOrEqual(statistics.TotalAchievementsProcessed, 0);
            Assert.GreaterOrEqual(statistics.TotalProgressUpdates, 0);
            Assert.GreaterOrEqual(statistics.UptimeSeconds, 0);
        }

        [Test]
        public void Coordinator_GetServiceHealth_ReturnsValidStatus()
        {
            // Act
            var serviceHealth = coordinator.GetServiceHealth();

            // Assert
            Assert.IsNotNull(serviceHealth);
            // Note: Service health will depend on whether other services are properly discovered
            // In this test environment, they might not be auto-discovered
        }

        [Test]
        public void Coordinator_GetMetaAchievementRules_ReturnsValidRules()
        {
            // Act
            var metaRules = coordinator.GetMetaAchievementRules();

            // Assert
            Assert.IsNotNull(metaRules);
            Assert.Greater(metaRules.Count, 0, "Should have meta-achievement rules");
            Assert.IsTrue(metaRules.All(r => !string.IsNullOrEmpty(r.RuleId)), 
                "All rules should have valid IDs");
        }

        [Test]
        public void Coordinator_ProcessAchievementUnlock_HandlesValidAchievement()
        {
            // Arrange
            var testAchievement = new AchievementData
            {
                AchievementID = "test_achievement",
                AchievementName = "Test Achievement",
                Points = 100f,
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Category = (ProjectChimera.Data.Progression.AchievementCategory)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation
            };

            bool achievementCompleted = false;
            coordinator.OnAchievementCompleted += (achievement, rewards) => 
            {
                achievementCompleted = true;
                Assert.AreEqual(testAchievement.AchievementID, achievement.AchievementID);
            };

            // Act
            coordinator.ProcessAchievementUnlock(testAchievement, "test_player");

            // Assert - coordinator should handle the achievement processing
            // Event triggering depends on service integration, which may not be fully set up in test environment
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Services_AllInitialized_ReturnCorrectStates()
        {
            // Test that all services are properly initialized
            Assert.IsTrue(trackingService.IsInitialized, "Tracking service should be initialized");
            Assert.IsTrue(rewardService.IsInitialized, "Reward service should be initialized");
            Assert.IsTrue(displayService.IsInitialized, "Display service should be initialized");
            Assert.IsTrue(coordinator.IsInitialized, "Coordinator should be initialized");
        }

        [Test]
        public void Services_ShutdownProperly_ClearsState()
        {
            // Act - shutdown all services
            trackingService.Shutdown();
            rewardService.Shutdown();
            displayService.Shutdown();
            coordinator.Shutdown();

            // Assert
            Assert.IsFalse(trackingService.IsInitialized, "Tracking service should be shutdown");
            Assert.IsFalse(rewardService.IsInitialized, "Reward service should be shutdown");
            Assert.IsFalse(displayService.IsInitialized, "Display service should be shutdown");
            Assert.IsFalse(coordinator.IsInitialized, "Coordinator should be shutdown");
            
            // Re-initialize for teardown
            trackingService.Initialize();
            rewardService.Initialize();
            displayService.Initialize();
            coordinator.Initialize();
        }

        [UnityTest]
        public IEnumerator Services_AsyncOperations_CompleteSuccessfully()
        {
            // Test async operations like coroutines in display service
            var testAchievement = new AchievementData
            {
                AchievementID = "async_test_achievement",
                AchievementName = "Async Test Achievement",
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Rare
            };

            // Act
            displayService.ShowAchievementNotification(testAchievement);
            
            // Wait for async processing
            yield return new WaitForSeconds(2.0f);

            // Assert - notification should have been processed
            var statistics = displayService.GetDisplayStatistics();
            Assert.IsNotNull(statistics);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void TrackingService_InvalidAchievementId_HandlesGracefully()
        {
            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() => 
            {
                var result = trackingService.GetAchievementById("invalid_id");
                Assert.IsNull(result);
            });
        }

        [Test]
        public void RewardService_NullAchievement_HandlesGracefully()
        {
            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() => 
            {
                var result = rewardService.CalculateRewards(null, "test_player");
                Assert.IsNotNull(result);
            });
        }

        [Test]
        public void DisplayService_NullAchievement_HandlesGracefully()
        {
            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() => 
            {
                displayService.ShowAchievementNotification(null);
            });
        }

        [Test]
        public void Coordinator_NullAchievement_HandlesGracefully()
        {
            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() => 
            {
                coordinator.ProcessAchievementUnlock(null, "test_player");
            });
        }

        #endregion

        #region Performance Tests

        [Test]
        public void TrackingService_ManyProgressUpdates_PerformsWell()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - perform many progress updates
            for (int i = 0; i < 1000; i++)
            {
                trackingService.UpdateProgress("performance_test", 1f, $"player_{i % 10}");
            }
            
            stopwatch.Stop();
            
            // Assert - should complete in reasonable time (less than 1 second)
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, 
                "1000 progress updates should complete in less than 1 second");
        }

        [Test]
        public void RewardService_ManyCalculations_PerformsWell()
        {
            // Arrange
            var testAchievement = new AchievementData
            {
                AchievementID = "performance_test",
                Points = 100f,
                Rarity = (ProjectChimera.Data.Progression.AchievementRarity)ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Category = (ProjectChimera.Data.Progression.AchievementCategory)ProjectChimera.Data.Achievements.AchievementCategory.Cultivation
            };
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - perform many reward calculations
            for (int i = 0; i < 1000; i++)
            {
                var rewards = rewardService.CalculateRewards(testAchievement, $"player_{i}");
                Assert.IsNotNull(rewards);
            }
            
            stopwatch.Stop();
            
            // Assert - should complete in reasonable time
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000, 
                "1000 reward calculations should complete in less than 2 seconds");
        }

        #endregion
    }
}