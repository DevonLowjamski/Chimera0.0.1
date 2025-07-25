using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Generic object pool implementation for frequently created/destroyed objects
    /// Provides type-safe pooling with configurable lifecycle management
    /// </summary>
    public class GenericObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly int _maxPoolSize;
        private readonly Func<T> _createFunction;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        
        // Statistics tracking
        private int _totalCreated = 0;
        private int _totalGets = 0;
        private int _totalReturns = 0;
        private int _poolHits = 0;
        
        public int PoolSize => _pool.Count;
        public int TotalCreated => _totalCreated;
        public int TotalGets => _totalGets;
        public int TotalReturns => _totalReturns;
        public float HitRate => _totalGets > 0 ? (float)_poolHits / _totalGets : 0f;
        
        /// <summary>
        /// Creates a new object pool with default new() constructor
        /// </summary>
        public GenericObjectPool(int maxPoolSize = 100, Action<T> resetAction = null, Action<T> destroyAction = null)
            : this(() => new T(), maxPoolSize, resetAction, destroyAction)
        {
        }
        
        /// <summary>
        /// Creates a new object pool with custom creation function
        /// </summary>
        public GenericObjectPool(Func<T> createFunction, int maxPoolSize = 100, Action<T> resetAction = null, Action<T> destroyAction = null)
        {
            _createFunction = createFunction ?? throw new ArgumentNullException(nameof(createFunction));
            _maxPoolSize = maxPoolSize;
            _resetAction = resetAction;
            _destroyAction = destroyAction;
        }
        
        /// <summary>
        /// Get an object from the pool or create a new one
        /// </summary>
        public T Get()
        {
            _totalGets++;
            
            if (_pool.Count > 0)
            {
                _poolHits++;
                return _pool.Pop();
            }
            
            // Create new object if pool is empty
            _totalCreated++;
            return _createFunction();
        }
        
        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            
            _totalReturns++;
            
            // Reset object state if reset action provided
            try
            {
                _resetAction?.Invoke(obj);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GenericObjectPool<{typeof(T).Name}>] Error resetting object: {ex.Message}");
                // Don't pool object if reset failed
                _destroyAction?.Invoke(obj);
                return;
            }
            
            // Add to pool if not at capacity
            if (_pool.Count < _maxPoolSize)
            {
                _pool.Push(obj);
            }
            else
            {
                // Pool is full, destroy the object
                _destroyAction?.Invoke(obj);
            }
        }
        
        /// <summary>
        /// Pre-warm the pool with the specified number of objects
        /// </summary>
        public void Prewarm(int count)
        {
            count = Mathf.Min(count, _maxPoolSize);
            
            for (int i = 0; i < count; i++)
            {
                if (_pool.Count >= _maxPoolSize) break;
                
                var obj = _createFunction();
                _totalCreated++;
                _pool.Push(obj);
            }
            
            Debug.Log($"[GenericObjectPool<{typeof(T).Name}>] Pre-warmed pool with {count} objects");
        }
        
        /// <summary>
        /// Clear all objects from the pool
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                _destroyAction?.Invoke(obj);
            }
            
            Debug.Log($"[GenericObjectPool<{typeof(T).Name}>] Pool cleared");
        }
        
        /// <summary>
        /// Get comprehensive pool statistics
        /// </summary>
        public ObjectPoolStats GetStats()
        {
            return new ObjectPoolStats
            {
                TypeName = typeof(T).Name,
                CurrentPoolSize = _pool.Count,
                MaxPoolSize = _maxPoolSize,
                TotalCreated = _totalCreated,
                TotalGets = _totalGets,
                TotalReturns = _totalReturns,
                HitRate = HitRate,
                EfficiencyRating = _totalCreated > 0 ? (float)_poolHits / _totalCreated : 0f
            };
        }
    }
    
    /// <summary>
    /// Statistics for object pool performance
    /// </summary>
    public class ObjectPoolStats
    {
        public string TypeName { get; set; }
        public int CurrentPoolSize { get; set; }
        public int MaxPoolSize { get; set; }
        public int TotalCreated { get; set; }
        public int TotalGets { get; set; }
        public int TotalReturns { get; set; }
        public float HitRate { get; set; }
        public float EfficiencyRating { get; set; }
        
        public override string ToString()
        {
            return $"{TypeName} Pool: {CurrentPoolSize}/{MaxPoolSize} pooled, " +
                   $"{HitRate:P1} hit rate, {EfficiencyRating:P1} efficiency, " +
                   $"{TotalCreated} created, {TotalGets} gets, {TotalReturns} returns";
        }
    }
}