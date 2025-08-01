using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;
using AchievementProgress = ProjectChimera.Data.Achievements.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// PC-012-2d: Achievement Coordinator - Central orchestration service for all achievement operations
    /// Coordinates achievement services, manages meta-achievements, handles service dependencies
    /// Part of the decomposed AchievementSystemManager (500/1903 lines)
    /// Final service in the 4-service decomposition architecture
    /// </summary>
    public class AchievementCoordinator : MonoBehaviour, IAchievementCoordinator
    {
        [Header("Coordinator Configuration")]
        [SerializeField] private bool _enableCoordination = true;
        [SerializeField] private bool _enableMetaAchievements = true;
        [SerializeField] private bool _enableServiceIntegration = true;
        [SerializeField] private bool _enableAutoRewards = true;
        [SerializeField] private float _coordinationUpdateInterval = 1.0f;

        [Header("Meta-Achievement Settings")]
        [SerializeField] private bool _enableAchievementStreaks = true;
        [SerializeField] private bool _enableDailyAchievements = true;
        [SerializeField] private bool _enableCategoryMastery = true;
        [SerializeField] private bool _enablePointMilestones = true;
        [SerializeField] private int _streakThreshold = 3;

        [Header("Service Integration")]
        [SerializeField] private bool _enableServiceValidation = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _serviceHealthCheckInterval = 5.0f;

        // Service dependencies
        private IAchievementTrackingService _trackingService;
        private IAchievementRewardService _rewardService;
        private IAchievementDisplayService _displayService;

        // Service state
        private bool _isInitialized = false;
        private Dictionary<string, DateTime> _playerAchievementTimes = new Dictionary<string, DateTime>();
        private Dictionary<string, int> _playerStreakCounts = new Dictionary<string, int>();
        private Dictionary<ProjectChimera.Data.Progression.AchievementCategory, CategoryMasteryStatus> _categoryMastery = new Dictionary<ProjectChimera.Data.Progression.AchievementCategory, CategoryMasteryStatus>();
        private List<MetaAchievementRule> _metaAchievementRules = new List<MetaAchievementRule>();
        private Coroutine _coordinationUpdateCoroutine;
        private Coroutine _serviceHealthCheckCoroutine;

        // Statistics and monitoring
        private CoordinatorStatistics _statistics = new CoordinatorStatistics();
        private ServiceHealthStatus _serviceHealth = new ServiceHealthStatus();

        // Events for coordination
        public event Action<Achievement, RewardBundle> OnAchievementCompleted;
        public event Action<string, int> OnAchievementStreak;
        public event Action<ProjectChimera.Data.Progression.AchievementCategory, float> OnCategoryMasteryUpdated;
        public event Action<string, float> OnPointMilestoneReached;
        public event Action<MetaAchievementRule> OnMetaAchievementTriggered;
        public event Action<ServiceHealthStatus> OnServiceHealthUpdated;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Coordinator";
        public bool AllServicesHealthy => _serviceHealth.AllServicesHealthy;
        public CoordinatorStatistics Statistics => _statistics;
        public ServiceHealthStatus ServiceHealth => _serviceHealth;

        public void Initialize()
        {
            InitializeService();
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeCoordinatorSystems();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeCoordinatorSystems()
        {
            _playerAchievementTimes = new Dictionary<string, DateTime>();
            _playerStreakCounts = new Dictionary<string, int>();
            _categoryMastery = new Dictionary<AchievementCategory, CategoryMasteryStatus>();
            _metaAchievementRules = new List<MetaAchievementRule>();
            _statistics = new CoordinatorStatistics();
            _serviceHealth = new ServiceHealthStatus();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementCoordinator already initialized", this);
                return;
            }

            try
            {
                DiscoverServices();
                SetupServiceIntegration();
                InitializeMetaAchievements();
                StartCoordinationSystems();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementCoordinator initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementCoordinator: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            StopCoordinationSystems();
            UnsubscribeFromServices();
            _playerAchievementTimes.Clear();
            _playerStreakCounts.Clear();
            _categoryMastery.Clear();
            _metaAchievementRules.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("AchievementCoordinator shutdown completed", this);
        }

        #endregion

        #region Service Discovery and Integration

        private void DiscoverServices()
        {
            // Find achievement services in the scene
            _trackingService = FindObjectOfType<AchievementTrackingService>();
            _rewardService = FindObjectOfType<AchievementRewardService>();
            _displayService = FindObjectOfType<AchievementDisplayService>();

            // Validate service availability
            _serviceHealth.TrackingServiceAvailable = _trackingService != null;
            _serviceHealth.RewardServiceAvailable = _rewardService != null;
            _serviceHealth.DisplayServiceAvailable = _displayService != null;
            _serviceHealth.AllServicesHealthy = _serviceHealth.TrackingServiceAvailable && 
                                              _serviceHealth.RewardServiceAvailable && 
                                              _serviceHealth.DisplayServiceAvailable;

            ChimeraLogger.Log($"Service discovery complete - Healthy: {_serviceHealth.AllServicesHealthy}", this);
        }

        private void SetupServiceIntegration()
        {
            if (!_enableServiceIntegration) return;

            // Subscribe to tracking service events
            if (_trackingService != null)
            {
                _trackingService.OnAchievementUnlocked += OnAchievementUnlocked;
                _trackingService.OnProgressUpdated += OnProgressUpdated;
                _trackingService.OnProgressMilestone += OnProgressMilestone;
            }

            // Subscribe to reward service events
            if (_rewardService != null)
            {
                _rewardService.OnRewardDistributed += OnRewardDistributed;
                _rewardService.OnRewardTransactionCompleted += OnRewardTransactionCompleted;
            }

            // Subscribe to display service events
            if (_displayService != null)
            {
                _displayService.OnNotificationDisplayed += OnNotificationDisplayed;
                _displayService.OnCelebrationStarted += OnCelebrationStarted;
            }

            ChimeraLogger.Log("Service integration setup completed", this);
        }

        private void UnsubscribeFromServices()
        {
            // Unsubscribe from tracking service events
            if (_trackingService != null)
            {
                _trackingService.OnAchievementUnlocked -= OnAchievementUnlocked;
                _trackingService.OnProgressUpdated -= OnProgressUpdated;
                _trackingService.OnProgressMilestone -= OnProgressMilestone;
            }

            // Unsubscribe from reward service events
            if (_rewardService != null)
            {
                _rewardService.OnRewardDistributed -= OnRewardDistributed;
                _rewardService.OnRewardTransactionCompleted -= OnRewardTransactionCompleted;
            }

            // Unsubscribe from display service events
            if (_displayService != null)
            {
                _displayService.OnNotificationDisplayed -= OnNotificationDisplayed;
                _displayService.OnCelebrationStarted -= OnCelebrationStarted;
            }
        }

        #endregion

        #region Achievement Coordination

        public void ProcessAchievementUnlock(Achievement achievement, string playerId = "current_player")
        {
            if (!_isInitialized || !_enableCoordination)
            {
                return;
            }

            try
            {
                // Update statistics
                _statistics.TotalAchievementsProcessed++;
                _statistics.LastProcessedTime = DateTime.Now;

                // Handle achievement streak tracking
                if (_enableAchievementStreaks)
                {
                    ProcessAchievementStreak(playerId);
                }

                // Update category mastery progress
                if (_enableCategoryMastery)
                {
                    UpdateCategoryMastery(achievement.Category, playerId);
                }

                // Calculate and distribute rewards
                if (_enableAutoRewards && _rewardService != null)
                {
                    var rewards = _rewardService.CalculateRewards(achievement, playerId);
                    var distributed = _rewardService.DistributeRewards(rewards);
                    
                    if (distributed)
                    {
                        OnAchievementCompleted?.Invoke(achievement, rewards);
                    }
                }

                // Display achievement notification
                if (_displayService != null)
                {
                    var rewards = _rewardService?.CalculateRewards(achievement, playerId);
                    _displayService.ShowAchievementNotification(achievement, rewards);
                }

                // Check for meta-achievements
                if (_enableMetaAchievements)
                {
                    CheckMetaAchievements(playerId);
                }

                ChimeraLogger.Log($"Achievement coordination completed: {achievement.AchievementName}", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error processing achievement unlock: {ex.Message}", this);
            }
        }

        private void ProcessAchievementStreak(string playerId)
        {
            var now = DateTime.Now;
            var lastAchievementTime = _playerAchievementTimes.TryGetValue(playerId, out var time) ? time : DateTime.MinValue;
            
            // Check if this achievement is within streak window (24 hours)
            if (lastAchievementTime != DateTime.MinValue && (now - lastAchievementTime).TotalHours <= 24)
            {
                if (!_playerStreakCounts.ContainsKey(playerId))
                {
                    _playerStreakCounts[playerId] = 0;
                }
                
                _playerStreakCounts[playerId]++;
                
                if (_playerStreakCounts[playerId] >= _streakThreshold)
                {
                    OnAchievementStreak?.Invoke(playerId, _playerStreakCounts[playerId]);
                    
                    // Trigger streak achievement if tracking service is available
                    if (_trackingService != null)
                    {
                        _trackingService.UpdateProgress("achievement_streak", _playerStreakCounts[playerId], playerId);
                    }
                }
            }
            else
            {
                // Reset streak if too much time has passed
                _playerStreakCounts[playerId] = 1;
            }
            
            _playerAchievementTimes[playerId] = now;
        }

        private void UpdateCategoryMastery(ProjectChimera.Data.Progression.AchievementCategory category, string playerId)
        {
            if (!_categoryMastery.ContainsKey(category))
            {
                _categoryMastery[category] = new CategoryMasteryStatus
                {
                    Category = category,
                    CompletedAchievements = 0,
                    TotalAchievements = GetTotalAchievementsInCategory(category),
                    MasteryPercentage = 0f
                };
            }

            var mastery = _categoryMastery[category];
            mastery.CompletedAchievements++;
            mastery.MasteryPercentage = mastery.TotalAchievements > 0 ? 
                (float)mastery.CompletedAchievements / mastery.TotalAchievements : 0f;
            mastery.LastUpdateTime = DateTime.Now;

            OnCategoryMasteryUpdated?.Invoke(category, mastery.MasteryPercentage);

            // Check for category mastery milestones
            if (mastery.MasteryPercentage >= 1.0f && _trackingService != null)
            {
                _trackingService.UpdateProgress("category_mastery_complete", 1f, playerId);
            }
            else if (mastery.MasteryPercentage >= 0.75f && _trackingService != null)
            {
                _trackingService.UpdateProgress("category_mastery_75", 1f, playerId);
            }
        }

        private int GetTotalAchievementsInCategory(ProjectChimera.Data.Progression.AchievementCategory category)
        {
            if (_trackingService != null)
            {
                // Convert from Data.Progression.AchievementCategory to Data.Achievements.AchievementCategory
                var achievementsCategory = (ProjectChimera.Data.Achievements.AchievementCategory)(int)category;
                return _trackingService.GetAchievementsByCategory(achievementsCategory).Count;
            }
            return 10; // Default estimate
        }

        #endregion

        #region Meta-Achievement System

        private void InitializeMetaAchievements()
        {
            if (!_enableMetaAchievements) return;

            // Define meta-achievement rules
            _metaAchievementRules.Add(new MetaAchievementRule
            {
                RuleId = "achievement_hunter",
                Name = "Achievement Hunter",
                Description = "Unlock 10 achievements",
                Condition = (playerId) => GetPlayerAchievementCount(playerId) >= 10,
                TriggerEvent = "achievement_hunter",
                IsActive = true
            });

            _metaAchievementRules.Add(new MetaAchievementRule
            {
                RuleId = "point_milestone_1000",
                Name = "Point Collector",
                Description = "Earn 1,000 achievement points",
                Condition = (playerId) => GetPlayerTotalPoints(playerId) >= 1000f,
                TriggerEvent = "point_milestone_1000",
                IsActive = true
            });

            _metaAchievementRules.Add(new MetaAchievementRule
            {
                RuleId = "daily_achiever",
                Name = "Daily Achiever",
                Description = "Unlock achievements on consecutive days",
                Condition = (playerId) => GetPlayerStreakCount(playerId) >= 7,
                TriggerEvent = "daily_achievement",
                IsActive = _enableDailyAchievements
            });

            _metaAchievementRules.Add(new MetaAchievementRule
            {
                RuleId = "category_specialist",
                Name = "Category Specialist",
                Description = "Master a specific achievement category",
                Condition = (playerId) => HasCategoryMastery(playerId),
                TriggerEvent = "category_mastery_complete",
                IsActive = _enableCategoryMastery
            });

            ChimeraLogger.Log($"Meta-achievements initialized: {_metaAchievementRules.Count} rules", this);
        }

        private void CheckMetaAchievements(string playerId)
        {
            foreach (var rule in _metaAchievementRules.Where(r => r.IsActive))
            {
                try
                {
                    if (rule.Condition(playerId) && !rule.HasTriggered)
                    {
                        rule.HasTriggered = true;
                        rule.TriggerTime = DateTime.Now;
                        
                        OnMetaAchievementTriggered?.Invoke(rule);
                        
                        // Trigger the meta-achievement through tracking service
                        if (_trackingService != null)
                        {
                            _trackingService.UpdateProgress(rule.TriggerEvent, 1f, playerId);
                        }
                        
                        ChimeraLogger.Log($"Meta-achievement triggered: {rule.Name} for {playerId}", this);
                    }
                }
                catch (System.Exception ex)
                {
                    ChimeraLogger.LogError($"Error checking meta-achievement {rule.RuleId}: {ex.Message}", this);
                }
            }
        }

        private int GetPlayerAchievementCount(string playerId)
        {
            return _trackingService?.GetCompletedAchievementCount(playerId) ?? 0;
        }

        private float GetPlayerTotalPoints(string playerId)
        {
            return _trackingService?.GetTotalAchievementPoints(playerId) ?? 0f;
        }

        private int GetPlayerStreakCount(string playerId)
        {
            return _playerStreakCounts.TryGetValue(playerId, out var count) ? count : 0;
        }

        private bool HasCategoryMastery(string playerId)
        {
            return _categoryMastery.Values.Any(m => m.MasteryPercentage >= 1.0f);
        }

        #endregion

        #region Service Event Handlers

        private void OnAchievementUnlocked(string achievementId, ProjectChimera.Data.Achievements.AchievementProgress progress)
        {
            var achievement = _trackingService?.GetAchievementById(achievementId);
            if (achievement != null)
            {
                ProcessAchievementUnlock(achievement, progress.PlayerId);
            }
        }

        private void OnProgressUpdated(string playerId, float progress)
        {
            _statistics.TotalProgressUpdates++;
        }

        private void OnProgressMilestone(string playerId, string achievementId)
        {
            _statistics.TotalMilestonesReached++;
        }

        private void OnRewardDistributed(string playerId, RewardBundle rewards)
        {
            _statistics.TotalRewardsDistributed++;
            _statistics.TotalRewardValue += rewards.TotalValue;
        }

        private void OnRewardTransactionCompleted(RewardTransaction transaction)
        {
            _statistics.TotalTransactions++;
        }

        private void OnNotificationDisplayed(AchievementNotification notification)
        {
            _statistics.TotalNotificationsDisplayed++;
        }

        private void OnCelebrationStarted(Achievement achievement, CelebrationStyle style)
        {
            _statistics.TotalCelebrationsTriggered++;
        }

        #endregion

        #region Coordination Systems

        private void StartCoordinationSystems()
        {
            if (_coordinationUpdateCoroutine == null)
            {
                _coordinationUpdateCoroutine = StartCoroutine(CoordinationUpdateLoop());
            }

            if (_enablePerformanceMonitoring && _serviceHealthCheckCoroutine == null)
            {
                _serviceHealthCheckCoroutine = StartCoroutine(ServiceHealthCheckLoop());
            }
        }

        private void StopCoordinationSystems()
        {
            if (_coordinationUpdateCoroutine != null)
            {
                StopCoroutine(_coordinationUpdateCoroutine);
                _coordinationUpdateCoroutine = null;
            }

            if (_serviceHealthCheckCoroutine != null)
            {
                StopCoroutine(_serviceHealthCheckCoroutine);
                _serviceHealthCheckCoroutine = null;
            }
        }

        private IEnumerator CoordinationUpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_coordinationUpdateInterval);

                if (_enableCoordination)
                {
                    UpdateCoordinationSystems();
                }
            }
        }

        private IEnumerator ServiceHealthCheckLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_serviceHealthCheckInterval);

                if (_enableServiceValidation)
                {
                    PerformServiceHealthCheck();
                }
            }
        }

        private void UpdateCoordinationSystems()
        {
            // Update statistics
            _statistics.LastUpdateTime = DateTime.Now;
            _statistics.UptimeSeconds = (DateTime.Now - _statistics.StartTime).TotalSeconds;

            // Check for point milestones
            if (_enablePointMilestones)
            {
                CheckPointMilestones();
            }

            // Process daily achievements
            if (_enableDailyAchievements)
            {
                CheckDailyAchievements();
            }
        }

        private void PerformServiceHealthCheck()
        {
            _serviceHealth.TrackingServiceHealthy = _trackingService?.IsInitialized ?? false;
            _serviceHealth.RewardServiceHealthy = _rewardService?.IsInitialized ?? false;
            _serviceHealth.DisplayServiceHealthy = _displayService?.IsInitialized ?? false;
            _serviceHealth.AllServicesHealthy = _serviceHealth.TrackingServiceHealthy && 
                                              _serviceHealth.RewardServiceHealthy && 
                                              _serviceHealth.DisplayServiceHealthy;
            _serviceHealth.LastHealthCheck = DateTime.Now;

            OnServiceHealthUpdated?.Invoke(_serviceHealth);
        }

        private void CheckPointMilestones()
        {
            // Implementation for point milestone checking
            // This would check player point totals and trigger milestone achievements
        }

        private void CheckDailyAchievements()
        {
            // Implementation for daily achievement tracking
            // This would check for daily achievement completion patterns
        }

        #endregion

        #region Public API

        public CoordinatorStatistics GetStatistics()
        {
            return _statistics;
        }

        public ServiceHealthStatus GetServiceHealth()
        {
            return _serviceHealth;
        }

        public List<MetaAchievementRule> GetMetaAchievementRules()
        {
            return new List<MetaAchievementRule>(_metaAchievementRules);
        }

        public CategoryMasteryStatus GetCategoryMastery(ProjectChimera.Data.Progression.AchievementCategory category)
        {
            return _categoryMastery.TryGetValue(category, out var mastery) ? mastery : null;
        }

        public Dictionary<ProjectChimera.Data.Progression.AchievementCategory, CategoryMasteryStatus> GetAllCategoryMastery()
        {
            return new Dictionary<ProjectChimera.Data.Progression.AchievementCategory, CategoryMasteryStatus>(_categoryMastery);
        }

        public void ForceMetaAchievementCheck(string playerId)
        {
            if (_enableMetaAchievements)
            {
                CheckMetaAchievements(playerId);
            }
        }

        public void ResetPlayerStreak(string playerId)
        {
            _playerStreakCounts[playerId] = 0;
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MetaAchievementRule
    {
        public string RuleId = "";
        public string Name = "";
        public string Description = "";
        public System.Func<string, bool> Condition;
        public string TriggerEvent = "";
        public bool IsActive = true;
        public bool HasTriggered = false;
        public DateTime TriggerTime = DateTime.MinValue;
    }

    [System.Serializable]
    public class CategoryMasteryStatus
    {
        public ProjectChimera.Data.Progression.AchievementCategory Category;
        public int CompletedAchievements = 0;
        public int TotalAchievements = 0;
        public float MasteryPercentage = 0f;
        public DateTime LastUpdateTime = DateTime.Now;
    }

    [System.Serializable]
    public class CoordinatorStatistics
    {
        public DateTime StartTime = DateTime.Now;
        public DateTime LastUpdateTime = DateTime.Now;
        public DateTime LastProcessedTime = DateTime.MinValue;
        public double UptimeSeconds = 0;
        public int TotalAchievementsProcessed = 0;
        public int TotalProgressUpdates = 0;
        public int TotalMilestonesReached = 0;
        public int TotalRewardsDistributed = 0;
        public float TotalRewardValue = 0f;
        public int TotalTransactions = 0;
        public int TotalNotificationsDisplayed = 0;
        public int TotalCelebrationsTriggered = 0;
    }

    [System.Serializable]
    public class ServiceHealthStatus
    {
        public bool TrackingServiceAvailable = false;
        public bool RewardServiceAvailable = false;
        public bool DisplayServiceAvailable = false;
        public bool TrackingServiceHealthy = false;
        public bool RewardServiceHealthy = false;
        public bool DisplayServiceHealthy = false;
        public bool AllServicesHealthy = false;
        public DateTime LastHealthCheck = DateTime.Now;
    }

    #endregion
}