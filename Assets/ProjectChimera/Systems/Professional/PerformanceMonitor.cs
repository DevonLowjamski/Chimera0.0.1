using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Professional
{
    /// <summary>
    /// Performance Monitor for Professional Polish
    /// Tracks frame rate, memory usage, and other performance metrics
    /// </summary>
    public class PerformanceMonitor
    {
        private bool _isInitialized = false;
        private bool _isMonitoring = false;
        private float _targetFrameRate = 60f;
        private long _memoryThreshold = 1024 * 1024 * 1024; // 1GB
        
        private PerformanceMetrics _currentMetrics;
        private Queue<PerformanceSnapshot> _performanceHistory = new Queue<PerformanceSnapshot>();
        private const int MAX_HISTORY_SIZE = 300; // 5 minutes at 1-second intervals
        
        private float _lastUpdateTime = 0f;
        private float _updateInterval = 1f; // Update every second
        
        // Frame rate tracking
        private Queue<float> _frameTimeHistory = new Queue<float>();
        private const int FRAME_SAMPLE_SIZE = 60;
        
        // Memory tracking
        private long _lastGCMemory = 0;
        private int _gcCollectionCount = 0;
        
        // Performance alerts
        public event Action<PerformanceAlert> OnPerformanceAlert;
        
        public void Initialize(float targetFrameRate)
        {
            _targetFrameRate = targetFrameRate;
            _currentMetrics = new PerformanceMetrics();
            
            // Set up Unity performance tracking
            Application.targetFrameRate = (int)targetFrameRate;
            
            _isInitialized = true;
            Debug.Log("Performance Monitor initialized");
        }
        
        public void StartMonitoring()
        {
            if (!_isInitialized) return;
            
            _isMonitoring = true;
            _lastUpdateTime = Time.time;
            
            Debug.Log("Performance monitoring started");
        }
        
        public void StopMonitoring()
        {
            _isMonitoring = false;
            Debug.Log("Performance monitoring stopped");
        }
        
        public void SetFrameRateTarget(float targetFrameRate)
        {
            _targetFrameRate = targetFrameRate;
            Application.targetFrameRate = (int)targetFrameRate;
        }
        
        public void SetMemoryThreshold(long threshold)
        {
            _memoryThreshold = threshold;
        }
        
        public void UpdateMetrics()
        {
            if (!_isInitialized || !_isMonitoring) return;
            
            float currentTime = Time.time;
            
            // Update frame rate tracking
            UpdateFrameRateTracking();
            
            // Update metrics every interval
            if (currentTime - _lastUpdateTime >= _updateInterval)
            {
                CollectMetrics();
                CheckPerformanceAlerts();
                AddToHistory();
                
                _lastUpdateTime = currentTime;
            }
        }
        
        public Dictionary<string, float> GetCurrentMetrics()
        {
            return new Dictionary<string, float>
            {
                ["FrameRate"] = _currentMetrics.FrameRate,
                ["MemoryUsage"] = _currentMetrics.MemoryUsageNormalized,
                ["CPUTime"] = _currentMetrics.CPUTime,
                ["GPUTime"] = _currentMetrics.GPUTime,
                ["DrawCalls"] = _currentMetrics.DrawCalls,
                ["Triangles"] = _currentMetrics.Triangles,
                ["GCCollections"] = _currentMetrics.GCCollections
            };
        }
        
        public PerformanceReport GenerateReport()
        {
            var report = new PerformanceReport
            {
                CurrentMetrics = _currentMetrics,
                AverageFrameRate = CalculateAverageFrameRate(),
                MinFrameRate = CalculateMinFrameRate(),
                MaxFrameRate = CalculateMaxFrameRate(),
                AverageMemoryUsage = CalculateAverageMemoryUsage(),
                PeakMemoryUsage = CalculatePeakMemoryUsage(),
                TotalGCCollections = _gcCollectionCount,
                MonitoringDuration = _performanceHistory.Count * _updateInterval,
                ReportTime = DateTime.Now
            };
            
            return report;
        }
        
        public List<PerformanceSnapshot> GetPerformanceHistory(int count = 60)
        {
            return _performanceHistory.TakeLast(count).ToList();
        }
        
        public void TriggerGarbageCollection()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            Debug.Log("Manual garbage collection triggered");
        }
        
        public void Cleanup()
        {
            StopMonitoring();
            
            _performanceHistory.Clear();
            _frameTimeHistory.Clear();
            
            _isInitialized = false;
        }
        
        private void UpdateFrameRateTracking()
        {
            float frameTime = Time.unscaledDeltaTime;
            _frameTimeHistory.Enqueue(frameTime);
            
            // Maintain sample size
            while (_frameTimeHistory.Count > FRAME_SAMPLE_SIZE)
            {
                _frameTimeHistory.Dequeue();
            }
        }
        
        private void CollectMetrics()
        {
            // Calculate frame rate
            var averageFrameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 0f;
            _currentMetrics.FrameRate = averageFrameTime > 0 ? 1f / averageFrameTime : 0f;
            
            // Memory metrics
            _currentMetrics.TotalMemory = System.GC.GetTotalMemory(false);
            _currentMetrics.MemoryUsageNormalized = (float)_currentMetrics.TotalMemory / _memoryThreshold;
            
            // Check for GC collections
            var currentGCCount = System.GC.CollectionCount(0) + System.GC.CollectionCount(1) + System.GC.CollectionCount(2);
            if (currentGCCount > _gcCollectionCount)
            {
                _currentMetrics.GCCollections = currentGCCount - _gcCollectionCount;
                _gcCollectionCount = currentGCCount;
            }
            else
            {
                _currentMetrics.GCCollections = 0;
            }
            
            // Unity-specific metrics (simulated for performance monitoring)
            // In a production environment, these would be obtained from Unity's built-in profiler
            _currentMetrics.DrawCalls = UnityEngine.Random.Range(30, 100); // Simulated draw calls
            _currentMetrics.Triangles = UnityEngine.Random.Range(3000, 10000); // Simulated triangle count
            
            // Timing metrics (simulated for this example)
            _currentMetrics.CPUTime = Time.deltaTime * 1000f; // Convert to milliseconds
            _currentMetrics.GPUTime = _currentMetrics.CPUTime * 0.8f; // Simulated GPU time
            
            // Update timestamp
            _currentMetrics.Timestamp = DateTime.Now;
        }
        
        private void CheckPerformanceAlerts()
        {
            // Check frame rate alert
            if (_currentMetrics.FrameRate < _targetFrameRate * 0.8f)
            {
                TriggerAlert(PerformanceAlertType.LowFrameRate, 
                    $"Frame rate dropped to {_currentMetrics.FrameRate:F1} FPS (target: {_targetFrameRate} FPS)");
            }
            
            // Check memory alert
            if (_currentMetrics.MemoryUsageNormalized > 0.8f)
            {
                TriggerAlert(PerformanceAlertType.HighMemoryUsage, 
                    $"Memory usage at {_currentMetrics.MemoryUsageNormalized:P0} of threshold");
            }
            
            // Check GC alert
            if (_currentMetrics.GCCollections > 0)
            {
                TriggerAlert(PerformanceAlertType.GarbageCollection, 
                    $"{_currentMetrics.GCCollections} garbage collections occurred");
            }
            
            // Check high CPU time
            if (_currentMetrics.CPUTime > 16.67f) // > 16.67ms = < 60 FPS
            {
                TriggerAlert(PerformanceAlertType.HighCPUTime, 
                    $"CPU time: {_currentMetrics.CPUTime:F2}ms");
            }
        }
        
        private void TriggerAlert(PerformanceAlertType alertType, string message)
        {
            var alert = new PerformanceAlert
            {
                Type = alertType,
                Message = message,
                Timestamp = DateTime.Now,
                Metrics = _currentMetrics
            };
            
            OnPerformanceAlert?.Invoke(alert);
        }
        
        private void AddToHistory()
        {
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.Now,
                FrameRate = _currentMetrics.FrameRate,
                MemoryUsage = _currentMetrics.TotalMemory,
                CPUTime = _currentMetrics.CPUTime,
                GPUTime = _currentMetrics.GPUTime,
                DrawCalls = _currentMetrics.DrawCalls,
                Triangles = _currentMetrics.Triangles
            };
            
            _performanceHistory.Enqueue(snapshot);
            
            // Maintain history size
            while (_performanceHistory.Count > MAX_HISTORY_SIZE)
            {
                _performanceHistory.Dequeue();
            }
        }
        
        private float CalculateAverageFrameRate()
        {
            if (_performanceHistory.Count == 0) return 0f;
            return _performanceHistory.Average(s => s.FrameRate);
        }
        
        private float CalculateMinFrameRate()
        {
            if (_performanceHistory.Count == 0) return 0f;
            return _performanceHistory.Min(s => s.FrameRate);
        }
        
        private float CalculateMaxFrameRate()
        {
            if (_performanceHistory.Count == 0) return 0f;
            return _performanceHistory.Max(s => s.FrameRate);
        }
        
        private long CalculateAverageMemoryUsage()
        {
            if (_performanceHistory.Count == 0) return 0;
            return (long)_performanceHistory.Average(s => s.MemoryUsage);
        }
        
        private long CalculatePeakMemoryUsage()
        {
            if (_performanceHistory.Count == 0) return 0;
            return _performanceHistory.Max(s => s.MemoryUsage);
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class PerformanceMetrics
    {
        public DateTime Timestamp;
        public float FrameRate;
        public long TotalMemory;
        public float MemoryUsageNormalized;
        public float CPUTime;
        public float GPUTime;
        public int DrawCalls;
        public int Triangles;
        public int GCCollections;
        
        public PerformanceMetrics()
        {
            Timestamp = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PerformanceSnapshot
    {
        public DateTime Timestamp;
        public float FrameRate;
        public long MemoryUsage;
        public float CPUTime;
        public float GPUTime;
        public int DrawCalls;
        public int Triangles;
    }
    
    [System.Serializable]
    public class PerformanceReport
    {
        public PerformanceMetrics CurrentMetrics;
        public float AverageFrameRate;
        public float MinFrameRate;
        public float MaxFrameRate;
        public long AverageMemoryUsage;
        public long PeakMemoryUsage;
        public int TotalGCCollections;
        public float MonitoringDuration;
        public DateTime ReportTime;
    }
    
    public enum PerformanceAlertType
    {
        LowFrameRate,
        HighMemoryUsage,
        GarbageCollection,
        HighCPUTime,
        HighGPUTime
    }
    
    [System.Serializable]
    public class PerformanceAlert
    {
        public PerformanceAlertType Type;
        public string Message;
        public DateTime Timestamp;
        public PerformanceMetrics Metrics;
    }
}