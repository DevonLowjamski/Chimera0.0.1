using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for all Project Chimera data ScriptableObjects.
    /// Used for game configuration data that doesn't change at runtime.
    /// </summary>
    public abstract class ChimeraDataSO : ChimeraScriptableObject
    {
        [Header("Data Properties")]
        [SerializeField] private bool _isReadOnly = true;
        [SerializeField] private string _category = "General";

        /// <summary>
        /// Whether this data is read-only and should not be modified at runtime.
        /// </summary>
        public bool IsReadOnly => _isReadOnly;

        /// <summary>
        /// Category for organizing data assets in editors and browsers.
        /// </summary>
        public string Category => _category;

        public override bool ValidateData()
        {
            if (!base.ValidateData()) return false;

            // Ensure category is set
            if (string.IsNullOrEmpty(_category))
            {
                LogWarning("Data asset has no category assigned");
                return false;
            }

            return ValidateDataSpecific();
        }

        /// <summary>
        /// Override this method to implement data-specific validation logic.
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        protected virtual bool ValidateDataSpecific()
        {
            return true;
        }

        /// <summary>
        /// Logs a warning message for this data asset.
        /// </summary>
        /// <param name="message">The warning message</param>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[Chimera][{GetType().Name}] {message}", this);
        }

        /// <summary>
        /// Logs an error message for this data asset.
        /// </summary>
        /// <param name="message">The error message</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[Chimera][{GetType().Name}] {message}", this);
        }
    }
}