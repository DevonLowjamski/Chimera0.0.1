using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Data.Construction;

namespace ProjectChimera.Data.Environment
{
    /// <summary>
    /// Data structures for atmospheric physics simulation in environmental systems.
    /// These types support the AtmosphericPhysicsSimulator for advanced environmental modeling.
    /// </summary>
    
    // Environmental Challenge System
    [System.Serializable]
    public enum EnvironmentalChallengeType
    {
        TemperatureFluctuation,
        TemperatureOptimization, // Added missing value
        HumiditySpike,
        HumidityControl, // Added missing value
        CO2Depletion,
        AirflowDisruption,
        LightingFailure,
        PowerOutage,
        ContaminationEvent,
        EquipmentMalfunction,
        ExternalWeatherEvent,
        PestInfestation,
        NutrientImbalance,
        VentilationBlockage,
        VPDMastery, // Added missing value
        EnergyEfficiency, // Required by EnhancedEnvironmentalGamingManager.cs line 430
        CrisisResponse // Added missing value required by line 433
    }
    
    [System.Serializable]
    public class EnvironmentalChallenge
    {
        public string ChallengeId;
        public EnvironmentalChallengeType ChallengeType;
        public EnvironmentalChallengeType Type { get => ChallengeType; set => ChallengeType = value; } // Added missing Type property (alias for ChallengeType)
        public string Description;
        public float Severity; // 0-1 scale
        public float Duration; // In hours
        public Vector3 AffectedArea;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        public Dictionary<string, float> ImpactFactors = new Dictionary<string, float>();
        public List<string> RequiredActions = new List<string>();
        public List<string> LearningResources = new List<string>(); // Added missing LearningResources property
        public string Status; // Added missing Status property
        
        // Helper method to set status from ChallengeStatus enum
        public void SetStatus(ChallengeStatus status)
        {
            Status = status.ToString();
        }
        
        // Property to allow direct assignment from ChallengeStatus enum
        public ChallengeStatus StatusEnum
        {
            get 
            { 
                if (System.Enum.TryParse<ChallengeStatus>(Status, out var result))
                    return result;
                return ChallengeStatus.NotStarted;
            }
            set { Status = value.ToString(); }
        }
        public ChallengeObjectives Objectives; // Added missing Objectives property
        public bool HasTimeLimit = true; // Added missing HasTimeLimit property
        public TimeSpan TimeLimit = TimeSpan.FromHours(24); // Added missing TimeLimit property
        public string TypeString { get => Type.ToString(); set => Type = System.Enum.Parse<EnvironmentalChallengeType>(value); } // Added missing TypeString property for string assignments
        public DifficultyLevel Difficulty = DifficultyLevel.Medium; // Added missing Difficulty property
        
                // Added missing UpdateChallenge method
        public void UpdateChallenge(float deltaTime)
        {
            if (IsActive && HasTimeLimit)
            {
                Duration -= deltaTime / 3600f; // Convert seconds to hours
                if (Duration <= 0)
                {
                    IsActive = false;
                    Status = "Expired";
                }
            }
        }
    }
    
    [System.Serializable]
    public class ChallengeObjectives
    {
        public List<ObjectiveTarget> PrimaryTargets = new List<ObjectiveTarget>();
        public List<ObjectiveTarget> SecondaryTargets = new List<ObjectiveTarget>();
        public List<ObjectiveTarget> BonusTargets = new List<ObjectiveTarget>();
        public bool RequireAllPrimaryTargets = true;
        public float MinimumSuccessScore = 0.7f;
    }
    
    [System.Serializable]
    public class EnvironmentalChallengeResult
    {
        public string ChallengeId;
        public bool WasSuccessful;
        public float ResponseTime; // In minutes
        public float EffectivenessScore; // 0-1 scale
        public float DamageMinimized; // 0-1 scale
        public List<string> ActionsPerformed = new List<string>();
        public float ExperienceGained;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
        
        // Additional properties referenced by EnvironmentalChallengeFramework - changed to settable
        public bool IsSuccessful { get; set; }
        public float FinalScore { get; set; }
        public float CompletionTime { get; set; } // Changed from DateTime to float for minutes
        public List<ObjectiveResult> ObjectiveResults = new List<ObjectiveResult>();
        public PerformanceMetrics Performance;
        public List<string> Innovations = new List<string>();
        public string Feedback;
        public float ExperiencePointsEarned { get; set; }
        public float Score { get; set; } // Added missing Score property
        public DateTime CompletedAt { get; set; } // Added missing CompletedAt property
    }
    
    [System.Serializable]
    public class ObjectiveResult
    {
        public string ObjectiveId;
        public bool IsCompleted;
        public float Score;
        public string Description;
        public float Weight;
        // Missing properties referenced in EnvironmentalChallengeFramework
        public float TargetValue;
        public float AchievedValue;
    }
    
    [System.Serializable]
    public class EnvironmentalCrisis
    {
        public string CrisisId;
        public string CrisisName;
        public List<EnvironmentalChallenge> CascadingChallenges = new List<EnvironmentalChallenge>();
        public float OverallSeverity;
        public float TimeToResolve;
        public bool RequiresImmediateAttention;
        public Vector3 EpicenterLocation;
        public float AffectedRadius;
    }
    
    // Physics Simulation Profiles and Parameters
    [System.Serializable]
    public class PhysicsSimulationProfile
    {
        public bool IsEnabled = true;
        public SimulationQuality Quality = SimulationQuality.Medium;
        public float UpdateFrequency = 1.0f; // Hz
        public int ParticleCount = 1000;
        public bool EnableRealTimeVisualization = false;
        public FluidDynamicsModel FluidModel;
        public ThermalModel ThermalModel;
        public TransportModel TransportModel;
        public Dictionary<string, object> CustomParameters = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public enum SimulationQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    [System.Serializable]
    public class SimulationParameters
    {
        public float TimeStep = 0.01f; // seconds
        public int MaxIterations = 1000;
        public float ConvergenceTolerance = 0.001f;
        public bool UseGPUAcceleration = true;
        public int MeshResolution = 32;
        public Vector3 SimulationBounds = Vector3.one * 10f;
        public Dictionary<string, float> PhysicsConstants = new Dictionary<string, float>();
    }
    
    // Facility and Geometry
    [System.Serializable]
    public class FacilityGeometry
    {
        public Vector3 Dimensions; // Length, Width, Height in meters
        public Vector3 Position;
        public List<Vector3> WallPositions = new List<Vector3>();
        public List<Vector3> VentPositions = new List<Vector3>();
        public List<Vector3> EquipmentPositions = new List<Vector3>();
        public List<Vector3> ObstaclePositions = new List<Vector3>();
        public float FloorArea; // Square meters
        public float Volume; // Cubic meters
        public float CeilingHeight; // Added missing CeilingHeight property
        public string GeometryType = "Rectangular"; // Rectangular, Circular, Custom
    }
    
    [System.Serializable]
    public class EnvironmentalDesignRequirements
    {
        public float MinTemperature = 18f;
        public float MaxTemperature = 28f;
        public float MinHumidity = 40f;
        public float MaxHumidity = 70f;
        public float MinCO2 = 400f;
        public float MaxCO2 = 1200f;
        public float MinAirflow = 0.1f;
        public float MaxAirflow = 2.0f;
        public float RequiredAirChangesPerHour = 12f;
        public bool RequiresFiltration = true;
        public float MaxTemperatureGradient = 2f; // °C difference max
        public float MaxHumidityGradient = 10f; // % difference max
        public float MinEfficiencyRating = 0.8f; // Added missing MinEfficiencyRating property
        
        // Missing range properties for AtmosphericPhysicsSimulator
        public TemperatureRange TargetTemperatureRange => new TemperatureRange { Min = MinTemperature, Max = MaxTemperature, Optimal = (MinTemperature + MaxTemperature) / 2f };
        public HumidityRange TargetHumidityRange => new HumidityRange { Min = MinHumidity, Max = MaxHumidity, Optimal = (MinHumidity + MaxHumidity) / 2f };
        public AirflowRequirements AirflowRequirements => new AirflowRequirements { MinimumVelocity = MinAirflow, MaximumVelocity = MaxAirflow, OptimalVelocity = (MinAirflow + MaxAirflow) / 2f };
    }
    
    [System.Serializable]
    public class EnvironmentalZoneSpecification
    {
        public string ZoneId;
        public string ZoneName;
        public string ZoneType; // Added missing ZoneType property
        public FacilityGeometry Geometry;
        public EnvironmentalDesignRequirements Requirements;
        public List<string> EquipmentIds = new List<string>();
        public Vector3 CenterPoint;
        public float Priority = 1.0f; // 0-1 scale for simulation priority
        public bool EnableAdvancedPhysics = true; // Added missing EnableAdvancedPhysics property
        public bool RequiresHVACIntegration = true; // Added missing RequiresHVACIntegration property
        public float MinEfficiencyRating = 0.8f; // Added missing MinEfficiencyRating property
    }
    
    // Boundary Conditions
    [System.Serializable]
    public class BoundaryCondition
    {
        public string ConditionId;
        public BoundaryType Type;
        public Vector3 Position;
        public Vector3 Normal;
        public float Value; // Temperature, velocity, pressure, etc.
        public string Units;
        public bool IsActive = true;
        public Dictionary<string, float> Parameters = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public enum BoundaryType
    {
        Wall,
        Inlet,
        Outlet,
        HeatSource,
        HeatSink,
        MoistureSource,
        MoistureSink,
        PressureOutlet,
        VelocityInlet,
        SymmetryPlane
    }
    
    // Performance Metrics
    [System.Serializable]
    public class PerformanceMetrics
    {
        public float ResponseTime;
        public float EfficiencyScore;
        public float AccuracyScore;
        public float InnovationScore;
        public float ResourceUtilization;
        public float SustainabilityScore;
        public Dictionary<string, float> CustomMetrics = new Dictionary<string, float>();
        public System.DateTime MeasurementTime;
        public int TotalActions;
        public int SuccessfulActions;
        public float AverageDecisionTime;
    }
    
    // Simulation Results and Fields
    [System.Serializable]
    public class AtmosphericSimulationResult
    {
        public string SimulationId;
        public DateTime StartTime;
        public DateTime EndTime;
        public float ComputationTime;
        public bool WasSuccessful;
        public Vector3Field VelocityField;
        public ScalarField TemperatureField;
        public ScalarField HumidityField;
        public ScalarField PressureField;
        public TurbulenceData TurbulenceData;
        public PerformanceMetrics PerformanceMetrics;
        public string ErrorMessage;
        
        // Missing properties referenced in AtmosphericPhysicsSimulator - changed to settable properties
        public ScalarField TemperatureDistribution { get; set; }
        public ScalarField HumidityDistribution { get; set; }
        public float TurbulenceIntensity { get; set; }
        public DateTime SimulationTime { get; set; }
        public Vector3Field Airflow { get; set; }
    }
    
    [System.Serializable]
    public class Vector3Field
    {
        public Vector3[,,] Data;
        public Vector3 GridSpacing;
        public Vector3 Origin;
        public Vector3Int Resolution;
        public float MinMagnitude;
        public float MaxMagnitude;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class ScalarField
    {
        public float[,,] Data;
        public Vector3 GridSpacing;
        public Vector3 Origin;
        public Vector3Int Resolution;
        public float MinValue;
        public float MaxValue;
        public string Units;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class TurbulenceData
    {
        public float TurbulentKineticEnergy;
        public float DissipationRate;
        public float TurbulenceIntensity;
        public float MixingLength;
        public Vector3 TurbulentViscosity;
        public ScalarField TurbulenceField;
        
        // Implicit conversion operator to float for TurbulenceIntensity access
        public static implicit operator float(TurbulenceData turbulenceData)
        {
            return turbulenceData?.TurbulenceIntensity ?? 0f;
        }
    }
    
    // Simulation Models
    [System.Serializable]
    public class FluidDynamicsModel
    {
        public string ModelType = "NavierStokes";
        public float Density = 1.225f; // kg/m³
        public float Viscosity = 1.81e-5f; // kg/(m·s)
        public bool IncludeGravity = true;
        public bool IncludeBuoyancy = true;
        public Vector3 GravityVector = new Vector3(0, -9.81f, 0);
        public Dictionary<string, float> ModelParameters = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class ThermalModel
    {
        public string ModelType = "ConvectionDiffusion";
        public float ThermalConductivity = 0.0262f; // W/(m·K)
        public float SpecificHeat = 1005f; // J/(kg·K)
        public bool IncludeRadiation = false;
        public bool IncludeConvection = true;
        public Dictionary<string, float> ModelParameters = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class TransportModel
    {
        public string ModelType = "AdvectionDiffusion";
        public float DiffusionCoefficient = 2.0e-5f; // m²/s
        public bool IncludeChemicalReactions = false;
        public bool IncludePhaseChange = false;
        public Dictionary<string, float> ModelParameters = new Dictionary<string, float>();
    }
    
    // Physics Simulation Result Types
    [System.Serializable]
    public class PhysicsSimulationResult
    {
        public string ResultId;
        public string ZoneId;
        public DateTime SimulationTime;
        public AtmosphericSimulationResult AtmosphericResult;
        public PerformanceMetrics Performance;
        public bool IsValid;
        public float QualityScore; // 0-1 scale
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
    
    // Challenge System Supporting Types
    [System.Serializable]
    public enum DifficultyLevel
    {
        Beginner = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Expert = 4,
        Master = 5
    }
    
    [System.Serializable]
    public enum CrisisType
    {
        EquipmentFailure,
        PowerOutage,
        EnvironmentalExtreme,
        SystemOverload,
        ContaminationEvent,
        WeatherEvent,
        HumanError,
        CascadingFailure
    }
    
    [System.Serializable]
    public enum CrisisSeverity
    {
        Minor = 1,
        Moderate = 2,
        Severe = 3,
        Critical = 4,
        Catastrophic = 5
    }
    
    [System.Serializable]
    public class EnvironmentalSolution
    {
        public string SolutionId;
        public string Title;
        public string Description;
        public string ChallengeId;
        public string PlayerId;
        public List<SolutionStep> Steps = new List<SolutionStep>();
        public Dictionary<string, float> Parameters = new Dictionary<string, float>();
        public List<string> TechniquesUsed = new List<string>();
        public List<string> ResourcesUsed = new List<string>();
        public float EstimatedCost;
        public float EstimatedTime;
        public SolutionComplexity Complexity;
        public System.DateTime SubmissionTime;
        public bool IsInnovative;
        public float SustainabilityScore;
    }
    
    [System.Serializable]
    public class SolutionStep
    {
        public string StepId;
        public string Title;
        public string Description;
        public int Order;
        public StepType Type;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public bool IsCompleted;
        public System.DateTime CompletionTime;
        public string Notes;
    }
    
    public enum StepType
    {
        Analysis,
        Design,
        Implementation,
        Testing,
        Optimization,
        Validation,
        Documentation
    }
    
    public enum SolutionComplexity
    {
        Simple,
        Moderate,
        Complex,
        Advanced,
        Expert
    }
    
    [System.Serializable]
    public enum ChallengeStatus
    {
        NotStarted,
        Created, // Added missing Created status
        InProgress,
        Completed,
        Failed,
        Expired,
        Cancelled
    }
    
    [System.Serializable]
    public class ObjectiveTarget
    {
        public string ObjectiveId;
        public string TargetId; // Added missing TargetId property
        public string Title;
        public string Description;
        public ObjectiveType Type;
        public float TargetValue;
        public float CurrentValue;
        public float Weight;
        public bool IsRequired;
        public bool IsCompleted;
        public System.DateTime DueDate;
        public List<string> Requirements = new List<string>();
        public Dictionary<string, float> Parameters = new Dictionary<string, float>();
        public Vector2 ToleranceRange = new Vector2(0.95f, 1.05f); // Added missing ToleranceRange property
        
        // Implicit conversion to float for cases where ToleranceRange is used as a single value
        public float ToleranceValue => ToleranceRange.y - ToleranceRange.x;
    }
    
    [System.Serializable]
    public class ChallengeHint
    {
        public string HintId;
        public string Title;
        public string Content;
        public string HintText; // Added missing HintText property
        public string Category; // Conceptual, Procedural, Strategic
        public int Level; // 1 = gentle nudge, 3 = detailed guidance
        public int SequenceNumber; // Added missing SequenceNumber property  
        public bool IsEducational;
        public bool IsDelivered; // Added missing IsDelivered property
        public DateTime DeliveryTime; // Added missing DeliveryTime property
        public List<string> RelatedConcepts = new List<string>();
        public string MediaUrl; // For diagrams, videos, etc.
    }
    
    #region Additional Environmental Types
    
    [Serializable]
    public class EnvironmentalZone
    {
        public string ZoneId;
        public string ZoneName;
        public string ZoneType;
        public FacilityGeometry Geometry;
        public EnvironmentalDesignRequirements DesignRequirements;
        public AtmosphericConfiguration AtmosphericConfiguration;
        public PhysicsSimulation PhysicsSimulation;
        public ClimateControl ClimateControl;
        public HVACIntegration HVACIntegration;
        public EnergyProfile EnergyProfile;
        public DateTime CreationTime;
        public EnvironmentalZoneStatus Status;
        public float Temperature;
        public float Humidity;
        public float CO2Level;
        public float LightIntensity;
        public int PlantCount;
        public float HealthScore;
        public List<string> ActiveIssues = new List<string>();
        public float ProductivityScore;
        
        // Additional property referenced in HVACDataStructures.cs
        public EnvironmentalConditions CurrentConditions;
    }
    
    [Serializable]
    public class AtmosphericConfiguration
    {
        public string ConfigurationId;
        public FluidDynamicsModel FluidModel;
        public ThermalModel ThermalModel;
        public TransportModel TransportModel;
        public List<BoundaryCondition> BoundaryConditions;
        public SimulationParameters SimulationParams;
        public bool IsActive;
    }
    
    [Serializable]
    public class PhysicsSimulation
    {
        public string SimulationId;
        public PhysicsSimulationProfile Profile;
        public AtmosphericSimulationResult LastResult;
        public bool IsRunning;
        public DateTime LastUpdate;
    }
    
    [Serializable]
    public class ClimateControl
    {
        public string ControlId;
        public Dictionary<string, float> TargetParameters;
        public Dictionary<string, float> CurrentParameters;
        public List<string> ActiveControllers;
        public bool IsAutoMode;
    }
    
    [Serializable]
    public class HVACIntegration
    {
        public string IntegrationId;
        public List<string> ConnectedDevices;
        public HVACSystemConfig SystemConfig;
        public float EfficiencyRating;
        public bool IsOperational;
    }
    
    [Serializable]
    public class EnergyProfile
    {
        public string ProfileId;
        public float CurrentConsumption;
        public float PeakConsumption;
        public float EfficiencyRating;
        public List<EnergyOptimizationSuggestion> OptimizationSuggestions;
        public DateTime LastAnalysis;
    }
    
    [Serializable]
    public class HVACSystemConfig
    {
        public string SystemType;
        public float Capacity;
        public float EfficiencyRating;
        public List<string> Components;
        public Dictionary<string, float> OperatingParameters;
    }
    
    [Serializable]
    public class EnergyOptimizationSuggestion
    {
        public string SuggestionId;
        public string Description;
        public float PotentialSavings;
        public float ImplementationCost;
        public string Priority;
    }
    
    // PlayerEnvironmentalProfile removed - already defined in HVACDataStructures.cs
    
    // EnvironmentalBreakthrough removed - already defined in HVACDataStructures.cs
    
    // CollaborativeEnvironmentalConfig removed - already defined in HVACDataStructures.cs
    
    // EnvironmentalKnowledge removed - already defined in HVACDataStructures.cs
    
    // ProfessionalActivity removed - already defined in HVACDataStructures.cs
    
    public enum ActivityType
    {
        Training,
        Certification,
        Workshop,
        Conference,
        Mentoring,
        Research,
        Publication,
        Presentation
    }
    
    // ProfessionalInterests removed - already defined in HVACDataStructures.cs
    
    // IndustryConnectionResult removed - already defined in HVACDataStructures.cs
    
    // ProfessionalContact removed - already defined in HVACDataStructures.cs
    
    // PredictionTimeframe enum removed - already defined in HVACDataStructures.cs
    
    // EnvironmentalPrediction removed - already defined in HVACDataStructures.cs
    
    // OptimizationRecommendation removed - already defined in HVACDataStructures.cs
    
    // System Classes (placeholder implementations)
        // EnvironmentalKnowledgeNetwork removed - already defined in HVACDataStructures.cs
    
    [Serializable]
    public class AtmosphericEngineeringEngine
    {
        public void Initialize() { }
        public void Shutdown() { }
        public AtmosphericConfiguration CreateAtmosphericConfiguration(EnvironmentalZoneSpecification spec) { return new AtmosphericConfiguration(); }
        public AtmosphericSimulationResult InitializeAtmosphericSimulation(EnvironmentalZone zone) { return new AtmosphericSimulationResult(); }
        public void EnableAdvancedPhysics(bool enabled) { }
        public void EnableCFDSimulation(bool enabled) { } // Added missing method
        public void SetPhysicsAccuracy(float accuracy) { } // Added missing method
        public void UpdateAtmosphericSimulation(EnvironmentalZone zone) { } // Added missing method
        public void UpdateChallenge(EnvironmentalChallenge challenge) { } // Added missing UpdateChallenge method
    }
    
    [Serializable]
    public class EnvironmentalPhysicsSimulator
    {
        public void Initialize() { }
        public void Initialize(bool enableCFD, float accuracy) { } // Added missing overload
        public void Shutdown() { }
        public PhysicsSimulation InitializeZoneSimulation(EnvironmentalZoneSpecification spec) { return new PhysicsSimulation(); }
        public PhysicsSimulationResult RunAdvancedSimulation(EnvironmentalZone zone) { return new PhysicsSimulationResult(); }
        public AtmosphericSimulationResult RunAdvancedSimulation(EnvironmentalZone zone, SimulationParameters parameters) { return new AtmosphericSimulationResult(); }
        public void UpdatePhysicsSimulation(EnvironmentalZone zone) { } // Added missing method
    }
    
    [Serializable]
    public class MultiZoneClimateController
    {
        public void Initialize() { }
        public void Initialize(bool enableMultiZone) { } // Added missing overload
        public void Shutdown() { }
        public ClimateControl SetupZoneControl(EnvironmentalZoneSpecification spec) { return new ClimateControl(); }
    }
    
    [Serializable]
    public class HVACSystemIntegrator
    {
        public void Initialize() { }
        public void Initialize(bool enableHVAC) { } // Added missing overload
        public void Shutdown() { }
        public HVACIntegration DesignHVACSystem(EnvironmentalZoneSpecification spec) { return new HVACIntegration(); }
    }
    
    [Serializable]
    public class EnergyOptimizationEngine
    {
        public void Initialize() { }
        public void Initialize(bool enableOptimization) { } // Added missing overload
        public EnergyOptimizationEngine() { }
        public EnergyOptimizationEngine(bool enabled) { }
        public void Shutdown() { }
        public EnergyProfile AnalyzeEnergyRequirements(EnvironmentalZoneSpecification spec) { return new EnergyProfile(); }
    }
    
    [Serializable]
    public class CrisisSimulationEngine
    {
        public void Initialize() { }
        public void Shutdown() { }
        public EnvironmentalCrisis GenerateCrisis(CrisisType crisisType, CrisisSeverity severity) { return new EnvironmentalCrisis(); }
    }
    
    [Serializable]
    public class OptimizationCompetitionManager
    {
        public void Initialize() { }
        public void Initialize(bool enableCompetitions) { 
            Initialize(); // Call base initialization
            // Additional competition features could be enabled here
        }
        public void Shutdown() { }
    }
    
    [Serializable]
    public class EnvironmentalPuzzleGenerator
    {
        public void Initialize() { }
        public void Shutdown() { }
    }
    
    // PredictiveEnvironmentalIntelligence removed - already defined in HVACDataStructures.cs
    
    // CollaborativeResearchPlatform removed - already defined in HVACDataStructures.cs
    
    // EnvironmentalKnowledgeNetwork removed - already defined in HVACDataStructures.cs
    
    // GlobalEnvironmentalCompetitions removed - already defined in HVACDataStructures.cs
    
    // HVACCertificationSystem removed - already defined in HVACDataStructures.cs
    
    // ProfessionalNetworkingPlatform removed - already defined in HVACDataStructures.cs
    
    // CareerPathwayManager removed - already defined in HVACDataStructures.cs
    
    // CollaborativeSession removed - already defined in HVACDataStructures.cs
    
    // OptimizationObjectives and HVACCertification removed - already defined in HVACDataStructures.cs
    
    // EnvironmentalInnovationHub class removed - already defined in HVACDataStructures.cs
    
    public enum EnvironmentalZoneStatus
    {
        Designing,
        Active,
        Inactive,
        Optimizing,
        Maintenance,
        Error,
        Shutdown
    }
    
    public enum EnvironmentalZoneType
    {
        VegetativeChamber,
        FloweringRoom,
        SeedlingNursery,
        MotherPlantRoom,
        DryingRoom,
        CuringRoom,
        ProcessingLab,
        StorageArea,
        WorkArea
    }
    
    public enum CollaborativeSessionType
    {
        ResearchProject,
        DesignSession,
        ProblemSolving,
        TrainingSession,
        CertificationPrep,
        CompetitionTeam,
        KnowledgeSharing,
        Innovation
    }
    
    public enum IEnvironmentalGamingSystemStatus
    {
        Inactive,
        Starting,
        Active,
        Paused,
        Stopping,
        Error
    }
    
    // Note: IEnvironmentalGamingSystem interface already exists in ProjectChimera.Core - removed duplicate
    // Note: CollaborativeSession is already defined in HVACDataStructures.cs - removed duplicate
    
    public enum OptimizationStatus
    {
        InProgress,
        Completed,
        Failed,
        Cancelled,
        Pending
    }
    
    public enum OptimizationPriority
    {
        Critical,
        High,
        Medium,
        Low,
        Optional
    }
    
    public enum OptimizationTimeframe
    {
        Immediate,
        ShortTerm,
        MediumTerm,
        LongTerm
    }
    
    public enum OptimizationMetricType
    {
        Energy,
        Cost,
        Performance,
        Efficiency,
        Quality,
        Sustainability,
        Safety,
        Reliability,
        Comfort,
        Productivity
    }
    
    #endregion
    
    #region Environmental Gaming Data Structures
    
    // Environmental Gaming Achievement Types
    [Serializable]
    public class EnvironmentalOptimization
    {
        public string OptimizationId;
        public EnvironmentalZone Zone;
        public EnvironmentalOptimizationResult Result;
        public DateTime AchievedAt;
        public string PlayerId;
        public OptimizationCategory Category;
        public float ImprovementScore;
        public List<OptimizationMetric> Metrics;
        public OptimizationComplexity Complexity;
        public List<string> TechniquesUsed;
    }

    // EnvironmentalOptimizationResult class removed - already defined in HVACDataStructures.cs
    
    [Serializable]
    public class AtmosphericBreakthrough
    {
        public string BreakthroughId;
        public BreakthroughType Type;
        public string Title;
        public string Description;
        public DateTime DiscoveredAt;
        public string PlayerId;
        public EnvironmentalZone Zone;
        public float InnovationScore;
        public List<BreakthroughMetric> Metrics;
        public BreakthroughImpact Impact;
        public List<string> ResearchMethods;
        public bool IsIndustryRelevant;
    }
    
    // HVACCertification removed - already defined in HVACDataStructures.cs
    
    [Serializable]
    public class EnvironmentalInnovation
    {
        public string InnovationId;
        public InnovationType Type;
        public string Title;
        public string Description;
        public DateTime CreatedAt;
        public string PlayerId;
        public InnovationCategory Category;
        public float InnovationScore;
        public List<InnovationMetric> Metrics;
        public InnovationComplexity Complexity;
        public List<string> TechnologiesUsed;
        public bool HasCommercialPotential;
        
        // Additional properties referenced in HVACDataStructures.cs
        public DateTime DiscoveryDate { get => CreatedAt; set => CreatedAt = value; }
        public string ZoneId;
        public InnovationType InnovationType { get => Type; set => Type = value; }
    }
    
    // CollaborativeSession removed - already defined in HVACDataStructures.cs
    
    [Serializable]
    public class EnergyEfficiencyAchievement
    {
        public string AchievementId;
        public EfficiencyCategory Category;
        public string Title;
        public string Description;
        public DateTime AchievedAt;
        public string PlayerId;
        public float EfficiencyGain;
        public float EnergySaved;
        public List<EfficiencyMetric> Metrics;
        public EfficiencyComplexity Complexity;
        public List<string> TechniquesUsed;
        public bool IsIndustryBenchmark;
    }
    
    // EnvironmentalGamingMetrics removed - already defined in HVACDataStructures.cs
    
    // Supporting Enums
    // Note: BreakthroughType enum is defined in HVACDataStructures.cs
    
    public enum CertificationType
    {
        HVACFundamentals,
        AdvancedClimateControl,
        EnergyEfficiencySpecialist,
        AtmosphericEngineering,
        VentilationDesign,
        AutomationSystems,
        EnvironmentalCompliance,
        IndustryStandards,
        SafetyProtocols,
        MaintenanceExcellence
    }
    
    public enum InnovationType
    {
        ProcessImprovement,
        TechnologyIntegration,
        EfficiencyOptimization,
        AutomationAdvancement,
        SustainabilityInnovation,
        CostReduction,
        QualityEnhancement,
        SafetyImprovement,
        ComplianceInnovation,
        UserExperienceImprovement
    }
    
    public enum CollaborationGoal
    {
        ResearchProject,
        ProblemSolving,
        SkillDevelopment,
        Innovation,
        Competition,
        Training,
        Certification,
        KnowledgeSharing
    }
    
    public enum EfficiencyCategory
    {
        EnergyConsumption,
        ResourceUtilization,
        OperationalEfficiency,
        MaintenanceOptimization,
        CostReduction,
        PerformanceImprovement,
        SustainabilityGains,
        AutomationEfficiency,
        ProcessStreamlining,
        WasteReduction
    }
    
    public enum ObjectiveType
    {
        OptimizationMastery,
        BreakthroughAchievement,
        CertificationCompletion,
        InnovationCreation,
        CollaborationSuccess,
        EfficiencyImprovement,
        KnowledgeAcquisition,
        SkillDevelopment,
        IndustryRecognition,
        LeadershipDemonstration
    }
    
    public enum OptimizationCategory
    {
        EnergyEfficiency,
        TemperatureControl,
        HumidityManagement,
        AirQuality,
        VPDOptimization,
        SystemIntegration,
        CostReduction,
        Performance,
        Sustainability
    }
    
    public enum InnovationCategory
    {
        TechnicalInnovation,
        ProcessInnovation,
        ProductInnovation,
        ServiceInnovation,
        BusinessModelInnovation,
        SustainabilityInnovation,
        DigitalInnovation,
        AutomationInnovation,
        IntegrationInnovation,
        UserExperienceInnovation
    }
    
    public enum SessionStatus
    {
        Planning,
        Active,
        Paused,
        Completed,
        Cancelled,
        Archived
    }
    
    public enum CertificationStatus
    {
        InProgress,
        Completed,
        Expired,
        Suspended,
        Revoked
    }
    
    // Note: BreakthroughImpact enum is defined in HVACDataStructures.cs
    
    public enum OptimizationComplexity
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum InnovationComplexity
    {
        Simple,
        Moderate,
        Complex,
        Advanced,
        Groundbreaking
    }
    
    public enum EfficiencyComplexity
    {
        Straightforward,
        Moderate,
        Challenging,
        Advanced,
        ExpertLevel
    }
    
    public enum GameProgressLevel
    {
        Novice,
        Apprentice,
        Practitioner,
        Expert,
        Master,
        GrandMaster
    }
    
    public enum HVACCertificationLevel
    {
        Foundation,
        Basic,
        Intermediate,
        Advanced,
        Professional,
        Expert,
        Master
    }
    
    public enum EnvironmentalSkillLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    [Serializable]
    public class HVACCertificationEnrollment
    {
        public string EnrollmentId;
        public HVACCertificationLevel Level;
        public DateTime EnrolledAt;
        public DateTime ExpectedCompletion;
        public float Progress; // 0-1
        public CertificationStatus Status;
        public string PlayerId;
        public List<string> CompletedModules = new List<string>();
        public float CurrentScore;
    }
    
    [Serializable]
    public class EnvironmentalAchievement
    {
        public string AchievementId;
        public string Title;
        public string Description;
        public DateTime EarnedAt;
        public string PlayerId;
        public AchievementCategory Category;
        public float Points;
        public AchievementRarity Rarity;
        public bool IsVisible;
        public List<string> Prerequisites = new List<string>();
    }
    
    public enum AchievementCategory
    {
        Optimization,
        Innovation,
        Collaboration,
        Certification,
        Challenge,
        Discovery,
        Efficiency,
        Breakthrough,
        Leadership,
        Education
    }
    
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    // Supporting Data Structures
    [Serializable]
    public class OptimizationMetric
    {
        public string MetricName;
        public float Value;
        public string Unit;
        public float ImprovementPercentage;
        public MetricCategory Category;
    }
    
    [Serializable]
    public class BreakthroughMetric
    {
        public string MetricName;
        public float Value;
        public string Unit;
        public float SignificanceScore;
        public MetricCategory Category;
    }
    
    [Serializable]
    public class InnovationMetric
    {
        public string MetricName;
        public float Value;
        public string Unit;
        public float InnovationScore;
        public MetricCategory Category;
    }
    
    [Serializable]
    public class EfficiencyMetric
    {
        public string MetricName;
        public float Value;
        public string Unit;
        public float EfficiencyGain;
        public MetricCategory Category;
    }
    
    [Serializable]
    public class CompetencyArea
    {
        public string AreaName;
        public string Description;
        public float ProficiencyLevel;
        public List<string> Skills;
        public DateTime LastAssessed;
    }
    
    [Serializable]
    public class PracticalAssessment
    {
        public string AssessmentId;
        public string Title;
        public float Score;
        public DateTime CompletedAt;
        public List<string> CompetenciesEvaluated;
        public string FeedbackNotes;
    }
    
    // CollaborativeObjective removed - already defined in CollaborativeConstructionSystem.cs
    
    // CollaborationMetrics removed - already defined in CollaborativeConstructionSystem.cs
    
    // SharedResource removed - already defined in CollaborativeConstructionSystem.cs
    
    // CommunicationChannel removed - already defined in CollaborativeConstructionSystem.cs
    
    // CommunicationMessage removed - already defined in CollaborativeConstructionSystem.cs
    
    // Additional Supporting Enums
    public enum MetricCategory
    {
        Performance,
        Efficiency,
        Quality,
        Innovation,
        Sustainability,
        Cost,
        Safety,
        Compliance
    }
    
    public enum ObjectiveStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold,
        Cancelled
    }
    
    public enum ResourceType
    {
        Data,
        Equipment,
        Expertise,
        Documentation,
        Software,
        Facilities,
        Knowledge,
        Contacts,
        Materials,
        Tools
    }
    
    public enum ChannelType
    {
        TextChat,
        VoiceChat,
        VideoCall,
        SharedWorkspace,
        DocumentSharing,
        ScreenShare,
        VirtualMeeting,
        Forum
    }
    
    public enum MessageType
    {
        Text,
        Voice,
        Video,
        File,
        Image,
        Document,
        Link,
        System
    }
    
    // Missing range types for AtmosphericPhysicsSimulator
    [Serializable]
    public class TemperatureRange
    {
        public float Min;
        public float Max;
        public float Optimal;
        public string Units = "°C";
    }
    
    [Serializable]
    public class HumidityRange
    {
        public float Min;
        public float Max;
        public float Optimal;
        public string Units = "%";
    }
    
    [Serializable]
    public class AirflowRequirements
    {
        public float MinimumVelocity;
        public float MaximumVelocity;
        public float OptimalVelocity;
        public string Units = "m/s";
        public float RequiredAirChangesPerHour = 12f;
        public bool RequiresFiltration = true;
    }
    
    // EnvironmentalConditions class removed - using the comprehensive version from EnvironmentalConditions.cs
    
    [Serializable]
    public class LearningObjective
    {
        public string ObjectiveId;
        public string Title;
        public string Description;
        public ObjectiveType Type;
        public float TargetValue;
        public float CurrentValue;
        public float Weight;
        public bool IsRequired;
        public bool IsCompleted;
        public DateTime DueDate;
        public List<string> Requirements = new List<string>();
        public Dictionary<string, float> Parameters = new Dictionary<string, float>();
        
        // Implicit conversion from ObjectiveResult to LearningObjective
        public static implicit operator LearningObjective(ObjectiveResult result)
        {
            return new LearningObjective
            {
                ObjectiveId = result.ObjectiveId,
                Title = result.Description,
                Description = result.Description,
                IsCompleted = result.IsCompleted,
                Weight = result.Weight,
                TargetValue = result.TargetValue,
                CurrentValue = result.AchievedValue
            };
        }
    }
    
    [Serializable] 
    public class LearningResource 
    { 
        public string ResourceId;
        public string Title;
        public string Description;
        public string Url;
        public string Type; // Video, Article, Interactive, etc.
        
        public override string ToString()
        {
            return Title ?? ResourceId ?? "Unknown Resource";
        }
    }
    
    [Serializable] 
    public class ChallengeTemplate 
    { 
        public string TemplateId;
        public string TemplateName;
        public string Description;
        public EnvironmentalChallengeType ChallengeType;
        public DifficultyLevel Difficulty;
    }
    
    [Serializable]
    public class EnvironmentalForecast
    {
        public string Parameter;
        public float PredictedValue;
        public float ConfidenceLevel;
        public DateTime TimePoint;
        public string Units;
        public string TrendDirection;
    }
    
    [Serializable]
    public class EnvironmentalAnalyticsEngine
    {
        public void Initialize() { }
        public void Initialize(bool enableAnalytics) { 
            Initialize(); // Call base initialization
            // Additional analytics features could be enabled here
        }
        public void Shutdown() { }
        public void UpdateAnalytics() { }
    }
    
    [Serializable]
    public class EnvironmentalTrendAnalyzer
    {
        public void Initialize() { }
        public void Shutdown() { }
    }
    
    #endregion
} 