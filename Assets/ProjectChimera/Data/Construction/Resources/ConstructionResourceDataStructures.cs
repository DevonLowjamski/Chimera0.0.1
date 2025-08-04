/*
 * ConstructionResourceDataStructures.cs
 * 
 * Construction Resource and Material Management Data Structures
 * Extracted from ConstructionDataStructures.cs to provide focused resource management functionality
 * 
 * This file contains all data structures related to:
 * - Materials and material management
 * - Workforce and worker management  
 * - Equipment and tool management
 * - Cost tracking and financial planning
 * - Issue tracking and resolution
 * - Resource allocation and optimization
 * 
 * Part of Project Chimera - Cannabis Cultivation Facility Management System
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Core.Interfaces;

namespace ProjectChimera.Data.Construction.Resources
{
    #region Resource and Material Enums
    
    [System.Serializable]
    public enum BuildingMaterial
    {
        Wood,
        Steel,
        Concrete,
        Glass,
        Aluminum,
        Insulation,
        Electrical,
        Plumbing,
        HVAC,
        Specialized
    }

    [System.Serializable]
    public enum WorkerSpecialty
    {
        GeneralConstruction,
        GeneralLabor,
        Electrician,
        Electrical,
        Plumber,
        Plumbing,
        HVAC_Technician,
        HVAC,
        Carpenter,
        Mason,
        Roofer,
        Painter,
        Flooring_Specialist,
        Equipment_Installer,
        Inspector,
        Project_Manager
    }

    [System.Serializable]
    public enum WorkerStatus
    {
        Assigned,
        Active,
        On_Break,
        Completed,
        Reassigned
    }

    [System.Serializable]
    public enum SkillLevel
    {
        Beginner = 0,
        Novice = 1,
        Apprentice = 2,
        Intermediate = 3,
        Skilled = 4,
        Advanced = 5,
        Expert = 6,
        Master = 7
    }

    [System.Serializable]
    public enum MaterialStatus
    {
        Planning,
        Ordered,
        In_Transit,
        Delivered,
        Installed,
        Rejected
    }

    [System.Serializable]
    public enum IssueType
    {
        ValidationFailed,
        InvalidPlacement,
        MaterialShortage,
        WorkerUnavailable,
        WeatherDelay,
        PermitIssue,
        QualityFailure,
        SafetyViolation,
        DesignChange,
        BudgetOverrun
    }

    [System.Serializable]
    public enum IssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    [System.Serializable]
    public enum IssueCategory
    {
        Safety,
        Quality,
        Schedule,
        Budget,
        Materials,
        Labor,
        Permits,
        Weather,
        Design_Change,
        Equipment,
        Other
    }

    [System.Serializable]
    public enum IssueStatus
    {
        Open,
        In_Progress,
        Resolved,
        Closed,
        Escalated
    }

    #endregion

    #region Material Management Classes

    /// <summary>
    /// Defines material requirements for construction tasks
    /// </summary>
    [System.Serializable]
    public class MaterialRequirement
    {
        [Header("Material Information")]
        public string MaterialId;
        public string MaterialName;
        public BuildingMaterial Type;
        
        [Header("Quantity and Units")]
        public float RequiredQuantity;
        public float AvailableQuantity;
        public float OrderedQuantity;
        public string Unit;
        
        [Header("Cost and Supplier")]
        public float UnitCost;
        public string Supplier;
        
        [Header("Timeline")]
        public DateTime RequiredDate;
        public DateTime OrderDate;
        public DateTime ExpectedDelivery;
        
        [Header("Status")]
        public MaterialStatus Status;
        
        public MaterialRequirement()
        {
            MaterialId = System.Guid.NewGuid().ToString();
            RequiredDate = DateTime.Now.AddDays(7);
            OrderDate = DateTime.Now;
            ExpectedDelivery = DateTime.Now.AddDays(14);
            Status = MaterialStatus.Planning;
        }
        
        /// <summary>
        /// Calculate total cost for this material requirement
        /// </summary>
        public float GetTotalCost()
        {
            return RequiredQuantity * UnitCost;
        }
        
        /// <summary>
        /// Check if material requirement is fulfilled
        /// </summary>
        public bool IsFulfilled()
        {
            return AvailableQuantity >= RequiredQuantity;
        }
    }

    /// <summary>
    /// Manages inventory of construction materials
    /// </summary>
    [System.Serializable]
    public class MaterialInventory
    {
        [Header("Inventory Data")]
        public List<MaterialStock> Materials;
        public Dictionary<string, MaterialStock> MaterialLookup;
        
        [Header("Inventory Metrics")]
        public float TotalInventoryValue;
        public DateTime LastUpdated;

        public MaterialInventory()
        {
            Materials = new List<MaterialStock>();
            MaterialLookup = new Dictionary<string, MaterialStock>();
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Check if specific material is available in required quantity
        /// </summary>
        public bool HasMaterial(string materialType, float quantity)
        {
            return MaterialLookup.ContainsKey(materialType) && 
                   MaterialLookup[materialType].AvailableQuantity >= quantity;
        }
        
        /// <summary>
        /// Check if all required materials are available
        /// </summary>
        public bool HasMaterials(List<string> requiredMaterials)
        {
            foreach (var mat in requiredMaterials)
            {
                if (!MaterialLookup.ContainsKey(mat)) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Check if all material requirements can be fulfilled
        /// </summary>
        public bool HasMaterials(List<MaterialRequirement> requiredMaterials)
        {
            foreach (var req in requiredMaterials)
            {
                if (!HasMaterial(req.MaterialName, req.RequiredQuantity)) return false;
            }
            return true;
        }

        /// <summary>
        /// Add material to inventory
        /// </summary>
        public void AddMaterial(string materialType, float quantity, float costPerUnit)
        {
            if (MaterialLookup.ContainsKey(materialType))
            {
                MaterialLookup[materialType].AvailableQuantity += quantity;
                MaterialLookup[materialType].LastUpdated = DateTime.Now;
            }
            else
            {
                var newStock = new MaterialStock 
                { 
                    MaterialType = materialType, 
                    AvailableQuantity = quantity, 
                    CostPerUnit = costPerUnit,
                    LastUpdated = DateTime.Now
                };
                Materials.Add(newStock);
                MaterialLookup[materialType] = newStock;
            }
            UpdateInventoryValue();
        }

        /// <summary>
        /// Consume material from inventory
        /// </summary>
        public bool ConsumeMaterial(string materialType, float quantity)
        {
            if (HasMaterial(materialType, quantity))
            {
                MaterialLookup[materialType].AvailableQuantity -= quantity;
                MaterialLookup[materialType].LastUpdated = DateTime.Now;
                UpdateInventoryValue();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get material stock data
        /// </summary>
        public MaterialStock GetMaterialData(string materialType)
        {
            return MaterialLookup.ContainsKey(materialType) ? MaterialLookup[materialType] : null;
        }
        
        /// <summary>
        /// Update total inventory value
        /// </summary>
        private void UpdateInventoryValue()
        {
            TotalInventoryValue = Materials.Sum(m => m.AvailableQuantity * m.CostPerUnit);
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Get low stock materials
        /// </summary>
        public List<MaterialStock> GetLowStockMaterials(float threshold = 10f)
        {
            return Materials.Where(m => m.AvailableQuantity <= threshold).ToList();
        }
    }

    /// <summary>
    /// Represents stock of a specific material type
    /// </summary>
    [System.Serializable]
    public class MaterialStock
    {
        [Header("Material Information")]
        public string MaterialType;
        public BuildingMaterial MaterialCategory;
        
        [Header("Quantity and Cost")]
        public float AvailableQuantity;
        public float ReservedQuantity;
        public float CostPerUnit;
        public float TotalValue => AvailableQuantity * CostPerUnit;
        
        [Header("Supply Chain")]
        public DateTime LastUpdated;
        public string Supplier;
        public DateTime ExpirationDate;
        public string StorageLocation;
        
        [Header("Quality")]
        public float QualityRating; // 0-5 scale
        public string QualityNotes;
        public bool RequiresInspection;
        
        public MaterialStock()
        {
            LastUpdated = DateTime.Now;
            QualityRating = 5f;
            RequiresInspection = false;
        }
        
        /// <summary>
        /// Check if material is expired
        /// </summary>
        public bool IsExpired()
        {
            return ExpirationDate != default(DateTime) && DateTime.Now > ExpirationDate;
        }
        
        /// <summary>
        /// Get available quantity after reservations
        /// </summary>
        public float GetAvailableForUse()
        {
            return Mathf.Max(0, AvailableQuantity - ReservedQuantity);
        }
    }

    #endregion

    #region Workforce Management Classes

    /// <summary>
    /// Represents a construction worker with skills and performance metrics
    /// </summary>
    [System.Serializable]
    public class ConstructionWorker
    {
        [Header("Worker Information")]
        public string WorkerId;
        public string Name;
        public WorkerSpecialty Specialty;
        public SkillLevel SkillLevel;

        [Header("Employment")]
        public float HourlyRate;
        public bool IsAvailable;
        public bool IsContractor; // vs employee
        public DateTime HireDate;

        [Header("Performance")]
        public float EfficiencyMultiplier; // 1.0 is normal
        public float ProductivityModifier;
        public float QualityRating; // 0-5
        public int ProjectsCompleted;
        public float ExperienceYears;

        [Header("Current Assignment")]
        public string CurrentProjectId;
        public string CurrentTaskId;
        public DateTime AssignmentStartDate;
        public float HoursWorkedToday;
        public float HoursWorkedThisWeek;

        [Header("Certifications")]
        public List<string> Certifications;
        public List<string> Licenses;
        public DateTime LastSafetyTraining;
        
        [Header("Health and Safety")]
        public int SafetyViolations;
        public DateTime LastIncident;
        public bool SafetyCertified;
        
        public ConstructionWorker()
        {
            WorkerId = System.Guid.NewGuid().ToString();
            Certifications = new List<string>();
            Licenses = new List<string>();
            HireDate = DateTime.Now;
            IsAvailable = true;
            EfficiencyMultiplier = 1.0f;
            ProductivityModifier = 1.0f;
            QualityRating = 3.0f;
            SafetyCertified = true;
        }
        
        /// <summary>
        /// Calculate worker's overall performance score
        /// </summary>
        public float GetPerformanceScore()
        {
            float efficiencyScore = Mathf.Clamp01(EfficiencyMultiplier);
            float productivityScore = Mathf.Clamp01(ProductivityModifier);
            float qualityScore = QualityRating / 5f;
            float safetyScore = SafetyViolations > 0 ? Mathf.Max(0, 1f - (SafetyViolations * 0.1f)) : 1f;
            
            return (efficiencyScore + productivityScore + qualityScore + safetyScore) / 4f;
        }
        
        /// <summary>
        /// Check if worker needs safety training
        /// </summary>
        public bool NeedsSafetyTraining()
        {
            return DateTime.Now.Subtract(LastSafetyTraining).TotalDays > 365;
        }
        
        /// <summary>
        /// Calculate daily labor cost for this worker
        /// </summary>
        public float GetDailyLaborCost(float hoursPerDay = 8f)
        {
            return HourlyRate * hoursPerDay;
        }
    }

    /// <summary>
    /// Manages the construction workforce
    /// </summary>
    [System.Serializable]
    public class ConstructionWorkforce
    {
        [Header("Workforce Data")]
        public List<ConstructionWorker> Workers;
        public Dictionary<string, ConstructionWorker> WorkerLookup;
        
        [Header("Workforce Metrics")]
        public int TotalWorkers => Workers.Count;
        public int AvailableWorkers => Workers.Count(w => w.IsAvailable);
        public int ActiveWorkers => Workers.Count(w => !w.IsAvailable);
        public float AverageSkillLevel => Workers.Average(w => (float)w.SkillLevel);
        public float AveragePerformanceScore => Workers.Average(w => w.GetPerformanceScore());

        public ConstructionWorkforce()
        {
            Workers = new List<ConstructionWorker>();
            WorkerLookup = new Dictionary<string, ConstructionWorker>();
        }

        /// <summary>
        /// Add worker to workforce
        /// </summary>
        public void AddWorker(ConstructionWorker worker)
        {
            if (!WorkerLookup.ContainsKey(worker.WorkerId))
            {
                Workers.Add(worker);
                WorkerLookup[worker.WorkerId] = worker;
            }
        }

        /// <summary>
        /// Get available workers by specialty
        /// </summary>
        public List<ConstructionWorker> GetAvailableWorkers(WorkerSpecialty specialty = WorkerSpecialty.GeneralConstruction)
        {
            return Workers.Where(w => w.IsAvailable && w.Specialty == specialty).ToList();
        }

        /// <summary>
        /// Get all active workers
        /// </summary>
        public List<ConstructionWorker> GetActiveWorkers()
        {
            return Workers.Where(w => !w.IsAvailable).ToList();
        }
        
        /// <summary>
        /// Get worker by ID
        /// </summary>
        public ConstructionWorker GetWorker(string workerId)
        {
            return WorkerLookup.ContainsKey(workerId) ? WorkerLookup[workerId] : null;
        }

        /// <summary>
        /// Assign workers to a construction task
        /// </summary>
        public List<ConstructionWorker> AssignWorkers(string projectId, string taskId, WorkerSpecialty specialty, int workerCount)
        {
            var availableWorkers = GetAvailableWorkers(specialty)
                .OrderByDescending(w => w.GetPerformanceScore())
                .Take(workerCount)
                .ToList();
                
            foreach (var worker in availableWorkers)
            {
                worker.IsAvailable = false;
                worker.CurrentProjectId = projectId;
                worker.CurrentTaskId = taskId;
                worker.AssignmentStartDate = DateTime.Now;
            }
            return availableWorkers;
        }

        /// <summary>
        /// Release workers from assignment
        /// </summary>
        public void ReleaseWorkers(List<ConstructionWorker> workers)
        {
            foreach (var worker in workers)
            {
                var w = GetWorker(worker.WorkerId);
                if (w != null)
                {
                    w.IsAvailable = true;
                    w.CurrentProjectId = null;
                    w.CurrentTaskId = null;
                    w.ProjectsCompleted++;
                }
            }
        }
        
        /// <summary>
        /// Get workers requiring safety training
        /// </summary>
        public List<ConstructionWorker> GetWorkersNeedingTraining()
        {
            return Workers.Where(w => w.NeedsSafetyTraining()).ToList();
        }
        
        /// <summary>
        /// Calculate total daily labor cost
        /// </summary>
        public float GetDailyLaborCost()
        {
            return Workers.Sum(w => w.GetDailyLaborCost());
        }
    }

    /// <summary>
    /// Represents a worker assignment to a specific task
    /// </summary>
    [System.Serializable]
    public class WorkerAssignment
    {
        [Header("Assignment Information")]
        public string AssignmentId;
        public string WorkerId;
        public string WorkerName;
        public string ProjectId;
        public string TaskId;
        
        [Header("Worker Details")]
        public WorkerSpecialty Specialty;
        public SkillLevel SkillLevel;
        
        [Header("Assignment Timeline")]
        public DateTime StartDate;
        public DateTime EndDate;
        public DateTime ActualStartDate;
        public DateTime ActualEndDate;
        
        [Header("Work Details")]
        public float HoursPerDay;
        public float HourlyRate;
        public string TaskDescription;
        public float CompletionPercentage;
        public WorkerStatus Status;
        
        [Header("Performance")]
        public float HoursWorked;
        public float OverTimeHours;
        public float ProductivityRating;
        public List<string> Notes;
        
        public WorkerAssignment()
        {
            AssignmentId = System.Guid.NewGuid().ToString();
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(1);
            Status = WorkerStatus.Assigned;
            HoursPerDay = 8f;
            CompletionPercentage = 0f;
            Notes = new List<string>();
        }
        
        /// <summary>
        /// Calculate total labor cost for this assignment
        /// </summary>
        public float GetTotalLaborCost()
        {
            return (HoursWorked + (OverTimeHours * 1.5f)) * HourlyRate;
        }
        
        /// <summary>
        /// Check if assignment is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return DateTime.Now > EndDate && CompletionPercentage < 1f;
        }
    }

    #endregion

    #region Issue Management Classes

    /// <summary>
    /// Represents a construction issue or problem
    /// </summary>
    [System.Serializable]
    public class ConstructionIssue
    {
        [Header("Issue Information")]
        public string IssueId;
        public string Title;
        public string Description;
        public IssueType IssueType;
        public IssueSeverity Severity;
        public IssueCategory Category;
        
        [Header("Issue Timeline")]
        public DateTime ReportedDate;
        public DateTime ResolvedDate;
        public DateTime TargetResolutionDate;
        
        [Header("Issue Management")]
        public string ReportedBy;
        public string AssignedTo;
        public string ResolvedBy;
        public IssueStatus Status;
        public string ResolutionDescription;
        
        [Header("Impact Assessment")]
        public float CostImpact;
        public int DelayDays;
        public float QualityImpact;
        public float SafetyRisk; // 0-1 scale
        
        [Header("Related Items")]
        public string ProjectId;
        public string TaskId;
        public List<string> AffectedWorkers;
        public List<string> AffectedMaterials;
        
        public ConstructionIssue()
        {
            IssueId = System.Guid.NewGuid().ToString();
            ReportedDate = DateTime.Now;
            Status = IssueStatus.Open;
            TargetResolutionDate = DateTime.Now.AddDays(7);
            AffectedWorkers = new List<string>();
            AffectedMaterials = new List<string>();
        }
        
        /// <summary>
        /// Calculate total impact score
        /// </summary>
        public float GetImpactScore()
        {
            float costWeight = CostImpact > 1000 ? 0.4f : CostImpact / 2500f * 0.4f;
            float delayWeight = DelayDays > 0 ? Mathf.Min(DelayDays / 30f, 1f) * 0.3f : 0f;
            float qualityWeight = QualityImpact * 0.2f;
            float safetyWeight = SafetyRisk * 0.1f;
            
            return costWeight + delayWeight + qualityWeight + safetyWeight;
        }
        
        /// <summary>
        /// Check if issue is critical
        /// </summary>
        public bool IsCritical()
        {
            return Severity == IssueSeverity.Critical || SafetyRisk > 0.7f || GetImpactScore() > 0.8f;
        }
        
        /// <summary>
        /// Check if issue is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return DateTime.Now > TargetResolutionDate && Status != IssueStatus.Resolved && Status != IssueStatus.Closed;
        }
    }

    #endregion

    #region Equipment Management Classes

    /// <summary>
    /// Manages pool of available construction equipment
    /// </summary>
    [System.Serializable]
    public class EquipmentPool
    {
        [Header("Equipment Inventory")]
        public Dictionary<string, int> AvailableEquipment;
        public Dictionary<string, int> ReservedEquipment;
        public Dictionary<string, float> EquipmentCosts;
        public Dictionary<string, DateTime> LastMaintenance;
        
        [Header("Equipment Metrics")]
        public float TotalEquipmentValue;
        public int TotalEquipmentCount;
        public DateTime LastUpdated;
        
        public EquipmentPool()
        {
            AvailableEquipment = new Dictionary<string, int>();
            ReservedEquipment = new Dictionary<string, int>();
            EquipmentCosts = new Dictionary<string, float>();
            LastMaintenance = new Dictionary<string, DateTime>();
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Check if equipment is available
        /// </summary>
        public bool HasEquipment(string equipmentType, int quantity = 1)
        {
            return AvailableEquipment.ContainsKey(equipmentType) && 
                   AvailableEquipment[equipmentType] >= quantity;
        }
        
        /// <summary>
        /// Reserve equipment for use
        /// </summary>
        public bool ReserveEquipment(string equipmentType, int quantity = 1)
        {
            if (HasEquipment(equipmentType, quantity))
            {
                AvailableEquipment[equipmentType] -= quantity;
                
                if (!ReservedEquipment.ContainsKey(equipmentType))
                    ReservedEquipment[equipmentType] = 0;
                ReservedEquipment[equipmentType] += quantity;
                
                LastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Release reserved equipment
        /// </summary>
        public bool ReleaseEquipment(string equipmentType, int quantity = 1)
        {
            if (ReservedEquipment.ContainsKey(equipmentType) && 
                ReservedEquipment[equipmentType] >= quantity)
            {
                ReservedEquipment[equipmentType] -= quantity;
                AvailableEquipment[equipmentType] += quantity;
                LastUpdated = DateTime.Now;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Add equipment to pool
        /// </summary>
        public void AddEquipment(string equipmentType, int quantity, float costPerUnit)
        {
            if (!AvailableEquipment.ContainsKey(equipmentType))
                AvailableEquipment[equipmentType] = 0;
                
            AvailableEquipment[equipmentType] += quantity;
            EquipmentCosts[equipmentType] = costPerUnit;
            LastMaintenance[equipmentType] = DateTime.Now;
            
            UpdateMetrics();
        }
        
        /// <summary>
        /// Get equipment requiring maintenance
        /// </summary>
        public List<string> GetEquipmentNeedingMaintenance(int maintenanceIntervalDays = 90)
        {
            var needsMaintenance = new List<string>();
            foreach (var equipment in LastMaintenance)
            {
                if (DateTime.Now.Subtract(equipment.Value).TotalDays > maintenanceIntervalDays)
                {
                    needsMaintenance.Add(equipment.Key);
                }
            }
            return needsMaintenance;
        }
        
        /// <summary>
        /// Update equipment metrics
        /// </summary>
        private void UpdateMetrics()
        {
            TotalEquipmentCount = AvailableEquipment.Values.Sum() + ReservedEquipment.Values.Sum();
            TotalEquipmentValue = AvailableEquipment.Sum(e => e.Value * (EquipmentCosts.ContainsKey(e.Key) ? EquipmentCosts[e.Key] : 0));
            LastUpdated = DateTime.Now;
        }
    }

    #endregion

    #region Contractor Management Classes

    /// <summary>
    /// Manages external contractors
    /// </summary>
    [System.Serializable]
    public class ContractorManager
    {
        [Header("Contractor Database")]
        public Dictionary<string, string> AvailableContractors;
        public Dictionary<string, float> ContractorRates;
        public Dictionary<string, float> ContractorRatings;
        public Dictionary<string, List<WorkerSpecialty>> ContractorSpecialties;
        
        [Header("Performance Tracking")]
        public Dictionary<string, int> ContractorProjects;
        public Dictionary<string, DateTime> LastContractDate;
        
        public ContractorManager()
        {
            AvailableContractors = new Dictionary<string, string>();
            ContractorRates = new Dictionary<string, float>();
            ContractorRatings = new Dictionary<string, float>();
            ContractorSpecialties = new Dictionary<string, List<WorkerSpecialty>>();
            ContractorProjects = new Dictionary<string, int>();
            LastContractDate = new Dictionary<string, DateTime>();
        }
        
        /// <summary>
        /// Find contractor by specialty
        /// </summary>
        public string FindContractor(WorkerSpecialty specialty)
        {
            var suitableContractors = ContractorSpecialties
                .Where(c => c.Value.Contains(specialty))
                .OrderByDescending(c => ContractorRatings.ContainsKey(c.Key) ? ContractorRatings[c.Key] : 0f)
                .Select(c => c.Key);
                
            return suitableContractors.FirstOrDefault();
        }
        
        /// <summary>
        /// Get best contractors by rating
        /// </summary>
        public List<string> GetTopContractors(int count = 5)
        {
            return ContractorRatings
                .OrderByDescending(c => c.Value)
                .Take(count)
                .Select(c => c.Key)
                .ToList();
        }
        
        /// <summary>
        /// Add contractor to database
        /// </summary>
        public void AddContractor(string contractorId, string name, float rate, List<WorkerSpecialty> specialties)
        {
            AvailableContractors[contractorId] = name;
            ContractorRates[contractorId] = rate;
            ContractorSpecialties[contractorId] = specialties;
            ContractorRatings[contractorId] = 3.0f; // Default rating
            ContractorProjects[contractorId] = 0;
        }
        
        /// <summary>
        /// Update contractor rating
        /// </summary>
        public void UpdateContractorRating(string contractorId, float newRating)
        {
            if (ContractorRatings.ContainsKey(contractorId))
            {
                ContractorRatings[contractorId] = Mathf.Clamp(newRating, 0f, 5f);
                LastContractDate[contractorId] = DateTime.Now;
            }
        }
    }

    #endregion

    #region Cost Management Classes

    /// <summary>
    /// Tracks cost updates for construction projects
    /// </summary>
    [System.Serializable]
    public class ConstructionCostUpdate
    {
        [Header("Cost Information")]
        public string UpdateId;
        public string ProjectId;
        public DateTime UpdateDate;
        public string UpdatedBy;
        
        [Header("Cost Details")]
        public float PreviousCost;
        public float NewCost;
        public float EstimatedCost;
        public float ActualCostToDate;
        public float CostDifference;
        public float CostVariance;
        
        [Header("Cost Breakdown")]
        public float MaterialCosts;
        public float LaborCosts;
        public float EquipmentCosts;
        public float PermitCosts;
        public float ContingencyCosts;
        
        [Header("Analysis")]
        public string Reason;
        public string CostCategory;
        public bool RequiresApproval;
        
        public ConstructionCostUpdate()
        {
            UpdateId = System.Guid.NewGuid().ToString();
            UpdateDate = DateTime.Now;
            RequiresApproval = false;
        }
        
        /// <summary>
        /// Calculate cost variance percentage
        /// </summary>
        public float GetCostVariancePercentage()
        {
            if (EstimatedCost > 0)
                return (ActualCostToDate - EstimatedCost) / EstimatedCost * 100f;
            return 0f;
        }
        
        /// <summary>
        /// Check if cost increase is significant
        /// </summary>
        public bool IsSignificantIncrease(float threshold = 0.1f)
        {
            return GetCostVariancePercentage() > threshold * 100f;
        }
    }

    #endregion

    #region Configuration and Settings Classes

    /// <summary>
    /// Configuration settings for construction systems
    /// </summary>
    [System.Serializable]
    public class ConstructionSettings
    {
        [Header("Construction Configuration")]
        public bool EnableConstruction;
        public bool EnforceZoningLaws;
        public bool RequirePermits;
        public float ConstructionSpeedMultiplier;

        [Header("Design Tools")]
        public bool UseDesignTool;
        public bool ShowConstructionGuides;
        public bool ValidateRealTime;

        [Header("Resource Management")]
        public bool AutoOrderMaterials;
        public float MaterialSafetyStock; // Percentage
        public bool EnableEquipmentTracking;
        public int MaxWorkersPerTask;

        [Header("Economic Settings")]
        public float DemolitionCostPerSqFt;
        public float LaborCostPerHour;
        public float MaterialMarkup;
        public float ContractorMarkup;

        [Header("Quality Control")]
        public bool EnableQualitySystem;
        public float QualityThreshold;
        public bool AutoFailOnCriticalIssues;
        public bool RequireInspections;

        [Header("Safety and Compliance")]
        public bool EnforceSafetyProtocols;
        public bool RequireSafetyTraining;
        public int SafetyTrainingIntervalDays;
        public bool EnableIncidentTracking;
        
        public ConstructionSettings()
        {
            EnableConstruction = true;
            EnforceZoningLaws = true;
            RequirePermits = true;
            ConstructionSpeedMultiplier = 1.0f;
            UseDesignTool = true;
            ShowConstructionGuides = true;
            ValidateRealTime = true;
            AutoOrderMaterials = false;
            MaterialSafetyStock = 0.1f;
            EnableEquipmentTracking = true;
            MaxWorkersPerTask = 10;
            DemolitionCostPerSqFt = 5.0f;
            LaborCostPerHour = 25.0f;
            MaterialMarkup = 0.15f;
            ContractorMarkup = 0.20f;
            EnableQualitySystem = true;
            QualityThreshold = 0.8f;
            AutoFailOnCriticalIssues = true;
            RequireInspections = true;
            EnforceSafetyProtocols = true;
            RequireSafetyTraining = true;
            SafetyTrainingIntervalDays = 365;
            EnableIncidentTracking = true;
        }
    }

    #endregion

    #region Metrics and Reporting Classes

    /// <summary>
    /// Comprehensive construction metrics tracking
    /// </summary>
    [System.Serializable]
    public class ConstructionMetrics
    {
        [Header("Overall Metrics")]
        public int TotalProjects;
        public int ActiveProjects;
        public int CompletedProjects;
        public float TotalValue;
        public int ActiveWorkers;
        public float ConstructionEfficiency;
        public DateTime LastUpdated;

        [Header("Financial Metrics")]
        public float TotalSpent;
        public float TotalBudget;
        public float AverageCostPerSqFt;
        public float AverageCostOverrun;
        public float MaterialCosts;
        public float LaborCosts;
        public float EquipmentCosts;

        [Header("Quality Metrics")]
        public float AverageQualityScore;
        public int TotalDefects;
        public int TotalRework;
        public float CustomerSatisfactionRating;
        public int QualityInspectionsPassed;
        public int QualityInspectionsFailed;

        [Header("Efficiency Metrics")]
        public float AverageProjectDuration;
        public float AverageCompletionTime;
        public float MaterialWastePercentage;
        public float ScheduleAdherence;
        public float WorkerProductivity;
        
        [Header("Resource Utilization")]
        public float WorkerUtilization;
        public float EquipmentUtilization;
        public float MaterialUtilization;
        
        [Header("Safety Metrics")]
        public int SafetyIncidents;
        public int SafetyViolations;
        public float SafetyScore;
        public DateTime LastSafetyIncident;

        public ConstructionMetrics()
        {
            LastUpdated = DateTime.Now;
            CustomerSatisfactionRating = 4.0f;
            SafetyScore = 95f;
        }

        /// <summary>
        /// Get list of active workers (placeholder for compatibility)
        /// </summary>
        public List<ConstructionWorker> GetActiveWorkers()
        {
            return new List<ConstructionWorker>();
        }
        
        /// <summary>
        /// Calculate overall performance score
        /// </summary>
        public float GetOverallPerformanceScore()
        {
            float qualityScore = AverageQualityScore / 5f * 0.25f;
            float efficiencyScore = ConstructionEfficiency * 0.25f;
            float safetyScore = SafetyScore / 100f * 0.25f;
            float scheduleScore = ScheduleAdherence * 0.25f;
            
            return qualityScore + efficiencyScore + safetyScore + scheduleScore;
        }
        
        /// <summary>
        /// Update metrics with new data
        /// </summary>
        public void UpdateMetrics()
        {
            LastUpdated = DateTime.Now;
            
            // Calculate derived metrics
            WorkerUtilization = ActiveWorkers > 0 ? (float)ActiveWorkers / TotalProjects : 0f;
            
            if (TotalProjects > 0)
            {
                AverageCostOverrun = (TotalSpent - TotalBudget) / TotalBudget;
                ScheduleAdherence = CompletedProjects > 0 ? 
                    (float)CompletedProjects / TotalProjects : 0f;
            }
        }
    }

    /// <summary>
    /// Event logging for construction activities
    /// </summary>
    [System.Serializable]
    public class ConstructionEvent
    {
        [Header("Event Information")]
        public string EventId;
        public string ProjectId;
        public string TaskId;
        public DateTime EventDate;
        public string EventType;
        
        [Header("Event Details")]
        public string Description;
        public string Details;
        public IssueSeverity Severity;
        public string ReportedBy;
        
        [Header("Impact")]
        public float FinancialImpact;
        public float ActualCost;
        public int TimeImpactDays;
        public float QualityImpact;
        
        [Header("Related Resources")]
        public List<string> AffectedWorkers;
        public List<string> AffectedMaterials;
        public List<string> AffectedEquipment;
        
        public ConstructionEvent()
        {
            EventId = System.Guid.NewGuid().ToString();
            EventDate = DateTime.Now;
            AffectedWorkers = new List<string>();
            AffectedMaterials = new List<string>();
            AffectedEquipment = new List<string>();
        }
        
        /// <summary>
        /// Calculate total impact score for the event
        /// </summary>
        public float GetTotalImpactScore()
        {
            float financialWeight = FinancialImpact > 0 ? Mathf.Min(FinancialImpact / 10000f, 1f) * 0.4f : 0f;
            float timeWeight = TimeImpactDays > 0 ? Mathf.Min(TimeImpactDays / 30f, 1f) * 0.3f : 0f;
            float qualityWeight = QualityImpact * 0.3f;
            
            return financialWeight + timeWeight + qualityWeight;
        }
    }

    #endregion

    #region Optimization Classes

    /// <summary>
    /// Design solutions for construction optimization
    /// </summary>
    [System.Serializable]
    public class ConstructionDesignSolution
    {
        [Header("Solution Information")]
        public string SolutionId;
        public string Name;
        public string Description;
        public string SolutionType;
        
        [Header("Solution Features")]
        public List<string> Features;
        public List<string> Benefits;
        public List<string> Requirements;
        
        [Header("Performance Metrics")]
        public float Cost;
        public float Efficiency;
        public float QualityScore;
        public float ImplementationTime;
        public float ROI;
        
        [Header("Metadata")]
        public DateTime CreatedDate;
        public string CreatedBy;
        public float PopularityScore;
        public int TimesImplemented;
        
        public ConstructionDesignSolution()
        {
            SolutionId = System.Guid.NewGuid().ToString();
            Features = new List<string>();
            Benefits = new List<string>();
            Requirements = new List<string>();
            CreatedDate = DateTime.Now;
            PopularityScore = 0f;
            TimesImplemented = 0;
        }
        
        /// <summary>
        /// Calculate overall solution value score
        /// </summary>
        public float GetValueScore()
        {
            float costEfficiency = Cost > 0 ? Mathf.Clamp01(10000f / Cost) : 1f;
            float qualityWeight = QualityScore / 5f;
            float efficiencyWeight = Efficiency;
            float roiWeight = ROI > 0 ? Mathf.Clamp01(ROI / 2f) : 0f;
            
            return (costEfficiency + qualityWeight + efficiencyWeight + roiWeight) / 4f;
        }
    }
    
    /// <summary>
    /// Optimization goals for construction projects
    /// </summary>
    [System.Serializable]
    public class ConstructionOptimizationGoals
    {
        [Header("Goal Information")]
        public string GoalId;
        public string GoalName;
        public string Description;
        
        [Header("Objectives")]
        public List<string> Objectives;
        public Dictionary<string, float> Weights;
        public Dictionary<string, float> Constraints;
        public Dictionary<string, float> Targets;
        
        [Header("Priority Settings")]
        public bool OptimizeForCost;
        public bool OptimizeForTime;
        public bool OptimizeForQuality;
        public bool OptimizeForSafety;
        public bool OptimizeForEfficiency;
        
        [Header("Goal Metrics")]
        public float SuccessThreshold;
        public DateTime TargetDate;
        public float CurrentProgress;
        
        public ConstructionOptimizationGoals()
        {
            GoalId = System.Guid.NewGuid().ToString();
            Objectives = new List<string>();
            Weights = new Dictionary<string, float>();
            Constraints = new Dictionary<string, float>();
            Targets = new Dictionary<string, float>();
            
            // Default optimization settings
            OptimizeForCost = true;
            OptimizeForTime = true;
            OptimizeForQuality = true;
            OptimizeForSafety = true;
            OptimizeForEfficiency = true;
            
            SuccessThreshold = 0.8f;
            TargetDate = DateTime.Now.AddDays(30);
            CurrentProgress = 0f;
        }
        
        /// <summary>
        /// Calculate weighted optimization score
        /// </summary>
        public float CalculateOptimizationScore(Dictionary<string, float> actualValues)
        {
            float totalScore = 0f;
            float totalWeight = 0f;
            
            foreach (var objective in Objectives)
            {
                if (Weights.ContainsKey(objective) && actualValues.ContainsKey(objective))
                {
                    float weight = Weights[objective];
                    float actual = actualValues[objective];
                    float target = Targets.ContainsKey(objective) ? Targets[objective] : 1f;
                    
                    float objectiveScore = Mathf.Clamp01(actual / target);
                    totalScore += objectiveScore * weight;
                    totalWeight += weight;
                }
            }
            
            return totalWeight > 0 ? totalScore / totalWeight : 0f;
        }
    }

    #endregion

} // End namespace ProjectChimera.Data.Construction.Resources