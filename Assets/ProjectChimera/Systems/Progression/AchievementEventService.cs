using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;
using ProgressionAchievementProgress = ProjectChimera.Data.Progression.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement Event Service - Manages game event integration, progress updates, and real-time tracking
    /// Extracted from AchievementSystemManager to provide focused event management functionality
    /// Handles game event listening, achievement trigger detection, progress monitoring, and real-time updates
    /// Provides comprehensive event-driven achievement progression and real-time analytics
    /// </summary>
    public class AchievementEventService : MonoBehaviour
    {
        [Header("Event Configuration")]
        [SerializeField] private bool _enableEventProcessing = true;
        [SerializeField] private bool _enableRealTimeTracking = true;
        [SerializeField] private bool _enableEventLogging = false;
        [SerializeField] private float _eventProcessingInterval = 0.1f;

        [Header("Progress Monitoring")]
        [SerializeField] private bool _enableProgressMonitoring = true;
        [SerializeField] private bool _enableProgressPrediction = true;
        [SerializeField] private float _progressUpdateThreshold = 0.01f;
        [SerializeField] private int _maxEventQueueSize = 1000;

        [Header("Performance Settings")]
        [SerializeField] private bool _enableEventBatching = true;
        [SerializeField] private int _maxEventsPerBatch = 50;
        [SerializeField] private float _batchProcessingInterval = 0.2f;
        [SerializeField] private bool _enableEventFiltering = true;

        [Header("Analytics Settings")]
        [SerializeField] private bool _enableEventAnalytics = true;
        [SerializeField] private bool _enablePlayerBehaviorTracking = true;
        [SerializeField] private bool _enableAchievementTrends = true;
        [SerializeField] private int _analyticsHistorySize = 1000;

        // Service state
        private bool _isInitialized = false;
        private float _lastProcessTime = 0f;
        private float _lastBatchTime = 0f;

        // Event processing
        private Queue<GameEvent> _eventQueue = new Queue<GameEvent>();
        private List<GameEvent> _eventBatch = new List<GameEvent>();
        private Dictionary<string, List<AchievementEventListener>> _eventListeners = new Dictionary<string, List<AchievementEventListener>>();
        private HashSet<string> _activeEventTypes = new HashSet<string>();

        // Progress tracking
        private Dictionary<string, Dictionary<string, float>> _playerProgressCache = new Dictionary<string, Dictionary<string, float>>();
        private Dictionary<string, DateTime> _lastProgressUpdate = new Dictionary<string, DateTime>();
        private Dictionary<string, AchievementEventMetrics> _eventMetrics = new Dictionary<string, AchievementEventMetrics>();

        // Analytics data
        private Queue<EventAnalyticsData> _analyticsHistory = new Queue<EventAnalyticsData>();
        private Dictionary<string, PlayerBehaviorData> _playerBehaviorData = new Dictionary<string, PlayerBehaviorData>();
        private AchievementTrendData _trendData = new AchievementTrendData();

        // Event filtering
        private HashSet<string> _filteredEventTypes = new HashSet<string>();
        private Dictionary<string, float> _eventCooldowns = new Dictionary<string, float>();
        private Dictionary<string, int> _eventRateLimits = new Dictionary<string, int>();

        // Events for external integration
        public static event Action<GameEvent> OnEventProcessed;
        public static event Action<string, string, float> OnProgressUpdated;
        public static event Action<string, Achievement> OnAchievementTriggered;
        public static event Action<EventAnalyticsData> OnAnalyticsUpdate;
        public static event Action<string, float> OnPlayerBehaviorChanged;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Event Service";
        public int QueuedEvents => _eventQueue.Count;
        public int ActiveListeners => _eventListeners.Values.Sum(list => list.Count);
        public int TrackedPlayers => _playerProgressCache.Count;

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

        private void Update()
        {
            if (_isInitialized && _enableEventProcessing)
            {
                ProcessEventQueue();
                ProcessEventBatches();
                UpdateProgressMonitoring();
                UpdateAnalytics();
            }
        }

        private void InitializeDataStructures()
        {
            _eventQueue = new Queue<GameEvent>();
            _eventBatch = new List<GameEvent>();
            _eventListeners = new Dictionary<string, List<AchievementEventListener>>();
            _activeEventTypes = new HashSet<string>();
            _playerProgressCache = new Dictionary<string, Dictionary<string, float>>();
            _lastProgressUpdate = new Dictionary<string, DateTime>();
            _eventMetrics = new Dictionary<string, AchievementEventMetrics>();
            _analyticsHistory = new Queue<EventAnalyticsData>();
            _playerBehaviorData = new Dictionary<string, PlayerBehaviorData>();
            _trendData = new AchievementTrendData();
            _filteredEventTypes = new HashSet<string>();
            _eventCooldowns = new Dictionary<string, float>();
            _eventRateLimits = new Dictionary<string, int>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementEventService already initialized", this);
                return;
            }

            try
            {
                InitializeEventListeners();
                InitializeEventFilters();
                InitializeAnalytics();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementEventService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementEventService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                ProcessRemainingEvents();
                SaveAnalyticsData();
                ClearAllData();
                
                _isInitialized = false;
                ChimeraLogger.Log("AchievementEventService shutdown completed", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error during AchievementEventService shutdown: {ex.Message}", this);
            }
        }

        #endregion

        #region Event System Initialization

        private void InitializeEventListeners()
        {
            // Register listeners for all major game events that can trigger achievements
            var eventTypes = new string[]
            {
                // Cultivation Events
                "plant_planted", "plant_harvested", "plant_watered", "plant_fertilized",
                "seed_germinated", "plant_died", "harvest_quality_achieved", "plant_care_action",
                
                // Breeding & Genetics Events
                "breeding_started", "breeding_completed", "genetic_trait_discovered",
                "perfect_genetics_achieved", "breeding_experiment_completed", "strain_created",
                
                // Research Events
                "research_started", "research_completed", "technology_unlocked",
                "skill_point_earned", "level_up", "expertise_gained",
                
                // Business Events
                "product_sold", "contract_completed", "market_transaction",
                "profit_milestone", "business_expansion", "facility_upgraded",
                
                // Social Events
                "player_helped", "knowledge_shared", "community_event_joined",
                "mentorship_provided", "collaboration_completed", "social_recognition",
                
                // System Events
                "achievement_unlocked", "milestone_reached", "streak_maintained",
                "daily_login", "session_time", "perfect_completion",
                
                // Special Events
                "secret_discovered", "easter_egg_found", "hidden_area_accessed",
                "perfect_storm_conditions", "rare_event_triggered", "legendary_action"
            };

            foreach (string eventType in eventTypes)
            {
                RegisterEventListener(eventType);
                _activeEventTypes.Add(eventType);
            }

            ChimeraLogger.Log($"Event listeners initialized: {eventTypes.Length} event types registered", this);
        }

        private void RegisterEventListener(string eventType)
        {
            if (!_eventListeners.ContainsKey(eventType))
            {
                _eventListeners[eventType] = new List<AchievementEventListener>();
            }

            var listener = new AchievementEventListener
            {
                EventType = eventType,
                IsActive = true,
                LastTriggered = DateTime.MinValue,
                TriggerCount = 0
            };

            _eventListeners[eventType].Add(listener);
        }

        private void InitializeEventFilters()
        {
            // Set up event filtering to prevent spam and improve performance
            _filteredEventTypes.Add("mouse_move");
            _filteredEventTypes.Add("camera_update");
            _filteredEventTypes.Add("ui_hover");
            
            // Set up rate limits for high-frequency events
            _eventRateLimits["plant_care_action"] = 10; // Max 10 per second
            _eventRateLimits["ui_interaction"] = 20;
            _eventRateLimits["progress_update"] = 5;

            ChimeraLogger.Log("Event filters initialized", this);
        }

        private void InitializeAnalytics()
        {
            if (!_enableEventAnalytics) return;

            _trendData = new AchievementTrendData
            {
                InitializationTime = DateTime.Now,
                DailyEventCounts = new Dictionary<string, Dictionary<DateTime, int>>(),
                PlayerEngagementMetrics = new Dictionary<string, float>(),
                AchievementCompletionRates = new Dictionary<string, float>()
            };

            ChimeraLogger.Log("Event analytics initialized", this);
        }

        #endregion

        #region Event Processing

        public void ProcessGameEvent(string eventType, string playerId = "current_player", float value = 1f, Dictionary<string, object> eventData = null)
        {
            if (!_isInitialized || !_enableEventProcessing)
            {
                return;
            }

            // Apply event filtering
            if (_enableEventFiltering && ShouldFilterEvent(eventType, playerId))
            {
                return;
            }

            var gameEvent = new GameEvent
            {
                EventType = eventType,
                PlayerId = playerId,
                Value = value,
                EventData = eventData ?? new Dictionary<string, object>(),
                Timestamp = DateTime.Now,
                EventId = Guid.NewGuid().ToString()
            };

            // Queue event for processing
            if (_eventQueue.Count < _maxEventQueueSize)
            {
                _eventQueue.Enqueue(gameEvent);
                
                if (_enableEventLogging)
                {
                    ChimeraLogger.Log($"Event queued: {eventType} by {playerId} with value {value}", this);
                }
            }
            else
            {
                ChimeraLogger.LogWarning($"Event queue full, dropping event: {eventType}", this);
            }

            // Update real-time analytics
            if (_enableEventAnalytics)
            {
                UpdateEventAnalytics(gameEvent);
            }
        }

        private void ProcessEventQueue()
        {
            if (Time.time - _lastProcessTime < _eventProcessingInterval)
            {
                return;
            }

            int processedCount = 0;
            int maxToProcess = _enableEventBatching ? _maxEventsPerBatch : _eventQueue.Count;

            while (_eventQueue.Count > 0 && processedCount < maxToProcess)
            {
                var gameEvent = _eventQueue.Dequeue();
                
                if (_enableEventBatching)
                {
                    _eventBatch.Add(gameEvent);
                }
                else
                {
                    ProcessSingleEvent(gameEvent);
                }
                
                processedCount++;
            }

            _lastProcessTime = Time.time;
        }

        private void ProcessEventBatches()
        {
            if (!_enableEventBatching || Time.time - _lastBatchTime < _batchProcessingInterval)
            {
                return;
            }

            if (_eventBatch.Count > 0)
            {
                ProcessEventBatch(_eventBatch);
                _eventBatch.Clear();
            }

            _lastBatchTime = Time.time;
        }

        private void ProcessEventBatch(List<GameEvent> events)
        {
            // Group events by player for efficient processing
            var eventsByPlayer = events.GroupBy(e => e.PlayerId);

            foreach (var playerGroup in eventsByPlayer)
            {
                string playerId = playerGroup.Key;
                var playerEvents = playerGroup.ToList();

                ProcessPlayerEventBatch(playerId, playerEvents);
            }
        }

        private void ProcessPlayerEventBatch(string playerId, List<GameEvent> events)
        {
            // Initialize player progress cache if needed
            if (!_playerProgressCache.ContainsKey(playerId))
            {
                _playerProgressCache[playerId] = new Dictionary<string, float>();
            }

            foreach (var gameEvent in events)
            {
                ProcessSingleEvent(gameEvent);
            }

            // Update player behavior tracking
            if (_enablePlayerBehaviorTracking)
            {
                UpdatePlayerBehavior(playerId, events);
            }
        }

        private void ProcessSingleEvent(GameEvent gameEvent)
        {
            try
            {
                // Check if this event type has registered listeners
                if (_eventListeners.TryGetValue(gameEvent.EventType, out var listeners))
                {
                    foreach (var listener in listeners.Where(l => l.IsActive))
                    {
                        ProcessEventListener(gameEvent, listener);
                    }
                }

                // Update progress tracking
                if (_enableProgressMonitoring)
                {
                    UpdateProgressTracking(gameEvent);
                }

                // Fire external event
                OnEventProcessed?.Invoke(gameEvent);

                // Update event metrics
                UpdateEventMetrics(gameEvent);
                
                if (_enableEventLogging)
                {
                    ChimeraLogger.Log($"Event processed: {gameEvent.EventType} by {gameEvent.PlayerId}", this);
                }
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error processing event {gameEvent.EventType}: {ex.Message}", this);
            }
        }

        private void ProcessEventListener(GameEvent gameEvent, AchievementEventListener listener)
        {
            listener.LastTriggered = gameEvent.Timestamp;
            listener.TriggerCount++;

            // Check for achievement triggers based on this event
            CheckAchievementTriggers(gameEvent, listener);
        }

        private void CheckAchievementTriggers(GameEvent gameEvent, AchievementEventListener listener)
        {
            // This would integrate with AchievementTrackingService to check for achievement completion
            // For now, we'll fire the event to notify other services
            var eventTriggerData = new AchievementTriggerData
            {
                EventType = gameEvent.EventType,
                PlayerId = gameEvent.PlayerId,
                Value = gameEvent.Value,
                Timestamp = gameEvent.Timestamp
            };

            // Check if this event might trigger achievements
            if (ShouldTriggerAchievementCheck(gameEvent))
            {
                OnAchievementTriggered?.Invoke(gameEvent.PlayerId, null); // Would pass actual achievement
                
                if (_enableEventLogging)
                {
                    ChimeraLogger.Log($"Achievement trigger detected for event: {gameEvent.EventType}", this);
                }
            }
        }

        private bool ShouldTriggerAchievementCheck(GameEvent gameEvent)
        {
            // Define events that are likely to trigger achievements
            var achievementTriggerEvents = new HashSet<string>
            {
                "plant_harvested", "breeding_completed", "research_completed",
                "product_sold", "milestone_reached", "perfect_completion",
                "level_up", "skill_point_earned", "social_recognition"
            };

            return achievementTriggerEvents.Contains(gameEvent.EventType) && gameEvent.Value > 0;
        }

        #endregion

        #region Progress Monitoring

        private void UpdateProgressTracking(GameEvent gameEvent)
        {
            string playerId = gameEvent.PlayerId;
            string eventType = gameEvent.EventType;
            
            if (!_playerProgressCache.ContainsKey(playerId))
            {
                _playerProgressCache[playerId] = new Dictionary<string, float>();
            }

            var playerProgress = _playerProgressCache[playerId];
            
            // Update cumulative progress for this event type
            if (playerProgress.ContainsKey(eventType))
            {
                float oldValue = playerProgress[eventType];
                playerProgress[eventType] += gameEvent.Value;
                
                // Check if progress update is significant enough to report
                if (Math.Abs(playerProgress[eventType] - oldValue) >= _progressUpdateThreshold)
                {
                    OnProgressUpdated?.Invoke(playerId, eventType, playerProgress[eventType]);
                    _lastProgressUpdate[playerId] = DateTime.Now;
                }
            }
            else
            {
                playerProgress[eventType] = gameEvent.Value;
                OnProgressUpdated?.Invoke(playerId, eventType, gameEvent.Value);
                _lastProgressUpdate[playerId] = DateTime.Now;
            }

            // Update progress prediction if enabled
            if (_enableProgressPrediction)
            {
                UpdateProgressPredictions(playerId, eventType, gameEvent.Value);
            }
        }

        private void UpdateProgressPredictions(string playerId, string eventType, float value)
        {
            // Calculate predicted achievement completion times based on current progress rate
            // This is a simplified implementation
            if (_playerBehaviorData.TryGetValue(playerId, out var behaviorData))
            {
                behaviorData.AverageSessionLength = (behaviorData.AverageSessionLength + Time.time) / 2f;
                behaviorData.EventFrequency[eventType] = behaviorData.EventFrequency.GetValueOrDefault(eventType, 0) + 1;
            }
        }

        private void UpdateProgressMonitoring()
        {
            if (!_enableProgressMonitoring) return;

            // Periodic progress monitoring and cleanup
            var currentTime = DateTime.Now;
            var playersToCleanup = new List<string>();

            foreach (var kvp in _lastProgressUpdate)
            {
                if ((currentTime - kvp.Value).TotalMinutes > 60) // Clean up after 1 hour of inactivity
                {
                    playersToCleanup.Add(kvp.Key);
                }
            }

            foreach (string playerId in playersToCleanup)
            {
                _lastProgressUpdate.Remove(playerId);
            }
        }

        #endregion

        #region Event Filtering

        private bool ShouldFilterEvent(string eventType, string playerId)
        {
            // Check if event type is filtered
            if (_filteredEventTypes.Contains(eventType))
            {
                return true;
            }

            // Check rate limiting
            if (_eventRateLimits.TryGetValue(eventType, out int rateLimit))
            {
                string rateLimitKey = $"{eventType}_{playerId}";
                if (_eventCooldowns.TryGetValue(rateLimitKey, out float lastTime))
                {
                    if (Time.time - lastTime < 1f / rateLimit)
                    {
                        return true; // Rate limited
                    }
                }
                _eventCooldowns[rateLimitKey] = Time.time;
            }

            return false;
        }

        #endregion

        #region Analytics

        private void UpdateEventAnalytics(GameEvent gameEvent)
        {
            var analyticsData = new EventAnalyticsData
            {
                EventType = gameEvent.EventType,
                PlayerId = gameEvent.PlayerId,
                Timestamp = gameEvent.Timestamp,
                Value = gameEvent.Value,
                SessionId = GetCurrentSessionId()
            };

            _analyticsHistory.Enqueue(analyticsData);

            // Keep analytics history within limits
            while (_analyticsHistory.Count > _analyticsHistorySize)
            {
                _analyticsHistory.Dequeue();
            }

            OnAnalyticsUpdate?.Invoke(analyticsData);
        }

        private void UpdatePlayerBehavior(string playerId, List<GameEvent> events)
        {
            if (!_playerBehaviorData.ContainsKey(playerId))
            {
                _playerBehaviorData[playerId] = new PlayerBehaviorData
                {
                    PlayerId = playerId,
                    FirstSeen = DateTime.Now,
                    EventFrequency = new Dictionary<string, int>(),
                    PreferredEventTypes = new List<string>(),
                    AverageSessionLength = 0f,
                    TotalEvents = 0
                };
            }

            var behaviorData = _playerBehaviorData[playerId];
            behaviorData.LastSeen = DateTime.Now;
            behaviorData.TotalEvents += events.Count;

            // Update event frequency data
            foreach (var gameEvent in events)
            {
                behaviorData.EventFrequency[gameEvent.EventType] = 
                    behaviorData.EventFrequency.GetValueOrDefault(gameEvent.EventType, 0) + 1;
            }

            // Update preferred event types
            UpdatePreferredEventTypes(behaviorData);

            // Calculate engagement score
            float engagementScore = CalculateEngagementScore(behaviorData);
            OnPlayerBehaviorChanged?.Invoke(playerId, engagementScore);
        }

        private void UpdatePreferredEventTypes(PlayerBehaviorData behaviorData)
        {
            behaviorData.PreferredEventTypes = behaviorData.EventFrequency
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private float CalculateEngagementScore(PlayerBehaviorData behaviorData)
        {
            // Calculate engagement based on event diversity and frequency
            float diversityScore = behaviorData.EventFrequency.Count / 10f; // Normalize to 0-1
            float frequencyScore = Math.Min(behaviorData.TotalEvents / 100f, 1f); // Cap at 100 events
            
            return (diversityScore + frequencyScore) / 2f;
        }

        private void UpdateAnalytics()
        {
            if (!_enableEventAnalytics) return;

            // Update trend data periodically
            var currentTime = DateTime.Now;
            if (_trendData.LastUpdate == default || (currentTime - _trendData.LastUpdate).TotalMinutes >= 5)
            {
                UpdateTrendData();
                _trendData.LastUpdate = currentTime;
            }
        }

        private void UpdateTrendData()
        {
            // Update daily event counts
            var today = DateTime.Today;
            
            foreach (var analyticsData in _analyticsHistory)
            {
                if (analyticsData.Timestamp.Date == today)
                {
                    if (!_trendData.DailyEventCounts.ContainsKey(analyticsData.EventType))
                    {
                        _trendData.DailyEventCounts[analyticsData.EventType] = new Dictionary<DateTime, int>();
                    }
                    
                    var eventCounts = _trendData.DailyEventCounts[analyticsData.EventType];
                    eventCounts[today] = eventCounts.GetValueOrDefault(today, 0) + 1;
                }
            }

            // Update player engagement metrics
            foreach (var kvp in _playerBehaviorData)
            {
                float engagementScore = CalculateEngagementScore(kvp.Value);
                _trendData.PlayerEngagementMetrics[kvp.Key] = engagementScore;
            }
        }

        #endregion

        #region Utility Methods

        private void ProcessRemainingEvents()
        {
            while (_eventQueue.Count > 0)
            {
                var gameEvent = _eventQueue.Dequeue();
                ProcessSingleEvent(gameEvent);
            }

            if (_eventBatch.Count > 0)
            {
                ProcessEventBatch(_eventBatch);
                _eventBatch.Clear();
            }
        }

        private void SaveAnalyticsData()
        {
            if (_enableEventAnalytics && _analyticsHistory.Count > 0)
            {
                // Save analytics data - would integrate with data persistence service
                ChimeraLogger.Log($"Saving analytics data: {_analyticsHistory.Count} events", this);
            }
        }

        private void UpdateEventMetrics(GameEvent gameEvent)
        {
            if (!_eventMetrics.ContainsKey(gameEvent.EventType))
            {
                _eventMetrics[gameEvent.EventType] = new AchievementEventMetrics
                {
                    EventType = gameEvent.EventType,
                    TotalCount = 0,
                    AverageValue = 0f,
                    LastOccurrence = DateTime.MinValue
                };
            }

            var metrics = _eventMetrics[gameEvent.EventType];
            metrics.TotalCount++;
            metrics.AverageValue = (metrics.AverageValue * (metrics.TotalCount - 1) + gameEvent.Value) / metrics.TotalCount;
            metrics.LastOccurrence = gameEvent.Timestamp;
        }

        private string GetCurrentSessionId()
        {
            // Simple session ID based on application start time
            return $"session_{Time.realtimeSinceStartup:F0}";
        }

        private void ClearAllData()
        {
            _eventQueue.Clear();
            _eventBatch.Clear();
            _eventListeners.Clear();
            _activeEventTypes.Clear();
            _playerProgressCache.Clear();
            _lastProgressUpdate.Clear();
            _eventMetrics.Clear();
            _analyticsHistory.Clear();
            _playerBehaviorData.Clear();
            _filteredEventTypes.Clear();
            _eventCooldowns.Clear();
            _eventRateLimits.Clear();
        }

        #endregion

        #region Public API

        public void RegisterCustomEventListener(string eventType, bool isActive = true)
        {
            if (!_eventListeners.ContainsKey(eventType))
            {
                _eventListeners[eventType] = new List<AchievementEventListener>();
                _activeEventTypes.Add(eventType);
            }

            var listener = new AchievementEventListener
            {
                EventType = eventType,
                IsActive = isActive,
                LastTriggered = DateTime.MinValue,
                TriggerCount = 0
            };

            _eventListeners[eventType].Add(listener);
            
            ChimeraLogger.Log($"Custom event listener registered: {eventType}", this);
        }

        public void SetEventFilter(string eventType, bool filtered)
        {
            if (filtered)
            {
                _filteredEventTypes.Add(eventType);
            }
            else
            {
                _filteredEventTypes.Remove(eventType);
            }
        }

        public void SetEventRateLimit(string eventType, int maxPerSecond)
        {
            _eventRateLimits[eventType] = maxPerSecond;
        }

        public AchievementEventMetrics GetEventMetrics(string eventType)
        {
            return _eventMetrics.TryGetValue(eventType, out var metrics) ? metrics : null;
        }

        public PlayerBehaviorData GetPlayerBehavior(string playerId)
        {
            return _playerBehaviorData.TryGetValue(playerId, out var data) ? data : null;
        }

        public Dictionary<string, float> GetPlayerProgress(string playerId)
        {
            return _playerProgressCache.TryGetValue(playerId, out var progress) ? 
                   new Dictionary<string, float>(progress) : new Dictionary<string, float>();
        }

        public List<EventAnalyticsData> GetRecentAnalytics(int count = 100)
        {
            return _analyticsHistory.TakeLast(count).ToList();
        }

        public void UpdateEventSettings(bool enableProcessing, bool enableTracking, float processingInterval)
        {
            _enableEventProcessing = enableProcessing;
            _enableRealTimeTracking = enableTracking;
            _eventProcessingInterval = processingInterval;
            
            ChimeraLogger.Log($"Event settings updated: processing={enableProcessing}, tracking={enableTracking}, interval={processingInterval}", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    /// <summary>
    /// Game event data structure for achievement system
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        public string EventId = "";
        public string EventType = "";
        public string PlayerId = "";
        public float Value = 0f;
        public DateTime Timestamp = DateTime.Now;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Event listener for achievement triggers
    /// </summary>
    [System.Serializable]
    public class AchievementEventListener
    {
        public string EventType = "";
        public bool IsActive = true;
        public DateTime LastTriggered = DateTime.MinValue;
        public int TriggerCount = 0;
    }

    /// <summary>
    /// Achievement trigger data
    /// </summary>
    [System.Serializable]
    public class AchievementTriggerData
    {
        public string EventType = "";
        public string PlayerId = "";
        public float Value = 0f;
        public DateTime Timestamp = DateTime.Now;
    }

    /// <summary>
    /// Event metrics for achievement system
    /// </summary>
    [System.Serializable]
    public class AchievementEventMetrics
    {
        public string EventType = "";
        public int TotalCount = 0;
        public float AverageValue = 0f;
        public DateTime LastOccurrence = DateTime.MinValue;
    }

    /// <summary>
    /// Event analytics data
    /// </summary>
    [System.Serializable]
    public class EventAnalyticsData
    {
        public string EventType = "";
        public string PlayerId = "";
        public DateTime Timestamp = DateTime.Now;
        public float Value = 0f;
        public string SessionId = "";
    }

    /// <summary>
    /// Player behavior tracking data
    /// </summary>
    [System.Serializable]
    public class PlayerBehaviorData
    {
        public string PlayerId = "";
        public DateTime FirstSeen = DateTime.Now;
        public DateTime LastSeen = DateTime.Now;
        public Dictionary<string, int> EventFrequency = new Dictionary<string, int>();
        public List<string> PreferredEventTypes = new List<string>();
        public float AverageSessionLength = 0f;
        public int TotalEvents = 0;
    }

    /// <summary>
    /// Achievement trend data for analytics
    /// </summary>
    [System.Serializable]
    public class AchievementTrendData
    {
        public DateTime InitializationTime = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
        public Dictionary<string, Dictionary<DateTime, int>> DailyEventCounts = new Dictionary<string, Dictionary<DateTime, int>>();
        public Dictionary<string, float> PlayerEngagementMetrics = new Dictionary<string, float>();
        public Dictionary<string, float> AchievementCompletionRates = new Dictionary<string, float>();
    }
}