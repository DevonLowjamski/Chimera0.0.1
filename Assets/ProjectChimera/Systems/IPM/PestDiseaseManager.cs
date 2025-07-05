using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using IPMPestType = ProjectChimera.Data.IPM.PestType;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Phase 4.1.a: Advanced Pest and Disease Management System
    /// Implements dynamic pest appearances, disease outbreaks, and environmental health challenges
    /// Transforms cultivation into engaging strategic defense gameplay with real-world accuracy
    /// </summary>
    public class PestDiseaseManager : ChimeraManager
    {
        [Header("Phase 4.1 Configuration")]
        public bool EnableDynamicInfestation = true;
        public bool EnableDiseaseOutbreaks = true;
        public bool EnableEnvironmentalThreats = true;
        public bool EnableAdaptivePests = true;
        
        [Header("Threat Generation Settings")]
        [Range(0f, 1f)] public float BaseThreatLevel = 0.15f;
        [Range(0f, 2f)] public float EnvironmentalStressMultiplier = 1.5f;
        [Range(1, 100)] public int MaxSimultaneousThreats = 5;
        public float ThreatSpreadRate = 0.25f;
        
        [Header("Dynamic Threats")]
        [SerializeField] private List<ActiveThreat> activeThreats = new List<ActiveThreat>();
        [SerializeField] private List<ThreatIncident> threatHistory = new List<ThreatIncident>();
        [SerializeField] private Dictionary<string, ThreatProfile> pestProfiles = new Dictionary<string, ThreatProfile>();
        [SerializeField] private Dictionary<string, DiseaseProfile> diseaseProfiles = new Dictionary<string, DiseaseProfile>();
        
        [Header("Environmental Monitoring")]
        [SerializeField] private EnvironmentalRiskAssessment currentRisk = new EnvironmentalRiskAssessment();
        [SerializeField] private List<string> facilityVulnerabilities = new List<string>();
        [SerializeField] private Dictionary<string, float> threatResistance = new Dictionary<string, float>();
        
        // Phase 4.1 Data Structures
        [System.Serializable]
        public class ActiveThreat
        {
            public string ThreatId;
            public ThreatType Type;
            public string OriginLocation;
            public DateTime DetectionTime;
            public float Severity;
            public float SpreadRadius;
            public List<string> AffectedPlants;
            public List<string> VulnerablePlants;
            public bool IsContained;
            public float ContainmentEffectiveness;
            public IPMResponse RequiredResponse;
        }
        
        [System.Serializable]
        public class ThreatIncident
        {
            public string IncidentId;
            public DateTime OccurrenceTime;
            public ThreatType Type;
            public string CauseAnalysis;
            public float EconomicImpact;
            public string ResolutionMethod;
            public TimeSpan ResolutionTime;
            public List<string> LessonsLearned;
            public float PreventionScore;
        }
        
        [System.Serializable]
        public class ThreatProfile
        {
            public string PestName;
            public IPMPestType Category;
            public float BaseAggression;
            public float ReproductionRate;
            public float EnvironmentalTolerance;
            public List<string> PreferredHosts;
            public List<string> EnvironmentalTriggers;
            public float AdaptationRate;
            public Dictionary<string, float> TreatmentResistance;
        }
        
        [System.Serializable]
        public class DiseaseProfile
        {
            public string DiseaseName;
            public DiseaseType Category;
            public float TransmissionRate;
            public float Virulence;
            public List<string> TransmissionVectors;
            public Dictionary<string, float> EnvironmentalFactors;
            public float IncubationPeriod;
            public List<string> VisibleSymptoms;
            public bool IsSystemic;
        }
        
        [System.Serializable]
        public class EnvironmentalRiskAssessment
        {
            public float OverallRiskLevel;
            public float TemperatureRisk;
            public float HumidityRisk;
            public float AirCirculationRisk;
            public float SanitationRisk;
            public float PlantStressRisk;
            public DateTime LastAssessment;
            public List<string> HighRiskFactors;
            public Dictionary<string, float> ZoneRiskLevels;
        }
        
        public enum ThreatType
        {
            SuckingInsects,      // Aphids, Spider Mites, Whiteflies
            ChewingInsects,      // Caterpillars, Leaf Miners
            FungalDisease,       // Powdery Mildew, Botrytis, Root Rot
            BacterialDisease,    // Bacterial Blight, Soft Rot
            ViralDisease,        // Mosaic Viruses, Leaf Curl
            NematodeInfestation, // Root-knot, Lesion Nematodes
            PhysiologicalStress, // Nutrient Deficiency, pH Issues
            EnvironmentalStress  // Heat Stress, Light Burn
        }
        
        public enum DiseaseType
        {
            Fungal,
            Bacterial,
            Viral,
            Physiological,
            Environmental,
            Nutritional
        }
        
        public enum IPMResponse
        {
            BiologicalControl,
            ChemicalIntervention,
            EnvironmentalModification,
            PhysicalRemoval,
            QuarantineMeasures,
            SystemicTreatment,
            PreventiveMaintenance
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeThreatProfiles();
            InitializeDiseaseProfiles();
            SetupEnvironmentalMonitoring();
            RegisterEventListeners();
            
            Debug.Log($"[Phase 4.1] PestDiseaseManager initialized - Advanced threats active");
        }
        
        protected override void OnManagerShutdown()
        {
            UnregisterEventListeners();
            SaveThreatHistory();
            
            Debug.Log($"[Phase 4.1] PestDiseaseManager shutdown - {activeThreats.Count} threats logged");
        }
        
        private void InitializeThreatProfiles()
        {
            // Phase 4.1.a: Initialize scientifically accurate pest profiles
            pestProfiles["AphidColony"] = new ThreatProfile
            {
                PestName = "Green Peach Aphid",
                Category = IPMPestType.Aphids,
                BaseAggression = 0.6f,
                ReproductionRate = 0.8f,
                EnvironmentalTolerance = 0.4f,
                PreferredHosts = new List<string> { "Cannabis", "Tomato", "Pepper" },
                EnvironmentalTriggers = new List<string> { "HighNitrogen", "WarmTemperature" },
                AdaptationRate = 0.3f,
                TreatmentResistance = new Dictionary<string, float>
                {
                    ["Neem"] = 0.1f,
                    ["Ladybugs"] = 0.05f,
                    ["Pyrethrin"] = 0.2f
                }
            };
            
            pestProfiles["SpiderMiteInfestation"] = new ThreatProfile
            {
                PestName = "Two-Spotted Spider Mite",
                Category = IPMPestType.SpiderMites,
                BaseAggression = 0.8f,
                ReproductionRate = 0.9f,
                EnvironmentalTolerance = 0.6f,
                PreferredHosts = new List<string> { "Cannabis", "StressedPlants" },
                EnvironmentalTriggers = new List<string> { "LowHumidity", "HighTemperature", "PoorAirflow" },
                AdaptationRate = 0.4f,
                TreatmentResistance = new Dictionary<string, float>
                {
                    ["PredatoryMites"] = 0.03f,
                    ["Neem"] = 0.15f,
                    ["Miticide"] = 0.25f
                }
            };
        }
        
        private void InitializeDiseaseProfiles()
        {
            // Phase 4.1.b: Initialize disease profiles with realistic progression
            diseaseProfiles["PowderyMildew"] = new DiseaseProfile
            {
                DiseaseName = "Powdery Mildew",
                Category = DiseaseType.Fungal,
                TransmissionRate = 0.7f,
                Virulence = 0.5f,
                TransmissionVectors = new List<string> { "Airborne", "DirectContact" },
                EnvironmentalFactors = new Dictionary<string, float>
                {
                    ["HighHumidity"] = 0.8f,
                    ["PoorAirflow"] = 0.6f,
                    ["ModerateTemperature"] = 0.4f
                },
                IncubationPeriod = 3f,
                VisibleSymptoms = new List<string> { "WhitePowderySpots", "LeafDistortion" },
                IsSystemic = false
            };
            
            diseaseProfiles["BotrytisGrayMold"] = new DiseaseProfile
            {
                DiseaseName = "Botrytis Gray Mold",
                Category = DiseaseType.Fungal,
                TransmissionRate = 0.6f,
                Virulence = 0.8f,
                TransmissionVectors = new List<string> { "Airborne", "WoundInfection" },
                EnvironmentalFactors = new Dictionary<string, float>
                {
                    ["HighHumidity"] = 0.9f,
                    ["LowTemperature"] = 0.5f,
                    ["PoorAirflow"] = 0.7f,
                    ["PlantWounds"] = 0.8f
                },
                IncubationPeriod = 2f,
                VisibleSymptoms = new List<string> { "GrayFuzzyGrowth", "TissueNecrosis", "BudRot" },
                IsSystemic = true
            };
        }
        
        private void SetupEnvironmentalMonitoring()
        {
            // Phase 4.1.c: Continuous environmental risk assessment
            InvokeRepeating(nameof(AssessEnvironmentalRisk), 60f, 300f); // Every 5 minutes
            InvokeRepeating(nameof(EvaluateThreatGeneration), 120f, 600f); // Every 10 minutes
        }
        
        private void AssessEnvironmentalRisk()
        {
            currentRisk = new EnvironmentalRiskAssessment
            {
                LastAssessment = DateTime.Now,
                HighRiskFactors = new List<string>(),
                ZoneRiskLevels = new Dictionary<string, float>()
            };
            
            // Calculate environmental risk factors
            float tempRisk = CalculateTemperatureRisk();
            float humidityRisk = CalculateHumidityRisk();
            float airflowRisk = CalculateAirflowRisk();
            float sanitationRisk = CalculateSanitationRisk();
            float stressRisk = CalculatePlantStressRisk();
            
            currentRisk.TemperatureRisk = tempRisk;
            currentRisk.HumidityRisk = humidityRisk;
            currentRisk.AirCirculationRisk = airflowRisk;
            currentRisk.SanitationRisk = sanitationRisk;
            currentRisk.PlantStressRisk = stressRisk;
            
            currentRisk.OverallRiskLevel = (tempRisk + humidityRisk + airflowRisk + sanitationRisk + stressRisk) / 5f;
            
            // Log high-risk factors
            if (tempRisk > 0.7f) currentRisk.HighRiskFactors.Add("ExtremeTemperature");
            if (humidityRisk > 0.7f) currentRisk.HighRiskFactors.Add("HumidityImbalance");
            if (airflowRisk > 0.7f) currentRisk.HighRiskFactors.Add("PoorVentilation");
            if (sanitationRisk > 0.7f) currentRisk.HighRiskFactors.Add("SanitationIssues");
            if (stressRisk > 0.7f) currentRisk.HighRiskFactors.Add("PlantStress");
        }
        
        private void EvaluateThreatGeneration()
        {
            // Phase 4.1.d: Dynamic threat generation based on environmental conditions
            if (!EnableDynamicInfestation || activeThreats.Count >= MaxSimultaneousThreats)
                return;
                
            float threatChance = BaseThreatLevel * (1f + currentRisk.OverallRiskLevel * EnvironmentalStressMultiplier);
            
            if (UnityEngine.Random.Range(0f, 1f) < threatChance)
            {
                GenerateNewThreat();
            }
            
            ProcessActiveThreatEvolution();
        }
        
        private void GenerateNewThreat()
        {
            // Phase 4.1.e: Intelligent threat generation
            ThreatType threatType = SelectThreatTypeByRisk();
            string threatId = Guid.NewGuid().ToString("N")[..8];
            
            ActiveThreat newThreat = new ActiveThreat
            {
                ThreatId = threatId,
                Type = threatType,
                OriginLocation = SelectOriginLocation(),
                DetectionTime = DateTime.Now,
                Severity = UnityEngine.Random.Range(0.1f, 0.7f),
                SpreadRadius = UnityEngine.Random.Range(1f, 5f),
                AffectedPlants = new List<string>(),
                VulnerablePlants = new List<string>(),
                IsContained = false,
                ContainmentEffectiveness = 0f,
                RequiredResponse = DetermineOptimalResponse(threatType)
            };
            
            activeThreats.Add(newThreat);
            
            Debug.Log($"[Phase 4.1] New {threatType} threat detected: {threatId} (Severity: {newThreat.Severity:F2})");
            
            // Fire threat detection event for UI and other systems
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("ThreatDetected", newThreat);
        }
        
        private ThreatType SelectThreatTypeByRisk()
        {
            // Phase 4.1.f: Risk-based threat selection
            if (currentRisk.HumidityRisk > 0.8f && currentRisk.AirCirculationRisk > 0.6f)
                return ThreatType.FungalDisease;
            else if (currentRisk.TemperatureRisk > 0.7f && currentRisk.PlantStressRisk > 0.5f)
                return ThreatType.SuckingInsects;
            else if (currentRisk.SanitationRisk > 0.6f)
                return ThreatType.BacterialDisease;
            else
                return (ThreatType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ThreatType)).Length);
        }
        
        private void ProcessActiveThreatEvolution()
        {
            // Phase 4.1.g: Threat progression and adaptation
            for (int i = activeThreats.Count - 1; i >= 0; i--)
            {
                var threat = activeThreats[i];
                
                // Evolve threat characteristics
                threat.Severity += ThreatSpreadRate * Time.deltaTime * (1f - threat.ContainmentEffectiveness);
                threat.SpreadRadius += ThreatSpreadRate * 0.5f * Time.deltaTime;
                
                // Check for threat resolution
                if (threat.ContainmentEffectiveness > 0.95f || threat.Severity < 0.01f)
                {
                    ResolveThreat(threat);
                    activeThreats.RemoveAt(i);
                }
                // Check for critical threshold
                else if (threat.Severity > 1f)
                {
                    EscalateThreat(threat);
                }
            }
        }
        
        private void ResolveThreat(ActiveThreat threat)
        {
            // Phase 4.1.h: Threat resolution logging
            var incident = new ThreatIncident
            {
                IncidentId = threat.ThreatId,
                OccurrenceTime = threat.DetectionTime,
                Type = threat.Type,
                ResolutionTime = DateTime.Now - threat.DetectionTime,
                ResolutionMethod = threat.RequiredResponse.ToString(),
                PreventionScore = 1f - threat.Severity
            };
            
            threatHistory.Add(incident);
            
            Debug.Log($"[Phase 4.1] Threat resolved: {threat.ThreatId} in {incident.ResolutionTime.TotalMinutes:F1} minutes");
            
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("ThreatResolved", incident);
        }
        
        private void EscalateThreat(ActiveThreat threat)
        {
            // Phase 4.1.i: Critical threat escalation
            threat.Severity = 1f; // Cap at maximum
            threat.SpreadRadius *= 1.5f;
            
            Debug.LogWarning($"[Phase 4.1] CRITICAL: Threat {threat.ThreatId} has escalated to maximum severity!");
            
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("ThreatEscalated", threat);
        }
        
        // Risk calculation methods
        private float CalculateTemperatureRisk()
        {
            // Implement temperature-based risk assessment
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private float CalculateHumidityRisk()
        {
            // Implement humidity-based risk assessment
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private float CalculateAirflowRisk()
        {
            // Implement airflow-based risk assessment
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private float CalculateSanitationRisk()
        {
            // Implement sanitation-based risk assessment
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private float CalculatePlantStressRisk()
        {
            // Implement plant stress-based risk assessment
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private string SelectOriginLocation()
        {
            var locations = new[] { "VegetativeZone", "FloweringZone", "PropagationArea", "DryingRoom" };
            return locations[UnityEngine.Random.Range(0, locations.Length)];
        }
        
        private IPMResponse DetermineOptimalResponse(ThreatType threatType)
        {
            return threatType switch
            {
                ThreatType.SuckingInsects => IPMResponse.BiologicalControl,
                ThreatType.FungalDisease => IPMResponse.EnvironmentalModification,
                ThreatType.BacterialDisease => IPMResponse.QuarantineMeasures,
                ThreatType.ViralDisease => IPMResponse.PhysicalRemoval,
                _ => IPMResponse.PreventiveMaintenance
            };
        }
        
        private void RegisterEventListeners()
        {
            // Register for plant health updates, environmental changes, etc.
        }
        
        private void UnregisterEventListeners()
        {
            // Unregister event listeners
        }
        
        private void SaveThreatHistory()
        {
            // Save threat history to persistent storage
            Debug.Log($"[Phase 4.1] Saving {threatHistory.Count} threat incidents to history");
        }
        
        // Public API for other systems
        public List<ActiveThreat> GetActiveThreats() => new List<ActiveThreat>(activeThreats);
        public EnvironmentalRiskAssessment GetCurrentRisk() => currentRisk;
        public List<ThreatIncident> GetThreatHistory() => new List<ThreatIncident>(threatHistory);
        
        public bool ContainThreat(string threatId, float effectiveness)
        {
            var threat = activeThreats.FirstOrDefault(t => t.ThreatId == threatId);
            if (threat != null)
            {
                threat.ContainmentEffectiveness = Mathf.Clamp01(effectiveness);
                threat.IsContained = effectiveness > 0.7f;
                return true;
            }
            return false;
        }
        
        public void TriggerManualThreatScan()
        {
            AssessEnvironmentalRisk();
            EvaluateThreatGeneration();
            Debug.Log($"[Phase 4.1] Manual threat scan completed - Risk Level: {currentRisk.OverallRiskLevel:F2}");
        }
    }
}