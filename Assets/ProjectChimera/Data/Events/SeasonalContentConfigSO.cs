using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Configuration ScriptableObject for Project Chimera's seasonal content management system.
    /// Provides comprehensive settings for seasonal transitions, content delivery, cultural events,
    /// and environmental integration for the seasonal content system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Seasonal Content Config", menuName = "Project Chimera/Events/Seasonal Content Config", order = 151)]
    public class SeasonalContentConfigSO : ChimeraConfigSO
    {
        [Header("Core Seasonal Configuration")]
        [SerializeField] private bool _enableSeasonalContent = true;
        [SerializeField] private bool _enableAutomaticTransitions = true;
        [SerializeField] private bool _enableRegionalVariations = true;
        [SerializeField] private bool _enableRealWorldSync = false;
        [SerializeField] private SeasonalTransitionMode _transitionMode = SeasonalTransitionMode.Automatic;
        
        [Header("Transition Settings")]
        [SerializeField] private float _transitionDuration = 3600f; // 1 hour
        [SerializeField] private bool _enableGradualTransitions = true;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool _enableTransitionEffects = true;
        [SerializeField] private float _preTransitionWarningTime = 1800f; // 30 minutes
        
        [Header("Content Management")]
        [SerializeField] private bool _enableContentPreloading = true;
        [SerializeField] private int _preloadDaysAhead = 7;
        [SerializeField] private bool _enableContentCaching = true;
        [SerializeField] private int _maxCachedSeasons = 4;
        [SerializeField] private bool _enableAsyncLoading = true;
        [SerializeField] private int _maxConcurrentLoads = 5;
        
        [Header("Cultural Events Configuration")]
        [SerializeField] private bool _enableCulturalEvents = true;
        [SerializeField] private bool _enableHolidayIntegration = true;
        [SerializeField] private bool _enableHistoricalEvents = true;
        [SerializeField] private bool _enableCulturalSensitivity = true;
        [SerializeField] private float _culturalEventDuration = 86400f; // 24 hours
        [SerializeField] private bool _enableMultiCulturalSupport = true;
        
        [Header("Environmental Integration")]
        [SerializeField] private bool _enableEnvironmentalSync = true;
        [SerializeField] private bool _enableWeatherIntegration = true;
        [SerializeField] private bool _enableLightingChanges = true;
        [SerializeField] private bool _enableTemperatureSync = true;
        [SerializeField] private bool _enableClimateModifiers = true;
        [SerializeField] private float _environmentalUpdateInterval = 300f; // 5 minutes
        
        [Header("Performance Optimization")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private float _updateInterval = 300f; // 5 minutes
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _maxBatchSize = 100;
        [SerializeField] private bool _enableMemoryOptimization = true;
        [SerializeField] private float _memoryCleanupInterval = 1800f; // 30 minutes
        
        [Header("Regional Configuration")]
        [SerializeField] private List<RegionalConfig> _regionalConfigs = new List<RegionalConfig>();
        [SerializeField] private string _defaultRegion = "northern_hemisphere";
        [SerializeField] private bool _enableAutomaticRegionDetection = false;
        [SerializeField] private bool _enableRegionalCustomization = true;
        
        [Header("Seasonal Event Types")]
        [SerializeField] private List<SeasonalEventTypeConfig> _seasonalEventTypes = new List<SeasonalEventTypeConfig>();
        [SerializeField] private bool _enableCustomEventTypes = true;
        [SerializeField] private float _eventActivationThreshold = 0.5f;
        [SerializeField] private int _maxConcurrentEvents = 10;
        
        [Header("Content Delivery")]
        [SerializeField] private ContentDeliveryMode _deliveryMode = ContentDeliveryMode.Progressive;
        [SerializeField] private bool _enablePriorityDelivery = true;
        [SerializeField] private float _deliveryBatchInterval = 60f; // 1 minute
        [SerializeField] private int _maxDeliveryRetries = 3;
        [SerializeField] private float _deliveryTimeoutSeconds = 30f;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalContent = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private float _educationalContentRatio = 0.3f;
        [SerializeField] private bool _enableSeasonalLearning = true;
        [SerializeField] private bool _enableCulturalEducation = true;
        
        [Header("Analytics and Monitoring")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _enablePerformanceMetrics = true;
        [SerializeField] private float _metricsUpdateInterval = 60f; // 1 minute
        [SerializeField] private int _maxMetricsHistory = 1000;
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Weather System Integration")]
        [SerializeField] private bool _enableRealWeatherAPI = false;
        [SerializeField] private string _weatherAPIKey = "";
        [SerializeField] private float _weatherUpdateInterval = 3600f; // 1 hour
        [SerializeField] private bool _enableWeatherEffects = true;
        [SerializeField] private float _weatherInfluenceStrength = 1.0f;
        
        // Properties for easy access
        public bool EnableSeasonalContent => _enableSeasonalContent;
        public bool EnableAutomaticTransitions => _enableAutomaticTransitions;
        public bool EnableRegionalVariations => _enableRegionalVariations;
        public bool EnableRealWorldSync => _enableRealWorldSync;
        public SeasonalTransitionMode TransitionMode => _transitionMode;
        
        public float TransitionDuration => _transitionDuration;
        public bool EnableGradualTransitions => _enableGradualTransitions;
        public AnimationCurve TransitionCurve => _transitionCurve;
        public bool EnableTransitionEffects => _enableTransitionEffects;
        public float PreTransitionWarningTime => _preTransitionWarningTime;
        
        public bool EnableContentPreloading => _enableContentPreloading;
        public int PreloadDaysAhead => _preloadDaysAhead;
        public bool EnableContentCaching => _enableContentCaching;
        public int MaxCachedSeasons => _maxCachedSeasons;
        public bool EnableAsyncLoading => _enableAsyncLoading;
        public int MaxConcurrentLoads => _maxConcurrentLoads;
        
        public bool EnableCulturalEvents => _enableCulturalEvents;
        public bool EnableHolidayIntegration => _enableHolidayIntegration;
        public bool EnableHistoricalEvents => _enableHistoricalEvents;
        public bool EnableCulturalSensitivity => _enableCulturalSensitivity;
        public float CulturalEventDuration => _culturalEventDuration;
        public bool EnableMultiCulturalSupport => _enableMultiCulturalSupport;
        
        public bool EnableEnvironmentalSync => _enableEnvironmentalSync;
        public bool EnableWeatherIntegration => _enableWeatherIntegration;
        public bool EnableLightingChanges => _enableLightingChanges;
        public bool EnableTemperatureSync => _enableTemperatureSync;
        public bool EnableClimateModifiers => _enableClimateModifiers;
        public float EnvironmentalUpdateInterval => _environmentalUpdateInterval;
        
        public bool EnablePerformanceOptimization => _enablePerformanceOptimization;
        public float UpdateInterval => _updateInterval;
        public bool EnableBatchProcessing => _enableBatchProcessing;
        public int MaxBatchSize => _maxBatchSize;
        public bool EnableMemoryOptimization => _enableMemoryOptimization;
        public float MemoryCleanupInterval => _memoryCleanupInterval;
        
        public List<RegionalConfig> RegionalConfigs => _regionalConfigs;
        public string DefaultRegion => _defaultRegion;
        public bool EnableAutomaticRegionDetection => _enableAutomaticRegionDetection;
        public bool EnableRegionalCustomization => _enableRegionalCustomization;
        
        public List<SeasonalEventTypeConfig> SeasonalEventTypes => _seasonalEventTypes;
        public bool EnableCustomEventTypes => _enableCustomEventTypes;
        public float EventActivationThreshold => _eventActivationThreshold;
        public int MaxConcurrentEvents => _maxConcurrentEvents;
        
        public ContentDeliveryMode DeliveryMode => _deliveryMode;
        public bool EnablePriorityDelivery => _enablePriorityDelivery;
        public float DeliveryBatchInterval => _deliveryBatchInterval;
        public int MaxDeliveryRetries => _maxDeliveryRetries;
        public float DeliveryTimeoutSeconds => _deliveryTimeoutSeconds;
        
        public bool EnableEducationalContent => _enableEducationalContent;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        public float EducationalContentRatio => _educationalContentRatio;
        public bool EnableSeasonalLearning => _enableSeasonalLearning;
        public bool EnableCulturalEducation => _enableCulturalEducation;
        
        public bool EnableAnalytics => _enableAnalytics;
        public bool EnablePerformanceMetrics => _enablePerformanceMetrics;
        public float MetricsUpdateInterval => _metricsUpdateInterval;
        public int MaxMetricsHistory => _maxMetricsHistory;
        public bool EnableDetailedLogging => _enableDetailedLogging;
        
        public bool EnableRealWeatherAPI => _enableRealWeatherAPI;
        public string WeatherAPIKey => _weatherAPIKey;
        public float WeatherUpdateInterval => _weatherUpdateInterval;
        public bool EnableWeatherEffects => _enableWeatherEffects;
        public float WeatherInfluenceStrength => _weatherInfluenceStrength;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate transition settings
            _transitionDuration = Mathf.Max(0f, _transitionDuration);
            _preTransitionWarningTime = Mathf.Max(0f, _preTransitionWarningTime);
            
            // Validate content management settings
            _preloadDaysAhead = Mathf.Max(0, _preloadDaysAhead);
            _maxCachedSeasons = Mathf.Clamp(_maxCachedSeasons, 1, 4);
            _maxConcurrentLoads = Mathf.Max(1, _maxConcurrentLoads);
            
            // Validate cultural event settings
            _culturalEventDuration = Mathf.Max(0f, _culturalEventDuration);
            
            // Validate environmental settings
            _environmentalUpdateInterval = Mathf.Max(1f, _environmentalUpdateInterval);
            
            // Validate performance settings
            _updateInterval = Mathf.Max(1f, _updateInterval);
            _maxBatchSize = Mathf.Max(1, _maxBatchSize);
            _memoryCleanupInterval = Mathf.Max(1f, _memoryCleanupInterval);
            
            // Validate event settings
            _eventActivationThreshold = Mathf.Clamp01(_eventActivationThreshold);
            _maxConcurrentEvents = Mathf.Max(1, _maxConcurrentEvents);
            
            // Validate delivery settings
            _deliveryBatchInterval = Mathf.Max(1f, _deliveryBatchInterval);
            _maxDeliveryRetries = Mathf.Max(0, _maxDeliveryRetries);
            _deliveryTimeoutSeconds = Mathf.Max(1f, _deliveryTimeoutSeconds);
            
            // Validate educational settings
            _educationalContentRatio = Mathf.Clamp01(_educationalContentRatio);
            
            // Validate analytics settings
            _metricsUpdateInterval = Mathf.Max(1f, _metricsUpdateInterval);
            _maxMetricsHistory = Mathf.Max(1, _maxMetricsHistory);
            
            // Validate weather settings
            _weatherUpdateInterval = Mathf.Max(60f, _weatherUpdateInterval);
            _weatherInfluenceStrength = Mathf.Max(0f, _weatherInfluenceStrength);
            
            // Initialize default configurations if empty
            InitializeDefaultConfigurations();
        }
        
        private void InitializeDefaultConfigurations()
        {
            // Initialize default regional configs if empty
            if (_regionalConfigs.Count == 0)
            {
                _regionalConfigs = new List<RegionalConfig>
                {
                    new RegionalConfig
                    {
                        RegionId = "northern_hemisphere",
                        RegionName = "Northern Hemisphere",
                        Timezone = "UTC",
                        SeasonOffset = 0,
                        EnabledFeatures = RegionalFeatures.All
                    },
                    new RegionalConfig
                    {
                        RegionId = "southern_hemisphere",
                        RegionName = "Southern Hemisphere",
                        Timezone = "UTC",
                        SeasonOffset = 6, // 6 months offset
                        EnabledFeatures = RegionalFeatures.All
                    }
                };
            }
            
            // Initialize default seasonal event types if empty
            if (_seasonalEventTypes.Count == 0)
            {
                _seasonalEventTypes = new List<SeasonalEventTypeConfig>
                {
                    new SeasonalEventTypeConfig
                    {
                        EventType = SeasonalEventType.Seasonal,
                        IsEnabled = true,
                        MaxDuration = 7776000f, // 90 days
                        Priority = EventPriority.Medium,
                        RequiresPreloading = true
                    },
                    new SeasonalEventTypeConfig
                    {
                        EventType = SeasonalEventType.Cultural,
                        IsEnabled = true,
                        MaxDuration = 86400f, // 1 day
                        Priority = EventPriority.High,
                        RequiresPreloading = false
                    },
                    new SeasonalEventTypeConfig
                    {
                        EventType = SeasonalEventType.Holiday,
                        IsEnabled = true,
                        MaxDuration = 86400f, // 1 day
                        Priority = EventPriority.High,
                        RequiresPreloading = false
                    },
                    new SeasonalEventTypeConfig
                    {
                        EventType = SeasonalEventType.Weather,
                        IsEnabled = true,
                        MaxDuration = 3600f, // 1 hour
                        Priority = EventPriority.Low,
                        RequiresPreloading = false
                    }
                };
            }
        }
        
        public RegionalConfig GetRegionalConfig(string regionId)
        {
            return _regionalConfigs.Find(config => config.RegionId == regionId);
        }
        
        public SeasonalEventTypeConfig GetEventTypeConfig(SeasonalEventType eventType)
        {
            return _seasonalEventTypes.Find(config => config.EventType == eventType);
        }
        
        public bool IsEventTypeEnabled(SeasonalEventType eventType)
        {
            var config = GetEventTypeConfig(eventType);
            return config?.IsEnabled ?? false;
        }
        
        public float GetEventMaxDuration(SeasonalEventType eventType)
        {
            var config = GetEventTypeConfig(eventType);
            return config?.MaxDuration ?? _culturalEventDuration;
        }
        
        public bool RequiresPreloading(SeasonalEventType eventType)
        {
            var config = GetEventTypeConfig(eventType);
            return config?.RequiresPreloading ?? false;
        }
        
        public List<string> GetAvailableRegions()
        {
            return _regionalConfigs.ConvertAll(config => config.RegionId);
        }
        
        public SeasonalContentConfigSummary GetConfigurationSummary()
        {
            return new SeasonalContentConfigSummary
            {
                SeasonalContentEnabled = _enableSeasonalContent,
                AutomaticTransitionsEnabled = _enableAutomaticTransitions,
                RegionalVariationsEnabled = _enableRegionalVariations,
                CulturalEventsEnabled = _enableCulturalEvents,
                EnvironmentalSyncEnabled = _enableEnvironmentalSync,
                PerformanceOptimizationEnabled = _enablePerformanceOptimization,
                TotalRegions = _regionalConfigs.Count,
                TotalEventTypes = _seasonalEventTypes.Count,
                ConfigurationTime = DateTime.Now
            };
        }
    }
    
    // Supporting enums and data structures
    public enum SeasonalTransitionMode
    {
        Automatic,
        Manual,
        RealWorld,
        Custom
    }
    
    public enum ContentDeliveryMode
    {
        Immediate,
        Progressive,
        Scheduled,
        OnDemand
    }
    
    [Flags]
    public enum RegionalFeatures
    {
        None = 0,
        SeasonalTransitions = 1 << 0,
        CulturalEvents = 1 << 1,
        WeatherIntegration = 1 << 2,
        EnvironmentalSync = 1 << 3,
        LocalizedContent = 1 << 4,
        All = SeasonalTransitions | CulturalEvents | WeatherIntegration | EnvironmentalSync | LocalizedContent
    }
    
    [Serializable]
    public class RegionalConfig
    {
        [SerializeField] private string _regionId;
        [SerializeField] private string _regionName;
        [SerializeField] private string _timezone;
        [SerializeField] private int _seasonOffset; // Months offset from northern hemisphere
        [SerializeField] private RegionalFeatures _enabledFeatures = RegionalFeatures.All;
        [SerializeField] private bool _useCustomSeasonDates = false;
        [SerializeField] private List<CustomSeasonDate> _customSeasonDates = new List<CustomSeasonDate>();
        
        public string RegionId
        {
            get => _regionId;
            set => _regionId = value;
        }
        
        public string RegionName
        {
            get => _regionName;
            set => _regionName = value;
        }
        
        public string Timezone
        {
            get => _timezone;
            set => _timezone = value;
        }
        
        public int SeasonOffset
        {
            get => _seasonOffset;
            set => _seasonOffset = value;
        }
        
        public RegionalFeatures EnabledFeatures
        {
            get => _enabledFeatures;
            set => _enabledFeatures = value;
        }
        
        public bool UseCustomSeasonDates
        {
            get => _useCustomSeasonDates;
            set => _useCustomSeasonDates = value;
        }
        
        public List<CustomSeasonDate> CustomSeasonDates
        {
            get => _customSeasonDates;
            set => _customSeasonDates = value;
        }
    }
    
    [Serializable]
    public class CustomSeasonDate
    {
        [SerializeField] private Season _season;
        [SerializeField] private int _month;
        [SerializeField] private int _day;
        
        public Season Season
        {
            get => _season;
            set => _season = value;
        }
        
        public int Month
        {
            get => _month;
            set => _month = value;
        }
        
        public int Day
        {
            get => _day;
            set => _day = value;
        }
    }
    
    [Serializable]
    public class SeasonalEventTypeConfig
    {
        [SerializeField] private SeasonalEventType _eventType;
        [SerializeField] private bool _isEnabled = true;
        [SerializeField] private float _maxDuration = 86400f; // 24 hours
        [SerializeField] private EventPriority _priority = EventPriority.Medium;
        [SerializeField] private bool _requiresPreloading = false;
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private int _maxConcurrentInstances = 1;
        
        public SeasonalEventType EventType
        {
            get => _eventType;
            set => _eventType = value;
        }
        
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }
        
        public float MaxDuration
        {
            get => _maxDuration;
            set => _maxDuration = value;
        }
        
        public EventPriority Priority
        {
            get => _priority;
            set => _priority = value;
        }
        
        public bool RequiresPreloading
        {
            get => _requiresPreloading;
            set => _requiresPreloading = value;
        }
        
        public bool EnableAnalytics
        {
            get => _enableAnalytics;
            set => _enableAnalytics = value;
        }
        
        public int MaxConcurrentInstances
        {
            get => _maxConcurrentInstances;
            set => _maxConcurrentInstances = value;
        }
    }
    
    [Serializable]
    public class SeasonalContentConfigSummary
    {
        public bool SeasonalContentEnabled;
        public bool AutomaticTransitionsEnabled;
        public bool RegionalVariationsEnabled;
        public bool CulturalEventsEnabled;
        public bool EnvironmentalSyncEnabled;
        public bool PerformanceOptimizationEnabled;
        public int TotalRegions;
        public int TotalEventTypes;
        public DateTime ConfigurationTime;
    }
}