using UnityEngine;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Data.Progression;

namespace ProjectChimera
{
    /// <summary>
    /// Simple compilation test to verify progression system fixes
    /// Tests that all types compile without namespace conflicts or duplicate definitions
    /// </summary>
    public class ProgressionCompilationTest : MonoBehaviour
    {
        void Start()
        {
            // Test that we can reference all progression types without conflicts
            var progressionManager = GetComponent<ProgressionManager>();
            var comprehensiveManager = GetComponent<ComprehensiveProgressionManager>();
            var achievementManager = GetComponent<AchievementSystemManager>();
            var cleanManager = GetComponent<CleanProgressionManager>();
            
            // Test enum usage
            var category = ProgressionCategory.General;
            var genetics = ProgressionCategory.Genetics;
            var cultivation = ProgressionCategory.Cultivation;
            
            // Test shared types
            var achievement = new CleanProgressionAchievement();
            var experience = new CleanProgressionExperience();
            var milestone = new CleanProgressionMilestone();
            
            Debug.Log("✅ Progression compilation test passed - no conflicts or duplicate definitions");
        }
    }
}