using System;
using System.Collections.Generic;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Advanced service container interface providing sophisticated dependency management
    /// Extends basic service location with collections, decorators, and conditional registration
    /// </summary>
    public interface IServiceContainer : IServiceLocator, IDisposable
    {
        #region Advanced Registration

        /// <summary>
        /// Register multiple implementations of the same interface
        /// </summary>
        void RegisterCollection<TInterface>(params Type[] implementations) where TInterface : class;

        /// <summary>
        /// Register a service with a specific name/key
        /// </summary>
        void RegisterNamed<TInterface, TImplementation>(string name) 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register a service with conditional activation
        /// </summary>
        void RegisterConditional<TInterface, TImplementation>(Func<IServiceLocator, bool> condition) 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register a decorator for an existing service
        /// </summary>
        void RegisterDecorator<TInterface, TDecorator>() 
            where TDecorator : class, TInterface
            where TInterface : class;

        /// <summary>
        /// Register a service with initialization callback
        /// </summary>
        void RegisterWithCallback<TInterface, TImplementation>(Action<TImplementation> initializer) 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register an open generic type
        /// </summary>
        void RegisterOpenGeneric(Type serviceType, Type implementationType);

        #endregion

        #region Advanced Resolution

        /// <summary>
        /// Resolve a named service
        /// </summary>
        T ResolveNamed<T>(string name) where T : class;

        /// <summary>
        /// Resolve all implementations that match a condition
        /// </summary>
        IEnumerable<T> ResolveWhere<T>(Func<T, bool> predicate) where T : class;

        /// <summary>
        /// Try to resolve a service with fallback factory
        /// </summary>
        T ResolveOrCreate<T>(Func<T> factory) where T : class;

        /// <summary>
        /// Resolve the most recently registered implementation
        /// </summary>
        T ResolveLast<T>() where T : class;

        /// <summary>
        /// Resolve with specific lifetime override
        /// </summary>
        T ResolveWithLifetime<T>(ServiceLifetime lifetime) where T : class;

        #endregion

        #region Container Management

        /// <summary>
        /// Create a child container with inherited registrations
        /// </summary>
        IServiceContainer CreateChildContainer();

        /// <summary>
        /// Verify all registered services can be resolved
        /// </summary>
        ContainerVerificationResult Verify();

        /// <summary>
        /// Get detailed registration information
        /// </summary>
        IEnumerable<AdvancedServiceDescriptor> GetServiceDescriptors();

        /// <summary>
        /// Replace an existing registration
        /// </summary>
        void Replace<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Remove a service registration
        /// </summary>
        bool Unregister<T>() where T : class;

        /// <summary>
        /// Check if container has been disposed
        /// </summary>
        bool IsDisposed { get; }

        #endregion

        #region Event Notifications

        /// <summary>
        /// Raised when a service is registered
        /// </summary>
        event Action<AdvancedServiceDescriptor> ServiceRegistered;

        /// <summary>
        /// Raised when a service is resolved
        /// </summary>
        event Action<Type, object> ServiceResolved;

        /// <summary>
        /// Raised when service resolution fails
        /// </summary>
        event Action<Type, Exception> ResolutionFailed;

        #endregion
    }

    /// <summary>
    /// Detailed service descriptor with advanced metadata
    /// </summary>
    public class AdvancedServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public string Name { get; set; }
        public Func<IServiceLocator, object> Factory { get; set; }
        public Func<IServiceLocator, bool> Condition { get; set; }
        public object Instance { get; set; }
        public DateTime RegistrationTime { get; set; } = DateTime.Now;
        public int Priority { get; set; } = 0;
        public bool IsDecorator { get; set; } = false;
        public bool IsOpenGeneric { get; set; } = false;
        public string[] Tags { get; set; } = new string[0];
    }

    /// <summary>
    /// Container verification result
    /// </summary>
    public class ContainerVerificationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public TimeSpan VerificationTime { get; set; }
        public int TotalServices { get; set; }
        public int VerifiedServices { get; set; }
    }

    /// <summary>
    /// Service collection registration helper
    /// </summary>
    public class ServiceCollectionHelper
    {
        public Type ServiceType { get; set; }
        public List<Type> ImplementationTypes { get; set; } = new List<Type>();
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
    }

    /// <summary>
    /// Conditional service registration
    /// </summary>
    public class ConditionalRegistration
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public Func<IServiceLocator, bool> Condition { get; set; }
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
    }
}