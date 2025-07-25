using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Data.Construction
{
    /// <summary>
    /// Comprehensive data structures for construction cost and resource management.
    /// Supports cannabis-specific costing, resource optimization, and budget tracking.
    /// </summary>
    
    // Cost and Resource Enums
    public enum CostType
    {
        Labor,
        Materials,
        Equipment,
        Permits,
        Utilities,
        Transportation,
        Security,
        Compliance,
        Contingency,
        Overhead,
        Profit
    }
    
    public enum ResourceType
    {
        Steel,
        Concrete,
        Lumber,
        Electrical,
        Plumbing,
        HVAC,
        Insulation,
        Drywall,
        Flooring,
        Lighting,
        Security,
        Automation,
        Specialized
    }
    
    public enum BudgetStatus
    {
        Draft,
        Approved,
        Active,
        OnHold,
        Completed,
        Cancelled,
        Overbudget
    }
    
    public enum EstimateStatus
    {
        Draft,
        InProgress,
        Completed,
        Approved,
        Rejected,
        Expired
    }
    
    public enum AllocationStatus
    {
        Pending,
        Allocated,
        PartialAllocation,
        ResourceShortage,
        Cancelled,
        Completed
    }
    
    public enum BudgetAlertType
    {
        Information,
        Warning,
        Critical,
        Emergency
    }
    
    public enum ConstructionProjectType
    {
        NewConstruction,
        Renovation,
        Expansion,
        EquipmentInstallation,
        Compliance,
        Security,
        Infrastructure
    }
    
    // Core Cost Data Structures
    [System.Serializable]
    public class ConstructionCostEstimate
    {
        [Header("Estimate Identity")]
        public string EstimateId;
        public string ProjectId;
        public ConstructionProjectType ProjectType;
        public DateTime EstimateDate;
        public DateTime EstimateValidUntil;
        public EstimateStatus EstimateStatus;
        
        [Header("Cost Categories")]
        public CostCategory BaseCosts;
        public CostCategory LaborCosts;
        public CostCategory MaterialCosts;
        public CostCategory EquipmentCosts;
        public CostCategory ComplianceCosts;
        public CostCategory SecurityCosts;
        public CostCategory UtilityCosts;
        
        [Header("Cost Totals")]
        public float SubtotalCost;
        public float ContingencyCost;
        public float TotalCost;
        public float CostPerSquareFoot;
        public float CostVariance;
        
        [Header("Cannabis-Specific")]
        public float CannabisComplianceMultiplier = 1.25f;
        public float SecurityRequirementMultiplier = 1.50f;
        public float HVACRequirementMultiplier = 1.30f;
        public float LightingRequirementMultiplier = 1.40f;
        
        [Header("Estimate Metadata")]
        public string EstimatedBy;
        public string ApprovedBy;
        public DateTime LastUpdated;
        public List<EstimateAssumption> Assumptions = new List<EstimateAssumption>();
        public List<EstimateRisk> Risks = new List<EstimateRisk>();
        
        public ConstructionCostEstimate()
        {
            EstimateId = Guid.NewGuid().ToString();
            EstimateDate = DateTime.Now;
            EstimateValidUntil = DateTime.Now.AddDays(30);
            LastUpdated = DateTime.Now;
            Assumptions = new List<EstimateAssumption>();
            Risks = new List<EstimateRisk>();
            
            // Initialize cost categories
            BaseCosts = new CostCategory { CategoryName = "Base Construction" };
            LaborCosts = new CostCategory { CategoryName = "Labor" };
            MaterialCosts = new CostCategory { CategoryName = "Materials" };
            EquipmentCosts = new CostCategory { CategoryName = "Equipment" };
            ComplianceCosts = new CostCategory { CategoryName = "Compliance" };
            SecurityCosts = new CostCategory { CategoryName = "Security" };
            UtilityCosts = new CostCategory { CategoryName = "Utilities" };
        }
    }
    
    [System.Serializable]
    public class CostCategory
    {
        public string CategoryName;
        public float TotalCost;
        public Dictionary<CostType, float> CostBreakdown = new Dictionary<CostType, float>();
        public List<CostLineItem> LineItems = new List<CostLineItem>();
        public float CostPerUnit;
        public string Unit;
        public float Markup;
        public float Tax;
        
        public CostCategory()
        {
            CostBreakdown = new Dictionary<CostType, float>();
            LineItems = new List<CostLineItem>();
        }
    }
    
    [System.Serializable]
    public class CostLineItem
    {
        public string ItemId;
        public string Description;
        public float Quantity;
        public string Unit;
        public float UnitCost;
        public float TotalCost;
        public CostType CostType;
        public string VendorId;
        public DateTime PriceDate;
        public bool IsEstimated;
        
        public CostLineItem()
        {
            ItemId = Guid.NewGuid().ToString();
            PriceDate = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class ProjectBudget
    {
        [Header("Budget Identity")]
        public string BudgetId;
        public string ProjectId;
        public string BudgetName;
        public BudgetStatus Status;
        
        [Header("Budget Amounts")]
        public float ApprovedAmount;
        public float EstimatedCost;
        public float RemainingAmount;
        public float ContingencyReserve;
        public float SpentAmount;
        
        [Header("Budget Categories")]
        public Dictionary<CostType, float> CategoryBudgets = new Dictionary<CostType, float>();
        public Dictionary<CostType, float> CategorySpent = new Dictionary<CostType, float>();
        public Dictionary<CostType, float> CategoryRemaining = new Dictionary<CostType, float>();
        
        [Header("Budget Tracking")]
        public List<CostRecord> ActualCosts = new List<CostRecord>();
        public List<BudgetRevision> Revisions = new List<BudgetRevision>();
        public DateTime CreatedDate;
        public DateTime LastUpdated;
        
        [Header("Performance Metrics")]
        public float BudgetUtilization;
        public float CostVariance;
        public float ScheduleVariance;
        public float EstimateAtCompletion;
        
        public ProjectBudget()
        {
            BudgetId = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            CategoryBudgets = new Dictionary<CostType, float>();
            CategorySpent = new Dictionary<CostType, float>();
            CategoryRemaining = new Dictionary<CostType, float>();
            ActualCosts = new List<CostRecord>();
            Revisions = new List<BudgetRevision>();
        }
    }
    
    [System.Serializable]
    public class CostRecord
    {
        public string RecordId;
        public string ProjectId;
        public CostType CostType;
        public float Amount;
        public string Description;
        public string VendorId;
        public string InvoiceNumber;
        public DateTime RecordDate;
        public DateTime PaymentDate;
        public bool IsApproved;
        public string ApprovedBy;
        
        public CostRecord()
        {
            RecordId = Guid.NewGuid().ToString();
            RecordDate = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class ResourceInventory
    {
        [Header("Resource Identity")]
        public ResourceType ResourceType;
        public string ResourceName;
        public string ResourceDescription;
        public string Unit;
        
        [Header("Inventory Levels")]
        public float TotalAmount;
        public float AvailableAmount;
        public float AllocatedAmount;
        public float ReservedAmount;
        public float ReorderPoint;
        public float MaxStockLevel;
        
        [Header("Cost Information")]
        public float UnitCost;
        public float TotalValue;
        public float AverageCost;
        public DateTime LastCostUpdate;
        
        [Header("Tracking Information")]
        public string SupplierId;
        public int LeadTimeDays;
        public float MinimumOrderQuantity;
        public DateTime LastUpdated;
        public List<ResourceTransaction> Transactions = new List<ResourceTransaction>();
        
        public ResourceInventory()
        {
            LastCostUpdate = DateTime.Now;
            LastUpdated = DateTime.Now;
            Transactions = new List<ResourceTransaction>();
        }
    }
    
    [System.Serializable]
    public class ResourceTransaction
    {
        public string TransactionId;
        public string ProjectId;
        public ResourceType ResourceType;
        public TransactionType TransactionType;
        public float Quantity;
        public float UnitCost;
        public float TotalCost;
        public DateTime TransactionDate;
        public string Reference;
        
        public ResourceTransaction()
        {
            TransactionId = Guid.NewGuid().ToString();
            TransactionDate = DateTime.Now;
        }
    }
    
    public enum TransactionType
    {
        Purchase,
        Allocation,
        Return,
        Adjustment,
        Waste,
        Transfer
    }
    
    [System.Serializable]
    public class ResourceAllocation
    {
        [Header("Allocation Identity")]
        public string AllocationId;
        public string ProjectId;
        public DateTime AllocationDate;
        public AllocationStatus Status;
        
        [Header("Resource Details")]
        public Dictionary<ResourceType, float> AllocatedResources = new Dictionary<ResourceType, float>();
        public Dictionary<ResourceType, float> AllocationCosts = new Dictionary<ResourceType, float>();
        public float TotalAllocationCost;
        
        [Header("Allocation Tracking")]
        public DateTime RequestedDate;
        public DateTime CompletedDate;
        public string AllocatedBy;
        public string RequestedBy;
        public string Notes;
        
        public ResourceAllocation()
        {
            AllocationId = Guid.NewGuid().ToString();
            AllocationDate = DateTime.Now;
            RequestedDate = DateTime.Now;
            AllocatedResources = new Dictionary<ResourceType, float>();
            AllocationCosts = new Dictionary<ResourceType, float>();
        }
    }
    
    [System.Serializable]
    public class ResourceAvailabilityCheck
    {
        public bool IsAvailable;
        public List<ResourceType> UnavailableResources = new List<ResourceType>();
        public Dictionary<ResourceType, float> ShortageAmounts = new Dictionary<ResourceType, float>();
        public DateTime CheckDate;
        
        public ResourceAvailabilityCheck()
        {
            CheckDate = DateTime.Now;
            UnavailableResources = new List<ResourceType>();
            ShortageAmounts = new Dictionary<ResourceType, float>();
        }
    }
    
    [System.Serializable]
    public class BudgetAlert
    {
        public string AlertId;
        public string ProjectId;
        public string BudgetId;
        public BudgetAlertType AlertType;
        public string Message;
        public float Threshold;
        public float CurrentUtilization;
        public DateTime CreatedDate;
        public bool IsResolved;
        public DateTime ResolvedDate;
        
        public BudgetAlert()
        {
            AlertId = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class CostPerformanceData
    {
        [Header("Performance Identity")]
        public string ProjectId;
        public string BudgetId;
        public DateTime StartDate;
        public DateTime LastUpdated;
        
        [Header("Cost Performance")]
        public float PlannedValue;
        public float ActualCost;
        public float EarnedValue;
        public float CostVariance;
        public float ScheduleVariance;
        public float CostPerformanceIndex;
        public float SchedulePerformanceIndex;
        
        [Header("Forecasting")]
        public float EstimateAtCompletion;
        public float EstimateToComplete;
        public float VarianceAtCompletion;
        public float ToCompletePerformanceIndex;
        
        public CostPerformanceData()
        {
            StartDate = DateTime.Now;
            LastUpdated = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class CostOptimizationOpportunity
    {
        public string OpportunityId;
        public string ProjectId;
        public string Description;
        public float PotentialSavings;
        public float ImplementationCost;
        public float NetSavings;
        public OptimizationType OptimizationType;
        public float RiskLevel;
        public DateTime IdentifiedDate;
        public bool IsImplemented;
        
        public CostOptimizationOpportunity()
        {
            OpportunityId = Guid.NewGuid().ToString();
            IdentifiedDate = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class CostOptimizationResult
    {
        public string OptimizationId;
        public string ProjectId;
        public float TotalSavings;
        public float ImplementationCost;
        public float NetSavings;
        public List<CostOptimizationOpportunity> Opportunities = new List<CostOptimizationOpportunity>();
        public CannabisOptimizationResult CannabisOptimizations;
        public DateTime CompletedDate;
        
        public CostOptimizationResult()
        {
            OptimizationId = Guid.NewGuid().ToString();
            CompletedDate = DateTime.Now;
            Opportunities = new List<CostOptimizationOpportunity>();
        }
    }
    
    [System.Serializable]
    public class CannabisOptimizationResult
    {
        public float ComplianceCostSavings;
        public float SecurityCostSavings;
        public float HVACOptimizationSavings;
        public float LightingOptimizationSavings;
        public float TotalCannabisSpecificSavings;
        public List<string> ComplianceRecommendations = new List<string>();
        public List<string> SecurityRecommendations = new List<string>();
        
        public CannabisOptimizationResult()
        {
            ComplianceRecommendations = new List<string>();
            SecurityRecommendations = new List<string>();
        }
    }
    
    [System.Serializable]
    public class ConstructionProjectData
    {
        [Header("Project Identity")]
        public string ProjectId;
        public string ProjectName;
        public ConstructionProjectType ProjectType;
        
        [Header("Project Specifications")]
        public float TotalSquareFootage;
        public int NumberOfRooms;
        public List<RoomType> RoomTypes = new List<RoomType>();
        public Dictionary<EquipmentType, int> EquipmentRequirements = new Dictionary<EquipmentType, int>();
        
        [Header("Site Information")]
        public string SiteLocation;
        public float SiteSize;
        public bool IsExistingBuilding;
        public int BuildingAge;
        public string BuildingCondition;
        
        [Header("Cannabis-Specific Requirements")]
        public bool RequiresComplianceRoom;
        public bool RequiresSecuritySystem;
        public bool RequiresSpecializedHVAC;
        public bool RequiresControlledAccess;
        public float CannabisSpecificSquareFootage;
        
        [Header("Timeline")]
        public DateTime ProjectStartDate;
        public DateTime ProjectEndDate;
        public int EstimatedDurationDays;
        
        public ConstructionProjectData()
        {
            ProjectId = Guid.NewGuid().ToString();
            ProjectStartDate = DateTime.Now;
            RoomTypes = new List<RoomType>();
            EquipmentRequirements = new Dictionary<EquipmentType, int>();
        }
    }
    
    [System.Serializable]
    public class EstimateAssumption
    {
        public string AssumptionId;
        public string Description;
        public float ImpactOnCost;
        public float ProbabilityOfChange;
        public string MitigationStrategy;
        
        public EstimateAssumption()
        {
            AssumptionId = Guid.NewGuid().ToString();
        }
    }
    
    [System.Serializable]
    public class EstimateRisk
    {
        public string RiskId;
        public string Description;
        public float ProbabilityPercent;
        public float ImpactAmount;
        public float ExpectedValue;
        public string MitigationPlan;
        
        public EstimateRisk()
        {
            RiskId = Guid.NewGuid().ToString();
        }
    }
    
    [System.Serializable]
    public class BudgetRevision
    {
        public string RevisionId;
        public string BudgetId;
        public float PreviousAmount;
        public float NewAmount;
        public float ChangeAmount;
        public string ChangeReason;
        public DateTime RevisionDate;
        public string RevisedBy;
        public string ApprovedBy;
        
        public BudgetRevision()
        {
            RevisionId = Guid.NewGuid().ToString();
            RevisionDate = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class ResourceInventoryStatus
    {
        public int TotalResources;
        public int LowStockResources;
        public float TotalInventoryValue;
        public float AvailableInventoryValue;
        public float InventoryUtilization;
        public DateTime LastUpdated;
        
        public ResourceInventoryStatus()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class ConstructionCostMetrics
    {
        [Header("Budget Metrics")]
        public float TotalBudgetAllocated;
        public float TotalBudgetSpent;
        public float BudgetUtilization;
        public int ActiveProjects;
        public float AverageCostPerProject;
        public float AverageCostPerSquareFoot;
        
        [Header("Performance Metrics")]
        public float AverageCostVariance;
        public float AverageScheduleVariance;
        public float ProjectCompletionRate;
        public float BudgetAccuracy;
        
        [Header("Resource Metrics")]
        public float ResourceUtilization;
        public float AverageResourceCost;
        public int ResourceShortageEvents;
        public float WastePercentage;
        
        [Header("Cannabis-Specific Metrics")]
        public float CannabisComplianceCostRatio;
        public float SecurityCostRatio;
        public float HVACCostRatio;
        public float LightingCostRatio;
        
        [Header("Tracking")]
        public DateTime LastUpdated;
        
        public ConstructionCostMetrics()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    // Supporting Classes for Cost Management System
    [System.Serializable]
    public class ConstructionCostCalculator
    {
        public CostCategory CalculateBaseCosts(ConstructionProjectData projectData)
        {
            var category = new CostCategory { CategoryName = "Base Construction" };
            
            // Calculate base construction costs per square foot
            float baseCostPerSqFt = projectData.ProjectType switch
            {
                ConstructionProjectType.NewConstruction => 150f,
                ConstructionProjectType.Renovation => 100f,
                ConstructionProjectType.Expansion => 120f,
                _ => 100f
            };
            
            category.TotalCost = projectData.TotalSquareFootage * baseCostPerSqFt;
            category.CostPerUnit = baseCostPerSqFt;
            category.Unit = "sq ft";
            
            return category;
        }
        
        public CostCategory CalculateLaborCosts(ConstructionProjectData projectData, float laborMultiplier)
        {
            var category = new CostCategory { CategoryName = "Labor" };
            
            // Calculate labor costs based on project complexity
            float laborCostPerSqFt = projectData.ProjectType switch
            {
                ConstructionProjectType.NewConstruction => 50f,
                ConstructionProjectType.Renovation => 40f,
                ConstructionProjectType.Expansion => 45f,
                _ => 35f
            };
            
            category.TotalCost = projectData.TotalSquareFootage * laborCostPerSqFt * laborMultiplier;
            category.CostPerUnit = laborCostPerSqFt * laborMultiplier;
            category.Unit = "sq ft";
            
            return category;
        }
        
        public CostCategory CalculateMaterialCosts(ConstructionProjectData projectData, float markupPercentage)
        {
            var category = new CostCategory { CategoryName = "Materials" };
            
            // Calculate material costs
            float materialCostPerSqFt = projectData.ProjectType switch
            {
                ConstructionProjectType.NewConstruction => 75f,
                ConstructionProjectType.Renovation => 60f,
                ConstructionProjectType.Expansion => 65f,
                _ => 50f
            };
            
            float baseCost = projectData.TotalSquareFootage * materialCostPerSqFt;
            category.TotalCost = baseCost * (1f + markupPercentage);
            category.CostPerUnit = materialCostPerSqFt * (1f + markupPercentage);
            category.Unit = "sq ft";
            category.Markup = markupPercentage;
            
            return category;
        }
        
        public CostCategory CalculateEquipmentCosts(ConstructionProjectData projectData)
        {
            var category = new CostCategory { CategoryName = "Equipment" };
            
            // Calculate equipment costs based on requirements
            float totalEquipmentCost = 0f;
            
            foreach (var equipmentReq in projectData.EquipmentRequirements)
            {
                float unitCost = GetEquipmentUnitCost(equipmentReq.Key);
                totalEquipmentCost += unitCost * equipmentReq.Value;
            }
            
            category.TotalCost = totalEquipmentCost;
            category.CostPerUnit = projectData.EquipmentRequirements.Count > 0 ? 
                totalEquipmentCost / projectData.EquipmentRequirements.Values.Sum() : 0f;
            category.Unit = "unit";
            
            return category;
        }
        
        private float GetEquipmentUnitCost(EquipmentType equipmentType)
        {
            return equipmentType switch
            {
                EquipmentType.LED_Light => 500f,
                EquipmentType.HPS_Light => 300f,
                EquipmentType.Exhaust_Fan => 150f,
                EquipmentType.Air_Conditioner => 2000f,
                EquipmentType.Dehumidifier => 800f,
                EquipmentType.Humidifier => 300f,
                EquipmentType.Water_Pump => 200f,
                EquipmentType.pH_Controller => 400f,
                EquipmentType.Security_Camera => 250f,
                EquipmentType.Environmental_Controller => 1500f,
                _ => 100f
            };
        }
    }
    
    [System.Serializable]
    public class ResourceAllocationOptimizer
    {
        public ResourceAllocation OptimizeAllocation(Dictionary<ResourceType, float> requirements, 
            Dictionary<ResourceType, ResourceInventory> inventory)
        {
            // Placeholder optimization logic
            return new ResourceAllocation();
        }
    }
    
    [System.Serializable]
    public class BudgetTracker
    {
        public void UpdateProjectBudgets(Dictionary<string, ProjectBudget> budgets)
        {
            foreach (var budget in budgets.Values)
            {
                budget.SpentAmount = budget.ActualCosts.Sum(c => c.Amount);
                budget.RemainingAmount = budget.ApprovedAmount - budget.SpentAmount;
                budget.BudgetUtilization = budget.ApprovedAmount > 0 ? budget.SpentAmount / budget.ApprovedAmount : 0f;
                budget.LastUpdated = DateTime.Now;
            }
        }
    }
    
    [System.Serializable]
    public class CostAnalyzer
    {
        public CostOptimizationResult OptimizeCosts(ProjectBudget budget)
        {
            var result = new CostOptimizationResult
            {
                ProjectId = budget.ProjectId
            };
            
            // Analyze cost optimization opportunities
            result.Opportunities.AddRange(IdentifyOptimizationOpportunities(budget));
            result.TotalSavings = result.Opportunities.Sum(o => o.PotentialSavings);
            result.ImplementationCost = result.Opportunities.Sum(o => o.ImplementationCost);
            result.NetSavings = result.TotalSavings - result.ImplementationCost;
            
            return result;
        }
        
        public List<CostOptimizationOpportunity> IdentifyOptimizationOpportunities(ProjectBudget budget)
        {
            var opportunities = new List<CostOptimizationOpportunity>();
            
            // Identify high-cost categories for optimization
            foreach (var category in budget.CategoryBudgets)
            {
                if (category.Value > budget.ApprovedAmount * 0.20f) // Categories over 20% of budget
                {
                    opportunities.Add(new CostOptimizationOpportunity
                    {
                        ProjectId = budget.ProjectId,
                        Description = $"Optimize {category.Key} costs",
                        PotentialSavings = category.Value * 0.10f, // 10% potential savings
                        ImplementationCost = category.Value * 0.02f, // 2% implementation cost
                        OptimizationType = OptimizationType.Efficiency,
                        RiskLevel = 0.3f
                    });
                }
            }
            
            return opportunities;
        }
    }
    
    [System.Serializable]
    public class CannabisConstructionCostAnalyzer
    {
        public float GetCostMultiplier(ConstructionProjectData projectData)
        {
            float multiplier = 1.0f;
            
            if (projectData.RequiresComplianceRoom) multiplier += 0.15f;
            if (projectData.RequiresSecuritySystem) multiplier += 0.25f;
            if (projectData.RequiresSpecializedHVAC) multiplier += 0.20f;
            if (projectData.RequiresControlledAccess) multiplier += 0.10f;
            
            return multiplier;
        }
        
        public CannabisOptimizationResult OptimizeCannabisConstructionCosts(ProjectBudget budget)
        {
            return new CannabisOptimizationResult
            {
                ComplianceCostSavings = budget.ApprovedAmount * 0.05f,
                SecurityCostSavings = budget.ApprovedAmount * 0.03f,
                HVACOptimizationSavings = budget.ApprovedAmount * 0.08f,
                LightingOptimizationSavings = budget.ApprovedAmount * 0.06f,
                TotalCannabisSpecificSavings = budget.ApprovedAmount * 0.22f
            };
        }
    }
    
    [System.Serializable]
    public class ComplianceCostCalculator
    {
        public CostCategory CalculateComplianceCosts(ConstructionProjectData projectData)
        {
            var category = new CostCategory { CategoryName = "Compliance" };
            
            if (projectData.RequiresComplianceRoom)
            {
                category.TotalCost = projectData.CannabisSpecificSquareFootage * 50f; // $50 per sq ft
                category.CostPerUnit = 50f;
                category.Unit = "sq ft";
            }
            
            return category;
        }
    }
    
    [System.Serializable]
    public class SecurityCostOptimizer
    {
        public CostCategory CalculateSecurityCosts(ConstructionProjectData projectData)
        {
            var category = new CostCategory { CategoryName = "Security" };
            
            if (projectData.RequiresSecuritySystem)
            {
                category.TotalCost = projectData.TotalSquareFootage * 25f; // $25 per sq ft
                category.CostPerUnit = 25f;
                category.Unit = "sq ft";
            }
            
            return category;
        }
    }
}