using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.SpeedTree;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Central manager for all object pooling in Project Chimera
    /// Coordinates generic object pools, Unity object pools, and specialized pools
    /// </summary>
    public class PooledObjectManager : ChimeraManager
    {
        [Header("Pool Configuration")]
        [SerializeField] private bool _enableGenericPooling = true;
        [SerializeField] private bool _enableUnityObjectPooling = true;
        [SerializeField] private bool _enableSpecializedPooling = true;
        [SerializeField] private int _defaultPoolSize = 50;
        [SerializeField] private int _largePoolSize = 200;
        
        [Header("Performance Monitoring")]
        [SerializeField] private float _statsUpdateInterval = 30f;
        [SerializeField] private bool _logPoolStats = false;
        [SerializeField] private int _statsHistorySize = 20;
        
        [Header("Pool Parent Objects")]
        [SerializeField] private Transform _uiElementPoolParent;
        [SerializeField] private Transform _effectPoolParent;
        [SerializeField] private Transform _particlePoolParent;
        
        // Generic object pools
        private readonly Dictionary<Type, object> _genericPools = new Dictionary<Type, object>();
        
        // Unity object pools  
        private readonly Dictionary<string, object> _unityPools = new Dictionary<string, object>();
        
        // Specialized pools for common types
        private GenericObjectPool<List<SpeedTreePlantInstance>> _plantListPool;
        private GenericObjectPool<Dictionary<string, object>> _dataDictionaryPool;
        private GenericObjectPool<List<EnvironmentalConditions>> _environmentalConditionsPool;
        private GenericObjectPool<List<Vector3>> _vector3ListPool;
        private GenericObjectPool<List<float>> _floatListPool;
        private GenericObjectPool<List<string>> _stringListPool;
        
        // Performance monitoring
        private readonly Queue<PoolManagerSnapshot> _performanceHistory = new Queue<PoolManagerSnapshot>();
        private DateTime _initTime;
        
        public static PooledObjectManager Instance { get; private set; }
        
        #region Unity Lifecycle
        
        protected override void OnManagerInitialize()
        {
            if (Instance == null)
            {
                Instance = this;
                _initTime = DateTime.Now;
                
                InitializePoolParents();
                InitializeSpecializedPools();
                
                if (_statsUpdateInterval > 0)
                {
                    InvokeRepeating(nameof(UpdatePerformanceStats), _statsUpdateInterval, _statsUpdateInterval);
                }
                
                Debug.Log("[PooledObjectManager] Object pooling system initialized");
            }
            else
            {
                Debug.LogWarning("[PooledObjectManager] Multiple instances detected, destroying duplicate");
                Destroy(gameObject);
            }
        }
        
        protected override void OnManagerShutdown()
        {
            if (Instance == this)
            {
                ClearAllPools();
                CancelInvoke();
                Instance = null;
                
                Debug.Log("[PooledObjectManager] Object pooling system shut down");
            }
        }
        
        #endregion
        
        #region Pool Parent Management
        
        private void InitializePoolParents()
        {
            // Create pool parent objects if not assigned
            if (_uiElementPoolParent == null)
            {
                var uiPoolGO = new GameObject("UI Element Pool");
                uiPoolGO.transform.SetParent(transform);
                _uiElementPoolParent = uiPoolGO.transform;
            }
            
            if (_effectPoolParent == null)
            {
                var effectPoolGO = new GameObject("Effect Pool");
                effectPoolGO.transform.SetParent(transform);
                _effectPoolParent = effectPoolGO.transform;
            }
            
            if (_particlePoolParent == null)
            {
                var particlePoolGO = new GameObject("Particle Pool");
                particlePoolGO.transform.SetParent(transform);
                _particlePoolParent = particlePoolGO.transform;
            }
        }
        
        #endregion
        
        #region Specialized Pool Initialization
        
        private void InitializeSpecializedPools()
        {
            if (!_enableSpecializedPooling) return;
            
            // Plant data pools
            _plantListPool = new GenericObjectPool<List<SpeedTreePlantInstance>>(
                _largePoolSize,
                list => list?.Clear(),
                null
            );
            
            // Dictionary pools for caching and temporary data
            _dataDictionaryPool = new GenericObjectPool<Dictionary<string, object>>(
                _defaultPoolSize,
                dict => dict?.Clear(),
                null
            );
            
            // Environmental data pools
            _environmentalConditionsPool = new GenericObjectPool<List<EnvironmentalConditions>>(
                _defaultPoolSize,
                list => list?.Clear(),
                null
            );
            
            // Common collection pools
            _vector3ListPool = new GenericObjectPool<List<Vector3>>(
                _defaultPoolSize,
                list => list?.Clear(),
                null
            );
            
            _floatListPool = new GenericObjectPool<List<float>>(
                _defaultPoolSize,
                list => list?.Clear(),
                null
            );
            
            _stringListPool = new GenericObjectPool<List<string>>(
                _defaultPoolSize,
                list => list?.Clear(),
                null
            );
            
            // Pre-warm critical pools
            _plantListPool.Prewarm(10);
            _dataDictionaryPool.Prewarm(15);
            _environmentalConditionsPool.Prewarm(8);
            _vector3ListPool.Prewarm(12);
            
            Debug.Log("[PooledObjectManager] Specialized pools initialized and pre-warmed");
        }
        
        #endregion
        
        #region Generic Object Pool API
        
        /// <summary>
        /// Get or create a generic object pool for the specified type
        /// </summary>
        public GenericObjectPool<T> GetGenericPool<T>() where T : class, new()
        {
            if (!_enableGenericPooling) return null;
            
            var type = typeof(T);
            if (_genericPools.TryGetValue(type, out var poolObj))
            {
                return (GenericObjectPool<T>)poolObj;
            }
            
            // Create new pool
            var pool = new GenericObjectPool<T>(_defaultPoolSize);
            _genericPools[type] = pool;
            
            Debug.Log($"[PooledObjectManager] Created new generic pool for {type.Name}");
            return pool;
        }
        
        /// <summary>
        /// Get an object from the generic pool
        /// </summary>
        public T GetPooledObject<T>() where T : class, new()
        {
            var pool = GetGenericPool<T>();
            return pool?.Get() ?? new T();
        }
        
        /// <summary>
        /// Return an object to the generic pool
        /// </summary>
        public void ReturnPooledObject<T>(T obj) where T : class, new()
        {
            if (obj == null) return;
            
            var type = typeof(T);
            if (_genericPools.TryGetValue(type, out var poolObj))
            {
                var pool = (GenericObjectPool<T>)poolObj;
                pool.Return(obj);
            }
        }
        
        #endregion
        
        #region Unity Object Pool API
        
        /// <summary>
        /// Create a Unity object pool for the specified prefab
        /// </summary>
        public UnityObjectPool<T> CreateUnityPool<T>(GameObject prefab, int maxPoolSize = -1, Transform poolParent = null) where T : Component
        {
            if (!_enableUnityObjectPooling) return null;
            
            var poolName = $"{typeof(T).Name}_{prefab.name}";
            maxPoolSize = maxPoolSize > 0 ? maxPoolSize : _defaultPoolSize;
            poolParent = poolParent ?? GetDefaultPoolParent<T>();
            
            var pool = new UnityObjectPool<T>(prefab, maxPoolSize, poolParent);
            _unityPools[poolName] = pool;
            
            Debug.Log($"[PooledObjectManager] Created Unity pool for {poolName}");
            return pool;
        }
        
        /// <summary>
        /// Get a Unity object pool by name
        /// </summary>
        public UnityObjectPool<T> GetUnityPool<T>(string poolName) where T : Component
        {
            if (_unityPools.TryGetValue(poolName, out var poolObj))
            {
                return (UnityObjectPool<T>)poolObj;
            }
            return null;
        }
        
        private Transform GetDefaultPoolParent<T>() where T : Component
        {
            // Determine appropriate parent based on component type
            if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                if (typeof(T).Name.Contains("UI") || typeof(T).Name.Contains("Canvas"))
                    return _uiElementPoolParent;
                if (typeof(T).Name.Contains("Effect"))
                    return _effectPoolParent;
                if (typeof(T).Name.Contains("Particle"))
                    return _particlePoolParent;
            }
            
            return transform; // Default to manager transform
        }
        
        #endregion
        
        #region Specialized Pool API
        
        /// <summary>
        /// Get a pooled plant instance list
        /// </summary>
        public List<SpeedTreePlantInstance> GetPlantList()
        {
            return _plantListPool?.Get() ?? new List<SpeedTreePlantInstance>();
        }
        
        /// <summary>
        /// Return a plant instance list to the pool
        /// </summary>
        public void ReturnPlantList(List<SpeedTreePlantInstance> list)
        {
            _plantListPool?.Return(list);
        }
        
        /// <summary>
        /// Get a pooled data dictionary
        /// </summary>
        public Dictionary<string, object> GetDataDictionary()
        {
            return _dataDictionaryPool?.Get() ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Return a data dictionary to the pool
        /// </summary>
        public void ReturnDataDictionary(Dictionary<string, object> dict)
        {
            _dataDictionaryPool?.Return(dict);
        }
        
        /// <summary>
        /// Get a pooled environmental conditions list
        /// </summary>
        public List<EnvironmentalConditions> GetEnvironmentalConditionsList()
        {
            return _environmentalConditionsPool?.Get() ?? new List<EnvironmentalConditions>();
        }
        
        /// <summary>
        /// Return an environmental conditions list to the pool
        /// </summary>
        public void ReturnEnvironmentalConditionsList(List<EnvironmentalConditions> list)
        {
            _environmentalConditionsPool?.Return(list);
        }
        
        /// <summary>
        /// Get a pooled Vector3 list
        /// </summary>
        public List<Vector3> GetVector3List()
        {
            return _vector3ListPool?.Get() ?? new List<Vector3>();
        }
        
        /// <summary>
        /// Return a Vector3 list to the pool
        /// </summary>
        public void ReturnVector3List(List<Vector3> list)
        {
            _vector3ListPool?.Return(list);
        }
        
        /// <summary>
        /// Get a pooled float list
        /// </summary>
        public List<float> GetFloatList()
        {
            return _floatListPool?.Get() ?? new List<float>();
        }
        
        /// <summary>
        /// Return a float list to the pool
        /// </summary>
        public void ReturnFloatList(List<float> list)
        {
            _floatListPool?.Return(list);
        }
        
        /// <summary>
        /// Get a pooled string list
        /// </summary>
        public List<string> GetStringList()
        {
            return _stringListPool?.Get() ?? new List<string>();
        }
        
        /// <summary>
        /// Return a string list to the pool
        /// </summary>
        public void ReturnStringList(List<string> list)
        {
            _stringListPool?.Return(list);
        }
        
        #endregion
        
        #region Batch Operations
        
        /// <summary>
        /// Execute a batch operation using pooled plant list and data dictionary
        /// </summary>
        public void ExecuteBatchOperation(IEnumerable<SpeedTreePlantInstance> plants, 
            Action<List<SpeedTreePlantInstance>, Dictionary<string, object>> operation)
        {
            var plantList = GetPlantList();
            var dataDict = GetDataDictionary();
            
            try
            {
                // Populate plant list
                foreach (var plant in plants)
                {
                    plantList.Add(plant);
                }
                
                // Execute operation
                operation(plantList, dataDict);
            }
            finally
            {
                // Always return to pools
                ReturnPlantList(plantList);
                ReturnDataDictionary(dataDict);
            }
        }
        
        /// <summary>
        /// Execute a Vector3 calculation using pooled collections
        /// </summary>
        public void ExecuteVector3Calculation(IEnumerable<Vector3> positions, 
            Action<List<Vector3>, List<float>> calculation)
        {
            var positionList = GetVector3List();
            var resultList = GetFloatList();
            
            try
            {
                // Populate position list
                foreach (var pos in positions)
                {
                    positionList.Add(pos);
                }
                
                // Execute calculation
                calculation(positionList, resultList);
            }
            finally
            {
                // Always return to pools
                ReturnVector3List(positionList);
                ReturnFloatList(resultList);
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceStats()
        {
            var snapshot = new PoolManagerSnapshot
            {
                Timestamp = DateTime.Now,
                GenericPoolCount = _genericPools.Count,
                UnityPoolCount = _unityPools.Count,
                TotalMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f),
                PlantListStats = _plantListPool?.GetStats(),
                DataDictStats = _dataDictionaryPool?.GetStats(),
                EnvironmentalStats = _environmentalConditionsPool?.GetStats()
            };
            
            _performanceHistory.Enqueue(snapshot);
            
            // Maintain history size
            while (_performanceHistory.Count > _statsHistorySize)
            {
                _performanceHistory.Dequeue();
            }
            
            if (_logPoolStats)
            {
                LogPerformanceStats(snapshot);
            }
        }
        
        private void LogPerformanceStats(PoolManagerSnapshot snapshot)
        {
            Debug.Log($"[PooledObjectManager] Performance Stats - " +
                     $"Generic Pools: {snapshot.GenericPoolCount}, " +
                     $"Unity Pools: {snapshot.UnityPoolCount}, " +
                     $"Memory: {snapshot.TotalMemoryMB:F1}MB");
            
            if (snapshot.PlantListStats != null)
            {
                Debug.Log($"[PooledObjectManager] Plant Lists: {snapshot.PlantListStats.HitRate:P1} hit rate, " +
                         $"{snapshot.PlantListStats.CurrentPoolSize} pooled");
            }
        }
        
        #endregion
        
        #region Pool Management
        
        /// <summary>
        /// Clear all pools and reset statistics
        /// </summary>
        public void ClearAllPools()
        {
            // Clear generic pools
            foreach (var pool in _genericPools.Values)
            {
                if (pool is GenericObjectPool<object> genericPool)
                {
                    genericPool.Clear();
                }
            }
            _genericPools.Clear();
            
            // Clear Unity pools
            foreach (var pool in _unityPools.Values)
            {
                // Use reflection to call Clear() method on various pool types
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod?.Invoke(pool, null);
            }
            _unityPools.Clear();
            
            // Clear specialized pools
            _plantListPool?.Clear();
            _dataDictionaryPool?.Clear();
            _environmentalConditionsPool?.Clear();
            _vector3ListPool?.Clear();
            _floatListPool?.Clear();
            _stringListPool?.Clear();
            
            Debug.Log("[PooledObjectManager] All pools cleared");
        }
        
        /// <summary>
        /// Get comprehensive pool statistics
        /// </summary>
        public PoolManagerStats GetPoolManagerStats()
        {
            var stats = new PoolManagerStats
            {
                UptimeSeconds = (float)(DateTime.Now - _initTime).TotalSeconds,
                GenericPoolCount = _genericPools.Count,
                UnityPoolCount = _unityPools.Count,
                PerformanceHistoryCount = _performanceHistory.Count,
                LatestSnapshot = _performanceHistory.Count > 0 ? _performanceHistory.ToArray()[_performanceHistory.Count - 1] : null
            };
            
            return stats;
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    /// <summary>
    /// Performance snapshot for pool manager monitoring
    /// </summary>
    public class PoolManagerSnapshot
    {
        public DateTime Timestamp { get; set; }
        public int GenericPoolCount { get; set; }
        public int UnityPoolCount { get; set; }
        public float TotalMemoryMB { get; set; }
        public ObjectPoolStats PlantListStats { get; set; }
        public ObjectPoolStats DataDictStats { get; set; }
        public ObjectPoolStats EnvironmentalStats { get; set; }
    }
    
    /// <summary>
    /// Comprehensive pool manager statistics
    /// </summary>
    public class PoolManagerStats
    {
        public float UptimeSeconds { get; set; }
        public int GenericPoolCount { get; set; }
        public int UnityPoolCount { get; set; }
        public int PerformanceHistoryCount { get; set; }
        public PoolManagerSnapshot LatestSnapshot { get; set; }
        
        public override string ToString()
        {
            return $"Pool Manager Stats:\\n" +
                   $"  Uptime: {UptimeSeconds:F0}s\\n" +
                   $"  Generic Pools: {GenericPoolCount}\\n" +
                   $"  Unity Pools: {UnityPoolCount}\\n" +
                   $"  Performance History: {PerformanceHistoryCount} snapshots";
        }
    }
    
    #endregion
}