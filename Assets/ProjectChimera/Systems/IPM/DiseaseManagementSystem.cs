using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using IPMRiskLevel = ProjectChimera.Data.IPM.RiskLevel;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Advanced disease management system for cannabis cultivation.
    /// Provides comprehensive disease detection, identification, progression monitoring,
    /// and treatment recommendations for common cannabis diseases.
    /// </summary>
    public class DiseaseManagementSystem : ChimeraManager
    {
        [Header("Disease Detection Configuration")]
        [SerializeField] private bool _enableAutomaticScanning = true;
        [SerializeField] private bool _enablePredictiveModeling = true;
        [SerializeField] private bool _enableEarlyWarning = true;
        [SerializeField] private float _scanInterval = 60f; // seconds
        [SerializeField] private float _detectionAccuracy = 0.85f;
        [SerializeField] private float _falsePositiveRate = 0.03f;
        
        [Header("Disease Progression Settings")]
        [SerializeField] private float _progressionSpeed = 1.0f;
        [SerializeField] private float _environmentalImpact = 0.3f;
        [SerializeField] private float _plantResistanceWeight = 0.4f;
        [SerializeField] private float _treatmentEffectiveness = 0.8f;
        
        [Header("Cannabis-Specific Disease Monitoring")]
        [SerializeField] private bool _enablePowderyMildewDetection = true;
        [SerializeField] private bool _enableBotrytisDetection = true;
        [SerializeField] private bool _enableRootRotDetection = true;
        [SerializeField] private bool _enableBudRotDetection = true;
        [SerializeField] private bool _enableLeafSpotDetection = true;
        
        [Header("Environmental Risk Factors")]
        [SerializeField] private float _humidityRiskThreshold = 0.7f;
        [SerializeField] private float _temperatureRiskThreshold = 28f;
        [SerializeField] private float _airflowRiskThreshold = 0.3f;
        [SerializeField] private float _moistureRiskThreshold = 0.8f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onDiseaseDetected;
        [SerializeField] private SimpleGameEventSO _onDiseaseProgressed;
        [SerializeField] private SimpleGameEventSO _onTreatmentApplied;
        [SerializeField] private SimpleGameEventSO _onDiseaseEradicated;
        
        // Core disease data
        private Dictionary<string, PlantDiseaseData> _plantDiseases = new Dictionary<string, PlantDiseaseData>();
        private Dictionary<string, List<DiseaseDetectionResult>> _detectionHistory = new Dictionary<string, List<DiseaseDetectionResult>>();
        private Dictionary<DiseaseType, DiseaseProfile> _diseaseProfiles = new Dictionary<DiseaseType, DiseaseProfile>();
        private Dictionary<string, EnvironmentalRiskFactor> _environmentalRisks = new Dictionary<string, EnvironmentalRiskFactor>();
        
        // Disease detection systems
        private VisualDiseaseDetector _visualDetector;
        private MicroscopicAnalyzer _microscopicAnalyzer;
        private ChemicalSensorArray _chemicalSensors;
        private EnvironmentalRiskAnalyzer _riskAnalyzer;
        private DiseaseProgressionPredictor _progressionPredictor;
        
        // Performance metrics
        private DiseaseMetrics _metrics;
        private float _lastScanTime = 0f;
        private int _totalDiseasesDetected = 0;
        private int _totalTreatmentsApplied = 0;
        private int _diseasesEradicated = 0;
        private int _falsePositives = 0;
        
        // Events
        public System.Action<string, DiseaseDetectionResult> OnDiseaseDetected;
        public System.Action<string, DiseaseProgressionData> OnDiseaseProgressed;
        public System.Action<string, DiseaseTreatmentData> OnTreatmentApplied;
        public System.Action<string, DiseaseType> OnDiseaseEradicated;
        
        // Properties
        public override string ManagerName => "Disease Management System";
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalDiseasesDetected => _totalDiseasesDetected;
        public int TotalTreatmentsApplied => _totalTreatmentsApplied;
        public int DiseasesEradicated => _diseasesEradicated;
        public int FalsePositives => _falsePositives;
        public float DetectionAccuracy => _detectionAccuracy;
        public float ActualAccuracy => CalculateActualAccuracy();
        public DiseaseMetrics Metrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeDiseaseDetectionSystems();
            InitializeDiseaseProfiles();
            InitializeEnvironmentalRisks();
            
            _metrics = new DiseaseMetrics();
            
            LogInfo("Disease Management System initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Automatic disease scanning
            if (_enableAutomaticScanning && currentTime - _lastScanTime >= _scanInterval)
            {
                PerformAutomaticDiseaseScanning();
                _lastScanTime = currentTime;
            }
            
            // Update disease progression
            UpdateDiseaseProgression();
            
            // Update environmental risks
            UpdateEnvironmentalRisks();
            
            // Update predictive models
            if (_enablePredictiveModeling)
            {
                UpdatePredictiveModels();
            }
            
            // Update metrics
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            _plantDiseases.Clear();
            _detectionHistory.Clear();
            _diseaseProfiles.Clear();
            _environmentalRisks.Clear();
            
            LogInfo("Disease Management System shutdown completed");
        }
        
        /// <summary>
        /// Perform comprehensive disease scan on a specific plant
        /// </summary>
        public DiseaseDetectionResult ScanPlant(string plantId, Vector3 plantPosition)
        {
            var result = new DiseaseDetectionResult
            {
                PlantId = plantId,
                ScanId = System.Guid.NewGuid().ToString(),
                DetectedDiseases = new List<DiseaseDetectionData>(),
                DetectionTime = DateTime.Now,
                Confidence = 0f,
                RiskLevel = IPMRiskLevel.Low
            };
            
            // Visual disease detection
            if (_enableAutomaticScanning)
            {
                var visualResults = _visualDetector.PerformVisualAnalysis(plantId, plantPosition);
                result.DetectedDiseases.AddRange(visualResults);
            }
            
            // Microscopic analysis
            var microscopicResults = _microscopicAnalyzer.PerformMicroscopicAnalysis(plantId, plantPosition);
            result.DetectedDiseases.AddRange(microscopicResults);
            
            // Chemical sensor analysis
            var chemicalResults = _chemicalSensors.PerformChemicalAnalysis(plantId, plantPosition);
            result.DetectedDiseases.AddRange(chemicalResults);
            
            // Environmental risk analysis
            var environmentalResults = _riskAnalyzer.AnalyzeEnvironmentalRisks(plantId, plantPosition);
            result.DetectedDiseases.AddRange(environmentalResults);
            
            // Calculate overall confidence and risk level
            result.Confidence = CalculateOverallConfidence(result.DetectedDiseases);
            result.RiskLevel = CalculateOverallRiskLevel(result.DetectedDiseases);
            
            // Store detection history
            if (!_detectionHistory.ContainsKey(plantId))
            {
                _detectionHistory[plantId] = new List<DiseaseDetectionResult>();
            }
            _detectionHistory[plantId].Add(result);
            
            // Update plant disease data
            UpdatePlantDiseaseData(plantId, result);
            
            // Trigger events
            if (result.DetectedDiseases.Count > 0)
            {
                OnDiseaseDetected?.Invoke(plantId, result);
                _onDiseaseDetected?.Raise();
                _totalDiseasesDetected++;
            }
            
            LogInfo($"Disease scan completed for plant {plantId}: {result.DetectedDiseases.Count} diseases detected");
            
            return result;
        }
        
        /// <summary>
        /// Get disease detection history for a specific plant
        /// </summary>
        public List<DiseaseDetectionResult> GetDetectionHistory(string plantId)
        {
            return _detectionHistory.ContainsKey(plantId) ? _detectionHistory[plantId] : new List<DiseaseDetectionResult>();
        }
        
        /// <summary>
        /// Get current disease data for a plant
        /// </summary>
        public PlantDiseaseData GetPlantDiseaseData(string plantId)
        {
            return _plantDiseases.ContainsKey(plantId) ? _plantDiseases[plantId] : null;
        }
        
        /// <summary>
        /// Apply treatment to a diseased plant
        /// </summary>
        public DiseaseTreatmentResult ApplyTreatment(string plantId, DiseaseType diseaseType, TreatmentType treatmentType)
        {
            var treatmentData = new DiseaseTreatmentData
            {
                TreatmentId = System.Guid.NewGuid().ToString(),
                PlantId = plantId,
                DiseaseType = diseaseType,
                TreatmentType = treatmentType,
                ApplicationTime = DateTime.Now,
                ExpectedEffectiveness = CalculateTreatmentEffectiveness(diseaseType, treatmentType),
                ActualEffectiveness = 0f
            };
            
            var result = new DiseaseTreatmentResult
            {
                TreatmentId = treatmentData.TreatmentId,
                IsSuccessful = UnityEngine.Random.value < treatmentData.ExpectedEffectiveness,
                ResultMessage = treatmentData.IsSuccessful ? "Treatment applied successfully" : "Treatment partially effective",
                ExpectedEffectiveness = treatmentData.ExpectedEffectiveness,
                ActualEffectiveness = treatmentData.IsSuccessful ? treatmentData.ExpectedEffectiveness : treatmentData.ExpectedEffectiveness * 0.5f
            };
            
            // Update plant disease data
            if (_plantDiseases.ContainsKey(plantId))
            {
                var plantData = _plantDiseases[plantId];
                var diseaseToTreat = plantData.ActiveDiseases.FirstOrDefault(d => d.DiseaseType == diseaseType);
                
                if (diseaseToTreat != null)
                {
                    diseaseToTreat.TreatmentProgress += result.ActualEffectiveness;
                    diseaseToTreat.LastTreatmentDate = DateTime.Now;
                    
                    // Check if disease is eradicated
                    if (diseaseToTreat.TreatmentProgress >= 1.0f)
                    {
                        plantData.ActiveDiseases.Remove(diseaseToTreat);
                        plantData.EradicatedDiseases.Add(diseaseToTreat);
                        
                        OnDiseaseEradicated?.Invoke(plantId, diseaseType);
                        _onDiseaseEradicated?.Raise();
                        _diseasesEradicated++;
                        
                        LogInfo($"Disease {diseaseType} eradicated from plant {plantId}");
                    }
                }
            }
            
            // Trigger events
            OnTreatmentApplied?.Invoke(plantId, treatmentData);
            _onTreatmentApplied?.Raise();
            _totalTreatmentsApplied++;
            
            LogInfo($"Treatment {treatmentType} applied to plant {plantId} for {diseaseType}");
            
            return result;
        }
        
        /// <summary>
        /// Get comprehensive disease system report
        /// </summary>
        public DiseaseSystemReport GetSystemReport()
        {
            return new DiseaseSystemReport
            {
                TotalPlantsMonitored = _plantDiseases.Count,
                TotalDiseasesDetected = _totalDiseasesDetected,
                TotalTreatmentsApplied = _totalTreatmentsApplied,
                DiseasesEradicated = _diseasesEradicated,
                SystemEffectiveness = CalculateSystemEffectiveness(),
                PreventionSuccessRate = CalculatePreventionSuccessRate(),
                AverageDetectionAccuracy = _detectionAccuracy,
                ActiveDiseases = _plantDiseases.Values.Sum(p => p.ActiveDiseases.Count),
                CriticalDiseases = _plantDiseases.Values.Sum(p => p.ActiveDiseases.Count(d => d.Severity == DiseaseSeverity.Critical)),
                ReportDate = DateTime.Now
            };
        }
        
        /// <summary>
        /// Configure disease detection sensitivity
        /// </summary>
        public void ConfigureDiseaseDetection(DiseaseType diseaseType, float sensitivity)
        {
            if (_diseaseProfiles.ContainsKey(diseaseType))
            {
                _diseaseProfiles[diseaseType].DetectionSensitivity = Mathf.Clamp01(sensitivity);
                LogInfo($"Disease detection sensitivity for {diseaseType} set to {sensitivity:P0}");
            }
        }
        
        #region Private Implementation
        
        private void InitializeDiseaseDetectionSystems()
        {
            _visualDetector = new VisualDiseaseDetector(_detectionAccuracy);
            _microscopicAnalyzer = new MicroscopicAnalyzer(0.9f);
            _chemicalSensors = new ChemicalSensorArray(0.8f);
            _riskAnalyzer = new EnvironmentalRiskAnalyzer(0.7f);
            _progressionPredictor = new DiseaseProgressionPredictor();
        }
        
        private void InitializeDiseaseProfiles()
        {
            // Powdery Mildew Profile
            _diseaseProfiles[DiseaseType.PowderyMildew] = new DiseaseProfile
            {
                DiseaseType = DiseaseType.PowderyMildew,
                DetectionSensitivity = 0.8f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Microscopic },
                TypicalSymptoms = new List<string> { "White powdery spots", "Leaf yellowing", "Stunted growth" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Humidity", 0.6f }, { "Temperature", 24f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.Fungicide, TreatmentType.BiologicalControl, TreatmentType.EnvironmentalAdjustment },
                Severity = DiseaseSeverity.Moderate,
                ContagiousLevel = 0.7f,
                ProgressionRate = 0.3f
            };
            
            // Botrytis (Gray Mold) Profile
            _diseaseProfiles[DiseaseType.Botrytis] = new DiseaseProfile
            {
                DiseaseType = DiseaseType.Botrytis,
                DetectionSensitivity = 0.9f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Chemical },
                TypicalSymptoms = new List<string> { "Gray fuzzy mold", "Brown spots", "Bud rot", "Stem lesions" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Humidity", 0.8f }, { "Temperature", 20f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.Fungicide, TreatmentType.BiologicalControl, TreatmentType.PlantRemoval },
                Severity = DiseaseSeverity.High,
                ContagiousLevel = 0.8f,
                ProgressionRate = 0.5f
            };
            
            // Root Rot Profile
            _diseaseProfiles[DiseaseType.RootRot] = new DiseaseProfile
            {
                DiseaseType = DiseaseType.RootRot,
                DetectionSensitivity = 0.7f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Chemical },
                TypicalSymptoms = new List<string> { "Wilting", "Yellow leaves", "Brown roots", "Stunted growth" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Moisture", 0.9f }, { "Temperature", 22f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.Fungicide, TreatmentType.BiologicalControl, TreatmentType.EnvironmentalAdjustment },
                Severity = DiseaseSeverity.High,
                ContagiousLevel = 0.6f,
                ProgressionRate = 0.4f
            };
            
            // Bud Rot Profile
            _diseaseProfiles[DiseaseType.BudRot] = new DiseaseProfile
            {
                DiseaseType = DiseaseType.BudRot,
                DetectionSensitivity = 0.85f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Microscopic },
                TypicalSymptoms = new List<string> { "Brown/black buds", "Musty smell", "Fuzzy growth", "Bud discoloration" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Humidity", 0.75f }, { "Temperature", 18f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.PlantRemoval, TreatmentType.BiologicalControl, TreatmentType.EnvironmentalAdjustment },
                Severity = DiseaseSeverity.Critical,
                ContagiousLevel = 0.9f,
                ProgressionRate = 0.6f
            };
            
            // Leaf Spot Profile
            _diseaseProfiles[DiseaseType.LeafSpot] = new DiseaseProfile
            {
                DiseaseType = DiseaseType.LeafSpot,
                DetectionSensitivity = 0.75f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Microscopic },
                TypicalSymptoms = new List<string> { "Brown spots", "Yellow halos", "Leaf drop", "Necrotic tissue" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Humidity", 0.7f }, { "Temperature", 25f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.Fungicide, TreatmentType.BiologicalControl, TreatmentType.EnvironmentalAdjustment },
                Severity = DiseaseSeverity.Moderate,
                ContagiousLevel = 0.5f,
                ProgressionRate = 0.2f
            };
        }
        
        private void InitializeEnvironmentalRisks()
        {
            _environmentalRisks["HighHumidity"] = new EnvironmentalRiskFactor
            {
                Name = "High Humidity",
                RiskMultiplier = 1.8f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                ThresholdValue = _humidityRiskThreshold,
                IsActive = false
            };
            
            _environmentalRisks["PoorAirflow"] = new EnvironmentalRiskFactor
            {
                Name = "Poor Airflow",
                RiskMultiplier = 1.5f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                ThresholdValue = _airflowRiskThreshold,
                IsActive = false
            };
            
            _environmentalRisks["ExcessiveMoisture"] = new EnvironmentalRiskFactor
            {
                Name = "Excessive Moisture",
                RiskMultiplier = 2.0f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                ThresholdValue = _moistureRiskThreshold,
                IsActive = false
            };
            
            _environmentalRisks["TemperatureFluctuation"] = new EnvironmentalRiskFactor
            {
                Name = "Temperature Fluctuation",
                RiskMultiplier = 1.3f,
                AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                ThresholdValue = _temperatureRiskThreshold,
                IsActive = false
            };
        }
        
        private void PerformAutomaticDiseaseScanning()
        {
            var plantsToScan = _plantDiseases.Keys.ToList();
            
            foreach (var plantId in plantsToScan)
            {
                // Simulate plant position - in real implementation, this would come from the plant management system
                var plantPosition = new Vector3(
                    UnityEngine.Random.Range(-10f, 10f),
                    0f,
                    UnityEngine.Random.Range(-10f, 10f)
                );
                
                ScanPlant(plantId, plantPosition);
            }
            
            // Also scan any new plants that might have been added
            for (int i = _plantDiseases.Count; i < 10; i++)
            {
                var newPlantId = $"plant_{i:D3}";
                var plantPosition = new Vector3(
                    UnityEngine.Random.Range(-10f, 10f),
                    0f,
                    UnityEngine.Random.Range(-10f, 10f)
                );
                
                ScanPlant(newPlantId, plantPosition);
            }
        }
        
        private void UpdateDiseaseProgression()
        {
            foreach (var kvp in _plantDiseases)
            {
                var plantId = kvp.Key;
                var plantData = kvp.Value;
                
                foreach (var disease in plantData.ActiveDiseases)
                {
                    // Calculate progression based on disease profile and environmental factors
                    var profile = _diseaseProfiles[disease.DiseaseType];
                    var environmentalImpact = CalculateEnvironmentalImpact(disease.DiseaseType);
                    var plantResistance = CalculatePlantResistance(plantId, disease.DiseaseType);
                    
                    var progressionRate = profile.ProgressionRate * _progressionSpeed * environmentalImpact * (1f - plantResistance);
                    
                    // Apply treatment effects
                    progressionRate *= (1f - disease.TreatmentProgress);
                    
                    // Update disease progression
                    disease.ProgressionLevel = Mathf.Clamp01(disease.ProgressionLevel + progressionRate * Time.deltaTime);
                    
                    // Update severity based on progression
                    disease.Severity = CalculateDiseaseSeverity(disease.ProgressionLevel);
                    
                    // Trigger progression events
                    if (disease.ProgressionLevel > disease.LastProgressionLevel + 0.1f)
                    {
                        var progressionData = new DiseaseProgressionData
                        {
                            PlantId = plantId,
                            DiseaseType = disease.DiseaseType,
                            ProgressionLevel = disease.ProgressionLevel,
                            Severity = disease.Severity,
                            ProgressionRate = progressionRate
                        };
                        
                        OnDiseaseProgressed?.Invoke(plantId, progressionData);
                        _onDiseaseProgressed?.Raise();
                        
                        disease.LastProgressionLevel = disease.ProgressionLevel;
                    }
                }
            }
        }
        
        private void UpdateEnvironmentalRisks()
        {
            // Simulate environmental data - in real implementation, this would come from environmental sensors
            var currentHumidity = UnityEngine.Random.Range(0.4f, 0.9f);
            var currentTemperature = UnityEngine.Random.Range(18f, 30f);
            var currentAirflow = UnityEngine.Random.Range(0.1f, 1.0f);
            var currentMoisture = UnityEngine.Random.Range(0.3f, 1.0f);
            
            // Update risk factors
            _environmentalRisks["HighHumidity"].IsActive = currentHumidity > _humidityRiskThreshold;
            _environmentalRisks["PoorAirflow"].IsActive = currentAirflow < _airflowRiskThreshold;
            _environmentalRisks["ExcessiveMoisture"].IsActive = currentMoisture > _moistureRiskThreshold;
            _environmentalRisks["TemperatureFluctuation"].IsActive = currentTemperature > _temperatureRiskThreshold;
        }
        
        private void UpdatePredictiveModels()
        {
            _progressionPredictor.UpdatePredictions(_plantDiseases, _environmentalRisks);
        }
        
        private void UpdateMetrics()
        {
            _metrics.TotalDiseasesDetected = _totalDiseasesDetected;
            _metrics.TotalTreatmentsApplied = _totalTreatmentsApplied;
            _metrics.DiseasesEradicated = _diseasesEradicated;
            _metrics.FalsePositives = _falsePositives;
            _metrics.DetectionAccuracy = _detectionAccuracy;
            _metrics.SystemEffectiveness = CalculateSystemEffectiveness();
            _metrics.LastUpdated = DateTime.Now;
        }
        
        private float CalculateOverallConfidence(List<DiseaseDetectionData> detectedDiseases)
        {
            if (detectedDiseases.Count == 0) return 0f;
            return detectedDiseases.Average(d => d.Confidence);
        }
        
        private IPMRiskLevel CalculateOverallRiskLevel(List<DiseaseDetectionData> detectedDiseases)
        {
            if (detectedDiseases.Count == 0) return IPMRiskLevel.Low;
            
            var maxSeverity = detectedDiseases.Max(d => d.Severity);
            
            return maxSeverity switch
            {
                DiseaseSeverity.Low => IPMRiskLevel.Low,
                DiseaseSeverity.Moderate => IPMRiskLevel.Moderate,
                DiseaseSeverity.High => IPMRiskLevel.High,
                DiseaseSeverity.Critical => IPMRiskLevel.Critical,
                _ => IPMRiskLevel.Low
            };
        }
        
        private void UpdatePlantDiseaseData(string plantId, DiseaseDetectionResult result)
        {
            if (!_plantDiseases.ContainsKey(plantId))
            {
                _plantDiseases[plantId] = new PlantDiseaseData
                {
                    PlantId = plantId,
                    ActiveDiseases = new List<DiseaseInstanceData>(),
                    EradicatedDiseases = new List<DiseaseInstanceData>(),
                    LastScanDate = DateTime.Now,
                    OverallHealthScore = 1.0f
                };
            }
            
            var plantData = _plantDiseases[plantId];
            plantData.LastScanDate = DateTime.Now;
            
            // Add new diseases
            foreach (var detectedDisease in result.DetectedDiseases)
            {
                var existingDisease = plantData.ActiveDiseases.FirstOrDefault(d => d.DiseaseType == detectedDisease.DiseaseType);
                
                if (existingDisease == null)
                {
                    plantData.ActiveDiseases.Add(new DiseaseInstanceData
                    {
                        DiseaseType = detectedDisease.DiseaseType,
                        DetectionDate = DateTime.Now,
                        Severity = detectedDisease.Severity,
                        Confidence = detectedDisease.Confidence,
                        ProgressionLevel = 0.1f,
                        LastProgressionLevel = 0f,
                        TreatmentProgress = 0f,
                        LastTreatmentDate = DateTime.MinValue,
                        Location = detectedDisease.Location,
                        Symptoms = detectedDisease.Symptoms
                    });
                }
                else
                {
                    // Update existing disease
                    existingDisease.Confidence = Math.Max(existingDisease.Confidence, detectedDisease.Confidence);
                    existingDisease.Severity = (DiseaseSeverity)Math.Max((int)existingDisease.Severity, (int)detectedDisease.Severity);
                }
            }
            
            // Update overall health score
            plantData.OverallHealthScore = CalculateOverallHealthScore(plantData);
        }
        
        private float CalculateOverallHealthScore(PlantDiseaseData plantData)
        {
            if (plantData.ActiveDiseases.Count == 0) return 1.0f;
            
            var totalImpact = plantData.ActiveDiseases.Sum(d => d.ProgressionLevel * GetSeverityMultiplier(d.Severity));
            return Mathf.Clamp01(1.0f - totalImpact / plantData.ActiveDiseases.Count);
        }
        
        private float GetSeverityMultiplier(DiseaseSeverity severity)
        {
            return severity switch
            {
                DiseaseSeverity.Low => 0.1f,
                DiseaseSeverity.Moderate => 0.3f,
                DiseaseSeverity.High => 0.6f,
                DiseaseSeverity.Critical => 1.0f,
                _ => 0.1f
            };
        }
        
        private float CalculateTreatmentEffectiveness(DiseaseType diseaseType, TreatmentType treatmentType)
        {
            if (!_diseaseProfiles.ContainsKey(diseaseType)) return 0.5f;
            
            var profile = _diseaseProfiles[diseaseType];
            var baseEffectiveness = profile.TreatmentOptions.Contains(treatmentType) ? 0.8f : 0.3f;
            
            return baseEffectiveness * _treatmentEffectiveness;
        }
        
        private float CalculateEnvironmentalImpact(DiseaseType diseaseType)
        {
            if (!_diseaseProfiles.ContainsKey(diseaseType)) return 1.0f;
            
            var profile = _diseaseProfiles[diseaseType];
            var impact = 1.0f;
            
            foreach (var risk in _environmentalRisks.Values)
            {
                if (risk.IsActive)
                {
                    impact *= risk.RiskMultiplier;
                }
            }
            
            return Mathf.Clamp(impact, 0.1f, 3.0f);
        }
        
        private float CalculatePlantResistance(string plantId, DiseaseType diseaseType)
        {
            // Simulate plant resistance based on genetics and health
            // In real implementation, this would query the plant's genetic traits
            return UnityEngine.Random.Range(0.1f, 0.7f) * _plantResistanceWeight;
        }
        
        private DiseaseSeverity CalculateDiseaseSeverity(float progressionLevel)
        {
            if (progressionLevel < 0.2f) return DiseaseSeverity.Low;
            if (progressionLevel < 0.5f) return DiseaseSeverity.Moderate;
            if (progressionLevel < 0.8f) return DiseaseSeverity.High;
            return DiseaseSeverity.Critical;
        }
        
        private float CalculateActualAccuracy()
        {
            if (_totalDiseasesDetected == 0) return 1.0f;
            return 1.0f - (_falsePositives / (float)_totalDiseasesDetected);
        }
        
        private float CalculateSystemEffectiveness()
        {
            if (_totalDiseasesDetected == 0) return 1.0f;
            return _diseasesEradicated / (float)_totalDiseasesDetected;
        }
        
        private float CalculatePreventionSuccessRate()
        {
            var totalPlants = _plantDiseases.Count;
            var healthyPlants = _plantDiseases.Values.Count(p => p.ActiveDiseases.Count == 0);
            
            return totalPlants > 0 ? healthyPlants / (float)totalPlants : 1.0f;
        }
        
        #endregion
    }
    
    // Disease detection system components
    public class VisualDiseaseDetector
    {
        private float _accuracy;
        public bool IsActive { get; private set; }
        
        public VisualDiseaseDetector(float accuracy)
        {
            _accuracy = accuracy;
            IsActive = true;
        }
        
        public List<DiseaseDetectionData> PerformVisualAnalysis(string plantId, Vector3 plantPosition)
        {
            var results = new List<DiseaseDetectionData>();
            
            // Simulate visual disease detection
            if (UnityEngine.Random.value < _accuracy)
            {
                var diseaseType = (DiseaseType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(DiseaseType)).Length);
                results.Add(new DiseaseDetectionData
                {
                    DiseaseType = diseaseType,
                    Confidence = UnityEngine.Random.Range(0.6f, 0.9f),
                    Location = plantPosition,
                    Symptoms = new List<string> { "Visual symptoms detected" },
                    DetectionMethod = DiseaseDetectionMethod.Visual,
                    Severity = (DiseaseSeverity)UnityEngine.Random.Range(0, 4),
                    ProgressionLevel = UnityEngine.Random.Range(0.1f, 0.3f)
                });
            }
            
            return results;
        }
    }
    
    public class MicroscopicAnalyzer
    {
        private float _accuracy;
        public bool IsActive { get; private set; }
        
        public MicroscopicAnalyzer(float accuracy)
        {
            _accuracy = accuracy;
            IsActive = true;
        }
        
        public List<DiseaseDetectionData> PerformMicroscopicAnalysis(string plantId, Vector3 plantPosition)
        {
            var results = new List<DiseaseDetectionData>();
            
            // Simulate microscopic analysis
            if (UnityEngine.Random.value < _accuracy)
            {
                var diseaseType = (DiseaseType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(DiseaseType)).Length);
                results.Add(new DiseaseDetectionData
                {
                    DiseaseType = diseaseType,
                    Confidence = UnityEngine.Random.Range(0.7f, 0.95f),
                    Location = plantPosition,
                    Symptoms = new List<string> { "Microscopic pathogens detected" },
                    DetectionMethod = DiseaseDetectionMethod.Microscopic,
                    Severity = (DiseaseSeverity)UnityEngine.Random.Range(0, 4),
                    ProgressionLevel = UnityEngine.Random.Range(0.05f, 0.2f)
                });
            }
            
            return results;
        }
    }
    
    public class ChemicalSensorArray
    {
        private float _accuracy;
        public bool IsActive { get; private set; }
        
        public ChemicalSensorArray(float accuracy)
        {
            _accuracy = accuracy;
            IsActive = true;
        }
        
        public List<DiseaseDetectionData> PerformChemicalAnalysis(string plantId, Vector3 plantPosition)
        {
            var results = new List<DiseaseDetectionData>();
            
            // Simulate chemical analysis
            if (UnityEngine.Random.value < _accuracy)
            {
                var diseaseType = (DiseaseType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(DiseaseType)).Length);
                results.Add(new DiseaseDetectionData
                {
                    DiseaseType = diseaseType,
                    Confidence = UnityEngine.Random.Range(0.8f, 0.95f),
                    Location = plantPosition,
                    Symptoms = new List<string> { "Chemical markers detected" },
                    DetectionMethod = DiseaseDetectionMethod.Chemical,
                    Severity = (DiseaseSeverity)UnityEngine.Random.Range(0, 4),
                    ProgressionLevel = UnityEngine.Random.Range(0.1f, 0.4f)
                });
            }
            
            return results;
        }
    }
    
    public class EnvironmentalRiskAnalyzer
    {
        private float _accuracy;
        public bool IsActive { get; private set; }
        
        public EnvironmentalRiskAnalyzer(float accuracy)
        {
            _accuracy = accuracy;
            IsActive = true;
        }
        
        public List<DiseaseDetectionData> AnalyzeEnvironmentalRisks(string plantId, Vector3 plantPosition)
        {
            var results = new List<DiseaseDetectionData>();
            
            // Simulate environmental risk analysis
            if (UnityEngine.Random.value < _accuracy)
            {
                var diseaseType = (DiseaseType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(DiseaseType)).Length);
                results.Add(new DiseaseDetectionData
                {
                    DiseaseType = diseaseType,
                    Confidence = UnityEngine.Random.Range(0.5f, 0.8f),
                    Location = plantPosition,
                    Symptoms = new List<string> { "Environmental risk factors detected" },
                    DetectionMethod = DiseaseDetectionMethod.Environmental,
                    Severity = (DiseaseSeverity)UnityEngine.Random.Range(0, 3),
                    ProgressionLevel = UnityEngine.Random.Range(0.0f, 0.1f)
                });
            }
            
            return results;
        }
    }
    
    public class DiseaseProgressionPredictor
    {
        public bool IsActive { get; private set; }
        
        public DiseaseProgressionPredictor()
        {
            IsActive = true;
        }
        
        public void UpdatePredictions(Dictionary<string, PlantDiseaseData> plantDiseases, Dictionary<string, EnvironmentalRiskFactor> environmentalRisks)
        {
            // Simulate predictive modeling
            foreach (var plantData in plantDiseases.Values)
            {
                foreach (var disease in plantData.ActiveDiseases)
                {
                    // Predict future progression based on current state and environmental factors
                    var predictedProgression = disease.ProgressionLevel + UnityEngine.Random.Range(0.05f, 0.15f);
                    
                    // This would be used for early warning systems
                    if (predictedProgression > 0.8f && disease.Severity != DiseaseSeverity.Critical)
                    {
                        // Trigger early warning
                        UnityEngine.Debug.Log($"Early warning: Disease {disease.DiseaseType} on plant {plantData.PlantId} may become critical");
                    }
                }
            }
        }
    }
    
}