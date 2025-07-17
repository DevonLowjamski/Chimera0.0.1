using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Game Event Channel - ScriptableObject-based event system for decoupled communication
    /// Provides type-safe event channels for inter-system communication
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event Channel", menuName = "Project Chimera/Events/Game Event Channel")]
    public class GameEventChannelSO : ChimeraDataSO
    {
        [Header("Event Configuration")]
        public string EventName;
        public EventCategory Category;
        public EventPriority Priority = EventPriority.Medium;
        public bool LogEvents = false;
        public bool PersistEvents = false;
        
        [Header("Event History")]
        [SerializeField] protected List<EventLogEntry> _eventHistory = new List<EventLogEntry>();
        [Range(1, 1000)] public int MaxHistoryEntries = 100;
        
        // Unity Event system for inspector integration
        [Header("Unity Events")]
        public UnityEvent OnEventRaised = new UnityEvent();
        public UnityEvent<object> OnEventRaisedWithData = new UnityEvent<object>();
        
        // Generic event system
        private event Action _onEventRaised;
        private event Action<object> _onEventRaisedWithData;
        
        /// <summary>
        /// Public event that external systems can subscribe to
        /// </summary>
        public event Action EventRaised
        {
            add { _onEventRaised += value; }
            remove { _onEventRaised -= value; }
        }
        
        /// <summary>
        /// Public event with data that external systems can subscribe to
        /// </summary>
        public event Action<object> EventRaisedWithData
        {
            add { _onEventRaisedWithData += value; }
            remove { _onEventRaisedWithData -= value; }
        }
        
        /// <summary>
        /// Raise an event without data
        /// </summary>
        public virtual void RaiseEvent()
        {
            if (LogEvents)
            {
                LogEvent("Event Raised", null);
            }
            
            _onEventRaised?.Invoke();
            OnEventRaised?.Invoke();
        }
        
        /// <summary>
        /// Raise an event with data
        /// </summary>
        public virtual void RaiseEvent(object eventData)
        {
            if (LogEvents)
            {
                LogEvent("Event Raised with Data", eventData);
            }
            
            _onEventRaisedWithData?.Invoke(eventData);
            OnEventRaisedWithData?.Invoke(eventData);
        }
        
        /// <summary>
        /// Subscribe to events without data
        /// </summary>
        public virtual void RegisterListener(Action listener)
        {
            _onEventRaised += listener;
        }
        
        /// <summary>
        /// Subscribe to events and return a disposable for cleanup
        /// </summary>
        public virtual IDisposable Subscribe(Action listener)
        {
            _onEventRaised += listener;
            return new EventSubscription(() => _onEventRaised -= listener);
        }
        
        /// <summary>
        /// Subscribe to events with data and return a disposable for cleanup
        /// </summary>
        public virtual IDisposable Subscribe(Action<object> listener)
        {
            _onEventRaisedWithData += listener;
            return new EventSubscription(() => _onEventRaisedWithData -= listener);
        }
        
        /// <summary>
        /// Subscribe to events with data
        /// </summary>
        public virtual void RegisterListener(Action<object> listener)
        {
            _onEventRaisedWithData += listener;
        }
        
        /// <summary>
        /// Unsubscribe from events without data
        /// </summary>
        public virtual void UnregisterListener(Action listener)
        {
            _onEventRaised -= listener;
        }
        
        /// <summary>
        /// Unsubscribe from events with data
        /// </summary>
        public virtual void UnregisterListener(Action<object> listener)
        {
            _onEventRaisedWithData -= listener;
        }
        
        /// <summary>
        /// Subscribe to Unity events without data (for inspector/visual scripting)
        /// </summary>
        public virtual void AddUnityListener(UnityEngine.Events.UnityAction listener)
        {
            OnEventRaised.AddListener(listener);
        }
        
        /// <summary>
        /// Subscribe to Unity events with data (for inspector/visual scripting)
        /// </summary>
        public virtual void AddUnityListener(UnityEngine.Events.UnityAction<object> listener)
        {
            OnEventRaisedWithData.AddListener(listener);
        }
        
        /// <summary>
        /// Unsubscribe from Unity events without data
        /// </summary>
        public virtual void RemoveUnityListener(UnityEngine.Events.UnityAction listener)
        {
            OnEventRaised.RemoveListener(listener);
        }
        
        /// <summary>
        /// Unsubscribe from Unity events with data
        /// </summary>
        public virtual void RemoveUnityListener(UnityEngine.Events.UnityAction<object> listener)
        {
            OnEventRaisedWithData.RemoveListener(listener);
        }
        
        /// <summary>
        /// Clear all listeners
        /// </summary>
        public virtual void ClearListeners()
        {
            _onEventRaised = null;
            _onEventRaisedWithData = null;
            OnEventRaised?.RemoveAllListeners();
            OnEventRaisedWithData?.RemoveAllListeners();
        }
        
        /// <summary>
        /// Get event history
        /// </summary>
        public List<EventLogEntry> GetEventHistory()
        {
            return new List<EventLogEntry>(_eventHistory);
        }
        
        /// <summary>
        /// Clear event history
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
        }
        
        /// <summary>
        /// Initialize the event channel
        /// </summary>
        public virtual void Initialize()
        {
            // Clear any existing listeners and history on initialization
            ClearListeners();
            if (!PersistEvents)
            {
                ClearHistory();
            }
            
            if (LogEvents)
            {
                Debug.Log($"Event channel '{EventName}' initialized", this);
            }
        }
        
        private void LogEvent(string eventType, object eventData)
        {
            var entry = new EventLogEntry
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                EventData = eventData?.ToString() ?? "null",
                EventName = EventName
            };
            
            _eventHistory.Add(entry);
            
            // Limit history size
            while (_eventHistory.Count > MaxHistoryEntries)
            {
                _eventHistory.RemoveAt(0);
            }
            
            if (LogEvents)
            {
                Debug.Log($"[{EventName}] {eventType}: {entry.EventData}");
            }
        }
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(EventName))
            {
                EventName = name;
            }
        }
    }
    
    /// <summary>
    /// Typed Game Event Channel for specific data types
    /// </summary>
    [System.Serializable]
    public abstract class TypedGameEventChannelSO<T> : GameEventChannelSO
    {
        private event Action<T> _onTypedEventRaised;
        
        public virtual void RaiseEvent(T eventData)
        {
            if (LogEvents)
            {
                LogEvent($"Typed Event Raised: {typeof(T).Name}", eventData);
            }
            
            _onTypedEventRaised?.Invoke(eventData);
            base.RaiseEvent(eventData);
        }
        
        public virtual void RegisterListener(Action<T> listener)
        {
            _onTypedEventRaised += listener;
        }
        
        public virtual void UnregisterListener(Action<T> listener)
        {
            _onTypedEventRaised -= listener;
        }
        
        public override void ClearListeners()
        {
            _onTypedEventRaised = null;
            base.ClearListeners();
        }
        
        private void LogEvent(string eventType, T eventData)
        {
            var entry = new EventLogEntry
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                EventData = eventData?.ToString() ?? "null",
                EventName = EventName
            };
            
            _eventHistory.Add(entry);
            
            // Limit history size
            while (_eventHistory.Count > MaxHistoryEntries)
            {
                _eventHistory.RemoveAt(0);
            }
        }
    }
    
    [System.Serializable]
    public class EventLogEntry
    {
        public DateTime Timestamp;
        public string EventType;
        public string EventData;
        public string EventName;
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {EventName}: {EventType} - {EventData}";
        }
    }
    
    public enum EventCategory
    {
        System,
        Gameplay,
        UI,
        Audio,
        Animation,
        Network,
        Debug,
        Analytics,
        Achievement,
        Progression
    }
    
    // EventPriority is defined in ProjectChimera.Core.Events
    
    /// <summary>
    /// Helper class for event subscription management
    /// </summary>
    public class EventSubscription : IDisposable
    {
        private readonly Action _unsubscribeAction;
        
        public EventSubscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }
        
        public void Dispose()
        {
            _unsubscribeAction?.Invoke();
        }
    }
}