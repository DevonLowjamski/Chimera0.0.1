using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Achievements
{
    /// <summary>
    /// Basic Achievement data structure for service interfaces
    /// </summary>
    [System.Serializable]
    public class Achievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Name; // Alias for AchievementName
        public string Description;
        public AchievementCategory Category;
        public AchievementRarity Rarity;
        public AchievementType Type;
        public AchievementDifficulty Difficulty;
        public float Points;
        public float PointValue; // Alias for Points
        public string TriggerEvent;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public List<string> Requirements;
    }

}