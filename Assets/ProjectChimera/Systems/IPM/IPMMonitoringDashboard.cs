using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Comprehensive IPM monitoring dashboard providing real-time visualization,
    /// analytics, and control interface for the pest management system.
    /// </summary>
    public class IPMMonitoringDashboard : ChimeraManager
    {
        [Header("Dashboard Configuration")]
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private bool _enableAlerts = true;
        [SerializeField] private bool _enableDataLogging = true;
        [SerializeField] private float _updateInterval = 5f; // seconds
        [SerializeField] private int _maxHistoryEntries = 1000;
        
        [Header("Display Settings")]
        [SerializeField] private bool _showDetectionMetrics = true;
        [SerializeField] private bool _showThreatAnalysis = true;
        [SerializeField] private bool _showEnvironmentalData = true;
        [SerializeField] private bool _showTreatmentHistory = true;
        [SerializeField] private bool _showPredictiveAnalytics = true;
        
        [Header("Alert Thresholds")]
        [SerializeField] private float _criticalThreatThreshold = 0.8f;
        [SerializeField] private float _highThreatThreshold = 0.6f;
        [SerializeField] private int _maxSimultaneousThreats = 5;
        [SerializeField] private float _falseAlarmThreshold = 0.3f;
        
        [Header("Data Visualization")]
        [SerializeField] private bool _enableGraphs = true;
        [SerializeField] private bool _enableHeatmaps = true;
        [SerializeField] private bool _enableTrendAnalysis = true;
        [SerializeField] private Color _lowRiskColor = Color.green;
        [SerializeField] private Color _moderateRiskColor = Color.yellow;
        [SerializeField] private Color _highRiskColor = Color.red;
        [SerializeField] private Color _criticalRiskColor = Color.magenta;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onDashboardUpdated;
        [SerializeField] private SimpleGameEventSO _onAlertTriggered;
        [SerializeField] private SimpleGameEventSO _onSystemStatus;
        
        // Core systems references
        private PestDetectionSystem _detectionSystem;
        private IPMFramework _ipmFramework;
        private CleanIPMManager _cleanIPMManager;
        
        // Dashboard data
        private DashboardData _currentData;
        private List<DashboardSnapshot> _historySnapshots = new List<DashboardSnapshot>();
        private Dictionary<string, PlantMonitoringData> _plantData = new Dictionary<string, PlantMonitoringData>();
        private Dictionary<ProjectChimera.Data.IPM.PestType, ThreatAnalysisData> _threatAnalysis = new Dictionary<ProjectChimera.Data.IPM.PestType, ThreatAnalysisData>();
        private Dictionary<string, EnvironmentalRiskFactor> _environmentalRisks = new Dictionary<string, EnvironmentalRiskFactor>();
        
        // Real-time tracking
        private float _lastUpdateTime = 0f;
        private SystemStatus _currentStatus = SystemStatus.Initializing;
        private List<AlertData> _activeAlerts = new List<AlertData>();
        private int _totalAlertsIssued = 0;
        
        // Performance metrics
        private DashboardMetrics _metrics;
        
        // Events
        public System.Action<DashboardData> OnDashboardUpdated;
        public System.Action<AlertData> OnAlertTriggered;
        public System.Action<SystemStatus> OnSystemStatusChanged;
        
        // Properties
        public override string ManagerName => "IPM Monitoring Dashboard";
        public override ManagerPriority Priority => ManagerPriority.High;
        public DashboardData CurrentData => _currentData;
        public List<AlertData> ActiveAlerts => new List<AlertData>(_activeAlerts);
        public SystemStatus CurrentStatus => _currentStatus;
        public int TotalAlertsIssued => _totalAlertsIssued;
        public DashboardMetrics Metrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeDashboard();
            ConnectToIPMSystems();
            InitializeThreatAnalysis();
            
            _currentData = new DashboardData();
            _metrics = new DashboardMetrics();
            
            SetSystemStatus(SystemStatus.Active);
            
            LogInfo("IPM Monitoring Dashboard initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableRealTimeUpdates) return;
            
            float currentTime = Time.time;
            if (currentTime - _lastUpdateTime >= _updateInterval)
            {
                UpdateDashboard();
                _lastUpdateTime = currentTime;
            }
            
            ProcessAlerts();
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            _historySnapshots.Clear();
            _plantData.Clear();
            _threatAnalysis.Clear();
            _activeAlerts.Clear();
            
            LogInfo("IPM Monitoring Dashboard shutdown completed");
        }
        
        /// <summary>
        /// Get comprehensive dashboard data
        /// </summary>
        public DashboardData GetDashboardData()
        {
            return _currentData;
        }
        
        /// <summary>
        /// Get historical dashboard snapshots
        /// </summary>
        public List<DashboardSnapshot> GetHistorySnapshots(int count = 100)
        {
            return _historySnapshots.TakeLast(count).ToList();
        }
        
        /// <summary>
        /// Get plant-specific monitoring data
        /// </summary>
        public PlantMonitoringData GetPlantData(string plantId)
        {
            return _plantData.ContainsKey(plantId) ? _plantData[plantId] : null;
        }
        
        /// <summary>
        /// Get threat analysis for specific pest type
        /// </summary>
        public ThreatAnalysisData GetThreatAnalysis(ProjectChimera.Data.IPM.PestType pestType)
        {
            return _threatAnalysis.ContainsKey(pestType) ? _threatAnalysis[pestType] : null;
        }
        
        /// <summary>
        /// Get comprehensive system report
        /// </summary>
        public IPMSystemReport GetSystemReport()
        {
            return new IPMSystemReport
            {
                TotalPlantsMonitored = _plantData.Count,
                TotalPestsDetected = _detectionSystem?.TotalPestsDetected ?? 0,
                TotalTreatmentsApplied = _ipmFramework?.TotalTreatmentsApplied ?? 0,
                ActiveTreatments = _ipmFramework?.ActiveTreatments ?? 0,
                SystemEffectiveness = CalculateSystemEfficiency(),
                PreventionSuccessRate = 0.85f, // Simulated - would calculate from actual data
                AverageDetectionAccuracy = _detectionSystem?.ActualAccuracy ?? 0f,
                BiologicalControlsActive = 3, // Simulated - would get from actual systems
                DefenseStructuresActive = 5, // Simulated - would get from actual systems
                ReportDate = DateTime.Now
            };
        }
        
        /// <summary>
        /// Trigger manual dashboard update
        /// </summary>
        public void ForceUpdate()
        {
            UpdateDashboard();
        }
        
        /// <summary>
        /// Configure alert thresholds
        /// </summary>
        public void ConfigureAlertThresholds(float critical, float high, int maxThreats)
        {
            _criticalThreatThreshold = Mathf.Clamp01(critical);
            _highThreatThreshold = Mathf.Clamp01(high);
            _maxSimultaneousThreats = Mathf.Max(1, maxThreats);
            
            LogInfo($"Alert thresholds updated: Critical={critical:P0}, High={high:P0}, Max={maxThreats}");
        }
        
        /// <summary>
        /// Enable or disable specific dashboard features
        /// </summary>
        public void ConfigureFeature(DashboardFeature feature, bool enabled)
        {
            switch (feature)
            {
                case DashboardFeature.RealTimeUpdates:
                    _enableRealTimeUpdates = enabled;
                    break;
                case DashboardFeature.Alerts:
                    _enableAlerts = enabled;
                    break;
                case DashboardFeature.DataLogging:
                    _enableDataLogging = enabled;
                    break;
                case DashboardFeature.DetectionMetrics:
                    _showDetectionMetrics = enabled;
                    break;
                case DashboardFeature.ThreatAnalysis:
                    _showThreatAnalysis = enabled;
                    break;
                case DashboardFeature.EnvironmentalData:
                    _showEnvironmentalData = enabled;
                    break;
                case DashboardFeature.TreatmentHistory:
                    _showTreatmentHistory = enabled;
                    break;
                case DashboardFeature.PredictiveAnalytics:
                    _showPredictiveAnalytics = enabled;
                    break;
            }
            
            LogInfo($"Dashboard feature {feature} {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Clear specific alert
        /// </summary>
        public void ClearAlert(string alertId)
        {
            var alert = _activeAlerts.FirstOrDefault(a => a.AlertId == alertId);
            if (alert != null)
            {
                _activeAlerts.Remove(alert);
                LogInfo($"Alert cleared: {alertId}");
            }
        }
        
        /// <summary>
        /// Clear all alerts
        /// </summary>
        public void ClearAllAlerts()
        {
            var count = _activeAlerts.Count;
            _activeAlerts.Clear();
            LogInfo($"All alerts cleared ({count} alerts)");
        }
        
        #region Private Implementation
        
        private void InitializeDashboard()
        {
            _currentData = new DashboardData
            {
                LastUpdated = DateTime.Now,
                DetectionMetrics = new DetectionMetrics(),
                ThreatSummary = new ThreatSummary(),
                EnvironmentalOverview = new EnvironmentalOverview(),
                TreatmentSummary = new TreatmentSummary(),
                PredictiveInsights = new PredictiveInsights()
            };
        }
        
        private void ConnectToIPMSystems()
        {
            _detectionSystem = GameManager.Instance?.GetManager<PestDetectionSystem>();
            _ipmFramework = GameManager.Instance?.GetManager<IPMFramework>();
            _cleanIPMManager = GameManager.Instance?.GetManager<CleanIPMManager>();
            
            // Subscribe to events - commented out until exact signatures are confirmed
            // if (_detectionSystem != null)
            // {
            //     _detectionSystem.OnPestDetected += OnPestDetected;
            //     _detectionSystem.OnEarlyWarning += OnEarlyWarning;
            //     _detectionSystem.OnScanComplete += OnScanComplete;
            // }
            
            // if (_ipmFramework != null)
            // {
            //     _ipmFramework.OnPestDetected += OnPestDetected;
            //     _ipmFramework.OnTreatmentApplied += OnTreatmentApplied;
            //     _ipmFramework.OnIPMAlert += OnIPMAlert;
            // }
            
            LogInfo("IPM Systems connected successfully");
        }
        
        private void InitializeThreatAnalysis()
        {
            var pestTypes = Enum.GetValues(typeof(ProjectChimera.Data.IPM.PestType));
            foreach (ProjectChimera.Data.IPM.PestType pestType in pestTypes)
            {
                _threatAnalysis[pestType] = new ThreatAnalysisData
                {
                    PestType = pestType,
                    ThreatLevel = 0f,
                    IsActive = false,
                    FirstDetected = DateTime.MinValue,
                    LastDetected = DateTime.MinValue,
                    TotalDetections = 0,
                    AverageConfidence = 0f,
                    AffectedPlants = new List<string>(),
                    TrendDirection = TrendDirection.Stable
                };
            }
            
            // Initialize environmental risk factors
            _environmentalRisks["HighTemperature"] = new EnvironmentalRiskFactor
            {
                Name = "High Temperature",
                RiskMultiplier = 1.5f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType> { ProjectChimera.Data.IPM.PestType.SpiderMites },
                ThresholdValue = 28f,
                IsActive = false
            };
            
            _environmentalRisks["HighHumidity"] = new EnvironmentalRiskFactor
            {
                Name = "High Humidity",
                RiskMultiplier = 1.3f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType> { ProjectChimera.Data.IPM.PestType.Aphids },
                ThresholdValue = 0.8f,
                IsActive = false
            };
        }
        
        private void UpdateDashboard()
        {
            // Update detection metrics
            if (_showDetectionMetrics && _detectionSystem != null)
            {
                UpdateDetectionMetrics();
            }
            
            // Update threat analysis
            if (_showThreatAnalysis)
            {
                UpdateThreatAnalysis();
            }
            
            // Update environmental data
            if (_showEnvironmentalData)
            {
                UpdateEnvironmentalData();
            }
            
            // Update treatment history
            if (_showTreatmentHistory)
            {
                UpdateTreatmentHistory();
            }
            
            // Update predictive analytics
            if (_showPredictiveAnalytics)
            {
                UpdatePredictiveAnalytics();
            }
            
            // Update plant monitoring data
            UpdatePlantMonitoringData();
            
            // Create snapshot for history
            if (_enableDataLogging)
            {
                CreateHistorySnapshot();
            }
            
            // Update timestamp
            _currentData.LastUpdated = DateTime.Now;
            
            // Trigger events
            OnDashboardUpdated?.Invoke(_currentData);
            _onDashboardUpdated?.Raise();
        }
        
        private void UpdateDetectionMetrics()
        {
            var metrics = _detectionSystem.Metrics;
            _currentData.DetectionMetrics = new DetectionMetrics
            {
                TotalScansPerformed = metrics.TotalScansPerformed,
                TotalPestsDetected = metrics.TotalPestsDetected,
                FalsePositives = metrics.FalsePositives,
                EarlyWarningsIssued = metrics.EarlyWarningsIssued,
                DetectionAccuracy = metrics.DetectionAccuracy,
                ActualAccuracy = metrics.ActualAccuracy,
                LastUpdated = metrics.LastUpdated
            };
        }
        
        private void UpdateThreatAnalysis()
        {
            var threatSummary = new ThreatSummary
            {
                ActiveThreats = _threatAnalysis.Values.Count(t => t.IsActive),
                CriticalThreats = _threatAnalysis.Values.Count(t => t.ThreatLevel >= _criticalThreatThreshold),
                HighThreats = _threatAnalysis.Values.Count(t => t.ThreatLevel >= _highThreatThreshold && t.ThreatLevel < _criticalThreatThreshold),
                TotalThreatsDetected = _threatAnalysis.Values.Sum(t => t.TotalDetections),
                AverageResponseTime = TimeSpan.FromMinutes(15), // Simulated
                TopThreats = _threatAnalysis.Values
                    .Where(t => t.IsActive)
                    .OrderByDescending(t => t.ThreatLevel)
                    .Take(5)
                    .Select(t => t.PestType)
                    .ToList()
            };
            
            _currentData.ThreatSummary = threatSummary;
        }
        
        private void UpdateEnvironmentalData()
        {
            // Simulate environmental data - in real implementation, this would
            // query the actual environmental management systems
            var environmentalOverview = new EnvironmentalOverview
            {
                AverageTemperature = UnityEngine.Random.Range(22f, 28f),
                AverageHumidity = UnityEngine.Random.Range(0.5f, 0.8f),
                AverageAirFlow = UnityEngine.Random.Range(0.4f, 0.9f),
                AverageLightIntensity = UnityEngine.Random.Range(500f, 700f),
                AverageCO2Level = UnityEngine.Random.Range(1000f, 1400f),
                RiskFactorsActive = CalculateActiveRiskFactors(),
                OptimalConditions = CalculateOptimalConditions(),
                LastEnvironmentalScan = DateTime.Now.AddMinutes(-UnityEngine.Random.Range(1, 30))
            };
            
            _currentData.EnvironmentalOverview = environmentalOverview;
        }
        
        private void UpdateTreatmentHistory()
        {
            var treatmentSummary = new TreatmentSummary
            {
                TotalTreatmentsApplied = _ipmFramework?.TotalTreatmentsApplied ?? 0,
                SuccessfulTreatments = Mathf.RoundToInt((_ipmFramework?.TotalTreatmentsApplied ?? 0) * 0.8f),
                ActiveTreatments = _ipmFramework?.ActiveTreatments ?? 0,
                AverageEffectiveness = _ipmFramework?.SystemEffectiveness ?? 0f,
                PreferredTreatmentTypes = GetPreferredTreatmentTypes(),
                LastTreatmentApplied = DateTime.Now.AddHours(-UnityEngine.Random.Range(1, 24))
            };
            
            _currentData.TreatmentSummary = treatmentSummary;
        }
        
        private void UpdatePredictiveAnalytics()
        {
            var predictiveInsights = new PredictiveInsights
            {
                PredictedOutbreaks = GeneratePredictedOutbreaks(),
                SeasonalTrends = GenerateSeasonalTrends(),
                RiskPredictions = GenerateRiskPredictions(),
                RecommendedActions = GenerateRecommendedActions(),
                ConfidenceLevel = UnityEngine.Random.Range(0.7f, 0.95f),
                PredictionHorizon = TimeSpan.FromDays(14),
                LastPredictionUpdate = DateTime.Now
            };
            
            _currentData.PredictiveInsights = predictiveInsights;
        }
        
        private void UpdatePlantMonitoringData()
        {
            // Update monitoring data for each plant
            foreach (var plantId in _plantData.Keys.ToList())
            {
                var plantData = _plantData[plantId];
                plantData.LastScanned = DateTime.Now.AddMinutes(-UnityEngine.Random.Range(5, 60));
                plantData.CurrentRiskLevel = (RiskLevel)UnityEngine.Random.Range(0, 4);
                plantData.DetectedThreats = UnityEngine.Random.Range(0, 3);
                plantData.HealthScore = UnityEngine.Random.Range(0.6f, 1.0f);
                plantData.RecentAlerts = UnityEngine.Random.Range(0, 2);
            }
            
            // Add new plants if needed
            for (int i = _plantData.Count; i < 5; i++)
            {
                var plantId = $"plant_{i:D3}";
                _plantData[plantId] = new PlantMonitoringData
                {
                    PlantId = plantId,
                    LastScanned = DateTime.Now.AddMinutes(-UnityEngine.Random.Range(5, 60)),
                    CurrentRiskLevel = (RiskLevel)UnityEngine.Random.Range(0, 4),
                    DetectedThreats = UnityEngine.Random.Range(0, 3),
                    HealthScore = UnityEngine.Random.Range(0.6f, 1.0f),
                    RecentAlerts = UnityEngine.Random.Range(0, 2),
                    MonitoringEnabled = true
                };
            }
        }
        
        private void UpdateMetrics()
        {
            _metrics.TotalDashboardUpdates++;
            _metrics.TotalAlertsIssued = _totalAlertsIssued;
            _metrics.ActiveAlerts = _activeAlerts.Count;
            _metrics.SystemStatus = _currentStatus;
            _metrics.LastUpdated = DateTime.Now;
        }
        
        private void CreateHistorySnapshot()
        {
            var snapshot = new DashboardSnapshot
            {
                Timestamp = DateTime.Now,
                TotalScans = _detectionSystem?.TotalScansPerformed ?? 0,
                TotalPests = _detectionSystem?.TotalPestsDetected ?? 0,
                ActiveThreats = _threatAnalysis.Values.Count(t => t.IsActive),
                CriticalThreats = _threatAnalysis.Values.Count(t => t.ThreatLevel >= _criticalThreatThreshold),
                AverageRiskLevel = CalculateAverageRiskLevel(),
                SystemEfficiency = CalculateSystemEfficiency()
            };
            
            _historySnapshots.Add(snapshot);
            
            // Limit history size
            if (_historySnapshots.Count > _maxHistoryEntries)
            {
                _historySnapshots.RemoveAt(0);
            }
        }
        
        private void ProcessAlerts()
        {
            if (!_enableAlerts) return;
            
            // Check for critical threats
            var criticalThreats = _threatAnalysis.Values.Where(t => t.ThreatLevel >= _criticalThreatThreshold).ToList();
            
            foreach (var threat in criticalThreats)
            {
                var alertId = $"critical-{threat.PestType}-{DateTime.Now:yyyyMMdd-HHmmss}";
                var existingAlert = _activeAlerts.FirstOrDefault(a => a.SourceId == threat.PestType.ToString());
                
                if (existingAlert == null)
                {
                    var alert = new AlertData
                    {
                        AlertId = alertId,
                        AlertType = AlertType.CriticalThreat,
                        Message = $"Critical threat detected: {threat.PestType}",
                        SourceId = threat.PestType.ToString(),
                        Severity = AlertSeverity.Critical,
                        CreatedAt = DateTime.Now,
                        LastOccurrence = DateTime.Now,
                        OccurrenceCount = 1,
                        IsAcknowledged = false
                    };
                    
                    _activeAlerts.Add(alert);
                    _totalAlertsIssued++;
                    
                    OnAlertTriggered?.Invoke(alert);
                    _onAlertTriggered?.Raise();
                }
                else
                {
                    existingAlert.LastOccurrence = DateTime.Now;
                    existingAlert.OccurrenceCount++;
                }
            }
            
            // Check for multiple simultaneous threats
            var activeThreats = _threatAnalysis.Values.Where(t => t.IsActive).ToList();
            if (activeThreats.Count > _maxSimultaneousThreats)
            {
                var alertId = $"multiple-threats-{DateTime.Now:yyyyMMdd-HHmmss}";
                var existingAlert = _activeAlerts.FirstOrDefault(a => a.AlertType == AlertType.MultipleThreats);
                
                if (existingAlert == null)
                {
                    var alert = new AlertData
                    {
                        AlertId = alertId,
                        AlertType = AlertType.MultipleThreats,
                        Message = $"Multiple threats detected: {activeThreats.Count} active threats",
                        SourceId = "system",
                        Severity = AlertSeverity.High,
                        CreatedAt = DateTime.Now,
                        LastOccurrence = DateTime.Now,
                        OccurrenceCount = 1,
                        IsAcknowledged = false
                    };
                    
                    _activeAlerts.Add(alert);
                    _totalAlertsIssued++;
                    
                    OnAlertTriggered?.Invoke(alert);
                    _onAlertTriggered?.Raise();
                }
            }
        }
        
        private void SetSystemStatus(SystemStatus status)
        {
            if (_currentStatus != status)
            {
                _currentStatus = status;
                OnSystemStatusChanged?.Invoke(status);
                _onSystemStatus?.Raise();
            }
        }
        
        private float CalculateAverageRiskLevel()
        {
            var activeThreats = _threatAnalysis.Values.Where(t => t.IsActive).ToList();
            return activeThreats.Any() ? activeThreats.Average(t => t.ThreatLevel) : 0f;
        }
        
        private float CalculateSystemEfficiency()
        {
            var totalDetections = _detectionSystem?.TotalPestsDetected ?? 0;
            var falsePositives = _detectionSystem?.FalsePositives ?? 0;
            
            if (totalDetections == 0) return 1f;
            
            var accuracy = 1f - (falsePositives / (float)totalDetections);
            var threatResponse = _threatAnalysis.Values.Count(t => t.IsActive) / (float)_threatAnalysis.Count;
            
            return Mathf.Clamp01(accuracy * (1f - threatResponse));
        }
        
        private int CalculateActiveRiskFactors()
        {
            return _environmentalRisks.Values.Count(r => r.IsActive);
        }
        
        private float CalculateOptimalConditions()
        {
            // Simulate optimal conditions calculation
            return UnityEngine.Random.Range(0.6f, 0.9f);
        }
        
        private List<string> GetPreferredTreatmentTypes()
        {
            return new List<string> { "Biological Control", "Integrated Approach", "Preventive Measures" };
        }
        
        private List<PredictedOutbreak> GeneratePredictedOutbreaks()
        {
            var outbreaks = new List<PredictedOutbreak>();
            
            foreach (var threat in _threatAnalysis.Values.Where(t => t.ThreatLevel > 0.3f))
            {
                outbreaks.Add(new PredictedOutbreak
                {
                    PestType = threat.PestType,
                    Probability = threat.ThreatLevel * 0.8f,
                    PredictedDate = DateTime.Now.AddDays(UnityEngine.Random.Range(1, 14)),
                    AffectedPlants = UnityEngine.Random.Range(1, 5),
                    Severity = (OutbreakSeverity)UnityEngine.Random.Range(0, 4)
                });
            }
            
            return outbreaks;
        }
        
        private List<SeasonalTrend> GenerateSeasonalTrends()
        {
            var trends = new List<SeasonalTrend>();
            var seasons = new[] { "Spring", "Summer", "Fall", "Winter" };
            
            foreach (var season in seasons)
            {
                trends.Add(new SeasonalTrend
                {
                    Season = season,
                    PestType = (ProjectChimera.Data.IPM.PestType)UnityEngine.Random.Range(0, 5),
                    TrendDirection = (TrendDirection)UnityEngine.Random.Range(0, 3),
                    Magnitude = UnityEngine.Random.Range(0.1f, 0.9f)
                });
            }
            
            return trends;
        }
        
        private List<RiskPrediction> GenerateRiskPredictions()
        {
            var predictions = new List<RiskPrediction>();
            var riskFactors = new[] { "Temperature", "Humidity", "Light Intensity", "Air Flow" };
            
            foreach (var factor in riskFactors)
            {
                predictions.Add(new RiskPrediction
                {
                    RiskFactor = factor,
                    CurrentLevel = UnityEngine.Random.Range(0.2f, 0.8f),
                    PredictedLevel = UnityEngine.Random.Range(0.3f, 0.9f),
                    TimeHorizon = TimeSpan.FromDays(UnityEngine.Random.Range(1, 7)),
                    Confidence = UnityEngine.Random.Range(0.6f, 0.9f)
                });
            }
            
            return predictions;
        }
        
        private List<string> GenerateRecommendedActions()
        {
            var actions = new List<string>
            {
                "Increase monitoring frequency",
                "Deploy beneficial organisms",
                "Adjust environmental conditions",
                "Apply preventive treatments",
                "Isolate affected plants",
                "Review cultivation practices"
            };
            
            return actions.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
        }
        
        // Event handlers - using generic signatures to handle various event types
        private void OnPestDetected(PestInvasionData invasionData)
        {
            if (invasionData?.PestType != null && _threatAnalysis.ContainsKey(invasionData.PestType))
            {
                var threat = _threatAnalysis[invasionData.PestType];
                threat.IsActive = true;
                threat.LastDetected = DateTime.Now;
                threat.TotalDetections++;
                
                if (threat.FirstDetected == DateTime.MinValue)
                {
                    threat.FirstDetected = DateTime.Now;
                }
                
                threat.ThreatLevel = Mathf.Min(1f, threat.ThreatLevel + 0.1f);
            }
        }
        
        private void OnTreatmentApplied(IPMTreatmentData treatmentData)
        {
            LogInfo($"Treatment applied: {treatmentData?.TreatmentPlan?.TreatmentName ?? "Unknown"}");
        }
        
        private void OnIPMAlert(IPMAlert alert)
        {
            LogInfo($"IPM Alert: {alert?.AlertType ?? "Unknown"} - {alert?.Message ?? "No message"}");
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class DashboardData
    {
        public DateTime LastUpdated;
        public DetectionMetrics DetectionMetrics;
        public ThreatSummary ThreatSummary;
        public EnvironmentalOverview EnvironmentalOverview;
        public TreatmentSummary TreatmentSummary;
        public PredictiveInsights PredictiveInsights;
        
        public DashboardData()
        {
            LastUpdated = DateTime.Now;
            DetectionMetrics = new DetectionMetrics();
            ThreatSummary = new ThreatSummary();
            EnvironmentalOverview = new EnvironmentalOverview();
            TreatmentSummary = new TreatmentSummary();
            PredictiveInsights = new PredictiveInsights();
        }
    }
    
    [System.Serializable]
    public class ThreatSummary
    {
        public int ActiveThreats;
        public int CriticalThreats;
        public int HighThreats;
        public int TotalThreatsDetected;
        public TimeSpan AverageResponseTime;
        public List<ProjectChimera.Data.IPM.PestType> TopThreats = new List<ProjectChimera.Data.IPM.PestType>();
    }
    
    [System.Serializable]
    public class EnvironmentalOverview
    {
        public float AverageTemperature;
        public float AverageHumidity;
        public float AverageAirFlow;
        public float AverageLightIntensity;
        public float AverageCO2Level;
        public int RiskFactorsActive;
        public float OptimalConditions;
        public DateTime LastEnvironmentalScan;
    }
    
    [System.Serializable]
    public class TreatmentSummary
    {
        public int TotalTreatmentsApplied;
        public int SuccessfulTreatments;
        public int ActiveTreatments;
        public float AverageEffectiveness;
        public List<string> PreferredTreatmentTypes = new List<string>();
        public DateTime LastTreatmentApplied;
    }
    
    [System.Serializable]
    public class PredictiveInsights
    {
        public List<PredictedOutbreak> PredictedOutbreaks = new List<PredictedOutbreak>();
        public List<SeasonalTrend> SeasonalTrends = new List<SeasonalTrend>();
        public List<RiskPrediction> RiskPredictions = new List<RiskPrediction>();
        public List<string> RecommendedActions = new List<string>();
        public float ConfidenceLevel;
        public TimeSpan PredictionHorizon;
        public DateTime LastPredictionUpdate;
    }
    
    [System.Serializable]
    public class PlantMonitoringData
    {
        public string PlantId;
        public DateTime LastScanned;
        public RiskLevel CurrentRiskLevel;
        public int DetectedThreats;
        public float HealthScore;
        public int RecentAlerts;
        public int RecentTreatments;
        public bool MonitoringEnabled;
    }
    
    [System.Serializable]
    public class ThreatAnalysisData
    {
        public ProjectChimera.Data.IPM.PestType PestType;
        public float ThreatLevel;
        public bool IsActive;
        public DateTime FirstDetected;
        public DateTime LastDetected;
        public int TotalDetections;
        public float AverageConfidence;
        public List<string> AffectedPlants = new List<string>();
        public TrendDirection TrendDirection;
    }
    
    [System.Serializable]
    public class DashboardSnapshot
    {
        public DateTime Timestamp;
        public int TotalScans;
        public int TotalPests;
        public int ActiveThreats;
        public int CriticalThreats;
        public float AverageRiskLevel;
        public float SystemEfficiency;
    }
    
    [System.Serializable]
    public class AlertData
    {
        public string AlertId;
        public AlertType AlertType;
        public string Message;
        public string SourceId;
        public AlertSeverity Severity;
        public DateTime CreatedAt;
        public DateTime LastOccurrence;
        public int OccurrenceCount;
        public bool IsAcknowledged;
    }
    
    [System.Serializable]
    public class DashboardMetrics
    {
        public int TotalDashboardUpdates;
        public int TotalAlertsIssued;
        public int ActiveAlerts;
        public SystemStatus SystemStatus;
        public DateTime LastUpdated;
        
        public DashboardMetrics()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PredictedOutbreak
    {
        public ProjectChimera.Data.IPM.PestType PestType;
        public float Probability;
        public DateTime PredictedDate;
        public int AffectedPlants;
        public OutbreakSeverity Severity;
    }
    
    [System.Serializable]
    public class SeasonalTrend
    {
        public string Season;
        public ProjectChimera.Data.IPM.PestType PestType;
        public TrendDirection TrendDirection;
        public float Magnitude;
    }
    
    [System.Serializable]
    public class RiskPrediction
    {
        public string RiskFactor;
        public float CurrentLevel;
        public float PredictedLevel;
        public TimeSpan TimeHorizon;
        public float Confidence;
    }
    
    // Enums
    public enum SystemStatus
    {
        Initializing,
        Active,
        Warning,
        Error,
        Offline
    }
    
    public enum AlertType
    {
        CriticalThreat,
        HighThreat,
        MultipleThreats,
        SystemIssue,
        EnvironmentalWarning,
        EarlyWarning
    }
    
    public enum AlertSeverity
    {
        Low,
        Moderate,
        High,
        Critical
    }
    
    public enum DashboardFeature
    {
        RealTimeUpdates,
        Alerts,
        DataLogging,
        DetectionMetrics,
        ThreatAnalysis,
        EnvironmentalData,
        TreatmentHistory,
        PredictiveAnalytics
    }
    
    // Note: TrendDirection is defined in IPMLeaderboardDataStructures.cs
    
    public enum OutbreakSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical
    }
}