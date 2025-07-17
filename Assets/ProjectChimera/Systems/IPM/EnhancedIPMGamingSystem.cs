using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Enhanced IPM Gaming System with clean architecture patterns.
    /// Separates concerns through proper abstraction layers and dependency injection.
    /// Features modular design, comprehensive event handling, and robust testing support.
    /// 
    /// Phase 3.2 IPM Gaming System Rebuild - Clean Architecture Implementation
    /// </summary>
    public class EnhancedIPMGamingSystem : ChimeraManager
    {
        [Header("Gaming System Configuration")]
        [SerializeField] private bool _enableGamingFeatures = true;
        [SerializeField] private bool _enableAdvancedBattles = true;
        [SerializeField] private bool _enableTournamentMode = true;
        [SerializeField] private bool _enableProgressionSystem = true;
        [SerializeField] private bool _enableAnalytics = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentBattles = 5;
        [SerializeField] private float _battleUpdateInterval = 0.1f;
        [SerializeField] private int _maxPlayerHistoryEntries = 100;
        [SerializeField] private bool _enableBattleLogging = true;
        
        // Core gaming subsystems using clean architecture
        private IPMBattleEngine _battleEngine;
        private IPMPlayerProgressionService _progressionService;
        private IPMTournamentManager _tournamentManager;
        private IPMAnalyticsService _analyticsService;
        private IPMNotificationSystem _notificationSystem;
        
        // Domain services
        private IPMPestCatalogService _pestCatalogService;
        private IPMWeaponCatalogService _weaponCatalogService;
        private IPMStrategyService _strategyService;
        private IPMBalancingService _balancingService;
        
        // Data repositories
        private IPMBattleRepository _battleRepository;
        private IPMPlayerRepository _playerRepository;
        private IPMLeaderboardRepository _leaderboardRepository;
        
        // Event system
        private IPMGameEventBus _eventBus;
        
        // System state
        private bool _isSystemInitialized = false;
        private DateTime _lastSystemUpdate = DateTime.Now;
        private IPMEnhancedSystemMetrics _systemMetrics;
        
        public override string ManagerName => "Enhanced IPM Gaming System";
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public API properties
        public bool IsSystemReady => _isSystemInitialized && _battleEngine != null;
        public IPMEnhancedSystemMetrics SystemMetrics => _systemMetrics;
        public int ActiveBattlesCount => _battleRepository?.GetActiveBattlesCount() ?? 0;
        public int TotalPlayersCount => _playerRepository?.GetTotalPlayersCount() ?? 0;
        
        // Events for external integration
        public static event System.Action<IPMBattleResult> OnBattleCompleted;
        public static event System.Action<IPMPlayerAchievement> OnPlayerAchievement;
        public static event System.Action<IPMTournamentUpdate> OnTournamentUpdate;
        public static event System.Action<IPMSystemAlert> OnSystemAlert;
        
        protected override void OnManagerInitialize()
        {
            ChimeraLogger.Log("IPMGaming", "Initializing Enhanced IPM Gaming System...", this);
            
            try
            {
                InitializeSystemComponents();
                InitializeDomainServices();
                InitializeDataRepositories();
                InitializeEventSystem();
                InitializeSubsystems();
                
                _systemMetrics = new IPMEnhancedSystemMetrics();
                _isSystemInitialized = true;
                
                ChimeraLogger.Log("IPMGaming", "✅ Enhanced IPM Gaming System initialized successfully", this);
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogError("IPMGaming", $"Failed to initialize IPM Gaming System: {ex.Message}", this);
                _isSystemInitialized = false;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            ChimeraLogger.Log("IPMGaming", "Shutting down Enhanced IPM Gaming System...", this);
            
            try
            {
                ShutdownSubsystems();
                CleanupEventSystem();
                CleanupRepositories();
                
                _isSystemInitialized = false;
                
                ChimeraLogger.Log("IPMGaming", "✅ Enhanced IPM Gaming System shutdown completed", this);
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogError("IPMGaming", $"Error during IPM Gaming System shutdown: {ex.Message}", this);
            }
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_isSystemInitialized || !_enableGamingFeatures) return;
            
            var currentTime = DateTime.Now;
            if ((currentTime - _lastSystemUpdate).TotalSeconds >= _battleUpdateInterval)
            {
                UpdateSystemComponents();
                UpdateMetrics();
                _lastSystemUpdate = currentTime;
            }
        }
        
        #region System Initialization
        
        private void InitializeSystemComponents()
        {
            // Initialize core components with dependency injection pattern
            var config = CreateSystemConfiguration();
            
            _battleEngine = new IPMBattleEngine(config.BattleConfig);
            _progressionService = new IPMPlayerProgressionService(config.ProgressionConfig);
            _analyticsService = new IPMAnalyticsService(config.AnalyticsConfig);
            _notificationSystem = new IPMNotificationSystem(config.NotificationConfig);
            
            if (_enableTournamentMode)
            {
                _tournamentManager = new IPMTournamentManager(config.TournamentConfig);
            }
        }
        
        private void InitializeDomainServices()
        {
            _pestCatalogService = new IPMPestCatalogService();
            _weaponCatalogService = new IPMWeaponCatalogService();
            _strategyService = new IPMStrategyService();
            _balancingService = new IPMBalancingService();
            
            // Load game content
            _pestCatalogService.LoadPestCatalog();
            _weaponCatalogService.LoadWeaponCatalog();
            _strategyService.LoadStrategyCatalog();
        }
        
        private void InitializeDataRepositories()
        {
            _battleRepository = new IPMBattleRepository();
            _playerRepository = new IPMPlayerRepository();
            _leaderboardRepository = new IPMLeaderboardRepository();
            
            // Initialize repositories
            _battleRepository.Initialize();
            _playerRepository.Initialize();
            _leaderboardRepository.Initialize();
        }
        
        private void InitializeEventSystem()
        {
            _eventBus = new IPMGameEventBus();
            
            // Subscribe to domain events
            _eventBus.Subscribe<IPMBattleStartedEvent>(OnBattleStartedEvent);
            _eventBus.Subscribe<IPMBattleCompletedEvent>(OnBattleCompletedEvent);
            _eventBus.Subscribe<IPMPlayerLevelUpEvent>(OnPlayerLevelUpEvent);
            _eventBus.Subscribe<IPMTournamentStartedEvent>(OnTournamentStartedEvent);
            _eventBus.Subscribe<IPMSystemErrorEvent>(OnSystemErrorEvent);
        }
        
        private void InitializeSubsystems()
        {
            // Initialize battle engine
            _battleEngine.Initialize(_eventBus, _pestCatalogService, _weaponCatalogService, _strategyService);
            
            // Initialize progression service
            _progressionService.Initialize(_eventBus, _playerRepository);
            
            // Initialize analytics service
            if (_enableAnalytics)
            {
                _analyticsService.Initialize(_eventBus);
            }
            
            // Initialize tournament manager
            if (_enableTournamentMode && _tournamentManager != null)
            {
                _tournamentManager.Initialize(_eventBus, _leaderboardRepository);
            }
            
            // Initialize notification system
            _notificationSystem.Initialize(_eventBus);
        }
        
        private IPMSystemConfiguration CreateSystemConfiguration()
        {
            return new IPMSystemConfiguration
            {
                BattleConfig = new IPMBattleConfiguration
                {
                    MaxConcurrentBattles = _maxConcurrentBattles,
                    EnableAdvancedBattles = _enableAdvancedBattles,
                    BattleUpdateInterval = _battleUpdateInterval,
                    EnableBattleLogging = _enableBattleLogging
                },
                ProgressionConfig = new IPMProgressionConfiguration
                {
                    EnableProgressionSystem = _enableProgressionSystem,
                    MaxHistoryEntries = _maxPlayerHistoryEntries
                },
                AnalyticsConfig = new IPMAnalyticsConfiguration
                {
                    EnableAnalytics = _enableAnalytics,
                    DataRetentionDays = 30
                },
                TournamentConfig = new IPMTournamentConfiguration
                {
                    EnableTournaments = _enableTournamentMode,
                    MaxTournamentParticipants = 32
                },
                NotificationConfig = new IPMNotificationConfiguration
                {
                    EnableNotifications = true,
                    NotificationCooldownSeconds = 5f
                }
            };
        }
        
        #endregion
        
        #region Public API - Battle Management
        
        /// <summary>
        /// Start a new IPM battle with specified parameters.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMEnhancedBattleSession> StartBattleAsync(IPMBattleRequest request)
        {
            if (!IsSystemReady)
            {
                throw new InvalidOperationException("IPM Gaming System is not ready");
            }
            
            try
            {
                // Validate request
                var validationResult = await ValidateBattleRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException($"Invalid battle request: {validationResult.ErrorMessage}");
                }
                
                // Create battle session
                var battleSession = await _battleEngine.CreateBattleSessionAsync(request);
                
                // Record battle creation
                await _battleRepository.SaveBattleSessionAsync(battleSession);
                
                // Update player statistics
                await _progressionService.RecordBattleStartAsync(request.PlayerId, battleSession.BattleId);
                
                // Publish event
                _eventBus.Publish(new IPMBattleStartedEvent
                {
                    BattleId = battleSession.BattleId,
                    PlayerId = request.PlayerId,
                    BattleMode = request.BattleMode,
                    Difficulty = request.Difficulty,
                    StartTime = DateTime.Now
                });
                
                ChimeraLogger.Log("IPMGaming", $"Battle started: {battleSession.BattleId} for player {request.PlayerId}", this);
                return battleSession;
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogError("IPMGaming", $"Failed to start battle: {ex.Message}", this);
                throw;
            }
        }
        
        /// <summary>
        /// Execute a battle action (attack, strategy, etc.).
        /// </summary>
        public async System.Threading.Tasks.Task<IPMBattleActionResult> ExecuteBattleActionAsync(IPMBattleActionRequest request)
        {
            if (!IsSystemReady)
            {
                throw new InvalidOperationException("IPM Gaming System is not ready");
            }
            
            try
            {
                // Get battle session
                var battleSession = await _battleRepository.GetBattleSessionAsync(request.BattleId);
                if (battleSession == null)
                {
                    throw new ArgumentException($"Battle session not found: {request.BattleId}");
                }
                
                // Execute action through battle engine
                var actionResult = await _battleEngine.ExecuteActionAsync(battleSession, request);
                
                // Update battle state
                await _battleRepository.UpdateBattleSessionAsync(battleSession);
                
                // Check for battle completion
                if (actionResult.IsBattleCompleted)
                {
                    await CompleteBattleAsync(battleSession, actionResult.BattleResult);
                }
                
                return actionResult;
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogError("IPMGaming", $"Failed to execute battle action: {ex.Message}", this);
                throw;
            }
        }
        
        /// <summary>
        /// Get active battles for a player.
        /// </summary>
        public async System.Threading.Tasks.Task<List<IPMEnhancedBattleSession>> GetActiveBattlesAsync(string playerId)
        {
            if (!IsSystemReady)
            {
                return new List<IPMEnhancedBattleSession>();
            }
            
            return await _battleRepository.GetActiveBattlesByPlayerAsync(playerId);
        }
        
        /// <summary>
        /// Get battle history for a player.
        /// </summary>
        public async System.Threading.Tasks.Task<List<IPMBattleResult>> GetBattleHistoryAsync(string playerId, int maxResults = 20)
        {
            if (!IsSystemReady)
            {
                return new List<IPMBattleResult>();
            }
            
            return await _battleRepository.GetBattleHistoryAsync(playerId, maxResults);
        }
        
        #endregion
        
        #region Public API - Player Management
        
        /// <summary>
        /// Get or create player profile.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMPlayerProfile> GetPlayerProfileAsync(string playerId)
        {
            if (!IsSystemReady)
            {
                throw new InvalidOperationException("IPM Gaming System is not ready");
            }
            
            var profile = await _playerRepository.GetPlayerProfileAsync(playerId);
            if (profile == null)
            {
                profile = await CreateNewPlayerProfileAsync(playerId);
            }
            
            return profile;
        }
        
        /// <summary>
        /// Update player progression based on achievements.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMProgressionResult> UpdatePlayerProgressionAsync(string playerId, IPMProgressionUpdate update)
        {
            if (!IsSystemReady)
            {
                throw new InvalidOperationException("IPM Gaming System is not ready");
            }
            
            return await _progressionService.UpdateProgressionAsync(playerId, update);
        }
        
        /// <summary>
        /// Get player leaderboard standings.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMLeaderboardResult> GetLeaderboardAsync(IPMLeaderboardQuery query)
        {
            if (!IsSystemReady)
            {
                return new IPMLeaderboardResult { Players = new List<IPMLeaderboardEntry>() };
            }
            
            return await _leaderboardRepository.GetLeaderboardAsync(query);
        }
        
        #endregion
        
        #region Public API - Tournament Management
        
        /// <summary>
        /// Create a new tournament.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMTournament> CreateTournamentAsync(IPMTournamentRequest request)
        {
            if (!IsSystemReady || !_enableTournamentMode || _tournamentManager == null)
            {
                throw new InvalidOperationException("Tournament system is not available");
            }
            
            return await _tournamentManager.CreateTournamentAsync(request);
        }
        
        /// <summary>
        /// Join an existing tournament.
        /// </summary>
        public async System.Threading.Tasks.Task<bool> JoinTournamentAsync(string tournamentId, string playerId)
        {
            if (!IsSystemReady || !_enableTournamentMode || _tournamentManager == null)
            {
                return false;
            }
            
            return await _tournamentManager.JoinTournamentAsync(tournamentId, playerId);
        }
        
        /// <summary>
        /// Get active tournaments.
        /// </summary>
        public async System.Threading.Tasks.Task<List<IPMTournament>> GetActiveTournamentsAsync()
        {
            if (!IsSystemReady || !_enableTournamentMode || _tournamentManager == null)
            {
                return new List<IPMTournament>();
            }
            
            return await _tournamentManager.GetActiveTournamentsAsync();
        }
        
        #endregion
        
        #region Public API - Analytics and Insights
        
        /// <summary>
        /// Get player analytics and insights.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMPlayerAnalytics> GetPlayerAnalyticsAsync(string playerId)
        {
            if (!IsSystemReady || !_enableAnalytics)
            {
                return new IPMPlayerAnalytics();
            }
            
            return await _analyticsService.GetPlayerAnalyticsAsync(playerId);
        }
        
        /// <summary>
        /// Get system-wide analytics and metrics.
        /// </summary>
        public async System.Threading.Tasks.Task<IPMSystemAnalytics> GetSystemAnalyticsAsync()
        {
            if (!IsSystemReady || !_enableAnalytics)
            {
                return new IPMSystemAnalytics();
            }
            
            return await _analyticsService.GetSystemAnalyticsAsync();
        }
        
        #endregion
        
        #region Event Handlers
        
        private async void OnBattleStartedEvent(IPMBattleStartedEvent eventData)
        {
            _systemMetrics.TotalBattlesStarted++;
            
            if (_enableBattleLogging)
            {
                ChimeraLogger.Log("IPMGaming", $"Battle started event: {eventData.BattleId}", this);
            }
        }
        
        private async void OnBattleCompletedEvent(IPMBattleCompletedEvent eventData)
        {
            _systemMetrics.TotalBattlesCompleted++;
            
            // Trigger external event
            OnBattleCompleted?.Invoke(eventData.BattleResult);
            
            if (_enableBattleLogging)
            {
                ChimeraLogger.Log("IPMGaming", $"Battle completed event: {eventData.BattleId} - {eventData.BattleResult.Outcome}", this);
            }
        }
        
        private async void OnPlayerLevelUpEvent(IPMPlayerLevelUpEvent eventData)
        {
            _systemMetrics.TotalLevelUps++;
            
            // Create achievement
            var achievement = new IPMPlayerAchievement
            {
                PlayerId = eventData.PlayerId,
                AchievementType = "Level Up",
                Description = $"Reached level {eventData.NewLevel}",
                UnlockedAt = DateTime.Now
            };
            
            // Trigger external event
            OnPlayerAchievement?.Invoke(achievement);
            
            ChimeraLogger.Log("IPMGaming", $"Player {eventData.PlayerId} leveled up to {eventData.NewLevel}", this);
        }
        
        private async void OnTournamentStartedEvent(IPMTournamentStartedEvent eventData)
        {
            _systemMetrics.TotalTournamentsStarted++;
            
            // Create tournament update
            var update = new IPMTournamentUpdate
            {
                TournamentId = eventData.TournamentId,
                UpdateType = "Started",
                Message = $"Tournament {eventData.TournamentName} has begun!",
                Timestamp = DateTime.Now
            };
            
            // Trigger external event
            OnTournamentUpdate?.Invoke(update);
            
            ChimeraLogger.Log("IPMGaming", $"Tournament started: {eventData.TournamentId}", this);
        }
        
        private async void OnSystemErrorEvent(IPMSystemErrorEvent eventData)
        {
            _systemMetrics.TotalErrors++;
            
            // Create system alert
            var alert = new IPMSystemAlert
            {
                AlertType = "Error",
                Message = eventData.ErrorMessage,
                Severity = eventData.Severity,
                Timestamp = DateTime.Now
            };
            
            // Trigger external event
            OnSystemAlert?.Invoke(alert);
            
            ChimeraLogger.LogError("IPMGaming", $"System error: {eventData.ErrorMessage}", this);
        }
        
        #endregion
        
        #region Private Implementation
        
        private void UpdateSystemComponents()
        {
            // Update battle engine
            _battleEngine?.Update();
            
            // Update tournament manager
            _tournamentManager?.Update();
            
            // Update analytics service
            _analyticsService?.Update();
            
            // Process notifications
            _notificationSystem?.Update();
        }
        
        private void UpdateMetrics()
        {
            if (_systemMetrics != null)
            {
                _systemMetrics.LastUpdateTime = DateTime.Now;
                _systemMetrics.ActiveBattlesCount = ActiveBattlesCount;
                _systemMetrics.TotalPlayersCount = TotalPlayersCount;
                _systemMetrics.SystemUptime = DateTime.Now - _systemMetrics.SystemStartTime;
            }
        }
        
        private async System.Threading.Tasks.Task<IPMBattleValidationResult> ValidateBattleRequestAsync(IPMBattleRequest request)
        {
            var result = new IPMBattleValidationResult { IsValid = true };
            
            // Validate player exists
            var playerProfile = await _playerRepository.GetPlayerProfileAsync(request.PlayerId);
            if (playerProfile == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Player profile not found";
                return result;
            }
            
            // Check active battles limit
            var activeBattles = await _battleRepository.GetActiveBattlesByPlayerAsync(request.PlayerId);
            if (activeBattles.Count >= _maxConcurrentBattles)
            {
                result.IsValid = false;
                result.ErrorMessage = "Maximum concurrent battles limit reached";
                return result;
            }
            
            // Validate battle mode
            if (string.IsNullOrEmpty(request.BattleMode))
            {
                result.IsValid = false;
                result.ErrorMessage = "Battle mode is required";
                return result;
            }
            
            return result;
        }
        
        private async System.Threading.Tasks.Task<IPMPlayerProfile> CreateNewPlayerProfileAsync(string playerId)
        {
            var newProfile = new IPMPlayerProfile
            {
                PlayerId = playerId,
                PlayerName = $"Player_{playerId.Substring(0, Math.Min(8, playerId.Length))}",
                IPMLevel = 1,
                TotalExperience = 0,
                StrategyProficiency = new Dictionary<IPMStrategyType, float>
                {
                    { IPMStrategyType.Preventive, 0.5f },
                    { IPMStrategyType.Biological, 0.5f },
                    { IPMStrategyType.Integrated, 0.5f }
                },
                PestEncounters = new Dictionary<PestType, int>(),
                OrganismDeployments = new Dictionary<BeneficialOrganismType, int>(),
                UnlockedTechnologies = new List<string>(),
                ResearchProjects = new List<string>(),
                Statistics = new IPMStatistics
                {
                    TotalBattles = 0,
                    BattlesWon = 0,
                    BattlesLost = 0,
                    WinRate = 0f,
                    PestsEliminated = 0,
                    DefenseStructuresBuilt = 0,
                    BeneficialOrganismsReleased = 0,
                    ChemicalApplications = 0,
                    EnvironmentalZonesCreated = 0,
                    TotalBattleTime = TimeSpan.Zero,
                    AverageScore = 0f,
                    MultikillRecord = 0,
                    LongestWinStreak = 0,
                    PestKills = new Dictionary<PestType, int>(),
                    StructureUsage = new Dictionary<DefenseStructureType, int>()
                },
                ProfileCreated = DateTime.Now,
                LastActive = DateTime.Now
            };
            
            // Give player basic starting equipment
            var basicWeapons = _weaponCatalogService.GetBasicWeapons();
            newProfile.UnlockedTechnologies.AddRange(basicWeapons.Select(w => w.WeaponId));
            
            await _playerRepository.SavePlayerProfileAsync(newProfile);
            
            ChimeraLogger.Log("IPMGaming", $"Created new player profile: {playerId}", this);
            return newProfile;
        }
        
        private async System.Threading.Tasks.Task CompleteBattleAsync(IPMEnhancedBattleSession battleSession, IPMBattleResult battleResult)
        {
            // Update player progression
            var progressionUpdate = new IPMProgressionUpdate
            {
                PlayerId = battleSession.PlayerId,
                BattleResult = battleResult,
                ExperienceGained = CalculateExperienceGained(battleResult),
                WeaponsUsed = battleSession.WeaponsUsed,
                StrategiesUsed = battleSession.StrategiesUsed
            };
            
            await _progressionService.UpdateProgressionAsync(battleSession.PlayerId, progressionUpdate);
            
            // Save battle result
            await _battleRepository.SaveBattleResultAsync(battleResult);
            
            // Update leaderboards
            await _leaderboardRepository.UpdatePlayerRankingAsync(battleSession.PlayerId, battleResult);
            
            // Publish completion event
            _eventBus.Publish(new IPMBattleCompletedEvent
            {
                BattleId = battleSession.BattleId,
                PlayerId = battleSession.PlayerId,
                BattleResult = battleResult,
                CompletedAt = DateTime.Now
            });
        }
        
        private int CalculateExperienceGained(IPMBattleResult battleResult)
        {
            int baseExperience = 100;
            
            if (battleResult.Outcome == IPMBattleOutcome.Victory)
            {
                baseExperience += 50; // Victory bonus
            }
            
            baseExperience += (int)(battleResult.FinalScore * 0.1f); // Score bonus
            baseExperience += battleResult.PestsDefeated * 10; // Pest defeat bonus
            
            return Mathf.Max(10, baseExperience); // Minimum 10 XP
        }
        
        private void ShutdownSubsystems()
        {
            _battleEngine?.Shutdown();
            _progressionService?.Shutdown();
            _tournamentManager?.Shutdown();
            _analyticsService?.Shutdown();
            _notificationSystem?.Shutdown();
        }
        
        private void CleanupEventSystem()
        {
            _eventBus?.Dispose();
        }
        
        private void CleanupRepositories()
        {
            _battleRepository?.Dispose();
            _playerRepository?.Dispose();
            _leaderboardRepository?.Dispose();
        }
        
        #endregion
        
        #region Testing and Validation
        
        /// <summary>
        /// Comprehensive system health check for testing and validation.
        /// </summary>
        public IPMSystemHealthCheck PerformSystemHealthCheck()
        {
            var healthCheck = new IPMSystemHealthCheck
            {
                Timestamp = DateTime.Now,
                OverallHealth = IPMSystemHealth.Healthy,
                ComponentStatuses = new Dictionary<string, bool>(),
                ErrorMessages = new List<string>(),
                PerformanceMetrics = new Dictionary<string, float>()
            };
            
            try
            {
                // Check core components
                healthCheck.ComponentStatuses["BattleEngine"] = _battleEngine != null;
                healthCheck.ComponentStatuses["ProgressionService"] = _progressionService != null;
                healthCheck.ComponentStatuses["AnalyticsService"] = _analyticsService != null;
                healthCheck.ComponentStatuses["NotificationSystem"] = _notificationSystem != null;
                
                // Check repositories
                healthCheck.ComponentStatuses["BattleRepository"] = _battleRepository != null;
                healthCheck.ComponentStatuses["PlayerRepository"] = _playerRepository != null;
                healthCheck.ComponentStatuses["LeaderboardRepository"] = _leaderboardRepository != null;
                
                // Check domain services
                healthCheck.ComponentStatuses["PestCatalogService"] = _pestCatalogService != null;
                healthCheck.ComponentStatuses["WeaponCatalogService"] = _weaponCatalogService != null;
                healthCheck.ComponentStatuses["StrategyService"] = _strategyService != null;
                
                // Check optional components
                if (_enableTournamentMode)
                {
                    healthCheck.ComponentStatuses["TournamentManager"] = _tournamentManager != null;
                }
                
                // Calculate overall health
                bool allComponentsHealthy = healthCheck.ComponentStatuses.Values.All(status => status);
                if (!allComponentsHealthy)
                {
                    healthCheck.OverallHealth = IPMSystemHealth.Degraded;
                    healthCheck.ErrorMessages.Add("Some system components are not initialized");
                }
                
                // Add performance metrics
                healthCheck.PerformanceMetrics["ActiveBattles"] = ActiveBattlesCount;
                healthCheck.PerformanceMetrics["TotalPlayers"] = TotalPlayersCount;
                healthCheck.PerformanceMetrics["SystemUptimeSeconds"] = (float)_systemMetrics.SystemUptime.TotalSeconds;
                
                ChimeraLogger.Log("IPMGaming", $"System health check completed: {healthCheck.OverallHealth}", this);
            }
            catch (Exception ex)
            {
                healthCheck.OverallHealth = IPMSystemHealth.Critical;
                healthCheck.ErrorMessages.Add($"Health check failed: {ex.Message}");
                ChimeraLogger.LogError("IPMGaming", $"System health check failed: {ex.Message}", this);
            }
            
            return healthCheck;
        }
        
        /// <summary>
        /// Test method for validating system functionality.
        /// </summary>
        [ContextMenu("Test IPM Gaming System")]
        public async void TestIPMGamingSystemAsync()
        {
            ChimeraLogger.Log("IPMGaming", "=== Testing Enhanced IPM Gaming System ===", this);
            
            try
            {
                // Test system health
                var healthCheck = PerformSystemHealthCheck();
                ChimeraLogger.Log("IPMGaming", $"✓ System Health: {healthCheck.OverallHealth}", this);
                
                // Test player creation
                string testPlayerId = $"test_player_{DateTime.Now.Ticks}";
                var playerProfile = await GetPlayerProfileAsync(testPlayerId);
                ChimeraLogger.Log("IPMGaming", $"✓ Player Profile Created: {playerProfile.PlayerId}", this);
                
                // Test battle creation
                var battleRequest = new IPMBattleRequest
                {
                    PlayerId = testPlayerId,
                    BattleMode = "Quick Strike",
                    Difficulty = 1
                };
                
                var battleSession = await StartBattleAsync(battleRequest);
                ChimeraLogger.Log("IPMGaming", $"✓ Battle Session Created: {battleSession.BattleId}", this);
                
                // Test analytics
                if (_enableAnalytics)
                {
                    var analytics = await GetPlayerAnalyticsAsync(testPlayerId);
                    ChimeraLogger.Log("IPMGaming", $"✓ Player Analytics Retrieved: {analytics.TotalGamesPlayed} games", this);
                }
                
                ChimeraLogger.Log("IPMGaming", "✅ Enhanced IPM Gaming System test completed successfully", this);
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogError("IPMGaming", $"❌ System test failed: {ex.Message}", this);
            }
        }
        
        #endregion
    }
}