using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Performance
{
    // Core LOD data structures
    [Serializable]
    public class LODObject
    {
        public GameObject GameObject;
        public Transform Transform;
        public int InstanceId;
        public LODConfiguration Configuration;
        public int CurrentLODLevel;
        public float LastUpdateTime;
        public bool IsVisible;
        public bool IsCulled;
        public float DistanceToCamera;
        public LODComponents LODComponents;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class LODConfiguration
    {
        public float DistanceMultiplier = 1.0f;
        public int BaseParticleCount = 100;
        public List<List<Mesh>> MeshLODs = new List<List<Mesh>>();
        public List<Material> LODMaterials = new List<Material>();
        public Dictionary<string, object> CustomProperties = new Dictionary<string, object>();
        public bool EnableShadowCasting = true;
        public bool EnableShadowReceiving = true;
        public bool EnableColliders = true;
        public float CullingDistance = 200.0f;
    }
    
    [Serializable]
    public class LODComponents
    {
        public List<Renderer> Renderers = new List<Renderer>();
        public List<MeshFilter> MeshFilters = new List<MeshFilter>();
        public List<ParticleSystem> ParticleSystems = new List<ParticleSystem>();
        public List<Collider> Colliders = new List<Collider>();
        public List<Component> PlantComponents = new List<Component>();
        public List<Light> Lights = new List<Light>();
        public List<AudioSource> AudioSources = new List<AudioSource>();
    }
    
    // Camera-related data structures
    [Serializable]
    public class CameraLODData
    {
        public Vector3 Position;
        public Vector3 Forward;
        public float FieldOfView;
        public float FarClipPlane;
        public float NearClipPlane;
        public Matrix4x4 ViewMatrix;
        public Matrix4x4 ProjectionMatrix;
        public Plane[] FrustumPlanes = new Plane[6];
        public DateTime LastUpdate;
    }
    
    // Performance and metrics structures
    [Serializable]
    public class LODSystemMetrics
    {
        public int ManagedObjects;
        public int ActiveCameras;
        public int TotalLODSwitches;
        public int TotalObjectsCulled;
        public float CurrentLODScale;
        public float CurrentFrameRate;
        public float TargetFrameRate;
        public float LODUpdateInterval;
        public DateTime LastUpdateTime;
        public Dictionary<int, int> LODLevelDistribution = new Dictionary<int, int>();
        public float MemoryUsage;
        public float CPUUsage;
        public int ObjectsProcessedPerFrame;
    }
    
    // LOD level definitions and settings
    [Serializable]
    public class LODLevelSettings
    {
        public int Level;
        public string Name;
        public float Distance;
        public float QualityMultiplier;
        public bool EnableShadows;
        public bool EnableReflections;
        public bool EnableParticles;
        public bool EnableColliders;
        public bool EnableAudio;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }
    
    // Adaptive scaling configuration
    [Serializable]
    public class AdaptiveScalingConfig
    {
        public bool EnableAdaptiveScaling = true;
        public float TargetFrameRate = 60.0f;
        public float PerformanceThreshold = 0.8f;
        public float ScalingFactor = 1.2f;
        public float ScalingCooldown = 2.0f;
        public float MinScale = 0.5f;
        public float MaxScale = 3.0f;
        public int FrameHistorySize = 30;
        public bool EnableGPUScaling = true;
        public bool EnableCPUScaling = true;
    }
    
    // Plant-specific LOD structures
    [Serializable]
    public class PlantLODSettings
    {
        public bool EnablePlantLOD = true;
        public float PlantLODMultiplier = 1.0f;
        public bool EnableLeafCulling = true;
        public bool EnableBranchOptimization = true;
        public float LeafCullingDistance = 25.0f;
        public float BranchCullingDistance = 50.0f;
        public bool EnableFlowerCulling = true;
        public bool EnableFruitCulling = true;
        public Dictionary<string, float> ComponentDistances = new Dictionary<string, float>();
    }
    
    // Occlusion and visibility structures
    [Serializable]
    public class VisibilityData
    {
        public bool IsVisible;
        public bool IsOccluded;
        public float LastVisibilityCheck;
        public List<Camera> VisibleFromCameras = new List<Camera>();
        public Bounds WorldBounds;
        public float ScreenSize;
        public Dictionary<Camera, float> CameraDistances = new Dictionary<Camera, float>();
    }
    
    // LOD transition settings
    [Serializable]
    public class LODTransitionSettings
    {
        public bool EnableSmoothTransitions = true;
        public float TransitionDuration = 0.5f;
        public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool EnableFadeTransitions = true;
        public bool EnableMorphTransitions = false;
        public float HysteresisDistance = 2.0f; // Prevent LOD flickering
    }
    
    // Batch processing structures
    [Serializable]
    public class LODBatch
    {
        public List<LODObject> Objects = new List<LODObject>();
        public int BatchSize = 50;
        public float ProcessingTime;
        public bool IsProcessing;
        public int Priority;
        public DateTime CreationTime;
    }
    
    // Memory management structures
    [Serializable]
    public class LODMemoryStats
    {
        public long TotalMemoryUsage;
        public long MeshMemoryUsage;
        public long TextureMemoryUsage;
        public long MaterialMemoryUsage;
        public Dictionary<int, long> LODLevelMemory = new Dictionary<int, long>();
        public int ObjectsInMemory;
        public DateTime LastMemoryCheck;
    }
    
    // Culling system structures
    [Serializable]
    public class CullingSettings
    {
        public bool EnableFrustumCulling = true;
        public bool EnableOcclusionCulling = true;
        public bool EnableDistanceCulling = true;
        public float MaxCullingDistance = 200.0f;
        public float OcclusionCheckInterval = 0.5f;
        public int MaxOcclusionChecksPerFrame = 20;
        public bool EnableHierarchicalCulling = true;
        public float SmallObjectCullingSize = 0.01f;
    }
    
    // Quality settings integration
    [Serializable]
    public class QualityLODSettings
    {
        public Dictionary<int, LODQualityLevel> QualityLevels = new Dictionary<int, LODQualityLevel>();
        public int CurrentQualityLevel;
        public bool AutoAdjustQuality = true;
        public float QualityAdjustmentThreshold = 5.0f;
    }
    
    [Serializable]
    public class LODQualityLevel
    {
        public string Name;
        public float LODDistanceMultiplier;
        public float ParticleCountMultiplier;
        public bool EnableShadows;
        public bool EnableReflections;
        public int MaxLODLevel;
        public float CullingDistance;
    }
    
    // Event system structures
    [Serializable]
    public class LODEvent
    {
        public string EventType;
        public int ObjectId;
        public int OldLODLevel;
        public int NewLODLevel;
        public float Distance;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }
    
    // Profiling and debugging structures
    [Serializable]
    public class LODProfileData
    {
        public Dictionary<int, float> LODLevelPerformance = new Dictionary<int, float>();
        public Dictionary<string, float> ComponentPerformance = new Dictionary<string, float>();
        public float TotalUpdateTime;
        public float AverageUpdateTime;
        public int UpdateCount;
        public List<float> FrameTimeHistory = new List<float>();
        public DateTime ProfilingStartTime;
    }
    
    // Configuration presets
    [Serializable]
    public class LODPreset
    {
        public string PresetName;
        public string Description;
        public List<LODLevelSettings> LODLevels = new List<LODLevelSettings>();
        public AdaptiveScalingConfig ScalingConfig;
        public PlantLODSettings PlantSettings;
        public CullingSettings CullingSettings;
        public bool IsDefault;
        public DateTime CreatedTime;
    }
    
    // Platform-specific settings
    [Serializable]
    public class PlatformLODSettings
    {
        public RuntimePlatform Platform;
        public LODPreset LODPreset;
        public float PerformanceMultiplier;
        public bool EnableMobileLOD;
        public bool EnableConsoleLOD;
        public Dictionary<string, object> PlatformSpecificSettings = new Dictionary<string, object>();
    }
    
    // Streaming and loading structures
    [Serializable]
    public class LODStreamingData
    {
        public bool EnableLODStreaming = false;
        public float StreamingDistance = 100.0f;
        public int MaxStreamingRequests = 5;
        public Queue<LODStreamingRequest> StreamingQueue = new Queue<LODStreamingRequest>();
        public Dictionary<int, AssetReference> LODAssetReferences = new Dictionary<int, AssetReference>();
    }
    
    [Serializable]
    public class LODStreamingRequest
    {
        public int ObjectId;
        public int RequestedLODLevel;
        public int Priority;
        public DateTime RequestTime;
        public bool IsLoading;
        public float Progress;
    }
    
    [Serializable]
    public class AssetReference
    {
        public string AssetPath;
        public string AssetGUID;
        public UnityEngine.Object Asset;
        public bool IsLoaded;
        public DateTime LoadTime;
        public long MemorySize;
    }
    
    // Animation and transition structures
    [Serializable]
    public class LODTransition
    {
        public int ObjectId;
        public int FromLODLevel;
        public int ToLODLevel;
        public float TransitionProgress;
        public float TransitionSpeed;
        public bool IsTransitioning;
        public DateTime StartTime;
        public LODTransitionSettings Settings;
    }
    
    // Spatial partitioning for optimization
    [Serializable]
    public class LODSpatialNode
    {
        public Bounds Bounds;
        public List<LODObject> Objects = new List<LODObject>();
        public List<LODSpatialNode> Children = new List<LODSpatialNode>();
        public LODSpatialNode Parent;
        public int Depth;
        public bool IsDirty;
        public DateTime LastUpdate;
    }
}