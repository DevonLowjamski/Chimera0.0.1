using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

#if UNITY_SPEEDTREE
using UnityEngine.Rendering;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Advanced material property controller for cannabis plant visualization.
    /// Manages dynamic material properties for SpeedTree integration, genetic trait expression,
    /// environmental response visualization, and real-time cannabis cultivation effects.
    /// Provides comprehensive material control for photorealistic cannabis rendering.
    /// </summary>
    public class AdvancedMaterialPropertyController : ChimeraManager, IVFXCompatibleSystem
    {
        [Header("Material Control Configuration")]
        [SerializeField] private bool _enableMaterialPropertyControl = true;
        [SerializeField] private bool _enableRealtimeMaterialUpdates = true;
        [SerializeField] private float _materialUpdateFrequency = 5f; // Updates per second
        [SerializeField] private bool _enablePerformanceOptimization = true;
        
        [Header("Cannabis-Specific Material Properties")]
        [SerializeField] private bool _enableTrichromeVisualization = true;
        [SerializeField] private bool _enableBudDevelopmentMaterials = true;
        [SerializeField] private bool _enableHealthStatusMaterials = true;
        [SerializeField] private bool _enableGeneticTraitVisualization = true;
        [SerializeField] private bool _enableEnvironmentalResponseMaterials = true;
        
        [Header("SpeedTree Material Integration")]
        [SerializeField] private bool _enableSpeedTreeMaterialControl = true;
        [SerializeField] private bool _enableWindMaterialEffects = true;
        [SerializeField] private bool _enableSeasonalMaterialChanges = true;
        [SerializeField] private bool _enableGrowthStageMaterials = true;
        
        [Header("Trichrome Material Settings")]
        [SerializeField, Range(0f, 1f)] private float _baseTrichromeAmount = 0.0f;
        [SerializeField, Range(0f, 2f)] private float _trichromeDensityMultiplier = 1.0f;
        [SerializeField] private Color _trichromeColor = Color.white;
        [SerializeField] private AnimationCurve _trichromeDevelopmentCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Health Visualization Materials")]
        [SerializeField] private Color _healthyColor = Color.green;
        [SerializeField] private Color _stressedColor = Color.yellow;
        [SerializeField] private Color _diseaseColor = Color.red;
        [SerializeField] private Color _deficiencyColor = new Color(0.8f, 0.6f, 0.2f);
        [SerializeField] private AnimationCurve _healthTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Environmental Response Materials")]
        [SerializeField] private bool _enableTemperatureColorResponse = true;
        [SerializeField] private bool _enableHumidityEffects = true;
        [SerializeField] private bool _enableLightResponseMaterials = true;
        [SerializeField] private bool _enableNutrientVisualization = true;
        
        [Header("Growth Stage Material Transitions")]
        [SerializeField] private Color _seedlingColor = new Color(0.7f, 1f, 0.7f);
        [SerializeField] private Color _vegetativeColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color _floweringColor = new Color(0.2f, 0.6f, 0.2f);
        [SerializeField] private Color _harvestColor = new Color(0.8f, 0.6f, 0.2f);
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentMaterialUpdates = 30;
        [SerializeField] private float _materialCullingDistance = 40f;
        [SerializeField] private bool _enableLODMaterialOptimization = true;
        [SerializeField] private bool _enableAdaptiveMaterialQuality = true;
        [SerializeField] private int _materialUpdateBudget = 1500; // Max updates per frame
        
        // Material Management State
        private Dictionary<string, PlantMaterialInstance> _activeMaterialInstances = new Dictionary<string, PlantMaterialInstance>();
        private Queue<string> _materialUpdateQueue = new Queue<string>();
        private List<MaterialPropertyUpdate> _pendingMaterialUpdates = new List<MaterialPropertyUpdate>();
        
        // Material Property Libraries
        private Dictionary<string, MaterialPropertySet> _materialPropertySets = new Dictionary<string, MaterialPropertySet>();
        private Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GrowthStageMaterialProfile> _growthStageMaterials;
        private Dictionary<string, GeneticTraitMaterialMapping> _geneticTraitMaterials;
        
        // VFX System Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private EnvironmentalResponseVFXController _environmentalResponseController;
        private DynamicGrowthAnimationSystem _dynamicGrowthSystem;
        private VFXCompatibilityLayer _vfxCompatibilityLayer;
        
        // Material System Components
        private MaterialPropertyAnimator _materialAnimator;
        private EnvironmentalMaterialProcessor _environmentalProcessor;
        private GeneticMaterialMapper _geneticMapper;
        private MaterialPerformanceMetrics _performanceMetrics;
        
        // Timing and Performance
        private float _lastMaterialUpdate = 0f;
        private int _materialUpdatesThisFrame = 0;
        private float _materialDeltaTime = 0f;
        
        // Environmental State
        private EnvironmentalConditions _currentEnvironment;
        
        // Events
        public System.Action<string, MaterialPropertySet> OnMaterialPropertiesUpdated;
        public System.Action<string, float> OnTrichromeAmountChanged;
        public System.Action<string, Color> OnHealthColorChanged;
        public System.Action<MaterialPerformanceMetrics> OnMaterialPerformanceUpdate;
        
        // Properties
        public int ActiveMaterialInstances => _activeMaterialInstances.Count;
        public MaterialPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public bool EnableMaterialControl => _enableMaterialPropertyControl;
        
        protected override void OnManagerInitialize()
        {
            InitializeMaterialSystem();
            InitializeMaterialLibraries();
            InitializeGrowthStageMaterials();
            InitializeGeneticTraitMaterials();
            ConnectToVFXSystems();
            StartMaterialProcessing();
            LogInfo("Advanced Material Property Controller initialized");
        }
        
        #region IVFXCompatibleSystem Implementation
        
        /// <summary>
        /// Initializes VFX compatibility layer integration for material property system.
        /// </summary>
        public void InitializeCompatibility(VFXCompatibilityLayer compatibilityLayer)
        {
            _vfxCompatibilityLayer = compatibilityLayer;
            
            if (_vfxCompatibilityLayer != null)
            {
                LogInfo("🎨 Advanced Material Property Controller connected to compatibility layer");
            }
        }
        
        /// <summary>
        /// Cleans up VFX compatibility layer integration.
        /// </summary>
        public void CleanupCompatibility()
        {
            _vfxCompatibilityLayer = null;
            LogInfo("🎨 Advanced Material Property Controller disconnected from compatibility layer");
        }
        
        /// <summary>
        /// Updates environmental response materials based on new environmental conditions.
        /// </summary>
        public void UpdateEnvironmentalResponse(EnvironmentalConditions conditions)
        {
            if (conditions == null) return;
            
            // Update internal environmental state
            _currentEnvironment = conditions.Clone();
            
            // Update all material instances with new environmental conditions
            UpdateAllMaterialEnvironmentalResponse(conditions);
            
            // Process environmental updates through compatibility layer
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(conditions);
            }
            
            LogInfo($"🎨 Updated material environmental response for {_activeMaterialInstances.Count} plant materials");
        }
        
        /// <summary>
        /// Updates growth animation materials based on growth data.
        /// </summary>
        public void UpdateGrowthAnimation(GrowthAnimationData growthData)
        {
            if (growthData == null) return;
            
            // Update material properties based on growth stage and progress
            if (_activeMaterialInstances.TryGetValue(growthData.PlantInstanceId, out var materialInstance))
            {
                UpdateMaterialForGrowthStage(materialInstance, growthData.GrowthStage, growthData.GrowthProgress);
            }
            
            LogInfo($"🎨 Updated material properties for growth stage: {growthData.PlantInstanceId.Substring(0, Math.Min(8, growthData.PlantInstanceId.Length))}");
        }
        
        #endregion
        
        private void Update()
        {
            if (!_enableMaterialPropertyControl) return;
            
            _materialDeltaTime = Time.deltaTime;
            
            if (Time.time - _lastMaterialUpdate >= 1f / _materialUpdateFrequency)
            {
                ProcessMaterialUpdateQueue();
                UpdateMaterialAnimations();
                UpdatePerformanceMetrics();
                _lastMaterialUpdate = Time.time;
            }
            
            ProcessRealtimeMaterialEffects();
        }
        
        private void InitializeMaterialSystem()
        {
            LogInfo("=== INITIALIZING ADVANCED MATERIAL PROPERTY SYSTEM ===");
            
            // Initialize material system components
            _materialAnimator = new MaterialPropertyAnimator();
            _environmentalProcessor = new EnvironmentalMaterialProcessor();
            _geneticMapper = new GeneticMaterialMapper();
            
            // Initialize environmental tracking
            _currentEnvironment = new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 0.6f,
                LightIntensity = 600f,
                WindSpeed = 2f,
                WindDirection = 90f,
                AirFlow = 1.0f,
                BarometricPressure = 1013.25f,
                CO2Level = 400f,
                LightDirection = -90f
            };
            
            // Initialize performance metrics
            _performanceMetrics = new MaterialPerformanceMetrics
            {
                ActiveMaterialInstances = 0,
                MaterialUpdatesPerSecond = 0f,
                AverageUpdateTime = 0f,
                TotalPropertySets = 0,
                MemoryUsage = 0f
            };
            
            LogInfo("✅ Material property system initialized");
        }
        
        private void InitializeMaterialLibraries()
        {
            LogInfo("Setting up material property libraries...");
            
            // Initialize base material property sets
            _materialPropertySets["DefaultCannabis"] = new MaterialPropertySet
            {
                SetName = "Default Cannabis",
                BaseColor = new Color(0.3f, 0.8f, 0.3f),
                Metallic = 0.1f,
                Smoothness = 0.4f,
                Normal = 1f,
                Emission = Color.black,
                TrichromeAmount = 0f,
                HealthMultiplier = 1f,
                EnvironmentalResponse = 1f
            };
            
            _materialPropertySets["HealthyCannabis"] = new MaterialPropertySet
            {
                SetName = "Healthy Cannabis",
                BaseColor = _healthyColor,
                Metallic = 0.05f,
                Smoothness = 0.5f,
                Normal = 1f,
                Emission = Color.black,
                TrichromeAmount = 0.3f,
                HealthMultiplier = 1.2f,
                EnvironmentalResponse = 1f
            };
            
            _materialPropertySets["StressedCannabis"] = new MaterialPropertySet
            {
                SetName = "Stressed Cannabis",
                BaseColor = _stressedColor,
                Metallic = 0.2f,
                Smoothness = 0.3f,
                Normal = 0.8f,
                Emission = Color.black,
                TrichromeAmount = 0.1f,
                HealthMultiplier = 0.8f,
                EnvironmentalResponse = 1.2f
            };
            
            _materialPropertySets["FloweringCannabis"] = new MaterialPropertySet
            {
                SetName = "Flowering Cannabis",
                BaseColor = _floweringColor,
                Metallic = 0.1f,
                Smoothness = 0.6f,
                Normal = 1.2f,
                Emission = new Color(0.1f, 0.05f, 0f),
                TrichromeAmount = 0.8f,
                HealthMultiplier = 1f,
                EnvironmentalResponse = 0.8f
            };
            
            LogInfo($"✅ Initialized {_materialPropertySets.Count} material property sets");
        }
        
        private void InitializeGrowthStageMaterials()
        {
            _growthStageMaterials = new Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GrowthStageMaterialProfile>();
            
            _growthStageMaterials[ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling] = new GrowthStageMaterialProfile
            {
                GrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                BaseColor = _seedlingColor,
                TrichromeAmount = 0f,
                HealthSensitivity = 1.5f,
                EnvironmentalSensitivity = 1.8f,
                MaterialTransitionSpeed = 2f
            };
            
            _growthStageMaterials[ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative] = new GrowthStageMaterialProfile
            {
                GrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                BaseColor = _vegetativeColor,
                TrichromeAmount = 0.1f,
                HealthSensitivity = 1f,
                EnvironmentalSensitivity = 1.2f,
                MaterialTransitionSpeed = 1f
            };
            
            _growthStageMaterials[ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering] = new GrowthStageMaterialProfile
            {
                GrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                BaseColor = _floweringColor,
                TrichromeAmount = 0.8f,
                HealthSensitivity = 1.3f,
                EnvironmentalSensitivity = 1f,
                MaterialTransitionSpeed = 0.5f
            };
            
            LogInfo($"✅ Initialized materials for {_growthStageMaterials.Count} growth stages");
        }
        
        private void InitializeGeneticTraitMaterials()
        {
            _geneticTraitMaterials = new Dictionary<string, GeneticTraitMaterialMapping>();
            
            // High THC strains
            _geneticTraitMaterials["HighTHC"] = new GeneticTraitMaterialMapping
            {
                TraitName = "High THC",
                ColorModifier = new Color(0.9f, 1f, 0.9f),
                TrichromeMultiplier = 1.5f,
                MaterialIntensity = 1.2f,
                SpecialEffects = true
            };
            
            // High CBD strains
            _geneticTraitMaterials["HighCBD"] = new GeneticTraitMaterialMapping
            {
                TraitName = "High CBD",
                ColorModifier = new Color(0.8f, 0.9f, 1f),
                TrichromeMultiplier = 1.2f,
                MaterialIntensity = 1f,
                SpecialEffects = false
            };
            
            // Purple genetics
            _geneticTraitMaterials["PurpleGenetics"] = new GeneticTraitMaterialMapping
            {
                TraitName = "Purple Genetics",
                ColorModifier = new Color(0.6f, 0.4f, 0.8f),
                TrichromeMultiplier = 1.1f,
                MaterialIntensity = 1.1f,
                SpecialEffects = true
            };
            
            LogInfo($"✅ Initialized {_geneticTraitMaterials.Count} genetic trait material mappings");
        }
        
        private void ConnectToVFXSystems()
        {
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            _environmentalResponseController = FindObjectOfType<EnvironmentalResponseVFXController>();
            _dynamicGrowthSystem = FindObjectOfType<DynamicGrowthAnimationSystem>();
            _vfxCompatibilityLayer = FindObjectOfType<VFXCompatibilityLayer>();
            
            if (_vfxCompatibilityLayer != null)
            {
                InitializeCompatibility(_vfxCompatibilityLayer);
                LogInfo("✅ VFX Compatibility Layer connected");
            }
            
            LogInfo("✅ Connected to VFX management systems");
        }
        
        private void StartMaterialProcessing()
        {
            if (_enableMaterialPropertyControl)
            {
                StartCoroutine(MaterialUpdateCoroutine());
                LogInfo("🎨 Material property processing started");
            }
        }
        
        /// <summary>
        /// Registers a plant for advanced material property control.
        /// </summary>
        public string RegisterPlantMaterials(Transform plantTransform, Renderer plantRenderer, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, PlantStrainSO strainData = null)
        {
            string plantId = System.Guid.NewGuid().ToString();
            
            var materialInstance = new PlantMaterialInstance
            {
                PlantId = plantId,
                PlantTransform = plantTransform,
                PlantRenderer = plantRenderer,
                GrowthStage = growthStage,
                StrainData = strainData,
                IsActive = true,
                RegistrationTime = Time.time,
                LastMaterialUpdate = Time.time,
                
                // Initialize material properties
                CurrentMaterialSet = _materialPropertySets["DefaultCannabis"].Clone(),
                TargetMaterialSet = _materialPropertySets["DefaultCannabis"].Clone(),
                MaterialTransitionProgress = 1f,
                TransitionSpeed = 1f,
                
                // Initialize cannabis-specific properties
                CurrentTrichromeAmount = _baseTrichromeAmount,
                TargetTrichromeAmount = _baseTrichromeAmount,
                HealthStatus = 1f,
                EnvironmentalStress = 0f,
                
                // Initialize original material backup
                OriginalMaterials = new Material[plantRenderer.materials.Length]
            };
            
            // Backup original materials
            for (int i = 0; i < plantRenderer.materials.Length; i++)
            {
                materialInstance.OriginalMaterials[i] = new Material(plantRenderer.materials[i]);
            }
            
            // Apply initial material properties based on growth stage
            ApplyGrowthStageMaterialProperties(materialInstance, growthStage);
            
            _activeMaterialInstances[plantId] = materialInstance;
            _materialUpdateQueue.Enqueue(plantId);
            
            LogInfo($"🎨 Registered plant materials {plantId.Substring(0, 8)} for advanced property control");
            OnMaterialPropertiesUpdated?.Invoke(plantId, materialInstance.CurrentMaterialSet);
            
            return plantId;
        }
        
        /// <summary>
        /// Updates material properties for a specific plant based on health status.
        /// </summary>
        public void UpdatePlantHealthMaterials(string plantId, float healthStatus, float stressLevel = 0f)
        {
            if (!_activeMaterialInstances.TryGetValue(plantId, out var materialInstance))
                return;
                
            materialInstance.HealthStatus = Mathf.Clamp01(healthStatus);
            materialInstance.EnvironmentalStress = Mathf.Clamp01(stressLevel);
            
            // Calculate target health color
            Color healthColor = CalculateHealthColor(healthStatus, stressLevel);
            materialInstance.TargetMaterialSet.BaseColor = healthColor;
            
            // Update health multiplier
            materialInstance.TargetMaterialSet.HealthMultiplier = healthStatus;
            
            // Trigger immediate material update
            _materialUpdateQueue.Enqueue(plantId);
            
            OnHealthColorChanged?.Invoke(plantId, healthColor);
            LogInfo($"🎨 Updated health materials for plant {plantId.Substring(0, 8)}: Health={healthStatus:F2}, Stress={stressLevel:F2}");
        }
        
        /// <summary>
        /// Updates trichrome amount for a specific plant.
        /// </summary>
        public void UpdatePlantTrichromes(string plantId, float trichromeAmount, bool animate = true)
        {
            if (!_activeMaterialInstances.TryGetValue(plantId, out var materialInstance))
                return;
                
            float clampedAmount = Mathf.Clamp01(trichromeAmount) * _trichromeDensityMultiplier;
            
            if (animate)
            {
                materialInstance.TargetTrichromeAmount = clampedAmount;
            }
            else
            {
                materialInstance.CurrentTrichromeAmount = clampedAmount;
                materialInstance.TargetTrichromeAmount = clampedAmount;
            }
            
            materialInstance.TargetMaterialSet.TrichromeAmount = clampedAmount;
            
            // Apply trichrome visual effects
            ApplyTrichromeEffects(materialInstance);
            
            OnTrichromeAmountChanged?.Invoke(plantId, clampedAmount);
            LogInfo($"🎨 Updated trichrome amount for plant {plantId.Substring(0, 8)}: {clampedAmount:F2}");
        }
        
        private void UpdateAllMaterialEnvironmentalResponse(EnvironmentalConditions conditions)
        {
            foreach (var kvp in _activeMaterialInstances)
            {
                var materialInstance = kvp.Value;
                if (materialInstance.IsActive)
                {
                    UpdateMaterialEnvironmentalResponse(materialInstance, conditions);
                }
            }
        }
        
        private void UpdateMaterialEnvironmentalResponse(PlantMaterialInstance materialInstance, EnvironmentalConditions conditions)
        {
            // Temperature response
            if (_enableTemperatureColorResponse)
            {
                Color tempColor = CalculateTemperatureColorResponse(conditions.Temperature);
                materialInstance.TargetMaterialSet.BaseColor = Color.Lerp(materialInstance.TargetMaterialSet.BaseColor, tempColor, 0.3f);
            }
            
            // Light response
            if (_enableLightResponseMaterials)
            {
                float lightResponse = CalculateLightMaterialResponse(conditions.LightIntensity);
                materialInstance.TargetMaterialSet.EnvironmentalResponse = lightResponse;
            }
            
            // Humidity effects
            if (_enableHumidityEffects)
            {
                float humidityEffect = CalculateHumidityMaterialEffect(conditions.Humidity);
                materialInstance.TargetMaterialSet.Smoothness = Mathf.Lerp(0.3f, 0.7f, humidityEffect);
            }
            
            materialInstance.LastMaterialUpdate = Time.time;
        }
        
        private void UpdateMaterialForGrowthStage(PlantMaterialInstance materialInstance, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, float growthProgress)
        {
            materialInstance.GrowthStage = growthStage;
            
            if (_growthStageMaterials.TryGetValue(growthStage, out var stageProfile))
            {
                // Update material properties based on growth stage
                materialInstance.TargetMaterialSet.BaseColor = stageProfile.BaseColor;
                materialInstance.TargetTrichromeAmount = stageProfile.TrichromeAmount * growthProgress;
                materialInstance.TransitionSpeed = stageProfile.MaterialTransitionSpeed;
                
                // Apply growth stage specific effects
                ApplyGrowthStageMaterialProperties(materialInstance, growthStage);
            }
        }
        
        private void ApplyGrowthStageMaterialProperties(PlantMaterialInstance materialInstance, 
            ProjectChimera.Data.Genetics.PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling:
                    materialInstance.TargetMaterialSet = _materialPropertySets["DefaultCannabis"].Clone();
                    materialInstance.TargetMaterialSet.BaseColor = _seedlingColor;
                    materialInstance.TargetTrichromeAmount = 0f;
                    break;
                    
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative:
                    materialInstance.TargetMaterialSet = _materialPropertySets["HealthyCannabis"].Clone();
                    materialInstance.TargetMaterialSet.BaseColor = _vegetativeColor;
                    materialInstance.TargetTrichromeAmount = 0.1f;
                    break;
                    
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering:
                    materialInstance.TargetMaterialSet = _materialPropertySets["FloweringCannabis"].Clone();
                    materialInstance.TargetMaterialSet.BaseColor = _floweringColor;
                    materialInstance.TargetTrichromeAmount = 0.8f;
                    break;
                    
                default:
                    materialInstance.TargetMaterialSet = _materialPropertySets["DefaultCannabis"].Clone();
                    break;
            }
        }
        
        private Color CalculateHealthColor(float healthStatus, float stressLevel)
        {
            if (healthStatus > 0.8f && stressLevel < 0.2f)
                return _healthyColor;
            else if (healthStatus > 0.6f && stressLevel < 0.4f)
                return Color.Lerp(_healthyColor, _stressedColor, (1f - healthStatus) * 2f);
            else if (healthStatus > 0.3f)
                return Color.Lerp(_stressedColor, _diseaseColor, (1f - healthStatus) * 1.5f);
            else
                return _diseaseColor;
        }
        
        private Color CalculateTemperatureColorResponse(float temperature)
        {
            // Optimal temperature range: 20-26°C
            if (temperature < 18f) // Cold stress - blue tint
                return Color.Lerp(Color.white, Color.blue, (18f - temperature) / 10f);
            else if (temperature > 30f) // Heat stress - red tint
                return Color.Lerp(Color.white, Color.red, (temperature - 30f) / 10f);
            else
                return Color.white; // Normal temperature
        }
        
        private float CalculateLightMaterialResponse(float lightIntensity)
        {
            // Optimal light range: 400-800 PPFD
            return Mathf.Clamp01(lightIntensity / 800f);
        }
        
        private float CalculateHumidityMaterialEffect(float humidity)
        {
            // Optimal humidity: 50-70%
            return Mathf.Clamp01((humidity - 0.3f) / 0.4f);
        }
        
        private void ApplyTrichromeEffects(PlantMaterialInstance materialInstance)
        {
            // Apply trichrome visual effects to materials
            if (materialInstance.PlantRenderer != null && _enableTrichromeVisualization)
            {
                foreach (var material in materialInstance.PlantRenderer.materials)
                {
                    if (material.HasProperty("_TrichromeAmount"))
                    {
                        material.SetFloat("_TrichromeAmount", materialInstance.CurrentTrichromeAmount);
                    }
                    
                    if (material.HasProperty("_TrichromeColor"))
                    {
                        material.SetColor("_TrichromeColor", _trichromeColor);
                    }
                }
            }
        }
        
        private void ProcessMaterialUpdateQueue()
        {
            int updatesToProcess = Mathf.Min(_maxConcurrentMaterialUpdates, _materialUpdateQueue.Count);
            _materialUpdatesThisFrame = 0;
            
            for (int i = 0; i < updatesToProcess && _materialUpdateQueue.Count > 0; i++)
            {
                string plantId = _materialUpdateQueue.Dequeue();
                if (_activeMaterialInstances.TryGetValue(plantId, out var materialInstance))
                {
                    ProcessMaterialInstanceUpdate(materialInstance);
                    _materialUpdatesThisFrame++;
                }
            }
        }
        
        private void ProcessMaterialInstanceUpdate(PlantMaterialInstance materialInstance)
        {
            if (!materialInstance.IsActive || materialInstance.PlantRenderer == null)
                return;
                
            // Update material transitions
            UpdateMaterialTransitions(materialInstance);
            
            // Apply material properties to renderer
            ApplyMaterialProperties(materialInstance);
            
            materialInstance.LastMaterialUpdate = Time.time;
        }
        
        private void UpdateMaterialTransitions(PlantMaterialInstance materialInstance)
        {
            if (materialInstance.MaterialTransitionProgress < 1f)
            {
                materialInstance.MaterialTransitionProgress = Mathf.MoveTowards(
                    materialInstance.MaterialTransitionProgress, 
                    1f, 
                    materialInstance.TransitionSpeed * Time.deltaTime
                );
                
                // Lerp material properties
                materialInstance.CurrentMaterialSet = MaterialPropertySet.Lerp(
                    materialInstance.CurrentMaterialSet,
                    materialInstance.TargetMaterialSet,
                    materialInstance.MaterialTransitionProgress
                );
            }
            
            // Update trichrome transitions
            materialInstance.CurrentTrichromeAmount = Mathf.MoveTowards(
                materialInstance.CurrentTrichromeAmount,
                materialInstance.TargetTrichromeAmount,
                2f * Time.deltaTime
            );
        }
        
        private void ApplyMaterialProperties(PlantMaterialInstance materialInstance)
        {
            foreach (var material in materialInstance.PlantRenderer.materials)
            {
                // Apply base color
                if (material.HasProperty("_BaseColor") || material.HasProperty("_Color"))
                {
                    string colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                    material.SetColor(colorProperty, materialInstance.CurrentMaterialSet.BaseColor);
                }
                
                // Apply metallic and smoothness
                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", materialInstance.CurrentMaterialSet.Metallic);
                    
                if (material.HasProperty("_Smoothness") || material.HasProperty("_Glossiness"))
                {
                    string smoothnessProperty = material.HasProperty("_Smoothness") ? "_Smoothness" : "_Glossiness";
                    material.SetFloat(smoothnessProperty, materialInstance.CurrentMaterialSet.Smoothness);
                }
                
                // Apply emission
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", materialInstance.CurrentMaterialSet.Emission);
                
                // Apply cannabis-specific properties
                if (material.HasProperty("_TrichromeAmount"))
                    material.SetFloat("_TrichromeAmount", materialInstance.CurrentTrichromeAmount);
                    
                if (material.HasProperty("_HealthMultiplier"))
                    material.SetFloat("_HealthMultiplier", materialInstance.CurrentMaterialSet.HealthMultiplier);
            }
        }
        
        private void UpdateMaterialAnimations()
        {
            // Update material animations and effects
            foreach (var kvp in _activeMaterialInstances)
            {
                var materialInstance = kvp.Value;
                if (materialInstance.IsActive)
                {
                    UpdateMaterialInstanceAnimations(materialInstance);
                }
            }
        }
        
        private void UpdateMaterialInstanceAnimations(PlantMaterialInstance materialInstance)
        {
            // Apply time-based material animations
            if (_enableRealtimeMaterialUpdates && materialInstance.PlantRenderer != null)
            {
                foreach (var material in materialInstance.PlantRenderer.materials)
                {
                    // Animate emission for flowering plants
                    if (materialInstance.GrowthStage == ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering)
                    {
                        float emissionPulse = Mathf.Sin(Time.time * 0.5f) * 0.05f + 0.05f;
                        Color emissionColor = materialInstance.CurrentMaterialSet.Emission + Color.white * emissionPulse;
                        
                        if (material.HasProperty("_EmissionColor"))
                            material.SetColor("_EmissionColor", emissionColor);
                    }
                }
            }
        }
        
        private void ProcessRealtimeMaterialEffects()
        {
            if (!_enableRealtimeMaterialUpdates) return;
            
            // Process real-time effects like wind material displacement
            foreach (var kvp in _activeMaterialInstances)
            {
                var materialInstance = kvp.Value;
                if (materialInstance.IsActive && _enableWindMaterialEffects)
                {
                    ApplyWindMaterialEffects(materialInstance);
                }
            }
        }
        
        private void ApplyWindMaterialEffects(PlantMaterialInstance materialInstance)
        {
            if (materialInstance.PlantRenderer == null) return;
            
            // Apply wind-based material effects
            float windStrength = _currentEnvironment?.WindSpeed ?? 0f;
            
            foreach (var material in materialInstance.PlantRenderer.materials)
            {
                if (material.HasProperty("_WindEffect"))
                {
                    material.SetFloat("_WindEffect", windStrength * 0.1f);
                }
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveMaterialInstances = _activeMaterialInstances.Count;
            _performanceMetrics.MaterialUpdatesPerSecond = _materialUpdatesThisFrame / _materialDeltaTime;
            _performanceMetrics.TotalPropertySets = _materialPropertySets.Count;
            
            OnMaterialPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        private IEnumerator MaterialUpdateCoroutine()
        {
            while (_enableMaterialPropertyControl)
            {
                yield return new WaitForSeconds(1f / _materialUpdateFrequency);
                
                // Queue all plants for material updates
                foreach (var plantId in _activeMaterialInstances.Keys)
                {
                    _materialUpdateQueue.Enqueue(plantId);
                }
            }
        }
        
        /// <summary>
        /// Unregisters a plant from material property control.
        /// </summary>
        public void UnregisterPlantMaterials(string plantId)
        {
            if (_activeMaterialInstances.TryGetValue(plantId, out var materialInstance))
            {
                // Restore original materials
                if (materialInstance.PlantRenderer != null && materialInstance.OriginalMaterials != null)
                {
                    materialInstance.PlantRenderer.materials = materialInstance.OriginalMaterials;
                }
                
                _activeMaterialInstances.Remove(plantId);
                LogInfo($"🎨 Unregistered plant materials {plantId.Substring(0, 8)}");
            }
        }
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            
            // Restore all original materials
            foreach (var kvp in _activeMaterialInstances)
            {
                UnregisterPlantMaterials(kvp.Key);
            }
            
            _activeMaterialInstances.Clear();
            LogInfo("Advanced Material Property Controller shutdown");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class PlantMaterialInstance
    {
        public string PlantId;
        public Transform PlantTransform;
        public Renderer PlantRenderer;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public PlantStrainSO StrainData;
        public bool IsActive;
        public float RegistrationTime;
        public float LastMaterialUpdate;
        
        // Material Property State
        public MaterialPropertySet CurrentMaterialSet;
        public MaterialPropertySet TargetMaterialSet;
        public float MaterialTransitionProgress;
        public float TransitionSpeed;
        
        // Cannabis-Specific Properties
        public float CurrentTrichromeAmount;
        public float TargetTrichromeAmount;
        public float HealthStatus;
        public float EnvironmentalStress;
        
        // Material Backup
        public Material[] OriginalMaterials;
    }
    
    [System.Serializable]
    public class MaterialPropertySet
    {
        public string SetName;
        public Color BaseColor;
        public float Metallic;
        public float Smoothness;
        public float Normal;
        public Color Emission;
        public float TrichromeAmount;
        public float HealthMultiplier;
        public float EnvironmentalResponse;
        
        public MaterialPropertySet Clone()
        {
            return new MaterialPropertySet
            {
                SetName = SetName,
                BaseColor = BaseColor,
                Metallic = Metallic,
                Smoothness = Smoothness,
                Normal = Normal,
                Emission = Emission,
                TrichromeAmount = TrichromeAmount,
                HealthMultiplier = HealthMultiplier,
                EnvironmentalResponse = EnvironmentalResponse
            };
        }
        
        public static MaterialPropertySet Lerp(MaterialPropertySet a, MaterialPropertySet b, float t)
        {
            return new MaterialPropertySet
            {
                SetName = b.SetName,
                BaseColor = Color.Lerp(a.BaseColor, b.BaseColor, t),
                Metallic = Mathf.Lerp(a.Metallic, b.Metallic, t),
                Smoothness = Mathf.Lerp(a.Smoothness, b.Smoothness, t),
                Normal = Mathf.Lerp(a.Normal, b.Normal, t),
                Emission = Color.Lerp(a.Emission, b.Emission, t),
                TrichromeAmount = Mathf.Lerp(a.TrichromeAmount, b.TrichromeAmount, t),
                HealthMultiplier = Mathf.Lerp(a.HealthMultiplier, b.HealthMultiplier, t),
                EnvironmentalResponse = Mathf.Lerp(a.EnvironmentalResponse, b.EnvironmentalResponse, t)
            };
        }
    }
    
    [System.Serializable]
    public class GrowthStageMaterialProfile
    {
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public Color BaseColor;
        public float TrichromeAmount;
        public float HealthSensitivity;
        public float EnvironmentalSensitivity;
        public float MaterialTransitionSpeed;
    }
    
    [System.Serializable]
    public class GeneticTraitMaterialMapping
    {
        public string TraitName;
        public Color ColorModifier;
        public float TrichromeMultiplier;
        public float MaterialIntensity;
        public bool SpecialEffects;
    }
    
    [System.Serializable]
    public class MaterialPropertyUpdate
    {
        public string PlantId;
        public string PropertyName;
        public object NewValue;
        public float UpdateTime;
        public bool Animate;
    }
    
    public class MaterialPropertyAnimator
    {
        public void UpdateAnimations(float deltaTime)
        {
            // Animation update logic
        }
    }
    
    public class EnvironmentalMaterialProcessor
    {
        public void ProcessEnvironmentalEffects(EnvironmentalConditions conditions)
        {
            // Environmental processing logic
        }
    }
    
    public class GeneticMaterialMapper
    {
        public void ApplyGeneticTraits(PlantMaterialInstance instance, PlantStrainSO strainData)
        {
            // Genetic trait mapping logic
        }
    }
    
    [System.Serializable]
    public class MaterialPerformanceMetrics
    {
        public int ActiveMaterialInstances;
        public float MaterialUpdatesPerSecond;
        public float AverageUpdateTime;
        public int TotalPropertySets;
        public float MemoryUsage;
    }
    
    #endregion
}