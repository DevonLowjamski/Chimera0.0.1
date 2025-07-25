using ProjectChimera.Data;
using ProjectChimera.Core;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectChimera.Data.Gaming
{
    /// <summary>
    /// ScriptableObject configuration for Mini-Game Manager following Project Chimera data architecture standards.
    /// Provides centralized configuration for all mini-game system parameters with validation and performance optimization.
    /// </summary>
    [CreateAssetMenu(fileName = "New MiniGame Config", menuName = "Project Chimera/Gaming/Mini-Game Config", order = 100)]
    public class MiniGameConfigSO : ChimeraConfigSO
    {
        [Header("Core Configuration")]
        [SerializeField] private bool _enableMiniGames = true;
        [SerializeField] private float _defaultSessionTimeout = 300f;
        [SerializeField] private int _maxConcurrentSessions = 3;
        [SerializeField] private bool _enableCrossSystemIntegration = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxUpdateBatchSize = 10;
        [SerializeField] private bool _enableMemoryPooling = true;
        [SerializeField] private float _performanceMonitoringInterval = 5.0f;
        
        [Header("Difficulty Scaling")]
        [SerializeField] private bool _enableAdaptiveDifficulty = true;
        [SerializeField] private float _difficultyAdjustmentRate = 0.1f;
        [SerializeField] private AnimationCurve _skillProgressionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private DifficultyLevel _startingDifficulty = DifficultyLevel.Beginner;
        [SerializeField] private float _difficultyUpdateInterval = 10.0f;
        
        [Header("Reward Configuration")]
        [SerializeField] private bool _enableSkillBasedRewards = true;
        [SerializeField] private float _basePerformanceMultiplier = 1.0f;
        [SerializeField] private bool _enableInstantFeedback = true;
        [SerializeField] private RewardDistributionType _rewardDistribution = RewardDistributionType.Immediate;
        [SerializeField] private float _streakBonusMultiplier = 0.1f;
        
        [Header("Analytics and Telemetry")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _enablePlayerBehaviorTracking = true;
        [SerializeField] private float _analyticsUpdateInterval = 5.0f;
        [SerializeField] private bool _enablePerformanceMetrics = true;
        [SerializeField] private int _maxAnalyticsHistorySize = 1000;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalContent = true;
        [SerializeField] private bool _enableScientificAccuracy = true;
        [SerializeField] private float _scientificAccuracyThreshold = 0.95f;
        [SerializeField] private bool _enableLearningProgressTracking = true;
        
        [Header("Social Features")]
        [SerializeField] private bool _enableMultiplayerGames = true;
        [SerializeField] private bool _enableLeaderboards = true;
        [SerializeField] private bool _enableSocialSharing = true;
        [SerializeField] private int _maxMultiplayerParticipants = 8;
        
        // Properties following Project Chimera naming conventions
        public bool EnableMiniGames => _enableMiniGames;
        public float DefaultSessionTimeout => _defaultSessionTimeout;
        public int MaxConcurrentSessions => _maxConcurrentSessions;
        public bool EnableCrossSystemIntegration => _enableCrossSystemIntegration;
        public int TargetFrameRate => _targetFrameRate;
        public bool EnablePerformanceOptimization => _enablePerformanceOptimization;
        public int MaxUpdateBatchSize => _maxUpdateBatchSize;
        public bool EnableMemoryPooling => _enableMemoryPooling;
        public float PerformanceMonitoringInterval => _performanceMonitoringInterval;
        public bool EnableAdaptiveDifficulty => _enableAdaptiveDifficulty;
        public float DifficultyAdjustmentRate => _difficultyAdjustmentRate;
        public AnimationCurve SkillProgressionCurve => _skillProgressionCurve;
        public DifficultyLevel StartingDifficulty => _startingDifficulty;
        public float DifficultyUpdateInterval => _difficultyUpdateInterval;
        public bool EnableSkillBasedRewards => _enableSkillBasedRewards;
        public float BasePerformanceMultiplier => _basePerformanceMultiplier;
        public bool EnableInstantFeedback => _enableInstantFeedback;
        public RewardDistributionType RewardDistribution => _rewardDistribution;
        public float StreakBonusMultiplier => _streakBonusMultiplier;
        public bool EnableAnalytics => _enableAnalytics;
        public bool EnablePlayerBehaviorTracking => _enablePlayerBehaviorTracking;
        public float AnalyticsUpdateInterval => _analyticsUpdateInterval;
        public bool EnablePerformanceMetrics => _enablePerformanceMetrics;
        public int MaxAnalyticsHistorySize => _maxAnalyticsHistorySize;
        public bool EnableEducationalContent => _enableEducationalContent;
        public bool EnableScientificAccuracy => _enableScientificAccuracy;
        public float ScientificAccuracyThreshold => _scientificAccuracyThreshold;
        public bool EnableLearningProgressTracking => _enableLearningProgressTracking;
        public bool EnableMultiplayerGames => _enableMultiplayerGames;
        public bool EnableLeaderboards => _enableLeaderboards;
        public bool EnableSocialSharing => _enableSocialSharing;
        public int MaxMultiplayerParticipants => _maxMultiplayerParticipants;
        
        public override bool ValidateData()
        {
            var isValid = base.ValidateData();
            var validationErrors = new List<string>();
            
            // Validate timeout values
            if (_defaultSessionTimeout <= 0)
            {
                validationErrors.Add("Default session timeout must be positive");
                isValid = false;
            }
            
            if (_defaultSessionTimeout > 3600) // 1 hour maximum
            {
                validationErrors.Add("Default session timeout should not exceed 1 hour");
                isValid = false;
            }
            
            // Validate concurrent session limits
            if (_maxConcurrentSessions <= 0)
            {
                validationErrors.Add("Max concurrent sessions must be positive");
                isValid = false;
            }
            
            if (_maxConcurrentSessions > 10)
            {
                validationErrors.Add("Max concurrent sessions should not exceed 10 for performance reasons");
                isValid = false;
            }
            
            // Validate performance settings
            if (_targetFrameRate < 30)
            {
                validationErrors.Add("Target frame rate should be at least 30 FPS");
                isValid = false;
            }
            
            if (_maxUpdateBatchSize <= 0)
            {
                validationErrors.Add("Max update batch size must be positive");
                isValid = false;
            }
            
            // Validate difficulty settings
            if (_difficultyAdjustmentRate < 0 || _difficultyAdjustmentRate > 1)
            {
                validationErrors.Add("Difficulty adjustment rate must be between 0 and 1");
                isValid = false;
            }
            
            // Validate skill progression curve
            if (_skillProgressionCurve == null)
            {
                validationErrors.Add("Skill progression curve is required");
                isValid = false;
            }
            
            // Validate multiplayer settings
            if (_enableMultiplayerGames && _maxMultiplayerParticipants <= 1)
            {
                validationErrors.Add("Max multiplayer participants must be greater than 1 when multiplayer is enabled");
                isValid = false;
            }
            
            // Validate scientific accuracy
            if (_enableScientificAccuracy && (_scientificAccuracyThreshold < 0.5f || _scientificAccuracyThreshold > 1.0f))
            {
                validationErrors.Add("Scientific accuracy threshold must be between 0.5 and 1.0");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"MiniGameConfigSO validation failed: {string.Join(", ", validationErrors)}");
            }
            else
            {
                Debug.Log("MiniGameConfigSO validation passed successfully");
            }
            
            return isValid;
        }
        
        public override void ResetToDefaults()
        {
            base.ResetToDefaults();
            
            _enableMiniGames = true;
            _defaultSessionTimeout = 300f;
            _maxConcurrentSessions = 3;
            _enableCrossSystemIntegration = true;
            _targetFrameRate = 60;
            _enablePerformanceOptimization = true;
            _maxUpdateBatchSize = 10;
            _enableMemoryPooling = true;
            _performanceMonitoringInterval = 5.0f;
            _enableAdaptiveDifficulty = true;
            _difficultyAdjustmentRate = 0.1f;
            _skillProgressionCurve = AnimationCurve.Linear(0, 0, 1, 1);
            _startingDifficulty = DifficultyLevel.Beginner;
            _difficultyUpdateInterval = 10.0f;
            _enableSkillBasedRewards = true;
            _basePerformanceMultiplier = 1.0f;
            _enableInstantFeedback = true;
            _rewardDistribution = RewardDistributionType.Immediate;
            _streakBonusMultiplier = 0.1f;
            _enableAnalytics = true;
            _enablePlayerBehaviorTracking = true;
            _analyticsUpdateInterval = 5.0f;
            _enablePerformanceMetrics = true;
            _maxAnalyticsHistorySize = 1000;
            _enableEducationalContent = true;
            _enableScientificAccuracy = true;
            _scientificAccuracyThreshold = 0.95f;
            _enableLearningProgressTracking = true;
            _enableMultiplayerGames = true;
            _enableLeaderboards = true;
            _enableSocialSharing = true;
            _maxMultiplayerParticipants = 8;
        }
        
        /// <summary>
        /// Get performance configuration optimized for current hardware
        /// </summary>
        public PerformanceConfiguration GetOptimizedPerformanceConfig()
        {
            var systemMemory = SystemInfo.systemMemorySize;
            var processorCount = SystemInfo.processorCount;
            
            return new PerformanceConfiguration
            {
                MaxConcurrentSessions = systemMemory >= 8192 ? _maxConcurrentSessions : Mathf.Max(1, _maxConcurrentSessions / 2),
                UpdateBatchSize = processorCount >= 4 ? _maxUpdateBatchSize : Mathf.Max(5, _maxUpdateBatchSize / 2),
                EnableMemoryPooling = systemMemory >= 4096 ? _enableMemoryPooling : true,
                PerformanceMonitoringInterval = systemMemory >= 8192 ? _performanceMonitoringInterval : _performanceMonitoringInterval * 2
            };
        }
    }
    
    /// <summary>
    /// Reward distribution types for mini-game completion
    /// </summary>
    public enum RewardDistributionType
    {
        Immediate,      // Rewards given immediately upon completion
        Delayed,        // Rewards given after a short delay
        Batch,          // Rewards accumulated and given in batches
        Progressive     // Rewards given progressively during gameplay
    }
    
    /// <summary>
    /// Difficulty levels for mini-games with Project Chimera sophistication
    /// </summary>
    public enum DifficultyLevel
    {
        Tutorial,       // Guided learning experience
        Beginner,       // Basic challenges for new players
        Intermediate,   // Standard difficulty for regular players
        Advanced,       // Challenging content for experienced players
        Expert,         // Very difficult content for skilled players
        Master,         // Extremely challenging content for experts
        Adaptive,       // Dynamic difficulty based on player performance
        Custom          // Player-defined difficulty parameters
    }
    
    /// <summary>
    /// Performance configuration structure
    /// </summary>
    [System.Serializable]
    public class PerformanceConfiguration
    {
        public int MaxConcurrentSessions;
        public int UpdateBatchSize;
        public bool EnableMemoryPooling;
        public float PerformanceMonitoringInterval;
    }
}