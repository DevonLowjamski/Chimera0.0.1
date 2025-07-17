using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for all Project Chimera event ScriptableObjects.
    /// Implements the event channel pattern for decoupled system communication.
    /// </summary>
    public abstract class ChimeraEventSO : ChimeraScriptableObject
    {
        [Header("Event Properties")]
        [SerializeField] private bool _logEventRaises = false;
        [SerializeField] private int _maxListenerCount = 100;

        /// <summary>
        /// Whether to log when this event is raised (useful for debugging).
        /// </summary>
        public bool LogEventRaises => _logEventRaises;

        /// <summary>
        /// Maximum number of listeners this event can have (prevents memory leaks).
        /// </summary>
        public int MaxListenerCount => _maxListenerCount;

        /// <summary>
        /// Current number of active listeners.
        /// </summary>
        public abstract int ListenerCount { get; }

        /// <summary>
        /// Clears all listeners from this event (useful for cleanup).
        /// </summary>
        public abstract void ClearAllListeners();

        /// <summary>
        /// Logs an event raise if logging is enabled.
        /// </summary>
        /// <param name="eventData">Optional event data to include in log</param>
        protected void LogEventRaise(object eventData = null)
        {
            if (_logEventRaises)
            {
                string dataString = eventData != null ? $" with data: {eventData}" : "";
                Debug.Log($"[Chimera][Event] {DisplayName} raised{dataString}", this);
            }
        }

        /// <summary>
        /// Validates listener count to prevent memory issues.
        /// </summary>
        /// <returns>True if listener count is within limits</returns>
        protected bool ValidateListenerCount()
        {
            if (ListenerCount >= _maxListenerCount)
            {
                Debug.LogWarning($"[Chimera] Event {DisplayName} has reached maximum listener count ({_maxListenerCount}). " +
                               "This may indicate a memory leak.", this);
                return false;
            }
            return true;
        }
    }
}