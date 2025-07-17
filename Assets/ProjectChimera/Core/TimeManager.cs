using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Manages game time acceleration, offline progression, and coordinates all time-dependent systems.
    /// Critical for Project Chimera's simulation mechanics where plants grow over real-world time.
    /// </summary>
    public class TimeManager : ChimeraManager, IGameStateListener, IPausable
    {
        [Header("Time Configuration")]
        [SerializeField] private TimeConfigSO _timeConfig;
        [SerializeField] private bool _enableOfflineProgression = true;
        [SerializeField] private float _defaultTimeScale = 1.0f;
        [SerializeField] private float _maxTimeScale = 100.0f;
        [SerializeField] private float _minTimeScale = 0.1f;

        [Header("Time Scale Events")]
        [SerializeField] private FloatGameEventSO _onTimeScaleChanged;
        [SerializeField] private SimpleGameEventSO _onTimePaused;
        [SerializeField] private SimpleGameEventSO _onTimeResumed;
        [SerializeField] private FloatGameEventSO _onOfflineProgressionCalculated;

        [Header("Debug Settings")]
        [SerializeField] private bool _enableTimeDebug = false;

        // Time tracking
        private DateTime _gameStartTime;
        private DateTime _lastSaveTime;
        private DateTime _sessionStartTime;
        private float _currentTimeScale = 1.0f;
        private bool _isTimePaused = false;
        private bool _wasTimeScaledBeforePause = false;
        private float _timeScaleBeforePause = 1.0f;

        // Performance tracking
        private float _accumulatedGameTime = 0.0f;
        private float _accumulatedRealTime = 0.0f;
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private const int MAX_FRAME_HISTORY = 60;

        // Time listeners
        private readonly List<ITimeScaleListener> _timeScaleListeners = new List<ITimeScaleListener>();
        private readonly List<IOfflineProgressionListener> _offlineProgressionListeners = new List<IOfflineProgressionListener>();

        /// <summary>
        /// Current time scale multiplier affecting all game time calculations.
        /// </summary>
        public float CurrentTimeScale 
        { 
            get => _currentTimeScale; 
            private set => _currentTimeScale = Mathf.Clamp(value, _minTimeScale, _maxTimeScale);
        }

        /// <summary>
        /// Whether time progression is currently paused.
        /// </summary>
        public bool IsTimePaused => _isTimePaused;

        /// <summary>
        /// Time when the current game session started.
        /// </summary>
        public DateTime SessionStartTime => _sessionStartTime;

        /// <summary>
        /// Time when the game world was first created.
        /// </summary>
        public DateTime GameStartTime => _gameStartTime;

        /// <summary>
        /// Total real-world time the game has been running.
        /// </summary>
        public TimeSpan TotalGameTime => DateTime.Now - _gameStartTime;

        /// <summary>
        /// Current session duration.
        /// </summary>
        public TimeSpan SessionDuration => DateTime.Now - _sessionStartTime;

        /// <summary>
        /// Accelerated game time (affected by time scale).
        /// </summary>
        public float AcceleratedGameTime => _accumulatedGameTime;

        /// <summary>
        /// Average frame time over the last 60 frames.
        /// </summary>
        public float AverageFrameTime
        {
            get
            {
                if (_frameTimeHistory.Count == 0) return 0.0f;
                float sum = 0.0f;
                foreach (float frameTime in _frameTimeHistory)
                {
                    sum += frameTime;
                }
                return sum / _frameTimeHistory.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _sessionStartTime = DateTime.Now;
        }

        protected override void OnManagerInitialize()
        {
            LogDebug("Initializing Time Manager");

            // Load time configuration
            if (_timeConfig != null)
            {
                _defaultTimeScale = _timeConfig.DefaultTimeScale;
                _maxTimeScale = _timeConfig.MaxTimeScale;
                _minTimeScale = _timeConfig.MinTimeScale;
                _enableOfflineProgression = _timeConfig.EnableOfflineProgression;
            }

            // Set initial time scale
            SetTimeScale(_defaultTimeScale);

            // Initialize game start time (this would normally be loaded from save data)
            _gameStartTime = DateTime.Now;
            _lastSaveTime = DateTime.Now;

            // Calculate offline progression if enabled
            if (_enableOfflineProgression)
            {
                StartCoroutine(CalculateOfflineProgressionCoroutine());
            }

            LogDebug($"Time Manager initialized - Time Scale: {CurrentTimeScale}x");
        }

        protected override void OnManagerShutdown()
        {
            LogDebug("Shutting down Time Manager");

            // Record shutdown time for offline progression
            _lastSaveTime = DateTime.Now;

            // Clear listeners
            _timeScaleListeners.Clear();
            _offlineProgressionListeners.Clear();
            _frameTimeHistory.Clear();
        }

        private new void Update()
        {
            if (!IsInitialized) return;

            // Track performance
            TrackFrameTime();

            // Update accelerated game time
            if (!_isTimePaused)
            {
                float deltaTime = Time.unscaledDeltaTime;
                _accumulatedGameTime += deltaTime * CurrentTimeScale;
                _accumulatedRealTime += deltaTime;
            }

            // Debug output
            if (_enableTimeDebug && Time.frameCount % 60 == 0) // Every 60 frames
            {
                LogDebug($"Time Scale: {CurrentTimeScale}x | Game Time: {_accumulatedGameTime:F1}s | Real Time: {_accumulatedRealTime:F1}s");
            }
        }

        /// <summary>
        /// Sets the time scale for the game world.
        /// </summary>
        public void SetTimeScale(float newTimeScale)
        {
            float previousTimeScale = CurrentTimeScale;
            CurrentTimeScale = newTimeScale;

            if (!_isTimePaused)
            {
                Time.timeScale = CurrentTimeScale;
            }

            LogDebug($"Time scale changed: {previousTimeScale}x -> {CurrentTimeScale}x");

            // Notify listeners
            NotifyTimeScaleListeners(previousTimeScale, CurrentTimeScale);
            _onTimeScaleChanged?.Raise(CurrentTimeScale);
        }

        /// <summary>
        /// Increases time scale by the specified multiplier.
        /// </summary>
        public void IncreaseTimeScale(float multiplier = 2.0f)
        {
            SetTimeScale(CurrentTimeScale * multiplier);
        }

        /// <summary>
        /// Decreases time scale by the specified divisor.
        /// </summary>
        public void DecreaseTimeScale(float divisor = 2.0f)
        {
            SetTimeScale(CurrentTimeScale / divisor);
        }

        /// <summary>
        /// Resets time scale to the default value.
        /// </summary>
        public void ResetTimeScale()
        {
            SetTimeScale(_defaultTimeScale);
        }

        /// <summary>
        /// Pauses time progression.
        /// </summary>
        public void Pause()
        {
            if (_isTimePaused) return;

            _isTimePaused = true;
            _timeScaleBeforePause = CurrentTimeScale;
            _wasTimeScaledBeforePause = CurrentTimeScale != 1.0f;

            Time.timeScale = 0.0f;

            LogDebug("Time paused");
            _onTimePaused?.Raise();
        }

        /// <summary>
        /// Resumes time progression.
        /// </summary>
        public void Resume()
        {
            if (!_isTimePaused) return;

            _isTimePaused = false;
            Time.timeScale = CurrentTimeScale;

            LogDebug($"Time resumed at {CurrentTimeScale}x scale");
            _onTimeResumed?.Raise();
        }

        /// <summary>
        /// Calculates offline progression since last save.
        /// </summary>
        private IEnumerator CalculateOfflineProgressionCoroutine()
        {
            yield return new WaitForEndOfFrame(); // Wait for other systems to initialize

            // This would normally load the last save time from save data
            // For now, we'll simulate no offline time
            DateTime lastPlayTime = _lastSaveTime;
            DateTime currentTime = DateTime.Now;
            TimeSpan offlineTime = currentTime - lastPlayTime;

            if (offlineTime.TotalMinutes > 1.0) // Only calculate if offline for more than 1 minute
            {
                LogDebug($"Calculating offline progression for {offlineTime.TotalHours:F2} hours");

                float offlineHours = (float)offlineTime.TotalHours;
                
                // Notify offline progression listeners
                NotifyOfflineProgressionListeners(offlineHours);
                _onOfflineProgressionCalculated?.Raise(offlineHours);

                LogDebug($"Offline progression calculated: {offlineHours:F2} hours processed");
            }
            else
            {
                LogDebug("No significant offline time detected");
            }
        }

        /// <summary>
        /// Tracks frame time for performance monitoring.
        /// </summary>
        private void TrackFrameTime()
        {
            _frameTimeHistory.Enqueue(Time.unscaledDeltaTime);
            
            if (_frameTimeHistory.Count > MAX_FRAME_HISTORY)
            {
                _frameTimeHistory.Dequeue();
            }
        }

        /// <summary>
        /// Registers a listener for time scale changes.
        /// </summary>
        public void RegisterTimeScaleListener(ITimeScaleListener listener)
        {
            if (listener != null && !_timeScaleListeners.Contains(listener))
            {
                _timeScaleListeners.Add(listener);
                LogDebug($"Registered time scale listener: {listener.GetType().Name}");
            }
        }

        /// <summary>
        /// Unregisters a time scale listener.
        /// </summary>
        public void UnregisterTimeScaleListener(ITimeScaleListener listener)
        {
            if (_timeScaleListeners.Remove(listener))
            {
                LogDebug($"Unregistered time scale listener: {listener.GetType().Name}");
            }
        }

        /// <summary>
        /// Registers a listener for offline progression events.
        /// </summary>
        public void RegisterOfflineProgressionListener(IOfflineProgressionListener listener)
        {
            if (listener != null && !_offlineProgressionListeners.Contains(listener))
            {
                _offlineProgressionListeners.Add(listener);
                LogDebug($"Registered offline progression listener: {listener.GetType().Name}");
            }
        }

        /// <summary>
        /// Unregisters an offline progression listener.
        /// </summary>
        public void UnregisterOfflineProgressionListener(IOfflineProgressionListener listener)
        {
            if (_offlineProgressionListeners.Remove(listener))
            {
                LogDebug($"Unregistered offline progression listener: {listener.GetType().Name}");
            }
        }

        /// <summary>
        /// Notifies all time scale listeners of a change.
        /// </summary>
        private void NotifyTimeScaleListeners(float previousScale, float newScale)
        {
            for (int i = _timeScaleListeners.Count - 1; i >= 0; i--)
            {
                try
                {
                    _timeScaleListeners[i]?.OnTimeScaleChanged(previousScale, newScale);
                }
                catch (Exception e)
                {
                    LogError($"Error notifying time scale listener: {e.Message}");
                    _timeScaleListeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Notifies all offline progression listeners.
        /// </summary>
        private void NotifyOfflineProgressionListeners(float offlineHours)
        {
            for (int i = _offlineProgressionListeners.Count - 1; i >= 0; i--)
            {
                try
                {
                    _offlineProgressionListeners[i]?.OnOfflineProgressionCalculated(offlineHours);
                }
                catch (Exception e)
                {
                    LogError($"Error notifying offline progression listener: {e.Message}");
                    _offlineProgressionListeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Handles game state changes from the GameManager.
        /// </summary>
        public void OnGameStateChanged(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Paused:
                    Pause();
                    break;
                case GameState.Running:
                    if (previousState == GameState.Paused)
                    {
                        Resume();
                    }
                    break;
                case GameState.Saving:
                    _lastSaveTime = DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// Converts real-world time to accelerated game time.
        /// </summary>
        public float RealTimeToGameTime(float realTime)
        {
            return realTime * CurrentTimeScale;
        }

        /// <summary>
        /// Converts accelerated game time to real-world time.
        /// </summary>
        public float GameTimeToRealTime(float gameTime)
        {
            return gameTime / CurrentTimeScale;
        }

        /// <summary>
        /// Gets the time scale-adjusted delta time for frame-rate independent calculations.
        /// </summary>
        public float GetScaledDeltaTime()
        {
            return Time.unscaledDeltaTime * CurrentTimeScale;
        }

        /// <summary>
        /// Gets the current time scale as a formatted string for UI display.
        /// </summary>
        public string GetTimeScaleDisplayString()
        {
            if (CurrentTimeScale < 1.0f)
            {
                return $"1/{(1.0f / CurrentTimeScale):F1}x";
            }
            else
            {
                return $"{CurrentTimeScale:F1}x";
            }
        }
    }

    /// <summary>
    /// Interface for systems that need to respond to time scale changes.
    /// </summary>
    public interface ITimeScaleListener
    {
        void OnTimeScaleChanged(float previousScale, float newScale);
    }

    /// <summary>
    /// Interface for systems that need to process offline progression.
    /// </summary>
    public interface IOfflineProgressionListener
    {
        void OnOfflineProgressionCalculated(float offlineHours);
    }

    /// <summary>
    /// Configuration for time management behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "Time Config", menuName = "Project Chimera/Core/Time Config")]
    public class TimeConfigSO : ChimeraConfigSO
    {
        [Header("Time Scale Settings")]
        [Range(0.1f, 1.0f)]
        public float DefaultTimeScale = 1.0f;
        
        [Range(1.0f, 1000.0f)]
        public float MaxTimeScale = 100.0f;
        
        [Range(0.01f, 1.0f)]
        public float MinTimeScale = 0.1f;

        [Header("Offline Progression")]
        public bool EnableOfflineProgression = true;
        
        [Range(1.0f, 168.0f)] // 1 hour to 1 week
        public float MaxOfflineHours = 72.0f;
        
        [Range(0.1f, 10.0f)]
        public float OfflineProgressionMultiplier = 0.5f;

        [Header("Performance")]
        public bool EnableFrameTimeTracking = true;
        public bool EnableTimeDebugLogging = false;
        
        [Range(30, 120)]
        public int FrameHistorySize = 60;
    }
}