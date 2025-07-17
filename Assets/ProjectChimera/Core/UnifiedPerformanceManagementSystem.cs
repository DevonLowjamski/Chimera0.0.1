using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Unified performance management system for Project Chimera.
    /// Provides system-wide performance monitoring, optimization coordination,
    /// dynamic quality adjustment, and automated performance management.
    /// 
    /// Phase 3.1 Enhanced Integration Feature - Performance Optimization Across All Systems
    /// </summary>
    public class UnifiedPerformanceManagementSystem : MonoBehaviour
    {
        [Header("Performance Management Configuration")]
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private bool _enableDynamicQualityAdjustment = true;
        [SerializeField] private bool _enableMemoryManagement = true;
        [SerializeField] private float _monitoringIntervalSeconds = 1f;
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _criticalFrameRateThreshold = 30f;

        [Header("Performance Thresholds")]
        [SerializeField] private float _memoryWarningThresholdMB = 1024f;
        [SerializeField] private float _memoryCriticalThresholdMB = 2048f;
        [SerializeField] private float _cpuWarningThresholdMs = 16.67f; // 60 FPS target
        [SerializeField] private float _cpuCriticalThresholdMs = 33.33f; // 30 FPS critical

        // Performance monitoring data
        private Dictionary<string, PerformanceProfile> _systemProfiles = new Dictionary<string, PerformanceProfile>();
        private PerformanceMetrics _globalMetrics = new PerformanceMetrics();
        private Queue<FramePerformanceData> _frameHistory = new Queue<FramePerformanceData>();
        private Dictionary<string, PerformanceOptimizer> _optimizers = new Dictionary<string, PerformanceOptimizer>();
        private List<IPerformanceManagedSystem> _managedSystems = new List<IPerformanceManagedSystem>();

        public static UnifiedPerformanceManagementSystem Instance { get; private set; }

        // Events for performance notifications
        public static event System.Action<PerformanceAlert> OnPerformanceAlert;
        public static event System.Action<PerformanceReport> OnPerformanceReport;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePerformanceSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Performance profile for individual systems.
        /// </summary>
        [System.Serializable]
        public class PerformanceProfile
        {
            public string SystemName;
            public float AverageCpuTime;
            public float PeakCpuTime;
            public long AverageMemoryUsage;
            public long PeakMemoryUsage;
            public int UpdateFrequency;
            public PerformanceQualityLevel CurrentQuality;
            public DateTime LastOptimization;
            public List<float> RecentCpuTimes = new List<float>();
            public List<long> RecentMemoryUsages = new List<long>();
            public int TotalOptimizations;
            public bool IsBottleneck;

            public float CpuEfficiency => AverageCpuTime > 0 ? 1f / AverageCpuTime : 1f;
            public float MemoryEfficiency => AverageMemoryUsage > 0 ? 1000000f / AverageMemoryUsage : 1f;
        }

        /// <summary>
        /// Global performance metrics for the entire application.
        /// </summary>
        [System.Serializable]
        public class PerformanceMetrics
        {
            public float CurrentFrameRate;
            public float AverageFrameRate;
            public float TargetFrameRate;
            public float CpuFrameTime;
            public float GpuFrameTime;
            public long TotalMemoryUsage;
            public long AvailableMemory;
            public float MemoryPressure;
            public int ActiveSystems;
            public int OptimizedSystems;
            public PerformanceState OverallState;
            public DateTime LastUpdate;

            public bool IsPerformanceAcceptable => CurrentFrameRate >= TargetFrameRate * 0.8f;
            public bool IsMemoryUnderPressure => MemoryPressure > 0.8f;
        }

        /// <summary>
        /// Frame-by-frame performance data for trend analysis.
        /// </summary>
        public class FramePerformanceData
        {
            public float FrameTime;
            public float CpuTime;
            public float GpuTime;
            public long MemoryUsage;
            public int ActiveObjects;
            public DateTime Timestamp;
        }

        /// <summary>
        /// Performance optimizer for specific system types.
        /// </summary>
        public class PerformanceOptimizer
        {
            public string SystemType;
            public Func<object, bool> OptimizationFunction;
            public float OptimizationCooldown;
            public float LastOptimizationTime;
            public int OptimizationCount;
            public bool IsEnabled;

            public bool CanOptimize => IsEnabled && (Time.realtimeSinceStartup - LastOptimizationTime) > OptimizationCooldown;
        }

        /// <summary>
        /// Performance alert for notification system.
        /// </summary>
        public class PerformanceAlert
        {
            public PerformanceAlertType Type;
            public string SystemName;
            public string Message;
            public float Severity; // 0-1
            public DateTime Timestamp;
            public string SuggestedAction;
        }

        /// <summary>
        /// Performance report for analysis and debugging.
        /// </summary>
        public class PerformanceReport
        {
            public PerformanceMetrics GlobalMetrics;
            public Dictionary<string, PerformanceProfile> SystemProfiles;
            public List<PerformanceAlert> RecentAlerts;
            public List<string> OptimizationRecommendations;
            public string Summary;
        }

        public enum PerformanceQualityLevel
        {
            Ultra = 4,
            High = 3,
            Medium = 2,
            Low = 1,
            Minimal = 0
        }

        public enum PerformanceState
        {
            Optimal,
            Good,
            Acceptable,
            Poor,
            Critical
        }

        public enum PerformanceAlertType
        {
            Info,
            Warning,
            Critical,
            Optimization
        }

        /// <summary>
        /// Interface for systems that can be performance managed.
        /// </summary>
        public interface IPerformanceManagedSystem
        {
            string SystemName { get; }
            PerformanceQualityLevel CurrentQuality { get; set; }
            bool CanOptimize { get; }
            void OptimizePerformance(PerformanceQualityLevel targetQuality);
            PerformanceProfile GetPerformanceProfile();
        }

        /// <summary>
        /// Initialize the performance management system.
        /// </summary>
        private void InitializePerformanceSystem()
        {
            Application.targetFrameRate = (int)_targetFrameRate;
            QualitySettings.vSyncCount = 0; // Disable VSync for accurate frame rate control
            
            // Register built-in optimizers
            RegisterBuiltInOptimizers();
            
            // Start performance monitoring
            if (_enablePerformanceMonitoring)
            {
                InvokeRepeating(nameof(MonitorPerformance), _monitoringIntervalSeconds, _monitoringIntervalSeconds);
                InvokeRepeating(nameof(AnalyzePerformanceTrends), 5f, 5f);
            }

            ChimeraLogger.Log("Performance", "Unified Performance Management System initialized", this);
        }

        /// <summary>
        /// Register a system for performance management.
        /// </summary>
        public void RegisterManagedSystem(IPerformanceManagedSystem system)
        {
            if (system == null) return;

            if (!_managedSystems.Contains(system))
            {
                _managedSystems.Add(system);
                
                if (!_systemProfiles.ContainsKey(system.SystemName))
                {
                    _systemProfiles[system.SystemName] = new PerformanceProfile
                    {
                        SystemName = system.SystemName,
                        CurrentQuality = system.CurrentQuality
                    };
                }

                ChimeraLogger.Log("Performance", $"Registered system for performance management: {system.SystemName}", this);
            }
        }

        /// <summary>
        /// Unregister a system from performance management.
        /// </summary>
        public void UnregisterManagedSystem(IPerformanceManagedSystem system)
        {
            if (system != null)
            {
                _managedSystems.Remove(system);
                _systemProfiles.Remove(system.SystemName);
            }
        }

        /// <summary>
        /// Register a performance optimizer for a specific system type.
        /// </summary>
        public void RegisterOptimizer(string systemType, Func<object, bool> optimizationFunction, float cooldown = 5f)
        {
            _optimizers[systemType] = new PerformanceOptimizer
            {
                SystemType = systemType,
                OptimizationFunction = optimizationFunction,
                OptimizationCooldown = cooldown,
                IsEnabled = true
            };

            ChimeraLogger.Log("Performance", $"Registered optimizer for: {systemType}", this);
        }

        /// <summary>
        /// Monitor current performance metrics.
        /// </summary>
        private void MonitorPerformance()
        {
            // Update global metrics
            UpdateGlobalMetrics();
            
            // Update system profiles
            UpdateSystemProfiles();
            
            // Record frame data
            RecordFrameData();
            
            // Check for performance issues
            CheckPerformanceThresholds();
            
            // Apply dynamic quality adjustments if enabled
            if (_enableDynamicQualityAdjustment)
            {
                ApplyDynamicQualityAdjustments();
            }
        }

        /// <summary>
        /// Update global performance metrics.
        /// </summary>
        private void UpdateGlobalMetrics()
        {
            _globalMetrics.CurrentFrameRate = 1f / Time.unscaledDeltaTime;
            _globalMetrics.CpuFrameTime = Time.unscaledDeltaTime * 1000f;
            _globalMetrics.TotalMemoryUsage = Profiler.GetTotalReservedMemory();
            _globalMetrics.AvailableMemory = Profiler.GetTotalUnusedReservedMemory();
            _globalMetrics.ActiveSystems = _managedSystems.Count;
            _globalMetrics.OptimizedSystems = _systemProfiles.Values.Count(p => p.TotalOptimizations > 0);
            _globalMetrics.LastUpdate = DateTime.Now;

            // Calculate memory pressure
            var totalSystemMemory = SystemInfo.systemMemorySize * 1024 * 1024; // Convert to bytes
            _globalMetrics.MemoryPressure = (float)_globalMetrics.TotalMemoryUsage / totalSystemMemory;

            // Determine overall performance state
            _globalMetrics.OverallState = DeterminePerformanceState();

            // Update average frame rate (rolling average)
            UpdateRollingAverage(ref _globalMetrics.AverageFrameRate, _globalMetrics.CurrentFrameRate, 60);
        }

        /// <summary>
        /// Update performance profiles for all managed systems.
        /// </summary>
        private void UpdateSystemProfiles()
        {
            foreach (var system in _managedSystems)
            {
                if (_systemProfiles.TryGetValue(system.SystemName, out var profile))
                {
                    var systemProfile = system.GetPerformanceProfile();
                    if (systemProfile != null)
                    {
                        // Update profile with current data
                        UpdateRollingAverage(ref profile.AverageCpuTime, systemProfile.AverageCpuTime, 30);
                        profile.PeakCpuTime = Mathf.Max(profile.PeakCpuTime, systemProfile.AverageCpuTime);
                        
                        profile.AverageMemoryUsage = systemProfile.AverageMemoryUsage;
                        profile.PeakMemoryUsage = Math.Max(profile.PeakMemoryUsage, systemProfile.AverageMemoryUsage);
                        
                        profile.CurrentQuality = system.CurrentQuality;
                        
                        // Track recent performance data
                        profile.RecentCpuTimes.Add(systemProfile.AverageCpuTime);
                        if (profile.RecentCpuTimes.Count > 60) // Keep last 60 samples
                        {
                            profile.RecentCpuTimes.RemoveAt(0);
                        }
                        
                        profile.RecentMemoryUsages.Add(systemProfile.AverageMemoryUsage);
                        if (profile.RecentMemoryUsages.Count > 60)
                        {
                            profile.RecentMemoryUsages.RemoveAt(0);
                        }
                        
                        // Detect bottlenecks
                        profile.IsBottleneck = IsSystemBottleneck(profile);
                    }
                }
            }
        }

        /// <summary>
        /// Record frame performance data for trend analysis.
        /// </summary>
        private void RecordFrameData()
        {
            var frameData = new FramePerformanceData
            {
                FrameTime = Time.unscaledDeltaTime,
                CpuTime = _globalMetrics.CpuFrameTime,
                GpuTime = _globalMetrics.GpuFrameTime,
                MemoryUsage = _globalMetrics.TotalMemoryUsage,
                ActiveObjects = FindObjectsOfType<MonoBehaviour>().Length,
                Timestamp = DateTime.Now
            };

            _frameHistory.Enqueue(frameData);
            
            // Keep only recent frame data (last 300 frames = ~5 minutes at 60 FPS)
            while (_frameHistory.Count > 300)
            {
                _frameHistory.Dequeue();
            }
        }

        /// <summary>
        /// Check performance thresholds and trigger alerts.
        /// </summary>
        private void CheckPerformanceThresholds()
        {
            // Check frame rate
            if (_globalMetrics.CurrentFrameRate < _criticalFrameRateThreshold)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "Global", 
                    $"Critical frame rate: {_globalMetrics.CurrentFrameRate:F1} FPS", 0.9f,
                    "Consider reducing quality settings or optimizing systems");
            }
            else if (_globalMetrics.CurrentFrameRate < _targetFrameRate * 0.8f)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Warning, "Global",
                    $"Low frame rate: {_globalMetrics.CurrentFrameRate:F1} FPS", 0.6f,
                    "Monitor system performance and consider optimizations");
            }

            // Check memory usage
            var memoryUsageMB = _globalMetrics.TotalMemoryUsage / (1024f * 1024f);
            if (memoryUsageMB > _memoryCriticalThresholdMB)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "Memory",
                    $"Critical memory usage: {memoryUsageMB:F1} MB", 0.9f,
                    "Trigger garbage collection or reduce memory allocation");
            }
            else if (memoryUsageMB > _memoryWarningThresholdMB)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Warning, "Memory",
                    $"High memory usage: {memoryUsageMB:F1} MB", 0.6f,
                    "Monitor memory allocation patterns");
            }

            // Check CPU frame time
            if (_globalMetrics.CpuFrameTime > _cpuCriticalThresholdMs)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "CPU",
                    $"Critical CPU frame time: {_globalMetrics.CpuFrameTime:F1} ms", 0.9f,
                    "Optimize CPU-intensive operations or reduce update frequency");
            }
        }

        /// <summary>
        /// Apply dynamic quality adjustments based on performance.
        /// </summary>
        private void ApplyDynamicQualityAdjustments()
        {
            if (_globalMetrics.OverallState == PerformanceState.Critical || 
                _globalMetrics.OverallState == PerformanceState.Poor)
            {
                // Automatically reduce quality for poor performance
                foreach (var system in _managedSystems)
                {
                    if (system.CanOptimize && system.CurrentQuality > PerformanceQualityLevel.Low)
                    {
                        var newQuality = (PerformanceQualityLevel)((int)system.CurrentQuality - 1);
                        system.OptimizePerformance(newQuality);
                        
                        if (_systemProfiles.TryGetValue(system.SystemName, out var profile))
                        {
                            profile.TotalOptimizations++;
                            profile.LastOptimization = DateTime.Now;
                        }

                        ChimeraLogger.Log("Performance", $"Auto-optimized {system.SystemName} to {newQuality}", this);
                    }
                }
            }
            else if (_globalMetrics.OverallState == PerformanceState.Optimal)
            {
                // Gradually increase quality for good performance
                foreach (var system in _managedSystems)
                {
                    if (system.CurrentQuality < PerformanceQualityLevel.Ultra)
                    {
                        var profile = _systemProfiles.GetValueOrDefault(system.SystemName);
                        if (profile != null && (DateTime.Now - profile.LastOptimization).TotalSeconds > 30)
                        {
                            var newQuality = (PerformanceQualityLevel)((int)system.CurrentQuality + 1);
                            system.OptimizePerformance(newQuality);
                            profile.LastOptimization = DateTime.Now;
                            
                            ChimeraLogger.Log("Performance", $"Enhanced quality for {system.SystemName} to {newQuality}", this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Analyze performance trends and generate insights.
        /// </summary>
        private void AnalyzePerformanceTrends()
        {
            if (_frameHistory.Count < 60) return; // Need enough data for analysis

            var recentFrames = _frameHistory.TakeLast(60).ToList();
            var frameTimeVariance = CalculateVariance(recentFrames.Select(f => f.FrameTime));
            var memoryTrend = CalculateTrend(recentFrames.Select(f => (float)f.MemoryUsage));

            // Detect performance patterns
            if (frameTimeVariance > 0.01f) // High frame time variance
            {
                TriggerPerformanceAlert(PerformanceAlertType.Warning, "Stability",
                    "Frame time instability detected", 0.5f,
                    "Check for intermittent performance spikes");
            }

            if (memoryTrend > 0.1f) // Rising memory usage
            {
                TriggerPerformanceAlert(PerformanceAlertType.Info, "Memory",
                    "Memory usage trending upward", 0.3f,
                    "Monitor for potential memory leaks");
            }

            // Generate performance report
            GeneratePerformanceReport();
        }

        /// <summary>
        /// Determine overall performance state based on metrics.
        /// </summary>
        private PerformanceState DeterminePerformanceState()
        {
            var frameRateRatio = _globalMetrics.CurrentFrameRate / _targetFrameRate;
            
            if (frameRateRatio >= 0.95f && _globalMetrics.MemoryPressure < 0.7f)
                return PerformanceState.Optimal;
            else if (frameRateRatio >= 0.85f && _globalMetrics.MemoryPressure < 0.8f)
                return PerformanceState.Good;
            else if (frameRateRatio >= 0.7f && _globalMetrics.MemoryPressure < 0.9f)
                return PerformanceState.Acceptable;
            else if (frameRateRatio >= 0.5f)
                return PerformanceState.Poor;
            else
                return PerformanceState.Critical;
        }

        /// <summary>
        /// Determine if a system is a performance bottleneck.
        /// </summary>
        private bool IsSystemBottleneck(PerformanceProfile profile)
        {
            if (profile.RecentCpuTimes.Count < 10) return false;

            var avgCpuTime = profile.RecentCpuTimes.Average();
            var totalSystemCpuTime = _systemProfiles.Values.Sum(p => p.AverageCpuTime);
            
            // System is a bottleneck if it uses more than 25% of total CPU time
            return avgCpuTime > totalSystemCpuTime * 0.25f;
        }

        /// <summary>
        /// Register built-in performance optimizers.
        /// </summary>
        private void RegisterBuiltInOptimizers()
        {
            // SpeedTree optimization
            RegisterOptimizer("SpeedTree", (system) =>
            {
                // Reduce LOD distances and update frequencies
                return true;
            }, 10f);

            // Genetics system optimization
            RegisterOptimizer("Genetics", (system) =>
            {
                // Reduce calculation frequency for non-critical updates
                return true;
            }, 15f);

            // Environmental system optimization
            RegisterOptimizer("Environmental", (system) =>
            {
                // Reduce sensor update frequency
                return true;
            }, 5f);

            // UI optimization
            RegisterOptimizer("UI", (system) =>
            {
                // Reduce UI update frequency and disable non-essential animations
                return true;
            }, 5f);
        }

        /// <summary>
        /// Trigger a performance alert.
        /// </summary>
        private void TriggerPerformanceAlert(PerformanceAlertType type, string systemName, string message, float severity, string suggestedAction = "")
        {
            var alert = new PerformanceAlert
            {
                Type = type,
                SystemName = systemName,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.Now,
                SuggestedAction = suggestedAction
            };

            OnPerformanceAlert?.Invoke(alert);

            // Log critical and warning alerts
            if (type == PerformanceAlertType.Critical)
            {
                ChimeraLogger.LogError("Performance", $"[{systemName}] {message}", this);
            }
            else if (type == PerformanceAlertType.Warning)
            {
                ChimeraLogger.LogWarning("Performance", $"[{systemName}] {message}", this);
            }
        }

        /// <summary>
        /// Generate comprehensive performance report.
        /// </summary>
        private void GeneratePerformanceReport()
        {
            var recommendations = new List<string>();
            var recentAlerts = new List<PerformanceAlert>(); // Would be populated from alert history

            // Generate optimization recommendations
            foreach (var profile in _systemProfiles.Values)
            {
                if (profile.IsBottleneck)
                {
                    recommendations.Add($"Optimize {profile.SystemName} - identified as performance bottleneck");
                }
                
                if (profile.AverageCpuTime > _cpuWarningThresholdMs * 0.5f)
                {
                    recommendations.Add($"Consider reducing update frequency for {profile.SystemName}");
                }
            }

            var report = new PerformanceReport
            {
                GlobalMetrics = _globalMetrics,
                SystemProfiles = new Dictionary<string, PerformanceProfile>(_systemProfiles),
                RecentAlerts = recentAlerts,
                OptimizationRecommendations = recommendations,
                Summary = GeneratePerformanceSummary()
            };

            OnPerformanceReport?.Invoke(report);
        }

        /// <summary>
        /// Generate performance summary text.
        /// </summary>
        private string GeneratePerformanceSummary()
        {
            return $"Performance State: {_globalMetrics.OverallState} | " +
                   $"FPS: {_globalMetrics.CurrentFrameRate:F1}/{_targetFrameRate} | " +
                   $"Memory: {_globalMetrics.TotalMemoryUsage / (1024 * 1024):F1}MB | " +
                   $"Systems: {_globalMetrics.ActiveSystems} ({_globalMetrics.OptimizedSystems} optimized)";
        }

        /// <summary>
        /// Update rolling average value.
        /// </summary>
        private void UpdateRollingAverage(ref float average, float newValue, int sampleCount)
        {
            if (average == 0)
            {
                average = newValue;
            }
            else
            {
                var alpha = 1f / sampleCount;
                average = (1f - alpha) * average + alpha * newValue;
            }
        }

        /// <summary>
        /// Calculate variance of a data series.
        /// </summary>
        private float CalculateVariance(IEnumerable<float> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count < 2) return 0f;

            var mean = valuesList.Average();
            var variance = valuesList.Sum(v => Mathf.Pow(v - mean, 2)) / valuesList.Count;
            return variance;
        }

        /// <summary>
        /// Calculate trend direction of a data series.
        /// </summary>
        private float CalculateTrend(IEnumerable<float> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count < 2) return 0f;

            var firstHalf = valuesList.Take(valuesList.Count / 2).Average();
            var secondHalf = valuesList.Skip(valuesList.Count / 2).Average();
            
            return (secondHalf - firstHalf) / firstHalf;
        }

        /// <summary>
        /// Force optimization of a specific system.
        /// </summary>
        public void ForceOptimizeSystem(string systemName, PerformanceQualityLevel targetQuality)
        {
            var system = _managedSystems.FirstOrDefault(s => s.SystemName == systemName);
            if (system != null && system.CanOptimize)
            {
                system.OptimizePerformance(targetQuality);
                
                if (_systemProfiles.TryGetValue(systemName, out var profile))
                {
                    profile.TotalOptimizations++;
                    profile.LastOptimization = DateTime.Now;
                }

                ChimeraLogger.Log("Performance", $"Force optimized {systemName} to {targetQuality}", this);
            }
        }

        /// <summary>
        /// Get current performance metrics.
        /// </summary>
        public PerformanceMetrics GetGlobalMetrics()
        {
            return _globalMetrics;
        }

        /// <summary>
        /// Get performance profile for a specific system.
        /// </summary>
        public PerformanceProfile GetSystemProfile(string systemName)
        {
            return _systemProfiles.GetValueOrDefault(systemName);
        }

        /// <summary>
        /// Get all system performance profiles.
        /// </summary>
        public Dictionary<string, PerformanceProfile> GetAllSystemProfiles()
        {
            return new Dictionary<string, PerformanceProfile>(_systemProfiles);
        }

        private void OnDestroy()
        {
            CancelInvoke();
            _systemProfiles.Clear();
            _managedSystems.Clear();
            _optimizers.Clear();
            _frameHistory.Clear();
        }
    }
}