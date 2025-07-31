using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// High-performance event bus interface for real-time gaming events in Project Chimera.
    /// Part of Module 1: Gaming Experience Core - Deliverable 1.1
    /// 
    /// Designed for cannabis cultivation gaming with specific performance requirements:
    /// - <16ms for immediate events (UI responsiveness)
    /// - <100ms for standard events (achievement notifications)
    /// - Background processing for analytics events
    /// </summary>
    public interface IGameEventBus
    {
        /// <summary>
        /// Subscribe to events of type T with specified handler
        /// </summary>
        void Subscribe<T>(IGameEventHandler<T> handler) where T : GameEvent;
        
        /// <summary>
        /// Subscribe to events of type T with action handler
        /// </summary>
        void Subscribe<T>(Action<T> handler) where T : GameEvent;
        
        /// <summary>
        /// Publish event with standard processing (background queue)
        /// </summary>
        void Publish<T>(T gameEvent) where T : GameEvent;
        
        /// <summary>
        /// Publish event asynchronously for fire-and-forget scenarios
        /// </summary>
        Task PublishAsync<T>(T gameEvent) where T : GameEvent;
        
        /// <summary>
        /// Publish event immediately with <16ms guarantee for UI responsiveness
        /// Used for player input feedback, growth celebrations, achievement notifications
        /// </summary>
        void PublishImmediate<T>(T gameEvent) where T : GameEvent;
        
        /// <summary>
        /// Publish event with specified priority level
        /// </summary>
        void PublishWithPriority<T>(T gameEvent, EventPriority priority) where T : GameEvent;
        
        /// <summary>
        /// Unsubscribe specific handler from events of type T
        /// </summary>
        void Unsubscribe<T>(IGameEventHandler<T> handler) where T : GameEvent;
        
        /// <summary>
        /// Unsubscribe action handler from events of type T
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : GameEvent;
        
        /// <summary>
        /// Unsubscribe all handlers for the given subscriber object
        /// </summary>
        void UnsubscribeAll(object subscriber);
        
        /// <summary>
        /// Get current event bus performance metrics
        /// </summary>
        EventBusMetrics GetMetrics();
        
        /// <summary>
        /// Clear all event queues and reset the event bus
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Check if there are any subscribers for event type T
        /// </summary>
        bool HasSubscribers<T>() where T : GameEvent;
    }
    
    /// <summary>
    /// Generic event handler interface for strongly-typed event handling
    /// </summary>
    public interface IGameEventHandler<in T> where T : GameEvent
    {
        /// <summary>
        /// Handle the game event
        /// </summary>
        void Handle(T gameEvent);
        
        /// <summary>
        /// Priority level for this handler (higher priority handlers execute first)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Whether this handler can process events on background threads
        /// </summary>
        bool CanProcessAsync { get; }
    }
    
    /// <summary>
    /// Event priority levels for gaming responsiveness
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// Background processing - analytics, logging (no time constraint)
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// Standard processing - general game events (<100ms)
        /// </summary>
        Standard = 1,
        
        /// <summary>
        /// High priority - important notifications (<50ms)
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Immediate processing - UI responsiveness, player feedback (<16ms)
        /// </summary>
        Immediate = 3,
        
        /// <summary>
        /// Critical priority - system events that must be processed immediately
        /// </summary>
        Critical = 4
    }
    
    /// <summary>
    /// Performance metrics for the event bus system
    /// </summary>
    [Serializable]
    public struct EventBusMetrics
    {
        /// <summary>
        /// Total number of events processed since startup
        /// </summary>
        public long TotalEventsProcessed;
        
        /// <summary>
        /// Number of events currently in all queues
        /// </summary>
        public int PendingEvents;
        
        /// <summary>
        /// Average event processing time in milliseconds
        /// </summary>
        public float AverageProcessingTime;
        
        /// <summary>
        /// Maximum event processing time in milliseconds
        /// </summary>
        public float MaxProcessingTime;
        
        /// <summary>
        /// Events processed per second (current rate)
        /// </summary>
        public float EventsPerSecond;
        
        /// <summary>
        /// Number of currently subscribed handlers
        /// </summary>
        public int SubscriberCount;
        
        /// <summary>
        /// Memory usage of event queues in bytes
        /// </summary>
        public long MemoryUsage;
        
        /// <summary>
        /// Percentage of immediate events meeting <16ms target
        /// </summary>
        public float ImmediateEventSuccessRate;
        
        /// <summary>
        /// Number of events dropped due to queue overflow
        /// </summary>
        public long DroppedEvents;
        
        /// <summary>
        /// Whether the event bus is operating within performance targets
        /// </summary>
        public bool IsPerformingOptimally;
    }
}