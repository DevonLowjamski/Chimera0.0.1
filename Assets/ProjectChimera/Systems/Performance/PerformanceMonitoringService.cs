using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Performance Monitoring Service - Dedicated service for metrics collection and monitoring
    /// Extracted from UnifiedPerformanceManagementSystem to provide focused monitoring functionality
    /// Handles frame monitoring, system profiling, and performance data collection
    /// </summary>
    public class PerformanceMonitoringService : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _monitoringIntervalSeconds = 1f;
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private int _maxFrameHistorySize = 300; // 5 minutes at 60 FPS
        [SerializeField] private int _maxProfileHistorySize = 100;

        [Header("Performance Thresholds")]
        [SerializeField] private float _memoryWarningThresholdMB = 1024f;
        [SerializeField] private float _memoryCriticalThresholdMB = 2048f;
        [SerializeField] private float _cpuWarningThresholdMs = 16.67f; // 60 FPS target
        [SerializeField] private float _cpuCriticalThresholdMs = 33.33f; // 30 FPS critical

        // Performance monitoring data
        private Dictionary<string, PerformanceProfile> _systemProfiles = new Dictionary<string, PerformanceProfile>();
        private GlobalPerformanceMetrics _globalMetrics = new GlobalPerformanceMetrics();
        private Queue<FramePerformanceData> _frameHistory = new Queue<FramePerformanceData>();
        private List<IPerformanceManagedSystem> _managedSystems = new List<IPerformanceManagedSystem>();

        // Monitoring state
        private bool _isInitialized = false;
        private float _lastMonitoringTime = 0f;
        private int _frameCount = 0;
        private float _fpsAccumulator = 0f;

        // Events for performance notifications
        public static event System.Action<GlobalPerformanceMetrics> OnMetricsUpdated;
        public static event System.Action<PerformanceProfile> OnSystemProfileUpdated;
        public static event System.Action<FramePerformanceData> OnFrameDataRecorded;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Performance Monitoring Service";
        public GlobalPerformanceMetrics GlobalMetrics => _globalMetrics;
        public IReadOnlyDictionary<string, PerformanceProfile> SystemProfiles => _systemProfiles;
        public IReadOnlyCollection<FramePerformanceData> FrameHistory => _frameHistory;

        public void Initialize()
        {
            InitializeService();
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDataStructures()
        {
            _systemProfiles = new Dictionary<string, PerformanceProfile>();
            _globalMetrics = new GlobalPerformanceMetrics
            {
                TargetFrameRate = _targetFrameRate,
                OverallState = PerformanceState.Good,
                LastUpdate = DateTime.Now
            };
            _frameHistory = new Queue<FramePerformanceData>();
            _managedSystems = new List<IPerformanceManagedSystem>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PerformanceMonitoringService already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                InitializeMonitoring();
                
                _isInitialized = true;
                ChimeraLogger.Log("PerformanceMonitoringService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PerformanceMonitoringService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            SaveMonitoringData();
            _managedSystems.Clear();
            _systemProfiles.Clear();
            _frameHistory.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("PerformanceMonitoringService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_monitoringIntervalSeconds <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid monitoring interval, using default 1s", this);
                _monitoringIntervalSeconds = 1f;
            }

            if (_targetFrameRate <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid target frame rate, using default 60 FPS", this);
                _targetFrameRate = 60f;
            }
        }

        private void InitializeMonitoring()
        {
            _lastMonitoringTime = Time.time;
            _frameCount = 0;
            _fpsAccumulator = 0f;
            
            ChimeraLogger.Log($"Performance monitoring initialized - Target FPS: {_targetFrameRate}", this);
        }

        #endregion

        #region System Registration

        /// <summary>
        /// Register a system for performance monitoring
        /// </summary>
        public void RegisterManagedSystem(IPerformanceManagedSystem system)
        {
            if (system == null)
            {
                ChimeraLogger.LogWarning("Cannot register null system for performance monitoring", this);
                return;
            }

            if (!_managedSystems.Contains(system))
            {
                _managedSystems.Add(system);
                
                // Create initial performance profile
                var profile = new PerformanceProfile
                {
                    SystemName = system.SystemName,
                    CurrentQuality = PerformanceQualityLevel.High,
                    LastOptimization = DateTime.Now,
                    IsBottleneck = false
                };
                
                _systemProfiles[system.SystemName] = profile;
                
                ChimeraLogger.Log($"Registered system for monitoring: {system.SystemName}", this);
            }
        }

        /// <summary>
        /// Unregister a system from performance monitoring
        /// </summary>
        public void UnregisterManagedSystem(IPerformanceManagedSystem system)
        {
            if (system == null) return;

            if (_managedSystems.Remove(system))
            {
                _systemProfiles.Remove(system.SystemName);
                ChimeraLogger.Log($"Unregistered system from monitoring: {system.SystemName}", this);
            }
        }

        #endregion

        #region Performance Monitoring

        private void Update()
        {
            if (!_isInitialized || !_enablePerformanceMonitoring) return;

            // Record frame data every frame
            RecordFrameData();

            // Update global metrics at specified interval
            if (Time.time - _lastMonitoringTime >= _monitoringIntervalSeconds)
            {
                MonitorPerformance();
                _lastMonitoringTime = Time.time;
            }
        }

        private void MonitorPerformance()
        {
            UpdateGlobalMetrics();
            UpdateSystemProfiles();
            TriggerMetricsUpdated();
        }

        private void UpdateGlobalMetrics()
        {
            // Calculate average FPS
            _globalMetrics.CurrentFrameRate = 1f / Time.unscaledDeltaTime;
            _globalMetrics.AverageFrameRate = _frameCount > 0 ? _fpsAccumulator / _frameCount : _globalMetrics.CurrentFrameRate;
            
            // Update frame timing
            _globalMetrics.CpuFrameTime = Time.deltaTime * 1000f; // Convert to milliseconds
            _globalMetrics.GpuFrameTime = Time.deltaTime * 1000f; // Simplified - actual GPU timing requires deeper profiling
            
            // Update memory metrics
            _globalMetrics.TotalMemoryUsage = (long)Profiler.GetTotalAllocatedMemory();
            _globalMetrics.AvailableMemory = (long)Profiler.GetTotalReservedMemory() - _globalMetrics.TotalMemoryUsage;
            _globalMetrics.MemoryPressure = CalculateMemoryPressure();
            
            // Update system counts
            _globalMetrics.ActiveSystems = _managedSystems.Count;
            _globalMetrics.OptimizedSystems = _systemProfiles.Values.Count(p => p.CurrentQuality < PerformanceQualityLevel.High);
            
            // Update overall state
            _globalMetrics.OverallState = CalculateOverallPerformanceState();
            _globalMetrics.LastUpdate = DateTime.Now;

            // Reset frame accumulator
            _frameCount = 0;
            _fpsAccumulator = 0f;
        }

        private void UpdateSystemProfiles()
        {
            foreach (var system in _managedSystems)
            {
                if (_systemProfiles.TryGetValue(system.SystemName, out var profile))
                {
                    UpdateSystemProfile(system, profile);
                    OnSystemProfileUpdated?.Invoke(profile);
                }
            }
        }

        private void UpdateSystemProfile(IPerformanceManagedSystem system, PerformanceProfile profile)
        {
            // Get system performance data
            var systemMetrics = system.GetPerformanceMetrics();
            
            // Update CPU timing
            profile.RecentCpuTimes.Add(systemMetrics.CpuTime);
            if (profile.RecentCpuTimes.Count > _maxProfileHistorySize)
                profile.RecentCpuTimes.RemoveAt(0);
            
            profile.AverageCpuTime = profile.RecentCpuTimes.Average();
            profile.PeakCpuTime = profile.RecentCpuTimes.Max();
            
            // Update memory usage
            profile.RecentMemoryUsages.Add(systemMetrics.MemoryUsage);
            if (profile.RecentMemoryUsages.Count > _maxProfileHistorySize)
                profile.RecentMemoryUsages.RemoveAt(0);
            
            profile.AverageMemoryUsage = (long)profile.RecentMemoryUsages.Average();
            profile.PeakMemoryUsage = profile.RecentMemoryUsages.Max();
            
            // Update other metrics
            profile.UpdateFrequency = systemMetrics.UpdateFrequency;
            profile.IsBottleneck = DetermineIfBottleneck(profile);
        }

        private void RecordFrameData()
        {
            var frameData = new FramePerformanceData
            {
                Timestamp = Time.time,
                FrameRate = 1f / Time.unscaledDeltaTime,
                FrameTime = Time.deltaTime * 1000f,
                MemoryUsage = (long)Profiler.GetTotalAllocatedMemory(),
                DrawCalls = 0, // Would need to be populated from actual rendering stats
                Triangles = 0   // Would need to be populated from actual rendering stats
            };

            _frameHistory.Enqueue(frameData);
            
            // Maintain frame history size
            if (_frameHistory.Count > _maxFrameHistorySize)
                _frameHistory.Dequeue();

            // Update FPS accumulator
            _frameCount++;
            _fpsAccumulator += frameData.FrameRate;

            OnFrameDataRecorded?.Invoke(frameData);
        }

        #endregion

        #region Performance Analysis

        private float CalculateMemoryPressure()
        {
            var memoryUsageMB = _globalMetrics.TotalMemoryUsage / (1024f * 1024f);
            
            if (memoryUsageMB > _memoryCriticalThresholdMB)
                return 1.0f; // Critical pressure
            else if (memoryUsageMB > _memoryWarningThresholdMB)
                return 0.5f + 0.5f * (memoryUsageMB - _memoryWarningThresholdMB) / (_memoryCriticalThresholdMB - _memoryWarningThresholdMB);
            else
                return memoryUsageMB / _memoryWarningThresholdMB * 0.5f;
        }

        private PerformanceState CalculateOverallPerformanceState()
        {
            var avgFrameRate = _globalMetrics.AverageFrameRate;
            var memoryPressure = _globalMetrics.MemoryPressure;
            var cpuTime = _globalMetrics.CpuFrameTime;

            // Critical conditions
            if (avgFrameRate < _targetFrameRate * 0.5f || 
                memoryPressure > 0.9f || 
                cpuTime > _cpuCriticalThresholdMs)
            {
                return PerformanceState.Critical;
            }

            // Warning conditions
            if (avgFrameRate < _targetFrameRate * 0.8f || 
                memoryPressure > 0.6f || 
                cpuTime > _cpuWarningThresholdMs)
            {
                return PerformanceState.Warning;
            }

            return PerformanceState.Good;
        }

        private bool DetermineIfBottleneck(PerformanceProfile profile)
        {
            // System is a bottleneck if its CPU time is significantly above average
            var avgCpuTime = _systemProfiles.Values.Where(p => p != profile).Average(p => p.AverageCpuTime);
            return profile.AverageCpuTime > avgCpuTime * 2f && profile.AverageCpuTime > _cpuWarningThresholdMs;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get performance profile for a specific system
        /// </summary>
        public PerformanceProfile GetSystemProfile(string systemName)
        {
            return _systemProfiles.TryGetValue(systemName, out var profile) ? profile : null;
        }

        /// <summary>
        /// Get current frame rate statistics
        /// </summary>
        public FrameRateStats GetFrameRateStats()
        {
            if (_frameHistory.Count == 0)
                return new FrameRateStats();

            var frameRates = _frameHistory.Select(f => f.FrameRate).ToList();
            
            return new FrameRateStats
            {
                Current = _globalMetrics.CurrentFrameRate,
                Average = frameRates.Average(),
                Min = frameRates.Min(),
                Max = frameRates.Max(),
                Target = _targetFrameRate,
                SampleCount = frameRates.Count
            };
        }

        /// <summary>
        /// Get memory usage statistics
        /// </summary>
        public MemoryStats GetMemoryStats()
        {
            return new MemoryStats
            {
                TotalAllocated = _globalMetrics.TotalMemoryUsage,
                Available = _globalMetrics.AvailableMemory,
                Pressure = _globalMetrics.MemoryPressure,
                WarningThreshold = (long)(_memoryWarningThresholdMB * 1024 * 1024),
                CriticalThreshold = (long)(_memoryCriticalThresholdMB * 1024 * 1024)
            };
        }

        /// <summary>
        /// Force refresh of all performance data
        /// </summary>
        public void RefreshPerformanceData()
        {
            if (!_isInitialized) return;

            MonitorPerformance();
            ChimeraLogger.Log("Performance data manually refreshed", this);
        }

        #endregion

        #region Event Triggers

        private void TriggerMetricsUpdated()
        {
            OnMetricsUpdated?.Invoke(_globalMetrics);
        }

        #endregion

        #region Utility Methods

        private void SaveMonitoringData()
        {
            // Save performance metrics for analysis
            var sessionData = new PerformanceSessionData
            {
                SessionDuration = Time.time,
                TotalFramesRecorded = _frameHistory.Count,
                AverageFrameRate = _globalMetrics.AverageFrameRate,
                PeakMemoryUsage = _frameHistory.Count > 0 ? _frameHistory.Max(f => f.MemoryUsage) : 0,
                SystemProfileCount = _systemProfiles.Count
            };

            ChimeraLogger.Log($"Performance session completed: {sessionData.SessionDuration:F1}s, " +
                            $"Avg FPS: {sessionData.AverageFrameRate:F1}, " +
                            $"Systems monitored: {sessionData.SystemProfileCount}", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Performance profile for individual systems
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
    /// Global performance metrics for the entire application
    /// </summary>
    [System.Serializable]
    public class GlobalPerformanceMetrics
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
    }

    /// <summary>
    /// Frame-by-frame performance data
    /// </summary>
    [System.Serializable]
    public class FramePerformanceData
    {
        public float Timestamp;
        public float FrameRate;
        public float FrameTime;
        public long MemoryUsage;
        public int DrawCalls;
        public int Triangles;
    }

    /// <summary>
    /// Frame rate statistics
    /// </summary>
    [System.Serializable]
    public class FrameRateStats
    {
        public float Current;
        public float Average;
        public float Min;
        public float Max;
        public float Target;
        public int SampleCount;
    }

    /// <summary>
    /// Memory usage statistics
    /// </summary>
    [System.Serializable]
    public class MemoryStats
    {
        public long TotalAllocated;
        public long Available;
        public float Pressure;
        public long WarningThreshold;
        public long CriticalThreshold;
    }

    /// <summary>
    /// Performance session summary data
    /// </summary>
    [System.Serializable]
    public class PerformanceSessionData
    {
        public float SessionDuration;
        public int TotalFramesRecorded;
        public float AverageFrameRate;
        public long PeakMemoryUsage;
        public int SystemProfileCount;
    }

    // Enums and interfaces
    public enum PerformanceState
    {
        Good,
        Warning,
        Critical
    }

    public enum PerformanceQualityLevel
    {
        Ultra,
        High,
        Medium,
        Low,
        Minimal
    }

    /// <summary>
    /// Interface for systems that can be monitored for performance
    /// </summary>
    public interface IPerformanceManagedSystem
    {
        string SystemName { get; }
        SystemPerformanceMetrics GetPerformanceMetrics();
        void SetQualityLevel(PerformanceQualityLevel level);
    }

    /// <summary>
    /// Performance metrics specific to individual systems
    /// </summary>
    [System.Serializable]
    public class SystemPerformanceMetrics
    {
        public float CpuTime;
        public long MemoryUsage;
        public int UpdateFrequency;
        public float LastUpdateTime;
    }

    #endregion
}