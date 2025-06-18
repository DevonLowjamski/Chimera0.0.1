using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Represents a specific variant (allele) of a gene with its unique properties and effects.
    /// Contains detailed information about the allele's molecular characteristics and phenotypic impact.
    /// </summary>
    [CreateAssetMenu(fileName = "New Allele", menuName = "Project Chimera/Genetics/Allele", order = 11)]
    public class AlleleSO : ChimeraScriptableObject
    {
        [Header("Allele Identity")]
        [SerializeField] private GeneDefinitionSO _parentGene;
        [SerializeField] private string _alleleName;
        [SerializeField] private string _alleleSymbol; // e.g., "A", "a", "A1", "B+"
        [SerializeField] private string _alleleCode; // e.g., "THC1-High", "CBD2-Low"
        [SerializeField, TextArea(3, 5)] private string _alleleDescription;

        [Header("Molecular Properties")]
        [SerializeField] private AlleleType _alleleType = AlleleType.WildType;
        [SerializeField] private MutationType _mutationType = MutationType.None;
        [SerializeField, TextArea(2, 4)] private string _molecularBasis; // Description of the molecular change
        [SerializeField] private int _nucleotideChanges = 0; // Number of base pair changes from wild type

        [Header("Dominance and Expression")]
        [SerializeField] private bool _isDominant = false;
        [SerializeField] private bool _isRecessive = true;
        [SerializeField] private bool _isNullAllele = false; // Produces no functional product
        [SerializeField, Range(0f, 3f)] private float _expressionLevel = 1f; // Relative to wild type
        [SerializeField, Range(0f, 2f)] private float _proteinActivity = 1f; // Functional activity level

        [Header("Phenotypic Effects")]
        [SerializeField, Range(-2f, 2f)] private float _effectStrength = 1f; // Positive = beneficial, negative = detrimental
        [SerializeField] private EffectDirection _effectDirection = EffectDirection.Neutral;
        [SerializeField] private List<TraitEffect> _traitEffects = new List<TraitEffect>();

        [Header("Frequency and Origin")]
        [SerializeField, Range(0f, 1f)] private float _populationFrequency = 0.5f; // Frequency in wild populations
        [SerializeField] private AlleleOrigin _origin = AlleleOrigin.Natural;
        [SerializeField] private string _originDescription;
        [SerializeField] private bool _isRare = false;
        [SerializeField] private bool _isBeneficial = false;
        [SerializeField] private bool _isDetrimental = false;

        [Header("Stability and Mutation")]
        [SerializeField, Range(0f, 0.01f)] private float _mutationRate = 0.0001f; // Rate of mutation to other alleles
        [SerializeField] private bool _isStable = true;
        [SerializeField] private List<AlleleSO> _canMutateTo = new List<AlleleSO>();
        [SerializeField, Range(0f, 1f)] private float _reversibility = 0f; // Can revert to wild type

        [Header("Environmental Sensitivity")]
        [SerializeField] private bool _environmentallySensitive = false;
        [SerializeField] private List<EnvironmentalModifier> _environmentalModifiers = new List<EnvironmentalModifier>();

        [Header("Pleiotropy (Multiple Effects)")]
        [SerializeField] private bool _hasPleiotropicEffects = false;
        [SerializeField] private List<PleiotropicEffect> _pleiotropicEffects = new List<PleiotropicEffect>();

        [Header("Breeding Implications")]
        [SerializeField] private bool _isDesirableForBreeding = true;
        [SerializeField] private bool _isEssentialForViability = false;
        [SerializeField] private bool _causesLethalPhenotype = false;
        [SerializeField] private DifficultyLevel _selectionDifficulty = DifficultyLevel.Intermediate;
        [SerializeField, Range(0f, 1f)] private float _linkageStrength = 0f; // Strength of linkage to other alleles

        [Header("Commercial Value")]
        [SerializeField, Range(0f, 100f)] private float _commercialValue = 50f; // Market value modifier (%)
        [SerializeField] private bool _patented = false;
        [SerializeField] private bool _trademarked = false;
        [SerializeField] private string _breederInfo;

        // Public Properties
        public GeneDefinitionSO ParentGene => _parentGene;
        public string AlleleName => _alleleName;
        public string AlleleSymbol => _alleleSymbol;
        public string AlleleCode => _alleleCode;
        public string AlleleDescription => _alleleDescription;
        
        /// <summary>
        /// Gets the allele ID (same as AlleleCode for compatibility)
        /// </summary>
        public string AlleleId => _alleleCode;
        
        /// <summary>
        /// Gets the ID of the parent gene for compatibility
        /// </summary>
        public string GeneId => _parentGene?.GeneCode ?? string.Empty;

        // Molecular
        public AlleleType AlleleType => _alleleType;
        public MutationType MutationType => _mutationType;
        public string MolecularBasis => _molecularBasis;
        public int NucleotideChanges => _nucleotideChanges;

        // Dominance
        public bool IsDominant => _isDominant;
        public bool IsRecessive => _isRecessive;
        public bool IsNullAllele => _isNullAllele;
        public float ExpressionLevel => _expressionLevel;
        public float ProteinActivity => _proteinActivity;

        // Effects
        public float EffectStrength => _effectStrength;
        public EffectDirection EffectDirection => _effectDirection;
        public List<TraitEffect> TraitEffects => _traitEffects;

        // Frequency
        public float PopulationFrequency => _populationFrequency;
        public AlleleOrigin Origin => _origin;
        public string OriginDescription => _originDescription;
        public bool IsRare => _isRare;
        public bool IsBeneficial => _isBeneficial;
        public bool IsDetrimental => _isDetrimental;

        // Stability
        public float MutationRate => _mutationRate;
        public bool IsStable => _isStable;
        public List<AlleleSO> CanMutateTo => _canMutateTo;
        public float Reversibility => _reversibility;

        // Environment
        public bool EnvironmentallySensitive => _environmentallySensitive;
        public List<EnvironmentalModifier> EnvironmentalModifiers => _environmentalModifiers;

        // Pleiotropy
        public bool HasPleiotropicEffects => _hasPleiotropicEffects;
        public List<PleiotropicEffect> PleiotropicEffects => _pleiotropicEffects;

        // Breeding
        public bool IsDesirableForBreeding => _isDesirableForBreeding;
        public bool IsEssentialForViability => _isEssentialForViability;
        public bool CausesLethalPhenotype => _causesLethalPhenotype;
        public DifficultyLevel SelectionDifficulty => _selectionDifficulty;
        public float LinkageStrength => _linkageStrength;

        // Commercial
        public float CommercialValue => _commercialValue;
        public bool Patented => _patented;
        public bool Trademarked => _trademarked;
        public string BreederInfo => _breederInfo;

        /// <summary>
        /// Calculates the phenotypic contribution of this allele for a specific trait.
        /// </summary>
        /// <param name="trait">The trait to calculate effect for</param>
        /// <param name="environment">Current environmental conditions</param>
        /// <returns>Phenotypic effect value</returns>
        public float CalculateTraitEffect(PlantTrait trait, ProjectChimera.Data.Cultivation.EnvironmentalConditions environment = default)
        {
            // Find the specific trait effect
            var traitEffect = _traitEffects.Find(te => te.AffectedTrait == trait);
            if (traitEffect == null) return 0f;

            float baseEffect = traitEffect.EffectMagnitude * _effectStrength;

            // Apply expression level modifier
            baseEffect *= _expressionLevel;

            // Apply protein activity modifier for biochemical traits
            if (IsBiochemicalTrait(trait))
            {
                baseEffect *= _proteinActivity;
            }

            // Apply environmental modifiers if applicable
            if (_environmentallySensitive && environment.IsInitialized())
            {
                baseEffect *= CalculateEnvironmentalEffect(environment);
            }

            return baseEffect;
        }

        /// <summary>
        /// Calculates the environmental effect on this allele's expression.
        /// </summary>
        private float CalculateEnvironmentalEffect(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment)
        {
            float modifier = 1f;

            foreach (var envMod in _environmentalModifiers)
            {
                float factorValue = GetEnvironmentalFactorValue(environment, envMod.Factor);
                float effect = CalculateFactorEffect(factorValue, envMod);
                modifier *= effect;
            }

            return modifier;
        }

        private float GetEnvironmentalFactorValue(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment, EnvironmentalFactor factor)
        {
            switch (factor)
            {
                case EnvironmentalFactor.Temperature: return environment.Temperature;
                case EnvironmentalFactor.Light: return environment.LightIntensity;
                case EnvironmentalFactor.Humidity: return environment.Humidity;
                case EnvironmentalFactor.CO2: return environment.CO2Level;
                default: return 1f;
            }
        }

        private float CalculateFactorEffect(float value, EnvironmentalModifier modifier)
        {
            // Simple linear response for now - could be made more complex
            if (value >= modifier.OptimalRange.x && value <= modifier.OptimalRange.y)
            {
                return modifier.MaxEffect;
            }

            float distance = 0f;
            if (value < modifier.OptimalRange.x)
            {
                distance = modifier.OptimalRange.x - value;
            }
            else if (value > modifier.OptimalRange.y)
            {
                distance = value - modifier.OptimalRange.y;
            }

            float normalizedDistance = distance / (modifier.OptimalRange.y - modifier.OptimalRange.x);
            return Mathf.Lerp(modifier.MaxEffect, modifier.MinEffect, normalizedDistance);
        }

        private bool IsBiochemicalTrait(PlantTrait trait)
        {
            return trait == PlantTrait.THCContent ||
                   trait == PlantTrait.CBDContent ||
                   trait == PlantTrait.TerpeneProfile ||
                   trait == PlantTrait.ChlorophyllContent ||
                   trait == PlantTrait.AntioxidantLevels;
        }

        /// <summary>
        /// Determines if this allele can mutate under current conditions.
        /// </summary>
        public bool CanMutate()
        {
            return !_isStable && _canMutateTo.Count > 0 && Random.value < _mutationRate;
        }

        /// <summary>
        /// Gets a random mutation target allele.
        /// </summary>
        public AlleleSO GetMutationTarget()
        {
            if (_canMutateTo.Count == 0) return null;
            return _canMutateTo[Random.Range(0, _canMutateTo.Count)];
        }

        /// <summary>
        /// Calculates the fitness contribution of this allele.
        /// </summary>
        public float CalculateFitness(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment)
        {
            float fitness = 1f;

            if (_causesLethalPhenotype) return 0f;
            if (_isEssentialForViability && _isNullAllele) return 0f;

            // Base fitness from effect strength
            fitness += _effectStrength * 0.1f;

            // Environmental adaptation
            if (_environmentallySensitive)
            {
                fitness *= CalculateEnvironmentalEffect(environment);
            }

            // Beneficial vs detrimental effects
            if (_isBeneficial) fitness *= 1.1f;
            if (_isDetrimental) fitness *= 0.9f;

            return Mathf.Max(0f, fitness);
        }

        public override bool ValidateData()
        {
            bool isValid = base.ValidateData();

            if (_parentGene == null)
            {
                Debug.LogWarning($"[Chimera] AlleleSO '{DisplayName}' has no parent gene assigned.", this);
                isValid = false;
            }

            if (string.IsNullOrEmpty(_alleleName))
            {
                Debug.LogWarning($"[Chimera] AlleleSO '{DisplayName}' has no allele name assigned.", this);
                isValid = false;
            }

            if (_isDominant && _isRecessive)
            {
                Debug.LogWarning($"[Chimera] AlleleSO '{DisplayName}' cannot be both dominant and recessive.", this);
                isValid = false;
            }

            if (_causesLethalPhenotype && _isDesirableForBreeding)
            {
                Debug.LogWarning($"[Chimera] AlleleSO '{DisplayName}' is lethal but marked as desirable for breeding.", this);
                isValid = false;
            }

            return isValid;
        }
    }

    [System.Serializable]
    public class TraitEffect
    {
        public PlantTrait AffectedTrait;
        [Range(-2f, 2f)] public float EffectMagnitude = 1f;
        public bool IsMainEffect = true; // vs secondary effect
        [TextArea(2, 3)] public string EffectDescription;
    }

    [System.Serializable]
    public class EnvironmentalModifier
    {
        public EnvironmentalFactor Factor;
        public Vector2 OptimalRange = new Vector2(20f, 26f);
        [Range(0.1f, 2f)] public float MaxEffect = 1.2f;
        [Range(0.1f, 2f)] public float MinEffect = 0.8f;
        [TextArea(2, 3)] public string ModifierDescription;
    }

    [System.Serializable]
    public class PleiotropicEffect
    {
        public PlantTrait SecondaryTrait;
        [Range(-1f, 1f)] public float SecondaryEffect = 0.5f;
        public bool IsPositiveCorrelation = true;
        [TextArea(2, 3)] public string CorrelationDescription;
    }

    public enum AlleleType
    {
        WildType,          // Normal, common allele
        Mutant,            // Altered from wild type
        Null,              // No functional product
        Hypomorph,         // Reduced function
        Hypermorph,        // Increased function
        Neomorph,          // New function
        Antimorph,         // Dominant negative
        Synthetic          // Artificially created
    }

    public enum MutationType
    {
        None,              // No mutation (wild type)
        PointMutation,     // Single nucleotide change
        Insertion,         // DNA insertion
        Deletion,          // DNA deletion
        Duplication,       // Gene/segment duplication
        Inversion,         // DNA sequence inversion
        Translocation,     // Movement between chromosomes
        Substitution,      // Amino acid substitution
        Frameshift,        // Reading frame alteration
        Splice,            // Splicing mutation
        Regulatory,        // Affects gene regulation
        Chromosomal       // Large chromosomal change
    }

    public enum AlleleOrigin
    {
        Natural,           // Naturally occurring
        Induced,           // Laboratory induced
        Selected,          // Breeding selection
        Engineered,        // Genetic engineering
        Hybrid,            // From hybridization
        Landrace,          // Traditional variety
        Spontaneous,       // Spontaneous mutation
        Introgressed      // From another species
    }

    public enum EffectDirection
    {
        Positive,          // Increases trait value
        Negative,          // Decreases trait value
        Neutral,           // No significant effect
        Variable          // Effect depends on environment
    }
}