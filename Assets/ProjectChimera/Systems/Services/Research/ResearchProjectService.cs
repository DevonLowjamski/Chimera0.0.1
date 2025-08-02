using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Research;
using ResourceRequirements = ProjectChimera.Data.Research.ResourceRequirements;
using ProjectChimera.Systems.Registry;
using ResearchProject = ProjectChimera.Data.Research.ResearchProject;
using ActiveResearchProject = ProjectChimera.Data.Research.ActiveResearchProject;
using ResearchProjectOffer = ProjectChimera.Data.Research.ResearchProjectOffer;
using ResearchStatus = ProjectChimera.Data.Research.ResearchStatus;
using ResearchCategory = ProjectChimera.Data.Research.ResearchCategory;
using ResearchEvent = ProjectChimera.Data.Research.ResearchEvent;
using ResearchEventType = ProjectChimera.Data.Research.ResearchEventType;

namespace ProjectChimera.Systems.Services.Research
{
    /// <summary>
    /// PC014-2a: Research Project Service
    /// Individual project management and progress tracking
    /// Decomposed from ResearchManager (460 lines target)
    /// </summary>
    public class ResearchProjectService : MonoBehaviour, IResearchProjectService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Project Configuration")]
        [SerializeField] private bool _enableAutoProgress = true;
        [SerializeField] private float _progressUpdateInterval = 1f;
        [Range(1, 10)] [SerializeField] private int _maxActiveProjects = 3;
        [Range(5, 50)] [SerializeField] private int _maxAvailableProjects = 15;
        
        [Header("Active Data")]
        [SerializeField] private List<ActiveResearchProject> _activeProjects = new List<ActiveResearchProject>();
        [SerializeField] private List<ResearchProjectOffer> _availableOffers = new List<ResearchProjectOffer>();
        [SerializeField] private Dictionary<string, float> _projectProgress = new Dictionary<string, float>();
        [SerializeField] private List<ResearchEvent> _projectHistory = new List<ResearchEvent>();
        
        private float _timeSinceLastUpdate;
        private IResearchResourceService _resourceService;
        private ITechnologyTreeService _technologyService;
        
        #endregion

        #region Events
        
        public event Action<string> OnProjectStarted;
        public event Action<string> OnProjectCompleted;
        public event Action<string, float> OnProgressUpdated;
        public event Action<string> OnProjectFailed;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing ResearchProjectService...");
            
            // Get service dependencies
            _resourceService = ServiceRegistry.Instance.GetService<IResearchResourceService>();
            _technologyService = ServiceRegistry.Instance.GetService<ITechnologyTreeService>();
            
            // Initialize project system
            InitializeProjectSystem();
            
            // Load existing projects
            LoadExistingProjects();
            
            // Generate initial offers
            GenerateInitialProjectOffers();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IResearchProjectService>(this, ServiceDomain.Research);
            
            IsInitialized = true;
            Debug.Log("ResearchProjectService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down ResearchProjectService...");
            
            // Save project state
            SaveProjectState();
            
            // Clear collections
            _activeProjects.Clear();
            _availableOffers.Clear();
            _projectProgress.Clear();
            _projectHistory.Clear();
            
            _resourceService = null;
            _technologyService = null;
            
            IsInitialized = false;
            Debug.Log("ResearchProjectService shutdown complete");
        }
        
        #endregion

        #region Project Management
        
        public string CreateProject(string name, ResearchCategory category, ResearchRequirements requirements)
        {
            if (!IsInitialized)
            {
                Debug.LogError("ResearchProjectService not initialized");
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Project name cannot be empty");
                return null;
            }

            string projectId = GenerateProjectId();
            
            var project = new ResearchProject
            {
                ProjectId = projectId,
                ProjectName = name,
                Category = category,
                Requirements = requirements,
                Phases = GenerateDefaultPhases(category),
                Milestones = GenerateDefaultMilestones(category),
                EstimatedDuration = CalculateEstimatedDuration(requirements),
                SuccessProbability = CalculateSuccessProbability(requirements),
                Metadata = new Dictionary<string, object>()
            };

            var offer = new ResearchProjectOffer
            {
                ResearchProject = project,
                OfferValidUntil = Time.time + 7 * 24 * 60 * 60, // 7 days
                Priority = DeterminePriority(category),
                IsRecommended = true,
                RecommendationReasons = new List<string> { "Custom project created by player" }
            };

            _availableOffers.Add(offer);
            
            Debug.Log($"Created research project: {name} (ID: {projectId})");
            return projectId;
        }

        public bool StartProject(string projectId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("ResearchProjectService not initialized");
                return false;
            }

            if (_activeProjects.Count >= _maxActiveProjects)
            {
                Debug.LogWarning("Cannot start project: Maximum active projects reached");
                return false;
            }

            var offer = _availableOffers.FirstOrDefault(o => o.ResearchProject.ProjectId == projectId);
            if (offer == null)
            {
                Debug.LogError($"Project offer not found: {projectId}");
                return false;
            }

            // Validate resources
            if (_resourceService != null && !_resourceService.ValidateResources(projectId))
            {
                Debug.LogWarning("Insufficient resources to start project");
                return false;
            }

            var activeProject = new ActiveResearchProject
            {
                ResearchProject = offer.ResearchProject,
                Status = ResearchStatus.Active,
                StartDate = DateTime.Now,
                CurrentPhaseIndex = 0,
                OverallProgress = 0f,
                CompletedPhases = new List<CompletedPhase>(),
                CompletedMilestones = new List<CompletedMilestone>(),
                TotalInvestment = 0f,
                CurrentQuality = 0.7f,
                TeamExpertise = 1.0f,
                HadSetbacks = false,
                ProjectHistory = new List<ResearchEvent>()
            };

            _activeProjects.Add(activeProject);
            _availableOffers.Remove(offer);
            _projectProgress[projectId] = 0f;

            // Allocate resources
            if (_resourceService != null)
            {
                _resourceService.ConsumeResources(projectId);
            }

            RecordProjectEvent(projectId, ResearchEventType.Project_Started, "Project started successfully");
            
            OnProjectStarted?.Invoke(projectId);
            Debug.Log($"Started research project: {offer.ResearchProject.ProjectName}");
            
            return true;
        }

        public bool CompleteProject(string projectId)
        {
            var activeProject = _activeProjects.FirstOrDefault(p => p.ResearchProject.ProjectId == projectId);
            if (activeProject == null)
            {
                Debug.LogError($"Active project not found: {projectId}");
                return false;
            }

            if (!IsProjectComplete(projectId))
            {
                Debug.LogError($"Project {projectId} is not ready for completion");
                return false;
            }

            activeProject.Status = ResearchStatus.Completed;
            
            // Process completion rewards
            ProcessProjectCompletion(activeProject);
            
            // Move to completed projects
            _activeProjects.Remove(activeProject);
            _projectProgress.Remove(projectId);

            RecordProjectEvent(projectId, ResearchEventType.Project_Completed, "Project completed successfully");
            
            OnProjectCompleted?.Invoke(projectId);
            Debug.Log($"Completed research project: {activeProject.ResearchProject.ProjectName}");
            
            return true;
        }

        public ResearchProject GetProject(string projectId)
        {
            var activeProject = _activeProjects.FirstOrDefault(p => p.ResearchProject.ProjectId == projectId);
            if (activeProject != null)
            {
                return activeProject.ResearchProject;
            }

            var offer = _availableOffers.FirstOrDefault(o => o.ResearchProject.ProjectId == projectId);
            return offer?.ResearchProject;
        }

        public List<ResearchProject> GetActiveProjects()
        {
            return _activeProjects.Select(ap => ap.ResearchProject).ToList();
        }
        
        #endregion

        #region Progress Tracking
        
        public void UpdateProgress(string projectId, float progressDelta)
        {
            if (!_projectProgress.ContainsKey(projectId))
            {
                Debug.LogWarning($"Project progress not found: {projectId}");
                return;
            }

            var activeProject = _activeProjects.FirstOrDefault(p => p.ResearchProject.ProjectId == projectId);
            if (activeProject == null)
            {
                Debug.LogWarning($"Active project not found: {projectId}");
                return;
            }

            float currentProgress = _projectProgress[projectId];
            float newProgress = Mathf.Clamp01(currentProgress + progressDelta);
            
            _projectProgress[projectId] = newProgress;
            activeProject.OverallProgress = newProgress;

            // Check for phase completion
            CheckPhaseCompletion(activeProject);
            
            // Check for milestone achievement
            CheckMilestoneCompletion(activeProject);

            OnProgressUpdated?.Invoke(projectId, newProgress);
            
            if (newProgress >= 1.0f)
            {
                CompleteProject(projectId);
            }
        }

        public float GetProgress(string projectId)
        {
            if (_projectProgress.ContainsKey(projectId))
            {
                return _projectProgress[projectId];
            }
            
            return 0f;
        }

        public TimeSpan GetEstimatedTimeToCompletion(string projectId)
        {
            var activeProject = _activeProjects.FirstOrDefault(p => p.ResearchProject.ProjectId == projectId);
            if (activeProject == null)
                return TimeSpan.Zero;

            float currentProgress = GetProgress(projectId);
            if (currentProgress <= 0f)
                return TimeSpan.FromDays(activeProject.ResearchProject.EstimatedDuration);

            float remainingProgress = 1f - currentProgress;
            float remainingDays = activeProject.ResearchProject.EstimatedDuration * remainingProgress;
            
            return TimeSpan.FromDays(remainingDays);
        }

        public bool IsProjectComplete(string projectId)
        {
            return GetProgress(projectId) >= 1.0f;
        }
        
        #endregion

        #region Resource Management
        
        public bool ValidateResources(string projectId)
        {
            var project = GetProject(projectId);
            if (project == null)
                return false;

            // Delegate to resource service
            return _resourceService?.ValidateResources(projectId) ?? true;
        }

        public ResearchRequirements GetResourceRequirements(string projectId)
        {
            var project = GetProject(projectId);
            return project?.Requirements;
        }

        public void ConsumeResources(string projectId)
        {
            _resourceService?.ConsumeResources(projectId);
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeProjectSystem()
        {
            _projectProgress = new Dictionary<string, float>();
            _projectHistory = new List<ResearchEvent>();
            Debug.Log("Project system initialized");
        }

        private void LoadExistingProjects()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing projects...");
        }

        private void SaveProjectState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving project state...");
        }

        private void GenerateInitialProjectOffers()
        {
            // Generate basic starter projects
            for (int i = 0; i < 5; i++)
            {
                var project = GenerateRandomProject();
                var offer = new ResearchProjectOffer
                {
                    ResearchProject = project,
                    OfferValidUntil = Time.time + 30 * 24 * 60 * 60, // 30 days
                    Priority = 0.5f,
                    IsRecommended = i < 2,
                    RecommendationReasons = new List<string> { "Suitable for beginners" }
                };
                
                _availableOffers.Add(offer);
            }
            
            Debug.Log($"Generated {_availableOffers.Count} initial project offers");
        }

        private ResearchProject GenerateRandomProject()
        {
            var categories = Enum.GetValues(typeof(ResearchCategory)).Cast<ResearchCategory>().ToArray();
            var category = categories[UnityEngine.Random.Range(0, categories.Length)];
            
            return new ResearchProject
            {
                ProjectId = GenerateProjectId(),
                ProjectName = $"Research Project {category}",
                Category = category,
                Phases = GenerateDefaultPhases(category),
                Milestones = GenerateDefaultMilestones(category),
                EstimatedDuration = UnityEngine.Random.Range(7f, 30f),
                SuccessProbability = UnityEngine.Random.Range(0.6f, 0.9f),
                Metadata = new Dictionary<string, object>()
            };
        }

        private List<ResearchPhase> GenerateDefaultPhases(ResearchCategory category)
        {
            return new List<ResearchPhase>
            {
                new ResearchPhase
                {
                    PhaseId = "phase_1",
                    PhaseName = "Planning",
                    Duration = 3f,
                    CompletionCriteria = 0.25f
                },
                new ResearchPhase
                {
                    PhaseId = "phase_2", 
                    PhaseName = "Execution",
                    Duration = 15f,
                    CompletionCriteria = 0.75f
                },
                new ResearchPhase
                {
                    PhaseId = "phase_3",
                    PhaseName = "Analysis",
                    Duration = 5f,
                    CompletionCriteria = 1.0f
                }
            };
        }

        private List<ResearchMilestone> GenerateDefaultMilestones(ResearchCategory category)
        {
            return new List<ResearchMilestone>
            {
                new ResearchMilestone
                {
                    MilestoneId = "milestone_1",
                    Name = "Initial Results",
                    Progress = 0.33f
                },
                new ResearchMilestone
                {
                    MilestoneId = "milestone_2",
                    Name = "Mid-point Analysis",
                    Progress = 0.66f
                },
                new ResearchMilestone
                {
                    MilestoneId = "milestone_3",
                    Name = "Final Results",
                    Progress = 1.0f
                }
            };
        }

        private string GenerateProjectId()
        {
            return $"PROJ_{DateTime.Now:yyyyMMdd}_{UnityEngine.Random.Range(1000, 9999)}";
        }

        private float CalculateEstimatedDuration(ResourceRequirements requirements)
        {
            // Base duration calculation
            return UnityEngine.Random.Range(7f, 30f);
        }

        private float CalculateSuccessProbability(ResourceRequirements requirements)
        {
            // Base success probability calculation
            return UnityEngine.Random.Range(0.6f, 0.9f);
        }

        private float DeterminePriority(ResearchCategory category)
        {
            return category switch
            {
                ResearchCategory.Genetics => 0.9f,
                ResearchCategory.Cultivation => 0.8f,
                ResearchCategory.Processing => 0.7f,
                _ => 0.5f
            };
        }

        private void CheckPhaseCompletion(ActiveResearchProject activeProject)
        {
            var currentPhase = activeProject.ResearchProject.Phases.ElementAtOrDefault(activeProject.CurrentPhaseIndex);
            if (currentPhase == null) return;

            if (activeProject.OverallProgress >= currentPhase.CompletionCriteria)
            {
                var completedPhase = new CompletedPhase
                {
                    PhaseId = currentPhase.PhaseId,
                    CompletionDate = DateTime.Now,
                    ActualDuration = currentPhase.Duration,
                    QualityScore = activeProject.CurrentQuality
                };

                activeProject.CompletedPhases.Add(completedPhase);
                activeProject.CurrentPhaseIndex++;

                RecordProjectEvent(activeProject.ResearchProject.ProjectId, 
                    ResearchEventType.Phase_Completed, 
                    $"Completed phase: {currentPhase.PhaseName}");
            }
        }

        private void CheckMilestoneCompletion(ActiveResearchProject activeProject)
        {
            foreach (var milestone in activeProject.ResearchProject.Milestones)
            {
                if (!activeProject.CompletedMilestones.Any(cm => cm.MilestoneId == milestone.MilestoneId) &&
                    activeProject.OverallProgress >= milestone.Progress)
                {
                    var completedMilestone = new CompletedMilestone
                    {
                        MilestoneId = milestone.MilestoneId,
                        CompletionDate = DateTime.Now,
                        QualityScore = activeProject.CurrentQuality
                    };

                    activeProject.CompletedMilestones.Add(completedMilestone);

                    RecordProjectEvent(activeProject.ResearchProject.ProjectId,
                        ResearchEventType.Milestone_Achieved,
                        $"Achieved milestone: {milestone.Name}");
                }
            }
        }

        private void ProcessProjectCompletion(ActiveResearchProject activeProject)
        {
            // Unlock technologies through technology service
            if (_technologyService != null)
            {
                foreach (var techUnlock in activeProject.ResearchProject.PotentialUnlocks)
                {
                    _technologyService.UnlockTechnology(techUnlock.TechnologyId);
                }
            }

            Debug.Log($"Processed completion for project: {activeProject.ResearchProject.ProjectName}");
        }

        private void RecordProjectEvent(string projectId, ResearchEventType eventType, string description)
        {
            var researchEvent = new ResearchEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = eventType,
                Timestamp = DateTime.Now,
                Description = description,
                EventData = new Dictionary<string, object> { ["ProjectId"] = projectId }
            };

            _projectHistory.Add(researchEvent);

            // Keep history manageable
            if (_projectHistory.Count > 1000)
            {
                _projectHistory.RemoveRange(0, 100);
            }
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Update()
        {
            if (!IsInitialized || !_enableAutoProgress) return;

            _timeSinceLastUpdate += Time.deltaTime;

            if (_timeSinceLastUpdate >= _progressUpdateInterval)
            {
                UpdateActiveProjectsProgress();
                _timeSinceLastUpdate = 0f;
            }
        }

        private void UpdateActiveProjectsProgress()
        {
            foreach (var activeProject in _activeProjects.ToList())
            {
                // Simulate research progress
                float progressDelta = CalculateProgressDelta(activeProject);
                UpdateProgress(activeProject.ResearchProject.ProjectId, progressDelta);
            }
        }

        private float CalculateProgressDelta(ActiveResearchProject activeProject)
        {
            // Simple progress calculation
            float baseProgress = 0.001f; // 0.1% per update
            float efficiencyMultiplier = activeProject.TeamExpertise * activeProject.CurrentQuality;
            
            return baseProgress * efficiencyMultiplier;
        }
        
        #endregion
    }
}