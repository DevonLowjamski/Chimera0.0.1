using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Data structures for the Random Event System in Project Chimera.
    /// Defines event templates, active events, choices, consequences, and display data
    /// for dynamic gameplay events that enhance entertainment value.
    /// </summary>
    
    /// <summary>
    /// Template for defining reusable random events
    /// </summary>
    [System.Serializable]
    public class RandomEventTemplate
    {
        [Header("Event Identity")]
        public string EventId;
        public string Title;
        [TextArea(3, 6)]
        public string Description;
        
        [Header("Event Classification")]
        public RandomEventCategory Category;
        public EventSeverity Severity;
        public float Probability = 0.1f;
        
        [Header("Timing")]
        public bool HasTimeLimit = false;
        public TimeSpan TimeLimit = TimeSpan.FromHours(6);
        
        [Header("Player Choices")]
        public List<EventChoice> Choices = new List<EventChoice>();
        
        [Header("Requirements")]
        public int MinPlayerLevel = 1;
        public int MaxPlayerLevel = 50;
        public List<string> RequiredFeatures = new List<string>();
        public List<string> RequiredCompletedEvents = new List<string>();
    }
    
    /// <summary>
    /// An active random event that the player must respond to
    /// </summary>
    [System.Serializable]
    public class ActiveRandomEvent
    {
        [Header("Event Data")]
        public string EventId;
        public string Title;
        public string Description;
        public RandomEventCategory Category;
        public EventSeverity Severity;
        
        [Header("Timing")]
        public bool HasTimeLimit;
        public TimeSpan TimeLimit;
        public DateTime StartTime;
        public DateTime ExpirationTime;
        
        [Header("Choices & Resolution")]
        public List<EventChoice> Choices = new List<EventChoice>();
        public bool IsResolved = false;
        public EventChoice SelectedChoice;
        public DateTime ResolutionTime;
        
        [Header("Context")]
        public string StoryContext;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// A choice the player can make in response to an event
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        [Header("Choice Description")]
        public string ChoiceText;
        [TextArea(2, 4)]
        public string DetailedDescription;
        
        [Header("Costs")]
        public float CostCurrency = 0f;
        public float CostTime = 0f; // in hours
        public int CostSkillPoints = 0;
        public List<string> RequiredResources = new List<string>();
        
        [Header("Requirements")]
        public int MinPlayerLevel = 0;
        public float MinReputationScore = 0f;
        public List<string> RequiredUnlocks = new List<string>();
        
        [Header("Consequences")]
        public List<EventConsequence> Consequences = new List<EventConsequence>();
        
        [Header("Meta")]
        public bool IsDefaultChoice = false; // Used for auto-resolution
        public string ChoiceIcon = "";
        public EventChoiceRisk RiskLevel = EventChoiceRisk.Medium;
    }
    
    /// <summary>
    /// The result/consequence of making an event choice
    /// </summary>
    [System.Serializable]
    public class EventConsequence
    {
        [Header("Effect Type")]
        public ConsequenceType Type;
        public float Value;
        public float Duration = 0f; // in hours, 0 = permanent
        
        [Header("Details")]
        public string Description;
        public string TargetSystem; // Which system this affects
        public bool IsPositive = true;
        
        [Header("Advanced")]
        public AnimationCurve EffectCurve; // How the effect changes over time
        public List<string> ConditionalTriggers = new List<string>();
    }
    
    /// <summary>
    /// Data for displaying events in the UI
    /// </summary>
    [System.Serializable]
    public class EventDisplayData
    {
        public string EventId;
        public string Title;
        public string Description;
        public RandomEventCategory Category;
        public EventSeverity Severity;
        public TimeSpan TimeRemaining;
        public List<string> Choices;
        public string CategoryIcon;
        public Color SeverityColor;
        public bool HasTimeLimit;
        public string StoryContext;
        public float UrgencyScore; // 0-1, how urgent this event is
    }
    
    /// <summary>
    /// Story event for narrative progression
    /// </summary>
    [System.Serializable]
    public class StoryEvent
    {
        public string StoryEventId;
        public string StoryArc;
        public int SequenceNumber;
        public string Title;
        public string NarrativeText;
        public List<string> UnlockedFeatures = new List<string>();
        public List<EventChoice> StoryChoices = new List<EventChoice>();
        public bool IsCompleted = false;
    }
    
    /// <summary>
    /// Event outcome tracking for analytics and balancing
    /// </summary>
    [System.Serializable]
    public class EventOutcome
    {
        public string EventId;
        public string ChoiceSelected;
        public DateTime EventTime;
        public float PlayerLevel;
        public float PlayerReputationAtTime;
        public List<string> ConsequencesApplied = new List<string>();
        public bool PlayerSatisfied = true; // For balancing feedback
    }
    
    /// <summary>
    /// Player's event history and statistics
    /// </summary>
    [System.Serializable]
    public class EventHistory
    {
        public int TotalEventsEncountered = 0;
        public int EventsResolved = 0;
        public int EventsIgnored = 0;
        public Dictionary<EventCategory, int> EventsByCategory = new Dictionary<EventCategory, int>();
        public Dictionary<string, EventOutcome> CompletedEvents = new Dictionary<string, EventOutcome>();
        public List<string> UnlockedStoryArcs = new List<string>();
        public float AverageDecisionTime = 60f; // seconds
    }
    
    // Enums for event system
    
    /// <summary>
    /// Categories of random events
    /// </summary>
    public enum RandomEventCategory
    {
        Cultivation,    // Plant growing, health, genetics
        Market,         // Economic, trading, prices
        Weather,        // Environmental, power, disasters
        Social,         // Community, reputation, relationships
        Technology,     // Equipment, research, innovation
        Crisis,         // Emergencies, critical decisions
        Opportunity,    // Positive events, investments, growth
        Story           // Narrative progression events
    }
    
    /// <summary>
    /// Severity levels for events
    /// </summary>
    public enum EventSeverity
    {
        Positive,       // Good news, opportunities
        Low,           // Minor issues, easy decisions
        Medium,        // Moderate impact, important choices
        High,          // Significant consequences, urgent
        Critical       // Major crisis, game-changing
    }
    
    /// <summary>
    /// Types of consequences that events can have
    /// </summary>
    public enum ConsequenceType
    {
        // Economic
        Currency,
        Revenue,
        Expenses,
        MarketPrices,
        MarketDemand,
        MarketAccess,
        
        // Cultivation
        PlantHealth,
        PlantGrowth,
        PlantLoss,
        PlantStress,
        GrowthRate,
        YieldModifier,
        QualityModifier,
        
        // Progression
        Experience,
        SkillPoints,
        Research,
        Technology,
        
        // Environmental
        Temperature,
        Humidity,
        LightIntensity,
        CO2Levels,
        
        // Social
        Reputation,
        NetworkContacts,
        CommunityStanding,
        MediaAttention,
        
        // System
        UnlockFeature,
        UnlockStrain,
        EquipmentUpgrade,
        EquipmentLoss,
        AutomationEfficiency,
        
        // Business
        BusinessEquity,
        InventoryClear,
        QualityRequirement,
        ContractOffer,
        
        // Special
        DelayedOpportunity,
        StoryProgression,
        AchievementUnlock,
        
        // Meta
        EventFrequency,
        DifficultyModifier
    }
    
    /// <summary>
    /// Severity levels for consequences
    /// </summary>
    public enum ConsequenceSeverity
    {
        Trivial,        // Very minor impact
        Minor,          // Small impact
        Moderate,       // Medium impact
        Major,          // Significant impact
        Severe,         // High impact
        Critical        // Extreme impact
    }
    
    /// <summary>
    /// Risk levels for event choices
    /// </summary>
    public enum EventChoiceRisk
    {
        VeryLow,    // Safe choice, minimal consequences
        Low,        // Small risk, small reward
        Medium,     // Balanced risk/reward
        High,       // High risk, high reward
        VeryHigh    // Dangerous but potentially very rewarding
    }
    
    /// <summary>
    /// Difficulty scaling for events
    /// </summary>
    public enum EventDifficultyLevel
    {
        Easy,       // Fewer negative events, more time to decide
        Medium,     // Balanced event mix
        Hard        // More frequent negative events, time pressure
    }
    
    /// <summary>
    /// Event trigger conditions
    /// </summary>
    public enum EventTriggerType
    {
        Random,         // Standard random occurrence
        PlayerAction,   // Triggered by player behavior
        TimeBased,      // Time-triggered events
        StoryProgression, // Part of narrative sequence
        SystemState,    // Based on game state
        Achievement,    // Unlocked by achievements
        Seasonal,       // Calendar-based events
        Crisis,         // Emergency situations
        Chain          // Follow-up to previous events
    }
    
    /// <summary>
    /// Event resolution types
    /// </summary>
    public enum EventResolutionType
    {
        PlayerChoice,   // Player selects an option
        Automatic,      // Auto-resolved after timeout
        SystemDriven,   // Resolved by game systems
        Ignored,        // Player chose not to engage
        Interrupted     // Event ended by external factors
    }
}