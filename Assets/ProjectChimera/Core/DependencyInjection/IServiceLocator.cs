using System;
using System.Collections.Generic;

namespace ProjectChimera.Core.DependencyInjection
{
    /// <summary>
    /// Core service locator interface for dependency injection system
    /// Provides unified service resolution for Project Chimera's modular architecture
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Register a service instance with singleton lifetime
        /// </summary>
        void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register a service instance with singleton lifetime using provided instance
        /// </summary>
        void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class;

        /// <summary>
        /// Register a service with transient lifetime (new instance each time)
        /// </summary>
        void RegisterTransient<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register a service with scoped lifetime (per scope/context)
        /// </summary>
        void RegisterScoped<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new();

        /// <summary>
        /// Register a service using a factory function
        /// </summary>
        void RegisterFactory<TInterface>(Func<IServiceLocator, TInterface> factory) 
            where TInterface : class;

        /// <summary>
        /// Resolve a service by type
        /// </summary>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolve a service by type with optional fallback
        /// </summary>
        T Resolve<T>(T fallback) where T : class;

        /// <summary>
        /// Try to resolve a service, returning null if not found
        /// </summary>
        T TryResolve<T>() where T : class;

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        bool IsRegistered<T>() where T : class;

        /// <summary>
        /// Check if a service is registered by type
        /// </summary>
        bool IsRegistered(Type serviceType);

        /// <summary>
        /// Get all registered services of a type
        /// </summary>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Create a new scope for scoped services
        /// </summary>
        IServiceScope CreateScope();

        /// <summary>
        /// Clear all registrations (for testing/cleanup)
        /// </summary>
        void Clear();

        /// <summary>
        /// Get service registration information for debugging
        /// </summary>
        IDictionary<Type, ServiceRegistration> GetRegistrations();
    }


    /// <summary>
    /// Service lifetime enumeration
    /// </summary>
    public enum ServiceLifetime
    {
        Singleton,
        Transient,
        Scoped
    }

    /// <summary>
    /// Service registration information
    /// </summary>
    public class ServiceRegistration
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Func<IServiceLocator, object> Factory { get; set; }
        public object Instance { get; set; }
        public DateTime RegistrationTime { get; set; } = DateTime.Now;
    }
}