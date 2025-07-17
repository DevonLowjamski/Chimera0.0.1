using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Advanced manager dependency resolution system for Project Chimera.
    /// Provides automatic dependency injection, circular dependency detection,
    /// and optimized manager lifecycle management.
    /// 
    /// Phase 3.1 Enhanced Integration Feature - Manager Dependency Resolution Optimization
    /// </summary>
    public class AdvancedManagerDependencySystem : MonoBehaviour
    {
        [Header("Dependency System Configuration")]
        [SerializeField] private bool _enableDependencyLogging = true;
        [SerializeField] private bool _enableCircularDependencyDetection = true;
        [SerializeField] private bool _enableLazyLoading = true;
        [SerializeField] private float _dependencyResolutionTimeoutSeconds = 10f;

        // Dependency graph and resolution tracking
        private Dictionary<Type, HashSet<Type>> _dependencyGraph = new Dictionary<Type, HashSet<Type>>();
        private Dictionary<Type, ChimeraManager> _registeredManagers = new Dictionary<Type, ChimeraManager>();
        private Dictionary<Type, DependencyMetadata> _dependencyMetadata = new Dictionary<Type, DependencyMetadata>();
        private HashSet<Type> _initializingManagers = new HashSet<Type>();
        private List<Type> _initializationOrder = new List<Type>();

        public static AdvancedManagerDependencySystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDependencySystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Dependency metadata for tracking manager requirements and status.
        /// </summary>
        private class DependencyMetadata
        {
            public Type ManagerType;
            public HashSet<Type> Dependencies = new HashSet<Type>();
            public DependencyStatus Status = DependencyStatus.NotRegistered;
            public float InitializationStartTime;
            public string InitializationError;
            public int DependencyDepth;

            public bool IsInitialized => Status == DependencyStatus.Initialized;
            public bool HasFailedInitialization => Status == DependencyStatus.Failed;
        }

        private enum DependencyStatus
        {
            NotRegistered,
            Registered,
            Initializing,
            Initialized,
            Failed,
            Disposed
        }

        /// <summary>
        /// Initialize the advanced dependency system.
        /// </summary>
        private void InitializeDependencySystem()
        {
            ChimeraLogger.Log("Dependencies", "Advanced Manager Dependency System initialized", this);
            
            // Pre-analyze all manager types for dependency optimization
            AnalyzeManagerDependencies();
            
            if (_enableDependencyLogging)
            {
                LogDependencyGraph();
            }
        }

        /// <summary>
        /// Register a manager with automatic dependency analysis.
        /// </summary>
        public void RegisterManager<T>(T manager) where T : ChimeraManager
        {
            var managerType = typeof(T);
            
            if (_registeredManagers.ContainsKey(managerType))
            {
                ChimeraLogger.LogWarning("Dependencies", $"Manager {managerType.Name} already registered", this);
                return;
            }

            _registeredManagers[managerType] = manager;
            
            // Analyze dependencies for this manager
            AnalyzeManagerDependencies(managerType);
            
            if (_enableDependencyLogging)
            {
                ChimeraLogger.Log("Dependencies", $"Registered manager: {managerType.Name}", this);
            }
        }

        /// <summary>
        /// Get a manager with automatic dependency resolution.
        /// </summary>
        public T GetManager<T>() where T : ChimeraManager
        {
            var managerType = typeof(T);
            
            if (_registeredManagers.TryGetValue(managerType, out var manager))
            {
                // Ensure manager is initialized with dependencies resolved
                if (!_dependencyMetadata.ContainsKey(managerType) || 
                    !_dependencyMetadata[managerType].IsInitialized)
                {
                    ResolveDependenciesForManager(managerType);
                }
                
                return manager as T;
            }

            if (_enableLazyLoading)
            {
                ChimeraLogger.LogWarning("Dependencies", $"Manager {managerType.Name} not found, attempting lazy instantiation", this);
                return AttemptLazyManagerCreation<T>();
            }

            ChimeraLogger.LogError("Dependencies", $"Manager {managerType.Name} not found and lazy loading disabled", this);
            return null;
        }

        /// <summary>
        /// Resolve all dependencies for optimal initialization order.
        /// </summary>
        public void ResolveAllDependencies()
        {
            var startTime = Time.realtimeSinceStartup;
            
            if (_enableCircularDependencyDetection && DetectCircularDependencies())
            {
                ChimeraLogger.LogError("Dependencies", "Circular dependencies detected! Cannot proceed with initialization.", this);
                return;
            }

            // Calculate optimal initialization order
            CalculateInitializationOrder();
            
            // Initialize managers in dependency order
            foreach (var managerType in _initializationOrder)
            {
                if (_registeredManagers.ContainsKey(managerType))
                {
                    ResolveDependenciesForManager(managerType);
                }
            }

            var totalTime = Time.realtimeSinceStartup - startTime;
            ChimeraLogger.Log("Dependencies", $"Dependency resolution completed in {totalTime:F3} seconds", this);
        }

        /// <summary>
        /// Analyze manager dependencies using reflection and manual configuration.
        /// </summary>
        private void AnalyzeManagerDependencies(Type specificManagerType = null)
        {
            var managersToAnalyze = specificManagerType != null 
                ? new[] { specificManagerType }
                : _registeredManagers.Keys.ToArray();

            foreach (var managerType in managersToAnalyze)
            {
                if (!_dependencyMetadata.ContainsKey(managerType))
                {
                    _dependencyMetadata[managerType] = new DependencyMetadata
                    {
                        ManagerType = managerType,
                        Status = DependencyStatus.Registered
                    };
                }

                var metadata = _dependencyMetadata[managerType];
                
                // Analyze dependencies based on manager type patterns
                AnalyzeSpecificManagerDependencies(managerType, metadata);
            }
        }

        /// <summary>
        /// Analyze dependencies for specific manager types based on Project Chimera patterns.
        /// </summary>
        private void AnalyzeSpecificManagerDependencies(Type managerType, DependencyMetadata metadata)
        {
            var managerName = managerType.Name;

            // Core managers have no dependencies
            if (IsCoreManager(managerType))
            {
                metadata.DependencyDepth = 0;
                return;
            }

            // Data managers depend on core managers
            if (IsDataManager(managerType))
            {
                metadata.Dependencies.Add(typeof(GameManager));
                metadata.Dependencies.Add(typeof(EventManager));
                metadata.DependencyDepth = 1;
                return;
            }

            // System managers depend on core and data managers
            if (IsSystemManager(managerType))
            {
                metadata.Dependencies.Add(typeof(GameManager));
                metadata.Dependencies.Add(typeof(DataManager));
                metadata.Dependencies.Add(typeof(EventManager));
                
                // Add specific system dependencies
                AddSystemSpecificDependencies(managerType, metadata);
                metadata.DependencyDepth = 2;
                return;
            }

            // UI managers depend on systems and data
            if (IsUIManager(managerType))
            {
                metadata.Dependencies.Add(typeof(GameManager));
                metadata.Dependencies.Add(typeof(DataManager));
                metadata.Dependencies.Add(typeof(EventManager));
                metadata.DependencyDepth = 3;
                return;
            }

            // Default: depends on core managers
            metadata.Dependencies.Add(typeof(GameManager));
            metadata.DependencyDepth = 1;
        }

        /// <summary>
        /// Add system-specific dependencies based on manager type.
        /// </summary>
        private void AddSystemSpecificDependencies(Type managerType, DependencyMetadata metadata)
        {
            var managerName = managerType.Name;

            // PlantManager and related cultivation systems
            if (managerName.Contains("Plant") || managerName.Contains("Cultivation"))
            {
                // Cultivation systems depend on genetics and environmental systems
                AddDependencyIfManagerExists(metadata, "GeneticsManager");
                AddDependencyIfManagerExists(metadata, "EnvironmentalManager");
            }

            // Genetics systems
            if (managerName.Contains("Genetics") || managerName.Contains("Breeding"))
            {
                // Genetics systems are foundational - minimal dependencies
            }

            // Environmental systems
            if (managerName.Contains("Environmental") || managerName.Contains("HVAC") || managerName.Contains("Climate"))
            {
                // Environmental systems are foundational - minimal dependencies
            }

            // Economic systems
            if (managerName.Contains("Economy") || managerName.Contains("Market") || managerName.Contains("Trading"))
            {
                // Economic systems depend on cultivation data
                AddDependencyIfManagerExists(metadata, "PlantManager");
            }

            // Construction and facility systems
            if (managerName.Contains("Construction") || managerName.Contains("Facility"))
            {
                // Construction depends on economy and environmental systems
                AddDependencyIfManagerExists(metadata, "EconomyManager");
                AddDependencyIfManagerExists(metadata, "EnvironmentalManager");
            }
        }

        /// <summary>
        /// Add dependency if a manager with the specified name exists.
        /// </summary>
        private void AddDependencyIfManagerExists(DependencyMetadata metadata, string managerTypeName)
        {
            foreach (var registeredType in _registeredManagers.Keys)
            {
                if (registeredType.Name == managerTypeName)
                {
                    metadata.Dependencies.Add(registeredType);
                    break;
                }
            }
        }

        /// <summary>
        /// Determine if manager is a core manager type.
        /// </summary>
        private bool IsCoreManager(Type managerType)
        {
            var coreManagerTypes = new[]
            {
                "GameManager", "TimeManager", "EventManager"
            };
            
            return coreManagerTypes.Contains(managerType.Name);
        }

        /// <summary>
        /// Determine if manager is a data manager type.
        /// </summary>
        private bool IsDataManager(Type managerType)
        {
            var dataManagerTypes = new[]
            {
                "DataManager", "SaveManager", "SettingsManager"
            };
            
            return dataManagerTypes.Contains(managerType.Name);
        }

        /// <summary>
        /// Determine if manager is a system manager type.
        /// </summary>
        private bool IsSystemManager(Type managerType)
        {
            return managerType.Name.Contains("Manager") && 
                   !IsCoreManager(managerType) && 
                   !IsDataManager(managerType) && 
                   !IsUIManager(managerType);
        }

        /// <summary>
        /// Determine if manager is a UI manager type.
        /// </summary>
        private bool IsUIManager(Type managerType)
        {
            return managerType.Name.Contains("UI") || 
                   managerType.Name.Contains("Interface") ||
                   managerType.Name.Contains("Panel");
        }

        /// <summary>
        /// Detect circular dependencies in the dependency graph.
        /// </summary>
        private bool DetectCircularDependencies()
        {
            var visited = new HashSet<Type>();
            var recursionStack = new HashSet<Type>();

            foreach (var managerType in _dependencyMetadata.Keys)
            {
                if (HasCircularDependency(managerType, visited, recursionStack))
                {
                    ChimeraLogger.LogError("Dependencies", $"Circular dependency detected involving {managerType.Name}", this);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Recursive helper for circular dependency detection.
        /// </summary>
        private bool HasCircularDependency(Type managerType, HashSet<Type> visited, HashSet<Type> recursionStack)
        {
            if (recursionStack.Contains(managerType))
            {
                return true; // Circular dependency found
            }

            if (visited.Contains(managerType))
            {
                return false; // Already processed
            }

            visited.Add(managerType);
            recursionStack.Add(managerType);

            if (_dependencyMetadata.TryGetValue(managerType, out var metadata))
            {
                foreach (var dependency in metadata.Dependencies)
                {
                    if (HasCircularDependency(dependency, visited, recursionStack))
                    {
                        return true;
                    }
                }
            }

            recursionStack.Remove(managerType);
            return false;
        }

        /// <summary>
        /// Calculate optimal initialization order based on dependencies.
        /// </summary>
        private void CalculateInitializationOrder()
        {
            _initializationOrder.Clear();
            var remainingManagers = new HashSet<Type>(_dependencyMetadata.Keys);
            var resolvedManagers = new HashSet<Type>();

            // First pass: managers with no dependencies
            var independentManagers = _dependencyMetadata
                .Where(kvp => kvp.Value.Dependencies.Count == 0)
                .Select(kvp => kvp.Key)
                .OrderBy(type => _dependencyMetadata[type].DependencyDepth)
                .ToList();

            foreach (var manager in independentManagers)
            {
                _initializationOrder.Add(manager);
                resolvedManagers.Add(manager);
                remainingManagers.Remove(manager);
            }

            // Iterative resolution of remaining dependencies
            while (remainingManagers.Count > 0)
            {
                var readyManagers = new List<Type>();

                foreach (var managerType in remainingManagers)
                {
                    var metadata = _dependencyMetadata[managerType];
                    if (metadata.Dependencies.All(dep => resolvedManagers.Contains(dep) || !_registeredManagers.ContainsKey(dep)))
                    {
                        readyManagers.Add(managerType);
                    }
                }

                if (readyManagers.Count == 0)
                {
                    // Remaining managers have unresolvable dependencies
                    ChimeraLogger.LogWarning("Dependencies", $"Cannot resolve dependencies for {remainingManagers.Count} managers", this);
                    break;
                }

                // Sort by dependency depth for optimal ordering
                readyManagers.Sort((a, b) => _dependencyMetadata[a].DependencyDepth.CompareTo(_dependencyMetadata[b].DependencyDepth));

                foreach (var manager in readyManagers)
                {
                    _initializationOrder.Add(manager);
                    resolvedManagers.Add(manager);
                    remainingManagers.Remove(manager);
                }
            }
        }

        /// <summary>
        /// Resolve dependencies for a specific manager.
        /// </summary>
        private void ResolveDependenciesForManager(Type managerType)
        {
            if (!_dependencyMetadata.TryGetValue(managerType, out var metadata))
            {
                ChimeraLogger.LogWarning("Dependencies", $"No dependency metadata for {managerType.Name}", this);
                return;
            }

            if (metadata.IsInitialized)
            {
                return; // Already initialized
            }

            if (_initializingManagers.Contains(managerType))
            {
                ChimeraLogger.LogError("Dependencies", $"Circular initialization detected for {managerType.Name}", this);
                return;
            }

            _initializingManagers.Add(managerType);
            metadata.Status = DependencyStatus.Initializing;
            metadata.InitializationStartTime = Time.realtimeSinceStartup;

            try
            {
                // Ensure all dependencies are resolved first
                foreach (var dependencyType in metadata.Dependencies)
                {
                    if (_registeredManagers.ContainsKey(dependencyType))
                    {
                        ResolveDependenciesForManager(dependencyType);
                    }
                }

                // Initialize the manager
                if (_registeredManagers.TryGetValue(managerType, out var manager))
                {
                    // Call OnManagerInitialize if not already called
                    var initializeMethod = managerType.GetMethod("OnManagerInitialize", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (initializeMethod != null)
                    {
                        initializeMethod.Invoke(manager, null);
                    }
                }

                metadata.Status = DependencyStatus.Initialized;
                
                if (_enableDependencyLogging)
                {
                    var initTime = Time.realtimeSinceStartup - metadata.InitializationStartTime;
                    ChimeraLogger.Log("Dependencies", $"Initialized {managerType.Name} in {initTime:F3}s", this);
                }
            }
            catch (Exception ex)
            {
                metadata.Status = DependencyStatus.Failed;
                metadata.InitializationError = ex.Message;
                ChimeraLogger.LogError("Dependencies", $"Failed to initialize {managerType.Name}: {ex.Message}", this);
            }
            finally
            {
                _initializingManagers.Remove(managerType);
            }
        }

        /// <summary>
        /// Attempt lazy creation of a manager if lazy loading is enabled.
        /// </summary>
        private T AttemptLazyManagerCreation<T>() where T : ChimeraManager
        {
            var managerType = typeof(T);
            
            // Check if the manager GameObject exists in the scene
            var managerGameObject = GameObject.Find(managerType.Name);
            if (managerGameObject != null)
            {
                var manager = managerGameObject.GetComponent<T>();
                if (manager != null)
                {
                    RegisterManager(manager);
                    return manager;
                }
            }

            ChimeraLogger.LogWarning("Dependencies", $"Could not lazy load manager {managerType.Name}", this);
            return null;
        }

        /// <summary>
        /// Log the current dependency graph for debugging.
        /// </summary>
        private void LogDependencyGraph()
        {
            ChimeraLogger.Log("Dependencies", "=== Dependency Graph ===", this);
            
            foreach (var kvp in _dependencyMetadata.OrderBy(kvp => kvp.Value.DependencyDepth))
            {
                var managerName = kvp.Key.Name;
                var metadata = kvp.Value;
                var dependencyNames = metadata.Dependencies.Select(dep => dep.Name).ToArray();
                
                ChimeraLogger.Log("Dependencies", 
                    $"{managerName} (Depth: {metadata.DependencyDepth}) → [{string.Join(", ", dependencyNames)}]", this);
            }
            
            ChimeraLogger.Log("Dependencies", "=== Initialization Order ===", this);
            for (int i = 0; i < _initializationOrder.Count; i++)
            {
                ChimeraLogger.Log("Dependencies", $"{i + 1}. {_initializationOrder[i].Name}", this);
            }
        }

        /// <summary>
        /// Get detailed dependency information for debugging.
        /// </summary>
        public Dictionary<string, object> GetDependencySystemInfo()
        {
            return new Dictionary<string, object>
            {
                ["RegisteredManagers"] = _registeredManagers.Count,
                ["DependencyGraph"] = _dependencyGraph.Count,
                ["InitializationOrder"] = _initializationOrder.Select(t => t.Name).ToArray(),
                ["FailedInitializations"] = _dependencyMetadata.Values.Count(m => m.HasFailedInitialization),
                ["CircularDependenciesDetected"] = _enableCircularDependencyDetection && DetectCircularDependencies()
            };
        }

        private void OnDestroy()
        {
            // Cleanup dependency system
            _dependencyGraph.Clear();
            _registeredManagers.Clear();
            _dependencyMetadata.Clear();
            _initializingManagers.Clear();
            _initializationOrder.Clear();
        }
    }
}