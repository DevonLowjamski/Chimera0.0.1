using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// SpeedTree Genetics Service - Handles SpeedTree integration and genetic trait application to plant visuals
    /// Extracted from CannabisGeneticsEngine to provide focused SpeedTree genetics functionality
    /// Manages morphological and color trait application to SpeedTree plant instances for realistic genetic variation
    /// Implements visual genetics: trait-to-visual mapping, genetic rendering, and phenotype visualization
    /// </summary>
    public class SpeedTreeGeneticsService : MonoBehaviour
    {
        [Header("SpeedTree Integration Configuration")]
        [SerializeField] private bool _enableSpeedTreeIntegration = true;
        [SerializeField] private bool _enableMorphologicalTraits = true;
        [SerializeField] private bool _enableColorTraits = true;
        [SerializeField] private bool _enableGeneticsLogging = false;

        [Header("Trait Application Parameters")]
        [SerializeField] private float _morphologyMultiplier = 1.0f;
        [SerializeField] private float _colorIntensityMultiplier = 1.0f;
        [SerializeField] private float _traitVariationRange = 0.2f;
        [SerializeField] private float _geneticExpressionStrength = 0.8f;

        [Header("Visual Genetics Settings")]
        [SerializeField] private bool _enableHeightVariation = true;
        [SerializeField] private bool _enableLeafVariation = true;
        [SerializeField] private bool _enableBranchVariation = true;
        [SerializeField] private bool _enableBudVariation = true;

        [Header("Performance Settings")]
        [SerializeField] private bool _enableBatchUpdates = true;
        [SerializeField] private int _maxUpdatesPerFrame = 10;
        [SerializeField] private float _updateInterval = 0.1f;
        [SerializeField] private bool _enableLODOptimization = true;

        // Service state
        private bool _isInitialized = false;
        private AdvancedSpeedTreeManager _speedTreeManager;
        private ScriptableObject _geneticsConfig;

        // SpeedTree integration data
        private Dictionary<int, GeneticSpeedTreeMapping> _speedTreeMappings = new Dictionary<int, GeneticSpeedTreeMapping>();
        private Dictionary<int, SpeedTreePlantData> _managedInstances = new Dictionary<int, SpeedTreePlantData>();
        private Dictionary<string, CannabisPhenotype> _phenotypeCache = new Dictionary<string, CannabisPhenotype>();

        // Trait application tracking
        private Dictionary<string, TraitApplicationData> _traitApplications = new Dictionary<string, TraitApplicationData>();
        private Dictionary<int, GeneticRenderingData> _renderingData = new Dictionary<int, GeneticRenderingData>();

        // Update batching
        private Queue<SpeedTreeUpdateRequest> _updateQueue = new Queue<SpeedTreeUpdateRequest>();
        private float _lastUpdateTime = 0f;
        private int _updatesThisFrame = 0;

        // Performance tracking
        private int _totalAppliedTraits = 0;
        private int _totalInstancesManaged = 0;
        private float _averageApplicationTime = 0f;
        private SpeedTreeGeneticsAnalytics _analyticsData = new SpeedTreeGeneticsAnalytics();

        // Events
        public static event Action<SpeedTreePlantData, CannabisGenotype> OnGeneticsApplied;
        public static event Action<int, GeneticSpeedTreeMapping> OnMappingCreated;
        public static event Action<SpeedTreePlantData, CannabisPhenotype> OnTraitsApplied;
        public static event Action<SpeedTreeGeneticsAnalytics> OnAnalyticsUpdated;
        public static event Action<string> OnGeneticsError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "SpeedTree Genetics Service";
        public int TotalAppliedTraits => _totalAppliedTraits;
        public int ManagedInstances => _managedInstances.Count;
        public int ActiveMappings => _speedTreeMappings.Count;
        public float CacheHitRate => _totalAppliedTraits > 0 ? _phenotypeCache.Count / (float)_totalAppliedTraits : 0f;

        public void Initialize(ScriptableObject geneticsConfig = null)
        {
            InitializeService(geneticsConfig);
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            // Service will be initialized by orchestrator
        }

        private void Update()
        {
            if (_isInitialized)
            {
                ProcessUpdateQueue();
                UpdateAnalytics();
                _updatesThisFrame = 0; // Reset frame counter
            }
        }

        private void InitializeDataStructures()
        {
            _speedTreeMappings = new Dictionary<int, GeneticSpeedTreeMapping>();
            _managedInstances = new Dictionary<int, SpeedTreePlantData>();
            _phenotypeCache = new Dictionary<string, CannabisPhenotype>();
            _traitApplications = new Dictionary<string, TraitApplicationData>();
            _renderingData = new Dictionary<int, GeneticRenderingData>();
            _updateQueue = new Queue<SpeedTreeUpdateRequest>();
            _analyticsData = new SpeedTreeGeneticsAnalytics();
        }

        public void InitializeService(ScriptableObject geneticsConfig = null)
        {
            if (_isInitialized)
            {
                if (_enableGeneticsLogging)
                    Debug.LogWarning("SpeedTreeGeneticsService already initialized");
                return;
            }

            try
            {
                _geneticsConfig = geneticsConfig;

                InitializeSpeedTreeManager();
                InitializeTraitMappings();
                InitializeRenderingSystem();
                InitializeAnalytics();
                
                _isInitialized = true;
                
                if (_enableGeneticsLogging)
                    Debug.Log("SpeedTreeGeneticsService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize SpeedTreeGeneticsService: {ex.Message}");
                OnGeneticsError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Unregister from SpeedTree manager events
                if (_speedTreeManager != null)
                {
                    _speedTreeManager.OnPlantInstanceCreated -= HandlePlantInstanceCreated;
                }

                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableGeneticsLogging)
                    Debug.Log("SpeedTreeGeneticsService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during SpeedTreeGeneticsService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region SpeedTree Integration

        /// <summary>
        /// Apply genetics to SpeedTree plant instance
        /// </summary>
        public void ApplyGeneticsToSpeedTree(SpeedTreePlantData instance, CannabisGenotype genotype)
        {
            if (!_isInitialized || !_enableSpeedTreeIntegration || instance == null || genotype == null)
                return;

            try
            {
                var startTime = DateTime.Now;

                if (_enableGeneticsLogging)
                    Debug.Log($"Applying genetics to SpeedTree instance {instance.InstanceId}: {genotype.StrainName}");

                // Get or calculate phenotype
                var phenotype = GetOrCalculatePhenotype(genotype, instance.EnvironmentalConditions);
                if (phenotype == null)
                {
                    OnGeneticsError?.Invoke($"Failed to calculate phenotype for {genotype.StrainName}");
                    return;
                }

                // Create or update genetic mapping
                CreateOrUpdateMapping(instance, genotype, phenotype);

                // Apply trait categories
                if (_enableMorphologicalTraits)
                {
                    ApplyMorphologicalTraits(instance, phenotype);
                }

                if (_enableColorTraits)
                {
                    ApplyColorTraits(instance, phenotype);
                }

                ApplyGrowthTraits(instance, phenotype);
                ApplyEnvironmentalTraits(instance, phenotype);

                // Update instance genetic data
                UpdateInstanceGeneticData(instance, phenotype);

                // Track performance
                var applicationTime = (float)(DateTime.Now - startTime).TotalSeconds;
                UpdatePerformanceMetrics(applicationTime);

                // Register managed instance
                _managedInstances[instance.InstanceId] = instance;

                // Fire events
                OnGeneticsApplied?.Invoke(instance, genotype);
                OnTraitsApplied?.Invoke(instance, phenotype);

                if (_enableGeneticsLogging)
                    Debug.Log($"Genetics applied to SpeedTree instance {instance.InstanceId} in {applicationTime:F3}s");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying genetics to SpeedTree: {ex.Message}");
                OnGeneticsError?.Invoke($"Genetics application failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply morphological traits to plant instance
        /// </summary>
        private void ApplyMorphologicalTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance?.Renderer == null) return;

            try
            {
                // Apply size modifications
                if (_enableHeightVariation)
                {
                    var heightModifier = phenotype.PlantHeight * _morphologyMultiplier;
                    var stemModifier = phenotype.StemThickness * _morphologyMultiplier;
                    var sizeModifier = (heightModifier + stemModifier) / 2f;
                    
                    instance.Scale = Vector3.one * Mathf.Clamp(sizeModifier, 0.5f, 2.0f);
                }

                // Apply morphological properties to material
                if (instance.Renderer.materialProperties != null)
                {
                    if (_enableBranchVariation)
                    {
                        instance.Renderer.materialProperties.SetFloat("_BranchDensity", 
                            Mathf.Clamp(phenotype.BranchDensity * _morphologyMultiplier, 0.3f, 2.0f));
                    }

                    if (_enableLeafVariation)
                    {
                        instance.Renderer.materialProperties.SetFloat("_LeafDensity", 
                            Mathf.Clamp(phenotype.LeafDensity * _morphologyMultiplier, 0.5f, 1.8f));
                    }

                    instance.Renderer.materialProperties.SetFloat("_InternodeSpacing", 
                        Mathf.Clamp(phenotype.InternodeSpacing * _morphologyMultiplier, 0.6f, 1.6f));
                }

                // Update genetic data
                if (instance.GeneticData != null)
                {
                    instance.GeneticData.PlantSize = instance.Scale.x;
                    instance.GeneticData.BranchDensity = phenotype.BranchDensity;
                    instance.GeneticData.LeafDensity = phenotype.LeafDensity;
                    instance.GeneticData.StemThickness = phenotype.StemThickness;
                }

                _totalAppliedTraits++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying morphological traits: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply color traits to plant instance
        /// </summary>
        private void ApplyColorTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance?.Renderer == null) return;

            try
            {
                // Calculate color variations with genetic expression strength
                var leafColorIntensity = phenotype.ColorIntensity * _colorIntensityMultiplier * _geneticExpressionStrength;
                var leafColor = Color.Lerp(Color.green, phenotype.LeafColor, Mathf.Clamp01(leafColorIntensity));
                
                var budColorIntensity = phenotype.ColorIntensity * _colorIntensityMultiplier * _geneticExpressionStrength;
                var budColor = Color.Lerp(Color.green, phenotype.BudColor, Mathf.Clamp01(budColorIntensity));

                // Apply to instance
                instance.CurrentLeafColor = leafColor;

                // Apply to material properties
                if (instance.Renderer.materialProperties != null)
                {
                    instance.Renderer.materialProperties.SetColor("_LeafColor", leafColor);
                    
                    if (_enableBudVariation)
                    {
                        instance.Renderer.materialProperties.SetColor("_BudColor", budColor);
                    }
                    
                    instance.Renderer.materialProperties.SetFloat("_ColorVariation", 
                        Mathf.Clamp(phenotype.ColorVariation * _traitVariationRange, 0f, 0.5f));
                }

                // Update genetic data
                if (instance.GeneticData != null)
                {
                    instance.GeneticData.LeafColor = leafColor;
                    instance.GeneticData.BudColor = budColor;
                    instance.GeneticData.ColorVariation = phenotype.ColorVariation;
                }

                _totalAppliedTraits++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying color traits: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply growth traits to plant instance
        /// </summary>
        private void ApplyGrowthTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance == null) return;

            try
            {
                // Apply growth rate modifiers
                instance.GrowthRate = Mathf.Clamp(phenotype.GrowthRate * _geneticExpressionStrength, 0.3f, 2.0f);

                // Update environmental modifiers
                if (instance.EnvironmentalModifiers == null)
                {
                    instance.EnvironmentalModifiers = new Dictionary<string, float>();
                }

                instance.EnvironmentalModifiers["GrowthRate"] = phenotype.GrowthRate;
                instance.EnvironmentalModifiers["FloweringSpeed"] = phenotype.FloweringTime;
                instance.EnvironmentalModifiers["YieldPotential"] = phenotype.YieldPotential;

                // Update genetic data
                if (instance.GeneticData != null)
                {
                    instance.GeneticData.GrowthRate = phenotype.GrowthRate;
                    instance.GeneticData.FloweringSpeed = phenotype.FloweringTime;
                    instance.GeneticData.YieldPotential = phenotype.YieldPotential;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying growth traits: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply environmental traits to plant instance
        /// </summary>
        private void ApplyEnvironmentalTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance == null) return;

            try
            {
                // Update environmental modifiers
                if (instance.EnvironmentalModifiers == null)
                {
                    instance.EnvironmentalModifiers = new Dictionary<string, float>();
                }

                instance.EnvironmentalModifiers["HeatTolerance"] = phenotype.HeatTolerance;
                instance.EnvironmentalModifiers["ColdTolerance"] = phenotype.ColdTolerance;
                instance.EnvironmentalModifiers["DroughtTolerance"] = phenotype.DroughtTolerance;
                instance.EnvironmentalModifiers["StressResistance"] = phenotype.StressResistance;

                // Update genetic data
                if (instance.GeneticData != null)
                {
                    instance.GeneticData.HeatTolerance = phenotype.HeatTolerance;
                    instance.GeneticData.ColdTolerance = phenotype.ColdTolerance;
                    instance.GeneticData.DroughtTolerance = phenotype.DroughtTolerance;
                    instance.GeneticData.DiseaseResistance = phenotype.StressResistance; // Map to closest equivalent
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying environmental traits: {ex.Message}");
            }
        }

        #endregion

        #region Genetic Mapping

        /// <summary>
        /// Create or update genetic mapping for SpeedTree instance
        /// </summary>
        private void CreateOrUpdateMapping(SpeedTreePlantData instance, CannabisGenotype genotype, CannabisPhenotype phenotype)
        {
            var mapping = new GeneticSpeedTreeMapping
            {
                InstanceId = instance.InstanceId,
                GenotypeId = genotype.GenotypeId,
                StrainName = genotype.StrainName,
                Generation = genotype.Generation,
                LastEnvironment = instance.EnvironmentalConditions,
                MorphologicalTraits = ExtractMorphologicalTraits(phenotype),
                ColorTraits = ExtractColorTraits(phenotype),
                GrowthTraits = ExtractGrowthTraits(phenotype),
                EnvironmentalTraits = ExtractEnvironmentalTraits(phenotype),
                MappingCreationTime = DateTime.Now
            };

            _speedTreeMappings[instance.InstanceId] = mapping;
            
            OnMappingCreated?.Invoke(instance.InstanceId, mapping);
        }

        /// <summary>
        /// Extract morphological traits from phenotype
        /// </summary>
        private Dictionary<string, float> ExtractMorphologicalTraits(CannabisPhenotype phenotype)
        {
            return new Dictionary<string, float>
            {
                ["Height"] = phenotype.PlantHeight,
                ["StemThickness"] = phenotype.StemThickness,
                ["BranchDensity"] = phenotype.BranchDensity,
                ["LeafDensity"] = phenotype.LeafDensity,
                ["InternodeSpacing"] = phenotype.InternodeSpacing,
                ["ColorIntensity"] = phenotype.ColorIntensity,
                ["ColorVariation"] = phenotype.ColorVariation
            };
        }

        /// <summary>
        /// Extract color traits from phenotype
        /// </summary>
        private Dictionary<string, Color> ExtractColorTraits(CannabisPhenotype phenotype)
        {
            return new Dictionary<string, Color>
            {
                ["LeafColor"] = phenotype.LeafColor,
                ["BudColor"] = phenotype.BudColor
            };
        }

        /// <summary>
        /// Extract growth traits from phenotype
        /// </summary>
        private Dictionary<string, float> ExtractGrowthTraits(CannabisPhenotype phenotype)
        {
            return new Dictionary<string, float>
            {
                ["GrowthRate"] = phenotype.GrowthRate,
                ["FloweringTime"] = phenotype.FloweringTime,
                ["YieldPotential"] = phenotype.YieldPotential
            };
        }

        /// <summary>
        /// Extract environmental traits from phenotype
        /// </summary>
        private Dictionary<string, float> ExtractEnvironmentalTraits(CannabisPhenotype phenotype)
        {
            return new Dictionary<string, float>
            {
                ["HeatTolerance"] = phenotype.HeatTolerance,
                ["ColdTolerance"] = phenotype.ColdTolerance,
                ["DroughtTolerance"] = phenotype.DroughtTolerance,
                ["StressResistance"] = phenotype.StressResistance,
                ["OverallVigor"] = phenotype.OverallVigor
            };
        }

        #endregion

        #region Utility Methods

        private void InitializeSpeedTreeManager()
        {
            try
            {
                _speedTreeManager = GameManager.Instance.GetManager<AdvancedSpeedTreeManager>();
                
                if (_speedTreeManager != null)
                {
                    _speedTreeManager.OnPlantInstanceCreated += HandlePlantInstanceCreated;
                    
                    if (_enableGeneticsLogging)
                        Debug.Log("SpeedTree manager connected successfully");
                }
                else if (_enableGeneticsLogging)
                {
                    Debug.LogWarning("AdvancedSpeedTreeManager not found - SpeedTree integration disabled");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize SpeedTree manager: {ex.Message}");
            }
        }

        private void InitializeTraitMappings()
        {
            // Initialize trait-to-visual mappings and default values
        }

        private void InitializeRenderingSystem()
        {
            // Initialize rendering data structures and optimization systems
        }

        private void InitializeAnalytics()
        {
            _analyticsData = new SpeedTreeGeneticsAnalytics
            {
                TotalAppliedTraits = 0,
                ManagedInstances = 0,
                AverageApplicationTime = 0f,
                CacheHitRate = 0f,
                LastAnalyticsUpdate = DateTime.Now
            };
        }

        private CannabisPhenotype GetOrCalculatePhenotype(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            var cacheKey = $"{genotype.GenotypeId}_{(conditions != null ? conditions.GetHashCode() : 0)}";
            
            if (_phenotypeCache.TryGetValue(cacheKey, out var cachedPhenotype))
            {
                return cachedPhenotype;
            }

            // Calculate phenotype - this would typically use PhenotypeExpressionService
            // For now, create a basic phenotype from genotype
            var phenotype = CreateBasicPhenotype(genotype);
            _phenotypeCache[cacheKey] = phenotype;
            
            return phenotype;
        }

        private CannabisPhenotype CreateBasicPhenotype(CannabisGenotype genotype)
        {
            var phenotype = new CannabisPhenotype
            {
                PhenotypeId = Guid.NewGuid().ToString(),
                GenotypeId = genotype.GenotypeId,
                MorphologicalTraits = new Dictionary<string, float>(),
                ColorTraits = new Dictionary<string, Color>(),
                GrowthTraits = new Dictionary<string, float>(),
                EnvironmentalTraits = new Dictionary<string, float>(),
                BiochemicalTraits = new Dictionary<string, float>(),
                ExpressionDate = DateTime.Now
            };

            // Extract basic traits from genotype
            if (genotype.Traits != null)
            {
                foreach (var trait in genotype.Traits)
                {
                    switch (trait.TraitName.ToLower())
                    {
                        case "plant_height":
                            phenotype.MorphologicalTraits["Height"] = trait.ExpressedValue;
                            break;
                        case "growth_rate":
                            phenotype.GrowthTraits["GrowthRate"] = trait.ExpressedValue;
                            break;
                        case "branch_density":
                            phenotype.MorphologicalTraits["BranchDensity"] = trait.ExpressedValue;
                            break;
                        case "leaf_size":
                            phenotype.MorphologicalTraits["LeafSize"] = trait.ExpressedValue;
                            break;
                    }
                }
            }

            // Set defaults for missing traits
            SetDefaultTraitValues(phenotype);
            
            return phenotype;
        }

        private void SetDefaultTraitValues(CannabisPhenotype phenotype)
        {
            // Morphological defaults
            if (!phenotype.MorphologicalTraits.ContainsKey("Height"))
                phenotype.MorphologicalTraits["Height"] = 1.0f;
            if (!phenotype.MorphologicalTraits.ContainsKey("StemThickness"))
                phenotype.MorphologicalTraits["StemThickness"] = 1.0f;
            if (!phenotype.MorphologicalTraits.ContainsKey("BranchDensity"))
                phenotype.MorphologicalTraits["BranchDensity"] = 1.0f;
            if (!phenotype.MorphologicalTraits.ContainsKey("LeafDensity"))
                phenotype.MorphologicalTraits["LeafDensity"] = 1.0f;
            if (!phenotype.MorphologicalTraits.ContainsKey("InternodeSpacing"))
                phenotype.MorphologicalTraits["InternodeSpacing"] = 1.0f;
            if (!phenotype.MorphologicalTraits.ContainsKey("ColorIntensity"))
                phenotype.MorphologicalTraits["ColorIntensity"] = 0.5f;
            if (!phenotype.MorphologicalTraits.ContainsKey("ColorVariation"))
                phenotype.MorphologicalTraits["ColorVariation"] = 0.3f;

            // Color defaults
            if (!phenotype.ColorTraits.ContainsKey("LeafColor"))
                phenotype.ColorTraits["LeafColor"] = Color.green;
            if (!phenotype.ColorTraits.ContainsKey("BudColor"))
                phenotype.ColorTraits["BudColor"] = new Color(0.2f, 0.8f, 0.3f);

            // Growth defaults
            if (!phenotype.GrowthTraits.ContainsKey("GrowthRate"))
                phenotype.GrowthTraits["GrowthRate"] = 1.0f;
            if (!phenotype.GrowthTraits.ContainsKey("FloweringTime"))
                phenotype.GrowthTraits["FloweringTime"] = 1.0f;
            if (!phenotype.GrowthTraits.ContainsKey("YieldPotential"))
                phenotype.GrowthTraits["YieldPotential"] = 1.0f;

            // Environmental defaults
            if (!phenotype.EnvironmentalTraits.ContainsKey("HeatTolerance"))
                phenotype.EnvironmentalTraits["HeatTolerance"] = 0.5f;
            if (!phenotype.EnvironmentalTraits.ContainsKey("ColdTolerance"))
                phenotype.EnvironmentalTraits["ColdTolerance"] = 0.5f;
            if (!phenotype.EnvironmentalTraits.ContainsKey("DroughtTolerance"))
                phenotype.EnvironmentalTraits["DroughtTolerance"] = 0.5f;

            phenotype.OverallVigor = 1.0f;
            phenotype.StressResistance = 0.5f;
        }

        private void UpdateInstanceGeneticData(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance.GeneticData == null)
            {
                instance.GeneticData = new CannabisGeneticData();
            }

            // Update comprehensive genetic data based on phenotype
            instance.GeneticData.PhenotypeId = phenotype.PhenotypeId;
            instance.GeneticData.LastGeneticUpdate = DateTime.Now;
        }

        private void UpdatePerformanceMetrics(float applicationTime)
        {
            _totalAppliedTraits++;
            _averageApplicationTime = (_averageApplicationTime + applicationTime) / 2f;
        }

        private void ProcessUpdateQueue()
        {
            if (!_enableBatchUpdates) return;

            var currentTime = Time.time;
            if (currentTime - _lastUpdateTime < _updateInterval) return;

            int processedUpdates = 0;
            while (_updateQueue.Count > 0 && processedUpdates < _maxUpdatesPerFrame && _updatesThisFrame < _maxUpdatesPerFrame)
            {
                var request = _updateQueue.Dequeue();
                ProcessUpdateRequest(request);
                processedUpdates++;
                _updatesThisFrame++;
            }

            _lastUpdateTime = currentTime;
        }

        private void ProcessUpdateRequest(SpeedTreeUpdateRequest request)
        {
            try
            {
                if (_managedInstances.TryGetValue(request.InstanceId, out var instance))
                {
                    ApplyGeneticsToSpeedTree(instance, request.Genotype);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing update request: {ex.Message}");
            }
        }

        private void UpdateAnalytics()
        {
            var currentTime = DateTime.Now;
            if (_analyticsData.LastAnalyticsUpdate == default ||
                (currentTime - _analyticsData.LastAnalyticsUpdate).TotalMinutes >= 1)
            {
                _analyticsData.TotalAppliedTraits = _totalAppliedTraits;
                _analyticsData.ManagedInstances = _managedInstances.Count;
                _analyticsData.AverageApplicationTime = _averageApplicationTime;
                _analyticsData.CacheHitRate = CacheHitRate;
                _analyticsData.LastAnalyticsUpdate = currentTime;
                
                OnAnalyticsUpdated?.Invoke(_analyticsData);
            }
        }

        private void HandlePlantInstanceCreated(SpeedTreePlantData instance)
        {
            if (!_enableSpeedTreeIntegration) return;

            try
            {
                // Find matching genotype for this instance
                // This would typically query a genetics service or database
                // For now, we'll create a basic default genotype
                var defaultGenotype = CreateDefaultGenotype(instance);
                
                if (defaultGenotype != null)
                {
                    ApplyGeneticsToSpeedTree(instance, defaultGenotype);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error handling plant instance creation: {ex.Message}");
            }
        }

        private CannabisGenotype CreateDefaultGenotype(SpeedTreePlantData instance)
        {
            // Create a basic default genotype
            // In a real implementation, this would query the genetics service
            return new CannabisGenotype
            {
                GenotypeId = Guid.NewGuid().ToString(),
                StrainName = "Default Strain",
                Generation = 1,
                CreationDate = DateTime.Now
            };
        }

        private void ClearAllData()
        {
            _speedTreeMappings.Clear();
            _managedInstances.Clear();
            _phenotypeCache.Clear();
            _traitApplications.Clear();
            _renderingData.Clear();
            _updateQueue.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get genetic mapping for SpeedTree instance
        /// </summary>
        public GeneticSpeedTreeMapping GetGeneticMapping(int instanceId)
        {
            return _speedTreeMappings.TryGetValue(instanceId, out var mapping) ? mapping : null;
        }

        /// <summary>
        /// Queue genetics update for SpeedTree instance
        /// </summary>
        public void QueueGeneticsUpdate(SpeedTreePlantData instance, CannabisGenotype genotype)
        {
            if (_enableBatchUpdates)
            {
                _updateQueue.Enqueue(new SpeedTreeUpdateRequest
                {
                    InstanceId = instance.InstanceId,
                    Instance = instance,
                    Genotype = genotype,
                    RequestTime = DateTime.Now
                });
            }
            else
            {
                ApplyGeneticsToSpeedTree(instance, genotype);
            }
        }

        /// <summary>
        /// Update genetics settings at runtime
        /// </summary>
        public void UpdateGeneticsSettings(bool enableIntegration, float morphologyMultiplier, float colorMultiplier, float expressionStrength)
        {
            _enableSpeedTreeIntegration = enableIntegration;
            _morphologyMultiplier = morphologyMultiplier;
            _colorIntensityMultiplier = colorMultiplier;
            _geneticExpressionStrength = expressionStrength;
            
            if (_enableGeneticsLogging)
                Debug.Log($"Genetics settings updated: Integration={enableIntegration}, Morphology={morphologyMultiplier}, Color={colorMultiplier}, Expression={expressionStrength}");
        }

        /// <summary>
        /// Get genetics analytics data
        /// </summary>
        public SpeedTreeGeneticsAnalytics GetAnalytics()
        {
            return _analyticsData;
        }

        /// <summary>
        /// Clear phenotype cache
        /// </summary>
        public void ClearPhenotypeCache()
        {
            _phenotypeCache.Clear();
        }

        /// <summary>
        /// Get managed instance data
        /// </summary>
        public SpeedTreePlantData GetManagedInstance(int instanceId)
        {
            return _managedInstances.TryGetValue(instanceId, out var instance) ? instance : null;
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    /// <summary>
    /// SpeedTree update request for batch processing
    /// </summary>
    [System.Serializable]
    public class SpeedTreeUpdateRequest
    {
        public int InstanceId;
        public SpeedTreePlantData Instance;
        public CannabisGenotype Genotype;
        public DateTime RequestTime;
    }

    /// <summary>
    /// Trait application tracking data
    /// </summary>
    [System.Serializable]
    public class TraitApplicationData
    {
        public string TraitName = "";
        public float ApplicationTime = 0f;
        public int ApplicationCount = 0;
        public DateTime LastApplication = DateTime.Now;
        public bool IsActive = true;
    }

    /// <summary>
    /// Genetic rendering data for SpeedTree instances
    /// </summary>
    [System.Serializable]
    public class GeneticRenderingData
    {
        public int InstanceId;
        public string GenotypeId = "";
        public Dictionary<string, float> RenderingProperties = new Dictionary<string, float>();
        public Dictionary<string, Color> ColorProperties = new Dictionary<string, Color>();
        public DateTime LastRenderUpdate = DateTime.Now;
        public bool RequiresUpdate = false;
    }

    /// <summary>
    /// SpeedTree genetics analytics
    /// </summary>
    [System.Serializable]
    public class SpeedTreeGeneticsAnalytics
    {
        public int TotalAppliedTraits = 0;
        public int ManagedInstances = 0;
        public float AverageApplicationTime = 0f;
        public float CacheHitRate = 0f;
        public DateTime LastAnalyticsUpdate = DateTime.Now;
    }

    /// <summary>
    /// Genetic SpeedTree mapping for plant instances
    /// </summary>
    [System.Serializable]
    public class GeneticSpeedTreeMapping
    {
        public int InstanceId;
        public string GenotypeId = "";
        public string StrainName = "";
        public int Generation = 0;
        public EnvironmentalConditions LastEnvironment = new EnvironmentalConditions();
        public Dictionary<string, float> MorphologicalTraits = new Dictionary<string, float>();
        public Dictionary<string, Color> ColorTraits = new Dictionary<string, Color>();
        public Dictionary<string, float> GrowthTraits = new Dictionary<string, float>();
        public Dictionary<string, float> EnvironmentalTraits = new Dictionary<string, float>();
        public DateTime MappingCreationTime = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
        public bool IsActive = true;
    }
}