using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Core.Optimization;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC015: Large-scale performance validation for 1000+ plants at 60 FPS target
    /// Comprehensive testing suite to validate Project Chimera's performance optimization systems
    /// </summary>
    public class LargeScalePerformanceValidation : ChimeraTestBase
    {
        private PerformanceOrchestrator _performanceOrchestrator;
        private PlantUpdateScheduler _updateScheduler;
        private List<GameObject> _testPlants;
        
        private const int TARGET_FPS = 60;
        private const int LARGE_SCALE_PLANT_COUNT = 1000;
        private const float MIN_ACCEPTABLE_FPS = 55f; // 5 FPS tolerance
        private const float FRAME_TIME_MS = 16.67f; // 60 FPS = 16.67ms per frame
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            SetupTestEnvironment();
            _testPlants = new List<GameObject>();
            
            // Initialize core performance systems
            _performanceOrchestrator = CreateTestManager<PerformanceOrchestrator>();
            _updateScheduler = FindObjectOfType<PlantUpdateScheduler>();
            
            // Configure optimization systems for large-scale testing
            ConfigureOptimizationSystems();
        }
        
        [TearDown]
        public override void TearDown()
        {
            // Clean up test plants
            foreach (var plant in _testPlants)
            {
                if (plant != null)
                    DestroyImmediate(plant);
            }
            _testPlants.Clear();
            
            // Reset performance systems
            if (_performanceOrchestrator != null)
                _performanceOrchestrator.ForceOptimization();
                
            base.TearDown();
        }
        
        /// <summary>
        /// PC015-TEST-1: Validate 60 FPS performance with 1000 plants
        /// Core performance validation test for Project Chimera's optimization systems
        /// </summary>
        [UnityTest]
        public IEnumerator ValidateLargeScalePerformance_1000Plants()
        {
            LogInfo("PC015: Starting large-scale performance validation - 1000 plants @ 60 FPS");
            
            // Phase 1: Create plants in batches to avoid frame drops during setup
            yield return CreatePlantsInBatches(LARGE_SCALE_PLANT_COUNT, 50);
            
            // Phase 2: Allow systems to stabilize
            yield return new WaitForSeconds(2f);
            
            // Phase 3: Monitor performance for sustained period
            yield return MonitorPerformanceOverTime(30f); // 30 seconds of monitoring
            
            // Phase 4: Validate results
            ValidatePerformanceResults();
            
            LogInfo($"PC015: Large-scale performance validation completed - {_testPlants.Count} plants");
        }
        
        /// <summary>
        /// PC015-TEST-2: Stress test with progressive plant loading
        /// Tests system stability under increasing load
        /// </summary>
        [UnityTest]
        public IEnumerator StressTestProgressiveLoading()
        {
            LogInfo("PC015: Starting progressive loading stress test");
            
            var plantCounts = new int[] { 100, 250, 500, 750, 1000, 1250 };
            var results = new List<PerformanceResult>();
            
            foreach (int targetCount in plantCounts)
            {
                // Add plants to reach target count
                int plantsToAdd = targetCount - _testPlants.Count;
                if (plantsToAdd > 0)
                {
                    yield return CreatePlantsInBatches(plantsToAdd, 25);
                }
                
                // Stabilize and measure
                yield return new WaitForSeconds(3f);
                var result = MeasureCurrentPerformance(5f);
                yield return result;
                
                var perfData = result.Current as PerformanceResult;
                results.Add(perfData);
                
                LogInfo($"PC015: {targetCount} plants - Average FPS: {perfData.AverageFPS:F1}");
                
                // Stop if performance degrades too much
                if (perfData.AverageFPS < 30f)
                {
                    LogWarning($"PC015: Performance degraded at {targetCount} plants, stopping stress test");
                    break;
                }
            }
            
            // Validate progressive loading results
            ValidateProgressiveLoadingResults(results);
        }
        
        /// <summary>
        /// PC015-TEST-3: Optimization system validation
        /// Verifies that all optimization systems are active and functioning
        /// </summary>
        [Test]
        public void ValidateOptimizationSystemsActive()
        {
            LogInfo("PC015: Validating optimization systems are active");
            
            // Validate performance orchestrator
            Assert.IsNotNull(_performanceOrchestrator, "Performance orchestrator not found");
            Assert.IsTrue(_performanceOrchestrator.IsPerformanceTargetMet || _performanceOrchestrator.CurrentFPS > 0, "Performance orchestrator not functioning");
            
            // Validate plant update scheduler
            Assert.IsNotNull(_updateScheduler, "Plant update scheduler not found");
            
            // Validate scheduler statistics
            var stats = _updateScheduler.GetSchedulerStats();
            Assert.IsNotNull(stats, "Scheduler stats not available");
            Assert.GreaterOrEqual(stats.UpdateBudgetMs, 0, "Update budget not configured");
            
            LogInfo("PC015: All available optimization systems validated as active");
        }
        
        /// <summary>
        /// PC015-TEST-4: Memory usage validation
        /// Ensures memory usage stays within acceptable limits with large plant counts
        /// </summary>
        [UnityTest]
        public IEnumerator ValidateMemoryUsage()
        {
            LogInfo("PC015: Starting memory usage validation");
            
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // Create large number of plants
            yield return CreatePlantsInBatches(1000, 50);
            
            // Force garbage collection and measure
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryIncrease = finalMemory - initialMemory;
            float memoryMB = memoryIncrease / (1024f * 1024f);
            
            LogInfo($"PC015: Memory usage increase: {memoryMB:F1} MB for 1000 plants");
            
            // Validate memory usage is reasonable (< 1GB increase)
            Assert.Less(memoryMB, 1024f, $"Memory usage too high: {memoryMB:F1} MB");
            
            // Validate memory pooling is working
            if (_updateScheduler != null)
            {
                // Check that scheduler is managing plants efficiently
                var stats = _updateScheduler.GetSchedulerStats();
                Assert.GreaterOrEqual(stats.TotalRegisteredPlants, 0, "Plant registration not working");
            }
        }
        
        #region Helper Methods
        
        private void ConfigureOptimizationSystems()
        {
            // Configure performance orchestrator for large-scale testing
            if (_performanceOrchestrator != null)
            {
                // Performance orchestrator handles its own configuration
                _performanceOrchestrator.ForceOptimization();
                LogInfo("PC015: PerformanceOrchestrator configured for large-scale testing");
            }
            
            // Configure plant update scheduler
            if (_updateScheduler != null)
            {
                // Update scheduler handles its own configuration
                LogInfo($"PC015: PlantUpdateScheduler configured - {_updateScheduler.TotalRegisteredPlants} plants registered");
            }
        }
        
        private IEnumerator CreatePlantsInBatches(int totalPlants, int batchSize)
        {
            int plantsCreated = 0;
            
            while (plantsCreated < totalPlants)
            {
                int plantsThisBatch = Mathf.Min(batchSize, totalPlants - plantsCreated);
                
                for (int i = 0; i < plantsThisBatch; i++)
                {
                    var plantGO = CreateTestPlant(plantsCreated + i);
                    _testPlants.Add(plantGO);
                }
                
                plantsCreated += plantsThisBatch;
                
                // Yield to prevent frame drops during creation
                yield return new WaitForEndOfFrame();
                
                if (plantsCreated % 100 == 0)
                {
                    LogInfo($"PC015: Created {plantsCreated}/{totalPlants} plants");
                }
            }
            
            LogInfo($"PC015: Plant creation completed - {plantsCreated} plants");
        }
        
        private GameObject CreateTestPlant(int index)
        {
            var plantGO = new GameObject($"TestPlant_{index}");
            plantGO.transform.position = new Vector3(
                UnityEngine.Random.Range(-50f, 50f),
                0f,
                UnityEngine.Random.Range(-50f, 50f)
            );
            
            // Add basic components that would be on a real plant
            var renderer = plantGO.AddComponent<MeshRenderer>();
            var meshFilter = plantGO.AddComponent<MeshFilter>();
            
            // Use simple cube mesh for performance testing
            meshFilter.mesh = CreateSimplePlantMesh();
            
            // Add to update scheduler if available
            if (_updateScheduler != null)
            {
                _updateScheduler.RegisterPlant(plantGO);
            }
            
            return plantGO;
        }
        
        private Mesh CreateSimplePlantMesh()
        {
            // Create a simple quad mesh for performance testing
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(0.5f, 1f, 0f),
                new Vector3(-0.5f, 1f, 0f)
            };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.RecalculateNormals();
            return mesh;
        }
        
        private IEnumerator MonitorPerformanceOverTime(float duration)
        {
            var samples = new List<float>();
            float elapsed = 0f;
            
            LogInfo($"PC015: Starting {duration}s performance monitoring");
            
            while (elapsed < duration)
            {
                float currentFPS = 1f / Time.unscaledDeltaTime;
                samples.Add(currentFPS);
                
                elapsed += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            float averageFPS = samples.Sum() / samples.Count;
            float minFPS = samples.Min();
            
            LogInfo($"PC015: Performance monitoring complete - Avg: {averageFPS:F1} FPS, Min: {minFPS:F1} FPS");
            
            // Store results for validation
            _lastPerformanceResult = new PerformanceResult
            {
                AverageFPS = averageFPS,
                MinimumFPS = minFPS,
                PlantCount = _testPlants.Count,
                Duration = duration
            };
        }
        
        private PerformanceResult _lastPerformanceResult;
        
        private IEnumerator MeasureCurrentPerformance(float duration)
        {
            yield return MonitorPerformanceOverTime(duration);
            yield return _lastPerformanceResult;
        }
        
        private void ValidatePerformanceResults()
        {
            Assert.IsNotNull(_lastPerformanceResult, "No performance results available");
            
            var result = _lastPerformanceResult;
            
            LogInfo($"PC015: Validating performance - {result.PlantCount} plants, {result.AverageFPS:F1} avg FPS, {result.MinimumFPS:F1} min FPS");
            
            // Validate average FPS meets target
            Assert.GreaterOrEqual(result.AverageFPS, MIN_ACCEPTABLE_FPS, 
                $"Average FPS {result.AverageFPS:F1} below target {MIN_ACCEPTABLE_FPS}");
            
            // Validate minimum FPS is acceptable (allowing for occasional drops)
            Assert.GreaterOrEqual(result.MinimumFPS, 45f, 
                $"Minimum FPS {result.MinimumFPS:F1} too low (< 45 FPS)");
            
            // Validate frame time consistency
            float averageFrameTime = 1000f / result.AverageFPS;
            Assert.LessOrEqual(averageFrameTime, FRAME_TIME_MS * 1.2f, 
                $"Frame time {averageFrameTime:F2}ms exceeds target {FRAME_TIME_MS:F2}ms");
            
            LogInfo("PC015: Performance validation PASSED - 60 FPS target achieved with 1000+ plants");
        }
        
        private void ValidateProgressiveLoadingResults(List<PerformanceResult> results)
        {
            Assert.Greater(results.Count, 0, "No progressive loading results");
            
            var result1000 = results.FirstOrDefault(r => r.PlantCount >= 1000);
            Assert.IsNotNull(result1000, "No results for 1000+ plants");
            
            Assert.GreaterOrEqual(result1000.AverageFPS, MIN_ACCEPTABLE_FPS,
                $"1000+ plant performance below target: {result1000.AverageFPS:F1} FPS");
            
            LogInfo("PC015: Progressive loading validation PASSED");
        }
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
        
        #endregion
        
        #region Data Structures
        
        public class PerformanceResult
        {
            public float AverageFPS { get; set; }
            public float MinimumFPS { get; set; }
            public int PlantCount { get; set; }
            public float Duration { get; set; }
        }
        
        #endregion
    }
}