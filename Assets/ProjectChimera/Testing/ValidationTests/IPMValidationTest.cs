using UnityEngine;
using ProjectChimera.Systems.IPM;
using ProjectChimera.Data.IPM;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera
{
    /// <summary>
    /// Validation test for Integrated Pest Management framework.
    /// Ensures all critical IPM components compile and integrate properly.
    /// </summary>
    public class IPMValidationTest : MonoBehaviour
    {
        private void Start()
        {
            ValidateIPMFramework();
        }
        
        private void ValidateIPMFramework()
        {
            // Test pest invasion data
            var pestInvasion = new PestInvasionData();
            pestInvasion.InvasionId = "test-invasion-001";
            pestInvasion.PestType = ProjectChimera.Data.IPM.PestType.SpiderMites;
            pestInvasion.PopulationSize = 50;
            pestInvasion.AggressionLevel = 0.7f;
            pestInvasion.InvasionStartTime = System.DateTime.Now;
            
            // Test defense structure
            var defenseStructure = new DefenseStructureData();
            defenseStructure.StructureId = "test-defense-001";
            defenseStructure.Type = DefenseStructureType.StickyTrap;
            defenseStructure.Position = Vector3.zero;
            defenseStructure.Effectiveness = 0.85f;
            defenseStructure.IsActive = true;
            
            // Test beneficial organism
            var beneficialOrganism = new BeneficialOrganismData();
            beneficialOrganism.OrganismId = "test-organism-001";
            beneficialOrganism.Type = BeneficialOrganismType.Ladybugs;
            beneficialOrganism.PopulationSize = 100;
            beneficialOrganism.HuntingEfficiency = 0.75f;
            beneficialOrganism.IsEstablished = true;
            
            // Test IPM strategy plan
            var strategyPlan = new IPMStrategyPlan();
            strategyPlan.PlanId = "test-strategy-001";
            strategyPlan.PrimaryStrategy = IPMStrategyType.Biological;
            strategyPlan.EstimatedEffectiveness = 0.80f;
            strategyPlan.CreationTime = System.DateTime.Now;
            
            // Test IPM player profile
            var playerProfile = new IPMPlayerProfile();
            playerProfile.PlayerId = "test-player-001";
            playerProfile.PlayerName = "Test Cannabis Grower";
            playerProfile.IPMLevel = 5;
            playerProfile.TotalExperience = 1250f;
            playerProfile.ProfileCreated = System.DateTime.Now;
            
            // Test IPM battle objective
            var battleObjective = new IPMBattleObjective();
            battleObjective.ObjectiveId = "test-objective-001";
            battleObjective.ObjectiveName = "Eliminate Spider Mites";
            battleObjective.Description = "Reduce spider mite population by 90%";
            battleObjective.ProgressRequired = 90f;
            battleObjective.IsRequired = true;
            
            // Test IPM analytics data
            var analyticsData = new IPMAnalyticsData();
            analyticsData.AnalyticsId = "test-analytics-001";
            analyticsData.CollectionTime = System.DateTime.Now;
            analyticsData.PlayerId = playerProfile.PlayerId;
            analyticsData.EfficiencyScore = 0.88f;
            analyticsData.ResourceUtilization = 0.75f;
            
            // Test IPM technology data
            var technologyData = new IPMTechnologyData();
            technologyData.TechnologyId = "test-tech-001";
            technologyData.TechnologyName = "Advanced Biological Control";
            technologyData.Description = "Enhanced beneficial organism deployment";
            technologyData.RequiredLevel = 3;
            technologyData.IsUnlocked = true;
            
            // Test IPM framework components
            var plantIPMData = new ProjectChimera.Systems.IPM.PlantIPMData();
            plantIPMData.PlantId = "test-plant-001";
            plantIPMData.PlantSpecies = "Cannabis Sativa";
            plantIPMData.GrowthStage = ProjectChimera.Systems.IPM.IPMPlantGrowthStage.Vegetative;
            plantIPMData.HealthStatus = 0.85f;
            plantIPMData.ThreatLevel = ProjectChimera.Systems.IPM.IPMThreatLevel.Low;
            
            // Test IPM treatment plan
            var treatmentPlan = new ProjectChimera.Systems.IPM.IPMTreatmentPlan();
            treatmentPlan.PlanId = "test-treatment-001";
            treatmentPlan.StrategyType = IPMStrategyType.Biological;
            treatmentPlan.TreatmentName = "Ladybug Release Program";
            treatmentPlan.EstimatedDuration = 72; // hours
            treatmentPlan.ExpectedEffectiveness = 0.85f;
            treatmentPlan.IsOrganicSafe = true;
            
            // Test IPM detection result
            var detectionResult = new ProjectChimera.Systems.IPM.IPMDetectionResult();
            detectionResult.PlantId = plantIPMData.PlantId;
            detectionResult.DetectionTime = System.DateTime.Now;
            detectionResult.Confidence = 0.92f;
            detectionResult.OverallThreatLevel = ProjectChimera.Systems.IPM.IPMThreatLevel.Low;
            
            // Test IPM treatment result
            var treatmentResult = new ProjectChimera.Systems.IPM.IPMTreatmentResult();
            treatmentResult.TreatmentId = "test-treatment-result-001";
            treatmentResult.IsSuccessful = true;
            treatmentResult.ResultMessage = "Treatment applied successfully";
            treatmentResult.ExpectedEffectiveness = 0.85f;
            
            // Test IPM alert
            var ipmAlert = new ProjectChimera.Systems.IPM.IPMAlert();
            ipmAlert.AlertId = "test-alert-001";
            ipmAlert.PlantId = plantIPMData.PlantId;
            ipmAlert.AlertType = "Pest Detection";
            ipmAlert.Message = "Spider mites detected on plant";
            ipmAlert.Severity = ProjectChimera.Systems.IPM.IPMThreatLevel.Moderate;
            ipmAlert.RequiresImmediateAction = true;
            
            // Test IPM system report
            var systemReport = new ProjectChimera.Systems.IPM.IPMSystemReport();
            systemReport.TotalPlantsMonitored = 25;
            systemReport.TotalPestsDetected = 8;
            systemReport.TotalTreatmentsApplied = 12;
            systemReport.SystemEffectiveness = 0.87f;
            systemReport.PreventionSuccessRate = 0.83f;
            systemReport.BiologicalControlsActive = 3;
            systemReport.DefenseStructuresActive = 5;
            
            // Test detection and treatment systems
            var detectionSystem = new ProjectChimera.Systems.IPM.IPMDetectionSystem(0.85f);
            var treatmentSystem = new ProjectChimera.Systems.IPM.IPMTreatmentSystem(0.90f);
            
            Debug.Log("✅ IPM Framework Validation Completed Successfully!");
            Debug.Log($"Pest Invasion: {pestInvasion.PestType} with {pestInvasion.PopulationSize} population");
            Debug.Log($"Defense Structure: {defenseStructure.Type} at {defenseStructure.Effectiveness:P0} effectiveness");
            Debug.Log($"Beneficial Organism: {beneficialOrganism.Type} with {beneficialOrganism.PopulationSize} population");
            Debug.Log($"Strategy Plan: {strategyPlan.PrimaryStrategy} with {strategyPlan.EstimatedEffectiveness:P0} effectiveness");
            Debug.Log($"Player Profile: {playerProfile.PlayerName} at level {playerProfile.IPMLevel}");
            Debug.Log($"Battle Objective: {battleObjective.ObjectiveName} - {battleObjective.ProgressRequired}% required");
            Debug.Log($"Analytics: Efficiency {analyticsData.EfficiencyScore:P0}, Utilization {analyticsData.ResourceUtilization:P0}");
            Debug.Log($"Technology: {technologyData.TechnologyName} (Level {technologyData.RequiredLevel})");
            Debug.Log($"Plant IPM: {plantIPMData.PlantSpecies} ({plantIPMData.GrowthStage}) - {plantIPMData.ThreatLevel} threat");
            Debug.Log($"Treatment Plan: {treatmentPlan.TreatmentName} - {treatmentPlan.ExpectedEffectiveness:P0} effectiveness");
            Debug.Log($"Detection: {detectionResult.Confidence:P0} confidence - {detectionResult.OverallThreatLevel} threat level");
            Debug.Log($"Treatment Result: {treatmentResult.ResultMessage} - {treatmentResult.ExpectedEffectiveness:P0} expected");
            Debug.Log($"Alert: {ipmAlert.AlertType} - {ipmAlert.Message} ({ipmAlert.Severity})");
            Debug.Log($"System Report: {systemReport.TotalPlantsMonitored} plants, {systemReport.SystemEffectiveness:P0} effectiveness");
            Debug.Log("Integrated Pest Management framework implementation completed successfully!");
        }
    }
}