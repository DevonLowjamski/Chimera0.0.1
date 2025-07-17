using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Environment;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

// Explicit namespace alias to resolve FacilityType ambiguity
using DataFacilityType = ProjectChimera.Data.Facilities.FacilityType;

namespace ProjectChimera.Systems.Prefabs
{
    // Missing data structures for PrefabLibraryManager
    [System.Serializable]
    public class PrefabSystemMetrics
    {
        public int TotalPrefabsLoaded;
        public int ActiveInstances;
        public int PooledInstances;
        public float MemoryUsage;
        public float LoadTime;
        public int InstantiationsThisFrame;
        public int DestructionsThisFrame;
        public Dictionary<string, int> CategoryCounts = new Dictionary<string, int>();
        public DateTime LastUpdate;
    }

    [System.Serializable]
    public class PrefabSystemStatus
    {
        public bool IsInitialized;
        public bool IsProcessingQueue;
        public int QueuedRequests;
        public float SystemLoad;
        public List<string> ActiveCategories = new List<string>();
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
        
        // Missing properties for Error Wave 139
        public int ActiveInstances;
        public int LoadedPrefabs;
        public float MemoryUsage;
        
        // Missing property for Error Wave 141
        public int PooledInstances;
    }

    public enum PrefabCategory
    {
        Plant,
        Plants,
        Equipment,
        Facility,
        Facilities,
        Environmental,
        UI,
        Effects,
        Audio,
        Particle
    }

    [System.Serializable]
    public class PrefabLoadRequest
    {
        public string PrefabId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public System.Action<GameObject> Callback;
        public float Priority = 1f;
        public DateTime RequestTime;
    }

    [System.Serializable]
    public class PrefabInstance
    {
        public GameObject GameObject;
        public string PrefabId;
        public DateTime CreationTime;
        public bool IsPooled;
        public Transform OriginalParent;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
        
        // Compatibility properties for existing code
        public GameObject Instance 
        { 
            get => GameObject; 
            set => GameObject = value; 
        }
        
        public float LastAccessTime { get; set; }
    }

    [System.Serializable]
    public class ObjectPool
    {
        public string PrefabId;
        public GameObject PrefabTemplate;
        public Queue<GameObject> AvailableInstances = new Queue<GameObject>();
        public List<GameObject> ActiveInstances = new List<GameObject>();
        public int MaxSize = 50;
        public Transform PoolContainer;
        
        // Constructor to fix CS1729 error
        public ObjectPool(GameObject prefabTemplate, int maxSize, Transform poolContainer)
        {
            PrefabTemplate = prefabTemplate;
            MaxSize = maxSize;
            PoolContainer = poolContainer;
            PrefabId = prefabTemplate != null ? prefabTemplate.name : "Unknown";
            AvailableInstances = new Queue<GameObject>();
            ActiveInstances = new List<GameObject>();
        }
        
        // Get method to fix CS1061 error
        public GameObject Get()
        {
            if (AvailableInstances.Count > 0)
            {
                var instance = AvailableInstances.Dequeue();
                if (instance != null)
                {
                    ActiveInstances.Add(instance);
                    return instance;
                }
            }
            
            // Create new instance if pool is empty
            if (PrefabTemplate != null && ActiveInstances.Count < MaxSize)
            {
                var newInstance = UnityEngine.Object.Instantiate(PrefabTemplate, PoolContainer);
                newInstance.SetActive(false);
                ActiveInstances.Add(newInstance);
                return newInstance;
            }
            
            return null;
        }
        
        // Return method for completeness
        public void Return(GameObject instance)
        {
            if (instance != null && ActiveInstances.Contains(instance))
            {
                ActiveInstances.Remove(instance);
                instance.SetActive(false);
                instance.transform.SetParent(PoolContainer);
                AvailableInstances.Enqueue(instance);
            }
        }
        
        // Optimize method for pool management
        public void Optimize()
        {
            // Remove null instances from available queue
            var validInstances = new Queue<GameObject>();
            while (AvailableInstances.Count > 0)
            {
                var instance = AvailableInstances.Dequeue();
                if (instance != null)
                {
                    validInstances.Enqueue(instance);
                }
            }
            AvailableInstances = validInstances;
            
            // Remove null instances from active list
            ActiveInstances.RemoveAll(instance => instance == null);
            
            // Trim pool if it's too large
            while (AvailableInstances.Count > MaxSize / 2)
            {
                var instance = AvailableInstances.Dequeue();
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }
        
        // Property for pool metrics
        public int PooledCount => AvailableInstances.Count;
        
        // Preload method for Error Wave 141 compatibility
        public void Preload(int count)
        {
            for (int i = 0; i < count && AvailableInstances.Count < MaxSize; i++)
            {
                if (PrefabTemplate != null)
                {
                    var instance = UnityEngine.Object.Instantiate(PrefabTemplate, PoolContainer);
                    instance.SetActive(false);
                    AvailableInstances.Enqueue(instance);
                }
            }
        }
        
        // SetSize method for Error Wave 141 compatibility
        public void SetSize(int newSize)
        {
            MaxSize = newSize;
            
            // If new size is smaller, remove excess instances
            while (AvailableInstances.Count > MaxSize)
            {
                var instance = AvailableInstances.Dequeue();
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }
        
        // Clear method for Error Wave 141 compatibility
        public void Clear()
        {
            // Clear available instances
            while (AvailableInstances.Count > 0)
            {
                var instance = AvailableInstances.Dequeue();
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            
            // Clear active instances
            foreach (var instance in ActiveInstances.ToList())
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            ActiveInstances.Clear();
        }
    }

    /// <summary>
    /// Central prefab library management system for Project Chimera.
    /// Manages creation, instantiation, and lifecycle of all game objects
    /// including plants, equipment, facilities, and environmental elements.
    /// </summary>
    public class PrefabLibraryManager : ChimeraManager
    {
        [Header("Prefab Categories")]
        [SerializeField] private PlantPrefabLibrary _plantPrefabs;
        [SerializeField] private EquipmentPrefabLibrary _equipmentPrefabs;
        [SerializeField] private FacilityPrefabLibrary _facilityPrefabs;
        [SerializeField] private EnvironmentalPrefabLibrary _environmentalPrefabs;
        [SerializeField] private UIPrefabLibrary _uiPrefabs;
        [SerializeField] private EffectsPrefabLibrary _effectsPrefabs;
        
        [Header("Instantiation Settings")]
        [SerializeField] private bool _enablePooling = true;
        [SerializeField] private bool _enableAsyncLoading = true;
        [SerializeField] private int _defaultPoolSize = 50;
        [SerializeField] private Transform _poolContainer;
        
        [Header("Performance")]
        [SerializeField] private int _maxInstancesPerFrame = 10;
        [SerializeField] private float _cleanupInterval = 60f;
        [SerializeField] private bool _enableLODSystem = true;
        [SerializeField] private float _lodDistance = 50f;
        
        // Object Pooling
        private Dictionary<string, ObjectPool> _objectPools = new Dictionary<string, ObjectPool>();
        private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
        private List<PrefabInstance> _activeInstances = new List<PrefabInstance>();
        
        // Async Loading
        private Queue<PrefabLoadRequest> _loadQueue = new Queue<PrefabLoadRequest>();
        private bool _isProcessingQueue = false;
        
        // Performance Tracking
        private int _instantiationsThisFrame = 0;
        private float _lastCleanup = 0f;
        private PrefabSystemMetrics _metrics;
        
        // Events
        public System.Action<string, GameObject> OnPrefabLoaded;
        public System.Action<GameObject> OnInstanceCreated;
        public System.Action<GameObject> OnInstanceDestroyed;
        public System.Action<PrefabSystemMetrics> OnMetricsUpdated;
        
        // Properties
        public int ActiveInstanceCount => _activeInstances.Count;
        public int LoadedPrefabCount => _prefabCache.Count;
        public bool IsProcessingQueue => _isProcessingQueue;
        public PrefabSystemMetrics SystemMetrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializePrefabLibraries();
            SetupObjectPools();
            InitializeMetrics();
            StartCleanupRoutine();
        }
        
        private void Update()
        {
            _instantiationsThisFrame = 0;
            
            if (_enableAsyncLoading)
            {
                ProcessLoadQueue();
            }
            
            if (Time.time - _lastCleanup >= _cleanupInterval)
            {
                PerformCleanup();
                _lastCleanup = Time.time;
            }
            
            UpdateMetrics();
        }
        
        #region Initialization
        
        private void InitializePrefabLibraries()
        {
            // Initialize plant prefab library
            if (_plantPrefabs == null)
            {
                _plantPrefabs = CreatePlantPrefabLibrary();
            }
            
            // Initialize equipment prefab library
            if (_equipmentPrefabs == null)
            {
                _equipmentPrefabs = CreateEquipmentPrefabLibrary();
            }
            
            // Initialize facility prefab library
            if (_facilityPrefabs == null)
            {
                _facilityPrefabs = CreateFacilityPrefabLibrary();
            }
            
            // Initialize environmental prefab library
            if (_environmentalPrefabs == null)
            {
                _environmentalPrefabs = CreateEnvironmentalPrefabLibrary();
            }
            
            // Initialize UI prefab library
            if (_uiPrefabs == null)
            {
                _uiPrefabs = CreateUIPrefabLibrary();
            }
            
            // Initialize effects prefab library
            if (_effectsPrefabs == null)
            {
                _effectsPrefabs = CreateEffectsPrefabLibrary();
            }
            
            // Cache all prefabs for quick access
            CacheAllPrefabs();
        }
        
        private PlantPrefabLibrary CreatePlantPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<PlantPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private EquipmentPrefabLibrary CreateEquipmentPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<EquipmentPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private FacilityPrefabLibrary CreateFacilityPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<FacilityPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private EnvironmentalPrefabLibrary CreateEnvironmentalPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<EnvironmentalPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private UIPrefabLibrary CreateUIPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<UIPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private EffectsPrefabLibrary CreateEffectsPrefabLibrary()
        {
            var library = ScriptableObject.CreateInstance<EffectsPrefabLibrary>();
            library.InitializeDefaults();
            return library;
        }
        
        private void CacheAllPrefabs()
        {
            // Cache plant prefabs
            foreach (var plant in _plantPrefabs.PlantPrefabs)
            {
                if (plant.Prefab != null)
                {
                    _prefabCache[plant.PrefabId] = plant.Prefab;
                }
            }
            
            // Cache equipment prefabs
            foreach (var equipment in _equipmentPrefabs.EquipmentPrefabs)
            {
                if (equipment.Prefab != null)
                {
                    _prefabCache[equipment.PrefabId] = equipment.Prefab;
                }
            }
            
            // Cache facility prefabs
            foreach (var facility in _facilityPrefabs.FacilityPrefabs)
            {
                if (facility.Prefab != null)
                {
                    _prefabCache[facility.PrefabId] = facility.Prefab;
                }
            }
            
            // Cache environmental prefabs
            foreach (var env in _environmentalPrefabs.EnvironmentalPrefabs)
            {
                if (env.Prefab != null)
                {
                    _prefabCache[env.PrefabId] = env.Prefab;
                }
            }
            
            // Cache UI prefabs
            foreach (var ui in _uiPrefabs.UIPrefabs)
            {
                if (ui.Prefab != null)
                {
                    _prefabCache[ui.PrefabId] = ui.Prefab;
                }
            }
            
            // Cache effects prefabs
            foreach (var effect in _effectsPrefabs.EffectPrefabs)
            {
                if (effect.Prefab != null)
                {
                    _prefabCache[effect.PrefabId] = effect.Prefab;
                }
            }
            
            Debug.Log($"Cached {_prefabCache.Count} prefabs in library");
        }
        
        private void SetupObjectPools()
        {
            if (!_enablePooling) return;
            
            if (_poolContainer == null)
            {
                _poolContainer = new GameObject("Prefab Pools").transform;
                _poolContainer.SetParent(transform);
            }
            
            // Create pools for frequently used prefabs
            CreatePoolForCategory("Plants", _plantPrefabs.PlantPrefabs.Select(p => p.PrefabId));
            CreatePoolForCategory("Equipment", _equipmentPrefabs.EquipmentPrefabs.Select(e => e.PrefabId));
            CreatePoolForCategory("Effects", _effectsPrefabs.EffectPrefabs.Select(e => e.PrefabId));
        }
        
        private void CreatePoolForCategory(string category, IEnumerable<string> prefabIds)
        {
            foreach (var prefabId in prefabIds)
            {
                if (_prefabCache.TryGetValue(prefabId, out var prefab))
                {
                    var pool = new ObjectPool(prefab, _defaultPoolSize, _poolContainer);
                    _objectPools[prefabId] = pool;
                }
            }
        }
        
        private void InitializeMetrics()
        {
            _metrics = new PrefabSystemMetrics
            {
                TotalPrefabsLoaded = _prefabCache.Count,
                ActiveInstances = 0,
                PooledInstances = 0,
                MemoryUsage = 0f,
                LastUpdate = DateTime.Now
            };
        }
        
        #endregion
        
        #region Prefab Instantiation
        
        public GameObject InstantiatePrefab(string prefabId, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (_instantiationsThisFrame >= _maxInstancesPerFrame)
            {
                // Queue for next frame
                QueueInstantiation(prefabId, position, rotation, parent);
                return null;
            }
            
            GameObject instance = null;
            
            if (_enablePooling && _objectPools.TryGetValue(prefabId, out var pool))
            {
                instance = pool.Get();
                if (instance != null)
                {
                    instance.transform.position = position;
                    instance.transform.rotation = rotation;
                    instance.transform.SetParent(parent);
                    instance.SetActive(true);
                }
            }
            else if (_prefabCache.TryGetValue(prefabId, out var prefab))
            {
                instance = Instantiate(prefab, position, rotation, parent);
            }
            
            if (instance != null)
            {
                RegisterInstance(instance, prefabId);
                _instantiationsThisFrame++;
                OnInstanceCreated?.Invoke(instance);
            }
            
            return instance;
        }
        
        public void InstantiatePrefabAsync(string prefabId, Vector3 position, Quaternion rotation, 
                                          Transform parent, System.Action<GameObject> callback)
        {
            var request = new PrefabLoadRequest
            {
                PrefabId = prefabId,
                Position = position,
                Rotation = rotation,
                Parent = parent,
                Callback = callback,
                RequestTime = DateTime.Now
            };
            
            _loadQueue.Enqueue(request);
        }
        
        public GameObject InstantiatePlant(string strainId, PlantGrowthStage stage, Vector3 position, 
                                          Quaternion rotation, Transform parent = null)
        {
            var plantPrefab = _plantPrefabs.GetPrefabForStage(strainId, stage);
            if (plantPrefab != null)
            {
                var instance = InstantiatePrefab(plantPrefab.PrefabId, position, rotation, parent);
                
                // Configure plant-specific components
                if (instance != null)
                {
                    ConfigurePlantInstance(instance, strainId, stage);
                }
                
                return instance;
            }
            
            return null;
        }
        
        public GameObject InstantiateEquipment(EquipmentType equipmentType, string equipmentId, 
                                              Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var equipmentPrefab = _equipmentPrefabs.GetEquipmentPrefab(equipmentType, equipmentId);
            if (equipmentPrefab != null)
            {
                var instance = InstantiatePrefab(equipmentPrefab.PrefabId, position, rotation, parent);
                
                // Configure equipment-specific components
                if (instance != null)
                {
                    ConfigureEquipmentInstance(instance, equipmentType, equipmentId);
                }
                
                return instance;
            }
            
            return null;
        }
        
        public GameObject InstantiateFacility(DataFacilityType facilityType, Vector3 position, 
                                             Quaternion rotation, Transform parent = null)
        {
            if (_instantiationsThisFrame >= _maxInstancesPerFrame)
            {
                QueueInstantiation($"facility_{facilityType}", position, rotation, parent);
                return null;
            }
            
            string prefabId = $"facility_{facilityType}";
            var prefab = GetPrefab(prefabId);
            
            if (prefab == null)
            {
                LogWarning($"Facility prefab not found: {facilityType}");
                return null;
            }
            
            var instance = Instantiate(prefab, position, rotation, parent);
            ConfigureFacilityInstance(instance, facilityType);
            RegisterInstance(instance, prefabId);
            
            _instantiationsThisFrame++;
            OnInstanceCreated?.Invoke(instance);
            
            return instance;
        }
        
        private void QueueInstantiation(string prefabId, Vector3 position, Quaternion rotation, Transform parent)
        {
            var request = new PrefabLoadRequest
            {
                PrefabId = prefabId,
                Position = position,
                Rotation = rotation,
                Parent = parent,
                RequestTime = DateTime.Now
            };
            
            _loadQueue.Enqueue(request);
        }
        
        #endregion
        
        #region Instance Configuration
        
        private void ConfigurePlantInstance(GameObject instance, string strainId, PlantGrowthStage stage)
        {
            var plantComponent = instance.GetComponent<InteractivePlantComponent>();
            if (plantComponent == null)
            {
                plantComponent = instance.AddComponent<InteractivePlantComponent>();
            }
            
            // Load strain data
            var strainData = Resources.Load<PlantStrainSO>($"Strains/{strainId}");
            if (strainData != null)
            {
                // Configure plant with strain data
                // plantComponent.Initialize(strainData, stage);
            }
            
            // Setup visual components based on stage
            SetupPlantVisuals(instance, stage);
            
            // Setup interactive components
            SetupPlantInteraction(instance);
        }
        
        private void ConfigureEquipmentInstance(GameObject instance, EquipmentType equipmentType, string equipmentId)
        {
            // Add appropriate equipment component based on type
            switch (equipmentType)
            {
                case EquipmentType.GrowLight:
                    // TODO: Re-enable when AdvancedGrowLightSystem is available
                    // var lightComponent = instance.GetComponent<AdvancedGrowLightSystem>();
                    // if (lightComponent == null)
                    // {
                    //     lightComponent = instance.AddComponent<AdvancedGrowLightSystem>();
                    // }
                    break;
                    
                case EquipmentType.HVAC:
                    // TODO: Re-enable when HVACController is available
                    // var hvacComponent = instance.GetComponent<HVACController>();
                    // if (hvacComponent == null)
                    // {
                    //     hvacComponent = instance.AddComponent<HVACController>();
                    // }
                    break;
                    
                case EquipmentType.Irrigation:
                    // TODO: Re-enable when IrrigationController is available
                    // var irrigationComponent = instance.GetComponent<IrrigationController>();
                    // if (irrigationComponent == null)
                    // {
                    //     irrigationComponent = instance.AddComponent<IrrigationController>();
                    // }
                    break;
            }
            
            // Setup equipment interaction
            SetupEquipmentInteraction(instance);
        }
        
        private void ConfigureFacilityInstance(GameObject instance, DataFacilityType facilityType)
        {
            switch (facilityType)
            {
                case DataFacilityType.GrowRoom:
                    // TODO: Re-enable when AdvancedGrowRoomController is available
                    // var growRoomComponent = instance.GetComponent<AdvancedGrowRoomController>();
                    // if (growRoomComponent == null)
                    // {
                    //     growRoomComponent = instance.AddComponent<AdvancedGrowRoomController>();
                    // }
                    break;
                    
                case DataFacilityType.ProcessingRoom:
                    // Add processing room components
                    break;
                    
                case DataFacilityType.StorageRoom:
                    // Add storage room components
                    break;
            }
            
            // Setup facility interaction
            SetupFacilityInteraction(instance);
        }
        
        private void SetupPlantVisuals(GameObject instance, PlantGrowthStage stage)
        {
            // Setup LOD components
            if (_enableLODSystem)
            {
                var lodGroup = instance.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = instance.AddComponent<LODGroup>();
                    SetupPlantLOD(lodGroup, stage);
                }
            }
            
            // Setup particle effects
            SetupPlantParticleEffects(instance);
            
            // Setup materials
            SetupPlantMaterials(instance, stage);
        }
        
        private void SetupPlantLOD(LODGroup lodGroup, PlantGrowthStage stage)
        {
            // Create LOD levels based on plant stage
            var lods = new LOD[3];
            
            // High detail (close distance)
            lods[0] = new LOD(0.5f, new Renderer[0]); // Will be populated with high-detail renderers
            
            // Medium detail
            lods[1] = new LOD(0.15f, new Renderer[0]); // Medium detail renderers
            
            // Low detail (far distance)
            lods[2] = new LOD(0.05f, new Renderer[0]); // Low detail renderers
            
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
        
        private void SetupPlantParticleEffects(GameObject instance)
        {
            // Add growth particles
            var growthParticles = CreateChildObject(instance, "GrowthParticles");
            var growthPS = growthParticles.AddComponent<ParticleSystem>();
            ConfigureGrowthParticles(growthPS);
            
            // Add health particles
            var healthParticles = CreateChildObject(instance, "HealthParticles");
            var healthPS = healthParticles.AddComponent<ParticleSystem>();
            ConfigureHealthParticles(healthPS);
        }
        
        private void SetupPlantMaterials(GameObject instance, PlantGrowthStage stage)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // Apply stage-appropriate materials
                var material = GetStageMaterial(stage);
                if (material != null)
                {
                    renderer.material = material;
                }
            }
        }
        
        private void SetupPlantInteraction(GameObject instance)
        {
            // Add collider for interaction
            var collider = instance.GetComponent<Collider>();
            if (collider == null)
            {
                collider = instance.AddComponent<CapsuleCollider>();
                ((CapsuleCollider)collider).radius = 0.5f;
                ((CapsuleCollider)collider).height = 2f;
            }
            
            // Setup interaction highlight
            SetupInteractionHighlight(instance);
        }
        
        private void SetupEquipmentInteraction(GameObject instance)
        {
            // Add interaction components
            SetupInteractionHighlight(instance);
            
            // Add audio source for equipment sounds
            var audioSource = instance.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = instance.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.volume = 0.5f;
                audioSource.playOnAwake = false;
            }
        }
        
        private void SetupFacilityInteraction(GameObject instance)
        {
            // Add large-scale interaction zones
            SetupInteractionHighlight(instance);
            
            // Add environmental audio
            var audioSource = instance.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = instance.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.volume = 0.3f;
                audioSource.loop = true;
            }
        }
        
        private void SetupInteractionHighlight(GameObject instance)
        {
            // TODO: Re-enable when Outline component is available
            // Add outline effect for interaction feedback
            // var outline = instance.GetComponent<Outline>();
            // if (outline == null)
            // {
            //     outline = instance.AddComponent<Outline>();
            //     outline.OutlineColor = Color.yellow;
            //     outline.OutlineWidth = 2f;
            //     outline.enabled = false;
            // }
        }
        
        #endregion
        
        #region Object Pooling
        
        public void ReturnToPool(GameObject instance)
        {
            if (!_enablePooling) return;
            
            var prefabInstance = _activeInstances.FirstOrDefault(i => i.Instance == instance);
            if (prefabInstance != null && _objectPools.TryGetValue(prefabInstance.PrefabId, out var pool))
            {
                pool.Return(instance);
                UnregisterInstance(instance);
                OnInstanceDestroyed?.Invoke(instance);
            }
            else
            {
                DestroyInstance(instance);
            }
        }
        
        public void DestroyInstance(GameObject instance)
        {
            UnregisterInstance(instance);
            Destroy(instance);
            OnInstanceDestroyed?.Invoke(instance);
        }
        
        private void RegisterInstance(GameObject instance, string prefabId)
        {
            var prefabInstance = new PrefabInstance
            {
                Instance = instance,
                PrefabId = prefabId,
                CreationTime = DateTime.Now,
                LastAccessTime = Time.time
            };
            
            _activeInstances.Add(prefabInstance);
        }
        
        private void UnregisterInstance(GameObject instance)
        {
            _activeInstances.RemoveAll(i => i.Instance == instance);
        }
        
        #endregion
        
        #region Async Loading
        
        private void ProcessLoadQueue()
        {
            if (_isProcessingQueue || _loadQueue.Count == 0) return;
            
            StartCoroutine(ProcessLoadQueueCoroutine());
        }
        
        private IEnumerator ProcessLoadQueueCoroutine()
        {
            _isProcessingQueue = true;
            
            while (_loadQueue.Count > 0 && _instantiationsThisFrame < _maxInstancesPerFrame)
            {
                var request = _loadQueue.Dequeue();
                
                var instance = InstantiatePrefab(request.PrefabId, request.Position, 
                                               request.Rotation, request.Parent);
                
                request.Callback?.Invoke(instance);
                
                yield return null; // Spread across frames
            }
            
            _isProcessingQueue = false;
        }
        
        #endregion
        
        #region Cleanup and Optimization
        
        private void StartCleanupRoutine()
        {
            InvokeRepeating(nameof(PerformCleanup), _cleanupInterval, _cleanupInterval);
        }
        
        private void PerformCleanup()
        {
            // Clean up old instances
            CleanupOldInstances();
            
            // Optimize pools
            OptimizePools();
            
            // Clear unused prefabs from cache
            ClearUnusedPrefabs();
            
            // Update metrics
            UpdateMetrics();
        }
        
        private void CleanupOldInstances()
        {
            var cutoffTime = Time.time - 300f; // 5 minutes
            var toRemove = new List<PrefabInstance>();
            
            foreach (var instance in _activeInstances)
            {
                if (instance.Instance == null || instance.LastAccessTime < cutoffTime)
                {
                    toRemove.Add(instance);
                }
            }
            
            foreach (var instance in toRemove)
            {
                if (instance.Instance != null)
                {
                    ReturnToPool(instance.Instance);
                }
                else
                {
                    _activeInstances.Remove(instance);
                }
            }
        }
        
        private void OptimizePools()
        {
            foreach (var pool in _objectPools.Values)
            {
                pool.Optimize();
            }
        }
        
        private void ClearUnusedPrefabs()
        {
            // Keep cache for now - could implement LRU eviction if memory becomes an issue
        }
        
        #endregion
        
        #region Utility Methods
        
        private GameObject CreateChildObject(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            return child;
        }
        
        private void ConfigureGrowthParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = Color.green;
            main.maxParticles = 50;
            
            var emission = ps.emission;
            emission.rateOverTime = 10f;
            
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
        }
        
        private void ConfigureHealthParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 0.5f;
            main.startSize = 0.05f;
            main.startColor = Color.red;
            main.maxParticles = 20;
            
            var emission = ps.emission;
            emission.rateOverTime = 0f; // Only emit when health is low
        }
        
        private Material GetStageMaterial(PlantGrowthStage stage)
        {
            // Return stage-appropriate material
            return Resources.Load<Material>($"Materials/Plant_{stage}");
        }
        
        #endregion
        
        #region Metrics and Monitoring
        
        private void UpdateMetrics()
        {
            _metrics.ActiveInstances = _activeInstances.Count;
            _metrics.PooledInstances = _objectPools.Values.Sum(p => p.PooledCount);
            _metrics.TotalPrefabsLoaded = _prefabCache.Count;
            _metrics.MemoryUsage = CalculateMemoryUsage();
            _metrics.LastUpdate = DateTime.Now;
            
            OnMetricsUpdated?.Invoke(_metrics);
        }
        
        private float CalculateMemoryUsage()
        {
            // Simplified memory calculation
            float memory = 0f;
            
            memory += _activeInstances.Count * 0.1f; // ~100KB per active instance
            memory += _objectPools.Values.Sum(p => p.PooledCount * 0.05f); // ~50KB per pooled instance
            memory += _prefabCache.Count * 0.02f; // ~20KB per cached prefab
            
            return memory; // Returns MB
        }
        
        #endregion
        
        #region Public Interface
        
        public GameObject GetPrefab(string prefabId)
        {
            return _prefabCache.TryGetValue(prefabId, out var prefab) ? prefab : null;
        }
        
        public bool HasPrefab(string prefabId)
        {
            return _prefabCache.ContainsKey(prefabId);
        }
        
        public List<string> GetAvailablePrefabs(PrefabCategory category)
        {
            return category switch
            {
                PrefabCategory.Plants => _plantPrefabs.PlantPrefabs.Select(p => p.PrefabId).ToList(),
                PrefabCategory.Equipment => _equipmentPrefabs.EquipmentPrefabs.Select(e => e.PrefabId).ToList(),
                PrefabCategory.Facilities => _facilityPrefabs.FacilityPrefabs.Select(f => f.PrefabId).ToList(),
                PrefabCategory.Environmental => _environmentalPrefabs.EnvironmentalPrefabs.Select(e => e.PrefabId).ToList(),
                PrefabCategory.UI => _uiPrefabs.UIPrefabs.Select(u => u.PrefabId).ToList(),
                PrefabCategory.Effects => _effectsPrefabs.EffectPrefabs.Select(e => e.PrefabId).ToList(),
                _ => new List<string>()
            };
        }
        
        public void PreloadPrefabs(IEnumerable<string> prefabIds)
        {
            foreach (var prefabId in prefabIds)
            {
                if (_prefabCache.ContainsKey(prefabId) && _enablePooling)
                {
                    // Pre-populate pool
                    if (_objectPools.TryGetValue(prefabId, out var pool))
                    {
                        pool.Preload(_defaultPoolSize);
                    }
                }
            }
        }
        
        public void SetPoolSize(string prefabId, int size)
        {
            if (_objectPools.TryGetValue(prefabId, out var pool))
            {
                pool.SetSize(size);
            }
        }
        
        public PrefabSystemStatus GetSystemStatus()
        {
            return new PrefabSystemStatus
            {
                ActiveInstances = _activeInstances.Count,
                LoadedPrefabs = _prefabCache.Count,
                PooledInstances = _objectPools.Values.Sum(p => p.PooledCount),
                QueuedRequests = _loadQueue.Count,
                MemoryUsage = _metrics.MemoryUsage,
                IsProcessingQueue = _isProcessingQueue
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Clean up all instances
            foreach (var instance in _activeInstances.ToList())
            {
                if (instance.Instance != null)
                {
                    DestroyInstance(instance.Instance);
                }
            }
            
            // Clear pools
            foreach (var pool in _objectPools.Values)
            {
                pool.Clear();
            }
            
            CancelInvoke();
        }
    }
}