using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Data.Research
{
    /// <summary>
    /// Comprehensive data structures for the Research System
    /// Centralized definitions for all research-related types and enums
    /// </summary>

    #region Core Research Data

    [System.Serializable]
    public class ResearchRequirements
    {
        public List<string> RequiredTechnologies;
        public List<string> RequiredProjects;
        public Dictionary<ResearchCategory, float> ExperienceRequirements;
        public ResourceCost Cost;
        public List<string> Prerequisites;
        public float MinimumExpertise;
        public List<string> RequiredFacilities;
        public List<string> RequiredEquipment;
    }

    [System.Serializable]
    public class ResearchProject
    {
        public string ProjectId;
        public string ProjectName;
        public string Description;
        public ResearchCategory Category;
        public ResearchComplexity Complexity;
        public ResearchPriority Priority;
        public List<ResearchPhase> Phases;
        public List<ResearchMilestone> Milestones;
        public ResearchRequirements Requirements;
        public List<TechnologyUnlock> PotentialUnlocks;
        public float EstimatedDuration;
        public ResourceCost Cost;
        public float SuccessProbability;
        public Dictionary<string, object> Metadata;
    }

    [System.Serializable]
    public class ActiveResearchProject
    {
        public ResearchProject ResearchProject;
        public ResearchStatus Status;
        public DateTime StartDate;
        public DateTime EstimatedCompletionDate;
        public int CurrentPhaseIndex;
        public float OverallProgress;
        public List<CompletedPhase> CompletedPhases;
        public List<CompletedMilestone> CompletedMilestones;
        public float TotalInvestment;
        public float CurrentQuality;
        public float TeamExpertise;
        public bool HadSetbacks;
        public List<ResearchEvent> ProjectHistory;
    }

    [System.Serializable]
    public class ResearchProjectOffer
    {
        public ResearchProject ResearchProject;
        public float OfferValidUntil;
        public float Priority;
        public ResearchFeasibility Feasibility;
        public List<string> RecommendationReasons;
        public bool IsRecommended;
    }

    [System.Serializable]
    public class ResearchPhase
    {
        public string PhaseId;
        public string PhaseName;
        public string Description;
        public float Duration;
        public ResourceRequirements Requirements;
        public List<string> Dependencies;
        public float CompletionCriteria;
        public List<RiskFactor> Risks;
    }

    [System.Serializable]
    public class ResearchMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public float Progress;
        public bool IsCompleted;
        public DateTime CompletionDate;
        public List<string> Requirements;
        public ResearchReward Reward;
    }

    [System.Serializable]
    public class CompletedPhase
    {
        public string PhaseId;
        public DateTime CompletionDate;
        public float ActualDuration;
        public float QualityScore;
        public List<string> AchievedObjectives;
    }

    [System.Serializable]
    public class CompletedMilestone
    {
        public string MilestoneId;
        public DateTime CompletionDate;
        public float QualityScore;
        public ResearchReward RewardGranted;
    }

    #endregion

    #region Technology System

    [System.Serializable]
    public class Technology
    {
        public string TechnologyId;
        public string Name;
        public string Description;
        public TechnologyType Type;
        public TechnologyTier Tier;
        public List<string> Dependencies;
        public UnlockRequirements Requirements;
        public List<TechnologyBenefit> Benefits;
        public bool IsUnlocked;
        public DateTime UnlockDate;
    }

    [System.Serializable]
    public class TechnologyTree
    {
        public string TreeId;
        public string Name;
        public TechnologyType Type;
        public List<TechnologyNode> Nodes;
        public Dictionary<string, List<string>> Dependencies;
        public float CompletionPercentage;
    }

    [System.Serializable]
    public class TechnologyNode
    {
        public string NodeId;
        public Technology Technology;
        public Vector2 Position;
        public List<string> ConnectedNodes;
        public NodeStatus Status;
    }

    [System.Serializable]
    public class UnlockedTechnology
    {
        public TechnologyUnlock TechnologyUnlock;
        public DateTime UnlockDate;
        public string UnlockSource;
        public float ImpactScore;
        public bool IsActive;
    }

    [System.Serializable]
    public class TechnologyUnlock
    {
        public string TechnologyId;
        public string TechnologyName;
        public TechnologyType Type;
        public List<GameplayEffect> Effects;
        public float ImpactRating;
        public string UnlockDescription;
    }

    [System.Serializable]
    public class TechnologyPreview
    {
        public string TechnologyName;
        public TechnologyType Type;
        public string Description;
        public float UnlockProbability;
        public List<string> RequiredResearch;
    }

    [System.Serializable]
    public class TechnologyPathAnalysis
    {
        public string TargetTechnologyId;
        public List<string> OptimalPath;
        public float EstimatedTime;
        public ResourceCost TotalCost;
        public float SuccessProbability;
        public List<string> Alternatives;
    }

    [System.Serializable]
    public class TechnologyBenefit
    {
        public string BenefitId;
        public string Description;
        public BenefitType Type;
        public float Value;
        public string TargetSystem;
    }

    #endregion

    #region Discovery System

    [System.Serializable]
    public class Discovery
    {
        public string DiscoveryId;
        public string Name;
        public string Description;
        public DiscoveryType Type;
        public DiscoveryRarity Rarity;
        public DateTime DiscoveryDate;
        public string DiscoverySource;
        public DiscoveryImpact Impact;
        public List<TechnologyUnlock> UnlockedTechnologies;
    }

    [System.Serializable]
    public class DiscoveryEvent
    {
        public string EventId;
        public DiscoveryTrigger Trigger;
        public DiscoveryContext Context;
        public float Probability;
        public List<DiscoveryOutcome> PossibleOutcomes;
    }

    [System.Serializable]
    public class Innovation
    {
        public string InnovationId;
        public string Name;
        public string Description;
        public InnovationType Type;
        public InnovationTrigger Trigger;
        public float ImpactScore;
        public List<GameplayEffect> Effects;
        public DateTime CreationDate;
    }

    [System.Serializable]
    public class Breakthrough
    {
        public string BreakthroughId;
        public string Name;
        public string Description;
        public BreakthroughType Type;
        public BreakthroughConditions Conditions;
        public BreakthroughImpact Impact;
        public DateTime AchievementDate;
        public bool IsRepeatable;
    }

    [System.Serializable]
    public class ResearchBreakthrough
    {
        public string BreakthroughId;
        public string Name;
        public string Description;
        public BreakthroughType Type;
        public float ImpactScore;
        public List<TechnologyUnlock> TechnologiesUnlocked;
        public DateTime DiscoveryDate;
        public ResearchProject SourceProject;
    }

    [System.Serializable]
    public class PotentialDiscovery
    {
        public string DiscoveryId;
        public string Name;
        public DiscoveryType Type;
        public float DiscoveryProbability;
        public DiscoveryConditions Conditions;
        public List<DiscoveryOutcome> PotentialOutcomes;
    }

    [System.Serializable]
    public class DiscoveryContext
    {
        public string ContextId;
        public ResearchCategory Category;
        public List<string> ActiveProjects;
        public Dictionary<string, float> EnvironmentalFactors;
        public float PlayerExperience;
    }

    [System.Serializable]
    public class DiscoveryConditions
    {
        public List<string> RequiredTechnologies;
        public List<string> RequiredProjects;
        public float MinimumExperience;
        public List<EnvironmentalCondition> EnvironmentalRequirements;
    }

    [System.Serializable]
    public class DiscoveryOutcome
    {
        public string OutcomeId;
        public DiscoveryImpact Impact;
        public List<TechnologyUnlock> TechnologiesUnlocked;
        public List<ResearchProject> ProjectsUnlocked;
        public ResourceReward Rewards;
    }

    [System.Serializable]
    public class DiscoveryImpact
    {
        public float ScienceImpact;
        public float GameplayImpact;
        public float EconomicImpact;
        public List<string> AffectedSystems;
    }

    #endregion

    #region Resource Management

    [System.Serializable]
    public class ResourceAllocation
    {
        public string AllocationId;
        public string ProjectId;
        public Dictionary<ResourceType, float> AllocatedResources;
        public DateTime AllocationDate;
        public float AllocationPriority;
        public bool IsActive;
    }

    [System.Serializable]
    public class ResourceBudget
    {
        public string BudgetId;
        public string ProjectId;
        public Dictionary<ResourceType, float> BudgetLimits;
        public Dictionary<ResourceType, float> SpentResources;
        public float TotalBudget;
        public float RemainingBudget;
    }

    [System.Serializable]
    public class ResearchFacility
    {
        public string FacilityId;
        public string Name;
        public FacilityType Type;
        public FacilityLevel Level;
        public FacilityStatus Status;
        public List<string> Capabilities;
        public Dictionary<ResourceType, float> ResourceCapacity;
        public List<string> AssignedProjects;
        public float EfficiencyRating;
    }

    [System.Serializable]
    public class FacilityUpgrade
    {
        public string UpgradeId;
        public string Name;
        public string Description;
        public FacilityLevel RequiredLevel;
        public ResourceCost Cost;
        public List<FacilityImprovement> Improvements;
        public float UpgradeTime;
    }

    [System.Serializable]
    public class ResearchEquipment
    {
        public string EquipmentId;
        public string Name;
        public EquipmentType Type;
        public EquipmentStatus Status;
        public List<string> Capabilities;
        public float EfficiencyBonus;
        public float MaintenanceRequirement;
        public string AssignedProject;
        public DateTime LastMaintenance;
    }

    [System.Serializable]
    public class ResourceRequirements
    {
        public Dictionary<ResourceType, float> RequiredResources;
        public List<string> RequiredFacilities;
        public List<string> RequiredEquipment;
        public float MinimumExpertise;
        public TimeSpan EstimatedDuration;
    }

    [System.Serializable]
    public class ResourceCost
    {
        public float MonetaryCost;
        public float TimeCost;
        public Dictionary<ResourceType, float> MaterialCosts;
        public float ExpertiseRequired;
    }

    [System.Serializable]
    public class ResourceReward
    {
        public float MonetaryReward;
        public Dictionary<ResourceType, float> MaterialRewards;
        public float ExperienceGained;
        public List<string> UnlockedCapabilities;
    }

    #endregion

    #region Support Data

    [System.Serializable]
    public class ResearchEvent
    {
        public string EventId;
        public ResearchEventType EventType;
        public DateTime Timestamp;
        public string Description;
        public ResearchProject Project;
        public Dictionary<string, object> EventData;
    }

    [System.Serializable]
    public class ResearchFeasibility
    {
        public float OverallFeasibility;
        public float TechnicalFeasibility;
        public float ResourceFeasibility;
        public float ExpertiseFeasibility;
        public List<string> FeasibilityFactors;
        public List<string> Risks;
    }

    [System.Serializable]
    public class ResearchReward
    {
        public string RewardId;
        public RewardType Type;
        public float Value;
        public string Description;
        public List<GameplayEffect> Effects;
    }

    [System.Serializable]
    public class RiskFactor
    {
        public string RiskId;
        public string Description;
        public RiskSeverity Severity;
        public float Probability;
        public List<RiskMitigation> Mitigations;
    }

    [System.Serializable]
    public class RiskMitigation
    {
        public string MitigationId;
        public string Description;
        public float EffectivenessRating;
        public ResourceCost Cost;
    }

    [System.Serializable]
    public class GameplayEffect
    {
        public string EffectId;
        public string TargetSystem;
        public EffectType Type;
        public float Magnitude;
        public float Duration;
        public bool IsPermanent;
    }

    [System.Serializable]
    public class UnlockRequirements
    {
        public List<string> RequiredTechnologies;
        public List<string> RequiredProjects;
        public Dictionary<ResearchCategory, float> ExperienceRequirements;
        public ResourceCost Cost;
        public List<string> Prerequisites;
    }

    [System.Serializable]
    public class EnvironmentalCondition
    {
        public string ConditionId;
        public string Parameter;
        public float MinValue;
        public float MaxValue;
        public bool IsCritical;
    }

    [System.Serializable]
    public class FacilityImprovement
    {
        public string ImprovementId;
        public string Description;
        public ImprovementType Type;
        public float ImprovementValue;
        public string TargetCapability;
    }

    #endregion

    #region Enums

    public enum ResearchCategory
    {
        Genetics,
        Cultivation,
        Processing,
        Equipment,
        Sustainability,
        Analytics,
        Automation,
        Innovation
    }

    public enum ResearchComplexity
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Breakthrough
    }

    public enum ResearchPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Urgent
    }

    public enum ResearchStatus
    {
        Pending,
        Active,
        Paused,
        Completed,
        Failed,
        Cancelled
    }

    public enum TechnologyType
    {
        Genetic,
        Cultivation,
        Processing,
        Equipment,
        Analytics,
        Automation,
        Sustainability,
        Innovation
    }

    public enum TechnologyTier
    {
        Basic,
        Advanced,
        Expert,
        Master,
        Legendary
    }

    public enum NodeStatus
    {
        Locked,
        Available,
        InProgress,
        Unlocked
    }

    public enum DiscoveryType
    {
        Genetic,
        Cultivation,
        Processing,
        Equipment,
        Method,
        Innovation
    }

    public enum DiscoveryRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum InnovationType
    {
        Incremental,
        Breakthrough,
        Disruptive,
        Revolutionary
    }

    public enum BreakthroughType
    {
        Scientific,
        Technical,
        Process,
        Method
    }

    public enum ResourceType
    {
        Currency,
        Time,
        Expertise,
        Equipment,
        Materials,
        Energy,
        Space
    }

    public enum FacilityType
    {
        Laboratory,
        Greenhouse,
        ProcessingCenter,
        AnalyticsCenter,
        Library
    }

    public enum FacilityLevel
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Master
    }

    public enum FacilityStatus
    {
        Available,
        Occupied,
        Maintenance,
        Upgrading,
        Offline
    }

    public enum EquipmentType
    {
        Analytical,
        Cultivation,
        Processing,
        Monitoring,
        Computing
    }

    public enum EquipmentStatus
    {
        Available,
        InUse,
        Maintenance,
        Broken,
        Upgrading
    }

    public enum ResearchEventType
    {
        Project_Started,
        Project_Completed,
        Project_Failed,
        Phase_Completed,
        Milestone_Achieved,
        Technology_Unlocked,
        Discovery_Made,
        Breakthrough_Achieved,
        Collaboration_Started,
        Resource_Allocated
    }

    public enum RewardType
    {
        Experience,
        Technology,
        Resource,
        Capability,
        Access
    }

    public enum RiskSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum EffectType
    {
        Bonus,
        Penalty,
        Unlock,
        Modification,
        Grant
    }

    public enum BenefitType
    {
        Efficiency,
        Quality,
        Speed,
        Cost,
        Unlock
    }

    public enum ImprovementType
    {
        Efficiency,
        Capacity,
        Quality,
        Speed,
        Cost
    }

    public enum DiscoveryTrigger
    {
        ResearchCompletion,
        ExperimentalResult,
        Observation,
        Collaboration,
        Accident,
        Inspiration
    }

    public enum InnovationTrigger
    {
        Research,
        Problem,
        Opportunity,
        Accident,
        Collaboration,
        Inspiration
    }

    public enum BreakthroughConditions
    {
        MultipleProjects,
        HighExperience,
        RareConditions,
        PerfectTiming,
        RandomChance
    }

    public enum BreakthroughImpact
    {
        Minor,
        Moderate,
        Major,
        Revolutionary
    }

    #endregion
}