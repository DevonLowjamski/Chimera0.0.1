using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Time Acceleration Gaming Configuration - ScriptableObject for time system settings
    /// Configures time scales, transitions, and gaming balance for cultivation systems
    /// </summary>
    [CreateAssetMenu(fileName = "TimeAccelerationGamingConfig", menuName = "Project Chimera/Cultivation/Time Acceleration Config")]
    public class TimeAccelerationGamingConfigSO : ScriptableObject
    {
        [Header("Time Scale Configuration")]
        [SerializeField] private TimeScaleSettings[] _timeScaleSettings = new TimeScaleSettings[]
        {
            new TimeScaleSettings { Scale = GameTimeScale.SlowMotion, Multiplier = 0.5f, Description = "Slow Motion" },
            new TimeScaleSettings { Scale = GameTimeScale.Baseline, Multiplier = 1.0f, Description = "Baseline" },
            new TimeScaleSettings { Scale = GameTimeScale.Standard, Multiplier = 2.0f, Description = "Standard" },
            new TimeScaleSettings { Scale = GameTimeScale.Fast, Multiplier = 4.0f, Description = "Fast" },
            new TimeScaleSettings { Scale = GameTimeScale.VeryFast, Multiplier = 8.0f, Description = "Very Fast" },
            new TimeScaleSettings { Scale = GameTimeScale.Lightning, Multiplier = 12.0f, Description = "Lightning" }
        };
        
        [Header("Transition Settings")]
        [Range(0.5f, 10.0f)] public float DefaultTransitionDuration = 3.0f;
        [Range(0.1f, 5.0f)] public float TransitionSmoothness = 1.5f;
        [Range(1.0f, 60.0f)] public float MinimumTimeAtScale = 15.0f;
        [Range(0.1f, 2.0f)] public float TransitionCooldown = 1.0f;
        
        [Header("Gaming Balance")]
        [Range(0.1f, 3.0f)] public float ManualTaskDifficultyMultiplier = 1.2f;
        [Range(0.1f, 3.0f)] public float AutomationAdvantageMultiplier = 1.5f;
        [Range(0.1f, 1.0f)] public float PlayerEngagementThreshold = 0.7f;
        [Range(1.0f, 20.0f)] public float OptimalActionsPerMinute = 5.0f;
        
        [Header("Experience and Progression")]
        [Range(0.1f, 5.0f)] public float ExperienceGainMultiplier = 1.0f;
        [Range(0.1f, 2.0f)] public float SkillProgressionRate = 1.0f;
        [Range(0.5f, 3.0f)] public float AutomationUnlockThreshold = 2.0f;
        
        [Header("Visual and Audio Feedback")]
        public bool EnableTransitionEffects = true;
        public bool EnableTimeScaleIndicator = true;
        public bool EnableAudioFeedback = true;
        [Range(0.1f, 2.0f)] public float FeedbackIntensity = 1.0f;
        
        // Public Properties
        public TimeScaleSettings[] TimeScaleSettings => _timeScaleSettings;
        
        public TimeScaleSettings GetTimeScaleSettings(GameTimeScale scale)
        {
            foreach (var setting in _timeScaleSettings)
            {
                if (setting.Scale == scale)
                    return setting;
            }
            
            return _timeScaleSettings[1]; // Return baseline as default
        }
        
        public float GetTimeMultiplier(GameTimeScale scale)
        {
            return GetTimeScaleSettings(scale).Multiplier;
        }
        
        public bool IsValidTransition(GameTimeScale from, GameTimeScale to)
        {
            // Allow all transitions for now, can add restrictions later
            return true;
        }
        
        public float GetTransitionDuration(GameTimeScale from, GameTimeScale to)
        {
            var multiplierDifference = Mathf.Abs(GetTimeMultiplier(to) - GetTimeMultiplier(from));
            return DefaultTransitionDuration * (1f + multiplierDifference * 0.1f);
        }
    }
    
    [System.Serializable]
    public class TimeScaleSettings
    {
        public GameTimeScale Scale;
        [Range(0.1f, 20.0f)] public float Multiplier = 1.0f;
        public string Description;
        [Range(0.1f, 3.0f)] public float ManualTaskDifficulty = 1.0f;
        [Range(0.1f, 3.0f)] public float AutomationAdvantage = 1.0f;
        public bool PlayerEngagementOptimal = true;
        [Range(1f, 1000f)] public float RealTimeDayDuration = 600f; // seconds
        [Range(1f, 200f)] public float GameDaysPerRealHour = 6f;
    }
    
    public enum GameTimeScale
    {
        Paused,
        Slow,
        SlowMotion,
        Normal,
        Baseline,
        Standard,
        Fast,
        VeryFast,
        UltraFast,
        Lightning
    }
    
    public enum TimeTransitionState
    {
        Stable,
        Starting,
        Transitioning,
        Locked
    }
}