using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Base class for all gaming events in Project Chimera cannabis cultivation simulation.
    /// Part of Module 1: Gaming Experience Core - Deliverable 1.1
    /// 
    /// All events inherit from this base class to ensure consistent metadata and processing.
    /// </summary>
    [Serializable]
    public abstract class GameEvent
    {
        /// <summary>
        /// When the event was created (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Unique identifier for the player who triggered this event
        /// </summary>
        public string PlayerId { get; set; }
        
        /// <summary>
        /// Priority level for event processing
        /// </summary>
        public EventPriority Priority { get; set; } = EventPriority.Standard;
        
        /// <summary>
        /// Additional event-specific data
        /// </summary>
        public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Source system that generated this event
        /// </summary>
        public string SourceSystem { get; set; }
        
        /// <summary>
        /// Whether this event should be logged for analytics
        /// </summary>
        public bool LogForAnalytics { get; set; } = true;
        
        /// <summary>
        /// Whether this event should trigger UI notifications
        /// </summary>
        public bool TriggerUINotification { get; set; } = false;
        
        /// <summary>
        /// Set event data with type safety
        /// </summary>
        public void SetData<T>(string key, T value)
        {
            EventData[key] = value;
        }
        
        /// <summary>
        /// Get event data with type safety
        /// </summary>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (EventData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Create a copy of this event for modification
        /// </summary>
        public virtual GameEvent Clone()
        {
            var clone = (GameEvent)MemberwiseClone();
            clone.EventId = Guid.NewGuid().ToString();
            clone.EventData = new Dictionary<string, object>(EventData);
            return clone;
        }
        
        public override string ToString()
        {
            return $"{GetType().Name}[{EventId}] - Player: {PlayerId}, Priority: {Priority}, Time: {Timestamp:HH:mm:ss.fff}";
        }
    }
    
    /// <summary>
    /// Base class for system events (non-player triggered events)
    /// </summary>
    [Serializable]
    public abstract class SystemEvent : GameEvent
    {
        /// <summary>
        /// System component that generated this event
        /// </summary>
        public string SystemComponent { get; set; }
        
        /// <summary>
        /// Severity level for system events
        /// </summary>
        public SystemEventSeverity Severity { get; set; } = SystemEventSeverity.Info;
        
        protected SystemEvent()
        {
            // System events typically don't need UI notifications by default
            TriggerUINotification = false;
            PlayerId = "SYSTEM";
        }
    }
    
    /// <summary>
    /// Base class for player action events
    /// </summary>
    [Serializable]
    public abstract class PlayerActionEvent : GameEvent
    {
        /// <summary>
        /// Type of action the player performed
        /// </summary>
        public string ActionType { get; set; }
        
        /// <summary>
        /// Target object of the player's action
        /// </summary>
        public string TargetObject { get; set; }
        
        /// <summary>
        /// Input method used (mouse, keyboard, controller, etc.)
        /// </summary>
        public string InputMethod { get; set; }
        
        /// <summary>
        /// Whether this action should provide immediate feedback
        /// </summary>
        public bool RequiresImmediateFeedback { get; set; } = true;
        
        protected PlayerActionEvent()
        {
            Priority = EventPriority.Immediate; // Player actions need immediate response
            TriggerUINotification = true;
        }
    }
    
    /// <summary>
    /// Severity levels for system events
    /// </summary>
    public enum SystemEventSeverity
    {
        /// <summary>
        /// Informational messages
        /// </summary>
        Info = 0,
        
        /// <summary>
        /// Warning messages
        /// </summary>
        Warning = 1,
        
        /// <summary>
        /// Error messages that don't break functionality
        /// </summary>
        Error = 2,
        
        /// <summary>
        /// Critical errors that may break functionality
        /// </summary>
        Critical = 3
    }
    
    /// <summary>
    /// Event categories for organization and filtering
    /// </summary>
    public enum EventCategory
    {
        /// <summary>
        /// Player interaction events
        /// </summary>
        PlayerAction,
        
        /// <summary>
        /// Plant-related events (growth, health, etc.)
        /// </summary>
        Plant,
        
        /// <summary>
        /// Environmental events (temperature, humidity, etc.)
        /// </summary>
        Environment,
        
        /// <summary>
        /// Genetic events (breeding, mutations, etc.)
        /// </summary>
        Genetics,
        
        /// <summary>
        /// Achievement and progression events
        /// </summary>
        Achievement,
        
        /// <summary>
        /// Economic events (buying, selling, trading)
        /// </summary>
        Economy,
        
        /// <summary>
        /// System performance and health events
        /// </summary>
        System,
        
        /// <summary>
        /// UI interaction events
        /// </summary>
        UI,
        
        /// <summary>
        /// Analytics and telemetry events
        /// </summary>
        Analytics
    }
}