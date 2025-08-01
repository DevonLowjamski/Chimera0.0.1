using System;
using System.Collections.Generic;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Interface for Achievement Display Service - defines contract for UI notifications and celebrations
    /// Part of the decomposed AchievementSystemManager architecture
    /// </summary>
    public interface IAchievementDisplayService
    {
        #region Properties
        
        bool IsInitialized { get; }
        string ServiceName { get; }
        int ActiveNotificationCount { get; }
        int QueuedNotificationCount { get; }
        bool IsDisplayingNotifications { get; }
        
        #endregion

        #region Events
        
        event Action<AchievementNotification> OnNotificationDisplayed;
        event Action<AchievementNotification> OnNotificationCompleted;
        event Action<Achievement, CelebrationStyle> OnCelebrationStarted;
        event Action<Achievement> OnCelebrationCompleted;
        event Action<string, float> OnProgressIndicatorUpdated;
        
        #endregion

        #region Display Management
        
        void ShowAchievementNotification(Achievement achievement, RewardBundle rewards = null);
        void ShowProgressNotification(Achievement achievement, float progressPercentage);
        void ClearAllNotifications();
        List<AchievementNotification> GetActiveNotifications();
        DisplayStatistics GetDisplayStatistics();
        
        #endregion

        #region Service Lifecycle
        
        void Initialize();
        void Shutdown();
        
        #endregion
    }
}