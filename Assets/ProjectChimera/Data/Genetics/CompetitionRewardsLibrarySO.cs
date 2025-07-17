using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Competition Rewards Library - Collection of rewards for scientific competition system
    /// Contains reward definitions, tiers, and distribution rules
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Competition Rewards Library", menuName = "Project Chimera/Gaming/Competition Rewards Library")]
    public class CompetitionRewardsLibrarySO : ChimeraDataSO
    {
        [Header("Reward Collection")]
        public List<CompetitionReward> Rewards = new List<CompetitionReward>();
        
        [Header("Reward Categories")]
        public List<RewardCategoryData> Categories = new List<RewardCategoryData>();
        
        [Header("Reward Tiers")]
        public List<RewardTierData> RewardTiers = new List<RewardTierData>();
        
        #region Runtime Methods
        
        public CompetitionReward GetReward(string rewardID)
        {
            return Rewards.Find(r => r.RewardID == rewardID);
        }
        
        public List<CompetitionReward> GetRewardsByTier(CompetitionTier tier)
        {
            return Rewards.FindAll(r => r.ApplicableTier == tier);
        }
        
        public List<CompetitionReward> GetRewardsByResult(CompetitionResult result)
        {
            return Rewards.FindAll(r => r.ResultType == result);
        }
        
        public RewardTierData GetRewardTier(CompetitionTier tier)
        {
            return RewardTiers.Find(t => t.Tier == tier);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class CompetitionReward
    {
        public string RewardID;
        public string RewardName;
        public CompetitionResult ResultType;
        public CompetitionTier ApplicableTier;
        public float RewardValue;
        public RewardCategory Category;
        public List<RewardComponent> Components = new List<RewardComponent>();
        public bool IsSeasonalReward;
        public bool IsLegacyReward;
        public string Description;
        public Sprite RewardIcon;
    }
    
    [System.Serializable]
    public class RewardCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public RewardCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class RewardTierData
    {
        public string TierName;
        public CompetitionTier Tier;
        public float TierMultiplier;
        public Color TierColor = Color.white;
        public List<string> TierBenefits = new List<string>();
        public Sprite TierIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class RewardComponent
    {
        public string ComponentName;
        public RewardComponentType ComponentType;
        public float ComponentValue;
        public bool IsGuaranteed;
        public float DropChance;
        public string Description;
    }
    
    public enum RewardCategory
    {
        Experience,
        Reputation,
        Achievement,
        Item,
        Currency,
        Feature,
        Title,
        Recognition,
        Legacy
    }
}