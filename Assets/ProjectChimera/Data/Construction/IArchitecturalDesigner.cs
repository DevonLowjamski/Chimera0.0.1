using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Construction
{
    /// <summary>
    /// Interface for architectural design capabilities
    /// </summary>
    public interface IArchitecturalDesigner
    {
        /// <summary>
        /// Create a 3D blueprint based on design parameters
        /// </summary>
        Blueprint3D CreateBlueprint(DesignParameters parameters);
        
        /// <summary>
        /// Validate a design against structural and code requirements
        /// </summary>
        DesignValidation ValidateDesign(Blueprint3D blueprint);
        
        /// <summary>
        /// Optimize layout for specified goals
        /// </summary>
        OptimizationSuggestion OptimizeLayout(Blueprint3D blueprint, DesignOptimizationGoals goals);
        
        /// <summary>
        /// Calculate construction cost for a blueprint
        /// </summary>
        CostEstimate CalculateConstructionCost(Blueprint3D blueprint);
        
        /// <summary>
        /// Generate an architectural challenge
        /// </summary>
        ArchitecturalChallenge GenerateDesignChallenge(ChallengeType type, DifficultyLevel difficulty);
        
        /// <summary>
        /// Evaluate a design solution for a challenge
        /// </summary>
        ChallengeSolution EvaluateDesignSolution(ArchitecturalChallenge challenge, Blueprint3D solution);
        
        /// <summary>
        /// Check building code compliance
        /// </summary>
        ComplianceReport CheckBuildingCodes(Blueprint3D blueprint, RegionalCodes codes);
        
        /// <summary>
        /// Perform safety analysis
        /// </summary>
        SafetyAssessment PerformSafetyAnalysis(Blueprint3D blueprint);
        
        /// <summary>
        /// Calculate green building score
        /// </summary>
        SustainabilityRating CalculateGreenBuildingScore(Blueprint3D blueprint);
    }

    // Non-duplicate supporting classes that are unique to architectural design
    [System.Serializable]
    public class DesignParameters
    {
        public string Name;
        public string Description;
        public Vector3 PlotSize;
        public List<ArchitecturalDesignConstraint> Constraints = new List<ArchitecturalDesignConstraint>();
        public ProjectType ProjectType;
        public List<RequiredSystem> RequiredSystems = new List<RequiredSystem>();
        public float BudgetLimit;
        public List<string> BuildingCodes = new List<string>();
        public List<string> DesignPatterns = new List<string>();
        public DesignOptimizationGoals OptimizationGoals;
    }
    
    [System.Serializable]
    public class DesignValidation
    {
        public bool StructuralValid;
        public bool CodeCompliant;
        public CostImpact CostImpact;
        public List<OptimizationSuggestion> Optimizations = new List<OptimizationSuggestion>();
        public List<ValidationWarning> Warnings = new List<ValidationWarning>();
    }
    
    [System.Serializable]
    public class DesignOptimizationGoals
    {
        public bool MinimizeCost;
        public bool MaximizeEfficiency;
        public bool OptimizeWorkflow;
        public bool MaximizeSustainability;
        public bool OptimizeStructural;
        public List<string> CustomGoals = new List<string>();
    }

    [System.Serializable]
    public class ArchitecturalDesignSolution
    {
        public string SolutionId;
        public string Name;
        public string Description;
        public Blueprint3D Blueprint;
        public Dictionary<string, float> Metrics = new Dictionary<string, float>();
        public List<string> Features = new List<string>();
        public float EstimatedCost;
        public float ConstructionTime;
        public bool IsValid;
        public List<string> ValidationErrors = new List<string>();
    }

    // Supporting classes unique to architectural design interface
    [System.Serializable] 
    public class OptimizationSuggestion 
    { 
        public string SuggestionId;
        public string Title;
        public string Description;
        public string Category;
        public float ImpactScore;
        public float ImplementationCost;
        public float TimeToImplement;
        public List<DesignModification> Modifications = new List<DesignModification>();
        public List<string> ExpectedBenefits = new List<string>();
    }
    
    [System.Serializable] 
    public class CostEstimate 
    { 
        public float MaterialCosts;
        public float LaborCosts;
        public float PermitCosts;
        public float EquipmentCosts;
        public float ContingencyCosts;
        public float TotalCost;
        public float Accuracy; // 0-1 scale
        public DateTime EstimateDate;
    }
    
    [System.Serializable] 
    public class ChallengeSolution 
    { 
        public string SolutionId;
        public string ChallengeId;
        public Blueprint3D Design;
        public Blueprint3D Blueprint => Design; // Added missing property alias
        public float Score;
        public bool IsComplete;
        public DateTime SubmissionTime;
        public string PlayerId;
        public List<string> Feedback = new List<string>(); // Changed to List for feedback items
        public List<string> Achievements = new List<string>(); // Added missing property
    }
    
    [System.Serializable] 
    public class ComplianceReport 
    { 
        public string ReportId;
        public bool IsCompliant;
        public List<string> PassedChecks = new List<string>();
        public List<string> FailedChecks = new List<string>();
        public List<string> Recommendations = new List<string>();
        public DateTime ReportDate;
    }
    
    [System.Serializable] 
    public class RegionalCodes 
    { 
        public string Region;
        public List<string> BuildingCodes = new List<string>();
        public List<string> SafetyCodes = new List<string>();
        public List<string> EnvironmentalCodes = new List<string>();
        public DateTime LastUpdated;
    }
    
    [System.Serializable] 
    public class SafetyAssessment 
    { 
        public string AssessmentId;
        public string BlueprintId; // Added missing property
        public float SafetyScore;
        public float OverallSafetyScore; // Added missing property
        public List<string> SafetyHazards = new List<string>();
        public List<string> SafetyFeatures = new List<string>();
        public List<SafetyViolation> SafetyViolations = new List<SafetyViolation>(); // Added missing property
        public List<SafetyRecommendation> Recommendations = new List<SafetyRecommendation>(); // Added missing property
        public SafetyComplianceLevel ComplianceLevel = SafetyComplianceLevel.Unknown; // Added missing property
        public bool MeetsStandards;
        public DateTime AssessmentDate;
    }
    
    [System.Serializable] 
    public class SustainabilityRating 
    { 
        public string RatingId;
        public string BlueprintId; // Added missing property
        public float EnergyEfficiency;
        public float WaterEfficiency;
        public float MaterialSustainability;
        public float OverallRating;
        public float OverallScore; // Added missing property
        public string CertificationLevel;
        public Dictionary<string, float> Categories = new Dictionary<string, float>(); // Added missing property
        public List<GreenCertification> Certifications = new List<GreenCertification>(); // Added missing property
        public List<SustainabilityRecommendation> Recommendations = new List<SustainabilityRecommendation>(); // Added missing property
        public DateTime RatingDate;
    }
    
    [System.Serializable] 
    public class ArchitecturalDesignConstraint 
    { 
        public string ConstraintId;
        public string Name;
        public ConstraintType Type;
        public object Value;
        public string Description;
        public bool IsHardConstraint;
        public float Priority;
    }
    
    [System.Serializable] 
    public class RequiredSystem 
    { 
        public string SystemId;
        public string SystemName;
        public string SystemType;
        public bool IsRequired;
        public string Description;
    }
    
    [System.Serializable] 
    public class CostImpact 
    { 
        public float MaterialCostChange;
        public float LaborCostChange;
        public float PermitCostChange;
        public float TotalCostChange;
        public string CostChangeReason;
    }
    
    [System.Serializable] 
    public class ValidationWarning 
    { 
        public string WarningId;
        public string Message;
        public string Severity;
        public string Category;
        public Vector3 Location;
    }
    
    [System.Serializable] 
    public class DesignModification 
    { 
        public string ModificationId;
        public string ModificationType;
        public string Description;
        public Vector3 Location;
        public float CostImpact;
        public float TimeImpact;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }

    // Additional supporting classes for Safety and Sustainability
    [System.Serializable]
    public class SafetyViolation
    {
        public string ViolationId;
        public string ViolationType;
        public string Description;
        public string Severity;
        public Vector3 Location;
        public string Requirement;
    }

    [System.Serializable]
    public class SafetyRecommendation
    {
        public string RecommendationId;
        public string Title;
        public string Description;
        public string Priority;
        public float CostEstimate;
        public string Category;
    }

    [System.Serializable]
    public class GreenCertification
    {
        public string CertificationId;
        public string CertificationName;
        public string Level;
        public float Score;
        public DateTime AwardedDate;
        public bool IsValid;
    }

    [System.Serializable]
    public class SustainabilityRecommendation
    {
        public string RecommendationId;
        public string Title;
        public string Description;
        public string Category;
        public float ImpactScore;
        public float ImplementationCost;
        public string Priority;
    }

    // Missing type definitions for compilation
    [System.Serializable]
    public class Blueprint3D
    {
        public string BlueprintId;
        public string Name;
        public string Description;
        public Vector3 Dimensions;
        public List<RoomLayout> Rooms = new List<RoomLayout>();
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public DateTime CreatedDate;
        public string CreatedBy;
    }

    [System.Serializable]
    public class RoomLayout
    {
        public string RoomId;
        public string RoomName;
        public RoomType RoomType;
        public Vector3 Position;
        public Vector3 Size;
        public List<string> Features = new List<string>();
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class ArchitecturalChallenge
    {
        public string ChallengeId;
        public string ChallengeName;
        public string Description;
        public ChallengeType ChallengeType;
        public DifficultyLevel Difficulty;
        public List<ArchitecturalDesignConstraint> Constraints = new List<ArchitecturalDesignConstraint>();
        public float TimeLimit;
        public float BudgetLimit;
        public DateTime CreatedDate;
    }

    // Enums for architectural design
    public enum ConstraintType
    {
        Budget,
        Space,
        Material,
        Environmental,
        Regulatory,
        Safety,
        Performance,
        Aesthetic
    }

    // Note: ChallengeType and SafetyComplianceLevel are defined in ConstructionDataStructures.cs
}