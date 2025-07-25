using System;
using UnityEngine;
using ProjectChimera.Core.DependencyInjection;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Dependency injection-enabled base class for Project Chimera managers
    /// Extends ChimeraManager with service container integration and dependency resolution
    /// </summary>
    public abstract class DIChimeraManager : ChimeraManager, IDisposable
    {
        [Header("Dependency Injection")]
        [SerializeField] protected bool _autoRegisterWithContainer = true;
        [SerializeField] private ServiceLifetime _serviceLifetime = ServiceLifetime.Singleton;

        protected IServiceContainer ServiceContainer { get; private set; }
        protected ProjectChimera.Core.DependencyInjection.IServiceProvider ServiceProvider { get; private set; }

        private bool _isDisposed = false;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            InitializeDependencyInjection();
        }

        protected virtual void OnDestroy()
        {
            if (!_isDisposed)
            {
                Dispose();
            }
        }

        #endregion

        #region Dependency Injection

        /// <summary>
        /// Initialize dependency injection for this manager
        /// </summary>
        protected virtual void InitializeDependencyInjection()
        {
            try
            {
                // Get or create the global service container
                ServiceContainer = GetOrCreateServiceContainer();
                ServiceProvider = ServiceContainerIntegration.ToServiceProvider(ServiceContainer);

                // Auto-register this manager if enabled
                if (_autoRegisterWithContainer)
                {
                    RegisterSelfWithContainer();
                }

                // Resolve dependencies
                ResolveDependencies();

                Debug.Log($"[DIChimeraManager] Dependency injection initialized for {ManagerName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIChimeraManager] Failed to initialize DI for {ManagerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get or create the global service container
        /// </summary>
        protected virtual IServiceContainer GetOrCreateServiceContainer()
        {
            // Try to get existing container from GameManager or global static
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.TryGetComponent<ServiceContainerComponent>(out var containerComponent))
            {
                return containerComponent.Container;
            }

            // Create new container if none exists
            var container = ServiceContainerFactory.CreateForDevelopment();
            
            // Attach to GameManager if available
            if (gameManager != null)
            {
                var component = gameManager.gameObject.AddComponent<ServiceContainerComponent>();
                component.Initialize(container);
            }

            return container;
        }

        /// <summary>
        /// Set the service container for this manager (protected access for derived classes)
        /// </summary>
        protected void SetServiceContainer(IServiceContainer container)
        {
            ServiceContainer = container;
        }

        /// <summary>
        /// Register this manager with the service container
        /// </summary>
        protected virtual void RegisterSelfWithContainer()
        {
            var managerType = GetType();
            var interfaceTypes = managerType.GetInterfaces();

            // Register by concrete type - use RegisterSingleton without type parameter
            switch (_serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    ServiceContainer.RegisterSingleton(this);
                    break;
                case ServiceLifetime.Transient:
                    Debug.LogWarning($"[DIChimeraManager] Transient lifetime not recommended for managers: {ManagerName}");
                    break;
                case ServiceLifetime.Scoped:
                    Debug.LogWarning($"[DIChimeraManager] Scoped lifetime not recommended for managers: {ManagerName}");
                    break;
            }

            // Register by interfaces - skip automatic interface registration for now
            // TODO: Implement reflection-based interface registration if needed
            Debug.Log($"[DIChimeraManager] Registered {ManagerName} as concrete type");
        }

        /// <summary>
        /// Override this method to resolve dependencies using the service container
        /// </summary>
        protected virtual void ResolveDependencies()
        {
            // Override in derived classes to resolve specific dependencies
            // Example:
            // _timeManager = ServiceContainer.Resolve<ITimeManager>();
            // _dataManager = ServiceContainer.Resolve<IDataManager>();
        }

        #endregion

        #region Service Resolution Helpers

        /// <summary>
        /// Resolve a required service
        /// </summary>
        protected T ResolveService<T>() where T : class
        {
            try
            {
                return ServiceContainer.Resolve<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIChimeraManager] Failed to resolve required service {typeof(T).Name} in {ManagerName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Try to resolve an optional service
        /// </summary>
        protected T TryResolveService<T>() where T : class
        {
            try
            {
                return ServiceContainer.TryResolve<T>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DIChimeraManager] Failed to resolve optional service {typeof(T).Name} in {ManagerName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolve all implementations of a service
        /// </summary>
        protected System.Collections.Generic.IEnumerable<T> ResolveAllServices<T>() where T : class
        {
            try
            {
                return ServiceContainer.ResolveAll<T>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DIChimeraManager] Failed to resolve all services {typeof(T).Name} in {ManagerName}: {ex.Message}");
                return new T[0];
            }
        }

        /// <summary>
        /// Register a service with the container
        /// </summary>
        protected void RegisterService<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            try
            {
                ServiceContainer.RegisterSingleton<TInterface, TImplementation>();
                Debug.Log($"[DIChimeraManager] Registered service {typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIChimeraManager] Failed to register service {typeof(TInterface).Name} in {ManagerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a service instance with the container
        /// </summary>
        protected void RegisterServiceInstance<TInterface>(TInterface instance) where TInterface : class
        {
            try
            {
                ServiceContainer.RegisterSingleton(instance);
                Debug.Log($"[DIChimeraManager] Registered service instance {typeof(TInterface).Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIChimeraManager] Failed to register service instance {typeof(TInterface).Name} in {ManagerName}: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable Implementation

        public virtual void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                // Cleanup manager-specific resources
                OnDispose();

                // Cleanup DI resources
                ServiceProvider = null;
                ServiceContainer = null;

                _isDisposed = true;
                Debug.Log($"[DIChimeraManager] Disposed {ManagerName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIChimeraManager] Error disposing {ManagerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Override this method to perform manager-specific cleanup
        /// </summary>
        protected virtual void OnDispose()
        {
            // Override in derived classes for cleanup logic
        }

        #endregion
    }

    /// <summary>
    /// Component for holding service container reference on GameObjects
    /// </summary>
    public class ServiceContainerComponent : MonoBehaviour
    {
        public IServiceContainer Container { get; private set; }

        public void Initialize(IServiceContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Debug.Log("[ServiceContainerComponent] Service container attached to GameObject");
        }

        private void OnDestroy()
        {
            if (Container is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                    Debug.Log("[ServiceContainerComponent] Service container disposed with GameObject");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceContainerComponent] Error disposing service container: {ex.Message}");
                }
            }
        }
    }
}