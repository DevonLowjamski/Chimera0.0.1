using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Gaming Performance Monitor - Real-time 60fps gaming performance management
    /// Part of Module 1: Gaming Experience Core - Deliverable 2.1
    /// 
    /// Provides comprehensive real-time performance monitoring specifically designed for gaming,
    /// ensuring smooth 60fps gameplay through continuous monitoring and adaptive optimizations.
    /// </summary>
    public class GamingPerformanceMonitor : ChimeraManager, IGamingPerformanceMonitor
    {
        [Header("Performance Targets")]
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _maxAcceptableFrameTime = 16.67f; // 60fps = 16.67ms
        [SerializeField] private float _criticalPerformanceThreshold = 30f; // 30fps critical
        
        [Header("Monitoring Configuration")]
        [SerializeField] private bool _enableAutomaticOptimization = true;
        [SerializeField] private float _monitoringFrequency = 4f; // 4 updates per second
        [SerializeField] private int _frameAveragingWindow = 60; // Average over 60 frames
        [SerializeField] private bool _enableDetailedProfiling = false;
        
        [Header("Alert Thresholds")]
        [SerializeField] private float _performanceDegradationThreshold = 45f; // Below 45fps = degraded
        [SerializeField] private float _performanceRecoveryThreshold = 55f; // Above 55fps = recovered
        [SerializeField] private int _consecutiveFramesForStateChange = 30; // Frames to confirm state change
        
        [Header("Debug & Display")]
        [SerializeField] private bool _enableDebugDisplay = false;
        [SerializeField] private bool _enableVerboseLogging = false;
        
        // Performance data tracking
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private readonly Queue<GamingPerformanceData> _performanceHistory = new Queue<GamingPerformanceData>();
        private readonly List<float> _fpsBuffer = new List<float>();
        
        // Current performance metrics
        private float _currentFPS;
        private float _currentFrameTime;
        private float _averageFPS;
        private float _minFPS = float.MaxValue;
        private float _maxFPS = float.MinValue;
        private float _currentMemoryUsage;
        private float _gpuMemoryUsage;
        
        // Performance state tracking
        private GamingPerformanceState _currentPerformanceState = GamingPerformanceState.Optimal;
        private GamingPerformanceState _previousPerformanceState = GamingPerformanceState.Optimal;
        private int _consecutiveStateFrames = 0;
        private bool _isUnderPerformanceStress = false;
        
        // Statistics tracking
        private GamingPerformanceStatistics _sessionStatistics;
        private DateTime _sessionStartTime;
        private float _lastMonitoringUpdate;
        private int _totalFramesSinceStart = 0;
        
        // Optimization state
        private bool _intensiveMonitoringActive = false;
        private float _lastOptimizationTime = 0f;
        private const float OPTIMIZATION_COOLDOWN = 5f; // 5 seconds between optimizations
        
        #region IGamingPerformanceMonitor Properties
        
        public float TargetFrameRate
        {
            get => _targetFrameRate;
            set
            {
                _targetFrameRate = Mathf.Max(15f, value);
                _maxAcceptableFrameTime = 1000f / _targetFrameRate;
            }
        }
        
        public float MaxAcceptableFrameTime
        {
            get => _maxAcceptableFrameTime;
            set => _maxAcceptableFrameTime = Mathf.Max(1f, value);
        }
        
        public float CriticalPerformanceThreshold
        {
            get => _criticalPerformanceThreshold;
            set => _criticalPerformanceThreshold = Mathf.Max(5f, value);
        }
        
        public float CurrentFPS => _currentFPS;
        public float CurrentFrameTime => _currentFrameTime;
        public float AverageFPS => _averageFPS;
        public float MinFPS => _minFPS;
        public float MaxFPS => _maxFPS;
        public float CurrentMemoryUsage => _currentMemoryUsage;
        public float GPUMemoryUsage => _gpuMemoryUsage;
        
        public GamingPerformanceState CurrentPerformanceState => _currentPerformanceState;
        public bool IsPerformingOptimally => _currentPerformanceState == GamingPerformanceState.Optimal;
        public bool IsUnderPerformanceStress => _isUnderPerformanceStress;
        public bool IsHittingTargetFrameRate => _currentFPS >= _targetFrameRate * 0.9f; // Within 90% of target
        
        public bool EnableAutomaticOptimization
        {
            get => _enableAutomaticOptimization;
            set => _enableAutomaticOptimization = value;
        }
        
        public float MonitoringFrequency
        {
            get => _monitoringFrequency;
            set => _monitoringFrequency = Mathf.Clamp(value, 0.5f, 10f);
        }
        
        public int FrameAveragingWindow
        {
            get => _frameAveragingWindow;
            set => _frameAveragingWindow = Mathf.Clamp(value, 10, 300);
        }
        
        public bool EnableDetailedProfiling
        {
            get => _enableDetailedProfiling;
            set => _enableDetailedProfiling = value;
        }
        
        #endregion
        
        #region Events
        
        public event Action<GamingPerformanceData> OnPerformanceDegraded;
        public event Action<GamingPerformanceData> OnPerformanceRecovered;
        public event Action<GamingPerformanceData> OnCriticalPerformance;
        public event Action<GamingPerformanceData> OnPerformanceUpdate;
        
        #endregion
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializePerformanceMonitoring();
            Debug.Log("[GamingPerformanceMonitor] Real-time 60fps performance monitoring initialized");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            UpdatePerformanceMetrics();
            CheckPerformanceState();
            
            // Periodic comprehensive update
            if (Time.time - _lastMonitoringUpdate >= (1f / _monitoringFrequency))
            {
                PerformComprehensiveUpdate();
                _lastMonitoringUpdate = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            if (_enableVerboseLogging)
            {
                var stats = GetSessionStatistics();
                Debug.Log($"[GamingPerformanceMonitor] Session Summary: {stats}");
            }
            
            Debug.Log("[GamingPerformanceMonitor] Performance monitoring shutdown complete");
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void InitializePerformanceMonitoring()
        {
            _sessionStartTime = DateTime.UtcNow;
            _sessionStatistics = new GamingPerformanceStatistics
            {
                SessionStartTime = _sessionStartTime
            };
            
            // Initialize FPS buffer
            _fpsBuffer.Clear();
            for (int i = 0; i < _frameAveragingWindow; i++)
            {
                _fpsBuffer.Add(_targetFrameRate);
            }
            
            // Set initial values
            _currentFPS = _targetFrameRate;
            _averageFPS = _targetFrameRate;
            _minFPS = _targetFrameRate;
            _maxFPS = _targetFrameRate;
            
            // Configure Unity's target frame rate
            Application.targetFrameRate = (int)_targetFrameRate;
            QualitySettings.vSyncCount = 0; // Disable VSync for accurate monitoring
            
            if (_enableVerboseLogging)
            {
                Debug.Log($"[GamingPerformanceMonitor] Initialized with target: {_targetFrameRate}fps, monitoring at {_monitoringFrequency}Hz");
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            // Calculate current frame metrics
            _currentFrameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            _currentFPS = 1f / Time.unscaledDeltaTime;
            
            // Update frame time history for stability calculations
            _frameTimeHistory.Enqueue(_currentFrameTime);
            if (_frameTimeHistory.Count > _frameAveragingWindow)
                _frameTimeHistory.Dequeue();
            
            // Update FPS buffer for averaging
            _fpsBuffer[_totalFramesSinceStart % _frameAveragingWindow] = _currentFPS;
            _totalFramesSinceStart++;
            
            // Calculate average FPS
            var validFrames = Math.Min(_totalFramesSinceStart, _frameAveragingWindow);
            _averageFPS = _fpsBuffer.Take(validFrames).Sum() / validFrames;
            
            // Update min/max tracking
            if (_currentFPS < _minFPS) _minFPS = _currentFPS;
            if (_currentFPS > _maxFPS) _maxFPS = _currentFPS;
            
            // Update memory metrics
            UpdateMemoryMetrics();
        }
        
        private void UpdateMemoryMetrics()
        {
            // Get memory usage
            _currentMemoryUsage = Profiler.GetTotalAllocatedMemory() / (1024f * 1024f); // Convert to MB
            
            // Get GPU memory if available
            if (SystemInfo.graphicsMemorySize > 0)
            {
                _gpuMemoryUsage = Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);
            }
        }
        
        private void CheckPerformanceState()
        {
            // Determine current performance state based on FPS
            var newState = DeterminePerformanceState(_averageFPS);
            
            // Check for state changes with consecutive frame confirmation
            if (newState != _currentPerformanceState)
            {
                _consecutiveStateFrames++;
                
                if (_consecutiveStateFrames >= _consecutiveFramesForStateChange)
                {
                    var previousState = _currentPerformanceState;
                    _currentPerformanceState = newState;
                    _consecutiveStateFrames = 0;
                    
                    // Handle state change events
                    HandlePerformanceStateChange(previousState, newState);
                }
            }
            else
            {
                _consecutiveStateFrames = 0;
            }
            
            // Update stress indicator
            _isUnderPerformanceStress = _currentPerformanceState <= GamingPerformanceState.Degraded;
            
            // Trigger automatic optimization if enabled
            if (_enableAutomaticOptimization && ShouldTriggerOptimization())
            {
                TriggerAutomaticOptimization();
            }
        }
        
        private GamingPerformanceState DeterminePerformanceState(float fps)
        {
            if (fps >= _targetFrameRate * 0.9f) return GamingPerformanceState.Optimal;
            if (fps >= _performanceDegradationThreshold) return GamingPerformanceState.Good;
            if (fps >= _criticalPerformanceThreshold) return GamingPerformanceState.Degraded;
            if (fps >= 15f) return GamingPerformanceState.Poor;
            return GamingPerformanceState.Critical;
        }
        
        private void HandlePerformanceStateChange(GamingPerformanceState previousState, GamingPerformanceState newState)
        {
            var performanceData = GetCurrentPerformanceData();
            
            // Log state changes
            if (_enableVerboseLogging)
            {
                Debug.Log($"[GamingPerformanceMonitor] Performance state changed: {previousState} -> {newState} (FPS: {_currentFPS:F1})");
            }
            
            // Fire appropriate events
            if (newState < previousState) // Performance degraded
            {
                OnPerformanceDegraded?.Invoke(performanceData);
                _sessionStatistics.TotalPerformanceDrops++;
                
                if (newState == GamingPerformanceState.Critical)
                {
                    OnCriticalPerformance?.Invoke(performanceData);
                    _sessionStatistics.CriticalPerformanceEvents++;
                }
            }
            else if (newState > previousState) // Performance recovered
            {
                OnPerformanceRecovered?.Invoke(performanceData);
            }
        }
        
        private void PerformComprehensiveUpdate()
        {
            // Create comprehensive performance data
            var performanceData = GetCurrentPerformanceData();
            
            // Store in history
            _performanceHistory.Enqueue(performanceData);
            if (_performanceHistory.Count > 300) // Keep 5 minutes at 1Hz
                _performanceHistory.Dequeue();
            
            // Update session statistics
            UpdateSessionStatistics(performanceData);
            
            // Fire update event
            OnPerformanceUpdate?.Invoke(performanceData);
            
            // Reset min/max for next window if needed
            if (_totalFramesSinceStart % (_frameAveragingWindow * 4) == 0)
            {
                _minFPS = _currentFPS;
                _maxFPS = _currentFPS;
            }
        }
        
        private void UpdateSessionStatistics(GamingPerformanceData data)
        {
            var sessionTime = (float)(DateTime.UtcNow - _sessionStartTime).TotalSeconds;
            _sessionStatistics.TotalSessionTime = sessionTime;
            _sessionStatistics.TotalFrames = _totalFramesSinceStart;
            
            // Update FPS statistics
            _sessionStatistics.SessionAverageFPS = _averageFPS;
            if (data.MinFPS < _sessionStatistics.SessionMinFPS || _sessionStatistics.SessionMinFPS == 0)
                _sessionStatistics.SessionMinFPS = data.MinFPS;
            if (data.MaxFPS > _sessionStatistics.SessionMaxFPS)
                _sessionStatistics.SessionMaxFPS = data.MaxFPS;
            
            // Update memory statistics
            if (data.TotalMemoryUsage > _sessionStatistics.PeakMemoryUsage)
                _sessionStatistics.PeakMemoryUsage = data.TotalMemoryUsage;
            
            // Update performance state time distribution (simplified)
            var statePercentage = 1f / _monitoringFrequency / sessionTime * 100f; // Percentage for this frame
            switch (_currentPerformanceState)
            {
                case GamingPerformanceState.Optimal:
                    _sessionStatistics.TimeInOptimalState += statePercentage;
                    break;
                case GamingPerformanceState.Good:
                    _sessionStatistics.TimeInGoodState += statePercentage;
                    break;
                case GamingPerformanceState.Degraded:
                    _sessionStatistics.TimeInDegradedState += statePercentage;
                    break;
                case GamingPerformanceState.Poor:
                    _sessionStatistics.TimeInPoorState += statePercentage;
                    break;
                case GamingPerformanceState.Critical:
                    _sessionStatistics.TimeInCriticalState += statePercentage;
                    break;
            }
        }
        
        #endregion
        
        #region Optimization
        
        private bool ShouldTriggerOptimization()
        {
            return _isUnderPerformanceStress && 
                   Time.time - _lastOptimizationTime > OPTIMIZATION_COOLDOWN;
        }
        
        private void TriggerAutomaticOptimization()
        {
            _lastOptimizationTime = Time.time;
            _sessionStatistics.AutoOptimizationTriggered++;
            
            // Basic optimization strategies
            PerformBasicOptimizations();
            
            if (_enableVerboseLogging)
            {
                Debug.Log($"[GamingPerformanceMonitor] Automatic optimization triggered (FPS: {_currentFPS:F1})");
            }
        }
        
        private void PerformBasicOptimizations()
        {
            // Force garbage collection if memory usage is high
            if (_currentMemoryUsage > 500f) // 500MB threshold
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            
            // Reduce quality settings if in critical state
            if (_currentPerformanceState == GamingPerformanceState.Critical)
            {
                if (QualitySettings.GetQualityLevel() > 0)
                {
                    QualitySettings.DecreaseLevel();
                    Debug.Log("[GamingPerformanceMonitor] Reduced quality level for performance");
                }
            }
        }
        
        #endregion
        
        #region Public Interface
        
        public GamingPerformanceData GetCurrentPerformanceData()
        {
            return new GamingPerformanceData
            {
                CurrentFPS = _currentFPS,
                AverageFPS = _averageFPS,
                MinFPS = _minFPS,
                MaxFPS = _maxFPS,
                CurrentFrameTime = _currentFrameTime,
                AverageFrameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : _currentFrameTime,
                
                TotalMemoryUsage = _currentMemoryUsage,
                MonoMemoryUsage = Profiler.GetMonoUsedSize() / (1024f * 1024f),
                GPUMemoryUsage = _gpuMemoryUsage,
                TempAllocatorSize = Profiler.GetTempAllocatorSize() / (1024f * 1024f),
                
                PerformanceState = _currentPerformanceState,
                PerformanceScore = CalculatePerformanceScore(),
                IsStable = CalculateStability(),
                NeedsOptimization = _isUnderPerformanceStress,
                
                Timestamp = DateTime.UtcNow,
                SessionTime = (float)(DateTime.UtcNow - _sessionStartTime).TotalSeconds,
                
                CPUUsage = GetCPUUsage(),
                GPUUsage = GetGPUUsage(),
                ActiveGameObjects = FindObjectsOfType<GameObject>().Length,
                DrawCalls = 0,    // Would need profiler integration for accurate count
                SetPassCalls = 0, // Would need profiler integration
                Triangles = 0,    // Would need profiler integration
                Vertices = 0      // Would need profiler integration
            };
        }
        
        public GamingPerformanceHistory GetPerformanceHistory(float durationSeconds = 60f)
        {
            var relevantData = _performanceHistory
                .Where(data => (DateTime.UtcNow - data.Timestamp).TotalSeconds <= durationSeconds)
                .ToArray();
            
            if (relevantData.Length == 0)
                return new GamingPerformanceHistory { DurationSeconds = durationSeconds };
            
            var history = new GamingPerformanceHistory
            {
                DataPoints = relevantData,
                DurationSeconds = durationSeconds,
                AverageFPS = relevantData.Average(d => d.AverageFPS),
                MinFPS = relevantData.Min(d => d.MinFPS),
                MaxFPS = relevantData.Max(d => d.MaxFPS),
                PerformanceDropCount = relevantData.Count(d => d.PerformanceState <= GamingPerformanceState.Degraded),
                TotalTimeInOptimalState = relevantData.Count(d => d.PerformanceState == GamingPerformanceState.Optimal) / (float)relevantData.Length * 100f,
                TotalTimeInDegradedState = relevantData.Count(d => d.PerformanceState <= GamingPerformanceState.Degraded) / (float)relevantData.Length * 100f
            };
            
            // Calculate FPS standard deviation
            var avgFPS = history.AverageFPS;
            history.FPSStandardDeviation = Mathf.Sqrt(relevantData.Sum(d => Mathf.Pow(d.AverageFPS - avgFPS, 2)) / relevantData.Length);
            
            return history;
        }
        
        public GamingPerformanceStatistics GetSessionStatistics()
        {
            return _sessionStatistics;
        }
        
        public void ForcePerformanceCheck()
        {
            PerformComprehensiveUpdate();
            
            if (_enableVerboseLogging)
            {
                var data = GetCurrentPerformanceData();
                Debug.Log($"[GamingPerformanceMonitor] Forced performance check: {data}");
            }
        }
        
        public void ResetStatistics()
        {
            _sessionStartTime = DateTime.UtcNow;
            _sessionStatistics = new GamingPerformanceStatistics
            {
                SessionStartTime = _sessionStartTime
            };
            _performanceHistory.Clear();
            _minFPS = _currentFPS;
            _maxFPS = _currentFPS;
            _totalFramesSinceStart = 0;
            
            Debug.Log("[GamingPerformanceMonitor] Performance statistics reset");
        }
        
        public void StartIntensiveMonitoring()
        {
            _intensiveMonitoringActive = true;
            _monitoringFrequency = 10f; // 10 updates per second
            _enableDetailedProfiling = true;
            
            Debug.Log("[GamingPerformanceMonitor] Intensive monitoring started");
        }
        
        public void StopIntensiveMonitoring()
        {
            _intensiveMonitoringActive = false;
            _monitoringFrequency = 4f; // Back to normal
            _enableDetailedProfiling = false;
            
            Debug.Log("[GamingPerformanceMonitor] Intensive monitoring stopped");
        }
        
        #endregion
        
        #region Helper Methods
        
        private float CalculatePerformanceScore()
        {
            // Score from 0-100 based on FPS relative to target
            var fpsScore = Mathf.Clamp01(_currentFPS / _targetFrameRate) * 50f;
            
            // Stability score based on frame time variance
            var stabilityScore = CalculateStability() ? 25f : 0f;
            
            // Memory efficiency score
            var memoryScore = _currentMemoryUsage < 100f ? 25f : Mathf.Max(0f, 25f - (_currentMemoryUsage - 100f) * 0.1f);
            
            return fpsScore + stabilityScore + memoryScore;
        }
        
        private bool CalculateStability()
        {
            if (_frameTimeHistory.Count < 10) return true;
            
            var frameTimeArray = _frameTimeHistory.ToArray();
            var average = frameTimeArray.Average();
            var variance = frameTimeArray.Sum(ft => Mathf.Pow(ft - average, 2)) / frameTimeArray.Length;
            var standardDeviation = Mathf.Sqrt(variance);
            
            // Stable if standard deviation is less than 20% of average frame time
            return standardDeviation < (average * 0.2f);
        }
        
        private float GetCPUUsage()
        {
            // This would require platform-specific implementation
            // For now, return estimated based on frame time
            return Mathf.Clamp01(_currentFrameTime / _maxAcceptableFrameTime) * 100f;
        }
        
        private float GetGPUUsage()
        {
            // This would require platform-specific implementation or profiler integration
            // For now, return estimated based on performance state
            return _currentPerformanceState switch
            {
                GamingPerformanceState.Optimal => 60f,
                GamingPerformanceState.Good => 75f,
                GamingPerformanceState.Degraded => 85f,
                GamingPerformanceState.Poor => 95f,
                GamingPerformanceState.Critical => 100f,
                _ => 50f
            };
        }
        
        #endregion
        
        #region Debug Display
        
        private void OnGUI()
        {
            if (!_enableDebugDisplay || !IsInitialized) return;
            
            var data = GetCurrentPerformanceData();
            var rect = new Rect(10, 10, 400, 200);
            
            GUI.Box(rect, "Gaming Performance Monitor");
            
            var labelRect = new Rect(rect.x + 10, rect.y + 25, rect.width - 20, 20);
            GUI.Label(labelRect, $"FPS: {data.CurrentFPS:F1} ({data.PerformanceState})");
            
            labelRect.y += 25;
            GUI.Label(labelRect, $"Frame Time: {data.CurrentFrameTime:F2}ms");
            
            labelRect.y += 25;
            GUI.Label(labelRect, $"Memory: {data.TotalMemoryUsage:F0}MB");
            
            labelRect.y += 25;
            GUI.Label(labelRect, $"Performance Score: {data.PerformanceScore:F1}");
            
            labelRect.y += 25;
            GUI.Label(labelRect, $"Stable: {data.IsStable}");
            
            labelRect.y += 25;
            GUI.Label(labelRect, $"Session: {data.SessionTime:F0}s");
        }
        
        #endregion
    }
}