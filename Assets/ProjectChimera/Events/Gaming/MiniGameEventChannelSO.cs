using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Event channel for mini-game related events following Project Chimera event architecture.
    /// Provides type-safe event communication with logging, history tracking, and analytics integration.
    /// </summary>
    [CreateAssetMenu(fileName = "New MiniGame Event Channel", menuName = "Project Chimera/Events/Gaming/Mini-Game Event Channel")]
    public class MiniGameEventChannelSO : GameEventChannelSO<MiniGameEventData>
    {
        [Header("Mini-Game Event Configuration")]
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _enableEventHistory = true;
        [SerializeField] private int _maxHistoryEntries = 100;
        [SerializeField] private bool _enableAnalyticsIntegration = true;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableEventFiltering = false;
        [SerializeField] private List<string> _filteredGameIds = new List<string>();
        
        private Queue<MiniGameEventData> _eventHistory = new Queue<MiniGameEventData>();
        private Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
        
        public override void Raise(MiniGameEventData eventData)
        {
            // Validate event data
            if (eventData == null)
            {
                Debug.LogWarning("[MiniGameEventChannel] Attempted to raise null event data");
                return;
            }
            
            // Apply filtering if enabled
            if (_enableEventFiltering && _filteredGameIds.Contains(eventData.GameId))
            {
                return;
            }
            
            // Log event if enabled
            if (_logEvents)
            {
                Debug.Log($"[MiniGameEvent] {eventData.EventType}: {eventData.GameId} - Session: {eventData.SessionId}");
            }
            
            // Track event counts
            var eventKey = $"{eventData.GameId}_{eventData.EventType}";
            _eventCounts[eventKey] = _eventCounts.GetValueOrDefault(eventKey, 0) + 1;
            
            // Raise the event to all listeners
            base.Raise(eventData);
            
            // Add to history if enabled
            if (_enableEventHistory)
            {
                AddToHistory(eventData);
            }
            
            // Send to analytics if enabled
            if (_enableAnalyticsIntegration)
            {
                SendToAnalytics(eventData);
            }
        }
        
        private void AddToHistory(MiniGameEventData eventData)
        {
            _eventHistory.Enqueue(eventData);
            
            // Maintain history size limit
            while (_eventHistory.Count > _maxHistoryEntries)
            {
                _eventHistory.Dequeue();
            }
        }
        
        private void SendToAnalytics(MiniGameEventData eventData)
        {
            // Integration point for analytics system
            // This would be implemented to send data to the analytics manager
        }
        
        /// <summary>
        /// Get recent event history for debugging and analytics
        /// </summary>
        public List<MiniGameEventData> GetEventHistory()
        {
            return new List<MiniGameEventData>(_eventHistory);
        }
        
        /// <summary>
        /// Get event count statistics
        /// </summary>
        public Dictionary<string, int> GetEventCounts()
        {
            return new Dictionary<string, int>(_eventCounts);
        }
        
        /// <summary>
        /// Clear event history and reset counters
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
            _eventCounts.Clear();
        }
    }
    
    /// <summary>
    /// Event channel for mini-game progress updates with specialized handling
    /// </summary>
    [CreateAssetMenu(fileName = "New MiniGame Progress Event Channel", menuName = "Project Chimera/Events/Gaming/Mini-Game Progress Event Channel")]
    public class MiniGameProgressEventChannelSO : GameEventChannelSO<MiniGameProgressEventData>
    {
        [Header("Progress Event Configuration")]
        [SerializeField] private bool _trackProgressMetrics = true;
        [SerializeField] private bool _enableProgressAnalytics = true;
        [SerializeField] private float _progressUpdateThreshold = 5.0f; // Minimum progress change to trigger event
        
        private Dictionary<string, float> _lastProgressValues = new Dictionary<string, float>();
        private Dictionary<string, DateTime> _lastProgressTimes = new Dictionary<string, DateTime>();
        
        public override void Raise(MiniGameProgressEventData eventData)
        {
            // Apply progress threshold filtering to reduce event spam
            var sessionKey = eventData.SessionId;
            var currentTime = DateTime.Now;
            
            if (_lastProgressValues.TryGetValue(sessionKey, out var lastProgress))
            {
                var progressDelta = Mathf.Abs(eventData.Progress - lastProgress);
                if (progressDelta < _progressUpdateThreshold)
                {
                    return; // Skip event if progress change is too small
                }
            }
            
            // Update tracking data
            _lastProgressValues[sessionKey] = eventData.Progress;
            _lastProgressTimes[sessionKey] = currentTime;
            
            base.Raise(eventData);
            
            if (_trackProgressMetrics)
            {
                TrackProgressMetrics(eventData);
            }
        }
        
        private void TrackProgressMetrics(MiniGameProgressEventData eventData)
        {
            // Track progress rate, time to completion estimates, etc.
            // Implementation would integrate with analytics system
        }
        
        /// <summary>
        /// Get progress tracking data for a specific session
        /// </summary>
        public ProgressTrackingData GetProgressData(string sessionId)
        {
            return new ProgressTrackingData
            {
                SessionId = sessionId,
                LastProgress = _lastProgressValues.GetValueOrDefault(sessionId, 0f),
                LastUpdateTime = _lastProgressTimes.GetValueOrDefault(sessionId, DateTime.MinValue)
            };
        }
    }
    
    /// <summary>
    /// Event channel for mini-game achievement events with social features
    /// </summary>
    [CreateAssetMenu(fileName = "New MiniGame Achievement Event Channel", menuName = "Project Chimera/Events/Gaming/Mini-Game Achievement Event Channel")]
    public class MiniGameAchievementEventChannelSO : GameEventChannelSO<MiniGameAchievementEventData>
    {
        [Header("Achievement Event Configuration")]
        [SerializeField] private bool _enableAchievementTracking = true;
        [SerializeField] private bool _broadcastToSocialSystems = true;
        [SerializeField] private bool _enableRarityTracking = true;
        
        private Dictionary<string, AchievementRarity> _achievementRarities = new Dictionary<string, AchievementRarity>();
        private List<string> _recentAchievements = new List<string>();
        
        public override void Raise(MiniGameAchievementEventData eventData)
        {
            base.Raise(eventData);
            
            if (_enableAchievementTracking)
            {
                TrackAchievement(eventData);
            }
            
            if (_broadcastToSocialSystems)
            {
                BroadcastToSocialSystems(eventData);
            }
        }
        
        private void TrackAchievement(MiniGameAchievementEventData eventData)
        {
            _recentAchievements.Add(eventData.AchievementId);
            
            // Keep only recent achievements (last 50)
            if (_recentAchievements.Count > 50)
            {
                _recentAchievements.RemoveAt(0);
            }
            
            // Track rarity if enabled
            if (_enableRarityTracking)
            {
                UpdateAchievementRarity(eventData.AchievementId);
            }
        }
        
        private void UpdateAchievementRarity(string achievementId)
        {
            // Calculate rarity based on how often this achievement is earned
            var occurrences = _recentAchievements.Count(a => a == achievementId);
            var totalAchievements = _recentAchievements.Count;
            
            if (totalAchievements > 0)
            {
                var frequency = (float)occurrences / totalAchievements;
                
                var rarity = frequency switch
                {
                    > 0.5f => AchievementRarity.Common,
                    > 0.2f => AchievementRarity.Uncommon,
                    > 0.1f => AchievementRarity.Rare,
                    > 0.05f => AchievementRarity.Epic,
                    _ => AchievementRarity.Legendary
                };
                
                _achievementRarities[achievementId] = rarity;
            }
        }
        
        private void BroadcastToSocialSystems(MiniGameAchievementEventData eventData)
        {
            // Integration with existing achievement and social recognition systems
            // This would interface with the AchievementManager and social features
        }
        
        /// <summary>
        /// Get the rarity of a specific achievement
        /// </summary>
        public AchievementRarity GetAchievementRarity(string achievementId)
        {
            return _achievementRarities.GetValueOrDefault(achievementId, AchievementRarity.Common);
        }
    }
    
    /// <summary>
    /// Data structure for mini-game events
    /// </summary>
    [System.Serializable]
    public class MiniGameEventData
    {
        public string GameId;
        public string SessionId;
        public string PlayerId;
        public MiniGameEventType EventType;
        public DateTime Timestamp;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
        
        public MiniGameEventData()
        {
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Data structure for mini-game progress events
    /// </summary>
    [System.Serializable]
    public class MiniGameProgressEventData
    {
        public string SessionId;
        public string GameId;
        public float Progress; // 0-100
        public float TimeElapsed;
        public Dictionary<string, float> DetailedMetrics = new Dictionary<string, float>();
        public DateTime Timestamp;
        
        public MiniGameProgressEventData()
        {
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Data structure for mini-game achievement events
    /// </summary>
    [System.Serializable]
    public class MiniGameAchievementEventData
    {
        public string AchievementId;
        public string PlayerId;
        public string GameId;
        public string SessionId;
        public AchievementRarity Rarity;
        public float Score;
        public DateTime Timestamp;
        public Dictionary<string, object> Context = new Dictionary<string, object>();
        
        public MiniGameAchievementEventData()
        {
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Types of mini-game events
    /// </summary>
    public enum MiniGameEventType
    {
        GameStarted,
        GameCompleted,
        GameFailed,
        GamePaused,
        GameResumed,
        ProgressUpdate,
        AchievementUnlocked,
        DifficultyAdjusted,
        RewardEarned,
        SkillLevelUp,
        ErrorOccurred
    }
    
    /// <summary>
    /// Achievement rarity levels
    /// </summary>
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    
    /// <summary>
    /// Progress tracking data structure
    /// </summary>
    [System.Serializable]
    public class ProgressTrackingData
    {
        public string SessionId;
        public float LastProgress;
        public DateTime LastUpdateTime;
    }
}