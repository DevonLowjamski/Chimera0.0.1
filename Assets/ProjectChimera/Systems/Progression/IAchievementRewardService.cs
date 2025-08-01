using System;
using System.Collections.Generic;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Interface for Achievement Reward Service - defines contract for reward calculation and distribution
    /// Part of the decomposed AchievementSystemManager architecture
    /// </summary>
    public interface IAchievementRewardService
    {
        #region Properties
        
        bool IsInitialized { get; }
        string ServiceName { get; }
        IReadOnlyList<RewardTransaction> RewardHistory { get; }
        float TotalRewardsDistributed { get; }
        
        #endregion

        #region Events
        
        event Action<string, RewardBundle> OnRewardCalculated;
        event Action<string, RewardBundle> OnRewardDistributed;
        event Action<string, string, float> OnCurrencyRewarded;
        event Action<string, int> OnExperienceRewarded;
        event Action<string, List<ItemReward>> OnItemsRewarded;
        event Action<RewardTransaction> OnRewardTransactionCompleted;
        
        #endregion

        #region Reward Management
        
        RewardBundle CalculateRewards(Achievement achievement, string playerId = "current_player");
        bool DistributeRewards(RewardBundle rewardBundle);
        float GetPlayerTotalRewards(string playerId);
        List<RewardTransaction> GetPlayerRewardHistory(string playerId);
        RewardStatistics GetRewardStatistics();
        
        #endregion

        #region Service Lifecycle
        
        void Initialize();
        void Shutdown();
        
        #endregion
    }
}