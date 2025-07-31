using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Interface for AI Advisor Coordinator - defines service orchestration and coordination contracts
    /// </summary>
    public interface IAIAdvisorCoordinator
    {
        // Initialization
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        // Core Properties
        ServiceRegistry ServiceRegistry { get; }
        CoordinatorStatistics Statistics { get; }
        int ActiveRequestCount { get; }
        int QueuedRequestCount { get; }
        WorkflowMode CurrentWorkflowMode { get; }
        
        // Request Coordination
        Task<CoordinationResult> ProcessRequestAsync(CoordinationRequest request);
        
        // Service Management
        void RegisterService(string serviceName, object serviceInstance);
        void UnregisterService(string serviceName);
        bool IsServiceHealthy(string serviceName);
        
        // Statistics & Performance
        CoordinatorStatistics GetStatistics();
        Dictionary<string, float> GetServiceResponseTimes();
        
        // Events
        event Action<CoordinationRequest> OnRequestReceived;
        event Action<CoordinationRequest> OnRequestCompleted;
        event Action<CoordinationRequest> OnRequestFailed;
        event Action<string> OnServiceHealthChanged;
        event Action<CoordinatorStatistics> OnStatisticsUpdated;
        event Action<WorkflowType> OnWorkflowCompleted;
    }
}