using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Performance Management Orchestrator - Central coordinator for all performance services
    /// Coordinates PerformanceMonitoringService, PerformanceOptimizationService, PerformanceAlertService, and PerformanceAnalyticsService
    /// Provides unified interface for performance management and ensures proper service integration
    /// </summary>
    public class PerformanceManagementOrchestrator : MonoBehaviour
    {
        [Header("Service References")]
        [SerializeField] private PerformanceMonitoringService _monitoringService;
        [SerializeField] private PerformanceOptimizationService _optimizationService;
        [SerializeField] private PerformanceAlertService _alertService;
        [SerializeField] private PerformanceAnalyticsService _analyticsService;

        [Header("Orchestration Configuration")]
        [SerializeField] private bool _enableAutoInitialization = true;
        [SerializeField] private bool _enableServiceCoordination = true;
        [SerializeField] private float _coordinationInterval = 1f;
        [SerializeField] private bool _enablePerformanceReporting = true;

        [Header("Performance Targets")]
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _memoryWarningThresholdMB = 1024f;
        [SerializeField] private float _memoryCriticalThresholdMB = 2048f;

        // Orchestration state
        private bool _isInitialized = false;
        private bool _servicesInitialized = false;
        private float _lastCoordinationTime = 0f;
        private Dictionary<string, bool> _serviceStates = new Dictionary<string, bool>();

        // Performance data coordination
        private GlobalPerformanceMetrics _lastGlobalMetrics;
        private Dictionary<string, PerformanceProfile> _lastSystemProfiles = new Dictionary<string, PerformanceProfile>();

        // Events for orchestration notifications
        public static event System.Action<PerformanceOrchestrationStatus> OnOrchestrationStatusChanged;
        public static event System.Action<ServiceCoordinationReport> OnCoordinationCompleted;
        public static event System.Action<string> OnServiceError;

        #region Orchestrator Implementation

        public bool IsInitialized => _isInitialized;
        public string OrchestratorName => "Performance Management Orchestrator";
        public bool ServicesInitialized => _servicesInitialized;
        public IReadOnlyDictionary<string, bool> ServiceStates => _serviceStates;

        public void Initialize()
        {
            InitializeOrchestrator();
        }

        public void Shutdown()
        {
            ShutdownOrchestrator();
        }

        #endregion

        #region Orchestrator Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            if (_enableAutoInitialization)
            {
                InitializeOrchestrator();
            }
        }

        private void InitializeDataStructures()
        {
            _serviceStates = new Dictionary<string, bool>();
            _lastSystemProfiles = new Dictionary<string, PerformanceProfile>();
        }

        public void InitializeOrchestrator()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PerformanceManagementOrchestrator already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                InitializeServices();
                SetupServiceCoordination();
                
                _isInitialized = true;
                ChimeraLogger.Log("PerformanceManagementOrchestrator initialized successfully", this);
                
                UpdateOrchestrationStatus(PerformanceOrchestrationState.Active);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PerformanceManagementOrchestrator: {ex.Message}", this);
                UpdateOrchestrationStatus(PerformanceOrchestrationState.Error);
                throw;
            }
        }

        public void ShutdownOrchestrator()
        {
            if (!_isInitialized) return;

            UpdateOrchestrationStatus(PerformanceOrchestrationState.ShuttingDown);
            
            ShutdownServices();
            _serviceStates.Clear();
            _lastSystemProfiles.Clear();
            
            _isInitialized = false;
            _servicesInitialized = false;
            
            ChimeraLogger.Log("PerformanceManagementOrchestrator shutdown completed", this);
            UpdateOrchestrationStatus(PerformanceOrchestrationState.Inactive);
        }

        private void ValidateConfiguration()
        {
            if (_coordinationInterval <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid coordination interval, using default 1s", this);
                _coordinationInterval = 1f;
            }

            if (_targetFrameRate <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid target frame rate, using default 60 FPS", this);
                _targetFrameRate = 60f;
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// Initialize all performance services
        /// </summary>
        private void InitializeServices()
        {
            // Find services if not assigned
            FindServices();

            // Initialize services in dependency order
            InitializeMonitoringService();
            InitializeAlertService();
            InitializeOptimizationService();
            InitializeAnalyticsService();

            _servicesInitialized = ValidateAllServices();
            
            if (_servicesInitialized)
            {
                ChimeraLogger.Log("All performance services initialized successfully", this);
            }
            else
            {
                ChimeraLogger.LogWarning("Some performance services failed to initialize", this);
            }
        }

        /// <summary>
        /// Find services in the scene if not assigned
        /// </summary>
        private void FindServices()
        {
            if (_monitoringService == null)
            {
                _monitoringService = FindObjectOfType<PerformanceMonitoringService>();
                if (_monitoringService == null)
                {
                    ChimeraLogger.LogWarning("PerformanceMonitoringService not found, creating new instance", this);
                    var go = new GameObject("PerformanceMonitoringService");
                    _monitoringService = go.AddComponent<PerformanceMonitoringService>();
                }
            }

            if (_optimizationService == null)
            {
                _optimizationService = FindObjectOfType<PerformanceOptimizationService>();
                if (_optimizationService == null)
                {
                    ChimeraLogger.LogWarning("PerformanceOptimizationService not found, creating new instance", this);
                    var go = new GameObject("PerformanceOptimizationService");
                    _optimizationService = go.AddComponent<PerformanceOptimizationService>();
                }
            }

            if (_alertService == null)
            {
                _alertService = FindObjectOfType<PerformanceAlertService>();
                if (_alertService == null)
                {
                    ChimeraLogger.LogWarning("PerformanceAlertService not found, creating new instance", this);
                    var go = new GameObject("PerformanceAlertService");
                    _alertService = go.AddComponent<PerformanceAlertService>();
                }
            }

            if (_analyticsService == null)
            {
                _analyticsService = FindObjectOfType<PerformanceAnalyticsService>();
                if (_analyticsService == null)
                {
                    ChimeraLogger.LogWarning("PerformanceAnalyticsService not found, creating new instance", this);
                    var go = new GameObject("PerformanceAnalyticsService");
                    _analyticsService = go.AddComponent<PerformanceAnalyticsService>();
                }
            }
        }

        /// <summary>
        /// Initialize monitoring service
        /// </summary>
        private void InitializeMonitoringService()
        {
            try
            {
                if (_monitoringService != null)
                {
                    _monitoringService.Initialize();
                    _serviceStates["MonitoringService"] = _monitoringService.IsInitialized;
                    ChimeraLogger.Log("PerformanceMonitoringService initialized", this);
                }
            }
            catch (System.Exception ex)
            {
                _serviceStates["MonitoringService"] = false;
                OnServiceError?.Invoke($"MonitoringService initialization failed: {ex.Message}");
                ChimeraLogger.LogError($"Failed to initialize PerformanceMonitoringService: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Initialize alert service
        /// </summary>
        private void InitializeAlertService()
        {
            try
            {
                if (_alertService != null)
                {
                    _alertService.Initialize();
                    _serviceStates["AlertService"] = _alertService.IsInitialized;
                    ChimeraLogger.Log("PerformanceAlertService initialized", this);
                }
            }
            catch (System.Exception ex)
            {
                _serviceStates["AlertService"] = false;
                OnServiceError?.Invoke($"AlertService initialization failed: {ex.Message}");
                ChimeraLogger.LogError($"Failed to initialize PerformanceAlertService: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Initialize optimization service
        /// </summary>
        private void InitializeOptimizationService()
        {
            try
            {
                if (_optimizationService != null)
                {
                    _optimizationService.Initialize();
                    _serviceStates["OptimizationService"] = _optimizationService.IsInitialized;
                    ChimeraLogger.Log("PerformanceOptimizationService initialized", this);
                }
            }
            catch (System.Exception ex)
            {
                _serviceStates["OptimizationService"] = false;
                OnServiceError?.Invoke($"OptimizationService initialization failed: {ex.Message}");
                ChimeraLogger.LogError($"Failed to initialize PerformanceOptimizationService: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Initialize analytics service
        /// </summary>
        private void InitializeAnalyticsService()
        {
            try
            {
                if (_analyticsService != null)
                {
                    _analyticsService.Initialize();
                    _serviceStates["AnalyticsService"] = _analyticsService.IsInitialized;
                    ChimeraLogger.Log("PerformanceAnalyticsService initialized", this);
                }
            }
            catch (System.Exception ex)
            {
                _serviceStates["AnalyticsService"] = false;
                OnServiceError?.Invoke($"AnalyticsService initialization failed: {ex.Message}");
                ChimeraLogger.LogError($"Failed to initialize PerformanceAnalyticsService: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Validate all services are properly initialized
        /// </summary>
        private bool ValidateAllServices()
        {
            foreach (var serviceState in _serviceStates)
            {
                if (!serviceState.Value)
                {
                    ChimeraLogger.LogWarning($"Service {serviceState.Key} failed to initialize", this);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Shutdown all services
        /// </summary>
        private void ShutdownServices()
        {
            try
            {
                _analyticsService?.Shutdown();
                _optimizationService?.Shutdown();
                _alertService?.Shutdown();
                _monitoringService?.Shutdown();
                
                ChimeraLogger.Log("All performance services shut down", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error during service shutdown: {ex.Message}", this);
            }
        }

        #endregion

        #region Service Coordination

        private void Update()
        {
            if (!_isInitialized || !_servicesInitialized || !_enableServiceCoordination) return;

            // Coordinate services at specified interval
            if (Time.time - _lastCoordinationTime >= _coordinationInterval)
            {
                CoordinateServices();
                _lastCoordinationTime = Time.time;
            }
        }

        /// <summary>
        /// Setup service coordination and event subscriptions
        /// </summary>
        private void SetupServiceCoordination()
        {
            if (!_enableServiceCoordination) return;

            // Subscribe to service events for coordination
            if (_monitoringService != null)
            {
                PerformanceMonitoringService.OnMetricsUpdated += HandleMetricsUpdated;
                PerformanceMonitoringService.OnFrameDataRecorded += HandleFrameDataRecorded;
            }

            if (_alertService != null)
            {
                PerformanceAlertService.OnPerformanceAlert += HandlePerformanceAlert;
            }

            if (_optimizationService != null)
            {
                PerformanceOptimizationService.OnSystemOptimized += HandleSystemOptimized;
            }

            if (_analyticsService != null)
            {
                PerformanceAnalyticsService.OnTrendDetected += HandleTrendDetected;
                PerformanceAnalyticsService.OnBottleneckDetected += HandleBottleneckDetected;
            }

            ChimeraLogger.Log("Service coordination setup completed", this);
        }

        /// <summary>
        /// Coordinate all performance services
        /// </summary>
        private void CoordinateServices()
        {
            // Get latest metrics from monitoring service
            if (_monitoringService != null && _monitoringService.IsInitialized)
            {
                _lastGlobalMetrics = _monitoringService.GlobalMetrics;
                
                // Share metrics with other services
                ShareMetricsWithServices();
            }

            // Generate coordination report
            if (_enablePerformanceReporting)
            {
                GenerateCoordinationReport();
            }
        }

        /// <summary>
        /// Share performance metrics with all services
        /// </summary>
        private void ShareMetricsWithServices()
        {
            // Share with alert service for threshold monitoring
            if (_alertService != null && _alertService.IsInitialized)
            {
                _alertService.CheckPerformanceThresholds(_lastGlobalMetrics);
            }

            // Share with optimization service for dynamic quality adjustment
            if (_optimizationService != null && _optimizationService.IsInitialized)
            {
                _optimizationService.ApplyDynamicQualityAdjustments(_lastGlobalMetrics);
            }

            // Share frame data with analytics service
            if (_analyticsService != null && _analyticsService.IsInitialized && _monitoringService != null)
            {
                foreach (var frameData in _monitoringService.FrameHistory)
                {
                    _analyticsService.AddFrameData(frameData);
                }

                // Share system profiles
                foreach (var profile in _monitoringService.SystemProfiles)
                {
                    _analyticsService.AddSystemData(profile.Key, profile.Value);
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle metrics updated from monitoring service
        /// </summary>
        private void HandleMetricsUpdated(GlobalPerformanceMetrics metrics)
        {
            _lastGlobalMetrics = metrics;
            
            // Trigger coordinated response if needed
            if (metrics.OverallState == PerformanceState.Critical)
            {
                TriggerEmergencyOptimization();
            }
        }

        /// <summary>
        /// Handle frame data recorded
        /// </summary>
        private void HandleFrameDataRecorded(FramePerformanceData frameData)
        {
            // Pass to analytics service for trend analysis
            if (_analyticsService != null && _analyticsService.IsInitialized)
            {
                _analyticsService.AddFrameData(frameData);
            }
        }

        /// <summary>
        /// Handle performance alert
        /// </summary>
        private void HandlePerformanceAlert(PerformanceAlert alert)
        {
            ChimeraLogger.Log($"Coordinating response to {alert.Type} alert: {alert.Message}", this);
            
            // Trigger optimization if critical alert
            if (alert.Type == PerformanceAlertType.Critical)
            {
                TriggerTargetedOptimization(alert.SystemName);
            }
        }

        /// <summary>
        /// Handle system optimization
        /// </summary>
        private void HandleSystemOptimized(string systemName, PerformanceQualityLevel newQuality)
        {
            ChimeraLogger.Log($"System {systemName} optimized to {newQuality}", this);
        }

        /// <summary>
        /// Handle trend detection
        /// </summary>
        private void HandleTrendDetected(PerformanceTrend trend)
        {
            ChimeraLogger.Log($"Performance trend detected: {trend.Description}", this);
            
            // Proactive optimization based on trends
            if (trend.TrendDirection == TrendDirection.Increasing && trend.Confidence > 0.7f)
            {
                TriggerProactiveOptimization(trend.SystemName);
            }
        }

        /// <summary>
        /// Handle bottleneck detection
        /// </summary>
        private void HandleBottleneckDetected(BottleneckAnalysis bottleneck)
        {
            ChimeraLogger.LogWarning($"Bottleneck detected: {bottleneck.SystemName} using {bottleneck.CpuPercentage:P1} CPU", this);
            
            // Immediate optimization for bottlenecks
            TriggerTargetedOptimization(bottleneck.SystemName);
        }

        #endregion

        #region Optimization Coordination

        /// <summary>
        /// Trigger emergency optimization for critical performance issues
        /// </summary>
        private void TriggerEmergencyOptimization()
        {
            if (_optimizationService != null && _optimizationService.IsInitialized)
            {
                _optimizationService.ForceOptimizeAllSystems();
                ChimeraLogger.LogWarning("Emergency optimization triggered", this);
            }
        }

        /// <summary>
        /// Trigger targeted optimization for specific system
        /// </summary>
        private void TriggerTargetedOptimization(string systemName)
        {
            if (_optimizationService != null && _optimizationService.IsInitialized)
            {
                // Find specific optimizer for system
                if (_optimizationService.Optimizers.ContainsKey(systemName))
                {
                    _optimizationService.ExecuteOptimizer(systemName, null);
                    ChimeraLogger.Log($"Targeted optimization triggered for {systemName}", this);
                }
            }
        }

        /// <summary>
        /// Trigger proactive optimization based on trends
        /// </summary>
        private void TriggerProactiveOptimization(string systemName)
        {
            if (_optimizationService != null && _optimizationService.IsInitialized)
            {
                // Gentle optimization to prevent performance degradation
                ChimeraLogger.Log($"Proactive optimization suggested for {systemName}", this);
            }
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Generate coordination report
        /// </summary>
        private void GenerateCoordinationReport()
        {
            var report = new ServiceCoordinationReport
            {
                ReportId = $"COORD_{DateTime.Now:yyyyMMdd_HHmmss}",
                GeneratedAt = DateTime.Now,
                ServicesActive = CountActiveServices(),
                TotalServices = _serviceStates.Count,
                GlobalMetrics = _lastGlobalMetrics,
                CoordinationInterval = _coordinationInterval,
                SystemStates = new Dictionary<string, bool>(_serviceStates)
            };

            OnCoordinationCompleted?.Invoke(report);
        }

        /// <summary>
        /// Count active services
        /// </summary>
        private int CountActiveServices()
        {
            var activeCount = 0;
            foreach (var state in _serviceStates.Values)
            {
                if (state) activeCount++;
            }
            return activeCount;
        }

        /// <summary>
        /// Update orchestration status
        /// </summary>
        private void UpdateOrchestrationStatus(PerformanceOrchestrationState state)
        {
            var status = new PerformanceOrchestrationStatus
            {
                State = state,
                ServicesInitialized = _servicesInitialized,
                ActiveServices = CountActiveServices(),
                TotalServices = _serviceStates.Count,
                LastUpdate = DateTime.Now
            };

            OnOrchestrationStatusChanged?.Invoke(status);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get service status
        /// </summary>
        public ServiceStatus GetServiceStatus(string serviceName)
        {
            return new ServiceStatus
            {
                ServiceName = serviceName,
                IsActive = _serviceStates.ContainsKey(serviceName) && _serviceStates[serviceName],
                IsInitialized = _serviceStates.ContainsKey(serviceName) && _serviceStates[serviceName]
            };
        }

        /// <summary>
        /// Force coordination cycle
        /// </summary>
        public void ForceCoordination()
        {
            if (_isInitialized && _servicesInitialized)
            {
                CoordinateServices();
                ChimeraLogger.Log("Manual coordination cycle triggered", this);
            }
        }

        /// <summary>
        /// Get overall orchestration health
        /// </summary>
        public PerformanceOrchestrationHealth GetOrchestrationHealth()
        {
            var activeServices = CountActiveServices();
            var healthScore = _serviceStates.Count > 0 ? (float)activeServices / _serviceStates.Count : 0f;

            return new PerformanceOrchestrationHealth
            {
                HealthScore = healthScore,
                ActiveServices = activeServices,
                TotalServices = _serviceStates.Count,
                IsHealthy = healthScore >= 0.8f,
                LastAssessment = DateTime.Now
            };
        }

        /// <summary>
        /// Restart all services
        /// </summary>
        public void RestartAllServices()
        {
            ChimeraLogger.Log("Restarting all performance services", this);
            
            ShutdownServices();
            System.Threading.Thread.Sleep(100); // Brief pause
            InitializeServices();
            
            ChimeraLogger.Log("All performance services restarted", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_monitoringService != null)
            {
                PerformanceMonitoringService.OnMetricsUpdated -= HandleMetricsUpdated;
                PerformanceMonitoringService.OnFrameDataRecorded -= HandleFrameDataRecorded;
            }

            if (_alertService != null)
            {
                PerformanceAlertService.OnPerformanceAlert -= HandlePerformanceAlert;
            }

            if (_optimizationService != null)
            {
                PerformanceOptimizationService.OnSystemOptimized -= HandleSystemOptimized;
            }

            if (_analyticsService != null)
            {
                PerformanceAnalyticsService.OnTrendDetected -= HandleTrendDetected;
                PerformanceAnalyticsService.OnBottleneckDetected -= HandleBottleneckDetected;
            }

            ShutdownOrchestrator();
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Service coordination report
    /// </summary>
    [System.Serializable]
    public class ServiceCoordinationReport
    {
        public string ReportId;
        public DateTime GeneratedAt;
        public int ServicesActive;
        public int TotalServices;
        public GlobalPerformanceMetrics GlobalMetrics;
        public float CoordinationInterval;
        public Dictionary<string, bool> SystemStates;
    }

    /// <summary>
    /// Performance orchestration status
    /// </summary>
    [System.Serializable]
    public class PerformanceOrchestrationStatus
    {
        public PerformanceOrchestrationState State;
        public bool ServicesInitialized;
        public int ActiveServices;
        public int TotalServices;
        public DateTime LastUpdate;
    }

    /// <summary>
    /// Service status information
    /// </summary>
    [System.Serializable]
    public class ServiceStatus
    {
        public string ServiceName;
        public bool IsActive;
        public bool IsInitialized;
    }

    /// <summary>
    /// Performance orchestration health assessment
    /// </summary>
    [System.Serializable]
    public class PerformanceOrchestrationHealth
    {
        public float HealthScore;
        public int ActiveServices;
        public int TotalServices;
        public bool IsHealthy;
        public DateTime LastAssessment;
    }

    /// <summary>
    /// Performance orchestration states
    /// </summary>
    public enum PerformanceOrchestrationState
    {
        Inactive,
        Initializing,
        Active,
        ShuttingDown,
        Error
    }

    #endregion
}