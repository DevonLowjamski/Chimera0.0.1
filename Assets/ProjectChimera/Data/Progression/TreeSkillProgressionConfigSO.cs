using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data
{
    /// <summary>
    /// Tree Skill Progression Configuration - Configuration for skill tree progression mechanics
    /// Defines skill advancement, branching paths, and progression requirements
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Tree Skill Progression Config", menuName = "Project Chimera/Progression/Tree Skill Progression Config")]
    public class TreeSkillProgressionConfigSO : ChimeraConfigSO
    {
        [Header("Progression Settings")]
        [Range(1f, 1000f)] public float BaseExperienceRequired = 100f;
        [Range(1.1f, 3.0f)] public float ExperienceGrowthRate = 1.5f;
        [Range(1, 100)] public int MaxSkillLevel = 50;
        
        [Header("Skill Trees")]
        public List<SkillTreeDefinition> SkillTrees = new List<SkillTreeDefinition>();
        
        [Header("Progression Bonuses")]
        [Range(0.1f, 3.0f)] public float MasteryBonusMultiplier = 1.5f;
        [Range(0.1f, 2.0f)] public float CrossTreeSynergyBonus = 1.2f;
        [Range(0.01f, 0.1f)] public float SpecializationBonus = 0.05f;
        
        [Header("Unlock Requirements")]
        public List<SkillUnlockRequirement> UnlockRequirements = new List<SkillUnlockRequirement>();
        
        public float GetExperienceRequired(int currentLevel)
        {
            return BaseExperienceRequired * Mathf.Pow(ExperienceGrowthRate, currentLevel);
        }
        
        public SkillTreeDefinition GetSkillTree(SkillTreeType treeType)
        {
            return SkillTrees.Find(t => t.TreeType == treeType);
        }
        
        public bool CanUnlockSkill(string skillId, SkillProgressionProfile profile)
        {
            var requirement = UnlockRequirements.Find(r => r.SkillId == skillId);
            if (requirement == null) return true;
            
            return EvaluateUnlockRequirement(requirement, profile);
        }
        
        private bool EvaluateUnlockRequirement(SkillUnlockRequirement requirement, SkillProgressionProfile profile)
        {
            if (profile.Level < requirement.RequiredLevel) return false;
            if (profile.TotalExperience < requirement.RequiredExperience) return false;
            
            foreach (var prerequisite in requirement.PrerequisiteSkills)
            {
                if (!profile.UnlockedSkills.Contains(prerequisite))
                    return false;
            }
            
            return true;
        }
    }
    
    [System.Serializable]
    public class SkillTreeDefinition
    {
        public string TreeName;
        public SkillTreeType TreeType;
        public List<SkillNodeDefinition> Skills = new List<SkillNodeDefinition>();
        [Range(0.1f, 3.0f)] public float TreeBonusMultiplier = 1.0f;
        public string Description;
        public Sprite TreeIcon;
    }
    
    [System.Serializable]
    public class SkillNodeDefinition
    {
        public string SkillId;
        public string SkillName;
        public SkillCategory Category;
        [Range(1, 10)] public int MaxLevel = 5;
        [Range(0.1f, 5.0f)] public float EffectPerLevel = 0.2f;
        public List<SkillEffect> Effects = new List<SkillEffect>();
        public string Description;
        public Sprite SkillIcon;
    }
    
    [System.Serializable]
    public class SkillEffect
    {
        public string EffectName;
        public SkillEffectType EffectType;
        [Range(0.01f, 2.0f)] public float EffectValue = 0.1f;
        public string Description;
    }
    
    [System.Serializable]
    public class SkillUnlockRequirement
    {
        public string SkillId;
        [Range(1, 100)] public int RequiredLevel = 1;
        [Range(0f, 100000f)] public float RequiredExperience = 0f;
        public List<string> PrerequisiteSkills = new List<string>();
        public List<string> RequiredAchievements = new List<string>();
    }
    
    [System.Serializable]
    public class SkillProgressionProfile
    {
        public int Level;
        public float TotalExperience;
        public List<string> UnlockedSkills = new List<string>();
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
    }
    
    public enum SkillTreeType
    {
        Cultivation,
        Genetics,
        Aromatics,
        Business,
        Technology,
        Research,
        Community,
        Innovation
    }
    
    public enum SkillEffectType
    {
        GrowthRateBonus,
        YieldIncrease,
        QualityBonus,
        EfficiencyGain,
        CostReduction,
        TimeReduction,
        AutomationBonus,
        ExperienceMultiplier
    }
}