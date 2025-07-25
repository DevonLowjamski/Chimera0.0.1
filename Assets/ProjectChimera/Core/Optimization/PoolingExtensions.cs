using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.SpeedTree;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Extension methods for simplified pooling operations
    /// Provides fluent API for common pooling scenarios
    /// </summary>
    public static class PoolingExtensions
    {
        #region Generic Object Pooling Extensions
        
        /// <summary>
        /// Execute an operation using a pooled object of type T
        /// </summary>
        public static TResult WithPooledObject<T, TResult>(this PooledObjectManager manager, Func<T, TResult> operation) 
            where T : class, new()
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var obj = manager.GetPooledObject<T>();
            try
            {
                return operation(obj);
            }
            finally
            {
                manager.ReturnPooledObject(obj);
            }
        }
        
        /// <summary>
        /// Execute an operation using a pooled object of type T
        /// </summary>
        public static void WithPooledObject<T>(this PooledObjectManager manager, Action<T> operation) 
            where T : class, new()
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var obj = manager.GetPooledObject<T>();
            try
            {
                operation(obj);
            }
            finally
            {
                manager.ReturnPooledObject(obj);
            }
        }
        
        #endregion
        
        #region Collection Pooling Extensions
        
        /// <summary>
        /// Execute an operation using pooled plant list and data dictionary
        /// </summary>
        public static void WithPooledPlantOperation(this PooledObjectManager manager,
            IEnumerable<SpeedTreePlantInstance> plants,
            Action<List<SpeedTreePlantInstance>, Dictionary<string, object>> operation)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var plantList = manager.GetPlantList();
            var dataDict = manager.GetDataDictionary();
            
            try
            {
                // Populate plant list
                foreach (var plant in plants)
                {
                    plantList.Add(plant);
                }
                
                operation(plantList, dataDict);
            }
            finally
            {
                manager.ReturnPlantList(plantList);
                manager.ReturnDataDictionary(dataDict);
            }
        }
        
        /// <summary>
        /// Execute a Vector3 operation using pooled collections
        /// </summary>
        public static void WithPooledVector3Operation(this PooledObjectManager manager,
            IEnumerable<Vector3> vectors,
            Action<List<Vector3>, List<float>> operation)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var vectorList = manager.GetVector3List();
            var floatList = manager.GetFloatList();
            
            try
            {
                // Populate vector list
                foreach (var vector in vectors)
                {
                    vectorList.Add(vector);
                }
                
                operation(vectorList, floatList);
            }
            finally
            {
                manager.ReturnVector3List(vectorList);
                manager.ReturnFloatList(floatList);
            }
        }
        
        /// <summary>
        /// Execute an environmental conditions operation using pooled collections
        /// </summary>
        public static void WithPooledEnvironmentalOperation(this PooledObjectManager manager,
            IEnumerable<EnvironmentalConditions> conditions,
            Action<List<EnvironmentalConditions>, Dictionary<string, object>> operation)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var conditionsList = manager.GetEnvironmentalConditionsList();
            var dataDict = manager.GetDataDictionary();
            
            try
            {
                // Populate conditions list
                foreach (var condition in conditions)
                {
                    conditionsList.Add(condition);
                }
                
                operation(conditionsList, dataDict);
            }
            finally
            {
                manager.ReturnEnvironmentalConditionsList(conditionsList);
                manager.ReturnDataDictionary(dataDict);
            }
        }
        
        #endregion
        
        #region Unity Object Pooling Extensions
        
        /// <summary>
        /// Get a pooled Unity object with automatic return
        /// </summary>
        public static PooledUnityObjectHandle<T> GetPooledUnityObject<T>(this UnityObjectPool<T> pool) 
            where T : Component
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            
            var component = pool.Get();
            return new PooledUnityObjectHandle<T>(component, pool);
        }
        
        #endregion
        
        #region String Processing Extensions
        
        /// <summary>
        /// Process strings using pooled string collections
        /// </summary>
        public static void WithPooledStringOperation(this PooledObjectManager manager,
            IEnumerable<string> strings,
            Action<List<string>, Dictionary<string, object>> operation)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var stringList = manager.GetStringList();
            var dataDict = manager.GetDataDictionary();
            
            try
            {
                // Populate string list
                foreach (var str in strings)
                {
                    stringList.Add(str);
                }
                
                operation(stringList, dataDict);
            }
            finally
            {
                manager.ReturnStringList(stringList);
                manager.ReturnDataDictionary(dataDict);
            }
        }
        
        #endregion
        
        #region Batch Processing Extensions
        
        /// <summary>
        /// Process a large collection in batches using pooled collections
        /// </summary>
        public static void ProcessInBatches<T>(this PooledObjectManager manager,
            IEnumerable<T> items,
            int batchSize,
            Action<List<T>> batchProcessor) where T : class, new()
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (batchSize <= 0) throw new ArgumentException("Batch size must be positive", nameof(batchSize));
            
            var batch = manager.GetPooledObject<List<T>>();
            
            try
            {
                foreach (var item in items)
                {
                    batch.Add(item);
                    
                    if (batch.Count >= batchSize)
                    {
                        batchProcessor(batch);
                        batch.Clear();
                    }
                }
                
                // Process remaining items
                if (batch.Count > 0)
                {
                    batchProcessor(batch);
                }
            }
            finally
            {
                manager.ReturnPooledObject(batch);
            }
        }
        
        #endregion
        
        #region Performance Monitoring Extensions
        
        /// <summary>
        /// Execute an operation and log pool performance impact
        /// </summary>
        public static T WithPerformanceLogging<T>(this PooledObjectManager manager,
            string operationName,
            Func<T> operation)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            
            var startTime = Time.realtimeSinceStartup;
            var startMemory = System.GC.GetTotalMemory(false);
            
            try
            {
                var result = operation();
                
                var endTime = Time.realtimeSinceStartup;
                var endMemory = System.GC.GetTotalMemory(false);
                
                Debug.Log($"[PoolingExtensions] {operationName} completed in {(endTime - startTime) * 1000:F2}ms, " +
                         $"Memory delta: {(endMemory - startMemory) / 1024:F1}KB");
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PoolingExtensions] {operationName} failed: {ex.Message}");
                throw;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Handle for automatic return of Unity pooled objects
    /// Implements IDisposable for using() statement support
    /// </summary>
    public class PooledUnityObjectHandle<T> : IDisposable where T : Component
    {
        private readonly T _component;
        private readonly UnityObjectPool<T> _pool;
        private bool _disposed = false;
        
        public T Component => _component;
        public GameObject GameObject => _component?.gameObject;
        
        public PooledUnityObjectHandle(T component, UnityObjectPool<T> pool)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }
        
        public void Dispose()
        {
            if (!_disposed && _component != null && _pool != null)
            {
                _pool.Return(_component);
                _disposed = true;
            }
        }
        
        // Implicit conversion to component type
        public static implicit operator T(PooledUnityObjectHandle<T> handle)
        {
            return handle?.Component;
        }
        
        // Implicit conversion to GameObject
        public static implicit operator GameObject(PooledUnityObjectHandle<T> handle)
        {
            return handle?.GameObject;
        }
    }
}