using System;
using System.Collections.Generic;
using ProjectChimera.Core.DependencyInjection;
using ProjectChimera.Systems.Gaming;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Interface for AI Services Integration - defines service integration and coordination contracts
    /// </summary>
    public interface IAIServicesIntegration
    {
        // Initialization
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        // Status and Properties
        IntegrationStatus Status { get; }
        IServiceContainer ServiceContainer { get; }
        IGameEventBus EventBus { get; }
        Dictionary<Type, ServiceIntegrationInfo> ServiceIntegrations { get; }
        
        // Metrics and Monitoring
        IntegrationMetrics GetMetrics();
        
        // Events
        event Action<IntegrationStatus> OnIntegrationStatusChanged;
        event Action<Type, ServiceStatus> OnServiceStatusChanged;
        event Action<string, object> OnCrossServiceEvent;
        event Action<IntegrationMetrics> OnMetricsUpdated;
    }
}