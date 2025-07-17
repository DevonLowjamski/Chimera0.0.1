using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Defines a single skill node in the player progression system for cannabis cultivation expertise.
    /// Includes skill trees for cultivation, genetics, business, processing, and technology domains.
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill Node", menuName = "Project Chimera/Progression/Skill Node")]
    public class SkillNodeSO : ChimeraDataSO
    {
        [Header("Skill Identity")]
        [SerializeField] private string _skillName;
        [SerializeField] private SkillCategory _skillCategory = SkillCategory.Cultivation;
        [SerializeField] private SkillDomain _skillDomain = SkillDomain.Growing;
        [SerializeField, TextArea(3, 5)] private string _skillDescription;
        [SerializeField] private Sprite _skillIcon;
        
        [Header("Skill Hierarchy")]
        [SerializeField] private int _skillTier = 1; // 1 = Basic, 5 = Master
        [SerializeField] private int _maxSkillLevel = 10;
        [SerializeField] private List<SkillNodeSO> _prerequisiteSkills = new List<SkillNodeSO>();
        [SerializeField] private List<SkillNodeSO> _unlockedSkills = new List<SkillNodeSO>();
        [SerializeField] private bool _isCapstoneSkill = false;
        [SerializeField] private bool _isUnlocked = false;
        
        [Header("Learning Requirements")]
        [SerializeField] private SkillRequirements _learningRequirements;
        [SerializeField] private List<SkillCost> _levelUpCosts = new List<SkillCost>();
        [SerializeField] private AnimationCurve _experienceRequirementCurve;
        [SerializeField] private List<LearningMethod> _availableLearningMethods = new List<LearningMethod>();
        
        [Header("Skill Effects")]
        [SerializeField] private List<SkillEffect> _skillEffects = new List<SkillEffect>();
        [SerializeField] private List<SkillBonus> _skillBonuses = new List<SkillBonus>();
        [SerializeField] private List<UnlockableFeature> _unlockableFeatures = new List<UnlockableFeature>();
        [SerializeField] private bool _hasPassiveEffect = true;
        
        [Header("Practical Applications")]
        [SerializeField] private List<PracticalApplication> _practicalApplications = new List<PracticalApplication>();
        [SerializeField] private List<EquipmentUnlock> _equipmentUnlocks = new List<EquipmentUnlock>();
        [SerializeField] private List<ProcessUnlock> _processUnlocks = new List<ProcessUnlock>();
        [SerializeField] private List<ResearchUnlock> _researchUnlocks = new List<ResearchUnlock>();
        
        [Header("Mastery and Specialization")]
        [SerializeField] private List<MasteryBonus> _masteryBonuses = new List<MasteryBonus>();
        [SerializeField] private List<SpecializationPath> _specializationPaths = new List<SpecializationPath>();
        [SerializeField] private bool _allowsSpecialization = false;
        [SerializeField] private int _specializationUnlockLevel = 5;
        
        [Header("Teaching and Mentorship")]
        [SerializeField] private bool _canTeachOthers = false;
        [SerializeField] private int _teachingUnlockLevel = 7;
        [SerializeField] private float _teachingEfficiency = 1.2f;
        [SerializeField] private List<MentorshipBenefit> _mentorshipBenefits = new List<MentorshipBenefit>();
        
        // Public Properties
        public string SkillName => _skillName;
        public SkillCategory SkillCategory => _skillCategory;
        public SkillDomain SkillDomain => _skillDomain;
        public string SkillDescription => _skillDescription;
        public Sprite SkillIcon => _skillIcon;
        public int SkillTier => _skillTier;
        public int MaxSkillLevel => _maxSkillLevel;
        public List<SkillNodeSO> PrerequisiteSkills => _prerequisiteSkills;
        public List<SkillNodeSO> UnlockedSkills => _unlockedSkills;
        public bool IsCapstoneSkill => _isCapstoneSkill;
        public bool IsUnlocked => _isUnlocked;
        public SkillRequirements LearningRequirements => _learningRequirements;
        public List<SkillCost> LevelUpCosts => _levelUpCosts;
        public AnimationCurve ExperienceRequirementCurve => _experienceRequirementCurve;
        public List<LearningMethod> AvailableLearningMethods => _availableLearningMethods;
        public List<SkillEffect> SkillEffects => _skillEffects;
        public List<SkillBonus> SkillBonuses => _skillBonuses;
        public List<UnlockableFeature> UnlockableFeatures => _unlockableFeatures;
        public bool HasPassiveEffect => _hasPassiveEffect;
        public List<PracticalApplication> PracticalApplications => _practicalApplications;
        public List<EquipmentUnlock> EquipmentUnlocks => _equipmentUnlocks;
        public List<ProcessUnlock> ProcessUnlocks => _processUnlocks;
        public List<ResearchUnlock> ResearchUnlocks => _researchUnlocks;
        public List<MasteryBonus> MasteryBonuses => _masteryBonuses;
        public List<SpecializationPath> SpecializationPaths => _specializationPaths;
        public bool AllowsSpecialization => _allowsSpecialization;
        public int SpecializationUnlockLevel => _specializationUnlockLevel;
        public bool CanTeachOthers => _canTeachOthers;
        public int TeachingUnlockLevel => _teachingUnlockLevel;
        public float TeachingEfficiency => _teachingEfficiency;
        public List<MentorshipBenefit> MentorshipBenefits => _mentorshipBenefits;
        
        // Additional properties for SkillTreeManager compatibility
        public SkillDomain Domain => _skillDomain;
        public int SkillPointCost => GetSkillPointCost(1);
        public List<SkillEffect> Effects => _skillEffects;
        public bool IsStartingSkill => _prerequisiteSkills.Count == 0 && _skillTier == 1;
        public int SkillPriority => _skillTier; // Use tier as priority
        public int UnlockLevel => _skillTier; // Use tier as unlock level
        public string SkillId => name; // Use ScriptableObject name as ID
        public List<SkillNodeSO> Skills => new List<SkillNodeSO> { this }; // For compatibility
        public int MaxLevel => _maxSkillLevel; // Alias for MaxSkillLevel
        public List<string> PrerequisiteSkillIds => _prerequisiteSkills.Select(skill => skill.name).ToList(); // String IDs for compatibility
        public List<SkillNodeSO> Prerequisites => _prerequisiteSkills; // Alias for PrerequisiteSkills
        public int RequiredLevel => _skillTier; // Alias for UnlockLevel
        
        /// <summary>
        /// Get total experience required to max this skill
        /// </summary>
        public float GetTotalExperienceRequired()
        {
            float total = 0f;
            for (int level = 1; level <= _maxSkillLevel; level++)
            {
                total += GetExperienceRequiredForLevel(level);
            }
            return total;
        }
        
        /// <summary>
        /// Checks if the player meets the prerequisites to learn this skill.
        /// </summary>
        public bool CanLearnSkill(PlayerSkillProfile playerProfile)
        {
            // Check prerequisite skills
            foreach (var prereq in _prerequisiteSkills)
            {
                if (!playerProfile.HasSkillAtLevel(prereq, prereq.LearningRequirements.MinimumPrerequisiteLevel))
                    return false;
            }
            
            // Check other requirements
            if (playerProfile.PlayerLevel < _learningRequirements.MinimumPlayerLevel)
                return false;
                
            if (playerProfile.GetSkillPoints() < GetSkillPointCost(1))
                return false;
            
            // Check facility/equipment requirements
            foreach (var requirement in _learningRequirements.FacilityRequirements)
            {
                if (!playerProfile.HasFacility(requirement))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculates the experience required to reach a specific level.
        /// </summary>
        public float GetExperienceRequiredForLevel(int targetLevel)
        {
            if (targetLevel <= 1) return 0f;
            
            if (_experienceRequirementCurve != null && _experienceRequirementCurve.length > 0)
            {
                float normalizedLevel = (float)targetLevel / _maxSkillLevel;
                return _experienceRequirementCurve.Evaluate(normalizedLevel) * 1000f; // Scale to reasonable numbers
            }
            
            // Default exponential curve
            return Mathf.Pow(targetLevel, 1.5f) * 100f;
        }
        
        /// <summary>
        /// Gets the skill point cost to level up from current level.
        /// </summary>
        public int GetSkillPointCost(int currentLevel)
        {
            if (currentLevel >= _maxSkillLevel) return 0;
            
            var levelCost = _levelUpCosts.Find(lc => lc.Level == currentLevel + 1);
            return levelCost?.SkillPointCost ?? (currentLevel + 1);
        }
        
        /// <summary>
        /// Calculates the total effect value at a specific skill level.
        /// </summary>
        public float CalculateEffectValue(SkillEffectType effectType, int skillLevel)
        {
            var effect = _skillEffects.Find(se => se.EffectType == effectType);
            if (effect == null) return 0f;
            
            float baseValue = effect.BaseValue;
            float perLevelValue = effect.ValuePerLevel * skillLevel;
            
            return baseValue + perLevelValue;
        }
        
        /// <summary>
        /// Gets all bonuses available at a specific skill level.
        /// </summary>
        public List<SkillBonus> GetAvailableBonuses(int skillLevel)
        {
            var availableBonuses = new List<SkillBonus>();
            
            foreach (var bonus in _skillBonuses)
            {
                if (skillLevel >= bonus.UnlockLevel)
                    availableBonuses.Add(bonus);
            }
            
            return availableBonuses;
        }
        
        /// <summary>
        /// Gets all features unlocked at a specific skill level.
        /// </summary>
        public List<UnlockableFeature> GetUnlockedFeatures(int skillLevel)
        {
            var unlockedFeatures = new List<UnlockableFeature>();
            
            foreach (var feature in _unlockableFeatures)
            {
                if (skillLevel >= feature.UnlockLevel)
                    unlockedFeatures.Add(feature);
            }
            
            return unlockedFeatures;
        }
        
        /// <summary>
        /// Determines if this skill provides synergy with another skill.
        /// </summary>
        public float CalculateSynergyBonus(SkillNodeSO otherSkill, int thisSkillLevel, int otherSkillLevel)
        {
            // Check for synergies based on skill domains and categories
            float synergyBonus = 0f;
            
            // Same domain synergy
            if (_skillDomain == otherSkill.SkillDomain)
                synergyBonus += 0.1f;
            
            // Complementary skill synergy
            if (HasComplementaryRelationship(otherSkill))
                synergyBonus += 0.15f;
            
            // Scale by skill levels
            float avgLevel = (thisSkillLevel + otherSkillLevel) / 2f;
            synergyBonus *= (avgLevel / _maxSkillLevel);
            
            return synergyBonus;
        }
        
        /// <summary>
        /// Gets the most efficient learning method for this skill.
        /// </summary>
        public LearningMethod GetOptimalLearningMethod(PlayerSkillProfile playerProfile)
        {
            LearningMethod bestMethod = null;
            float bestEfficiency = 0f;
            
            foreach (var method in _availableLearningMethods)
            {
                float efficiency = CalculateLearningEfficiency(method, playerProfile);
                if (efficiency > bestEfficiency)
                {
                    bestEfficiency = efficiency;
                    bestMethod = method;
                }
            }
            
            return bestMethod ?? (_availableLearningMethods.Count > 0 ? _availableLearningMethods[0] : null);
        }
        
        private bool HasComplementaryRelationship(SkillNodeSO otherSkill)
        {
            // Define complementary relationships between different skill domains
            var complementaryPairs = new Dictionary<SkillDomain, SkillDomain>
            {
                { SkillDomain.Growing, SkillDomain.Processing },
                { SkillDomain.Genetics, SkillDomain.Growing },
                { SkillDomain.Business, SkillDomain.Marketing },
                { SkillDomain.Quality_Control, SkillDomain.Processing },
                { SkillDomain.Technology, SkillDomain.Automation }
            };
            
            return complementaryPairs.ContainsKey(_skillDomain) && 
                   complementaryPairs[_skillDomain] == otherSkill.SkillDomain;
        }
        
        private float CalculateLearningEfficiency(LearningMethod method, PlayerSkillProfile playerProfile)
        {
            float baseEfficiency = method.EfficiencyMultiplier;
            
            // Adjust based on player's learning style preferences
            if (playerProfile.PreferredLearningStyle == method.LearningType)
                baseEfficiency *= 1.2f;
            
            // Adjust based on available resources
            if (method.RequiredResources.Count > 0)
            {
                foreach (var resource in method.RequiredResources)
                {
                    if (!playerProfile.HasResource(resource))
                        baseEfficiency *= 0.5f;
                }
            }
            
            return baseEfficiency;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_skillName))
            {
                Debug.LogError($"SkillNodeSO '{name}' has no skill name assigned.", this);
                isValid = false;
            }
            
            if (_skillTier < 1 || _skillTier > 5)
            {
                Debug.LogError($"Skill Node {name}: Skill tier must be between 1 and 5");
                isValid = false;
            }
            
            if (_maxSkillLevel <= 0)
            {
                Debug.LogError($"Skill Node {name}: Max skill level must be positive");
                isValid = false;
            }
            
            if (_skillEffects.Count == 0)
            {
                Debug.LogWarning($"Skill Node {name}: No skill effects defined");
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    [System.Serializable]
    public class SkillRequirements
    {
        [Range(1, 100)] public int MinimumPlayerLevel = 1;
        [Range(1, 10)] public int MinimumPrerequisiteLevel = 1;
        public List<string> FacilityRequirements = new List<string>();
        public List<string> EquipmentRequirements = new List<string>();
        public List<string> CertificationRequirements = new List<string>();
        [Range(0f, 10000f)] public float MinimumExperience = 0f;
    }
    
    [System.Serializable]
    public class SkillCost
    {
        [Range(1, 10)] public int Level = 1;
        [Range(1, 20)] public int SkillPointCost = 1;
        [Range(0f, 100000f)] public float MoneyCost = 0f;
        [Range(0f, 1000f)] public float TimeCost = 1f; // hours
        public List<ResourceCost> ResourceCosts = new List<ResourceCost>();
    }
    
    [System.Serializable]
    public class ResourceCost
    {
        public string ResourceName;
        [Range(1, 1000)] public int Quantity = 1;
        public string ResourceDescription;
    }
    
    [System.Serializable]
    public class LearningMethod
    {
        public string MethodName;
        public LearningType LearningType;
        [Range(0.5f, 3f)] public float EfficiencyMultiplier = 1f;
        [Range(0.5f, 5f)] public float TimeMultiplier = 1f;
        [Range(0.5f, 5f)] public float CostMultiplier = 1f;
        public List<string> RequiredResources = new List<string>();
        [TextArea(2, 3)] public string MethodDescription;
    }
    
    [System.Serializable]
    public class SkillEffect
    {
        public SkillEffectType EffectType;
        [Range(-100f, 100f)] public float BaseValue = 0f;
        [Range(-10f, 10f)] public float ValuePerLevel = 1f;
        public bool IsPercentage = false;
        public EffectScope EffectScope = EffectScope.Player;
        [TextArea(2, 3)] public string EffectDescription;
    }
    
    [System.Serializable]
    public class SkillBonus
    {
        public string BonusName;
        public SkillBonusType BonusType;
        [Range(1, 10)] public int UnlockLevel = 1;
        [Range(-100f, 100f)] public float BonusValue = 10f;
        public bool IsPercentage = true;
        [TextArea(2, 3)] public string BonusDescription;
    }
    
    [System.Serializable]
    public class UnlockableFeature
    {
        public string FeatureName;
        public FeatureType FeatureType;
        [Range(1, 10)] public int UnlockLevel = 1;
        public bool IsOneTimeUnlock = true;
        [TextArea(2, 3)] public string FeatureDescription;
    }
    
    [System.Serializable]
    public class PracticalApplication
    {
        public string ApplicationName;
        public ApplicationDomain ApplicationDomain;
        [Range(0f, 2f)] public float EfficiencyBonus = 0.1f;
        [Range(0f, 1f)] public float QualityBonus = 0.05f;
        [TextArea(2, 3)] public string ApplicationDescription;
    }
    
    [System.Serializable]
    public class EquipmentUnlock
    {
        public ProjectChimera.Data.Equipment.EquipmentDataSO Equipment;
        [Range(1, 10)] public int UnlockLevel = 1;
        public bool ReducesOperatingCost = false;
        [Range(0f, 0.5f)] public float CostReduction = 0.1f;
        public string UnlockDescription;
    }
    
    [System.Serializable]
    public class ProcessUnlock
    {
        public string ProcessName;
        public ProcessType ProcessType;
        [Range(1, 10)] public int UnlockLevel = 1;
        [Range(0f, 2f)] public float ProcessEfficiency = 1.2f;
        public string ProcessDescription;
    }
    
    [System.Serializable]
    public class ResearchUnlock
    {
        public ResearchProjectSO ResearchProject;
        [Range(1, 10)] public int UnlockLevel = 1;
        [Range(0f, 0.5f)] public float ResearchSpeedBonus = 0.1f;
        public string UnlockDescription;
    }
    
    [System.Serializable]
    public class MasteryBonus
    {
        public string MasteryName;
        [Range(8, 10)] public int MasteryLevel = 10;
        [Range(0f, 1f)] public float MasteryBonusValue = 0.25f;
        public MasteryType MasteryType;
        [TextArea(2, 3)] public string MasteryDescription;
    }
    
    [System.Serializable]
    public class SpecializationPath
    {
        public string SpecializationName;
        public SpecializationType SpecializationType;
        public List<SkillNodeSO> RequiredSkills = new List<SkillNodeSO>();
        [Range(0f, 1f)] public float SpecializationBonus = 0.3f;
        [TextArea(2, 3)] public string SpecializationDescription;
    }
    
    [System.Serializable]
    public class MentorshipBenefit
    {
        public string BenefitName;
        public MentorshipType MentorshipType;
        [Range(0f, 2f)] public float BenefitMultiplier = 1.5f;
        [TextArea(2, 3)] public string BenefitDescription;
    }
    
    [System.Serializable]
    public class PlayerSkillProfile
    {
        public int PlayerLevel = 1;
        public int AvailableSkillPoints = 0;
        public LearningType PreferredLearningStyle = LearningType.Hands_On;
        public Dictionary<SkillNodeSO, int> SkillLevels = new Dictionary<SkillNodeSO, int>();
        public List<string> AvailableFacilities = new List<string>();
        public List<string> AvailableResources = new List<string>();
        
        public bool HasSkillAtLevel(SkillNodeSO skill, int level)
        {
            return SkillLevels.ContainsKey(skill) && SkillLevels[skill] >= level;
        }
        
        public int GetSkillPoints()
        {
            return AvailableSkillPoints;
        }
        
        public bool HasFacility(string facility)
        {
            return AvailableFacilities.Contains(facility);
        }
        
        public bool HasResource(string resource)
        {
            return AvailableResources.Contains(resource);
        }
    }
    
    // Enums for the skill system
    public enum SkillCategory
    {
        Basic,
        Cultivation,
        Genetics,
        Processing,
        Business,
        Technology,
        Quality_Control,
        Research,
        Management,
        Marketing,
        Compliance
    }
    
    public enum SkillDomain
    {
        Cultivation,
        Growing,
        Genetics,
        Breeding,
        Processing,
        Quality_Control,
        Business,
        Marketing,
        Technology,
        Automation,
        Research,
        Compliance,
        Management,
        Finance
    }
    
    public enum LearningType
    {
        Hands_On,
        Theoretical,
        Mentorship,
        Online_Course,
        Workshop,
        Conference,
        Self_Study,
        Research,
        Collaboration,
        Trial_And_Error
    }
    

    
    public enum EffectScope
    {
        Player,
        Facility,
        Equipment,
        Process,
        Product,
        Team,
        Global
    }
    
    public enum SkillBonusType
    {
        Production_Bonus,
        Quality_Bonus,
        Efficiency_Bonus,
        Cost_Reduction,
        Speed_Bonus,
        Automation_Bonus,
        Research_Bonus,
        Innovation_Bonus,
        Teaching_Bonus,
        Leadership_Bonus
    }
    
    public enum FeatureType
    {
        New_Equipment,
        New_Process,
        New_Research,
        Advanced_Analytics,
        Automation_Feature,
        Quality_Control,
        Market_Access,
        Certification,
        Teaching_Ability,
        Consultation_Service
    }
    
    public enum ApplicationDomain
    {
        Cultivation,
        Processing,
        Quality_Control,
        Business_Operations,
        Research_Development,
        Marketing_Sales,
        Compliance_Legal,
        Technology_Innovation
    }
    
    public enum ProcessType
    {
        Advanced_Cultivation,
        Specialized_Processing,
        Quality_Testing,
        Automation_Control,
        Research_Method,
        Business_Process,
        Marketing_Strategy,
        Compliance_Procedure
    }
    
    public enum MasteryType
    {
        Technical_Mastery,
        Artistic_Mastery,
        Scientific_Mastery,
        Business_Mastery,
        Innovation_Mastery,
        Teaching_Mastery,
        Leadership_Mastery
    }
    
    public enum SpecializationType
    {
        Cultivation_Specialist,
        Genetics_Expert,
        Processing_Master,
        Quality_Expert,
        Business_Leader,
        Technology_Innovator,
        Research_Scientist,
        Compliance_Specialist
    }
    
    public enum MentorshipType
    {
        Skill_Transfer,
        Experience_Sharing,
        Network_Access,
        Resource_Sharing,
        Emotional_Support,
        Career_Guidance,
        Innovation_Catalyst
    }
    
    public enum SkillEffectType
    {
        Yield_Bonus,
        Quality_Bonus,
        Growth_Speed,
        Disease_Resistance,
        Nutrient_Efficiency,
        Energy_Efficiency,
        Cost_Reduction,
        Time_Reduction,
        Success_Rate,
        Learning_Speed,
        Research_Speed,
        Innovation_Rate,
        Efficiency
    }
}