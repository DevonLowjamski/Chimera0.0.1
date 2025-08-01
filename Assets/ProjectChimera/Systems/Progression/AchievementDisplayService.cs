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
    /// PC-012-2c: Achievement Display Service - Specialized UI notification and celebration system
    /// Handles achievement popup displays, celebration animations, progress indicators, and visual feedback
    /// Part of the decomposed AchievementSystemManager (453/1903 lines)
    /// Integrates with Unity UI Toolkit and visual effects systems
    /// </summary>
    public class AchievementDisplayService : MonoBehaviour, IAchievementDisplayService
    {
        [Header("Display Configuration")]
        [SerializeField] private bool _enableNotifications = true;
        [SerializeField] private bool _enableCelebrations = true;
        [SerializeField] private bool _enableSounds = true;
        [SerializeField] private bool _enableVisualEffects = true;
        [SerializeField] private float _notificationDuration = 4.0f;
        [SerializeField] private int _maxSimultaneousNotifications = 3;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _displayDuration = 3.0f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Visual Effects")]
        [SerializeField] private bool _enableParticleEffects = true;
        [SerializeField] private bool _enableScreenShake = false;
        [SerializeField] private float _screenShakeIntensity = 0.1f;
        [SerializeField] private bool _enableColorFlash = true;

        [Header("Audio Settings")]
        [SerializeField] private bool _enableAudioFeedback = true;
        [SerializeField] private float _audioVolume = 0.8f;
        [SerializeField] private bool _enableRarityBasedAudio = true;

        // Service state
        private bool _isInitialized = false;
        private Queue<AchievementNotification> _notificationQueue = new Queue<AchievementNotification>();
        private List<AchievementNotification> _activeNotifications = new List<AchievementNotification>();
        private Dictionary<AchievementRarity, CelebrationStyle> _celebrationStyles = new Dictionary<AchievementRarity, CelebrationStyle>();
        private Dictionary<string, DateTime> _lastNotificationTimes = new Dictionary<string, DateTime>();
        private Coroutine _notificationProcessorCoroutine;

        // UI References (to be injected or found)
        private Transform _notificationContainer;
        private GameObject _notificationPrefab;
        private Dictionary<string, AudioClip> _achievementSounds = new Dictionary<string, AudioClip>();
        private Dictionary<string, GameObject> _particleEffects = new Dictionary<string, GameObject>();

        // Events for display coordination
        public event Action<AchievementNotification> OnNotificationDisplayed;
        public event Action<AchievementNotification> OnNotificationCompleted;
        public event Action<Achievement, CelebrationStyle> OnCelebrationStarted;
        public event Action<Achievement> OnCelebrationCompleted;
        public event Action<string, float> OnProgressIndicatorUpdated;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Display Service";
        public int ActiveNotificationCount => _activeNotifications.Count;
        public int QueuedNotificationCount => _notificationQueue.Count;
        public bool IsDisplayingNotifications => _activeNotifications.Count > 0 || _notificationQueue.Count > 0;

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
            InitializeDisplaySystem();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDisplaySystem()
        {
            _notificationQueue = new Queue<AchievementNotification>();
            _activeNotifications = new List<AchievementNotification>();
            _celebrationStyles = new Dictionary<AchievementRarity, CelebrationStyle>();
            _lastNotificationTimes = new Dictionary<string, DateTime>();
            _achievementSounds = new Dictionary<string, AudioClip>();
            _particleEffects = new Dictionary<string, GameObject>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementDisplayService already initialized", this);
                return;
            }

            try
            {
                SetupCelebrationStyles();
                SetupUIReferences();
                SetupAudioAssets();
                SetupVisualEffects();
                StartNotificationProcessor();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementDisplayService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementDisplayService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            StopNotificationProcessor();
            ClearAllNotifications();
            _celebrationStyles.Clear();
            _lastNotificationTimes.Clear();
            _achievementSounds.Clear();
            _particleEffects.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("AchievementDisplayService shutdown completed", this);
        }

        #endregion

        #region Display System Setup

        private void SetupCelebrationStyles()
        {
            _celebrationStyles[AchievementRarity.Common] = new CelebrationStyle
            {
                AnimationIntensity = 1.0f,
                ParticleEffectName = "celebration_basic",
                SoundEffectName = "achievement_common",
                ColorScheme = new Color(0.7f, 0.7f, 0.7f, 1f),
                Duration = 2.0f,
                ScreenShakeIntensity = 0f
            };

            _celebrationStyles[AchievementRarity.Uncommon] = new CelebrationStyle
            {
                AnimationIntensity = 1.2f,
                ParticleEffectName = "celebration_enhanced",
                SoundEffectName = "achievement_uncommon",
                ColorScheme = new Color(0.4f, 0.8f, 0.4f, 1f),
                Duration = 2.5f,
                ScreenShakeIntensity = 0.05f
            };

            _celebrationStyles[AchievementRarity.Rare] = new CelebrationStyle
            {
                AnimationIntensity = 1.5f,
                ParticleEffectName = "celebration_impressive",
                SoundEffectName = "achievement_rare",
                ColorScheme = new Color(0.4f, 0.6f, 1f, 1f),
                Duration = 3.0f,
                ScreenShakeIntensity = 0.08f
            };

            _celebrationStyles[AchievementRarity.Epic] = new CelebrationStyle
            {
                AnimationIntensity = 2.0f,
                ParticleEffectName = "celebration_spectacular",
                SoundEffectName = "achievement_epic",
                ColorScheme = new Color(0.8f, 0.4f, 1f, 1f),
                Duration = 4.0f,
                ScreenShakeIntensity = 0.12f
            };

            _celebrationStyles[AchievementRarity.Legendary] = new CelebrationStyle
            {
                AnimationIntensity = 3.0f,
                ParticleEffectName = "celebration_legendary",
                SoundEffectName = "achievement_legendary",
                ColorScheme = new Color(1f, 0.8f, 0.2f, 1f),
                Duration = 5.0f,
                ScreenShakeIntensity = 0.15f
            };
        }

        private void SetupUIReferences()
        {
            // TODO: Integrate with actual UI system
            // For now, create placeholder references
            
            // Find or create notification container
            var canvasObject = GameObject.Find("Canvas");
            if (canvasObject != null)
            {
                var notificationContainer = canvasObject.transform.Find("NotificationContainer");
                if (notificationContainer == null)
                {
                    var containerGO = new GameObject("NotificationContainer");
                    containerGO.transform.SetParent(canvasObject.transform, false);
                    _notificationContainer = containerGO.transform;
                }
                else
                {
                    _notificationContainer = notificationContainer;
                }
            }

            // TODO: Load notification prefab from resources
            // _notificationPrefab = Resources.Load<GameObject>("UI/AchievementNotificationPrefab");
        }

        private void SetupAudioAssets()
        {
            // TODO: Load audio assets from Resources or Addressables
            // For now, create placeholder dictionary
            
            _achievementSounds["achievement_common"] = null; // Resources.Load<AudioClip>("Audio/Achievement_Common");
            _achievementSounds["achievement_uncommon"] = null; // Resources.Load<AudioClip>("Audio/Achievement_Uncommon");
            _achievementSounds["achievement_rare"] = null; // Resources.Load<AudioClip>("Audio/Achievement_Rare");
            _achievementSounds["achievement_epic"] = null; // Resources.Load<AudioClip>("Audio/Achievement_Epic");
            _achievementSounds["achievement_legendary"] = null; // Resources.Load<AudioClip>("Audio/Achievement_Legendary");
        }

        private void SetupVisualEffects()
        {
            // TODO: Load particle effect prefabs from Resources or Addressables
            // For now, create placeholder dictionary
            
            _particleEffects["celebration_basic"] = null; // Resources.Load<GameObject>("Effects/Celebration_Basic");
            _particleEffects["celebration_enhanced"] = null; // Resources.Load<GameObject>("Effects/Celebration_Enhanced");
            _particleEffects["celebration_impressive"] = null; // Resources.Load<GameObject>("Effects/Celebration_Impressive");
            _particleEffects["celebration_spectacular"] = null; // Resources.Load<GameObject>("Effects/Celebration_Spectacular");
            _particleEffects["celebration_legendary"] = null; // Resources.Load<GameObject>("Effects/Celebration_Legendary");
        }

        #endregion

        #region Notification Display

        public void ShowAchievementNotification(Achievement achievement, RewardBundle rewards = null)
        {
            if (!_isInitialized || !_enableNotifications)
            {
                return;
            }

            // Check for duplicate notifications
            if (ShouldSuppressNotification(achievement.AchievementID))
            {
                return;
            }

            var notification = new AchievementNotification
            {
                NotificationId = Guid.NewGuid().ToString(),
                Achievement = achievement,
                Rewards = rewards,
                CreatedAt = DateTime.Now,
                Priority = GetNotificationPriority(achievement.Rarity),
                Status = NotificationStatus.Queued
            };

            _notificationQueue.Enqueue(notification);
            _lastNotificationTimes[achievement.AchievementID] = DateTime.Now;

            if (_enableNotifications)
            {
                ChimeraLogger.Log($"Queued achievement notification: {achievement.AchievementName}", this);
            }
        }

        public void ShowProgressNotification(Achievement achievement, float progressPercentage)
        {
            if (!_isInitialized || !_enableNotifications)
            {
                return;
            }

            // Only show progress notifications for significant milestones
            if (!IsSignificantProgress(progressPercentage))
            {
                return;
            }

            OnProgressIndicatorUpdated?.Invoke(achievement.AchievementID, progressPercentage);

            if (_enableNotifications)
            {
                ChimeraLogger.Log($"Progress notification: {achievement.AchievementName} - {progressPercentage:P0}", this);
            }
        }

        private bool ShouldSuppressNotification(string achievementId)
        {
            if (!_lastNotificationTimes.ContainsKey(achievementId))
            {
                return false;
            }

            var timeSinceLastNotification = DateTime.Now - _lastNotificationTimes[achievementId];
            return timeSinceLastNotification.TotalSeconds < 10.0; // Suppress duplicates within 10 seconds
        }

        private int GetNotificationPriority(AchievementRarity rarity)
        {
            return rarity switch
            {
                AchievementRarity.Common => 1,
                AchievementRarity.Uncommon => 2,
                AchievementRarity.Rare => 3,
                AchievementRarity.Epic => 4,
                AchievementRarity.Legendary => 5,
                _ => 1
            };
        }

        private bool IsSignificantProgress(float progressPercentage)
        {
            // Show progress at 25%, 50%, 75%, and 90%
            return progressPercentage >= 0.25f && 
                   (Mathf.Approximately(progressPercentage % 0.25f, 0f) || progressPercentage >= 0.9f);
        }

        #endregion

        #region Notification Processing

        private void StartNotificationProcessor()
        {
            if (_notificationProcessorCoroutine == null)
            {
                _notificationProcessorCoroutine = StartCoroutine(ProcessNotificationQueue());
            }
        }

        private void StopNotificationProcessor()
        {
            if (_notificationProcessorCoroutine != null)
            {
                StopCoroutine(_notificationProcessorCoroutine);
                _notificationProcessorCoroutine = null;
            }
        }

        private IEnumerator ProcessNotificationQueue()
        {
            while (true)
            {
                // Process queued notifications
                while (_notificationQueue.Count > 0 && _activeNotifications.Count < _maxSimultaneousNotifications)
                {
                    var notification = _notificationQueue.Dequeue();
                    StartCoroutine(DisplayNotification(notification));
                }

                // Clean up completed notifications
                _activeNotifications.RemoveAll(n => n.Status == NotificationStatus.Completed);

                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator DisplayNotification(AchievementNotification notification)
        {
            notification.Status = NotificationStatus.Displaying;
            _activeNotifications.Add(notification);

            // Start celebration effects (with error handling)
            if (_enableCelebrations)
            {
                SafeStartCelebrationEffects(notification);
            }

            // Display UI notification
            yield return StartCoroutine(ShowNotificationUI(notification));

            // Play audio feedback (with error handling)
            if (_enableSounds && _enableAudioFeedback)
            {
                SafePlayAchievementSound(notification.Achievement.Rarity);
            }

            SafeInvokeEvent(() => OnNotificationDisplayed?.Invoke(notification));

            // Wait for display duration
            yield return new WaitForSeconds(_displayDuration);

            // Fade out notification
            yield return StartCoroutine(HideNotificationUI(notification));

            notification.Status = NotificationStatus.Completed;
            SafeInvokeEvent(() => OnNotificationCompleted?.Invoke(notification));

            ChimeraLogger.Log($"Completed achievement notification: {notification.Achievement.AchievementName}", this);
        }

        private void SafeStartCelebrationEffects(AchievementNotification notification)
        {
            try
            {
                StartCelebrationEffects(notification);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error starting celebration effects: {ex.Message}", this);
            }
        }

        private void SafePlayAchievementSound(AchievementRarity rarity)
        {
            try
            {
                PlayAchievementSound(rarity);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error playing achievement sound: {ex.Message}", this);
            }
        }

        private void SafeInvokeEvent(System.Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error invoking event: {ex.Message}", this);
            }
        }

        private IEnumerator ShowNotificationUI(AchievementNotification notification)
        {
            // TODO: Implement actual UI notification display
            // For now, just simulate timing
            
            // Fade in
            yield return new WaitForSeconds(_fadeInDuration);
            
            // Update notification status
            notification.DisplayStartTime = DateTime.Now;
            
            ChimeraLogger.Log($"Displaying notification: {notification.Achievement.AchievementName}", this);
        }

        private IEnumerator HideNotificationUI(AchievementNotification notification)
        {
            // TODO: Implement actual UI notification hide
            // For now, just simulate timing
            
            // Fade out
            yield return new WaitForSeconds(_fadeOutDuration);
            
            ChimeraLogger.Log($"Hidden notification: {notification.Achievement.AchievementName}", this);
        }

        #endregion

        #region Celebration Effects

        private void StartCelebrationEffects(AchievementNotification notification)
        {
            var achievement = notification.Achievement;
            var celebrationStyle = GetCelebrationStyle(achievement.Rarity);

            OnCelebrationStarted?.Invoke(achievement, celebrationStyle);

            // Start particle effects
            if (_enableParticleEffects && _enableVisualEffects)
            {
                StartParticleEffect(celebrationStyle.ParticleEffectName, celebrationStyle.Duration);
            }

            // Apply screen shake
            if (_enableScreenShake && celebrationStyle.ScreenShakeIntensity > 0)
            {
                StartCoroutine(ApplyScreenShake(celebrationStyle.ScreenShakeIntensity, celebrationStyle.Duration * 0.5f));
            }

            // Apply color flash
            if (_enableColorFlash && _enableVisualEffects)
            {
                StartCoroutine(ApplyColorFlash(celebrationStyle.ColorScheme, 0.3f));
            }

            // Schedule celebration completion
            StartCoroutine(CompleteCelebration(achievement, celebrationStyle.Duration));
        }

        private void StartParticleEffect(string effectName, float duration)
        {
            if (_particleEffects.TryGetValue(effectName, out var effectPrefab) && effectPrefab != null)
            {
                var effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                Destroy(effectInstance, duration + 1.0f);
            }
            else
            {
                ChimeraLogger.Log($"Particle effect '{effectName}' would play for {duration}s", this);
            }
        }

        private IEnumerator ApplyScreenShake(float intensity, float duration)
        {
            // TODO: Implement actual screen shake
            // For now, just log the effect
            
            ChimeraLogger.Log($"Screen shake: intensity {intensity}, duration {duration}s", this);
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator ApplyColorFlash(Color flashColor, float duration)
        {
            // TODO: Implement actual color flash effect
            // For now, just log the effect
            
            ChimeraLogger.Log($"Color flash: {flashColor}, duration {duration}s", this);
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator CompleteCelebration(Achievement achievement, float delay)
        {
            yield return new WaitForSeconds(delay);
            OnCelebrationCompleted?.Invoke(achievement);
        }

        private void PlayAchievementSound(AchievementRarity rarity)
        {
            var celebrationStyle = GetCelebrationStyle(rarity);
            
            if (_achievementSounds.TryGetValue(celebrationStyle.SoundEffectName, out var audioClip) && audioClip != null)
            {
                // TODO: Integrate with actual audio system
                // AudioSource.PlayClipAtPoint(audioClip, transform.position, _audioVolume);
            }
            
            ChimeraLogger.Log($"Playing achievement sound: {celebrationStyle.SoundEffectName}", this);
        }

        #endregion

        #region Helper Methods

        private CelebrationStyle GetCelebrationStyle(AchievementRarity rarity)
        {
            return _celebrationStyles.TryGetValue(rarity, out var style) ? style : _celebrationStyles[AchievementRarity.Common];
        }

        public void ClearAllNotifications()
        {
            StopAllCoroutines();
            _notificationQueue.Clear();
            _activeNotifications.Clear();
            
            if (_notificationProcessorCoroutine != null)
            {
                StartNotificationProcessor(); // Restart processor
            }
        }

        public List<AchievementNotification> GetActiveNotifications()
        {
            return new List<AchievementNotification>(_activeNotifications);
        }

        public DisplayStatistics GetDisplayStatistics()
        {
            return new DisplayStatistics
            {
                TotalNotificationsDisplayed = _lastNotificationTimes.Count,
                ActiveNotifications = _activeNotifications.Count,
                QueuedNotifications = _notificationQueue.Count,
                AverageDisplayTime = _displayDuration,
                LastNotificationTime = _lastNotificationTimes.Count > 0 
                    ? _lastNotificationTimes.Values.Max() 
                    : DateTime.MinValue
            };
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
    public class AchievementNotification
    {
        public string NotificationId = "";
        public Achievement Achievement;
        public RewardBundle Rewards;
        public DateTime CreatedAt = DateTime.Now;
        public DateTime DisplayStartTime = DateTime.MinValue;
        public int Priority = 1;
        public NotificationStatus Status = NotificationStatus.Queued;
    }

    [System.Serializable]
    public class CelebrationStyle
    {
        public float AnimationIntensity = 1.0f;
        public string ParticleEffectName = "";
        public string SoundEffectName = "";
        public Color ColorScheme = Color.white;
        public float Duration = 2.0f;
        public float ScreenShakeIntensity = 0f;
    }

    [System.Serializable]
    public class DisplayStatistics
    {
        public int TotalNotificationsDisplayed = 0;
        public int ActiveNotifications = 0;
        public int QueuedNotifications = 0;
        public float AverageDisplayTime = 0f;
        public DateTime LastNotificationTime = DateTime.MinValue;
    }

    public enum NotificationStatus
    {
        Queued,
        Displaying,
        Completed,
        Failed
    }

    #endregion
}