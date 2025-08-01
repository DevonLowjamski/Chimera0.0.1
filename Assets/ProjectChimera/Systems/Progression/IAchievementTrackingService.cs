using System;
using System.Collections.Generic;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Interface for Achievement Tracking Service - defines contract for progress tracking and validation
    /// Part of the decomposed AchievementSystemManager architecture
    /// </summary>
    public interface IAchievementTrackingService
    {
        #region Properties
        
        bool IsInitialized { get; }
        int ActiveAchievementCount { get; }
        int CompletedAchievementCount { get; }
        float TotalProgressPercentage { get; }
        
        #endregion

        #region Events
        
        event Action<string, float> OnProgressUpdated;
        event Action<string, ProjectChimera.Data.Achievements.AchievementProgress> OnAchievementUnlocked;
        event Action<string, string> OnProgressMilestone;
        event Action<ProgressValidationResult> OnValidationCompleted;
        
        #endregion

        #region Progress Tracking
        
        void UpdateProgress(string triggerEvent, float value = 1f, string playerId = "current_player");
        ProjectChimera.Data.Achievements.AchievementProgress GetProgress(string achievementId);
        List<ProjectChimera.Data.Achievements.AchievementProgress> GetAllProgress();
        bool IsAchievementCompleted(string achievementId);
        float GetEventCounter(string eventName);
        
        #endregion

        #region Validation
        
        ProgressValidationResult ValidateProgress(string achievementId, string playerId = "current_player");
        
        #endregion
    }

    /// <summary>
    /// Result of progress validation operations
    /// </summary>
    [System.Serializable]
    public class ProgressValidationResult
    {
        public string AchievementId;
        public string PlayerId;
        public bool IsValid;
        public string ErrorMessage;
        public float ProgressPercentage;
        public DateTime ValidationTime;
    }
}