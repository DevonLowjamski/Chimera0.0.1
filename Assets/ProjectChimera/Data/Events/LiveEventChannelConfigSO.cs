using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Configuration ScriptableObject for Project Chimera's live event channel system.
    /// Provides comprehensive settings for event processing, filtering, performance optimization,
    /// and channel management for the live events system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Live Event Channel Config", menuName = "Project Chimera/Events/Live Event Channel Config", order = 150)]
    public class LiveEventChannelConfigSO : ChimeraConfigSO
    {
        [Header("Core Channel Configuration")]
        [SerializeField] private bool _enableEventChannels = true;
        [SerializeField] private bool _enableGlobalEventCoordination = true;
        [SerializeField] private bool _enableCrossSystemCommunication = true;
        [SerializeField] private bool _enableEventValidation = true;
        
        [Header("Performance Optimization")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private float _eventProcessingInterval = 0.1f;
        [SerializeField] private int _maxEventsPerBatch = 50;
        [SerializeField] private int _maxConcurrentChannels = 100;
        [SerializeField] private bool _enableEventBatching = true;
        [SerializeField] private bool _enableAsyncProcessing = true;
        
        [Header("Event Filtering and Prioritization")]
        [SerializeField] private bool _enableEventFiltering = true;
        [SerializeField] private bool _enableEventPrioritization = true;
        [SerializeField] private bool _enableContentFiltering = true;
        [SerializeField] private EventFilteringMode _filteringMode = EventFilteringMode.Smart;
        [SerializeField] private List<EventFilterRule> _globalFilterRules = new List<EventFilterRule>();
        
        [Header("Rate Limiting and Traffic Control")]
        [SerializeField] private bool _enableRateLimiting = true;
        [SerializeField] private int _globalMaxEventsPerSecond = 1000;
        [SerializeField] private int _channelMaxEventsPerSecond = 100;
        [SerializeField] private bool _enableTrafficShaping = true;
        [SerializeField] private float _burstAllowanceMultiplier = 2.0f;
        
        [Header("Caching and Storage")]
        [SerializeField] private bool _enableEventCaching = true;
        [SerializeField] private int _globalCacheSize = 10000;
        [SerializeField] private int _channelCacheSize = 1000;
        [SerializeField] private float _cacheExpirationTime = 3600f; // 1 hour
        [SerializeField] private bool _enablePersistentCaching = false;
        
        [Header("Channel Management")]
        [SerializeField] private int _maxChannelsPerType = 50;
        [SerializeField] private int _maxSubscriptionsPerChannel = 1000;
        [SerializeField] private bool _enableDynamicChannelCreation = true;
        [SerializeField] private bool _enableChannelHealthMonitoring = true;
        [SerializeField] private float _channelHealthCheckInterval = 60f;
        
        [Header("Event Types Configuration")]
        [SerializeField] private List<EventTypeConfig> _eventTypeConfigs = new List<EventTypeConfig>();
        [SerializeField] private Dictionary<LiveEventMessageType, EventPriority> _defaultPriorities = new Dictionary<LiveEventMessageType, EventPriority>();
        [SerializeField] private bool _enableCustomEventTypes = true;
        
        [Header("Security and Validation")]
        [SerializeField] private bool _enableEventSecurity = true;
        [SerializeField] private bool _enableSubscriberValidation = true;
        [SerializeField] private bool _enableEventAuthentication = false;
        [SerializeField] private bool _enableEventEncryption = false;
        [SerializeField] private List<string> _trustedEventSources = new List<string>();
        
        [Header("Monitoring and Analytics")]
        [SerializeField] private bool _enableEventAnalytics = true;
        [SerializeField] private bool _enablePerformanceMetrics = true;
        [SerializeField] private bool _enableDetailedLogging = false;
        [SerializeField] private float _metricsUpdateInterval = 10f;
        [SerializeField] private int _maxMetricsHistory = 1000;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalEventValidation = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private bool _enableCulturalSensitivityCheck = true;
        [SerializeField] private float _educationalContentQualityThreshold = 0.8f;
        
        [Header("Community Features")]
        [SerializeField] private bool _enableCommunityModeration = true;
        [SerializeField] private bool _enableCommunityVoting = true;
        [SerializeField] private bool _enableUserGeneratedEvents = false;
        [SerializeField] private int _minimumParticipantsForCommunityEvent = 5;
        
        [Header("Crisis Response Configuration")]
        [SerializeField] private bool _enableCrisisResponseChannels = true;
        [SerializeField] private EventPriority _crisisEventPriority = EventPriority.Immediate;
        [SerializeField] private bool _enableEmergencyBroadcast = true;
        [SerializeField] private float _crisisEventTimeout = 300f; // 5 minutes
        
        // Properties for easy access
        public bool EnableEventChannels => _enableEventChannels;
        public bool EnableGlobalEventCoordination => _enableGlobalEventCoordination;
        public bool EnableCrossSystemCommunication => _enableCrossSystemCommunication;
        public bool EnableEventValidation => _enableEventValidation;
        
        public bool EnablePerformanceOptimization => _enablePerformanceOptimization;
        public float EventProcessingInterval => _eventProcessingInterval;
        public int MaxEventsPerBatch => _maxEventsPerBatch;
        public int MaxConcurrentChannels => _maxConcurrentChannels;
        public bool EnableEventBatching => _enableEventBatching;
        public bool EnableAsyncProcessing => _enableAsyncProcessing;
        
        public bool EnableEventFiltering => _enableEventFiltering;
        public bool EnableEventPrioritization => _enableEventPrioritization;
        public bool EnableContentFiltering => _enableContentFiltering;
        public EventFilteringMode FilteringMode => _filteringMode;
        public List<EventFilterRule> GlobalFilterRules => _globalFilterRules;
        
        public bool EnableRateLimiting => _enableRateLimiting;
        public int GlobalMaxEventsPerSecond => _globalMaxEventsPerSecond;
        public int ChannelMaxEventsPerSecond => _channelMaxEventsPerSecond;
        public bool EnableTrafficShaping => _enableTrafficShaping;
        public float BurstAllowanceMultiplier => _burstAllowanceMultiplier;
        
        public bool EnableEventCaching => _enableEventCaching;
        public int GlobalCacheSize => _globalCacheSize;
        public int ChannelCacheSize => _channelCacheSize;
        public float CacheExpirationTime => _cacheExpirationTime;
        public bool EnablePersistentCaching => _enablePersistentCaching;
        
        public int MaxChannelsPerType => _maxChannelsPerType;
        public int MaxSubscriptionsPerChannel => _maxSubscriptionsPerChannel;
        public bool EnableDynamicChannelCreation => _enableDynamicChannelCreation;
        public bool EnableChannelHealthMonitoring => _enableChannelHealthMonitoring;
        public float ChannelHealthCheckInterval => _channelHealthCheckInterval;
        
        public List<EventTypeConfig> EventTypeConfigs => _eventTypeConfigs;
        public bool EnableCustomEventTypes => _enableCustomEventTypes;
        
        public bool EnableEventSecurity => _enableEventSecurity;
        public bool EnableSubscriberValidation => _enableSubscriberValidation;
        public bool EnableEventAuthentication => _enableEventAuthentication;
        public bool EnableEventEncryption => _enableEventEncryption;
        public List<string> TrustedEventSources => _trustedEventSources;
        
        public bool EnableEventAnalytics => _enableEventAnalytics;
        public bool EnablePerformanceMetrics => _enablePerformanceMetrics;
        public bool EnableDetailedLogging => _enableDetailedLogging;
        public float MetricsUpdateInterval => _metricsUpdateInterval;
        public int MaxMetricsHistory => _maxMetricsHistory;
        
        public bool EnableEducationalEventValidation => _enableEducationalEventValidation;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        public bool EnableCulturalSensitivityCheck => _enableCulturalSensitivityCheck;
        public float EducationalContentQualityThreshold => _educationalContentQualityThreshold;
        
        public bool EnableCommunityModeration => _enableCommunityModeration;
        public bool EnableCommunityVoting => _enableCommunityVoting;
        public bool EnableUserGeneratedEvents => _enableUserGeneratedEvents;
        public int MinimumParticipantsForCommunityEvent => _minimumParticipantsForCommunityEvent;
        
        public bool EnableCrisisResponseChannels => _enableCrisisResponseChannels;
        public EventPriority CrisisEventPriority => _crisisEventPriority;
        public bool EnableEmergencyBroadcast => _enableEmergencyBroadcast;
        public float CrisisEventTimeout => _crisisEventTimeout;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate performance settings
            _eventProcessingInterval = Mathf.Max(0.01f, _eventProcessingInterval);
            _maxEventsPerBatch = Mathf.Max(1, _maxEventsPerBatch);
            _maxConcurrentChannels = Mathf.Max(1, _maxConcurrentChannels);
            
            // Validate rate limiting settings
            _globalMaxEventsPerSecond = Mathf.Max(1, _globalMaxEventsPerSecond);
            _channelMaxEventsPerSecond = Mathf.Max(1, _channelMaxEventsPerSecond);
            _burstAllowanceMultiplier = Mathf.Max(1.0f, _burstAllowanceMultiplier);
            
            // Validate caching settings
            _globalCacheSize = Mathf.Max(1, _globalCacheSize);
            _channelCacheSize = Mathf.Max(1, _channelCacheSize);
            _cacheExpirationTime = Mathf.Max(1f, _cacheExpirationTime);
            
            // Validate channel settings
            _maxChannelsPerType = Mathf.Max(1, _maxChannelsPerType);
            _maxSubscriptionsPerChannel = Mathf.Max(1, _maxSubscriptionsPerChannel);
            _channelHealthCheckInterval = Mathf.Max(1f, _channelHealthCheckInterval);
            
            // Validate monitoring settings
            _metricsUpdateInterval = Mathf.Max(0.1f, _metricsUpdateInterval);
            _maxMetricsHistory = Mathf.Max(1, _maxMetricsHistory);
            
            // Validate educational settings
            _educationalContentQualityThreshold = Mathf.Clamp01(_educationalContentQualityThreshold);
            
            // Validate community settings
            _minimumParticipantsForCommunityEvent = Mathf.Max(1, _minimumParticipantsForCommunityEvent);
            
            // Validate crisis response settings
            _crisisEventTimeout = Mathf.Max(1f, _crisisEventTimeout);
            
            // Initialize default priorities if empty
            InitializeDefaultPriorities();
            
            // Initialize default event type configs if empty
            InitializeDefaultEventTypeConfigs();
        }
        
        private void InitializeDefaultPriorities()
        {
            if (_defaultPriorities.Count == 0)
            {
                _defaultPriorities = new Dictionary<LiveEventMessageType, EventPriority>
                {
                    { LiveEventMessageType.EventStarted, EventPriority.High },
                    { LiveEventMessageType.EventEnded, EventPriority.High },
                    { LiveEventMessageType.EventUpdated, EventPriority.Medium },
                    { LiveEventMessageType.PlayerAction, EventPriority.Medium },
                    { LiveEventMessageType.SeasonalTransition, EventPriority.High },
                    { LiveEventMessageType.CommunityGoalProgress, EventPriority.Medium },
                    { LiveEventMessageType.GlobalSynchronization, EventPriority.High },
                    { LiveEventMessageType.CompetitionPhaseChange, EventPriority.High },
                    { LiveEventMessageType.EducationalProgress, EventPriority.Medium },
                    { LiveEventMessageType.CrisisResponse, EventPriority.Immediate }
                };
            }
        }
        
        private void InitializeDefaultEventTypeConfigs()
        {
            if (_eventTypeConfigs.Count == 0)
            {
                _eventTypeConfigs = new List<EventTypeConfig>
                {
                    new EventTypeConfig
                    {
                        EventType = LiveEventMessageType.EventStarted,
                        MaxEventsPerSecond = 10,
                        EnableCaching = true,
                        CacheSize = 100,
                        RequiresValidation = true
                    },
                    new EventTypeConfig
                    {
                        EventType = LiveEventMessageType.CrisisResponse,
                        MaxEventsPerSecond = 1,
                        EnableCaching = false,
                        CacheSize = 0,
                        RequiresValidation = false,
                        Priority = EventPriority.Immediate
                    },
                    new EventTypeConfig
                    {
                        EventType = LiveEventMessageType.EducationalProgress,
                        MaxEventsPerSecond = 50,
                        EnableCaching = true,
                        CacheSize = 500,
                        RequiresValidation = true,
                        RequiresContentValidation = true
                    }
                };
            }
        }
        
        public EventPriority GetDefaultPriority(LiveEventMessageType eventType)
        {
            return _defaultPriorities.GetValueOrDefault(eventType, EventPriority.Medium);
        }
        
        public EventTypeConfig GetEventTypeConfig(LiveEventMessageType eventType)
        {
            return _eventTypeConfigs.Find(config => config.EventType == eventType);
        }
        
        public bool IsEventTypeAllowed(LiveEventMessageType eventType)
        {
            if (!_enableEventValidation)
                return true;
            
            var config = GetEventTypeConfig(eventType);
            return config?.IsEnabled ?? true;
        }
        
        public int GetMaxEventsPerSecond(LiveEventMessageType eventType)
        {
            var config = GetEventTypeConfig(eventType);
            return config?.MaxEventsPerSecond ?? _channelMaxEventsPerSecond;
        }
        
        public bool ShouldCacheEventType(LiveEventMessageType eventType)
        {
            if (!_enableEventCaching)
                return false;
            
            var config = GetEventTypeConfig(eventType);
            return config?.EnableCaching ?? true;
        }
        
        public bool RequiresValidation(LiveEventMessageType eventType)
        {
            if (!_enableEventValidation)
                return false;
            
            var config = GetEventTypeConfig(eventType);
            return config?.RequiresValidation ?? false;
        }
        
        public EventChannelConfigSummary GetConfigurationSummary()
        {
            return new EventChannelConfigSummary
            {
                TotalEventTypes = _eventTypeConfigs.Count,
                EnabledChannels = _enableEventChannels,
                PerformanceOptimized = _enablePerformanceOptimization,
                SecurityEnabled = _enableEventSecurity,
                AnalyticsEnabled = _enableEventAnalytics,
                CachingEnabled = _enableEventCaching,
                RateLimitingEnabled = _enableRateLimiting,
                EducationalValidationEnabled = _enableEducationalEventValidation,
                CrisisResponseEnabled = _enableCrisisResponseChannels,
                ConfigurationTime = DateTime.Now
            };
        }
    }
    
    // Supporting data structures
    public enum EventFilteringMode
    {
        None,
        Basic,
        Smart,
        Advanced,
        Custom
    }
    
    [Serializable]
    public class EventFilterRule
    {
        [SerializeField] private string _ruleName;
        [SerializeField] private LiveEventMessageType _eventType;
        [SerializeField] private EventFilterAction _action;
        [SerializeField] private List<string> _allowedTags = new List<string>();
        [SerializeField] private List<string> _blockedTags = new List<string>();
        [SerializeField] private EventPriority _minimumPriority = EventPriority.Low;
        [SerializeField] private bool _isEnabled = true;
        
        public string RuleName => _ruleName;
        public LiveEventMessageType EventType => _eventType;
        public EventFilterAction Action => _action;
        public List<string> AllowedTags => _allowedTags;
        public List<string> BlockedTags => _blockedTags;
        public EventPriority MinimumPriority => _minimumPriority;
        public bool IsEnabled => _isEnabled;
    }
    
    public enum EventFilterAction
    {
        Allow,
        Block,
        Modify,
        Delay,
        Prioritize
    }
    
    [Serializable]
    public class EventTypeConfig
    {
        [SerializeField] private LiveEventMessageType _eventType;
        [SerializeField] private bool _isEnabled = true;
        [SerializeField] private int _maxEventsPerSecond = 100;
        [SerializeField] private bool _enableCaching = true;
        [SerializeField] private int _cacheSize = 100;
        [SerializeField] private bool _requiresValidation = false;
        [SerializeField] private bool _requiresContentValidation = false;
        [SerializeField] private EventPriority _priority = EventPriority.Medium;
        [SerializeField] private float _processingTimeout = 5f;
        
        public LiveEventMessageType EventType
        {
            get => _eventType;
            set => _eventType = value;
        }
        
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }
        
        public int MaxEventsPerSecond
        {
            get => _maxEventsPerSecond;
            set => _maxEventsPerSecond = value;
        }
        
        public bool EnableCaching
        {
            get => _enableCaching;
            set => _enableCaching = value;
        }
        
        public int CacheSize
        {
            get => _cacheSize;
            set => _cacheSize = value;
        }
        
        public bool RequiresValidation
        {
            get => _requiresValidation;
            set => _requiresValidation = value;
        }
        
        public bool RequiresContentValidation
        {
            get => _requiresContentValidation;
            set => _requiresContentValidation = value;
        }
        
        public EventPriority Priority
        {
            get => _priority;
            set => _priority = value;
        }
        
        public float ProcessingTimeout
        {
            get => _processingTimeout;
            set => _processingTimeout = value;
        }
    }
    
    [Serializable]
    public class EventChannelConfigSummary
    {
        public int TotalEventTypes;
        public bool EnabledChannels;
        public bool PerformanceOptimized;
        public bool SecurityEnabled;
        public bool AnalyticsEnabled;
        public bool CachingEnabled;
        public bool RateLimitingEnabled;
        public bool EducationalValidationEnabled;
        public bool CrisisResponseEnabled;
        public DateTime ConfigurationTime;
    }
}