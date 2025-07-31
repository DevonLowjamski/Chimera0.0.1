using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Service registry and dependency injection container for gaming services.
    /// Part of Module 1: Gaming Experience Core - Deliverable 1.3
    /// 
    /// Provides centralized registration and discovery of gaming services,
    /// ensuring proper initialization order and dependency resolution.
    /// </summary>
    public class GamingServiceRegistry : MonoBehaviour, IGamingServiceRegistry
    {
        [Header("Service Registry Configuration")]
        [SerializeField] private bool _enableAutoDiscovery = true;
        [SerializeField] private bool _enableDependencyValidation = true;
        [SerializeField] private bool _enableServiceDebugging = false;
        [SerializeField] private float _serviceHealthCheckInterval = 30.0f;
        
        [Header("Gaming Services")]
        [SerializeField] private GameEventBus _gameEventBus;
        [SerializeField] private ServiceEventIntegration _serviceEventIntegration;
        
        // Service registry
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly List<IGamingService> _managedServices = new List<IGamingService>();
        private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors = new Dictionary<Type, ServiceDescriptor>();
        
        // Health monitoring
        private float _lastHealthCheck;
        private ServiceHealthMonitor _healthMonitor;
        
        // Singleton instance
        private static GamingServiceRegistry _instance;
        public static GamingServiceRegistry Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<GamingServiceRegistry>();
                return _instance;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServiceRegistry();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            RegisterCoreServices();
            DiscoverAndRegisterServices();
            InitializeServices();
            ValidateServiceDependencies();
        }
        
        private void Update()
        {
            if (Time.time - _lastHealthCheck >= _serviceHealthCheckInterval)
            {
                PerformServiceHealthCheck();
                _lastHealthCheck = Time.time;
            }
        }
        
        private void OnDestroy()
        {
            ShutdownServices();
        }
        
        #endregion
        
        #region IGamingServiceRegistry Implementation
        
        public void RegisterService<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[GamingServiceRegistry] Cannot register null service of type {typeof(T).Name}");
                return;
            }
            
            var serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"[GamingServiceRegistry] Service {serviceType.Name} is already registered. Replacing existing service.");
            }
            
            _services[serviceType] = service;
            
            // If service implements IGamingService, add to managed services
            if (service is IGamingService gamingService)
            {
                if (!_managedServices.Contains(gamingService))
                {
                    _managedServices.Add(gamingService);
                }
            }
            
            // Create service descriptor
            _serviceDescriptors[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                Instance = service,
                RegistrationTime = DateTime.UtcNow,
                IsInitialized = false
            };
            
            if (_enableServiceDebugging)
            {
                Debug.Log($"[GamingServiceRegistry] Registered service: {serviceType.Name}");
            }
        }
        
        public T GetService<T>() where T : class
        {
            var serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }
            
            // Try to find service by interface
            foreach (var kvp in _services)
            {
                if (serviceType.IsAssignableFrom(kvp.Key))
                {
                    return kvp.Value as T;
                }
            }
            
            if (_enableServiceDebugging)
            {
                Debug.LogWarning($"[GamingServiceRegistry] Service not found: {serviceType.Name}");
            }
            
            return null;
        }
        
        public bool HasService<T>() where T : class
        {
            var serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
                return true;
                
            // Check if any registered service implements the interface
            foreach (var kvp in _services)
            {
                if (serviceType.IsAssignableFrom(kvp.Key))
                    return true;
            }
            
            return false;
        }
        
        public void UnregisterService<T>() where T : class
        {
            var serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out var service))
            {
                // Remove from managed services if applicable
                if (service is IGamingService gamingService)
                {
                    _managedServices.Remove(gamingService);
                    
                    // Shutdown the service
                    try
                    {
                        gamingService.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GamingServiceRegistry] Error shutting down service {serviceType.Name}: {ex.Message}");
                    }
                }
                
                _services.Remove(serviceType);
                _serviceDescriptors.Remove(serviceType);
                
                if (_enableServiceDebugging)
                {
                    Debug.Log($"[GamingServiceRegistry] Unregistered service: {serviceType.Name}");
                }
            }
        }
        
        public ServiceRegistryStatus GetRegistryStatus()
        {
            var status = new ServiceRegistryStatus
            {
                TotalServicesRegistered = _services.Count,
                ManagedServicesCount = _managedServices.Count,
                InitializedServicesCount = 0,
                HealthyServicesCount = 0,
                LastHealthCheckTime = _lastHealthCheck
            };
            
            // Count initialized and healthy services
            foreach (var descriptor in _serviceDescriptors.Values)
            {
                if (descriptor.IsInitialized)
                    status.InitializedServicesCount++;
                    
                if (descriptor.IsHealthy)
                    status.HealthyServicesCount++;
            }
            
            return status;
        }
        
        #endregion
        
        #region Service Management
        
        /// <summary>
        /// Initialize the service registry
        /// </summary>
        private void InitializeServiceRegistry()
        {
            _healthMonitor = new ServiceHealthMonitor();
            _lastHealthCheck = Time.time;
            
            Debug.Log("[GamingServiceRegistry] Service registry initialized");
        }
        
        /// <summary>
        /// Register core gaming services
        /// </summary>
        private void RegisterCoreServices()
        {
            // Register GameEventBus
            if (_gameEventBus != null)
            {
                RegisterService<IGameEventBus>(_gameEventBus);
                RegisterService<GameEventBus>(_gameEventBus);
            }
            else
            {
                Debug.LogWarning("[GamingServiceRegistry] GameEventBus not assigned!");
            }
            
            // Register ServiceEventIntegration
            if (_serviceEventIntegration != null)
            {
                RegisterService<IServiceEventIntegration>(_serviceEventIntegration);
                RegisterService<ServiceEventIntegration>(_serviceEventIntegration);
            }
            else
            {
                Debug.LogWarning("[GamingServiceRegistry] ServiceEventIntegration not assigned!");
            }
            
            // Register this registry itself
            RegisterService<IGamingServiceRegistry>(this);
        }
        
        /// <summary>
        /// Discover and register services automatically
        /// </summary>
        private void DiscoverAndRegisterServices()
        {
            if (!_enableAutoDiscovery) return;
            
            // Find all gaming services in the scene
            var allComponents = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var component in allComponents)
            {
                // Check if component implements gaming service interfaces
                if (component is IGamingService gamingService)
                {
                    var interfaces = component.GetType().GetInterfaces();
                    
                    foreach (var interfaceType in interfaces)
                    {
                        if (interfaceType != typeof(IGamingService) && 
                            typeof(IGamingService).IsAssignableFrom(interfaceType))
                        {
                            // Register by interface type
                            if (!_services.ContainsKey(interfaceType))
                            {
                                _services[interfaceType] = component;
                                
                                if (_enableServiceDebugging)
                                {
                                    Debug.Log($"[GamingServiceRegistry] Auto-discovered service: {interfaceType.Name} -> {component.GetType().Name}");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Initialize all managed services
        /// </summary>
        private void InitializeServices()
        {
            Debug.Log($"[GamingServiceRegistry] Initializing {_managedServices.Count} managed services...");
            
            foreach (var service in _managedServices)
            {
                try
                {
                    if (!service.IsInitialized)
                    {
                        service.Initialize();
                        
                        // Update service descriptor
                        var serviceType = service.GetType();
                        if (_serviceDescriptors.TryGetValue(serviceType, out var descriptor))
                        {
                            descriptor.IsInitialized = true;
                            descriptor.InitializationTime = DateTime.UtcNow;
                        }
                        
                        if (_enableServiceDebugging)
                        {
                            Debug.Log($"[GamingServiceRegistry] Initialized service: {serviceType.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GamingServiceRegistry] Failed to initialize service {service.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Validate service dependencies
        /// </summary>
        private void ValidateServiceDependencies()
        {
            if (!_enableDependencyValidation) return;
            
            bool allDependenciesMet = true;
            
            // Check core dependencies
            if (!HasService<IGameEventBus>())
            {
                Debug.LogError("[GamingServiceRegistry] Missing required service: IGameEventBus");
                allDependenciesMet = false;
            }
            
            if (!HasService<IServiceEventIntegration>())
            {
                Debug.LogError("[GamingServiceRegistry] Missing required service: IServiceEventIntegration");
                allDependenciesMet = false;
            }
            
            if (allDependenciesMet)
            {
                Debug.Log("[GamingServiceRegistry] All service dependencies validated successfully");
            }
            else
            {
                Debug.LogWarning("[GamingServiceRegistry] Some service dependencies are missing - gaming features may not work correctly");
            }
        }
        
        /// <summary>
        /// Perform health check on all services
        /// </summary>
        private void PerformServiceHealthCheck()
        {
            _healthMonitor.PerformHealthCheck(_managedServices, _serviceDescriptors);
            
            if (_enableServiceDebugging)
            {
                var status = GetRegistryStatus();
                Debug.Log($"[GamingServiceRegistry] Health check complete - {status.HealthyServicesCount}/{status.TotalServicesRegistered} services healthy");
            }
        }
        
        /// <summary>
        /// Shutdown all services
        /// </summary>
        private void ShutdownServices()
        {
            Debug.Log($"[GamingServiceRegistry] Shutting down {_managedServices.Count} managed services...");
            
            // Shutdown in reverse order
            for (int i = _managedServices.Count - 1; i >= 0; i--)
            {
                var service = _managedServices[i];
                try
                {
                    service.Shutdown();
                    
                    if (_enableServiceDebugging)
                    {
                        Debug.Log($"[GamingServiceRegistry] Shutdown service: {service.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GamingServiceRegistry] Error shutting down service {service.GetType().Name}: {ex.Message}");
                }
            }
            
            _services.Clear();
            _managedServices.Clear();
            _serviceDescriptors.Clear();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for gaming service registry
    /// </summary>
    public interface IGamingServiceRegistry
    {
        void RegisterService<T>(T service) where T : class;
        T GetService<T>() where T : class;
        bool HasService<T>() where T : class;
        void UnregisterService<T>() where T : class;
        ServiceRegistryStatus GetRegistryStatus();
    }
    
    /// <summary>
    /// Base interface for all gaming services
    /// </summary>
    public interface IGamingService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }
    
    /// <summary>
    /// Service descriptor for registry management
    /// </summary>
    internal class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public object Instance { get; set; }
        public DateTime RegistrationTime { get; set; }
        public DateTime InitializationTime { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsHealthy { get; set; } = true;
        public string LastError { get; set; }
        public DateTime LastHealthCheck { get; set; }
    }
    
    /// <summary>
    /// Service registry status information
    /// </summary>
    [Serializable]
    public struct ServiceRegistryStatus
    {
        public int TotalServicesRegistered;
        public int ManagedServicesCount;
        public int InitializedServicesCount;
        public int HealthyServicesCount;
        public float LastHealthCheckTime;
        
        public bool IsHealthy => HealthyServicesCount == TotalServicesRegistered;
        public float HealthPercentage => TotalServicesRegistered > 0 ? (float)HealthyServicesCount / TotalServicesRegistered : 0f;
    }
    
    /// <summary>
    /// Service health monitoring utility
    /// </summary>
    internal class ServiceHealthMonitor
    {
        public void PerformHealthCheck(List<IGamingService> services, Dictionary<Type, ServiceDescriptor> descriptors)
        {
            foreach (var service in services)
            {
                var serviceType = service.GetType();
                
                if (descriptors.TryGetValue(serviceType, out var descriptor))
                {
                    try
                    {
                        // Basic health check - ensure service is still initialized
                        descriptor.IsHealthy = service.IsInitialized;
                        descriptor.LastHealthCheck = DateTime.UtcNow;
                        descriptor.LastError = null;
                    }
                    catch (Exception ex)
                    {
                        descriptor.IsHealthy = false;
                        descriptor.LastError = ex.Message;
                        Debug.LogError($"[ServiceHealthMonitor] Health check failed for {serviceType.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}