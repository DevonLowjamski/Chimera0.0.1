using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Progression.Leveling
{
    /// <summary>
    /// Leveling and experience system data structures extracted from ProgressionDataStructures.cs
    /// Contains player progression, experience tracking, level calculations, and progression events
    /// Part of Phase 1 Foundation Data Structures refactoring
    /// </summary>

    #region Core Leveling Types

    /// <summary>
    /// Experience source enumeration
    /// </summary>
    public enum ExperienceSource
    {
        Plant_Harvest,
        Breeding_Success,
        Research_Completion,
        Quality_Achievement,
        Facility_Completion,
        Skill_Usage,
        Achievement_Unlock,
        Quest_Completion,
        Passive_Time,
        PlantCare,
        Teaching_Others,
        Collaboration,
        Gameplay,
        Research,
        Teaching,
        Innovation,
        Achievement,
        Challenge,
        Mentorship,
        Tutorial_Completion
    }

    /// <summary>
    /// Progression event types
    /// </summary>
    public enum ProgressionEventType
    {
        Skill_Level_Up,
        SkillLevelUp,
        Research_Completed,
        ResearchCompleted,
        Achievement_Unlocked,
        AchievementUnlocked,
        Content_Unlocked,
        Experience_Gained,
        Milestone_Reached,
        Plant_Harvested,
        Construction_Completed,
        Sale_Completed
    }

    /// <summary>
    /// Progression notification types
    /// </summary>
    public enum ProgressionNotificationType
    {
        Skill_Level_Up,
        Achievement_Unlocked,
        Research_Completed,
        Content_Unlocked,
        Milestone_Reached,
        Experience_Gained,
        Level_Up,
        Bonus_Unlocked,
        Warning,
        Info
    }

    /// <summary>
    /// Level calculation methods
    /// </summary>
    public enum LevelCalculationType
    {
        Linear,
        Exponential,
        Logarithmic,
        Hybrid,
        Custom_Curve,
        Milestone_Based,
        Activity_Based,
        Time_Based
    }

    /// <summary>
    /// Experience gain modifiers
    /// </summary>
    public enum ExperienceModifierType
    {
        Base_Multiplier,
        Source_Bonus,
        Time_Bonus,
        Streak_Bonus,
        Premium_Bonus,
        Event_Bonus,
        Social_Bonus,
        Difficulty_Modifier
    }

    #endregion

    #region Player Progression Data

    /// <summary>
    /// Player progression data tracking all advancement across the game
    /// </summary>
    [System.Serializable]
    public class PlayerProgressionData
    {
        [Header("Player Identity")]
        public string PlayerId;
        public int PlayerLevel = 1;
        public float TotalExperience = 0f;
        public int AvailableSkillPoints = 0;
        public float PlayTimeHours = 0f;
        public DateTime CreationDate;
        public DateTime LastPlayDate;
        
        [Header("Completed Content")]
        public List<string> CompletedAchievements = new List<string>();
        public List<string> CompletedResearch = new List<string>();
        public List<string> UnlockedSkills = new List<string>();
        public List<string> UnlockedContent = new List<string>();
        public List<string> CompletedMilestones = new List<string>();
        public List<string> CompletedObjectives = new List<string>();
        
        [Header("Campaign Progress")]
        public CampaignProgressionData CampaignProgress = new CampaignProgressionData();
        public CampaignProgressionData CampaignData = new CampaignProgressionData(); // Alias for compatibility
        
        [Header("Statistics")]
        public PlayerStatTracker StatTracker = new PlayerStatTracker();
        public int FacilitiesBuilt = 0;
        public float TotalProfit = 0f;
        public int ResearchCompleted = 0;
        public int AchievementsUnlocked = 0;
        
        [Header("Skill Levels")]
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
        public Dictionary<string, float> SkillExperience = new Dictionary<string, float>();
        
        [Header("Preferences")]
        public ProgressionPreferences Preferences = new ProgressionPreferences();
        
        // Progression version for save compatibility
        public string ProgressionVersion = "1.0";
        public int OverallLevel = 1;
        
        public int GetSkillLevel(string skillId)
        {
            return SkillLevels.ContainsKey(skillId) ? SkillLevels[skillId] : 1;
        }
        
        public float GetSkillExperience(string skillId)
        {
            return SkillExperience.ContainsKey(skillId) ? SkillExperience[skillId] : 0f;
        }
        
        public bool HasCompletedAchievement(string achievementId)
        {
            return CompletedAchievements.Contains(achievementId);
        }
        
        public bool HasUnlockedSkill(string skillId)
        {
            return UnlockedSkills.Contains(skillId);
        }
        
        public bool HasCompletedResearch(string researchId)
        {
            return CompletedResearch.Contains(researchId);
        }
        
        public bool HasUnlockedContent(string contentId)
        {
            return UnlockedContent.Contains(contentId);
        }
    }

    /// <summary>
    /// Campaign progression data for tracking story and phase progress
    /// </summary>
    [System.Serializable]
    public class CampaignProgressionData
    {
        public CampaignPhase CurrentPhase = CampaignPhase.Tutorial;
        public List<string> CompletedMilestones = new List<string>();
        public List<string> UnlockedStoryContent = new List<string>();
        public float CampaignProgress = 0f;
        public DateTime LastCampaignUpdate;
        public Dictionary<string, bool> PhaseCompletionStatus = new Dictionary<string, bool>();
        
        // Additional properties needed by CampaignManager
        public float PhaseProgress = 0f;
        public DateTime PhaseStartTime;
        public List<CampaignPhase> CompletedPhases = new List<CampaignPhase>();
        public List<string> ReachedMilestones = new List<string>();
    }

    /// <summary>
    /// Player statistics tracker for detailed progression analytics
    /// </summary>
    [System.Serializable]
    public class PlayerStatTracker
    {
        public int TotalActionsPerformed = 0;
        public float TotalTimeSpent = 0f;
        public int SessionsCompleted = 0;
        public float AverageSessionLength = 0f;
        public Dictionary<string, int> ActivityCounts = new Dictionary<string, int>();
        public Dictionary<string, float> SkillUsageTime = new Dictionary<string, float>();
        public List<string> RecentActivities = new List<string>();
        public DateTime LastActivityTime;
        public float TotalRevenue = 0f;
        public float TotalExpenses = 0f;
        public int PlantsCultivated = 0;
        public int ResearchProjectsCompleted = 0;
        public int FacilitiesConstructed = 0;
        public float HighestQualityAchieved = 0f;
        public Dictionary<string, float> CategoryProgress = new Dictionary<string, float>();
    }

    /// <summary>
    /// Player progression preferences and settings
    /// </summary>
    [System.Serializable]
    public class ProgressionPreferences
    {
        [Header("Notification Settings")]
        public bool ShowLevelUpNotifications = true;
        public bool ShowSkillLevelUpNotifications = true;
        public bool ShowAchievementNotifications = true;
        public bool ShowMilestoneNotifications = true;
        public bool ShowProgressionTips = true;
        
        [Header("Progression Settings")]
        public bool AutoSaveProgression = true;
        public bool EnableProgressionAnalytics = true;
        public bool EnableProgressionEffects = true;
        public float ExperienceMultiplier = 1f;
        public float ProgressionDifficulty = 1f;
        public bool EnableAutomaticSkillAllocation = false;
        public bool ShowDetailedStats = true;
        
        [Header("Display Settings")]
        public bool ShowProgressBars = true;
        public bool ShowExperienceNumbers = true;
        public bool ShowLevelRequirements = true;
        public bool CompactProgressionUI = false;
        public bool EnableProgressionSounds = true;
        public bool EnableProgressionAnimations = true;
    }

    #endregion

    #region Experience System

    /// <summary>
    /// Experience gain record
    /// </summary>
    [System.Serializable]
    public class ExperienceGain
    {
        public string PlayerId;
        public string SourceId;
        public ExperienceSource Source;
        public float BaseAmount;
        public float MultipliedAmount;
        public float FinalAmount;
        public List<ExperienceModifier> Modifiers = new List<ExperienceModifier>();
        public DateTime GainTime;
        public string Context;
        public Dictionary<string, object> GainData = new Dictionary<string, object>();
        public bool IsBonus = false;
        public bool IsShared = false; // For multiplayer/collaboration gains
    }

    /// <summary>
    /// Experience modifier applied to gains
    /// </summary>
    [System.Serializable]
    public class ExperienceModifier
    {
        public string ModifierId;
        public string ModifierName;
        public ExperienceModifierType ModifierType;
        public float Multiplier = 1f;
        public float FlatBonus = 0f;
        public bool IsTemporary = false;
        public DateTime ExpiryTime;
        public string Description;
        public List<string> ApplicableSources = new List<string>();
        public bool IsStackable = true;
        public int MaxStacks = 1;
        public int CurrentStacks = 1;
    }

    /// <summary>
    /// Experience curve definition for level calculations
    /// </summary>
    [System.Serializable]
    public class ExperienceCurve
    {
        public string CurveId;
        public string CurveName;
        public LevelCalculationType CalculationType;
        public AnimationCurve LevelCurve;
        public float BaseExperience = 100f;
        public float ExperienceMultiplier = 1.5f;
        public int MaxLevel = 100;
        public List<LevelMilestone> LevelMilestones = new List<LevelMilestone>();
        public bool HasLevelCap = true;
        public float PrestigeMultiplier = 1f;
    }

    /// <summary>
    /// Level milestone definition
    /// </summary>
    [System.Serializable]
    public class LevelMilestone
    {
        public int Level;
        public float RequiredExperience;
        public List<LevelReward> Rewards = new List<LevelReward>();
        public string MilestoneName;
        public string Description;
        public bool IsPrestigeMilestone = false;
        public bool UnlocksFeatures = false;
        public List<string> UnlockedFeatures = new List<string>();
    }

    /// <summary>
    /// Reward granted at level milestone
    /// </summary>
    [System.Serializable]
    public class LevelReward
    {
        public string RewardId;
        public string RewardName;
        public LevelRewardType RewardType;
        public float RewardValue;
        public string Description;
        public bool IsAutomatic = true;
        public bool RequiresChoice = false;
        public List<string> RewardOptions = new List<string>();
    }

    #endregion

    #region Level Calculations

    /// <summary>
    /// Level calculation utility
    /// </summary>
    [System.Serializable]
    public class LevelCalculator
    {
        public ExperienceCurve Curve;
        public Dictionary<int, float> LevelRequirements = new Dictionary<int, float>();
        public bool IsInitialized = false;
        
        public void Initialize()
        {
            if (Curve == null) return;
            
            LevelRequirements.Clear();
            for (int level = 1; level <= Curve.MaxLevel; level++)
            {
                LevelRequirements[level] = CalculateExperienceForLevel(level);
            }
            IsInitialized = true;
        }
        
        public float CalculateExperienceForLevel(int level)
        {
            if (Curve == null) return 0f;
            
            switch (Curve.CalculationType)
            {
                case LevelCalculationType.Linear:
                    return Curve.BaseExperience * level;
                case LevelCalculationType.Exponential:
                    return Curve.BaseExperience * Mathf.Pow(Curve.ExperienceMultiplier, level - 1);
                case LevelCalculationType.Custom_Curve:
                    return Curve.LevelCurve != null ? Curve.LevelCurve.Evaluate(level) : 0f;
                default:
                    return Curve.BaseExperience * level * Curve.ExperienceMultiplier;
            }
        }
        
        public int CalculateLevelFromExperience(float experience)
        {
            if (!IsInitialized) Initialize();
            
            int level = 1;
            foreach (var requirement in LevelRequirements)
            {
                if (experience >= requirement.Value)
                    level = requirement.Key;
                else
                    break;
            }
            return level;
        }
        
        public float GetProgressToNextLevel(float currentExperience)
        {
            int currentLevel = CalculateLevelFromExperience(currentExperience);
            if (currentLevel >= Curve.MaxLevel) return 1f;
            
            float currentLevelExp = LevelRequirements[currentLevel];
            float nextLevelExp = LevelRequirements[currentLevel + 1];
            
            return (currentExperience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        }
    }

    /// <summary>
    /// Level up event data
    /// </summary>
    [System.Serializable]
    public class LevelUpEvent
    {
        public string PlayerId;
        public string SkillId; // Empty for overall level
        public int OldLevel;
        public int NewLevel;
        public float ExperienceAtLevelUp;
        public DateTime LevelUpTime;
        public List<LevelReward> EarnedRewards = new List<LevelReward>();
        public ExperienceSource TriggerSource;
        public string Context;
        public bool IsPrestigeLevelUp = false;
        public bool WasCelebrated = false;
    }

    #endregion

    #region Progression Events and Notifications

    /// <summary>
    /// Progression event for event-driven system
    /// </summary>
    [System.Serializable]
    public class ProgressionEvent
    {
        public string EventId;
        public ProgressionEventType EventType;
        public string PlayerId;
        public float Amount;
        public DateTime EventTime;
        public string SkillId;
        public string AchievementId;
        public string ResearchId;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        
        // Additional properties for ComprehensiveProgressionManager
        public string ContentId;
        public ExperienceSource Source;
        public string Description;
        
        // Compatibility properties for ComprehensiveProgressionManager
        public ProgressionEventType Type => EventType;
    }

    /// <summary>
    /// Progression notification for UI
    /// </summary>
    [System.Serializable]
    public class ProgressionNotification
    {
        public string NotificationId;
        public ProgressionNotificationType NotificationType;
        public string Title;
        public string Message;
        public Sprite Icon;
        public DateTime StartTime; // For ComprehensiveProgressionManager compatibility
        public float Duration = 3f; // For ComprehensiveProgressionManager compatibility
        public bool IsImportant = false;
        public bool RequiresAcknowledgment = false;
        public Color NotificationColor = Color.white;
        public string SoundEffect;
        public Dictionary<string, object> NotificationData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Progression session tracking
    /// </summary>
    [System.Serializable]
    public class ProgressionSession
    {
        public string SessionId;
        public string PlayerId;
        public DateTime StartTime;
        public DateTime EndTime;
        public float Duration => (float)(EndTime - StartTime).TotalHours;
        public Dictionary<string, float> ExperienceGained = new Dictionary<string, float>();
        public List<string> AchievementsUnlocked = new List<string>();
        public List<string> SkillsLeveledUp = new List<string>();
        public List<string> ContentUnlocked = new List<string>();
        public float TotalExperienceGained = 0f;
        public int ActionsPerformed = 0;
        public Dictionary<string, int> ActivityCounts = new Dictionary<string, int>();
        public string SessionType; // Tutorial, Gameplay, Research, etc.
        public bool IsActive = false;
    }

    #endregion

    #region Progression Analytics

    /// <summary>
    /// Progression metrics for analytics and monitoring
    /// </summary>
    [System.Serializable]
    public class ProgressionMetrics
    {
        public float AveragePlayTime = 0f;
        public float ExperiencePerHour = 0f;
        public float AchievementCompletionRate = 0f;
        public Dictionary<string, float> SkillProgressionRates = new Dictionary<string, float>();
        public Dictionary<string, float> ContentEngagement = new Dictionary<string, float>();
        public float RetentionRate = 0f;
        public DateTime LastMetricsUpdate;
        public int TotalSessions = 0;
        public float TotalPlayTime = 0f;
        public Dictionary<string, float> SourceEfficiency = new Dictionary<string, float>();
        
        // Compatibility properties for ComprehensiveProgressionManager
        public float SessionLength => TotalPlayTime / Mathf.Max(1, TotalSessions);
        public float ExperienceRate => ExperiencePerHour;
        public float EngagementScore => (RetentionRate + AchievementCompletionRate) / 2f;
    }

    /// <summary>
    /// Progression system report
    /// </summary>
    [System.Serializable]
    public class ProgressionSystemReport
    {
        public ProgressionMetrics Metrics;
        public PlayerProgressionData PlayerProgression;
        public List<ProgressionSession> RecentSessions = new List<ProgressionSession>();
        public Dictionary<string, float> CategoryProgress = new Dictionary<string, float>();
        public List<string> RecommendedActions = new List<string>();
        public float OverallProgressionHealth = 0f;
        public DateTime ReportGenerated;
        public string ReportSummary;
    }

    /// <summary>
    /// Progression analytics data
    /// </summary>
    [System.Serializable]
    public class ProgressionAnalytics
    {
        public string PlayerId;
        public Dictionary<string, float> SkillUsageStats = new Dictionary<string, float>();
        public Dictionary<string, int> AchievementAttempts = new Dictionary<string, int>();
        public Dictionary<string, float> ResearchEfficiency = new Dictionary<string, float>();
        public List<ProgressionSession> SessionHistory = new List<ProgressionSession>();
        public DateTime LastUpdate;
        
        /// Update analytics data - called by ComprehensiveProgressionManager
        public void UpdateAnalytics(PlayerProgressionData progressionData)
        {
            if (progressionData == null) return;
            
            PlayerId = progressionData.PlayerId;
            
            // Update skill usage statistics
            foreach (var skill in progressionData.SkillLevels)
            {
                if (!SkillUsageStats.ContainsKey(skill.Key))
                    SkillUsageStats[skill.Key] = 0f;
                SkillUsageStats[skill.Key] = skill.Value * 10f; // Rough usage approximation
            }
            
            LastUpdate = DateTime.Now;
        }
        
        /// Get efficiency rating for a specific activity
        public float GetEfficiencyRating(string activityType)
        {
            return ResearchEfficiency.GetValueOrDefault(activityType, 1f);
        }
        
        /// Cleanup analytics data - called by ComprehensiveProgressionManager on shutdown
        public void CleanupAnalytics()
        {
            // Remove old session data (keep last 30 days)
            var cutoffDate = DateTime.Now.AddDays(-30);
            SessionHistory.RemoveAll(s => s.EndTime < cutoffDate);
        }
        
        /// Record a progression event for analytics
        public void RecordEvent(ProgressionEvent progressionEvent)
        {
            if (progressionEvent == null) return;
            
            // Process different types of events
            switch (progressionEvent.EventType)
            {
                case ProgressionEventType.Experience_Gained:
                    if (!string.IsNullOrEmpty(progressionEvent.SkillId))
                    {
                        if (!SkillUsageStats.ContainsKey(progressionEvent.SkillId))
                            SkillUsageStats[progressionEvent.SkillId] = 0f;
                        SkillUsageStats[progressionEvent.SkillId] += progressionEvent.Amount;
                    }
                    break;
                    
                case ProgressionEventType.Achievement_Unlocked:
                    if (!string.IsNullOrEmpty(progressionEvent.AchievementId))
                    {
                        if (!AchievementAttempts.ContainsKey(progressionEvent.AchievementId))
                            AchievementAttempts[progressionEvent.AchievementId] = 0;
                        AchievementAttempts[progressionEvent.AchievementId]++;
                    }
                    break;
                    
                case ProgressionEventType.Research_Completed:
                    if (!string.IsNullOrEmpty(progressionEvent.ResearchId))
                    {
                        if (!ResearchEfficiency.ContainsKey(progressionEvent.ResearchId))
                            ResearchEfficiency[progressionEvent.ResearchId] = 1f;
                        // Update efficiency based on completion time, etc.
                    }
                    break;
            }
        }
    }

    #endregion

    #region Campaign System

    /// <summary>
    /// Campaign phase enumeration
    /// </summary>
    public enum CampaignPhase
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Endgame,
        Expansion,
        Custom
    }

    #endregion

    #region Supporting Enums

    /// <summary>
    /// Level reward types
    /// </summary>
    public enum LevelRewardType
    {
        Skill_Points,
        Experience_Bonus,
        Currency,
        Equipment_Unlock,
        Feature_Unlock,
        Research_Unlock,
        Achievement_Unlock,
        Customization_Option,
        Permanent_Bonus,
        Temporary_Boost
    }

    #endregion
}