using UnityEngine;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Supporting data structures for the research and progression system.
    /// Includes comprehensive research management, collaboration, and outcome tracking.
    /// </summary>
    
    [System.Serializable]
    public class ResearchObjective
    {
        public string ObjectiveName;
        public ObjectiveType ObjectiveType;
        public ObjectivePriority Priority = ObjectivePriority.Should_Have;
        [Range(0f, 1f)] public float SuccessProbability = 0.7f;
        public bool IsCriticalObjective = false;
        [TextArea(2, 4)] public string ObjectiveDescription;
        public List<string> SuccessMetrics = new List<string>();
    }
    
    [System.Serializable]
    public class Hypothesis
    {
        public string HypothesisStatement;
        public HypothesisType HypothesisType;
        [Range(0f, 1f)] public float ConfidenceLevel = 0.6f;
        public bool IsTestable = true;
        public bool IsNullHypothesis = false;
        [TextArea(2, 4)] public string RationaleAndEvidence;
        public List<string> TestingMethods = new List<string>();
    }
    
    [System.Serializable]
    public class ResearchRequirements
    {
        [Range(0f, 1000000f)] public float TotalBudgetRequired = 10000f;
        [Range(1, 50)] public int MinimumTeamSize = 1;
        [Range(1, 10)] public int MinimumExpertiseLevel = 3;
        [Range(1, 1000)] public int EstimatedPersonHours = 100;
        public bool RequiresEthicsApproval = false;
        public bool RequiresRegulatoryApproval = false;
        public List<string> RequiredCertifications = new List<string>();
    }
    
    [System.Serializable]
    public class RequiredSkill
    {
        public SkillNodeSO RequiredSkillNode;
        [Range(1, 10)] public int MinimumLevel = 3;
        public bool IsCriticalSkill = true;
        public bool CanBeOutsourced = false;
        public string SkillApplicationDescription;
        
        // Compatibility properties
        public string SkillId => RequiredSkillNode?.SkillId ?? "";
        public int RequiredLevel => MinimumLevel;
        public int MinLevel => MinimumLevel; // Compatibility alias
    }
    
    [System.Serializable]
    public class RequiredEquipment
    {
        public EquipmentDataSO Equipment;
        public bool IsCriticalEquipment = true;
        public bool CanBeRented = true;
        public bool CanBeShared = false;
        [Range(0f, 100f)] public float UtilizationPercentage = 50f;
        public string EquipmentUsageDescription;
    }
    
    [System.Serializable]
    public class RequiredResource
    {
        public string ResourceName;
        public ResourceType ResourceType;
        [Range(1, 10000)] public int RequiredQuantity = 10;
        public string ResourceUnit = "units";
        [Range(0f, 10000f)] public float ResourceCost = 100f;
        public bool IsConsumable = true;
        public string ResourceUsageDescription;
    }
    
    [System.Serializable]
    public class ResearchTimeline
    {
        [Range(1, 1000)] public int EstimatedDurationDays = 90;
        [Range(1, 100)] public int NumberOfPhases = 3;
        public bool HasFlexibleTimeline = true;
        [Range(0f, 1f)] public float TimeBufferPercentage = 0.2f;
        public List<CriticalPath> CriticalPaths = new List<CriticalPath>();
        public List<TimeConstraint> TimeConstraints = new List<TimeConstraint>();
    }
    
    [System.Serializable]
    public class ResearchPhase
    {
        public string PhaseID;
        public string PhaseName;
        public PhaseType PhaseType;
        [Range(1, 365)] public int EstimatedDurationDays = 30;
        [Range(0f, 1f)] public float PhaseWeight = 0.33f;
        public List<PhaseActivity> PhaseActivities = new List<PhaseActivity>();
        public List<string> Deliverables = new List<string>();
        public List<ResearchPhase> PrerequisitePhases = new List<ResearchPhase>();
        [TextArea(2, 4)] public string PhaseDescription;
    }
    
    [System.Serializable]
    public class PhaseActivity
    {
        public string ActivityName;
        public ActivityType ActivityType;
        [Range(1, 100)] public int EstimatedHours = 10;
        public bool IsCriticalActivity = false;
        public List<string> RequiredSkills = new List<string>();
        public List<string> RequiredResources = new List<string>();
        public string ActivityDescription;
    }
    
    [System.Serializable]
    public class Milestone
    {
        public string MilestoneID;
        public string MilestoneName;
        public ResearchMilestoneType MilestoneType;
        [Range(1, 365)] public int TargetDay = 30;
        public bool IsCriticalMilestone = false;
        public List<string> DeliverableCriteria = new List<string>();
        public List<string> SuccessMetrics = new List<string>();
        [TextArea(2, 4)] public string MilestoneDescription;
    }
    
    [System.Serializable]
    public class ResearchMethod
    {
        public string MethodName;
        public ResearchMethodType MethodType;
        public MethodologyApproach MethodologyApproach;
        [Range(0f, 2f)] public float AccuracyMultiplier = 1f;
        [Range(0f, 2f)] public float SpeedMultiplier = 1f;
        [Range(0f, 2f)] public float CostMultiplier = 1f;
        public bool RequiresSpecializedEquipment = false;
        public List<string> MethodLimitations = new List<string>();
        [TextArea(2, 4)] public string MethodDescription;
    }
    
    [System.Serializable]
    public class DataCollectionPlan
    {
        public List<DataType> DataTypesToCollect = new List<DataType>();
        public List<DataSource> DataSources = new List<DataSource>();
        public DataCollectionFrequency CollectionFrequency = DataCollectionFrequency.Daily;
        public int SampleSize = 100;
        public bool RequiresDataValidation = true;
        public StorageRequirements DataStorageRequirements;
        [TextArea(2, 4)] public string DataCollectionProtocol;
    }
    
    [System.Serializable]
    public class DataSource
    {
        public string SourceName;
        public DataSourceType SourceType;
        public bool IsReliableSource = true;
        [Range(0f, 1f)] public float DataQualityScore = 0.8f;
        public string AccessRequirements;
        public string SourceDescription;
    }
    
    [System.Serializable]
    public class StorageRequirements
    {
        [Range(1, 10000)] public int EstimatedStorageGB = 10;
        public bool RequiresSecureStorage = false;
        public bool RequiresBackup = true;
        [Range(1, 3650)] public int DataRetentionDays = 365;
        public string StorageFormat = "Database";
    }
    
    [System.Serializable]
    public class QualityAssurancePlan
    {
        public List<QualityControl> QualityControls = new List<QualityControl>();
        public bool RequiresPeerReview = true;
        public bool RequiresIndependentVerification = false;
        [Range(0f, 1f)] public float AcceptableErrorRate = 0.05f;
        public List<ValidationMethod> ValidationMethods = new List<ValidationMethod>();
        [TextArea(2, 4)] public string QualityStandards;
    }
    
    [System.Serializable]
    public class QualityControl
    {
        public string ControlName;
        public QualityControlType ControlType;
        public ControlFrequency Frequency = ControlFrequency.Weekly;
        public bool IsAutomated = false;
        public string ControlCriteria;
        public string ControlDescription;
    }
    
    [System.Serializable]
    public class ValidationMethod
    {
        public string MethodName;
        public ValidationType ValidationType;
        [Range(0f, 1f)] public float ValidationAccuracy = 0.95f;
        public bool RequiresExternalValidator = false;
        public string ValidationDescription;
    }
    
    [System.Serializable]
    public class StatisticalDesign
    {
        public ExperimentalDesign ExperimentalDesign;
        public StatisticalMethod PrimaryStatisticalMethod;
        public List<StatisticalTest> PlannedStatisticalTests = new List<StatisticalTest>();
        [Range(0.01f, 0.1f)] public float SignificanceLevel = 0.05f;
        [Range(0.5f, 0.99f)] public float PowerLevel = 0.8f;
        public bool RequiresRandomization = true;
        public bool RequiresBlinding = false;
        [TextArea(2, 4)] public string StatisticalRationale;
    }
    
    [System.Serializable]
    public class StatisticalTest
    {
        public string TestName;
        public TestType TestType;
        public string TestPurpose;
        public List<string> TestAssumptions = new List<string>();
        public string InterpretationGuidelines;
    }
    
    [System.Serializable]
    public class ExpectedOutcome
    {
        public string OutcomeName;
        public OutcomeType OutcomeType;
        [Range(0f, 1f)] public float SuccessProbability = 0.7f;
        [Range(0f, 1000000f)] public float CommercialValue = 10000f;
        public ImpactLevel ImpactLevel = ImpactLevel.Medium;
        public List<string> OutcomeMetrics = new List<string>();
        [TextArea(2, 4)] public string OutcomeDescription;
    }
    
    [System.Serializable]
    public class PotentialDiscovery
    {
        public string DiscoveryName;
        public DiscoveryType DiscoveryType;
        [Range(0f, 0.5f)] public float DiscoveryProbability = 0.1f;
        [Range(0f, 10000000f)] public float CommercialValue = 100000f;
        public NoveltyLevel NoveltyLevel = NoveltyLevel.Incremental;
        public bool RequiresPatentProtection = false;
        [TextArea(2, 4)] public string DiscoveryDescription;
    }
    
    [System.Serializable]
    public class TechnologyUnlock
    {
        public string TechnologyName;
        public TechnologyType TechnologyType;
        [Range(0f, 1f)] public float UnlockProbability = 0.5f;
        [Range(0f, 1000000f)] public float CommercialValue = 50000f;
        public TechnologyReadiness TechnologyReadinessLevel = TechnologyReadiness.Proof_of_Concept;
        public List<string> ApplicationAreas = new List<string>();
        [TextArea(2, 4)] public string TechnologyDescription;
    }
    
    [System.Serializable]
    public class KnowledgeAdvancement
    {
        public string AdvancementName;
        public KnowledgeType KnowledgeType;
        [Range(0f, 1f)] public float DiscoveryProbability = 0.6f;
        public KnowledgeImpact KnowledgeImpact = KnowledgeImpact.Significant;
        public bool IsPublishable = true;
        public bool IsProprietary = false;
        [TextArea(2, 4)] public string AdvancementDescription;
    }
    
    [System.Serializable]
    public class ResearchRiskProfile
    {
        [Range(0f, 1f)] public float TechnicalRisk = 0.3f;
        [Range(0f, 1f)] public float FinancialRisk = 0.2f;
        [Range(0f, 1f)] public float TimelineRisk = 0.4f;
        [Range(0f, 1f)] public float MarketRisk = 0.3f;
        [Range(0f, 1f)] public float RegulatoryRisk = 0.2f;
        [Range(0f, 1f)] public float CompetitiveRisk = 0.3f;
        [Range(0f, 1f)] public float OverallRiskLevel = 0.3f;
    }
    
    [System.Serializable]
    public class ResearchRisk
    {
        public string RiskName;
        public RiskType RiskType;
        [Range(0f, 1f)] public float RiskProbability = 0.2f;
        public RiskImpact RiskImpact = RiskImpact.Medium;
        [Range(0f, 1000000f)] public float PotentialCost = 5000f;
        [Range(0, 100)] public int PotentialDelayDays = 10;
        public List<string> MitigationStrategies = new List<string>();
        [TextArea(2, 4)] public string RiskDescription;
    }
    
    [System.Serializable]
    public class ContingencyPlan
    {
        public string PlanName;
        public string TriggerCondition;
        public ContingencyType ContingencyType;
        public List<ContingencyAction> ContingencyActions = new List<ContingencyAction>();
        [Range(0f, 1000000f)] public float ContingencyCost = 2000f;
        [Range(0, 50)] public int ImplementationTimeDays = 5;
        [TextArea(2, 4)] public string PlanDescription;
    }
    
    [System.Serializable]
    public class ContingencyAction
    {
        public string ActionName;
        public ActionType ActionType;
        public string ActionDescription;
        public List<string> RequiredResources = new List<string>();
        [Range(0, 30)] public int ActionTimeDays = 3;
    }
    
    [System.Serializable]
    public class CollaborationStructure
    {
        public bool AllowsCollaboration = false;
        public CollaborationType PreferredCollaborationType = CollaborationType.Academic;
        [Range(0f, 0.8f)] public float MaxCostSharingPercentage = 0.3f;
        [Range(0f, 0.8f)] public float MaxIPSharingPercentage = 0.2f;
        public bool RequiresNDA = true;
        public List<ResearchCollaborationBenefit> ExpectedBenefits = new List<ResearchCollaborationBenefit>();
    }
    
    [System.Serializable]
    public class ResearchPartner
    {
        public string PartnerName;
        public PartnerType PartnerType;
        public PartnerExpertise PartnerExpertise;
        [Range(0f, 1f)] public float PartnerContribution = 0.3f;
        [Range(0f, 1f)] public float PartnerReliability = 0.8f;
        public bool HasPreviousCollaboration = false;
        public List<string> PartnerCapabilities = new List<string>();
        [TextArea(2, 4)] public string PartnerDescription;
    }
    
    [System.Serializable]
    public class ResearchCollaborationBenefit
    {
        public string BenefitName;
        public BenefitType BenefitType;
        [Range(0f, 2f)] public float BenefitMultiplier = 1.2f;
        public string BenefitDescription;
    }
    
    [System.Serializable]
    public class IntellectualPropertyPlan
    {
        public IPStrategy IPStrategy = IPStrategy.Patent_Protection;
        public bool AllowsOpenSource = false;
        [Range(0f, 1f)] public float IPOwnershipPercentage = 1f;
        public List<IPAsset> ExpectedIPAssets = new List<IPAsset>();
        [Range(0f, 1000000f)] public float EstimatedIPValue = 100000f;
        public bool RequiresPatentSearch = true;
        [TextArea(2, 4)] public string IPStrategyDescription;
    }
    
    [System.Serializable]
    public class IPAsset
    {
        public string AssetName;
        public IPAssetType AssetType;
        [Range(0f, 1000000f)] public float EstimatedValue = 50000f;
        public bool RequiresProtection = true;
        public List<string> ProtectionMethods = new List<string>();
        public string AssetDescription;
    }
    
    [System.Serializable]
    public class CommercialApplication
    {
        public string ApplicationName;
        public ApplicationSector ApplicationSector;
        [Range(0f, 1f)] public float SuccessProbability = 0.6f;
        [Range(0f, 10000000f)] public float EstimatedRevenue = 500000f;
        [Range(0, 1000)] public int TimeToMarketDays = 365;
        public List<string> MarketRequirements = new List<string>();
        [TextArea(2, 4)] public string ApplicationDescription;
    }
    
    [System.Serializable]
    public class FollowUpResearch
    {
        public ResearchProjectSO FollowUpProject;
        public string ResearchContinuation;
        [Range(0f, 1f)] public float RecommendationProbability = 0.4f;
        public List<string> BuildsUponFindings = new List<string>();
        public string ContinuationDescription;
    }
    
    [System.Serializable]
    public class PublicationPlan
    {
        public bool AllowsPublication = true;
        public List<PublicationType> PlannedPublications = new List<PublicationType>();
        [Range(0, 365)] public int EmbargoperiodDays = 180;
        public bool RequiresPeerReview = true;
        public List<string> TargetJournals = new List<string>();
        [TextArea(2, 4)] public string PublicationStrategy;
    }
    
    [System.Serializable]
    public class PatentStrategy
    {
        public bool PursuesPatents = true;
        public List<PatentType> PlannedPatentTypes = new List<PatentType>();
        [Range(0f, 500000f)] public float PatentingBudget = 50000f;
        public List<string> PatentJurisdictions = new List<string>();
        [Range(0, 365)] public int PatentFilingDeadlineDays = 90;
        [TextArea(2, 4)] public string PatentStrategyDescription;
    }
    
    // Research progress and results tracking
    [System.Serializable]
    public class PlayerResearchCapabilities
    {
        public float AvailableBudget = 100000f;
        public int AvailableResearchTime = 180; // days
        public bool CanManageParallelProjects = false;
        public Dictionary<SkillNodeSO, int> SkillLevels = new Dictionary<SkillNodeSO, int>();
        public List<EquipmentDataSO> AvailableEquipment = new List<EquipmentDataSO>();
        public Dictionary<string, int> AvailableResources = new Dictionary<string, int>();
        
        public bool HasSkillAtLevel(SkillNodeSO skill, int level)
        {
            return SkillLevels.ContainsKey(skill) && SkillLevels[skill] >= level;
        }
        
        public bool HasEquipment(EquipmentDataSO equipment)
        {
            return AvailableEquipment.Contains(equipment);
        }
        
        public bool HasResource(string resourceName, int quantity)
        {
            return AvailableResources.ContainsKey(resourceName) && AvailableResources[resourceName] >= quantity;
        }
    }
    
    [System.Serializable]
    public class ResearchFeasibility
    {
        public bool SkillsAdequate;
        public bool EquipmentAvailable;
        public bool ResourcesAvailable;
        public bool BudgetAdequate;
        public bool TimeAvailable;
        public float OverallFeasibility;
    }
    

    
    [System.Serializable]
    public class CompletedPhase
    {
        public string PhaseID;
        public float CompletionQuality;
        public int ActualDurationDays;
        public float ActualCost;
        public List<string> Deliverables = new List<string>();
    }
    
    [System.Serializable]
    public class CompletedMilestone
    {
        public string MilestoneID;
        public bool WasSuccessful;
        public float QualityScore;
        public int DaysToComplete;
        public List<string> AchievedCriteria = new List<string>();
    }
    
    [System.Serializable]
    public class ResearchResults
    {
        public bool WasSuccessful;
        public List<ExpectedOutcome> AchievedOutcomes = new List<ExpectedOutcome>();
        public List<TechnologyUnlock> UnlocksTechnologies = new List<TechnologyUnlock>();
        public List<KnowledgeAdvancement> KnowledgeGains = new List<KnowledgeAdvancement>();
        public List<PotentialDiscovery> UnexpectedDiscoveries = new List<PotentialDiscovery>();
        public List<string> PartialResults = new List<string>();
        public List<string> LessonsLearned = new List<string>();
        public float CommercialValue;
    }
    
    [System.Serializable]
    public class ResearchROI
    {
        public float TotalInvestment;
        public float ExpectedRevenue;
        public float ExpectedProfit;
        public float ROIPercentage;
        public float PaybackPeriodMonths;
    }
    
    [System.Serializable]
    public class MarketConditions
    {
        public float MarketGrowthRate = 1.1f;
        public float CompetitionLevel = 0.5f;
        public float RegulatoryStability = 0.8f;
        public float InvestmentClimate = 0.7f;
    }
    
    [System.Serializable]
    public class ResearchStrategy
    {
        public ResearchExecutionApproach ExecutionApproach;
        public CollaborationLevel CollaborationLevel;
        public float EstimatedDuration;
        public float CostSharing;
        public float QualityVsSpeedBalance;
        public float SuccessProbabilityModifier;
    }
    
    [System.Serializable]
    public class ResearchBoost
    {
        public string BoostName;
        public List<ResearchCategory> ApplicableCategories = new List<ResearchCategory>();
        public float SpeedMultiplier = 1.5f;
        public float QualityMultiplier = 1.2f;
        public float SuccessProbabilityBonus = 0.1f;
        public int DurationDays = 30;
        public int UsageLimit = -1; // -1 for unlimited
        public float Cost = 5000f;
        public string BoostDescription;
    }

    [System.Serializable]
    public class ActiveResearchBoost
    {
        public ResearchBoost Boost;
        public System.DateTime StartDate;
        public System.DateTime ExpirationDate;
        public int RemainingUses;
    }

    [System.Serializable]
    public class ActiveResearchProject
    {
        public ResearchProjectSO ResearchProject;
        public ResearchStatus Status;
        public System.DateTime StartDate;
        public System.DateTime CompletionDate;
        public int CurrentPhaseIndex;
        public float Progress; // 0-1
        public List<CompletedPhase> CompletedPhases = new List<CompletedPhase>();
        public List<CompletedMilestone> CompletedMilestones = new List<CompletedMilestone>();
        public float TotalInvestment;
        public float CurrentQuality;
        public float TeamExpertise;
        public bool HadSetbacks;
        public float ResearchSpeedMultiplier = 1f;
        public float QualityBonusMultiplier = 1f;
        public float SuccessProbabilityBonus = 0f;
        public List<ActiveResearchBoost> ActiveBoosts = new List<ActiveResearchBoost>();
        public ResearchResults Results;
        
        // Additional fields for compatibility
        public ResearchProjectSO Project; // Alias for ResearchProject
        public float QualityScore; // Alias for CurrentQuality
    }

    [System.Serializable]
    public class ResearchConstraints
    {
        public bool TimeConstrained = false;
        public bool BudgetConstrained = false;
        public bool AllowsCollaboration = true;
        public bool RequiresHighConfidentiality = false;
        public float MaxAcceptableRisk = 0.5f;
    }
    
    [System.Serializable]
    public class CriticalPath
    {
        public string PathName;
        public List<string> PathActivities = new List<string>();
        public int TotalPathDuration;
        public float PathRisk;
        public string PathDescription;
    }
    
    [System.Serializable]
    public class TimeConstraint
    {
        public string ConstraintName;
        public ConstraintType ConstraintType;
        public int ConstraintDate; // Day number
        public bool IsHardConstraint = true;
        public string ConstraintDescription;
    }
    
    // Comprehensive enum definitions for the research system
    public enum ResearchCategory
    {
        Genetics,
        Cultivation,
        Processing,
        Quality_Control,
        Environmental_Science,
        Business_Innovation,
        Technology_Development,
        Market_Research,
        Regulatory_Compliance,
        Sustainability
    }
    
    public enum ResearchType
    {
        Basic_Research,
        Applied_Research,
        Development_Research,
        Market_Research,
        Feasibility_Study,
        Comparative_Study,
        Longitudinal_Study,
        Case_Study,
        Pilot_Study,
        Clinical_Trial
    }
    
    public enum ResearchComplexity
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Cutting_Edge
    }
    
    public enum ResearchPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Strategic
    }
    
    public enum ObjectiveType
    {
        Primary_Objective,
        Secondary_Objective,
        Exploratory_Objective,
        Safety_Objective,
        Regulatory_Objective,
        Commercial_Objective
    }
    
    public enum ObjectivePriority
    {
        Must_Have,
        Should_Have,
        Could_Have,
        Won_t_Have
    }
    
    public enum HypothesisType
    {
        Causal_Hypothesis,
        Descriptive_Hypothesis,
        Relational_Hypothesis,
        Comparative_Hypothesis,
        Null_Hypothesis,
        Alternative_Hypothesis
    }
    
    public enum ResourceType
    {
        Consumable_Material,
        Equipment_Time,
        Human_Resource,
        Laboratory_Access,
        Data_Access,
        Software_License,
        Facility_Space,
        External_Service
    }
    
    public enum PhaseType
    {
        Planning_Phase,
        Literature_Review,
        Methodology_Development,
        Data_Collection,
        Analysis_Phase,
        Validation_Phase,
        Reporting_Phase,
        Dissemination_Phase
    }
    
    public enum ActivityType
    {
        Research_Activity,
        Data_Collection,
        Analysis_Activity,
        Documentation,
        Review_Activity,
        Validation_Activity,
        Collaboration_Activity,
        Reporting_Activity
    }
    
    public enum ResearchMilestoneType
    {
        Phase_Completion,
        Deliverable_Completion,
        Quality_Gate,
        Decision_Point,
        Review_Milestone,
        Regulatory_Approval,
        Publication_Milestone,
        Patent_Filing
    }
    
    public enum ResearchMethodType
    {
        Experimental_Method,
        Observational_Method,
        Survey_Method,
        Interview_Method,
        Case_Study_Method,
        Statistical_Analysis,
        Computational_Method,
        Field_Study_Method
    }
    
    public enum MethodologyApproach
    {
        Quantitative,
        Qualitative,
        Mixed_Methods,
        Participatory,
        Action_Research,
        Ethnographic,
        Phenomenological,
        Grounded_Theory
    }
    
    public enum DataType
    {
        Quantitative_Data,
        Qualitative_Data,
        Observational_Data,
        Experimental_Data,
        Survey_Data,
        Interview_Data,
        Archival_Data,
        Real_Time_Data
    }
    
    public enum DataSourceType
    {
        Primary_Source,
        Secondary_Source,
        Published_Literature,
        Expert_Opinion,
        Field_Observation,
        Laboratory_Data,
        Market_Data,
        Regulatory_Data
    }
    
    public enum DataCollectionFrequency
    {
        Continuous,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Event_Driven,
        As_Needed
    }
    
    public enum QualityControlType
    {
        Data_Validation,
        Process_Verification,
        Equipment_Calibration,
        Peer_Review,
        Statistical_Check,
        Compliance_Check,
        Documentation_Review,
        Independent_Verification
    }
    
    public enum ControlFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Phase_Based,
        Milestone_Based,
        Continuous,
        As_Needed,
        Random_Sampling
    }
    
    public enum ValidationType
    {
        Cross_Validation,
        Independent_Replication,
        Expert_Review,
        Statistical_Validation,
        Peer_Validation,
        Regulatory_Validation,
        Market_Validation,
        Technical_Validation
    }
    
    public enum ExperimentalDesign
    {
        Randomized_Controlled,
        Factorial_Design,
        Crossover_Design,
        Cohort_Study,
        Case_Control,
        Observational_Study,
        Single_Arm_Study,
        Adaptive_Design
    }
    
    public enum StatisticalMethod
    {
        Descriptive_Statistics,
        Inferential_Statistics,
        Regression_Analysis,
        ANOVA,
        Chi_Square_Test,
        T_Test,
        Non_Parametric_Tests,
        Multivariate_Analysis
    }
    
    public enum TestType
    {
        Hypothesis_Test,
        Significance_Test,
        Equivalence_Test,
        Non_Inferiority_Test,
        Superiority_Test,
        Correlation_Test,
        Regression_Test,
        Power_Analysis
    }
    
    public enum OutcomeType
    {
        Primary_Outcome,
        Secondary_Outcome,
        Safety_Outcome,
        Efficacy_Outcome,
        Quality_Outcome,
        Economic_Outcome,
        Process_Outcome,
        Knowledge_Outcome
    }
    
    public enum ImpactLevel
    {
        Minimal,
        Low,
        Medium,
        High,
        Transformational
    }
    
    public enum DiscoveryType
    {
        Scientific_Discovery,
        Technical_Innovation,
        Process_Improvement,
        Product_Innovation,
        Market_Insight,
        Regulatory_Finding,
        Safety_Discovery,
        Efficiency_Gain
    }
    
    public enum NoveltyLevel
    {
        Incremental,
        Significant,
        Breakthrough,
        Paradigm_Shifting,
        Revolutionary
    }
    
    public enum TechnologyType
    {
        Process_Technology,
        Product_Technology,
        Equipment_Technology,
        Software_Technology,
        Analytical_Technology,
        Automation_Technology,
        Control_Technology,
        Monitoring_Technology
    }
    
    public enum TechnologyReadiness
    {
        Basic_Research,
        Proof_of_Concept,
        Prototype_Development,
        Laboratory_Testing,
        Pilot_Scale,
        Commercial_Scale,
        Market_Ready,
        Fully_Deployed
    }
    
    public enum KnowledgeType
    {
        Scientific_Knowledge,
        Technical_Knowledge,
        Process_Knowledge,
        Market_Knowledge,
        Regulatory_Knowledge,
        Best_Practices,
        Lessons_Learned,
        Expertise_Development
    }
    
    public enum KnowledgeImpact
    {
        Minimal,
        Moderate,
        Significant,
        Major,
        Transformational
    }
    
    public enum RiskType
    {
        Technical_Risk,
        Financial_Risk,
        Timeline_Risk,
        Market_Risk,
        Regulatory_Risk,
        Competitive_Risk,
        Resource_Risk,
        Quality_Risk
    }
    
    public enum RiskImpact
    {
        Negligible,
        Low,
        Medium,
        High,
        Catastrophic
    }
    
    public enum ContingencyType
    {
        Technical_Contingency,
        Financial_Contingency,
        Timeline_Contingency,
        Resource_Contingency,
        Quality_Contingency,
        Market_Contingency,
        Regulatory_Contingency,
        Risk_Mitigation
    }
    
    public enum ActionType
    {
        Corrective_Action,
        Preventive_Action,
        Mitigation_Action,
        Alternative_Approach,
        Resource_Reallocation,
        Timeline_Adjustment,
        Scope_Modification,
        Collaboration_Change
    }
    
    public enum CollaborationType
    {
        Academic,
        Industry,
        Government,
        Non_Profit,
        International,
        Public_Private,
        Consortium,
        Joint_Venture
    }
    
    public enum PartnerType
    {
        University,
        Research_Institute,
        Industry_Partner,
        Government_Agency,
        Consulting_Firm,
        Technology_Provider,
        Funding_Agency,
        Regulatory_Body
    }
    
    public enum PartnerExpertise
    {
        Scientific_Expertise,
        Technical_Expertise,
        Market_Expertise,
        Regulatory_Expertise,
        Manufacturing_Expertise,
        Quality_Expertise,
        Business_Expertise,
        Innovation_Expertise
    }
    
    public enum BenefitType
    {
        Cost_Reduction,
        Time_Reduction,
        Quality_Improvement,
        Risk_Reduction,
        Expertise_Access,
        Resource_Sharing,
        Market_Access,
        Technology_Transfer
    }

    public enum CollaborationBenefitType
    {
        Accelerated_Research,
        Quality_Improvement,
        Risk_Sharing,
        Cost_Reduction,
        Expertise_Access,
        Resource_Sharing,
        Technology_Transfer,
        Market_Access
    }
    
    public enum IPStrategy
    {
        Patent_Protection,
        Trade_Secret,
        Open_Source,
        Mixed_Strategy,
        Defensive_Strategy,
        Offensive_Strategy,
        Collaborative_Strategy,
        No_Protection
    }
    
    public enum IPAssetType
    {
        Patent,
        Trade_Secret,
        Copyright,
        Trademark,
        Know_How,
        Software,
        Database,
        Process
    }
    
    public enum ApplicationSector
    {
        Cultivation,
        Processing,
        Pharmaceutical,
        Nutraceutical,
        Consumer_Products,
        Industrial,
        Research_Tools,
        Regulatory_Tools
    }
    
    public enum PublicationType
    {
        Peer_Reviewed_Journal,
        Conference_Paper,
        Technical_Report,
        White_Paper,
        Patent_Application,
        Regulatory_Submission,
        Industry_Publication,
        Popular_Media
    }
    
    public enum PatentType
    {
        Utility_Patent,
        Process_Patent,
        Composition_Patent,
        Method_Patent,
        Device_Patent,
        Software_Patent,
        Design_Patent,
        Plant_Patent
    }
    
    public enum ResearchExecutionApproach
    {
        Sequential,
        Parallel,
        Hybrid,
        Agile,
        Waterfall,
        Iterative,
        Incremental,
        Adaptive
    }
    
    public enum CollaborationLevel
    {
        Independent,
        Consultative,
        Collaborative,
        High,
        Strategic_Partnership
    }
    
    public enum ConstraintType
    {
        Hard_Deadline,
        Soft_Deadline,
        Resource_Constraint,
        Budget_Constraint,
        Regulatory_Constraint,
        Market_Window,
        Seasonal_Constraint,
        Technology_Constraint
    }

    public enum ResearchStatus
    {
        Proposed,
        Active,
        Paused,
        Completed,
        Failed,
        Cancelled
    }

    public enum ResearchTier
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Cutting_Edge
    }
}