using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using MutationRecord = ProjectChimera.Data.Genetics.MutationRecord;
using PhenotypeProjection = ProjectChimera.Systems.Genetics.PhenotypeProjection;
using TraitStabilityAnalysis = ProjectChimera.Systems.Genetics.TraitStabilityAnalysis;
using TraitExpressionStats = ProjectChimera.Systems.Genetics.TraitExpressionStats;
using TraitType = ProjectChimera.Data.Genetics.TraitType;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC014-2c: Base interface for all genetic services
    /// Provides common initialization and lifecycle management for genetic system components
    /// </summary>
    public interface IGeneticService
    {
        /// <summary>
        /// Whether the service is initialized and ready for use
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initialize the service with necessary dependencies
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shutdown the service and clean up resources
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// Interface for inheritance calculation service
    /// </summary>
    public interface IInheritanceCalculationService : IGeneticService
    {
        PlantGenotype GenerateFounderGenotype(PlantStrainSO strain);
        PlantGenotype CalculateOffspringGenotype(PlantGenotype parent1, PlantGenotype parent2);
        float CalculateInbreedingCoefficient(PlantGenotype genotype);
        bool CheckForMutation(float mutationRate);
        List<AlleleExpression> CalculateAlleleInteractions(PlantGenotype genotype);
        void SetEpistasisEnabled(bool enabled);
        void SetPleiotropyEnabled(bool enabled);
    }

    /// <summary>
    /// Interface for trait expression processing service
    /// </summary>
    public interface ITraitExpressionService : IGeneticService
    {
        ProjectChimera.Systems.Genetics.TraitExpressionResult CalculateTraitExpression(PlantGenotype genotype, EnvironmentalConditions environment);
        List<ProjectChimera.Systems.Genetics.TraitExpressionResult> CalculateBatchTraitExpression(List<PlantInstanceSO> plants, EnvironmentalConditions environment);
        PhenotypeProjection PredictPhenotype(PlantGenotype genotype, EnvironmentalConditions environment, int daysToPredict);
        TraitStabilityAnalysis AnalyzeTraitStability(PlantGenotype genotype, List<EnvironmentalConditions> environments);
        TraitExpressionStats GetExpressionStats();
        float CalculateHeritability(string traitName, List<PlantGenotype> population);
        Dictionary<string, float> GetPhenotypeRanges();
        void SetEnvironmentalSensitivity(float sensitivity);
    }

    /// <summary>
    /// Interface for breeding calculation service
    /// </summary>
    public interface IBreedingCalculationService : IGeneticService
    {
        BreedingResult BreedPlants(PlantGenotype parent1, PlantGenotype parent2, int numberOfOffspring);
        float CalculateBreedingValue(PlantGenotype genotype);
        BreedingRecommendation GetBreedingRecommendation(List<PlantInstanceSO> candidates, BreedingGoal goal);
        void UpdatePedigreeDatabase(string plantId, BreedingLineage lineage);
        BreedingLineage GetBreedingLineage(string plantId);
        void SetInbreedingAllowed(bool allowed);
    }

    /// <summary>
    /// Interface for genetic analysis service
    /// </summary>
    public interface IGeneticAnalysisService : IGeneticService
    {
        GeneticDiversityAnalysis AnalyzeGeneticDiversity(List<PlantInstanceSO> population);
        BreedingRecommendation OptimizeBreedingSelection(List<PlantInstanceSO> candidates, TraitSelectionCriteria criteria);
        GenerationalSimulationResult SimulateGenerations(List<PlantInstanceSO> foundingPopulation, int generations, TraitSelectionCriteria selectionCriteria);
        BreedingValuePrediction PredictBreedingValue(PlantInstanceSO plant, List<TraitType> targetTraits);
        List<MutationRecord> AnalyzeMutations(PlantGenotype genotype);
        BreedingCompatibility AnalyzeBreedingCompatibility(PlantInstanceSO plant1, PlantInstanceSO plant2);
        PopulationAnalysisResult AnalyzePopulation(List<PlantInstanceSO> population, int generationsBack);
        GeneticAnalysisStats GetAnalysisStats();
        void ClearAnalysisCaches();
        PopulationGeneticAnalysis AnalyzePopulation(List<PlantGenotype> population);
        GeneticDiversityMetrics CalculateDiversityMetrics(List<PlantGenotype> population);
        float CalculateGeneticDistance(PlantGenotype genotype1, PlantGenotype genotype2);
        List<QTLMapping> PerformQTLMapping(List<PlantGenotype> population, string traitName);
        SelectionResponse PredictSelectionResponse(List<PlantGenotype> population, SelectionCriteria criteria);
        void GenerateGeneticReport(string populationId);
    }

    /// <summary>
    /// Interface for breeding optimization service
    /// </summary>
    public interface IBreedingOptimizationService : IGeneticService
    {
        OptimalBreedingPlan GenerateBreedingPlan(List<PlantInstanceSO> candidates, BreedingObjective objective);
        float OptimizeBreedingValue(PlantGenotype genotype, BreedingGoal goal);
        List<BreedingPair> SelectOptimalPairs(List<PlantInstanceSO> candidates, int maxPairs);
        BreedingProgress TrackBreedingProgress(string breedingProgramId);
        void SetOptimizationAlgorithm(OptimizationAlgorithm algorithm);
    }
}