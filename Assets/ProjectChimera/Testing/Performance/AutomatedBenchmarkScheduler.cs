using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1b: Automated scheduling and execution system for performance benchmarks
    /// Provides continuous integration support and automated performance monitoring
    /// </summary>
    public class AutomatedBenchmarkScheduler : MonoBehaviour
    {
        [Header("Scheduling Configuration")]
        [SerializeField] private bool _enableAutomatedScheduling = false;
        [SerializeField] private float _benchmarkInterval = 3600f; // 1 hour in seconds
        [SerializeField] private bool _runBenchmarkOnStart = false;
        [SerializeField] private bool _runBenchmarkOnBuild = true;
        
        [Header("Performance Monitoring")]
        [SerializeField] private bool _enableContinuousMonitoring = false;
        [SerializeField] private float _monitoringInterval = 300f; // 5 minutes
        [SerializeField] private float _performanceThresholdFPS = 45f;
        [SerializeField] private bool _triggerBenchmarkOnPerformanceDrop = true;
        
        [Header("Runtime Controls")]
        [SerializeField] private KeyCode _manualBenchmarkKey = KeyCode.F9;
        [SerializeField] private KeyCode _baselineEstablishKey = KeyCode.F10;
        [SerializeField] private bool _showRuntimeUI = true;
        
        [Header("Integration Settings")]
        [SerializeField] private bool _enableCIIntegration = false;
        [SerializeField] private string _ciResultsWebhook = "";
        [SerializeField] private bool _failBuildOnRegressions = false;
        
        private PerformanceBenchmarkRunner _benchmarkRunner;
        private Coroutine _scheduledBenchmarkCoroutine;
        private Coroutine _continuousMonitoringCoroutine;
        private bool _benchmarkInProgress = false;
        private float _lastBenchmarkTime = 0f;
        
        // Performance monitoring data
        private List<float> _recentPerformanceData = new List<float>();
        private float _lastPerformanceCheck = 0f;
        
        public static AutomatedBenchmarkScheduler Instance { get; private set; }
        
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
            InitializeBenchmarkScheduler();
            
            if (_runBenchmarkOnStart)
            {
                StartCoroutine(DelayedBenchmarkStart());
            }
        }
        
        private void Update()
        {
            HandleRuntimeControls();
            UpdatePerformanceMonitoring();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _enableAutomatedScheduling)
            {
                // Resume scheduling when application regains focus
                RestartScheduledBenchmarks();
            }
        }
        
        private void OnDestroy()
        {
            StopAllBenchmarks();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeBenchmarkScheduler()
        {
            // Find or create benchmark runner
            _benchmarkRunner = FindObjectOfType<PerformanceBenchmarkRunner>();
            if (_benchmarkRunner == null)
            {
                var runnerGO = new GameObject("PerformanceBenchmarkRunner");
                _benchmarkRunner = runnerGO.AddComponent<PerformanceBenchmarkRunner>();
            }
            
            // Start automated scheduling if enabled
            if (_enableAutomatedScheduling)
            {
                StartScheduledBenchmarks();
            }
            
            // Start continuous monitoring if enabled
            if (_enableContinuousMonitoring)
            {
                StartContinuousMonitoring();
            }
            
            Debug.Log($"[AutomatedBenchmarkScheduler] Initialized - " +
                     $"Automated: {_enableAutomatedScheduling}, Monitoring: {_enableContinuousMonitoring}");
        }
        
        private IEnumerator DelayedBenchmarkStart()
        {
            // Wait for systems to initialize
            yield return new WaitForSeconds(5f);
            
            Debug.Log("[AutomatedBenchmarkScheduler] Running startup benchmark");
            yield return RunScheduledBenchmark("Startup Benchmark");
        }
        
        #endregion
        
        #region Scheduled Benchmarking
        
        private void StartScheduledBenchmarks()
        {
            if (_scheduledBenchmarkCoroutine != null)
            {
                StopCoroutine(_scheduledBenchmarkCoroutine);
            }
            
            _scheduledBenchmarkCoroutine = StartCoroutine(ScheduledBenchmarkLoop());
            Debug.Log($"[AutomatedBenchmarkScheduler] Started scheduled benchmarks - Interval: {_benchmarkInterval}s");
        }
        
        private void RestartScheduledBenchmarks()
        {
            if (_enableAutomatedScheduling && _scheduledBenchmarkCoroutine == null)
            {
                StartScheduledBenchmarks();
            }
        }
        
        private IEnumerator ScheduledBenchmarkLoop()
        {
            while (_enableAutomatedScheduling)
            {
                yield return new WaitForSeconds(_benchmarkInterval);
                
                if (!_benchmarkInProgress)
                {
                    Debug.Log("[AutomatedBenchmarkScheduler] Running scheduled benchmark");
                    yield return RunScheduledBenchmark("Scheduled Benchmark");
                }
                else
                {
                    Debug.Log("[AutomatedBenchmarkScheduler] Skipping scheduled benchmark - another benchmark in progress");
                }
            }
        }
        
        private IEnumerator RunScheduledBenchmark(string benchmarkName)
        {
            if (_benchmarkInProgress) yield break;
            
            _benchmarkInProgress = true;
            _lastBenchmarkTime = Time.time;
            
            Debug.Log($"[AutomatedBenchmarkScheduler] Starting {benchmarkName}");
            
            // Run comprehensive benchmark
            yield return _benchmarkRunner.RunComprehensivePerformanceBenchmark();
            
            // Get results for analysis
            var results = _benchmarkRunner.GetLatestBenchmarkResults();
            
            // Handle CI integration
            if (_enableCIIntegration)
            {
                yield return HandleCIIntegration(results);
            }
            
            Debug.Log($"[AutomatedBenchmarkScheduler] {benchmarkName} completed successfully");
            
            _benchmarkInProgress = false;
        }
        
        #endregion
        
        #region Continuous Performance Monitoring
        
        private void StartContinuousMonitoring()
        {
            if (_continuousMonitoringCoroutine != null)
            {
                StopCoroutine(_continuousMonitoringCoroutine);
            }
            
            _continuousMonitoringCoroutine = StartCoroutine(ContinuousMonitoringLoop());
            Debug.Log($"[AutomatedBenchmarkScheduler] Started continuous monitoring - Interval: {_monitoringInterval}s");
        }
        
        private IEnumerator ContinuousMonitoringLoop()
        {
            while (_enableContinuousMonitoring)
            {
                yield return new WaitForSeconds(_monitoringInterval);
                
                // Check current performance
                var currentFPS = 1f / Time.unscaledDeltaTime;
                _recentPerformanceData.Add(currentFPS);
                
                // Keep only recent data (last 10 samples)
                if (_recentPerformanceData.Count > 10)
                {
                    _recentPerformanceData.RemoveAt(0);
                }
                
                // Analyze performance trend
                if (_recentPerformanceData.Count >= 5)
                {
                    var averageRecentFPS = 0f;
                    foreach (var fps in _recentPerformanceData)
                    {
                        averageRecentFPS += fps;
                    }
                    averageRecentFPS /= _recentPerformanceData.Count;
                    
                    // Check for performance degradation
                    if (averageRecentFPS < _performanceThresholdFPS && _triggerBenchmarkOnPerformanceDrop)
                    {
                        Debug.LogWarning($"[AutomatedBenchmarkScheduler] Performance degradation detected - " +
                                       $"Average FPS: {averageRecentFPS:F1}, Threshold: {_performanceThresholdFPS:F1}");
                        
                        // Trigger benchmark if not already running
                        if (!_benchmarkInProgress)
                        {
                            StartCoroutine(RunScheduledBenchmark("Performance Degradation Benchmark"));
                        }
                    }
                }
            }
        }
        
        private void UpdatePerformanceMonitoring()
        {
            if (_enableContinuousMonitoring && Time.time - _lastPerformanceCheck > 1f)
            {
                _lastPerformanceCheck = Time.time;
                
                // Log performance metrics periodically (every 60 seconds)
                if (Mathf.FloorToInt(Time.time) % 60 == 0)
                {
                    var currentFPS = 1f / Time.unscaledDeltaTime;
                    var memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                    
                    Debug.Log($"[AutomatedBenchmarkScheduler] Performance Check - " +
                             $"FPS: {currentFPS:F1}, Memory: {memoryUsage:F1}MB");
                }
            }
        }
        
        #endregion
        
        #region Runtime Controls
        
        private void HandleRuntimeControls()
        {
            if (Input.GetKeyDown(_manualBenchmarkKey) && !_benchmarkInProgress)
            {
                StartCoroutine(RunScheduledBenchmark("Manual Benchmark"));
            }
            
            if (Input.GetKeyDown(_baselineEstablishKey) && !_benchmarkInProgress)
            {
                StartCoroutine(EstablishBaselineFromInput());
            }
        }
        
        private IEnumerator EstablishBaselineFromInput()
        {
            _benchmarkInProgress = true;
            
            Debug.Log("[AutomatedBenchmarkScheduler] Establishing performance baseline from user input");
            
            yield return _benchmarkRunner.EstablishPerformanceBaseline();
            Debug.Log("[AutomatedBenchmarkScheduler] Performance baseline established successfully");
            
            _benchmarkInProgress = false;
        }
        
        #endregion
        
        #region CI Integration
        
        private IEnumerator HandleCIIntegration(List<BenchmarkResult> results)
        {
            if (!_enableCIIntegration) yield break;
            
            Debug.Log("[AutomatedBenchmarkScheduler] Processing CI integration");
            
            // Analyze results for CI
            var overallPass = true;
            var criticalIssues = new List<string>();
            
            foreach (var result in results)
            {
                if (!result.OverallPass)
                {
                    overallPass = false;
                    criticalIssues.Add($"{result.ScenarioName}: Failed performance targets");
                }
            }
            
            // Send results to webhook if configured
            if (!string.IsNullOrEmpty(_ciResultsWebhook))
            {
                yield return SendResultsToWebhook(results, overallPass, criticalIssues);
            }
            
            // Handle build failure condition
            if (_failBuildOnRegressions && !overallPass)
            {
                var errorMessage = $"Build failed due to performance regressions: {string.Join(", ", criticalIssues)}";
                Debug.LogError($"[AutomatedBenchmarkScheduler] {errorMessage}");
                
                // In a real CI environment, this would terminate the build
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.Exit(1);
                #endif
            }
            
            Debug.Log($"[AutomatedBenchmarkScheduler] CI integration completed - Pass: {overallPass}");
        }
        
        private IEnumerator SendResultsToWebhook(List<BenchmarkResult> results, bool overallPass, List<string> issues)
        {
            // In a real implementation, this would send HTTP requests to CI systems
            Debug.Log($"[AutomatedBenchmarkScheduler] Would send results to webhook: {_ciResultsWebhook}");
            Debug.Log($"  Overall Pass: {overallPass}");
            Debug.Log($"  Issues: {string.Join(", ", issues)}");
            
            yield return new WaitForSeconds(0.1f); // Simulate network delay
        }
        
        #endregion
        
        #region Runtime UI
        
        private void OnGUI()
        {
            if (!_showRuntimeUI) return;
            
            var rect = new Rect(Screen.width - 320, 10, 300, 200);
            GUILayout.BeginArea(rect);
            
            GUILayout.Label("Automated Benchmark Scheduler", GUI.skin.label);
            GUILayout.Space(5);
            
            // Status information
            GUILayout.Label($"Benchmark In Progress: {_benchmarkInProgress}");
            GUILayout.Label($"Last Benchmark: {(Time.time - _lastBenchmarkTime):F0}s ago");
            GUILayout.Label($"Scheduled Benchmarks: {_enableAutomatedScheduling}");
            GUILayout.Label($"Continuous Monitoring: {_enableContinuousMonitoring}");
            
            GUILayout.Space(10);
            
            // Control buttons
            if (!_benchmarkInProgress)
            {
                if (GUILayout.Button("Run Manual Benchmark"))
                {
                    StartCoroutine(RunScheduledBenchmark("GUI Manual Benchmark"));
                }
                
                if (GUILayout.Button("Establish Baseline"))
                {
                    StartCoroutine(EstablishBaselineFromInput());
                }
                
                if (GUILayout.Button("Toggle Scheduled Benchmarks"))
                {
                    _enableAutomatedScheduling = !_enableAutomatedScheduling;
                    if (_enableAutomatedScheduling)
                    {
                        StartScheduledBenchmarks();
                    }
                    else if (_scheduledBenchmarkCoroutine != null)
                    {
                        StopCoroutine(_scheduledBenchmarkCoroutine);
                        _scheduledBenchmarkCoroutine = null;
                    }
                }
                
                if (GUILayout.Button("Toggle Monitoring"))
                {
                    _enableContinuousMonitoring = !_enableContinuousMonitoring;
                    if (_enableContinuousMonitoring)
                    {
                        StartContinuousMonitoring();
                    }
                    else if (_continuousMonitoringCoroutine != null)
                    {
                        StopCoroutine(_continuousMonitoringCoroutine);
                        _continuousMonitoringCoroutine = null;
                    }
                }
            }
            else
            {
                GUILayout.Label("Benchmark Running...");
                if (GUILayout.Button("Stop All Benchmarks"))
                {
                    StopAllBenchmarks();
                }
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Manually trigger a benchmark run
        /// </summary>
        public void TriggerManualBenchmark()
        {
            if (!_benchmarkInProgress)
            {
                StartCoroutine(RunScheduledBenchmark("API Manual Benchmark"));
            }
        }
        
        /// <summary>
        /// Enable or disable automated scheduling
        /// </summary>
        public void SetAutomatedScheduling(bool enabled)
        {
            _enableAutomatedScheduling = enabled;
            
            if (enabled)
            {
                StartScheduledBenchmarks();
            }
            else
            {
                StopScheduledBenchmarks();
            }
        }
        
        /// <summary>
        /// Enable or disable continuous monitoring
        /// </summary>
        public void SetContinuousMonitoring(bool enabled)
        {
            _enableContinuousMonitoring = enabled;
            
            if (enabled)
            {
                StartContinuousMonitoring();
            }
            else
            {
                StopContinuousMonitoring();
            }
        }
        
        /// <summary>
        /// Stop all running benchmarks and monitoring
        /// </summary>
        public void StopAllBenchmarks()
        {
            StopScheduledBenchmarks();
            StopContinuousMonitoring();
            _benchmarkInProgress = false;
            
            Debug.Log("[AutomatedBenchmarkScheduler] All benchmarks and monitoring stopped");
        }
        
        /// <summary>
        /// Get current performance monitoring status
        /// </summary>
        public bool IsBenchmarkInProgress()
        {
            return _benchmarkInProgress;
        }
        
        /// <summary>
        /// Get recent performance data
        /// </summary>
        public List<float> GetRecentPerformanceData()
        {
            return new List<float>(_recentPerformanceData);
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void StopScheduledBenchmarks()
        {
            if (_scheduledBenchmarkCoroutine != null)
            {
                StopCoroutine(_scheduledBenchmarkCoroutine);
                _scheduledBenchmarkCoroutine = null;
            }
        }
        
        private void StopContinuousMonitoring()
        {
            if (_continuousMonitoringCoroutine != null)
            {
                StopCoroutine(_continuousMonitoringCoroutine);
                _continuousMonitoringCoroutine = null;
            }
        }
        
        #endregion
    }
}