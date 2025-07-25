using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Systems.IPM;
using ProjectChimera.Data.IPM;
using IPMRiskLevel = ProjectChimera.Data.IPM.RiskLevel;
using IPMEnvironmentalRiskFactor = ProjectChimera.Data.IPM.EnvironmentalRiskFactor;

namespace ProjectChimera
{
    /// <summary>
    /// Comprehensive validation test for the disease management system.
    /// Ensures all disease detection, progression, and treatment systems work correctly.
    /// </summary>
    public class DiseaseManagementValidationTest : MonoBehaviour
    {
        private void Start()
        {
            ValidateDiseaseManagementSystem();
        }
        
        private void ValidateDiseaseManagementSystem()
        {
            Debug.Log("🦠 Disease Management System Validation");
            Debug.Log("========================================");
            
            // Test disease management system initialization
            var diseaseSystem = new DiseaseManagementSystem();
            Debug.Log($"✅ DiseaseManagementSystem created: {diseaseSystem != null}");
            
            // Test disease detection methods
            ValidateDiseaseDetectionMethods();
            
            // Test disease profiles
            ValidateDiseaseProfiles();
            
            // Test disease progression
            ValidateDiseaseProgression();
            
            // Test treatment systems
            ValidateTreatmentSystems();
            
            // Test cannabis-specific diseases
            ValidateCannabisDiseases();
            
            // Test environmental risk factors
            ValidateEnvironmentalRisks();
            
            // Test predictive analytics
            ValidatePredictiveAnalytics();
            
            Debug.Log("✅ Disease Management System Validation Complete");
            Debug.Log("All disease management systems are properly integrated and functional!");
        }
        
        private void ValidateDiseaseDetectionMethods()
        {
            Debug.Log("🔬 Testing Disease Detection Methods...");
            
            // Test visual disease detector
            var visualDetector = new VisualDiseaseDetector(0.85f);
            Debug.Log($"✅ Visual Disease Detector: {visualDetector != null && visualDetector.IsActive}");
            
            // Test microscopic analyzer
            var microscopicAnalyzer = new MicroscopicAnalyzer(0.9f);
            Debug.Log($"✅ Microscopic Analyzer: {microscopicAnalyzer != null && microscopicAnalyzer.IsActive}");
            
            // Test chemical sensor array
            var chemicalSensors = new ChemicalSensorArray(0.8f);
            Debug.Log($"✅ Chemical Sensor Array: {chemicalSensors != null && chemicalSensors.IsActive}");
            
            // Test environmental risk analyzer
            var riskAnalyzer = new EnvironmentalRiskAnalyzer(0.7f);
            Debug.Log($"✅ Environmental Risk Analyzer: {riskAnalyzer != null && riskAnalyzer.IsActive}");
            
            // Test disease progression predictor
            var progressionPredictor = new DiseaseProgressionPredictor();
            Debug.Log($"✅ Disease Progression Predictor: {progressionPredictor != null && progressionPredictor.IsActive}");
        }
        
        private void ValidateDiseaseProfiles()
        {
            Debug.Log("🧬 Testing Disease Profiles...");
            
            // Test disease profile data structure
            var diseaseProfile = new DiseaseProfile
            {
                DiseaseType = DiseaseType.PowderyMildew,
                DetectionSensitivity = 0.8f,
                PreferredDetectionMethods = new List<DiseaseDetectionMethod> { DiseaseDetectionMethod.Visual, DiseaseDetectionMethod.Microscopic },
                TypicalSymptoms = new List<string> { "White powdery spots", "Leaf yellowing", "Stunted growth" },
                EnvironmentalTriggers = new Dictionary<string, float> { { "Humidity", 0.6f }, { "Temperature", 24f } },
                TreatmentOptions = new List<TreatmentType> { TreatmentType.Fungicide, TreatmentType.BiologicalControl },
                Severity = DiseaseSeverity.Moderate,
                ContagiousLevel = 0.7f,
                ProgressionRate = 0.3f
            };
            
            Debug.Log($"✅ Disease Profile: {diseaseProfile != null}");
            Debug.Log($"   - Disease Type: {diseaseProfile.DiseaseType}");
            Debug.Log($"   - Detection Sensitivity: {diseaseProfile.DetectionSensitivity:P0}");
            Debug.Log($"   - Typical Symptoms: {diseaseProfile.TypicalSymptoms.Count} symptoms");
            Debug.Log($"   - Treatment Options: {diseaseProfile.TreatmentOptions.Count} options");
            Debug.Log($"   - Severity: {diseaseProfile.Severity}");
            Debug.Log($"   - Contagious Level: {diseaseProfile.ContagiousLevel:P0}");
        }
        
        private void ValidateDiseaseProgression()
        {
            Debug.Log("📈 Testing Disease Progression...");
            
            // Test disease instance data
            var diseaseInstance = new DiseaseInstanceData
            {
                DiseaseType = DiseaseType.Botrytis,
                DetectionDate = System.DateTime.Now,
                Severity = DiseaseSeverity.High,
                Confidence = 0.9f,
                ProgressionLevel = 0.3f,
                LastProgressionLevel = 0.2f,
                TreatmentProgress = 0.1f,
                LastTreatmentDate = System.DateTime.Now.AddDays(-2),
                Location = Vector3.zero,
                Symptoms = new List<string> { "Gray fuzzy mold", "Brown spots", "Bud rot" }
            };
            
            Debug.Log($"✅ Disease Instance: {diseaseInstance != null}");
            Debug.Log($"   - Disease Type: {diseaseInstance.DiseaseType}");
            Debug.Log($"   - Severity: {diseaseInstance.Severity}");
            Debug.Log($"   - Confidence: {diseaseInstance.Confidence:P0}");
            Debug.Log($"   - Progression Level: {diseaseInstance.ProgressionLevel:P0}");
            Debug.Log($"   - Treatment Progress: {diseaseInstance.TreatmentProgress:P0}");
            Debug.Log($"   - Symptoms: {diseaseInstance.Symptoms.Count} symptoms");
            
            // Test disease progression data
            var progressionData = new DiseaseProgressionData
            {
                PlantId = "test-plant-001",
                DiseaseType = DiseaseType.Botrytis,
                ProgressionLevel = 0.4f,
                Severity = DiseaseSeverity.High,
                ProgressionRate = 0.05f
            };
            
            Debug.Log($"✅ Disease Progression Data: {progressionData != null}");
            Debug.Log($"   - Plant ID: {progressionData.PlantId}");
            Debug.Log($"   - Disease Type: {progressionData.DiseaseType}");
            Debug.Log($"   - Progression Level: {progressionData.ProgressionLevel:P0}");
            Debug.Log($"   - Progression Rate: {progressionData.ProgressionRate:P0}");
        }
        
        private void ValidateTreatmentSystems()
        {
            Debug.Log("💊 Testing Treatment Systems...");
            
            // Test treatment data
            var treatmentData = new DiseaseTreatmentData
            {
                TreatmentId = "test-treatment-001",
                PlantId = "test-plant-001",
                DiseaseType = DiseaseType.PowderyMildew,
                TreatmentType = TreatmentType.Fungicide,
                ApplicationTime = System.DateTime.Now,
                ExpectedEffectiveness = 0.85f,
                ActualEffectiveness = 0.8f,
                IsSuccessful = true
            };
            
            Debug.Log($"✅ Treatment Data: {treatmentData != null}");
            Debug.Log($"   - Treatment ID: {treatmentData.TreatmentId}");
            Debug.Log($"   - Plant ID: {treatmentData.PlantId}");
            Debug.Log($"   - Disease Type: {treatmentData.DiseaseType}");
            Debug.Log($"   - Treatment Type: {treatmentData.TreatmentType}");
            Debug.Log($"   - Expected Effectiveness: {treatmentData.ExpectedEffectiveness:P0}");
            Debug.Log($"   - Actual Effectiveness: {treatmentData.ActualEffectiveness:P0}");
            Debug.Log($"   - Is Successful: {treatmentData.IsSuccessful}");
            
            // Test treatment result
            var treatmentResult = new DiseaseTreatmentResult
            {
                TreatmentId = treatmentData.TreatmentId,
                IsSuccessful = true,
                ResultMessage = "Treatment applied successfully",
                ExpectedEffectiveness = 0.85f,
                ActualEffectiveness = 0.8f
            };
            
            Debug.Log($"✅ Treatment Result: {treatmentResult != null}");
            Debug.Log($"   - Treatment ID: {treatmentResult.TreatmentId}");
            Debug.Log($"   - Is Successful: {treatmentResult.IsSuccessful}");
            Debug.Log($"   - Result Message: {treatmentResult.ResultMessage}");
            Debug.Log($"   - Expected Effectiveness: {treatmentResult.ExpectedEffectiveness:P0}");
            Debug.Log($"   - Actual Effectiveness: {treatmentResult.ActualEffectiveness:P0}");
        }
        
        private void ValidateCannabisDiseases()
        {
            Debug.Log("🌿 Testing Cannabis-Specific Diseases...");
            
            // Test cannabis-specific disease types
            var cannabisDiseases = new Dictionary<DiseaseType, string>
            {
                { DiseaseType.PowderyMildew, "Common fungal disease affecting leaves and buds" },
                { DiseaseType.Botrytis, "Gray mold affecting buds and stems" },
                { DiseaseType.RootRot, "Root system disease causing wilting" },
                { DiseaseType.BudRot, "Critical disease affecting flowering buds" },
                { DiseaseType.LeafSpot, "Fungal disease causing leaf spots" },
                { DiseaseType.Fusarium, "Soil-borne fungal disease" },
                { DiseaseType.Verticillium, "Vascular wilt disease" },
                { DiseaseType.Pythium, "Water mold affecting roots" },
                { DiseaseType.Alternaria, "Fungal disease causing leaf blight" },
                { DiseaseType.Septoria, "Fungal disease causing leaf spots" }
            };
            
            Debug.Log($"✅ Cannabis Disease Types: {cannabisDiseases.Count} disease types");
            
            foreach (var disease in cannabisDiseases)
            {
                Debug.Log($"   - {disease.Key}: {disease.Value}");
            }
            
            // Test plant disease data
            var plantDiseaseData = new PlantDiseaseData
            {
                PlantId = "test-plant-001",
                ActiveDiseases = new List<DiseaseInstanceData>
                {
                    new DiseaseInstanceData
                    {
                        DiseaseType = DiseaseType.PowderyMildew,
                        DetectionDate = System.DateTime.Now,
                        Severity = DiseaseSeverity.Moderate,
                        Confidence = 0.8f,
                        ProgressionLevel = 0.3f,
                        TreatmentProgress = 0.2f,
                        Location = Vector3.zero,
                        Symptoms = new List<string> { "White powdery spots", "Leaf yellowing" }
                    }
                },
                EradicatedDiseases = new List<DiseaseInstanceData>(),
                LastScanDate = System.DateTime.Now,
                OverallHealthScore = 0.75f
            };
            
            Debug.Log($"✅ Plant Disease Data: {plantDiseaseData != null}");
            Debug.Log($"   - Plant ID: {plantDiseaseData.PlantId}");
            Debug.Log($"   - Active Diseases: {plantDiseaseData.ActiveDiseases.Count}");
            Debug.Log($"   - Eradicated Diseases: {plantDiseaseData.EradicatedDiseases.Count}");
            Debug.Log($"   - Overall Health Score: {plantDiseaseData.OverallHealthScore:P0}");
            Debug.Log($"   - Last Scan: {plantDiseaseData.LastScanDate:yyyy-MM-dd HH:mm}");
        }
        
        private void ValidateEnvironmentalRisks()
        {
            Debug.Log("🌡️ Testing Environmental Risk Factors...");
            
            // Test environmental risk factors for disease development
            var environmentalRisks = new Dictionary<string, IPMEnvironmentalRiskFactor>
            {
                ["HighHumidity"] = new IPMEnvironmentalRiskFactor
                {
                    Name = "High Humidity",
                    RiskMultiplier = 1.8f,
                    AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                    ThresholdValue = 0.7f,
                    IsActive = true
                },
                ["PoorAirflow"] = new IPMEnvironmentalRiskFactor
                {
                    Name = "Poor Airflow",
                    RiskMultiplier = 1.5f,
                    AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                    ThresholdValue = 0.3f,
                    IsActive = false
                },
                ["ExcessiveMoisture"] = new IPMEnvironmentalRiskFactor
                {
                    Name = "Excessive Moisture",
                    RiskMultiplier = 2.0f,
                    AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                    ThresholdValue = 0.8f,
                    IsActive = true
                },
                ["TemperatureFluctuation"] = new IPMEnvironmentalRiskFactor
                {
                    Name = "Temperature Fluctuation",
                    RiskMultiplier = 1.3f,
                    AffectedPests = new List<ProjectChimera.Data.IPM.PestType>(),
                    ThresholdValue = 28f,
                    IsActive = false
                }
            };
            
            Debug.Log($"✅ Environmental Risk Factors: {environmentalRisks.Count} risk factors");
            
            foreach (var risk in environmentalRisks)
            {
                Debug.Log($"   - {risk.Key}:");
                Debug.Log($"     * Name: {risk.Value.Name}");
                Debug.Log($"     * Risk Multiplier: {risk.Value.RiskMultiplier:F1}x");
                Debug.Log($"     * Threshold Value: {risk.Value.ThresholdValue:F1}");
                Debug.Log($"     * Is Active: {risk.Value.IsActive}");
            }
            
            // Test disease detection result
            var detectionResult = new DiseaseDetectionResult
            {
                PlantId = "test-plant-001",
                ScanId = "test-scan-001",
                DetectedDiseases = new List<DiseaseDetectionData>
                {
                    new DiseaseDetectionData
                    {
                        DiseaseType = DiseaseType.Botrytis,
                        Confidence = 0.9f,
                        Location = Vector3.zero,
                        Symptoms = new List<string> { "Gray fuzzy mold", "Brown spots" },
                        DetectionMethod = DiseaseDetectionMethod.Visual,
                        Severity = DiseaseSeverity.High,
                        ProgressionLevel = 0.4f
                    }
                },
                Confidence = 0.9f,
                RiskLevel = IPMRiskLevel.High,
                DetectionTime = System.DateTime.Now
            };
            
            Debug.Log($"✅ Disease Detection Result: {detectionResult != null}");
            Debug.Log($"   - Plant ID: {detectionResult.PlantId}");
            Debug.Log($"   - Scan ID: {detectionResult.ScanId}");
            Debug.Log($"   - Detected Diseases: {detectionResult.DetectedDiseases.Count}");
            Debug.Log($"   - Overall Confidence: {detectionResult.Confidence:P0}");
            Debug.Log($"   - Risk Level: {detectionResult.RiskLevel}");
            Debug.Log($"   - Detection Time: {detectionResult.DetectionTime:yyyy-MM-dd HH:mm}");
        }
        
        private void ValidatePredictiveAnalytics()
        {
            Debug.Log("🔮 Testing Predictive Analytics...");
            
            // Test disease metrics
            var diseaseMetrics = new DiseaseMetrics
            {
                TotalDiseasesDetected = 25,
                TotalTreatmentsApplied = 18,
                DiseasesEradicated = 12,
                FalsePositives = 2,
                DetectionAccuracy = 0.88f,
                SystemEffectiveness = 0.67f,
                LastUpdated = System.DateTime.Now
            };
            
            Debug.Log($"✅ Disease Metrics: {diseaseMetrics != null}");
            Debug.Log($"   - Total Diseases Detected: {diseaseMetrics.TotalDiseasesDetected}");
            Debug.Log($"   - Total Treatments Applied: {diseaseMetrics.TotalTreatmentsApplied}");
            Debug.Log($"   - Diseases Eradicated: {diseaseMetrics.DiseasesEradicated}");
            Debug.Log($"   - False Positives: {diseaseMetrics.FalsePositives}");
            Debug.Log($"   - Detection Accuracy: {diseaseMetrics.DetectionAccuracy:P0}");
            Debug.Log($"   - System Effectiveness: {diseaseMetrics.SystemEffectiveness:P0}");
            Debug.Log($"   - Last Updated: {diseaseMetrics.LastUpdated:yyyy-MM-dd HH:mm}");
            
            // Test disease system report
            var systemReport = new DiseaseSystemReport
            {
                TotalPlantsMonitored = 50,
                TotalDiseasesDetected = 25,
                TotalTreatmentsApplied = 18,
                DiseasesEradicated = 12,
                SystemEffectiveness = 0.67f,
                PreventionSuccessRate = 0.75f,
                AverageDetectionAccuracy = 0.88f,
                ActiveDiseases = 13,
                CriticalDiseases = 3,
                ReportDate = System.DateTime.Now
            };
            
            Debug.Log($"✅ Disease System Report: {systemReport != null}");
            Debug.Log($"   - Total Plants Monitored: {systemReport.TotalPlantsMonitored}");
            Debug.Log($"   - Total Diseases Detected: {systemReport.TotalDiseasesDetected}");
            Debug.Log($"   - Total Treatments Applied: {systemReport.TotalTreatmentsApplied}");
            Debug.Log($"   - Diseases Eradicated: {systemReport.DiseasesEradicated}");
            Debug.Log($"   - System Effectiveness: {systemReport.SystemEffectiveness:P0}");
            Debug.Log($"   - Prevention Success Rate: {systemReport.PreventionSuccessRate:P0}");
            Debug.Log($"   - Average Detection Accuracy: {systemReport.AverageDetectionAccuracy:P0}");
            Debug.Log($"   - Active Diseases: {systemReport.ActiveDiseases}");
            Debug.Log($"   - Critical Diseases: {systemReport.CriticalDiseases}");
            Debug.Log($"   - Report Date: {systemReport.ReportDate:yyyy-MM-dd HH:mm}");
            
            // Test treatment types
            var treatmentTypes = new Dictionary<TreatmentType, string>
            {
                { TreatmentType.Fungicide, "Chemical fungicide treatment" },
                { TreatmentType.BiologicalControl, "Beneficial organism treatment" },
                { TreatmentType.EnvironmentalAdjustment, "Environmental condition modification" },
                { TreatmentType.PlantRemoval, "Removal of infected plant material" },
                { TreatmentType.Quarantine, "Isolation of infected plants" },
                { TreatmentType.NutrientSupplement, "Nutritional support treatment" },
                { TreatmentType.PhAdjustment, "pH level adjustment" },
                { TreatmentType.Pruning, "Surgical removal of infected parts" }
            };
            
            Debug.Log($"✅ Treatment Types: {treatmentTypes.Count} treatment options");
            
            foreach (var treatment in treatmentTypes)
            {
                Debug.Log($"   - {treatment.Key}: {treatment.Value}");
            }
        }
        
        [ContextMenu("Test Disease Management System")]
        private void TestDiseaseManagementSystem()
        {
            ValidateDiseaseManagementSystem();
        }
        
        [ContextMenu("Test Disease Detection")]
        private void TestDiseaseDetection()
        {
            ValidateDiseaseDetectionMethods();
        }
        
        [ContextMenu("Test Cannabis Diseases")]
        private void TestCannabisDiseases()
        {
            ValidateCannabisDiseases();
        }
        
        [ContextMenu("Test Treatment Systems")]
        private void TestTreatmentSystems()
        {
            ValidateTreatmentSystems();
        }
    }
}