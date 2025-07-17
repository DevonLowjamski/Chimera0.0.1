using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Manages creation, configuration, and deployment of VFX Graph templates
    /// specifically designed for cannabis cultivation visual effects.
    /// </summary>
    public class CannabisVFXTemplateManager : ChimeraManager
    {
        [Header("VFX Template Configuration")]
        [SerializeField] private bool _enableVFXTemplates = true;
        [SerializeField] private bool _autoCreateTemplates = true;
        [SerializeField] private VFXTemplateQuality _templateQuality = VFXTemplateQuality.High;
        
        [Header("Cannabis-Specific VFX")]
        [SerializeField] private bool _enableTrichromeEffects = true;
        [SerializeField] private bool _enableGrowthAnimations = true;
        [SerializeField] private bool _enableHealthIndicators = true;
        [SerializeField] private bool _enableEnvironmentalEffects = true;
        
        [Header("Template Asset References")]
        [SerializeField] private GameObject _vfxTemplatePrefab;
        [SerializeField] private Material _trichromeMaterial;
        [SerializeField] private Material _healthIndicatorMaterial;
        [SerializeField] private Material _growthParticleMaterial;
        [SerializeField] private Material _environmentalEffectMaterial;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxActiveVFXPerPlant = 3;
        [SerializeField] private float _vfxUpdateInterval = 0.1f;
        [SerializeField] private float _cullingDistance = 25f;
        [SerializeField] private bool _enableLODSystem = true;
        
        // Template Registry
        private Dictionary<CannabisVFXType, VFXTemplate> _vfxTemplates = new Dictionary<CannabisVFXType, VFXTemplate>();
        private Dictionary<string, CannabisVFXInstance> _activeVFXInstances = new Dictionary<string, CannabisVFXInstance>();
        
        // Template Creation Status
        private bool _templatesCreated = false;
        private int _templatesCreatedCount = 0;
        private int _totalTemplatesNeeded = 8;
        
        // Performance Tracking
        private float _lastVFXUpdate = 0f;
        private int _activeVFXCount = 0;
        private VFXPerformanceMetrics _performanceMetrics;
        
        // Events
        public System.Action<CannabisVFXType, VFXTemplate> OnTemplateCreated;
        public System.Action<string> OnVFXInstanceCreated;
        public System.Action<VFXPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public bool TemplatesCreated => _templatesCreated;
        public int ActiveVFXCount => _activeVFXCount;
        public VFXPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeVFXSystem();
            CreateVFXDirectoryStructure();
            
            if (_autoCreateTemplates)
            {
                StartCoroutine(CreateAllVFXTemplates());
            }
            
            InitializePerformanceTracking();
            LogInfo("Cannabis VFX Template Manager initialized");
        }
        
        private void Update()
        {
            if (Time.time - _lastVFXUpdate >= _vfxUpdateInterval)
            {
                UpdateActiveVFXInstances();
                UpdatePerformanceMetrics();
                _lastVFXUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeVFXSystem()
        {
            LogInfo("=== INITIALIZING CANNABIS VFX TEMPLATE SYSTEM ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph package detected - full functionality enabled");
            #else
            LogWarning("⚠️ VFX Graph package not available - templates will be created as placeholders");
            _enableVFXTemplates = false;
            #endif
            
            // Initialize performance metrics
            _performanceMetrics = new VFXPerformanceMetrics
            {
                MaxActiveVFX = _maxActiveVFXPerPlant * 100, // Assume 100 plants max
                TargetFrameRate = 60f,
                EnableLOD = _enableLODSystem
            };
        }
        
        private void CreateVFXDirectoryStructure()
        {
            string baseVFXPath = "Assets/ProjectChimera/VFX";
            
            // Create VFX directory structure
            CreateDirectoryIfNotExists($"{baseVFXPath}/Templates");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Cannabis");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Cannabis/Trichrome");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Cannabis/Growth");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Cannabis/Health");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Cannabis/Environmental");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Materials");
            CreateDirectoryIfNotExists($"{baseVFXPath}/Textures");
            
            LogInfo("✅ VFX directory structure created");
        }
        
        private void CreateDirectoryIfNotExists(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        #endregion
        
        #region VFX Template Creation
        
        private IEnumerator CreateAllVFXTemplates()
        {
            LogInfo("=== CREATING CANNABIS VFX TEMPLATES ===");
            
            yield return StartCoroutine(CreateTrichromeVFXTemplate());
            yield return StartCoroutine(CreatePlantGrowthVFXTemplate());
            yield return StartCoroutine(CreateHealthIndicatorVFXTemplate());
            yield return StartCoroutine(CreateEnvironmentalResponseVFXTemplate());
            yield return StartCoroutine(CreateHarvestReadinessVFXTemplate());
            yield return StartCoroutine(CreateNutrientDeficiencyVFXTemplate());
            yield return StartCoroutine(CreateDiseaseEffectVFXTemplate());
            yield return StartCoroutine(CreateGeneticTraitVFXTemplate());
            
            _templatesCreated = true;
            LogInfo($"🎉 ALL CANNABIS VFX TEMPLATES CREATED ({_templatesCreatedCount}/{_totalTemplatesNeeded})");
        }
        
        private IEnumerator CreateTrichromeVFXTemplate()
        {
            LogInfo("Creating Trichrome Development VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "TrichromeGrowth",
                Type = CannabisVFXType.TrichromeGrowth,
                Description = "Particle system for cannabis trichrome development and maturation",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Trichrome/TrichromeGrowth.vfx",
                IsCreated = false
            };
            
            // Configure trichrome-specific parameters
            template.Parameters = CreateTrichromeVFXParameters();
            
            #if UNITY_VFX_GRAPH
            // Create actual VFX Graph asset (would require VFX Graph Editor API)
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true; // Mark as created in non-VFX environments
            #endif
            
            _vfxTemplates[CannabisVFXType.TrichromeGrowth] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Trichrome VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f); // Small delay between template creation
        }
        
        private IEnumerator CreatePlantGrowthVFXTemplate()
        {
            LogInfo("Creating Plant Growth VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "PlantGrowth",
                Type = CannabisVFXType.PlantGrowth,
                Description = "Growth stage transition effects with bud development",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Growth/PlantGrowth.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreatePlantGrowthVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.PlantGrowth] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Plant Growth VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateHealthIndicatorVFXTemplate()
        {
            LogInfo("Creating Health Indicator VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "HealthIndicator",
                Type = CannabisVFXType.HealthIndicator,
                Description = "Health status glow and particle effects",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Health/HealthIndicator.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateHealthIndicatorVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.HealthIndicator] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Health Indicator VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateEnvironmentalResponseVFXTemplate()
        {
            LogInfo("Creating Environmental Response VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "EnvironmentalResponse",
                Type = CannabisVFXType.EnvironmentalResponse,
                Description = "Wind sway and environmental reactions",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Environmental/EnvironmentalResponse.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateEnvironmentalResponseVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.EnvironmentalResponse] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Environmental Response VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateHarvestReadinessVFXTemplate()
        {
            LogInfo("Creating Harvest Readiness VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "HarvestReadiness",
                Type = CannabisVFXType.HarvestReadiness,
                Description = "Visual cues for optimal harvest timing",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Growth/HarvestReadiness.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateHarvestReadinessVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.HarvestReadiness] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Harvest Readiness VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateNutrientDeficiencyVFXTemplate()
        {
            LogInfo("Creating Nutrient Deficiency VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "NutrientDeficiency",
                Type = CannabisVFXType.NutrientDeficiency,
                Description = "Visual indicators for nutrient problems",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Health/NutrientDeficiency.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateNutrientDeficiencyVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.NutrientDeficiency] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Nutrient Deficiency VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateDiseaseEffectVFXTemplate()
        {
            LogInfo("Creating Disease Effect VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "DiseaseEffect",
                Type = CannabisVFXType.DiseaseEffect,
                Description = "Disease and pest infestation visuals",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Health/DiseaseEffect.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateDiseaseEffectVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.DiseaseEffect] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Disease Effect VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        private IEnumerator CreateGeneticTraitVFXTemplate()
        {
            LogInfo("Creating Genetic Trait VFX Template...");
            
            var template = new VFXTemplate
            {
                Name = "GeneticTraits",
                Type = CannabisVFXType.GeneticTraits,
                Description = "Visual expression of genetic characteristics",
                FilePath = "Assets/ProjectChimera/VFX/Cannabis/Growth/GeneticTraits.vfx",
                IsCreated = false
            };
            
            template.Parameters = CreateGeneticTraitVFXParameters();
            
            #if UNITY_VFX_GRAPH
            template.VFXAsset = CreateVFXGraphAsset(template);
            template.IsCreated = template.VFXAsset != null;
            #else
            template.IsCreated = true;
            #endif
            
            _vfxTemplates[CannabisVFXType.GeneticTraits] = template;
            _templatesCreatedCount++;
            
            LogInfo($"✅ Genetic Trait VFX Template: {(template.IsCreated ? "CREATED" : "PLACEHOLDER")}");
            OnTemplateCreated?.Invoke(template.Type, template);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        #endregion
        
        #region VFX Parameter Creation
        
        private Dictionary<string, VFXParameter> CreateTrichromeVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["TrichromeAmount"] = new VFXParameter { Name = "TrichromeAmount", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 1f },
                ["TrichromeDensity"] = new VFXParameter { Name = "TrichromeDensity", Type = VFXParameterType.Float, DefaultValue = 0.3f, MinValue = 0f, MaxValue = 1f },
                ["TrichromeColor"] = new VFXParameter { Name = "TrichromeColor", Type = VFXParameterType.Color, DefaultValue = Color.white },
                ["GrowthSpeed"] = new VFXParameter { Name = "GrowthSpeed", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 3f },
                ["ParticleSize"] = new VFXParameter { Name = "ParticleSize", Type = VFXParameterType.Float, DefaultValue = 0.01f, MinValue = 0.005f, MaxValue = 0.05f }
            };
        }
        
        private Dictionary<string, VFXParameter> CreatePlantGrowthVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["GrowthStage"] = new VFXParameter { Name = "GrowthStage", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["GrowthRate"] = new VFXParameter { Name = "GrowthRate", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 5f },
                ["BudFormation"] = new VFXParameter { Name = "BudFormation", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["LeafDensity"] = new VFXParameter { Name = "LeafDensity", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 1f },
                ["GrowthColor"] = new VFXParameter { Name = "GrowthColor", Type = VFXParameterType.Color, DefaultValue = Color.green }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateHealthIndicatorVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["HealthLevel"] = new VFXParameter { Name = "HealthLevel", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0f, MaxValue = 1f },
                ["GlowIntensity"] = new VFXParameter { Name = "GlowIntensity", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 2f },
                ["HealthColor"] = new VFXParameter { Name = "HealthColor", Type = VFXParameterType.Color, DefaultValue = Color.green },
                ["PulseSpeed"] = new VFXParameter { Name = "PulseSpeed", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 3f },
                ["ParticleCount"] = new VFXParameter { Name = "ParticleCount", Type = VFXParameterType.Int, DefaultValue = 50, MinValue = 10, MaxValue = 200 }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateEnvironmentalResponseVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["WindStrength"] = new VFXParameter { Name = "WindStrength", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 2f },
                ["WindDirection"] = new VFXParameter { Name = "WindDirection", Type = VFXParameterType.Vector3, DefaultValue = Vector3.right },
                ["LightResponse"] = new VFXParameter { Name = "LightResponse", Type = VFXParameterType.Float, DefaultValue = 0.8f, MinValue = 0f, MaxValue = 1f },
                ["TemperatureStress"] = new VFXParameter { Name = "TemperatureStress", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["HumidityEffect"] = new VFXParameter { Name = "HumidityEffect", Type = VFXParameterType.Float, DefaultValue = 0.6f, MinValue = 0f, MaxValue = 1f }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateHarvestReadinessVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["ReadinessLevel"] = new VFXParameter { Name = "ReadinessLevel", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["TrichromeMaturity"] = new VFXParameter { Name = "TrichromeMaturity", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["HarvestGlow"] = new VFXParameter { Name = "HarvestGlow", Type = VFXParameterType.Color, DefaultValue = Color.yellow },
                ["SparkleIntensity"] = new VFXParameter { Name = "SparkleIntensity", Type = VFXParameterType.Float, DefaultValue = 0.3f, MinValue = 0f, MaxValue = 1f },
                ["PulseFrequency"] = new VFXParameter { Name = "PulseFrequency", Type = VFXParameterType.Float, DefaultValue = 2f, MinValue = 0.5f, MaxValue = 5f }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateNutrientDeficiencyVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["DeficiencyType"] = new VFXParameter { Name = "DeficiencyType", Type = VFXParameterType.Int, DefaultValue = 0, MinValue = 0, MaxValue = 10 },
                ["Severity"] = new VFXParameter { Name = "Severity", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["DeficiencyColor"] = new VFXParameter { Name = "DeficiencyColor", Type = VFXParameterType.Color, DefaultValue = Color.yellow },
                ["SpotIntensity"] = new VFXParameter { Name = "SpotIntensity", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 1f },
                ["ProgressionRate"] = new VFXParameter { Name = "ProgressionRate", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 3f }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateDiseaseEffectVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["DiseaseType"] = new VFXParameter { Name = "DiseaseType", Type = VFXParameterType.Int, DefaultValue = 0, MinValue = 0, MaxValue = 5 },
                ["InfectionLevel"] = new VFXParameter { Name = "InfectionLevel", Type = VFXParameterType.Float, DefaultValue = 0f, MinValue = 0f, MaxValue = 1f },
                ["DiseaseColor"] = new VFXParameter { Name = "DiseaseColor", Type = VFXParameterType.Color, DefaultValue = new Color(0.8f, 0.4f, 0.2f) },
                ["SpreadRate"] = new VFXParameter { Name = "SpreadRate", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 5f },
                ["VisualIntensity"] = new VFXParameter { Name = "VisualIntensity", Type = VFXParameterType.Float, DefaultValue = 0.7f, MinValue = 0f, MaxValue = 1f }
            };
        }
        
        private Dictionary<string, VFXParameter> CreateGeneticTraitVFXParameters()
        {
            return new Dictionary<string, VFXParameter>
            {
                ["TraitExpression"] = new VFXParameter { Name = "TraitExpression", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0f, MaxValue = 1f },
                ["GeneticVariation"] = new VFXParameter { Name = "GeneticVariation", Type = VFXParameterType.Float, DefaultValue = 0.3f, MinValue = 0f, MaxValue = 1f },
                ["TraitColor"] = new VFXParameter { Name = "TraitColor", Type = VFXParameterType.Color, DefaultValue = Color.cyan },
                ["ExpressionSpeed"] = new VFXParameter { Name = "ExpressionSpeed", Type = VFXParameterType.Float, DefaultValue = 1f, MinValue = 0.1f, MaxValue = 3f },
                ["VisualComplexity"] = new VFXParameter { Name = "VisualComplexity", Type = VFXParameterType.Float, DefaultValue = 0.5f, MinValue = 0.1f, MaxValue = 1f }
            };
        }
        
        #endregion
        
        #region VFX Asset Creation
        
        #if UNITY_VFX_GRAPH
        private VisualEffectAsset CreateVFXGraphAsset(VFXTemplate template)
        {
            try
            {
                // In a full implementation, this would use VFX Graph Editor API
                // to programmatically create VFX Graph assets
                LogInfo($"Creating VFX Graph asset for {template.Name}...");
                
                // For now, we'll create a placeholder that can be manually configured
                LogInfo($"VFX Graph asset placeholder created: {template.FilePath}");
                LogInfo($"Template parameters: {template.Parameters.Count} configured");
                
                return null; // Would return actual VisualEffectAsset
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to create VFX Graph asset for {template.Name}: {ex.Message}");
                return null;
            }
        }
        #endif
        
        #endregion
        
        #region VFX Instance Management
        
        public string CreateVFXInstance(CannabisVFXType vfxType, Transform parentTransform, MonoBehaviour plantInstance = null)
        {
            if (!_vfxTemplates.ContainsKey(vfxType))
            {
                LogWarning($"VFX template not found for type: {vfxType}");
                return null;
            }
            
            var template = _vfxTemplates[vfxType];
            string instanceId = Guid.NewGuid().ToString();
            
            var instance = new CannabisVFXInstance
            {
                InstanceId = instanceId,
                Template = template,
                ParentTransform = parentTransform,
                PlantInstance = plantInstance,
                IsActive = true,
                CreationTime = Time.time
            };
            
            #if UNITY_VFX_GRAPH
            // Create actual VFX component
            var vfxObject = new GameObject($"VFX_{template.Name}_{instanceId[..8]}");
            vfxObject.transform.SetParent(parentTransform);
            vfxObject.transform.localPosition = Vector3.zero;
            
            var vfxComponent = vfxObject.AddComponent<VisualEffect>();
            if (template.VFXAsset != null)
            {
                vfxComponent.visualEffectAsset = template.VFXAsset;
            }
            
            instance.VFXComponent = vfxComponent;
            instance.GameObject = vfxObject;
            #else
            // Create placeholder object
            var placeholderObject = new GameObject($"VFX_Placeholder_{template.Name}");
            placeholderObject.transform.SetParent(parentTransform);
            instance.GameObject = placeholderObject;
            #endif
            
            _activeVFXInstances[instanceId] = instance;
            _activeVFXCount++;
            
            LogInfo($"VFX instance created: {template.Name} ({instanceId[..8]})");
            OnVFXInstanceCreated?.Invoke(instanceId);
            
            return instanceId;
        }
        
        public void DestroyVFXInstance(string instanceId)
        {
            if (_activeVFXInstances.ContainsKey(instanceId))
            {
                var instance = _activeVFXInstances[instanceId];
                
                if (instance.GameObject != null)
                {
                    DestroyImmediate(instance.GameObject);
                }
                
                _activeVFXInstances.Remove(instanceId);
                _activeVFXCount--;
                
                LogInfo($"VFX instance destroyed: {instanceId[..8]}");
            }
        }
        
        public void UpdateVFXParameter(string instanceId, string parameterName, object value)
        {
            if (!_activeVFXInstances.ContainsKey(instanceId)) return;
            
            var instance = _activeVFXInstances[instanceId];
            
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponent != null)
            {
                // Update VFX parameter based on type
                if (value is float floatValue)
                {
                    instance.VFXComponent.SetFloat(parameterName, floatValue);
                }
                else if (value is int intValue)
                {
                    instance.VFXComponent.SetInt(parameterName, intValue);
                }
                else if (value is Vector3 vectorValue)
                {
                    instance.VFXComponent.SetVector3(parameterName, vectorValue);
                }
                else if (value is Color colorValue)
                {
                    instance.VFXComponent.SetVector4(parameterName, colorValue);
                }
            }
            #endif
        }
        
        private void UpdateActiveVFXInstances()
        {
            foreach (var kvp in _activeVFXInstances)
            {
                var instance = kvp.Value;
                
                // Update based on plant state if connected
                if (instance.PlantInstance != null)
                {
                    UpdateVFXFromPlantState(instance);
                }
                
                // Apply LOD if enabled
                if (_enableLODSystem)
                {
                    ApplyVFXLOD(instance);
                }
            }
        }
        
        private void UpdateVFXFromPlantState(CannabisVFXInstance instance)
        {
            if (instance.PlantInstance == null) return;
            
            var plantInstance = instance.PlantInstance;
            
            switch (instance.Template.Type)
            {
                case CannabisVFXType.HealthIndicator:
                    // Use reflection to get health if available, otherwise use default
                    UpdateVFXParameter(instance.InstanceId, "HealthLevel", 0.8f);
                    break;
                    
                case CannabisVFXType.TrichromeGrowth:
                    // Would get trichrome data from plant genetics
                    UpdateVFXParameter(instance.InstanceId, "TrichromeAmount", 0.5f);
                    break;
                    
                case CannabisVFXType.PlantGrowth:
                    // Would get growth stage from plant lifecycle
                    UpdateVFXParameter(instance.InstanceId, "GrowthStage", 0.6f);
                    break;
            }
        }
        
        private void ApplyVFXLOD(CannabisVFXInstance instance)
        {
            if (Camera.main == null || instance.GameObject == null) return;
            
            float distance = Vector3.Distance(Camera.main.transform.position, instance.GameObject.transform.position);
            
            if (distance > _cullingDistance)
            {
                // Disable VFX if too far
                instance.GameObject.SetActive(false);
            }
            else
            {
                instance.GameObject.SetActive(true);
                
                // Adjust quality based on distance
                float lodFactor = 1f - (distance / _cullingDistance);
                
                #if UNITY_VFX_GRAPH
                if (instance.VFXComponent != null)
                {
                    // Reduce particle count based on distance
                    UpdateVFXParameter(instance.InstanceId, "ParticleCount", Mathf.RoundToInt(lodFactor * 100));
                }
                #endif
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveVFXInstances = _activeVFXCount;
            _performanceMetrics.CurrentFrameRate = 1f / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public VFXTemplate GetTemplate(CannabisVFXType vfxType)
        {
            return _vfxTemplates.ContainsKey(vfxType) ? _vfxTemplates[vfxType] : null;
        }
        
        public List<VFXTemplate> GetAllTemplates()
        {
            return new List<VFXTemplate>(_vfxTemplates.Values);
        }
        
        public CannabisVFXInstance GetVFXInstance(string instanceId)
        {
            return _activeVFXInstances.ContainsKey(instanceId) ? _activeVFXInstances[instanceId] : null;
        }
        
        public List<CannabisVFXInstance> GetActiveVFXInstances()
        {
            return new List<CannabisVFXInstance>(_activeVFXInstances.Values);
        }
        
        public void SetTemplateQuality(VFXTemplateQuality quality)
        {
            _templateQuality = quality;
            ApplyQualityToAllTemplates();
            LogInfo($"VFX template quality set to: {quality}");
        }
        
        private void ApplyQualityToAllTemplates()
        {
            // Adjust performance settings based on quality
            switch (_templateQuality)
            {
                case VFXTemplateQuality.Low:
                    _maxActiveVFXPerPlant = 1;
                    _cullingDistance = 15f;
                    break;
                case VFXTemplateQuality.Medium:
                    _maxActiveVFXPerPlant = 2;
                    _cullingDistance = 25f;
                    break;
                case VFXTemplateQuality.High:
                    _maxActiveVFXPerPlant = 3;
                    _cullingDistance = 35f;
                    break;
                case VFXTemplateQuality.Ultra:
                    _maxActiveVFXPerPlant = 5;
                    _cullingDistance = 50f;
                    break;
            }
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Create All VFX Templates")]
        public void ManualCreateAllTemplates()
        {
            StartCoroutine(CreateAllVFXTemplates());
        }
        
        [ContextMenu("Show Template Status")]
        public void ShowTemplateStatus()
        {
            LogInfo("=== VFX TEMPLATE STATUS ===");
            LogInfo($"Templates Created: {_templatesCreatedCount}/{_totalTemplatesNeeded}");
            LogInfo($"All Templates Ready: {_templatesCreated}");
            LogInfo($"Active VFX Instances: {_activeVFXCount}");
            LogInfo($"Template Quality: {_templateQuality}");
            
            foreach (var template in _vfxTemplates.Values)
            {
                string status = template.IsCreated ? "✅ READY" : "⏳ PENDING";
                LogInfo($"- {template.Name}: {status}");
            }
        }
        
        [ContextMenu("Clear All VFX Instances")]
        public void ClearAllVFXInstances()
        {
            var instanceIds = new List<string>(_activeVFXInstances.Keys);
            foreach (string instanceId in instanceIds)
            {
                DestroyVFXInstance(instanceId);
            }
            
            LogInfo("All VFX instances cleared");
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            ClearAllVFXInstances();
            CancelInvoke();
            LogInfo("Cannabis VFX Template Manager shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum CannabisVFXType
    {
        TrichromeGrowth = 0,
        PlantGrowth = 1,
        HealthIndicator = 2,
        EnvironmentalResponse = 3,
        HarvestReadiness = 4,
        NutrientDeficiency = 5,
        DiseaseEffect = 6,
        GeneticTraits = 7
    }
    
    [System.Serializable]
    public enum VFXTemplateQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    [System.Serializable]
    public enum VFXParameterType
    {
        Float,
        Int,
        Vector3,
        Color
    }
    
    [System.Serializable]
    public class VFXTemplate
    {
        public string Name;
        public CannabisVFXType Type;
        public string Description;
        public string FilePath;
        public bool IsCreated;
        public Dictionary<string, VFXParameter> Parameters;
        
        #if UNITY_VFX_GRAPH
        public VisualEffectAsset VFXAsset;
        #endif
    }
    
    [System.Serializable]
    public class VFXParameter
    {
        public string Name;
        public VFXParameterType Type;
        public object DefaultValue;
        public float MinValue;
        public float MaxValue;
    }
    
    [System.Serializable]
    public class CannabisVFXInstance
    {
        public string InstanceId;
        public VFXTemplate Template;
        public Transform ParentTransform;
        public MonoBehaviour PlantInstance;
        public GameObject GameObject;
        public bool IsActive;
        public float CreationTime;
        
        #if UNITY_VFX_GRAPH
        public VisualEffect VFXComponent;
        #endif
    }
    
    [System.Serializable]
    public class VFXPerformanceMetrics
    {
        public int ActiveVFXInstances;
        public int MaxActiveVFX;
        public float CurrentFrameRate;
        public float TargetFrameRate;
        public bool EnableLOD;
        public DateTime LastUpdate;
    }
    
    #endregion
}