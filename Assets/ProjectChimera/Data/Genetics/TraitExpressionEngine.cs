using UnityEngine;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Linq;
using System;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Trait expression engine for genetics system implementation with Height trait calculation.
    /// Phase 0.2: Core Genetic Engine - Phase 1 Implementation
    /// </summary>
    public class TraitExpressionEngine
    {
        private readonly bool _enableEpistasis;
        private readonly bool _enablePleiotropy;
        
        // Trait calculation constants
        private const float BASE_HEIGHT_DEFAULT = 1.5f; // Default height in meters
        private const float MIN_HEIGHT_MULTIPLIER = 0.3f; // Minimum height (30% of base)
        private const float MAX_HEIGHT_MULTIPLIER = 2.5f; // Maximum height (250% of base)
        private const float ENVIRONMENTAL_EFFECT_STRENGTH = 0.3f; // 30% environmental influence
        
        // Cannabinoid constants
        private const float BASE_THC_DEFAULT = 15f; // Default THC percentage
        private const float BASE_CBD_DEFAULT = 1f; // Default CBD percentage
        private const float MIN_CANNABINOID = 0f; // Minimum cannabinoid content
        private const float MAX_THC = 35f; // Maximum THC percentage
        private const float MAX_CBD = 25f; // Maximum CBD percentage
        
        // Yield constants
        private const float BASE_YIELD_DEFAULT = 400f; // Default yield in grams
        private const float MIN_YIELD_MULTIPLIER = 0.2f; // Minimum yield (20% of base)
        private const float MAX_YIELD_MULTIPLIER = 3.0f; // Maximum yield (300% of base)
        
        public TraitExpressionEngine(bool enableEpistasis, bool enablePleiotropy)
        {
            _enableEpistasis = enableEpistasis;
            _enablePleiotropy = enablePleiotropy;
        }
        
        /// <summary>
        /// Calculate comprehensive trait expression including Height, THC, CBD, and Yield traits.
        /// Phase 1.2 Implementation: Multi-trait system with genetic and environmental interactions.
        /// </summary>
        public TraitExpressionResult CalculateExpression(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            try
            {
                // Calculate all primary traits
                float heightExpression = CalculateHeightTrait(genotype, environment);
                float thcExpression = CalculateTHCTrait(genotype, environment);
                float cbdExpression = CalculateCBDTrait(genotype, environment);
                float yieldExpression = CalculateYieldTrait(genotype, environment);
                
                // Apply pleiotropy effects if enabled
                if (_enablePleiotropy)
                {
                    ApplyPleiotropicEffects(ref heightExpression, ref thcExpression, ref cbdExpression, ref yieldExpression, genotype);
                }
                
                // Calculate environmental stress response
                var stressResponse = CalculateEnvironmentalStressResponse(genotype, environment);
                
                // Apply stress effects to trait expressions
                heightExpression = ApplyStressResponseToTrait(heightExpression, PlantTrait.Height, stressResponse);
                thcExpression = ApplyStressResponseToTrait(thcExpression, PlantTrait.THCContent, stressResponse);
                cbdExpression = ApplyStressResponseToTrait(cbdExpression, PlantTrait.CBDContent, stressResponse);
                yieldExpression = ApplyStressResponseToTrait(yieldExpression, PlantTrait.FlowerYield, stressResponse);
                
                // Calculate overall fitness based on all traits and stress
                float overallFitness = CalculateOverallFitness(genotype, environment, heightExpression, thcExpression, cbdExpression, yieldExpression);
                
                // Adjust fitness for stress impact
                overallFitness *= (1f - stressResponse.OverallStressLevel * 0.3f); // Up to 30% fitness reduction from stress
                overallFitness = Mathf.Clamp01(overallFitness);
                
                return new TraitExpressionResult
                {
                    GenotypeID = genotype.GenotypeID,
                    OverallFitness = overallFitness,
                    HeightExpression = heightExpression,
                    THCExpression = thcExpression,
                    CBDExpression = cbdExpression,
                    YieldExpression = yieldExpression,
                    CalculationTimestamp = DateTime.Now,
                    EnvironmentalFactors = GetEnvironmentalFactorSummary(environment),
                    TraitCount = 4, // Height, THC, CBD, Yield
                    HasEpistasis = _enableEpistasis,
                    HasPleiotropy = _enablePleiotropy,
                    StressResponse = stressResponse // Include stress information
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"TraitExpressionEngine: Error calculating expression for {genotype.GenotypeID}: {ex.Message}");
                
                // Return safe fallback result
                return new TraitExpressionResult
                {
                    GenotypeID = genotype.GenotypeID,
                    OverallFitness = 0.5f,
                    HeightExpression = BASE_HEIGHT_DEFAULT,
                    THCExpression = BASE_THC_DEFAULT,
                    CBDExpression = BASE_CBD_DEFAULT,
                    YieldExpression = BASE_YIELD_DEFAULT,
                    CalculationTimestamp = DateTime.Now,
                    EnvironmentalFactors = "Error in calculation",
                    TraitCount = 0,
                    HasEpistasis = false,
                    HasPleiotropy = false
                };
            }
        }
        
        /// <summary>
        /// Calculate Height trait expression from genotype and environmental conditions.
        /// Implements polygenic inheritance with environmental interactions.
        /// </summary>
        private float CalculateHeightTrait(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            if (genotype?.Genotype == null || genotype.Genotype.Count == 0)
            {
                Debug.LogWarning("TraitExpressionEngine: No genetic data available, using default height");
                return BASE_HEIGHT_DEFAULT;
            }
            
            float totalGeneticEffect = 0f;
            int heightGenesFound = 0;
            float dominanceEffects = 0f;
            
            // Process each gene locus for Height-related effects
            foreach (var kvp in genotype.Genotype)
            {
                string geneLocusId = kvp.Key;
                AlleleCouple alleleCouple = kvp.Value;
                
                if (alleleCouple?.Allele1 == null || alleleCouple?.Allele2 == null)
                    continue;
                
                // Calculate individual allele effects for Height trait
                float allele1Effect = GetAlleleHeightEffect(alleleCouple.Allele1, environment);
                float allele2Effect = GetAlleleHeightEffect(alleleCouple.Allele2, environment);
                
                // Skip if neither allele affects height
                if (allele1Effect == 0f && allele2Effect == 0f)
                    continue;
                
                heightGenesFound++;
                
                // Apply dominance patterns
                float locusEffect = CalculateLocusDominance(alleleCouple, allele1Effect, allele2Effect);
                totalGeneticEffect += locusEffect;
                
                // Track dominance effects for epistasis calculation
                if (_enableEpistasis && allele1Effect != allele2Effect)
                {
                    dominanceEffects += Mathf.Abs(allele1Effect - allele2Effect) * 0.1f;
                }
            }
            
            // Get base height from strain or use default
            float baseHeight = GetStrainBaseHeight(genotype);
            
            // Calculate polygenic modifier (additive effects across loci)
            float geneticModifier = 1f;
            if (heightGenesFound > 0)
            {
                // Average effect per locus, scaled by gene count
                float averageEffect = totalGeneticEffect / heightGenesFound;
                geneticModifier = 1f + (averageEffect * 0.5f); // 50% genetic contribution
                
                // Apply epistatic interactions if enabled
                if (_enableEpistasis && heightGenesFound > 1)
                {
                    // Create locus effects dictionary for advanced epistasis
                    var locusEffects = new Dictionary<string, float>();
                    foreach (var kvp in genotype.Genotype)
                    {
                        if (kvp.Value?.Allele1 != null || kvp.Value?.Allele2 != null)
                        {
                            float allele1Effect = GetAlleleHeightEffect(kvp.Value.Allele1, environment);
                            float allele2Effect = GetAlleleHeightEffect(kvp.Value.Allele2, environment);
                            locusEffects[kvp.Key] = (allele1Effect + allele2Effect) * 0.5f;
                        }
                    }
                    
                    float epistasisModifier = CalculateAdvancedEpistasis(genotype, PlantTrait.Height, locusEffects);
                    geneticModifier *= epistasisModifier;
                }
            }
            
            // Apply environmental modifiers
            float environmentalModifier = CalculateEnvironmentalHeightModifier(environment);
            
            // Apply QTL effects for polygenic inheritance
            float qtlModifier = 1f;
            if (heightGenesFound > 0)
            {
                float qtlEffects = CalculateQTLEffects(genotype, PlantTrait.Height, environment);
                qtlModifier = 1f + (qtlEffects * 0.3f); // QTL effects contribute 30% to height
            }
            
            // Calculate final height expression
            float finalHeight = baseHeight * geneticModifier * environmentalModifier * qtlModifier;
            
            // Clamp to realistic bounds
            float minHeight = baseHeight * MIN_HEIGHT_MULTIPLIER;
            float maxHeight = baseHeight * MAX_HEIGHT_MULTIPLIER;
            finalHeight = Mathf.Clamp(finalHeight, minHeight, maxHeight);
            
            return finalHeight;
        }
        
        /// <summary>
        /// Calculate THC trait expression from genotype and environmental conditions.
        /// Implements cannabinoid biosynthesis pathway genetics.
        /// </summary>
        private float CalculateTHCTrait(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            if (genotype?.Genotype == null || genotype.Genotype.Count == 0)
            {
                Debug.LogWarning("TraitExpressionEngine: No genetic data available for THC, using default");
                return BASE_THC_DEFAULT;
            }
            
            float totalGeneticEffect = 0f;
            int thcGenesFound = 0;
            float dominanceEffects = 0f;
            
            // Process each gene locus for THC-related effects
            foreach (var kvp in genotype.Genotype)
            {
                string geneLocusId = kvp.Key;
                AlleleCouple alleleCouple = kvp.Value;
                
                if (alleleCouple?.Allele1 == null || alleleCouple?.Allele2 == null)
                    continue;
                
                // Calculate individual allele effects for THC trait
                float allele1Effect = GetAlleleTraitEffect(alleleCouple.Allele1, PlantTrait.THCContent, environment);
                float allele2Effect = GetAlleleTraitEffect(alleleCouple.Allele2, PlantTrait.THCContent, environment);
                
                // Skip if neither allele affects THC
                if (allele1Effect == 0f && allele2Effect == 0f)
                    continue;
                
                thcGenesFound++;
                
                // Apply dominance patterns
                float locusEffect = CalculateLocusDominance(alleleCouple, allele1Effect, allele2Effect);
                totalGeneticEffect += locusEffect;
                
                // Track dominance effects for epistasis calculation
                if (_enableEpistasis && allele1Effect != allele2Effect)
                {
                    dominanceEffects += Mathf.Abs(allele1Effect - allele2Effect) * 0.1f;
                }
            }
            
            // Get base THC from strain or use default
            float baseTHC = GetStrainBaseTHC(genotype);
            
            // Calculate genetic modifier
            float geneticModifier = 1f;
            if (thcGenesFound > 0)
            {
                float averageEffect = totalGeneticEffect / thcGenesFound;
                geneticModifier = 1f + (averageEffect * 0.6f); // 60% genetic contribution for cannabinoids
                
                // Apply epistatic interactions if enabled
                if (_enableEpistasis && thcGenesFound > 1)
                {
                    // Create locus effects dictionary for advanced epistasis
                    var locusEffects = new Dictionary<string, float>();
                    foreach (var kvp in genotype.Genotype)
                    {
                        if (kvp.Value?.Allele1 != null || kvp.Value?.Allele2 != null)
                        {
                            float allele1Effect = GetAlleleTraitEffect(kvp.Value.Allele1, PlantTrait.THCContent, environment);
                            float allele2Effect = GetAlleleTraitEffect(kvp.Value.Allele2, PlantTrait.THCContent, environment);
                            locusEffects[kvp.Key] = (allele1Effect + allele2Effect) * 0.5f;
                        }
                    }
                    
                    float epistasisModifier = CalculateAdvancedEpistasis(genotype, PlantTrait.THCContent, locusEffects);
                    geneticModifier *= epistasisModifier;
                }
            }
            
            // Apply environmental modifiers (THC is affected by light and temperature)
            float environmentalModifier = CalculateEnvironmentalCannabinoidModifier(environment, PlantTrait.THCContent);
            
            // Calculate final THC expression
            float finalTHC = baseTHC * geneticModifier * environmentalModifier;
            
            // Clamp to realistic bounds
            finalTHC = Mathf.Clamp(finalTHC, MIN_CANNABINOID, MAX_THC);
            
            return finalTHC;
        }
        
        /// <summary>
        /// Calculate CBD trait expression from genotype and environmental conditions.
        /// CBD and THC often have inverse relationship due to shared biosynthetic pathways.
        /// </summary>
        private float CalculateCBDTrait(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            if (genotype?.Genotype == null || genotype.Genotype.Count == 0)
            {
                Debug.LogWarning("TraitExpressionEngine: No genetic data available for CBD, using default");
                return BASE_CBD_DEFAULT;
            }
            
            float totalGeneticEffect = 0f;
            int cbdGenesFound = 0;
            float dominanceEffects = 0f;
            
            // Process each gene locus for CBD-related effects
            foreach (var kvp in genotype.Genotype)
            {
                string geneLocusId = kvp.Key;
                AlleleCouple alleleCouple = kvp.Value;
                
                if (alleleCouple?.Allele1 == null || alleleCouple?.Allele2 == null)
                    continue;
                
                // Calculate individual allele effects for CBD trait
                float allele1Effect = GetAlleleTraitEffect(alleleCouple.Allele1, PlantTrait.CBDContent, environment);
                float allele2Effect = GetAlleleTraitEffect(alleleCouple.Allele2, PlantTrait.CBDContent, environment);
                
                // Skip if neither allele affects CBD
                if (allele1Effect == 0f && allele2Effect == 0f)
                    continue;
                
                cbdGenesFound++;
                
                // Apply dominance patterns
                float locusEffect = CalculateLocusDominance(alleleCouple, allele1Effect, allele2Effect);
                totalGeneticEffect += locusEffect;
                
                // Track dominance effects for epistasis calculation
                if (_enableEpistasis && allele1Effect != allele2Effect)
                {
                    dominanceEffects += Mathf.Abs(allele1Effect - allele2Effect) * 0.1f;
                }
            }
            
            // Get base CBD from strain or use default
            float baseCBD = GetStrainBaseCBD(genotype);
            
            // Calculate genetic modifier
            float geneticModifier = 1f;
            if (cbdGenesFound > 0)
            {
                float averageEffect = totalGeneticEffect / cbdGenesFound;
                geneticModifier = 1f + (averageEffect * 0.6f); // 60% genetic contribution for cannabinoids
                
                // Apply epistatic interactions if enabled
                if (_enableEpistasis && cbdGenesFound > 1)
                {
                    // Create locus effects dictionary for advanced epistasis
                    var locusEffects = new Dictionary<string, float>();
                    foreach (var kvp in genotype.Genotype)
                    {
                        if (kvp.Value?.Allele1 != null || kvp.Value?.Allele2 != null)
                        {
                            float allele1Effect = GetAlleleTraitEffect(kvp.Value.Allele1, PlantTrait.CBDContent, environment);
                            float allele2Effect = GetAlleleTraitEffect(kvp.Value.Allele2, PlantTrait.CBDContent, environment);
                            locusEffects[kvp.Key] = (allele1Effect + allele2Effect) * 0.5f;
                        }
                    }
                    
                    float epistasisModifier = CalculateAdvancedEpistasis(genotype, PlantTrait.CBDContent, locusEffects);
                    geneticModifier *= epistasisModifier;
                }
            }
            
            // Apply environmental modifiers
            float environmentalModifier = CalculateEnvironmentalCannabinoidModifier(environment, PlantTrait.CBDContent);
            
            // Calculate final CBD expression
            float finalCBD = baseCBD * geneticModifier * environmentalModifier;
            
            // Clamp to realistic bounds
            finalCBD = Mathf.Clamp(finalCBD, MIN_CANNABINOID, MAX_CBD);
            
            return finalCBD;
        }
        
        /// <summary>
        /// Calculate Yield trait expression from genotype and environmental conditions.
        /// Yield is influenced by plant size, flowering time, and environmental optimization.
        /// </summary>
        private float CalculateYieldTrait(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            if (genotype?.Genotype == null || genotype.Genotype.Count == 0)
            {
                Debug.LogWarning("TraitExpressionEngine: No genetic data available for Yield, using default");
                return BASE_YIELD_DEFAULT;
            }
            
            float totalGeneticEffect = 0f;
            int yieldGenesFound = 0;
            float dominanceEffects = 0f;
            
            // Process each gene locus for Yield-related effects
            foreach (var kvp in genotype.Genotype)
            {
                string geneLocusId = kvp.Key;
                AlleleCouple alleleCouple = kvp.Value;
                
                if (alleleCouple?.Allele1 == null || alleleCouple?.Allele2 == null)
                    continue;
                
                // Calculate individual allele effects for Yield trait
                float allele1Effect = GetAlleleTraitEffect(alleleCouple.Allele1, PlantTrait.FlowerYield, environment);
                float allele2Effect = GetAlleleTraitEffect(alleleCouple.Allele2, PlantTrait.FlowerYield, environment);
                
                // Skip if neither allele affects Yield
                if (allele1Effect == 0f && allele2Effect == 0f)
                    continue;
                
                yieldGenesFound++;
                
                // Apply dominance patterns
                float locusEffect = CalculateLocusDominance(alleleCouple, allele1Effect, allele2Effect);
                totalGeneticEffect += locusEffect;
                
                // Track dominance effects for epistasis calculation
                if (_enableEpistasis && allele1Effect != allele2Effect)
                {
                    dominanceEffects += Mathf.Abs(allele1Effect - allele2Effect) * 0.1f;
                }
            }
            
            // Get base yield from strain or use default
            float baseYield = GetStrainBaseYield(genotype);
            
            // Calculate genetic modifier
            float geneticModifier = 1f;
            if (yieldGenesFound > 0)
            {
                float averageEffect = totalGeneticEffect / yieldGenesFound;
                geneticModifier = 1f + (averageEffect * 0.7f); // 70% genetic contribution for yield
                
                // Apply epistatic interactions if enabled
                if (_enableEpistasis && yieldGenesFound > 1)
                {
                    // Create locus effects dictionary for advanced epistasis
                    var locusEffects = new Dictionary<string, float>();
                    foreach (var kvp in genotype.Genotype)
                    {
                        if (kvp.Value?.Allele1 != null || kvp.Value?.Allele2 != null)
                        {
                            float allele1Effect = GetAlleleTraitEffect(kvp.Value.Allele1, PlantTrait.FlowerYield, environment);
                            float allele2Effect = GetAlleleTraitEffect(kvp.Value.Allele2, PlantTrait.FlowerYield, environment);
                            locusEffects[kvp.Key] = (allele1Effect + allele2Effect) * 0.5f;
                        }
                    }
                    
                    float epistasisModifier = CalculateAdvancedEpistasis(genotype, PlantTrait.FlowerYield, locusEffects);
                    geneticModifier *= epistasisModifier;
                }
            }
            
            // Apply environmental modifiers (yield is highly environmental)
            float environmentalModifier = CalculateEnvironmentalYieldModifier(environment);
            
            // Calculate final yield expression
            float finalYield = baseYield * geneticModifier * environmentalModifier;
            
            // Clamp to realistic bounds
            float minYield = baseYield * MIN_YIELD_MULTIPLIER;
            float maxYield = baseYield * MAX_YIELD_MULTIPLIER;
            finalYield = Mathf.Clamp(finalYield, minYield, maxYield);
            
            return finalYield;
        }
        
        /// <summary>
        /// Get trait effect from individual allele, with environmental interaction.
        /// Universal method for all trait types.
        /// </summary>
        private float GetAlleleTraitEffect(AlleleSO allele, PlantTrait targetTrait, EnvironmentalConditions environment)
        {
            if (allele?.TraitEffects == null || allele.TraitEffects.Count == 0)
                return 0f;
            
            // Find specific trait effects
            foreach (var traitEffect in allele.TraitEffects)
            {
                if (traitEffect.AffectedTrait == targetTrait)
                {
                    float baseEffect = traitEffect.EffectMagnitude;
                    
                    // Apply environmental modulation if allele supports it
                    if (allele.EnvironmentallySensitive && environment.IsInitialized())
                    {
                        float envModifier = CalculateEnvironmentalAlleleModifier(allele, environment, targetTrait);
                        baseEffect *= envModifier;
                    }
                    
                    return baseEffect;
                }
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Legacy method for backward compatibility with height calculations.
        /// </summary>
        private float GetAlleleHeightEffect(AlleleSO allele, EnvironmentalConditions environment)
        {
            return GetAlleleTraitEffect(allele, PlantTrait.Height, environment);
        }
        
        /// <summary>
        /// Calculate dominance effects between allele pairs at a single locus.
        /// </summary>
        private float CalculateLocusDominance(AlleleCouple alleleCouple, float allele1Effect, float allele2Effect)
        {
            var parentGene = alleleCouple.Allele1?.ParentGene ?? alleleCouple.Allele2?.ParentGene;
            
            if (parentGene == null)
                return (allele1Effect + allele2Effect) * 0.5f; // Simple additive if no gene info
            
            // Apply dominance pattern based on gene definition
            switch (parentGene.DominanceType)
            {
                case DominanceType.Complete:
                    // Dominant allele masks recessive
                    return Mathf.Max(allele1Effect, allele2Effect);
                
                case DominanceType.Incomplete:
                    // Intermediate expression
                    return (allele1Effect + allele2Effect) * 0.5f;
                
                case DominanceType.Codominant:
                    // Both alleles contribute equally
                    return allele1Effect + allele2Effect;
                
                case DominanceType.Overdominant:
                    // Heterozygote advantage
                    if (allele1Effect != allele2Effect)
                        return (allele1Effect + allele2Effect) * 1.2f;
                    return (allele1Effect + allele2Effect) * 0.5f;
                
                case DominanceType.Underdominant:
                    // Heterozygote disadvantage
                    if (allele1Effect != allele2Effect)
                        return (allele1Effect + allele2Effect) * 0.8f;
                    return (allele1Effect + allele2Effect) * 0.5f;
                
                default:
                    return (allele1Effect + allele2Effect) * 0.5f;
            }
        }
        
        /// <summary>
        /// Calculate sophisticated epistatic interactions between multiple genes.
        /// Phase 2.1: Enhanced epistasis with pathway-specific interactions.
        /// </summary>
        private float CalculateEpistasisEffect(float dominanceEffects, int geneCount)
        {
            if (geneCount < 2) return 1f;
            
            // Basic interaction strength
            float interactionStrength = dominanceEffects / geneCount;
            float epistasisModifier = 1f + (interactionStrength * 0.15f); // 15% max epistatic effect
            
            return Mathf.Clamp(epistasisModifier, 0.8f, 1.3f);
        }
        
        /// <summary>
        /// Advanced epistatic interaction calculator for multi-trait systems.
        /// Implements pathway-specific gene-gene interactions.
        /// </summary>
        private float CalculateAdvancedEpistasis(PlantGenotype genotype, PlantTrait targetTrait, 
            Dictionary<string, float> locusEffects)
        {
            if (genotype?.Genotype == null || locusEffects == null || locusEffects.Count < 2)
                return 1f;
            
            float epistasisModifier = 1f;
            
            // Cannabinoid pathway epistasis (THC and CBD share biosynthetic enzymes)
            if (targetTrait == PlantTrait.THCContent || targetTrait == PlantTrait.CBDContent)
            {
                epistasisModifier *= CalculateCannabinoidPathwayEpistasis(genotype, locusEffects);
            }
            
            // Height-related epistasis (multiple height genes interact)
            if (targetTrait == PlantTrait.Height)
            {
                epistasisModifier *= CalculateHeightPathwayEpistasis(genotype, locusEffects);
            }
            
            // Yield epistasis (yield affected by many pathways)
            if (targetTrait == PlantTrait.FlowerYield)
            {
                epistasisModifier *= CalculateYieldPathwayEpistasis(genotype, locusEffects);
            }
            
            // General multi-locus interactions
            epistasisModifier *= CalculateGeneralEpistasis(locusEffects);
            
            return Mathf.Clamp(epistasisModifier, 0.6f, 1.8f);
        }
        
        /// <summary>
        /// Calculate cannabinoid biosynthetic pathway epistasis.
        /// Models interactions between THCA/CBDA synthase genes and regulatory genes.
        /// </summary>
        private float CalculateCannabinoidPathwayEpistasis(PlantGenotype genotype, Dictionary<string, float> locusEffects)
        {
            float epistasisEffect = 1f;
            
            // Look for cannabinoid synthesis genes
            var cbgaGenes = new List<string>(); // CBGA (cannabigerolic acid) precursor genes
            var synthaseGenes = new List<string>(); // THCA/CBDA synthase genes
            var regulatoryGenes = new List<string>(); // Regulatory genes
            
            foreach (var kvp in genotype.Genotype)
            {
                string geneId = kvp.Key.ToUpper();
                
                // Classify genes by their likely function (simplified gene naming)
                if (geneId.Contains("CBGA") || geneId.Contains("CBG"))
                    cbgaGenes.Add(kvp.Key);
                else if (geneId.Contains("THCA") || geneId.Contains("CBDA") || geneId.Contains("SYNTHASE"))
                    synthaseGenes.Add(kvp.Key);
                else if (geneId.Contains("REG") || geneId.Contains("CONTROL"))
                    regulatoryGenes.Add(kvp.Key);
            }
            
            // CBGA-Synthase interaction: CBGA availability affects synthase efficiency
            if (cbgaGenes.Count > 0 && synthaseGenes.Count > 0)
            {
                float cbgaEffect = cbgaGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                float synthaseEffect = synthaseGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                
                // Positive epistasis when both CBGA and synthase are high
                if (cbgaEffect > 0.5f && synthaseEffect > 0.5f)
                {
                    epistasisEffect *= 1.2f; // 20% synergistic boost
                }
                // Negative epistasis when synthase is high but CBGA is low (rate-limiting)
                else if (synthaseEffect > 0.5f && cbgaEffect < 0.2f)
                {
                    epistasisEffect *= 0.8f; // 20% reduction due to substrate limitation
                }
            }
            
            // Regulatory gene effects
            if (regulatoryGenes.Count > 0 && synthaseGenes.Count > 0)
            {
                float regulatoryEffect = regulatoryGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                
                // Regulatory genes can enhance or suppress synthase activity
                epistasisEffect *= (1f + regulatoryEffect * 0.15f);
            }
            
            return epistasisEffect;
        }
        
        /// <summary>
        /// Calculate height pathway epistasis.
        /// Models interactions between cell division, cell elongation, and structural genes.
        /// </summary>
        private float CalculateHeightPathwayEpistasis(PlantGenotype genotype, Dictionary<string, float> locusEffects)
        {
            float epistasisEffect = 1f;
            
            // Classify height-related genes
            var growthHormoneGenes = new List<string>();
            var cellWallGenes = new List<string>();
            var structuralGenes = new List<string>();
            
            foreach (var kvp in genotype.Genotype)
            {
                string geneId = kvp.Key.ToUpper();
                
                if (geneId.Contains("HORMONE") || geneId.Contains("AUXIN") || geneId.Contains("GA"))
                    growthHormoneGenes.Add(kvp.Key);
                else if (geneId.Contains("CELL") || geneId.Contains("WALL"))
                    cellWallGenes.Add(kvp.Key);
                else if (geneId.Contains("HEIGHT") || geneId.Contains("STRUCT"))
                    structuralGenes.Add(kvp.Key);
            }
            
            // Growth hormone - structural gene interactions
            if (growthHormoneGenes.Count > 0 && structuralGenes.Count > 0)
            {
                float hormoneEffect = growthHormoneGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                float structuralEffect = structuralGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                
                // Synergistic effect when both hormone production and structural support are strong
                if (hormoneEffect > 0.3f && structuralEffect > 0.3f)
                {
                    epistasisEffect *= (1f + (hormoneEffect * structuralEffect * 0.3f));
                }
            }
            
            // Diminishing returns for too many height genes (developmental constraint)
            int totalHeightGenes = growthHormoneGenes.Count + cellWallGenes.Count + structuralGenes.Count;
            if (totalHeightGenes > 4)
            {
                float constraintPenalty = 1f - ((totalHeightGenes - 4) * 0.05f);
                epistasisEffect *= constraintPenalty;
            }
            
            return epistasisEffect;
        }
        
        /// <summary>
        /// Calculate yield pathway epistasis.
        /// Models complex interactions affecting flower development and biomass accumulation.
        /// </summary>
        private float CalculateYieldPathwayEpistasis(PlantGenotype genotype, Dictionary<string, float> locusEffects)
        {
            float epistasisEffect = 1f;
            
            // Yield is affected by multiple pathways
            var photosyntheticGenes = new List<string>();
            var floweringGenes = new List<string>();
            var metabolicGenes = new List<string>();
            var sizeGenes = new List<string>();
            
            foreach (var kvp in genotype.Genotype)
            {
                string geneId = kvp.Key.ToUpper();
                
                if (geneId.Contains("PHOTO") || geneId.Contains("LIGHT"))
                    photosyntheticGenes.Add(kvp.Key);
                else if (geneId.Contains("FLOWER") || geneId.Contains("BUD"))
                    floweringGenes.Add(kvp.Key);
                else if (geneId.Contains("METAB") || geneId.Contains("SUGAR"))
                    metabolicGenes.Add(kvp.Key);
                else if (geneId.Contains("SIZE") || geneId.Contains("HEIGHT"))
                    sizeGenes.Add(kvp.Key);
            }
            
            // Photosynthesis - metabolism interaction
            if (photosyntheticGenes.Count > 0 && metabolicGenes.Count > 0)
            {
                float photoEffect = photosyntheticGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                float metabEffect = metabolicGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                
                // Strong synergy between photosynthesis and metabolism
                epistasisEffect *= (1f + (photoEffect * metabEffect * 0.4f));
            }
            
            // Size - flowering interaction
            if (sizeGenes.Count > 0 && floweringGenes.Count > 0)
            {
                float sizeEffect = sizeGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                float flowerEffect = floweringGenes.Average(gene => locusEffects.GetValueOrDefault(gene, 0f));
                
                // Larger plants can support more flowers, but there's an optimal balance
                float sizeFlowerInteraction = sizeEffect * flowerEffect;
                if (sizeFlowerInteraction > 0.6f) // Optimal range
                {
                    epistasisEffect *= (1f + sizeFlowerInteraction * 0.3f);
                }
                else if (sizeFlowerInteraction > 0.9f) // Too much - resource competition
                {
                    epistasisEffect *= 0.9f;
                }
            }
            
            return epistasisEffect;
        }
        
        /// <summary>
        /// Calculate general multi-locus epistatic interactions.
        /// Models non-specific gene-gene interactions and genetic background effects.
        /// </summary>
        private float CalculateGeneralEpistasis(Dictionary<string, float> locusEffects)
        {
            if (locusEffects.Count < 2) return 1f;
            
            float epistasisEffect = 1f;
            var effectValues = locusEffects.Values.ToArray();
            
            // Calculate genetic variance (measure of trait complexity)
            float mean = effectValues.Average();
            float variance = effectValues.Sum(x => (x - mean) * (x - mean)) / effectValues.Length;
            
            // Moderate variance is optimal (balanced genetic architecture)
            float optimalVariance = 0.3f;
            float varianceDeviation = Mathf.Abs(variance - optimalVariance);
            
            if (varianceDeviation < 0.1f) // Well-balanced genetic architecture
            {
                epistasisEffect *= 1.1f; // 10% bonus
            }
            else if (varianceDeviation > 0.5f) // Highly unbalanced
            {
                epistasisEffect *= 0.9f; // 10% penalty
            }
            
            // Interaction complexity bonus/penalty
            int geneCount = locusEffects.Count;
            if (geneCount >= 3 && geneCount <= 6) // Optimal complexity
            {
                epistasisEffect *= (1f + (geneCount - 2) * 0.02f); // Small bonus per gene
            }
            else if (geneCount > 6) // Too complex - diminishing returns
            {
                epistasisEffect *= (1f - (geneCount - 6) * 0.03f); // Penalty for excess complexity
            }
            
            return Mathf.Clamp(epistasisEffect, 0.8f, 1.3f);
        }
        
        /// <summary>
        /// Get base height from plant strain, with fallback to default.
        /// </summary>
        private float GetStrainBaseHeight(PlantGenotype genotype)
        {
            // Try to get base height from strain
            if (genotype.StrainOrigin != null)
            {
                // Assuming PlantStrainSO has BaseHeight property
                // For now, use name-based estimation until exact property is confirmed
                var strainName = genotype.StrainOrigin.name?.ToLower();
                if (!string.IsNullOrEmpty(strainName))
                {
                    // Indica strains tend to be shorter, Sativa taller
                    if (strainName.Contains("indica"))
                        return 1.2f; // 1.2 meters average
                    else if (strainName.Contains("sativa"))
                        return 2.0f; // 2.0 meters average
                    else if (strainName.Contains("auto"))
                        return 0.8f; // Autoflower strains are typically shorter
                }
            }
            
            return BASE_HEIGHT_DEFAULT;
        }
        
        /// <summary>
        /// Calculate environmental modifier for height based on growing conditions.
        /// </summary>
        private float CalculateEnvironmentalHeightModifier(EnvironmentalConditions environment)
        {
            if (!environment.IsInitialized()) return 1f;
            
            float modifier = 1f;
            
            // Light intensity effect (more light = more growth potential)
            if (environment.LightIntensity > 0)
            {
                float lightOptimal = 600f; // Optimal PPFD for cannabis
                float lightEffect = Mathf.Clamp(environment.LightIntensity / lightOptimal, 0.3f, 1.5f);
                modifier *= Mathf.Lerp(1f, lightEffect, ENVIRONMENTAL_EFFECT_STRENGTH);
            }
            
            // Temperature effect
            float tempOptimal = 24f; // Optimal temperature (°C)
            float tempDeviation = Mathf.Abs(environment.Temperature - tempOptimal);
            float tempEffect = Mathf.Max(0.7f, 1f - (tempDeviation / 15f)); // Penalty increases with deviation
            modifier *= Mathf.Lerp(1f, tempEffect, ENVIRONMENTAL_EFFECT_STRENGTH);
            
            // CO2 effect
            if (environment.CO2Level > 400f)
            {
                float co2Boost = Mathf.Min(1.3f, 1f + ((environment.CO2Level - 400f) / 800f * 0.3f));
                modifier *= Mathf.Lerp(1f, co2Boost, ENVIRONMENTAL_EFFECT_STRENGTH);
            }
            
            return Mathf.Clamp(modifier, 0.5f, 1.8f);
        }
        
        /// <summary>
        /// Calculate environmental modifier for individual alleles.
        /// </summary>
        private float CalculateEnvironmentalAlleleModifier(AlleleSO allele, EnvironmentalConditions environment, PlantTrait targetTrait)
        {
            // Simplified environmental response - could be expanded with allele-specific modifiers
            float baseModifier = 1f;
            
            // Some alleles might be more sensitive to environmental changes
            if (allele.EnvironmentalModifiers?.Count > 0)
            {
                // Could implement specific environmental response curves here
                // For now, apply a small trait-specific variation
                float traitModifier = targetTrait switch
                {
                    PlantTrait.THCContent => UnityEngine.Random.Range(-0.15f, 0.15f), // THC more sensitive to environment
                    PlantTrait.CBDContent => UnityEngine.Random.Range(-0.1f, 0.1f),   // CBD moderately sensitive
                    PlantTrait.FlowerYield => UnityEngine.Random.Range(-0.2f, 0.2f), // Yield highly sensitive
                    PlantTrait.Height => UnityEngine.Random.Range(-0.1f, 0.1f), // Height moderately sensitive
                    _ => UnityEngine.Random.Range(-0.1f, 0.1f)
                };
                baseModifier += traitModifier;
            }
            
            return Mathf.Clamp(baseModifier, 0.7f, 1.3f);
        }
        
        /// <summary>
        /// Get base THC content from plant strain.
        /// </summary>
        private float GetStrainBaseTHC(PlantGenotype genotype)
        {
            if (genotype.StrainOrigin != null)
            {
                var strainName = genotype.StrainOrigin.name?.ToLower();
                if (!string.IsNullOrEmpty(strainName))
                {
                    // High-THC strains
                    if (strainName.Contains("og") || strainName.Contains("cookies") || strainName.Contains("diesel"))
                        return 22f; // High THC strains
                    else if (strainName.Contains("indica"))
                        return 18f; // Indica average
                    else if (strainName.Contains("sativa"))
                        return 16f; // Sativa average
                    else if (strainName.Contains("auto"))
                        return 14f; // Autoflower average
                    else if (strainName.Contains("cbd"))
                        return 8f;  // CBD-dominant strains have lower THC
                }
            }
            
            return BASE_THC_DEFAULT;
        }
        
        /// <summary>
        /// Get base CBD content from plant strain.
        /// </summary>
        private float GetStrainBaseCBD(PlantGenotype genotype)
        {
            if (genotype.StrainOrigin != null)
            {
                var strainName = genotype.StrainOrigin.name?.ToLower();
                if (!string.IsNullOrEmpty(strainName))
                {
                    // CBD-dominant strains
                    if (strainName.Contains("cbd") || strainName.Contains("hemp") || strainName.Contains("charlotte"))
                        return 15f; // High CBD strains
                    else if (strainName.Contains("balanced"))
                        return 8f;  // Balanced THC:CBD strains
                    else if (strainName.Contains("haze"))
                        return 0.5f; // Haze strains typically low CBD
                    else
                        return 1f;   // Most strains have minimal CBD
                }
            }
            
            return BASE_CBD_DEFAULT;
        }
        
        /// <summary>
        /// Get base yield from plant strain.
        /// </summary>
        private float GetStrainBaseYield(PlantGenotype genotype)
        {
            if (genotype.StrainOrigin != null)
            {
                var strainName = genotype.StrainOrigin.name?.ToLower();
                if (!string.IsNullOrEmpty(strainName))
                {
                    // High-yielding strains
                    if (strainName.Contains("critical") || strainName.Contains("big") || strainName.Contains("monster"))
                        return 600f; // High yield strains
                    else if (strainName.Contains("indica"))
                        return 450f; // Indica average
                    else if (strainName.Contains("sativa"))
                        return 350f; // Sativa average (taller but less dense)
                    else if (strainName.Contains("auto"))
                        return 200f; // Autoflower average (smaller plants)
                    else if (strainName.Contains("landrace"))
                        return 300f; // Landrace varieties
                }
            }
            
            return BASE_YIELD_DEFAULT;
        }
        
        /// <summary>
        /// Calculate environmental modifier for cannabinoid production.
        /// </summary>
        private float CalculateEnvironmentalCannabinoidModifier(EnvironmentalConditions environment, PlantTrait cannabinoidType)
        {
            if (!environment.IsInitialized()) return 1f;
            
            float modifier = 1f;
            
            // Light spectrum and intensity affect cannabinoid production
            if (environment.LightIntensity > 0)
            {
                // UV light increases cannabinoid production
                float lightOptimal = cannabinoidType == PlantTrait.THCContent ? 700f : 600f; // THC benefits from higher intensity
                float lightEffect = Mathf.Clamp(environment.LightIntensity / lightOptimal, 0.4f, 1.4f);
                modifier *= Mathf.Lerp(1f, lightEffect, 0.4f); // 40% environmental influence for cannabinoids
            }
            
            // Temperature stress can increase cannabinoid production (within limits)
            float tempOptimal = 22f; // Slightly cooler optimal for cannabinoids
            float tempDeviation = Mathf.Abs(environment.Temperature - tempOptimal);
            float tempEffect = Mathf.Max(0.6f, 1f - (tempDeviation / 20f));
            
            // Mild stress can increase cannabinoids
            if (tempDeviation > 2f && tempDeviation < 8f)
                tempEffect *= 1.1f; // Stress response bonus
            
            modifier *= Mathf.Lerp(1f, tempEffect, 0.3f);
            
            // Humidity affects cannabinoid development
            float humidityOptimal = cannabinoidType == PlantTrait.THCContent ? 45f : 50f; // THC prefers lower humidity
            if (environment.Humidity > 0)
            {
                float humidityDeviation = Mathf.Abs(environment.Humidity - humidityOptimal);
                float humidityEffect = Mathf.Max(0.7f, 1f - (humidityDeviation / 30f));
                modifier *= Mathf.Lerp(1f, humidityEffect, 0.2f);
            }
            
            return Mathf.Clamp(modifier, 0.5f, 1.6f);
        }
        
        /// <summary>
        /// Calculate environmental modifier for yield production.
        /// </summary>
        private float CalculateEnvironmentalYieldModifier(EnvironmentalConditions environment)
        {
            if (!environment.IsInitialized()) return 1f;
            
            float modifier = 1f;
            
            // Light is crucial for yield
            if (environment.LightIntensity > 0)
            {
                float lightOptimal = 800f; // Higher optimal for yield
                float lightEffect = Mathf.Clamp(environment.LightIntensity / lightOptimal, 0.2f, 1.8f);
                modifier *= Mathf.Lerp(1f, lightEffect, 0.6f); // 60% environmental influence for yield
            }
            
            // Temperature optimization
            float tempOptimal = 26f; // Warmer optimal for yield
            float tempDeviation = Mathf.Abs(environment.Temperature - tempOptimal);
            float tempEffect = Mathf.Max(0.4f, 1f - (tempDeviation / 12f));
            modifier *= Mathf.Lerp(1f, tempEffect, 0.5f);
            
            // CO2 significantly affects yield
            if (environment.CO2Level > 400f)
            {
                float co2Boost = Mathf.Min(1.6f, 1f + ((environment.CO2Level - 400f) / 600f * 0.6f));
                modifier *= Mathf.Lerp(1f, co2Boost, 0.4f);
            }
            
            // Humidity optimization
            if (environment.Humidity > 0)
            {
                float humidityOptimal = 55f; // Moderate humidity for yield
                float humidityDeviation = Mathf.Abs(environment.Humidity - humidityOptimal);
                float humidityEffect = Mathf.Max(0.6f, 1f - (humidityDeviation / 25f));
                modifier *= Mathf.Lerp(1f, humidityEffect, 0.3f);
            }
            
            return Mathf.Clamp(modifier, 0.2f, 2.2f); // Yield can vary dramatically with environment
        }
        
        /// <summary>
        /// Calculate overall fitness based on multiple traits and environmental adaptation.
        /// </summary>
        private float CalculateOverallFitness(PlantGenotype genotype, EnvironmentalConditions environment, 
            float heightExpression, float thcExpression, float cbdExpression, float yieldExpression)
        {
            float fitness = 0.7f; // Base fitness
            
            // Height fitness (moderate height is often optimal)
            float optimalHeight = GetStrainBaseHeight(genotype);
            float heightDeviation = Mathf.Abs(heightExpression - optimalHeight) / optimalHeight;
            float heightFitness = Mathf.Max(0.5f, 1f - heightDeviation);
            fitness += heightFitness * 0.2f; // Height contributes 20% to fitness
            
            // THC fitness (balance is important - very high THC can reduce other traits)
            float optimalTHC = GetStrainBaseTHC(genotype);
            float thcDeviation = Mathf.Abs(thcExpression - optimalTHC) / optimalTHC;
            float thcFitness = Mathf.Max(0.6f, 1f - thcDeviation * 0.5f); // Less penalty for THC deviation
            fitness += thcFitness * 0.25f; // THC contributes 25% to fitness
            
            // CBD fitness (generally beneficial, minimal penalty for high CBD)
            float cbdFitness = Mathf.Min(1f, 0.8f + (cbdExpression / 20f)); // Bonus for CBD content
            fitness += cbdFitness * 0.15f; // CBD contributes 15% to fitness
            
            // Yield fitness (higher yield generally better, but with diminishing returns)
            float optimalYield = GetStrainBaseYield(genotype);
            float yieldRatio = yieldExpression / optimalYield;
            float yieldFitness = Mathf.Min(1f, 0.6f + (yieldRatio * 0.4f)); // Bonus for higher yield
            fitness += yieldFitness * 0.3f; // Yield contributes 30% to fitness
            
            // Environmental adaptation bonus
            float envAdaptation = CalculateEnvironmentalHeightModifier(environment);
            fitness += (envAdaptation - 1f) * 0.1f; // Environmental adaptation contributes 10%
            
            // Genetic diversity bonus (if not too inbred)
            if (genotype.InbreedingCoefficient < 0.25f)
            {
                fitness += 0.05f; // Outbreeding bonus
            }
            
            // Trait balance bonus (reward plants with well-rounded traits)
            float traitBalance = CalculateTraitBalance(heightExpression, thcExpression, cbdExpression, yieldExpression, genotype);
            fitness += traitBalance * 0.1f; // Trait balance contributes 10%
            
            return Mathf.Clamp(fitness, 0.1f, 1.0f);
        }
        
        /// <summary>
        /// Calculate trait balance bonus for plants with harmonious trait combinations.
        /// </summary>
        private float CalculateTraitBalance(float height, float thc, float cbd, float yield, PlantGenotype genotype)
        {
            // Get optimal values for comparison
            float optimalHeight = GetStrainBaseHeight(genotype);
            float optimalTHC = GetStrainBaseTHC(genotype);
            float optimalCBD = GetStrainBaseCBD(genotype);
            float optimalYield = GetStrainBaseYield(genotype);
            
            // Calculate how close each trait is to optimal
            float heightBalance = 1f - Mathf.Abs(height - optimalHeight) / optimalHeight;
            float thcBalance = 1f - Mathf.Abs(thc - optimalTHC) / optimalTHC;
            float cbdBalance = 1f - Mathf.Abs(cbd - optimalCBD) / optimalCBD;
            float yieldBalance = 1f - Mathf.Abs(yield - optimalYield) / optimalYield;
            
            // Average balance across all traits
            float averageBalance = (heightBalance + thcBalance + cbdBalance + yieldBalance) / 4f;
            
            // Bonus for plants that don't have any extremely unbalanced traits
            float minBalance = Mathf.Min(heightBalance, Mathf.Min(thcBalance, Mathf.Min(cbdBalance, yieldBalance)));
            
            // Combine average and minimum for balanced scoring
            return (averageBalance * 0.7f + minBalance * 0.3f);
        }
        
        /// <summary>
        /// Apply pleiotropic effects between traits.
        /// Pleiotropy occurs when one gene affects multiple traits.
        /// </summary>
        private void ApplyPleiotropicEffects(ref float height, ref float thc, ref float cbd, ref float yield, PlantGenotype genotype)
        {
            if (genotype?.Genotype == null) return;
            
            float pleiotropicStrength = 0.1f; // 10% cross-trait influence
            
            // THC-CBD antagonism (they share biosynthetic pathways)
            float thcCbdRatio = (thc + cbd) > 0 ? thc / (thc + cbd) : 0.5f;
            if (thcCbdRatio > 0.8f) // High THC reduces CBD
            {
                cbd *= (1f - pleiotropicStrength);
            }
            else if (thcCbdRatio < 0.2f) // High CBD reduces THC
            {
                thc *= (1f - pleiotropicStrength);
            }
            
            // Height-Yield correlation (taller plants often yield more, but with limits)
            float heightYieldCorrelation = 0.15f;
            float baseHeight = GetStrainBaseHeight(genotype);
            float heightRatio = height / baseHeight;
            
            if (heightRatio > 1.2f) // Very tall plants
            {
                yield *= (1f + heightYieldCorrelation); // Yield bonus
                thc *= (1f - pleiotropicStrength * 0.5f); // Slight THC reduction (energy allocation)
            }
            else if (heightRatio < 0.8f) // Short plants
            {
                yield *= (1f - heightYieldCorrelation * 0.5f); // Yield penalty
                thc *= (1f + pleiotropicStrength * 0.3f); // Slight THC bonus (concentrate energy)
            }
            
            // Cannabinoid-Yield trade-off (very high cannabinoids can reduce yield)
            float totalCannabinoids = thc + cbd;
            if (totalCannabinoids > 25f) // High cannabinoid content
            {
                float reductionFactor = 1f - ((totalCannabinoids - 25f) / 50f * pleiotropicStrength);
                yield *= reductionFactor;
            }
            
            // Apply genetic complexity bonus for multiple trait genes
            int traitGenesCount = CountTraitInfluencingGenes(genotype);
            if (traitGenesCount > 3)
            {
                float complexityBonus = 1f + (traitGenesCount - 3) * 0.02f; // 2% bonus per additional gene
                
                // Apply small bonus to all traits for genetic complexity
                height *= Mathf.Min(complexityBonus, 1.1f);
                thc *= Mathf.Min(complexityBonus, 1.1f);
                cbd *= Mathf.Min(complexityBonus, 1.1f);
                yield *= Mathf.Min(complexityBonus, 1.1f);
            }
        }
        
        /// <summary>
        /// Count genes that influence multiple traits for complexity calculation.
        /// </summary>
        private int CountTraitInfluencingGenes(PlantGenotype genotype)
        {
            if (genotype?.Genotype == null) return 0;
            
            int count = 0;
            foreach (var kvp in genotype.Genotype)
            {
                var alleleCouple = kvp.Value;
                if (alleleCouple?.Allele1?.TraitEffects != null && alleleCouple.Allele1.TraitEffects.Count > 0)
                {
                    count++;
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// Calculate Quantitative Trait Loci (QTL) effects for polygenic traits.
        /// Phase 2.2: QTL system implementation for realistic polygenic inheritance.
        /// </summary>
        private float CalculateQTLEffects(PlantGenotype genotype, PlantTrait targetTrait, EnvironmentalConditions environment)
        {
            if (genotype?.Genotype == null) return 0f;
            
            var qtlEffects = new List<QTLEffect>();
            
            // Analyze each locus for QTL effects
            foreach (var kvp in genotype.Genotype)
            {
                var qtlEffect = AnalyzeLocusForQTL(kvp.Key, kvp.Value, targetTrait, environment);
                if (qtlEffect != null && qtlEffect.EffectSize != 0f)
                {
                    qtlEffects.Add(qtlEffect);
                }
            }
            
            if (qtlEffects.Count == 0) return 0f;
            
            // Sort QTL effects by magnitude (largest effect first)
            qtlEffects.Sort((a, b) => Mathf.Abs(b.EffectSize).CompareTo(Mathf.Abs(a.EffectSize)));
            
            // Calculate total QTL effect with diminishing returns for minor QTLs
            float totalEffect = 0f;
            for (int i = 0; i < qtlEffects.Count; i++)
            {
                float qtlContribution = qtlEffects[i].EffectSize;
                
                // Apply diminishing returns based on effect rank
                float diminishingFactor = CalculateQTLDiminishingReturns(i, qtlEffects.Count);
                qtlContribution *= diminishingFactor;
                
                // Apply QTL-specific environmental interactions
                qtlContribution *= CalculateQTLEnvironmentalInteraction(qtlEffects[i], environment);
                
                totalEffect += qtlContribution;
            }
            
            // Apply overall QTL architecture effects
            totalEffect *= CalculateQTLArchitectureModifier(qtlEffects, targetTrait);
            
            return totalEffect;
        }
        
        /// <summary>
        /// Analyze a single locus for QTL effects.
        /// </summary>
        private QTLEffect AnalyzeLocusForQTL(string locusId, AlleleCouple alleles, PlantTrait targetTrait, EnvironmentalConditions environment)
        {
            if (alleles?.Allele1 == null || alleles?.Allele2 == null) return null;
            
            // Calculate individual allele effects
            float allele1Effect = GetAlleleTraitEffect(alleles.Allele1, targetTrait, environment);
            float allele2Effect = GetAlleleTraitEffect(alleles.Allele2, targetTrait, environment);
            
            if (allele1Effect == 0f && allele2Effect == 0f) return null;
            
            // Determine QTL class based on effect magnitude
            float avgEffect = (Mathf.Abs(allele1Effect) + Mathf.Abs(allele2Effect)) * 0.5f;
            QTLClass qtlClass = ClassifyQTLByEffect(avgEffect);
            
            // Calculate additive and dominance effects
            float additiveEffect = (allele1Effect + allele2Effect) * 0.5f;
            float dominanceEffect = CalculateQTLDominanceEffect(alleles, allele1Effect, allele2Effect);
            
            return new QTLEffect
            {
                LocusID = locusId,
                TargetTrait = targetTrait,
                EffectSize = additiveEffect + dominanceEffect,
                AdditiveEffect = additiveEffect,
                DominanceEffect = dominanceEffect,
                QTLClass = qtlClass,
                EnvironmentalSensitivity = CalculateQTLEnvironmentalSensitivity(alleles, targetTrait),
                Heritability = CalculateQTLHeritability(qtlClass, targetTrait)
            };
        }
        
        /// <summary>
        /// Classify QTL by effect magnitude.
        /// </summary>
        private QTLClass ClassifyQTLByEffect(float effectMagnitude)
        {
            if (effectMagnitude >= 0.4f) return QTLClass.Major; // >40% effect
            if (effectMagnitude >= 0.15f) return QTLClass.Moderate; // 15-40% effect
            if (effectMagnitude >= 0.05f) return QTLClass.Minor; // 5-15% effect
            return QTLClass.Modifier; // <5% effect
        }
        
        /// <summary>
        /// Calculate QTL-specific dominance effects.
        /// </summary>
        private float CalculateQTLDominanceEffect(AlleleCouple alleles, float allele1Effect, float allele2Effect)
        {
            // If alleles have similar effects, dominance is minimal
            if (Mathf.Abs(allele1Effect - allele2Effect) < 0.1f)
                return 0f;
            
            // Determine dominance pattern
            var parentGene = alleles.Allele1?.ParentGene ?? alleles.Allele2?.ParentGene;
            if (parentGene == null) return 0f;
            
            float dominanceDeviation = 0f;
            switch (parentGene.DominanceType)
            {
                case DominanceType.Complete:
                    // Complete dominance: dominant allele fully expressed
                    dominanceDeviation = Mathf.Abs(Mathf.Max(allele1Effect, allele2Effect)) * 0.1f;
                    break;
                case DominanceType.Overdominant:
                    // Heterozygote advantage for QTLs
                    dominanceDeviation = (Mathf.Abs(allele1Effect) + Mathf.Abs(allele2Effect)) * 0.15f;
                    break;
                case DominanceType.Underdominant:
                    // Heterozygote disadvantage
                    dominanceDeviation = -(Mathf.Abs(allele1Effect) + Mathf.Abs(allele2Effect)) * 0.1f;
                    break;
            }
            
            return dominanceDeviation;
        }
        
        /// <summary>
        /// Calculate diminishing returns for QTL effects based on rank.
        /// </summary>
        private float CalculateQTLDiminishingReturns(int qtlRank, int totalQTLs)
        {
            // Major QTLs (first few) have full effect
            if (qtlRank == 0) return 1.0f; // Largest QTL - full effect
            if (qtlRank == 1) return 0.9f; // Second largest - 90% effect
            if (qtlRank == 2) return 0.8f; // Third largest - 80% effect
            
            // Minor QTLs have progressively reduced effects
            float reductionFactor = 0.7f - (qtlRank - 3) * 0.1f;
            return Mathf.Max(0.2f, reductionFactor); // Minimum 20% effect
        }
        
        /// <summary>
        /// Calculate QTL environmental sensitivity.
        /// </summary>
        private float CalculateQTLEnvironmentalSensitivity(AlleleCouple alleles, PlantTrait targetTrait)
        {
            float baseSensitivity = 0.3f; // Default sensitivity
            
            // Trait-specific environmental sensitivity
            switch (targetTrait)
            {
                case PlantTrait.FlowerYield:
                    baseSensitivity = 0.6f; // Yield highly environment-sensitive
                    break;
                case PlantTrait.THCContent:
                case PlantTrait.CBDContent:
                    baseSensitivity = 0.4f; // Cannabinoids moderately sensitive
                    break;
                case PlantTrait.Height:
                    baseSensitivity = 0.35f; // Height moderately sensitive
                    break;
            }
            
            // Allele-specific sensitivity modifications
            if (alleles?.Allele1?.EnvironmentallySensitive == true || 
                alleles?.Allele2?.EnvironmentallySensitive == true)
            {
                baseSensitivity *= 1.3f; // 30% increase for environmentally sensitive alleles
            }
            
            return Mathf.Clamp(baseSensitivity, 0.1f, 0.8f);
        }
        
        /// <summary>
        /// Calculate QTL heritability based on class and trait.
        /// </summary>
        private float CalculateQTLHeritability(QTLClass qtlClass, PlantTrait targetTrait)
        {
            float baseHeritability = 0.6f; // Default heritability
            
            // QTL class affects heritability
            switch (qtlClass)
            {
                case QTLClass.Major:
                    baseHeritability = 0.8f; // Major QTLs highly heritable
                    break;
                case QTLClass.Moderate:
                    baseHeritability = 0.7f;
                    break;
                case QTLClass.Minor:
                    baseHeritability = 0.5f;
                    break;
                case QTLClass.Modifier:
                    baseHeritability = 0.3f; // Modifier QTLs lowly heritable
                    break;
            }
            
            // Trait-specific heritability adjustments
            switch (targetTrait)
            {
                case PlantTrait.Height:
                    baseHeritability *= 1.1f; // Height generally highly heritable
                    break;
                case PlantTrait.FlowerYield:
                    baseHeritability *= 0.8f; // Yield less heritable due to environment
                    break;
                case PlantTrait.THCContent:
                case PlantTrait.CBDContent:
                    baseHeritability *= 0.9f; // Cannabinoids moderately heritable
                    break;
            }
            
            return Mathf.Clamp(baseHeritability, 0.2f, 0.9f);
        }
        
        /// <summary>
        /// Calculate QTL environmental interactions.
        /// </summary>
        private float CalculateQTLEnvironmentalInteraction(QTLEffect qtl, EnvironmentalConditions environment)
        {
            if (!environment.IsInitialized()) return 1f;
            
            float environmentalModifier = 1f;
            
            // Major QTLs are less sensitive to environment (more buffered)
            float sensitivityFactor = qtl.EnvironmentalSensitivity;
            if (qtl.QTLClass == QTLClass.Major)
                sensitivityFactor *= 0.7f; // Major QTLs 30% less sensitive
            else if (qtl.QTLClass == QTLClass.Modifier)
                sensitivityFactor *= 1.4f; // Modifier QTLs 40% more sensitive
            
            // Trait-specific environmental responses
            switch (qtl.TargetTrait)
            {
                case PlantTrait.FlowerYield:
                    environmentalModifier = CalculateYieldQTLEnvironmentalResponse(environment, sensitivityFactor);
                    break;
                case PlantTrait.THCContent:
                    environmentalModifier = CalculateCannabinoidQTLEnvironmentalResponse(environment, sensitivityFactor, true);
                    break;
                case PlantTrait.CBDContent:
                    environmentalModifier = CalculateCannabinoidQTLEnvironmentalResponse(environment, sensitivityFactor, false);
                    break;
                case PlantTrait.Height:
                    environmentalModifier = CalculateHeightQTLEnvironmentalResponse(environment, sensitivityFactor);
                    break;
            }
            
            return Mathf.Clamp(environmentalModifier, 0.6f, 1.5f);
        }
        
        /// <summary>
        /// Calculate yield QTL environmental response.
        /// </summary>
        private float CalculateYieldQTLEnvironmentalResponse(EnvironmentalConditions environment, float sensitivity)
        {
            float modifier = 1f;
            
            // Light response (most important for yield QTLs)
            if (environment.LightIntensity > 0)
            {
                float lightResponse = Mathf.Clamp(environment.LightIntensity / 800f, 0.3f, 1.4f);
                modifier *= Mathf.Lerp(1f, lightResponse, sensitivity * 0.8f);
            }
            
            // CO2 response
            if (environment.CO2Level > 400f)
            {
                float co2Response = 1f + ((environment.CO2Level - 400f) / 800f * 0.5f);
                modifier *= Mathf.Lerp(1f, co2Response, sensitivity * 0.6f);
            }
            
            // Temperature stress
            float tempOptimal = 26f;
            float tempStress = Mathf.Abs(environment.Temperature - tempOptimal) / 15f;
            modifier *= Mathf.Lerp(1f, 1f - tempStress * 0.3f, sensitivity);
            
            return modifier;
        }
        
        /// <summary>
        /// Calculate cannabinoid QTL environmental response.
        /// </summary>
        private float CalculateCannabinoidQTLEnvironmentalResponse(EnvironmentalConditions environment, float sensitivity, bool isTHC)
        {
            float modifier = 1f;
            
            // UV/Light stress response (increases cannabinoid production)
            if (environment.LightIntensity > 600f)
            {
                float lightStress = (environment.LightIntensity - 600f) / 400f;
                float cannabinoidBoost = 1f + lightStress * 0.2f; // Up to 20% boost from light stress
                modifier *= Mathf.Lerp(1f, cannabinoidBoost, sensitivity * 0.5f);
            }
            
            // Temperature stress (mild stress increases cannabinoids)
            float tempOptimal = isTHC ? 22f : 24f; // THC prefers slightly cooler temps
            float tempDeviation = Mathf.Abs(environment.Temperature - tempOptimal);
            if (tempDeviation > 2f && tempDeviation < 8f) // Mild stress range
            {
                float stressBoost = 1f + (tempDeviation - 2f) / 10f; // Small boost for mild stress
                modifier *= Mathf.Lerp(1f, stressBoost, sensitivity * 0.3f);
            }
            
            return modifier;
        }
        
        /// <summary>
        /// Calculate height QTL environmental response.
        /// </summary>
        private float CalculateHeightQTLEnvironmentalResponse(EnvironmentalConditions environment, float sensitivity)
        {
            float modifier = 1f;
            
            // Light response (affects height through photomorphogenesis)
            if (environment.LightIntensity > 0)
            {
                float lightResponse = Mathf.Clamp(environment.LightIntensity / 600f, 0.5f, 1.3f);
                modifier *= Mathf.Lerp(1f, lightResponse, sensitivity * 0.4f);
            }
            
            // Temperature response (affects cell elongation)
            float tempOptimal = 24f;
            float tempResponse = 1f - Mathf.Abs(environment.Temperature - tempOptimal) / 20f;
            modifier *= Mathf.Lerp(1f, Mathf.Max(0.6f, tempResponse), sensitivity * 0.3f);
            
            return modifier;
        }
        
        /// <summary>
        /// Calculate QTL architecture modifier based on overall genetic architecture.
        /// </summary>
        private float CalculateQTLArchitectureModifier(List<QTLEffect> qtlEffects, PlantTrait targetTrait)
        {
            if (qtlEffects.Count == 0) return 1f;
            
            float architectureModifier = 1f;
            
            // Count QTLs by class
            int majorQTLs = qtlEffects.Count(q => q.QTLClass == QTLClass.Major);
            int moderateQTLs = qtlEffects.Count(q => q.QTLClass == QTLClass.Moderate);
            int minorQTLs = qtlEffects.Count(q => q.QTLClass == QTLClass.Minor);
            int modifierQTLs = qtlEffects.Count(q => q.QTLClass == QTLClass.Modifier);
            
            // Optimal QTL architecture: few major QTLs + many minor QTLs
            if (majorQTLs >= 1 && majorQTLs <= 3 && minorQTLs >= 3)
            {
                architectureModifier *= 1.1f; // 10% bonus for good architecture
            }
            
            // Too many major QTLs can be unstable
            if (majorQTLs > 4)
            {
                architectureModifier *= 0.95f; // 5% penalty for too many major QTLs
            }
            
            // Modifier QTLs provide fine-tuning
            if (modifierQTLs > 2)
            {
                float modifierBonus = 1f + (modifierQTLs - 2) * 0.01f; // 1% per additional modifier
                architectureModifier *= Mathf.Min(modifierBonus, 1.05f); // Max 5% bonus
            }
            
            return Mathf.Clamp(architectureModifier, 0.9f, 1.15f);
        }
        
        /// <summary>
        /// Calculate comprehensive environmental stress response.
        /// Phase 2.3: Environmental stress system for realistic plant responses.
        /// </summary>
        private StressResponse CalculateEnvironmentalStressResponse(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            if (!environment.IsInitialized() || genotype?.Genotype == null)
            {
                return new StressResponse { OverallStressLevel = 0f, ActiveStresses = new List<StressFactor>() };
            }
            
            var stressResponse = new StressResponse
            {
                ActiveStresses = new List<StressFactor>(),
                StressResistance = CalculateGenotypeStressResistance(genotype),
                AdaptiveCapacity = CalculateAdaptiveCapacity(genotype)
            };
            
            // Analyze different stress types
            AnalyzeTemperatureStress(environment, stressResponse);
            AnalyzeLightStress(environment, stressResponse);
            AnalyzeWaterStress(environment, stressResponse);
            AnalyzeNutrientStress(environment, stressResponse);
            AnalyzeAtmosphericStress(environment, stressResponse);
            
            // Calculate overall stress level and responses
            CalculateOverallStressImpact(stressResponse);
            CalculateStressAdaptations(stressResponse, genotype);
            CalculateStressTolerance(stressResponse, genotype);
            
            return stressResponse;
        }
        
        /// <summary>
        /// Calculate genotype's inherent stress resistance.
        /// </summary>
        private float CalculateGenotypeStressResistance(PlantGenotype genotype)
        {
            float baseResistance = 0.5f; // Default resistance
            int resistanceGenes = 0;
            
            foreach (var kvp in genotype.Genotype)
            {
                string geneId = kvp.Key.ToUpper();
                var alleles = kvp.Value;
                
                // Look for stress resistance genes
                if (geneId.Contains("STRESS") || geneId.Contains("RESIST") || geneId.Contains("TOLERANCE") ||
                    geneId.Contains("HEAT") || geneId.Contains("COLD") || geneId.Contains("DROUGHT"))
                {
                    resistanceGenes++;
                    
                    // Calculate resistance contribution from this locus
                    float allele1Resistance = GetAlleleStressResistance(alleles?.Allele1);
                    float allele2Resistance = GetAlleleStressResistance(alleles?.Allele2);
                    baseResistance += (allele1Resistance + allele2Resistance) * 0.05f; // 5% per allele
                }
            }
            
            // Genetic diversity bonus (heterozygosity increases stress resistance)
            if (genotype.InbreedingCoefficient < 0.2f)
            {
                baseResistance += 0.1f; // Outbreeding bonus
            }
            
            // Strain-based resistance
            if (genotype.StrainOrigin != null)
            {
                string strainName = genotype.StrainOrigin.name?.ToLower() ?? "";
                if (strainName.Contains("ruderalis") || strainName.Contains("auto"))
                    baseResistance += 0.15f; // Ruderalis genetics = high stress resistance
                else if (strainName.Contains("indica"))
                    baseResistance += 0.05f; // Indica moderate resistance
            }
            
            return Mathf.Clamp(baseResistance, 0.2f, 0.9f);
        }
        
        /// <summary>
        /// Get stress resistance value from individual allele.
        /// </summary>
        private float GetAlleleStressResistance(AlleleSO allele)
        {
            if (allele?.TraitEffects == null) return 0f;
            
            float totalResistance = 0f;
            int resistanceTraits = 0;
            
            // Look for stress-related trait effects using actual PlantTrait enum values
            foreach (var effect in allele.TraitEffects)
            {
                // Check for specific stress tolerance traits that actually exist in PlantTrait enum
                if (effect.AffectedTrait == PlantTrait.HeatTolerance ||
                    effect.AffectedTrait == PlantTrait.ColdTolerance ||
                    effect.AffectedTrait == PlantTrait.DroughtTolerance ||
                    effect.AffectedTrait == PlantTrait.DiseaseResistance ||
                    effect.AffectedTrait == PlantTrait.PestResistance)
                {
                    totalResistance += effect.EffectMagnitude;
                    resistanceTraits++;
                }
            }
            
            // Return average resistance if any resistance traits found
            return resistanceTraits > 0 ? totalResistance / resistanceTraits : 0f;
        }
        
        /// <summary>
        /// Calculate adaptive capacity for long-term stress response.
        /// </summary>
        private float CalculateAdaptiveCapacity(PlantGenotype genotype)
        {
            float adaptiveCapacity = 0.3f; // Base adaptive capacity
            
            // Genetic complexity increases adaptive capacity
            int totalGenes = genotype.Genotype?.Count ?? 0;
            if (totalGenes > 5)
            {
                adaptiveCapacity += (totalGenes - 5) * 0.02f; // 2% per additional gene
            }
            
            // Heterozygosity increases adaptive flexibility
            float heterozygosity = 1f - genotype.InbreedingCoefficient;
            adaptiveCapacity += heterozygosity * 0.2f;
            
            return Mathf.Clamp(adaptiveCapacity, 0.1f, 0.7f);
        }
        
        /// <summary>
        /// Analyze temperature stress factors.
        /// </summary>
        private void AnalyzeTemperatureStress(EnvironmentalConditions environment, StressResponse response)
        {
            float optimalTemp = 24f; // Optimal temperature for cannabis
            float tempDeviation = Mathf.Abs(environment.Temperature - optimalTemp);
            
            if (tempDeviation > 3f) // Temperature stress threshold
            {
                StressType stressType = environment.Temperature > optimalTemp ? StressType.HeatStress : StressType.ColdStress;
                float stressIntensity = Mathf.Min(1f, (tempDeviation - 3f) / 12f); // Max stress at 15°C deviation
                
                var temperatureStress = new StressFactor
                {
                    StressType = stressType,
                    Intensity = stressIntensity,
                    Duration = 1f, // Will be tracked over time
                    TraitImpacts = CalculateTemperatureStressImpacts(stressType, stressIntensity)
                };
                
                response.ActiveStresses.Add(temperatureStress);
            }
        }
        
        /// <summary>
        /// Calculate trait impacts from temperature stress.
        /// </summary>
        private Dictionary<PlantTrait, float> CalculateTemperatureStressImpacts(StressType stressType, float intensity)
        {
            var impacts = new Dictionary<PlantTrait, float>();
            
            if (stressType == StressType.HeatStress)
            {
                impacts[PlantTrait.FlowerYield] = -intensity * 0.3f; // Heat reduces yield significantly
                impacts[PlantTrait.THCContent] = intensity * 0.1f;   // Mild heat can increase THC (stress response)
                impacts[PlantTrait.Height] = -intensity * 0.1f;     // Heat stress stunts growth
            }
            else if (stressType == StressType.ColdStress)
            {
                impacts[PlantTrait.Height] = -intensity * 0.2f;     // Cold stunts growth more
                impacts[PlantTrait.FlowerYield] = -intensity * 0.2f; // Cold reduces yield
                impacts[PlantTrait.THCContent] = -intensity * 0.15f; // Cold reduces cannabinoid production
                impacts[PlantTrait.CBDContent] = -intensity * 0.1f;
            }
            
            return impacts;
        }
        
        /// <summary>
        /// Analyze light stress factors.
        /// </summary>
        private void AnalyzeLightStress(EnvironmentalConditions environment, StressResponse response)
        {
            float optimalLight = 600f; // Optimal PPFD for cannabis
            
            // Light deficiency stress
            if (environment.LightIntensity < 200f)
            {
                float stressIntensity = (200f - environment.LightIntensity) / 200f;
                var lightDeficiency = new StressFactor
                {
                    StressType = StressType.LightDeficiency,
                    Intensity = stressIntensity,
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.Height] = stressIntensity * 0.5f,        // Etiolation - plants stretch
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.6f,  // Severe yield reduction
                        [PlantTrait.THCContent] = -stressIntensity * 0.4f,   // Reduced cannabinoid production
                        [PlantTrait.CBDContent] = -stressIntensity * 0.3f
                    }
                };
                response.ActiveStresses.Add(lightDeficiency);
            }
            
            // Light excess stress (light burn)
            else if (environment.LightIntensity > 1000f)
            {
                float stressIntensity = (environment.LightIntensity - 1000f) / 500f;
                var lightBurn = new StressFactor
                {
                    StressType = StressType.LightBurn,
                    Intensity = Mathf.Min(1f, stressIntensity),
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.Height] = -stressIntensity * 0.2f,       // Stunted growth from photodamage
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.3f,  // Yield reduction from damage
                        [PlantTrait.THCContent] = stressIntensity * 0.15f,   // UV stress can increase THC
                        [PlantTrait.CBDContent] = stressIntensity * 0.1f
                    }
                };
                response.ActiveStresses.Add(lightBurn);
            }
        }
        
        /// <summary>
        /// Analyze water stress factors.
        /// </summary>
        private void AnalyzeWaterStress(EnvironmentalConditions environment, StressResponse response)
        {
            float optimalHumidity = 55f; // Optimal relative humidity
            
            // Drought stress (low humidity as proxy)
            if (environment.Humidity < 30f)
            {
                float stressIntensity = (30f - environment.Humidity) / 30f;
                var droughtStress = new StressFactor
                {
                    StressType = StressType.DroughtStress,
                    Intensity = stressIntensity,
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.Height] = -stressIntensity * 0.3f,       // Reduced growth
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.5f,  // Severe yield impact
                        [PlantTrait.THCContent] = stressIntensity * 0.2f,    // Drought stress increases THC
                        [PlantTrait.CBDContent] = stressIntensity * 0.1f
                    }
                };
                response.ActiveStresses.Add(droughtStress);
            }
            
            // Humidity excess (fungal risk)
            else if (environment.Humidity > 75f)
            {
                float stressIntensity = (environment.Humidity - 75f) / 25f;
                var humidityStress = new StressFactor
                {
                    StressType = StressType.HighHumidity,
                    Intensity = stressIntensity,
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.4f,  // Mold/rot risk reduces yield
                        [PlantTrait.THCContent] = -stressIntensity * 0.1f,   // Quality degradation
                        [PlantTrait.CBDContent] = -stressIntensity * 0.1f
                    }
                };
                response.ActiveStresses.Add(humidityStress);
            }
        }
        
        /// <summary>
        /// Analyze nutrient stress factors (simplified - based on environmental proxies).
        /// </summary>
        private void AnalyzeNutrientStress(EnvironmentalConditions environment, StressResponse response)
        {
            // Simplified nutrient stress detection
            // In a full system, this would check actual nutrient levels
            
            // Use temperature as proxy for nutrient uptake issues
            if (environment.Temperature < 18f || environment.Temperature > 30f)
            {
                float stressIntensity = 0.3f; // Moderate nutrient stress from temperature
                var nutrientStress = new StressFactor
                {
                    StressType = StressType.NutrientStress,
                    Intensity = stressIntensity,
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.Height] = -stressIntensity * 0.2f,
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.3f,
                        [PlantTrait.THCContent] = -stressIntensity * 0.1f,
                        [PlantTrait.CBDContent] = -stressIntensity * 0.1f
                    }
                };
                response.ActiveStresses.Add(nutrientStress);
            }
        }
        
        /// <summary>
        /// Analyze atmospheric stress factors.
        /// </summary>
        private void AnalyzeAtmosphericStress(EnvironmentalConditions environment, StressResponse response)
        {
            // CO2 deficiency stress
            if (environment.CO2Level < 300f)
            {
                float stressIntensity = (300f - environment.CO2Level) / 100f;
                var co2Stress = new StressFactor
                {
                    StressType = StressType.CO2Deficiency,
                    Intensity = Mathf.Min(1f, stressIntensity),
                    Duration = 1f,
                    TraitImpacts = new Dictionary<PlantTrait, float>
                    {
                        [PlantTrait.Height] = -stressIntensity * 0.2f,
                        [PlantTrait.FlowerYield] = -stressIntensity * 0.4f,  // CO2 crucial for photosynthesis
                        [PlantTrait.THCContent] = -stressIntensity * 0.1f,
                        [PlantTrait.CBDContent] = -stressIntensity * 0.1f
                    }
                };
                response.ActiveStresses.Add(co2Stress);
            }
        }
        
        /// <summary>
        /// Calculate overall stress impact and interactions.
        /// </summary>
        private void CalculateOverallStressImpact(StressResponse response)
        {
            if (response.ActiveStresses.Count == 0)
            {
                response.OverallStressLevel = 0f;
                response.StressCategory = StressCategory.Optimal;
                return;
            }
            
            // Calculate weighted stress level
            float totalStress = 0f;
            float maxStress = 0f;
            
            foreach (var stress in response.ActiveStresses)
            {
                totalStress += stress.Intensity;
                maxStress = Mathf.Max(maxStress, stress.Intensity);
            }
            
            // Overall stress is between average and maximum (stress interactions)
            response.OverallStressLevel = (totalStress / response.ActiveStresses.Count + maxStress) * 0.5f;
            
            // Stress interactions (multiple stresses compound)
            if (response.ActiveStresses.Count > 1)
            {
                float interactionMultiplier = 1f + (response.ActiveStresses.Count - 1) * 0.15f; // 15% per additional stress
                response.OverallStressLevel *= interactionMultiplier;
            }
            
            response.OverallStressLevel = Mathf.Clamp01(response.OverallStressLevel);
            
            // Categorize stress level
            if (response.OverallStressLevel < 0.2f) response.StressCategory = StressCategory.Optimal;
            else if (response.OverallStressLevel < 0.4f) response.StressCategory = StressCategory.Mild;
            else if (response.OverallStressLevel < 0.7f) response.StressCategory = StressCategory.Moderate;
            else if (response.OverallStressLevel < 0.9f) response.StressCategory = StressCategory.Severe;
            else response.StressCategory = StressCategory.Critical;
        }
        
        /// <summary>
        /// Calculate stress-induced adaptations.
        /// </summary>
        private void CalculateStressAdaptations(StressResponse response, PlantGenotype genotype)
        {
            response.AdaptationEffects = new Dictionary<PlantTrait, float>();
            
            if (response.OverallStressLevel < 0.1f) return; // No adaptations under minimal stress
            
            float adaptationStrength = response.AdaptiveCapacity * response.OverallStressLevel;
            
            // Stress hardening effects (mild stress can improve resistance)
            if (response.OverallStressLevel > 0.1f && response.OverallStressLevel < 0.4f)
            {
                // Mild stress increases cannabinoid production (defense compounds)
                response.AdaptationEffects[PlantTrait.THCContent] = adaptationStrength * 0.1f;
                response.AdaptationEffects[PlantTrait.CBDContent] = adaptationStrength * 0.05f;
                
                // Stress hardening improves future stress resistance
                response.HardeningEffect = adaptationStrength * 0.2f;
            }
            
            // Severe stress triggers survival adaptations
            else if (response.OverallStressLevel > 0.6f)
            {
                // Resource reallocation - reduced growth, increased survival compounds
                response.AdaptationEffects[PlantTrait.Height] = -adaptationStrength * 0.3f;
                response.AdaptationEffects[PlantTrait.FlowerYield] = -adaptationStrength * 0.2f;
                response.AdaptationEffects[PlantTrait.THCContent] = adaptationStrength * 0.15f; // Stress compounds
            }
        }
        
        /// <summary>
        /// Calculate genotype-specific stress tolerance.
        /// </summary>
        private void CalculateStressTolerance(StressResponse response, PlantGenotype genotype)
        {
            // Apply genotype stress resistance to reduce overall stress impact
            float toleranceReduction = response.StressResistance * 0.4f; // Up to 40% stress reduction
            response.OverallStressLevel *= (1f - toleranceReduction);
            response.OverallStressLevel = Mathf.Clamp01(response.OverallStressLevel);
            
            // Calculate tolerance bonuses for specific stress types
            response.StressToleranceBonuses = new Dictionary<StressType, float>();
            
            // Check for specific resistance genes
            foreach (var kvp in genotype.Genotype)
            {
                string geneId = kvp.Key.ToUpper();
                
                if (geneId.Contains("HEAT")) response.StressToleranceBonuses[StressType.HeatStress] = 0.3f;
                if (geneId.Contains("COLD")) response.StressToleranceBonuses[StressType.ColdStress] = 0.3f;
                if (geneId.Contains("DROUGHT")) response.StressToleranceBonuses[StressType.DroughtStress] = 0.4f;
                if (geneId.Contains("LIGHT")) response.StressToleranceBonuses[StressType.LightBurn] = 0.2f;
            }
        }
        
        /// <summary>
        /// Apply stress response effects to trait expression.
        /// </summary>
        private float ApplyStressResponseToTrait(float baseTraitValue, PlantTrait trait, StressResponse stressResponse)
        {
            if (stressResponse.OverallStressLevel < 0.05f) return baseTraitValue; // No significant stress
            
            float modifiedValue = baseTraitValue;
            
            // Apply direct stress impacts
            foreach (var stressFactor in stressResponse.ActiveStresses)
            {
                if (stressFactor.TraitImpacts.ContainsKey(trait))
                {
                    float impact = stressFactor.TraitImpacts[trait];
                    
                    // Apply stress tolerance bonuses
                    if (stressResponse.StressToleranceBonuses.ContainsKey(stressFactor.StressType))
                    {
                        impact *= (1f - stressResponse.StressToleranceBonuses[stressFactor.StressType]);
                    }
                    
                    modifiedValue += modifiedValue * impact; // Proportional impact
                }
            }
            
            // Apply adaptive effects
            if (stressResponse.AdaptationEffects.ContainsKey(trait))
            {
                modifiedValue += modifiedValue * stressResponse.AdaptationEffects[trait];
            }
            
            return Mathf.Max(0f, modifiedValue); // Ensure non-negative values
        }
        
        /// <summary>
        /// Create summary of environmental factors for result tracking.
        /// </summary>
        private string GetEnvironmentalFactorSummary(EnvironmentalConditions environment)
        {
            if (!environment.IsInitialized()) return "No environmental data";
            
            return $"Temp: {environment.Temperature:F1}°C, Light: {environment.LightIntensity:F0} PPFD, CO2: {environment.CO2Level:F0} ppm";
        }
    }
    
    /// <summary>
    /// Comprehensive trait expression result with multiple trait implementation.
    /// Phase 1.2: Multi-trait system including Height, THC, CBD, and Yield
    /// </summary>
    [System.Serializable]
    public class TraitExpressionResult
    {
        [Header("Basic Information")]
        public string GenotypeID;
        public float OverallFitness;
        
        [Header("Physical Traits")]
        public float HeightExpression; // Calculated height in meters
        public float YieldExpression; // Calculated yield in grams
        
        [Header("Cannabinoid Traits")]
        public float THCExpression; // Calculated THC percentage
        public float CBDExpression; // Calculated CBD percentage
        
        [Header("Calculation Metadata")]
        public DateTime CalculationTimestamp;
        public string EnvironmentalFactors;
        public int TraitCount; // Number of traits calculated
        
        [Header("Genetic Complexity")]
        public bool HasEpistasis; // Whether epistatic interactions were considered
        public bool HasPleiotropy; // Whether pleiotropic effects were considered
        
        [Header("Environmental Stress")]
        public StressResponse StressResponse; // Environmental stress analysis and effects
        
        /// <summary>
        /// Get height expression in centimeters for display purposes.
        /// </summary>
        public float GetHeightInCentimeters()
        {
            return HeightExpression * 100f;
        }
        
        /// <summary>
        /// Get height category for classification.
        /// </summary>
        public string GetHeightCategory()
        {
            if (HeightExpression < 0.8f) return "Dwarf";
            if (HeightExpression < 1.2f) return "Short";
            if (HeightExpression < 1.8f) return "Medium";
            if (HeightExpression < 2.3f) return "Tall";
            return "Giant";
        }
        
        /// <summary>
        /// Get fitness rating for display.
        /// </summary>
        public string GetFitnessRating()
        {
            if (OverallFitness >= 0.9f) return "Excellent";
            if (OverallFitness >= 0.75f) return "Good";
            if (OverallFitness >= 0.6f) return "Average";
            if (OverallFitness >= 0.4f) return "Poor";
            return "Critical";
        }
        
        /// <summary>
        /// Get THC category for classification.
        /// </summary>
        public string GetTHCCategory()
        {
            if (THCExpression < 5f) return "Low THC";
            if (THCExpression < 15f) return "Moderate THC";
            if (THCExpression < 25f) return "High THC";
            if (THCExpression < 30f) return "Very High THC";
            return "Extreme THC";
        }
        
        /// <summary>
        /// Get CBD category for classification.
        /// </summary>
        public string GetCBDCategory()
        {
            if (CBDExpression < 1f) return "THC-Dominant";
            if (CBDExpression < 5f) return "Low CBD";
            if (CBDExpression < 10f) return "Moderate CBD";
            if (CBDExpression < 20f) return "High CBD";
            return "CBD-Dominant";
        }
        
        /// <summary>
        /// Get yield category for classification.
        /// </summary>
        public string GetYieldCategory()
        {
            if (YieldExpression < 200f) return "Low Yield";
            if (YieldExpression < 400f) return "Moderate Yield";
            if (YieldExpression < 600f) return "Good Yield";
            if (YieldExpression < 800f) return "High Yield";
            return "Exceptional Yield";
        }
        
        /// <summary>
        /// Get THC:CBD ratio for chemotype classification.
        /// </summary>
        public string GetChemotype()
        {
            float totalCannabinoids = THCExpression + CBDExpression;
            if (totalCannabinoids < 1f) return "Unknown";
            
            float thcRatio = THCExpression / totalCannabinoids;
            
            if (thcRatio > 0.8f) return "Type I (THC-Dominant)";
            if (thcRatio > 0.3f) return "Type II (Balanced)";
            return "Type III (CBD-Dominant)";
        }
        
        /// <summary>
        /// Get overall trait quality assessment.
        /// </summary>
        public string GetOverallQuality()
        {
            float qualityScore = 0f;
            
            // Height quality (moderate heights score better)
            float heightScore = HeightExpression > 1.0f && HeightExpression < 2.0f ? 1f : 0.7f;
            qualityScore += heightScore * 0.2f;
            
            // THC quality (higher is generally better, but with limits)
            float thcScore = Mathf.Clamp01(THCExpression / 25f);
            qualityScore += thcScore * 0.3f;
            
            // CBD quality (any CBD is good)
            float cbdScore = Mathf.Min(1f, CBDExpression / 10f);
            qualityScore += cbdScore * 0.2f;
            
            // Yield quality (higher is better)
            float yieldScore = Mathf.Clamp01(YieldExpression / 600f);
            qualityScore += yieldScore * 0.3f;
            
            if (qualityScore >= 0.8f) return "Premium";
            if (qualityScore >= 0.6f) return "High Quality";
            if (qualityScore >= 0.4f) return "Standard";
            if (qualityScore >= 0.2f) return "Basic";
            return "Poor";
        }
        
        /// <summary>
        /// Get yield efficiency (yield per unit height).
        /// </summary>
        public float GetYieldEfficiency()
        {
            return HeightExpression > 0 ? YieldExpression / HeightExpression : 0f;
        }
        
        /// <summary>
        /// Get cannabinoid density (total cannabinoids).
        /// </summary>
        public float GetCannabinoidDensity()
        {
            return THCExpression + CBDExpression;
        }
        
        /// <summary>
        /// Check if plant has balanced cannabinoid profile.
        /// </summary>
        public bool HasBalancedCannabinoids()
        {
            float total = THCExpression + CBDExpression;
            if (total < 5f) return false;
            
            float thcRatio = THCExpression / total;
            return thcRatio > 0.3f && thcRatio < 0.7f; // 30-70% THC is considered balanced
        }
        
        /// <summary>
        /// Get stress level description.
        /// </summary>
        public string GetStressLevelDescription()
        {
            if (StressResponse == null) return "No stress data";
            
            string description = $"{StressResponse.StressCategory} ({StressResponse.OverallStressLevel:P0})";
            
            if (StressResponse.ActiveStresses?.Count > 0)
            {
                description += $" - {StressResponse.ActiveStresses.Count} stress factor(s)";
            }
            
            return description;
        }
        
        /// <summary>
        /// Get dominant stress type affecting the plant.
        /// </summary>
        public string GetDominantStressType()
        {
            if (StressResponse?.ActiveStresses == null || StressResponse.ActiveStresses.Count == 0)
                return "None";
            
            var dominantStress = StressResponse.ActiveStresses
                .OrderByDescending(s => s.Intensity)
                .FirstOrDefault();
            
            return dominantStress?.StressType.ToString() ?? "Unknown";
        }
        
        /// <summary>
        /// Check if plant is stress hardened (benefits from mild stress).
        /// </summary>
        public bool IsStressHardened()
        {
            return StressResponse?.HardeningEffect > 0.1f;
        }
        
        /// <summary>
        /// Get environmental adaptability rating.
        /// </summary>
        public string GetAdaptabilityRating()
        {
            if (StressResponse == null) return "Unknown";
            
            float adaptability = StressResponse.AdaptiveCapacity * StressResponse.StressResistance;
            
            if (adaptability >= 0.6f) return "Excellent";
            if (adaptability >= 0.4f) return "Good";
            if (adaptability >= 0.25f) return "Average";
            if (adaptability >= 0.15f) return "Poor";
            return "Very Poor";
        }
        
        /// <summary>
        /// Check if calculation is recent (within last hour).
        /// </summary>
        public bool IsCalculationRecent()
        {
            return (DateTime.Now - CalculationTimestamp).TotalHours < 1.0;
        }
    }
    
    /// <summary>
    /// Quantitative Trait Loci (QTL) effect data structure.
    /// Phase 2.2: QTL system for polygenic trait modeling.
    /// </summary>
    [System.Serializable]
    public class QTLEffect
    {
        public string LocusID;
        public PlantTrait TargetTrait;
        public float EffectSize; // Total genetic effect (additive + dominance)
        public float AdditiveEffect; // Additive genetic component
        public float DominanceEffect; // Dominance deviation component
        public QTLClass QTLClass; // Classification by effect magnitude
        public float EnvironmentalSensitivity; // How sensitive this QTL is to environment
        public float Heritability; // Narrow-sense heritability of this QTL
    }
    
    /// <summary>
    /// QTL classification based on effect magnitude.
    /// </summary>
    public enum QTLClass
    {
        Major,      // >40% of trait variance explained
        Moderate,   // 15-40% of trait variance explained
        Minor,      // 5-15% of trait variance explained
        Modifier    // <5% of trait variance explained
    }
    
    /// <summary>
    /// Environmental stress response data structure.
    /// Phase 2.3: Stress response system for realistic plant responses.
    /// </summary>
    [System.Serializable]
    public class StressResponse
    {
        public float OverallStressLevel; // 0-1 overall stress magnitude
        public StressCategory StressCategory; // Categorized stress level
        public List<StressFactor> ActiveStresses; // Individual stress factors
        public float StressResistance; // Genotype's inherent stress resistance
        public float AdaptiveCapacity; // Ability to adapt to stress
        public float HardeningEffect; // Stress hardening bonus for future resistance
        public Dictionary<PlantTrait, float> AdaptationEffects; // Stress-induced trait changes
        public Dictionary<StressType, float> StressToleranceBonuses; // Gene-specific resistance bonuses
    }
    
    /// <summary>
    /// Individual stress factor affecting the plant.
    /// </summary>
    [System.Serializable]
    public class StressFactor
    {
        public StressType StressType; // Type of stress
        public float Intensity; // 0-1 stress magnitude
        public float Duration; // How long stress has been active
        public Dictionary<PlantTrait, float> TraitImpacts; // Trait-specific impacts
    }
    
    /// <summary>
    /// Types of environmental stress.
    /// </summary>
    public enum StressType
    {
        HeatStress,      // High temperature stress
        ColdStress,      // Low temperature stress
        LightDeficiency, // Insufficient light
        LightBurn,       // Excessive light/UV damage
        DroughtStress,   // Water deficiency
        HighHumidity,    // Excessive humidity (mold risk)
        NutrientStress,  // Nutrient deficiency or toxicity
        CO2Deficiency,   // Insufficient CO2
        SaltStress,      // High salinity
        PhStress         // pH imbalance
    }
    
    /// <summary>
    /// Stress level categories.
    /// </summary>
    public enum StressCategory
    {
        Optimal,   // No significant stress (0-20%)
        Mild,      // Minor stress (20-40%)
        Moderate,  // Moderate stress (40-70%)
        Severe,    // Severe stress (70-90%)
        Critical   // Critical stress (90-100%)
    }
}