using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Achievement Celebration Service - Manages unlock ceremonies, visual effects, and celebration events
    /// Extracted from AchievementSystemManager to provide focused celebration management functionality
    /// Handles achievement unlock ceremonies, visual feedback, celebration queuing, and effect coordination
    /// Provides satisfying visual celebrations for achievement unlocks and milestone completions
    /// </summary>
    public class AchievementCelebrationService : MonoBehaviour
    {
        [Header("Celebration Configuration")]
        [SerializeField] private bool _enableCelebrations = true;
        [SerializeField] private bool _enableScreenEffects = true;
        [SerializeField] private bool _enableSoundEffects = true;
        [SerializeField] private bool _enableVisualEffects = true;

        [Header("Celebration Timing")]
        [SerializeField] private float _celebrationDuration = 5f;
        [SerializeField] private float _achievementDisplayTime = 3f;
        [SerializeField] private float _effectFadeTime = 1f;
        [SerializeField] private float _celebrationInterval = 0.5f;

        [Header("Celebration Limits")]
        [SerializeField] private int _maxConcurrentCelebrations = 3;
        [SerializeField] private int _maxQueuedCelebrations = 10;
        [SerializeField] private bool _allowCelebrationStacking = false;
        [SerializeField] private bool _prioritizeRareCelebrations = true;

        [Header("Celebration Queue")]
        [SerializeField] private Queue<Achievement> _pendingCelebrations = new Queue<Achievement>();
        [SerializeField] private List<Achievement> _activeCelebrations = new List<Achievement>();
        [SerializeField] private List<Achievement> _recentCelebrations = new List<Achievement>();

        // Service state
        private bool _isInitialized = false;
        private bool _isCelebrating = false;
        private float _lastCelebrationTime = 0f;
        private Coroutine _celebrationProcessor = null;

        // Celebration data lookups
        private Dictionary<AchievementRarity, CelebrationConfiguration> _celebrationConfigs = new Dictionary<AchievementRarity, CelebrationConfiguration>();
        private Dictionary<string, float> _celebrationCooldowns = new Dictionary<string, float>();

        // Events for celebration coordination
        public static event Action<Achievement> OnCelebrationStarted;
        public static event Action<Achievement> OnCelebrationCompleted;
        public static event Action<Achievement, CelebrationConfiguration> OnCelebrationConfigured;
        public static event Action<string, float> OnCelebrationEffect;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Celebration Service";
        public bool IsCelebrating => _isCelebrating;
        public int PendingCelebrations => _pendingCelebrations.Count;
        public int ActiveCelebrations => _activeCelebrations.Count;

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
            InitializeDataStructures();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDataStructures()
        {
            _pendingCelebrations = new Queue<Achievement>();
            _activeCelebrations = new List<Achievement>();
            _recentCelebrations = new List<Achievement>();
            _celebrationConfigs = new Dictionary<AchievementRarity, CelebrationConfiguration>();
            _celebrationCooldowns = new Dictionary<string, float>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementCelebrationService already initialized", this);
                return;
            }

            try
            {
                InitializeCelebrationConfigurations();
                StartCelebrationProcessor();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementCelebrationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementCelebrationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            StopCelebrationProcessor();
            ClearAllCelebrations();
            
            _isInitialized = false;
            ChimeraLogger.Log("AchievementCelebrationService shutdown completed", this);
        }

        #endregion

        #region Celebration Configuration

        private void InitializeCelebrationConfigurations()
        {
            // Initialize celebration configurations for each rarity level
            var configurations = new (AchievementRarity, CelebrationConfiguration)[]
            {
                (AchievementRarity.Common, new CelebrationConfiguration
                {
                    CelebrationStyle = "Simple",
                    Duration = 2f,
                    EffectIntensity = 0.3f,
                    SoundVolume = 0.5f,
                    ScreenEffectType = "BasicGlow",
                    ParticleEffectType = "SimpleSparkle",
                    CelebrationMessage = "🎉 Achievement unlocked!",
                    UseScreenShake = false,
                    UseColorFlash = true
                }),
                (AchievementRarity.Uncommon, new CelebrationConfiguration
                {
                    CelebrationStyle = "Enhanced",
                    Duration = 3f,
                    EffectIntensity = 0.5f,
                    SoundVolume = 0.7f,
                    ScreenEffectType = "EnhancedGlow",
                    ParticleEffectType = "EnhancedSparkle",
                    CelebrationMessage = "✨ Great achievement!",
                    UseScreenShake = false,
                    UseColorFlash = true
                }),
                (AchievementRarity.Rare, new CelebrationConfiguration
                {
                    CelebrationStyle = "Spectacular",
                    Duration = 4f,
                    EffectIntensity = 0.7f,
                    SoundVolume = 0.8f,
                    ScreenEffectType = "SpectacularBurst",
                    ParticleEffectType = "RareExplosion",
                    CelebrationMessage = "🌟 Rare achievement earned!",
                    UseScreenShake = true,
                    UseColorFlash = true
                }),
                (AchievementRarity.Epic, new CelebrationConfiguration
                {
                    CelebrationStyle = "Magnificent",
                    Duration = 5f,
                    EffectIntensity = 0.9f,
                    SoundVolume = 1.0f,
                    ScreenEffectType = "MagnificentAura",
                    ParticleEffectType = "EpicFireworks",
                    CelebrationMessage = "💫 Epic achievement mastered!",
                    UseScreenShake = true,
                    UseColorFlash = true
                }),
                (AchievementRarity.Legendary, new CelebrationConfiguration
                {
                    CelebrationStyle = "Legendary",
                    Duration = 7f,
                    EffectIntensity = 1.0f,
                    SoundVolume = 1.0f,
                    ScreenEffectType = "LegendaryRadiance",
                    ParticleEffectType = "LegendarySpectacle",
                    CelebrationMessage = "👑 LEGENDARY ACHIEVEMENT!",
                    UseScreenShake = true,
                    UseColorFlash = true
                })
            };

            foreach (var (rarity, config) in configurations)
            {
                _celebrationConfigs[rarity] = config;
            }

            ChimeraLogger.Log($"Celebration configurations initialized: {_celebrationConfigs.Count} rarity levels", this);
        }

        #endregion

        #region Celebration Management

        public void QueueCelebration(Achievement achievement)
        {
            if (!_isInitialized || !_enableCelebrations || achievement == null)
            {
                return;
            }

            // Check if achievement is already being celebrated
            if (_activeCelebrations.Any(a => a.AchievementID == achievement.AchievementID) ||
                _pendingCelebrations.Any(a => a.AchievementID == achievement.AchievementID))
            {
                ChimeraLogger.LogWarning($"Achievement {achievement.AchievementName} already queued for celebration", this);
                return;
            }

            // Check queue capacity
            if (_pendingCelebrations.Count >= _maxQueuedCelebrations)
            {
                if (_prioritizeRareCelebrations && ShouldPrioritize(achievement))
                {
                    // Remove the least rare celebration to make room
                    var removedAchievements = new List<Achievement>();
                    var tempQueue = new Queue<Achievement>();

                    while (_pendingCelebrations.Count > 0)
                    {
                        var queued = _pendingCelebrations.Dequeue();
                        if (queued.Rarity < achievement.Rarity)
                        {
                            removedAchievements.Add(queued);
                            break;
                        }
                        tempQueue.Enqueue(queued);
                    }

                    // Restore queue
                    while (tempQueue.Count > 0)
                    {
                        _pendingCelebrations.Enqueue(tempQueue.Dequeue());
                    }

                    if (removedAchievements.Count == 0)
                    {
                        ChimeraLogger.LogWarning("Celebration queue full, dropping achievement celebration", this);
                        return;
                    }
                }
                else
                {
                    ChimeraLogger.LogWarning("Celebration queue full, dropping achievement celebration", this);
                    return;
                }
            }

            _pendingCelebrations.Enqueue(achievement);
            ChimeraLogger.Log($"Queued celebration for achievement: {achievement.AchievementName}", this);
        }

        private bool ShouldPrioritize(Achievement achievement)
        {
            return achievement.Rarity >= AchievementRarity.Rare || 
                   achievement.IsSecret || 
                   achievement.AchievementName.Contains("Milestone");
        }

        private void StartCelebrationProcessor()
        {
            if (_celebrationProcessor != null)
            {
                StopCoroutine(_celebrationProcessor);
            }
            _celebrationProcessor = StartCoroutine(ProcessCelebrationQueue());
        }

        private void StopCelebrationProcessor()
        {
            if (_celebrationProcessor != null)
            {
                StopCoroutine(_celebrationProcessor);
                _celebrationProcessor = null;
            }
        }

        private IEnumerator ProcessCelebrationQueue()
        {
            while (_isInitialized)
            {
                if (_enableCelebrations && _pendingCelebrations.Count > 0 && 
                    _activeCelebrations.Count < _maxConcurrentCelebrations)
                {
                    var achievement = _pendingCelebrations.Dequeue();
                    yield return StartCoroutine(CelebrateAchievement(achievement));
                }

                yield return new WaitForSeconds(_celebrationInterval);
            }
        }

        #endregion

        #region Celebration Execution

        private IEnumerator CelebrateAchievement(Achievement achievement)
        {
            if (!_celebrationConfigs.TryGetValue(achievement.Rarity, out var config))
            {
                config = _celebrationConfigs[AchievementRarity.Common]; // Fallback
            }

            // Start celebration
            _activeCelebrations.Add(achievement);
            _isCelebrating = true;
            _lastCelebrationTime = Time.time;

            try
            {
                // Generate special celebration messages
                string celebrationMessage = GenerateCelebrationMessage(achievement, config);

                // Configure celebration based on achievement properties
                var customConfig = CustomizeCelebrationConfig(achievement, config);
                OnCelebrationConfigured?.Invoke(achievement, customConfig);

                // Fire celebration started event
                OnCelebrationStarted?.Invoke(achievement);

                // Execute celebration phases
                yield return StartCoroutine(ExecuteCelebrationEffects(achievement, customConfig));

                // Display achievement information
                yield return StartCoroutine(DisplayAchievementInfo(achievement, customConfig));

                // Apply screen effects if enabled
                if (_enableScreenEffects && customConfig.UseScreenShake)
                {
                    yield return StartCoroutine(ApplyScreenEffects(customConfig));
                }

                // Wait for celebration duration
                yield return new WaitForSeconds(customConfig.Duration);

                // Fade out effects
                yield return StartCoroutine(FadeOutEffects(customConfig));

                ChimeraLogger.Log(celebrationMessage, this);
            }
            finally
            {
                // Clean up celebration
                _activeCelebrations.Remove(achievement);
                AddToRecentCelebrations(achievement);
                
                if (_activeCelebrations.Count == 0)
                {
                    _isCelebrating = false;
                }

                // Fire celebration completed event
                OnCelebrationCompleted?.Invoke(achievement);
            }
        }

        private string GenerateCelebrationMessage(Achievement achievement, CelebrationConfiguration config)
        {
            // Special messages for milestone and hidden achievements
            if (achievement.AchievementName.Contains("Milestone"))
            {
                return $"🏁 MILESTONE REACHED: {achievement.AchievementName} ({achievement.Rarity})";
            }
            else if (achievement.IsSecret)
            {
                return $"🕵️ SECRET DISCOVERED: {achievement.AchievementName} ({achievement.Rarity})";
            }
            else if (achievement.Rarity == AchievementRarity.Legendary)
            {
                return $"👑 LEGENDARY ACHIEVEMENT: {achievement.AchievementName}!";
            }
            else if (achievement.Rarity == AchievementRarity.Epic)
            {
                return $"💫 EPIC ACHIEVEMENT: {achievement.AchievementName}!";
            }
            else
            {
                return $"{config.CelebrationMessage} {achievement.AchievementName}";
            }
        }

        private CelebrationConfiguration CustomizeCelebrationConfig(Achievement achievement, CelebrationConfiguration baseConfig)
        {
            var customConfig = new CelebrationConfiguration
            {
                CelebrationStyle = baseConfig.CelebrationStyle,
                Duration = baseConfig.Duration,
                EffectIntensity = baseConfig.EffectIntensity,
                SoundVolume = baseConfig.SoundVolume,
                ScreenEffectType = baseConfig.ScreenEffectType,
                ParticleEffectType = baseConfig.ParticleEffectType,
                CelebrationMessage = baseConfig.CelebrationMessage,
                UseScreenShake = baseConfig.UseScreenShake,
                UseColorFlash = baseConfig.UseColorFlash
            };

            // Customize based on achievement properties
            if (achievement.IsSecret)
            {
                customConfig.EffectIntensity *= 1.2f;
                customConfig.Duration *= 1.3f;
                customConfig.ParticleEffectType = "SecretDiscovery";
            }

            if (achievement.AchievementName.Contains("Milestone"))
            {
                customConfig.EffectIntensity *= 1.1f;
                customConfig.UseScreenShake = true;
                customConfig.ParticleEffectType = "MilestoneReached";
            }

            // Category-specific customizations
            customConfig.ScreenEffectType = achievement.Category switch
            {
                AchievementCategory.Cultivation_Mastery => $"Cultivation_{baseConfig.ScreenEffectType}",
                AchievementCategory.Genetics_Innovation => $"Genetics_{baseConfig.ScreenEffectType}",
                AchievementCategory.Research_Excellence => $"Research_{baseConfig.ScreenEffectType}",
                AchievementCategory.Business_Success => $"Business_{baseConfig.ScreenEffectType}",
                AchievementCategory.Social => $"Social_{baseConfig.ScreenEffectType}",
                AchievementCategory.Special => $"Special_{baseConfig.ScreenEffectType}",
                AchievementCategory.Ultimate => $"Ultimate_{baseConfig.ScreenEffectType}",
                _ => baseConfig.ScreenEffectType
            };

            return customConfig;
        }

        private IEnumerator ExecuteCelebrationEffects(Achievement achievement, CelebrationConfiguration config)
        {
            if (!_enableVisualEffects) yield break;

            // Trigger particle effects
            OnCelebrationEffect?.Invoke($"particle_{config.ParticleEffectType}", config.EffectIntensity);

            // Trigger sound effects
            if (_enableSoundEffects)
            {
                OnCelebrationEffect?.Invoke($"sound_{achievement.Rarity}", config.SoundVolume);
            }

            // Brief delay for effect synchronization
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator DisplayAchievementInfo(Achievement achievement, CelebrationConfiguration config)
        {
            // Display achievement information (UI integration point)
            OnCelebrationEffect?.Invoke($"display_achievement", 1f);
            
            yield return new WaitForSeconds(_achievementDisplayTime);
        }

        private IEnumerator ApplyScreenEffects(CelebrationConfiguration config)
        {
            // Apply screen shake and color flash effects
            if (config.UseScreenShake)
            {
                OnCelebrationEffect?.Invoke($"screen_shake", config.EffectIntensity);
            }

            if (config.UseColorFlash)
            {
                OnCelebrationEffect?.Invoke($"color_flash_{config.ScreenEffectType}", config.EffectIntensity);
            }

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator FadeOutEffects(CelebrationConfiguration config)
        {
            // Fade out celebration effects
            OnCelebrationEffect?.Invoke("fade_out_effects", _effectFadeTime);
            yield return new WaitForSeconds(_effectFadeTime);
        }

        #endregion

        #region Celebration History

        private void AddToRecentCelebrations(Achievement achievement)
        {
            _recentCelebrations.Insert(0, achievement);
            
            // Keep only recent celebrations (last 20)
            if (_recentCelebrations.Count > 20)
            {
                _recentCelebrations.RemoveAt(_recentCelebrations.Count - 1);
            }

            // Update cooldown
            _celebrationCooldowns[achievement.AchievementID] = Time.time;
        }

        private void ClearAllCelebrations()
        {
            _pendingCelebrations.Clear();
            _activeCelebrations.Clear();
            _recentCelebrations.Clear();
            _celebrationCooldowns.Clear();
        }

        #endregion

        #region Public API

        public bool IsCelebrationQueued(string achievementId)
        {
            return _pendingCelebrations.Any(a => a.AchievementID == achievementId);
        }

        public bool IsCelebrationActive(string achievementId)
        {
            return _activeCelebrations.Any(a => a.AchievementID == achievementId);
        }

        public float GetLastCelebrationTime(string achievementId)
        {
            return _celebrationCooldowns.GetValueOrDefault(achievementId, 0f);
        }

        public List<Achievement> GetRecentCelebrations()
        {
            return _recentCelebrations.ToList();
        }

        public void ClearCelebrationQueue()
        {
            _pendingCelebrations.Clear();
            ChimeraLogger.Log("Celebration queue cleared", this);
        }

        public CelebrationConfiguration GetCelebrationConfig(AchievementRarity rarity)
        {
            return _celebrationConfigs.TryGetValue(rarity, out var config) ? config : _celebrationConfigs[AchievementRarity.Common];
        }

        public void UpdateCelebrationSettings(bool enableCelebrations, bool enableEffects, float duration)
        {
            _enableCelebrations = enableCelebrations;
            _enableVisualEffects = enableEffects;
            _celebrationDuration = duration;
            
            ChimeraLogger.Log($"Celebration settings updated: enabled={enableCelebrations}, effects={enableEffects}, duration={duration}", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    /// <summary>
    /// Configuration data for achievement celebrations
    /// </summary>
    [System.Serializable]
    public class CelebrationConfiguration
    {
        public string CelebrationStyle = "Simple";
        public float Duration = 3f;
        public float EffectIntensity = 0.5f;
        public float SoundVolume = 0.7f;
        public string ScreenEffectType = "BasicGlow";
        public string ParticleEffectType = "SimpleSparkle";
        public string CelebrationMessage = "Achievement unlocked!";
        public bool UseScreenShake = false;
        public bool UseColorFlash = true;
    }
}