using UnityEngine;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Progression;

namespace ProjectChimera
{
    /// <summary>
    /// Simple test to verify clean progression system compiles and works
    /// </summary>
    public class CleanProgressionTest : MonoBehaviour
    {
        [Header("Clean Progression Test")]
        [SerializeField] private CleanProgressionManager _progressionManager;
        
        [Header("Test Configuration")]
        [SerializeField] private ProgressionDifficulty _testDifficulty = ProgressionDifficulty.Beginner;
        [SerializeField] private ProgressionCategory _testCategory = ProgressionCategory.Genetics;
        [SerializeField] private CleanProgressionRewardType _testRewardType = CleanProgressionRewardType.Experience;
        [SerializeField] private ProgressionEventType _testEventType = ProgressionEventType.AchievementUnlocked;
        [SerializeField] private ProgressionSessionType _testSessionType = ProgressionSessionType.Training;
        
        private void Start()
        {
            TestCleanTypes();
            TestCleanManager();
        }
        
        private void TestCleanTypes()
        {
            // Test clean type instantiation
            var achievement = new CleanProgressionAchievement();
            var experience = new CleanProgressionExperience();
            var milestone = new CleanProgressionMilestone();
            var skillNode = new CleanProgressionSkillNode();
            var leaderboard = new CleanProgressionLeaderboard();
            var reward = new CleanProgressionReward();
            var campaign = new CleanProgressionCampaign();
            
            Debug.Log("Clean progression types instantiated successfully!");
            Debug.Log($"Test configuration: Difficulty={_testDifficulty}, Category={_testCategory}, " +
                     $"Reward={_testRewardType}, Event={_testEventType}, Session={_testSessionType}");
        }
        
        private void TestCleanManager()
        {
            if (_progressionManager == null)
            {
                _progressionManager = FindObjectOfType<CleanProgressionManager>();
            }
            
            if (_progressionManager != null)
            {
                Debug.Log($"Clean Progression Manager found: Ready={_progressionManager.IsSystemReady}");
                Debug.Log($"Total experience: {_progressionManager.GetTotalExperience()}");
                Debug.Log($"Total achievements: {_progressionManager.GetTotalAchievements()}");
                Debug.Log($"Unlocked skills: {_progressionManager.GetUnlockedSkills()}");
                Debug.Log($"Available achievements: {_progressionManager.GetAchievements().Count}");
                Debug.Log($"Available skill nodes: {_progressionManager.GetSkillNodes().Count}");
                Debug.Log($"Available campaigns: {_progressionManager.GetCampaigns().Count}");
            }
            else
            {
                Debug.LogWarning("No CleanProgressionManager found in scene");
            }
        }
        
        [ContextMenu("Run Clean Test")]
        public void RunCleanTest()
        {
            TestCleanTypes();
            TestCleanManager();
        }
        
        [ContextMenu("Test Progression Features")]
        public void TestProgressionFeatures()
        {
            if (_progressionManager != null && _progressionManager.IsSystemReady)
            {
                // Test experience gain
                _progressionManager.AddExperience("Test Action", 50f, _testCategory);
                
                // Test achievement progress
                var achievements = _progressionManager.GetAchievements();
                if (achievements.Count > 0)
                {
                    _progressionManager.UpdateAchievementProgress(achievements[0].AchievementID, achievements[0].RequiredProgress);
                }
                
                // Test skill unlock
                var skills = _progressionManager.GetSkillNodes();
                if (skills.Count > 0)
                {
                    _progressionManager.UnlockSkill(skills[0].NodeID);
                }
                
                Debug.Log("Progression features tested successfully!");
            }
            else
            {
                Debug.LogWarning("CleanProgressionManager not ready for testing");
            }
        }
    }
}