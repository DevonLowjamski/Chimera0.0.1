using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.SpeedTree;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Master optimization coordinator for plant update systems
    /// Integrates scheduler, batch processor, and pooling for maximum performance
    /// Target: 1000+ plants at 60 FPS
    /// </summary>
    public class PlantUpdateOptimizer : ChimeraManager
    {
        // Singleton instance
        private static PlantUpdateOptimizer _instance;
        public static PlantUpdateOptimizer Instance => _instance;
        
        [Header("Performance Targets")]
        [SerializeField] private int _targetPlantCount = 1000;
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _maxUpdateTimePerFrameMs = 5f; // 5ms budget per frame for plant updates
        
        [Header("Optimization Strategy")]
        [SerializeField] private OptimizationStrategy _strategy = OptimizationStrategy.Adaptive;
        [SerializeField] private bool _enableLODOptimization = true;
        [SerializeField] private bool _enableDistanceCulling = true;
        [SerializeField] private bool _enableFrustumCulling = true;
        
        [Header("LOD Configuration")]
        [SerializeField] private float _highDetailDistance = 25f;
        [SerializeField] private float _mediumDetailDistance = 75f;
        [SerializeField] private float _lowDetailDistance = 150f;
        [SerializeField] private float _cullingDistance = 300f;
        
        [Header("Update Frequency Control")]
        [SerializeField] private int _highDetailUpdateFrequency = 1;    // Every frame
        [SerializeField] private int _mediumDetailUpdateFrequency = 3;  // Every 3 frames
        [SerializeField] private int _lowDetailUpdateFrequency = 10;   // Every 10 frames
        [SerializeField] private int _culledUpdateFrequency = 30;      // Every 30 frames
        
        [Header("Performance Monitoring")]
        [SerializeField] private bool _enableRealTimeMonitoring = true;
        [SerializeField] private float _monitoringInterval = 1f;
        [SerializeField] private bool _enableAutoOptimization = true;
        
        // Component references
        private PlantUpdateScheduler _scheduler;
        private PlantBatchProcessor _batchProcessor;
        private PooledObjectManager _poolManager;
        private Camera _mainCamera;
        
        // Plant management
        private readonly Dictionary<SpeedTreePlantInstance, PlantLODData> _plantLODData = new Dictionary<SpeedTreePlantInstance, PlantLODData>();
        private readonly Dictionary<LODLevel, List<SpeedTreePlantInstance>> _lodGroups = new Dictionary<LODLevel, List<SpeedTreePlantInstance>>();
        
        // Performance tracking
        private float _currentFrameRate = 60f;
        private float _currentUpdateTime = 0f;
        private int _activePlantCount = 0;
        private int _frameCounter = 0;
        
        // Optimization state
        private bool _performanceWarningActive = false;
        private DateTime _lastOptimizationTime;
        
        // Statistics properties
        public int ActivePlantCount => _activePlantCount;
        public float CurrentFrameRate => _currentFrameRate;
        public float CurrentUpdateTime => _currentUpdateTime;
        public OptimizationStrategy CurrentStrategy => _strategy;
        
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
            _lastOptimizationTime = DateTime.Now;
            
            InitializeComponents();
            InitializeLODGroups();
            
            if (_enableRealTimeMonitoring)
            {
                InvokeRepeating(nameof(MonitorPerformance), _monitoringInterval, _monitoringInterval);
            }
            
            Debug.Log($"[PlantUpdateOptimizer] Initialized - Target: {_targetPlantCount} plants @ {_targetFrameRate} FPS, " +
                     $"Budget: {_maxUpdateTimePerFrameMs}ms, Strategy: {_strategy}");
        }
        
        protected override void OnManagerShutdown()
        {
            if (_instance == this)
            {
                CancelInvoke();
                ClearAllData();
                _instance = null;
                
                Debug.Log("[PlantUpdateOptimizer] Shutdown complete");
            }
        }
        
        private void Update()
        {
            if (_enableLODOptimization)
            {
                UpdatePlantLOD();
            }
            
            ProcessFrameBasedUpdates();
            
            _frameCounter++;
        }
        
        #endregion
        
        #region Component Initialization
        
        private void InitializeComponents()
        {
            // Get or create required components
            _scheduler = PlantUpdateScheduler.Instance;
            if (_scheduler == null)
            {
                var schedulerGO = new GameObject("PlantUpdateScheduler");
                schedulerGO.transform.SetParent(transform);
                _scheduler = schedulerGO.AddComponent<PlantUpdateScheduler>();
            }
            
            _batchProcessor = PlantBatchProcessor.Instance;
            if (_batchProcessor == null)
            {
                var processorGO = new GameObject("PlantBatchProcessor");
                processorGO.transform.SetParent(transform);
                _batchProcessor = processorGO.AddComponent<PlantBatchProcessor>();
            }
            
            _poolManager = PooledObjectManager.Instance;
            _mainCamera = Camera.main;
            
            if (_mainCamera == null)
            {
                Debug.LogWarning("[PlantUpdateOptimizer] Main camera not found, distance-based optimizations disabled");
            }
        }
        
        private void InitializeLODGroups()
        {
            // Initialize LOD group dictionaries
            foreach (LODLevel level in Enum.GetValues(typeof(LODLevel)))
            {
                _lodGroups[level] = new List<SpeedTreePlantInstance>();
            }
        }
        
        #endregion
        
        #region Plant Registration
        
        /// <summary>
        /// Register a plant for optimized updates
        /// </summary>
        public void RegisterPlant(SpeedTreePlantInstance plant)
        {
            if (plant == null || _plantLODData.ContainsKey(plant)) return;
            
            var lodData = new PlantLODData
            {
                Plant = plant,
                CurrentLOD = LODLevel.High,
                FramesSinceLastUpdate = 0,
                LastUpdateTime = Time.time,
                DistanceToCamera = float.MaxValue
            };
            
            _plantLODData[plant] = lodData;
            _lodGroups[LODLevel.High].Add(plant);
            
            // Register with scheduler
            _scheduler?.RegisterPlant(plant);
            
            _activePlantCount++;
            
            Debug.Log($"[PlantUpdateOptimizer] Registered plant {plant.name} for optimized updates");
        }
        
        /// <summary>
        /// Unregister a plant from optimized updates
        /// </summary>
        public void UnregisterPlant(SpeedTreePlantInstance plant)
        {
            if (plant == null || !_plantLODData.ContainsKey(plant)) return;
            
            var lodData = _plantLODData[plant];
            _lodGroups[lodData.CurrentLOD].Remove(plant);
            _plantLODData.Remove(plant);
            
            // Unregister from scheduler
            _scheduler?.UnregisterPlant(plant);
            
            _activePlantCount--;
            
            Debug.Log($"[PlantUpdateOptimizer] Unregistered plant {plant.name}");
        }
        
        #endregion
        
        #region LOD Management
        
        private void UpdatePlantLOD()
        {
            if (_mainCamera == null) return;
            
            var cameraPosition = _mainCamera.transform.position;
            var cameraFrustum = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
            
            // Update LOD for all plants
            foreach (var kvp in _plantLODData)
            {
                var plant = kvp.Key;
                var lodData = kvp.Value;
                
                if (plant == null || !plant.gameObject.activeInHierarchy) continue;
                
                // Calculate distance to camera
                var distanceToCamera = Vector3.Distance(cameraPosition, plant.transform.position);
                lodData.DistanceToCamera = distanceToCamera;
                
                // Determine new LOD level
                var newLOD = CalculateLODLevel(plant, distanceToCamera, cameraFrustum);
                
                // Update LOD if changed
                if (newLOD != lodData.CurrentLOD)
                {
                    UpdatePlantLODLevel(plant, lodData, newLOD);
                }
            }
        }
        
        private LODLevel CalculateLODLevel(SpeedTreePlantInstance plant, float distance, Plane[] frustumPlanes)
        {
            // Frustum culling check
            if (_enableFrustumCulling)
            {
                var bounds = new Bounds(plant.transform.position, Vector3.one * 2f); // Approximate plant bounds
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                {
                    return LODLevel.Culled;
                }
            }
            
            // Distance-based LOD
            if (_enableDistanceCulling && distance > _cullingDistance)
            {
                return LODLevel.Culled;
            }
            else if (distance <= _highDetailDistance)
            {
                return LODLevel.High;
            }
            else if (distance <= _mediumDetailDistance)
            {
                return LODLevel.Medium;
            }
            else if (distance <= _lowDetailDistance)
            {
                return LODLevel.Low;
            }
            else
            {
                return LODLevel.Culled;
            }
        }
        
        private void UpdatePlantLODLevel(SpeedTreePlantInstance plant, PlantLODData lodData, LODLevel newLOD)
        {
            // Remove from old LOD group
            _lodGroups[lodData.CurrentLOD].Remove(plant);
            
            // Add to new LOD group
            _lodGroups[newLOD].Add(plant);
            
            // Update LOD data
            lodData.CurrentLOD = newLOD;
            lodData.FramesSinceLastUpdate = 0;
        }
        
        #endregion
        
        #region Frame-Based Update Processing
        
        private void ProcessFrameBasedUpdates()
        {
            var frameStartTime = Time.realtimeSinceStartup;
            var budgetRemaining = _maxUpdateTimePerFrameMs / 1000f; // Convert to seconds
            
            // Process each LOD level with appropriate frequency
            ProcessLODGroup(LODLevel.High, _highDetailUpdateFrequency, PlantUpdateType.Full, ref budgetRemaining);
            if (budgetRemaining <= 0) return;
            
            ProcessLODGroup(LODLevel.Medium, _mediumDetailUpdateFrequency, PlantUpdateType.Growth, ref budgetRemaining);
            if (budgetRemaining <= 0) return;
            
            ProcessLODGroup(LODLevel.Low, _lowDetailUpdateFrequency, PlantUpdateType.Environmental, ref budgetRemaining);
            if (budgetRemaining <= 0) return;
            
            ProcessLODGroup(LODLevel.Culled, _culledUpdateFrequency, PlantUpdateType.Health, ref budgetRemaining);
            
            // Update performance metrics
            var frameEndTime = Time.realtimeSinceStartup;
            _currentUpdateTime = (frameEndTime - frameStartTime) * 1000f; // Convert to ms
        }
        
        private void ProcessLODGroup(LODLevel lodLevel, int updateFrequency, PlantUpdateType updateType, ref float budgetRemaining)
        {
            if (budgetRemaining <= 0 || _frameCounter % updateFrequency != 0) return;
            
            var plants = _lodGroups[lodLevel];
            if (plants.Count == 0) return;
            
            var processingStartTime = Time.realtimeSinceStartup;
            
            // Process plants in this LOD group
            _batchProcessor?.ProcessPlantCollection(plants, updateType);
            
            var processingEndTime = Time.realtimeSinceStartup;
            var processingTime = processingEndTime - processingStartTime;
            
            budgetRemaining -= processingTime;
            
            // Update frame counts for plants in this group
            foreach (var plant in plants)
            {
                if (_plantLODData.TryGetValue(plant, out var lodData))
                {
                    lodData.FramesSinceLastUpdate = 0;
                    lodData.LastUpdateTime = Time.time;
                }
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void MonitorPerformance()
        {
            // Update current frame rate
            _currentFrameRate = 1f / Time.unscaledDeltaTime;
            
            // Check performance thresholds
            var isPerformancePoor = _currentFrameRate < _targetFrameRate * 0.9f || // Less than 90% of target FPS
                                   _currentUpdateTime > _maxUpdateTimePerFrameMs * 1.5f; // Exceeding budget by 50%
            
            if (isPerformancePoor && !_performanceWarningActive)
            {
                _performanceWarningActive = true;
                Debug.LogWarning($"[PlantUpdateOptimizer] Performance degradation detected - " +
                               $"FPS: {_currentFrameRate:F1}, Update Time: {_currentUpdateTime:F2}ms");
                
                if (_enableAutoOptimization)
                {
                    OptimizePerformance();
                }
            }
            else if (!isPerformancePoor && _performanceWarningActive)
            {
                _performanceWarningActive = false;
                Debug.Log($"[PlantUpdateOptimizer] Performance restored - " +
                         $"FPS: {_currentFrameRate:F1}, Update Time: {_currentUpdateTime:F2}ms");
            }
        }
        
        private void OptimizePerformance()
        {
            var timeSinceLastOptimization = DateTime.Now - _lastOptimizationTime;
            if (timeSinceLastOptimization.TotalSeconds < 5f) return; // Don't optimize too frequently
            
            Debug.Log("[PlantUpdateOptimizer] Applying automatic performance optimizations");
            
            // Increase LOD distances to reduce high-detail plant count
            _highDetailDistance *= 0.9f;
            _mediumDetailDistance *= 0.9f;
            _lowDetailDistance *= 0.9f;
            
            // Reduce update frequencies
            _mediumDetailUpdateFrequency = Mathf.Min(_mediumDetailUpdateFrequency + 1, 10);
            _lowDetailUpdateFrequency = Mathf.Min(_lowDetailUpdateFrequency + 2, 30);
            
            // Update batch processor configuration
            if (_batchProcessor != null)
            {
                var stats = _batchProcessor.GetProcessorStats();
                var newBatchSize = Mathf.Max(stats.OptimalBatchSize - 5, 10);
                _batchProcessor.UpdateConfiguration(newBatchSize, false, true);
            }
            
            _lastOptimizationTime = DateTime.Now;
            
            Debug.Log($"[PlantUpdateOptimizer] Optimization applied - " +
                     $"LOD distances reduced, update frequencies increased");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Initialize the PlantUpdateOptimizer (alias for OnManagerInitialize for testing)
        /// </summary>
        public void Initialize()
        {
            OnManagerInitialize();
        }
        
        /// <summary>
        /// Force optimization of all plant update systems
        /// </summary>
        public void ForceOptimization()
        {
            OptimizePerformance();
        }
        
        /// <summary>
        /// Optimize plant updates for the given collection
        /// </summary>
        public void OptimizePlantUpdates(List<SpeedTreePlantInstance> plants)
        {
            if (plants == null || plants.Count == 0)
            {
                Debug.LogWarning("[PlantUpdateOptimizer] No plants provided for optimization");
                return;
            }
            
            // Update LOD data for all plants
            UpdateLODData(plants);
            
            // Distribute plants into LOD groups  
            DistributePlantsToLODGroups();
            
            Debug.Log($"[PlantUpdateOptimizer] Optimized {plants.Count} plants for LOD rendering");
        }
        
        /// <summary>
        /// Update optimization strategy
        /// </summary>
        public void SetOptimizationStrategy(OptimizationStrategy strategy)
        {
            _strategy = strategy;
            
            switch (strategy)
            {
                case OptimizationStrategy.Performance:
                    // Favor performance over quality
                    _highDetailDistance = 15f;
                    _mediumDetailDistance = 50f;
                    _lowDetailDistance = 100f;
                    _maxUpdateTimePerFrameMs = 3f;
                    break;
                    
                case OptimizationStrategy.Quality:
                    // Favor quality over performance
                    _highDetailDistance = 40f;
                    _mediumDetailDistance = 100f;
                    _lowDetailDistance = 200f;
                    _maxUpdateTimePerFrameMs = 8f;
                    break;
                    
                case OptimizationStrategy.Balanced:
                    // Balanced approach
                    _highDetailDistance = 25f;
                    _mediumDetailDistance = 75f;
                    _lowDetailDistance = 150f;
                    _maxUpdateTimePerFrameMs = 5f;
                    break;
                    
                case OptimizationStrategy.Adaptive:
                    // Use dynamic optimization based on performance
                    _enableAutoOptimization = true;
                    break;
            }
            
            Debug.Log($"[PlantUpdateOptimizer] Optimization strategy set to {strategy}");
        }
        
        /// <summary>
        /// Get comprehensive optimization statistics
        /// </summary>
        public PlantOptimizerStats GetOptimizerStats()
        {
            var lodCounts = new Dictionary<LODLevel, int>();
            foreach (var kvp in _lodGroups)
            {
                lodCounts[kvp.Key] = kvp.Value.Count;
            }
            
            return new PlantOptimizerStats
            {
                ActivePlantCount = _activePlantCount,
                CurrentFrameRate = _currentFrameRate,
                CurrentUpdateTime = _currentUpdateTime,
                TargetFrameRate = _targetFrameRate,
                UpdateBudgetMs = _maxUpdateTimePerFrameMs,
                OptimizationStrategy = _strategy,
                LODCounts = lodCounts,
                PerformanceWarningActive = _performanceWarningActive,
                HighDetailDistance = _highDetailDistance,
                MediumDetailDistance = _mediumDetailDistance,
                LowDetailDistance = _lowDetailDistance
            };
        }
        
        /// <summary>
        /// Clear all plant data and reset optimizer
        /// </summary>
        public void ClearAllData()
        {
            _plantLODData.Clear();
            foreach (var group in _lodGroups.Values)
            {
                group.Clear();
            }
            _activePlantCount = 0;
            
            Debug.Log("[PlantUpdateOptimizer] All plant data cleared");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Update LOD data for all plants based on camera distance
        /// </summary>
        private void UpdateLODData(List<SpeedTreePlantInstance> plants)
        {
            if (_mainCamera == null) return;
            
            var cameraPosition = _mainCamera.transform.position;
            
            foreach (var plant in plants)
            {
                if (plant == null) continue;
                
                var distance = Vector3.Distance(cameraPosition, plant.transform.position);
                
                if (!_plantLODData.ContainsKey(plant))
                {
                    _plantLODData[plant] = new PlantLODData { Plant = plant };
                }
                
                var lodData = _plantLODData[plant];
                lodData.DistanceToCamera = distance;
                
                // Determine LOD level based on distance
                if (distance <= _highDetailDistance)
                    lodData.CurrentLOD = LODLevel.High;
                else if (distance <= _mediumDetailDistance)
                    lodData.CurrentLOD = LODLevel.Medium;
                else if (distance <= _lowDetailDistance)
                    lodData.CurrentLOD = LODLevel.Low;
                else
                    lodData.CurrentLOD = LODLevel.Culled;
            }
        }
        
        /// <summary>
        /// Distribute plants into LOD groups for optimized processing
        /// </summary>
        private void DistributePlantsToLODGroups()
        {
            // Clear existing groups
            foreach (var group in _lodGroups.Values)
            {
                group.Clear();
            }
            
            // Distribute plants by LOD level
            foreach (var kvp in _plantLODData)
            {
                var plant = kvp.Key;
                var lodData = kvp.Value;
                
                if (!_lodGroups.ContainsKey(lodData.CurrentLOD))
                {
                    _lodGroups[lodData.CurrentLOD] = new List<SpeedTreePlantInstance>();
                }
                
                _lodGroups[lodData.CurrentLOD].Add(plant);
            }
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    /// <summary>
    /// Optimization strategies for plant updates
    /// </summary>
    public enum OptimizationStrategy
    {
        Performance,    // Maximize frame rate at cost of quality
        Quality,        // Maximize quality at cost of frame rate
        Balanced,       // Balance performance and quality
        Adaptive        // Dynamically adjust based on performance
    }
    
    /// <summary>
    /// Level of detail for plant updates
    /// </summary>
    public enum LODLevel
    {
        High,       // Full detail, every frame
        Medium,     // Reduced detail, every few frames
        Low,        // Minimal detail, infrequent updates
        Culled      // Not visible, very infrequent updates
    }
    
    /// <summary>
    /// LOD data for individual plants
    /// </summary>
    public class PlantLODData
    {
        public SpeedTreePlantInstance Plant { get; set; }
        public LODLevel CurrentLOD { get; set; }
        public int FramesSinceLastUpdate { get; set; }
        public float LastUpdateTime { get; set; }
        public float DistanceToCamera { get; set; }
    }
    
    /// <summary>
    /// Comprehensive optimizer statistics
    /// </summary>
    public class PlantOptimizerStats
    {
        public int ActivePlantCount { get; set; }
        public float CurrentFrameRate { get; set; }
        public float CurrentUpdateTime { get; set; }
        public float TargetFrameRate { get; set; }
        public float UpdateBudgetMs { get; set; }
        public OptimizationStrategy OptimizationStrategy { get; set; }
        public Dictionary<LODLevel, int> LODCounts { get; set; }
        public bool PerformanceWarningActive { get; set; }
        public float HighDetailDistance { get; set; }
        public float MediumDetailDistance { get; set; }
        public float LowDetailDistance { get; set; }
        
        public override string ToString()
        {
            return $"Plant Optimizer Stats:\n" +
                   $"  Active Plants: {ActivePlantCount}\n" +
                   $"  Frame Rate: {CurrentFrameRate:F1} FPS (Target: {TargetFrameRate})\n" +
                   $"  Update Time: {CurrentUpdateTime:F2}ms (Budget: {UpdateBudgetMs}ms)\n" +
                   $"  Strategy: {OptimizationStrategy}\n" +
                   $"  Performance Warning: {PerformanceWarningActive}\n" +
                   $"  LOD Distances: H{HighDetailDistance:F0}/M{MediumDetailDistance:F0}/L{LowDetailDistance:F0}";
        }
    }
    
    #endregion
}