using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages the research system including project lifecycle, technology unlocks, 
    /// collaboration opportunities, and research-driven progression in the cannabis 
    /// cultivation simulation.
    /// </summary>
    public class ResearchManager : ChimeraManager
    {
        [Header("Research Configuration")]
        [SerializeField] private List<ResearchProjectSO> _availableResearchProjects = new List<ResearchProjectSO>();
        [SerializeField] private ResearchSettings _researchSettings;
        [SerializeField] private float _researchUpdateInterval = 1f; // In-game days
        [SerializeField] private int _maxActiveProjects = 3;
        [SerializeField] private int _maxAvailableProjects = 15;
        
        [Header("Player Research Capabilities")]
        [SerializeField] private PlayerResearchCapabilities _playerCapabilities;
        [SerializeField] private ResearchFacilityLevel _facilityLevel = ResearchFacilityLevel.Basic;
        [SerializeField] private float _researchEfficiencyMultiplier = 1f;
        
        [Header("Discovery and Innovation")]
        [SerializeField] private DiscoverySettings _discoverySettings;
        [SerializeField] private List<TechnologyTree> _technologyTrees = new List<TechnologyTree>();
        [SerializeField] private List<InnovationOpportunity> _innovationOpportunities = new List<InnovationOpportunity>();
        
        [Header("Collaboration System")]
        [SerializeField] private CollaborationSettings _collaborationSettings;
        [SerializeField] private List<CollaborationOpportunity> _availableCollaborations = new List<CollaborationOpportunity>();
        [SerializeField] private List<ResearchPartnership> _activePartnerships = new List<ResearchPartnership>();
        
        [Header("Events")]
        [SerializeField] private SimpleGameEventSO _researchStartedEvent;
        [SerializeField] private SimpleGameEventSO _researchCompletedEvent;
        [SerializeField] private SimpleGameEventSO _technologyUnlockedEvent;
        [SerializeField] private SimpleGameEventSO _discoveryMadeEvent;
        [SerializeField] private SimpleGameEventSO _collaborationEvent;
        
        // Runtime Data
        private List<ActiveResearchProject> _activeProjects;
        private List<ResearchProjectOffer> _availableOffers;
        private Dictionary<TechnologyType, List<UnlockedTechnology>> _unlockedTechnologies;
        private Dictionary<ResearchCategory, float> _researchExperience;
        private Queue<ResearchEvent> _recentEvents;
        private List<ResearchBreakthrough> _breakthroughs;
        private float _timeSinceLastUpdate;
        
        public List<ActiveResearchProject> ActiveProjects => _activeProjects;
        public List<ResearchProjectOffer> AvailableOffers => _availableOffers;
        public PlayerResearchCapabilities PlayerCapabilities => _playerCapabilities;
        public List<ResearchBreakthrough> Breakthroughs => _breakthroughs;
        
        // Events
        public System.Action<ResearchProjectSO> OnResearchStarted;
        public System.Action<ActiveResearchProject, ResearchResults> OnResearchCompleted;
        public System.Action<TechnologyUnlock> OnTechnologyUnlocked;
        public System.Action<PotentialDiscovery> OnDiscoveryMade;
        public System.Action<CollaborationOpportunity> OnCollaborationOpportunityAvailable;
        
        protected override void OnManagerInitialize()
        {
            _activeProjects = new List<ActiveResearchProject>();
            _availableOffers = new List<ResearchProjectOffer>();
            _unlockedTechnologies = new Dictionary<TechnologyType, List<UnlockedTechnology>>();
            _researchExperience = new Dictionary<ResearchCategory, float>();
            _recentEvents = new Queue<ResearchEvent>();
            _breakthroughs = new List<ResearchBreakthrough>();
            
            InitializePlayerCapabilities();
            InitializeResearchExperience();
            InitializeTechnologyTrees();
            GenerateInitialResearchOffers();
            
            Debug.Log("ResearchManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup resources
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastUpdate += Time.deltaTime;
            
            float gameTimeDelta = GameManager.Instance.GetManager<TimeManager>().GetScaledDeltaTime();
            
            if (_timeSinceLastUpdate >= _researchUpdateInterval * gameTimeDelta)
            {
                UpdateActiveResearchProjects();
                UpdateResearchOffers();
                ProcessResearchEvents();
                UpdateCollaborationOpportunities();
                CheckForBreakthroughs();
                GenerateNewResearchOpportunities();
                
                _timeSinceLastUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Starts a new research project.
        /// </summary>
        public bool StartResearchProject(ResearchProjectSO project)
        {
            if (_activeProjects.Count >= _maxActiveProjects)
            {
                Debug.LogWarning("Cannot start research: Maximum active projects reached");
                return false;
            }
            
            // Check feasibility
            var feasibility = project.EvaluateResearchFeasibility(_playerCapabilities);
            if (feasibility.OverallFeasibility < _researchSettings.MinimumFeasibilityThreshold)
            {
                Debug.LogWarning($"Research project {project.ProjectName} not feasible (score: {feasibility.OverallFeasibility:F2})");
                return false;
            }
            
            // Create active research project
            var activeProject = new ActiveResearchProject
            {
                ResearchProject = project,
                Status = ResearchStatus.Active,
                StartDate = System.DateTime.Now,
                CurrentPhaseIndex = 0,
                CompletedPhases = new List<CompletedPhase>(),
                CompletedMilestones = new List<CompletedMilestone>(),
                TotalInvestment = 0f,
                CurrentQuality = 0.7f, // Starting quality
                TeamExpertise = CalculateTeamExpertise(project),
                HadSetbacks = false
            };
            
            // Apply resource costs
            if (!ApplyResearchCosts(project))
            {
                Debug.LogWarning("Insufficient resources to start research project");
                return false;
            }
            
            _activeProjects.Add(activeProject);
            
            // Remove from available offers
            _availableOffers.RemoveAll(offer => offer.ResearchProject == project);
            
            OnResearchStarted?.Invoke(project);
            _researchStartedEvent?.Raise();
            
            RecordResearchEvent(new ResearchEvent
            {
                EventType = ResearchEventType.Project_Started,
                Project = project,
                Timestamp = System.DateTime.Now,
                Description = $"Started research project: {project.ProjectName}"
            });
            
            return true;
        }
        
        /// <summary>
        /// Gets research projects available to the player.
        /// </summary>
        public List<ResearchProjectOffer> GetAvailableResearchProjects(ResearchCategory category = ResearchCategory.Genetics)
        {
            if (category == ResearchCategory.Genetics) // Treat as "All" categories
                return _availableOffers;
            
            return _availableOffers.Where(offer => offer.ResearchProject.ResearchCategory == category).ToList();
        }
        
        /// <summary>
        /// Gets unlocked technologies by type.
        /// </summary>
        public List<UnlockedTechnology> GetUnlockedTechnologies(TechnologyType technologyType)
        {
            return _unlockedTechnologies.ContainsKey(technologyType) ? 
                _unlockedTechnologies[technologyType] : new List<UnlockedTechnology>();
        }
        
        /// <summary>
        /// Checks if a specific technology is unlocked.
        /// </summary>
        public bool IsTechnologyUnlocked(string technologyName)
        {
            foreach (var techList in _unlockedTechnologies.Values)
            {
                if (techList.Any(tech => tech.TechnologyUnlock.TechnologyName == technologyName))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets research experience in a specific category.
        /// </summary>
        public float GetResearchExperience(ResearchCategory category)
        {
            return _researchExperience.ContainsKey(category) ? _researchExperience[category] : 0f;
        }
        
        /// <summary>
        /// Applies a research boost to accelerate current projects.
        /// </summary>
        public bool ApplyResearchBoost(ResearchBoost boost)
        {
            // Check if boost requirements are met
            if (!AreBoostRequirementsMet(boost))
                return false;
            
            // Apply boost to active projects
            foreach (var project in _activeProjects)
            {
                if (boost.ApplicableCategories.Contains(project.ResearchProject.ResearchCategory))
                {
                    project.ResearchSpeedMultiplier *= boost.SpeedMultiplier;
                    project.QualityBonusMultiplier *= boost.QualityMultiplier;
                    project.SuccessProbabilityBonus += boost.SuccessProbabilityBonus;
                    
                    // Apply boost duration
                    project.ActiveBoosts.Add(new ActiveResearchBoost
                    {
                        Boost = boost,
                        StartDate = System.DateTime.Now,
                        ExpirationDate = System.DateTime.Now.AddDays(boost.DurationDays),
                        RemainingUses = boost.UsageLimit
                    });
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Initiates a collaboration with another research entity.
        /// </summary>
        public bool InitiateCollaboration(CollaborationOpportunity opportunity)
        {
            // Check prerequisites
            if (!AreCollaborationRequirementsMet(opportunity))
                return false;
            
            var partnership = new ResearchPartnership
            {
                CollaborationOpportunity = opportunity,
                StartDate = System.DateTime.Now,
                Status = CollaborationStatus.Active,
                ContributionLevel = CalculatePlayerContribution(opportunity),
                RelationshipStrength = 0.5f, // Starting relationship
                SharedProjects = new List<ActiveResearchProject>()
            };
            
            // Apply collaboration benefits to relevant projects
            ApplyCollaborationBenefits(partnership);
            
            _activePartnerships.Add(partnership);
            OnCollaborationOpportunityAvailable?.Invoke(opportunity);
            _collaborationEvent?.Raise();
            
            return true;
        }
        
        /// <summary>
        /// Gets recommendations for next research projects based on player progression.
        /// </summary>
        public List<ResearchProjectSO> GetRecommendedResearchProjects(int maxRecommendations = 5)
        {
            var recommendations = new List<ResearchRecommendation>();
            
            foreach (var offer in _availableOffers)
            {
                var project = offer.ResearchProject;
                float score = CalculateResearchRecommendationScore(project);
                
                recommendations.Add(new ResearchRecommendation
                {
                    Project = project,
                    Score = score,
                    Reasoning = GenerateRecommendationReasoning(project, score)
                });
            }
            
            return recommendations
                .OrderByDescending(r => r.Score)
                .Take(maxRecommendations)
                .Select(r => r.Project)
                .ToList();
        }
        
        /// <summary>
        /// Gets technology unlocks that would be enabled by completing a research project.
        /// </summary>
        public List<TechnologyPreview> GetPotentialTechnologyUnlocks(ResearchProjectSO project)
        {
            var previews = new List<TechnologyPreview>();
            
            foreach (var techUnlock in project.TechnologyUnlocks)
            {
                previews.Add(new TechnologyPreview
                {
                    TechnologyUnlock = techUnlock,
                    UnlockProbability = techUnlock.UnlockProbability,
                    EstimatedImpact = CalculateTechnologyImpact(techUnlock),
                    PrerequisitesMet = AreTechnologyPrerequisitesMet(techUnlock)
                });
            }
            
            return previews;
        }
        
        private void InitializePlayerCapabilities()
        {
            if (_playerCapabilities == null)
            {
                _playerCapabilities = new PlayerResearchCapabilities
                {
                    AvailableBudget = _researchSettings.StartingResearchBudget,
                    AvailableResearchTime = _researchSettings.StartingResearchTime,
                    CanManageParallelProjects = false,
                    SkillLevels = new Dictionary<SkillNodeSO, int>(),
                    AvailableEquipment = new List<EquipmentDataSO>(),
                    AvailableResources = new Dictionary<string, int>()
                };
            }
        }
        
        private void InitializeResearchExperience()
        {
            foreach (ResearchCategory category in System.Enum.GetValues(typeof(ResearchCategory)))
            {
                _researchExperience[category] = 0f;
            }
        }
        
        private void InitializeTechnologyTrees()
        {
            foreach (TechnologyType techType in System.Enum.GetValues(typeof(TechnologyType)))
            {
                _unlockedTechnologies[techType] = new List<UnlockedTechnology>();
            }
        }
        
        private void GenerateInitialResearchOffers()
        {
            int offersToGenerate = Mathf.Min(_maxAvailableProjects, _availableResearchProjects.Count);
            
            for (int i = 0; i < offersToGenerate; i++)
            {
                if (i < _availableResearchProjects.Count)
                {
                    var project = _availableResearchProjects[i];
                    var feasibility = project.EvaluateResearchFeasibility(_playerCapabilities);
                    
                    // Only offer projects that have some feasibility
                    if (feasibility.OverallFeasibility >= 0.2f)
                    {
                        var offer = new ResearchProjectOffer
                        {
                            ResearchProject = project,
                            OfferedDate = System.DateTime.Now,
                            ExpirationDate = System.DateTime.Now.AddDays(Random.Range(30, 90)),
                            Priority = CalculateOfferPriority(project),
                            Feasibility = feasibility,
                            EstimatedDuration = project.Timeline.EstimatedDurationDays,
                            EstimatedCost = project.Requirements.TotalBudgetRequired
                        };
                        
                        _availableOffers.Add(offer);
                    }
                }
            }
        }
        
        private void UpdateActiveResearchProjects()
        {
            for (int i = _activeProjects.Count - 1; i >= 0; i--)
            {
                var project = _activeProjects[i];
                
                // Update project progress
                UpdateProjectProgress(project);
                
                // Check for phase completion
                CheckPhaseCompletion(project);
                
                // Check for milestone achievement
                CheckMilestoneAchievement(project);
                
                // Check for project completion
                if (IsProjectComplete(project))
                {
                    CompleteResearchProject(project);
                }
                
                // Update active boosts
                UpdateActiveBoosts(project);
            }
        }
        
        private void UpdateResearchOffers()
        {
            // Remove expired offers
            for (int i = _availableOffers.Count - 1; i >= 0; i--)
            {
                if (_availableOffers[i].ExpirationDate < System.DateTime.Now)
                {
                    _availableOffers.RemoveAt(i);
                }
            }
        }
        
        private void ProcessResearchEvents()
        {
            // Clean up old events
            while (_recentEvents.Count > 0 && 
                   (System.DateTime.Now - _recentEvents.Peek().Timestamp).TotalDays > 30)
            {
                _recentEvents.Dequeue();
            }
        }
        
        private void UpdateCollaborationOpportunities()
        {
            // Update active partnerships
            foreach (var partnership in _activePartnerships)
            {
                UpdatePartnershipRelationship(partnership);
                CheckPartnershipMilestones(partnership);
            }
            
            // Generate new collaboration opportunities
            if (Random.Range(0f, 1f) < _collaborationSettings.OpportunityGenerationRate)
            {
                GenerateNewCollaborationOpportunity();
            }
        }
        
        private void CheckForBreakthroughs()
        {
            foreach (var project in _activeProjects)
            {
                if (Random.Range(0f, 1f) < _discoverySettings.BreakthroughProbability)
                {
                    GenerateResearchBreakthrough(project);
                }
            }
        }
        
        private void GenerateNewResearchOpportunities()
        {
            if (_availableOffers.Count < _maxAvailableProjects && Random.Range(0f, 1f) < 0.1f)
            {
                var availableProjects = _availableResearchProjects
                    .Where(p => !_availableOffers.Any(o => o.ResearchProject == p))
                    .Where(p => !_activeProjects.Any(a => a.ResearchProject == p))
                    .ToList();
                
                if (availableProjects.Count > 0)
                {
                    var project = availableProjects[Random.Range(0, availableProjects.Count)];
                    var feasibility = project.EvaluateResearchFeasibility(_playerCapabilities);
                    
                    if (feasibility.OverallFeasibility >= 0.2f)
                    {
                        var offer = new ResearchProjectOffer
                        {
                            ResearchProject = project,
                            OfferedDate = System.DateTime.Now,
                            ExpirationDate = System.DateTime.Now.AddDays(Random.Range(30, 90)),
                            Priority = CalculateOfferPriority(project),
                            Feasibility = feasibility,
                            EstimatedDuration = project.Timeline.EstimatedDurationDays,
                            EstimatedCost = project.Requirements.TotalBudgetRequired
                        };
                        
                        _availableOffers.Add(offer);
                    }
                }
            }
        }
        
        private void UpdateProjectProgress(ActiveResearchProject project)
        {
            float baseProgress = _researchUpdateInterval / project.ResearchProject.Timeline.EstimatedDurationDays;
            
            // Apply modifiers
            baseProgress *= project.ResearchSpeedMultiplier;
            baseProgress *= _researchEfficiencyMultiplier;
            baseProgress *= project.TeamExpertise;
            
            // Apply facility level bonus
            baseProgress *= GetFacilityEfficiencyBonus();
            
            project.Progress += baseProgress;
            project.Progress = Mathf.Clamp01(project.Progress);
            
            // Update research experience
            float experienceGained = baseProgress * _researchSettings.ExperienceMultiplier;
            var category = project.ResearchProject.ResearchCategory;
            _researchExperience[category] += experienceGained;
            
            // Award skill experience if skill tree manager is available
            var skillTreeManager = GameManager.Instance.GetManager<SkillTreeManager>();
            if (skillTreeManager != null)
            {
                // Award experience to relevant research skills
                foreach (var skillReq in project.ResearchProject.RequiredSkills)
                {
                    skillTreeManager.AddSkillExperience(skillReq.RequiredSkillNode, experienceGained * 0.5f, ExperienceSource.Research);
                }
            }
        }
        
        private void CheckPhaseCompletion(ActiveResearchProject project)
        {
            var phases = project.ResearchProject.ResearchPhases;
            if (project.CurrentPhaseIndex < phases.Count)
            {
                var currentPhase = phases[project.CurrentPhaseIndex];
                var phaseProgress = project.Progress * phases.Count - project.CurrentPhaseIndex;
                
                if (phaseProgress >= 1f)
                {
                    // Phase completed
                    var completedPhase = new CompletedPhase
                    {
                        PhaseID = currentPhase.PhaseID,
                        CompletionQuality = project.CurrentQuality,
                        ActualDurationDays = (int)(System.DateTime.Now - project.StartDate).TotalDays,
                        ActualCost = project.TotalInvestment,
                        Deliverables = currentPhase.Deliverables.ToList()
                    };
                    
                    project.CompletedPhases.Add(completedPhase);
                    project.CurrentPhaseIndex++;
                    
                    RecordResearchEvent(new ResearchEvent
                    {
                        EventType = ResearchEventType.Phase_Completed,
                        Project = project.ResearchProject,
                        Timestamp = System.DateTime.Now,
                        Description = $"Completed phase: {currentPhase.PhaseName}"
                    });
                }
            }
        }
        
        private void CheckMilestoneAchievement(ActiveResearchProject project)
        {
            foreach (var milestone in project.ResearchProject.Milestones)
            {
                if (!project.CompletedMilestones.Any(cm => cm.MilestoneID == milestone.MilestoneID))
                {
                    float milestoneProgress = project.Progress * project.ResearchProject.Timeline.EstimatedDurationDays;
                    
                    if (milestoneProgress >= milestone.TargetDay)
                    {
                        var completedMilestone = new CompletedMilestone
                        {
                            MilestoneID = milestone.MilestoneID,
                            WasSuccessful = Random.Range(0f, 1f) < project.CurrentQuality,
                            QualityScore = project.CurrentQuality,
                            DaysToComplete = (int)(System.DateTime.Now - project.StartDate).TotalDays,
                            AchievedCriteria = milestone.DeliverableCriteria.ToList()
                        };
                        
                        project.CompletedMilestones.Add(completedMilestone);
                        
                        RecordResearchEvent(new ResearchEvent
                        {
                            EventType = ResearchEventType.Milestone_Achieved,
                            Project = project.ResearchProject,
                            Timestamp = System.DateTime.Now,
                            Description = $"Achieved milestone: {milestone.MilestoneName}"
                        });
                    }
                }
            }
        }
        
        private bool IsProjectComplete(ActiveResearchProject project)
        {
            return project.Progress >= 1f;
        }
        
        private void CompleteResearchProject(ActiveResearchProject project)
        {
            // Generate research results
            var results = project.ResearchProject.GenerateResearchResults(
                project.CurrentQuality,
                project.TeamExpertise,
                project.HadSetbacks
            );
            
            // Process technology unlocks
            foreach (var techUnlock in results.UnlocksTechnologies)
            {
                UnlockTechnology(techUnlock);
            }
            
            // Process discoveries
            foreach (var discovery in results.UnexpectedDiscoveries)
            {
                ProcessDiscovery(discovery);
            }
            
            // Update player capabilities
            UpdatePlayerCapabilitiesFromResearch(project, results);
            
            // Mark project as completed
            project.Status = results.WasSuccessful ? ResearchStatus.Completed : ResearchStatus.Failed;
            project.CompletionDate = System.DateTime.Now;
            project.Results = results;
            
            // Remove from active projects
            _activeProjects.Remove(project);
            
            OnResearchCompleted?.Invoke(project, results);
            _researchCompletedEvent?.Raise();
            
            RecordResearchEvent(new ResearchEvent
            {
                EventType = results.WasSuccessful ? ResearchEventType.Project_Completed : ResearchEventType.Project_Failed,
                Project = project.ResearchProject,
                Timestamp = System.DateTime.Now,
                Description = $"Research project {(results.WasSuccessful ? "completed successfully" : "failed")}: {project.ResearchProject.ProjectName}"
            });
        }
        
        private void UnlockTechnology(TechnologyUnlock techUnlock)
        {
            var unlockedTech = new UnlockedTechnology
            {
                TechnologyUnlock = techUnlock,
                UnlockDate = System.DateTime.Now,
                UnlockSource = "Research Project",
                IsActive = true
            };
            
            if (!_unlockedTechnologies.ContainsKey(techUnlock.TechnologyType))
                _unlockedTechnologies[techUnlock.TechnologyType] = new List<UnlockedTechnology>();
            
            _unlockedTechnologies[techUnlock.TechnologyType].Add(unlockedTech);
            
            OnTechnologyUnlocked?.Invoke(techUnlock);
            _technologyUnlockedEvent?.Raise();
        }
        
        private void ProcessDiscovery(PotentialDiscovery discovery)
        {
            var breakthrough = new ResearchBreakthrough
            {
                Discovery = discovery,
                DiscoveryDate = System.DateTime.Now,
                ImpactLevel = CalculateDiscoveryImpact(discovery),
                CommercialPotential = discovery.CommercialValue
            };
            
            _breakthroughs.Add(breakthrough);
            
            OnDiscoveryMade?.Invoke(discovery);
            _discoveryMadeEvent?.Raise();
        }
        
        private float CalculateTeamExpertise(ResearchProjectSO project)
        {
            float totalExpertise = 0f;
            int skillCount = 0;
            
            var skillTreeManager = GameManager.Instance.GetManager<SkillTreeManager>();
            if (skillTreeManager != null)
            {
                foreach (var skillReq in project.RequiredSkills)
                {
                    int playerLevel = skillTreeManager.GetSkillLevel(skillReq.RequiredSkillNode);
                    float expertiseContribution = Mathf.Min(playerLevel / (float)skillReq.MinimumLevel, 2f); // Cap at 2x
                    totalExpertise += expertiseContribution;
                    skillCount++;
                }
            }
            
            return skillCount > 0 ? totalExpertise / skillCount : 0.5f;
        }
        
        private bool ApplyResearchCosts(ResearchProjectSO project)
        {
            // Check budget
            if (_playerCapabilities.AvailableBudget < project.Requirements.TotalBudgetRequired)
                return false;
            
            // Apply costs
            _playerCapabilities.AvailableBudget -= project.Requirements.TotalBudgetRequired;
            _playerCapabilities.AvailableResearchTime -= project.Timeline.EstimatedDurationDays;
            
            return true;
        }
        
        private float GetFacilityEfficiencyBonus()
        {
            return _facilityLevel switch
            {
                ResearchFacilityLevel.Basic => 1f,
                ResearchFacilityLevel.Intermediate => 1.2f,
                ResearchFacilityLevel.Advanced => 1.5f,
                ResearchFacilityLevel.Cutting_Edge => 2f,
                _ => 1f
            };
        }
        
        private bool AreBoostRequirementsMet(ResearchBoost boost)
        {
            // Check if player has sufficient resources/budget for the boost
            return _playerCapabilities.AvailableBudget >= boost.Cost;
        }
        
        private bool AreCollaborationRequirementsMet(CollaborationOpportunity opportunity)
        {
            var skillTreeManager = GameManager.Instance.GetManager<SkillTreeManager>();
            if (skillTreeManager == null) return false;
            
            // Check if player has required expertise
            foreach (var expertise in opportunity.RequiredExpertise)
            {
                var relevantSkills = skillTreeManager.GetUnlockedSkillsInCategory((SkillCategory)expertise);
                if (relevantSkills.Count == 0 || 
                    relevantSkills.Average(skill => skillTreeManager.GetSkillLevel(skill)) < opportunity.MinimumSkillLevel)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private float CalculatePlayerContribution(CollaborationOpportunity opportunity)
        {
            float totalExpertise = 0f;
            int categoryCount = 0;
            
            var skillTreeManager = GameManager.Instance.GetManager<SkillTreeManager>();
            if (skillTreeManager != null)
            {
                foreach (var expertise in opportunity.RequiredExpertise)
                {
                    var relevantSkills = skillTreeManager.GetUnlockedSkillsInCategory((SkillCategory)expertise);
                    if (relevantSkills.Count > 0)
                    {
                        float avgLevel = (float)relevantSkills.Average(skill => skillTreeManager.GetSkillLevel(skill));
                        totalExpertise += avgLevel / 10f; // Normalize to 0-1 scale
                        categoryCount++;
                    }
                }
            }
            
            return categoryCount > 0 ? totalExpertise / categoryCount : 0.3f;
        }
        
        private void ApplyCollaborationBenefits(ResearchPartnership partnership)
        {
            // Apply benefits to relevant active projects
            foreach (var project in _activeProjects)
            {
                foreach (var benefit in partnership.CollaborationOpportunity.Benefits)
                {
                    switch (benefit.BenefitType)
                    {
                        case CollaborationBenefitType.Accelerated_Research:
                            project.ResearchSpeedMultiplier *= benefit.BenefitValue;
                            break;
                        case CollaborationBenefitType.Quality_Improvement:
                            project.QualityBonusMultiplier *= benefit.BenefitValue;
                            break;
                        case CollaborationBenefitType.Risk_Sharing:
                            project.SuccessProbabilityBonus += (float)(benefit.BenefitValue * 0.1);
                            break;
                    }
                }
            }
        }
        
        private float CalculateResearchRecommendationScore(ResearchProjectSO project)
        {
            float score = 0f;
            
            // Base score from priority
            score += project.Priority switch
            {
                ResearchPriority.Critical => 1f,
                ResearchPriority.High => 0.8f,
                ResearchPriority.Medium => 0.6f,
                ResearchPriority.Low => 0.4f,
                _ => 0.3f
            };
            
            // Feasibility bonus
            var feasibility = project.EvaluateResearchFeasibility(_playerCapabilities);
            score += feasibility.OverallFeasibility * 0.5f;
            
            // Technology unlock potential
            if (project.TechnologyUnlocks.Count > 0)
                score += 0.3f;
            
            // Research experience alignment
            float categoryExperience = GetResearchExperience(project.ResearchCategory);
            if (categoryExperience > 100f) // Player has experience in this category
                score += 0.2f;
            
            return Mathf.Clamp01(score);
        }
        
        private string GenerateRecommendationReasoning(ResearchProjectSO project, float score)
        {
            if (score > 0.8f)
                return "Highly recommended - excellent fit for your current capabilities and strategic goals.";
            else if (score > 0.6f)
                return "Good opportunity that aligns well with your research expertise.";
            else if (score > 0.4f)
                return "Decent project that could expand your research portfolio.";
            else
                return "Consider this project for specialized development or future planning.";
        }
        
        private void UpdateActiveBoosts(ActiveResearchProject project)
        {
            for (int i = project.ActiveBoosts.Count - 1; i >= 0; i--)
            {
                var boost = project.ActiveBoosts[i];
                
                if (System.DateTime.Now > boost.ExpirationDate || boost.RemainingUses <= 0)
                {
                    project.ActiveBoosts.RemoveAt(i);
                }
            }
        }
        
        private void UpdatePlayerCapabilitiesFromResearch(ActiveResearchProject project, ResearchResults results)
        {
            // Increase research capabilities based on successful completion
            if (results.WasSuccessful)
            {
                _playerCapabilities.AvailableBudget += results.CommercialValue * 0.1f; // 10% of commercial value
                
                // Chance to unlock parallel project management
                if (!_playerCapabilities.CanManageParallelProjects && 
                    GetResearchExperience(project.ResearchProject.ResearchCategory) > 500f)
                {
                    _playerCapabilities.CanManageParallelProjects = true;
                }
            }
        }
        
        private ResearchPriority CalculateOfferPriority(ResearchProjectSO project)
        {
            // Base priority from project
            var priority = project.Priority;
            
            // Increase priority based on player research experience
            float categoryExperience = GetResearchExperience(project.ResearchCategory);
            if (categoryExperience < 100f && priority == ResearchPriority.Low)
                priority = ResearchPriority.Medium; // Beginner-friendly projects get boosted
            
            return priority;
        }
        
        private float CalculateTechnologyImpact(TechnologyUnlock techUnlock)
        {
            return techUnlock.TechnologyReadinessLevel switch
            {
                TechnologyReadiness.Market_Ready => 1f,
                TechnologyReadiness.Commercial_Scale => 0.9f,
                TechnologyReadiness.Pilot_Scale => 0.7f,
                TechnologyReadiness.Laboratory_Testing => 0.5f,
                TechnologyReadiness.Prototype_Development => 0.4f,
                TechnologyReadiness.Proof_of_Concept => 0.3f,
                _ => 0.2f
            };
        }
        
        private bool AreTechnologyPrerequisitesMet(TechnologyUnlock techUnlock)
        {
            // Check if prerequisite technologies are unlocked
            // This would be implemented based on specific technology dependencies
            return true; // Simplified for now
        }
        
        private void UpdatePartnershipRelationship(ResearchPartnership partnership)
        {
            // Relationship naturally improves with successful collaboration
            if (partnership.SharedProjects.Any(p => p.Status == ResearchStatus.Active))
            {
                partnership.RelationshipStrength += 0.01f; // Gradual improvement
                partnership.RelationshipStrength = Mathf.Clamp01(partnership.RelationshipStrength);
            }
        }
        
        private void CheckPartnershipMilestones(ResearchPartnership partnership)
        {
            // Check for partnership achievements and benefits
            if (partnership.RelationshipStrength > 0.8f && partnership.SharedProjects.Count >= 2)
            {
                // Unlock advanced collaboration benefits
                partnership.Status = CollaborationStatus.Strategic_Partnership;
            }
        }
        
        private void GenerateNewCollaborationOpportunity()
        {
            // Create a new collaboration opportunity based on player research activity
            var opportunity = new CollaborationOpportunity
            {
                OpportunityName = $"Research Collaboration {System.DateTime.Now.Year}",
                CollaborationType = (CollaborationType)Random.Range(0, System.Enum.GetValues(typeof(CollaborationType)).Length),
                RequiredExpertise = new List<SkillCategory> { (SkillCategory)Random.Range(0, System.Enum.GetValues(typeof(SkillCategory)).Length) },
                MinimumSkillLevel = Random.Range(3, 8),
                DurationDays = Random.Range(30, 180),
                Benefits = GenerateCollaborationBenefits()
            };
            
            _availableCollaborations.Add(opportunity);
        }
        
        private List<CollaborationBenefit> GenerateCollaborationBenefits()
        {
            var benefits = new List<CollaborationBenefit>();
            
            // Always include research acceleration
            benefits.Add(new CollaborationBenefit
            {
                BenefitName = "Research Acceleration",
                BenefitType = CollaborationBenefitType.Accelerated_Research,
                BenefitValue = Random.Range(1.2f, 1.8f),
                BenefitDescription = "Speeds up research progress through shared expertise"
            });
            
            // Random additional benefit
            var benefitTypes = System.Enum.GetValues(typeof(CollaborationBenefitType));
            var randomBenefit = (CollaborationBenefitType)benefitTypes.GetValue(Random.Range(0, benefitTypes.Length));
            
            benefits.Add(new CollaborationBenefit
            {
                BenefitName = randomBenefit.ToString().Replace("_", " "),
                BenefitType = randomBenefit,
                BenefitValue = Random.Range(1.1f, 1.5f),
                BenefitDescription = $"Provides {randomBenefit.ToString().Replace("_", " ").ToLower()} benefits"
            });
            
            return benefits;
        }
        
        private void GenerateResearchBreakthrough(ActiveResearchProject project)
        {
            var discovery = new PotentialDiscovery
            {
                DiscoveryName = $"Research Breakthrough in {project.ResearchProject.ResearchCategory}",
                DiscoveryType = ProjectChimera.Data.Progression.DiscoveryType.Scientific_Discovery,
                DiscoveryProbability = 0.8f, // Already rolled for this
                CommercialValue = Random.Range(50000f, 500000f),
                NoveltyLevel = (NoveltyLevel)Random.Range(0, System.Enum.GetValues(typeof(NoveltyLevel)).Length),
                DiscoveryDescription = "Unexpected research finding with significant potential"
            };
            
            ProcessDiscovery(discovery);
        }
        
        private ImpactLevel CalculateDiscoveryImpact(PotentialDiscovery discovery)
        {
            return discovery.NoveltyLevel switch
            {
                NoveltyLevel.Revolutionary => ImpactLevel.Transformational,
                NoveltyLevel.Paradigm_Shifting => ImpactLevel.High,
                NoveltyLevel.Breakthrough => ImpactLevel.High,
                NoveltyLevel.Significant => ImpactLevel.Medium,
                NoveltyLevel.Incremental => ImpactLevel.Low,
                _ => ImpactLevel.Minimal
            };
        }
        
        private void RecordResearchEvent(ResearchEvent researchEvent)
        {
            _recentEvents.Enqueue(researchEvent);
            
            // Keep only recent events
            while (_recentEvents.Count > 50)
            {
                _recentEvents.Dequeue();
            }
        }
    }
    
    [System.Serializable]
    public class ResearchSettings
    {
        [Range(0.1f, 5f)] public float UpdateInterval = 1f; // In-game days
        [Range(10000f, 1000000f)] public float StartingResearchBudget = 50000f;
        [Range(30, 365)] public int StartingResearchTime = 90; // days
        [Range(0f, 1f)] public float MinimumFeasibilityThreshold = 0.3f;
        [Range(1f, 10f)] public float ExperienceMultiplier = 2f;
        public bool EnableCollaborativeResearch = true;
    }
    
    [System.Serializable]
    public class DiscoverySettings
    {
        [Range(0f, 0.1f)] public float BreakthroughProbability = 0.02f; // Per update
        [Range(0f, 1f)] public float SerendipityFactor = 0.15f;
        [Range(1f, 5f)] public float DiscoveryValueMultiplier = 2f;
        public bool EnableUnexpectedDiscoveries = true;
    }
    
    [System.Serializable]
    public class CollaborationSettings
    {
        [Range(0f, 0.2f)] public float OpportunityGenerationRate = 0.05f; // Per update
        [Range(1f, 3f)] public float CollaborationBenefitMultiplier = 1.5f;
        [Range(0f, 1f)] public float RelationshipDecayRate = 0.01f;
        public bool EnableInternationalCollaboration = true;
    }
    
    // Note: ActiveResearchProject class is now defined in ResearchDataStructures.cs
    
    [System.Serializable]
    public class ResearchProjectOffer
    {
        public ResearchProjectSO ResearchProject;
        public System.DateTime OfferedDate;
        public System.DateTime ExpirationDate;
        public ResearchPriority Priority;
        public ResearchFeasibility Feasibility;
        public int EstimatedDuration;
        public float EstimatedCost;
    }
    
    [System.Serializable]
    public class UnlockedTechnology
    {
        public TechnologyUnlock TechnologyUnlock;
        public System.DateTime UnlockDate;
        public string UnlockSource;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class ResearchBreakthrough
    {
        public PotentialDiscovery Discovery;
        public System.DateTime DiscoveryDate;
        public ImpactLevel ImpactLevel;
        public float CommercialPotential;
    }
    
    [System.Serializable]
    public class ResearchEvent
    {
        public ResearchEventType EventType;
        public ResearchProjectSO Project;
        public System.DateTime Timestamp;
        public string Description;
    }
    
    [System.Serializable]
    public class ResearchRecommendation
    {
        public ResearchProjectSO Project;
        public float Score;
        public string Reasoning;
    }
    
    [System.Serializable]
    public class TechnologyPreview
    {
        public TechnologyUnlock TechnologyUnlock;
        public float UnlockProbability;
        public float EstimatedImpact;
        public bool PrerequisitesMet;
    }
    
        // Note: ResearchBoost and ActiveResearchBoost classes are now defined in ResearchDataStructures.cs
    
    [System.Serializable]
    public class ResearchPartnership
    {
        public CollaborationOpportunity CollaborationOpportunity;
        public System.DateTime StartDate;
        public CollaborationStatus Status;
        public float ContributionLevel;
        public float RelationshipStrength;
        public List<ActiveResearchProject> SharedProjects = new List<ActiveResearchProject>();
    }
    
    [System.Serializable]
    public class TechnologyTree
    {
        public string TreeName;
        public TechnologyType TechnologyType;
        public List<TechnologyNode> TechnologyNodes = new List<TechnologyNode>();
        public Vector2 TreePosition; // For UI positioning
    }
    
    [System.Serializable]
    public class TechnologyNode
    {
        public TechnologyUnlock Technology;
        public List<TechnologyNode> Prerequisites = new List<TechnologyNode>();
        public bool IsUnlocked;
        public Vector2 NodePosition;
    }
    
    [System.Serializable]
    public class InnovationOpportunity
    {
        public string OpportunityName;
        public ResearchCategory ResearchCategory;
        public float InnovationPotential;
        public float MarketPotential;
        public List<string> RequiredTechnologies = new List<string>();
        public string OpportunityDescription;
    }
    
    // Note: ResearchStatus enum is now defined in ResearchDataStructures.cs
    
    public enum ResearchEventType
    {
        Project_Started,
        Project_Completed,
        Project_Failed,
        Phase_Completed,
        Milestone_Achieved,
        Technology_Unlocked,
        Discovery_Made,
        Collaboration_Started,
        Breakthrough_Achieved
    }
    
    public enum ResearchFacilityLevel
    {
        Basic,
        Intermediate,
        Advanced,
        Cutting_Edge
    }
    
    public enum CollaborationStatus
    {
        Proposed,
        Active,
        Strategic_Partnership,
        Completed,
        Terminated
    }
}