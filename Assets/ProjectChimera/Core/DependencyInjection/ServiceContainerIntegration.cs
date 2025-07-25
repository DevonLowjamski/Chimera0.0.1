using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Integration adapter that bridges Microsoft-style IServiceProvider with our ServiceContainer
    /// Provides seamless interoperability between the two DI systems
    /// </summary>
    public class ServiceContainerIntegration
    {
        /// <summary>
        /// Convert IServiceProvider to IServiceContainer
        /// </summary>
        public static IServiceContainer ToServiceContainer(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (serviceProvider is ServiceProviderAdapter adapter)
            {
                return adapter.GetUnderlyingContainer();
            }

            // Create a wrapper that makes IServiceProvider behave like IServiceContainer
            return new ServiceProviderContainerAdapter(serviceProvider);
        }

        /// <summary>
        /// Convert IServiceContainer to IServiceProvider
        /// </summary>
        public static IServiceProvider ToServiceProvider(IServiceContainer serviceContainer, ServiceProviderOptions options = null)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));

            return new ServiceProviderAdapter(serviceContainer, options ?? new ServiceProviderOptions());
        }

        /// <summary>
        /// Create a unified DI system that supports both patterns
        /// </summary>
        public static UnifiedDIContainer CreateUnified()
        {
            return new UnifiedDIContainer();
        }

        /// <summary>
        /// Migrate services from IServiceProvider to IServiceContainer
        /// </summary>
        public static void MigrateServices(IServiceProvider source, IServiceContainer target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Debug.Log("[ServiceContainerIntegration] Migrating services from IServiceProvider to IServiceContainer");
            
            // This would require reflection to discover and migrate services
            // For now, we'll just log the intent
            Debug.LogWarning("[ServiceContainerIntegration] Service migration requires manual registration of services");
        }
    }

    /// <summary>
    /// Adapter that makes IServiceProvider behave like IServiceContainer
    /// </summary>
    internal class ServiceProviderContainerAdapter : IServiceContainer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _additionalServices = new Dictionary<Type, object>();

        public ServiceProviderContainerAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool IsDisposed => false;

        public event Action<AdvancedServiceDescriptor> ServiceRegistered;
        public event Action<Type, object> ServiceResolved;
        public event Action<Type, Exception> ResolutionFailed;

        #region Basic IServiceLocator Implementation

        public T Resolve<T>() where T : class
        {
            try
            {
                var service = _serviceProvider.GetRequiredService<T>();
                ServiceResolved?.Invoke(typeof(T), service);
                return service;
            }
            catch (Exception ex)
            {
                ResolutionFailed?.Invoke(typeof(T), ex);
                throw;
            }
        }

        public T Resolve<T>(T fallback) where T : class
        {
            try
            {
                return _serviceProvider.GetService<T>() ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        public T TryResolve<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public bool IsRegistered<T>() where T : class
        {
            return _serviceProvider.GetService<T>() != null;
        }

        public bool IsRegistered(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType) != null;
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _serviceProvider.GetServices<T>();
        }

        public IServiceScope CreateScope()
        {
            return _serviceProvider.CreateScope();
        }

        public void Clear()
        {
            _additionalServices.Clear();
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot clear services from underlying IServiceProvider");
        }

        public IDictionary<Type, ServiceRegistration> GetRegistrations()
        {
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot get registrations from underlying IServiceProvider");
            return new Dictionary<Type, ServiceRegistration>();
        }

        #endregion

        #region Registration Methods (Limited Support)

        public void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot register services on underlying IServiceProvider");
            throw new NotSupportedException("Cannot register services on IServiceProvider adapter");
        }

        public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            _additionalServices[typeof(TInterface)] = instance;
            ServiceRegistered?.Invoke(new AdvancedServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = instance.GetType(),
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance
            });
        }

        public void RegisterTransient<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot register services on underlying IServiceProvider");
            throw new NotSupportedException("Cannot register services on IServiceProvider adapter");
        }

        public void RegisterScoped<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot register services on underlying IServiceProvider");
            throw new NotSupportedException("Cannot register services on IServiceProvider adapter");
        }

        public void RegisterFactory<TInterface>(Func<IServiceLocator, TInterface> factory) where TInterface : class
        {
            Debug.LogWarning("[ServiceProviderContainerAdapter] Cannot register factories on underlying IServiceProvider");
            throw new NotSupportedException("Cannot register factories on IServiceProvider adapter");
        }

        #endregion

        #region Advanced IServiceContainer Methods (Not Supported)

        public void RegisterCollection<TInterface>(params Type[] implementations) where TInterface : class
        {
            throw new NotSupportedException("Advanced registration not supported on IServiceProvider adapter");
        }

        public void RegisterNamed<TInterface, TImplementation>(string name) 
            where TImplementation : class, TInterface, new()
        {
            throw new NotSupportedException("Named registration not supported on IServiceProvider adapter");
        }

        public void RegisterConditional<TInterface, TImplementation>(Func<IServiceLocator, bool> condition) 
            where TImplementation : class, TInterface, new()
        {
            throw new NotSupportedException("Conditional registration not supported on IServiceProvider adapter");
        }

        public void RegisterDecorator<TInterface, TDecorator>() 
            where TDecorator : class, TInterface where TInterface : class
        {
            throw new NotSupportedException("Decorator registration not supported on IServiceProvider adapter");
        }

        public void RegisterWithCallback<TInterface, TImplementation>(Action<TImplementation> initializer) 
            where TImplementation : class, TInterface, new()
        {
            throw new NotSupportedException("Callback registration not supported on IServiceProvider adapter");
        }

        public void RegisterOpenGeneric(Type serviceType, Type implementationType)
        {
            throw new NotSupportedException("Open generic registration not supported on IServiceProvider adapter");
        }

        public T ResolveNamed<T>(string name) where T : class
        {
            throw new NotSupportedException("Named resolution not supported on IServiceProvider adapter");
        }

        public IEnumerable<T> ResolveWhere<T>(Func<T, bool> predicate) where T : class
        {
            return _serviceProvider.GetServices<T>().Where(predicate);
        }

        public T ResolveOrCreate<T>(Func<T> factory) where T : class
        {
            var service = _serviceProvider.GetService<T>();
            return service ?? factory();
        }

        public T ResolveLast<T>() where T : class
        {
            return _serviceProvider.GetServices<T>().LastOrDefault();
        }

        public T ResolveWithLifetime<T>(ServiceLifetime lifetime) where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public IServiceContainer CreateChildContainer()
        {
            throw new NotSupportedException("Child containers not supported on IServiceProvider adapter");
        }

        public ContainerVerificationResult Verify()
        {
            return new ContainerVerificationResult
            {
                IsValid = true,
                TotalServices = 0,
                VerifiedServices = 0,
                VerificationTime = TimeSpan.Zero
            };
        }

        public IEnumerable<AdvancedServiceDescriptor> GetServiceDescriptors()
        {
            return new AdvancedServiceDescriptor[0];
        }

        public void Replace<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            throw new NotSupportedException("Service replacement not supported on IServiceProvider adapter");
        }

        public bool Unregister<T>() where T : class
        {
            return _additionalServices.Remove(typeof(T));
        }

        #endregion

        public void Dispose()
        {
            _additionalServices.Clear();
            
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Extension methods for ServiceProviderAdapter to access underlying container
    /// </summary>
    internal static class ServiceProviderAdapterExtensions
    {
        public static IServiceContainer GetUnderlyingContainer(this ServiceProviderAdapter adapter)
        {
            // Use reflection to access the private _container field
            var field = typeof(ServiceProviderAdapter).GetField("_container", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return field?.GetValue(adapter) as IServiceContainer;
        }
    }

    /// <summary>
    /// Unified DI container that supports both IServiceContainer and IServiceProvider patterns
    /// </summary>
    public class UnifiedDIContainer : IServiceContainer, IServiceProvider, IServiceScopeFactory
    {
        private readonly IServiceContainer _container;
        private readonly ServiceProviderAdapter _serviceProvider;

        public UnifiedDIContainer()
        {
            _container = new ServiceContainer();
            _serviceProvider = new ServiceProviderAdapter(_container, new ServiceProviderOptions());
        }

        public bool IsDisposed => _container.IsDisposed;

        #region Events

        public event Action<AdvancedServiceDescriptor> ServiceRegistered
        {
            add => _container.ServiceRegistered += value;
            remove => _container.ServiceRegistered -= value;
        }

        public event Action<Type, object> ServiceResolved
        {
            add => _container.ServiceResolved += value;
            remove => _container.ServiceResolved -= value;
        }

        public event Action<Type, Exception> ResolutionFailed
        {
            add => _container.ResolutionFailed += value;
            remove => _container.ResolutionFailed -= value;
        }

        #endregion

        #region IServiceContainer Implementation

        public T Resolve<T>() where T : class => _container.Resolve<T>();
        public T Resolve<T>(T fallback) where T : class => _container.Resolve(fallback);
        public T TryResolve<T>() where T : class => _container.TryResolve<T>();
        public bool IsRegistered<T>() where T : class => _container.IsRegistered<T>();
        public bool IsRegistered(Type serviceType) => _container.IsRegistered(serviceType);
        public IEnumerable<T> ResolveAll<T>() where T : class => _container.ResolveAll<T>();
        public IServiceScope CreateScope() => _container.CreateScope();
        public void Clear() => _container.Clear();
        public IDictionary<Type, ServiceRegistration> GetRegistrations() => _container.GetRegistrations();

        public void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterSingleton<TInterface, TImplementation>();

        public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class => 
            _container.RegisterSingleton(instance);

        public void RegisterTransient<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterTransient<TInterface, TImplementation>();

        public void RegisterScoped<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterScoped<TInterface, TImplementation>();

        public void RegisterFactory<TInterface>(Func<IServiceLocator, TInterface> factory) where TInterface : class => 
            _container.RegisterFactory(factory);

        public void RegisterCollection<TInterface>(params Type[] implementations) where TInterface : class => 
            _container.RegisterCollection<TInterface>(implementations);

        public void RegisterNamed<TInterface, TImplementation>(string name) 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterNamed<TInterface, TImplementation>(name);

        public void RegisterConditional<TInterface, TImplementation>(Func<IServiceLocator, bool> condition) 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterConditional<TInterface, TImplementation>(condition);

        public void RegisterDecorator<TInterface, TDecorator>() 
            where TDecorator : class, TInterface where TInterface : class => 
            _container.RegisterDecorator<TInterface, TDecorator>();

        public void RegisterWithCallback<TInterface, TImplementation>(Action<TImplementation> initializer) 
            where TImplementation : class, TInterface, new() => 
            _container.RegisterWithCallback<TInterface, TImplementation>(initializer);

        public void RegisterOpenGeneric(Type serviceType, Type implementationType) => 
            _container.RegisterOpenGeneric(serviceType, implementationType);

        public T ResolveNamed<T>(string name) where T : class => _container.ResolveNamed<T>(name);
        public IEnumerable<T> ResolveWhere<T>(Func<T, bool> predicate) where T : class => _container.ResolveWhere<T>(predicate);
        public T ResolveOrCreate<T>(Func<T> factory) where T : class => _container.ResolveOrCreate<T>(factory);
        public T ResolveLast<T>() where T : class => _container.ResolveLast<T>();
        public T ResolveWithLifetime<T>(ServiceLifetime lifetime) where T : class => _container.ResolveWithLifetime<T>(lifetime);
        public IServiceContainer CreateChildContainer() => _container.CreateChildContainer();
        public ContainerVerificationResult Verify() => _container.Verify();
        public IEnumerable<AdvancedServiceDescriptor> GetServiceDescriptors() => _container.GetServiceDescriptors();

        public void Replace<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new() => 
            _container.Replace<TInterface, TImplementation>();

        public bool Unregister<T>() where T : class => _container.Unregister<T>();

        #endregion

        #region IServiceProvider Implementation

        public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);
        public T GetService<T>() => _serviceProvider.GetService<T>();
        public object GetRequiredService(Type serviceType) => _serviceProvider.GetRequiredService(serviceType);
        public T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();
        public IEnumerable<object> GetServices(Type serviceType) => _serviceProvider.GetServices(serviceType);
        public IEnumerable<T> GetServices<T>() => _serviceProvider.GetServices<T>();

        #endregion

        #region IServiceScopeFactory Implementation

        IServiceScope IServiceScopeFactory.CreateScope() => _serviceProvider.CreateScope();

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            _container?.Dispose();
        }

        #endregion
    }
}