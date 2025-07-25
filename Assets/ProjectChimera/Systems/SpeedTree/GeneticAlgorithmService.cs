using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Genetic Algorithm Service - Handles core genetic processing, mutation operations, and genetic variation generation
    /// Extracted from CannabisGeneticsEngine to provide focused genetic algorithm functionality
    /// Manages genetic variation, mutation application, environmental pressure adaptation, and allele processing
    /// </summary>
    public class GeneticAlgorithmService : MonoBehaviour
    {
        [Header("Genetic Algorithm Configuration")]
        [SerializeField] private bool _enableMutations = true;
        [SerializeField] private float _mutationRate = 0.001f;
        [SerializeField] private bool _enableEnvironmentalPressure = true;
        [SerializeField] private float _variationIntensity = 0.1f;
        [SerializeField] private bool _enableLogging = false;

        [Header("Mutation Settings")]
        [SerializeField] private float _pointMutationRate = 0.4f;
        [SerializeField] private float _dominanceShiftRate = 0.3f;
        [SerializeField] private float _stabilityChangeRate = 0.2f;
        [SerializeField] private float _colorMutationRate = 0.1f;

        [Header("Variation Parameters")]
        [SerializeField] private float _maxDominanceVariation = 0.1f;
        [SerializeField] private float _maxExpressionVariation = 0.05f;
        [SerializeField] private float _maxColorVariation = 0.05f;
        [SerializeField] private float _baseVariationIntensity = 0.1f;

        // Service state
        private bool _isInitialized = false;
        private ScriptableObject _geneticsConfig;
        private ScriptableObject _geneLibrary;

        // Performance tracking
        private int _totalMutations = 0;
        private int _totalVariations = 0;
        private float _lastUpdateTime = 0f;

        // Events
        public static event Action<Allele, MutationType> OnMutationApplied;
        public static event Action<CannabisGenotype> OnVariationGenerated;
        public static event Action<string> OnGeneticProcessingError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Genetic Algorithm Service";
        public int TotalMutations => _totalMutations;
        public int TotalVariations => _totalVariations;

        public void Initialize(ScriptableObject geneticsConfig = null, ScriptableObject geneLibrary = null)
        {
            InitializeService(geneticsConfig, geneLibrary);
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
                UpdatePerformanceTracking();
            }
        }

        private void InitializeDataStructures()
        {
            _totalMutations = 0;
            _totalVariations = 0;
            _lastUpdateTime = Time.time;
        }

        public void InitializeService(ScriptableObject geneticsConfig = null, ScriptableObject geneLibrary = null)
        {
            if (_isInitialized)
            {
                if (_enableLogging)
                    Debug.LogWarning("GeneticAlgorithmService already initialized");
                return;
            }

            try
            {
                _geneticsConfig = geneticsConfig;
                _geneLibrary = geneLibrary;

                _isInitialized = true;
                
                if (_enableLogging)
                    Debug.Log("GeneticAlgorithmService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GeneticAlgorithmService: {ex.Message}");
                OnGeneticProcessingError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Reset counters
                _totalMutations = 0;
                _totalVariations = 0;
                
                _isInitialized = false;
                
                if (_enableLogging)
                    Debug.Log("GeneticAlgorithmService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during GeneticAlgorithmService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Genetic Variation Generation

        /// <summary>
        /// Generate genetic variation from a base genotype with optional environmental pressure
        /// </summary>
        public CannabisGenotype GenerateGeneticVariation(CannabisGenotype baseGenotype, EnvironmentalConditions? conditions = null)
        {
            if (!_isInitialized || baseGenotype == null)
            {
                if (_enableLogging)
                    Debug.LogWarning("Cannot generate variation - service not initialized or invalid genotype");
                return null;
            }

            try
            {
                var newGenotype = new CannabisGenotype
                {
                    GenotypeId = Guid.NewGuid().ToString(),
                    StrainId = baseGenotype.StrainId,
                    StrainName = $"{baseGenotype.StrainName}_Var_{_totalVariations}",
                    Generation = baseGenotype.Generation + 1,
                    CreationDate = DateTime.Now,
                    ParentGenotypes = new List<string> { baseGenotype.GenotypeId }
                };

                // Apply genetic variation
                ApplyGeneticVariation(newGenotype, baseGenotype);

                // Apply mutations if enabled
                if (_enableMutations)
                {
                    ApplyMutations(newGenotype);
                }

                // Apply environmental pressure if conditions provided
                if (conditions != null && _enableEnvironmentalPressure)
                {
                    ApplyEnvironmentalPressure(newGenotype, conditions);
                }

                _totalVariations++;
                OnVariationGenerated?.Invoke(newGenotype);

                if (_enableLogging)
                    Debug.Log($"Generated genetic variation: {newGenotype.StrainName}");

                return newGenotype;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating genetic variation: {ex.Message}");
                OnGeneticProcessingError?.Invoke($"Variation generation failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Apply genetic variation to create allelic diversity
        /// </summary>
        private void ApplyGeneticVariation(CannabisGenotype newGenotype, CannabisGenotype baseGenotype)
        {
            newGenotype.Alleles = new Dictionary<string, List<object>>();

            foreach (var geneAlleles in baseGenotype.Alleles)
            {
                var geneId = geneAlleles.Key;
                var baseAlleles = geneAlleles.Value.OfType<Allele>().ToList();
                var newAlleles = new List<object>();

                foreach (var baseAllele in baseAlleles)
                {
                    var variantAllele = CreateVariantAllele(baseAllele);
                    newAlleles.Add(variantAllele);
                }

                newGenotype.Alleles[geneId] = newAlleles;
            }
        }

        /// <summary>
        /// Create a variant allele with controlled genetic variation
        /// </summary>
        private Allele CreateVariantAllele(Allele baseAllele)
        {
            var variant = new Allele
            {
                AlleleId = Guid.NewGuid().ToString(),
                GeneId = baseAllele.GeneId,
                AlleleName = $"{baseAllele.AlleleName}_Var",
                Dominance = ApplyVariation(baseAllele.Dominance, _maxDominanceVariation),
                Expression = ApplyVariation(baseAllele.Expression, _maxExpressionVariation),
                Stability = baseAllele.Stability,
                MutationRate = baseAllele.MutationRate,
                ColorValue = VaryColor(baseAllele.ColorValue, _maxColorVariation),
                MorphologyValue = baseAllele.MorphologyValue,
                Properties = new Dictionary<string, float>(baseAllele.Properties),
                StrainTypeInfluence = new Dictionary<CannabisStrainType, float>(baseAllele.StrainTypeInfluence),
                CreationDate = DateTime.Now,
                Origin = $"Variation of {baseAllele.AlleleId}"
            };

            // Clamp values to valid ranges
            variant.Dominance = Mathf.Clamp01(variant.Dominance);
            variant.Expression = Mathf.Clamp01(variant.Expression);

            return variant;
        }

        /// <summary>
        /// Apply controlled variation to a float value
        /// </summary>
        private float ApplyVariation(float baseValue, float maxVariation)
        {
            float variation = UnityEngine.Random.Range(-maxVariation, maxVariation) * _variationIntensity;
            return baseValue + variation;
        }

        /// <summary>
        /// Apply color variation with controlled intensity
        /// </summary>
        private Color VaryColor(Color baseColor, float maxVariation)
        {
            if (baseColor == Color.clear) return baseColor;

            float hue, saturation, value;
            Color.RGBToHSV(baseColor, out hue, out saturation, out value);

            hue += UnityEngine.Random.Range(-maxVariation, maxVariation);
            saturation += UnityEngine.Random.Range(-maxVariation * 0.5f, maxVariation * 0.5f);
            value += UnityEngine.Random.Range(-maxVariation * 0.5f, maxVariation * 0.5f);

            hue = Mathf.Repeat(hue, 1f);
            saturation = Mathf.Clamp01(saturation);
            value = Mathf.Clamp01(value);

            return Color.HSVToRGB(hue, saturation, value);
        }

        #endregion

        #region Mutation System

        /// <summary>
        /// Apply mutations to all alleles in a genotype
        /// </summary>
        public void ApplyMutations(CannabisGenotype genotype)
        {
            if (!_isInitialized || genotype == null || !_enableMutations)
                return;

            foreach (var geneAlleles in genotype.Alleles)
            {
                foreach (var alleleObj in geneAlleles.Value)
                {
                    if (alleleObj is Allele allele)
                    {
                        if (UnityEngine.Random.value < _mutationRate)
                        {
                            ApplyMutation(allele);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply a random mutation to an allele
        /// </summary>
        public void ApplyMutation(Allele allele)
        {
            if (allele == null) return;

            var mutationType = SelectMutationType();
            
            switch (mutationType)
            {
                case MutationType.PointMutation:
                    ApplyPointMutation(allele);
                    break;
                case MutationType.DominanceShift:
                    ApplyDominanceShift(allele);
                    break;
                case MutationType.StabilityChange:
                    ApplyStabilityChange(allele);
                    break;
                case MutationType.ColorMutation:
                    ApplyColorMutation(allele);
                    break;
            }

            // Record mutation
            allele.Mutations.Add($"{mutationType}_{DateTime.Now:yyyyMMdd_HHmmss}");
            _totalMutations++;
            
            OnMutationApplied?.Invoke(allele, mutationType);

            if (_enableLogging)
                Debug.Log($"Mutation applied to allele {allele.AlleleId}: {mutationType}");
        }

        /// <summary>
        /// Select mutation type based on configured probabilities
        /// </summary>
        private MutationType SelectMutationType()
        {
            float random = UnityEngine.Random.value;
            float cumulative = 0f;

            cumulative += _pointMutationRate;
            if (random < cumulative) return MutationType.PointMutation;

            cumulative += _dominanceShiftRate;
            if (random < cumulative) return MutationType.DominanceShift;

            cumulative += _stabilityChangeRate;
            if (random < cumulative) return MutationType.StabilityChange;

            return MutationType.ColorMutation;
        }

        /// <summary>
        /// Apply point mutation to allele expression
        /// </summary>
        private void ApplyPointMutation(Allele allele)
        {
            float mutationStrength = UnityEngine.Random.Range(0.05f, 0.15f);
            allele.Expression += UnityEngine.Random.Range(-mutationStrength, mutationStrength);
            allele.Expression = Mathf.Clamp01(allele.Expression);
        }

        /// <summary>
        /// Apply dominance shift mutation
        /// </summary>
        private void ApplyDominanceShift(Allele allele)
        {
            float shiftStrength = UnityEngine.Random.Range(0.1f, 0.3f);
            allele.Dominance += UnityEngine.Random.Range(-shiftStrength, shiftStrength);
            allele.Dominance = Mathf.Clamp01(allele.Dominance);
        }

        /// <summary>
        /// Apply stability change mutation
        /// </summary>
        private void ApplyStabilityChange(Allele allele)
        {
            float stabilityChange = UnityEngine.Random.Range(-0.1f, 0.1f);
            allele.Stability += stabilityChange;
            allele.Stability = Mathf.Clamp(allele.Stability, 0.1f, 1f);
        }

        /// <summary>
        /// Apply color mutation to allele
        /// </summary>
        private void ApplyColorMutation(Allele allele)
        {
            if (allele.ColorValue != Color.clear)
            {
                allele.ColorValue = VaryColor(allele.ColorValue, 0.2f); // Stronger color variation for mutations
            }
        }

        #endregion

        #region Environmental Pressure

        /// <summary>
        /// Apply environmental pressure to modify allele characteristics
        /// </summary>
        public void ApplyEnvironmentalPressure(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            if (!_isInitialized || genotype == null || conditions == null || !_enableEnvironmentalPressure)
                return;

            foreach (var geneAlleles in genotype.Alleles)
            {
                var environmentalFactor = CalculateEnvironmentalFactor(geneAlleles.Key, conditions);
                
                foreach (var alleleObj in geneAlleles.Value)
                {
                    if (alleleObj is Allele allele)
                    {
                        ApplyEnvironmentalModification(allele, environmentalFactor);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate environmental factor for a gene
        /// </summary>
        private float CalculateEnvironmentalFactor(string geneId, EnvironmentalConditions conditions)
        {
            float factor = 0.5f; // Neutral baseline

            // Temperature effects
            if (conditions.Temperature >= 20f && conditions.Temperature <= 28f)
                factor += 0.2f; // Favorable temperature
            else
                factor -= 0.3f; // Stress temperature

            // Humidity effects
            if (conditions.Humidity >= 50f && conditions.Humidity <= 70f)
                factor += 0.1f; // Favorable humidity
            else
                factor -= 0.2f; // Stress humidity

            // Light intensity effects
            if (conditions.LightIntensity >= 600f && conditions.LightIntensity <= 1000f)
                factor += 0.1f; // Favorable light
            else if (conditions.LightIntensity < 300f)
                factor -= 0.3f; // Too little light

            return Mathf.Clamp01(factor);
        }

        /// <summary>
        /// Apply environmental modifications to an allele
        /// </summary>
        private void ApplyEnvironmentalModification(Allele allele, float environmentalFactor)
        {
            // Favorable conditions increase expression and stability
            if (environmentalFactor > 0.7f)
            {
                allele.Expression *= 1.05f;
                allele.Stability *= 1.02f;
            }
            // Harsh conditions decrease expression and stability
            else if (environmentalFactor < 0.3f)
            {
                allele.Expression *= 0.95f;
                allele.Stability *= 0.98f;
            }

            // Clamp values
            allele.Expression = Mathf.Clamp01(allele.Expression);
            allele.Stability = Mathf.Clamp(allele.Stability, 0.1f, 1f);
        }

        #endregion

        #region Performance Tracking

        private void UpdatePerformanceTracking()
        {
            float currentTime = Time.time;
            if (currentTime - _lastUpdateTime >= 1f)
            {
                _lastUpdateTime = currentTime;
                // Performance metrics updated every second
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Update service settings at runtime
        /// </summary>
        public void UpdateSettings(bool enableMutations, float mutationRate, bool enableEnvironmentalPressure, float variationIntensity)
        {
            _enableMutations = enableMutations;
            _mutationRate = mutationRate;
            _enableEnvironmentalPressure = enableEnvironmentalPressure;
            _variationIntensity = variationIntensity;

            if (_enableLogging)
                Debug.Log($"GeneticAlgorithmService settings updated: mutations={enableMutations}, rate={mutationRate}, environmental={enableEnvironmentalPressure}, variation={variationIntensity}");
        }

        /// <summary>
        /// Get service performance metrics
        /// </summary>
        public GeneticAlgorithmMetrics GetMetrics()
        {
            return new GeneticAlgorithmMetrics
            {
                TotalMutations = _totalMutations,
                TotalVariations = _totalVariations,
                MutationRate = _mutationRate,
                VariationIntensity = _variationIntensity,
                LastUpdateTime = DateTime.Now
            };
        }

        /// <summary>
        /// Generate a founder genotype for a new strain
        /// </summary>
        public CannabisGenotype GenerateFounderGenotype(string strainName)
        {
            try
            {
                var genotype = new CannabisGenotype(strainName, 1);
                genotype.IsFounderStrain = true;
                genotype.Origin = GenotypeOrigin.Natural;
                
                // Apply some random variation to make each founder unique
                ApplyRandomVariation(genotype, _baseVariationIntensity);
                
                return genotype;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating founder genotype: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Apply genetic modifications to a genotype
        /// </summary>
        public CannabisGenotype ApplyGeneticModifications(CannabisGenotype genotype, Dictionary<string, float> modifications)
        {
            if (genotype == null || modifications == null || modifications.Count == 0)
                return genotype;

            try
            {
                var modifiedGenotype = new CannabisGenotype(genotype.StrainName, genotype.Generation);
                
                // Copy existing traits
                foreach (var trait in genotype.Traits)
                {
                    modifiedGenotype.AddTrait(trait.TraitName, trait.ExpressedValue, trait.Dominance);
                }

                // Apply modifications
                foreach (var modification in modifications)
                {
                    var existingTrait = modifiedGenotype.GetTrait(modification.Key);
                    if (existingTrait != null)
                    {
                        existingTrait.ExpressedValue = Mathf.Clamp(modification.Value, 0f, 2f);
                    }
                    else
                    {
                        modifiedGenotype.AddTrait(modification.Key, modification.Value);
                    }
                }

                return modifiedGenotype;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying genetic modifications: {ex.Message}");
                return genotype;
            }
        }

        /// <summary>
        /// Apply random variation to a genotype for founder strain diversity
        /// </summary>
        private void ApplyRandomVariation(CannabisGenotype genotype, float intensity)
        {
            if (genotype == null || intensity <= 0f) return;

            try
            {
                // Apply variation to existing traits
                foreach (var trait in genotype.Traits)
                {
                    float variation = UnityEngine.Random.Range(-intensity, intensity);
                    trait.ExpressedValue = Mathf.Clamp(trait.ExpressedValue + variation, 0f, 2f);
                    
                    // Small chance for discrete dominance variation
                    if (UnityEngine.Random.value < 0.1f) // 10% chance to change dominance
                    {
                        var dominanceValues = System.Enum.GetValues(typeof(TraitDominance));
                        trait.Dominance = (TraitDominance)dominanceValues.GetValue(UnityEngine.Random.Range(0, dominanceValues.Length));
                    }
                }

                // Add some random base traits if genotype has few traits
                if (genotype.Traits.Count < 5)
                {
                    var baseTraits = new string[] { "Height", "YieldPotential", "FloweringTime", "THCContent", "CBDContent" };
                    foreach (var traitName in baseTraits)
                    {
                        if (genotype.GetTrait(traitName) == null)
                        {
                            float baseValue = UnityEngine.Random.Range(0.5f, 1.5f);
                            TraitDominance dominance = (TraitDominance)UnityEngine.Random.Range(0, 4); // Random dominance type
                            genotype.AddTrait(traitName, baseValue, dominance);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying random variation: {ex.Message}");
            }
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
    /// Performance metrics for genetic algorithm service
    /// </summary>
    [System.Serializable]
    public class GeneticAlgorithmMetrics
    {
        public int TotalMutations;
        public int TotalVariations;
        public float MutationRate;
        public float VariationIntensity;
        public DateTime LastUpdateTime;
    }

    /// <summary>
    /// Mutation types for genetic algorithm
    /// </summary>
    public enum MutationType
    {
        PointMutation,
        DominanceShift,
        StabilityChange,
        ColorMutation
    }
}