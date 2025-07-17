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
    /// Advanced genetic trait morphology mapping system for SpeedTree cannabis visualization.
    /// Translates genetic trait data into visual morphology parameters including plant structure,
    /// bud development, trichrome density, coloration, and growth patterns. Provides real-time
    /// genetic expression visualization with scientific accuracy and performance optimization.
    /// </summary>
    public class GeneticMorphologyMappingSystem : ChimeraManager
    {
        [Header("Genetic Morphology Configuration")]
        [SerializeField] private bool _enableGeneticMorphologyMapping = true;
        [SerializeField] private bool _enableRealtimeGeneticVisualization = true;
        [SerializeField] private float _morphologyUpdateFrequency = 1f; // Updates per second
        [SerializeField] private bool _enableGeneticDiversity = true;
        
        [Header("Plant Structure Mapping")]
        [SerializeField] private bool _enableHeightMapping = true;
        [SerializeField] private bool _enableWidthMapping = true;
        [SerializeField] private bool _enableBranchingMapping = true;
        [SerializeField] private bool _enableInternodeMapping = true;
        [SerializeField] private AnimationCurve _heightGeneticCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 2f);
        [SerializeField] private AnimationCurve _widthGeneticCurve = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1.5f);
        
        [Header("Bud Development Mapping")]
        [SerializeField] private bool _enableBudSizeMapping = true;
        [SerializeField] private bool _enableBudDensityMapping = true;
        [SerializeField] private bool _enableBudColorMapping = true;
        [SerializeField] private bool _enableBudShapeMapping = true;
        [SerializeField] private AnimationCurve _budSizeGeneticCurve = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.8f);
        [SerializeField] private AnimationCurve _budDensityGeneticCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1.2f);
        
        [Header("Trichrome Visualization")]
        [SerializeField] private bool _enableTrichromeMapping = true;
        [SerializeField] private bool _enableTrichromeDensityMapping = true;
        [SerializeField] private bool _enableTrichromeColorMapping = true;
        [SerializeField] private bool _enableTrichromeSize = true;
        [SerializeField] private AnimationCurve _trichromeDensityGeneticCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1.5f);
        [SerializeField] private Color _baseTrichromeColor = new Color(0.95f, 0.95f, 0.9f, 0.8f);
        
        [Header("Coloration Mapping")]
        [SerializeField] private bool _enableColorationMapping = true;
        [SerializeField] private bool _enableLeafColorMapping = true;
        [SerializeField] private bool _enableStemColorMapping = true;
        [SerializeField] private bool _enableBudColorGradients = true;
        [SerializeField] private Color _baseLeafColor = new Color(0.2f, 0.6f, 0.1f, 1f);
        [SerializeField] private Color _baseStemColor = new Color(0.4f, 0.5f, 0.2f, 1f);
        
        [Header("Growth Pattern Mapping")]
        [SerializeField] private bool _enableGrowthPatternMapping = true;
        [SerializeField] private bool _enableApicalDominanceMapping = true;
        [SerializeField] private bool _enableLateralGrowthMapping = true;
        [SerializeField] private bool _enableFloweringTimeMapping = true;
        [SerializeField] private AnimationCurve _apicalDominanceGeneticCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1.8f);
        
        [Header("Cannabinoid Visualization")]
        [SerializeField] private bool _enableCannabinoidVisualization = true;
        [SerializeField] private bool _enableTHCVisualization = true;
        [SerializeField] private bool _enableCBDVisualization = true;
        [SerializeField] private bool _enableTerpeneVisualization = true;
        [SerializeField] private Color _thcVisualizationColor = new Color(0.9f, 0.3f, 0.1f, 0.6f);
        [SerializeField] private Color _cbdVisualizationColor = new Color(0.1f, 0.7f, 0.3f, 0.6f);
        
        [Header("Environmental Adaptation")]
        [SerializeField] private bool _enableEnvironmentalAdaptation = true;
        [SerializeField] private bool _enableLightAdaptationMapping = true;
        [SerializeField] private bool _enableTemperatureAdaptationMapping = true;
        [SerializeField] private bool _enableHumidityAdaptationMapping = true;
        [SerializeField] private float _adaptationResponseSpeed = 0.5f;
        
        [Header("Performance Optimization")]
        [SerializeField] private int _maxConcurrentMorphologyUpdates = 30;
        [SerializeField] private float _morphologyCullingDistance = 50f;
        [SerializeField] private bool _enableLODMorphologyMapping = true;
        [SerializeField] private bool _enableAdaptiveMorphologyQuality = true;
        
        // Genetic Morphology State
        private Dictionary<string, GeneticMorphologyInstance> _activeMorphologyInstances = new Dictionary<string, GeneticMorphologyInstance>();
        private Queue<string> _morphologyUpdateQueue = new Queue<string>();
        private Dictionary<string, CannabisGeneticProfile> _plantGeneticProfiles = new Dictionary<string, CannabisGeneticProfile>();
        
        // Morphology Mapping Configuration
        private Dictionary<string, MorphologyMappingConfiguration> _morphologyMappingConfigs = new Dictionary<string, MorphologyMappingConfiguration>();
        private Dictionary<string, GeneticTraitMapping> _traitMappings = new Dictionary<string, GeneticTraitMapping>();
        
        // Visual Property Caches
        private Dictionary<string, MaterialPropertyBlock> _materialPropertyBlocks = new Dictionary<string, MaterialPropertyBlock>();
        private Dictionary<string, Vector4> _morphologyParameters = new Dictionary<string, Vector4>();
        private Dictionary<string, Color> _geneticColoration = new Dictionary<string, Color>();
        
        // System Integration
        private object _geneticsManager;
        private object _vfxTemplateManager;
        private object _speedTreeIntegration;
        private object _lodSystem;
        private object _environmentalManager;
        
        // Performance Tracking
        private float _lastMorphologyUpdate = 0f;
        private int _morphologyUpdatesProcessedThisFrame = 0;
        private Camera _mainCamera;
        private Vector3 _lastCameraPosition;
        
        // Genetic Trait Definitions
        private Dictionary<string, GeneticTraitDefinition> _geneticTraitDefinitions = new Dictionary<string, GeneticTraitDefinition>();
        private List<string> _activeGeneticTraits = new List<string>();
        
        protected override void OnManagerInitialize()
        {
            InitializeMorphologyMappingSystem();
            InitializeGeneticTraitDefinitions();
            InitializeMorphologyConfigurations();
            
            _mainCamera = Camera.main;
            if (_mainCamera == null)
                _mainCamera = FindObjectOfType<Camera>();
                
            StartCoroutine(MorphologyUpdateRoutine());
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup morphology mapping resources
            _activeMorphologyInstances.Clear();
            _plantGeneticProfiles.Clear();
            _materialPropertyBlocks.Clear();
            _morphologyParameters.Clear();
            _geneticColoration.Clear();
        }
        
        private void InitializeMorphologyMappingSystem()
        {
            // System integration would be implemented when managers are available
            // _geneticsManager = GameManager.Instance?.GetManager<GeneticsManager>();
            // _vfxTemplateManager = GameManager.Instance?.GetManager<CannabisVFXTemplateManager>();
            // _speedTreeIntegration = GameManager.Instance?.GetManager<SpeedTreeVFXIntegrationManager>();
            // _lodSystem = GameManager.Instance?.GetManager<AdvancedLODSystem>();
            // _environmentalManager = GameManager.Instance?.GetManager<EnvironmentalManager>();
            
            // Event subscription would be implemented when managers are available
            // if (_geneticsManager != null)
            // {
            //     _geneticsManager.OnGeneticProfileUpdated += OnGeneticProfileUpdated;
            //     _geneticsManager.OnBreedingCompleted += OnBreedingCompleted;
            // }
        }
        
        private void InitializeGeneticTraitDefinitions()
        {
            // Plant Structure Traits
            _geneticTraitDefinitions["height"] = new GeneticTraitDefinition
            {
                TraitName = "Height",
                TraitType = "Quantitative",
                MinValue = 0.3f,
                MaxValue = 3.0f,
                MorphologyWeight = 1.0f,
                VisualProperty = "PlantHeight",
                MappingCurve = _heightGeneticCurve
            };
            
            _geneticTraitDefinitions["width"] = new GeneticTraitDefinition
            {
                TraitName = "Width",
                TraitType = "Quantitative",
                MinValue = 0.4f,
                MaxValue = 2.0f,
                MorphologyWeight = 0.8f,
                VisualProperty = "PlantWidth",
                MappingCurve = _widthGeneticCurve
            };
            
            _geneticTraitDefinitions["branching"] = new GeneticTraitDefinition
            {
                TraitName = "Branching",
                TraitType = "Quantitative",
                MinValue = 0.2f,
                MaxValue = 1.8f,
                MorphologyWeight = 0.9f,
                VisualProperty = "BranchingFactor",
                MappingCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1.5f)
            };
            
            // Bud Development Traits
            _geneticTraitDefinitions["bud_size"] = new GeneticTraitDefinition
            {
                TraitName = "BudSize",
                TraitType = "Quantitative",
                MinValue = 0.5f,
                MaxValue = 2.5f,
                MorphologyWeight = 1.2f,
                VisualProperty = "BudSize",
                MappingCurve = _budSizeGeneticCurve
            };
            
            _geneticTraitDefinitions["bud_density"] = new GeneticTraitDefinition
            {
                TraitName = "BudDensity",
                TraitType = "Quantitative",
                MinValue = 0.3f,
                MaxValue = 1.5f,
                MorphologyWeight = 1.0f,
                VisualProperty = "BudDensity",
                MappingCurve = _budDensityGeneticCurve
            };
            
            // Trichrome Traits
            _geneticTraitDefinitions["trichrome_density"] = new GeneticTraitDefinition
            {
                TraitName = "TrichromeDensity",
                TraitType = "Quantitative",
                MinValue = 0.1f,
                MaxValue = 2.0f,
                MorphologyWeight = 1.5f,
                VisualProperty = "TrichromeDensity",
                MappingCurve = _trichromeDensityGeneticCurve
            };
            
            // Coloration Traits
            _geneticTraitDefinitions["leaf_color"] = new GeneticTraitDefinition
            {
                TraitName = "LeafColor",
                TraitType = "Categorical",
                MinValue = 0f,
                MaxValue = 1f,
                MorphologyWeight = 0.7f,
                VisualProperty = "LeafColoration",
                MappingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
            };
            
            // Growth Pattern Traits
            _geneticTraitDefinitions["apical_dominance"] = new GeneticTraitDefinition
            {
                TraitName = "ApicalDominance",
                TraitType = "Quantitative",
                MinValue = 0.2f,
                MaxValue = 1.8f,
                MorphologyWeight = 0.9f,
                VisualProperty = "ApicalDominance",
                MappingCurve = _apicalDominanceGeneticCurve
            };
            
            // Cannabinoid Traits
            _geneticTraitDefinitions["thc_content"] = new GeneticTraitDefinition
            {
                TraitName = "THCContent",
                TraitType = "Quantitative",
                MinValue = 0.01f,
                MaxValue = 0.35f,
                MorphologyWeight = 0.8f,
                VisualProperty = "THCVisualization",
                MappingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
            
            _geneticTraitDefinitions["cbd_content"] = new GeneticTraitDefinition
            {
                TraitName = "CBDContent",
                TraitType = "Quantitative",
                MinValue = 0.01f,
                MaxValue = 0.25f,
                MorphologyWeight = 0.8f,
                VisualProperty = "CBDVisualization",
                MappingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
        }
        
        private void InitializeMorphologyConfigurations()
        {
            // Cannabis Sativa Configuration
            _morphologyMappingConfigs["sativa"] = new MorphologyMappingConfiguration
            {
                StrainType = "Sativa",
                HeightMultiplier = 1.4f,
                WidthMultiplier = 0.8f,
                BranchingMultiplier = 1.2f,
                InternodeMultiplier = 1.5f,
                BudSizeMultiplier = 0.9f,
                TrichromeMultiplier = 1.0f,
                FloweringTimeMultiplier = 1.2f,
                ApicalDominanceMultiplier = 1.3f
            };
            
            // Cannabis Indica Configuration
            _morphologyMappingConfigs["indica"] = new MorphologyMappingConfiguration
            {
                StrainType = "Indica",
                HeightMultiplier = 0.7f,
                WidthMultiplier = 1.3f,
                BranchingMultiplier = 1.1f,
                InternodeMultiplier = 0.6f,
                BudSizeMultiplier = 1.3f,
                TrichromeMultiplier = 1.4f,
                FloweringTimeMultiplier = 0.8f,
                ApicalDominanceMultiplier = 0.8f
            };
            
            // Hybrid Configuration
            _morphologyMappingConfigs["hybrid"] = new MorphologyMappingConfiguration
            {
                StrainType = "Hybrid",
                HeightMultiplier = 1.0f,
                WidthMultiplier = 1.0f,
                BranchingMultiplier = 1.0f,
                InternodeMultiplier = 1.0f,
                BudSizeMultiplier = 1.0f,
                TrichromeMultiplier = 1.0f,
                FloweringTimeMultiplier = 1.0f,
                ApicalDominanceMultiplier = 1.0f
            };
            
            // Ruderalis Configuration
            _morphologyMappingConfigs["ruderalis"] = new MorphologyMappingConfiguration
            {
                StrainType = "Ruderalis",
                HeightMultiplier = 0.4f,
                WidthMultiplier = 0.6f,
                BranchingMultiplier = 0.7f,
                InternodeMultiplier = 0.5f,
                BudSizeMultiplier = 0.6f,
                TrichromeMultiplier = 0.8f,
                FloweringTimeMultiplier = 0.6f,
                ApicalDominanceMultiplier = 0.5f
            };
        }
        
        private IEnumerator MorphologyUpdateRoutine()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(1f / _morphologyUpdateFrequency);
                
                if (_enableGeneticMorphologyMapping && _enableRealtimeGeneticVisualization)
                {
                    UpdateMorphologyMappings();
                }
            }
        }
        
        private void UpdateMorphologyMappings()
        {
            if (_morphologyUpdateQueue.Count == 0) return;
            
            _morphologyUpdatesProcessedThisFrame = 0;
            int maxUpdates = _maxConcurrentMorphologyUpdates;
            
            while (_morphologyUpdateQueue.Count > 0 && _morphologyUpdatesProcessedThisFrame < maxUpdates)
            {
                string plantInstanceId = _morphologyUpdateQueue.Dequeue();
                
                if (_activeMorphologyInstances.TryGetValue(plantInstanceId, out GeneticMorphologyInstance morphologyInstance))
                {
                    UpdatePlantMorphology(morphologyInstance);
                    _morphologyUpdatesProcessedThisFrame++;
                }
            }
        }
        
        private void UpdatePlantMorphology(GeneticMorphologyInstance morphologyInstance)
        {
            if (morphologyInstance == null || morphologyInstance.PlantRenderer == null) return;
            
            // Distance culling
            if (_mainCamera != null)
            {
                float distance = Vector3.Distance(_mainCamera.transform.position, morphologyInstance.PlantRenderer.transform.position);
                if (distance > _morphologyCullingDistance) return;
            }
            
            // Get genetic profile
            if (!_plantGeneticProfiles.TryGetValue(morphologyInstance.PlantInstanceId, out CannabisGeneticProfile geneticProfile))
                return;
            
            // Calculate morphology parameters
            Vector4 morphologyParams = CalculateMorphologyParameters(geneticProfile, morphologyInstance);
            Color geneticColoration = CalculateGeneticColoration(geneticProfile, morphologyInstance);
            
            // Update visual properties
            UpdateVisualProperties(morphologyInstance, morphologyParams, geneticColoration);
            
            // System integration updates would be implemented when managers are available
            // if (_speedTreeIntegration != null)
            // {
            //     _speedTreeIntegration.UpdateGeneticMorphology(morphologyInstance.PlantInstanceId, morphologyParams);
            // }
            // 
            // if (_vfxTemplateManager != null)
            // {
            //     _vfxTemplateManager.UpdateGeneticVisualization(morphologyInstance.PlantInstanceId, geneticProfile);
            // }
        }
        
        private Vector4 CalculateMorphologyParameters(CannabisGeneticProfile geneticProfile, GeneticMorphologyInstance morphologyInstance)
        {
            Vector4 morphologyParams = Vector4.zero;
            
            // Get strain configuration
            string strainType = DetermineStrainType(geneticProfile);
            if (!_morphologyMappingConfigs.TryGetValue(strainType.ToLower(), out MorphologyMappingConfiguration config))
            {
                config = _morphologyMappingConfigs["hybrid"];
            }
            
            // Calculate height parameter
            if (_enableHeightMapping && _geneticTraitDefinitions.TryGetValue("height", out GeneticTraitDefinition heightTrait))
            {
                float heightValue = GetTraitValue(geneticProfile, "height");
                float heightMorphology = heightTrait.MappingCurve.Evaluate(heightValue) * config.HeightMultiplier;
                morphologyParams.x = heightMorphology;
            }
            
            // Calculate width parameter
            if (_enableWidthMapping && _geneticTraitDefinitions.TryGetValue("width", out GeneticTraitDefinition widthTrait))
            {
                float widthValue = GetTraitValue(geneticProfile, "width");
                float widthMorphology = widthTrait.MappingCurve.Evaluate(widthValue) * config.WidthMultiplier;
                morphologyParams.y = widthMorphology;
            }
            
            // Calculate branching parameter
            if (_enableBranchingMapping && _geneticTraitDefinitions.TryGetValue("branching", out GeneticTraitDefinition branchingTrait))
            {
                float branchingValue = GetTraitValue(geneticProfile, "branching");
                float branchingMorphology = branchingTrait.MappingCurve.Evaluate(branchingValue) * config.BranchingMultiplier;
                morphologyParams.z = branchingMorphology;
            }
            
            // Calculate bud density parameter
            if (_enableBudDensityMapping && _geneticTraitDefinitions.TryGetValue("bud_density", out GeneticTraitDefinition budDensityTrait))
            {
                float budDensityValue = GetTraitValue(geneticProfile, "bud_density");
                float budDensityMorphology = budDensityTrait.MappingCurve.Evaluate(budDensityValue) * config.BudSizeMultiplier;
                morphologyParams.w = budDensityMorphology;
            }
            
            // Apply environmental adaptation
            if (_enableEnvironmentalAdaptation)
            {
                morphologyParams = ApplyEnvironmentalAdaptation(morphologyParams, morphologyInstance);
            }
            
            return morphologyParams;
        }
        
        private Color CalculateGeneticColoration(CannabisGeneticProfile geneticProfile, GeneticMorphologyInstance morphologyInstance)
        {
            Color baseColor = _baseLeafColor;
            
            // Apply genetic coloration
            if (_enableColorationMapping && _geneticTraitDefinitions.TryGetValue("leaf_color", out GeneticTraitDefinition colorTrait))
            {
                float colorValue = GetTraitValue(geneticProfile, "leaf_color");
                float colorIntensity = colorTrait.MappingCurve.Evaluate(colorValue);
                
                // Blend with genetic color variations
                Color geneticColor = Color.Lerp(baseColor, new Color(baseColor.r * 0.8f, baseColor.g * 1.2f, baseColor.b * 0.9f), colorIntensity);
                baseColor = geneticColor;
            }
            
            // Apply trichrome coloration
            if (_enableTrichromeMapping && _geneticTraitDefinitions.TryGetValue("trichrome_density", out GeneticTraitDefinition trichromeTrait))
            {
                float trichromeValue = GetTraitValue(geneticProfile, "trichrome_density");
                float trichromeIntensity = trichromeTrait.MappingCurve.Evaluate(trichromeValue);
                
                Color trichromeColor = Color.Lerp(baseColor, _baseTrichromeColor, trichromeIntensity * 0.3f);
                baseColor = trichromeColor;
            }
            
            // Apply cannabinoid visualization
            if (_enableCannabinoidVisualization)
            {
                if (_enableTHCVisualization && _geneticTraitDefinitions.TryGetValue("thc_content", out GeneticTraitDefinition thcTrait))
                {
                    float thcValue = GetTraitValue(geneticProfile, "thc_content");
                    float thcIntensity = thcTrait.MappingCurve.Evaluate(thcValue);
                    baseColor = Color.Lerp(baseColor, _thcVisualizationColor, thcIntensity * 0.2f);
                }
                
                if (_enableCBDVisualization && _geneticTraitDefinitions.TryGetValue("cbd_content", out GeneticTraitDefinition cbdTrait))
                {
                    float cbdValue = GetTraitValue(geneticProfile, "cbd_content");
                    float cbdIntensity = cbdTrait.MappingCurve.Evaluate(cbdValue);
                    baseColor = Color.Lerp(baseColor, _cbdVisualizationColor, cbdIntensity * 0.2f);
                }
            }
            
            return baseColor;
        }
        
        private Vector4 ApplyEnvironmentalAdaptation(Vector4 morphologyParams, GeneticMorphologyInstance morphologyInstance)
        {
            // Simplified environmental adaptation - would integrate with actual environmental system
            Vector4 adaptedParams = morphologyParams;
            
            // Placeholder environmental conditions
            var environmentalConditions = new EnvironmentalConditions
            {
                LightIntensity = 800f,
                Temperature = 24f,
                Humidity = 65f,
                AirFlow = 1.5f,
                BarometricPressure = 1013.25f,
                LightDirection = 270f, // degrees (downward)
                WindDirection = 0f, // degrees (north)
                WindSpeed = 2f,
                CO2Level = 400f
            };
            
            // Light adaptation
            if (_enableLightAdaptationMapping)
            {
                float lightIntensity = environmentalConditions.LightIntensity / 1000f; // Normalize PPFD
                float lightAdaptation = Mathf.Clamp01(lightIntensity);
                adaptedParams.x *= 1f + (lightAdaptation - 0.5f) * 0.3f; // Height adaptation
                adaptedParams.z *= 1f + (lightAdaptation - 0.5f) * 0.2f; // Branching adaptation
            }
            
            // Temperature adaptation
            if (_enableTemperatureAdaptationMapping)
            {
                float temperature = environmentalConditions.Temperature;
                float tempAdaptation = Mathf.Clamp01((temperature - 18f) / 12f); // Normalize 18-30°C range
                adaptedParams.y *= 1f + (tempAdaptation - 0.5f) * 0.2f; // Width adaptation
                adaptedParams.w *= 1f + (tempAdaptation - 0.5f) * 0.15f; // Bud density adaptation
            }
            
            // Humidity adaptation
            if (_enableHumidityAdaptationMapping)
            {
                float humidity = environmentalConditions.Humidity;
                float humidityAdaptation = Mathf.Clamp01(humidity / 100f);
                adaptedParams.w *= 1f + (humidityAdaptation - 0.5f) * 0.1f; // Bud density adaptation
            }
            
            return adaptedParams;
        }
        
        private void UpdateVisualProperties(GeneticMorphologyInstance morphologyInstance, Vector4 morphologyParams, Color geneticColoration)
        {
            // Get or create material property block
            if (!_materialPropertyBlocks.TryGetValue(morphologyInstance.PlantInstanceId, out MaterialPropertyBlock propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                _materialPropertyBlocks[morphologyInstance.PlantInstanceId] = propertyBlock;
            }
            
            // Update morphology parameters
            propertyBlock.SetVector("_MorphologyParams", morphologyParams);
            propertyBlock.SetColor("_GeneticColoration", geneticColoration);
            
            // Update bud development parameters
            if (_enableBudSizeMapping && _geneticTraitDefinitions.TryGetValue("bud_size", out GeneticTraitDefinition budSizeTrait))
            {
                float budSize = GetTraitValue(_plantGeneticProfiles[morphologyInstance.PlantInstanceId], "bud_size");
                float budSizeMorphology = budSizeTrait.MappingCurve.Evaluate(budSize);
                propertyBlock.SetFloat("_BudSize", budSizeMorphology);
            }
            
            // Update trichrome parameters
            if (_enableTrichromeMapping && _geneticTraitDefinitions.TryGetValue("trichrome_density", out GeneticTraitDefinition trichromeTrait))
            {
                float trichromeDensity = GetTraitValue(_plantGeneticProfiles[morphologyInstance.PlantInstanceId], "trichrome_density");
                float trichromeMorphology = trichromeTrait.MappingCurve.Evaluate(trichromeDensity);
                propertyBlock.SetFloat("_TrichromeDensity", trichromeMorphology);
                propertyBlock.SetColor("_TrichromeColor", _baseTrichromeColor);
            }
            
            // Apply material property block to renderer
            morphologyInstance.PlantRenderer.SetPropertyBlock(propertyBlock);
            
            // Cache parameters for optimization
            _morphologyParameters[morphologyInstance.PlantInstanceId] = morphologyParams;
            _geneticColoration[morphologyInstance.PlantInstanceId] = geneticColoration;
        }
        
        private float GetTraitValue(CannabisGeneticProfile geneticProfile, string traitName)
        {
            // This would integrate with the actual genetics system
            // For now, return a normalized value based on genetic profile
            if (geneticProfile == null) return 0.5f;
            
            // Calculate trait value based on genetic profile
            // This is a simplified implementation - actual genetics would be more complex
            float traitValue = 0.5f;
            
            switch (traitName)
            {
                case "height":
                    traitValue = geneticProfile.HeightPotential;
                    break;
                case "width":
                    traitValue = geneticProfile.WidthPotential;
                    break;
                case "branching":
                    traitValue = geneticProfile.BranchingPotential;
                    break;
                case "bud_size":
                    traitValue = geneticProfile.BudSizePotential;
                    break;
                case "bud_density":
                    traitValue = geneticProfile.BudDensityPotential;
                    break;
                case "trichrome_density":
                    traitValue = geneticProfile.TrichromeDensityPotential;
                    break;
                case "thc_content":
                    traitValue = geneticProfile.THCPotential;
                    break;
                case "cbd_content":
                    traitValue = geneticProfile.CBDPotential;
                    break;
                case "leaf_color":
                    traitValue = geneticProfile.ColorVariationPotential;
                    break;
                case "apical_dominance":
                    traitValue = geneticProfile.ApicalDominancePotential;
                    break;
            }
            
            return Mathf.Clamp01(traitValue);
        }
        
        private string DetermineStrainType(CannabisGeneticProfile geneticProfile)
        {
            if (geneticProfile == null) return "hybrid";
            
            // Determine strain type based on genetic profile characteristics
            if (geneticProfile.SativaPercentage > 0.7f)
                return "sativa";
            else if (geneticProfile.IndicaPercentage > 0.7f)
                return "indica";
            else if (geneticProfile.RuderalisPercentage > 0.5f)
                return "ruderalis";
            else
                return "hybrid";
        }
        
        public void RegisterPlantForMorphologyMapping(string plantInstanceId, Renderer plantRenderer, CannabisGeneticProfile geneticProfile)
        {
            if (string.IsNullOrEmpty(plantInstanceId) || plantRenderer == null || geneticProfile == null)
                return;
            
            // Create morphology instance
            var morphologyInstance = new GeneticMorphologyInstance
            {
                PlantInstanceId = plantInstanceId,
                PlantRenderer = plantRenderer,
                GeneticProfile = geneticProfile,
                LastUpdateTime = Time.time,
                IsActivelyMapping = true
            };
            
            _activeMorphologyInstances[plantInstanceId] = morphologyInstance;
            _plantGeneticProfiles[plantInstanceId] = geneticProfile;
            
            // Add to update queue
            _morphologyUpdateQueue.Enqueue(plantInstanceId);
        }
        
        public void UnregisterPlantFromMorphologyMapping(string plantInstanceId)
        {
            if (string.IsNullOrEmpty(plantInstanceId)) return;
            
            _activeMorphologyInstances.Remove(plantInstanceId);
            _plantGeneticProfiles.Remove(plantInstanceId);
            _materialPropertyBlocks.Remove(plantInstanceId);
            _morphologyParameters.Remove(plantInstanceId);
            _geneticColoration.Remove(plantInstanceId);
        }
        
        public void UpdatePlantGeneticProfile(string plantInstanceId, CannabisGeneticProfile updatedProfile)
        {
            if (string.IsNullOrEmpty(plantInstanceId) || updatedProfile == null)
                return;
            
            _plantGeneticProfiles[plantInstanceId] = updatedProfile;
            
            if (_activeMorphologyInstances.TryGetValue(plantInstanceId, out GeneticMorphologyInstance morphologyInstance))
            {
                morphologyInstance.GeneticProfile = updatedProfile;
                _morphologyUpdateQueue.Enqueue(plantInstanceId);
            }
        }
        
        public Vector4 GetPlantMorphologyParameters(string plantInstanceId)
        {
            if (_morphologyParameters.TryGetValue(plantInstanceId, out Vector4 parameters))
                return parameters;
                
            return Vector4.zero;
        }
        
        public Color GetPlantGeneticColoration(string plantInstanceId)
        {
            if (_geneticColoration.TryGetValue(plantInstanceId, out Color coloration))
                return coloration;
                
            return Color.white;
        }
        
        // Event handlers would be implemented when genetics system is available
        // private void OnGeneticProfileUpdated(string plantInstanceId, CannabisGeneticProfile geneticProfile)
        // {
        //     UpdatePlantGeneticProfile(plantInstanceId, geneticProfile);
        // }
        // 
        // private void OnBreedingCompleted(string parentAId, string parentBId, string offspringId, CannabisGeneticProfile offspringProfile)
        // {
        //     var plantRenderer = GetPlantRenderer(offspringId);
        //     if (plantRenderer != null)
        //     {
        //         RegisterPlantForMorphologyMapping(offspringId, plantRenderer, offspringProfile);
        //     }
        // }
        
        private Renderer GetPlantRenderer(string plantInstanceId)
        {
            // This would integrate with the actual plant management system
            // For now, return null as placeholder
            return null;
        }
        
        private void OnDestroy()
        {
            // Event unsubscription would be implemented when genetics system is available
            // if (_geneticsManager != null)
            // {
            //     _geneticsManager.OnGeneticProfileUpdated -= OnGeneticProfileUpdated;
            //     _geneticsManager.OnBreedingCompleted -= OnBreedingCompleted;
            // }
            
            // Clean up material property blocks
            _materialPropertyBlocks.Clear();
        }
    }
    
    // Supporting Data Structures
    [System.Serializable]
    public class GeneticMorphologyInstance
    {
        public string PlantInstanceId;
        public Renderer PlantRenderer;
        public CannabisGeneticProfile GeneticProfile;
        public float LastUpdateTime;
        public bool IsActivelyMapping;
    }
    
    [System.Serializable]
    public class GeneticTraitDefinition
    {
        public string TraitName;
        public string TraitType;
        public float MinValue;
        public float MaxValue;
        public float MorphologyWeight;
        public string VisualProperty;
        public AnimationCurve MappingCurve;
    }
    
    [System.Serializable]
    public class MorphologyMappingConfiguration
    {
        public string StrainType;
        public float HeightMultiplier;
        public float WidthMultiplier;
        public float BranchingMultiplier;
        public float InternodeMultiplier;
        public float BudSizeMultiplier;
        public float TrichromeMultiplier;
        public float FloweringTimeMultiplier;
        public float ApicalDominanceMultiplier;
    }
    
    [System.Serializable]
    public class GeneticTraitMapping
    {
        public string TraitName;
        public string ShaderProperty;
        public AnimationCurve MappingCurve;
        public Vector2 ValueRange;
        public Color TraitColor;
    }
    
    [System.Serializable]
    public class CannabisGeneticProfile
    {
        public float HeightPotential;
        public float WidthPotential;
        public float BranchingPotential;
        public float BudSizePotential;
        public float BudDensityPotential;
        public float TrichromeDensityPotential;
        public float THCPotential;
        public float CBDPotential;
        public float ColorVariationPotential;
        public float ApicalDominancePotential;
        public float SativaPercentage;
        public float IndicaPercentage;
        public float RuderalisPercentage;
    }
    
    [System.Serializable]
    public class EnvironmentalConditions
    {
        public float LightIntensity;
        public float Temperature;
        public float Humidity;
        public float AirFlow;
        public float BarometricPressure;
        public float LightDirection;
        public float WindDirection;
        public float WindSpeed;
        public float CO2Level;
        
        public EnvironmentalConditions Clone()
        {
            return new EnvironmentalConditions
            {
                LightIntensity = this.LightIntensity,
                Temperature = this.Temperature,
                Humidity = this.Humidity,
                AirFlow = this.AirFlow,
                BarometricPressure = this.BarometricPressure,
                LightDirection = this.LightDirection,
                WindDirection = this.WindDirection,
                WindSpeed = this.WindSpeed,
                CO2Level = this.CO2Level
            };
        }
    }
}