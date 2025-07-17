using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Controls genetic morphology visualization for cannabis plants.
    /// Manages real-time visual expression of genetic traits through sophisticated
    /// morphological changes, color variations, structural modifications, and VFX effects.
    /// </summary>
    public class GeneticMorphologyVFXController : ChimeraManager
    {
        [Header("Genetic Morphology Configuration")]
        [SerializeField] private bool _enableMorphologyVisualization = true;
        [SerializeField] private bool _enableRealtimeGenetics = true;
        [SerializeField] private float _morphologyUpdateInterval = 0.2f;
        
        [Header("Trait Expression Settings")]
        [SerializeField] private bool _enableColorTraits = true;
        [SerializeField] private bool _enableSizeTraits = true;
        [SerializeField] private bool _enableStructuralTraits = true;
        [SerializeField] private bool _enablePhysiologicalTraits = true;
        
        [Header("Cannabis Morphology Traits")]
        [SerializeField] private bool _enableLeafShape = true;
        [SerializeField] private bool _enableBudStructure = true;
        [SerializeField] private bool _enableTrichromeExpression = true;
        [SerializeField] private bool _enableCannabinoidVisualization = true;
        [SerializeField] private bool _enableTerpeneVisualization = true;
        
        [Header("Visual Expression Ranges")]
        [SerializeField, Range(0f, 2f)] private float _sizeVariationRange = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _colorVariationIntensity = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _structuralVariationRange = 0.2f;
        [SerializeField, Range(0f, 5f)] private float _expressionSpeed = 1f;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentMorphologies = 25;
        [SerializeField] private float _morphologyCullingDistance = 20f;
        [SerializeField] private bool _enableLODMorphology = true;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Genetic Morphology State
        private Dictionary<string, PlantMorphologyInstance> _activeMorphologies = new Dictionary<string, PlantMorphologyInstance>();
        private Queue<string> _morphologyUpdateQueue = new Queue<string>();
        private List<GeneticExpressionUpdate> _pendingGeneticUpdates = new List<GeneticExpressionUpdate>();
        
        // VFX Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private PlantVFXAttachmentManager _attachmentManager;
        private TrichromeVFXController _trichromeController;
        
        // Genetic Trait Mappings
        private Dictionary<GeneticTraitType, TraitVisualizationData> _traitVisualizations;
        private Dictionary<string, GeneticExpressionProfile> _expressionProfiles;
        private MorphologyPerformanceMetrics _performanceMetrics;
        private float _lastMorphologyUpdate = 0f;
        private int _morphologiesProcessedThisFrame = 0;
        
        // Cannabis-Specific Trait Definitions
        private Dictionary<CannabisTraitCategory, CannabisTraitVisualization> _cannabisTraits;
        
        // Events
        public System.Action<string, GeneticTraitType, float> OnTraitExpressed;
        public System.Action<string, PlantMorphologyInstance> OnMorphologyUpdated;
        public System.Action<string, CannabisTraitCategory, GeneticExpressionData> OnCannabisTraitChanged;
        public System.Action<MorphologyPerformanceMetrics> OnMorphologyPerformanceUpdate;
        
        // Properties
        public int ActiveMorphologies => _activeMorphologies.Count;
        public MorphologyPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public bool EnableMorphologyVisualization => _enableMorphologyVisualization;
        
        protected override void OnManagerInitialize()
        {
            InitializeMorphologySystem();
            InitializeGeneticTraitMappings();
            InitializeCannabisTraitDefinitions();
            ConnectToVFXSystems();
            InitializePerformanceTracking();
            StartMorphologyProcessing();
            LogInfo("Genetic Morphology VFX Controller initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            
            // Clean up all morphology instances
            foreach (var kvp in _activeMorphologies)
            {
                CleanupMorphologyInstance(kvp.Value);
            }
            _activeMorphologies.Clear();
            
            CancelInvoke();
            LogInfo("Genetic Morphology VFX Controller shutdown complete");
        }
        
        private void Update()
        {
            if (_enableMorphologyVisualization && Time.time - _lastMorphologyUpdate >= _morphologyUpdateInterval)
            {
                ProcessMorphologyUpdates();
                UpdateGeneticExpressions();
                UpdatePerformanceMetrics();
                _lastMorphologyUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeMorphologySystem()
        {
            LogInfo("=== INITIALIZING GENETIC MORPHOLOGY VISUALIZATION ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph available - genetic visualization effects enabled");
            #else
            LogWarning("⚠️ VFX Graph not available - using fallback genetic visualization");
            #endif
            
            // Initialize performance metrics
            _performanceMetrics = new MorphologyPerformanceMetrics
            {
                ActiveMorphologies = 0,
                MorphologyUpdatesPerSecond = 0f,
                AverageExpressionTime = 0f,
                GeneticComplexity = 0f,
                TargetFrameRate = 60f,
                LastUpdate = DateTime.Now
            };
            
            LogInfo("✅ Genetic morphology system initialized");
        }
        
        private void InitializeGeneticTraitMappings()
        {
            LogInfo("Setting up genetic trait visualization mappings...");
            
            _traitVisualizations = new Dictionary<GeneticTraitType, TraitVisualizationData>
            {
                [GeneticTraitType.PlantHeight] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.PlantHeight,
                    VisualizationMethod = TraitVisualizationMethod.Scale,
                    BaseScale = Vector3.one,
                    VariationRange = new Vector2(0.5f, 2.0f),
                    ExpressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                    VFXType = CannabisVFXType.GeneticTraits,
                    Description = "Plant height through scale modification"
                },
                
                [GeneticTraitType.LeafColor] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.LeafColor,
                    VisualizationMethod = TraitVisualizationMethod.MaterialColor,
                    BaseColor = Color.green,
                    ColorVariations = new Color[] { Color.green, new Color(0.4f, 0.6f, 0.2f), new Color(0.2f, 0.4f, 0.1f) },
                    ExpressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
                    VFXType = CannabisVFXType.GeneticTraits,
                    Description = "Leaf coloration variations"
                },
                
                [GeneticTraitType.BudDensity] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.BudDensity,
                    VisualizationMethod = TraitVisualizationMethod.ParticleCount,
                    VariationRange = new Vector2(50f, 200f),
                    ExpressionCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f),
                    VFXType = CannabisVFXType.TrichromeGrowth,
                    Description = "Bud density through particle visualization"
                },
                
                [GeneticTraitType.TrichromeProduction] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.TrichromeProduction,
                    VisualizationMethod = TraitVisualizationMethod.VFXIntensity,
                    VariationRange = new Vector2(0.1f, 1.0f),
                    ExpressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                    VFXType = CannabisVFXType.TrichromeGrowth,
                    Description = "Trichrome production visualization"
                },
                
                [GeneticTraitType.FloweringTime] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.FloweringTime,
                    VisualizationMethod = TraitVisualizationMethod.AnimationSpeed,
                    VariationRange = new Vector2(0.5f, 2.0f),
                    ExpressionCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 2f),
                    VFXType = CannabisVFXType.PlantGrowth,
                    Description = "Flowering speed visualization"
                },
                
                [GeneticTraitType.DiseaseResistance] = new TraitVisualizationData
                {
                    TraitType = GeneticTraitType.DiseaseResistance,
                    VisualizationMethod = TraitVisualizationMethod.HealthGlow,
                    BaseColor = Color.cyan,
                    VariationRange = new Vector2(0.2f, 1.0f),
                    ExpressionCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f),
                    VFXType = CannabisVFXType.HealthIndicator,
                    Description = "Disease resistance glow effect"
                }
            };
            
            LogInfo($"✅ Configured {_traitVisualizations.Count} genetic trait visualizations");
        }
        
        private void InitializeCannabisTraitDefinitions()
        {
            LogInfo("Setting up cannabis-specific trait visualizations...");
            
            _cannabisTraits = new Dictionary<CannabisTraitCategory, CannabisTraitVisualization>
            {
                [CannabisTraitCategory.Morphology] = new CannabisTraitVisualization
                {
                    Category = CannabisTraitCategory.Morphology,
                    CategoryName = "Plant Morphology",
                    PrimaryColor = Color.green,
                    SecondaryColor = new Color(0.3f, 0.7f, 0.2f),
                    EffectIntensity = 1.0f,
                    VisualizationMethod = TraitVisualizationMethod.Scale,
                    RequiresVFXGraph = false,
                    TraitCount = 5
                },
                
                [CannabisTraitCategory.Cannabinoids] = new CannabisTraitVisualization
                {
                    Category = CannabisTraitCategory.Cannabinoids,
                    CategoryName = "Cannabinoid Production",
                    PrimaryColor = new Color(0.8f, 0.3f, 0.8f), // Purple
                    SecondaryColor = new Color(0.6f, 0.1f, 0.6f),
                    EffectIntensity = 0.8f,
                    VisualizationMethod = TraitVisualizationMethod.VFXIntensity,
                    RequiresVFXGraph = true,
                    TraitCount = 4
                },
                
                [CannabisTraitCategory.Terpenes] = new CannabisTraitVisualization
                {
                    Category = CannabisTraitCategory.Terpenes,
                    CategoryName = "Terpene Profile",
                    PrimaryColor = new Color(1f, 0.8f, 0.2f), // Golden
                    SecondaryColor = new Color(0.8f, 0.6f, 0.1f),
                    EffectIntensity = 0.6f,
                    VisualizationMethod = TraitVisualizationMethod.ParticleColor,
                    RequiresVFXGraph = true,
                    TraitCount = 8
                },
                
                [CannabisTraitCategory.GrowthPattern] = new CannabisTraitVisualization
                {
                    Category = CannabisTraitCategory.GrowthPattern,
                    CategoryName = "Growth Characteristics",
                    PrimaryColor = Color.cyan,
                    SecondaryColor = new Color(0.2f, 0.8f, 0.8f),
                    EffectIntensity = 0.7f,
                    VisualizationMethod = TraitVisualizationMethod.AnimationSpeed,
                    RequiresVFXGraph = false,
                    TraitCount = 6
                },
                
                [CannabisTraitCategory.EnvironmentalResponse] = new CannabisTraitVisualization
                {
                    Category = CannabisTraitCategory.EnvironmentalResponse,
                    CategoryName = "Environmental Adaptation",
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.9f, 0.9f, 0.9f),
                    EffectIntensity = 0.5f,
                    VisualizationMethod = TraitVisualizationMethod.EnvironmentalGlow,
                    RequiresVFXGraph = true,
                    TraitCount = 7
                }
            };
            
            LogInfo($"✅ Configured {_cannabisTraits.Count} cannabis trait categories");
        }
        
        private void ConnectToVFXSystems()
        {
            _vfxTemplateManager = FindFirstObjectByType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindFirstObjectByType<SpeedTreeVFXIntegrationManager>();
            _attachmentManager = FindFirstObjectByType<PlantVFXAttachmentManager>();
            _trichromeController = FindFirstObjectByType<TrichromeVFXController>();
            
            if (_vfxTemplateManager == null)
                LogWarning("⚠️ CannabisVFXTemplateManager not found - VFX visualization limited");
                
            if (_speedTreeIntegration == null)
                LogWarning("⚠️ SpeedTreeVFXIntegrationManager not found - SpeedTree integration unavailable");
                
            if (_attachmentManager == null)
                LogWarning("⚠️ PlantVFXAttachmentManager not found - attachment point visualization unavailable");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        private void StartMorphologyProcessing()
        {
            if (_enableMorphologyVisualization)
            {
                StartCoroutine(ContinuousMorphologyProcessing());
                LogInfo("🧬 Genetic morphology processing started");
            }
        }
        
        #endregion
        
        #region Morphology Instance Management
        
        public string CreateMorphologyInstance(Transform plantTransform, PlantStrainSO strainData = null)
        {
            string instanceId = Guid.NewGuid().ToString();
            
            var morphologyInstance = new PlantMorphologyInstance
            {
                InstanceId = instanceId,
                PlantTransform = plantTransform,
                StrainData = strainData,
                IsActive = true,
                CreationTime = Time.time,
                LastUpdateTime = 0f,
                GeneticProfile = new GeneticExpressionProfile(),
                MorphologyData = new MorphologyVisualizationData(),
                VFXInstances = new Dictionary<CannabisTraitCategory, string>(),
                TraitHistory = new List<GeneticExpressionSnapshot>()
            };
            
            // Initialize genetic profile
            InitializeGeneticProfile(morphologyInstance);
            
            // Create VFX components for trait visualization
            CreateTraitVFXComponents(morphologyInstance);
            
            // Apply strain-specific genetics
            if (strainData != null)
            {
                ApplyStrainGeneticTraits(morphologyInstance, strainData);
            }
            
            _activeMorphologies[instanceId] = morphologyInstance;
            _morphologyUpdateQueue.Enqueue(instanceId);
            
            LogInfo($"Genetic morphology instance created: {instanceId[..8]} for plant {plantTransform.name}");
            OnMorphologyUpdated?.Invoke(instanceId, morphologyInstance);
            
            return instanceId;
        }
        
        private void InitializeGeneticProfile(PlantMorphologyInstance instance)
        {
            instance.GeneticProfile = new GeneticExpressionProfile
            {
                TraitExpressions = new Dictionary<GeneticTraitType, float>(),
                CannabisTraitExpressions = new Dictionary<CannabisTraitCategory, GeneticExpressionData>(),
                DominantAlleles = new List<string>(),
                RecessiveAlleles = new List<string>(),
                EpigeneticModifiers = new Dictionary<string, float>(),
                EnvironmentalInfluence = 0.2f,
                GeneticStability = 0.8f
            };
            
            // Initialize trait expressions with defaults
            foreach (var traitType in Enum.GetValues(typeof(GeneticTraitType)).Cast<GeneticTraitType>())
            {
                instance.GeneticProfile.TraitExpressions[traitType] = UnityEngine.Random.Range(0.3f, 0.7f);
            }
            
            // Initialize cannabis-specific traits
            foreach (var category in _cannabisTraits.Keys)
            {
                instance.GeneticProfile.CannabisTraitExpressions[category] = new GeneticExpressionData
                {
                    ExpressionLevel = UnityEngine.Random.Range(0.4f, 0.8f),
                    Heritability = UnityEngine.Random.Range(0.6f, 0.9f),
                    EnvironmentalSensitivity = UnityEngine.Random.Range(0.1f, 0.4f),
                    DevelopmentalStage = GeneticDevelopmentalStage.Mature,
                    LastUpdate = Time.time
                };
            }
            
            // Record initial genetic snapshot
            RecordGeneticSnapshot(instance, "Initial genetic expression profile");
        }
        
        private void CreateTraitVFXComponents(PlantMorphologyInstance instance)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                foreach (var kvp in _cannabisTraits)
                {
                    var category = kvp.Key;
                    var categoryData = kvp.Value;
                    
                    if (categoryData.RequiresVFXGraph)
                    {
                        // Create VFX instance for this trait category
                        string vfxInstanceId = _vfxTemplateManager.CreateVFXInstance(
                            CannabisVFXType.GeneticTraits,
                            instance.PlantTransform,
                            instance.PlantTransform.GetComponent<MonoBehaviour>()
                        );
                        
                        if (vfxInstanceId != null)
                        {
                            instance.VFXInstances[category] = vfxInstanceId;
                            
                            // Configure VFX for this specific trait category
                            ConfigureTraitCategoryVFX(vfxInstanceId, category, categoryData);
                        }
                    }
                }
            }
            #endif
        }
        
        private void ConfigureTraitCategoryVFX(string vfxInstanceId, CannabisTraitCategory category, CannabisTraitVisualization categoryData)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                // Set category-specific VFX parameters
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TraitCategory", (float)category);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "PrimaryColor", categoryData.PrimaryColor);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "SecondaryColor", categoryData.SecondaryColor);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "EffectIntensity", categoryData.EffectIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TraitExpression", 0.5f); // Start neutral
            }
            #endif
        }
        
        private void ApplyStrainGeneticTraits(PlantMorphologyInstance instance, PlantStrainSO strainData)
        {
            // Apply strain-specific genetic characteristics
            float strainHeightMultiplier = 1.0f; // Would get from strain genetics
            float strainColorVariation = 0.3f; // Would get from strain genetics  
            float strainTrichromeIntensity = 0.7f; // Would get from strain genetics
            
            // Adjust trait expressions based on strain
            if (instance.GeneticProfile.TraitExpressions.ContainsKey(GeneticTraitType.PlantHeight))
            {
                instance.GeneticProfile.TraitExpressions[GeneticTraitType.PlantHeight] *= strainHeightMultiplier;
            }
            
            if (instance.GeneticProfile.TraitExpressions.ContainsKey(GeneticTraitType.LeafColor))
            {
                instance.GeneticProfile.TraitExpressions[GeneticTraitType.LeafColor] = Mathf.Lerp(
                    instance.GeneticProfile.TraitExpressions[GeneticTraitType.LeafColor],
                    strainColorVariation,
                    0.5f
                );
            }
            
            // Apply to cannabis trait categories
            if (instance.GeneticProfile.CannabisTraitExpressions.ContainsKey(CannabisTraitCategory.Cannabinoids))
            {
                instance.GeneticProfile.CannabisTraitExpressions[CannabisTraitCategory.Cannabinoids].ExpressionLevel *= strainTrichromeIntensity;
            }
            
            LogInfo($"Applied strain genetic traits to morphology instance {instance.InstanceId[..8]}");
            RecordGeneticSnapshot(instance, $"Strain traits applied: {strainData.name}");
        }
        
        #endregion
        
        #region Genetic Expression Updates
        
        public void UpdateGeneticExpression(string instanceId, GeneticTraitType traitType, float expressionLevel)
        {
            if (!_activeMorphologies.ContainsKey(instanceId))
            {
                LogError($"Morphology instance not found: {instanceId}");
                return;
            }
            
            var instance = _activeMorphologies[instanceId];
            float previousLevel = instance.GeneticProfile.TraitExpressions.ContainsKey(traitType) ? 
                instance.GeneticProfile.TraitExpressions[traitType] : 0.5f;
            
            // Update trait expression
            instance.GeneticProfile.TraitExpressions[traitType] = Mathf.Clamp01(expressionLevel);
            
            // Create genetic update
            var geneticUpdate = new GeneticExpressionUpdate
            {
                InstanceId = instanceId,
                TraitType = traitType,
                ExpressionLevel = expressionLevel,
                PreviousLevel = previousLevel,
                UpdateTime = Time.time,
                EnvironmentalInfluence = instance.GeneticProfile.EnvironmentalInfluence
            };
            
            _pendingGeneticUpdates.Add(geneticUpdate);
            
            // Record significant changes
            if (Mathf.Abs(expressionLevel - previousLevel) > 0.1f)
            {
                RecordGeneticSnapshot(instance, $"{traitType} expression changed: {previousLevel:F2} → {expressionLevel:F2}");
            }
            
            OnTraitExpressed?.Invoke(instanceId, traitType, expressionLevel);
        }
        
        public void UpdateCannabisTraitCategory(string instanceId, CannabisTraitCategory category, GeneticExpressionData expressionData)
        {
            if (!_activeMorphologies.ContainsKey(instanceId))
            {
                LogError($"Morphology instance not found: {instanceId}");
                return;
            }
            
            var instance = _activeMorphologies[instanceId];
            
            // Update cannabis trait expression
            instance.GeneticProfile.CannabisTraitExpressions[category] = expressionData;
            expressionData.LastUpdate = Time.time;
            
            // Queue for visual update
            var categoryUpdate = new CannabisTraitUpdate
            {
                InstanceId = instanceId,
                Category = category,
                ExpressionData = expressionData,
                UpdateTime = Time.time
            };
            
            OnCannabisTraitChanged?.Invoke(instanceId, category, expressionData);
            
            // Apply immediate visual changes for critical traits
            ApplyImmediateTraitVisualization(instance, category, expressionData);
        }
        
        private void RecordGeneticSnapshot(PlantMorphologyInstance instance, string notes)
        {
            var snapshot = new GeneticExpressionSnapshot
            {
                Timestamp = Time.time,
                TraitExpressions = new Dictionary<GeneticTraitType, float>(instance.GeneticProfile.TraitExpressions),
                CannabisTraitExpressions = new Dictionary<CannabisTraitCategory, GeneticExpressionData>(instance.GeneticProfile.CannabisTraitExpressions),
                EnvironmentalInfluence = instance.GeneticProfile.EnvironmentalInfluence,
                Notes = notes
            };
            
            instance.TraitHistory.Add(snapshot);
            
            // Limit history size for performance
            if (instance.TraitHistory.Count > 50)
            {
                instance.TraitHistory.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Morphology Processing
        
        private void ProcessMorphologyUpdates()
        {
            _morphologiesProcessedThisFrame = 0;
            
            // Process pending genetic updates
            ProcessPendingGeneticUpdates();
            
            // Update morphology instances in queue
            while (_morphologyUpdateQueue.Count > 0 && _morphologiesProcessedThisFrame < _maxConcurrentMorphologies)
            {
                string instanceId = _morphologyUpdateQueue.Dequeue();
                
                if (_activeMorphologies.ContainsKey(instanceId))
                {
                    UpdateMorphologyInstance(instanceId);
                    _morphologiesProcessedThisFrame++;
                    
                    // Re-queue for next update cycle
                    _morphologyUpdateQueue.Enqueue(instanceId);
                }
            }
        }
        
        private void ProcessPendingGeneticUpdates()
        {
            foreach (var geneticUpdate in _pendingGeneticUpdates)
            {
                ApplyGeneticExpressionUpdate(geneticUpdate);
            }
            
            _pendingGeneticUpdates.Clear();
        }
        
        private void ApplyGeneticExpressionUpdate(GeneticExpressionUpdate geneticUpdate)
        {
            var instance = _activeMorphologies[geneticUpdate.InstanceId];
            var traitVisualization = _traitVisualizations[geneticUpdate.TraitType];
            
            // Apply visualization based on trait type
            switch (traitVisualization.VisualizationMethod)
            {
                case TraitVisualizationMethod.Scale:
                    ApplyScaleVisualization(instance, geneticUpdate, traitVisualization);
                    break;
                    
                case TraitVisualizationMethod.MaterialColor:
                    ApplyColorVisualization(instance, geneticUpdate, traitVisualization);
                    break;
                    
                case TraitVisualizationMethod.VFXIntensity:
                    ApplyVFXIntensityVisualization(instance, geneticUpdate, traitVisualization);
                    break;
                    
                case TraitVisualizationMethod.ParticleCount:
                    ApplyParticleCountVisualization(instance, geneticUpdate, traitVisualization);
                    break;
                    
                case TraitVisualizationMethod.AnimationSpeed:
                    ApplyAnimationSpeedVisualization(instance, geneticUpdate, traitVisualization);
                    break;
                    
                case TraitVisualizationMethod.HealthGlow:
                    ApplyHealthGlowVisualization(instance, geneticUpdate, traitVisualization);
                    break;
            }
        }
        
        private void ApplyScaleVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            float scaleMultiplier = Mathf.Lerp(visualization.VariationRange.x, visualization.VariationRange.y, expressionValue);
            
            Vector3 targetScale = visualization.BaseScale * scaleMultiplier;
            
            if (instance.PlantTransform != null && _enableSizeTraits)
            {
                // Smooth scale transition
                instance.PlantTransform.localScale = Vector3.Lerp(
                    instance.PlantTransform.localScale,
                    targetScale,
                    Time.deltaTime * _expressionSpeed
                );
            }
            
            instance.MorphologyData.CurrentScale = targetScale;
        }
        
        private void ApplyColorVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            if (!_enableColorTraits || visualization.ColorVariations == null || visualization.ColorVariations.Length == 0)
                return;
                
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            
            // Interpolate between color variations
            Color targetColor;
            if (visualization.ColorVariations.Length == 1)
            {
                targetColor = Color.Lerp(visualization.BaseColor, visualization.ColorVariations[0], expressionValue);
            }
            else
            {
                int colorIndex = Mathf.FloorToInt(expressionValue * (visualization.ColorVariations.Length - 1));
                int nextColorIndex = Mathf.Min(colorIndex + 1, visualization.ColorVariations.Length - 1);
                float t = (expressionValue * (visualization.ColorVariations.Length - 1)) - colorIndex;
                
                targetColor = Color.Lerp(visualization.ColorVariations[colorIndex], visualization.ColorVariations[nextColorIndex], t);
            }
            
            // Apply color to materials (simplified - would target specific materials)
            var renderers = instance.PlantTransform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime * _expressionSpeed);
                }
            }
            
            instance.MorphologyData.CurrentColor = targetColor;
        }
        
        private void ApplyVFXIntensityVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            #if UNITY_VFX_GRAPH
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            float intensity = Mathf.Lerp(visualization.VariationRange.x, visualization.VariationRange.y, expressionValue);
            
            // Apply to appropriate VFX instances
            foreach (var vfxInstanceId in instance.VFXInstances.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "TraitIntensity", intensity);
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ExpressionLevel", expressionValue);
                }
            }
            #endif
        }
        
        private void ApplyParticleCountVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            #if UNITY_VFX_GRAPH
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            int particleCount = Mathf.RoundToInt(Mathf.Lerp(visualization.VariationRange.x, visualization.VariationRange.y, expressionValue));
            
            foreach (var vfxInstanceId in instance.VFXInstances.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ParticleCount", particleCount);
                }
            }
            #endif
        }
        
        private void ApplyAnimationSpeedVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            float animationSpeed = Mathf.Lerp(visualization.VariationRange.x, visualization.VariationRange.y, expressionValue);
            
            // Apply to animator components
            var animators = instance.PlantTransform.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                animator.speed = animationSpeed;
            }
            
            #if UNITY_VFX_GRAPH
            foreach (var vfxInstanceId in instance.VFXInstances.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "AnimationSpeed", animationSpeed);
                }
            }
            #endif
        }
        
        private void ApplyHealthGlowVisualization(PlantMorphologyInstance instance, GeneticExpressionUpdate update, TraitVisualizationData visualization)
        {
            #if UNITY_VFX_GRAPH
            float expressionValue = visualization.ExpressionCurve.Evaluate(update.ExpressionLevel);
            float glowIntensity = Mathf.Lerp(visualization.VariationRange.x, visualization.VariationRange.y, expressionValue);
            
            foreach (var vfxInstanceId in instance.VFXInstances.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "HealthGlow", glowIntensity);
                    _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "GlowColor", visualization.BaseColor);
                }
            }
            #endif
        }
        
        private void ApplyImmediateTraitVisualization(PlantMorphologyInstance instance, CannabisTraitCategory category, GeneticExpressionData expressionData)
        {
            var categoryVisualization = _cannabisTraits[category];
            
            #if UNITY_VFX_GRAPH
            if (instance.VFXInstances.ContainsKey(category))
            {
                string vfxInstanceId = instance.VFXInstances[category];
                
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ExpressionLevel", expressionData.ExpressionLevel);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "Heritability", expressionData.Heritability);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "EnvironmentalSensitivity", expressionData.EnvironmentalSensitivity);
                
                // Category-specific visualizations
                switch (category)
                {
                    case CannabisTraitCategory.Cannabinoids:
                        _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "CannabinoidDensity", expressionData.ExpressionLevel);
                        break;
                        
                    case CannabisTraitCategory.Terpenes:
                        _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "TerpeneAroma", expressionData.ExpressionLevel);
                        break;
                        
                    case CannabisTraitCategory.Morphology:
                        _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "MorphologyVariation", expressionData.ExpressionLevel);
                        break;
                }
            }
            #endif
        }
        
        private void UpdateMorphologyInstance(string instanceId)
        {
            var instance = _activeMorphologies[instanceId];
            
            // Skip if too far away (LOD)
            if (_enableLODMorphology && IsMorphologyInstanceCulled(instance))
            {
                SetInstanceVisualizationActive(instance, false);
                return;
            }
            
            SetInstanceVisualizationActive(instance, true);
            
            // Update continuous genetic expressions
            UpdateContinuousGeneticExpressions(instance);
            
            // Update environmental influence
            UpdateEnvironmentalInfluence(instance);
            
            // Update developmental stage effects
            UpdateDevelopmentalStageEffects(instance);
            
            instance.LastUpdateTime = Time.time;
        }
        
        private bool IsMorphologyInstanceCulled(PlantMorphologyInstance instance)
        {
            if (Camera.main == null || instance.PlantTransform == null)
                return false;
                
            float distance = Vector3.Distance(Camera.main.transform.position, instance.PlantTransform.position);
            return distance > _morphologyCullingDistance;
        }
        
        private void SetInstanceVisualizationActive(PlantMorphologyInstance instance, bool active)
        {
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXInstances.Values)
            {
                if (vfxInstanceId != null)
                {
                    var vfxInstance = _vfxTemplateManager?.GetVFXInstance(vfxInstanceId);
                    if (vfxInstance?.GameObject != null)
                    {
                        vfxInstance.GameObject.SetActive(active);
                    }
                }
            }
            #endif
        }
        
        private void UpdateContinuousGeneticExpressions(PlantMorphologyInstance instance)
        {
            // Simulate continuous genetic expression changes
            float deltaTime = Time.time - instance.LastUpdateTime;
            
            foreach (var kvp in instance.GeneticProfile.TraitExpressions.ToList())
            {
                var traitType = kvp.Key;
                var currentExpression = kvp.Value;
                
                // Add small random variations
                float variation = UnityEngine.Random.Range(-0.01f, 0.01f) * deltaTime;
                float newExpression = Mathf.Clamp01(currentExpression + variation);
                
                if (Mathf.Abs(newExpression - currentExpression) > 0.005f)
                {
                    instance.GeneticProfile.TraitExpressions[traitType] = newExpression;
                    OnTraitExpressed?.Invoke(instance.InstanceId, traitType, newExpression);
                }
            }
        }
        
        private void UpdateEnvironmentalInfluence(PlantMorphologyInstance instance)
        {
            // Simulate environmental influence on genetic expression
            float environmentalStress = UnityEngine.Random.Range(0f, 0.3f); // Would get from actual environment
            instance.GeneticProfile.EnvironmentalInfluence = Mathf.Lerp(
                instance.GeneticProfile.EnvironmentalInfluence,
                environmentalStress,
                Time.deltaTime * 0.1f
            );
        }
        
        private void UpdateDevelopmentalStageEffects(PlantMorphologyInstance instance)
        {
            // Update developmental stage influences on trait expression
            foreach (var kvp in instance.GeneticProfile.CannabisTraitExpressions.ToList())
            {
                var category = kvp.Key;
                var expressionData = kvp.Value;
                
                // Developmental modulation
                float developmentalModifier = GetDevelopmentalModifier(expressionData.DevelopmentalStage, category);
                expressionData.ExpressionLevel *= developmentalModifier;
                
                instance.GeneticProfile.CannabisTraitExpressions[category] = expressionData;
            }
        }
        
        private float GetDevelopmentalModifier(GeneticDevelopmentalStage stage, CannabisTraitCategory category)
        {
            return (stage, category) switch
            {
                (GeneticDevelopmentalStage.Seedling, CannabisTraitCategory.Morphology) => 1.2f,
                (GeneticDevelopmentalStage.Vegetative, CannabisTraitCategory.GrowthPattern) => 1.3f,
                (GeneticDevelopmentalStage.Flowering, CannabisTraitCategory.Cannabinoids) => 1.5f,
                (GeneticDevelopmentalStage.Flowering, CannabisTraitCategory.Terpenes) => 1.4f,
                (GeneticDevelopmentalStage.Mature, _) => 1.0f,
                _ => 1.0f
            };
        }
        
        #endregion
        
        #region Continuous Processing
        
        private IEnumerator ContinuousMorphologyProcessing()
        {
            while (_enableRealtimeGenetics)
            {
                yield return new WaitForSeconds(_morphologyUpdateInterval);
                
                // Process all active morphology instances
                foreach (var instance in _activeMorphologies.Values)
                {
                    ProcessInstanceGeneticDrift(instance);
                }
            }
        }
        
        private void ProcessInstanceGeneticDrift(PlantMorphologyInstance instance)
        {
            // Simulate natural genetic expression drift over time
            float driftRate = 0.01f * Time.deltaTime;
            
            foreach (var traitType in instance.GeneticProfile.TraitExpressions.Keys.ToList())
            {
                float currentExpression = instance.GeneticProfile.TraitExpressions[traitType];
                float drift = UnityEngine.Random.Range(-driftRate, driftRate);
                
                float newExpression = Mathf.Clamp01(currentExpression + drift);
                instance.GeneticProfile.TraitExpressions[traitType] = newExpression;
            }
        }
        
        private void UpdateGeneticExpressions()
        {
            // Update global genetic expression trends
            foreach (var instance in _activeMorphologies.Values)
            {
                UpdateInstanceGeneticComplexity(instance);
            }
        }
        
        private void UpdateInstanceGeneticComplexity(PlantMorphologyInstance instance)
        {
            // Calculate genetic complexity score
            float complexity = 0f;
            int traitCount = instance.GeneticProfile.TraitExpressions.Count;
            
            foreach (var expression in instance.GeneticProfile.TraitExpressions.Values)
            {
                complexity += Mathf.Abs(expression - 0.5f); // Deviation from neutral
            }
            
            instance.GeneticProfile.GeneticComplexity = complexity / traitCount;
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveMorphologies = _activeMorphologies.Count;
            _performanceMetrics.MorphologyUpdatesPerSecond = _morphologiesProcessedThisFrame / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
            
            // Calculate average genetic complexity
            if (_activeMorphologies.Count > 0)
            {
                float totalComplexity = _activeMorphologies.Values.Sum(m => m.GeneticProfile.GeneticComplexity);
                _performanceMetrics.GeneticComplexity = totalComplexity / _activeMorphologies.Count;
            }
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnMorphologyPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public PlantMorphologyInstance GetMorphologyInstance(string instanceId)
        {
            return _activeMorphologies.ContainsKey(instanceId) ? _activeMorphologies[instanceId] : null;
        }
        
        public List<PlantMorphologyInstance> GetAllMorphologyInstances()
        {
            return new List<PlantMorphologyInstance>(_activeMorphologies.Values);
        }
        
        public void SetTraitExpression(string instanceId, GeneticTraitType traitType, float expressionLevel)
        {
            UpdateGeneticExpression(instanceId, traitType, expressionLevel);
        }
        
        public float GetTraitExpression(string instanceId, GeneticTraitType traitType)
        {
            if (_activeMorphologies.ContainsKey(instanceId))
            {
                var instance = _activeMorphologies[instanceId];
                return instance.GeneticProfile.TraitExpressions.ContainsKey(traitType) ? 
                    instance.GeneticProfile.TraitExpressions[traitType] : 0f;
            }
            return 0f;
        }
        
        public void DestroyMorphologyInstance(string instanceId)
        {
            if (_activeMorphologies.ContainsKey(instanceId))
            {
                var instance = _activeMorphologies[instanceId];
                CleanupMorphologyInstance(instance);
                _activeMorphologies.Remove(instanceId);
                LogInfo($"Genetic morphology instance destroyed: {instanceId[..8]}");
            }
        }
        
        private void CleanupMorphologyInstance(PlantMorphologyInstance instance)
        {
            // Cleanup VFX instances
            foreach (string vfxInstanceId in instance.VFXInstances.Values)
            {
                _vfxTemplateManager?.DestroyVFXInstance(vfxInstanceId);
            }
        }
        
        public GeneticMorphologyReport GetMorphologyReport()
        {
            return new GeneticMorphologyReport
            {
                ActiveMorphologies = _activeMorphologies.Count,
                AverageGeneticComplexity = _performanceMetrics.GeneticComplexity,
                TotalTraitExpressions = _activeMorphologies.Values.Sum(m => m.GeneticProfile.TraitExpressions.Count),
                MorphologyCategories = _cannabisTraits.Keys.ToList(),
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Show Morphology Status")]
        public void ShowMorphologyStatus()
        {
            LogInfo("=== GENETIC MORPHOLOGY STATUS ===");
            LogInfo($"Active Morphology Instances: {_activeMorphologies.Count}");
            LogInfo($"Average Genetic Complexity: {_performanceMetrics.GeneticComplexity:F2}");
            LogInfo($"Cannabis Trait Categories: {_cannabisTraits.Count}");
            LogInfo($"Genetic Trait Types: {_traitVisualizations.Count}");
        }
        
        [ContextMenu("Trigger Genetic Variation")]
        public void TriggerGeneticVariation()
        {
            foreach (var instance in _activeMorphologies.Values)
            {
                foreach (var traitType in instance.GeneticProfile.TraitExpressions.Keys.ToList())
                {
                    float variation = UnityEngine.Random.Range(0.1f, 0.9f);
                    UpdateGeneticExpression(instance.InstanceId, traitType, variation);
                }
            }
            
            LogInfo("Genetic variation triggered for all morphology instances");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum GeneticTraitType
    {
        PlantHeight = 0,
        LeafColor = 1,
        LeafShape = 2,
        BudDensity = 3,
        BudStructure = 4,
        TrichromeProduction = 5,
        FloweringTime = 6,
        THCContent = 7,
        CBDContent = 8,
        TerpeneProfile = 9,
        DiseaseResistance = 10,
        HeatTolerance = 11,
        DroughtResistance = 12,
        YieldPotential = 13,
        StemStrength = 14
    }
    
    [System.Serializable]
    public enum CannabisTraitCategory
    {
        Morphology = 0,
        Cannabinoids = 1,
        Terpenes = 2,
        GrowthPattern = 3,
        EnvironmentalResponse = 4
    }
    
    [System.Serializable]
    public enum TraitVisualizationMethod
    {
        Scale,
        MaterialColor,
        VFXIntensity,
        ParticleCount,
        ParticleColor,
        AnimationSpeed,
        HealthGlow,
        EnvironmentalGlow
    }
    
    [System.Serializable]
    public enum GeneticDevelopmentalStage
    {
        Embryonic = 0,
        Seedling = 1,
        Juvenile = 2,
        Vegetative = 3,
        PreFlowering = 4,
        Flowering = 5,
        Mature = 6
    }
    
    [System.Serializable]
    public class PlantMorphologyInstance
    {
        public string InstanceId;
        public Transform PlantTransform;
        public PlantStrainSO StrainData;
        public bool IsActive;
        public float CreationTime;
        public float LastUpdateTime;
        public GeneticExpressionProfile GeneticProfile;
        public MorphologyVisualizationData MorphologyData;
        public Dictionary<CannabisTraitCategory, string> VFXInstances;
        public List<GeneticExpressionSnapshot> TraitHistory;
    }
    
    [System.Serializable]
    public class GeneticExpressionProfile
    {
        public Dictionary<GeneticTraitType, float> TraitExpressions;
        public Dictionary<CannabisTraitCategory, GeneticExpressionData> CannabisTraitExpressions;
        public List<string> DominantAlleles;
        public List<string> RecessiveAlleles;
        public Dictionary<string, float> EpigeneticModifiers;
        public float EnvironmentalInfluence;
        public float GeneticStability;
        public float GeneticComplexity;
    }
    
    [System.Serializable]
    public class GeneticExpressionData
    {
        public float ExpressionLevel;
        public float Heritability;
        public float EnvironmentalSensitivity;
        public GeneticDevelopmentalStage DevelopmentalStage;
        public float LastUpdate;
    }
    
    [System.Serializable]
    public class TraitVisualizationData
    {
        public GeneticTraitType TraitType;
        public TraitVisualizationMethod VisualizationMethod;
        public Vector3 BaseScale;
        public Color BaseColor;
        public Color[] ColorVariations;
        public Vector2 VariationRange;
        public AnimationCurve ExpressionCurve;
        public CannabisVFXType VFXType;
        public string Description;
    }
    
    [System.Serializable]
    public class CannabisTraitVisualization
    {
        public CannabisTraitCategory Category;
        public string CategoryName;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public float EffectIntensity;
        public TraitVisualizationMethod VisualizationMethod;
        public bool RequiresVFXGraph;
        public int TraitCount;
    }
    
    [System.Serializable]
    public class MorphologyVisualizationData
    {
        public Vector3 CurrentScale;
        public Color CurrentColor;
        public float CurrentIntensity;
        public int CurrentParticleCount;
        public float CurrentAnimationSpeed;
        public float VisualizationVersion;
    }
    
    [System.Serializable]
    public class GeneticExpressionUpdate
    {
        public string InstanceId;
        public GeneticTraitType TraitType;
        public float ExpressionLevel;
        public float PreviousLevel;
        public float UpdateTime;
        public float EnvironmentalInfluence;
    }
    
    [System.Serializable]
    public class CannabisTraitUpdate
    {
        public string InstanceId;
        public CannabisTraitCategory Category;
        public GeneticExpressionData ExpressionData;
        public float UpdateTime;
    }
    
    [System.Serializable]
    public class GeneticExpressionSnapshot
    {
        public float Timestamp;
        public Dictionary<GeneticTraitType, float> TraitExpressions;
        public Dictionary<CannabisTraitCategory, GeneticExpressionData> CannabisTraitExpressions;
        public float EnvironmentalInfluence;
        public string Notes;
    }
    
    [System.Serializable]
    public class MorphologyPerformanceMetrics
    {
        public int ActiveMorphologies;
        public float MorphologyUpdatesPerSecond;
        public float AverageExpressionTime;
        public float GeneticComplexity;
        public float TargetFrameRate;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class GeneticMorphologyReport
    {
        public int ActiveMorphologies;
        public float AverageGeneticComplexity;
        public int TotalTraitExpressions;
        public List<CannabisTraitCategory> MorphologyCategories;
        public MorphologyPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}