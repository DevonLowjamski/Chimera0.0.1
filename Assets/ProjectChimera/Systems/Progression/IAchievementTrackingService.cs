using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Progression.Achievements;
using AchievementProgress = ProjectChimera.Data.Progression.Achievements.AchievementProgress;

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
        event Action<string, AchievementProgress> OnAchievementUnlocked;
        event Action<string, string> OnProgressMilestone;
        event Action<ProgressValidationResult> OnValidationCompleted;
        
        #endregion

        #region Progress Tracking
        
        void UpdateProgress(string triggerEvent, float value = 1f, string playerId = "current_player");
        AchievementProgress GetProgress(string achievementId);
        List<AchievementProgress> GetAllProgress();
        bool IsAchievementCompleted(string achievementId);
        float GetEventCounter(string eventName);
        
        #endregion

        #region Validation
        
        ProgressValidationResult ValidateProgress(string achievementId, string playerId = "current_player");
        
        #endregion

        #region Achievement Queries
        
        Achievement GetAchievementById(string achievementId);
        List<Achievement> GetAchievementsByCategory(AchievementCategory category);
        List<Achievement> GetAchievementsByRarity(AchievementRarity rarity);
        int GetCompletedAchievementCount(string playerId);
        float GetTotalAchievementPoints(string playerId);
        
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