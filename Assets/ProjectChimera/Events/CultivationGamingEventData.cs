using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation; // For cultivation data types
// Resolve ambiguous references with explicit aliases - prefer Data types for cultivation
using CultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;
using CultivationPathData = ProjectChimera.Data.Cultivation.CultivationPathData;
using CultivationTaskType = ProjectChimera.Data.Cultivation.CultivationTaskType;
using FacilityDesignApproach = ProjectChimera.Data.Cultivation.FacilityDesignApproach;
using FacilityDesignData = ProjectChimera.Data.Cultivation.FacilityDesignData;
using CareQuality = ProjectChimera.Data.Cultivation.CareQuality;
using AutomationSystemType = ProjectChimera.Data.Cultivation.AutomationSystemType;
using SkillNodeType = ProjectChimera.Data.Cultivation.SkillNodeType;
// Use specific namespaces to avoid conflicts
using PlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using AutomationUnlockEventData = ProjectChimera.Core.Events.AutomationUnlockEventData;
using SkillNodeEventData = ProjectChimera.Data.Cultivation.SkillNodeEventData;
using CreativityLevel = ProjectChimera.Events.CreativityLevel;
using PendingConsequence = ProjectChimera.Events.PendingConsequence;
using PlayerAgencyLevel = ProjectChimera.Events.PlayerAgencyLevel;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Cultivation Gaming Event Data - Event data structures for Enhanced Cultivation Gaming System
    /// Provides comprehensive event data for all cultivation gaming interactions and outcomes
    /// </summary>
    
    #region Plant Care Event Data
    
    // PlantCareEventData is defined in ProjectChimera.Events.EventDataStructures.cs to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class CareQualityEventData
    {
        public int PlantInstanceID;
        public CareQuality PreviousQuality;
        public CareQuality NewQuality;
        public float QualityImprovement;
        public CultivationTaskType CareType;
        public float SkillLevelAtTime;
        public string ImprovementReason;
    }
    
    [System.Serializable]
    public class PlantResponseEventData
    {
        public int PlantInstanceID;
        public PlantResponseType ResponseType;
        public float ResponseIntensity;
        public Vector3 ResponseLocation;
        public float ResponseDuration;
        public Color ResponseColor;
        public string ResponseDescription;
        public Dictionary<string, object> ResponseParameters;
    }
    
    #endregion
    
    #region Automation Event Data
    
    // AutomationUnlockEventData is defined in ProjectChimera.Data.Cultivation.SkillNodeEventData to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class TaskBurdenEventData
    {
        public CultivationTaskType TaskType;
        public float PreviousBurdenLevel;
        public float NewBurdenLevel;
        public float BurdenIncrease;
        public string BurdenSource;
        public int TaskFrequency;
        public float CognitiveLoad;
        public float TimeInvestment;
        public float ConsistencyChallenge;
    }
    
    [System.Serializable]
    public class AutomationBenefitEventData
    {
        public AutomationSystemType SystemType;
        public CultivationTaskType TaskType;
        public float BenefitValue;
        public AutomationBenefitType BenefitType;
        public float EfficiencyGain;
        public float TimeReduction;
        public float QualityImprovement;
        public float CostReduction;
        public string BenefitDescription;
    }
    
    #endregion
    
    #region Skill Tree Event Data
    
    // SkillNodeEventData is defined in ProjectChimera.Data.Cultivation.SkillNodeEventData to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class TreeVisualizationEventData
    {
        public SkillTreeBranch Branch;
        public TreeVisualizationState VisualizationState;
        public float GrowthPercentage;
        public List<string> VisibleNodes;
        public List<string> UnlockedNodes;
        public Vector3 TreePosition;
        public float TreeScale;
        public Color BranchColor;
        public Dictionary<string, object> VisualizationParameters;
    }
    
    [System.Serializable]
    public class ConceptIntroductionEventData
    {
        public string ConceptID;
        public string ConceptName;
        public string ConceptDescription;
        public SkillTreeBranch RelatedBranch;
        public ConceptComplexity Complexity;
        public float IntroductionTimestamp;
        public List<string> PrerequisiteConcepts;
        public List<string> RelatedConcepts;
        public Dictionary<string, object> ConceptParameters;
    }
    
    [System.Serializable]
    public class EquipmentUnlockEventData
    {
        public string EquipmentID;
        public string EquipmentName;
        public EquipmentCategory Category;
        public SkillNodeType UnlockingNode;
        public float EquipmentEfficiency;
        public float EquipmentCost;
        public List<string> EquipmentFeatures;
        public float UnlockTimestamp;
        public Dictionary<string, object> EquipmentParameters;
    }
    
    #endregion
    
    #region Time Acceleration Event Data
    
    // TimeScaleEventData is defined in ProjectChimera.Events.EventDataStructures.cs to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class TimeAccelerationLockEventData
    {
        public GameTimeScale LockedTimeScale;
        public float LockDuration;
        public float RemainingLockTime;
        public string LockReason;
        public bool CanOverride;
        public float OverrideCost;
        public float LockTimestamp;
        public Dictionary<string, object> LockParameters;
    }
    
    [System.Serializable]
    public class TimeTransitionEventData
    {
        public GameTimeScale FromScale;
        public GameTimeScale ToScale;
        public float TransitionDuration;
        public float TransitionProgress;
        public TransitionType TransitionType;
        public bool TransitionSuccess;
        public string TransitionResult;
        public float CompletionTimestamp;
    }
    
    #endregion
    
    #region Player Agency Event Data
    
    // PlayerChoiceEventData is defined in ProjectChimera.Events.EventDataStructures.cs
    // to avoid duplicate definitions and maintain consistency across the project
    
    [System.Serializable]
    public class CultivationPathGamingEventData
    {
        public CultivationApproach NewApproach;
        public CultivationApproach PreviousApproach;
        public CultivationPathData PathData;
        public float ExpressionValue;
        public float ApproachEfficiency;
        public float SwitchingCost;
        public List<string> PathBenefits;
        public float SelectionTimestamp;
        public Dictionary<string, object> PathParameters;
    }
    
    [System.Serializable]
    public class FacilityDesignGamingEventData
    {
        public FacilityDesignApproach NewApproach;
        public FacilityDesignApproach PreviousApproach;
        public FacilityDesignData DesignData;
        public float ExpressionValue;
        public float DesignComplexity;
        public float CreativityValue;
        public List<string> DesignFeatures;
        public float CompletionTimestamp;
        public Dictionary<string, object> DesignParameters;
    }
    
    [System.Serializable]
    public class CreativeSolutionEventData
    {
        public CreativeSolution Solution;
        public CreativeSolutionResult Result;
        public float CreativityBonus;
        public CreativityLevel NewCreativityLevel;
        public float InnovationValue;
        public float ComplexityValue;
        public List<string> SolutionBenefits;
        public float ImplementationTimestamp;
        public Dictionary<string, object> SolutionParameters;
    }
    
    [System.Serializable]
    public class ConsequenceEventData
    {
        public PendingConsequence Consequence;
        public float ProcessTime;
        public float ConsequenceImpact;
        public ConsequenceResult Result;
        public List<string> AffectedSystems;
        public float Duration;
        public string ConsequenceDescription;
        public Dictionary<string, object> ConsequenceParameters;
    }
    
    [System.Serializable]
    public class ExpressionLevelEventData
    {
        public CreativityLevel PreviousLevel;
        public CreativityLevel NewLevel;
        public float ExpressionScore;
        public float LevelProgress;
        public float ExpressionBonus;
        public List<string> UnlockedFeatures;
        public float LevelUpTimestamp;
        public Dictionary<string, object> ExpressionParameters;
    }
    
    #endregion
    
    #region Gaming Performance Event Data
    
    [System.Serializable]
    public class GamingSessionEventData
    {
        public string SessionID;
        public float SessionDuration;
        public PlayerAgencyLevel StartingAgencyLevel;
        public PlayerAgencyLevel EndingAgencyLevel;
        public int CareActionsPerformed;
        public int AutomationSystemsUnlocked;
        public int SkillNodesProgressed;
        public int CreativeSolutionsImplemented;
        public float TotalExpressionGained;
        public float AverageEngagementLevel;
        public Dictionary<string, object> SessionMetrics;
    }
    
    [System.Serializable]
    public class GamingMilestoneEventData
    {
        public string MilestoneID;
        public string MilestoneName;
        public string MilestoneDescription;
        public GamingMilestoneType MilestoneType;
        public float MilestoneValue;
        public List<string> MilestoneRewards;
        public float AchievementTimestamp;
        public Dictionary<string, object> MilestoneParameters;
    }
    
    [System.Serializable]
    public class EngagementEventData
    {
        public EngagementLevel PreviousLevel;
        public EngagementLevel NewLevel;
        public float EngagementScore;
        public float EngagementChange;
        public List<string> EngagementFactors;
        public float MeasurementTimestamp;
        public Dictionary<string, object> EngagementMetrics;
    }
    
    #endregion
    
    #region Integration Event Data
    
    [System.Serializable]
    public class SkillProgressionEventData
    {
        public CultivationTaskType TaskType;
        public SkillTreeBranch RelatedBranch;
        public float SkillGain;
        public float NewSkillLevel;
        public float ProgressionMultiplier;
        public string ProgressionTrigger;
        public List<string> UnlockedAbilities;
        public float ProgressionTimestamp;
        public Dictionary<string, object> ProgressionParameters;
        
        // Additional properties for InteractivePlantCareSystem compatibility
        public SkillMilestone Milestone;
        public float CurrentSkillLevel;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class InterdependencyBonusEventData
    {
        public List<SkillTreeBranch> InteractingBranches;
        public float BonusMultiplier;
        public InterdependencyType BonusType;
        public float BonusValue;
        public string BonusDescription;
        public List<string> AffectedSystems;
        public float BonusDuration;
        public float BonusTimestamp;
        public Dictionary<string, object> BonusParameters;
    }
    
    #endregion
    
    #region Supporting Data Structures
    
    // CultivationPathData is defined in ProjectChimera.Data.Cultivation.CultivationPathData.cs to avoid CS0101 duplicate definition errors
    // FacilityDesignData is defined in ProjectChimera.Events.EventDataStructures.cs to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class TreeVisualizationState
    {
        public float OverallGrowthPercentage;
        public Dictionary<SkillTreeBranch, float> BranchGrowthPercentages;
        public List<string> VisibleNodes;
        public List<string> UnlockedNodes;
        public List<string> AvailableNodes;
        public Vector3 TreeCenterPosition;
        public float TreeScale;
        public Dictionary<string, object> VisualizationData;
    }
    
    #endregion
    
    #region Enums for Event Data
    
    public enum PlantResponseType
    {
        PositiveGrowth,
        NegativeStress,
        NeutralMaintenance,
        HealthImprovement,
        HealthDegradation,
        VisualChange,
        SizeIncrease,
        ColorChange
    }
    
    public enum AutomationBenefitType
    {
        EfficiencyGain,
        TimeReduction,
        QualityImprovement,
        CostReduction,
        ConsistencyImprovement,
        ScalabilityIncrease,
        PrecisionEnhancement
    }
    
    public enum ConceptComplexity
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum EquipmentCategory
    {
        CareTools,
        MonitoringDevices,
        EnvironmentalControl,
        AutomationSystems,
        AnalysisEquipment,
        ProcessingTools,
        SafetyEquipment
    }
    
    public enum TransitionType
    {
        Instant,
        Smooth,
        Stepped,
        Gradual,
        Delayed
    }
    
    public enum ConsequenceResult
    {
        Positive,
        Negative,
        Neutral,
        Mixed,
        Unexpected
    }
    
    public enum GamingMilestoneType
    {
        CareActionCount,
        SkillProgression,
        AutomationUnlock,
        CreativeSolution,
        ExpressionLevel,
        SessionDuration,
        QualityAchievement,
        EfficiencyGain
    }
    
    public enum EngagementLevel
    {
        Low,
        Moderate,
        High,
        VeryHigh,
        Exceptional
    }
    
    public enum InterdependencyType
    {
        SkillSynergy,
        SystemIntegration,
        KnowledgeTransfer,
        EfficiencyBonus,
        QualityEnhancement,
        InnovationBoost,
        MasteryAcceleration
    }
    
    public enum SkillTreeBranch
    {
        Genetics,
        Cultivation,
        Environment,
        Construction,
        Harvest,
        Science,
        Business
    }
    
    #endregion
}