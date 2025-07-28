using UnityEngine;
using ProjectChimera.Core;
using System.Collections;
using System.Collections.Generic;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// PC015: Simplified Performance Orchestrator - Avoids circular dependencies
    /// Provides basic performance monitoring without direct Systems assembly references
    /// </summary>
    public class SimplifiedPerformanceOrchestrator : ChimeraManager
    {
        [Header("Configuration")]
        [SerializeField] private PerformanceOptimizationConfigSO _config;
        
        [Header("Current Status")]
        [SerializeField] private bool _isOptimizationActive = false;
        [SerializeField] private float _currentFPS = 60f;
        [SerializeField] private int _currentPlantCount = 0;
        [SerializeField] private float _frameTimeBudgetUsed = 0f;
        
        // Performance tracking
        private Queue<float> _fpsHistory = new Queue<float>();
        private const int FPS_HISTORY_SIZE = 60; // 1 second at 60 FPS
        private float _lastOptimizationTime = 0f;
        private const float OPTIMIZATION_INTERVAL = 1f; // Check every second
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public properties for monitoring
        public float CurrentFPS => _currentFPS;
        public int CurrentPlantCount => _currentPlantCount;
        public bool IsPerformanceTargetMet => _config != null && _currentFPS >= _config.MinAcceptableFPS;
        public float PerformanceEfficiency => _config != null ? _currentFPS / _config.TargetFrameRate : 1.0f;
        
        protected override void OnManagerInitialize()
        {
            if (_config == null)
            {
                // Try to load default configuration from Resources
                _config = Resources.Load<PerformanceOptimizationConfigSO>("Config/DefaultPerformanceOptimizationConfig");
                if (_config == null)
                {
                    LogError("SimplifiedPerformanceOrchestrator: No configuration assigned and default config not found in Resources");
                    return;
                }
                LogInfo("SimplifiedPerformanceOrchestrator: Loaded default configuration from Resources");
            }
            
            if (!_config.ValidateConfiguration())
            {
                LogWarning("SimplifiedPerformanceOrchestrator: Configuration validation failed");
            }
            
            StartCoroutine(InitializeOptimizationSystems());
            LogInfo("SimplifiedPerformanceOrchestrator initialized for performance monitoring");
        }
        
        private IEnumerator InitializeOptimizationSystems()
        {
            LogInfo("PC015: Initializing simplified performance monitoring");
            
            // Wait for managers to be ready
            yield return new WaitUntil(() => GameManager.Instance != null);
            yield return new WaitForSeconds(0.5f);
            
            // Start performance monitoring
            StartPerformanceMonitoring();
            
            _isOptimizationActive = true;
            
            LogInfo("PC015: Simplified performance monitoring initialized and active");
        }
        
        private void StartPerformanceMonitoring()
        {
            StartCoroutine(MonitorPerformanceCoroutine());
        }
        
        private IEnumerator MonitorPerformanceCoroutine()
        {
            while (_isOptimizationActive)
            {
                // Update current metrics
                UpdateCurrentMetrics();
                
                // Check if optimization is needed
                if (Time.time - _lastOptimizationTime >= OPTIMIZATION_INTERVAL)
                {
                    OptimizePerformance();
                    _lastOptimizationTime = Time.time;
                }
                
                yield return new WaitForSeconds(0.1f); // Check 10 times per second
            }
        }
        
        private void UpdateCurrentMetrics()
        {
            // Update FPS
            _currentFPS = 1f / Time.unscaledDeltaTime;
            
            // Maintain FPS history
            _fpsHistory.Enqueue(_currentFPS);
            if (_fpsHistory.Count > FPS_HISTORY_SIZE)
                _fpsHistory.Dequeue();
            
            // Update frame budget usage
            if (_config != null)
            {
                _frameTimeBudgetUsed = Time.unscaledDeltaTime / (_config.FrameTimeBudgetMS / 1000f);
            }
        }
        
        private void OptimizePerformance()
        {
            if (_config == null) return;
            
            float avgFPS = GetAverageFPS();
            
            // Determine if optimization is needed
            bool needsOptimization = avgFPS < _config.MinAcceptableFPS || _frameTimeBudgetUsed > 1.1f;
            
            if (needsOptimization && _config.LogPerformanceWarnings)
            {
                LogWarning($"PC015: Performance below target - {avgFPS:F1} FPS (target: {_config.TargetFrameRate})");
            }
            
            // Future: Send events to other systems for optimization
            // This avoids circular dependencies while allowing system coordination
        }
        
        private float GetAverageFPS()
        {
            if (_fpsHistory.Count == 0) return _currentFPS;
            
            float sum = 0f;
            foreach (float fps in _fpsHistory)
                sum += fps;
            
            return sum / _fpsHistory.Count;
        }
        
        protected override void OnManagerUpdate()
        {
            // Lightweight update for monitoring
            if (_config != null && _config.EnableDebugOverlays)
            {
                // Debug overlays would be handled here
            }
        }
        
        protected override void OnManagerShutdown()
        {
            _isOptimizationActive = false;
            LogInfo("SimplifiedPerformanceOrchestrator shutdown complete");
        }
        
        #region Public API
        
        /// <summary>
        /// Force a performance optimization pass
        /// </summary>
        public void ForceOptimization()
        {
            OptimizePerformance();
            LogInfo("PC015: Forced performance optimization completed");
        }
        
        /// <summary>
        /// Get current performance status
        /// </summary>
        public PerformanceStatus GetPerformanceStatus()
        {
            return new PerformanceStatus
            {
                CurrentFPS = _currentFPS,
                AverageFPS = GetAverageFPS(),
                PlantCount = _currentPlantCount,
                QualityLevel = 0, // Simplified version
                IsTargetMet = IsPerformanceTargetMet,
                FrameBudgetUsed = _frameTimeBudgetUsed,
                OptimizationSystemsActive = new Dictionary<string, bool>
                {
                    {"SimplifiedMonitoring", _isOptimizationActive}
                }
            };
        }
        
        #endregion
    }
    
}