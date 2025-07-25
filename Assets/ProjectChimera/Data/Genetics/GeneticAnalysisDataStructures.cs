using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Genetic analysis data structures for breeding and genetics systems.
    /// Provides compatibility with legacy PlantTrait references.
    /// </summary>
    
    // Legacy compatibility - PlantTrait is now TraitType
    using PlantTrait = TraitType;
    
    [System.Serializable]
    public class GeneticDiversityAnalysis
    {
        public string AnalysisID;
        public List<GenotypeDataSO> AnalyzedGenotypes = new List<GenotypeDataSO>();
        public float OverallDiversity;
        public float AllelicRichness;
        public float Heterozygosity;
        public float InbreedingCoefficient;
        public System.DateTime AnalysisDate;
        
        // Missing properties for Systems layer
        public List<TraitGap> TraitGaps { get; set; } = new List<TraitGap>();
        public int TotalStrains = 0;
        public float DiversityScore = 0f;
        
        // Additional properties for GeneticAnalysisEngine
        public int TotalIndividuals = 0;
        public Dictionary<string, float> AlleleFrequencies = new Dictionary<string, float>();
        public float HeterozygosityIndex = 0f;
    }
    
    [System.Serializable]
    public class HybridVigorOpportunity
    {
        public string OpportunityID;
        public PlantStrainSO ParentA;
        public PlantStrainSO ParentB;
        public float PredictedVigor;
        public List<TraitType> BenefitingTraits = new List<TraitType>();
        public float ConfidenceLevel;
        public string Description;
        
        // Missing properties for Systems layer
        public string ParentName1 { get; set; } = "";
        public string ParentName2 { get; set; } = "";
        public string Parent1Name { get; set; } = ""; // Changed to settable property
        public string Parent2Name { get; set; } = ""; // Changed to settable property
        public float ExpectedHeterosis { get; set; } = 0f;
        public float GeneticDistance { get; set; } = 0f;
    }
    
    [System.Serializable]
    public class GeneticCompatibilityAnalysis
    {
        public PlantStrainSO StrainA;
        public PlantStrainSO StrainB;
        public float CompatibilityScore;
        public List<TraitCompatibility> TraitCompatibilities = new List<TraitCompatibility>();
        public bool IsRecommended;
        public string Analysis;
    }
    
    [System.Serializable]
    public class TraitCompatibility
    {
        public TraitType Trait;
        public float CompatibilityScore;
        public bool ComplementaryAlleles;
        public float ExpectedOutcome;
    }
    
    [System.Serializable]
    public class BreedingPotentialAnalysis
    {
        public GenotypeDataSO Genotype;
        public List<TraitPotential> TraitPotentials = new List<TraitPotential>();
        public float OverallBreedingValue;
        public List<RecommendedMating> RecommendedMatings = new List<RecommendedMating>();
    }
    
    [System.Serializable]
    public class TraitPotential
    {
        public TraitType Trait;
        public float CurrentValue;
        public float MaxPotential;
        public float MinPotential;
        public float Heritability;
        public bool CanBeImproved;
    }
    
    [System.Serializable]
    public class RecommendedMating
    {
        public GenotypeDataSO RecommendedPartner;
        public float ExpectedImprovement;
        public List<TraitType> TargetTraits = new List<TraitType>();
        public float SuccessProbability;
    }
    
    [System.Serializable]
    public class GeneticLineageAnalysis
    {
        public GenotypeDataSO SubjectGenotype;
        public List<GenotypeDataSO> Ancestors = new List<GenotypeDataSO>();
        public List<GenotypeDataSO> Descendants = new List<GenotypeDataSO>();
        public float InbreedingLevel;
        public List<string> GeneticMarkers = new List<string>();
    }
    
    [System.Serializable]
    public class TraitHeritabilityData
    {
        public TraitType Trait;
        public float Heritability; // 0-1
        public float EnvironmentalVariance;
        public float GeneticVariance;
        public float PhenotypicVariance;
        public bool IsHighlyHeritable;
    }
    
    // Additional data structures for GeneticAnalysisEngine
    [System.Serializable]
    public class GenerationalSimulationResult
    {
        public List<PlantGenotype> FoundingPopulation = new List<PlantGenotype>();
        public float TotalGeneticGain = 0f;
        public List<float> DiversityTrends = new List<float>();
        public List<float> InbreedingTrends = new List<float>();
    }
    
    [System.Serializable]
    public class BreedingValuePrediction
    {
        public string GenotypeID;
        public Dictionary<TraitType, float> PredictedValues = new Dictionary<TraitType, float>();
        public Dictionary<TraitType, float> ReliabilityScores = new Dictionary<TraitType, float>();
        public float GenomicEstimatedBreedingValue = 0f;
        public Dictionary<TraitType, float> HeritabilityEstimates = new Dictionary<TraitType, float>();
        public Dictionary<string, float> MarkerEffects = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class PopulationAnalysisResult
    {
        public int PopulationSize;
        public System.DateTime AnalysisDate;
        public int GenerationsAnalyzed;
        public GeneticDiversityAnalysis DiversityAnalysis;
        public BottleneckDetectionResult BottleneckDetection;
        public FounderEffectAnalysis FounderEffects;
        public float EffectivePopulationSize;
        public InbreedingAnalysisResult InbreedingAnalysis;
    }
    
    [System.Serializable]
    public class BottleneckDetectionResult
    {
        public bool BottleneckDetected;
        public float BottleneckSeverity;
        public int GenerationsAffected;
    }
    
    [System.Serializable]
    public class FounderEffectAnalysis
    {
        public float FounderContribution;
        public float FounderDiversity;
        public float GenerationalDrift;
    }
    
    [System.Serializable]
    public class InbreedingAnalysisResult
    {
        public float AverageInbreedingCoefficient;
        public float InbreedingTrend;
        public int InbredIndividuals;
    }
    
    [System.Serializable]
    public class GeneticAnalysisStats
    {
        public int CachedDiversityAnalyses;
        public int CachedMutationAnalyses;
        public int CachedPopulationAnalyses;
        public bool DetailedAnalysisEnabled;
        public bool PopulationTrackingEnabled;
        public float DiversityThreshold;
        public int MaxPopulationHistory;
    }
}