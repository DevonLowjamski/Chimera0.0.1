using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Central manager for all purge optimization systems
    /// Coordinates ObjectPurgeManager, PlantUpdateCollectionPurger, and UICollectionPurger
    /// Provides unified interface and performance monitoring
    /// </summary>
    public class PurgeOptimizationManager : ChimeraManager
    {
        [Header("Optimization Configuration")]
        [SerializeField] private bool _enablePlantCollectionPurging = true;
        [SerializeField] private bool _enableUICollectionPurging = true;
        [SerializeField] private bool _enableGeneralObjectPurging = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;

        [Header("Performance Monitoring")]
        [SerializeField] private float _statsUpdateInterval = 10f;
        [SerializeField] private int _performanceHistorySize = 60; // Keep 10 minutes of data at 10s intervals
        [SerializeField] private bool _logPerformanceStats = false;

        // Component references
        private ObjectPurgeManager _objectPurgeManager;
        private PlantUpdateCollectionPurger _plantCollectionPurger;
        private UICollectionPurger _uiCollectionPurger;

        // Performance monitoring
        private readonly Queue<PerformanceSnapshot> _performanceHistory = new Queue<PerformanceSnapshot>();
        private Coroutine _monitoringCoroutine;
        private DateTime _initializationTime;

        // Statistics
        private PurgeOptimizationStats _currentStats = new PurgeOptimizationStats();

        #region Unity Lifecycle

        protected override void OnManagerInitialize()
        {
            Debug.Log("[PurgeOptimizationManager] Initializing purge optimization systems");
            _initializationTime = DateTime.Now;

            InitializeComponents();
            
            if (_enablePerformanceMonitoring)
            {
                _monitoringCoroutine = StartCoroutine(PerformanceMonitoringCoroutine());
            }
        }

        protected override void OnManagerShutdown()
        {
            Debug.Log("[PurgeOptimizationManager] Shutting down purge optimization");
            
            if (_monitoringCoroutine != null)
            {
                StopCoroutine(_monitoringCoroutine);
                _monitoringCoroutine = null;
            }

            // Final cleanup
            PurgeAllSystems();
            
            LogFinalStats();
        }

        #endregion

        #region Component Initialization

        private void InitializeComponents()
        {
            // Initialize Object Purge Manager
            if (_enableGeneralObjectPurging)
            {
                _objectPurgeManager = FindObjectOfType<ObjectPurgeManager>();
                if (_objectPurgeManager == null)
                {
                    var purgeManagerGO = new GameObject("ObjectPurgeManager");
                    purgeManagerGO.transform.SetParent(transform);
                    _objectPurgeManager = purgeManagerGO.AddComponent<ObjectPurgeManager>();
                }
                Debug.Log("[PurgeOptimizationManager] Object purge manager initialized");
            }

            // Initialize Plant Collection Purger
            if (_enablePlantCollectionPurging)
            {
                _plantCollectionPurger = FindObjectOfType<PlantUpdateCollectionPurger>();
                if (_plantCollectionPurger == null)
                {
                    var plantPurgerGO = new GameObject("PlantUpdateCollectionPurger");
                    plantPurgerGO.transform.SetParent(transform);
                    _plantCollectionPurger = plantPurgerGO.AddComponent<PlantUpdateCollectionPurger>();
                }
                Debug.Log("[PurgeOptimizationManager] Plant collection purger initialized");
            }

            // Initialize UI Collection Purger
            if (_enableUICollectionPurging)
            {
                _uiCollectionPurger = FindObjectOfType<UICollectionPurger>();
                if (_uiCollectionPurger == null)
                {
                    var uiPurgerGO = new GameObject("UICollectionPurger");
                    uiPurgerGO.transform.SetParent(transform);
                    _uiCollectionPurger = uiPurgerGO.AddComponent<UICollectionPurger>();
                }
                Debug.Log("[PurgeOptimizationManager] UI collection purger initialized");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get or create optimized plant instance list
        /// </summary>
        public List<GameObject> GetOptimizedPlantList()
        {
            return _plantCollectionPurger?.GetPlantInstanceList() ?? new List<GameObject>();
        }

        /// <summary>
        /// Return plant instance list to optimized pool
        /// </summary>
        public void ReturnOptimizedPlantList(List<GameObject> list)
        {
            _plantCollectionPurger?.ReturnPlantInstanceList(list);
        }

        /// <summary>
        /// Get optimized UI data dictionary
        /// </summary>
        public Dictionary<string, object> GetOptimizedUIData()
        {
            return _uiCollectionPurger?.GetDataDictionary() ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Return UI data dictionary to optimized pool
        /// </summary>
        public void ReturnOptimizedUIData(Dictionary<string, object> data)
        {
            _uiCollectionPurger?.ReturnDataDictionary(data);
        }

        /// <summary>
        /// Execute optimized plant batch operation
        /// </summary>
        public void ExecuteOptimizedPlantBatch(IEnumerable<GameObject> plants, 
            System.Action<List<GameObject>, Dictionary<string, object>> batchProcessor)
        {
            if (_plantCollectionPurger != null)
            {
                _plantCollectionPurger.ExecuteOptimizedPlantBatch(plants, batchProcessor);
            }
            else
            {
                // Fallback to non-optimized operation
                var plantList = new List<GameObject>(plants);
                var cache = new Dictionary<string, object>();
                batchProcessor(plantList, cache);
            }
        }

        /// <summary>
        /// Execute optimized UI refresh operation
        /// </summary>
        public void ExecuteOptimizedUIRefresh(System.Action<List<SensorReadingData>, Dictionary<string, string>> refreshAction)
        {
            if (_uiCollectionPurger != null)
            {
                _uiCollectionPurger.RefreshDashboardData(refreshAction);
            }
            else
            {
                // Fallback to non-optimized operation
                var sensorList = new List<SensorReadingData>();
                var metadata = new Dictionary<string, string>();
                refreshAction(sensorList, metadata);
            }
        }

        /// <summary>
        /// Force purge of all optimization systems
        /// </summary>
        public void PurgeAllSystems()
        {
            _objectPurgeManager?.ForcePurgeAll();
            _plantCollectionPurger?.PurgeAllPools();
            _uiCollectionPurger?.PurgeAllUIPools();
            
            // Force garbage collection after purging
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            Debug.Log("[PurgeOptimizationManager] All purge systems cleared and GC forced");
        }

        /// <summary>
        /// Get comprehensive optimization statistics
        /// </summary>
        public PurgeOptimizationStats GetOptimizationStats()
        {
            UpdateCurrentStats();
            return _currentStats;
        }

        /// <summary>
        /// Get performance history for analysis
        /// </summary>
        public IReadOnlyList<PerformanceSnapshot> GetPerformanceHistory()
        {
            return _performanceHistory.ToArray();
        }

        #endregion

        #region Performance Monitoring

        private IEnumerator PerformanceMonitoringCoroutine()
        {
            while (_enablePerformanceMonitoring)
            {
                try
                {
                    CapturePerformanceSnapshot();
                    UpdateCurrentStats();
                    
                    if (_logPerformanceStats)
                    {
                        LogCurrentStats();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PurgeOptimizationManager] Error in performance monitoring: {ex.Message}");
                }

                yield return new WaitForSeconds(_statsUpdateInterval);
            }
        }

        private void CapturePerformanceSnapshot()
        {
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.Now,
                TotalMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f),
                FrameRate = 1f / Time.unscaledDeltaTime,
                PlantPoolStats = _plantCollectionPurger?.GetPoolStats(),
                UIPoolStats = _uiCollectionPurger?.GetUsageStats(),
                ObjectPurgeStats = _objectPurgeManager?.GetPurgeStats()
            };

            _performanceHistory.Enqueue(snapshot);
            
            // Maintain history size
            while (_performanceHistory.Count > _performanceHistorySize)
            {
                _performanceHistory.Dequeue();
            }
        }

        private void UpdateCurrentStats()
        {
            var uptime = DateTime.Now - _initializationTime;
            
            _currentStats = new PurgeOptimizationStats
            {
                IsOptimizationActive = _enableGeneralObjectPurging || _enablePlantCollectionPurging || _enableUICollectionPurging,
                UptimeSeconds = (float)uptime.TotalSeconds,
                PlantOptimizationEnabled = _enablePlantCollectionPurging,
                UIOptimizationEnabled = _enableUICollectionPurging,
                GeneralPurgeEnabled = _enableGeneralObjectPurging,
                PerformanceHistoryCount = _performanceHistory.Count,
                CurrentMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f),
                CurrentFrameRate = 1f / Time.unscaledDeltaTime
            };

            // Add latest pool stats if available
            if (_performanceHistory.Count > 0)
            {
                var latest = _performanceHistory.ToArray()[_performanceHistory.Count - 1];
                _currentStats.LatestSnapshot = latest;
            }
        }

        private void LogCurrentStats()
        {
            var stats = _currentStats;
            Debug.Log($"[PurgeOptimizationManager] Performance Stats - " +
                     $"Memory: {stats.CurrentMemoryMB:F1}MB, " +
                     $"FPS: {stats.CurrentFrameRate:F1}, " +
                     $"Uptime: {stats.UptimeSeconds:F0}s, " +
                     $"History: {stats.PerformanceHistoryCount} snapshots");
            
            if (stats.LatestSnapshot?.PlantPoolStats != null)
            {
                Debug.Log($"[PurgeOptimizationManager] Plant Pool - " +
                         $"Lists: {stats.LatestSnapshot.PlantPoolStats.PlantListPoolSize}, " +
                         $"Reuse: {stats.LatestSnapshot.PlantPoolStats.PlantListReuseRate:P1}");
            }
        }

        private void LogFinalStats()
        {
            var uptime = DateTime.Now - _initializationTime;
            Debug.Log($"[PurgeOptimizationManager] Final Stats - " +
                     $"Total uptime: {uptime.TotalMinutes:F1} minutes, " +
                     $"Performance snapshots captured: {_performanceHistory.Count}");
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Enable or disable specific optimization systems at runtime
        /// </summary>
        public void ConfigureOptimization(bool plantOptimization, bool uiOptimization, bool generalPurge)
        {
            _enablePlantCollectionPurging = plantOptimization;
            _enableUICollectionPurging = uiOptimization;
            _enableGeneralObjectPurging = generalPurge;
            
            Debug.Log($"[PurgeOptimizationManager] Configuration updated - " +
                     $"Plant: {plantOptimization}, UI: {uiOptimization}, General: {generalPurge}");
        }

        /// <summary>
        /// Enable or disable performance monitoring
        /// </summary>
        public void SetPerformanceMonitoring(bool enabled)
        {
            if (_enablePerformanceMonitoring != enabled)
            {
                _enablePerformanceMonitoring = enabled;
                
                if (enabled && _monitoringCoroutine == null)
                {
                    _monitoringCoroutine = StartCoroutine(PerformanceMonitoringCoroutine());
                }
                else if (!enabled && _monitoringCoroutine != null)
                {
                    StopCoroutine(_monitoringCoroutine);
                    _monitoringCoroutine = null;
                }
                
                Debug.Log($"[PurgeOptimizationManager] Performance monitoring {(enabled ? "enabled" : "disabled")}");
            }
        }

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// Performance snapshot for monitoring
    /// </summary>
    public class PerformanceSnapshot
    {
        public DateTime Timestamp { get; set; }
        public float TotalMemoryMB { get; set; }
        public float FrameRate { get; set; }
        public PlantCollectionPoolStats PlantPoolStats { get; set; }
        public UICollectionUsageStats UIPoolStats { get; set; }
        public PurgeManagerStats ObjectPurgeStats { get; set; }
    }

    /// <summary>
    /// Comprehensive optimization statistics
    /// </summary>
    public class PurgeOptimizationStats
    {
        public bool IsOptimizationActive { get; set; }
        public float UptimeSeconds { get; set; }
        public bool PlantOptimizationEnabled { get; set; }
        public bool UIOptimizationEnabled { get; set; }
        public bool GeneralPurgeEnabled { get; set; }
        public int PerformanceHistoryCount { get; set; }
        public float CurrentMemoryMB { get; set; }
        public float CurrentFrameRate { get; set; }
        public PerformanceSnapshot LatestSnapshot { get; set; }

        public override string ToString()
        {
            return $"Purge Optimization Stats:\n" +
                   $"  Active: {IsOptimizationActive}\n" +
                   $"  Uptime: {UptimeSeconds:F0}s\n" +
                   $"  Memory: {CurrentMemoryMB:F1}MB\n" +
                   $"  FPS: {CurrentFrameRate:F1}\n" +
                   $"  Plant Opt: {PlantOptimizationEnabled}\n" +
                   $"  UI Opt: {UIOptimizationEnabled}\n" +
                   $"  General Purge: {GeneralPurgeEnabled}\n" +
                   $"  History: {PerformanceHistoryCount} snapshots";
        }
    }

    #endregion
}