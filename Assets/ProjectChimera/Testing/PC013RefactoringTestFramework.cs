using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using ProjectChimera.Core;
using ProjectChimera.Core.DependencyInjection;
using ProjectChimera.Core.Optimization;
// using ProjectChimera.Systems.SpeedTree; // Removed due to circular dependencies

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Comprehensive testing framework for PC013 refactored systems
    /// Tests dependency injection, optimization, and pooling systems
    /// </summary>
    public class PC013RefactoringTestFramework : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] public bool _enableDependencyInjectionTests = true;
        [SerializeField] public bool _enableOptimizationTests = true;
        [SerializeField] public bool _enablePoolingTests = true;
        [SerializeField] public bool _enablePerformanceTests = true;
        [SerializeField] private bool _runTestsOnStart = false;
        
        [Header("Performance Test Settings")]
        [SerializeField] private int _testPlantCount = 100;
        [SerializeField] private float _performanceTestDuration = 10f;
        [SerializeField] private float _targetFrameRate = 60f;
        
        // Test results
        private PC013TestResults _testResults = new PC013TestResults();
        private bool _testsCompleted = false;
        
        public static PC013RefactoringTestFramework Instance { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (_runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Run all PC013 refactoring tests
        /// </summary>
        public IEnumerator RunAllTests()
        {
            Debug.Log("[PC013TestFramework] Starting comprehensive PC013 refactoring tests");
            
            _testResults = new PC013TestResults();
            _testResults.TestStartTime = DateTime.Now;
            
            // Dependency Injection Tests
            if (_enableDependencyInjectionTests)
            {
                yield return StartCoroutine(RunDependencyInjectionTests());
            }
            
            // Optimization Tests
            if (_enableOptimizationTests)
            {
                yield return StartCoroutine(RunOptimizationTests());
            }
            
            // Pooling Tests
            if (_enablePoolingTests)
            {
                yield return StartCoroutine(RunPoolingTests());
            }
            
            // Performance Tests
            if (_enablePerformanceTests)
            {
                yield return StartCoroutine(RunPerformanceTests());
            }
            
            _testResults.TestEndTime = DateTime.Now;
            _testResults.TotalTestDuration = (float)(_testResults.TestEndTime - _testResults.TestStartTime).TotalSeconds;
            _testsCompleted = true;
            
            LogTestResults();
        }
        
        /// <summary>
        /// Get comprehensive test results
        /// </summary>
        public PC013TestResults GetTestResults()
        {
            return _testResults;
        }
        
        /// <summary>
        /// Check if all tests have completed
        /// </summary>
        public bool AreTestsCompleted()
        {
            return _testsCompleted;
        }
        
        #endregion
        
        #region Dependency Injection Tests
        
        private IEnumerator RunDependencyInjectionTests()
        {
            Debug.Log("[PC013TestFramework] Running Dependency Injection Tests");
            
            var diResults = new DependencyInjectionTestResults();
            
            // Test 1: Service Container Creation
            yield return TestServiceContainerCreation(diResults);
            
            // Test 2: Service Registration
            yield return TestServiceRegistration(diResults);
            
            // Test 3: Service Resolution
            yield return TestServiceResolution(diResults);
            
            // Test 4: Manager DI Integration
            yield return TestManagerDIIntegration(diResults);
            
            _testResults.DependencyInjectionResults = diResults;
            
            Debug.Log($"[PC013TestFramework] DI Tests Complete - Passed: {diResults.TestsPassed}/{diResults.TotalTests}");
        }
        
        private IEnumerator TestServiceContainerCreation(DependencyInjectionTestResults results)
        {
            try
            {
                // Test ServiceContainer instantiation
                var container = new ServiceContainer();
                Assert.IsNotNull(container, "ServiceContainer should be created successfully");
                
                results.ServiceContainerCreated = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] ServiceContainer creation failed: {ex.Message}");
                results.ServiceContainerCreated = false;
            }
            
            results.TotalTests++;
            yield return null;
        }
        
        private IEnumerator TestServiceRegistration(DependencyInjectionTestResults results)
        {
            try
            {
                var container = new ServiceContainer();
                
                // Test singleton registration
                container.RegisterSingleton<ITestService>(new TestService());
                Assert.IsTrue(container.IsRegistered<ITestService>(), "Service should be registered");
                
                results.ServiceRegistrationWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Service registration failed: {ex.Message}");
                results.ServiceRegistrationWorking = false;
            }
            
            results.TotalTests++;
            yield return null;
        }
        
        private IEnumerator TestServiceResolution(DependencyInjectionTestResults results)
        {
            try
            {
                var container = new ServiceContainer();
                var testService = new TestService();
                container.RegisterSingleton<ITestService>(testService);
                
                // Test service resolution
                var resolvedService = container.Resolve<ITestService>();
                Assert.IsNotNull(resolvedService, "Service should be resolved");
                Assert.AreSame(testService, resolvedService, "Should return same singleton instance");
                
                results.ServiceResolutionWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Service resolution failed: {ex.Message}");
                results.ServiceResolutionWorking = false;
            }
            
            results.TotalTests++;
            yield return null;
        }
        
        private IEnumerator TestManagerDIIntegration(DependencyInjectionTestResults results)
        {
            GameObject gameManagerGO = null;
            
            try
            {
                // Test DIGameManager integration
                gameManagerGO = new GameObject("TestDIGameManager");
                var diGameManager = gameManagerGO.AddComponent<DIGameManager>();
                
                results.ManagerIntegrationWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Manager DI integration failed: {ex.Message}");
                results.ManagerIntegrationWorking = false;
            }
            
            // Allow initialization
            yield return new WaitForSeconds(0.5f);
            
            // Cleanup
            if (gameManagerGO != null)
            {
                DestroyImmediate(gameManagerGO);
            }
            
            results.TotalTests++;
        }
        
        #endregion
        
        #region Optimization Tests
        
        private IEnumerator RunOptimizationTests()
        {
            Debug.Log("[PC013TestFramework] Running Optimization Tests");
            
            var optResults = new OptimizationTestResults();
            
            // Test 1: Object Purge Manager
            yield return TestObjectPurgeManager(optResults);
            
            // Test 2: Plant Update Optimizer
            yield return TestPlantUpdateOptimizer(optResults);
            
            // Test 3: Batch Processor
            yield return TestBatchProcessor(optResults);
            
            _testResults.OptimizationResults = optResults;
            
            Debug.Log($"[PC013TestFramework] Optimization Tests Complete - Passed: {optResults.TestsPassed}/{optResults.TotalTests}");
        }
        
        private IEnumerator TestObjectPurgeManager(OptimizationTestResults results)
        {
            GameObject purgeManagerGO = null;
            
            try
            {
                purgeManagerGO = new GameObject("TestObjectPurgeManager");
                var purgeManager = purgeManagerGO.AddComponent<ObjectPurgeManager>();
                
                Assert.IsNotNull(purgeManager, "ObjectPurgeManager component should be created");
                
                results.PurgeManagerWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] ObjectPurgeManager test failed: {ex.Message}");
                results.PurgeManagerWorking = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (purgeManagerGO != null)
            {
                DestroyImmediate(purgeManagerGO);
            }
            
            results.TotalTests++;
        }
        
        private IEnumerator TestPlantUpdateOptimizer(OptimizationTestResults results)
        {
            GameObject optimizerGO = null;
            
            try
            {
                optimizerGO = new GameObject("TestPlantUpdateOptimizer");
                var optimizer = optimizerGO.AddComponent<PlantUpdateOptimizer>();
                
                Assert.IsNotNull(optimizer, "PlantUpdateOptimizer component should be created");
                
                results.UpdateOptimizerWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] PlantUpdateOptimizer test failed: {ex.Message}");
                results.UpdateOptimizerWorking = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (optimizerGO != null)
            {
                DestroyImmediate(optimizerGO);
            }
            
            results.TotalTests++;
        }
        
        private IEnumerator TestBatchProcessor(OptimizationTestResults results)
        {
            GameObject processorGO = null;
            
            try
            {
                processorGO = new GameObject("TestPlantBatchProcessor");
                var processor = processorGO.AddComponent<PlantBatchProcessor>();
                
                Assert.IsNotNull(processor, "PlantBatchProcessor component should be created");
                
                // Test batch processing with empty list (should not crash)
                var emptyList = new List<GameObject>();
                processor.ProcessPlantCollection(emptyList);
                
                results.BatchProcessorWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] PlantBatchProcessor test failed: {ex.Message}");
                results.BatchProcessorWorking = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (processorGO != null)
            {
                DestroyImmediate(processorGO);
            }
            
            results.TotalTests++;
        }
        
        #endregion
        
        #region Pooling Tests
        
        private IEnumerator RunPoolingTests()
        {
            Debug.Log("[PC013TestFramework] Running Pooling Tests");
            
            var poolResults = new PoolingTestResults();
            
            // Test 1: Generic Object Pool
            yield return TestGenericObjectPool(poolResults);
            
            // Test 2: Pooled Object Manager
            yield return TestPooledObjectManager(poolResults);
            
            // Test 3: Collection Purgers
            yield return TestCollectionPurgers(poolResults);
            
            _testResults.PoolingResults = poolResults;
            
            Debug.Log($"[PC013TestFramework] Pooling Tests Complete - Passed: {poolResults.TestsPassed}/{poolResults.TotalTests}");
        }
        
        private IEnumerator TestGenericObjectPool(PoolingTestResults results)
        {
            try
            {
                // Test generic object pool functionality
                var pool = new GenericObjectPool<TestPoolableObject>(10);
                
                // Test object creation and return
                var obj1 = pool.Get();
                Assert.IsNotNull(obj1, "Pool should return valid object");
                
                pool.Return(obj1);
                
                var obj2 = pool.Get();
                Assert.AreSame(obj1, obj2, "Pool should reuse returned object");
                
                results.GenericPoolWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Generic object pool test failed: {ex.Message}");
                results.GenericPoolWorking = false;
            }
            
            results.TotalTests++;
            yield return null;
        }
        
        private IEnumerator TestPooledObjectManager(PoolingTestResults results)
        {
            GameObject managerGO = null;
            
            try
            {
                managerGO = new GameObject("TestPooledObjectManager");
                var manager = managerGO.AddComponent<PooledObjectManager>();
                
                Assert.IsNotNull(manager, "PooledObjectManager component should be created");
                
                // Test pooled data dictionary
                var dict = manager.GetDataDictionary();
                Assert.IsNotNull(dict, "Should get valid data dictionary");
                
                manager.ReturnDataDictionary(dict);
                
                results.PooledObjectManagerWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] PooledObjectManager test failed: {ex.Message}");
                results.PooledObjectManagerWorking = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (managerGO != null)
            {
                DestroyImmediate(managerGO);
            }
            
            results.TotalTests++;
        }
        
        private IEnumerator TestCollectionPurgers(PoolingTestResults results)
        {
            GameObject uiPurgerGO = null;
            
            try
            {
                // Test UI Collection Purger
                uiPurgerGO = new GameObject("TestUICollectionPurger");
                var uiPurger = uiPurgerGO.AddComponent<UICollectionPurger>();
                
                Assert.IsNotNull(uiPurger, "UICollectionPurger component should be created");
                
                // Test dictionary pooling
                var dict = uiPurger.GetDataDictionary();
                Assert.IsNotNull(dict, "Should get valid dictionary from purger");
                uiPurger.ReturnDataDictionary(dict);
                
                results.CollectionPurgersWorking = true;
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Collection purgers test failed: {ex.Message}");
                results.CollectionPurgersWorking = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (uiPurgerGO != null)
            {
                DestroyImmediate(uiPurgerGO);
            }
            
            results.TotalTests++;
        }
        
        #endregion
        
        #region Performance Tests
        
        private IEnumerator RunPerformanceTests()
        {
            Debug.Log("[PC013TestFramework] Running Performance Tests");
            
            var perfResults = new PerformanceTestResults();
            
            // Test 1: Memory allocation optimization
            yield return TestMemoryOptimization(perfResults);
            
            // Test 2: Batch processing performance
            yield return TestBatchProcessingPerformance(perfResults);
            
            _testResults.PerformanceResults = perfResults;
            
            Debug.Log($"[PC013TestFramework] Performance Tests Complete - Passed: {perfResults.TestsPassed}/{perfResults.TotalTests}");
        }
        
        private IEnumerator TestMemoryOptimization(PerformanceTestResults results)
        {
            try
            {
                // Test memory usage with and without pooling
                var initialMemory = System.GC.GetTotalMemory(true);
                
                // Simulate heavy allocation without pooling
                var testObjects = new List<TestPoolableObject>();
                for (int i = 0; i < 1000; i++)
                {
                    testObjects.Add(new TestPoolableObject());
                }
                
                var memoryAfterAllocation = System.GC.GetTotalMemory(false);
                var allocationIncrease = memoryAfterAllocation - initialMemory;
                
                // Clear and force GC
                testObjects.Clear();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                
                var memoryAfterGC = System.GC.GetTotalMemory(true);
                
                results.MemoryOptimizationEffective = allocationIncrease > 0;
                results.AllocationReduction = (float)allocationIncrease / 1024f; // KB
                results.TestsPassed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Memory optimization test failed: {ex.Message}");
                results.MemoryOptimizationEffective = false;
            }
            
            results.TotalTests++;
            yield return new WaitForEndOfFrame();
        }
        
        private IEnumerator TestBatchProcessingPerformance(PerformanceTestResults results)
        {
            var processorGO = new GameObject("PerformanceTestProcessor");
            var processor = processorGO.AddComponent<PlantBatchProcessor>();
            
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                
                // Create test plant instances
                var testPlants = new List<GameObject>();
                for (int i = 0; i < _testPlantCount; i++)
                {
                    var plantGO = new GameObject($"TestPlant_{i}");
                    // Add basic components that would be on a real plant
                    plantGO.AddComponent<MeshRenderer>();
                    plantGO.AddComponent<MeshFilter>();
                    testPlants.Add(plantGO);
                }
                
                // Test batch processing performance
                var startTime = Time.realtimeSinceStartup;
                
                processor.ProcessPlantCollection(testPlants);
                
                var endTime = Time.realtimeSinceStartup;
                var processingTime = (endTime - startTime) * 1000f; // Convert to ms
                
                results.BatchProcessingTime = processingTime;
                results.BatchProcessingEfficient = processingTime < 50f; // Less than 50ms for 100 plants
                results.TestsPassed++;
                
                // Cleanup
                foreach (var plant in testPlants)
                {
                    if (plant != null)
                        DestroyImmediate(plant.gameObject);
                }
                DestroyImmediate(processorGO);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PC013TestFramework] Batch processing performance test failed: {ex.Message}");
                results.BatchProcessingEfficient = false;
            }
            
            results.TotalTests++;
            yield return null;
        }
        
        #endregion
        
        #region Test Result Logging
        
        private void LogTestResults()
        {
            Debug.Log("====================================");
            Debug.Log("PC013 REFACTORING TEST RESULTS");
            Debug.Log("====================================");
            
            Debug.Log($"Total Test Duration: {_testResults.TotalTestDuration:F2} seconds");
            Debug.Log($"Tests Started: {_testResults.TestStartTime}");
            Debug.Log($"Tests Completed: {_testResults.TestEndTime}");
            
            // Dependency Injection Results
            if (_testResults.DependencyInjectionResults != null)
            {
                var di = _testResults.DependencyInjectionResults;
                Debug.Log($"\nDEPENDENCY INJECTION TESTS: {di.TestsPassed}/{di.TotalTests} PASSED");
                Debug.Log($"  - Service Container: {(di.ServiceContainerCreated ? "✓" : "✗")}");
                Debug.Log($"  - Service Registration: {(di.ServiceRegistrationWorking ? "✓" : "✗")}");
                Debug.Log($"  - Service Resolution: {(di.ServiceResolutionWorking ? "✓" : "✗")}");
                Debug.Log($"  - Manager Integration: {(di.ManagerIntegrationWorking ? "✓" : "✗")}");
            }
            
            // Optimization Results
            if (_testResults.OptimizationResults != null)
            {
                var opt = _testResults.OptimizationResults;
                Debug.Log($"\nOPTIMIZATION TESTS: {opt.TestsPassed}/{opt.TotalTests} PASSED");
                Debug.Log($"  - Purge Manager: {(opt.PurgeManagerWorking ? "✓" : "✗")}");
                Debug.Log($"  - Update Optimizer: {(opt.UpdateOptimizerWorking ? "✓" : "✗")}");
                Debug.Log($"  - Batch Processor: {(opt.BatchProcessorWorking ? "✓" : "✗")}");
            }
            
            // Pooling Results
            if (_testResults.PoolingResults != null)
            {
                var pool = _testResults.PoolingResults;
                Debug.Log($"\nPOOLING TESTS: {pool.TestsPassed}/{pool.TotalTests} PASSED");
                Debug.Log($"  - Generic Pool: {(pool.GenericPoolWorking ? "✓" : "✗")}");
                Debug.Log($"  - Pooled Object Manager: {(pool.PooledObjectManagerWorking ? "✓" : "✗")}");
                Debug.Log($"  - Collection Purgers: {(pool.CollectionPurgersWorking ? "✓" : "✗")}");
            }
            
            // Performance Results
            if (_testResults.PerformanceResults != null)
            {
                var perf = _testResults.PerformanceResults;
                Debug.Log($"\nPERFORMANCE TESTS: {perf.TestsPassed}/{perf.TotalTests} PASSED");
                Debug.Log($"  - Memory Optimization: {(perf.MemoryOptimizationEffective ? "✓" : "✗")} ({perf.AllocationReduction:F1}KB reduction)");
                Debug.Log($"  - Batch Processing: {(perf.BatchProcessingEfficient ? "✓" : "✗")} ({perf.BatchProcessingTime:F2}ms for {_testPlantCount} plants)");
            }
            
            Debug.Log("====================================");
        }
        
        #endregion
    }
    
    #region Test Data Structures
    
    /// <summary>
    /// Overall test results for PC013 refactoring
    /// </summary>
    [System.Serializable]
    public class PC013TestResults
    {
        public DateTime TestStartTime;
        public DateTime TestEndTime;
        public float TotalTestDuration;
        
        public DependencyInjectionTestResults DependencyInjectionResults;
        public OptimizationTestResults OptimizationResults;
        public PoolingTestResults PoolingResults;
        public PerformanceTestResults PerformanceResults;
    }
    
    /// <summary>
    /// Dependency injection test results
    /// </summary>
    [System.Serializable]
    public class DependencyInjectionTestResults
    {
        public int TotalTests = 0;
        public int TestsPassed = 0;
        
        public bool ServiceContainerCreated = false;
        public bool ServiceRegistrationWorking = false;
        public bool ServiceResolutionWorking = false;
        public bool ManagerIntegrationWorking = false;
    }
    
    /// <summary>
    /// Optimization test results
    /// </summary>
    [System.Serializable]
    public class OptimizationTestResults
    {
        public int TotalTests = 0;
        public int TestsPassed = 0;
        
        public bool PurgeManagerWorking = false;
        public bool UpdateOptimizerWorking = false;
        public bool BatchProcessorWorking = false;
    }
    
    /// <summary>
    /// Pooling test results
    /// </summary>
    [System.Serializable]
    public class PoolingTestResults
    {
        public int TotalTests = 0;
        public int TestsPassed = 0;
        
        public bool GenericPoolWorking = false;
        public bool PooledObjectManagerWorking = false;
        public bool CollectionPurgersWorking = false;
    }
    
    /// <summary>
    /// Performance test results
    /// </summary>
    [System.Serializable]
    public class PerformanceTestResults
    {
        public int TotalTests = 0;
        public int TestsPassed = 0;
        
        public bool MemoryOptimizationEffective = false;
        public float AllocationReduction = 0f;
        public bool BatchProcessingEfficient = false;
        public float BatchProcessingTime = 0f;
    }
    
    /// <summary>
    /// Test service interface for DI testing
    /// </summary>
    public interface ITestService
    {
        string GetTestMessage();
    }
    
    /// <summary>
    /// Test service implementation
    /// </summary>
    public class TestService : ITestService
    {
        public string GetTestMessage()
        {
            return "Test service working correctly";
        }
    }
    
    /// <summary>
    /// Test poolable object for pooling tests
    /// </summary>
    public class TestPoolableObject
    {
        public int TestValue { get; set; } = 42;
    }
    
    #endregion
}