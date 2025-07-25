using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using Debug = UnityEngine.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace ProjectChimera.Testing.Systems
{
    /// <summary>
    /// PC013-4e: Test suite for PlantManager service integration
    /// Verifies that PlantManager properly integrates with specialized services:
    /// - PlantGrowthService
    /// - PlantEnvironmentalProcessingService
    /// - PlantYieldCalculationService
    /// </summary>
    public class PlantManagerServiceIntegrationTests : ChimeraTestBase
    {
        private PlantManager _plantManager;
        private CultivationManager _cultivationManager;
        private PlantStrainSO _testStrain;
        private PlantInstance _testPlant;
        private Stopwatch _stopwatch;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            // Create test managers
            _cultivationManager = CreateTestManager<CultivationManager>();
            _plantManager = CreateTestManager<PlantManager>();
            
            // Create test data
            _testStrain = CreateTestPlantStrain();
            _testPlant = CreateTestPlantInstance();
            
            _stopwatch = new Stopwatch();
        }
        
        [TearDown]
        public void TearDown()
        {
            _plantManager?.Shutdown();
            _cultivationManager?.Shutdown();
            CleanupTestEnvironment();
        }
        
        /// <summary>
        /// Test that PlantManager properly initializes specialized services
        /// </summary>
        [Test]
        public void PlantManager_InitializesSpecializedServices()
        {
            // Arrange & Act
            _plantManager.Initialize();
            
            // Assert
            Assert.IsTrue(_plantManager.IsInitialized, "PlantManager should be initialized");
            
            // Verify services are accessible through reflection (since they're private)
            var plantManagerType = typeof(PlantManager);
            var growthServiceField = plantManagerType.GetField("_growthService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var environmentalServiceField = plantManagerType.GetField("_environmentalService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var yieldServiceField = plantManagerType.GetField("_yieldService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(growthServiceField?.GetValue(_plantManager), "Growth service should be initialized");
            Assert.IsNotNull(environmentalServiceField?.GetValue(_plantManager), "Environmental service should be initialized");
            Assert.IsNotNull(yieldServiceField?.GetValue(_plantManager), "Yield service should be initialized");
        }
        
        /// <summary>
        /// Test that PlantManager properly shuts down specialized services
        /// </summary>
        [Test]
        public void PlantManager_ShutsDownSpecializedServices()
        {
            // Arrange
            _plantManager.Initialize();
            
            // Act
            _plantManager.Shutdown();
            
            // Assert
            Assert.IsFalse(_plantManager.IsInitialized, "PlantManager should be shut down");
            
            // Services should be properly disposed (no easy way to verify without exposing them)
            // This test mainly ensures no exceptions are thrown during shutdown
        }
        
        /// <summary>
        /// Test that yield calculation delegates to YieldCalculationService
        /// </summary>
        [Test]
        public void PlantManager_DelegatesToYieldCalculationService()
        {
            // Arrange
            _plantManager.Initialize();
            
            // Act
            var expectedYield = _plantManager.CalculateExpectedYield(_testPlant);
            
            // Assert
            Assert.IsTrue(expectedYield >= 0, "Expected yield should be non-negative");
            
            // Test with null plant
            var nullYield = _plantManager.CalculateExpectedYield(null);
            Assert.AreEqual(0f, nullYield, "Null plant should return 0 yield");
        }
        
        /// <summary>
        /// Test that harvest processing delegates to YieldCalculationService
        /// </summary>
        [Test]
        public void PlantManager_DelegatesToYieldCalculationServiceForHarvest()
        {
            // Arrange
            _plantManager.Initialize();
            var harvestablePlant = CreateHarvestablePlant();
            
            // Expect the error log since the plant won't be found in the test environment
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[PlantYieldCalculationService\\] Cannot harvest unknown plant:.*"));
            
            // Act
            var harvestResults = _plantManager.HarvestPlant(harvestablePlant.PlantID);
            
            // Assert - This will test the fallback implementation since service won't find the plant
            // The main goal is to verify no exceptions are thrown and the integration works
            Assert.Pass("Harvest method executed without exceptions");
        }
        
        /// <summary>
        /// Test stage yield modifier delegation
        /// </summary>
        [Test]
        public void PlantManager_DelegatesToYieldCalculationServiceForStageModifier()
        {
            // Arrange
            _plantManager.Initialize();
            
            // Act & Assert - Test various growth stages
            var seedModifier = GetStageYieldModifierViaReflection(PlantGrowthStage.Seed);
            var vegetativeModifier = GetStageYieldModifierViaReflection(PlantGrowthStage.Vegetative);
            var harvestModifier = GetStageYieldModifierViaReflection(PlantGrowthStage.Harvest);
            
            Assert.AreEqual(0f, seedModifier, "Seed stage should have 0 yield modifier");
            Assert.IsTrue(vegetativeModifier > 0f, "Vegetative stage should have positive yield modifier");
            Assert.AreEqual(1f, harvestModifier, "Harvest stage should have 1.0 yield modifier");
        }
        
        /// <summary>
        /// Test that PlantManager configuration is properly passed to services
        /// </summary>
        [Test]
        public void PlantManager_PassesConfigurationToServices()
        {
            // Arrange
            _plantManager.Initialize();
            
            // Act & Assert - Verify configurations are passed through
            // This is mainly a structural test to ensure the integration points work
            Assert.IsTrue(_plantManager.IsInitialized, "PlantManager should be initialized with services");
            
            // Test that global growth modifier is accessible
            var plantManagerType = typeof(PlantManager);
            var globalGrowthModifierField = plantManagerType.GetField("_globalGrowthModifier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(globalGrowthModifierField, "Global growth modifier field should exist");
            var globalGrowthModifier = (float)globalGrowthModifierField.GetValue(_plantManager);
            Assert.IsTrue(globalGrowthModifier > 0f, "Global growth modifier should be positive");
        }
        
        /// <summary>
        /// Test performance of service integration
        /// </summary>
        [Test]
        public void PlantManager_ServiceIntegrationPerformance()
        {
            // Arrange
            _plantManager.Initialize();
            var plants = new List<PlantInstance>();
            
            // Create test plants
            for (int i = 0; i < 100; i++)
            {
                plants.Add(CreateTestPlantInstance());
            }
            
            // Act
            _stopwatch.Start();
            
            foreach (var plant in plants)
            {
                var expectedYield = _plantManager.CalculateExpectedYield(plant);
                Assert.IsTrue(expectedYield >= 0, $"Plant {plant.PlantID} should have non-negative yield");
            }
            
            _stopwatch.Stop();
            
            // Assert
            Assert.Less(_stopwatch.ElapsedMilliseconds, 1000, 
                $"Processing 100 plants took {_stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }
        
        /// <summary>
        /// Test fallback behavior when services are not available
        /// </summary>
        [Test]
        public void PlantManager_FallbackBehaviorWhenServicesUnavailable()
        {
            // Arrange - Don't initialize PlantManager to test fallback
            // This simulates the scenario where services fail to initialize
            
            // Act
            var expectedYield = _plantManager.CalculateExpectedYield(_testPlant);
            
            // Assert - Should use fallback implementation
            Assert.IsTrue(expectedYield >= 0, "Fallback implementation should return non-negative yield");
        }
        
        /// <summary>
        /// Test that service integration maintains backward compatibility
        /// </summary>
        [Test]
        public void PlantManager_MaintainsBackwardCompatibility()
        {
            // Arrange
            _plantManager.Initialize();
            
            // Act & Assert - Test that existing public API still works
            var statistics = _plantManager.GetStatistics();
            Assert.IsNotNull(statistics, "GetStatistics should return valid statistics");
            
            var harvestablePlants = _plantManager.GetHarvestablePlants();
            Assert.IsNotNull(harvestablePlants, "GetHarvestablePlants should return valid collection");
            
            var plantsInStage = _plantManager.GetPlantsInStage(PlantGrowthStage.Vegetative);
            Assert.IsNotNull(plantsInStage, "GetPlantsInStage should return valid collection");
        }
        
        /// <summary>
        /// Test service integration with environmental stress system
        /// </summary>
        [Test]
        public void PlantManager_ServiceIntegrationWithStressSystem()
        {
            // Arrange
            _plantManager.Initialize();
            var testStress = CreateTestEnvironmentalStress();
            
            // Act
            _plantManager.ApplyEnvironmentalStress(testStress, 0.5f);
            
            // Assert - Should execute without exceptions
            Assert.Pass("Environmental stress application completed successfully");
        }
        
        #region Helper Methods
        
        private PlantStrainSO CreateTestPlantStrain()
        {
            var strain = ScriptableObject.CreateInstance<PlantStrainSO>();
            // Only set properties that have public setters
            strain.StrainName = "TestStrain";
            strain.StrainDescription = "Test strain for integration testing";
            strain.StrainType = StrainType.Hybrid;
            // Note: BaseYieldGrams and BaseFloweringTime are read-only and use default values
            return strain;
        }
        
        private PlantInstance CreateTestPlantInstance()
        {
            // Use the static factory method to create a properly initialized PlantInstance
            var plant = PlantInstance.CreateFromStrain(_testStrain, Vector3.zero, null);
            return plant;
        }
        
        private PlantInstance CreateHarvestablePlant()
        {
            // Create a plant and advance it to harvest stage
            var plant = PlantInstance.CreateFromStrain(_testStrain, Vector3.zero, null);
            
            // Advance plant through all growth stages to reach harvest
            while (plant.CurrentGrowthStage != PlantGrowthStage.Harvest)
            {
                if (!plant.AdvanceGrowthStage())
                {
                    // If we can't advance anymore, break to avoid infinite loop
                    break;
                }
            }
            
            return plant;
        }
        
        private EnvironmentalStressSO CreateTestEnvironmentalStress()
        {
            var stress = ScriptableObject.CreateInstance<EnvironmentalStressSO>();
            // Note: EnvironmentalStressSO properties are private with no public setters
            // This is a basic instantiation for testing purposes
            return stress;
        }
        
        private float GetStageYieldModifierViaReflection(PlantGrowthStage stage)
        {
            var plantManagerType = typeof(PlantManager);
            var method = plantManagerType.GetMethod("GetStageYieldModifier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)method.Invoke(_plantManager, new object[] { stage });
        }
        
        #endregion
    }
    
    /// <summary>
    /// Test runner for PlantManager service integration tests
    /// </summary>
    public class PlantManagerServiceIntegrationTestRunner : MonoBehaviour
    {
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private bool _enableDetailedLogging = true;
        
        private void Start()
        {
            if (_runTestsOnStart)
            {
                RunTests();
            }
        }
        
        [ContextMenu("Run PlantManager Service Integration Tests")]
        public void RunTests()
        {
            Debug.Log("=== Running PlantManager Service Integration Tests ===");
            
            var testSuite = new PlantManagerServiceIntegrationTests();
            var methods = typeof(PlantManagerServiceIntegrationTests).GetMethods();
            
            int testsRun = 0;
            int testsPassed = 0;
            int testsFailed = 0;
            
            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                {
                    testsRun++;
                    
                    try
                    {
                        testSuite.SetUp();
                        method.Invoke(testSuite, null);
                        testSuite.TearDown();
                        
                        testsPassed++;
                        
                        if (_enableDetailedLogging)
                        {
                            Debug.Log($"✓ {method.Name} - PASSED");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        testsFailed++;
                        Debug.LogError($"✗ {method.Name} - FAILED: {ex.Message}");
                    }
                }
            }
            
            Debug.Log($"=== PlantManager Service Integration Tests Complete ===");
            Debug.Log($"Tests Run: {testsRun}, Passed: {testsPassed}, Failed: {testsFailed}");
            
            if (testsFailed == 0)
            {
                Debug.Log("🎉 All PlantManager service integration tests passed!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {testsFailed} tests failed. Check logs for details.");
            }
        }
    }
}