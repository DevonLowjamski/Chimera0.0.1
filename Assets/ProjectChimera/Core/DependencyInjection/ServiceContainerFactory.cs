using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Factory for creating pre-configured service containers for different scenarios
    /// Provides standardized container configurations for various Project Chimera use cases
    /// </summary>
    public static class ServiceContainerFactory
    {
        private static readonly Dictionary<string, Func<IServiceContainer>> _containerFactories 
            = new Dictionary<string, Func<IServiceContainer>>();

        static ServiceContainerFactory()
        {
            RegisterStandardFactories();
        }

        #region Standard Factories

        /// <summary>
        /// Create a basic service container with minimal configuration
        /// </summary>
        public static IServiceContainer CreateBasic()
        {
            return ServiceContainerBuilder.Create()
                .ConfigureForUnity()
                .Build();
        }

        /// <summary>
        /// Create a container configured for Project Chimera development
        /// </summary>
        public static IServiceContainer CreateForDevelopment()
        {
            return ServiceContainerBuilder.CreateForChimera()
                .ConfigureForDevelopment()
                .Configure(container =>
                {
                    // Add development-specific services
                    RegisterDevelopmentServices(container);
                })
                .BuildAndValidate();
        }

        /// <summary>
        /// Create a container configured for Project Chimera production
        /// </summary>
        public static IServiceContainer CreateForProduction()
        {
            return ServiceContainerBuilder.CreateForChimera()
                .ConfigureForProduction()
                .Configure(container =>
                {
                    // Add production-specific services
                    RegisterProductionServices(container);
                })
                .BuildAndValidate();
        }

        /// <summary>
        /// Create a container for testing scenarios
        /// </summary>
        public static IServiceContainer CreateForTesting()
        {
            return ServiceContainerBuilder.Create()
                .Configure(container =>
                {
                    // Add test-specific services and mocks
                    RegisterTestingServices(container);
                })
                .Build();
        }

        /// <summary>
        /// Create a container with all Project Chimera managers
        /// </summary>
        public static IServiceContainer CreateWithAllManagers()
        {
            return ServiceContainerBuilder.CreateForChimera()
                .AddChimeraManagers()
                .Configure(container =>
                {
                    RegisterAllManagers(container);
                })
                .BuildAndValidate();
        }

        /// <summary>
        /// Create a high-performance container optimized for runtime
        /// </summary>
        public static IServiceContainer CreateHighPerformance()
        {
            return ServiceContainerBuilder.Create()
                .ConfigureForUnity()
                .Configure(container =>
                {
                    // Configure for high performance scenarios
                    RegisterHighPerformanceServices(container);
                })
                .Build();
        }

        /// <summary>
        /// Create a container for specific game phases
        /// </summary>
        public static IServiceContainer CreateForGamePhase(GamePhase phase)
        {
            var builder = ServiceContainerBuilder.CreateForChimera();

            switch (phase)
            {
                case GamePhase.MainMenu:
                    return builder.Configure(RegisterMainMenuServices).Build();
                
                case GamePhase.Gameplay:
                    return builder.Configure(RegisterGameplayServices).BuildAndValidate();
                
                case GamePhase.Settings:
                    return builder.Configure(RegisterSettingsServices).Build();
                
                default:
                    return builder.Build();
            }
        }

        #endregion

        #region Custom Factory Registration

        /// <summary>
        /// Register a custom factory function
        /// </summary>
        public static void RegisterFactory(string name, Func<IServiceContainer> factory)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Factory name cannot be null or empty", nameof(name));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _containerFactories[name] = factory;
            Debug.Log($"[ServiceContainerFactory] Registered custom factory: {name}");
        }

        /// <summary>
        /// Create a container using a registered factory
        /// </summary>
        public static IServiceContainer CreateFromFactory(string name)
        {
            if (_containerFactories.TryGetValue(name, out var factory))
            {
                try
                {
                    var container = factory();
                    Debug.Log($"[ServiceContainerFactory] Created container from factory: {name}");
                    return container;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceContainerFactory] Error creating container from factory '{name}': {ex.Message}");
                    throw;
                }
            }

            throw new ArgumentException($"No factory registered with name '{name}'", nameof(name));
        }

        /// <summary>
        /// Get all registered factory names
        /// </summary>
        public static IEnumerable<string> GetRegisteredFactories()
        {
            return _containerFactories.Keys;
        }

        #endregion

        #region Service Registration Helpers

        private static void RegisterStandardFactories()
        {
            RegisterFactory("Basic", CreateBasic);
            RegisterFactory("Development", CreateForDevelopment);
            RegisterFactory("Production", CreateForProduction);
            RegisterFactory("Testing", CreateForTesting);
            RegisterFactory("AllManagers", CreateWithAllManagers);
            RegisterFactory("HighPerformance", CreateHighPerformance);
        }

        private static void RegisterDevelopmentServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering development services");
            
            // Add development-specific services here
            // Example: Debug services, profiling tools, etc.
        }

        private static void RegisterProductionServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering production services");
            
            // Add production-specific services here
            // Example: Analytics, crash reporting, etc.
        }

        private static void RegisterTestingServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering testing services");
            
            // Add test-specific services and mocks here
            // Example: Mock services, test utilities, etc.
        }

        private static void RegisterAllManagers(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering all Project Chimera managers");
            
            // This would register all the manager interfaces
            // We'll implement this when we update the managers to use DI
        }

        private static void RegisterHighPerformanceServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering high-performance services");
            
            // Configure for performance-critical scenarios
            // Example: Object pools, cached services, etc.
        }

        private static void RegisterMainMenuServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering main menu services");
            
            // Services needed for main menu
            // Example: UI services, settings services, etc.
        }

        private static void RegisterGameplayServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering gameplay services");
            
            // Services needed during gameplay
            // Example: Plant managers, genetics services, etc.
        }

        private static void RegisterSettingsServices(IServiceContainer container)
        {
            Debug.Log("[ServiceContainerFactory] Registering settings services");
            
            // Services needed for settings management
            // Example: Configuration services, input services, etc.
        }

        #endregion

        #region Unity Integration

        /// <summary>
        /// Create a container and integrate it with Unity's lifecycle
        /// </summary>
        public static IServiceContainer CreateWithUnityIntegration(GameObject host = null)
        {
            var container = CreateForDevelopment();

            if (host == null)
            {
                host = new GameObject("ServiceContainer");
                UnityEngine.Object.DontDestroyOnLoad(host);
            }

            // Add a component to handle Unity lifecycle events
            var integration = host.GetComponent<ServiceContainerUnityIntegration>() 
                ?? host.AddComponent<ServiceContainerUnityIntegration>();
            
            integration.Initialize(container);

            return container;
        }

        #endregion
    }

    /// <summary>
    /// Game phase enumeration for context-specific container creation
    /// </summary>
    public enum GamePhase
    {
        MainMenu,
        Gameplay,
        Settings,
        Loading
    }

    /// <summary>
    /// Unity integration component for service container lifecycle management
    /// </summary>
    public class ServiceContainerUnityIntegration : MonoBehaviour
    {
        private IServiceContainer _container;
        private bool _isInitialized = false;

        public void Initialize(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _isInitialized = true;
            
            Debug.Log("[ServiceContainerUnityIntegration] Initialized with Unity lifecycle");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isInitialized)
            {
                Debug.Log($"[ServiceContainerUnityIntegration] Application focus changed: {hasFocus}");
                // Handle focus changes if needed
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isInitialized)
            {
                Debug.Log($"[ServiceContainerUnityIntegration] Application pause changed: {pauseStatus}");
                // Handle pause/resume if needed
            }
        }

        private void OnDestroy()
        {
            if (_isInitialized && _container != null)
            {
                try
                {
                    if (_container is IDisposable disposableContainer)
                    {
                        disposableContainer.Dispose();
                    }
                    Debug.Log("[ServiceContainerUnityIntegration] Container disposed with Unity lifecycle");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceContainerUnityIntegration] Error disposing container: {ex.Message}");
                }
            }
        }
    }
}