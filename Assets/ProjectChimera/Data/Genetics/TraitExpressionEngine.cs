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
                
                // Calculate overall fitness based on all traits
                float overallFitness = CalculateOverallFitness(genotype, environment, heightExpression, thcExpression, cbdExpression, yieldExpression);
                
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
                    HasPleiotropy = _enablePleiotropy
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
                    float epistasisModifier = CalculateEpistasisEffect(dominanceEffects, heightGenesFound);
                    geneticModifier *= epistasisModifier;
                }
            }
            
            // Apply environmental modifiers
            float environmentalModifier = CalculateEnvironmentalHeightModifier(environment);
            
            // Calculate final height expression
            float finalHeight = baseHeight * geneticModifier * environmentalModifier;
            
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
                    float epistasisModifier = CalculateEpistasisEffect(dominanceEffects, thcGenesFound);
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
                    float epistasisModifier = CalculateEpistasisEffect(dominanceEffects, cbdGenesFound);
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
                    float epistasisModifier = CalculateEpistasisEffect(dominanceEffects, yieldGenesFound);
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
        /// Calculate epistatic interactions between multiple height genes.
        /// </summary>
        private float CalculateEpistasisEffect(float dominanceEffects, int geneCount)
        {
            if (geneCount < 2) return 1f;
            
            // Simple epistasis model: interaction strength increases with gene number
            float interactionStrength = dominanceEffects / geneCount;
            float epistasisModifier = 1f + (interactionStrength * 0.15f); // 15% max epistatic effect
            
            return Mathf.Clamp(epistasisModifier, 0.8f, 1.3f);
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
        /// Check if calculation is recent (within last hour).
        /// </summary>
        public bool IsCalculationRecent()
        {
            return (DateTime.Now - CalculationTimestamp).TotalHours < 1.0;
        }
    }
}