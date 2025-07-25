using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Construction; // For SkillLevel enum
using ProjectChimera.Data.Cultivation; // For various cultivation enums and classes
using ProjectChimera.Data.Genetics; // For PlantGrowthStage enum
using ProjectChimera.Core; // For core types
using ProjectChimera.Data.Narrative; // For CharacterEventType

// Type alias to explicitly use the Data.Cultivation version for event compatibility
using DataInteractivePlant = ProjectChimera.Data.Cultivation.InteractivePlant;
using SkillNodeEventData = ProjectChimera.Data.Cultivation.SkillNodeEventData;
using AutomationUnlockEventData = ProjectChimera.Core.Events.AutomationUnlockEventData;
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;

namespace ProjectChimera.Events
{

/// <summary>
/// Event data for player choice events in cultivation gaming system
/// </summary>
[System.Serializable]
public class PlayerChoiceEventData
{
    public string PlayerId;
    public PlayerChoiceType ChoiceType;
    public string ChoiceDescription;
    public float ImpactLevel;
    public Dictionary<string, object> ChoiceParameters;
    public float ChoiceTimestamp;
    public ChoiceConsequences Consequences;
    
    // Additional properties for enhanced gaming system integration
    public string ChoiceId;
    public string ChoiceCategory;
    public string SelectedOption;
    public Dictionary<string, object> ChoiceContext = new Dictionary<string, object>();
    public List<string> ConsequenceIds = new List<string>();
    public float Timestamp;
    public float ImpactScore;
    public string CultivationPathId;
    
    // Additional properties used by PlayerAgencyGamingSystem
    public PlayerChoice Choice;
    public ChoiceConsequences ImmediateConsequences;
    public List<PendingConsequence> DelayedConsequences;
    public PlayerAgencyLevel PlayerAgencyLevel;
}

/// <summary>
/// Event data for cultivation path selection events
/// </summary>
[System.Serializable]
public class CultivationPathEventData
{
    public string PlayerId;
    public CultivationApproach SelectedApproach;
    public CultivationApproach PreviousApproach;
    public CultivationPathData PathData;
    public float SelectionTimestamp;
    public CultivationPathEffects Effects;
}

/// <summary>
/// Event data for facility design choice events
/// </summary>
[System.Serializable]
public class FacilityDesignEventData
{
    public string PlayerId;
    public FacilityDesignApproach SelectedDesign;
    public FacilityDesignApproach PreviousDesign;
    public FacilityDesignData DesignData;
    public float SelectionTimestamp;
    public FacilityDesignEffects Effects;
}


/// <summary>
/// Event data for skill node progression events
/// </summary>

/// <summary>
/// Event data for time scale changes
/// </summary>
[System.Serializable]
public class TimeScaleEventData
{
    public GameTimeScale PreviousTimeScale;
    public GameTimeScale NewTimeScale;
    public float ScaleMultiplier;
    public float TransitionDuration;
    public string ScaleChangeReason;
    public float LockInPeriod;
    public bool IsPlayerInitiated;
    public float ChangeTimestamp;
    public Dictionary<string, object> ScaleParameters;
    
    // Additional properties referenced in gaming systems
    public float RealTimeDayDuration;
    public float GameDaysPerRealHour;
    public string Description;
    public float PlayerEngagementOptimal;
    public float RealTimeDayDuration2;
    public float GameDayPerRealHour;
}

// Enums for event data types
public enum PlayerChoiceType
{
    CultivationMethod,
    FacilityDesign,
    ResourceAllocation,
    TechnologyAdoption,
    QualityStandard,
    MarketStrategy,
    EnvironmentalApproach
}

[System.Flags]
public enum ChoiceConsequences
{
    None = 0,
    YieldIncrease = 1 << 0,
    YieldDecrease = 1 << 1,
    EfficiencyGain = 1 << 2,
    EfficiencyLoss = 1 << 3,
    CostReduction = 1 << 4,
    CostIncrease = 1 << 5,
    QualityImprovement = 1 << 6,
    QualityDegradation = 1 << 7,
    UnlockNewOption = 1 << 8,
    LockExistingOption = 1 << 9
}

public enum CultivationApproach
{
    OrganicTraditional,
    HydroponicPrecision,
    AeroponicCutting,
    BiodynamicHolistic,
    TechnologicalAutomated,
    ExperimentalInnovative,
    EconomicOptimized
}

public enum FacilityDesignApproach
{
    MinimalistEfficient,
    CreativeInnovative,
    ModularExpandable,
    AestheticShowcase,
    BudgetOptimized,
    TechnologicalCutting,
    SustainableEcological
}

public enum CareAction
{
    Watering,
    Feeding,
    Pruning,
    Training,
    Monitoring,
    Harvesting
}

public enum CareQuality
{
    Poor,
    Adequate,
    Good,
    Excellent,
    Perfect
}

public enum CultivationTaskType
{
    None,
    Watering,
    Feeding,
    Fertilizing,
    Pruning,
    Training,
    Monitoring,
    Harvesting,
    Transplanting,
    Cloning,
    PestControl,
    Defoliation,
    Cleaning,
    Maintenance,
    Research,
    EnvironmentalControl,
    DataCollection
}

public enum AutomationSystemType
{
    IrrigationSystem,
    NutrientDelivery,
    ClimateControl,
    LightingControl,
    LightingSchedule,
    MonitoringSensors,
    HarvestAssist,
    SensorNetwork,
    DataCollection,
    VentilationControl,
    IPM
}

public enum AutomationLevel
{
    FullyManual,
    BasicAutomation,
    IntermediateAutomation,
    AdvancedAutomation,
    FullyAutomated
}

public enum SkillNodeType
{
    WateringMastery,
    NutrientExpert,
    PruningSpecialist,
    TrainingTechnician,
    MonitoringPro,
    HarvestMaster
}

public enum PlantCareResult
{
    Perfect,
    Successful,
    Adequate,
    Suboptimal,
    Failed
}

public enum AutomationUnlockResult
{
    Success,
    InsufficientSkill,
    InsufficientResources,
    PrerequisiteNotMet,
    Failed
}

public enum SkillNodeUnlockResult
{
    Success,
    InsufficientPoints,
    PrerequisiteNotMet,
    AlreadyUnlocked,
    Failed
}

// InteractivePlant is a class defined in ProjectChimera.Data.Cultivation namespace

// Data structure classes
[System.Serializable]
public class CultivationPathData
{
    public string PathName;
    public string Description;
    public CultivationApproach Approach;
    public float YieldModifier;
    public float EfficiencyModifier;
    public float CostModifier;
    public float QualityModifier;
    public List<string> RequiredSkills;
    public List<string> UnlockedFeatures;
}

[System.Serializable]
public class FacilityDesignData
{
    public string DesignName;
    public string Description;
    public FacilityDesignApproach Approach;
    public float SpaceEfficiency;
    public float EnergyEfficiency;
    public float ConstructionCost;
    public float MaintenanceCost;
    public List<string> RequiredComponents;
    public List<string> OptionalUpgrades;
}

[System.Serializable]
public class CultivationPathEffects
{
    public float YieldChange;
    public float EfficiencyChange;
    public float CostChange;
    public float QualityChange;
    public List<string> NewUnlocks;
    public List<string> RemovedOptions;
}

[System.Serializable]
public class FacilityDesignEffects
{
    public float SpaceEfficiencyChange;
    public float EnergyEfficiencyChange;
    public float ConstructionCostChange;
    public float MaintenanceCostChange;
    public List<string> NewComponents;
    public List<string> RemovedComponents;
}

// Additional missing types referenced in event data structures
[System.Serializable]
public class PlayerChoice
{
    public string ChoiceId;
    public string Description;
    public PlayerChoiceType Type;
    public Dictionary<string, object> Parameters = new Dictionary<string, object>();
}

[System.Serializable]
public class PendingConsequence
{
    public string ConsequenceId;
    public string Description;
    public float DelayTime;
    public float TriggerTime;
    public ConsequenceType Type;
    public ConsequenceSeverity Severity;
    public float ImpactValue;
}

public enum PlayerAgencyLevel
{
    Low,
    Medium,
    High,
    Maximum
}

public enum CreativityLevel
{
    Low,
    Medium,
    High,
    Expert
}

public enum CreativeLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Master
}

[System.Serializable]
public class CharacterRelationshipTracker
{
    public string CharacterId;
    public float RelationshipLevel;
    public float TrustLevel;
    public float RespectLevel;
    public float InfluenceLevel;
    public float LastUpdateTime;
    public List<RelationshipChange> RelationshipHistory = new List<RelationshipChange>();
    public List<EmotionalStateRecord> EmotionalHistory = new List<EmotionalStateRecord>();
    public Dictionary<string, object> TrackingData = new Dictionary<string, object>();
    public float CurrentEmotionalState;
    public float EmotionalIntensity;
    public float LastEmotionalUpdate;
}


public enum ConsequenceType
{
    Immediate,
    Delayed,
    Educational,
    Environmental,
    Economic,
    Social,
    Narrative
}

public enum ConsequenceSeverity
{
    Minor,
    Moderate,
    Major,
    Critical
}

[System.Serializable]
public class CreativeSolution
{
    public string SolutionId;
    public string Description;
    public CreativityLevel RequiredLevel;
    public float ComplexityValue;
    public float InnovationValue;
    public List<string> Benefits = new List<string>();
    public Dictionary<string, object> Parameters = new Dictionary<string, object>();
}

public enum CreativeSolutionResult
{
    Success,
    PartialSuccess,
    Failure,
    UnexpectedOutcome,
    RequiresRevision
}

public enum GameTimeScale
{
    Paused,
    RealTime,
    Baseline,
    Standard,
    Fast,
    VeryFast,
    Accelerated,
    Lightning,
    Maximum,
    SlowMotion
}

public enum AutomationDesire
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum AutomationDesireThreshold
{
    Low,
    Medium,
    High,
    Critical
}

public enum SkillBranch
{
    BasicCultivation,
    AdvancedCultivation,
    Environment,
    PostHarvest,
    TreeGrowthStage,
    TreeHealthStage,
    EfficiencyGain,
    QualityImprovement,
    TreeGrowthStageBranch,
    TreeHealthStageBranch
}

public enum TreeGrowthStage
{
    Seedling,
    Vegetative,
    Flowering,
    Harvest,
    PostHarvest
}

public enum TreeHealthStage
{
    Healthy,
    Stressed,
    Recovering,
    Optimal,
    Critical
}

[System.Serializable]
public class EducationalInteractionTracker
{
    public string TrackerId;
    public string CharacterId;
    public int TotalInteractions;
    public float CumulativeEffectiveness;
    public float AverageEffectiveness;
    public List<EducationalInteraction> Interactions = new List<EducationalInteraction>();
    public List<string> InteractionHistory = new List<string>();
    public bool IsEducationalMentor;
    public List<CultivationExpertise> ExpertiseAreas = new List<CultivationExpertise>();
    public float CredibilityLevel;
    public float LastInteractionTime;
}

[System.Serializable]
public class TeachingEffectivenessData
{
    public string CharacterId;
    public string Topic;
    public float EffectivenessScore;
    public int TotalSessions;
    public float CumulativeEffectiveness;
    public float AverageEffectiveness;
    public int ScientificallyAccurateCount;
    public float AccuracyRate;
}

[System.Serializable]
public class CharacterEventStatistics
{
    public int TotalEventsProcessed;
    public int TrackedCharactersCount;
    public int EducationalMentorsCount;
    public Dictionary<CharacterEventType, int> EventTypeDistribution = new Dictionary<CharacterEventType, int>();
    public float AverageRelationshipLevel;
    public float AverageTeachingEffectiveness;
}

[System.Serializable]
public class RelationshipChange
{
    public string CharacterId;
    public float TrustChange;
    public float RespectChange;
    public float InfluenceChange;
    public float ChangeTime; // Changed from DateTime to float for consistency
    public string ChangeReason;
    public string EventType;
    public float Timestamp;
    public string Context;
}

[System.Serializable]
public class EmotionalStateRecord
{
    public string CharacterId;
    public float EmotionalState;
    public float Intensity;
    public float Timestamp;
    public string State;
    public string Trigger;
}

[System.Serializable]
public class EducationalInteraction
{
    public string InteractionId;
    public string CharacterId;
    public string Topic;
    public float Effectiveness;
    public float Timestamp;
    public CharacterEventType InteractionType;
    public float EffectivenessRating;
    public bool IsScientificallyAccurate;
    public string LearningOutcome;
}


// Note: CharacterEventData and CharacterEventType are defined in ProjectChimera.Data.Narrative namespace

public enum CultivationExpertise
{
    Genetics,
    Environment,
    Nutrition,
    IPM,
    Harvesting,
    Processing,
    Quality,
    Equipment,
    Automation,
    Research
}

public enum RelationshipAlertType
{
    SignificantTrustChange,
    SignificantRespectChange,
    SignificantInfluenceChange,
    EmotionalStateChange,
    CredibilityChange
}

[System.Serializable]
public class CharacterRelationshipAlert
{
    public string CharacterId;
    public RelationshipAlertType AlertType;
    public float ChangeAmount;
    public float CurrentLevel;
    public float Timestamp;
}

// Event-compatible type definitions for PlantCareEventData
// These are lightweight versions for event communication

[System.Serializable]
public class CareEffects
{
    public float HydrationChange;
    public float NutritionImprovement;
    public float HealthImprovement;
    public float StressReduction;
    public float StressIncrease;
    public float GrowthRateBoost;
    public float GrowthRedirection;
    public float StructuralImprovement;
    public float YieldPotentialIncrease;
    public float GrowthPotentialIncrease;
    public float RootSpaceIncrease;
    public float TransplantShock;
}

[System.Serializable]
public class CareActionData
{
    public CultivationTaskType TaskType;
    public string ToolName; // Simplified for Events namespace
    public float Timestamp;
    public float Duration;
    public float MaxDuration = 5.0f;
    public float PlayerSkillLevel;
}

} // End namespace ProjectChimera.Events 