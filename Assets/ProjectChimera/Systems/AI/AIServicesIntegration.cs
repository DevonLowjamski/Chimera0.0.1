using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.DependencyInjection;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Gaming;
using ProjectChimera.Systems.Progression;
using AIEnvironmentalAnalysisResult = ProjectChimera.Data.AI.EnvironmentalAnalysisResult;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-6: AI Services Integration - Comprehensive DI and event-driven integration system
    /// Provides seamless integration of all AI advisor services through dependency injection
    /// and event-driven communication with the broader game ecosystem
    /// </summary>
    public class AIServicesIntegration : MonoBehaviour, IAIServicesIntegration
    {
        [Header("Integration Configuration")]
        [SerializeField] private IntegrationSettings _integrationConfig;
        [SerializeField] private bool _enableAutoServiceRegistration = true;
        [SerializeField] private bool _enableEventBusIntegration = true;
        [SerializeField] private float _serviceHealthCheckInterval = 30f;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        
        [Header("Service Lifecycle")]
        [SerializeField] private ServiceLifecycleMode _lifecycleMode = ServiceLifecycleMode.Automatic;
        [SerializeField] private float _initializationTimeout = 10f;
        [SerializeField] private int _maxInitializationRetries = 3;
        [SerializeField] private bool _enableGracefulShutdown = true;
        
        [Header("Event Configuration")]
        [SerializeField] private bool _enableCrossServiceEvents = true;
        [SerializeField] private int _maxEventQueueSize = 1000;
        [SerializeField] private float _eventProcessingTimeout = 1f;
        [SerializeField] private bool _enableEventMetrics = true;
        
        // Core dependencies
        private IServiceContainer _serviceContainer;
        private IGameEventBus _eventBus;
        private ServiceBootstrapper _bootstrapper;
        
        // Concrete implementations for tests
        private ServiceContainer _concreteServiceContainer;
        private GameEventBus _concreteEventBus;
        
        // AI Services
        private IAIAnalysisService _analysisService;
        private IAIRecommendationService _recommendationService;
        private IAIPersonalityService _personalityService;
        private IAILearningService _learningService;
        private IAIAdvisorCoordinator _coordinatorService;
        
        // Integration state
        private IntegrationStatus _integrationStatus = IntegrationStatus.NotInitialized;
        private ServiceHealthMonitor _healthMonitor;
        private EventBridge _eventBridge;
        private PerformanceTracker _performanceTracker;
        private Dictionary<Type, ServiceIntegrationInfo> _serviceIntegrations = new Dictionary<Type, ServiceIntegrationInfo>();
        
        // Event subscriptions
        private readonly List<EventSubscription> _eventSubscriptions = new List<EventSubscription>();
        private readonly Dictionary<string, int> _eventMetrics = new Dictionary<string, int>();
        
        // Timing tracking
        private TimeSpan _initializationTime;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public IntegrationStatus Status => _integrationStatus;
        public IServiceContainer ServiceContainer => _serviceContainer;
        public IGameEventBus EventBus => _eventBus;
        
        // Additional properties for test access to concrete implementations
        public ServiceContainer ConcreteServiceContainer => _concreteServiceContainer;
        public GameEventBus ConcreteEventBus => _concreteEventBus;
        public Dictionary<Type, ServiceIntegrationInfo> ServiceIntegrations => new Dictionary<Type, ServiceIntegrationInfo>(_serviceIntegrations);
        
        // Events
        public event Action<IntegrationStatus> OnIntegrationStatusChanged;
        public event Action<Type, ServiceStatus> OnServiceStatusChanged;
        public event Action<string, object> OnCrossServiceEvent;
        public event Action<IntegrationMetrics> OnMetricsUpdated;
        
        #region Initialization & Lifecycle
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            var initStartTime = DateTime.UtcNow;
            LogInfo("Initializing AI Services Integration...");
            _integrationStatus = IntegrationStatus.Initializing;
            OnIntegrationStatusChanged?.Invoke(_integrationStatus);
            
            try
            {
                InitializeDependencyInjection();
                InitializeEventBus();
                InitializeServiceRegistration();
                InitializeEventBridge();
                InitializeHealthMonitoring();
                InitializePerformanceTracking();
                
                RegisterAIServices();
                SetupEventSubscriptions();
                StartServices();
                
                _integrationStatus = IntegrationStatus.Running;
                IsInitialized = true;
                _initializationTime = DateTime.UtcNow - initStartTime;
                
                LogInfo("AI Services Integration initialized successfully");
                OnIntegrationStatusChanged?.Invoke(_integrationStatus);
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize AI Services Integration: {ex.Message}");
                _integrationStatus = IntegrationStatus.Failed;
                OnIntegrationStatusChanged?.Invoke(_integrationStatus);
                throw;
            }
        }
        
        private void InitializeDependencyInjection()
        {
            // Create concrete service container for tests
            _concreteServiceContainer = new ServiceContainer();
            
            // Get or create service container
            _serviceContainer = ServiceLocator.Instance.TryResolve<IServiceContainer>();
            if (_serviceContainer == null)
            {
                _serviceContainer = _concreteServiceContainer;
                ServiceLocator.Instance.RegisterSingleton<IServiceContainer>(_serviceContainer);
                LogInfo("Created new service container");
            }
            
            // Initialize bootstrapper
            _bootstrapper = ServiceBootstrapper.Instance;
            
            LogInfo("Dependency injection initialized");
        }
        
        private void InitializeEventBus()
        {
            // Find existing event bus or create one
            _eventBus = FindObjectOfType<GameEventBus>();
            if (_eventBus == null)
            {
                var eventBusObject = new GameObject("GameEventBus");
                _eventBus = eventBusObject.AddComponent<GameEventBus>();
                LogWarning("Created new GameEventBus - consider using existing instance");
            }
            
            // Set concrete event bus for test access
            _concreteEventBus = _eventBus as GameEventBus;
            if (_concreteEventBus == null)
            {
                // Create concrete instance if casting failed
                var eventBusObject = new GameObject("GameEventBus");
                _concreteEventBus = eventBusObject.AddComponent<GameEventBus>();
                _eventBus = _concreteEventBus;
            }
            
            // Register event bus in service container
            _serviceContainer.RegisterSingleton<IGameEventBus>(_eventBus);
            
            LogInfo("Event bus integration initialized");
        }
        
        private void InitializeServiceRegistration()
        {
            // Configure service registration options
            var registrationOptions = new ServiceRegistrationOptions
            {
                EnableAutoDiscovery = _enableAutoServiceRegistration,
                ValidateServices = true,
                TrackPerformance = _enablePerformanceMonitoring
            };
            
            LogInfo("Service registration configuration initialized");
        }
        
        private void InitializeEventBridge()
        {
            _eventBridge = new EventBridge(_eventBus, _maxEventQueueSize)
            {
                EnableMetrics = _enableEventMetrics,
                ProcessingTimeout = _eventProcessingTimeout
            };
            
            LogInfo("Event bridge initialized");
        }
        
        private void InitializeHealthMonitoring()
        {
            _healthMonitor = new ServiceHealthMonitor(_serviceHealthCheckInterval)
            {
                EnableAutoRecovery = true,
                MaxRecoveryAttempts = _maxInitializationRetries
            };
            
            _healthMonitor.OnServiceHealthChanged += (serviceType, isHealthy) =>
            {
                var status = isHealthy ? ServiceStatus.Healthy : ServiceStatus.Unhealthy;
                OnServiceStatusChanged?.Invoke(serviceType, status);
                LogInfo($"Service {serviceType.Name} health changed to: {status}");
            };
            
            LogInfo("Health monitoring initialized");
        }
        
        private void InitializePerformanceTracking()
        {
            if (!_enablePerformanceMonitoring) return;
            
            _performanceTracker = new PerformanceTracker
            {
                EnableDetailedMetrics = true,
                SampleInterval = 1f,
                MaxSamples = 1000
            };
            
            LogInfo("Performance tracking initialized");
        }
        
        #endregion
        
        #region Service Registration
        
        private void RegisterAIServices()
        {
            LogInfo("Registering AI services...");
            
            // Register Analysis Service
            RegisterService<IAIAnalysisService, AIAnalysisService>("AIAnalysisService");
            
            // Register Recommendation Service  
            RegisterService<IAIRecommendationService, AIRecommendationService>("AIRecommendationService");
            
            // Register Personality Service
            RegisterService<IAIPersonalityService, AIPersonalityService>("AIPersonalityService");
            
            // Register Learning Service
            RegisterService<IAILearningService, AILearningService>("AILearningService");
            
            // Register Coordinator Service
            RegisterService<IAIAdvisorCoordinator, AIAdvisorCoordinator>("AIAdvisorCoordinator");
            
            // Register supporting services
            RegisterSupportingServices();
            
            LogInfo($"Registered {_serviceIntegrations.Count} AI services");
        }
        
        private void RegisterService<TInterface, TImplementation>(string serviceName) 
            where TInterface : class
            where TImplementation : MonoBehaviour, TInterface
        {
            try
            {
                // Find existing instance or create new one
                var existingInstance = FindObjectOfType<TImplementation>();
                if (existingInstance != null)
                {
                    _serviceContainer.RegisterSingleton(existingInstance as TInterface);
                    LogInfo($"Registered existing {serviceName}");
                }
                else
                {
                    // Create new instance
                    var serviceObject = new GameObject(serviceName);
                    var serviceInstance = serviceObject.AddComponent<TImplementation>();
                    _serviceContainer.RegisterSingleton(serviceInstance as TInterface);
                    LogInfo($"Created and registered new {serviceName}");
                }
                
                // Track service integration
                var integrationInfo = new ServiceIntegrationInfo
                {
                    ServiceType = typeof(TInterface),
                    ImplementationType = typeof(TImplementation),
                    ServiceName = serviceName,
                    RegistrationTime = DateTime.UtcNow,
                    Status = ServiceStatus.Registered,
                    IsAutoManaged = true
                };
                
                _serviceIntegrations[typeof(TInterface)] = integrationInfo;
            }
            catch (Exception ex)
            {
                LogError($"Failed to register {serviceName}: {ex.Message}");
                throw;
            }
        }
        
        private void RegisterSupportingServices()
        {
            // Register progression service if available
            var progressionService = FindObjectOfType<MonoBehaviour>()?.GetComponent<IPlayerProgressionService>();
            if (progressionService != null)
            {
                _serviceContainer.RegisterSingleton<IPlayerProgressionService>(progressionService);
                LogInfo("Registered existing progression service");
            }
            
            // Register other supporting services as needed
            RegisterUtilityServices();
        }
        
        private void RegisterUtilityServices()
        {
            // Register utility services that AI services might need
            _serviceContainer.RegisterSingleton<ServiceHealthMonitor>(_healthMonitor);
            _serviceContainer.RegisterSingleton<EventBridge>(_eventBridge);
            
            if (_performanceTracker != null)
            {
                _serviceContainer.RegisterSingleton<PerformanceTracker>(_performanceTracker);
            }
        }
        
        #endregion
        
        #region Service Lifecycle Management
        
        private async void StartServices()
        {
            LogInfo("Starting AI services...");
            
            var servicesToStart = new List<(Type serviceType, object serviceInstance)>
            {
                (typeof(IAIAnalysisService), _serviceContainer.TryResolve<IAIAnalysisService>()),
                (typeof(IAIPersonalityService), _serviceContainer.TryResolve<IAIPersonalityService>()),
                (typeof(IAILearningService), _serviceContainer.TryResolve<IAILearningService>()),
                (typeof(IAIRecommendationService), _serviceContainer.TryResolve<IAIRecommendationService>()),
                (typeof(IAIAdvisorCoordinator), _serviceContainer.TryResolve<IAIAdvisorCoordinator>())
            };
            
            foreach (var (serviceType, serviceInstance) in servicesToStart)
            {
                if (serviceInstance != null)
                {
                    await StartServiceAsync(serviceType, serviceInstance);
                }
            }
            
            // Cache service references for easier access
            CacheServiceReferences();
            
            LogInfo("All AI services started");
        }
        
        private async Task StartServiceAsync(Type serviceType, object serviceInstance)
        {
            try
            {
                UpdateServiceStatus(serviceType, ServiceStatus.Starting);
                
                // Initialize the service
                if (serviceInstance is IAIAnalysisService analysisService)
                {
                    analysisService.Initialize();
                    await WaitForServiceInitialization(() => analysisService.IsInitialized);
                }
                else if (serviceInstance is IAIRecommendationService recommendationService)
                {
                    recommendationService.Initialize();
                    await WaitForServiceInitialization(() => recommendationService.IsInitialized);
                }
                else if (serviceInstance is IAIPersonalityService personalityService)
                {
                    personalityService.Initialize();
                    await WaitForServiceInitialization(() => personalityService.IsInitialized);
                }
                else if (serviceInstance is IAILearningService learningService)
                {
                    learningService.Initialize();
                    await WaitForServiceInitialization(() => learningService.IsInitialized);
                }
                else if (serviceInstance is IAIAdvisorCoordinator coordinatorService)
                {
                    coordinatorService.Initialize();
                    await WaitForServiceInitialization(() => coordinatorService.IsInitialized);
                }
                
                UpdateServiceStatus(serviceType, ServiceStatus.Running);
                LogInfo($"Service {serviceType.Name} started successfully");
            }
            catch (Exception ex)
            {
                UpdateServiceStatus(serviceType, ServiceStatus.Failed);
                LogError($"Failed to start service {serviceType.Name}: {ex.Message}");
                throw;
            }
        }
        
        private async Task WaitForServiceInitialization(Func<bool> isInitializedCheck)
        {
            var timeout = DateTime.UtcNow.AddSeconds(_initializationTimeout);
            
            while (!isInitializedCheck() && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100);
            }
            
            if (!isInitializedCheck())
            {
                throw new TimeoutException("Service initialization timed out");
            }
        }
        
        private void CacheServiceReferences()
        {
            _analysisService = _serviceContainer.TryResolve<IAIAnalysisService>();
            _recommendationService = _serviceContainer.TryResolve<IAIRecommendationService>();
            _personalityService = _serviceContainer.TryResolve<IAIPersonalityService>();
            _learningService = _serviceContainer.TryResolve<IAILearningService>();
            _coordinatorService = _serviceContainer.TryResolve<IAIAdvisorCoordinator>();
        }
        
        #endregion
        
        #region Event-Driven Communication
        
        private void SetupEventSubscriptions()
        {
            if (!_enableEventBusIntegration) return;
            
            LogInfo("Setting up event subscriptions...");
            
            // Subscribe to cross-service events
            SetupAnalysisServiceEvents();
            SetupRecommendationServiceEvents();
            SetupPersonalityServiceEvents();
            SetupLearningServiceEvents();
            SetupCoordinatorServiceEvents();
            
            // Subscribe to game events
            SetupGameEventSubscriptions();
            
            LogInfo($"Set up {_eventSubscriptions.Count} event subscriptions");
        }
        
        private void SetupAnalysisServiceEvents()
        {
            if (_analysisService is IAIAnalysisServiceExtended extendedAnalysis)
            {
                extendedAnalysis.OnCultivationAnalysisComplete += OnCultivationAnalysisComplete;
                extendedAnalysis.OnEnvironmentalAnalysisComplete += OnEnvironmentalAnalysisComplete;
                extendedAnalysis.OnGeneticsAnalysisComplete += OnGeneticsAnalysisComplete;
            }
        }
        
        private void SetupRecommendationServiceEvents()
        {
            if (_recommendationService != null)
            {
                _recommendationService.OnRecommendationCreated += OnRecommendationCreated;
                _recommendationService.OnRecommendationImplemented += OnRecommendationImplemented;
            }
        }
        
        private void SetupPersonalityServiceEvents()
        {
            if (_personalityService != null)
            {
                _personalityService.OnPersonalityChanged += OnPersonalityChanged;
                _personalityService.OnPlayerInteractionRecorded += OnPlayerInteractionRecorded;
            }
        }
        
        private void SetupLearningServiceEvents()
        {
            if (_learningService != null)
            {
                _learningService.OnModelTrained += OnModelTrained;
                _learningService.OnPredictionMade += OnPredictionMade;
            }
        }
        
        private void SetupCoordinatorServiceEvents()
        {
            if (_coordinatorService != null)
            {
                _coordinatorService.OnRequestCompleted += OnRequestCompleted;
                _coordinatorService.OnWorkflowCompleted += OnWorkflowCompleted;
            }
        }
        
        private void SetupGameEventSubscriptions()
        {
            // Subscribe to relevant game events that AI services should respond to
            _eventBridge.Subscribe<PlantGrowthEvent>("Game.PlantGrowth", OnPlantGrowthEvent);
            _eventBridge.Subscribe<PlantHarvestEvent>("Game.PlantHarvest", OnPlantHarvestEvent);
            _eventBridge.Subscribe<PlayerActionEvent>("Game.PlayerAction", OnPlayerActionEvent);
        }
        
        private void SubscribeToEvent<T>(string eventName, Action<T> handler)
        {
            var subscription = new EventSubscription
            {
                EventName = eventName,
                EventType = typeof(T),
                Handler = handler,
                SubscriptionTime = DateTime.UtcNow
            };
            
            _eventSubscriptions.Add(subscription);
            _eventBridge.Subscribe<T>(eventName, handler);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlantGrowthEvent(PlantGrowthEvent growthEvent)
        {
            LogInfo($"Received plant growth event: {growthEvent.PlantId}");
            
            // Trigger analysis if significant growth occurred
            if (growthEvent.GrowthAmount > 0.1f)
            {
                _coordinatorService?.ProcessRequestAsync(new CoordinationRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    WorkflowType = WorkflowType.FastRecommendation,
                    Priority = CoordinationPriority.Medium,
                    Data = growthEvent
                });
            }
            
            IncrementEventMetric("PlantGrowth");
        }
        
        private void OnPlantHarvestEvent(PlantHarvestEvent harvestEvent)
        {
            LogInfo($"Received plant harvest event: {harvestEvent.PlantId}");
            
            // Record learning data
            _learningService?.AddLearningRecord(
                "PlantHarvested",
                $"Plant: {harvestEvent.PlantId}, Quality: {harvestEvent.Quality}",
                "Harvest completed",
                harvestEvent.Quality / 100f
            );
            
            IncrementEventMetric("PlantHarvest");
        }
        
        private void OnPlayerActionEvent(PlayerActionEvent actionEvent)
        {
            LogInfo($"Received player action event: {actionEvent.ActionType}");
            
            // Record player interaction
            _personalityService?.RecordPlayerInteraction(
                actionEvent.ActionType,
                actionEvent.Context,
                actionEvent.Success ? 0.8f : 0.3f
            );
            
            IncrementEventMetric("PlayerAction");
        }
        
        #endregion
        
        #region Health Monitoring & Performance
        
        private void UpdateServiceStatus(Type serviceType, ServiceStatus status)
        {
            if (_serviceIntegrations.ContainsKey(serviceType))
            {
                _serviceIntegrations[serviceType].Status = status;
                _serviceIntegrations[serviceType].LastStatusUpdate = DateTime.UtcNow;
                
                if (status == ServiceStatus.Failed)
                {
                    _serviceIntegrations[serviceType].FailureCount++;
                }
                
                OnServiceStatusChanged?.Invoke(serviceType, status);
            }
        }
        
        private void IncrementEventMetric(string eventType)
        {
            if (!_enableEventMetrics) return;
            
            if (!_eventMetrics.ContainsKey(eventType))
            {
                _eventMetrics[eventType] = 0;
            }
            _eventMetrics[eventType]++;
        }
        
        public IntegrationMetrics GetMetrics()
        {
            return new IntegrationMetrics
            {
                TotalServices = _serviceIntegrations.Count,
                HealthyServices = _serviceIntegrations.Values.Count(s => s.Status == ServiceStatus.Running),
                TotalEventSubscriptions = _eventSubscriptions.Count,
                EventMetrics = new Dictionary<string, int>(_eventMetrics),
                LastUpdated = DateTime.UtcNow,
                InitializationTime = _initializationTime,
                TotalEvents = _eventMetrics.Values.Sum(),
                TotalEventsProcessed = _eventMetrics.Values.Sum()
            };
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Update health monitoring
            _healthMonitor?.Update();
            
            // Update event bridge
            _eventBridge?.ProcessPendingEvents();
            
            // Update performance tracking
            _performanceTracker?.Update();
            
            // Periodic metrics update
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60 FPS
            {
                OnMetricsUpdated?.Invoke(GetMetrics());
            }
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            LogInfo("Shutting down AI Services Integration...");
            _integrationStatus = IntegrationStatus.ShuttingDown;
            OnIntegrationStatusChanged?.Invoke(_integrationStatus);
            
            try
            {
                // Shutdown services in reverse order
                ShutdownServices();
                
                // Clean up subscriptions
                CleanupEventSubscriptions();
                
                // Dispose resources
                _healthMonitor?.Dispose();
                _eventBridge?.Dispose();
                _performanceTracker?.Dispose();
                
                _integrationStatus = IntegrationStatus.Shutdown;
                IsInitialized = false;
                
                LogInfo("AI Services Integration shut down successfully");
                OnIntegrationStatusChanged?.Invoke(_integrationStatus);
            }
            catch (Exception ex)
            {
                LogError($"Error during shutdown: {ex.Message}");
            }
        }
        
        private void ShutdownServices()
        {
            var servicesToShutdown = new object[]
            {
                _coordinatorService,
                _recommendationService,
                _learningService,
                _personalityService,
                _analysisService
            };
            
            foreach (var service in servicesToShutdown)
            {
                try
                {
                    if (service is MonoBehaviour mb)
                    {
                        if (mb != null && mb.gameObject != null)
                            DestroyImmediate(mb.gameObject);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error shutting down service: {ex.Message}");
                }
            }
        }
        
        private void CleanupEventSubscriptions()
        {
            foreach (var subscription in _eventSubscriptions)
            {
                try
                {
                    _eventBridge.Unsubscribe(subscription.EventName, subscription.Handler);
                }
                catch (Exception ex)
                {
                    LogError($"Error unsubscribing from {subscription.EventName}: {ex.Message}");
                }
            }
            _eventSubscriptions.Clear();
        }
        
        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnCultivationAnalysisComplete(CultivationAnalysisResult result)
        {
            LogInfo($"Cultivation analysis completed - {result.TotalPlants} plants analyzed");
            OnCrossServiceEvent?.Invoke("Analysis.CultivationComplete", result);
        }
        
        private void OnEnvironmentalAnalysisComplete(AIEnvironmentalAnalysisResult result)
        {
            LogInfo($"Environmental analysis completed - Overall score: {result.OverallScore:P0}");
            OnCrossServiceEvent?.Invoke("Analysis.EnvironmentalComplete", result);
        }
        
        private void OnGeneticsAnalysisComplete(GeneticsAnalysisResult result)
        {
            LogInfo($"Genetics analysis completed - {result.TotalStrains} strains analyzed");
            OnCrossServiceEvent?.Invoke("Analysis.GeneticsComplete", result);
        }
        
        private void OnRecommendationCreated(AIRecommendation recommendation)
        {
            LogInfo($"New recommendation created: {recommendation.Title}");
            OnCrossServiceEvent?.Invoke("Recommendation.Created", recommendation);
        }
        
        private void OnRecommendationImplemented(AIRecommendation recommendation)
        {
            LogInfo($"Recommendation implemented: {recommendation.Title}");
            OnCrossServiceEvent?.Invoke("Recommendation.Implemented", recommendation);
        }
        
        private void OnPersonalityChanged(AIPersonality oldPersonality, AIPersonality newPersonality)
        {
            LogInfo($"AI personality changed from {oldPersonality} to {newPersonality}");
            OnCrossServiceEvent?.Invoke("Personality.Changed", newPersonality);
        }
        
        private void OnPlayerInteractionRecorded(PlayerInteractionData interactionData)
        {
            LogInfo($"Player interaction recorded: {interactionData.InteractionType}");
            OnCrossServiceEvent?.Invoke("Personality.InteractionRecorded", interactionData);
        }
        
        private void OnModelTrained(MLModelData modelInfo)
        {
            LogInfo($"AI model trained: {modelInfo.ModelId}");
            OnCrossServiceEvent?.Invoke("Learning.ModelTrained", modelInfo);
        }
        
        private void OnPredictionMade(PredictionResult predictionData)
        {
            LogInfo($"AI prediction made: {predictionData.PredictedVariable} = {predictionData.PredictedValue:F2}");
            OnCrossServiceEvent?.Invoke("Learning.PredictionMade", predictionData);
        }
        
        private void OnRequestCompleted(CoordinationRequest request)
        {
            LogInfo($"Coordinator request completed: {request.RequestId}");
            OnCrossServiceEvent?.Invoke("Coordinator.RequestCompleted", request);
        }
        
        private void OnWorkflowCompleted(WorkflowType workflow)
        {
            LogInfo($"Coordinator workflow completed: {workflow}");
            OnCrossServiceEvent?.Invoke("Coordinator.WorkflowCompleted", workflow);
        }
        
        #endregion
        
        #region Utility Methods
        
        private void LogInfo(string message)
        {
            Debug.Log($"[AIServicesIntegration] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AIServicesIntegration] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AIServicesIntegration] {message}");
        }
        
        #endregion
    }
    
}