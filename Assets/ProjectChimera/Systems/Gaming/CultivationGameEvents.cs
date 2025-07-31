using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    // ============================================================================
    // PLANT LIFECYCLE EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when a plant progresses through growth stages
    /// </summary>
    [Serializable]
    public class PlantGrowthEvent : GameEvent
    {
        /// <summary>
        /// The plant instance that experienced growth
        /// </summary>
        public string PlantId { get; set; }
        
        /// <summary>
        /// Previous growth stage
        /// </summary>
        public string PreviousStage { get; set; }
        
        /// <summary>
        /// New growth stage reached
        /// </summary>
        public string NewStage { get; set; }
        
        /// <summary>
        /// Growth progress percentage (0.0 to 1.0)
        /// </summary>
        public float GrowthProgress { get; set; }
        
        /// <summary>
        /// Whether this growth milestone should trigger celebrations
        /// </summary>
        public bool ShouldCelebrate { get; set; }
        
        /// <summary>
        /// Time it took to reach this stage
        /// </summary>
        public TimeSpan GrowthDuration { get; set; }
        
        /// <summary>
        /// Health status when growth occurred
        /// </summary>
        public float PlantHealth { get; set; }
        
        /// <summary>
        /// Environmental conditions during growth
        /// </summary>
        public Dictionary<string, float> EnvironmentalConditions { get; set; } = new Dictionary<string, float>();
        
        public PlantGrowthEvent()
        {
            Priority = EventPriority.High; // Growth is important for player engagement
            TriggerUINotification = true;
            SourceSystem = "PlantLifecycle";
        }
    }
    
    /// <summary>
    /// Fired when a plant is ready for harvest
    /// </summary>
    [Serializable]
    public class PlantHarvestReadyEvent : GameEvent
    {
        public string PlantId { get; set; }
        public string PlantStrain { get; set; }
        public float ExpectedYield { get; set; }
        public float QualityRating { get; set; }
        public int DaysToHarvest { get; set; }
        public bool IsOptimalHarvestTime { get; set; }
        
        public PlantHarvestReadyEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "HarvestManager";
        }
    }
    
    /// <summary>
    /// Fired when a plant is successfully harvested
    /// </summary>
    [Serializable]
    public class PlantHarvestedEvent : GameEvent
    {
        public string PlantId { get; set; }
        public string PlantStrain { get; set; }
        public float ActualYield { get; set; }
        public float QualityGrade { get; set; }
        public bool IsRecordBreaking { get; set; }
        public Dictionary<string, float> HarvestMetrics { get; set; } = new Dictionary<string, float>();
        public string[] UnlockedAchievements { get; set; } = new string[0];
        public float ExperienceGained { get; set; }
        
        public PlantHarvestedEvent()
        {
            Priority = EventPriority.Immediate; // Harvest completion needs immediate feedback
            TriggerUINotification = true;
            SourceSystem = "HarvestManager";
        }
    }
    
    /// <summary>
    /// Fired when a plant experiences health changes
    /// </summary>
    [Serializable]
    public class PlantHealthEvent : GameEvent
    {
        public string PlantId { get; set; }
        public float PreviousHealth { get; set; }
        public float CurrentHealth { get; set; }
        public string HealthChangeReason { get; set; }
        public string[] AffectedSystems { get; set; } = new string[0];
        public bool RequiresPlayerAttention { get; set; }
        public string RecommendedAction { get; set; }
        
        public PlantHealthEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "PlantHealth";
        }
    }
    
    // ============================================================================
    // ENVIRONMENTAL EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when environmental conditions change significantly
    /// </summary>
    [Serializable]
    public class EnvironmentalChangeEvent : GameEvent
    {
        public string EnvironmentalZone { get; set; }
        public string ParameterChanged { get; set; }
        public float PreviousValue { get; set; }
        public float NewValue { get; set; }
        public float OptimalValue { get; set; }
        public bool IsWithinOptimalRange { get; set; }
        public string[] AffectedPlants { get; set; } = new string[0];
        public string ChangeSource { get; set; } // "Player", "System", "Equipment"
        
        public EnvironmentalChangeEvent()
        {
            Priority = EventPriority.Standard;
            SourceSystem = "EnvironmentalManager";
        }
    }
    
    /// <summary>
    /// Fired when plants experience environmental stress
    /// </summary>
    [Serializable]
    public class EnvironmentalStressEvent : GameEvent
    {
        public string StressType { get; set; } // "Temperature", "Humidity", "Light", "Nutrients"
        public float SeverityLevel { get; set; } // 0.0 to 1.0
        public string[] AffectedPlants { get; set; } = new string[0];
        public bool RequiresPlayerAction { get; set; }
        public string RecommendedSolution { get; set; }
        public TimeSpan EstimatedRecoveryTime { get; set; }
        
        public EnvironmentalStressEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "EnvironmentalManager";
        }
    }
    
    /// <summary>
    /// Fired when environmental conditions reach optimal levels
    /// </summary>
    [Serializable]
    public class OptimalEnvironmentAchievedEvent : GameEvent
    {
        public string EnvironmentalZone { get; set; }
        public Dictionary<string, float> OptimalParameters { get; set; } = new Dictionary<string, float>();
        public string[] BenefitingPlants { get; set; } = new string[0];
        public float ExpectedGrowthBonus { get; set; }
        public bool ShouldCelebrate { get; set; }
        
        public OptimalEnvironmentAchievedEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            ShouldCelebrate = true;
            SourceSystem = "EnvironmentalManager";
        }
    }
    
    // ============================================================================
    // PLAYER ACTION EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when player interacts with a plant
    /// </summary>
    [Serializable]
    public class PlantInteractionEvent : PlayerActionEvent
    {
        public string PlantId { get; set; }
        public string InteractionType { get; set; } // "Water", "Prune", "Inspect", "Harvest"
        public Dictionary<string, object> InteractionParameters { get; set; } = new Dictionary<string, object>();
        public bool WasSuccessful { get; set; }
        public string FeedbackMessage { get; set; }
        
        public PlantInteractionEvent()
        {
            ActionType = "PlantInteraction";
            SourceSystem = "PlayerInput";
        }
    }
    
    /// <summary>
    /// Fired when player adjusts environmental controls
    /// </summary>
    [Serializable]
    public class EnvironmentalControlEvent : PlayerActionEvent
    {
        public string ControlType { get; set; } // "Temperature", "Humidity", "Lighting", "Ventilation"
        public float PreviousValue { get; set; }
        public float NewValue { get; set; }
        public string EnvironmentalZone { get; set; }
        public bool IsAutomated { get; set; }
        
        public EnvironmentalControlEvent()
        {
            ActionType = "EnvironmentalControl";
            SourceSystem = "EnvironmentalControls";
        }
    }
    
    /// <summary>
    /// Fired when player purchases or uses items
    /// </summary>
    [Serializable]
    public class ItemTransactionEvent : PlayerActionEvent
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string TransactionType { get; set; } // "Purchase", "Sell", "Use", "Craft"
        public int Quantity { get; set; }
        public float Cost { get; set; }
        public string Currency { get; set; }
        public Dictionary<string, object> ItemProperties { get; set; } = new Dictionary<string, object>();
        
        public ItemTransactionEvent()
        {
            ActionType = "ItemTransaction";
            SourceSystem = "Economy";
        }
    }
    
    // ============================================================================
    // ACHIEVEMENT & PROGRESSION EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when player unlocks an achievement
    /// </summary>
    [Serializable]
    public class AchievementUnlockedEvent : GameEvent
    {
        public string AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string AchievementDescription { get; set; }
        public string AchievementCategory { get; set; }
        public int Points { get; set; }
        public string UnlockReason { get; set; }
        public bool IsRare { get; set; }
        public bool IsHidden { get; set; }
        public Dictionary<string, object> UnlockData { get; set; } = new Dictionary<string, object>();
        
        public AchievementUnlockedEvent()
        {
            Priority = EventPriority.Immediate;
            TriggerUINotification = true;
            SourceSystem = "AchievementSystem";
        }
    }
    
    /// <summary>
    /// Fired when player levels up or gains experience
    /// </summary>
    [Serializable]
    public class ExperienceGainedEvent : GameEvent
    {
        public string SkillCategory { get; set; }
        public float ExperienceAmount { get; set; }
        public float PreviousLevel { get; set; }
        public float NewLevel { get; set; }
        public bool LeveledUp { get; set; }
        public string ExperienceSource { get; set; }
        public string[] UnlockedAbilities { get; set; } = new string[0];
        
        public ExperienceGainedEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "ProgressionSystem";
        }
    }
    
    /// <summary>
    /// Fired when player completes a milestone
    /// </summary>
    [Serializable]
    public class MilestoneCompletedEvent : GameEvent
    {
        public string MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public string MilestoneCategory { get; set; }
        public Dictionary<string, object> CompletionData { get; set; } = new Dictionary<string, object>();
        public string[] Rewards { get; set; } = new string[0];
        public bool UnlocksNewContent { get; set; }
        
        public MilestoneCompletedEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "ProgressionSystem";
        }
    }
    
    // ============================================================================
    // GENETICS & BREEDING EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when a breeding operation completes
    /// </summary>
    [Serializable]
    public class BreedingCompletedEvent : GameEvent
    {
        public string ParentAId { get; set; }
        public string ParentBId { get; set; }
        public string OffspringId { get; set; }
        public string[] InheritedTraits { get; set; } = new string[0];
        public string[] NewTraits { get; set; } = new string[0];
        public bool IsUniqueCombination { get; set; }
        public float GeneticDiversity { get; set; }
        public Dictionary<string, object> GeneticData { get; set; } = new Dictionary<string, object>();
        
        public BreedingCompletedEvent()
        {
            Priority = EventPriority.High;
            TriggerUINotification = true;
            SourceSystem = "GeneticsManager";
        }
    }
    
    /// <summary>
    /// Fired when a rare genetic mutation occurs
    /// </summary>
    [Serializable]
    public class GeneticMutationEvent : GameEvent
    {
        public string PlantId { get; set; }
        public string MutationType { get; set; }
        public string MutationName { get; set; }
        public string[] AffectedTraits { get; set; } = new string[0];
        public bool IsBeneficial { get; set; }
        public float RarityScore { get; set; }
        public bool CreatesNewStrain { get; set; }
        
        public GeneticMutationEvent()
        {
            Priority = EventPriority.Immediate;
            TriggerUINotification = true;
            SourceSystem = "GeneticsManager";
        }
    }
    
    // ============================================================================
    // SYSTEM EVENTS
    // ============================================================================
    
    /// <summary>
    /// Fired when the system detects performance issues
    /// </summary>
    [Serializable]
    public class PerformanceAlertEvent : SystemEvent
    {
        public string PerformanceMetric { get; set; }
        public float CurrentValue { get; set; }
        public float ThresholdValue { get; set; }
        public string RecommendedAction { get; set; }
        public bool RequiresImmediateAttention { get; set; }
        
        public PerformanceAlertEvent()
        {
            Priority = EventPriority.High;
            SystemComponent = "PerformanceMonitor";
            Severity = SystemEventSeverity.Warning;
        }
    }
    
    /// <summary>
    /// Fired for save/load operations
    /// </summary>
    [Serializable]
    public class SaveGameEvent : SystemEvent
    {
        public string SaveOperation { get; set; } // "Save", "Load", "AutoSave"
        public bool WasSuccessful { get; set; }
        public string SaveFileName { get; set; }
        public long SaveFileSize { get; set; }
        public TimeSpan OperationDuration { get; set; }
        public string ErrorMessage { get; set; }
        
        public SaveGameEvent()
        {
            Priority = EventPriority.Standard;
            SystemComponent = "SaveManager";
        }
    }
    
    /// <summary>
    /// Fired when tutorial steps are completed
    /// </summary>
    [Serializable]
    public class TutorialStepEvent : GameEvent
    {
        public string TutorialId { get; set; }
        public string StepId { get; set; }
        public string StepName { get; set; }
        public bool Completed { get; set; }
        public bool Skipped { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public int AttemptsRequired { get; set; }
        
        public TutorialStepEvent()
        {
            Priority = EventPriority.Standard;
            SourceSystem = "TutorialManager";
        }
    }
}