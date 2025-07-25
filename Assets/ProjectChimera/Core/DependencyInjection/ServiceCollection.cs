using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Microsoft-style service collection implementation
    /// Provides fluent API for service registration compatible with standard .NET DI patterns
    /// </summary>
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> _services = new List<ServiceDescriptor>();
        private readonly IServiceContainer _container;

        public ServiceCollection() : this(null) { }

        public ServiceCollection(IServiceContainer container)
        {
            _container = container;
        }

        #region IList<ServiceDescriptor> Implementation

        public ServiceDescriptor this[int index]
        {
            get => _services[index];
            set => _services[index] = value;
        }

        public int Count => _services.Count;

        public bool IsReadOnly => false;

        public void Add(ServiceDescriptor item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _services.Add(item);
        }

        public void Clear()
        {
            _services.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return _services.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _services.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return _services.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _services.Insert(index, item);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return _services.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _services.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IServiceCollection Implementation

        public IServiceCollection Add(params ServiceDescriptor[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
            return this;
        }

        public IServiceCollection AddSingleton<TService>() where TService : class
        {
            return AddSingleton<TService, TService>();
        }

        public IServiceCollection AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Add(ServiceDescriptor.Singleton<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddSingleton<TService>(TService instance) where TService : class
        {
            Add(ServiceDescriptor.Singleton(instance));
            return this;
        }

        public IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class
        {
            Add(ServiceDescriptor.Singleton(factory));
            return this;
        }

        public IServiceCollection AddTransient<TService>() where TService : class
        {
            return AddTransient<TService, TService>();
        }

        public IServiceCollection AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Add(ServiceDescriptor.Transient<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class
        {
            Add(ServiceDescriptor.Transient(factory));
            return this;
        }

        public IServiceCollection AddScoped<TService>() where TService : class
        {
            return AddScoped<TService, TService>();
        }

        public IServiceCollection AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Add(ServiceDescriptor.Scoped<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddScoped<TService>(Func<IServiceProvider, TService> factory) where TService : class
        {
            Add(ServiceDescriptor.Scoped(factory));
            return this;
        }

        public IServiceCollection Replace<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            // Remove existing registrations
            var toRemove = _services.Where(s => s.ServiceType == typeof(TService)).ToList();
            foreach (var service in toRemove)
            {
                _services.Remove(service);
            }

            // Add new registration
            AddTransient<TService, TImplementation>();
            return this;
        }

        public bool Remove<TService>() where TService : class
        {
            var toRemove = _services.Where(s => s.ServiceType == typeof(TService)).ToList();
            var removed = false;

            foreach (var service in toRemove)
            {
                if (_services.Remove(service))
                {
                    removed = true;
                }
            }

            return removed;
        }

        public IServiceProvider BuildServiceProvider()
        {
            return BuildServiceProvider(new ServiceProviderOptions());
        }

        public IServiceProvider BuildServiceProvider(ServiceProviderOptions options)
        {
            if (options == null)
                options = new ServiceProviderOptions();

            var container = _container ?? new ServiceContainer();
            
            // Register all services with the container
            foreach (var descriptor in _services)
            {
                RegisterWithContainer(container, descriptor);
            }

            // Create and return the service provider adapter
            var serviceProvider = new ServiceProviderAdapter(container, options);

            if (options.ValidateOnBuild)
            {
                var result = container.Verify();
                if (!result.IsValid)
                {
                    var errorMessage = $"Service provider validation failed with {result.Errors.Count} errors:\n" +
                                     string.Join("\n", result.Errors);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            return serviceProvider;
        }

        #endregion

        #region Private Helpers

        private void RegisterWithContainer(IServiceContainer container, ServiceDescriptor descriptor)
        {
            try
            {
                if (descriptor.ImplementationInstance != null)
                {
                    // Register instance
                    var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.RegisterSingleton), new[] { descriptor.ServiceType });
                    method?.Invoke(container, new[] { descriptor.ImplementationInstance });
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    // Register factory - convert IServiceProvider factory to IServiceLocator factory
                    var serviceProviderAdapter = new ServiceProviderAdapter(container as ServiceContainer, new ServiceProviderOptions());
                    Func<IServiceLocator, object> locatorFactory = locator => descriptor.ImplementationFactory(serviceProviderAdapter);
                    
                    var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.RegisterFactory))?.MakeGenericMethod(descriptor.ServiceType);
                    method?.Invoke(container, new object[] { locatorFactory });
                }
                else if (descriptor.ImplementationType != null)
                {
                    // Register type mapping
                    switch (descriptor.Lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            RegisterByLifetime(container, "RegisterSingleton", descriptor.ServiceType, descriptor.ImplementationType);
                            break;
                        case ServiceLifetime.Transient:
                            RegisterByLifetime(container, "RegisterTransient", descriptor.ServiceType, descriptor.ImplementationType);
                            break;
                        case ServiceLifetime.Scoped:
                            RegisterByLifetime(container, "RegisterScoped", descriptor.ServiceType, descriptor.ImplementationType);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ServiceCollection] Error registering service {descriptor.ServiceType?.Name}: {ex.Message}");
                throw;
            }
        }

        private void RegisterByLifetime(IServiceContainer container, string methodName, Type serviceType, Type implementationType)
        {
            var method = typeof(IServiceLocator).GetMethod(methodName, new Type[0])?.MakeGenericMethod(serviceType, implementationType);
            method?.Invoke(container, new object[0]);
        }

        #endregion
    }

    /// <summary>
    /// Service provider adapter that bridges IServiceProvider with IServiceContainer
    /// </summary>
    public class ServiceProviderAdapter : IServiceProvider, IServiceScopeFactory, IDisposable
    {
        private readonly IServiceContainer _container;
        private readonly ServiceProviderOptions _options;
        private readonly List<IServiceScope> _activeScopes = new List<IServiceScope>();
        private bool _disposed = false;

        public ServiceProviderAdapter(IServiceContainer container, ServiceProviderOptions options)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _options = options ?? new ServiceProviderOptions();
        }

        #region IServiceProvider Implementation

        public object GetService(Type serviceType)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProviderAdapter));

            try
            {
                if (_options.EnableLogging)
                {
                    Debug.Log($"[ServiceProviderAdapter] Resolving service: {serviceType.Name}");
                }

                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.TryResolve))?.MakeGenericMethod(serviceType);
                return method?.Invoke(_container, new object[0]);
            }
            catch (Exception ex)
            {
                if (_options.EnableLogging)
                {
                    Debug.LogError($"[ServiceProviderAdapter] Error resolving {serviceType.Name}: {ex.Message}");
                }
                return null;
            }
        }

        public T GetService<T>()
        {
            var service = GetService(typeof(T));
            return service != null ? (T)service : default(T);
        }

        public object GetRequiredService(Type serviceType)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProviderAdapter));

            try
            {
                if (_options.EnableLogging)
                {
                    Debug.Log($"[ServiceProviderAdapter] Resolving required service: {serviceType.Name}");
                }

                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.Resolve), new[] { typeof(Type) });
                return method?.Invoke(_container, new object[] { serviceType });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Required service of type {serviceType.Name} could not be resolved", ex);
            }
        }

        public T GetRequiredService<T>()
        {
            var service = GetRequiredService(typeof(T));
            return (T)service;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProviderAdapter));

            try
            {
                var method = typeof(IServiceLocator).GetMethod(nameof(IServiceLocator.ResolveAll))?.MakeGenericMethod(serviceType);
                var result = method?.Invoke(_container, new object[0]);
                return result as IEnumerable<object> ?? new object[0];
            }
            catch (Exception ex)
            {
                if (_options.EnableLogging)
                {
                    Debug.LogError($"[ServiceProviderAdapter] Error resolving multiple services of type {serviceType.Name}: {ex.Message}");
                }
                return new object[0];
            }
        }

        public IEnumerable<T> GetServices<T>()
        {
            var services = GetServices(typeof(T));
            return services.Cast<T>();
        }

        #endregion

        #region IServiceScopeFactory Implementation

        public IServiceScope CreateScope()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProviderAdapter));

            var containerScope = _container.CreateScope();
            var scope = new ServiceScopeAdapter(containerScope, this);
            
            lock (_activeScopes)
            {
                _activeScopes.Add(scope);
            }

            return scope;
        }

        #endregion

        #region Internal Methods

        internal void RemoveScope(IServiceScope scope)
        {
            lock (_activeScopes)
            {
                _activeScopes.Remove(scope);
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Dispose all active scopes
                lock (_activeScopes)
                {
                    foreach (var scope in _activeScopes.ToList())
                    {
                        try
                        {
                            scope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ServiceProviderAdapter] Error disposing scope: {ex.Message}");
                        }
                    }
                    _activeScopes.Clear();
                }

                // Dispose the container if it implements IDisposable
                if (_container is IDisposable disposableContainer)
                {
                    disposableContainer.Dispose();
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ServiceProviderAdapter] Error during disposal: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Service scope adapter that bridges IServiceScope with IServiceScope from ServiceContainer
    /// </summary>
    public class ServiceScopeAdapter : IServiceScope
    {
        private readonly IServiceScope _containerScope;
        private readonly ServiceProviderAdapter _parentProvider;
        private bool _disposed = false;

        public ServiceScopeAdapter(IServiceScope containerScope, ServiceProviderAdapter parentProvider)
        {
            _containerScope = containerScope ?? throw new ArgumentNullException(nameof(containerScope));
            _parentProvider = parentProvider ?? throw new ArgumentNullException(nameof(parentProvider));
        }

        public IServiceProvider ServiceProvider => new ServiceProviderAdapter(_containerScope.ServiceProvider as IServiceContainer, new ServiceProviderOptions());

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _containerScope?.Dispose();
                _parentProvider?.RemoveScope(this);
                _disposed = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ServiceScopeAdapter] Error during disposal: {ex.Message}");
            }
        }
    }
}