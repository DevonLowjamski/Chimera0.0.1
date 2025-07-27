using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Gene definition for genetic system compatibility.
    /// Extended implementation to support genetics system requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "New Gene Definition", menuName = "Project Chimera/Genetics/Gene Definition")]
    public class GeneDefinitionSO : ChimeraScriptableObjectSO
    {
        [Header("Gene Identity")]
        [SerializeField] private string _geneName;
        [SerializeField] private string _geneSymbol;
        [SerializeField] private string _geneCode;
        [SerializeField] private TraitType _primaryTrait;
        
        [Header("Gene Properties")]
        [SerializeField] private GeneCategory _category = GeneCategory.Specialized;
        [SerializeField] private GeneType _geneType = GeneType.PlantHeight;
        [SerializeField] private bool _environmentallyRegulated = false;
        [SerializeField] private List<TraitInfluence> _influencedTraits = new List<TraitInfluence>();
        [SerializeField] private List<AlleleSO> _knownAlleles = new List<AlleleSO>();
        
        // Public Properties
        public string GeneName => _geneName;
        public string GeneSymbol => _geneSymbol;
        public string GeneCode => _geneCode;
        public TraitType PrimaryTrait => _primaryTrait;
        public GeneCategory Category => _category;
        public GeneType GeneType => _geneType;
        public bool EnvironmentallyRegulated => _environmentallyRegulated;
        public List<TraitInfluence> InfluencedTraits => _influencedTraits;
        public List<AlleleSO> KnownAlleles => _knownAlleles;
        
        /// <summary>
        /// Gets a wild-type allele for this gene.
        /// </summary>
        public AlleleSO WildTypeAllele()
        {
            return _knownAlleles.Find(a => a != null) ?? null;
        }
        
        /// <summary>
        /// Gets a random allele for this gene.
        /// </summary>
        public AlleleSO GetRandomAllele()
        {
            if (_knownAlleles.Count == 0) return null;
            return _knownAlleles[Random.Range(0, _knownAlleles.Count)];
        }
        
        /// <summary>
        /// Calculates phenotypic effect of two alleles.
        /// </summary>
        public float CalculatePhenotypicEffect(AlleleSO allele1, AlleleSO allele2, EnvironmentalConditions environment)
        {
            if (allele1 == null || allele2 == null) return 0f;
            
            float effect1 = allele1.CalculateTraitEffect(_primaryTrait, environment);
            float effect2 = allele2.CalculateTraitEffect(_primaryTrait, environment);
            
            // Simple dominance model
            return Mathf.Max(effect1, effect2);
        }
    }
    
    [System.Serializable]
    public class TraitInfluence
    {
        public TraitType TraitType;
        [Range(0f, 1f)] public float InfluenceStrength = 1f;
    }
}