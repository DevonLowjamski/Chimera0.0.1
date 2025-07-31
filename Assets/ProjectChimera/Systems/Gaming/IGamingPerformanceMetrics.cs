using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Interface for Gaming-Specific Performance Metrics - Comprehensive gaming performance tracking
    /// Part of Module 1: Gaming Experience Core - Deliverable 2.3
    /// 
    /// Provides comprehensive performance metrics and analytics specifically designed for gaming applications,
    /// including advanced profiling, bottleneck detection, and performance trend analysis.
    /// </summary>
    public interface IGamingPerformanceMetrics : IGamingService
    {
        #region Metric Collection Control
        
        /// <summary>
        /// Enable/disable comprehensive metrics collection
        /// </summary>
        bool EnableMetricsCollection { get; set; }
        
        /// <summary>
        /// Enable/disable detailed profiling (may impact performance)
        /// </summary>
        bool EnableDetailedProfiling { get; set; }
        
        /// <summary>
        /// Metrics collection frequency (samples per second)
        /// </summary>
        float MetricsCollectionFrequency { get; set; }
        
        /// <summary>
        /// Maximum number of metric samples to retain in memory
        /// </summary>
        int MaxMetricSamples { get; set; }
        
        #endregion
        
        #region Real-Time Gaming Metrics
        
        /// <summary>
        /// Current comprehensive gaming performance metrics
        /// </summary>
        GamingMetricsSnapshot CurrentMetrics { get; }
        
        /// <summary>
        /// Frame rate metrics and analysis
        /// </summary>
        FrameRateMetrics FrameRateMetrics { get; }
        
        /// <summary>
        /// Rendering performance metrics
        /// </summary>
        RenderingMetrics RenderingMetrics { get; }
        
        /// <summary>
        /// Memory usage and allocation metrics
        /// </summary>
        MemoryMetrics MemoryMetrics { get; }
        
        /// <summary>
        /// Input latency and responsiveness metrics
        /// </summary>
        InputLatencyMetrics InputLatencyMetrics { get; }
        
        /// <summary>
        /// CPU performance metrics
        /// </summary>
        CPUMetrics CPUMetrics { get; }
        
        /// <summary>
        /// GPU performance metrics
        /// </summary>
        GPUMetrics GPUMetrics { get; }
        
        #endregion
        
        #region Performance Analysis
        
        /// <summary>
        /// Detect and analyze performance bottlenecks
        /// </summary>
        BottleneckAnalysis AnalyzeBottlenecks();
        
        /// <summary>
        /// Get performance trend analysis over time
        /// </summary>
        PerformanceTrendAnalysis GetTrendAnalysis(TimeSpan duration);
        
        /// <summary>
        /// Calculate overall gaming performance score (0-100)
        /// </summary>
        float CalculatePerformanceScore();
        
        /// <summary>
        /// Get optimization recommendations based on current metrics
        /// </summary>
        List<OptimizationRecommendation> GetOptimizationRecommendations();
        
        #endregion
        
        #region Metric History & Analytics
        
        /// <summary>
        /// Get metric history for a specific time range
        /// </summary>
        MetricHistory GetMetricHistory(MetricType metricType, TimeSpan duration);
        
        /// <summary>
        /// Get performance statistics for the current session
        /// </summary>
        GamingSessionStatistics GetSessionStatistics();
        
        /// <summary>
        /// Get comparative performance data against previous sessions
        /// </summary>
        ComparativePerformanceData GetComparativeData();
        
        /// <summary>
        /// Export performance data for external analysis
        /// </summary>
        string ExportPerformanceData(DataExportFormat format);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when a performance bottleneck is detected
        /// </summary>
        event Action<BottleneckDetectedEventData> OnBottleneckDetected;
        
        /// <summary>
        /// Event fired when performance metrics are updated
        /// </summary>
        event Action<GamingMetricsSnapshot> OnMetricsUpdated;
        
        /// <summary>
        /// Event fired when performance anomaly is detected
        /// </summary>
        event Action<PerformanceAnomalyEventData> OnPerformanceAnomaly;
        
        /// <summary>
        /// Event fired when new optimization recommendation is available
        /// </summary>
        event Action<OptimizationRecommendation> OnOptimizationRecommendation;
        
        #endregion
        
        #region Manual Controls
        
        /// <summary>
        /// Force immediate metrics collection and analysis
        /// </summary>
        void CollectMetricsNow();
        
        /// <summary>
        /// Start performance profiling session
        /// </summary>
        void StartProfilingSession(string sessionName);
        
        /// <summary>
        /// Stop current profiling session and generate report
        /// </summary>
        ProfilingReport StopProfilingSession();
        
        /// <summary>
        /// Reset all collected metrics and start fresh
        /// </summary>
        void ResetMetrics();
        
        /// <summary>
        /// Take performance benchmark snapshot
        /// </summary>
        BenchmarkSnapshot TakeBenchmark(string benchmarkName);
        
        #endregion
    }
    
    /// <summary>
    /// Types of performance metrics that can be tracked
    /// </summary>
    public enum MetricType
    {
        FrameRate,
        FrameTime,
        CPUUsage,
        GPUUsage,
        MemoryUsage,
        InputLatency,
        RenderTime,
        UpdateTime,
        DrawCalls,
        SetPassCalls,
        Triangles,
        Vertices
    }
    
    /// <summary>
    /// Data export formats for performance data
    /// </summary>
    public enum DataExportFormat
    {
        JSON,
        CSV,
        XML
    }
    
    /// <summary>
    /// Comprehensive gaming performance metrics snapshot
    /// </summary>
    [Serializable]
    public struct GamingMetricsSnapshot
    {
        [Header("Timestamp")]
        public DateTime Timestamp;
        public float SessionTime;
        
        [Header("Frame Rate")]
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float FrameTimeMS;
        public float FrameTimeVariance;
        
        [Header("System Performance")]
        public float CPUUsagePercent;
        public float GPUUsagePercent;
        public float MemoryUsageMB;
        public float GPUMemoryUsageMB;
        
        [Header("Rendering")]
        public int DrawCalls;
        public int SetPassCalls;
        public int Triangles;
        public int Vertices;
        public float RenderTimeMS;
        
        [Header("Input")]
        public float InputLatencyMS;
        public float InputResponseTime;
        
        [Header("Quality")]
        public int QualityLevel;
        public float RenderScale;
        public bool VSyncEnabled;
        
        [Header("Performance State")]
        public GamingPerformanceState PerformanceState;
        public float PerformanceScore;
        public bool IsStable;
        
        public override string ToString()
        {
            return $"FPS: {CurrentFPS:F1} | CPU: {CPUUsagePercent:F1}% | GPU: {GPUUsagePercent:F1}% | Mem: {MemoryUsageMB:F0}MB | Score: {PerformanceScore:F1}";
        }
    }
    
    /// <summary>
    /// Frame rate specific metrics
    /// </summary>
    [Serializable]
    public struct FrameRateMetrics
    {
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float FrameTimeMS;
        public float FrameTimeStandardDeviation;
        public float FrameDropCount;
        public float SmoothnessFactor;
        public bool IsFrameRateStable;
        
        public override string ToString()
        {
            return $"FPS: {CurrentFPS:F1} (Avg: {AverageFPS:F1}, Range: {MinFPS:F1}-{MaxFPS:F1}) | Smooth: {SmoothnessFactor:F2}";
        }
    }
    
    /// <summary>
    /// Rendering performance metrics
    /// </summary>
    [Serializable]
    public struct RenderingMetrics
    {
        public float RenderTimeMS;
        public float GPUFrameTimeMS;
        public int DrawCalls;
        public int SetPassCalls;
        public int Triangles;
        public int Vertices;
        public int BatchedDrawCalls;
        public float FillRateUtilization;
        public float TextureMemoryUsage;
        public float ShaderCompilationTime;
        
        public override string ToString()
        {
            return $"Render: {RenderTimeMS:F2}ms | DrawCalls: {DrawCalls} | Tris: {Triangles}k | Fill: {FillRateUtilization:F1}%";
        }
    }
    
    /// <summary>
    /// Memory usage and allocation metrics
    /// </summary>
    [Serializable]
    public struct MemoryMetrics
    {
        public float TotalMemoryMB;
        public float MonoMemoryMB;
        public float GraphicsMemoryMB;
        public float AudioMemoryMB;
        public float ProfilerMemoryMB;
        public int GCCollections;
        public float GCAllocatedPerFrame;
        public float MemoryFragmentation;
        public bool IsMemoryPressure;
        
        public override string ToString()
        {
            return $"Memory: {TotalMemoryMB:F0}MB | Mono: {MonoMemoryMB:F0}MB | GFX: {GraphicsMemoryMB:F0}MB | GC: {GCCollections}";
        }
    }
    
    /// <summary>
    /// Input latency and responsiveness metrics
    /// </summary>
    [Serializable]
    public struct InputLatencyMetrics
    {
        public float InputLatencyMS;
        public float DisplayLatencyMS;
        public float TotalLatencyMS;
        public float InputJitter;
        public bool IsInputResponsive;
        public float LastInputTime;
        
        public override string ToString()
        {
            return $"Input Latency: {InputLatencyMS:F1}ms | Display: {DisplayLatencyMS:F1}ms | Total: {TotalLatencyMS:F1}ms";
        }
    }
    
    /// <summary>
    /// CPU performance metrics
    /// </summary>
    [Serializable]
    public struct CPUMetrics
    {
        public float CPUUsagePercent;
        public float MainThreadTimeMS;
        public float RenderThreadTimeMS;
        public float WorkerThreadsTimeMS;
        public int ActiveThreads;
        public float CPUTemperature;
        public bool IsCPUBottleneck;
        
        public override string ToString()
        {
            return $"CPU: {CPUUsagePercent:F1}% | Main: {MainThreadTimeMS:F1}ms | Render: {RenderThreadTimeMS:F1}ms | Threads: {ActiveThreads}";
        }
    }
    
    /// <summary>
    /// GPU performance metrics
    /// </summary>
    [Serializable]
    public struct GPUMetrics
    {
        public float GPUUsagePercent;
        public float GPUMemoryUsageMB;
        public float GPUMemoryBandwidth;
        public float GPUTemperature;
        public float ShaderExecutionTime;
        public bool IsGPUBottleneck;
        public string GPUName;
        
        public override string ToString()
        {
            return $"GPU: {GPUUsagePercent:F1}% | VRAM: {GPUMemoryUsageMB:F0}MB | Temp: {GPUTemperature:F0}°C | {GPUName}";
        }
    }
    
    /// <summary>
    /// Performance bottleneck analysis results
    /// </summary>
    [Serializable]
    public class BottleneckAnalysis
    {
        public BottleneckType PrimaryBottleneck;
        public float BottleneckSeverity; // 0-1
        public List<BottleneckFactor> ContributingFactors;
        public string Description;
        public List<string> Recommendations;
        public DateTime AnalysisTime;
        
        public override string ToString()
        {
            return $"Primary Bottleneck: {PrimaryBottleneck} (Severity: {BottleneckSeverity:F2}) - {Description}";
        }
    }
    
    /// <summary>
    /// Types of performance bottlenecks
    /// </summary>
    public enum BottleneckType
    {
        None,
        CPU,
        GPU,
        Memory,
        Storage,
        Network,
        Input,
        Rendering,
        Physics,
        Audio
    }
    
    /// <summary>
    /// Factors contributing to performance bottlenecks
    /// </summary>
    [Serializable]
    public struct BottleneckFactor
    {
        public string Name;
        public float Impact; // 0-1
        public string Description;
    }
    
    /// <summary>
    /// Performance trend analysis over time
    /// </summary>
    [Serializable]
    public class PerformanceTrendAnalysis
    {
        public TimeSpan AnalysisDuration;
        public TrendDirection FPSTrend;
        public TrendDirection MemoryTrend;
        public TrendDirection CPUTrend;
        public TrendDirection GPUTrend;
        public float StabilityScore; // 0-1
        public List<PerformanceRegression> Regressions;
        public List<PerformanceImprovement> Improvements;
        public string Summary;
        
        public override string ToString()
        {
            return $"Trends ({AnalysisDuration.TotalMinutes:F1}min): FPS {FPSTrend}, Memory {MemoryTrend}, Stability: {StabilityScore:F2}";
        }
    }
    
    /// <summary>
    /// Trend directions for performance metrics
    /// </summary>
    public enum TrendDirection
    {
        Stable,
        Improving,
        Degrading,
        Volatile
    }
    
    /// <summary>
    /// Performance regression information
    /// </summary>
    [Serializable]
    public struct PerformanceRegression
    {
        public MetricType AffectedMetric;
        public float RegressionAmount;
        public DateTime DetectedTime;
        public string PossibleCause;
    }
    
    /// <summary>
    /// Performance improvement information
    /// </summary>
    [Serializable]
    public struct PerformanceImprovement
    {
        public MetricType AffectedMetric;
        public float ImprovementAmount;
        public DateTime DetectedTime;
        public string PossibleCause;
    }
    
    /// <summary>
    /// Optimization recommendation
    /// </summary>
    [Serializable]
    public class OptimizationRecommendation
    {
        public string Title;
        public string Description;
        public OptimizationPriority Priority;
        public float EstimatedImpact; // 0-1
        public OptimizationCategory Category;
        public List<string> ActionItems;
        public bool IsAutoApplicable;
        public DateTime GeneratedTime;
        
        public override string ToString()
        {
            return $"{Priority} - {Title}: {Description} (Impact: {EstimatedImpact:F2})";
        }
    }
    
    /// <summary>
    /// Optimization recommendation priorities
    /// </summary>
    public enum OptimizationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Optimization categories
    /// </summary>
    public enum OptimizationCategory
    {
        Rendering,
        Memory,
        CPU,
        GPU,
        Input,
        Quality,
        System
    }
    
    /// <summary>
    /// Metric history data
    /// </summary>
    [Serializable]
    public class MetricHistory
    {
        public MetricType MetricType;
        public List<MetricDataPoint> DataPoints;
        public float Average;
        public float Minimum;
        public float Maximum;
        public float StandardDeviation;
        public TimeSpan Duration;
        
        public override string ToString()
        {
            return $"{MetricType} History: Avg {Average:F2}, Range {Minimum:F2}-{Maximum:F2}, StdDev {StandardDeviation:F2}";
        }
    }
    
    /// <summary>
    /// Individual metric data point
    /// </summary>
    [Serializable]
    public struct MetricDataPoint
    {
        public DateTime Timestamp;
        public float Value;
        public string Context;
    }
    
    /// <summary>
    /// Gaming session statistics
    /// </summary>
    [Serializable]
    public class GamingSessionStatistics
    {
        public DateTime SessionStart;
        public TimeSpan SessionDuration;
        public float AveragePerformanceScore;
        public int TotalFramesRendered;
        public float AverageFPS;
        public int PerformanceDropEvents;
        public int QualityAdjustments;
        public List<BottleneckType> DetectedBottlenecks;
        public float TotalMemoryAllocated;
        public int OptimizationsApplied;
        
        public override string ToString()
        {
            return $"Session: {SessionDuration.TotalMinutes:F1}min | Avg FPS: {AverageFPS:F1} | Score: {AveragePerformanceScore:F1} | Drops: {PerformanceDropEvents}";
        }
    }
    
    /// <summary>
    /// Comparative performance data
    /// </summary>
    [Serializable]
    public struct ComparativePerformanceData
    {
        public float CurrentSessionScore;
        public float PreviousSessionScore;
        public float ScoreChange;
        public TrendDirection OverallTrend;
        public Dictionary<MetricType, float> MetricComparisons;
        public List<string> ImprovementAreas;
        public List<string> RegressionAreas;
    }
    
    /// <summary>
    /// Profiling report from a profiling session
    /// </summary>
    [Serializable]
    public class ProfilingReport
    {
        public string SessionName;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public GamingMetricsSnapshot StartSnapshot;
        public GamingMetricsSnapshot EndSnapshot;
        public List<PerformanceRegression> Regressions;
        public List<PerformanceImprovement> Improvements;
        public BottleneckAnalysis BottleneckAnalysis;
        public List<OptimizationRecommendation> Recommendations;
        public string Summary;
        
        public override string ToString()
        {
            return $"Profiling Report '{SessionName}': {Duration.TotalSeconds:F1}s | Regressions: {Regressions.Count} | Improvements: {Improvements.Count}";
        }
    }
    
    /// <summary>
    /// Performance benchmark snapshot
    /// </summary>
    [Serializable]
    public struct BenchmarkSnapshot
    {
        public string BenchmarkName;
        public DateTime Timestamp;
        public GamingMetricsSnapshot Metrics;
        public float BenchmarkScore;
        public Dictionary<string, float> CustomMetrics;
        public string SystemInfo;
        
        public override string ToString()
        {
            return $"Benchmark '{BenchmarkName}': Score {BenchmarkScore:F1} | {Metrics}";
        }
    }
    
    /// <summary>
    /// Bottleneck detected event data
    /// </summary>
    [Serializable]
    public struct BottleneckDetectedEventData
    {
        public BottleneckType BottleneckType;
        public float Severity;
        public string Description;
        public List<string> ImmediateActions;
        public DateTime DetectionTime;
        
        public override string ToString()
        {
            return $"Bottleneck Detected: {BottleneckType} (Severity: {Severity:F2}) - {Description}";
        }
    }
    
    /// <summary>
    /// Performance anomaly event data
    /// </summary>
    [Serializable]
    public struct PerformanceAnomalyEventData
    {
        public AnomalyType AnomalyType;
        public MetricType AffectedMetric;
        public float ExpectedValue;
        public float ActualValue;
        public float DeviationAmount;
        public string Description;
        public DateTime DetectionTime;
        
        public override string ToString()
        {
            return $"Performance Anomaly: {AnomalyType} in {AffectedMetric} | Expected: {ExpectedValue:F2}, Actual: {ActualValue:F2}";
        }
    }
    
    /// <summary>
    /// Types of performance anomalies
    /// </summary>
    public enum AnomalyType
    {
        Spike,
        Drop,
        Plateau,
        Oscillation,
        Drift
    }
}