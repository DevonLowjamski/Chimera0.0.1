using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Time Transition Configuration - Configuration for time scale transitions and temporal state changes
    /// Defines smooth time transitions, temporal effects, and transition animations
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Time Transition Config", menuName = "Project Chimera/Cultivation/Time Transition Config")]
    public class TimeTransitionConfigSO : ChimeraConfigSO
    {
        [Header("Transition Settings")]
        [Range(0.1f, 10f)] public float DefaultTransitionDuration = 1f;
        [Range(0.1f, 5f)] public float FastTransitionDuration = 0.3f;
        [Range(1f, 20f)] public float SlowTransitionDuration = 3f;
        
        [Header("Transition Curves")]
        public AnimationCurve AccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve DecelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve SmoothTransitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Header("Transition Types")]
        public List<TimeTransitionDefinition> TransitionDefinitions = new List<TimeTransitionDefinition>();
        
        [Header("Visual Effects")]
        public List<TransitionEffect> TransitionEffects = new List<TransitionEffect>();
        
        [Header("Audio Settings")]
        [Range(0f, 1f)] public float TransitionAudioVolume = 0.5f;
        public bool EnableTransitionSounds = true;
        public List<AudioClip> TransitionSounds = new List<AudioClip>();
        
        public TimeTransitionDefinition GetTransitionDefinition(TransitionType transitionType)
        {
            return TransitionDefinitions.Find(t => t.TransitionType == transitionType);
        }
        
        public float GetTransitionDuration(TransitionType transitionType, float fromScale, float toScale)
        {
            var definition = GetTransitionDefinition(transitionType);
            if (definition == null) return DefaultTransitionDuration;
            
            var scaleDifference = Mathf.Abs(toScale - fromScale);
            return definition.BaseDuration * (1f + scaleDifference * definition.ScaleInfluence);
        }
    }
    
    [System.Serializable]
    public class TimeTransitionDefinition
    {
        public string TransitionName;
        public TransitionType TransitionType;
        [Range(0.1f, 10f)] public float BaseDuration = 1f;
        [Range(0f, 2f)] public float ScaleInfluence = 0.5f;
        public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool EnableVisualEffects = true;
        public bool EnableAudioEffects = true;
        public string Description;
    }
    
    [System.Serializable]
    public class TransitionEffect
    {
        public string EffectName;
        public TransitionType ApplicableTransition;
        public EffectType EffectType;
        [Range(0f, 1f)] public float EffectIntensity = 0.5f;
        [Range(0f, 10f)] public float EffectDuration = 1f;
        public Color EffectColor = Color.white;
        public GameObject EffectPrefab;
        public AnimationCurve EffectCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
    
    public enum TransitionType
    {
        Instant,
        Fast,
        Normal,
        Smooth,
        Cinematic,
        Custom
    }
    
    public enum EffectType
    {
        ScreenFade,
        ParticleEffect,
        MotionBlur,
        ColorGrading,
        ScreenDistortion,
        LightFlash,
        CameraShake,
        PostProcessing
    }
}