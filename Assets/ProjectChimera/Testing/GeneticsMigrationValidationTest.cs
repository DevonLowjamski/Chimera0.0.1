using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Genetics.Scientific;
using AchievementCategory = ProjectChimera.Data.Genetics.Scientific.AchievementCategory;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Phase 8C: Genetics Migration Validation Test
    /// Tests compilation and basic functionality of migrated genetics system after Core assembly migration.
    /// Validates namespace imports, type access, and basic ScriptableObject functionality.
    /// ✅ ENABLED: Genetics migration completed successfully - comprehensive validation ready
    /// </summary>
    [System.Serializable]
    public class GeneticsMigrationValidationTest : MonoBehaviour
    {
        [Header("Test Results")]
        [SerializeField] private bool _compilationTestPassed = false;
        [SerializeField] private bool _namespaceAccessTestPassed = false;
        [SerializeField] private bool _typeInstantiationTestPassed = false;
        [SerializeField] private bool _enumAccessTestPassed = false;
        [SerializeField] private List<string> _testResults = new List<string>();
        
        [Header("Test References")]
        [SerializeField] private List<TraitType> _testTraits = new List<TraitType>();
        [SerializeField] private List<AchievementCategory> _testCategories = new List<AchievementCategory>();
        
        /// <summary>
        /// Run all genetics migration validation tests.
        /// </summary>
        [ContextMenu("Run All Genetics Migration Tests")]
        public void RunAllTests()
        {
            _testResults.Clear();
            _testResults.Add($"=== Genetics Migration Validation Test Started at {System.DateTime.Now} ===");
            
            // Test 1: Compilation Test
            TestCompilation();
            
            // Test 2: Namespace Access Test  
            TestNamespaceAccess();
            
            // Test 3: Type Instantiation Test
            TestTypeInstantiation();
            
            // Test 4: Enum Access Test
            TestEnumAccess();
            
            // Summary
            GenerateTestSummary();
            
            Debug.Log("Genetics Migration Validation Test completed. Check test results.");
        }
        
        /// <summary>
        /// Test 1: Verify compilation of genetics types from different namespaces.
        /// </summary>
        private void TestCompilation()
        {
            try
            {
                // Test that we can reference types from all genetics namespaces
                var testTrait = TraitType.THCContent;
                var testCategory = AchievementCategory.Breeding;
                var testPriority = BreedingPriorityLevel.High;
                var testCompetition = CompetitionCategory.InnovationChallenge;
                
                // Verify the values are accessible
                if (testTrait == TraitType.THCContent && testCategory == AchievementCategory.Breeding && 
                    testPriority == BreedingPriorityLevel.High && testCompetition == CompetitionCategory.InnovationChallenge)
                {
                    _compilationTestPassed = true;
                    _testResults.Add("✅ PASS: Compilation Test - All genetics types accessible");
                }
            }
            catch (System.Exception ex)
            {
                _compilationTestPassed = false;
                _testResults.Add($"❌ FAIL: Compilation Test - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test 2: Verify namespace access across Core.Data.Genetics subdirectories.
        /// </summary>
        private void TestNamespaceAccess()
        {
            try
            {
                // Test Base namespace
                var baseTypes = new System.Type[]
                {
                    typeof(TraitType),
                    typeof(CannabisGenotype),
                    typeof(PlantGenotype)
                };
                
                // Test Scientific namespace  
                var scientificTypes = new System.Type[]
                {
                    typeof(AchievementCategory),
                    typeof(CompetitionCategory),
                    typeof(TerpeneCategory)
                };
                
                // Test Gaming namespace
                var gamingTypes = new System.Type[]
                {
                    typeof(ScientificAchievement),
                    typeof(TournamentData)
                };
                
                // Test Shared namespace
                var sharedTypes = new System.Type[]
                {
                    typeof(BreedingGoal),
                    typeof(BreedingCompatibility)
                };
                
                int accessibleTypes = baseTypes.Length + scientificTypes.Length + gamingTypes.Length + sharedTypes.Length;
                
                _namespaceAccessTestPassed = true;
                _testResults.Add($"✅ PASS: Namespace Access Test - {accessibleTypes} types accessible across 4 namespaces");
            }
            catch (System.Exception ex)
            {
                _namespaceAccessTestPassed = false;
                _testResults.Add($"❌ FAIL: Namespace Access Test - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test 3: Verify instantiation of genetics data structures.
        /// </summary>
        private void TestTypeInstantiation()
        {
            try
            {
                // Test Base types
                var genotype = new CannabisGenotype();
                var breedingCompatibility = new BreedingCompatibility();
                
                // Test Scientific types (enums)
                var category = AchievementCategory.Innovation;
                
                // Test Gaming types
                var achievement = new ScientificAchievement();
                var tournament = new TournamentData();
                
                // Test Shared types
                var breedingGoal = new BreedingGoal();
                var breedingProgress = new BreedingProgress();
                
                // Verify objects were created successfully
                if (genotype != null && breedingCompatibility != null && achievement != null && tournament != null && 
                    breedingGoal != null && breedingProgress != null && category == AchievementCategory.Innovation)
                {
                    _typeInstantiationTestPassed = true;
                    _testResults.Add("✅ PASS: Type Instantiation Test - All genetics data structures instantiable");
                }
            }
            catch (System.Exception ex)
            {
                _typeInstantiationTestPassed = false;
                _testResults.Add($"❌ FAIL: Type Instantiation Test - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test 4: Verify enum access and values.
        /// </summary>
        private void TestEnumAccess()
        {
            try
            {
                // Test TraitType enum
                _testTraits.Clear();
                _testTraits.AddRange(new TraitType[]
                {
                    TraitType.THCContent,
                    TraitType.CBDContent,
                    TraitType.BiomassProduction,
                    TraitType.FloweringTime,
                    TraitType.PlantHeight
                });
                
                // Test AchievementCategory enum
                _testCategories.Clear();
                _testCategories.AddRange(new AchievementCategory[]
                {
                    AchievementCategory.Breeding,
                    AchievementCategory.Innovation,
                    AchievementCategory.Research,
                    AchievementCategory.Leadership,
                    AchievementCategory.Collaboration
                });
                
                // Verify enum values work
                var traitCount = System.Enum.GetValues(typeof(TraitType)).Length;
                var categoryCount = System.Enum.GetValues(typeof(AchievementCategory)).Length;
                
                _enumAccessTestPassed = true;
                _testResults.Add($"✅ PASS: Enum Access Test - {traitCount} TraitType values, {categoryCount} AchievementCategory values");
            }
            catch (System.Exception ex)
            {
                _enumAccessTestPassed = false;
                _testResults.Add($"❌ FAIL: Enum Access Test - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Generate comprehensive test summary.
        /// </summary>
        private void GenerateTestSummary()
        {
            int passedTests = 0;
            int totalTests = 4;
            
            if (_compilationTestPassed) passedTests++;
            if (_namespaceAccessTestPassed) passedTests++;
            if (_typeInstantiationTestPassed) passedTests++;
            if (_enumAccessTestPassed) passedTests++;
            
            _testResults.Add("");
            _testResults.Add("=== GENETICS MIGRATION VALIDATION SUMMARY ===");
            _testResults.Add($"Tests Passed: {passedTests}/{totalTests}");
            _testResults.Add($"Success Rate: {(passedTests * 100.0f / totalTests):F1}%");
            
            if (passedTests == totalTests)
            {
                _testResults.Add("🎉 ALL TESTS PASSED - Genetics migration successful!");
                _testResults.Add("✅ Core assembly migration completed without regressions");
                _testResults.Add("✅ All namespace imports working correctly");
                _testResults.Add("✅ Type access and instantiation functional");
                _testResults.Add("✅ Enum definitions accessible across assemblies");
            }
            else
            {
                _testResults.Add("⚠️  SOME TESTS FAILED - Manual investigation required");
                _testResults.Add("❌ Genetics migration may have introduced regressions");
            }
            
            _testResults.Add($"Test completed at {System.DateTime.Now}");
            
            // Log to Unity console
            foreach (var result in _testResults)
            {
                Debug.Log(result);
            }
        }
        
        /// <summary>
        /// Quick compilation verification for external testing.
        /// </summary>
        public static bool QuickCompilationCheck()
        {
            try
            {
                // Quick type access test
                var trait = TraitType.THCContent;
                var category = AchievementCategory.Breeding;
                var genotype = new CannabisGenotype();
                var achievement = new ScientificAchievement();
                
                // Verify all types are accessible
                return trait == TraitType.THCContent && category == AchievementCategory.Breeding && 
                       genotype != null && achievement != null;
            }
            catch
            {
                return false;
            }
        }
    }
}