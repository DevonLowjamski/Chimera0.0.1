using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Global event configuration ScriptableObject for Project Chimera's worldwide live events system.
    /// Defines global coordination settings, regional configurations, and synchronization parameters
    /// for cross-region event management and conflict resolution.
    /// </summary>
    [CreateAssetMenu(fileName = "New Global Event Config", menuName = "Project Chimera/Events/Global Event Config", order = 120)]
    public class GlobalEventConfigSO : ChimeraConfigSO
    {
        [Header("Global Coordination Settings")]
        [SerializeField] private bool _enableGlobalSynchronization = true;
        [SerializeField] private float _synchronizationInterval = 60f;
        [SerializeField] private int _maxSimultaneousGlobalEvents = 5;
        [SerializeField] private bool _enableDistributedProcessing = true;
        [SerializeField] private float _globalEventTimeout = 7200f; // 2 hours
        
        [Header("Regional Management")]
        [SerializeField] private bool _enableRegionalCoordination = true;
        [SerializeField] private bool _enableRegionalAutonomy = true;
        [SerializeField] private bool _enableCrossRegionalEvents = true;
        [SerializeField] private int _maxRegionalCoordinators = 20;
        [SerializeField] private float _regionalSyncInterval = 120f;
        
        [Header("Time Zone Configuration")]
        [SerializeField] private bool _enableTimeZoneSync = true;
        [SerializeField] private bool _useUTCAsReference = true;
        [SerializeField] private float _timeZoneSyncTolerance = 300f; // 5 minutes
        [SerializeField] private List<TimeZoneConfig> _supportedTimeZones = new List<TimeZoneConfig>();
        
        [Header("Conflict Resolution")]
        [SerializeField] private ConflictResolutionMode _defaultConflictResolution = ConflictResolutionMode.Automatic;
        [SerializeField] private bool _enablePriorityBasedResolution = true;
        [SerializeField] private bool _enableCommunityVoting = false;
        [SerializeField] private float _conflictResolutionTimeout = 1800f; // 30 minutes
        [SerializeField] private int _maxActiveConflicts = 50;
        
        [Header("Performance and Scaling")]
        [SerializeField] private bool _enableEventBatching = true;
        [SerializeField] private int _maxBatchSize = 100;
        [SerializeField] private bool _enableCaching = true;
        [SerializeField] private int _globalEventCacheSize = 1000;
        [SerializeField] private float _cacheExpirationTime = 3600f; // 1 hour
        
        [Header("Network and Communication")]
        [SerializeField] private bool _enableRealTimeSync = true;
        [SerializeField] private float _networkTimeout = 30f;
        [SerializeField] private int _maxRetryAttempts = 3;
        [SerializeField] private float _retryDelay = 5f;
        [SerializeField] private bool _enableCompression = true;
        
        [Header("Security and Validation")]
        [SerializeField] private bool _enableEventValidation = true;
        [SerializeField] private bool _requireEventAuthentication = false;
        [SerializeField] private bool _enableAntiCheat = true;
        [SerializeField] private float _validationTimeout = 10f;
        [SerializeField] private List<string> _trustedRegions = new List<string>();
        
        [Header("Analytics and Monitoring")]
        [SerializeField] private bool _enableGlobalAnalytics = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private bool _enableHealthChecks = true;
        [SerializeField] private float _healthCheckInterval = 60f;
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Fallback and Recovery")]
        [SerializeField] private bool _enableAutomaticRecovery = true;
        [SerializeField] private bool _enableFallbackMode = true;
        [SerializeField] private float _recoveryTimeout = 300f; // 5 minutes
        [SerializeField] private int _maxRecoveryAttempts = 5;
        [SerializeField] private bool _enableOfflineMode = true;
        
        // Public Properties
        public bool EnableGlobalSynchronization => _enableGlobalSynchronization;
        public float SynchronizationInterval => _synchronizationInterval;
        public int MaxSimultaneousGlobalEvents => _maxSimultaneousGlobalEvents;
        public bool EnableDistributedProcessing => _enableDistributedProcessing;
        public float GlobalEventTimeout => _globalEventTimeout;
        
        public bool EnableRegionalCoordination => _enableRegionalCoordination;
        public bool EnableRegionalAutonomy => _enableRegionalAutonomy;
        public bool EnableCrossRegionalEvents => _enableCrossRegionalEvents;
        public int MaxRegionalCoordinators => _maxRegionalCoordinators;
        public float RegionalSyncInterval => _regionalSyncInterval;
        
        public bool EnableTimeZoneSync => _enableTimeZoneSync;
        public bool UseUTCAsReference => _useUTCAsReference;
        public float TimeZoneSyncTolerance => _timeZoneSyncTolerance;
        public List<TimeZoneConfig> SupportedTimeZones => _supportedTimeZones;
        
        public ConflictResolutionMode DefaultConflictResolution => _defaultConflictResolution;
        public bool EnablePriorityBasedResolution => _enablePriorityBasedResolution;
        public bool EnableCommunityVoting => _enableCommunityVoting;
        public float ConflictResolutionTimeout => _conflictResolutionTimeout;
        public int MaxActiveConflicts => _maxActiveConflicts;
        
        public bool EnableEventBatching => _enableEventBatching;
        public int MaxBatchSize => _maxBatchSize;
        public bool EnableCaching => _enableCaching;
        public int GlobalEventCacheSize => _globalEventCacheSize;
        public float CacheExpirationTime => _cacheExpirationTime;
        
        public bool EnableRealTimeSync => _enableRealTimeSync;
        public float NetworkTimeout => _networkTimeout;
        public int MaxRetryAttempts => _maxRetryAttempts;
        public float RetryDelay => _retryDelay;
        public bool EnableCompression => _enableCompression;
        
        public bool EnableEventValidation => _enableEventValidation;
        public bool RequireEventAuthentication => _requireEventAuthentication;
        public bool EnableAntiCheat => _enableAntiCheat;
        public float ValidationTimeout => _validationTimeout;
        public List<string> TrustedRegions => _trustedRegions;
        
        public bool EnableGlobalAnalytics => _enableGlobalAnalytics;
        public bool EnablePerformanceMonitoring => _enablePerformanceMonitoring;
        public bool EnableHealthChecks => _enableHealthChecks;
        public float HealthCheckInterval => _healthCheckInterval;
        public bool EnableDetailedLogging => _enableDetailedLogging;
        
        public bool EnableAutomaticRecovery => _enableAutomaticRecovery;
        public bool EnableFallbackMode => _enableFallbackMode;
        public float RecoveryTimeout => _recoveryTimeout;
        public int MaxRecoveryAttempts => _maxRecoveryAttempts;
        public bool EnableOfflineMode => _enableOfflineMode;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate timing settings
            _synchronizationInterval = Mathf.Max(1f, _synchronizationInterval);
            _regionalSyncInterval = Mathf.Max(1f, _regionalSyncInterval);
            _timeZoneSyncTolerance = Mathf.Max(1f, _timeZoneSyncTolerance);
            _conflictResolutionTimeout = Mathf.Max(1f, _conflictResolutionTimeout);
            _globalEventTimeout = Mathf.Max(1f, _globalEventTimeout);
            
            // Validate capacity settings
            _maxSimultaneousGlobalEvents = Mathf.Max(1, _maxSimultaneousGlobalEvents);
            _maxRegionalCoordinators = Mathf.Max(1, _maxRegionalCoordinators);
            _maxActiveConflicts = Mathf.Max(1, _maxActiveConflicts);
            _maxBatchSize = Mathf.Max(1, _maxBatchSize);
            _globalEventCacheSize = Mathf.Max(1, _globalEventCacheSize);
            
            // Validate network settings
            _networkTimeout = Mathf.Max(1f, _networkTimeout);
            _maxRetryAttempts = Mathf.Max(0, _maxRetryAttempts);
            _retryDelay = Mathf.Max(0.1f, _retryDelay);
            _validationTimeout = Mathf.Max(1f, _validationTimeout);
            
            // Validate recovery settings
            _recoveryTimeout = Mathf.Max(1f, _recoveryTimeout);
            _maxRecoveryAttempts = Mathf.Max(1, _maxRecoveryAttempts);
            _healthCheckInterval = Mathf.Max(1f, _healthCheckInterval);
            _cacheExpirationTime = Mathf.Max(1f, _cacheExpirationTime);
            
            // Initialize default time zones if empty
            if (_supportedTimeZones.Count == 0)
            {
                InitializeDefaultTimeZones();
            }
            
            // Initialize default trusted regions if empty
            if (_trustedRegions.Count == 0)
            {
                InitializeDefaultTrustedRegions();
            }
        }
        
        private void InitializeDefaultTimeZones()
        {
            _supportedTimeZones = new List<TimeZoneConfig>
            {
                new TimeZoneConfig { TimeZoneId = "UTC", DisplayName = "Coordinated Universal Time", OffsetHours = 0 },
                new TimeZoneConfig { TimeZoneId = "EST", DisplayName = "Eastern Standard Time", OffsetHours = -5 },
                new TimeZoneConfig { TimeZoneId = "PST", DisplayName = "Pacific Standard Time", OffsetHours = -8 },
                new TimeZoneConfig { TimeZoneId = "GMT", DisplayName = "Greenwich Mean Time", OffsetHours = 0 },
                new TimeZoneConfig { TimeZoneId = "CET", DisplayName = "Central European Time", OffsetHours = 1 },
                new TimeZoneConfig { TimeZoneId = "JST", DisplayName = "Japan Standard Time", OffsetHours = 9 },
                new TimeZoneConfig { TimeZoneId = "AEST", DisplayName = "Australian Eastern Standard Time", OffsetHours = 10 }
            };
        }
        
        private void InitializeDefaultTrustedRegions()
        {
            _trustedRegions = new List<string>
            {
                "north_america",
                "europe",
                "asia_pacific",
                "south_america",
                "africa",
                "oceania"
            };
        }
        
        public TimeZoneConfig GetTimeZoneConfig(string timeZoneId)
        {
            return _supportedTimeZones.Find(tz => tz.TimeZoneId == timeZoneId);
        }
        
        public bool IsRegionTrusted(string regionId)
        {
            return _trustedRegions.Contains(regionId);
        }
        
        public GlobalEventLimits GetEventLimits()
        {
            return new GlobalEventLimits
            {
                MaxSimultaneousEvents = _maxSimultaneousGlobalEvents,
                MaxActiveConflicts = _maxActiveConflicts,
                MaxRegionalCoordinators = _maxRegionalCoordinators,
                MaxBatchSize = _maxBatchSize,
                CacheSize = _globalEventCacheSize
            };
        }
        
        public NetworkConfiguration GetNetworkConfiguration()
        {
            return new NetworkConfiguration
            {
                EnableRealTimeSync = _enableRealTimeSync,
                NetworkTimeout = _networkTimeout,
                MaxRetryAttempts = _maxRetryAttempts,
                RetryDelay = _retryDelay,
                EnableCompression = _enableCompression,
                EnableValidation = _enableEventValidation,
                ValidationTimeout = _validationTimeout
            };
        }
        
        public PerformanceConfiguration GetPerformanceConfiguration()
        {
            return new PerformanceConfiguration
            {
                EnableBatching = _enableEventBatching,
                EnableCaching = _enableCaching,
                EnableDistributedProcessing = _enableDistributedProcessing,
                CacheExpirationTime = _cacheExpirationTime,
                HealthCheckInterval = _healthCheckInterval,
                EnablePerformanceMonitoring = _enablePerformanceMonitoring
            };
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class TimeZoneConfig
    {
        public string TimeZoneId;
        public string DisplayName;
        public int OffsetHours;
        public bool SupportsDaylightSaving = false;
        public DateTime DaylightSavingStart;
        public DateTime DaylightSavingEnd;
    }
    
    [Serializable]
    public class GlobalEventLimits
    {
        public int MaxSimultaneousEvents;
        public int MaxActiveConflicts;
        public int MaxRegionalCoordinators;
        public int MaxBatchSize;
        public int CacheSize;
    }
    
    [Serializable]
    public class NetworkConfiguration
    {
        public bool EnableRealTimeSync;
        public float NetworkTimeout;
        public int MaxRetryAttempts;
        public float RetryDelay;
        public bool EnableCompression;
        public bool EnableValidation;
        public float ValidationTimeout;
    }
    
    [Serializable]
    public class PerformanceConfiguration
    {
        public bool EnableBatching;
        public bool EnableCaching;
        public bool EnableDistributedProcessing;
        public float CacheExpirationTime;
        public float HealthCheckInterval;
        public bool EnablePerformanceMonitoring;
    }
    
    public enum ConflictResolutionMode
    {
        Automatic,
        Manual,
        Community,
        Hybrid
    }
}