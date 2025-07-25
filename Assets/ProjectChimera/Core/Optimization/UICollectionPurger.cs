using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Specialized purge optimization for UI data collections
    /// Manages frequently created/destroyed UI data structures to reduce garbage collection pressure
    /// </summary>
    public class UICollectionPurger : MonoBehaviour, IPoolable
    {
        [Header("UI Collection Pool Configuration")]
        [SerializeField] private int _maxDictionarySize = 100;
        [SerializeField] private int _maxListSize = 200;
        [SerializeField] private int _poolWarmupSize = 15;
        [SerializeField] private float _purgeInterval = 45f;

        // UI data structure pools
        private readonly Stack<Dictionary<string, object>> _dataDictionaries = new Stack<Dictionary<string, object>>();
        private readonly Stack<Dictionary<string, string>> _stringDictionaries = new Stack<Dictionary<string, string>>();
        private readonly Stack<List<object>> _genericLists = new Stack<List<object>>();
        private readonly Stack<List<string>> _stringLists = new Stack<List<string>>();
        private readonly Stack<List<float>> _floatLists = new Stack<List<float>>();
        private readonly Stack<List<int>> _intLists = new Stack<List<int>>();

        // Specialized UI data pools
        private readonly Stack<List<SensorReadingData>> _sensorReadingLists = new Stack<List<SensorReadingData>>();
        private readonly Stack<List<AutomationRuleData>> _automationRuleLists = new Stack<List<AutomationRuleData>>();
        private readonly Stack<List<LeaderboardEntryData>> _leaderboardLists = new Stack<List<LeaderboardEntryData>>();
        
        // Usage tracking
        private UICollectionUsageStats _usageStats = new UICollectionUsageStats();
        
        // Singleton instance
        private static UICollectionPurger _instance;
        public static UICollectionPurger Instance => _instance;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializePools();
                InvokeRepeating(nameof(PerformPeriodicPurge), _purgeInterval, _purgeInterval);
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
                CancelInvoke();
            }
        }

        #endregion

        #region Pool Initialization

        private void InitializePools()
        {
            // Pre-warm common UI data structures
            for (int i = 0; i < _poolWarmupSize; i++)
            {
                _dataDictionaries.Push(new Dictionary<string, object>());
                _stringDictionaries.Push(new Dictionary<string, string>());
                _genericLists.Push(new List<object>());
                _stringLists.Push(new List<string>());
                _floatLists.Push(new List<float>());
                _intLists.Push(new List<int>());
                _sensorReadingLists.Push(new List<SensorReadingData>());
                _automationRuleLists.Push(new List<AutomationRuleData>());
                _leaderboardLists.Push(new List<LeaderboardEntryData>());
            }

            Debug.Log($"[UICollectionPurger] Initialized UI collection pools with {_poolWarmupSize} pre-warmed collections");
        }

        #endregion

        #region Dictionary Pool Management

        /// <summary>
        /// Get a pooled data dictionary for UI operations
        /// </summary>
        public Dictionary<string, object> GetDataDictionary()
        {
            _usageStats.RecordDataDictionaryGet();
            
            if (_dataDictionaries.Count > 0)
            {
                var dict = _dataDictionaries.Pop();
                dict.Clear();
                return dict;
            }

            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Return a data dictionary to the pool
        /// </summary>
        public void ReturnDataDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null) return;

            _usageStats.RecordDataDictionaryReturn();
            
            // Only pool if not too large
            if (dictionary.Count <= _maxDictionarySize)
            {
                dictionary.Clear();
                _dataDictionaries.Push(dictionary);
            }
        }

        /// <summary>
        /// Get a pooled string dictionary
        /// </summary>
        public Dictionary<string, string> GetStringDictionary()
        {
            _usageStats.RecordStringDictionaryGet();
            
            if (_stringDictionaries.Count > 0)
            {
                var dict = _stringDictionaries.Pop();
                dict.Clear();
                return dict;
            }

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Return a string dictionary to the pool
        /// </summary>
        public void ReturnStringDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null) return;

            _usageStats.RecordStringDictionaryReturn();
            
            if (dictionary.Count <= _maxDictionarySize)
            {
                dictionary.Clear();
                _stringDictionaries.Push(dictionary);
            }
        }

        #endregion

        #region List Pool Management

        /// <summary>
        /// Get a pooled generic object list
        /// </summary>
        public List<object> GetGenericList()
        {
            _usageStats.RecordGenericListGet();
            
            if (_genericLists.Count > 0)
            {
                var list = _genericLists.Pop();
                list.Clear();
                return list;
            }

            return new List<object>();
        }

        /// <summary>
        /// Return a generic list to the pool
        /// </summary>
        public void ReturnGenericList(List<object> list)
        {
            if (list == null) return;

            _usageStats.RecordGenericListReturn();
            
            if (list.Capacity <= _maxListSize)
            {
                list.Clear();
                _genericLists.Push(list);
            }
        }

        /// <summary>
        /// Get a pooled string list
        /// </summary>
        public List<string> GetStringList()
        {
            if (_stringLists.Count > 0)
            {
                var list = _stringLists.Pop();
                list.Clear();
                return list;
            }

            return new List<string>();
        }

        /// <summary>
        /// Return a string list to the pool
        /// </summary>
        public void ReturnStringList(List<string> list)
        {
            if (list == null) return;

            if (list.Capacity <= _maxListSize)
            {
                list.Clear();
                _stringLists.Push(list);
            }
        }

        /// <summary>
        /// Get a pooled float list
        /// </summary>
        public List<float> GetFloatList()
        {
            if (_floatLists.Count > 0)
            {
                var list = _floatLists.Pop();
                list.Clear();
                return list;
            }

            return new List<float>();
        }

        /// <summary>
        /// Return a float list to the pool
        /// </summary>
        public void ReturnFloatList(List<float> list)
        {
            if (list == null) return;

            if (list.Capacity <= _maxListSize)
            {
                list.Clear();
                _floatLists.Push(list);
            }
        }

        /// <summary>
        /// Get a pooled int list
        /// </summary>
        public List<int> GetIntList()
        {
            if (_intLists.Count > 0)
            {
                var list = _intLists.Pop();
                list.Clear();
                return list;
            }

            return new List<int>();
        }

        /// <summary>
        /// Return an int list to the pool
        /// </summary>
        public void ReturnIntList(List<int> list)
        {
            if (list == null) return;

            if (list.Capacity <= _maxListSize)
            {
                list.Clear();
                _intLists.Push(list);
            }
        }

        #endregion

        #region Specialized UI Data Pool Management

        /// <summary>
        /// Get a pooled sensor reading data list
        /// </summary>
        public List<SensorReadingData> GetSensorReadingList()
        {
            if (_sensorReadingLists.Count > 0)
            {
                var list = _sensorReadingLists.Pop();
                list.Clear();
                return list;
            }

            return new List<SensorReadingData>();
        }

        /// <summary>
        /// Return a sensor reading list to the pool
        /// </summary>
        public void ReturnSensorReadingList(List<SensorReadingData> list)
        {
            if (list == null) return;

            // Reset all sensor data before pooling
            foreach (var reading in list)
            {
                reading?.Reset();
            }

            list.Clear();
            _sensorReadingLists.Push(list);
        }

        /// <summary>
        /// Get a pooled automation rule list
        /// </summary>
        public List<AutomationRuleData> GetAutomationRuleList()
        {
            if (_automationRuleLists.Count > 0)
            {
                var list = _automationRuleLists.Pop();
                list.Clear();
                return list;
            }

            return new List<AutomationRuleData>();
        }

        /// <summary>
        /// Return an automation rule list to the pool
        /// </summary>
        public void ReturnAutomationRuleList(List<AutomationRuleData> list)
        {
            if (list == null) return;

            foreach (var rule in list)
            {
                rule?.Reset();
            }

            list.Clear();
            _automationRuleLists.Push(list);
        }

        /// <summary>
        /// Get a pooled leaderboard entry list
        /// </summary>
        public List<LeaderboardEntryData> GetLeaderboardList()
        {
            if (_leaderboardLists.Count > 0)
            {
                var list = _leaderboardLists.Pop();
                list.Clear();
                return list;
            }

            return new List<LeaderboardEntryData>();
        }

        /// <summary>
        /// Return a leaderboard list to the pool
        /// </summary>
        public void ReturnLeaderboardList(List<LeaderboardEntryData> list)
        {
            if (list == null) return;

            foreach (var entry in list)
            {
                entry?.Reset();
            }

            list.Clear();
            _leaderboardLists.Push(list);
        }

        #endregion

        #region Batch Operations with Pooled Collections

        /// <summary>
        /// Execute a UI data operation using pooled collections
        /// </summary>
        public void ExecuteUIOperation<T>(IEnumerable<T> sourceData, System.Action<List<T>, Dictionary<string, object>> operation) where T : class
        {
            var dataList = GetGenericList();
            var contextDict = GetDataDictionary();

            try
            {
                // Convert to generic list
                foreach (var item in sourceData)
                {
                    dataList.Add(item);
                }

                // Cast for operation
                var typedList = dataList.Cast<T>().ToList();
                operation(typedList, contextDict);
            }
            finally
            {
                ReturnGenericList(dataList);
                ReturnDataDictionary(contextDict);
            }
        }

        /// <summary>
        /// Optimized dashboard data refresh using pooled collections
        /// </summary>
        public void RefreshDashboardData(System.Action<List<SensorReadingData>, Dictionary<string, string>> refreshAction)
        {
            var sensorList = GetSensorReadingList();
            var metadataDict = GetStringDictionary();

            try
            {
                refreshAction(sensorList, metadataDict);
            }
            finally
            {
                ReturnSensorReadingList(sensorList);
                ReturnStringDictionary(metadataDict);
            }
        }

        #endregion

        #region Pool Maintenance
        
        /// <summary>
        /// Initialize the UICollectionPurger manually (for testing)
        /// </summary>
        public void Initialize()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            InitializePools();
        }

        private void PerformPeriodicPurge()
        {
            // Trim pools to prevent memory bloat
            TrimPool(_dataDictionaries, _poolWarmupSize * 2);
            TrimPool(_stringDictionaries, _poolWarmupSize);
            TrimPool(_genericLists, _poolWarmupSize * 2);
            TrimPool(_stringLists, _poolWarmupSize);
            TrimPool(_floatLists, _poolWarmupSize);
            TrimPool(_intLists, _poolWarmupSize);
            TrimPool(_sensorReadingLists, _poolWarmupSize);
            TrimPool(_automationRuleLists, _poolWarmupSize);
            TrimPool(_leaderboardLists, _poolWarmupSize);

            Debug.Log("[UICollectionPurger] Performed periodic pool maintenance");
        }

        private void TrimPool<T>(Stack<T> pool, int maxSize)
        {
            while (pool.Count > maxSize)
            {
                pool.Pop();
            }
        }

        /// <summary>
        /// Get comprehensive usage statistics
        /// </summary>
        public UICollectionUsageStats GetUsageStats()
        {
            _usageStats.UpdatePoolSizes(
                _dataDictionaries.Count,
                _stringDictionaries.Count,
                _genericLists.Count,
                _stringLists.Count + _floatLists.Count + _intLists.Count,
                _sensorReadingLists.Count + _automationRuleLists.Count + _leaderboardLists.Count
            );

            return _usageStats;
        }

        /// <summary>
        /// Force purge all UI collection pools
        /// </summary>
        public void PurgeAllUIPools()
        {
            _dataDictionaries.Clear();
            _stringDictionaries.Clear();
            _genericLists.Clear();
            _stringLists.Clear();
            _floatLists.Clear();
            _intLists.Clear();
            _sensorReadingLists.Clear();
            _automationRuleLists.Clear();
            _leaderboardLists.Clear();

            Debug.Log("[UICollectionPurger] All UI collection pools purged");
        }

        #endregion

        #region IPoolable Implementation

        public void Reset()
        {
            _usageStats = new UICollectionUsageStats();
        }

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// Usage statistics for UI collection pools
    /// </summary>
    public class UICollectionUsageStats
    {
        public int DataDictionaryGets { get; private set; }
        public int DataDictionaryReturns { get; private set; }
        public int StringDictionaryGets { get; private set; }
        public int StringDictionaryReturns { get; private set; }
        public int GenericListGets { get; private set; }
        public int GenericListReturns { get; private set; }

        public int CurrentDataDictPoolSize { get; private set; }
        public int CurrentStringDictPoolSize { get; private set; }
        public int CurrentGenericListPoolSize { get; private set; }
        public int CurrentTypedListPoolSize { get; private set; }
        public int CurrentSpecializedListPoolSize { get; private set; }

        public float DataDictionaryReuseRate => DataDictionaryGets > 0 ? (float)DataDictionaryReturns / DataDictionaryGets : 0f;
        public float StringDictionaryReuseRate => StringDictionaryGets > 0 ? (float)StringDictionaryReturns / StringDictionaryGets : 0f;
        public float GenericListReuseRate => GenericListGets > 0 ? (float)GenericListReturns / GenericListGets : 0f;

        public void RecordDataDictionaryGet() => DataDictionaryGets++;
        public void RecordDataDictionaryReturn() => DataDictionaryReturns++;
        public void RecordStringDictionaryGet() => StringDictionaryGets++;
        public void RecordStringDictionaryReturn() => StringDictionaryReturns++;
        public void RecordGenericListGet() => GenericListGets++;
        public void RecordGenericListReturn() => GenericListReturns++;

        public void UpdatePoolSizes(int dataDictSize, int stringDictSize, int genericListSize, int typedListSize, int specializedListSize)
        {
            CurrentDataDictPoolSize = dataDictSize;
            CurrentStringDictPoolSize = stringDictSize;
            CurrentGenericListPoolSize = genericListSize;
            CurrentTypedListPoolSize = typedListSize;
            CurrentSpecializedListPoolSize = specializedListSize;
        }

        public override string ToString()
        {
            return $"UI Collection Pool Stats:\n" +
                   $"  Data Dictionaries: {CurrentDataDictPoolSize} pooled, {DataDictionaryReuseRate:P1} reuse\n" +
                   $"  String Dictionaries: {CurrentStringDictPoolSize} pooled, {StringDictionaryReuseRate:P1} reuse\n" +
                   $"  Generic Lists: {CurrentGenericListPoolSize} pooled, {GenericListReuseRate:P1} reuse\n" +
                   $"  Typed Lists: {CurrentTypedListPoolSize} pooled\n" +
                   $"  Specialized Lists: {CurrentSpecializedListPoolSize} pooled";
        }
    }

    /// <summary>
    /// Sensor reading data structure for UI operations
    /// </summary>
    public class SensorReadingData : IPoolable
    {
        public string SensorId { get; set; }
        public float Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string Unit { get; set; }
        public bool IsValid { get; set; }

        public void Reset()
        {
            SensorId = null;
            Value = 0f;
            Timestamp = default;
            Unit = null;
            IsValid = false;
        }
    }

    /// <summary>
    /// Automation rule data structure
    /// </summary>
    public class AutomationRuleData : IPoolable
    {
        public string RuleId { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public bool IsActive { get; set; }
        public float Threshold { get; set; }

        public void Reset()
        {
            RuleId = null;
            Condition = null;
            Action = null;
            IsActive = false;
            Threshold = 0f;
        }
    }

    /// <summary>
    /// Leaderboard entry data structure
    /// </summary>
    public class LeaderboardEntryData : IPoolable
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public float Score { get; set; }
        public int Rank { get; set; }
        public DateTime LastUpdate { get; set; }

        public void Reset()
        {
            PlayerId = null;
            PlayerName = null;
            Score = 0f;
            Rank = 0;
            LastUpdate = default;
        }
    }

    #endregion
}