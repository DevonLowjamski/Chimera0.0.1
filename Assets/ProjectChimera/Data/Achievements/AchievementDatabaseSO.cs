using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Achievements
{
    /// <summary>
    /// Achievement database ScriptableObjects for Phase 4.3.b Legacy Achievement System
    /// Uses achievement data structures from AchievementDataStructures.cs
    /// </summary>
    
    [CreateAssetMenu(fileName = "New Achievement Database", menuName = "Project Chimera/Achievements/Achievement Database")]
    public class AchievementDatabaseSO : ScriptableObject
    {
        [Header("Achievement Database")]
        public List<BaseAchievement> AllAchievements = new List<BaseAchievement>();
        
        [Header("Progressive Achievements")]
        public List<ProgressiveAchievement> ProgressiveAchievements = new List<ProgressiveAchievement>();
        
        [Header("Hidden Achievements")]
        public List<HiddenAchievement> HiddenAchievements = new List<HiddenAchievement>();
        
        [Header("Community Achievements")]
        public List<CommunityAchievement> CommunityAchievements = new List<CommunityAchievement>();
        
        [Header("Categories")]
        public List<AchievementCategorySO> Categories = new List<AchievementCategorySO>();
        
        [Header("Templates")]
        public List<AchievementTemplateSO> Templates = new List<AchievementTemplateSO>();
    }
    
    [CreateAssetMenu(fileName = "New Achievement Category", menuName = "Project Chimera/Achievements/Achievement Category")]
    public class AchievementCategorySO : ScriptableObject
    {
        [Header("Category Information")]
        public string CategoryName;
        public string Description;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        
        [Header("Settings")]
        public int MaxAchievementsInCategory = 50;
        public bool IsHiddenCategory = false;
    }
    
    [CreateAssetMenu(fileName = "New Achievement Template", menuName = "Project Chimera/Achievements/Achievement Template")]
    public class AchievementTemplateSO : ScriptableObject
    {
        [Header("Template Information")]
        public string TemplateName;
        public string TemplateDescription;
        
        [Header("Default Values")]
        public AchievementCategory DefaultCategory;
        public AchievementRarity DefaultRarity;
        public AchievementType DefaultType;
        public long DefaultExperienceReward = 100;
        public float DefaultDifficultyRating = 1.0f;
    }
}