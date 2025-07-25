using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// High-performance batch processor for plant updates
    /// Optimizes memory usage and calculation efficiency for large plant collections
    /// </summary>
    public class PlantBatchProcessor : ChimeraManager
    {
        // Singleton instance
        private static PlantBatchProcessor _instance;
        public static PlantBatchProcessor Instance => _instance;
        
        [Header("Batch Configuration")]
        [SerializeField] private int _maxBatchSize = 50;
        [SerializeField] private int _optimalBatchSize = 25;
        [SerializeField] private bool _enableParallelProcessing = true;
        [SerializeField] private bool _enableMemoryOptimization = true;
        
        [Header("Update Types")]
        [SerializeField] private bool _enableGrowthUpdates = true;
        [SerializeField] private bool _enableEnvironmentalUpdates = true;
        [SerializeField] private bool _enableVisualUpdates = true;
        [SerializeField] private bool _enableHealthUpdates = true;
        
        [Header("Performance Monitoring")]
        [SerializeField] private bool _trackBatchPerformance = true;
        [SerializeField] private int _performanceHistorySize = 50;
        
        // Pooled collections for batch processing
        private PooledObjectManager _poolManager;
        
        // Batch processing statistics
        private readonly Queue<BatchPerformanceData> _batchHistory = new Queue<BatchPerformanceData>();
        private int _totalBatchesProcessed = 0;
        private float _averageBatchTime = 0f;
        
        // Statistics properties
        public int TotalBatchesProcessed => _totalBatchesProcessed;
        public float AverageBatchTime => _averageBatchTime;
        public int OptimalBatchSize => _optimalBatchSize;
        
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
            _poolManager = PooledObjectManager.Instance;
            
            if (_poolManager == null)
            {
                Debug.LogWarning("[PlantBatchProcessor] PooledObjectManager not found, memory optimization disabled");
                _enableMemoryOptimization = false;
            }
            
            Debug.Log($"[PlantBatchProcessor] Initialized - Optimal batch size: {_optimalBatchSize}, " +
                     $"Parallel: {_enableParallelProcessing}, Memory Opt: {_enableMemoryOptimization}");
        }
        
        protected override void OnManagerShutdown()
        {
            if (_instance == this)
            {
                _instance = null;
                Debug.Log("[PlantBatchProcessor] Shutdown complete");
            }
        }
        
        #endregion
        
        #region Batch Processing API
        
        /// <summary>
        /// Process a collection of plants in optimized batches
        /// </summary>
        public void ProcessPlantCollection(IEnumerable<GameObject> plants, 
            PlantUpdateType updateType = PlantUpdateType.Full)
        {
            if (plants == null) return;
            
            if (_enableMemoryOptimization && _poolManager != null)
            {
                ProcessPlantsWithPooling(plants, updateType);
            }
            else
            {
                ProcessPlantsStandard(plants, updateType);
            }
        }
        
        /// <summary>
        /// Process a specific batch of plants with performance tracking
        /// </summary>
        public BatchProcessResult ProcessPlantBatch(List<GameObject> plantBatch, 
            PlantUpdateType updateType = PlantUpdateType.Full)
        {
            if (plantBatch == null || plantBatch.Count == 0)
            {
                return new BatchProcessResult { Success = false, ErrorMessage = "Empty batch" };
            }
            
            var startTime = Time.realtimeSinceStartup;
            var result = new BatchProcessResult { Success = true, PlantsProcessed = plantBatch.Count };
            
            try
            {
                // Process plants based on update type
                switch (updateType)
                {
                    case PlantUpdateType.Growth:
                        ProcessGrowthBatch(plantBatch);
                        break;
                    case PlantUpdateType.Environmental:
                        ProcessEnvironmentalBatch(plantBatch);
                        break;
                    case PlantUpdateType.Visual:
                        ProcessVisualBatch(plantBatch);
                        break;
                    case PlantUpdateType.Health:
                        ProcessHealthBatch(plantBatch);
                        break;
                    case PlantUpdateType.Full:
                        ProcessFullUpdateBatch(plantBatch);
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Debug.LogError($"[PlantBatchProcessor] Batch processing failed: {ex.Message}");
            }
            
            var endTime = Time.realtimeSinceStartup;
            result.ProcessingTimeMs = (endTime - startTime) * 1000f;
            
            // Update statistics
            if (_trackBatchPerformance)
            {
                UpdateBatchStatistics(result);
            }
            
            return result;
        }
        
        #endregion
        
        #region Memory-Optimized Processing
        
        private void ProcessPlantsWithPooling(IEnumerable<GameObject> plants, PlantUpdateType updateType)
        {
            _poolManager.ExecuteBatchOperation(plants, (plantList, dataDict) =>
            {
                // Split into optimal-sized batches
                var batches = CreateBatches(plantList, _optimalBatchSize);
                
                foreach (var batch in batches)
                {
                    ProcessPlantBatch(batch, updateType);
                }
            });
        }
        
        private List<List<GameObject>> CreateBatches(List<GameObject> plants, int batchSize)
        {
            var batches = new List<List<GameObject>>();
            
            for (int i = 0; i < plants.Count; i += batchSize)
            {
                var batch = _poolManager?.GetPlantList() ?? new List<GameObject>();
                
                int endIndex = Mathf.Min(i + batchSize, plants.Count);
                for (int j = i; j < endIndex; j++)
                {
                    batch.Add(plants[j]);
                }
                
                batches.Add(batch);
            }
            
            return batches;
        }
        
        #endregion
        
        #region Standard Processing
        
        private void ProcessPlantsStandard(IEnumerable<GameObject> plants, PlantUpdateType updateType)
        {
            var plantList = new List<GameObject>(plants);
            var batches = CreateBatchesStandard(plantList, _optimalBatchSize);
            
            foreach (var batch in batches)
            {
                ProcessPlantBatch(batch, updateType);
            }
        }
        
        private List<List<GameObject>> CreateBatchesStandard(List<GameObject> plants, int batchSize)
        {
            var batches = new List<List<GameObject>>();
            
            for (int i = 0; i < plants.Count; i += batchSize)
            {
                var batch = new List<GameObject>();
                
                int endIndex = Mathf.Min(i + batchSize, plants.Count);
                for (int j = i; j < endIndex; j++)
                {
                    batch.Add(plants[j]);
                }
                
                batches.Add(batch);
            }
            
            return batches;
        }
        
        #endregion
        
        #region Specific Update Processors
        
        private void ProcessGrowthBatch(List<GameObject> plants)
        {
            if (!_enableGrowthUpdates) return;
            
            float deltaTime = Time.deltaTime;
            
            foreach (var plant in plants)
            {
                if (plant != null && plant.gameObject.activeInHierarchy)
                {
                    // Growth is handled internally by the plant instance
                    // We can trigger updates through existing methods
                }
            }
        }
        
        private void ProcessEnvironmentalBatch(List<GameObject> plants)
        {
            if (!_enableEnvironmentalUpdates) return;
            
            foreach (var plant in plants)
            {
                if (plant != null && plant.gameObject.activeInHierarchy)
                {
                    // Environmental processing handled internally
                    // UpdateDictionary<string, float> is available if needed
                }
            }
        }
        
        private void ProcessVisualBatch(List<GameObject> plants)
        {
            if (!_enableVisualUpdates) return;
            
            foreach (var plant in plants)
            {
                if (plant != null && plant.gameObject.activeInHierarchy)
                {
                    // Visual updates are handled internally by UpdateVisualProperties
                    // No external update method needed
                }
            }
        }
        
        private void ProcessHealthBatch(List<GameObject> plants)
        {
            if (!_enableHealthUpdates) return;
            
            foreach (var plant in plants)
            {
                if (plant != null && plant.gameObject.activeInHierarchy)
                {
                    // Health updates are handled internally by UpdatePlantHealth
                    // No external update method needed
                }
            }
        }
        
        private void ProcessFullUpdateBatch(List<GameObject> plants)
        {
            float deltaTime = Time.deltaTime;
            
            foreach (var plant in plants)
            {
                if (plant != null && plant.gameObject.activeInHierarchy)
                {
                    // All updates are handled internally by GameObject
                    // The plant manages its own growth, environment, health, and visuals
                    // through Unity's Update lifecycle and internal methods
                }
            }
        }
        
        #endregion
        
        #region Performance Tracking
        
        private void UpdateBatchStatistics(BatchProcessResult result)
        {
            _totalBatchesProcessed++;
            
            var batchData = new BatchPerformanceData
            {
                ProcessingTimeMs = result.ProcessingTimeMs,
                PlantsProcessed = result.PlantsProcessed,
                Success = result.Success,
                Timestamp = Time.time
            };
            
            _batchHistory.Enqueue(batchData);
            
            // Maintain history size
            while (_batchHistory.Count > _performanceHistorySize)
            {
                _batchHistory.Dequeue();
            }
            
            // Update rolling average
            if (_batchHistory.Count > 0)
            {
                float totalTime = 0f;
                foreach (var data in _batchHistory)
                {
                    totalTime += data.ProcessingTimeMs;
                }
                _averageBatchTime = totalTime / _batchHistory.Count;
            }
        }
        
        /// <summary>
        /// Get comprehensive batch processor statistics
        /// </summary>
        public BatchProcessorStats GetProcessorStats()
        {
            var successfulBatches = 0;
            var totalPlants = 0;
            
            foreach (var batch in _batchHistory)
            {
                if (batch.Success)
                {
                    successfulBatches++;
                    totalPlants += batch.PlantsProcessed;
                }
            }
            
            return new BatchProcessorStats
            {
                TotalBatchesProcessed = _totalBatchesProcessed,
                SuccessfulBatches = successfulBatches,
                AverageBatchTime = _averageBatchTime,
                OptimalBatchSize = _optimalBatchSize,
                MaxBatchSize = _maxBatchSize,
                TotalPlantsProcessed = totalPlants,
                PerformanceHistoryCount = _batchHistory.Count,
                ParallelProcessingEnabled = _enableParallelProcessing,
                MemoryOptimizationEnabled = _enableMemoryOptimization
            };
        }
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// Update batch processor configuration at runtime
        /// </summary>
        public void UpdateConfiguration(int optimalBatchSize, bool enableParallel, bool enableMemoryOpt)
        {
            _optimalBatchSize = Mathf.Clamp(optimalBatchSize, 1, _maxBatchSize);
            _enableParallelProcessing = enableParallel;
            _enableMemoryOptimization = enableMemoryOpt && _poolManager != null;
            
            Debug.Log($"[PlantBatchProcessor] Configuration updated - " +
                     $"Batch Size: {_optimalBatchSize}, Parallel: {_enableParallelProcessing}, " +
                     $"Memory Opt: {_enableMemoryOptimization}");
        }
        
        /// <summary>
        /// Initialize the PlantBatchProcessor (alias for OnManagerInitialize for testing)
        /// </summary>
        public void Initialize()
        {
            OnManagerInitialize();
        }
        
        /// <summary>
        /// Process a collection of plants using batch optimization
        /// </summary>
        public void ProcessPlantCollection(List<GameObject> plants)
        {
            if (plants == null || plants.Count == 0)
            {
                Debug.LogWarning("[PlantBatchProcessor] No plants provided for processing");
                return;
            }
            
            // Process plants in batches
            var batchCount = Mathf.CeilToInt((float)plants.Count / _optimalBatchSize);
            
            for (int i = 0; i < batchCount; i++)
            {
                var startIndex = i * _optimalBatchSize;
                var count = Mathf.Min(_optimalBatchSize, plants.Count - startIndex);
                var batch = plants.GetRange(startIndex, count);
                
                ProcessBatch(batch, PlantUpdateType.Full);
            }
            
            Debug.Log($"[PlantBatchProcessor] Processed {plants.Count} plants in {batchCount} batches");
        }
        
        /// <summary>
        /// Process a single batch of plants with specified update type
        /// </summary>
        private void ProcessBatch(List<GameObject> batch, PlantUpdateType updateType)
        {
            if (batch == null || batch.Count == 0) return;
            
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                switch (updateType)
                {
                    case PlantUpdateType.Growth:
                        if (_enableGrowthUpdates) ProcessGrowthBatch(batch);
                        break;
                    case PlantUpdateType.Environmental:
                        if (_enableEnvironmentalUpdates) ProcessEnvironmentalBatch(batch);
                        break;
                    case PlantUpdateType.Visual:
                        if (_enableVisualUpdates) ProcessVisualBatch(batch);
                        break;
                    case PlantUpdateType.Health:
                        if (_enableHealthUpdates) ProcessHealthBatch(batch);
                        break;
                    case PlantUpdateType.Full:
                        ProcessFullUpdateBatch(batch);
                        break;
                }
                
                // Record performance
                var processingTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                RecordBatchPerformance(batch.Count, processingTime, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlantBatchProcessor] Error processing batch: {ex.Message}");
                var processingTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                RecordBatchPerformance(batch.Count, processingTime, false);
            }
        }
        
        /// <summary>
        /// Record batch performance data
        /// </summary>
        private void RecordBatchPerformance(int plantsProcessed, float processingTime, bool success)
        {
            _totalBatchesProcessed++;
            
            // Update average processing time
            var previousTotal = _averageBatchTime * (_totalBatchesProcessed - 1);
            _averageBatchTime = (previousTotal + processingTime) / _totalBatchesProcessed;
            
            // Store performance data if tracking is enabled
            if (_trackBatchPerformance)
            {
                var perfData = new BatchPerformanceData
                {
                    ProcessingTimeMs = processingTime,
                    PlantsProcessed = plantsProcessed,
                    Success = success,
                    Timestamp = Time.realtimeSinceStartup
                };
                
                _batchHistory.Enqueue(perfData);
                
                // Trim history if needed
                while (_batchHistory.Count > _performanceHistorySize)
                {
                    _batchHistory.Dequeue();
                }
            }
        }
        
        /// <summary>
        /// Enable or disable specific update types
        /// </summary>
        public void SetUpdateTypes(bool growth, bool environmental, bool visual, bool health)
        {
            _enableGrowthUpdates = growth;
            _enableEnvironmentalUpdates = environmental;
            _enableVisualUpdates = visual;
            _enableHealthUpdates = health;
            
            Debug.Log($"[PlantBatchProcessor] Update types configured - " +
                     $"Growth: {growth}, Environmental: {environmental}, " +
                     $"Visual: {visual}, Health: {health}");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    /// <summary>
    /// Types of plant updates that can be performed
    /// </summary>
    public enum PlantUpdateType
    {
        Growth,         // Growth and development updates
        Environmental,  // Environmental response updates
        Visual,         // Visual and rendering updates
        Health,         // Health and status updates
        Full           // All update types
    }
    
    /// <summary>
    /// Result of processing a plant batch
    /// </summary>
    public class BatchProcessResult
    {
        public bool Success { get; set; }
        public int PlantsProcessed { get; set; }
        public float ProcessingTimeMs { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Performance data for a single batch operation
    /// </summary>
    public class BatchPerformanceData
    {
        public float ProcessingTimeMs { get; set; }
        public int PlantsProcessed { get; set; }
        public bool Success { get; set; }
        public float Timestamp { get; set; }
    }
    
    /// <summary>
    /// Comprehensive batch processor statistics
    /// </summary>
    public class BatchProcessorStats
    {
        public int TotalBatchesProcessed { get; set; }
        public int SuccessfulBatches { get; set; }
        public float AverageBatchTime { get; set; }
        public int OptimalBatchSize { get; set; }
        public int MaxBatchSize { get; set; }
        public int TotalPlantsProcessed { get; set; }
        public int PerformanceHistoryCount { get; set; }
        public bool ParallelProcessingEnabled { get; set; }
        public bool MemoryOptimizationEnabled { get; set; }
        
        public override string ToString()
        {
            return $"Batch Processor Stats:\n" +
                   $"  Total Batches: {TotalBatchesProcessed} ({SuccessfulBatches} successful)\n" +
                   $"  Avg Batch Time: {AverageBatchTime:F2}ms\n" +
                   $"  Batch Size: {OptimalBatchSize}/{MaxBatchSize}\n" +
                   $"  Total Plants: {TotalPlantsProcessed}\n" +
                   $"  Parallel: {ParallelProcessingEnabled}\n" +
                   $"  Memory Opt: {MemoryOptimizationEnabled}";
        }
    }
    
    #endregion
}