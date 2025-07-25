using UnityEngine;
using UnityEngine.Events;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Component that listens to GameEventSO events and responds with UnityEvents.
    /// Provides a bridge between the event system and Unity's inspector-based events.
    /// </summary>
    /// <typeparam name="T">Type of data the event carries</typeparam>
    public abstract class GameEventListener<T> : ChimeraMonoBehaviour
    {
        [Header("Event Listener Configuration")]
        [SerializeField] private GameEventSO<T> _gameEvent;
        [SerializeField] private bool _registerOnEnable = true;
        [SerializeField] private bool _unregisterOnDisable = true;

        [Header("Response Configuration")]
        [SerializeField] private EventResponse<T> _response = new EventResponse<T>();

        /// <summary>
        /// The event this listener is subscribed to.
        /// </summary>
        public GameEventSO<T> GameEvent => _gameEvent;

        /// <summary>
        /// The response that will be invoked when the event is raised.
        /// </summary>
        public EventResponse<T> Response => _response;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_registerOnEnable && _gameEvent != null)
            {
                RegisterToEvent();
            }
        }

        protected override void OnDisable()
        {
            if (_unregisterOnDisable && _gameEvent != null)
            {
                UnregisterFromEvent();
            }

            base.OnDisable();
        }

        /// <summary>
        /// Registers this listener to the configured event.
        /// </summary>
        public void RegisterToEvent()
        {
            if (_gameEvent == null)
            {
                LogWarning("Cannot register to event - no event assigned");
                return;
            }

            _gameEvent.RegisterListener(this);
            LogDebug($"Registered to event: {_gameEvent.DisplayName}");
        }

        /// <summary>
        /// Unregisters this listener from the configured event.
        /// </summary>
        public void UnregisterFromEvent()
        {
            if (_gameEvent == null) return;

            _gameEvent.UnregisterListener(this);
            LogDebug($"Unregistered from event: {_gameEvent.DisplayName}");
        }

        /// <summary>
        /// Called when the subscribed event is raised.
        /// </summary>
        /// <param name="eventData">Data passed with the event</param>
        public virtual void OnEventRaised(T eventData)
        {
            LogDebug($"Event raised: {_gameEvent?.DisplayName} with data: {eventData}");
            
            _response?.Invoke(eventData);
            OnEventRaisedInternal(eventData);
        }

        /// <summary>
        /// Override this method to handle event responses in code.
        /// </summary>
        /// <param name="eventData">Data passed with the event</param>
        protected virtual void OnEventRaisedInternal(T eventData)
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Changes the event this listener is subscribed to.
        /// </summary>
        /// <param name="newEvent">The new event to subscribe to</param>
        public void ChangeEvent(GameEventSO<T> newEvent)
        {
            if (_gameEvent != null)
            {
                UnregisterFromEvent();
            }

            _gameEvent = newEvent;

            if (_gameEvent != null && gameObject.activeInHierarchy)
            {
                RegisterToEvent();
            }
        }
    }

    /// <summary>
    /// UnityEvent wrapper for typed event responses.
    /// </summary>
    /// <typeparam name="T">Type of data the event carries</typeparam>
    [System.Serializable]
    public class EventResponse<T> : UnityEvent<T>
    {
        // UnityEvent with generic type support
    }

    /// <summary>
    /// Concrete listener for simple events with no data.
    /// </summary>
    [AddComponentMenu("Project Chimera/Events/Simple Game Event Listener")]
    public class SimpleGameEventListener : GameEventListener<System.EventArgs>
    {
        [Header("Simple Event Response")]
        [SerializeField] private UnityEvent _simpleResponse = new UnityEvent();

        protected override void OnEventRaisedInternal(System.EventArgs eventData)
        {
            _simpleResponse?.Invoke();
        }
    }

    /// <summary>
    /// Concrete listener for string events.
    /// </summary>
    [AddComponentMenu("Project Chimera/Events/String Game Event Listener")]
    public class StringGameEventListener : GameEventListener<string>
    {
        // Inherits all functionality from GameEventListener<string>
    }

    /// <summary>
    /// Concrete listener for float events.
    /// </summary>
    [AddComponentMenu("Project Chimera/Events/Float Game Event Listener")]
    public class FloatGameEventListener : GameEventListener<float>
    {
        // Inherits all functionality from GameEventListener<float>
    }

    /// <summary>
    /// Concrete listener for int events.
    /// </summary>
    [AddComponentMenu("Project Chimera/Events/Int Game Event Listener")]
    public class IntGameEventListener : GameEventListener<int>
    {
        // Inherits all functionality from GameEventListener<int>
    }
}