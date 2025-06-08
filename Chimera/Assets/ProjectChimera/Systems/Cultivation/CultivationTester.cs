using UnityEngine;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using DataEnvironmental = ProjectChimera.Data.Cultivation.EnvironmentalConditions;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Testing component for cannabis cultivation mechanics.
    /// Provides runtime testing of the cultivation system functionality.
    /// </summary>
    public class CultivationTester : ChimeraMonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private bool _enableAutoTesting = false;
        [SerializeField] private float _testInterval = 5f;
        
        [Header("Test Assets")]
        [SerializeField] private PlantStrainSO _testStrain;
        [SerializeField] private GenotypeDataSO _testGenotype;
        [SerializeField] private GrowthCalculationSO _testGrowthCalculation;
        
        [Header("Test Results")]
        [SerializeField, TextArea(5, 15)] private string _lastTestResults = "No tests run yet";
        
        private CultivationManager _cultivationManager;
        private float _lastTestTime;
        private int _testRunCount = 0;
        
        protected override void Start()
        {
            base.Start();
            
            _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            
            if (_runTestsOnStart)
            {
                RunCultivationTests();
            }
        }
        
        private void Update()
        {
            if (_enableAutoTesting && Time.time - _lastTestTime > _testInterval)
            {
                RunCultivationTests();
                _lastTestTime = Time.time;
            }
        }
        
        [ContextMenu("Run Cultivation Tests")]
        public void RunCultivationTests()
        {
            Debug.Log("[CultivationTester] Starting cultivation system tests...");
            _testRunCount++;
            
            string results = $"=== CULTIVATION TEST RUN #{_testRunCount} ===\n";
            results += $"Time: {System.DateTime.Now:HH:mm:ss}\n\n";
            
            bool allTestsPassed = true;
            
            allTestsPassed &= TestManagerInitialization(ref results);
            allTestsPassed &= TestPlantCreation(ref results);
            allTestsPassed &= TestEnvironmentalConditions(ref results);
            allTestsPassed &= TestGrowthCalculations(ref results);
            allTestsPassed &= TestPlantLifecycle(ref results);
            allTestsPassed &= TestResourceManagement(ref results);
            
            if (allTestsPassed)
            {
                results += "\n✅ ALL CULTIVATION TESTS PASSED!\n";
                Debug.Log("[CultivationTester] ✅ All cultivation tests PASSED!");
            }
            else
            {
                results += "\n❌ SOME CULTIVATION TESTS FAILED!\n";
                Debug.LogError("[CultivationTester] ❌ Some cultivation tests FAILED!");
            }
            
            _lastTestResults = results;
        }
        
        private bool TestManagerInitialization(ref string results)
        {
            results += "Testing CultivationManager Initialization...\n";
            
            if (_cultivationManager == null)
            {
                results += "❌ CultivationManager not found!\n";
                Debug.LogError("[CultivationTester] CultivationManager not found!");
                return false;
            }
            
            if (!_cultivationManager.IsInitialized)
            {
                results += "❌ CultivationManager not initialized!\n";
                Debug.LogError("[CultivationTester] CultivationManager not initialized!");
                return false;
            }
            
            results += $"✅ CultivationManager initialized successfully\n";
            results += $"   - Active plants: {_cultivationManager.ActivePlantCount}\n";
            results += $"   - Total grown: {_cultivationManager.TotalPlantsGrown}\n";
            results += $"   - Auto-growth enabled: {_cultivationManager.EnableAutoGrowth}\n";
            
            return true;
        }
        
        private bool TestPlantCreation(ref string results)
        {
            results += "\nTesting Plant Creation...\n";
            
            if (_testStrain == null)
            {
                results += "⚠️ No test strain assigned, skipping plant creation test\n";
                return true;
            }
            
            try
            {
                // Create a test plant
                Vector3 testPosition = new Vector3(0f, 0f, 0f);
                var testPlant = _cultivationManager.PlantSeed("Test Plant", _testStrain, _testGenotype, testPosition);
                
                if (testPlant == null)
                {
                    results += "❌ Failed to create test plant!\n";
                    return false;
                }
                
                results += $"✅ Plant created successfully\n";
                results += $"   - Plant ID: {testPlant.PlantID}\n";
                results += $"   - Name: {testPlant.PlantName}\n";
                results += $"   - Strain: {testPlant.Strain?.name ?? "None"}\n";
                results += $"   - Current stage: {testPlant.CurrentGrowthStage}\n";
                results += $"   - Health: {testPlant.OverallHealth:F2}\n";
                results += $"   - Height: {testPlant.CurrentHeight:F1}cm\n";
                
                // Clean up test plant
                _cultivationManager.RemovePlant(testPlant.PlantID, false);
                
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Exception during plant creation: {e.Message}\n";
                Debug.LogError($"[CultivationTester] Exception during plant creation: {e.Message}");
                return false;
            }
        }
        
        private bool TestEnvironmentalConditions(ref string results)
        {
            results += "\nTesting Environmental Conditions...\n";
            
            try
            {
                // Test default environment
                var indoorEnv = DataEnvironmental.CreateIndoorDefault();
                var outdoorEnv = DataEnvironmental.CreateOutdoorDefault();
                var stressEnv = DataEnvironmental.CreateStressConditions();
                
                float indoorSuitability = indoorEnv.CalculateOverallSuitability();
                float outdoorSuitability = outdoorEnv.CalculateOverallSuitability();
                float stressSuitability = stressEnv.CalculateOverallSuitability();
                
                results += $"✅ Environmental conditions created successfully\n";
                results += $"   - Indoor suitability: {indoorSuitability:F2}\n";
                results += $"   - Outdoor suitability: {outdoorSuitability:F2}\n";
                results += $"   - Stress suitability: {stressSuitability:F2}\n";
                
                // Test environment validation
                bool indoorValid = indoorEnv.IsWithinAcceptableRanges();
                bool stressValid = stressEnv.IsWithinAcceptableRanges();
                
                results += $"   - Indoor environment valid: {indoorValid}\n";
                results += $"   - Stress environment valid: {stressValid}\n";
                
                // Test zone environment management
                _cultivationManager.SetZoneEnvironment("test_zone", indoorEnv);
                var retrievedEnv = _cultivationManager.GetZoneEnvironment("test_zone");
                
                bool environmentMatches = Mathf.Approximately(retrievedEnv.Temperature, indoorEnv.Temperature);
                results += $"   - Zone environment storage: {(environmentMatches ? "✅" : "❌")}\n";
                
                return indoorValid && environmentMatches;
            }
            catch (System.Exception e)
            {
                results += $"❌ Exception during environmental testing: {e.Message}\n";
                Debug.LogError($"[CultivationTester] Exception during environmental testing: {e.Message}");
                return false;
            }
        }
        
        private bool TestGrowthCalculations(ref string results)
        {
            results += "\nTesting Growth Calculations...\n";
            
            if (_testGrowthCalculation == null)
            {
                results += "⚠️ No test growth calculation assigned, skipping growth calculation test\n";
                return true;
            }
            
            try
            {
                // Create a test plant for calculations
                var testPlant = ScriptableObject.CreateInstance<PlantInstanceSO>();
                testPlant.InitializePlant("test_calc", "Test Calc Plant", _testStrain, _testGenotype, Vector3.zero);
                
                var testEnvironment = DataEnvironmental.CreateIndoorDefault();
                
                // Test growth rate calculation
                float growthRate = _testGrowthCalculation.CalculateGrowthRate(testPlant, testEnvironment);
                float waterConsumption = _testGrowthCalculation.CalculateWaterConsumption(testPlant, testEnvironment);
                float nutrientConsumption = _testGrowthCalculation.CalculateNutrientConsumption(testPlant, testEnvironment);
                float energyConsumption = _testGrowthCalculation.CalculateEnergyConsumption(testPlant, testEnvironment);
                float healthChange = _testGrowthCalculation.CalculateHealthChange(testPlant, testEnvironment);
                float yieldPotential = _testGrowthCalculation.CalculateYieldPotential(testPlant);
                
                results += $"✅ Growth calculations completed successfully\n";
                results += $"   - Growth rate: {growthRate:F3}\n";
                results += $"   - Water consumption: {waterConsumption:F3}\n";
                results += $"   - Nutrient consumption: {nutrientConsumption:F3}\n";
                results += $"   - Energy consumption: {energyConsumption:F3}\n";
                results += $"   - Health change: {healthChange:F3}\n";
                results += $"   - Yield potential: {yieldPotential:F3}\n";
                
                // Validate calculation ranges
                bool validRanges = growthRate >= 0f && growthRate <= 10f &&
                                 waterConsumption >= 0f && waterConsumption <= 1f &&
                                 nutrientConsumption >= 0f && nutrientConsumption <= 1f &&
                                 energyConsumption >= 0f && energyConsumption <= 1f &&
                                 yieldPotential >= 0f && yieldPotential <= 5f;
                
                results += $"   - Calculation ranges valid: {(validRanges ? "✅" : "❌")}\n";
                
                // Clean up
                DestroyImmediate(testPlant);
                
                return validRanges;
            }
            catch (System.Exception e)
            {
                results += $"❌ Exception during growth calculation testing: {e.Message}\n";
                Debug.LogError($"[CultivationTester] Exception during growth calculation testing: {e.Message}");
                return false;
            }
        }
        
        private bool TestPlantLifecycle(ref string results)
        {
            results += "\nTesting Plant Lifecycle...\n";
            
            if (_testStrain == null)
            {
                results += "⚠️ No test strain assigned, skipping lifecycle test\n";
                return true;
            }
            
            try
            {
                // Create a test plant
                var testPlant = _cultivationManager.PlantSeed("Lifecycle Test", _testStrain, _testGenotype, Vector3.zero);
                
                if (testPlant == null)
                {
                    results += "❌ Failed to create plant for lifecycle test!\n";
                    return false;
                }
                
                // Test basic operations
                bool wateringWorked = _cultivationManager.WaterPlant(testPlant.PlantID, 0.3f);
                bool feedingWorked = _cultivationManager.FeedPlant(testPlant.PlantID, 0.2f);
                bool trainingWorked = _cultivationManager.TrainPlant(testPlant.PlantID, "lst");
                
                results += $"✅ Plant lifecycle operations tested\n";
                results += $"   - Watering: {(wateringWorked ? "✅" : "❌")}\n";
                results += $"   - Feeding: {(feedingWorked ? "✅" : "❌")}\n";
                results += $"   - Training: {(trainingWorked ? "✅" : "❌")}\n";
                results += $"   - Water level after watering: {testPlant.WaterLevel:F2}\n";
                results += $"   - Nutrient level after feeding: {testPlant.NutrientLevel:F2}\n";
                results += $"   - Stress level after training: {testPlant.StressLevel:F2}\n";
                
                // Test forced growth update
                _cultivationManager.ForceGrowthUpdate();
                
                results += $"   - Growth update completed\n";
                results += $"   - Plant age: {testPlant.AgeInDays:F1} days\n";
                results += $"   - Current stage: {testPlant.CurrentGrowthStage}\n";
                
                // Clean up
                bool removalWorked = _cultivationManager.RemovePlant(testPlant.PlantID, false);
                results += $"   - Plant removal: {(removalWorked ? "✅" : "❌")}\n";
                
                return wateringWorked && feedingWorked && trainingWorked && removalWorked;
            }
            catch (System.Exception e)
            {
                results += $"❌ Exception during lifecycle testing: {e.Message}\n";
                Debug.LogError($"[CultivationTester] Exception during lifecycle testing: {e.Message}");
                return false;
            }
        }
        
        private bool TestResourceManagement(ref string results)
        {
            results += "\nTesting Resource Management...\n";
            
            try
            {
                // Test bulk operations
                var stats = _cultivationManager.GetCultivationStats();
                results += $"✅ Resource management tested\n";
                results += $"   - Active plants: {stats.active}\n";
                results += $"   - Total grown: {stats.grown}\n";
                results += $"   - Total harvested: {stats.harvested}\n";
                results += $"   - Total yield: {stats.yield:F1}g\n";
                results += $"   - Average health: {stats.avgHealth:F2}\n";
                
                // Test plants needing attention
                var needingAttention = _cultivationManager.GetPlantsNeedingAttention();
                results += $"   - Plants needing attention: {needingAttention.Count()}\n";
                
                // Test plants by stage
                var seedlings = _cultivationManager.GetPlantsByStage(PlantGrowthStage.Seedling);
                var vegetative = _cultivationManager.GetPlantsByStage(PlantGrowthStage.Vegetative);
                var flowering = _cultivationManager.GetPlantsByStage(PlantGrowthStage.Flowering);
                
                results += $"   - Seedlings: {seedlings.Count()}\n";
                results += $"   - Vegetative: {vegetative.Count()}\n";
                results += $"   - Flowering: {flowering.Count()}\n";
                
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Exception during resource management testing: {e.Message}\n";
                Debug.LogError($"[CultivationTester] Exception during resource management testing: {e.Message}");
                return false;
            }
        }
        
        [ContextMenu("Create Test Plants")]
        public void CreateTestPlants()
        {
            if (_cultivationManager == null || _testStrain == null)
            {
                Debug.LogWarning("[CultivationTester] Cannot create test plants: Missing manager or strain");
                return;
            }
            
            // Create several test plants
            for (int i = 0; i < 5; i++)
            {
                Vector3 position = new Vector3(i * 2f, 0f, 0f);
                string plantName = $"Test Plant {i + 1}";
                _cultivationManager.PlantSeed(plantName, _testStrain, _testGenotype, position);
            }
            
            Debug.Log("[CultivationTester] Created 5 test plants");
        }
        
        [ContextMenu("Water All Test Plants")]
        public void WaterAllTestPlants()
        {
            if (_cultivationManager == null) return;
            
            _cultivationManager.WaterAllPlants(0.5f);
            Debug.Log("[CultivationTester] Watered all test plants");
        }
        
        [ContextMenu("Feed All Test Plants")]
        public void FeedAllTestPlants()
        {
            if (_cultivationManager == null) return;
            
            _cultivationManager.FeedAllPlants(0.4f);
            Debug.Log("[CultivationTester] Fed all test plants");
        }
        
        [ContextMenu("Force Growth Update")]
        public void ForceGrowthUpdate()
        {
            if (_cultivationManager == null) return;
            
            _cultivationManager.ForceGrowthUpdate();
            Debug.Log("[CultivationTester] Forced growth update for all plants");
        }
        
        [ContextMenu("Clear Test Results")]
        public void ClearTestResults()
        {
            _lastTestResults = "Test results cleared";
        }
    }
}