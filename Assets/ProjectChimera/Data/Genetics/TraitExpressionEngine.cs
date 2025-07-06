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
        
        public TraitExpressionEngine(bool enableEpistasis, bool enablePleiotropy)
        {
            _enableEpistasis = enableEpistasis;
            _enablePleiotropy = enablePleiotropy;
        }
        
        /// <summary>
        /// Calculate trait expression including Height trait with full genetic and environmental interaction.
        /// Phase 0.2 Implementation: Single trait (Height) focus for genetic foundation.
        /// </summary>
        public TraitExpressionResult CalculateExpression(PlantGenotype genotype, EnvironmentalConditions environment)
        {
            try
            {
                // Calculate Height trait as primary implementation
                float heightExpression = CalculateHeightTrait(genotype, environment);
                
                // Calculate overall fitness based on trait harmony
                float overallFitness = CalculateOverallFitness(genotype, environment, heightExpression);
                
                return new TraitExpressionResult
                {
                    GenotypeID = genotype.GenotypeID,
                    OverallFitness = overallFitness,
                    HeightExpression = heightExpression,
                    CalculationTimestamp = DateTime.Now,
                    EnvironmentalFactors = GetEnvironmentalFactorSummary(environment),
                    TraitCount = 1, // Currently implementing single trait (Height)
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
        /// Get height effect from individual allele, with environmental interaction.
        /// </summary>
        private float GetAlleleHeightEffect(AlleleSO allele, EnvironmentalConditions environment)
        {
            if (allele?.TraitEffects == null || allele.TraitEffects.Count == 0)
                return 0f;
            
            // Find Height trait effects
            foreach (var traitEffect in allele.TraitEffects)
            {
                if (traitEffect.AffectedTrait == PlantTrait.Height)
                {
                    float baseEffect = traitEffect.EffectMagnitude;
                    
                    // Apply environmental modulation if allele supports it
                    if (allele.EnvironmentallySensitive && environment.IsInitialized())
                    {
                        float envModifier = CalculateEnvironmentalAlleleModifier(allele, environment);
                        baseEffect *= envModifier;
                    }
                    
                    return baseEffect;
                }
            }
            
            return 0f;
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
        private float CalculateEnvironmentalAlleleModifier(AlleleSO allele, EnvironmentalConditions environment)
        {
            // Simplified environmental response - could be expanded with allele-specific modifiers
            float baseModifier = 1f;
            
            // Some alleles might be more sensitive to environmental changes
            if (allele.EnvironmentalModifiers?.Count > 0)
            {
                // Could implement specific environmental response curves here
                // For now, apply a small random variation based on allele properties
                baseModifier += UnityEngine.Random.Range(-0.1f, 0.1f);
            }
            
            return Mathf.Clamp(baseModifier, 0.8f, 1.2f);
        }
        
        /// <summary>
        /// Calculate overall fitness based on trait harmony and environmental adaptation.
        /// </summary>
        private float CalculateOverallFitness(PlantGenotype genotype, EnvironmentalConditions environment, float heightExpression)
        {
            float fitness = 0.8f; // Base fitness
            
            // Height contributes to fitness (moderate height is often optimal)
            float optimalHeight = GetStrainBaseHeight(genotype);
            float heightDeviation = Mathf.Abs(heightExpression - optimalHeight) / optimalHeight;
            float heightFitness = Mathf.Max(0.5f, 1f - heightDeviation);
            
            fitness += heightFitness * 0.3f; // Height contributes 30% to fitness
            
            // Environmental adaptation contributes to fitness
            float envAdaptation = CalculateEnvironmentalHeightModifier(environment);
            fitness += (envAdaptation - 1f) * 0.2f; // Environmental adaptation contributes 20%
            
            // Genetic diversity bonus (if not too inbred)
            if (genotype.InbreedingCoefficient < 0.25f)
            {
                fitness += 0.1f; // Outbreeding bonus
            }
            
            return Mathf.Clamp(fitness, 0.1f, 1.0f);
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
    /// Enhanced trait expression result with Height trait implementation.
    /// Phase 0.2: Core Genetic Engine - Phase 1 Result Structure
    /// </summary>
    [System.Serializable]
    public class TraitExpressionResult
    {
        [Header("Basic Information")]
        public string GenotypeID;
        public float OverallFitness;
        
        [Header("Height Trait Expression")]
        public float HeightExpression; // Calculated height in meters
        
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
        /// Check if calculation is recent (within last hour).
        /// </summary>
        public bool IsCalculationRecent()
        {
            return (DateTime.Now - CalculationTimestamp).TotalHours < 1.0;
        }
    }
}