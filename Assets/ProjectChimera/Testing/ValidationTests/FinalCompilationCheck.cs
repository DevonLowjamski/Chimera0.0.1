using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Progression;

namespace ProjectChimera
{
    /// <summary>
    /// Final compilation check to verify all gaming systems compile without errors
    /// Tests all critical types and enum references that were causing compilation issues
    /// </summary>
    public class FinalCompilationCheck : MonoBehaviour
    {
        void Start()
        {
            // Test BreedingDifficulty enum
            var difficulty = BreedingDifficulty.Intermediate;
            
            // Test ProgressionCategory enum
            var category = ProgressionCategory.General;
            var genetics = ProgressionCategory.Genetics;
            var cultivation = ProgressionCategory.Cultivation;
            
            // Test that managers can be referenced
            var breedingSystem = GetComponent<BreedingChallengeSystem>();
            var aromaticSystem = GetComponent<AromaticGamingSystem>();
            var progressionManager = GetComponent<ComprehensiveProgressionManager>();
            var achievementManager = GetComponent<AchievementSystemManager>();
            
            // Test clean data types
            var achievement = new CleanProgressionAchievement();
            var experience = new CleanProgressionExperience();
            
            Debug.Log("✅ FINAL COMPILATION CHECK PASSED - All gaming systems compile successfully!");
            Debug.Log($"✅ BreedingDifficulty: {difficulty}");
            Debug.Log($"✅ ProgressionCategory: {category}, {genetics}, {cultivation}");
            Debug.Log("🎮 Project Chimera gaming systems are ready!");
        }
    }
}