using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProjectChimera.Core;
using ProjectChimera.Core.Optimization;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1b: Automated performance benchmarking system
    /// Provides standardized performance measurements and regression detection
    /// </summary>
    public class PerformanceBenchmarkRunner : ChimeraTestBase
    {
        [Header("Benchmark Configuration")]
        [SerializeField] private bool _enableAutomatedBenchmarks = true;
        [SerializeField] private bool _saveResultsToFile = true;
        [SerializeField] private string _benchmarkResultsPath = "Assets/ProjectChimera/Testing/Performance/BenchmarkResults/";
        
        [Header("Performance Targets")]
        [SerializeField] private float _targetFPS = 60f;
        [SerializeField] private float _minimumAcceptableFPS = 45f;
        [SerializeField] private float _maxFrameTimeMs = 16.67f; // 60 FPS = 16.67ms per frame
        [SerializeField] private float _maxMemoryIncreaseMB = 100f;
        
        [Header("Benchmark Scenarios")]
        [SerializeField] private List<BenchmarkScenario> _benchmarkScenarios = new List<BenchmarkScenario>();
        
        // Performance tracking
        private List<BenchmarkResult> _benchmarkResults = new List<BenchmarkResult>();
        private PlantStressTestingFramework _stressTestFramework;
        private PerformanceOrchestrator _performanceOrchestrator;
        
        // Baseline performance data
        private static BenchmarkBaseline _performanceBaseline;
        
        public static PerformanceBenchmarkRunner Instance { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeBenchmarkScenarios();
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
            InitializeBenchmarkingSystems();
            LoadPerformanceBaseline();
        }
        
        [TearDown]
        public override void TearDown()
        {
            if (_saveResultsToFile)
            {
                SaveBenchmarkResults();
            }
            base.TearDown();
        }
        
        #endregion
        
        #region Benchmark Test Methods
        
        /// <summary>
        /// PC016-1b-1: Comprehensive performance benchmark suite
        /// </summary>
        [UnityTest]
        [Category("Performance Benchmark")]
        public IEnumerator RunComprehensivePerformanceBenchmark()
        {
            if (!_enableAutomatedBenchmarks) yield break;
            
            LogInfo("PC016-1b: Starting comprehensive performance benchmark suite");
            
            foreach (var scenario in _benchmarkScenarios)
            {
                if (!scenario.Enabled) continue;
                
                LogInfo($"PC016-1b: Running benchmark scenario: {scenario.Name}");
                yield return RunBenchmarkScenario(scenario);
                
                // Allow system to stabilize between scenarios
                yield return new WaitForSeconds(2f);
                
                // Force garbage collection between scenarios
                System.GC.Collect();
                yield return new WaitForEndOfFrame();
            }
            
            // Generate comprehensive benchmark report
            GenerateBenchmarkReport();
            
            // Perform regression analysis
            PerformRegressionAnalysis();
            
            LogInfo("PC016-1b: Comprehensive performance benchmark completed");
        }
        
        /// <summary>
        /// PC016-1b-2: Baseline performance establishment
        /// </summary>
        [UnityTest]
        [Category("Performance Benchmark")]
        public IEnumerator EstablishPerformanceBaseline()
        {
            LogInfo("PC016-1b: Establishing performance baseline");
            
            var baselineScenario = new BenchmarkScenario
            {
                Name = "Baseline Performance",
                PlantCount = 100,
                Duration = 30f,
                Enabled = true,
                Description = "Clean system baseline with minimal load"
            };
            
            BenchmarkResult baselineResult = null;
            yield return RunBenchmarkScenarioCoroutine(baselineScenario, result => baselineResult = result);
            
            // Create and save new baseline
            _performanceBaseline = new BenchmarkBaseline
            {
                BaselineFPS = baselineResult.AverageFPS,
                BaselineFrameTime = baselineResult.AverageFrameTime,
                BaselineMemoryMB = baselineResult.PeakMemoryMB,
                EstablishedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UnityVersion = Application.unityVersion,
                Platform = Application.platform.ToString()
            };
            
            SavePerformanceBaseline();
            
            LogInfo($"PC016-1b: Performance baseline established - FPS: {_performanceBaseline.BaselineFPS:F1}, " +
                   $"Frame Time: {_performanceBaseline.BaselineFrameTime:F2}ms, Memory: {_performanceBaseline.BaselineMemoryMB:F1}MB");
        }
        
        /// <summary>
        /// PC016-1b-3: Performance regression detection
        /// </summary>
        [UnityTest]
        [Category("Performance Benchmark")]
        public IEnumerator DetectPerformanceRegression()
        {
            if (_performanceBaseline == null)
            {
                LogWarning("PC016-1b: No performance baseline found, establishing new baseline");
                yield return EstablishPerformanceBaseline();
            }
            
            LogInfo("PC016-1b: Running performance regression detection");
            
            // Run current performance test
            var regressionScenario = new BenchmarkScenario
            {
                Name = "Regression Detection",
                PlantCount = 100,
                Duration = 30f,
                Enabled = true,
                Description = "Detect performance regression against baseline"
            };
            
            BenchmarkResult currentResult = null;
            yield return RunBenchmarkScenarioCoroutine(regressionScenario, result => currentResult = result);
            
            // Compare against baseline
            var regressionReport = AnalyzePerformanceRegression(currentResult, _performanceBaseline);
            
            LogInfo($"PC016-1b: Regression analysis completed");
            LogInfo($"  FPS Change: {regressionReport.FPSChangePercent:F1}% ({regressionReport.FPSRegression})");
            LogInfo($"  Frame Time Change: {regressionReport.FrameTimeChangePercent:F1}% ({regressionReport.FrameTimeRegression})");
            LogInfo($"  Memory Change: {regressionReport.MemoryChangePercent:F1}% ({regressionReport.MemoryRegression})");
            
            // Assert no critical regressions
            Assert.IsFalse(regressionReport.CriticalRegression, 
                $"Critical performance regression detected: {regressionReport.Summary}");
        }
        
        #endregion
        
        #region Benchmark Execution
        
        private IEnumerator RunBenchmarkScenario(BenchmarkScenario scenario)
        {
            BenchmarkResult result = null;
            yield return RunBenchmarkScenarioCoroutine(scenario, r => result = r);
            // Result is handled via callback, no return value needed
        }
        
        private IEnumerator RunBenchmarkScenarioCoroutine(BenchmarkScenario scenario, System.Action<BenchmarkResult> onComplete)
        {
            var result = new BenchmarkResult
            {
                ScenarioName = scenario.Name,
                PlantCount = scenario.PlantCount,
                Duration = scenario.Duration,
                StartTime = Time.time,
                PerformanceSnapshots = new List<PerformanceSnapshot>()
            };
            
            LogInfo($"PC016-1b: Starting benchmark scenario '{scenario.Name}' - {scenario.PlantCount} plants for {scenario.Duration}s");
            
            // Pre-benchmark setup
            var startMemory = System.GC.GetTotalMemory(true);
            result.StartMemoryMB = startMemory / (1024f * 1024f);
            
            // Create test plants
            var testPlants = new List<GameObject>();
            yield return CreateBenchmarkPlants(scenario.PlantCount, testPlants);
            
            // Warm-up period
            yield return new WaitForSeconds(2f);
            
            // Start performance monitoring
            var monitoringStartTime = Time.time;
            var performanceData = new List<float>();
            var frameTimeData = new List<float>();
            var memoryData = new List<float>();
            
            // Performance sampling loop
            while (Time.time - monitoringStartTime < scenario.Duration)
            {
                var frameStart = Time.realtimeSinceStartup;
                yield return new WaitForEndOfFrame();
                var frameEnd = Time.realtimeSinceStartup;
                
                var currentFPS = 1f / Time.unscaledDeltaTime;
                var frameTime = (frameEnd - frameStart) * 1000f; // Convert to ms
                var currentMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                
                performanceData.Add(currentFPS);
                frameTimeData.Add(frameTime);
                memoryData.Add(currentMemory);
                
                // Record detailed snapshot every second
                if (performanceData.Count % 60 == 0) // Roughly every second at 60 FPS
                {
                    result.PerformanceSnapshots.Add(new PerformanceSnapshot
                    {
                        PlantCount = testPlants.Count,
                        FPS = currentFPS,
                        MemoryUsageMB = currentMemory,
                        Timestamp = Time.time - result.StartTime
                    });
                }
            }
            
            // Calculate benchmark statistics
            result.EndTime = Time.time;
            result.AverageFPS = performanceData.Average();
            result.MinFPS = performanceData.Min();
            result.MaxFPS = performanceData.Max();
            result.AverageFrameTime = frameTimeData.Average();
            result.MaxFrameTime = frameTimeData.Max();
            result.PeakMemoryMB = memoryData.Max();
            result.EndMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryIncreaseMB = result.EndMemoryMB - result.StartMemoryMB;
            
            // Performance evaluation
            result.PassedFPSTarget = result.AverageFPS >= _targetFPS;
            result.PassedMinFPSTarget = result.MinFPS >= _minimumAcceptableFPS;
            result.PassedFrameTimeTarget = result.AverageFrameTime <= _maxFrameTimeMs;
            result.PassedMemoryTarget = result.MemoryIncreaseMB <= _maxMemoryIncreaseMB;
            result.OverallPass = result.PassedFPSTarget && result.PassedMinFPSTarget && 
                               result.PassedFrameTimeTarget && result.PassedMemoryTarget;
            
            // Cleanup
            CleanupBenchmarkPlants(testPlants);
            
            _benchmarkResults.Add(result);
            
            LogInfo($"PC016-1b: Scenario '{scenario.Name}' completed - " +
                   $"Avg FPS: {result.AverageFPS:F1}, Frame Time: {result.AverageFrameTime:F2}ms, " +
                   $"Memory: +{result.MemoryIncreaseMB:F1}MB, Pass: {result.OverallPass}");
            
            // Call completion callback
            onComplete?.Invoke(result);
        }
        
        private IEnumerator CreateBenchmarkPlants(int count, List<GameObject> plantList)
        {
            int plantsCreated = 0;
            int batchSize = 25;
            
            while (plantsCreated < count)
            {
                int plantsThisBatch = Mathf.Min(batchSize, count - plantsCreated);
                
                for (int i = 0; i < plantsThisBatch; i++)
                {
                    var plant = CreateBenchmarkPlant($"BenchmarkPlant_{plantsCreated + i}");
                    plantList.Add(plant);
                }
                
                plantsCreated += plantsThisBatch;
                yield return new WaitForEndOfFrame();
            }
            
            LogInfo($"PC016-1b: Created {plantsCreated} benchmark plants");
        }
        
        private GameObject CreateBenchmarkPlant(string plantName)
        {
            var plant = new GameObject(plantName);
            
            // Add basic components for realistic simulation
            var meshRenderer = plant.AddComponent<MeshRenderer>();
            var meshFilter = plant.AddComponent<MeshFilter>();
            
            // Random position
            plant.transform.position = new Vector3(
                UnityEngine.Random.Range(-50f, 50f),
                0f,
                UnityEngine.Random.Range(-50f, 50f)
            );
            
            // Create simple mesh
            meshFilter.mesh = CreateSimpleBenchmarkMesh();
            
            return plant;
        }
        
        private Mesh CreateSimpleBenchmarkMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f),
                new Vector3(0.5f, 1f, 0f), new Vector3(-0.5f, 1f, 0f)
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
        
        private void CleanupBenchmarkPlants(List<GameObject> plants)
        {
            foreach (var plant in plants)
            {
                if (plant != null)
                {
                    DestroyImmediate(plant);
                }
            }
            plants.Clear();
            
            // Force garbage collection
            System.GC.Collect();
        }
        
        #endregion
        
        #region Initialization and Configuration
        
        private void InitializeBenchmarkingSystems()
        {
            // Get or create stress testing framework
            _stressTestFramework = FindObjectOfType<PlantStressTestingFramework>();
            if (_stressTestFramework == null)
            {
                var frameworkGO = new GameObject("BenchmarkStressFramework");
                _stressTestFramework = frameworkGO.AddComponent<PlantStressTestingFramework>();
            }
            
            // Get performance orchestrator
            _performanceOrchestrator = FindObjectOfType<PerformanceOrchestrator>();
            if (_performanceOrchestrator == null)
            {
                var orchestratorGO = new GameObject("BenchmarkPerformanceOrchestrator");
                _performanceOrchestrator = orchestratorGO.AddComponent<PerformanceOrchestrator>();
            }
            
            // Ensure results directory exists
            if (_saveResultsToFile && !Directory.Exists(_benchmarkResultsPath))
            {
                Directory.CreateDirectory(_benchmarkResultsPath);
            }
            
            LogInfo("PC016-1b: Benchmarking systems initialized");
        }
        
        private void InitializeBenchmarkScenarios()
        {
            if (_benchmarkScenarios.Count == 0)
            {
                _benchmarkScenarios = new List<BenchmarkScenario>
                {
                    new BenchmarkScenario
                    {
                        Name = "Light Load",
                        PlantCount = 100,
                        Duration = 30f,
                        Enabled = true,
                        Description = "Baseline performance with light plant load"
                    },
                    new BenchmarkScenario
                    {
                        Name = "Medium Load",
                        PlantCount = 500,
                        Duration = 45f,
                        Enabled = true,
                        Description = "Medium load performance testing"
                    },
                    new BenchmarkScenario
                    {
                        Name = "Heavy Load",
                        PlantCount = 1000,
                        Duration = 60f,
                        Enabled = true,
                        Description = "Heavy load performance testing"
                    },
                    new BenchmarkScenario
                    {
                        Name = "Extreme Load",
                        PlantCount = 2000,
                        Duration = 30f,
                        Enabled = true,
                        Description = "Extreme load stress testing"
                    }
                };
            }
        }
        
        #endregion
        
        #region Results Analysis and Reporting
        
        private void GenerateBenchmarkReport()
        {
            LogInfo("PC016-1b: Generating comprehensive benchmark report");
            
            foreach (var result in _benchmarkResults)
            {
                LogInfo($"Benchmark: {result.ScenarioName}");
                LogInfo($"  Plants: {result.PlantCount}, Duration: {result.Duration:F1}s");
                LogInfo($"  FPS - Avg: {result.AverageFPS:F1}, Min: {result.MinFPS:F1}, Max: {result.MaxFPS:F1}");
                LogInfo($"  Frame Time - Avg: {result.AverageFrameTime:F2}ms, Max: {result.MaxFrameTime:F2}ms");
                LogInfo($"  Memory - Peak: {result.PeakMemoryMB:F1}MB, Increase: {result.MemoryIncreaseMB:F1}MB");
                LogInfo($"  Status: {(result.OverallPass ? "PASSED" : "FAILED")}");
                LogInfo($"  Targets - FPS: {result.PassedFPSTarget}, MinFPS: {result.PassedMinFPSTarget}, " +
                       $"FrameTime: {result.PassedFrameTimeTarget}, Memory: {result.PassedMemoryTarget}");
            }
            
            // Overall summary
            var passedCount = _benchmarkResults.Count(r => r.OverallPass);
            var totalCount = _benchmarkResults.Count;
            
            LogInfo($"PC016-1b: Benchmark Summary - {passedCount}/{totalCount} scenarios passed");
            LogInfo($"PC016-1b: Overall benchmark result: {(passedCount == totalCount ? "PASSED" : "FAILED")}");
        }
        
        private void PerformRegressionAnalysis()
        {
            if (_performanceBaseline == null)
            {
                LogWarning("PC016-1b: No baseline available for regression analysis");
                return;
            }
            
            LogInfo("PC016-1b: Performing regression analysis against baseline");
            
            var baselineScenario = _benchmarkResults.FirstOrDefault(r => r.PlantCount <= 100);
            if (baselineScenario != null)
            {
                var regression = AnalyzePerformanceRegression(baselineScenario, _performanceBaseline);
                LogInfo($"PC016-1b: Regression Analysis Results:");
                LogInfo($"  {regression.Summary}");
                
                if (regression.CriticalRegression)
                {
                    LogWarning("PC016-1b: CRITICAL PERFORMANCE REGRESSION DETECTED!");
                }
            }
        }
        
        private RegressionAnalysisResult AnalyzePerformanceRegression(BenchmarkResult current, BenchmarkBaseline baseline)
        {
            var result = new RegressionAnalysisResult();
            
            // FPS analysis
            result.FPSChangePercent = ((current.AverageFPS - baseline.BaselineFPS) / baseline.BaselineFPS) * 100f;
            result.FPSRegression = result.FPSChangePercent < -10f ? "REGRESSION" : 
                                 result.FPSChangePercent > 10f ? "IMPROVEMENT" : "STABLE";
            
            // Frame time analysis
            result.FrameTimeChangePercent = ((current.AverageFrameTime - baseline.BaselineFrameTime) / baseline.BaselineFrameTime) * 100f;
            result.FrameTimeRegression = result.FrameTimeChangePercent > 20f ? "REGRESSION" : 
                                       result.FrameTimeChangePercent < -20f ? "IMPROVEMENT" : "STABLE";
            
            // Memory analysis
            result.MemoryChangePercent = ((current.PeakMemoryMB - baseline.BaselineMemoryMB) / baseline.BaselineMemoryMB) * 100f;
            result.MemoryRegression = result.MemoryChangePercent > 50f ? "REGRESSION" : 
                                    result.MemoryChangePercent < -20f ? "IMPROVEMENT" : "STABLE";
            
            // Overall assessment
            result.CriticalRegression = result.FPSChangePercent < -25f || 
                                      result.FrameTimeChangePercent > 50f || 
                                      result.MemoryChangePercent > 100f;
            
            result.Summary = $"FPS: {result.FPSRegression} ({result.FPSChangePercent:F1}%), " +
                           $"FrameTime: {result.FrameTimeRegression} ({result.FrameTimeChangePercent:F1}%), " +
                           $"Memory: {result.MemoryRegression} ({result.MemoryChangePercent:F1}%)";
            
            return result;
        }
        
        #endregion
        
        #region Data Persistence
        
        private void SaveBenchmarkResults()
        {
            if (!_saveResultsToFile) return;
            
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"BenchmarkResults_{timestamp}.json";
            var filepath = Path.Combine(_benchmarkResultsPath, filename);
            
            try
            {
                var reportData = new BenchmarkReport
                {
                    Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UnityVersion = Application.unityVersion,
                    Platform = Application.platform.ToString(),
                    Results = _benchmarkResults,
                    Summary = new BenchmarkSummary
                    {
                        TotalScenarios = _benchmarkResults.Count,
                        PassedScenarios = _benchmarkResults.Count(r => r.OverallPass),
                        AverageOverallFPS = _benchmarkResults.Average(r => r.AverageFPS),
                        AverageFrameTime = _benchmarkResults.Average(r => r.AverageFrameTime),
                        TotalMemoryUsed = _benchmarkResults.Sum(r => r.MemoryIncreaseMB)
                    }
                };
                
                var json = JsonUtility.ToJson(reportData, true);
                File.WriteAllText(filepath, json);
                
                LogInfo($"PC016-1b: Benchmark results saved to {filepath}");
            }
            catch (System.Exception ex)
            {
                LogError($"PC016-1b: Failed to save benchmark results: {ex.Message}");
            }
        }
        
        private void LoadPerformanceBaseline()
        {
            var baselinePath = Path.Combine(_benchmarkResultsPath, "PerformanceBaseline.json");
            
            if (File.Exists(baselinePath))
            {
                try
                {
                    var json = File.ReadAllText(baselinePath);
                    _performanceBaseline = JsonUtility.FromJson<BenchmarkBaseline>(json);
                    LogInfo($"PC016-1b: Performance baseline loaded - FPS: {_performanceBaseline.BaselineFPS:F1}");
                }
                catch (System.Exception ex)
                {
                    LogWarning($"PC016-1b: Failed to load performance baseline: {ex.Message}");
                }
            }
            else
            {
                LogInfo("PC016-1b: No performance baseline found");
            }
        }
        
        private void SavePerformanceBaseline()
        {
            if (_performanceBaseline == null) return;
            
            var baselinePath = Path.Combine(_benchmarkResultsPath, "PerformanceBaseline.json");
            
            try
            {
                var json = JsonUtility.ToJson(_performanceBaseline, true);
                File.WriteAllText(baselinePath, json);
                LogInfo($"PC016-1b: Performance baseline saved to {baselinePath}");
            }
            catch (System.Exception ex)
            {
                LogError($"PC016-1b: Failed to save performance baseline: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Run specific benchmark scenario programmatically
        /// </summary>
        public IEnumerator RunSpecificBenchmark(string scenarioName)
        {
            var scenario = _benchmarkScenarios.FirstOrDefault(s => s.Name.Equals(scenarioName, System.StringComparison.OrdinalIgnoreCase));
            if (scenario != null)
            {
                yield return RunBenchmarkScenario(scenario);
            }
            else
            {
                LogWarning($"PC016-1b: Benchmark scenario '{scenarioName}' not found");
            }
        }
        
        /// <summary>
        /// Get the latest benchmark results
        /// </summary>
        public List<BenchmarkResult> GetLatestBenchmarkResults()
        {
            return new List<BenchmarkResult>(_benchmarkResults);
        }
        
        /// <summary>
        /// Clear all benchmark results
        /// </summary>
        public void ClearBenchmarkResults()
        {
            _benchmarkResults.Clear();
            LogInfo("PC016-1b: Benchmark results cleared");
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class BenchmarkScenario
    {
        public string Name;
        public string Description;
        public int PlantCount;
        public float Duration;
        public bool Enabled;
    }
    
    [System.Serializable]
    public class BenchmarkResult
    {
        public string ScenarioName;
        public int PlantCount;
        public float Duration;
        public float StartTime;
        public float EndTime;
        public float StartMemoryMB;
        public float EndMemoryMB;
        public float MemoryIncreaseMB;
        public float PeakMemoryMB;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float AverageFrameTime;
        public float MaxFrameTime;
        public bool PassedFPSTarget;
        public bool PassedMinFPSTarget;
        public bool PassedFrameTimeTarget;
        public bool PassedMemoryTarget;
        public bool OverallPass;
        public List<PerformanceSnapshot> PerformanceSnapshots;
    }
    
    [System.Serializable]
    public class BenchmarkBaseline
    {
        public float BaselineFPS;
        public float BaselineFrameTime;
        public float BaselineMemoryMB;
        public string EstablishedDate;
        public string UnityVersion;
        public string Platform;
    }
    
    [System.Serializable]
    public class RegressionAnalysisResult
    {
        public float FPSChangePercent;
        public string FPSRegression;
        public float FrameTimeChangePercent;
        public string FrameTimeRegression;
        public float MemoryChangePercent;
        public string MemoryRegression;
        public bool CriticalRegression;
        public string Summary;
    }
    
    [System.Serializable]
    public class BenchmarkReport
    {
        public string Timestamp;
        public string UnityVersion;
        public string Platform;
        public List<BenchmarkResult> Results;
        public BenchmarkSummary Summary;
    }
    
    [System.Serializable]
    public class BenchmarkSummary
    {
        public int TotalScenarios;
        public int PassedScenarios;
        public float AverageOverallFPS;
        public float AverageFrameTime;
        public float TotalMemoryUsed;
    }
    
    #endregion
}