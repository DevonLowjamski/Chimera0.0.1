using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Data structures for cultivation gaming systems
    /// </summary>
    
    [System.Serializable]
    public class TimeScaleData
    {
        public GameTimeScale Scale;
        public float Multiplier;
        public string DisplayName;
        public bool IsUnlocked = true;
        public int RequiredLevel = 1;
        public float ManualTaskDifficulty = 1.0f;
        public float AutomationAdvantage = 1.0f;
        public float RealTimeDayDuration;
        public float GameDaysPerRealHour;
        public string Description;
        public bool PlayerEngagementOptimal;
    }
    
    [System.Serializable]
    public class TimeAccelerationResult
    {
        public bool Success;
        public GameTimeScale PreviousScale;
        public GameTimeScale NewScale;
        public float TransitionDuration;
        public string Message;
        public List<string> Warnings = new List<string>();
        public float ExperienceGained;
    }
    
    [System.Serializable]
    public class TimeTransition
    {
        public GameTimeScale FromScale;
        public GameTimeScale ToScale;
        public float Duration;
        public float Progress;
        public TimeTransitionState State;
        public DateTime StartTime;
        public AnimationCurve TransitionCurve;
    }
    
    // TimeTransitionConfigSO is defined in TimeTransitionConfigSO.cs as a ScriptableObject
    
    [System.Serializable]
    public class TransitionRule
    {
        public GameTimeScale FromScale;
        public GameTimeScale ToScale;
        public bool IsAllowed = true;
        public float CustomDuration = -1f; // -1 means use default
        public string RestrictionReason;
    }
    
    // Note: TreeProgressionMetrics is defined in SkillTreeDataStructures.cs
    
    [System.Serializable]
    public class PlantCareQuality
    {
        public float OverallScore;
        public float TimingAccuracy;
        public float ActionPrecision;
        public float ConsistencyBonus;
        public int PerfectActionsCount;
        public int TotalActionsCount;
        
        public float GetQualityRating()
        {
            return Mathf.Clamp01(OverallScore);
        }
    }
    
    [System.Serializable]
    public class AutomationProgressData
    {
        public CultivationTaskType TaskType;
        public float CurrentBurden = 0f;
        public float BurdenThreshold = 20f;
        public AutomationDesireLevel AutomationDesireLevel = AutomationDesireLevel.None;
        public bool IsAutomationAvailable = false;
        public bool IsAutomationUnlocked = false;
        public float TimeSinceLastBurdenIncrease = 0f;
        public Dictionary<string, object> ProgressData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class PlantInteractionData
    {
        public string InteractionId;
        public string PlantId;
        public InteractionCategory Category;
        public DateTime Timestamp;
        public float QualityScore;
        public bool WasAutomated;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class CareActionResult
    {
        public bool Success;
        public float QualityScore;
        public float ExperienceGained;
        public float SkillProgressGained;
        public List<string> UnlockedActions = new List<string>();
        public List<string> Messages = new List<string>();
        public PlantResponseData PlantResponse;
    }
    
    [System.Serializable]
    public class PlantResponseData
    {
        public float HealthChange;
        public float StressChange;
        public float GrowthSpeedChange;
        public bool TriggeredGrowthStage;
        public List<string> VisualEffects = new List<string>();
        public List<string> AudioCues = new List<string>();
    }
    
    [System.Serializable]
    public class SkillUnlockData
    {
        public string SkillId;
        public string SkillName;
        public string Description;
        public int RequiredLevel;
        public List<string> Prerequisites = new List<string>();
        public float UnlockProgress;
        public bool IsUnlocked;
    }
    
    #region Unique Gaming Data Structures (Non-Duplicates Only)
    
    [System.Serializable]
    public class AutomationSystemState
    {
        public AutomationSystemType SystemType;
        public bool IsUnlocked = false;
        public bool IsActive = false;
        public float Efficiency = 1f;
        public float Reliability = 1f;
        public float MaintenanceLevel = 1f;
        public float OperationalCost = 0f;
        public Dictionary<string, object> SystemData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class TaskBurdenState
    {
        public CultivationTaskType TaskType;
        public float CurrentBurden = 0f;
        public float BurdenAccumulationRate = 1f;
        public float BurdenDecayRate = 0.1f;
        public float CognitiveLoad = 0f;
        public float PhysicalDemand = 0f;
        public float TimeInvestment = 0f;
        public float ConsistencyRequirement = 0f;
        public Dictionary<string, object> BurdenFactors = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class PlantCareState
    {
        public int PlantInstanceId;
        public Dictionary<CultivationTaskType, float> CareQualityScores = new Dictionary<CultivationTaskType, float>();
        public Dictionary<CultivationTaskType, float> LastCareTime = new Dictionary<CultivationTaskType, float>();
        public float OverallCareQuality = 0f;
        public bool NeedsCare = false;
        public List<CultivationTaskType> RequiredCareTypes = new List<CultivationTaskType>();
        public Dictionary<string, object> CareHistory = new Dictionary<string, object>();
    }
    
    #endregion
    
    #region Unique Enums (Non-Duplicates Only)
    
    public enum AutomationDesireLevel
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }
    
    // Note: AutomationSystemType is defined in AutomationUnlockLibrarySO.cs
    
    public enum AutomationDesireThreshold
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }
    
    #endregion
    
    #region Additional Event Data Types - Canonical definitions are in ProjectChimera.Events.CultivationGamingEventData to avoid CS0101 duplicate definition errors:
    // - PlantCareEventData
    // - TimeScaleEventData  
    // - PlayerChoiceEventData
    
    // Note: PlayerChoice is defined in PlayerChoiceManagerSO.cs
    
    [System.Serializable]
    public class ChoiceConsequences
    {
        public static ChoiceConsequences None = new ChoiceConsequences { HasConsequences = false };
        
        public bool HasConsequences = true;
        public Dictionary<string, float> StatChanges = new Dictionary<string, float>();
        public List<string> UnlockedFeatures = new List<string>();
        public List<string> Messages = new List<string>();
        public float ExperienceGained = 0f;
    }
    
    [System.Serializable]
    public class CreativeSolution
    {
        public string SolutionId;
        public string Description;
        public CultivationTaskType TaskType;
        public float InnovationScore;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class CreativeSolutionResult
    {
        public bool Success;
        public float EffectivenessScore;
        public float InnovationBonus;
        public List<string> UnlockedTechniques = new List<string>();
        public string ResultMessage;
    }
    
    #endregion
    
    [System.Serializable]
    public class AutomationUnlock
    {
        public CultivationTaskType TaskType;
        public AutomationSystemUnlock IrrigationAutomation;
        public AutomationSystemUnlock EnvironmentalAutomation;
        public AutomationSystemUnlock NutrientAutomation;
        public AutomationSystemUnlock MonitoringAutomation;
        public AutomationSystemUnlock LightingAutomation;
        public float UnlockTimestamp;
        public string UnlockId;
    }
    
    [System.Serializable]
    public class AutomationSystemUnlock
    {
        public bool IsUnlocked;
        public AutomationLevel Level;
        public string SystemId;
        public float EfficiencyBonus;
    }
    
    // Note: Enums are defined in their respective original files:
    // - InteractionCategory: PlantInteractionConfigSO.cs
    // - CareActionType: PlantInteractionConfigSO.cs  
    // - TimeTransitionState: TimeAccelerationGamingConfigSO.cs
    // - GameTimeScale: TimeAccelerationGamingConfigSO.cs
}