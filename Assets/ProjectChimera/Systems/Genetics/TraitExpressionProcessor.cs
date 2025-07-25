using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using TraitExpressionResult = ProjectChimera.Systems.Genetics.TraitExpressionResult;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC013-7b: Extracted trait expression processor from GeneticsManager
    /// Handles trait expression calculations, environmental interactions, and phenotype prediction.
    /// Coordinates with the existing TraitExpressionEngine for computation while providing
    /// high-level trait expression management and caching.
    /// </summary>
    public class TraitExpressionProcessor
    {
        private readonly bool _enableEpistasis;
        private readonly bool _enablePleiotropy;
        private readonly bool _enableDetailedLogging;
        private readonly bool _enableEnvironmentalCaching;
        
        private TraitExpressionEngine _traitExpressionEngine;
        private Dictionary<string, TraitExpressionResult> _expressionCache;
        private Dictionary<string, EnvironmentalModifier> _environmentalModifiers;
        private float _cacheExpiryTime = 300f; // 5 minutes
        
        // Events for trait expression operations
        public System.Action<PlantInstanceSO> OnTraitExpressionCalculated;
        public System.Action<List<PlantInstanceSO>> OnBatchExpressionCompleted;
        public System.Action<string, float> OnTraitValueUpdated;
        
        public TraitExpressionProcessor(bool enableEpistasis = true, bool enablePleiotropy = true, 
            bool enableDetailedLogging = false, bool enableEnvironmentalCaching = true)
        {
            _enableEpistasis = enableEpistasis;
            _enablePleiotropy = enablePleiotropy;
            _enableDetailedLogging = enableDetailedLogging;
            _enableEnvironmentalCaching = enableEnvironmentalCaching;
            
            _expressionCache = new Dictionary<string, TraitExpressionResult>();
            _environmentalModifiers = new Dictionary<string, EnvironmentalModifier>();
            
            // Initialize the underlying trait expression engine
            _traitExpressionEngine = new TraitExpressionEngine(_enableEpistasis, _enablePleiotropy);
            
            Debug.Log($"[TraitExpressionProcessor] Initialized with epistasis: {_enableEpistasis}, pleiotropy: {_enablePleiotropy}");
        }
        
        /// <summary>
        /// Calculates trait expression for a plant based on its genotype and environment.
        /// </summary>
        public TraitExpressionResult CalculateTraitExpression(PlantInstanceSO plant, EnvironmentalConditions environment)
        {
            if (plant == null)
            {
                Debug.LogError("[TraitExpressionProcessor] Cannot calculate trait expression: plant is null");
                return null;
            }
            
            // Get plant genotype
            var genotype = GetPlantGenotype(plant);
            if (genotype == null)
            {
                Debug.LogError($"[TraitExpressionProcessor] Cannot calculate trait expression: no genotype for plant {plant.PlantID}");
                return CreateDefaultTraitExpression(plant);
            }
            
            // Check cache first
            string cacheKey = GenerateExpressionCacheKey(plant.PlantID, environment);
            if (_enableEnvironmentalCaching && _expressionCache.TryGetValue(cacheKey, out var cachedResult))
            {
                if (_enableDetailedLogging)
                    Debug.Log($"[TraitExpressionProcessor] Using cached trait expression for plant {plant.PlantID}");
                return cachedResult;
            }
            
            // Apply environmental modifiers
            var modifiedEnvironment = ApplyEnvironmentalModifiers(environment, plant);
            
            // Calculate expression using the underlying engine
            var expressionResult = _traitExpressionEngine.CalculateExpression(genotype, modifiedEnvironment);
            
            // Post-process results
            PostProcessTraitExpression(expressionResult, plant, modifiedEnvironment);
            
            // Cache result
            if (_enableEnvironmentalCaching)
            {
                _expressionCache[cacheKey] = expressionResult;
            }
            
            if (_enableDetailedLogging)
                Debug.Log($"[TraitExpressionProcessor] Calculated trait expression for plant {plant.PlantID}");
            
            OnTraitExpressionCalculated?.Invoke(plant);
            
            return expressionResult;
        }
        
        /// <summary>
        /// Calculates trait expression for multiple plants in batch for efficiency.
        /// </summary>
        public List<TraitExpressionResult> CalculateBatchTraitExpression(List<PlantInstanceSO> plants, 
            EnvironmentalConditions environment)
        {
            if (plants == null || plants.Count == 0)
            {
                Debug.LogWarning("[TraitExpressionProcessor] Cannot calculate batch trait expression: empty plant list");
                return new List<TraitExpressionResult>();
            }
            
            var results = new List<TraitExpressionResult>();
            var batchData = new List<(PlantGenotype, EnvironmentalConditions)>();
            
            // Prepare batch data
            foreach (var plant in plants)
            {
                var genotype = GetPlantGenotype(plant);
                if (genotype != null)
                {
                    var modifiedEnvironment = ApplyEnvironmentalModifiers(environment, plant);
                    batchData.Add((genotype, modifiedEnvironment));
                }
                else
                {
                    results.Add(CreateDefaultTraitExpression(plant));
                }
            }
            
            // Process batch using the underlying engine
            if (batchData.Count > 0)
            {
                var batchResults = _traitExpressionEngine.CalculateExpressionBatch(batchData);
                
                // Post-process batch results
                for (int i = 0; i < batchResults.Count && i < plants.Count; i++)
                {
                    PostProcessTraitExpression(batchResults[i], plants[i], environment);
                    results.Add(batchResults[i]);
                }
            }
            
            OnBatchExpressionCompleted?.Invoke(plants);
            
            Debug.Log($"[TraitExpressionProcessor] Completed batch trait expression for {results.Count} plants");
            return results;
        }
        
        /// <summary>
        /// Predicts phenotype based on genotype and environmental conditions.
        /// </summary>
        public PhenotypeProjection PredictPhenotype(PlantGenotype genotype, EnvironmentalConditions environment, 
            int daysToPredict = 90)
        {
            if (genotype == null)
            {
                Debug.LogError("[TraitExpressionProcessor] Cannot predict phenotype: genotype is null");
                return null;
            }
            
            var projection = new PhenotypeProjection
            {
                GenotypeID = genotype.GenotypeID,
                PredictionDays = daysToPredict,
                BaseEnvironment = environment,
                TraitProjections = new Dictionary<TraitType, TraitProjection>(),
                PredictionDate = System.DateTime.Now
            };
            
            // Calculate base trait expression
            var baseExpression = _traitExpressionEngine.CalculateExpression(genotype, environment);
            
            // Project key traits over time
            var keyTraits = new[] { TraitType.Height, TraitType.Yield, TraitType.Quality, 
                                   TraitType.THCContent, TraitType.CBDContent, TraitType.GrowthVigor };
            
            foreach (var trait in keyTraits)
            {
                var traitProjection = CalculateTraitProjection(trait, baseExpression, environment, daysToPredict);
                projection.TraitProjections[trait] = traitProjection;
            }
            
            projection.OverallFitness = baseExpression.OverallFitness;
            projection.ConfidenceScore = CalculatePredictionConfidence(genotype, environment);
            
            return projection;
        }
        
        /// <summary>
        /// Analyzes trait stability across different environmental conditions.
        /// </summary>
        public TraitStabilityAnalysis AnalyzeTraitStability(PlantGenotype genotype, 
            List<EnvironmentalConditions> environments)
        {
            if (genotype == null || environments == null || environments.Count == 0)
            {
                Debug.LogError("[TraitExpressionProcessor] Cannot analyze trait stability: invalid parameters");
                return null;
            }
            
            var analysis = new TraitStabilityAnalysis
            {
                GenotypeID = genotype.GenotypeID,
                TestedEnvironments = environments.Count,
                TraitStability = new Dictionary<TraitType, float>(),
                AnalysisDate = System.DateTime.Now
            };
            
            // Calculate expressions for all environments
            var expressions = new List<TraitExpressionResult>();
            foreach (var environment in environments)
            {
                var expression = _traitExpressionEngine.CalculateExpression(genotype, environment);
                expressions.Add(expression);
            }
            
            // Analyze stability for key traits
            var keyTraits = new[] { TraitType.Height, TraitType.Yield, TraitType.Quality, 
                                   TraitType.THCContent, TraitType.CBDContent };
            
            foreach (var trait in keyTraits)
            {
                var stability = CalculateTraitStabilityScore(trait, expressions);
                analysis.TraitStability[trait] = stability;
            }
            
            analysis.OverallStability = analysis.TraitStability.Values.Average();
            
            return analysis;
        }
        
        /// <summary>
        /// Updates environmental modifiers for specific conditions.
        /// </summary>
        public void UpdateEnvironmentalModifier(string modifierName, EnvironmentalModifier modifier)
        {
            _environmentalModifiers[modifierName] = modifier;
            
            if (_enableDetailedLogging)
                Debug.Log($"[TraitExpressionProcessor] Updated environmental modifier: {modifierName}");
        }
        
        /// <summary>
        /// Clears the expression cache.
        /// </summary>
        public void ClearExpressionCache()
        {
            _expressionCache.Clear();
            Debug.Log("[TraitExpressionProcessor] Expression cache cleared");
        }
        
        /// <summary>
        /// Gets trait expression statistics for monitoring.
        /// </summary>
        public TraitExpressionStats GetExpressionStats()
        {
            return new TraitExpressionStats
            {
                CachedExpressions = _expressionCache.Count,
                EnvironmentalModifiers = _environmentalModifiers.Count,
                CacheHitRate = CalculateCacheHitRate(),
                AverageCalculationTime = GetAverageCalculationTime(),
                EnabledFeatures = new List<string>
                {
                    _enableEpistasis ? "Epistasis" : null,
                    _enablePleiotropy ? "Pleiotropy" : null,
                    _enableEnvironmentalCaching ? "Environmental Caching" : null,
                    _enableDetailedLogging ? "Detailed Logging" : null
                }.Where(f => f != null).ToList()
            };
        }
        
        // Private helper methods
        private PlantGenotype GetPlantGenotype(PlantInstanceSO plant)
        {
            // For now, use the plant's strain to generate a genotype
            // In a full implementation, this would retrieve or generate the plant's actual genotype
            if (plant.Strain != null)
            {
                // Simplified genotype generation from strain
                return new PlantGenotype
                {
                    GenotypeID = plant.PlantID,
                    StrainOrigin = plant.Strain,
                    Generation = 1,
                    IsFounder = true,
                    CreationDate = System.DateTime.Now
                };
            }
            
            return null;
        }
        
        private string GenerateExpressionCacheKey(string plantID, EnvironmentalConditions environment)
        {
            // Simplified cache key generation
            var envHash = environment.GetHashCode();
            return $"{plantID}_{envHash}";
        }
        
        private EnvironmentalConditions ApplyEnvironmentalModifiers(EnvironmentalConditions baseEnvironment, 
            PlantInstanceSO plant)
        {
            if (!_environmentalModifiers.Any())
                return baseEnvironment;
            
            // Apply modifiers to create modified environment
            var modifiedEnvironment = baseEnvironment; // In full implementation, would create a copy and apply modifiers
            
            foreach (var modifier in _environmentalModifiers.Values)
            {
                // Apply modifier logic here
                // modifiedEnvironment = modifier.Apply(modifiedEnvironment, plant);
            }
            
            return modifiedEnvironment;
        }
        
        private void PostProcessTraitExpression(TraitExpressionResult result, PlantInstanceSO plant, 
            EnvironmentalConditions environment)
        {
            if (result == null) return;
            
            // Apply any post-processing logic
            result.GenotypeID = plant.PlantID;
            
            // Notify about specific trait value updates
            OnTraitValueUpdated?.Invoke("Height", result.HeightExpression);
            OnTraitValueUpdated?.Invoke("THC", result.THCExpression);
            OnTraitValueUpdated?.Invoke("CBD", result.CBDExpression);
            OnTraitValueUpdated?.Invoke("Yield", result.YieldExpression);
        }
        
        private TraitExpressionResult CreateDefaultTraitExpression(PlantInstanceSO plant)
        {
            return new TraitExpressionResult
            {
                GenotypeID = plant.PlantID,
                OverallFitness = 0.5f,
                HeightExpression = 1.5f,
                THCExpression = 15f,
                CBDExpression = 1f,
                YieldExpression = 400f
            };
        }
        
        private TraitProjection CalculateTraitProjection(TraitType trait, TraitExpressionResult baseExpression, 
            EnvironmentalConditions environment, int days)
        {
            // Simplified trait projection calculation
            var baseValue = GetTraitValueFromExpression(trait, baseExpression);
            
            return new TraitProjection
            {
                Trait = trait,
                BaseValue = baseValue,
                ProjectedValue = baseValue * UnityEngine.Random.Range(0.8f, 1.2f),
                ProjectionDays = days,
                Confidence = UnityEngine.Random.Range(0.7f, 0.95f)
            };
        }
        
        private float GetTraitValueFromExpression(TraitType trait, TraitExpressionResult expression)
        {
            return trait switch
            {
                TraitType.Height => expression.HeightExpression,
                TraitType.THCContent => expression.THCExpression,
                TraitType.CBDContent => expression.CBDExpression,
                TraitType.Yield => expression.YieldExpression,
                _ => expression.OverallFitness
            };
        }
        
        private float CalculateTraitStabilityScore(TraitType trait, List<TraitExpressionResult> expressions)
        {
            var values = expressions.Select(e => GetTraitValueFromExpression(trait, e)).ToList();
            var mean = values.Average();
            var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Count;
            var stability = 1f / (1f + variance / (mean * mean)); // Coefficient of variation based stability
            
            return Mathf.Clamp01(stability);
        }
        
        private float CalculatePredictionConfidence(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            // Simplified confidence calculation
            return UnityEngine.Random.Range(0.7f, 0.95f);
        }
        
        private float CalculateCacheHitRate()
        {
            // In a full implementation, this would track actual cache hits/misses
            return 0.75f;
        }
        
        private float GetAverageCalculationTime()
        {
            // In a full implementation, this would track actual calculation times
            return 2.5f; // milliseconds
        }
    }
    
    /// <summary>
    /// Supporting data structures for trait expression processing
    /// </summary>
    [System.Serializable]
    public class PhenotypeProjection
    {
        public string GenotypeID;
        public string PlantId;
        public int PredictionDays;
        public EnvironmentalConditions BaseEnvironment;
        public Dictionary<TraitType, TraitProjection> TraitProjections;
        public Dictionary<TraitType, float> ProjectedTraits;
        public float OverallFitness;
        public float ConfidenceScore;
        public System.DateTime PredictionDate;
    }
    
    [System.Serializable]
    public class TraitProjection
    {
        public TraitType Trait;
        public float BaseValue;
        public float ProjectedValue;
        public int ProjectionDays;
        public float Confidence;
    }
    
    [System.Serializable]
    public class TraitStabilityAnalysis
    {
        public string GenotypeID;
        public string PlantId;
        public int TestedEnvironments;
        public Dictionary<TraitType, float> TraitStability;
        public float OverallStability;
        public float StabilityScore;
        public System.DateTime AnalysisDate;
    }
    
    [System.Serializable]
    public class EnvironmentalModifier
    {
        public string ModifierName;
        public Dictionary<string, float> ParameterModifiers;
        public float Intensity = 1.0f;
        public bool IsActive = true;
    }
    
    [System.Serializable]
    public class TraitExpressionStats
    {
        public int CachedExpressions;
        public int EnvironmentalModifiers;
        public float CacheHitRate;
        public float AverageCalculationTime;
        public List<string> EnabledFeatures;
    }
}