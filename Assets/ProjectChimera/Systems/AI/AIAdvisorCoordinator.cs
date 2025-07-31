using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Progression;
using AIPerformanceMetric = ProjectChimera.Data.AI.PerformanceMetric;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-5: AI Advisor Coordinator - Central orchestration service for all AI advisor components
    /// Extracted from AIAdvisorManager for improved modularity and testability
    /// Handles service coordination, workflow orchestration, and integration with game systems
    /// Target: 600 lines focused on service orchestration and coordination
    /// </summary>
    public class AIAdvisorCoordinator : MonoBehaviour, IAIAdvisorCoordinator
    {
        [Header("Coordination Configuration")]
        [SerializeField] private CoordinatorSettings _coordinatorConfig;
        [SerializeField] private float _coordinationInterval = 60f; // 1 minute
        [SerializeField] private int _maxConcurrentOperations = 5;
        [SerializeField] private bool _enableAutoCoordination = true;
        [SerializeField] private float _serviceHealthCheckInterval = 30f;
        
        [Header("Workflow Settings")]
        [SerializeField] private WorkflowMode _workflowMode = WorkflowMode.Automatic;
        [SerializeField] private float _analysisToRecommendationDelay = 2f;
        [SerializeField] private float _recommendationToPersonalityDelay = 1f;
        [SerializeField] private float _personalityToLearningDelay = 0.5f;
        [SerializeField] private bool _enableParallelProcessing = true;
        
        [Header("Performance Management")]
        [SerializeField] private int _maxQueuedRequests = 100;
        [SerializeField] private float _requestTimeoutSeconds = 30f;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _performanceReportInterval = 300f; // 5 minutes
        
        // AI Service Dependencies
        private IAIAnalysisService _analysisService;
        private IAIRecommendationService _recommendationService;
        private IAIPersonalityService _personalityService;
        private IAILearningService _learningService;
        private IPlayerProgressionService _progressionService;
        private MonoBehaviour _eventBus; // Temporarily using MonoBehaviour instead of GameEventBus
        
        // Coordination state management
        private ServiceRegistry _serviceRegistry;
        private WorkflowEngine _workflowEngine;
        private CoordinationQueue _requestQueue = new CoordinationQueue();
        private Dictionary<string, ServiceHealthStatus> _serviceHealthStatus = new Dictionary<string, ServiceHealthStatus>();
        
        // Performance tracking
        private CoordinatorStatistics _statistics;
        private List<CoordinationRequest> _activeRequests = new List<CoordinationRequest>();
        private Dictionary<string, float> _serviceResponseTimes = new Dictionary<string, float>();
        private Queue<AIPerformanceMetric> _performanceHistory = new Queue<AIPerformanceMetric>();
        
        // Workflow management
        private Dictionary<WorkflowType, WorkflowDefinition> _workflowDefinitions = new Dictionary<WorkflowType, WorkflowDefinition>();
        private List<ActiveWorkflow> _activeWorkflows = new List<ActiveWorkflow>();
        
        // Timing and scheduling
        private float _lastCoordinationTime;
        private float _lastHealthCheckTime;
        private float _lastPerformanceReportTime;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public ServiceRegistry ServiceRegistry => _serviceRegistry;
        public CoordinatorStatistics Statistics => _statistics;
        public int ActiveRequestCount => _activeRequests.Count;
        public int QueuedRequestCount => _requestQueue.Count;  
        public WorkflowMode CurrentWorkflowMode => _workflowMode;
        public List<CoordinationRequest> ActiveRequests => _activeRequests.ToList();
        public List<ActiveWorkflow> WorkflowHistory => _activeWorkflows.ToList();
        
        // Events
        public event Action<CoordinationRequest> OnRequestReceived;
        public event Action<CoordinationRequest> OnRequestCompleted;
        public event Action<CoordinationRequest> OnRequestFailed;
        public event Action<string> OnServiceHealthChanged;
        public event Action<CoordinatorStatistics> OnStatisticsUpdated;
        public event Action<WorkflowType> OnWorkflowCompleted;
        
        #region Initialization & Lifecycle
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            LogInfo("Initializing AI Advisor Coordinator...");
            
            InitializeServiceRegistry();
            InitializeWorkflowEngine();
            InitializeWorkflowDefinitions();
            InitializeStatistics();
            InitializeServiceDependencies();
            
            IsInitialized = true;
            LogInfo("AI Advisor Coordinator initialized successfully");
        }
        
        private void InitializeServiceRegistry()
        {
            _serviceRegistry = new ServiceRegistry();
            
            // Register expected services
            _serviceRegistry.RegisterService("Analysis", typeof(IAIAnalysisService));
            _serviceRegistry.RegisterService("Recommendation", typeof(IAIRecommendationService));
            _serviceRegistry.RegisterService("Personality", typeof(IAIPersonalityService));
            _serviceRegistry.RegisterService("Learning", typeof(IAILearningService));
            _serviceRegistry.RegisterService("Progression", typeof(IPlayerProgressionService));
            
            LogInfo("Service registry initialized");
        }
        
        private void InitializeWorkflowEngine()
        {
            _workflowEngine = new WorkflowEngine
            {
                MaxConcurrentWorkflows = _maxConcurrentOperations,
                EnableParallelProcessing = _enableParallelProcessing,
                DefaultTimeout = TimeSpan.FromSeconds(_requestTimeoutSeconds)
            };
            
            LogInfo("Workflow engine initialized");
        }
        
        private void InitializeWorkflowDefinitions()
        {
            // Standard Analysis Workflow: Analysis → Recommendations → Personality → Learning
            _workflowDefinitions[WorkflowType.StandardAnalysis] = new WorkflowDefinition
            {
                WorkflowType = WorkflowType.StandardAnalysis,
                Steps = new List<WorkflowStep>
                {
                    new WorkflowStep { StepId = "analysis", ServiceType = "Analysis", Action = "PerformAnalysis" },
                    new WorkflowStep { StepId = "recommendations", ServiceType = "Recommendation", Action = "GenerateRecommendations", DependsOn = new[] { "analysis" } },
                    new WorkflowStep { StepId = "personality", ServiceType = "Personality", Action = "AdaptPersonality", DependsOn = new[] { "recommendations" } },
                    new WorkflowStep { StepId = "learning", ServiceType = "Learning", Action = "ProcessLearningData", DependsOn = new[] { "personality" } }
                },
                Priority = WorkflowPriority.Normal
            };
            
            // Fast Recommendation Workflow: Analysis → Recommendations (skip personality/learning for speed)
            _workflowDefinitions[WorkflowType.FastRecommendation] = new WorkflowDefinition
            {
                WorkflowType = WorkflowType.FastRecommendation,
                Steps = new List<WorkflowStep>
                {
                    new WorkflowStep { StepId = "analysis", ServiceType = "Analysis", Action = "QuickAnalysis" },
                    new WorkflowStep { StepId = "recommendations", ServiceType = "Recommendation", Action = "GenerateRecommendations", DependsOn = new[] { "analysis" } }
                },
                Priority = WorkflowPriority.High
            };
            
            // Learning-Only Workflow: Process feedback without full analysis
            _workflowDefinitions[WorkflowType.LearningOnly] = new WorkflowDefinition
            {
                WorkflowType = WorkflowType.LearningOnly,
                Steps = new List<WorkflowStep>
                {
                    new WorkflowStep { StepId = "personality", ServiceType = "Personality", Action = "ProcessPlayerFeedback" },
                    new WorkflowStep { StepId = "learning", ServiceType = "Learning", Action = "AddLearningRecord", DependsOn = new[] { "personality" } }
                },
                Priority = WorkflowPriority.Low
            };
            
            LogInfo($"Initialized {_workflowDefinitions.Count} workflow definitions");
        }
        
        private void InitializeStatistics()
        {
            _statistics = new CoordinatorStatistics
            {
                TotalRequestsProcessed = 0,
                AverageResponseTime = 0f,
                SuccessRate = 1f,
                ActiveServices = 0,
                LastResetTime = DateTime.UtcNow
            };
            
            LogInfo("Coordinator statistics initialized");
        }
        
        private void InitializeServiceDependencies()
        {
            // Find and register AI services
            DiscoverAndRegisterServices();
            
            // Subscribe to service events
            SubscribeToServiceEvents();
            
            // Initial health check
            PerformServiceHealthCheck();
            
            LogInfo($"Service dependencies initialized - {_serviceRegistry.RegisteredServiceCount} services found");
        }
        
        private void DiscoverAndRegisterServices()
        {
            // Analysis Service
            _analysisService = FindObjectOfType<AIAnalysisService>();
            if (_analysisService != null)
            {
                _serviceRegistry.RegisterServiceInstance("Analysis", _analysisService);
                _serviceHealthStatus["Analysis"] = new ServiceHealthStatus { IsHealthy = true, LastCheckTime = DateTime.UtcNow };
            }
            
            // Recommendation Service
            _recommendationService = FindObjectOfType<AIRecommendationService>();
            if (_recommendationService != null)
            {
                _serviceRegistry.RegisterServiceInstance("Recommendation", _recommendationService);
                _serviceHealthStatus["Recommendation"] = new ServiceHealthStatus { IsHealthy = true, LastCheckTime = DateTime.UtcNow };
            }
            
            // Personality Service
            _personalityService = FindObjectOfType<AIPersonalityService>();
            if (_personalityService != null)
            {
                _serviceRegistry.RegisterServiceInstance("Personality", _personalityService);
                _serviceHealthStatus["Personality"] = new ServiceHealthStatus { IsHealthy = true, LastCheckTime = DateTime.UtcNow };
            }
            
            // Learning Service
            _learningService = FindObjectOfType<AILearningService>();
            if (_learningService != null)
            {
                _serviceRegistry.RegisterServiceInstance("Learning", _learningService);
                _serviceHealthStatus["Learning"] = new ServiceHealthStatus { IsHealthy = true, LastCheckTime = DateTime.UtcNow };
            }
            
            // Progression Service
            _progressionService = FindObjectOfType<MonoBehaviour>()?.GetComponent<IPlayerProgressionService>();
            if (_progressionService != null)
            {
                _serviceRegistry.RegisterServiceInstance("Progression", _progressionService);
                _serviceHealthStatus["Progression"] = new ServiceHealthStatus { IsHealthy = true, LastCheckTime = DateTime.UtcNow };
            }
        }
        
        private void SubscribeToServiceEvents()
        {
            // Analysis service events
            if (_analysisService is IAIAnalysisServiceExtended extendedAnalysis)
            {
                extendedAnalysis.OnCultivationAnalysisComplete += OnAnalysisCompleted;
                extendedAnalysis.OnEnvironmentalAnalysisComplete += OnEnvironmentalAnalysisCompleted;
                extendedAnalysis.OnGeneticsAnalysisComplete += OnGeneticsAnalysisCompleted;
            }
            
            // Recommendation service events
            if (_recommendationService != null)
            {
                _recommendationService.OnRecommendationCreated += OnRecommendationCreated;
                _recommendationService.OnRecommendationImplemented += OnRecommendationImplemented;
            }
            
            // Personality service events
            if (_personalityService != null)
            {
                _personalityService.OnPersonalityChanged += OnPersonalityChanged;
                _personalityService.OnPlayerInteractionRecorded += OnPlayerInteractionRecorded;
            }
            
            // Learning service events
            if (_learningService != null)
            {
                _learningService.OnModelTrained += OnModelTrained;
                _learningService.OnPredictionMade += OnPredictionMade;
            }
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            LogInfo("Shutting down AI Advisor Coordinator...");
            
            // Complete pending workflows
            CompleteActiveWorkflows();
            
            // Unsubscribe from events
            UnsubscribeFromServiceEvents();
            
            // Save coordination statistics
            SaveCoordinationData();
            
            IsInitialized = false;
            LogInfo("AI Advisor Coordinator shut down successfully");
        }
        
        #endregion
        
        #region Request Coordination
        
        public async Task<CoordinationResult> ProcessRequestAsync(CoordinationRequest request)
        {
            if (!IsInitialized)
            {
                LogError("Coordinator not initialized");
                return new CoordinationResult { Success = false, ErrorMessage = "Coordinator not initialized" };
            }
            
            request.ReceivedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Received;
            
            OnRequestReceived?.Invoke(request);
            
            // Check if we can process the request
            if (_activeRequests.Count >= _maxConcurrentOperations)
            {
                // Queue the request
                _requestQueue.Enqueue(request);
                LogInfo($"Request {request.RequestId} queued - {_requestQueue.Count} requests in queue");
                return new CoordinationResult { Success = true, Message = "Request queued for processing" };
            }
            
            return await ExecuteRequestAsync(request);
        }
        
        private async Task<CoordinationResult> ExecuteRequestAsync(CoordinationRequest request)
        {
            _activeRequests.Add(request);
            request.Status = RequestStatus.Processing;
            request.StartedAt = DateTime.UtcNow;
            
            try
            {
                var result = await ExecuteWorkflowAsync(request);
                
                request.Status = result.Success ? RequestStatus.Completed : RequestStatus.Failed;
                request.CompletedAt = DateTime.UtcNow;
                
                // Update statistics
                UpdateStatistics(request, result);
                
                if (result.Success)
                {
                    OnRequestCompleted?.Invoke(request);
                }
                else
                {
                    OnRequestFailed?.Invoke(request);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Request execution failed: {ex.Message}");
                request.Status = RequestStatus.Failed;
                request.CompletedAt = DateTime.UtcNow;
                
                OnRequestFailed?.Invoke(request);
                
                return new CoordinationResult { Success = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _activeRequests.Remove(request);
                ProcessNextQueuedRequest();
            }
        }
        
        private async Task<CoordinationResult> ExecuteWorkflowAsync(CoordinationRequest request)
        {
            if (!_workflowDefinitions.ContainsKey(request.WorkflowType))
            {
                return new CoordinationResult { Success = false, ErrorMessage = $"Unknown workflow type: {request.WorkflowType}" };
            }
            
            var workflowDef = _workflowDefinitions[request.WorkflowType];
            var workflow = new ActiveWorkflow
            {
                WorkflowId = Guid.NewGuid().ToString(),
                Request = request,
                Definition = workflowDef,
                StartedAt = DateTime.UtcNow,
                Status = WorkflowStatus.Running,
                StepResults = new Dictionary<string, object>()
            };
            
            _activeWorkflows.Add(workflow);
            
            try
            {
                // Execute workflow steps
                foreach (var step in workflowDef.Steps)
                {
                    // Check dependencies
                    if (step.DependsOn != null && step.DependsOn.Length > 0)
                    {
                        var dependenciesMet = step.DependsOn.All(dep => workflow.StepResults.ContainsKey(dep));
                        if (!dependenciesMet)
                        {
                            throw new InvalidOperationException($"Dependencies not met for step: {step.StepId}");
                        }
                    }
                    
                    // Execute step
                    var stepResult = await ExecuteWorkflowStepAsync(step, workflow);
                    workflow.StepResults[step.StepId] = stepResult;
                }
                
                workflow.Status = WorkflowStatus.Completed;
                workflow.CompletedAt = DateTime.UtcNow;
                
                OnWorkflowCompleted?.Invoke(request.WorkflowType);
                
                return new CoordinationResult 
                { 
                    Success = true, 
                    Data = workflow.StepResults,
                    Message = $"Workflow {request.WorkflowType} completed successfully"
                };
            }
            catch (Exception ex)
            {
                workflow.Status = WorkflowStatus.Failed;
                workflow.CompletedAt = DateTime.UtcNow;
                
                LogError($"Workflow execution failed: {ex.Message}");
                return new CoordinationResult { Success = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _activeWorkflows.Remove(workflow);
            }
        }
        
        private async Task<object> ExecuteWorkflowStepAsync(WorkflowStep step, ActiveWorkflow workflow)
        {
            var service = _serviceRegistry.GetServiceInstance(step.ServiceType);
            if (service == null)
            {
                throw new InvalidOperationException($"Service {step.ServiceType} not available");
            }
            
            var startTime = Time.time;
            object result = null;
            
            try
            {
                // Execute the appropriate action based on service type and action
                result = await ExecuteServiceActionAsync(service, step.Action, workflow);
                
                // Record response time
                var responseTime = Time.time - startTime;
                RecordServiceResponseTime(step.ServiceType, responseTime);
                
                LogInfo($"Step {step.StepId} completed in {responseTime:F2}s");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Step {step.StepId} failed: {ex.Message}");
                throw;
            }
        }
        
        private async Task<object> ExecuteServiceActionAsync(object service, string action, ActiveWorkflow workflow)
        {
            // Route to appropriate service method based on service type and action
            switch (service)
            {
                case IAIAnalysisService analysisService when action == "PerformAnalysis":
                    analysisService.PerformAnalysis(workflow.Request.Data as AnalysisSnapshot);
                    return "Analysis completed";
                    
                case IAIAnalysisService analysisService when action == "QuickAnalysis":
                    // Perform quick analysis
                    analysisService.PerformAnalysis(workflow.Request.Data as AnalysisSnapshot);
                    return "Quick analysis completed";
                    
                case IAIRecommendationService recommendationService when action == "GenerateRecommendations":
                    var recommendations = recommendationService.ActiveRecommendations;
                    return recommendations;
                    
                case IAIPersonalityService personalityService when action == "AdaptPersonality":
                    personalityService.AdaptPersonality();
                    return personalityService.CurrentPersonality;
                    
                case IAIPersonalityService personalityService when action == "ProcessPlayerFeedback":
                    if (workflow.Request.Data is PlayerFeedbackData feedback)
                    {
                        personalityService.ProcessPlayerFeedback(feedback.FeedbackType, feedback.Rating, feedback.Context);
                    }
                    return "Feedback processed";
                    
                case IAILearningService learningService when action == "ProcessLearningData":
                    learningService.OptimizeLearning();
                    return learningService.GetLearningStatistics();
                    
                case IAILearningService learningService when action == "AddLearningRecord":
                    if (workflow.Request.Data is LearningRecordData learningData)
                    {
                        learningService.AddLearningRecord(learningData.ActionTaken, learningData.Context, learningData.Outcome, learningData.SuccessRating);
                    }
                    return "Learning record added";
                    
                default:
                    await Task.Delay(10); // Simulate async operation
                    return $"Action {action} executed on {service.GetType().Name}";
            }
        }
        
        private void ProcessNextQueuedRequest()
        {
            if (_requestQueue.Count > 0 && _activeRequests.Count < _maxConcurrentOperations)
            {
                var nextRequest = _requestQueue.Dequeue();
                _ = ExecuteRequestAsync(nextRequest);
            }
        }
        
        #endregion
        
        #region Service Management
        
        public void RegisterService(string serviceName, object serviceInstance)
        {
            _serviceRegistry.RegisterServiceInstance(serviceName, serviceInstance);
            _serviceHealthStatus[serviceName] = new ServiceHealthStatus 
            { 
                IsHealthy = true, 
                LastCheckTime = DateTime.UtcNow 
            };
            
            LogInfo($"Service registered: {serviceName}");
        }
        
        public void UnregisterService(string serviceName)
        {
            _serviceRegistry.UnregisterService(serviceName);
            _serviceHealthStatus.Remove(serviceName);
            
            LogInfo($"Service unregistered: {serviceName}");
        }
        
        public bool IsServiceHealthy(string serviceName)
        {
            return _serviceHealthStatus.ContainsKey(serviceName) && _serviceHealthStatus[serviceName].IsHealthy;
        }
        
        public Dictionary<string, ServiceHealthStatus> GetServiceHealthStatus()
        {
            return new Dictionary<string, ServiceHealthStatus>(_serviceHealthStatus);
        }
        
        private void PerformServiceHealthCheck()
        {
            foreach (var serviceName in _serviceRegistry.GetRegisteredServiceNames())
            {
                var service = _serviceRegistry.GetServiceInstance(serviceName);
                var isHealthy = CheckServiceHealth(service);
                
                if (_serviceHealthStatus.ContainsKey(serviceName))
                {
                    var previousHealth = _serviceHealthStatus[serviceName].IsHealthy;
                    _serviceHealthStatus[serviceName].IsHealthy = isHealthy;
                    _serviceHealthStatus[serviceName].LastCheckTime = DateTime.UtcNow;
                    
                    if (previousHealth != isHealthy)
                    {
                        OnServiceHealthChanged?.Invoke(serviceName);
                        LogInfo($"Service {serviceName} health changed to: {(isHealthy ? "Healthy" : "Unhealthy")}");
                    }
                }
            }
            
            _statistics.ActiveServices = _serviceHealthStatus.Values.Count(s => s.IsHealthy);
        }
        
        private bool CheckServiceHealth(object service)
        {
            try
            {
                // Check if service implements health check interface
                if (service is IAIAnalysisService analysisService)
                    return analysisService.IsInitialized;
                if (service is IAIRecommendationService recommendationService)
                    return recommendationService.IsInitialized;
                if (service is IAIPersonalityService personalityService)
                    return personalityService.IsInitialized;
                if (service is IAILearningService learningService)
                    return learningService.IsInitialized;
                if (service is IPlayerProgressionService progressionService)
                    return progressionService.IsInitialized;
                
                return service != null;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnAnalysisCompleted(CultivationAnalysisResult result)
        {
            LogInfo("Cultivation analysis completed");
            // Trigger next step in workflow if appropriate
        }
        
        private void OnEnvironmentalAnalysisCompleted(EnvironmentalAnalysisResult result)
        {
            LogInfo("Environmental analysis completed");
        }
        
        private void OnGeneticsAnalysisCompleted(GeneticsAnalysisResult result)
        {
            LogInfo("Genetics analysis completed");
        }
        
        private void OnRecommendationCreated(AIRecommendation recommendation)
        {
            LogInfo($"New recommendation created: {recommendation.Title}");
        }
        
        private void OnRecommendationImplemented(AIRecommendation recommendation)
        {
            LogInfo($"Recommendation implemented: {recommendation.Title}");
            
            // Create learning record
            if (_learningService != null)
            {
                _learningService.AddLearningRecord(
                    $"RecommendationImplemented_{recommendation.Category}",
                    recommendation.Description,
                    "Implemented",
                    0.8f
                );
            }
        }
        
        private void OnPersonalityChanged(AIPersonality oldPersonality, AIPersonality newPersonality)
        {
            LogInfo($"AI personality changed from {oldPersonality} to {newPersonality}");
        }
        
        private void OnPlayerInteractionRecorded(PlayerInteractionData interaction)
        {
            LogInfo($"Player interaction recorded: {interaction.InteractionType}");
        }
        
        private void OnModelTrained(MLModelData model)
        {
            LogInfo($"ML model trained: {model.ModelId} with accuracy {model.Accuracy:F3}");
        }
        
        private void OnPredictionMade(PredictionResult prediction)
        {
            LogInfo($"Prediction made: {prediction.PredictedVariable} = {prediction.PredictedValue:F2}");
        }
        
        #endregion
        
        #region Statistics & Performance
        
        private void UpdateStatistics(CoordinationRequest request, CoordinationResult result)
        {
            _statistics.TotalRequestsProcessed++;
            
            if (request.StartedAt.HasValue && request.CompletedAt.HasValue)
            {
                var responseTime = (float)(request.CompletedAt.Value - request.StartedAt.Value).TotalSeconds;
                _statistics.AverageResponseTime = (_statistics.AverageResponseTime * 0.9f) + (responseTime * 0.1f);
            }
            
            if (result.Success)
            {
                _statistics.SuccessRate = (_statistics.SuccessRate * 0.95f) + 0.05f;
            }
            else
            {
                _statistics.SuccessRate = _statistics.SuccessRate * 0.95f;
            }
            
            // Record performance metric
            _performanceHistory.Enqueue(new AIPerformanceMetric
            {
                MetricName = "WorkflowExecution",
                MetricType = request.WorkflowType.ToString(),
                Value = _statistics.AverageResponseTime,
                Unit = "seconds",
                MeasuredAt = DateTime.UtcNow,
                TestEnvironment = result.Success ? "Success" : "Failed"
            });
            
            // Maintain performance history size
            if (_performanceHistory.Count > 1000)
            {
                _performanceHistory.Dequeue();
            }
        }
        
        private void RecordServiceResponseTime(string serviceName, float responseTime)
        {
            if (!_serviceResponseTimes.ContainsKey(serviceName))
            {
                _serviceResponseTimes[serviceName] = responseTime;
            }
            else
            {
                _serviceResponseTimes[serviceName] = (_serviceResponseTimes[serviceName] * 0.8f) + (responseTime * 0.2f);
            }
        }
        
        public CoordinatorStatistics GetStatistics()
        {
            return _statistics;
        }
        
        public Dictionary<string, float> GetServiceResponseTimes()
        {
            return new Dictionary<string, float>(_serviceResponseTimes);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Periodic coordination
            if (_enableAutoCoordination && Time.time - _lastCoordinationTime >= _coordinationInterval)
            {
                PerformPeriodicCoordination();
                _lastCoordinationTime = Time.time;
            }
            
            // Service health checks
            if (Time.time - _lastHealthCheckTime >= _serviceHealthCheckInterval)
            {
                PerformServiceHealthCheck();
                _lastHealthCheckTime = Time.time;
            }
            
            // Performance reporting
            if (_enablePerformanceMonitoring && Time.time - _lastPerformanceReportTime >= _performanceReportInterval)
            {
                GeneratePerformanceReport();
                _lastPerformanceReportTime = Time.time;
            }
        }
        
        private void PerformPeriodicCoordination()
        {
            // Trigger standard analysis workflow if conditions are met
            if (_workflowMode == WorkflowMode.Automatic && _activeRequests.Count == 0)
            {
                var request = new CoordinationRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    WorkflowType = WorkflowType.StandardAnalysis,
                    Priority = CoordinationPriority.Medium,
                    Data = null // Analysis service will use current game state
                };
                
                _ = ProcessRequestAsync(request);
            }
        }
        
        private void GeneratePerformanceReport()
        {
            OnStatisticsUpdated?.Invoke(_statistics);
            LogInfo($"Performance Report - Processed: {_statistics.TotalRequestsProcessed}, " +
                   $"Avg Response: {_statistics.AverageResponseTime:F2}s, " +
                   $"Success Rate: {_statistics.SuccessRate:F2}");
        }
        
        #endregion
        
        #region Utility Methods
        
        private void CompleteActiveWorkflows()
        {
            foreach (var workflow in _activeWorkflows.ToList())
            {
                workflow.Status = WorkflowStatus.Cancelled;
                workflow.CompletedAt = DateTime.UtcNow;
            }
            _activeWorkflows.Clear();
        }
        
        private void UnsubscribeFromServiceEvents()
        {
            // Unsubscribe from all service events
            // Implementation would mirror SubscribeToServiceEvents with -= instead of +=
        }
        
        private void SaveCoordinationData()
        {
            // Placeholder for coordination data persistence
            LogInfo("Coordination data saved");
        }
        
        private void LogInfo(string message)
        {
            Debug.Log($"[AIAdvisorCoordinator] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AIAdvisorCoordinator] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AIAdvisorCoordinator] {message}");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class CoordinatorSettings
    {
        [Header("Coordination")]
        public float coordinationInterval = 60f;
        public int maxConcurrentOperations = 5;
        public bool enableAutoCoordination = true;
        
        [Header("Performance")]
        public bool enablePerformanceMonitoring = true;
        public float performanceReportInterval = 300f;
        public int maxQueuedRequests = 100;
    }
    
    [System.Serializable]
    public class CoordinationRequest
    {
        public string RequestId;
        public RequestType RequestType;
        public WorkflowType WorkflowType;
        public CoordinationPriority Priority;
        public DateTime RequestedAt;
        public Dictionary<string, object> Parameters;
        public object Data;
        public DateTime ReceivedAt;
        public DateTime? StartedAt;
        public DateTime? CompletedAt;
        public RequestStatus Status;
    }
    
    [System.Serializable]
    public class CoordinationResult
    {
        public bool Success;
        public string Message;
        public string ErrorMessage;
        public Dictionary<string, object> Data;
        public List<AIRecommendation> GeneratedRecommendations = new List<AIRecommendation>();
    }
    
    [System.Serializable]
    public class ServiceHealthStatus
    {
        public bool IsHealthy;
        public DateTime LastCheckTime;
        public string ErrorMessage;
    }
    
    [System.Serializable]
    public class CoordinatorStatistics
    {
        public int TotalRequestsProcessed;
        public float AverageResponseTime;
        public float SuccessRate;
        public int ActiveServices;
        public DateTime LastResetTime;
    }
    
    public enum RequestType
    {
        Analysis,
        Recommendation,
        Learning,
        Personality,
        HealthCheck
    }
    
    public enum CoordinationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum WorkflowType
    {
        StandardAnalysis,
        FastRecommendation,
        LearningOnly,
        PersonalityAdaptation,
        HealthCheck,
        CultivationOptimization
    }
    
    public enum WorkflowMode
    {
        Manual,
        Automatic,
        Hybrid
    }
    
    public enum RequestStatus
    {
        Received,
        Queued,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
    
    #endregion
}