using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Core.Interfaces;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Construction.Buildings
{
    /// <summary>
    /// Building-related data types extracted from ConstructionDataStructures.cs
    /// This file contains all building, facility, and room-specific data structures
    /// for the Project Chimera construction system, separated from process-oriented types.
    /// </summary>

    // ============================================================================
    // ENUMS - Building Types and Classifications
    // ============================================================================

    [System.Serializable]
    public enum BuildingType
    {
        GrowTent,
        GrowRoom,
        Greenhouse,
        ProcessingFacility,
        StorageWarehouse,
        LaboratoryFacility,
        OfficeSpace,
        SecurityFacility,
        UtilityBuilding,
        ResearchFacility
    }

    [System.Serializable]
    public enum BuildingQuality
    {
        Basic,
        Standard,
        Premium,
        Luxury,
        Industrial
    }

    [System.Serializable]
    public enum RoomType
    {
        General,
        GrowRoom,
        ProcessingRoom,
        StorageRoom,
        OfficeSpace,
        LaboratoryRoom,
        SecurityRoom,
        UtilityRoom,
        MaintenanceRoom,
        ReceptionArea,
        ConferenceRoom,
        // Cannabis-specific room types
        Propagation,
        Vegetative,
        Flowering,
        Drying,
        Curing,
        Storage,
        Processing,
        Laboratory,
        Office,
        Security,
        Utility,
        Corridor,
        Restroom,
        Break,
        Meeting
    }

    [System.Serializable]
    public enum RoomStatus
    {
        Planning,
        Planned,
        UnderConstruction,
        Completed,
        Active,
        Maintenance,
        Idle,
        InProgress,
        OnHold,
        Cancelled,
        Inactive,
        Decommissioned
    }

    [System.Serializable]
    public enum ProjectType
    {
        Residential,
        Commercial,
        Industrial,
        Agricultural,
        Research,
        Mixed,
        GrowRoom,
        ProcessingFacility,
        Greenhouse,
        Laboratory
    }

    [System.Serializable]
    public enum SecurityLevel
    {
        None,
        Standard,
        Medium,
        High,
        Maximum
    }

    [System.Serializable]
    public enum AccessControlLevel
    {
        Public,
        Restricted,
        Authorized,
        HighSecurity,
        Maximum
    }

    [System.Serializable]
    public enum MonitoringLevel
    {
        None,
        Basic,
        Standard,
        Enhanced,
        Comprehensive
    }

    [System.Serializable]
    public enum ComplianceStatusType
    {
        Pending,
        Compliant,
        NonCompliant,
        UnderReview,
        Expired
    }

    // ============================================================================
    // FACILITY INFORMATION AND TEMPLATES
    // ============================================================================

    /// <summary>
    /// Comprehensive facility information including capacity, utilization, and operational metrics
    /// </summary>
    [System.Serializable]
    public class FacilityInfo
    {
        [Header("Facility Identification")]
        public string ProjectId;
        public string FacilityName;
        public BuildingType FacilityType;
        public ProjectStatus Status;
        
        [Header("Capacity and Utilization")]
        public float TotalCapacity; // Max plants or processing capacity
        public float CurrentUtilization; // Current usage
        public float UtilizationPercentage; // Utilization as percentage
        public float EstimatedCapacity; // Projected capacity when complete
        public float AvailableCapacity; // Remaining capacity
        
        [Header("Room Information")]
        public int TotalRooms;
        public int CompletedRooms;
        public int InProgressRooms;
        public int PlannedRooms;
        public float TotalArea; // Total floor area in sqft
        public float UsableArea; // Area available for cultivation/processing
        
        [Header("Resource Consumption")]
        public float PowerConsumption; // Daily power consumption in kWh
        public float WaterConsumption; // Daily water consumption in gallons
        public float CO2Consumption; // Daily CO2 consumption in lbs
        
        [Header("Operational Costs")]
        public float MaintenanceCost; // Annual maintenance cost
        public float OperationalCost; // Annual operational cost
        public float EnergyCost; // Annual energy cost
        public float WaterCost; // Annual water cost
        public float TotalOperatingCost; // Total annual operating cost
        
        [Header("Expansion and Growth")]
        public float ExpansionPotential; // Available area for expansion
        public float MaximumCapacity; // Theoretical maximum capacity
        public bool CanExpand; // Whether facility can be expanded
        public List<string> RecommendedExpansions; // Suggested expansion types
        
        [Header("Environmental Conditions")]
        public float AverageTemperature; // Average facility temperature
        public float AverageHumidity; // Average facility humidity
        public float AirQualityIndex; // Air quality rating
        public bool ClimateControlActive; // Climate control status
        
        [Header("Efficiency Metrics")]
        public float EnergyEfficiency; // Energy efficiency rating (0-1)
        public float WaterEfficiency; // Water efficiency rating (0-1)
        public float SpaceEfficiency; // Space utilization efficiency (0-1)
        public float OverallEfficiency; // Overall facility efficiency (0-1)
        
        [Header("Compliance and Safety")]
        public bool ComplianceStatus; // Regulatory compliance status
        public List<string> SafetyIncidents; // Recent safety incidents
        public DateTime LastInspectionDate; // Last safety inspection
        public DateTime NextInspectionDate; // Next scheduled inspection
        
        [Header("Performance Tracking")]
        public DateTime LastUpdated;
        public float DailyThroughput; // Daily processing throughput
        public float WeeklyThroughput; // Weekly processing throughput
        public float MonthlyThroughput; // Monthly processing throughput
        public float YearlyThroughput; // Yearly processing throughput
        
        [Header("Quality Metrics")]
        public float ProductQualityScore; // Average product quality
        public float ProcessingEfficiency; // Processing efficiency
        public float YieldEfficiency; // Yield efficiency
        public float WasteReduction; // Waste reduction percentage
        
        [Header("Automation and Technology")]
        public int AutomationLevel; // Level of automation (0-5)
        public List<string> InstalledSystems; // Installed automation systems
        public List<string> PendingUpgrades; // Pending technology upgrades
        public bool SmartControlsActive; // Smart control system status
        
        [Header("Staff and Workforce")]
        public int TotalStaff; // Total staff count
        public int ActiveStaff; // Currently active staff
        public float StaffProductivity; // Staff productivity rating
        public float StaffSatisfaction; // Staff satisfaction rating
        
        [Header("Economic Performance")]
        public float RevenueGeneration; // Monthly revenue generation
        public float ProfitMargin; // Profit margin percentage
        public float ROI; // Return on investment
        public float PaybackPeriod; // Payback period in months
        
        public FacilityInfo()
        {
            RecommendedExpansions = new List<string>();
            SafetyIncidents = new List<string>();
            InstalledSystems = new List<string>();
            PendingUpgrades = new List<string>();
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Calculates comprehensive facility efficiency score
        /// </summary>
        public float CalculateOverallEfficiency()
        {
            OverallEfficiency = (EnergyEfficiency + WaterEfficiency + SpaceEfficiency) / 3f;
            return OverallEfficiency;
        }
        
        /// <summary>
        /// Determines if facility needs immediate attention
        /// </summary>
        public bool RequiresAttention()
        {
            return OverallEfficiency < 0.6f || 
                   !ComplianceStatus || 
                   SafetyIncidents.Count > 0 ||
                   UtilizationPercentage > 95f;
        }
        
        /// <summary>
        /// Gets facility status summary
        /// </summary>
        public string GetStatusSummary()
        {
            if (RequiresAttention())
                return "Requires Attention";
            if (UtilizationPercentage > 85f)
                return "High Utilization";
            if (OverallEfficiency > 0.8f)
                return "Optimal Performance";
            return "Normal Operations";
        }
        
        /// <summary>
        /// Gets next recommended action
        /// </summary>
        public string GetNextRecommendedAction()
        {
            if (!ComplianceStatus)
                return "Address compliance issues";
            if (SafetyIncidents.Count > 0)
                return "Resolve safety incidents";
            if (UtilizationPercentage > 90f && CanExpand)
                return "Consider facility expansion";
            if (OverallEfficiency < 0.7f)
                return "Optimize facility operations";
            if (PendingUpgrades.Count > 0)
                return "Install pending upgrades";
            return "Continue monitoring";
        }
    }

    [System.Serializable]
    public class FacilityTemplate
    {
        [Header("Template Information")]
        public string TemplateId;
        public string TemplateName;
        public string Description;
        public BuildingType FacilityType;
        public BuildingQuality QualityLevel;

        [Header("Dimensions and Layout")]
        public Vector2 Dimensions;
        public float TotalArea;
        public List<ConstructionRoomTemplate> RoomTemplates;

        [Header("System Requirements")]
        public float RequiredHVACCapacity; // in tons
        public float RequiredPowerCapacity; // in watts

        [Header("Construction Requirements")]
        public List<MaterialRequirement> RequiredMaterials;
        public List<PermitType> RequiredPermits;
        public List<WorkerSpecialty> RequiredSpecialties;
        public int EstimatedConstructionDays;

        [Header("Cost Estimates")]
        public float BaseConstructionCost;
        public float EstimatedMaterialCost;
        public float EstimatedLaborCost;
        public float EstimatedPermitCost;
        public float EstimatedTotalCost;

        [Header("Compliance and Standards")]
        public List<string> BuildingCodes;
        public List<string> SafetyStandards;
        public bool CannabisSuitability;

        public List<ConstructionRoomTemplate> Rooms => RoomTemplates;
    }

    [System.Serializable]
    public class ConstructionRoomTemplate
    {
        [Header("Room Information")]
        public string TemplateRoomId;
        public string RoomName;
        public string RoomType;
        public string Description;

        [Header("Dimensions")]
        public Vector2 Dimensions;
        public float Area;
        public float Height;
        public float Length;
        public float Width;

        [Header("Cost")]
        public float EstimatedCost;
        public List<MaterialRequirement> MaterialRequirements;

        public bool IsGrowRoom => RoomType == "GrowRoom";
        public bool IsProcessingRoom => RoomType == "ProcessingRoom";
    }

    [System.Serializable]
    public class PlannedRoom
    {
        public string PlannedRoomId;
        public ConstructionRoomTemplate RoomTemplate;
        public Vector3 Position;
        public Quaternion Rotation;
        public RoomStatus Status;
        public float EstimatedCost;
        public float ActualCost;
        public DateTime PlannedDate;
        public DateTime CompletedDate;
        public List<string> ModificationNotes;
    }

    // ============================================================================
    // ROOM DATA STRUCTURES
    // ============================================================================

    /// <summary>
    /// Core room data structure for construction system
    /// </summary>
    [System.Serializable]
    public class Room
    {
        [Header("Room Identification")]
        public string RoomId;
        public string RoomName;
        public string ProjectId;
        public RoomType RoomType;
        
        [Header("Physical Properties")]
        public Vector3 Position;
        public Vector2 Dimensions;
        public float FloorArea;
        public float CeilingHeight;
        public float Volume;
        
        [Header("Status")]
        public RoomStatus Status;
        public DateTime CreationDate;
        public DateTime CompletionDate;
        
        [Header("Configuration")]
        public RoomConfiguration Configuration;
        public EnvironmentalConditions EnvironmentalRequirements;
        public SecurityLevel SecurityLevel;
        public List<RegulatoryRequirement> RegulatoryRequirements = new List<RegulatoryRequirement>();
        
        [Header("Capacity")]
        public int MaxOccupancy;
        public int CurrentOccupancy;
        public float CapacityUtilization => MaxOccupancy > 0 ? (float)CurrentOccupancy / MaxOccupancy : 0f;
        
        public Room()
        {
            RoomId = System.Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
            Status = RoomStatus.Planning;
            RegulatoryRequirements = new List<RegulatoryRequirement>();
        }
    }

    /// <summary>
    /// Room configuration settings
    /// </summary>
    [System.Serializable]
    public class RoomConfiguration
    {
        public string ConfigurationId;
        public RoomType RoomType;
        public int MaxOccupancy;
        public EnvironmentalSettings EnvironmentalSettings;
        public SecuritySettings SecuritySettings;
        public List<string> EquipmentRequirements = new List<string>();
        public AccessControlLevel AccessControlLevel;
        public MonitoringLevel MonitoringLevel;
        
        public RoomConfiguration()
        {
            ConfigurationId = System.Guid.NewGuid().ToString();
            EquipmentRequirements = new List<string>();
        }
    }

    /// <summary>
    /// Environmental conditions for rooms
    /// </summary>
    [System.Serializable]
    public class EnvironmentalConditions
    {
        [Header("Temperature")]
        public Vector2 TemperatureRange; // Min/Max in Celsius
        public float OptimalTemperature;
        
        [Header("Humidity")]
        public Vector2 HumidityRange; // Min/Max percentage
        public float OptimalHumidity;
        
        [Header("Air Quality")]
        public float CO2Level; // PPM
        public float AirChangesPerHour;
        public float AirFlowRate; // CFM
        
        [Header("Lighting")]
        public float LightIntensity; // PPFD
        public float PhotoperiodHours;
        public LightSpectrum LightSpectrum;
        
        [Header("Pressure")]
        public float AtmosphericPressure; // kPa
        public float DifferentialPressure; // Pa
    }

    /// <summary>
    /// Room performance tracking data
    /// </summary>
    [System.Serializable]
    public class RoomPerformanceData
    {
        public string RoomId;
        public DateTime LastUpdated;
        
        [Header("Environmental Performance")]
        public float TemperatureStability;
        public float HumidityStability;
        public float CO2Stability;
        public float LightingConsistency;
        
        [Header("Energy Efficiency")]
        public float PowerConsumption; // kWh
        public float EnergyEfficiencyRating;
        public float CostPerSquareFoot;
        
        [Header("Utilization")]
        public float AverageOccupancy;
        public float PeakOccupancy;
        public int TotalUsageHours;
        
        [Header("Maintenance")]
        public int MaintenanceEvents;
        public float MaintenanceCost;
        public float DowntimeHours;
        
        public RoomPerformanceData()
        {
            LastUpdated = DateTime.Now;
        }
    }

    // ============================================================================
    // FACILITY METRICS AND PERFORMANCE
    // ============================================================================

    [System.Serializable]
    public class FacilityMetrics
    {
        public float TotalFloorArea;
        public float UsableFloorArea;
        public float EnergyEfficiency;
        public float OperationalCost;
        public float ProductionCapacity;
        public float ComplianceScore;
        
        public FacilityMetrics()
        {
            // Initialize with default values
        }
    }

    // ============================================================================
    // GRID AND LAYOUT SETTINGS
    // ============================================================================

    [System.Serializable]
    public class GridSnapSettings
    {
        public float GridSize;
        public bool SnapToGrid;
        public bool ShowGrid;
        public Color GridColor;
        public Color MajorGridColor;
        public int MajorGridInterval;
        public Vector3 MinRoomSize;
        public Vector3 MaxRoomSize;
        public float WallThickness;
        public bool RequireFoundation;
        public bool EnforceFireSafety;
        public bool RequireVentilation;
        public float MaxBuildingHeight;
        public float MinSetbackDistance;
        public int RequiredParkingSpaces;
        public float MaxLotCoverage;
    }

    // ============================================================================
    // ENVIRONMENTAL AND SECURITY SETTINGS
    // ============================================================================

    [System.Serializable]
    public class EnvironmentalSettings
    {
        public bool AutomaticClimateControl;
        public bool CO2Supplementation;
        public bool DehumidificationEnabled;
        public bool AirCirculationEnabled;
        public float TargetTemperature;
        public float TargetHumidity;
        public float TargetCO2;
        
        public EnvironmentalSettings()
        {
            AutomaticClimateControl = true;
            CO2Supplementation = false;
            DehumidificationEnabled = true;
            AirCirculationEnabled = true;
        }
    }

    [System.Serializable]
    public class SecuritySettings
    {
        public bool AccessControlEnabled;
        public bool VideoSurveillanceEnabled;
        public bool MotionDetectionEnabled;
        public bool AlarmSystemEnabled;
        public bool BiometricAccess;
        public int MaxSimultaneousAccess;
        
        public SecuritySettings()
        {
            AccessControlEnabled = true;
            VideoSurveillanceEnabled = true;
            MotionDetectionEnabled = false;
            AlarmSystemEnabled = true;
            BiometricAccess = false;
            MaxSimultaneousAccess = 1;
        }
    }

    // ============================================================================
    // REGULATORY AND COMPLIANCE
    // ============================================================================

    /// <summary>
    /// Supporting data structures
    /// </summary>
    [System.Serializable]
    public class RegulatoryRequirement
    {
        public string RequirementType;
        public string Description;
        public bool IsMandatory;
        public ComplianceStatusType Status;
        public DateTime LastChecked;
        
        public RegulatoryRequirement()
        {
            IsMandatory = true;
            Status = ComplianceStatusType.Pending;
            LastChecked = DateTime.Now;
        }
    }

    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public ComplianceStatusType ComplianceStatus;
        
        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
            Warnings = new List<string>();
            ComplianceStatus = ComplianceStatusType.Compliant;
        }
    }

    [System.Serializable]
    public class ComplianceStatus
    {
        public bool IsCompliant;
        public ComplianceStatusType Status;
        public List<string> Violations = new List<string>();
        public List<string> Recommendations = new List<string>();
        public DateTime LastAssessment;
        public string AssessmentNotes;
        
        public ComplianceStatus()
        {
            IsCompliant = true;
            Status = ComplianceStatusType.Compliant;
            Violations = new List<string>();
            Recommendations = new List<string>();
            LastAssessment = DateTime.Now;
        }
    }

    // ============================================================================
    // FACILITY EXPANSION PLANNING
    // ============================================================================

    /// <summary>
    /// Comprehensive facility expansion planning data structure
    /// </summary>
    [System.Serializable]
    public class FacilityExpansionPlan
    {
        [Header("Expansion Identification")]
        public string ExpansionPlanId;
        public string FacilityProjectId;
        public string ExpansionName;
        public string Description;
        
        [Header("Expansion Scope")]
        public List<ConstructionRoomTemplate> NewRooms = new List<ConstructionRoomTemplate>();
        public List<string> RoomModifications = new List<string>();
        public List<string> EquipmentUpgrades = new List<string>();
        public List<string> SystemUpgrades = new List<string>();
        
        [Header("Budget and Resources")]
        public float TotalBudget;
        public float EstimatedCost;
        public float MaterialCosts;
        public float LaborCosts;
        public float PermitCosts;
        public float ContingencyFunds;
        
        [Header("Timeline")]
        public DateTime PlannedStartDate;
        public DateTime EstimatedCompletionDate;
        public int EstimatedDurationDays;
        public List<string> CriticalMilestones = new List<string>();
        
        [Header("Capacity Impact")]
        public float CurrentCapacity;
        public float AdditionalCapacity;
        public float FinalCapacity;
        public float CapacityIncrease;
        public float ROIProjection;
        
        [Header("Requirements")]
        public List<PermitType> RequiredPermits = new List<PermitType>();
        public List<MaterialRequirement> MaterialRequirements = new List<MaterialRequirement>();
        public List<WorkerSpecialty> RequiredSpecialties = new List<WorkerSpecialty>();
        
        [Header("Risk Assessment")]
        public float RiskLevel; // 0-1 scale
        public List<string> IdentifiedRisks = new List<string>();
        public List<string> MitigationStrategies = new List<string>();
        
        public FacilityExpansionPlan()
        {
            ExpansionPlanId = System.Guid.NewGuid().ToString();
            PlannedStartDate = DateTime.Now.AddDays(30);
            EstimatedCompletionDate = DateTime.Now.AddDays(90);
            EstimatedDurationDays = 60;
        }
        
        /// <summary>
        /// Calculates the financial viability of the expansion
        /// </summary>
        public bool IsFinanciallyViable()
        {
            return TotalBudget >= EstimatedCost && ROIProjection > 0;
        }
        
        /// <summary>
        /// Gets the expansion complexity level
        /// </summary>
        public ComplexityLevel GetComplexityLevel()
        {
            if (NewRooms.Count <= 2 && SystemUpgrades.Count <= 3)
                return ComplexityLevel.Simple;
            if (NewRooms.Count <= 5 && SystemUpgrades.Count <= 6)
                return ComplexityLevel.Moderate;
            if (NewRooms.Count <= 10 && SystemUpgrades.Count <= 10)
                return ComplexityLevel.Complex;
            if (NewRooms.Count <= 15)
                return ComplexityLevel.Very_Complex;
            return ComplexityLevel.Extreme;
        }
    }

    // ============================================================================
    // LAYOUT OPTIMIZATION
    // ============================================================================

    /// <summary>
    /// Layout optimization result
    /// </summary>
    [System.Serializable]
    public class LayoutOptimizationResult
    {
        public string OptimizationId;
        public string RoomId;
        public bool IsSuccessful;
        public float ImprovementPercentage;
        public string OptimizationType;
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
        public List<OptimizationRecommendation> Recommendations = new List<OptimizationRecommendation>();
        public DateTime CompletedAt;
        
        public LayoutOptimizationResult()
        {
            OptimizationId = System.Guid.NewGuid().ToString();
            CompletedAt = DateTime.Now;
            Metrics = new Dictionary<string, float>();
            Recommendations = new List<OptimizationRecommendation>();
        }
    }

    /// <summary>
    /// Facility optimization result for multiple rooms
    /// </summary>
    [System.Serializable]
    public class FacilityOptimizationResult
    {
        public string FacilityId;
        public bool IsSuccessful;
        public float OverallImprovementPercentage;
        public List<LayoutOptimizationResult> RoomOptimizations = new List<LayoutOptimizationResult>();
        public FacilityMetrics OptimizedMetrics;
        public DateTime CompletedAt;
        
        public FacilityOptimizationResult()
        {
            CompletedAt = DateTime.Now;
            RoomOptimizations = new List<LayoutOptimizationResult>();
        }
    }

    [System.Serializable]
    public class OptimizationCriteria
    {
        public bool OptimizeForEfficiency;
        public bool OptimizeForCost;
        public bool OptimizeForProduction;
        public bool OptimizeForCompliance;
        public float EfficiencyWeight;
        public float CostWeight;
        public float ProductionWeight;
        public float ComplianceWeight;
        
        public OptimizationCriteria()
        {
            OptimizeForEfficiency = true;
            OptimizeForCost = true;
            OptimizeForProduction = true;
            OptimizeForCompliance = true;
            EfficiencyWeight = 0.25f;
            CostWeight = 0.25f;
            ProductionWeight = 0.25f;
            ComplianceWeight = 0.25f;
        }
    }

    [System.Serializable]
    public class FacilityOptimizationCriteria
    {
        public OptimizationCriteria RoomCriteria;
        public bool OptimizeLayout;
        public bool OptimizeWorkflow;
        public bool OptimizeResourceSharing;
        public bool MinimizeOperationalCosts;
        
        public FacilityOptimizationCriteria()
        {
            RoomCriteria = new OptimizationCriteria();
            OptimizeLayout = true;
            OptimizeWorkflow = true;
            OptimizeResourceSharing = false;
            MinimizeOperationalCosts = true;
        }
    }

    [System.Serializable]
    public class OptimizationRecommendation
    {
        public string RecommendationType;
        public string Description;
        public float ExpectedImprovement;
        public float ImplementationCost;
        public int Priority; // 1-10 scale
        
        public OptimizationRecommendation()
        {
            Priority = 5;
        }
    }

    [System.Serializable]
    public class OptimizationOpportunity
    {
        public string OpportunityType;
        public string Description;
        public float PotentialImprovement;
        public string ImplementationGuidance;
        public int Priority;
        
        public OptimizationOpportunity()
        {
            Priority = 5;
        }
    }

    // ============================================================================
    // SUPPORTING ENUMS FROM PARENT NAMESPACE (Required for Compilation)
    // ============================================================================

    // These enums are referenced by building-related classes but defined in the parent namespace
    [System.Serializable]
    public enum ProjectStatus
    {
        Planning,
        RequiresRevision,
        PermitPending,
        ReadyToStart,
        InProgress,
        OnHold,
        Completed,
        Cancelled,
        Paused
    }

    [System.Serializable]
    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex,
        Very_Complex,
        Extreme
    }

    [System.Serializable]
    public enum PermitType
    {
        Building,
        Electrical,
        Plumbing,
        HVAC,
        Fire_Safety,
        Environmental,
        Zoning,
        Cannabis_License,
        Mechanical,
        Fire,
        Cannabis_Cultivation,
        Cannabis_Processing
    }

    [System.Serializable]
    public enum WorkerSpecialty
    {
        GeneralConstruction,
        GeneralLabor,
        Electrician,
        Electrical,
        Plumber,
        Plumbing,
        HVAC_Technician,
        HVAC,
        Carpenter,
        Mason,
        Roofer,
        Painter,
        Flooring_Specialist,
        Equipment_Installer,
        Inspector,
        Project_Manager
    }

    [System.Serializable]
    public enum BuildingMaterial
    {
        Wood,
        Steel,
        Concrete,
        Glass,
        Aluminum,
        Insulation,
        Electrical,
        Plumbing,
        HVAC,
        Specialized
    }

    // ============================================================================
    // SUPPORTING CLASSES FROM PARENT NAMESPACE (Required for Compilation)
    // ============================================================================

    [System.Serializable]
    public class MaterialRequirement
    {
        public string MaterialId;
        public string MaterialName;
        public BuildingMaterial Type;
        public float RequiredQuantity;
        public float AvailableQuantity;
        public float OrderedQuantity;
        public string Unit;
        public float UnitCost;
        public string Supplier;
        public DateTime RequiredDate;
        public DateTime OrderDate;
        public DateTime ExpectedDelivery;
        public MaterialStatus Status;
    }

    [System.Serializable]
    public enum MaterialStatus
    {
        Planning,
        Ordered,
        In_Transit,
        Delivered,
        Installed,
        Rejected
    }

} // End namespace ProjectChimera.Data.Construction.Buildings