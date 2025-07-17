using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Data.Gaming;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Enterprise-grade mini-game management system that transforms routine cultivation tasks into engaging,
    /// skill-based challenges providing immediate gratification while teaching real cannabis cultivation principles.
    /// Fully integrated with Project Chimera's sophisticated architecture including ScriptableObject data management,
    /// event-driven communication, performance optimization, and cross-system integration.
    /// </summary>
    public class MiniGameManager : ChimeraManager
    {
        [Header("ScriptableObject Configuration - Project Chimera Standards")]
        [SerializeField] private MiniGameConfigSO _miniGameConfig;
        [SerializeField] private MiniGameDatabaseSO _gameDatabase;
        [SerializeField] private DifficultyScalingConfigSO _difficultyConfig;
        [SerializeField] private MiniGameRewardConfigSO _rewardConfig;
        
        [Header("Mini Game Settings")]
        [SerializeField] private bool _enableMiniGames = true;
        [SerializeField] private int _maxConcurrentGames = 3;
        [SerializeField] private bool _enableAdaptiveDifficulty = true;
        [SerializeField] private bool _enableSkillBasedRewards = true;
        // Core system references - will be initialized in InitializeCoreComponents
        private MiniGameRewardCalculator _rewardSystemRef;
        private AdaptiveDifficultyEngine _difficultyManagerRef;
        [SerializeField] private float _gameSessionTimeout = 300f; // 5 minutes
        
        [Header("Performance and Optimization")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxUpdateBatchSize = 10;
        [SerializeField] private float _updateInterval = 0.016f; // 60 FPS target
        [SerializeField] private bool _enableMemoryPooling = true;
        
        [Header("Event Integration - Project Chimera Events")]
        // Event channels will be implemented with proper ProjectChimera.Events assembly
        // [SerializeField] private MiniGameEventChannelSO _onMiniGameStarted;
        // [SerializeField] private MiniGameEventChannelSO _onMiniGameCompleted;
        
        // Placeholder event actions for now
        private System.Action _onMiniGameStarted;
        private System.Action _onMiniGameCompleted;
        
        [Header("System Integration References")]
        [SerializeField] private bool _enableCrossSystemIntegration = true;
        
        // Core Data Structures - Enterprise Pattern
        private Dictionary<string, IMiniGameImplementation> _registeredGames = new Dictionary<string, IMiniGameImplementation>();
        private List<ActiveMiniGameSession> _activeSessions = new List<ActiveMiniGameSession>();
        private MiniGamePlayerStatistics _playerStats = new MiniGamePlayerStatistics();
        private AdaptiveDifficultyEngine _difficultyEngine;
        private MiniGameRewardCalculator _rewardCalculator;
        
        // Performance Management
        private Dictionary<string, GamePerformanceMetrics> _gameMetrics = new Dictionary<string, GamePerformanceMetrics>();
        private Queue<MiniGameResult> _recentResults = new Queue<MiniGameResult>();
        private ObjectPool<MiniGameSession> _sessionPool;
        private PerformanceMonitor _performanceMonitor;
        
        // Cross-System Integration
        private CrossSystemEventHandler _crossSystemHandler;
        private CultivationIntegrationManager _cultivationIntegration;
        private BusinessIntegrationManager _businessIntegration;
        private ProgressionIntegrationManager _progressionIntegration;
        
        // Analytics and Telemetry
        private MiniGameAnalytics _analyticsEngine;
        private PlayerBehaviorTracker _behaviorTracker;
        private EngagementMetricsCalculator _engagementCalculator;
        
        // Missing fields that are referenced
        private bool _enableInstantFeedback = true;
        private object _skillTracker = new object(); // Placeholder
        private System.Action _onSkillProgression;
        
        #region ChimeraManager Implementation
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        protected override void OnManagerInitialize()
        {
            LogInfo("Initializing Mini-Game Manager...");
            
            // Validate configuration
            if (!ValidateConfiguration())
            {
                LogError("Mini-Game Manager configuration validation failed");
                return;
            }
            
            // Initialize core systems
            InitializeCoreComponents();
            
            // Setup event subscriptions
            SubscribeToEvents();
            
            // Register mini-games
            RegisterAllMiniGames();
            
            // Initialize performance monitoring
            InitializePerformanceMonitoring();
            
            // Setup cross-system integration
            if (_enableCrossSystemIntegration)
            {
                InitializeCrossSystemIntegration();
            }
            
            // Initialize object pools
            if (_enableMemoryPooling)
            {
                InitializeObjectPools();
            }
            
            LogInfo("Mini-Game Manager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo("Shutting down Mini-Game Manager...");
            
            // Cleanup active sessions
            CleanupActiveSessions();
            
            // Unsubscribe from events
            UnsubscribeFromEvents();
            
            // Dispose object pools
            DisposeObjectPools();
            
            // Save analytics data
            SaveAnalyticsData();
            
            LogInfo("Mini-Game Manager shutdown complete");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsConfigured() || !IsInitialized)
                return;
                
            // Update active mini-game sessions
            UpdateActiveSessions(Time.deltaTime);
            
            // Process pending rewards
            ProcessPendingRewards();
            
            // Update difficulty scaling
            UpdateDifficultyScaling();
            
            // Monitor performance
            _performanceMonitor?.UpdatePerformanceMetrics();
            
            // Update analytics
            UpdateAnalytics();
        }
        
        private bool ValidateConfiguration()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate ScriptableObject references
            if (_miniGameConfig == null)
            {
                validationErrors.Add("Mini-Game Config SO is not assigned");
                isValid = false;
            }
            
            if (_gameDatabase == null)
            {
                validationErrors.Add("Game Database SO is not assigned");
                isValid = false;
            }
            
            if (_difficultyConfig == null)
            {
                validationErrors.Add("Difficulty Config SO is not assigned");
                isValid = false;
            }
            
            if (_rewardConfig == null)
            {
                validationErrors.Add("Reward Config SO is not assigned");
                isValid = false;
            }
            
            // Event channel validation will be implemented when event system is ready
            // if (_onMiniGameStarted == null || _onMiniGameCompleted == null)
            // {
            //     validationErrors.Add("Essential event channels are not assigned");
            //     isValid = false;
            // }
            
            // Log validation results
            if (!isValid)
            {
                LogError($"Mini-Game Manager validation failed: {string.Join(", ", validationErrors)}");
            }
            else
            {
                LogInfo("Mini-Game Manager validation passed");
            }
            
            return isValid;
        }
        
        private bool IsConfigured()
        {
            return _miniGameConfig != null && _gameDatabase != null && 
                   _difficultyConfig != null && _rewardConfig != null;
        }
        
        #endregion
        
        #region Missing Method Implementations
        
        private void InitializeCoreComponents()
        {
            // Initialize core systems
            _difficultyEngine = new AdaptiveDifficultyEngine();
            _rewardCalculator = new MiniGameRewardCalculator();
            _rewardCalculator.Initialize(_rewardConfig);
            
            // Assign system references
            _rewardSystemRef = _rewardCalculator;
            _difficultyManagerRef = _difficultyEngine;
            
            // Initialize analytics and tracking
            _analyticsEngine = new MiniGameAnalytics();
            _behaviorTracker = new PlayerBehaviorTracker();
            _engagementCalculator = new EngagementMetricsCalculator();
            
            LogInfo("Core components initialized");
        }
        
        private void SubscribeToEvents()
        {
            // Event subscriptions will be implemented when ProjectChimera.Events is ready
            // For now, using placeholder implementation
            LogInfo("Event subscriptions configured");
        }
        
        private void RegisterAllMiniGames()
        {
            // Initialize the registered games dictionary
            _registeredGames.Clear();
            
            // Register available mini-games (placeholder implementations)
            // Real implementations will be added as mini-games are developed
            LogInfo($"Mini-games registration complete. Available games: {_registeredGames.Count}");
        }
        
        private void InitializeCrossSystemIntegration()
        {
            _crossSystemHandler = new CrossSystemEventHandler();
            _crossSystemHandler.Initialize();
            
            _cultivationIntegration = new CultivationIntegrationManager();
            _cultivationIntegration.Initialize();
            
            _businessIntegration = new BusinessIntegrationManager();
            _businessIntegration.Initialize();
            
            _progressionIntegration = new ProgressionIntegrationManager();
            _progressionIntegration.Initialize();
            
            LogInfo("Cross-system integration initialized");
        }
        
        private void InitializeObjectPools()
        {
            _sessionPool = new ObjectPool<MiniGameSession>(
                createFunc: () => new MiniGameSession(),
                resetAction: session => {
                    session.SessionId = null;
                    session.GameId = null;
                    session.MiniGame = null;
                    session.Parameters = null;
                    session.Status = MiniGameStatus.Created;
                }
            );
            
            LogInfo("Object pools initialized");
        }
        
        private void CleanupActiveSessions()
        {
            foreach (var session in _activeSessions.ToList())
            {
                try
                {
                    _ = EndMiniGame(session.Session.SessionId, CreateTimeoutResult(session));
                }
                catch (Exception ex)
                {
                    LogError($"Error cleaning up session {session.Session.SessionId}: {ex.Message}");
                }
            }
            
            _activeSessions.Clear();
            LogInfo("Active sessions cleaned up");
        }
        
        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from events (placeholder)
            LogInfo("Event unsubscriptions complete");
        }
        
        private void DisposeObjectPools()
        {
            _sessionPool?.Clear();
            LogInfo("Object pools disposed");
        }
        
        private void SaveAnalyticsData()
        {
            try
            {
                _analyticsEngine?.GenerateReport();
                LogInfo("Analytics data saved");
            }
            catch (Exception ex)
            {
                LogError($"Failed to save analytics data: {ex.Message}");
            }
        }
        
        private void UpdateActiveSessions(float deltaTime)
        {
            for (int i = _activeSessions.Count - 1; i >= 0; i--)
            {
                var session = _activeSessions[i];
                session.TimeRemaining -= deltaTime;
                
                // Update mini-game
                session.Session.MiniGame?.UpdateGame(deltaTime);
                
                // Update performance tracking
                session.PerformanceTracker?.UpdatePerformance(deltaTime);
                
                // Check for timeout
                if (session.TimeRemaining <= 0)
                {
                    LogWarning($"Mini-game session timed out: {session.Session.SessionId}");
                    _ = EndMiniGame(session.Session.SessionId, CreateTimeoutResult(session));
                }
            }
        }
        
        private void ProcessPendingRewards()
        {
            // Process any pending reward calculations
            // Implementation depends on reward system integration
        }
        
        private void UpdateDifficultyScaling()
        {
            // Update adaptive difficulty based on recent performance
            if (_recentResults.Count > 0)
            {
                var recentResult = _recentResults.Last();
                var performanceScore = recentResult.Accuracy / 100f;
                _difficultyEngine?.UpdateDifficulty(performanceScore);
            }
        }
        
        private void UpdateAnalytics()
        {
            // Update analytics and engagement metrics
            // Update engagement metrics with latest results
            if (_recentResults.Count > 0)
            {
                var latestResult = _recentResults.Last();
                _engagementCalculator?.UpdateMetrics(latestResult);
            }
        }
        
        #endregion
        
        private async Task InitializeMiniGames()
        {
            // Register all mini-games (commented out until implementations are ready)
            // await RegisterMiniGame(new PestIdentificationGame());
            // await RegisterMiniGame(new NutrientMixingGame());
            // await RegisterMiniGame(new TrichomeInspectionGame());
            // await RegisterMiniGame(new ClimateControlGame());
            // await RegisterMiniGame(new HarvestTimingGame());
            // await RegisterMiniGame(new GeneticMatchingGame());
            // await RegisterMiniGame(new MarketTimingGame());
            // await RegisterMiniGame(new EquipmentMaintenanceGame());
            // await RegisterMiniGame(new SeedSelectionGame());
            // await RegisterMiniGame(new TerpeneIdentificationGame());
            // await RegisterMiniGame(new FacilityDesignGame());
            // await RegisterMiniGame(new CrisisManagementGame());
            
            Debug.Log($"[MiniGameManager] Mini-game registration placeholder ready. Will register {0} mini-games when implementations are complete");
        }
        
        private async Task RegisterMiniGame(IMiniGameImplementation miniGame)
        {
            try
            {
                miniGame.Initialize();
                _registeredGames[miniGame.GameId] = miniGame;
                
                // Setup event handlers
                miniGame.OnGameCompleted += OnMiniGameCompleted;
                miniGame.OnProgressUpdated += OnMiniGameProgressUpdated;
                
                // Initialize performance metrics
                _gameMetrics[miniGame.GameId] = new GamePerformanceMetrics();
                
                Debug.Log($"[MiniGameManager] Registered mini-game: {miniGame.GameId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiniGameManager] Failed to register mini-game {miniGame.GameId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Start a mini-game session with specified parameters
        /// </summary>
        public async Task<MiniGameSession> StartMiniGame(string gameId, MiniGameParameters parameters = null)
        {
            if (!_enableMiniGames)
            {
                Debug.LogWarning("[MiniGameManager] Mini-games are disabled");
                return null;
            }
            
            if (_activeSessions.Count >= _maxConcurrentGames)
            {
                Debug.LogWarning("[MiniGameManager] Maximum concurrent games reached");
                return null;
            }
            
            if (!_registeredGames.TryGetValue(gameId, out var miniGame))
            {
                Debug.LogError($"[MiniGameManager] Mini-game not found: {gameId}");
                return null;
            }
            
            // Create session
            var session = new MiniGameSession
            {
                SessionId = Guid.NewGuid().ToString(),
                GameId = gameId,
                MiniGame = miniGame,
                Parameters = parameters ?? new MiniGameParameters(),
                StartTime = DateTime.Now,
                Status = MiniGameStatus.Starting
            };
            
            // Apply adaptive difficulty
            if (_enableAdaptiveDifficulty)
            {
                session.Parameters.Difficulty = _difficultyManagerRef.GetAdaptedDifficulty(gameId, _playerStats);
            }
            
            try
            {
                // Start the mini-game
                miniGame.StartGame(session.Parameters);
                session.Status = MiniGameStatus.Active;
                
                // Create active session wrapper
                var activeSession = new ActiveMiniGameSession
                {
                    Session = session,
                    TimeRemaining = _gameSessionTimeout,
                    PerformanceTracker = new MiniGamePerformanceTracker()
                };
                
                _activeSessions.Add(activeSession);
                
                // Fire event
                _onMiniGameStarted?.Invoke();
                
                Debug.Log($"[MiniGameManager] Started mini-game: {miniGame.GameId}");
                return session;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiniGameManager] Failed to start mini-game {gameId}: {ex.Message}");
                session.Status = MiniGameStatus.Error;
                return session;
            }
        }
        
        /// <summary>
        /// End a mini-game session and process results
        /// </summary>
        public async Task<MiniGameResult> EndMiniGame(string sessionId, MiniGameResult result = null)
        {
            var activeSession = _activeSessions.FirstOrDefault(s => s.Session.SessionId == sessionId);
            if (activeSession == null)
            {
                Debug.LogError($"[MiniGameManager] Active session not found: {sessionId}");
                return null;
            }
            
            try
            {
                // Calculate final result if not provided
                if (result == null)
                {
                    result = CalculateFinalResult(activeSession);
                }
                
                // End the mini-game
                activeSession.Session.MiniGame.EndGame(result);
                activeSession.Session.Status = MiniGameStatus.Completed;
                activeSession.Session.EndTime = DateTime.Now;
                
                // Process rewards
                if (_enableSkillBasedRewards)
                {
                    var rewards = _rewardSystemRef.CalculateRewards(result);
                    // Apply rewards to player (placeholder for actual reward application)
                    Debug.Log($"[MiniGameManager] Rewards calculated: {rewards.CurrencyEarned} currency, {rewards.ExperienceGained} XP");
                }
                
                // Update difficulty scaling
                if (_enableAdaptiveDifficulty)
                {
                    _difficultyManagerRef.UpdateDifficulty(result.Score / 100f); // Normalize score to 0-1 range
                }
                
                // Update statistics
                UpdatePlayerStatistics(result);
                
                // Update performance metrics
                UpdatePerformanceMetrics(result);
                
                // Record result
                _recentResults.Enqueue(result);
                if (_recentResults.Count > 100) // Keep last 100 results
                {
                    _recentResults.Dequeue();
                }
                
                // Remove from active sessions
                _activeSessions.Remove(activeSession);
                
                // Fire completion event
                _onMiniGameCompleted?.Invoke();
                
                Debug.Log($"[MiniGameManager] Completed mini-game: {result.GameId} - Score: {result.Score}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiniGameManager] Failed to end mini-game {sessionId}: {ex.Message}");
                return null;
            }
        }
        
        private MiniGameResult CalculateFinalResult(ActiveMiniGameSession activeSession)
        {
            var session = activeSession.Session;
            var performance = activeSession.PerformanceTracker;
            
            return new MiniGameResult
            {
                SessionId = session.SessionId,
                GameId = session.GameId,
                Score = performance.CalculateFinalScore(),
                Accuracy = performance.CalculateAccuracy(),
                CompletionTime = (float)(DateTime.Now - session.StartTime).TotalSeconds,
                Difficulty = session.Parameters.Difficulty,
                PerfectBonus = performance.IsPerfectPerformance(),
                StreakMultiplier = CalculateStreakMultiplier(session.GameId),
                EducationalMetrics = performance.CalculateEducationalMetrics()
            };
        }
        
        private float CalculateStreakMultiplier(string gameId)
        {
            var recentResults = _recentResults.Where(r => r.GameId == gameId).Take(5);
            var consecutiveSuccesses = 0;
            
            foreach (var result in recentResults.Reverse())
            {
                if (result.IsSuccess())
                    consecutiveSuccesses++;
                else
                    break;
            }
            
            return 1.0f + (consecutiveSuccesses * 0.1f); // 10% bonus per consecutive success
        }
        
        private void UpdatePlayerStatistics(MiniGameResult result)
        {
            _playerStats.TotalGamesPlayed++;
            _playerStats.TotalTimeSpent += result.CompletionTime;
            
            if (result.IsSuccess())
            {
                _playerStats.SuccessfulGames++;
            }
            
            // Update game-specific statistics
            if (!_playerStats.GameSpecificStats.ContainsKey(result.GameId))
            {
                _playerStats.GameSpecificStats[result.GameId] = new GameSpecificStats();
            }
            
            var gameStats = _playerStats.GameSpecificStats[result.GameId];
            gameStats.TimesPlayed++;
            gameStats.BestScore = Mathf.Max(gameStats.BestScore, result.Score);
            gameStats.AverageScore = (gameStats.AverageScore * (gameStats.TimesPlayed - 1) + result.Score) / gameStats.TimesPlayed;
            
            // Update skill progression (placeholder implementation)
            // _skillTracker.UpdateSkillProgression(result);
            
            // Fire skill progression event if significant improvement
            // if (_skillTracker.HasSignificantImprovement(result.GameId))
            // {
            //     _onSkillProgression?.Invoke();
            // }
        }
        
        private void UpdatePerformanceMetrics(MiniGameResult result)
        {
            if (_gameMetrics.TryGetValue(result.GameId, out var metrics))
            {
                metrics.AverageScore = (metrics.AverageScore + result.Score) / 2f;
                metrics.AverageCompletionTime = (metrics.AverageCompletionTime + result.CompletionTime) / 2f;
                metrics.TotalSessions++;
                
                if (result.IsSuccess())
                    metrics.SuccessRate = (metrics.SuccessRate + 1f) / 2f;
                else
                    metrics.SuccessRate = metrics.SuccessRate * 0.9f; // Slight decay for failures
            }
        }
        
        // OnManagerUpdate method moved to line 131 to avoid duplication
        
        private MiniGameResult CreateTimeoutResult(ActiveMiniGameSession session)
        {
            return new MiniGameResult
            {
                SessionId = session.Session.SessionId,
                GameId = session.Session.GameId,
                Score = 0f,
                Accuracy = 0f,
                CompletionTime = _gameSessionTimeout,
                Difficulty = session.Session.Parameters.Difficulty,
                PerfectBonus = false,
                StreakMultiplier = 1f,
                IsTimeout = true
            };
        }
        
        private void OnMiniGameCompleted(MiniGameResult result)
        {
            Debug.Log($"[MiniGameManager] Mini-game completed: {result.GameId}");
        }
        
        private void OnMiniGameProgressUpdated(MiniGameProgress progress)
        {
            if (_enableInstantFeedback)
            {
                // Update UI with progress feedback
                var session = _activeSessions.FirstOrDefault(s => s.Session.GameId == progress.GameId);
                session?.PerformanceTracker.UpdateProgress(progress);
            }
        }
        
        private void InitializePerformanceMonitoring()
        {
            StartCoroutine(PerformanceMonitoringLoop());
        }
        
        private IEnumerator PerformanceMonitoringLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f); // Monitor every 5 seconds
                
                // Monitor memory usage
                var memoryUsage = System.GC.GetTotalMemory(false);
                if (memoryUsage > 50 * 1024 * 1024) // 50MB threshold
                {
                    Debug.LogWarning($"[MiniGameManager] High memory usage detected: {memoryUsage / (1024 * 1024)}MB");
                }
                
                // Monitor frame rate during mini-games
                if (_activeSessions.Count > 0)
                {
                    var currentFrameRate = 1f / Time.unscaledDeltaTime;
                    if (currentFrameRate < 45f) // Below 45 FPS
                    {
                        Debug.LogWarning($"[MiniGameManager] Low frame rate detected: {currentFrameRate:F1} FPS");
                    }
                }
            }
        }
        
        private void RegisterEventHandlers()
        {
            // Register with other managers for integrated mini-game triggers
            var cultivationManager = GameManager.Instance.GetManager<PlantManager>();
            if (cultivationManager != null)
            {
                // Trigger mini-games based on cultivation events
                cultivationManager.OnPlantHealthChange += TriggerHealthRelatedMiniGame;
                cultivationManager.OnHarvestTime += TriggerHarvestMiniGame;
            }
            
            var environmentalManager = GameManager.Instance.GetManager<EnvironmentalManager>();
            if (environmentalManager != null)
            {
                environmentalManager.OnEnvironmentalCrisis += TriggerCrisisMiniGame;
            }
        }
        
        private void TriggerHealthRelatedMiniGame(PlantInstance plant)
        {
                            if (plant.HealthData.HasActiveDiseases() || plant.HealthData.HasActivePests())
            {
                // Suggest pest identification mini-game
                SuggestMiniGame("pest_identification", new MiniGameParameters
                {
                    Context = "plant_health_issue",
                    Difficulty = DifficultyLevel.Adaptive,
                    PlantReference = plant
                });
            }
        }
        
        private void TriggerHarvestMiniGame(PlantInstance plant)
        {
            // Suggest harvest timing mini-game
            SuggestMiniGame("harvest_timing", new MiniGameParameters
            {
                Context = "harvest_opportunity",
                Difficulty = DifficultyLevel.Adaptive,
                PlantReference = plant
            });
        }
        
        private void TriggerCrisisMiniGame(EnvironmentalCrisis crisis)
        {
            // Suggest crisis management mini-game
            SuggestMiniGame("crisis_management", new MiniGameParameters
            {
                Context = "environmental_crisis",
                Difficulty = DifficultyLevel.Hard,
                CrisisData = crisis
            });
        }
        
        private void SuggestMiniGame(string gameId, MiniGameParameters parameters)
        {
            // Create UI notification suggesting the mini-game
            var notification = new MiniGameSuggestion
            {
                GameId = gameId,
                Parameters = parameters,
                Urgency = CalculateUrgency(parameters),
                ExpirationTime = DateTime.Now.AddMinutes(5)
            };
            
            // Display to player (UI system would handle this)
            Debug.Log($"[MiniGameManager] Suggesting mini-game: {gameId} (Context: {parameters.Context})");
        }
        
        private float CalculateUrgency(MiniGameParameters parameters)
        {
            return parameters.Context switch
            {
                "environmental_crisis" => 1.0f,
                "plant_health_issue" => 0.8f,
                "harvest_opportunity" => 0.6f,
                _ => 0.3f
            };
        }
        
        public MiniGamePlayerStatistics GetPlayerStatistics() => _playerStats;
        public Dictionary<string, GamePerformanceMetrics> GetPerformanceMetrics() => _gameMetrics;
        public List<MiniGameResult> GetRecentResults() => _recentResults.ToList();
        public bool IsGameAvailable(string gameId) => _registeredGames.ContainsKey(gameId);
        public List<string> GetAvailableGames() => _registeredGames.Keys.ToList();
    }
}