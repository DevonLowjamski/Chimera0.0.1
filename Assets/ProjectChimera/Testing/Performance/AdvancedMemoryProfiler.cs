using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectChimera.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1c: Advanced memory profiler with deep Unity integration
    /// Provides detailed memory analysis using Unity's Profiler API and custom tracking
    /// </summary>
    public class AdvancedMemoryProfiler : MonoBehaviour
    {
        [Header("Profiling Configuration")]
        [SerializeField] private bool _enableDetailedProfiling = true;
        [SerializeField] private bool _enableObjectTracking = true;
        [SerializeField] private bool _enableTextureTracking = true;
        [SerializeField] private bool _enableMeshTracking = true;
        [SerializeField] private bool _enableAudioTracking = true;
        
        [Header("Tracking Intervals")]
        [SerializeField] private float _objectTrackingInterval = 5f;
        [SerializeField] private float _assetTrackingInterval = 10f;
        [SerializeField] private int _maxTrackedObjects = 1000;
        
        // Object tracking
        private Dictionary<Type, int> _objectCounts = new Dictionary<Type, int>();
        private Dictionary<Type, long> _objectMemoryUsage = new Dictionary<Type, long>();
        private List<UnityEngine.Object> _trackedObjects = new List<UnityEngine.Object>();
        
        // Asset tracking
        private Dictionary<string, AssetMemoryInfo> _assetMemoryTracking = new Dictionary<string, AssetMemoryInfo>();
        private Dictionary<Texture, TextureMemoryInfo> _textureTracking = new Dictionary<Texture, TextureMemoryInfo>();
        private Dictionary<Mesh, MeshMemoryInfo> _meshTracking = new Dictionary<Mesh, MeshMemoryInfo>();
        private Dictionary<AudioClip, AudioMemoryInfo> _audioTracking = new Dictionary<AudioClip, AudioMemoryInfo>();
        
        // Profiling data
        private List<DetailedMemorySnapshot> _detailedSnapshots = new List<DetailedMemorySnapshot>();
        private MemoryBreakdown _currentBreakdown;
        
        // Monitoring coroutines
        private Coroutine _objectTrackingCoroutine;
        private Coroutine _assetTrackingCoroutine;
        
        public static AdvancedMemoryProfiler Instance { get; private set; }
        
        // Events
        public event Action<DetailedMemorySnapshot> OnDetailedSnapshotTaken;
        public event Action<MemoryBreakdown> OnMemoryBreakdownUpdated;
        public event Action<AssetMemoryAlert> OnAssetMemoryAlert;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeProfiler();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            StartProfiling();
        }
        
        private void OnDestroy()
        {
            StopProfiling();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeProfiler()
        {
            // Initialize tracking dictionaries
            _objectCounts.Clear();
            _objectMemoryUsage.Clear();
            _assetMemoryTracking.Clear();
            _textureTracking.Clear();
            _meshTracking.Clear();
            _audioTracking.Clear();
            
            Debug.Log("[AdvancedMemoryProfiler] PC016-1c: Advanced memory profiler initialized");
        }
        
        #endregion
        
        #region Profiling Control
        
        public void StartProfiling()
        {
            if (_enableObjectTracking && _objectTrackingCoroutine == null)
            {
                _objectTrackingCoroutine = StartCoroutine(ContinuousObjectTracking());
            }
            
            if (_enableDetailedProfiling && _assetTrackingCoroutine == null)
            {
                _assetTrackingCoroutine = StartCoroutine(ContinuousAssetTracking());
            }
            
            Debug.Log("[AdvancedMemoryProfiler] PC016-1c: Profiling started");
        }
        
        public void StopProfiling()
        {
            if (_objectTrackingCoroutine != null)
            {
                StopCoroutine(_objectTrackingCoroutine);
                _objectTrackingCoroutine = null;
            }
            
            if (_assetTrackingCoroutine != null)
            {
                StopCoroutine(_assetTrackingCoroutine);
                _assetTrackingCoroutine = null;
            }
            
            Debug.Log("[AdvancedMemoryProfiler] PC016-1c: Profiling stopped");
        }
        
        #endregion
        
        #region Detailed Memory Snapshots
        
        public DetailedMemorySnapshot TakeDetailedSnapshot(string label = "Manual")
        {
            var snapshot = new DetailedMemorySnapshot
            {
                Timestamp = DateTime.Now,
                Label = label,
                GameTime = Time.time,
                
                // Basic Unity memory info
                TotalReservedMemory = Profiler.GetTotalReservedMemoryLong(),
                TotalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong(),
                TotalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong(),
                
                // Graphics memory
                GraphicsDriverAllocatedMemory = Profiler.GetAllocatedMemoryForGraphicsDriver(),
                
                // Managed heap
                MonoHeapSize = Profiler.GetMonoHeapSizeLong(),
                MonoUsedSize = Profiler.GetMonoUsedSizeLong(),
                
                // Temp allocator
                TempAllocatorSize = Profiler.GetTempAllocatorSize(),
                
                // Object counts
                ObjectCounts = new Dictionary<Type, int>(_objectCounts),
                ObjectMemoryUsage = new Dictionary<Type, long>(_objectMemoryUsage),
                
                // Asset memory breakdown
                AssetMemoryBreakdown = CreateAssetMemoryBreakdown(),
                
                // Unity-specific memory areas
                TextureMemory = CalculateTextureMemoryUsage(),
                MeshMemory = CalculateMeshMemoryUsage(),
                AudioMemory = CalculateAudioMemoryUsage(),
                AnimationMemory = 0, // Unity doesn't provide direct API
                PhysicsMemory = 0,   // Unity doesn't provide direct API
                
                // Render pipeline memory
                RenderTextureMemory = CalculateRenderTextureMemory(),
                ShaderMemory = 0,    // Unity doesn't provide direct API
                
                // System information
                SystemMemorySize = SystemInfo.systemMemorySize * 1024L * 1024L,
                GraphicsMemorySize = SystemInfo.graphicsMemorySize * 1024L * 1024L
            };
            
            _detailedSnapshots.Add(snapshot);
            
            // Maintain snapshot history
            if (_detailedSnapshots.Count > 100)
            {
                _detailedSnapshots.RemoveAt(0);
            }
            
            OnDetailedSnapshotTaken?.Invoke(snapshot);
            
            return snapshot;
        }
        
        #endregion
        
        #region Object Tracking
        
        private IEnumerator ContinuousObjectTracking()
        {
            while (_enableObjectTracking)
            {
                TrackUnityObjects();
                yield return new WaitForSeconds(_objectTrackingInterval);
            }
        }
        
        private void TrackUnityObjects()
        {
            _objectCounts.Clear();
            _objectMemoryUsage.Clear();
            _trackedObjects.Clear();
            
            // Find all Unity objects
            var allObjects = FindObjectsByType<UnityEngine.Object>(FindObjectsSortMode.None);
            
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;
                
                var type = obj.GetType();
                
                // Count objects by type
                if (!_objectCounts.ContainsKey(type))
                {
                    _objectCounts[type] = 0;
                    _objectMemoryUsage[type] = 0;
                }
                
                _objectCounts[type]++;
                
                // Estimate memory usage
                long memorySize = EstimateObjectMemorySize(obj);
                _objectMemoryUsage[type] += memorySize;
                
                // Track individual objects if under limit
                if (_trackedObjects.Count < _maxTrackedObjects)
                {
                    _trackedObjects.Add(obj);
                }
            }
            
            Debug.Log($"[AdvancedMemoryProfiler] PC016-1c: Tracked {allObjects.Length} objects across {_objectCounts.Count} types");
        }
        
        private long EstimateObjectMemorySize(UnityEngine.Object obj)
        {
            // Estimate memory size based on object type
            switch (obj)
            {
                case Texture texture:
                    return EstimateTextureMemorySize(texture);
                    
                case Mesh mesh:
                    return EstimateMeshMemorySize(mesh);
                    
                case AudioClip audioClip:
                    return EstimateAudioClipMemorySize(audioClip);
                    
                case GameObject gameObject:
                    return EstimateGameObjectMemorySize(gameObject);
                    
                case MonoBehaviour script:
                    return 1024; // Rough estimate for script instances
                    
                default:
                    return 256; // Default estimate for unknown objects
            }
        }
        
        #endregion
        
        #region Asset Tracking
        
        private IEnumerator ContinuousAssetTracking()
        {
            while (_enableDetailedProfiling)
            {
                if (_enableTextureTracking)
                    TrackTextureMemory();
                    
                if (_enableMeshTracking)
                    TrackMeshMemory();
                    
                if (_enableAudioTracking)
                    TrackAudioMemory();
                
                UpdateMemoryBreakdown();
                
                yield return new WaitForSeconds(_assetTrackingInterval);
            }
        }
        
        private void TrackTextureMemory()
        {
            _textureTracking.Clear();
            
            var textures = FindObjectsByType<Texture>(FindObjectsSortMode.None);
            
            foreach (var texture in textures)
            {
                if (texture == null) continue;
                
                var memoryInfo = new TextureMemoryInfo
                {
                    Texture = texture,
                    Width = texture.width,
                    Height = texture.height,
                    Format = texture.graphicsFormat.ToString(),
                    MipMapCount = texture.mipmapCount,
                    EstimatedMemorySize = EstimateTextureMemorySize(texture),
                    IsReadable = texture.isReadable
                };
                
                _textureTracking[texture] = memoryInfo;
            }
            
            Debug.Log($"[AdvancedMemoryProfiler] PC016-1c: Tracked {_textureTracking.Count} textures");
        }
        
        private void TrackMeshMemory()
        {
            _meshTracking.Clear();
            
            var meshes = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Select(mf => mf.sharedMesh)
                .Where(m => m != null)
                .Distinct();
            
            foreach (var mesh in meshes)
            {
                var memoryInfo = new MeshMemoryInfo
                {
                    Mesh = mesh,
                    VertexCount = mesh.vertexCount,
                    TriangleCount = mesh.triangles.Length / 3,
                    SubMeshCount = mesh.subMeshCount,
                    EstimatedMemorySize = EstimateMeshMemorySize(mesh),
                    IsReadable = mesh.isReadable
                };
                
                _meshTracking[mesh] = memoryInfo;
            }
            
            Debug.Log($"[AdvancedMemoryProfiler] PC016-1c: Tracked {_meshTracking.Count} meshes");
        }
        
        private void TrackAudioMemory()
        {
            _audioTracking.Clear();
            
            var audioClips = FindObjectsByType<AudioSource>(FindObjectsSortMode.None)
                .Select(source => source.clip)
                .Where(clip => clip != null)
                .Distinct();
            
            foreach (var clip in audioClips)
            {
                var memoryInfo = new AudioMemoryInfo
                {
                    AudioClip = clip,
                    Length = clip.length,
                    Frequency = clip.frequency,
                    Channels = clip.channels,
                    LoadType = clip.loadType.ToString(),
                    EstimatedMemorySize = EstimateAudioClipMemorySize(clip)
                };
                
                _audioTracking[clip] = memoryInfo;
            }
            
            Debug.Log($"[AdvancedMemoryProfiler] PC016-1c: Tracked {_audioTracking.Count} audio clips");
        }
        
        #endregion
        
        #region Memory Calculations
        
        private long EstimateTextureMemorySize(Texture texture)
        {
            if (texture == null) return 0;
            
            // Basic calculation: width * height * bytes per pixel * mip levels
            var pixelCount = texture.width * texture.height;
            var bytesPerPixel = GetBytesPerPixel(texture.graphicsFormat);
            var mipMultiplier = texture.mipmapCount > 1 ? 1.33f : 1f; // Approximation for mip chain
            
            return (long)(pixelCount * bytesPerPixel * mipMultiplier);
        }
        
        private long EstimateMeshMemorySize(Mesh mesh)
        {
            if (mesh == null) return 0;
            
            // Estimate based on vertex data
            var vertexSize = 32; // Rough estimate: position (12) + normal (12) + UV (8)
            var indexSize = mesh.triangles.Length * 4; // 4 bytes per index
            
            return mesh.vertexCount * vertexSize + indexSize;
        }
        
        private long EstimateAudioClipMemorySize(AudioClip clip)
        {
            if (clip == null) return 0;
            
            // PCM data size calculation
            var sampleCount = (long)(clip.frequency * clip.length * clip.channels);
            var bytesPerSample = 2; // 16-bit samples
            
            return sampleCount * bytesPerSample;
        }
        
        private long EstimateGameObjectMemorySize(GameObject gameObject)
        {
            long totalSize = 256; // Base GameObject overhead
            
            var components = gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                totalSize += EstimateComponentMemorySize(component);
            }
            
            return totalSize;
        }
        
        private long EstimateComponentMemorySize(Component component)
        {
            // Rough estimates based on component type
            switch (component)
            {
                case MeshRenderer _:
                    return 512;
                case MeshFilter meshFilter:
                    return meshFilter.sharedMesh != null ? EstimateMeshMemorySize(meshFilter.sharedMesh) / 10 : 128;
                case Collider _:
                    return 256;
                case Rigidbody _:
                    return 512;
                case AudioSource audioSource:
                    return audioSource.clip != null ? EstimateAudioClipMemorySize(audioSource.clip) / 10 : 128;
                default:
                    return 128; // Default component size
            }
        }
        
        private int GetBytesPerPixel(UnityEngine.Experimental.Rendering.GraphicsFormat format)
        {
            // Simplified mapping of common formats
            switch (format)
            {
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm:
                    return 4;
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm:
                    return 3;
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R5G6B5_UNormPack16:
                    return 2;
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm:
                    return 1;
                default:
                    return 4; // Default to 4 bytes
            }
        }
        
        #endregion
        
        #region Memory Breakdown Analysis
        
        private void UpdateMemoryBreakdown()
        {
            _currentBreakdown = new MemoryBreakdown
            {
                Timestamp = DateTime.Now,
                
                TextureMemory = CalculateTextureMemoryUsage(),
                MeshMemory = CalculateMeshMemoryUsage(),
                AudioMemory = CalculateAudioMemoryUsage(),
                
                GameObjectCount = _objectCounts.ContainsKey(typeof(GameObject)) ? _objectCounts[typeof(GameObject)] : 0,
                ComponentCount = _objectCounts.Where(kvp => typeof(Component).IsAssignableFrom(kvp.Key)).Sum(kvp => kvp.Value),
                
                LargestAssets = GetLargestAssets(),
                MemoryHotspots = IdentifyMemoryHotspots()
            };
            
            OnMemoryBreakdownUpdated?.Invoke(_currentBreakdown);
        }
        
        private long CalculateTextureMemoryUsage()
        {
            return _textureTracking.Values.Sum(info => info.EstimatedMemorySize);
        }
        
        private long CalculateMeshMemoryUsage()
        {
            return _meshTracking.Values.Sum(info => info.EstimatedMemorySize);
        }
        
        private long CalculateAudioMemoryUsage()
        {
            return _audioTracking.Values.Sum(info => info.EstimatedMemorySize);
        }
        
        private long CalculateRenderTextureMemory()
        {
            var renderTextures = FindObjectsByType<RenderTexture>(FindObjectsSortMode.None);
            return renderTextures.Sum(rt => EstimateTextureMemorySize(rt));
        }
        
        private AssetMemoryBreakdown CreateAssetMemoryBreakdown()
        {
            return new AssetMemoryBreakdown
            {
                TextureMemory = CalculateTextureMemoryUsage(),
                MeshMemory = CalculateMeshMemoryUsage(),
                AudioMemory = CalculateAudioMemoryUsage(),
                RenderTextureMemory = CalculateRenderTextureMemory(),
                TotalAssetMemory = CalculateTextureMemoryUsage() + CalculateMeshMemoryUsage() + CalculateAudioMemoryUsage()
            };
        }
        
        private List<AssetMemoryInfo> GetLargestAssets()
        {
            var largestAssets = new List<AssetMemoryInfo>();
            
            // Get largest textures
            largestAssets.AddRange(_textureTracking.Values
                .OrderByDescending(info => info.EstimatedMemorySize)
                .Take(5)
                .Select(info => new AssetMemoryInfo
                {
                    AssetName = info.Texture.name,
                    AssetType = "Texture",
                    MemorySize = info.EstimatedMemorySize
                }));
            
            // Get largest meshes
            largestAssets.AddRange(_meshTracking.Values
                .OrderByDescending(info => info.EstimatedMemorySize)
                .Take(5)
                .Select(info => new AssetMemoryInfo
                {
                    AssetName = info.Mesh.name,
                    AssetType = "Mesh",
                    MemorySize = info.EstimatedMemorySize
                }));
            
            return largestAssets.OrderByDescending(asset => asset.MemorySize).Take(10).ToList();
        }
        
        private List<MemoryHotspot> IdentifyMemoryHotspots()
        {
            var hotspots = new List<MemoryHotspot>();
            
            // Identify object type hotspots
            foreach (var kvp in _objectMemoryUsage.OrderByDescending(x => x.Value).Take(5))
            {
                hotspots.Add(new MemoryHotspot
                {
                    Category = "ObjectType",
                    Description = $"{kvp.Key.Name} ({_objectCounts[kvp.Key]} instances)",
                    MemoryUsage = kvp.Value,
                    Severity = kvp.Value > 50 * 1024 * 1024 ? "High" : kvp.Value > 10 * 1024 * 1024 ? "Medium" : "Low"
                });
            }
            
            return hotspots;
        }
        
        #endregion
        
        #region Public API
        
        public MemoryBreakdown GetCurrentMemoryBreakdown()
        {
            return _currentBreakdown;
        }
        
        public List<DetailedMemorySnapshot> GetDetailedSnapshots()
        {
            return new List<DetailedMemorySnapshot>(_detailedSnapshots);
        }
        
        public Dictionary<Type, int> GetObjectCounts()
        {
            return new Dictionary<Type, int>(_objectCounts);
        }
        
        public Dictionary<Type, long> GetObjectMemoryUsage()
        {
            return new Dictionary<Type, long>(_objectMemoryUsage);
        }
        
        public void GenerateDetailedReport()
        {
            var report = new StringBuilder();
            report.AppendLine("=== ADVANCED MEMORY PROFILER REPORT ===");
            report.AppendLine($"Generated: {DateTime.Now}");
            report.AppendLine();
            
            if (_currentBreakdown != null)
            {
                report.AppendLine("MEMORY BREAKDOWN:");
                report.AppendLine($"Textures: {_currentBreakdown.TextureMemory / (1024 * 1024):F1} MB");
                report.AppendLine($"Meshes: {_currentBreakdown.MeshMemory / (1024 * 1024):F1} MB");
                report.AppendLine($"Audio: {_currentBreakdown.AudioMemory / (1024 * 1024):F1} MB");
                report.AppendLine($"GameObjects: {_currentBreakdown.GameObjectCount}");
                report.AppendLine($"Components: {_currentBreakdown.ComponentCount}");
                report.AppendLine();
                
                if (_currentBreakdown.LargestAssets?.Count > 0)
                {
                    report.AppendLine("LARGEST ASSETS:");
                    foreach (var asset in _currentBreakdown.LargestAssets)
                    {
                        report.AppendLine($"  {asset.AssetType}: {asset.AssetName} - {asset.MemorySize / (1024 * 1024):F1} MB");
                    }
                    report.AppendLine();
                }
                
                if (_currentBreakdown.MemoryHotspots?.Count > 0)
                {
                    report.AppendLine("MEMORY HOTSPOTS:");
                    foreach (var hotspot in _currentBreakdown.MemoryHotspots)
                    {
                        report.AppendLine($"  {hotspot.Description}: {hotspot.MemoryUsage / (1024 * 1024):F1} MB ({hotspot.Severity})");
                    }
                }
            }
            
            Debug.Log(report.ToString());
        }
        
        public void ClearProfilerData()
        {
            _detailedSnapshots.Clear();
            _objectCounts.Clear();
            _objectMemoryUsage.Clear();
            _assetMemoryTracking.Clear();
            _textureTracking.Clear();
            _meshTracking.Clear();
            _audioTracking.Clear();
            
            Debug.Log("[AdvancedMemoryProfiler] PC016-1c: Profiler data cleared");
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [Serializable]
    public class DetailedMemorySnapshot
    {
        public DateTime Timestamp;
        public string Label;
        public float GameTime;
        
        // Unity Profiler data
        public long TotalReservedMemory;
        public long TotalAllocatedMemory;
        public long TotalUnusedReservedMemory;
        public long GraphicsDriverAllocatedMemory;
        public long MonoHeapSize;
        public long MonoUsedSize;
        public long TempAllocatorSize;
        
        // Object tracking
        public Dictionary<Type, int> ObjectCounts;
        public Dictionary<Type, long> ObjectMemoryUsage;
        
        // Asset breakdown
        public AssetMemoryBreakdown AssetMemoryBreakdown;
        
        // Specific memory areas
        public long TextureMemory;
        public long MeshMemory;
        public long AudioMemory;
        public long AnimationMemory;
        public long PhysicsMemory;
        public long RenderTextureMemory;
        public long ShaderMemory;
        
        // System info
        public long SystemMemorySize;
        public long GraphicsMemorySize;
    }
    
    [Serializable]
    public class AssetMemoryBreakdown
    {
        public long TextureMemory;
        public long MeshMemory;
        public long AudioMemory;
        public long RenderTextureMemory;
        public long TotalAssetMemory;
    }
    
    [Serializable]
    public class AssetMemoryInfo
    {
        public string AssetName;
        public string AssetType;
        public long MemorySize;
    }
    
    [Serializable]
    public class TextureMemoryInfo
    {
        public Texture Texture;
        public int Width;
        public int Height;
        public string Format;
        public int MipMapCount;
        public long EstimatedMemorySize;
        public bool IsReadable;
    }
    
    [Serializable]
    public class MeshMemoryInfo
    {
        public Mesh Mesh;
        public int VertexCount;
        public int TriangleCount;
        public int SubMeshCount;
        public long EstimatedMemorySize;
        public bool IsReadable;
    }
    
    [Serializable]
    public class AudioMemoryInfo
    {
        public AudioClip AudioClip;
        public float Length;
        public int Frequency;
        public int Channels;
        public string LoadType;
        public long EstimatedMemorySize;
    }
    
    [Serializable]
    public class MemoryBreakdown
    {
        public DateTime Timestamp;
        public long TextureMemory;
        public long MeshMemory;
        public long AudioMemory;
        public int GameObjectCount;
        public int ComponentCount;
        public List<AssetMemoryInfo> LargestAssets;
        public List<MemoryHotspot> MemoryHotspots;
    }
    
    [Serializable]
    public class MemoryHotspot
    {
        public string Category;
        public string Description;
        public long MemoryUsage;
        public string Severity;
    }
    
    [Serializable]
    public class AssetMemoryAlert
    {
        public string AssetName;
        public string AssetType;
        public long CurrentMemoryUsage;
        public long PreviousMemoryUsage;
        public string AlertType;
        public string Message;
    }
    
    #endregion
}