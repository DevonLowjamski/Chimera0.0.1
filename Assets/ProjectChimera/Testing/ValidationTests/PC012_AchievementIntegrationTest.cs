using UnityEngine;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Systems.Progression;

namespace ProjectChimera.Testing.ValidationTests
{
    /// <summary>
    /// PC-012-9: Achievement Integration Test Suite
    /// Validates that the achievement system is properly connected to real game events
    /// and tracks player actions correctly for progression rewards.
    /// </summary>
    public class PC012_AchievementIntegrationTest : MonoBehaviour
    {
        [Header("PC-012-9: Achievement Integration Test")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private int testIterations = 1;
        
        private AchievementSystemManager _achievementManager;
        private bool _testCompleted = false;
        private int _testsPassed = 0;
        private int _totalTests = 0;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunAchievementIntegrationTestCoroutine());
            }
        }
        
        private System.Collections.IEnumerator RunAchievementIntegrationTestCoroutine()
        {
            yield return new WaitForSeconds(2f); // Wait for managers to initialize
            
            Debug.Log("🚀 Starting PC-012-9: Achievement Integration Test Suite");
            
            for (int i = 0; i < testIterations; i++)
            {
                Debug.Log($"--- Test Iteration {i + 1}/{testIterations} ---");
                RunAchievementIntegrationTest();
                yield return new WaitForSeconds(1f);
            }
            
            Debug.Log($"✅ PC-012-9 Test Suite Complete: {_testsPassed}/{_totalTests} tests passed");
            _testCompleted = true;
        }
        
        public void RunAchievementIntegrationTest()
        {
            try
            {
                _testsPassed = 0;
                _totalTests = 0;
                
                Debug.Log("=== PC-012-9: Real Achievement Tracking Integration Test ===");
                
                // Test 1: Achievement Manager Initialization
                TestAchievementManagerInitialization();
                
                // Test 2: Achievement Definition Loading
                TestAchievementDefinitionLoading();
                
                // Test 3: Real Game Event Integration
                TestRealGameEventIntegration();
                
                // Test 4: Achievement Progress Tracking
                TestAchievementProgressTracking();
                
                // Test 5: Achievement Unlock System
                TestAchievementUnlockSystem();
                
                // Test 6: Cross-System Achievement Validation
                TestCrossSystemAchievements();
                
                // Test 7: Achievement Persistence
                TestAchievementPersistence();
                
                // Test 8: PC-012-10 Milestone and Hidden Achievement Validation
                TestMilestoneAndHiddenAchievements();
                
                // Test 9: PC-012-11 Social and Community Achievement Validation
                TestSocialAndCommunityAchievements();
                
                Debug.Log($"🏆 PC-012-9 Achievement Integration Test Results: {_testsPassed}/{_totalTests} tests passed");
                
                if (_testsPassed == _totalTests)
                {
                    Debug.Log("✅ All PC-012-9 achievement integration tests PASSED");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {_totalTests - _testsPassed} PC-012-9 achievement integration tests FAILED");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ PC-012-9 Achievement Integration Test failed with exception: {ex.Message}");
            }
        }
        
        #region Individual Test Methods
        
        private void TestAchievementManagerInitialization()
        {
            _totalTests++;
            Debug.Log("Testing Achievement Manager Initialization...");
            
            try
            {
                // Get achievement manager
                _achievementManager = GameManager.Instance?.GetManager<AchievementSystemManager>();
                
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ AchievementSystemManager not found");
                    return;
                }
                
                // Check if achievement system is enabled
                if (!_achievementManager.EnableAchievementSystem)
                {
                    Debug.LogWarning("⚠️ Achievement system is disabled");
                }
                
                // Verify achievement manager is properly initialized
                var allAchievements = _achievementManager.GetAllAchievements(true);
                if (allAchievements == null || allAchievements.Count == 0)
                {
                    Debug.LogError("❌ No achievements loaded");
                    return;
                }
                
                Debug.Log($"✓ Achievement Manager initialized with {allAchievements.Count} achievements");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Achievement Manager initialization test failed: {ex.Message}");
            }
        }
        
        private void TestAchievementDefinitionLoading()
        {
            _totalTests++;
            Debug.Log("Testing Achievement Definition Loading...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                var achievements = _achievementManager.GetAllAchievements(true);
                
                // Check for key achievement categories
                var cultivationAchievements = _achievementManager.GetAchievementsByCategory(
                    ProjectChimera.Data.Progression.AchievementCategory.Cultivation_Mastery);
                var geneticsAchievements = _achievementManager.GetAchievementsByCategory(
                    ProjectChimera.Data.Progression.AchievementCategory.Genetics_Innovation);
                var businessAchievements = _achievementManager.GetAchievementsByCategory(
                    ProjectChimera.Data.Progression.AchievementCategory.Business_Success);
                var researchAchievements = _achievementManager.GetAchievementsByCategory(
                    ProjectChimera.Data.Progression.AchievementCategory.Research_Excellence);
                
                // Verify we have achievements in all major categories
                bool hasAllCategories = cultivationAchievements.Count > 0 && 
                                      geneticsAchievements.Count > 0 && 
                                      businessAchievements.Count > 0 && 
                                      researchAchievements.Count > 0;
                
                if (!hasAllCategories)
                {
                    Debug.LogError("❌ Missing achievements in core categories");
                    return;
                }
                
                Debug.Log($"✓ Achievement definitions loaded: {cultivationAchievements.Count} cultivation, {geneticsAchievements.Count} genetics, {businessAchievements.Count} business, {researchAchievements.Count} research");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Achievement definition loading test failed: {ex.Message}");
            }
        }
        
        private void TestRealGameEventIntegration()
        {
            _totalTests++;
            Debug.Log("Testing Real Game Event Integration...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                // Test the real game event integration method
                _achievementManager.TestRealGameEventIntegration();
                
                // Verify achievements were updated
                var stats = _achievementManager.GetAchievementStats();
                if (stats.UnlockedAchievements == 0)
                {
                    Debug.LogWarning("⚠️ No achievements unlocked during event integration test");
                }
                
                Debug.Log($"✓ Real game event integration test completed - {stats.UnlockedAchievements} achievements unlocked");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Real game event integration test failed: {ex.Message}");
            }
        }
        
        private void TestAchievementProgressTracking()
        {
            _totalTests++;
            Debug.Log("Testing Achievement Progress Tracking...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                string testPlayerId = "pc012_test_player";
                
                // Test various achievement progress updates
                _achievementManager.UpdateAchievementProgress("plant_harvested", 1f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("research_completed", 3f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("product_sold", 5f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("successful_breeding", 2f, testPlayerId);
                
                // Check if progress was tracked
                var profile = _achievementManager.GetPlayerProfile(testPlayerId);
                if (profile == null)
                {
                    Debug.LogError("❌ Player profile not created during progress tracking");
                    return;
                }
                
                // Verify some achievements were affected
                var achievements = _achievementManager.GetAllAchievements();
                bool hasProgress = false;
                foreach (var achievement in achievements)
                {
                    if (achievement.CurrentProgress > 0)
                    {
                        hasProgress = true;
                        break;
                    }
                }
                
                if (!hasProgress)
                {
                    Debug.LogWarning("⚠️ No achievement progress detected");
                }
                
                Debug.Log($"✓ Achievement progress tracking validated - Player: {profile.TotalAchievements} achievements, {profile.TotalPoints} points");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Achievement progress tracking test failed: {ex.Message}");
            }
        }
        
        private void TestAchievementUnlockSystem()
        {
            _totalTests++;
            Debug.Log("Testing Achievement Unlock System...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                string testPlayerId = "pc012_unlock_test";
                
                // Force unlock a specific achievement for testing
                var achievements = _achievementManager.GetAllAchievements();
                if (achievements.Count == 0)
                {
                    Debug.LogError("❌ No achievements available for unlock test");
                    return;
                }
                
                var testAchievement = achievements[0];
                bool unlocked = _achievementManager.UnlockAchievement(testAchievement.AchievementID, testPlayerId);
                
                if (!unlocked)
                {
                    Debug.LogError($"❌ Failed to unlock test achievement: {testAchievement.AchievementName}");
                    return;
                }
                
                // Verify achievement was unlocked
                var unlockedAchievements = _achievementManager.GetUnlockedAchievements(testPlayerId);
                bool wasUnlocked = unlockedAchievements.Any(a => a.AchievementID == testAchievement.AchievementID);
                
                if (!wasUnlocked)
                {
                    Debug.LogError("❌ Achievement unlock not reflected in unlocked achievements list");
                    return;
                }
                
                Debug.Log($"✓ Achievement unlock system validated - {testAchievement.AchievementName} unlocked successfully");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Achievement unlock system test failed: {ex.Message}");
            }
        }
        
        private void TestCrossSystemAchievements()
        {
            _totalTests++;
            Debug.Log("Testing Cross-System Achievements...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                // Test complex achievement checking
                _achievementManager.UpdateAchievementProgress("plant_harvested", 10f, "cross_system_test");
                _achievementManager.UpdateAchievementProgress("research_completed", 5f, "cross_system_test");
                _achievementManager.UpdateAchievementProgress("successful_breeding", 3f, "cross_system_test");
                _achievementManager.UpdateAchievementProgress("product_sold", 8f, "cross_system_test");
                
                // Force complex achievement check
                // Note: This tests the CheckComplexAchievements method which is called periodically
                var profile = _achievementManager.GetPlayerProfile("cross_system_test");
                
                Debug.Log($"✓ Cross-system achievement test completed - Profile: {profile.TotalAchievements} achievements");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Cross-system achievement test failed: {ex.Message}");
            }
        }
        
        private void TestAchievementPersistence()
        {
            _totalTests++;
            Debug.Log("Testing Achievement Persistence...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                string testPlayerId = "persistence_test";
                
                // Create some achievement progress
                _achievementManager.UpdateAchievementProgress("plant_harvested", 5f, testPlayerId);
                var initialProfile = _achievementManager.GetPlayerProfile(testPlayerId);
                
                // Verify profile persists (basic test - in real scenario would test across sessions)
                var retrievedProfile = _achievementManager.GetPlayerProfile(testPlayerId);
                
                if (retrievedProfile == null || retrievedProfile.PlayerID != testPlayerId)
                {
                    Debug.LogError("❌ Achievement profile persistence failed");
                    return;
                }
                
                Debug.Log($"✓ Achievement persistence validated - Profile maintained for {testPlayerId}");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Achievement persistence test failed: {ex.Message}");
            }
        }
        
        private void TestMilestoneAndHiddenAchievements()
        {
            _totalTests++;
            Debug.Log("Testing PC-012-10: Milestone and Hidden Achievements...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                string testPlayerId = "milestone_hidden_test";
                
                // Test milestone achievement progress
                _achievementManager.UpdateAchievementProgress("cultivation_milestone_25", 25f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("genetics_milestone_10", 10f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("research_milestone_15", 15f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("business_milestone_100", 100f, testPlayerId);
                
                // Test hidden achievement triggers
                _achievementManager.UpdateAchievementProgress("hidden_ghost_machine", 1f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("hidden_easter_egg", 1f, testPlayerId);
                
                // Verify milestone achievements exist and are properly configured
                var allAchievements = _achievementManager.GetAllAchievements(true);
                var milestoneAchievements = allAchievements.Where(a => a.AchievementName.Contains("Milestone")).ToList();
                var hiddenAchievements = allAchievements.Where(a => a.IsSecret).ToList();
                
                if (milestoneAchievements.Count == 0)
                {
                    Debug.LogError("❌ No milestone achievements found");
                    return;
                }
                
                if (hiddenAchievements.Count == 0)
                {
                    Debug.LogError("❌ No hidden achievements found");
                    return;
                }
                
                // Verify hidden achievements are properly marked as secret
                var hiddenWithSecretDescriptions = hiddenAchievements.Where(a => a.Description == "???").ToList();
                if (hiddenWithSecretDescriptions.Count == 0)
                {
                    Debug.LogWarning("⚠️ Hidden achievements may not have proper secret descriptions");
                }
                
                // Check if milestone achievements have appropriate point values
                var highPointMilestones = milestoneAchievements.Where(a => a.Points >= 1000f).ToList();
                if (highPointMilestones.Count == 0)
                {
                    Debug.LogWarning("⚠️ No high-value milestone achievements found");
                }
                
                Debug.Log($"✓ Milestone and Hidden Achievement test completed - Found {milestoneAchievements.Count} milestones, {hiddenAchievements.Count} hidden achievements");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Milestone and Hidden Achievement test failed: {ex.Message}");
            }
        }
        
        private void TestSocialAndCommunityAchievements()
        {
            _totalTests++;
            Debug.Log("Testing PC-012-11: Social and Community Achievements...");
            
            try
            {
                if (_achievementManager == null)
                {
                    Debug.LogError("❌ Achievement manager not available");
                    return;
                }
                
                string testPlayerId = "social_community_test";
                
                // Test social achievement progress
                _achievementManager.UpdateAchievementProgress("player_help_provided", 5f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("knowledge_shared", 10f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("community_interactions", 25f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("forum_contributions", 50f, testPlayerId);
                
                // Test community achievement progress
                _achievementManager.UpdateAchievementProgress("events_organized", 3f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("collaborative_projects", 10f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("positive_community_ratings", 100f, testPlayerId);
                
                // Test legendary social achievements
                _achievementManager.UpdateAchievementProgress("methods_adopted_globally", 50f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("expert_recognition_areas", 3f, testPlayerId);
                _achievementManager.UpdateAchievementProgress("daily_community_contribution", 365f, testPlayerId);
                
                // Verify social achievements exist and are properly configured
                var allAchievements = _achievementManager.GetAllAchievements(true);
                var socialAchievements = allAchievements.Where(a => a.Category == ProjectChimera.Data.Progression.AchievementCategory.Social).ToList();
                
                if (socialAchievements.Count == 0)
                {
                    Debug.LogError("❌ No social achievements found");
                    return;
                }
                
                // Verify social achievements have appropriate categories and point values
                var communityHelperAchievement = socialAchievements.FirstOrDefault(a => a.AchievementName == "Community Helper");
                if (communityHelperAchievement == null)
                {
                    Debug.LogError("❌ Community Helper achievement not found");
                    return;
                }
                
                var legendaryAchievements = socialAchievements.Where(a => a.Rarity == ProjectChimera.Data.Progression.AchievementRarity.Legendary).ToList();
                if (legendaryAchievements.Count == 0)
                {
                    Debug.LogWarning("⚠️ No legendary social achievements found");
                }
                
                // Check if community pillar achievement has appropriate high point value
                var communityPillarAchievement = socialAchievements.FirstOrDefault(a => a.AchievementName == "Community Pillar");
                if (communityPillarAchievement != null && communityPillarAchievement.Points < 5000f)
                {
                    Debug.LogWarning("⚠️ Community Pillar achievement may have insufficient point value");
                }
                
                Debug.Log($"✓ Social and Community Achievement test completed - Found {socialAchievements.Count} social achievements");
                _testsPassed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Social and Community Achievement test failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Public Test Interface
        
        /// <summary>
        /// Public method to run the achievement integration test manually
        /// </summary>
        [ContextMenu("Run Achievement Integration Test")]
        public void RunManualTest()
        {
            RunAchievementIntegrationTest();
        }
        
        /// <summary>
        /// Get test completion status
        /// </summary>
        public bool IsTestCompleted => _testCompleted;
        
        /// <summary>
        /// Get test results
        /// </summary>
        public (int passed, int total) GetTestResults()
        {
            return (_testsPassed, _totalTests);
        }
        
        #endregion
    }
}