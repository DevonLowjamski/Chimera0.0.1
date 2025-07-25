using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Specialized object pool for Unity GameObjects and MonoBehaviours
    /// Handles Unity-specific lifecycle management and pooling patterns
    /// </summary>
    public class UnityObjectPool<T> where T : Component
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly int _maxPoolSize;
        private readonly GameObject _prefab;
        private readonly Transform _poolParent;
        private readonly bool _setActiveOnGet;
        private readonly bool _setInactiveOnReturn;
        
        // Statistics tracking
        private int _totalInstantiated = 0;
        private int _totalGets = 0;
        private int _totalReturns = 0;
        private int _poolHits = 0;
        
        public int PoolSize => _pool.Count;
        public int TotalInstantiated => _totalInstantiated;
        public int TotalGets => _totalGets;
        public int TotalReturns => _totalReturns;
        public float HitRate => _totalGets > 0 ? (float)_poolHits / _totalGets : 0f;
        
        /// <summary>
        /// Creates a Unity object pool for the specified prefab
        /// </summary>
        public UnityObjectPool(GameObject prefab, int maxPoolSize = 50, Transform poolParent = null, 
            bool setActiveOnGet = true, bool setInactiveOnReturn = true)
        {
            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _maxPoolSize = maxPoolSize;
            _poolParent = poolParent;
            _setActiveOnGet = setActiveOnGet;
            _setInactiveOnReturn = setInactiveOnReturn;
            
            // Verify prefab has required component
            if (_prefab.GetComponent<T>() == null)
            {
                throw new ArgumentException($"Prefab does not contain component of type {typeof(T).Name}");
            }
        }
        
        /// <summary>
        /// Get an object from the pool or instantiate a new one
        /// </summary>
        public T Get()
        {
            _totalGets++;
            
            if (_pool.Count > 0)
            {
                _poolHits++;
                var pooledObject = _pool.Pop();
                
                if (pooledObject != null)
                {
                    if (_setActiveOnGet && pooledObject.gameObject != null)
                    {
                        pooledObject.gameObject.SetActive(true);
                    }
                    return pooledObject;
                }
            }
            
            // Create new object if pool is empty or object was destroyed
            _totalInstantiated++;
            var newGameObject = UnityEngine.Object.Instantiate(_prefab, _poolParent);
            var component = newGameObject.GetComponent<T>();
            
            if (_setActiveOnGet)
            {
                newGameObject.SetActive(true);
            }
            
            return component;
        }
        
        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T component)
        {
            if (component == null || component.gameObject == null) return;
            
            _totalReturns++;
            
            // Reset component if it implements IPoolable
            if (component is IPoolable poolable)
            {
                try
                {
                    poolable.Reset();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityObjectPool<{typeof(T).Name}>] Error resetting poolable object: {ex.Message}");
                    // Destroy object if reset failed
                    if (component.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(component.gameObject);
                    }
                    return;
                }
            }
            
            // Set inactive and reparent if returning to pool
            if (_pool.Count < _maxPoolSize)
            {
                if (_setInactiveOnReturn)
                {
                    component.gameObject.SetActive(false);
                }
                
                if (_poolParent != null)
                {
                    component.transform.SetParent(_poolParent);
                }
                
                _pool.Push(component);
            }
            else
            {
                // Pool is full, destroy the object
                if (component.gameObject != null)
                {
                    UnityEngine.Object.Destroy(component.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Return a GameObject to the pool (convenience method)
        /// </summary>
        public void Return(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                Return(component);
            }
            else
            {
                Debug.LogWarning($"[UnityObjectPool<{typeof(T).Name}>] GameObject does not contain required component");
                UnityEngine.Object.Destroy(gameObject);
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
                
                var gameObject = UnityEngine.Object.Instantiate(_prefab, _poolParent);
                var component = gameObject.GetComponent<T>();
                
                _totalInstantiated++;
                
                if (_setInactiveOnReturn)
                {
                    gameObject.SetActive(false);
                }
                
                _pool.Push(component);
            }
            
            Debug.Log($"[UnityObjectPool<{typeof(T).Name}>] Pre-warmed pool with {count} objects");
        }
        
        /// <summary>
        /// Clear all objects from the pool
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                if (obj != null && obj.gameObject != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            Debug.Log($"[UnityObjectPool<{typeof(T).Name}>] Pool cleared");
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
                TotalCreated = _totalInstantiated,
                TotalGets = _totalGets,
                TotalReturns = _totalReturns,
                HitRate = HitRate,
                EfficiencyRating = _totalInstantiated > 0 ? (float)_poolHits / _totalInstantiated : 0f
            };
        }
    }
}