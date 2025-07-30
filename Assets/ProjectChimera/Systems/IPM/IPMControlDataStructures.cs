using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    // Enums for control method types
    public enum ControlMethodType
    {
        Biological,
        Chemical,
        Cultural,
        Physical,
        Integrated
    }
    
    public enum ChemicalType
    {
        Insecticide,
        Miticide,
        Fungicide,
        Bactericide,
        Nematicide,
        Organic,
        Systemic,
        Contact,
        Fumigant
    }
    
    public enum ApplicationMethod
    {
        Spray,
        Drench,
        Granular,
        Fumigation,
        SystemicInjection,
        SoilApplication,
        FogApplication
    }
    
    public enum CulturalControlType
    {
        Sanitation,
        Quarantine,
        CropRotation,
        CompanionPlanting,
        PhysicalBarriers,
        EnvironmentalModification,
        PlantResistance,
        Timing,
        Spacing,
        Pruning
    }
    
    public enum EstablishmentStatus
    {
        Establishing,
        Established,
        Declining,
        Failed
    }
    
    // Data structures for biological control
    [Serializable]
    public class BiologicalControlAgent
    {
        public string AgentId;
        public BeneficialOrganismType OrganismType;
        public string ZoneId;
        public int InitialPopulation;
        public int CurrentPopulation;
        public List<PestType> TargetPests;
        public DateTime DeploymentTime;
        public bool IsActive;
        public EstablishmentStatus EstablishmentStatus;
        public float HuntingEfficiency;
        public float SurvivalRate;
        public float SearchRadius;
        public float EnvironmentalStress;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }
    
    // Data structures for chemical control
    [Serializable]
    public class ChemicalApplication
    {
        public string ApplicationId;
        public string ChemicalName;
        public ChemicalType ChemicalType;
        public string ZoneId;
        public float Concentration;
        public List<PestType> TargetPests;
        public ApplicationMethod ApplicationMethod;
        public DateTime ApplicationTime;
        public bool IsActive;
        public float InitialEffectiveness;
        public float CurrentEffectiveness;
        public float RemainingDuration;
        public float EnvironmentalImpact;
        public float ResistanceRisk;
        public Dictionary<string, object> ApplicationParameters = new Dictionary<string, object>();
    }
    
    // Data structures for cultural control
    [Serializable]
    public class CulturalControlMeasure
    {
        public string ControlId;
        public CulturalControlType ControlType;
        public string ZoneId;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public DateTime ImplementationTime;
        public bool IsActive;
        public float Effectiveness;
        public bool MaintenanceRequired;
        public float Duration;
        public List<PestType> AffectedPestTypes = new List<PestType>();
        public Dictionary<string, float> MaintenanceSchedule = new Dictionary<string, float>();
    }
    
    // Resistance tracking
    [Serializable]
    public class ResistanceProfile
    {
        public PestType PestType;
        public Dictionary<ChemicalType, float> ChemicalResistance = new Dictionary<ChemicalType, float>();
        public Dictionary<BeneficialOrganismType, float> BiologicalResistance = new Dictionary<BeneficialOrganismType, float>();
        public float ResistanceDevelopmentRate;
        public DateTime LastResistanceUpdate;
        public List<string> ResistanceHistory = new List<string>();
    }
    
    // Effectiveness tracking
    [Serializable]
    public class ControlMethodEffectiveness
    {
        public string ControlId;
        public ControlMethodType MethodType;
        public string ZoneId;
        public DateTime DeploymentTime;
        public float InitialEffectiveness;
        public float CurrentEffectiveness;
        public float CostEffectiveness;
        public float EnvironmentalImpact;
        public float ResistanceRisk;
        public int TotalPestsControlled;
        public bool IsActive = true;
        public List<float> EffectivenessHistory = new List<float>();
    }
    
    // Integrated strategy data structures
    [Serializable]
    public class IntegratedIPMStrategy
    {
        public string StrategyId;
        public string StrategyName;
        public List<BiologicalControlPlan> BiologicalControls = new List<BiologicalControlPlan>();
        public List<ChemicalControlPlan> ChemicalControls = new List<ChemicalControlPlan>();
        public List<CulturalControlPlan> CulturalControls = new List<CulturalControlPlan>();
        public float EstimatedCost;
        public float EstimatedEffectiveness;
        public float EnvironmentalImpactScore;
        public DateTime CreationTime;
        public string CreatedBy;
    }
    
    [Serializable]
    public class BiologicalControlPlan
    {
        public BeneficialOrganismType OrganismType;
        public int ReleaseQuantity;
        public List<PestType> TargetPests = new List<PestType>();
        public DateTime ScheduledDeployment;
        public string Notes;
    }
    
    [Serializable]
    public class ChemicalControlPlan
    {
        public string ChemicalName;
        public ChemicalType ChemicalType;
        public float Concentration;
        public List<PestType> TargetPests = new List<PestType>();
        public ApplicationMethod Method;
        public DateTime ScheduledApplication;
        public string Notes;
    }
    
    [Serializable]
    public class CulturalControlPlan
    {
        public CulturalControlType ControlType;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public DateTime ScheduledImplementation;
        public string Notes;
    }
    
    // Reporting data structures
    [Serializable]
    public class ControlEffectivenessReport
    {
        public string ZoneId;
        public DateTime ReportTime;
        public List<BiologicalControlAgent> BiologicalControls = new List<BiologicalControlAgent>();
        public List<ChemicalApplication> ChemicalApplications = new List<ChemicalApplication>();
        public List<CulturalControlMeasure> CulturalControls = new List<CulturalControlMeasure>();
        public float OverallEffectiveness;
        public float EnvironmentalImpact;
        public float CostEffectiveness;
        public float ResistanceRisk;
        public Dictionary<PestType, float> PestControlRates = new Dictionary<PestType, float>();
        public Dictionary<ControlMethodType, int> MethodCounts = new Dictionary<ControlMethodType, int>();
    }
    
    [Serializable]
    public class IPMControlSystemStatus
    {
        public int TotalActiveBiologicalAgents;
        public int TotalActiveChemicalApplications;
        public int TotalActiveCulturalControls;
        public int TotalControlsDeployed;
        public float OverallSystemEffectiveness;
        public bool BiologicalControlEnabled;
        public bool ChemicalControlEnabled;
        public bool CulturalControlEnabled;
        public DateTime LastUpdateTime;
        public Dictionary<string, int> ZoneControlCounts = new Dictionary<string, int>();
        public Dictionary<ControlMethodType, float> MethodEffectiveness = new Dictionary<ControlMethodType, float>();
    }
    
    // Treatment application data structures
    [Serializable]
    public class TreatmentApplication
    {
        public string TreatmentId;
        public string ApplicatorId;
        public Vector3 ApplicationLocation;
        public float ApplicationRadius;
        public DateTime ApplicationTime;
        public Dictionary<string, object> TreatmentParameters = new Dictionary<string, object>();
        public List<string> AffectedPestIds = new List<string>();
        public float SuccessRate;
        public string Notes;
    }
    
    // Environmental impact tracking
    [Serializable]
    public class EnvironmentalImpactData
    {
        public string ImpactId;
        public string ControlMethodId;
        public ControlMethodType MethodType;
        public float ImpactSeverity;
        public List<string> AffectedSystems = new List<string>();
        public DateTime ImpactStartTime;
        public float RecoveryTime;
        public bool IsReversible;
        public Dictionary<string, float> ImpactMetrics = new Dictionary<string, float>();
    }
    
    // Cost-benefit analysis structures
    [Serializable]
    public class CostBenefitAnalysis
    {
        public string AnalysisId;
        public string ControlMethodId;
        public float ImplementationCost;
        public float MaintenanceCost;
        public float EstimatedBenefit;
        public float ROI; // Return on Investment
        public float CostPerPestControlled;
        public TimeSpan PaybackPeriod;
        public DateTime AnalysisDate;
        public Dictionary<string, float> CostBreakdown = new Dictionary<string, float>();
        public Dictionary<string, float> BenefitBreakdown = new Dictionary<string, float>();
    }
}