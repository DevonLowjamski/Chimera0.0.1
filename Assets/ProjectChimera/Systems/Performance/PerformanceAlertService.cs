using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Performance Alert Service - Dedicated service for alert generation and threshold monitoring
    /// Extracted from UnifiedPerformanceManagementSystem to provide focused alert functionality
    /// Handles threshold monitoring, alert generation, and alert history management
    /// </summary>
    public class PerformanceAlertService : MonoBehaviour
    {
        [Header("Alert Configuration")]
        [SerializeField] private bool _enableAlerts = true;
        [SerializeField] private float _alertCheckInterval = 1f;
        [SerializeField] private int _maxAlertHistory = 100;
        [SerializeField] private bool _enableAlertSupression = true;

        [Header("Performance Thresholds")]
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _criticalFrameRateThreshold = 20f;
        [SerializeField] private float _memoryWarningThresholdMB = 1024f;
        [SerializeField] private float _memoryCriticalThresholdMB = 2048f;
        [SerializeField] private float _cpuWarningThresholdMs = 16.67f; // 60 FPS target
        [SerializeField] private float _cpuCriticalThresholdMs = 33.33f; // 30 FPS critical

        [Header("Alert Suppression")]
        [SerializeField] private float _alertSuppressionTime = 30f; // Seconds between identical alerts
        [SerializeField] private float _criticalAlertSuppressionTime = 10f; // Shorter for critical alerts

        // Alert management
        private List<PerformanceAlert> _alertHistory = new List<PerformanceAlert>();
        private Dictionary<string, float> _lastAlertTimes = new Dictionary<string, float>();
        private Dictionary<string, PerformanceAlertType> _lastAlertTypes = new Dictionary<string, PerformanceAlertType>();

        // Alert state
        private bool _isInitialized = false;
        private float _lastAlertCheck = 0f;
        private int _totalAlertsGenerated = 0;

        // Events for alert notifications
        public static event System.Action<PerformanceAlert> OnPerformanceAlert;
        public static event System.Action<AlertSummary> OnAlertSummaryGenerated;
        public static event System.Action<string, PerformanceAlertType> OnAlertSuppressed;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Performance Alert Service";
        public IReadOnlyList<PerformanceAlert> AlertHistory => _alertHistory;
        public int TotalAlertsGenerated => _totalAlertsGenerated;
        public bool EnableAlerts => _enableAlerts;

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
            _alertHistory = new List<PerformanceAlert>();
            _lastAlertTimes = new Dictionary<string, float>();
            _lastAlertTypes = new Dictionary<string, PerformanceAlertType>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PerformanceAlertService already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                
                _isInitialized = true;
                ChimeraLogger.Log("PerformanceAlertService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PerformanceAlertService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            GenerateAlertSummary();
            _alertHistory.Clear();
            _lastAlertTimes.Clear();
            _lastAlertTypes.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("PerformanceAlertService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_alertCheckInterval <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid alert check interval, using default 1s", this);
                _alertCheckInterval = 1f;
            }

            if (_targetFrameRate <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid target frame rate, using default 60 FPS", this);
                _targetFrameRate = 60f;
            }

            if (_criticalFrameRateThreshold >= _targetFrameRate)
            {
                ChimeraLogger.LogWarning("Critical frame rate threshold too high, adjusting", this);
                _criticalFrameRateThreshold = _targetFrameRate * 0.3f;
            }
        }

        #endregion

        #region Threshold Monitoring

        private void Update()
        {
            if (!_isInitialized || !_enableAlerts) return;

            // Check thresholds at specified interval
            if (Time.time - _lastAlertCheck >= _alertCheckInterval)
            {
                // Note: In a real implementation, we would get metrics from PerformanceMonitoringService
                // For now, we'll create a placeholder method
                CheckAllThresholds();
                _lastAlertCheck = Time.time;
            }
        }

        /// <summary>
        /// Check all performance thresholds and trigger alerts as needed
        /// </summary>
        public void CheckPerformanceThresholds(GlobalPerformanceMetrics globalMetrics)
        {
            if (!_enableAlerts) return;

            CheckFrameRateThresholds(globalMetrics);
            CheckMemoryThresholds(globalMetrics);
            CheckCpuThresholds(globalMetrics);
        }

        /// <summary>
        /// Check frame rate thresholds
        /// </summary>
        private void CheckFrameRateThresholds(GlobalPerformanceMetrics globalMetrics)
        {
            if (globalMetrics.CurrentFrameRate < _criticalFrameRateThreshold)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "Global", 
                    $"Critical frame rate: {globalMetrics.CurrentFrameRate:F1} FPS", 0.9f,
                    "Consider reducing quality settings or optimizing systems");
            }
            else if (globalMetrics.CurrentFrameRate < _targetFrameRate * 0.8f)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Warning, "Global",
                    $"Low frame rate: {globalMetrics.CurrentFrameRate:F1} FPS", 0.6f,
                    "Monitor system performance and consider optimizations");
            }
        }

        /// <summary>
        /// Check memory usage thresholds
        /// </summary>
        private void CheckMemoryThresholds(GlobalPerformanceMetrics globalMetrics)
        {
            var memoryUsageMB = globalMetrics.TotalMemoryUsage / (1024f * 1024f);
            
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
        }

        /// <summary>
        /// Check CPU frame time thresholds
        /// </summary>
        private void CheckCpuThresholds(GlobalPerformanceMetrics globalMetrics)
        {
            if (globalMetrics.CpuFrameTime > _cpuCriticalThresholdMs)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "CPU",
                    $"Critical CPU frame time: {globalMetrics.CpuFrameTime:F1} ms", 0.9f,
                    "Optimize CPU-intensive operations or reduce update frequency");
            }
            else if (globalMetrics.CpuFrameTime > _cpuWarningThresholdMs)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Warning, "CPU",
                    $"High CPU frame time: {globalMetrics.CpuFrameTime:F1} ms", 0.6f,
                    "Monitor CPU usage and consider optimizations");
            }
        }

        /// <summary>
        /// Placeholder method for checking thresholds when metrics aren't available
        /// </summary>
        private void CheckAllThresholds()
        {
            // In a real implementation, this would get current metrics from PerformanceMonitoringService
            // For now, we'll check basic Unity metrics
            var currentFPS = 1f / Time.unscaledDeltaTime;
            var frameTime = Time.unscaledDeltaTime * 1000f;

            if (currentFPS < _criticalFrameRateThreshold)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "Global",
                    $"Critical frame rate detected: {currentFPS:F1} FPS", 0.9f,
                    "System performance is critically low");
            }

            if (frameTime > _cpuCriticalThresholdMs)
            {
                TriggerPerformanceAlert(PerformanceAlertType.Critical, "CPU",
                    $"Critical frame time: {frameTime:F1} ms", 0.9f,
                    "CPU performance is critically impacted");
            }
        }

        #endregion

        #region Alert Generation

        /// <summary>
        /// Trigger a performance alert with suppression logic
        /// </summary>
        public void TriggerPerformanceAlert(PerformanceAlertType type, string systemName, string message, float severity, string suggestedAction = "")
        {
            // Check for alert suppression
            if (_enableAlertSupression && ShouldSuppressAlert(type, systemName))
            {
                OnAlertSuppressed?.Invoke(systemName, type);
                return;
            }

            var alert = new PerformanceAlert
            {
                Type = type,
                SystemName = systemName,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.Now,
                SuggestedAction = suggestedAction,
                AlertId = GenerateAlertId()
            };

            // Add to history
            AddAlertToHistory(alert);

            // Update suppression tracking
            UpdateSuppressionTracking(systemName, type);

            // Trigger events
            OnPerformanceAlert?.Invoke(alert);

            // Log alerts
            LogAlert(alert);

            _totalAlertsGenerated++;
        }

        /// <summary>
        /// Trigger a custom alert for specific system conditions
        /// </summary>
        public void TriggerCustomAlert(string systemName, string condition, float value, float threshold, string unit = "")
        {
            var severity = CalculateSeverity(value, threshold);
            var alertType = severity > 0.8f ? PerformanceAlertType.Critical : 
                           severity > 0.5f ? PerformanceAlertType.Warning : PerformanceAlertType.Info;

            var message = $"{condition}: {value:F1}{unit} (threshold: {threshold:F1}{unit})";
            var suggestion = GenerateSuggestionForCondition(condition, severity);

            TriggerPerformanceAlert(alertType, systemName, message, severity, suggestion);
        }

        /// <summary>
        /// Check trend-based alerts
        /// </summary>
        public void CheckTrendAlerts(IEnumerable<FramePerformanceData> frameHistory)
        {
            if (frameHistory.Count() < 60) return; // Need enough data for trend analysis

            var recentFrames = frameHistory.TakeLast(60).ToList();
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
        }

        #endregion

        #region Alert Management

        /// <summary>
        /// Add alert to history with size management
        /// </summary>
        private void AddAlertToHistory(PerformanceAlert alert)
        {
            _alertHistory.Add(alert);

            // Maintain history size
            if (_alertHistory.Count > _maxAlertHistory)
            {
                _alertHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Check if alert should be suppressed
        /// </summary>
        private bool ShouldSuppressAlert(PerformanceAlertType type, string systemName)
        {
            var alertKey = $"{systemName}:{type}";
            
            if (!_lastAlertTimes.TryGetValue(alertKey, out var lastTime))
                return false;

            var suppressionTime = type == PerformanceAlertType.Critical ? 
                _criticalAlertSuppressionTime : _alertSuppressionTime;

            return (Time.time - lastTime) < suppressionTime;
        }

        /// <summary>
        /// Update suppression tracking
        /// </summary>
        private void UpdateSuppressionTracking(string systemName, PerformanceAlertType type)
        {
            var alertKey = $"{systemName}:{type}";
            _lastAlertTimes[alertKey] = Time.time;
            _lastAlertTypes[systemName] = type;
        }

        /// <summary>
        /// Generate unique alert ID
        /// </summary>
        private string GenerateAlertId()
        {
            return $"ALERT_{DateTime.Now:yyyyMMdd_HHmmss}_{_totalAlertsGenerated:000}";
        }

        /// <summary>
        /// Log alert based on type
        /// </summary>
        private void LogAlert(PerformanceAlert alert)
        {
            var logMessage = $"[{alert.SystemName}] {alert.Message}";

            switch (alert.Type)
            {
                case PerformanceAlertType.Critical:
                    ChimeraLogger.LogError(logMessage, this);
                    break;
                case PerformanceAlertType.Warning:
                    ChimeraLogger.LogWarning(logMessage, this);
                    break;
                case PerformanceAlertType.Info:
                case PerformanceAlertType.Optimization:
                    ChimeraLogger.Log(logMessage, this);
                    break;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculate severity based on value and threshold
        /// </summary>
        private float CalculateSeverity(float value, float threshold)
        {
            if (value <= threshold) return 0f;
            return Mathf.Clamp01((value - threshold) / threshold);
        }

        /// <summary>
        /// Generate suggestion based on condition and severity
        /// </summary>
        private string GenerateSuggestionForCondition(string condition, float severity)
        {
            if (severity > 0.8f)
                return $"Immediate action required for {condition}";
            else if (severity > 0.5f)
                return $"Monitor {condition} closely";
            else
                return $"Consider optimizing {condition}";
        }

        /// <summary>
        /// Calculate variance for trend analysis
        /// </summary>
        private float CalculateVariance(IEnumerable<float> values)
        {
            var valueList = values.ToList();
            if (valueList.Count == 0) return 0f;

            var mean = valueList.Average();
            var variance = valueList.Average(v => (v - mean) * (v - mean));
            return variance;
        }

        /// <summary>
        /// Calculate trend (positive = increasing)
        /// </summary>
        private float CalculateTrend(IEnumerable<float> values)
        {
            var valueList = values.ToList();
            if (valueList.Count < 2) return 0f;

            var firstHalf = valueList.Take(valueList.Count / 2).Average();
            var secondHalf = valueList.Skip(valueList.Count / 2).Average();
            
            return (secondHalf - firstHalf) / firstHalf;
        }

        /// <summary>
        /// Generate alert summary
        /// </summary>
        private void GenerateAlertSummary()
        {
            var summary = new AlertSummary
            {
                TotalAlerts = _totalAlertsGenerated,
                CriticalAlerts = _alertHistory.Count(a => a.Type == PerformanceAlertType.Critical),
                WarningAlerts = _alertHistory.Count(a => a.Type == PerformanceAlertType.Warning),
                InfoAlerts = _alertHistory.Count(a => a.Type == PerformanceAlertType.Info),
                MostFrequentSystem = GetMostFrequentAlertSystem(),
                SessionDuration = Time.time,
                GeneratedAt = DateTime.Now
            };

            OnAlertSummaryGenerated?.Invoke(summary);
            
            ChimeraLogger.Log($"Alert session summary: {summary.TotalAlerts} total alerts " +
                            $"({summary.CriticalAlerts} critical, {summary.WarningAlerts} warning, {summary.InfoAlerts} info)", this);
        }

        /// <summary>
        /// Get system with most frequent alerts
        /// </summary>
        private string GetMostFrequentAlertSystem()
        {
            if (_alertHistory.Count == 0) return "None";

            return _alertHistory
                .GroupBy(a => a.SystemName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "None";
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get alerts for a specific system
        /// </summary>
        public List<PerformanceAlert> GetSystemAlerts(string systemName)
        {
            return _alertHistory.Where(a => a.SystemName == systemName).ToList();
        }

        /// <summary>
        /// Get alerts by type
        /// </summary>
        public List<PerformanceAlert> GetAlertsByType(PerformanceAlertType type)
        {
            return _alertHistory.Where(a => a.Type == type).ToList();
        }

        /// <summary>
        /// Get recent alerts within time period
        /// </summary>
        public List<PerformanceAlert> GetRecentAlerts(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now - timeSpan;
            return _alertHistory.Where(a => a.Timestamp >= cutoffTime).ToList();
        }

        /// <summary>
        /// Clear alert history
        /// </summary>
        public void ClearAlertHistory()
        {
            _alertHistory.Clear();
            ChimeraLogger.Log("Alert history cleared", this);
        }

        /// <summary>
        /// Set alert suppression enabled/disabled
        /// </summary>
        public void SetAlertSuppression(bool enabled)
        {
            _enableAlertSupression = enabled;
            ChimeraLogger.Log($"Alert suppression {(enabled ? "enabled" : "disabled")}", this);
        }

        /// <summary>
        /// Update threshold values
        /// </summary>
        public void UpdateThresholds(float targetFPS, float memoryWarningMB, float memoryCriticalMB, float cpuWarningMs, float cpuCriticalMs)
        {
            _targetFrameRate = targetFPS;
            _memoryWarningThresholdMB = memoryWarningMB;
            _memoryCriticalThresholdMB = memoryCriticalMB;
            _cpuWarningThresholdMs = cpuWarningMs;
            _cpuCriticalThresholdMs = cpuCriticalMs;
            
            ChimeraLogger.Log("Performance thresholds updated", this);
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
    /// Performance alert data structure
    /// </summary>
    [System.Serializable]
    public class PerformanceAlert
    {
        public PerformanceAlertType Type;
        public string SystemName;
        public string Message;
        public float Severity; // 0-1
        public DateTime Timestamp;
        public string SuggestedAction;
        public string AlertId;
    }

    /// <summary>
    /// Alert summary for reporting
    /// </summary>
    [System.Serializable]
    public class AlertSummary
    {
        public int TotalAlerts;
        public int CriticalAlerts;
        public int WarningAlerts;
        public int InfoAlerts;
        public string MostFrequentSystem;
        public float SessionDuration;
        public DateTime GeneratedAt;
    }

    /// <summary>
    /// Performance alert types
    /// </summary>
    public enum PerformanceAlertType
    {
        Info,
        Warning,
        Critical,
        Optimization
    }

    #endregion
}