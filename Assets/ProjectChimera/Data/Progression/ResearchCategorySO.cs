using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Defines research categories that organize and provide bonuses for related research projects.
    /// Specializes in advanced genetics and cultivation techniques with progressive unlocks.
    /// </summary>
    [CreateAssetMenu(fileName = "New Research Category", menuName = "Project Chimera/Progression/Research Category")]
    public class ResearchCategorySO : ChimeraDataSO
    {
        [Header("Category Identity")]
        [SerializeField] private string _categoryName;
        [SerializeField] private ResearchCategory _categoryType;
        [SerializeField, TextArea(3, 5)] private string _categoryDescription;
        [SerializeField] private Sprite _categoryIcon;
        [SerializeField] private Color _categoryColor = Color.white;
        
        [Header("Research Organization")]
        [SerializeField] private List<ResearchProjectSO> _availableProjects = new List<ResearchProjectSO>();
        [SerializeField] private List<ResearchProjectSO> _foundationProjects = new List<ResearchProjectSO>();
        [SerializeField] private List<ResearchProjectSO> _advancedProjects = new List<ResearchProjectSO>();
        [SerializeField] private List<ResearchProjectSO> _cuttingEdgeProjects = new List<ResearchProjectSO>();
        
        [Header("Unlock Requirements")]
        [SerializeField] private int _minimumPlayerLevel = 1;
        [SerializeField] private List<ResearchCategorySO> _prerequisiteCategories = new List<ResearchCategorySO>();
        [SerializeField] private List<TechnologyUnlock> _requiredTechnologies = new List<TechnologyUnlock>();
        [SerializeField] private int _minimumCompletedProjects = 0;
        
        [Header("Category Progression")]
        [SerializeField] private bool _enablesCategoryMastery = true;
        [SerializeField] private int _projectsRequiredForMastery = 10;
        [SerializeField] private List<CategoryMasteryBonus> _masteryBonuses = new List<CategoryMasteryBonus>();
        [SerializeField] private bool _allowsSpecialization = true;
        [SerializeField] private List<ResearchSpecialization> _specializations = new List<ResearchSpecialization>();
        
        [Header("Research Bonuses")]
        [SerializeField] private CategoryResearchBonuses _researchBonuses;
        [SerializeField] private bool _enablesSynergyBonuses = true;
        [SerializeField] private List<CategorySynergy> _synergyCategories = new List<CategorySynergy>();
        [SerializeField] private float _categoryExpertiseMultiplier = 1.2f;
        
        [Header("Innovation Unlocks")]
        [SerializeField] private List<InnovationUnlock> _innovationUnlocks = new List<InnovationUnlock>();
        [SerializeField] private List<AdvancedTechnique> _advancedTechniques = new List<AdvancedTechnique>();
        [SerializeField] private List<CultivationMethod> _cultivationMethods = new List<CultivationMethod>();
        [SerializeField] private List<GeneticTool> _geneticTools = new List<GeneticTool>();
        
        // Public Properties
        public string CategoryName => _categoryName;
        public ResearchCategory CategoryType => _categoryType;
        public string CategoryDescription => _categoryDescription;
        public Sprite CategoryIcon => _categoryIcon;
        public Color CategoryColor => _categoryColor;
        public List<ResearchProjectSO> AvailableProjects => _availableProjects;
        public List<ResearchProjectSO> FoundationProjects => _foundationProjects;
        public List<ResearchProjectSO> AdvancedProjects => _advancedProjects;
        public List<ResearchProjectSO> CuttingEdgeProjects => _cuttingEdgeProjects;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        public List<ResearchCategorySO> PrerequisiteCategories => _prerequisiteCategories;
        public List<TechnologyUnlock> RequiredTechnologies => _requiredTechnologies;
        public int MinimumCompletedProjects => _minimumCompletedProjects;
        public bool EnablesCategoryMastery => _enablesCategoryMastery;
        public int ProjectsRequiredForMastery => _projectsRequiredForMastery;
        public List<CategoryMasteryBonus> MasteryBonuses => _masteryBonuses;
        public bool AllowsSpecialization => _allowsSpecialization;
        public List<ResearchSpecialization> Specializations => _specializations;
        public CategoryResearchBonuses ResearchBonuses => _researchBonuses;
        public bool EnablesSynergyBonuses => _enablesSynergyBonuses;
        public List<CategorySynergy> SynergyCategories => _synergyCategories;
        public float CategoryExpertiseMultiplier => _categoryExpertiseMultiplier;
        public List<InnovationUnlock> InnovationUnlocks => _innovationUnlocks;
        public List<AdvancedTechnique> AdvancedTechniques => _advancedTechniques;
        public List<CultivationMethod> CultivationMethods => _cultivationMethods;
        public List<GeneticTool> GeneticTools => _geneticTools;
        
        /// <summary>
        /// Evaluates if the player can access this research category.
        /// </summary>
        public CategoryAccessibility EvaluateCategoryAccess(PlayerResearchCapabilities playerCapabilities, List<ResearchCategorySO> unlockedCategories, List<TechnologyUnlock> unlockedTechnologies)
        {
            var accessibility = new CategoryAccessibility();
            
            // Check player level requirement
            accessibility.MeetsLevelRequirement = playerCapabilities.AvailableBudget >= 0; // Placeholder for actual level check
            
            // Check prerequisite categories
            accessibility.MeetsPrerequisites = true;
            foreach (var prereq in _prerequisiteCategories)
            {
                if (!unlockedCategories.Contains(prereq))
                {
                    accessibility.MeetsPrerequisites = false;
                    accessibility.MissingPrerequisites.Add(prereq.CategoryName);
                }
            }
            
            // Check required technologies
            accessibility.HasRequiredTechnologies = true;
            foreach (var tech in _requiredTechnologies)
            {
                bool hasTech = unlockedTechnologies.Exists(t => t.TechnologyName == tech.TechnologyName);
                if (!hasTech)
                {
                    accessibility.HasRequiredTechnologies = false;
                    accessibility.MissingTechnologies.Add(tech.TechnologyName);
                }
            }
            
            // Check completed projects requirement
            // This would need integration with player's research history
            accessibility.MeetsProjectRequirement = true; // Placeholder
            
            accessibility.IsAccessible = accessibility.MeetsLevelRequirement && 
                                       accessibility.MeetsPrerequisites && 
                                       accessibility.HasRequiredTechnologies && 
                                       accessibility.MeetsProjectRequirement;
            
            return accessibility;
        }
        
        /// <summary>
        /// Gets available research projects based on player's current research level in this category.
        /// </summary>
        public List<ResearchProjectSO> GetAvailableProjectsForLevel(CategoryProgressionLevel level)
        {
            return level switch
            {
                CategoryProgressionLevel.Foundation => _foundationProjects,
                CategoryProgressionLevel.Advanced => _foundationProjects.Concat(_advancedProjects).ToList(),
                CategoryProgressionLevel.Cutting_Edge => _availableProjects,
                _ => _foundationProjects
            };
        }
        
        /// <summary>
        /// Calculates research bonuses for projects in this category.
        /// </summary>
        public ResearchProjectModifiers CalculateCategoryBonuses(int completedProjectsInCategory, List<ResearchCategorySO> masteredCategories)
        {
            var modifiers = new ResearchProjectModifiers();
            
            // Base category bonuses
            modifiers.SpeedMultiplier = _researchBonuses.ResearchSpeedBonus;
            modifiers.QualityMultiplier = _researchBonuses.QualityBonus;
            modifiers.CostReduction = _researchBonuses.CostReduction;
            modifiers.SuccessProbabilityBonus = _researchBonuses.SuccessProbabilityBonus;
            
            // Experience bonuses
            float experienceMultiplier = 1f + (completedProjectsInCategory * 0.05f); // 5% per completed project
            modifiers.ExperienceMultiplier = experienceMultiplier;
            
            // Synergy bonuses
            if (_enablesSynergyBonuses)
            {
                foreach (var synergy in _synergyCategories)
                {
                    if (masteredCategories.Contains(synergy.SynergyCategory))
                    {
                        modifiers.SpeedMultiplier += synergy.SpeedBonus;
                        modifiers.QualityMultiplier += synergy.QualityBonus;
                        modifiers.InnovationBonus += synergy.InnovationBonus;
                    }
                }
            }
            
            return modifiers;
        }
        
        /// <summary>
        /// Checks for category mastery and returns unlocked bonuses.
        /// </summary>
        public CategoryMasteryResult CheckForMastery(int completedProjects, float averageQuality)
        {
            var result = new CategoryMasteryResult();
            
            if (!_enablesCategoryMastery)
            {
                result.CanAchieveMastery = false;
                return result;
            }
            
            result.CanAchieveMastery = true;
            result.RequiredProjects = _projectsRequiredForMastery;
            result.CurrentProjects = completedProjects;
            result.RequiredQuality = 0.8f; // 80% average quality
            result.CurrentQuality = averageQuality;
            
            result.HasAchievedMastery = completedProjects >= _projectsRequiredForMastery && 
                                      averageQuality >= 0.8f;
            
            if (result.HasAchievedMastery)
            {
                result.UnlockedBonuses = _masteryBonuses;
                result.UnlockedSpecializations = _allowsSpecialization ? _specializations : new List<ResearchSpecialization>();
                result.UnlockedInnovations = _innovationUnlocks;
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets specialized research techniques available in this category.
        /// </summary>
        public List<ResearchTechnique> GetAvailableTechniques(List<ResearchSpecialization> playerSpecializations)
        {
            var techniques = new List<ResearchTechnique>();
            
            // Add advanced techniques based on specializations
            foreach (var specialization in playerSpecializations)
            {
                if (_specializations.Contains(specialization))
                {
                    techniques.AddRange(specialization.UnlockedTechniques);
                }
            }
            
            // Add general advanced techniques
            foreach (var technique in _advancedTechniques)
            {
                techniques.Add(new ResearchTechnique
                {
                    TechniqueName = technique.TechniqueName,
                    TechniqueDescription = technique.Description,
                    SpeedModifier = technique.SpeedModifier,
                    QualityModifier = technique.QualityModifier,
                    CostModifier = technique.CostModifier,
                    RequiredSpecialization = technique.RequiredSpecialization
                });
            }
            
            return techniques;
        }
        
        /// <summary>
        /// Calculates innovation potential for new research in this category.
        /// </summary>
        public InnovationPotential CalculateInnovationPotential(PlayerResearchCapabilities capabilities, List<ResearchProjectSO> completedProjects)
        {
            var potential = new InnovationPotential();
            
            // Base innovation rate
            potential.BaseInnovationRate = 0.1f; // 10% base chance
            
            // Category expertise bonus
            int categoryExperience = completedProjects.Count(p => p.ResearchCategory == _categoryType);
            potential.ExpertiseBonus = categoryExperience * 0.02f; // 2% per completed project
            
            // Cross-category synergy bonus
            potential.SynergyBonus = CalculateCrossCategorySynergy(completedProjects);
            
            // Equipment and facility bonuses
            potential.InfrastructureBonus = CalculateInfrastructureBonus(capabilities);
            
            // Calculate total innovation potential
            potential.TotalInnovationRate = potential.BaseInnovationRate + 
                                          potential.ExpertiseBonus + 
                                          potential.SynergyBonus + 
                                          potential.InfrastructureBonus;
            
            potential.TotalInnovationRate = Mathf.Clamp(potential.TotalInnovationRate, 0f, 0.5f); // Cap at 50%
            
            return potential;
        }
        
        private float CalculateCrossCategorySynergy(List<ResearchProjectSO> completedProjects)
        {
            float synergy = 0f;
            var categoryCounts = new Dictionary<ResearchCategory, int>();
            
            foreach (var project in completedProjects)
            {
                if (!categoryCounts.ContainsKey(project.ResearchCategory))
                    categoryCounts[project.ResearchCategory] = 0;
                categoryCounts[project.ResearchCategory]++;
            }
            
            // Bonus for diversity across categories
            int uniqueCategories = categoryCounts.Keys.Count;
            synergy += uniqueCategories * 0.01f; // 1% per unique category
            
            return synergy;
        }
        
        private float CalculateInfrastructureBonus(PlayerResearchCapabilities capabilities)
        {
            float bonus = 0f;
            
            // Equipment quality bonus
            foreach (var equipment in capabilities.AvailableEquipment)
            {
                // This would integrate with equipment data to calculate quality bonuses
                bonus += 0.005f; // Placeholder: 0.5% per equipment
            }
            
            return bonus;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_categoryName))
            {
                Debug.LogError($"ResearchCategorySO '{name}' has no category name assigned.", this);
                isValid = false;
            }
            
            if (_availableProjects.Count == 0)
            {
                Debug.LogWarning($"ResearchCategorySO '{name}' has no available projects assigned.", this);
            }
            
            if (_foundationProjects.Count == 0)
            {
                Debug.LogWarning($"ResearchCategorySO '{name}' has no foundation projects. Players may not be able to start in this category.", this);
            }
            
            return isValid;
        }
    }
    
    // Supporting data structures for research categories
    
    [System.Serializable]
    public class CategoryAccessibility
    {
        public bool IsAccessible;
        public bool MeetsLevelRequirement;
        public bool MeetsPrerequisites;
        public bool HasRequiredTechnologies;
        public bool MeetsProjectRequirement;
        public List<string> MissingPrerequisites = new List<string>();
        public List<string> MissingTechnologies = new List<string>();
    }
    
    [System.Serializable]
    public class CategoryResearchBonuses
    {
        [Range(1f, 3f)] public float ResearchSpeedBonus = 1.1f;
        [Range(1f, 2f)] public float QualityBonus = 1.05f;
        [Range(0f, 0.5f)] public float CostReduction = 0.1f;
        [Range(0f, 0.3f)] public float SuccessProbabilityBonus = 0.05f;
        [Range(0f, 0.2f)] public float InnovationRateBonus = 0.02f;
    }
    
    [System.Serializable]
    public class CategoryMasteryBonus
    {
        public string BonusName;
        public MasteryBonusType BonusType;
        [Range(0f, 2f)] public float BonusValue = 0.1f;
        public bool IsPassiveBonus = true;
        public string BonusDescription;
    }
    
    [System.Serializable]
    public class ResearchSpecialization
    {
        public string SpecializationName;
        public ResearchSpecializationType SpecializationType;
        [TextArea(2, 4)] public string SpecializationDescription;
        public List<ResearchTechnique> UnlockedTechniques = new List<ResearchTechnique>();
        public List<string> RequiredCompletedProjects = new List<string>();
        public int MinimumCategoryMastery = 1;
    }
    
    [System.Serializable]
    public class CategorySynergy
    {
        public ResearchCategorySO SynergyCategory;
        [Range(0f, 0.5f)] public float SpeedBonus = 0.1f;
        [Range(0f, 0.3f)] public float QualityBonus = 0.05f;
        [Range(0f, 0.2f)] public float InnovationBonus = 0.03f;
        public string SynergyDescription;
    }
    
    [System.Serializable]
    public class InnovationUnlock
    {
        public string InnovationName;
        public InnovationType InnovationType;
        public bool RequiresMastery = true;
        [Range(0f, 1000000f)] public float CommercialValue = 100000f;
        [TextArea(2, 4)] public string InnovationDescription;
        public List<string> UnlockRequirements = new List<string>();
    }
    
    [System.Serializable]
    public class AdvancedTechnique
    {
        public string TechniqueName;
        public TechniqueCategory TechniqueCategory;
        [Range(0.5f, 2f)] public float SpeedModifier = 1.2f;
        [Range(0.8f, 2f)] public float QualityModifier = 1.1f;
        [Range(0.5f, 2f)] public float CostModifier = 1.3f;
        public ResearchSpecializationType RequiredSpecialization;
        [TextArea(2, 4)] public string Description;
    }
    
    [System.Serializable]
    public class CultivationMethod
    {
        public string MethodName;
        public CultivationApproach CultivationApproach;
        [Range(0f, 2f)] public float YieldModifier = 1.2f;
        [Range(0f, 2f)] public float QualityModifier = 1.1f;
        [Range(0f, 2f)] public float ResourceEfficiency = 1.15f;
        public List<string> RequiredEquipment = new List<string>();
        [TextArea(2, 4)] public string MethodDescription;
    }
    
    [System.Serializable]
    public class GeneticTool
    {
        public string ToolName;
        public GeneticToolType ToolType;
        [Range(0f, 2f)] public float AccuracyBonus = 1.3f;
        [Range(0f, 2f)] public float SpeedBonus = 1.2f;
        public bool EnablesAdvancedBreeding = false;
        public List<string> UnlockedCapabilities = new List<string>();
        [TextArea(2, 4)] public string ToolDescription;
    }
    
    [System.Serializable]
    public class ResearchProjectModifiers
    {
        public float SpeedMultiplier = 1f;
        public float QualityMultiplier = 1f;
        public float CostReduction = 0f;
        public float SuccessProbabilityBonus = 0f;
        public float ExperienceMultiplier = 1f;
        public float InnovationBonus = 0f;
    }
    
    [System.Serializable]
    public class CategoryMasteryResult
    {
        public bool CanAchieveMastery;
        public bool HasAchievedMastery;
        public int RequiredProjects;
        public int CurrentProjects;
        public float RequiredQuality;
        public float CurrentQuality;
        public List<CategoryMasteryBonus> UnlockedBonuses = new List<CategoryMasteryBonus>();
        public List<ResearchSpecialization> UnlockedSpecializations = new List<ResearchSpecialization>();
        public List<InnovationUnlock> UnlockedInnovations = new List<InnovationUnlock>();
    }
    
    [System.Serializable]
    public class ResearchTechnique
    {
        public string TechniqueName;
        public string TechniqueDescription;
        public float SpeedModifier = 1f;
        public float QualityModifier = 1f;
        public float CostModifier = 1f;
        public ResearchSpecializationType RequiredSpecialization;
    }
    
    [System.Serializable]
    public class InnovationPotential
    {
        public float BaseInnovationRate;
        public float ExpertiseBonus;
        public float SynergyBonus;
        public float InfrastructureBonus;
        public float TotalInnovationRate;
    }
    
    // Additional enums for research categories
    
    public enum CategoryProgressionLevel
    {
        Foundation,
        Advanced,
        Cutting_Edge
    }
    
    public enum MasteryBonusType
    {
        Research_Speed,
        Quality_Improvement,
        Cost_Reduction,
        Innovation_Rate,
        Experience_Gain,
        Unlock_Access
    }
    
    public enum InnovationType
    {
        Cultivation_Innovation,
        Genetic_Innovation,
        Processing_Innovation,
        Equipment_Innovation,
        Method_Innovation,
        Technology_Innovation
    }
    
    public enum TechniqueCategory
    {
        Data_Collection,
        Analysis_Method,
        Breeding_Technique,
        Cultivation_Practice,
        Quality_Control,
        Process_Optimization
    }
    
    public enum CultivationApproach
    {
        Hydroponic_Advanced,
        Aeroponic_System,
        Living_Soil_Enhanced,
        Coco_Precision,
        Automated_System,
        Climate_Controlled,
        Vertical_Farming,
        Regenerative_Practice
    }
    
    public enum GeneticToolType
    {
        DNA_Sequencing,
        Marker_Assisted_Selection,
        Tissue_Culture,
        Genetic_Testing,
        Breeding_Software,
        Population_Analysis,
        Trait_Mapping,
        Genomic_Selection
    }
    
    public enum ResearchSpecializationType
    {
        Cultivation_Specialist,
        Genetics_Researcher,
        Processing_Expert,
        Quality_Analyst,
        Innovation_Lead,
        Business_Developer,
        Sustainability_Expert,
        Technology_Integrator
    }
}