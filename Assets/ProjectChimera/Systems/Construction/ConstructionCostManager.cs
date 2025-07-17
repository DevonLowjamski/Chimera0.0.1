using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Economy;
// Explicit type aliases to resolve ambiguous references
using ConstructionResourceType = ProjectChimera.Data.Construction.ResourceType;
using ConstructionCostType = ProjectChimera.Data.Construction.CostType;
using ConstructionBudgetAlert = ProjectChimera.Data.Construction.BudgetAlert;
using ConstructionBudgetStatus = ProjectChimera.Data.Construction.BudgetStatus;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Advanced construction cost and resource management system for Project Chimera.
    /// Handles comprehensive cost estimation, resource allocation, budget tracking,
    /// and financial optimization for all construction activities.
    /// </summary>
    public class ConstructionCostManager : ChimeraManager
    {
        [Header("Cost Management Configuration")]
        [SerializeField] private bool _enableDynamicPricing = true;
        [SerializeField] private bool _enableResourceTracking = true;
        [SerializeField] private bool _enableBudgetAlerts = true;
        [SerializeField] private bool _enableCostOptimization = true;
        [SerializeField] private float _costInflationRate = 0.02f; // 2% per year
        [SerializeField] private float _laborCostMultiplier = 1.0f;
        [SerializeField] private float _materialMarkupPercentage = 0.15f; // 15% markup
        
        [Header("Resource Management")]
        [SerializeField] private float _resourceBufferPercentage = 0.10f; // 10% buffer
        [SerializeField] private bool _enableResourceOptimization = true;
        [SerializeField] private float _wasteReductionTarget = 0.95f; // 95% efficiency
        [SerializeField] private int _maxConcurrentProjects = 5;
        [SerializeField] private float _resourceReorderThreshold = 0.20f; // 20% remaining
        
        [Header("Budget Tracking")]
        [SerializeField] private float _budgetWarningThreshold = 0.80f; // 80% budget used
        [SerializeField] private float _budgetCriticalThreshold = 0.95f; // 95% budget used
        [SerializeField] private bool _enableAutomaticBudgetAdjustment = true;
        [SerializeField] private float _contingencyPercentage = 0.10f; // 10% contingency
        
        [Header("Cannabis-Specific Costs")]
        [SerializeField] private float _complianceCostMultiplier = 1.25f;
        [SerializeField] private float _securityCostMultiplier = 1.50f;
        [SerializeField] private float _hvacCostMultiplier = 1.30f;
        [SerializeField] private float _lightingCostMultiplier = 1.40f;
        [SerializeField] private bool _enableComplianceTracking = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onBudgetAlert;
        [SerializeField] private SimpleGameEventSO _onCostEstimateCompleted;
        [SerializeField] private SimpleGameEventSO _onResourceAllocated;
        [SerializeField] private SimpleGameEventSO _onProjectBudgetExceeded;
        [SerializeField] private SimpleGameEventSO _onResourceShortage;
        
        // Core cost management
        private Dictionary<string, ProjectBudget> _projectBudgets = new Dictionary<string, ProjectBudget>();
        private Dictionary<string, ConstructionCostEstimate> _costEstimates = new Dictionary<string, ConstructionCostEstimate>();
        private Dictionary<ConstructionResourceType, ResourceInventory> _resourceInventory = new Dictionary<ConstructionResourceType, ResourceInventory>();
        private Dictionary<string, ResourceAllocation> _resourceAllocations = new Dictionary<string, ResourceAllocation>();
        
        // Cost calculation systems
        private ConstructionCostCalculator _costCalculator;
        private ResourceAllocationOptimizer _resourceOptimizer;
        private BudgetTracker _budgetTracker;
        private CostAnalyzer _costAnalyzer;
        
        // Cannabis-specific systems
        private CannabisConstructionCostAnalyzer _cannabisCostAnalyzer;
        private ComplianceCostCalculator _complianceCostCalculator;
        private SecurityCostOptimizer _securityCostOptimizer;
        
        // Performance tracking
        private ConstructionCostMetrics _costMetrics;
        private Dictionary<string, CostPerformanceData> _projectPerformance = new Dictionary<string, CostPerformanceData>();
        private List<CostOptimizationOpportunity> _optimizationOpportunities = new List<CostOptimizationOpportunity>();
        
        // Runtime tracking
        private float _totalBudgetAllocated = 0f;
        private float _totalBudgetSpent = 0f;
        private float _totalResourcesAllocated = 0f;
        private DateTime _lastCostUpdate = DateTime.Now;
        
        // Events
        public System.Action<string, ProjectBudget> OnBudgetCreated;
        public System.Action<string, ConstructionCostEstimate> OnCostEstimateCompleted;
        public System.Action<string, ResourceAllocation> OnResourceAllocated;
        public System.Action<string, ConstructionBudgetAlert> OnBudgetAlert;
        public System.Action<ConstructionResourceType, float> OnResourceShortage;
        public System.Action<string, CostOptimizationOpportunity> OnOptimizationOpportunity;
        
        // Properties
        public override ManagerPriority Priority => ManagerPriority.High;
        public float TotalBudgetAllocated => _totalBudgetAllocated;
        public float TotalBudgetSpent => _totalBudgetSpent;
        public float BudgetUtilization => _totalBudgetAllocated > 0 ? _totalBudgetSpent / _totalBudgetAllocated : 0f;
        public int ActiveProjects => _projectBudgets.Count;
        public ConstructionCostMetrics CostMetrics => _costMetrics;
        public Dictionary<string, ProjectBudget> ProjectBudgets => _projectBudgets;
        
        protected override void OnManagerInitialize()
        {
            InitializeCostSystems();
            InitializeResourceManagement();
            InitializeBudgetTracking();
            InitializeCannabisCosting();
            
            _costMetrics = new ConstructionCostMetrics();
            
            LogInfo("ConstructionCostManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            UpdateCostTracking();
            UpdateResourceManagement();
            UpdateBudgetAlerts();
            UpdateOptimizationOpportunities();
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup cost management systems
            _projectBudgets.Clear();
            _costEstimates.Clear();
            _resourceInventory.Clear();
            _resourceAllocations.Clear();
            _projectPerformance.Clear();
            _optimizationOpportunities.Clear();
            
            LogInfo("ConstructionCostManager shutdown completed");
        }
        
        /// <summary>
        /// Create a comprehensive cost estimate for a construction project
        /// </summary>
        public ConstructionCostEstimate CreateCostEstimate(string projectId, ConstructionProjectData projectData)
        {
            var estimate = new ConstructionCostEstimate
            {
                EstimateId = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                ProjectType = projectData.ProjectType,
                EstimateDate = DateTime.Now,
                EstimateValidUntil = DateTime.Now.AddDays(30),
                EstimateStatus = EstimateStatus.InProgress
            };
            
            // Calculate base construction costs
            estimate.BaseCosts = _costCalculator.CalculateBaseCosts(projectData);
            
            // Calculate labor costs
            estimate.LaborCosts = _costCalculator.CalculateLaborCosts(projectData, _laborCostMultiplier);
            
            // Calculate material costs
            estimate.MaterialCosts = _costCalculator.CalculateMaterialCosts(projectData, _materialMarkupPercentage);
            
            // Calculate equipment costs
            estimate.EquipmentCosts = _costCalculator.CalculateEquipmentCosts(projectData);
            
            // Calculate cannabis-specific costs
            if (_enableComplianceTracking)
            {
                estimate.ComplianceCosts = _complianceCostCalculator.CalculateComplianceCosts(projectData);
                estimate.SecurityCosts = _securityCostOptimizer.CalculateSecurityCosts(projectData);
            }
            
            // Calculate total costs
            estimate.SubtotalCost = estimate.BaseCosts.TotalCost + estimate.LaborCosts.TotalCost + 
                                   estimate.MaterialCosts.TotalCost + estimate.EquipmentCosts.TotalCost;
            
            estimate.ContingencyCost = estimate.SubtotalCost * _contingencyPercentage;
            estimate.TotalCost = estimate.SubtotalCost + estimate.ContingencyCost;
            
            // Apply cannabis-specific multipliers
            estimate.TotalCost *= _cannabisCostAnalyzer.GetCostMultiplier(projectData);
            
            // Store estimate
            _costEstimates[estimate.EstimateId] = estimate;
            estimate.EstimateStatus = EstimateStatus.Completed;
            
            // Trigger events
            OnCostEstimateCompleted?.Invoke(projectId, estimate);
            _onCostEstimateCompleted?.Raise();
            
            LogInfo($"Cost estimate completed for project {projectId}: ${estimate.TotalCost:F2}");
            return estimate;
        }
        
        /// <summary>
        /// Create project budget based on cost estimate
        /// </summary>
        public ProjectBudget CreateProjectBudget(string projectId, ConstructionCostEstimate costEstimate, float approvedAmount)
        {
            var budget = new ProjectBudget
            {
                BudgetId = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                ApprovedAmount = approvedAmount,
                EstimatedCost = costEstimate.TotalCost,
                RemainingAmount = approvedAmount,
                CreatedDate = DateTime.Now,
                Status = ConstructionBudgetStatus.Active,
                ContingencyReserve = approvedAmount * _contingencyPercentage
            };
            
            // Initialize budget categories
            budget.CategoryBudgets = new Dictionary<ConstructionCostType, float>
            {
                { ConstructionCostType.Labor, costEstimate.LaborCosts.TotalCost },
                { ConstructionCostType.Materials, costEstimate.MaterialCosts.TotalCost },
                { ConstructionCostType.Equipment, costEstimate.EquipmentCosts.TotalCost },
                { ConstructionCostType.Compliance, costEstimate.ComplianceCosts.TotalCost },
                { ConstructionCostType.Security, costEstimate.SecurityCosts.TotalCost }
            };
            
            // Store budget
            _projectBudgets[budget.BudgetId] = budget;
            _totalBudgetAllocated += approvedAmount;
            
            // Initialize performance tracking
            _projectPerformance[projectId] = new CostPerformanceData
            {
                ProjectId = projectId,
                BudgetId = budget.BudgetId,
                PlannedValue = costEstimate.TotalCost,
                ActualCost = 0f,
                EarnedValue = 0f,
                StartDate = DateTime.Now
            };
            
            // Trigger events
            OnBudgetCreated?.Invoke(projectId, budget);
            
            LogInfo($"Project budget created for {projectId}: ${approvedAmount:F2}");
            return budget;
        }
        
        /// <summary>
        /// Allocate resources for a construction project
        /// </summary>
        public ResourceAllocation AllocateResources(string projectId, Dictionary<ConstructionResourceType, float> resourceRequirements)
        {
            var allocation = new ResourceAllocation
            {
                AllocationId = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                AllocationDate = DateTime.Now,
                Status = AllocationStatus.Pending
            };
            
            // Check resource availability
            var availabilityCheck = CheckResourceAvailability(resourceRequirements);
            if (!availabilityCheck.IsAvailable)
            {
                allocation.Status = AllocationStatus.ResourceShortage;
                LogWarning($"Resource allocation failed for project {projectId}: Insufficient resources");
                return allocation;
            }
            
            // Allocate resources
            foreach (var requirement in resourceRequirements)
            {
                var resourceType = requirement.Key;
                var requiredAmount = requirement.Value;
                
                if (_resourceInventory.ContainsKey(resourceType))
                {
                    var inventory = _resourceInventory[resourceType];
                    var adjustedAmount = requiredAmount * (1f + _resourceBufferPercentage);
                    
                    if (inventory.AvailableAmount >= adjustedAmount)
                    {
                        inventory.AllocatedAmount += adjustedAmount;
                        inventory.AvailableAmount -= adjustedAmount;
                        
                        allocation.AllocatedResources[resourceType] = adjustedAmount;
                        allocation.AllocationCosts[resourceType] = adjustedAmount * inventory.UnitCost;
                        
                        // Check for reorder threshold
                        if (inventory.AvailableAmount <= inventory.TotalAmount * _resourceReorderThreshold)
                        {
                            TriggerResourceReorder(resourceType, inventory);
                        }
                    }
                    else
                    {
                        allocation.Status = AllocationStatus.PartialAllocation;
                        LogWarning($"Partial allocation for {resourceType}: Required {adjustedAmount}, Available {inventory.AvailableAmount}");
                    }
                }
            }
            
            // Calculate total allocation cost
            allocation.TotalAllocationCost = allocation.AllocationCosts.Values.Sum();
            
            if (allocation.Status == AllocationStatus.Pending)
            {
                allocation.Status = AllocationStatus.Allocated;
            }
            
            // Store allocation
            _resourceAllocations[allocation.AllocationId] = allocation;
            _totalResourcesAllocated += allocation.TotalAllocationCost;
            
            // Trigger events
            OnResourceAllocated?.Invoke(projectId, allocation);
            _onResourceAllocated?.Raise();
            
            LogInfo($"Resources allocated for project {projectId}: ${allocation.TotalAllocationCost:F2}");
            return allocation;
        }
        
        /// <summary>
        /// Record actual costs for a project
        /// </summary>
        public void RecordActualCost(string projectId, ConstructionCostType costType, float amount, string description = "")
        {
            // Find project budget
            var budget = _projectBudgets.Values.FirstOrDefault(b => b.ProjectId == projectId);
            if (budget == null)
            {
                LogError($"No budget found for project {projectId}");
                return;
            }
            
            // Record cost
            var costRecord = new CostRecord
            {
                RecordId = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                CostType = costType,
                Amount = amount,
                Description = description,
                RecordDate = DateTime.Now
            };
            
            budget.ActualCosts.Add(costRecord);
            budget.RemainingAmount -= amount;
            _totalBudgetSpent += amount;
            
            // Update category budget
            if (budget.CategoryBudgets.ContainsKey(costType))
            {
                budget.CategoryBudgets[costType] -= amount;
            }
            
            // Update performance tracking
            if (_projectPerformance.ContainsKey(projectId))
            {
                var performance = _projectPerformance[projectId];
                performance.ActualCost += amount;
                performance.CostPerformanceIndex = performance.EarnedValue / performance.ActualCost;
                performance.SchedulePerformanceIndex = performance.EarnedValue / performance.PlannedValue;
            }
            
            // Check budget alerts
            CheckBudgetAlerts(budget);
            
            LogInfo($"Recorded cost for project {projectId}: ${amount:F2} ({costType})");
        }
        
        /// <summary>
        /// Get project cost performance data
        /// </summary>
        public CostPerformanceData GetProjectPerformance(string projectId)
        {
            return _projectPerformance.GetValueOrDefault(projectId);
        }
        
        /// <summary>
        /// Get resource inventory status
        /// </summary>
        public ResourceInventoryStatus GetResourceInventoryStatus()
        {
            return new ResourceInventoryStatus
            {
                TotalResources = _resourceInventory.Count,
                LowStockResources = _resourceInventory.Values.Count(r => r.AvailableAmount <= r.TotalAmount * _resourceReorderThreshold),
                TotalInventoryValue = _resourceInventory.Values.Sum(r => r.TotalAmount * r.UnitCost),
                AvailableInventoryValue = _resourceInventory.Values.Sum(r => r.AvailableAmount * r.UnitCost),
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Optimize construction costs for a project
        /// </summary>
        public CostOptimizationResult OptimizeProjectCosts(string projectId)
        {
            var budget = _projectBudgets.Values.FirstOrDefault(b => b.ProjectId == projectId);
            if (budget == null)
            {
                LogError($"No budget found for project {projectId}");
                return null;
            }
            
            var optimizationResult = _costAnalyzer.OptimizeCosts(budget);
            
            // Apply cannabis-specific optimizations
            var cannabisOptimization = _cannabisCostAnalyzer.OptimizeCannabisConstructionCosts(budget);
            optimizationResult.CannabisOptimizations = cannabisOptimization;
            
            // Update optimization opportunities
            _optimizationOpportunities.AddRange(optimizationResult.Opportunities);
            
            return optimizationResult;
        }
        
        #region Private Implementation
        
        private void InitializeCostSystems()
        {
            _costCalculator = new ConstructionCostCalculator();
            _resourceOptimizer = new ResourceAllocationOptimizer();
            _budgetTracker = new BudgetTracker();
            _costAnalyzer = new CostAnalyzer();
        }
        
        private void InitializeResourceManagement()
        {
            // Initialize resource inventory
            foreach (ConstructionResourceType resourceType in Enum.GetValues(typeof(ConstructionResourceType)))
            {
                _resourceInventory[resourceType] = new ResourceInventory
                {
                    ResourceType = resourceType,
                    TotalAmount = 1000f, // Default amount
                    AvailableAmount = 1000f,
                    AllocatedAmount = 0f,
                    UnitCost = GetDefaultResourceCost(resourceType),
                    LastUpdated = DateTime.Now
                };
            }
        }
        
        private void InitializeBudgetTracking()
        {
            // Initialize budget tracking systems
        }
        
        private void InitializeCannabisCosting()
        {
            _cannabisCostAnalyzer = new CannabisConstructionCostAnalyzer();
            _complianceCostCalculator = new ComplianceCostCalculator();
            _securityCostOptimizer = new SecurityCostOptimizer();
        }
        
        private void UpdateCostTracking()
        {
            _budgetTracker.UpdateProjectBudgets(_projectBudgets);
        }
        
        private void UpdateResourceManagement()
        {
            foreach (var inventory in _resourceInventory.Values)
            {
                inventory.LastUpdated = DateTime.Now;
            }
        }
        
        private void UpdateBudgetAlerts()
        {
            foreach (var budget in _projectBudgets.Values)
            {
                CheckBudgetAlerts(budget);
            }
        }
        
        private void UpdateOptimizationOpportunities()
        {
            // Update optimization opportunities for all active projects
            foreach (var budget in _projectBudgets.Values)
            {
                if (budget.Status == ConstructionBudgetStatus.Active)
                {
                    var opportunities = _costAnalyzer.IdentifyOptimizationOpportunities(budget);
                    _optimizationOpportunities.AddRange(opportunities);
                }
            }
        }
        
        private void UpdateMetrics()
        {
            _costMetrics.TotalBudgetAllocated = _totalBudgetAllocated;
            _costMetrics.TotalBudgetSpent = _totalBudgetSpent;
            _costMetrics.BudgetUtilization = BudgetUtilization;
            _costMetrics.ActiveProjects = ActiveProjects;
            _costMetrics.AverageCostPerProject = ActiveProjects > 0 ? _totalBudgetSpent / ActiveProjects : 0f;
            _costMetrics.LastUpdated = DateTime.Now;
        }
        
        private ResourceAvailabilityCheck CheckResourceAvailability(Dictionary<ConstructionResourceType, float> requirements)
        {
            var check = new ResourceAvailabilityCheck
            {
                IsAvailable = true,
                UnavailableResources = new List<ConstructionResourceType>(),
                ShortageAmounts = new Dictionary<ConstructionResourceType, float>()
            };
            
            foreach (var requirement in requirements)
            {
                var resourceType = requirement.Key;
                var requiredAmount = requirement.Value * (1f + _resourceBufferPercentage);
                
                if (_resourceInventory.ContainsKey(resourceType))
                {
                    var inventory = _resourceInventory[resourceType];
                    if (inventory.AvailableAmount < requiredAmount)
                    {
                        check.IsAvailable = false;
                        check.UnavailableResources.Add(resourceType);
                        check.ShortageAmounts[resourceType] = requiredAmount - inventory.AvailableAmount;
                    }
                }
                else
                {
                    check.IsAvailable = false;
                    check.UnavailableResources.Add(resourceType);
                    check.ShortageAmounts[resourceType] = requiredAmount;
                }
            }
            
            return check;
        }
        
        private void TriggerResourceReorder(ConstructionResourceType resourceType, ResourceInventory inventory)
        {
            OnResourceShortage?.Invoke(resourceType, inventory.AvailableAmount);
            _onResourceShortage?.Raise();
            
            LogWarning($"Resource {resourceType} below reorder threshold: {inventory.AvailableAmount} remaining");
        }
        
        private void CheckBudgetAlerts(ProjectBudget budget)
        {
            float budgetUtilization = (budget.ApprovedAmount - budget.RemainingAmount) / budget.ApprovedAmount;
            
            if (budgetUtilization >= _budgetCriticalThreshold)
            {
                var alert = new ConstructionBudgetAlert
                {
                    AlertId = Guid.NewGuid().ToString(),
                    ProjectId = budget.ProjectId,
                    BudgetId = budget.BudgetId,
                    AlertType = BudgetAlertType.Critical,
                    Message = $"Budget critically low: {budgetUtilization:P0} used",
                    Threshold = _budgetCriticalThreshold,
                    CurrentUtilization = budgetUtilization,
                    CreatedDate = DateTime.Now
                };
                
                OnBudgetAlert?.Invoke(budget.ProjectId, alert);
                _onBudgetAlert?.Raise();
            }
            else if (budgetUtilization >= _budgetWarningThreshold)
            {
                var alert = new ConstructionBudgetAlert
                {
                    AlertId = Guid.NewGuid().ToString(),
                    ProjectId = budget.ProjectId,
                    BudgetId = budget.BudgetId,
                    AlertType = BudgetAlertType.Warning,
                    Message = $"Budget warning: {budgetUtilization:P0} used",
                    Threshold = _budgetWarningThreshold,
                    CurrentUtilization = budgetUtilization,
                    CreatedDate = DateTime.Now
                };
                
                OnBudgetAlert?.Invoke(budget.ProjectId, alert);
                _onBudgetAlert?.Raise();
            }
        }
        
        private float GetDefaultResourceCost(ConstructionResourceType resourceType)
        {
            return resourceType switch
            {
                ConstructionResourceType.Steel => 2.50f,
                ConstructionResourceType.Concrete => 0.75f,
                ConstructionResourceType.Lumber => 1.25f,
                ConstructionResourceType.Electrical => 3.00f,
                ConstructionResourceType.Plumbing => 2.75f,
                ConstructionResourceType.HVAC => 4.50f,
                ConstructionResourceType.Insulation => 1.50f,
                ConstructionResourceType.Drywall => 0.85f,
                ConstructionResourceType.Flooring => 3.25f,
                ConstructionResourceType.Lighting => 5.00f,
                ConstructionResourceType.Security => 8.00f,
                ConstructionResourceType.Automation => 12.00f,
                _ => 1.00f
            };
        }
        
        #endregion
    }
}