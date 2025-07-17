using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Construction;
using ProjectChimera.Data.Construction;
using DataConstructionIssue = ProjectChimera.Data.Construction.ConstructionIssue;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Effects;
using ProjectChimera.Systems.Economy;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
// Add alias for Environment namespace EnvironmentalConditions
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using EnvironmentalAlert = ProjectChimera.Systems.Environment.EnvironmentalAlert;

namespace ProjectChimera.Systems.Effects
{
    #region Supporting Data Structures
    // UIFeedbackController and other data structures are defined in VisualFeedbackDataStructures.cs
    #endregion
    
    /// <summary>
    /// Visual feedback system providing comprehensive visual responses to gameplay events.
    /// Manages screen effects, UI animations, world indicators, and visual confirmations
    /// for user interactions and system state changes.
    /// </summary>
    public class VisualFeedbackSystem : ChimeraManager
    {
        [Header("Screen Effects")]
        [SerializeField] private Material _screenFlashMaterial;
        [SerializeField] private Material _screenShakeMaterial;
        [SerializeField] private AnimationCurve _shakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float _maxShakeIntensity = 0.1f;
        
        [Header("World Indicators")]
        [SerializeField] private GameObject _worldIndicatorPrefab;
        [SerializeField] private GameObject _progressIndicatorPrefab;
        [SerializeField] private GameObject _alertIndicatorPrefab;
        [SerializeField] private GameObject _successIndicatorPrefab;
        [SerializeField] private GameObject _warningIndicatorPrefab;
        
        [Header("UI Feedback")]
        [SerializeField] private bool _enableUIAnimations = true;
        [SerializeField] private bool _enableHapticFeedback = true;
        [SerializeField] private float _defaultAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve _defaultEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Visual Confirmation")]
        [SerializeField] private Color _successColor = Color.green;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _errorColor = Color.red;
        [SerializeField] private Color _infoColor = Color.cyan;
        
        // Screen Effects
        private Camera _mainCamera;
        private Coroutine _currentShakeCoroutine;
        private List<ScreenFlashEffect> _activeFlashEffects = new List<ScreenFlashEffect>();
        
        // World Indicators
        private Queue<GameObject> _indicatorPool = new Queue<GameObject>();
        private List<WorldIndicator> _activeIndicators = new List<WorldIndicator>();
        private Transform _indicatorContainer;
        
        // UI Feedback
        private Dictionary<VisualElement, UIFeedbackAnimation> _activeUIAnimations = new Dictionary<VisualElement, UIFeedbackAnimation>();
        private UIFeedbackController _uiFeedbackController;
        
        // Visual Confirmations
        private List<VisualConfirmation> _activeConfirmations = new List<VisualConfirmation>();
        private ConfirmationRenderer _confirmationRenderer;
        
        // System Integration
        // PHASE 1 PC-007: Removed direct manager references - using event-driven communication only
        // System connections now handled through GameEventSO channels instead of direct references
        
        // Performance Tracking
        private FeedbackPerformanceMetrics _performanceMetrics;
        private float _lastMetricsUpdate = 0f;
        
        // Events
        public System.Action<FeedbackType, Vector3> OnFeedbackTriggered;
        public System.Action<string> OnVisualConfirmation;
        public System.Action<FeedbackPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public FeedbackPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public bool UIAnimationsEnabled => _enableUIAnimations;
        
        protected override void OnManagerInitialize()
        {
            InitializeFeedbackSystem();
            SetupWorldIndicators();
            ConnectToGameSystems();
            InitializePerformanceTracking();
            LogInfo("Visual Feedback System initialized");
        }
        
        private void Update()
        {
            UpdateActiveIndicators();
            UpdateUIAnimations();
            UpdateVisualConfirmations();
            UpdatePerformanceMetrics();
            CleanupFinishedEffects();
        }
        
        #region Initialization
        
        private void InitializeFeedbackSystem()
        {
            _mainCamera = FindObjectOfType<Camera>();
            
            // Initialize UI feedback controller
            _uiFeedbackController = new UIFeedbackController();
            
            // Initialize confirmation renderer
            _confirmationRenderer = new ConfirmationRenderer(_mainCamera);
            
            // Initialize performance metrics
            _performanceMetrics = new FeedbackPerformanceMetrics
            {
                MaxActiveIndicators = 20,
                MaxUIAnimations = 10,
                UIAnimationsEnabled = _enableUIAnimations
            };
        }
        
        private void SetupWorldIndicators()
        {
            // Create indicator container
            var containerGO = new GameObject("IndicatorContainer");
            containerGO.transform.SetParent(transform);
            _indicatorContainer = containerGO.transform;
            
            // Pre-populate indicator pool
            for (int i = 0; i < 10; i++)
            {
                if (_worldIndicatorPrefab != null)
                {
                    var indicator = CreatePooledIndicator();
                    _indicatorPool.Enqueue(indicator);
                }
            }
            
            LogInfo($"Created world indicator pool with {_indicatorPool.Count} indicators");
        }
        
        private GameObject CreatePooledIndicator()
        {
            var go = _worldIndicatorPrefab != null 
                ? Instantiate(_worldIndicatorPrefab, _indicatorContainer) 
                : new GameObject("PooledIndicator");
            
            go.SetActive(false);
            return go;
        }
        
        private void ConnectToGameSystems()
        {
            // PHASE 1 PC-007: Removed direct manager references - using event-driven communication only
            // Direct manager access violates architectural principles established in Phase 1
            // All system communication now uses GameEventSO channels
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            // PHASE 1 PC-007: Event connections now handled through GameEventSO channels
            // Direct manager event subscriptions removed - will be replaced with event-driven communication
            
            // TODO Phase 2: Subscribe to GameEventSO channels for:
            // - Plant events (OnPlantAdded, OnPlantStageChanged, OnPlantHarvested, OnPlantHealthUpdated)
            // - Environmental events (OnConditionsChanged, OnAlertTriggered)
            // - Construction events (OnProjectStarted, OnProjectCompleted, OnConstructionIssue)
            // - Economic events (OnSaleCompleted, OnProfitGenerated)
            
            LogInfo("Visual Feedback System: Event system connections deferred to Phase 2 event channel implementation");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        #endregion
        
        #region Screen Effects
        
        public void TriggerScreenShake(float intensity, float duration, Vector3? epicenter = null)
        {
            if (_currentShakeCoroutine != null)
            {
                StopCoroutine(_currentShakeCoroutine);
            }
            
            _currentShakeCoroutine = StartCoroutine(ScreenShakeCoroutine(intensity, duration, epicenter));
        }
        
        private IEnumerator ScreenShakeCoroutine(float intensity, float duration, Vector3? epicenter)
        {
            if (_mainCamera == null) yield break;
            
            Vector3 originalPosition = _mainCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentIntensity = intensity * _shakeCurve.Evaluate(progress);
                
                // Calculate distance-based intensity if epicenter is provided
                if (epicenter.HasValue)
                {
                    float distance = Vector3.Distance(_mainCamera.transform.position, epicenter.Value);
                    float distanceMultiplier = Mathf.Clamp01(1f - (distance / 50f)); // 50 unit falloff
                    currentIntensity *= distanceMultiplier;
                }
                
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * currentIntensity * _maxShakeIntensity;
                randomOffset.z = 0f; // Don't shake forward/backward
                
                _mainCamera.transform.localPosition = originalPosition + randomOffset;
                
                yield return null;
            }
            
            _mainCamera.transform.localPosition = originalPosition;
            _currentShakeCoroutine = null;
        }
        
        public void TriggerScreenFlash(Color flashColor, float intensity = 1f, float duration = 0.2f)
        {
            var flashEffect = new ScreenFlashEffect
            {
                Color = flashColor,
                Intensity = intensity,
                Duration = duration,
                StartTime = Time.time
            };
            
            _activeFlashEffects.Add(flashEffect);
            StartCoroutine(ScreenFlashCoroutine(flashEffect));
        }
        
        private IEnumerator ScreenFlashCoroutine(ScreenFlashEffect flashEffect)
        {
            float elapsed = 0f;
            
            while (elapsed < flashEffect.Duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / flashEffect.Duration;
                float currentIntensity = flashEffect.Intensity * (1f - progress);
                
                // Apply flash effect to screen
                ApplyScreenFlash(flashEffect.Color, currentIntensity);
                
                yield return null;
            }
            
            _activeFlashEffects.Remove(flashEffect);
        }
        
        private void ApplyScreenFlash(Color color, float intensity)
        {
            // Implementation would use post-processing or overlay canvas
            // For now, just track the effect
        }
        
        #endregion
        
        #region World Indicators
        
        public void ShowWorldIndicator(Vector3 worldPosition, IndicatorType indicatorType, string message = "", float duration = 3f)
        {
            var indicator = GetPooledIndicator();
            if (indicator == null) return;
            
            var worldIndicator = new WorldIndicator
            {
                GameObject = indicator,
                Position = worldPosition,
                Type = indicatorType,
                Message = message,
                Duration = duration,
                StartTime = Time.time
            };
            
            ConfigureWorldIndicator(worldIndicator);
            _activeIndicators.Add(worldIndicator);
        }
        
        private GameObject GetPooledIndicator()
        {
            GameObject indicator;
            
            if (_indicatorPool.Count > 0)
            {
                indicator = _indicatorPool.Dequeue();
            }
            else
            {
                indicator = CreatePooledIndicator();
            }
            
            indicator.SetActive(true);
            return indicator;
        }
        
        private void ConfigureWorldIndicator(WorldIndicator indicator)
        {
            indicator.GameObject.transform.position = indicator.Position;
            
            // Configure indicator appearance based on type
            var renderer = indicator.GameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color indicatorColor = indicator.Type switch
                {
                    IndicatorType.Success => _successColor,
                    IndicatorType.Warning => _warningColor,
                    IndicatorType.Error => _errorColor,
                    IndicatorType.Info => _infoColor,
                    _ => Color.white
                };
                
                renderer.material.color = indicatorColor;
            }
            
            // Add text component for message if available
            var textMesh = indicator.GameObject.GetComponent<TextMesh>();
            if (textMesh != null && !string.IsNullOrEmpty(indicator.Message))
            {
                textMesh.text = indicator.Message;
            }
            
            // Start animation
            StartCoroutine(AnimateWorldIndicator(indicator));
        }
        
        private IEnumerator AnimateWorldIndicator(WorldIndicator indicator)
        {
            Transform transform = indicator.GameObject.transform;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + Vector3.up * 2f;
            Vector3 startScale = Vector3.one * 0.1f;
            Vector3 endScale = Vector3.one;
            
            float elapsed = 0f;
            float animationDuration = Mathf.Min(indicator.Duration * 0.3f, 1f);
            
            // Animate in
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                
                transform.position = Vector3.Lerp(startPosition, endPosition, progress);
                transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                
                yield return null;
            }
            
            // Wait for display duration
            yield return new WaitForSeconds(indicator.Duration - animationDuration * 2f);
            
            // Animate out
            elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                
                transform.localScale = Vector3.Lerp(endScale, Vector3.zero, progress);
                
                var renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - progress;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            ReturnIndicatorToPool(indicator);
        }
        
        private void ReturnIndicatorToPool(WorldIndicator indicator)
        {
            indicator.GameObject.SetActive(false);
            indicator.GameObject.transform.localScale = Vector3.one;
            
            var renderer = indicator.GameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f;
                renderer.material.color = color;
            }
            
            _indicatorPool.Enqueue(indicator.GameObject);
            _activeIndicators.Remove(indicator);
        }
        
        public void ShowProgressIndicator(Vector3 worldPosition, float progress, string label = "")
        {
            // Find existing progress indicator at this position or create new one
            var existingIndicator = _activeIndicators.FirstOrDefault(i => 
                Vector3.Distance(i.Position, worldPosition) < 1f && i.Type == IndicatorType.Progress);
            
            if (existingIndicator != null)
            {
                UpdateProgressIndicator(existingIndicator, progress, label);
            }
            else
            {
                CreateProgressIndicator(worldPosition, progress, label);
            }
        }
        
        private void CreateProgressIndicator(Vector3 position, float progress, string label)
        {
            var indicator = GetPooledIndicator();
            if (indicator == null) return;
            
            var worldIndicator = new WorldIndicator
            {
                GameObject = indicator,
                Position = position,
                Type = IndicatorType.Progress,
                Message = label,
                Duration = float.MaxValue, // Progress indicators persist until manually removed
                StartTime = Time.time,
                Progress = progress
            };
            
            ConfigureProgressIndicator(worldIndicator);
            _activeIndicators.Add(worldIndicator);
        }
        
        private void ConfigureProgressIndicator(WorldIndicator indicator)
        {
            // Setup progress bar visualization
            var progressBar = indicator.GameObject.GetComponent<ProgressBarComponent>();
            if (progressBar == null)
            {
                progressBar = indicator.GameObject.AddComponent<ProgressBarComponent>();
            }
            
            progressBar.SetProgress(indicator.Progress);
            progressBar.SetLabel(indicator.Message);
        }
        
        private void UpdateProgressIndicator(WorldIndicator indicator, float progress, string label)
        {
            indicator.Progress = progress;
            indicator.Message = label;
            
            var progressBar = indicator.GameObject.GetComponent<ProgressBarComponent>();
            if (progressBar != null)
            {
                progressBar.SetProgress(progress);
                progressBar.SetLabel(label);
            }
        }
        
        #endregion
        
        #region UI Feedback
        
        public void AnimateUIElement(VisualElement element, UIAnimationType animationType, float duration = -1f)
        {
            if (!_enableUIAnimations || element == null) return;
            
            if (duration < 0f) duration = _defaultAnimationDuration;
            
            var animation = new UIFeedbackAnimation
            {
                Element = element,
                AnimationType = animationType,
                Duration = duration,
                StartTime = Time.time,
                StartValue = GetElementCurrentValue(element, animationType),
                EndValue = GetElementTargetValue(element, animationType)
            };
            
            _activeUIAnimations[element] = animation;
            StartCoroutine(AnimateUIElementCoroutine(animation));
        }
        
        private Vector4 GetElementCurrentValue(VisualElement element, UIAnimationType animationType)
        {
            return animationType switch
            {
                UIAnimationType.Scale => new Vector4(element.transform.scale.x, element.transform.scale.y, 0f, 0f),
                UIAnimationType.Opacity => new Vector4(element.style.opacity.value, 0f, 0f, 0f),
                UIAnimationType.Position => new Vector4(element.style.left.value.value, element.style.top.value.value, 0f, 0f),
                UIAnimationType.Color => Vector4.one, // Would get actual color
                _ => Vector4.zero
            };
        }
        
        private Vector4 GetElementTargetValue(VisualElement element, UIAnimationType animationType)
        {
            return animationType switch
            {
                UIAnimationType.Scale => new Vector4(1.1f, 1.1f, 0f, 0f), // Slight scale up
                UIAnimationType.Opacity => new Vector4(0.8f, 0f, 0f, 0f), // Slight fade
                UIAnimationType.Position => new Vector4(element.style.left.value.value + 5f, element.style.top.value.value, 0f, 0f),
                UIAnimationType.Color => new Vector4(1.2f, 1.2f, 1.2f, 1f), // Brighten
                _ => Vector4.one
            };
        }
        
        private IEnumerator AnimateUIElementCoroutine(UIFeedbackAnimation animation)
        {
            float elapsed = 0f;
            
            while (elapsed < animation.Duration)
            {
                elapsed += Time.deltaTime;
                float progress = _defaultEasing.Evaluate(elapsed / animation.Duration);
                
                Vector4 currentValue = Vector4.Lerp(animation.StartValue, animation.EndValue, progress);
                ApplyUIAnimationValue(animation.Element, animation.AnimationType, currentValue);
                
                yield return null;
            }
            
            // Animate back to original
            elapsed = 0f;
            while (elapsed < animation.Duration)
            {
                elapsed += Time.deltaTime;
                float progress = _defaultEasing.Evaluate(elapsed / animation.Duration);
                
                Vector4 currentValue = Vector4.Lerp(animation.EndValue, animation.StartValue, progress);
                ApplyUIAnimationValue(animation.Element, animation.AnimationType, currentValue);
                
                yield return null;
            }
            
            _activeUIAnimations.Remove(animation.Element);
        }
        
        private void ApplyUIAnimationValue(VisualElement element, UIAnimationType animationType, Vector4 value)
        {
            switch (animationType)
            {
                case UIAnimationType.Scale:
                    element.transform.scale = new Vector3(value.x, value.y, 1f);
                    break;
                case UIAnimationType.Opacity:
                    element.style.opacity = value.x;
                    break;
                case UIAnimationType.Position:
                    element.style.left = value.x;
                    element.style.top = value.y;
                    break;
                case UIAnimationType.Color:
                    // Would apply color changes
                    break;
            }
        }
        
        public void TriggerHapticFeedback(HapticType hapticType)
        {
            if (!_enableHapticFeedback) return;
            
            // Implementation would depend on platform
            // For mobile: Handheld.Vibrate()
            // For console: controller rumble
            // For PC: potentially audio cues
        }
        
        #endregion
        
        #region Visual Confirmations
        
        public void ShowVisualConfirmation(Vector3 worldPosition, ConfirmationType confirmationType, string message = "")
        {
            var confirmation = new VisualConfirmation
            {
                Position = worldPosition,
                Type = confirmationType,
                Message = message,
                StartTime = Time.time,
                Duration = 2f
            };
            
            _activeConfirmations.Add(confirmation);
            _confirmationRenderer.RenderConfirmation(confirmation);
            
            OnVisualConfirmation?.Invoke(message);
        }
        
        public void ShowUIConfirmation(VisualElement element, ConfirmationType confirmationType)
        {
            // Add visual confirmation to UI element
            element.AddToClassList($"confirmation-{confirmationType.ToString().ToLower()}");
            
            // Animate confirmation
            AnimateUIElement(element, UIAnimationType.Scale, 0.5f);
            
            // Remove confirmation class after delay
            StartCoroutine(RemoveUIConfirmationClass(element, confirmationType, 1f));
        }
        
        private IEnumerator RemoveUIConfirmationClass(VisualElement element, ConfirmationType confirmationType, float delay)
        {
            yield return new WaitForSeconds(delay);
            element.RemoveFromClassList($"confirmation-{confirmationType.ToString().ToLower()}");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantAdded(PlantInstance plant)
        {
            ShowWorldIndicator(plant.transform.position, IndicatorType.Success, "Plant Added", 2f);
            TriggerScreenFlash(_successColor, 0.3f, 0.1f);
        }
        
        private void HandlePlantStageChanged(PlantInstance plant)
        {
            string message = $"Stage: {plant.CurrentGrowthStage}";
            ShowWorldIndicator(plant.transform.position, IndicatorType.Info, message, 3f);
        }
        
        private void HandlePlantHarvested(PlantInstance plant)
        {
            ShowWorldIndicator(plant.transform.position, IndicatorType.Success, "Harvested!", 3f);
            TriggerScreenShake(0.2f, 0.3f, plant.transform.position);
            ShowVisualConfirmation(plant.transform.position, ConfirmationType.Success, "Plant Harvested");
        }
        
        private void HandlePlantHealthChanged(PlantInstance plant)
        {
            float health = plant.CurrentHealth;
            
            if (health < 30f)
            {
                ShowWorldIndicator(plant.transform.position, IndicatorType.Warning, "Low Health", 4f);
            }
            else if (health > 80f)
            {
                ShowWorldIndicator(plant.transform.position, IndicatorType.Success, "Healthy", 2f);
            }
        }
        
        private void HandleEnvironmentalChange(EnvironmentalConditions conditions)
        {
            // Show environmental warnings
            if (conditions.Temperature > 30f || conditions.Temperature < 18f)
            {
                ShowWorldIndicator(Vector3.zero, IndicatorType.Warning, "Temperature Alert", 3f);
            }
            
            if (conditions.Humidity > 80f || conditions.Humidity < 30f)
            {
                ShowWorldIndicator(Vector3.zero, IndicatorType.Warning, "Humidity Alert", 3f);
            }
        }
        
        private void HandleEnvironmentalAlert(EnvironmentalAlert alert)
        {
            ShowWorldIndicator(Vector3.zero, IndicatorType.Warning, alert.Message, 4f);
            TriggerScreenShake(0.2f, 0.3f);
        }
        
        private void HandleConstructionStarted(object projectObj)
        {
            ShowWorldIndicator(Vector3.zero, IndicatorType.Info, "Construction Started", 3f);
            ShowProgressIndicator(Vector3.zero, 0f, "Construction Project");
        }
        
        private void HandleConstructionCompleted(object projectObj)
        {
            ShowWorldIndicator(Vector3.zero, IndicatorType.Success, "Construction Complete!", 4f);
            TriggerScreenShake(0.4f, 0.6f, Vector3.zero);
            ShowVisualConfirmation(Vector3.zero, ConfirmationType.Success, "Construction Completed");
            
            // Remove progress indicator
            var progressIndicator = _activeIndicators.FirstOrDefault(i => 
                Vector3.Distance(i.Position, Vector3.zero) < 1f && i.Type == IndicatorType.Progress);
            if (progressIndicator != null)
            {
                ReturnIndicatorToPool(progressIndicator);
            }
        }
        
        private void HandleConstructionIssue(object issueObj)
        {
            ShowWorldIndicator(Vector3.zero, IndicatorType.Warning, "Construction issue detected", 4f);
        }
        
        private void HandleSaleCompleted(MarketTransaction transaction)
        {
            string message = $"+${transaction.TotalValue:F0}";
            ShowWorldIndicator(Vector3.zero, IndicatorType.Success, message, 3f);
            TriggerScreenFlash(_successColor, 0.2f, 0.1f);
        }
        
        private void HandleProfitGenerated(float profit)
        {
            if (profit > 0)
            {
                string message = $"Profit: +${profit:F0}";
                ShowWorldIndicator(Vector3.zero, IndicatorType.Success, message, 2f);
            }
        }
        
        #endregion
        
        #region Update and Cleanup
        
        private void UpdateActiveIndicators()
        {
            for (int i = _activeIndicators.Count - 1; i >= 0; i--)
            {
                var indicator = _activeIndicators[i];
                
                if (indicator.Type == IndicatorType.Progress)
                {
                    // Progress indicators are managed externally
                    continue;
                }
                
                if (Time.time - indicator.StartTime >= indicator.Duration)
                {
                    ReturnIndicatorToPool(indicator);
                }
            }
        }
        
        private void UpdateUIAnimations()
        {
            // UI animations are handled by coroutines
        }
        
        private void UpdateVisualConfirmations()
        {
            for (int i = _activeConfirmations.Count - 1; i >= 0; i--)
            {
                var confirmation = _activeConfirmations[i];
                
                if (Time.time - confirmation.StartTime >= confirmation.Duration)
                {
                    _activeConfirmations.RemoveAt(i);
                }
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (Time.time - _lastMetricsUpdate >= 1f)
            {
                _performanceMetrics.ActiveIndicators = _activeIndicators.Count;
                _performanceMetrics.ActiveUIAnimations = _activeUIAnimations.Count;
                _performanceMetrics.ActiveConfirmations = _activeConfirmations.Count;
                _performanceMetrics.ScreenEffects = _activeFlashEffects.Count + (_currentShakeCoroutine != null ? 1 : 0);
                _performanceMetrics.LastUpdate = DateTime.Now;
                
                _lastMetricsUpdate = Time.time;
            }
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        private void CleanupFinishedEffects()
        {
            // Clean up finished flash effects
            _activeFlashEffects.RemoveAll(effect => Time.time - effect.StartTime >= effect.Duration);
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetUIAnimationsEnabled(bool enabled)
        {
            _enableUIAnimations = enabled;
            _performanceMetrics.UIAnimationsEnabled = enabled;
        }
        
        public void SetHapticFeedbackEnabled(bool enabled)
        {
            _enableHapticFeedback = enabled;
        }
        
        public void ClearAllIndicators()
        {
            foreach (var indicator in _activeIndicators.ToList())
            {
                ReturnIndicatorToPool(indicator);
            }
        }
        
        public void ClearAllUIAnimations()
        {
            foreach (var animation in _activeUIAnimations.Values.ToList())
            {
                StopCoroutine(AnimateUIElementCoroutine(animation));
            }
            _activeUIAnimations.Clear();
        }
        
        public FeedbackSystemReport GetSystemReport()
        {
            return new FeedbackSystemReport
            {
                Metrics = _performanceMetrics,
                ActiveIndicatorTypes = _activeIndicators.GroupBy(i => i.Type).ToDictionary(g => g.Key, g => g.Count()),
                ActiveAnimationTypes = _activeUIAnimations.Values.GroupBy(a => a.AnimationType).ToDictionary(g => g.Key, g => g.Count()),
                PooledIndicators = _indicatorPool.Count
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop all coroutines
            StopAllCoroutines();
            
            // Clear all active effects
            ClearAllIndicators();
            ClearAllUIAnimations();
            
            // Return all indicators to pool
            foreach (var indicator in _indicatorPool)
            {
                if (indicator != null)
                {
                    DestroyImmediate(indicator);
                }
            }
            
            // Disconnect events
            DisconnectSystemEvents();
            
            LogInfo("Visual Feedback System shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            // PHASE 1 PC-007: Event disconnections now handled through GameEventSO channels
            // Direct manager event unsubscriptions removed - will be replaced with event-driven communication
            
            // TODO Phase 2: Unsubscribe from GameEventSO channels when implemented
            
            LogInfo("Visual Feedback System: Event system disconnections deferred to Phase 2 event channel implementation");
        }
    }
}