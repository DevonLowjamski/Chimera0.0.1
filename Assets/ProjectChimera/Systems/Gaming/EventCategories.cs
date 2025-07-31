using System;
using System.Collections.Generic;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Event categories and helper utilities for organizing gaming events.
    /// Part of Module 1: Gaming Experience Core - Deliverable 1.2
    /// </summary>
    public static class EventCategories
    {
        /// <summary>
        /// Plant lifecycle and growth related events
        /// </summary>
        public static class Plant
        {
            public const string GROWTH = "Plant.Growth";
            public const string HEALTH = "Plant.Health";
            public const string HARVEST = "Plant.Harvest";
            public const string LIFECYCLE = "Plant.Lifecycle";
            public const string INTERACTION = "Plant.Interaction";
        }
        
        /// <summary>
        /// Environmental control and monitoring events
        /// </summary>
        public static class Environment
        {
            public const string CHANGE = "Environment.Change";
            public const string STRESS = "Environment.Stress";
            public const string OPTIMAL = "Environment.Optimal";
            public const string CONTROL = "Environment.Control";
            public const string MONITORING = "Environment.Monitoring";
        }
        
        /// <summary>
        /// Player action and interaction events
        /// </summary>
        public static class Player
        {
            public const string INTERACTION = "Player.Interaction";
            public const string TRANSACTION = "Player.Transaction";
            public const string CONTROL = "Player.Control";
            public const string INPUT = "Player.Input";
            public const string ACHIEVEMENT = "Player.Achievement";
        }
        
        /// <summary>
        /// Achievement and progression events
        /// </summary>
        public static class Achievement
        {
            public const string UNLOCKED = "Achievement.Unlocked";
            public const string EXPERIENCE = "Achievement.Experience";
            public const string MILESTONE = "Achievement.Milestone";
            public const string LEVEL_UP = "Achievement.LevelUp";
            public const string PROGRESS = "Achievement.Progress";
        }
        
        /// <summary>
        /// Genetics and breeding events
        /// </summary>
        public static class Genetics
        {
            public const string BREEDING = "Genetics.Breeding";
            public const string MUTATION = "Genetics.Mutation";
            public const string TRAIT = "Genetics.Trait";
            public const string INHERITANCE = "Genetics.Inheritance";
            public const string DISCOVERY = "Genetics.Discovery";
        }
        
        /// <summary>
        /// System and performance events
        /// </summary>
        public static class System
        {
            public const string PERFORMANCE = "System.Performance";
            public const string SAVE_LOAD = "System.SaveLoad";
            public const string ERROR = "System.Error";
            public const string WARNING = "System.Warning";
            public const string INFO = "System.Info";
        }
        
        /// <summary>
        /// UI and user experience events
        /// </summary>
        public static class UI
        {
            public const string NOTIFICATION = "UI.Notification";
            public const string CELEBRATION = "UI.Celebration";
            public const string FEEDBACK = "UI.Feedback";
            public const string TUTORIAL = "UI.Tutorial";
            public const string NAVIGATION = "UI.Navigation";
        }
        
        /// <summary>
        /// Economic and trading events
        /// </summary>
        public static class Economy
        {
            public const string TRANSACTION = "Economy.Transaction";
            public const string MARKET = "Economy.Market";
            public const string TRADING = "Economy.Trading";
            public const string INVESTMENT = "Economy.Investment";
            public const string PRICING = "Economy.Pricing";
        }
    }
    
    /// <summary>
    /// Event priority definitions for different types of gaming events
    /// </summary>
    public static class EventPriorities
    {
        /// <summary>
        /// Critical events that must be processed immediately
        /// </summary>
        public static readonly Dictionary<string, EventPriority> Critical = new Dictionary<string, EventPriority>
        {
            { "PlantHarvestedEvent", EventPriority.Immediate },
            { "GeneticMutationEvent", EventPriority.Immediate },
            { "AchievementUnlockedEvent", EventPriority.Immediate },
            { "PerformanceAlertEvent", EventPriority.Critical }
        };
        
        /// <summary>
        /// High priority events for important gameplay moments
        /// </summary>
        public static readonly Dictionary<string, EventPriority> High = new Dictionary<string, EventPriority>
        {
            { "PlantGrowthEvent", EventPriority.High },
            { "PlantHealthEvent", EventPriority.High },
            { "EnvironmentalStressEvent", EventPriority.High },
            { "BreedingCompletedEvent", EventPriority.High },
            { "ExperienceGainedEvent", EventPriority.High }
        };
        
        /// <summary>
        /// Standard priority events for general gameplay
        /// </summary>
        public static readonly Dictionary<string, EventPriority> Standard = new Dictionary<string, EventPriority>
        {
            { "EnvironmentalChangeEvent", EventPriority.Standard },
            { "TutorialStepEvent", EventPriority.Standard },
            { "SaveGameEvent", EventPriority.Standard }
        };
        
        /// <summary>
        /// Background priority events for analytics and logging
        /// </summary>
        public static readonly Dictionary<string, EventPriority> Background = new Dictionary<string, EventPriority>
        {
            { "AnalyticsEvent", EventPriority.Background },
            { "LoggingEvent", EventPriority.Background },
            { "MetricsEvent", EventPriority.Background }
        };
    }
    
    /// <summary>
    /// Event timing requirements for different event types
    /// </summary>
    public static class EventTiming
    {
        /// <summary>
        /// Events that require immediate UI feedback (<16ms)
        /// </summary>
        public static readonly HashSet<string> ImmediateFeedback = new HashSet<string>
        {
            "PlantInteractionEvent",
            "EnvironmentalControlEvent",
            "ItemTransactionEvent",
            "PlantHarvestedEvent",
            "AchievementUnlockedEvent"
        };
        
        /// <summary>
        /// Events that should trigger celebration UI
        /// </summary>
        public static readonly HashSet<string> CelebrationEvents = new HashSet<string>
        {
            "PlantHarvestedEvent",
            "AchievementUnlockedEvent",
            "GeneticMutationEvent",
            "MilestoneCompletedEvent",
            "OptimalEnvironmentAchievedEvent"
        };
        
        /// <summary>
        /// Events that should be logged for analytics
        /// </summary>
        public static readonly HashSet<string> AnalyticsEvents = new HashSet<string>
        {
            "PlantGrowthEvent",
            "PlantHarvestedEvent",
            "PlantInteractionEvent",
            "AchievementUnlockedEvent",
            "ExperienceGainedEvent",
            "BreedingCompletedEvent",
            "ItemTransactionEvent"
        };
        
        /// <summary>
        /// Events that require player notification
        /// </summary>
        public static readonly HashSet<string> NotificationEvents = new HashSet<string>
        {
            "PlantHarvestReadyEvent",
            "EnvironmentalStressEvent",
            "AchievementUnlockedEvent",
            "BreedingCompletedEvent",
            "GeneticMutationEvent",
            "PerformanceAlertEvent"
        };
    }
    
    /// <summary>
    /// Event validation and utility methods
    /// </summary>
    public static class EventValidation
    {
        /// <summary>
        /// Validate that an event has required properties
        /// </summary>
        public static bool ValidateEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return false;
            if (string.IsNullOrEmpty(gameEvent.EventId)) return false;
            if (gameEvent.Timestamp == default) return false;
            if (string.IsNullOrEmpty(gameEvent.SourceSystem)) return false;
            
            return true;
        }
        
        /// <summary>
        /// Get the appropriate priority for an event type
        /// </summary>
        public static EventPriority GetEventPriority(string eventTypeName)
        {
            if (EventPriorities.Critical.TryGetValue(eventTypeName, out var criticalPriority))
                return criticalPriority;
                
            if (EventPriorities.High.TryGetValue(eventTypeName, out var highPriority))
                return highPriority;
                
            if (EventPriorities.Standard.TryGetValue(eventTypeName, out var standardPriority))
                return standardPriority;
                
            if (EventPriorities.Background.TryGetValue(eventTypeName, out var backgroundPriority))
                return backgroundPriority;
                
            return EventPriority.Standard; // Default priority
        }
        
        /// <summary>
        /// Check if an event should trigger immediate UI feedback
        /// </summary>
        public static bool RequiresImmediateFeedback(string eventTypeName)
        {
            return EventTiming.ImmediateFeedback.Contains(eventTypeName);
        }
        
        /// <summary>
        /// Check if an event should trigger celebration UI
        /// </summary>
        public static bool ShouldCelebrate(string eventTypeName)
        {
            return EventTiming.CelebrationEvents.Contains(eventTypeName);
        }
        
        /// <summary>
        /// Check if an event should be logged for analytics
        /// </summary>
        public static bool ShouldLogAnalytics(string eventTypeName)
        {
            return EventTiming.AnalyticsEvents.Contains(eventTypeName);
        }
        
        /// <summary>
        /// Check if an event should trigger player notification
        /// </summary>
        public static bool ShouldNotifyPlayer(string eventTypeName)
        {
            return EventTiming.NotificationEvents.Contains(eventTypeName);
        }
    }
    
    /// <summary>
    /// Event filtering utilities for different contexts
    /// </summary>
    public static class EventFilters
    {
        /// <summary>
        /// Filter events by category
        /// </summary>
        public static bool ByCategory(GameEvent gameEvent, string category)
        {
            return gameEvent.SourceSystem?.StartsWith(category) == true;
        }
        
        /// <summary>
        /// Filter events by priority level
        /// </summary>
        public static bool ByPriority(GameEvent gameEvent, EventPriority minPriority)
        {
            return gameEvent.Priority >= minPriority;
        }
        
        /// <summary>
        /// Filter events by player ID
        /// </summary>
        public static bool ByPlayer(GameEvent gameEvent, string playerId)
        {
            return gameEvent.PlayerId == playerId;
        }
        
        /// <summary>
        /// Filter events by time range
        /// </summary>
        public static bool ByTimeRange(GameEvent gameEvent, DateTime startTime, DateTime endTime)
        {
            return gameEvent.Timestamp >= startTime && gameEvent.Timestamp <= endTime;
        }
        
        /// <summary>
        /// Filter events that require UI notifications
        /// </summary>
        public static bool RequiresUINotification(GameEvent gameEvent)
        {
            return gameEvent.TriggerUINotification || 
                   EventValidation.ShouldNotifyPlayer(gameEvent.GetType().Name);
        }
        
        /// <summary>
        /// Filter events for analytics logging
        /// </summary>
        public static bool ForAnalytics(GameEvent gameEvent)
        {
            return gameEvent.LogForAnalytics || 
                   EventValidation.ShouldLogAnalytics(gameEvent.GetType().Name);
        }
    }
}