using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Central game state coordinator for Project Chimera.
    /// Manages overall game flow, system initialization, and state transitions.
    /// </summary>
    public class GameManager : ChimeraManager
    {
        [Header("Game State Configuration")]
        [SerializeField] private GameStateConfigSO _gameStateConfig;
        [SerializeField] private bool _loadLastSaveOnStart = true;
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveInterval = 300.0f; // 5 minutes

        [Header("Manager References")]
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private EventManager _eventManager;
        [SerializeField] private SettingsManager _settingsManager;
        

        [Header("Game State Events")]
        [SerializeField] private SimpleGameEventSO _onGameInitialized;
        [SerializeField] private SimpleGameEventSO _onGamePaused;
        [SerializeField] private SimpleGameEventSO _onGameResumed;
        [SerializeField] private SimpleGameEventSO _onGameShutdown;

        // Manager registry for dynamic access
        private readonly Dictionary<System.Type, ChimeraManager> _managerRegistry = new Dictionary<System.Type, ChimeraManager>();
        private Coroutine _autoSaveCoroutine;

        /// <summary>
        /// Singleton instance of the GameManager.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Current game state.
        /// </summary>
        public GameState CurrentGameState { get; private set; } = GameState.Uninitialized;

        /// <summary>
        /// Whether the game is currently paused.
        /// </summary>
        public bool IsGamePaused { get; private set; }

        /// <summary>
        /// Time when the game was started.
        /// </summary>
        public System.DateTime GameStartTime { get; private set; }

        /// <summary>
        /// Total time the game has been running.
        /// </summary>
        public System.TimeSpan TotalGameTime => System.DateTime.Now - GameStartTime;

        protected override void Awake()
        {
            // Implement singleton pattern
            if (Instance != null && Instance != this)
            {
                LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            base.Awake();
        }

        protected override void OnManagerInitialize()
        {
            LogDebug("Initializing Game Manager");

            // Initialize audio focus handler to prevent FMOD errors
            InitializeAudioFocusHandler();

            // Set game start time
            GameStartTime = System.DateTime.Now;

            // Initialize core systems in order
            StartCoroutine(InitializeGameSystems());
        }

        protected override void OnManagerShutdown()
        {
            LogDebug("Shutting down Game Manager");

            // Stop auto-save
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }

            // Shutdown all managers in reverse order
            ShutdownAllManagers();

            // Raise shutdown event
            _onGameShutdown?.Raise();

            // Clear singleton reference
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Initializes all game systems in the correct order.
        /// </summary>
        private IEnumerator InitializeGameSystems()
        {
            ChangeGameState(GameState.Initializing);

            yield return StartCoroutine(InitializeManager(_settingsManager, "Settings"));
            yield return StartCoroutine(InitializeManager(_eventManager, "Event"));
            yield return StartCoroutine(InitializeManager(_dataManager, "Data"));
            yield return StartCoroutine(InitializeManager(_timeManager, "Time"));
            
            // Register all managers
            RegisterManager(_settingsManager);
            RegisterManager(_eventManager);
            RegisterManager(_dataManager);
            RegisterManager(_timeManager);

            // Load game data
            yield return StartCoroutine(LoadGameData());

            // Start auto-save if enabled
            if (_autoSaveEnabled)
            {
                _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }

            // Game is now ready
            ChangeGameState(GameState.Running);
            _onGameInitialized?.Raise();

            LogDebug("Game Manager initialization complete");
        }

        /// <summary>
        /// Initializes a specific manager with error handling.
        /// </summary>
        private IEnumerator InitializeManager(ChimeraManager manager, string managerName)
        {
            if (manager == null)
            {
                LogWarning($"{managerName} Manager not assigned");
                yield break;
            }

            LogDebug($"Initializing {managerName} Manager");
            
            try
            {
                manager.InitializeManager();
                
                if (!manager.IsInitialized)
                {
                    LogError($"Failed to initialize {managerName} Manager");
                }
            }
            catch (System.Exception e)
            {
                LogError($"Exception during {managerName} Manager initialization: {e.Message}");
            }
            
            yield return new WaitForEndOfFrame(); // Allow one frame for initialization
        }

        /// <summary>
        /// Loads game data (save file or new game).
        /// </summary>
        private IEnumerator LoadGameData()
        {
            if (_loadLastSaveOnStart)
            {
                LogDebug("Loading last save file - will be handled by Systems.Save.SaveManager");
                // Save system will be accessed via manager registry when needed
            }
            else
            {
                LogDebug("Starting new game");
                // Initialize new game data
            }

            yield return null;
        }

        /// <summary>
        /// Auto-save coroutine that saves the game at regular intervals.
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveInterval);
                
                if (CurrentGameState == GameState.Running && !IsGamePaused)
                {
                    LogDebug("Performing auto-save - will be handled by Systems.Save.SaveManager");
                    // Save system will be accessed via manager registry when needed
                }
            }
        }

        /// <summary>
        /// Changes the current game state and notifies systems.
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            if (CurrentGameState == newState) return;

            GameState previousState = CurrentGameState;
            CurrentGameState = newState;

            LogDebug($"Game state changed: {previousState} -> {newState}");

            // Notify all systems of state change
            OnGameStateChanged(previousState, newState);
        }

        /// <summary>
        /// Called when the game state changes. Override to handle state transitions.
        /// </summary>
        protected virtual void OnGameStateChanged(GameState previousState, GameState newState)
        {
            // Notify all registered managers
            foreach (var manager in _managerRegistry.Values)
            {
                if (manager is IGameStateListener listener)
                {
                    listener.OnGameStateChanged(previousState, newState);
                }
            }
        }

        /// <summary>
        /// Registers a manager in the registry for dynamic access.
        /// </summary>
        public void RegisterManager<T>(T manager) where T : ChimeraManager
        {
            if (manager == null) return;

            var type = typeof(T);
            _managerRegistry[type] = manager;
            LogDebug($"Registered manager: {type.Name}");
        }

        /// <summary>
        /// Gets a manager by type from the registry.
        /// </summary>
        public T GetManager<T>() where T : ChimeraManager
        {
            var type = typeof(T);
            return _managerRegistry.TryGetValue(type, out ChimeraManager manager) ? manager as T : null;
        }
        
        /// <summary>
        /// Gets a manager by its type name (for avoiding circular assembly dependencies).
        /// </summary>
        public ChimeraManager GetManagerByName(string typeName)
        {
            foreach (var kvp in _managerRegistry)
            {
                if (kvp.Key.Name == typeName)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Pauses the game and all systems.
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused) return;

            IsGamePaused = true;
            Time.timeScale = 0.0f;

            // Pause all managers that support pausing
            foreach (var manager in _managerRegistry.Values)
            {
                if (manager is IPausable pausable)
                {
                    pausable.Pause();
                }
            }

            _onGamePaused?.Raise();
            LogDebug("Game paused");
        }

        /// <summary>
        /// Resumes the game and all systems.
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused) return;

            IsGamePaused = false;
            Time.timeScale = 1.0f;

            // Resume all managers that support pausing
            foreach (var manager in _managerRegistry.Values)
            {
                if (manager is IPausable pausable)
                {
                    pausable.Resume();
                }
            }

            _onGameResumed?.Raise();
            LogDebug("Game resumed");
        }

        /// <summary>
        /// Performs a manual save of the game.
        /// </summary>
        public void ManualSave()
        {
            if (CurrentGameState == GameState.Running)
            {
                LogDebug("Performing manual save - will be handled by Systems.Save.SaveManager");
                // Save system will be accessed via manager registry when needed
            }
        }

        /// <summary>
        /// Shuts down all registered managers.
        /// </summary>
        private void ShutdownAllManagers()
        {
            // Shutdown in reverse order of initialization
            var managers = new List<ChimeraManager>(_managerRegistry.Values);
            for (int i = managers.Count - 1; i >= 0; i--)
            {
                try
                {
                    managers[i]?.ShutdownManager();
                }
                catch (System.Exception e)
                {
                    LogError($"Exception during manager shutdown: {e.Message}");
                }
            }

            _managerRegistry.Clear();
        }

        /// <summary>
        /// Called when the application is being quit.
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !IsGamePaused)
            {
                PauseGame();
            }
            else if (!pauseStatus && IsGamePaused)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// Initializes the AudioFocusHandler to prevent FMOD System::init errors
        /// </summary>
        private void InitializeAudioFocusHandler()
        {
            // Check if AudioFocusHandler already exists
            var existingHandler = FindObjectOfType<AudioFocusHandler>();
            if (existingHandler == null)
            {
                // Create a new GameObject with AudioFocusHandler component
                var audioHandlerObject = new GameObject("AudioFocusHandler");
                audioHandlerObject.AddComponent<AudioFocusHandler>();
                LogInfo("AudioFocusHandler created to prevent FMOD errors");
            }
            else
            {
                LogInfo("AudioFocusHandler already exists");
            }
        }

        /// <summary>
        /// Called when the application focus changes.
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !IsGamePaused && _gameStateConfig?.PauseOnLostFocus == true)
            {
                PauseGame();
            }
            
            // Prevent FMOD audio reinitialization issues
            if (!hasFocus)
            {
                // Ensure audio system doesn't reinitialize when losing focus
                Application.runInBackground = true;
            }
        }
    }

    /// <summary>
    /// Possible game states for Project Chimera.
    /// </summary>
    public enum GameState
    {
        Uninitialized,  // Game has not been initialized yet
        Initializing,   // Game is currently initializing
        Running,        // Game is running normally
        Paused,         // Game is paused
        Loading,        // Game is loading data
        Saving,         // Game is saving data
        Error,          // Game encountered an error
        Shutdown        // Game is shutting down
    }

    /// <summary>
    /// Interface for managers that need to respond to game state changes.
    /// </summary>
    public interface IGameStateListener
    {
        void OnGameStateChanged(GameState previousState, GameState newState);
    }

    /// <summary>
    /// Interface for systems that can be paused and resumed.
    /// </summary>
    public interface IPausable
    {
        void Pause();
        void Resume();
    }

    /// <summary>
    /// Configuration for game state behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "Game State Config", menuName = "Project Chimera/Core/Game State Config")]
    public class GameStateConfigSO : ChimeraConfigSO
    {
        [Header("Pause Behavior")]
        public bool PauseOnLostFocus = true;
        public bool PauseOnApplicationPause = true;

        [Header("Auto-Save Configuration")]
        public bool AutoSaveEnabled = true;
        public float AutoSaveInterval = 300.0f; // 5 minutes
        public int MaxAutoSaves = 10;

        [Header("Performance Settings")]
        public bool EnablePerformanceMonitoring = true;
        public float PerformanceUpdateInterval = 1.0f;
    }
}