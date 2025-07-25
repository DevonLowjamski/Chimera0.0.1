using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject configuration for experience and leveling system
    /// </summary>
    [CreateAssetMenu(fileName = "ExperienceConfig", menuName = "Project Chimera/Progression/Experience Config")]
    public class ExperienceConfigSO : ScriptableObject
    {
        [Header("Level Configuration")]
        [SerializeField] private int _maxPlayerLevel = 100;
        [SerializeField] private float _baseExperienceRequired = 100f;
        [SerializeField] private float _experienceScalingFactor = 1.2f;
        [SerializeField] private int _skillPointsPerLevel = 1;
        
        [Header("Experience Sources")]
        [SerializeField] private List<ExperienceSourceConfig> _experienceSources = new List<ExperienceSourceConfig>();
        
        [Header("Skill Experience")]
        [SerializeField] private int _maxSkillLevel = 20;
        [SerializeField] private float _baseSkillExperience = 50f;
        [SerializeField] private float _skillExperienceScaling = 1.15f;
        
        [Header("Bonuses")]
        [SerializeField] private float _globalExperienceMultiplier = 1f;
        [SerializeField] private bool _enableExperienceBonuses = true;
        [SerializeField] private float _teachingExperienceBonus = 0.5f;
        [SerializeField] private float _collaborationExperienceBonus = 0.3f;
        
        public int MaxPlayerLevel => _maxPlayerLevel;
        public float BaseExperienceRequired => _baseExperienceRequired;
        public float ExperienceScalingFactor => _experienceScalingFactor;
        public int SkillPointsPerLevel => _skillPointsPerLevel;
        public List<ExperienceSourceConfig> ExperienceSources => _experienceSources;
        public int MaxSkillLevel => _maxSkillLevel;
        public float BaseSkillExperience => _baseSkillExperience;
        public float SkillExperienceScaling => _skillExperienceScaling;
        public float GlobalExperienceMultiplier => _globalExperienceMultiplier;
        public bool EnableExperienceBonuses => _enableExperienceBonuses;
        public float TeachingExperienceBonus => _teachingExperienceBonus;
        public float CollaborationExperienceBonus => _collaborationExperienceBonus;
        
        /// <summary>
        /// Calculate experience required for a specific level
        /// </summary>
        public float GetExperienceRequiredForLevel(int level)
        {
            if (level <= 1) return 0f;
            return _baseExperienceRequired * Mathf.Pow(_experienceScalingFactor, level - 2);
        }
        
        /// <summary>
        /// Calculate skill experience required for a specific skill level
        /// </summary>
        public float GetSkillExperienceRequiredForLevel(int level)
        {
            if (level <= 1) return 0f;
            return _baseSkillExperience * Mathf.Pow(_skillExperienceScaling, level - 2);
        }
        
        /// <summary>
        /// Get experience value for a specific source
        /// </summary>
        public float GetExperienceForSource(ExperienceSource source)
        {
            var config = _experienceSources.Find(s => s.Source == source);
            return config != null ? config.BaseExperience * _globalExperienceMultiplier : 0f;
        }
    }
    
    /// <summary>
    /// Configuration for experience sources
    /// </summary>
    [System.Serializable]
    public class ExperienceSourceConfig
    {
        public ExperienceSource Source;
        public float BaseExperience = 10f;
        public float QualityMultiplier = 1f;
        public bool ScalesWithLevel = true;
        public string Description;
    }
} 