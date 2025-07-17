using UnityEngine;
using ProjectChimera.Systems.IPM;
using ProjectChimera.Data.IPM;

namespace ProjectChimera
{
    /// <summary>
    /// Simple compilation test for IPM system to verify all types are resolved.
    /// </summary>
    public class IPMCompilationTest : MonoBehaviour
    {
        private void Start()
        {
            TestIPMCompilation();
        }
        
        private void TestIPMCompilation()
        {
            // Test IPM Framework
            var ipmFramework = new IPMFramework();
            
            // Test Clean IPM Manager
            var cleanManager = new CleanIPMManager();
            
            // Test IPM data structures
            var pestData = new PestInvasionData();
            pestData.PestType = PestType.Aphids;
            pestData.PopulationSize = 50;
            
            var organism = new BeneficialOrganismData();
            organism.Type = BeneficialOrganismType.Ladybugs;
            organism.PopulationSize = 100;
            
            var defenseStructure = new DefenseStructureData();
            defenseStructure.Type = DefenseStructureType.StickyTrap;
            defenseStructure.Effectiveness = 0.8f;
            
            // Test IPM Framework types
            var plantData = new PlantIPMData();
            plantData.GrowthStage = IPMPlantGrowthStage.Vegetative;
            plantData.ThreatLevel = IPMThreatLevel.Low;
            
            var treatmentPlan = new IPMTreatmentPlan();
            treatmentPlan.StrategyType = IPMStrategyType.Biological;
            
            var detectionResult = new IPMDetectionResult();
            detectionResult.OverallThreatLevel = IPMThreatLevel.Moderate;
            
            var treatmentResult = new IPMTreatmentResult();
            treatmentResult.IsSuccessful = true;
            
            var alert = new IPMAlert();
            alert.Severity = IPMThreatLevel.High;
            
            var systemReport = new IPMSystemReport();
            systemReport.SystemEffectiveness = 0.85f;
            
            Debug.Log("✅ IPM Compilation Test Passed - All types resolved successfully!");
            Debug.Log($"IPM Framework: {ipmFramework != null}");
            Debug.Log($"Clean Manager: {cleanManager != null}");
            Debug.Log($"Pest Data: {pestData.PestType}");
            Debug.Log($"Organism: {organism.Type}");
            Debug.Log($"Defense: {defenseStructure.Type}");
            Debug.Log($"Plant IPM: {plantData.GrowthStage}");
            Debug.Log($"Treatment Plan: {treatmentPlan.StrategyType}");
            Debug.Log($"Detection: {detectionResult.OverallThreatLevel}");
            Debug.Log($"Treatment Result: {treatmentResult.IsSuccessful}");
            Debug.Log($"Alert: {alert.Severity}");
            Debug.Log($"System Report: {systemReport.SystemEffectiveness:F2}");
        }
    }
}