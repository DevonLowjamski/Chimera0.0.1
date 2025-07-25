using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Generic event channel for type-safe event communication.
    /// Used throughout Project Chimera for decoupled system messaging.
    /// </summary>
    /// <typeparam name="T">Type of data passed with the event</typeparam>
    public abstract class GameEventSO<T> : ChimeraEventSO
    {
        /// <summary>
        /// Event that can be subscribed to directly
        /// </summary>
        public System.Action<T> OnEventRaised;

        /// <summary>
        /// List of listeners for this event.
        /// </summary>
        private readonly List<GameEventListener<T>> _listeners = new List<GameEventListener<T>>();

        /// <summary>
        /// Current number of active listeners.
        /// </summary>
        public override int ListenerCount => _listeners.Count;

        /// <summary>
        /// Raises this event, notifying all registered listeners.
        /// </summary>
        /// <param name="eventData">Data to pass to listeners</param>
        public void Raise(T eventData)
        {
            LogEventRaise(eventData);

            // Trigger the direct event subscription
            OnEventRaised?.Invoke(eventData);

            // Iterate backwards to allow listeners to unregister during the event
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (_listeners[i] != null)
                {
                    _listeners[i].OnEventRaised(eventData);
                }
                else
                {
                    // Remove null listeners
                    _listeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Registers a listener for this event.
        /// </summary>
        /// <param name="listener">The listener to register</param>
        public void RegisterListener(GameEventListener<T> listener)
        {
            if (listener == null)
            {
                Debug.LogWarning($"[Chimera] Attempted to register null listener for event {DisplayName}", this);
                return;
            }

            if (!ValidateListenerCount())
            {
                return;
            }

            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a listener from this event.
        /// </summary>
        /// <param name="listener">The listener to unregister</param>
        public void UnregisterListener(GameEventListener<T> listener)
        {
            if (listener == null) return;

            _listeners.Remove(listener);
        }

        /// <summary>
        /// Clears all listeners from this event.
        /// </summary>
        public override void ClearAllListeners()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Called when the ScriptableObject is destroyed or domain is reloaded.
        /// </summary>
        private void OnDisable()
        {
            // Clear listeners on disable to prevent issues during domain reload
            if (Application.isPlaying)
            {
                ClearAllListeners();
            }
        }
    }

    /// <summary>
    /// Simple event channel with no data payload.
    /// </summary>
    [CreateAssetMenu(fileName = "New Simple Game Event", menuName = "Project Chimera/Events/Simple Game Event", order = 1)]
    public class SimpleGameEventSO : GameEventSO<System.EventArgs>
    {
        /// <summary>
        /// Raises this event with no data.
        /// </summary>
        public void Raise()
        {
            Raise(System.EventArgs.Empty);
        }
        
        /// <summary>
        /// Raises this event with optional data (for compatibility).
        /// </summary>
        public void RaiseEvent(object eventData = null)
        {
            Raise();
        }
    }

    /// <summary>
    /// Event channel that passes a string message.
    /// </summary>
    [CreateAssetMenu(fileName = "New String Game Event", menuName = "Project Chimera/Events/String Game Event", order = 2)]
    public class StringGameEventSO : GameEventSO<string>
    {
        // Inherits all functionality from GameEventSO<string>
    }

    /// <summary>
    /// Event channel that passes a float value.
    /// </summary>
    [CreateAssetMenu(fileName = "New Float Game Event", menuName = "Project Chimera/Events/Float Game Event", order = 3)]
    public class FloatGameEventSO : GameEventSO<float>
    {
        // Inherits all functionality from GameEventSO<float>
    }

    /// <summary>
    /// Event channel that passes an integer value.
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Game Event", menuName = "Project Chimera/Events/Int Game Event", order = 4)]
    public class IntGameEventSO : GameEventSO<int>
    {
        // Inherits all functionality from GameEventSO<int>
    }
}