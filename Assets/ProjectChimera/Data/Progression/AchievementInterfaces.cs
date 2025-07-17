using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Core interface for all achievement trigger systems in Project Chimera.
    /// Provides a unified contract for cultivation, environmental, facility, economic,
    /// and educational achievement detection across all gameplay systems.
    /// </summary>
    public interface IAchievementTrigger
    {
        /// <summary>
        /// Unique identifier for this trigger type
        /// </summary>
        string TriggerId { get; }
        
        /// <summary>
        /// Category of achievement this trigger can activate
        /// </summary>
        AchievementCategory Category { get; }
        
        /// <summary>
        /// Type of trigger event
        /// </summary>
        AchievementTriggerType TriggerType { get; }
        
        /// <summary>
        /// Process the trigger and determine if any achievements should be unlocked
        /// </summary>
        /// <param name="context">Current achievement context</param>
        /// <param name="playerProfile">Player's achievement profile</param>
        /// <returns>List of achievements that should be processed</returns>
        List<string> ProcessTrigger(AchievementContext context, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Validate if this trigger can process the given data
        /// </summary>
        /// <param name="triggerData">Data to validate</param>
        /// <returns>True if trigger can process this data</returns>
        bool CanProcessTrigger(object triggerData);
        
        /// <summary>
        /// Get the priority of this trigger (higher priority triggers are processed first)
        /// </summary>
        int GetPriority();
    }
    
    /// <summary>
    /// Interface for achievement progress tracking and calculation
    /// </summary>
    public interface IAchievementProgress
    {
        /// <summary>
        /// Achievement this progress tracker is monitoring
        /// </summary>
        string AchievementId { get; }
        
        /// <summary>
        /// Current progress value (0.0 to 1.0)
        /// </summary>
        float CurrentProgress { get; set; }
        
        /// <summary>
        /// Calculate progress based on provided data
        /// </summary>
        /// <param name="progressData">Data to calculate progress from</param>
        /// <param name="context">Current achievement context</param>
        /// <returns>Updated progress value</returns>
        float CalculateProgress(object progressData, AchievementContext context);
        
        /// <summary>
        /// Check if achievement is complete
        /// </summary>
        /// <returns>True if progress >= 1.0</returns>
        bool IsComplete();
        
        /// <summary>
        /// Reset progress to initial state
        /// </summary>
        void ResetProgress();
        
        /// <summary>
        /// Get detailed progress information
        /// </summary>
        /// <returns>Progress details for UI display</returns>
        AchievementProgressInfo GetProgressInfo();
    }
    
    /// <summary>
    /// Interface for milestone tracking and management
    /// </summary>
    public interface IMilestoneTracker
    {
        /// <summary>
        /// Milestone this tracker is monitoring
        /// </summary>
        string MilestoneId { get; }
        
        /// <summary>
        /// Update milestone progress based on achievement unlocks
        /// </summary>
        /// <param name="unlockedAchievement">Recently unlocked achievement</param>
        /// <param name="playerProfile">Player's current profile</param>
        /// <returns>Updated milestone progress</returns>
        float UpdateMilestoneProgress(UnlockedAchievement unlockedAchievement, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Check if milestone is complete
        /// </summary>
        /// <param name="playerProfile">Player's current profile</param>
        /// <returns>True if milestone requirements are met</returns>
        bool IsMilestoneComplete(PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Get milestone requirements that are still needed
        /// </summary>
        /// <param name="playerProfile">Player's current profile</param>
        /// <returns>List of remaining requirements</returns>
        List<MilestoneRequirement> GetRemainingRequirements(PlayerAchievementProfile playerProfile);
    }
    
    /// <summary>
    /// Interface for reward calculation and distribution
    /// </summary>
    public interface IRewardCalculator
    {
        /// <summary>
        /// Calculate rewards for achievement unlock
        /// </summary>
        /// <param name="achievement">Unlocked achievement</param>
        /// <param name="context">Achievement context</param>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Calculated rewards</returns>
        List<AchievementReward> CalculateAchievementRewards(AchievementSO achievement, AchievementContext context, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Calculate rewards for milestone completion
        /// </summary>
        /// <param name="milestone">Completed milestone</param>
        /// <param name="context">Milestone context</param>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Calculated rewards</returns>
        List<AchievementReward> CalculateMilestoneRewards(MilestoneSO milestone, MilestoneContext context, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Apply reward multipliers based on player status
        /// </summary>
        /// <param name="baseRewards">Base reward values</param>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Modified rewards with multipliers applied</returns>
        List<AchievementReward> ApplyRewardMultipliers(List<AchievementReward> baseRewards, PlayerAchievementProfile playerProfile);
    }
    
    /// <summary>
    /// Interface for social recognition features
    /// </summary>
    public interface ISocialRecognition
    {
        /// <summary>
        /// Process achievement unlock for social features
        /// </summary>
        /// <param name="unlockedAchievement">Achievement that was unlocked</param>
        void ProcessAchievementUnlock(UnlockedAchievement unlockedAchievement);
        
        /// <summary>
        /// Get social recognition level for player
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Social recognition data</returns>
        SocialRecognitionData GetSocialRecognition(PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Update player's reputation based on achievements
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Updated reputation score</returns>
        float UpdateReputation(PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Get leaderboard position for player
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <param name="category">Achievement category</param>
        /// <returns>Leaderboard position and stats</returns>
        LeaderboardPosition GetLeaderboardPosition(PlayerAchievementProfile playerProfile, AchievementCategory category);
    }
    
    /// <summary>
    /// Interface for achievement analytics and intelligence
    /// </summary>
    public interface IAchievementAnalytics
    {
        /// <summary>
        /// Track achievement trigger event
        /// </summary>
        /// <param name="trigger">Trigger that was fired</param>
        void TrackAchievementTrigger(AchievementTrigger trigger);
        
        /// <summary>
        /// Track achievement unlock event
        /// </summary>
        /// <param name="unlockedAchievement">Achievement that was unlocked</param>
        void TrackAchievementUnlock(UnlockedAchievement unlockedAchievement);
        
        /// <summary>
        /// Track milestone completion event
        /// </summary>
        /// <param name="milestoneId">Milestone that was completed</param>
        /// <param name="milestone">Milestone data</param>
        void TrackMilestoneCompletion(string milestoneId, MilestoneSO milestone);
        
        /// <summary>
        /// Get personalized achievement recommendations
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Recommended achievements</returns>
        List<AchievementSO> GetPersonalizedRecommendations(PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Predict player's next likely achievements
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Predicted achievements with probability scores</returns>
        List<AchievementPrediction> PredictNextAchievements(PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Generate analytics report
        /// </summary>
        /// <returns>Comprehensive analytics data</returns>
        AchievementAnalyticsReport GenerateAnalyticsReport();
    }
    
    /// <summary>
    /// Interface for cultivation-specific achievement detection
    /// </summary>
    public interface ICultivationAchievementDetector
    {
        /// <summary>
        /// Monitor plant lifecycle events for achievements
        /// </summary>
        /// <param name="plantEvent">Plant lifecycle event data</param>
        void MonitorPlantLifecycle(PlantLifecycleEventData plantEvent);
        
        /// <summary>
        /// Monitor irrigation and fertigation events
        /// </summary>
        /// <param name="irrigationEvent">Irrigation event data</param>
        void MonitorIrrigationEvent(IrrigationEventData irrigationEvent);
        
        /// <summary>
        /// Monitor environmental control achievements
        /// </summary>
        /// <param name="environmentalEvent">Environmental event data</param>
        void MonitorEnvironmentalControl(EnvironmentalEventData environmentalEvent);
        
        /// <summary>
        /// Monitor IPM (Integrated Pest Management) activities
        /// </summary>
        /// <param name="ipmEvent">IPM event data</param>
        void MonitorIPMActivity(IPMEventData ipmEvent);
        
        /// <summary>
        /// Monitor harvest and post-harvest achievements
        /// </summary>
        /// <param name="harvestEvent">Harvest event data</param>
        void MonitorHarvestActivity(HarvestEventData harvestEvent);
        
        /// <summary>
        /// Evaluate cultivation mastery achievements
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>List of mastery achievements to unlock</returns>
        List<string> EvaluateCultivationMastery(PlayerAchievementProfile playerProfile);
    }
    
    /// <summary>
    /// Interface for facility construction achievement detection
    /// </summary>
    public interface IFacilityAchievementDetector
    {
        /// <summary>
        /// Monitor structural construction achievements
        /// </summary>
        /// <param name="constructionEvent">Construction event data</param>
        void MonitorConstruction(ConstructionEventData constructionEvent);
        
        /// <summary>
        /// Monitor equipment placement and optimization
        /// </summary>
        /// <param name="equipmentEvent">Equipment placement event data</param>
        void MonitorEquipmentPlacement(EquipmentEventData equipmentEvent);
        
        /// <summary>
        /// Monitor utility system installation
        /// </summary>
        /// <param name="utilityEvent">Utility system event data</param>
        void MonitorUtilityInstallation(UtilityEventData utilityEvent);
        
        /// <summary>
        /// Evaluate facility efficiency achievements
        /// </summary>
        /// <param name="facilityData">Current facility state</param>
        /// <returns>List of efficiency achievements to unlock</returns>
        List<string> EvaluateFacilityEfficiency(FacilityStateData facilityData);
    }
    
    /// <summary>
    /// Interface for economic achievement detection
    /// </summary>
    public interface IEconomicAchievementDetector
    {
        /// <summary>
        /// Monitor financial performance achievements
        /// </summary>
        /// <param name="financialEvent">Financial event data</param>
        void MonitorFinancialPerformance(FinancialEventData financialEvent);
        
        /// <summary>
        /// Monitor resource efficiency achievements
        /// </summary>
        /// <param name="resourceEvent">Resource usage event data</param>
        void MonitorResourceEfficiency(ResourceEventData resourceEvent);
        
        /// <summary>
        /// Monitor market performance achievements
        /// </summary>
        /// <param name="marketEvent">Market event data</param>
        void MonitorMarketPerformance(MarketEventData marketEvent);
        
        /// <summary>
        /// Evaluate economic mastery achievements
        /// </summary>
        /// <param name="economicData">Current economic state</param>
        /// <returns>List of economic achievements to unlock</returns>
        List<string> EvaluateEconomicMastery(EconomicStateData economicData);
    }
    
    /// <summary>
    /// Interface for progressive achievement systems
    /// </summary>
    public interface IProgressiveAchievement : IAchievementProgress
    {
        /// <summary>
        /// Achievement tiers for progressive unlocks
        /// </summary>
        List<AchievementTier> Tiers { get; }
        
        /// <summary>
        /// Current tier index
        /// </summary>
        int CurrentTier { get; set; }
        
        /// <summary>
        /// Check if next tier is unlocked
        /// </summary>
        /// <param name="progressData">Current progress data</param>
        /// <returns>True if next tier should be unlocked</returns>
        bool CheckTierUnlock(object progressData);
        
        /// <summary>
        /// Get requirements for next tier
        /// </summary>
        /// <returns>Next tier requirements</returns>
        AchievementTierRequirements GetNextTierRequirements();
        
        /// <summary>
        /// Unlock next tier
        /// </summary>
        /// <returns>Newly unlocked tier</returns>
        AchievementTier UnlockNextTier();
    }
    
    /// <summary>
    /// Interface for hidden achievement detection
    /// </summary>
    public interface IHiddenAchievementDetector
    {
        /// <summary>
        /// Detect hidden achievement triggers
        /// </summary>
        /// <param name="gameplayData">Current gameplay state</param>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>List of hidden achievements to check</returns>
        List<string> DetectHiddenTriggers(GameplayStateData gameplayData, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Evaluate complex trigger combinations
        /// </summary>
        /// <param name="triggerHistory">Recent trigger history</param>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Hidden achievements that meet complex conditions</returns>
        List<string> EvaluateComplexTriggers(List<AchievementTrigger> triggerHistory, PlayerAchievementProfile playerProfile);
        
        /// <summary>
        /// Check for secret achievement conditions
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <returns>Secret achievements to unlock</returns>
        List<string> CheckSecretConditions(PlayerAchievementProfile playerProfile);
    }
    
    /// <summary>
    /// Interface for community achievement coordination
    /// </summary>
    public interface ICommunityAchievementCoordinator
    {
        /// <summary>
        /// Process individual contribution to community goal
        /// </summary>
        /// <param name="contribution">Player's contribution data</param>
        void ProcessCommunityContribution(CommunityContributionData contribution);
        
        /// <summary>
        /// Update community achievement progress
        /// </summary>
        /// <param name="communityData">Current community state</param>
        void UpdateCommunityProgress(CommunityStateData communityData);
        
        /// <summary>
        /// Check for community achievement unlocks
        /// </summary>
        /// <returns>Community achievements ready to unlock</returns>
        List<string> CheckCommunityUnlocks();
        
        /// <summary>
        /// Get player's community contribution rank
        /// </summary>
        /// <param name="playerProfile">Player's profile</param>
        /// <param name="communityGoalId">Community goal identifier</param>
        /// <returns>Player's rank and contribution data</returns>
        CommunityRankData GetCommunityRank(PlayerAchievementProfile playerProfile, string communityGoalId);
    }
}