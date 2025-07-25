using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System;
using MutationRecord = ProjectChimera.Data.Genetics.MutationRecord;
using TraitExpressionResult = ProjectChimera.Data.Genetics.TraitExpressionResult;
using PhenotypeProjection = ProjectChimera.Systems.Genetics.PhenotypeProjection;
using TraitStabilityAnalysis = ProjectChimera.Systems.Genetics.TraitStabilityAnalysis;
using TraitExpressionStats = ProjectChimera.Systems.Genetics.TraitExpressionStats;
using TraitType = ProjectChimera.Data.Genetics.TraitType;
using BreedingStrategyType = ProjectChimera.Data.Genetics.BreedingStrategyType;
using BreedingRecommendation = ProjectChimera.Data.AI.BreedingRecommendation;
using BreedingPair = ProjectChimera.Systems.Genetics.BreedingPair;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC014-2c: Additional data structures needed for genetic service interfaces
    /// These complement existing data structures without duplicating them
    /// </summary>
    
    [Serializable]
    public class AlleleExpression
    {
        public string GeneLocus;
        public string AlleleId;
        public float ExpressionLevel;
        public List<string> InteractingLoci;
        public float EpistaticEffect;
    }

    [Serializable]
    public class AlleleData
    {
        public string AlleleId;
        public string GeneLocus;
        public float EffectValue;
        public float Dominance;
        public bool IsWildType;
        public string OriginStrain;
        public DateTime FirstObserved;
    }

    [Serializable]
    public class GeneticDiversityAnalysis
    {
        public int PopulationSize;
        public int TotalIndividuals;
        public int TotalStrains;
        public float DiversityIndex;
        public float DiversityScore;
        public float OverallDiversity;
        public float AllelicRichness;
        public float Heterozygosity;
        public float HeterozygosityIndex;
        public float InbreedingCoefficient;
        public float AlleleFrequency;
        public int RareAlleles;
        public DateTime AnalysisDate;
        public List<TraitGap> TraitGaps;
        public Dictionary<string, float> AlleleFrequencies;
    }

    [Serializable]
    public class GenerationalSimulationResult
    {
        public int GenerationsSimulated;
        public int FinalPopulationSize;
        public float GeneticGainAchieved;
        public List<GenerationSnapshot> GenerationHistory;
        public DateTime SimulationDate;
        public List<PlantInstanceSO> FoundingPopulation;
        public float TotalGeneticGain;
        public List<float> DiversityTrends;
        public List<float> InbreedingTrends;
    }

    [Serializable]
    public class GenerationSnapshot
    {
        public int Generation;
        public int PopulationSize;
        public float AverageFitness;
        public float GeneticDiversity;
    }

    [Serializable]
    public class BreedingValuePrediction
    {
        public string PlantId;
        public string GenotypeId;
        public string GenotypeID { get => GenotypeId; set => GenotypeId = value; } // Alias for backward compatibility
        public float PredictedValue;
        public Dictionary<string, float> PredictedValues;
        public Dictionary<TraitType, float> PredictedValuesByTrait;
        public Dictionary<string, float> TraitContributions;
        public Dictionary<TraitType, float> ReliabilityScores;
        public float Reliability;
        public float ReliabilityScore;
        public float GenomicEstimatedBreedingValue;
        public DateTime PredictionDate;
        public Dictionary<TraitType, float> HeritabilityEstimates;
        public Dictionary<string, float> MarkerEffects;
    }


    [Serializable]
    public class BreedingCompatibility
    {
        public string Plant1Id;
        public string Plant2Id;
        public float CompatibilityScore;
        public List<string> CompatibilityFactors;
        public float InbreedingRisk;
        public string Parent1ID;
        public string Parent2ID;
        public float GeneticDistance;
        public float ExpectedHeterosis;
        public float ComplementarityScore;
        public float PredictedHeterosis;
    }

    [Serializable]
    public class PopulationAnalysisResult
    {
        public int PopulationSize;
        public float OverallFitness;
        public float GeneticDiversity;
        public float DiversityAnalysis;
        public float BottleneckDetection;
        public float FounderEffects;
        public float EffectivePopulationSize;
        public float InbreedingAnalysis;
        public List<string> PopulationIssues;
        public DateTime AnalysisDate;
        public DateTime GenerationAnalyzed;
    }

    [Serializable]
    public class GeneticAnalysisStats
    {
        public int TotalAnalysesPerformed;
        public float AverageAnalysisTime;
        public int CachedResults;
        public DateTime LastUpdate;
        
        // Missing properties that were referenced in errors
        public int CachedDiversityAnalyses;
        public int CachedMutationAnalyses;
        public int CachedPopulationAnalyses;
        public bool DetailedAnalysisEnabled;
        public bool PopulationTrackingEnabled;
        public float DiversityThreshold;
    }

    [Serializable]
    public class OptimalBreedingPlan
    {
        public List<BreedingPair> Phase1Crosses;
        public List<BreedingPair> Phase2Crosses;
        public List<BreedingPair> Phase3Crosses;
        public float ExpectedGeneticGain;
        public int EstimatedTimeToCompletion;
        public List<string> CriticalDecisionPoints;
    }

    [Serializable]
    public class PopulationGeneticAnalysis
    {
        public int PopulationSize;
        public float AlleleFrequency;
        public float HeterozygosityObserved;
        public float HeterozygosityExpected;
        public float InbreedingCoefficient;
        public List<string> RareAlleles;
        public float GeneticDiversity;
    }

    [Serializable]
    public class GeneticDiversityMetrics
    {
        public float ShannonIndex;
        public float SimpsonIndex;
        public int NumberOfAlleles;
        public float EffectivePopulationSize;
        public float NucleotideDiversity;
    }

    [Serializable]
    public class QTLMapping
    {
        public string ChromosomeLocation;
        public string TraitName;
        public float EffectSize;
        public float SignificanceLevel;
        public float VarianceExplained;
    }

    [Serializable]
    public class SelectionCriteria
    {
        public Dictionary<string, float> TraitWeights;
        public float SelectionIntensity;
        public int NumberOfGenerations;
        public bool EnableCorrelatedResponse;
    }

    [Serializable]
    public class SelectionResponse
    {
        public string TraitName;
        public float ResponsePerGeneration;
        public float CumulativeResponse;
        public float Heritability;
        public float SelectionIntensity;
    }

    [Serializable]
    public class BreedingObjective
    {
        public BreedingGoal PrimaryGoal;
        public List<BreedingGoal> SecondaryGoals;
        public Dictionary<string, float> TraitPriorities;
        public float TimeHorizon;
        public float RiskTolerance;
        public BreedingStrategyType? PreferredStrategy;
    }

    [Serializable]
    public class BreedingProgress
    {
        public string ProgramId;
        public int CurrentGeneration;
        public float GeneticGainAchieved;
        public float InbreedingLevel;
        public List<string> Milestones;
        public DateTime LastUpdate;
        public int TotalAttempts;
        public int SuccessfulAttempts;
        public float SuccessRate;
        public Dictionary<TraitType, float> TraitProgress;
        public float OverallProgress;
        public DateTime LastUpdated;
    }

    [Serializable]
    public class BreedingGoalConfiguration
    {
        public string GoalName;
        public string Description;
        public List<TraitType> TargetTraits;
        public float Priority;
        public DateTime Deadline;
    }

    [Serializable]
    public class BreedingGoal
    {
        public string GoalId;
        public string GoalName;
        public string Description;
        public List<TraitType> TargetTraits;
        public float Priority;
        public DateTime Deadline;
        public DateTime CreatedAt;
        public DateTime? CompletedAt;
        public BreedingGoalStatus Status;
        
        // Static instances for common goals
        public static BreedingGoal MaximizeYield = new BreedingGoal 
        { 
            GoalId = "yield", 
            GoalName = "Maximize Yield", 
            TargetTraits = new List<TraitType> { TraitType.Yield },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal MaximizeTHC = new BreedingGoal 
        { 
            GoalId = "thc", 
            GoalName = "Maximize THC", 
            TargetTraits = new List<TraitType> { TraitType.THCContent },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal MaximizeCBD = new BreedingGoal 
        { 
            GoalId = "cbd", 
            GoalName = "Maximize CBD", 
            TargetTraits = new List<TraitType> { TraitType.CBDContent },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal OptimizeTerpenes = new BreedingGoal 
        { 
            GoalId = "terpenes", 
            GoalName = "Optimize Terpenes", 
            TargetTraits = new List<TraitType> { TraitType.TerpeneProduction },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal ImproveResistance = new BreedingGoal 
        { 
            GoalId = "resistance", 
            GoalName = "Improve Resistance", 
            TargetTraits = new List<TraitType> { TraitType.DiseaseResistance },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal ReduceFloweringTime = new BreedingGoal 
        { 
            GoalId = "flowering", 
            GoalName = "Reduce Flowering Time", 
            TargetTraits = new List<TraitType> { TraitType.FloweringTime },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
        public static BreedingGoal BalancedImprovement = new BreedingGoal 
        { 
            GoalId = "balanced", 
            GoalName = "Balanced Improvement", 
            TargetTraits = new List<TraitType> { TraitType.Yield, TraitType.THCContent, TraitType.CBDContent },
            Status = BreedingGoalStatus.Active,
            Priority = 1.0f,
            CreatedAt = DateTime.Now
        };
    }


    [Serializable]
    public enum OptimizationAlgorithm
    {
        GeneticAlgorithm,
        SimulatedAnnealing,
        ParticleSwarm,
        DifferentialEvolution,
        Bayesian
    }

    [Serializable]
    public enum BreedingGoalStatus
    {
        Active,
        Completed,
        Paused,
        Cancelled,
        InProgress,
        Planning
    }

    [Serializable]
    public class BreedingGoalTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public string Description;
        public List<TraitType> DefaultTraits;
        public List<TargetTrait> DefaultTargetTraits;
        public float DefaultPriority;
        public int EstimatedDuration;
        
        public BreedingGoalConfiguration CreateConfiguration(int maxGoals)
        {
            return new BreedingGoalConfiguration
            {
                GoalName = TemplateName,
                Description = Description,
                TargetTraits = DefaultTraits ?? new List<TraitType>(),
                Priority = DefaultPriority,
                Deadline = DateTime.Now.AddDays(EstimatedDuration)
            };
        }
    }

    [Serializable]
    public class BreedingAttempt
    {
        public string AttemptId;
        public string Parent1Id;
        public string Parent2Id;
        public DateTime AttemptDate;
        public DateTime AttemptTime;
        public bool WasSuccessful;
        public bool Success => WasSuccessful;
        public List<string> OffspringIds;
        public Dictionary<TraitType, float> OffspringTraits;
        public float QualityScore;
    }

    [Serializable]
    public class TargetTrait
    {
        public TraitType Trait;
        public float TargetValue;
        public float Weight = 1.0f;
    }

    [Serializable]
    public class TraitGap
    {
        public string TraitName;
        public float CurrentRange;
        public float OptimalRange;
        public float Priority;
    }

    [Serializable]
    public class BreedingGoalSuggestion
    {
        public string SuggestionId;
        public string GoalName;
        public string Description;
        public int Priority;
        public int EstimatedGenerations;
        public float SuccessProbability;
    }

    [Serializable]
    public class HybridVigorOpportunity
    {
        public string Parent1Name;
        public string Parent2Name;
        public float GeneticDistance;
        public float ExpectedHeterosis;
    }

    [Serializable]
    public enum BreedingPriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Serializable]
    public enum BreedingStrategyType
    {
        SingleTrait,
        MultiTraitBalanced,
        HybridVigor
    }

    [Serializable]
    public class BreedingStrategyData
    {
        public string StrategyId;
        public string GoalId;
        public string StrategyName;
        public BreedingStrategyType StrategyType;
        public List<RecommendedCross> RecommendedCrosses;
        public int GenerationsPlanned;
        public float MutationRate;
        public float SelectionPressure;
        public DateTime CreatedAt;
        public DateTime LastOptimized;
        public int OptimizationCount;
    }

    [Serializable]
    public class RecommendedCross
    {
        public string CrossName;
        public float EstimatedSuccessRate;
        public int GenerationsRequired;
        public string Notes;
    }

    // Additional data structures for genetics analysis
    [Serializable]
    public class BottleneckDetectionResult
    {
        public bool BottleneckDetected;
        public float BottleneckSeverity;
        public int GenerationsAffected;
    }

    [Serializable]
    public class FounderEffectAnalysis
    {
        public float FounderContribution;
        public float FounderDiversity;
        public float GenerationalDrift;
    }

    [Serializable]
    public class InbreedingAnalysisResult
    {
        public float AverageInbreedingCoefficient;
        public float InbreedingTrend;
        public int InbredIndividuals;
    }
}