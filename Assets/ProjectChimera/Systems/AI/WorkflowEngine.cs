using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-5: Workflow Engine supporting classes for AI Coordinator
    /// Provides workflow orchestration, service registry, and coordination queue functionality
    /// </summary>
    
    [System.Serializable]
    public class ServiceRegistry
    {
        private Dictionary<string, Type> _registeredServiceTypes = new Dictionary<string, Type>();
        private Dictionary<string, object> _serviceInstances = new Dictionary<string, object>();
        
        public int RegisteredServiceCount => _registeredServiceTypes.Count;
        
        public void RegisterService(string serviceName, Type serviceType)
        {
            _registeredServiceTypes[serviceName] = serviceType;
        }
        
        public void RegisterServiceInstance(string serviceName, object serviceInstance)
        {
            _serviceInstances[serviceName] = serviceInstance;
            if (serviceInstance != null)
            {
                _registeredServiceTypes[serviceName] = serviceInstance.GetType();
            }
        }
        
        public void UnregisterService(string serviceName)
        {
            _registeredServiceTypes.Remove(serviceName);
            _serviceInstances.Remove(serviceName);
        }
        
        public object GetServiceInstance(string serviceName)
        {
            return _serviceInstances.ContainsKey(serviceName) ? _serviceInstances[serviceName] : null;
        }
        
        public Type GetServiceType(string serviceName)
        {
            return _registeredServiceTypes.ContainsKey(serviceName) ? _registeredServiceTypes[serviceName] : null;
        }
        
        public List<string> GetRegisteredServiceNames()
        {
            return _registeredServiceTypes.Keys.ToList();
        }
        
        public bool IsServiceRegistered(string serviceName)
        {
            return _registeredServiceTypes.ContainsKey(serviceName);
        }
    }
    
    [System.Serializable]
    public class WorkflowEngine
    {
        public int MaxConcurrentWorkflows = 5;
        public bool EnableParallelProcessing = true;
        public TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        
        public bool CanExecuteWorkflow()
        {
            return true; // Simplified implementation
        }
    }
    
    [System.Serializable]
    public class CoordinationQueue
    {
        private Queue<CoordinationRequest> _requests = new Queue<CoordinationRequest>();
        
        public int Count => _requests.Count;
        
        public void Enqueue(CoordinationRequest request)
        {
            _requests.Enqueue(request);
        }
        
        public CoordinationRequest Dequeue()
        {
            return _requests.Count > 0 ? _requests.Dequeue() : null;
        }
        
        public CoordinationRequest Peek()
        {
            return _requests.Count > 0 ? _requests.Peek() : null;
        }
        
        public void Clear()
        {
            _requests.Clear();
        }
    }
    
    [System.Serializable]
    public class WorkflowDefinition
    {
        public WorkflowType WorkflowType;
        public List<WorkflowStep> Steps = new List<WorkflowStep>();
        public WorkflowPriority Priority;
        public TimeSpan Timeout = TimeSpan.FromSeconds(60);
    }
    
    [System.Serializable]
    public class WorkflowStep
    {
        public string StepId;
        public string ServiceType;
        public string Action;
        public string[] DependsOn;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public TimeSpan Timeout = TimeSpan.FromSeconds(10);
    }
    
    [System.Serializable]
    public class ActiveWorkflow
    {
        public string WorkflowId;
        public CoordinationRequest Request;
        public WorkflowDefinition Definition;
        public DateTime StartedAt;
        public DateTime? CompletedAt;
        public WorkflowStatus Status;
        public Dictionary<string, object> StepResults = new Dictionary<string, object>();
        public string ErrorMessage;
    }
    
    
    public enum WorkflowStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Timeout
    }
    
    public enum WorkflowPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}