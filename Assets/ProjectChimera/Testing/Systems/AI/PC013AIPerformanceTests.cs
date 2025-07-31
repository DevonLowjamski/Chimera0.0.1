using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.AI;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Testing.Systems.AI
{
    /// <summary>
    /// PC013: Performance test suite for AI services
    /// Tests memory usage, execution time, and scalability of AI services
    /// </summary>
    public class PC013AIPerformanceTests : ChimeraTestBase
    {
        private AIAnalysisService _analysisService;
        private AIRecommendationService _recommendationService;
        private AIServicesIntegration _integrationService;
        
        // Performance thresholds
        private const float MAX_ANALYSIS_TIME_MS = 100f;
        private const float MAX_RECOMMENDATION_TIME_MS = 50f;
        private const int MAX_MEMORY_INCREASE_MB = 10;
        private const int STRESS_TEST_ITERATIONS = 100;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            _analysisService = CreateTestManager<AIAnalysisService>();
            _recommendationService = CreateTestManager<AIRecommendationService>();
            _integrationService = CreateTestManager<AIServicesIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            _integrationService?.Shutdown();
            _recommendationService?.Shutdown();
            _analysisService?.Shutdown();
            
            CleanupTestEnvironment();
            
            // Force garbage collection after tests
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        #region Performance Tests
        
        [Test]
        public void AIAnalysisService_CultivationAnalysis_PerformanceTest()
        {
            // Arrange
            _analysisService.Initialize();
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var task = _analysisService.AnalyzeCultivationDataAsync();
            Task.WaitAll(task);
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024); // Convert to MB
            
            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < MAX_ANALYSIS_TIME_MS, 
                $"Cultivation analysis took {stopwatch.ElapsedMilliseconds}ms, should be under {MAX_ANALYSIS_TIME_MS}ms");
            Assert.IsTrue(memoryIncrease < MAX_MEMORY_INCREASE_MB, 
                $"Memory increase was {memoryIncrease}MB, should be under {MAX_MEMORY_INCREASE_MB}MB");
            
            UnityEngine.Debug.Log($"[Performance] Cultivation Analysis: {stopwatch.ElapsedMilliseconds}ms, Memory: +{memoryIncrease}MB");
        }
        
        [Test]
        public void AIAnalysisService_EnvironmentalAnalysis_PerformanceTest()
        {
            // Arrange
            _analysisService.Initialize();
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var task = _analysisService.AnalyzeEnvironmentalDataAsync();
            Task.WaitAll(task);
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024);
            
            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < MAX_ANALYSIS_TIME_MS, 
                $"Environmental analysis took {stopwatch.ElapsedMilliseconds}ms, should be under {MAX_ANALYSIS_TIME_MS}ms");
            Assert.IsTrue(memoryIncrease < MAX_MEMORY_INCREASE_MB, 
                $"Memory increase was {memoryIncrease}MB, should be under {MAX_MEMORY_INCREASE_MB}MB");
            
            UnityEngine.Debug.Log($"[Performance] Environmental Analysis: {stopwatch.ElapsedMilliseconds}ms, Memory: +{memoryIncrease}MB");
        }
        
        [Test]
        public void AIRecommendationService_GenerateRecommendations_PerformanceTest()
        {
            // Arrange
            _recommendationService.Initialize();
            var testAnalysis = CreateLargeTestAnalysis();
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var task = _recommendationService.GenerateRecommendationsAsync(testAnalysis);
            Task.WaitAll(task);
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024);
            
            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < MAX_RECOMMENDATION_TIME_MS, 
                $"Recommendation generation took {stopwatch.ElapsedMilliseconds}ms, should be under {MAX_RECOMMENDATION_TIME_MS}ms");
            Assert.IsTrue(memoryIncrease < MAX_MEMORY_INCREASE_MB, 
                $"Memory increase was {memoryIncrease}MB, should be under {MAX_MEMORY_INCREASE_MB}MB");
            
            UnityEngine.Debug.Log($"[Performance] Recommendation Generation: {stopwatch.ElapsedMilliseconds}ms, Memory: +{memoryIncrease}MB");
        }
        
        #endregion
        
        #region Stress Tests
        
        [Test]
        public void AIAnalysisService_StressTest_MultipleAnalyses()
        {
            // Arrange
            _analysisService.Initialize();
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(true);
            var tasks = new List<Task>();
            
            // Act - Run multiple analyses concurrently
            for (int i = 0; i < STRESS_TEST_ITERATIONS; i++)
            {
                tasks.Add(_analysisService.AnalyzeCultivationDataAsync());
                
                // Stagger the starts slightly to avoid overwhelming the system
                if (i % 10 == 0)
                {
                    Task.Delay(1).Wait();
                }
            }
            
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024);
            
            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < STRESS_TEST_ITERATIONS * MAX_ANALYSIS_TIME_MS, 
                $"Stress test took {stopwatch.ElapsedMilliseconds}ms for {STRESS_TEST_ITERATIONS} analyses");
            Assert.IsTrue(memoryIncrease < MAX_MEMORY_INCREASE_MB * 5, // Allow more memory for stress test
                $"Memory increase was {memoryIncrease}MB during stress test");
            
            UnityEngine.Debug.Log($"[Stress Test] {STRESS_TEST_ITERATIONS} Analyses: {stopwatch.ElapsedMilliseconds}ms, Memory: +{memoryIncrease}MB");
        }
        
        [UnityTest]
        public IEnumerator AIServicesIntegration_StressTest_ContinuousOperation()
        {
            // Arrange
            _integrationService.Initialize();
            yield return new WaitForSeconds(1f);
            
            var initialMemory = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            var operationCount = 0;
            
            // Act - Run continuous operations for 5 seconds
            while (stopwatch.ElapsedMilliseconds < 5000)
            {
                var analysisTask = _analysisService.AnalyzeCultivationDataAsync();
                var recommendationTask = _recommendationService.GenerateRecommendationsAsync(CreateTestAnalysis());
                
                yield return new WaitUntil(() => analysisTask.IsCompleted && recommendationTask.IsCompleted);
                operationCount++;
                
                // Brief pause to prevent overwhelming
                yield return new WaitForEndOfFrame();
            }
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024);
            
            // Assert
            Assert.IsTrue(operationCount > 0, "Should have completed at least one operation");
            Assert.IsTrue(memoryIncrease < MAX_MEMORY_INCREASE_MB * 3, 
                $"Memory increase was {memoryIncrease}MB during continuous operation");
            
            UnityEngine.Debug.Log($"[Continuous Operation] {operationCount} operations in 5s, Memory: +{memoryIncrease}MB");
        }
        
        #endregion
        
        #region Scalability Tests
        
        [Test]
        public void AIRecommendationService_ScalesWithRecommendationCount()
        {
            // Arrange
            _recommendationService.Initialize();
            var testSizes = new int[] { 10, 50, 100, 500 };
            var results = new Dictionary<int, long>();
            
            foreach (var size in testSizes)
            {
                // Create test data of varying sizes
                var recommendations = new List<AIRecommendation>();
                for (int i = 0; i < size; i++)
                {
                    recommendations.Add(CreateTestRecommendation($"Test {i}"));
                }
                
                // Act
                var stopwatch = Stopwatch.StartNew();
                var prioritized = _recommendationService.PrioritizeRecommendations(recommendations);
                stopwatch.Stop();
                
                results[size] = stopwatch.ElapsedMilliseconds;
                
                // Assert individual performance
                Assert.IsNotNull(prioritized, $"Should prioritize {size} recommendations");
                Assert.AreEqual(size, prioritized.Count, $"Should return all {size} recommendations");
            }
            
            // Assert scaling characteristics
            var scaling10to50 = (float)results[50] / results[10];
            var scaling50to100 = (float)results[100] / results[50];
            
            // Should scale roughly linearly (within reasonable bounds)
            Assert.IsTrue(scaling10to50 < 10f, $"Scaling from 10 to 50 items: {scaling10to50}x (should be <10x)");
            Assert.IsTrue(scaling50to100 < 5f, $"Scaling from 50 to 100 items: {scaling50to100}x (should be <5x)");
            
            UnityEngine.Debug.Log($"[Scalability] 10: {results[10]}ms, 50: {results[50]}ms, 100: {results[100]}ms, 500: {results[500]}ms");
        }
        
        #endregion
        
        #region Memory Leak Tests
        
        [Test]
        public void AIServices_NoMemoryLeaks_RepeatedInitialization()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act - Initialize and shutdown multiple times
            for (int i = 0; i < 10; i++)
            {
                var analysisService = CreateTestManager<AIAnalysisService>();
                analysisService.Initialize();
                analysisService.Shutdown();
                DestroyImmediate(analysisService.gameObject);
                
                var recommendationService = CreateTestManager<AIRecommendationService>();
                recommendationService.Initialize();
                recommendationService.Shutdown();
                DestroyImmediate(recommendationService.gameObject);
                
                // Force cleanup
                if (i % 3 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            
            // Final cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024 * 1024);
            
            // Assert
            Assert.IsTrue(memoryIncrease < 5, // Allow some tolerance for test infrastructure
                $"Memory leak detected: {memoryIncrease}MB increase after repeated initialization");
            
            UnityEngine.Debug.Log($"[Memory Leak Test] Memory change: {memoryIncrease}MB");
        }
        
        #endregion
        
        #region Benchmark Tests
        
        [Test]
        public void AIServices_ComprehensiveBenchmark()
        {
            // Arrange
            _integrationService.Initialize();
            var benchmarkResults = new Dictionary<string, long>();
            var stopwatch = new Stopwatch();
            
            // Benchmark Analysis Service
            stopwatch.Restart();
            var cultivationTask = _analysisService.AnalyzeCultivationDataAsync();
            Task.WaitAll(cultivationTask);
            stopwatch.Stop();
            benchmarkResults["CultivationAnalysis"] = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            var environmentalTask = _analysisService.AnalyzeEnvironmentalDataAsync();
            Task.WaitAll(environmentalTask);
            stopwatch.Stop();
            benchmarkResults["EnvironmentalAnalysis"] = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            var geneticsTask = _analysisService.AnalyzeGeneticsDataAsync();
            Task.WaitAll(geneticsTask);
            stopwatch.Stop();
            benchmarkResults["GeneticsAnalysis"] = stopwatch.ElapsedMilliseconds;
            
            // Benchmark Recommendation Service
            stopwatch.Restart();
            var recommendationTask = _recommendationService.GenerateRecommendationsAsync(cultivationTask.Result);
            Task.WaitAll(recommendationTask);
            stopwatch.Stop();
            benchmarkResults["RecommendationGeneration"] = stopwatch.ElapsedMilliseconds;
            
            // Log all benchmark results
            foreach (var result in benchmarkResults)
            {
                UnityEngine.Debug.Log($"[Benchmark] {result.Key}: {result.Value}ms");
                
                // Assert reasonable performance
                Assert.IsTrue(result.Value < 200, $"{result.Key} took {result.Value}ms, should be under 200ms");
            }
            
            // Calculate total pipeline time
            var totalTime = benchmarkResults.Values.Sum();
            Assert.IsTrue(totalTime < 500, $"Total AI pipeline took {totalTime}ms, should be under 500ms");
            
            UnityEngine.Debug.Log($"[Benchmark] Total AI Pipeline: {totalTime}ms");
        }
        
        #endregion
        
        #region Helper Methods
        
        private CultivationAnalysisResult CreateTestAnalysis()
        {
            return new CultivationAnalysisResult
            {
                TotalPlants = 10,
                ActivePlants = 8,
                AverageHealth = 0.85f,
                PredictedYield = 150f,
                OptimalYield = 180f,
                HealthIssues = new List<string> { "Test issue" },
                GrowthRecommendations = new List<string> { "Test recommendation" },
                EfficiencyScore = 0.8f
            };
        }
        
        private CultivationAnalysisResult CreateLargeTestAnalysis()
        {
            var result = CreateTestAnalysis();
            result.TotalPlants = 1000;
            result.ActivePlants = 850;
            
            // Add more data to simulate larger analysis
            for (int i = 0; i < 50; i++)
            {
                result.HealthIssues.Add($"Health issue {i}");
                result.GrowthRecommendations.Add($"Growth recommendation {i}");
            }
            
            return result;
        }
        
        private AIRecommendation CreateTestRecommendation(string title)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = $"Test recommendation: {title}",
                Description = $"Detailed description for {title}",
                Priority = (RecommendationPriority)(UnityEngine.Random.Range(0, 4)),
                Category = "Performance Test",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                ConfidenceScore = UnityEngine.Random.Range(0.5f, 1f),
                EstimatedImpact = UnityEngine.Random.Range(0.3f, 0.9f)
            };
        }
        
        #endregion
    }
}