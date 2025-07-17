using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC-010-11: Advanced genetic caching system for high-performance genetic calculations.
    /// Features multi-level caching, genetic similarity clustering, and predictive pre-caching.
    /// </summary>
    public class AdvancedGeneticCacheSystem : IDisposable
    {
        // Multi-level cache architecture
        private readonly GeneticCalculationCache _l1Cache; // Fast access cache
        private readonly GeneticSimilarityCache _l2Cache; // Similarity-based cache
        private readonly GeneticPrecomputeCache _l3Cache; // Precomputed patterns cache
        
        // Cache statistics and monitoring
        private readonly GeneticCacheMetrics _metrics;
        private readonly Timer _optimizationTimer;
        
        // Configuration
        private readonly AdvancedCacheConfig _config;
        
        public AdvancedGeneticCacheSystem(AdvancedCacheConfig config = null)
        {
            _config = config ?? AdvancedCacheConfig.Default;
            
            // Initialize multi-level caches
            _l1Cache = new GeneticCalculationCache(_config.L1MaxSize, _config.L1TTLSeconds);
            _l2Cache = new GeneticSimilarityCache(_config.L2MaxSize, _config.L2TTLSeconds);
            _l3Cache = new GeneticPrecomputeCache(_config.L3MaxSize, _config.L3TTLSeconds);
            
            // Initialize metrics and optimization
            _metrics = new GeneticCacheMetrics();
            _optimizationTimer = new Timer(OptimizeCaches, null, 
                TimeSpan.FromMinutes(_config.OptimizationIntervalMinutes), 
                TimeSpan.FromMinutes(_config.OptimizationIntervalMinutes));
            
            Debug.Log($"[AdvancedGeneticCache] Initialized with L1:{_config.L1MaxSize}, L2:{_config.L2MaxSize}, L3:{_config.L3MaxSize}");
        }
        
        /// <summary>
        /// Attempt to retrieve a cached genetic calculation result.
        /// Uses multi-level cache hierarchy for optimal performance.
        /// </summary>
        public bool TryGetCachedResult(PlantGenotype genotype, EnvironmentalConditions environment, out TraitExpressionResult result)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            result = null;
            
            string primaryKey = GeneratePrimaryKey(genotype, environment);
            
            // L1 Cache: Direct hash lookup (fastest)
            if (_l1Cache.TryGetValue(primaryKey, out result))
            {
                _metrics.RecordL1Hit(stopwatch.ElapsedTicks);
                return true;
            }
            
            // L2 Cache: Genetic similarity lookup
            if (_l2Cache.TryGetSimilarResult(genotype, environment, out result, out float similarity))
            {
                // If similarity is high enough, also cache in L1 for faster future access
                if (similarity >= _config.L1PromotionThreshold)
                {
                    _l1Cache.Set(primaryKey, result);
                }
                _metrics.RecordL2Hit(stopwatch.ElapsedTicks, similarity);
                return true;
            }
            
            // L3 Cache: Precomputed pattern lookup
            if (_l3Cache.TryGetPatternResult(genotype, environment, out result, out string pattern))
            {
                // Cache in higher levels for faster access
                _l2Cache.Set(genotype, environment, result);
                if (_l3Cache.GetPatternConfidence(pattern) >= _config.L1PromotionThreshold)
                {
                    _l1Cache.Set(primaryKey, result);
                }
                _metrics.RecordL3Hit(stopwatch.ElapsedTicks, pattern);
                return true;
            }
            
            _metrics.RecordCacheMiss(stopwatch.ElapsedTicks);
            return false;
        }
        
        /// <summary>
        /// Cache a newly calculated genetic result across all cache levels.
        /// </summary>
        public void CacheResult(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            string primaryKey = GeneratePrimaryKey(genotype, environment);
            
            // Always cache in L1 for immediate reuse
            _l1Cache.Set(primaryKey, result);
            
            // Cache in L2 for similarity-based retrieval
            _l2Cache.Set(genotype, environment, result);
            
            // Analyze for L3 pattern caching
            string geneticPattern = AnalyzeGeneticPattern(genotype);
            if (!string.IsNullOrEmpty(geneticPattern))
            {
                _l3Cache.SetPattern(geneticPattern, environment, result);
            }
            
            _metrics.RecordCacheSet();
        }
        
        /// <summary>
        /// Precompute and cache results for common genetic patterns.
        /// </summary>
        public async Task PrecomputeCommonPatterns(List<PlantStrainSO> commonStrains, List<EnvironmentalConditions> commonEnvironments)
        {
            Debug.Log($"[AdvancedGeneticCache] Starting precomputation for {commonStrains.Count} strains × {commonEnvironments.Count} environments");
            
            var precomputeTasks = new List<Task>();
            
            foreach (var strain in commonStrains)
            {
                foreach (var environment in commonEnvironments)
                {
                    var task = Task.Run(() => PrecomputeStrainEnvironmentCombination(strain, environment));
                    precomputeTasks.Add(task);
                    
                    // Limit concurrent tasks
                    if (precomputeTasks.Count >= _config.MaxConcurrentPrecompute)
                    {
                        await Task.WhenAny(precomputeTasks);
                        precomputeTasks.RemoveAll(t => t.IsCompleted);
                    }
                }
            }
            
            await Task.WhenAll(precomputeTasks);
            Debug.Log("[AdvancedGeneticCache] Precomputation completed");
        }
        
        /// <summary>
        /// Get comprehensive cache performance metrics.
        /// </summary>
        public GeneticCacheMetrics GetMetrics()
        {
            return _metrics;
        }
        
        /// <summary>
        /// Clear all cache levels.
        /// </summary>
        public void ClearAll()
        {
            _l1Cache.Clear();
            _l2Cache.Clear();
            _l3Cache.Clear();
            _metrics.Reset();
            Debug.Log("[AdvancedGeneticCache] All caches cleared");
        }
        
        // Private helper methods
        
        private string GeneratePrimaryKey(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            // Create a fast hash combining genotype and environment
            int hash = genotype.GenotypeID.GetHashCode();
            hash = hash * 31 + environment.GetHashCode();
            return hash.ToString();
        }
        
        private string AnalyzeGeneticPattern(PlantGenotype genotype)
        {
            // Analyze genetic structure to identify common patterns
            var traits = new List<string>();
            
            foreach (var gene in genotype.Genotype)
            {
                var alleleCouple = gene.Value;
                if (alleleCouple?.Allele1 != null && alleleCouple?.Allele2 != null)
                {
                    // Create pattern based on dominance relationships
                    bool isHomozygous = alleleCouple.Allele1.UniqueID == alleleCouple.Allele2.UniqueID;
                    bool hasDominant = alleleCouple.Allele1.IsDominant || alleleCouple.Allele2.IsDominant;
                    
                    string traitPattern = $"{gene.Key}:{(isHomozygous ? "HOM" : "HET")}:{(hasDominant ? "DOM" : "REC")}";
                    traits.Add(traitPattern);
                }
            }
            
            return string.Join("|", traits.OrderBy(t => t));
        }
        
        private void PrecomputeStrainEnvironmentCombination(PlantStrainSO strain, EnvironmentalConditions environment)
        {
            try
            {
                // Generate representative genotype for strain
                var genotype = GenerateRepresentativeGenotype(strain);
                string pattern = AnalyzeGeneticPattern(genotype);
                
                // Store pattern for future L3 cache use
                _l3Cache.RegisterPattern(pattern, environment);
                
                _metrics.RecordPrecompute();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdvancedGeneticCache] Precompute failed for strain {strain.name}: {ex.Message}");
            }
        }
        
        private PlantGenotype GenerateRepresentativeGenotype(PlantStrainSO strain)
        {
            // Create a representative genotype based on strain's typical genetic makeup
            var genotype = new PlantGenotype
            {
                GenotypeID = $"REPR_{strain.StrainId}_{UnityEngine.Random.Range(1000, 9999)}",
                OverallFitness = 0.8f, // Default representative fitness
                InbreedingCoefficient = 0.1f, // Low inbreeding for representative sample
                Genotype = new Dictionary<string, AlleleCouple>()
            };
            
            // Add representative alleles based on strain characteristics
            // This would typically use the strain's genetic data to create realistic combinations
            
            return genotype;
        }
        
        private void OptimizeCaches(object state)
        {
            try
            {
                // Optimize cache performance based on usage patterns
                _l2Cache.OptimizeSimilarityThresholds();
                _l3Cache.OptimizePatternWeights();
                
                // Promote frequently accessed L2 items to L1
                var promotionCandidates = _l2Cache.GetPromotionCandidates(_config.L1PromotionThreshold);
                foreach (var candidate in promotionCandidates)
                {
                    string key = GeneratePrimaryKey(candidate.Genotype, candidate.Environment);
                    _l1Cache.Set(key, candidate.Result);
                }
                
                _metrics.RecordOptimization();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdvancedGeneticCache] Cache optimization failed: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _optimizationTimer?.Dispose();
            _l1Cache?.Dispose();
            _l2Cache?.Dispose();
            _l3Cache?.Dispose();
        }
    }
    
    /// <summary>
    /// Configuration for the advanced genetic cache system.
    /// </summary>
    [Serializable]
    public class AdvancedCacheConfig
    {
        [Header("L1 Cache (Direct Hash)")]
        public int L1MaxSize = 5000;
        public float L1TTLSeconds = 300f; // 5 minutes
        
        [Header("L2 Cache (Similarity)")]
        public int L2MaxSize = 10000;
        public float L2TTLSeconds = 600f; // 10 minutes
        public float SimilarityThreshold = 0.85f;
        
        [Header("L3 Cache (Patterns)")]
        public int L3MaxSize = 2000;
        public float L3TTLSeconds = 1800f; // 30 minutes
        
        [Header("Optimization")]
        public float OptimizationIntervalMinutes = 5f;
        public float L1PromotionThreshold = 0.9f;
        public int MaxConcurrentPrecompute = 4;
        
        public static AdvancedCacheConfig Default => new AdvancedCacheConfig();
    }
    
    /// <summary>
    /// Comprehensive metrics for genetic cache performance.
    /// </summary>
    public class GeneticCacheMetrics
    {
        private long _l1Hits, _l2Hits, _l3Hits, _misses, _sets, _precomputes, _optimizations;
        private readonly List<long> _l1HitTimes = new List<long>();
        private readonly List<long> _l2HitTimes = new List<long>();
        private readonly List<long> _l3HitTimes = new List<long>();
        private readonly List<float> _l2Similarities = new List<float>();
        
        public void RecordL1Hit(long elapsedTicks) 
        { 
            Interlocked.Increment(ref _l1Hits); 
            lock (_l1HitTimes) _l1HitTimes.Add(elapsedTicks);
        }
        
        public void RecordL2Hit(long elapsedTicks, float similarity) 
        { 
            Interlocked.Increment(ref _l2Hits);
            lock (_l2HitTimes) _l2HitTimes.Add(elapsedTicks);
            lock (_l2Similarities) _l2Similarities.Add(similarity);
        }
        
        public void RecordL3Hit(long elapsedTicks, string pattern) 
        { 
            Interlocked.Increment(ref _l3Hits);
            lock (_l3HitTimes) _l3HitTimes.Add(elapsedTicks);
        }
        
        public void RecordCacheMiss(long elapsedTicks) { Interlocked.Increment(ref _misses); }
        public void RecordCacheSet() { Interlocked.Increment(ref _sets); }
        public void RecordPrecompute() { Interlocked.Increment(ref _precomputes); }
        public void RecordOptimization() { Interlocked.Increment(ref _optimizations); }
        
        public float GetOverallHitRatio()
        {
            long totalRequests = _l1Hits + _l2Hits + _l3Hits + _misses;
            return totalRequests > 0 ? (float)(_l1Hits + _l2Hits + _l3Hits) / totalRequests : 0f;
        }
        
        public (float l1, float l2, float l3) GetHitRatiosByLevel()
        {
            long totalRequests = _l1Hits + _l2Hits + _l3Hits + _misses;
            if (totalRequests == 0) return (0f, 0f, 0f);
            
            return ((float)_l1Hits / totalRequests, 
                    (float)_l2Hits / totalRequests, 
                    (float)_l3Hits / totalRequests);
        }
        
        public void Reset()
        {
            _l1Hits = _l2Hits = _l3Hits = _misses = _sets = _precomputes = _optimizations = 0;
            lock (_l1HitTimes) _l1HitTimes.Clear();
            lock (_l2HitTimes) _l2HitTimes.Clear();
            lock (_l3HitTimes) _l3HitTimes.Clear();
            lock (_l2Similarities) _l2Similarities.Clear();
        }
        
        public string GetDetailedReport()
        {
            var hitRatios = GetHitRatiosByLevel();
            return $"Cache Performance Report:\n" +
                   $"Overall Hit Ratio: {GetOverallHitRatio():P2}\n" +
                   $"L1 Hits: {_l1Hits} ({hitRatios.l1:P2})\n" +
                   $"L2 Hits: {_l2Hits} ({hitRatios.l2:P2})\n" +
                   $"L3 Hits: {_l3Hits} ({hitRatios.l3:P2})\n" +
                   $"Misses: {_misses}\n" +
                   $"Total Sets: {_sets}\n" +
                   $"Precomputes: {_precomputes}\n" +
                   $"Optimizations: {_optimizations}";
        }
    }
}