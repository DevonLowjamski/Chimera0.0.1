using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data
{
    /// <summary>
    /// Time Scale Library - Library of predefined time scales and temporal configurations
    /// Defines time scale presets, their effects, and associated gameplay mechanics
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Time Scale Library", menuName = "Project Chimera/Cultivation/Time Scale Library")]
    public class TimeScaleLibrarySO : ChimeraDataSO
    {
        [Header("Time Scale Presets")]
        public List<TimeScalePreset> TimeScalePresets = new List<TimeScalePreset>();
        
        [Header("Dynamic Scaling")]
        public List<DynamicScaleRule> DynamicScaleRules = new List<DynamicScaleRule>();
        
        [Header("Conditional Scaling")]
        public List<ConditionalTimeScale> ConditionalScales = new List<ConditionalTimeScale>();
        
        public TimeScalePreset GetPreset(TimeScaleCategory category, int level)
        {
            return TimeScalePresets.Find(p => p.Category == category && p.UnlockLevel <= level);
        }
        
        public List<TimeScalePreset> GetAvailablePresets(int playerLevel)
        {
            return TimeScalePresets.FindAll(p => p.UnlockLevel <= playerLevel);
        }
        
        public float GetDynamicTimeScale(GameState gameState, PlayerState playerState)
        {
            foreach (var rule in DynamicScaleRules)
            {
                if (EvaluateRule(rule, gameState, playerState))
                {
                    return rule.TimeScale;
                }
            }
            return 1f; // Default time scale
        }
        
        private bool EvaluateRule(DynamicScaleRule rule, GameState gameState, PlayerState playerState)
        {
            // Simplified rule evaluation - in real implementation would be more complex
            return true; // Placeholder
        }
    }
    
    [System.Serializable]
    public class TimeScalePreset
    {
        public string PresetName;
        public TimeScaleCategory Category;
        [Range(0.1f, 100f)] public float TimeScale = 1f;
        [Range(1, 100)] public int UnlockLevel = 1;
        
        [Header("Effects")]
        [Range(0.1f, 2f)] public float QualityMultiplier = 1f;
        [Range(0.1f, 2f)] public float EfficiencyMultiplier = 1f;
        [Range(0.1f, 5f)] public float CostMultiplier = 1f;
        
        [Header("Visual Settings")]
        public Color TimeEffectColor = Color.white;
        [Range(0f, 1f)] public float EffectIntensity = 0.5f;
        public bool EnableVisualEffects = true;
        
        [Header("Restrictions")]
        public bool RestrictedToExpertMode = false;
        public List<string> RequiredAchievements = new List<string>();
        public List<GameMode> AllowedGameModes = new List<GameMode>();
        
        public string Description;
        public Sprite PresetIcon;
    }
    
    [System.Serializable]
    public class DynamicScaleRule
    {
        public string RuleName;
        public DynamicScaleCondition Condition;
        [Range(0.1f, 100f)] public float TimeScale = 1f;
        [Range(1, 100)] public int Priority = 1;
        public bool IsActive = true;
        public string Description;
    }
    
    [System.Serializable]
    public class ConditionalTimeScale
    {
        public string ConditionName;
        public List<TimeScaleCondition> Conditions = new List<TimeScaleCondition>();
        [Range(0.1f, 100f)] public float ResultingTimeScale = 1f;
        [Range(0f, 10f)] public float ConditionDuration = 0f;
        public bool IsPermanent = false;
    }
    
    [System.Serializable]
    public class TimeScaleCondition
    {
        public ConditionType ConditionType;
        public string ConditionParameter;
        public float ConditionValue;
        public ComparisonOperator Operator;
    }
    
    [System.Serializable]
    public class GameState
    {
        public int ActivePlants;
        public float AverageGrowthProgress;
        public bool IsInCriticalPeriod;
        public GameMode CurrentGameMode;
    }
    
    [System.Serializable]
    public class PlayerState
    {
        public int Level;
        public float Experience;
        public List<string> UnlockedAchievements;
        public bool IsExpertMode;
    }
    
    public enum TimeScaleCategory
    {
        Beginner,
        Normal,
        Advanced,
        Expert,
        Creative,
        Challenge,
        Emergency,
        Precision
    }
    
    public enum DynamicScaleCondition
    {
        PlayerLevel,
        PlantCount,
        CriticalEvent,
        PerformanceMode,
        ExpertMode,
        TimeOfDay,
        SeasonalEvent,
        EmergencyMode
    }
    
    public enum ConditionType
    {
        PlayerLevel,
        PlantHealth,
        ResourceAvailability,
        TimeRemaining,
        QualityScore,
        EfficiencyRating,
        CompletionPercentage,
        ErrorCount
    }
    
    public enum ComparisonOperator
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NotEqual
    }
    
    public enum GameMode
    {
        Tutorial,
        Normal,
        Advanced,
        Expert,
        Creative,
        Challenge,
        Competitive,
        Collaborative
    }
}