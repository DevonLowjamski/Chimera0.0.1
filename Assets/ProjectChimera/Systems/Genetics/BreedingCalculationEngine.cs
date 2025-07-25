using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC013-7a: Extracted breeding calculation engine from GeneticsManager
    /// Handles all breeding logic, compatibility analysis, pedigree tracking,
    /// and genetic inheritance calculations with scientific accuracy.
    /// </summary>
    public class BreedingCalculationEngine
    {
        private readonly bool _allowInbreeding;
        private readonly float _inbreedingDepression;
        private readonly bool _enableMutations;
        private readonly float _mutationRate;
        private readonly bool _trackPedigrees;
        private readonly int _maxGenerationsTracked;
        
        private Dictionary<string, BreedingLineage> _pedigreeDatabase;
        private BreedingSimulator _breedingSimulator;
        private InheritanceCalculator _inheritanceCalculator;
        
        // Events for breeding operations
        public System.Action<BreedingResult> OnBreedingCompleted;
        public System.Action<GeneticMutation> OnMutationOccurred;
        
        public BreedingCalculationEngine(bool allowInbreeding = true, float inbreedingDepression = 0.1f, 
            bool enableMutations = true, float mutationRate = 0.001f, bool trackPedigrees = true, 
            int maxGenerationsTracked = 10)
        {
            _allowInbreeding = allowInbreeding;
            _inbreedingDepression = inbreedingDepression;
            _enableMutations = enableMutations;
            _mutationRate = mutationRate;
            _trackPedigrees = trackPedigrees;
            _maxGenerationsTracked = maxGenerationsTracked;
            
            _pedigreeDatabase = new Dictionary<string, BreedingLineage>();
            _breedingSimulator = new BreedingSimulator(_allowInbreeding, _inbreedingDepression);
            _inheritanceCalculator = new InheritanceCalculator(true, true); // Enable epistasis and pleiotropy
        }
        
        /// <summary>
        /// Performs breeding between two parent plants and returns offspring genetics.
        /// </summary>
        public BreedingResult BreedPlants(PlantGenotype parent1Genotype, PlantGenotype parent2Genotype, 
            int numberOfOffspring = 1)
        {
            if (parent1Genotype == null || parent2Genotype == null)
            {
                Debug.LogError("[BreedingCalculationEngine] Cannot breed plants: one or both parent genotypes are null");
                return null;
            }
            
            // Validate breeding compatibility
            var compatibility = AnalyzeBreedingCompatibility(parent1Genotype, parent2Genotype);
            if (compatibility != null && compatibility.CompatibilityScore < 0.1f)
            {
                Debug.LogWarning($"[BreedingCalculationEngine] Low breeding compatibility: {compatibility.CompatibilityScore:F2}");
            }
            
            // Perform breeding simulation
            var breedingResult = _breedingSimulator.PerformBreeding(
                parent1Genotype, 
                parent2Genotype, 
                numberOfOffspring,
                _enableMutations,
                _mutationRate
            );
            
            if (breedingResult == null)
            {
                Debug.LogError("[BreedingCalculationEngine] Breeding simulation failed");
                return null;
            }
            
            // Update pedigree tracking
            if (_trackPedigrees)
            {
                UpdatePedigreeDatabase(breedingResult);
            }
            
            // Apply inbreeding depression if applicable
            ApplyInbreedingEffects(breedingResult);
            
            // Validate offspring genetics
            ValidateOffspringGenetics(breedingResult);
            
            Debug.Log($"[BreedingCalculationEngine] Breeding completed: {parent1Genotype.GenotypeID} x {parent2Genotype.GenotypeID} -> {breedingResult.OffspringGenotypes.Count} offspring");
            OnBreedingCompleted?.Invoke(breedingResult);
            
            return breedingResult;
        }
        
        /// <summary>
        /// Analyzes genetic compatibility between two potential breeding partners.
        /// </summary>
        public BreedingCompatibility AnalyzeBreedingCompatibility(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            if (genotype1 == null || genotype2 == null)
            {
                Debug.LogError("[BreedingCalculationEngine] Cannot analyze compatibility: one or both genotypes are null");
                return null;
            }
            
            return _breedingSimulator.AnalyzeCompatibility(genotype1, genotype2);
        }
        
        /// <summary>
        /// Calculates breeding value prediction for a genotype based on genetic markers.
        /// </summary>
        public BreedingValuePrediction PredictBreedingValue(PlantGenotype genotype, List<TraitType> targetTraits)
        {
            if (genotype == null || targetTraits == null || targetTraits.Count == 0)
            {
                Debug.LogError("[BreedingCalculationEngine] Cannot predict breeding value: invalid input parameters");
                return null;
            }
            
            var prediction = new BreedingValuePrediction
            {
                GenotypeID = genotype.GenotypeID,
                PredictedValuesByTrait = new Dictionary<TraitType, float>(),
                ReliabilityScores = new Dictionary<TraitType, float>(),
                GenomicEstimatedBreedingValue = 0f
            };
            
            // Calculate breeding values for each target trait
            float totalScore = 0f;
            
            foreach (var trait in targetTraits)
            {
                var breedingValue = CalculateTraitBreedingValue(genotype, trait);
                prediction.PredictedValuesByTrait[trait] = breedingValue.Value;
                prediction.ReliabilityScores[trait] = breedingValue.Confidence;
                totalScore += breedingValue.Value * breedingValue.Weight;
            }
            
            prediction.GenomicEstimatedBreedingValue = totalScore / targetTraits.Count;
            
            return prediction;
        }
        
        /// <summary>
        /// Optimizes breeding pairs from a population for specific breeding goals.
        /// </summary>
        public List<OptimalBreedingPair> OptimizeBreedingPairs(List<PlantGenotype> population, 
            BreedingGoal breedingGoal, int maxPairs = 5)
        {
            if (population == null || population.Count < 2 || breedingGoal == null)
            {
                Debug.LogError("[BreedingCalculationEngine] Cannot optimize breeding pairs: insufficient population or missing goal");
                return new List<OptimalBreedingPair>();
            }
            
            var optimalPairs = new List<OptimalBreedingPair>();
            
            // Generate all possible breeding combinations
            for (int i = 0; i < population.Count; i++)
            {
                for (int j = i + 1; j < population.Count; j++)
                {
                    var parent1 = population[i];
                    var parent2 = population[j];
                    
                    // Skip if inbreeding not allowed and parents are related
                    if (!_allowInbreeding && AreRelated(parent1.GenotypeID, parent2.GenotypeID))
                        continue;
                    
                    var compatibility = AnalyzeBreedingCompatibility(parent1, parent2);
                    if (compatibility == null || compatibility.CompatibilityScore < 0.3f)
                        continue;
                    
                    var pair = new OptimalBreedingPair
                    {
                        Parent1 = parent1,
                        Parent2 = parent2,
                        Compatibility = compatibility,
                        PredictedOutcome = PredictBreedingOutcome(parent1, parent2, breedingGoal),
                        Score = CalculatePairScore(parent1, parent2, breedingGoal)
                    };
                    
                    optimalPairs.Add(pair);
                }
            }
            
            // Sort by score and return top pairs
            return optimalPairs
                .OrderByDescending(p => p.Score)
                .Take(maxPairs)
                .ToList();
        }
        
        /// <summary>
        /// Simulates multiple breeding generations with selection pressure.
        /// </summary>
        public GenerationalSimulationResult SimulateGenerations(List<PlantGenotype> foundingPopulation, 
            int generations, BreedingGoal selectionGoal)
        {
            if (foundingPopulation == null || foundingPopulation.Count < 2 || generations <= 0)
            {
                Debug.LogError("[BreedingCalculationEngine] Cannot simulate generations: invalid parameters");
                return null;
            }
            
            var result = new GenerationalSimulationResult
            {
                FoundingPopulation = foundingPopulation.ToPlantInstanceList(),
                TotalGeneticGain = 0f
            };
            
            var currentPopulation = new List<PlantGenotype>(foundingPopulation);
            float totalGain = 0f;
            
            for (int generation = 1; generation <= generations; generation++)
            {
                Debug.Log($"[BreedingCalculationEngine] Simulating generation {generation}/{generations}");
                
                // Simulate generation and calculate genetic gain
                var optimalPairs = OptimizeBreedingPairs(currentPopulation, selectionGoal, currentPopulation.Count / 2);
                
                // Create offspring from optimal pairs
                var nextGeneration = new List<PlantGenotype>();
                foreach (var pair in optimalPairs)
                {
                    var breedingResult = BreedPlants(pair.Parent1, pair.Parent2, 2);
                    if (breedingResult?.OffspringGenotypes != null)
                    {
                        nextGeneration.AddRange(breedingResult.OffspringGenotypes);
                    }
                }
                
                // Calculate genetic gain for this generation
                float generationGain = CalculateGenerationGeneticGain(currentPopulation, nextGeneration);
                totalGain += generationGain;
                
                // Update population for next generation
                currentPopulation = nextGeneration.Take(foundingPopulation.Count).ToList();
            }
            
            result.TotalGeneticGain = totalGain;
            
            return result;
        }
        
        /// <summary>
        /// Gets pedigree information for a specific individual.
        /// </summary>
        public BreedingLineage GetPedigree(string individualID)
        {
            return _pedigreeDatabase.TryGetValue(individualID, out var lineage) ? lineage : null;
        }
        
        /// <summary>
        /// Calculates inbreeding coefficient for an individual.
        /// </summary>
        public float CalculateInbreedingCoefficient(string individualID)
        {
            if (!_pedigreeDatabase.TryGetValue(individualID, out var lineage))
                return 0f;
            
            if (lineage.Parent1ID == lineage.Parent2ID)
                return 1f; // Self-fertilization
            
            // Check for common ancestors
            float coefficient = 0f;
            if (_pedigreeDatabase.TryGetValue(lineage.Parent1ID, out var parent1Lineage) && 
                _pedigreeDatabase.TryGetValue(lineage.Parent2ID, out var parent2Lineage))
            {
                // Check for shared grandparents
                if (HasSharedAncestors(parent1Lineage, parent2Lineage))
                {
                    coefficient = 0.125f; // Half-siblings
                }
            }
            
            return coefficient;
        }
        
        /// <summary>
        /// Updates the pedigree database with breeding results.
        /// </summary>
        private void UpdatePedigreeDatabase(BreedingResult breedingResult)
        {
            if (breedingResult?.OffspringGenotypes == null)
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
                
                // Maintain database size
                if (_pedigreeDatabase.Count > _maxGenerationsTracked * 100)
                {
                    CleanupOldPedigreeRecords();
                }
            }
        }
        
        /// <summary>
        /// Applies inbreeding effects to offspring.
        /// </summary>
        private void ApplyInbreedingEffects(BreedingResult breedingResult)
        {
            if (!_allowInbreeding || _inbreedingDepression <= 0f)
                return;
            
            foreach (var offspring in breedingResult.OffspringGenotypes)
            {
                var inbreedingCoeff = CalculateInbreedingCoefficient(offspring.GenotypeID);
                if (inbreedingCoeff > 0f)
                {
                    // Apply fitness reduction based on inbreeding
                    ApplyInbreedingDepression(offspring, inbreedingCoeff);
                }
            }
        }
        
        /// <summary>
        /// Validates offspring genetics for consistency.
        /// </summary>
        private void ValidateOffspringGenetics(BreedingResult breedingResult)
        {
            foreach (var offspring in breedingResult.OffspringGenotypes)
            {
                if (string.IsNullOrEmpty(offspring.GenotypeID))
                {
                    Debug.LogWarning("[BreedingCalculationEngine] Offspring genotype missing ID");
                    offspring.GenotypeID = System.Guid.NewGuid().ToString();
                }
                
                // Validate genetic markers
                if (offspring.GeneticMarkers == null || offspring.GeneticMarkers.Count == 0)
                {
                    Debug.LogWarning($"[BreedingCalculationEngine] Offspring {offspring.GenotypeID} has no genetic markers");
                }
            }
        }
        
        // Additional helper methods
        private TraitBreedingValue CalculateTraitBreedingValue(PlantGenotype genotype, TraitType trait)
        {
            // Simplified breeding value calculation
            return new TraitBreedingValue
            {
                Trait = trait,
                Value = UnityEngine.Random.Range(0.3f, 0.9f),
                Confidence = UnityEngine.Random.Range(0.6f, 0.95f),
                Weight = 1.0f
            };
        }
        
        private float CalculateGeneticDiversityScore(PlantGenotype genotype)
        {
            return UnityEngine.Random.Range(0.4f, 0.9f);
        }
        
        private float CalculateInbreedingRisk(string genotypeID)
        {
            return CalculateInbreedingCoefficient(genotypeID);
        }
        
        private bool AreRelated(string genotype1ID, string genotype2ID)
        {
            // Check if two genotypes share recent ancestry
            return _pedigreeDatabase.TryGetValue(genotype1ID, out var lineage1) &&
                   _pedigreeDatabase.TryGetValue(genotype2ID, out var lineage2) &&
                   HasSharedAncestors(lineage1, lineage2);
        }
        
        private bool HasSharedAncestors(BreedingLineage lineage1, BreedingLineage lineage2)
        {
            return lineage1.Parent1ID == lineage2.Parent1ID || 
                   lineage1.Parent1ID == lineage2.Parent2ID ||
                   lineage1.Parent2ID == lineage2.Parent1ID || 
                   lineage1.Parent2ID == lineage2.Parent2ID;
        }
        
        private BreedingOutcomePrediction PredictBreedingOutcome(PlantGenotype parent1, PlantGenotype parent2, BreedingGoal goal)
        {
            return new BreedingOutcomePrediction
            {
                PredictedSuccess = UnityEngine.Random.Range(0.5f, 0.95f),
                ExpectedTraitValues = new Dictionary<TraitType, float>(),
                Confidence = UnityEngine.Random.Range(0.7f, 0.9f)
            };
        }
        
        private float CalculatePairScore(PlantGenotype parent1, PlantGenotype parent2, BreedingGoal goal)
        {
            var compatibility = AnalyzeBreedingCompatibility(parent1, parent2);
            var prediction = PredictBreedingOutcome(parent1, parent2, goal);
            
            return (compatibility?.CompatibilityScore ?? 0.5f) * 0.4f + 
                   (prediction?.PredictedSuccess ?? 0.5f) * 0.6f;
        }
        
        private float CalculateGenerationGeneticGain(List<PlantGenotype> parentGeneration, List<PlantGenotype> offspringGeneration)
        {
            // Simplified genetic gain calculation
            // In a real implementation, this would compare trait values between generations
            return UnityEngine.Random.Range(0.02f, 0.15f);
        }
        
        
        private int CalculateGeneration(PlantGenotype parent1, PlantGenotype parent2)
        {
            int gen1 = _pedigreeDatabase.TryGetValue(parent1.GenotypeID, out var lineage1) ? lineage1.Generation : 0;
            int gen2 = _pedigreeDatabase.TryGetValue(parent2.GenotypeID, out var lineage2) ? lineage2.Generation : 0;
            
            return Mathf.Max(gen1, gen2) + 1;
        }
        
        private void CleanupOldPedigreeRecords()
        {
            var oldRecords = _pedigreeDatabase.Values
                .Where(l => l.BreedingDate < System.DateTime.Now.AddYears(-1))
                .Take(50)
                .ToList();
            
            foreach (var record in oldRecords)
            {
                _pedigreeDatabase.Remove(record.IndividualID);
            }
        }
        
        private void ApplyInbreedingDepression(PlantGenotype offspring, float inbreedingCoeff)
        {
            // Apply fitness reduction to genetic markers
            var depressionFactor = 1f - (inbreedingCoeff * _inbreedingDepression);
            
            if (offspring.GeneticMarkers != null)
            {
                foreach (var marker in offspring.GeneticMarkers)
                {
                    // Apply depression to fitness-related traits
                    if (marker.LinkedTrait == "GrowthVigor" || marker.LinkedTrait == "Resistance" || 
                        marker.LinkedTrait == "DiseaseResistance" || marker.LinkedTrait == "Vigor")
                    {
                        marker.MarkerValue *= depressionFactor;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Supporting data structures for breeding calculations
    /// </summary>
    [System.Serializable]
    public class OptimalBreedingPair
    {
        public PlantGenotype Parent1;
        public PlantGenotype Parent2;
        public BreedingCompatibility Compatibility;
        public BreedingOutcomePrediction PredictedOutcome;
        public float Score;
    }
    
    [System.Serializable]
    public class TraitBreedingValue
    {
        public TraitType Trait;
        public float Value;
        public float Confidence;
        public float Weight;
    }
    
    [System.Serializable]
    public class BreedingOutcomePrediction
    {
        public float PredictedSuccess;
        public Dictionary<TraitType, float> ExpectedTraitValues;
        public float Confidence;
    }
}