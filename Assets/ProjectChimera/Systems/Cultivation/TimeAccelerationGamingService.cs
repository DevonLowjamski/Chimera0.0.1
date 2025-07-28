using UnityEngine;
using System;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Events.Core;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Service responsible for time acceleration gaming mechanics
    /// Handles time scale changes, transitions, and game speed control
    /// </summary>
    public class TimeAccelerationGamingService
    {
        private readonly TimeAccelerationGamingSystem _timeAccelerationSystem;
        private readonly TimeTransitionManager _timeTransitionManager;
        private readonly GameSpeedController _gameSpeedController;
        private readonly EnhancedCultivationGamingConfigSO _config;
        
        // Event channels
        private readonly GameEventChannelSO _onTimeScaleChanged;
        
        // State
        private bool _isInitialized = false;
        private GameTimeScale _currentTimeScale = GameTimeScale.Baseline;
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Time Acceleration Gaming Service";
        
        public TimeAccelerationGamingService(
            TimeAccelerationGamingSystem timeAccelerationSystem,
            TimeTransitionManager timeTransitionManager,
            GameSpeedController gameSpeedController,
            EnhancedCultivationGamingConfigSO config,
            GameEventChannelSO onTimeScaleChanged)
        {
            _timeAccelerationSystem = timeAccelerationSystem ?? throw new ArgumentNullException(nameof(timeAccelerationSystem));
            _timeTransitionManager = timeTransitionManager ?? throw new ArgumentNullException(nameof(timeTransitionManager));
            _gameSpeedController = gameSpeedController ?? throw new ArgumentNullException(nameof(gameSpeedController));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _onTimeScaleChanged = onTimeScaleChanged;
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[TimeAccelerationGamingService] Already initialized");
                return;
            }
            
            try
            {
                InitializeTimeAccelerationSystem();
                InitializeTimeTransitionManager();
                InitializeGameSpeedController();
                RegisterEventHandlers();
                
                _isInitialized = true;
                Debug.Log("TimeAccelerationGamingService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize TimeAccelerationGamingService: {ex.Message}");
                throw;
            }
        }
        
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            _isInitialized = false;
            
            Debug.Log("TimeAccelerationGamingService shutdown completed");
        }
        
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;
            
            _timeAccelerationSystem?.UpdateSystem();
        }
        
        private void InitializeTimeAccelerationSystem()
        {
            _timeAccelerationSystem?.Initialize(_config?.TimeAccelerationGamingConfig);
        }
        
        private void InitializeTimeTransitionManager()
        {
            _timeTransitionManager?.Initialize(_config?.TimeTransitionConfig);
        }
        
        private void InitializeGameSpeedController()
        {
            _gameSpeedController?.Initialize(_config?.TimeAccelerationGamingConfig);
        }
        
        private void RegisterEventHandlers()
        {
            if (_onTimeScaleChanged != null)
                _onTimeScaleChanged.OnEventRaisedWithData.AddListener(OnTimeScaleChanged);
        }
        
        private void UnregisterEventHandlers()
        {
            if (_onTimeScaleChanged != null)
                _onTimeScaleChanged.OnEventRaisedWithData.RemoveListener(OnTimeScaleChanged);
        }
        
        private void OnTimeScaleChanged(object eventData)
        {
            if (eventData is TimeScaleEventData timeData)
            {
                _currentTimeScale = timeData.NewTimeScale;
                ProcessTimeScaleChange(timeData);
            }
        }
        
        private void ProcessTimeScaleChange(TimeScaleEventData timeData)
        {
            var complexityMultiplier = CalculateComplexityMultiplier(timeData.NewTimeScale);
            // Apply complexity adjustments to related systems
        }
        
        private float CalculateComplexityMultiplier(GameTimeScale timeScale)
        {
            return timeScale switch
            {
                GameTimeScale.SlowMotion => 0.5f,
                GameTimeScale.Baseline => 1.0f,
                GameTimeScale.Standard => 1.2f,
                GameTimeScale.Fast => 1.5f,
                GameTimeScale.VeryFast => 2.0f,
                GameTimeScale.Lightning => 2.5f,
                _ => 1.0f
            };
        }
        
        // Public API
        public bool ChangeTimeScale(GameTimeScale newScale)
        {
            if (!_isInitialized) return false;
            return _timeTransitionManager?.RequestTimeScaleChange(newScale) ?? false;
        }
        
        public GameTimeScale GetCurrentTimeScale()
        {
            return _currentTimeScale;
        }
        
        public float GetComplexityMultiplier()
        {
            return CalculateComplexityMultiplier(_currentTimeScale);
        }
        
        public void AdjustGameplayComplexity(Action<float> complexityCallback)
        {
            if (!_isInitialized || complexityCallback == null) return;
            
            var multiplier = CalculateComplexityMultiplier(_currentTimeScale);
            complexityCallback.Invoke(multiplier);
        }
    }
}