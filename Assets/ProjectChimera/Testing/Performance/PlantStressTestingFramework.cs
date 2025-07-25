using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Optimization;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1a: Comprehensive 1000+ plant stress testing framework
    /// Provides extensive stress testing scenarios for plant simulation systems
    /// </summary>
    public class PlantStressTestingFramework : ChimeraTestBase
    {
        [Header("Stress Test Configuration")]
        [SerializeField] private int _maxPlantCount = 2000;
        [SerializeField] private int _stressTestDuration = 60; // seconds
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _minAcceptableFPS = 45f;
        
        [Header("Test Scenarios")]
        [SerializeField] private bool _enableGradualLoadTest = true;
        [SerializeField] private bool _enableSuddenLoadTest = true;
        [SerializeField] private bool _enableMemoryStressTest = true;
        [SerializeField] private bool _enableSustainedLoadTest = true;
        
        // Test data tracking
        private List<GameObject> _stressTestPlants = new List<GameObject>();
        private List<StressTestResult> _testResults = new List<StressTestResult>();
        private PerformanceOrchestrator _performanceOrchestrator;
        private PlantUpdateOptimizer _plantOptimizer;
        private PlantUpdateScheduler _updateScheduler;
        
        // Performance monitoring
        private float _testStartTime;
        private int _frameCount;
        private float _totalFrameTime;
        private float _worstFrameTime;
        private long _initialMemory;
        private long _peakMemory;
        
        public static PlantStressTestingFramework Instance { get; private set; }
        
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
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            SetupTestEnvironment();
            InitializeStressTestingSystems();
        }
        
        [TearDown]
        public override void TearDown()
        {
            CleanupStressTestPlants();
            base.TearDown();
        }
        
        #endregion
        
        #region Stress Test Scenarios
        
        /// <summary>
        /// PC016-1a-1: Gradual load stress test - Plants added incrementally
        /// </summary>
        [UnityTest]
        [Category("Stress Test")]
        public IEnumerator GradualLoadStressTest()
        {
            if (!_enableGradualLoadTest) yield break;
            
            // Ensure systems are initialized before test
            yield return EnsureSystemsInitialized();
            
            LogInfo($"PC016-1a: Starting gradual load stress test - up to {_maxPlantCount} plants");
            
            var testResult = new StressTestResult
            {
                TestName = "Gradual Load Stress Test",
                MaxPlantCount = _maxPlantCount,
                StartTime = Time.time
            };
            
            // Phase 1: Gradual plant addition (100 plants every 5 seconds)
            int batchSize = 100;
            int batches = _maxPlantCount / batchSize;
            
            for (int batch = 1; batch <= batches; batch++)
            {
                yield return CreatePlantBatch(batchSize, $"GradualBatch_{batch}");
                
                // Monitor performance after each batch
                yield return new WaitForSeconds(2f); // Stabilization time
                var fps = 1f / Time.unscaledDeltaTime;
                
                LogInfo($"PC016-1a: Batch {batch}/{batches} - {_stressTestPlants.Count} plants, FPS: {fps:F1}");
                
                // Record critical performance data
                testResult.PerformanceSnapshots.Add(new PerformanceSnapshot
                {
                    PlantCount = _stressTestPlants.Count,
                    FPS = fps,
                    MemoryUsageMB = GetCurrentMemoryMB(),
                    Timestamp = Time.time - testResult.StartTime
                });
                
                // Stop if performance degrades too much
                if (fps < _minAcceptableFPS && _stressTestPlants.Count > 500)
                {
                    LogWarning($"PC016-1a: Performance degraded at {_stressTestPlants.Count} plants, stopping gradual load test");
                    break;
                }
                
                yield return new WaitForSeconds(3f); // Wait between batches
            }
            
            // Phase 2: Sustained load monitoring
            yield return MonitorSustainedPerformance(testResult, 15f);
            
            testResult.EndTime = Time.time;
            testResult.Success = testResult.MinFPS >= _minAcceptableFPS;
            _testResults.Add(testResult);
            
            LogInfo($"PC016-1a: Gradual load test completed - Final count: {_stressTestPlants.Count}, Min FPS: {testResult.MinFPS:F1}");
        }
        
        /// <summary>
        /// PC016-1a-2: Sudden load stress test - 1000+ plants spawned rapidly
        /// </summary>
        [UnityTest]
        [Category("Stress Test")]
        public IEnumerator SuddenLoadStressTest()
        {
            if (!_enableSuddenLoadTest) yield break;
            
            // Ensure systems are initialized before test
            yield return EnsureSystemsInitialized();
            
            LogInfo("PC016-1a: Starting sudden load stress test - 1000 plants in 10 seconds");
            
            var testResult = new StressTestResult
            {
                TestName = "Sudden Load Stress Test",
                MaxPlantCount = 1000,
                StartTime = Time.time
            };
            
            // Phase 1: Rapid plant creation
            yield return CreatePlantsRapidly(1000, 10f); // 1000 plants over 10 seconds
            
            // Phase 2: Monitor initial shock response
            yield return MonitorInitialShockResponse(testResult, 10f);
            
            // Phase 3: Extended monitoring
            yield return MonitorSustainedPerformance(testResult, 20f);
            
            testResult.EndTime = Time.time;
            testResult.Success = testResult.MinFPS >= _minAcceptableFPS;
            _testResults.Add(testResult);
            
            LogInfo($"PC016-1a: Sudden load test completed - Plants: {_stressTestPlants.Count}, Min FPS: {testResult.MinFPS:F1}");
        }
        
        /// <summary>
        /// PC016-1a-3: Memory stress test - Tests memory usage patterns
        /// </summary>
        [UnityTest]
        [Category("Stress Test")]
        public IEnumerator MemoryStressTest()
        {
            if (!_enableMemoryStressTest) yield break;
            
            // Ensure systems are initialized before test
            yield return EnsureSystemsInitialized();
            
            LogInfo("PC016-1a: Starting memory stress test");
            
            var testResult = new StressTestResult
            {
                TestName = "Memory Stress Test",
                MaxPlantCount = 1500,
                StartTime = Time.time
            };
            
            _initialMemory = System.GC.GetTotalMemory(true);
            
            // Phase 1: Create plants and monitor memory growth
            yield return CreatePlantsWithMemoryMonitoring(1500, testResult);
            
            // Phase 2: Force garbage collection and measure
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
            
            long memoryAfterGC = System.GC.GetTotalMemory(false);
            float memoryIncreaseMB = (memoryAfterGC - _initialMemory) / (1024f * 1024f);
            
            testResult.MemoryIncreaseMB = memoryIncreaseMB;
            testResult.PeakMemoryMB = GetCurrentMemoryMB();
            
            // Phase 3: Memory stability test
            yield return MonitorMemoryStability(testResult, 15f);
            
            testResult.EndTime = Time.time;
            testResult.Success = memoryIncreaseMB < 500f && testResult.MinFPS >= _minAcceptableFPS; // Less than 500MB increase
            _testResults.Add(testResult);
            
            LogInfo($"PC016-1a: Memory stress test completed - Memory increase: {memoryIncreaseMB:F1}MB, Peak: {testResult.PeakMemoryMB:F1}MB");
        }
        
        /// <summary>
        /// PC016-1a-4: Sustained load test - Long duration with 1000+ plants
        /// </summary>
        [UnityTest]
        [Category("Stress Test")]
        public IEnumerator SustainedLoadStressTest()
        {
            if (!_enableSustainedLoadTest) yield break;
            
            // Ensure systems are initialized before test
            yield return EnsureSystemsInitialized();
            
            LogInfo($"PC016-1a: Starting sustained load stress test - {_stressTestDuration} seconds");
            
            var testResult = new StressTestResult
            {
                TestName = "Sustained Load Stress Test",
                MaxPlantCount = 1200,
                StartTime = Time.time
            };
            
            // Phase 1: Create target plant count
            yield return CreatePlantBatch(1200, "SustainedLoad");
            
            // Phase 2: Extended monitoring period
            yield return MonitorSustainedPerformance(testResult, _stressTestDuration);
            
            testResult.EndTime = Time.time;
            testResult.Success = testResult.MinFPS >= _minAcceptableFPS && testResult.AverageFPS >= _minAcceptableFPS + 5f;
            _testResults.Add(testResult);
            
            LogInfo($"PC016-1a: Sustained load test completed - Duration: {_stressTestDuration}s, Avg FPS: {testResult.AverageFPS:F1}");
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Ensure all systems are properly initialized before running tests
        /// </summary>
        private IEnumerator EnsureSystemsInitialized()
        {
            // Wait a frame to ensure all Unity systems are ready
            yield return new WaitForEndOfFrame();
            
            // Check if systems need initialization
            if (_plantOptimizer == null || _updateScheduler == null)
            {
                LogWarning("PC016-1a: Systems not initialized, forcing initialization");
                InitializeStressTestingSystems();
                yield return new WaitForEndOfFrame();
            }
            
            // Verify optimizer has LOD groups initialized
            if (_plantOptimizer != null)
            {
                var stats = _plantOptimizer.GetOptimizerStats();
                if (stats == null || stats.LODCounts.Count < 4) // Should have 4 LOD levels
                {
                    LogWarning("PC016-1a: PlantUpdateOptimizer LOD groups incomplete, forcing re-initialization");
                    _plantOptimizer.Initialize();
                    yield return new WaitForEndOfFrame();
                }
            }
            
            LogInfo("PC016-1a: Systems initialization verified");
        }
        
        private void InitializeStressTestingSystems()
        {
            // Initialize performance monitoring systems
            _performanceOrchestrator = FindObjectOfType<PerformanceOrchestrator>();
            if (_performanceOrchestrator == null)
            {
                var orchestratorGO = new GameObject("StressTest_PerformanceOrchestrator");
                _performanceOrchestrator = orchestratorGO.AddComponent<PerformanceOrchestrator>();
            }
            
            // Initialize PlantUpdateOptimizer BEFORE accessing it
            _plantOptimizer = PlantUpdateOptimizer.Instance;
            if (_plantOptimizer == null)
            {
                var optimizerGO = new GameObject("StressTest_PlantOptimizer");
                _plantOptimizer = optimizerGO.AddComponent<PlantUpdateOptimizer>();
            }
            
            // Ensure the optimizer is properly initialized with LOD groups
            if (_plantOptimizer != null)
            {
                _plantOptimizer.Initialize();
                
                // Force immediate initialization verification
                var stats = _plantOptimizer.GetOptimizerStats();
                if (stats == null || stats.LODCounts.Count == 0)
                {
                    LogWarning("PC016-1a: PlantUpdateOptimizer LOD groups not properly initialized, forcing re-initialization");
                    _plantOptimizer.Initialize();
                }
            }
            
            _updateScheduler = PlantUpdateScheduler.Instance;
            if (_updateScheduler == null)
            {
                var schedulerGO = new GameObject("StressTest_PlantScheduler");
                _updateScheduler = schedulerGO.AddComponent<PlantUpdateScheduler>();
            }
            
            LogInfo("PC016-1a: Stress testing systems initialized");
        }
        
        private IEnumerator CreatePlantBatch(int count, string batchName)
        {
            int plantsCreated = 0;
            int batchSize = 25; // Create 25 plants per frame
            
            while (plantsCreated < count)
            {
                int plantsThisBatch = Mathf.Min(batchSize, count - plantsCreated);
                
                for (int i = 0; i < plantsThisBatch; i++)
                {
                    var plant = CreateStressTestPlant($"{batchName}_{plantsCreated + i}");
                    _stressTestPlants.Add(plant);
                }
                
                plantsCreated += plantsThisBatch;
                yield return new WaitForEndOfFrame(); // Yield to prevent frame drops
            }
            
            LogInfo($"PC016-1a: Created {plantsCreated} plants for {batchName}");
        }
        
        private IEnumerator CreatePlantsRapidly(int count, float duration)
        {
            float plantsPerSecond = count / duration;
            float timeBetweenPlants = 1f / plantsPerSecond;
            float nextPlantTime = Time.time;
            
            for (int i = 0; i < count; i++)
            {
                if (Time.time >= nextPlantTime)
                {
                    var plant = CreateStressTestPlant($"RapidPlant_{i}");
                    _stressTestPlants.Add(plant);
                    nextPlantTime += timeBetweenPlants;
                }
                
                yield return null;
            }
        }
        
        private IEnumerator CreatePlantsWithMemoryMonitoring(int count, StressTestResult testResult)
        {
            int batchSize = 100;
            int batches = count / batchSize;
            
            for (int batch = 0; batch < batches; batch++)
            {
                yield return CreatePlantBatch(batchSize, $"MemoryBatch_{batch}");
                
                // Monitor memory after each batch
                long currentMemory = System.GC.GetTotalMemory(false);
                float memoryMB = currentMemory / (1024f * 1024f);
                
                testResult.PerformanceSnapshots.Add(new PerformanceSnapshot
                {
                    PlantCount = _stressTestPlants.Count,
                    FPS = 1f / Time.unscaledDeltaTime,
                    MemoryUsageMB = memoryMB,
                    Timestamp = Time.time - testResult.StartTime
                });
                
                if (memoryMB > _peakMemory)
                    _peakMemory = (long)memoryMB;
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private GameObject CreateStressTestPlant(string plantName)
        {
            var plant = new GameObject(plantName);
            
            // Add basic components for realistic plant simulation
            var meshRenderer = plant.AddComponent<MeshRenderer>();
            var meshFilter = plant.AddComponent<MeshFilter>();
            
            // Set random position to simulate real plant distribution
            plant.transform.position = new Vector3(
                Random.Range(-100f, 100f),
                0f,
                Random.Range(-100f, 100f)
            );
            
            // Create simple plant mesh for rendering
            meshFilter.mesh = CreateSimplePlantMesh();
            
            // Register with optimization systems with error handling
            try
            {
                _plantOptimizer?.RegisterPlant(plant);
                _updateScheduler?.RegisterPlant(plant);
            }
            catch (System.Exception ex)
            {
                LogWarning($"PC016-1a: Failed to register plant {plantName} with optimization systems: {ex.Message}");
                // Continue without optimization system registration - plant will still function for testing
            }
            
            return plant;
        }
        
        private Mesh CreateSimplePlantMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f),
                new Vector3(0.5f, 2f, 0f), new Vector3(-0.5f, 2f, 0f)
            };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            };
            mesh.RecalculateNormals();
            return mesh;
        }
        
        private IEnumerator MonitorSustainedPerformance(StressTestResult testResult, float duration)
        {
            float startTime = Time.time;
            float minFPS = float.MaxValue;
            float maxFPS = 0f;
            float totalFPS = 0f;
            int sampleCount = 0;
            
            while (Time.time - startTime < duration)
            {
                float currentFPS = 1f / Time.unscaledDeltaTime;
                
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);
                totalFPS += currentFPS;
                sampleCount++;
                
                yield return new WaitForSeconds(0.1f); // Sample every 100ms
            }
            
            testResult.MinFPS = minFPS;
            testResult.MaxFPS = maxFPS;
            testResult.AverageFPS = totalFPS / sampleCount;
        }
        
        private IEnumerator MonitorInitialShockResponse(StressTestResult testResult, float duration)
        {
            // More frequent sampling during initial shock period
            float startTime = Time.time;
            float worstFPS = float.MaxValue;
            
            while (Time.time - startTime < duration)
            {
                float currentFPS = 1f / Time.unscaledDeltaTime;
                worstFPS = Mathf.Min(worstFPS, currentFPS);
                
                testResult.PerformanceSnapshots.Add(new PerformanceSnapshot
                {
                    PlantCount = _stressTestPlants.Count,
                    FPS = currentFPS,
                    MemoryUsageMB = GetCurrentMemoryMB(),
                    Timestamp = Time.time - testResult.StartTime
                });
                
                yield return new WaitForSeconds(0.05f); // Sample every 50ms during shock
            }
            
            testResult.InitialShockMinFPS = worstFPS;
        }
        
        private IEnumerator MonitorMemoryStability(StressTestResult testResult, float duration)
        {
            float startTime = Time.time;
            long startMemory = System.GC.GetTotalMemory(false);
            
            while (Time.time - startTime < duration)
            {
                long currentMemory = System.GC.GetTotalMemory(false);
                float memoryDrift = (currentMemory - startMemory) / (1024f * 1024f);
                
                testResult.MemoryDriftMB = Mathf.Max(testResult.MemoryDriftMB, memoryDrift);
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private float GetCurrentMemoryMB()
        {
            return System.GC.GetTotalMemory(false) / (1024f * 1024f);
        }
        
        private void CleanupStressTestPlants()
        {
            foreach (var plant in _stressTestPlants)
            {
                if (plant != null)
                {
                    try
                    {
                        _plantOptimizer?.UnregisterPlant(plant);
                        _updateScheduler?.UnregisterPlant(plant);
                    }
                    catch (System.Exception ex)
                    {
                        LogWarning($"PC016-1a: Failed to unregister plant {plant.name}: {ex.Message}");
                        // Continue with cleanup regardless
                    }
                    
                    DestroyImmediate(plant);
                }
            }
            _stressTestPlants.Clear();
            
            // Force garbage collection after cleanup
            System.GC.Collect();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Run all stress test scenarios
        /// </summary>
        public IEnumerator RunAllStressTests()
        {
            LogInfo("PC016-1a: Starting comprehensive stress testing suite");
            
            if (_enableGradualLoadTest)
                yield return GradualLoadStressTest();
                
            CleanupStressTestPlants();
            yield return new WaitForSeconds(2f);
            
            if (_enableSuddenLoadTest)
                yield return SuddenLoadStressTest();
                
            CleanupStressTestPlants();
            yield return new WaitForSeconds(2f);
            
            if (_enableMemoryStressTest)
                yield return MemoryStressTest();
                
            CleanupStressTestPlants();
            yield return new WaitForSeconds(2f);
            
            if (_enableSustainedLoadTest)
                yield return SustainedLoadStressTest();
                
            GenerateStressTestReport();
        }
        
        /// <summary>
        /// Generate comprehensive stress test report
        /// </summary>
        public void GenerateStressTestReport()
        {
            LogInfo("PC016-1a: Generating stress test report");
            
            foreach (var result in _testResults)
            {
                LogInfo($"Test: {result.TestName}");
                LogInfo($"  Success: {result.Success}");
                LogInfo($"  Plants: {result.MaxPlantCount}");
                LogInfo($"  Duration: {result.EndTime - result.StartTime:F1}s");
                LogInfo($"  FPS - Min: {result.MinFPS:F1}, Avg: {result.AverageFPS:F1}, Max: {result.MaxFPS:F1}");
                
                if (result.MemoryIncreaseMB > 0)
                    LogInfo($"  Memory: +{result.MemoryIncreaseMB:F1}MB, Peak: {result.PeakMemoryMB:F1}MB");
            }
            
            bool overallSuccess = _testResults.All(r => r.Success);
            LogInfo($"PC016-1a: Overall stress test result: {(overallSuccess ? "PASSED" : "FAILED")}");
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class StressTestResult
    {
        public string TestName;
        public bool Success;
        public int MaxPlantCount;
        public float StartTime;
        public float EndTime;
        public float MinFPS = float.MaxValue;
        public float MaxFPS = 0f;
        public float AverageFPS;
        public float InitialShockMinFPS;
        public float MemoryIncreaseMB;
        public float PeakMemoryMB;
        public float MemoryDriftMB;
        public List<PerformanceSnapshot> PerformanceSnapshots = new List<PerformanceSnapshot>();
    }
    
    [System.Serializable]
    public class PerformanceSnapshot
    {
        public int PlantCount;
        public float FPS;
        public float MemoryUsageMB;
        public float Timestamp;
    }
    
    #endregion
}