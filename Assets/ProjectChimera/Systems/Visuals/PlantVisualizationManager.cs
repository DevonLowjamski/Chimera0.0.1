using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ProjectChimera.Core;
using ProjectChimera.Data.Visuals;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
// Explicit alias to resolve Camera namespace conflict
using Camera = UnityEngine.Camera;

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Manages SpeedTree plant instances in the cultivation simulation, handling real-time
    /// parameter updates, LOD management, and performance optimization.
    /// </summary>
    public class PlantVisualizationManager : ChimeraManager
    {
        [Header("Visualization Settings")]
        [SerializeField] private Transform _plantContainer;
        [SerializeField] private LayerMask _plantLayers = 1 << 8; // Default to layer 8
        [SerializeField] private bool _enableDynamicLOD = true;
        [SerializeField] private bool _enableGPUInstancing = true;

        [Header("Update Frequencies")]
        [SerializeField, Range(0.1f, 5f)] private float _visualUpdateInterval = 1f;
        [SerializeField, Range(0.5f, 10f)] private float _lodUpdateInterval = 2f;
        [SerializeField, Range(1f, 30f)] private float _performanceCheckInterval = 5f;

        [Header("Performance Limits")]
        [SerializeField, Range(50, 1000)] private int _maxActiveInstances = 500;
        [SerializeField, Range(10, 100)] private int _maxHighDetailInstances = 50;
        [SerializeField, Range(5, 50)] private float _maxRenderDistance = 100f;

        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _logPerformanceMetrics = false;

        // Active plant instances
        private Dictionary<string, PlantVisualInstance> _activeInstances = new Dictionary<string, PlantVisualInstance>();
        private Dictionary<PlantStrainSO, SpeedTreePlantAssetSO> _strainAssetMap = new Dictionary<PlantStrainSO, SpeedTreePlantAssetSO>();
        
        // Performance tracking
        private int _frameCount = 0;
        private float _averageFPS = 60f;
        private Camera _mainCamera;
        
        // Coroutines
        private Coroutine _visualUpdateCoroutine;
        private Coroutine _lodUpdateCoroutine;
        private Coroutine _performanceCheckCoroutine;

        // Events
        public System.Action<PlantVisualInstance> OnPlantInstanceCreated;
        public System.Action<string> OnPlantInstanceDestroyed;
        public System.Action<float> OnPerformanceMetricsUpdated;

        protected override void OnManagerInitialize()
        {
            // Get main camera reference
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogWarning("[Chimera] PlantVisualizationManager: No main camera found. LOD calculations may not work properly.");
            }

            // Create plant container if not assigned
            if (_plantContainer == null)
            {
                GameObject container = new GameObject("Plant Instances");
                _plantContainer = container.transform;
                container.transform.SetParent(transform);
            }

            // Load SpeedTree asset mappings
            LoadSpeedTreeAssetMappings();

            // Start update coroutines
            StartUpdateCoroutines();

            Debug.Log("[Chimera] PlantVisualizationManager initialized successfully.");
        }

        protected override void OnManagerShutdown()
        {
            StopUpdateCoroutines();
            ClearAllInstances();
        }

        /// <summary>
        /// Creates a new plant visual instance for the given strain at the specified position.
        /// </summary>
        public PlantVisualInstance CreatePlantInstance(string instanceId, PlantStrainSO strain, Vector3 position, Quaternion rotation = default)
        {
            if (strain == null)
            {
                Debug.LogError("[Chimera] Cannot create plant instance with null strain.");
                return null;
            }

            if (_activeInstances.ContainsKey(instanceId))
            {
                Debug.LogWarning($"[Chimera] Plant instance '{instanceId}' already exists. Updating existing instance.");
                return UpdatePlantInstance(instanceId, strain, position, rotation);
            }

            if (_activeInstances.Count >= _maxActiveInstances)
            {
                Debug.LogWarning("[Chimera] Maximum plant instances reached. Consider optimizing or increasing limit.");
                return null;
            }

            // Get SpeedTree asset for strain
            if (!_strainAssetMap.TryGetValue(strain, out SpeedTreePlantAssetSO speedTreeAsset))
            {
                Debug.LogError($"[Chimera] No SpeedTree asset mapped for strain '{strain.StrainName}'.");
                return null;
            }

            // Create the instance
            PlantVisualInstance instance = CreateInstance(instanceId, strain, speedTreeAsset, position, rotation);
            if (instance != null)
            {
                _activeInstances[instanceId] = instance;
                OnPlantInstanceCreated?.Invoke(instance);
                
                if (_showDebugInfo)
                {
                    Debug.Log($"[Chimera] Created plant instance '{instanceId}' for strain '{strain.StrainName}' at {position}.");
                }
            }

            return instance;
        }

        /// <summary>
        /// Updates an existing plant instance with new cultivation conditions.
        /// </summary>
        public void UpdatePlantInstanceConditions(string instanceId, CultivationConditions conditions)
        {
            if (_activeInstances.TryGetValue(instanceId, out PlantVisualInstance instance))
            {
                instance.UpdateCultivationConditions(conditions);
            }
            else
            {
                Debug.LogWarning($"[Chimera] Plant instance '{instanceId}' not found for condition update.");
            }
        }

        /// <summary>
        /// Updates an existing plant instance's growth stage.
        /// </summary>
        public void UpdatePlantInstanceGrowthStage(string instanceId, PlantGrowthStage newStage)
        {
            if (_activeInstances.TryGetValue(instanceId, out PlantVisualInstance instance))
            {
                instance.UpdateGrowthStage(newStage);
            }
            else
            {
                Debug.LogWarning($"[Chimera] Plant instance '{instanceId}' not found for growth stage update.");
            }
        }

        /// <summary>
        /// Destroys a plant visual instance.
        /// </summary>
        public void DestroyPlantInstance(string instanceId)
        {
            if (_activeInstances.TryGetValue(instanceId, out PlantVisualInstance instance))
            {
                instance.Destroy();
                _activeInstances.Remove(instanceId);
                OnPlantInstanceDestroyed?.Invoke(instanceId);
                
                if (_showDebugInfo)
                {
                    Debug.Log($"[Chimera] Destroyed plant instance '{instanceId}'.");
                }
            }
        }

        /// <summary>
        /// Gets a plant visual instance by ID.
        /// </summary>
        public PlantVisualInstance GetPlantInstance(string instanceId)
        {
            _activeInstances.TryGetValue(instanceId, out PlantVisualInstance instance);
            return instance;
        }

        /// <summary>
        /// Registers a SpeedTree asset for a specific strain.
        /// </summary>
        public void RegisterStrainAsset(PlantStrainSO strain, SpeedTreePlantAssetSO speedTreeAsset)
        {
            if (strain != null && speedTreeAsset != null)
            {
                _strainAssetMap[strain] = speedTreeAsset;
                Debug.Log($"[Chimera] Registered SpeedTree asset for strain '{strain.StrainName}'.");
            }
        }

        /// <summary>
        /// Gets performance metrics for the visualization system.
        /// </summary>
        public VisualizationPerformanceMetrics GetPerformanceMetrics()
        {
            return new VisualizationPerformanceMetrics
            {
                ActiveInstances = _activeInstances.Count,
                MaxActiveInstances = _maxActiveInstances,
                AverageFPS = _averageFPS,
                HighDetailInstances = CountHighDetailInstances(),
                MaxHighDetailInstances = _maxHighDetailInstances,
                MemoryUsageMB = GetEstimatedMemoryUsage()
            };
        }

        private PlantVisualInstance CreateInstance(string instanceId, PlantStrainSO strain, SpeedTreePlantAssetSO speedTreeAsset, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = speedTreeAsset.SpeedTreePrefab;
            if (prefab == null)
            {
                Debug.LogError($"[Chimera] SpeedTree prefab is null for strain '{strain.StrainName}'.");
                return null;
            }

            GameObject instanceObject = Instantiate(prefab, position, rotation, _plantContainer);
            instanceObject.name = $"Plant_{instanceId}_{strain.StrainName}";
            instanceObject.layer = LayerMaskUtility.GetFirstLayerFromMask(_plantLayers);

            PlantVisualInstance instance = instanceObject.AddComponent<PlantVisualInstance>();
            instance.Initialize(instanceId, strain, speedTreeAsset, _mainCamera);

            return instance;
        }

        private PlantVisualInstance UpdatePlantInstance(string instanceId, PlantStrainSO strain, Vector3 position, Quaternion rotation)
        {
            if (_activeInstances.TryGetValue(instanceId, out PlantVisualInstance instance))
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                
                // Update strain if different
                if (instance.Strain != strain)
                {
                    // This would require recreating the instance with new SpeedTree asset
                    DestroyPlantInstance(instanceId);
                    return CreatePlantInstance(instanceId, strain, position, rotation);
                }
                
                return instance;
            }
            
            return null;
        }

        private void LoadSpeedTreeAssetMappings()
        {
            // This would typically load from a configuration asset or discover assets automatically
            SpeedTreePlantAssetSO[] speedTreeAssets = Resources.FindObjectsOfTypeAll<SpeedTreePlantAssetSO>();
            
            foreach (var asset in speedTreeAssets)
            {
                if (asset.TargetStrain != null)
                {
                    _strainAssetMap[asset.TargetStrain] = asset;
                }
            }
            
            Debug.Log($"[Chimera] Loaded {_strainAssetMap.Count} SpeedTree asset mappings.");
        }

        private void StartUpdateCoroutines()
        {
            _visualUpdateCoroutine = StartCoroutine(VisualUpdateLoop());
            
            if (_enableDynamicLOD)
            {
                _lodUpdateCoroutine = StartCoroutine(LODUpdateLoop());
            }
            
            if (_logPerformanceMetrics)
            {
                _performanceCheckCoroutine = StartCoroutine(PerformanceCheckLoop());
            }
        }

        private void StopUpdateCoroutines()
        {
            if (_visualUpdateCoroutine != null)
            {
                StopCoroutine(_visualUpdateCoroutine);
                _visualUpdateCoroutine = null;
            }
            
            if (_lodUpdateCoroutine != null)
            {
                StopCoroutine(_lodUpdateCoroutine);
                _lodUpdateCoroutine = null;
            }
            
            if (_performanceCheckCoroutine != null)
            {
                StopCoroutine(_performanceCheckCoroutine);
                _performanceCheckCoroutine = null;
            }
        }

        private IEnumerator VisualUpdateLoop()
        {
            while (IsInitialized)
            {
                yield return new WaitForSeconds(_visualUpdateInterval);
                
                foreach (var instance in _activeInstances.Values)
                {
                    instance.UpdateVisualParameters();
                }
            }
        }

        private IEnumerator LODUpdateLoop()
        {
            while (IsInitialized)
            {
                yield return new WaitForSeconds(_lodUpdateInterval);
                
                if (_mainCamera != null)
                {
                    UpdateLODForAllInstances();
                }
            }
        }

        private IEnumerator PerformanceCheckLoop()
        {
            while (IsInitialized)
            {
                yield return new WaitForSeconds(_performanceCheckInterval);
                
                UpdatePerformanceMetrics();
                OptimizePerformanceIfNeeded();
            }
        }

        private void UpdateLODForAllInstances()
        {
            Vector3 cameraPosition = _mainCamera.transform.position;
            
            foreach (var instance in _activeInstances.Values)
            {
                float distance = Vector3.Distance(cameraPosition, instance.transform.position);
                instance.UpdateLOD(distance);
            }
        }

        private void UpdatePerformanceMetrics()
        {
            _frameCount++;
            _averageFPS = 1f / Time.deltaTime;
            
            OnPerformanceMetricsUpdated?.Invoke(_averageFPS);
        }

        private void OptimizePerformanceIfNeeded()
        {
            if (_averageFPS < 30f && _activeInstances.Count > _maxActiveInstances * 0.5f)
            {
                // Reduce LOD quality for distant instances
                foreach (var instance in _activeInstances.Values)
                {
                    if (_mainCamera != null)
                    {
                        float distance = Vector3.Distance(_mainCamera.transform.position, instance.transform.position);
                        if (distance > _maxRenderDistance * 0.7f)
                        {
                            instance.ForceLODLevel(2); // Force low LOD
                        }
                    }
                }
                
                Debug.Log("[Chimera] Performance optimization applied: Reduced LOD for distant plants.");
            }
        }

        private int CountHighDetailInstances()
        {
            int count = 0;
            foreach (var instance in _activeInstances.Values)
            {
                if (instance.CurrentLODLevel == 0)
                {
                    count++;
                }
            }
            return count;
        }

        private float GetEstimatedMemoryUsage()
        {
            // Rough estimation based on instance count and typical SpeedTree memory usage
            return _activeInstances.Count * 2.5f; // ~2.5MB per instance estimate
        }

        private void ClearAllInstances()
        {
            foreach (var instance in _activeInstances.Values)
            {
                instance.Destroy();
            }
            _activeInstances.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (_showDebugInfo && _mainCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_mainCamera.transform.position, _maxRenderDistance);
                
                Gizmos.color = Color.green;
                foreach (var instance in _activeInstances.Values)
                {
                    Gizmos.DrawWireCube(instance.transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }

    /// <summary>
    /// Performance metrics for the visualization system.
    /// </summary>
    public struct VisualizationPerformanceMetrics
    {
        public int ActiveInstances;
        public int MaxActiveInstances;
        public float AverageFPS;
        public int HighDetailInstances;
        public int MaxHighDetailInstances;
        public float MemoryUsageMB;
    }

    /// <summary>
    /// Utility class for layer mask operations.
    /// </summary>
    public static class LayerMaskUtility
    {
        public static int GetFirstLayerFromMask(LayerMask layerMask)
        {
            int mask = layerMask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    return i;
                }
            }
            return 0; // Default layer
        }
    }
}