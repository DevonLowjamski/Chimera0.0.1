using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Generic base class for typed event channels in Project Chimera.
    /// Provides event-driven communication with logging, validation, and history tracking.
    /// Follows Project Chimera's ScriptableObject-based event architecture.
    /// </summary>
    public abstract class GameEventChannelSO<T> : ChimeraDataSO where T : class
    {
        [Header("Event Channel Configuration")]
        [SerializeField] protected string _channelName;
        [SerializeField] protected string _description;
        [SerializeField] protected bool _logEvents = false;
        [SerializeField] protected bool _validateEvents = true;
        
        [Header("Event History")]
        [SerializeField] protected int _maxHistorySize = 100;
        [SerializeField] protected bool _maintainHistory = true;
        
        // Event history for debugging and analytics
        protected Queue<EventHistoryEntry<T>> _eventHistory = new Queue<EventHistoryEntry<T>>();
        
        // Event listeners
        public event Action<T> OnEventRaised;
        
        // Properties
        public string ChannelName => _channelName;
        public string Description => _description;
        public bool LogEvents => _logEvents;
        public IReadOnlyCollection<EventHistoryEntry<T>> EventHistory => _eventHistory;
        public int ListenerCount => OnEventRaised?.GetInvocationList().Length ?? 0;
        
        /// <summary>
        /// Raise an event on this channel
        /// </summary>
        public virtual void Raise(T eventData)
        {
            // Validate event data if enabled
            if (_validateEvents && !ValidateEventData(eventData))
            {
                LogError($"Invalid event data for channel {_channelName}");
                return;
            }
            
            // Log event if enabled
            if (_logEvents)
            {
                Debug.Log($"[{_channelName}] Event raised: {GetEventDescription(eventData)}");
            }
            
            // Add to history if enabled
            if (_maintainHistory)
            {
                AddToHistory(eventData);
            }
            
            // Raise the event to all listeners
            try
            {
                OnEventRaised?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                LogError($"Error raising event on channel {_channelName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Subscribe to events on this channel
        /// </summary>
        public void Subscribe(Action<T> listener)
        {
            if (listener != null)
            {
                OnEventRaised += listener;
                
                if (_logEvents)
                {
                    Debug.Log($"[{_channelName}] Listener subscribed. Total listeners: {ListenerCount}");
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe from events on this channel
        /// </summary>
        public void Unsubscribe(Action<T> listener)
        {
            if (listener != null)
            {
                OnEventRaised -= listener;
                
                if (_logEvents)
                {
                    Debug.Log($"[{_channelName}] Listener unsubscribed. Total listeners: {ListenerCount}");
                }
            }
        }
        
        /// <summary>
        /// Clear all event listeners
        /// </summary>
        public void ClearAllListeners()
        {
            OnEventRaised = null;
            
            if (_logEvents)
            {
                Debug.Log($"[{_channelName}] All listeners cleared");
            }
        }
        
        /// <summary>
        /// Clear event history
        /// </summary>
        public void ClearHistory()
        {
            _eventHistory.Clear();
            
            if (_logEvents)
            {
                Debug.Log($"[{_channelName}] Event history cleared");
            }
        }
        
        /// <summary>
        /// Get the most recent events from history
        /// </summary>
        public List<EventHistoryEntry<T>> GetRecentEvents(int count = 10)
        {
            var recentEvents = new List<EventHistoryEntry<T>>();
            var historyArray = _eventHistory.ToArray();
            
            int startIndex = Mathf.Max(0, historyArray.Length - count);
            for (int i = startIndex; i < historyArray.Length; i++)
            {
                recentEvents.Add(historyArray[i]);
            }
            
            return recentEvents;
        }
        
        protected virtual void AddToHistory(T eventData)
        {
            var historyEntry = new EventHistoryEntry<T>
            {
                EventData = eventData,
                Timestamp = DateTime.Now,
                ListenerCount = ListenerCount
            };
            
            _eventHistory.Enqueue(historyEntry);
            
            // Maintain history size limit
            while (_eventHistory.Count > _maxHistorySize)
            {
                _eventHistory.Dequeue();
            }
        }
        
        /// <summary>
        /// Validate event data before raising. Override in derived classes for custom validation.
        /// </summary>
        protected virtual bool ValidateEventData(T eventData)
        {
            return eventData != null;
        }
        
        /// <summary>
        /// Get a description of the event for logging. Override in derived classes for custom descriptions.
        /// </summary>
        protected virtual string GetEventDescription(T eventData)
        {
            return eventData?.ToString() ?? "null";
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_channelName))
            {
                _channelName = name;
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_channelName))
            {
                LogError("Channel name cannot be empty");
                isValid = false;
            }
            
            if (_maxHistorySize < 0)
            {
                LogError("Max history size cannot be negative");
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    /// <summary>
    /// Event history entry for tracking and debugging
    /// </summary>
    [System.Serializable]
    public class EventHistoryEntry<T>
    {
        public T EventData;
        public DateTime Timestamp;
        public int ListenerCount;
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {EventData} (Listeners: {ListenerCount})";
        }
    }
} 