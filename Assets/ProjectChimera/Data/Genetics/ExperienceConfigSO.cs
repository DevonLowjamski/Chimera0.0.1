using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Experience Configuration - Defines experience systems, levels, and progression mechanics
    /// for scientific gaming systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Experience Config", menuName = "Project Chimera/Genetics/Experience Config")]
    public class ExperienceConfigSO : ChimeraConfigSO
    {
        [Header("Experience Settings")]
        [Range(0.1f, 10.0f)] public float BaseExperienceGain = 1.0f;
        [Range(0.1f, 5.0f)] public float ExperienceMultiplier = 1.0f;
        [Range(1f, 1000000f)] public float MaxExperience = 100000f;
        [Range(1, 100)] public int MaxLevel = 50;
        
        [Header("Experience Types")]
        public List<ExperienceTypeTemplate> ExperienceTypes = new List<ExperienceTypeTemplate>();
        public List<ExperienceLevelTemplate> ExperienceLevels = new List<ExperienceLevelTemplate>();
        public List<ExperienceModifierTemplate> ExperienceModifiers = new List<ExperienceModifierTemplate>();
        
        [Header("Level Progression")]
        [Range(1f, 10000f)] public float BaseLevelRequirement = 100f;
        [Range(1.1f, 3.0f)] public float LevelProgressionRate = 1.5f;
        public bool EnablePrestige = true;
        [Range(1, 10)] public int MaxPrestigeLevel = 5;
        
        [Header("Bonus Systems")]
        public List<ExperienceBonusTemplate> ExperienceBonuses = new List<ExperienceBonusTemplate>();
        public bool EnableStreakBonuses = true;
        public bool EnableTimeBasedBonuses = true;
        public bool EnableQualityBonuses = true;
        
        [Header("Visual Configuration")]
        public Color ExperienceGainColor = Color.cyan;
        public Color LevelUpColor = Color.gold;
        public Color PrestigeColor = Color.magenta;
        public Color BonusExperienceColor = Color.green;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (ExperienceTypes.Count == 0)
            {
                Debug.LogWarning("No experience types defined", this);
            }
            
            if (ExperienceLevels.Count == 0)
            {
                Debug.LogWarning("No experience levels defined", this);
            }
            
            // Validate level progression
            ValidateLevelProgression();
        }
        
        private void ValidateLevelProgression()
        {
            float lastThreshold = 0f;
            foreach (var level in ExperienceLevels)
            {
                if (level.ExperienceRequirement <= lastThreshold)
                {
                    Debug.LogWarning($"Experience level {level.LevelName} has invalid threshold ordering", this);
                }
                lastThreshold = level.ExperienceRequirement;
            }
        }
    }
    
    [System.Serializable]
    public class ExperienceTypeTemplate
    {
        public string TypeId;
        public string TypeName;
        public ExperienceType Type;
        public string Description;
        [Range(0.1f, 10.0f)] public float BaseValue = 1.0f;
        [Range(0.1f, 5.0f)] public float GrowthRate = 1.2f;
        public Color TypeColor = Color.white;
        public Sprite TypeIcon;
        public List<string> ApplicableSystems = new List<string>();
        public bool RequiresValidation = false;
    }
    
    [System.Serializable]
    public class ExperienceLevelTemplate
    {
        public string LevelId;
        public string LevelName;
        public ExperienceLevel Level;
        [Range(0f, 1000000f)] public float ExperienceRequirement = 0f;
        [Range(0.1f, 5.0f)] public float LevelMultiplier = 1.0f;
        public List<string> UnlockedFeatures = new List<string>();
        public List<string> UnlockedRewards = new List<string>();
        public Sprite LevelIcon;
        public string Description;
        public bool IsPrestigeLevel = false;
    }
    
    [System.Serializable]
    public class ExperienceModifierTemplate
    {
        public string ModifierId;
        public string ModifierName;
        public ExperienceModifierType ModifierType;
        public string Description;
        [Range(0.1f, 10.0f)] public float ModifierValue = 1.0f;
        public List<ExperienceConditionTemplate> Conditions = new List<ExperienceConditionTemplate>();
        public bool IsTemporary = false;
        public float Duration = 0f; // 0 = permanent
        public bool StacksWithOthers = true;
    }
    
    [System.Serializable]
    public class ExperienceConditionTemplate
    {
        public string ConditionId;
        public string ConditionName;
        public ExperienceConditionType ConditionType;
        public float ConditionValue;
        public string Description;
        public bool IsRequired = true;
    }
    
    [System.Serializable]
    public class ExperienceBonusTemplate
    {
        public string BonusId;
        public string BonusName;
        public ExperienceBonusType BonusType;
        public string Description;
        [Range(0.1f, 10.0f)] public float BonusMultiplier = 1.5f;
        public List<string> TriggerConditions = new List<string>();
        public float Duration = 0f; // 0 = instant
        public bool IsRepeatable = true;
        public Sprite BonusIcon;
    }
    
    public enum ExperienceBonusType
    {
        StreakBonus,
        TimeBasedBonus,
        QualityBonus,
        FirstTimeBonus,
        CompletionBonus,
        CombinationBonus,
        SocialBonus,
        SeasonalBonus
    }
}