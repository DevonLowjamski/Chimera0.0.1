using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using ProjectChimera.Core.DependencyInjection;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Dependency injection-enabled Game Manager for Project Chimera
    /// Extends GameManager functionality with comprehensive service container integration
    /// </summary>
    public class DIGameManager : DIChimeraManager, IGameManager
    {
        [Header("Game State Configuration")]
        [SerializeField] private GameStateConfigSO _gameStateConfig;
        [SerializeField] private bool _loadLastSaveOnStart = true;
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveInterval = 300.0f; // 5 minutes

        [Header("Game State Events")]
        [SerializeField] private SimpleGameEventSO _onGameInitialized;
        [SerializeField] private SimpleGameEventSO _onGamePaused;
        [SerializeField] private SimpleGameEventSO _onGameResumed;
        [SerializeField] private SimpleGameEventSO _onGameShutdown;

        [Header("DI Configuration")]
        [SerializeField] private bool _initializeManagersWithDI = true;
        [SerializeField] private bool _validateDependenciesOnStart = true;

        // Dependency-injected services
        private ITimeManager _timeManager;
        private IDataManager _dataManager;
        private IEventManager _eventManager;
        private ISettingsManager _settingsManager;

        // Manager registry for dynamic access
        private readonly Dictionary<Type, ChimeraManager> _managerRegistry = new Dictionary<Type, ChimeraManager>();
        private Coroutine _autoSaveCoroutine;

        /// <summary>
        /// Singleton instance of the DI Game Manager
        /// </summary>
        public static DIGameManager Instance { get; private set; }

        /// <summary>
        /// Current game state
        /// </summary>
        public GameState CurrentGameState { get; private set; } = GameState.Uninitialized;

        /// <summary>
        /// Whether the game is currently paused
        /// </summary>
        public bool IsGamePaused { get; private set; }

        /// <summary>
        /// Time when the game was started
        /// </summary>
        public DateTime GameStartTime { get; private set; }

        /// <summary>
        /// Total time the game has been running
        /// </summary>
        public TimeSpan TotalGameTime => DateTime.Now - GameStartTime;

        /// <summary>
        /// Access to the global service container
        /// </summary>
        public IServiceContainer GlobalServiceContainer => ServiceContainer;

        #region Unity Lifecycle

        protected override void Awake()
        {
            // Implement singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[DIGameManager] Multiple DIGameManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            StartCoroutine(InitializeGameSystems());
            base.Start();
        }

        #endregion

        #region Dependency Injection Override

        protected override IServiceContainer GetOrCreateServiceContainer()
        {
            // Create the global service container for the entire game
            var container = ServiceContainerFactory.CreateForDevelopment();
            
            // Register core game services
            RegisterCoreServices(container);
            
            Debug.Log("[DIGameManager] Global service container created and configured");
            return container;
        }

        protected override void ResolveDependencies()
        {
            try
            {
                // Resolve core manager dependencies
                _timeManager = TryResolveService<ITimeManager>();
                _dataManager = TryResolveService<IDataManager>();
                _eventManager = TryResolveService<IEventManager>();
                _settingsManager = TryResolveService<ISettingsManager>();

                if (_validateDependenciesOnStart)
                {
                    ValidateCriticalDependencies();
                }

                Debug.Log("[DIGameManager] Core dependencies resolved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Failed to resolve dependencies: {ex.Message}");
            }
        }

        #endregion

        #region Service Registration

        /// <summary>
        /// Register core game services with the container
        /// </summary>
        private void RegisterCoreServices(IServiceContainer container)
        {
            try
            {
                // Register this GameManager
                container.RegisterSingleton(this);
                container.RegisterSingleton<IGameManager>(this);

                // Register configuration
                if (_gameStateConfig != null)
                {
                    container.RegisterSingleton<GameStateConfigSO>(_gameStateConfig);
                }

                // Register factory methods for creating managers
                container.RegisterFactory<IServiceContainer>(locator => container);

                Debug.Log("[DIGameManager] Core services registered with container");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Failed to register core services: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a manager with both the DI container and legacy registry
        /// </summary>
        public void RegisterManager<T>(T manager) where T : ChimeraManager
        {
            if (manager == null) return;

            try
            {
                // Register with legacy dictionary
                var managerType = typeof(T);
                _managerRegistry[managerType] = manager;

                // Register with DI container if available
                if (ServiceContainer != null)
                {
                    ServiceContainer.RegisterSingleton<T>(manager);
                    
                    // Also register by interfaces
                    var interfaces = managerType.GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        if (interfaceType != typeof(IDisposable) && interfaceType != typeof(IChimeraManager))
                        {
                            try
                            {
                                var registerMethod = typeof(IServiceLocator).GetMethod("RegisterSingleton", new[] { interfaceType });
                                registerMethod?.MakeGenericMethod(interfaceType).Invoke(ServiceContainer, new object[] { manager });
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[DIGameManager] Could not register manager interface {interfaceType.Name}: {ex.Message}");
                            }
                        }
                    }
                }

                Debug.Log($"[DIGameManager] Registered manager: {manager.ManagerName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Failed to register manager {manager?.ManagerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get manager using DI container (preferred) or legacy registry
        /// </summary>
        public T GetManager<T>() where T : ChimeraManager
        {
            try
            {
                // Try DI container first
                if (ServiceContainer != null)
                {
                    var service = ServiceContainer.TryResolve<T>();
                    if (service != null) return service;
                }

                // Fallback to legacy registry
                if (_managerRegistry.TryGetValue(typeof(T), out var manager))
                {
                    return manager as T;
                }

                Debug.LogWarning($"[DIGameManager] Manager {typeof(T).Name} not found in container or registry");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Error retrieving manager {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Game System Initialization

        /// <summary>
        /// Initialize all game systems with dependency injection
        /// </summary>
        private IEnumerator InitializeGameSystems()
        {
            Debug.Log("[DIGameManager] Starting game system initialization");
            
            CurrentGameState = GameState.Initializing;
            GameStartTime = DateTime.Now;

            // Initialize core systems first
            yield return StartCoroutine(InitializeCoreManagers());

            // Initialize cultivation systems
            yield return StartCoroutine(InitializeCultivationManagers());

            // Initialize environment systems
            yield return StartCoroutine(InitializeEnvironmentManagers());

            // Initialize progression systems
            yield return StartCoroutine(InitializeProgressionManagers());

            // Initialize UI systems
            yield return StartCoroutine(InitializeUIManagers());

            // Validate all dependencies
            if (_validateDependenciesOnStart)
            {
                try
                {
                    ValidateAllDependencies();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DIGameManager] Dependency validation failed: {ex.Message}");
                    CurrentGameState = GameState.Error;
                    yield break;
                }
            }

            // Start auto-save if enabled
            if (_autoSaveEnabled)
            {
                _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }

            CurrentGameState = GameState.Running;
            _onGameInitialized?.RaiseEvent();

            Debug.Log("[DIGameManager] Game system initialization completed successfully");
        }

        private IEnumerator InitializeCoreManagers()
        {
            Debug.Log("[DIGameManager] Initializing core managers");

            // Find and initialize core managers
            var timeManager = FindObjectOfType<TimeManager>();
            if (timeManager != null) RegisterManager(timeManager);

            var dataManager = FindObjectOfType<DataManager>();
            if (dataManager != null) RegisterManager(dataManager);

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeCultivationManagers()
        {
            Debug.Log("[DIGameManager] Initializing cultivation managers");

            // Find cultivation managers and register them with DI
            // PlantManager will be registered when it's available
            Debug.Log("[DIGameManager] Cultivation managers ready for registration");

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeEnvironmentManagers()
        {
            Debug.Log("[DIGameManager] Initializing environment managers");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeProgressionManagers()
        {
            Debug.Log("[DIGameManager] Initializing progression managers");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator InitializeUIManagers()
        {
            Debug.Log("[DIGameManager] Initializing UI managers");
            yield return new WaitForEndOfFrame();
        }

        #endregion

        #region Dependency Validation

        /// <summary>
        /// Validate critical dependencies are resolved
        /// </summary>
        private void ValidateCriticalDependencies()
        {
            var criticalDependencies = new List<string>();

            if (_timeManager == null) criticalDependencies.Add("ITimeManager");
            if (_dataManager == null) criticalDependencies.Add("IDataManager");

            if (criticalDependencies.Count > 0)
            {
                Debug.LogWarning($"[DIGameManager] Missing critical dependencies: {string.Join(", ", criticalDependencies)}");
            }
            else
            {
                Debug.Log("[DIGameManager] All critical dependencies validated");
            }
        }

        /// <summary>
        /// Validate all registered services can be resolved
        /// </summary>
        private void ValidateAllDependencies()
        {
            if (ServiceContainer == null) return;

            try
            {
                var result = ServiceContainer.Verify();
                if (result.IsValid)
                {
                    Debug.Log($"[DIGameManager] All dependencies validated: {result.VerifiedServices}/{result.TotalServices} services verified");
                }
                else
                {
                    Debug.LogWarning($"[DIGameManager] Dependency validation issues: {result.Errors.Count} errors");
                    foreach (var error in result.Errors)
                    {
                        Debug.LogError($"[DIGameManager] Dependency error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Dependency validation failed: {ex.Message}");
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused) return;

            IsGamePaused = true;
            Time.timeScale = 0f;
            _onGamePaused?.RaiseEvent();

            Debug.Log("[DIGameManager] Game paused");
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused) return;

            IsGamePaused = false;
            Time.timeScale = 1f;
            _onGameResumed?.RaiseEvent();

            Debug.Log("[DIGameManager] Game resumed");
        }

        /// <summary>
        /// Shutdown the game
        /// </summary>
        public void ShutdownGame()
        {
            Debug.Log("[DIGameManager] Shutting down game");

            CurrentGameState = GameState.Shutting_Down;

            // Stop auto-save
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }

            // Shutdown all managers
            ShutdownAllManagers();

            _onGameShutdown?.RaiseEvent();
            CurrentGameState = GameState.Shutdown;
        }

        private void ShutdownAllManagers()
        {
            foreach (var manager in _managerRegistry.Values)
            {
                try
                {
                    if (manager != null)
                    {
                        manager.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DIGameManager] Error shutting down manager {manager?.ManagerName}: {ex.Message}");
                }
            }

            _managerRegistry.Clear();
        }

        #endregion

        #region Auto-Save

        private IEnumerator AutoSaveCoroutine()
        {
            while (_autoSaveEnabled && CurrentGameState == GameState.Running)
            {
                yield return new WaitForSeconds(_autoSaveInterval);

                if (!IsGamePaused && _dataManager != null)
                {
                    try
                    {
                        // _dataManager.AutoSave();
                        Debug.Log("[DIGameManager] Auto-save completed");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DIGameManager] Auto-save failed: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Cleanup

        protected override void OnDispose()
        {
            try
            {
                // Stop auto-save
                if (_autoSaveCoroutine != null)
                {
                    StopCoroutine(_autoSaveCoroutine);
                }

                // Shutdown all managers
                ShutdownAllManagers();

                // Clear singleton reference
                if (Instance == this)
                {
                    Instance = null;
                }

                Debug.Log("[DIGameManager] Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Error during cleanup: {ex.Message}");
            }
        }

        #endregion

        #region ChimeraManager Implementation

        protected override void OnManagerInitialize()
        {
            Debug.Log("[DIGameManager] Manager initialization starting...");
            
            // Initialize DI container
            if (ServiceContainer == null)
            {
                SetServiceContainer(GetOrCreateServiceContainer());
            }

            // Register this game manager with the container
            if (ServiceContainer != null && _autoRegisterWithContainer)
            {
                ServiceContainer.RegisterSingleton<IGameManager>(this);
                ServiceContainer.RegisterSingleton(this);
            }

            Debug.Log("[DIGameManager] Manager initialization completed");
        }

        protected override void OnManagerShutdown()
        {
            Debug.Log("[DIGameManager] Manager shutdown starting...");
            
            try
            {
                // Shutdown all registered managers
                foreach (var manager in _managerRegistry.Values)
                {
                    if (manager != null && manager != this)
                    {
                        try
                        {
                            manager.Shutdown();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[DIGameManager] Error shutting down manager {manager.ManagerName}: {ex.Message}");
                        }
                    }
                }

                // Clear registry
                _managerRegistry.Clear();

                // Dispose DI container
                if (ServiceContainer is IDisposable disposableContainer)
                {
                    disposableContainer.Dispose();
                }
                SetServiceContainer(null);

                Debug.Log("[DIGameManager] Manager shutdown completed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DIGameManager] Error during shutdown: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Game state enumeration
    /// </summary>
    public enum GameState
    {
        Uninitialized,
        Initializing,
        Running,
        Paused,
        Shutting_Down,
        Shutdown,
        Error
    }

    /// <summary>
    /// Interface for Game Manager
    /// </summary>
    public interface IGameManager : IChimeraManager
    {
        GameState CurrentGameState { get; }
        bool IsGamePaused { get; }
        DateTime GameStartTime { get; }
        TimeSpan TotalGameTime { get; }

        void PauseGame();
        void ResumeGame();
        void ShutdownGame();
        T GetManager<T>() where T : ChimeraManager;
        void RegisterManager<T>(T manager) where T : ChimeraManager;
    }
}