using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject library containing all achievement definitions
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementLibrary", menuName = "Project Chimera/Progression/Achievement Library")]
    public class AchievementLibrarySO : ScriptableObject
    {
        [Header("Achievement Library")]
        [SerializeField] private List<AchievementDefinition> _achievements = new List<AchievementDefinition>();
        
        [Header("Categories")]
        [SerializeField] private List<AchievementCategory> _availableCategories = new List<AchievementCategory>();
        
        [Header("Configuration")]
        [SerializeField] private bool _enableHiddenAchievements = true;
        [SerializeField] private bool _enableRepeatableAchievements = true;
        [SerializeField] private float _globalExperienceMultiplier = 1f;
        
        public List<AchievementDefinition> Achievements => _achievements;
        public List<AchievementCategory> AvailableCategories => _availableCategories;
        public bool EnableHiddenAchievements => _enableHiddenAchievements;
        public bool EnableRepeatableAchievements => _enableRepeatableAchievements;
        public float GlobalExperienceMultiplier => _globalExperienceMultiplier;
        
        /// <summary>
        /// Get achievement by ID
        /// </summary>
        public AchievementDefinition GetAchievement(string achievementId)
        {
            return _achievements.Find(a => a.AchievementId == achievementId);
        }
        
        /// <summary>
        /// Get achievements by category
        /// </summary>
        public List<AchievementDefinition> GetAchievementsByCategory(AchievementCategory category)
        {
            return _achievements.FindAll(a => a.Category == category);
        }
        
        /// <summary>
        /// Get achievements by type
        /// </summary>
        public List<AchievementDefinition> GetAchievementsByType(AchievementType type)
        {
            return _achievements.FindAll(a => a.Type == type);
        }
        
        /// <summary>
        /// Get all visible achievements (non-hidden)
        /// </summary>
        public List<AchievementDefinition> GetVisibleAchievements()
        {
            return _achievements.FindAll(a => !a.IsHidden);
        }
        
        /// <summary>
        /// Get all achievements in the library
        /// </summary>
        public List<AchievementDefinition> GetAllAchievements()
        {
            return _achievements;
        }
    }
} 