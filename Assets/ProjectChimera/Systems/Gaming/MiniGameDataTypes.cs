using System;
using System.Collections.Generic;
using UnityEngine;

// Types MiniGameResult and MiniGameSession are defined in MiniGameInterfaces.cs

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Placeholder for EnvironmentalCrisis - will be replaced with proper implementation
    /// </summary>
    [Serializable]
    public class EnvironmentalCrisis
    {
        public string CrisisType;
        public float Severity;
        public string Description;
        public float Duration;
        public bool IsActive;
    }
    
    /// <summary>
    /// Placeholder for MarketData - will be replaced with proper implementation
    /// </summary>
    [Serializable]
    public class MarketData
    {
        public string MarketName;
        public float CurrentPrice;
        public float PriceChangePercent;
        public string Trend;
        public float Volume;
    }
    
    /// <summary>
    /// Placeholder for EquipmentData - will be replaced with proper implementation
    /// </summary>
    [Serializable]
    public class EquipmentData
    {
        public string EquipmentName;
        public string EquipmentType;
        public float Efficiency;
        public bool IsOperational;
        public float MaintenanceLevel;
    }
    
    /// <summary>
    /// Placeholder for PlantInstance - using proper reference
    /// This should eventually reference ProjectChimera.Systems.Cultivation.PlantInstance
    /// </summary>
    [Serializable]
    public class PlantInstance
    {
        public string PlantId;
        public string SpeciesName;
        public float Health;
        public string GrowthStage;
        public float WaterLevel;
        public DateTime PlantedDate;
        
        // Property to access health data for mini-game integration
        public PlantHealthData HealthData => this.GetHealthData();
    }
    
    /// <summary>
    /// Game-specific statistics for individual mini-games
    /// </summary>
    [Serializable]
    public class GameSpecificStats
    {
        public string GameId;
        public int TimesPlayed;
        public float BestScore;
        public float AverageScore;
        public float TotalTimePlayed;
        public int HighestStreak;
        public DateTime LastPlayed;
        public DifficultyLevel HighestDifficultyCompleted;
        public List<string> AchievementsUnlocked = new List<string>();
    }
    
    /// <summary>
    /// Difficulty levels for mini-games
    /// </summary>
    public enum DifficultyLevel
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Hard,
        Expert,
        Master,
        Adaptive
    }
    

} 