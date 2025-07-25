using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Automated test runner for PC013 refactoring systems
    /// Can be used in CI/CD pipelines and automated testing scenarios
    /// </summary>
    public class PC013AutomatedTestRunner
    {
        private static PC013RefactoringTestFramework _testFramework;
        
        #region NUnit Test Cases
        
        /// <summary>
        /// Setup test framework before running tests
        /// </summary>
        [OneTimeSetUp]
        public void SetupTestFramework()
        {
            // Create test framework instance
            var testGO = new GameObject("PC013TestFramework");
            _testFramework = testGO.AddComponent<PC013RefactoringTestFramework>();
            
            Assert.IsNotNull(_testFramework, "Test framework should be created successfully");
        }
        
        /// <summary>
        /// Test dependency injection systems
        /// </summary>
        [UnityTest]
        public IEnumerator TestDependencyInjectionSystems()
        {
            Assert.IsNotNull(_testFramework, "Test framework must be initialized");
            
            // Enable only DI tests
            _testFramework._enableDependencyInjectionTests = true;
            _testFramework._enableOptimizationTests = false;
            _testFramework._enablePoolingTests = false;
            _testFramework._enablePerformanceTests = false;
            
            yield return _testFramework.StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            Assert.IsNotNull(results.DependencyInjectionResults, "DI test results should exist");
            
            var diResults = results.DependencyInjectionResults;
            Assert.IsTrue(diResults.ServiceContainerCreated, "Service container should be created");
            Assert.IsTrue(diResults.ServiceRegistrationWorking, "Service registration should work");
            Assert.IsTrue(diResults.ServiceResolutionWorking, "Service resolution should work");
            
            Debug.Log($"Dependency Injection Tests: {diResults.TestsPassed}/{diResults.TotalTests} passed");
        }
        
        /// <summary>
        /// Test optimization systems
        /// </summary>
        [UnityTest]
        public IEnumerator TestOptimizationSystems()
        {
            Assert.IsNotNull(_testFramework, "Test framework must be initialized");
            
            // Enable only optimization tests
            _testFramework._enableDependencyInjectionTests = false;
            _testFramework._enableOptimizationTests = true;
            _testFramework._enablePoolingTests = false;
            _testFramework._enablePerformanceTests = false;
            
            yield return _testFramework.StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            Assert.IsNotNull(results.OptimizationResults, "Optimization test results should exist");
            
            var optResults = results.OptimizationResults;
            Assert.IsTrue(optResults.PurgeManagerWorking, "Purge manager should work");
            Assert.IsTrue(optResults.UpdateOptimizerWorking, "Update optimizer should work");
            Assert.IsTrue(optResults.BatchProcessorWorking, "Batch processor should work");
            
            Debug.Log($"Optimization Tests: {optResults.TestsPassed}/{optResults.TotalTests} passed");
        }
        
        /// <summary>
        /// Test pooling systems
        /// </summary>
        [UnityTest]
        public IEnumerator TestPoolingSystems()
        {
            Assert.IsNotNull(_testFramework, "Test framework must be initialized");
            
            // Enable only pooling tests
            _testFramework._enableDependencyInjectionTests = false;
            _testFramework._enableOptimizationTests = false;
            _testFramework._enablePoolingTests = true;
            _testFramework._enablePerformanceTests = false;
            
            yield return _testFramework.StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            Assert.IsNotNull(results.PoolingResults, "Pooling test results should exist");
            
            var poolResults = results.PoolingResults;
            Assert.IsTrue(poolResults.GenericPoolWorking, "Generic pool should work");
            Assert.IsTrue(poolResults.PooledObjectManagerWorking, "Pooled object manager should work");
            Assert.IsTrue(poolResults.CollectionPurgersWorking, "Collection purgers should work");
            
            Debug.Log($"Pooling Tests: {poolResults.TestsPassed}/{poolResults.TotalTests} passed");
        }
        
        /// <summary>
        /// Test performance optimizations
        /// </summary>
        [UnityTest]
        public IEnumerator TestPerformanceOptimizations()
        {
            Assert.IsNotNull(_testFramework, "Test framework must be initialized");
            
            // Enable only performance tests
            _testFramework._enableDependencyInjectionTests = false;
            _testFramework._enableOptimizationTests = false;
            _testFramework._enablePoolingTests = false;
            _testFramework._enablePerformanceTests = true;
            
            yield return _testFramework.StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            Assert.IsNotNull(results.PerformanceResults, "Performance test results should exist");
            
            var perfResults = results.PerformanceResults;
            Assert.IsTrue(perfResults.MemoryOptimizationEffective, "Memory optimization should be effective");
            Assert.IsTrue(perfResults.BatchProcessingEfficient, "Batch processing should be efficient");
            Assert.Less(perfResults.BatchProcessingTime, 100f, "Batch processing should be under 100ms");
            
            Debug.Log($"Performance Tests: {perfResults.TestsPassed}/{perfResults.TotalTests} passed");
            Debug.Log($"Memory reduction: {perfResults.AllocationReduction:F1}KB");
            Debug.Log($"Batch processing time: {perfResults.BatchProcessingTime:F2}ms");
        }
        
        /// <summary>
        /// Comprehensive integration test
        /// </summary>
        [UnityTest]
        public IEnumerator TestCompletePC013Integration()
        {
            Assert.IsNotNull(_testFramework, "Test framework must be initialized");
            
            // Enable all tests
            _testFramework._enableDependencyInjectionTests = true;
            _testFramework._enableOptimizationTests = true;
            _testFramework._enablePoolingTests = true;
            _testFramework._enablePerformanceTests = true;
            
            yield return _testFramework.StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            Assert.IsNotNull(results, "Test results should exist");
            
            // Verify all major systems are working
            Assert.IsNotNull(results.DependencyInjectionResults, "DI results should exist");
            Assert.IsNotNull(results.OptimizationResults, "Optimization results should exist");
            Assert.IsNotNull(results.PoolingResults, "Pooling results should exist");
            Assert.IsNotNull(results.PerformanceResults, "Performance results should exist");
            
            // Calculate overall success rate
            int totalTests = results.DependencyInjectionResults.TotalTests +
                           results.OptimizationResults.TotalTests +
                           results.PoolingResults.TotalTests +
                           results.PerformanceResults.TotalTests;
                           
            int totalPassed = results.DependencyInjectionResults.TestsPassed +
                            results.OptimizationResults.TestsPassed +
                            results.PoolingResults.TestsPassed +
                            results.PerformanceResults.TestsPassed;
            
            float successRate = totalTests > 0 ? (float)totalPassed / totalTests : 0f;
            
            Assert.GreaterOrEqual(successRate, 0.8f, "Overall success rate should be at least 80%");
            
            Debug.Log($"Complete PC013 Integration Test: {totalPassed}/{totalTests} passed ({successRate:P1} success rate)");
            Debug.Log($"Total test duration: {results.TotalTestDuration:F2} seconds");
        }
        
        /// <summary>
        /// Cleanup after tests
        /// </summary>
        [OneTimeTearDown]
        public void CleanupTestFramework()
        {
            if (_testFramework != null)
            {
                UnityEngine.Object.DestroyImmediate(_testFramework.gameObject);
                _testFramework = null;
            }
        }
        
        #endregion
        
        #region Static Test Utilities
        
        /// <summary>
        /// Run PC013 tests programmatically
        /// </summary>
        public static IEnumerator RunPC013TestsAsync()
        {
            var testGO = new GameObject("PC013TestFramework");
            var framework = testGO.AddComponent<PC013RefactoringTestFramework>();
            
            yield return framework.StartCoroutine(framework.RunAllTests());
            
            var results = framework.GetTestResults();
            
            UnityEngine.Object.DestroyImmediate(testGO);
            
            Debug.Log($"PC013 tests completed in {results.TotalTestDuration:F2} seconds");
        }
        
        /// <summary>
        /// Quick validation of PC013 systems
        /// </summary>
        public static bool ValidatePC013Systems()
        {
            try
            {
                // Quick validation without full test suite
                var container = new ProjectChimera.Core.DependencyInjection.ServiceContainer();
                var pool = new ProjectChimera.Core.Optimization.GenericObjectPool<TestPoolableObject>(10);
                
                return container != null && pool != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PC013 system validation failed: {ex.Message}");
                return false;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Test utilities for PC013 testing
    /// </summary>
    public static class PC013TestUtilities
    {
        /// <summary>
        /// Create test plant instances for performance testing
        /// </summary>
        public static GameObject[] CreateTestPlantInstances(int count)
        {
            var plants = new GameObject[count];
            
            for (int i = 0; i < count; i++)
            {
                var plantGO = new GameObject($"TestPlant_{i}");
                // Add basic components that would be on a real plant
                plantGO.AddComponent<MeshRenderer>();
                plantGO.AddComponent<MeshFilter>();
                plants[i] = plantGO;
            }
            
            return plants;
        }
        
        /// <summary>
        /// Cleanup test plant instances
        /// </summary>
        public static void CleanupTestPlantInstances(GameObject[] plants)
        {
            if (plants == null) return;
            
            foreach (var plant in plants)
            {
                if (plant != null)
                    UnityEngine.Object.DestroyImmediate(plant);
            }
        }
        
        /// <summary>
        /// Measure memory usage during operation
        /// </summary>
        public static long MeasureMemoryUsage(System.Action operation)
        {
            var initialMemory = System.GC.GetTotalMemory(true);
            
            operation?.Invoke();
            
            var finalMemory = System.GC.GetTotalMemory(false);
            return finalMemory - initialMemory;
        }
        
        /// <summary>
        /// Time an operation
        /// </summary>
        public static float TimeOperation(System.Action operation)
        {
            var startTime = Time.realtimeSinceStartup;
            
            operation?.Invoke();
            
            var endTime = Time.realtimeSinceStartup;
            return endTime - startTime;
        }
    }
}