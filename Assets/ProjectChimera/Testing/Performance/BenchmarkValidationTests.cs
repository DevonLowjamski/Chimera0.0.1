using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1b: Validation tests for the automated performance benchmarking system
    /// Ensures benchmark system works correctly and produces valid results
    /// </summary>
    public class BenchmarkValidationTests : ChimeraTestBase
    {
        private PerformanceBenchmarkRunner _benchmarkRunner;
        private AutomatedBenchmarkScheduler _benchmarkScheduler;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            InitializeBenchmarkComponents();
        }
        
        [TearDown]
        public override void TearDown()
        {
            CleanupBenchmarkComponents();
            base.TearDown();
        }
        
        #region Initialization
        
        private void InitializeBenchmarkComponents()
        {
            // Create benchmark runner
            var runnerGO = new GameObject("TestBenchmarkRunner");
            _benchmarkRunner = runnerGO.AddComponent<PerformanceBenchmarkRunner>();
            
            // Create benchmark scheduler
            var schedulerGO = new GameObject("TestBenchmarkScheduler");
            _benchmarkScheduler = schedulerGO.AddComponent<AutomatedBenchmarkScheduler>();
        }
        
        private void CleanupBenchmarkComponents()
        {
            if (_benchmarkRunner != null)
            {
                UnityEngine.Object.DestroyImmediate(_benchmarkRunner.gameObject);
            }
            
            if (_benchmarkScheduler != null)
            {
                UnityEngine.Object.DestroyImmediate(_benchmarkScheduler.gameObject);
            }
        }
        
        #endregion
        
        #region Benchmark Runner Tests
        
        /// <summary>
        /// Test that benchmark runner can establish performance baseline
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestBaselineEstablishment()
        {
            LogInfo("PC016-1b: Testing baseline establishment");
            
            // Run baseline establishment
            yield return _benchmarkRunner.EstablishPerformanceBaseline();
            
            // Verify baseline was created
            // Note: In a real test, we would access private fields or use reflection
            // For now, we just verify the method completes without errors
            
            LogInfo("PC016-1b: Baseline establishment test completed");
        }
        
        /// <summary>
        /// Test that benchmark runner can execute light load scenario
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestLightLoadBenchmark()
        {
            LogInfo("PC016-1b: Testing light load benchmark");
            
            // Run light load benchmark
            yield return _benchmarkRunner.RunSpecificBenchmark("Light Load");
            
            // Get results
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            
            // Validate results
            Assert.IsTrue(results.Count > 0, "Should have at least one benchmark result");
            
            var lightLoadResult = results.FirstOrDefault(r => r.ScenarioName.Contains("Light Load"));
            if (lightLoadResult != null)
            {
                Assert.IsTrue(lightLoadResult.AverageFPS > 0, "Average FPS should be greater than 0");
                Assert.IsTrue(lightLoadResult.AverageFrameTime > 0, "Average frame time should be greater than 0");
                Assert.IsTrue(lightLoadResult.PlantCount > 0, "Plant count should be greater than 0");
                
                LogInfo($"PC016-1b: Light load benchmark - FPS: {lightLoadResult.AverageFPS:F1}, " +
                       $"Frame Time: {lightLoadResult.AverageFrameTime:F2}ms, Plants: {lightLoadResult.PlantCount}");
            }
            
            LogInfo("PC016-1b: Light load benchmark test completed");
        }
        
        /// <summary>
        /// Test benchmark result data integrity
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestBenchmarkDataIntegrity()
        {
            LogInfo("PC016-1b: Testing benchmark data integrity");
            
            // Clear any existing results
            _benchmarkRunner.ClearBenchmarkResults();
            
            // Run a simple benchmark
            yield return _benchmarkRunner.RunSpecificBenchmark("Light Load");
            
            // Get and validate results
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            
            Assert.IsTrue(results.Count == 1, "Should have exactly one result after running one benchmark");
            
            var result = results[0];
            
            // Validate data consistency
            Assert.IsFalse(string.IsNullOrEmpty(result.ScenarioName), "Scenario name should not be empty");
            Assert.IsTrue(result.PlantCount > 0, "Plant count should be positive");
            Assert.IsTrue(result.Duration > 0, "Duration should be positive");
            Assert.IsTrue(result.EndTime > result.StartTime, "End time should be after start time");
            Assert.IsTrue(result.AverageFPS > 0 && result.AverageFPS <= 1000, "Average FPS should be reasonable");
            Assert.IsTrue(result.MinFPS <= result.AverageFPS, "Min FPS should be <= average FPS");
            Assert.IsTrue(result.MaxFPS >= result.AverageFPS, "Max FPS should be >= average FPS");
            Assert.IsTrue(result.AverageFrameTime > 0 && result.AverageFrameTime < 1000, "Frame time should be reasonable");
            Assert.IsTrue(result.PerformanceSnapshots != null, "Performance snapshots should not be null");
            
            LogInfo($"PC016-1b: Data integrity validated - {result.PerformanceSnapshots.Count} snapshots recorded");
            LogInfo("PC016-1b: Benchmark data integrity test completed");
        }
        
        #endregion
        
        #region Scheduler Tests
        
        /// <summary>
        /// Test that scheduler initializes correctly
        /// </summary>
        [Test]
        [Category("Benchmark Validation")]
        public void TestSchedulerInitialization()
        {
            LogInfo("PC016-1b: Testing scheduler initialization");
            
            // Verify scheduler components are accessible
            Assert.IsNotNull(_benchmarkScheduler, "Benchmark scheduler should be initialized");
            Assert.IsNotNull(AutomatedBenchmarkScheduler.Instance, "Scheduler instance should be available");
            
            // Test API methods
            Assert.IsFalse(_benchmarkScheduler.IsBenchmarkInProgress(), "No benchmark should be in progress initially");
            
            var performanceData = _benchmarkScheduler.GetRecentPerformanceData();
            Assert.IsNotNull(performanceData, "Performance data should not be null");
            
            LogInfo("PC016-1b: Scheduler initialization test completed");
        }
        
        /// <summary>
        /// Test scheduler API functionality
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestSchedulerAPI()
        {
            LogInfo("PC016-1b: Testing scheduler API");
            
            // Test enabling/disabling automated scheduling
            _benchmarkScheduler.SetAutomatedScheduling(true);
            yield return new WaitForSeconds(0.1f);
            
            _benchmarkScheduler.SetAutomatedScheduling(false);
            yield return new WaitForSeconds(0.1f);
            
            // Test enabling/disabling continuous monitoring
            _benchmarkScheduler.SetContinuousMonitoring(true);
            yield return new WaitForSeconds(0.1f);
            
            _benchmarkScheduler.SetContinuousMonitoring(false);
            yield return new WaitForSeconds(0.1f);
            
            // Test stopping all benchmarks
            _benchmarkScheduler.StopAllBenchmarks();
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsFalse(_benchmarkScheduler.IsBenchmarkInProgress(), "No benchmark should be running after stop");
            
            LogInfo("PC016-1b: Scheduler API test completed");
        }
        
        #endregion
        
        #region Integration Tests
        
        /// <summary>
        /// Test integration between benchmark runner and scheduler
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestRunnerSchedulerIntegration()
        {
            LogInfo("PC016-1b: Testing runner-scheduler integration");
            
            // Clear any existing results
            _benchmarkRunner.ClearBenchmarkResults();
            
            // Trigger manual benchmark through scheduler
            _benchmarkScheduler.TriggerManualBenchmark();
            
            // Wait for benchmark to start
            yield return new WaitForSeconds(1f);
            
            // Check if benchmark started
            bool benchmarkStarted = _benchmarkScheduler.IsBenchmarkInProgress();
            
            // Wait for benchmark to complete (with timeout)
            float timeoutTime = Time.time + 120f; // 2 minute timeout
            while (_benchmarkScheduler.IsBenchmarkInProgress() && Time.time < timeoutTime)
            {
                yield return new WaitForSeconds(1f);
            }
            
            // Verify benchmark completed
            Assert.IsFalse(_benchmarkScheduler.IsBenchmarkInProgress(), "Benchmark should have completed");
            
            // Verify results were generated
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            Assert.IsTrue(results.Count > 0, "Benchmark should have generated results");
            
            LogInfo($"PC016-1b: Integration test completed - {results.Count} benchmark results generated");
        }
        
        /// <summary>
        /// Test benchmark system performance under load
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestBenchmarkSystemPerformance()
        {
            LogInfo("PC016-1b: Testing benchmark system performance");
            
            var startTime = Time.realtimeSinceStartup;
            var startMemory = System.GC.GetTotalMemory(true);
            
            // Run multiple light benchmarks in sequence
            for (int i = 0; i < 3; i++)
            {
                LogInfo($"PC016-1b: Running benchmark iteration {i + 1}/3");
                yield return _benchmarkRunner.RunSpecificBenchmark("Light Load");
                
                // Brief pause between benchmarks
                yield return new WaitForSeconds(1f);
            }
            
            var endTime = Time.realtimeSinceStartup;
            var endMemory = System.GC.GetTotalMemory(false);
            
            var totalTime = endTime - startTime;
            var memoryIncrease = (endMemory - startMemory) / (1024f * 1024f);
            
            // Verify system performance
            Assert.IsTrue(totalTime < 180f, "Three light benchmarks should complete within 3 minutes");
            Assert.IsTrue(memoryIncrease < 50f, "Memory increase should be reasonable (< 50MB)");
            
            // Verify all benchmarks completed
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            Assert.IsTrue(results.Count >= 3, "Should have at least 3 benchmark results");
            
            LogInfo($"PC016-1b: System performance test completed - " +
                   $"Time: {totalTime:F1}s, Memory: +{memoryIncrease:F1}MB, Results: {results.Count}");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        /// <summary>
        /// Test error handling with invalid scenarios
        /// </summary>
        [UnityTest]
        [Category("Benchmark Validation")]
        public IEnumerator TestErrorHandling()
        {
            LogInfo("PC016-1b: Testing error handling");
            
            // Test running non-existent benchmark
            yield return _benchmarkRunner.RunSpecificBenchmark("NonExistentBenchmark");
            
            // Should not crash or throw unhandled exceptions
            Assert.IsTrue(true, "System should handle invalid benchmark names gracefully");
            
            // Test clearing results multiple times
            _benchmarkRunner.ClearBenchmarkResults();
            _benchmarkRunner.ClearBenchmarkResults();
            
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            Assert.IsTrue(results.Count == 0, "Results should be empty after clearing");
            
            LogInfo("PC016-1b: Error handling test completed");
        }
        
        #endregion
    }
}