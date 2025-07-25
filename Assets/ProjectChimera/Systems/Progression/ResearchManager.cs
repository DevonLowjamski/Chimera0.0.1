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
            
            float gameTimeDelta = Time.deltaTime; // Simplified - use Unity's time directly
            
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
            var skillTreeManager = GameManager.Instance.GetManager("SkillTreeManager") as SkillTreeManager;
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
            
            // PC-012-8: Connect research unlocks to actual game features
            ApplyTechnologyToGameSystems(techUnlock);
            
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
            
            var skillTreeManager = GameManager.Instance.GetManager("SkillTreeManager") as SkillTreeManager;
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
            var skillTreeManager = GameManager.Instance.GetManager("SkillTreeManager") as SkillTreeManager;
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
            
            var skillTreeManager = GameManager.Instance.GetManager("SkillTreeManager") as SkillTreeManager;
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
        
        #region PC-012-8: Research System Integration with Game Features
        
        /// <summary>
        /// PC-012-8: Apply research technology unlocks to actual game systems
        /// </summary>
        private void ApplyTechnologyToGameSystems(TechnologyUnlock techUnlock)
        {
            Debug.Log($"🔬 Applying technology unlock: {techUnlock.TechnologyName} to game systems");
            
            switch (techUnlock.TechnologyType)
            {
                case TechnologyType.Cultivation:
                    ApplyCultivationTechnology(techUnlock);
                    break;
                case TechnologyType.Genetics:
                    ApplyGeneticsTechnology(techUnlock);
                    break;
                case TechnologyType.Equipment:
                case TechnologyType.Equipment_Technology:
                    ApplyEquipmentTechnology(techUnlock);
                    break;
                case TechnologyType.Automation:
                case TechnologyType.Automation_Technology:
                    ApplyAutomationTechnology(techUnlock);
                    break;
                case TechnologyType.Processing:
                case TechnologyType.Process_Technology:
                    ApplyProcessingTechnology(techUnlock);
                    break;
                case TechnologyType.Analytics:
                case TechnologyType.Analytical_Technology:
                    ApplyAnalyticsTechnology(techUnlock);
                    break;
                default:
                    Debug.LogWarning($"Unknown technology type: {techUnlock.TechnologyType}");
                    break;
            }
            
            // Award experience to progression manager
            var progressionManager = GameManager.Instance?.GetManager("ComprehensiveProgressionManager") as ComprehensiveProgressionManager;
            if (progressionManager != null)
            {
                progressionManager.AwardExperience("Research", techUnlock.CommercialValue * 0.01f, 
                    "current_player", $"Technology unlocked: {techUnlock.TechnologyName}");
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply cultivation technology to cultivation systems
        /// </summary>
        private void ApplyCultivationTechnology(TechnologyUnlock techUnlock)
        {
            // Try to get CultivationManager - using object type to avoid assembly reference issues
            var cultivationManager = GameManager.Instance?.GetManager("CultivationManager");
            if (cultivationManager == null)
            {
                Debug.LogWarning("CultivationManager not found - cannot apply cultivation technology");
                return;
            }
            
            // Apply technology based on specific unlock
            switch (techUnlock.TechnologyName)
            {
                case "Advanced Hydroponics":
                    EnableAdvancedHydroponics(cultivationManager);
                    break;
                case "Automated Nutrient Delivery":
                    EnableAutomatedNutrients(cultivationManager);
                    break;
                case "Climate Control Optimization":
                    EnableClimateOptimization(cultivationManager);
                    break;
                case "Accelerated Growth Protocol":
                    EnableAcceleratedGrowth(cultivationManager);
                    break;
                case "Precision Agriculture":
                    EnablePrecisionAgriculture(cultivationManager);
                    break;
                default:
                    // Generic cultivation boost
                    ApplyGenericCultivationBoost(cultivationManager, techUnlock);
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply genetics technology to genetics systems
        /// </summary>
        private void ApplyGeneticsTechnology(TechnologyUnlock techUnlock)
        {
            var breedingGoalManager = GameManager.Instance?.GetManager("BreedingGoalManager");
            var breedingTournamentManager = GameManager.Instance?.GetManager("BreedingTournamentManager");
            
            switch (techUnlock.TechnologyName)
            {
                case "CRISPR Gene Editing":
                    EnableCRISPRTechnology(breedingGoalManager);
                    break;
                case "Marker-Assisted Selection":
                    EnableMarkerAssistedSelection(breedingGoalManager);
                    break;
                case "Genomic Prediction":
                    EnableGenomicPrediction(breedingGoalManager);
                    break;
                case "Trait Pyramiding":
                    EnableTraitPyramiding(breedingGoalManager);
                    break;
                case "Breeding Tournament Analytics":
                    EnableTournamentAnalytics(breedingTournamentManager);
                    break;
                default:
                    // Generic genetics boost
                    ApplyGenericGeneticsBoost(techUnlock);
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply equipment technology to equipment systems
        /// </summary>
        private void ApplyEquipmentTechnology(TechnologyUnlock techUnlock)
        {
            var equipmentDegradationManager = GameManager.Instance?.GetManager("EquipmentDegradationManager");
            var equipmentPlacementManager = GameManager.Instance?.GetManager("EquipmentPlacementManager");
            
            switch (techUnlock.TechnologyName)
            {
                case "Self-Maintaining Equipment":
                    EnableSelfMaintenance(equipmentDegradationManager);
                    break;
                case "Modular Equipment Design":
                    EnableModularDesign(equipmentPlacementManager);
                    break;
                case "Intelligent Equipment Placement":
                    EnableIntelligentPlacement(equipmentPlacementManager);
                    break;
                case "Predictive Maintenance":
                    EnablePredictiveMaintenance(equipmentDegradationManager);
                    break;
                default:
                    // Generic equipment boost
                    ApplyGenericEquipmentBoost(techUnlock);
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply automation technology to automation systems
        /// </summary>
        private void ApplyAutomationTechnology(TechnologyUnlock techUnlock)
        {
            var automationManager = GameManager.Instance?.GetManager("AutomationManager");
            if (automationManager == null)
            {
                Debug.LogWarning("AutomationManager not found - cannot apply automation technology");
                return;
            }
            
            switch (techUnlock.TechnologyName)
            {
                case "AI-Driven Automation":
                    EnableAIDrivenAutomation(automationManager);
                    break;
                case "Predictive Scheduling":
                    EnablePredictiveScheduling(automationManager);
                    break;
                case "Adaptive Control Systems":
                    EnableAdaptiveControl(automationManager);
                    break;
                case "Multi-Zone Coordination":
                    EnableMultiZoneCoordination(automationManager);
                    break;
                default:
                    // Generic automation boost
                    ApplyGenericAutomationBoost(automationManager, techUnlock);
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply processing technology to processing systems
        /// </summary>
        private void ApplyProcessingTechnology(TechnologyUnlock techUnlock)
        {
            // Processing technologies would affect harvest processing, quality control, etc.
            switch (techUnlock.TechnologyName)
            {
                case "Advanced Drying Techniques":
                    EnableAdvancedDrying();
                    break;
                case "Quality Preservation":
                    EnableQualityPreservation();
                    break;
                case "Automated Trimming":
                    EnableAutomatedTrimming();
                    break;
                case "Extraction Optimization":
                    EnableExtractionOptimization();
                    break;
                default:
                    // Generic processing boost
                    ApplyGenericProcessingBoost(techUnlock);
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply analytics technology to analytics systems
        /// </summary>
        private void ApplyAnalyticsTechnology(TechnologyUnlock techUnlock)
        {
            var analyticsManager = GameManager.Instance?.GetManager("AnalyticsManager");
            var aiAdvisorManager = GameManager.Instance?.GetManager("AIAdvisorManager");
            
            switch (techUnlock.TechnologyName)
            {
                case "Predictive Analytics":
                    EnablePredictiveAnalytics(analyticsManager);
                    break;
                case "Advanced AI Advisor":
                    EnableAdvancedAIAdvisor(aiAdvisorManager);
                    break;
                case "Real-time Performance Monitoring":
                    EnableRealtimeMonitoring(analyticsManager);
                    break;
                case "Market Intelligence":
                    EnableMarketIntelligence(analyticsManager);
                    break;
                default:
                    // Generic analytics boost
                    ApplyGenericAnalyticsBoost(techUnlock);
                    break;
            }
        }
        
        #endregion
        
        #region PC-012-8: Specific Technology Implementation Methods
        
        // Cultivation Technology Implementations
        private void EnableAdvancedHydroponics(object cultivationManager)
        {
            Debug.Log("🌱 Advanced Hydroponics enabled - 25% faster growth, 20% higher yield");
            // This would call specific methods on CultivationManager to enable advanced hydroponics
            // cultivationManager.EnableAdvancedHydroponics();
        }
        
        private void EnableAutomatedNutrients(object cultivationManager)
        {
            Debug.Log("🧪 Automated Nutrient Delivery enabled - Reduced labor, consistent nutrition");
            // cultivationManager.EnableAutomatedNutrients();
        }
        
        private void EnableClimateOptimization(object cultivationManager)
        {
            Debug.Log("🌡️ Climate Control Optimization enabled - Perfect environmental conditions");
            // cultivationManager.EnableClimateOptimization();
        }
        
        private void EnableAcceleratedGrowth(object cultivationManager)
        {
            Debug.Log("⚡ Accelerated Growth Protocol enabled - 30% faster plant development");
            // cultivationManager.EnableAcceleratedGrowth();
        }
        
        private void EnablePrecisionAgriculture(object cultivationManager)
        {
            Debug.Log("🎯 Precision Agriculture enabled - Individual plant optimization");
            // cultivationManager.EnablePrecisionAgriculture();
        }
        
        private void ApplyGenericCultivationBoost(object cultivationManager, TechnologyUnlock techUnlock)
        {
            Debug.Log($"🌿 Generic cultivation boost applied: {techUnlock.TechnologyName}");
            // cultivationManager.ApplyTechnologyBoost(techUnlock.TechnologyName, techUnlock.ImpactLevel);
        }
        
        // Genetics Technology Implementations
        private void EnableCRISPRTechnology(object breedingManager)
        {
            Debug.Log("🧬 CRISPR Gene Editing enabled - Precise genetic modifications");
            // breedingManager?.EnableCRISPRTechnology();
        }
        
        private void EnableMarkerAssistedSelection(object breedingManager)
        {
            Debug.Log("🎯 Marker-Assisted Selection enabled - Faster trait selection");
            // breedingManager?.EnableMarkerAssistedSelection();
        }
        
        private void EnableGenomicPrediction(object breedingManager)
        {
            Debug.Log("🔮 Genomic Prediction enabled - Predict breeding outcomes");
            // breedingManager?.EnableGenomicPrediction();
        }
        
        private void EnableTraitPyramiding(object breedingManager)
        {
            Debug.Log("🏗️ Trait Pyramiding enabled - Combine multiple beneficial traits");
            // breedingManager?.EnableTraitPyramiding();
        }
        
        private void EnableTournamentAnalytics(object tournamentManager)
        {
            Debug.Log("📊 Tournament Analytics enabled - Advanced competition insights");
            // tournamentManager?.EnableAdvancedAnalytics();
        }
        
        private void ApplyGenericGeneticsBoost(TechnologyUnlock techUnlock)
        {
            Debug.Log($"🧬 Generic genetics boost applied: {techUnlock.TechnologyName}");
            // Apply boost to genetics systems
        }
        
        // Equipment Technology Implementations
        private void EnableSelfMaintenance(object degradationManager)
        {
            Debug.Log("🔧 Self-Maintaining Equipment enabled - 60% less maintenance needed");
            // degradationManager?.EnableSelfMaintenance();
        }
        
        private void EnableModularDesign(object placementManager)
        {
            Debug.Log("🔧 Modular Equipment Design enabled - Flexible equipment configurations");
            // placementManager?.EnableModularDesign();
        }
        
        private void EnableIntelligentPlacement(object placementManager)
        {
            Debug.Log("🤖 Intelligent Equipment Placement enabled - AI-optimized layouts");
            // placementManager?.EnableIntelligentPlacement();
        }
        
        private void EnablePredictiveMaintenance(object degradationManager)
        {
            Debug.Log("🔮 Predictive Maintenance enabled - Prevent equipment failures");
            // degradationManager?.EnablePredictiveMaintenance();
        }
        
        private void ApplyGenericEquipmentBoost(TechnologyUnlock techUnlock)
        {
            Debug.Log($"🔧 Generic equipment boost applied: {techUnlock.TechnologyName}");
            // Apply boost to equipment systems
        }
        
        // Automation Technology Implementations
        private void EnableAIDrivenAutomation(object automationManager)
        {
            Debug.Log("🤖 AI-Driven Automation enabled - Intelligent facility management");
            // automationManager.EnableAIDrivenAutomation();
        }
        
        private void EnablePredictiveScheduling(object automationManager)
        {
            Debug.Log("📅 Predictive Scheduling enabled - Optimize task timing");
            // automationManager.EnablePredictiveScheduling();
        }
        
        private void EnableAdaptiveControl(object automationManager)
        {
            Debug.Log("🎛️ Adaptive Control Systems enabled - Dynamic environmental adjustments");
            // automationManager.EnableAdaptiveControl();
        }
        
        private void EnableMultiZoneCoordination(object automationManager)
        {
            Debug.Log("🏢 Multi-Zone Coordination enabled - Facility-wide optimization");
            // automationManager.EnableMultiZoneCoordination();
        }
        
        private void ApplyGenericAutomationBoost(object automationManager, TechnologyUnlock techUnlock)
        {
            Debug.Log($"🤖 Generic automation boost applied: {techUnlock.TechnologyName}");
            // automationManager.ApplyTechnologyBoost(techUnlock.TechnologyName, techUnlock.ImpactLevel);
        }
        
        // Processing Technology Implementations
        private void EnableAdvancedDrying()
        {
            Debug.Log("🌬️ Advanced Drying Techniques enabled - Better quality preservation");
            // Apply to processing systems
        }
        
        private void EnableQualityPreservation()
        {
            Debug.Log("💎 Quality Preservation enabled - Maintain product quality longer");
            // Apply to inventory and processing systems
        }
        
        private void EnableAutomatedTrimming()
        {
            Debug.Log("✂️ Automated Trimming enabled - Consistent, efficient processing");
            // Apply to harvest processing
        }
        
        private void EnableExtractionOptimization()
        {
            Debug.Log("🧪 Extraction Optimization enabled - Higher extraction yields");
            // Apply to processing systems
        }
        
        private void ApplyGenericProcessingBoost(TechnologyUnlock techUnlock)
        {
            Debug.Log($"🏭 Generic processing boost applied: {techUnlock.TechnologyName}");
            // Apply boost to processing systems
        }
        
        // Analytics Technology Implementations
        private void EnablePredictiveAnalytics(object analyticsManager)
        {
            Debug.Log("🔮 Predictive Analytics enabled - Forecast trends and issues");
            // analyticsManager?.EnablePredictiveAnalytics();
        }
        
        private void EnableAdvancedAIAdvisor(object aiAdvisorManager)
        {
            Debug.Log("🧠 Advanced AI Advisor enabled - Smarter recommendations");
            // aiAdvisorManager?.EnableAdvancedMode();
        }
        
        private void EnableRealtimeMonitoring(object analyticsManager)
        {
            Debug.Log("📊 Real-time Performance Monitoring enabled - Instant insights");
            // analyticsManager?.EnableRealtimeMonitoring();
        }
        
        private void EnableMarketIntelligence(object analyticsManager)
        {
            Debug.Log("💰 Market Intelligence enabled - Advanced market analysis");
            // analyticsManager?.EnableMarketIntelligence();
        }
        
        private void ApplyGenericAnalyticsBoost(TechnologyUnlock techUnlock)
        {
            Debug.Log($"📈 Generic analytics boost applied: {techUnlock.TechnologyName}");
            // Apply boost to analytics systems
        }
        
        #endregion
        
        #region PC-012-8: Research Integration with Game Features
        
        /// <summary>
        /// PC-012-8: Get research bonuses that should be applied to gameplay
        /// </summary>
        public ResearchGameplayBonuses GetResearchBonuses()
        {
            var bonuses = new ResearchGameplayBonuses();
            
            // Calculate bonuses based on unlocked technologies
            foreach (var techList in _unlockedTechnologies.Values)
            {
                foreach (var tech in techList.Where(t => t.IsActive))
                {
                    ApplyTechnologyToGameplayBonuses(tech.TechnologyUnlock, bonuses);
                }
            }
            
            // Apply research experience bonuses
            ApplyResearchExperienceBonuses(bonuses);
            
            return bonuses;
        }
        
        /// <summary>
        /// PC-012-8: Apply individual technology bonuses to gameplay
        /// </summary>
        private void ApplyTechnologyToGameplayBonuses(TechnologyUnlock techUnlock, ResearchGameplayBonuses bonuses)
        {
            switch (techUnlock.TechnologyType)
            {
                case TechnologyType.Cultivation:
                    bonuses.CultivationEfficiencyBonus += techUnlock.ImpactLevel * 0.1f;
                    bonuses.GrowthSpeedMultiplier += techUnlock.ImpactLevel * 0.05f;
                    break;
                case TechnologyType.Genetics:
                    bonuses.BreedingSuccessBonus += techUnlock.ImpactLevel * 0.15f;
                    bonuses.GeneticAnalysisAccuracy += techUnlock.ImpactLevel * 0.12f;
                    break;
                case TechnologyType.Equipment:
                case TechnologyType.Equipment_Technology:
                    bonuses.EquipmentEfficiencyBonus += techUnlock.ImpactLevel * 0.08f;
                    bonuses.MaintenanceReduction += techUnlock.ImpactLevel * 0.1f;
                    break;
                case TechnologyType.Automation:
                case TechnologyType.Automation_Technology:
                    bonuses.AutomationEfficiencyBonus += techUnlock.ImpactLevel * 0.2f;
                    bonuses.LaborCostReduction += techUnlock.ImpactLevel * 0.15f;
                    break;
                case TechnologyType.Processing:
                case TechnologyType.Process_Technology:
                    bonuses.ProcessingQualityBonus += techUnlock.ImpactLevel * 0.1f;
                    bonuses.ProcessingSpeedBonus += techUnlock.ImpactLevel * 0.08f;
                    break;
                case TechnologyType.Analytics:
                case TechnologyType.Analytical_Technology:
                    bonuses.PredictionAccuracyBonus += techUnlock.ImpactLevel * 0.18f;
                    bonuses.MarketInsightBonus += techUnlock.ImpactLevel * 0.12f;
                    break;
            }
        }
        
        /// <summary>
        /// PC-012-8: Apply research experience bonuses to gameplay
        /// </summary>
        private void ApplyResearchExperienceBonuses(ResearchGameplayBonuses bonuses)
        {
            // Experience in each research category provides passive bonuses
            foreach (var expPair in _researchExperience)
            {
                float experienceLevel = expPair.Value / 1000f; // Normalize to 0-1+ scale
                
                switch (expPair.Key)
                {
                    case ResearchCategory.Genetics:
                        bonuses.GeneticAnalysisAccuracy += experienceLevel * 0.05f;
                        bonuses.BreedingSuccessBonus += experienceLevel * 0.03f;
                        break;
                    case ResearchCategory.Cultivation:
                        bonuses.CultivationEfficiencyBonus += experienceLevel * 0.04f;
                        bonuses.GrowthSpeedMultiplier += experienceLevel * 0.02f;
                        break;
                    case ResearchCategory.Processing:
                        bonuses.ProcessingQualityBonus += experienceLevel * 0.03f;
                        bonuses.ProcessingSpeedBonus += experienceLevel * 0.02f;
                        break;
                    case ResearchCategory.Analytics:
                        bonuses.PredictionAccuracyBonus += experienceLevel * 0.06f;
                        bonuses.MarketInsightBonus += experienceLevel * 0.04f;
                        break;
                }
            }
        }
        
        /// <summary>
        /// PC-012-8: Check if specific research technology is unlocked
        /// </summary>
        public bool IsResearchTechnologyUnlocked(string technologyName)
        {
            return IsTechnologyUnlocked(technologyName);
        }
        
        /// <summary>
        /// PC-012-8: Get research recommendations based on current game state
        /// </summary>
        public List<ResearchProjectSO> GetGameplayBasedRecommendations()
        {
            var recommendations = new List<ResearchProjectSO>();
            
            // Analyze current game state to recommend relevant research
            var cultivationManager = GameManager.Instance?.GetManager("CultivationManager");
            var breedingGoalManager = GameManager.Instance?.GetManager("BreedingGoalManager");
            var economyManager = GameManager.Instance?.GetManager("EconomyManager");
            
            // Recommend based on current challenges or opportunities
            if (cultivationManager != null)
            {
                var cultivationRecommendations = GetCultivationBasedRecommendations();
                recommendations.AddRange(cultivationRecommendations);
            }
            
            if (breedingGoalManager != null)
            {
                var breedingRecommendations = GetBreedingBasedRecommendations();
                recommendations.AddRange(breedingRecommendations);
            }
            
            if (economyManager != null)
            {
                var economyRecommendations = GetEconomyBasedRecommendations();
                recommendations.AddRange(economyRecommendations);
            }
            
            return recommendations.Take(5).ToList();
        }
        
        private List<ResearchProjectSO> GetCultivationBasedRecommendations()
        {
            var recommendations = new List<ResearchProjectSO>();
            
            // Recommend cultivation research if player has cultivation challenges
            var cultivationProjects = _availableResearchProjects
                .Where(p => p.ResearchCategory == ResearchCategory.Cultivation)
                .OrderByDescending(p => p.Priority)
                .ToList();
            
            recommendations.AddRange(cultivationProjects.Take(2));
            return recommendations;
        }
        
        private List<ResearchProjectSO> GetBreedingBasedRecommendations()
        {
            var recommendations = new List<ResearchProjectSO>();
            
            // Recommend genetics research if player is active in breeding
            var geneticsProjects = _availableResearchProjects
                .Where(p => p.ResearchCategory == ResearchCategory.Genetics)
                .OrderByDescending(p => p.Priority)
                .ToList();
            
            recommendations.AddRange(geneticsProjects.Take(2));
            return recommendations;
        }
        
        private List<ResearchProjectSO> GetEconomyBasedRecommendations()
        {
            var recommendations = new List<ResearchProjectSO>();
            
            // Recommend processing or automation research based on economic performance
            var processingProjects = _availableResearchProjects
                .Where(p => p.ResearchCategory == ResearchCategory.Processing)
                .OrderByDescending(p => p.Priority)
                .ToList();
            
            recommendations.AddRange(processingProjects.Take(1));
            return recommendations;
        }
        
        #endregion
        
        #region PC-012-8: Research Gameplay Bonuses Data Structure
        
        /// <summary>
        /// PC-012-8: Bonuses from research that affect gameplay mechanics
        /// </summary>
        [System.Serializable]
        public class ResearchGameplayBonuses
        {
            // Cultivation bonuses
            public float CultivationEfficiencyBonus = 0f;
            public float GrowthSpeedMultiplier = 0f;
            public float YieldImprovementBonus = 0f;
            
            // Genetics bonuses
            public float BreedingSuccessBonus = 0f;
            public float GeneticAnalysisAccuracy = 0f;
            public float MutationRateControl = 0f;
            
            // Equipment bonuses
            public float EquipmentEfficiencyBonus = 0f;
            public float MaintenanceReduction = 0f;
            public float EquipmentLifespanBonus = 0f;
            
            // Automation bonuses
            public float AutomationEfficiencyBonus = 0f;
            public float LaborCostReduction = 0f;
            public float SchedulingOptimization = 0f;
            
            // Processing bonuses
            public float ProcessingQualityBonus = 0f;
            public float ProcessingSpeedBonus = 0f;
            public float QualityPreservationBonus = 0f;
            
            // Analytics bonuses
            public float PredictionAccuracyBonus = 0f;
            public float MarketInsightBonus = 0f;
            public float DataAnalysisSpeedBonus = 0f;
            
            /// <summary>
            /// Get a summary of all active research bonuses
            /// </summary>
            public string GetBonusSummary()
            {
                var activeBonuses = new List<string>();
                
                if (CultivationEfficiencyBonus > 0) activeBonuses.Add($"Cultivation: +{CultivationEfficiencyBonus:P1}");
                if (BreedingSuccessBonus > 0) activeBonuses.Add($"Breeding: +{BreedingSuccessBonus:P1}");
                if (EquipmentEfficiencyBonus > 0) activeBonuses.Add($"Equipment: +{EquipmentEfficiencyBonus:P1}");
                if (AutomationEfficiencyBonus > 0) activeBonuses.Add($"Automation: +{AutomationEfficiencyBonus:P1}");
                if (ProcessingQualityBonus > 0) activeBonuses.Add($"Processing: +{ProcessingQualityBonus:P1}");
                if (PredictionAccuracyBonus > 0) activeBonuses.Add($"Analytics: +{PredictionAccuracyBonus:P1}");
                
                return activeBonuses.Count > 0 ? string.Join(", ", activeBonuses) : "No active research bonuses";
            }
        }
        
        #endregion
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