using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Comprehensive breeding simulator implementing Mendelian genetics for cannabis cultivation.
    /// Phase 1.1: Complete breeding system with genetic inheritance, mutations, and compatibility analysis.
    /// </summary>
    public class BreedingSimulator
    {
        private readonly bool _allowInbreeding;
        private readonly float _inbreedingDepression;
        private readonly System.Random _random;
        
        // Breeding configuration constants
        private const float DEFAULT_BREEDING_SUCCESS_RATE = 0.95f;
        private const float INBREEDING_THRESHOLD = 0.25f;
        private const float MUTATION_BASE_RATE = 0.001f; // 0.1% base mutation rate
        private const int MAX_OFFSPRING_PER_BREEDING = 20;
        
        public BreedingSimulator(bool allowInbreeding, float inbreedingDepression)
        {
            _allowInbreeding = allowInbreeding;
            _inbreedingDepression = inbreedingDepression;
            _random = new System.Random();
        }
        
        /// <summary>
        /// Perform breeding between two parent genotypes using Mendelian genetics principles.
        /// Implements proper gamete formation, fertilization, and offspring generation.
        /// </summary>
        public BreedingResult PerformBreeding(PlantGenotype parent1, PlantGenotype parent2, 
            int numberOfOffspring, bool enableMutations, float mutationRate)
        {
            if (parent1 == null || parent2 == null)
            {
                Debug.LogError("BreedingSimulator: Cannot breed with null parents");
                return CreateFailedBreedingResult(parent1, parent2, "Null parent genotype");
            }
            
            if (numberOfOffspring <= 0 || numberOfOffspring > MAX_OFFSPRING_PER_BREEDING)
            {
                Debug.LogWarning($"BreedingSimulator: Invalid offspring count {numberOfOffspring}, clamping to valid range");
                numberOfOffspring = Mathf.Clamp(numberOfOffspring, 1, MAX_OFFSPRING_PER_BREEDING);
            }
            
            var result = new BreedingResult
            {
                Parent1Genotype = parent1,
                Parent2Genotype = parent2,
                OffspringGenotypes = new List<PlantGenotype>(),
                MutationsOccurred = new List<GeneticMutation>(),
                BreedingDate = DateTime.Now,
                BreedingNotes = ""
            };
            
            try
            {
                // Check breeding compatibility
                var compatibility = AnalyzeCompatibility(parent1, parent2);
                result.BreedingSuccess = CalculateBreedingSuccessRate(compatibility);
                
                // Check inbreeding restrictions
                if (!_allowInbreeding && compatibility.InbreedingRisk > INBREEDING_THRESHOLD)
                {
                    result.BreedingNotes = $"Breeding blocked due to high inbreeding risk ({compatibility.InbreedingRisk:F2})";
                    result.BreedingSuccess = 0f;
                    return result;
                }
                
                // Generate offspring through Mendelian inheritance
                for (int i = 0; i < numberOfOffspring; i++)
                {
                    var offspring = GenerateOffspring(parent1, parent2, i, enableMutations, mutationRate, result);
                    if (offspring != null)
                    {
                        result.OffspringGenotypes.Add(offspring);
                    }
                }
                
                // Apply inbreeding depression if applicable
                if (compatibility.InbreedingRisk > 0.1f)
                {
                    ApplyInbreedingDepression(result.OffspringGenotypes, compatibility.InbreedingRisk);
                    result.BreedingNotes += $" Inbreeding depression applied ({compatibility.InbreedingRisk:F2} risk).";
                }
                
                // Calculate final breeding statistics
                CalculateBreedingStatistics(result);
                
                Debug.Log($"BreedingSimulator: Successfully bred {result.OffspringGenotypes.Count} offspring " +
                         $"from {parent1.GenotypeID} x {parent2.GenotypeID} " +
                         $"(Success rate: {result.BreedingSuccess:F2}, Mutations: {result.MutationsOccurred.Count})");
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error during breeding - {ex.Message}");
                return CreateFailedBreedingResult(parent1, parent2, ex.Message);
            }
        }
        
        /// <summary>
        /// Generate a single offspring through Mendelian genetic inheritance.
        /// </summary>
        private PlantGenotype GenerateOffspring(PlantGenotype parent1, PlantGenotype parent2, int offspringIndex,
            bool enableMutations, float mutationRate, BreedingResult breedingResult)
        {
            try
            {
                var offspring = new PlantGenotype
                {
                    GenotypeID = GenerateOffspringId(parent1, parent2, offspringIndex),
                    StrainOrigin = GetOffspringStrain(parent1, parent2),
                    Generation = Mathf.Max(parent1.Generation, parent2.Generation) + 1,
                    IsFounder = false,
                    CreationDate = DateTime.Now,
                    ParentIDs = new List<string> { parent1.GenotypeID, parent2.GenotypeID },
                    Genotype = new Dictionary<string, AlleleCouple>()
                };
                
                // Get all gene loci from both parents
                var allGeneLoci = GetAllGeneLoci(parent1, parent2);
                
                // Perform Mendelian inheritance for each gene locus
                foreach (var geneLocus in allGeneLoci)
                {
                    var offspringAlleles = PerformMendelianInheritance(
                        parent1.Genotype.GetValueOrDefault(geneLocus),
                        parent2.Genotype.GetValueOrDefault(geneLocus),
                        geneLocus,
                        enableMutations,
                        mutationRate,
                        breedingResult
                    );
                    
                    if (offspringAlleles != null)
                    {
                        offspring.Genotype[geneLocus] = offspringAlleles;
                    }
                }
                
                // Calculate offspring genetic metrics
                offspring.InbreedingCoefficient = CalculateInbreedingCoefficient(parent1, parent2);
                offspring.Mutations = breedingResult.MutationsOccurred.ToList();
                
                return offspring;
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error generating offspring {offspringIndex} - {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Perform Mendelian inheritance for a single gene locus.
        /// </summary>
        private AlleleCouple PerformMendelianInheritance(AlleleCouple parent1Alleles, AlleleCouple parent2Alleles,
            string geneLocus, bool enableMutations, float mutationRate, BreedingResult breedingResult)
        {
            // Handle cases where one or both parents lack this gene locus
            if (parent1Alleles == null && parent2Alleles == null)
                return null;
            
            // Get gametes from each parent (random allele selection)
            var gamete1 = GetGameteFromParent(parent1Alleles, geneLocus);
            var gamete2 = GetGameteFromParent(parent2Alleles, geneLocus);
            
            // Apply mutations if enabled
            if (enableMutations)
            {
                gamete1 = ApplyMutation(gamete1, mutationRate, geneLocus, breedingResult);
                gamete2 = ApplyMutation(gamete2, mutationRate, geneLocus, breedingResult);
            }
            
            // Create offspring allele combination
            return new AlleleCouple(gamete1, gamete2);
        }
        
        /// <summary>
        /// Get a random allele from parent's gene locus (simulates gamete formation).
        /// </summary>
        private AlleleSO GetGameteFromParent(AlleleCouple parentAlleles, string geneLocus)
        {
            if (parentAlleles == null)
                return null;
            
            // Random selection of one allele (Mendel's First Law - Segregation)
            return _random.NextDouble() < 0.5 ? parentAlleles.Allele1 : parentAlleles.Allele2;
        }
        
        /// <summary>
        /// Apply potential mutation to an allele during gamete formation.
        /// </summary>
        private AlleleSO ApplyMutation(AlleleSO originalAllele, float mutationRate, string geneLocus, BreedingResult breedingResult)
        {
            if (originalAllele == null)
                return null;
            
            float actualMutationRate = MUTATION_BASE_RATE * mutationRate;
            
            if (_random.NextDouble() < actualMutationRate)
            {
                // Create mutation event
                var mutation = new GeneticMutation
                {
                    MutationID = Guid.NewGuid().ToString(),
                    GeneLocusAffected = geneLocus,
                    OriginalAlleleID = originalAllele.name,
                    MutatedAlleleID = CreateMutatedAllele(originalAllele).name,
                    MutationType = DetermineMutationType(),
                    OccurrenceDate = DateTime.Now,
                    PhenotypicEffect = 0.1f,
                    Description = $"Spontaneous mutation in {geneLocus}",
                    IsBeneficial = false,
                    IsHarmful = false,
                    IsNeutral = true
                };
                
                breedingResult.MutationsOccurred.Add(mutation);
                
                Debug.Log($"BreedingSimulator: Mutation occurred at {geneLocus} - {mutation.MutationType}");
                return CreateMutatedAllele(originalAllele);
            }
            
            return originalAllele;
        }
        
        /// <summary>
        /// Create a mutated version of an allele.
        /// This is a simplified version that returns the original allele with a modified name.
        /// In a full implementation, this would create a new AlleleSO with mutated properties.
        /// </summary>
        private AlleleSO CreateMutatedAllele(AlleleSO originalAllele)
        {
            // For now, return a reference to the original allele
            // In a full implementation, this would create a new ScriptableObject instance
            // with properly initialized private fields using reflection or a factory method
            
            // TODO: Implement proper allele mutation creation
            // This would require either:
            // 1. A factory method in AlleleSO that can set private fields
            // 2. Reflection to set private serialized fields
            // 3. A mutation system that modifies existing alleles
            
            return originalAllele;
        }
        
        /// <summary>
        /// Determine the type of mutation that occurred.
        /// </summary>
        private MutationType DetermineMutationType()
        {
            float rand = (float)_random.NextDouble();
            if (rand < 0.6f) return MutationType.PointMutation;
            if (rand < 0.8f) return MutationType.Insertion;
            if (rand < 0.9f) return MutationType.Deletion;
            return MutationType.Duplication;
        }
        
        /// <summary>
        /// Get all unique gene loci from both parents.
        /// </summary>
        private HashSet<string> GetAllGeneLoci(PlantGenotype parent1, PlantGenotype parent2)
        {
            var allLoci = new HashSet<string>();
            
            if (parent1.Genotype != null)
            {
                foreach (var locus in parent1.Genotype.Keys)
                    allLoci.Add(locus);
            }
            
            if (parent2.Genotype != null)
            {
                foreach (var locus in parent2.Genotype.Keys)
                    allLoci.Add(locus);
            }
            
            return allLoci;
        }
        
        /// <summary>
        /// Calculate inbreeding coefficient for offspring.
        /// </summary>
        private float CalculateInbreedingCoefficient(PlantGenotype parent1, PlantGenotype parent2)
        {
            // Simplified calculation - in reality would use pedigree analysis
            float baseInbreeding = (parent1.InbreedingCoefficient + parent2.InbreedingCoefficient) * 0.5f;
            
            // Check for common ancestry (simplified)
            if (parent1.StrainOrigin == parent2.StrainOrigin)
            {
                baseInbreeding += 0.125f; // Additional inbreeding from same strain
            }
            
            return Mathf.Clamp01(baseInbreeding);
        }
        
        /// <summary>
        /// Apply inbreeding depression to offspring fitness.
        /// </summary>
        private void ApplyInbreedingDepression(List<PlantGenotype> offspring, float inbreedingRisk)
        {
            float depressionFactor = 1f - (inbreedingRisk * _inbreedingDepression);
            
            foreach (var child in offspring)
            {
                // Reduce vigor and fitness due to inbreeding
                // This would affect trait expression calculations
                child.InbreedingCoefficient *= depressionFactor;
            }
        }
        
        /// <summary>
        /// Generate unique offspring ID.
        /// </summary>
        private string GenerateOffspringId(PlantGenotype parent1, PlantGenotype parent2, int index)
        {
            var parent1Short = parent1.GenotypeID.Substring(0, Math.Min(4, parent1.GenotypeID.Length));
            var parent2Short = parent2.GenotypeID.Substring(0, Math.Min(4, parent2.GenotypeID.Length));
            return $"{parent1Short}x{parent2Short}_F1_{index:D2}";
        }
        
        /// <summary>
        /// Determine offspring strain from parent strains.
        /// </summary>
        private PlantStrainSO GetOffspringStrain(PlantGenotype parent1, PlantGenotype parent2)
        {
            // If same strain, keep it
            if (parent1.StrainOrigin == parent2.StrainOrigin)
                return parent1.StrainOrigin;
            
            // For different strains, create hybrid (would need hybrid strain creation logic)
            // For now, randomly select one parent's strain
            return _random.NextDouble() < 0.5 ? parent1.StrainOrigin : parent2.StrainOrigin;
        }
        
        /// <summary>
        /// Calculate breeding success rate based on compatibility.
        /// </summary>
        private float CalculateBreedingSuccessRate(BreedingCompatibility compatibility)
        {
            float baseSuccess = DEFAULT_BREEDING_SUCCESS_RATE;
            
            // Reduce success for high inbreeding
            if (compatibility.InbreedingRisk > INBREEDING_THRESHOLD)
            {
                baseSuccess *= (1f - compatibility.InbreedingRisk * 0.5f);
            }
            
            // Adjust based on genetic distance (moderate distance is optimal)
            float optimalDistance = 0.3f;
            float distanceFromOptimal = Mathf.Abs(compatibility.GeneticDistance - optimalDistance);
            baseSuccess *= (1f - distanceFromOptimal * 0.2f);
            
            return Mathf.Clamp01(baseSuccess);
        }
        
        /// <summary>
        /// Calculate final breeding statistics.
        /// </summary>
        private void CalculateBreedingStatistics(BreedingResult result)
        {
            if (result.OffspringGenotypes.Count == 0)
            {
                result.BreedingNotes += " No viable offspring produced.";
                return;
            }
            
            // Calculate genetic diversity in offspring
            float avgInbreeding = result.OffspringGenotypes.Average(o => o.InbreedingCoefficient);
            int mutationCount = result.MutationsOccurred.Count;
            
            result.BreedingNotes += $" Avg inbreeding: {avgInbreeding:F3}, Mutations: {mutationCount}";
        }
        
        /// <summary>
        /// Create a failed breeding result.
        /// </summary>
        private BreedingResult CreateFailedBreedingResult(PlantGenotype parent1, PlantGenotype parent2, string errorMessage)
        {
            return new BreedingResult
            {
                Parent1Genotype = parent1,
                Parent2Genotype = parent2,
                OffspringGenotypes = new List<PlantGenotype>(),
                MutationsOccurred = new List<GeneticMutation>(),
                BreedingSuccess = 0f,
                BreedingDate = DateTime.Now,
                BreedingNotes = $"Breeding failed: {errorMessage}"
            };
        }
        
        /// <summary>
        /// Comprehensive compatibility analysis between two plant genotypes.
        /// Calculates genetic distance, inbreeding risk, and overall compatibility score.
        /// </summary>
        public BreedingCompatibility AnalyzeCompatibility(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            if (genotype1 == null || genotype2 == null)
            {
                Debug.LogError("BreedingSimulator: Cannot analyze compatibility with null genotypes");
                return CreateFailedCompatibility(genotype1, genotype2);
            }
            
            try
            {
                var compatibility = new BreedingCompatibility
                {
                    Parent1ID = genotype1.GenotypeID,
                    Parent2ID = genotype2.GenotypeID
                };
                
                // Calculate genetic distance
                compatibility.GeneticDistance = CalculateGeneticDistance(genotype1, genotype2);
                
                // Calculate inbreeding risk
                compatibility.InbreedingRisk = CalculateInbreedingRisk(genotype1, genotype2);
                
                // Calculate overall compatibility score
                compatibility.CompatibilityScore = CalculateCompatibilityScore(
                    compatibility.GeneticDistance, 
                    compatibility.InbreedingRisk,
                    genotype1,
                    genotype2
                );
                
                Debug.Log($"BreedingSimulator: Compatibility analysis complete - " +
                         $"Distance: {compatibility.GeneticDistance:F3}, " +
                         $"Inbreeding Risk: {compatibility.InbreedingRisk:F3}, " +
                         $"Score: {compatibility.CompatibilityScore:F3}");
                
                return compatibility;
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error analyzing compatibility - {ex.Message}");
                return CreateFailedCompatibility(genotype1, genotype2);
            }
        }
        
        /// <summary>
        /// Calculate genetic distance between two genotypes using allele comparison.
        /// </summary>
        private float CalculateGeneticDistance(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            if (genotype1.Genotype == null || genotype2.Genotype == null)
                return 0.5f; // Default moderate distance for missing data
            
            var allLoci = new HashSet<string>();
            foreach (var locus in genotype1.Genotype.Keys)
                allLoci.Add(locus);
            foreach (var locus in genotype2.Genotype.Keys)
                allLoci.Add(locus);
            
            if (allLoci.Count == 0)
                return 0.5f; // Default distance
            
            float totalDistance = 0f;
            int comparedLoci = 0;
            
            foreach (var locus in allLoci)
            {
                var alleles1 = genotype1.Genotype.GetValueOrDefault(locus);
                var alleles2 = genotype2.Genotype.GetValueOrDefault(locus);
                
                if (alleles1 != null && alleles2 != null)
                {
                    float locusDistance = CalculateLocusDistance(alleles1, alleles2);
                    totalDistance += locusDistance;
                    comparedLoci++;
                }
            }
            
            return comparedLoci > 0 ? totalDistance / comparedLoci : 0.5f;
        }
        
        /// <summary>
        /// Calculate genetic distance at a specific locus.
        /// </summary>
        private float CalculateLocusDistance(AlleleCouple alleles1, AlleleCouple alleles2)
        {
            // Count identical alleles between the two genotypes
            int identicalCount = 0;
            
            // Compare allele1 of genotype1 with both alleles of genotype2
            if (alleles1.Allele1?.AlleleCode == alleles2.Allele1?.AlleleCode ||
                alleles1.Allele1?.AlleleCode == alleles2.Allele2?.AlleleCode)
            {
                identicalCount++;
            }
            
            // Compare allele2 of genotype1 with both alleles of genotype2
            if (alleles1.Allele2?.AlleleCode == alleles2.Allele1?.AlleleCode ||
                alleles1.Allele2?.AlleleCode == alleles2.Allele2?.AlleleCode)
            {
                identicalCount++;
            }
            
            // Distance is inversely related to similarity
            // 0 identical = distance 1.0, 1 identical = distance 0.5, 2 identical = distance 0.0
            return (2 - identicalCount) / 2f;
        }
        
        /// <summary>
        /// Calculate inbreeding risk based on genetic similarity and lineage.
        /// </summary>
        private float CalculateInbreedingRisk(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            float inbreedingRisk = 0f;
            
            // Check for shared ancestry
            if (HasSharedAncestry(genotype1, genotype2))
            {
                inbreedingRisk += 0.3f; // Base risk for shared ancestry
            }
            
            // Check for same strain origin
            if (genotype1.StrainOrigin == genotype2.StrainOrigin)
            {
                inbreedingRisk += 0.2f; // Additional risk for same strain
            }
            
            // Check for close genetic relationship based on existing inbreeding coefficients
            float avgInbredCoeff = (genotype1.InbreedingCoefficient + genotype2.InbreedingCoefficient) * 0.5f;
            inbreedingRisk += avgInbredCoeff * 0.4f; // Contribute existing inbreeding
            
            // High genetic similarity increases inbreeding risk
            float geneticSimilarity = 1f - CalculateGeneticDistance(genotype1, genotype2);
            if (geneticSimilarity > 0.8f)
            {
                inbreedingRisk += (geneticSimilarity - 0.8f) * 2f; // Exponential increase for very similar genetics
            }
            
            // Check generation relationship
            int generationDifference = Mathf.Abs(genotype1.Generation - genotype2.Generation);
            if (generationDifference < 2) // Parent-offspring or sibling relationship
            {
                inbreedingRisk += 0.4f;
            }
            else if (generationDifference < 4) // Grandparent-grandchild or cousin relationship
            {
                inbreedingRisk += 0.2f;
            }
            
            return Mathf.Clamp01(inbreedingRisk);
        }
        
        /// <summary>
        /// Check if two genotypes share ancestry through parent lineage.
        /// </summary>
        private bool HasSharedAncestry(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            if (genotype1.ParentIDs == null || genotype2.ParentIDs == null)
                return false;
            
            // Check for direct parent-offspring relationship
            if (genotype1.ParentIDs.Contains(genotype2.GenotypeID) || 
                genotype2.ParentIDs.Contains(genotype1.GenotypeID))
            {
                return true;
            }
            
            // Check for shared parents (siblings)
            foreach (var parent1 in genotype1.ParentIDs)
            {
                if (genotype2.ParentIDs.Contains(parent1))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Calculate overall compatibility score from genetic factors.
        /// </summary>
        private float CalculateCompatibilityScore(float geneticDistance, float inbreedingRisk, 
            PlantGenotype genotype1, PlantGenotype genotype2)
        {
            float baseScore = 0.8f; // Starting compatibility score
            
            // Optimal genetic distance is moderate (0.3-0.7)
            float optimalDistance = 0.5f;
            float distanceFromOptimal = Mathf.Abs(geneticDistance - optimalDistance);
            float distanceScore = 1f - (distanceFromOptimal * 1.5f); // Penalty for extreme similarity or difference
            
            // Penalty for inbreeding risk
            float inbreedingScore = 1f - (inbreedingRisk * 1.2f);
            
            // Fitness compatibility (healthier plants breed better)
            float fitness1 = CalculatePlantFitness(genotype1);
            float fitness2 = CalculatePlantFitness(genotype2);
            float fitnessScore = (fitness1 + fitness2) * 0.5f;
            
            // Viability check
            float viabilityScore = 1f;
            if (!IsPlantViable(genotype1) || !IsPlantViable(genotype2))
            {
                viabilityScore = 0.3f; // Significant penalty for non-viable genotypes
            }
            
            // Generation compatibility (avoid excessive generation gaps)
            int generationGap = Mathf.Abs(genotype1.Generation - genotype2.Generation);
            float generationScore = generationGap > 5 ? 0.8f : 1f; // Slight penalty for large generation gaps
            
            // Combine all factors
            float finalScore = baseScore * distanceScore * inbreedingScore * fitnessScore * viabilityScore * generationScore;
            
            return Mathf.Clamp01(finalScore);
        }
        
        /// <summary>
        /// Create a failed compatibility result for error cases.
        /// </summary>
        private BreedingCompatibility CreateFailedCompatibility(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return new BreedingCompatibility
            {
                Parent1ID = genotype1?.GenotypeID ?? "Unknown",
                Parent2ID = genotype2?.GenotypeID ?? "Unknown",
                GeneticDistance = 0f,
                InbreedingRisk = 1f, // Maximum risk for failed analysis
                CompatibilityScore = 0f // No compatibility
            };
        }

        /// <summary>
        /// Calculate a plant's overall fitness based on its genetic makeup.
        /// </summary>
        private float CalculatePlantFitness(PlantGenotype genotype)
        {
            if (genotype == null) return 0f;
            
            // Base fitness starts at 1.0
            float fitness = 1f;
            
            // Reduce fitness based on inbreeding coefficient
            fitness -= genotype.InbreedingCoefficient * 0.3f;
            
            // Reduce fitness if there are harmful mutations
            if (genotype.Mutations != null)
            {
                int harmfulMutations = genotype.Mutations.Count(m => m.IsHarmful);
                fitness -= harmfulMutations * 0.1f;
            }
            
            // Reduce fitness for very old generations (genetic load)
            if (genotype.Generation > 10)
            {
                fitness -= (genotype.Generation - 10) * 0.02f;
            }
            
            return Mathf.Clamp01(fitness);
        }
        
        /// <summary>
        /// Determine if a plant genotype is viable for breeding.
        /// </summary>
        private bool IsPlantViable(PlantGenotype genotype)
        {
            if (genotype == null) return false;
            
            // Check for lethal mutations
            if (genotype.Mutations != null)
            {
                foreach (var mutation in genotype.Mutations)
                {
                    if (mutation.IsHarmful && mutation.PhenotypicEffect < -0.8f)
                    {
                        return false; // Lethal mutation
                    }
                }
            }
            
            // Check inbreeding coefficient - too high is lethal
            if (genotype.InbreedingCoefficient > 0.8f)
            {
                return false;
            }
            
            // Check if strain origin exists
            if (genotype.StrainOrigin == null)
            {
                return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Simplified breeding result.
    /// </summary>
    [System.Serializable]
    public class BreedingResult
    {
        public PlantGenotype Parent1Genotype;
        public PlantGenotype Parent2Genotype;
        public List<PlantGenotype> OffspringGenotypes = new List<PlantGenotype>();
        public List<GeneticMutation> MutationsOccurred = new List<GeneticMutation>();
        public float BreedingSuccess = 1f;
        public System.DateTime BreedingDate;
        public string BreedingNotes;
    }
    
    /// <summary>
    /// Simplified breeding compatibility.
    /// </summary>
    [System.Serializable]
    public class BreedingCompatibility
    {
        public string Parent1ID;
        public string Parent2ID;
        public float GeneticDistance;
        public float InbreedingRisk;
        public float CompatibilityScore;
    }
}