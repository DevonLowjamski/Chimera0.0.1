using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement data classes for Project Chimera progression system
    /// Moved from nested classes to standalone for better accessibility
    /// </summary>
    
    [System.Serializable]
    public class Achievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public AchievementCategory Category;
        public AchievementRarity Rarity;
        public float Points;
        public string TriggerEvent;
        public float TargetValue;
        public float CurrentProgress;
        public bool IsUnlocked;
        public bool IsSecret;
        public DateTime UnlockDate;
        public string Icon;
        public string CelebrationStyle;
    }
    
    [System.Serializable]
    public class AchievementProgress
    {
        public string AchievementID;
        public string PlayerID;
        public float CurrentValue;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class PlayerAchievementProfile
    {
        public string PlayerID;
        public int TotalAchievements;
        public float TotalPoints;
        public string CurrentTier;
        public AchievementCategory FavoriteCategory;
        public DateTime LastUnlock;
        public List<string> UnlockHistory = new List<string>();
        public List<string> EarnedBadges = new List<string>();
        public Dictionary<AchievementCategory, int> CategoryProgress = new Dictionary<AchievementCategory, int>();
        
        // PC-012-11: Social and Community Achievement Tracking
        public List<string> CommunityInteractions = new List<string>();
        public int ConsecutiveCommunityDays = 0;
        public DateTime LastCommunityContribution = DateTime.MinValue;
        
        // Additional properties for PlayerRecognitionService compatibility
        public DateTime ProfileCreatedDate = DateTime.Now;
        public DateTime LastUpdateDate = DateTime.Now;
        public int Prestige = 0;
        public int SocialRecognition = 0;
        public int CommunityContributions = 0;
        public float CompletionPercentage = 0f;
    }
    
    [System.Serializable]
    public class AchievementBadge
    {
        public string BadgeID;
        public string BadgeName;
        public string Description;
        public BadgeType BadgeType;
        public int RequiredAchievements;
        public bool IsEarned;
        public DateTime EarnDate;
        public int Prestige;
        public string Color;
        
        // Additional properties for PlayerRecognitionService compatibility
        public string Icon = "";
        public AchievementRarity Rarity = AchievementRarity.Common;
    }
    
    [System.Serializable]
    public class AchievementTier
    {
        public string TierID;
        public string TierName;
        public float RequiredPoints;
        public string TierIcon;
        public string Description;
        public List<string> Benefits = new List<string>();
        public int Prestige;
        
        // Additional properties for PlayerRecognitionService compatibility
        public int Level = 1;
        public DateTime UnlockDate = DateTime.MinValue;
        public bool IsUnlocked = false;
    }
    
    [System.Serializable]
    public class AchievementStats
    {
        public int TotalAchievements;
        public int UnlockedAchievements;
        public float TotalPoints;
        public float CompletionPercentage;
        public int RecentUnlocks;
        public int AvailableBadges;
        public int EarnedBadges;
        public DateTime LastUpdate;
    }
    
    public enum BadgeType
    {
        Milestone,
        Category,
        Special,
        Social,
        Ultimate
    }
}