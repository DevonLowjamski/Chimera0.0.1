using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Progression.Achievements;
using AchievementProgress = ProjectChimera.Data.Progression.Achievements.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Interface for Achievement Coordinator - defines contract for achievement service coordination
    /// Part of the decomposed AchievementSystemManager architecture
    /// </summary>
    public interface IAchievementCoordinator
    {
        #region Properties
        
        bool IsInitialized { get; }
        string ServiceName { get; }
        bool AllServicesHealthy { get; }
        CoordinatorStatistics Statistics { get; }
        ServiceHealthStatus ServiceHealth { get; }
        
        #endregion

        #region Events
        
        event Action<Achievement, RewardBundle> OnAchievementCompleted;
        event Action<string, int> OnAchievementStreak;
        event Action<AchievementCategory, float> OnCategoryMasteryUpdated;
        event Action<string, float> OnPointMilestoneReached;
        event Action<MetaAchievementRule> OnMetaAchievementTriggered;
        event Action<ServiceHealthStatus> OnServiceHealthUpdated;
        
        #endregion

        #region Coordination Management
        
        void ProcessAchievementUnlock(Achievement achievement, string playerId = "current_player");
        CoordinatorStatistics GetStatistics();
        ServiceHealthStatus GetServiceHealth();
        List<MetaAchievementRule> GetMetaAchievementRules();
        CategoryMasteryStatus GetCategoryMastery(AchievementCategory category);
        Dictionary<AchievementCategory, CategoryMasteryStatus> GetAllCategoryMastery();
        void ForceMetaAchievementCheck(string playerId);
        void ResetPlayerStreak(string playerId);
        
        #endregion

        #region Service Lifecycle
        
        void Initialize();
        void Shutdown();
        
        #endregion
    }
}