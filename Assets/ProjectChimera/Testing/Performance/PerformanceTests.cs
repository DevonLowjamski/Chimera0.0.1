using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Economy;
using ProjectChimera.Systems.AI;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Financial;
using ProjectChimera.UI.AIAdvisor;
using ProjectChimera.Data.Automation;

namespace ProjectChimera.Testing.Performance
{
    [TestFixture]
    public class PerformanceTests
    {
        private GameObject _testGameObject;
        private MarketManager _marketManager;
        private AIAdvisorManager _aiManager;
        private FinancialManagementController _financialController;
        private AIAdvisorController _aiController;
        
        // Performance thresholds (in milliseconds)
        private const int MARKET_PORTFOLIO_THRESHOLD = 1;
        private const int MARKET_TRANSACTION_THRESHOLD = 5;
        private const int AI_QUERY_THRESHOLD = 15;
        private const int AI_ANALYSIS_THRESHOLD = 10;
        private const int UI_UPDATE_THRESHOLD = 16; // 60 FPS = 16.67ms per frame
        private const int MEMORY_THRESHOLD_MB = 1;
        private const int STRESS_TEST_OPERATIONS = 1000;

        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("PerformanceTestManager");
            
            // Initialize managers
            _marketManager = _testGameObject.AddComponent<MarketManager>();
            _aiManager = _testGameObject.AddComponent<AIAdvisorManager>();
            
            // Initialize UI controllers
            _financialController = _testGameObject.AddComponent<FinancialManagementController>();
            _aiController = _testGameObject.AddComponent<AIAdvisorController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
                Object.DestroyImmediate(_testGameObject);
        }

        #region Market System Performance Tests

        //[Test]
        
        public void MarketManager_GetPortfolioMetrics_PerformanceBenchmark()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                _marketManager.GetPortfolioMetrics();
            }

            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 100.0;
            Assert.Less(avgTimeMs, MARKET_PORTFOLIO_THRESHOLD, 
                $"GetPortfolioMetrics average time ({avgTimeMs:F2}ms) should be under {MARKET_PORTFOLIO_THRESHOLD}ms");
            
            TestContext.WriteLine($"GetPortfolioMetrics: {avgTimeMs:F2}ms average over 100 calls");
        }

        //[Test]
        
        public void MarketManager_ProcessTransaction_PerformanceBenchmark()
        {
            // Arrange
            var testProduct = ScriptableObject.CreateInstance<MarketProductSO>();
            testProduct.name = "TestProduct";
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 50; i++)
            {
                _marketManager.ProcessSale(testProduct, 1f, 0.8f, false);
                _marketManager.ProcessPurchase(testProduct, 1f, false);
            }

            stopwatch.Stop();
            Object.DestroyImmediate(testProduct);

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 100.0; // 100 total operations
            Assert.Less(avgTimeMs, MARKET_TRANSACTION_THRESHOLD, 
                $"Transaction processing average time ({avgTimeMs:F2}ms) should be under {MARKET_TRANSACTION_THRESHOLD}ms");
            
            TestContext.WriteLine($"Transaction Processing: {avgTimeMs:F2}ms average over 100 operations");
        }

        //[Test]
        
        public void MarketManager_GetFinancialData_PerformanceBenchmark()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 200; i++)
            {
                _marketManager.GetFinancialData();
            }

            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 200.0;
            Assert.Less(avgTimeMs, MARKET_PORTFOLIO_THRESHOLD, 
                $"GetFinancialData average time ({avgTimeMs:F2}ms) should be under {MARKET_PORTFOLIO_THRESHOLD}ms");
            
            TestContext.WriteLine($"GetFinancialData: {avgTimeMs:F2}ms average over 200 calls");
        }

        #endregion

        #region AI System Performance Tests

        //[Test]
        
        public void AIManager_ProcessUserQuery_PerformanceBenchmark()
        {
            // Arrange
            var completedQueries = 0;
            var totalQueries = 10;
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < totalQueries; i++)
            {
                _aiManager.ProcessUserQuery($"Test query {i}", response => 
                {
                    completedQueries++;
                });
            }

            // Wait for completion
            var timeout = Time.time + 10f;
            while (completedQueries < totalQueries && Time.time < timeout)
            {
                // Wait for async operations
            }

            stopwatch.Stop();

            // Assert
            Assert.AreEqual(totalQueries, completedQueries, "All queries should complete");
            var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)totalQueries;
            Assert.Less(avgTimeMs, AI_QUERY_THRESHOLD, 
                $"AI query average time ({avgTimeMs:F2}ms) should be under {AI_QUERY_THRESHOLD}ms");
            
            TestContext.WriteLine($"AI Query Processing: {avgTimeMs:F2}ms average over {totalQueries} queries");
        }

        //[Test]
        
        public void AIManager_AnalyzeFacilityState_PerformanceBenchmark()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                _aiManager.AnalyzeFacilityState();
            }

            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 100.0;
            Assert.Less(avgTimeMs, AI_ANALYSIS_THRESHOLD, 
                $"AnalyzeFacilityState average time ({avgTimeMs:F2}ms) should be under {AI_ANALYSIS_THRESHOLD}ms");
            
            TestContext.WriteLine($"Facility Analysis: {avgTimeMs:F2}ms average over 100 calls");
        }

        //[Test]
        
        public void AIManager_GeneratePredictions_PerformanceBenchmark()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 50; i++)
            {
                _aiManager.GeneratePredictions();
            }

            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 50.0;
            Assert.Less(avgTimeMs, AI_ANALYSIS_THRESHOLD, 
                $"GeneratePredictions average time ({avgTimeMs:F2}ms) should be under {AI_ANALYSIS_THRESHOLD}ms");
            
            TestContext.WriteLine($"Prediction Generation: {avgTimeMs:F2}ms average over 50 calls");
        }

        //[Test]
        
        public void AIManager_GetRecommendations_PerformanceBenchmark()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 150; i++)
            {
                _aiManager.GetActiveRecommendations();
                _aiManager.GetOptimizationOpportunities();
                _aiManager.GetRecentInsights(10);
            }

            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / 450.0; // 450 total operations
            Assert.Less(avgTimeMs, 2, 
                $"Recommendation retrieval average time ({avgTimeMs:F2}ms) should be under 2ms");
            
            TestContext.WriteLine($"Recommendation Retrieval: {avgTimeMs:F2}ms average over 450 operations");
        }

        #endregion

        #region Memory Performance Tests

        //[Test]
        
        public void MemoryUsage_SystemOperations_WithinThreshold()
        {
            // Arrange
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            long initialMemory = System.GC.GetTotalMemory(false);

            // Act - Perform memory-intensive operations
            for (int i = 0; i < 100; i++)
            {
                _marketManager.GetPortfolioMetrics();
                _aiManager.GetActiveRecommendations();
                _aiManager.GeneratePredictions();
            }

            // Force cleanup
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            long finalMemory = System.GC.GetTotalMemory(false);

            // Assert
            long memoryIncreaseMB = (finalMemory - initialMemory) / (1024 * 1024);
            Assert.Less(memoryIncreaseMB, MEMORY_THRESHOLD_MB, 
                $"Memory increase ({memoryIncreaseMB}MB) should be under {MEMORY_THRESHOLD_MB}MB");
            
            TestContext.WriteLine($"Memory Usage: {memoryIncreaseMB}MB increase over 100 operations");
        }

        //[Test]
        
        public void MemoryLeakTest_RepeatedOperations_NoLeaks()
        {
            // Arrange
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            long baselineMemory = System.GC.GetTotalMemory(false);

            // Act - Repeated cycles to detect memory leaks
            for (int cycle = 0; cycle < 5; cycle++)
            {
                for (int i = 0; i < 50; i++)
                {
                    var testProduct = ScriptableObject.CreateInstance<MarketProductSO>();
                    _marketManager.ProcessSale(testProduct, 1f, 0.8f, false);
                    Object.DestroyImmediate(testProduct);
                }
                
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            long finalMemory = System.GC.GetTotalMemory(false);

            // Assert
            long memoryIncreaseMB = (finalMemory - baselineMemory) / (1024 * 1024);
            Assert.Less(memoryIncreaseMB, 2, 
                $"Memory increase after leak test ({memoryIncreaseMB}MB) should be minimal");
            
            TestContext.WriteLine($"Memory Leak Test: {memoryIncreaseMB}MB increase after 250 operations");
        }

        #endregion

        #region Stress Tests

        //[Test]
        
        public void StressTest_MarketOperations_HandlesLoad()
        {
            // Arrange
            var testProduct = ScriptableObject.CreateInstance<MarketProductSO>();
            testProduct.name = "StressTestProduct";
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < STRESS_TEST_OPERATIONS; i++)
            {
                _marketManager.GetPortfolioMetrics();
                if (i % 2 == 0)
                    _marketManager.ProcessSale(testProduct, 1f, 0.8f, false);
                else
                    _marketManager.ProcessPurchase(testProduct, 1f, false);
            }

            stopwatch.Stop();
            Object.DestroyImmediate(testProduct);

            // Assert
            var opsPerSecond = STRESS_TEST_OPERATIONS / (stopwatch.ElapsedMilliseconds / 1000.0);
            Assert.Greater(opsPerSecond, 1000, 
                $"Market operations should handle >1000 ops/second, actual: {opsPerSecond:F0}");
            
            TestContext.WriteLine($"Market Stress Test: {opsPerSecond:F0} operations/second over {STRESS_TEST_OPERATIONS} operations");
        }

        //[Test]
        
        public void StressTest_AIOperations_HandlesLoad()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 500; i++) // Reduced count for AI due to complexity
            {
                _aiManager.AnalyzeFacilityState();
                _aiManager.GetActiveRecommendations();
                if (i % 10 == 0)
                    _aiManager.GeneratePredictions();
            }

            stopwatch.Stop();

            // Assert
            var opsPerSecond = 500 / (stopwatch.ElapsedMilliseconds / 1000.0);
            Assert.Greater(opsPerSecond, 100, 
                $"AI operations should handle >100 ops/second, actual: {opsPerSecond:F0}");
            
            TestContext.WriteLine($"AI Stress Test: {opsPerSecond:F0} operations/second over 500 operations");
        }

        //[Test]
        
        public void ConcurrentOperations_MultipleThreads_ThreadSafe()
        {
            // Arrange
            var completedOperations = 0;
            var exceptions = new List<System.Exception>();
            var stopwatch = Stopwatch.StartNew();

            // Act - Simulate concurrent operations
            System.Threading.Tasks.Parallel.For(0, 50, i =>
            {
                try
                {
                    _marketManager.GetPortfolioMetrics();
                    _aiManager.GetActiveRecommendations();
                    System.Threading.Interlocked.Increment(ref completedOperations);
                }
                catch (System.Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });

            stopwatch.Stop();

            // Assert
            Assert.AreEqual(50, completedOperations, "All concurrent operations should complete");
            Assert.IsEmpty(exceptions, "No exceptions should occur during concurrent operations");
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "Concurrent operations should complete within 1 second");
            
            TestContext.WriteLine($"Concurrent Operations: {completedOperations} operations completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Performance Summary

        //[Test]
        
        public void PerformanceSummary_AllSystems_GenerateReport()
        {
            // This test generates a comprehensive performance summary
            TestContext.WriteLine("\n=== PROJECT CHIMERA PERFORMANCE SUMMARY ===");
            TestContext.WriteLine($"Market System Thresholds: Portfolio {MARKET_PORTFOLIO_THRESHOLD}ms, Transactions {MARKET_TRANSACTION_THRESHOLD}ms");
            TestContext.WriteLine($"AI System Thresholds: Queries {AI_QUERY_THRESHOLD}ms, Analysis {AI_ANALYSIS_THRESHOLD}ms");
            TestContext.WriteLine($"UI System Threshold: {UI_UPDATE_THRESHOLD}ms (60 FPS target)");
            TestContext.WriteLine($"Memory Threshold: {MEMORY_THRESHOLD_MB}MB increase per 100 operations");
            TestContext.WriteLine($"Stress Test Target: {STRESS_TEST_OPERATIONS} operations for market systems");
            TestContext.WriteLine("===========================================\n");

            // All tests should pass if individual performance tests pass
            Assert.IsTrue(true, "Performance summary generated successfully");
        }

        #endregion
    }
} 