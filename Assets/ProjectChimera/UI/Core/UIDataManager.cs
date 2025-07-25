using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Manages data caching, binding, and updates for the UI system.
    /// Handles efficient data refresh and provides cached access to system data.
    /// </summary>
    public class UIDataManager : IDisposable
    {
        private Dictionary<string, CachedData> _dataCache = new Dictionary<string, CachedData>();
        private Dictionary<string, List<IDataBinding>> _dataBindings = new Dictionary<string, List<IDataBinding>>();
        private float _cacheExpirationTime = 1f; // 1 second cache
        
        // Data source delegates
        public System.Func<EnvironmentalData> GetEnvironmentalData;
        public System.Func<PlantStatusData> GetPlantStatusData;
        public System.Func<SystemPerformanceData> GetSystemPerformanceData;
        public System.Func<ConstructionData> GetConstructionData;
        public System.Func<EconomicData> GetEconomicData;
        
        public T GetCachedData<T>(string dataKey, System.Func<T> dataProvider) where T : class
        {
            if (_dataCache.TryGetValue(dataKey, out var cachedData))
            {
                if (Time.time - cachedData.Timestamp < _cacheExpirationTime)
                {
                    return cachedData.Data as T;
                }
            }
            
            // Cache expired or doesn't exist, refresh
            var newData = dataProvider?.Invoke();
            if (newData != null)
            {
                _dataCache[dataKey] = new CachedData
                {
                    Data = newData,
                    Timestamp = Time.time
                };
                
                // Notify bindings
                NotifyDataBindings(dataKey, newData);
            }
            
            return newData;
        }
        
        public void RegisterDataBinding(string dataKey, IDataBinding binding)
        {
            if (!_dataBindings.ContainsKey(dataKey))
            {
                _dataBindings[dataKey] = new List<IDataBinding>();
            }
            
            _dataBindings[dataKey].Add(binding);
        }
        
        public void UnregisterDataBinding(string dataKey, IDataBinding binding)
        {
            if (_dataBindings.TryGetValue(dataKey, out var bindings))
            {
                bindings.Remove(binding);
            }
        }
        
        private void NotifyDataBindings(string dataKey, object data)
        {
            if (_dataBindings.TryGetValue(dataKey, out var bindings))
            {
                foreach (var binding in bindings.ToList())
                {
                    binding.UpdateData(data);
                }
            }
        }
        
        public void RefreshAllData()
        {
            var keysToRefresh = _dataCache.Keys.ToList();
            foreach (var key in keysToRefresh)
            {
                // Force cache expiration
                _dataCache[key].Timestamp = 0f;
            }
        }
        
        public void ClearCache()
        {
            _dataCache.Clear();
        }
        
        public void Dispose()
        {
            _dataCache.Clear();
            _dataBindings.Clear();
        }
        
        private class CachedData
        {
            public object Data;
            public float Timestamp;
        }
    }
    
    public interface IDataBinding
    {
        void UpdateData(object data);
    }
}