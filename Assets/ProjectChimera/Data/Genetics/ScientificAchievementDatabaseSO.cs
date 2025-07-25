using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Achievement Database - Collection of achievements for Enhanced Scientific Gaming System v2.0
    /// Contains all achievement definitions for genetics, aromatics, competition, and community systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Achievement Database", menuName = "Project Chimera/Gaming/Scientific Achievement Database")]
    public class ScientificAchievementDatabaseSO : ChimeraDataSO
    {
        [Header("Achievement Collection")]
        public List<ScientificAchievement> Achievements = new List<ScientificAchievement>();
        
        [Header("Achievement Categories")]
        public List<AchievementCategoryData> Categories = new List<AchievementCategoryData>();
        
        [Header("Legacy Achievements")]
        public List<ScientificAchievement> LegacyAchievements = new List<ScientificAchievement>();
        
        #region Runtime Methods
        
        public ScientificAchievement GetAchievement(string achievementId)
        {
            return Achievements.Find(a => a.AchievementID == achievementId);
        }
        
        public List<ScientificAchievement> GetAchievementsByCategory(ScientificAchievementCategory category)
        {
            return Achievements.FindAll(a => a.Category == category);
        }
        
        public List<ScientificAchievement> GetLegacyAchievements()
        {
            return LegacyAchievements;
        }
        
        public bool IsLegacyAchievement(string achievementId)
        {
            return LegacyAchievements.Exists(a => a.AchievementID == achievementId);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class ScientificAchievement
    {
        public string AchievementID;
        public string DisplayName;
        public ScientificAchievementCategory Category;
        public List<AchievementCriterion> Criteria = new List<AchievementCriterion>();
        public float ReputationReward;
        public bool IsCrossSystemAchievement;
        public bool IsLegacyAchievement;
        public string Description;
        public Sprite AchievementIcon;
        
        // Additional properties for Systems compatibility
        public string AchievementId 
        { 
            get => AchievementID; 
            set => AchievementID = value; 
        }
        public string Name 
        { 
            get => DisplayName; 
            set => DisplayName = value; 
        }
        public AchievementRarity Rarity = AchievementRarity.Common;
        public List<string> UnlockRequirements = new List<string>();
        
        // Additional properties for ScientificAchievementManager compatibility
        public System.DateTime CreatedAt;
        public System.DateTime UnlockedDate;
        public string Value;
        
        // Default constructor
        public ScientificAchievement()
        {
            CreatedAt = System.DateTime.Now;
            UnlockedDate = System.DateTime.MinValue;
        }
        
        // Constructor with achievement data
        public ScientificAchievement(ScientificAchievementCategory category)
        {
            Category = category;
            CreatedAt = System.DateTime.Now;
            UnlockedDate = System.DateTime.MinValue;
        }
        
        // Copy constructor for creating unlocked instances
        public ScientificAchievement(ScientificAchievement source)
        {
            AchievementID = source.AchievementID;
            DisplayName = source.DisplayName;
            Category = source.Category;
            Criteria = new List<AchievementCriterion>(source.Criteria);
            ReputationReward = source.ReputationReward;
            IsCrossSystemAchievement = source.IsCrossSystemAchievement;
            IsLegacyAchievement = source.IsLegacyAchievement;
            Description = source.Description;
            AchievementIcon = source.AchievementIcon;
            Rarity = source.Rarity;
            UnlockRequirements = new List<string>(source.UnlockRequirements);
            CreatedAt = source.CreatedAt;
            UnlockedDate = System.DateTime.MinValue;
            Value = source.Value;
        }
    }
    
    // AchievementCriterion moved to AchievementDataStructures.cs to resolve namespace conflict
    
    [System.Serializable]
    public class AchievementCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public ScientificAchievementCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
}