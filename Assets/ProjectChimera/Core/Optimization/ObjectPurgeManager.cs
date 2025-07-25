using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Systems.SpeedTree;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Manages automatic purging and cleanup of frequently created/destroyed objects
    /// Implements intelligent memory management for high-frequency object usage patterns
    /// </summary>
    public class ObjectPurgeManager : ChimeraManager
    {
        // Singleton instance
        private static ObjectPurgeManager _instance;
        public static ObjectPurgeManager Instance => _instance;
        
        [Header("Purge Configuration")]
        [SerializeField] private float _purgeInterval = 30f; // Purge every 30 seconds
        [SerializeField] private int _maxPoolSize = 1000; // Maximum objects to keep pooled
        [SerializeField] private float _objectLifetime = 60f; // Time before object is eligible for purge
        [SerializeField] private bool _enableAutomaticPurging = true;
        [SerializeField] private bool _enableMemoryPressureDetection = true;

        [Header("Memory Thresholds")]
        [SerializeField] private long _memoryPressureThreshold = 512 * 1024 * 1024; // 512MB
        [SerializeField] private float _memoryPressurePurgeMultiplier = 2f;

        // Purge tracking data
        private readonly Dictionary<Type, PurgeableObjectPool> _objectPools = new Dictionary<Type, PurgeableObjectPool>();
        private readonly Dictionary<Type, ObjectUsageStats> _usageStats = new Dictionary<Type, ObjectUsageStats>();
        
        // Coroutine tracking
        private Coroutine _purgeCoroutine;
        private DateTime _lastPurgeTime = DateTime.Now;

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected override void OnManagerInitialize()
        {
            Debug.Log("[ObjectPurgeManager] Initializing object purge optimization system");
            
            // Start automatic purging if enabled
            if (_enableAutomaticPurging)
            {
                _purgeCoroutine = StartCoroutine(AutomaticPurgeCoroutine());
            }

            // Register common object types for purging
            RegisterCommonObjectTypes();
        }

        protected override void OnManagerShutdown()
        {
            Debug.Log("[ObjectPurgeManager] Shutting down purge manager");
            
            if (_purgeCoroutine != null)
            {
                StopCoroutine(_purgeCoroutine);
                _purgeCoroutine = null;
            }

            // Purge all pools on shutdown
            PurgeAllPools();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Register an object type for purge management
        /// </summary>
        public void RegisterObjectType<T>(int maxPoolSize = -1) where T : class, new()
        {
            var type = typeof(T);
            if (!_objectPools.ContainsKey(type))
            {
                var poolSize = maxPoolSize > 0 ? maxPoolSize : _maxPoolSize;
                _objectPools[type] = new PurgeableObjectPool(type, poolSize, _objectLifetime);
                _usageStats[type] = new ObjectUsageStats();
                
                Debug.Log($"[ObjectPurgeManager] Registered {type.Name} for purge management (max pool: {poolSize})");
            }
        }

        /// <summary>
        /// Get or create an object from the managed pool
        /// </summary>
        public T GetPooledObject<T>() where T : class, new()
        {
            var type = typeof(T);
            
            // Register type if not already registered
            if (!_objectPools.ContainsKey(type))
            {
                RegisterObjectType<T>();
            }

            var pool = _objectPools[type];
            var stats = _usageStats[type];
            
            stats.RecordGet();
            
            var obj = pool.GetObject();
            if (obj == null)
            {
                obj = new T();
                stats.RecordCreation();
            }
            else
            {
                stats.RecordReuse();
            }

            return (T)obj;
        }

        /// <summary>
        /// Return an object to the managed pool
        /// </summary>
        public void ReturnPooledObject<T>(T obj) where T : class
        {
            if (obj == null) return;

            var type = typeof(T);
            if (_objectPools.TryGetValue(type, out var pool))
            {
                pool.ReturnObject(obj);
                _usageStats[type].RecordReturn();
            }
        }

        /// <summary>
        /// Force purge of specific object type
        /// </summary>
        public void PurgeObjectType<T>() where T : class
        {
            var type = typeof(T);
            if (_objectPools.TryGetValue(type, out var pool))
            {
                var purgedCount = pool.PurgeExpiredObjects(DateTime.Now);
                Debug.Log($"[ObjectPurgeManager] Purged {purgedCount} expired {type.Name} objects");
            }
        }

        /// <summary>
        /// Get usage statistics for an object type
        /// </summary>
        public ObjectUsageStats GetUsageStats<T>() where T : class
        {
            var type = typeof(T);
            return _usageStats.TryGetValue(type, out var stats) ? stats : new ObjectUsageStats();
        }

        /// <summary>
        /// Force immediate purge of all pools
        /// </summary>
        public void ForcePurgeAll()
        {
            PurgeAllPools();
            _lastPurgeTime = DateTime.Now;
        }

        #endregion

        #region Private Implementation

        private void RegisterCommonObjectTypes()
        {
            // Register frequently used collection types
            RegisterObjectType<List<SpeedTreePlantInstance>>(100);
            RegisterObjectType<List<EnvironmentalConditions>>(50);
            RegisterObjectType<Dictionary<string, object>>(50);
            RegisterObjectType<List<GameObject>>(200);
            
            // Register UI data structures
            RegisterObjectType<List<object>>(100); // Generic UI data lists
            RegisterObjectType<Dictionary<string, string>>(30);
            
            Debug.Log("[ObjectPurgeManager] Registered common object types for optimization");
        }

        private System.Collections.IEnumerator AutomaticPurgeCoroutine()
        {
            while (_enableAutomaticPurging)
            {
                yield return new WaitForSeconds(_purgeInterval);
                
                try
                {
                    PerformAutomaticPurge();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ObjectPurgeManager] Error during automatic purge: {ex.Message}");
                }
            }
        }

        private void PerformAutomaticPurge()
        {
            var currentTime = DateTime.Now;
            var timeSinceLastPurge = (currentTime - _lastPurgeTime).TotalSeconds;
            
            // Check memory pressure if enabled
            var isMemoryPressure = _enableMemoryPressureDetection && IsMemoryUnderPressure();
            var purgeMultiplier = isMemoryPressure ? _memoryPressurePurgeMultiplier : 1f;
            
            int totalPurged = 0;
            foreach (var kvp in _objectPools)
            {
                var type = kvp.Key;
                var pool = kvp.Value;
                
                // Adjust purge aggressiveness based on memory pressure
                var effectiveLifetime = _objectLifetime / purgeMultiplier;
                var purged = pool.PurgeExpiredObjects(currentTime.AddSeconds(-effectiveLifetime));
                totalPurged += purged;
                
                if (purged > 0)
                {
                    Debug.Log($"[ObjectPurgeManager] Purged {purged} {type.Name} objects (memory pressure: {isMemoryPressure})");
                }
            }
            
            if (totalPurged > 0 || timeSinceLastPurge >= _purgeInterval * 2)
            {
                // Force garbage collection after significant purging or if it's been a while
                System.GC.Collect();
                Debug.Log($"[ObjectPurgeManager] Completed purge cycle: {totalPurged} objects purged, GC triggered");
            }
            
            _lastPurgeTime = currentTime;
        }

        private bool IsMemoryUnderPressure()
        {
            try
            {
                var allocatedMemory = System.GC.GetTotalMemory(false);
                return allocatedMemory > _memoryPressureThreshold;
            }
            catch
            {
                return false; // Assume no pressure if we can't detect
            }
        }

        private void PurgeAllPools()
        {
            foreach (var pool in _objectPools.Values)
            {
                pool.Clear();
            }
            _objectPools.Clear();
            _usageStats.Clear();
        }

        #endregion

        #region Public API Methods
        
        /// <summary>
        /// Initialize the ObjectPurgeManager (alias for OnManagerInitialize for testing)
        /// </summary>
        public void Initialize()
        {
            OnManagerInitialize();
        }
        
        /// <summary>
        /// Manually trigger purging of unused objects
        /// </summary>
        public void PurgeUnusedObjects()
        {
            Debug.Log("[ObjectPurgeManager] Manual purge triggered");
            PerformAutomaticPurge();
        }

        #endregion

        #region Statistics and Monitoring

        /// <summary>
        /// Get comprehensive purge statistics
        /// </summary>
        public PurgeManagerStats GetPurgeStats()
        {
            var stats = new PurgeManagerStats
            {
                RegisteredObjectTypes = _objectPools.Count,
                TotalPooledObjects = 0,
                LastPurgeTime = _lastPurgeTime,
                UsageStatsByType = new Dictionary<string, ObjectUsageStats>()
            };

            foreach (var kvp in _objectPools)
            {
                stats.TotalPooledObjects += kvp.Value.ActiveObjectCount;
                stats.UsageStatsByType[kvp.Key.Name] = _usageStats[kvp.Key];
            }

            return stats;
        }

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// Manages a pool of objects with automatic expiration
    /// </summary>
    public class PurgeableObjectPool
    {
        private readonly Type _objectType;
        private readonly int _maxPoolSize;
        private readonly float _objectLifetime;
        private readonly Queue<PooledObjectWrapper> _availableObjects = new Queue<PooledObjectWrapper>();

        public int ActiveObjectCount => _availableObjects.Count;

        public PurgeableObjectPool(Type objectType, int maxPoolSize, float objectLifetime)
        {
            _objectType = objectType;
            _maxPoolSize = maxPoolSize;
            _objectLifetime = objectLifetime;
        }

        public object GetObject()
        {
            while (_availableObjects.Count > 0)
            {
                var wrapper = _availableObjects.Dequeue();
                if (!wrapper.IsExpired(DateTime.Now))
                {
                    return wrapper.Object;
                }
                // Object is expired, don't return it
            }
            return null; // No available objects
        }

        public void ReturnObject(object obj)
        {
            if (obj == null || _availableObjects.Count >= _maxPoolSize) return;

            // Reset object if it supports reset
            if (obj is IPoolable poolable)
            {
                poolable.Reset();
            }

            _availableObjects.Enqueue(new PooledObjectWrapper(obj, DateTime.Now.AddSeconds(_objectLifetime)));
        }

        public int PurgeExpiredObjects(DateTime cutoffTime)
        {
            var originalCount = _availableObjects.Count;
            var tempQueue = new Queue<PooledObjectWrapper>();

            while (_availableObjects.Count > 0)
            {
                var wrapper = _availableObjects.Dequeue();
                if (!wrapper.IsExpired(cutoffTime))
                {
                    tempQueue.Enqueue(wrapper);
                }
            }

            // Restore non-expired objects
            while (tempQueue.Count > 0)
            {
                _availableObjects.Enqueue(tempQueue.Dequeue());
            }

            return originalCount - _availableObjects.Count;
        }

        public void Clear()
        {
            _availableObjects.Clear();
        }
    }

    /// <summary>
    /// Wrapper for pooled objects with expiration tracking
    /// </summary>
    public class PooledObjectWrapper
    {
        public object Object { get; }
        public DateTime ExpirationTime { get; }

        public PooledObjectWrapper(object obj, DateTime expirationTime)
        {
            Object = obj;
            ExpirationTime = expirationTime;
        }

        public bool IsExpired(DateTime currentTime) => currentTime >= ExpirationTime;
    }

    /// <summary>
    /// Tracks usage statistics for object types
    /// </summary>
    public class ObjectUsageStats
    {
        public int GetsRequested { get; private set; }
        public int ObjectsCreated { get; private set; }
        public int ObjectsReused { get; private set; }
        public int ObjectsReturned { get; private set; }
        public DateTime FirstUsage { get; private set; } = DateTime.Now;
        public DateTime LastUsage { get; private set; } = DateTime.Now;

        public float ReuseRate => GetsRequested > 0 ? (float)ObjectsReused / GetsRequested : 0f;
        public float EfficiencyRating => ObjectsCreated > 0 ? (float)ObjectsReused / ObjectsCreated : 0f;

        public void RecordGet() { GetsRequested++; LastUsage = DateTime.Now; }
        public void RecordCreation() { ObjectsCreated++; }
        public void RecordReuse() { ObjectsReused++; }
        public void RecordReturn() { ObjectsReturned++; }
    }

    /// <summary>
    /// Overall purge manager statistics
    /// </summary>
    public class PurgeManagerStats
    {
        public int RegisteredObjectTypes { get; set; }
        public int TotalPooledObjects { get; set; }
        public DateTime LastPurgeTime { get; set; }
        public Dictionary<string, ObjectUsageStats> UsageStatsByType { get; set; }
    }

    /// <summary>
    /// Interface for objects that can be reset when returned to pool
    /// </summary>
    public interface IPoolable
    {
        void Reset();
    }

    #endregion
}