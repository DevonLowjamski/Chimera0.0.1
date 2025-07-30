using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    // Enums for decision support system
    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }
    
    public enum DecisionOutcome
    {
        Successful,
        PartiallySuccessful,
        Failed,
        Pending,
        Cancelled
    }
    
    public enum AnalysisType
    {
        ThreatAssessment,
        CostBenefit,
        RiskAnalysis,
        EffectivenessPreduction,
        EnvironmentalImpact,
        PlayerPreference
    }
    
    // Core threat assessment structures
    [Serializable]
    public class ThreatAssessment
    {
        public string AssessmentId;
        public string ZoneId;
        public DateTime AssessmentTime;
        public RiskLevel ThreatLevel;
        public List<PestThreatAnalysis> PestThreats = new List<PestThreatAnalysis>();
        public float EnvironmentalRisk;
        public float EconomicImpact;
        public float UrgencyScore;
        public List<string> RecommendedActions = new List<string>();
        public Dictionary<string, float> RiskFactors = new Dictionary<string, float>();
        public float OverallConfidence;
        public DateTime ExpiryTime;
    }
    
    [Serializable]
    public class PestThreatAnalysis
    {
        public PestType PestType;
        public int PopulationSize;
        public float GrowthRate;
        public float ThreatScore;
        public OutbreakSeverity SeverityLevel;
        public float ReproductionPotential;
        public float EnvironmentalSuitability;
        public Dictionary<string, float> ResistanceLevels = new Dictionary<string, float>();
        public float DamageProjection;
        public float SpreadRisk;
        public List<string> VulnerablePlants = new List<string>();
    }
    
    // Strategy recommendation structures
    [Serializable]
    public class StrategyRecommendation
    {
        public string RecommendationId;
        public string ZoneId;
        public StrategyType StrategyType;
        public RecommendationPriority Priority;
        public float ConfidenceLevel;
        public float EstimatedEffectiveness;
        public float EstimatedCost;
        public float EnvironmentalImpact;
        public float TimeToEffect;
        public string Description;
        public string DetailedPlan;
        public List<string> RiskFactors = new List<string>();
        public List<string> Prerequisites = new List<string>();
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public DateTime RecommendedStartTime;
        public float PlayerCompatibilityScore;
        public List<string> AlternativeOptions = new List<string>();
    }
    
    // Decision tracking structures
    [Serializable]
    public class DecisionHistory
    {
        public string DecisionId;
        public string PlayerId;
        public string RecommendationId;
        public string ZoneId;
        public StrategyType StrategyType;
        public DateTime DecisionTime;
        public bool WasRecommended;
        public bool ImplementationSuccess;
        public float ActualEffectiveness;
        public float ActualCost;
        public float PlayerSatisfaction;
        public DecisionOutcome Outcome;
        public string Notes;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
        public TimeSpan TimeToImplement;
        public List<string> LessonsLearned = new List<string>();
    }
    
    // Player preference and learning structures
    [Serializable]
    public class PlayerPreferenceProfile
    {
        public string PlayerId;
        public Dictionary<StrategyType, float> StrategyPreferences = new Dictionary<StrategyType, float>();
        public bool PreferOrganicOnly;
        public float CostSensitivity;
        public float EnvironmentalConcern;
        public float RiskTolerance;
        public float SpeedPreference; // Prefers quick vs. long-term solutions
        public DateTime ProfileCreated;
        public DateTime LastUpdated;
        public int TotalDecisions;
        public float AverageDecisionTime;
        public Dictionary<string, float> CustomPreferences = new Dictionary<string, float>();
    }
    
    // AI and analysis engine structures
    [Serializable]
    public class DecisionAnalyzer
    {
        public string AnalyzerId;
        public Dictionary<AnalysisType, float> AnalysisWeights = new Dictionary<AnalysisType, float>();
        public float AnalysisAccuracy;
        public int TotalAnalyses;
        public DateTime LastUpdate;
        
        public AnalysisResult AnalyzeDecision(StrategyRecommendation recommendation)
        {
            // Placeholder implementation
            return new AnalysisResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                Recommendation = recommendation,
                AnalysisScore = 0.75f,
                ConfidenceLevel = 0.8f,
                RiskAssessment = 0.3f,
                Factors = new Dictionary<string, float> { { "effectiveness", 0.8f }, { "cost", 0.7f } }
            };
        }
    }
    
    [Serializable]
    public class RiskAssessmentEngine
    {
        public string EngineId;
        public Dictionary<RiskLevel, float> RiskThresholds = new Dictionary<RiskLevel, float>();
        public float PredictionAccuracy;
        public DateTime LastCalibration;
        
        public RiskAssessmentResult AssessRisk(ThreatAssessment threat)
        {
            // Placeholder implementation
            return new RiskAssessmentResult
            {
                AssessmentId = Guid.NewGuid().ToString(),
                OverallRisk = threat.ThreatLevel,
                RiskScore = threat.UrgencyScore,
                MitigationRecommendations = new List<string> { "Immediate monitoring", "Prepare interventions" },
                TimeWindow = TimeSpan.FromHours(24)
            };
        }
    }
    
    [Serializable]
    public class StrategyOptimizer
    {
        public string OptimizerId;
        public Dictionary<string, float> ObjectiveWeights = new Dictionary<string, float>();
        public float OptimizationEfficiency;
        public int OptimizationCycles;
        
        public OptimizationResult OptimizeStrategies(List<StrategyRecommendation> strategies)
        {
            // Placeholder implementation
            return new OptimizationResult
            {
                OptimizationId = Guid.NewGuid().ToString(),
                OptimizedStrategies = strategies,
                ImprovementScore = 0.15f,
                ResourceSavings = 0.2f,
                EffectivenessGain = 0.1f
            };
        }
    }
    
    [Serializable]
    public class LearningEngine
    {
        public string EngineId;
        public float LearningRate;
        public Dictionary<string, float> FeatureWeights = new Dictionary<string, float>();
        public int TrainingIterations;
        public float ModelAccuracy;
        
        public void UpdateEffectivenessData(StrategyRecommendation recommendation, float actualEffectiveness)
        {
            // Placeholder implementation for machine learning updates
            var error = Mathf.Abs(recommendation.EstimatedEffectiveness - actualEffectiveness);
            // Update model weights based on error
        }
        
        public PredictionResult PredictOutcome(StrategyRecommendation recommendation)
        {
            // Placeholder implementation
            return new PredictionResult
            {
                PredictionId = Guid.NewGuid().ToString(),
                PredictedEffectiveness = recommendation.EstimatedEffectiveness * 0.95f,
                PredictedCost = recommendation.EstimatedCost * 1.05f,
                Confidence = 0.8f,
                VarianceEstimate = 0.1f
            };
        }
    }
    
    // Analysis result structures
    [Serializable]
    public class AnalysisResult
    {
        public string AnalysisId;
        public StrategyRecommendation Recommendation;
        public float AnalysisScore;
        public float ConfidenceLevel;
        public float RiskAssessment;
        public Dictionary<string, float> Factors = new Dictionary<string, float>();
        public DateTime AnalysisTime;
        public string AnalysisMethod;
        public List<string> Insights = new List<string>();
    }
    
    [Serializable]
    public class RiskAssessmentResult
    {
        public string AssessmentId;
        public RiskLevel OverallRisk;
        public float RiskScore;
        public List<string> MitigationRecommendations = new List<string>();
        public TimeSpan TimeWindow;
        public Dictionary<string, float> RiskBreakdown = new Dictionary<string, float>();
        public float UncertaintyLevel;
        public DateTime ValidUntil;
    }
    
    [Serializable]
    public class OptimizationResult
    {
        public string OptimizationId;
        public List<StrategyRecommendation> OptimizedStrategies = new List<StrategyRecommendation>();
        public float ImprovementScore;
        public float ResourceSavings;
        public float EffectivenessGain;
        public Dictionary<string, float> OptimizationMetrics = new Dictionary<string, float>();
        public DateTime OptimizationTime;
        public string OptimizationMethod;
    }
    
    [Serializable]
    public class PredictionResult
    {
        public string PredictionId;
        public float PredictedEffectiveness;
        public float PredictedCost;
        public float Confidence;
        public float VarianceEstimate;
        public Dictionary<string, float> FeatureImportance = new Dictionary<string, float>();
        public DateTime PredictionTime;
        public string ModelVersion;
    }
    
    // Reporting structures
    [Serializable]
    public class DecisionSupportReport
    {
        public string ZoneId;
        public DateTime ReportTime;
        public RiskLevel CurrentThreatLevel;
        public List<StrategyRecommendation> ActiveRecommendations = new List<StrategyRecommendation>();
        public float SystemConfidence;
        public List<DecisionHistory> RecentDecisionHistory = new List<DecisionHistory>();
        public Dictionary<StrategyType, int> RecommendationCounts = new Dictionary<StrategyType, int>();
        public Dictionary<string, float> ZoneMetrics = new Dictionary<string, float>();
        public List<string> KeyInsights = new List<string>();
        public string ExecutiveSummary;
    }
    
    [Serializable]
    public class DecisionSupportMetrics
    {
        public int TotalRecommendationsGenerated;
        public float AverageRecommendationAccuracy;
        public int ActiveThreats;
        public int ActiveRecommendations;
        public Dictionary<StrategyType, float> StrategySuccessRates = new Dictionary<StrategyType, float>();
        public int TotalDecisions;
        public float RecentDecisionSuccessRate;
        public DateTime LastUpdateTime;
        public float SystemEfficiency;
        public float PlayerSatisfactionScore;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }
    
    // Chemical data structure for recommendations
    [Serializable]
    public class ChemicalData
    {
        public string ChemicalId;
        public string ChemicalName;
        public ChemicalType ChemicalType;
        public List<PestType> TargetPests = new List<PestType>();
        public float BaseEffectiveness;
        public float EnvironmentalImpact;
        public float Cost;
        public bool IsOrganic;
        public float ResistanceRisk;
        public TimeSpan ActiveDuration;
        public List<string> ApplicationMethods = new List<string>();
        public Dictionary<string, float> EffectivenessModifiers = new Dictionary<string, float>();
    }
    
    // Integrated strategy structure
    [Serializable]
    public class IntegratedStrategy
    {
        public string StrategyId;
        public string StrategyName;
        public List<StrategyComponent> Components = new List<StrategyComponent>();
        public Dictionary<string, float> ExpectedOutcomes = new Dictionary<string, float>();
        public float TotalCost;
        public float TotalEffectiveness;
        public float TotalEnvironmentalImpact;
        public TimeSpan ImplementationTime;
        public List<string> CriticalSuccessFactors = new List<string>();
        public Dictionary<string, string> ComponentInteractions = new Dictionary<string, string>();
    }
    
    [Serializable]
    public class StrategyComponent
    {
        public string ComponentId;
        public StrategyType ComponentType;
        public string Description;
        public float Weight;
        public float ExpectedContribution;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public List<string> Dependencies = new List<string>();
        public TimeSpan Timing;
    }
    
    // Note: CostBenefitAnalysis is defined in IPMControlDataStructures.cs
    
    // Scenario analysis structures
    [Serializable]
    public class ScenarioAnalysis
    {
        public string ScenarioId;
        public string ScenarioName;
        public Dictionary<string, object> ScenarioParameters = new Dictionary<string, object>();
        public List<StrategyRecommendation> OptimalStrategies = new List<StrategyRecommendation>();
        public float ExpectedOutcome;
        public float WorstCaseOutcome;
        public float BestCaseOutcome;
        public float Probability;
        public Dictionary<string, float> SensitivityFactors = new Dictionary<string, float>();
        public DateTime CreationTime;
        public string CreatedBy;
    }
    
    // Real-time monitoring structures  
    [Serializable]
    public class DecisionSupportMonitor
    {
        public string MonitorId;
        public List<string> MonitoredZones = new List<string>();
        public Dictionary<string, float> AlertThresholds = new Dictionary<string, float>();
        public List<AlertCondition> ActiveAlerts = new List<AlertCondition>();
        public float MonitoringFrequency;
        public DateTime LastUpdate;
        public bool IsActive;
        public Dictionary<string, float> MonitoringMetrics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class AlertCondition
    {
        public string AlertId;
        public string ZoneId;
        public string AlertType;
        public float Severity;
        public string Description;
        public DateTime TriggeredTime;
        public bool IsResolved;
        public List<string> RecommendedActions = new List<string>();
        public Dictionary<string, object> AlertData = new Dictionary<string, object>();
    }
}