using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for all Project Chimera ScriptableObjects.
    /// Provides common functionality, validation, and enforces naming conventions.
    /// </summary>
    public abstract class ChimeraScriptableObject : ScriptableObject
    {
        [Header("Chimera Base Properties")]
        [SerializeField] private string _uniqueID;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(3, 5)] private string _description;
        [SerializeField] private string _version = "1.0.0";

        /// <summary>
        /// Unique identifier for this ScriptableObject instance.
        /// Generated automatically if not set.
        /// </summary>
        public string UniqueID 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(_uniqueID))
                {
                    _uniqueID = System.Guid.NewGuid().ToString();
                }
                return _uniqueID; 
            } 
        }

        /// <summary>
        /// Human-readable display name for UI purposes.
        /// </summary>
        public string DisplayName 
        { 
            get 
            { 
                return string.IsNullOrEmpty(_displayName) ? name : _displayName; 
            } 
        }

        /// <summary>
        /// Description of this ScriptableObject's purpose and functionality.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Version string for tracking asset iterations.
        /// </summary>
        public string Version => _version;

        /// <summary>
        /// Called when the ScriptableObject is created or loaded.
        /// Override to implement initialization logic.
        /// </summary>
        protected virtual void OnEnable()
        {
            ValidateData();
        }

        /// <summary>
        /// Validates the data in this ScriptableObject.
        /// Override to implement custom validation logic.
        /// </summary>
        public virtual bool ValidateData()
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning($"[Chimera] ScriptableObject of type {GetType().Name} has no name assigned.", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when values are changed in the inspector.
        /// Override to implement custom validation or update logic.
        /// </summary>
        protected virtual void OnValidate()
        {
            #if UNITY_EDITOR
            ValidateData();
            #endif
        }

        /// <summary>
        /// Returns a formatted string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}: {DisplayName} (ID: {UniqueID})";
        }

        /// <summary>
        /// Logs an info message with this object as context.
        /// </summary>
        protected void LogInfo(string message)
        {
            Debug.Log($"[{GetType().Name}] {message}", this);
        }

        /// <summary>
        /// Logs a warning message with this object as context.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}] {message}", this);
        }

        /// <summary>
        /// Logs an error message with this object as context.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}] {message}", this);
        }
    }
}