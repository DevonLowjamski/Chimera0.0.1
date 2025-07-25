using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Phenotype Expression Service - Handles phenotype calculation, trait expression, and environmental modifications
    /// Extracted from CannabisGeneticsEngine to provide focused phenotype expression functionality
    /// Manages phenotype calculation from genotypes, environmental trait modifications, and expression analytics
    /// Implements scientific trait expression with environmental interactions and phenotypic plasticity
    /// </summary>
    public class PhenotypeExpressionService : MonoBehaviour
    {
        [Header("Expression Configuration")]
        [SerializeField] private bool _enableEnvironmentalModifications = true;
        [SerializeField] private bool _enablePhenotypicPlasticity = true;
        [SerializeField] private bool _enableTraitInteractions = true;
        [SerializeField] private bool _enableExpressionLogging = false;

        [Header("Expression Parameters")]
        [SerializeField] private float _environmentalInfluence = 0.3f;
        [SerializeField] private float _plasticityFactor = 0.2f;
        [SerializeField] private float _traitInteractionStrength = 0.1f;
        [SerializeField] private float _expressionVariability = 0.05f;

        [Header("Environmental Response")]
        [SerializeField] private bool _enableTemperatureResponse = true;
        [SerializeField] private bool _enableLightResponse = true;
        [SerializeField] private bool _enableHumidityResponse = true;
        [SerializeField] private bool _enableNutrientResponse = true;

        [Header("Expression Analytics")]
        [SerializeField] private bool _enableExpressionAnalytics = true;
        [SerializeField] private bool _enableTraitCorrelations = true;
        [SerializeField] private bool _enableExpressionTracking = true;
        [SerializeField] private int _maxExpressionHistory = 500;

        // Service state
        private bool _isInitialized = false;
        private ScriptableObject _geneLibrary;
        private ScriptableObject _expressionConfig;

        // Expression data
        private Dictionary<string, CannabisPhenotype> _phenotypeCache = new Dictionary<string, CannabisPhenotype>();
        private Dictionary<string, List<PhenotypeExpression>> _expressionHistory = new Dictionary<string, List<PhenotypeExpression>>();
        private Dictionary<string, TraitCorrelationData> _traitCorrelations = new Dictionary<string, TraitCorrelationData>();

        // Environmental response mappings
        private Dictionary<string, EnvironmentalResponseCurve> _environmentalResponses = new Dictionary<string, EnvironmentalResponseCurve>();
        private Dictionary<string, Dictionary<string, float>> _traitInteractionMatrix = new Dictionary<string, Dictionary<string, float>>();

        // Analytics data
        private PhenotypeExpressionAnalytics _expressionAnalytics = new PhenotypeExpressionAnalytics();
        private Dictionary<string, float> _traitDistributions = new Dictionary<string, float>();
        private Dictionary<string, float> _environmentalEffects = new Dictionary<string, float>();

        // Performance tracking
        private int _totalExpressions = 0;
        private int _cacheHits = 0;
        private int _cacheMisses = 0;
        private float _averageExpressionTime = 0f;

        // Events
        public static event Action<CannabisPhenotype> OnPhenotypeExpressed;
        public static event Action<CannabisPhenotype> OnPhenotypeCalculated;
        public static event Action<string, float> OnTraitExpressed;
        public static event Action<EnvironmentalModification> OnEnvironmentalModification;
        public static event Action<PhenotypeExpressionAnalytics> OnExpressionAnalyticsUpdated;
        public static event Action<string> OnExpressionError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Phenotype Expression Service";
        public int TotalExpressions => _totalExpressions;
        public int CachedPhenotypes => _phenotypeCache.Count;
        public float CacheHitRate => _totalExpressions > 0 ? (float)_cacheHits / _totalExpressions : 0f;
        public int TrackedGenotypes => _expressionHistory.Count;

        public void Initialize(ScriptableObject geneLibrary = null, ScriptableObject expressionConfig = null)
        {
            InitializeService(geneLibrary, expressionConfig);
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
                UpdateExpressionAnalytics();
                UpdateTraitCorrelations();
            }
        }

        private void InitializeDataStructures()
        {
            _phenotypeCache = new Dictionary<string, CannabisPhenotype>();
            _expressionHistory = new Dictionary<string, List<PhenotypeExpression>>();
            _traitCorrelations = new Dictionary<string, TraitCorrelationData>();
            _environmentalResponses = new Dictionary<string, EnvironmentalResponseCurve>();
            _traitInteractionMatrix = new Dictionary<string, Dictionary<string, float>>();
            _expressionAnalytics = new PhenotypeExpressionAnalytics();
            _traitDistributions = new Dictionary<string, float>();
            _environmentalEffects = new Dictionary<string, float>();
        }

        public void InitializeService(ScriptableObject geneLibrary = null, ScriptableObject expressionConfig = null)
        {
            if (_isInitialized)
            {
                if (_enableExpressionLogging)
                    Debug.LogWarning("PhenotypeExpressionService already initialized");
                return;
            }

            try
            {
                _geneLibrary = geneLibrary;
                _expressionConfig = expressionConfig;

                InitializeEnvironmentalResponses();
                InitializeTraitInteractions();
                InitializeExpressionAnalytics();
                
                _isInitialized = true;
                
                if (_enableExpressionLogging)
                    Debug.Log("PhenotypeExpressionService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize PhenotypeExpressionService: {ex.Message}");
                OnExpressionError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Save expression analytics
                if (_enableExpressionAnalytics)
                {
                    SaveExpressionData();
                }
                
                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableExpressionLogging)
                    Debug.Log("PhenotypeExpressionService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during PhenotypeExpressionService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Phenotype Expression

        /// <summary>
        /// Calculate phenotype from genotype with environmental conditions (alias for ExpressPhenotype)
        /// </summary>
        public CannabisPhenotype CalculatePhenotype(CannabisGenotype genotype, EnvironmentalConditions? conditions = null)
        {
            return ExpressPhenotype(genotype, conditions);
        }

        /// <summary>
        /// Calculate phenotype from genotype with environmental conditions
        /// </summary>
        public CannabisPhenotype ExpressPhenotype(CannabisGenotype genotype, EnvironmentalConditions? conditions = null)
        {
            if (!_isInitialized || genotype == null)
            {
                OnExpressionError?.Invoke("Invalid genotype or service not initialized");
                return null;
            }

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableExpressionLogging)
                    Debug.Log($"Expressing phenotype for genotype: {genotype.StrainName}");

                // Check cache first
                var cacheKey = GenerateCacheKey(genotype, conditions);
                if (_phenotypeCache.TryGetValue(cacheKey, out var cachedPhenotype))
                {
                    _cacheHits++;
                    return cachedPhenotype;
                }
                _cacheMisses++;

                // Calculate base phenotype from genotype
                var phenotype = CalculateBasePhenotype(genotype);
                
                // Apply environmental modifications if conditions provided
                if (conditions != null && _enableEnvironmentalModifications)
                {
                    ApplyEnvironmentalModifications(phenotype, conditions);
                }

                // Apply phenotypic plasticity
                if (_enablePhenotypicPlasticity)
                {
                    ApplyPhenotypicPlasticity(phenotype, genotype, conditions);
                }

                // Apply trait interactions
                if (_enableTraitInteractions)
                {
                    ApplyTraitInteractions(phenotype);
                }

                // Calculate expression time
                var expressionTime = (float)(DateTime.Now - startTime).TotalSeconds;
                
                // Cache the result
                _phenotypeCache[cacheKey] = phenotype;
                
                // Record expression
                RecordPhenotypeExpression(genotype, phenotype, conditions, expressionTime);
                
                // Update analytics
                _totalExpressions++;
                _averageExpressionTime = (_averageExpressionTime * (_totalExpressions - 1) + expressionTime) / _totalExpressions;

                // Fire events
                OnPhenotypeExpressed?.Invoke(phenotype);
                
                if (_enableExpressionLogging)
                    Debug.Log($"Phenotype expression completed for {genotype.StrainName} in {expressionTime:F3} seconds");

                return phenotype;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during phenotype expression: {ex.Message}");
                OnExpressionError?.Invoke($"Phenotype expression failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculate base phenotype from genotype
        /// </summary>
        private CannabisPhenotype CalculateBasePhenotype(CannabisGenotype genotype)
        {
            var phenotype = new CannabisPhenotype
            {
                PhenotypeId = Guid.NewGuid().ToString(),
                GenotypeId = genotype.GenotypeId,
                ExpressionDate = DateTime.Now,
                MorphologicalTraits = new Dictionary<string, float>(),
                ColorTraits = new Dictionary<string, Color>(),
                GrowthTraits = new Dictionary<string, float>(),
                EnvironmentalTraits = new Dictionary<string, float>(),
                BiochemicalTraits = new Dictionary<string, float>()
            };

            // Process alleles if available
            if (genotype.Alleles != null)
            {
                foreach (var geneAlleles in genotype.Alleles)
                {
                    var geneId = geneAlleles.Key;
                    var alleles = geneAlleles.Value.OfType<Allele>().ToList();
                    
                    if (alleles.Count > 0)
                    {
                        var dominantAllele = GetDominantAllele(alleles);
                        ApplyAlleleToPhenotype(phenotype, dominantAllele, geneId);
                    }
                }
            }

            // Process genotype traits if available
            if (genotype.Traits != null)
            {
                foreach (var trait in genotype.Traits)
                {
                    ApplyGenotypeTraitToPhenotype(phenotype, trait);
                }
            }

            // Calculate overall vigor and stress resistance
            CalculateOverallTraits(phenotype);

            return phenotype;
        }

        /// <summary>
        /// Get dominant allele from a list of alleles
        /// </summary>
        private Allele GetDominantAllele(List<Allele> alleles)
        {
            return alleles.OrderByDescending(a => a.Dominance * a.Expression).FirstOrDefault();
        }

        /// <summary>
        /// Apply allele effects to phenotype
        /// </summary>
        private void ApplyAlleleToPhenotype(CannabisPhenotype phenotype, Allele allele, string geneId)
        {
            // Map gene IDs to trait categories and apply effects
            switch (geneId.ToLower())
            {
                case "height":
                case "plant_height":
                    phenotype.MorphologicalTraits["Height"] = allele.Expression;
                    break;
                    
                case "leaf_size":
                case "leafsize":
                    phenotype.MorphologicalTraits["LeafSize"] = allele.Expression;
                    break;
                    
                case "bud_density":
                case "buddensity":
                    phenotype.MorphologicalTraits["BudDensity"] = allele.Expression;
                    break;
                    
                case "stem_thickness":
                case "stemthickness":
                    phenotype.MorphologicalTraits["StemThickness"] = allele.Expression;
                    break;
                    
                case "branch_density":
                case "branchdensity":
                    phenotype.MorphologicalTraits["BranchDensity"] = allele.Expression;
                    break;
                    
                case "flowering_time":
                case "floweringtime":
                    phenotype.GrowthTraits["FloweringTime"] = allele.Expression;
                    break;
                    
                case "growth_rate":
                case "growthrate":
                    phenotype.GrowthTraits["GrowthRate"] = allele.Expression;
                    break;
                    
                case "yield_potential":
                case "yieldpotential":
                    phenotype.GrowthTraits["YieldPotential"] = allele.Expression;
                    break;
                    
                case "thc_production":
                case "thcproduction":
                    phenotype.BiochemicalTraits["THCContent"] = allele.Expression;
                    break;
                    
                case "cbd_production":
                case "cbdproduction":
                    phenotype.BiochemicalTraits["CBDContent"] = allele.Expression;
                    break;
                    
                case "heat_tolerance":
                case "heattolerance":
                    phenotype.EnvironmentalTraits["HeatTolerance"] = allele.Expression;
                    break;
                    
                case "cold_tolerance":
                case "coldtolerance":
                    phenotype.EnvironmentalTraits["ColdTolerance"] = allele.Expression;
                    break;
                    
                case "drought_tolerance":
                case "droughttolerance":
                    phenotype.EnvironmentalTraits["DroughtTolerance"] = allele.Expression;
                    break;
            }

            // Apply color traits if allele has color value
            if (allele.ColorValue != Color.clear)
            {
                ApplyColorTraits(phenotype, allele, geneId);
            }

            // Fire trait expression event
            OnTraitExpressed?.Invoke(geneId, allele.Expression);
        }

        /// <summary>
        /// Apply genotype trait to phenotype
        /// </summary>
        private void ApplyGenotypeTraitToPhenotype(CannabisPhenotype phenotype, GeneticTrait trait)
        {
            var traitName = trait.TraitName.ToLower();
            var expressedValue = trait.ExpressedValue;

            // Map trait names to phenotype categories
            switch (traitName)
            {
                case "growth_rate":
                    phenotype.GrowthTraits["GrowthRate"] = expressedValue;
                    break;
                case "plant_height":
                    phenotype.MorphologicalTraits["Height"] = expressedValue;
                    break;
                case "leaf_size":
                    phenotype.MorphologicalTraits["LeafSize"] = expressedValue;
                    break;
                case "branch_density":
                    phenotype.MorphologicalTraits["BranchDensity"] = expressedValue;
                    break;
                case "flowering_time":
                    phenotype.GrowthTraits["FloweringTime"] = expressedValue;
                    break;
                case "yield_potential":
                    phenotype.GrowthTraits["YieldPotential"] = expressedValue;
                    break;
                case "thc_production":
                    phenotype.BiochemicalTraits["THCContent"] = expressedValue;
                    break;
                case "cbd_production":
                    phenotype.BiochemicalTraits["CBDContent"] = expressedValue;
                    break;
                case "disease_resistance":
                    phenotype.EnvironmentalTraits["DiseaseResistance"] = expressedValue;
                    break;
                case "stress_tolerance":
                    phenotype.EnvironmentalTraits["StressTolerance"] = expressedValue;
                    break;
            }
        }

        /// <summary>
        /// Apply color traits from allele
        /// </summary>
        private void ApplyColorTraits(CannabisPhenotype phenotype, Allele allele, string geneId)
        {
            switch (geneId.ToLower())
            {
                case "leaf_color":
                case "leafcolor":
                    phenotype.ColorTraits["LeafColor"] = allele.ColorValue;
                    break;
                case "bud_color":
                case "budcolor":
                    phenotype.ColorTraits["BudColor"] = allele.ColorValue;
                    break;
                case "stem_color":
                case "stemcolor":
                    phenotype.ColorTraits["StemColor"] = allele.ColorValue;
                    break;
                default:
                    phenotype.ColorTraits[$"{geneId}_Color"] = allele.ColorValue;
                    break;
            }
        }

        /// <summary>
        /// Calculate overall traits like vigor and stress resistance
        /// </summary>
        private void CalculateOverallTraits(CannabisPhenotype phenotype)
        {
            // Calculate overall vigor from growth traits
            var vigorTraits = new[] { "GrowthRate", "YieldPotential" };
            float vigorSum = 0f;
            int vigorCount = 0;
            
            foreach (var traitName in vigorTraits)
            {
                if (phenotype.GrowthTraits.TryGetValue(traitName, out float value))
                {
                    vigorSum += value;
                    vigorCount++;
                }
            }
            phenotype.OverallVigor = vigorCount > 0 ? vigorSum / vigorCount : 1.0f;

            // Calculate stress resistance from environmental traits
            var resistanceTraits = new[] { "HeatTolerance", "ColdTolerance", "DroughtTolerance", "DiseaseResistance", "StressTolerance" };
            float resistanceSum = 0f;
            int resistanceCount = 0;
            
            foreach (var traitName in resistanceTraits)
            {
                if (phenotype.EnvironmentalTraits.TryGetValue(traitName, out float value))
                {
                    resistanceSum += value;
                    resistanceCount++;
                }
            }
            phenotype.StressResistance = resistanceCount > 0 ? resistanceSum / resistanceCount : 0.5f;
        }

        #endregion

        #region Environmental Modifications

        /// <summary>
        /// Apply environmental modifications to phenotype
        /// </summary>
        private void ApplyEnvironmentalModifications(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            phenotype.ExpressionEnvironment = conditions;

            if (_enableTemperatureResponse)
            {
                ApplyTemperatureEffects(phenotype, conditions);
            }

            if (_enableLightResponse)
            {
                ApplyLightEffects(phenotype, conditions);
            }

            if (_enableHumidityResponse)
            {
                ApplyHumidityEffects(phenotype, conditions);
            }

            if (_enableNutrientResponse)
            {
                ApplyNutrientEffects(phenotype, conditions);
            }

            // Record environmental modification
            var modification = new EnvironmentalModification
            {
                PhenotypeId = phenotype.PhenotypeId,
                Conditions = conditions,
                ModificationTime = DateTime.Now,
                EffectMagnitude = CalculateEnvironmentalEffectMagnitude(conditions)
            };

            OnEnvironmentalModification?.Invoke(modification);
        }

        /// <summary>
        /// Apply temperature effects to phenotype
        /// </summary>
        private void ApplyTemperatureEffects(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            var temperature = conditions.Temperature;
            var optimalTemp = 24f; // Optimal temperature for cannabis
            var tempDeviation = Math.Abs(temperature - optimalTemp) / 10f;

            // Temperature affects growth rate and morphology
            if (phenotype.GrowthTraits.ContainsKey("GrowthRate"))
            {
                var temperatureEffect = 1f - (tempDeviation * 0.2f * _environmentalInfluence);
                phenotype.GrowthTraits["GrowthRate"] *= temperatureEffect;
            }

            // Extreme temperatures affect leaf size and stem thickness
            if (temperature > 30f) // High temperature
            {
                ModifyTrait(phenotype.MorphologicalTraits, "LeafSize", -0.1f * _environmentalInfluence);
                ModifyTrait(phenotype.MorphologicalTraits, "StemThickness", 0.05f * _environmentalInfluence);
            }
            else if (temperature < 18f) // Low temperature
            {
                ModifyTrait(phenotype.GrowthTraits, "FloweringTime", 0.1f * _environmentalInfluence);
                ModifyTrait(phenotype.MorphologicalTraits, "StemThickness", 0.1f * _environmentalInfluence);
            }
        }

        /// <summary>
        /// Apply light effects to phenotype
        /// </summary>
        private void ApplyLightEffects(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            var lightIntensity = conditions.LightIntensity;
            var optimalLight = 800f; // Optimal PPFD for cannabis
            var lightRatio = lightIntensity / optimalLight;

            // Light affects growth rate and bud density
            if (phenotype.GrowthTraits.ContainsKey("GrowthRate"))
            {
                var lightEffect = Mathf.Min(lightRatio, 1.2f) * _environmentalInfluence;
                phenotype.GrowthTraits["GrowthRate"] *= 1f + (lightEffect - _environmentalInfluence);
            }

            // High light intensity increases bud density and resin production
            if (lightRatio > 1.0f)
            {
                ModifyTrait(phenotype.MorphologicalTraits, "BudDensity", 0.15f * _environmentalInfluence);
                ModifyTrait(phenotype.BiochemicalTraits, "THCContent", 0.1f * _environmentalInfluence);
            }
            // Low light affects internode spacing and stem thickness
            else if (lightRatio < 0.7f)
            {
                ModifyTrait(phenotype.MorphologicalTraits, "InternodeSpacing", 0.2f * _environmentalInfluence);
                ModifyTrait(phenotype.MorphologicalTraits, "StemThickness", -0.1f * _environmentalInfluence);
            }
        }

        /// <summary>
        /// Apply humidity effects to phenotype
        /// </summary>
        private void ApplyHumidityEffects(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            var humidity = conditions.Humidity;
            var optimalHumidity = 60f;
            var humidityDeviation = Math.Abs(humidity - optimalHumidity) / 20f;

            // Humidity affects leaf density and disease resistance
            if (phenotype.MorphologicalTraits.ContainsKey("LeafDensity"))
            {
                var humidityEffect = 1f - (humidityDeviation * 0.15f * _environmentalInfluence);
                phenotype.MorphologicalTraits["LeafDensity"] *= humidityEffect;
            }

            // High humidity can reduce disease resistance
            if (humidity > 70f)
            {
                ModifyTrait(phenotype.EnvironmentalTraits, "DiseaseResistance", -0.1f * _environmentalInfluence);
            }
            // Low humidity affects leaf size
            else if (humidity < 40f)
            {
                ModifyTrait(phenotype.MorphologicalTraits, "LeafSize", -0.05f * _environmentalInfluence);
            }
        }

        /// <summary>
        /// Apply nutrient effects to phenotype
        /// </summary>
        private void ApplyNutrientEffects(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            // Note: EnvironmentalConditions might not have nutrient info, 
            // but we can simulate based on general conditions
            var generalHealth = (conditions.Temperature / 30f + conditions.Humidity / 100f + 
                               Mathf.Min(conditions.LightIntensity / 1000f, 1f)) / 3f;

            // Good conditions enhance all traits slightly
            if (generalHealth > 0.8f)
            {
                foreach (var trait in phenotype.GrowthTraits.Keys.ToList())
                {
                    ModifyTrait(phenotype.GrowthTraits, trait, 0.05f * _environmentalInfluence);
                }
            }
            // Poor conditions reduce growth traits
            else if (generalHealth < 0.5f)
            {
                foreach (var trait in phenotype.GrowthTraits.Keys.ToList())
                {
                    ModifyTrait(phenotype.GrowthTraits, trait, -0.1f * _environmentalInfluence);
                }
            }
        }

        /// <summary>
        /// Apply phenotypic plasticity based on environmental conditions
        /// </summary>
        private void ApplyPhenotypicPlasticity(CannabisPhenotype phenotype, CannabisGenotype genotype, EnvironmentalConditions? conditions)
        {
            if (conditions == null) return;

            // Calculate plasticity based on genetic diversity and environmental stress
            var geneticDiversity = CalculateGeneticDiversity(genotype);
            var environmentalStress = CalculateEnvironmentalStress(conditions);
            var plasticityMagnitude = geneticDiversity * environmentalStress * _plasticityFactor;

            // Apply plasticity to morphological traits
            foreach (var trait in phenotype.MorphologicalTraits.Keys.ToList())
            {
                var plasticityChange = UnityEngine.Random.Range(-plasticityMagnitude, plasticityMagnitude);
                ModifyTrait(phenotype.MorphologicalTraits, trait, plasticityChange);
            }

            // Apply plasticity to growth traits
            foreach (var trait in phenotype.GrowthTraits.Keys.ToList())
            {
                var plasticityChange = UnityEngine.Random.Range(-plasticityMagnitude * 0.5f, plasticityMagnitude * 0.5f);
                ModifyTrait(phenotype.GrowthTraits, trait, plasticityChange);
            }
        }

        /// <summary>
        /// Apply trait interactions within phenotype
        /// </summary>
        private void ApplyTraitInteractions(CannabisPhenotype phenotype)
        {
            // Example: High growth rate might reduce bud density
            if (phenotype.GrowthTraits.TryGetValue("GrowthRate", out float growthRate) && growthRate > 1.2f)
            {
                ModifyTrait(phenotype.MorphologicalTraits, "BudDensity", -0.1f * _traitInteractionStrength);
            }

            // Example: High leaf density might increase disease susceptibility
            if (phenotype.MorphologicalTraits.TryGetValue("LeafDensity", out float leafDensity) && leafDensity > 1.1f)
            {
                ModifyTrait(phenotype.EnvironmentalTraits, "DiseaseResistance", -0.05f * _traitInteractionStrength);
            }

            // Example: Thick stems support larger leaf size
            if (phenotype.MorphologicalTraits.TryGetValue("StemThickness", out float stemThickness) && stemThickness > 1.1f)
            {
                ModifyTrait(phenotype.MorphologicalTraits, "LeafSize", 0.05f * _traitInteractionStrength);
            }
        }

        #endregion

        #region Utility Methods

        private void ModifyTrait(Dictionary<string, float> traitDict, string traitName, float modification)
        {
            if (traitDict.ContainsKey(traitName))
            {
                traitDict[traitName] += modification;
                traitDict[traitName] = Mathf.Clamp(traitDict[traitName], 0.1f, 2.0f);
            }
            else
            {
                traitDict[traitName] = Mathf.Clamp(1.0f + modification, 0.1f, 2.0f);
            }
        }

        private string GenerateCacheKey(CannabisGenotype genotype, EnvironmentalConditions? conditions)
        {
            var baseKey = genotype.GenotypeId;
            if (conditions != null)
            {
                var conditionsHash = $"{conditions.Temperature:F1}_{conditions.Humidity:F1}_{conditions.LightIntensity:F0}";
                baseKey += $"_{conditionsHash}";
            }
            return baseKey;
        }

        private void RecordPhenotypeExpression(CannabisGenotype genotype, CannabisPhenotype phenotype, EnvironmentalConditions? conditions, float expressionTime)
        {
            if (!_enableExpressionTracking) return;

            var expression = new PhenotypeExpression
            {
                ExpressionId = Guid.NewGuid().ToString(),
                GenotypeId = genotype.GenotypeId,
                PhenotypeId = phenotype.PhenotypeId,
                ExpressionDate = DateTime.Now,
                EnvironmentalConditions = conditions,
                ExpressionTime = expressionTime,
                TraitCount = CountTotalTraits(phenotype)
            };

            if (!_expressionHistory.ContainsKey(genotype.GenotypeId))
            {
                _expressionHistory[genotype.GenotypeId] = new List<PhenotypeExpression>();
            }

            _expressionHistory[genotype.GenotypeId].Add(expression);

            // Limit history size
            if (_expressionHistory[genotype.GenotypeId].Count > _maxExpressionHistory)
            {
                _expressionHistory[genotype.GenotypeId].RemoveAt(0);
            }
        }

        private int CountTotalTraits(CannabisPhenotype phenotype)
        {
            return phenotype.MorphologicalTraits.Count +
                   phenotype.ColorTraits.Count +
                   phenotype.GrowthTraits.Count +
                   phenotype.EnvironmentalTraits.Count +
                   phenotype.BiochemicalTraits.Count;
        }

        private float CalculateGeneticDiversity(CannabisGenotype genotype)
        {
            if (genotype.Alleles == null || genotype.Alleles.Count == 0)
                return 0.5f;

            float diversitySum = 0f;
            int geneCount = 0;

            foreach (var geneAlleles in genotype.Alleles)
            {
                var alleles = geneAlleles.Value.OfType<Allele>().ToList();
                if (alleles.Count > 1)
                {
                    // Calculate diversity as variance in expression levels
                    var avgExpression = alleles.Average(a => a.Expression);
                    var variance = alleles.Sum(a => Mathf.Pow(a.Expression - avgExpression, 2)) / alleles.Count;
                    diversitySum += variance;
                }
                geneCount++;
            }

            return geneCount > 0 ? diversitySum / geneCount : 0.5f;
        }

        private float CalculateEnvironmentalStress(EnvironmentalConditions conditions)
        {
            float stressSum = 0f;
            int stressFactors = 0;

            // Temperature stress
            var tempStress = Math.Abs(conditions.Temperature - 24f) / 10f;
            stressSum += Mathf.Clamp01(tempStress);
            stressFactors++;

            // Light stress
            var lightStress = Math.Abs(conditions.LightIntensity - 800f) / 400f;
            stressSum += Mathf.Clamp01(lightStress);
            stressFactors++;

            // Humidity stress
            var humidityStress = Math.Abs(conditions.Humidity - 60f) / 30f;
            stressSum += Mathf.Clamp01(humidityStress);
            stressFactors++;

            return stressFactors > 0 ? stressSum / stressFactors : 0f;
        }

        private float CalculateEnvironmentalEffectMagnitude(EnvironmentalConditions conditions)
        {
            var stress = CalculateEnvironmentalStress(conditions);
            return stress * _environmentalInfluence;
        }

        private void UpdateExpressionAnalytics()
        {
            if (!_enableExpressionAnalytics) return;

            var currentTime = DateTime.Now;
            if (_expressionAnalytics.LastAnalyticsUpdate == default ||
                (currentTime - _expressionAnalytics.LastAnalyticsUpdate).TotalMinutes >= 1)
            {
                UpdateAnalyticsData();
                _expressionAnalytics.LastAnalyticsUpdate = currentTime;
                
                OnExpressionAnalyticsUpdated?.Invoke(_expressionAnalytics);
            }
        }

        private void UpdateAnalyticsData()
        {
            _expressionAnalytics.TotalExpressions = _totalExpressions;
            _expressionAnalytics.CacheHitRate = CacheHitRate;
            _expressionAnalytics.AverageExpressionTime = _averageExpressionTime;
            _expressionAnalytics.CachedPhenotypes = _phenotypeCache.Count;
            _expressionAnalytics.TrackedGenotypes = _expressionHistory.Count;
            
            // Update trait distributions
            UpdateTraitDistributions();
        }

        private void UpdateTraitDistributions()
        {
            _traitDistributions.Clear();
            
            foreach (var phenotype in _phenotypeCache.Values)
            {
                foreach (var trait in phenotype.MorphologicalTraits)
                {
                    _traitDistributions[$"Morphological_{trait.Key}"] = 
                        _traitDistributions.GetValueOrDefault($"Morphological_{trait.Key}", 0f) + trait.Value;
                }
                
                foreach (var trait in phenotype.GrowthTraits)
                {
                    _traitDistributions[$"Growth_{trait.Key}"] = 
                        _traitDistributions.GetValueOrDefault($"Growth_{trait.Key}", 0f) + trait.Value;
                }
            }
            
            // Normalize by phenotype count
            if (_phenotypeCache.Count > 0)
            {
                foreach (var key in _traitDistributions.Keys.ToList())
                {
                    _traitDistributions[key] /= _phenotypeCache.Count;
                }
            }
        }

        private void UpdateTraitCorrelations()
        {
            if (!_enableTraitCorrelations) return;
            
            // Update trait correlations periodically
            // This would implement correlation analysis between traits
        }

        private void InitializeEnvironmentalResponses()
        {
            // Initialize environmental response curves for different traits
            // This would load from configuration or set up default curves
        }

        private void InitializeTraitInteractions()
        {
            // Initialize trait interaction matrix
            // This would define how traits influence each other
        }

        private void InitializeExpressionAnalytics()
        {
            _expressionAnalytics = new PhenotypeExpressionAnalytics
            {
                TotalExpressions = 0,
                CacheHitRate = 0f,
                AverageExpressionTime = 0f,
                CachedPhenotypes = 0,
                TrackedGenotypes = 0,
                LastAnalyticsUpdate = DateTime.Now
            };
        }

        private void SaveExpressionData()
        {
            // Save expression analytics and history - would integrate with data persistence service
            if (_enableExpressionLogging)
                Debug.Log($"Saving expression data: {_phenotypeCache.Count} cached phenotypes, {_totalExpressions} total expressions");
        }

        private void ClearAllData()
        {
            _phenotypeCache.Clear();
            _expressionHistory.Clear();
            _traitCorrelations.Clear();
            _environmentalResponses.Clear();
            _traitInteractionMatrix.Clear();
            _traitDistributions.Clear();
            _environmentalEffects.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get cached phenotype for genotype and conditions
        /// </summary>
        public CannabisPhenotype GetCachedPhenotype(CannabisGenotype genotype, EnvironmentalConditions? conditions = null)
        {
            var cacheKey = GenerateCacheKey(genotype, conditions);
            return _phenotypeCache.TryGetValue(cacheKey, out var phenotype) ? phenotype : null;
        }

        /// <summary>
        /// Clear phenotype cache
        /// </summary>
        public void ClearPhenotypeCache()
        {
            _phenotypeCache.Clear();
            _cacheHits = 0;
            _cacheMisses = 0;
        }

        /// <summary>
        /// Get expression history for a genotype
        /// </summary>
        public List<PhenotypeExpression> GetExpressionHistory(string genotypeId)
        {
            return _expressionHistory.TryGetValue(genotypeId, out var history) ? 
                   new List<PhenotypeExpression>(history) : new List<PhenotypeExpression>();
        }

        /// <summary>
        /// Get expression analytics
        /// </summary>
        public PhenotypeExpressionAnalytics GetExpressionAnalytics()
        {
            return _expressionAnalytics;
        }

        /// <summary>
        /// Get trait distributions
        /// </summary>
        public Dictionary<string, float> GetTraitDistributions()
        {
            return new Dictionary<string, float>(_traitDistributions);
        }

        /// <summary>
        /// Update expression settings at runtime
        /// </summary>
        public void UpdateExpressionSettings(bool enableEnvironmental, bool enablePlasticity, float environmentalInfluence, float plasticityFactor)
        {
            _enableEnvironmentalModifications = enableEnvironmental;
            _enablePhenotypicPlasticity = enablePlasticity;
            _environmentalInfluence = environmentalInfluence;
            _plasticityFactor = plasticityFactor;
            
            if (_enableExpressionLogging)
                Debug.Log($"Expression settings updated: Environmental={enableEnvironmental}, Plasticity={enablePlasticity}, Influence={environmentalInfluence}, Plasticity={plasticityFactor}");
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
    /// Cannabis phenotype data structure
    /// </summary>
    [System.Serializable]
    public class CannabisPhenotype
    {
        public string PhenotypeId = "";
        public string GenotypeId = "";
        public Dictionary<string, float> MorphologicalTraits = new Dictionary<string, float>();
        public Dictionary<string, Color> ColorTraits = new Dictionary<string, Color>();
        public Dictionary<string, float> GrowthTraits = new Dictionary<string, float>();
        public Dictionary<string, float> EnvironmentalTraits = new Dictionary<string, float>();
        public Dictionary<string, float> BiochemicalTraits = new Dictionary<string, float>();
        public float OverallVigor = 1.0f;
        public float StressResistance = 0.5f;
        public DateTime ExpressionDate = DateTime.Now;
        public EnvironmentalConditions? ExpressionEnvironment;
        
        // Direct property accessors for commonly used traits
        public float PlantHeight => MorphologicalTraits.GetValueOrDefault("Height", 1.0f);
        public float StemThickness => MorphologicalTraits.GetValueOrDefault("StemThickness", 1.0f);
        public float BranchDensity => MorphologicalTraits.GetValueOrDefault("BranchDensity", 1.0f);
        public float LeafDensity => MorphologicalTraits.GetValueOrDefault("LeafDensity", 1.0f);
        public float InternodeSpacing => MorphologicalTraits.GetValueOrDefault("InternodeSpacing", 1.0f);
        
        public Color LeafColor => ColorTraits.GetValueOrDefault("LeafColor", Color.green);
        public Color BudColor => ColorTraits.GetValueOrDefault("BudColor", Color.green);
        public float ColorIntensity => MorphologicalTraits.GetValueOrDefault("ColorIntensity", 0.5f);
        public float ColorVariation => MorphologicalTraits.GetValueOrDefault("ColorVariation", 0.3f);
        
        public float GrowthRate => GrowthTraits.GetValueOrDefault("GrowthRate", 1.0f);
        public float FloweringTime => GrowthTraits.GetValueOrDefault("FloweringTime", 1.0f);
        public float YieldPotential => GrowthTraits.GetValueOrDefault("YieldPotential", 1.0f);
        
        public float HeatTolerance => EnvironmentalTraits.GetValueOrDefault("HeatTolerance", 0.5f);
        public float ColdTolerance => EnvironmentalTraits.GetValueOrDefault("ColdTolerance", 0.5f);
        public float DroughtTolerance => EnvironmentalTraits.GetValueOrDefault("DroughtTolerance", 0.5f);
    }

    /// <summary>
    /// Phenotype expression record
    /// </summary>
    [System.Serializable]
    public class PhenotypeExpression
    {
        public string ExpressionId = "";
        public string GenotypeId = "";
        public string PhenotypeId = "";
        public DateTime ExpressionDate = DateTime.Now;
        public EnvironmentalConditions? EnvironmentalConditions;
        public float ExpressionTime = 0f;
        public int TraitCount = 0;
    }

    /// <summary>
    /// Environmental modification record
    /// </summary>
    [System.Serializable]
    public class EnvironmentalModification
    {
        public string PhenotypeId = "";
        public EnvironmentalConditions Conditions = new EnvironmentalConditions();
        public DateTime ModificationTime = DateTime.Now;
        public float EffectMagnitude = 0f;
    }

    /// <summary>
    /// Trait correlation data
    /// </summary>
    [System.Serializable]
    public class TraitCorrelationData
    {
        public string Trait1 = "";
        public string Trait2 = "";
        public float CorrelationCoefficient = 0f;
        public int SampleSize = 0;
        public DateTime LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Environmental response curve
    /// </summary>
    [System.Serializable]
    public class EnvironmentalResponseCurve
    {
        public string TraitName = "";
        public string EnvironmentalFactor = "";
        public AnimationCurve ResponseCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float MinValue = 0f;
        public float MaxValue = 2f;
    }

    /// <summary>
    /// Phenotype expression analytics
    /// </summary>
    [System.Serializable]
    public class PhenotypeExpressionAnalytics
    {
        public int TotalExpressions = 0;
        public float CacheHitRate = 0f;
        public float AverageExpressionTime = 0f;
        public int CachedPhenotypes = 0;
        public int TrackedGenotypes = 0;
        public DateTime LastAnalyticsUpdate = DateTime.Now;
    }
}