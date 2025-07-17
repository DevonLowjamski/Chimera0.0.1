using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Analytics
{
    /// <summary>
    /// Analytics Manager for Project Chimera
    /// Provides data collection, analysis, and reporting capabilities
    /// </summary>
    public class AnalyticsManager : ChimeraManager
    {
        [Header("Analytics Configuration")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private float _collectionInterval = 60f;
        [SerializeField] private int _maxDataPoints = 10000;
        [SerializeField] private bool _enableRealtimeAnalysis = true;
        
        // Core analytics data
        private Dictionary<string, List<DataPoint>> _collectedData = new Dictionary<string, List<DataPoint>>();
        private Dictionary<string, AnalyticsMetric> _metrics = new Dictionary<string, AnalyticsMetric>();
        private List<AnalyticsEvent> _events = new List<AnalyticsEvent>();
        
        // State
        private float _lastCollectionTime;
        
        // Events
        public System.Action<AnalyticsEvent> OnEventRecorded;
        public System.Action<string, float> OnMetricUpdated;
        public System.Action<AnalyticsReport> OnReportGenerated;
        
        public string ManagerName => "Analytics";
        
        protected override void OnManagerInitialize()
        {
            _lastCollectionTime = Time.time;
            
            if (_enableAnalytics)
            {
                InvokeRepeating(nameof(CollectAnalytics), 1f, _collectionInterval);
                InitializeDefaultMetrics();
            }
            
            LogDebug("Analytics Manager initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            CancelInvoke();
            GenerateSessionReport();
            LogDebug("Analytics Manager shutdown");
        }
        
        // Public interface methods
        public void RecordEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventName = eventName,
                Timestamp = DateTime.Now,
                Parameters = parameters ?? new Dictionary<string, object>(),
                SessionId = GetCurrentSessionId()
            };
            
            _events.Add(analyticsEvent);
            
            // Limit event history
            if (_events.Count > _maxDataPoints)
            {
                _events.RemoveAt(0);
            }
            
            OnEventRecorded?.Invoke(analyticsEvent);
            Debug.Log($"Analytics event recorded: {eventName}");
        }
        
        public void UpdateMetric(string metricName, float value)
        {
            if (!_metrics.ContainsKey(metricName))
            {
                _metrics[metricName] = new AnalyticsMetric
                {
                    Name = metricName,
                    History = new List<DataPoint>()
                };
            }
            
            var metric = _metrics[metricName];
            metric.CurrentValue = value;
            metric.LastUpdated = DateTime.Now;
            
            var dataPoint = new DataPoint
            {
                Value = value,
                Timestamp = DateTime.Now
            };
            
            metric.History.Add(dataPoint);
            
            // Limit history size
            if (metric.History.Count > 1000)
            {
                metric.History.RemoveAt(0);
            }
            
            OnMetricUpdated?.Invoke(metricName, value);
        }
        
        public List<AnalyticsEvent> GetEvents(TimeSpan? timeRange = null)
        {
            if (timeRange == null)
            {
                return new List<AnalyticsEvent>(_events);
            }
            
            var cutoffTime = DateTime.Now - timeRange.Value;
            return _events.FindAll(e => e.Timestamp >= cutoffTime);
        }
        
        public AnalyticsMetric GetMetric(string metricName)
        {
            return _metrics.ContainsKey(metricName) ? _metrics[metricName] : null;
        }
        
        public List<AnalyticsMetric> GetAllMetrics()
        {
            return new List<AnalyticsMetric>(_metrics.Values);
        }
        
        public AnalyticsReport GenerateReport(TimeSpan period)
        {
            var endTime = DateTime.Now;
            var startTime = endTime - period;
            
            var periodEvents = _events.FindAll(e => e.Timestamp >= startTime && e.Timestamp <= endTime);
            
            var report = new AnalyticsReport
            {
                Period = period,
                GeneratedAt = DateTime.Now,
                TotalEvents = periodEvents.Count,
                UniqueEvents = periodEvents.GroupBy(e => e.EventName).Count(),
                TopEvents = GetTopEvents(periodEvents),
                MetricsSummary = GetMetricsSummary(startTime, endTime),
                PerformanceData = GetPerformanceData(startTime, endTime)
            };
            
            OnReportGenerated?.Invoke(report);
            return report;
        }
        
        public void ClearData()
        {
            _events.Clear();
            _collectedData.Clear();
            foreach (var metric in _metrics.Values)
            {
                metric.History.Clear();
            }
            
            Debug.Log("Analytics data cleared");
        }
        
        private void CollectAnalytics()
        {
            // Collect system performance data
            UpdateMetric("fps", 1f / Time.deltaTime);
            UpdateMetric("memory_usage", (float)(GC.GetTotalMemory(false) / 1024 / 1024)); // MB
            
            // Record periodic health check
            RecordEvent("system_health_check", new Dictionary<string, object>
            {
                {"fps", 1f / Time.deltaTime},
                {"memory_mb", (float)(GC.GetTotalMemory(false) / 1024 / 1024)},
                {"uptime_seconds", Time.time}
            });
        }
        
        private void InitializeDefaultMetrics()
        {
            // Initialize some default metrics
            UpdateMetric("fps", 60f);
            UpdateMetric("memory_usage", 0f);
            UpdateMetric("session_duration", 0f);
            UpdateMetric("user_actions", 0f);
        }
        
        private string GetCurrentSessionId()
        {
            // Simple session ID based on startup time
            return $"session_{Time.time:F0}";
        }
        
        private List<EventSummary> GetTopEvents(List<AnalyticsEvent> events)
        {
            var eventCounts = new Dictionary<string, int>();
            
            foreach (var evt in events)
            {
                if (eventCounts.ContainsKey(evt.EventName))
                {
                    eventCounts[evt.EventName]++;
                }
                else
                {
                    eventCounts[evt.EventName] = 1;
                }
            }
            
            var topEvents = new List<EventSummary>();
            foreach (var kvp in eventCounts)
            {
                topEvents.Add(new EventSummary
                {
                    EventName = kvp.Key,
                    Count = kvp.Value
                });
            }
            
            topEvents.Sort((a, b) => b.Count.CompareTo(a.Count));
            return topEvents.Take(10).ToList();
        }
        
        private Dictionary<string, float> GetMetricsSummary(DateTime startTime, DateTime endTime)
        {
            var summary = new Dictionary<string, float>();
            
            foreach (var metric in _metrics.Values)
            {
                var periodData = metric.History.FindAll(d => d.Timestamp >= startTime && d.Timestamp <= endTime);
                if (periodData.Count > 0)
                {
                    summary[$"{metric.Name}_avg"] = periodData.Average(d => d.Value);
                    summary[$"{metric.Name}_max"] = periodData.Max(d => d.Value);
                    summary[$"{metric.Name}_min"] = periodData.Min(d => d.Value);
                }
            }
            
            return summary;
        }
        
        private Dictionary<string, object> GetPerformanceData(DateTime startTime, DateTime endTime)
        {
            var performanceData = new Dictionary<string, object>();
            
            // Add performance metrics
            performanceData["avg_fps"] = GetAverageMetricValue("fps", startTime, endTime);
            performanceData["avg_memory_usage"] = GetAverageMetricValue("memory_usage", startTime, endTime);
            
            return performanceData;
        }
        
        private float GetAverageMetricValue(string metricName, DateTime startTime, DateTime endTime)
        {
            if (!_metrics.ContainsKey(metricName))
                return 0f;
                
            var metric = _metrics[metricName];
            var relevantData = metric.History.FindAll(dp => dp.Timestamp >= startTime && dp.Timestamp <= endTime);
            
            if (relevantData.Count == 0)
                return 0f;
                
            return relevantData.Average(dp => dp.Value);
        }
        
        private void GenerateSessionReport()
        {
            var sessionReport = GenerateReport(TimeSpan.FromHours(1)); // Last hour
            Debug.Log($"Session report generated: {sessionReport.TotalEvents} events");
        }
    }
    
    [System.Serializable]
    public class AnalyticsEvent
    {
        public string EventName;
        public DateTime Timestamp;
        public Dictionary<string, object> Parameters;
        public string SessionId;
    }
    
    [System.Serializable]
    public class AnalyticsMetric
    {
        public string Name;
        public float CurrentValue;
        public DateTime LastUpdated;
        public List<DataPoint> History;
    }
    
    [System.Serializable]
    public class DataPoint
    {
        public float Value;
        public DateTime Timestamp;
    }
    
    [System.Serializable]
    public class AnalyticsReport
    {
        public TimeSpan Period;
        public DateTime GeneratedAt;
        public int TotalEvents;
        public int UniqueEvents;
        public List<EventSummary> TopEvents;
        public Dictionary<string, float> MetricsSummary;
        public Dictionary<string, object> PerformanceData;
    }
    
    [System.Serializable]
    public class EventSummary
    {
        public string EventName;
        public int Count;
    }
} 