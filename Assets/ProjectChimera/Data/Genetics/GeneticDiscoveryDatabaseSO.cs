using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Genetic Discovery Database - Collection of genetic discoveries for genetics gaming system
    /// Contains discovery patterns, innovation tracking, and breakthrough definitions
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Genetic Discovery Database", menuName = "Project Chimera/Gaming/Genetic Discovery Database")]
    public class GeneticDiscoveryDatabaseSO : ChimeraDataSO
    {
        [Header("Discovery Collection")]
        public List<GeneticDiscovery> Discoveries = new List<GeneticDiscovery>();
        
        [Header("Discovery Categories")]
        public List<DiscoveryCategoryData> Categories = new List<DiscoveryCategoryData>();
        
        [Header("Innovation Patterns")]
        public List<InnovationPattern> InnovationPatterns = new List<InnovationPattern>();
        
        #region Runtime Methods
        
        public GeneticDiscovery GetDiscovery(string discoveryID)
        {
            return Discoveries.Find(d => d.DiscoveryID == discoveryID);
        }
        
        public List<GeneticDiscovery> GetDiscoveriesByCategory(DiscoveryCategory category)
        {
            return Discoveries.FindAll(d => d.Category == category);
        }
        
        public List<InnovationPattern> GetInnovationPatterns(InnovationType innovationType)
        {
            return InnovationPatterns.FindAll(p => p.InnovationType == innovationType);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class GeneticDiscovery
    {
        public string DiscoveryID;
        public string DiscoveryName;
        public DiscoveryCategory Category;
        public DiscoveryRarity Rarity;
        public List<DiscoveryRequirement> Requirements = new List<DiscoveryRequirement>();
        public float InnovationValue;
        public List<string> UnlockedFeatures = new List<string>();
        public string Description;
        public Sprite DiscoveryIcon;
    }
    
    [System.Serializable]
    public class DiscoveryCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public DiscoveryCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class InnovationPattern
    {
        public string PatternID;
        public string PatternName;
        public InnovationType InnovationType;
        public List<PatternCriterion> Criteria = new List<PatternCriterion>();
        public float DetectionThreshold;
        public float InnovationScore;
        public string Description;
    }
    
    [System.Serializable]
    public class DiscoveryRequirement
    {
        public string RequirementName;
        public DiscoveryRequirementType RequirementType;
        public float RequiredValue;
        public string Description;
    }
    
    [System.Serializable]
    public class PatternCriterion
    {
        public string CriterionName;
        public PatternCriterionType CriterionType;
        public float CriterionValue;
        public string Description;
    }
    
    public enum DiscoveryCategory
    {
        NovelTrait,
        GeneticPattern,
        InheritancePattern,
        TraitCombination,
        EpigeneticEffect,
        BreedingInnovation,
        GeneInteraction,
        PopulationGenetics
    }
    
    public enum DiscoveryRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
    
    public enum InnovationType
    {
        TraitOptimization,
        NovelCombination,
        BreedingMethod,
        GeneticPattern,
        EpigeneticDiscovery,
        PopulationDynamics
    }
    
    public enum DiscoveryRequirementType
    {
        GenerationCount,
        TraitExpression,
        GeneticDiversity,
        BreedingAccuracy,
        InnovationScore,
        ExperienceLevel
    }
    
    public enum PatternCriterionType
    {
        TraitCorrelation,
        GeneticStability,
        ExpressionLevel,
        InteractionStrength,
        NoveltyScore,
        ComplexityFactor
    }
}