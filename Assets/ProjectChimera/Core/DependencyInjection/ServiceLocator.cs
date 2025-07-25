using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Unity-optimized service locator implementation for Project Chimera
    /// Provides efficient dependency injection with proper Unity lifecycle integration
    /// Thread-safe singleton pattern with performance optimization for game development
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {
        private static ServiceLocator _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<Type, ServiceRegistration> _services 
            = new Dictionary<Type, ServiceRegistration>();
        private readonly Dictionary<Type, object> _singletonInstances 
            = new Dictionary<Type, object>();
        private readonly List<IServiceScope> _activeScopes 
            = new List<IServiceScope>();

        // Performance tracking
        private int _totalResolutions = 0;
        private int _cacheHits = 0;
        private readonly Dictionary<Type, int> _resolutionCounts 
            = new Dictionary<Type, int>();

        /// <summary>
        /// Global service locator instance
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceLocator();
                            _instance.RegisterCoreServices();
                        }
                    }
                }
                return _instance;
            }
        }

        private ServiceLocator() { }

        #region Service Registration

        public void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            lock (_lock)
            {
                var registration = new ServiceRegistration
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = typeof(TImplementation),
                    Lifetime = ServiceLifetime.Singleton,
                    Factory = locator => new TImplementation()
                };

                _services[typeof(TInterface)] = registration;
                
                Debug.Log($"[ServiceLocator] Registered Singleton: {typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
            }
        }

        public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            lock (_lock)
            {
                var registration = new ServiceRegistration
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = instance.GetType(),
                    Lifetime = ServiceLifetime.Singleton,
                    Instance = instance
                };

                _services[typeof(TInterface)] = registration;
                _singletonInstances[typeof(TInterface)] = instance;
                
                Debug.Log($"[ServiceLocator] Registered Singleton Instance: {typeof(TInterface).Name}");
            }
        }

        public void RegisterTransient<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            lock (_lock)
            {
                var registration = new ServiceRegistration
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = typeof(TImplementation),
                    Lifetime = ServiceLifetime.Transient,
                    Factory = locator => new TImplementation()
                };

                _services[typeof(TInterface)] = registration;
                
                Debug.Log($"[ServiceLocator] Registered Transient: {typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
            }
        }

        public void RegisterScoped<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            lock (_lock)
            {
                var registration = new ServiceRegistration
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = typeof(TImplementation),
                    Lifetime = ServiceLifetime.Scoped,
                    Factory = locator => new TImplementation()
                };

                _services[typeof(TInterface)] = registration;
                
                Debug.Log($"[ServiceLocator] Registered Scoped: {typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
            }
        }

        public void RegisterFactory<TInterface>(Func<IServiceLocator, TInterface> factory) 
            where TInterface : class
        {
            lock (_lock)
            {
                var registration = new ServiceRegistration
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = typeof(TInterface),
                    Lifetime = ServiceLifetime.Transient, // Factories are transient by default
                    Factory = locator => factory(locator)
                };

                _services[typeof(TInterface)] = registration;
                
                Debug.Log($"[ServiceLocator] Registered Factory: {typeof(TInterface).Name}");
            }
        }

        #endregion

        #region Service Resolution

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public T Resolve<T>(T fallback) where T : class
        {
            try
            {
                return Resolve<T>() ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        public T TryResolve<T>() where T : class
        {
            try
            {
                return Resolve<T>();
            }
            catch
            {
                return null;
            }
        }

        private object Resolve(Type serviceType)
        {
            lock (_lock)
            {
                _totalResolutions++;
                UpdateResolutionCount(serviceType);

                if (!_services.TryGetValue(serviceType, out var registration))
                {
                    throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered");
                }

                return CreateServiceInstance(registration);
            }
        }

        private object CreateServiceInstance(ServiceRegistration registration)
        {
            switch (registration.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return GetOrCreateSingleton(registration);

                case ServiceLifetime.Transient:
                    return registration.Factory(this);

                case ServiceLifetime.Scoped:
                    // For now, treat scoped as singleton until proper scope implementation
                    return GetOrCreateSingleton(registration);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object GetOrCreateSingleton(ServiceRegistration registration)
        {
            if (registration.Instance != null)
            {
                _cacheHits++;
                return registration.Instance;
            }

            if (_singletonInstances.TryGetValue(registration.ServiceType, out var existingInstance))
            {
                _cacheHits++;
                return existingInstance;
            }

            var newInstance = registration.Factory(this);
            _singletonInstances[registration.ServiceType] = newInstance;
            registration.Instance = newInstance;

            return newInstance;
        }

        #endregion

        #region Service Queries

        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type serviceType)
        {
            lock (_lock)
            {
                return _services.ContainsKey(serviceType);
            }
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            lock (_lock)
            {
                var serviceType = typeof(T);
                return _services.Values
                    .Where(r => serviceType.IsAssignableFrom(r.ImplementationType))
                    .Select(r => (T)CreateServiceInstance(r))
                    .ToList();
            }
        }

        public IDictionary<Type, ServiceRegistration> GetRegistrations()
        {
            lock (_lock)
            {
                return new Dictionary<Type, ServiceRegistration>(_services);
            }
        }

        #endregion

        #region Scope Management

        public IServiceScope CreateScope()
        {
            var scope = new ServiceScope(this);
            lock (_lock)
            {
                _activeScopes.Add(scope);
            }
            return scope;
        }

        internal void RemoveScope(IServiceScope scope)
        {
            lock (_lock)
            {
                _activeScopes.Remove(scope);
            }
        }

        #endregion

        #region Lifecycle Management

        public void Clear()
        {
            lock (_lock)
            {
                // Dispose all singleton instances that implement IDisposable
                foreach (var instance in _singletonInstances.Values.OfType<IDisposable>())
                {
                    try
                    {
                        instance.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ServiceLocator] Error disposing service: {ex.Message}");
                    }
                }

                // Dispose all active scopes
                foreach (var scope in _activeScopes.ToList())
                {
                    try
                    {
                        scope.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ServiceLocator] Error disposing scope: {ex.Message}");
                    }
                }

                _services.Clear();
                _singletonInstances.Clear();
                _activeScopes.Clear();
                _resolutionCounts.Clear();
                _totalResolutions = 0;
                _cacheHits = 0;

                Debug.Log("[ServiceLocator] All services cleared");
            }
        }

        /// <summary>
        /// Get performance metrics for debugging
        /// </summary>
        public ServiceLocatorMetrics GetMetrics()
        {
            lock (_lock)
            {
                return new ServiceLocatorMetrics
                {
                    TotalResolutions = _totalResolutions,
                    CacheHits = _cacheHits,
                    CacheHitRate = _totalResolutions > 0 ? (float)_cacheHits / _totalResolutions : 0f,
                    RegisteredServices = _services.Count,
                    SingletonInstances = _singletonInstances.Count,
                    ActiveScopes = _activeScopes.Count,
                    ResolutionCounts = new Dictionary<Type, int>(_resolutionCounts)
                };
            }
        }

        #endregion

        #region Core Services Registration

        private void RegisterCoreServices()
        {
            // Register self
            RegisterSingleton<IServiceLocator>(this);
            
            Debug.Log("[ServiceLocator] Core services registered");
        }

        #endregion

        #region Utility Methods

        private void UpdateResolutionCount(Type serviceType)
        {
            if (_resolutionCounts.ContainsKey(serviceType))
            {
                _resolutionCounts[serviceType]++;
            }
            else
            {
                _resolutionCounts[serviceType] = 1;
            }
        }

        #endregion
    }

    /// <summary>
    /// Service scope implementation
    /// </summary>
    internal class ServiceScope : IServiceScope
    {
        private readonly ServiceLocator _parentLocator;
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();
        private bool _disposed = false;

        public IServiceProvider ServiceProvider => new ServiceLocatorProviderAdapter(_parentLocator);

        internal ServiceScope(ServiceLocator parentLocator)
        {
            _parentLocator = parentLocator;
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Dispose all scoped instances
            foreach (var instance in _scopedInstances.Values.OfType<IDisposable>())
            {
                try
                {
                    instance.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceScope] Error disposing scoped service: {ex.Message}");
                }
            }

            _scopedInstances.Clear();
            _parentLocator.RemoveScope(this);
            _disposed = true;
        }
    }

    /// <summary>
    /// Service locator performance metrics
    /// </summary>
    public class ServiceLocatorMetrics
    {
        public int TotalResolutions { get; set; }
        public int CacheHits { get; set; }
        public float CacheHitRate { get; set; }
        public int RegisteredServices { get; set; }
        public int SingletonInstances { get; set; }
        public int ActiveScopes { get; set; }
        public Dictionary<Type, int> ResolutionCounts { get; set; }
    }

    /// <summary>
    /// Adapter that wraps IServiceLocator to provide IServiceProvider functionality
    /// </summary>
    internal class ServiceLocatorProviderAdapter : IServiceProvider
    {
        private readonly IServiceLocator _serviceLocator;

        public ServiceLocatorProviderAdapter(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
        }

        public object GetService(Type serviceType)
        {
            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.TryResolve))?.MakeGenericMethod(serviceType);
                return method?.Invoke(_serviceLocator, new object[0]);
            }
            catch
            {
                return null;
            }
        }

        public T GetService<T>()
        {
            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.TryResolve))?.MakeGenericMethod(typeof(T));
                var result = method?.Invoke(_serviceLocator, new object[0]);
                return result != null ? (T)result : default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public object GetRequiredService(Type serviceType)
        {
            var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.Resolve), new Type[0])?.MakeGenericMethod(serviceType);
            var result = method?.Invoke(_serviceLocator, new object[0]);
            if (result == null)
            {
                throw new InvalidOperationException($"Required service of type {serviceType.Name} could not be resolved");
            }
            return result;
        }

        public T GetRequiredService<T>()
        {
            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.Resolve), new Type[0])?.MakeGenericMethod(typeof(T));
                var result = method?.Invoke(_serviceLocator, new object[0]);
                if (result == null)
                {
                    throw new InvalidOperationException($"Required service of type {typeof(T).Name} could not be resolved");
                }
                return (T)result;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"Required service of type {typeof(T).Name} could not be resolved", ex);
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.ResolveAll))?.MakeGenericMethod(serviceType);
                var result = method?.Invoke(_serviceLocator, new object[0]);
                return result as IEnumerable<object> ?? new object[0];
            }
            catch
            {
                return new object[0];
            }
        }

        public IEnumerable<T> GetServices<T>()
        {
            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.ResolveAll))?.MakeGenericMethod(typeof(T));
                var result = method?.Invoke(_serviceLocator, new object[0]);
                return result as IEnumerable<T> ?? new T[0];
            }
            catch
            {
                return new T[0];
            }
        }
    }
}