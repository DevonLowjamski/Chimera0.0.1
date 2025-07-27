using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1c: Comprehensive memory usage monitoring and leak detection system
    /// Provides real-time memory tracking, leak detection, and detailed profiling capabilities
    /// </summary>
    public class MemoryMonitoringSystem : ChimeraTestBase
    {
        [Header("Memory Monitoring Configuration")]
        [SerializeField] private bool _enableContinuousMonitoring = true;
        [SerializeField] private float _monitoringInterval = 1f; // Check every second
        [SerializeField] private int _maxMemorySnapshots = 1000; // Keep last 1000 snapshots
        [SerializeField] private bool _enableLeakDetection = true;
        [SerializeField] private bool _saveMemoryReports = true;
        
        [Header("Memory Thresholds")]
        [SerializeField] private long _warningThresholdMB = 512; // 512 MB warning
        [SerializeField] private long _criticalThresholdMB = 1024; // 1 GB critical
        [SerializeField] private long _leakDetectionThresholdMB = 100; // 100 MB increase = potential leak
        [SerializeField] private int _leakDetectionSamples = 10; // Check last 10 samples for leak trend
        
        [Header("Garbage Collection Monitoring")]
        [SerializeField] private bool _enableGCMonitoring = true;
        [SerializeField] private bool _logGCEvents = true;
        [SerializeField] private bool _enableDetailedGCStats = true;
        
        // Memory tracking data
        private List<MemorySnapshot> _memorySnapshots = new List<MemorySnapshot>();
        private Dictionary<string, long> _categoryMemoryTracking = new Dictionary<string, long>();
        private List<MemoryLeakReport> _detectedLeaks = new List<MemoryLeakReport>();
        
        // GC tracking
        private int _lastGCCount = 0;
        private float _lastGCTime = 0f;
        private List<GCEvent> _gcEvents = new List<GCEvent>();
        
        // Monitoring state
        private Coroutine _monitoringCoroutine;
        private bool _isMonitoring = false;
        private float _monitoringStartTime;
        
        // Analysis state
        private MemoryAnalysisResult _lastAnalysisResult;
        private DateTime _lastLeakCheckTime;
        
        public static MemoryMonitoringSystem Instance { get; private set; }
        
        // Events
        public event Action<MemoryLeakReport> OnMemoryLeakDetected;
        public event Action<MemorySnapshot> OnMemoryThresholdExceeded;
        public event Action<GCEvent> OnGarbageCollectionEvent;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMemoryMonitoring();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (_enableContinuousMonitoring)
            {
                StartMemoryMonitoring();
            }
        }
        
        private void OnDestroy()
        {
            StopMemoryMonitoring();
            SaveMemoryReport();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeMemoryMonitoring()
        {
            _monitoringStartTime = Time.time;
            _lastGCCount = System.GC.CollectionCount(0);
            _lastGCTime = Time.time;
            _lastLeakCheckTime = DateTime.Now;
            
            // Initialize memory category tracking
            _categoryMemoryTracking["TotalReserved"] = 0;
            _categoryMemoryTracking["TotalAllocated"] = 0;
            _categoryMemoryTracking["GfxReserved"] = 0;
            _categoryMemoryTracking["GfxUsed"] = 0;
            _categoryMemoryTracking["AudioReserved"] = 0;
            _categoryMemoryTracking["AudioUsed"] = 0;
            _categoryMemoryTracking["VideoReserved"] = 0;
            _categoryMemoryTracking["VideoUsed"] = 0;
            _categoryMemoryTracking["ProfilerReserved"] = 0;
            _categoryMemoryTracking["ProfilerUsed"] = 0;
            _categoryMemoryTracking["ManagedHeap"] = 0;
            
            LogInfo("PC016-1c: Memory monitoring system initialized");
        }
        
        #endregion
        
        #region Memory Monitoring Control
        
        /// <summary>
        /// Start continuous memory monitoring
        /// </summary>
        public void StartMemoryMonitoring()
        {
            if (_isMonitoring) return;
            
            _isMonitoring = true;
            _monitoringCoroutine = StartCoroutine(ContinuousMemoryMonitoring());
            
            LogInfo("PC016-1c: Memory monitoring started");
        }
        
        /// <summary>
        /// Stop continuous memory monitoring
        /// </summary>
        public void StopMemoryMonitoring()
        {
            if (!_isMonitoring) return;
            
            _isMonitoring = false;
            
            if (_monitoringCoroutine != null)
            {
                StopCoroutine(_monitoringCoroutine);
                _monitoringCoroutine = null;
            }
            
            LogInfo("PC016-1c: Memory monitoring stopped");
        }
        
        /// <summary>
        /// Take an immediate memory snapshot
        /// </summary>
        public MemorySnapshot TakeMemorySnapshot(string label = "Manual")
        {
            var snapshot = new MemorySnapshot
            {
                Timestamp = DateTime.Now,
                Label = label,
                GameTime = Time.time,
                
                // Unity Profiler memory data
                TotalReservedMemory = Profiler.GetTotalReservedMemoryLong(),
                TotalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong(),
                TotalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong(),
                
                // Graphics memory
                GraphicsReservedMemory = Profiler.GetAllocatedMemoryForGraphicsDriver(),
                
                // Audio memory (approximation)
                AudioReservedMemory = 0, // Unity doesn't provide direct API
                
                // Managed heap
                ManagedHeapSize = System.GC.GetTotalMemory(false),
                
                // Garbage collection info
                GCGen0Collections = System.GC.CollectionCount(0),
                GCGen1Collections = System.GC.CollectionCount(1),
                GCGen2Collections = System.GC.CollectionCount(2),
                
                // Unity specific (approximation)
                TextureMemory = 0, // Unity doesn't provide direct texture memory API
                MeshMemory = 0, // Unity doesn't provide direct mesh memory API
                AudioClipMemory = 0, // Unity doesn't provide direct audio memory API
                
                // Application info
                ApplicationMemory = SystemInfo.systemMemorySize * 1024L * 1024L, // Convert MB to bytes
                AvailableMemory = SystemInfo.systemMemorySize * 1024L * 1024L - Profiler.GetTotalAllocatedMemoryLong()
            };
            
            // Add to snapshots list
            _memorySnapshots.Add(snapshot);
            
            // Maintain maximum snapshots
            if (_memorySnapshots.Count > _maxMemorySnapshots)
            {
                _memorySnapshots.RemoveAt(0);
            }
            
            // Check thresholds
            CheckMemoryThresholds(snapshot);
            
            return snapshot;
        }
        
        #endregion
        
        #region Continuous Monitoring
        
        private IEnumerator ContinuousMemoryMonitoring()
        {
            while (_isMonitoring)
            {
                // Take memory snapshot
                var snapshot = TakeMemorySnapshot("Continuous");
                
                // Monitor garbage collection
                if (_enableGCMonitoring)
                {
                    MonitorGarbageCollection();
                }
                
                // Perform leak detection
                if (_enableLeakDetection && _memorySnapshots.Count >= _leakDetectionSamples)
                {
                    PerformLeakDetection();
                }
                
                // Update category tracking
                UpdateCategoryTracking(snapshot);
                
                yield return new WaitForSeconds(_monitoringInterval);
            }
        }
        
        private void MonitorGarbageCollection()
        {
            int currentGCCount = System.GC.CollectionCount(0);
            
            if (currentGCCount > _lastGCCount)
            {
                var gcEvent = new GCEvent
                {
                    Timestamp = DateTime.Now,
                    GameTime = Time.time,
                    Generation = 0, // Simplified - checking Gen 0 only
                    TimeSinceLastGC = Time.time - _lastGCTime,
                    MemoryBeforeGC = _memorySnapshots.LastOrDefault()?.ManagedHeapSize ?? 0,
                    MemoryAfterGC = System.GC.GetTotalMemory(false)
                };
                
                gcEvent.MemoryFreed = gcEvent.MemoryBeforeGC - gcEvent.MemoryAfterGC;
                
                _gcEvents.Add(gcEvent);
                
                // Maintain GC event history
                if (_gcEvents.Count > 100)
                {
                    _gcEvents.RemoveAt(0);
                }
                
                if (_logGCEvents)
                {
                    LogInfo($"PC016-1c: GC Event - Freed: {gcEvent.MemoryFreed / (1024 * 1024):F1}MB, " +
                           $"Interval: {gcEvent.TimeSinceLastGC:F2}s");
                }
                
                OnGarbageCollectionEvent?.Invoke(gcEvent);
                
                _lastGCCount = currentGCCount;
                _lastGCTime = Time.time;
            }
        }
        
        private void UpdateCategoryTracking(MemorySnapshot snapshot)
        {
            _categoryMemoryTracking["TotalReserved"] = snapshot.TotalReservedMemory;
            _categoryMemoryTracking["TotalAllocated"] = snapshot.TotalAllocatedMemory;
            _categoryMemoryTracking["GfxReserved"] = snapshot.GraphicsReservedMemory;
            _categoryMemoryTracking["ManagedHeap"] = snapshot.ManagedHeapSize;
        }
        
        #endregion
        
        #region Memory Leak Detection
        
        private void PerformLeakDetection()
        {
            if (DateTime.Now - _lastLeakCheckTime < TimeSpan.FromMinutes(5))
                return; // Don't check too frequently
            
            _lastLeakCheckTime = DateTime.Now;
            
            // Get recent snapshots for trend analysis
            var recentSnapshots = _memorySnapshots.TakeLast(_leakDetectionSamples).ToList();
            
            if (recentSnapshots.Count < _leakDetectionSamples)
                return;
            
            // Analyze memory growth trends
            var memoryCategories = new Dictionary<string, List<long>>
            {
                ["TotalAllocated"] = recentSnapshots.Select(s => s.TotalAllocatedMemory).ToList(),
                ["ManagedHeap"] = recentSnapshots.Select(s => s.ManagedHeapSize).ToList(),
                ["Graphics"] = recentSnapshots.Select(s => s.GraphicsReservedMemory).ToList()
            };
            
            foreach (var category in memoryCategories)
            {
                var trend = AnalyzeMemoryTrend(category.Value);
                
                if (trend.IsIncreasing && trend.AverageIncreasePerSample > _leakDetectionThresholdMB * 1024 * 1024 / _leakDetectionSamples)
                {
                    var leakReport = new MemoryLeakReport
                    {
                        DetectionTime = DateTime.Now,
                        Category = category.Key,
                        TrendAnalysis = trend,
                        Severity = CalculateLeakSeverity(trend),
                        RecommendedAction = GenerateLeakRecommendation(category.Key, trend),
                        AffectedSnapshots = recentSnapshots.ToList()
                    };
                    
                    _detectedLeaks.Add(leakReport);
                    OnMemoryLeakDetected?.Invoke(leakReport);
                    
                    LogWarning($"PC016-1c: MEMORY LEAK DETECTED in {category.Key} - " +
                              $"Growth: {trend.TotalIncrease / (1024 * 1024):F1}MB, " +
                              $"Severity: {leakReport.Severity}");
                }
            }
        }
        
        private MemoryTrendAnalysis AnalyzeMemoryTrend(List<long> memoryValues)
        {
            if (memoryValues.Count < 2)
                return new MemoryTrendAnalysis { IsIncreasing = false };
            
            var trend = new MemoryTrendAnalysis();
            var increases = 0;
            var decreases = 0;
            var totalIncrease = 0L;
            
            for (int i = 1; i < memoryValues.Count; i++)
            {
                var diff = memoryValues[i] - memoryValues[i - 1];
                if (diff > 0)
                {
                    increases++;
                    totalIncrease += diff;
                }
                else if (diff < 0)
                {
                    decreases++;
                }
            }
            
            trend.IsIncreasing = increases > decreases;
            trend.TotalIncrease = totalIncrease;
            trend.AverageIncreasePerSample = totalIncrease / Math.Max(1, increases);
            trend.IncreasingTrendStrength = (float)increases / (memoryValues.Count - 1);
            trend.MinValue = memoryValues.Min();
            trend.MaxValue = memoryValues.Max();
            trend.StartValue = memoryValues.First();
            trend.EndValue = memoryValues.Last();
            
            return trend;
        }
        
        private MemoryLeakSeverity CalculateLeakSeverity(MemoryTrendAnalysis trend)
        {
            var increaseMB = trend.TotalIncrease / (1024 * 1024);
            
            if (increaseMB > 500) return MemoryLeakSeverity.Critical;
            if (increaseMB > 200) return MemoryLeakSeverity.High;
            if (increaseMB > 50) return MemoryLeakSeverity.Medium;
            return MemoryLeakSeverity.Low;
        }
        
        private string GenerateLeakRecommendation(string category, MemoryTrendAnalysis trend)
        {
            switch (category)
            {
                case "ManagedHeap":
                    return "Check for unreferenced objects, event handler leaks, or excessive allocations. Consider calling GC.Collect() for testing.";
                    
                case "Graphics":
                    return "Check for texture leaks, unbounded mesh creation, or graphics buffer leaks. Verify proper disposal of graphics resources.";
                    
                case "TotalAllocated":
                    return "General memory leak detected. Review object lifecycle management, pooling systems, and resource cleanup.";
                    
                default:
                    return "Monitor memory usage patterns and identify sources of continuous allocation.";
            }
        }
        
        #endregion
        
        #region Memory Analysis
        
        /// <summary>
        /// Perform comprehensive memory analysis
        /// </summary>
        public MemoryAnalysisResult PerformMemoryAnalysis()
        {
            if (_memorySnapshots.Count == 0)
            {
                LogWarning("PC016-1c: No memory snapshots available for analysis");
                return null;
            }
            
            var result = new MemoryAnalysisResult
            {
                AnalysisTime = DateTime.Now,
                TotalSnapshots = _memorySnapshots.Count,
                MonitoringDuration = Time.time - _monitoringStartTime,
                
                // Basic statistics
                StartSnapshot = _memorySnapshots.First(),
                EndSnapshot = _memorySnapshots.Last(),
                PeakMemorySnapshot = _memorySnapshots.OrderByDescending(s => s.TotalAllocatedMemory).First(),
                
                // Memory growth analysis
                TotalMemoryGrowth = _memorySnapshots.Last().TotalAllocatedMemory - _memorySnapshots.First().TotalAllocatedMemory,
                ManagedHeapGrowth = _memorySnapshots.Last().ManagedHeapSize - _memorySnapshots.First().ManagedHeapSize,
                
                // GC analysis
                TotalGCEvents = _gcEvents.Count,
                AverageGCInterval = _gcEvents.Count > 1 ? _gcEvents.Average(gc => gc.TimeSinceLastGC) : 0f,
                TotalMemoryFreedByGC = _gcEvents.Sum(gc => gc.MemoryFreed),
                
                // Leak detection results
                DetectedLeaks = _detectedLeaks.ToList(),
                HasMemoryLeaks = _detectedLeaks.Any(leak => leak.Severity >= MemoryLeakSeverity.Medium)
            };
            
            // Calculate memory efficiency metrics
            result.MemoryEfficiency = CalculateMemoryEfficiency();
            result.GCEfficiency = CalculateGCEfficiency();
            
            _lastAnalysisResult = result;
            
            LogInfo($"PC016-1c: Memory analysis completed - Growth: {result.TotalMemoryGrowth / (1024 * 1024):F1}MB, " +
                   $"GC Events: {result.TotalGCEvents}, Leaks: {result.DetectedLeaks.Count}");
            
            return result;
        }
        
        private float CalculateMemoryEfficiency()
        {
            if (_memorySnapshots.Count < 2) return 1f;
            
            var totalAllocated = _memorySnapshots.Last().TotalAllocatedMemory;
            var totalReserved = _memorySnapshots.Last().TotalReservedMemory;
            
            return totalReserved > 0 ? (float)totalAllocated / totalReserved : 1f;
        }
        
        private float CalculateGCEfficiency()
        {
            if (_gcEvents.Count == 0) return 1f;
            
            var avgMemoryFreed = _gcEvents.Average(gc => gc.MemoryFreed);
            var avgHeapSize = _memorySnapshots.Average(s => s.ManagedHeapSize);
            
            return avgHeapSize > 0 ? (float)(avgMemoryFreed / avgHeapSize) : 0f;
        }
        
        #endregion
        
        #region Threshold Monitoring
        
        private void CheckMemoryThresholds(MemorySnapshot snapshot)
        {
            var totalMemoryMB = snapshot.TotalAllocatedMemory / (1024 * 1024);
            
            if (totalMemoryMB > _criticalThresholdMB)
            {
                LogError($"PC016-1c: CRITICAL MEMORY USAGE - {totalMemoryMB}MB (Threshold: {_criticalThresholdMB}MB)");
                OnMemoryThresholdExceeded?.Invoke(snapshot);
            }
            else if (totalMemoryMB > _warningThresholdMB)
            {
                LogWarning($"PC016-1c: Memory usage warning - {totalMemoryMB}MB (Threshold: {_warningThresholdMB}MB)");
            }
        }
        
        #endregion
        
        #region Data Export and Reporting
        
        /// <summary>
        /// Generate comprehensive memory report
        /// </summary>
        public void GenerateMemoryReport()
        {
            if (_memorySnapshots.Count == 0)
            {
                LogWarning("PC016-1c: No memory data available for report generation");
                return;
            }
            
            var analysis = PerformMemoryAnalysis();
            
            LogInfo("PC016-1c: === MEMORY MONITORING REPORT ===");
            LogInfo($"Monitoring Duration: {analysis.MonitoringDuration:F1} seconds");
            LogInfo($"Total Snapshots: {analysis.TotalSnapshots}");
            LogInfo($"Memory Growth: {analysis.TotalMemoryGrowth / (1024 * 1024):F1}MB");
            LogInfo($"Managed Heap Growth: {analysis.ManagedHeapGrowth / (1024 * 1024):F1}MB");
            LogInfo($"Peak Memory Usage: {analysis.PeakMemorySnapshot.TotalAllocatedMemory / (1024 * 1024):F1}MB");
            LogInfo($"GC Events: {analysis.TotalGCEvents}");
            LogInfo($"Memory Efficiency: {analysis.MemoryEfficiency:P1}");
            LogInfo($"GC Efficiency: {analysis.GCEfficiency:P1}");
            LogInfo($"Memory Leaks Detected: {analysis.DetectedLeaks.Count}");
            
            if (analysis.HasMemoryLeaks)
            {
                LogWarning("PC016-1c: === MEMORY LEAK DETAILS ===");
                foreach (var leak in analysis.DetectedLeaks)
                {
                    LogWarning($"Leak in {leak.Category}: {leak.TrendAnalysis.TotalIncrease / (1024 * 1024):F1}MB growth, Severity: {leak.Severity}");
                    LogWarning($"  Recommendation: {leak.RecommendedAction}");
                }
            }
            else
            {
                LogInfo("PC016-1c: No significant memory leaks detected");
            }
        }
        
        /// <summary>
        /// Save memory report to file
        /// </summary>
        public void SaveMemoryReport()
        {
            if (!_saveMemoryReports || _memorySnapshots.Count == 0) return;
            
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"MemoryReport_{timestamp}.json";
                var filepath = Path.Combine(Application.persistentDataPath, "MemoryReports", filename);
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                
                var reportData = new MemoryReportData
                {
                    ReportTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    SystemInfo = new SystemInfoData
                    {
                        SystemMemorySize = SystemInfo.systemMemorySize,
                        GraphicsMemorySize = SystemInfo.graphicsMemorySize,
                        ProcessorType = SystemInfo.processorType,
                        OperatingSystem = SystemInfo.operatingSystem
                    },
                    Analysis = _lastAnalysisResult ?? PerformMemoryAnalysis(),
                    MemorySnapshots = _memorySnapshots.TakeLast(100).ToList(), // Save last 100 snapshots
                    GCEvents = _gcEvents.TakeLast(50).ToList(), // Save last 50 GC events
                    DetectedLeaks = _detectedLeaks.ToList()
                };
                
                var json = JsonUtility.ToJson(reportData, true);
                File.WriteAllText(filepath, json);
                
                LogInfo($"PC016-1c: Memory report saved to {filepath}");
            }
            catch (Exception ex)
            {
                LogError($"PC016-1c: Failed to save memory report: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get current memory usage statistics
        /// </summary>
        public MemorySnapshot GetCurrentMemorySnapshot()
        {
            return TakeMemorySnapshot("API_Request");
        }
        
        /// <summary>
        /// Get all memory snapshots
        /// </summary>
        public List<MemorySnapshot> GetAllMemorySnapshots()
        {
            return new List<MemorySnapshot>(_memorySnapshots);
        }
        
        /// <summary>
        /// Get detected memory leaks
        /// </summary>
        public List<MemoryLeakReport> GetDetectedLeaks()
        {
            return new List<MemoryLeakReport>(_detectedLeaks);
        }
        
        /// <summary>
        /// Clear all monitoring data
        /// </summary>
        public void ClearMonitoringData()
        {
            _memorySnapshots.Clear();
            _gcEvents.Clear();
            _detectedLeaks.Clear();
            _categoryMemoryTracking.Clear();
            
            LogInfo("PC016-1c: Memory monitoring data cleared");
        }
        
        /// <summary>
        /// Force garbage collection for testing
        /// </summary>
        public void ForceGarbageCollection()
        {
            var beforeMemory = System.GC.GetTotalMemory(false);
            
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            var afterMemory = System.GC.GetTotalMemory(false);
            var freedMemory = beforeMemory - afterMemory;
            
            LogInfo($"PC016-1c: Forced GC - Freed: {freedMemory / (1024 * 1024):F1}MB");
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [Serializable]
    public class MemorySnapshot
    {
        public DateTime Timestamp;
        public string Label;
        public float GameTime;
        
        // Unity memory data
        public long TotalReservedMemory;
        public long TotalAllocatedMemory;
        public long TotalUnusedReservedMemory;
        public long GraphicsReservedMemory;
        public long AudioReservedMemory;
        public long TextureMemory;
        public long MeshMemory;
        public long AudioClipMemory;
        
        // Managed heap
        public long ManagedHeapSize;
        
        // Garbage collection
        public int GCGen0Collections;
        public int GCGen1Collections;
        public int GCGen2Collections;
        
        // System memory
        public long ApplicationMemory;
        public long AvailableMemory;
    }
    
    [Serializable]
    public class MemoryLeakReport
    {
        public DateTime DetectionTime;
        public string Category;
        public MemoryTrendAnalysis TrendAnalysis;
        public MemoryLeakSeverity Severity;
        public string RecommendedAction;
        public List<MemorySnapshot> AffectedSnapshots;
    }
    
    [Serializable]
    public class MemoryTrendAnalysis
    {
        public bool IsIncreasing;
        public long TotalIncrease;
        public long AverageIncreasePerSample;
        public float IncreasingTrendStrength;
        public long MinValue;
        public long MaxValue;
        public long StartValue;
        public long EndValue;
    }
    
    [Serializable]
    public class GCEvent
    {
        public DateTime Timestamp;
        public float GameTime;
        public int Generation;
        public float TimeSinceLastGC;
        public long MemoryBeforeGC;
        public long MemoryAfterGC;
        public long MemoryFreed;
    }
    
    [Serializable]
    public class MemoryAnalysisResult
    {
        public DateTime AnalysisTime;
        public int TotalSnapshots;
        public float MonitoringDuration;
        
        public MemorySnapshot StartSnapshot;
        public MemorySnapshot EndSnapshot;
        public MemorySnapshot PeakMemorySnapshot;
        
        public long TotalMemoryGrowth;
        public long ManagedHeapGrowth;
        
        public int TotalGCEvents;
        public float AverageGCInterval;
        public long TotalMemoryFreedByGC;
        
        public List<MemoryLeakReport> DetectedLeaks;
        public bool HasMemoryLeaks;
        
        public float MemoryEfficiency;
        public float GCEfficiency;
    }
    
    [Serializable]
    public class MemoryReportData
    {
        public string ReportTimestamp;
        public SystemInfoData SystemInfo;
        public MemoryAnalysisResult Analysis;
        public List<MemorySnapshot> MemorySnapshots;
        public List<GCEvent> GCEvents;
        public List<MemoryLeakReport> DetectedLeaks;
    }
    
    [Serializable]
    public class SystemInfoData
    {
        public int SystemMemorySize;
        public int GraphicsMemorySize;
        public string ProcessorType;
        public string OperatingSystem;
    }
    
    public enum MemoryLeakSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    #endregion
}