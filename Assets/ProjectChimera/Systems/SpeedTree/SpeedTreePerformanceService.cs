using UnityEngine;
using ProjectChimera.Data.SpeedTree;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8e: SpeedTreePerformanceService extracted from AdvancedSpeedTreeManager
    /// Handles performance optimization, LOD management, culling, and batching for cannabis plants.
    /// Ensures smooth 60+ FPS performance with hundreds of SpeedTree plant instances.
    /// </summary>
    public class SpeedTreePerformanceService
    {
        private readonly SpeedTreeLODConfigSO _lodConfig;
        private readonly Camera _mainCamera;
        private int _maxVisiblePlants;
        private readonly bool _enablePerformanceOptimization;
        private readonly float _performanceUpdateFrequency;
        
        private Dictionary<int, SpeedTreePlantData> _trackedPlants;
        private List<SpeedTreePlantData> _visiblePlants;
        private List<SpeedTreePlantData> _culledPlants;
        private Dictionary<int, float> _plantDistances;
        private Dictionary<int, int> _plantLODLevels;
        
        private Coroutine _performanceUpdateCoroutine;
        private MonoBehaviour _coroutineHost;
        
        // Performance metrics
        private SpeedTreePerformanceMetrics _performanceMetrics;
        private int _frameCount;
        private float _lastFPSUpdateTime;
        private float _currentFPS;
        private float _targetFPS = 60f;
        
        // Culling parameters
        private float _cullingDistance = 500f;
        private float _lodTransitionDistance = 100f;
        private int _maxLODLevel = 3;
        
        // Batching parameters
        private bool _enableGPUInstancing;
        private bool _enableDynamicBatching;
        private Dictionary<string, List<SpeedTreePlantData>> _batchGroups;
        
        // Events for performance monitoring
        public System.Action<float> OnFPSChanged;
        public System.Action<SpeedTreePerformanceMetrics> OnPerformanceMetricsUpdated;
        public System.Action<int> OnVisiblePlantsChanged;
        public System.Action<SpeedTreeQualityLevel> OnQualityLevelChanged;
        
        public SpeedTreePerformanceService(SpeedTreeLODConfigSO lodConfig, Camera mainCamera,
            MonoBehaviour coroutineHost, int maxVisiblePlants = 1000, bool enableOptimization = true,
            float updateFrequency = 0.1f)
        {
            _lodConfig = lodConfig;
            _mainCamera = mainCamera ?? Camera.main;
            _coroutineHost = coroutineHost;
            _maxVisiblePlants = maxVisiblePlants;
            _enablePerformanceOptimization = enableOptimization;
            _performanceUpdateFrequency = updateFrequency;
            
            _trackedPlants = new Dictionary<int, SpeedTreePlantData>();
            _visiblePlants = new List<SpeedTreePlantData>();
            _culledPlants = new List<SpeedTreePlantData>();
            _plantDistances = new Dictionary<int, float>();
            _plantLODLevels = new Dictionary<int, int>();
            _batchGroups = new Dictionary<string, List<SpeedTreePlantData>>();
            
            InitializePerformanceMetrics();
            InitializeLODSystem();
            InitializeBatchingSystem();
            
            if (_enablePerformanceOptimization && _coroutineHost != null)
            {
                StartPerformanceMonitoring();
            }
            
            Debug.Log($"[SpeedTreePerformanceService] Initialized with max visible plants: {_maxVisiblePlants}");
        }
        
        /// <summary>
        /// Registers a plant for performance tracking and optimization.
        /// </summary>
        public void RegisterPlant(SpeedTreePlantData plantData)
        {
            if (plantData == null)
            {
                Debug.LogWarning("[SpeedTreePerformanceService] Cannot register null plant data");
                return;
            }
            
            _trackedPlants[plantData.InstanceId] = plantData;
            
            // Initialize LOD level
            var distance = CalculateDistanceToCamera(plantData);
            var lodLevel = CalculateLODLevel(distance);
            _plantDistances[plantData.InstanceId] = distance;
            _plantLODLevels[plantData.InstanceId] = lodLevel;
            
            // Apply initial LOD settings
            ApplyLODLevel(plantData, lodLevel);
            
            // Add to appropriate batch group
            AddToBatchGroup(plantData);
            
            Debug.Log($"[SpeedTreePerformanceService] Registered plant {plantData.InstanceId} at distance {distance:F1}m, LOD {lodLevel}");
        }
        
        /// <summary>
        /// Unregisters a plant from performance tracking.
        /// </summary>
        public void UnregisterPlant(int instanceId)
        {
            if (_trackedPlants.TryGetValue(instanceId, out var plantData))
            {
                _trackedPlants.Remove(instanceId);
                _plantDistances.Remove(instanceId);
                _plantLODLevels.Remove(instanceId);
                
                _visiblePlants.Remove(plantData);
                _culledPlants.Remove(plantData);
                
                RemoveFromBatchGroup(plantData);
                
                Debug.Log($"[SpeedTreePerformanceService] Unregistered plant {instanceId}");
            }
        }
        
        /// <summary>
        /// Updates performance optimization settings.
        /// </summary>
        public void UpdatePerformanceSettings(SpeedTreePerformanceMetrics newMetrics)
        {
            if (newMetrics == null)
            {
                Debug.LogWarning("[SpeedTreePerformanceService] Cannot update with null performance metrics");
                return;
            }
            
            var oldQualityLevel = _performanceMetrics.QualityLevel;
            _performanceMetrics = newMetrics;
            
            _maxVisiblePlants = newMetrics.MaxVisiblePlants;
            _cullingDistance = newMetrics.CullingDistance;
            _enableGPUInstancing = newMetrics.GPUInstancingEnabled;
            _enableDynamicBatching = newMetrics.DynamicBatchingEnabled;
            
            // Apply quality changes if needed
            if (oldQualityLevel != newMetrics.QualityLevel)
            {
                ApplyQualityLevelToAllPlants(newMetrics.QualityLevel);
                OnQualityLevelChanged?.Invoke(newMetrics.QualityLevel);
            }
            
            OnPerformanceMetricsUpdated?.Invoke(newMetrics);
            
            Debug.Log($"[SpeedTreePerformanceService] Updated performance settings: Quality={newMetrics.QualityLevel}, MaxVisible={newMetrics.MaxVisiblePlants}");
        }
        
        /// <summary>
        /// Forces an immediate performance update cycle.
        /// </summary>
        public void ForcePerformanceUpdate()
        {
            UpdatePlantDistances();
            UpdateLODLevels();
            UpdateVisibility();
            UpdateBatching();
            UpdatePerformanceMetrics();
        }
        
        /// <summary>
        /// Gets current performance statistics.
        /// </summary>
        public SpeedTreePerformanceStats GetPerformanceStats()
        {
            return new SpeedTreePerformanceStats
            {
                TrackedPlants = _trackedPlants.Count,
                VisiblePlants = _visiblePlants.Count,
                CulledPlants = _culledPlants.Count,
                CurrentFPS = _currentFPS,
                TargetFPS = _targetFPS,
                QualityLevel = _performanceMetrics.QualityLevel,
                GPUInstancingEnabled = _enableGPUInstancing,
                DynamicBatchingEnabled = _enableDynamicBatching,
                LODDistribution = GetLODDistribution()
            };
        }
        
        /// <summary>
        /// Sets target FPS for performance optimization.
        /// </summary>
        public void SetTargetFPS(float targetFPS)
        {
            _targetFPS = Mathf.Clamp(targetFPS, 30f, 120f);
            Debug.Log($"[SpeedTreePerformanceService] Set target FPS to {_targetFPS}");
        }
        
        /// <summary>
        /// Enables or disables performance optimization.
        /// </summary>
        public void SetPerformanceOptimizationEnabled(bool enabled)
        {
            if (enabled && !_enablePerformanceOptimization && _coroutineHost != null)
            {
                StartPerformanceMonitoring();
            }
            else if (!enabled && _enablePerformanceOptimization)
            {
                StopPerformanceMonitoring();
            }
        }
        
        /// <summary>
        /// Cleans up performance monitoring resources.
        /// </summary>
        public void Cleanup()
        {
            StopPerformanceMonitoring();
            
            _trackedPlants.Clear();
            _visiblePlants.Clear();
            _culledPlants.Clear();
            _plantDistances.Clear();
            _plantLODLevels.Clear();
            _batchGroups.Clear();
            
            Debug.Log("[SpeedTreePerformanceService] Cleanup completed");
        }
        
        // Private methods
        private void InitializePerformanceMetrics()
        {
            _performanceMetrics = new SpeedTreePerformanceMetrics
            {
                MaxVisiblePlants = _maxVisiblePlants,
                CullingDistance = _cullingDistance,
                QualityLevel = SpeedTreeQualityLevel.High,
                GPUInstancingEnabled = true,
                DynamicBatchingEnabled = true,
                FrameRate = 60f
            };
        }
        
        private void InitializeLODSystem()
        {
            if (_lodConfig != null)
            {
                _cullingDistance = _lodConfig.CullingDistance;
                _lodTransitionDistance = _lodConfig.LODTransitionDistance;
                _maxLODLevel = _lodConfig.MaxLODLevel;
            }
        }
        
        private void InitializeBatchingSystem()
        {
            _enableGPUInstancing = _performanceMetrics.GPUInstancingEnabled;
            _enableDynamicBatching = _performanceMetrics.DynamicBatchingEnabled;
        }
        
        private void StartPerformanceMonitoring()
        {
            if (_coroutineHost != null)
            {
                _performanceUpdateCoroutine = _coroutineHost.StartCoroutine(PerformanceUpdateCoroutine());
                Debug.Log("[SpeedTreePerformanceService] Started performance monitoring");
            }
        }
        
        private void StopPerformanceMonitoring()
        {
            if (_performanceUpdateCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_performanceUpdateCoroutine);
                _performanceUpdateCoroutine = null;
                Debug.Log("[SpeedTreePerformanceService] Stopped performance monitoring");
            }
        }
        
        private IEnumerator PerformanceUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_performanceUpdateFrequency);
                
                UpdatePlantDistances();
                UpdateLODLevels();
                UpdateVisibility();
                UpdateBatching();
                UpdatePerformanceMetrics();
                CheckPerformanceThresholds();
            }
        }
        
        private void UpdatePlantDistances()
        {
            if (_mainCamera == null) return;
            
            var cameraPosition = _mainCamera.transform.position;
            
            foreach (var kvp in _trackedPlants)
            {
                var plantData = kvp.Value;
                var distance = Vector3.Distance(cameraPosition, plantData.Position);
                _plantDistances[kvp.Key] = distance;
                plantData.DistanceToCamera = distance;
            }
        }
        
        private void UpdateLODLevels()
        {
            foreach (var kvp in _plantDistances)
            {
                var instanceId = kvp.Key;
                var distance = kvp.Value;
                var newLODLevel = CalculateLODLevel(distance);
                
                if (_plantLODLevels.TryGetValue(instanceId, out var currentLODLevel) && 
                    currentLODLevel != newLODLevel)
                {
                    _plantLODLevels[instanceId] = newLODLevel;
                    
                    if (_trackedPlants.TryGetValue(instanceId, out var plantData))
                    {
                        ApplyLODLevel(plantData, newLODLevel);
                    }
                }
            }
        }
        
        private void UpdateVisibility()
        {
            _visiblePlants.Clear();
            _culledPlants.Clear();
            
            // Sort plants by distance
            var sortedPlants = _trackedPlants.Values
                .OrderBy(p => p.DistanceToCamera)
                .ToList();
            
            var visibleCount = 0;
            
            foreach (var plant in sortedPlants)
            {
                if (visibleCount < _maxVisiblePlants && plant.DistanceToCamera <= _cullingDistance)
                {
                    if (!plant.IsActive)
                    {
                        SetPlantVisibility(plant, true);
                    }
                    _visiblePlants.Add(plant);
                    visibleCount++;
                }
                else
                {
                    if (plant.IsActive)
                    {
                        SetPlantVisibility(plant, false);
                    }
                    _culledPlants.Add(plant);
                }
            }
            
            OnVisiblePlantsChanged?.Invoke(visibleCount);
        }
        
        private void UpdateBatching()
        {
            if (!_enableGPUInstancing && !_enableDynamicBatching) return;
            
            // Update batch groups based on current visible plants
            _batchGroups.Clear();
            
            foreach (var plant in _visiblePlants)
            {
                AddToBatchGroup(plant);
            }
            
            // Apply batching optimizations
            ApplyBatchingOptimizations();
        }
        
        private void UpdatePerformanceMetrics()
        {
            _frameCount++;
            
            if (Time.time - _lastFPSUpdateTime >= 1f)
            {
                _currentFPS = _frameCount / (Time.time - _lastFPSUpdateTime);
                _frameCount = 0;
                _lastFPSUpdateTime = Time.time;
                
                _performanceMetrics.FrameRate = _currentFPS;
                OnFPSChanged?.Invoke(_currentFPS);
            }
        }
        
        private void CheckPerformanceThresholds()
        {
            if (_currentFPS < _targetFPS * 0.9f) // 10% below target
            {
                ReduceQuality();
            }
            else if (_currentFPS > _targetFPS * 1.1f) // 10% above target
            {
                IncreaseQuality();
            }
        }
        
        private void ReduceQuality()
        {
            var currentLevel = _performanceMetrics.QualityLevel.QualityLevel;
            if (currentLevel > 1)
            {
                var newQualityLevel = new SpeedTreeQualityLevel 
                { 
                    QualityLevel = currentLevel - 1,
                    QualityName = GetQualityName(currentLevel - 1)
                };
                
                _performanceMetrics.QualityLevel = newQualityLevel;
                ApplyQualityLevelToAllPlants(newQualityLevel);
                OnQualityLevelChanged?.Invoke(newQualityLevel);
                
                Debug.Log($"[SpeedTreePerformanceService] Reduced quality to {newQualityLevel.QualityName} due to low FPS: {_currentFPS:F1}");
            }
        }
        
        private void IncreaseQuality()
        {
            var currentLevel = _performanceMetrics.QualityLevel.QualityLevel;
            if (currentLevel < 4)
            {
                var newQualityLevel = new SpeedTreeQualityLevel 
                { 
                    QualityLevel = currentLevel + 1,
                    QualityName = GetQualityName(currentLevel + 1)
                };
                
                _performanceMetrics.QualityLevel = newQualityLevel;
                ApplyQualityLevelToAllPlants(newQualityLevel);
                OnQualityLevelChanged?.Invoke(newQualityLevel);
                
                Debug.Log($"[SpeedTreePerformanceService] Increased quality to {newQualityLevel.QualityName} due to high FPS: {_currentFPS:F1}");
            }
        }
        
        private string GetQualityName(int level)
        {
            return level switch
            {
                1 => "Low",
                2 => "Medium", 
                3 => "High",
                4 => "Ultra",
                _ => "Medium"
            };
        }
        
        private float CalculateDistanceToCamera(SpeedTreePlantData plantData)
        {
            if (_mainCamera == null) return float.MaxValue;
            
            return Vector3.Distance(_mainCamera.transform.position, plantData.Position);
        }
        
        private int CalculateLODLevel(float distance)
        {
            if (distance <= _lodTransitionDistance * 0.3f) return 0; // Highest detail
            if (distance <= _lodTransitionDistance * 0.6f) return 1;
            if (distance <= _lodTransitionDistance) return 2;
            return _maxLODLevel; // Lowest detail
        }
        
        private void ApplyLODLevel(SpeedTreePlantData plantData, int lodLevel)
        {
            if (plantData?.Renderer == null) return;
            
            #if UNITY_SPEEDTREE
            // Apply LOD-specific settings to SpeedTree renderer
            var renderer = plantData.Renderer;
            
            // Adjust cross-fade and detail levels based on LOD
            switch (lodLevel)
            {
                case 0: // Highest detail
                    renderer.enableCrossFade = true;
                    break;
                case 1: // Medium detail
                    renderer.enableCrossFade = true;
                    break;
                case 2: // Low detail
                    renderer.enableCrossFade = false;
                    break;
                default: // Lowest detail
                    renderer.enableCrossFade = false;
                    break;
            }
            #endif
        }
        
        private void SetPlantVisibility(SpeedTreePlantData plantData, bool visible)
        {
            if (plantData?.Renderer?.gameObject == null) return;
            
            plantData.Renderer.gameObject.SetActive(visible);
            plantData.IsActive = visible;
        }
        
        private void AddToBatchGroup(SpeedTreePlantData plantData)
        {
            if (plantData?.StrainAsset == null) return;
            
            var batchKey = plantData.StrainAsset.StrainId;
            
            if (!_batchGroups.ContainsKey(batchKey))
            {
                _batchGroups[batchKey] = new List<SpeedTreePlantData>();
            }
            
            _batchGroups[batchKey].Add(plantData);
        }
        
        private void RemoveFromBatchGroup(SpeedTreePlantData plantData)
        {
            if (plantData?.StrainAsset == null) return;
            
            var batchKey = plantData.StrainAsset.StrainId;
            
            if (_batchGroups.TryGetValue(batchKey, out var batchGroup))
            {
                batchGroup.Remove(plantData);
                
                if (batchGroup.Count == 0)
                {
                    _batchGroups.Remove(batchKey);
                }
            }
        }
        
        private void ApplyBatchingOptimizations()
        {
            foreach (var kvp in _batchGroups)
            {
                var batchGroup = kvp.Value;
                
                if (batchGroup.Count > 1)
                {
                    // Apply GPU instancing or dynamic batching
                    if (_enableGPUInstancing)
                    {
                        ApplyGPUInstancing(batchGroup);
                    }
                    else if (_enableDynamicBatching)
                    {
                        ApplyDynamicBatching(batchGroup);
                    }
                }
            }
        }
        
        private void ApplyGPUInstancing(List<SpeedTreePlantData> batchGroup)
        {
            // Configure GPU instancing for similar plants
            foreach (var plant in batchGroup)
            {
                if (plant?.Renderer != null)
                {
                    // Enable GPU instancing properties on renderer
                    var rendererComponent = plant.Renderer.GetComponent<Renderer>();
                    if (rendererComponent != null)
                    {
                        // GPU instancing is controlled by the material and batching system
                        // Set additional properties as needed
                    }
                }
            }
        }
        
        private void ApplyDynamicBatching(List<SpeedTreePlantData> batchGroup)
        {
            // Configure dynamic batching for similar plants
            foreach (var plant in batchGroup)
            {
                if (plant?.Renderer != null)
                {
                    // Configure for dynamic batching
                    var rendererComponent = plant.Renderer.GetComponent<Renderer>();
                    if (rendererComponent != null)
                    {
                        // Apply batching-friendly settings
                    }
                }
            }
        }
        
        private void ApplyQualityLevelToAllPlants(SpeedTreeQualityLevel qualityLevel)
        {
            foreach (var plant in _trackedPlants.Values)
            {
                ApplyQualityLevelToPlant(plant, qualityLevel);
            }
        }
        
        private void ApplyQualityLevelToPlant(SpeedTreePlantData plantData, SpeedTreeQualityLevel qualityLevel)
        {
            if (plantData?.Renderer == null) return;
            
            #if UNITY_SPEEDTREE
            // Apply quality-specific settings to SpeedTree renderer
            var renderer = plantData.Renderer;
            
            switch (qualityLevel.QualityLevel)
            {
                case 1: // Low
                    renderer.fadeOutLength = 0.5f;
                    renderer.animateOnCulling = false;
                    break;
                case 2: // Medium
                    renderer.fadeOutLength = 1f;
                    renderer.animateOnCulling = false;
                    break;
                case 3: // High
                    renderer.fadeOutLength = 1.5f;
                    renderer.animateOnCulling = true;
                    break;
                case 4: // Ultra
                    renderer.fadeOutLength = 2f;
                    renderer.animateOnCulling = true;
                    break;
            }
            #endif
        }
        
        private Dictionary<int, int> GetLODDistribution()
        {
            var distribution = new Dictionary<int, int>();
            
            for (int i = 0; i <= _maxLODLevel; i++)
            {
                distribution[i] = 0;
            }
            
            foreach (var lodLevel in _plantLODLevels.Values)
            {
                if (distribution.ContainsKey(lodLevel))
                {
                    distribution[lodLevel]++;
                }
            }
            
            return distribution;
        }
    }
    
    /// <summary>
    /// Performance statistics for SpeedTree system.
    /// </summary>
    [System.Serializable]
    public class SpeedTreePerformanceStats
    {
        public int TrackedPlants;
        public int VisiblePlants;
        public int CulledPlants;
        public float CurrentFPS;
        public float TargetFPS;
        public SpeedTreeQualityLevel QualityLevel;
        public bool GPUInstancingEnabled;
        public bool DynamicBatchingEnabled;
        public Dictionary<int, int> LODDistribution;
    }
}