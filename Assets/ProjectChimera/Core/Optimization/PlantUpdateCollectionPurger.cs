using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.SpeedTree;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Specialized purge optimization for plant update collections
    /// Manages high-frequency plant calculation data structures to reduce allocation pressure
    /// </summary>
    public class PlantUpdateCollectionPurger : MonoBehaviour, IPoolable
    {
        [Header("Collection Pool Configuration")]
        [SerializeField] private int _maxPlantListSize = 500;
        [SerializeField] private int _maxGenotypeListSize = 200;
        [SerializeField] private int _maxConditionsListSize = 100;
        [SerializeField] private int _poolWarmupSize = 10;

        // Collection pools for plant update operations
        private readonly Stack<List<SpeedTreePlantInstance>> _plantInstanceLists = new Stack<List<SpeedTreePlantInstance>>();
        private readonly Stack<List<(CannabisGenotype, EnvironmentalConditions)>> _genotypeTuples = new Stack<List<(CannabisGenotype, EnvironmentalConditions)>>();
        private readonly Stack<List<EnvironmentalConditions>> _environmentalConditionsLists = new Stack<List<EnvironmentalConditions>>();
        private readonly Stack<Dictionary<string, object>> _calculationCaches = new Stack<Dictionary<string, object>>();
        private readonly Stack<List<Vector3>> _positionLists = new Stack<List<Vector3>>();

        // Usage tracking
        private int _plantListGets = 0;
        private int _plantListReturns = 0;
        private int _genotypeListGets = 0;
        private int _genotypeListReturns = 0;

        // Singleton instance
        private static PlantUpdateCollectionPurger _instance;
        public static PlantUpdateCollectionPurger Instance => _instance;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializePools();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Pool Management

        private void InitializePools()
        {
            // Pre-warm pools with commonly used collection sizes
            for (int i = 0; i < _poolWarmupSize; i++)
            {
                _plantInstanceLists.Push(new List<SpeedTreePlantInstance>());
                _genotypeTuples.Push(new List<(CannabisGenotype, EnvironmentalConditions)>());
                _environmentalConditionsLists.Push(new List<EnvironmentalConditions>());
                _calculationCaches.Push(new Dictionary<string, object>());
                _positionLists.Push(new List<Vector3>());
            }

            Debug.Log($"[PlantUpdateCollectionPurger] Initialized pools with {_poolWarmupSize} pre-warmed collections");
        }

        /// <summary>
        /// Get a pooled list for plant instances
        /// </summary>
        public List<SpeedTreePlantInstance> GetPlantInstanceList()
        {
            _plantListGets++;
            
            if (_plantInstanceLists.Count > 0)
            {
                var list = _plantInstanceLists.Pop();
                list.Clear(); // Ensure clean state
                return list;
            }

            // Create new list if pool is empty
            return new List<SpeedTreePlantInstance>();
        }

        /// <summary>
        /// Return a plant instance list to the pool
        /// </summary>
        public void ReturnPlantInstanceList(List<SpeedTreePlantInstance> list)
        {
            if (list == null) return;

            _plantListReturns++;
            
            // Only pool if not too large (prevent memory bloat)
            if (list.Capacity <= _maxPlantListSize)
            {
                list.Clear();
                _plantInstanceLists.Push(list);
            }
            // Large lists are allowed to be garbage collected
        }

        /// <summary>
        /// Get a pooled list for genotype/environment tuples
        /// </summary>
        public List<(CannabisGenotype, EnvironmentalConditions)> GetGenotypeTupleList()
        {
            _genotypeListGets++;
            
            if (_genotypeTuples.Count > 0)
            {
                var list = _genotypeTuples.Pop();
                list.Clear();
                return list;
            }

            return new List<(CannabisGenotype, EnvironmentalConditions)>();
        }

        /// <summary>
        /// Return a genotype tuple list to the pool
        /// </summary>
        public void ReturnGenotypeTupleList(List<(CannabisGenotype, EnvironmentalConditions)> list)
        {
            if (list == null) return;

            _genotypeListReturns++;
            
            if (list.Capacity <= _maxGenotypeListSize)
            {
                list.Clear();
                _genotypeTuples.Push(list);
            }
        }

        /// <summary>
        /// Get a pooled environmental conditions list
        /// </summary>
        public List<EnvironmentalConditions> GetEnvironmentalConditionsList()
        {
            if (_environmentalConditionsLists.Count > 0)
            {
                var list = _environmentalConditionsLists.Pop();
                list.Clear();
                return list;
            }

            return new List<EnvironmentalConditions>();
        }

        /// <summary>
        /// Return an environmental conditions list to the pool
        /// </summary>
        public void ReturnEnvironmentalConditionsList(List<EnvironmentalConditions> list)
        {
            if (list == null) return;

            if (list.Capacity <= _maxConditionsListSize)
            {
                list.Clear();
                _environmentalConditionsLists.Push(list);
            }
        }

        /// <summary>
        /// Get a pooled calculation cache dictionary
        /// </summary>
        public Dictionary<string, object> GetCalculationCache()
        {
            if (_calculationCaches.Count > 0)
            {
                var cache = _calculationCaches.Pop();
                cache.Clear();
                return cache;
            }

            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Return a calculation cache to the pool
        /// </summary>
        public void ReturnCalculationCache(Dictionary<string, object> cache)
        {
            if (cache == null) return;

            cache.Clear();
            _calculationCaches.Push(cache);
        }

        /// <summary>
        /// Get a pooled Vector3 position list
        /// </summary>
        public List<Vector3> GetPositionList()
        {
            if (_positionLists.Count > 0)
            {
                var list = _positionLists.Pop();
                list.Clear();
                return list;
            }

            return new List<Vector3>();
        }

        /// <summary>
        /// Return a position list to the pool
        /// </summary>
        public void ReturnPositionList(List<Vector3> list)
        {
            if (list == null) return;

            list.Clear();
            _positionLists.Push(list);
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Optimized batch operation for plant updates using pooled collections
        /// </summary>
        public void ExecuteOptimizedPlantBatch(IEnumerable<SpeedTreePlantInstance> plants, System.Action<List<SpeedTreePlantInstance>, Dictionary<string, object>> batchProcessor)
        {
            var plantList = GetPlantInstanceList();
            var calculationCache = GetCalculationCache();

            try
            {
                // Populate batch
                foreach (var plant in plants)
                {
                    plantList.Add(plant);
                }

                // Execute batch operation
                batchProcessor(plantList, calculationCache);
            }
            finally
            {
                // Always return collections to pool
                ReturnPlantInstanceList(plantList);
                ReturnCalculationCache(calculationCache);
            }
        }

        /// <summary>
        /// Optimized genetic batch operation using pooled collections
        /// </summary>
        public void ExecuteOptimizedGeneticBatch(IEnumerable<(CannabisGenotype, EnvironmentalConditions)> geneticData, 
            System.Action<List<(CannabisGenotype, EnvironmentalConditions)>> batchProcessor)
        {
            var geneticList = GetGenotypeTupleList();

            try
            {
                // Populate batch
                foreach (var data in geneticData)
                {
                    geneticList.Add(data);
                }

                // Execute batch operation
                batchProcessor(geneticList);
            }
            finally
            {
                // Always return to pool
                ReturnGenotypeTupleList(geneticList);
            }
        }

        #endregion

        #region Pool Statistics and Monitoring

        /// <summary>
        /// Get current pool usage statistics
        /// </summary>
        public PlantCollectionPoolStats GetPoolStats()
        {
            return new PlantCollectionPoolStats
            {
                PlantListPoolSize = _plantInstanceLists.Count,
                GenotypeListPoolSize = _genotypeTuples.Count,
                ConditionsListPoolSize = _environmentalConditionsLists.Count,
                CalculationCachePoolSize = _calculationCaches.Count,
                PositionListPoolSize = _positionLists.Count,
                PlantListGets = _plantListGets,
                PlantListReturns = _plantListReturns,
                GenotypeListGets = _genotypeListGets,
                GenotypeListReturns = _genotypeListReturns,
                PlantListReuseRate = _plantListGets > 0 ? (float)_plantListReturns / _plantListGets : 0f,
                GenotypeListReuseRate = _genotypeListGets > 0 ? (float)_genotypeListReturns / _genotypeListGets : 0f
            };
        }

        /// <summary>
        /// Force cleanup of all pools
        /// </summary>
        public void PurgeAllPools()
        {
            _plantInstanceLists.Clear();
            _genotypeTuples.Clear();
            _environmentalConditionsLists.Clear();
            _calculationCaches.Clear();
            _positionLists.Clear();

            Debug.Log("[PlantUpdateCollectionPurger] All pools purged");
        }

        /// <summary>
        /// Trim pools to optimal size based on usage patterns
        /// </summary>
        public void OptimizePools()
        {
            // Keep pools at reasonable sizes based on usage
            TrimPool(_plantInstanceLists, _poolWarmupSize * 2);
            TrimPool(_genotypeTuples, _poolWarmupSize * 2);
            TrimPool(_environmentalConditionsLists, _poolWarmupSize);
            TrimPool(_calculationCaches, _poolWarmupSize);
            TrimPool(_positionLists, _poolWarmupSize);

            Debug.Log("[PlantUpdateCollectionPurger] Pools optimized for memory usage");
        }

        private void TrimPool<T>(Stack<T> pool, int targetSize)
        {
            while (pool.Count > targetSize)
            {
                pool.Pop();
            }
        }

        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Initialize the PlantUpdateCollectionPurger manually (for testing)
        /// </summary>
        public void Initialize()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            InitializePools();
        }

        #endregion

        #region IPoolable Implementation

        public void Reset()
        {
            // Reset usage statistics
            _plantListGets = 0;
            _plantListReturns = 0;
            _genotypeListGets = 0;
            _genotypeListReturns = 0;
        }

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// Statistics for plant collection pools
    /// </summary>
    public class PlantCollectionPoolStats
    {
        public int PlantListPoolSize { get; set; }
        public int GenotypeListPoolSize { get; set; }
        public int ConditionsListPoolSize { get; set; }
        public int CalculationCachePoolSize { get; set; }
        public int PositionListPoolSize { get; set; }
        public int PlantListGets { get; set; }
        public int PlantListReturns { get; set; }
        public int GenotypeListGets { get; set; }
        public int GenotypeListReturns { get; set; }
        public float PlantListReuseRate { get; set; }
        public float GenotypeListReuseRate { get; set; }

        public override string ToString()
        {
            return $"Plant Collections Pool Stats:\n" +
                   $"  Plant Lists: {PlantListPoolSize} pooled, {PlantListReuseRate:P1} reuse rate\n" +
                   $"  Genotype Lists: {GenotypeListPoolSize} pooled, {GenotypeListReuseRate:P1} reuse rate\n" +
                   $"  Conditions Lists: {ConditionsListPoolSize} pooled\n" +
                   $"  Calculation Caches: {CalculationCachePoolSize} pooled\n" +
                   $"  Position Lists: {PositionListPoolSize} pooled";
        }
    }


    #endregion
}