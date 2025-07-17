using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
// Explicit type alias to resolve ambiguous reference
using IPMPestInfestation = ProjectChimera.Data.IPM.PestInvasionData;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Comprehensive Integrated Pest Management framework for Project Chimera.
    /// Provides scientific pest and disease management for cannabis cultivation.
    /// </summary>
    public class IPMFramework : ChimeraManager
    {
        [Header("IPM Framework Configuration")]
        [SerializeField] private bool _enableAutomaticDetection = true;
        [SerializeField] private bool _enablePreventiveMeasures = true;
        [SerializeField] private bool _enableBiologicalControls = true;
        [SerializeField] private float _detectionAccuracy = 0.85f;
        [SerializeField] private float _treatmentEffectiveness = 0.90f;
        [SerializeField] private int _maxActiveTreatments = 5;
        
        [Header("Detection Parameters")]
        [SerializeField] private float _inspectionInterval = 24f; // hours
        [SerializeField] private float _monitoringRadius = 2f; // meters
        [SerializeField] private float _alertThreshold = 0.3f; // 30% infestation
        [SerializeField] private float _actionThreshold = 0.1f; // 10% infestation
        
        [Header("Cannabis-Specific IPM")]
        [SerializeField] private bool _enableTrichomeProtection = true;
        [SerializeField] private bool _enableBudRotPrevention = true;
        [SerializeField] private bool _enableOrganicOnly = false;
        [SerializeField] private float _preHarvestSafetyDays = 14f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onPestDetected;
        [SerializeField] private SimpleGameEventSO _onTreatmentApplied;
        [SerializeField] private SimpleGameEventSO _onIPMAlert;
        [SerializeField] private SimpleGameEventSO _onPreventionSuccess;
        
        // Core IPM data
        private Dictionary<string, PlantIPMData> _plantIPMData = new Dictionary<string, PlantIPMData>();
        private Dictionary<string, PestInvasionData> _activePests = new Dictionary<string, PestInvasionData>();
        private Dictionary<string, DefenseStructureData> _defenseStructures = new Dictionary<string, DefenseStructureData>();
        private Dictionary<string, BeneficialOrganismData> _beneficialOrganisms = new Dictionary<string, BeneficialOrganismData>();
        private Dictionary<string, IPMTreatmentData> _activeTreatments = new Dictionary<string, IPMTreatmentData>();
        
        // IPM systems
        private IPMDetectionSystem _detectionSystem;
        private IPMTreatmentSystem _treatmentSystem;
        private IPMPreventionSystem _preventionSystem;
        private IPMAnalyticsSystem _analyticsSystem;
        
        // Cannabis-specific systems
        private CannabisIPMManager _cannabisManager;
        private OrganicControlManager _organicManager;
        private TrichomeProtectionManager _trichomeManager;
        
        // Performance tracking
        private IPMMetrics _ipmMetrics;
        private float _lastInspectionTime = 0f;
        private int _totalPestsDetected = 0;
        private int _totalTreatmentsApplied = 0;
        private int _successfulPrevention = 0;
        
        // Events
        public System.Action<string, PestInvasionData> OnPestDetected;
        public System.Action<string, IPMTreatmentData> OnTreatmentApplied;
        public System.Action<IPMAlert> OnIPMAlert;
        public System.Action<string, IPMSuccessData> OnPreventionSuccess;
        
        // Properties
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalPlantsMonitored => _plantIPMData.Count;
        public int TotalPestsDetected => _totalPestsDetected;
        public int TotalTreatmentsApplied => _totalTreatmentsApplied;
        public int ActiveTreatments => _activeTreatments.Count;
        public float SystemEffectiveness => CalculateSystemEffectiveness();
        public IPMMetrics Metrics => _ipmMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeIPMSystems();
            InitializeCannabisSpecialization();
            
            _ipmMetrics = new IPMMetrics();
            
            LogInfo("IPM Framework initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Regular inspection cycle
            if (currentTime - _lastInspectionTime >= _inspectionInterval * 3600f)
            {
                PerformSystemWideInspection();
                _lastInspectionTime = currentTime;
            }
            
            UpdateActiveTreatments();
            UpdatePestPopulations();
            UpdatePreventiveMeasures();
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            _plantIPMData.Clear();
            _activePests.Clear();
            _defenseStructures.Clear();
            _beneficialOrganisms.Clear();
            _activeTreatments.Clear();
            
            LogInfo("IPM Framework shutdown completed");
        }
        
        /// <summary>
        /// Register a plant for IPM monitoring
        /// </summary>
        public void RegisterPlant(string plantId, string plantSpecies)
        {
            var ipmData = new PlantIPMData
            {
                PlantId = plantId,
                PlantSpecies = plantSpecies,
                GrowthStage = IPMPlantGrowthStage.Vegetative,
                HealthStatus = 1.0f,
                LastInspection = DateTime.Now,
                PestResistance = 0.5f,
                ThreatLevel = IPMThreatLevel.Low,
                ActiveThreats = new List<string>(),
                TreatmentHistory = new List<string>()
            };
            
            _plantIPMData[plantId] = ipmData;
            
            LogInfo($"Plant {plantId} registered for IPM monitoring");
        }
        
        /// <summary>
        /// Perform pest detection on a specific plant
        /// </summary>
        public IPMDetectionResult DetectPests(string plantId)
        {
            if (!_plantIPMData.ContainsKey(plantId))
            {
                LogError($"Plant {plantId} not registered for IPM");
                return null;
            }
            
            var plantData = _plantIPMData[plantId];
            var detectionResult = _detectionSystem.ScanForPests(plantId, plantData);
            
            if (detectionResult.ThreatsDetected.Count > 0)
            {
                ProcessDetectedThreats(plantId, detectionResult.ThreatsDetected);
                _totalPestsDetected += detectionResult.ThreatsDetected.Count;
                
                OnPestDetected?.Invoke(plantId, detectionResult.PrimaryThreat);
                _onPestDetected?.Raise();
            }
            
            plantData.LastInspection = DateTime.Now;
            return detectionResult;
        }
        
        /// <summary>
        /// Apply IPM treatment to address pest issues
        /// </summary>
        public IPMTreatmentResult ApplyTreatment(string plantId, IPMTreatmentPlan treatmentPlan)
        {
            if (!_plantIPMData.ContainsKey(plantId))
            {
                LogError($"Plant {plantId} not registered for IPM");
                return null;
            }
            
            if (_activeTreatments.Count >= _maxActiveTreatments)
            {
                LogWarning("Maximum active treatments reached");
                return null;
            }
            
            var treatment = new IPMTreatmentData
            {
                TreatmentId = Guid.NewGuid().ToString(),
                PlantId = plantId,
                TreatmentPlan = treatmentPlan,
                StartTime = DateTime.Now,
                Status = IPMTreatmentStatus.Active,
                Progress = 0f,
                Effectiveness = 0f
            };
            
            var result = _treatmentSystem.ApplyTreatment(treatment);
            
            if (result.IsSuccessful)
            {
                _activeTreatments[treatment.TreatmentId] = treatment;
                _totalTreatmentsApplied++;
                
                var plantData = _plantIPMData[plantId];
                plantData.TreatmentHistory.Add(treatment.TreatmentId);
                
                OnTreatmentApplied?.Invoke(plantId, treatment);
                _onTreatmentApplied?.Raise();
                
                LogInfo($"Applied {treatmentPlan.StrategyType} treatment to plant {plantId}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Deploy biological control agents
        /// </summary>
        public void DeployBeneficialOrganisms(string areaId, BeneficialOrganismType organismType, int quantity)
        {
            var organism = new BeneficialOrganismData
            {
                OrganismId = Guid.NewGuid().ToString(),
                Type = organismType,
                PopulationSize = quantity,
                ReleaseLocation = Vector3.zero, // Would be set based on area
                ReleaseTime = DateTime.Now,
                IsEstablished = false,
                SurvivalRate = 0.8f,
                HuntingEfficiency = 0.75f
            };
            
            // Set target pests based on organism type
            organism.TargetPests = GetTargetPests(organismType);
            
            _beneficialOrganisms[organism.OrganismId] = organism;
            
            LogInfo($"Deployed {quantity} {organismType} in area {areaId}");
        }
        
        /// <summary>
        /// Install defense structure for pest prevention
        /// </summary>
        public void InstallDefenseStructure(DefenseStructureType structureType, Vector3 position)
        {
            var structure = new DefenseStructureData
            {
                StructureId = Guid.NewGuid().ToString(),
                Type = structureType,
                Position = position,
                Effectiveness = GetDefaultEffectiveness(structureType),
                Range = GetDefaultRange(structureType),
                IsActive = true,
                InstallationTime = DateTime.Now,
                PestEffectiveness = GetPestEffectiveness(structureType)
            };
            
            _defenseStructures[structure.StructureId] = structure;
            
            LogInfo($"Installed {structureType} defense structure");
        }
        
        /// <summary>
        /// Get comprehensive IPM status for a plant
        /// </summary>
        public IPMStatusReport GetPlantStatus(string plantId)
        {
            if (!_plantIPMData.ContainsKey(plantId))
            {
                return null;
            }
            
            var plantData = _plantIPMData[plantId];
            var activePests = GetActivePestsForPlant(plantId);
            var activeTreatments = GetActiveTreatmentsForPlant(plantId);
            
            return new IPMStatusReport
            {
                PlantId = plantId,
                PlantData = plantData,
                ActivePests = activePests,
                ActiveTreatments = activeTreatments,
                ThreatLevel = plantData.ThreatLevel,
                RecommendedActions = GetRecommendedActions(plantId),
                PreventiveMeasures = GetApplicablePreventiveMeasures(plantId),
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Get system-wide IPM metrics and performance
        /// </summary>
        public IPMSystemReport GetSystemReport()
        {
            return new IPMSystemReport
            {
                TotalPlantsMonitored = TotalPlantsMonitored,
                TotalPestsDetected = _totalPestsDetected,
                TotalTreatmentsApplied = _totalTreatmentsApplied,
                ActiveTreatments = ActiveTreatments,
                SystemEffectiveness = SystemEffectiveness,
                PreventionSuccessRate = CalculatePreventionSuccessRate(),
                AverageDetectionAccuracy = _detectionAccuracy,
                BiologicalControlsActive = _beneficialOrganisms.Count,
                DefenseStructuresActive = _defenseStructures.Values.Count(s => s.IsActive),
                ReportDate = DateTime.Now
            };
        }
        
        #region Private Implementation
        
        private void InitializeIPMSystems()
        {
            _detectionSystem = new IPMDetectionSystem(_detectionAccuracy);
            _treatmentSystem = new IPMTreatmentSystem(_treatmentEffectiveness);
            _preventionSystem = new IPMPreventionSystem();
            _analyticsSystem = new IPMAnalyticsSystem();
        }
        
        private void InitializeCannabisSpecialization()
        {
            _cannabisManager = new CannabisIPMManager();
            _organicManager = new OrganicControlManager();
            _trichomeManager = new TrichomeProtectionManager();
        }
        
        private void PerformSystemWideInspection()
        {
            foreach (var plantId in _plantIPMData.Keys.ToList())
            {
                DetectPests(plantId);
            }
        }
        
        private void UpdateActiveTreatments()
        {
            foreach (var treatment in _activeTreatments.Values.ToList())
            {
                UpdateTreatmentProgress(treatment);
                
                if (treatment.Status == IPMTreatmentStatus.Completed)
                {
                    CompleteTreatment(treatment);
                }
            }
        }
        
        private void UpdatePestPopulations()
        {
            foreach (var pest in _activePests.Values.ToList())
            {
                UpdatePestGrowth(pest);
                CheckPestThresholds(pest);
            }
        }
        
        private void UpdatePreventiveMeasures()
        {
            if (_enablePreventiveMeasures)
            {
                _preventionSystem.UpdatePreventiveMeasures(_plantIPMData);
            }
        }
        
        private void UpdateMetrics()
        {
            _ipmMetrics.TotalPlantsMonitored = TotalPlantsMonitored;
            _ipmMetrics.TotalPestsDetected = _totalPestsDetected;
            _ipmMetrics.TotalTreatmentsApplied = _totalTreatmentsApplied;
            _ipmMetrics.SystemEffectiveness = SystemEffectiveness;
            _ipmMetrics.LastUpdated = DateTime.Now;
        }
        
        private float CalculatePestResistance(float healthStatus)
        {
            float baseResistance = 0.5f;
            float healthModifier = healthStatus * 0.3f;
            
            return Mathf.Clamp01(baseResistance + healthModifier);
        }
        
        private void ProcessDetectedThreats(string plantId, List<PestInvasionData> threats)
        {
            var plantData = _plantIPMData[plantId];
            
            foreach (var threat in threats)
            {
                _activePests[threat.InvasionId] = threat;
                plantData.ActiveThreats.Add(threat.InvasionId);
                
                // Update threat level
                plantData.ThreatLevel = CalculateThreatLevel(plantData);
                
                // Check if immediate action is required
                if (threat.PopulationSize > _alertThreshold * 100)
                {
                    // Create and trigger alert without undefined method
                    var alert = new IPMAlert
                    {
                        PlantId = plantId,
                        AlertType = "Pest Detection",
                        Message = $"High population of {threat.PestType} detected",
                        Severity = IPMThreatLevel.High,
                        RequiresImmediateAction = true
                    };
                    
                    OnIPMAlert?.Invoke(alert);
                    _onIPMAlert?.Raise();
                }
            }
        }
        
        private void UpdateTreatmentProgress(IPMTreatmentData treatment)
        {
            // Simple progress simulation
            treatment.Progress += Time.deltaTime / (treatment.TreatmentPlan.EstimatedDuration * 3600f);
            treatment.Progress = Mathf.Clamp01(treatment.Progress);
            
            // Update effectiveness based on progress
            treatment.Effectiveness = treatment.Progress * _treatmentEffectiveness;
            
            if (treatment.Progress >= 1f)
            {
                treatment.Status = IPMTreatmentStatus.Completed;
            }
        }
        
        private void CompleteTreatment(IPMTreatmentData treatment)
        {
            _activeTreatments.Remove(treatment.TreatmentId);
            
            // Apply treatment effects to reduce pest populations
            ApplyTreatmentEffects(treatment);
            
            LogInfo($"Treatment {treatment.TreatmentId} completed with {treatment.Effectiveness:P0} effectiveness");
        }
        
        private void ApplyTreatmentEffects(IPMTreatmentData treatment)
        {
            var plantData = _plantIPMData[treatment.PlantId];
            
            foreach (var threatId in plantData.ActiveThreats.ToList())
            {
                if (_activePests.ContainsKey(threatId))
                {
                    var pest = _activePests[threatId];
                    
                    // Reduce pest population based on treatment effectiveness
                    int reduction = Mathf.RoundToInt(pest.PopulationSize * treatment.Effectiveness);
                    pest.PopulationSize = Mathf.Max(0, pest.PopulationSize - reduction);
                    
                    // Remove pest if population is eliminated
                    if (pest.PopulationSize <= 0)
                    {
                        _activePests.Remove(threatId);
                        plantData.ActiveThreats.Remove(threatId);
                        _successfulPrevention++;
                    }
                }
            }
            
            // Update threat level
            plantData.ThreatLevel = CalculateThreatLevel(plantData);
        }
        
        private float CalculateSystemEffectiveness()
        {
            if (_totalTreatmentsApplied == 0) return 1f;
            
            return (float)_successfulPrevention / _totalTreatmentsApplied;
        }
        
        private float CalculatePreventionSuccessRate()
        {
            if (_totalPestsDetected == 0) return 1f;
            
            return (float)_successfulPrevention / _totalPestsDetected;
        }
        
        private IPMThreatLevel CalculateThreatLevel(PlantIPMData plantData)
        {
            if (plantData.ActiveThreats.Count >= 3) return IPMThreatLevel.Critical;
            if (plantData.ActiveThreats.Count >= 2) return IPMThreatLevel.High;
            if (plantData.ActiveThreats.Count >= 1) return IPMThreatLevel.Moderate;
            return IPMThreatLevel.Low;
        }
        
        private List<PestInvasionData> GetActivePestsForPlant(string plantId)
        {
            var result = new List<PestInvasionData>();
            if (!_plantIPMData.ContainsKey(plantId)) return result;
            
            var plantData = _plantIPMData[plantId];
            foreach (var threatId in plantData.ActiveThreats)
            {
                if (_activePests.ContainsKey(threatId))
                {
                    result.Add(_activePests[threatId]);
                }
            }
            return result;
        }
        
        private List<IPMTreatmentData> GetActiveTreatmentsForPlant(string plantId)
        {
            return _activeTreatments.Values.Where(t => t.PlantId == plantId).ToList();
        }
        
        private List<string> GetRecommendedActions(string plantId)
        {
            var actions = new List<string>();
            if (!_plantIPMData.ContainsKey(plantId)) return actions;
            
            var plantData = _plantIPMData[plantId];
            if (plantData.ThreatLevel >= IPMThreatLevel.Moderate)
            {
                actions.Add("Apply biological control agents");
                actions.Add("Increase inspection frequency");
            }
            if (plantData.HealthStatus < 0.7f)
            {
                actions.Add("Improve plant health");
                actions.Add("Check environmental conditions");
            }
            return actions;
        }
        
        private List<string> GetApplicablePreventiveMeasures(string plantId)
        {
            var measures = new List<string>();
            measures.Add("Regular plant inspection");
            measures.Add("Maintain optimal environmental conditions");
            measures.Add("Deploy beneficial organisms preventively");
            measures.Add("Install sticky traps");
            return measures;
        }
        
        private List<ProjectChimera.Data.IPM.PestType> GetTargetPests(BeneficialOrganismType organismType)
        {
            var targets = new List<ProjectChimera.Data.IPM.PestType>();
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs:
                    targets.Add(ProjectChimera.Data.IPM.PestType.Aphids);
                    targets.Add(ProjectChimera.Data.IPM.PestType.Mealybugs);
                    break;
                case BeneficialOrganismType.PredatoryMites:
                    targets.Add(ProjectChimera.Data.IPM.PestType.SpiderMites);
                    targets.Add(ProjectChimera.Data.IPM.PestType.Thrips);
                    break;
                case BeneficialOrganismType.LacewingLarvae:
                    targets.Add(ProjectChimera.Data.IPM.PestType.Aphids);
                    targets.Add(ProjectChimera.Data.IPM.PestType.Whiteflies);
                    break;
                default:
                    targets.Add(ProjectChimera.Data.IPM.PestType.Aphids);
                    break;
            }
            return targets;
        }
        
        private float GetDefaultEffectiveness(DefenseStructureType structureType)
        {
            switch (structureType)
            {
                case DefenseStructureType.StickyTrap: return 0.6f;
                case DefenseStructureType.PheromoneTrap: return 0.8f;
                case DefenseStructureType.BiologicalReleaseStation: return 0.9f;
                case DefenseStructureType.UVSterilizer: return 0.7f;
                default: return 0.5f;
            }
        }
        
        private float GetDefaultRange(DefenseStructureType structureType)
        {
            switch (structureType)
            {
                case DefenseStructureType.StickyTrap: return 1.0f;
                case DefenseStructureType.PheromoneTrap: return 3.0f;
                case DefenseStructureType.BiologicalReleaseStation: return 5.0f;
                case DefenseStructureType.UVSterilizer: return 2.0f;
                default: return 1.5f;
            }
        }
        
        private Dictionary<ProjectChimera.Data.IPM.PestType, float> GetPestEffectiveness(DefenseStructureType structureType)
        {
            var effectiveness = new Dictionary<ProjectChimera.Data.IPM.PestType, float>();
            switch (structureType)
            {
                case DefenseStructureType.StickyTrap:
                    effectiveness[ProjectChimera.Data.IPM.PestType.Fungusgnats] = 0.9f;
                    effectiveness[ProjectChimera.Data.IPM.PestType.Whiteflies] = 0.7f;
                    effectiveness[ProjectChimera.Data.IPM.PestType.Thrips] = 0.6f;
                    break;
                case DefenseStructureType.PheromoneTrap:
                    effectiveness[ProjectChimera.Data.IPM.PestType.Caterpillars] = 0.8f;
                    effectiveness[ProjectChimera.Data.IPM.PestType.Leafminers] = 0.7f;
                    break;
                default:
                    effectiveness[ProjectChimera.Data.IPM.PestType.Aphids] = 0.5f;
                    break;
            }
            return effectiveness;
        }
        
        private void UpdatePestGrowth(PestInvasionData pest)
        {
            // Simple population growth simulation
            pest.PopulationSize = Mathf.RoundToInt(pest.PopulationSize * (1f + pest.ReproductionRate * Time.deltaTime));
        }
        
        private void CheckPestThresholds(PestInvasionData pest)
        {
            if (pest.PopulationSize > 200)
            {
                pest.AggressionLevel = Mathf.Min(1f, pest.AggressionLevel + 0.1f);
            }
        }
        
        #endregion
    }
    
    // Supporting data structures for the IPM Framework
    [System.Serializable]
    public class PlantIPMData
    {
        public string PlantId;
        public string PlantSpecies;
        public IPMPlantGrowthStage GrowthStage;
        public float HealthStatus;
        public DateTime LastInspection;
        public float PestResistance;
        public IPMThreatLevel ThreatLevel;
        public List<string> ActiveThreats = new List<string>();
        public List<string> TreatmentHistory = new List<string>();
        
        public PlantIPMData()
        {
            ActiveThreats = new List<string>();
            TreatmentHistory = new List<string>();
        }
    }
    
    [System.Serializable]
    public class IPMTreatmentData
    {
        public string TreatmentId;
        public string PlantId;
        public IPMTreatmentPlan TreatmentPlan;
        public DateTime StartTime;
        public IPMTreatmentStatus Status;
        public float Progress;
        public float Effectiveness;
    }
    
    [System.Serializable]
    public class IPMTreatmentPlan
    {
        public string PlanId;
        public IPMStrategyType StrategyType;
        public string TreatmentName;
        public List<ProjectChimera.Data.IPM.PestType> TargetPests = new List<ProjectChimera.Data.IPM.PestType>();
        public int EstimatedDuration; // hours
        public float ExpectedEffectiveness;
        public bool IsOrganicSafe;
        public int PreHarvestDays;
        
        public IPMTreatmentPlan()
        {
            PlanId = Guid.NewGuid().ToString();
            TargetPests = new List<ProjectChimera.Data.IPM.PestType>();
        }
    }
    
    // Additional enums and data structures
    public enum IPMThreatLevel
    {
        None,
        Low,
        Moderate,
        High,
        Critical
    }
    
    public enum IPMTreatmentStatus
    {
        Planned,
        Active,
        Completed,
        Failed,
        Cancelled
    }
    
    public enum IPMPlantGrowthStage
    {
        Seedling,
        Vegetative,
        Flowering,
        Mature
    }
    
    [System.Serializable]
    public class IPMDetectionResult
    {
        public string PlantId;
        public DateTime DetectionTime;
        public List<PestInvasionData> ThreatsDetected = new List<PestInvasionData>();
        public PestInvasionData PrimaryThreat;
        public float Confidence;
        public IPMThreatLevel OverallThreatLevel;
        
        public IPMDetectionResult()
        {
            ThreatsDetected = new List<PestInvasionData>();
        }
    }
    
    [System.Serializable]
    public class IPMTreatmentResult
    {
        public string TreatmentId;
        public bool IsSuccessful;
        public string ResultMessage;
        public DateTime ApplicationTime;
        public float ExpectedEffectiveness;
    }
    
    [System.Serializable]
    public class IPMStatusReport
    {
        public string PlantId;
        public PlantIPMData PlantData;
        public List<PestInvasionData> ActivePests = new List<PestInvasionData>();
        public List<IPMTreatmentData> ActiveTreatments = new List<IPMTreatmentData>();
        public IPMThreatLevel ThreatLevel;
        public List<string> RecommendedActions = new List<string>();
        public List<string> PreventiveMeasures = new List<string>();
        public DateTime LastUpdated;
        
        public IPMStatusReport()
        {
            ActivePests = new List<PestInvasionData>();
            ActiveTreatments = new List<IPMTreatmentData>();
            RecommendedActions = new List<string>();
            PreventiveMeasures = new List<string>();
        }
    }
    
    [System.Serializable]
    public class IPMSystemReport
    {
        public int TotalPlantsMonitored;
        public int TotalPestsDetected;
        public int TotalTreatmentsApplied;
        public int ActiveTreatments;
        public float SystemEffectiveness;
        public float PreventionSuccessRate;
        public float AverageDetectionAccuracy;
        public int BiologicalControlsActive;
        public int DefenseStructuresActive;
        public DateTime ReportDate;
    }
    
    [System.Serializable]
    public class IPMAlert
    {
        public string AlertId;
        public string PlantId;
        public string AlertType;
        public string Message;
        public DateTime CreatedAt;
        public IPMThreatLevel Severity;
        public bool RequiresImmediateAction;
        
        public IPMAlert()
        {
            AlertId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class IPMSuccessData
    {
        public string PlantId;
        public string TreatmentId;
        public float EffectivenessAchieved;
        public int PestsEliminated;
        public DateTime AchievedAt;
        
        public IPMSuccessData()
        {
            AchievedAt = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class IPMMetrics
    {
        public int TotalPlantsMonitored;
        public int TotalPestsDetected;
        public int TotalTreatmentsApplied;
        public float SystemEffectiveness;
        public DateTime LastUpdated;
        
        public IPMMetrics()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    // Supporting system classes (simplified implementations)
    public class IPMDetectionSystem
    {
        private float _accuracy;
        
        public IPMDetectionSystem(float accuracy)
        {
            _accuracy = accuracy;
        }
        
        public IPMDetectionResult ScanForPests(string plantId, PlantIPMData plantData)
        {
            var result = new IPMDetectionResult
            {
                PlantId = plantId,
                DetectionTime = DateTime.Now,
                Confidence = _accuracy
            };
            
            // Simulate pest detection based on plant health and environmental factors
            if (ShouldDetectPests(plantData))
            {
                var pest = SimulatePestThreat(plantData);
                result.ThreatsDetected.Add(pest);
                result.PrimaryThreat = pest;
                result.OverallThreatLevel = CalculateThreatLevel(pest);
            }
            
            return result;
        }
        
        private bool ShouldDetectPests(PlantIPMData plantData)
        {
            float detectionChance = 0.1f; // Base 10% chance
            
            if (plantData.HealthStatus < 0.7f) detectionChance += 0.2f;
            if (plantData.PestResistance < 0.5f) detectionChance += 0.15f;
            
            return UnityEngine.Random.Range(0f, 1f) < detectionChance;
        }
        
        private PestInvasionData SimulatePestThreat(PlantIPMData plantData)
        {
            var pestTypes = Enum.GetValues(typeof(PestType));
            var randomPest = (PestType)pestTypes.GetValue(UnityEngine.Random.Range(0, pestTypes.Length));
            
            return new PestInvasionData
            {
                InvasionId = Guid.NewGuid().ToString(),
                PestType = randomPest,
                PopulationSize = UnityEngine.Random.Range(10, 100),
                AggressionLevel = UnityEngine.Random.Range(0.3f, 0.8f),
                InvasionStartTime = DateTime.Now,
                ReproductionRate = UnityEngine.Random.Range(0.1f, 0.3f)
            };
        }
        
        private IPMThreatLevel CalculateThreatLevel(PestInvasionData pest)
        {
            if (pest.PopulationSize > 80) return IPMThreatLevel.Critical;
            if (pest.PopulationSize > 60) return IPMThreatLevel.High;
            if (pest.PopulationSize > 30) return IPMThreatLevel.Moderate;
            return IPMThreatLevel.Low;
        }
    }
    
    public class IPMTreatmentSystem
    {
        private float _effectiveness;
        
        public IPMTreatmentSystem(float effectiveness)
        {
            _effectiveness = effectiveness;
        }
        
        public IPMTreatmentResult ApplyTreatment(IPMTreatmentData treatment)
        {
            return new IPMTreatmentResult
            {
                TreatmentId = treatment.TreatmentId,
                IsSuccessful = true,
                ResultMessage = "Treatment applied successfully",
                ApplicationTime = DateTime.Now,
                ExpectedEffectiveness = _effectiveness
            };
        }
    }
    
    // Additional placeholder classes for compilation
    public class IPMPreventionSystem 
    {
        public void UpdatePreventiveMeasures(Dictionary<string, PlantIPMData> plantData) { }
    }
    
    public class IPMAnalyticsSystem { }
    public class CannabisIPMManager { }
    public class OrganicControlManager { }
    public class TrichomeProtectionManager { }
}