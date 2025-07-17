using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Advanced pest detection and monitoring system for cannabis cultivation.
    /// Provides real-time pest scanning, early warning systems, and predictive analytics.
    /// </summary>
    public class PestDetectionSystem : ChimeraManager
    {
        [Header("Detection Configuration")]
        [SerializeField] private bool _enableAutomaticScanning = true;
        [SerializeField] private bool _enablePredictiveAnalytics = true;
        [SerializeField] private bool _enableEarlyWarning = true;
        [SerializeField] private float _scanInterval = 30f; // seconds
        [SerializeField] private float _detectionAccuracy = 0.85f;
        [SerializeField] private float _falsePositiveRate = 0.05f;
        
        [Header("Detection Sensitivity")]
        [SerializeField] private float _visualDetectionSensitivity = 0.8f;
        [SerializeField] private float _pheromoneSensitivity = 0.9f;
        [SerializeField] private float _damagePatternSensitivity = 0.7f;
        [SerializeField] private float _behavioralSensitivity = 0.6f;
        
        [Header("Environmental Factors")]
        [SerializeField] private float _temperatureInfluence = 0.3f;
        [SerializeField] private float _humidityInfluence = 0.4f;
        [SerializeField] private float _lightInfluence = 0.2f;
        [SerializeField] private float _airflowInfluence = 0.1f;
        
        [Header("Cannabis-Specific Detection")]
        [SerializeField] private bool _enableTrichomeMonitoring = true;
        [SerializeField] private bool _enableBudInspection = true;
        [SerializeField] private bool _enableLeafSurfaceScanning = true;
        [SerializeField] private bool _enableRootZoneMonitoring = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onPestDetected;
        [SerializeField] private SimpleGameEventSO _onEarlyWarning;
        [SerializeField] private SimpleGameEventSO _onScanComplete;
        [SerializeField] private SimpleGameEventSO _onFalseAlarm;
        
        // Core detection data
        private Dictionary<string, PlantScanData> _plantScanHistory = new Dictionary<string, PlantScanData>();
        private Dictionary<string, List<DetectionResult>> _detectionHistory = new Dictionary<string, List<DetectionResult>>();
        private Dictionary<ProjectChimera.Data.IPM.PestType, PestDetectionProfile> _pestProfiles = new Dictionary<ProjectChimera.Data.IPM.PestType, PestDetectionProfile>();
        private Dictionary<string, EnvironmentalRiskFactor> _environmentalRisks = new Dictionary<string, EnvironmentalRiskFactor>();
        
        // Detection systems
        private VisualDetectionSystem _visualSystem;
        private PheromoneDetectionSystem _pheromoneSystem;
        private DamagePatternAnalyzer _damageAnalyzer;
        private BehavioralPatternDetector _behaviorDetector;
        private PredictiveAnalyticsEngine _predictiveEngine;
        
        // Performance metrics
        private DetectionMetrics _metrics;
        private float _lastScanTime = 0f;
        private int _totalScansPerformed = 0;
        private int _totalPestsDetected = 0;
        private int _falsePositives = 0;
        private int _earlyWarningsIssued = 0;
        
        // Events
        public System.Action<string, DetectionResult> OnPestDetected;
        public System.Action<string, EarlyWarning> OnEarlyWarning;
        public System.Action<string, ScanResult> OnScanComplete;
        public System.Action<string, FalseAlarmData> OnFalseAlarm;
        
        // Properties
        public override string ManagerName => "Pest Detection System";
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalScansPerformed => _totalScansPerformed;
        public int TotalPestsDetected => _totalPestsDetected;
        public int FalsePositives => _falsePositives;
        public int EarlyWarningsIssued => _earlyWarningsIssued;
        public float DetectionAccuracy => _detectionAccuracy;
        public float ActualAccuracy => CalculateActualAccuracy();
        public DetectionMetrics Metrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeDetectionSystems();
            InitializePestProfiles();
            InitializeEnvironmentalRisks();
            
            _metrics = new DetectionMetrics();
            
            LogInfo("Pest Detection System initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Automatic scanning cycle
            if (_enableAutomaticScanning && currentTime - _lastScanTime >= _scanInterval)
            {
                PerformSystemWideScan();
                _lastScanTime = currentTime;
            }
            
            // Update predictive analytics
            if (_enablePredictiveAnalytics)
            {
                _predictiveEngine.UpdatePredictions(Time.deltaTime);
            }
            
            // Process early warning system
            if (_enableEarlyWarning)
            {
                ProcessEarlyWarnings();
            }
            
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            _plantScanHistory.Clear();
            _detectionHistory.Clear();
            _pestProfiles.Clear();
            _environmentalRisks.Clear();
            
            LogInfo("Pest Detection System shutdown completed");
        }
        
        /// <summary>
        /// Perform comprehensive pest scan on a specific plant
        /// </summary>
        public DetectionResult ScanPlant(string plantId, Vector3 plantPosition)
        {
            var scanData = new PlantScanData
            {
                PlantId = plantId,
                ScanTime = DateTime.Now,
                PlantPosition = plantPosition,
                EnvironmentalConditions = GetEnvironmentalConditions(plantPosition),
                ScanMethods = GetActiveScanMethods()
            };
            
            var result = new DetectionResult
            {
                PlantId = plantId,
                ScanId = Guid.NewGuid().ToString(),
                ScanTime = DateTime.Now,
                DetectedPests = new List<PestDetectionData>(),
                Confidence = 0f,
                RiskLevel = RiskLevel.Low
            };
            
            // Visual detection
            if (_visualSystem.IsActive)
            {
                var visualResults = _visualSystem.PerformVisualScan(scanData);
                result.DetectedPests.AddRange(visualResults);
            }
            
            // Pheromone detection
            if (_pheromoneSystem.IsActive)
            {
                var pheromoneResults = _pheromoneSystem.DetectPheromones(scanData);
                result.DetectedPests.AddRange(pheromoneResults);
            }
            
            // Damage pattern analysis
            if (_damageAnalyzer.IsActive)
            {
                var damageResults = _damageAnalyzer.AnalyzeDamagePatterns(scanData);
                result.DetectedPests.AddRange(damageResults);
            }
            
            // Behavioral pattern detection
            if (_behaviorDetector.IsActive)
            {
                var behaviorResults = _behaviorDetector.DetectBehavioralPatterns(scanData);
                result.DetectedPests.AddRange(behaviorResults);
            }
            
            // Calculate overall confidence and risk level
            result.Confidence = CalculateOverallConfidence(result.DetectedPests);
            result.RiskLevel = CalculateRiskLevel(result.DetectedPests, scanData.EnvironmentalConditions);
            
            // Apply environmental risk factors
            ApplyEnvironmentalRiskFactors(result, scanData.EnvironmentalConditions);
            
            // Store scan data and results
            _plantScanHistory[plantId] = scanData;
            
            if (!_detectionHistory.ContainsKey(plantId))
            {
                _detectionHistory[plantId] = new List<DetectionResult>();
            }
            _detectionHistory[plantId].Add(result);
            
            // Update metrics
            _totalScansPerformed++;
            if (result.DetectedPests.Count > 0)
            {
                _totalPestsDetected += result.DetectedPests.Count;
                
                // Trigger events
                OnPestDetected?.Invoke(plantId, result);
                _onPestDetected?.Raise();
            }
            
            // Check for false alarms
            if (result.Confidence < 0.3f && result.DetectedPests.Count > 0)
            {
                ProcessFalseAlarm(plantId, result);
            }
            
            OnScanComplete?.Invoke(plantId, new ScanResult { Result = result, ScanData = scanData });
            _onScanComplete?.Raise();
            
            LogInfo($"Plant scan completed: {plantId} - {result.DetectedPests.Count} threats detected");
            
            return result;
        }
        
        /// <summary>
        /// Get detection history for a specific plant
        /// </summary>
        public List<DetectionResult> GetDetectionHistory(string plantId)
        {
            return _detectionHistory.ContainsKey(plantId) ? 
                new List<DetectionResult>(_detectionHistory[plantId]) : 
                new List<DetectionResult>();
        }
        
        /// <summary>
        /// Get comprehensive detection report for all plants
        /// </summary>
        public DetectionSystemReport GetSystemReport()
        {
            return new DetectionSystemReport
            {
                TotalScansPerformed = _totalScansPerformed,
                TotalPestsDetected = _totalPestsDetected,
                FalsePositives = _falsePositives,
                EarlyWarningsIssued = _earlyWarningsIssued,
                DetectionAccuracy = _detectionAccuracy,
                ActualAccuracy = ActualAccuracy,
                ActiveScanMethods = GetActiveScanMethods().Count,
                MonitoredPlants = _plantScanHistory.Count,
                AverageRiskLevel = CalculateAverageRiskLevel(),
                TopThreats = GetTopThreats(),
                ReportDate = DateTime.Now
            };
        }
        
        /// <summary>
        /// Configure detection sensitivity for specific pest types
        /// </summary>
        public void ConfigurePestSensitivity(ProjectChimera.Data.IPM.PestType pestType, float sensitivity)
        {
            if (_pestProfiles.ContainsKey(pestType))
            {
                _pestProfiles[pestType].DetectionSensitivity = Mathf.Clamp01(sensitivity);
                LogInfo($"Updated {pestType} detection sensitivity to {sensitivity:P0}");
            }
        }
        
        /// <summary>
        /// Enable or disable specific detection methods
        /// </summary>
        public void ConfigureDetectionMethod(DetectionMethod method, bool enabled)
        {
            switch (method)
            {
                case DetectionMethod.Visual:
                    _visualSystem.SetActive(enabled);
                    break;
                case DetectionMethod.Pheromone:
                    _pheromoneSystem.SetActive(enabled);
                    break;
                case DetectionMethod.DamagePattern:
                    _damageAnalyzer.SetActive(enabled);
                    break;
                case DetectionMethod.Behavioral:
                    _behaviorDetector.SetActive(enabled);
                    break;
            }
            
            LogInfo($"{method} detection {(enabled ? "enabled" : "disabled")}");
        }
        
        #region Private Implementation
        
        private void InitializeDetectionSystems()
        {
            _visualSystem = new VisualDetectionSystem(_visualDetectionSensitivity);
            _pheromoneSystem = new PheromoneDetectionSystem(_pheromoneSensitivity);
            _damageAnalyzer = new DamagePatternAnalyzer(_damagePatternSensitivity);
            _behaviorDetector = new BehavioralPatternDetector(_behavioralSensitivity);
            _predictiveEngine = new PredictiveAnalyticsEngine();
        }
        
        private void InitializePestProfiles()
        {
            var pestTypes = Enum.GetValues(typeof(ProjectChimera.Data.IPM.PestType));
            foreach (ProjectChimera.Data.IPM.PestType pestType in pestTypes)
            {
                _pestProfiles[pestType] = CreatePestProfile(pestType);
            }
        }
        
        private PestDetectionProfile CreatePestProfile(ProjectChimera.Data.IPM.PestType pestType)
        {
            return pestType switch
            {
                ProjectChimera.Data.IPM.PestType.Aphids => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.8f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual, DetectionMethod.Pheromone },
                    TypicalSigns = new List<string> { "Sticky honeydew", "Curled leaves", "Yellowing", "Clusters on stems" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 22f }, { "Humidity", 0.6f } }
                },
                ProjectChimera.Data.IPM.PestType.SpiderMites => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.7f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual, DetectionMethod.DamagePattern },
                    TypicalSigns = new List<string> { "Fine webbing", "Stippled leaves", "Bronze coloration", "Tiny moving dots" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 27f }, { "Humidity", 0.4f } }
                },
                ProjectChimera.Data.IPM.PestType.Thrips => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.6f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual, DetectionMethod.Behavioral },
                    TypicalSigns = new List<string> { "Silver streaks", "Black specks", "Leaf scarring", "Rapid movement" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 25f }, { "Humidity", 0.5f } }
                },
                ProjectChimera.Data.IPM.PestType.Whiteflies => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.9f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual, DetectionMethod.Behavioral },
                    TypicalSigns = new List<string> { "White flying insects", "Sticky honeydew", "Yellowing leaves", "Clouds when disturbed" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 26f }, { "Humidity", 0.7f } }
                },
                ProjectChimera.Data.IPM.PestType.Fungusgnats => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.8f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual, DetectionMethod.Behavioral },
                    TypicalSigns = new List<string> { "Small flying insects", "Larvae in soil", "Root damage", "Slow plant growth" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 24f }, { "Humidity", 0.8f } }
                },
                _ => new PestDetectionProfile
                {
                    PestType = pestType,
                    DetectionSensitivity = 0.7f,
                    PreferredDetectionMethods = new List<DetectionMethod> { DetectionMethod.Visual },
                    TypicalSigns = new List<string> { "General pest damage" },
                    EnvironmentalPreferences = new Dictionary<string, float> { { "Temperature", 25f }, { "Humidity", 0.6f } }
                }
            };
        }
        
        private void InitializeEnvironmentalRisks()
        {
            _environmentalRisks["High_Temperature"] = new EnvironmentalRiskFactor
            {
                Name = "High Temperature",
                RiskMultiplier = 1.5f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType> { ProjectChimera.Data.IPM.PestType.SpiderMites, ProjectChimera.Data.IPM.PestType.Thrips },
                ThresholdValue = 28f
            };
            
            _environmentalRisks["High_Humidity"] = new EnvironmentalRiskFactor
            {
                Name = "High Humidity",
                RiskMultiplier = 1.3f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType> { ProjectChimera.Data.IPM.PestType.Fungusgnats, ProjectChimera.Data.IPM.PestType.Whiteflies },
                ThresholdValue = 0.75f
            };
            
            _environmentalRisks["Low_Airflow"] = new EnvironmentalRiskFactor
            {
                Name = "Low Airflow",
                RiskMultiplier = 1.2f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType> { ProjectChimera.Data.IPM.PestType.Aphids, ProjectChimera.Data.IPM.PestType.Whiteflies },
                ThresholdValue = 0.3f
            };
        }
        
        private void PerformSystemWideScan()
        {
            // Get all registered plants from the IPM framework
            var ipmFramework = GameManager.Instance?.GetManager<IPMFramework>();
            if (ipmFramework != null)
            {
                // This would scan all plants in the system
                // For now, we'll simulate with a few test plants
                var testPlants = new List<string> { "plant_001", "plant_002", "plant_003" };
                
                foreach (var plantId in testPlants)
                {
                    var randomPosition = new Vector3(
                        UnityEngine.Random.Range(-5f, 5f),
                        0f,
                        UnityEngine.Random.Range(-5f, 5f)
                    );
                    
                    ScanPlant(plantId, randomPosition);
                }
            }
        }
        
        private void ProcessEarlyWarnings()
        {
            foreach (var plantHistory in _detectionHistory)
            {
                var recentScans = plantHistory.Value.Where(r => 
                    (DateTime.Now - r.ScanTime).TotalHours < 24).ToList();
                
                if (recentScans.Count >= 3)
                {
                    var trendingUp = IsTrendingUp(recentScans);
                    var highRiskConditions = HasHighRiskConditions(plantHistory.Key);
                    
                    if (trendingUp || highRiskConditions)
                    {
                        IssueEarlyWarning(plantHistory.Key, recentScans);
                    }
                }
            }
        }
        
        private bool IsTrendingUp(List<DetectionResult> recentScans)
        {
            if (recentScans.Count < 3) return false;
            
            var sortedScans = recentScans.OrderBy(r => r.ScanTime).ToList();
            var firstHalf = sortedScans.Take(sortedScans.Count / 2).Average(r => r.DetectedPests.Count);
            var secondHalf = sortedScans.Skip(sortedScans.Count / 2).Average(r => r.DetectedPests.Count);
            
            return secondHalf > firstHalf * 1.5f;
        }
        
        private bool HasHighRiskConditions(string plantId)
        {
            if (!_plantScanHistory.ContainsKey(plantId)) return false;
            
            var scanData = _plantScanHistory[plantId];
            var conditions = scanData.EnvironmentalConditions;
            
            return conditions.Temperature > 28f || 
                   conditions.Humidity > 0.75f || 
                   conditions.AirFlow < 0.3f;
        }
        
        private void IssueEarlyWarning(string plantId, List<DetectionResult> recentScans)
        {
            var warning = new EarlyWarning
            {
                PlantId = plantId,
                WarningType = "Increasing Pest Activity",
                Severity = CalculateWarningSeverity(recentScans),
                RecommendedActions = GenerateRecommendations(recentScans),
                IssuedAt = DateTime.Now
            };
            
            _earlyWarningsIssued++;
            
            OnEarlyWarning?.Invoke(plantId, warning);
            _onEarlyWarning?.Raise();
            
            LogWarning($"Early warning issued for plant {plantId}: {warning.WarningType}");
        }
        
        private float CalculateOverallConfidence(List<PestDetectionData> detectedPests)
        {
            if (detectedPests.Count == 0) return 0f;
            
            return detectedPests.Average(p => p.Confidence);
        }
        
        private RiskLevel CalculateRiskLevel(List<PestDetectionData> detectedPests, EnvironmentalConditions conditions)
        {
            if (detectedPests.Count == 0) return RiskLevel.Low;
            
            var avgConfidence = detectedPests.Average(p => p.Confidence);
            var pestCount = detectedPests.Count;
            
            if (avgConfidence > 0.8f && pestCount > 3) return RiskLevel.Critical;
            if (avgConfidence > 0.6f && pestCount > 2) return RiskLevel.High;
            if (avgConfidence > 0.4f && pestCount > 1) return RiskLevel.Moderate;
            
            return RiskLevel.Low;
        }
        
        private void ApplyEnvironmentalRiskFactors(DetectionResult result, EnvironmentalConditions conditions)
        {
            foreach (var riskFactor in _environmentalRisks.Values)
            {
                bool riskActive = riskFactor.Name switch
                {
                    "High Temperature" => conditions.Temperature > riskFactor.ThresholdValue,
                    "High Humidity" => conditions.Humidity > riskFactor.ThresholdValue,
                    "Low Airflow" => conditions.AirFlow < riskFactor.ThresholdValue,
                    _ => false
                };
                
                if (riskActive)
                {
                    foreach (var pest in result.DetectedPests)
                    {
                        if (riskFactor.AffectedPests.Contains(pest.PestType))
                        {
                            pest.Confidence *= riskFactor.RiskMultiplier;
                            pest.Confidence = Mathf.Clamp01(pest.Confidence);
                        }
                    }
                }
            }
        }
        
        private List<DetectionMethod> GetActiveScanMethods()
        {
            var methods = new List<DetectionMethod>();
            
            if (_visualSystem.IsActive) methods.Add(DetectionMethod.Visual);
            if (_pheromoneSystem.IsActive) methods.Add(DetectionMethod.Pheromone);
            if (_damageAnalyzer.IsActive) methods.Add(DetectionMethod.DamagePattern);
            if (_behaviorDetector.IsActive) methods.Add(DetectionMethod.Behavioral);
            
            return methods;
        }
        
        private EnvironmentalConditions GetEnvironmentalConditions(Vector3 position)
        {
            // Simulate environmental conditions - in real implementation, this would
            // query the actual environmental systems
            return new EnvironmentalConditions
            {
                Temperature = UnityEngine.Random.Range(20f, 30f),
                Humidity = UnityEngine.Random.Range(0.4f, 0.8f),
                AirFlow = UnityEngine.Random.Range(0.2f, 1.0f),
                LightIntensity = UnityEngine.Random.Range(400f, 800f),
                CO2Level = UnityEngine.Random.Range(800f, 1500f)
            };
        }
        
        private void ProcessFalseAlarm(string plantId, DetectionResult result)
        {
            _falsePositives++;
            
            var falseAlarm = new FalseAlarmData
            {
                PlantId = plantId,
                ScanId = result.ScanId,
                DetectedPests = result.DetectedPests,
                ActualConfidence = result.Confidence,
                ReasonForFalseAlarm = "Low confidence detection",
                OccurredAt = DateTime.Now
            };
            
            OnFalseAlarm?.Invoke(plantId, falseAlarm);
            _onFalseAlarm?.Raise();
        }
        
        private float CalculateActualAccuracy()
        {
            if (_totalScansPerformed == 0) return 0f;
            
            var correctDetections = _totalScansPerformed - _falsePositives;
            return (float)correctDetections / _totalScansPerformed;
        }
        
        private void UpdateMetrics()
        {
            _metrics.TotalScansPerformed = _totalScansPerformed;
            _metrics.TotalPestsDetected = _totalPestsDetected;
            _metrics.FalsePositives = _falsePositives;
            _metrics.EarlyWarningsIssued = _earlyWarningsIssued;
            _metrics.DetectionAccuracy = _detectionAccuracy;
            _metrics.ActualAccuracy = ActualAccuracy;
            _metrics.LastUpdated = DateTime.Now;
        }
        
        private float CalculateAverageRiskLevel()
        {
            if (_detectionHistory.Count == 0) return 0f;
            
            var allResults = _detectionHistory.Values.SelectMany(r => r).ToList();
            if (allResults.Count == 0) return 0f;
            
            return allResults.Average(r => (float)r.RiskLevel);
        }
        
        private List<ProjectChimera.Data.IPM.PestType> GetTopThreats()
        {
            var allDetections = _detectionHistory.Values
                .SelectMany(r => r)
                .SelectMany(r => r.DetectedPests)
                .GroupBy(p => p.PestType)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
            
            return allDetections;
        }
        
        private WarningSeverity CalculateWarningSeverity(List<DetectionResult> recentScans)
        {
            var avgPests = recentScans.Average(r => r.DetectedPests.Count);
            var avgConfidence = recentScans.Average(r => r.Confidence);
            
            if (avgPests > 5 && avgConfidence > 0.8f) return WarningSeverity.Critical;
            if (avgPests > 3 && avgConfidence > 0.6f) return WarningSeverity.High;
            if (avgPests > 1 && avgConfidence > 0.4f) return WarningSeverity.Moderate;
            
            return WarningSeverity.Low;
        }
        
        private List<string> GenerateRecommendations(List<DetectionResult> recentScans)
        {
            var recommendations = new List<string>();
            
            var commonPests = recentScans
                .SelectMany(r => r.DetectedPests)
                .GroupBy(p => p.PestType)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToList();
            
            foreach (var pest in commonPests)
            {
                recommendations.Add($"Deploy targeted treatment for {pest}");
                recommendations.Add($"Increase monitoring frequency for {pest}");
            }
            
            recommendations.Add("Review environmental conditions");
            recommendations.Add("Consider preventive measures");
            
            return recommendations;
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class PlantScanData
    {
        public string PlantId;
        public DateTime ScanTime;
        public Vector3 PlantPosition;
        public EnvironmentalConditions EnvironmentalConditions;
        public List<DetectionMethod> ScanMethods = new List<DetectionMethod>();
    }
    
    [System.Serializable]
    public class DetectionResult
    {
        public string PlantId;
        public string ScanId;
        public DateTime ScanTime;
        public List<PestDetectionData> DetectedPests = new List<PestDetectionData>();
        public float Confidence;
        public RiskLevel RiskLevel;
    }
    
    [System.Serializable]
    public class PestDetectionData
    {
        public ProjectChimera.Data.IPM.PestType PestType;
        public float Confidence;
        public Vector3 Location;
        public List<string> DetectedSigns = new List<string>();
        public DetectionMethod DetectionMethod;
        public DateTime FirstDetected;
        public int EstimatedPopulation;
    }
    
    [System.Serializable]
    public class PestDetectionProfile
    {
        public ProjectChimera.Data.IPM.PestType PestType;
        public float DetectionSensitivity;
        public List<DetectionMethod> PreferredDetectionMethods = new List<DetectionMethod>();
        public List<string> TypicalSigns = new List<string>();
        public Dictionary<string, float> EnvironmentalPreferences = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class EnvironmentalRiskFactor
    {
        public string Name;
        public float RiskMultiplier;
        public List<ProjectChimera.Data.IPM.PestType> AffectedPests = new List<ProjectChimera.Data.IPM.PestType>();
        public float ThresholdValue;
        public bool IsActive = false;
    }
    
    [System.Serializable]
    public class EarlyWarning
    {
        public string PlantId;
        public string WarningType;
        public WarningSeverity Severity;
        public List<string> RecommendedActions = new List<string>();
        public DateTime IssuedAt;
    }
    
    [System.Serializable]
    public class ScanResult
    {
        public DetectionResult Result;
        public PlantScanData ScanData;
    }
    
    [System.Serializable]
    public class FalseAlarmData
    {
        public string PlantId;
        public string ScanId;
        public List<PestDetectionData> DetectedPests = new List<PestDetectionData>();
        public float ActualConfidence;
        public string ReasonForFalseAlarm;
        public DateTime OccurredAt;
    }
    
    [System.Serializable]
    public class DetectionSystemReport
    {
        public int TotalScansPerformed;
        public int TotalPestsDetected;
        public int FalsePositives;
        public int EarlyWarningsIssued;
        public float DetectionAccuracy;
        public float ActualAccuracy;
        public int ActiveScanMethods;
        public int MonitoredPlants;
        public float AverageRiskLevel;
        public List<ProjectChimera.Data.IPM.PestType> TopThreats = new List<ProjectChimera.Data.IPM.PestType>();
        public DateTime ReportDate;
    }
    
    [System.Serializable]
    public class DetectionMetrics
    {
        public int TotalScansPerformed;
        public int TotalPestsDetected;
        public int FalsePositives;
        public int EarlyWarningsIssued;
        public float DetectionAccuracy;
        public float ActualAccuracy;
        public DateTime LastUpdated;
        
        public DetectionMetrics()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    // Enums
    public enum DetectionMethod
    {
        Visual,
        Pheromone,
        DamagePattern,
        Behavioral
    }
    
    public enum RiskLevel
    {
        Low,
        Moderate,
        High,
        Critical
    }
    
    public enum WarningSeverity
    {
        Low,
        Moderate,
        High,
        Critical
    }
    
    // Supporting system classes (simplified implementations)
    public class VisualDetectionSystem
    {
        private float _sensitivity;
        private bool _isActive = true;
        
        public bool IsActive => _isActive;
        
        public VisualDetectionSystem(float sensitivity)
        {
            _sensitivity = sensitivity;
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        public List<PestDetectionData> PerformVisualScan(PlantScanData scanData)
        {
            var results = new List<PestDetectionData>();
            
            // Simulate visual detection
            if (UnityEngine.Random.Range(0f, 1f) < _sensitivity * 0.3f)
            {
                results.Add(new PestDetectionData
                {
                    PestType = ProjectChimera.Data.IPM.PestType.Aphids,
                    Confidence = UnityEngine.Random.Range(0.6f, 0.9f),
                    Location = scanData.PlantPosition,
                    DetectedSigns = new List<string> { "Visible insects", "Sticky residue" },
                    DetectionMethod = DetectionMethod.Visual,
                    FirstDetected = DateTime.Now,
                    EstimatedPopulation = UnityEngine.Random.Range(10, 50)
                });
            }
            
            return results;
        }
    }
    
    public class PheromoneDetectionSystem
    {
        private float _sensitivity;
        private bool _isActive = true;
        
        public bool IsActive => _isActive;
        
        public PheromoneDetectionSystem(float sensitivity)
        {
            _sensitivity = sensitivity;
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        public List<PestDetectionData> DetectPheromones(PlantScanData scanData)
        {
            var results = new List<PestDetectionData>();
            
            // Simulate pheromone detection
            if (UnityEngine.Random.Range(0f, 1f) < _sensitivity * 0.2f)
            {
                results.Add(new PestDetectionData
                {
                    PestType = ProjectChimera.Data.IPM.PestType.Caterpillars,
                    Confidence = UnityEngine.Random.Range(0.7f, 0.95f),
                    Location = scanData.PlantPosition,
                    DetectedSigns = new List<string> { "Pheromone signature detected" },
                    DetectionMethod = DetectionMethod.Pheromone,
                    FirstDetected = DateTime.Now,
                    EstimatedPopulation = UnityEngine.Random.Range(5, 20)
                });
            }
            
            return results;
        }
    }
    
    public class DamagePatternAnalyzer
    {
        private float _sensitivity;
        private bool _isActive = true;
        
        public bool IsActive => _isActive;
        
        public DamagePatternAnalyzer(float sensitivity)
        {
            _sensitivity = sensitivity;
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        public List<PestDetectionData> AnalyzeDamagePatterns(PlantScanData scanData)
        {
            var results = new List<PestDetectionData>();
            
            // Simulate damage pattern analysis
            if (UnityEngine.Random.Range(0f, 1f) < _sensitivity * 0.25f)
            {
                results.Add(new PestDetectionData
                {
                    PestType = ProjectChimera.Data.IPM.PestType.SpiderMites,
                    Confidence = UnityEngine.Random.Range(0.5f, 0.8f),
                    Location = scanData.PlantPosition,
                    DetectedSigns = new List<string> { "Stippling damage pattern", "Webbing traces" },
                    DetectionMethod = DetectionMethod.DamagePattern,
                    FirstDetected = DateTime.Now,
                    EstimatedPopulation = UnityEngine.Random.Range(20, 100)
                });
            }
            
            return results;
        }
    }
    
    public class BehavioralPatternDetector
    {
        private float _sensitivity;
        private bool _isActive = true;
        
        public bool IsActive => _isActive;
        
        public BehavioralPatternDetector(float sensitivity)
        {
            _sensitivity = sensitivity;
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        public List<PestDetectionData> DetectBehavioralPatterns(PlantScanData scanData)
        {
            var results = new List<PestDetectionData>();
            
            // Simulate behavioral pattern detection
            if (UnityEngine.Random.Range(0f, 1f) < _sensitivity * 0.15f)
            {
                results.Add(new PestDetectionData
                {
                    PestType = ProjectChimera.Data.IPM.PestType.Whiteflies,
                    Confidence = UnityEngine.Random.Range(0.8f, 0.95f),
                    Location = scanData.PlantPosition,
                    DetectedSigns = new List<string> { "Erratic flight patterns", "Clustering behavior" },
                    DetectionMethod = DetectionMethod.Behavioral,
                    FirstDetected = DateTime.Now,
                    EstimatedPopulation = UnityEngine.Random.Range(30, 80)
                });
            }
            
            return results;
        }
    }
    
    public class PredictiveAnalyticsEngine
    {
        public void UpdatePredictions(float deltaTime)
        {
            // Placeholder for predictive analytics
            // This would analyze trends, environmental data, and historical patterns
            // to predict future pest outbreaks
        }
    }
}