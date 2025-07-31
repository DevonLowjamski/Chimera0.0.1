using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-6: AI Services Integration Data Structures
    /// Supporting classes and data structures for AI services integration system
    /// </summary>
    
    [System.Serializable]
    public class IntegrationSettings
    {
        [Header("Service Management")]
        public bool autoServiceDiscovery = true;
        public bool validateServiceHealth = true;
        public float healthCheckInterval = 30f;
        public int maxRetries = 3;
        
        [Header("Event Configuration")]
        public bool enableEventBridge = true;
        public int maxEventQueueSize = 1000;
        public float eventProcessingTimeout = 1f;
        public bool trackEventMetrics = true;
        
        [Header("Performance")]
        public bool enablePerformanceMonitoring = true;
        public float performanceReportInterval = 60f;
        public bool enableDetailedMetrics = false;
    }
    
    [System.Serializable]
    public class ServiceIntegrationInfo
    {
        public Type ServiceType;
        public Type ImplementationType;
        public string ServiceName;
        public DateTime RegistrationTime;
        public DateTime LastStatusUpdate;
        public ServiceStatus Status;
        public bool IsAutoManaged;
        public int FailureCount;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class EventSubscription
    {
        public string EventName;
        public Type EventType;
        public object Handler;
        public DateTime SubscriptionTime;
        public int EventCount;
        public DateTime LastEventTime;
    }
    
    [System.Serializable]
    public class IntegrationMetrics
    {
        public int TotalServices;
        public int HealthyServices;
        public int TotalEventSubscriptions;
        public Dictionary<string, int> EventMetrics;
        public DateTime LastUpdated;
        public float AverageServiceResponseTime;
        public int TotalEventsProcessed;
        public int TotalEvents;
        public TimeSpan InitializationTime;
    }
    
    [System.Serializable]
    public class ServiceRegistrationOptions
    {
        public bool EnableAutoDiscovery = true;
        public bool ValidateServices = true;
        public bool TrackPerformance = true;
        public bool EnableHealthChecks = true;
        public TimeSpan InitializationTimeout = TimeSpan.FromSeconds(30);
    }
    
    public class ServiceHealthMonitor : IDisposable
    {
        private float _checkInterval;
        private bool _disposed = false;
        
        public bool EnableAutoRecovery { get; set; } = true;
        public int MaxRecoveryAttempts { get; set; } = 3;
        
        public event Action<Type, bool> OnServiceHealthChanged;
        
        public ServiceHealthMonitor(float checkInterval)
        {
            _checkInterval = checkInterval;
        }
        
        public void Update()
        {
            // Implementation for periodic health checks
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
    
    public class EventBridge : IDisposable
    {
        private readonly object _eventBus;
        private readonly int _maxQueueSize;
        private bool _disposed = false;
        
        public bool EnableMetrics { get; set; } = true;
        public float ProcessingTimeout { get; set; } = 1f;
        
        public EventBridge(object eventBus, int maxQueueSize)
        {
            _eventBus = eventBus;
            _maxQueueSize = maxQueueSize;
        }
        
        public void Subscribe<T>(string eventName, Action<T> handler)
        {
            // Implementation for event subscription
        }
        
        public void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            // Implementation for event unsubscription
        }
        
        public void Unsubscribe(string eventName, object handler)
        {
            // Generic unsubscribe implementation
        }
        
        public void ProcessPendingEvents()
        {
            // Implementation for processing queued events
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
    
    public class PerformanceTracker : IDisposable
    {
        private bool _disposed = false;
        
        public bool EnableDetailedMetrics { get; set; } = true;
        public float SampleInterval { get; set; } = 1f;
        public int MaxSamples { get; set; } = 1000;
        
        public void Update()
        {
            // Implementation for performance tracking
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
    
    // Event data structures for game events
    [System.Serializable]
    public class PlantGrowthEvent
    {
        public string PlantId;
        public float GrowthAmount;
        public DateTime Timestamp;
        public Dictionary<string, object> GrowthData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class PlantHarvestEvent
    {
        public string PlantId;
        public float Quality;
        public float Yield;
        public DateTime HarvestTime;
        public Dictionary<string, object> HarvestData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class PlayerActionEvent
    {
        public string ActionType;
        public string Context;
        public bool Success;
        public DateTime ActionTime;
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
    }
    
    public enum IntegrationStatus
    {
        NotInitialized,
        Initializing,
        Active,
        Running,
        Degraded,
        Failed,
        ShuttingDown,
        Shutdown
    }
    
    public enum ServiceStatus
    {
        NotRegistered,
        Registered,
        Starting,
        Running,
        Healthy,
        Unhealthy,
        Failed,
        Stopped
    }
    
    public enum ServiceLifecycleMode
    {
        Manual,
        Automatic,
        Hybrid
    }
}