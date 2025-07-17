using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC-010-11: L3 Cache - Pattern-based precomputed genetic results cache.
    /// Stores and retrieves results based on genetic patterns and environmental templates.
    /// </summary>
    public class GeneticPrecomputeCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, PatternCacheEntry> _patternCache;
        private readonly ConcurrentDictionary<string, PatternTemplate> _patternTemplates;
        private readonly int _maxCacheSize;
        private readonly float _ttlSeconds;
        
        // Pattern analysis and confidence tracking
        private readonly ConcurrentDictionary<string, PatternConfidence> _patternConfidence;
        private readonly object _optimizationLock = new object();
        
        private struct PatternCacheEntry
        {
            public string Pattern;
            public EnvironmentalConditions Environment;
            public TraitExpressionResult Result;
            public DateTime ExpiryTime;
            public long AccessCount;
            public float ConfidenceScore;
            public DateTime LastAccessed;
        }
        
        private struct PatternTemplate
        {
            public string PatternSignature;
            public List<string> GeneticComponents;
            public Dictionary<string, float> TraitWeights;
            public float BaselineReliability;
            public int UsageCount;
        }
        
        private struct PatternConfidence
        {
            public float AccuracyScore;
            public int PredictionCount;
            public int SuccessfulPredictions;
            public DateTime LastUpdated;
        }
        
        public GeneticPrecomputeCache(int maxCacheSize = 2000, float ttlSeconds = 1800f)
        {
            _patternCache = new ConcurrentDictionary<string, PatternCacheEntry>();
            _patternTemplates = new ConcurrentDictionary<string, PatternTemplate>();
            _patternConfidence = new ConcurrentDictionary<string, PatternConfidence>();
            _maxCacheSize = maxCacheSize;
            _ttlSeconds = ttlSeconds;
            
            // Initialize common genetic patterns
            InitializeCommonPatterns();
        }
        
        /// <summary>
        /// Attempt to retrieve a result based on genetic pattern matching.
        /// </summary>
        public bool TryGetPatternResult(PlantGenotype genotype, EnvironmentalConditions environment, 
            out TraitExpressionResult result, out string matchedPattern)
        {
            result = null;
            matchedPattern = null;
            
            string inputPattern = GenerateGeneticPattern(genotype);
            
            // Find matching patterns
            var patternMatches = new List<(string pattern, float similarity)>();
            
            foreach (var template in _patternTemplates)
            {
                float similarity = CalculatePatternSimilarity(inputPattern, template.Key);
                if (similarity >= 0.8f) // High similarity threshold for pattern matching
                {
                    patternMatches.Add((template.Key, similarity));
                }
            }
            
            // Find the best matching cached entry
            foreach (var (pattern, similarity) in patternMatches.OrderByDescending(m => m.similarity))
            {
                string cacheKey = GeneratePatternKey(pattern, environment);
                
                if (_patternCache.TryGetValue(cacheKey, out var entry))
                {
                    if (DateTime.Now < entry.ExpiryTime)
                    {
                        // Update access statistics
                        entry.AccessCount++;
                        entry.LastAccessed = DateTime.Now;
                        _patternCache[cacheKey] = entry;
                        
                        result = entry.Result;
                        matchedPattern = pattern;
                        
                        // Update pattern confidence
                        UpdatePatternConfidence(pattern, true);
                        
                        return true;
                    }
                    else
                    {
                        _patternCache.TryRemove(cacheKey, out _);
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Cache a result using genetic pattern analysis.
        /// </summary>
        public void SetPattern(string geneticPattern, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            string cacheKey = GeneratePatternKey(geneticPattern, environment);
            
            var entry = new PatternCacheEntry
            {
                Pattern = geneticPattern,
                Environment = environment,
                Result = result,
                ExpiryTime = DateTime.Now.AddSeconds(_ttlSeconds),
                AccessCount = 1,
                ConfidenceScore = CalculateInitialConfidence(geneticPattern),
                LastAccessed = DateTime.Now
            };
            
            _patternCache[cacheKey] = entry;
            
            // Register or update pattern template
            RegisterPatternTemplate(geneticPattern, result);
            
            if (_patternCache.Count > _maxCacheSize)
            {
                EvictLowConfidencePatterns();
            }
        }
        
        /// <summary>
        /// Register a genetic pattern for future pattern matching.
        /// </summary>
        public void RegisterPattern(string geneticPattern, EnvironmentalConditions environment)
        {
            if (_patternTemplates.TryGetValue(geneticPattern, out var template))
            {
                template.UsageCount++;
                _patternTemplates[geneticPattern] = template;
            }
            else
            {
                var newTemplate = new PatternTemplate
                {
                    PatternSignature = geneticPattern,
                    GeneticComponents = ParseGeneticComponents(geneticPattern),
                    TraitWeights = AnalyzeTraitWeights(geneticPattern),
                    BaselineReliability = 0.5f,
                    UsageCount = 1
                };
                
                _patternTemplates[geneticPattern] = newTemplate;
            }
        }
        
        /// <summary>
        /// Get confidence score for a specific genetic pattern.
        /// </summary>
        public float GetPatternConfidence(string pattern)
        {
            if (_patternConfidence.TryGetValue(pattern, out var confidence))
            {
                return confidence.AccuracyScore;
            }
            return 0.5f; // Default confidence for unknown patterns
        }
        
        /// <summary>
        /// Optimize pattern weights based on usage and accuracy.
        /// </summary>
        public void OptimizePatternWeights()
        {
            lock (_optimizationLock)
            {
                var patternsToOptimize = _patternConfidence
                    .Where(kvp => kvp.Value.PredictionCount >= 10) // Minimum predictions for optimization
                    .OrderByDescending(kvp => kvp.Value.AccuracyScore)
                    .Take(100) // Optimize top 100 patterns
                    .ToList();
                
                foreach (var (pattern, confidence) in patternsToOptimize)
                {
                    if (_patternTemplates.TryGetValue(pattern, out var template))
                    {
                        // Adjust baseline reliability based on accuracy
                        template.BaselineReliability = Mathf.Lerp(template.BaselineReliability, confidence.AccuracyScore, 0.1f);
                        
                        // Update trait weights based on successful predictions
                        OptimizeTraitWeights(ref template, confidence);
                        
                        _patternTemplates[pattern] = template;
                    }
                }
                
                Debug.Log($"[GeneticPrecomputeCache] Optimized {patternsToOptimize.Count} patterns");
            }
        }
        
        /// <summary>
        /// Clear all cached patterns and templates.
        /// </summary>
        public void Clear()
        {
            _patternCache.Clear();
            _patternConfidence.Clear();
            // Keep pattern templates as they represent learned knowledge
        }
        
        // Private helper methods
        
        private void InitializeCommonPatterns()
        {
            // Initialize common cannabis genetic patterns
            var commonPatterns = new[]
            {
                "THC:HOM:DOM|CBD:HET:REC|HEIGHT:HET:DOM",  // High THC, moderate CBD, tall
                "THC:HET:DOM|CBD:HOM:DOM|HEIGHT:HOM:REC",  // Balanced THC/CBD, short
                "THC:HOM:REC|CBD:HOM:DOM|HEIGHT:HET:DOM",  // CBD dominant, tall
                "YIELD:HOM:DOM|THC:HET:DOM|HEIGHT:HOM:DOM" // High yield, high THC, tall
            };
            
            foreach (var pattern in commonPatterns)
            {
                RegisterPatternTemplate(pattern, null);
            }
        }
        
        private string GenerateGeneticPattern(PlantGenotype genotype)
        {
            var traits = new List<string>();
            
            foreach (var gene in genotype.Genotype)
            {
                var alleleCouple = gene.Value;
                if (alleleCouple?.Allele1 != null && alleleCouple?.Allele2 != null)
                {
                    bool isHomozygous = alleleCouple.Allele1.UniqueID == alleleCouple.Allele2.UniqueID;
                    bool hasDominant = alleleCouple.Allele1.IsDominant || alleleCouple.Allele2.IsDominant;
                    
                    string traitPattern = $"{gene.Key}:{(isHomozygous ? "HOM" : "HET")}:{(hasDominant ? "DOM" : "REC")}";
                    traits.Add(traitPattern);
                }
            }
            
            return string.Join("|", traits.OrderBy(t => t));
        }
        
        private float CalculatePatternSimilarity(string pattern1, string pattern2)
        {
            var components1 = pattern1.Split('|').ToHashSet();
            var components2 = pattern2.Split('|').ToHashSet();
            
            var intersection = components1.Intersect(components2).Count();
            var union = components1.Union(components2).Count();
            
            return union > 0 ? (float)intersection / union : 0f;
        }
        
        private string GeneratePatternKey(string pattern, EnvironmentalConditions environment)
        {
            // Create a simplified environmental signature
            string envSignature = $"T{(int)environment.Temperature}_H{(int)environment.Humidity}_L{(int)environment.LightIntensity}";
            return $"{pattern}#{envSignature}";
        }
        
        private float CalculateInitialConfidence(string geneticPattern)
        {
            // Base confidence on pattern complexity and known components
            var components = geneticPattern.Split('|').Length;
            
            // More complex patterns start with lower confidence
            float complexityFactor = Mathf.Clamp01(1f - (components - 3) * 0.1f);
            
            return 0.5f + (complexityFactor * 0.3f); // Range: 0.5 to 0.8
        }
        
        private void RegisterPatternTemplate(string geneticPattern, TraitExpressionResult result)
        {
            if (!_patternTemplates.TryGetValue(geneticPattern, out var template))
            {
                template = new PatternTemplate
                {
                    PatternSignature = geneticPattern,
                    GeneticComponents = ParseGeneticComponents(geneticPattern),
                    TraitWeights = AnalyzeTraitWeights(geneticPattern),
                    BaselineReliability = 0.5f,
                    UsageCount = 0
                };
            }
            
            template.UsageCount++;
            
            // Update trait weights if we have a result
            if (result != null)
            {
                UpdateTraitWeightsFromResult(ref template, result);
            }
            
            _patternTemplates[geneticPattern] = template;
        }
        
        private List<string> ParseGeneticComponents(string pattern)
        {
            return pattern.Split('|').ToList();
        }
        
        private Dictionary<string, float> AnalyzeTraitWeights(string pattern)
        {
            var weights = new Dictionary<string, float>();
            var components = pattern.Split('|');
            
            foreach (var component in components)
            {
                var parts = component.Split(':');
                if (parts.Length >= 3)
                {
                    string trait = parts[0];
                    string zygosity = parts[1]; // HOM or HET
                    string dominance = parts[2]; // DOM or REC
                    
                    // Calculate weight based on genetic architecture
                    float weight = 1.0f;
                    if (zygosity == "HOM") weight *= 1.2f; // Homozygous more predictable
                    if (dominance == "DOM") weight *= 1.1f; // Dominant effects more pronounced
                    
                    weights[trait] = weight;
                }
            }
            
            return weights;
        }
        
        private void UpdateTraitWeightsFromResult(ref PatternTemplate template, TraitExpressionResult result)
        {
            // Update weights based on observed trait expressions
            if (template.TraitWeights.ContainsKey("THC"))
            {
                template.TraitWeights["THC"] = Mathf.Lerp(template.TraitWeights["THC"], result.THCExpression, 0.1f);
            }
            
            if (template.TraitWeights.ContainsKey("CBD"))
            {
                template.TraitWeights["CBD"] = Mathf.Lerp(template.TraitWeights["CBD"], result.CBDExpression, 0.1f);
            }
            
            if (template.TraitWeights.ContainsKey("HEIGHT"))
            {
                template.TraitWeights["HEIGHT"] = Mathf.Lerp(template.TraitWeights["HEIGHT"], result.HeightExpression, 0.1f);
            }
            
            if (template.TraitWeights.ContainsKey("YIELD"))
            {
                template.TraitWeights["YIELD"] = Mathf.Lerp(template.TraitWeights["YIELD"], result.YieldExpression, 0.1f);
            }
        }
        
        private void UpdatePatternConfidence(string pattern, bool successful)
        {
            if (_patternConfidence.TryGetValue(pattern, out var confidence))
            {
                confidence.PredictionCount++;
                if (successful) confidence.SuccessfulPredictions++;
                
                confidence.AccuracyScore = confidence.PredictionCount > 0 ? 
                    (float)confidence.SuccessfulPredictions / confidence.PredictionCount : 0f;
                confidence.LastUpdated = DateTime.Now;
                
                _patternConfidence[pattern] = confidence;
            }
            else
            {
                _patternConfidence[pattern] = new PatternConfidence
                {
                    PredictionCount = 1,
                    SuccessfulPredictions = successful ? 1 : 0,
                    AccuracyScore = successful ? 1f : 0f,
                    LastUpdated = DateTime.Now
                };
            }
        }
        
        private void OptimizeTraitWeights(ref PatternTemplate template, PatternConfidence confidence)
        {
            // Adjust trait weights based on prediction accuracy
            float adjustmentFactor = (confidence.AccuracyScore - 0.5f) * 0.1f; // Range: -0.05 to +0.05
            
            var optimizedWeights = new Dictionary<string, float>();
            foreach (var kvp in template.TraitWeights)
            {
                float newWeight = Mathf.Clamp01(kvp.Value + adjustmentFactor);
                optimizedWeights[kvp.Key] = newWeight;
            }
            
            template.TraitWeights = optimizedWeights;
        }
        
        private void EvictLowConfidencePatterns()
        {
            var now = DateTime.Now;
            var candidates = _patternCache.ToList()
                .Where(kvp => now < kvp.Value.ExpiryTime)
                .OrderBy(kvp => kvp.Value.ConfidenceScore * kvp.Value.AccessCount) // Remove low confidence, low usage
                .Take(_patternCache.Count - _maxCacheSize + 100)
                .ToList();
            
            foreach (var candidate in candidates)
            {
                _patternCache.TryRemove(candidate.Key, out _);
            }
        }
        
        public void Dispose()
        {
            _patternCache?.Clear();
            _patternTemplates?.Clear();
            _patternConfidence?.Clear();
        }
    }
}