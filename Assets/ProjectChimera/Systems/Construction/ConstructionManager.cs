using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Construction;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Comprehensive construction management system for Project Chimera.
    /// Handles facility planning, construction scheduling, resource management,
    /// quality control, and compliance tracking for cannabis cultivation facilities.
    /// </summary>
    public class ConstructionManager : ChimeraManager
    {
        [Header("Construction Configuration")]
        [SerializeField] private bool _enableDetailedScheduling = true;
        [SerializeField] private bool _enableQualityControl = true;
        [SerializeField] private bool _enablePermitTracking = true;
        [SerializeField] private bool _enableCostTracking = true;
        [SerializeField] private float _dailyUpdateInterval = 86400f; // 24 hours
        [SerializeField] private float _qualityThreshold = 80f; // 0-100
        
        [Header("Economic Settings")]
        [SerializeField] private float _laborCostMultiplier = 1.0f;
        [SerializeField] private float _materialCostMultiplier = 1.0f;
        [SerializeField] private float _permitCostMultiplier = 1.0f;
        [SerializeField] private float _contingencyPercentage = 10f; // 10% contingency
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onProjectStarted;
        [SerializeField] private SimpleGameEventSO _onProjectCompleted;
        [SerializeField] private SimpleGameEventSO _onMilestoneReached;
        [SerializeField] private SimpleGameEventSO _onIssueReported;
        [SerializeField] private SimpleGameEventSO _onInspectionCompleted;
        [SerializeField] private SimpleGameEventSO _onBudgetExceeded;
        
        // Core Construction Data
        private List<BuildingProject> _activeProjects = new List<BuildingProject>();
        private Dictionary<string, BuildingBlueprint> _availableBlueprints = new Dictionary<string, BuildingBlueprint>();
        private List<ConstructionWorker> _availableWorkers = new List<ConstructionWorker>();
        private Dictionary<string, MaterialSupplier> _suppliers = new Dictionary<string, MaterialSupplier>();
        
        // Scheduling and Progress
        private Dictionary<string, ConstructionSchedule> _projectSchedules = new Dictionary<string, ConstructionSchedule>();
        private List<ConstructionTask> _activeTasks = new List<ConstructionTask>();
        private Dictionary<string, List<ConstructionIssue>> _projectIssues = new Dictionary<string, List<ConstructionIssue>>();
        
        // Quality and Compliance
        private Dictionary<string, QualityMetrics> _projectQuality = new Dictionary<string, QualityMetrics>();
        private List<ConstructionInspection> _pendingInspections = new List<ConstructionInspection>();
        private Dictionary<string, List<ConstructionPermit>> _projectPermits = new Dictionary<string, List<ConstructionPermit>>();
        
        // Performance Tracking
        private float _lastDailyUpdate = 0f;
        private ConstructionMetrics _overallMetrics = new ConstructionMetrics();
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public List<BuildingProject> ActiveProjects => _activeProjects;
        public List<BuildingBlueprint> AvailableBlueprints => _availableBlueprints.Values.ToList();
        public List<ConstructionWorker> AvailableWorkers => _availableWorkers.Where(w => w.IsAvailable).ToList();
        public ConstructionMetrics OverallMetrics => _overallMetrics;
        public float TotalActiveBudget => _activeProjects.Sum(p => p.TotalBudget);
        public float TotalSpentAmount => _activeProjects.Sum(p => p.SpentAmount);
        public int ActiveProjectCount => _activeProjects.Count(p => p.CurrentStage != ConstructionStage.Completed);
        
        // Events
        public System.Action<BuildingProject> OnProjectStarted;
        public System.Action<BuildingProject> OnProjectCompleted;
        public System.Action<BuildingProject, string> OnMilestoneReached;
        public System.Action<ConstructionIssue> OnIssueReported;
        public System.Action<ConstructionInspection> OnInspectionCompleted;
        public System.Action<BuildingProject, float> OnBudgetExceeded;
        
        protected override void OnManagerInitialize()
        {
            InitializeBlueprints();
            InitializeWorkers();
            InitializeSuppliers();
            InitializePermitSystem();
            LoadSampleProjects();
            
            LogInfo("ConstructionManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Daily updates for construction progress
            if (currentTime - _lastDailyUpdate >= _dailyUpdateInterval)
            {
                ProcessDailyUpdates();
                _lastDailyUpdate = currentTime;
            }
            
            // Update active projects
            UpdateActiveProjects();
            
            // Process pending inspections
            ProcessPendingInspections();
            
            // Update construction metrics
            UpdateConstructionMetrics();
            
            // Check for issues and alerts
            CheckForIssuesAndAlerts();
        }
        
        #region Project Management
        
        /// <summary>
        /// Start a new construction project
        /// </summary>
        public string StartConstructionProject(string blueprintId, Vector3 location, BuildingQuality quality = BuildingQuality.Standard)
        {
            if (!_availableBlueprints.TryGetValue(blueprintId, out var blueprint))
            {
                LogWarning($"Blueprint not found: {blueprintId}");
                return null;
            }
            
            string projectId = Guid.NewGuid().ToString();
            var project = new BuildingProject
            {
                ProjectId = projectId,
                ProjectName = $"{blueprint.Name} - {DateTime.Now:yyyy-MM-dd}",
                Description = blueprint.Description,
                BuildingType = blueprint.BuildingType,
                QualityLevel = quality,
                Blueprint = blueprint,
                PlannedLocation = location,
                PlannedDimensions = blueprint.Dimensions,
                PlannedArea = blueprint.TotalArea,
                CurrentStage = ConstructionStage.Planning,
                OverallProgress = 0f,
                ProjectStartDate = DateTime.Now,
                EstimatedCompletionDate = DateTime.Now.AddDays(blueprint.EstimatedDays),
                TotalBudget = CalculateProjectBudget(blueprint, quality),
                SpentAmount = 0f,
                Priority = ConstructionPriority.Normal,
                Issues = new List<ConstructionIssue>(),
                CompletedMilestones = new List<string>(),
                IsOnSchedule = true,
                IsOnBudget = true
            };
            
            // Calculate budget breakdown
            project.RemainingBudget = project.TotalBudget;
            
            // Initialize requirements
            project.Requirements = GenerateProjectRequirements(blueprint);
            project.MaterialRequirements = GenerateMaterialRequirements(blueprint);
            project.RequiredPermits = GenerateRequiredPermits(blueprint);
            
            // Initialize quality metrics
            _projectQuality[projectId] = new QualityMetrics
            {
                StructuralQuality = 100f,
                FinishQuality = 100f,
                SystemsQuality = 100f,
                SafetyCompliance = 100f,
                EnvironmentalCompliance = 100f,
                OverallQuality = 100f
            };
            
            // Create construction schedule
            var schedule = CreateConstructionSchedule(project);
            _projectSchedules[projectId] = schedule;
            
            _activeProjects.Add(project);
            _projectIssues[projectId] = new List<ConstructionIssue>();
            _projectPermits[projectId] = new List<ConstructionPermit>(project.RequiredPermits);
            
            _onProjectStarted?.Raise();
            OnProjectStarted?.Invoke(project);
            
            LogInfo($"Started construction project: {project.ProjectName} (Budget: ${project.TotalBudget:F0})");
            return projectId;
        }
        
        /// <summary>
        /// Get project by ID
        /// </summary>
        public BuildingProject GetProject(string projectId)
        {
            return _activeProjects.FirstOrDefault(p => p.ProjectId == projectId);
        }
        
        /// <summary>
        /// Assign workers to a project
        /// </summary>
        public bool AssignWorkersToProject(string projectId, List<string> workerIds, DateTime startDate, DateTime endDate)
        {
            var project = GetProject(projectId);
            if (project == null) return false;
            
            foreach (var workerId in workerIds)
            {
                var worker = _availableWorkers.FirstOrDefault(w => w.WorkerId == workerId && w.IsAvailable);
                if (worker == null) continue;
                
                var assignment = new WorkerAssignment
                {
                    AssignmentId = Guid.NewGuid().ToString(),
                    WorkerId = workerId,
                    WorkerName = worker.Name,
                    Specialty = worker.Specialty,
                    StartDate = startDate,
                    EndDate = endDate,
                    HoursPerDay = 8f,
                    HourlyRate = worker.HourlyRate,
                    TaskDescription = $"Construction work on {project.ProjectName}",
                    CompletionPercentage = 0f,
                    Status = WorkerStatus.Assigned
                };
                
                project.WorkerAssignments.Add(assignment);
                worker.IsAvailable = false;
                worker.CurrentProjectId = projectId;
            }
            
            LogInfo($"Assigned {workerIds.Count} workers to project: {project.ProjectName}");
            return true;
        }
        
        /// <summary>
        /// Advance project to next construction stage
        /// </summary>
        public bool AdvanceProjectStage(string projectId)
        {
            var project = GetProject(projectId);
            if (project == null) return false;
            
            // Check if current stage requirements are met
            if (!CanAdvanceToNextStage(project))
            {
                LogWarning($"Cannot advance project stage - requirements not met: {project.ProjectName}");
                return false;
            }
            
            var currentStage = project.CurrentStage;
            var nextStage = GetNextConstructionStage(currentStage);
            
            if (nextStage == currentStage)
            {
                // Project is complete
                CompleteProject(projectId);
                return true;
            }
            
            project.CurrentStage = nextStage;
            project.CompletedMilestones.Add(currentStage.ToString());
            
            // Update progress
            project.OverallProgress = CalculateProjectProgress(project);
            
            // Schedule required inspections for new stage
            ScheduleStageInspections(project, nextStage);
            
            _onMilestoneReached?.Raise();
            OnMilestoneReached?.Invoke(project, nextStage.ToString());
            
            LogInfo($"Advanced project stage: {project.ProjectName} -> {nextStage}");
            return true;
        }
        
        /// <summary>
        /// Report a construction issue
        /// </summary>
        public string ReportIssue(string projectId, string title, string description, IssueSeverity severity, IssueCategory category)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            string issueId = Guid.NewGuid().ToString();
            var issue = new ConstructionIssue
            {
                IssueId = issueId,
                Title = title,
                Description = description,
                Severity = severity,
                Category = category,
                ReportedDate = DateTime.Now,
                ReportedBy = "System", // In real implementation, would use current user
                Status = IssueStatus.Open,
                CostImpact = EstimateIssueCostImpact(severity, category),
                DelayDays = EstimateIssueDelayImpact(severity, category)
            };
            
            project.Issues.Add(issue);
            _projectIssues[projectId].Add(issue);
            
            // Update project status
            if (severity >= IssueSeverity.High)
            {
                project.IsOnSchedule = false;
                if (issue.CostImpact > 0)
                {
                    project.IsOnBudget = false;
                }
            }
            
            _onIssueReported?.Raise();
            OnIssueReported?.Invoke(issue);
            
            LogInfo($"Reported {severity} issue: {title} on project {project.ProjectName}");
            return issueId;
        }
        
        #endregion
        
        #region Quality Control
        
        /// <summary>
        /// Perform quality inspection
        /// </summary>
        public string PerformQualityInspection(string projectId, string area, string inspectorName)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            string inspectionId = Guid.NewGuid().ToString();
            var inspection = new QualityInspection
            {
                InspectionId = inspectionId,
                Area = area,
                InspectionDate = DateTime.Now,
                InspectorName = inspectorName,
                QualityScore = UnityEngine.Random.Range(70f, 100f), // Simulated score
                Defects = new List<QualityDefect>(),
                PassedChecks = new List<string>(),
                OverallNotes = $"Quality inspection of {area}"
            };
            
            // Simulate defect detection
            if (inspection.QualityScore < _qualityThreshold)
            {
                int defectCount = UnityEngine.Random.Range(1, 4);
                for (int i = 0; i < defectCount; i++)
                {
                    inspection.Defects.Add(GenerateRandomDefect(area));
                }
            }
            
            // Update project quality metrics
            if (_projectQuality.TryGetValue(projectId, out var metrics))
            {
                UpdateQualityMetrics(metrics, inspection);
                metrics.OverallQuality = (metrics.StructuralQuality + metrics.FinishQuality + 
                                        metrics.SystemsQuality + metrics.SafetyCompliance + 
                                        metrics.EnvironmentalCompliance) / 5f;
            }
            
            LogInfo($"Quality inspection completed: {area} - Score: {inspection.QualityScore:F1}");
            return inspectionId;
        }
        
        /// <summary>
        /// Get project quality metrics
        /// </summary>
        public QualityMetrics GetProjectQuality(string projectId)
        {
            return _projectQuality.TryGetValue(projectId, out var metrics) ? metrics : null;
        }
        
        #endregion
        
        #region Permit and Compliance
        
        /// <summary>
        /// Apply for construction permit
        /// </summary>
        public string ApplyForPermit(string projectId, PermitType permitType, string description)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            string permitId = Guid.NewGuid().ToString();
            var permit = new ConstructionPermit
            {
                PermitId = permitId,
                Type = permitType,
                Description = description,
                IssuingAuthority = GetPermitAuthority(permitType),
                ApplicationDate = DateTime.Now,
                Cost = CalculatePermitCost(permitType, project),
                Status = PermitStatus.Application_Submitted,
                Conditions = new List<string>(),
                RequiredDocuments = GetRequiredDocuments(permitType)
            };
            
            // Simulate permit processing time
            permit.IssuedDate = DateTime.Now.AddDays(GetPermitProcessingDays(permitType));
            permit.ExpirationDate = permit.IssuedDate.AddYears(1);
            
            _projectPermits[projectId].Add(permit);
            
            // Deduct permit cost from budget
            project.SpentAmount += permit.Cost;
            project.RemainingBudget -= permit.Cost;
            
            LogInfo($"Applied for {permitType} permit: {project.ProjectName} (Cost: ${permit.Cost:F0})");
            return permitId;
        }
        
        /// <summary>
        /// Schedule construction inspection
        /// </summary>
        public string ScheduleInspection(string projectId, InspectionType inspectionType, DateTime scheduledDate)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            string inspectionId = Guid.NewGuid().ToString();
            var inspection = new ConstructionInspection
            {
                InspectionId = inspectionId,
                Type = inspectionType,
                Description = $"{inspectionType} inspection for {project.ProjectName}",
                ScheduledDate = scheduledDate,
                InspectorName = GetRandomInspectorName(),
                Result = InspectionResult.Pending,
                PassedItems = new List<string>(),
                FailedItems = new List<string>(),
                Notes = new List<string>()
            };
            
            _pendingInspections.Add(inspection);
            project.Inspections.Add(inspection);
            
            LogInfo($"Scheduled {inspectionType} inspection for {scheduledDate:yyyy-MM-dd}");
            return inspectionId;
        }
        
        #endregion
        
        #region Resource Management
        
        /// <summary>
        /// Order materials for project
        /// </summary>
        public bool OrderMaterials(string projectId, List<string> materialIds, string supplierId)
        {
            var project = GetProject(projectId);
            if (project == null) return false;
            
            if (!_suppliers.TryGetValue(supplierId, out var supplier))
            {
                LogWarning($"Supplier not found: {supplierId}");
                return false;
            }
            
            float totalCost = 0f;
            foreach (var materialId in materialIds)
            {
                var material = project.MaterialRequirements.FirstOrDefault(m => m.MaterialId == materialId);
                if (material == null) continue;
                
                material.OrderedQuantity = material.RequiredQuantity - material.AvailableQuantity;
                material.Supplier = supplier.Name;
                material.OrderDate = DateTime.Now;
                material.ExpectedDelivery = DateTime.Now.AddDays(supplier.DeliveryDays);
                material.Status = MaterialStatus.Ordered;
                
                float orderCost = material.OrderedQuantity * material.UnitCost * _materialCostMultiplier;
                totalCost += orderCost;
            }
            
            // Deduct cost from budget
            project.SpentAmount += totalCost;
            project.RemainingBudget -= totalCost;
            
            if (project.RemainingBudget < 0)
            {
                project.IsOnBudget = false;
                _onBudgetExceeded?.Raise();
                OnBudgetExceeded?.Invoke(project, Math.Abs(project.RemainingBudget));
            }
            
            LogInfo($"Ordered materials from {supplier.Name}: ${totalCost:F0}");
            return true;
        }
        
        /// <summary>
        /// Get available workers by specialty
        /// </summary>
        public List<ConstructionWorker> GetWorkersBySpecialty(WorkerSpecialty specialty)
        {
            return _availableWorkers.Where(w => w.Specialty == specialty && w.IsAvailable).ToList();
        }
        
        /// <summary>
        /// Get project cost breakdown
        /// </summary>
        public ProjectCostBreakdown GetProjectCostBreakdown(string projectId)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            return new ProjectCostBreakdown
            {
                ProjectId = projectId,
                TotalBudget = project.TotalBudget,
                SpentAmount = project.SpentAmount,
                RemainingBudget = project.RemainingBudget,
                LaborCosts = CalculateLaborCosts(project),
                MaterialCosts = CalculateMaterialCosts(project),
                PermitCosts = CalculatePermitCosts(projectId),
                OverheadCosts = project.TotalBudget * 0.15f, // 15% overhead
                ContingencyCosts = project.TotalBudget * (_contingencyPercentage / 100f)
            };
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeBlueprints()
        {
            // Create sample blueprints for different building types
            CreateSampleBlueprint(BuildingType.GrowRoom, "Standard Grow Room", 100000f, 30);
            CreateSampleBlueprint(BuildingType.Greenhouse, "Commercial Greenhouse", 250000f, 60);
            CreateSampleBlueprint(BuildingType.ProcessingFacility, "Processing Facility", 300000f, 45);
            CreateSampleBlueprint(BuildingType.StorageWarehouse, "Storage Warehouse", 150000f, 25);
            CreateSampleBlueprint(BuildingType.LaboratoryFacility, "Testing Laboratory", 200000f, 40);
        }
        
        private void CreateSampleBlueprint(BuildingType type, string name, float cost, int days)
        {
            string blueprintId = Guid.NewGuid().ToString();
            var blueprint = new BuildingBlueprint
            {
                BlueprintId = blueprintId,
                Name = name,
                Description = $"Professional {name.ToLower()} designed for cannabis cultivation",
                BuildingType = type,
                ArchitectName = "Cannabis Facility Designs Inc.",
                CreatedDate = DateTime.Now,
                Version = "1.0",
                Dimensions = GetStandardDimensions(type),
                EstimatedCost = cost,
                EstimatedDays = days,
                CannabisSuitability = true,
                Rooms = new List<RoomLayout>(),
                MaterialSpecs = new List<MaterialSpecification>(),
                EquipmentSpecs = new List<EquipmentSpecification>(),
                BuildingCodes = new List<string> { "IBC 2021", "NFPA 70", "Local Cannabis Regulations" }
            };
            
            blueprint.TotalArea = blueprint.Dimensions.x * blueprint.Dimensions.z;
            blueprint.LaborCostEstimate = cost * 0.4f; // 40% labor
            blueprint.MaterialCostEstimate = cost * 0.45f; // 45% materials
            blueprint.PermitCostEstimate = cost * 0.05f; // 5% permits
            
            _availableBlueprints[blueprintId] = blueprint;
        }
        
        private Vector3 GetStandardDimensions(BuildingType type)
        {
            return type switch
            {
                BuildingType.GrowRoom => new Vector3(12f, 3f, 8f), // 12x8 room, 3m height
                BuildingType.Greenhouse => new Vector3(30f, 5f, 20f), // 30x20 greenhouse
                BuildingType.ProcessingFacility => new Vector3(20f, 4f, 15f),
                BuildingType.StorageWarehouse => new Vector3(25f, 6f, 20f),
                BuildingType.LaboratoryFacility => new Vector3(15f, 3f, 10f),
                _ => new Vector3(10f, 3f, 10f)
            };
        }
        
        private void InitializeWorkers()
        {
            var specialties = Enum.GetValues(typeof(WorkerSpecialty)).Cast<WorkerSpecialty>();
            foreach (var specialty in specialties)
            {
                for (int i = 0; i < 3; i++) // 3 workers per specialty
                {
                    _availableWorkers.Add(new ConstructionWorker
                    {
                        WorkerId = Guid.NewGuid().ToString(),
                        Name = $"{specialty} {i + 1}",
                        Specialty = specialty,
                        HourlyRate = GetWorkerHourlyRate(specialty),
                        SkillLevel = (SkillLevel)UnityEngine.Random.Range(1, 5), // 1-4 skill level (Apprentice to Master)
                        ProductivityModifier = UnityEngine.Random.Range(0.8f, 1.3f),
                        QualityRating = UnityEngine.Random.Range(3.0f, 5.0f),
                        IsAvailable = true,
                        ExperienceYears = UnityEngine.Random.Range(1, 15)
                    });
                }
            }
        }
        
        private float GetWorkerHourlyRate(WorkerSpecialty specialty)
        {
            return specialty switch
            {
                WorkerSpecialty.GeneralLabor => 20f,
                WorkerSpecialty.Electrician => 45f,
                WorkerSpecialty.Plumber => 40f,
                WorkerSpecialty.HVAC_Technician => 42f,
                WorkerSpecialty.Carpenter => 35f,
                WorkerSpecialty.Equipment_Installer => 38f,
                WorkerSpecialty.Project_Manager => 60f,
                WorkerSpecialty.Inspector => 50f,
                _ => 25f
            };
        }
        
        private void InitializeSuppliers()
        {
            _suppliers["supplier1"] = new MaterialSupplier
            {
                SupplierId = "supplier1",
                Name = "Cannabis Construction Supply Co.",
                DeliveryDays = 7,
                ReliabilityRating = 4.5f,
                PriceRating = 3.8f,
                QualityRating = 4.2f
            };
            
            _suppliers["supplier2"] = new MaterialSupplier
            {
                SupplierId = "supplier2",
                Name = "Professional Grow Equipment Ltd.",
                DeliveryDays = 5,
                ReliabilityRating = 4.8f,
                PriceRating = 3.2f,
                QualityRating = 4.7f
            };
        }
        
        private void InitializePermitSystem()
        {
            // Initialize permit processing system
        }
        
        private void LoadSampleProjects()
        {
            // Load any existing projects from save data
        }
        
        private float CalculateProjectBudget(BuildingBlueprint blueprint, BuildingQuality quality)
        {
            float baseCost = blueprint.EstimatedCost;
            float qualityMultiplier = quality switch
            {
                BuildingQuality.Basic => 0.8f,
                BuildingQuality.Standard => 1.0f,
                BuildingQuality.Premium => 1.3f,
                BuildingQuality.Luxury => 1.6f,
                BuildingQuality.Industrial => 1.1f,
                _ => 1.0f
            };
            
            float totalCost = baseCost * qualityMultiplier;
            totalCost += totalCost * (_contingencyPercentage / 100f); // Add contingency
            
            return totalCost;
        }
        
        private List<BuildingRequirement> GenerateProjectRequirements(BuildingBlueprint blueprint)
        {
            var requirements = new List<BuildingRequirement>();
            
            // Add standard requirements based on building type
            if (blueprint.BuildingType == BuildingType.GrowRoom || blueprint.BuildingType == BuildingType.Greenhouse)
            {
                requirements.Add(new BuildingRequirement
                {
                    RequirementId = Guid.NewGuid().ToString(),
                    Description = "Cannabis cultivation license compliance",
                    Category = "Legal",
                    Priority = RequirementPriority.Critical,
                    IsMandatory = true,
                    Source = "State Cannabis Regulations"
                });
                
                requirements.Add(new BuildingRequirement
                {
                    RequirementId = Guid.NewGuid().ToString(),
                    Description = "Environmental controls for cannabis cultivation",
                    Category = "Technical",
                    Priority = RequirementPriority.High,
                    IsMandatory = true,
                    Source = "Industry Standards"
                });
            }
            
            return requirements;
        }
        
        private List<MaterialRequirement> GenerateMaterialRequirements(BuildingBlueprint blueprint)
        {
            var materials = new List<MaterialRequirement>();
            
            // Generate material requirements based on blueprint
            float area = blueprint.TotalArea;
            
            materials.Add(new MaterialRequirement
            {
                MaterialId = Guid.NewGuid().ToString(),
                MaterialName = "Structural Steel",
                Type = BuildingMaterial.Steel,
                RequiredQuantity = area * 0.5f, // 0.5 tons per sqm
                Unit = "tons",
                UnitCost = 800f,
                Status = MaterialStatus.Planning
            });
            
            materials.Add(new MaterialRequirement
            {
                MaterialId = Guid.NewGuid().ToString(),
                MaterialName = "Insulation",
                Type = BuildingMaterial.Insulation,
                RequiredQuantity = area * 2f, // 2 sqm insulation per sqm floor
                Unit = "sqm",
                UnitCost = 15f,
                Status = MaterialStatus.Planning
            });
            
            return materials;
        }
        
        private List<ConstructionPermit> GenerateRequiredPermits(BuildingBlueprint blueprint)
        {
            var permits = new List<ConstructionPermit>();
            
            permits.Add(new ConstructionPermit
            {
                PermitId = Guid.NewGuid().ToString(),
                Type = PermitType.Building,
                Description = "General building permit",
                Status = PermitStatus.Not_Applied
            });
            
            permits.Add(new ConstructionPermit
            {
                PermitId = Guid.NewGuid().ToString(),
                Type = PermitType.Electrical,
                Description = "Electrical work permit",
                Status = PermitStatus.Not_Applied
            });
            
            if (blueprint.CannabisSuitability)
            {
                permits.Add(new ConstructionPermit
                {
                    PermitId = Guid.NewGuid().ToString(),
                    Type = PermitType.Cannabis_License,
                    Description = "Cannabis facility construction permit",
                    Status = PermitStatus.Not_Applied
                });
            }
            
            return permits;
        }
        
        private void ProcessDailyUpdates()
        {
            foreach (var project in _activeProjects)
            {
                if (project.CurrentStage == ConstructionStage.Completed) continue;
                
                // Update construction progress
                UpdateProjectProgress(project);
                
                // Process worker assignments
                ProcessWorkerProgress(project);
                
                // Update material deliveries
                ProcessMaterialDeliveries(project);
                
                // Check for issues
                CheckForRandomIssues(project);
            }
        }
        
        private void UpdateActiveProjects()
        {
            foreach (var project in _activeProjects)
            {
                // Update project progress
                UpdateProjectProgress(project);
                
                // Process worker assignments
                ProcessWorkerProgress(project);
                
                // Process material deliveries
                ProcessMaterialDeliveries(project);
                
                // Check for random issues
                CheckForRandomIssues(project);
                
                // Check schedule status
                CheckScheduleStatus();
                
                // Check budget status
                CheckBudgetStatus();
            }
        }
        
        private void ProcessPendingInspections()
        {
            var currentDate = DateTime.Now;
            var dueInspections = _pendingInspections.Where(i => 
                i.ScheduledDate <= currentDate && i.Result == InspectionResult.Pending).ToList();
            
            foreach (var inspection in dueInspections)
            {
                ProcessInspection(inspection);
            }
        }
        
        private void UpdateConstructionMetrics()
        {
            _overallMetrics.TotalProjects = _activeProjects.Count;
            _overallMetrics.ActiveProjects = _activeProjects.Count(p => p.CurrentStage != ConstructionStage.Completed);
            _overallMetrics.CompletedProjects = _activeProjects.Count(p => p.CurrentStage == ConstructionStage.Completed);
            _overallMetrics.TotalValue = _activeProjects.Sum(p => p.TotalBudget);
            _overallMetrics.ActiveWorkers = _availableWorkers.Count(w => !w.IsAvailable);
            _overallMetrics.ConstructionEfficiency = CalculateOnTimeCompletionRate();
            _overallMetrics.AverageCompletionTime = CalculateAverageCompletionTime();
            _overallMetrics.WorkerProductivity = CalculateWorkerProductivity();
            _overallMetrics.LastUpdated = System.DateTime.Now;
        }
        
        /// <summary>
        /// Calculate the percentage of projects completed on time
        /// </summary>
        private float CalculateOnTimeCompletionRate()
        {
            var completedProjects = _activeProjects.Where(p => p.CurrentStage == ConstructionStage.Completed).ToList();
            if (completedProjects.Count == 0) return 1.0f; // 100% if no completed projects yet
            
            var onTimeProjects = completedProjects.Count(p => 
                p.ActualCompletionDate <= p.EstimatedCompletionDate);
            
            return (float)onTimeProjects / completedProjects.Count;
        }
        
        /// <summary>
        /// Calculate the percentage of projects staying within budget
        /// </summary>
        private float CalculateBudgetComplianceRate()
        {
            if (_activeProjects.Count == 0) return 1.0f; // 100% if no projects
            
            var budgetCompliantProjects = _activeProjects.Count(p => p.IsOnBudget);
            return (float)budgetCompliantProjects / _activeProjects.Count;
        }
        
        /// <summary>
        /// Calculate the average completion time for completed projects
        /// </summary>
        private float CalculateAverageCompletionTime()
        {
            var completedProjects = _activeProjects.Where(p => p.CurrentStage == ConstructionStage.Completed).ToList();
            if (completedProjects.Count == 0) return 0f;
            
            var totalDays = completedProjects.Sum(p => (p.ActualCompletionDate - p.ProjectStartDate).TotalDays);
            return (float)(totalDays / completedProjects.Count);
        }
        
        /// <summary>
        /// Calculate overall worker productivity
        /// </summary>
        private float CalculateWorkerProductivity()
        {
            if (_availableWorkers.Count == 0) return 1.0f;
            
            var activeWorkers = _availableWorkers.Where(w => !w.IsAvailable).ToList();
            if (activeWorkers.Count == 0) return 1.0f;
            
            return activeWorkers.Average(w => w.ProductivityModifier);
        }
        
        private void CheckForIssuesAndAlerts()
        {
            foreach (var project in _activeProjects)
            {
                // Check for budget alerts
                if (project.RemainingBudget / project.TotalBudget < 0.1f && project.IsOnBudget)
                {
                    ReportIssue(project.ProjectId, "Budget Alert", 
                        "Project budget is running low", IssueSeverity.High, IssueCategory.Budget);
                }
                
                // Check for schedule alerts
                var daysRemaining = (project.EstimatedCompletionDate - DateTime.Now).TotalDays;
                var progressExpected = 1.0f - (float)(daysRemaining / (project.EstimatedCompletionDate - project.ProjectStartDate).TotalDays);
                
                if (project.OverallProgress < progressExpected - 0.1f && project.IsOnSchedule)
                {
                    ReportIssue(project.ProjectId, "Schedule Alert", 
                        "Project is falling behind schedule", IssueSeverity.Medium, IssueCategory.Schedule);
                }
            }
        }
        
        private float CalculateProjectProgress(BuildingProject project)
        {
            // Calculate progress based on completed stages
            int totalStages = Enum.GetValues(typeof(ConstructionStage)).Length - 1; // Exclude Completed
            int currentStageIndex = (int)project.CurrentStage;
            
            return (float)currentStageIndex / totalStages;
        }
        
        private bool CanAdvanceToNextStage(BuildingProject project)
        {
            // Check if all requirements for current stage are met
            switch (project.CurrentStage)
            {
                case ConstructionStage.Planning:
                    return project.RequiredPermits.Any(p => p.Status == PermitStatus.Issued);
                
                case ConstructionStage.Foundation:
                    return project.Inspections.Any(i => i.Type == InspectionType.Foundation && i.Result == InspectionResult.Passed);
                
                case ConstructionStage.Framing:
                    return project.Inspections.Any(i => i.Type == InspectionType.Framing && i.Result == InspectionResult.Passed);
                
                default:
                    return true; // Simplified for other stages
            }
        }
        
        private ConstructionStage GetNextConstructionStage(ConstructionStage current)
        {
            var stages = Enum.GetValues(typeof(ConstructionStage)).Cast<ConstructionStage>().ToArray();
            int currentIndex = Array.IndexOf(stages, current);
            
            if (currentIndex >= 0 && currentIndex < stages.Length - 1)
            {
                return stages[currentIndex + 1];
            }
            
            return current; // Already at final stage
        }
        
        private void CompleteProject(string projectId)
        {
            var project = GetProject(projectId);
            if (project == null) return;
            
            project.CurrentStage = ConstructionStage.Completed;
            project.ActualCompletionDate = DateTime.Now;
            project.OverallProgress = 1.0f;
            
            // Release assigned workers
            foreach (var assignment in project.WorkerAssignments)
            {
                var worker = _availableWorkers.FirstOrDefault(w => w.WorkerId == assignment.WorkerId);
                if (worker != null)
                {
                    worker.IsAvailable = true;
                    worker.CurrentProjectId = null;
                }
            }
            
            _onProjectCompleted?.Raise();
            OnProjectCompleted?.Invoke(project);
            
            LogInfo($"Project completed: {project.ProjectName}");
        }
        
        /// <summary>
        /// Create construction schedule for a project
        /// </summary>
        private ConstructionSchedule CreateConstructionSchedule(BuildingProject project)
        {
            var schedule = new ConstructionSchedule
            {
                ScheduleId = Guid.NewGuid().ToString(),
                ProjectStartDate = DateTime.Now,
                ProjectEndDate = DateTime.Now.AddDays(project.EstimatedDays),
                Tasks = new List<ConstructionTask>(),
                Dependencies = new List<TaskDependency>(),
                Milestones = new List<ScheduleMilestone>(),
                OverallProgress = 0f,
                IsOnSchedule = true,
                DelayDays = 0
            };
            
            // Add tasks based on construction stages
            var stages = System.Enum.GetValues(typeof(ConstructionStage)).Cast<ConstructionStage>().ToList();
            foreach (var stage in stages)
            {
                if (stage == ConstructionStage.Completed) continue;
                
                var task = new ConstructionTask
                {
                    TaskId = Guid.NewGuid().ToString(),
                    TaskName = $"{stage} - {project.ProjectName}",
                    Description = $"Complete {stage} stage for {project.ProjectName}",
                    Stage = stage,
                    StartDate = DateTime.Now.AddDays(stages.IndexOf(stage) * 7),
                    EndDate = DateTime.Now.AddDays((stages.IndexOf(stage) + 1) * 7),
                    DurationDays = 7,
                    EstimatedHours = 56f, // 7 days * 8 hours
                    RequiredWorkerCount = 3,
                    Progress = 0f,
                    Status = TaskStatus.Not_Started,
                    Prerequisites = new List<string>(),
                    RequiredWorkers = new List<string>(),
                    RequiredMaterials = new List<MaterialRequirement>(),
                    Cost = project.TotalBudget / stages.Count,
                    Priority = TaskPriority.Normal
                };
                schedule.Tasks.Add(task);
            }
            
            return schedule;
        }
        
        /// <summary>
        /// Schedule required inspections for a construction stage
        /// </summary>
        private void ScheduleStageInspections(BuildingProject project, ConstructionStage stage)
        {
            var inspectionTypes = GetRequiredInspectionsForStage(stage);
            foreach (var inspectionType in inspectionTypes)
            {
                var scheduledDate = DateTime.Now.AddDays(UnityEngine.Random.Range(1, 7));
                ScheduleInspection(project.ProjectId, inspectionType, scheduledDate);
            }
        }
        
        /// <summary>
        /// Get required inspection types for a construction stage
        /// </summary>
        private List<InspectionType> GetRequiredInspectionsForStage(ConstructionStage stage)
        {
            return stage switch
            {
                ConstructionStage.Foundation => new List<InspectionType> { InspectionType.Foundation, InspectionType.Safety },
                ConstructionStage.Framing => new List<InspectionType> { InspectionType.Structural, InspectionType.Safety },
                ConstructionStage.Electrical => new List<InspectionType> { InspectionType.Electrical, InspectionType.Safety },
                ConstructionStage.Plumbing => new List<InspectionType> { InspectionType.Plumbing, InspectionType.Safety },
                ConstructionStage.HVAC => new List<InspectionType> { InspectionType.HVAC, InspectionType.Safety },
                ConstructionStage.Finishing => new List<InspectionType> { InspectionType.Final, InspectionType.Safety },
                _ => new List<InspectionType> { InspectionType.Safety }
            };
        }
        
        /// <summary>
        /// Estimate cost impact of a construction issue
        /// </summary>
        private float EstimateIssueCostImpact(IssueSeverity severity, IssueCategory category)
        {
            float baseCost = category switch
            {
                IssueCategory.Safety => 5000f,
                IssueCategory.Quality => 2000f,
                IssueCategory.Schedule => 1500f,
                IssueCategory.Budget => 3000f,
                IssueCategory.Materials => 1000f,
                IssueCategory.Labor => 2500f,
                IssueCategory.Permits => 800f,
                IssueCategory.Weather => 1200f,
                IssueCategory.Design_Change => 4000f,
                _ => 500f
            };
            
            float multiplier = severity switch
            {
                IssueSeverity.Critical => 3.0f,
                IssueSeverity.High => 2.0f,
                IssueSeverity.Medium => 1.5f,
                IssueSeverity.Low => 1.0f,
                _ => 0.5f
            };
            
            return baseCost * multiplier;
        }
        
        /// <summary>
        /// Estimate delay impact of a construction issue
        /// </summary>
        private int EstimateIssueDelayImpact(IssueSeverity severity, IssueCategory category)
        {
            int baseDays = category switch
            {
                IssueCategory.Safety => 7,
                IssueCategory.Quality => 3,
                IssueCategory.Schedule => 2,
                IssueCategory.Budget => 5,
                IssueCategory.Materials => 1,
                IssueCategory.Labor => 4,
                IssueCategory.Permits => 2,
                IssueCategory.Weather => 1,
                IssueCategory.Design_Change => 6,
                _ => 1
            };
            
            int multiplier = severity switch
            {
                IssueSeverity.Critical => 3,
                IssueSeverity.High => 2,
                IssueSeverity.Medium => 1,
                IssueSeverity.Low => 1,
                _ => 0
            };
            
            return baseDays * multiplier;
        }
        
        /// <summary>
        /// Generate a random quality defect for simulation
        /// </summary>
        private QualityDefect GenerateRandomDefect(string area)
        {
            var defectTypes = new[] { "Crack", "Misalignment", "Poor finish", "Missing component", "Incorrect installation" };
            var severities = new[] { DefectSeverity.Minor, DefectSeverity.Major, DefectSeverity.Critical };
            
            return new QualityDefect
            {
                DefectId = Guid.NewGuid().ToString(),
                Area = area,
                DefectType = defectTypes[UnityEngine.Random.Range(0, defectTypes.Length)],
                Severity = severities[UnityEngine.Random.Range(0, severities.Length)],
                Description = $"Quality issue detected in {area}",
                DetectedDate = DateTime.Now,
                Status = DefectStatus.Open
            };
        }
        
        /// <summary>
        /// Update quality metrics based on inspection results
        /// </summary>
        private void UpdateQualityMetrics(QualityMetrics metrics, QualityInspection inspection)
        {
            // Update metrics based on inspection area and results
            switch (inspection.Area.ToLower())
            {
                case "structural":
                    metrics.StructuralQuality = (metrics.StructuralQuality + inspection.QualityScore) / 2f;
                    break;
                case "finish":
                    metrics.FinishQuality = (metrics.FinishQuality + inspection.QualityScore) / 2f;
                    break;
                case "systems":
                    metrics.SystemsQuality = (metrics.SystemsQuality + inspection.QualityScore) / 2f;
                    break;
                case "safety":
                    metrics.SafetyCompliance = (metrics.SafetyCompliance + inspection.QualityScore) / 2f;
                    break;
                case "environmental":
                    metrics.EnvironmentalCompliance = (metrics.EnvironmentalCompliance + inspection.QualityScore) / 2f;
                    break;
            }
            
            metrics.TotalInspections++;
            if (inspection.QualityScore >= _qualityThreshold)
            {
                metrics.PassedInspections++;
            }
        }
        
        /// <summary>
        /// Get permit issuing authority for permit type
        /// </summary>
        private string GetPermitAuthority(PermitType permitType)
        {
            return permitType switch
            {
                PermitType.Building => "City Building Department",
                PermitType.Electrical => "State Electrical Board",
                PermitType.Plumbing => "City Plumbing Department",
                PermitType.HVAC => "HVAC Licensing Board",
                PermitType.Fire_Safety => "Fire Department",
                PermitType.Environmental => "Environmental Protection Agency",
                PermitType.Cannabis_License => "Cannabis Control Board",
                _ => "General Permits Office"
            };
        }
        
        /// <summary>
        /// Calculate permit cost based on type and project
        /// </summary>
        private float CalculatePermitCost(PermitType permitType, BuildingProject project)
        {
            float baseCost = permitType switch
            {
                PermitType.Building => project.TotalBudget * 0.02f, // 2% of project cost
                PermitType.Electrical => 1500f,
                PermitType.Plumbing => 1200f,
                PermitType.HVAC => 1800f,
                PermitType.Fire_Safety => 800f,
                PermitType.Environmental => 2500f,
                PermitType.Cannabis_License => 5000f, // Cannabis permits are expensive
                _ => 500f
            };
            
            return baseCost * _permitCostMultiplier;
        }
        
        /// <summary>
        /// Get required documents for permit type
        /// </summary>
        private List<string> GetRequiredDocuments(PermitType permitType)
        {
            return permitType switch
            {
                PermitType.Building => new List<string> { "Architectural Plans", "Structural Drawings", "Site Plan", "Zoning Compliance" },
                PermitType.Electrical => new List<string> { "Electrical Plans", "Load Calculations", "Equipment Specifications" },
                PermitType.Plumbing => new List<string> { "Plumbing Plans", "Water Supply Calculations", "Drainage Plans" },
                PermitType.HVAC => new List<string> { "HVAC Plans", "Load Calculations", "Equipment Specifications", "Ductwork Drawings" },
                PermitType.Fire_Safety => new List<string> { "Fire Safety Plan", "Sprinkler System Plans", "Emergency Exit Plans" },
                PermitType.Environmental => new List<string> { "Environmental Impact Assessment", "Waste Management Plan", "Air Quality Report" },
                PermitType.Cannabis_License => new List<string> { "Security Plan", "Cultivation Plan", "Waste Disposal Plan", "Track and Trace System" },
                _ => new List<string> { "General Application", "Site Plan" }
            };
        }
        
        /// <summary>
        /// Get permit processing time in days
        /// </summary>
        private int GetPermitProcessingDays(PermitType permitType)
        {
            return permitType switch
            {
                PermitType.Building => 30,
                PermitType.Electrical => 14,
                PermitType.Plumbing => 10,
                PermitType.HVAC => 14,
                PermitType.Fire_Safety => 21,
                PermitType.Environmental => 45,
                PermitType.Cannabis_License => 60, // Cannabis permits take longer
                _ => 7
            };
        }
        
        /// <summary>
        /// Get random inspector name for simulation
        /// </summary>
        private string GetRandomInspectorName()
        {
            var inspectorNames = new[] 
            { 
                "John Smith", "Sarah Johnson", "Mike Davis", "Lisa Wilson", "Tom Brown", 
                "Jennifer Garcia", "David Miller", "Amanda Taylor", "Chris Anderson", "Maria Rodriguez" 
            };
            return inspectorNames[UnityEngine.Random.Range(0, inspectorNames.Length)];
        }
        
        /// <summary>
        /// Calculate total labor costs for a project
        /// </summary>
        private float CalculateLaborCosts(BuildingProject project)
        {
            float totalLaborCost = 0f;
            foreach (var assignment in project.WorkerAssignments)
            {
                float hoursWorked = assignment.HoursPerDay * (DateTime.Now - assignment.StartDate).Days;
                totalLaborCost += hoursWorked * assignment.HourlyRate;
            }
            return totalLaborCost;
        }
        
        /// <summary>
        /// Calculate total material costs for a project
        /// </summary>
        private float CalculateMaterialCosts(BuildingProject project)
        {
            float totalMaterialCost = 0f;
            foreach (var material in project.MaterialRequirements)
            {
                totalMaterialCost += material.OrderedQuantity * material.UnitCost;
            }
            return totalMaterialCost;
        }
        
        /// <summary>
        /// Calculate total permit costs for a project
        /// </summary>
        private float CalculatePermitCosts(string projectId)
        {
            if (!_projectPermits.TryGetValue(projectId, out var permits))
                return 0f;
                
            return permits.Sum(p => p.Cost);
        }
        
        /// <summary>
        /// Update project progress based on worker assignments and milestones
        /// </summary>
        private void UpdateProjectProgress(BuildingProject project)
        {
            // Calculate progress based on completed milestones and worker progress
            float milestoneProgress = (float)project.CompletedMilestones.Count / Enum.GetValues(typeof(ConstructionStage)).Length * 100f;
            float workerProgress = project.WorkerAssignments.Count > 0 ? project.WorkerAssignments.Average(w => w.CompletionPercentage) : 0f;
            
            project.OverallProgress = (milestoneProgress + workerProgress) / 2f;
            project.OverallProgress = Mathf.Clamp(project.OverallProgress, 0f, 100f);
        }
        
        /// <summary>
        /// Process worker progress for all active projects
        /// </summary>
        private void ProcessWorkerProgress(BuildingProject project)
        {
            foreach (var assignment in project.WorkerAssignments)
            {
                if (assignment.Status == WorkerStatus.Active)
                {
                    // Simulate daily progress based on worker skill and performance
                    var worker = _availableWorkers.FirstOrDefault(w => w.WorkerId == assignment.WorkerId);
                    if (worker != null)
                    {
                        float dailyProgress = ((int)worker.SkillLevel * worker.QualityRating) / 25f; // 0-1 progress per day
                        assignment.CompletionPercentage += dailyProgress;
                        assignment.CompletionPercentage = Mathf.Clamp(assignment.CompletionPercentage, 0f, 100f);
                        
                        if (assignment.CompletionPercentage >= 100f)
                        {
                            assignment.Status = WorkerStatus.Completed;
                            worker.IsAvailable = true;
                            worker.CurrentProjectId = null;
                        }
                    }
                }
            }
            
            UpdateProjectProgress(project);
        }
        
        /// <summary>
        /// Process material deliveries for all active projects
        /// </summary>
        private void ProcessMaterialDeliveries(BuildingProject project)
        {
            foreach (var material in project.MaterialRequirements)
            {
                if (material.Status == MaterialStatus.Ordered && 
                    DateTime.Now >= material.ExpectedDelivery)
                {
                    material.Status = MaterialStatus.Delivered;
                    material.AvailableQuantity += material.OrderedQuantity;
                    
                    LogInfo($"Materials delivered: {material.MaterialName} x{material.OrderedQuantity} to {project.ProjectName}");
                }
            }
        }
        
        /// <summary>
        /// Check for random construction issues
        /// </summary>
        private void CheckForRandomIssues(BuildingProject project)
        {
            // Random chance of issues occurring
            if (UnityEngine.Random.Range(0f, 1f) < 0.05f) // 5% chance per day
            {
                var categories = Enum.GetValues(typeof(IssueCategory)).Cast<IssueCategory>().ToArray();
                var severities = Enum.GetValues(typeof(IssueSeverity)).Cast<IssueSeverity>().ToArray();
                
                var category = categories[UnityEngine.Random.Range(0, categories.Length)];
                var severity = severities[UnityEngine.Random.Range(0, severities.Length)];
                
                ReportIssue(project.ProjectId, $"Random {category} Issue", 
                           $"Unexpected {category.ToString().ToLower()} issue detected", 
                           severity, category);
            }
        }
        
        /// <summary>
        /// Check schedule status for projects
        /// </summary>
        private void CheckScheduleStatus()
        {
            foreach (var project in _activeProjects)
            {
                if (_projectSchedules.TryGetValue(project.ProjectId, out var schedule))
                {
                    project.IsOnSchedule = DateTime.Now <= schedule.ProjectEndDate;
                    
                    if (!project.IsOnSchedule && project.CurrentStage != ConstructionStage.Completed)
                    {
                        LogWarning($"Project behind schedule: {project.ProjectName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Check budget status for projects
        /// </summary>
        private void CheckBudgetStatus()
        {
            foreach (var project in _activeProjects)
            {
                project.IsOnBudget = project.RemainingBudget >= 0;
                
                if (!project.IsOnBudget)
                {
                    LogWarning($"Project over budget: {project.ProjectName} (${Math.Abs(project.RemainingBudget):F0} over)");
                }
            }
        }
        
        /// <summary>
        /// Process pending inspections
        /// </summary>
        private void ProcessInspection(ConstructionInspection inspection)
        {
            // Simulate inspection results
            float passChance = UnityEngine.Random.Range(0.7f, 0.95f); // 70-95% pass rate
            inspection.Result = UnityEngine.Random.Range(0f, 1f) < passChance ? 
                               InspectionResult.Passed : InspectionResult.Failed;
            
            inspection.CompletedDate = DateTime.Now;
            inspection.Notes.Add($"Inspection completed on {DateTime.Now:yyyy-MM-dd}");
            
            if (inspection.Result == InspectionResult.Passed)
            {
                inspection.PassedItems.Add("All items passed inspection");
            }
            else
            {
                inspection.FailedItems.Add("Minor deficiencies found - requires correction");
            }
            
            _onInspectionCompleted?.Raise();
            OnInspectionCompleted?.Invoke(inspection);
            
            LogInfo($"Inspection {inspection.Result}: {inspection.Type} for {inspection.InspectionId}");
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            LogInfo("Construction Manager shutting down...");
            
            // Save any pending data
            // Clean up resources
        }
    }
    
    // Supporting classes - Note: ConstructionWorker is now defined in ConstructionDataStructures.cs
    
    [System.Serializable]
    public class MaterialSupplier
    {
        public string SupplierId;
        public string Name;
        public int DeliveryDays;
        public float ReliabilityRating; // 0-5
        public float PriceRating; // 0-5
        public float QualityRating; // 0-5
        public List<BuildingMaterial> AvailableMaterials;
    }
    
    [System.Serializable]
    public class ProjectCostBreakdown
    {
        public string ProjectId;
        public float TotalBudget;
        public float SpentAmount;
        public float RemainingBudget;
        public float LaborCosts;
        public float MaterialCosts;
        public float PermitCosts;
        public float OverheadCosts;
        public float ContingencyCosts;
    }
}