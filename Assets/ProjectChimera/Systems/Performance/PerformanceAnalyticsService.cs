using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Performance Analytics Service - Dedicated service for trend analysis and performance reporting
    /// Extracted from UnifiedPerformanceManagementSystem to provide focused analytics functionality
    /// Handles trend analysis, bottleneck detection, performance reporting, and predictive analytics
    /// </summary>
    public class PerformanceAnalyticsService : MonoBehaviour
    {
        [Header("Analytics Configuration")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private float _analysisInterval = 5f;
        [SerializeField] private int _minDataPointsForAnalysis = 60;
        [SerializeField] private int _maxDataHistorySize = 600; // 10 minutes at 60 FPS

        [Header("Trend Detection")]
        [SerializeField] private float _varianceThreshold = 0.01f;
        [SerializeField] private float _trendThreshold = 0.1f;
        [SerializeField] private float _bottleneckThreshold = 0.25f; // 25% of total CPU time
        [SerializeField] private int _trendSampleSize = 60;

        [Header("Report Generation")]
        [SerializeField] private bool _enablePeriodicReports = true;
        [SerializeField] private float _reportGenerationInterval = 30f;
        [SerializeField] private int _maxReportHistory = 20;

        // Analytics data storage
        private List<FramePerformanceData> _frameDataHistory = new List<FramePerformanceData>();
        private List<PerformanceReport> _reportHistory = new List<PerformanceReport>();
        private Dictionary<string, PerformanceTrend> _systemTrends = new Dictionary<string, PerformanceTrend>();
        private Dictionary<string, AnalyticsProfile> _systemAnalytics = new Dictionary<string, AnalyticsProfile>();

        // Analytics state
        private bool _isInitialized = false;
        private float _lastAnalysisTime = 0f;
        private float _lastReportTime = 0f;
        private int _totalAnalysisRuns = 0;

        // Events for analytics notifications
        public static event System.Action<PerformanceTrend> OnTrendDetected;
        public static event System.Action<PerformanceReport> OnReportGenerated;
        public static event System.Action<BottleneckAnalysis> OnBottleneckDetected;
        public static event System.Action<PredictiveInsight> OnPredictiveInsight;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Performance Analytics Service";
        public IReadOnlyList<FramePerformanceData> FrameDataHistory => _frameDataHistory;
        public IReadOnlyList<PerformanceReport> ReportHistory => _reportHistory;
        public IReadOnlyDictionary<string, PerformanceTrend> SystemTrends => _systemTrends;

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
            _frameDataHistory = new List<FramePerformanceData>();
            _reportHistory = new List<PerformanceReport>();
            _systemTrends = new Dictionary<string, PerformanceTrend>();
            _systemAnalytics = new Dictionary<string, AnalyticsProfile>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PerformanceAnalyticsService already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                
                _isInitialized = true;
                ChimeraLogger.Log("PerformanceAnalyticsService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PerformanceAnalyticsService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            GenerateFinalReport();
            _frameDataHistory.Clear();
            _reportHistory.Clear();
            _systemTrends.Clear();
            _systemAnalytics.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("PerformanceAnalyticsService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_analysisInterval <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid analysis interval, using default 5s", this);
                _analysisInterval = 5f;
            }

            if (_minDataPointsForAnalysis < 10)
            {
                ChimeraLogger.LogWarning("Minimum data points too low, using default 60", this);
                _minDataPointsForAnalysis = 60;
            }
        }

        #endregion

        #region Data Collection

        private void Update()
        {
            if (!_isInitialized || !_enableAnalytics) return;

            // Run analysis at specified interval
            if (Time.time - _lastAnalysisTime >= _analysisInterval)
            {
                PerformAnalysis();
                _lastAnalysisTime = Time.time;
            }

            // Generate reports periodically
            if (_enablePeriodicReports && Time.time - _lastReportTime >= _reportGenerationInterval)
            {
                GeneratePerformanceReport();
                _lastReportTime = Time.time;
            }
        }

        /// <summary>
        /// Add frame data for analysis
        /// </summary>
        public void AddFrameData(FramePerformanceData frameData)
        {
            _frameDataHistory.Add(frameData);

            // Maintain history size
            if (_frameDataHistory.Count > _maxDataHistorySize)
            {
                _frameDataHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Add system performance data for analysis
        /// </summary>
        public void AddSystemData(string systemName, PerformanceProfile systemProfile)
        {
            if (!_systemAnalytics.ContainsKey(systemName))
            {
                _systemAnalytics[systemName] = new AnalyticsProfile
                {
                    SystemName = systemName,
                    CpuTimeHistory = new List<float>(),
                    MemoryUsageHistory = new List<long>(),
                    FirstDataPoint = DateTime.Now
                };
            }

            var analytics = _systemAnalytics[systemName];
            analytics.CpuTimeHistory.Add(systemProfile.AverageCpuTime);
            analytics.MemoryUsageHistory.Add(systemProfile.AverageMemoryUsage);
            analytics.LastDataPoint = DateTime.Now;

            // Maintain history size
            if (analytics.CpuTimeHistory.Count > _maxDataHistorySize)
            {
                analytics.CpuTimeHistory.RemoveAt(0);
            }
            if (analytics.MemoryUsageHistory.Count > _maxDataHistorySize)
            {
                analytics.MemoryUsageHistory.RemoveAt(0);
            }
        }

        #endregion

        #region Trend Analysis

        /// <summary>
        /// Perform comprehensive performance analysis
        /// </summary>
        private void PerformAnalysis()
        {
            if (_frameDataHistory.Count < _minDataPointsForAnalysis) return;

            AnalyzeFrameDataTrends();
            AnalyzeSystemTrends();
            DetectBottlenecks();
            GeneratePredictiveInsights();

            _totalAnalysisRuns++;
        }

        /// <summary>
        /// Analyze frame data trends
        /// </summary>
        private void AnalyzeFrameDataTrends()
        {
            var recentFrames = _frameDataHistory.TakeLast(_trendSampleSize).ToList();
            if (recentFrames.Count < _trendSampleSize) return;

            // Frame time analysis
            var frameTimeVariance = CalculateVariance(recentFrames.Select(f => f.FrameTime));
            var frameTimeTrend = CalculateTrend(recentFrames.Select(f => f.FrameTime));

            if (frameTimeVariance > _varianceThreshold)
            {
                var trend = new PerformanceTrend
                {
                    SystemName = "Global",
                    MetricName = "FrameTime",
                    TrendDirection = frameTimeTrend > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing,
                    Magnitude = frameTimeVariance,
                    Confidence = CalculateConfidence(frameTimeVariance, _varianceThreshold),
                    DetectedAt = DateTime.Now,
                    Description = "Frame time instability detected"
                };

                _systemTrends["Global_FrameTime"] = trend;
                OnTrendDetected?.Invoke(trend);
            }

            // Memory usage analysis
            var memoryTrend = CalculateTrend(recentFrames.Select(f => (float)f.MemoryUsage));
            if (Mathf.Abs(memoryTrend) > _trendThreshold)
            {
                var trend = new PerformanceTrend
                {
                    SystemName = "Global",
                    MetricName = "MemoryUsage",
                    TrendDirection = memoryTrend > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing,
                    Magnitude = Mathf.Abs(memoryTrend),
                    Confidence = CalculateConfidence(Mathf.Abs(memoryTrend), _trendThreshold),
                    DetectedAt = DateTime.Now,
                    Description = $"Memory usage trending {(memoryTrend > 0 ? "upward" : "downward")}"
                };

                _systemTrends["Global_Memory"] = trend;
                OnTrendDetected?.Invoke(trend);
            }
        }

        /// <summary>
        /// Analyze individual system trends
        /// </summary>
        private void AnalyzeSystemTrends()
        {
            foreach (var analytics in _systemAnalytics.Values)
            {
                if (analytics.CpuTimeHistory.Count < _trendSampleSize) continue;

                var recentCpuTimes = analytics.CpuTimeHistory.TakeLast(_trendSampleSize).ToList();
                var cpuTrend = CalculateTrend(recentCpuTimes);
                var cpuVariance = CalculateVariance(recentCpuTimes);

                // Detect significant CPU trend
                if (Mathf.Abs(cpuTrend) > _trendThreshold)
                {
                    var trend = new PerformanceTrend
                    {
                        SystemName = analytics.SystemName,
                        MetricName = "CpuTime",
                        TrendDirection = cpuTrend > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing,
                        Magnitude = Mathf.Abs(cpuTrend),
                        Confidence = CalculateConfidence(Mathf.Abs(cpuTrend), _trendThreshold),
                        DetectedAt = DateTime.Now,
                        Description = $"CPU usage trending {(cpuTrend > 0 ? "upward" : "downward")}"
                    };

                    _systemTrends[$"{analytics.SystemName}_CPU"] = trend;
                    OnTrendDetected?.Invoke(trend);
                }

                // Detect CPU instability
                if (cpuVariance > _varianceThreshold)
                {
                    var trend = new PerformanceTrend
                    {
                        SystemName = analytics.SystemName,
                        MetricName = "CpuStability",
                        TrendDirection = TrendDirection.Unstable,
                        Magnitude = cpuVariance,
                        Confidence = CalculateConfidence(cpuVariance, _varianceThreshold),
                        DetectedAt = DateTime.Now,
                        Description = "CPU performance instability detected"
                    };

                    _systemTrends[$"{analytics.SystemName}_Stability"] = trend;
                    OnTrendDetected?.Invoke(trend);
                }
            }
        }

        /// <summary>
        /// Detect system bottlenecks
        /// </summary>
        private void DetectBottlenecks()
        {
            if (_systemAnalytics.Count < 2) return;

            var totalCpuTime = _systemAnalytics.Values
                .Where(a => a.CpuTimeHistory.Count > 0)
                .Sum(a => a.CpuTimeHistory.LastOrDefault());

            foreach (var analytics in _systemAnalytics.Values)
            {
                if (analytics.CpuTimeHistory.Count == 0) continue;

                var systemCpuTime = analytics.CpuTimeHistory.LastOrDefault();
                var cpuPercentage = totalCpuTime > 0 ? systemCpuTime / totalCpuTime : 0f;

                if (cpuPercentage > _bottleneckThreshold)
                {
                    var bottleneck = new BottleneckAnalysis
                    {
                        SystemName = analytics.SystemName,
                        CpuPercentage = cpuPercentage,
                        Severity = CalculateBottleneckSeverity(cpuPercentage),
                        DetectedAt = DateTime.Now,
                        Recommendations = GenerateBottleneckRecommendations(analytics.SystemName, cpuPercentage)
                    };

                    OnBottleneckDetected?.Invoke(bottleneck);
                }
            }
        }

        /// <summary>
        /// Generate predictive insights
        /// </summary>
        private void GeneratePredictiveInsights()
        {
            // Predict memory exhaustion
            if (_frameDataHistory.Count >= _trendSampleSize)
            {
                var recentMemory = _frameDataHistory.TakeLast(_trendSampleSize)
                    .Select(f => (float)f.MemoryUsage).ToList();
                
                var memoryTrend = CalculateTrend(recentMemory);
                if (memoryTrend > 0.05f) // Growing by more than 5%
                {
                    var currentMemory = recentMemory.LastOrDefault();
                    var timeToExhaustion = EstimateTimeToMemoryExhaustion(recentMemory, memoryTrend);

                    if (timeToExhaustion < 300f) // Less than 5 minutes
                    {
                        var insight = new PredictiveInsight
                        {
                            Type = InsightType.MemoryExhaustion,
                            Severity = timeToExhaustion < 60f ? 0.9f : 0.6f,
                            TimeToOccurrence = timeToExhaustion,
                            Description = $"Memory exhaustion predicted in {timeToExhaustion:F0} seconds",
                            Recommendations = new List<string>
                            {
                                "Trigger garbage collection",
                                "Reduce memory allocation rate",
                                "Optimize data structures"
                            },
                            GeneratedAt = DateTime.Now
                        };

                        OnPredictiveInsight?.Invoke(insight);
                    }
                }
            }

            // Predict performance degradation
            PredictPerformanceDegradation();
        }

        #endregion

        #region Report Generation

        /// <summary>
        /// Generate comprehensive performance report
        /// </summary>
        public void GeneratePerformanceReport()
        {
            var recommendations = GenerateOptimizationRecommendations();
            var trendSummary = GenerateTrendSummary();

            var report = new PerformanceReport
            {
                ReportId = $"PERF_REPORT_{DateTime.Now:yyyyMMdd_HHmmss}",
                GeneratedAt = DateTime.Now,
                AnalysisPeriod = TimeSpan.FromSeconds(Time.time),
                TotalAnalysisRuns = _totalAnalysisRuns,
                OptimizationRecommendations = recommendations,
                TrendSummary = trendSummary,
                SystemAnalytics = new Dictionary<string, AnalyticsProfile>(_systemAnalytics),
                Summary = GenerateReportSummary()
            };

            AddReportToHistory(report);
            OnReportGenerated?.Invoke(report);

            ChimeraLogger.Log($"Generated performance report: {report.ReportId}", this);
        }

        /// <summary>
        /// Generate optimization recommendations
        /// </summary>
        private List<string> GenerateOptimizationRecommendations()
        {
            var recommendations = new List<string>();

            // Analyze trends for recommendations
            foreach (var trend in _systemTrends.Values)
            {
                if (trend.TrendDirection == TrendDirection.Increasing && trend.Magnitude > _trendThreshold)
                {
                    if (trend.MetricName == "CpuTime")
                    {
                        recommendations.Add($"Optimize {trend.SystemName} - CPU usage increasing by {trend.Magnitude:P1}");
                    }
                    else if (trend.MetricName == "MemoryUsage")
                    {
                        recommendations.Add($"Monitor {trend.SystemName} - Memory usage trending upward");
                    }
                }

                if (trend.TrendDirection == TrendDirection.Unstable)
                {
                    recommendations.Add($"Stabilize {trend.SystemName} - Performance variance detected");
                }
            }

            // System-specific recommendations
            foreach (var analytics in _systemAnalytics.Values)
            {
                if (analytics.CpuTimeHistory.Count > 0)
                {
                    var avgCpuTime = analytics.CpuTimeHistory.Average();
                    if (avgCpuTime > 5f) // Arbitrary threshold for demo
                    {
                        recommendations.Add($"Consider reducing update frequency for {analytics.SystemName}");
                    }
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Generate trend summary
        /// </summary>
        private string GenerateTrendSummary()
        {
            var increasingTrends = _systemTrends.Values.Count(t => t.TrendDirection == TrendDirection.Increasing);
            var decreasingTrends = _systemTrends.Values.Count(t => t.TrendDirection == TrendDirection.Decreasing);
            var unstableTrends = _systemTrends.Values.Count(t => t.TrendDirection == TrendDirection.Unstable);

            return $"Trends: {increasingTrends} increasing, {decreasingTrends} decreasing, {unstableTrends} unstable";
        }

        /// <summary>
        /// Generate report summary
        /// </summary>
        private string GenerateReportSummary()
        {
            var systemCount = _systemAnalytics.Count;
            var dataPoints = _frameDataHistory.Count;
            var trendsDetected = _systemTrends.Count;

            return $"Analytics: {systemCount} systems monitored, {dataPoints} data points, {trendsDetected} trends detected";
        }

        /// <summary>
        /// Generate final report on shutdown
        /// </summary>
        private void GenerateFinalReport()
        {
            if (!_enableAnalytics) return;

            GeneratePerformanceReport();
            
            ChimeraLogger.Log($"Final analytics summary: {_totalAnalysisRuns} analysis runs completed", this);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculate variance of a data series
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
        /// Calculate trend direction of a data series
        /// </summary>
        private float CalculateTrend(IEnumerable<float> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count < 2) return 0f;

            var firstHalf = valuesList.Take(valuesList.Count / 2).Average();
            var secondHalf = valuesList.Skip(valuesList.Count / 2).Average();
            
            return firstHalf > 0 ? (secondHalf - firstHalf) / firstHalf : 0f;
        }

        /// <summary>
        /// Calculate confidence level for trend detection
        /// </summary>
        private float CalculateConfidence(float magnitude, float threshold)
        {
            return Mathf.Clamp01(magnitude / (threshold * 2f));
        }

        /// <summary>
        /// Calculate bottleneck severity
        /// </summary>
        private float CalculateBottleneckSeverity(float cpuPercentage)
        {
            return Mathf.Clamp01((cpuPercentage - _bottleneckThreshold) / (1f - _bottleneckThreshold));
        }

        /// <summary>
        /// Generate bottleneck recommendations
        /// </summary>
        private List<string> GenerateBottleneckRecommendations(string systemName, float cpuPercentage)
        {
            var recommendations = new List<string>();
            
            if (cpuPercentage > 0.5f)
            {
                recommendations.Add($"Critical: {systemName} consuming {cpuPercentage:P1} of CPU time");
                recommendations.Add("Consider aggressive optimization or load balancing");
            }
            else if (cpuPercentage > 0.3f)
            {
                recommendations.Add($"High: {systemName} identified as bottleneck");
                recommendations.Add("Optimize update frequency or algorithm efficiency");
            }

            return recommendations;
        }

        /// <summary>
        /// Estimate time to memory exhaustion
        /// </summary>
        private float EstimateTimeToMemoryExhaustion(List<float> memoryHistory, float growthRate)
        {
            var currentMemory = memoryHistory.LastOrDefault();
            var maxMemory = SystemInfo.systemMemorySize * 1024f * 1024f * 0.8f; // 80% of system memory
            
            if (growthRate <= 0) return float.MaxValue;
            
            var remainingMemory = maxMemory - currentMemory;
            return remainingMemory / (currentMemory * growthRate / _analysisInterval);
        }

        /// <summary>
        /// Predict performance degradation
        /// </summary>
        private void PredictPerformanceDegradation()
        {
            if (_frameDataHistory.Count < _trendSampleSize * 2) return;

            var recentFrames = _frameDataHistory.TakeLast(_trendSampleSize).ToList();
            var previousFrames = _frameDataHistory.Skip(_frameDataHistory.Count - _trendSampleSize * 2)
                                                 .Take(_trendSampleSize).ToList();

            var recentAvgFrameTime = recentFrames.Average(f => f.FrameTime);
            var previousAvgFrameTime = previousFrames.Average(f => f.FrameTime);

            var degradationRate = (recentAvgFrameTime - previousAvgFrameTime) / previousAvgFrameTime;

            if (degradationRate > 0.1f) // More than 10% degradation
            {
                var insight = new PredictiveInsight
                {
                    Type = InsightType.PerformanceDegradation,
                    Severity = Mathf.Clamp01(degradationRate),
                    TimeToOccurrence = 0f, // Already occurring
                    Description = $"Performance degradation detected: {degradationRate:P1} increase in frame time",
                    Recommendations = new List<string>
                    {
                        "Review recent changes for performance impact",
                        "Check for memory leaks or resource buildup",
                        "Consider system optimization"
                    },
                    GeneratedAt = DateTime.Now
                };

                OnPredictiveInsight?.Invoke(insight);
            }
        }

        /// <summary>
        /// Add report to history with size management
        /// </summary>
        private void AddReportToHistory(PerformanceReport report)
        {
            _reportHistory.Add(report);

            if (_reportHistory.Count > _maxReportHistory)
            {
                _reportHistory.RemoveAt(0);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get latest performance trends
        /// </summary>
        public List<PerformanceTrend> GetLatestTrends(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now - timeSpan;
            return _systemTrends.Values.Where(t => t.DetectedAt >= cutoffTime).ToList();
        }

        /// <summary>
        /// Get system analytics for specific system
        /// </summary>
        public AnalyticsProfile GetSystemAnalytics(string systemName)
        {
            return _systemAnalytics.TryGetValue(systemName, out var analytics) ? analytics : null;
        }

        /// <summary>
        /// Clear analytics history
        /// </summary>
        public void ClearAnalyticsHistory()
        {
            _frameDataHistory.Clear();
            _systemTrends.Clear();
            _systemAnalytics.Clear();
            ChimeraLogger.Log("Analytics history cleared", this);
        }

        /// <summary>
        /// Set analytics configuration
        /// </summary>
        public void SetAnalyticsConfiguration(float analysisInterval, int minDataPoints, float varianceThreshold, float trendThreshold)
        {
            _analysisInterval = analysisInterval;
            _minDataPointsForAnalysis = minDataPoints;
            _varianceThreshold = varianceThreshold;
            _trendThreshold = trendThreshold;
            
            ChimeraLogger.Log("Analytics configuration updated", this);
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
    /// Performance trend data
    /// </summary>
    [System.Serializable]
    public class PerformanceTrend
    {
        public string SystemName;
        public string MetricName;
        public TrendDirection TrendDirection;
        public float Magnitude;
        public float Confidence;
        public DateTime DetectedAt;
        public string Description;
    }

    /// <summary>
    /// Analytics profile for individual systems
    /// </summary>
    [System.Serializable]
    public class AnalyticsProfile
    {
        public string SystemName;
        public List<float> CpuTimeHistory;
        public List<long> MemoryUsageHistory;
        public DateTime FirstDataPoint;
        public DateTime LastDataPoint;
    }

    /// <summary>
    /// Bottleneck analysis results
    /// </summary>
    [System.Serializable]
    public class BottleneckAnalysis
    {
        public string SystemName;
        public float CpuPercentage;
        public float Severity;
        public DateTime DetectedAt;
        public List<string> Recommendations;
    }

    /// <summary>
    /// Predictive insight data
    /// </summary>
    [System.Serializable]
    public class PredictiveInsight
    {
        public InsightType Type;
        public float Severity;
        public float TimeToOccurrence;
        public string Description;
        public List<string> Recommendations;
        public DateTime GeneratedAt;
    }

    /// <summary>
    /// Performance report structure
    /// </summary>
    [System.Serializable]
    public class PerformanceReport
    {
        public string ReportId;
        public DateTime GeneratedAt;
        public TimeSpan AnalysisPeriod;
        public int TotalAnalysisRuns;
        public List<string> OptimizationRecommendations;
        public string TrendSummary;
        public Dictionary<string, AnalyticsProfile> SystemAnalytics;
        public string Summary;
    }

    /// <summary>
    /// Trend direction enumeration
    /// </summary>
    public enum TrendDirection
    {
        Stable,
        Increasing,
        Decreasing,
        Unstable
    }

    /// <summary>
    /// Insight type enumeration
    /// </summary>
    public enum InsightType
    {
        MemoryExhaustion,
        PerformanceDegradation,
        SystemBottleneck,
        OptimizationOpportunity
    }

    #endregion
}