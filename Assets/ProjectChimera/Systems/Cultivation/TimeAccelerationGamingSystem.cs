using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Core.Events;
using ProjectChimera.Data.Events;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Time Acceleration Gaming System - Flexible time control for biological process observation
    /// Integrates with existing TimeManager and provides gaming-focused time manipulation
    /// Core component of Enhanced Cultivation Gaming System v2.0
    /// </summary>
    public class TimeAccelerationGamingSystem : MonoBehaviour
    {
        [Header("Time Acceleration Configuration")]
        [SerializeField] private TimeAccelerationGamingConfigSO _timeConfig;
        [SerializeField] private TimeScaleLibrarySO _timeScaleLibrary;
        [SerializeField] private TimeTransitionConfigSO _transitionConfig;
        
        [Header("Gaming Time Settings")]
        [Range(0.1f, 12.0f)] public float MaxTimeAcceleration = 8.0f;
        [Range(1.0f, 60.0f)] public float TransitionLockInDuration = 15.0f;
        [Range(0.5f, 10.0f)] public float TransitionDelay = 3.0f;
        [Range(0.1f, 2.0f)] public float AutomationTimeAdvantage = 1.5f;
        
        [Header("Player Experience Optimization")]
        [Range(0.1f, 2.0f)] public float ManualTaskComplexityMultiplier = 1.2f;
        [Range(0.1f, 1.0f)] public float PlayerEngagementThreshold = 0.7f;
        [Range(1.0f, 10.0f)] public float OptimalActionsPerMinute = 5.0f;
        
        // System State
        private bool _isInitialized = false;
        private GameTimeScale _currentTimeScale = GameTimeScale.Baseline;
        private GameTimeScale _previousTimeScale = GameTimeScale.Baseline;
        private TimeTransitionState _transitionState = TimeTransitionState.Stable;
        private float _timeScaleLockEndTime = 0f;
        private bool _isInTransition = false;
        
        // Time Acceleration Data
        private Dictionary<GameTimeScale, TimeScaleData> _timeScaleData = new Dictionary<GameTimeScale, TimeScaleData>();
        private TimeAccelerationResult _currentAccelerationResult;
        private TimeTransition _currentTransition;
        
        // Integration with existing systems
        private TimeManager _existingTimeManager;
        private TimeTransitionManager _transitionManager;
        private GameSpeedController _speedController;
        private InteractivePlantCareSystem _plantCareSystem;
        private EarnedAutomationProgressionSystem _automationSystem;
        
        // Gaming Performance Tracking
        private float _sessionStartTime;
        private int _timeScaleChanges = 0;
        private float _totalAcceleratedTime = 0f;
        private Dictionary<GameTimeScale, float> _timeSpentAtScale = new Dictionary<GameTimeScale, float>();
        private float _playerEngagementScore = 0f;
        
        // Events
        private GameEventChannelSO _onTimeScaleChanged;
        private GameEventChannelSO _onTransitionStarted;
        private GameEventChannelSO _onTransitionCompleted;
        private GameEventChannelSO _onTimeSpeedOptimized;
        private GameEventChannelSO _onPlayerEngagementChanged;
        
        #region Initialization
        
        public void Initialize(TimeAccelerationGamingConfigSO config)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("TimeAccelerationGamingSystem already initialized", this);
                return;
            }
            
            _timeConfig = config ?? _timeConfig;
            
            if (_timeConfig == null)
            {
                ChimeraLogger.LogError("TimeAccelerationGamingConfigSO is required for initialization", this);
                return;
            }
            
            InitializeTimeScales();
            InitializeTimeScaleData();
            SetupIntegrationWithExistingSystems();
            SetupEventChannels();
            
            _sessionStartTime = Time.time;
            _isInitialized = true;
            
            ChimeraLogger.Log("TimeAccelerationGamingSystem initialized successfully", this);
        }
        
        private void InitializeTimeScales()
        {
            // Initialize all time scale tracking
            foreach (GameTimeScale scale in System.Enum.GetValues(typeof(GameTimeScale)))
            {
                _timeSpentAtScale[scale] = 0f;
            }
        }
        
        private void InitializeTimeScaleData()
        {
            // Define time scale characteristics
            _timeScaleData[GameTimeScale.SlowMotion] = new TimeScaleData
            {
                Scale = GameTimeScale.SlowMotion,
                Multiplier = 0.5f,
                RealTimeDayDuration = 300f, // 5 minutes per in-game day
                GameDaysPerRealHour = 12f,
                Description = "Slow motion for detailed observation",
                ManualTaskDifficulty = 0.8f,
                AutomationAdvantage = 1.0f,
                PlayerEngagementOptimal = true
            };
            
            _timeScaleData[GameTimeScale.Baseline] = new TimeScaleData
            {
                Scale = GameTimeScale.Baseline,
                Multiplier = 1.0f,
                RealTimeDayDuration = 600f, // 1 in-game week = 1 real hour (6 days = 60 min)
                GameDaysPerRealHour = 6f,
                Description = "Standard baseline time flow",
                ManualTaskDifficulty = 1.0f,
                AutomationAdvantage = 1.0f,
                PlayerEngagementOptimal = true
            };
            
            _timeScaleData[GameTimeScale.Standard] = new TimeScaleData
            {
                Scale = GameTimeScale.Standard,
                Multiplier = 2.0f,
                RealTimeDayDuration = 120f, // 2 minutes per in-game day
                GameDaysPerRealHour = 30f,
                Description = "Standard accelerated cultivation",
                ManualTaskDifficulty = 1.2f,
                AutomationAdvantage = 1.2f,
                PlayerEngagementOptimal = true
            };
            
            _timeScaleData[GameTimeScale.Fast] = new TimeScaleData
            {
                Scale = GameTimeScale.Fast,
                Multiplier = 4.0f,
                RealTimeDayDuration = 60f, // 1 minute per in-game day
                GameDaysPerRealHour = 60f,
                Description = "Fast cultivation for experienced players",
                ManualTaskDifficulty = 1.5f,
                AutomationAdvantage = 1.5f,
                PlayerEngagementOptimal = false
            };
            
            _timeScaleData[GameTimeScale.VeryFast] = new TimeScaleData
            {
                Scale = GameTimeScale.VeryFast,
                Multiplier = 8.0f,
                RealTimeDayDuration = 30f, // 30 seconds per in-game day
                GameDaysPerRealHour = 120f,
                Description = "Very fast for automated facilities",
                ManualTaskDifficulty = 2.0f,
                AutomationAdvantage = 2.0f,
                PlayerEngagementOptimal = false
            };
            
            _timeScaleData[GameTimeScale.Lightning] = new TimeScaleData
            {
                Scale = GameTimeScale.Lightning,
                Multiplier = 12.0f,
                RealTimeDayDuration = 20f, // 20 seconds per in-game day
                GameDaysPerRealHour = 180f,
                Description = "Maximum speed for fully automated operations",
                ManualTaskDifficulty = 3.0f,
                AutomationAdvantage = 3.0f,
                PlayerEngagementOptimal = false
            };
        }
        
        private void SetupIntegrationWithExistingSystems()
        {
            // Find existing time manager
            _existingTimeManager = FindObjectOfType<TimeManager>();
            if (_existingTimeManager == null)
            {
                ChimeraLogger.LogWarning("Existing TimeManager not found, time integration may be limited", this);
            }
            
            // Setup transition manager
            _transitionManager = GetComponentInChildren<TimeTransitionManager>();
            if (_transitionManager == null)
            {
                var transitionGO = new GameObject("TimeTransitionManager");
                transitionGO.transform.SetParent(transform);
                _transitionManager = transitionGO.AddComponent<TimeTransitionManager>();
            }
            
            // Setup speed controller
            _speedController = GetComponentInChildren<GameSpeedController>();
            if (_speedController == null)
            {
                var speedGO = new GameObject("GameSpeedController");
                speedGO.transform.SetParent(transform);
                _speedController = speedGO.AddComponent<GameSpeedController>();
            }
            
            // Find other systems for integration
            _plantCareSystem = FindObjectOfType<InteractivePlantCareSystem>();
            _automationSystem = FindObjectOfType<EarnedAutomationProgressionSystem>();
        }
        
        private void SetupEventChannels()
        {
            // Event channels will be connected through the main cultivation gaming manager
        }
        
        #endregion
        
        #region System Updates
        
        /// <summary>
        /// Main system update method - processes time acceleration, transitions, and player engagement
        /// Called by the EnhancedCultivationGamingManager or Unity Update loop
        /// </summary>
        public void UpdateSystem()
        {
            if (!_isInitialized) return;
            
            // Update time tracking
            UpdateTimeTracking();
            
            // Process active transitions
            ProcessTimeTransitions();
            
            // Update player engagement metrics
            UpdatePlayerEngagement();
            
            // Apply current time scale
            ApplyTimeScale();
            
            // Update performance metrics
            UpdatePerformanceMetrics();
        }
        
        private void UpdateTimeTracking()
        {
            float deltaTime = Time.unscaledDeltaTime;
            _timeSpentAtScale[_currentTimeScale] += deltaTime;
            
            if (_currentTimeScale != GameTimeScale.Baseline)
            {
                _totalAcceleratedTime += deltaTime;
            }
        }
        
        private void ProcessTimeTransitions()
        {
            if (!_isInTransition) return;
            
            if (_transitionManager != null)
            {
                _transitionManager.ProcessTransition();
                
                if (_transitionManager.IsTransitionComplete())
                {
                    CompleteTimeTransition();
                }
            }
        }
        
        private void UpdatePlayerEngagement()
        {
            // Calculate player engagement based on actions per minute and current time scale
            float timeScaleMultiplier = _timeScaleData.ContainsKey(_currentTimeScale) 
                ? _timeScaleData[_currentTimeScale].Multiplier 
                : 1.0f;
                
            // Higher time scales require more engagement to maintain optimal play
            float requiredEngagement = OptimalActionsPerMinute * (timeScaleMultiplier * 0.5f);
            
            // Update engagement score (simplified calculation)
            _playerEngagementScore = Mathf.Lerp(_playerEngagementScore, 
                CalculateCurrentEngagement() / requiredEngagement, 
                Time.unscaledDeltaTime * 0.5f);
                
            // Clamp engagement score
            _playerEngagementScore = Mathf.Clamp01(_playerEngagementScore);
        }
        
        private float CalculateCurrentEngagement()
        {
            // This would integrate with actual player action tracking
            // For now, return a baseline value
            return OptimalActionsPerMinute * 0.8f;
        }
        
        private void ApplyTimeScale()
        {
            if (!_timeScaleData.ContainsKey(_currentTimeScale)) return;
            
            float targetTimeScale = _timeScaleData[_currentTimeScale].Multiplier;
            
            // Apply time scale through Unity's Time.timeScale
            Time.timeScale = targetTimeScale;
            
            // Also update existing TimeManager if present
            if (_existingTimeManager != null)
            {
                // Integration with existing time manager would go here
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            // Track performance metrics for analytics
            if (Time.frameCount % 60 == 0) // Every second at 60 FPS
            {
                // Log performance data, check for issues, etc.
                ValidateTimeSystemPerformance();
            }
        }
        
        private void ValidateTimeSystemPerformance()
        {
            // Ensure time system is performing within acceptable parameters
            if (Time.timeScale <= 0f || Time.timeScale > MaxTimeAcceleration)
            {
                ChimeraLogger.LogWarning($"Time scale out of bounds: {Time.timeScale}, resetting to baseline", this);
                SetTimeScale(GameTimeScale.Baseline);
            }
        }
        
        #endregion
        
        #region Public Interface Methods
        
        /// <summary>
        /// Set the current time scale
        /// </summary>
        public void SetTimeScale(GameTimeScale newScale)
        {
            if (!_isInitialized) return;
            
            if (_isInTransition)
            {
                ChimeraLogger.LogWarning("Cannot change time scale during transition", this);
                return;
            }
            
            _previousTimeScale = _currentTimeScale;
            _currentTimeScale = newScale;
            _timeScaleChanges++;
            
            // Start transition if configured
            if (_transitionConfig != null && _transitionManager != null)
            {
                StartTimeTransition();
            }
            else
            {
                // Immediate change
                ApplyTimeScale();
            }
            
            ChimeraLogger.Log($"Time scale changed from {_previousTimeScale} to {_currentTimeScale}", this);
        }
        
        /// <summary>
        /// Get current time scale information
        /// </summary>
        public TimeScaleData GetCurrentTimeScaleData()
        {
            return _timeScaleData.ContainsKey(_currentTimeScale) 
                ? _timeScaleData[_currentTimeScale] 
                : new TimeScaleData();
        }
        
        /// <summary>
        /// Check if the system can accept a time scale change
        /// </summary>
        public bool CanChangeTimeScale()
        {
            return _isInitialized && !_isInTransition;
        }
        
        private void StartTimeTransition()
        {
            _isInTransition = true;
            _transitionState = TimeTransitionState.Starting;
            
            if (_transitionManager != null)
            {
                _transitionManager.StartTransition(_previousTimeScale, _currentTimeScale);
            }
        }
        
        private void CompleteTimeTransition()
        {
            _isInTransition = false;
            _transitionState = TimeTransitionState.Stable;
            
            ChimeraLogger.Log($"Time transition completed: {_previousTimeScale} -> {_currentTimeScale}", this);
        }
        
        #endregion
    }
} 