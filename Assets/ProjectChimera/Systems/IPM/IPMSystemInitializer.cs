using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Interfaces;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Initializes and registers all IPM gaming system managers.
    /// Call this during game startup to ensure all IPM systems are properly configured.
    /// </summary>
    public class IPMSystemInitializer : MonoBehaviour
    {
        [Header("IPM System Configuration")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _enableDebugLogging = true;
        
        // IPM Manager References
        [Header("IPM Manager Components")]
        [SerializeField] private CleanIPMManager _cleanIPMManager;
        
        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeIPMSystems();
            }
        }
        
        public void InitializeIPMSystems()
        {
            if (_enableDebugLogging)
                Debug.Log("[IPMSystemInitializer] Initializing IPM Gaming Systems...");
            
            // Create manager instances if not assigned
            CreateManagerInstances();
            
            // Register IPM managers with the GameManager
            RegisterIPMManagers();
            
            if (_enableDebugLogging)
                Debug.Log("[IPMSystemInitializer] IPM Gaming Systems initialized successfully!");
        }
        
        private void CreateManagerInstances()
        {
            // Create manager instances if not already assigned in the inspector
            if (_cleanIPMManager == null)
                _cleanIPMManager = gameObject.AddComponent<CleanIPMManager>();
        }
        
        private void RegisterIPMManagers()
        {
            // Register IPM managers with the GameManager using Project Chimera's pattern
            if (GameManager.Instance != null)
            {
                // Register managers as ChimeraManager instances
                GameManager.Instance.RegisterManager(_cleanIPMManager);
                
                if (_enableDebugLogging)
                    Debug.Log("[IPMSystemInitializer] Clean IPM manager registered with GameManager successfully");
            }
            else
            {
                Debug.LogWarning("[IPMSystemInitializer] GameManager.Instance not found - IPM managers not registered");
                Debug.LogWarning("[IPMSystemInitializer] Make sure GameManager is initialized before IPMSystemInitializer");
            }
        }
        
        /// <summary>
        /// Get a registered IPM manager by type. This can be called after initialization.
        /// </summary>
        public T GetIPMManager<T>() where T : ChimeraManager
        {
            return GameManager.Instance?.GetManager<T>();
        }
        
        /// <summary>
        /// Check if all IPM managers are properly initialized and registered.
        /// </summary>
        public bool AreAllManagersInitialized()
        {
            return _cleanIPMManager != null && _cleanIPMManager.IsInitialized;
        }
    }
}