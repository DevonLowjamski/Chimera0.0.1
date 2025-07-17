using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Effects;
using ProjectChimera.Systems.Progression;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

// Explicit alias for Data layer PlantGrowthStage to resolve ambiguity
using DataPlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
// Explicit alias for Data layer EnvironmentalConditions to resolve delegate signature mismatch
using DataEnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
// Explicit alias for EnvironmentalManager to resolve ambiguity
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Advanced SpeedTree manager providing comprehensive cannabis plant simulation.
    /// Integrates SpeedTree with genetics, environmental systems, growth animation,
    /// and real-time plant behavior for photorealistic cannabis cultivation.
    /// </summary>
    public class AdvancedSpeedTreeManager : ChimeraManager
    {
        [Header("SpeedTree Configuration")]
        [SerializeField] private SpeedTreeLibrarySO _speedTreeLibrary;
        [SerializeField] private SpeedTreeShaderConfigSO _shaderConfig;
        [SerializeField] private SpeedTreeWindConfigSO _windConfig;
        [SerializeField] private SpeedTreeLODConfigSO _lodConfig;
        
        [Header("Cannabis Plant Specifications")]
        [SerializeField] private List<CannabisStrainAsset> _cannabisStrains = new List<CannabisStrainAsset>();
        [SerializeField] private SpeedTreeGeneticsConfigSO _geneticsConfig;
        [SerializeField] private SpeedTreeGrowthConfigSO _growthConfig;
        
        [Header("Environmental Integration")]
        [SerializeField] private bool _enableEnvironmentalResponse = true;
        [SerializeField] private bool _enableSeasonalChanges = true;
        [SerializeField] private bool _enableStressVisualization = true;
        [SerializeField] private float _environmentalUpdateFrequency = 0.5f;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxVisiblePlants = 500;
        [SerializeField] private float _cullingDistance = 100f;
        [SerializeField] private bool _enableGPUInstancing = true;
        [SerializeField] private bool _enableDynamicBatching = true;
        [SerializeField] private SpeedTreeQualityLevel _defaultQuality = SpeedTreeQualityLevel.High;
        
        [Header("Animation & Physics")]
        [SerializeField] private bool _enableWindAnimation = true;
        [SerializeField] private bool _enableGrowthAnimation = true;
        [SerializeField] private bool _enablePhysicsInteraction = true;
        [SerializeField] private float _animationTimeScale = 1f;
        
        // Core SpeedTree Systems
        private SpeedTreeRenderer[] _activeRenderers;
        private Dictionary<string, SpeedTreeAsset> _loadedAssets = new Dictionary<string, SpeedTreeAsset>();
        private Dictionary<int, SpeedTreePlantData> _plantInstances = new Dictionary<int, SpeedTreePlantData>();
        
        // Cannabis-Specific Systems
        private CannabisGeneticsProcessor _geneticsProcessor;
        private CannabisGrowthAnimator _growthAnimator;
        private CannabisEnvironmentalProcessor _environmentalProcessor;
        private CannabisStressVisualizer _stressVisualizer;
        
        // Optimization Systems
        private SpeedTreeLODManager _lodManager;
        private SpeedTreeBatchingManager _batchingManager;
        private SpeedTreeCullingManager _cullingManager;
        private SpeedTreeMemoryManager _memoryManager;
        
        // Integration with Game Systems
        private PlantManager _plantManager;
        private EnvironmentalManager _environmentalManager;
        // private AdvancedEffectsManager _effectsManager; // Disabled - system not available
        // private ComprehensiveProgressionManager _progressionManager; // Disabled - using CleanProgressionManager instead
        
        // Wind and Animation
        private SpeedTreeWindController _windController;
        private Dictionary<WindZone, SpeedTreeWindSettings> _windZones = new Dictionary<WindZone, SpeedTreeWindSettings>();
        
        // Performance Monitoring
        private SpeedTreePerformanceMetrics _performanceMetrics;
        private float _lastPerformanceUpdate = 0f;
        
        // Events
        public System.Action<SpeedTreePlantData> OnPlantInstanceCreated;
        public System.Action<SpeedTreePlantData> OnPlantInstanceDestroyed;
        public System.Action<SpeedTreePlantData, PlantGrowthStage> OnPlantStageChanged;
        public System.Action<SpeedTreePerformanceMetrics> OnPerformanceMetricsUpdated;
        
        // Properties
        public SpeedTreePerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public int ActivePlantCount => _plantInstances.Count;
        public bool SpeedTreeEnabled => IsSpeedTreeAvailable();
        
        // Missing data structures for SpeedTree system
        [System.Serializable]
        public class SpeedTreeRenderer
        {
            public GameObject gameObject;
            public GameObject GameObject;
            public Renderer Renderer;
            public LODGroup LODGroup;
            public WindZone WindZone;
            public SpeedTreeAsset speedTreeAsset;
            public MaterialPropertyBlock materialProperties;
            public Material[] materials; // Materials array for compatibility
            public Transform transform => gameObject?.transform;
            public Dictionary<string, object> Properties = new Dictionary<string, object>();
            public bool IsActive;
            public float LastUpdateTime;
        }

        // CannabisGeneticData class removed - using the one from SpeedTreeDataStructures.cs to resolve namespace conflict

        [System.Serializable]
        public class SpeedTreeAsset
        {
            public string AssetId;
            public string AssetPath;
            public GameObject Prefab;
            public Material[] Materials;
            public Texture2D[] Textures;
            public Dictionary<string, object> Properties = new Dictionary<string, object>();
        }
        
        [System.Serializable]
        public class SpeedTreePlantData
        {
            public int InstanceId;
            public InteractivePlantComponent PlantComponent;
            public CannabisStrainAsset StrainAsset;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            public PlantGrowthStage GrowthStage;
            public float Health;
            public float Age;
            public float CreationTime;
            public float LastUpdateTime;
            public float DistanceToCamera;
            public CannabisGeneticData GeneticData;
            public EnvironmentalStressData StressData;
            public SpeedTreeRenderer Renderer;
            public bool IsActive;
            public Dictionary<string, float> EnvironmentalModifiers = new Dictionary<string, float>();
            public ProjectChimera.Data.Environment.EnvironmentalConditions EnvironmentalConditions;
            
            // Additional properties for compatibility
            public float GrowthRate = 1f;
            public float StressResistance = 1f;
            public float MutationRate = 0.1f; // Added missing MutationRate property
            
            public string SpeedTreeAssetPath => StrainAsset?.SpeedTreeAssetPath ?? "";
            public Color CurrentLeafColor = Color.green;
            public float GrowthAnimationTime = 1f;
            
            // Additional properties for Error Wave 143 compatibility
            public bool HasValue => IsActive;
            public SpeedTreePlantData Value => this;
        }
        
        // Helper components for SpeedTree cannabis simulation
        public class SpeedTreeCannabisComponent : MonoBehaviour
        {
            private SpeedTreePlantData _instance;
            private AdvancedSpeedTreeManager _manager;
            
            public void Initialize(SpeedTreePlantData instance, AdvancedSpeedTreeManager manager)
            {
                _instance = instance;
                _manager = manager;
            }
        }
        
        public class SpeedTreePlantInteraction : MonoBehaviour
        {
            private SpeedTreePlantData _instance;
            
            public void Initialize(SpeedTreePlantData instance)
            {
                _instance = instance;
            }
        }
        
        protected override void OnManagerInitialize()
        {
            if (!IsSpeedTreeAvailable())
            {
                LogError("SpeedTree not available - Advanced SpeedTree Manager disabled");
                return;
            }
            
            InitializeSpeedTreeSystems();
            InitializeCannabisSimulation();
            ConnectToGameSystems();
            InitializePerformanceMonitoring();
            StartSpeedTreeUpdateLoop();
            
            LogInfo("Advanced SpeedTree Manager initialized with cannabis simulation");
        }
        
        private void Update()
        {
            if (!SpeedTreeEnabled) return;
            
            UpdateSpeedTreeSystems();
            UpdateCannabisSimulation();
            UpdatePerformanceOptimization();
            UpdatePerformanceMetrics();
        }
        
        private void LateUpdate()
        {
            if (!SpeedTreeEnabled) return;
            
            UpdateWindSystem();
            UpdateLODSystem();
            ProcessBatching();
        }
        
        #region Initialization
        
        private bool IsSpeedTreeAvailable()
        {
#if UNITY_SPEEDTREE
            return true;
#else
            return false;
#endif
        }
        
        private void InitializeSpeedTreeSystems()
        {
#if UNITY_SPEEDTREE
            // Initialize core SpeedTree systems
            _lodManager = new SpeedTreeLODManager(_lodConfig);
            _batchingManager = new SpeedTreeBatchingManager(_enableDynamicBatching, _enableGPUInstancing);
            _cullingManager = new SpeedTreeCullingManager(_cullingDistance);
            _memoryManager = new SpeedTreeMemoryManager();
            
            // Initialize wind controller
            _windController = new SpeedTreeWindController(_windConfig);
            
            // Load SpeedTree assets
            LoadSpeedTreeAssets();
            
            LogInfo("SpeedTree core systems initialized");
#endif
        }
        
        private void InitializeCannabisSimulation()
        {
            // Initialize cannabis-specific processors
            _geneticsProcessor = new CannabisGeneticsProcessor(_geneticsConfig);
            _growthAnimator = new CannabisGrowthAnimator(_growthConfig);
            _environmentalProcessor = new CannabisEnvironmentalProcessor();
            _stressVisualizer = new CannabisStressVisualizer(_enableStressVisualization);
            
            // Initialize strain database
            InitializeStrainDatabase();
            
            LogInfo("Cannabis simulation systems initialized");
        }
        
        private void LoadSpeedTreeAssets()
        {
            if (_speedTreeLibrary == null) return;
            
            var assetPaths = _speedTreeLibrary.GetAllAssetPaths();
            foreach (var assetPath in assetPaths)
            {
                var asset = LoadSpeedTreeAsset(assetPath);
                if (asset != null)
                {
                    _loadedAssets[assetPath] = asset;
                }
            }
            
            LogInfo($"Loaded {_loadedAssets.Count} SpeedTree assets");
        }
        
        private SpeedTreeAsset LoadSpeedTreeAsset(string assetPath)
        {
#if UNITY_SPEEDTREE
            var asset = Resources.Load<SpeedTreeAsset>(assetPath);
            if (asset != null)
            {
                // Configure asset for cannabis simulation
                ConfigureAssetForCannabis(asset);
                return asset;
            }
#endif
            return null;
        }
        
        private void ConfigureAssetForCannabis(SpeedTreeAsset asset)
        {
#if UNITY_SPEEDTREE
            // Configure SpeedTree asset specifically for cannabis plants
            asset.materialLocation = SpeedTreeMaterialLocation.External;
            asset.scaleFactor = 1f;
            asset.enableSmoothLOD = true;
            asset.animateCrossFading = true;
            asset.enableHue = true;
            asset.hueVariation = 0.1f;
            
            // Cannabis-specific settings
            asset.billboardShadowFade = 0.5f;
            asset.enableBillboardFacingLeavesFacing = true;
#endif
        }
        
        private void InitializeStrainDatabase()
        {
            foreach (var strain in _cannabisStrains)
            {
                _geneticsProcessor.RegisterStrain(strain);
            }
            
            LogInfo($"Registered {_cannabisStrains.Count} cannabis strains");
        }
        
        private void ConnectToGameSystems()
        {
            if (GameManager.Instance != null)
            {
                _plantManager = GameManager.Instance.GetManager<PlantManager>();
                _environmentalManager = GameManager.Instance.GetManager<EnvironmentalManager>();
                // _effectsManager = GameManager.Instance.GetManager<AdvancedEffectsManager>(); // Disabled
                // _progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>(); // Disabled
            }
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            if (_plantManager != null)
            {
                _plantManager.OnPlantAdded += HandlePlantAdded;
                _plantManager.OnPlantHarvested += HandlePlantRemoved;
                _plantManager.OnPlantStageChanged += HandlePlantStageChanged;
                _plantManager.OnPlantHealthUpdated += HandlePlantHealthChanged;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged += HandleEnvironmentalChange;
                _environmentalManager.OnWindChanged += HandleWindChange;
                _environmentalManager.OnLightingChanged += HandleLightingChange;
            }
        }
        
        private void InitializePerformanceMonitoring()
        {
            _performanceMetrics = new SpeedTreePerformanceMetrics
            {
                MaxVisiblePlants = _maxVisiblePlants,
                CullingDistance = _cullingDistance,
                QualityLevel = _defaultQuality,
                GPUInstancingEnabled = _enableGPUInstancing,
                DynamicBatchingEnabled = _enableDynamicBatching
            };
            
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        private void StartSpeedTreeUpdateLoop()
        {
            StartCoroutine(SpeedTreeUpdateCoroutine());
        }
        
        private IEnumerator SpeedTreeUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_environmentalUpdateFrequency);
                
                if (_enableEnvironmentalResponse)
                {
                    UpdateEnvironmentalResponse();
                }
                
                if (_enableSeasonalChanges)
                {
                    UpdateSeasonalChanges();
                }
                
                UpdateGrowthAnimations();
                UpdateStressVisualization();
            }
        }
        
        #endregion
        
        #region Plant Instance Management
        
        public SpeedTreePlantData CreatePlantInstance(InteractivePlantComponent plantComponent, string strainId = "")
        {
            if (!SpeedTreeEnabled) return null;
            
            var strain = GetCannabisStrain(strainId) ?? GetDefaultCannabisStrain();
            if (strain == null)
            {
                LogWarning($"No cannabis strain found for: {strainId}");
                return null;
            }
            
            var instance = new SpeedTreePlantData
            {
                InstanceId = plantComponent.GetInstanceID(),
                PlantComponent = plantComponent,
                StrainAsset = strain,
                Position = plantComponent.transform.position,
                Rotation = plantComponent.transform.rotation,
                Scale = Vector3.one,
                GrowthStage = ConvertToPlantGrowthStage(plantComponent.CurrentGrowthStage),
                Health = plantComponent.Health,
                Age = 0f,
                CreationTime = Time.time
            };
            
            // Generate genetic variation
            var genetics = _geneticsProcessor.GenerateGeneticVariation(strain, plantComponent.Genotype);
            instance.GeneticData = genetics;
            
            // Create SpeedTree renderer
            var renderer = CreateSpeedTreeRenderer(instance);
            if (renderer != null)
            {
                instance.Renderer = renderer;
                instance.IsActive = true;
                
                _plantInstances[instance.InstanceId] = instance;
                
                // Initialize growth animation
                _growthAnimator.InitializePlant(instance);
                
                // Apply initial environmental conditions
                _environmentalProcessor.ApplyEnvironmentalConditions(instance, GetCurrentEnvironmentalConditions());
                
                OnPlantInstanceCreated?.Invoke(instance);
                
                LogInfo($"Created SpeedTree plant instance: {strain.StrainName}");
                return instance;
            }
            
            LogError("Failed to create SpeedTree renderer");
            return null;
        }
        
        private SpeedTreeRenderer CreateSpeedTreeRenderer(SpeedTreePlantData instance)
        {
#if UNITY_SPEEDTREE
            var speedTreeAsset = GetSpeedTreeAssetForStrain(instance.StrainAsset);
            if (speedTreeAsset == null) return null;
            
            var rendererGO = new GameObject($"SpeedTree_{instance.StrainAsset.StrainName}_{instance.InstanceId}");
            rendererGO.transform.position = instance.Position;
            rendererGO.transform.rotation = instance.Rotation;
            rendererGO.transform.localScale = instance.Scale;
            
            var renderer = rendererGO.AddComponent<SpeedTreeRenderer>();
            renderer.speedTreeAsset = speedTreeAsset;
            
            // Apply genetic variations to renderer
            ApplyGeneticVariationsToRenderer(renderer, instance.GeneticData);
            
            // Configure renderer for cannabis simulation
            ConfigureRendererForCannabis(renderer, instance);
            
            return renderer;
#else
            return null;
#endif
        }
        
        private void ApplyGeneticVariationsToRenderer(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
#if UNITY_SPEEDTREE
            if (renderer == null || genetics == null) return;
            
            // Apply size variations
            renderer.transform.localScale = Vector3.one * genetics.PlantSize;
            
            // Apply color variations
            if (renderer.materialProperties != null)
            {
                renderer.materialProperties.hueVariation = genetics.ColorVariation;
                renderer.materialProperties.saturation = genetics.Saturation;
                renderer.materialProperties.brightness = genetics.Brightness;
                
                // Cannabis-specific color properties
                renderer.materialProperties.SetColor("_LeafColor", genetics.LeafColor);
                renderer.materialProperties.SetColor("_BudColor", genetics.BudColor);
                renderer.materialProperties.SetFloat("_TrichromeAmount", genetics.TrichromeAmount);
            }
            
            // Apply morphological variations
            ApplyMorphologicalVariations(renderer, genetics);
#endif
        }
        
        private void ApplyMorphologicalVariations(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
#if UNITY_SPEEDTREE
            // Adjust branch characteristics
            if (renderer.speedTreeAsset != null)
            {
                // Branch density
                var branchDensity = genetics.BranchDensity;
                renderer.materialProperties?.SetFloat("_BranchDensity", branchDensity);
                
                // Leaf characteristics
                var leafSize = genetics.LeafSize;
                var leafDensity = genetics.LeafDensity;
                renderer.materialProperties?.SetFloat("_LeafScale", leafSize);
                renderer.materialProperties?.SetFloat("_LeafDensity", leafDensity);
                
                // Cannabis-specific morphology
                renderer.materialProperties?.SetFloat("_BudDensity", genetics.BudDensity);
                renderer.materialProperties?.SetFloat("_PistilLength", genetics.PistilLength);
                renderer.materialProperties?.SetFloat("_StemThickness", genetics.StemThickness);
            }
#endif
        }
        
        private void ConfigureRendererForCannabis(SpeedTreeRenderer renderer, SpeedTreePlantData instance)
        {
#if UNITY_SPEEDTREE
            // Configure LOD settings
            _lodManager.ConfigureRenderer(renderer, instance);
            
            // Configure wind settings
            _windController.ConfigureRenderer(renderer, instance);
            
            // Configure batching
            _batchingManager.RegisterRenderer(renderer);
            
            // Add cannabis-specific components
            var cannabisComponent = renderer.gameObject.AddComponent<SpeedTreeCannabisComponent>();
            cannabisComponent.Initialize(instance, this);
            
            // Add physics interaction if enabled
            if (_enablePhysicsInteraction)
            {
                AddPhysicsInteraction(renderer, instance);
            }
#endif
        }
        
        private void AddPhysicsInteraction(SpeedTreeRenderer renderer, SpeedTreePlantData instance)
        {
            // Add colliders for plant interaction
            var collider = renderer.gameObject.AddComponent<CapsuleCollider>();
            collider.height = instance.GeneticData.PlantSize * 2f;
            collider.radius = instance.GeneticData.PlantSize * 0.3f;
            collider.isTrigger = true;
            
            // Add interaction component
            var interaction = renderer.gameObject.AddComponent<SpeedTreePlantInteraction>();
            interaction.Initialize(instance);
        }
        
        public void DestroyPlantInstance(int instanceId)
        {
            if (_plantInstances.TryGetValue(instanceId, out var instance))
            {
                // Cleanup renderer
                if (instance.Renderer != null)
                {
                    _batchingManager.UnregisterRenderer(instance.Renderer);
                    DestroyImmediate(instance.Renderer.gameObject);
                }
                
                // Cleanup growth animation
                _growthAnimator.RemovePlant(instance);
                
                _plantInstances.Remove(instanceId);
                
                OnPlantInstanceDestroyed?.Invoke(instance);
                
                LogInfo($"Destroyed SpeedTree plant instance: {instanceId}");
            }
        }
        
        #endregion
        
        #region Growth Animation System
        
        private void UpdateGrowthAnimations()
        {
            if (!_enableGrowthAnimation) return;
            
            foreach (var instance in _plantInstances.Values)
            {
                _growthAnimator.UpdatePlantGrowth(instance, Time.deltaTime * _animationTimeScale);
            }
        }
        
        public void TriggerGrowthStageTransition(SpeedTreePlantData instance, PlantGrowthStage newStage)
        {
            if (instance == null) return;
            
            var oldStage = instance.GrowthStage;
            instance.GrowthStage = newStage;
            
            // Animate stage transition
            _growthAnimator.AnimateStageTransition(instance, oldStage, newStage);
            
            // Update visual appearance
            UpdatePlantAppearanceForStage(instance, newStage);
            
            // Trigger effects
            // if (_effectsManager != null)
            // {
            //     _effectsManager.PlayEffect(EffectType.PlantGrowth, instance.Position, instance.Renderer?.transform, 3f);
            // }
            
            OnPlantStageChanged?.Invoke(instance, newStage);
            
            LogInfo($"Plant {instance.InstanceId} transitioned to {newStage}");
        }
        
        private void UpdatePlantAppearanceForStage(SpeedTreePlantData instance, PlantGrowthStage stage)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            var stageConfig = _growthConfig.GetStageConfiguration(stage);
            if (stageConfig == null) return;
            
            // Update size
            float targetSize = stageConfig.SizeMultiplier * instance.GeneticData.PlantSize;
            instance.Renderer.transform.localScale = Vector3.one * targetSize;
            
            // Update materials based on stage
            switch (stage)
            {
                case PlantGrowthStage.Seedling:
                    instance.Renderer.materialProperties.SetColor("_LeafColor", Color.green * 0.8f);
                    instance.Renderer.materialProperties.SetFloat("_BudDensity", 0f);
                    break;
                
                case PlantGrowthStage.Vegetative:
                    instance.Renderer.materialProperties.SetColor("_LeafColor", Color.green);
                    instance.Renderer.materialProperties.SetFloat("_LeafDensity", 1f);
                    instance.Renderer.materialProperties.SetFloat("_BudDensity", 0.1f);
                    break;
                
                case PlantGrowthStage.Flowering:
                    instance.Renderer.materialProperties.SetColor("_LeafColor", Color.green * 0.9f);
                    instance.Renderer.materialProperties.SetFloat("_BudDensity", 0.8f);
                    instance.Renderer.materialProperties.SetColor("_BudColor", instance.GeneticData.BudColor);
                    instance.Renderer.materialProperties.SetFloat("_TrichromeAmount", instance.GeneticData.TrichromeAmount);
                    break;
                
                case PlantGrowthStage.Harvest:
                    instance.Renderer.materialProperties.SetFloat("_BudDensity", 1f);
                    instance.Renderer.materialProperties.SetFloat("_TrichromeAmount", instance.GeneticData.TrichromeAmount * 1.2f);
                    break;
            }
#endif
        }
        
        #endregion
        
        #region Environmental Response System
        
        private void UpdateEnvironmentalResponse()
        {
            var currentConditions = GetCurrentEnvironmentalConditions();
            
            foreach (var instance in _plantInstances.Values)
            {
                _environmentalProcessor.UpdateEnvironmentalResponse(instance, currentConditions);
            }
        }
        
        private void UpdateSeasonalChanges()
        {
            // Implement seasonal color and behavior changes
            var seasonalFactor = CalculateSeasonalFactor();
            
            foreach (var instance in _plantInstances.Values)
            {
                ApplySeasonalChanges(instance, seasonalFactor);
            }
        }
        
        private float CalculateSeasonalFactor()
        {
            // Calculate seasonal factor based on game time or real time
            var dayOfYear = DateTime.Now.DayOfYear;
            return Mathf.Sin((dayOfYear / 365f) * 2f * Mathf.PI);
        }
        
        private void ApplySeasonalChanges(SpeedTreePlantData instance, float seasonalFactor)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            // Adjust colors based on season
            var baseLeafColor = instance.GeneticData.LeafColor;
            var seasonalColor = Color.Lerp(baseLeafColor, Color.yellow * 0.8f, Mathf.Max(0f, -seasonalFactor));
            
            instance.Renderer.materialProperties.SetColor("_LeafColor", seasonalColor);
            
            // Adjust growth rate based on season
            var growthModifier = 1f + (seasonalFactor * 0.3f);
            instance.EnvironmentalModifiers["seasonal_growth"] = growthModifier;
#endif
        }
        
        private void UpdateStressVisualization()
        {
            if (!_enableStressVisualization) return;
            
            foreach (var instance in _plantInstances.Values)
            {
                _stressVisualizer.UpdateStressVisualization(instance);
            }
        }
        
        private DataEnvironmentalConditions GetCurrentEnvironmentalConditions()
        {
            if (_environmentalManager != null)
            {
                return _environmentalManager.GetCurrentConditions(null);
            }
            
            return new DataEnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                CO2Level = 400f,
                LightIntensity = 800f,
                AirVelocity = 0.5f
            };
        }
        
        #endregion
        
        #region Wind System
        
        private void UpdateWindSystem()
        {
            if (!_enableWindAnimation) return;
            
            _windController.UpdateWind();
            
            // Update wind zones
            foreach (var windZone in FindObjectsByType<WindZone>(FindObjectsSortMode.None))
            {
                UpdateWindZone(windZone);
            }
        }
        
        private void UpdateWindZone(WindZone windZone)
        {
            if (!_windZones.ContainsKey(windZone))
            {
                _windZones[windZone] = new SpeedTreeWindSettings
                {
                    WindMain = windZone.windMain,
                    WindTurbulence = windZone.windTurbulence,
                    WindPulseMagnitude = windZone.windPulseMagnitude,
                    WindPulseFrequency = windZone.windPulseFrequency
                };
            }
            
            var settings = _windZones[windZone];
            _windController.ApplyWindSettings(settings);
        }
        
        #endregion
        
        #region Performance Optimization
        
        private void UpdateSpeedTreeSystems()
        {
            _lodManager?.Update();
            _batchingManager?.Update();
            var plantInstances = _plantInstances.Values.ToList();
            _cullingManager?.UpdateLODs(plantInstances);
            _memoryManager?.Update();
        }
        
        private void UpdateCannabisSimulation()
        {
            if (_geneticsProcessor != null)
            {
                _geneticsProcessor.Update();
            }
            
            if (_growthAnimator != null)
            {
                _growthAnimator.Update();
            }
            
            if (_environmentalProcessor != null)
            {
                _environmentalProcessor.Update();
            }
            
            if (_stressVisualizer != null)
            {
                _stressVisualizer.Update();
            }
        }
        
        private void UpdatePerformanceOptimization()
        {
            // Dynamic quality adjustment based on performance
            if (_performanceMetrics.FrameRate < 30f && _performanceMetrics.ActivePlants > 100)
            {
                ReduceQuality();
            }
            else if (_performanceMetrics.FrameRate > 50f && _performanceMetrics.QualityLevel < SpeedTreeQualityLevel.Ultra)
            {
                IncreaseQuality();
            }
            
            // Cull distant plants
            var plantInstances = _plantInstances.Values.ToList();
            _cullingManager.CullDistantPlants(plantInstances);
        }
        
        private void UpdateLODSystem()
        {
            var plantInstances = _plantInstances.Values.ToList();
            _lodManager.UpdateLODs(plantInstances);
            _cullingManager.UpdateLODs(plantInstances);
        }
        
        private void ProcessBatching()
        {
            if (_enableDynamicBatching)
            {
                _batchingManager.ProcessBatching();
            }
        }
        
        private void ReduceQuality()
        {
            if (_performanceMetrics.QualityLevel > SpeedTreeQualityLevel.Low)
            {
                _performanceMetrics.QualityLevel = (SpeedTreeQualityLevel)((int)_performanceMetrics.QualityLevel - 1);
                ApplyQualitySettings(_performanceMetrics.QualityLevel);
                LogInfo($"Reduced SpeedTree quality to: {_performanceMetrics.QualityLevel}");
            }
        }
        
        private void IncreaseQuality()
        {
            if (_performanceMetrics.QualityLevel < SpeedTreeQualityLevel.Ultra)
            {
                _performanceMetrics.QualityLevel = (SpeedTreeQualityLevel)((int)_performanceMetrics.QualityLevel + 1);
                ApplyQualitySettings(_performanceMetrics.QualityLevel);
                LogInfo($"Increased SpeedTree quality to: {_performanceMetrics.QualityLevel}");
            }
        }
        
        private void ApplyQualitySettings(SpeedTreeQualityLevel quality)
        {
            foreach (var instance in _plantInstances.Values)
            {
                if (instance.Renderer != null)
                {
                    _lodManager.ApplyQualitySettings(instance.Renderer, quality);
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantAdded(PlantInstance plant)
        {
            CreatePlantInstance(plant.GetComponent<InteractivePlantComponent>());
        }
        
        private void HandlePlantRemoved(PlantInstance plant)
        {
            var instanceId = plant.GetInstanceID();
            DestroyPlantInstance(instanceId);
        }
        
        private void HandlePlantStageChanged(PlantInstance plant)
        {
            var instance = GetPlantInstance(plant.GetInstanceID());
            if (instance != null)
            {
                var newStage = plant.CurrentGrowthStage;
                TriggerGrowthStageTransition(instance, newStage);
                LogInfo($"Plant {plant.PlantID} transitioned to {newStage}");
            }
        }
        
        private void HandlePlantHealthChanged(PlantInstance plant)
        {
            var instanceId = plant.GetInstanceID();
            if (_plantInstances.TryGetValue(instanceId, out var instance))
            {
                // Update plant health visualization
                UpdatePlantHealthVisualization(instance, plant.Health);
            }
        }
        
        /// <summary>
        /// Update plant health visualization
        /// </summary>
        private void UpdatePlantHealthVisualization(SpeedTreePlantData instance, float newHealth)
        {
            if (instance == null) return;
            
            // Update stress visualizer
            _stressVisualizer?.UpdateHealthVisualization(instance, newHealth);
            
            // Update plant appearance based on health
            if (instance.Renderer != null)
            {
                var healthFactor = newHealth / 100f;
                var stressFactor = 1f - healthFactor;
                
                // Apply health-based visual changes
                ApplyHealthVisualization(instance, healthFactor, stressFactor);
            }
        }
        
        /// <summary>
        /// Apply health-based visual changes to plant
        /// </summary>
        private void ApplyHealthVisualization(SpeedTreePlantData instance, float healthFactor, float stressFactor)
        {
            // Implement health visualization logic here
            // This could include color changes, wilting effects, etc.
        }
        
        private void HandleEnvironmentalChange(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            foreach (var instance in _plantInstances.Values)
            {
                _environmentalProcessor.UpdateEnvironmentalResponse(instance, conditions);
            }
        }
        
        private void HandleWindChange(float windStrength)
        {
            // Update wind settings for all plant instances
            _windController?.UpdateGlobalWindStrength(windStrength);
        }
        
        private void HandleLightingChange(LightingConditions lightingConditions)
        {
            // Update lighting for all plant instances
            foreach (var instance in _plantInstances.Values)
            {
                UpdatePlantLighting(instance, lightingConditions.Intensity, lightingConditions.Color);
            }
        }
        
        private void UpdatePlantLighting(SpeedTreePlantData instance, float intensity, Color color)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            instance.Renderer.materialProperties.SetFloat("_LightIntensity", intensity);
            instance.Renderer.materialProperties.SetColor("_LightColor", color);
            
            // Adjust plant response to lighting
            var lightResponse = intensity / 1000f; // Normalize PPFD to 0-1
            instance.EnvironmentalModifiers["light_response"] = lightResponse;
#endif
        }
        
        #endregion
        
        #region Utility Methods
        
        private CannabisStrainAsset GetCannabisStrain(string strainId)
        {
            return _cannabisStrains.FirstOrDefault(s => s.StrainId == strainId);
        }
        
        private CannabisStrainAsset GetDefaultCannabisStrain()
        {
            return _cannabisStrains.FirstOrDefault();
        }
        
        private SpeedTreeAsset GetSpeedTreeAssetForStrain(CannabisStrainAsset strain)
        {
            if (strain?.SpeedTreeAssetPath != null && _loadedAssets.TryGetValue(strain.SpeedTreeAssetPath, out var asset))
            {
                return asset;
            }
            
            return _loadedAssets.Values.FirstOrDefault();
        }
        
        #endregion
        
        #region Performance Metrics
        
        private void UpdatePerformanceMetrics()
        {
            if (Time.time - _lastPerformanceUpdate >= 1f)
            {
                _performanceMetrics.ActivePlants = _plantInstances.Count;
                _performanceMetrics.VisiblePlants = _cullingManager?.VisiblePlantCount ?? _plantInstances.Count;
                _performanceMetrics.FrameRate = 1f / Time.unscaledDeltaTime;
                _performanceMetrics.MemoryUsage = _memoryManager?.GetMemoryUsage() ?? 0f;
                _performanceMetrics.BatchCount = _batchingManager?.CurrentBatchCount ?? 0;
                _performanceMetrics.DrawCalls = _batchingManager?.EstimatedDrawCalls ?? _plantInstances.Count;
                _performanceMetrics.LastUpdate = DateTime.Now;
                
                _lastPerformanceUpdate = Time.time;
            }
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceMetricsUpdated?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public SpeedTreePlantData GetPlantInstance(int instanceId)
        {
            return _plantInstances.TryGetValue(instanceId, out var instance) ? instance : null;
        }
        
        public List<SpeedTreePlantData> GetAllPlantInstances()
        {
            return _plantInstances.Values.ToList();
        }
        
        public List<SpeedTreePlantData> GetPlantInstancesByStrain(string strainId)
        {
            return _plantInstances.Values.Where(i => i.StrainAsset.StrainId == strainId).ToList();
        }
        
        public void SetQualityLevel(SpeedTreeQualityLevel quality)
        {
            _defaultQuality = quality;
            ApplyQualitySettings(quality);
        }
        
        public void SetWindEnabled(bool enabled)
        {
            _enableWindAnimation = enabled;
            _windController.SetEnabled(enabled);
        }
        
        public void SetGrowthAnimationEnabled(bool enabled)
        {
            _enableGrowthAnimation = enabled;
        }
        
        public void SetEnvironmentalResponseEnabled(bool enabled)
        {
            _enableEnvironmentalResponse = enabled;
        }
        
        public SpeedTreeSystemReport GetSystemReport()
        {
            return new SpeedTreeSystemReport
            {
                PerformanceMetrics = _performanceMetrics,
                PlantInstanceCount = _plantInstances.Count,
                LoadedAssetCount = _loadedAssets.Count,
                RegisteredStrainCount = _cannabisStrains.Count,
                ActiveSystemsStatus = new Dictionary<string, bool>
                {
                    ["WindAnimation"] = _enableWindAnimation,
                    ["GrowthAnimation"] = _enableGrowthAnimation,
                    ["EnvironmentalResponse"] = _enableEnvironmentalResponse,
                    ["StressVisualization"] = _enableStressVisualization,
                    ["GPUInstancing"] = _enableGPUInstancing,
                    ["DynamicBatching"] = _enableDynamicBatching
                }
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop all coroutines
            StopAllCoroutines();
            CancelInvoke();
            
            // Cleanup all plant instances
            foreach (var instance in _plantInstances.Values.ToList())
            {
                DestroyPlantInstance(instance.InstanceId);
            }
            
            // Cleanup subsystems
            _lodManager?.Cleanup();
            _batchingManager?.Cleanup();
            _cullingManager?.Cleanup();
            _memoryManager?.Cleanup();
            _windController?.Cleanup();
            _geneticsProcessor?.Cleanup();
            _growthAnimator?.Cleanup();
            _environmentalProcessor?.Cleanup();
            _stressVisualizer?.Cleanup();
            
            // Disconnect events
            DisconnectSystemEvents();
            
            LogInfo("Advanced SpeedTree Manager shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            if (_plantManager != null)
            {
                _plantManager.OnPlantAdded -= HandlePlantAdded;
                _plantManager.OnPlantHarvested -= HandlePlantRemoved;
                _plantManager.OnPlantStageChanged -= HandlePlantStageChanged;
                _plantManager.OnPlantHealthUpdated -= HandlePlantHealthChanged;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged -= HandleEnvironmentalChange;
                _environmentalManager.OnWindChanged -= HandleWindChange;
                _environmentalManager.OnLightingChanged -= HandleLightingChange;
            }
        }
        
        /// <summary>
        /// Convert DataPlantGrowthStage to PlantGrowthStage for compatibility
        /// </summary>
        private PlantGrowthStage ConvertToPlantGrowthStage(DataPlantGrowthStage dataStage)
        {
            return dataStage switch
            {
                DataPlantGrowthStage.Seed => PlantGrowthStage.Seed,
                DataPlantGrowthStage.Seedling => PlantGrowthStage.Seedling,
                DataPlantGrowthStage.Vegetative => PlantGrowthStage.Vegetative,
                DataPlantGrowthStage.Flowering => PlantGrowthStage.Flowering,
                DataPlantGrowthStage.Ripening => PlantGrowthStage.Harvest,
                DataPlantGrowthStage.Harvest => PlantGrowthStage.Harvest,
                _ => PlantGrowthStage.Seedling
            };
        }
    }
}