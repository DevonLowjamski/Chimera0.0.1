using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.Construction;
// using ProjectChimera.Systems.Prefabs; // Cannot reference without circular dependency
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using RoomTemplate = ProjectChimera.Data.Facilities.RoomTemplate;
// Explicit aliases for Data layer types to resolve ambiguity
using DataIssueType = ProjectChimera.Data.Construction.IssueType;
using DataIssueSeverity = ProjectChimera.Data.Construction.IssueSeverity;
using DataIssueCategory = ProjectChimera.Data.Construction.IssueCategory;
using RoomStatus = ProjectChimera.Data.Construction.RoomStatus;
using DataIssueStatus = ProjectChimera.Data.Construction.IssueStatus;
using ConstructionPhaseType = ProjectChimera.Data.Construction.ConstructionPhase;
using ConstructionEvent = ProjectChimera.Data.Construction.ConstructionEvent;
using ConstructionProjectType = ProjectChimera.Data.Construction.ProjectType;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Interactive facility construction system for Project Chimera.
    /// Handles real-time facility design, construction planning, and building execution
    /// with realistic construction timelines, costs, and building code compliance.
    /// </summary>
    public class InteractiveFacilityConstructor : ChimeraManager
    {
        [Header("Construction Configuration")]
        [SerializeField] private ConstructionSettings _constructionSettings;
        [SerializeField] private bool _enableRealTimeConstruction = true;
        [SerializeField] private bool _enforceZoningLaws = true;
        [SerializeField] private bool _requirePermits = true;
        [SerializeField] private float _constructionSpeedMultiplier = 1f;
        
        [Header("Design Tools")]
        [SerializeField] private GridSnapSettings _gridSnapSettings;
        [SerializeField] private bool _enableGridSnapping = true;
        [SerializeField] private bool _showConstructionGuides = true;
        [SerializeField] private bool _validateRealTime = true;
        
        [Header("Building Systems")]
        [SerializeField] private LayerMask _constructionLayer = 1;
        [SerializeField] private Material _previewMaterial;
        [SerializeField] private Material _validPlacementMaterial;
        [SerializeField] private Material _invalidPlacementMaterial;
        
        [Header("Economic Integration")]
        [SerializeField] private bool _enableCostEstimation = true;
        [SerializeField] private bool _enableFinancing = true;
        [SerializeField] private float _laborCostPerHour = 45f;
        [SerializeField] private float _materialMarkup = 1.3f;
        
        // Core construction state
        private ConstructionProject _activeProject;
        private ConstructionPhaseType _currentPhase = ConstructionPhaseType.Planning;
        private List<ConstructionProject> _allProjects = new List<ConstructionProject>();
        private Queue<ConstructionTask> _constructionQueue = new Queue<ConstructionTask>();
        
        // Design and preview systems
        private GameObject _previewObject;
        private FacilityDesignTool _designTool;
        private BuildingValidator _buildingValidator;
        private ConstructionPlanner _constructionPlanner;
        
        // Resource and workforce management
        private ConstructionWorkforce _workforce;
        private MaterialInventory _materialInventory;
        private EquipmentPool _equipmentPool;
        private ContractorManager _contractorManager;
        
        // Runtime tracking
        private Dictionary<string, ConstructionProgress> _activeConstructions = new Dictionary<string, ConstructionProgress>();
        private List<PermitApplication> _pendingPermits = new List<PermitApplication>();
        private ConstructionSchedule _masterSchedule;
        
        // Performance tracking
        private ConstructionMetrics _metrics;
        private List<ConstructionEvent> _constructionHistory = new List<ConstructionEvent>();
        
        // Events
        public System.Action<object> OnProjectStarted;
        public System.Action<ConstructionProject, ConstructionPhaseType> OnPhaseCompleted;
        public System.Action<object> OnProjectCompleted;
        public System.Action<object> OnConstructionIssue;
        public System.Action<PermitApplication> OnPermitApproved;
        public System.Action<ConstructionCostUpdate> OnCostUpdated;
        public System.Action<string> OnMilestoneReached;
        public System.Action<ConstructionProgress> OnConstructionProgress;
        public System.Action<string, float> OnFacilityExpansionStarted;
        public System.Action<string, float> OnFacilityExpansionCompleted;
        public System.Action<string, ConstructionRoomTemplate> OnRoomAdded;
        public System.Action<string, string> OnRoomRemoved;
        public System.Action<string, float> OnBuildingCapacityUpdated;
        
        // Properties
        public ConstructionProject ActiveProject => _activeProject;
        public ConstructionPhaseType CurrentPhase => _currentPhase;
        public List<ConstructionProject> AllProjects => _allProjects;
        public ConstructionMetrics Metrics => _metrics;
        public bool IsConstructing => _constructionQueue.Count > 0;
        
        protected override void OnManagerInitialize()
        {
            InitializeConstructionSystems();
            SetupDesignTools();
            InitializeWorkforce();
            StartConstructionLoop();
        }
        
        private void Update()
        {
            if (_enableRealTimeConstruction)
            {
                ProcessConstructionQueue();
                UpdateActiveConstructions();
                ProcessPermitApplications();
            }
            
            UpdateDesignPreview();
            UpdateConstructionMetrics();
        }
        
        #region Initialization
        
        private void InitializeConstructionSystems()
        {
            // Initialize construction settings
            if (_constructionSettings == null)
            {
                _constructionSettings = CreateDefaultConstructionSettings();
            }
            
            // Initialize core systems
            _designTool = new FacilityDesignTool(_gridSnapSettings);
            _buildingValidator = new BuildingValidator(_constructionSettings);
            _constructionPlanner = new ConstructionPlanner();
            
            // Initialize workforce and resources
            _workforce = new ConstructionWorkforce();
            _materialInventory = new MaterialInventory();
            _equipmentPool = new EquipmentPool();
            _contractorManager = new ContractorManager();
            
            // Initialize tracking systems
            _masterSchedule = new ConstructionSchedule();
            _metrics = new ConstructionMetrics();
            
            LogInfo("Interactive Facility Constructor initialized");
        }
        
        private ConstructionSettings CreateDefaultConstructionSettings()
        {
            return new ConstructionSettings
            {
                MinRoomSize = new Vector3(2f, 2.5f, 2f),
                MaxRoomSize = new Vector3(50f, 6f, 50f),
                WallThickness = 0.2f,
                RequireFoundation = true,
                EnforceFireSafety = true,
                RequireVentilation = true,
                MaxBuildingHeight = 10f,
                MinSetbackDistance = 3f,
                RequiredParkingSpaces = 2,
                MaxLotCoverage = 0.8f
            };
        }
        
        private void SetupDesignTools()
        {
            if (_gridSnapSettings == null)
            {
                _gridSnapSettings = new GridSnapSettings
                {
                    GridSize = 1f,
                    SnapToGrid = true,
                    ShowGrid = true,
                    GridColor = Color.gray,
                    MajorGridColor = Color.white,
                    MajorGridInterval = 5
                };
            }
            
            // Create preview materials if not assigned
            if (_previewMaterial == null)
            {
                _previewMaterial = CreatePreviewMaterial(new Color(0.5f, 0.5f, 1f, 0.5f));
            }
            
            if (_validPlacementMaterial == null)
            {
                _validPlacementMaterial = CreatePreviewMaterial(new Color(0f, 1f, 0f, 0.5f));
            }
            
            if (_invalidPlacementMaterial == null)
            {
                _invalidPlacementMaterial = CreatePreviewMaterial(new Color(1f, 0f, 0f, 0.5f));
            }
        }
        
        private Material CreatePreviewMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.5f);
            material.renderQueue = 3000;
            return material;
        }
        
        private void InitializeWorkforce()
        {
            // Initialize default workforce
            _workforce.AddWorker(new ConstructionWorker
            {
                WorkerId = "foreman_001",
                Name = "Site Foreman",
                Specialty = WorkerSpecialty.GeneralConstruction,
                SkillLevel = SkillLevel.Expert,
                HourlyRate = 65f,
                IsAvailable = true,
                ProductivityModifier = 1.2f
            });
            
            _workforce.AddWorker(new ConstructionWorker
            {
                WorkerId = "electrician_001",
                Name = "Master Electrician",
                Specialty = WorkerSpecialty.Electrical,
                SkillLevel = SkillLevel.Expert,
                HourlyRate = 75f,
                IsAvailable = true,
                ProductivityModifier = 1.1f
            });
            
            _workforce.AddWorker(new ConstructionWorker
            {
                WorkerId = "plumber_001",
                Name = "Licensed Plumber",
                Specialty = WorkerSpecialty.Plumbing,
                SkillLevel = SkillLevel.Skilled,
                HourlyRate = 70f,
                IsAvailable = true,
                ProductivityModifier = 1.0f
            });
            
            _workforce.AddWorker(new ConstructionWorker
            {
                WorkerId = "hvac_001",
                Name = "HVAC Technician",
                Specialty = WorkerSpecialty.HVAC,
                SkillLevel = SkillLevel.Expert,
                HourlyRate = 68f,
                IsAvailable = true,
                ProductivityModifier = 1.15f
            });
        }
        
        private void StartConstructionLoop()
        {
            if (_enableRealTimeConstruction)
            {
                InvokeRepeating(nameof(ProcessConstructionTick), 1f, 1f);
            }
        }
        
        #endregion
        
        #region Project Management
        
        public ConstructionProject CreateNewProject(string projectName, Vector3 buildingSite, FacilityTemplate template)
        {
            var project = new ConstructionProject
            {
                ProjectId = System.Guid.NewGuid().ToString(),
                ProjectName = projectName,
                BuildingSite = buildingSite,
                FacilityTemplate = template,
                Status = ProjectStatus.Planning,
                CreatedDate = System.DateTime.Now,
                EstimatedCost = CalculateProjectCost(template),
                EstimatedDuration = Mathf.RoundToInt(CalculateProjectDuration(template) / 24f), // Convert hours to days
                RequiredPermits = DetermineRequiredPermits(template).Select(p => p.ToString()).ToList()
            };
            
            // Validate building site
            var validation = _buildingValidator.ValidateBuildingSite(buildingSite, template);
            project.ValidationResults = validation;
            
            if (!validation.IsValid)
            {
                project.Status = ProjectStatus.RequiresRevision;
                OnConstructionIssue?.Invoke(new ProjectChimera.Data.Construction.ConstructionIssue
                {
                    IssueType = DataIssueType.ValidationFailed,
                    Description = "Building site validation failed",
                    Severity = DataIssueSeverity.High
                });
            }
            
            _allProjects.Add(project);
            _activeProject = project;
            
            OnProjectStarted?.Invoke(project);
            LogInfo($"Created new construction project: {projectName}");
            
            return project;
        }
        
        public bool StartProjectPhase(string projectId, ConstructionPhaseType phase)
        {
            var project = _allProjects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
            {
                LogWarning($"Project {projectId} not found");
                return false;
            }
            
            // Validate phase prerequisites
            if (!ValidatePhasePrerequisites(project, phase))
            {
                return false;
            }
            
            project.CurrentPhase = phase;
            _currentPhase = phase;
            
            // Create construction tasks for this phase
            var tasks = _constructionPlanner.CreateTasksForPhase(project, phase);
            foreach (var task in tasks)
            {
                _constructionQueue.Enqueue(task);
            }
            
            LogInfo($"Started {phase} phase for project {project.ProjectName}");
            return true;
        }
        
        private bool ValidatePhasePrerequisites(ConstructionProject project, ConstructionPhaseType phase)
        {
            switch (phase)
            {
                case ConstructionPhaseType.Planning:
                    return true;
                    
                case ConstructionPhaseType.Permitting:
                    return project.ValidationResults.IsValid;
                    
                case ConstructionPhaseType.SitePreparation:
                    return project.PermitsApproved && project.ValidationResults.IsValid;
                    
                case ConstructionPhaseType.Foundation:
                    return project.CompletedPhases.Contains(ConstructionPhaseType.SitePreparation);
                    
                case ConstructionPhaseType.Structure:
                    return project.CompletedPhases.Contains(ConstructionPhaseType.Foundation);
                    
                case ConstructionPhaseType.Systems:
                    return project.CompletedPhases.Contains(ConstructionPhaseType.Structure);
                    
                case ConstructionPhaseType.Finishing:
                    return project.CompletedPhases.Contains(ConstructionPhaseType.Systems);
                    
                case ConstructionPhaseType.Final:
                    return project.CompletedPhases.Contains(ConstructionPhaseType.Finishing);
                    
                default:
                    return false;
            }
        }
        
        #endregion
        
        #region Interactive Design Tools
        
        public void StartDesignMode(FacilityTemplate template)
        {
            _designTool.StartDesign(template);
            ShowConstructionGuides(true);
        }
        
        public void EndDesignMode()
        {
            _designTool.EndDesign();
            ShowConstructionGuides(false);
            ClearPreview();
        }
        
        public void PlaceRoomPreview(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            ClearPreview();
            
            // Create preview object
            _previewObject = _designTool.CreateRoomPreview(roomTemplate, position, rotation);
            
            // Validate placement
            bool isValid = _buildingValidator.ValidateRoomPlacement(roomTemplate, position, rotation);
            
            // Apply appropriate material
            var renderer = _previewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isValid ? _validPlacementMaterial : _invalidPlacementMaterial;
            }
            
            // Show placement feedback
            ShowPlacementFeedback(position, isValid);
        }
        
        public bool ConfirmRoomPlacement(ConstructionRoomTemplate roomTemplate, Vector3 position, Quaternion rotation)
        {
            if (!_buildingValidator.ValidateRoomPlacement(roomTemplate, position, rotation))
            {
                OnConstructionIssue?.Invoke(new ProjectChimera.Data.Construction.ConstructionIssue
                {
                    IssueType = DataIssueType.InvalidPlacement,
                    Description = "Room placement violates building codes",
                    Severity = DataIssueSeverity.Medium
                });
                return false;
            }
            
            // Add room to active project
            if (_activeProject != null)
            {
                _activeProject.PlannedRooms.Add(new PlannedRoom
                {
                    RoomTemplate = roomTemplate,
                    Position = position,
                    Rotation = rotation,
                    Status = RoomStatus.Planned
                });
                
                // Update cost estimate
                UpdateProjectCostEstimate(_activeProject);
            }
            
            ClearPreview();
            return true;
        }
        
        private void UpdateDesignPreview()
        {
            if (_previewObject != null && Input.GetMouseButton(0))
            {
                // Update preview position based on mouse input
                var mousePos = Input.mousePosition;
                var ray = Camera.main.ScreenPointToRay(mousePos);
                
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _constructionLayer))
                {
                    Vector3 targetPosition = hit.point;
                    
                    if (_enableGridSnapping)
                    {
                        targetPosition = SnapToGrid(targetPosition);
                    }
                    
                    _previewObject.transform.position = targetPosition;
                }
            }
        }
        
        private Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = _gridSnapSettings.GridSize;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }
        
        private void ShowConstructionGuides(bool show)
        {
            if (!_showConstructionGuides) return;
            
            // Implementation would show/hide construction grid, guidelines, etc.
            Debug.Log($"Construction guides: {(show ? "Shown" : "Hidden")}");
        }
        
        private void ShowPlacementFeedback(Vector3 position, bool isValid)
        {
            // Implementation would show visual feedback for placement validity
            Debug.Log($"Placement at {position}: {(isValid ? "Valid" : "Invalid")}");
        }
        
        private void ClearPreview()
        {
            if (_previewObject != null)
            {
                DestroyImmediate(_previewObject);
                _previewObject = null;
            }
        }
        
        #endregion
        
        #region Construction Execution
        
        private void ProcessConstructionQueue()
        {
            if (_constructionQueue.Count == 0) return;
            
            var task = _constructionQueue.Peek();
            
            // Check if workers are available
            var availableWorkers = _workforce.GetAvailableWorkers(task.RequiredSpecialty);
            if (availableWorkers.Count == 0)
            {
                return; // Wait for workers
            }
            
            // Check if materials are available
            if (!_materialInventory.HasMaterials(task.RequiredMaterials))
            {
                return; // Wait for materials
            }
            
            // Process the task
            ProcessConstructionTask(_constructionQueue.Dequeue());
        }
        
        private void ProcessConstructionTask(ConstructionTask task)
        {
            if (!_activeConstructions.ContainsKey(task.TaskId))
            {
                // Start new construction task
                var progress = new ConstructionProgress
                {
                    TaskId = task.TaskId,
                    Task = task,
                    StartTime = System.DateTime.Now,
                    Progress = 0f,
                    AssignedWorkers = _workforce.AssignWorkers(task.RequiredSpecialty, task.RequiredWorkerCount),
                    Status = TaskStatus.In_Progress
                };
                
                _activeConstructions[task.TaskId] = progress;
                LogInfo($"Started construction task: {task.TaskName}");
            }
        }
        
        private void ProcessConstructionTick()
        {
            var completedTasks = new List<string>();
            
            foreach (var kvp in _activeConstructions)
            {
                var progress = kvp.Value;
                var task = progress.Task;
                
                // Calculate progress based on worker productivity
                float workerProductivity = progress.AssignedWorkers.Sum(w => w.ProductivityModifier);
                float progressPerTick = (workerProductivity / task.EstimatedHours) * _constructionSpeedMultiplier;
                
                progress.Progress += progressPerTick / 3600f; // Convert to per-second progress
                progress.Progress = Mathf.Clamp01(progress.Progress);
                
                // Check for completion
                if (progress.Progress >= 1f)
                {
                    CompleteConstructionTask(progress);
                    completedTasks.Add(kvp.Key);
                }
                
                // Update costs
                UpdateTaskCosts(progress);
            }
            
            // Remove completed tasks
            foreach (var taskId in completedTasks)
            {
                _activeConstructions.Remove(taskId);
            }
        }
        
        private void CompleteConstructionTask(ConstructionProgress progress)
        {
            var task = progress.Task;
            
            // Mark task as completed
            progress.Status = TaskStatus.Completed;
            progress.CompletionTime = System.DateTime.Now;
            
            // Release workers
            _workforce.ReleaseWorkers(progress.AssignedWorkers);
            
            // Update project progress
            if (_activeProject != null)
            {
                _activeProject.CompletedTasks.Add(task.TaskId);
                
                // Check if phase is complete
                if (IsPhaseComplete(_activeProject, (ConstructionPhaseType)task.ConstructionPhase))
                {
                    CompleteProjectPhase(_activeProject, (ConstructionPhaseType)task.ConstructionPhase);
                }
            }
            
            LogInfo($"Completed construction task: {task.TaskName}");
        }
        
        private bool IsPhaseComplete(ConstructionProject project, ConstructionPhaseType phase)
        {
            var phaseTasks = _constructionPlanner.GetTasksForPhase(project, phase);
            return phaseTasks.All(t => project.CompletedTasks.Contains(t.TaskId));
        }
        
        private void CompleteProjectPhase(ConstructionProject project, ConstructionPhaseType phase)
        {
            project.CompletedPhases.Add(phase);
            
            OnPhaseCompleted?.Invoke(project, phase);
            
            // Invoke OnMilestoneReached event for other systems
            OnMilestoneReached?.Invoke($"{project.ProjectName}_{phase}");
            
            // Check if project is complete
            if (phase == ConstructionPhaseType.Final)
            {
                CompleteProject(project);
            }
            
            LogInfo($"Completed {phase} phase for project {project.ProjectName}");
        }
        
        private void CompleteProject(ConstructionProject project)
        {
            project.Status = ProjectStatus.Completed;
            project.CompletionDate = System.DateTime.Now;
            project.ActualCost = CalculateActualProjectCost(project);
            
            OnProjectCompleted?.Invoke(project);
            
            LogInfo($"Project completed: {project.ProjectName}");
        }
        
        private void UpdateActiveConstructions()
        {
            foreach (var progress in _activeConstructions.Values)
            {
                // Update visual progress indicators
                UpdateConstructionVisuals(progress);
                
                // Check for construction issues
                CheckForConstructionIssues(progress);
                
                // Update worker efficiency based on conditions
                UpdateWorkerEfficiency(progress);
            }
        }
        
        private void UpdateConstructionVisuals(ConstructionProgress progress)
        {
            // Implementation would update 3D construction progress visuals
            // This could include scaffolding, partial structures, etc.
        }
        
        private void CheckForConstructionIssues(ConstructionProgress progress)
        {
            // Weather delays
            if (IsWeatherDelaying())
            {
                progress.Task.EstimatedHours *= 1.1f; // 10% delay
                
                OnConstructionIssue?.Invoke(new ProjectChimera.Data.Construction.ConstructionIssue
                {
                    IssueType = DataIssueType.WeatherDelay,
                    Description = "Construction delayed due to weather conditions",
                    Severity = DataIssueSeverity.Low
                });
            }
            
            // Material shortages
            if (!_materialInventory.HasMaterials(progress.Task.RequiredMaterials))
            {
                OnConstructionIssue?.Invoke(new ProjectChimera.Data.Construction.ConstructionIssue
                {
                    IssueType = DataIssueType.MaterialShortage,
                    Description = "Missing required materials for construction",
                    Severity = DataIssueSeverity.Medium
                });
            }
        }
        
        private bool IsWeatherDelaying()
        {
            // Simplified weather check - would integrate with weather system
            return UnityEngine.Random.value < 0.1f; // 10% chance of weather delay
        }
        
        private void UpdateWorkerEfficiency(ConstructionProgress progress)
        {
            foreach (var worker in progress.AssignedWorkers)
            {
                // Fatigue system
                if (progress.Task.EstimatedHours > 8f)
                {
                    worker.ProductivityModifier *= 0.95f; // Slight efficiency loss for long tasks
                }
                
                // Skill matching
                if (worker.Specialty == progress.Task.RequiredSpecialty)
                {
                    worker.ProductivityModifier *= 1.1f; // Bonus for matching specialty
                }
            }
        }
        
        #endregion
        
        #region Cost and Economics
        
        private float CalculateProjectCost(FacilityTemplate template)
        {
            float totalCost = 0f;
            
            // Base construction costs
            totalCost += template.BaseConstructionCost;
            
            // Room-specific costs
            foreach (var room in template.RoomTemplates)
            {
                totalCost += CalculateRoomCost(room);
            }
            
            // System installation costs
            totalCost += CalculateSystemCosts(template);
            
            // Labor costs
            totalCost += CalculateLaborCosts(template);
            
            // Permit and regulatory costs
            totalCost += CalculatePermitCosts(template);
            
            // Apply markup
            totalCost *= _materialMarkup;
            
            return totalCost;
        }
        
        private float CalculateRoomCost(ConstructionRoomTemplate room)
        {
            float area = room.Area;
            float baseCostPerSqM = 1500f; // Base cost per square meter
            
            // Adjust for room type
            float typeMultiplier = room.RoomType switch
            {
                "GrowRoom" => 2.5f,
                "ProcessingRoom" => 2.0f,
                "StorageRoom" => 1.2f,
                "Office" => 1.0f,
                "Utility" => 1.5f,
                _ => 1.0f
            };
            
            return area * baseCostPerSqM * typeMultiplier;
        }
        
        private float CalculateSystemCosts(FacilityTemplate template)
        {
            float systemCosts = 0f;
            
            // HVAC systems
            systemCosts += template.RequiredHVACCapacity * 800f; // $800 per ton
            
            // Electrical systems
            systemCosts += template.RequiredPowerCapacity * 5f; // $5 per watt
            
            // Plumbing systems
            systemCosts += template.TotalArea * 100f; // $100 per sqm
            
            // Security systems
            systemCosts += template.TotalArea * 50f; // $50 per sqm
            
            return systemCosts;
        }
        
        private float CalculateLaborCosts(FacilityTemplate template)
        {
            float totalHours = CalculateProjectDuration(template);
            return totalHours * _laborCostPerHour;
        }
        
        private float CalculatePermitCosts(FacilityTemplate template)
        {
            float permitCosts = 0f;
            
            permitCosts += 2500f; // Base building permit
            permitCosts += 1500f; // Electrical permit
            permitCosts += 1200f; // Plumbing permit
            permitCosts += 3000f; // Cannabis facility license
            
            if (template.TotalArea > 500f)
            {
                permitCosts += 5000f; // Commercial permit surcharge
            }
            
            return permitCosts;
        }
        
        private float CalculateProjectDuration(FacilityTemplate template)
        {
            float totalHours = 0f;
            
            // Base hours per square meter
            float hoursPerSqM = 15f;
            totalHours += template.TotalArea * hoursPerSqM;
            
            // Complex systems add time
            totalHours += template.RequiredHVACCapacity * 2f; // 2 hours per ton
            totalHours += template.RequiredPowerCapacity * 0.01f; // 0.01 hours per watt
            
            // Room complexity factors
            foreach (var room in template.RoomTemplates)
            {
                float roomComplexity = room.RoomType switch
                {
                    "GrowRoom" => 1.5f,
                    "ProcessingRoom" => 1.3f,
                    _ => 1.0f
                };
                
                totalHours *= roomComplexity;
            }
            
            return totalHours;
        }
        
        private float CalculateActualProjectCost(ConstructionProject project)
        {
            float actualCost = 0f;
            
            // Sum up actual task costs
            foreach (var taskId in project.CompletedTasks)
            {
                if (_constructionHistory.Any(e => e.TaskId == taskId))
                {
                    var taskEvent = _constructionHistory.First(e => e.TaskId == taskId);
                    actualCost += taskEvent.ActualCost;
                }
            }
            
            return actualCost;
        }
        
        private void UpdateProjectCostEstimate(ConstructionProject project)
        {
            project.EstimatedCost = CalculateProjectCost(project.FacilityTemplate);
            
            OnCostUpdated?.Invoke(new ConstructionCostUpdate
            {
                ProjectId = project.ProjectId,
                EstimatedCost = project.EstimatedCost,
                ActualCostToDate = CalculateActualCostToDate(project),
                CostVariance = project.EstimatedCost - CalculateActualCostToDate(project)
            });
        }
        
        private float CalculateActualCostToDate(ConstructionProject project)
        {
            return _constructionHistory
                .Where(e => e.ProjectId == project.ProjectId)
                .Sum(e => e.ActualCost);
        }
        
        private void UpdateTaskCosts(ConstructionProgress progress)
        {
            // Calculate labor costs
            float laborCost = progress.AssignedWorkers.Sum(w => w.HourlyRate) * Time.deltaTime / 3600f;
            
            // Calculate material costs
            float materialCost = CalculateTaskMaterialCost(progress.Task);
            
            // Update running total
            progress.ActualCost += laborCost + materialCost;
        }
        
        private float CalculateTaskMaterialCost(ConstructionTask task)
        {
            float materialCost = 0f;
            
            foreach (var materialRequirement in task.RequiredMaterials)
            {
                var materialData = _materialInventory.GetMaterialData(materialRequirement.MaterialName);
                if (materialData != null)
                {
                    materialCost += materialData.CostPerUnit * materialRequirement.RequiredQuantity;
                }
            }
            
            return materialCost;
        }
        
        #endregion
        
        #region Permit System
        
        private List<PermitType> DetermineRequiredPermits(FacilityTemplate template)
        {
            var permits = new List<PermitType>
            {
                PermitType.Building,
                PermitType.Electrical,
                PermitType.Plumbing
            };
            
            if (template.RequiredHVACCapacity > 5f) // > 5 tons
            {
                permits.Add(PermitType.Mechanical);
            }
            
            if (template.TotalArea > 1000f) // Large facility
            {
                permits.Add(PermitType.Fire);
                permits.Add(PermitType.Environmental);
            }
            
            // Cannabis-specific permits
            permits.Add(PermitType.Cannabis_Cultivation);
            
            if (template.RoomTemplates.Any(r => r.RoomType == "ProcessingRoom"))
            {
                permits.Add(PermitType.Cannabis_Processing);
            }
            
            return permits;
        }
        
        public void SubmitPermitApplication(ConstructionProject project, PermitType permitType)
        {
            var application = new PermitApplication
            {
                ApplicationId = System.Guid.NewGuid().ToString(),
                ProjectId = project.ProjectId,
                PermitType = permitType,
                SubmissionDate = System.DateTime.Now,
                Status = PermitStatus.Submitted,
                EstimatedProcessingDays = GetPermitProcessingTime(permitType),
                ApplicationFee = GetPermitFee(permitType)
            };
            
            _pendingPermits.Add(application);
            
            LogInfo($"Submitted {permitType} permit application for project {project.ProjectName}");
        }
        
        private void ProcessPermitApplications()
        {
            var now = System.DateTime.Now;
            
            foreach (var application in _pendingPermits.ToList())
            {
                if (application.Status == PermitStatus.Submitted)
                {
                    var daysSinceSubmission = (now - application.SubmissionDate).TotalDays;
                    
                    if (daysSinceSubmission >= application.EstimatedProcessingDays)
                    {
                        // Simulate permit approval process
                        bool approved = UnityEngine.Random.value > 0.1f; // 90% approval rate
                        
                        if (approved)
                        {
                            application.Status = PermitStatus.Approved;
                            application.ApprovalDate = now;
                            
                            OnPermitApproved?.Invoke(application);
                            
                            // Update project permit status
                            var project = _allProjects.FirstOrDefault(p => p.ProjectId == application.ProjectId);
                            if (project != null)
                            {
                                project.ApprovedPermitTypes.Add(application.PermitType);
                                
                                // Check if all permits are approved
                                if (project.RequiredPermits.All(p => project.ApprovedPermitTypes.Contains(Enum.Parse<PermitType>(p))))
                                {
                                    project.PermitsApproved = true;
                                }
                            }
                        }
                        else
                        {
                            application.Status = PermitStatus.Rejected;
                            application.RejectionReason = "Failed to meet building code requirements";
                        }
                        
                        _pendingPermits.Remove(application);
                    }
                }
            }
        }
        
        private int GetPermitProcessingTime(PermitType permitType)
        {
            return permitType switch
            {
                PermitType.Building => 14,
                PermitType.Electrical => 7,
                PermitType.Plumbing => 5,
                PermitType.Mechanical => 10,
                PermitType.Fire => 21,
                PermitType.Environmental => 30,
                PermitType.Cannabis_Cultivation => 45,
                PermitType.Cannabis_Processing => 60,
                _ => 14
            };
        }
        
        private float GetPermitFee(PermitType permitType)
        {
            return permitType switch
            {
                PermitType.Building => 2500f,
                PermitType.Electrical => 800f,
                PermitType.Plumbing => 600f,
                PermitType.Mechanical => 1200f,
                PermitType.Fire => 1500f,
                PermitType.Environmental => 3000f,
                PermitType.Cannabis_Cultivation => 15000f,
                PermitType.Cannabis_Processing => 25000f,
                _ => 500f
            };
        }
        
        #endregion
        
        #region Metrics and Analytics
        
        private void UpdateConstructionMetrics()
        {
            _metrics.TotalProjects = _allProjects.Count;
            _metrics.ActiveProjects = _allProjects.Count(p => p.Status == ProjectStatus.InProgress);
            _metrics.CompletedProjects = _allProjects.Count(p => p.Status == ProjectStatus.Completed);
            _metrics.TotalValue = _allProjects.Sum(p => p.EstimatedCost);
            _metrics.ActiveWorkers = _workforce.GetActiveWorkers().Count;
            _metrics.ConstructionEfficiency = CalculateConstructionEfficiency();
            _metrics.LastUpdated = System.DateTime.Now;
        }
        
        private float CalculateConstructionEfficiency()
        {
            if (_allProjects.Count == 0) return 1f;
            
            var completedProjects = _allProjects.Where(p => p.Status == ProjectStatus.Completed);
            if (!completedProjects.Any()) return 1f;
            
            float totalEfficiency = 0f;
            foreach (var project in completedProjects)
            {
                float costEfficiency = project.EstimatedCost / Mathf.Max(project.ActualCost, 1f);
                float timeEfficiency = project.EstimatedDuration / Mathf.Max((float)(project.CompletionDate - project.StartDate).TotalHours, 1f);
                
                totalEfficiency += (costEfficiency + timeEfficiency) / 2f;
            }
            
            return totalEfficiency / completedProjects.Count();
        }
        
        public ConstructionReport GenerateConstructionReport(string projectId = null)
        {
            var projects = string.IsNullOrEmpty(projectId) ? 
                _allProjects : 
                _allProjects.Where(p => p.ProjectId == projectId).ToList();
            
            return new ConstructionReport
            {
                ReportDate = System.DateTime.Now,
                ProjectSummaries = projects.Select(p => new ProjectSummary
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Status = p.Status,
                    Progress = CalculateProjectProgress(p),
                    EstimatedCost = p.EstimatedCost,
                    ActualCostToDate = CalculateActualCostToDate(p),
                    EstimatedCompletion = EstimateCompletionDate(p)
                }).ToList(),
                TotalMetrics = _metrics
            };
        }
        
        private float CalculateProjectProgress(ConstructionProject project)
        {
            if (project.Status == ProjectStatus.Completed) return 1f;
            if (project.Status == ProjectStatus.Planning) return 0f;
            
            float totalPhases = System.Enum.GetValues(typeof(ConstructionPhaseType)).Length;
            return project.CompletedPhases.Count / totalPhases;
        }
        
        private System.DateTime EstimateCompletionDate(ConstructionProject project)
        {
            if (project.Status == ProjectStatus.Completed)
            {
                // Use CompletionDate if available, otherwise use ActualCompletionDate, fallback to CreatedDate
                if (project.CompletionDate != default(DateTime))
                    return project.CompletionDate;
                else if (project.ActualCompletionDate != default(DateTime))
                    return project.ActualCompletionDate;
                else
                    return project.CreatedDate;
            }
            
            float progress = CalculateProjectProgress(project);
            if (progress <= 0f) return project.CreatedDate.AddDays(project.EstimatedDuration / 24f);
            
            var elapsedTime = System.DateTime.Now - project.CreatedDate;
            var totalEstimatedTime = TimeSpan.FromHours(elapsedTime.TotalHours / progress);
            
            return project.CreatedDate.Add(totalEstimatedTime);
        }
        
        #endregion
        
        #region Facility Building & Expansion
        
        /// <summary>
        /// Creates a new facility from a template at the specified location
        /// </summary>
        public ConstructionProject CreateFacility(string facilityName, Vector3 location, FacilityTemplate template)
        {
            var project = CreateNewProject(facilityName, location, template);
            
            // Initialize facility-specific data
            project.FacilityTemplate = template;
            project.ProjectType = ConstructionProjectType.Commercial;
            
            // Set up facility building phases
            project.CompletedPhases = new List<ConstructionPhaseType>();
            project.CurrentPhase = ConstructionPhaseType.Planning;
            
            // Calculate facility capacity
            var facilityCapacity = CalculateFacilityCapacity(template);
            OnBuildingCapacityUpdated?.Invoke(project.ProjectId, facilityCapacity);
            
            LogInfo($"Created new facility: {facilityName} with capacity: {facilityCapacity}");
            return project;
        }
        
        /// <summary>
        /// Expands an existing facility by adding new rooms or structures
        /// </summary>
        public bool ExpandFacility(string projectId, List<ConstructionRoomTemplate> newRooms, float expansionBudget)
        {
            var project = GetProject(projectId);
            if (project == null)
            {
                LogWarning($"Project {projectId} not found for expansion");
                return false;
            }
            
            // Validate expansion feasibility
            var expansionValidation = ValidateExpansion(project, newRooms, expansionBudget);
            if (!expansionValidation.IsValid)
            {
                LogWarning($"Expansion validation failed: {string.Join(", ", expansionValidation.Errors)}");
                return false;
            }
            
            // Calculate expansion cost
            var expansionCost = CalculateExpansionCost(newRooms);
            if (expansionCost > expansionBudget)
            {
                LogWarning($"Expansion cost {expansionCost:C} exceeds budget {expansionBudget:C}");
                return false;
            }
            
            // Start expansion process
            OnFacilityExpansionStarted?.Invoke(projectId, expansionCost);
            
            // Add rooms to project
            foreach (var room in newRooms)
            {
                var plannedRoom = new PlannedRoom
                {
                    PlannedRoomId = System.Guid.NewGuid().ToString(),
                    RoomTemplate = room,
                    Status = RoomStatus.Planned,
                    EstimatedCost = CalculateRoomCost(room),
                    PlannedDate = System.DateTime.Now
                };
                
                project.PlannedRooms.Add(plannedRoom);
                OnRoomAdded?.Invoke(projectId, room);
            }
            
            // Update project costs
            project.EstimatedCost += expansionCost;
            project.EstimatedDuration += CalculateExpansionDuration(newRooms);
            
            // Update facility capacity
            var newCapacity = CalculateFacilityCapacity(project.FacilityTemplate);
            OnBuildingCapacityUpdated?.Invoke(projectId, newCapacity);
            
            // Create expansion tasks
            var expansionTasks = CreateExpansionTasks(project, newRooms);
            foreach (var task in expansionTasks)
            {
                _constructionQueue.Enqueue(task);
            }
            
            LogInfo($"Facility expansion started for project {project.ProjectName} with {newRooms.Count} new rooms");
            return true;
        }
        
        /// <summary>
        /// Removes a room from the facility and adjusts capacity
        /// </summary>
        public bool RemoveRoom(string projectId, string roomId)
        {
            var project = GetProject(projectId);
            if (project == null) return false;
            
            var roomToRemove = project.PlannedRooms.FirstOrDefault(r => r.PlannedRoomId == roomId);
            if (roomToRemove == null)
            {
                LogWarning($"Room {roomId} not found in project {projectId}");
                return false;
            }
            
            // Check if room is in use or has dependencies
            if (roomToRemove.Status == RoomStatus.InProgress || roomToRemove.Status == RoomStatus.Completed)
            {
                LogWarning($"Cannot remove room {roomId} - room is {roomToRemove.Status}");
                return false;
            }
            
            // Remove room
            project.PlannedRooms.Remove(roomToRemove);
            project.EstimatedCost -= roomToRemove.EstimatedCost;
            
            // Update facility capacity
            var newCapacity = CalculateFacilityCapacity(project.FacilityTemplate);
            OnBuildingCapacityUpdated?.Invoke(projectId, newCapacity);
            
            OnRoomRemoved?.Invoke(projectId, roomId);
            LogInfo($"Room {roomId} removed from project {project.ProjectName}");
            
            return true;
        }
        
        /// <summary>
        /// Modifies an existing room's configuration
        /// </summary>
        public bool ModifyRoom(string projectId, string roomId, ConstructionRoomTemplate newTemplate)
        {
            var project = GetProject(projectId);
            if (project == null) return false;
            
            var roomToModify = project.PlannedRooms.FirstOrDefault(r => r.PlannedRoomId == roomId);
            if (roomToModify == null)
            {
                LogWarning($"Room {roomId} not found in project {projectId}");
                return false;
            }
            
            // Check if room can be modified
            if (roomToModify.Status != RoomStatus.Planned)
            {
                LogWarning($"Cannot modify room {roomId} - room is {roomToModify.Status}");
                return false;
            }
            
            // Update room template and cost
            var oldCost = roomToModify.EstimatedCost;
            roomToModify.RoomTemplate = newTemplate;
            roomToModify.EstimatedCost = CalculateRoomCost(newTemplate);
            
            // Update project cost
            project.EstimatedCost += (roomToModify.EstimatedCost - oldCost);
            
            // Update facility capacity
            var newCapacity = CalculateFacilityCapacity(project.FacilityTemplate);
            OnBuildingCapacityUpdated?.Invoke(projectId, newCapacity);
            
            LogInfo($"Room {roomId} modified in project {project.ProjectName}");
            return true;
        }
        
        /// <summary>
        /// Gets facility information including capacity and utilization
        /// </summary>
        public FacilityInfo GetFacilityInfo(string projectId)
        {
            var project = GetProject(projectId);
            if (project == null) return null;
            
            var capacity = CalculateFacilityCapacity(project.FacilityTemplate);
            var utilization = CalculateFacilityUtilization(project);
            
            return new FacilityInfo
            {
                ProjectId = projectId,
                FacilityName = project.ProjectName,
                TotalCapacity = capacity,
                CurrentUtilization = utilization,
                UtilizationPercentage = capacity > 0 ? (utilization / capacity) * 100f : 0f,
                TotalRooms = project.PlannedRooms.Count,
                CompletedRooms = project.PlannedRooms.Count(r => r.Status == RoomStatus.Completed),
                InProgressRooms = project.PlannedRooms.Count(r => r.Status == RoomStatus.InProgress),
                PlannedRooms = project.PlannedRooms.Count(r => r.Status == RoomStatus.Planned),
                TotalArea = project.PlannedRooms.Sum(r => r.RoomTemplate.Area),
                EstimatedCapacity = CalculateEstimatedCapacity(project),
                PowerConsumption = CalculatePowerConsumption(project),
                WaterConsumption = CalculateWaterConsumption(project),
                MaintenanceCost = CalculateMaintenanceCost(project),
                OperationalCost = CalculateOperationalCost(project),
                ExpansionPotential = CalculateExpansionPotential(project)
            };
        }
        
        private ValidationResult ValidateExpansion(ConstructionProject project, List<ConstructionRoomTemplate> newRooms, float budget)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };
            
            // Check if project is in a state that allows expansion
            if (project.Status != ProjectStatus.Completed && project.Status != ProjectStatus.InProgress)
            {
                result.IsValid = false;
                result.Errors.Add("Project must be completed or in progress to allow expansion");
            }
            
            // Check budget
            var expansionCost = CalculateExpansionCost(newRooms);
            if (expansionCost > budget)
            {
                result.IsValid = false;
                result.Errors.Add($"Expansion cost {expansionCost:C} exceeds budget {budget:C}");
            }
            
            // Check site capacity
            var currentArea = project.PlannedRooms.Sum(r => r.RoomTemplate.Area);
            var newArea = newRooms.Sum(r => r.Area);
            var totalArea = currentArea + newArea;
            
            if (totalArea > GetMaximumSiteArea())
            {
                result.IsValid = false;
                result.Errors.Add($"Total area {totalArea} exceeds maximum site capacity");
            }
            
            return result;
        }
        
        private float CalculateExpansionCost(List<ConstructionRoomTemplate> newRooms)
        {
            float totalCost = 0f;
            
            foreach (var room in newRooms)
            {
                totalCost += CalculateRoomCost(room);
            }
            
            // Add expansion overhead (20%)
            totalCost *= 1.2f;
            
            return totalCost;
        }
        
        private int CalculateExpansionDuration(List<ConstructionRoomTemplate> newRooms)
        {
            int totalDays = 0;
            
            foreach (var room in newRooms)
            {
                // Base construction time: 3 days per 100 sqft
                totalDays += Mathf.CeilToInt(room.Area / 100f * 3f);
            }
            
            return totalDays;
        }
        
        private List<ConstructionTask> CreateExpansionTasks(ConstructionProject project, List<ConstructionRoomTemplate> newRooms)
        {
            var tasks = new List<ConstructionTask>();
            
            foreach (var room in newRooms)
            {
                // Create construction task for each room
                var task = new ConstructionTask
                {
                    TaskId = System.Guid.NewGuid().ToString(),
                    ProjectId = project.ProjectId,
                    TaskName = $"Expand - {room.RoomName}",
                    Description = $"Construct new {room.RoomType} room",
                    ConstructionPhase = ConstructionPhaseType.Structure,
                    EstimatedHours = CalculateRoomConstructionHours(room),
                    RequiredWorkerCount = DetermineRequiredWorkers(room),
                    RequiredSpecialty = DetermineRequiredSpecialty(room),
                    RequiredMaterials = DetermineRequiredMaterials(room),
                    Status = TaskStatus.Not_Started,
                    Priority = TaskPriority.Normal,
                    Cost = CalculateRoomCost(room)
                };
                
                tasks.Add(task);
            }
            
            return tasks;
        }
        
        private float CalculateRoomConstructionHours(ConstructionRoomTemplate room)
        {
            // Base: 8 hours per 100 sqft
            float baseHours = room.Area / 100f * 8f;
            
            // Complexity multiplier based on room type
            float complexityMultiplier = room.RoomType switch
            {
                "GrowRoom" => 1.8f,
                "ProcessingRoom" => 1.5f,
                "StorageRoom" => 1.0f,
                "Office" => 0.8f,
                "Utility" => 1.2f,
                _ => 1.0f
            };
            
            return baseHours * complexityMultiplier;
        }
        
        private int DetermineRequiredWorkers(ConstructionRoomTemplate room)
        {
            // Base worker count based on room size
            int baseWorkers = Mathf.CeilToInt(room.Area / 200f);
            
            // Minimum 2 workers for safety
            return Mathf.Max(2, baseWorkers);
        }
        
        private WorkerSpecialty DetermineRequiredSpecialty(ConstructionRoomTemplate room)
        {
            return room.RoomType switch
            {
                "GrowRoom" => WorkerSpecialty.HVAC,
                "ProcessingRoom" => WorkerSpecialty.Electrical,
                "StorageRoom" => WorkerSpecialty.GeneralConstruction,
                "Office" => WorkerSpecialty.GeneralConstruction,
                "Utility" => WorkerSpecialty.Plumbing,
                _ => WorkerSpecialty.GeneralConstruction
            };
        }
        
        private List<MaterialRequirement> DetermineRequiredMaterials(ConstructionRoomTemplate room)
        {
            var materials = new List<MaterialRequirement>();
            
            // Basic materials for all rooms
            materials.Add(new MaterialRequirement
            {
                MaterialName = "Concrete",
                RequiredQuantity = room.Area * 0.5f,
                UnitCost = 120f
            });
            
            materials.Add(new MaterialRequirement
            {
                MaterialName = "Steel Framing",
                RequiredQuantity = room.Area * 0.3f,
                UnitCost = 850f
            });
            
            materials.Add(new MaterialRequirement
            {
                MaterialName = "Drywall",
                RequiredQuantity = room.Area * 1.2f,
                UnitCost = 45f
            });
            
            // Room-specific materials
            if (room.RoomType == "GrowRoom")
            {
                materials.Add(new MaterialRequirement
                {
                    MaterialName = "Reflective Insulation",
                    RequiredQuantity = room.Area * 0.8f,
                    UnitCost = 95f
                });
                
                materials.Add(new MaterialRequirement
                {
                    MaterialName = "Ventilation Ducting",
                    RequiredQuantity = room.Area * 0.2f,
                    UnitCost = 125f
                });
            }
            
            return materials;
        }
        
        private float CalculateFacilityCapacity(FacilityTemplate template)
        {
            if (template?.RoomTemplates == null) return 0f;
            
            float totalCapacity = 0f;
            
            foreach (var room in template.RoomTemplates)
            {
                // Cannabis-specific capacity calculation
                if (room.RoomType == "GrowRoom")
                {
                    // Assume 1 plant per 4 sqft in grow rooms
                    totalCapacity += room.Area / 4f;
                }
                else if (room.RoomType == "ProcessingRoom")
                {
                    // Processing capacity in kg/day
                    totalCapacity += room.Area / 10f;
                }
                else if (room.RoomType == "StorageRoom")
                {
                    // Storage capacity in kg
                    totalCapacity += room.Area / 2f;
                }
            }
            
            return totalCapacity;
        }
        
        private float CalculateFacilityUtilization(ConstructionProject project)
        {
            // This would integrate with plant management system
            // For now, return estimated utilization
            return CalculateFacilityCapacity(project.FacilityTemplate) * 0.75f;
        }
        
        private float CalculateEstimatedCapacity(ConstructionProject project)
        {
            return project.PlannedRooms.Sum(r => r.RoomTemplate.Area / 4f);
        }
        
        private float CalculatePowerConsumption(ConstructionProject project)
        {
            float totalPower = 0f;
            
            foreach (var room in project.PlannedRooms)
            {
                if (room.RoomTemplate.RoomType == "GrowRoom")
                {
                    // LED lights: 35W per sqft
                    totalPower += room.RoomTemplate.Area * 35f;
                }
                else if (room.RoomTemplate.RoomType == "ProcessingRoom")
                {
                    // Processing equipment: 15W per sqft
                    totalPower += room.RoomTemplate.Area * 15f;
                }
                else
                {
                    // General power: 5W per sqft
                    totalPower += room.RoomTemplate.Area * 5f;
                }
            }
            
            return totalPower;
        }
        
        private float CalculateWaterConsumption(ConstructionProject project)
        {
            float totalWater = 0f;
            
            foreach (var room in project.PlannedRooms)
            {
                if (room.RoomTemplate.RoomType == "GrowRoom")
                {
                    // Water: 2 gallons per sqft per day
                    totalWater += room.RoomTemplate.Area * 2f;
                }
                else if (room.RoomTemplate.RoomType == "ProcessingRoom")
                {
                    // Processing water: 0.5 gallons per sqft per day
                    totalWater += room.RoomTemplate.Area * 0.5f;
                }
            }
            
            return totalWater;
        }
        
        private float CalculateMaintenanceCost(ConstructionProject project)
        {
            // Annual maintenance cost: 2% of construction cost
            return project.EstimatedCost * 0.02f;
        }
        
        private float CalculateOperationalCost(ConstructionProject project)
        {
            float powerCost = CalculatePowerConsumption(project) * 0.12f * 24f * 365f; // $0.12/kWh
            float waterCost = CalculateWaterConsumption(project) * 0.003f * 365f; // $0.003/gallon
            float maintenanceCost = CalculateMaintenanceCost(project);
            
            return powerCost + waterCost + maintenanceCost;
        }
        
        private float CalculateExpansionPotential(ConstructionProject project)
        {
            float currentArea = project.PlannedRooms.Sum(r => r.RoomTemplate.Area);
            float maxArea = GetMaximumSiteArea();
            
            return maxArea - currentArea;
        }
        
        private float GetMaximumSiteArea()
        {
            // Maximum site area in sqft (configurable)
            return 50000f;
        }
        
        #endregion
        
        #region Public Interface
        
        public List<ConstructionProject> GetProjectsByStatus(ProjectStatus status)
        {
            return _allProjects.Where(p => p.Status == status).ToList();
        }
        
        public ConstructionProject GetProject(string projectId)
        {
            return _allProjects.FirstOrDefault(p => p.ProjectId == projectId);
        }
        
        public List<ConstructionWorker> GetAvailableWorkers(WorkerSpecialty specialty = WorkerSpecialty.GeneralConstruction)
        {
            return _workforce.GetAvailableWorkers(specialty);
        }
        
        public bool HasRequiredMaterials(List<MaterialRequirement> materials)
        {
            return _materialInventory.HasMaterials(materials);
        }
        
        public void AddMaterials(string materialType, float quantity, float costPerUnit)
        {
            _materialInventory.AddMaterial(materialType, quantity, costPerUnit);
        }
        
        public void SetConstructionSpeed(float speedMultiplier)
        {
            _constructionSpeedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 5f);
        }
        
        public void PauseConstruction(string projectId)
        {
            var project = GetProject(projectId);
            if (project != null)
            {
                project.Status = ProjectStatus.Paused;
                
                // Remove project tasks from queue
                var projectTasks = _constructionQueue.Where(t => t.ProjectId == projectId).ToList();
                _constructionQueue = new Queue<ConstructionTask>(_constructionQueue.Except(projectTasks));
            }
        }
        
        public void ResumeConstruction(string projectId)
        {
            var project = GetProject(projectId);
            if (project != null && project.Status == ProjectStatus.Paused)
            {
                project.Status = ProjectStatus.InProgress;
                
                // Re-add project tasks to queue
                var tasks = _constructionPlanner.CreateTasksForPhase(project, project.CurrentPhase);
                foreach (var task in tasks.Where(t => !project.CompletedTasks.Contains(t.TaskId)))
                {
                    _constructionQueue.Enqueue(task);
                }
            }
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            CancelInvoke();
            ClearPreview();
            
            LogInfo("Interactive Facility Constructor shutdown complete");
        }
    }
}