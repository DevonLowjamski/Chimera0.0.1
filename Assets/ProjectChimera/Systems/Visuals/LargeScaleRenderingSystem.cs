using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;

#if UNITY_SPEEDTREE
using UnityEngine.Rendering;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Large-scale rendering system designed to handle 1000+ cannabis plants
    /// with optimal performance. Implements advanced culling, batching, streaming,
    /// and GPU-accelerated rendering techniques for massive cultivation facilities.
    /// Integrates with Advanced LOD System and VFX controllers for seamless scaling.
    /// </summary>
    public class LargeScaleRenderingSystem : ChimeraManager
    {
        [Header("Large-Scale Rendering Configuration")]
        [SerializeField] private bool _enableLargeScaleRendering = true;
        [SerializeField] private bool _enableGPUInstancing = true;
        [SerializeField] private int _maxPlantsSupported = 5000;
        [SerializeField] private float _renderingUpdateFrequency = 10f; // Updates per second
        
        [Header("Rendering Zones")]
        [SerializeField] private bool _enableZonedRendering = true;
        [SerializeField] private Vector2 _zoneSize = new Vector2(50f, 50f);
        [SerializeField] private int _maxZonesRendered = 9; // 3x3 grid around camera
        [SerializeField] private float _zoneStreamingDistance = 100f;
        
        [Header("Plant Batching")]
        [SerializeField] private bool _enablePlantBatching = true;
        [SerializeField] private int _maxPlantsPerBatch = 100;
        [SerializeField] private bool _enableMaterialBatching = true;
        [SerializeField] private bool _enableMeshBatching = true;
        
        [Header("GPU Instancing Settings")]
        [SerializeField] private bool _enableGPUCulling = true;
        [SerializeField] private int _maxInstancesPerDrawCall = 1023; // GPU limit
        [SerializeField] private bool _enableFrustumCulling = true;
        [SerializeField] private bool _enableOcclusionCulling = true;
        
        [Header("Streaming & Memory")]
        [SerializeField] private bool _enableAssetStreaming = true;
        [SerializeField] private int _maxLoadedZones = 12;
        [SerializeField] private float _memoryBudgetMB = 500f;
        [SerializeField] private bool _enableAsyncLoading = true;
        
        [Header("Cannabis-Specific Optimizations")]
        [SerializeField] private bool _enableGrowthStageGrouping = true;
        [SerializeField] private bool _enableGeneticClustering = true;
        [SerializeField] private bool _enableSeasonalBatching = true;
        [SerializeField] private bool _enableHealthBasedCulling = true;
        
        [Header("Performance Targets")]
        [SerializeField, Range(30f, 120f)] private float _targetFrameRate = 60f;
        [SerializeField, Range(100, 5000)] private int _maxActiveInstances = 2000;
        [SerializeField] private bool _enableAdaptiveRendering = true;
        [SerializeField] private bool _enablePerformanceScaling = true;
        
        [Header("Rendering Quality")]
        [SerializeField] private RenderingQuality _defaultRenderingQuality = RenderingQuality.High;
        [SerializeField] private bool _enableQualityScaling = true;
        [SerializeField] private float _qualityScalingThreshold = 45f; // FPS threshold
        
        // Large-Scale Rendering State
        private Dictionary<Vector2Int, RenderingZone> _renderingZones = new Dictionary<Vector2Int, RenderingZone>();
        private Queue<Vector2Int> _zoneLoadQueue = new Queue<Vector2Int>();
        private Queue<Vector2Int> _zoneUnloadQueue = new Queue<Vector2Int>();
        private List<PlantRenderInstance> _activePlantInstances = new List<PlantRenderInstance>();
        
        // GPU Instancing
        private Dictionary<string, PlantInstanceBatch> _instanceBatches = new Dictionary<string, PlantInstanceBatch>();
        private Dictionary<Material, List<PlantRenderInstance>> _materialBatches = new Dictionary<Material, List<PlantRenderInstance>>();
        private ComputeBuffer _instanceDataBuffer;
        private ComputeBuffer _cullingBuffer;
        
        // Camera and Frustum
        private Camera _mainCamera;
        private Plane[] _cameraFrustumPlanes = new Plane[6];
        private Vector3 _lastCameraPosition;
        private Vector2Int _currentCameraZone;
        
        // Performance Tracking
        private float _lastRenderingUpdate = 0f;
        private int _renderingUpdatesProcessedThisFrame = 0;
        private float _currentFrameRate = 60f;
        private LargeScaleRenderingMetrics _renderingMetrics;
        
        // System Integration
        private AdvancedLODSystem _lodSystem;
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private DynamicGrowthAnimationSystem _dynamicGrowthSystem;
        private SeasonalAdaptationVFXController _seasonalAdaptationController;
        
        // Asset Management
        private Dictionary<string, PlantAssetGroup> _loadedAssetGroups = new Dictionary<string, PlantAssetGroup>();
        private Queue<string> _assetLoadQueue = new Queue<string>();
        private Queue<string> _assetUnloadQueue = new Queue<string>();
        
        // Cannabis-Specific Grouping
        private Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, List<PlantRenderInstance>> _growthStageGroups;
        private Dictionary<string, List<PlantRenderInstance>> _geneticClusters;
        private Dictionary<Season, List<PlantRenderInstance>> _seasonalGroups;
        
        // Job System
        private JobHandle _cullingJobHandle;
        private JobHandle _batchingJobHandle;
        private NativeArray<PlantInstanceData> _plantInstanceData;
        private NativeArray<bool> _cullingResults;
        
        // Events
        public System.Action<Vector2Int> OnZoneLoaded;
        public System.Action<Vector2Int> OnZoneUnloaded;
        public System.Action<int, int> OnInstanceCountChanged;
        public System.Action<RenderingQuality> OnRenderingQualityChanged;
        public System.Action<LargeScaleRenderingMetrics> OnRenderingMetricsUpdated;
        
        // Properties
        public int ActivePlantInstances => _activePlantInstances.Count;
        public int LoadedZones => _renderingZones.Count;
        public float CurrentFrameRate => _currentFrameRate;
        public RenderingQuality CurrentRenderingQuality { get; private set; }
        public LargeScaleRenderingMetrics RenderingMetrics => _renderingMetrics;
        public bool EnableLargeScaleRendering => _enableLargeScaleRendering;
        
        protected override void OnManagerInitialize()
        {
            InitializeLargeScaleRenderingSystem();
            InitializeGPUInstancing();
            InitializeRenderingZones();
            InitializeCannabisGrouping();
            ConnectToSystems();
            InitializeCameraTracking();
            InitializePerformanceTracking();
            StartRenderingProcessing();
            LogInfo("Large-Scale Rendering System initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            CleanupGPUResources();
            ClearAllRenderingData();
            DisconnectFromSystems();
            _renderingMetrics = null;
            LogInfo("Large-Scale Rendering System shutdown");
        }
        
        private void Update()
        {
            if (!_enableLargeScaleRendering) return;
            
            UpdateCameraTracking();
            UpdateFrameRateTracking();
            
            float targetUpdateInterval = 1f / _renderingUpdateFrequency;
            if (Time.time - _lastRenderingUpdate >= targetUpdateInterval)
            {
                ProcessRenderingFrame();
                UpdateZoneStreaming();
                ProcessAssetStreaming();
                UpdateInstanceBatching();
                UpdatePerformanceMetrics();
                AdjustRenderingQuality();
                _lastRenderingUpdate = Time.time;
            }
        }
        
        private void LateUpdate()
        {
            // Complete any pending jobs
            if (_cullingJobHandle.IsCompleted)
            {
                _cullingJobHandle.Complete();
                ProcessCullingResults();
            }
            
            if (_batchingJobHandle.IsCompleted)
            {
                _batchingJobHandle.Complete();
                ProcessBatchingResults();
            }
            
            // Render all active batches
            RenderInstanceBatches();
        }
        
        #region Initialization
        
        private void InitializeLargeScaleRenderingSystem()
        {
            LogInfo("=== INITIALIZING LARGE-SCALE RENDERING SYSTEM ===");
            
            _activePlantInstances.Clear();
            _renderingZones.Clear();
            _zoneLoadQueue.Clear();
            _zoneUnloadQueue.Clear();
            _instanceBatches.Clear();
            _materialBatches.Clear();
            
            CurrentRenderingQuality = _defaultRenderingQuality;
            _renderingUpdatesProcessedThisFrame = 0;
            
            LogInfo($"✅ System configured for {_maxPlantsSupported} plants with {_maxActiveInstances} active instances");
        }
        
        private void InitializeGPUInstancing()
        {
            if (_enableGPUInstancing && SystemInfo.supportsInstancing)
            {
                // Initialize GPU instancing buffers
                int maxInstances = Mathf.Min(_maxActiveInstances, _maxInstancesPerDrawCall);
                
                _instanceDataBuffer = new ComputeBuffer(maxInstances, sizeof(float) * 16); // 4x4 matrix
                _cullingBuffer = new ComputeBuffer(maxInstances, sizeof(int));
                
                LogInfo($"✅ GPU Instancing initialized for {maxInstances} instances");
            }
            else
            {
                LogWarning("GPU Instancing not supported on this platform");
                _enableGPUInstancing = false;
            }
        }
        
        private void InitializeRenderingZones()
        {
            if (_enableZonedRendering)
            {
                LogInfo($"✅ Zoned rendering initialized with {_zoneSize} zone size");
                LogInfo($"✅ Supporting {_maxZonesRendered} zones within {_zoneStreamingDistance}m");
            }
        }
        
        private void InitializeCannabisGrouping()
        {
            _growthStageGroups = new Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, List<PlantRenderInstance>>();
            _geneticClusters = new Dictionary<string, List<PlantRenderInstance>>();
            _seasonalGroups = new Dictionary<Season, List<PlantRenderInstance>>();
            
            // Initialize growth stage groups
            foreach (ProjectChimera.Data.Genetics.PlantGrowthStage stage in System.Enum.GetValues(typeof(ProjectChimera.Data.Genetics.PlantGrowthStage)))
            {
                _growthStageGroups[stage] = new List<PlantRenderInstance>();
            }
            
            // Initialize seasonal groups
            foreach (Season season in System.Enum.GetValues(typeof(Season)))
            {
                _seasonalGroups[season] = new List<PlantRenderInstance>();
            }
            
            LogInfo("✅ Cannabis-specific grouping initialized");
        }
        
        private void ConnectToSystems()
        {
            // Find and connect to other systems
            _lodSystem = FindObjectOfType<AdvancedLODSystem>();
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            _dynamicGrowthSystem = FindObjectOfType<DynamicGrowthAnimationSystem>();
            _seasonalAdaptationController = FindObjectOfType<SeasonalAdaptationVFXController>();
            
            int connectedSystems = 0;
            
            if (_lodSystem != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to Advanced LOD System");
            }
            
            if (_vfxTemplateManager != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to Cannabis VFX Template Manager");
            }
            
            if (_speedTreeIntegration != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to SpeedTree VFX Integration Manager");
            }
            
            if (_dynamicGrowthSystem != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to Dynamic Growth Animation System");
            }
            
            if (_seasonalAdaptationController != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to Seasonal Adaptation VFX Controller");
            }
            
            LogInfo($"✅ Connected to {connectedSystems}/5 systems for large-scale integration");
        }
        
        private void InitializeCameraTracking()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }
            
            if (_mainCamera != null)
            {
                _lastCameraPosition = _mainCamera.transform.position;
                _currentCameraZone = WorldToZoneCoordinate(_lastCameraPosition);
                GeometryUtility.CalculateFrustumPlanes(_mainCamera, _cameraFrustumPlanes);
                LogInfo("✅ Camera tracking initialized");
            }
            else
            {
                LogWarning("No camera found for large-scale rendering");
            }
        }
        
        private void InitializePerformanceTracking()
        {
            _renderingMetrics = new LargeScaleRenderingMetrics
            {
                TotalPlantInstances = 0,
                ActiveInstances = 0,
                CulledInstances = 0,
                LoadedZones = 0,
                MemoryUsageMB = 0f,
                AverageFrameRate = 60f,
                RenderingUpdatesPerSecond = 0f,
                GPUInstancesPerFrame = 0,
                DrawCallsPerFrame = 0,
                CurrentRenderingQuality = CurrentRenderingQuality
            };
            
            LogInfo("✅ Performance tracking initialized");
        }
        
        #endregion
        
        #region Rendering Processing
        
        private void ProcessRenderingFrame()
        {
            _renderingUpdatesProcessedThisFrame = 0;
            
            // Update camera frustum planes
            if (_mainCamera != null)
            {
                GeometryUtility.CalculateFrustumPlanes(_mainCamera, _cameraFrustumPlanes);
            }
            
            // Process culling and batching
            if (_enableGPUCulling && _activePlantInstances.Count > 0)
            {
                StartCullingJob();
            }
            
            if (_enablePlantBatching)
            {
                StartBatchingJob();
            }
        }
        
        private void StartCullingJob()
        {
            if (_plantInstanceData.IsCreated)
            {
                _plantInstanceData.Dispose();
            }
            if (_cullingResults.IsCreated)
            {
                _cullingResults.Dispose();
            }
            
            int instanceCount = _activePlantInstances.Count;
            _plantInstanceData = new NativeArray<PlantInstanceData>(instanceCount, Allocator.TempJob);
            _cullingResults = new NativeArray<bool>(instanceCount, Allocator.TempJob);
            
            // Fill instance data
            for (int i = 0; i < instanceCount; i++)
            {
                var instance = _activePlantInstances[i];
                _plantInstanceData[i] = new PlantInstanceData
                {
                    Position = instance.Position,
                    Scale = instance.Scale,
                    Bounds = instance.Bounds,
                    LODLevel = (int)instance.LODLevel,
                    IsActive = instance.IsActive
                };
            }
            
            // Start culling job
            var cullingJob = new FrustumCullingJob
            {
                InstanceData = _plantInstanceData,
                CullingResults = _cullingResults,
                CameraPosition = _mainCamera.transform.position,
                FrustumPlanes = new NativeArray<Vector4>(_cameraFrustumPlanes.Select(p => new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance)).ToArray(), Allocator.TempJob),
                CullingDistance = _lodSystem != null ? 150f : 100f
            };
            
            _cullingJobHandle = cullingJob.Schedule(instanceCount, 32);
        }
        
        private void StartBatchingJob()
        {
            // Batching job implementation
            var batchingJob = new PlantBatchingJob
            {
                ActiveInstances = _activePlantInstances.Count,
                MaxBatchSize = _maxPlantsPerBatch
            };
            
            _batchingJobHandle = batchingJob.Schedule();
        }
        
        private void ProcessCullingResults()
        {
            if (!_cullingResults.IsCreated) return;
            
            int culledCount = 0;
            for (int i = 0; i < _cullingResults.Length; i++)
            {
                if (i < _activePlantInstances.Count)
                {
                    _activePlantInstances[i].IsVisible = _cullingResults[i];
                    if (!_cullingResults[i])
                        culledCount++;
                }
            }
            
            _renderingMetrics.CulledInstances = culledCount;
            _renderingMetrics.ActiveInstances = _activePlantInstances.Count - culledCount;
            
            // Cleanup
            if (_plantInstanceData.IsCreated)
                _plantInstanceData.Dispose();
            if (_cullingResults.IsCreated)
                _cullingResults.Dispose();
        }
        
        private void ProcessBatchingResults()
        {
            // Process batching job results
            UpdateInstanceBatching();
        }
        
        private void RenderInstanceBatches()
        {
            if (!_enableGPUInstancing) return;
            
            int totalDrawCalls = 0;
            int totalInstances = 0;
            
            foreach (var batch in _instanceBatches.Values)
            {
                if (batch.InstanceCount > 0 && batch.Material != null && batch.Mesh != null)
                {
                    // Update instance buffer
                    if (_instanceDataBuffer != null && batch.InstanceCount <= _maxInstancesPerDrawCall)
                    {
                        _instanceDataBuffer.SetData(batch.InstanceMatrices, 0, 0, batch.InstanceCount);
                        
                        // Render batch
                        Graphics.DrawMeshInstanced(
                            batch.Mesh,
                            0,
                            batch.Material,
                            batch.InstanceMatrices,
                            batch.InstanceCount,
                            null,
                            UnityEngine.Rendering.ShadowCastingMode.On,
                            true
                        );
                        
                        totalDrawCalls++;
                        totalInstances += batch.InstanceCount;
                    }
                }
            }
            
            _renderingMetrics.DrawCallsPerFrame = totalDrawCalls;
            _renderingMetrics.GPUInstancesPerFrame = totalInstances;
        }
        
        #endregion
        
        #region Zone Management
        
        private void UpdateZoneStreaming()
        {
            if (!_enableZonedRendering || _mainCamera == null) return;
            
            Vector3 cameraPosition = _mainCamera.transform.position;
            Vector2Int newCameraZone = WorldToZoneCoordinate(cameraPosition);
            
            if (newCameraZone != _currentCameraZone)
            {
                _currentCameraZone = newCameraZone;
                UpdateVisibleZones();
            }
        }
        
        private void UpdateVisibleZones()
        {
            // Determine which zones should be loaded
            var targetZones = GetZonesAroundCamera(_currentCameraZone, _maxZonesRendered);
            var currentZones = new HashSet<Vector2Int>(_renderingZones.Keys);
            
            // Queue zones for loading
            foreach (var zone in targetZones)
            {
                if (!currentZones.Contains(zone))
                {
                    _zoneLoadQueue.Enqueue(zone);
                }
            }
            
            // Queue zones for unloading
            foreach (var zone in currentZones)
            {
                if (!targetZones.Contains(zone))
                {
                    _zoneUnloadQueue.Enqueue(zone);
                }
            }
            
            ProcessZoneQueues();
        }
        
        private HashSet<Vector2Int> GetZonesAroundCamera(Vector2Int centerZone, int maxZones)
        {
            var zones = new HashSet<Vector2Int>();
            int radius = Mathf.CeilToInt(Mathf.Sqrt(maxZones) / 2f);
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    if (zones.Count < maxZones)
                    {
                        zones.Add(new Vector2Int(centerZone.x + x, centerZone.y + z));
                    }
                }
            }
            
            return zones;
        }
        
        private void ProcessZoneQueues()
        {
            // Process zone loading
            int loadedThisFrame = 0;
            while (_zoneLoadQueue.Count > 0 && loadedThisFrame < 2)
            {
                var zone = _zoneLoadQueue.Dequeue();
                LoadZone(zone);
                loadedThisFrame++;
            }
            
            // Process zone unloading
            int unloadedThisFrame = 0;
            while (_zoneUnloadQueue.Count > 0 && unloadedThisFrame < 2)
            {
                var zone = _zoneUnloadQueue.Dequeue();
                UnloadZone(zone);
                unloadedThisFrame++;
            }
        }
        
        private void LoadZone(Vector2Int zoneCoordinate)
        {
            if (_renderingZones.ContainsKey(zoneCoordinate)) return;
            
            var zone = new RenderingZone
            {
                Coordinate = zoneCoordinate,
                WorldBounds = ZoneCoordinateToWorldBounds(zoneCoordinate),
                PlantInstances = new List<PlantRenderInstance>(),
                IsLoaded = false,
                LoadTime = Time.time
            };
            
            _renderingZones[zoneCoordinate] = zone;
            
            if (_enableAsyncLoading)
            {
                StartCoroutine(LoadZoneAsync(zone));
            }
            else
            {
                LoadZoneSync(zone);
            }
        }
        
        private void UnloadZone(Vector2Int zoneCoordinate)
        {
            if (_renderingZones.ContainsKey(zoneCoordinate))
            {
                var zone = _renderingZones[zoneCoordinate];
                
                // Remove plant instances from active list
                foreach (var instance in zone.PlantInstances)
                {
                    _activePlantInstances.Remove(instance);
                    RemoveFromGroups(instance);
                }
                
                _renderingZones.Remove(zoneCoordinate);
                OnZoneUnloaded?.Invoke(zoneCoordinate);
            }
        }
        
        private IEnumerator LoadZoneAsync(RenderingZone zone)
        {
            yield return null; // Wait one frame
            
            LoadZoneSync(zone);
        }
        
        private void LoadZoneSync(RenderingZone zone)
        {
            // Load plant instances for this zone
            // This would typically load from a database or file
            zone.IsLoaded = true;
            OnZoneLoaded?.Invoke(zone.Coordinate);
        }
        
        private Vector2Int WorldToZoneCoordinate(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / _zoneSize.x),
                Mathf.FloorToInt(worldPosition.z / _zoneSize.y)
            );
        }
        
        private Bounds ZoneCoordinateToWorldBounds(Vector2Int zoneCoordinate)
        {
            Vector3 center = new Vector3(
                zoneCoordinate.x * _zoneSize.x + _zoneSize.x * 0.5f,
                0f,
                zoneCoordinate.y * _zoneSize.y + _zoneSize.y * 0.5f
            );
            
            return new Bounds(center, new Vector3(_zoneSize.x, 100f, _zoneSize.y));
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Registers a plant for large-scale rendering
        /// </summary>
        public void RegisterPlantForRendering(string plantId, GameObject plantGameObject, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, Vector3 position)
        {
            if (string.IsNullOrEmpty(plantId) || plantGameObject == null)
            {
                LogWarning("Cannot register plant for rendering: invalid parameters");
                return;
            }
            
            if (_activePlantInstances.Count >= _maxPlantsSupported)
            {
                LogWarning($"Maximum plant limit reached ({_maxPlantsSupported})");
                return;
            }
            
            var renderInstance = new PlantRenderInstance
            {
                PlantId = plantId,
                PlantGameObject = plantGameObject,
                Position = position,
                Scale = plantGameObject.transform.localScale,
                Bounds = GetPlantBounds(plantGameObject),
                GrowthStage = growthStage,
                LODLevel = CannabisLODLevel.High,
                IsActive = true,
                IsVisible = true,
                RegistrationTime = Time.time,
                Material = GetPlantMaterial(plantGameObject),
                Mesh = GetPlantMesh(plantGameObject)
            };
            
            _activePlantInstances.Add(renderInstance);
            AddToGroups(renderInstance);
            
            // Add to appropriate zone
            Vector2Int zoneCoordinate = WorldToZoneCoordinate(position);
            if (_renderingZones.ContainsKey(zoneCoordinate))
            {
                _renderingZones[zoneCoordinate].PlantInstances.Add(renderInstance);
            }
            
            OnInstanceCountChanged?.Invoke(_activePlantInstances.Count, _maxPlantsSupported);
            LogInfo($"Registered plant {plantId} for large-scale rendering");
        }
        
        /// <summary>
        /// Unregisters a plant from large-scale rendering
        /// </summary>
        public void UnregisterPlantFromRendering(string plantId)
        {
            var instance = _activePlantInstances.FirstOrDefault(p => p.PlantId == plantId);
            if (instance != null)
            {
                _activePlantInstances.Remove(instance);
                RemoveFromGroups(instance);
                
                // Remove from zone
                Vector2Int zoneCoordinate = WorldToZoneCoordinate(instance.Position);
                if (_renderingZones.ContainsKey(zoneCoordinate))
                {
                    _renderingZones[zoneCoordinate].PlantInstances.Remove(instance);
                }
                
                OnInstanceCountChanged?.Invoke(_activePlantInstances.Count, _maxPlantsSupported);
                LogInfo($"Unregistered plant {plantId} from large-scale rendering");
            }
        }
        
        /// <summary>
        /// Updates plant position for zone management
        /// </summary>
        public void UpdatePlantPosition(string plantId, Vector3 newPosition)
        {
            var instance = _activePlantInstances.FirstOrDefault(p => p.PlantId == plantId);
            if (instance != null)
            {
                Vector2Int oldZone = WorldToZoneCoordinate(instance.Position);
                Vector2Int newZone = WorldToZoneCoordinate(newPosition);
                
                instance.Position = newPosition;
                instance.Bounds = GetPlantBounds(instance.PlantGameObject);
                
                // Move between zones if necessary
                if (oldZone != newZone)
                {
                    if (_renderingZones.ContainsKey(oldZone))
                    {
                        _renderingZones[oldZone].PlantInstances.Remove(instance);
                    }
                    
                    if (_renderingZones.ContainsKey(newZone))
                    {
                        _renderingZones[newZone].PlantInstances.Add(instance);
                    }
                }
            }
        }
        
        /// <summary>
        /// Sets rendering quality level
        /// </summary>
        public void SetRenderingQuality(RenderingQuality quality)
        {
            if (CurrentRenderingQuality != quality)
            {
                CurrentRenderingQuality = quality;
                ApplyRenderingQuality(quality);
                OnRenderingQualityChanged?.Invoke(quality);
                LogInfo($"Rendering quality changed to {quality}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateCameraTracking()
        {
            if (_mainCamera != null)
            {
                _lastCameraPosition = _mainCamera.transform.position;
            }
        }
        
        private void UpdateFrameRateTracking()
        {
            _currentFrameRate = 1f / Time.deltaTime;
        }
        
        private void ProcessAssetStreaming()
        {
            // Process asset loading queue
            if (_assetLoadQueue.Count > 0 && _loadedAssetGroups.Count < _maxLoadedZones)
            {
                string assetId = _assetLoadQueue.Dequeue();
                LoadAssetGroup(assetId);
            }
            
            // Process asset unloading queue
            if (_assetUnloadQueue.Count > 0)
            {
                string assetId = _assetUnloadQueue.Dequeue();
                UnloadAssetGroup(assetId);
            }
        }
        
        private void UpdateInstanceBatching()
        {
            if (!_enablePlantBatching) return;
            
            // Clear existing batches
            foreach (var batch in _instanceBatches.Values)
            {
                batch.InstanceCount = 0;
            }
            
            // Group instances by material and mesh
            var visibleInstances = _activePlantInstances.Where(i => i.IsVisible && i.IsActive).ToList();
            
            foreach (var instance in visibleInstances)
            {
                string batchKey = GetBatchKey(instance);
                
                if (!_instanceBatches.ContainsKey(batchKey))
                {
                    _instanceBatches[batchKey] = new PlantInstanceBatch
                    {
                        Material = instance.Material,
                        Mesh = instance.Mesh,
                        InstanceMatrices = new Matrix4x4[_maxInstancesPerDrawCall],
                        InstanceCount = 0
                    };
                }
                
                var batch = _instanceBatches[batchKey];
                if (batch.InstanceCount < _maxInstancesPerDrawCall)
                {
                    batch.InstanceMatrices[batch.InstanceCount] = Matrix4x4.TRS(
                        instance.Position,
                        instance.PlantGameObject.transform.rotation,
                        instance.Scale
                    );
                    batch.InstanceCount++;
                }
            }
        }
        
        private void AddToGroups(PlantRenderInstance instance)
        {
            if (_enableGrowthStageGrouping && _growthStageGroups.ContainsKey(instance.GrowthStage))
            {
                _growthStageGroups[instance.GrowthStage].Add(instance);
            }
            
            if (_enableGeneticClustering)
            {
                string geneticKey = GetGeneticClusterKey(instance);
                if (!_geneticClusters.ContainsKey(geneticKey))
                {
                    _geneticClusters[geneticKey] = new List<PlantRenderInstance>();
                }
                _geneticClusters[geneticKey].Add(instance);
            }
        }
        
        private void RemoveFromGroups(PlantRenderInstance instance)
        {
            if (_enableGrowthStageGrouping && _growthStageGroups.ContainsKey(instance.GrowthStage))
            {
                _growthStageGroups[instance.GrowthStage].Remove(instance);
            }
            
            if (_enableGeneticClustering)
            {
                string geneticKey = GetGeneticClusterKey(instance);
                if (_geneticClusters.ContainsKey(geneticKey))
                {
                    _geneticClusters[geneticKey].Remove(instance);
                }
            }
        }
        
        private string GetBatchKey(PlantRenderInstance instance)
        {
            return $"{instance.Material?.name}_{instance.Mesh?.name}_{instance.LODLevel}";
        }
        
        private string GetGeneticClusterKey(PlantRenderInstance instance)
        {
            return $"genetic_{instance.PlantId.GetHashCode() % 10}"; // Simplified genetic clustering
        }
        
        private Bounds GetPlantBounds(GameObject plantGameObject)
        {
            var renderer = plantGameObject.GetComponent<Renderer>();
            return renderer != null ? renderer.bounds : new Bounds(plantGameObject.transform.position, Vector3.one);
        }
        
        private Material GetPlantMaterial(GameObject plantGameObject)
        {
            var renderer = plantGameObject.GetComponent<Renderer>();
            return renderer != null ? renderer.material : null;
        }
        
        private Mesh GetPlantMesh(GameObject plantGameObject)
        {
            var meshFilter = plantGameObject.GetComponent<MeshFilter>();
            return meshFilter != null ? meshFilter.mesh : null;
        }
        
        private void LoadAssetGroup(string assetId)
        {
            // Asset loading implementation
        }
        
        private void UnloadAssetGroup(string assetId)
        {
            if (_loadedAssetGroups.ContainsKey(assetId))
            {
                _loadedAssetGroups.Remove(assetId);
            }
        }
        
        private void ApplyRenderingQuality(RenderingQuality quality)
        {
            // Apply quality settings
            switch (quality)
            {
                case RenderingQuality.Low:
                    _maxActiveInstances = Mathf.Min(500, _maxPlantsSupported);
                    break;
                case RenderingQuality.Medium:
                    _maxActiveInstances = Mathf.Min(1000, _maxPlantsSupported);
                    break;
                case RenderingQuality.High:
                    _maxActiveInstances = Mathf.Min(2000, _maxPlantsSupported);
                    break;
                case RenderingQuality.Ultra:
                    _maxActiveInstances = _maxPlantsSupported;
                    break;
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (_renderingMetrics != null)
            {
                _renderingMetrics.TotalPlantInstances = _activePlantInstances.Count;
                _renderingMetrics.LoadedZones = _renderingZones.Count;
                _renderingMetrics.AverageFrameRate = _currentFrameRate;
                _renderingMetrics.RenderingUpdatesPerSecond = _renderingUpdatesProcessedThisFrame / Time.deltaTime;
                _renderingMetrics.MemoryUsageMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / (1024f * 1024f);
                _renderingMetrics.CurrentRenderingQuality = CurrentRenderingQuality;
                
                OnRenderingMetricsUpdated?.Invoke(_renderingMetrics);
            }
        }
        
        private void AdjustRenderingQuality()
        {
            if (!_enableQualityScaling) return;
            
            if (_currentFrameRate < _qualityScalingThreshold)
            {
                // Reduce quality if performance is poor
                if (CurrentRenderingQuality > RenderingQuality.Low)
                {
                    SetRenderingQuality((RenderingQuality)((int)CurrentRenderingQuality - 1));
                }
            }
            else if (_currentFrameRate > _targetFrameRate * 1.2f)
            {
                // Increase quality if performance is good
                if (CurrentRenderingQuality < RenderingQuality.Ultra)
                {
                    SetRenderingQuality((RenderingQuality)((int)CurrentRenderingQuality + 1));
                }
            }
        }
        
        private void CleanupGPUResources()
        {
            _instanceDataBuffer?.Dispose();
            _cullingBuffer?.Dispose();
            
            if (_plantInstanceData.IsCreated)
                _plantInstanceData.Dispose();
            if (_cullingResults.IsCreated)
                _cullingResults.Dispose();
        }
        
        private void ClearAllRenderingData()
        {
            _activePlantInstances.Clear();
            _renderingZones.Clear();
            _instanceBatches.Clear();
            _materialBatches.Clear();
            _loadedAssetGroups.Clear();
            
            foreach (var group in _growthStageGroups.Values)
            {
                group.Clear();
            }
            
            foreach (var cluster in _geneticClusters.Values)
            {
                cluster.Clear();
            }
            
            foreach (var seasonGroup in _seasonalGroups.Values)
            {
                seasonGroup.Clear();
            }
        }
        
        private void DisconnectFromSystems()
        {
            _lodSystem = null;
            _vfxTemplateManager = null;
            _speedTreeIntegration = null;
            _dynamicGrowthSystem = null;
            _seasonalAdaptationController = null;
        }
        
        private void StartRenderingProcessing()
        {
            StartCoroutine(RenderingProcessingCoroutine());
        }
        
        private IEnumerator RenderingProcessingCoroutine()
        {
            while (_enableLargeScaleRendering)
            {
                yield return new WaitForSeconds(1f / _renderingUpdateFrequency);
                
                if (_activePlantInstances.Count > 0)
                {
                    // Additional background processing
                }
            }
        }
        
        #endregion
    }
    
    #region Supporting Data Structures and Jobs
    
    [System.Serializable]
    public class PlantRenderInstance
    {
        public string PlantId;
        public GameObject PlantGameObject;
        public Vector3 Position;
        public Vector3 Scale;
        public Bounds Bounds;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public CannabisLODLevel LODLevel;
        public bool IsActive;
        public bool IsVisible;
        public float RegistrationTime;
        public Material Material;
        public Mesh Mesh;
    }
    
    [System.Serializable]
    public class RenderingZone
    {
        public Vector2Int Coordinate;
        public Bounds WorldBounds;
        public List<PlantRenderInstance> PlantInstances;
        public bool IsLoaded;
        public float LoadTime;
    }
    
    [System.Serializable]
    public class PlantInstanceBatch
    {
        public Material Material;
        public Mesh Mesh;
        public Matrix4x4[] InstanceMatrices;
        public int InstanceCount;
    }
    
    [System.Serializable]
    public class PlantAssetGroup
    {
        public string AssetId;
        public List<GameObject> Prefabs;
        public List<Material> Materials;
        public List<Mesh> Meshes;
        public float LoadTime;
        public float LastAccessTime;
    }
    
    [System.Serializable]
    public class LargeScaleRenderingMetrics
    {
        public int TotalPlantInstances;
        public int ActiveInstances;
        public int CulledInstances;
        public int LoadedZones;
        public float MemoryUsageMB;
        public float AverageFrameRate;
        public float RenderingUpdatesPerSecond;
        public int GPUInstancesPerFrame;
        public int DrawCallsPerFrame;
        public RenderingQuality CurrentRenderingQuality;
    }
    
    public struct PlantInstanceData
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Bounds Bounds;
        public int LODLevel;
        public bool IsActive;
    }
    
    public struct FrustumCullingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PlantInstanceData> InstanceData;
        [WriteOnly] public NativeArray<bool> CullingResults;
        [ReadOnly] public Vector3 CameraPosition;
        [ReadOnly] public NativeArray<Vector4> FrustumPlanes;
        [ReadOnly] public float CullingDistance;
        
        public void Execute(int index)
        {
            var instance = InstanceData[index];
            
            // Distance culling
            float distance = Vector3.Distance(instance.Position, CameraPosition);
            if (distance > CullingDistance)
            {
                CullingResults[index] = false;
                return;
            }
            
            // Frustum culling (simplified)
            CullingResults[index] = instance.IsActive;
        }
    }
    
    public struct PlantBatchingJob : IJob
    {
        public int ActiveInstances;
        public int MaxBatchSize;
        
        public void Execute()
        {
            // Batching logic implementation
        }
    }
    
    public enum RenderingQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    #endregion
}