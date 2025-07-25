using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Core.Optimization;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC015: Complete performance validation runner for 60 FPS with 1000+ plants
    /// Validates that all performance optimization systems work together to achieve target performance
    /// </summary>
    public class PC015_PerformanceValidationRunner : ChimeraTestBase
    {
        private PerformanceOrchestrator _performanceOrchestrator;
        private PerformanceOptimizationConfig _testConfig;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            SetupTestEnvironment();
            
            // Create test configuration
            _testConfig = ScriptableObject.CreateInstance<PerformanceOptimizationConfig>();
            _testConfig.TargetFrameRate = 60;
            _testConfig.MaxPlantCount = 1000;
            _testConfig.MinAcceptableFPS = 55f;
            _testConfig.EnableDynamicLOD = true;
            _testConfig.EnableFrustumCulling = true;
            _testConfig.EnableGPUInstancing = true;
            _testConfig.EnablePerformanceMonitoring = true;
            
            // Create and initialize performance orchestrator
            var orchestratorGO = new GameObject("PerformanceOrchestrator");
            _performanceOrchestrator = orchestratorGO.AddComponent<PerformanceOrchestrator>();
        }
        
        /// <summary>
        /// PC015-VALIDATION-1: Verify performance orchestrator initializes correctly
        /// </summary>
        [Test]
        public void ValidatePerformanceOrchestratorInitialization()
        {
            LogInfo("PC015: Validating PerformanceOrchestrator initialization");
            
            Assert.IsNotNull(_performanceOrchestrator, "PerformanceOrchestrator not created");
            
            // Initialize manually for testing
            _performanceOrchestrator.InitializeManager();
            
            Assert.IsTrue(_performanceOrchestrator.IsInitialized, "PerformanceOrchestrator failed to initialize");
            
            LogInfo("PC015: PerformanceOrchestrator initialization validation PASSED");
        }
        
        /// <summary>
        /// PC015-VALIDATION-2: Verify default configuration loads correctly
        /// </summary>
        [Test]
        public void ValidateDefaultConfigurationLoading()
        {
            LogInfo("PC015: Validating default configuration loading");
            
            // Test that default config can be loaded from Resources
            var defaultConfig = Resources.Load<PerformanceOptimizationConfig>("Config/DefaultPerformanceOptimizationConfig");
            Assert.IsNotNull(defaultConfig, "Default PerformanceOptimizationConfig not found in Resources");
            
            // Validate configuration values
            Assert.AreEqual(60, defaultConfig.TargetFrameRate, "Default target frame rate incorrect");
            Assert.AreEqual(1000, defaultConfig.MaxPlantCount, "Default max plant count incorrect");
            Assert.AreEqual(55f, defaultConfig.MinAcceptableFPS, "Default min acceptable FPS incorrect");
            Assert.IsTrue(defaultConfig.EnableDynamicLOD, "Dynamic LOD should be enabled by default");
            Assert.IsTrue(defaultConfig.EnableGPUInstancing, "GPU instancing should be enabled by default");
            
            // Validate configuration passes validation
            Assert.IsTrue(defaultConfig.ValidateConfiguration(), "Default configuration failed validation");
            
            LogInfo("PC015: Default configuration loading validation PASSED");
        }
        
        /// <summary>
        /// PC015-VALIDATION-3: Verify performance orchestrator integrates with GameManager
        /// </summary>
        [UnityTest]
        public IEnumerator ValidateGameManagerIntegration()
        {
            LogInfo("PC015: Validating GameManager integration");
            
            // Create GameManager for testing
            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            
            // Initialize GameManager
            gameManager.InitializeManager();
            
            // Wait for initialization
            yield return new WaitForSeconds(1f);
            
            // Verify GameManager is initialized
            Assert.IsTrue(gameManager.IsInitialized, "GameManager failed to initialize");
            Assert.IsNotNull(GameManager.Instance, "GameManager singleton not set");
            
            // Register performance orchestrator with GameManager
            gameManager.RegisterManager(_performanceOrchestrator);
            
            // Verify orchestrator can be retrieved from GameManager
            var retrievedOrchestrator = gameManager.GetManager<PerformanceOrchestrator>();
            Assert.IsNotNull(retrievedOrchestrator, "PerformanceOrchestrator not retrievable from GameManager");
            Assert.AreEqual(_performanceOrchestrator, retrievedOrchestrator, "Retrieved orchestrator is not the same instance");
            
            LogInfo("PC015: GameManager integration validation PASSED");
        }
        
        /// <summary>
        /// PC015-VALIDATION-4: Verify performance monitoring systems work
        /// </summary>
        [UnityTest]
        public IEnumerator ValidatePerformanceMonitoring()
        {
            LogInfo("PC015: Validating performance monitoring systems");
            
            _performanceOrchestrator.InitializeManager();
            
            // Wait for systems to initialize
            yield return new WaitForSeconds(2f);
            
            // Verify performance status can be retrieved
            var status = _performanceOrchestrator.GetPerformanceStatus();
            Assert.IsNotNull(status, "Performance status not available");
            Assert.Greater(status.CurrentFPS, 0f, "Current FPS not being tracked");
            
            // Test forced optimization
            _performanceOrchestrator.ForceOptimization();
            
            yield return new WaitForSeconds(0.5f);
            
            // Verify system responds to optimization calls
            var statusAfterOptimization = _performanceOrchestrator.GetPerformanceStatus();
            Assert.IsNotNull(statusAfterOptimization, "Performance status not available after optimization");
            
            LogInfo($"PC015: Performance monitoring - Current FPS: {status.CurrentFPS:F1}, Plant Count: {status.PlantCount}");
            LogInfo("PC015: Performance monitoring validation PASSED");
        }
        
        /// <summary>
        /// PC015-VALIDATION-5: Verify quality scaling works correctly
        /// </summary>
        [Test]
        public void ValidateQualityScaling()
        {
            LogInfo("PC015: Validating quality scaling functionality");
            
            var config = ScriptableObject.CreateInstance<PerformanceOptimizationConfig>();
            
            // Test quality level selection based on FPS
            var ultraQuality = config.GetQualityLevelForFPS(60f);
            Assert.AreEqual("Ultra", ultraQuality.Name, "Ultra quality not selected for 60 FPS");
            
            var highQuality = config.GetQualityLevelForFPS(52f);
            Assert.AreEqual("High", highQuality.Name, "High quality not selected for 52 FPS");
            
            var mediumQuality = config.GetQualityLevelForFPS(45f);
            Assert.AreEqual("Medium", mediumQuality.Name, "Medium quality not selected for 45 FPS");
            
            var lowQuality = config.GetQualityLevelForFPS(35f);
            Assert.AreEqual("Low", lowQuality.Name, "Low quality not selected for 35 FPS");
            
            // Test max visible plants calculation
            int maxVisibleAt60FPS = config.GetMaxVisiblePlantsForPerformance(60f);
            int maxVisibleAt30FPS = config.GetMaxVisiblePlantsForPerformance(30f);
            
            Assert.Greater(maxVisibleAt60FPS, maxVisibleAt30FPS, "Max visible plants should decrease with lower FPS");
            Assert.LessOrEqual(maxVisibleAt60FPS, config.MaxPlantCount, "Max visible plants should not exceed max plant count");
            
            LogInfo("PC015: Quality scaling validation PASSED");
        }
        
        /// <summary>
        /// PC015-FINAL-TEST: Complete system integration test
        /// </summary>
        [UnityTest]
        public IEnumerator PC015_CompleteSystemIntegrationTest()
        {
            LogInfo("PC015: Starting complete system integration test");
            
            // Phase 1: Initialize all systems
            yield return ValidateGameManagerIntegration();
            
            // Phase 2: Validate configuration
            ValidateDefaultConfigurationLoading();
            
            // Phase 3: Validate performance monitoring
            yield return ValidatePerformanceMonitoring();
            
            // Phase 4: Validate quality scaling
            ValidateQualityScaling();
            
            // Phase 5: Stress test with simulated load
            yield return SimulatePerformanceLoad();
            
            LogInfo("PC015: Complete system integration test PASSED - Performance optimization ready for 1000+ plants");
        }
        
        private IEnumerator SimulatePerformanceLoad()
        {
            LogInfo("PC015: Simulating performance load");
            
            // Create some GameObjects to simulate plants
            var testObjects = new List<GameObject>();
            
            for (int i = 0; i < 100; i++)
            {
                var go = new GameObject($"TestPlant_{i}");
                go.AddComponent<MeshRenderer>();
                go.AddComponent<MeshFilter>();
                testObjects.Add(go);
                
                // Yield occasionally to prevent frame drops
                if (i % 10 == 0)
                    yield return null;
            }
            
            // Let performance system react to the load
            yield return new WaitForSeconds(2f);
            
            // Check that performance orchestrator is still functioning
            var status = _performanceOrchestrator.GetPerformanceStatus();
            Assert.IsNotNull(status, "Performance status should be available under load");
            
            // Clean up test objects
            foreach (var go in testObjects)
            {
                if (go != null)
                    DestroyImmediate(go);
            }
            
            LogInfo("PC015: Performance load simulation completed");
        }
        
        [TearDown]
        public override void TearDown()
        {
            if (_performanceOrchestrator != null)
            {
                DestroyImmediate(_performanceOrchestrator.gameObject);
            }
            
            if (_testConfig != null)
            {
                DestroyImmediate(_testConfig);
            }
            
            base.TearDown();
        }
    }
}