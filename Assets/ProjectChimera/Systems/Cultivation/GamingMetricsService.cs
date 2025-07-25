using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Service responsible for gaming performance metrics and analytics
    /// Tracks player engagement, performance indicators, and provides analytics
    /// </summary>
    public class GamingMetricsService
    {
        private readonly EnhancedCultivationGamingConfigSO _config;
        
        // State
        private bool _isInitialized = false;
        private float _sessionStartTime;
        private readonly Dictionary<string, object> _sessionMetrics = new Dictionary<string, object>();
        private readonly List<MetricSnapshot> _metricHistory = new List<MetricSnapshot>();
        
        // Performance tracking
        private int _careActionsPerformed = 0;
        private int _automationSystemsUnlocked = 0;
        private int _skillNodesProgressed = 0;
        private int _playerChoicesMade = 0;
        private float _totalEngagementTime = 0f;
        private float _lastActivityTime;
        
        // Analytics calculation
        private readonly Dictionary<string, float> _engagementMetrics = new Dictionary<string, float>();
        private readonly Queue<float> _recentActivityTimes = new Queue<float>();
        private const int MaxActivitySamples = 100;
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Gaming Metrics Service";
        
        public GamingMetricsService(EnhancedCultivationGamingConfigSO config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[GamingMetricsService] Already initialized");
                return;
            }
            
            try
            {
                InitializeMetrics();
                StartMetricsTracking();
                
                _isInitialized = true;
                Debug.Log("GamingMetricsService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize GamingMetricsService: {ex.Message}");
                throw;
            }
        }
        
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            SaveFinalMetrics();
            LogSessionSummary();
            
            _isInitialized = false;
            Debug.Log("GamingMetricsService shutdown completed");
        }
        
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;
            
            UpdateEngagementMetrics(deltaTime);
            UpdateActivityTracking();
            CheckMetricSnapshots();
        }
        
        private void InitializeMetrics()
        {
            _sessionStartTime = Time.time;
            _lastActivityTime = Time.time;
            
            // Initialize engagement metrics
            _engagementMetrics["ActionsPerMinute"] = 0f;
            _engagementMetrics["EngagementIntensity"] = 0f;
            _engagementMetrics["ProgressionRate"] = 0f;
            _engagementMetrics["DecisionFrequency"] = 0f;
        }
        
        private void StartMetricsTracking()
        {
            RecordMetric("SessionStartTime", _sessionStartTime);
            RecordMetric("InitialEngagementLevel", 0f);
        }
        
        private void UpdateEngagementMetrics(float deltaTime)
        {
            var sessionDuration = Time.time - _sessionStartTime;
            
            // Calculate actions per minute
            var actionsPerMinute = sessionDuration > 0 ? (_careActionsPerformed / (sessionDuration / 60f)) : 0;
            _engagementMetrics["ActionsPerMinute"] = actionsPerMinute;
            
            // Calculate engagement intensity
            var engagementIntensity = CalculateEngagementIntensity();
            _engagementMetrics["EngagementIntensity"] = engagementIntensity;
            
            // Calculate progression rate
            var progressionRate = CalculateProgressionRate(sessionDuration);
            _engagementMetrics["ProgressionRate"] = progressionRate;
            
            // Calculate decision frequency
            var decisionFrequency = sessionDuration > 0 ? (_playerChoicesMade / (sessionDuration / 60f)) : 0;
            _engagementMetrics["DecisionFrequency"] = decisionFrequency;
        }
        
        private void UpdateActivityTracking()
        {
            // Track recent activity for engagement calculation
            _recentActivityTimes.Enqueue(Time.time);
            
            while (_recentActivityTimes.Count > MaxActivitySamples)
            {
                _recentActivityTimes.Dequeue();
            }
        }
        
        private void CheckMetricSnapshots()
        {
            // Take periodic snapshots (every minute)
            var timeSinceLastSnapshot = _metricHistory.Count > 0 ? 
                Time.time - _metricHistory[_metricHistory.Count - 1].Timestamp : 60f;
            
            if (timeSinceLastSnapshot >= 60f)
            {
                TakeMetricSnapshot();
            }
        }
        
        private float CalculateEngagementIntensity()
        {
            var sessionDuration = Time.time - _sessionStartTime;
            if (sessionDuration <= 0) return 0f;
            
            // Combine multiple engagement factors
            var actionIntensity = Mathf.Clamp01(_careActionsPerformed / (sessionDuration / 10f)); // 1 action per 10 seconds baseline
            var progressionIntensity = Mathf.Clamp01((_skillNodesProgressed + _automationSystemsUnlocked) / 5f); // 5 progressions baseline
            var choiceIntensity = Mathf.Clamp01(_playerChoicesMade / 3f); // 3 choices baseline
            
            return (actionIntensity + progressionIntensity + choiceIntensity) / 3f;
        }
        
        private float CalculateProgressionRate(float sessionDuration)
        {
            if (sessionDuration <= 0) return 0f;
            
            var totalProgressions = _skillNodesProgressed + _automationSystemsUnlocked;
            return totalProgressions / (sessionDuration / 60f); // Progressions per minute
        }
        
        private void TakeMetricSnapshot()
        {
            var snapshot = new MetricSnapshot
            {
                Timestamp = Time.time,
                SessionDuration = Time.time - _sessionStartTime,
                Metrics = GetCurrentMetrics(),
                EngagementLevel = CalculateEngagementLevel()
            };
            
            _metricHistory.Add(snapshot);
        }
        
        private void SaveFinalMetrics()
        {
            TakeMetricSnapshot();
            RecordMetric("FinalEngagementLevel", CalculateEngagementLevel());
            RecordMetric("SessionEndTime", Time.time);
        }
        
        private void LogSessionSummary()
        {
            var metrics = GetSessionMetrics();
            Debug.Log($"Gaming Session Summary - Duration: {metrics.SessionDuration:F1}s, " +
                            $"Care Actions: {metrics.CareActionsPerformed}, " +
                            $"Automation Unlocks: {metrics.AutomationSystemsUnlocked}, " +
                            $"Skill Progressions: {metrics.SkillNodesProgressed}, " +
                            $"Player Choices: {metrics.PlayerChoicesMade}, " +
                            $"Engagement Level: {metrics.CurrentEngagementLevel:F2}");
        }
        
        // Metric recording
        public void RecordCareAction()
        {
            if (!_isInitialized) return;
            
            _careActionsPerformed++;
            _lastActivityTime = Time.time;
            RecordMetric("CareActionsPerformed", _careActionsPerformed);
        }
        
        public void RecordAutomationUnlock()
        {
            if (!_isInitialized) return;
            
            _automationSystemsUnlocked++;
            _lastActivityTime = Time.time;
            RecordMetric("AutomationSystemsUnlocked", _automationSystemsUnlocked);
        }
        
        public void RecordSkillProgression()
        {
            if (!_isInitialized) return;
            
            _skillNodesProgressed++;
            _lastActivityTime = Time.time;
            RecordMetric("SkillNodesProgressed", _skillNodesProgressed);
        }
        
        public void RecordPlayerChoice()
        {
            if (!_isInitialized) return;
            
            _playerChoicesMade++;
            _lastActivityTime = Time.time;
            RecordMetric("PlayerChoicesMade", _playerChoicesMade);
        }
        
        public void RecordMetric(string key, object value)
        {
            if (!_isInitialized || string.IsNullOrEmpty(key)) return;
            
            _sessionMetrics[key] = value;
        }
        
        // Public API
        public CultivationGamingMetrics GetSessionMetrics()
        {
            if (!_isInitialized) return new CultivationGamingMetrics();
            
            return new CultivationGamingMetrics
            {
                SessionDuration = Time.time - _sessionStartTime,
                CareActionsPerformed = _careActionsPerformed,
                AutomationSystemsUnlocked = _automationSystemsUnlocked,
                SkillNodesProgressed = _skillNodesProgressed,
                PlayerChoicesMade = _playerChoicesMade,
                CurrentEngagementLevel = CalculateEngagementLevel()
            };
        }
        
        public float CalculateEngagementLevel()
        {
            var sessionDuration = Time.time - _sessionStartTime;
            if (sessionDuration <= 0) return 0f;
            
            var actionsPerMinute = _careActionsPerformed / (sessionDuration / 60f);
            return Mathf.Clamp01(actionsPerMinute / 10f); // Normalize to 0-1 based on 10 actions per minute target
        }
        
        public Dictionary<string, object> GetCurrentMetrics()
        {
            var metrics = new Dictionary<string, object>(_sessionMetrics);
            
            // Add real-time calculated metrics
            metrics["CurrentTime"] = Time.time;
            metrics["SessionDuration"] = Time.time - _sessionStartTime;
            metrics["EngagementLevel"] = CalculateEngagementLevel();
            
            foreach (var engagementMetric in _engagementMetrics)
            {
                metrics[engagementMetric.Key] = engagementMetric.Value;
            }
            
            return metrics;
        }
        
        public T GetMetric<T>(string key, T defaultValue = default(T))
        {
            if (!_isInitialized || string.IsNullOrEmpty(key) || !_sessionMetrics.ContainsKey(key))
                return defaultValue;
            
            try
            {
                return (T)_sessionMetrics[key];
            }
            catch
            {
                return defaultValue;
            }
        }
        
        public List<MetricSnapshot> GetMetricHistory()
        {
            return new List<MetricSnapshot>(_metricHistory);
        }
        
        public MetricSnapshot GetLatestSnapshot()
        {
            return _metricHistory.Count > 0 ? _metricHistory[_metricHistory.Count - 1] : null;
        }
        
        public float GetTimeSinceLastActivity()
        {
            return Time.time - _lastActivityTime;
        }
        
        public Dictionary<string, float> GetEngagementMetrics()
        {
            return new Dictionary<string, float>(_engagementMetrics);
        }
        
        public void ClearMetrics()
        {
            _sessionMetrics.Clear();
            _metricHistory.Clear();
            _engagementMetrics.Clear();
            
            _careActionsPerformed = 0;
            _automationSystemsUnlocked = 0;
            _skillNodesProgressed = 0;
            _playerChoicesMade = 0;
            
            InitializeMetrics();
            Debug.Log("Gaming metrics cleared and reinitialized");
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class MetricSnapshot
    {
        public float Timestamp;
        public float SessionDuration;
        public Dictionary<string, object> Metrics;
        public float EngagementLevel;
    }
}