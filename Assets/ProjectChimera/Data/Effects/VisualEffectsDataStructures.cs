using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectChimera.Data.Effects
{
    /// <summary>
    /// Visual effects and celebration data structures for gaming experiences
    /// Focuses on particle effects, UI animations, achievement celebrations, and feedback systems
    /// Transforms gameplay achievements into visually stunning and emotionally rewarding experiences
    /// </summary>
    
    #region Achievement Celebration Data
    
    [System.Serializable]
    public class AchievementCelebration
    {
        public string CelebrationID;
        public string CelebrationName;
        public string Description;
        public AchievementType AchievementType;
        public CelebrationIntensity Intensity;
        public List<VisualEffect> VisualEffects = new List<VisualEffect>();
        public List<AudioEffect> AudioEffects = new List<AudioEffect>();
        public List<UIAnimation> UIAnimations = new List<UIAnimation>();
        public CelebrationDuration Duration;
        public bool IsRepeatable;
        public float DelayBeforeStart;
        public CelebrationTrigger Trigger;
    }
    
    [System.Serializable]
    public class VisualEffect
    {
        public string EffectID;
        public string EffectName;
        public EffectType EffectType;
        public Vector3 Position;
        public Vector3 Scale;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public float Duration;
        public float Intensity;
        public EffectBehavior Behavior;
        public List<EffectKeyframe> Keyframes = new List<EffectKeyframe>();
        public bool FollowTarget;
        public string TargetObjectID;
    }
    
    [System.Serializable]
    public class AudioEffect
    {
        public string AudioID;
        public string AudioName;
        public AudioClip AudioClip;
        public float Volume;
        public float Pitch;
        public bool Loop;
        public float FadeInTime;
        public float FadeOutTime;
        public AudioMixerGroup MixerGroup;
        public SpatialAudioSettings SpatialSettings;
    }
    
    [System.Serializable]
    public class UIAnimation
    {
        public string AnimationID;
        public string AnimationName;
        public UIAnimationType AnimationType;
        public string TargetUIElement;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 StartScale;
        public Vector3 EndScale;
        public Color StartColor;
        public Color EndColor;
        public float Duration;
        public AnimationCurve EasingCurve;
        public bool PingPong;
        public int LoopCount;
    }
    
    #endregion
    
    #region Feedback Effects Data
    
    [System.Serializable]
    public class FeedbackEffect
    {
        public string FeedbackID;
        public string FeedbackName;
        public FeedbackType FeedbackType;
        public FeedbackIntensity Intensity;
        public List<VisualFeedback> VisualFeedbacks = new List<VisualFeedback>();
        public List<HapticFeedback> HapticFeedbacks = new List<HapticFeedback>();
        public List<ScreenEffect> ScreenEffects = new List<ScreenEffect>();
        public FeedbackTrigger Trigger;
        public float Duration;
        public bool IsContextual;
    }
    
    [System.Serializable]
    public class VisualFeedback
    {
        public string FeedbackID;
        public VisualFeedbackType FeedbackType;
        public Color EffectColor;
        public float Intensity;
        public float Duration;
        public Vector3 Position;
        public float Radius;
        public FeedbackPattern Pattern;
        public AnimationCurve IntensityCurve;
    }
    
    [System.Serializable]
    public class HapticFeedback
    {
        public string HapticID;
        public HapticType HapticType;
        public float Intensity;
        public float Duration;
        public HapticPattern Pattern;
        public float Frequency;
        public List<HapticPulse> Pulses = new List<HapticPulse>();
    }
    
    [System.Serializable]
    public class ScreenEffect
    {
        public string EffectID;
        public ScreenEffectType EffectType;
        public float Intensity;
        public float Duration;
        public Color EffectColor;
        public ScreenBlendMode BlendMode;
        public AnimationCurve EffectCurve;
        public bool AffectUIOnly;
    }
    
    #endregion
    
    #region Progress Visualization Data
    
    [System.Serializable]
    public class ProgressVisualization
    {
        public string VisualizationID;
        public string VisualizationName;
        public ProgressVisualizationType VisualizationType;
        public ProgressDisplayMode DisplayMode;
        public Vector3 Position;
        public Vector3 Scale;
        public ProgressBarConfig ProgressBar;
        public ParticleTrailConfig ParticleTrail;
        public GlowEffectConfig GlowEffect;
        public NumberCounterConfig NumberCounter;
        public bool AnimateChanges;
        public float AnimationDuration;
    }
    
    [System.Serializable]
    public class ProgressBarConfig
    {
        public Color FillColor;
        public Color BackgroundColor;
        public Color BorderColor;
        public float BorderWidth;
        public ProgressBarStyle Style;
        public FillDirection FillDirection;
        public bool ShowPercentage;
        public bool ShowNumbers;
        public GradientConfig GradientFill;
    }
    
    [System.Serializable]
    public class ParticleTrailConfig
    {
        public Color StartColor;
        public Color EndColor;
        public float ParticleSize;
        public int ParticleCount;
        public float Speed;
        public float Lifetime;
        public ParticleShape Shape;
        public EmissionPattern EmissionPattern;
    }
    
    [System.Serializable]
    public class GlowEffectConfig
    {
        public Color GlowColor;
        public float GlowIntensity;
        public float GlowRadius;
        public GlowPulseMode PulseMode;
        public float PulseSpeed;
        public AnimationCurve PulseCurve;
    }
    
    [System.Serializable]
    public class NumberCounterConfig
    {
        public CounterStyle CounterStyle;
        public Font Font;
        public int FontSize;
        public Color TextColor;
        public Color OutlineColor;
        public float OutlineWidth;
        public bool AnimateDigits;
        public float CountDuration;
        public string NumberFormat;
    }
    
    #endregion
    
    #region Particle System Data
    
    [System.Serializable]
    public class ParticleSystemConfig
    {
        public string SystemID;
        public string SystemName;
        public ParticleSystemType SystemType;
        public EmissionConfig Emission;
        public ParticleShapeConfig Shape;
        public VelocityConfig Velocity;
        public LifetimeConfig Lifetime;
        public ColorConfig Color;
        public SizeConfig Size;
        public RotationConfig Rotation;
        public GravityConfig Gravity;
        public CollisionConfig Collision;
        public TextureConfig Texture;
    }
    
    [System.Serializable]
    public class EmissionConfig
    {
        public float Rate;
        public int Bursts;
        public float BurstInterval;
        public EmissionShape EmissionShape;
        public Vector3 EmissionDirection;
        public float EmissionAngle;
        public bool EmitOverTime;
        public AnimationCurve EmissionCurve;
    }
    
    [System.Serializable]
    public class ParticleShapeConfig
    {
        public ParticleShape Shape;
        public Vector3 Scale;
        public float Radius;
        public float Arc;
        public bool EmitFromShell;
        public bool RandomizeDirection;
    }
    
    [System.Serializable]
    public class VelocityConfig
    {
        public Vector3 InitialVelocity;
        public float SpeedVariation;
        public VelocityMode VelocityMode;
        public AnimationCurve VelocityCurve;
        public bool InheritVelocity;
        public float Damping;
    }
    
    [System.Serializable]
    public class LifetimeConfig
    {
        public float MinLifetime;
        public float MaxLifetime;
        public LifetimeMode LifetimeMode;
        public AnimationCurve LifetimeCurve;
        public bool FadeOut;
        public float FadeOutDuration;
    }
    
    [System.Serializable]
    public class ColorConfig
    {
        public Gradient ColorGradient;
        public ColorMode ColorMode;
        public float ColorVariation;
        public bool UseRandomHue;
        public float HueShift;
    }
    
    [System.Serializable]
    public class SizeConfig
    {
        public float StartSize;
        public float EndSize;
        public AnimationCurve SizeCurve;
        public float SizeVariation;
        public bool UniformScale;
    }
    
    [System.Serializable]
    public class RotationConfig
    {
        public float StartRotation;
        public float RotationSpeed;
        public bool RandomizeRotation;
        public RotationMode RotationMode;
        public AnimationCurve RotationCurve;
    }
    
    [System.Serializable]
    public class GravityConfig
    {
        public Vector3 GravityDirection;
        public float GravityStrength;
        public bool UseGlobalGravity;
        public GravityMode GravityMode;
    }
    
    [System.Serializable]
    public class CollisionConfig
    {
        public bool EnableCollision;
        public LayerMask CollisionLayers;
        public float Bounciness;
        public float LifetimeLoss;
        public bool CreateSubEmitters;
        public ParticleSystemConfig SubEmitterConfig;
    }
    
    [System.Serializable]
    public class TextureConfig
    {
        public Texture2D ParticleTexture;
        public Material ParticleMaterial;
        public TextureMode TextureMode;
        public int FrameCount;
        public float AnimationSpeed;
        public bool RandomFrame;
    }
    
    #endregion
    
    #region Camera Effects Data
    
    [System.Serializable]
    public class CameraEffect
    {
        public string EffectID;
        public string EffectName;
        public CameraEffectType EffectType;
        public float Duration;
        public float Intensity;
        public CameraShakeConfig Shake;
        public CameraZoomConfig Zoom;
        public CameraColorConfig ColorGrading;
        public PostProcessingConfig PostProcessing;
        public bool AffectMainCamera;
        public bool AffectUICamera;
    }
    
    [System.Serializable]
    public class CameraShakeConfig
    {
        public Vector3 ShakeDirection;
        public float ShakeIntensity;
        public float ShakeFrequency;
        public AnimationCurve ShakeCurve;
        public ShakeMode ShakeMode;
        public bool FadeOut;
    }
    
    [System.Serializable]
    public class CameraZoomConfig
    {
        public float StartZoom;
        public float EndZoom;
        public float ZoomDuration;
        public AnimationCurve ZoomCurve;
        public ZoomMode ZoomMode;
        public bool ReturnToOriginal;
    }
    
    [System.Serializable]
    public class CameraColorConfig
    {
        public Color ColorFilter;
        public float Saturation;
        public float Brightness;
        public float Contrast;
        public float Hue;
        public AnimationCurve ColorCurve;
    }
    
    [System.Serializable]
    public class PostProcessingConfig
    {
        public BloomConfig Bloom;
        public VignetteConfig Vignette;
        public ChromaticAberrationConfig ChromaticAberration;
        public MotionBlurConfig MotionBlur;
        public DepthOfFieldConfig DepthOfField;
    }
    
    #endregion
    
    #region Supporting Classes
    
    [System.Serializable]
    public class EffectKeyframe
    {
        public float Time;
        public Vector3 Position;
        public Vector3 Scale;
        public Color Color;
        public float Intensity;
        public AnimationCurve EasingCurve;
    }
    
    [System.Serializable]
    public class SpatialAudioSettings
    {
        public bool Enable3D;
        public float MinDistance;
        public float MaxDistance;
        public AudioRolloffMode RolloffMode;
        public AnimationCurve CustomRolloff;
        public float DopplerLevel;
        public float Spread;
    }
    
    [System.Serializable]
    public class HapticPulse
    {
        public float StartTime;
        public float Duration;
        public float Intensity;
        public HapticWaveform Waveform;
    }
    
    [System.Serializable]
    public class GradientConfig
    {
        public Gradient Gradient;
        public GradientDirection Direction;
        public bool UseWorldSpace;
        public Vector2 Offset;
        public Vector2 Scale;
    }
    
    [System.Serializable]
    public class BloomConfig
    {
        public bool Enabled;
        public float Intensity;
        public float Threshold;
        public float SoftKnee;
        public float Clamp;
        public float Diffusion;
        public float AnamorphicRatio;
        public Color Color;
    }
    
    [System.Serializable]
    public class VignetteConfig
    {
        public bool Enabled;
        public Color Color;
        public Vector2 Center;
        public float Intensity;
        public float Smoothness;
        public float Roundness;
        public bool Rounded;
    }
    
    [System.Serializable]
    public class ChromaticAberrationConfig
    {
        public bool Enabled;
        public float Intensity;
        public bool FastMode;
    }
    
    [System.Serializable]
    public class MotionBlurConfig
    {
        public bool Enabled;
        public float Shutter;
        public int SampleCount;
    }
    
    [System.Serializable]
    public class DepthOfFieldConfig
    {
        public bool Enabled;
        public float FocusDistance;
        public float Aperture;
        public float FocalLength;
        public int MaxBlurSize;
        public bool HighQuality;
    }
    
    [System.Serializable]
    public class CelebrationDuration
    {
        public float PreDelay;
        public float MainDuration;
        public float PostDelay;
        public float TotalDuration;
        public bool HasFadeIn;
        public bool HasFadeOut;
        public float FadeInDuration;
        public float FadeOutDuration;
    }
    
    #endregion
    
    #region Enums
    
    public enum AchievementType
    {
        Cultivation_Achievement,
        Genetics_Achievement,
        Economics_Achievement,
        Construction_Achievement,
        AI_Achievement,
        Community_Achievement,
        Competition_Achievement,
        Milestone_Achievement,
        Rare_Achievement,
        Legendary_Achievement
    }
    
    public enum CelebrationIntensity
    {
        Subtle,
        Moderate,
        Impressive,
        Spectacular,
        Epic,
        Legendary
    }
    
    public enum CelebrationTrigger
    {
        Immediate,
        Delayed,
        User_Activated,
        Proximity_Based,
        Time_Based,
        Condition_Based
    }
    
    public enum EffectType
    {
        Particle_Burst,
        Particle_Trail,
        Light_Flash,
        Glow_Effect,
        Explosion,
        Fireworks,
        Sparkles,
        Confetti,
        Energy_Wave,
        Magic_Circle
    }
    
    public enum EffectBehavior
    {
        Static,
        Animated,
        Physics_Based,
        Orbital,
        Following,
        Expanding,
        Contracting,
        Pulsing
    }
    
    public enum FeedbackType
    {
        Success_Feedback,
        Error_Feedback,
        Warning_Feedback,
        Progress_Feedback,
        Interaction_Feedback,
        Achievement_Feedback,
        System_Feedback,
        Contextual_Feedback
    }
    
    public enum FeedbackIntensity
    {
        Minimal,
        Subtle,
        Moderate,
        Strong,
        Intense,
        Maximum
    }
    
    public enum FeedbackTrigger
    {
        Button_Press,
        Hover,
        Success,
        Error,
        Progress_Change,
        Achievement_Unlock,
        System_Event,
        Custom_Event
    }
    
    public enum VisualFeedbackType
    {
        Color_Flash,
        Border_Highlight,
        Glow_Effect,
        Shake_Effect,
        Scale_Pulse,
        Fade_Effect,
        Ripple_Effect,
        Particle_Burst
    }
    
    public enum FeedbackPattern
    {
        Single,
        Double,
        Triple,
        Rapid_Fire,
        Escalating,
        Fading,
        Rhythmic,
        Random
    }
    
    public enum HapticType
    {
        Light_Impact,
        Medium_Impact,
        Heavy_Impact,
        Notification,
        Success,
        Warning,
        Error,
        Selection
    }
    
    public enum HapticPattern
    {
        Single_Pulse,
        Double_Pulse,
        Triple_Pulse,
        Long_Press,
        Heartbeat,
        Notification,
        Error_Buzz,
        Success_Chime
    }
    
    public enum HapticWaveform
    {
        Sine,
        Square,
        Triangle,
        Sawtooth,
        Noise,
        Custom
    }
    
    public enum ScreenEffectType
    {
        Flash,
        Fade,
        Vignette,
        Blur,
        Distortion,
        Color_Grading,
        Chromatic_Aberration,
        Scanlines
    }
    
    public enum ScreenBlendMode
    {
        Normal,
        Additive,
        Multiply,
        Screen,
        Overlay,
        Soft_Light,
        Hard_Light,
        Color_Dodge
    }
    
    public enum UIAnimationType
    {
        Fade,
        Scale,
        Move,
        Rotate,
        Color_Change,
        Bounce,
        Elastic,
        Swing,
        Pulse,
        Flip
    }
    
    public enum ProgressVisualizationType
    {
        Progress_Bar,
        Circular_Progress,
        Particle_Trail,
        Number_Counter,
        Glow_Effect,
        Fill_Animation,
        Growing_Bar,
        Animated_Icons
    }
    
    public enum ProgressDisplayMode
    {
        Percentage,
        Fraction,
        Numbers_Only,
        Visual_Only,
        Hybrid,
        Custom_Format
    }
    
    public enum ProgressBarStyle
    {
        Simple_Bar,
        Rounded_Bar,
        Segmented_Bar,
        Gradient_Bar,
        Textured_Bar,
        Animated_Bar,
        Particle_Bar,
        Custom_Shape
    }
    
    public enum FillDirection
    {
        Left_To_Right,
        Right_To_Left,
        Bottom_To_Top,
        Top_To_Bottom,
        Center_Outward,
        Radial_Clockwise,
        Radial_Counter_Clockwise,
        Custom_Path
    }
    
    public enum GlowPulseMode
    {
        None,
        Constant,
        Slow_Pulse,
        Fast_Pulse,
        Breathing,
        Random,
        Synchronized,
        Heartbeat
    }
    
    public enum CounterStyle
    {
        Digital,
        Analog,
        Handwritten,
        Neon,
        Holographic,
        Vintage,
        Modern,
        Custom
    }
    
    public enum ParticleSystemType
    {
        Achievement_Burst,
        Progress_Trail,
        Success_Sparkles,
        Error_Smoke,
        Magic_Effect,
        Celebration_Confetti,
        Energy_Flow,
        Ambient_Particles
    }
    
    public enum EmissionShape
    {
        Point,
        Line,
        Circle,
        Sphere,
        Box,
        Cone,
        Custom_Mesh,
        Edge_Emission
    }
    
    public enum ParticleShape
    {
        Sphere,
        Cube,
        Plane,
        Circle,
        Star,
        Heart,
        Custom_Mesh,
        Sprite
    }
    
    public enum VelocityMode
    {
        Constant,
        Random,
        Curve_Based,
        Physics_Based,
        Directional,
        Orbital,
        Turbulent,
        Custom
    }
    
    public enum LifetimeMode
    {
        Constant,
        Random_Range,
        Curve_Based,
        Distance_Based,
        Collision_Based,
        Custom_Logic
    }
    
    public enum ColorMode
    {
        Solid_Color,
        Gradient,
        Random_Colors,
        Texture_Based,
        Emission_Based,
        Custom_Shader
    }
    
    public enum RotationMode
    {
        None,
        Constant_Speed,
        Random_Speed,
        Curve_Based,
        Velocity_Aligned,
        Camera_Facing,
        Custom_Axis
    }
    
    public enum GravityMode
    {
        Global_Gravity,
        Local_Gravity,
        No_Gravity,
        Custom_Force,
        Attraction_Point,
        Repulsion_Point,
        Orbital_Force
    }
    
    public enum TextureMode
    {
        Single_Texture,
        Animated_Texture,
        Random_Texture,
        Flipbook_Animation,
        Mesh_Particles,
        Custom_Shader
    }
    
    public enum EmissionPattern
    {
        Continuous,
        Burst,
        Waves,
        Spiral,
        Random,
        Rhythmic,
        Triggered,
        Custom
    }
    
    public enum CameraEffectType
    {
        Shake,
        Zoom,
        Color_Grading,
        Post_Processing,
        Focus_Pull,
        Lens_Flare,
        Screen_Distortion,
        Custom_Effect
    }
    
    public enum ShakeMode
    {
        Position_Only,
        Rotation_Only,
        Both,
        Custom_Axis,
        Perlin_Noise,
        Random_Direction,
        Directional,
        Orbital
    }
    
    public enum ZoomMode
    {
        Field_Of_View,
        Camera_Distance,
        Orthographic_Size,
        Custom_Zoom,
        Smooth_Zoom,
        Instant_Zoom,
        Elastic_Zoom
    }
    
    public enum GradientDirection
    {
        Horizontal,
        Vertical,
        Diagonal_Up,
        Diagonal_Down,
        Radial,
        Custom_Angle
    }
    
    public enum EffectQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }
    
    #endregion
}