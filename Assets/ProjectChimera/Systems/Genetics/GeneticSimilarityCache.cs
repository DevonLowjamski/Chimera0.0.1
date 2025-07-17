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
    /// PC-010-11: L2 Cache - Genetic similarity-based caching system.
    /// Retrieves cached results for genetically similar organisms and environmental conditions.
    /// </summary>
    public class GeneticSimilarityCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, SimilarityCacheEntry> _cache;
        private readonly int _maxCacheSize;
        private readonly float _ttlSeconds;
        private readonly object _optimizationLock = new object();
        
        // Similarity calculation settings
        private float _currentSimilarityThreshold = 0.85f;
        private readonly SimilarityWeights _weights;
        
        private struct SimilarityCacheEntry
        {
            public PlantGenotype Genotype;
            public EnvironmentalConditions Environment;
            public TraitExpressionResult Result;
            public DateTime ExpiryTime;
            public long AccessCount;
            public DateTime LastAccessed;
            public float AverageSimilarity; // Average similarity of matches found
        }
        
        public struct PromotionCandidate
        {
            public PlantGenotype Genotype;
            public EnvironmentalConditions Environment;
            public TraitExpressionResult Result;
            public float AccessFrequency;
        }
        
        private struct SimilarityWeights
        {
            public float GenotypeWeight;
            public float EnvironmentWeight;
            public float FitnessWeight;
            public float InbreedingWeight;
            
            public static SimilarityWeights Default => new SimilarityWeights
            {
                GenotypeWeight = 0.6f,
                EnvironmentWeight = 0.3f,
                FitnessWeight = 0.05f,
                InbreedingWeight = 0.05f
            };
        }
        
        public GeneticSimilarityCache(int maxCacheSize = 10000, float ttlSeconds = 600f)
        {
            _cache = new ConcurrentDictionary<string, SimilarityCacheEntry>();
            _maxCacheSize = maxCacheSize;
            _ttlSeconds = ttlSeconds;
            _weights = SimilarityWeights.Default;
        }
        
        /// <summary>
        /// Attempt to find a cached result for a genetically similar organism.
        /// </summary>
        public bool TryGetSimilarResult(PlantGenotype genotype, EnvironmentalConditions environment, 
            out TraitExpressionResult result, out float similarity)
        {
            result = null;
            similarity = 0f;
            
            var candidateMatches = new List<(SimilarityCacheEntry entry, float similarity)>();
            
            // Find all entries that exceed minimum similarity threshold
            foreach (var kvp in _cache)
            {
                var entry = kvp.Value;
                
                // Skip expired entries
                if (DateTime.Now >= entry.ExpiryTime)
                {
                    _cache.TryRemove(kvp.Key, out _);
                    continue;
                }
                
                float calculatedSimilarity = CalculateSimilarity(genotype, environment, entry.Genotype, entry.Environment);
                
                if (calculatedSimilarity >= _currentSimilarityThreshold)
                {
                    candidateMatches.Add((entry, calculatedSimilarity));
                }
            }
            
            // If we have matches, use the best one
            if (candidateMatches.Count > 0)
            {
                var bestMatch = candidateMatches.OrderByDescending(m => m.similarity).First();
                var entry = bestMatch.entry;
                similarity = bestMatch.similarity;
                
                // Update access statistics
                entry.AccessCount++;
                entry.LastAccessed = DateTime.Now;
                entry.AverageSimilarity = (entry.AverageSimilarity + similarity) * 0.5f;
                
                _cache[GenerateKey(entry.Genotype, entry.Environment)] = entry;
                result = entry.Result;
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Cache a genetic calculation result for similarity-based retrieval.
        /// </summary>
        public void Set(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            string key = GenerateKey(genotype, environment);
            
            var entry = new SimilarityCacheEntry
            {
                Genotype = genotype,
                Environment = environment,
                Result = result,
                ExpiryTime = DateTime.Now.AddSeconds(_ttlSeconds),
                AccessCount = 1,
                LastAccessed = DateTime.Now,
                AverageSimilarity = 1.0f // Perfect match for self
            };
            
            _cache[key] = entry;
            
            // Check if cache size limit is exceeded
            if (_cache.Count > _maxCacheSize)
            {
                EvictLeastUseful();
            }
        }
        
        /// <summary>
        /// Get candidates for promotion to L1 cache based on access patterns.
        /// </summary>
        public List<PromotionCandidate> GetPromotionCandidates(float promotionThreshold)
        {
            var candidates = new List<PromotionCandidate>();
            var now = DateTime.Now;
            
            foreach (var entry in _cache.Values)
            {
                if (now < entry.ExpiryTime)
                {
                    // Calculate access frequency (accesses per hour)
                    var timeAlive = now - (entry.ExpiryTime.AddSeconds(-_ttlSeconds));
                    float accessFrequency = timeAlive.TotalHours > 0 ? (float)(entry.AccessCount / timeAlive.TotalHours) : 0f;
                    
                    // Promote if access frequency and average similarity are high
                    if (accessFrequency >= promotionThreshold && entry.AverageSimilarity >= promotionThreshold)
                    {
                        candidates.Add(new PromotionCandidate
                        {
                            Genotype = entry.Genotype,
                            Environment = entry.Environment,
                            Result = entry.Result,
                            AccessFrequency = accessFrequency
                        });
                    }
                }
            }
            
            return candidates.OrderByDescending(c => c.AccessFrequency).Take(100).ToList();
        }
        
        /// <summary>
        /// Optimize similarity thresholds based on usage patterns.
        /// </summary>
        public void OptimizeSimilarityThresholds()
        {
            lock (_optimizationLock)
            {
                var allSimilarities = _cache.Values
                    .Where(e => DateTime.Now < e.ExpiryTime)
                    .Select(e => e.AverageSimilarity)
                    .Where(s => s > 0f)
                    .ToList();
                
                if (allSimilarities.Count > 100) // Need sufficient data
                {
                    // Calculate optimal threshold based on 80th percentile of successful matches
                    allSimilarities.Sort();
                    int percentile80Index = (int)(allSimilarities.Count * 0.8f);
                    float optimalThreshold = allSimilarities[percentile80Index];
                    
                    // Gradually adjust threshold towards optimal value
                    _currentSimilarityThreshold = Mathf.Lerp(_currentSimilarityThreshold, optimalThreshold, 0.1f);
                    _currentSimilarityThreshold = Mathf.Clamp(_currentSimilarityThreshold, 0.7f, 0.95f);
                    
                    Debug.Log($"[GeneticSimilarityCache] Optimized similarity threshold to {_currentSimilarityThreshold:F3}");
                }
            }
        }
        
        /// <summary>
        /// Clear all cached entries.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }
        
        // Private helper methods
        
        private float CalculateSimilarity(PlantGenotype genotype1, EnvironmentalConditions env1,
            PlantGenotype genotype2, EnvironmentalConditions env2)
        {
            // Calculate genetic similarity
            float geneticSimilarity = CalculateGeneticSimilarity(genotype1, genotype2);
            
            // Calculate environmental similarity
            float environmentalSimilarity = CalculateEnvironmentalSimilarity(env1, env2);
            
            // Calculate fitness similarity
            float fitnessSimilarity = CalculateFitnessSimilarity(genotype1, genotype2);
            
            // Calculate inbreeding similarity
            float inbreedingSimilarity = CalculateInbreedingSimilarity(genotype1, genotype2);
            
            // Weighted combination
            return (geneticSimilarity * _weights.GenotypeWeight) +
                   (environmentalSimilarity * _weights.EnvironmentWeight) +
                   (fitnessSimilarity * _weights.FitnessWeight) +
                   (inbreedingSimilarity * _weights.InbreedingWeight);
        }
        
        private float CalculateGeneticSimilarity(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            if (genotype1?.Genotype == null || genotype2?.Genotype == null)
                return 0f;
            
            var commonGenes = genotype1.Genotype.Keys.Intersect(genotype2.Genotype.Keys).ToList();
            if (commonGenes.Count == 0) return 0f;
            
            float totalSimilarity = 0f;
            
            foreach (var gene in commonGenes)
            {
                var alleles1 = genotype1.Genotype[gene];
                var alleles2 = genotype2.Genotype[gene];
                
                if (alleles1?.Allele1 != null && alleles1?.Allele2 != null &&
                    alleles2?.Allele1 != null && alleles2?.Allele2 != null)
                {
                    // Calculate allele similarity
                    float alleleSimilarity = CalculateAlleleSimilarity(alleles1, alleles2);
                    totalSimilarity += alleleSimilarity;
                }
            }
            
            return totalSimilarity / commonGenes.Count;
        }
        
        private float CalculateAlleleSimilarity(AlleleCouple alleles1, AlleleCouple alleles2)
        {
            // Check for exact matches
            if ((alleles1.Allele1.UniqueID == alleles2.Allele1.UniqueID && alleles1.Allele2.UniqueID == alleles2.Allele2.UniqueID) ||
                (alleles1.Allele1.UniqueID == alleles2.Allele2.UniqueID && alleles1.Allele2.UniqueID == alleles2.Allele1.UniqueID))
            {
                return 1.0f; // Perfect match
            }
            
            // Check for partial matches
            int matches = 0;
            if (alleles1.Allele1.UniqueID == alleles2.Allele1.UniqueID || alleles1.Allele1.UniqueID == alleles2.Allele2.UniqueID)
                matches++;
            if (alleles1.Allele2.UniqueID == alleles2.Allele1.UniqueID || alleles1.Allele2.UniqueID == alleles2.Allele2.UniqueID)
                matches++;
            
            if (matches > 0)
                return 0.5f + (matches * 0.25f); // Partial similarity
            
            // Check effect strength similarity for different alleles
            float effect1Avg = (alleles1.Allele1.EffectStrength + alleles1.Allele2.EffectStrength) * 0.5f;
            float effect2Avg = (alleles2.Allele1.EffectStrength + alleles2.Allele2.EffectStrength) * 0.5f;
            
            float effectSimilarity = 1f - Mathf.Abs(effect1Avg - effect2Avg);
            return Mathf.Max(0.1f, effectSimilarity * 0.3f); // Minimum similarity for similar effects
        }
        
        private float CalculateEnvironmentalSimilarity(EnvironmentalConditions env1, EnvironmentalConditions env2)
        {
            float tempSimilarity = 1f - Mathf.Abs(env1.Temperature - env2.Temperature) / 50f; // Assume 50° range
            float humiditySimilarity = 1f - Mathf.Abs(env1.Humidity - env2.Humidity) / 100f;
            float lightSimilarity = 1f - Mathf.Abs(env1.LightIntensity - env2.LightIntensity) / 1000f;
            float co2Similarity = 1f - Mathf.Abs(env1.CO2Level - env2.CO2Level) / 2000f;
            
            return Mathf.Clamp01((tempSimilarity + humiditySimilarity + lightSimilarity + co2Similarity) * 0.25f);
        }
        
        private float CalculateFitnessSimilarity(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return 1f - Mathf.Abs(genotype1.OverallFitness - genotype2.OverallFitness);
        }
        
        private float CalculateInbreedingSimilarity(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return 1f - Mathf.Abs(genotype1.InbreedingCoefficient - genotype2.InbreedingCoefficient);
        }
        
        private string GenerateKey(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            return $"{genotype.GenotypeID}_{environment.GetHashCode()}";
        }
        
        private void EvictLeastUseful()
        {
            var now = DateTime.Now;
            var candidates = _cache.ToList()
                .Where(kvp => now < kvp.Value.ExpiryTime)
                .OrderBy(kvp => kvp.Value.AccessCount / Math.Max(1, (now - kvp.Value.LastAccessed).TotalHours))
                .Take(_cache.Count - _maxCacheSize + 100) // Remove extra entries
                .ToList();
            
            foreach (var candidate in candidates)
            {
                _cache.TryRemove(candidate.Key, out _);
            }
        }
        
        public void Dispose()
        {
            _cache?.Clear();
        }
    }
}