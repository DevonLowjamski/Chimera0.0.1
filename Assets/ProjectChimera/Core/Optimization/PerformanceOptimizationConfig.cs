using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// PC015: Centralized performance optimization configuration for large-scale plant simulation
    /// Ensures all optimization systems are properly configured for 60 FPS with 1000+ plants
    /// </summary>
    [CreateAssetMenu(fileName = "PerformanceOptimizationConfig", menuName = "Project Chimera/Optimization/Performance Config")]
    public class PerformanceOptimizationConfig : ChimeraConfigSO
    {
        [Header("Performance Targets")]
        [Tooltip("Target frame rate for the simulation")]
        public int TargetFrameRate = 60;
        
        [Tooltip("Maximum number of plants before aggressive optimization kicks in")]
        public int MaxPlantCount = 1000;
        
        [Tooltip("Frame time budget in milliseconds (16.67ms = 60 FPS)")]
        public float FrameTimeBudgetMS = 16.67f;
        
        [Tooltip("Minimum acceptable FPS before quality scaling")]
        public float MinAcceptableFPS = 55f;
        
        [Header("LOD (Level of Detail) Settings")]
        [Tooltip("Enable dynamic LOD based on distance and performance")]
        public bool EnableDynamicLOD = true;
        
        [Tooltip("Distance thresholds for LOD levels")]
        public LODSettings LODConfiguration = new LODSettings();
        
        [Header("Culling Settings")]
        [Tooltip("Enable frustum culling (plants outside camera view)")]
        public bool EnableFrustumCulling = true;
        
        [Tooltip("Enable distance culling for far plants")]
        public bool EnableDistanceCulling = true;
        
        [Tooltip("Enable occlusion culling for hidden plants")]
        public bool EnableOcclusionCulling = true;
        
        [Tooltip("Maximum visible distance for plants")]
        public float MaxVisibleDistance = 100f;
        
        [Header("Update Optimization")]
        [Tooltip("Enable priority-based plant update scheduling")]
        public bool EnablePriorityUpdates = true;
        
        [Tooltip("Maximum plants to update per frame")]
        public int MaxPlantsPerFrameUpdate = 50;
        
        [Tooltip("Percentage of frame time allocated to plant updates")]
        [Range(0.1f, 0.5f)]
        public float PlantUpdateFrameBudget = 0.3f;
        
        [Tooltip("Update frequency scaling based on distance")]
        public AnimationCurve DistanceUpdateScaling = AnimationCurve.Linear(0f, 1f, 100f, 0.1f);
        
        [Header("Batching and Instancing")]
        [Tooltip("Enable GPU instancing for similar plants")]
        public bool EnableGPUInstancing = true;
        
        [Tooltip("Enable dynamic batching")]
        public bool EnableDynamicBatching = true;
        
        [Tooltip("Maximum batch size for plant rendering")]
        public int MaxBatchSize = 100;
        
        [Header("Memory Optimization")]
        [Tooltip("Enable object pooling for frequently created objects")]
        public bool EnableObjectPooling = true;
        
        [Tooltip("Enable texture streaming for distant plants")]
        public bool EnableTextureStreaming = true;
        
        [Tooltip("Maximum memory usage in MB before cleanup")]
        public float MaxMemoryUsageMB = 1024f;
        
        [Tooltip("Enable automatic garbage collection management")]
        public bool EnableGCManagement = true;
        
        [Header("Quality Scaling")]
        [Tooltip("Enable automatic quality reduction when performance drops")]
        public bool EnableDynamicQuality = true;
        
        [Tooltip("Quality levels available for scaling")]
        public QualityLevel[] QualityLevels = new QualityLevel[]
        {
            new QualityLevel { Name = "Ultra", LODDistanceMultiplier = 1.0f, UpdateFrequencyMultiplier = 1.0f },
            new QualityLevel { Name = "High", LODDistanceMultiplier = 0.8f, UpdateFrequencyMultiplier = 0.8f },
            new QualityLevel { Name = "Medium", LODDistanceMultiplier = 0.6f, UpdateFrequencyMultiplier = 0.6f },
            new QualityLevel { Name = "Low", LODDistanceMultiplier = 0.4f, UpdateFrequencyMultiplier = 0.4f }
        };
        
        [Header("SpeedTree Specific")]
        [Tooltip("Enable SpeedTree wind system")]
        public bool EnableSpeedTreeWind = true;
        
        [Tooltip("Enable SpeedTree shadows")]
        public bool EnableSpeedTreeShadows = true;
        
        [Tooltip("SpeedTree LOD transition speed")]
        [Range(0.1f, 2.0f)]
        public float SpeedTreeLODTransitionSpeed = 1.0f;
        
        [Header("Debug and Monitoring")]
        [Tooltip("Enable performance monitoring and metrics collection")]
        public bool EnablePerformanceMonitoring = true;
        
        [Tooltip("Enable debug overlays for optimization systems")]
        public bool EnableDebugOverlays = false;
        
        [Tooltip("Log performance warnings")]
        public bool LogPerformanceWarnings = true;
        
        [Tooltip("Performance monitoring update frequency in seconds")]
        public float MonitoringUpdateFrequency = 1.0f;
        
        /// <summary>
        /// Get the appropriate quality level based on current performance
        /// </summary>
        public QualityLevel GetQualityLevelForFPS(float currentFPS)
        {
            if (currentFPS >= TargetFrameRate * 0.95f)
                return QualityLevels[0]; // Ultra
            else if (currentFPS >= TargetFrameRate * 0.85f)
                return QualityLevels[1]; // High
            else if (currentFPS >= TargetFrameRate * 0.70f)
                return QualityLevels[2]; // Medium
            else
                return QualityLevels[3]; // Low
        }
        
        /// <summary>
        /// Calculate the maximum plants that should be visible based on target performance
        /// </summary>
        public int GetMaxVisiblePlantsForPerformance(float currentFPS)
        {
            float performanceRatio = currentFPS / TargetFrameRate;
            return Mathf.RoundToInt(MaxPlantCount * Mathf.Clamp(performanceRatio, 0.3f, 1.0f));
        }
        
        /// <summary>
        /// Get update frequency multiplier based on distance from camera
        /// </summary>
        public float GetUpdateFrequencyForDistance(float distance)
        {
            return DistanceUpdateScaling.Evaluate(distance);
        }
        
        /// <summary>
        /// Validate configuration values
        /// </summary>
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (TargetFrameRate < 30 || TargetFrameRate > 120)
            {
                Debug.LogWarning($"PerformanceOptimizationConfig: Target frame rate {TargetFrameRate} is outside recommended range (30-120)");
                isValid = false;
            }
            
            if (MaxPlantCount < 100)
            {
                Debug.LogWarning($"PerformanceOptimizationConfig: Max plant count {MaxPlantCount} seems low for large-scale simulation");
                isValid = false;
            }
            
            if (PlantUpdateFrameBudget > 0.5f)
            {
                Debug.LogWarning($"PerformanceOptimizationConfig: Plant update frame budget {PlantUpdateFrameBudget} may cause frame drops");
                isValid = false;
            }
            
            if (QualityLevels.Length == 0)
            {
                Debug.LogError("PerformanceOptimizationConfig: No quality levels defined");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Public wrapper for configuration validation
        /// </summary>
        public bool ValidateConfiguration()
        {
            return ValidateData();
        }
    }
    
    [System.Serializable]
    public class LODSettings
    {
        [Tooltip("Distance for LOD 0 (highest detail)")]
        public float LOD0Distance = 20f;
        
        [Tooltip("Distance for LOD 1")]
        public float LOD1Distance = 40f;
        
        [Tooltip("Distance for LOD 2")]
        public float LOD2Distance = 70f;
        
        [Tooltip("Distance for LOD 3 (billboard/lowest detail)")]
        public float LOD3Distance = 100f;
        
        [Tooltip("Enable smooth LOD transitions")]
        public bool EnableSmoothTransitions = true;
        
        [Tooltip("LOD transition fade time")]
        public float TransitionFadeTime = 0.5f;
    }
    
    [System.Serializable]
    public class QualityLevel
    {
        [Tooltip("Name of the quality level")]
        public string Name = "Medium";
        
        [Tooltip("Multiplier for LOD distances (lower = more aggressive LOD)")]
        [Range(0.1f, 2.0f)]
        public float LODDistanceMultiplier = 1.0f;
        
        [Tooltip("Multiplier for update frequency (lower = less frequent updates)")]
        [Range(0.1f, 2.0f)]
        public float UpdateFrequencyMultiplier = 1.0f;
        
        [Tooltip("Maximum visible plants for this quality level")]
        public int MaxVisiblePlants = 500;
        
        [Tooltip("Enable shadows for this quality level")]
        public bool EnableShadows = true;
        
        [Tooltip("Enable wind effects for this quality level")]
        public bool EnableWind = true;
        
        [Tooltip("Texture quality multiplier")]
        [Range(0.25f, 1.0f)]
        public float TextureQuality = 1.0f;
    }
}