using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.IPM
{
    #region Core IPM Enums


    public enum PestType
    {
        Aphids,
        SpiderMites,
        Thrips,
        Whiteflies,
        Fungusgnats,
        Caterpillars,
        Leafminers,
        Mealybugs,
        Scale,
        Nematodes
    }

    public enum BeneficialOrganismType
    {
        Ladybugs,
        LacewingLarvae,
        PredatoryMites,
        Parasitoids,
        Nematodes,
        BacillusThuringiensis,
        Beauveria,
        Metarhizium,
        Trichoderma,
        PredatoryBeetles
    }

    public enum DefenseStructureType
    {
        StickyTrap,
        PheromoneTrap,
        BiologicalReleaseStation,
        ChemicalSprayer,
        EnvironmentalController,
        UVSterilizer,
        AirFiltrationUnit,
        SonicDeterrent,
        ElectronicGrid,
        QuarantineBarrier
    }

    public enum IPMStrategyType
    {
        Preventive,
        Biological,
        Chemical,
        Environmental,
        Integrated,
        Emergency
    }

    public enum IPMDifficultyLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }

    public enum TreatmentType
    {
        Fungicide,
        BiologicalControl,
        EnvironmentalAdjustment,
        PlantRemoval,
        Quarantine,
        NutrientSupplement,
        PhAdjustment,
        Pruning
    }

    public enum DiseaseType
    {
        PowderyMildew,
        Botrytis,
        RootRot,
        BudRot,
        LeafSpot,
        Fusarium,
        Verticillium,
        Pythium,
        Alternaria,
        Septoria
    }

    public enum DiseaseSeverity
    {
        Low,
        Moderate,
        High,
        Critical
    }

    public enum DiseaseDetectionMethod
    {
        Visual,
        Microscopic,
        Chemical,
        Environmental
    }

    public enum RiskLevel
    {
        Low,
        Moderate,
        High,
        Critical
    }


    public enum ResistanceLevel
    {
        None,
        Low,
        Medium,
        High,
        Extreme,
        Immune
    }

    public enum EnvironmentalZoneType
    {
        TemperatureGradient,
        HumidityBarrier,
        AirflowChannel,
        LightSpectrum,
        CO2Concentration,
        PheromoneMask,
        SonicField,
        ElectromagneticField
    }

    #endregion

    #region Core Data Structures


    [Serializable]
    public class PestInvasionData
    {
        public string InvasionId;
        public PestType PestType;
        public int PopulationSize;
        public float AggressionLevel;
        public Vector3 OriginPoint;
        public List<Vector3> InvasionPaths;
        public ResistanceLevel ChemicalResistance;
        public ResistanceLevel BiologicalResistance;
        public float EnvironmentalTolerance;
        public List<string> PreferredTargets;
        public float ReproductionRate;
        public DateTime InvasionStartTime;
        public bool IsAdaptive;
        public Dictionary<string, float> BehaviorModifiers;
    }

    [Serializable]
    public class DefenseStructureData
    {
        public string StructureId;
        public DefenseStructureType Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Effectiveness;
        public float Range;
        public float PowerConsumption;
        public float MaintenanceCost;
        public bool IsActive;
        public DateTime InstallationTime;
        public Dictionary<PestType, float> PestEffectiveness;
        public List<string> RequiredResources;
        public float UpgradeLevel;
        public string PlayerId;
    }

    [Serializable]
    public class BeneficialOrganismData
    {
        public string OrganismId;
        public BeneficialOrganismType Type;
        public int PopulationSize;
        public Vector3 ReleaseLocation;
        public float SearchRadius;
        public List<PestType> TargetPests;
        public float HuntingEfficiency;
        public float SurvivalRate;
        public DateTime ReleaseTime;
        public bool IsEstablished;
        public float EnvironmentalStress;
        public Dictionary<string, float> PerformanceMetrics;
    }

    [Serializable]
    public class ChemicalApplicationData
    {
        public string ApplicationId;
        public string ChemicalName;
        public string ChemicalType;
        public Vector3 ApplicationZone;
        public float Concentration;
        public float Coverage;
        public DateTime ApplicationTime;
        public TimeSpan ActiveDuration;
        public List<PestType> TargetPests;
        public float EnvironmentalImpact;
        public bool IsOrganic;
        public string ApplicationMethod;
        public float Precision;
        public string PlayerId;
    }

    [Serializable]
    public class EnvironmentalZoneData
    {
        public string ZoneId;
        public EnvironmentalZoneType Type;
        public Vector3 Center;
        public Vector3 Size;
        public float Intensity;
        public Dictionary<string, float> Parameters;
        public bool IsActive;
        public float EnergyConsumption;
        public DateTime CreationTime;
        public List<PestType> AffectedPests;
        public float Effectiveness;
        public string PlayerId;
    }

    [Serializable]
    public class IPMResourceData
    {
        public string ResourceId;
        public string ResourceName;
        public string ResourceType;
        public int Quantity;
        public float Cost;
        public bool IsRenewable;
        public float RegenerationRate;
        public DateTime LastUpdated;
        public Dictionary<string, float> Properties;
    }

    [Serializable]
    public class IPMPlayerProfile
    {
        public string PlayerId;
        public string PlayerName;
        public int IPMLevel;
        public float TotalExperience;
        public Dictionary<IPMStrategyType, float> StrategyProficiency;
        public Dictionary<PestType, int> PestEncounters;
        public Dictionary<BeneficialOrganismType, int> OrganismDeployments;
        public List<string> UnlockedTechnologies;
        public List<string> ResearchProjects;
        public IPMStatistics Statistics;
        public DateTime ProfileCreated;
        public DateTime LastActive;
    }

    [Serializable]
    public class IPMStatistics
    {
        public int TotalBattles;
        public int BattlesWon;
        public int BattlesLost;
        public float WinRate;
        public int PestsEliminated;
        public int DefenseStructuresBuilt;
        public int BeneficialOrganismsReleased;
        public int ChemicalApplications;
        public int EnvironmentalZonesCreated;
        public TimeSpan TotalBattleTime;
        public float AverageScore;
        public int MultikillRecord;
        public int LongestWinStreak;
        public Dictionary<PestType, int> PestKills;
        public Dictionary<DefenseStructureType, int> StructureUsage;
    }

    [Serializable]
    public class IPMTechnologyData
    {
        public string TechnologyId;
        public string TechnologyName;
        public string Description;
        public int RequiredLevel;
        public List<string> Prerequisites;
        public float ResearchCost;
        public TimeSpan ResearchTime;
        public Dictionary<string, float> Benefits;
        public bool IsUnlocked;
        public DateTime UnlockTime;
        public string Category;
    }

    [Serializable]
    public class IPMResearchProject
    {
        public string ProjectId;
        public string ProjectName;
        public string Description;
        public IPMStrategyType ResearchCategory;
        public float Progress;
        public float RequiredResearch;
        public TimeSpan EstimatedCompletion;
        public DateTime StartTime;
        public List<string> RequiredResources;
        public Dictionary<string, float> ExpectedBenefits;
        public bool IsCompleted;
        public string ResearcherId;
    }

    [Serializable]
    public class DiseaseProfile
    {
        public DiseaseType DiseaseType;
        public float DetectionSensitivity;
        public List<DiseaseDetectionMethod> PreferredDetectionMethods;
        public List<string> TypicalSymptoms;
        public Dictionary<string, float> EnvironmentalTriggers;
        public List<TreatmentType> TreatmentOptions;
        public DiseaseSeverity Severity;
        public float ContagiousLevel;
        public float ProgressionRate;
    }

    [Serializable]
    public class DiseaseInstanceData
    {
        public DiseaseType DiseaseType;
        public DateTime DetectionDate;
        public DiseaseSeverity Severity;
        public float Confidence;
        public float ProgressionLevel;
        public float LastProgressionLevel;
        public float TreatmentProgress;
        public DateTime LastTreatmentDate;
        public Vector3 Location;
        public List<string> Symptoms;
    }

    [Serializable]
    public class PlantDiseaseData
    {
        public string PlantId;
        public List<DiseaseInstanceData> ActiveDiseases;
        public List<DiseaseInstanceData> EradicatedDiseases;
        public DateTime LastScanDate;
        public float OverallHealthScore;
    }

    [Serializable]
    public class DiseaseDetectionData
    {
        public DiseaseType DiseaseType;
        public float Confidence;
        public Vector3 Location;
        public List<string> Symptoms;
        public DiseaseDetectionMethod DetectionMethod;
        public DiseaseSeverity Severity;
        public float ProgressionLevel;
    }

    [Serializable]
    public class DiseaseDetectionResult
    {
        public string PlantId;
        public string ScanId;
        public List<DiseaseDetectionData> DetectedDiseases;
        public float Confidence;
        public RiskLevel RiskLevel;
        public DateTime DetectionTime;
    }

    [Serializable]
    public class DiseaseTreatmentData
    {
        public string TreatmentId;
        public string PlantId;
        public DiseaseType DiseaseType;
        public TreatmentType TreatmentType;
        public DateTime ApplicationTime;
        public float ExpectedEffectiveness;
        public float ActualEffectiveness;
        public bool IsSuccessful;
    }

    [Serializable]
    public class DiseaseTreatmentResult
    {
        public string TreatmentId;
        public bool IsSuccessful;
        public string ResultMessage;
        public float ExpectedEffectiveness;
        public float ActualEffectiveness;
    }

    [Serializable]
    public class DiseaseProgressionData
    {
        public string PlantId;
        public DiseaseType DiseaseType;
        public float ProgressionLevel;
        public DiseaseSeverity Severity;
        public float ProgressionRate;
    }

    [Serializable]
    public class DiseaseMetrics
    {
        public int TotalDiseasesDetected;
        public int TotalTreatmentsApplied;
        public int DiseasesEradicated;
        public int FalsePositives;
        public float DetectionAccuracy;
        public float SystemEffectiveness;
        public DateTime LastUpdated;
    }

    [Serializable]
    public class DiseaseSystemReport
    {
        public int TotalPlantsMonitored;
        public int TotalDiseasesDetected;
        public int TotalTreatmentsApplied;
        public int DiseasesEradicated;
        public float SystemEffectiveness;
        public float PreventionSuccessRate;
        public float AverageDetectionAccuracy;
        public int ActiveDiseases;
        public int CriticalDiseases;
        public DateTime ReportDate;
    }

    [Serializable]
    public class EnvironmentalRiskFactor
    {
        public string Name;
        public float RiskMultiplier;
        public List<PestType> AffectedPests;
        public float ThresholdValue;
        public bool IsActive;
    }

    [Serializable]
    public class DetectionMetrics
    {
        public int TotalScansPerformed;
        public int TotalPestsDetected;
        public int FalsePositives;
        public int EarlyWarningsIssued;
        public float DetectionAccuracy;
        public float ActualAccuracy;
        public DateTime LastUpdated;
    }

    #endregion

    #region Battle System Data Structures


    [Serializable]
    public class PestInvasionWave
    {
        public int WaveNumber;
        public TimeSpan StartDelay;
        public List<PestInvasionData> PestGroups;
        public Vector3 SpawnLocation;
        public List<Vector3> Waypoints;
        public float WaveIntensity;
        public Dictionary<string, object> SpecialConditions;
    }

    [Serializable]
    public class IPMBattleObjective
    {
        public string ObjectiveId;
        public string ObjectiveName;
        public string Description;
        public bool IsRequired;
        public float ProgressRequired;
        public float CurrentProgress;
        public bool IsCompleted;
        public Dictionary<string, object> Conditions;
        public List<string> RewardIds;
    }

    [Serializable]
    public class IPMStrategyPlan
    {
        public string PlanId;
        public IPMStrategyType PrimaryStrategy;
        public List<IPMStrategyType> SecondaryStrategies;
        public Dictionary<string, DefenseStructureData> PlannedDefenses;
        public Dictionary<string, BeneficialOrganismData> PlannedOrganisms;
        public Dictionary<string, ChemicalApplicationData> PlannedApplications;
        public Dictionary<string, EnvironmentalZoneData> PlannedZones;
        public float EstimatedCost;
        public float EstimatedEffectiveness;
        public DateTime CreationTime;
        public string CreatorId;
    }

    [Serializable]
    public class IPMBattleEvent
    {
        public string EventId;
        public string EventType;
        public DateTime Timestamp;
        public Vector3 Location;
        public string Description;
        public Dictionary<string, object> EventData;
        public List<string> AffectedEntities;
        public float Severity;
        public bool RequiresPlayerAction;
    }

    #endregion

    #region Analytics and Performance Data

    [Serializable]
    public class IPMAnalyticsData
    {
        public string AnalyticsId;
        public DateTime CollectionTime;
        public string PlayerId;
        public string BattleId;
        public Dictionary<string, float> PerformanceMetrics;
        public Dictionary<string, int> ActionCounts;
        public List<string> StrategySequence;
        public float EfficiencyScore;
        public Dictionary<PestType, float> PestControlEffectiveness;
        public float ResourceUtilization;
        public TimeSpan DecisionTime;
        public int APM; // Actions Per Minute
    }

    [Serializable]
    public class IPMSystemMetrics
    {
        public int ActiveBattles;
        public int TotalPlayersOnline;
        public float AverageFrameRate;
        public float MemoryUsage;
        public int NetworkLatency;
        public float AIProcessingTime;
        public int PestsSimulated;
        public int DefenseStructuresActive;
        public int BeneficialOrganismsActive;
        public DateTime LastUpdate;
        public Dictionary<string, float> SubSystemPerformance;
    }

    [Serializable]
    public class IPMLeaderboardEntry
    {
        public string PlayerId;
        public string PlayerName;
        public int Rank;
        public float Score;
        public IPMDifficultyLevel HighestDifficulty;
        public int TotalVictories;
        public float WinRate;
        public IPMStrategyType PreferredStrategy;
        public DateTime LastBattle;
        public List<string> Achievements;
    }

    #endregion

    #region AI and Learning Data

    [Serializable]
    public class PestAIBehavior
    {
        public PestType PestType;
        public float Aggressiveness;
        public float Intelligence;
        public float Adaptability;
        public Dictionary<DefenseStructureType, float> DefenseAvoidance;
        public Dictionary<BeneficialOrganismType, float> PredatorAvoidance;
        public Dictionary<string, float> ChemicalResistance;
        public List<Vector3> PreferredPaths;
        public float LearningRate;
        public Dictionary<string, float> PlayerCounters;
    }

    [Serializable]
    public class IPMLearningData
    {
        public string LearningSessionId;
        public string PlayerId;
        public List<string> ActionSequence;
        public List<float> Outcomes;
        public Dictionary<string, float> StrategyEffectiveness;
        public float OverallPerformance;
        public DateTime SessionStart;
        public DateTime SessionEnd;
        public Dictionary<string, object> ContextualFactors;
    }


    #endregion

    #region Events and Notifications

    [Serializable]
    public class IPMGameEvent
    {
        public string EventId;
        public string EventType;
        public string PlayerId;
        public string BattleId;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData;
        public bool IsNetworked;
        public int Priority;
    }

    [Serializable]
    public class IPMNotification
    {
        public string NotificationId;
        public string NotificationType;
        public string Title;
        public string Message;
        public string IconPath;
        public DateTime Timestamp;
        public bool IsRead;
        public Dictionary<string, object> ActionData;
        public int Priority;
    }

    // Additional types for Systems layer compatibility
    public enum AnalyticsType
    {
        Performance,
        Behavior,
        Strategy,
        Resource,
        Efficiency,
        Learning
    }
    
    public enum StrategyType
    {
        Preventive,
        Reactive,
        Biological,
        Chemical,
        Environmental,
        Integrated,
        Emergency,
        Adaptive
    }
    
    [Serializable]
    public class AnalyticsData
    {
        public string AnalyticsId;
        public AnalyticsType Type;
        public DateTime Timestamp;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public string PlayerId;
        public string SessionId;
    }
    
    [Serializable]
    public class StrategyData
    {
        public string StrategyId;
        public IPMStrategyType Type;
        public string Name;
        public string Description;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public float Effectiveness;
        public float Cost;
        public List<string> Requirements = new List<string>();
        public bool IsActive;
    }
    
    #endregion
}