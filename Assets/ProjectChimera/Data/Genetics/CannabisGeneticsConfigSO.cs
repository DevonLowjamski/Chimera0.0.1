using UnityEngine;
using ProjectChimera.Core;
using PlantTrait = ProjectChimera.Data.Genetics.TraitType;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Configuration settings for the cannabis genetics engine.
    /// Controls genetic simulation parameters, mutation rates, and inheritance patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "CannabisGeneticsConfig", menuName = "Project Chimera/Genetics/Cannabis Genetics Config")]
    public class CannabisGeneticsConfigSO : ChimeraConfigSO
    {
        [Header("Genetic Simulation")]
        [SerializeField] private bool _enableMendelianInheritance = true;
        [SerializeField] private bool _enablePolygeneticTraits = true;
        [SerializeField] private bool _enableEpigenetics = true;
        [SerializeField] private bool _enableGeneticDrift = false;
        [SerializeField] private bool _enableLinkageMapping = true;

        [Header("Mutation Settings")]
        [SerializeField, Range(0f, 0.01f)] private float _baseMutationRate = 0.001f;
        [SerializeField, Range(0f, 0.1f)] private float _environmentalMutationModifier = 0.02f;
        [SerializeField] private bool _enableBeneficialMutations = true;
        [SerializeField] private bool _enableLethalMutations = true;
        [SerializeField, Range(0f, 1f)] private float _mutationReversibility = 0.1f;

        [Header("Inheritance Patterns")]
        [SerializeField, Range(0f, 1f)] private float _dominanceVariability = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _recessiveExpressionThreshold = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _codominanceThreshold = 0.4f;
        [SerializeField] private bool _enableOverdominance = true;
        [SerializeField, Range(0f, 2f)] private float _heterosisMultiplier = 1.2f;

        [Header("Environmental Genetics")]
        [SerializeField] private bool _enableGxEInteractions = true;
        [SerializeField, Range(0f, 1f)] private float _environmentalSensitivity = 0.5f;
        [SerializeField, Range(0f, 0.1f)] private float _phenotypicPlasticityRate = 0.02f;
        [SerializeField] private int _adaptationGenerations = 5;
        [SerializeField, Range(0f, 1f)] private float _environmentalStressThreshold = 0.7f;

        [Header("Genetic Diversity")]
        [SerializeField, Range(0f, 1f)] private float _minimumGeneticDiversity = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _maximumInbreedingCoefficient = 0.8f;
        [SerializeField] private int _founderPopulationSize = 50;
        [SerializeField] private bool _maintainGeneticDiversity = true;

        [Header("Population Genetics")]
        [SerializeField] private int _effectivePopulationSize = 100;
        [SerializeField, Range(0f, 1f)] private float _geneticBottleneckThreshold = 0.1f;
        [SerializeField] private bool _enablePopulationStructure = false;
        [SerializeField] private int _migrationRate = 5; // individuals per generation

        [Header("Quantitative Genetics")]
        [SerializeField] private bool _enableQuantitativeTraits = true;
        [SerializeField, Range(0f, 1f)] private float _heritabilityCoefficient = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _geneticCorrelationStrength = 0.5f;
        [SerializeField] private int _polygeneticTraitLoci = 10;

        [Header("Breeding Parameters")]
        [SerializeField] private int _maxBreedingGenerations = 20;
        [SerializeField, Range(0f, 1f)] private float _selectionIntensity = 0.8f;
        [SerializeField] private bool _enableAssortativeMating = false;
        [SerializeField, Range(0f, 1f)] private float _inbreedingTolerance = 0.5f;

        [Header("Genetic Algorithms")]
        [SerializeField] private bool _enableGeneticOptimization = true;
        [SerializeField] private int _optimizationPopulationSize = 50;
        [SerializeField, Range(0f, 1f)] private float _crossoverRate = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _elitismRate = 0.1f;

        [Header("Performance Settings")]
        [SerializeField] private bool _enableMultithreading = true;
        [SerializeField] private int _maxConcurrentCalculations = 4;
        [SerializeField] private bool _enableGeneticCaching = true;
        [SerializeField] private int _cacheSize = 1000;

        [Header("Validation and Quality Control")]
        [SerializeField] private bool _enableGeneticValidation = true;
        [SerializeField] private bool _enableFitnessChecking = true;
        [SerializeField] private bool _enableLethalityChecking = true;
        [SerializeField, Range(0f, 1f)] private float _minimumViabilityThreshold = 0.3f;

        // Public Properties
        public bool EnableMendelianInheritance => _enableMendelianInheritance;
        public bool EnablePolygeneticTraits => _enablePolygeneticTraits;
        public bool EnableEpigenetics => _enableEpigenetics;
        public bool EnableGeneticDrift => _enableGeneticDrift;
        public bool EnableLinkageMapping => _enableLinkageMapping;

        public float BaseMutationRate => _baseMutationRate;
        public float EnvironmentalMutationModifier => _environmentalMutationModifier;
        public bool EnableBeneficialMutations => _enableBeneficialMutations;
        public bool EnableLethalMutations => _enableLethalMutations;
        public float MutationReversibility => _mutationReversibility;

        public float DominanceVariability => _dominanceVariability;
        public float RecessiveExpressionThreshold => _recessiveExpressionThreshold;
        public float CodominanceThreshold => _codominanceThreshold;
        public bool EnableOverdominance => _enableOverdominance;
        public float HeterosisMultiplier => _heterosisMultiplier;

        public bool EnableGxEInteractions => _enableGxEInteractions;
        public float EnvironmentalSensitivity => _environmentalSensitivity;
        public float PhenotypicPlasticityRate => _phenotypicPlasticityRate;
        public int AdaptationGenerations => _adaptationGenerations;
        public float EnvironmentalStressThreshold => _environmentalStressThreshold;

        public float MinimumGeneticDiversity => _minimumGeneticDiversity;
        public float MaximumInbreedingCoefficient => _maximumInbreedingCoefficient;
        public int FounderPopulationSize => _founderPopulationSize;
        public bool MaintainGeneticDiversity => _maintainGeneticDiversity;

        public int EffectivePopulationSize => _effectivePopulationSize;
        public float GeneticBottleneckThreshold => _geneticBottleneckThreshold;
        public bool EnablePopulationStructure => _enablePopulationStructure;
        public int MigrationRate => _migrationRate;

        public bool EnableQuantitativeTraits => _enableQuantitativeTraits;
        public float HeritabilityCoefficient => _heritabilityCoefficient;
        public float GeneticCorrelationStrength => _geneticCorrelationStrength;
        public int PolygeneticTraitLoci => _polygeneticTraitLoci;

        public int MaxBreedingGenerations => _maxBreedingGenerations;
        public float SelectionIntensity => _selectionIntensity;
        public bool EnableAssortativeMating => _enableAssortativeMating;
        public float InbreedingTolerance => _inbreedingTolerance;

        public bool EnableGeneticOptimization => _enableGeneticOptimization;
        public int OptimizationPopulationSize => _optimizationPopulationSize;
        public float CrossoverRate => _crossoverRate;
        public float ElitismRate => _elitismRate;

        public bool EnableMultithreading => _enableMultithreading;
        public int MaxConcurrentCalculations => _maxConcurrentCalculations;
        public bool EnableGeneticCaching => _enableGeneticCaching;
        public int CacheSize => _cacheSize;

        public bool EnableGeneticValidation => _enableGeneticValidation;
        public bool EnableFitnessChecking => _enableFitnessChecking;
        public bool EnableLethalityChecking => _enableLethalityChecking;
        public float MinimumViabilityThreshold => _minimumViabilityThreshold;

        /// <summary>
        /// Calculate effective mutation rate based on environmental conditions
        /// </summary>
        public float CalculateEffectiveMutationRate(float environmentalStress)
        {
            float stressModifier = environmentalStress > _environmentalStressThreshold ? 
                _environmentalMutationModifier : 0f;
            return _baseMutationRate * (1f + stressModifier);
        }

        /// <summary>
        /// Determine if a trait should use quantitative genetics
        /// </summary>
        public bool ShouldUseQuantitativeGenetics(PlantTrait trait)
        {
            if (!_enableQuantitativeTraits) return false;

            // Quantitative traits are typically continuous and polygenic
            return trait switch
            {
                PlantTrait.Height => true,
                PlantTrait.Width => true,
                PlantTrait.TotalBiomass => true,
                PlantTrait.FlowerYield => true,
                PlantTrait.THCContent => true,
                PlantTrait.CBDContent => true,
                PlantTrait.GrowthRate => true,
                PlantTrait.PhotosyntheticEfficiency => true,
                _ => false
            };
        }

        /// <summary>
        /// Calculate selection pressure for a trait
        /// </summary>
        public float CalculateSelectionPressure(PlantTrait trait, float currentValue, float targetValue)
        {
            float difference = Mathf.Abs(targetValue - currentValue);
            float normalizedDifference = difference / targetValue;
            return normalizedDifference * _selectionIntensity;
        }

        /// <summary>
        /// Determine if genetic diversity is below threshold
        /// </summary>
        public bool IsGeneticDiversityLow(float currentDiversity)
        {
            return currentDiversity < _minimumGeneticDiversity;
        }

        /// <summary>
        /// Calculate inbreeding depression effect
        /// </summary>
        public float CalculateInbreedingDepression(float inbreedingCoefficient)
        {
            if (inbreedingCoefficient <= _inbreedingTolerance) return 1f;
            
            float excessInbreeding = inbreedingCoefficient - _inbreedingTolerance;
            float maxInbreeding = _maximumInbreedingCoefficient - _inbreedingTolerance;
            float depressionFactor = excessInbreeding / maxInbreeding;
            
            return 1f - (depressionFactor * 0.5f); // Max 50% reduction
        }

        /// <summary>
        /// Get recommended population size for maintaining diversity
        /// </summary>
        public int GetRecommendedPopulationSize()
        {
            return Mathf.Max(_founderPopulationSize, _effectivePopulationSize);
        }

        /// <summary>
        /// Check if environmental conditions warrant genetic adaptation
        /// </summary>
        public bool ShouldTriggerAdaptation(float environmentalChange)
        {
            return _enableGxEInteractions && environmentalChange > _environmentalStressThreshold;
        }

        public override bool ValidateData()
        {
            bool isValid = base.ValidateData();

            if (_baseMutationRate <= 0f)
            {
                LogWarning("Base mutation rate should be greater than 0");
                isValid = false;
            }

            if (_founderPopulationSize < 10)
            {
                LogWarning("Founder population size should be at least 10 for genetic diversity");
                isValid = false;
            }

            if (_heritabilityCoefficient < 0f || _heritabilityCoefficient > 1f)
            {
                LogError("Heritability coefficient must be between 0 and 1");
                isValid = false;
            }

            if (_minimumViabilityThreshold > 0.9f)
            {
                LogWarning("Minimum viability threshold is very high - may prevent viable offspring");
            }

            return isValid;
        }

        /// <summary>
        /// Reset to default values
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            _enableMendelianInheritance = true;
            _enablePolygeneticTraits = true;
            _enableEpigenetics = true;
            _baseMutationRate = 0.001f;
            _heritabilityCoefficient = 0.7f;
            _selectionIntensity = 0.8f;
            _minimumGeneticDiversity = 0.3f;
            
            LogInfo("Cannabis genetics config reset to default values");
        }

        /// <summary>
        /// Apply a preset configuration
        /// </summary>
        public void ApplyPreset(GeneticsPreset preset)
        {
            switch (preset)
            {
                case GeneticsPreset.Realistic:
                    ApplyRealisticPreset();
                    break;
                case GeneticsPreset.Simplified:
                    ApplySimplifiedPreset();
                    break;
                case GeneticsPreset.Advanced:
                    ApplyAdvancedPreset();
                    break;
                case GeneticsPreset.Experimental:
                    ApplyExperimentalPreset();
                    break;
            }
        }

        private void ApplyRealisticPreset()
        {
            _baseMutationRate = 0.0001f;
            _enableEpigenetics = true;
            _heritabilityCoefficient = 0.6f;
            _environmentalSensitivity = 0.4f;
            _selectionIntensity = 0.3f;
        }

        private void ApplySimplifiedPreset()
        {
            _enableEpigenetics = false;
            _enableGxEInteractions = false;
            _baseMutationRate = 0.01f;
            _heritabilityCoefficient = 0.8f;
            _selectionIntensity = 0.9f;
        }

        private void ApplyAdvancedPreset()
        {
            _enableEpigenetics = true;
            _enableGxEInteractions = true;
            _enableQuantitativeTraits = true;
            _baseMutationRate = 0.0005f;
            _heritabilityCoefficient = 0.7f;
            _polygeneticTraitLoci = 15;
        }

        private void ApplyExperimentalPreset()
        {
            _enableGeneticDrift = true;
            _enablePopulationStructure = true;
            _baseMutationRate = 0.005f;
            _environmentalMutationModifier = 0.1f;
            _selectionIntensity = 0.95f;
        }
    }

    public enum GeneticsPreset
    {
        Realistic,
        Simplified,
        Advanced,
        Experimental
    }
}