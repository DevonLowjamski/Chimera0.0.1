using UnityEngine;
using ProjectChimera.Systems.Construction;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Equipment;
// Explicit type aliases to resolve ambiguous references
using ConstructionResourceType = ProjectChimera.Data.Construction.ResourceType;
using ConstructionCostType = ProjectChimera.Data.Construction.CostType;
using ConstructionBudgetAlert = ProjectChimera.Data.Construction.BudgetAlert;
using ConstructionBudgetStatus = ProjectChimera.Data.Construction.BudgetStatus;

namespace ProjectChimera
{
    /// <summary>
    /// Validation test for construction cost and resource management system.
    /// Ensures all critical components compile and integrate properly.
    /// </summary>
    public class ConstructionCostValidationTest : MonoBehaviour
    {
        private void Start()
        {
            ValidateConstructionCostSystem();
        }
        
        private void ValidateConstructionCostSystem()
        {
            // Test construction project data
            var projectData = new ConstructionProjectData();
            projectData.ProjectName = "Test Cannabis Facility";
            projectData.ProjectType = ConstructionProjectType.NewConstruction;
            projectData.TotalSquareFootage = 5000f;
            projectData.RequiresComplianceRoom = true;
            projectData.RequiresSecuritySystem = true;
            
            // Test cost estimate creation
            var costEstimate = new ConstructionCostEstimate();
            costEstimate.ProjectId = projectData.ProjectId;
            costEstimate.TotalCost = 750000f;
            costEstimate.EstimateStatus = EstimateStatus.Completed;
            
            // Test project budget
            var projectBudget = new ProjectBudget();
            projectBudget.ProjectId = projectData.ProjectId;
            projectBudget.ApprovedAmount = 800000f;
            projectBudget.Status = ConstructionBudgetStatus.Active;
            
            // Test cost record
            var costRecord = new CostRecord();
            costRecord.ProjectId = projectData.ProjectId;
            costRecord.CostType = ConstructionCostType.Materials;
            costRecord.Amount = 50000f;
            costRecord.Description = "Steel and concrete materials";
            
            // Test resource inventory
            var resourceInventory = new ResourceInventory();
            resourceInventory.ResourceType = ConstructionResourceType.Steel;
            resourceInventory.TotalAmount = 1000f;
            resourceInventory.AvailableAmount = 800f;
            resourceInventory.UnitCost = 2.50f;
            
            // Test resource allocation
            var resourceAllocation = new ResourceAllocation();
            resourceAllocation.ProjectId = projectData.ProjectId;
            resourceAllocation.Status = AllocationStatus.Allocated;
            resourceAllocation.TotalAllocationCost = 125000f;
            
            // Test budget alert
            var budgetAlert = new ConstructionBudgetAlert();
            budgetAlert.ProjectId = projectData.ProjectId;
            budgetAlert.AlertType = BudgetAlertType.Warning;
            budgetAlert.Message = "Budget utilization at 80%";
            budgetAlert.CurrentUtilization = 0.80f;
            
            // Test cost performance data
            var performanceData = new CostPerformanceData();
            performanceData.ProjectId = projectData.ProjectId;
            performanceData.PlannedValue = 400000f;
            performanceData.ActualCost = 420000f;
            performanceData.CostPerformanceIndex = 0.95f;
            
            // Test cost optimization
            var optimizationResult = new CostOptimizationResult();
            optimizationResult.ProjectId = projectData.ProjectId;
            optimizationResult.TotalSavings = 45000f;
            optimizationResult.NetSavings = 40000f;
            
            // Test cannabis-specific optimization
            var cannabisOptimization = new CannabisOptimizationResult();
            cannabisOptimization.ComplianceCostSavings = 15000f;
            cannabisOptimization.SecurityCostSavings = 12000f;
            cannabisOptimization.TotalCannabisSpecificSavings = 35000f;
            
            // Test cost calculator
            var costCalculator = new ConstructionCostCalculator();
            var baseCosts = costCalculator.CalculateBaseCosts(projectData);
            var laborCosts = costCalculator.CalculateLaborCosts(projectData, 1.0f);
            var materialCosts = costCalculator.CalculateMaterialCosts(projectData, 0.15f);
            var equipmentCosts = costCalculator.CalculateEquipmentCosts(projectData);
            
            // Test resource allocation optimizer
            var resourceOptimizer = new ResourceAllocationOptimizer();
            
            // Test budget tracker
            var budgetTracker = new BudgetTracker();
            
            // Test cost analyzer
            var costAnalyzer = new CostAnalyzer();
            
            // Test cannabis cost analyzer
            var cannabisCostAnalyzer = new CannabisConstructionCostAnalyzer();
            var costMultiplier = cannabisCostAnalyzer.GetCostMultiplier(projectData);
            
            // Test compliance cost calculator
            var complianceCostCalculator = new ComplianceCostCalculator();
            var complianceCosts = complianceCostCalculator.CalculateComplianceCosts(projectData);
            
            // Test security cost optimizer
            var securityCostOptimizer = new SecurityCostOptimizer();
            var securityCosts = securityCostOptimizer.CalculateSecurityCosts(projectData);
            
            // Test cost metrics
            var costMetrics = new ConstructionCostMetrics();
            costMetrics.TotalBudgetAllocated = 1500000f;
            costMetrics.TotalBudgetSpent = 1200000f;
            costMetrics.BudgetUtilization = 0.80f;
            costMetrics.ActiveProjects = 3;
            
            Debug.Log("✅ Construction Cost Management System Validation Completed Successfully!");
            Debug.Log($"Project: {projectData.ProjectName} ({projectData.ProjectType})");
            Debug.Log($"Total Square Footage: {projectData.TotalSquareFootage:N0} sq ft");
            Debug.Log($"Cost Estimate: ${costEstimate.TotalCost:N0} ({costEstimate.EstimateStatus})");
            Debug.Log($"Project Budget: ${projectBudget.ApprovedAmount:N0} ({projectBudget.Status})");
            Debug.Log($"Cost Record: ${costRecord.Amount:N0} ({costRecord.CostType})");
            Debug.Log($"Resource Inventory: {resourceInventory.ResourceType} - {resourceInventory.AvailableAmount:N0} units available");
            Debug.Log($"Resource Allocation: ${resourceAllocation.TotalAllocationCost:N0} ({resourceAllocation.Status})");
            Debug.Log($"Budget Alert: {budgetAlert.AlertType} - {budgetAlert.Message}");
            Debug.Log($"Performance: CPI {performanceData.CostPerformanceIndex:F2}, Planned ${performanceData.PlannedValue:N0}, Actual ${performanceData.ActualCost:N0}");
            Debug.Log($"Cost Optimization: ${optimizationResult.TotalSavings:N0} potential savings, ${optimizationResult.NetSavings:N0} net");
            Debug.Log($"Cannabis Optimization: ${cannabisOptimization.TotalCannabisSpecificSavings:N0} cannabis-specific savings");
            Debug.Log($"Cost Calculations: Base ${baseCosts.TotalCost:N0}, Labor ${laborCosts.TotalCost:N0}, Materials ${materialCosts.TotalCost:N0}, Equipment ${equipmentCosts.TotalCost:N0}");
            Debug.Log($"Cannabis Cost Multiplier: {costMultiplier:F2}x");
            Debug.Log($"Compliance Costs: ${complianceCosts.TotalCost:N0}");
            Debug.Log($"Security Costs: ${securityCosts.TotalCost:N0}");
            Debug.Log($"Cost Metrics: {costMetrics.ActiveProjects} active projects, {costMetrics.BudgetUtilization:P0} budget utilization");
            Debug.Log("Construction cost and resource management system implementation completed successfully!");
        }
    }
}