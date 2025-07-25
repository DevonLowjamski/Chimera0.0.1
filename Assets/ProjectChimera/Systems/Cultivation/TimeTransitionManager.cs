using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Manages time transitions and acceleration mechanics
    /// </summary>
    public class TimeTransitionManager : MonoBehaviour
    {
        [Header("Time Configuration")]
        [SerializeField] private TimeTransitionConfigSO _transitionConfig;
        
        private bool _isInitialized = false;
        private float _currentTimeScale = 1f;
        
        public float CurrentTimeScale => _currentTimeScale;
        
        public void Initialize(TimeTransitionConfigSO config)
        {
            _transitionConfig = config;
            _isInitialized = true;
        }
        
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update time transition system
        }
        
        public void SetTimeScale(float scale)
        {
            _currentTimeScale = Mathf.Clamp(scale, 0.1f, 10f);
            Time.timeScale = _currentTimeScale;
        }
        
        public void PauseTime()
        {
            SetTimeScale(0f);
        }
        
        public void ResumeTime()
        {
            SetTimeScale(1f);
        }
        
        public bool RequestTimeScaleChange(GameTimeScale newScale)
        {
            // Convert GameTimeScale enum to float multiplier
            float multiplier = newScale switch
            {
                GameTimeScale.Paused => 0f,
                GameTimeScale.Normal => 1f,
                GameTimeScale.Fast => 2f,
                GameTimeScale.VeryFast => 4f,
                GameTimeScale.UltraFast => 8f,
                _ => 1f
            };
            
            SetTimeScale(multiplier);
            return true;
        }
        
        // Time transition state management
        private TimeTransitionState _transitionState = TimeTransitionState.Stable;
        private GameTimeScale _fromScale = GameTimeScale.Normal;
        private GameTimeScale _toScale = GameTimeScale.Normal;
        private float _transitionStartTime = 0f;
        private float _transitionDuration = 1f;
        
        public void ProcessTransition()
        {
            if (_transitionState == TimeTransitionState.Starting)
            {
                _transitionState = TimeTransitionState.Transitioning;
                _transitionStartTime = Time.unscaledTime;
            }
            else if (_transitionState == TimeTransitionState.Transitioning)
            {
                float elapsed = Time.unscaledTime - _transitionStartTime;
                float progress = Mathf.Clamp01(elapsed / _transitionDuration);
                
                // Interpolate between time scales
                float fromMultiplier = GetTimeScaleMultiplier(_fromScale);
                float toMultiplier = GetTimeScaleMultiplier(_toScale);
                float currentMultiplier = Mathf.Lerp(fromMultiplier, toMultiplier, progress);
                
                SetTimeScale(currentMultiplier);
                
                if (progress >= 1f)
                {
                    _transitionState = TimeTransitionState.Stable;
                }
            }
        }
        
        public bool IsTransitionComplete()
        {
            return _transitionState == TimeTransitionState.Stable;
        }
        
        public void StartTransition(GameTimeScale fromScale, GameTimeScale toScale)
        {
            _fromScale = fromScale;
            _toScale = toScale;
            _transitionState = TimeTransitionState.Starting;
            
            // Calculate transition duration based on scale difference
            float fromMultiplier = GetTimeScaleMultiplier(fromScale);
            float toMultiplier = GetTimeScaleMultiplier(toScale);
            float scaleDifference = Mathf.Abs(toMultiplier - fromMultiplier);
            _transitionDuration = Mathf.Max(0.5f, scaleDifference * 0.5f);
        }
        
        private float GetTimeScaleMultiplier(GameTimeScale scale)
        {
            return scale switch
            {
                GameTimeScale.Paused => 0f,
                GameTimeScale.Slow => 0.5f,
                GameTimeScale.SlowMotion => 0.5f,
                GameTimeScale.Normal => 1f,
                GameTimeScale.Baseline => 1f,
                GameTimeScale.Standard => 2f,
                GameTimeScale.Fast => 4f,
                GameTimeScale.VeryFast => 8f,
                GameTimeScale.UltraFast => 8f,
                GameTimeScale.Lightning => 12f,
                _ => 1f
            };
        }
    }
}