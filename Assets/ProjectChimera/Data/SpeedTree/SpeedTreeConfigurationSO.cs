using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.SpeedTree
{
    /// <summary>
    /// ScriptableObject configurations for SpeedTree system.
    /// Defines all configuration assets needed by SpeedTree services.
    /// </summary>
    
    /// <summary>
    /// SpeedTree library configuration containing asset paths and references.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeLibrary", menuName = "Project Chimera/SpeedTree/Library Configuration")]
    public class SpeedTreeLibrarySO : ScriptableObject
    {
        [Header("Asset Library")]
        public List<string> AssetPaths = new List<string>();
        public List<GameObject> AssetPrefabs = new List<GameObject>();
        
        [Header("Cannabis Strains")]
        public List<string> CannabisStrainsAssetPaths = new List<string>();
        
        [Header("Library Settings")]
        public bool AutoLoadAssets = true;
        public bool EnableAssetCaching = true;
        public int MaxCachedAssets = 50;
        
        // Missing method
        public List<string> GetAllAssetPaths()
        {
            return AssetPaths;
        }
    }
    
    /// <summary>
    /// SpeedTree shader configuration for material properties.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeShaderConfig", menuName = "Project Chimera/SpeedTree/Shader Configuration")]
    public class SpeedTreeShaderConfigSO : ScriptableObject
    {
        [Header("Cannabis-Specific Shaders")]
        public Material CannabisLeafMaterial;
        public Material CannabisBudMaterial;
        public Material CannabisStemMaterial;
        
        [Header("Shader Properties")]
        public float HealthColorIntensity = 1f;
        public float StressColorIntensity = 0.5f;
        public float GrowthProgressInfluence = 1f;
        
        [Header("Environmental Response")]
        public bool EnableEnvironmentalTinting = true;
        public bool EnableStressVisualization = true;
        public bool EnableGrowthVisualization = true;
        
        [Header("Performance")]
        public bool EnableShaderLOD = true;
        public int MaxShaderComplexity = 3;
    }
    
    /// <summary>
    /// SpeedTree LOD configuration for performance optimization.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeLODConfig", menuName = "Project Chimera/SpeedTree/LOD Configuration")]
    public class SpeedTreeLODConfigSO : ScriptableObject
    {
        [Header("LOD Settings")]
        public bool EnableCrossFade = true;
        public float FadeOutLength = 5f;
        public bool AnimateOnCulling = false;
        
        [Header("Distance Thresholds")]
        public float LOD0Distance = 50f;
        public float LOD1Distance = 100f;
        public float LOD2Distance = 200f;
        public float CullDistance = 500f;
        
        [Header("Quality Levels")]
        public int MaxLODLevel = 3;
        public bool EnableImpostors = true;
        public float ImpostorDistance = 300f;
        
        // Aliases for compatibility
        public float CullingDistance => CullDistance;
        public float LODTransitionDistance => LOD1Distance;
    }
    
    /// <summary>
    /// SpeedTree genetics configuration for genetic variation visualization.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeGeneticsConfig", menuName = "Project Chimera/SpeedTree/Genetics Configuration")]
    public class SpeedTreeGeneticsConfigSO : ScriptableObject
    {
        [Header("Genetic Variation Ranges")]
        public Vector2 HeightVariationRange = new Vector2(0.8f, 1.2f);
        public Vector2 BranchingVariationRange = new Vector2(0.7f, 1.3f);
        public Vector2 LeafSizeVariationRange = new Vector2(0.9f, 1.1f);
        
        [Header("Color Genetics")]
        public bool EnableColorVariation = true;
        public Color[] GeneticColorPalette = new Color[5];
        public float ColorVariationIntensity = 0.3f;
        
        [Header("Growth Characteristics")]
        public Vector2 GrowthRateRange = new Vector2(0.5f, 1.5f);
        public bool EnableMorphologicalVariation = true;
        public float VariationSmoothness = 0.8f;
    }
    
    /// <summary>
    /// SpeedTree growth configuration for animation and lifecycle.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeGrowthConfig", menuName = "Project Chimera/SpeedTree/Growth Configuration")]
    public class SpeedTreeGrowthConfigSO : ScriptableObject
    {
        [Header("Growth Animation")]
        public float BaseGrowthRate = 0.01f;
        public bool EnableGrowthAnimation = true;
        public float AnimationUpdateFrequency = 0.1f;
        
        [Header("Stage Durations (in game hours)")]
        public float SeedStageDuration = 24f;
        public float GerminationStageDuration = 48f;
        public float SeedlingStageDuration = 168f; // 1 week
        public float VegetativeStageDuration = 504f; // 3 weeks
        public float FloweringStageDuration = 840f; // 5 weeks
        
        [Header("Growth Modifiers")]
        public AnimationCurve HealthGrowthModifier;
        public AnimationCurve EnvironmentalGrowthModifier;
        public AnimationCurve GeneticGrowthModifier;
        
        [Header("Visual Effects")]
        public bool EnableGrowthEffects = true;
        public GameObject GrowthParticleEffect;
        public AudioClip GrowthSoundEffect;
    }
    
    /// <summary>
    /// SpeedTree wind configuration for environmental response.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeWindConfig", menuName = "Project Chimera/SpeedTree/Wind Configuration")]
    public class SpeedTreeWindConfigSO : ScriptableObject
    {
        [Header("Wind Settings")]
        public float DefaultWindStrength = 1f;
        public Vector3 DefaultWindDirection = Vector3.forward;
        public float WindTurbulence = 0.5f;
        public float WindGustiness = 0.3f;
        
        [Header("Plant Response")]
        public bool EnableBending = true;
        public bool EnableFlutter = true;
        public float BendingStiffness = 0.8f;
        public float FlutterIntensity = 0.4f;
        
        [Header("Environmental Wind")]
        public bool EnableEnvironmentalWind = true;
        public float EnvironmentalWindScale = 1f;
        public AnimationCurve WindResponseCurve;
    }
    
    /// <summary>
    /// SpeedTree performance metrics configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreePerformanceMetrics", menuName = "Project Chimera/SpeedTree/Performance Metrics")]
    public class SpeedTreePerformanceMetricsSO : ScriptableObject
    {
        [Header("Performance Targets")]
        public int TargetFPS = 60;
        public int MaxConcurrentPlants = 1000;
        public float MaxFrameTime = 16.67f; // ms
        
        [Header("Monitoring")]
        public bool EnablePerformanceMonitoring = true;
        public float MonitoringInterval = 1f;
        public bool LogPerformanceWarnings = true;
        
        [Header("Optimization Thresholds")]
        public float CPUUsageThreshold = 80f;
        public float MemoryUsageThreshold = 1024f; // MB
        public int MaxDrawCalls = 500;
    }
    
    /// <summary>
    /// SpeedTree quality level settings for different performance tiers.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeQualityLevel", menuName = "Project Chimera/SpeedTree/Quality Level")]
    public class SpeedTreeQualityLevelSO : ScriptableObject
    {
        [Header("Quality Settings")]
        public string QualityName = "Medium";
        public int QualityLevel = 2;
        
        [Header("Rendering Quality")]
        public bool EnableHighQualityShaders = true;
        public bool EnableShadows = true;
        public bool EnableReflections = false;
        
        [Header("Performance Limits")]
        public int MaxPlantInstances = 500;
        public float MaxRenderDistance = 200f;
        public int MaxLODLevel = 2;
        
        [Header("Effects")]
        public bool EnableWindAnimation = true;
        public bool EnableGrowthAnimation = true;
        public bool EnableParticleEffects = false;
    }
    
    /// <summary>
    /// SpeedTree system report configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedTreeSystemReport", menuName = "Project Chimera/SpeedTree/System Report")]
    public class SpeedTreeSystemReportSO : ScriptableObject
    {
        [Header("Report Settings")]
        public bool EnableReporting = true;
        public float ReportInterval = 10f;
        public bool LogToConsole = false;
        public bool SaveToFile = true;
        
        [Header("Report Content")]
        public bool IncludePerformanceMetrics = true;
        public bool IncludePlantStatistics = true;
        public bool IncludeMemoryUsage = true;
        public bool IncludeErrorLog = true;
        
        [Header("File Settings")]
        public string ReportFileName = "speedtree_report";
        public string ReportDirectory = "Reports/SpeedTree/";
        public bool AppendTimestamp = true;
    }
}