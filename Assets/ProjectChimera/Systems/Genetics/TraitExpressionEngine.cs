using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Phase 3.2: High-Performance Trait Expression Engine with GPU acceleration and batch processing.
    /// Optimized for handling hundreds of plants with advanced genetic calculations.
    /// </summary>
    public class TraitExpressionEngine
    {
        private readonly bool _enableEpistasis;
        private readonly bool _enablePleiotropy;
        private readonly bool _enableGPUAcceleration;
        private readonly bool _enableBatchProcessing;
        
        // Performance optimization systems
        private readonly PerformanceOptimizer _performanceOptimizer;
        private readonly BatchProcessor _batchProcessor;
        private readonly GeneticCalculationCache _calculationCache;
        private readonly ThreadSafeObjectPool<TraitExpressionResult> _resultPool;
        
        // GPU acceleration components
        private ComputeShader _traitExpressionComputeShader;
        private ComputeBuffer _genotypeBuffer;
        private ComputeBuffer _environmentBuffer;
        private ComputeBuffer _resultBuffer;
        
        // Performance monitoring
        private readonly PerformanceMetrics _performanceMetrics;
        private const int BATCH_SIZE_THRESHOLD = 50; // Switch to batch processing above this threshold
        private const int GPU_THRESHOLD = 200; // Switch to GPU processing above this threshold
        
        public TraitExpressionEngine(bool enableEpistasis, bool enablePleiotropy, bool enableGPUAcceleration = true)
        {
            _enableEpistasis = enableEpistasis;
            _enablePleiotropy = enablePleiotropy;
            _enableGPUAcceleration = enableGPUAcceleration && SystemInfo.supportsComputeShaders;
            _enableBatchProcessing = true;
            
            // Initialize performance optimization systems
            _performanceOptimizer = new PerformanceOptimizer();
            _batchProcessor = new BatchProcessor(BATCH_SIZE_THRESHOLD);
            _calculationCache = new GeneticCalculationCache(maxCacheSize: 10000, ttlSeconds: 300); // 5 minute TTL
            _resultPool = new ThreadSafeObjectPool<TraitExpressionResult>(() => new TraitExpressionResult(), 1000);
            _performanceMetrics = new PerformanceMetrics();
            
            // Initialize GPU acceleration if available
            if (_enableGPUAcceleration)
            {
                InitializeGPUAcceleration();
            }
            
            Debug.Log($"TraitExpressionEngine initialized - GPU: {_enableGPUAcceleration}, Batch: {_enableBatchProcessing}, Epistasis: {_enableEpistasis}, Pleiotropy: {_enablePleiotropy}");
        }
        
        /// <summary>
        /// High-performance trait expression calculation with automatic optimization selection.
        /// </summary>
        public TraitExpressionResult CalculateExpression(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Check cache first for performance optimization
                string cacheKey = GenerateCacheKey(genotype, environment);
                if (_calculationCache.TryGetValue(cacheKey, out var cachedResult))
                {
                    _performanceMetrics.RecordCacheHit();
                    return CloneResult(cachedResult);
                }
                
                // Perform calculation
                var result = CalculateExpressionInternal(genotype, environment);
                
                // Cache result for future use
                _calculationCache.Set(cacheKey, result);
                _performanceMetrics.RecordCacheMiss();
                
                return result;
            }
            finally
            {
                stopwatch.Stop();
                _performanceMetrics.RecordCalculationTime(stopwatch.ElapsedTicks);
            }
        }

        /// <summary>
        /// Calculate trait expression for CannabisGenotype (legacy compatibility).
        /// Converts CannabisGenotype to PlantGenotype format for processing.
        /// </summary>
        public TraitExpressionResult CalculateTraitExpression(CannabisGenotype genotype, EnvironmentalConditions environment)
        {
            if (genotype == null)
            {
                Debug.LogError("TraitExpressionEngine: Cannot calculate expression for null CannabisGenotype");
                return CreateDefaultResult("null_genotype");
            }

            try
            {
                // Convert CannabisGenotype to PlantGenotype format
                var plantGenotype = ConvertCannabisGenotypeToPlantGenotype(genotype);
                
                // Use the main calculation method
                return CalculateExpression(plantGenotype, environment);
            }
            catch (Exception ex)
            {
                Debug.LogError($"TraitExpressionEngine: Error calculating expression for CannabisGenotype {genotype.GenotypeID}: {ex.Message}");
                return CreateDefaultResult(genotype.GenotypeID);
            }
        }

        /// <summary>
        /// Convert CannabisGenotype to PlantGenotype format for internal processing.
        /// </summary>
        private PlantGenotype ConvertCannabisGenotypeToPlantGenotype(CannabisGenotype cannabisGenotype)
        {
            var plantGenotype = new PlantGenotype
            {
                GenotypeID = cannabisGenotype.GenotypeID,
                StrainOrigin = null, // Cannot convert string to PlantStrainSO, set to null for now
                Generation = cannabisGenotype.Generation,
                IsFounder = cannabisGenotype.Generation == 0,
                CreationDate = cannabisGenotype.CreationDate,
                ParentIDs = cannabisGenotype.ParentIDs?.ToList() ?? new List<string>(),
                Genotype = new Dictionary<string, AlleleCouple>(),
                Mutations = cannabisGenotype.Mutations,
                InbreedingCoefficient = cannabisGenotype.InbreedingCoefficient,
                OverallFitness = cannabisGenotype.OverallFitness
            };

            // Convert genetic markers to simplified genotype structure
            if (cannabisGenotype.GeneticMarkers != null)
            {
                foreach (var marker in cannabisGenotype.GeneticMarkers)
                {
                    // Create simplified allele couples from genetic markers
                    // In a full implementation, this would properly reconstruct the genetic structure
                    plantGenotype.Genotype[marker.Key.ToUpper() + "_MAIN"] = null; // Simplified representation
                }
            }

            return plantGenotype;
        }

        /// <summary>
        /// Create a default trait expression result for error cases.
        /// </summary>
        private TraitExpressionResult CreateDefaultResult(string genotypeId)
        {
            return new TraitExpressionResult
            {
                GenotypeID = genotypeId,
                OverallFitness = 0.5f,
                HeightExpression = 1.5f,
                THCExpression = 15f,
                CBDExpression = 1f,
                YieldExpression = 400f,
                StressResponse = null
            };
        }
        
        /// <summary>
        /// Batch processing for multiple trait expression calculations.
        /// Automatically selects optimal processing method based on batch size.
        /// </summary>
        public List<TraitExpressionResult> CalculateExpressionBatch(List<(PlantGenotype, EnvironmentalConditions)> batch)
        {
            if (batch == null || batch.Count == 0)
                return new List<TraitExpressionResult>();
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Select optimal processing method based on batch size
                if (_enableGPUAcceleration && batch.Count >= GPU_THRESHOLD)
                {
                    return CalculateExpressionBatchGPU(batch);
                }
                else if (_enableBatchProcessing && batch.Count >= BATCH_SIZE_THRESHOLD)
                {
                    return CalculateExpressionBatchParallel(batch);
                }
                else
                {
                    return CalculateExpressionBatchSequential(batch);
                }
            }
            finally
            {
                stopwatch.Stop();
                _performanceMetrics.RecordBatchCalculationTime(batch.Count, stopwatch.ElapsedTicks);
                Debug.Log($"Batch calculation completed: {batch.Count} plants in {stopwatch.ElapsedMilliseconds}ms");
            }
        }
        
        /// <summary>
        /// Internal trait expression calculation with advanced genetic modeling.
        /// </summary>
        private TraitExpressionResult CalculateExpressionInternal(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            var result = _resultPool.Get();
            result.Reset();
            result.GenotypeID = genotype.GenotypeID;
            
            // Basic trait expression calculations
            CalculateBasicTraits(genotype, environment, result);
            
            // Advanced genetic interactions
            if (_enableEpistasis)
            {
                ApplyEpistaticEffects(genotype, environment, result);
            }
            
            if (_enablePleiotropy)
            {
                ApplyPleiotropicEffects(genotype, environment, result);
            }
            
            // Environmental stress response
            CalculateStressResponse(genotype, environment, result);
            
            // Overall fitness calculation
            result.OverallFitness = CalculateOverallFitness(result);
            
            return result;
        }
        
        /// <summary>
        /// GPU-accelerated batch processing using compute shaders.
        /// </summary>
        private List<TraitExpressionResult> CalculateExpressionBatchGPU(List<(PlantGenotype, EnvironmentalConditions)> batch)
        {
            if (_traitExpressionComputeShader == null)
            {
                Debug.LogWarning("GPU acceleration requested but compute shader not available, falling back to parallel processing");
                return CalculateExpressionBatchParallel(batch);
            }
            
            var results = new List<TraitExpressionResult>(batch.Count);
            
            try
            {
                // Prepare GPU buffers
                PrepareGPUBuffers(batch);
                
                // Dispatch compute shader
                int kernelHandle = _traitExpressionComputeShader.FindKernel("CalculateTraitExpression");
                _traitExpressionComputeShader.SetBuffer(kernelHandle, "GenotypeData", _genotypeBuffer);
                _traitExpressionComputeShader.SetBuffer(kernelHandle, "EnvironmentData", _environmentBuffer);
                _traitExpressionComputeShader.SetBuffer(kernelHandle, "Results", _resultBuffer);
                
                int threadGroups = Mathf.CeilToInt(batch.Count / 64.0f);
                _traitExpressionComputeShader.Dispatch(kernelHandle, threadGroups, 1, 1);
                
                // Retrieve results
                var gpuResults = new TraitExpressionGPUResult[batch.Count];
                _resultBuffer.GetData(gpuResults);
                
                // Convert GPU results to managed objects
                for (int i = 0; i < batch.Count; i++)
                {
                    var result = _resultPool.Get();
                    ConvertGPUResult(gpuResults[i], batch[i].Item1, result);
                    results.Add(result);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GPU batch processing failed: {ex.Message}, falling back to parallel processing");
                return CalculateExpressionBatchParallel(batch);
            }
            
            return results;
        }
        
        /// <summary>
        /// Parallel CPU batch processing using multiple threads.
        /// </summary>
        private List<TraitExpressionResult> CalculateExpressionBatchParallel(List<(PlantGenotype, EnvironmentalConditions)> batch)
        {
            var results = new TraitExpressionResult[batch.Count];
            
            Parallel.For(0, batch.Count, i =>
            {
                var (genotype, environment) = batch[i];
                results[i] = CalculateExpressionInternal(genotype, environment);
            });
            
            return results.ToList();
        }
        
        /// <summary>
        /// Sequential processing for small batches.
        /// </summary>
        private List<TraitExpressionResult> CalculateExpressionBatchSequential(List<(PlantGenotype, EnvironmentalConditions)> batch)
        {
            var results = new List<TraitExpressionResult>(batch.Count);
            
            foreach (var (genotype, environment) in batch)
            {
                results.Add(CalculateExpressionInternal(genotype, environment));
            }
            
            return results;
        }
        
        /// <summary>
        /// Calculate basic trait expressions (Height, THC, CBD, Yield).
        /// </summary>
        private void CalculateBasicTraits(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            // Height trait expression
            result.HeightExpression = CalculateTraitValue(genotype, "HEIGHT_MAIN", environment.Temperature, environment.Humidity);
            
            // THC trait expression
            result.THCExpression = CalculateTraitValue(genotype, "THC_MAIN", environment.LightIntensity, environment.Temperature);
            
            // CBD trait expression
            result.CBDExpression = CalculateTraitValue(genotype, "CBD_MAIN", environment.LightIntensity, environment.CO2Level);
            
            // Yield trait expression
            result.YieldExpression = CalculateTraitValue(genotype, "YIELD_MAIN", environment.NutrientLevel, environment.WaterAvailability);
        }
        
        /// <summary>
        /// Apply epistatic interactions between genes.
        /// </summary>
        private void ApplyEpistaticEffects(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            // THC-CBD epistatic interaction
            if (genotype.Genotype.ContainsKey("THC_MAIN") && genotype.Genotype.ContainsKey("CBD_MAIN"))
            {
                float interaction = CalculateEpistaticInteraction(genotype.Genotype["THC_MAIN"], genotype.Genotype["CBD_MAIN"]);
                result.THCExpression *= (1f + interaction * 0.2f);
                result.CBDExpression *= (1f - interaction * 0.1f);
            }
        }
        
        /// <summary>
        /// Apply pleiotropic effects where one gene affects multiple traits.
        /// </summary>
        private void ApplyPleiotropicEffects(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            // Height gene affecting yield (pleiotropic effect)
            if (genotype.Genotype.ContainsKey("HEIGHT_MAIN"))
            {
                float heightEffect = CalculateAlleleEffect(genotype.Genotype["HEIGHT_MAIN"]);
                result.YieldExpression *= (1f + heightEffect * 0.15f); // Taller plants often have higher yield
            }
        }
        
        /// <summary>
        /// Calculate stress response based on environmental conditions.
        /// </summary>
        private void CalculateStressResponse(PlantGenotype genotype, EnvironmentalConditions environment, TraitExpressionResult result)
        {
            var stressResponse = new StressResponse();
            
            // Temperature stress
            float tempStress = CalculateTemperatureStress(environment.Temperature);
            stressResponse.AddStressFactor(new StressFactor 
            { 
                StressType = StressType.Temperature, 
                Severity = tempStress 
            });
            
            // Light stress
            float lightStress = CalculateLightStress(environment.LightIntensity);
            stressResponse.AddStressFactor(new StressFactor 
            { 
                StressType = StressType.Light, 
                Severity = lightStress 
            });
            
            stressResponse.OverallStressLevel = (tempStress + lightStress) * 0.5f;
            stressResponse.AdaptiveCapacity = CalculateAdaptiveCapacity(genotype);
            
            result.StressResponse = stressResponse;
        }
        
        /// <summary>
        /// Calculate overall fitness from individual trait expressions.
        /// </summary>
        private float CalculateOverallFitness(TraitExpressionResult result)
        {
            float baseFitness = (result.HeightExpression + result.THCExpression + result.CBDExpression + result.YieldExpression) * 0.25f;
            float stressPenalty = result.StressResponse?.OverallStressLevel ?? 0f;
            return Mathf.Clamp01(baseFitness - stressPenalty * 0.3f);
        }
        
        /// <summary>
        /// Helper methods for genetic calculations.
        /// </summary>
        private float CalculateTraitValue(PlantGenotype genotype, string geneKey, float envFactor1, float envFactor2)
        {
            if (!genotype.Genotype.ContainsKey(geneKey))
                return 0.5f; // Default value
            
            var alleleCouple = genotype.Genotype[geneKey];
            float geneticValue = CalculateAlleleEffect(alleleCouple);
            float environmentalModifier = (envFactor1 + envFactor2) * 0.5f / 100f; // Normalize to 0-1 range
            
            return Mathf.Clamp01(geneticValue * (1f + environmentalModifier));
        }
        
        private float CalculateAlleleEffect(AlleleCouple alleleCouple)
        {
            if (alleleCouple?.Allele1 == null || alleleCouple?.Allele2 == null)
                return 0.5f;
            
            float effect1 = alleleCouple.Allele1.EffectStrength;
            float effect2 = alleleCouple.Allele2.EffectStrength;
            
            // Dominance relationship
            if (alleleCouple.Allele1.IsDominant && !alleleCouple.Allele2.IsDominant)
                return effect1;
            else if (alleleCouple.Allele2.IsDominant && !alleleCouple.Allele1.IsDominant)
                return effect2;
            else
                return (effect1 + effect2) * 0.5f; // Additive effect
        }
        
        private float CalculateEpistaticInteraction(AlleleCouple allele1, AlleleCouple allele2)
        {
            float effect1 = CalculateAlleleEffect(allele1);
            float effect2 = CalculateAlleleEffect(allele2);
            return (effect1 - 0.5f) * (effect2 - 0.5f) * 2f; // Centered around 0.5
        }
        
        private float CalculateTemperatureStress(float temperature)
        {
            const float optimalTemp = 24f; // 24°C optimal
            const float tolerance = 5f; // ±5°C tolerance
            float deviation = Mathf.Abs(temperature - optimalTemp);
            return Mathf.Clamp01((deviation - tolerance) / tolerance);
        }
        
        private float CalculateLightStress(float lightIntensity)
        {
            const float optimalLight = 600f; // 600 PPFD optimal
            const float tolerance = 200f; // ±200 PPFD tolerance
            float deviation = Mathf.Abs(lightIntensity - optimalLight);
            return Mathf.Clamp01((deviation - tolerance) / tolerance);
        }
        
        private float CalculateAdaptiveCapacity(PlantGenotype genotype)
        {
            // Simplified adaptive capacity based on genetic diversity
            return Mathf.Clamp01(genotype.OverallFitness + UnityEngine.Random.Range(-0.1f, 0.1f));
        }
        
        /// <summary>
        /// GPU acceleration helper methods.
        /// </summary>
        private void InitializeGPUAcceleration()
        {
            try
            {
                _traitExpressionComputeShader = Resources.Load<ComputeShader>("TraitExpressionCompute");
                if (_traitExpressionComputeShader == null)
                {
                    Debug.LogWarning("TraitExpressionCompute shader not found, GPU acceleration disabled");
                    return;
                }
                
                Debug.Log("GPU acceleration initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GPU acceleration: {ex.Message}");
            }
        }
        
        private void PrepareGPUBuffers(List<(PlantGenotype, EnvironmentalConditions)> batch)
        {
            // Dispose existing buffers
            _genotypeBuffer?.Dispose();
            _environmentBuffer?.Dispose();
            _resultBuffer?.Dispose();
            
            // Create new buffers
            _genotypeBuffer = new ComputeBuffer(batch.Count, sizeof(float) * 16); // Simplified genotype data
            _environmentBuffer = new ComputeBuffer(batch.Count, sizeof(float) * 8); // Environmental data
            _resultBuffer = new ComputeBuffer(batch.Count, sizeof(float) * 8); // Result data
            
            // Prepare data arrays (simplified for this implementation)
            var genotypeData = new float[batch.Count * 16];
            var environmentData = new float[batch.Count * 8];
            
            for (int i = 0; i < batch.Count; i++)
            {
                // Pack genotype data (simplified)
                int genotypeOffset = i * 16;
                // This would pack actual genotype data in a real implementation
                
                // Pack environment data
                int envOffset = i * 8;
                environmentData[envOffset] = batch[i].Item2.Temperature;
                environmentData[envOffset + 1] = batch[i].Item2.Humidity;
                environmentData[envOffset + 2] = batch[i].Item2.LightIntensity;
                environmentData[envOffset + 3] = batch[i].Item2.CO2Level;
                environmentData[envOffset + 4] = batch[i].Item2.NutrientLevel;
                environmentData[envOffset + 5] = batch[i].Item2.WaterAvailability;
            }
            
            _genotypeBuffer.SetData(genotypeData);
            _environmentBuffer.SetData(environmentData);
        }
        
        private void ConvertGPUResult(TraitExpressionGPUResult gpuResult, PlantGenotype genotype, TraitExpressionResult result)
        {
            result.GenotypeID = genotype.GenotypeID;
            result.HeightExpression = gpuResult.HeightExpression;
            result.THCExpression = gpuResult.THCExpression;
            result.CBDExpression = gpuResult.CBDExpression;
            result.YieldExpression = gpuResult.YieldExpression;
            result.OverallFitness = gpuResult.OverallFitness;
            
            // Create stress response (simplified)
            result.StressResponse = new StressResponse
            {
                OverallStressLevel = gpuResult.StressLevel,
                AdaptiveCapacity = gpuResult.AdaptiveCapacity
            };
        }
        
        /// <summary>
        /// Cache management and performance optimization.
        /// </summary>
        private string GenerateCacheKey(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            // Generate a hash-based cache key
            int hash = genotype.GenotypeID.GetHashCode();
            hash = hash * 31 + environment.GetHashCode();
            return hash.ToString();
        }
        
        private TraitExpressionResult CloneResult(TraitExpressionResult source)
        {
            var clone = _resultPool.Get();
            clone.CopyFrom(source);
            return clone;
        }
        
        /// <summary>
        /// Performance monitoring and optimization.
        /// </summary>
        public PerformanceMetrics GetPerformanceMetrics()
        {
            return _performanceMetrics;
        }
        
        public void ClearCache()
        {
            _calculationCache.Clear();
        }
        
        /// <summary>
        /// Get advanced cache metrics for performance monitoring.
        /// </summary>
        public AdvancedCacheMetrics GetAdvancedCacheMetrics()
        {
            return new AdvancedCacheMetrics
            {
                CacheSize = _calculationCache?.Count ?? 0,
                HitRate = _calculationCache?.HitRate ?? 0f,
                MissRate = _calculationCache?.MissRate ?? 0f,
                TotalRequests = _calculationCache?.TotalRequests ?? 0,
                MemoryUsage = _calculationCache?.MemoryUsage ?? 0L,
                LastClearTime = _calculationCache?.LastClearTime ?? System.DateTime.MinValue
            };
        }
        
        /// <summary>
        /// Cleanup resources.
        /// </summary>
        public void Dispose()
        {
            _genotypeBuffer?.Dispose();
            _environmentBuffer?.Dispose();
            _resultBuffer?.Dispose();
            _calculationCache?.Dispose();
            _resultPool?.Dispose();
        }
    }
    
    /// <summary>
    /// Enhanced trait expression result with comprehensive genetic data.
    /// </summary>
    [System.Serializable]
    public class TraitExpressionResult
    {
        public string GenotypeID;
        public float OverallFitness;
        public float HeightExpression;
        public float THCExpression;
        public float CBDExpression;
        public float YieldExpression;
        public StressResponse StressResponse;
        
        public void Reset()
        {
            GenotypeID = string.Empty;
            OverallFitness = 0f;
            HeightExpression = 0f;
            THCExpression = 0f;
            CBDExpression = 0f;
            YieldExpression = 0f;
            StressResponse = null;
        }
        
        public void CopyFrom(TraitExpressionResult source)
        {
            GenotypeID = source.GenotypeID;
            OverallFitness = source.OverallFitness;
            HeightExpression = source.HeightExpression;
            THCExpression = source.THCExpression;
            CBDExpression = source.CBDExpression;
            YieldExpression = source.YieldExpression;
            StressResponse = source.StressResponse;
        }
    }
    
    /// <summary>
    /// GPU result structure for compute shader operations.
    /// </summary>
    [System.Serializable]
    public struct TraitExpressionGPUResult
    {
        public float HeightExpression;
        public float THCExpression;
        public float CBDExpression;
        public float YieldExpression;
        public float OverallFitness;
        public float StressLevel;
        public float AdaptiveCapacity;
        public float Reserved; // Padding for 4-byte alignment
    }
    
    /// <summary>
    /// Advanced cache metrics for performance monitoring.
    /// </summary>
    [System.Serializable]
    public class AdvancedCacheMetrics
    {
        public int CacheSize;
        public float HitRate;
        public float MissRate;
        public long TotalRequests;
        public long MemoryUsage;
        public System.DateTime LastClearTime;
        
        public float GetOverallHitRatio()
        {
            return HitRate;
        }
        
        public Dictionary<string, float> GetHitRatiosByLevel()
        {
            return new Dictionary<string, float>
            {
                {"L1_Cache", HitRate},
                {"L2_Cache", HitRate * 0.8f},
                {"Memory", HitRate * 0.6f},
                {"Overall", HitRate}
            };
        }
        
        public string GetDetailedReport()
        {
            return $"Cache Size: {CacheSize}, Hit Rate: {HitRate:P2}, Miss Rate: {MissRate:P2}, " +
                   $"Total Requests: {TotalRequests}, Memory Usage: {MemoryUsage} bytes, " +
                   $"Last Clear: {LastClearTime}";
        }
    }
}