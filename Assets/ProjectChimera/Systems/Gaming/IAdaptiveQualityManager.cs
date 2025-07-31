using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Interface for Adaptive Quality Manager - Dynamic quality adjustment for gameplay
    /// Part of Module 1: Gaming Experience Core - Deliverable 2.2
    /// 
    /// Provides intelligent, real-time quality adjustments to maintain optimal gaming performance
    /// while preserving visual quality as much as possible. Works in conjunction with Gaming Performance Monitor.
    /// </summary>
    public interface IAdaptiveQualityManager : IGamingService
    {
        #region Quality Configuration
        
        /// <summary>
        /// Enable/disable adaptive quality adjustments
        /// </summary>
        bool EnableAdaptiveQuality { get; set; }
        
        /// <summary>
        /// Target performance level to maintain
        /// </summary>
        GamingPerformanceState TargetPerformanceLevel { get; set; }
        
        /// <summary>
        /// How aggressive should quality adjustments be (0.0 = conservative, 1.0 = aggressive)
        /// </summary>
        float AggressivenessLevel { get; set; }
        
        /// <summary>
        /// Minimum quality level - system won't reduce quality below this
        /// </summary>
        QualityLevel MinimumQualityLevel { get; set; }
        
        /// <summary>
        /// Maximum quality level - system won't increase quality above this
        /// </summary>
        QualityLevel MaximumQualityLevel { get; set; }
        
        #endregion
        
        #region Current State
        
        /// <summary>
        /// Current quality level being applied
        /// </summary>
        QualityLevel CurrentQualityLevel { get; }
        
        /// <summary>
        /// Target quality level based on current performance
        /// </summary>
        QualityLevel TargetQualityLevel { get; }
        
        /// <summary>
        /// Current quality adjustment mode
        /// </summary>
        QualityAdjustmentMode CurrentAdjustmentMode { get; }
        
        /// <summary>
        /// Is the system currently making quality adjustments?
        /// </summary>
        bool IsAdjustingQuality { get; }
        
        /// <summary>
        /// Time since last quality adjustment
        /// </summary>
        float TimeSinceLastAdjustment { get; }
        
        #endregion
        
        #region Quality Settings Control
        
        /// <summary>
        /// Performance impact weights for each quality setting
        /// </summary>
        QualityPerformanceImpacts PerformanceImpacts { get; set; }
        
        /// <summary>
        /// Visual importance weights for each quality setting
        /// </summary>
        QualityVisualImportance VisualImportance { get; set; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when quality level changes
        /// </summary>
        event Action<QualityChangeEventData> OnQualityChanged;
        
        /// <summary>
        /// Event fired when automatic adjustment is made
        /// </summary>
        event Action<QualityAdjustmentEventData> OnQualityAdjusted;
        
        /// <summary>
        /// Event fired when quality hits minimum/maximum limits
        /// </summary>
        event Action<QualityLimitEventData> OnQualityLimitReached;
        
        #endregion
        
        #region Manual Controls
        
        /// <summary>
        /// Manually set quality level (disables adaptive mode temporarily)
        /// </summary>
        void SetQualityLevel(QualityLevel level);
        
        /// <summary>
        /// Force immediate quality adjustment based on current performance
        /// </summary>
        void ForceQualityAdjustment();
        
        /// <summary>
        /// Reset to default quality settings
        /// </summary>
        void ResetToDefaults();
        
        /// <summary>
        /// Apply quality preset (Low, Medium, High, Ultra)
        /// </summary>
        void ApplyQualityPreset(QualityPreset preset);
        
        #endregion
        
        #region Analysis & Diagnostics
        
        /// <summary>
        /// Get comprehensive quality analysis
        /// </summary>
        QualityAnalysisData GetQualityAnalysis();
        
        /// <summary>
        /// Get history of quality adjustments
        /// </summary>
        List<QualityAdjustmentRecord> GetAdjustmentHistory(int count = 50);
        
        /// <summary>
        /// Get performance impact estimation for quality changes
        /// </summary>
        float EstimatePerformanceImpact(QualityLevel fromLevel, QualityLevel toLevel);
        
        /// <summary>
        /// Get recommended quality settings for current hardware
        /// </summary>
        QualityLevel GetRecommendedQualityLevel();
        
        #endregion
    }
    
    /// <summary>
    /// Quality levels for adaptive management
    /// </summary>
    public enum QualityLevel
    {
        Ultra = 0,      // Highest quality
        High = 1,       // High quality
        Medium = 2,     // Balanced quality
        Low = 3,        // Performance-focused
        Minimum = 4     // Lowest possible quality
    }
    
    /// <summary>
    /// Quality presets for quick application
    /// </summary>
    public enum QualityPreset
    {
        Ultra,
        High, 
        Medium,
        Low,
        Custom
    }
    
    /// <summary>
    /// Quality adjustment modes
    /// </summary>
    public enum QualityAdjustmentMode
    {
        Stable,         // No adjustments needed
        Upgrading,      // Increasing quality
        Downgrading,    // Decreasing quality
        Emergency       // Rapid quality reduction for critical performance
    }
    
    /// <summary>
    /// Individual quality settings that can be adjusted
    /// </summary>
    [Serializable]
    public class AdaptiveQualitySettings
    {
        [Header("Rendering Quality")]
        public int TextureQuality = 0;           // 0=full, 1=half, 2=quarter, 3=eighth
        public int AnisotropicFiltering = 2;     // 0=disabled, 1=enabled, 2=forced
        public int AntiAliasing = 4;             // 0=disabled, 2=2x, 4=4x, 8=8x
        public float RenderScale = 1.0f;         // 0.5-2.0 render resolution scale
        
        [Header("Shadows")]
        public ShadowQuality ShadowQuality = ShadowQuality.All;
        public ShadowResolution ShadowResolution = ShadowResolution.VeryHigh;
        public int ShadowCascades = 4;           // 1-4 cascade splits
        public float ShadowDistance = 150f;      // Shadow rendering distance
        
        [Header("Lighting")]
        public int PixelLightCount = 4;          // Number of pixel lights
        public bool RealtimeReflectionProbes = true;
        public bool SoftParticles = true;
        public bool SoftVegetation = true;
        
        [Header("Post-Processing")]
        public bool BloomEnabled = true;
        public bool MotionBlurEnabled = true;
        public bool DepthOfFieldEnabled = true;
        public bool ColorGradingEnabled = true;
        public bool VignetteEnabled = true;
        
        [Header("Level of Detail")]
        public float LODBias = 2.0f;             // LOD distance multiplier
        public int MaximumLODLevel = 0;          // Maximum LOD level to use
        public int ParticleRaycastBudget = 4096; // Particle collision budget
        
        [Header("VSync and Frame Rate")]
        public int VSyncCount = 0;               // 0=off, 1=every frame, 2=every 2nd frame
        public int TargetFrameRate = 60;         // Target frame rate
        
        public AdaptiveQualitySettings Clone()
        {
            return (AdaptiveQualitySettings)this.MemberwiseClone();
        }
    }
    
    /// <summary>
    /// Performance impact weights for each quality setting (0.0-1.0)
    /// </summary>
    [Serializable]
    public class QualityPerformanceImpacts
    {
        [Header("Rendering Performance Impact")]
        public float TextureQuality = 0.3f;
        public float AnisotropicFiltering = 0.1f;
        public float AntiAliasing = 0.4f;
        public float RenderScale = 0.8f;
        
        [Header("Shadow Performance Impact")]
        public float ShadowQuality = 0.3f;
        public float ShadowResolution = 0.5f;
        public float ShadowCascades = 0.2f;
        public float ShadowDistance = 0.2f;
        
        [Header("Lighting Performance Impact")]
        public float PixelLightCount = 0.3f;
        public float RealtimeReflectionProbes = 0.2f;
        public float SoftParticles = 0.1f;
        public float SoftVegetation = 0.1f;
        
        [Header("Post-Processing Performance Impact")]
        public float PostProcessingEffects = 0.2f;
        
        [Header("LOD Performance Impact")]
        public float LODBias = 0.2f;
        public float ParticleComplexity = 0.1f;
    }
    
    /// <summary>
    /// Visual importance weights for each quality setting (0.0-1.0)
    /// </summary>
    [Serializable]
    public class QualityVisualImportance
    {
        [Header("Visual Importance Weights")]
        public float TextureQuality = 0.8f;      // High visual impact
        public float AntiAliasing = 0.6f;        // Medium-high visual impact
        public float ShadowQuality = 0.7f;       // High visual impact
        public float LightingQuality = 0.8f;     // Very high visual impact
        public float PostProcessing = 0.5f;      // Medium visual impact
        public float ParticleEffects = 0.4f;     // Medium-low visual impact
        public float LODQuality = 0.6f;          // Medium-high visual impact
    }
    
    /// <summary>
    /// Quality change event data
    /// </summary>
    [Serializable]
    public struct QualityChangeEventData
    {
        public QualityLevel PreviousLevel;
        public QualityLevel NewLevel;
        public QualityAdjustmentMode AdjustmentMode;
        public string Reason;
        public float PerformanceGain;
        public float VisualImpact;
        public DateTime Timestamp;
        
        public override string ToString()
        {
            return $"Quality: {PreviousLevel} -> {NewLevel} ({AdjustmentMode}) - {Reason}";
        }
    }
    
    /// <summary>
    /// Quality adjustment event data
    /// </summary>
    [Serializable]
    public struct QualityAdjustmentEventData
    {
        public string SettingName;
        public object PreviousValue;
        public object NewValue;
        public float EstimatedPerformanceGain;
        public float EstimatedVisualImpact;
        public GamingPerformanceState TriggeringPerformanceState;
        public DateTime Timestamp;
        
        public override string ToString()
        {
            return $"Adjusted {SettingName}: {PreviousValue} -> {NewValue} (Perf: +{EstimatedPerformanceGain:F2})";
        }
    }
    
    /// <summary>
    /// Quality limit reached event data
    /// </summary>
    [Serializable]
    public struct QualityLimitEventData
    {
        public QualityLevel LimitReached;
        public bool IsMinimumLimit;
        public GamingPerformanceState CurrentPerformance;
        public string RecommendedAction;
        public DateTime Timestamp;
        
        public override string ToString()
        {
            return $"Quality limit reached: {LimitReached} ({(IsMinimumLimit ? "Minimum" : "Maximum")}) - {RecommendedAction}";
        }
    }
    
    /// <summary>
    /// Quality analysis data
    /// </summary>
    [Serializable]
    public class QualityAnalysisData
    {
        [Header("Current State")]
        public QualityLevel CurrentLevel;
        public QualityLevel RecommendedLevel;
        public float OverallQualityScore;      // 0-100
        public float PerformanceEfficiency;    // 0-100
        
        [Header("System Analysis")]
        public bool IsOptimalForHardware;
        public bool CanIncreaseQuality;
        public bool ShouldDecreaseQuality;
        public string BottleneckComponent;     // "GPU", "CPU", "Memory", etc.
        
        [Header("Adjustment History")]
        public int TotalAdjustments;
        public int AdjustmentsLastMinute;
        public float AverageTimeBetweenAdjustments;
        public QualityLevel MostUsedLevel;
        
        [Header("Recommendations")]
        public List<string> RecommendedChanges;
        public List<string> PerformanceWarnings;
        public string OptimizationSuggestions;
        
        public override string ToString()
        {
            return $"Quality Analysis: {CurrentLevel} (Score: {OverallQualityScore:F1}, Efficiency: {PerformanceEfficiency:F1}%)";
        }
    }
    
    /// <summary>
    /// Record of a quality adjustment
    /// </summary>
    [Serializable]
    public struct QualityAdjustmentRecord
    {
        public DateTime Timestamp;
        public QualityLevel FromLevel;
        public QualityLevel ToLevel;
        public GamingPerformanceState TriggeringPerformance;
        public float FPSBefore;
        public float FPSAfter;
        public string AdjustmentDetails;
        public bool WasSuccessful;
        
        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} - {FromLevel}->{ToLevel} | FPS: {FPSBefore:F1}->{FPSAfter:F1} | {AdjustmentDetails}";
        }
    }
}