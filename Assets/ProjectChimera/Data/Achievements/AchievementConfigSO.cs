using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Achievements
{
    /// <summary>
    /// Core configuration ScriptableObject for Project Chimera's Achievement & Milestone system.
    /// Defines system-wide settings, performance parameters, and integration configurations
    /// for the most comprehensive achievement system ever designed for a cultivation simulation.
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement Config", menuName = "Project Chimera/Achievements/Achievement Config", order = 100)]
    public class AchievementConfigSO : ChimeraConfigSO
    {
        [Header("System Configuration")]
        [SerializeField] private bool _enableAchievements = true;
        [SerializeField] private bool _enableMilestones = true;
        [SerializeField] private bool _enableHiddenAchievements = true;
        [SerializeField] private bool _enableCommunityAchievements = true;
        [SerializeField] private bool _enableSeasonalAchievements = true;
        [SerializeField] private bool _enableCulturalCelebrations = true;
        
        [Header("Progress Tracking Settings")]
        [SerializeField] private float _progressUpdateInterval = 1f;
        [SerializeField] private int _maxRecentAchievements = 10;
        [SerializeField] private bool _enableRealTimeTracking = true;
        [SerializeField] private bool _enableAchievementHints = true;
        [SerializeField] private bool _enableProgressPrediction = true;
        [SerializeField] private float _predictionAccuracyThreshold = 0.85f;
        
        [Header("Reward Configuration")]
        [SerializeField] private bool _enableInstantRewards = true;
        [SerializeField] private bool _enableCumulativeRewards = true;
        [SerializeField] private bool _enableTimedRewards = true;
        [SerializeField] private float _experienceMultiplier = 1.0f;
        [SerializeField] private float _reputationMultiplier = 1.0f;
        [SerializeField] private float _currencyMultiplier = 1.0f;
        [SerializeField] private bool _enableRewardScaling = true;
        
        [Header("Social Recognition Settings")]
        [SerializeField] private bool _enableSocialRecognition = true;
        [SerializeField] private bool _enableGlobalAnnouncements = true;
        [SerializeField] private bool _enableCommunityVoting = true;
        [SerializeField] private bool _enableMentorshipProgram = true;
        [SerializeField] private int _maxGlobalAnnouncementsPerDay = 50;
        [SerializeField] private float _socialInfluenceMultiplier = 1.0f;
        
        [Header("Hidden Achievement Settings")]
        [SerializeField] private bool _enableComplexTriggers = true;
        [SerializeField] private bool _enableTimedSecrets = true;
        [SerializeField] private bool _enableLocationSecrets = true;
        [SerializeField] private bool _enableSequenceSecrets = true;
        [SerializeField] private float _hiddenDiscoveryBonusMultiplier = 2.0f;
        [SerializeField] private int _maxSimultaneousHiddenTracking = 100;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentAchievementTracking = 1000;
        [SerializeField] private float _achievementProcessingTimeout = 5f;
        [SerializeField] private bool _enableAchievementCaching = true;
        [SerializeField] private int _achievementCacheSize = 500;
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _batchProcessingSize = 25;
        
        [Header("Integration Settings")]
        [SerializeField] private bool _enableCultivationIntegration = true;
        [SerializeField] private bool _enableBusinessIntegration = true;
        [SerializeField] private bool _enableGeneticsIntegration = true;
        [SerializeField] private bool _enableFacilityIntegration = true;
        [SerializeField] private bool _enableEducationalIntegration = true;
        [SerializeField] private bool _enableNarrativeIntegration = true;
        
        [Header("Analytics and Monitoring")]
        [SerializeField] private bool _enableAchievementAnalytics = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private bool _enablePlayerBehaviorTracking = true;
        [SerializeField] private bool _enableEngagementOptimization = true;
        [SerializeField] private float _analyticsUpdateInterval = 30f;
        [SerializeField] private int _maxAnalyticsHistoryDays = 365;
        
        [Header("Cultural and Educational Settings")]
        [SerializeField] private bool _enableCulturalAchievements = true;
        [SerializeField] private bool _enableEducationalValidation = true;
        [SerializeField] private bool _enableScientificAccuracy = true;
        [SerializeField] private bool _enableHistoricalAchievements = true;
        [SerializeField] private float _educationalEffectivenessThreshold = 0.8f;
        [SerializeField] private bool _enableMultilingualSupport = true;
        
        // Properties for easy access
        public bool EnableAchievements => _enableAchievements;
        public bool EnableMilestones => _enableMilestones;
        public bool EnableHiddenAchievements => _enableHiddenAchievements;
        public bool EnableCommunityAchievements => _enableCommunityAchievements;
        public bool EnableSeasonalAchievements => _enableSeasonalAchievements;
        public bool EnableCulturalCelebrations => _enableCulturalCelebrations;
        
        public float ProgressUpdateInterval => _progressUpdateInterval;
        public int MaxRecentAchievements => _maxRecentAchievements;
        public bool EnableRealTimeTracking => _enableRealTimeTracking;
        public bool EnableAchievementHints => _enableAchievementHints;
        public bool EnableProgressPrediction => _enableProgressPrediction;
        public float PredictionAccuracyThreshold => _predictionAccuracyThreshold;
        
        public bool EnableInstantRewards => _enableInstantRewards;
        public bool EnableCumulativeRewards => _enableCumulativeRewards;
        public bool EnableTimedRewards => _enableTimedRewards;
        public float ExperienceMultiplier => _experienceMultiplier;
        public float ReputationMultiplier => _reputationMultiplier;
        public float CurrencyMultiplier => _currencyMultiplier;
        public bool EnableRewardScaling => _enableRewardScaling;
        
        public bool EnableSocialRecognition => _enableSocialRecognition;
        public bool EnableGlobalAnnouncements => _enableGlobalAnnouncements;
        public bool EnableCommunityVoting => _enableCommunityVoting;
        public bool EnableMentorshipProgram => _enableMentorshipProgram;
        public int MaxGlobalAnnouncementsPerDay => _maxGlobalAnnouncementsPerDay;
        public float SocialInfluenceMultiplier => _socialInfluenceMultiplier;
        
        public bool EnableComplexTriggers => _enableComplexTriggers;
        public bool EnableTimedSecrets => _enableTimedSecrets;
        public bool EnableLocationSecrets => _enableLocationSecrets;
        public bool EnableSequenceSecrets => _enableSequenceSecrets;
        public float HiddenDiscoveryBonusMultiplier => _hiddenDiscoveryBonusMultiplier;
        public int MaxSimultaneousHiddenTracking => _maxSimultaneousHiddenTracking;
        
        public int MaxConcurrentAchievementTracking => _maxConcurrentAchievementTracking;
        public float AchievementProcessingTimeout => _achievementProcessingTimeout;
        public bool EnableAchievementCaching => _enableAchievementCaching;
        public int AchievementCacheSize => _achievementCacheSize;
        public bool EnableBatchProcessing => _enableBatchProcessing;
        public int BatchProcessingSize => _batchProcessingSize;
        
        public bool EnableCultivationIntegration => _enableCultivationIntegration;
        public bool EnableBusinessIntegration => _enableBusinessIntegration;
        public bool EnableGeneticsIntegration => _enableGeneticsIntegration;
        public bool EnableFacilityIntegration => _enableFacilityIntegration;
        public bool EnableEducationalIntegration => _enableEducationalIntegration;
        public bool EnableNarrativeIntegration => _enableNarrativeIntegration;
        
        public bool EnableAchievementAnalytics => _enableAchievementAnalytics;
        public bool EnablePerformanceMonitoring => _enablePerformanceMonitoring;
        public bool EnablePlayerBehaviorTracking => _enablePlayerBehaviorTracking;
        public bool EnableEngagementOptimization => _enableEngagementOptimization;
        public float AnalyticsUpdateInterval => _analyticsUpdateInterval;
        public int MaxAnalyticsHistoryDays => _maxAnalyticsHistoryDays;
        
        public bool EnableCulturalAchievements => _enableCulturalAchievements;
        public bool EnableEducationalValidation => _enableEducationalValidation;
        public bool EnableScientificAccuracy => _enableScientificAccuracy;
        public bool EnableHistoricalAchievements => _enableHistoricalAchievements;
        public float EducationalEffectivenessThreshold => _educationalEffectivenessThreshold;
        public bool EnableMultilingualSupport => _enableMultilingualSupport;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate timing settings
            _progressUpdateInterval = Mathf.Max(0.1f, _progressUpdateInterval);
            _achievementProcessingTimeout = Mathf.Max(1f, _achievementProcessingTimeout);
            _analyticsUpdateInterval = Mathf.Max(1f, _analyticsUpdateInterval);
            
            // Validate multipliers
            _experienceMultiplier = Mathf.Max(0.1f, _experienceMultiplier);
            _reputationMultiplier = Mathf.Max(0.1f, _reputationMultiplier);
            _currencyMultiplier = Mathf.Max(0.1f, _currencyMultiplier);
            _socialInfluenceMultiplier = Mathf.Max(0.1f, _socialInfluenceMultiplier);
            _hiddenDiscoveryBonusMultiplier = Mathf.Max(1f, _hiddenDiscoveryBonusMultiplier);
            
            // Validate thresholds
            _predictionAccuracyThreshold = Mathf.Clamp01(_predictionAccuracyThreshold);
            _educationalEffectivenessThreshold = Mathf.Clamp01(_educationalEffectivenessThreshold);
            
            // Validate counts and sizes
            _maxRecentAchievements = Mathf.Max(1, _maxRecentAchievements);
            _maxGlobalAnnouncementsPerDay = Mathf.Max(1, _maxGlobalAnnouncementsPerDay);
            _maxSimultaneousHiddenTracking = Mathf.Max(10, _maxSimultaneousHiddenTracking);
            _maxConcurrentAchievementTracking = Mathf.Max(100, _maxConcurrentAchievementTracking);
            _achievementCacheSize = Mathf.Max(50, _achievementCacheSize);
            _batchProcessingSize = Mathf.Max(5, _batchProcessingSize);
            _maxAnalyticsHistoryDays = Mathf.Max(30, _maxAnalyticsHistoryDays);
        }
        
        /// <summary>
        /// Get the complete configuration as a runtime data structure
        /// </summary>
        public AchievementSystemConfiguration GetConfiguration()
        {
            return new AchievementSystemConfiguration
            {
                // System settings
                EnableAchievements = _enableAchievements,
                EnableMilestones = _enableMilestones,
                EnableHiddenAchievements = _enableHiddenAchievements,
                EnableCommunityAchievements = _enableCommunityAchievements,
                EnableSeasonalAchievements = _enableSeasonalAchievements,
                EnableCulturalCelebrations = _enableCulturalCelebrations,
                
                // Progress tracking
                ProgressUpdateInterval = _progressUpdateInterval,
                MaxRecentAchievements = _maxRecentAchievements,
                EnableRealTimeTracking = _enableRealTimeTracking,
                EnableAchievementHints = _enableAchievementHints,
                EnableProgressPrediction = _enableProgressPrediction,
                PredictionAccuracyThreshold = _predictionAccuracyThreshold,
                
                // Rewards
                EnableInstantRewards = _enableInstantRewards,
                EnableCumulativeRewards = _enableCumulativeRewards,
                EnableTimedRewards = _enableTimedRewards,
                ExperienceMultiplier = _experienceMultiplier,
                ReputationMultiplier = _reputationMultiplier,
                CurrencyMultiplier = _currencyMultiplier,
                EnableRewardScaling = _enableRewardScaling,
                
                // Social recognition
                EnableSocialRecognition = _enableSocialRecognition,
                EnableGlobalAnnouncements = _enableGlobalAnnouncements,
                EnableCommunityVoting = _enableCommunityVoting,
                EnableMentorshipProgram = _enableMentorshipProgram,
                MaxGlobalAnnouncementsPerDay = _maxGlobalAnnouncementsPerDay,
                SocialInfluenceMultiplier = _socialInfluenceMultiplier,
                
                // Hidden achievements
                EnableComplexTriggers = _enableComplexTriggers,
                EnableTimedSecrets = _enableTimedSecrets,
                EnableLocationSecrets = _enableLocationSecrets,
                EnableSequenceSecrets = _enableSequenceSecrets,
                HiddenDiscoveryBonusMultiplier = _hiddenDiscoveryBonusMultiplier,
                MaxSimultaneousHiddenTracking = _maxSimultaneousHiddenTracking,
                
                // Performance
                MaxConcurrentAchievementTracking = _maxConcurrentAchievementTracking,
                AchievementProcessingTimeout = _achievementProcessingTimeout,
                EnableAchievementCaching = _enableAchievementCaching,
                AchievementCacheSize = _achievementCacheSize,
                EnableBatchProcessing = _enableBatchProcessing,
                BatchProcessingSize = _batchProcessingSize,
                
                // Integration
                EnableCultivationIntegration = _enableCultivationIntegration,
                EnableBusinessIntegration = _enableBusinessIntegration,
                EnableGeneticsIntegration = _enableGeneticsIntegration,
                EnableFacilityIntegration = _enableFacilityIntegration,
                EnableEducationalIntegration = _enableEducationalIntegration,
                EnableNarrativeIntegration = _enableNarrativeIntegration,
                
                // Analytics
                EnableAchievementAnalytics = _enableAchievementAnalytics,
                EnablePerformanceMonitoring = _enablePerformanceMonitoring,
                EnablePlayerBehaviorTracking = _enablePlayerBehaviorTracking,
                EnableEngagementOptimization = _enableEngagementOptimization,
                AnalyticsUpdateInterval = _analyticsUpdateInterval,
                MaxAnalyticsHistoryDays = _maxAnalyticsHistoryDays,
                
                // Cultural and educational
                EnableCulturalAchievements = _enableCulturalAchievements,
                EnableEducationalValidation = _enableEducationalValidation,
                EnableScientificAccuracy = _enableScientificAccuracy,
                EnableHistoricalAchievements = _enableHistoricalAchievements,
                EducationalEffectivenessThreshold = _educationalEffectivenessThreshold,
                EnableMultilingualSupport = _enableMultilingualSupport
            };
        }
        
        protected override bool ValidateDataSpecific()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate essential settings
            if (_progressUpdateInterval <= 0f)
            {
                validationErrors.Add("Progress update interval must be greater than 0");
                isValid = false;
            }
            
            if (_achievementProcessingTimeout <= 0f)
            {
                validationErrors.Add("Achievement processing timeout must be greater than 0");
                isValid = false;
            }
            
            if (_maxConcurrentAchievementTracking <= 0)
            {
                validationErrors.Add("Max concurrent achievement tracking must be greater than 0");
                isValid = false;
            }
            
            // Validate multipliers are reasonable
            if (_experienceMultiplier <= 0f || _experienceMultiplier > 10f)
            {
                validationErrors.Add("Experience multiplier should be between 0.1 and 10");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"[AchievementConfigSO] Validation failed for {name}: {string.Join(", ", validationErrors)}");
            }
            
            return base.ValidateDataSpecific() && isValid;
        }
    }
    
    /// <summary>
    /// Runtime configuration data structure for the achievement system
    /// </summary>
    [Serializable]
    public class AchievementSystemConfiguration
    {
        // System settings
        public bool EnableAchievements;
        public bool EnableMilestones;
        public bool EnableHiddenAchievements;
        public bool EnableCommunityAchievements;
        public bool EnableSeasonalAchievements;
        public bool EnableCulturalCelebrations;
        
        // Progress tracking
        public float ProgressUpdateInterval;
        public int MaxRecentAchievements;
        public bool EnableRealTimeTracking;
        public bool EnableAchievementHints;
        public bool EnableProgressPrediction;
        public float PredictionAccuracyThreshold;
        
        // Rewards
        public bool EnableInstantRewards;
        public bool EnableCumulativeRewards;
        public bool EnableTimedRewards;
        public float ExperienceMultiplier;
        public float ReputationMultiplier;
        public float CurrencyMultiplier;
        public bool EnableRewardScaling;
        
        // Social recognition
        public bool EnableSocialRecognition;
        public bool EnableGlobalAnnouncements;
        public bool EnableCommunityVoting;
        public bool EnableMentorshipProgram;
        public int MaxGlobalAnnouncementsPerDay;
        public float SocialInfluenceMultiplier;
        
        // Hidden achievements
        public bool EnableComplexTriggers;
        public bool EnableTimedSecrets;
        public bool EnableLocationSecrets;
        public bool EnableSequenceSecrets;
        public float HiddenDiscoveryBonusMultiplier;
        public int MaxSimultaneousHiddenTracking;
        
        // Performance
        public int MaxConcurrentAchievementTracking;
        public float AchievementProcessingTimeout;
        public bool EnableAchievementCaching;
        public int AchievementCacheSize;
        public bool EnableBatchProcessing;
        public int BatchProcessingSize;
        
        // Integration
        public bool EnableCultivationIntegration;
        public bool EnableBusinessIntegration;
        public bool EnableGeneticsIntegration;
        public bool EnableFacilityIntegration;
        public bool EnableEducationalIntegration;
        public bool EnableNarrativeIntegration;
        
        // Analytics
        public bool EnableAchievementAnalytics;
        public bool EnablePerformanceMonitoring;
        public bool EnablePlayerBehaviorTracking;
        public bool EnableEngagementOptimization;
        public float AnalyticsUpdateInterval;
        public int MaxAnalyticsHistoryDays;
        
        // Cultural and educational
        public bool EnableCulturalAchievements;
        public bool EnableEducationalValidation;
        public bool EnableScientificAccuracy;
        public bool EnableHistoricalAchievements;
        public float EducationalEffectivenessThreshold;
        public bool EnableMultilingualSupport;
    }
}