using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics.Scientific;
using RewardType = ProjectChimera.Data.Genetics.Scientific.RewardType;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Achievement Configuration - Defines achievements, milestones, and recognition systems
    /// for scientific gaming systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement Config", menuName = "Project Chimera/Genetics/Achievement Config")]
    public class AchievementConfigSO : ChimeraConfigSO
    {
        [Header("Achievement Settings")]
        [Range(1, 1000)] public int MaxAchievements = 500;
        [Range(0.1f, 5.0f)] public float AchievementValueMultiplier = 1.0f;
        public bool EnableHiddenAchievements = true;
        public bool EnableProgressiveAchievements = true;
        
        [Header("Achievement Categories")]
        public List<AchievementCategoryTemplate> AchievementCategories = new List<AchievementCategoryTemplate>();
        public List<AchievementTemplate> AchievementTemplates = new List<AchievementTemplate>();
        public List<MilestoneTemplate> MilestoneTemplates = new List<MilestoneTemplate>();
        
        [Header("Reward Configuration")]
        public List<RewardTemplate> RewardTemplates = new List<RewardTemplate>();
        [Range(0.1f, 10.0f)] public float BaseRewardMultiplier = 1.0f;
        public bool EnableBonusRewards = true;
        
        [Header("Social Features")]
        public bool EnableSocialSharing = true;
        public bool EnableLeaderboards = true;
        public bool EnablePeerEndorsement = true;
        
        [Header("Visual Configuration")]
        public Color UnlockedAchievementColor = Color.gold;
        public Color LockedAchievementColor = Color.gray;
        public Color ProgressiveAchievementColor = Color.cyan;
        public Color HiddenAchievementColor = Color.magenta;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (AchievementCategories.Count == 0)
            {
                Debug.LogWarning("No achievement categories defined", this);
            }
            
            if (AchievementTemplates.Count == 0)
            {
                Debug.LogWarning("No achievement templates defined", this);
            }
        }
    }
    
    [System.Serializable]
    public class AchievementCategoryTemplate
    {
        public string CategoryId;
        public string CategoryName;
        public AchievementCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
        [Range(0.1f, 5.0f)] public float CategoryMultiplier = 1.0f;
    }
    
    [System.Serializable]
    public class AchievementTemplate
    {
        public string AchievementId;
        public string AchievementName;
        public string Description;
        public AchievementCategory Category;
        public AchievementType Type;
        public AchievementRarity Rarity;
        [Range(1f, 1000f)] public float PointValue = 10f;
        public List<AchievementCriterion> Criteria = new List<AchievementCriterion>();
        public List<string> RewardIds = new List<string>();
        public Sprite AchievementIcon;
        public bool IsHidden = false;
        public bool IsProgressive = false;
    }
    
    [System.Serializable]
    public class MilestoneTemplate
    {
        public string MilestoneId;
        public string MilestoneName;
        public string Description;
        public MilestoneType Type;
        [Range(1f, 10000f)] public float RequiredProgress = 100f;
        public List<string> RewardIds = new List<string>();
        public Sprite MilestoneIcon;
    }
    
    
    [System.Serializable]
    public class RewardTemplate
    {
        public string RewardId;
        public string RewardName;
        public RewardType Type;
        public float RewardValue;
        public string Description;
        public Sprite RewardIcon;
        public bool IsExclusive = false;
    }
    
    public enum AchievementType
    {
        OneTime,
        Progressive,
        Repeatable,
        Conditional,
        Social,
        Hidden,
        Legacy
    }
    
    // AchievementRarity and RewardType moved to AchievementDataStructures.cs to resolve namespace conflict
}