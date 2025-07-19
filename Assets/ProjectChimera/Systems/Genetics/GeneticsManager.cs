using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Central manager for cannabis genetics including inheritance calculations, breeding mechanics,
    /// trait expression, and genetic algorithms. Implements scientifically accurate cannabis genetics
    /// based on research including epistasis, pleiotropy, and QTL effects.
    /// </summary>
    public class GeneticsManager : ChimeraManager
    {
        [Header("Genetics Configuration")]
        [SerializeField] private bool _enableEpistasis = true;
        [SerializeField] private bool _enablePleiotropy = true;
        [SerializeField] private bool _enableMutations = true;
        [SerializeField] private float _mutationRate = 0.001f; // 0.1% chance per allele
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Breeding Settings")]
        [SerializeField] private int _maxGenerationsTracked = 10;
        [SerializeField] private bool _allowInbreeding = true;
        [SerializeField] private float _inbreedingDepression = 0.1f; // 10% fitness reduction per inbreeding coefficient unit
        [SerializeField] private bool _trackPedigrees = true;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<BreedingResult> _onBreedingCompleted;
        [SerializeField] private GameEventSO<GeneticMutation> _onMutationOccurred;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onTraitExpressionCalculated;
        
        // Private fields
        private Dictionary<string, PlantGenotype> _genotypeCache = new Dictionary<string, PlantGenotype>();
        private Dictionary<string, BreedingLineage> _pedigreeDatabase = new Dictionary<string, BreedingLineage>();
        // Core genetic system components
        private InheritanceCalculator _inheritanceCalculator;
        private ProjectChimera.Systems.Genetics.TraitExpressionEngine _traitExpressionEngine;
        private BreedingCalculationEngine _breedingCalculationEngine;
        private GeneticAlgorithms _geneticAlgorithms;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public bool EnableEpistasis => _enableEpistasis;
        public bool EnablePleiotropy => _enablePleiotropy;
        public bool EnableMutations => _enableMutations;
        public float MutationRate => _mutationRate;
        public int TrackedGenotypes => _genotypeCache.Count;
        
        protected override void OnManagerInitialize()
        {
            _inheritanceCalculator = new InheritanceCalculator(_enableEpistasis, _enablePleiotropy);
            _traitExpressionEngine = new ProjectChimera.Systems.Genetics.TraitExpressionEngine(_enableEpistasis, _enablePleiotropy);
            _breedingCalculationEngine = new BreedingCalculationEngine(
                _allowInbreeding, _inbreedingDepression, _enableMutations, 
                _mutationRate, _trackPedigrees, _maxGenerationsTracked);
            _geneticAlgorithms = new GeneticAlgorithms();
            
            // Subscribe to breeding events
            _breedingCalculationEngine.OnBreedingCompleted += HandleBreedingCompleted;
            _breedingCalculationEngine.OnMutationOccurred += HandleMutationOccurred;
            
            LogInfo($"GeneticsManager initialized with epistasis: {_enableEpistasis}, pleiotropy: {_enablePleiotropy}");
        }
        
        protected override void OnManagerUpdate()
        {
            // Genetics manager typically doesn't need per-frame updates
            // Most genetic calculations are event-driven
        }
        
        /// <summary>
        /// Generates a genotype for a plant from a strain definition.
        /// </summary>
        public PlantGenotype GenerateGenotypeFromStrain(PlantStrainSO strain)
        {
            if (strain == null)
            {
                LogError("Cannot generate genotype: strain is null");
                return null;
            }
            
            var genotype = _inheritanceCalculator.GenerateFounderGenotype(strain);
            
            // Cache the genotype
            if (!string.IsNullOrEmpty(genotype.GenotypeID))
            {
                _genotypeCache[genotype.GenotypeID] = genotype;
            }
            
            if (_enableDetailedLogging)
                LogInfo($"Generated genotype {genotype.GenotypeID} from strain {strain.StrainName}");
            
            return genotype;
        }
        
        /// <summary>
        /// Performs breeding between two parent plants and returns offspring genetics.
        /// </summary>
        public BreedingResult BreedPlants(PlantInstanceSO parent1, PlantInstanceSO parent2, int numberOfOffspring = 1)
        {
            if (parent1 == null || parent2 == null)
            {
                LogError("Cannot breed plants: one or both parents are null");
                return null;
            }
            
            // Get parent genotypes
            var parent1Genotype = GetOrGenerateGenotype(parent1);
            var parent2Genotype = GetOrGenerateGenotype(parent2);
            
            if (parent1Genotype == null || parent2Genotype == null)
            {
                LogError("Cannot breed plants: failed to obtain parent genotypes");
                return null;
            }
            
            // Use extracted breeding calculation engine
            var breedingResult = _breedingCalculationEngine.BreedPlants(
                parent1Genotype, 
                parent2Genotype, 
                numberOfOffspring
            );
            
            // Cache offspring genotypes
            if (breedingResult?.OffspringGenotypes != null)
            {
                foreach (var offspring in breedingResult.OffspringGenotypes)
                {
                    _genotypeCache[offspring.GenotypeID] = offspring;
                }
            }
            
            LogInfo($"Breeding completed: {parent1.PlantName} x {parent2.PlantName} -> {breedingResult?.OffspringGenotypes?.Count ?? 0} offspring");
            
            return breedingResult;
        }
        
        /// <summary>
        /// Calculates trait expression for a plant based on its genotype and environment.
        /// </summary>
        public TraitExpressionResult CalculateTraitExpression(PlantInstanceSO plant, EnvironmentalConditions environment)
        {
            if (plant == null)
            {
                LogError("Cannot calculate trait expression: plant is null");
                return null;
            }
            
            var genotype = GetOrGenerateGenotype(plant);
            if (genotype == null)
            {
                LogError($"Cannot calculate trait expression: no genotype for plant {plant.PlantID}");
                return null;
            }
            
            var expressionResult = _traitExpressionEngine.CalculateExpression(genotype, environment);
            
            if (_enableDetailedLogging)
                LogInfo($"Calculated trait expression for plant {plant.PlantID}");
            
            _onTraitExpressionCalculated?.Raise(plant);
            
            return expressionResult;
        }
        
        /// <summary>
        /// Analyzes genetic diversity within a population of plants.
        /// </summary>
        public GeneticDiversityAnalysis AnalyzeGeneticDiversity(List<PlantInstanceSO> population)
        {
            if (population == null || population.Count == 0)
            {
                LogWarning("Cannot analyze genetic diversity: empty population");
                return null;
            }
            
            var genotypes = new List<PlantGenotype>();
            foreach (var plant in population)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                    genotypes.Add(genotype);
            }
            
            return _geneticAlgorithms.AnalyzeDiversity(genotypes);
        }
        
        /// <summary>
        /// Optimizes breeding selections using genetic algorithms.
        /// </summary>
        public BreedingRecommendation OptimizeBreedingSelection(List<PlantInstanceSO> candidates, TraitSelectionCriteria criteria)
        {
            if (candidates == null || candidates.Count < 2)
            {
                LogWarning("Cannot optimize breeding selection: insufficient candidates");
                return null;
            }
            
            var genotypes = new List<PlantGenotype>();
            foreach (var candidate in candidates)
            {
                var genotype = GetOrGenerateGenotype(candidate);
                if (genotype != null)
                    genotypes.Add(genotype);
            }
            
            // Convert criteria to breeding goal for new engine
            var breedingGoal = ConvertCriteriaToBreedingGoal(criteria);
            var optimalPairs = _breedingCalculationEngine.OptimizeBreedingPairs(genotypes, breedingGoal);
            
            return ConvertOptimalPairsToRecommendation(optimalPairs);
        }
        
        /// <summary>
        /// Simulates multiple generations of breeding with selection pressure.
        /// </summary>
        public GenerationalSimulationResult SimulateGenerations(List<PlantInstanceSO> foundingPopulation, 
            int generations, TraitSelectionCriteria selectionCriteria)
        {
            var foundingGenotypes = new List<PlantGenotype>();
            foreach (var plant in foundingPopulation)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                    foundingGenotypes.Add(genotype);
            }
            
            // Convert criteria to breeding goal for new engine
            var breedingGoal = ConvertCriteriaToBreedingGoal(selectionCriteria);
            
            return _breedingCalculationEngine.SimulateGenerations(foundingGenotypes, generations, breedingGoal);
        }
        
        /// <summary>
        /// Gets breeding value prediction for a plant based on genetic markers.
        /// </summary>
        public BreedingValuePrediction PredictBreedingValue(PlantInstanceSO plant, List<TraitType> targetTraits)
        {
            var genotype = GetOrGenerateGenotype(plant);
            if (genotype == null)
                return null;
            
            return _breedingCalculationEngine.PredictBreedingValue(genotype, targetTraits);
        }
        
        /// <summary>
        /// Identifies potential genetic mutations and their effects.
        /// </summary>
        public List<MutationRecord> AnalyzeMutations(PlantGenotype genotype)
        {
            if (genotype == null)
                return new List<MutationRecord>();
            
            return _inheritanceCalculator.IdentifyMutations(genotype);
        }
        
        /// <summary>
        /// Gets genetic compatibility between two potential breeding partners.
        /// </summary>
        public BreedingCompatibility AnalyzeBreedingCompatibility(PlantInstanceSO plant1, PlantInstanceSO plant2)
        {
            var genotype1 = GetOrGenerateGenotype(plant1);
            var genotype2 = GetOrGenerateGenotype(plant2);
            
            if (genotype1 == null || genotype2 == null)
                return null;
            
            return _breedingCalculationEngine.AnalyzeBreedingCompatibility(genotype1, genotype2);
        }
        
        /// <summary>
        /// Gets or generates a genotype for a plant instance.
        /// </summary>
        private PlantGenotype GetOrGenerateGenotype(PlantInstanceSO plant)
        {
            if (plant == null || plant.Strain == null)
                return null;
            
            // Check cache first
            if (_genotypeCache.TryGetValue(plant.PlantID, out var cachedGenotype))
                return cachedGenotype;
            
            // Generate new genotype from strain
            var genotype = GenerateGenotypeFromStrain(plant.Strain);
            if (genotype != null)
            {
                genotype.GenotypeID = plant.PlantID; // Use plant ID as genotype ID
                _genotypeCache[plant.PlantID] = genotype;
            }
            
            return genotype;
        }
        
        /// <summary>
        /// Handles breeding completion events from the breeding calculation engine.
        /// </summary>
        private void HandleBreedingCompleted(BreedingResult breedingResult)
        {
            _onBreedingCompleted?.Raise(breedingResult);
        }
        
        /// <summary>
        /// Handles mutation events from the breeding calculation engine.
        /// </summary>
        private void HandleMutationOccurred(GeneticMutation mutation)
        {
            _onMutationOccurred?.Raise(mutation);
        }
        
        /// <summary>
        /// Converts trait selection criteria to breeding goal format.
        /// </summary>
        private BreedingGoal ConvertCriteriaToBreedingGoal(TraitSelectionCriteria criteria)
        {
            // Simplified conversion - in real implementation this would be more sophisticated
            return new BreedingGoal
            {
                GoalID = System.Guid.NewGuid().ToString(),
                GoalName = "Optimization Goal",
                Description = "Generated from trait selection criteria",
                GoalType = BreedingGoalType.CustomObjective,
                TargetTraits = new List<TargetTrait>(),
                Priority = 1,
                Status = BreedingGoalStatus.Active,
                CreatedAt = System.DateTime.Now
            };
        }
        
        /// <summary>
        /// Converts optimal breeding pairs to breeding recommendation format.
        /// </summary>
        private BreedingRecommendation ConvertOptimalPairsToRecommendation(List<OptimalBreedingPair> optimalPairs)
        {
            var recommendation = new BreedingRecommendation
            {
                RecommendedPairs = new List<BreedingPair>(),
                ExpectedGeneticGain = 0.15f,
                ReasoningNotes = new List<string>(),
                ConfidenceScore = 0.8f
            };
            
            // Convert optimal pairs to simple breeding pairs
            foreach (var optimalPair in optimalPairs)
            {
                var breedingPair = new BreedingPair
                {
                    Parent1ID = optimalPair.Parent1?.GenotypeID ?? "",
                    Parent2ID = optimalPair.Parent2?.GenotypeID ?? "",
                    ExpectedOffspringValue = optimalPair.Score,
                    GeneticDistance = optimalPair.Compatibility?.CompatibilityScore ?? 0.5f,
                    Justification = "AI-optimized breeding pair selection"
                };
                
                recommendation.RecommendedPairs.Add(breedingPair);
                recommendation.ReasoningNotes.Add($"High compatibility pair: {breedingPair.Parent1ID} x {breedingPair.Parent2ID}");
            }
            
            return recommendation;
        }
        
        protected override void OnManagerShutdown()
        {
            // Unsubscribe from breeding events
            if (_breedingCalculationEngine != null)
            {
                _breedingCalculationEngine.OnBreedingCompleted -= HandleBreedingCompleted;
                _breedingCalculationEngine.OnMutationOccurred -= HandleMutationOccurred;
            }
            
            _genotypeCache.Clear();
            _pedigreeDatabase.Clear();
            
            LogInfo("GeneticsManager shutdown complete");
        }
    }
    
    /// <summary>
    /// Pedigree information for tracking breeding lineages.
    /// </summary>
    [System.Serializable]
    public class BreedingLineage
    {
        public string IndividualID;
        public string Parent1ID;
        public string Parent2ID;
        public int Generation;
        public float InbreedingCoefficient;
        public System.DateTime BreedingDate;
        public List<string> AncestorIDs = new List<string>();
    }
}