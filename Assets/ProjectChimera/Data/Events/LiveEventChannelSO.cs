using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Core live event channel ScriptableObject for Project Chimera's event system.
    /// Provides typed event channels for live event coordination and communication.
    /// </summary>
    [CreateAssetMenu(fileName = "New Live Event Channel", menuName = "Project Chimera/Events/Channels/Live Event Channel", order = 200)]
    public class LiveEventChannelSO : ChimeraDataSO, IEventChannel
    {
        [Header("Channel Configuration")]
        [SerializeField] private string _channelId;
        [SerializeField] private string _channelName;
        [SerializeField] private string _description;
        [SerializeField] private EventChannelType _channelType = EventChannelType.LiveEvent;
        [SerializeField] private EventPriority _defaultPriority = EventPriority.Medium;
        
        [Header("Event Filtering")]
        [SerializeField] private bool _enableEventFiltering = false;
        [SerializeField] private List<LiveEventMessageType> _allowedEventTypes = new List<LiveEventMessageType>();
        [SerializeField] private List<string> _allowedEventTags = new List<string>();
        [SerializeField] private EventScope _allowedScope = EventScope.All;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableEventCaching = true;
        [SerializeField] private int _maxCachedEvents = 100;
        [SerializeField] private bool _enableRateLimiting = false;
        [SerializeField] private int _maxEventsPerSecond = 100;
        
        [Header("Subscription Management")]
        [SerializeField] private int _maxSubscriptions = 1000;
        [SerializeField] private bool _enableSubscriptionValidation = true;
        [SerializeField] private bool _enablePrioritySubscriptions = true;
        
        // Channel state
        private List<IEventSubscriber> _subscribers = new List<IEventSubscriber>();
        private Dictionary<EventPriority, List<IEventSubscriber>> _prioritySubscribers = new Dictionary<EventPriority, List<IEventSubscriber>>();
        private Queue<LiveEventMessage> _eventQueue = new Queue<LiveEventMessage>();
        private DateTime _lastEventTime;
        private int _eventsThisSecond;
        private EventChannelMetrics _metrics = new EventChannelMetrics();
        
        // Events
        public event Action<LiveEventMessage> OnEventRaised;
        public event Action<IEventSubscriber> OnSubscriberAdded;
        public event Action<IEventSubscriber> OnSubscriberRemoved;
        
        // Properties
        public string ChannelId => _channelId;
        public string ChannelName => _channelName;
        public string Description => _description;
        public EventChannelType ChannelType => _channelType;
        public EventPriority DefaultPriority => _defaultPriority;
        public int SubscriberCount => _subscribers.Count;
        public bool IsRateLimited => _enableRateLimiting;
        public EventChannelMetrics Metrics => _metrics;
        
        private void OnEnable()
        {
            InitializeChannel();
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Ensure channel ID is set
            if (string.IsNullOrEmpty(_channelId))
            {
                _channelId = $"live_event_channel_{Guid.NewGuid():N}";
            }
            
            // Validate performance settings
            _maxCachedEvents = Mathf.Max(1, _maxCachedEvents);
            _maxEventsPerSecond = Mathf.Max(1, _maxEventsPerSecond);
            _maxSubscriptions = Mathf.Max(1, _maxSubscriptions);
        }
        
        private void InitializeChannel()
        {
            // Initialize priority subscriber dictionary
            foreach (EventPriority priority in Enum.GetValues(typeof(EventPriority)))
            {
                _prioritySubscribers[priority] = new List<IEventSubscriber>();
            }
            
            // Reset metrics
            _metrics = new EventChannelMetrics
            {
                ChannelId = _channelId,
                ChannelType = _channelType,
                InitializationTime = DateTime.Now
            };
        }
        
        public bool Subscribe(IEventSubscriber subscriber, EventPriority priority = EventPriority.Medium)
        {
            if (subscriber == null)
            {
                Debug.LogWarning($"[LiveEventChannelSO] Attempted to subscribe null subscriber to channel {_channelId}");
                return false;
            }
            
            // Check subscription limits
            if (_subscribers.Count >= _maxSubscriptions)
            {
                Debug.LogWarning($"[LiveEventChannelSO] Channel {_channelId} has reached maximum subscriptions ({_maxSubscriptions})");
                return false;
            }
            
            // Validate subscription if enabled
            if (_enableSubscriptionValidation && !ValidateSubscriber(subscriber))
            {
                Debug.LogWarning($"[LiveEventChannelSO] Subscriber validation failed for channel {_channelId}");
                return false;
            }
            
            // Add to general subscribers
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                _metrics.TotalSubscriptions++;
            }
            
            // Add to priority subscribers if enabled
            if (_enablePrioritySubscriptions)
            {
                if (!_prioritySubscribers[priority].Contains(subscriber))
                {
                    _prioritySubscribers[priority].Add(subscriber);
                }
            }
            
            OnSubscriberAdded?.Invoke(subscriber);
            return true;
        }
        
        public bool Unsubscribe(IEventSubscriber subscriber)
        {
            if (subscriber == null) return false;
            
            var wasRemoved = _subscribers.Remove(subscriber);
            
            // Remove from priority subscribers
            if (_enablePrioritySubscriptions)
            {
                foreach (var priorityList in _prioritySubscribers.Values)
                {
                    priorityList.Remove(subscriber);
                }
            }
            
            if (wasRemoved)
            {
                OnSubscriberRemoved?.Invoke(subscriber);
                _metrics.TotalUnsubscriptions++;
            }
            
            return wasRemoved;
        }
        
        public void RaiseEvent(LiveEventMessage eventMessage)
        {
            if (eventMessage == null)
            {
                Debug.LogWarning($"[LiveEventChannelSO] Attempted to raise null event on channel {_channelId}");
                return;
            }
            
            // Check rate limiting
            if (_enableRateLimiting && IsRateLimitExceeded())
            {
                Debug.LogWarning($"[LiveEventChannelSO] Rate limit exceeded for channel {_channelId}");
                _metrics.RateLimitViolations++;
                return;
            }
            
            // Apply event filtering
            if (_enableEventFiltering && !PassesEventFilter(eventMessage))
            {
                _metrics.FilteredEvents++;
                return;
            }
            
            // Cache event if enabled
            if (_enableEventCaching)
            {
                CacheEvent(eventMessage);
            }
            
            // Notify subscribers
            NotifySubscribers(eventMessage);
            
            // Raise global event
            OnEventRaised?.Invoke(eventMessage);
            
            // Update metrics
            _metrics.EventsRaised++;
            _metrics.LastEventTime = DateTime.Now;
            
            UpdateRateLimitTracking();
        }
        
        public void RaiseEventImmediate(LiveEventMessage eventMessage)
        {
            eventMessage.Priority = EventPriority.Immediate;
            RaiseEvent(eventMessage);
        }
        
        public List<LiveEventMessage> GetCachedEvents(int maxEvents = -1)
        {
            if (!_enableEventCaching || _eventQueue.Count == 0)
                return new List<LiveEventMessage>();
            
            var events = new List<LiveEventMessage>();
            var eventsToReturn = maxEvents > 0 ? Mathf.Min(maxEvents, _eventQueue.Count) : _eventQueue.Count;
            
            var queueArray = _eventQueue.ToArray();
            for (int i = 0; i < eventsToReturn; i++)
            {
                events.Add(queueArray[i]);
            }
            
            return events;
        }
        
        public void ClearCache()
        {
            _eventQueue.Clear();
            _metrics.CacheClears++;
        }
        
        public EventChannelMetrics GetMetrics()
        {
            return _metrics;
        }
        
        private bool ValidateSubscriber(IEventSubscriber subscriber)
        {
            // Basic validation - can be extended with more sophisticated checks
            return subscriber != null && !string.IsNullOrEmpty(subscriber.SubscriberId);
        }
        
        private bool IsRateLimitExceeded()
        {
            var currentTime = DateTime.Now;
            
            // Reset counter if we're in a new second
            if (currentTime.Second != _lastEventTime.Second)
            {
                _eventsThisSecond = 0;
            }
            
            return _eventsThisSecond >= _maxEventsPerSecond;
        }
        
        private void UpdateRateLimitTracking()
        {
            var currentTime = DateTime.Now;
            
            if (currentTime.Second == _lastEventTime.Second)
            {
                _eventsThisSecond++;
            }
            else
            {
                _eventsThisSecond = 1;
            }
            
            _lastEventTime = currentTime;
        }
        
        private bool PassesEventFilter(LiveEventMessage eventMessage)
        {
            // Check allowed event types
            if (_allowedEventTypes.Count > 0 && !_allowedEventTypes.Contains(eventMessage.EventType))
            {
                return false;
            }
            
            // Check allowed tags
            if (_allowedEventTags.Count > 0)
            {
                bool hasAllowedTag = false;
                foreach (var tag in eventMessage.Tags)
                {
                    if (_allowedEventTags.Contains(tag))
                    {
                        hasAllowedTag = true;
                        break;
                    }
                }
                if (!hasAllowedTag) return false;
            }
            
            // Check event scope
            if (_allowedScope != EventScope.All)
            {
                if (eventMessage.Data.TryGetValue("eventScope", out var scopeValue))
                {
                    if (scopeValue is EventScope eventScope && eventScope != _allowedScope)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private void CacheEvent(LiveEventMessage eventMessage)
        {
            _eventQueue.Enqueue(eventMessage);
            
            // Maintain cache size limit
            while (_eventQueue.Count > _maxCachedEvents)
            {
                _eventQueue.Dequeue();
            }
            
            _metrics.CachedEvents++;
        }
        
        private void NotifySubscribers(LiveEventMessage eventMessage)
        {
            var subscribersToNotify = new List<IEventSubscriber>();
            
            // Collect subscribers based on priority
            if (_enablePrioritySubscriptions && eventMessage.Priority == EventPriority.Immediate)
            {
                // Notify immediate priority subscribers first
                subscribersToNotify.AddRange(_prioritySubscribers[EventPriority.Immediate]);
                subscribersToNotify.AddRange(_prioritySubscribers[EventPriority.High]);
            }
            else
            {
                // Notify all subscribers
                subscribersToNotify.AddRange(_subscribers);
            }
            
            // Notify subscribers
            foreach (var subscriber in subscribersToNotify)
            {
                try
                {
                    subscriber.OnEventReceived(eventMessage);
                    _metrics.NotificationsDelivered++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveEventChannelSO] Error notifying subscriber {subscriber.SubscriberId}: {ex.Message}");
                    _metrics.NotificationErrors++;
                }
            }
        }
    }
    
    /// <summary>
    /// Community event channel for community goals and collaborative events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Community Event Channel", menuName = "Project Chimera/Events/Channels/Community Event Channel", order = 202)]
    public class CommunityEventChannelSO : LiveEventChannelSO
    {
        [Header("Community Event Settings")]
        [SerializeField] private bool _enableCommunityValidation = true;
        [SerializeField] private int _minParticipantsForEvent = 1;
        [SerializeField] private bool _enableCollaborativeFeatures = true;
        
        public bool EnableCommunityValidation => _enableCommunityValidation;
        public int MinParticipantsForEvent => _minParticipantsForEvent;
        public bool EnableCollaborativeFeatures => _enableCollaborativeFeatures;
        
        public void RaiseCommunityGoalProgress(string goalId, float progress, float targetAmount, int participantCount)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.CommunityGoalProgress, "Community Goal Progress");
            eventMessage.SourceId = goalId;
            eventMessage.Timestamp = DateTime.Now;
            eventMessage.Priority = EventPriority.Medium;
            eventMessage.AddTag("community");
            eventMessage.AddTag("goal");
            eventMessage.AddTag("progress");
            
            eventMessage.SetData("goalId", goalId);
            eventMessage.SetData("progress", progress);
            eventMessage.SetData("targetAmount", targetAmount);
            eventMessage.SetData("progressPercentage", (progress / targetAmount) * 100f);
            eventMessage.SetData("participantCount", participantCount);
            
            RaiseEvent(eventMessage);
        }
    }
    
    /// <summary>
    /// Seasonal event channel for seasonal transitions and seasonal content events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Seasonal Event Channel", menuName = "Project Chimera/Events/Channels/Seasonal Event Channel", order = 203)]
    public class SeasonalEventChannelSO : LiveEventChannelSO
    {
        [Header("Seasonal Event Settings")]
        [SerializeField] private bool _enableSeasonalFiltering = true;
        [SerializeField] private bool _enableRegionalSupport = true;
        [SerializeField] private bool _enableTransitionEffects = true;
        
        public bool EnableSeasonalFiltering => _enableSeasonalFiltering;
        public bool EnableRegionalSupport => _enableRegionalSupport;
        public bool EnableTransitionEffects => _enableTransitionEffects;
        
        public void RaiseSeasonalTransition(Season newSeason, Season previousSeason)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.SeasonalTransition, $"Season Transition: {previousSeason} to {newSeason}");
            eventMessage.SourceId = "seasonal_manager";
            eventMessage.Timestamp = DateTime.Now;
            eventMessage.Priority = EventPriority.High;
            eventMessage.AddTag("seasonal");
            eventMessage.AddTag("transition");
            eventMessage.AddTag(newSeason.ToString().ToLower());
            
            eventMessage.SetData("newSeason", newSeason);
            eventMessage.SetData("previousSeason", previousSeason);
            eventMessage.SetData("transitionTime", DateTime.Now);
            
            RaiseEvent(eventMessage);
        }
        
        public void RaiseSeasonalContentChanged(Season season, string regionId)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.SeasonalTransition, "Seasonal Content Changed");
            eventMessage.SourceId = "seasonal_content_manager";
            eventMessage.Timestamp = DateTime.Now;
            eventMessage.Priority = EventPriority.Medium;
            eventMessage.AddTag("seasonal");
            eventMessage.AddTag("content");
            eventMessage.AddTag("changed");
            
            eventMessage.SetData("season", season);
            eventMessage.SetData("regionId", regionId);
            eventMessage.SetData("contentChangeTime", DateTime.Now);
            
            RaiseEvent(eventMessage);
        }
    }
    
    /// <summary>
    /// Competition event channel for competition-specific events and updates.
    /// </summary>
    [CreateAssetMenu(fileName = "New Competition Event Channel", menuName = "Project Chimera/Events/Channels/Competition Event Channel", order = 204)]
    public class CompetitionEventChannelSO : LiveEventChannelSO
    {
        [Header("Competition Event Settings")]
        [SerializeField] private List<CompetitionType> _allowedCompetitionTypes = new List<CompetitionType>();
        [SerializeField] private bool _enableCompetitionFiltering = true;
        [SerializeField] private bool _enableResultsValidation = true;
        
        public List<CompetitionType> AllowedCompetitionTypes => _allowedCompetitionTypes;
        public bool EnableCompetitionFiltering => _enableCompetitionFiltering;
        public bool EnableResultsValidation => _enableResultsValidation;
        
        public void RaiseCompetitionPhaseChange(string competitionId, CompetitionPhase newPhase, CompetitionPhase previousPhase)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.CompetitionPhaseChange, $"Competition Phase: {newPhase}");
            eventMessage.SourceId = competitionId;
            eventMessage.Timestamp = DateTime.Now;
            eventMessage.Priority = EventPriority.High;
            eventMessage.AddTag("competition");
            eventMessage.AddTag("phase");
            eventMessage.AddTag(newPhase.ToString().ToLower());
            
            eventMessage.SetData("competitionId", competitionId);
            eventMessage.SetData("newPhase", newPhase);
            eventMessage.SetData("previousPhase", previousPhase);
            eventMessage.SetData("phaseChangeTime", DateTime.Now);
            
            RaiseEvent(eventMessage);
        }
    }
    
    /// <summary>
    /// Educational event channel for learning and knowledge sharing events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Educational Event Channel", menuName = "Project Chimera/Events/Channels/Educational Event Channel", order = 205)]
    public class EducationalEventChannelSO : LiveEventChannelSO
    {
        [Header("Educational Event Settings")]
        [SerializeField] private bool _enableContentValidation = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private bool _enableMentorshipTracking = true;
        [SerializeField] private bool _enableProgressTracking = true;
        
        public bool EnableContentValidation => _enableContentValidation;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        public bool EnableMentorshipTracking => _enableMentorshipTracking;
        public bool EnableProgressTracking => _enableProgressTracking;
        
        public void RaiseLearningObjectiveCompleted(string playerId, string objectiveId, float completionScore)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.EducationalProgress, "Learning Objective Completed");
            eventMessage.SourceId = objectiveId;
            eventMessage.Timestamp = DateTime.Now;
            eventMessage.Priority = EventPriority.Medium;
            eventMessage.AddTag("education");
            eventMessage.AddTag("learning");
            eventMessage.AddTag("objective");
            eventMessage.AddTag("completed");
            
            eventMessage.SetData("playerId", playerId);
            eventMessage.SetData("objectiveId", objectiveId);
            eventMessage.SetData("completionScore", completionScore);
            eventMessage.SetData("completionTime", DateTime.Now);
            
            RaiseEvent(eventMessage);
        }
    }
    
    // Supporting interfaces and data structures
    public interface IEventChannel
    {
        string ChannelId { get; }
        string ChannelName { get; }
        EventChannelType ChannelType { get; }
        int SubscriberCount { get; }
        
        bool Subscribe(IEventSubscriber subscriber, EventPriority priority = EventPriority.Medium);
        bool Unsubscribe(IEventSubscriber subscriber);
        void RaiseEvent(LiveEventMessage eventMessage);
        EventChannelMetrics GetMetrics();
    }
    
    public interface IEventSubscriber
    {
        string SubscriberId { get; }
        void OnEventReceived(LiveEventMessage eventMessage);
    }
    
    public enum EventChannelType
    {
        LiveEvent,
        Seasonal,
        Community,
        Global,
        Competition,
        Educational,
        System
    }
    
    [Serializable]
    public class EventChannelMetrics
    {
        public string ChannelId;
        public EventChannelType ChannelType;
        public DateTime InitializationTime;
        public DateTime LastEventTime;
        public int EventsRaised;
        public int TotalSubscriptions;
        public int TotalUnsubscriptions;
        public int NotificationsDelivered;
        public int NotificationErrors;
        public int FilteredEvents;
        public int CachedEvents;
        public int CacheClears;
        public int RateLimitViolations;
    }
}