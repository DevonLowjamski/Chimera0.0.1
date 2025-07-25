using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Type-safe event channel for story progression events in Project Chimera's narrative system.
    /// Features comprehensive event validation, history tracking, analytics integration, and
    /// educational content progression monitoring with scientific accuracy validation.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Event Channel", menuName = "Project Chimera/Events/Narrative/Story Event Channel", order = 200)]
    public class StoryEventChannelSO : GameEventChannelSO<StoryEventData>
    {
        [Header("Story Event Configuration")]
        [SerializeField] private bool _enableEventValidation = true;
        [SerializeField] private bool _enableEventHistory = true;
        [SerializeField] private int _maxHistoryEntries = 100;
        [SerializeField] private bool _enableAnalyticsIntegration = true;
        [SerializeField] private bool _enableEducationalTracking = true;
        
        [Header("Event Filtering")]
        [SerializeField] private List<StoryEventType> _allowedEventTypes = new List<StoryEventType>();
        [SerializeField] private List<string> _blockedArcIds = new List<string>();
        [SerializeField] private bool _requireScientificValidation = true;
        [SerializeField] private float _minimumEducationalWeight = 0.0f;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableEventBatching = false;
        [SerializeField] private int _maxBatchSize = 10;
        [SerializeField] private float _batchProcessingInterval = 0.1f;
        [SerializeField] private bool _enableAsyncProcessing = false;
        
        // Event history and analytics
        private List<StoryEventData> _eventHistory = new List<StoryEventData>();
        private Queue<StoryEventData> _batchedEvents = new Queue<StoryEventData>();
        private Dictionary<StoryEventType, int> _eventTypeCounters = new Dictionary<StoryEventType, int>();
        private Dictionary<string, List<StoryEventData>> _arcEventHistory = new Dictionary<string, List<StoryEventData>>();
        
        // Performance monitoring
        private float _lastBatchProcessTime;
        private int _totalEventsProcessed;
        private float _averageProcessingTime;
        
        public override void Raise(StoryEventData eventData)
        {
            if (!ValidateEvent(eventData))
            {
                Debug.LogWarning($"[StoryEventChannelSO] Invalid story event rejected: {eventData?.EventType}");
                return;
            }
            
            // Add timestamp
            eventData.Timestamp = DateTime.Now;
            
            // Process or batch the event
            if (_enableEventBatching)
            {
                BatchEvent(eventData);
            }
            else
            {
                ProcessEventImmediate(eventData);
            }
        }
        
        private bool ValidateEvent(StoryEventData eventData)
        {
            if (!_enableEventValidation) return true;
            if (eventData == null) return false;
            
            // Check event type filtering
            if (_allowedEventTypes.Count > 0 && !_allowedEventTypes.Contains(eventData.EventType))
            {
                return false;
            }
            
            // Check blocked arc IDs
            if (!string.IsNullOrEmpty(eventData.ArcId) && _blockedArcIds.Contains(eventData.ArcId))
            {
                return false;
            }
            
            // Validate educational content if required
            if (_enableEducationalTracking && eventData.HasEducationalContent)
            {
                if (eventData.EducationalWeight < _minimumEducationalWeight)
                {
                    return false;
                }
                
                if (_requireScientificValidation && !eventData.IsScientificallyValidated)
                {
                    return false;
                }
            }
            
            // Validate required fields
            if (string.IsNullOrEmpty(eventData.EventId))
            {
                return false;
            }
            
            return true;
        }
        
        private void BatchEvent(StoryEventData eventData)
        {
            _batchedEvents.Enqueue(eventData);
            
            // Process batch if size limit reached or enough time has passed
            if (_batchedEvents.Count >= _maxBatchSize || 
                Time.time - _lastBatchProcessTime >= _batchProcessingInterval)
            {
                ProcessEventBatch();
            }
        }
        
        private void ProcessEventBatch()
        {
            var startTime = Time.realtimeSinceStartup;
            var batchSize = _batchedEvents.Count;
            
            while (_batchedEvents.Count > 0)
            {
                var eventData = _batchedEvents.Dequeue();
                ProcessEventImmediate(eventData);
            }
            
            var processingTime = Time.realtimeSinceStartup - startTime;
            UpdatePerformanceMetrics(batchSize, processingTime);
            
            _lastBatchProcessTime = Time.time;
        }
        
        private void ProcessEventImmediate(StoryEventData eventData)
        {
            var startTime = Time.realtimeSinceStartup;
            
            // Add to history
            AddToHistory(eventData);
            
            // Update counters
            UpdateEventCounters(eventData);
            
            // Track arc-specific events
            TrackArcEvent(eventData);
            
            // Integrate with analytics
            if (_enableAnalyticsIntegration)
            {
                IntegrateWithAnalytics(eventData);
            }
            
            // Raise the actual event
            base.Raise(eventData);
            
            var processingTime = Time.realtimeSinceStartup - startTime;
            UpdatePerformanceMetrics(1, processingTime);
        }
        
        private void AddToHistory(StoryEventData eventData)
        {
            if (!_enableEventHistory) return;
            
            _eventHistory.Add(eventData);
            
            // Maintain history size limit
            if (_eventHistory.Count > _maxHistoryEntries)
            {
                _eventHistory.RemoveAt(0);
            }
        }
        
        private void UpdateEventCounters(StoryEventData eventData)
        {
            if (!_eventTypeCounters.ContainsKey(eventData.EventType))
            {
                _eventTypeCounters[eventData.EventType] = 0;
            }
            
            _eventTypeCounters[eventData.EventType]++;
        }
        
        private void TrackArcEvent(StoryEventData eventData)
        {
            if (string.IsNullOrEmpty(eventData.ArcId)) return;
            
            if (!_arcEventHistory.ContainsKey(eventData.ArcId))
            {
                _arcEventHistory[eventData.ArcId] = new List<StoryEventData>();
            }
            
            _arcEventHistory[eventData.ArcId].Add(eventData);
        }
        
        private void IntegrateWithAnalytics(StoryEventData eventData)
        {
            // Create analytics event
            var analyticsEvent = new AnalyticsEventData
            {
                EventType = "StoryProgression",
                EventId = eventData.EventId,
                ArcId = eventData.ArcId,
                BeatId = eventData.BeatId,
                Timestamp = eventData.Timestamp,
                EducationalWeight = eventData.EducationalWeight,
                PlayerChoiceId = eventData.PlayerChoiceId,
                CharacterIds = eventData.InvolvedCharacterIds,
                CustomData = eventData.AdditionalData
            };
            
            // Send to analytics system (placeholder for actual implementation)
            // AnalyticsManager.Instance?.TrackEvent(analyticsEvent);
        }
        
        private void UpdatePerformanceMetrics(int eventCount, float processingTime)
        {
            _totalEventsProcessed += eventCount;
            
            // Update rolling average
            var weight = 0.1f; // Exponential moving average weight
            _averageProcessingTime = (_averageProcessingTime * (1f - weight)) + (processingTime * weight);
        }
        
        /// <summary>
        /// Get event history for specific arc
        /// </summary>
        public List<StoryEventData> GetArcEventHistory(string arcId)
        {
            return _arcEventHistory.ContainsKey(arcId) ? 
                new List<StoryEventData>(_arcEventHistory[arcId]) : 
                new List<StoryEventData>();
        }
        
        /// <summary>
        /// Get recent events of specific type
        /// </summary>
        public List<StoryEventData> GetRecentEventsByType(StoryEventType eventType, int count = 10)
        {
            var recentEvents = new List<StoryEventData>();
            
            for (int i = _eventHistory.Count - 1; i >= 0 && recentEvents.Count < count; i--)
            {
                if (_eventHistory[i].EventType == eventType)
                {
                    recentEvents.Add(_eventHistory[i]);
                }
            }
            
            return recentEvents;
        }
        
        /// <summary>
        /// Get event statistics
        /// </summary>
        public StoryEventStatistics GetEventStatistics()
        {
            return new StoryEventStatistics
            {
                TotalEventsProcessed = _totalEventsProcessed,
                AverageProcessingTime = _averageProcessingTime,
                EventTypeDistribution = new Dictionary<StoryEventType, int>(_eventTypeCounters),
                ActiveArcs = _arcEventHistory.Keys.Count,
                HistorySize = _eventHistory.Count,
                LastEventTime = _eventHistory.Count > 0 ? _eventHistory[_eventHistory.Count - 1].Timestamp : DateTime.MinValue
            };
        }
        
        /// <summary>
        /// Clear event history
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
            _arcEventHistory.Clear();
            _eventTypeCounters.Clear();
        }
        
        /// <summary>
        /// Force process any batched events
        /// </summary>
        public void FlushBatchedEvents()
        {
            if (_batchedEvents.Count > 0)
            {
                ProcessEventBatch();
            }
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate configuration
            if (_maxHistoryEntries <= 0)
            {
                _maxHistoryEntries = 100;
            }
            
            if (_maxBatchSize <= 0)
            {
                _maxBatchSize = 10;
            }
            
            if (_batchProcessingInterval <= 0f)
            {
                _batchProcessingInterval = 0.1f;
            }
            
            if (_minimumEducationalWeight < 0f || _minimumEducationalWeight > 1f)
            {
                _minimumEducationalWeight = Mathf.Clamp01(_minimumEducationalWeight);
            }
        }
    }
    
    /// <summary>
    /// Story event data structure
    /// </summary>
    [Serializable]
    public class StoryEventData
    {
        [Header("Core Event Data")]
        public string EventId;
        public StoryEventType EventType;
        public string ArcId;
        public string BeatId;
        public DateTime Timestamp;
        
        [Header("Player Interaction")]
        public string PlayerChoiceId;
        public string SelectedChoiceText;
        public float DecisionTime;
        public bool WasTimeoutChoice;
        
        [Header("Character Involvement")]
        public List<string> InvolvedCharacterIds = new List<string>();
        public string PrimarySpeakerId;
        public Dictionary<string, float> RelationshipChanges = new Dictionary<string, float>();
        
        [Header("Educational Content")]
        public bool HasEducationalContent;
        public float EducationalWeight;
        public List<string> LearningObjectiveIds = new List<string>();
        public List<string> CultivationTopics = new List<string>();
        public bool IsScientificallyValidated;
        
        [Header("Consequences and Outcomes")]
        public List<string> TriggeredConsequences = new List<string>();
        public Dictionary<string, float> StateChanges = new Dictionary<string, float>();
        public List<string> UnlockedContent = new List<string>();
        
        [Header("Performance Data")]
        public float ProcessingTime;
        public bool WasSkipped;
        public float TimeSpentOnBeat;
        public int DialogueEntriesViewed;
        
        [Header("Additional Context")]
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
        public string Notes;
        public float Priority = 1.0f;
    }
    
    /// <summary>
    /// Analytics event data for integration
    /// </summary>
    [Serializable]
    public class AnalyticsEventData
    {
        public string EventType;
        public string EventId;
        public string ArcId;
        public string BeatId;
        public DateTime Timestamp;
        public float EducationalWeight;
        public string PlayerChoiceId;
        public List<string> CharacterIds = new List<string>();
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Story event statistics
    /// </summary>
    [Serializable]
    public class StoryEventStatistics
    {
        public int TotalEventsProcessed;
        public float AverageProcessingTime;
        public Dictionary<StoryEventType, int> EventTypeDistribution = new Dictionary<StoryEventType, int>();
        public int ActiveArcs;
        public int HistorySize;
        public DateTime LastEventTime;
    }
    
    /// <summary>
    /// Types of story events
    /// </summary>
    public enum StoryEventType
    {
        BeatStarted,
        BeatCompleted,
        ChoiceSelected,
        DialogueStarted,
        DialogueCompleted,
        ArcStarted,
        ArcCompleted,
        CharacterIntroduced,
        RelationshipChanged,
        ConsequenceTriggered,
        EducationalMilestone,
        SkillProgression,
        ContentUnlocked,
        AchievementEarned,
        BranchingPointReached,
        StoryPathChanged,
        EmotionalResponse,
        LearningAssessment,
        ScientificValidation,
        Custom
    }
}