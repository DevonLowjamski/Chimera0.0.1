using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Visuals;
using ProjectChimera.Data.Genetics;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Runtime test component for SpeedTree integration. Can be added to GameObjects in the scene.
    /// Tests the core integration points and data flow without requiring actual SpeedTree assets.
    /// </summary>
    public class SpeedTreeRuntimeTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runTestOnStart = false;
        [SerializeField] private PlantSpeciesSO _testSpecies;
        [SerializeField] private PlantStrainSO _testStrain;

        [Header("Test Results")]
        [SerializeField, TextArea(3, 10)] private string _lastTestResults = "No tests run yet";

        private void Start()
        {
            if (_runTestOnStart)
            {
                RunIntegrationTest();
            }
        }

        [ContextMenu("Run SpeedTree Integration Test")]
        public void RunIntegrationTest()
        {
            Debug.Log("[SpeedTree Test] Starting integration test...");
            _lastTestResults = "Running tests...\n";

            bool allTestsPassed = true;
            string results = "";

            allTestsPassed &= TestManagerInitialization(ref results);
            allTestsPassed &= TestParameterCalculation(ref results);
            allTestsPassed &= TestGrowthStageConfiguration(ref results);
            allTestsPassed &= TestEnvironmentalMapping(ref results);

            if (allTestsPassed)
            {
                results += "\n✅ ALL INTEGRATION TESTS PASSED!";
                Debug.Log("[SpeedTree Test] ✅ All integration tests PASSED!");
            }
            else
            {
                results += "\n❌ SOME INTEGRATION TESTS FAILED!";
                Debug.LogError("[SpeedTree Test] ❌ Some integration tests FAILED!");
            }

            _lastTestResults = results;
        }

        private bool TestManagerInitialization(ref string results)
        {
            results += "Testing PlantVisualizationManager initialization...\n";

            var manager = FindAnyObjectByType<PlantVisualizationManager>();
            if (manager == null)
            {
                results += "❌ PlantVisualizationManager not found in scene!\n";
                Debug.LogError("[SpeedTree Test] PlantVisualizationManager not found in scene!");
                return false;
            }

            if (!manager.IsInitialized)
            {
                results += "❌ PlantVisualizationManager is not initialized!\n";
                Debug.LogError("[SpeedTree Test] PlantVisualizationManager is not initialized!");
                return false;
            }

            results += "✅ Manager initialization test passed\n";
            Debug.Log("[SpeedTree Test] ✅ Manager initialization test passed");
            return true;
        }

        private bool TestParameterCalculation(ref string results)
        {
            results += "Testing parameter calculation system...\n";

            // Create test parameter mapper
            var mapper = ScriptableObject.CreateInstance<SpeedTreeParameterMapperSO>();
            
            // Create test cultivation conditions
            var conditions = new CultivationConditions
            {
                Temperature = 24f,
                Humidity = 55f,
                LightIntensity = 400f,
                CO2Level = 800f,
                NitrogenLevel = 0.8f,
                PhosphorusLevel = 0.8f,
                PotassiumLevel = 0.8f
            };

            try
            {
                var parameters = mapper.CalculateSpeedTreeParameters(conditions, _testStrain);
                
                if (parameters.OverallHealth < 0f || parameters.OverallHealth > 1f)
                {
                    results += $"❌ Invalid OverallHealth value: {parameters.OverallHealth}\n";
                    Debug.LogError("[SpeedTree Test] Invalid OverallHealth value: " + parameters.OverallHealth);
                    return false;
                }

                results += $"✅ Parameter calculation test passed (Health: {parameters.OverallHealth:F2}, Growth: {parameters.GrowthRate:F2})\n";
                Debug.Log($"[SpeedTree Test] Calculated parameters - Health: {parameters.OverallHealth:F2}, Growth: {parameters.GrowthRate:F2}");
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Parameter calculation failed: {e.Message}\n";
                Debug.LogError("[SpeedTree Test] Parameter calculation failed: " + e.Message);
                return false;
            }
            finally
            {
                DestroyImmediate(mapper);
            }
        }

        private bool TestGrowthStageConfiguration(ref string results)
        {
            results += "Testing growth stage configuration...\n";

            var config = ScriptableObject.CreateInstance<GrowthStageConfigurationSO>();
            
            try
            {
                // Test seasonal parameter calculation
                float seedlingParam = config.GetSeasonalParameterForStage(PlantGrowthStage.Seedling);
                float floweringParam = config.GetSeasonalParameterForStage(PlantGrowthStage.Flowering);
                
                if (seedlingParam < 0f || seedlingParam > 1f || floweringParam < 0f || floweringParam > 1f)
                {
                    results += "❌ Invalid seasonal parameters!\n";
                    Debug.LogError("[SpeedTree Test] Invalid seasonal parameters!");
                    return false;
                }

                results += $"✅ Growth stage configuration test passed (Seedling: {seedlingParam:F2}, Flowering: {floweringParam:F2})\n";
                Debug.Log($"[SpeedTree Test] Seasonal parameters - Seedling: {seedlingParam:F2}, Flowering: {floweringParam:F2}");
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Growth stage configuration failed: {e.Message}\n";
                Debug.LogError("[SpeedTree Test] Growth stage configuration failed: " + e.Message);
                return false;
            }
            finally
            {
                DestroyImmediate(config);
            }
        }

        private bool TestEnvironmentalMapping(ref string results)
        {
            results += "Testing environmental data mapping...\n";

            if (_testSpecies == null)
            {
                results += "⚠️ No test species assigned, skipping environmental mapping test\n";
                Debug.LogWarning("[SpeedTree Test] No test species assigned, skipping environmental mapping test");
                return true;
            }

            try
            {
                var optimal = _testSpecies.GetOptimalEnvironment();
                
                if (optimal.Temperature <= 0f || optimal.Humidity <= 0f)
                {
                    results += "❌ Invalid optimal environment values!\n";
                    Debug.LogError("[SpeedTree Test] Invalid optimal environment values!");
                    return false;
                }

                var testEnv = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
                float suitability = _testSpecies.EvaluateEnvironmentalSuitability(testEnv);
                
                if (suitability < 0f || suitability > 1f)
                {
                    results += $"❌ Invalid suitability score: {suitability}\n";
                    Debug.LogError("[SpeedTree Test] Invalid suitability score: " + suitability);
                    return false;
                }

                results += $"✅ Environmental mapping test passed (Suitability: {suitability:F2})\n";
                Debug.Log($"[SpeedTree Test] Environmental mapping - Suitability: {suitability:F2}");
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Environmental mapping failed: {e.Message}\n";
                Debug.LogError("[SpeedTree Test] Environmental mapping failed: " + e.Message);
                return false;
            }
        }

        [ContextMenu("Test Instance Creation (Mock)")]
        public void TestMockInstanceCreation()
        {
            Debug.Log("[SpeedTree Test] Testing mock instance creation...");

            var manager = FindAnyObjectByType<PlantVisualizationManager>();
            if (manager == null || _testStrain == null)
            {
                Debug.LogError("[SpeedTree Test] Manager or test strain not available!");
                _lastTestResults = "❌ Manager or test strain not available for mock instance test!";
                return;
            }

            // This will fail gracefully without SpeedTree assets, but tests the call path
            var instance = manager.CreatePlantInstance("test_001", _testStrain, Vector3.zero);
            
            if (instance == null)
            {
                _lastTestResults = "⚠️ Instance creation failed (expected without SpeedTree assets)";
                Debug.LogWarning("[SpeedTree Test] Instance creation failed (expected without SpeedTree assets)");
            }
            else
            {
                _lastTestResults = "✅ Mock instance created successfully!";
                Debug.Log("[SpeedTree Test] ✅ Mock instance created successfully!");
            }
        }

        [ContextMenu("Clear Test Results")]
        public void ClearTestResults()
        {
            _lastTestResults = "Test results cleared";
        }
    }
}