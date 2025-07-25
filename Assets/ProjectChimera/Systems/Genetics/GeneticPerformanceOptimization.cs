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
    /// Phase 3.2: Performance optimization components for genetic calculations.
    /// Includes caching, object pooling, batch processing, and performance monitoring.
    /// </summary>
    
    /// <summary>
    /// High-performance cache for genetic calculations with TTL and memory management.
    /// </summary>
    public class GeneticCalculationCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache;
        private readonly int _maxCacheSize;
        private readonly float _ttlSeconds;
        private readonly Timer _cleanupTimer;
        private readonly object _cleanupLock = new object();
        
        // Performance tracking fields
        private long _totalRequests = 0;
        private long _cacheHits = 0;
        private long _cacheMisses = 0;
        private DateTime _lastClearTime = DateTime.Now;
        
        private struct CacheEntry
        {
            public TraitExpressionResult Value;
            public DateTime ExpiryTime;
            public long AccessCount;
            public DateTime LastAccessed;
        }
        
        public GeneticCalculationCache(int maxCacheSize = 10000, float ttlSeconds = 300f)
        {
            _cache = new ConcurrentDictionary<string, CacheEntry>();
            _maxCacheSize = maxCacheSize;
            _ttlSeconds = ttlSeconds;
            
            // Setup cleanup timer to run every minute
            _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
        
        public bool TryGetValue(string key, out TraitExpressionResult value)
        {
            value = null;
            Interlocked.Increment(ref _totalRequests);
            
            if (_cache.TryGetValue(key, out var entry))
            {
                if (DateTime.Now < entry.ExpiryTime)
                {
                    // Update access statistics
                    entry.AccessCount++;
                    entry.LastAccessed = DateTime.Now;
                    _cache[key] = entry;
                    
                    value = entry.Value;
                    Interlocked.Increment(ref _cacheHits);
                    return true;
                }
                else
                {
                    // Remove expired entry
                    _cache.TryRemove(key, out _);
                }
            }
            
            Interlocked.Increment(ref _cacheMisses);
            return false;
        }
        
        public void Set(string key, TraitExpressionResult value)
        {
            var entry = new CacheEntry
            {
                Value = value,
                ExpiryTime = DateTime.Now.AddSeconds(_ttlSeconds),
                AccessCount = 1,
                LastAccessed = DateTime.Now
            };
            
            _cache[key] = entry;
            
            // Check if cache size limit is exceeded
            if (_cache.Count > _maxCacheSize)
            {
                Task.Run(EvictLeastRecentlyUsed);
            }
        }
        
        public void Clear()
        {
            _cache.Clear();
            _lastClearTime = DateTime.Now;
        }
        
        public (int count, float hitRatio) GetStatistics()
        {
            int count = _cache.Count;
            float hitRatio = _totalRequests > 0 ? (float)_cacheHits / _totalRequests : 0f;
            return (count, hitRatio);
        }
        
        // Properties for GetAdvancedCacheMetrics compatibility
        public int Count => _cache.Count;
        public float HitRate => _totalRequests > 0 ? (float)_cacheHits / _totalRequests : 0f;
        public float MissRate => _totalRequests > 0 ? (float)_cacheMisses / _totalRequests : 0f;
        public long TotalRequests => _totalRequests;
        public long MemoryUsage => _cache.Count * 256; // Approximate memory usage estimate
        public DateTime LastClearTime => _lastClearTime;
        
        private void CleanupExpiredEntries(object state)
        {
            if (!Monitor.TryEnter(_cleanupLock, TimeSpan.FromSeconds(1)))
                return;
            
            try
            {
                var now = DateTime.Now;
                var expiredKeys = _cache
                    .Where(kvp => now >= kvp.Value.ExpiryTime)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }
                
                if (expiredKeys.Count > 0)
                {
                    Debug.Log($"GeneticCalculationCache: Cleaned up {expiredKeys.Count} expired entries");
                }
            }
            finally
            {
                Monitor.Exit(_cleanupLock);
            }
        }
        
        private void EvictLeastRecentlyUsed()
        {
            try
            {
                var entriesToRemove = _cache.Count - (int)(_maxCacheSize * 0.8f); // Remove 20% when limit exceeded
                
                var lruEntries = _cache
                    .OrderBy(kvp => kvp.Value.LastAccessed)
                    .Take(entriesToRemove)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (var key in lruEntries)
                {
                    _cache.TryRemove(key, out _);
                }
                
                Debug.Log($"GeneticCalculationCache: Evicted {lruEntries.Count} LRU entries");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during LRU eviction: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _cache?.Clear();
            _lastClearTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Thread-safe object pool for genetic calculation results.
    /// </summary>
    public class ThreadSafeObjectPool<T> : IDisposable where T : class, new()
    {
        private readonly ConcurrentQueue<T> _objects;
        private readonly Func<T> _objectGenerator;
        private readonly int _maxSize;
        private int _currentCount;
        
        public ThreadSafeObjectPool(Func<T> objectGenerator, int maxSize = 1000)
        {
            _objects = new ConcurrentQueue<T>();
            _objectGenerator = objectGenerator ?? (() => new T());
            _maxSize = maxSize;
            _currentCount = 0;
        }
        
        public T Get()
        {
            if (_objects.TryDequeue(out T item))
            {
                Interlocked.Decrement(ref _currentCount);
                return item;
            }
            
            return _objectGenerator();
        }
        
        public void Return(T item)
        {
            if (item != null && _currentCount < _maxSize)
            {
                _objects.Enqueue(item);
                Interlocked.Increment(ref _currentCount);
            }
        }
        
        public int Count => _currentCount;
        
        public void Dispose()
        {
            while (_objects.TryDequeue(out _)) { }
            _currentCount = 0;
        }
    }
    
    /// <summary>
    /// Batch processor for optimizing multiple genetic calculations.
    /// </summary>
    public class BatchProcessor
    {
        private readonly int _batchSizeThreshold;
        private readonly Queue<(PlantGenotype, EnvironmentalConditions)> _pendingCalculations;
        private readonly object _queueLock = new object();
        
        public BatchProcessor(int batchSizeThreshold = 50)
        {
            _batchSizeThreshold = batchSizeThreshold;
            _pendingCalculations = new Queue<(PlantGenotype, EnvironmentalConditions)>();
        }
        
        public void AddCalculation(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            lock (_queueLock)
            {
                _pendingCalculations.Enqueue((genotype, environment));
            }
        }
        
        public bool ShouldProcessBatch()
        {
            lock (_queueLock)
            {
                return _pendingCalculations.Count >= _batchSizeThreshold;
            }
        }
        
        public List<(PlantGenotype, EnvironmentalConditions)> GetBatch()
        {
            lock (_queueLock)
            {
                var batch = new List<(PlantGenotype, EnvironmentalConditions)>();
                
                while (_pendingCalculations.Count > 0 && batch.Count < _batchSizeThreshold)
                {
                    batch.Add(_pendingCalculations.Dequeue());
                }
                
                return batch;
            }
        }
        
        public int PendingCount
        {
            get
            {
                lock (_queueLock)
                {
                    return _pendingCalculations.Count;
                }
            }
        }
    }
    
    /// <summary>
    /// Performance metrics collector for genetic calculations.
    /// </summary>
    public class PerformanceMetrics
    {
        private long _totalCalculations;
        private long _totalCalculationTicks;
        private long _cacheHits;
        private long _cacheMisses;
        private long _batchCalculations;
        private long _batchCalculationTicks;
        private readonly object _statsLock = new object();
        
        public void RecordCalculationTime(long ticks)
        {
            lock (_statsLock)
            {
                _totalCalculations++;
                _totalCalculationTicks += ticks;
            }
        }
        
        public void RecordBatchCalculationTime(int batchSize, long ticks)
        {
            lock (_statsLock)
            {
                _batchCalculations++;
                _batchCalculationTicks += ticks;
            }
        }
        
        public void RecordCacheHit()
        {
            Interlocked.Increment(ref _cacheHits);
        }
        
        public void RecordCacheMiss()
        {
            Interlocked.Increment(ref _cacheMisses);
        }
        
        public GeneticsPerformanceStats GetStats()
        {
            lock (_statsLock)
            {
                return new GeneticsPerformanceStats
                {
                    TotalCalculations = _totalCalculations,
                    AverageCalculationTimeMs = _totalCalculations > 0 ? 
                        (_totalCalculationTicks / _totalCalculations) / 10000.0 : 0.0,
                    CacheHitRatio = _cacheHits + _cacheMisses > 0 ? 
                        (double)_cacheHits / (_cacheHits + _cacheMisses) : 0.0,
                    BatchCalculations = _batchCalculations,
                    AverageBatchTimeMs = _batchCalculations > 0 ? 
                        (_batchCalculationTicks / _batchCalculations) / 10000.0 : 0.0
                };
            }
        }
        
        public void Reset()
        {
            lock (_statsLock)
            {
                _totalCalculations = 0;
                _totalCalculationTicks = 0;
                _cacheHits = 0;
                _cacheMisses = 0;
                _batchCalculations = 0;
                _batchCalculationTicks = 0;
            }
        }
    }
    
    /// <summary>
    /// Performance optimizer that automatically adjusts processing methods.
    /// </summary>
    public class PerformanceOptimizer
    {
        private readonly Queue<double> _recentCalculationTimes;
        private readonly int _maxSamples = 100;
        private ProcessingMethod _recommendedMethod = ProcessingMethod.Sequential;
        
        public enum ProcessingMethod
        {
            Sequential,
            Parallel,
            GPU
        }
        
        public void RecordPerformance(ProcessingMethod method, double calculationTimeMs, int itemCount)
        {
            _recentCalculationTimes.Enqueue(calculationTimeMs / itemCount); // Per-item time
            
            if (_recentCalculationTimes.Count > _maxSamples)
            {
                _recentCalculationTimes.Dequeue();
            }
            
            // Update recommendation based on recent performance
            UpdateRecommendation();
        }
        
        public ProcessingMethod GetRecommendedMethod(int itemCount)
        {
            // Base recommendation on item count and recent performance
            if (itemCount >= 200 && SystemInfo.supportsComputeShaders)
                return ProcessingMethod.GPU;
            else if (itemCount >= 50)
                return ProcessingMethod.Parallel;
            else
                return ProcessingMethod.Sequential;
        }
        
        private void UpdateRecommendation()
        {
            if (_recentCalculationTimes.Count < 10)
                return;
            
            double averageTime = _recentCalculationTimes.Average();
            
            // Adjust recommendations based on performance trends
            if (averageTime > 10.0) // ms per item
            {
                if (_recommendedMethod == ProcessingMethod.Sequential)
                    _recommendedMethod = ProcessingMethod.Parallel;
                else if (_recommendedMethod == ProcessingMethod.Parallel && SystemInfo.supportsComputeShaders)
                    _recommendedMethod = ProcessingMethod.GPU;
            }
        }
        
        public PerformanceOptimizer()
        {
            _recentCalculationTimes = new Queue<double>();
        }
    }
    
    /// <summary>
    /// Performance statistics structure.
    /// </summary>
    [Serializable]
    public struct GeneticsPerformanceStats
    {
        public long TotalCalculations;
        public double AverageCalculationTimeMs;
        public double CacheHitRatio;
        public long BatchCalculations;
        public double AverageBatchTimeMs;
        
        public override string ToString()
        {
            return $"Calculations: {TotalCalculations}, Avg Time: {AverageCalculationTimeMs:F2}ms, " +
                   $"Cache Hit Rate: {CacheHitRatio:P1}, Batches: {BatchCalculations}, " +
                   $"Batch Avg: {AverageBatchTimeMs:F2}ms";
        }
    }
    
    /// <summary>
    /// Stress response data structures for environmental adaptation.
    /// </summary>
    public enum StressType
    {
        Temperature,
        Heat,
        Cold,
        Light,
        Water,
        Drought,
        Flood,
        Nutrient,
        Atmospheric,
        Humidity,
        CO2,
        pH,
        Salinity,
        Toxicity,
        Physical,
        Biotic
    }
    
    [Serializable]
    public class StressFactor
    {
        public StressType StressType;
        public float Severity; // 0-1 range
        public float Duration; // Time in hours
        public float RecoveryRate; // Recovery speed
    }
    
    [Serializable]
    public class StressResponse
    {
        public float OverallStressLevel; // 0-1 range
        public float AdaptiveCapacity; // Plant's ability to adapt
        public List<StressFactor> ActiveStresses = new List<StressFactor>();
        
        public void AddStressFactor(StressFactor factor)
        {
            ActiveStresses.Add(factor);
            RecalculateOverallStress();
        }
        
        private void RecalculateOverallStress()
        {
            if (ActiveStresses.Count == 0)
            {
                OverallStressLevel = 0f;
                return;
            }
            
            float totalStress = ActiveStresses.Sum(s => s.Severity);
            OverallStressLevel = Mathf.Clamp01(totalStress / ActiveStresses.Count);
        }
    }
    
    /// <summary>
    /// Phase 3.2: Genetic performance monitor for tracking and optimizing genetic calculation performance.
    /// </summary>
    public class GeneticPerformanceMonitor
    {
        private readonly Queue<PerformanceDataPoint> _performanceHistory;
        private readonly int _maxHistorySize = 100;
        private readonly object _historyLock = new object();
        
        private struct PerformanceDataPoint
        {
            public DateTime Timestamp;
            public int PlantCount;
            public double UpdateTimeMs;
            public GeneticsPerformanceStats GeneticStats;
        }
        
        public GeneticPerformanceMonitor()
        {
            _performanceHistory = new Queue<PerformanceDataPoint>();
        }
        
        /// <summary>
        /// Record a batch update performance data point.
        /// </summary>
        public void RecordBatchUpdate(int plantCount, GeneticsPerformanceStats geneticStats)
        {
            lock (_historyLock)
            {
                var dataPoint = new PerformanceDataPoint
                {
                    Timestamp = DateTime.Now,
                    PlantCount = plantCount,
                    UpdateTimeMs = geneticStats.AverageBatchTimeMs,
                    GeneticStats = geneticStats
                };
                
                _performanceHistory.Enqueue(dataPoint);
                
                if (_performanceHistory.Count > _maxHistorySize)
                {
                    _performanceHistory.Dequeue();
                }
            }
        }
        
        /// <summary>
        /// Get aggregated performance data from recent history.
        /// </summary>
        public PerformanceData GetPerformanceData()
        {
            lock (_historyLock)
            {
                if (_performanceHistory.Count == 0)
                {
                    return new PerformanceData
                    {
                        AverageUpdateTimeMs = 10.0f,
                        AveragePlantCount = 10,
                        CacheHitRatio = 0.0f,
                        TotalSamples = 0
                    };
                }
                
                double totalUpdateTime = 0;
                int totalPlants = 0;
                double totalCacheHitRatio = 0;
                int validSamples = 0;
                
                foreach (var dataPoint in _performanceHistory)
                {
                    totalUpdateTime += dataPoint.UpdateTimeMs;
                    totalPlants += dataPoint.PlantCount;
                    totalCacheHitRatio += dataPoint.GeneticStats.CacheHitRatio;
                    validSamples++;
                }
                
                return new PerformanceData
                {
                    AverageUpdateTimeMs = (float)(totalUpdateTime / validSamples),
                    AveragePlantCount = totalPlants / validSamples,
                    CacheHitRatio = (float)(totalCacheHitRatio / validSamples),
                    TotalSamples = validSamples
                };
            }
        }
        
        /// <summary>
        /// Get detailed performance analytics.
        /// </summary>
        public PerformanceAnalytics GetAnalytics()
        {
            lock (_historyLock)
            {
                if (_performanceHistory.Count < 5)
                {
                    return new PerformanceAnalytics
                    {
                        PerformanceTrend = PerformanceTrend.Stable,
                        OptimalBatchSize = 20,
                        RecommendedOptimizations = new List<string>()
                    };
                }
                
                var recentData = _performanceHistory.TakeLast(10).ToArray();
                var olderData = _performanceHistory.Take(_performanceHistory.Count - 10).ToArray();
                
                double recentAvg = recentData.Average(d => d.UpdateTimeMs);
                double olderAvg = olderData.Length > 0 ? olderData.Average(d => d.UpdateTimeMs) : recentAvg;
                
                PerformanceTrend trend;
                if (recentAvg > olderAvg * 1.1f)
                    trend = PerformanceTrend.Declining;
                else if (recentAvg < olderAvg * 0.9f)
                    trend = PerformanceTrend.Improving;
                else
                    trend = PerformanceTrend.Stable;
                
                var recommendations = GenerateOptimizationRecommendations(recentData);
                
                return new PerformanceAnalytics
                {
                    PerformanceTrend = trend,
                    OptimalBatchSize = CalculateOptimalBatchSizeFromData(recentData),
                    RecommendedOptimizations = recommendations
                };
            }
        }
        
        /// <summary>
        /// Get performance statistics for monitoring and optimization.
        /// </summary>
        public GeneticsPerformanceStats GetPerformanceStats()
        {
            lock (_historyLock)
            {
                if (_performanceHistory.Count == 0)
                {
                    return new GeneticsPerformanceStats
                    {
                        TotalCalculations = 0,
                        AverageCalculationTimeMs = 0.0,
                        CacheHitRatio = 0.0,
                        BatchCalculations = 0,
                        AverageBatchTimeMs = 0.0
                    };
                }
                
                var latestData = _performanceHistory.Last();
                return latestData.GeneticStats;
            }
        }
        
        private List<string> GenerateOptimizationRecommendations(PerformanceDataPoint[] recentData)
        {
            var recommendations = new List<string>();
            
            double avgUpdateTime = recentData.Average(d => d.UpdateTimeMs);
            double avgCacheHitRatio = recentData.Average(d => d.GeneticStats.CacheHitRatio);
            
            if (avgUpdateTime > 20.0f)
            {
                recommendations.Add("Consider reducing batch size to improve frame rate");
            }
            
            if (avgCacheHitRatio < 0.5f)
            {
                recommendations.Add("Cache hit ratio is low - consider increasing cache size or TTL");
            }
            
            if (recentData.Any(d => d.PlantCount > 100))
            {
                recommendations.Add("Large plant counts detected - consider enabling GPU acceleration");
            }
            
            return recommendations;
        }
        
        private int CalculateOptimalBatchSizeFromData(PerformanceDataPoint[] data)
        {
            // Find the batch size that gives the best performance per plant
            var batchPerformance = data
                .GroupBy(d => (d.PlantCount / 10) * 10) // Group by batches of 10
                .Select(g => new
                {
                    BatchSize = g.Key,
                    AvgTimePerPlant = g.Average(d => d.UpdateTimeMs / d.PlantCount)
                })
                .OrderBy(x => x.AvgTimePerPlant)
                .FirstOrDefault();
            
            return batchPerformance?.BatchSize ?? 20;
        }
    }
    
    /// <summary>
    /// Performance data aggregated from recent monitoring.
    /// </summary>
    [Serializable]
    public struct PerformanceData
    {
        public float AverageUpdateTimeMs;
        public int AveragePlantCount;
        public float CacheHitRatio;
        public int TotalSamples;
        
        public override string ToString()
        {
            return $"AvgTime: {AverageUpdateTimeMs:F2}ms, AvgPlants: {AveragePlantCount}, " +
                   $"CacheHit: {CacheHitRatio:P1}, Samples: {TotalSamples}";
        }
    }
    
    /// <summary>
    /// Performance analytics for optimization recommendations.
    /// </summary>
    [Serializable]
    public struct PerformanceAnalytics
    {
        public PerformanceTrend PerformanceTrend;
        public int OptimalBatchSize;
        public List<string> RecommendedOptimizations;
    }
    
    /// <summary>
    /// Performance trend enumeration.
    /// </summary>
    public enum PerformanceTrend
    {
        Improving,
        Stable,
        Declining
    }
    
    /// <summary>
    /// Mutation type enumeration for genetic calculations.
    /// </summary>
    public enum MutationType
    {
        PointMutation,
        Insertion,
        Deletion,
        Duplication,
        Inversion,
        Translocation,
        RegulatoryMutation,
        EpigeneticModification,
        Regulatory  // Added for BreedingSimulator compatibility
    }
}