using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Core.Interfaces;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Construction.Processes
{
    /// <summary>
    /// Construction Process and Workflow Data Structures
    /// Extracted from ConstructionDataStructures.cs - Contains all process, scheduling, 
    /// project management, permits, inspections, and workflow-related functionality
    /// </summary>

    // ===== CONSTRUCTION PROCESS ENUMS =====

    /// <summary>
    /// Construction stages for project progression
    /// </summary>
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

    /// <summary>
    /// Construction phases for high-level project organization
    /// </summary>
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

    /// <summary>
    /// Task status tracking
    /// </summary>
    [System.Serializable]
    public enum TaskStatus
    {
        Not_Started,
        In_Progress,
        Completed,
        On_Hold,
        Cancelled
    }

    /// <summary>
    /// Task priority levels
    /// </summary>
    [System.Serializable]
    public enum TaskPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Project status tracking
    /// </summary>
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

    /// <summary>
    /// Permit types for regulatory compliance
    /// </summary>
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

    /// <summary>
    /// Permit status tracking
    /// </summary>
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

    /// <summary>
    /// Inspection types for quality control
    /// </summary>
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

    /// <summary>
    /// Inspection results
    /// </summary>
    [System.Serializable]
    public enum InspectionResult
    {
        Pending,
        Passed,
        Failed,
        Conditional_Pass,
        Cancelled
    }

    // ===== CORE CONSTRUCTION PROCESS CLASSES =====

    /// <summary>
    /// Individual construction task definition and tracking
    /// </summary>
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

        public ConstructionTask()
        {
            Prerequisites = new List<string>();
            RequiredWorkers = new List<string>();
            RequiredMaterials = new List<MaterialRequirement>();
        }
    }

    /// <summary>
    /// Main construction project container
    /// </summary>
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

        public List<PermitApplication> ApprovedPermits => Permits?.Where(p => p.Status == PermitStatus.Approved).ToList() ?? new List<PermitApplication>();
        public List<PermitApplication> RejectedPermits => Permits?.Where(p => p.Status == PermitStatus.Rejected).ToList() ?? new List<PermitApplication>();
        public List<PermitType> ApprovedPermitTypes = new List<PermitType>();

        public ConstructionProject()
        {
            CompletedPhases = new List<ConstructionPhase>();
            CompletedTasks = new List<string>();
            RequiredPermits = new List<string>();
            Permits = new List<PermitApplication>();
            Tasks = new List<ConstructionTask>();
            PlannedRooms = new List<PlannedRoom>();
            Issues = new List<ConstructionIssue>();
            WorkerAssignments = new List<WorkerAssignment>();
            AssignedWorkers = new List<ConstructionWorker>();
            ApprovedPermitTypes = new List<PermitType>();
        }
    }

    /// <summary>
    /// Construction progress tracking
    /// </summary>
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

        public ConstructionProgress()
        {
            AssignedWorkers = new List<ConstructionWorker>();
            CompletedMilestones = new List<string>();
            Issues = new List<ConstructionIssue>();
        }
    }

    /// <summary>
    /// Task planning logic and management
    /// </summary>
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
            return project.Tasks?.Where(t => t.ConstructionPhase == phase).ToList() ?? new List<ConstructionTask>();
        }

        public void ScheduleTasks(List<ConstructionTask> tasks)
        {
            // Implementation for task scheduling logic
        }

        public List<ConstructionTask> OptimizeTaskSequence(List<ConstructionTask> tasks)
        {
            // Implementation for task sequence optimization
            return tasks;
        }
    }

    /// <summary>
    /// Construction scheduling data and management
    /// </summary>
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
        public List<string> CriticalPath;
        public List<ScheduleMilestone> Milestones;

        public ConstructionSchedule()
        {
            Tasks = new List<ConstructionTask>();
            CriticalPath = new List<string>();
            Milestones = new List<ScheduleMilestone>();
        }

        public void UpdateProgress()
        {
            // Calculate overall progress from tasks
            if (Tasks?.Count > 0)
            {
                OverallProgress = Tasks.Average(t => t.Progress);
            }
        }

        public List<ConstructionTask> GetOverdueTasks()
        {
            return Tasks?.Where(t => t.EndDate < DateTime.Now && t.Status != TaskStatus.Completed).ToList() ?? new List<ConstructionTask>();
        }
    }

    /// <summary>
    /// Permit application tracking and management
    /// </summary>
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

        public DateTime EstimatedProcessingDate => ApplicationDate.AddDays(EstimatedProcessingDays > 0 ? EstimatedProcessingDays : 7); // Default 7 days
        public DateTime SubmittedDate => SubmissionDate;
        public DateTime ApprovalDate;
        public string RejectionReason;

        public PermitApplication()
        {
            RequiredDocuments = new List<string>();
            SubmittedDocuments = new List<string>();
            EstimatedProcessingDays = 7;
        }

        public bool IsExpired()
        {
            return Status == PermitStatus.Expired || 
                   (Status == PermitStatus.Issued && ApprovalDate.AddYears(1) < DateTime.Now);
        }

        public void UpdateStatus(PermitStatus newStatus)
        {
            Status = newStatus;
            if (newStatus == PermitStatus.Approved)
            {
                ApprovalDate = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Quality inspection tracking and management
    /// </summary>
    [System.Serializable]
    public class QualityInspection
    {
        public string InspectionId;
        public string ProjectId;
        public string TaskId;
        public InspectionType Type;
        public DateTime InspectionDate;
        public DateTime ScheduledDate;
        public InspectionResult Result;
        public float QualityScore;
        public List<string> Notes = new List<string>();
        public List<string> DeficienciesFound = new List<string>();
        public List<string> RecommendedActions = new List<string>();
        public string InspectorName;
        public string InspectorId;
        public bool RequiresReinspection;
        public DateTime ReinspectionDate;

        public QualityInspection()
        {
            InspectionId = System.Guid.NewGuid().ToString();
            Notes = new List<string>();
            DeficienciesFound = new List<string>();
            RecommendedActions = new List<string>();
        }

        public bool IsPassed()
        {
            return Result == InspectionResult.Passed || Result == InspectionResult.Conditional_Pass;
        }

        public void ScheduleReinspection(int daysFromNow = 7)
        {
            RequiresReinspection = true;
            ReinspectionDate = DateTime.Now.AddDays(daysFromNow);
        }
    }

    /// <summary>
    /// Comprehensive construction reporting
    /// </summary>
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
            ActiveIssues = new List<string>();
            ResolvedIssues = new List<string>();
            IdentifiedRisks = new List<string>();
            Recommendations = new List<string>();
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

        public void GenerateExecutiveSummary()
        {
            ExecutiveSummary = $"Project {ProjectName} is {GetHealthRating().ToLower()} with {OverallProgress:P0} completion. " +
                              $"Budget utilization: {BudgetUtilization:P0}, Schedule variance: {DelayDays} days.";
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

    // ===== PROCESS MANAGEMENT CLASSES =====

    /// <summary>
    /// Interactive design session management
    /// </summary>
    [System.Serializable]
    public class RoomDesignSession
    {
        public string SessionId;
        public string DesignerId;
        public string ProjectId;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        public bool IsComplete;
        public List<string> RoomIds = new List<string>();
        public DesignSessionData SessionData;
        public List<DesignAction> DesignActions = new List<DesignAction>();
        public CollaborativeSessionStatus CollaborationStatus;
        
        public void Update()
        {
            // Update session logic
            if (IsActive && !IsComplete)
            {
                // Session is ongoing
            }
        }
        
        public RoomDesignSession()
        {
            SessionId = System.Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
            IsActive = true;
            IsComplete = false;
            RoomIds = new List<string>();
            DesignActions = new List<DesignAction>();
            CollaborationStatus = CollaborativeSessionStatus.Active;
        }

        public void CompleteSession()
        {
            IsActive = false;
            IsComplete = true;
            EndTime = DateTime.Now;
            CollaborationStatus = CollaborativeSessionStatus.Completed;
        }

        public void PauseSession()
        {
            IsActive = false;
            CollaborationStatus = CollaborativeSessionStatus.Paused;
        }
    }

    /// <summary>
    /// Facility design tool functionality
    /// </summary>
    [System.Serializable]
    public class FacilityDesignTool
    {
        public GridSnapSettings GridSettings;
        public bool IsDesigning;
        public FacilityTemplate CurrentTemplate;
        public List<GameObject> PreviewObjects;
        public DesignMode CurrentMode;
        public bool ShowGuidelines;
        public bool SnapToGrid;
        
        public FacilityDesignTool(GridSnapSettings gridSettings)
        {
            GridSettings = gridSettings;
            PreviewObjects = new List<GameObject>();
            CurrentMode = DesignMode.Selection;
            ShowGuidelines = true;
            SnapToGrid = true;
        }

        public void StartDesign(FacilityTemplate template)
        {
            IsDesigning = true;
            CurrentTemplate = template;
            CurrentMode = DesignMode.Design;
            // Logic to enter design mode
        }

        public void EndDesign()
        {
            IsDesigning = false;
            CurrentMode = DesignMode.Selection;
            ClearPreviews();
            // Logic to exit design mode
        }

        public GameObject CreateRoomPreview(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            // Logic to create a visual preview of a room
            var preview = new GameObject("RoomPreview");
            PreviewObjects.Add(preview);
            return preview;
        }

        private void ClearPreviews()
        {
            foreach (var preview in PreviewObjects)
            {
                if (preview != null)
                {
                    GameObject.Destroy(preview);
                }
            }
            PreviewObjects.Clear();
        }

        public ValidationResult ValidateDesign()
        {
            return new ValidationResult { IsValid = true };
        }
    }

    /// <summary>
    /// Building validation logic
    /// </summary>
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
            
            // Site validation logic
            if (buildingSite.y < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Building site cannot be underground.");
            }

            // Check minimum setback requirements
            if (Settings?.MinSetbackDistance > 0)
            {
                // Setback validation logic
            }

            // Check zoning compliance
            if (Settings?.EnforceZoningLaws == true)
            {
                // Zoning validation logic
            }

            return result;
        }

        public ValidationResult ValidateRoomPlacement(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };
            
            // Room placement validation
            if (roomTemplate != null)
            {
                // Check minimum room size
                if (Settings?.MinRoomSize != null)
                {
                    if (roomTemplate.Width < Settings.MinRoomSize.x || roomTemplate.Length < Settings.MinRoomSize.z)
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Room {roomTemplate.RoomName} is below minimum size requirements.");
                    }
                }
            }

            return result;
        }

        public ValidationResult ValidateProject(ConstructionProject project)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>(), Warnings = new List<string>() };

            // Project-level validation
            if (project.Tasks?.Count == 0)
            {
                result.Warnings.Add("Project has no tasks defined.");
            }

            if (project.TotalBudget <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Project must have a positive budget.");
            }

            return result;
        }
    }

    /// <summary>
    /// Session management data
    /// </summary>
    [System.Serializable]
    public class DesignSessionData
    {
        public Dictionary<string, object> SessionSettings = new Dictionary<string, object>();
        public List<string> DesignActions = new List<string>();
        public List<DesignSnapshot> Snapshots = new List<DesignSnapshot>();
        public int CurrentSnapshotIndex;
        public bool AutoSaveEnabled;
        public int AutoSaveIntervalMinutes;
        
        public DesignSessionData()
        {
            SessionSettings = new Dictionary<string, object>();
            DesignActions = new List<string>();
            Snapshots = new List<DesignSnapshot>();
            CurrentSnapshotIndex = -1;
            AutoSaveEnabled = true;
            AutoSaveIntervalMinutes = 5;
        }

        public void SaveSnapshot(string description)
        {
            var snapshot = new DesignSnapshot
            {
                Description = description,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>(SessionSettings)
            };
            Snapshots.Add(snapshot);
            CurrentSnapshotIndex = Snapshots.Count - 1;
        }

        public bool RestoreSnapshot(int index)
        {
            if (index >= 0 && index < Snapshots.Count)
            {
                SessionSettings = new Dictionary<string, object>(Snapshots[index].Data);
                CurrentSnapshotIndex = index;
                return true;
            }
            return false;
        }
    }

    // ===== SUPPORTING ENUMS AND CLASSES =====

    /// <summary>
    /// Worker specialty types
    /// </summary>
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

    /// <summary>
    /// Project types
    /// </summary>
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

    /// <summary>
    /// Collaborative session status
    /// </summary>
    [System.Serializable]
    public enum CollaborativeSessionStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Design mode enumeration
    /// </summary>
    [System.Serializable]
    public enum DesignMode
    {
        Selection,
        Design,
        Preview,
        Edit,
        Measurement
    }

    /// <summary>
    /// Complexity levels for projects
    /// </summary>
    [System.Serializable]
    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex,
        Very_Complex,
        Extreme
    }

    /// <summary>
    /// Issue types for construction problems
    /// </summary>
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

    /// <summary>
    /// Issue severity levels
    /// </summary>
    [System.Serializable]
    public enum IssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Issue categories
    /// </summary>
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

    /// <summary>
    /// Issue status tracking
    /// </summary>
    [System.Serializable]
    public enum IssueStatus
    {
        Open,
        In_Progress,
        Resolved,
        Closed,
        Escalated
    }

    /// <summary>
    /// Worker status tracking
    /// </summary>
    [System.Serializable]
    public enum WorkerStatus
    {
        Assigned,
        Active,
        On_Break,
        Completed,
        Reassigned
    }

    /// <summary>
    /// Material requirement definition
    /// </summary>
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

        public MaterialRequirement()
        {
            RequiredDate = DateTime.Now.AddDays(7); // Default requirement
            Status = MaterialStatus.Planning;
        }
    }

    /// <summary>
    /// Construction worker definition
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

        [Header("Certifications")]
        public List<string> Certifications;
        public List<string> Licenses;
        public DateTime LastSafetyTraining;

        public ConstructionWorker()
        {
            WorkerId = System.Guid.NewGuid().ToString();
            Certifications = new List<string>();
            Licenses = new List<string>();
            EfficiencyMultiplier = 1.0f;
            ProductivityModifier = 1.0f;
            IsAvailable = true;
        }
    }

    /// <summary>
    /// Construction issue tracking
    /// </summary>
    [System.Serializable]
    public class ConstructionIssue
    {
        public string IssueId;
        public string ProjectId;
        public string TaskId;
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
        public List<string> AffectedTasks;

        public ConstructionIssue()
        {
            IssueId = System.Guid.NewGuid().ToString();
            ReportedDate = DateTime.Now;
            Status = IssueStatus.Open;
            AffectedTasks = new List<string>();
        }

        public bool IsResolved()
        {
            return Status == IssueStatus.Resolved || Status == IssueStatus.Closed;
        }

        public void Resolve(string resolutionDescription, string resolvedBy)
        {
            Status = IssueStatus.Resolved;
            ResolutionDescription = resolutionDescription;
            ResolvedBy = resolvedBy;
            ResolvedDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Worker assignment tracking
    /// </summary>
    [System.Serializable]
    public class WorkerAssignment
    {
        public string AssignmentId;
        public string WorkerId;
        public string WorkerName;
        public string ProjectId;
        public string TaskId;
        public WorkerSpecialty Specialty;
        public DateTime StartDate;
        public DateTime EndDate;
        public float HoursPerDay;
        public float HourlyRate;
        public string TaskDescription;
        public float CompletionPercentage;
        public WorkerStatus Status;

        public WorkerAssignment()
        {
            AssignmentId = System.Guid.NewGuid().ToString();
            StartDate = DateTime.Now;
            Status = WorkerStatus.Assigned;
        }
    }

    /// <summary>
    /// Schedule milestone tracking
    /// </summary>
    [System.Serializable]
    public class ScheduleMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public DateTime PlannedDate;
        public DateTime ActualDate;
        public bool IsCompleted;
        public List<string> RequiredTasks;
        public float CompletionPercentage;

        public ScheduleMilestone()
        {
            MilestoneId = System.Guid.NewGuid().ToString();
            RequiredTasks = new List<string>();
        }
    }

    /// <summary>
    /// Design action tracking
    /// </summary>
    [System.Serializable]
    public class DesignAction
    {
        public string ActionId;
        public string ActionType;
        public DateTime Timestamp;
        public string Description;
        public Dictionary<string, object> Parameters;

        public DesignAction()
        {
            ActionId = System.Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Parameters = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Design snapshot for undo/redo functionality
    /// </summary>
    [System.Serializable]
    public class DesignSnapshot
    {
        public string SnapshotId;
        public string Description;
        public DateTime Timestamp;
        public Dictionary<string, object> Data;

        public DesignSnapshot()
        {
            SnapshotId = System.Guid.NewGuid().ToString();
            Data = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Construction metrics for reporting
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

        public ConstructionMetrics()
        {
            LastUpdated = DateTime.Now;
            ConstructionEfficiency = 1.0f;
            AverageQualityScore = 80f;
            ScheduleAdherence = 0.85f;
            WorkerProductivity = 1.0f;
        }

        public List<ConstructionWorker> GetActiveWorkers()
        {
            return new List<ConstructionWorker>();
        }

        public void UpdateMetrics()
        {
            LastUpdated = DateTime.Now;
            // Recalculate metrics
        }
    }

    // ===== PLACEHOLDER CLASSES FOR REFERENCED TYPES =====

    /// <summary>
    /// Placeholder enums and classes referenced in the process system
    /// </summary>
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
    public enum LightSpectrum
    {
        FullSpectrum,
        BlueHeavy,
        RedHeavy,
        Vegetative,
        Flowering,
        Custom
    }

    /// <summary>
    /// Placeholder classes for construction system integration
    /// </summary>
    [System.Serializable]
    public class FacilityTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public string Description;
        public Vector2 Dimensions;
        public float TotalArea;
        public List<ConstructionRoomTemplate> RoomTemplates;

        public FacilityTemplate()
        {
            RoomTemplates = new List<ConstructionRoomTemplate>();
        }
    }

    [System.Serializable]
    public class ConstructionRoomTemplate
    {
        public string TemplateRoomId;
        public string RoomName;
        public string RoomType;
        public string Description;
        public Vector2 Dimensions;
        public float Area;
        public float Height;
        public float Length;
        public float Width;
        public float EstimatedCost;

        public ConstructionRoomTemplate()
        {
            TemplateRoomId = System.Guid.NewGuid().ToString();
        }
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

        public PlannedRoom()
        {
            PlannedRoomId = System.Guid.NewGuid().ToString();
        }
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
    public class ConstructionSettings
    {
        [Header("Construction Configuration")]
        public bool EnableConstruction = true;
        public bool EnforceZoningLaws = true;
        public bool RequirePermits = true;
        public float ConstructionSpeedMultiplier = 1.0f;

        [Header("Building Constraints")]
        public Vector3 MaxRoomSize = new Vector3(50, 5, 50);
        public Vector3 MinRoomSize = new Vector3(3, 2.5f, 3);
        public float WallThickness = 0.15f;
        public bool RequireFoundation = true;
        public bool EnforceFireSafety = true;
        public bool RequireVentilation = true;
        public float MaxBuildingHeight = 15f;
        public float MinSetbackDistance = 3f;
    }

    [System.Serializable]
    public class GridSnapSettings
    {
        public float GridSize = 1.0f;
        public bool SnapToGrid = true;
        public bool ShowGrid = true;
        public Color GridColor = Color.gray;
        public Vector3 MinRoomSize = new Vector3(3, 2.5f, 3);
        public Vector3 MaxRoomSize = new Vector3(50, 5, 50);
    }

    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid = true;
        public string ErrorMessage;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();

        public ValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }
    }

} // End namespace ProjectChimera.Data.Construction.Processes