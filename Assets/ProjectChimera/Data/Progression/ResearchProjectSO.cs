using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Equipment;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Defines research projects that advance scientific understanding and unlock new capabilities
    /// in cannabis cultivation, genetics, processing, and business operations.
    /// </summary>
    [CreateAssetMenu(fileName = "New Research Project", menuName = "Project Chimera/Progression/Research Project")]
    public class ResearchProjectSO : ChimeraDataSO
    {
        [Header("Research Identity")]
        [SerializeField] private string _projectName;
        [SerializeField] private ResearchCategory _researchCategory = ResearchCategory.Genetics;
        [SerializeField] private ResearchType _researchType = ResearchType.Applied_Research;
        [SerializeField, TextArea(3, 6)] private string _researchDescription;
        [SerializeField] private Sprite _projectIcon;
        
        [Header("Research Scope")]
        [SerializeField] private ResearchComplexity _complexity = ResearchComplexity.Intermediate;
        [SerializeField] private ResearchPriority _priority = ResearchPriority.Medium;
        [SerializeField] private List<ResearchObjective> _researchObjectives = new List<ResearchObjective>();
        [SerializeField] private List<Hypothesis> _hypotheses = new List<Hypothesis>();
        
        [Header("Research Requirements")]
        [SerializeField] private ResearchRequirements _requirements;
        [SerializeField] private List<RequiredSkill> _requiredSkills = new List<RequiredSkill>();
        [SerializeField] private List<RequiredEquipment> _requiredEquipment = new List<RequiredEquipment>();
        [SerializeField] private List<RequiredResource> _requiredResources = new List<RequiredResource>();
        
        [Header("Research Timeline")]
        [SerializeField] private ResearchTimeline _timeline;
        [SerializeField] private List<ResearchPhase> _researchPhases = new List<ResearchPhase>();
        [SerializeField] private List<Milestone> _milestones = new List<Milestone>();
        [SerializeField] private bool _allowsParallelExecution = false;
        
        [Header("Research Methodology")]
        [SerializeField] private List<ResearchMethod> _researchMethods = new List<ResearchMethod>();
        [SerializeField] private DataCollectionPlan _dataCollectionPlan;
        [SerializeField] private QualityAssurancePlan _qualityAssurancePlan;
        [SerializeField] private StatisticalDesign _statisticalDesign;
        
        [Header("Expected Outcomes")]
        [SerializeField] private List<ExpectedOutcome> _expectedOutcomes = new List<ExpectedOutcome>();
        [SerializeField] private List<PotentialDiscovery> _potentialDiscoveries = new List<PotentialDiscovery>();
        [SerializeField] private List<TechnologyUnlock> _technologyUnlocks = new List<TechnologyUnlock>();
        [SerializeField] private List<KnowledgeAdvancement> _knowledgeAdvancements = new List<KnowledgeAdvancement>();
        
        [Header("Risk Assessment")]
        [SerializeField] private ResearchRiskProfile _riskProfile;
        [SerializeField] private List<ResearchRisk> _identifiedRisks = new List<ResearchRisk>();
        [SerializeField] private List<ContingencyPlan> _contingencyPlans = new List<ContingencyPlan>();
        [SerializeField] private float _successProbability = 0.7f;
        
        [Header("Collaboration and IP")]
        [SerializeField] private CollaborationStructure _collaborationStructure;
        [SerializeField] private List<ResearchPartner> _researchPartners = new List<ResearchPartner>();
        [SerializeField] private IntellectualPropertyPlan _ipPlan;
        [SerializeField] private bool _allowsOpenResearch = false;
        
        [Header("Post-Research Applications")]
        [SerializeField] private List<CommercialApplication> _commercialApplications = new List<CommercialApplication>();
        [SerializeField] private List<FollowUpResearch> _followUpResearchProjects = new List<FollowUpResearch>();
        [SerializeField] private PublicationPlan _publicationPlan;
        [SerializeField] private PatentStrategy _patentStrategy;
        
        // Public Properties
        public string ProjectName => _projectName;
        public ResearchCategory ResearchCategory => _researchCategory;
        public ResearchType ResearchType => _researchType;
        public string ResearchDescription => _researchDescription;
        public Sprite ProjectIcon => _projectIcon;
        public ResearchComplexity Complexity => _complexity;
        public ResearchPriority Priority => _priority;
        public List<ResearchObjective> ResearchObjectives => _researchObjectives;
        public List<Hypothesis> Hypotheses => _hypotheses;
        public ResearchRequirements Requirements => _requirements;
        public List<RequiredSkill> RequiredSkills => _requiredSkills;
        public List<RequiredEquipment> RequiredEquipment => _requiredEquipment;
        public List<RequiredResource> RequiredResources => _requiredResources;
        public ResearchTimeline Timeline => _timeline;
        public List<ResearchPhase> ResearchPhases => _researchPhases;
        public List<Milestone> Milestones => _milestones;
        public bool AllowsParallelExecution => _allowsParallelExecution;
        public List<ResearchMethod> ResearchMethods => _researchMethods;
        public DataCollectionPlan DataCollectionPlan => _dataCollectionPlan;
        public QualityAssurancePlan QualityAssurancePlan => _qualityAssurancePlan;
        public StatisticalDesign StatisticalDesign => _statisticalDesign;
        public List<ExpectedOutcome> ExpectedOutcomes => _expectedOutcomes;
        public List<PotentialDiscovery> PotentialDiscoveries => _potentialDiscoveries;
        public List<TechnologyUnlock> TechnologyUnlocks => _technologyUnlocks;
        public List<KnowledgeAdvancement> KnowledgeAdvancements => _knowledgeAdvancements;
        public ResearchRiskProfile RiskProfile => _riskProfile;
        public List<ResearchRisk> IdentifiedRisks => _identifiedRisks;
        public List<ContingencyPlan> ContingencyPlans => _contingencyPlans;
        public float SuccessProbability => _successProbability;
        public CollaborationStructure CollaborationStructure => _collaborationStructure;
        public List<ResearchPartner> ResearchPartners => _researchPartners;
        public IntellectualPropertyPlan IPPlan => _ipPlan;
        public bool AllowsOpenResearch => _allowsOpenResearch;
        public List<CommercialApplication> CommercialApplications => _commercialApplications;
        public List<FollowUpResearch> FollowUpResearchProjects => _followUpResearchProjects;
        public PublicationPlan PublicationPlan => _publicationPlan;
        public PatentStrategy PatentStrategy => _patentStrategy;
        
        // Additional properties for compatibility
        public string ProjectId => name; // Use ScriptableObject name as ID
        public string ResearchId => name; // Compatibility alias for ProjectId
        public ResearchTier Tier => (ResearchTier)((int)_complexity); // Map complexity to tier
        public List<string> Prerequisites => new List<string>(); // Empty list of prerequisite IDs for now
        public List<RequiredSkill> SkillRequirements => _requiredSkills; // Alias
        public float EstimatedDurationHours => _timeline?.EstimatedDurationDays * 24f ?? 0f; // Convert days to hours
        public List<string> Keywords => new List<string> { _projectName, _researchCategory.ToString(), _researchType.ToString() }; // Generate keywords from project data
        
        // Additional compatibility properties for ComprehensiveProgressionManager
        public string ResearchName => _projectName; // Compatibility alias
        public float ResearchDuration => EstimatedDurationHours; // Compatibility alias
        public List<string> ContentUnlocks => new List<string>(); // Empty list for now
        public List<PassiveBonus> PassiveBonuses => new List<PassiveBonus>(); // Empty list for now
        
        // UI Compatibility properties
        public int ResearchCost => (int)(_requirements?.TotalBudgetRequired ?? 100f); // Default cost of 100 if no requirements
        public int ResearchTimeMinutes => (int)(EstimatedDurationHours * 60f); // Convert hours to minutes
        
        /// <summary>
        /// Evaluates if the player can initiate this research project.
        /// </summary>
        public ResearchFeasibility EvaluateResearchFeasibility(PlayerResearchCapabilities playerCapabilities)
        {
            var feasibility = new ResearchFeasibility();
            
            // Check skill requirements
            feasibility.SkillsAdequate = EvaluateSkillRequirements(playerCapabilities);
            
            // Check equipment availability
            feasibility.EquipmentAvailable = EvaluateEquipmentRequirements(playerCapabilities);
            
            // Check resource availability
            feasibility.ResourcesAvailable = EvaluateResourceRequirements(playerCapabilities);
            
            // Check budget adequacy
            feasibility.BudgetAdequate = playerCapabilities.AvailableBudget >= _requirements.TotalBudgetRequired;
            
            // Check time availability
            feasibility.TimeAvailable = playerCapabilities.AvailableResearchTime >= _timeline.EstimatedDurationDays;
            
            // Calculate overall feasibility score
            feasibility.OverallFeasibility = CalculateOverallFeasibility(feasibility);
            
            return feasibility;
        }
        
        /// <summary>
        /// Calculates the current research progress based on completed phases and milestones.
        /// </summary>
        public ResearchProgress CalculateResearchProgress(List<CompletedPhase> completedPhases, List<CompletedMilestone> completedMilestones)
        {
            var progress = new ResearchProgress();
            
            // Calculate phase progress
            float totalPhaseWeight = 0f;
            float completedPhaseWeight = 0f;
            
            foreach (var phase in _researchPhases)
            {
                totalPhaseWeight += phase.PhaseWeight;
                var completed = completedPhases.Find(cp => cp.PhaseID == phase.PhaseID);
                if (completed != null)
                {
                    completedPhaseWeight += phase.PhaseWeight;
                }
            }
            
            progress.PhaseProgress = totalPhaseWeight > 0 ? completedPhaseWeight / totalPhaseWeight : 0f;
            
            // Calculate milestone progress
            progress.MilestoneProgress = _milestones.Count > 0 ? (float)completedMilestones.Count / _milestones.Count : 0f;
            
            // Overall progress is calculated automatically from PhaseProgress and MilestoneProgress
            
            // Estimate time remaining
            progress.EstimatedTimeRemaining = CalculateRemainingTime(progress.OverallProgress);
            
            return progress;
        }
        
        /// <summary>
        /// Generates research outcomes based on success factors and random elements.
        /// </summary>
        public ResearchResults GenerateResearchResults(float researchQuality, float teamExpertise, bool hadSetbacks)
        {
            var results = new ResearchResults();
            
            // Calculate success probability modifiers
            float modifiedSuccessProbability = _successProbability;
            modifiedSuccessProbability *= researchQuality;
            modifiedSuccessProbability *= teamExpertise;
            
            if (hadSetbacks)
                modifiedSuccessProbability *= 0.8f;
            
            // Determine if research succeeded
            results.WasSuccessful = Random.value < modifiedSuccessProbability;
            
            if (results.WasSuccessful)
            {
                // Generate positive outcomes
                results.AchievedOutcomes = GenerateAchievedOutcomes(researchQuality);
                results.UnlocksTechnologies = GenerateUnlockedTechnologies(researchQuality);
                results.KnowledgeGains = GenerateKnowledgeGains(teamExpertise);
                
                // Check for unexpected discoveries
                if (Random.value < 0.2f * researchQuality)
                {
                    results.UnexpectedDiscoveries = GenerateUnexpectedDiscoveries();
                }
            }
            else
            {
                // Generate partial results or lessons learned
                results.PartialResults = GeneratePartialResults();
                results.LessonsLearned = GenerateLessonsLearned();
            }
            
            // Calculate commercial value
            results.CommercialValue = CalculateCommercialValue(results);
            
            return results;
        }
        
        /// <summary>
        /// Estimates the return on investment for this research project.
        /// </summary>
        public ResearchROI EstimateResearchROI(MarketConditions marketConditions)
        {
            var roi = new ResearchROI();
            
            // Calculate investment cost
            roi.TotalInvestment = _requirements.TotalBudgetRequired;
            
            // Estimate potential revenue from commercial applications
            float potentialRevenue = 0f;
            foreach (var application in _commercialApplications)
            {
                potentialRevenue += application.EstimatedRevenue * application.SuccessProbability;
            }
            
            // Apply market condition modifiers
            potentialRevenue *= marketConditions.MarketGrowthRate;
            
            // Calculate ROI metrics
            roi.ExpectedRevenue = potentialRevenue * _successProbability;
            roi.ExpectedProfit = roi.ExpectedRevenue - roi.TotalInvestment;
            roi.ROIPercentage = roi.TotalInvestment > 0 ? (roi.ExpectedProfit / roi.TotalInvestment) * 100f : 0f;
            roi.PaybackPeriodMonths = CalculatePaybackPeriod(roi.ExpectedRevenue, roi.TotalInvestment);
            
            return roi;
        }
        
        /// <summary>
        /// Gets the optimal research strategy based on available resources and constraints.
        /// </summary>
        public ResearchStrategy GetOptimalResearchStrategy(PlayerResearchCapabilities capabilities, ResearchConstraints constraints)
        {
            var strategy = new ResearchStrategy();
            
            // Determine execution approach
            if (_allowsParallelExecution && capabilities.CanManageParallelProjects)
            {
                strategy.ExecutionApproach = ResearchExecutionApproach.Parallel;
                strategy.EstimatedDuration = _timeline.EstimatedDurationDays * 0.8f; // 20% time savings
            }
            else
            {
                strategy.ExecutionApproach = ResearchExecutionApproach.Sequential;
                strategy.EstimatedDuration = _timeline.EstimatedDurationDays;
            }
            
            // Determine collaboration strategy
            if (_researchPartners.Count > 0 && constraints.AllowsCollaboration)
            {
                strategy.CollaborationLevel = CollaborationLevel.High;
                strategy.CostSharing = 0.3f; // 30% cost reduction through collaboration
            }
            else
            {
                strategy.CollaborationLevel = CollaborationLevel.Independent;
                strategy.CostSharing = 0f;
            }
            
            // Determine quality vs speed tradeoff
            if (constraints.TimeConstrained)
            {
                strategy.QualityVsSpeedBalance = 0.3f; // Favor speed
                strategy.SuccessProbabilityModifier = 0.9f; // Slight reduction in success probability
            }
            else
            {
                strategy.QualityVsSpeedBalance = 0.7f; // Favor quality
                strategy.SuccessProbabilityModifier = 1.1f; // Slight increase in success probability
            }
            
            return strategy;
        }
        
        private bool EvaluateSkillRequirements(PlayerResearchCapabilities capabilities)
        {
            foreach (var skillReq in _requiredSkills)
            {
                if (!capabilities.HasSkillAtLevel(skillReq.RequiredSkillNode, skillReq.MinimumLevel))
                    return false;
            }
            return true;
        }
        
        private bool EvaluateEquipmentRequirements(PlayerResearchCapabilities capabilities)
        {
            foreach (var equipReq in _requiredEquipment)
            {
                if (!capabilities.HasEquipment(equipReq.Equipment))
                    return false;
            }
            return true;
        }
        
        private bool EvaluateResourceRequirements(PlayerResearchCapabilities capabilities)
        {
            foreach (var resourceReq in _requiredResources)
            {
                if (!capabilities.HasResource(resourceReq.ResourceName, resourceReq.RequiredQuantity))
                    return false;
            }
            return true;
        }
        
        private float CalculateOverallFeasibility(ResearchFeasibility feasibility)
        {
            float score = 0f;
            if (feasibility.SkillsAdequate) score += 0.3f;
            if (feasibility.EquipmentAvailable) score += 0.25f;
            if (feasibility.ResourcesAvailable) score += 0.2f;
            if (feasibility.BudgetAdequate) score += 0.15f;
            if (feasibility.TimeAvailable) score += 0.1f;
            
            return score;
        }
        
        private float CalculateRemainingTime(float overallProgress)
        {
            float remainingProgress = 1f - overallProgress;
            return _timeline.EstimatedDurationDays * remainingProgress;
        }
        
        private List<ExpectedOutcome> GenerateAchievedOutcomes(float researchQuality)
        {
            var achieved = new List<ExpectedOutcome>();
            
            foreach (var outcome in _expectedOutcomes)
            {
                float achievementProbability = outcome.SuccessProbability * researchQuality;
                if (Random.value < achievementProbability)
                    achieved.Add(outcome);
            }
            
            return achieved;
        }
        
        private List<TechnologyUnlock> GenerateUnlockedTechnologies(float researchQuality)
        {
            var unlocked = new List<TechnologyUnlock>();
            
            foreach (var tech in _technologyUnlocks)
            {
                float unlockProbability = tech.UnlockProbability * researchQuality;
                if (Random.value < unlockProbability)
                    unlocked.Add(tech);
            }
            
            return unlocked;
        }
        
        private List<KnowledgeAdvancement> GenerateKnowledgeGains(float teamExpertise)
        {
            var gains = new List<KnowledgeAdvancement>();
            
            foreach (var advancement in _knowledgeAdvancements)
            {
                float gainProbability = advancement.DiscoveryProbability * teamExpertise;
                if (Random.value < gainProbability)
                    gains.Add(advancement);
            }
            
            return gains;
        }
        
        private List<PotentialDiscovery> GenerateUnexpectedDiscoveries()
        {
            var discoveries = new List<PotentialDiscovery>();
            
            if (_potentialDiscoveries.Count > 0)
            {
                int numDiscoveries = Random.Range(1, Mathf.Min(3, _potentialDiscoveries.Count + 1));
                for (int i = 0; i < numDiscoveries; i++)
                {
                    var discovery = _potentialDiscoveries[Random.Range(0, _potentialDiscoveries.Count)];
                    if (!discoveries.Contains(discovery))
                        discoveries.Add(discovery);
                }
            }
            
            return discoveries;
        }
        
        private List<string> GeneratePartialResults()
        {
            var results = new List<string>();
            
            // Generate partial results based on research objectives
            foreach (var objective in _researchObjectives)
            {
                if (Random.value < 0.4f) // 40% chance of partial success per objective
                {
                    results.Add($"Partial progress on: {objective.ObjectiveName}");
                }
            }
            
            return results;
        }
        
        private List<string> GenerateLessonsLearned()
        {
            var lessons = new List<string>
            {
                "Improved understanding of research methodology",
                "Identified key variables for future studies",
                "Refined experimental procedures",
                "Enhanced team collaboration skills",
                "Better resource planning for future projects"
            };
            
            // Return 1-3 random lessons
            var selectedLessons = new List<string>();
            int numLessons = Random.Range(1, 4);
            for (int i = 0; i < numLessons; i++)
            {
                var lesson = lessons[Random.Range(0, lessons.Count)];
                if (!selectedLessons.Contains(lesson))
                    selectedLessons.Add(lesson);
            }
            
            return selectedLessons;
        }
        
        private float CalculateCommercialValue(ResearchResults results)
        {
            float value = 0f;
            
            // Value from achieved outcomes
            foreach (var outcome in results.AchievedOutcomes)
            {
                value += outcome.CommercialValue;
            }
            
            // Value from technology unlocks
            foreach (var tech in results.UnlocksTechnologies)
            {
                value += tech.CommercialValue;
            }
            
            // Bonus value from unexpected discoveries
            foreach (var discovery in results.UnexpectedDiscoveries)
            {
                value += discovery.CommercialValue * 1.5f; // Unexpected discoveries have higher value
            }
            
            return value;
        }
        
        private float CalculatePaybackPeriod(float expectedRevenue, float totalInvestment)
        {
            if (expectedRevenue <= 0f) return float.MaxValue;
            
            // Assume revenue is generated over 3 years
            float monthlyRevenue = expectedRevenue / 36f;
            return totalInvestment / monthlyRevenue;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_projectName))
            {
                Debug.LogError($"ResearchProjectSO '{name}' has no project name assigned.", this);
                isValid = false;
            }
            
            if (_timeline.EstimatedDurationDays <= 0f)
            {
                Debug.LogError($"ResearchProjectSO '{name}' has invalid research time: {_timeline.EstimatedDurationDays} days", this);
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    // Note: Supporting data structures are defined in ResearchDataStructures.cs
}