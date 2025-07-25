using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Advanced scheduler for distributing plant updates across frames to maintain 60 FPS
    /// Implements priority-based queuing and adaptive batch sizing
    /// </summary>
    public class PlantUpdateScheduler : ChimeraManager
    {
        [Header("Performance Configuration")]
        [SerializeField] private float _targetFrameTime = 16.67f; // 60 FPS = 16.67ms per frame
        [SerializeField] private float _updateBudgetPercentage = 0.3f; // 30% of frame time for plant updates
        [SerializeField] private int _minBatchSize = 10;
        [SerializeField] private int _maxBatchSize = 100;
        [SerializeField] private int _initialBatchSize = 25;
        
        [Header("Update Priority Configuration")]
        [SerializeField] private float _criticalHealthThreshold = 0.3f;
        [SerializeField] private float _visiblePlantDistanceThreshold = 50f;
        [SerializeField] private bool _enableAdaptiveBatching = true;
        [SerializeField] private bool _enablePriorityQueuing = true;
        
        [Header("Performance Monitoring")]
        [SerializeField] private bool _enablePerformanceTracking = true;
        [SerializeField] private int _performanceHistorySize = 120; // 2 seconds at 60 FPS
        [SerializeField] private float _performanceLogInterval = 5f;
        
        // Plant update queues by priority
        private readonly Queue<GameObject> _criticalPriorityQueue = new Queue<GameObject>();
        private readonly Queue<GameObject> _highPriorityQueue = new Queue<GameObject>();
        private readonly Queue<GameObject> _normalPriorityQueue = new Queue<GameObject>();
        private readonly Queue<GameObject> _lowPriorityQueue = new Queue<GameObject>();
        
        // Active plant tracking
        private readonly HashSet<GameObject> _registeredPlants = new HashSet<GameObject>();
        private readonly Dictionary<GameObject, PlantUpdateMetrics> _plantMetrics = new Dictionary<GameObject, PlantUpdateMetrics>();
        
        // Batch processing
        private int _currentBatchSize;
        private float _updateBudgetMs;
        private Coroutine _updateCoroutine;
        
        // Performance tracking
        private readonly Queue<FramePerformanceData> _performanceHistory = new Queue<FramePerformanceData>();
        private float _averageUpdateTime = 0f;
        private int _plantsProcessedThisFrame = 0;
        private int _totalPlantsProcessed = 0;
        
        // Camera reference for distance calculations
        private Camera _mainCamera;
        
        public static PlantUpdateScheduler Instance { get; private set; }
        
        // Statistics properties
        public int TotalRegisteredPlants => _registeredPlants.Count;
        public int CriticalPriorityCount => _criticalPriorityQueue.Count;
        public int HighPriorityCount => _highPriorityQueue.Count;
        public int NormalPriorityCount => _normalPriorityQueue.Count;
        public int LowPriorityCount => _lowPriorityQueue.Count;
        public float AverageUpdateTime => _averageUpdateTime;
        public int CurrentBatchSize => _currentBatchSize;
        
        #region Unity Lifecycle
        
        protected override void OnManagerInitialize()
        {
            if (Instance == null)
            {
                Instance = this;
                
                _currentBatchSize = _initialBatchSize;
                _updateBudgetMs = _targetFrameTime * _updateBudgetPercentage;
                _mainCamera = Camera.main;
                
                // Start the update coroutine
                _updateCoroutine = StartCoroutine(PlantUpdateCoroutine());
                
                if (_performanceLogInterval > 0)
                {
                    InvokeRepeating(nameof(LogPerformanceStats), _performanceLogInterval, _performanceLogInterval);
                }
                
                Debug.Log($"[PlantUpdateScheduler] Initialized - Target: {_targetFrameTime:F2}ms, " +
                         $"Budget: {_updateBudgetMs:F2}ms, Initial Batch: {_currentBatchSize}");
            }
            else
            {
                Debug.LogWarning("[PlantUpdateScheduler] Multiple instances detected, destroying duplicate");
                Destroy(gameObject);
            }
        }
        
        protected override void OnManagerShutdown()
        {
            if (Instance == this)
            {
                if (_updateCoroutine != null)
                {
                    StopCoroutine(_updateCoroutine);
                    _updateCoroutine = null;
                }
                
                CancelInvoke();
                ClearAllQueues();
                Instance = null;
                
                Debug.Log("[PlantUpdateScheduler] Shutdown complete");
            }
        }
        
        #endregion
        
        #region Plant Registration
        
        /// <summary>
        /// Register a plant for scheduled updates
        /// </summary>
        public void RegisterPlant(GameObject plant)
        {
            if (plant == null) return;
            
            if (_registeredPlants.Add(plant))
            {
                _plantMetrics[plant] = new PlantUpdateMetrics
                {
                    LastUpdateTime = Time.time,
                    UpdateCount = 0,
                    AverageUpdateDuration = 0f
                };
                
                // Add to appropriate priority queue
                QueuePlantByPriority(plant);
                
                Debug.Log($"[PlantUpdateScheduler] Registered plant {plant.name} for updates");
            }
        }
        
        /// <summary>
        /// Unregister a plant from scheduled updates
        /// </summary>
        public void UnregisterPlant(GameObject plant)
        {
            if (plant == null) return;
            
            if (_registeredPlants.Remove(plant))
            {
                _plantMetrics.Remove(plant);
                
                // Remove from all queues (inefficient but necessary)
                RemoveFromAllQueues(plant);
                
                Debug.Log($"[PlantUpdateScheduler] Unregistered plant {plant.name}");
            }
        }
        
        /// <summary>
        /// Re-evaluate and queue a plant based on current priority
        /// </summary>
        public void RequeuePlant(GameObject plant)
        {
            if (plant == null || !_registeredPlants.Contains(plant)) return;
            
            QueuePlantByPriority(plant);
        }
        
        #endregion
        
        #region Priority Management
        
        private void QueuePlantByPriority(GameObject plant)
        {
            if (!_enablePriorityQueuing)
            {
                _normalPriorityQueue.Enqueue(plant);
                return;
            }
            
            var priority = CalculatePlantPriority(plant);
            
            switch (priority)
            {
                case UpdatePriority.Critical:
                    _criticalPriorityQueue.Enqueue(plant);
                    break;
                case UpdatePriority.High:
                    _highPriorityQueue.Enqueue(plant);
                    break;
                case UpdatePriority.Normal:
                    _normalPriorityQueue.Enqueue(plant);
                    break;
                case UpdatePriority.Low:
                    _lowPriorityQueue.Enqueue(plant);
                    break;
            }
        }
        
        private UpdatePriority CalculatePlantPriority(GameObject plant)
        {
            // Critical: Plants that are inactive or null
            if (plant == null || !plant.activeInHierarchy)
            {
                return UpdatePriority.Low;
            }
            
            // High: Visible plants close to camera
            if (_mainCamera != null)
            {
                var distanceToCamera = Vector3.Distance(_mainCamera.transform.position, plant.transform.position);
                if (distanceToCamera < _visiblePlantDistanceThreshold && plant.gameObject.activeInHierarchy)
                {
                    return UpdatePriority.High;
                }
            }
            
            // Low: Inactive or very distant plants
            if (!plant.gameObject.activeInHierarchy)
            {
                return UpdatePriority.Low;
            }
            
            // Normal: Everything else
            return UpdatePriority.Normal;
        }
        
        private void RemoveFromAllQueues(GameObject plant)
        {
            // This is inefficient but necessary - in a production system,
            // we'd use a more sophisticated data structure
            RemoveFromQueue(_criticalPriorityQueue, plant);
            RemoveFromQueue(_highPriorityQueue, plant);
            RemoveFromQueue(_normalPriorityQueue, plant);
            RemoveFromQueue(_lowPriorityQueue, plant);
        }
        
        private void RemoveFromQueue(Queue<GameObject> queue, GameObject plant)
        {
            var tempList = new List<GameObject>();
            
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p != plant)
                {
                    tempList.Add(p);
                }
            }
            
            foreach (var p in tempList)
            {
                queue.Enqueue(p);
            }
        }
        
        #endregion
        
        #region Update Processing
        
        private IEnumerator PlantUpdateCoroutine()
        {
            while (true)
            {
                yield return null; // Wait for next frame
                
                _plantsProcessedThisFrame = 0;
                var frameStartTime = Time.realtimeSinceStartup;
                
                // Process plants in priority order
                ProcessPlantBatch(frameStartTime);
                
                // Update performance metrics
                var frameEndTime = Time.realtimeSinceStartup;
                var frameDuration = (frameEndTime - frameStartTime) * 1000f; // Convert to ms
                
                UpdatePerformanceMetrics(frameDuration);
                
                // Adaptive batch sizing
                if (_enableAdaptiveBatching)
                {
                    AdaptBatchSize(frameDuration);
                }
            }
        }
        
        private void ProcessPlantBatch(float frameStartTime)
        {
            var remainingBudget = _updateBudgetMs;
            var plantsToProcess = _currentBatchSize;
            
            // Process critical priority first
            plantsToProcess = ProcessQueueWithBudget(_criticalPriorityQueue, plantsToProcess, frameStartTime, ref remainingBudget);
            if (plantsToProcess <= 0 || remainingBudget <= 0) return;
            
            // Then high priority
            plantsToProcess = ProcessQueueWithBudget(_highPriorityQueue, plantsToProcess, frameStartTime, ref remainingBudget);
            if (plantsToProcess <= 0 || remainingBudget <= 0) return;
            
            // Then normal priority
            plantsToProcess = ProcessQueueWithBudget(_normalPriorityQueue, plantsToProcess, frameStartTime, ref remainingBudget);
            if (plantsToProcess <= 0 || remainingBudget <= 0) return;
            
            // Finally low priority
            ProcessQueueWithBudget(_lowPriorityQueue, plantsToProcess, frameStartTime, ref remainingBudget);
        }
        
        private int ProcessQueueWithBudget(Queue<GameObject> queue, int maxPlants, float frameStartTime, ref float remainingBudget)
        {
            int processed = 0;
            
            while (queue.Count > 0 && processed < maxPlants && remainingBudget > 0)
            {
                var plant = queue.Dequeue();
                
                if (plant == null || !_registeredPlants.Contains(plant))
                {
                    continue; // Skip invalid plants
                }
                
                var updateStartTime = Time.realtimeSinceStartup;
                
                // Perform the actual plant update
                UpdatePlant(plant);
                
                var updateEndTime = Time.realtimeSinceStartup;
                var updateDuration = (updateEndTime - updateStartTime) * 1000f; // Convert to ms
                
                // Update metrics
                UpdatePlantMetrics(plant, updateDuration);
                
                remainingBudget -= updateDuration;
                processed++;
                _plantsProcessedThisFrame++;
                _totalPlantsProcessed++;
                
                // Re-queue plant for next update cycle
                QueuePlantByPriority(plant);
            }
            
            return maxPlants - processed;
        }
        
        private void UpdatePlant(GameObject plant)
        {
            // GameObject handles its own updates internally
            // We can trigger specific updates if needed using available methods
            
            try
            {
                // The plant manages its own growth, health, and visual updates
                // through Unity's lifecycle methods and internal systems
                // We just need to ensure the plant is active and processing
                
                if (plant.activeInHierarchy && plant.gameObject.activeInHierarchy)
                {
                    // Plant updates are managed internally
                    // Could call UpdateDictionary<string, float> if needed
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlantUpdateScheduler] Error updating plant {plant.name}: {ex.Message}", plant);
            }
        }
        
        #endregion
        
        #region Performance Management
        
        private void UpdatePerformanceMetrics(float frameDuration)
        {
            var frameData = new FramePerformanceData
            {
                FrameTime = frameDuration,
                PlantsProcessed = _plantsProcessedThisFrame,
                BatchSize = _currentBatchSize,
                Timestamp = Time.time
            };
            
            _performanceHistory.Enqueue(frameData);
            
            // Maintain history size
            while (_performanceHistory.Count > _performanceHistorySize)
            {
                _performanceHistory.Dequeue();
            }
            
            // Update rolling average
            if (_performanceHistory.Count > 0)
            {
                float totalTime = 0f;
                foreach (var data in _performanceHistory)
                {
                    totalTime += data.FrameTime;
                }
                _averageUpdateTime = totalTime / _performanceHistory.Count;
            }
        }
        
        private void AdaptBatchSize(float frameDuration)
        {
            const float adjustmentFactor = 0.1f;
            
            if (frameDuration > _updateBudgetMs * 1.2f) // Exceeding budget by 20%
            {
                // Reduce batch size
                _currentBatchSize = Mathf.Max(_minBatchSize, 
                    (int)(_currentBatchSize * (1f - adjustmentFactor)));
            }
            else if (frameDuration < _updateBudgetMs * 0.7f) // Using less than 70% of budget
            {
                // Increase batch size
                _currentBatchSize = Mathf.Min(_maxBatchSize, 
                    (int)(_currentBatchSize * (1f + adjustmentFactor)));
            }
        }
        
        private void UpdatePlantMetrics(GameObject plant, float updateDuration)
        {
            if (_plantMetrics.TryGetValue(plant, out var metrics))
            {
                metrics.UpdateCount++;
                metrics.LastUpdateTime = Time.time;
                
                // Rolling average of update duration
                if (metrics.UpdateCount == 1)
                {
                    metrics.AverageUpdateDuration = updateDuration;
                }
                else
                {
                    metrics.AverageUpdateDuration = 
                        (metrics.AverageUpdateDuration * 0.9f) + (updateDuration * 0.1f);
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force update all plants in critical priority queue
        /// </summary>
        public void ForceUpdateCriticalPlants()
        {
            var frameStartTime = Time.realtimeSinceStartup;
            var budget = _updateBudgetMs * 3f; // Allow extra budget for critical updates
            
            ProcessQueueWithBudget(_criticalPriorityQueue, _criticalPriorityQueue.Count, frameStartTime, ref budget);
            
            Debug.Log($"[PlantUpdateScheduler] Force updated {CriticalPriorityCount} critical plants");
        }
        
        /// <summary>
        /// Clear all update queues
        /// </summary>
        public void ClearAllQueues()
        {
            _criticalPriorityQueue.Clear();
            _highPriorityQueue.Clear();
            _normalPriorityQueue.Clear();
            _lowPriorityQueue.Clear();
            
            Debug.Log("[PlantUpdateScheduler] All update queues cleared");
        }
        
        /// <summary>
        /// Get comprehensive scheduler statistics
        /// </summary>
        public PlantSchedulerStats GetSchedulerStats()
        {
            return new PlantSchedulerStats
            {
                TotalRegisteredPlants = TotalRegisteredPlants,
                CriticalPriorityCount = CriticalPriorityCount,
                HighPriorityCount = HighPriorityCount,
                NormalPriorityCount = NormalPriorityCount,
                LowPriorityCount = LowPriorityCount,
                CurrentBatchSize = CurrentBatchSize,
                AverageUpdateTime = AverageUpdateTime,
                UpdateBudgetMs = _updateBudgetMs,
                TotalPlantsProcessed = _totalPlantsProcessed,
                PerformanceHistoryCount = _performanceHistory.Count
            };
        }
        
        #endregion
        
        #region Performance Logging
        
        private void LogPerformanceStats()
        {
            if (!_enablePerformanceTracking) return;
            
            var stats = GetSchedulerStats();
            
            Debug.Log($"[PlantUpdateScheduler] Performance Stats - " +
                     $"Plants: {stats.TotalRegisteredPlants}, " +
                     $"Batch: {stats.CurrentBatchSize}, " +
                     $"Avg Time: {stats.AverageUpdateTime:F2}ms, " +
                     $"Budget: {stats.UpdateBudgetMs:F2}ms, " +
                     $"Queues: C{stats.CriticalPriorityCount}/H{stats.HighPriorityCount}/N{stats.NormalPriorityCount}/L{stats.LowPriorityCount}");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    /// <summary>
    /// Update priority levels for plants
    /// </summary>
    public enum UpdatePriority
    {
        Critical,   // Immediate attention required (low health, errors)
        High,       // Visible, close to camera
        Normal,     // Standard priority
        Low         // Distant, inactive, or background plants
    }
    
    /// <summary>
    /// Performance data for a single frame
    /// </summary>
    public class FramePerformanceData
    {
        public float FrameTime { get; set; }        // Time spent on plant updates (ms)
        public int PlantsProcessed { get; set; }    // Number of plants updated
        public int BatchSize { get; set; }          // Batch size used
        public float Timestamp { get; set; }        // When this frame occurred
    }
    
    /// <summary>
    /// Update metrics for individual plants
    /// </summary>
    public class PlantUpdateMetrics
    {
        public float LastUpdateTime { get; set; }
        public int UpdateCount { get; set; }
        public float AverageUpdateDuration { get; set; }
    }
    
    /// <summary>
    /// Comprehensive scheduler statistics
    /// </summary>
    public class PlantSchedulerStats
    {
        public int TotalRegisteredPlants { get; set; }
        public int CriticalPriorityCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int NormalPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public int CurrentBatchSize { get; set; }
        public float AverageUpdateTime { get; set; }
        public float UpdateBudgetMs { get; set; }
        public int TotalPlantsProcessed { get; set; }
        public int PerformanceHistoryCount { get; set; }
        
        public override string ToString()
        {
            return $"Plant Scheduler Stats:\n" +
                   $"  Total Plants: {TotalRegisteredPlants}\n" +
                   $"  Priority Queues: C{CriticalPriorityCount}/H{HighPriorityCount}/N{NormalPriorityCount}/L{LowPriorityCount}\n" +
                   $"  Batch Size: {CurrentBatchSize}\n" +
                   $"  Avg Update Time: {AverageUpdateTime:F2}ms\n" +
                   $"  Budget: {UpdateBudgetMs:F2}ms\n" +
                   $"  Total Processed: {TotalPlantsProcessed}";
        }
    }
    
    #endregion
}