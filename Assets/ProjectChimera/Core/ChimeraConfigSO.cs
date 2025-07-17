using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for Project Chimera configuration ScriptableObjects.
    /// Used for settings and parameters that may be modified by the player or system.
    /// </summary>
    public abstract class ChimeraConfigSO : ChimeraScriptableObject
    {
        [Header("Configuration Properties")]
        [SerializeField] private bool _allowRuntimeModification = false;
        [SerializeField] private bool _persistChanges = true;

        /// <summary>
        /// Whether this configuration can be modified at runtime.
        /// </summary>
        public bool AllowRuntimeModification => _allowRuntimeModification;

        /// <summary>
        /// Whether changes to this configuration should be persisted.
        /// </summary>
        public bool PersistChanges => _persistChanges;

        /// <summary>
        /// Event triggered when configuration values change.
        /// </summary>
        public System.Action<ChimeraConfigSO> OnConfigurationChanged;

        /// <summary>
        /// Applies changes to this configuration if runtime modification is allowed.
        /// </summary>
        protected void NotifyConfigurationChanged()
        {
            if (!_allowRuntimeModification && Application.isPlaying)
            {
                Debug.LogWarning($"[Chimera] Attempted to modify read-only configuration: {DisplayName}", this);
                return;
            }

            OnConfigurationChanged?.Invoke(this);
            
            if (_persistChanges && Application.isPlaying)
            {
                // Mark for persistence - implementation depends on save system
                OnConfigurationPersistenceRequired();
            }
        }

        /// <summary>
        /// Override this method to handle configuration persistence requirements.
        /// </summary>
        protected virtual void OnConfigurationPersistenceRequired()
        {
            // Base implementation - override in derived classes or handle via save system
        }

        /// <summary>
        /// Resets this configuration to its default values.
        /// </summary>
        public virtual void ResetToDefaults()
        {
            OnResetToDefaults();
            NotifyConfigurationChanged();
        }

        /// <summary>
        /// Override this method to implement configuration-specific reset logic.
        /// </summary>
        protected virtual void OnResetToDefaults()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Logs a warning message for this configuration asset.
        /// </summary>
        /// <param name="message">The warning message</param>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[Chimera][{GetType().Name}] {message}", this);
        }

        /// <summary>
        /// Logs an error message for this configuration asset.
        /// </summary>
        /// <param name="message">The error message</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[Chimera][{GetType().Name}] {message}", this);
        }

        public override bool ValidateData()
        {
            if (!base.ValidateData()) return false;

            return ValidateDataSpecific();
        }

        /// <summary>
        /// Override this method to implement configuration-specific validation logic.
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        protected virtual bool ValidateDataSpecific()
        {
            return true;
        }
    }
}