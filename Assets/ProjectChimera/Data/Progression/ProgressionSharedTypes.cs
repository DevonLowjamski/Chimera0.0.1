using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Shared types for progression system - single source of truth for all progression data structures
    /// This eliminates circular dependencies and namespace conflicts
    /// </summary>
    
    #region Core Progression Types
    
    [Serializable]
    public class CleanProgressionAchievement
    {
        public string AchievementID = "";
        public string AchievementName = "";
        public string Description = "";
        public bool IsUnlocked = false;
        public bool IsHidden = false;
        public DateTime UnlockedDate = DateTime.Now;
        public float Progress = 0f;
        public float RequiredProgress = 100f;
        public string Category = "";
        public int Points = 0;
    }
    
    [Serializable]
    public class CleanProgressionExperience
    {
        public string SourceID = "";
        public string SourceType = "";
        public float ExperienceGained = 0f;
        public float TotalExperience = 0f;
        public int CurrentLevel = 1;
        public float ProgressToNextLevel = 0f;
        public DateTime EarnedDate = DateTime.Now;
        public string Category = "";
    }
    
    [Serializable]
    public class CleanProgressionMilestone
    {
        public string MilestoneID = "";
        public string MilestoneName = "";
        public string Description = "";
        public bool IsCompleted = false;
        public DateTime CompletedDate = DateTime.Now;
        public List<string> Requirements = new List<string>();
        public List<string> Rewards = new List<string>();
        public int Order = 0;
    }
    
    [Serializable]
    public class CleanProgressionSkillNode
    {
        public string NodeID = "";
        public string NodeName = "";
        public string Description = "";
        public bool IsUnlocked = false;
        public bool IsMaxed = false;
        public int CurrentLevel = 0;
        public int MaxLevel = 5;
        public float ProgressToNextLevel = 0f;
        public List<string> Prerequisites = new List<string>();
        public string SkillTree = "";
    }
    
    [Serializable]
    public class CleanProgressionLeaderboard
    {
        public string LeaderboardID = "";
        public string LeaderboardName = "";
        public string Category = "";
        public List<CleanProgressionLeaderboardEntry> Entries = new List<CleanProgressionLeaderboardEntry>();
        public DateTime LastUpdated = DateTime.Now;
        public bool IsActive = true;
    }
    
    [Serializable]
    public class CleanProgressionLeaderboardEntry
    {
        public string PlayerID = "";
        public string PlayerName = "";
        public float Score = 0f;
        public int Rank = 1;
        public DateTime AchievedDate = DateTime.Now;
        public string Details = "";
    }
    
    [Serializable]
    public class CleanProgressionReward
    {
        public string RewardID = "";
        public string RewardName = "";
        public string RewardType = "";
        public string Description = "";
        public bool IsClaimed = false;
        public DateTime ClaimedDate = DateTime.Now;
        public Dictionary<string, object> RewardData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CleanProgressionCampaign
    {
        public string CampaignID = "";
        public string CampaignName = "";
        public string Description = "";
        public bool IsActive = true;
        public bool IsCompleted = false;
        public float Progress = 0f;
        public List<string> CompletedObjectives = new List<string>();
        public List<string> AvailableObjectives = new List<string>();
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
    }
    
    #endregion
    
    #region Simple Enums
    
    public enum ProgressionDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum ProgressionCategory
    {
        Genetics,
        Cultivation,
        IPM,
        Business,
        Research,
        Social,
        Creative,
        General
    }
    
    public enum CleanProgressionRewardType
    {
        Experience,
        Currency,
        Item,
        Unlock,
        Title,
        Badge,
        Access,
        Boost
    }
    
    public enum ProgressionSessionType
    {
        Training,
        Competition,
        Research,
        Campaign,
        FreePlay,
        Tutorial,
        Challenge,
        Social
    }
    
    #endregion
}