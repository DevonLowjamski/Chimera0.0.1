using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using System;

// Explicit type aliases to resolve ambiguity
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using DataMutationType = ProjectChimera.Data.Genetics.MutationType;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Phase 2.4: Advanced Breeding Simulator with sophisticated mutation system.
    /// Integrates with stress response and QTL systems for realistic genetic variation.
    /// </summary>
    public class BreedingSimulator
    {
        private readonly bool _allowInbreeding;
        private readonly float _inbreedingDepression;
        private readonly System.Random _random;
        private readonly AdvancedMutationEngine _mutationEngine;
        
        // Advanced mutation system constants
        private const float BASE_MUTATION_RATE = 0.001f; // 0.1% base rate
        private const float STRESS_MUTATION_MULTIPLIER = 3f; // Stress increases mutation rate
        private const float INBREEDING_MUTATION_MULTIPLIER = 2f; // Inbreeding increases harmful mutations
        private const float GENERATION_MUTATION_ACCUMULATION = 0.0002f; // Mutations accumulate over generations
        private const int MAX_OFFSPRING_PER_BREEDING = 20;
        
        public BreedingSimulator(bool allowInbreeding, float inbreedingDepression)
        {
            _allowInbreeding = allowInbreeding;
            _inbreedingDepression = inbreedingDepression;
            _random = new System.Random();
            _mutationEngine = new AdvancedMutationEngine(_random);
        }
        
        /// <summary>
        /// Advanced breeding operation with sophisticated genetic inheritance and mutation system.
        /// Phase 2.4: Integrates stress response, QTL effects, and advanced mutations.
        /// </summary>
        public BreedingResult PerformBreeding(PlantGenotype parent1, PlantGenotype parent2, 
            int numberOfOffspring, bool enableMutations, float mutationRate)
        {
            if (parent1 == null || parent2 == null)
            {
                Debug.LogError("BreedingSimulator: Cannot breed with null parents");
                return CreateFailedBreedingResult(parent1, parent2, "Null parent genotype");
            }
            
            numberOfOffspring = Mathf.Clamp(numberOfOffspring, 1, MAX_OFFSPRING_PER_BREEDING);
            
            var result = new BreedingResult
            {
                Parent1Genotype = parent1,
                Parent2Genotype = parent2,
                OffspringGenotypes = new List<PlantGenotype>(),
                MutationsOccurred = new List<MutationRecord>(),
                BreedingSuccess = 1.0f,
                BreedingDate = System.DateTime.Now,
                BreedingNotes = ""
            };
            
            try
            {
                // Analyze breeding compatibility and environmental factors
                var compatibility = AnalyzeCompatibility(parent1, parent2);
                result.BreedingSuccess = CalculateBreedingSuccessRate(compatibility);
                
                // Calculate mutation factors based on parental stress and inbreeding
                var mutationFactors = CalculateMutationFactors(parent1, parent2, compatibility, mutationRate);
                
                // Generate offspring with advanced genetics
                for (int i = 0; i < numberOfOffspring; i++)
                {
                    var offspring = GenerateAdvancedOffspring(parent1, parent2, i, enableMutations, mutationFactors, result);
                    if (offspring != null)
                    {
                        result.OffspringGenotypes.Add(offspring);
                    }
                }
                
                // Apply post-breeding genetic effects
                ApplyPostBreedingEffects(result, compatibility);
                
                // Generate breeding summary
                GenerateBreedingSummary(result, mutationFactors);
                
                Debug.Log($"BreedingSimulator: Advanced breeding complete - {result.OffspringGenotypes.Count} offspring, " +
                         $"{result.MutationsOccurred.Count} mutations, success rate: {result.BreedingSuccess:F2}");
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error during advanced breeding - {ex.Message}");
                return CreateFailedBreedingResult(parent1, parent2, ex.Message);
            }
        }
        
        /// <summary>
        /// Generate offspring with advanced genetic inheritance including QTL and stress effects.
        /// </summary>
        private PlantGenotype GenerateAdvancedOffspring(PlantGenotype parent1, PlantGenotype parent2, int offspringIndex,
            bool enableMutations, MutationFactors mutationFactors, BreedingResult breedingResult)
        {
            try
            {
                var offspring = new PlantGenotype
                {
                    GenotypeID = GenerateOffspringId(parent1, parent2, offspringIndex),
                    StrainOrigin = DetermineOffspringStrain(parent1, parent2),
                    Generation = Mathf.Max(parent1.Generation, parent2.Generation) + 1,
                    IsFounder = false,
                    CreationDate = DateTime.Now,
                    ParentIDs = new List<string> { parent1.GenotypeID, parent2.GenotypeID },
                    Genotype = new Dictionary<string, AlleleCouple>(),
                    Mutations = new List<MutationRecord>(),
                    InbreedingCoefficient = CalculateInbreedingCoefficient(parent1, parent2)
                };
                
                // Perform Mendelian inheritance for all gene loci
                var allGeneLoci = GetAllGeneLoci(parent1, parent2);
                foreach (var geneLocus in allGeneLoci)
                {
                    var offspringAlleles = PerformMendelianInheritance(
                        parent1.Genotype?.GetValueOrDefault(geneLocus),
                        parent2.Genotype?.GetValueOrDefault(geneLocus),
                        geneLocus,
                        enableMutations,
                        mutationFactors,
                        breedingResult
                    );
                    
                    if (offspringAlleles != null)
                    {
                        offspring.Genotype[geneLocus] = offspringAlleles;
                    }
                }
                
                // Apply advanced mutation effects
                if (enableMutations)
                {
                    ApplyAdvancedMutations(offspring, mutationFactors, breedingResult);
                }
                
                // OverallFitness is now a computed property - it will be calculated automatically
                // based on the genetic traits and mutations in the genotype
                
                return offspring;
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error generating offspring {offspringIndex} - {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Perform Mendelian inheritance with advanced mutation possibilities.
        /// </summary>
        private AlleleCouple PerformMendelianInheritance(AlleleCouple parent1Alleles, AlleleCouple parent2Alleles,
            string geneLocus, bool enableMutations, MutationFactors mutationFactors, BreedingResult breedingResult)
        {
            // Handle missing alleles (incomplete penetrance)
            if (parent1Alleles == null && parent2Alleles == null)
                return null;
            
            // Get gametes from each parent (random allele selection)
            var gamete1 = GetGameteFromParent(parent1Alleles, geneLocus);
            var gamete2 = GetGameteFromParent(parent2Alleles, geneLocus);
            
            // Apply mutations during gamete formation if enabled
            if (enableMutations)
            {
                gamete1 = _mutationEngine.ApplyGameticMutation(gamete1, geneLocus, mutationFactors, breedingResult);
                gamete2 = _mutationEngine.ApplyGameticMutation(gamete2, geneLocus, mutationFactors, breedingResult);
            }
            
            return new AlleleCouple(gamete1, gamete2);
        }
        
        /// <summary>
        /// Get a random allele from parent's gene locus (simulates meiosis).
        /// </summary>
        private AlleleSO GetGameteFromParent(AlleleCouple parentAlleles, string geneLocus)
        {
            if (parentAlleles == null)
                return CreateNullAllele(geneLocus); // Handle missing alleles
            
            // Random segregation (Mendel's First Law)
            return _random.NextDouble() < 0.5 ? parentAlleles.Allele1 : parentAlleles.Allele2;
        }
        
        /// <summary>
        /// Calculate mutation factors based on parental stress, inbreeding, and environmental conditions.
        /// </summary>
        private MutationFactors CalculateMutationFactors(PlantGenotype parent1, PlantGenotype parent2, 
            BreedingCompatibility compatibility, float baseMutationRate)
        {
            var factors = new MutationFactors
            {
                BaseMutationRate = BASE_MUTATION_RATE * baseMutationRate,
                StressMultiplier = 1f,
                InbreedingMultiplier = 1f,
                GenerationMultiplier = 1f,
                EnvironmentalMultiplier = 1f
            };
            
            // Calculate stress-induced mutation rate increase
            float avgParentalStress = CalculateParentalStressLevel(parent1, parent2);
            factors.StressMultiplier = 1f + (avgParentalStress * STRESS_MUTATION_MULTIPLIER);
            
            // Calculate inbreeding-induced mutation rate increase
            if (compatibility.InbreedingRisk > 0.1f)
            {
                factors.InbreedingMultiplier = 1f + (compatibility.InbreedingRisk * INBREEDING_MUTATION_MULTIPLIER);
            }
            
            // Calculate generation-based mutation accumulation
            float avgGeneration = (parent1.Generation + parent2.Generation) * 0.5f;
            factors.GenerationMultiplier = 1f + (avgGeneration * GENERATION_MUTATION_ACCUMULATION);
            
            // Environmental factors (simulated for now)
            factors.EnvironmentalMultiplier = UnityEngine.Random.Range(0.8f, 1.3f);
            
            // Calculate final effective mutation rate
            factors.EffectiveMutationRate = factors.BaseMutationRate * 
                                          factors.StressMultiplier * 
                                          factors.InbreedingMultiplier * 
                                          factors.GenerationMultiplier * 
                                          factors.EnvironmentalMultiplier;
            
            return factors;
        }
        
        /// <summary>
        /// Apply advanced mutations including somatic mutations and epigenetic changes.
        /// </summary>
        private void ApplyAdvancedMutations(PlantGenotype offspring, MutationFactors mutationFactors, BreedingResult breedingResult)
        {
            // Apply different types of mutations based on probability
            
            // 1. Spontaneous point mutations
            _mutationEngine.ApplySpontaneousMutations(offspring, mutationFactors, breedingResult);
            
            // 2. Chromosomal mutations (rare but impactful)
            _mutationEngine.ApplyChromosomalMutations(offspring, mutationFactors, breedingResult);
            
            // 3. Regulatory mutations (affect gene expression)
            _mutationEngine.ApplyRegulatoryMutations(offspring, mutationFactors, breedingResult);
            
            // 4. Copy number variations
            _mutationEngine.ApplyCopyNumberVariations(offspring, mutationFactors, breedingResult);
            
            // 5. Epigenetic modifications
            _mutationEngine.ApplyEpigeneticModifications(offspring, mutationFactors, breedingResult);
        }
        
        /// <summary>
        /// Calculate offspring fitness considering QTL effects and mutations.
        /// </summary>
        private float CalculateOffspringFitness(PlantGenotype offspring, PlantGenotype parent1, PlantGenotype parent2, 
            MutationFactors mutationFactors)
        {
            float baseFitness = (parent1.OverallFitness + parent2.OverallFitness) * 0.5f;
            
            // Apply inbreeding depression
            float inbreedingPenalty = offspring.InbreedingCoefficient * _inbreedingDepression;
            baseFitness -= inbreedingPenalty;
            
            // Apply mutation effects
            if (offspring.Mutations != null && offspring.Mutations.Count > 0)
            {
                float mutationEffect = 0f;
                foreach (var mutation in offspring.Mutations)
                {
                    if (mutation.IsBeneficial)
                        mutationEffect += mutation.PhenotypicEffect * 0.5f;
                    else if (mutation.IsHarmful)
                        mutationEffect -= mutation.PhenotypicEffect;
                    // Neutral mutations have no fitness effect
                }
                baseFitness += mutationEffect;
            }
            
            // Heterozygote advantage (hybrid vigor)
            if (parent1.StrainOrigin != parent2.StrainOrigin)
            {
                baseFitness += 0.05f; // 5% hybrid vigor bonus
            }
            
            // Generation penalty (genetic load accumulation)
            float generationPenalty = offspring.Generation * 0.01f; // 1% per generation
            baseFitness -= generationPenalty;
            
            return Mathf.Clamp01(baseFitness);
        }
        
        /// <summary>
        /// Calculate parental stress level for mutation rate calculation.
        /// </summary>
        private float CalculateParentalStressLevel(PlantGenotype parent1, PlantGenotype parent2)
        {
            // Simplified stress calculation - in full implementation would integrate with stress response system
            float stress1 = parent1.Mutations?.Count(m => m.IsHarmful) * 0.1f ?? 0f;
            float stress2 = parent2.Mutations?.Count(m => m.IsHarmful) * 0.1f ?? 0f;
            
            // High inbreeding coefficient indicates stress
            stress1 += parent1.InbreedingCoefficient * 0.3f;
            stress2 += parent2.InbreedingCoefficient * 0.3f;
            
            return (stress1 + stress2) * 0.5f;
        }
        
        /// <summary>
        /// Apply post-breeding effects like genetic drift and selection.
        /// </summary>
        private void ApplyPostBreedingEffects(BreedingResult result, BreedingCompatibility compatibility)
        {
            // Apply selection pressure - remove severely unfit offspring
            var viableOffspring = new List<PlantGenotype>();
            foreach (var offspring in result.OffspringGenotypes)
            {
                if (offspring.OverallFitness > 0.1f) // Minimum viability threshold
                {
                    viableOffspring.Add(offspring);
                }
                else
                {
                    result.BreedingNotes += $" Offspring {offspring.GenotypeID} removed due to low fitness ({offspring.OverallFitness:F2}).";
                }
            }
            result.OffspringGenotypes = viableOffspring;
            
            // Apply inbreeding depression to remaining offspring
            if (compatibility.InbreedingRisk > 0.2f)
            {
                foreach (var offspring in result.OffspringGenotypes)
                {
                    // OverallFitness is a computed property - cannot be assigned directly
                    // The fitness calculation will account for inbreeding effects automatically
                }
            }
        }
        
        /// <summary>
        /// Generate comprehensive breeding summary.
        /// </summary>
        private void GenerateBreedingSummary(BreedingResult result, MutationFactors mutationFactors)
        {
            var summary = $"Advanced breeding completed. ";
            summary += $"Effective mutation rate: {mutationFactors.EffectiveMutationRate:F4} ";
            summary += $"({mutationFactors.EffectiveMutationRate / mutationFactors.BaseMutationRate:F1}x base rate). ";
            
            if (result.MutationsOccurred.Count > 0)
            {
                var beneficialMutations = result.MutationsOccurred.Count(m => m.IsBeneficial);
                var harmfulMutations = result.MutationsOccurred.Count(m => m.IsHarmful);
                var neutralMutations = result.MutationsOccurred.Count(m => m.IsNeutral);
                
                summary += $"Mutations: {beneficialMutations} beneficial, {harmfulMutations} harmful, {neutralMutations} neutral. ";
            }
            
            var avgFitness = result.OffspringGenotypes.Average(o => o.OverallFitness);
            summary += $"Average offspring fitness: {avgFitness:F2}.";
            
            result.BreedingNotes = summary;
        }
        
        // Utility methods
        
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
        
        private float CalculateInbreedingCoefficient(PlantGenotype parent1, PlantGenotype parent2)
        {
            float baseInbreeding = (parent1.InbreedingCoefficient + parent2.InbreedingCoefficient) * 0.5f;
            
            // Additional inbreeding from shared ancestry
            if (parent1.StrainOrigin == parent2.StrainOrigin)
            {
                baseInbreeding += 0.125f;
            }
            
            // Check for direct relationship
            if (parent1.ParentIDs != null && parent2.ParentIDs != null)
            {
                // Check for parent-offspring relationship
                if (parent1.ParentIDs.Contains(parent2.GenotypeID) || 
                    parent2.ParentIDs.Contains(parent1.GenotypeID))
                {
                    baseInbreeding += 0.25f; // 25% additional inbreeding
                }
                // Check for sibling relationship
                else if (parent1.ParentIDs.Intersect(parent2.ParentIDs).Any())
                {
                    baseInbreeding += 0.125f; // 12.5% additional inbreeding
                }
            }
            
            return Mathf.Clamp01(baseInbreeding);
        }
        
        private string GenerateOffspringId(PlantGenotype parent1, PlantGenotype parent2, int index)
        {
            var parent1Short = parent1.GenotypeID.Substring(0, Math.Min(4, parent1.GenotypeID.Length));
            var parent2Short = parent2.GenotypeID.Substring(0, Math.Min(4, parent2.GenotypeID.Length));
            var generation = Mathf.Max(parent1.Generation, parent2.Generation) + 1;
            return $"{parent1Short}x{parent2Short}_F{generation}_{index:D2}";
        }
        
        private PlantStrainSO DetermineOffspringStrain(PlantGenotype parent1, PlantGenotype parent2)
        {
            // If same strain, keep it
            if (parent1.StrainOrigin == parent2.StrainOrigin)
                return parent1.StrainOrigin;
            
            // For different strains, randomly select one parent's strain
            // In full implementation, this would create hybrid strains
            return _random.NextDouble() < 0.5 ? parent1.StrainOrigin : parent2.StrainOrigin;
        }
        
        private AlleleSO CreateNullAllele(string geneLocus)
        {
            // Create a null allele for missing genetic information
            // In full implementation, this would be a proper null allele ScriptableObject
            return null;
        }
        
        private float CalculateBreedingSuccessRate(BreedingCompatibility compatibility)
        {
            float baseSuccess = 0.95f;
            
            // Reduce success for high inbreeding
            if (compatibility.InbreedingRisk > 0.25f)
            {
                baseSuccess *= (1f - compatibility.InbreedingRisk * 0.5f);
            }
            
            // Optimal genetic distance improves success
            float optimalDistance = 0.3f;
            float distanceFromOptimal = Mathf.Abs(compatibility.GeneticDistance - optimalDistance);
            baseSuccess *= (1f - distanceFromOptimal * 0.2f);
            
            return Mathf.Clamp01(baseSuccess);
        }
        
        private BreedingResult CreateFailedBreedingResult(PlantGenotype parent1, PlantGenotype parent2, string errorMessage)
        {
            return new BreedingResult
            {
                Parent1Genotype = parent1,
                Parent2Genotype = parent2,
                OffspringGenotypes = new List<PlantGenotype>(),
                MutationsOccurred = new List<MutationRecord>(),
                BreedingSuccess = 0f,
                BreedingDate = DateTime.Now,
                BreedingNotes = $"Breeding failed: {errorMessage}"
            };
        }
        
        /// <summary>
        /// Simplified compatibility analysis.
        /// </summary>
        public BreedingCompatibility AnalyzeCompatibility(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            float geneticDistance = 0.3f;
            float expectedHeterosis = CalculateExpectedHeterosis(geneticDistance);
            
            return new BreedingCompatibility
            {
                Parent1ID = genotype1.GenotypeID,
                Parent2ID = genotype2.GenotypeID,
                CompatibilityScore = 0.8f,
                GeneticDistance = geneticDistance,
                InbreedingRisk = 0.1f,
                ExpectedHeterosis = expectedHeterosis
            };
        }

        /// <summary>
        /// Analyze breeding compatibility between two plant strains.
        /// </summary>
        public BreedingCompatibility AnalyzeBreedingCompatibility(PlantStrainSO strain1, PlantStrainSO strain2)
        {
            if (strain1 == null || strain2 == null)
            {
                return new BreedingCompatibility
                {
                    Parent1ID = strain1?.name ?? "Unknown",
                    Parent2ID = strain2?.name ?? "Unknown",
                    CompatibilityScore = 0f,
                    GeneticDistance = 1f,
                    InbreedingRisk = 1f,
                    ExpectedHeterosis = 0f
                };
            }

            // Calculate compatibility based on strain characteristics
            float geneticDistance = CalculateStrainGeneticDistance(strain1, strain2);
            float inbreedingRisk = CalculateStrainInbreedingRisk(strain1, strain2);
            float compatibilityScore = CalculateStrainCompatibilityScore(strain1, strain2, geneticDistance, inbreedingRisk);
            float expectedHeterosis = CalculateExpectedHeterosis(geneticDistance);

            return new BreedingCompatibility
            {
                Parent1ID = strain1.name,
                Parent2ID = strain2.name,
                CompatibilityScore = compatibilityScore,
                GeneticDistance = geneticDistance,
                InbreedingRisk = inbreedingRisk,
                ExpectedHeterosis = expectedHeterosis
            };
        }

        /// <summary>
        /// Calculate genetic distance between two strains.
        /// </summary>
        private float CalculateStrainGeneticDistance(PlantStrainSO strain1, PlantStrainSO strain2)
        {
            // Simple distance calculation based on strain properties
            float thcDiff = Mathf.Abs(strain1.THCContent() - strain2.THCContent()) / 30f; // Max ~30% THC
            float cbdDiff = Mathf.Abs(strain1.CBDContent() - strain2.CBDContent()) / 25f; // Max ~25% CBD
            float yieldDiff = Mathf.Abs(strain1.BaseYield() - strain2.BaseYield()) / 1000f; // Max ~1000g yield
            float heightDiff = Mathf.Abs(strain1.BaseHeight - strain2.BaseHeight) / 3f; // Max ~3m height

            return Mathf.Clamp01((thcDiff + cbdDiff + yieldDiff + heightDiff) / 4f);
        }

        /// <summary>
        /// Calculate inbreeding risk between two strains.
        /// </summary>
        private float CalculateStrainInbreedingRisk(PlantStrainSO strain1, PlantStrainSO strain2)
        {
            // Higher risk if strains are very similar
            float geneticSimilarity = 1f - CalculateStrainGeneticDistance(strain1, strain2);
            
            // Same strain = high inbreeding risk
            if (strain1 == strain2 || strain1.StrainName == strain2.StrainName)
                return 0.9f;
            
            // Similar strains have moderate risk
            return Mathf.Clamp01(geneticSimilarity * 0.7f);
        }

        /// <summary>
        /// Calculate overall compatibility score between two strains.
        /// </summary>
        private float CalculateStrainCompatibilityScore(PlantStrainSO strain1, PlantStrainSO strain2, float geneticDistance, float inbreedingRisk)
        {
            // Optimal genetic distance is around 0.3-0.7 (not too similar, not too different)
            float distanceScore = 1f - Mathf.Abs(geneticDistance - 0.5f) * 2f;
            
            // Lower inbreeding risk is better
            float inbreedingScore = 1f - inbreedingRisk;
            
            // Combine scores
            return Mathf.Clamp01((distanceScore * 0.6f + inbreedingScore * 0.4f));
        }

        /// <summary>
        /// Calculate expected heterosis from genetic distance.
        /// </summary>
        private float CalculateExpectedHeterosis(float geneticDistance)
        {
            // Optimal heterosis occurs at moderate genetic distances
            float optimalDistance = 0.3f;
            float distanceFromOptimal = Mathf.Abs(geneticDistance - optimalDistance);
            float heterosis = Mathf.Max(0f, 0.2f - distanceFromOptimal * 0.5f);
            return heterosis;
        }

        /// <summary>
        /// Simulate offspring genotype from two parent strains.
        /// Used by testing and UI systems for breeding predictions.
        /// </summary>
        public CannabisGenotype SimulateOffspringGenotype(PlantStrainSO parent1, PlantStrainSO parent2)
        {
            if (parent1 == null || parent2 == null)
            {
                Debug.LogError("BreedingSimulator: Cannot simulate offspring with null parent strains");
                return null;
            }

            try
            {
                // Convert strains to genotypes for breeding simulation
                var parent1Genotype = ConvertStrainToGenotype(parent1);
                var parent2Genotype = ConvertStrainToGenotype(parent2);

                if (parent1Genotype == null || parent2Genotype == null)
                {
                    Debug.LogError("BreedingSimulator: Failed to convert strains to genotypes");
                    return null;
                }

                // Perform breeding to get offspring
                var breedingResult = PerformBreeding(parent1Genotype, parent2Genotype, 1, true, 1.0f);
                
                if (breedingResult?.OffspringGenotypes?.Count > 0)
                {
                    // Convert the first offspring back to CannabisGenotype format
                    return ConvertPlantGenotypeToCannabisGenotype(breedingResult.OffspringGenotypes[0]);
                }

                Debug.LogWarning("BreedingSimulator: No viable offspring generated");
                return CreateDefaultOffspringGenotype(parent1, parent2);
            }
            catch (Exception ex)
            {
                Debug.LogError($"BreedingSimulator: Error simulating offspring genotype: {ex.Message}");
                return CreateDefaultOffspringGenotype(parent1, parent2);
            }
        }

        /// <summary>
        /// Convert PlantStrainSO to PlantGenotype for breeding simulation.
        /// </summary>
        private PlantGenotype ConvertStrainToGenotype(PlantStrainSO strain)
        {
            if (strain == null) return null;

            var genotype = new PlantGenotype
            {
                GenotypeID = $"genotype_{strain.name}_{Guid.NewGuid().ToString("N")[..8]}",
                StrainOrigin = strain, // Assign the PlantStrainSO object directly
                Generation = 0,
                IsFounder = true,
                CreationDate = DateTime.Now,
                ParentIDs = new List<string>(),
                Genotype = new Dictionary<string, AlleleCouple>(),
                Mutations = new List<MutationRecord>(),
                InbreedingCoefficient = 0f,
                OverallFitness = 0.8f // Default fitness for founder strains
            };

            // Create basic genetic structure from strain characteristics
            CreateBasicGeneticStructure(genotype, strain);

            return genotype;
        }

        /// <summary>
        /// Create basic genetic structure from strain characteristics.
        /// </summary>
        private void CreateBasicGeneticStructure(PlantGenotype genotype, PlantStrainSO strain)
        {
            // Create simplified allele couples for major traits
            // In a full implementation, this would use actual genetic data from the strain

            // Height genes (simplified)
            genotype.Genotype["HEIGHT_MAIN"] = CreateAlleleCouple("HEIGHT", strain.name, "height");
            
            // THC genes (simplified) 
            genotype.Genotype["THC_MAIN"] = CreateAlleleCouple("THC", strain.name, "thc");
            
            // CBD genes (simplified)
            genotype.Genotype["CBD_MAIN"] = CreateAlleleCouple("CBD", strain.name, "cbd");
            
            // Yield genes (simplified)
            genotype.Genotype["YIELD_MAIN"] = CreateAlleleCouple("YIELD", strain.name, "yield");
        }

        /// <summary>
        /// Create a simplified allele couple for a trait.
        /// </summary>
        private AlleleCouple CreateAlleleCouple(string traitType, string strainName, string traitName)
        {
            // Create simplified alleles - in full implementation these would be proper ScriptableObjects
            // For now, we'll return null to represent simplified genetics
            return null;
        }

        /// <summary>
        /// Convert PlantGenotype back to CannabisGenotype format.
        /// </summary>
        private CannabisGenotype ConvertPlantGenotypeToCannabisGenotype(PlantGenotype plantGenotype)
        {
            if (plantGenotype == null) return null;

            return new CannabisGenotype
            {
                GenotypeId = plantGenotype.GenotypeID, // Use backing field instead of read-only property
                StrainName = plantGenotype.StrainOrigin?.name ?? "Unknown", // Convert PlantStrainSO to string
                Generation = plantGenotype.Generation,
                ParentGenotypes = plantGenotype.ParentIDs?.ToList() ?? new List<string>(), // Use backing field instead of read-only property
                CreationDate = plantGenotype.CreationDate,
                // OverallFitness is a computed property, don't assign to it
                InbreedingCoefficient = plantGenotype.InbreedingCoefficient,
                Mutations = plantGenotype.Mutations?.ToList() ?? new List<MutationRecord>(), // Convert to List
                // Simplified genetic data - in full implementation would include detailed allele information
                GeneticMarkers = new List<GeneticMarker>
                {
                    new GeneticMarker { MarkerId = "height", MarkerValue = _random.Next(50, 200) / 100f },
                    new GeneticMarker { MarkerId = "thc", MarkerValue = _random.Next(5, 30) },
                    new GeneticMarker { MarkerId = "cbd", MarkerValue = _random.Next(0, 20) },
                    new GeneticMarker { MarkerId = "yield", MarkerValue = _random.Next(200, 800) }
                }
            };
        }

        /// <summary>
        /// Create a default offspring genotype when simulation fails.
        /// </summary>
        private CannabisGenotype CreateDefaultOffspringGenotype(PlantStrainSO parent1, PlantStrainSO parent2)
        {
            return new CannabisGenotype
            {
                GenotypeId = $"offspring_{Guid.NewGuid().ToString("N")[..8]}", // Use backing field instead of read-only property
                StrainName = $"{parent1.name} x {parent2.name}",
                Generation = 1,
                ParentGenotypes = new List<string> { parent1.name, parent2.name }, // Use backing field instead of read-only property
                CreationDate = DateTime.Now,
                // OverallFitness is a computed property, don't assign to it
                InbreedingCoefficient = 0.1f,
                Mutations = new List<MutationRecord>(), // Convert to List
                GeneticMarkers = new List<GeneticMarker>
                {
                    new GeneticMarker { MarkerId = "height", MarkerValue = _random.Next(80, 150) / 100f },
                    new GeneticMarker { MarkerId = "thc", MarkerValue = _random.Next(10, 25) },
                    new GeneticMarker { MarkerId = "cbd", MarkerValue = _random.Next(1, 10) },
                    new GeneticMarker { MarkerId = "yield", MarkerValue = _random.Next(300, 600) }
                }
            };
        }
    }
    
    /// <summary>
    /// Phase 2.4: Advanced Mutation Engine implementing sophisticated genetic mutation system.
    /// Supports multiple mutation types including point mutations, chromosomal alterations, 
    /// regulatory changes, copy number variations, and epigenetic modifications.
    /// </summary>
    public class AdvancedMutationEngine
    {
        private readonly System.Random _random;
        
        // Mutation type probabilities
        private const float POINT_MUTATION_PROBABILITY = 0.7f; // 70% of mutations are point mutations
        private const float CHROMOSOMAL_MUTATION_PROBABILITY = 0.05f; // 5% chromosomal
        private const float REGULATORY_MUTATION_PROBABILITY = 0.15f; // 15% regulatory
        private const float COPY_NUMBER_VARIATION_PROBABILITY = 0.08f; // 8% CNV
        private const float EPIGENETIC_MODIFICATION_PROBABILITY = 0.02f; // 2% epigenetic
        
        // Mutation effect parameters
        private const float BENEFICIAL_MUTATION_CHANCE = 0.1f; // 10% of mutations are beneficial
        private const float HARMFUL_MUTATION_CHANCE = 0.3f; // 30% of mutations are harmful
        private const float NEUTRAL_MUTATION_CHANCE = 0.6f; // 60% of mutations are neutral
        
        public AdvancedMutationEngine(System.Random random)
        {
            _random = random;
        }
        
        /// <summary>
        /// Apply mutations during gamete formation (meiotic mutations).
        /// </summary>
        public AlleleSO ApplyGameticMutation(AlleleSO originalAllele, string geneLocus, 
            MutationFactors mutationFactors, BreedingResult breedingResult)
        {
            if (originalAllele == null) return null;
            
            float mutationChance = mutationFactors.EffectiveMutationRate;
            
            if (_random.NextDouble() < mutationChance)
            {
                var mutation = CreatePointMutation(originalAllele, geneLocus, DataMutationType.PointMutation);
                breedingResult.MutationsOccurred.Add(mutation);
                
                // For simplified implementation, return original allele
                // In full implementation, would create mutated allele copy
                return originalAllele;
            }
            
            return originalAllele;
        }
        
        /// <summary>
        /// Apply spontaneous point mutations to individual gene loci.
        /// </summary>
        public void ApplySpontaneousMutations(PlantGenotype offspring, MutationFactors mutationFactors, 
            BreedingResult breedingResult)
        {
            if (offspring.Genotype == null) return;
            
            foreach (var geneLocus in offspring.Genotype.Keys)
            {
                float mutationChance = mutationFactors.EffectiveMutationRate * POINT_MUTATION_PROBABILITY;
                
                if (_random.NextDouble() < mutationChance)
                {
                    var mutation = CreatePointMutation(null, geneLocus, DataMutationType.PointMutation);
                    offspring.Mutations.Add(mutation);
                    breedingResult.MutationsOccurred.Add(mutation);
                }
            }
        }
        
        /// <summary>
        /// Apply rare but impactful chromosomal mutations.
        /// </summary>
        public void ApplyChromosomalMutations(PlantGenotype offspring, MutationFactors mutationFactors, 
            BreedingResult breedingResult)
        {
            float mutationChance = mutationFactors.EffectiveMutationRate * CHROMOSOMAL_MUTATION_PROBABILITY;
            
            if (_random.NextDouble() < mutationChance)
            {
                var mutationType = DetermineChromosomalMutationType();
                var mutation = CreateChromosomalMutation(offspring, mutationType);
                offspring.Mutations.Add(mutation);
                breedingResult.MutationsOccurred.Add(mutation);
            }
        }
        
        /// <summary>
        /// Apply regulatory mutations that affect gene expression levels.
        /// </summary>
        public void ApplyRegulatoryMutations(PlantGenotype offspring, MutationFactors mutationFactors, 
            BreedingResult breedingResult)
        {
            float mutationChance = mutationFactors.EffectiveMutationRate * REGULATORY_MUTATION_PROBABILITY;
            
            if (_random.NextDouble() < mutationChance)
            {
                var affectedGene = SelectRandomGeneLocus(offspring);
                if (affectedGene != null)
                {
                    var mutation = CreateRegulatoryMutation(affectedGene);
                    offspring.Mutations.Add(mutation);
                    breedingResult.MutationsOccurred.Add(mutation);
                }
            }
        }
        
        /// <summary>
        /// Apply copy number variations affecting gene dosage.
        /// </summary>
        public void ApplyCopyNumberVariations(PlantGenotype offspring, MutationFactors mutationFactors, 
            BreedingResult breedingResult)
        {
            float mutationChance = mutationFactors.EffectiveMutationRate * COPY_NUMBER_VARIATION_PROBABILITY;
            
            if (_random.NextDouble() < mutationChance)
            {
                var affectedGene = SelectRandomGeneLocus(offspring);
                if (affectedGene != null)
                {
                    var mutation = CreateCopyNumberVariation(affectedGene);
                    offspring.Mutations.Add(mutation);
                    breedingResult.MutationsOccurred.Add(mutation);
                }
            }
        }
        
        /// <summary>
        /// Apply epigenetic modifications affecting gene expression without changing DNA sequence.
        /// </summary>
        public void ApplyEpigeneticModifications(PlantGenotype offspring, MutationFactors mutationFactors, 
            BreedingResult breedingResult)
        {
            float mutationChance = mutationFactors.EffectiveMutationRate * EPIGENETIC_MODIFICATION_PROBABILITY;
            
            if (_random.NextDouble() < mutationChance)
            {
                var affectedGene = SelectRandomGeneLocus(offspring);
                if (affectedGene != null)
                {
                    var mutation = CreateEpigeneticModification(affectedGene);
                    offspring.Mutations.Add(mutation);
                    breedingResult.MutationsOccurred.Add(mutation);
                }
            }
        }
        
        /// <summary>
        /// Create a point mutation affecting a single nucleotide.
        /// </summary>
        private MutationRecord CreatePointMutation(AlleleSO affectedAllele, string geneLocus, DataMutationType mutationType)
        {
            var mutationEffect = DetermineMutationEffect();
            
            return new MutationRecord
            {
                MutationId = Guid.NewGuid().ToString(),
                AffectedGene = geneLocus,
                MutationType = mutationType.ToString(),
                EffectMagnitude = CalculatePhenotypicEffect(mutationEffect, mutationType),
                OccurrenceDate = DateTime.Now,
                IsBeneficial = mutationEffect == MutationEffectType.Beneficial,
                PhenotypicEffect = CalculatePhenotypicEffect(mutationEffect, mutationType),
                IsHarmful = mutationEffect == MutationEffectType.Harmful
            };
        }
        
        /// <summary>
        /// Create a chromosomal mutation affecting large genetic regions.
        /// </summary>
        private MutationRecord CreateChromosomalMutation(PlantGenotype offspring, DataMutationType mutationType)
        {
            var mutationEffect = DetermineMutationEffect();
            
            // Chromosomal mutations are typically more severe
            if (mutationEffect == MutationEffectType.Beneficial)
                mutationEffect = MutationEffectType.Neutral; // Reduce beneficial chance
            else if (mutationEffect == MutationEffectType.Neutral && _random.NextDouble() < 0.3f)
                mutationEffect = MutationEffectType.Harmful; // Increase harmful chance
            
            return new MutationRecord
            {
                MutationId = Guid.NewGuid().ToString(),
                AffectedGene = "CHROMOSOMAL_REGION",
                MutationType = mutationType.ToString(),
                EffectMagnitude = CalculatePhenotypicEffect(mutationEffect, mutationType) * 2f, // Double effect for chromosomal
                OccurrenceDate = DateTime.Now,
                IsBeneficial = mutationEffect == MutationEffectType.Beneficial,
                PhenotypicEffect = CalculatePhenotypicEffect(mutationEffect, mutationType) * 2f,
                IsHarmful = mutationEffect == MutationEffectType.Harmful
            };
        }
        
        /// <summary>
        /// Create a regulatory mutation affecting gene expression.
        /// </summary>
        private MutationRecord CreateRegulatoryMutation(string geneLocus)
        {
            var mutationEffect = DetermineMutationEffect();
            
            return new MutationRecord
            {
                MutationId = Guid.NewGuid().ToString(),
                AffectedGene = geneLocus,
                MutationType = DataMutationType.Regulatory.ToString(),
                EffectMagnitude = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Regulatory),
                OccurrenceDate = DateTime.Now,
                IsBeneficial = mutationEffect == MutationEffectType.Beneficial,
                PhenotypicEffect = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Regulatory),
                IsHarmful = mutationEffect == MutationEffectType.Harmful
            };
        }
        
        /// <summary>
        /// Create a copy number variation affecting gene dosage.
        /// </summary>
        private MutationRecord CreateCopyNumberVariation(string geneLocus)
        {
            var isAmplification = _random.NextDouble() < 0.5; // 50% chance of amplification vs deletion
            var mutationEffect = DetermineMutationEffect();
            
            return new MutationRecord
            {
                MutationId = Guid.NewGuid().ToString(),
                AffectedGene = geneLocus,
                MutationType = (isAmplification ? DataMutationType.Duplication : DataMutationType.Deletion).ToString(),
                EffectMagnitude = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Duplication) * 1.5f,
                OccurrenceDate = DateTime.Now,
                IsBeneficial = mutationEffect == MutationEffectType.Beneficial,
                PhenotypicEffect = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Duplication) * 1.5f,
                IsHarmful = mutationEffect == MutationEffectType.Harmful
            };
        }
        
        /// <summary>
        /// Create an epigenetic modification affecting gene expression.
        /// </summary>
        private MutationRecord CreateEpigeneticModification(string geneLocus)
        {
            var mutationEffect = DetermineMutationEffect();
            
            return new MutationRecord
            {
                MutationId = Guid.NewGuid().ToString(),
                AffectedGene = geneLocus,
                MutationType = DataMutationType.Regulatory.ToString(),
                EffectMagnitude = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Regulatory) * 0.8f,
                OccurrenceDate = DateTime.Now,
                IsBeneficial = mutationEffect == MutationEffectType.Beneficial,
                PhenotypicEffect = CalculatePhenotypicEffect(mutationEffect, DataMutationType.Regulatory) * 0.8f,
                IsHarmful = mutationEffect == MutationEffectType.Harmful
            };
        }
        
        /// <summary>
        /// Determine the chromosomal mutation type.
        /// </summary>
        private DataMutationType DetermineChromosomalMutationType()
        {
            float rand = (float)_random.NextDouble();
            if (rand < 0.3f) return DataMutationType.Deletion;
            if (rand < 0.6f) return DataMutationType.Duplication;
            if (rand < 0.8f) return DataMutationType.Inversion;
            return DataMutationType.Translocation;
        }
        
        /// <summary>
        /// Determine whether a mutation is beneficial, harmful, or neutral.
        /// </summary>
        private MutationEffectType DetermineMutationEffect()
        {
            float rand = (float)_random.NextDouble();
            if (rand < BENEFICIAL_MUTATION_CHANCE) return MutationEffectType.Beneficial;
            if (rand < BENEFICIAL_MUTATION_CHANCE + HARMFUL_MUTATION_CHANCE) return MutationEffectType.Harmful;
            return MutationEffectType.Neutral;
        }
        
        /// <summary>
        /// Calculate the phenotypic effect magnitude of a mutation.
        /// </summary>
        private float CalculatePhenotypicEffect(MutationEffectType effectType, DataMutationType mutationType)
        {
            float baseEffect = (float)_random.NextDouble() * 0.2f + 0.05f; // 0.05 to 0.25
            
            switch (effectType)
            {
                case MutationEffectType.Beneficial:
                    return baseEffect;
                case MutationEffectType.Harmful:
                    return -baseEffect;
                case MutationEffectType.Neutral:
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Generate a descriptive string for the mutation.
        /// </summary>
        private string GenerateMutationDescription(DataMutationType mutationType, string geneLocus)
        {
            return mutationType switch
            {
                DataMutationType.PointMutation => $"Point mutation in {geneLocus}",
                DataMutationType.Insertion => $"Insertion mutation in {geneLocus}",
                DataMutationType.Deletion => $"Deletion mutation in {geneLocus}",
                DataMutationType.Duplication => $"Duplication of {geneLocus}",
                DataMutationType.Inversion => $"Chromosomal inversion affecting {geneLocus}",
                DataMutationType.Translocation => $"Chromosomal translocation involving {geneLocus}",
                DataMutationType.Regulatory => $"Regulatory mutation affecting {geneLocus} expression",
                _ => $"Unknown mutation type in {geneLocus}"
            };
        }
        
        /// <summary>
        /// Select a random gene locus from the offspring's genotype.
        /// </summary>
        private string SelectRandomGeneLocus(PlantGenotype offspring)
        {
            if (offspring.Genotype == null || offspring.Genotype.Count == 0)
                return null;
            
            var geneLoci = offspring.Genotype.Keys.ToList();
            int randomIndex = _random.Next(geneLoci.Count);
            return geneLoci[randomIndex];
        }
    }
    
    /// <summary>
    /// Structure containing mutation rate factors for advanced breeding calculations.
    /// </summary>
    [System.Serializable]
    public class MutationFactors
    {
        public float BaseMutationRate;
        public float StressMultiplier;
        public float InbreedingMultiplier;
        public float GenerationMultiplier;
        public float EnvironmentalMultiplier;
        public float EffectiveMutationRate;
    }
    
    /// <summary>
    /// Enumeration of mutation effect types.
    /// </summary>
    public enum MutationEffectType
    {
        Beneficial,
        Harmful,
        Neutral
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
        public List<MutationRecord> MutationsOccurred = new List<MutationRecord>();
        public float BreedingSuccess = 1f;
        public System.DateTime BreedingDate;
        public string BreedingNotes;
    }
    
}