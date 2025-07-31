using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core.DependencyInjection;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-6: AI Service Locator - Simple service location for AI services
    /// Provides easy access to AI services throughout the application
    /// </summary>
    public static class AIServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static IServiceContainer _container;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the AI service locator with a service container
        /// </summary>
        public static void Initialize(IServiceContainer container)
        {
            _container = container;
            _isInitialized = true;
        }
        
        /// <summary>
        /// Get a service instance by type
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[AIServiceLocator] Not initialized, attempting to find service manually");
                return FindServiceManually<T>();
            }
            
            try
            {
                // Try container first
                var service = _container?.TryResolve<T>();
                if (service != null)
                {
                    return service;
                }
                
                // Fall back to cached services
                if (_services.ContainsKey(typeof(T)))
                {
                    return _services[typeof(T)] as T;
                }
                
                // Last resort: find manually
                return FindServiceManually<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AIServiceLocator] Error getting service {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Register a service instance
        /// </summary>
        public static void RegisterService<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
            
            if (_container != null)
            {
                try
                {
                    _container.RegisterSingleton<T>(instance);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AIServiceLocator] Could not register with container: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Check if a service is available
        /// </summary>
        public static bool HasService<T>() where T : class
        {
            return GetService<T>() != null;
        }
        
        /// <summary>
        /// Get all AI services
        /// </summary>
        public static AIServiceCollection GetAllAIServices()
        {
            return new AIServiceCollection
            {
                AnalysisService = GetService<IAIAnalysisService>(),
                RecommendationService = GetService<IAIRecommendationService>(),
                PersonalityService = GetService<IAIPersonalityService>(),
                LearningService = GetService<IAILearningService>(),
                CoordinatorService = GetService<IAIAdvisorCoordinator>(),
                IntegrationService = GetService<IAIServicesIntegration>()
            };
        }
        
        private static T FindServiceManually<T>() where T : class
        {
            // Find service in scene as last resort
            if (typeof(T) == typeof(IAIAnalysisService))
            {
                return UnityEngine.Object.FindObjectOfType<AIAnalysisService>() as T;
            }
            if (typeof(T) == typeof(IAIRecommendationService))
            {
                return UnityEngine.Object.FindObjectOfType<AIRecommendationService>() as T;
            }
            if (typeof(T) == typeof(IAIPersonalityService))
            {
                return UnityEngine.Object.FindObjectOfType<AIPersonalityService>() as T;
            }
            if (typeof(T) == typeof(IAILearningService))
            {
                return UnityEngine.Object.FindObjectOfType<AILearningService>() as T;
            }
            if (typeof(T) == typeof(IAIAdvisorCoordinator))
            {
                return UnityEngine.Object.FindObjectOfType<AIAdvisorCoordinator>() as T;
            }
            if (typeof(T) == typeof(IAIServicesIntegration))
            {
                return UnityEngine.Object.FindObjectOfType<AIServicesIntegration>() as T;
            }
            
            return null;
        }
        
        /// <summary>
        /// Clear all cached services
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
            _container = null;
        }
    }
    
    /// <summary>
    /// Collection of all AI services for easy access
    /// </summary>
    public class AIServiceCollection
    {
        public IAIAnalysisService AnalysisService { get; set; }
        public IAIRecommendationService RecommendationService { get; set; }
        public IAIPersonalityService PersonalityService { get; set; }
        public IAILearningService LearningService { get; set; }
        public IAIAdvisorCoordinator CoordinatorService { get; set; }
        public IAIServicesIntegration IntegrationService { get; set; }
        
        /// <summary>
        /// Check if all core services are available
        /// </summary>
        public bool AllCoreServicesAvailable =>
            AnalysisService != null &&
            RecommendationService != null &&
            PersonalityService != null &&
            LearningService != null &&
            CoordinatorService != null;
        
        /// <summary>
        /// Get count of available services
        /// </summary>
        public int AvailableServiceCount
        {
            get
            {
                int count = 0;
                if (AnalysisService != null) count++;
                if (RecommendationService != null) count++;
                if (PersonalityService != null) count++;
                if (LearningService != null) count++;
                if (CoordinatorService != null) count++;
                if (IntegrationService != null) count++;
                return count;
            }
        }
    }
}