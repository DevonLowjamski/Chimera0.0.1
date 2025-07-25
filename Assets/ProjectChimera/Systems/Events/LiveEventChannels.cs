using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data.Events;

// Import key enums from Data.Events namespace
using Season = ProjectChimera.Data.Events.Season;
using CoreEventPriority = ProjectChimera.Core.Events.EventPriority;

namespace ProjectChimera.Systems.Events
{
    /// <summary>
    /// Advanced event channel system for Project Chimera's live events coordination.
    /// Provides typed event channels for different categories of live events with
    /// performance optimization, filtering, and cross-system communication.
    /// </summary>
    public class LiveEventChannels : ChimeraManager, IChimeraManager
    {
        [Header("Event Channel Configuration")]
        [SerializeField] private LiveEventChannelConfigSO _channelConfig;
        [SerializeField] private bool _enableEventFiltering = true;
        [SerializeField] private bool _enableEventPrioritization = true;
        [SerializeField] private bool _enableEventBatching = true;
        [SerializeField] private int _maxEventsPerBatch = 50;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private float _eventProcessingInterval = 0.1f;
        [SerializeField] private int _maxSubscriptionsPerChannel = 1000;
        [SerializeField] private bool _enableEventCaching = true;
        
        [Header("Core Event Channels - Live Events System")]
        [SerializeField] private LiveEventChannelSO _onLiveEventStarted;
        [SerializeField] private LiveEventChannelSO _onLiveEventEnded;
        [SerializeField] private LiveEventChannelSO _onLiveEventUpdated;
        [SerializeField] private LiveEventChannelSO _onPlayerActionProcessed;
        
        [Header("Seasonal Event Channels")]
        [SerializeField] private SeasonalEventChannelSO _onSeasonalTransition;
        [SerializeField] private SeasonalEventChannelSO _onSeasonalContentChanged;
        [SerializeField] private SeasonalEventChannelSO _onCulturalEventActivated;
        [SerializeField] private SeasonalEventChannelSO _onWeatherEventTriggered;
        
        [Header("Community Event Channels")]
        [SerializeField] private CommunityEventChannelSO _onCommunityGoalCreated;
        [SerializeField] private CommunityEventChannelSO _onCommunityGoalProgress;
        [SerializeField] private CommunityEventChannelSO _onCommunityGoalCompleted;
        [SerializeField] private CommunityEventChannelSO _onCollaborativeEventStarted;
        
        [Header("Global Event Channels")]
        [SerializeField] private GlobalEventChannelSO _onGlobalEventSynchronized;
        [SerializeField] private GlobalEventChannelSO _onGlobalChallengeAnnounced;
        [SerializeField] private GlobalEventChannelSO _onGlobalStateConflict;
        [SerializeField] private GlobalEventChannelSO _onCrisisResponseActivated;
        
        [Header("Competition Event Channels")]
        [SerializeField] private CompetitionEventChannelSO _onCompetitionPhaseChanged;
        [SerializeField] private CompetitionEventChannelSO _onCompetitionRegistration;
        [SerializeField] private CompetitionEventChannelSO _onCompetitionSubmission;
        [SerializeField] private CompetitionEventChannelSO _onCompetitionResults;
        
        [Header("Educational Event Channels")]
        [SerializeField] private EducationalEventChannelSO _onLearningObjectiveCompleted;
        [SerializeField] private EducationalEventChannelSO _onEducationalContentValidated;
        [SerializeField] private EducationalEventChannelSO _onMentorshipEstablished;
        [SerializeField] private EducationalEventChannelSO _onKnowledgeShared;
        
        // Event processing system
        private EventProcessor _eventProcessor;
        private EventFilterManager _filterManager;
        private EventPriorityQueue _priorityQueue;
        private EventBatchProcessor _batchProcessor;
        
        // Channel management
        private Dictionary<string, IEventChannel> _registeredChannels = new Dictionary<string, IEventChannel>();
        private Dictionary<Type, List<IEventChannel>> _channelsByType = new Dictionary<Type, List<IEventChannel>>();
        private EventChannelStatistics _channelStatistics = new EventChannelStatistics();
        
        // Performance monitoring
        private EventChannelPerformanceMonitor _performanceMonitor;
        private EventChannelCache _eventCache;
        private System.Collections.Concurrent.ConcurrentQueue<LiveEventMessage> _eventQueue = new System.Collections.Concurrent.ConcurrentQueue<LiveEventMessage>();
        
        // System state
        private bool _isProcessingEvents = false;
        private DateTime _lastEventProcessing;
        private Coroutine _eventProcessingCoroutine;
        
        #region ChimeraManager Implementation
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        protected override void OnManagerInitialize()
        {
            LogInfo("Initializing Live Event Channels...");
            
            if (!ValidateConfiguration())
            {
                LogError("Live Event Channels configuration validation failed");
                return;
            }
            
            InitializeEventProcessingSystem();
            RegisterEventChannels();
            InitializePerformanceMonitoring();
            StartEventProcessing();
            
            LogInfo("Live Event Channels initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo("Shutting down Live Event Channels...");
            
            StopEventProcessing();
            UnregisterEventChannels();
            DisposeResources();
            
            LogInfo("Live Event Channels shutdown complete");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || _eventProcessor == null)
                return;
            
            // Performance monitoring update
            if (_performanceMonitor != null)
            {
                _performanceMonitor.UpdateMetrics();
            }
            
            // Process any immediate priority events
            ProcessImmediatePriorityEvents();
        }
        
        private bool ValidateConfiguration()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            if (_channelConfig == null)
            {
                validationErrors.Add("Channel Config SO is not assigned");
                isValid = false;
            }
            
            // Validate core event channels
            if (_onLiveEventStarted == null)
            {
                validationErrors.Add("Live Event Started channel is not assigned");
                isValid = false;
            }
            
            if (_onLiveEventEnded == null)
            {
                validationErrors.Add("Live Event Ended channel is not assigned");
                isValid = false;
            }
            
            // Validate performance settings
            if (_eventProcessingInterval <= 0f)
            {
                validationErrors.Add("Event processing interval must be greater than 0");
                isValid = false;
            }
            
            if (_maxEventsPerBatch <= 0)
            {
                validationErrors.Add("Max events per batch must be greater than 0");
                isValid = false;
            }
            
            if (!isValid)
            {
                LogError($"Live Event Channels validation failed: {string.Join(", ", validationErrors)}");
            }
            else
            {
                LogInfo("Live Event Channels validation passed");
            }
            
            return isValid;
        }
        
        #endregion
        
        #region IChimeraManager Implementation
        
        public string ManagerName => "Live Event Channels Manager";
        
        public void Initialize()
        {
            OnManagerInitialize();
        }
        
        public void Shutdown()
        {
            OnManagerShutdown();
        }
        
        #endregion
        
        #region Event Processing System
        
        private void InitializeEventProcessingSystem()
        {
            // Initialize event processor
            _eventProcessor = new EventProcessor(_channelConfig);
            
            // Initialize filtering if enabled
            if (_enableEventFiltering)
            {
                _filterManager = new EventFilterManager(_channelConfig);
            }
            
            // Initialize prioritization if enabled
            if (_enableEventPrioritization)
            {
                _priorityQueue = new EventPriorityQueue(_channelConfig);
            }
            
            // Initialize batching if enabled
            if (_enableEventBatching)
            {
                _batchProcessor = new EventBatchProcessor(_maxEventsPerBatch);
            }
            
            LogInfo("Event processing system initialized");
        }
        
        private void InitializePerformanceMonitoring()
        {
            if (_enablePerformanceOptimization)
            {
                _performanceMonitor = new EventChannelPerformanceMonitor(_channelConfig);
            }
            
            if (_enableEventCaching)
            {
                _eventCache = new EventChannelCache(_channelConfig);
            }
        }
        
        private void StartEventProcessing()
        {
            if (_eventProcessingCoroutine == null)
            {
                _eventProcessingCoroutine = StartCoroutine(EventProcessingLoop());
            }
            
            _isProcessingEvents = true;
            LogInfo("Event processing started");
        }
        
        private void StopEventProcessing()
        {
            _isProcessingEvents = false;
            
            if (_eventProcessingCoroutine != null)
            {
                StopCoroutine(_eventProcessingCoroutine);
                _eventProcessingCoroutine = null;
            }
            
            LogInfo("Event processing stopped");
        }
        
        private System.Collections.IEnumerator EventProcessingLoop()
        {
            while (_isProcessingEvents)
            {
                yield return new WaitForSeconds(_eventProcessingInterval);
                
                try
                {
                    ProcessEventQueue();
                    _lastEventProcessing = DateTime.Now;
                }
                catch (Exception ex)
                {
                    LogError($"Error in event processing loop: {ex.Message}");
                }
            }
        }
        
        private void ProcessEventQueue()
        {
            if (!_eventQueue.IsEmpty)
            {
                var eventsToProcess = new List<LiveEventMessage>();
                
                // Dequeue events for batch processing
                int eventsProcessed = 0;
                while (_eventQueue.TryDequeue(out var eventMessage) && eventsProcessed < _maxEventsPerBatch)
                {
                    eventsToProcess.Add(eventMessage);
                    eventsProcessed++;
                }
                
                // Process events
                if (eventsToProcess.Count > 0)
                {
                    if (_enableEventBatching && _batchProcessor != null)
                    {
                        _batchProcessor.ProcessEventBatch(eventsToProcess);
                    }
                    else
                    {
                        foreach (var eventMessage in eventsToProcess)
                        {
                            ProcessSingleEvent(eventMessage);
                        }
                    }
                    
                    // Update statistics
                    _channelStatistics.EventsProcessed += eventsToProcess.Count;
                    _channelStatistics.LastProcessingTime = DateTime.Now;
                }
            }
        }
        
        private void ProcessSingleEvent(LiveEventMessage eventMessage)
        {
            try
            {
                // Apply filtering if enabled
                if (_enableEventFiltering && _filterManager != null)
                {
                    if (!_filterManager.ShouldProcessEvent(eventMessage))
                    {
                        _channelStatistics.EventsFiltered++;
                        return;
                    }
                }
                
                // Route event to appropriate channels
                RouteEventToChannels(eventMessage);
                
                // Cache event if caching is enabled
                if (_enableEventCaching && _eventCache != null)
                {
                    _eventCache.CacheEvent(eventMessage);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing event {eventMessage.EventType}: {ex.Message}");
                _channelStatistics.ProcessingErrors++;
            }
        }
        
        private void ProcessImmediatePriorityEvents()
        {
            if (_enableEventPrioritization && _priorityQueue != null)
            {
                var immediatePriorityEvents = _priorityQueue.GetImmediatePriorityEvents();
                foreach (var eventMessage in immediatePriorityEvents)
                {
                    ProcessSingleEvent(eventMessage);
                }
            }
        }
        
        #endregion
        
        #region Channel Management
        
        private void RegisterEventChannels()
        {
            // Register core live event channels
            RegisterChannel("live_event_started", _onLiveEventStarted);
            RegisterChannel("live_event_ended", _onLiveEventEnded);
            RegisterChannel("live_event_updated", _onLiveEventUpdated);
            RegisterChannel("player_action_processed", _onPlayerActionProcessed);
            
            // Register seasonal event channels
            RegisterChannel("seasonal_transition", _onSeasonalTransition);
            RegisterChannel("seasonal_content_changed", _onSeasonalContentChanged);
            RegisterChannel("cultural_event_activated", _onCulturalEventActivated);
            RegisterChannel("weather_event_triggered", _onWeatherEventTriggered);
            
            // Register community event channels
            RegisterChannel("community_goal_created", _onCommunityGoalCreated);
            RegisterChannel("community_goal_progress", _onCommunityGoalProgress);
            RegisterChannel("community_goal_completed", _onCommunityGoalCompleted);
            RegisterChannel("collaborative_event_started", _onCollaborativeEventStarted);
            
            // Register global event channels
            RegisterChannel("global_event_synchronized", _onGlobalEventSynchronized);
            RegisterChannel("global_challenge_announced", _onGlobalChallengeAnnounced);
            RegisterChannel("global_state_conflict", _onGlobalStateConflict);
            RegisterChannel("crisis_response_activated", _onCrisisResponseActivated);
            
            // Register competition event channels
            RegisterChannel("competition_phase_changed", _onCompetitionPhaseChanged);
            RegisterChannel("competition_registration", _onCompetitionRegistration);
            RegisterChannel("competition_submission", _onCompetitionSubmission);
            RegisterChannel("competition_results", _onCompetitionResults);
            
            // Register educational event channels
            RegisterChannel("learning_objective_completed", _onLearningObjectiveCompleted);
            RegisterChannel("educational_content_validated", _onEducationalContentValidated);
            RegisterChannel("mentorship_established", _onMentorshipEstablished);
            RegisterChannel("knowledge_shared", _onKnowledgeShared);
            
            LogInfo($"Registered {_registeredChannels.Count} event channels");
        }
        
        private void RegisterChannel(string channelId, IEventChannel channel)
        {
            if (channel == null)
            {
                LogWarning($"Attempted to register null channel: {channelId}");
                return;
            }
            
            _registeredChannels[channelId] = channel;
            
            // Group channels by type for efficient routing
            var channelType = channel.GetType();
            if (!_channelsByType.ContainsKey(channelType))
            {
                _channelsByType[channelType] = new List<IEventChannel>();
            }
            _channelsByType[channelType].Add(channel);
            
            _channelStatistics.RegisteredChannels++;
        }

        // Overloaded RegisterChannel methods for specific ScriptableObject channel types
        private void RegisterChannel(string channelId, LiveEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }

        private void RegisterChannel(string channelId, GlobalEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }

        private void RegisterChannel(string channelId, SeasonalEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }

        private void RegisterChannel(string channelId, CommunityEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }

        private void RegisterChannel(string channelId, CompetitionEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }

        private void RegisterChannel(string channelId, EducationalEventChannelSO channel)
        {
            if (channel != null)
                RegisterChannel(channelId, channel as IEventChannel);
        }
        
        private void UnregisterEventChannels()
        {
            _registeredChannels.Clear();
            _channelsByType.Clear();
            _channelStatistics.RegisteredChannels = 0;
            
            LogInfo("Event channels unregistered");
        }
        
        private void RouteEventToChannels(LiveEventMessage eventMessage)
        {
            var routedChannels = 0;
            
            // Route based on event type
            switch (eventMessage.MessageType)
            {
                case LiveEventMessageType.EventStarted:
                    _onLiveEventStarted?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.EventEnded:
                    _onLiveEventEnded?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.EventUpdated:
                    _onLiveEventUpdated?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.PlayerAction:
                    _onPlayerActionProcessed?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.SeasonalTransition:
                    _onSeasonalTransition?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.CommunityGoalProgress:
                    _onCommunityGoalProgress?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.GlobalSynchronization:
                    _onGlobalEventSynchronized?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.CompetitionPhaseChange:
                    _onCompetitionPhaseChanged?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.EducationalProgress:
                    _onLearningObjectiveCompleted?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.CrisisResponse:
                    _onCrisisResponseActivated?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
                    
                case LiveEventMessageType.SystemNotification:
                case LiveEventMessageType.GameStateChange:
                case LiveEventMessageType.CompetitionUpdate:
                case LiveEventMessageType.EducationalContent:
                case LiveEventMessageType.NarrativeEvent:
                case LiveEventMessageType.EnvironmentalChange:
                case LiveEventMessageType.EconomicUpdate:
                case LiveEventMessageType.Achievement:
                case LiveEventMessageType.Alert:
                case LiveEventMessageType.Debug:
                default:
                    // Route to general global channels for unspecific event types
                    _onGlobalEventSynchronized?.RaiseEvent(eventMessage);
                    routedChannels++;
                    break;
            }
            
            _channelStatistics.EventsRouted += routedChannels;
        }
        
        #endregion
        
        #region Public API
        
        public void PublishEvent(LiveEventMessage eventMessage)
        {
            if (eventMessage == null)
            {
                LogWarning("Attempted to publish null event message");
                return;
            }
            
            // Add to processing queue
            _eventQueue.Enqueue(eventMessage);
            _channelStatistics.EventsPublished++;
            
            // Process immediately if high priority
            if (_enableEventPrioritization && eventMessage.Priority == CoreEventPriority.Immediate)
            {
                ProcessSingleEvent(eventMessage);
            }
        }
        
        public void PublishLiveEventStarted(ILiveEvent liveEvent)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.EventStarted, "Live Event Started")
            {
                SourceId = liveEvent.EventId
            };
            eventMessage.SetData("liveEvent", liveEvent);
            eventMessage.SetData("eventName", liveEvent.EventName);
            eventMessage.SetData("eventScope", liveEvent.Scope);
            eventMessage.Priority = CoreEventPriority.High;
            
            PublishEvent(eventMessage);
        }
        
        public void PublishLiveEventEnded(ILiveEvent liveEvent, EventResult result)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.EventEnded, "Live Event Ended")
            {
                SourceId = liveEvent.EventId
            };
            eventMessage.SetData("liveEvent", liveEvent);
            eventMessage.SetData("eventResult", result);
            eventMessage.SetData("duration", liveEvent.Duration);
            eventMessage.Priority = CoreEventPriority.High;
            
            PublishEvent(eventMessage);
        }
        
        public void PublishSeasonalTransition(Season newSeason, Season previousSeason)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.SeasonalTransition, "Seasonal Transition");
            eventMessage.SetData("newSeason", newSeason);
            eventMessage.SetData("previousSeason", previousSeason);
            eventMessage.SetData("transitionTime", DateTime.Now);
            eventMessage.Priority = CoreEventPriority.Medium;
            
            PublishEvent(eventMessage);
        }
        
        public void PublishCommunityGoalProgress(string goalId, float progress, float targetAmount)
        {
            var eventMessage = new LiveEventMessage(LiveEventMessageType.CommunityGoalProgress, "Community Goal Progress")
            {
                SourceId = goalId
            };
            eventMessage.SetData("goalId", goalId);
            eventMessage.SetData("progress", progress);
            eventMessage.SetData("targetAmount", targetAmount);
            eventMessage.SetData("progressPercentage", (progress / targetAmount) * 100f);
            eventMessage.Priority = CoreEventPriority.Medium;
            
            PublishEvent(eventMessage);
        }
        
        public EventChannelStatistics GetChannelStatistics()
        {
            return _channelStatistics;
        }
        
        public List<IEventChannel> GetRegisteredChannels()
        {
            return new List<IEventChannel>(_registeredChannels.Values);
        }
        
        public IEventChannel GetChannel(string channelId)
        {
            return _registeredChannels.GetValueOrDefault(channelId);
        }
        
        #endregion
        
        #region Cleanup
        
        private void DisposeResources()
        {
            _eventProcessor?.Dispose();
            _filterManager?.Dispose();
            _priorityQueue?.Dispose();
            _batchProcessor?.Dispose();
            _performanceMonitor?.Dispose();
            _eventCache?.Dispose();
        }
        
        #endregion
    }
    
    // Supporting data structures
    
    [Serializable]
    public class EventChannelStatistics
    {
        public int RegisteredChannels;
        public int EventsPublished;
        public int EventsProcessed;
        public int EventsRouted;
        public int EventsFiltered;
        public int ProcessingErrors;
        public DateTime LastProcessingTime;
        public float AverageProcessingTime;
    }
    
    // Placeholder classes for compilation
    public class EventProcessor
    {
        public EventProcessor(LiveEventChannelConfigSO config) { }
        public void Dispose() { }
    }
    
    public class EventFilterManager
    {
        public EventFilterManager(LiveEventChannelConfigSO config) { }
        public bool ShouldProcessEvent(LiveEventMessage eventMessage) => true;
        public void Dispose() { }
    }
    
    public class EventPriorityQueue
    {
        public EventPriorityQueue(LiveEventChannelConfigSO config) { }
        public List<LiveEventMessage> GetImmediatePriorityEvents() => new List<LiveEventMessage>();
        public void Dispose() { }
    }
    
    public class EventBatchProcessor
    {
        public EventBatchProcessor(int maxBatchSize) { }
        public void ProcessEventBatch(List<LiveEventMessage> events) { }
        public void Dispose() { }
    }
    
    public class EventChannelPerformanceMonitor
    {
        public EventChannelPerformanceMonitor(LiveEventChannelConfigSO config) { }
        public void UpdateMetrics() { }
        public void Dispose() { }
    }
    
    public class EventChannelCache
    {
        public EventChannelCache(LiveEventChannelConfigSO config) { }
        public void CacheEvent(LiveEventMessage eventMessage) { }
        public void Dispose() { }
    }
}