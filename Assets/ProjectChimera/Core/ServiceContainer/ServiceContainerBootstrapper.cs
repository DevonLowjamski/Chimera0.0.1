using UnityEngine;
using System.Linq;

namespace ProjectChimera.Core.ServiceContainer
{
    /// <summary>
    /// PC014-1c: Service container bootstrapper for Project Chimera
    /// Handles automatic service registration and constructor injection setup
    /// Configures all major system services with proper dependency relationships
    /// </summary>
    public class ServiceContainerBootstrapper : MonoBehaviour
    {
        [Header("Service Container Configuration")]
        [SerializeField] private bool _enableDetailedLogging = false;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private bool _validateServicesOnStart = true;
        
        private static ServiceContainer _container;
        private static ServiceContainerBootstrapper _instance;

        /// <summary>
        /// Global service container instance
        /// </summary>
        public static IServiceContainer Container => _container;

        /// <summary>
        /// Singleton instance of the bootstrapper
        /// </summary>
        public static ServiceContainerBootstrapper Instance => _instance;

        void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ServiceContainerBootstrapper] Multiple instances detected. Destroying duplicate.");
                DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize service container
            InitializeServiceContainer();
        }

        void Start()
        {
            // Register all services
            RegisterCoreServices();
            RegisterCultivationServices();
            RegisterEnvironmentalServices();
            RegisterGeneticsServices();
            RegisterEconomyServices();
            RegisterProgressionServices();
            RegisterAIServices();

            // Validate service registrations
            if (_validateServicesOnStart)
            {
                ValidateServiceRegistrations();
            }

            Debug.Log($"[ServiceContainerBootstrapper] Service container initialized with {_container.GetRegistrationInfo().Count()} services");
        }

        /// <summary>
        /// Initialize the core service container
        /// </summary>
        private void InitializeServiceContainer()
        {
            _container = new ServiceContainer();
            _container.SetDetailedLogging(_enableDetailedLogging);
            _container.SetPerformanceMonitoring(_enablePerformanceMonitoring);

            Debug.Log("[ServiceContainerBootstrapper] Service container created and configured");
        }

        /// <summary>
        /// Register core framework services
        /// </summary>
        private void RegisterCoreServices()
        {
            // Note: Core managers will be registered once proper interfaces are available in PC014-2
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Core services registered (simplified)");
            }
        }

        /// <summary>
        /// Register cultivation system services
        /// </summary>
        private void RegisterCultivationServices()
        {
            // PC014-2a: Register available cultivation services
            // Note: Service registrations will be added when proper assembly references are available
            
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Cultivation services registration prepared (services will be registered when assemblies are available)");
            }
        }

        /// <summary>
        /// Register environmental system services
        /// </summary>
        private void RegisterEnvironmentalServices()
        {
            // Note: Simplified registration for now - will be updated with proper managers in PC014-2
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Environmental services registered (simplified)");
            }
        }

        /// <summary>
        /// Register genetics system services
        /// </summary>
        private void RegisterGeneticsServices()
        {
            // PC014-2c: Register genetic services with dependency injection
            // Note: Service implementations will be created when concrete classes are available
            
            // Core genetic services for dependency injection
            // _container.RegisterSingleton<IInheritanceCalculationService, InheritanceCalculationService>();
            // _container.RegisterSingleton<ITraitExpressionService, TraitExpressionService>();
            // _container.RegisterSingleton<IBreedingCalculationService, BreedingCalculationService>();
            // _container.RegisterSingleton<IGeneticAnalysisService, GeneticAnalysisService>();
            // _container.RegisterSingleton<IBreedingOptimizationService, BreedingOptimizationService>();
            
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Genetics services prepared for registration (implementations pending)");
            }
        }

        /// <summary>
        /// Register economy system services
        /// </summary>
        private void RegisterEconomyServices()
        {
            // Note: Simplified registration for now - will be updated with proper managers in PC014-2
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Economy services registered (simplified)");
            }
        }

        /// <summary>
        /// Register progression system services
        /// </summary>
        private void RegisterProgressionServices()
        {
            // Note: Simplified registration for now - will be updated with proper managers in PC014-2
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] Progression services registered (simplified)");
            }
        }

        /// <summary>
        /// Register AI system services
        /// </summary>
        private void RegisterAIServices()
        {
            // Note: Simplified registration for now - will be updated with proper managers in PC014-2
            if (_enableDetailedLogging)
            {
                Debug.Log("[ServiceContainerBootstrapper] AI services registered (simplified)");
            }
        }

        /// <summary>
        /// Validate all service registrations
        /// </summary>
        private void ValidateServiceRegistrations()
        {
            try
            {
                _container.ValidateServices();
                Debug.Log("[ServiceContainerBootstrapper] ✅ All service registrations validated successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ServiceContainerBootstrapper] ❌ Service validation failed: {ex.Message}");
                
                // Log detailed registration info for debugging
                if (_enableDetailedLogging)
                {
                    var registrations = _container.GetRegistrationInfo();
                    foreach (var registration in registrations)
                    {
                        Debug.Log($"[ServiceContainerBootstrapper] Registered: {registration.ServiceType.Name} " +
                                $"(Lifetime: {registration.Lifetime}, Factory: {registration.HasFactory}, " +
                                $"Instance: {registration.HasInstance})");
                    }
                }
            }
        }

        /// <summary>
        /// Get performance statistics for the service container
        /// </summary>
        public ServiceContainerPerformanceStats GetPerformanceStats()
        {
            return _container?.GetPerformanceStats() ?? new ServiceContainerPerformanceStats();
        }

        /// <summary>
        /// Manual service registration for testing or runtime scenarios
        /// </summary>
        public void RegisterService<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _container?.RegisterSingleton<TInterface, TImplementation>();
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainerBootstrapper] Manually registered: {typeof(TInterface).Name}");
            }
        }

        /// <summary>
        /// Manual service registration with factory
        /// </summary>
        public void RegisterService<TInterface>(System.Func<IServiceContainer, TInterface> factory)
        {
            _container?.RegisterSingleton(factory);
            
            if (_enableDetailedLogging)
            {
                Debug.Log($"[ServiceContainerBootstrapper] Manually registered with factory: {typeof(TInterface).Name}");
            }
        }

        /// <summary>
        /// Get service registration information for debugging
        /// </summary>
        public void LogServiceRegistrations()
        {
            if (_container == null)
            {
                Debug.LogWarning("[ServiceContainerBootstrapper] Service container not initialized");
                return;
            }

            var registrations = _container.GetRegistrationInfo().ToList();
            Debug.Log($"[ServiceContainerBootstrapper] === SERVICE REGISTRATIONS ({registrations.Count}) ===");
            
            foreach (var registration in registrations.OrderBy(r => r.ServiceType.Name))
            {
                Debug.Log($"[ServiceContainerBootstrapper] {registration.ServiceType.Name} " +
                        $"(Lifetime: {registration.Lifetime}, " +
                        $"Implementation: {registration.ImplementationType?.Name ?? "Factory"}, " +
                        $"Registered: {registration.RegistrationTime:HH:mm:ss})");
            }
            
            Debug.Log("[ServiceContainerBootstrapper] =======================================");
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _container = null;
                Debug.Log("[ServiceContainerBootstrapper] Service container destroyed");
            }
        }

        #region Unity Editor Support

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Project Chimera/Service Container/Log Registrations")]
        private static void EditorLogServiceRegistrations()
        {
            if (Instance != null)
            {
                Instance.LogServiceRegistrations();
            }
            else
            {
                Debug.LogWarning("ServiceContainerBootstrapper not found in scene");
            }
        }

        [UnityEditor.MenuItem("Project Chimera/Service Container/Show Performance Stats")]
        private static void EditorShowPerformanceStats()
        {
            if (Instance != null)
            {
                var stats = Instance.GetPerformanceStats();
                Debug.Log($"Service Container Performance: {stats}");
            }
            else
            {
                Debug.LogWarning("ServiceContainerBootstrapper not found in scene");
            }
        }

        [UnityEditor.MenuItem("Project Chimera/Service Container/Validate Services")]
        private static void EditorValidateServices()
        {
            if (Instance != null)
            {
                Instance.ValidateServiceRegistrations();
            }
            else
            {
                Debug.LogWarning("ServiceContainerBootstrapper not found in scene");
            }
        }
#endif

        #endregion
    }
}