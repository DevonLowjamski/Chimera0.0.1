using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProjectChimera.Core.ServiceContainer
{
    /// <summary>
    /// PC014-1c: Extension methods for easy service resolution throughout Project Chimera
    /// Provides convenient access to dependency injection container from any component
    /// Supports both MonoBehaviour and non-MonoBehaviour service consumers
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Resolves a service from the global service container
        /// </summary>
        public static T GetService<T>(this MonoBehaviour component)
        {
            if (ServiceContainerBootstrapper.Container == null)
            {
                Debug.LogError($"[ServiceExtensions] Service container not initialized. Cannot resolve {typeof(T).Name}");
                return default(T);
            }

            try
            {
                return ServiceContainerBootstrapper.Container.Resolve<T>();
            }
            catch (ServiceResolutionException ex)
            {
                Debug.LogError($"[ServiceExtensions] Failed to resolve service {typeof(T).Name} for {component.name}: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Tries to resolve a service from the global service container, returns null if not found
        /// </summary>
        public static T TryGetService<T>(this MonoBehaviour component) where T : class
        {
            if (ServiceContainerBootstrapper.Container == null)
            {
                return null;
            }

            return ServiceContainerBootstrapper.Container.TryResolve<T>();
        }

        /// <summary>
        /// Checks if a service is registered in the container
        /// </summary>
        public static bool HasService<T>(this MonoBehaviour component)
        {
            return ServiceContainerBootstrapper.Container?.IsRegistered<T>() ?? false;
        }

        /// <summary>
        /// Resolves a service from the global service container (for non-MonoBehaviour classes)
        /// </summary>
        public static T GetService<T>()
        {
            if (ServiceContainerBootstrapper.Container == null)
            {
                Debug.LogError($"[ServiceExtensions] Service container not initialized. Cannot resolve {typeof(T).Name}");
                return default(T);
            }

            try
            {
                return ServiceContainerBootstrapper.Container.Resolve<T>();
            }
            catch (ServiceResolutionException ex)
            {
                Debug.LogError($"[ServiceExtensions] Failed to resolve service {typeof(T).Name}: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Tries to resolve a service from the global service container (for non-MonoBehaviour classes)
        /// </summary>
        public static T TryGetService<T>() where T : class
        {
            return ServiceContainerBootstrapper.Container?.TryResolve<T>();
        }

        /// <summary>
        /// Checks if a service is registered in the container (for non-MonoBehaviour classes)
        /// </summary>
        public static bool HasService<T>()
        {
            return ServiceContainerBootstrapper.Container?.IsRegistered<T>() ?? false;
        }

        /// <summary>
        /// Resolves a service by type from the global service container
        /// </summary>
        public static object GetService(Type serviceType)
        {
            if (ServiceContainerBootstrapper.Container == null)
            {
                Debug.LogError($"[ServiceExtensions] Service container not initialized. Cannot resolve {serviceType.Name}");
                return null;
            }

            try
            {
                return ServiceContainerBootstrapper.Container.Resolve(serviceType);
            }
            catch (ServiceResolutionException ex)
            {
                Debug.LogError($"[ServiceExtensions] Failed to resolve service {serviceType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tries to resolve a service by type from the global service container
        /// </summary>
        public static object TryGetService(Type serviceType)
        {
            return ServiceContainerBootstrapper.Container?.TryResolve(serviceType);
        }

        /// <summary>
        /// Checks if a service type is registered in the container
        /// </summary>
        public static bool HasService(Type serviceType)
        {
            return ServiceContainerBootstrapper.Container?.IsRegistered(serviceType) ?? false;
        }
    }

    /// <summary>
    /// Attribute to mark fields or properties for automatic dependency injection
    /// Used with ServiceInjector component to automatically inject dependencies
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Whether this dependency is optional (won't log errors if not found)
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <summary>
        /// Custom service type to resolve (if different from field/property type)
        /// </summary>
        public Type ServiceType { get; set; }

        public InjectAttribute()
        {
        }

        public InjectAttribute(bool optional)
        {
            Optional = optional;
        }

        public InjectAttribute(Type serviceType, bool optional = false)
        {
            ServiceType = serviceType;
            Optional = optional;
        }
    }

    /// <summary>
    /// Component that provides automatic dependency injection for MonoBehaviours
    /// Add to GameObjects that need dependency injection without manual GetService calls
    /// </summary>
    public class ServiceInjector : MonoBehaviour
    {
        [Header("Injection Configuration")]
        [SerializeField] private bool _injectOnAwake = true;
        [SerializeField] private bool _injectOnStart = false;
        [SerializeField] private bool _enableDetailedLogging = false;

        void Awake()
        {
            if (_injectOnAwake)
            {
                InjectDependencies();
            }
        }

        void Start()
        {
            if (_injectOnStart)
            {
                InjectDependencies();
            }
        }

        /// <summary>
        /// Performs dependency injection on all components of this GameObject
        /// </summary>
        public void InjectDependencies()
        {
            var components = GetComponents<MonoBehaviour>();
            
            foreach (var component in components)
            {
                if (component == this) continue; // Skip self
                
                InjectDependencies(component);
            }
        }

        /// <summary>
        /// Performs dependency injection on a specific component
        /// </summary>
        public void InjectDependencies(MonoBehaviour target)
        {
            if (target == null) return;

            var targetType = target.GetType();
            var fields = targetType.GetFields(System.Reflection.BindingFlags.NonPublic | 
                                            System.Reflection.BindingFlags.Public | 
                                            System.Reflection.BindingFlags.Instance);

            int injectedCount = 0;

            foreach (var field in fields)
            {
                var injectAttribute = field.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault() as InjectAttribute;
                if (injectAttribute == null) continue;

                var serviceType = injectAttribute.ServiceType ?? field.FieldType;
                var service = ServiceExtensions.TryGetService(serviceType);

                if (service != null)
                {
                    field.SetValue(target, service);
                    injectedCount++;

                    if (_enableDetailedLogging)
                    {
                        Debug.Log($"[ServiceInjector] Injected {serviceType.Name} into {targetType.Name}.{field.Name}");
                    }
                }
                else if (!injectAttribute.Optional)
                {
                    Debug.LogWarning($"[ServiceInjector] Failed to inject required service {serviceType.Name} into {targetType.Name}.{field.Name}");
                }
            }

            // Also handle properties
            var properties = targetType.GetProperties(System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.Instance);

            foreach (var property in properties)
            {
                var injectAttribute = property.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault() as InjectAttribute;
                if (injectAttribute == null) continue;
                if (!property.CanWrite) continue;

                var serviceType = injectAttribute.ServiceType ?? property.PropertyType;
                var service = ServiceExtensions.TryGetService(serviceType);

                if (service != null)
                {
                    property.SetValue(target, service);
                    injectedCount++;

                    if (_enableDetailedLogging)
                    {
                        Debug.Log($"[ServiceInjector] Injected {serviceType.Name} into {targetType.Name}.{property.Name}");
                    }
                }
                else if (!injectAttribute.Optional)
                {
                    Debug.LogWarning($"[ServiceInjector] Failed to inject required service {serviceType.Name} into {targetType.Name}.{property.Name}");
                }
            }

            if (_enableDetailedLogging && injectedCount > 0)
            {
                Debug.Log($"[ServiceInjector] Completed dependency injection for {targetType.Name}: {injectedCount} services injected");
            }
        }

        /// <summary>
        /// Manual dependency injection for any object
        /// </summary>
        public static void InjectDependencies(object target)
        {
            if (target == null) return;

            var targetType = target.GetType();
            var fields = targetType.GetFields(System.Reflection.BindingFlags.NonPublic | 
                                            System.Reflection.BindingFlags.Public | 
                                            System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var injectAttribute = field.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault() as InjectAttribute;
                if (injectAttribute == null) continue;

                var serviceType = injectAttribute.ServiceType ?? field.FieldType;
                var service = ServiceExtensions.TryGetService(serviceType);

                if (service != null)
                {
                    field.SetValue(target, service);
                }
                else if (!injectAttribute.Optional)
                {
                    Debug.LogWarning($"[ServiceInjector] Failed to inject required service {serviceType.Name} into {targetType.Name}.{field.Name}");
                }
            }

            var properties = targetType.GetProperties(System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.Instance);

            foreach (var property in properties)
            {
                var injectAttribute = property.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault() as InjectAttribute;
                if (injectAttribute == null) continue;
                if (!property.CanWrite) continue;

                var serviceType = injectAttribute.ServiceType ?? property.PropertyType;
                var service = ServiceExtensions.TryGetService(serviceType);

                if (service != null)
                {
                    property.SetValue(target, service);
                }
                else if (!injectAttribute.Optional)
                {
                    Debug.LogWarning($"[ServiceInjector] Failed to inject required service {serviceType.Name} into {targetType.Name}.{property.Name}");
                }
            }
        }
    }
}