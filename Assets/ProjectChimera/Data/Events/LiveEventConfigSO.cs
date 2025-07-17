using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Comprehensive live events configuration ScriptableObject for Project Chimera's event system.
    /// Defines all parameters for dynamic time-based content including global competitions,
    /// seasonal celebrations, community challenges, and cultural events with real-time coordination.
    /// </summary>
    [CreateAssetMenu(fileName = "New Live Event Config", menuName = "Project Chimera/Events/Live Event Config", order = 100)]
    public class LiveEventConfigSO : ChimeraConfigSO
    {
        [Header("Core Event Configuration")]
        [SerializeField] private bool _enableLiveEvents = true;
        [SerializeField] private bool _enableSeasonalContent = true;
        [SerializeField] private bool _enableCommunityEvents = true;
        [SerializeField] private bool _enableGlobalChallenges = true;
        [SerializeField] private bool _enableCulturalEvents = true;
        
        [Header("Timing Systems")]
        [SerializeField] private float _eventCheckInterval = 300f; // 5 minutes
        [SerializeField] private TimeZoneHandling _serverTimeZone = TimeZoneHandling.UTC;
        [SerializeField] private bool _useLocalTime = false;
        [SerializeField] private bool _enableEventNotifications = true;
        [SerializeField] private float _notificationAdvanceTime = 3600f; // 1 hour
        
        [Header("Event Management")]
        [SerializeField] private int _maxActiveEvents = 10;
        [SerializeField] private int _maxUpcomingEvents = 50;
        [SerializeField] private float _eventCleanupInterval = 86400f; // 24 hours
        [SerializeField] private bool _enableEventHistoryTracking = true;
        [SerializeField] private int _maxEventHistoryEntries = 1000;
        
        [Header("Participation Settings")]
        [SerializeField] private ParticipationMode _participationMode = ParticipationMode.OptIn;
        [SerializeField] private bool _allowCrossRegionParticipation = true;
        [SerializeField] private float _participationCooldownTime = 300f; // 5 minutes
        [SerializeField] private bool _enableParticipationLimits = false;
        [SerializeField] private int _maxDailyEventParticipation = 10;
        
        [Header("Global Coordination")]
        [SerializeField] private bool _enableGlobalSynchronization = true;
        [SerializeField] private float _synchronizationInterval = 60f; // 1 minute
        [SerializeField] private int _maxGlobalParticipants = 1000000;
        [SerializeField] private bool _enableLoadBalancing = true;
        [SerializeField] private float _serverCapacityThreshold = 0.8f;
        
        [Header("Community Features")]
        [SerializeField] private bool _enableCommunityGoals = true;
        [SerializeField] private bool _enableCollaborativeEvents = true;
        [SerializeField] private bool _enablePlayerGrouping = true;
        [SerializeField] private int _maxPlayersPerGroup = 50;
        [SerializeField] private bool _enableCommunityVoting = true;
        
        [Header("Seasonal Content")]
        [SerializeField] private bool _enableAutomaticSeasonalTransitions = true;
        [SerializeField] private bool _enableCulturalCalendarIntegration = true;
        [SerializeField] private bool _enableRegionalVariations = true;
        [SerializeField] private bool _enableWeatherIntegration = true;
        [SerializeField] private float _seasonalContentUpdateInterval = 3600f; // 1 hour
        
        [Header("Reward Systems")]
        [SerializeField] private bool _enableTieredRewards = true;
        [SerializeField] private bool _enableParticipationRewards = true;
        [SerializeField] private bool _enableExclusiveContent = true;
        [SerializeField] private bool _enableLimitedTimeUnlocks = true;
        [SerializeField] private float _rewardDistributionDelay = 300f; // 5 minutes
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalContent = true;
        [SerializeField] private bool _enableCulturalEducation = true;
        [SerializeField] private bool _enableHistoricalContext = true;
        [SerializeField] private float _educationalContentRatio = 0.6f;
        [SerializeField] private bool _requireScientificAccuracy = true;
        
        [Header("Analytics and Monitoring")]
        [SerializeField] private bool _enableEventAnalytics = true;
        [SerializeField] private bool _enableParticipationTracking = true;
        [SerializeField] private bool _enableCommunityHealthMonitoring = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _analyticsUpdateInterval = 300f; // 5 minutes
        
        [Header("Crisis Response")]
        [SerializeField] private bool _enableCrisisResponseEvents = true;
        [SerializeField] private bool _enableEmergencyBroadcasts = true;
        [SerializeField] private float _crisisResponseTime = 900f; // 15 minutes
        [SerializeField] private bool _enableCommunityMobilization = true;
        
        [Header("Performance Optimization")]
        [SerializeField] private bool _enableEventCaching = true;
        [SerializeField] private bool _enableContentPreloading = true;
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _maxEventsPerUpdate = 5;
        [SerializeField] private bool _enableMemoryOptimization = true;
        
        [Header("Database References")]
        [SerializeField] private EventDatabaseSO _eventDatabase;
        [SerializeField] private SeasonalContentLibrarySO _seasonalContentLibrary;
        [SerializeField] private CulturalEventLibrarySO _culturalEventLibrary;
        [SerializeField] private CommunityChallengeDatabaseSO _communityDatabase;
        [SerializeField] private GlobalEventTemplateSO _globalEventTemplates;
        
        // Properties
        public bool EnableLiveEvents => _enableLiveEvents;
        public bool EnableSeasonalContent => _enableSeasonalContent;
        public bool EnableCommunityEvents => _enableCommunityEvents;
        public bool EnableGlobalChallenges => _enableGlobalChallenges;
        public bool EnableCulturalEvents => _enableCulturalEvents;
        
        public float EventCheckInterval => _eventCheckInterval;
        public TimeZoneHandling ServerTimeZone => _serverTimeZone;
        public bool UseLocalTime => _useLocalTime;
        public bool EnableEventNotifications => _enableEventNotifications;
        public float NotificationAdvanceTime => _notificationAdvanceTime;
        
        public int MaxActiveEvents => _maxActiveEvents;
        public int MaxUpcomingEvents => _maxUpcomingEvents;
        public float EventCleanupInterval => _eventCleanupInterval;
        public bool EnableEventHistoryTracking => _enableEventHistoryTracking;
        public int MaxEventHistoryEntries => _maxEventHistoryEntries;
        
        public ParticipationMode ParticipationMode => _participationMode;
        public bool AllowCrossRegionParticipation => _allowCrossRegionParticipation;
        public float ParticipationCooldownTime => _participationCooldownTime;
        public bool EnableParticipationLimits => _enableParticipationLimits;
        public int MaxDailyEventParticipation => _maxDailyEventParticipation;
        
        public bool EnableGlobalSynchronization => _enableGlobalSynchronization;
        public float SynchronizationInterval => _synchronizationInterval;
        public float GlobalSynchronizationInterval => _synchronizationInterval; // Alias for compatibility
        public int MaxGlobalParticipants => _maxGlobalParticipants;
        public bool EnableLoadBalancing => _enableLoadBalancing;
        public float ServerCapacityThreshold => _serverCapacityThreshold;
        
        public bool EnableCommunityGoals => _enableCommunityGoals;
        public bool EnableCollaborativeEvents => _enableCollaborativeEvents;
        public bool EnablePlayerGrouping => _enablePlayerGrouping;
        public int MaxPlayersPerGroup => _maxPlayersPerGroup;
        public bool EnableCommunityVoting => _enableCommunityVoting;
        
        public bool EnableAutomaticSeasonalTransitions => _enableAutomaticSeasonalTransitions;
        public bool EnableCulturalCalendarIntegration => _enableCulturalCalendarIntegration;
        public bool EnableRegionalVariations => _enableRegionalVariations;
        public bool EnableWeatherIntegration => _enableWeatherIntegration;
        public float SeasonalContentUpdateInterval => _seasonalContentUpdateInterval;
        
        public bool EnableTieredRewards => _enableTieredRewards;
        public bool EnableParticipationRewards => _enableParticipationRewards;
        public bool EnableExclusiveContent => _enableExclusiveContent;
        public bool EnableLimitedTimeUnlocks => _enableLimitedTimeUnlocks;
        public float RewardDistributionDelay => _rewardDistributionDelay;
        
        public bool EnableEducationalContent => _enableEducationalContent;
        public bool EnableCulturalEducation => _enableCulturalEducation;
        public bool EnableHistoricalContext => _enableHistoricalContext;
        public float EducationalContentRatio => _educationalContentRatio;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        
        public bool EnableEventAnalytics => _enableEventAnalytics;
        public bool EnableParticipationTracking => _enableParticipationTracking;
        public bool EnableCommunityHealthMonitoring => _enableCommunityHealthMonitoring;
        public bool EnablePerformanceMonitoring => _enablePerformanceMonitoring;
        public float AnalyticsUpdateInterval => _analyticsUpdateInterval;
        
        public bool EnableCrisisResponseEvents => _enableCrisisResponseEvents;
        public bool EnableEmergencyBroadcasts => _enableEmergencyBroadcasts;
        public float CrisisResponseTime => _crisisResponseTime;
        public bool EnableCommunityMobilization => _enableCommunityMobilization;
        
        public bool EnableEventCaching => _enableEventCaching;
        public bool EnableContentPreloading => _enableContentPreloading;
        public bool EnableBatchProcessing => _enableBatchProcessing;
        public int MaxEventsPerUpdate => _maxEventsPerUpdate;
        public bool EnableMemoryOptimization => _enableMemoryOptimization;
        
        public EventDatabaseSO EventDatabase => _eventDatabase;
        public SeasonalContentLibrarySO SeasonalContentLibrary => _seasonalContentLibrary;
        public CulturalEventLibrarySO CulturalEventLibrary => _culturalEventLibrary;
        public CommunityChallengeDatabaseSO CommunityDatabase => _communityDatabase;
        public GlobalEventTemplateSO GlobalEventTemplates => _globalEventTemplates;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate timing parameters
            _eventCheckInterval = Mathf.Max(60f, _eventCheckInterval); // Minimum 1 minute
            _notificationAdvanceTime = Mathf.Max(300f, _notificationAdvanceTime); // Minimum 5 minutes
            _eventCleanupInterval = Mathf.Max(3600f, _eventCleanupInterval); // Minimum 1 hour
            
            // Validate participation limits
            _maxActiveEvents = Mathf.Max(1, _maxActiveEvents);
            _maxUpcomingEvents = Mathf.Max(10, _maxUpcomingEvents);
            _maxEventHistoryEntries = Mathf.Max(100, _maxEventHistoryEntries);
            
            // Validate performance parameters
            _synchronizationInterval = Mathf.Max(30f, _synchronizationInterval); // Minimum 30 seconds
            _maxGlobalParticipants = Mathf.Max(1000, _maxGlobalParticipants);
            _serverCapacityThreshold = Mathf.Clamp01(_serverCapacityThreshold);
            
            // Validate community settings
            _maxPlayersPerGroup = Mathf.Max(5, _maxPlayersPerGroup);
            _maxDailyEventParticipation = Mathf.Max(1, _maxDailyEventParticipation);
            
            // Validate content parameters
            _educationalContentRatio = Mathf.Clamp01(_educationalContentRatio);
            _seasonalContentUpdateInterval = Mathf.Max(300f, _seasonalContentUpdateInterval);
            
            // Validate optimization settings
            _maxEventsPerUpdate = Mathf.Max(1, _maxEventsPerUpdate);
            _analyticsUpdateInterval = Mathf.Max(60f, _analyticsUpdateInterval);
            
            // Log validation results
            if (Application.isPlaying)
            {
                Debug.Log($"[LiveEventConfigSO] Validated configuration with {_maxActiveEvents} max active events and {_maxGlobalParticipants} max participants");
            }
        }
        
        public bool IsValidConfiguration()
        {
            var errors = new List<string>();
            
            // Check database references
            if (_eventDatabase == null) errors.Add("Event Database is not assigned");
            if (_seasonalContentLibrary == null) errors.Add("Seasonal Content Library is not assigned");
            if (_culturalEventLibrary == null) errors.Add("Cultural Event Library is not assigned");
            if (_communityDatabase == null) errors.Add("Community Database is not assigned");
            if (_globalEventTemplates == null) errors.Add("Global Event Templates is not assigned");
            
            // Check feature consistency
            if (_enableCommunityEvents && !_enableCommunityGoals)
            {
                Debug.LogWarning("[LiveEventConfigSO] Community events enabled but community goals disabled - may limit functionality");
            }
            
            if (_enableGlobalChallenges && !_enableGlobalSynchronization)
            {
                errors.Add("Global challenges require global synchronization to be enabled");
            }
            
            if (_enableEventAnalytics && !_enableParticipationTracking)
            {
                Debug.LogWarning("[LiveEventConfigSO] Event analytics enabled but participation tracking disabled - analytics will be limited");
            }
            
            if (errors.Count > 0)
            {
                Debug.LogError($"[LiveEventConfigSO] Configuration validation failed: {string.Join(", ", errors)}");
                return false;
            }
            
            return true;
        }
        
        public void LogConfigurationSummary()
        {
            var summary = $@"
Live Events Configuration Summary:
- Events Enabled: {_enableLiveEvents}
- Seasonal Content: {_enableSeasonalContent}
- Community Events: {_enableCommunityEvents}
- Global Challenges: {_enableGlobalChallenges}
- Cultural Events: {_enableCulturalEvents}
- Max Active Events: {_maxActiveEvents}
- Max Global Participants: {_maxGlobalParticipants}
- Educational Content Ratio: {_educationalContentRatio:P0}
- Analytics Enabled: {_enableEventAnalytics}
- Crisis Response: {_enableCrisisResponseEvents}";
            
            Debug.Log($"[LiveEventConfigSO] {summary}");
        }
    }
    
    // Supporting enums and structures
    public enum TimeZoneHandling
    {
        UTC,
        ServerLocal,
        PlayerLocal,
        Automatic
    }
    
    public enum ParticipationMode
    {
        OptIn,
        OptOut,
        Automatic,
        InviteOnly
    }
    
    [Serializable]
    public class EventValidationSettings
    {
        public bool ValidateEventTiming = true;
        public bool ValidateParticipationRequirements = true;
        public bool ValidateRewardSettings = true;
        public bool ValidateEducationalContent = true;
        public bool ValidateCulturalSensitivity = true;
    }
    
    [Serializable]
    public class CommunityModerationSettings
    {
        public bool EnableContentModeration = true;
        public bool EnableToxicityDetection = true;
        public bool EnableSpamPrevention = true;
        public bool EnableCulturalSensitivityCheck = true;
        public float ModerationResponseTime = 300f; // 5 minutes
    }
    
    [Serializable]
    public class PerformanceOptimizationSettings
    {
        public bool EnableEventBatching = true;
        public bool EnableParticipantBatching = true;
        public bool EnableContentStreaming = true;
        public bool EnableDynamicLoadBalancing = true;
        public int BatchSize = 100;
        public float OptimizationInterval = 60f;
    }
}