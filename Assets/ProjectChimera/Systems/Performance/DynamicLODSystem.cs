using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Dynamic Level of Detail (LOD) system with camera distance-based switching for optimal performance.
    /// Part of PC015-3c: Add dynamic LOD switching based on camera distance
    /// </summary>
    public class DynamicLODSystem : ChimeraManager
    {
        [Header("LOD Configuration")]
        [SerializeField] private bool _enableDynamicLOD = true;
        [SerializeField] private bool _enablePerformanceScaling = true;
        [SerializeField] private bool _enableCulling = true;
        [SerializeField] private float _lodUpdateInterval = 0.1f; // 10 FPS for LOD updates
        [SerializeField] private int _maxObjectsPerFrame = 50;
        [SerializeField] private float _cullingDistance = 200.0f;
        
        [Header("LOD Distance Thresholds")]
        [SerializeField] private float _lodLevel0Distance = 15.0f; // Highest detail
        [SerializeField] private float _lodLevel1Distance = 30.0f; // High detail
        [SerializeField] private float _lodLevel2Distance = 60.0f; // Medium detail
        [SerializeField] private float _lodLevel3Distance = 120.0f; // Low detail
        [SerializeField] private float _lodLevel4Distance = 200.0f; // Lowest detail/culling
        
        [Header("Performance Scaling")]
        [SerializeField] private float _targetFrameRate = 60.0f;
        [SerializeField] private float _performanceThreshold = 0.8f; // 80% of target
        [SerializeField] private float _scalingFactor = 1.2f;
        [SerializeField] private bool _adaptiveScaling = true;
        [SerializeField] private float _scalingCooldown = 2.0f;
        
        [Header("Plant-Specific LOD")]
        [SerializeField] private bool _enablePlantLOD = true;
        [SerializeField] private float _plantLODMultiplier = 1.0f;
        [SerializeField] private bool _enableLeafCulling = true;
        [SerializeField] private bool _enableBranchOptimization = true;
        [SerializeField] private float _leafCullingDistance = 25.0f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onLODLevelChanged;
        [SerializeField] private SimpleGameEventSO _onPerformanceScaling;
        [SerializeField] private SimpleGameEventSO _onObjectCulled;
        
        // Core LOD data
        private Dictionary<int, LODObject> _managedObjects = new Dictionary<int, LODObject>();
        private List<Camera> _activeCameras = new List<Camera>();
        private Dictionary<Camera, CameraLODData> _cameraLODData = new Dictionary<Camera, CameraLODData>();
        
        // Performance tracking
        private float _lastLODUpdate;
        private float _lastPerformanceCheck;
        private float _currentFrameRate;
        private int _objectsProcessedThisFrame;
        private int _totalLODSwitches;
        private int _totalObjectsCulled;
        
        // Adaptive scaling
        private float _currentLODScale = 1.0f;
        private float _lastScalingTime;
        private Queue<float> _frameTimeHistory = new Queue<float>();
        private const int FRAME_HISTORY_SIZE = 30;
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeLODSystem();
            FindActiveCameras();
            RegisterLODObjects();
            
            _lastLODUpdate = Time.time;
            _lastPerformanceCheck = Time.time;
            _lastScalingTime = Time.time;
            
            Debug.Log($"[DynamicLODSystem] Initialized with dynamic LOD: {_enableDynamicLOD}, " +
                     $"Performance scaling: {_enablePerformanceScaling}, Managed objects: {_managedObjects.Count}");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_enableDynamicLOD) return;
            
            UpdateFrameRateTracking();
            
            float deltaTime = Time.time - _lastLODUpdate;
            if (deltaTime >= _lodUpdateInterval)
            {
                _objectsProcessedThisFrame = 0;
                
                UpdateCameraData();
                ProcessLODUpdates();
                
                if (_enablePerformanceScaling && _adaptiveScaling)
                    ProcessPerformanceScaling();
                
                _lastLODUpdate = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            _managedObjects.Clear();
            _activeCameras.Clear();
            _cameraLODData.Clear();
            _frameTimeHistory.Clear();
            
            Debug.Log("[DynamicLODSystem] Manager shutdown completed - LOD system disabled");
        }
        
        #endregion
        
        #region LOD Object Management
        
        /// <summary>
        /// Register an object for LOD management
        /// </summary>
        public void RegisterLODObject(GameObject obj, LODConfiguration config = null)
        {
            if (obj == null) return;
            
            int instanceId = obj.GetInstanceID();
            if (_managedObjects.ContainsKey(instanceId)) return;
            
            var lodObject = new LODObject
            {
                GameObject = obj,
                Transform = obj.transform,
                InstanceId = instanceId,
                Configuration = config ?? CreateDefaultLODConfiguration(obj),
                CurrentLODLevel = 0,
                LastUpdateTime = Time.time,
                IsVisible = true,
                IsCulled = false,
                DistanceToCamera = 0.0f,
                LODComponents = CollectLODComponents(obj)
            };
            
            _managedObjects[instanceId] = lodObject;
            
            Debug.Log($"[DynamicLODSystem] Registered LOD object: {obj.name}");
        }
        
        /// <summary>
        /// Unregister an object from LOD management
        /// </summary>
        public void UnregisterLODObject(GameObject obj)
        {
            if (obj == null) return;
            
            int instanceId = obj.GetInstanceID();
            if (_managedObjects.Remove(instanceId))
            {
                Debug.Log($"[DynamicLODSystem] Unregistered LOD object: {obj.name}");
            }
        }
        
        /// <summary>
        /// Update LOD level for a specific object
        /// </summary>
        public void UpdateObjectLOD(LODObject lodObject, Camera camera)
        {
            if (lodObject?.GameObject == null || camera == null) return;
            
            // Calculate distance to camera
            float distance = Vector3.Distance(lodObject.Transform.position, camera.transform.position);
            lodObject.DistanceToCamera = distance;
            
            // Apply LOD scaling
            distance /= _currentLODScale * lodObject.Configuration.DistanceMultiplier;
            
            // Determine appropriate LOD level
            int newLODLevel = CalculateLODLevel(distance);
            
            // Check if LOD level changed
            if (newLODLevel != lodObject.CurrentLODLevel)
            {
                ApplyLODLevel(lodObject, newLODLevel);
                lodObject.CurrentLODLevel = newLODLevel;
                lodObject.LastUpdateTime = Time.time;
                _totalLODSwitches++;
                
                _onLODLevelChanged?.Raise();
            }
            
            // Handle culling
            bool shouldBeCulled = distance > _cullingDistance || newLODLevel >= 5;
            if (shouldBeCulled != lodObject.IsCulled)
            {
                SetObjectCulled(lodObject, shouldBeCulled);
            }
        }
        
        #endregion
        
        #region LOD Level Calculation and Application
        
        /// <summary>
        /// Calculate appropriate LOD level based on distance
        /// </summary>
        private int CalculateLODLevel(float distance)
        {
            if (distance <= _lodLevel0Distance) return 0; // Highest detail
            if (distance <= _lodLevel1Distance) return 1; // High detail
            if (distance <= _lodLevel2Distance) return 2; // Medium detail
            if (distance <= _lodLevel3Distance) return 3; // Low detail
            if (distance <= _lodLevel4Distance) return 4; // Lowest detail
            return 5; // Culled
        }
        
        /// <summary>
        /// Apply LOD level to object
        /// </summary>
        private void ApplyLODLevel(LODObject lodObject, int lodLevel)
        {
            var config = lodObject.Configuration;
            var components = lodObject.LODComponents;
            
            // Apply renderer LOD
            if (components.Renderers != null)
            {
                foreach (var renderer in components.Renderers)
                {
                    if (renderer == null) continue;
                    
                    // Adjust shadow casting
                    renderer.shadowCastingMode = lodLevel <= 1 ? 
                        UnityEngine.Rendering.ShadowCastingMode.On : 
                        UnityEngine.Rendering.ShadowCastingMode.Off;
                    
                    // Adjust receive shadows
                    renderer.receiveShadows = lodLevel <= 2;
                    
                    // Apply material LOD if available
                    ApplyMaterialLOD(renderer, lodLevel);
                }
            }
            
            // Apply mesh LOD
            if (components.MeshFilters != null && config.MeshLODs.Count > lodLevel)
            {
                for (int i = 0; i < components.MeshFilters.Count && i < config.MeshLODs[lodLevel].Count; i++)
                {
                    var meshFilter = components.MeshFilters[i];
                    if (meshFilter != null && config.MeshLODs[lodLevel][i] != null)
                    {
                        meshFilter.mesh = config.MeshLODs[lodLevel][i];
                    }
                }
            }
            
            // Apply particle system LOD
            if (components.ParticleSystems != null)
            {
                foreach (var particleSystem in components.ParticleSystems)
                {
                    if (particleSystem == null) continue;
                    
                    var main = particleSystem.main;
                    main.maxParticles = Mathf.RoundToInt(config.BaseParticleCount * GetLODMultiplier(lodLevel));
                }
            }
            
            // Apply plant-specific LOD
            if (_enablePlantLOD && components.PlantComponents != null)
            {
                ApplyPlantLOD(components.PlantComponents, lodLevel, lodObject.DistanceToCamera);
            }
            
            // Apply collider LOD
            if (components.Colliders != null)
            {
                foreach (var collider in components.Colliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = lodLevel <= 3; // Disable colliders at lowest LOD
                    }
                }
            }
            
            Debug.Log($"[DynamicLODSystem] Applied LOD level {lodLevel} to {lodObject.GameObject.name}");
        }
        
        /// <summary>
        /// Apply material-specific LOD optimizations
        /// </summary>
        private void ApplyMaterialLOD(Renderer renderer, int lodLevel)
        {
            if (renderer.materials == null) return;
            
            foreach (var material in renderer.materials)
            {
                if (material == null) continue;
                
                // Adjust texture quality based on LOD level
                float textureScale = GetLODMultiplier(lodLevel);
                
                // Set shader LOD level
                material.SetFloat("_LODLevel", lodLevel);
                
                // Disable expensive shader features at higher LOD levels
                if (lodLevel >= 3)
                {
                    material.DisableKeyword("_NORMALMAP");
                    material.DisableKeyword("_PARALLAXMAP");
                    material.DisableKeyword("_DETAIL_MULX2");
                }
                
                // Adjust material properties for performance
                if (material.HasProperty("_DetailScale"))
                {
                    material.SetFloat("_DetailScale", textureScale);
                }
            }
        }
        
        /// <summary>
        /// Apply plant-specific LOD optimizations
        /// </summary>
        private void ApplyPlantLOD(List<Component> plantComponents, int lodLevel, float distance)
        {
            foreach (var component in plantComponents)
            {
                if (component == null) continue;
                
                // Handle leaf culling
                if (_enableLeafCulling && distance > _leafCullingDistance)
                {
                    if (component.name.ToLower().Contains("leaf"))
                    {
                        component.gameObject.SetActive(false);
                    }
                }
                
                // Handle branch optimization
                if (_enableBranchOptimization && lodLevel >= 2)
                {
                    if (component.name.ToLower().Contains("branch") && 
                        component.name.ToLower().Contains("small"))
                    {
                        component.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        #endregion
        
        #region Performance Scaling
        
        /// <summary>
        /// Process adaptive performance scaling based on frame rate
        /// </summary>
        private void ProcessPerformanceScaling()
        {
            float timeSinceLastScaling = Time.time - _lastScalingTime;
            if (timeSinceLastScaling < _scalingCooldown) return;
            
            float averageFrameTime = CalculateAverageFrameTime();
            float currentFPS = 1.0f / averageFrameTime;
            float targetFPS = _targetFrameRate * _performanceThreshold;
            
            bool needsScaling = false;
            float newScale = _currentLODScale;
            
            if (currentFPS < targetFPS)
            {
                // Performance is below threshold, reduce LOD quality
                newScale = Mathf.Min(_currentLODScale * _scalingFactor, 3.0f);
                needsScaling = true;
            }
            else if (currentFPS > _targetFrameRate * 1.1f && _currentLODScale > 1.0f)
            {
                // Performance is good, increase LOD quality
                newScale = Mathf.Max(_currentLODScale / _scalingFactor, 1.0f);
                needsScaling = true;
            }
            
            if (needsScaling)
            {
                _currentLODScale = newScale;
                _lastScalingTime = Time.time;
                
                _onPerformanceScaling?.Raise();
                
                Debug.Log($"[DynamicLODSystem] Performance scaling applied: {newScale:F2} (FPS: {currentFPS:F1})");
            }
        }
        
        /// <summary>
        /// Update frame rate tracking
        /// </summary>
        private void UpdateFrameRateTracking()
        {
            _frameTimeHistory.Enqueue(Time.unscaledDeltaTime);
            
            if (_frameTimeHistory.Count > FRAME_HISTORY_SIZE)
            {
                _frameTimeHistory.Dequeue();
            }
            
            _currentFrameRate = 1.0f / Time.unscaledDeltaTime;
        }
        
        /// <summary>
        /// Calculate average frame time from history
        /// </summary>
        private float CalculateAverageFrameTime()
        {
            if (_frameTimeHistory.Count == 0) return 1.0f / 60.0f;
            
            return _frameTimeHistory.Average();
        }
        
        #endregion
        
        #region Camera Management
        
        /// <summary>
        /// Update camera data for LOD calculations
        /// </summary>
        private void UpdateCameraData()
        {
            FindActiveCameras();
            
            foreach (var camera in _activeCameras)
            {
                if (!_cameraLODData.ContainsKey(camera))
                {
                    _cameraLODData[camera] = new CameraLODData();
                }
                
                var cameraData = _cameraLODData[camera];
                cameraData.Position = camera.transform.position;
                cameraData.Forward = camera.transform.forward;
                cameraData.FieldOfView = camera.fieldOfView;
                cameraData.FarClipPlane = camera.farClipPlane;
            }
        }
        
        /// <summary>
        /// Find all active cameras in the scene
        /// </summary>
        private void FindActiveCameras()
        {
            _activeCameras.Clear();
            
            Camera[] allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in allCameras)
            {
                if (camera.isActiveAndEnabled)
                {
                    _activeCameras.Add(camera);
                }
            }
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get LOD system performance metrics
        /// </summary>
        public LODSystemMetrics GetPerformanceMetrics()
        {
            return new LODSystemMetrics
            {
                ManagedObjects = _managedObjects.Count,
                ActiveCameras = _activeCameras.Count,
                TotalLODSwitches = _totalLODSwitches,
                TotalObjectsCulled = _totalObjectsCulled,
                CurrentLODScale = _currentLODScale,
                CurrentFrameRate = _currentFrameRate,
                TargetFrameRate = _targetFrameRate,
                LODUpdateInterval = _lodUpdateInterval,
                LastUpdateTime = DateTime.Now
            };
        }
        
        /// <summary>
        /// Force LOD update for all managed objects
        /// </summary>
        public void ForceUpdateAllLODs()
        {
            foreach (var lodObject in _managedObjects.Values)
            {
                if (_activeCameras.Count > 0)
                {
                    UpdateObjectLOD(lodObject, _activeCameras[0]);
                }
            }
            
            Debug.Log($"[DynamicLODSystem] Force updated LOD for {_managedObjects.Count} objects");
        }
        
        /// <summary>
        /// Set LOD system enabled state
        /// </summary>
        public void SetLODEnabled(bool enabled)
        {
            _enableDynamicLOD = enabled;
            
            if (!enabled)
            {
                // Reset all objects to highest LOD
                foreach (var lodObject in _managedObjects.Values)
                {
                    ApplyLODLevel(lodObject, 0);
                    SetObjectCulled(lodObject, false);
                }
            }
            
            Debug.Log($"[DynamicLODSystem] LOD system {(enabled ? "enabled" : "disabled")}");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeLODSystem()
        {
            _managedObjects.Clear();
            _activeCameras.Clear();
            _cameraLODData.Clear();
            _frameTimeHistory.Clear();
            
            _totalLODSwitches = 0;
            _totalObjectsCulled = 0;
            _currentLODScale = 1.0f;
        }
        
        private void RegisterLODObjects()
        {
            // Find all objects with LOD components in the scene
            var lodGroups = UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);
            foreach (var lodGroup in lodGroups)
            {
                RegisterLODObject(lodGroup.gameObject);
            }
            
            // Find all renderers without LOD groups
            var allRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var renderer in allRenderers)
            {
                if (renderer.GetComponent<LODGroup>() == null)
                {
                    RegisterLODObject(renderer.gameObject);
                }
            }
        }
        
        private void ProcessLODUpdates()
        {
            var objectsToUpdate = _managedObjects.Values
                .Where(obj => obj.GameObject != null && !obj.IsCulled)
                .OrderBy(obj => obj.DistanceToCamera)
                .Take(_maxObjectsPerFrame)
                .ToList();
            
            foreach (var lodObject in objectsToUpdate)
            {
                if (_activeCameras.Count > 0)
                {
                    UpdateObjectLOD(lodObject, _activeCameras[0]); // Use primary camera
                }
                
                _objectsProcessedThisFrame++;
                
                if (_objectsProcessedThisFrame >= _maxObjectsPerFrame)
                    break;
            }
        }
        
        private void SetObjectCulled(LODObject lodObject, bool culled)
        {
            if (lodObject.IsCulled == culled) return;
            
            lodObject.IsCulled = culled;
            lodObject.GameObject.SetActive(!culled);
            
            if (culled)
                _totalObjectsCulled++;
            
            _onObjectCulled?.Raise();
        }
        
        private LODConfiguration CreateDefaultLODConfiguration(GameObject obj)
        {
            return new LODConfiguration
            {
                DistanceMultiplier = 1.0f,
                BaseParticleCount = 100,
                MeshLODs = new List<List<Mesh>>(),
                CustomProperties = new Dictionary<string, object>()
            };
        }
        
        private LODComponents CollectLODComponents(GameObject obj)
        {
            return new LODComponents
            {
                Renderers = obj.GetComponentsInChildren<Renderer>().ToList(),
                MeshFilters = obj.GetComponentsInChildren<MeshFilter>().ToList(),
                ParticleSystems = obj.GetComponentsInChildren<ParticleSystem>().ToList(),
                Colliders = obj.GetComponentsInChildren<Collider>().ToList(),
                PlantComponents = obj.GetComponentsInChildren<Component>()
                    .Where(c => c.name.ToLower().Contains("plant") || 
                               c.name.ToLower().Contains("leaf") || 
                               c.name.ToLower().Contains("branch"))
                    .ToList()
            };
        }
        
        private float GetLODMultiplier(int lodLevel)
        {
            return lodLevel switch
            {
                0 => 1.0f,    // Highest detail
                1 => 0.8f,    // High detail
                2 => 0.6f,    // Medium detail
                3 => 0.4f,    // Low detail
                4 => 0.2f,    // Lowest detail
                _ => 0.0f     // Culled
            };
        }
        
        #endregion
    }
}