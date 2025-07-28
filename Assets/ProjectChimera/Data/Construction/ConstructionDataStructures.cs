using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Core.Interfaces;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Construction
{
    // Building and Construction System Data Structures
    [System.Serializable]
    public enum BuildingType
    {
        GrowTent,
        GrowRoom,
        Greenhouse,
        ProcessingFacility,
        StorageWarehouse,
        LaboratoryFacility,
        OfficeSpace,
        SecurityFacility,
        UtilityBuilding,
        ResearchFacility
    }

    [System.Serializable]
    public enum ConstructionStage
    {
        Planning,
        Foundation,
        Framing,
        Utilities,
        Electrical,
        Plumbing,
        HVAC,
        Insulation,
        Drywall,
        Flooring,
        Equipment,
        Finishing,
        Inspection,
        Completed
    }

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
    public enum ConstructionPriority
    {
        Low,
        Normal,
        High,
        Emergency
    }

    [System.Serializable]
    public enum PermitType
    {
        Building,
        Electrical,
        Plumbing,
        HVAC,
        Fire_Safety,
        Environmental,
        Zoning,
        Cannabis_License,
        Mechanical,
        Fire,
        Cannabis_Cultivation,
        Cannabis_Processing
    }

    [System.Serializable]
    public enum InspectionType
    {
        Foundation,
        Framing,
        Electrical,
        Plumbing,
        HVAC,
        Insulation,
        Fire_Safety,
        Final,
        Cannabis_Compliance,
        Safety,
        Structural
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
    public enum BuildingQuality
    {
        Basic,
        Standard,
        Premium,
        Luxury,
        Industrial
    }

    // Missing types that are causing CS0234 errors - these are construction-specific versions
    [System.Serializable]
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Medium,
        Hard,
        Expert
    }

    [System.Serializable]
    public enum ProjectType
    {
        Residential,
        Commercial,
        Industrial,
        Agricultural,
        Research,
        Mixed,
        GrowRoom,
        ProcessingFacility,
        Greenhouse,
        Laboratory
    }

    [System.Serializable]
    public enum ChallengeType
    {
        TimeTrial,
        Budget,
        Quality,
        Efficiency,
        Innovation,
        Safety,
        SpaceOptimization
    }

    // Additional missing types for collaborative construction
    [System.Serializable]
    public enum CollaborativeSessionStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled
    }

    [System.Serializable]
    public enum ShareDataBetweenSessions
    {
        None,
        BasicData,
        FullData
    }

    [System.Serializable]
    public enum ConflictResolutionStrategy
    {
        Vote,
        Authority,
        Consensus,
        Automatic
    }

    [System.Serializable]
    public enum ParticipantRole
    {
        Lead,
        Architect,
        Engineer,
        Designer,
        Reviewer,
        Observer
    }

    [System.Serializable]
    public enum InnovationType
    {
        Design,
        Material,
        Process,
        Technology,
        Sustainability,
        Safety,
        Efficiency
    }

    /// <summary>
    /// Comprehensive facility information including capacity, utilization, and operational metrics
    /// </summary>
    [System.Serializable]
    public class FacilityInfo
    {
        [Header("Facility Identification")]
        public string ProjectId;
        public string FacilityName;
        public BuildingType FacilityType;
        public ProjectStatus Status;
        
        [Header("Capacity and Utilization")]
        public float TotalCapacity; // Max plants or processing capacity
        public float CurrentUtilization; // Current usage
        public float UtilizationPercentage; // Utilization as percentage
        public float EstimatedCapacity; // Projected capacity when complete
        public float AvailableCapacity; // Remaining capacity
        
        [Header("Room Information")]
        public int TotalRooms;
        public int CompletedRooms;
        public int InProgressRooms;
        public int PlannedRooms;
        public float TotalArea; // Total floor area in sqft
        public float UsableArea; // Area available for cultivation/processing
        
        [Header("Resource Consumption")]
        public float PowerConsumption; // Daily power consumption in kWh
        public float WaterConsumption; // Daily water consumption in gallons
        public float CO2Consumption; // Daily CO2 consumption in lbs
        
        [Header("Operational Costs")]
        public float MaintenanceCost; // Annual maintenance cost
        public float OperationalCost; // Annual operational cost
        public float EnergyCost; // Annual energy cost
        public float WaterCost; // Annual water cost
        public float TotalOperatingCost; // Total annual operating cost
        
        [Header("Expansion and Growth")]
        public float ExpansionPotential; // Available area for expansion
        public float MaximumCapacity; // Theoretical maximum capacity
        public bool CanExpand; // Whether facility can be expanded
        public List<string> RecommendedExpansions; // Suggested expansion types
        
        [Header("Environmental Conditions")]
        public float AverageTemperature; // Average facility temperature
        public float AverageHumidity; // Average facility humidity
        public float AirQualityIndex; // Air quality rating
        public bool ClimateControlActive; // Climate control status
        
        [Header("Efficiency Metrics")]
        public float EnergyEfficiency; // Energy efficiency rating (0-1)
        public float WaterEfficiency; // Water efficiency rating (0-1)
        public float SpaceEfficiency; // Space utilization efficiency (0-1)
        public float OverallEfficiency; // Overall facility efficiency (0-1)
        
        [Header("Compliance and Safety")]
        public bool ComplianceStatus; // Regulatory compliance status
        public List<string> SafetyIncidents; // Recent safety incidents
        public DateTime LastInspectionDate; // Last safety inspection
        public DateTime NextInspectionDate; // Next scheduled inspection
        
        [Header("Performance Tracking")]
        public DateTime LastUpdated;
        public float DailyThroughput; // Daily processing throughput
        public float WeeklyThroughput; // Weekly processing throughput
        public float MonthlyThroughput; // Monthly processing throughput
        public float YearlyThroughput; // Yearly processing throughput
        
        [Header("Quality Metrics")]
        public float ProductQualityScore; // Average product quality
        public float ProcessingEfficiency; // Processing efficiency
        public float YieldEfficiency; // Yield efficiency
        public float WasteReduction; // Waste reduction percentage
        
        [Header("Automation and Technology")]
        public int AutomationLevel; // Level of automation (0-5)
        public List<string> InstalledSystems; // Installed automation systems
        public List<string> PendingUpgrades; // Pending technology upgrades
        public bool SmartControlsActive; // Smart control system status
        
        [Header("Staff and Workforce")]
        public int TotalStaff; // Total staff count
        public int ActiveStaff; // Currently active staff
        public float StaffProductivity; // Staff productivity rating
        public float StaffSatisfaction; // Staff satisfaction rating
        
        [Header("Economic Performance")]
        public float RevenueGeneration; // Monthly revenue generation
        public float ProfitMargin; // Profit margin percentage
        public float ROI; // Return on investment
        public float PaybackPeriod; // Payback period in months
        
        public FacilityInfo()
        {
            RecommendedExpansions = new List<string>();
            SafetyIncidents = new List<string>();
            InstalledSystems = new List<string>();
            PendingUpgrades = new List<string>();
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Calculates comprehensive facility efficiency score
        /// </summary>
        public float CalculateOverallEfficiency()
        {
            OverallEfficiency = (EnergyEfficiency + WaterEfficiency + SpaceEfficiency) / 3f;
            return OverallEfficiency;
        }
        
        /// <summary>
        /// Determines if facility needs immediate attention
        /// </summary>
        public bool RequiresAttention()
        {
            return OverallEfficiency < 0.6f || 
                   !ComplianceStatus || 
                   SafetyIncidents.Count > 0 ||
                   UtilizationPercentage > 95f;
        }
        
        /// <summary>
        /// Gets facility status summary
        /// </summary>
        public string GetStatusSummary()
        {
            if (RequiresAttention())
                return "Requires Attention";
            if (UtilizationPercentage > 85f)
                return "High Utilization";
            if (OverallEfficiency > 0.8f)
                return "Optimal Performance";
            return "Normal Operations";
        }
        
        /// <summary>
        /// Gets next recommended action
        /// </summary>
        public string GetNextRecommendedAction()
        {
            if (!ComplianceStatus)
                return "Address compliance issues";
            if (SafetyIncidents.Count > 0)
                return "Resolve safety incidents";
            if (UtilizationPercentage > 90f && CanExpand)
                return "Consider facility expansion";
            if (OverallEfficiency < 0.7f)
                return "Optimize facility operations";
            if (PendingUpgrades.Count > 0)
                return "Install pending upgrades";
            return "Continue monitoring";
        }
    }

    // Add other necessary enums for compilation
    [System.Serializable]
    public enum ProjectStatus
    {
        Planning,
        RequiresRevision,
        PermitPending,
        ReadyToStart,
        InProgress,
        OnHold,
        Completed,
        Cancelled,
        Paused
    }

    [System.Serializable]
    public enum RoomStatus
    {
        Planning,
        Planned,
        UnderConstruction,
        Completed,
        Active,
        Maintenance,
        Idle,
        InProgress,
        OnHold,
        Cancelled,
        Inactive,
        Decommissioned
    }

    [System.Serializable]
    public enum ConstructionPhase
    {
        Planning,
        Permitting,
        SitePreparation,
        Foundation,
        Structure,
        Systems,
        Finishing,
        Final,
        Completed
    }

    [System.Serializable]
    public enum TaskStatus
    {
        Not_Started,
        In_Progress,
        Completed,
        On_Hold,
        Cancelled
    }

    [System.Serializable]
    public enum TaskPriority
    {
        Low,
        Normal,
        High,
        Critical
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

    [System.Serializable]
    public enum SafetyComplianceLevel
    {
        Unknown,
        NonCompliant,
        PartiallyCompliant,
        Compliant,
        ExceedsStandards
    }

    [System.Serializable]
    public enum PermitStatus
    {
        Not_Applied,
        Application_Submitted,
        Under_Review,
        Approved,
        Issued,
        Rejected,
        Expired,
        Submitted
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
    public enum WorkerStatus
    {
        Assigned,
        Active,
        On_Break,
        Completed,
        Reassigned
    }

    [System.Serializable]
    public enum InspectionResult
    {
        Pending,
        Passed,
        Failed,
        Conditional_Pass,
        Cancelled
    }

    [System.Serializable]
    public enum RoomType
    {
        General,
        GrowRoom,
        ProcessingRoom,
        StorageRoom,
        OfficeSpace,
        LaboratoryRoom,
        SecurityRoom,
        UtilityRoom,
        MaintenanceRoom,
        ReceptionArea,
        ConferenceRoom,
        // Cannabis-specific room types
        Propagation,
        Vegetative,
        Flowering,
        Drying,
        Curing,
        Storage,
        Processing,
        Laboratory,
        Office,
        Security,
        Utility,
        Corridor,
        Restroom,
        Break,
        Meeting
    }

    // Core data structures with proper class definitions

    [System.Serializable]
    public class MaterialRequirement
    {
        public string MaterialId;
        public string MaterialName;
        public BuildingMaterial Type;
        public float RequiredQuantity;
        public float AvailableQuantity;
        public float OrderedQuantity;
        public string Unit;
        public float UnitCost;
        public string Supplier;
        public DateTime RequiredDate;
        public DateTime OrderDate;
        public DateTime ExpectedDelivery;
        public MaterialStatus Status;
    }

    [System.Serializable]
    public class ConstructionTask
    {
        [Header("Task Information")]
        public string TaskId;
        public string ProjectId;
        public string TaskName;
        public string Description;
        public ConstructionStage Stage;
        public ConstructionPhase ConstructionPhase;
        public DateTime StartDate;
        public DateTime EndDate;
        public int DurationDays;
        public float EstimatedHours;
        public int RequiredWorkerCount;
        public float Progress; // 0-1
        public TaskStatus Status;
        public List<string> Prerequisites;
        public List<string> RequiredWorkers;
        public List<MaterialRequirement> RequiredMaterials;
        public WorkerSpecialty RequiredSpecialty;
        public float Cost;
        public TaskPriority Priority;

        public bool InProgress => Status == TaskStatus.In_Progress;
    }

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

        [Header("Certifications")]
        public List<string> Certifications;
        public List<string> Licenses;
        public DateTime LastSafetyTraining;
    }

    [System.Serializable]
    public class ConstructionIssue
    {
        public string IssueId;
        public string Title;
        public string Description;
        public IssueType IssueType;
        public IssueSeverity Severity;
        public IssueCategory Category;
        public DateTime ReportedDate;
        public string ReportedBy;
        public DateTime ResolvedDate;
        public string ResolvedBy;
        public IssueStatus Status;
        public string ResolutionDescription;
        public float CostImpact;
        public int DelayDays;
    }

    [System.Serializable]
    public class PermitApplication
    {
        [Header("Application Information")]
        public string ApplicationId;
        public string ProjectId;
        public PermitType PermitType;
        public DateTime ApplicationDate;
        public DateTime ExpectedApprovalDate;
        public PermitStatus Status;
        public string IssuingAuthority;
        public float Fee;
        public List<string> RequiredDocuments;
        public List<string> SubmittedDocuments;
        public string Notes;

        public DateTime SubmissionDate;
        public int EstimatedProcessingDays;
        public float ApplicationFee;
        public bool Submitted => Status != PermitStatus.Not_Applied;

        public DateTime EstimatedProcessingDate => ApplicationDate.AddDays(7); // Default 7 days
        public DateTime SubmittedDate => SubmissionDate;
        public DateTime ApprovalDate;
        public string RejectionReason;
    }

    [System.Serializable]
    public class ConstructionProgress
    {
        public string ProgressId;
        public string ProjectId;
        public string TaskId;
        public ConstructionTask Task;
        public ConstructionProject Project; // Added for compatibility
        public DateTime StartTime;
        public float Progress; // 0-1
        public float CompletionPercentage; // 0-1
        public TaskStatus Status;
        public DateTime CompletionTime;
        public List<ConstructionWorker> AssignedWorkers;
        public float EstimatedCost;
        public float ActualCost;
        public DateTime LastUpdated;
        public List<string> CompletedMilestones;
        public List<ConstructionIssue> Issues;
    }

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

        [Header("Building Constraints")]
        public Vector2 MaxBuildingSize;
        public Vector3 MaxRoomSize;
        public Vector3 MinRoomSize;
        public float WallThickness;
        public bool RequireFoundation;
        public bool EnforceFireSafety;
        public bool RequireVentilation;
        public float MaxBuildingHeight;
        public float MinSetbackDistance;
        public int RequiredParkingSpaces;
        public float MaxLotCoverage;

        [Header("Economic Settings")]
        public float DemolitionCostPerSqFt;
        public float LaborCostPerHour;
        public float MaterialMarkup;

        [Header("Quality Control")]
        public bool EnableQualitySystem;
        public float QualityThreshold;
        public bool AutoFailOnCriticalIssues;
    }

    [System.Serializable]
    public class GridSnapSettings
    {
        public float GridSize;
        public bool SnapToGrid;
        public bool ShowGrid;
        public Color GridColor;
        public Color MajorGridColor;
        public int MajorGridInterval;
        public Vector3 MinRoomSize;
        public Vector3 MaxRoomSize;
        public float WallThickness;
        public bool RequireFoundation;
        public bool EnforceFireSafety;
        public bool RequireVentilation;
        public float MaxBuildingHeight;
        public float MinSetbackDistance;
        public int RequiredParkingSpaces;
        public float MaxLotCoverage;
    }

    [System.Serializable]
    public class FacilityTemplate
    {
        [Header("Template Information")]
        public string TemplateId;
        public string TemplateName;
        public string Description;
        public BuildingType FacilityType;
        public BuildingQuality QualityLevel;

        [Header("Dimensions and Layout")]
        public Vector2 Dimensions;
        public float TotalArea;
        public List<ConstructionRoomTemplate> RoomTemplates;

        [Header("System Requirements")]
        public float RequiredHVACCapacity; // in tons
        public float RequiredPowerCapacity; // in watts

        [Header("Construction Requirements")]
        public List<MaterialRequirement> RequiredMaterials;
        public List<PermitType> RequiredPermits;
        public List<WorkerSpecialty> RequiredSpecialties;
        public int EstimatedConstructionDays;

        [Header("Cost Estimates")]
        public float BaseConstructionCost;
        public float EstimatedMaterialCost;
        public float EstimatedLaborCost;
        public float EstimatedPermitCost;
        public float EstimatedTotalCost;

        [Header("Compliance and Standards")]
        public List<string> BuildingCodes;
        public List<string> SafetyStandards;
        public bool CannabisSuitability;

        public List<ConstructionRoomTemplate> Rooms => RoomTemplates;
    }

    [System.Serializable]
    public class ConstructionRoomTemplate
    {
        [Header("Room Information")]
        public string TemplateRoomId;
        public string RoomName;
        public string RoomType;
        public string Description;

        [Header("Dimensions")]
        public Vector2 Dimensions;
        public float Area;
        public float Height;
        public float Length;
        public float Width;

        [Header("Cost")]
        public float EstimatedCost;
        public List<MaterialRequirement> MaterialRequirements;

        public bool IsGrowRoom => RoomType == "GrowRoom";
        public bool IsProcessingRoom => RoomType == "ProcessingRoom";
    }

    [System.Serializable]
    public class PlannedRoom
    {
        public string PlannedRoomId;
        public ConstructionRoomTemplate RoomTemplate;
        public Vector3 Position;
        public Quaternion Rotation;
        public RoomStatus Status;
        public float EstimatedCost;
        public float ActualCost;
        public DateTime PlannedDate;
        public DateTime CompletedDate;
        public List<string> ModificationNotes;
    }

    [System.Serializable]
    public class ConstructionProject
    {
        [Header("Project Information")]
        public string ProjectId;
        public string ProjectName;
        public string Description;
        public ProjectType ProjectType;
        public Vector3 BuildingSite;
        public Vector3 Position;
        public FacilityTemplate FacilityTemplate;

        [Header("Status and Progress")]
        public ProjectStatus Status;
        public ConstructionPhase CurrentPhase;
        public List<ConstructionPhase> CompletedPhases;
        public float OverallProgress; // 0-1
        public List<string> CompletedTasks;

        [Header("Dates")]
        public DateTime CreatedDate;
        public DateTime StartDate;
        public DateTime EstimatedCompletionDate;
        public DateTime ActualCompletionDate;
        public DateTime CompletionDate;

        [Header("Cost and Budget")]
        public float TotalBudget;
        public float EstimatedCost;
        public float ActualCost;
        public float RemainingBudget;

        [Header("Duration")]
        public int EstimatedDuration; // days
        public int ActualDuration; // days

        [Header("Permits and Validation")]
        public List<string> RequiredPermits = new List<string>();
        public List<PermitApplication> Permits;
        public bool PermitsApproved;
        public ValidationResult ValidationResults;

        [Header("Planning")]
        public List<ConstructionTask> Tasks;
        public List<PlannedRoom> PlannedRooms = new List<PlannedRoom>();

        [Header("Issues and Quality")]
        public List<ConstructionIssue> Issues;

        [Header("Worker Management")]
        public List<WorkerAssignment> WorkerAssignments = new List<WorkerAssignment>();
        public List<ConstructionWorker> AssignedWorkers = new List<ConstructionWorker>(); // Added for compatibility

        public List<PermitApplication> ApprovedPermits => new List<PermitApplication>();
        public List<PermitApplication> RejectedPermits => new List<PermitApplication>();
        public List<PermitType> ApprovedPermitTypes = new List<PermitType>();
    }

    // Supporting classes for construction management
    [System.Serializable]
    public class MaterialInventory
    {
        public List<MaterialStock> Materials;
        public Dictionary<string, MaterialStock> MaterialLookup;

        public MaterialInventory()
        {
            Materials = new List<MaterialStock>();
            MaterialLookup = new Dictionary<string, MaterialStock>();
        }

        public bool HasMaterial(string materialType, float quantity)
        {
            return MaterialLookup.ContainsKey(materialType) && MaterialLookup[materialType].AvailableQuantity >= quantity;
        }
        
        public bool HasMaterials(List<string> requiredMaterials)
        {
            foreach (var mat in requiredMaterials)
            {
                if (!MaterialLookup.ContainsKey(mat)) return false;
            }
            return true;
        }
        
        public bool HasMaterials(List<MaterialRequirement> requiredMaterials)
        {
            foreach (var req in requiredMaterials)
            {
                if (!HasMaterial(req.MaterialName, req.RequiredQuantity)) return false;
            }
            return true;
        }

        public void AddMaterial(string materialType, float quantity, float costPerUnit)
        {
            if (MaterialLookup.ContainsKey(materialType))
            {
                MaterialLookup[materialType].AvailableQuantity += quantity;
            }
            else
            {
                var newStock = new MaterialStock { MaterialType = materialType, AvailableQuantity = quantity, CostPerUnit = costPerUnit };
                Materials.Add(newStock);
                MaterialLookup[materialType] = newStock;
            }
        }

        public bool ConsumeMaterial(string materialType, float quantity)
        {
            if (HasMaterial(materialType, quantity))
            {
                MaterialLookup[materialType].AvailableQuantity -= quantity;
                return true;
            }
            return false;
        }
        
        public MaterialStock GetMaterialData(string materialType)
        {
            return MaterialLookup.ContainsKey(materialType) ? MaterialLookup[materialType] : null;
        }
    }

    [System.Serializable]
    public class MaterialStock
    {
        public string MaterialType;
        public float AvailableQuantity;
        public float CostPerUnit;
        public DateTime LastUpdated;
        public string Supplier;
        public DateTime ExpirationDate;
        public string StorageLocation;
    }

    [System.Serializable]
    public class ConstructionWorkforce
    {
        public List<ConstructionWorker> Workers;
        public Dictionary<string, ConstructionWorker> WorkerLookup;

        public ConstructionWorkforce()
        {
            Workers = new List<ConstructionWorker>();
            WorkerLookup = new Dictionary<string, ConstructionWorker>();
        }

        public void AddWorker(ConstructionWorker worker)
        {
            Workers.Add(worker);
            WorkerLookup[worker.WorkerId] = worker;
        }

        public List<ConstructionWorker> GetAvailableWorkers(WorkerSpecialty specialty = WorkerSpecialty.GeneralConstruction)
        {
            return Workers.Where(w => w.IsAvailable && w.Specialty == specialty).ToList();
        }

        public List<ConstructionWorker> GetActiveWorkers()
        {
            return Workers.Where(w => !w.IsAvailable).ToList();
        }
        
        public ConstructionWorker GetWorker(string workerId)
        {
            return WorkerLookup.ContainsKey(workerId) ? WorkerLookup[workerId] : null;
        }

        public List<ConstructionWorker> AssignWorkers(ConstructionTask task, int workerCount)
        {
            var availableWorkers = GetAvailableWorkers(task.RequiredSpecialty).Take(workerCount).ToList();
            foreach (var worker in availableWorkers)
            {
                worker.IsAvailable = false;
                worker.CurrentProjectId = task.ProjectId;
                worker.CurrentTaskId = task.TaskId;
            }
            return availableWorkers;
        }

        public List<ConstructionWorker> AssignWorkers(WorkerSpecialty specialty, int workerCount)
        {
            var availableWorkers = GetAvailableWorkers(specialty).Take(workerCount).ToList();
            foreach (var worker in availableWorkers)
            {
                worker.IsAvailable = false;
            }
            return availableWorkers;
        }

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
                }
            }
        }
    }

    // Supporting classes for design tools
    [System.Serializable]
    public class FacilityDesignTool
    {
        public GridSnapSettings GridSettings;
        public bool IsDesigning;
        public FacilityTemplate CurrentTemplate;
        public List<GameObject> PreviewObjects;
        
        public FacilityDesignTool(GridSnapSettings gridSettings)
        {
            GridSettings = gridSettings;
            PreviewObjects = new List<GameObject>();
        }

        public void StartDesign(FacilityTemplate template)
        {
            IsDesigning = true;
            CurrentTemplate = template;
            // Logic to enter design mode
        }

        public void EndDesign()
        {
            IsDesigning = false;
            ClearPreviews();
            // Logic to exit design mode
        }

        public GameObject CreateRoomPreview(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            // Logic to create a visual preview of a room
            return new GameObject("RoomPreview");
        }

        private void ClearPreviews()
        {
            foreach (var preview in PreviewObjects)
            {
                GameObject.Destroy(preview);
            }
            PreviewObjects.Clear();
        }
    }

    [System.Serializable]
    public class BuildingValidator
    {
        public ConstructionSettings Settings;

        public BuildingValidator(ConstructionSettings settings)
        {
            Settings = settings;
        }

        public ValidationResult ValidateBuildingSite(Vector3 buildingSite, FacilityTemplate template)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };
            // Example validation logic
            if (buildingSite.y < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Building site cannot be underground.");
            }
            return result;
        }

        public bool ValidateRoomPlacement(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            // Placeholder for room placement validation
            return true;
        }
    }

    [System.Serializable]
    public class ConstructionPlanner
    {
        public List<ConstructionTask> CreateTasksForPhase(ConstructionProject project, ConstructionPhase phase)
        {
            // Logic to generate tasks based on phase and template
            var tasks = new List<ConstructionTask>();
            // Example:
            tasks.Add(new ConstructionTask { TaskId = "TASK-001", TaskName = "Clear Site" });
            return tasks;
        }

        public List<ConstructionTask> GetTasksForPhase(ConstructionProject project, ConstructionPhase phase)
        {
            return project.Tasks.Where(t => t.ConstructionPhase == phase).ToList();
        }
    }

    [System.Serializable]
    public class ConstructionSchedule
    {
        public string ScheduleId;
        public DateTime ProjectStartDate;
        public DateTime ProjectEndDate;
        public List<ConstructionTask> Tasks;
        public float OverallProgress;
        public bool IsOnSchedule;
        public int DelayDays;
    }

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
        public float AverageCostPerSqFt;
        public float AverageCostOverrun;

        [Header("Quality Metrics")]
        public float AverageQualityScore;
        public int TotalDefects;
        public int TotalRework;
        public float CustomerSatisfactionRating;

        [Header("Efficiency Metrics")]
        public float AverageProjectDuration;
        public float MaterialWastePercentage;
        public float ScheduleAdherence;
        
        // Additional properties referenced in error messages
        public float AverageCompletionTime;
        public float WorkerProductivity;

        public List<ConstructionWorker> GetActiveWorkers()
        {
            return new List<ConstructionWorker>();
        }
    }

    [System.Serializable]
    public class ConstructionCostUpdate
    {
        public string ProjectId;
        public float PreviousCost;
        public float NewCost;
        public float EstimatedCost;
        public float ActualCostToDate;
        public float CostDifference;
        public float CostVariance;
        public string Reason;
        public DateTime UpdateDate;
        public string UpdatedBy;
    }

    [System.Serializable]
    public class ConstructionEvent
    {
        public string EventId;
        public string ProjectId;
        public string TaskId;
        public DateTime EventDate;
        public string Description;
        public string Details;
        public float FinancialImpact;
        public float ActualCost;
        public IssueSeverity Severity;
        public string ReportedBy;
    }

    [System.Serializable]
    public class ConstructionDesignSolution
    {
        public string SolutionId;
        public string Name;
        public string Description;
        public List<string> Features;
        public float Cost;
        public float Efficiency;
        public float QualityScore;
        public DateTime CreatedDate;
        public string CreatedBy;
        
        public ConstructionDesignSolution()
        {
            Features = new List<string>();
        }
    }
    
    [System.Serializable]
    public class ConstructionOptimizationGoals
    {
        public string GoalId;
        public List<string> Objectives;
        public Dictionary<string, float> Weights;
        public Dictionary<string, float> Constraints;
        
        public ConstructionOptimizationGoals()
        {
            Objectives = new List<string>();
            Weights = new Dictionary<string, float>();
            Constraints = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// Comprehensive facility expansion planning data structure
    /// </summary>
    [System.Serializable]
    public class FacilityExpansionPlan
    {
        [Header("Expansion Identification")]
        public string ExpansionPlanId;
        public string FacilityProjectId;
        public string ExpansionName;
        public string Description;
        
        [Header("Expansion Scope")]
        public List<ConstructionRoomTemplate> NewRooms = new List<ConstructionRoomTemplate>();
        public List<string> RoomModifications = new List<string>();
        public List<string> EquipmentUpgrades = new List<string>();
        public List<string> SystemUpgrades = new List<string>();
        
        [Header("Budget and Resources")]
        public float TotalBudget;
        public float EstimatedCost;
        public float MaterialCosts;
        public float LaborCosts;
        public float PermitCosts;
        public float ContingencyFunds;
        
        [Header("Timeline")]
        public DateTime PlannedStartDate;
        public DateTime EstimatedCompletionDate;
        public int EstimatedDurationDays;
        public List<string> CriticalMilestones = new List<string>();
        
        [Header("Capacity Impact")]
        public float CurrentCapacity;
        public float AdditionalCapacity;
        public float FinalCapacity;
        public float CapacityIncrease;
        public float ROIProjection;
        
        [Header("Requirements")]
        public List<PermitType> RequiredPermits = new List<PermitType>();
        public List<MaterialRequirement> MaterialRequirements = new List<MaterialRequirement>();
        public List<WorkerSpecialty> RequiredSpecialties = new List<WorkerSpecialty>();
        
        [Header("Risk Assessment")]
        public float RiskLevel; // 0-1 scale
        public List<string> IdentifiedRisks = new List<string>();
        public List<string> MitigationStrategies = new List<string>();
        
        public FacilityExpansionPlan()
        {
            ExpansionPlanId = System.Guid.NewGuid().ToString();
            PlannedStartDate = DateTime.Now.AddDays(30);
            EstimatedCompletionDate = DateTime.Now.AddDays(90);
            EstimatedDurationDays = 60;
        }
        
        /// <summary>
        /// Calculates the financial viability of the expansion
        /// </summary>
        public bool IsFinanciallyViable()
        {
            return TotalBudget >= EstimatedCost && ROIProjection > 0;
        }
        
        /// <summary>
        /// Gets the expansion complexity level
        /// </summary>
        public ComplexityLevel GetComplexityLevel()
        {
            if (NewRooms.Count <= 2 && SystemUpgrades.Count <= 3)
                return ComplexityLevel.Simple;
            if (NewRooms.Count <= 5 && SystemUpgrades.Count <= 6)
                return ComplexityLevel.Moderate;
            if (NewRooms.Count <= 10 && SystemUpgrades.Count <= 10)
                return ComplexityLevel.Complex;
            if (NewRooms.Count <= 15)
                return ComplexityLevel.Very_Complex;
            return ComplexityLevel.Extreme;
        }
    }
    
    /// <summary>
    /// Missing classes for construction system compilation
    /// </summary>
    [System.Serializable]
    public class WorkerAssignment
    {
        public string AssignmentId;
        public string WorkerId;
        public string WorkerName;
        public WorkerSpecialty Specialty;
        public DateTime StartDate;
        public DateTime EndDate;
        public float HoursPerDay;
        public float HourlyRate;
        public string TaskDescription;
        public float CompletionPercentage;
        public WorkerStatus Status;
    }

    [System.Serializable]
    public class QualityInspection
    {
        public string InspectionId;
        public string ProjectId;
        public InspectionType Type;
        public DateTime InspectionDate;
        public InspectionResult Result;
        public float QualityScore;
        public List<string> Notes = new List<string>();
        public string InspectorName;
    }

    [System.Serializable]
    public class EquipmentPool
    {
        public Dictionary<string, int> AvailableEquipment = new Dictionary<string, int>();
        public Dictionary<string, float> EquipmentCosts = new Dictionary<string, float>();
        
        public bool HasEquipment(string equipmentType, int quantity = 1)
        {
            return AvailableEquipment.ContainsKey(equipmentType) && AvailableEquipment[equipmentType] >= quantity;
        }
        
        public bool ReserveEquipment(string equipmentType, int quantity = 1)
        {
            if (HasEquipment(equipmentType, quantity))
            {
                AvailableEquipment[equipmentType] -= quantity;
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class ContractorManager
    {
        public Dictionary<string, string> AvailableContractors = new Dictionary<string, string>();
        public Dictionary<string, float> ContractorRates = new Dictionary<string, float>();
        
        public string FindContractor(WorkerSpecialty specialty)
        {
            return AvailableContractors.ContainsKey(specialty.ToString()) ? AvailableContractors[specialty.ToString()] : null;
        }
    }

    [System.Serializable]
    public class ConstructionReport
    {
        [Header("Report Information")]
        public string ReportId;
        public string ReportTitle;
        public DateTime GeneratedDate;
        public DateTime ReportDate; // Added for compatibility
        public string GeneratedBy;
        public string ProjectId;
        public string ProjectName;
        
        [Header("Project Summaries")]
        public List<ProjectSummary> ProjectSummaries = new List<ProjectSummary>();
        
        [Header("Overall Metrics")]
        public ConstructionMetrics TotalMetrics;
        
        [Header("Project Status")]
        public ProjectStatus Status;
        public float OverallProgress;
        public int TotalTasks;
        public int CompletedTasks;
        public int ActiveTasks;
        public int PendingTasks;
        
        [Header("Financial Summary")]
        public float TotalBudget;
        public float ActualCost;
        public float RemainingBudget;
        public float CostOverrun;
        public float BudgetUtilization;
        
        [Header("Timeline")]
        public DateTime StartDate;
        public DateTime EstimatedCompletionDate;
        public DateTime ActualCompletionDate;
        public int EstimatedDuration;
        public int ActualDuration;
        public int DelayDays;
        
        [Header("Quality Metrics")]
        public float AverageQualityScore;
        public int QualityIssues;
        public int SafetyIncidents;
        public float ComplianceRating;
        
        [Header("Resource Utilization")]
        public int TotalWorkers;
        public int ActiveWorkers;
        public float WorkerEfficiency;
        public float MaterialUtilization;
        public float EquipmentUtilization;
        
        [Header("Issues and Risks")]
        public List<string> ActiveIssues = new List<string>();
        public List<string> ResolvedIssues = new List<string>();
        public List<string> IdentifiedRisks = new List<string>();
        public List<string> Recommendations = new List<string>();
        
        [Header("Summary")]
        public string ExecutiveSummary;
        public string StatusSummary;
        public string NextSteps;
        
        public ConstructionReport()
        {
            ReportId = System.Guid.NewGuid().ToString();
            GeneratedDate = DateTime.Now;
            ReportDate = DateTime.Now; // Initialize ReportDate
            GeneratedBy = "System";
            ProjectSummaries = new List<ProjectSummary>();
            TotalMetrics = new ConstructionMetrics();
        }
        
        /// <summary>
        /// Determines if the project is on track
        /// </summary>
        public bool IsOnTrack()
        {
            return DelayDays <= 0 && CostOverrun <= 0.1f && AverageQualityScore >= 80f;
        }
        
        /// <summary>
        /// Gets the overall project health rating
        /// </summary>
        public string GetHealthRating()
        {
            if (IsOnTrack() && ActiveIssues.Count == 0)
                return "Excellent";
            if (DelayDays <= 5 && CostOverrun <= 0.2f)
                return "Good";
            if (DelayDays <= 15 && CostOverrun <= 0.3f)
                return "Fair";
            return "Poor";
        }
    }
    
    /// <summary>
    /// Project summary information for construction projects
    /// </summary>
    [System.Serializable]
    public class ProjectSummary
    {
        [Header("Project Identification")]
        public string ProjectId;
        public string ProjectName;
        public ProjectType ProjectType;
        public ProjectStatus Status;
        
        [Header("Timeline")]
        public DateTime StartDate;
        public DateTime PlannedEndDate;
        public DateTime ActualEndDate;
        public DateTime EstimatedCompletion; // Added for compatibility
        public float CompletionPercentage;
        public float Progress; // Added for compatibility
        
        [Header("Budget")]
        public float TotalBudget;
        public float EstimatedCost; // Added for compatibility
        public float SpentAmount;
        public float ActualCostToDate; // Added for compatibility
        public float RemainingBudget;
        public float CostOverrun;
        
        [Header("Quality Metrics")]
        public float OverallQualityScore;
        public int PassedInspections;
        public int FailedInspections;
        public int ActiveIssues;
        
        [Header("Performance")]
        public int TotalTasks;
        public int CompletedTasks;
        public float ProductivityScore;
        public float EfficiencyRating;
        
        public ProjectSummary()
        {
            ProjectId = System.Guid.NewGuid().ToString();
            StartDate = DateTime.Now;
            EstimatedCompletion = DateTime.Now.AddDays(30); // Default 30 day estimate
            CompletionPercentage = 0f;
            Progress = 0f;
            EstimatedCost = 0f;
            ActualCostToDate = 0f;
        }
        
        /// <summary>
        /// Calculate overall project health score
        /// </summary>
        public float GetHealthScore()
        {
            float scheduleScore = CompletionPercentage >= 80f ? 25f : (CompletionPercentage / 80f * 25f);
            float budgetScore = CostOverrun <= 0.1f ? 25f : (CostOverrun > 0.5f ? 0f : (0.5f - CostOverrun) / 0.4f * 25f);
            float qualityScore = OverallQualityScore >= 80f ? 25f : (OverallQualityScore / 80f * 25f);
            float issueScore = ActiveIssues == 0 ? 25f : (ActiveIssues > 10 ? 0f : (10 - ActiveIssues) / 10f * 25f);
            
            return scheduleScore + budgetScore + qualityScore + issueScore;
        }
        
        /// <summary>
        /// Get project status description
        /// </summary>
        public string GetStatusDescription()
        {
            float healthScore = GetHealthScore();
            if (healthScore >= 80f) return "Excellent - Project performing above expectations";
            if (healthScore >= 60f) return "Good - Project on track with minor issues";
            if (healthScore >= 40f) return "Fair - Project facing some challenges";
            return "Poor - Project requires immediate attention";
        }
    }
    
    /// <summary>
    /// Room data structure for construction system
    /// </summary>
    [System.Serializable]
    public class Room
    {
        [Header("Room Identification")]
        public string RoomId;
        public string RoomName;
        public string ProjectId;
        public RoomType RoomType;
        
        [Header("Physical Properties")]
        public Vector3 Position;
        public Vector2 Dimensions;
        public float FloorArea;
        public float CeilingHeight;
        public float Volume;
        
        [Header("Status")]
        public RoomStatus Status;
        public DateTime CreationDate;
        public DateTime CompletionDate;
        
        [Header("Configuration")]
        public RoomConfiguration Configuration;
        public EnvironmentalConditions EnvironmentalRequirements;
        public SecurityLevel SecurityLevel;
        public List<RegulatoryRequirement> RegulatoryRequirements = new List<RegulatoryRequirement>();
        
        [Header("Capacity")]
        public int MaxOccupancy;
        public int CurrentOccupancy;
        public float CapacityUtilization => MaxOccupancy > 0 ? (float)CurrentOccupancy / MaxOccupancy : 0f;
        
        public Room()
        {
            RoomId = System.Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
            Status = RoomStatus.Planning;
            RegulatoryRequirements = new List<RegulatoryRequirement>();
        }
    }
    
    /// <summary>
    /// Room configuration settings
    /// </summary>
    [System.Serializable]
    public class RoomConfiguration
    {
        public string ConfigurationId;
        public RoomType RoomType;
        public int MaxOccupancy;
        public EnvironmentalSettings EnvironmentalSettings;
        public SecuritySettings SecuritySettings;
        public List<string> EquipmentRequirements = new List<string>();
        public AccessControlLevel AccessControlLevel;
        public MonitoringLevel MonitoringLevel;
        
        public RoomConfiguration()
        {
            ConfigurationId = System.Guid.NewGuid().ToString();
            EquipmentRequirements = new List<string>();
        }
    }
    
    /// <summary>
    /// Environmental conditions for rooms
    /// </summary>
    [System.Serializable]
    public class EnvironmentalConditions
    {
        [Header("Temperature")]
        public Vector2 TemperatureRange; // Min/Max in Celsius
        public float OptimalTemperature;
        
        [Header("Humidity")]
        public Vector2 HumidityRange; // Min/Max percentage
        public float OptimalHumidity;
        
        [Header("Air Quality")]
        public float CO2Level; // PPM
        public float AirChangesPerHour;
        public float AirFlowRate; // CFM
        
        [Header("Lighting")]
        public float LightIntensity; // PPFD
        public float PhotoperiodHours;
        public LightSpectrum LightSpectrum;
        
        [Header("Pressure")]
        public float AtmosphericPressure; // kPa
        public float DifferentialPressure; // Pa
    }
    
    /// <summary>
    /// Room performance tracking data
    /// </summary>
    [System.Serializable]
    public class RoomPerformanceData
    {
        public string RoomId;
        public DateTime LastUpdated;
        
        [Header("Environmental Performance")]
        public float TemperatureStability;
        public float HumidityStability;
        public float CO2Stability;
        public float LightingConsistency;
        
        [Header("Energy Efficiency")]
        public float PowerConsumption; // kWh
        public float EnergyEfficiencyRating;
        public float CostPerSquareFoot;
        
        [Header("Utilization")]
        public float AverageOccupancy;
        public float PeakOccupancy;
        public int TotalUsageHours;
        
        [Header("Maintenance")]
        public int MaintenanceEvents;
        public float MaintenanceCost;
        public float DowntimeHours;
        
        public RoomPerformanceData()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Room design session for interactive design
    /// </summary>
    [System.Serializable]
    public class RoomDesignSession
    {
        public string SessionId;
        public string DesignerId;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        public bool IsComplete;
        public List<string> RoomIds = new List<string>();
        public DesignSessionData SessionData;
        
        public void Update()
        {
            // Update session logic
        }
        
        public RoomDesignSession()
        {
            SessionId = System.Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
            IsActive = true;
            IsComplete = false;
            RoomIds = new List<string>();
        }
    }
    
    /// <summary>
    /// Layout optimization result
    /// </summary>
    [System.Serializable]
    public class LayoutOptimizationResult
    {
        public string OptimizationId;
        public string RoomId;
        public bool IsSuccessful;
        public float ImprovementPercentage;
        public string OptimizationType;
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
        public List<OptimizationRecommendation> Recommendations = new List<OptimizationRecommendation>();
        public DateTime CompletedAt;
        
        public LayoutOptimizationResult()
        {
            OptimizationId = System.Guid.NewGuid().ToString();
            CompletedAt = DateTime.Now;
            Metrics = new Dictionary<string, float>();
            Recommendations = new List<OptimizationRecommendation>();
        }
    }
    
    /// <summary>
    /// Facility optimization result for multiple rooms
    /// </summary>
    [System.Serializable]
    public class FacilityOptimizationResult
    {
        public string FacilityId;
        public bool IsSuccessful;
        public float OverallImprovementPercentage;
        public List<LayoutOptimizationResult> RoomOptimizations = new List<LayoutOptimizationResult>();
        public FacilityMetrics OptimizedMetrics;
        public DateTime CompletedAt;
        
        public FacilityOptimizationResult()
        {
            CompletedAt = DateTime.Now;
            RoomOptimizations = new List<LayoutOptimizationResult>();
        }
    }
    
    /// <summary>
    /// Supporting enums for room system
    /// </summary>
    public enum SecurityLevel
    {
        None,
        Standard,
        Medium,
        High,
        Maximum
    }
    
    public enum AccessControlLevel
    {
        Public,
        Restricted,
        Authorized,
        HighSecurity,
        Maximum
    }
    
    public enum MonitoringLevel
    {
        None,
        Basic,
        Standard,
        Enhanced,
        Comprehensive
    }
    
    public enum ComplianceStatusType
    {
        Pending,
        Compliant,
        NonCompliant,
        UnderReview,
        Expired
    }
    
    /// <summary>
    /// Supporting data structures
    /// </summary>
    [System.Serializable]
    public class RegulatoryRequirement
    {
        public string RequirementType;
        public string Description;
        public bool IsMandatory;
        public ComplianceStatusType Status;
        public DateTime LastChecked;
        
        public RegulatoryRequirement()
        {
            IsMandatory = true;
            Status = ComplianceStatusType.Pending;
            LastChecked = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class EnvironmentalSettings
    {
        public bool AutomaticClimateControl;
        public bool CO2Supplementation;
        public bool DehumidificationEnabled;
        public bool AirCirculationEnabled;
        public float TargetTemperature;
        public float TargetHumidity;
        public float TargetCO2;
        
        public EnvironmentalSettings()
        {
            AutomaticClimateControl = true;
            CO2Supplementation = false;
            DehumidificationEnabled = true;
            AirCirculationEnabled = true;
        }
    }
    
    [System.Serializable]
    public class SecuritySettings
    {
        public bool AccessControlEnabled;
        public bool VideoSurveillanceEnabled;
        public bool MotionDetectionEnabled;
        public bool AlarmSystemEnabled;
        public bool BiometricAccess;
        public int MaxSimultaneousAccess;
        
        public SecuritySettings()
        {
            AccessControlEnabled = true;
            VideoSurveillanceEnabled = true;
            MotionDetectionEnabled = false;
            AlarmSystemEnabled = true;
            BiometricAccess = false;
            MaxSimultaneousAccess = 1;
        }
    }
    
    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public ComplianceStatusType ComplianceStatus;
        
        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
            Warnings = new List<string>();
            ComplianceStatus = ComplianceStatusType.Compliant;
        }
    }
    
    [System.Serializable]
    public class OptimizationCriteria
    {
        public bool OptimizeForEfficiency;
        public bool OptimizeForCost;
        public bool OptimizeForProduction;
        public bool OptimizeForCompliance;
        public float EfficiencyWeight;
        public float CostWeight;
        public float ProductionWeight;
        public float ComplianceWeight;
        
        public OptimizationCriteria()
        {
            OptimizeForEfficiency = true;
            OptimizeForCost = true;
            OptimizeForProduction = true;
            OptimizeForCompliance = true;
            EfficiencyWeight = 0.25f;
            CostWeight = 0.25f;
            ProductionWeight = 0.25f;
            ComplianceWeight = 0.25f;
        }
    }
    
    [System.Serializable]
    public class FacilityOptimizationCriteria
    {
        public OptimizationCriteria RoomCriteria;
        public bool OptimizeLayout;
        public bool OptimizeWorkflow;
        public bool OptimizeResourceSharing;
        public bool MinimizeOperationalCosts;
        
        public FacilityOptimizationCriteria()
        {
            RoomCriteria = new OptimizationCriteria();
            OptimizeLayout = true;
            OptimizeWorkflow = true;
            OptimizeResourceSharing = false;
            MinimizeOperationalCosts = true;
        }
    }
    
    [System.Serializable]
    public class OptimizationRecommendation
    {
        public string RecommendationType;
        public string Description;
        public float ExpectedImprovement;
        public float ImplementationCost;
        public int Priority; // 1-10 scale
        
        public OptimizationRecommendation()
        {
            Priority = 5;
        }
    }
    
    [System.Serializable]
    public class OptimizationOpportunity
    {
        public string OpportunityType;
        public string Description;
        public float PotentialImprovement;
        public string ImplementationGuidance;
        public int Priority;
        
        public OptimizationOpportunity()
        {
            Priority = 5;
        }
    }
    
    [System.Serializable]
    public class ComplianceStatus
    {
        public bool IsCompliant;
        public ComplianceStatusType Status;
        public List<string> Violations = new List<string>();
        public List<string> Recommendations = new List<string>();
        public DateTime LastAssessment;
        public string AssessmentNotes;
        
        public ComplianceStatus()
        {
            IsCompliant = true;
            Status = ComplianceStatusType.Compliant;
            Violations = new List<string>();
            Recommendations = new List<string>();
            LastAssessment = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class FacilityMetrics
    {
        public float TotalFloorArea;
        public float UsableFloorArea;
        public float EnergyEfficiency;
        public float OperationalCost;
        public float ProductionCapacity;
        public float ComplianceScore;
        
        public FacilityMetrics()
        {
            // Initialize with default values
        }
    }
    
    // Placeholder classes for systems referenced in RoomCreationManager
    public class RoomLayoutOptimizer
    {
        public LayoutOptimizationResult OptimizeLayout(Room room, OptimizationCriteria criteria)
        {
            return new LayoutOptimizationResult { IsSuccessful = true, ImprovementPercentage = 15f };
        }
        
        public FacilityOptimizationResult OptimizeFacility(List<Room> rooms, FacilityOptimizationCriteria criteria)
        {
            return new FacilityOptimizationResult { IsSuccessful = true, OverallImprovementPercentage = 20f };
        }
    }
    
    public class EnvironmentalValidator
    {
        public ValidationResult ValidateEnvironmentalRequirements(Room room)
        {
            return new ValidationResult { IsValid = true };
        }
    }
    
    public class RegulatoryComplianceChecker
    {
        public ValidationResult CheckCompliance(Room room)
        {
            return new ValidationResult { IsValid = true };
        }
    }
    
    public class RoomVisualizationSystem
    {
        public void Initialize() { }
        public void Update() { }
    }
    
    public class InteractiveRoomDesigner
    {
        // Room design functionality
    }
    
    public class SmartLayoutGenerator
    {
        // Layout generation functionality
    }
    
    public class EnvironmentalConfigurationWizard
    {
        // Environmental configuration functionality
    }
    
    public class DesignSessionData
    {
        public Dictionary<string, object> SessionSettings = new Dictionary<string, object>();
        public List<string> DesignActions = new List<string>();
        
        public DesignSessionData()
        {
            SessionSettings = new Dictionary<string, object>();
            DesignActions = new List<string>();
        }
    }
    
} // End namespace ProjectChimera.Data.Construction