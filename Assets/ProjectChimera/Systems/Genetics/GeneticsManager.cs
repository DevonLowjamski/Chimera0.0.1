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
        private InheritanceCalculator _inheritanceCalculator;
        private TraitExpressionEngine _traitExpressionEngine;
        private BreedingSimulator _breedingSimulator;
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
            _traitExpressionEngine = new TraitExpressionEngine(_enableEpistasis, _enablePleiotropy);
            _breedingSimulator = new BreedingSimulator(_allowInbreeding, _inbreedingDepression);
            _geneticAlgorithms = new GeneticAlgorithms();
            
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
            
            // Perform breeding simulation
            var breedingResult = _breedingSimulator.PerformBreeding(
                parent1Genotype, 
                parent2Genotype, 
                numberOfOffspring,
                _enableMutations,
                _mutationRate
            );
            
            // Update pedigree tracking
            if (_trackPedigrees && breedingResult != null)
            {
                UpdatePedigreeDatabase(breedingResult);
            }
            
            // Cache offspring genotypes
            foreach (var offspring in breedingResult.OffspringGenotypes)
            {
                _genotypeCache[offspring.GenotypeID] = offspring;
            }
            
            LogInfo($"Breeding completed: {parent1.PlantName} x {parent2.PlantName} -> {breedingResult.OffspringGenotypes.Count} offspring");
            _onBreedingCompleted?.Raise(breedingResult);
            
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
            
            return _geneticAlgorithms.OptimizeBreedingPairs(genotypes, criteria);
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
            
            return _geneticAlgorithms.SimulateGenerations(foundingGenotypes, generations, selectionCriteria);
        }
        
        /// <summary>
        /// Gets breeding value prediction for a plant based on genetic markers.
        /// </summary>
        public BreedingValuePrediction PredictBreedingValue(PlantInstanceSO plant, List<TraitType> targetTraits)
        {
            var genotype = GetOrGenerateGenotype(plant);
            if (genotype == null)
                return null;
            
            return _geneticAlgorithms.PredictBreedingValue(genotype, targetTraits);
        }
        
        /// <summary>
        /// Identifies potential genetic mutations and their effects.
        /// </summary>
        public List<GeneticMutation> AnalyzeMutations(PlantGenotype genotype)
        {
            if (genotype == null)
                return new List<GeneticMutation>();
            
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
            
            return _breedingSimulator.AnalyzeCompatibility(genotype1, genotype2);
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
        /// Updates the pedigree database with breeding results.
        /// </summary>
        private void UpdatePedigreeDatabase(BreedingResult breedingResult)
        {
            if (breedingResult == null)
                return;
            
            foreach (var offspring in breedingResult.OffspringGenotypes)
            {
                var lineage = new BreedingLineage
                {
                    IndividualID = offspring.GenotypeID,
                    Parent1ID = breedingResult.Parent1Genotype.GenotypeID,
                    Parent2ID = breedingResult.Parent2Genotype.GenotypeID,
                    Generation = CalculateGeneration(breedingResult.Parent1Genotype, breedingResult.Parent2Genotype),
                    BreedingDate = System.DateTime.Now,
                    InbreedingCoefficient = CalculateInbreedingCoefficient(offspring.GenotypeID)
                };
                
                _pedigreeDatabase[offspring.GenotypeID] = lineage;
            }
        }
        
        /// <summary>
        /// Calculates the generation number for offspring.
        /// </summary>
        private int CalculateGeneration(PlantGenotype parent1, PlantGenotype parent2)
        {
            int gen1 = _pedigreeDatabase.ContainsKey(parent1.GenotypeID) ? 
                _pedigreeDatabase[parent1.GenotypeID].Generation : 0;
            int gen2 = _pedigreeDatabase.ContainsKey(parent2.GenotypeID) ? 
                _pedigreeDatabase[parent2.GenotypeID].Generation : 0;
            
            return Mathf.Max(gen1, gen2) + 1;
        }
        
        /// <summary>
        /// Calculates inbreeding coefficient based on pedigree.
        /// </summary>
        private float CalculateInbreedingCoefficient(string individualID)
        {
            // Simplified inbreeding coefficient calculation
            // In reality, this would require more complex pedigree analysis
            if (!_pedigreeDatabase.ContainsKey(individualID))
                return 0f;
            
            var lineage = _pedigreeDatabase[individualID];
            if (lineage.Parent1ID == lineage.Parent2ID)
                return 1f; // Self-fertilization
            
            // Check for common ancestors (simplified)
            float coefficient = 0f;
            if (_pedigreeDatabase.ContainsKey(lineage.Parent1ID) && 
                _pedigreeDatabase.ContainsKey(lineage.Parent2ID))
            {
                var parent1Lineage = _pedigreeDatabase[lineage.Parent1ID];
                var parent2Lineage = _pedigreeDatabase[lineage.Parent2ID];
                
                // Check for shared grandparents
                if (parent1Lineage.Parent1ID == parent2Lineage.Parent1ID || 
                    parent1Lineage.Parent1ID == parent2Lineage.Parent2ID ||
                    parent1Lineage.Parent2ID == parent2Lineage.Parent1ID || 
                    parent1Lineage.Parent2ID == parent2Lineage.Parent2ID)
                {
                    coefficient = 0.125f; // Half-siblings
                }
            }
            
            return coefficient;
        }
        
        protected override void OnManagerShutdown()
        {
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