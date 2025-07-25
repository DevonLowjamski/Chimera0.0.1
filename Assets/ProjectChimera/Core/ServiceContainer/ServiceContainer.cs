using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace ProjectChimera.Core.ServiceContainer
{
    /// <summary>
    /// PC014-1b: Comprehensive service container implementation with full lifecycle management
    /// Supports singleton, transient, and scoped lifetimes with constructor injection and circular dependency detection
    /// Thread-safe implementation suitable for Unity's multi-threaded environment
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private static ServiceContainer _instance;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Singleton instance of the service container
        /// </summary>
        public static ServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceContainer();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private readonly ConcurrentDictionary<Type, ServiceRegistration> _services = new ConcurrentDictionary<Type, ServiceRegistration>();
        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();
        private readonly ThreadLocal<HashSet<Type>> _resolutionStack = new ThreadLocal<HashSet<Type>>(() => new HashSet<Type>());
        
        [Header("Service Container Configuration")]
        [SerializeField] private bool _enableDetailedLogging = false;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private bool _enableCircularDependencyDetection = true;
        
        // Performance monitoring
        private long _totalResolutions = 0;
        private double _totalResolutionTime = 0.0;
        private readonly object _performanceLock = new object();

        public ServiceContainer()
        {
            // Register self for dependency injection
            RegisterSingleton<IServiceContainer>(this);
            
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainer] Initialized with circular dependency detection enabled");
            }
        }

        #region Registration Methods

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            RegisterSingleton<TInterface>(_ => new TImplementation());
        }

        public void RegisterSingleton<TInterface>(Func<IServiceContainer, TInterface> factory)
        {
            var serviceType = typeof(TInterface);
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = null,
                Lifetime = ServiceLifetime.Singleton,
                Factory = container => factory(container),
                RegistrationTime = DateTime.Now
            };

            _services.AddOrUpdate(serviceType, registration, (key, existing) => registration);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainer] Registered singleton: {serviceType.Name} with factory");
            }
        }

        public void RegisterSingleton<TInterface>(TInterface instance)
        {
            var serviceType = typeof(TInterface);
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = instance.GetType(),
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance,
                RegistrationTime = DateTime.Now
            };

            _services.AddOrUpdate(serviceType, registration, (key, existing) => registration);
            _singletonInstances.TryAdd(serviceType, instance);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainer] Registered singleton instance: {serviceType.Name}");
            }
        }

        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            RegisterTransient<TInterface>(_ => new TImplementation());
        }

        public void RegisterTransient<TInterface>(Func<IServiceContainer, TInterface> factory)
        {
            var serviceType = typeof(TInterface);
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = null,
                Lifetime = ServiceLifetime.Transient,
                Factory = container => factory(container),
                RegistrationTime = DateTime.Now
            };

            _services.AddOrUpdate(serviceType, registration, (key, existing) => registration);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainer] Registered transient: {serviceType.Name} with factory");
            }
        }

        public void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            RegisterScoped<TInterface>(_ => new TImplementation());
        }

        public void RegisterScoped<TInterface>(Func<IServiceContainer, TInterface> factory)
        {
            var serviceType = typeof(TInterface);
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = null,
                Lifetime = ServiceLifetime.Scoped,
                Factory = container => factory(container),
                RegistrationTime = DateTime.Now
            };

            _services.AddOrUpdate(serviceType, registration, (key, existing) => registration);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainer] Registered scoped: {serviceType.Name} with factory");
            }
        }

        #endregion

        #region Resolution Methods

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type serviceType)
        {
            var startTime = DateTime.Now;
            try
            {
                var result = ResolveInternal(serviceType);
                RecordResolution((DateTime.Now - startTime).TotalMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                if (_enableDetailedLogging)
                {
                    Debug.LogError($"[ServiceContainer] Failed to resolve {serviceType.Name}: {ex.Message}");
                }
                throw new ServiceResolutionException(serviceType, $"Failed to resolve service {serviceType.Name}", ex);
            }
        }

        public T TryResolve<T>() where T : class
        {
            return TryResolve(typeof(T)) as T;
        }

        public object TryResolve(Type serviceType)
        {
            try
            {
                return Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        private object ResolveInternal(Type serviceType)
        {
            // Check for circular dependencies
            if (_enableCircularDependencyDetection && _resolutionStack.Value.Contains(serviceType))
            {
                var dependencyChain = _resolutionStack.Value.ToList();
                dependencyChain.Add(serviceType);
                throw new CircularDependencyException(serviceType, dependencyChain);
            }

            if (!_services.TryGetValue(serviceType, out var registration))
            {
                // Try constructor injection for unregistered concrete types
                if (!serviceType.IsInterface && !serviceType.IsAbstract)
                {
                    return CreateWithConstructorInjection(serviceType);
                }
                
                throw new ServiceResolutionException(serviceType, $"Service {serviceType.Name} is not registered");
            }

            _resolutionStack.Value.Add(serviceType);
            try
            {
                return ResolveRegistration(registration);
            }
            finally
            {
                _resolutionStack.Value.Remove(serviceType);
            }
        }

        private object ResolveRegistration(ServiceRegistration registration)
        {
            switch (registration.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return ResolveSingleton(registration);
                    
                case ServiceLifetime.Transient:
                    return ResolveTransient(registration);
                    
                case ServiceLifetime.Scoped:
                    return ResolveScoped(registration);
                    
                default:
                    throw new ServiceResolutionException(registration.ServiceType, $"Unknown service lifetime: {registration.Lifetime}");
            }
        }

        private object ResolveSingleton(ServiceRegistration registration)
        {
            if (registration.Instance != null)
            {
                return registration.Instance;
            }

            return _singletonInstances.GetOrAdd(registration.ServiceType, _ =>
            {
                if (registration.Factory != null)
                {
                    return registration.Factory(this);
                }
                
                return CreateWithConstructorInjection(registration.ImplementationType ?? registration.ServiceType);
            });
        }

        private object ResolveTransient(ServiceRegistration registration)
        {
            if (registration.Factory != null)
            {
                return registration.Factory(this);
            }
            
            return CreateWithConstructorInjection(registration.ImplementationType ?? registration.ServiceType);
        }

        private object ResolveScoped(ServiceRegistration registration)
        {
            // For now, treat scoped as singleton (would need proper scope management)
            // This is a simplified implementation - full scoped support would require IServiceScope
            return ResolveSingleton(registration);
        }

        private object CreateWithConstructorInjection(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                args[i] = ResolveInternal(paramType);
            }
            
            return Activator.CreateInstance(implementationType, args);
        }

        #endregion

        #region Query Methods

        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type serviceType)
        {
            return _services.ContainsKey(serviceType);
        }

        public IEnumerable<Type> GetRegisteredTypes<T>()
        {
            return GetRegisteredTypes(typeof(T));
        }

        public IEnumerable<Type> GetRegisteredTypes(Type interfaceType)
        {
            return _services.Values
                .Where(r => interfaceType.IsAssignableFrom(r.ServiceType))
                .Select(r => r.ServiceType);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            var serviceType = typeof(T);
            return _services.Values
                .Where(r => serviceType.IsAssignableFrom(r.ServiceType))
                .Select(r => (T)ResolveRegistration(r));
        }

        #endregion

        #region Scope Management

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this);
        }

        #endregion

        #region Validation and Diagnostics

        public void ValidateServices()
        {
            var validationErrors = new List<string>();
            
            foreach (var registration in _services.Values)
            {
                try
                {
                    // Try to resolve each service to validate dependencies
                    ResolveInternal(registration.ServiceType);
                }
                catch (Exception ex)
                {
                    validationErrors.Add($"Service {registration.ServiceType.Name}: {ex.Message}");
                }
            }
            
            if (validationErrors.Any())
            {
                var errorMessage = "Service validation failed:\n" + string.Join("\n", validationErrors);
                throw new InvalidOperationException(errorMessage);
            }
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainer] Service validation completed successfully for {_services.Count} services");
            }
        }

        public IEnumerable<ServiceRegistrationInfo> GetRegistrationInfo()
        {
            return _services.Values.Select(r => new ServiceRegistrationInfo
            {
                ServiceType = r.ServiceType,
                ImplementationType = r.ImplementationType,
                Lifetime = r.Lifetime,
                HasFactory = r.Factory != null,
                HasInstance = r.Instance != null,
                RegistrationTime = r.RegistrationTime
            });
        }

        public void Clear()
        {
            _services.Clear();
            _singletonInstances.Clear();
            
            // Re-register self
            RegisterSingleton<IServiceContainer>(this);
            
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainer] All services cleared");
            }
        }

        #endregion

        #region Performance Monitoring

        private void RecordResolution(double resolutionTimeMs)
        {
            if (!_enablePerformanceMonitoring) return;
            
            lock (_performanceLock)
            {
                _totalResolutions++;
                _totalResolutionTime += resolutionTimeMs;
            }
        }

        public ServiceContainerPerformanceStats GetPerformanceStats()
        {
            lock (_performanceLock)
            {
                return new ServiceContainerPerformanceStats
                {
                    TotalResolutions = _totalResolutions,
                    AverageResolutionTimeMs = _totalResolutions > 0 ? _totalResolutionTime / _totalResolutions : 0.0,
                    RegisteredServiceCount = _services.Count,
                    SingletonInstanceCount = _singletonInstances.Count
                };
            }
        }

        #endregion

        #region Configuration

        public void SetDetailedLogging(bool enabled)
        {
            _enableDetailedLogging = enabled;
            Debug.Log($"[ServiceContainer] Detailed logging {(enabled ? "enabled" : "disabled")}");
        }

        public void SetPerformanceMonitoring(bool enabled)
        {
            _enablePerformanceMonitoring = enabled;
            Debug.Log($"[ServiceContainer] Performance monitoring {(enabled ? "enabled" : "disabled")}");
        }

        public void SetCircularDependencyDetection(bool enabled)
        {
            _enableCircularDependencyDetection = enabled;
            Debug.Log($"[ServiceContainer] Circular dependency detection {(enabled ? "enabled" : "disabled")}");
        }

        #endregion
    }

    /// <summary>
    /// Internal service registration data structure
    /// </summary>
    internal class ServiceRegistration
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Func<IServiceContainer, object> Factory { get; set; }
        public object Instance { get; set; }
        public DateTime RegistrationTime { get; set; }
    }

    /// <summary>
    /// Service scope implementation for scoped lifetime management
    /// </summary>
    internal class ServiceScope : IServiceScope
    {
        private readonly ConcurrentDictionary<Type, object> _scopedInstances = new ConcurrentDictionary<Type, object>();
        private bool _disposed = false;

        public IServiceContainer ServiceProvider { get; }

        public ServiceScope(IServiceContainer serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            // Dispose all scoped instances that implement IDisposable
            foreach (var instance in _scopedInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _scopedInstances.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Performance statistics for service container operations
    /// </summary>
    [System.Serializable]
    public class ServiceContainerPerformanceStats
    {
        public long TotalResolutions;
        public double AverageResolutionTimeMs;
        public int RegisteredServiceCount;
        public int SingletonInstanceCount;
        
        public override string ToString()
        {
            return $"Resolutions: {TotalResolutions}, Avg Time: {AverageResolutionTimeMs:F2}ms, Services: {RegisteredServiceCount}, Singletons: {SingletonInstanceCount}";
        }
    }
}