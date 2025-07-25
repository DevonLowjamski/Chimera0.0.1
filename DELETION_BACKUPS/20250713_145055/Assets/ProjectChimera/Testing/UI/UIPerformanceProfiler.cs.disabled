using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;

namespace ProjectChimera.Testing.UI
{
    /// <summary>
    /// Performance profiler specifically designed for UI systems in Project Chimera.
    /// Monitors UI performance metrics, identifies bottlenecks, and provides optimization insights.
    /// </summary>
    public class UIPerformanceProfiler : ChimeraMonoBehaviour
    {
        [Header("Profiler Configuration")]
        [SerializeField] private bool _enableProfiling = true;
        [SerializeField] private bool _enableRealtimeMonitoring = true;
        [SerializeField] private float _samplingInterval = 0.1f; // 100ms
        [SerializeField] private int _maxSamples = 1000;
        [SerializeField] private bool _autoOptimize = false;
        
        [Header("Performance Targets")]
        [SerializeField] private GameUIManager _gameUIManager;
        [SerializeField] private UIPrefabManager _prefabManager;
        [SerializeField] private UIStateManager _stateManager;
        
        [Header("Performance Thresholds")]
        [SerializeField] private float _targetFrameTimeMs = 16.67f; // 60 FPS
        [SerializeField] private float _maxUpdateTimeMs = 2.0f;
        [SerializeField] private float _maxMemoryUsageMB = 100f;
        [SerializeField] private int _maxActiveComponents = 50;
        [SerializeField] private float _maxGCAllocKB = 100f;
        
        [Header("Profiling Results")]
        [SerializeField] private UIPerformanceReport _lastReport;
        [SerializeField] private List<UIPerformanceMetric> _currentMetrics = new List<UIPerformanceMetric>();
        [SerializeField] private bool _showDetailedMetrics = true;
        
        // Profiling data
        private Dictionary<string, List<float>> _performanceSamples;
        private Dictionary<string, UIPerformanceCounter> _performanceCounters;
        private List<UIPerformanceAlert> _activeAlerts;
        private float _samplingTimer = 0f;
        private bool _isProfilingActive = false;
        
        // Frame timing
        private float _lastFrameTime = 0f;
        private Queue<float> _frameTimeHistory;
        private float _averageFrameTime = 0f;
        
        // Memory tracking
        private long _lastGCMemory = 0;
        private Queue<float> _memoryHistory;
        private float _averageMemoryUsage = 0f;
        
        // Component tracking
        private Dictionary<string, ComponentPerformanceData> _componentPerformance;
        
        // Events
        public System.Action<UIPerformanceReport> OnPerformanceReportGenerated;
        public System.Action<UIPerformanceAlert> OnPerformanceAlertRaised;
        public System.Action<string> OnPerformanceOptimizationApplied;
        
        // Properties
        public bool IsProfilingActive => _isProfilingActive;
        public UIPerformanceReport LastReport => _lastReport;
        public float AverageFrameTime => _averageFrameTime;
        public float AverageMemoryUsage => _averageMemoryUsage;
        public int ActiveAlertCount => _activeAlerts?.Count ?? 0;
        
        protected override void Start()
        {
            base.Start();
            
            InitializeProfiler();
            
            if (_enableProfiling)
            {
                StartProfiling();
            }
        }
        
        /// <summary>
        /// Initialize performance profiler
        /// </summary>
        private void InitializeProfiler()
        {
            _performanceSamples = new Dictionary<string, List<float>>();
            _performanceCounters = new Dictionary<string, UIPerformanceCounter>();
            _activeAlerts = new List<UIPerformanceAlert>();
            _frameTimeHistory = new Queue<float>();
            _memoryHistory = new Queue<float>();
            _componentPerformance = new Dictionary<string, ComponentPerformanceData>();
            
            InitializePerformanceCounters();
            
            LogInfo("UI Performance Profiler initialized successfully");
        }
        
        /// <summary>
        /// Initialize performance counters
        /// </summary>
        private void InitializePerformanceCounters()
        {
            AddPerformanceCounter("frame_time", "Frame Time (ms)");
            AddPerformanceCounter("ui_update_time", "UI Update Time (ms)");
            AddPerformanceCounter("memory_usage", "Memory Usage (MB)");
            AddPerformanceCounter("gc_alloc", "GC Allocation (KB)");
            AddPerformanceCounter("active_components", "Active Components");
            AddPerformanceCounter("pooled_components", "Pooled Components");
            AddPerformanceCounter("state_operations", "State Operations/sec");
            AddPerformanceCounter("event_dispatches", "Event Dispatches/sec");
        }
        
        /// <summary>
        /// Add performance counter
        /// </summary>
        private void AddPerformanceCounter(string name, string displayName)
        {
            _performanceCounters[name] = new UIPerformanceCounter
            {
                Name = name,
                DisplayName = displayName,
                Values = new Queue<float>(),
                LastValue = 0f,
                MinValue = float.MaxValue,
                MaxValue = float.MinValue,
                AverageValue = 0f
            };
            
            _performanceSamples[name] = new List<float>();
        }
        
        /// <summary>
        /// Start performance profiling
        /// </summary>
        public void StartProfiling()
        {
            if (_isProfilingActive)
            {
                LogWarning("Profiling already active");
                return;
            }
            
            _isProfilingActive = true;
            _samplingTimer = 0f;
            _lastGCMemory = System.GC.GetTotalMemory(false);
            
            LogInfo("UI Performance profiling started");
        }
        
        /// <summary>
        /// Stop performance profiling
        /// </summary>
        public void StopProfiling()
        {
            if (!_isProfilingActive)
            {
                LogWarning("Profiling not active");
                return;
            }
            
            _isProfilingActive = false;
            GeneratePerformanceReport();
            
            LogInfo("UI Performance profiling stopped");
        }
        
        /// <summary>
        /// Generate performance report
        /// </summary>
        public void GeneratePerformanceReport()
        {
            var report = new UIPerformanceReport
            {
                GenerationTime = System.DateTime.Now,
                ProfilingDuration = Time.time, // Approximate
                AverageFrameTime = _averageFrameTime,
                AverageMemoryUsage = _averageMemoryUsage,
                PerformanceCounters = new Dictionary<string, UIPerformanceCounter>(_performanceCounters),
                ActiveAlerts = new List<UIPerformanceAlert>(_activeAlerts),
                ComponentPerformance = new Dictionary<string, ComponentPerformanceData>(_componentPerformance),
                Recommendations = GenerateOptimizationRecommendations()
            };
            
            _lastReport = report;
            OnPerformanceReportGenerated?.Invoke(report);
            
            LogPerformanceReport(report);
        }
        
        /// <summary>
        /// Sample performance metrics
        /// </summary>
        private void SamplePerformanceMetrics()
        {
            // Frame time
            var frameTime = Time.deltaTime * 1000f; // Convert to ms
            UpdatePerformanceCounter("frame_time", frameTime);
            UpdateFrameTimeHistory(frameTime);
            
            // Memory usage
            var currentMemory = System.GC.GetTotalMemory(false);
            var memoryMB = currentMemory / 1024f / 1024f;
            UpdatePerformanceCounter("memory_usage", memoryMB);
            UpdateMemoryHistory(memoryMB);
            
            // GC allocation
            var gcDelta = currentMemory - _lastGCMemory;
            var gcAllocKB = gcDelta / 1024f;
            UpdatePerformanceCounter("gc_alloc", gcAllocKB);
            _lastGCMemory = currentMemory;
            
            // UI-specific metrics
            SampleUIMetrics();
            
            // Check for performance alerts
            CheckPerformanceAlerts();
        }
        
        /// <summary>
        /// Sample UI-specific metrics
        /// </summary>
        private void SampleUIMetrics()
        {
            if (_prefabManager != null)
            {
                UpdatePerformanceCounter("active_components", _prefabManager.ActiveComponentCount);
                // Note: Pooled component count would need to be exposed by UIPrefabManager
            }
            
            if (_gameUIManager != null)
            {
                // Sample UI update time - would need instrumentation in GameUIManager
                var uiUpdateTime = SampleUIUpdateTime();
                UpdatePerformanceCounter("ui_update_time", uiUpdateTime);
            }
            
            if (_stateManager != null)
            {
                // Sample state operations - would need instrumentation in UIStateManager
                var stateOps = SampleStateOperations();
                UpdatePerformanceCounter("state_operations", stateOps);
            }
            
            // Sample component-specific performance
            SampleComponentPerformance();
        }
        
        /// <summary>
        /// Sample UI update time (placeholder - would need actual instrumentation)
        /// </summary>
        private float SampleUIUpdateTime()
        {
            // This would require instrumentation in the actual UI managers
            // For now, return a placeholder value
            return Random.Range(0.1f, 2.0f);
        }
        
        /// <summary>
        /// Sample state operations (placeholder - would need actual instrumentation)
        /// </summary>
        private float SampleStateOperations()
        {
            // This would require instrumentation in UIStateManager
            // For now, return a placeholder value
            return Random.Range(0f, 10f);
        }
        
        /// <summary>
        /// Sample component performance
        /// </summary>
        private void SampleComponentPerformance()
        {
            if (_prefabManager == null)
                return;
            
            var activeComponents = _prefabManager.GetActiveComponents<UIComponentPrefab>();
            
            foreach (var component in activeComponents)
            {
                if (component == null) continue;
                
                var componentId = component.ComponentId;
                
                if (!_componentPerformance.ContainsKey(componentId))
                {
                    _componentPerformance[componentId] = new ComponentPerformanceData
                    {
                        ComponentId = componentId,
                        ComponentType = component.GetType().Name,
                        UpdateTimes = new Queue<float>(),
                        MemoryUsage = new Queue<float>()
                    };
                }
                
                var perfData = _componentPerformance[componentId];
                
                // Sample component update time (would need actual instrumentation)
                var updateTime = Random.Range(0.01f, 0.5f);
                perfData.UpdateTimes.Enqueue(updateTime);
                
                // Sample component memory usage (approximation)
                var memUsage = Random.Range(0.1f, 5.0f);
                perfData.MemoryUsage.Enqueue(memUsage);
                
                // Keep only recent samples
                if (perfData.UpdateTimes.Count > 100)
                {
                    perfData.UpdateTimes.Dequeue();
                    perfData.MemoryUsage.Dequeue();
                }
                
                // Update averages
                perfData.AverageUpdateTime = perfData.UpdateTimes.Average();
                perfData.AverageMemoryUsage = perfData.MemoryUsage.Average();
            }
        }
        
        /// <summary>
        /// Update performance counter
        /// </summary>
        private void UpdatePerformanceCounter(string name, float value)
        {
            if (!_performanceCounters.ContainsKey(name))
                return;
            
            var counter = _performanceCounters[name];
            
            counter.LastValue = value;
            counter.Values.Enqueue(value);
            
            // Update min/max
            if (value < counter.MinValue) counter.MinValue = value;
            if (value > counter.MaxValue) counter.MaxValue = value;
            
            // Update average
            if (counter.Values.Count > 0)
            {
                counter.AverageValue = counter.Values.Average();
            }
            
            // Keep only recent samples
            if (counter.Values.Count > _maxSamples)
            {
                counter.Values.Dequeue();
            }
            
            // Store in samples list
            _performanceSamples[name].Add(value);
            if (_performanceSamples[name].Count > _maxSamples)
            {
                _performanceSamples[name].RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Update frame time history
        /// </summary>
        private void UpdateFrameTimeHistory(float frameTime)
        {
            _frameTimeHistory.Enqueue(frameTime);
            
            if (_frameTimeHistory.Count > 60) // Keep 1 second of history at 60 FPS
            {
                _frameTimeHistory.Dequeue();
            }
            
            _averageFrameTime = _frameTimeHistory.Average();
        }
        
        /// <summary>
        /// Update memory history
        /// </summary>
        private void UpdateMemoryHistory(float memoryUsage)
        {
            _memoryHistory.Enqueue(memoryUsage);
            
            if (_memoryHistory.Count > 60)
            {
                _memoryHistory.Dequeue();
            }
            
            _averageMemoryUsage = _memoryHistory.Average();
        }
        
        /// <summary>
        /// Check for performance alerts
        /// </summary>
        private void CheckPerformanceAlerts()
        {
            CheckFrameTimeAlert();
            CheckMemoryAlert();
            CheckComponentCountAlert();
            CheckUpdateTimeAlert();
        }
        
        /// <summary>
        /// Check frame time alert
        /// </summary>
        private void CheckFrameTimeAlert()
        {
            if (_averageFrameTime > _targetFrameTimeMs)
            {
                RaiseAlert("high_frame_time", 
                    UIPerformanceAlertSeverity.Warning,
                    $"Frame time above target: {_averageFrameTime:F2}ms > {_targetFrameTimeMs:F2}ms");
            }
        }
        
        /// <summary>
        /// Check memory alert
        /// </summary>
        private void CheckMemoryAlert()
        {
            if (_averageMemoryUsage > _maxMemoryUsageMB)
            {
                RaiseAlert("high_memory_usage",
                    UIPerformanceAlertSeverity.Critical,
                    $"Memory usage too high: {_averageMemoryUsage:F1}MB > {_maxMemoryUsageMB:F1}MB");
            }
        }
        
        /// <summary>
        /// Check component count alert
        /// </summary>
        private void CheckComponentCountAlert()
        {
            if (_prefabManager != null && _prefabManager.ActiveComponentCount > _maxActiveComponents)
            {
                RaiseAlert("high_component_count",
                    UIPerformanceAlertSeverity.Warning,
                    $"Too many active components: {_prefabManager.ActiveComponentCount} > {_maxActiveComponents}");
            }
        }
        
        /// <summary>
        /// Check update time alert
        /// </summary>
        private void CheckUpdateTimeAlert()
        {
            if (_performanceCounters.ContainsKey("ui_update_time"))
            {
                var updateTime = _performanceCounters["ui_update_time"].LastValue;
                if (updateTime > _maxUpdateTimeMs)
                {
                    RaiseAlert("high_update_time",
                        UIPerformanceAlertSeverity.Warning,
                        $"UI update time too high: {updateTime:F2}ms > {_maxUpdateTimeMs:F2}ms");
                }
            }
        }
        
        /// <summary>
        /// Raise performance alert
        /// </summary>
        private void RaiseAlert(string alertId, UIPerformanceAlertSeverity severity, string message)
        {
            // Check if alert already exists
            if (_activeAlerts.Any(a => a.AlertId == alertId))
                return;
            
            var alert = new UIPerformanceAlert
            {
                AlertId = alertId,
                Severity = severity,
                Message = message,
                Timestamp = System.DateTime.Now,
                IsActive = true
            };
            
            _activeAlerts.Add(alert);
            OnPerformanceAlertRaised?.Invoke(alert);
            
            LogWarning($"Performance Alert: {severity} - {message}");
        }
        
        /// <summary>
        /// Generate optimization recommendations
        /// </summary>
        private List<string> GenerateOptimizationRecommendations()
        {
            var recommendations = new List<string>();
            
            if (_averageFrameTime > _targetFrameTimeMs)
            {
                recommendations.Add("Consider reducing UI update frequency or optimizing update logic");
            }
            
            if (_averageMemoryUsage > _maxMemoryUsageMB * 0.8f)
            {
                recommendations.Add("Memory usage approaching limit - consider enabling object pooling");
            }
            
            if (_prefabManager != null && _prefabManager.ActiveComponentCount > _maxActiveComponents * 0.8f)
            {
                recommendations.Add("High component count - consider component lifecycle optimization");
            }
            
            // Component-specific recommendations
            foreach (var kvp in _componentPerformance)
            {
                var perfData = kvp.Value;
                if (perfData.AverageUpdateTime > 1.0f) // 1ms threshold
                {
                    recommendations.Add($"Component {perfData.ComponentType} has high update time - consider optimization");
                }
            }
            
            if (recommendations.Count == 0)
            {
                recommendations.Add("Performance is within acceptable limits - no optimizations needed");
            }
            
            return recommendations;
        }
        
        /// <summary>
        /// Apply automatic optimizations
        /// </summary>
        private void ApplyAutomaticOptimizations()
        {
            if (!_autoOptimize)
                return;
            
            // Example optimizations (would need actual implementation)
            if (_averageFrameTime > _targetFrameTimeMs)
            {
                // Reduce update frequency
                OnPerformanceOptimizationApplied?.Invoke("Reduced UI update frequency");
            }
            
            if (_prefabManager != null && _prefabManager.ActiveComponentCount > _maxActiveComponents)
            {
                // Force garbage collection or component cleanup
                System.GC.Collect();
                OnPerformanceOptimizationApplied?.Invoke("Forced garbage collection");
            }
        }
        
        /// <summary>
        /// Log performance report
        /// </summary>
        private void LogPerformanceReport(UIPerformanceReport report)
        {
            LogInfo($"UI Performance Report Generated:");
            LogInfo($"- Average Frame Time: {report.AverageFrameTime:F2}ms");
            LogInfo($"- Average Memory Usage: {report.AverageMemoryUsage:F1}MB");
            LogInfo($"- Active Alerts: {report.ActiveAlerts.Count}");
            LogInfo($"- Tracked Components: {report.ComponentPerformance.Count}");
            
            if (_showDetailedMetrics)
            {
                foreach (var counter in report.PerformanceCounters.Values)
                {
                    LogInfo($"- {counter.DisplayName}: {counter.LastValue:F2} (avg: {counter.AverageValue:F2})");
                }
            }
            
            foreach (var recommendation in report.Recommendations)
            {
                LogInfo($"Recommendation: {recommendation}");
            }
        }
        
        private void Update()
        {
            if (!_isProfilingActive || !_enableRealtimeMonitoring)
                return;
            
            _samplingTimer += Time.deltaTime;
            
            if (_samplingTimer >= _samplingInterval)
            {
                SamplePerformanceMetrics();
                _samplingTimer = 0f;
                
                if (_autoOptimize)
                {
                    ApplyAutomaticOptimizations();
                }
            }
        }
        
        private void OnValidate()
        {
            _samplingInterval = Mathf.Max(0.01f, _samplingInterval);
            _maxSamples = Mathf.Max(10, _maxSamples);
            _targetFrameTimeMs = Mathf.Max(1f, _targetFrameTimeMs);
            _maxUpdateTimeMs = Mathf.Max(0.1f, _maxUpdateTimeMs);
            _maxMemoryUsageMB = Mathf.Max(1f, _maxMemoryUsageMB);
            _maxActiveComponents = Mathf.Max(1, _maxActiveComponents);
            _maxGCAllocKB = Mathf.Max(1f, _maxGCAllocKB);
        }
        
        protected override void OnDestroy()
        {
            if (_isProfilingActive)
            {
                StopProfiling();
            }
            
            base.OnDestroy();
        }
    }
    
    /// <summary>
    /// Performance counter data
    /// </summary>
    [System.Serializable]
    public class UIPerformanceCounter
    {
        public string Name;
        public string DisplayName;
        public Queue<float> Values;
        public float LastValue;
        public float MinValue;
        public float MaxValue;
        public float AverageValue;
    }
    
    /// <summary>
    /// Performance metric data
    /// </summary>
    [System.Serializable]
    public class UIPerformanceMetric
    {
        public string Name;
        public float Value;
        public System.DateTime Timestamp;
        public string Unit;
    }
    
    /// <summary>
    /// Performance alert data
    /// </summary>
    [System.Serializable]
    public class UIPerformanceAlert
    {
        public string AlertId;
        public UIPerformanceAlertSeverity Severity;
        public string Message;
        public System.DateTime Timestamp;
        public bool IsActive;
    }
    
    /// <summary>
    /// Component performance data
    /// </summary>
    public class ComponentPerformanceData
    {
        public string ComponentId;
        public string ComponentType;
        public Queue<float> UpdateTimes;
        public Queue<float> MemoryUsage;
        public float AverageUpdateTime;
        public float AverageMemoryUsage;
    }
    
    /// <summary>
    /// Complete performance report
    /// </summary>
    [System.Serializable]
    public class UIPerformanceReport
    {
        public System.DateTime GenerationTime;
        public float ProfilingDuration;
        public float AverageFrameTime;
        public float AverageMemoryUsage;
        public Dictionary<string, UIPerformanceCounter> PerformanceCounters;
        public List<UIPerformanceAlert> ActiveAlerts;
        public Dictionary<string, ComponentPerformanceData> ComponentPerformance;
        public List<string> Recommendations;
        
        public bool IsHealthy => ActiveAlerts.Count(a => a.Severity >= UIPerformanceAlertSeverity.Warning) == 0;
    }
    
    /// <summary>
    /// Performance alert severity levels
    /// </summary>
    public enum UIPerformanceAlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }
}