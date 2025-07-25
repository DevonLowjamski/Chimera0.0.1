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
using UnityEngine.Rendering;

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Advanced GPU batching and instancing system for cannabis cultivation rendering.
    /// Provides hardware-accelerated rendering for thousands of plants using GPU instancing,
    /// indirect rendering, and compute shader optimizations. Designed for maximum performance
    /// with minimal CPU overhead and optimal GPU utilization.
    /// </summary>
    public class GPUBatchingInstanceSystem : ChimeraManager
    {
        [Header("GPU Batching Configuration")]
        [SerializeField] private bool _enableGPUBatching = true;
        [SerializeField] private bool _enableIndirectRendering = true;
        [SerializeField] private int _maxInstancesPerBatch = 1023; // GPU hardware limit
        [SerializeField] private float _batchingUpdateFrequency = 30f; // Updates per second
        
        [Header("Instancing Settings")]
        [SerializeField] private bool _enableGPUInstancing = true;
        [SerializeField] private bool _enableInstancedIndirect = true;
        [SerializeField] private int _maxTotalInstances = 10000;
        [SerializeField] private bool _enableInstanceCulling = true;
        
        [Header("Compute Shader Integration")]
        [SerializeField] private bool _enableComputeShaderCulling = true;
        [SerializeField] private ComputeShader _cullingComputeShader;
        [SerializeField] private ComputeShader _batchingComputeShader;
        [SerializeField] private int _computeThreadGroups = 64;
        
        [Header("Cannabis Batching Optimization")]
        [SerializeField] private bool _enableGrowthStageBatching = true;
        [SerializeField] private bool _enableGeneticBatching = true;
        [SerializeField] private bool _enableMaterialBatching = true;
        [SerializeField] private bool _enableLODBatching = true;
        
        [Header("GPU Memory Management")]
        [SerializeField] private int _instanceBufferSize = 5000;
        [SerializeField] private int _indirectArgsBufferSize = 1000;
        [SerializeField] private bool _enableBufferPooling = true;
        [SerializeField] private bool _enableDynamicBufferResize = true;
        
        [Header("Performance Optimization")]
        [SerializeField] private bool _enableGPUCulling = true;
        [SerializeField] private bool _enableOcclusionCulling = true;
        [SerializeField] private bool _enableFrustumCulling = true;
        [SerializeField] private bool _enableDistanceCulling = true;
        [SerializeField, Range(10f, 500f)] private float _maxRenderDistance = 150f;
        
        [Header("Rendering Quality")]
        [SerializeField] private GPUBatchingQuality _batchingQuality = GPUBatchingQuality.High;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        [SerializeField, Range(30f, 120f)] private float _targetFrameRate = 60f;
        [SerializeField] private bool _enablePerformanceScaling = true;
        
        // GPU Batching State
        private Dictionary<string, GPUInstanceBatch> _gpuInstanceBatches = new Dictionary<string, GPUInstanceBatch>();
        private Dictionary<Material, List<GPUPlantInstance>> _materialGroups = new Dictionary<Material, List<GPUPlantInstance>>();
        private Queue<GPUPlantInstance> _instanceUpdateQueue = new Queue<GPUPlantInstance>();
        private List<GPUPlantInstance> _allInstances = new List<GPUPlantInstance>();
        
        // GPU Buffers
        private ComputeBuffer _instanceDataBuffer;
        private ComputeBuffer _indirectArgsBuffer;
        private ComputeBuffer _cullingResultsBuffer;
        private ComputeBuffer _visibilityBuffer;
        private ComputeBuffer _lodDataBuffer;
        
        // Indirect Rendering
        private uint[] _indirectArgs = new uint[5] { 0, 0, 0, 0, 0 };
        private Dictionary<string, GraphicsBuffer> _indirectBuffers = new Dictionary<string, GraphicsBuffer>();
        
        // Camera and Culling
        private Camera _mainCamera;
        private Plane[] _frustumPlanes = new Plane[6];
        private Matrix4x4 _vpMatrix;
        private Vector3 _cameraPosition;
        
        // Performance Tracking
        private float _lastBatchingUpdate = 0f;
        private int _batchingUpdatesProcessedThisFrame = 0;
        private float _currentFrameRate = 60f;
        private GPUBatchingMetrics _batchingMetrics;
        
        // System Integration
        private LargeScaleRenderingSystem _largeScaleRenderer;
        private AdvancedLODSystem _lodSystem;
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        
        // Cannabis-Specific Batching
        private Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GPUBatchGroup> _growthStageBatches;
        private Dictionary<CannabisLODLevel, GPUBatchGroup> _lodBatches;
        private Dictionary<string, GPUBatchGroup> _geneticBatches;
        
        // Buffer Pool
        private Queue<ComputeBuffer> _bufferPool = new Queue<ComputeBuffer>();
        private List<ComputeBuffer> _activeBuffers = new List<ComputeBuffer>();
        
        // Compute Shader Kernels
        private int _cullingKernel = -1;
        private int _batchingKernel = -1;
        private int _lodKernel = -1;
        
        // Job System Integration
        private JobHandle _batchingJobHandle;
        private NativeArray<GPUInstanceData> _instanceDataArray;
        private NativeArray<bool> _cullingResultsArray;
        
        // Events
        public System.Action<int, int> OnInstanceCountChanged;
        public System.Action<GPUBatchingQuality> OnBatchingQualityChanged;
        public System.Action<string> OnBatchCreated;
        public System.Action<string> OnBatchDestroyed;
        public System.Action<GPUBatchingMetrics> OnBatchingMetricsUpdated;
        
        // Properties
        public int TotalInstances => _allInstances.Count;
        public int ActiveBatches => _gpuInstanceBatches.Count;
        public GPUBatchingQuality CurrentBatchingQuality => _batchingQuality;
        public GPUBatchingMetrics BatchingMetrics => _batchingMetrics;
        public bool EnableGPUBatching => _enableGPUBatching;
        public bool SupportsGPUInstancing => SystemInfo.supportsInstancing;
        
        protected override void OnManagerInitialize()
        {
            InitializeGPUBatchingSystem();
            InitializeGPUBuffers();
            InitializeComputeShaders();
            InitializeCannabisSpecificBatching();
            ConnectToRenderingSystems();
            InitializeCameraTracking();
            InitializePerformanceTracking();
            StartBatchingProcessing();
            LogInfo("GPU Batching & Instancing System initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            CleanupGPUResources();
            ClearAllBatches();
            DisconnectFromRenderingSystems();
            _batchingMetrics = null;
            LogInfo("GPU Batching & Instancing System shutdown");
        }
        
        private void Update()
        {
            if (!_enableGPUBatching) return;
            
            UpdateCameraTracking();
            UpdateFrameRateTracking();
            
            float targetUpdateInterval = 1f / _batchingUpdateFrequency;
            if (Time.time - _lastBatchingUpdate >= targetUpdateInterval)
            {
                ProcessGPUBatchingFrame();
                UpdateInstanceBatches();
                ProcessCullingAndLOD();
                UpdatePerformanceMetrics();
                AdjustBatchingQuality();
                _lastBatchingUpdate = Time.time;
            }
        }
        
        private void LateUpdate()
        {
            // Complete any pending batching jobs
            if (_batchingJobHandle.IsCompleted)
            {
                _batchingJobHandle.Complete();
                ProcessBatchingJobResults();
            }
            
            // Execute GPU instanced rendering
            RenderAllInstanceBatches();
        }
        
        #region Initialization
        
        private void InitializeGPUBatchingSystem()
        {
            LogInfo("=== INITIALIZING GPU BATCHING & INSTANCING SYSTEM ===");
            
            if (!SystemInfo.supportsInstancing)
            {
                LogWarning("GPU Instancing not supported on this platform");
                _enableGPUInstancing = false;
                return;
            }
            
            if (!SystemInfo.supportsComputeShaders)
            {
                LogWarning("Compute Shaders not supported - disabling compute shader optimizations");
                _enableComputeShaderCulling = false;
            }
            
            _gpuInstanceBatches.Clear();
            _materialGroups.Clear();
            _instanceUpdateQueue.Clear();
            _allInstances.Clear();
            
            _batchingUpdatesProcessedThisFrame = 0;
            
            LogInfo($"✅ GPU Batching configured for {_maxTotalInstances} instances");
            LogInfo($"✅ Max instances per batch: {_maxInstancesPerBatch}");
        }
        
        private void InitializeGPUBuffers()
        {
            if (!_enableGPUInstancing) return;
            
            try
            {
                // Instance data buffer (Matrix4x4 = 16 floats)
                _instanceDataBuffer = new ComputeBuffer(_instanceBufferSize, sizeof(float) * 16, ComputeBufferType.Default);
                
                // Indirect args buffer
                _indirectArgsBuffer = new ComputeBuffer(_indirectArgsBufferSize, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
                
                // Culling results buffer
                _cullingResultsBuffer = new ComputeBuffer(_instanceBufferSize, sizeof(int), ComputeBufferType.Default);
                
                // Visibility buffer
                _visibilityBuffer = new ComputeBuffer(_instanceBufferSize, sizeof(int), ComputeBufferType.Default);
                
                // LOD data buffer
                _lodDataBuffer = new ComputeBuffer(_instanceBufferSize, sizeof(float) * 4, ComputeBufferType.Default);
                
                _activeBuffers.Add(_instanceDataBuffer);
                _activeBuffers.Add(_indirectArgsBuffer);
                _activeBuffers.Add(_cullingResultsBuffer);
                _activeBuffers.Add(_visibilityBuffer);
                _activeBuffers.Add(_lodDataBuffer);
                
                LogInfo("✅ GPU buffers initialized successfully");
                LogInfo($"✅ Instance buffer size: {_instanceBufferSize}");
                LogInfo($"✅ Indirect args buffer size: {_indirectArgsBufferSize}");
            }
            catch (System.Exception e)
            {
                LogError($"Failed to initialize GPU buffers: {e.Message}");
                _enableGPUBatching = false;
            }
        }
        
        private void InitializeComputeShaders()
        {
            if (!_enableComputeShaderCulling || _cullingComputeShader == null) return;
            
            try
            {
                // Find compute shader kernels
                _cullingKernel = _cullingComputeShader.FindKernel("CullInstances");
                if (_batchingComputeShader != null)
                {
                    _batchingKernel = _batchingComputeShader.FindKernel("BatchInstances");
                    _lodKernel = _batchingComputeShader.FindKernel("CalculateLOD");
                }
                
                LogInfo("✅ Compute shaders initialized");
                if (_cullingKernel >= 0) LogInfo("✅ Culling compute kernel found");
                if (_batchingKernel >= 0) LogInfo("✅ Batching compute kernel found");
                if (_lodKernel >= 0) LogInfo("✅ LOD compute kernel found");
            }
            catch (System.Exception e)
            {
                LogWarning($"Compute shader initialization failed: {e.Message}");
                _enableComputeShaderCulling = false;
            }
        }
        
        private void InitializeCannabisSpecificBatching()
        {
            _growthStageBatches = new Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GPUBatchGroup>();
            _lodBatches = new Dictionary<CannabisLODLevel, GPUBatchGroup>();
            _geneticBatches = new Dictionary<string, GPUBatchGroup>();
            
            // Initialize growth stage batches
            foreach (ProjectChimera.Data.Genetics.PlantGrowthStage stage in System.Enum.GetValues(typeof(ProjectChimera.Data.Genetics.PlantGrowthStage)))
            {
                _growthStageBatches[stage] = new GPUBatchGroup
                {
                    GroupId = $"GrowthStage_{stage}",
                    BatchType = GPUBatchType.GrowthStage,
                    Instances = new List<GPUPlantInstance>(),
                    IsActive = true
                };
            }
            
            // Initialize LOD batches
            foreach (CannabisLODLevel lodLevel in System.Enum.GetValues(typeof(CannabisLODLevel)))
            {
                _lodBatches[lodLevel] = new GPUBatchGroup
                {
                    GroupId = $"LOD_{lodLevel}",
                    BatchType = GPUBatchType.LODLevel,
                    Instances = new List<GPUPlantInstance>(),
                    IsActive = true
                };
            }
            
            LogInfo($"✅ Cannabis-specific batching initialized");
            LogInfo($"✅ Growth stage batches: {_growthStageBatches.Count}");
            LogInfo($"✅ LOD level batches: {_lodBatches.Count}");
        }
        
        private void ConnectToRenderingSystems()
        {
            // Find and connect to rendering systems
            _largeScaleRenderer = FindObjectOfType<LargeScaleRenderingSystem>();
            _lodSystem = FindObjectOfType<AdvancedLODSystem>();
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            
            int connectedSystems = 0;
            
            if (_largeScaleRenderer != null)
            {
                connectedSystems++;
                LogInfo("✅ Connected to Large-Scale Rendering System");
            }
            
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
            
            LogInfo($"✅ Connected to {connectedSystems}/4 rendering systems");
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
                _cameraPosition = _mainCamera.transform.position;
                GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustumPlanes);
                _vpMatrix = _mainCamera.projectionMatrix * _mainCamera.worldToCameraMatrix;
                LogInfo("✅ Camera tracking initialized for GPU batching");
            }
            else
            {
                LogWarning("No camera found for GPU batching");
            }
        }
        
        private void InitializePerformanceTracking()
        {
            _batchingMetrics = new GPUBatchingMetrics
            {
                TotalInstances = 0,
                ActiveBatches = 0,
                CulledInstances = 0,
                RenderedInstances = 0,
                GPUMemoryUsageMB = 0f,
                AverageFrameRate = 60f,
                BatchingUpdatesPerSecond = 0f,
                IndirectDrawCalls = 0,
                GPUUtilization = 0f,
                CurrentBatchingQuality = _batchingQuality
            };
            
            LogInfo("✅ GPU batching performance tracking initialized");
        }
        
        #endregion
        
        #region GPU Batching Processing
        
        private void ProcessGPUBatchingFrame()
        {
            _batchingUpdatesProcessedThisFrame = 0;
            
            // Update camera data
            if (_mainCamera != null)
            {
                _cameraPosition = _mainCamera.transform.position;
                GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustumPlanes);
                _vpMatrix = _mainCamera.projectionMatrix * _mainCamera.worldToCameraMatrix;
            }
            
            // Process instance updates
            ProcessInstanceUpdateQueue();
            
            // Start GPU culling if enabled
            if (_enableGPUCulling && _enableComputeShaderCulling)
            {
                StartGPUCulling();
            }
            
            // Start batching job
            StartBatchingJob();
        }
        
        private void ProcessInstanceUpdateQueue()
        {
            int processedCount = 0;
            int maxUpdatesPerFrame = Mathf.Min(100, _instanceUpdateQueue.Count);
            
            while (_instanceUpdateQueue.Count > 0 && processedCount < maxUpdatesPerFrame)
            {
                var instance = _instanceUpdateQueue.Dequeue();
                UpdateInstanceInBatches(instance);
                processedCount++;
            }
            
            _batchingUpdatesProcessedThisFrame += processedCount;
        }
        
        private void StartGPUCulling()
        {
            if (_cullingComputeShader == null || _cullingKernel < 0) return;
            
            // Set compute shader parameters
            _cullingComputeShader.SetBuffer(_cullingKernel, "InstanceData", _instanceDataBuffer);
            _cullingComputeShader.SetBuffer(_cullingKernel, "CullingResults", _cullingResultsBuffer);
            _cullingComputeShader.SetBuffer(_cullingKernel, "VisibilityBuffer", _visibilityBuffer);
            
            _cullingComputeShader.SetMatrix("ViewProjectionMatrix", _vpMatrix);
            _cullingComputeShader.SetVector("CameraPosition", _cameraPosition);
            _cullingComputeShader.SetFloat("MaxRenderDistance", _maxRenderDistance);
            
            // Dispatch compute shader
            int threadGroups = Mathf.CeilToInt(_allInstances.Count / (float)_computeThreadGroups);
            _cullingComputeShader.Dispatch(_cullingKernel, threadGroups, 1, 1);
        }
        
        private void StartBatchingJob()
        {
            if (_allInstances.Count == 0) return;
            
            // Prepare native arrays for job
            if (_instanceDataArray.IsCreated)
                _instanceDataArray.Dispose();
            if (_cullingResultsArray.IsCreated)
                _cullingResultsArray.Dispose();
            
            _instanceDataArray = new NativeArray<GPUInstanceData>(_allInstances.Count, Allocator.TempJob);
            _cullingResultsArray = new NativeArray<bool>(_allInstances.Count, Allocator.TempJob);
            
            // Fill instance data
            for (int i = 0; i < _allInstances.Count; i++)
            {
                var instance = _allInstances[i];
                _instanceDataArray[i] = new GPUInstanceData
                {
                    InstanceMatrix = instance.InstanceMatrix,
                    Position = instance.Position,
                    Scale = instance.Scale,
                    LODLevel = (int)instance.LODLevel,
                    IsVisible = instance.IsVisible,
                    DistanceToCamera = Vector3.Distance(instance.Position, _cameraPosition)
                };
            }
            
            // Start batching job
            var batchingJob = new GPUBatchingJob
            {
                InstanceData = _instanceDataArray,
                CullingResults = _cullingResultsArray,
                CameraPosition = _cameraPosition,
                MaxRenderDistance = _maxRenderDistance,
                EnableFrustumCulling = _enableFrustumCulling,
                EnableDistanceCulling = _enableDistanceCulling
            };
            
            _batchingJobHandle = batchingJob.Schedule(_allInstances.Count, 32);
        }
        
        private void ProcessBatchingJobResults()
        {
            if (!_cullingResultsArray.IsCreated) return;
            
            // Update instance visibility based on job results
            for (int i = 0; i < _cullingResultsArray.Length && i < _allInstances.Count; i++)
            {
                _allInstances[i].IsVisible = _cullingResultsArray[i];
            }
            
            // Cleanup native arrays
            if (_instanceDataArray.IsCreated)
                _instanceDataArray.Dispose();
            if (_cullingResultsArray.IsCreated)
                _cullingResultsArray.Dispose();
            
            // Update metrics
            int visibleCount = _allInstances.Count(i => i.IsVisible);
            _batchingMetrics.RenderedInstances = visibleCount;
            _batchingMetrics.CulledInstances = _allInstances.Count - visibleCount;
        }
        
        private void ProcessCullingAndLOD()
        {
            if (_enableComputeShaderCulling && _batchingComputeShader != null && _lodKernel >= 0)
            {
                // Set LOD compute shader parameters
                _batchingComputeShader.SetBuffer(_lodKernel, "InstanceData", _instanceDataBuffer);
                _batchingComputeShader.SetBuffer(_lodKernel, "LODData", _lodDataBuffer);
                _batchingComputeShader.SetVector("CameraPosition", _cameraPosition);
                
                // Dispatch LOD calculation
                int threadGroups = Mathf.CeilToInt(_allInstances.Count / (float)_computeThreadGroups);
                _batchingComputeShader.Dispatch(_lodKernel, threadGroups, 1, 1);
            }
        }
        
        #endregion
        
        #region Instance Batch Management
        
        private void UpdateInstanceBatches()
        {
            // Clear existing batches
            foreach (var batch in _gpuInstanceBatches.Values)
            {
                batch.InstanceCount = 0;
                batch.VisibleInstanceCount = 0;
            }
            
            // Group visible instances by batch criteria
            var visibleInstances = _allInstances.Where(i => i.IsVisible).ToList();
            
            foreach (var instance in visibleInstances)
            {
                string batchKey = GenerateBatchKey(instance);
                
                if (!_gpuInstanceBatches.ContainsKey(batchKey))
                {
                    CreateNewBatch(batchKey, instance);
                }
                
                AddInstanceToBatch(_gpuInstanceBatches[batchKey], instance);
            }
            
            // Update buffer data for modified batches
            UpdateBatchBufferData();
        }
        
        private string GenerateBatchKey(GPUPlantInstance instance)
        {
            List<string> keyParts = new List<string>();
            
            // Material-based batching
            if (_enableMaterialBatching && instance.Material != null)
            {
                keyParts.Add($"Mat_{instance.Material.name}");
            }
            
            // LOD-based batching
            if (_enableLODBatching)
            {
                keyParts.Add($"LOD_{instance.LODLevel}");
            }
            
            // Growth stage batching
            if (_enableGrowthStageBatching)
            {
                keyParts.Add($"Stage_{instance.GrowthStage}");
            }
            
            // Genetic batching
            if (_enableGeneticBatching)
            {
                keyParts.Add($"Genetic_{instance.GeneticHash}");
            }
            
            return string.Join("_", keyParts);
        }
        
        private void CreateNewBatch(string batchKey, GPUPlantInstance referenceInstance)
        {
            var newBatch = new GPUInstanceBatch
            {
                BatchKey = batchKey,
                Material = referenceInstance.Material,
                Mesh = referenceInstance.Mesh,
                LODLevel = referenceInstance.LODLevel,
                GrowthStage = referenceInstance.GrowthStage,
                InstanceMatrices = new Matrix4x4[_maxInstancesPerBatch],
                InstanceCount = 0,
                VisibleInstanceCount = 0,
                IsActive = true,
                CreationTime = Time.time
            };
            
            _gpuInstanceBatches[batchKey] = newBatch;
            OnBatchCreated?.Invoke(batchKey);
        }
        
        private void AddInstanceToBatch(GPUInstanceBatch batch, GPUPlantInstance instance)
        {
            if (batch.InstanceCount >= _maxInstancesPerBatch) return;
            
            batch.InstanceMatrices[batch.InstanceCount] = instance.InstanceMatrix;
            batch.InstanceCount++;
            
            if (instance.IsVisible)
            {
                batch.VisibleInstanceCount++;
            }
        }
        
        private void UpdateBatchBufferData()
        {
            foreach (var batch in _gpuInstanceBatches.Values)
            {
                if (batch.InstanceCount > 0)
                {
                    // Update instance data buffer for this batch
                    if (_instanceDataBuffer != null)
                    {
                        Matrix4x4[] batchData = new Matrix4x4[batch.InstanceCount];
                        System.Array.Copy(batch.InstanceMatrices, batchData, batch.InstanceCount);
                        
                        // Set buffer data (in a real implementation, you'd manage buffer offsets)
                        // This is a simplified version
                    }
                }
            }
        }
        
        private void UpdateInstanceInBatches(GPUPlantInstance instance)
        {
            // Remove from old batch groups
            RemoveInstanceFromBatchGroups(instance);
            
            // Add to new batch groups
            AddInstanceToBatchGroups(instance);
        }
        
        private void AddInstanceToBatchGroups(GPUPlantInstance instance)
        {
            // Add to growth stage batch
            if (_enableGrowthStageBatching && _growthStageBatches.ContainsKey(instance.GrowthStage))
            {
                _growthStageBatches[instance.GrowthStage].Instances.Add(instance);
            }
            
            // Add to LOD batch
            if (_enableLODBatching && _lodBatches.ContainsKey(instance.LODLevel))
            {
                _lodBatches[instance.LODLevel].Instances.Add(instance);
            }
            
            // Add to genetic batch
            if (_enableGeneticBatching)
            {
                string geneticKey = instance.GeneticHash;
                if (!_geneticBatches.ContainsKey(geneticKey))
                {
                    _geneticBatches[geneticKey] = new GPUBatchGroup
                    {
                        GroupId = $"Genetic_{geneticKey}",
                        BatchType = GPUBatchType.Genetic,
                        Instances = new List<GPUPlantInstance>(),
                        IsActive = true
                    };
                }
                _geneticBatches[geneticKey].Instances.Add(instance);
            }
        }
        
        private void RemoveInstanceFromBatchGroups(GPUPlantInstance instance)
        {
            // Remove from growth stage batch
            if (_enableGrowthStageBatching && _growthStageBatches.ContainsKey(instance.GrowthStage))
            {
                _growthStageBatches[instance.GrowthStage].Instances.Remove(instance);
            }
            
            // Remove from LOD batch
            if (_enableLODBatching && _lodBatches.ContainsKey(instance.LODLevel))
            {
                _lodBatches[instance.LODLevel].Instances.Remove(instance);
            }
            
            // Remove from genetic batch
            if (_enableGeneticBatching)
            {
                string geneticKey = instance.GeneticHash;
                if (_geneticBatches.ContainsKey(geneticKey))
                {
                    _geneticBatches[geneticKey].Instances.Remove(instance);
                }
            }
        }
        
        #endregion
        
        #region GPU Rendering
        
        private void RenderAllInstanceBatches()
        {
            if (!_enableGPUBatching || !_enableGPUInstancing) return;
            
            int totalDrawCalls = 0;
            int totalInstancesRendered = 0;
            
            foreach (var batch in _gpuInstanceBatches.Values)
            {
                if (batch.IsActive && batch.VisibleInstanceCount > 0)
                {
                    if (_enableIndirectRendering)
                    {
                        RenderBatchIndirect(batch);
                    }
                    else
                    {
                        RenderBatchDirect(batch);
                    }
                    
                    totalDrawCalls++;
                    totalInstancesRendered += batch.VisibleInstanceCount;
                }
            }
            
            _batchingMetrics.IndirectDrawCalls = totalDrawCalls;
            _batchingMetrics.RenderedInstances = totalInstancesRendered;
        }
        
        private void RenderBatchDirect(GPUInstanceBatch batch)
        {
            if (batch.Material == null || batch.Mesh == null) return;
            
            Matrix4x4[] visibleMatrices = new Matrix4x4[batch.VisibleInstanceCount];
            System.Array.Copy(batch.InstanceMatrices, visibleMatrices, batch.VisibleInstanceCount);
            
            Graphics.DrawMeshInstanced(
                batch.Mesh,
                0,
                batch.Material,
                visibleMatrices,
                batch.VisibleInstanceCount,
                null,
                ShadowCastingMode.On,
                true,
                0,
                _mainCamera
            );
        }
        
        private void RenderBatchIndirect(GPUInstanceBatch batch)
        {
            if (batch.Material == null || batch.Mesh == null || _indirectArgsBuffer == null) return;
            
            // Setup indirect args
            _indirectArgs[0] = batch.Mesh.GetIndexCount(0);
            _indirectArgs[1] = (uint)batch.VisibleInstanceCount;
            _indirectArgs[2] = batch.Mesh.GetIndexStart(0);
            _indirectArgs[3] = batch.Mesh.GetBaseVertex(0);
            _indirectArgs[4] = 0;
            
            _indirectArgsBuffer.SetData(_indirectArgs);
            
            // Set material properties
            batch.Material.SetBuffer("_InstanceData", _instanceDataBuffer);
            batch.Material.SetMatrix("_ViewProjectionMatrix", _vpMatrix);
            
            // Draw mesh instanced indirect
            Graphics.DrawMeshInstancedIndirect(
                batch.Mesh,
                0,
                batch.Material,
                new Bounds(_cameraPosition, Vector3.one * _maxRenderDistance * 2f),
                _indirectArgsBuffer,
                0,
                null,
                ShadowCastingMode.On,
                true,
                0,
                _mainCamera
            );
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Registers a plant for GPU batching and instancing
        /// </summary>
        public void RegisterPlantForGPUBatching(string plantId, GameObject plantGameObject, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, CannabisLODLevel lodLevel)
        {
            if (string.IsNullOrEmpty(plantId) || plantGameObject == null)
            {
                LogWarning("Cannot register plant for GPU batching: invalid parameters");
                return;
            }
            
            if (_allInstances.Count >= _maxTotalInstances)
            {
                LogWarning($"Maximum instance limit reached ({_maxTotalInstances})");
                return;
            }
            
            var gpuInstance = new GPUPlantInstance
            {
                PlantId = plantId,
                PlantGameObject = plantGameObject,
                Position = plantGameObject.transform.position,
                Scale = plantGameObject.transform.localScale,
                InstanceMatrix = plantGameObject.transform.localToWorldMatrix,
                GrowthStage = growthStage,
                LODLevel = lodLevel,
                Material = GetPlantMaterial(plantGameObject),
                Mesh = GetPlantMesh(plantGameObject),
                IsVisible = true,
                IsActive = true,
                GeneticHash = GenerateGeneticHash(plantId),
                RegistrationTime = Time.time
            };
            
            _allInstances.Add(gpuInstance);
            _instanceUpdateQueue.Enqueue(gpuInstance);
            
            OnInstanceCountChanged?.Invoke(_allInstances.Count, _maxTotalInstances);
            LogInfo($"Registered plant {plantId} for GPU batching");
        }
        
        /// <summary>
        /// Unregisters a plant from GPU batching
        /// </summary>
        public void UnregisterPlantFromGPUBatching(string plantId)
        {
            var instance = _allInstances.FirstOrDefault(i => i.PlantId == plantId);
            if (instance != null)
            {
                RemoveInstanceFromBatchGroups(instance);
                _allInstances.Remove(instance);
                
                OnInstanceCountChanged?.Invoke(_allInstances.Count, _maxTotalInstances);
                LogInfo($"Unregistered plant {plantId} from GPU batching");
            }
        }
        
        /// <summary>
        /// Updates plant properties for batching optimization
        /// </summary>
        public void UpdatePlantBatchingData(string plantId, Vector3 position, Vector3 scale, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, CannabisLODLevel lodLevel)
        {
            var instance = _allInstances.FirstOrDefault(i => i.PlantId == plantId);
            if (instance != null)
            {
                instance.Position = position;
                instance.Scale = scale;
                instance.InstanceMatrix = Matrix4x4.TRS(position, Quaternion.identity, scale);
                instance.GrowthStage = growthStage;
                instance.LODLevel = lodLevel;
                
                _instanceUpdateQueue.Enqueue(instance);
            }
        }
        
        /// <summary>
        /// Sets GPU batching quality level
        /// </summary>
        public void SetBatchingQuality(GPUBatchingQuality quality)
        {
            if (_batchingQuality != quality)
            {
                _batchingQuality = quality;
                ApplyBatchingQuality(quality);
                OnBatchingQualityChanged?.Invoke(quality);
                LogInfo($"GPU batching quality changed to {quality}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateCameraTracking()
        {
            if (_mainCamera != null)
            {
                _cameraPosition = _mainCamera.transform.position;
            }
        }
        
        private void UpdateFrameRateTracking()
        {
            _currentFrameRate = 1f / Time.deltaTime;
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
        
        private string GenerateGeneticHash(string plantId)
        {
            return (plantId.GetHashCode() % 100).ToString();
        }
        
        private void ApplyBatchingQuality(GPUBatchingQuality quality)
        {
            switch (quality)
            {
                case GPUBatchingQuality.Low:
                    _maxInstancesPerBatch = 256;
                    _batchingUpdateFrequency = 15f;
                    break;
                case GPUBatchingQuality.Medium:
                    _maxInstancesPerBatch = 512;
                    _batchingUpdateFrequency = 20f;
                    break;
                case GPUBatchingQuality.High:
                    _maxInstancesPerBatch = 1023;
                    _batchingUpdateFrequency = 30f;
                    break;
                case GPUBatchingQuality.Ultra:
                    _maxInstancesPerBatch = 1023;
                    _batchingUpdateFrequency = 60f;
                    break;
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (_batchingMetrics != null)
            {
                _batchingMetrics.TotalInstances = _allInstances.Count;
                _batchingMetrics.ActiveBatches = _gpuInstanceBatches.Count;
                _batchingMetrics.AverageFrameRate = _currentFrameRate;
                _batchingMetrics.BatchingUpdatesPerSecond = _batchingUpdatesProcessedThisFrame / Time.deltaTime;
                _batchingMetrics.GPUMemoryUsageMB = CalculateGPUMemoryUsage();
                _batchingMetrics.GPUUtilization = CalculateGPUUtilization();
                _batchingMetrics.CurrentBatchingQuality = _batchingQuality;
                
                OnBatchingMetricsUpdated?.Invoke(_batchingMetrics);
            }
        }
        
        private float CalculateGPUMemoryUsage()
        {
            float totalMemory = 0f;
            
            foreach (var buffer in _activeBuffers)
            {
                if (buffer != null)
                {
                    totalMemory += buffer.count * buffer.stride;
                }
            }
            
            return totalMemory / (1024f * 1024f); // Convert to MB
        }
        
        private float CalculateGPUUtilization()
        {
            if (_maxTotalInstances == 0) return 0f;
            return Mathf.Clamp01((float)_allInstances.Count / _maxTotalInstances);
        }
        
        private void AdjustBatchingQuality()
        {
            if (!_enableAdaptiveQuality) return;
            
            if (_currentFrameRate < _targetFrameRate * 0.8f)
            {
                // Reduce quality if performance is poor
                if (_batchingQuality > GPUBatchingQuality.Low)
                {
                    SetBatchingQuality((GPUBatchingQuality)((int)_batchingQuality - 1));
                }
            }
            else if (_currentFrameRate > _targetFrameRate * 1.2f)
            {
                // Increase quality if performance is good
                if (_batchingQuality < GPUBatchingQuality.Ultra)
                {
                    SetBatchingQuality((GPUBatchingQuality)((int)_batchingQuality + 1));
                }
            }
        }
        
        private void CleanupGPUResources()
        {
            // Dispose compute buffers
            foreach (var buffer in _activeBuffers)
            {
                buffer?.Dispose();
            }
            _activeBuffers.Clear();
            
            // Dispose buffer pool
            while (_bufferPool.Count > 0)
            {
                var buffer = _bufferPool.Dequeue();
                buffer?.Dispose();
            }
            
            // Dispose indirect buffers
            foreach (var buffer in _indirectBuffers.Values)
            {
                buffer?.Dispose();
            }
            _indirectBuffers.Clear();
            
            // Cleanup native arrays
            if (_instanceDataArray.IsCreated)
                _instanceDataArray.Dispose();
            if (_cullingResultsArray.IsCreated)
                _cullingResultsArray.Dispose();
        }
        
        private void ClearAllBatches()
        {
            _gpuInstanceBatches.Clear();
            _materialGroups.Clear();
            _instanceUpdateQueue.Clear();
            _allInstances.Clear();
            
            foreach (var group in _growthStageBatches.Values)
            {
                group.Instances.Clear();
            }
            
            foreach (var group in _lodBatches.Values)
            {
                group.Instances.Clear();
            }
            
            foreach (var group in _geneticBatches.Values)
            {
                group.Instances.Clear();
            }
        }
        
        private void DisconnectFromRenderingSystems()
        {
            _largeScaleRenderer = null;
            _lodSystem = null;
            _vfxTemplateManager = null;
            _speedTreeIntegration = null;
        }
        
        private void StartBatchingProcessing()
        {
            StartCoroutine(BatchingProcessingCoroutine());
        }
        
        private IEnumerator BatchingProcessingCoroutine()
        {
            while (_enableGPUBatching)
            {
                yield return new WaitForSeconds(1f / _batchingUpdateFrequency);
                
                if (_allInstances.Count > 0)
                {
                    // Additional background processing
                }
            }
        }
        
        #endregion
    }
    
    #region Supporting Data Structures and Jobs
    
    [System.Serializable]
    public class GPUPlantInstance
    {
        public string PlantId;
        public GameObject PlantGameObject;
        public Vector3 Position;
        public Vector3 Scale;
        public Matrix4x4 InstanceMatrix;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public CannabisLODLevel LODLevel;
        public Material Material;
        public Mesh Mesh;
        public bool IsVisible;
        public bool IsActive;
        public string GeneticHash;
        public float RegistrationTime;
    }
    
    [System.Serializable]
    public class GPUInstanceBatch
    {
        public string BatchKey;
        public Material Material;
        public Mesh Mesh;
        public CannabisLODLevel LODLevel;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public Matrix4x4[] InstanceMatrices;
        public int InstanceCount;
        public int VisibleInstanceCount;
        public bool IsActive;
        public float CreationTime;
    }
    
    [System.Serializable]
    public class GPUBatchGroup
    {
        public string GroupId;
        public GPUBatchType BatchType;
        public List<GPUPlantInstance> Instances;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class GPUBatchingMetrics
    {
        public int TotalInstances;
        public int ActiveBatches;
        public int CulledInstances;
        public int RenderedInstances;
        public float GPUMemoryUsageMB;
        public float AverageFrameRate;
        public float BatchingUpdatesPerSecond;
        public int IndirectDrawCalls;
        public float GPUUtilization;
        public GPUBatchingQuality CurrentBatchingQuality;
    }
    
    public struct GPUInstanceData
    {
        public Matrix4x4 InstanceMatrix;
        public Vector3 Position;
        public Vector3 Scale;
        public int LODLevel;
        public bool IsVisible;
        public float DistanceToCamera;
    }
    
    public struct GPUBatchingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GPUInstanceData> InstanceData;
        [WriteOnly] public NativeArray<bool> CullingResults;
        [ReadOnly] public Vector3 CameraPosition;
        [ReadOnly] public float MaxRenderDistance;
        [ReadOnly] public bool EnableFrustumCulling;
        [ReadOnly] public bool EnableDistanceCulling;
        
        public void Execute(int index)
        {
            var instance = InstanceData[index];
            
            bool isVisible = true;
            
            // Distance culling
            if (EnableDistanceCulling)
            {
                float distance = Vector3.Distance(instance.Position, CameraPosition);
                if (distance > MaxRenderDistance)
                {
                    isVisible = false;
                }
            }
            
            // Additional culling logic would go here
            
            CullingResults[index] = isVisible && instance.IsVisible;
        }
    }
    
    public enum GPUBatchingQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    public enum GPUBatchType
    {
        Material,
        LODLevel,
        GrowthStage,
        Genetic
    }
    
    #endregion
}