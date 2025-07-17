using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Effects
{
    /// <summary>
    /// Data structures for the visual feedback system.
    /// Includes feedback types, performance metrics, animations, and confirmation systems.
    /// </summary>
    
    // Core Enums
    public enum FeedbackType
    {
        Success,
        Warning,
        Error,
        Info,
        Progress,
        Achievement,
        Notification
    }
    
    public enum IndicatorType
    {
        Success,
        Warning,
        Error,
        Info,
        Progress,
        Achievement
    }
    
    public enum ConfirmationType
    {
        Success,
        Warning,
        Error,
        Info,
        Purchase,
        Sale,
        Achievement
    }
    
    public enum UIAnimationType
    {
        Scale,
        Opacity,
        Position,
        Color,
        Rotation,
        Size,
        Fade,
        Slide,
        Bounce
    }
    
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error
    }
    
    // Screen Effects
    [System.Serializable]
    public class ScreenFlashEffect
    {
        public Color Color;
        public float Intensity;
        public float Duration;
        public float StartTime;
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
    
    [System.Serializable]
    public class ScreenShakeData
    {
        public float Intensity;
        public float Duration;
        public Vector3 Epicenter;
        public float Falloff = 50f;
        public AnimationCurve ShakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
    
    // World Indicators
    [System.Serializable]
    public class WorldIndicator
    {
        public GameObject GameObject;
        public Vector3 Position;
        public IndicatorType Type;
        public string Message;
        public float Duration;
        public float StartTime;
        public float Progress; // For progress indicators
        public bool IsAnimating;
        public WorldIndicatorSettings Settings;
    }
    
    [System.Serializable]
    public class WorldIndicatorSettings
    {
        public Vector3 AnimationOffset = Vector3.up * 2f;
        public float ScaleFrom = 0.1f;
        public float ScaleTo = 1f;
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve FadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public bool FollowCamera = true;
        public float MinDistance = 2f;
        public float MaxDistance = 100f;
    }
    
    // Progress Bar Component
    public class ProgressBarComponent : MonoBehaviour
    {
        [SerializeField] private Transform _fillTransform;
        [SerializeField] private TMPro.TextMeshPro _labelText;
        [SerializeField] private TMPro.TextMeshPro _percentageText;
        [SerializeField] private Renderer _fillRenderer;
        [SerializeField] private Gradient _progressGradient;
        
        private float _currentProgress = 0f;
        
        public void SetProgress(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);
            UpdateVisuals();
        }
        
        public void SetLabel(string label)
        {
            if (_labelText != null)
            {
                _labelText.text = label;
            }
        }
        
        private void UpdateVisuals()
        {
            // Update fill amount
            if (_fillTransform != null)
            {
                Vector3 scale = _fillTransform.localScale;
                scale.x = _currentProgress;
                _fillTransform.localScale = scale;
            }
            
            // Update color based on progress
            if (_fillRenderer != null && _progressGradient != null)
            {
                Color progressColor = _progressGradient.Evaluate(_currentProgress);
                _fillRenderer.material.color = progressColor;
            }
            
            // Update percentage text
            if (_percentageText != null)
            {
                _percentageText.text = $"{_currentProgress * 100f:F0}%";
            }
        }
        
        private void Start()
        {
            if (_progressGradient == null)
            {
                _progressGradient = new Gradient();
                _progressGradient.SetKeys(
                    new GradientColorKey[] 
                    { 
                        new GradientColorKey(Color.red, 0f),
                        new GradientColorKey(Color.yellow, 0.5f),
                        new GradientColorKey(Color.green, 1f)
                    },
                    new GradientAlphaKey[] 
                    { 
                        new GradientAlphaKey(1f, 0f), 
                        new GradientAlphaKey(1f, 1f) 
                    }
                );
            }
        }
    }
    
    // UI Feedback
    [System.Serializable]
    public class UIFeedbackAnimation
    {
        public VisualElement Element;
        public UIAnimationType AnimationType;
        public float Duration;
        public float StartTime;
        public Vector4 StartValue;
        public Vector4 EndValue;
        public AnimationCurve EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public bool IsLooping;
        public int LoopCount = 1;
        public bool PingPong;
    }
    
    public class UIFeedbackController
    {
        private Dictionary<string, UIFeedbackTemplate> _feedbackTemplates = new Dictionary<string, UIFeedbackTemplate>();
        
        public UIFeedbackController()
        {
            InitializeDefaultTemplates();
        }
        
        private void InitializeDefaultTemplates()
        {
            // Button hover template
            _feedbackTemplates["button_hover"] = new UIFeedbackTemplate
            {
                TemplateId = "button_hover",
                AnimationType = UIAnimationType.Scale,
                Duration = 0.2f,
                ScaleMultiplier = 1.05f,
                EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
            
            // Button click template
            _feedbackTemplates["button_click"] = new UIFeedbackTemplate
            {
                TemplateId = "button_click",
                AnimationType = UIAnimationType.Scale,
                Duration = 0.1f,
                ScaleMultiplier = 0.95f,
                EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
            
            // Success notification template
            _feedbackTemplates["success_notification"] = new UIFeedbackTemplate
            {
                TemplateId = "success_notification",
                AnimationType = UIAnimationType.Color,
                Duration = 0.5f,
                ColorTint = Color.green,
                EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
            
            // Error notification template
            _feedbackTemplates["error_notification"] = new UIFeedbackTemplate
            {
                TemplateId = "error_notification",
                AnimationType = UIAnimationType.Color,
                Duration = 0.5f,
                ColorTint = Color.red,
                EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            };
        }
        
        public UIFeedbackTemplate GetTemplate(string templateId)
        {
            return _feedbackTemplates.TryGetValue(templateId, out var template) ? template : null;
        }
        
        public void RegisterTemplate(string templateId, UIFeedbackTemplate template)
        {
            _feedbackTemplates[templateId] = template;
        }
    }
    
    [System.Serializable]
    public class UIFeedbackTemplate
    {
        public string TemplateId;
        public UIAnimationType AnimationType;
        public float Duration = 0.3f;
        public float ScaleMultiplier = 1.1f;
        public Color ColorTint = Color.white;
        public Vector2 PositionOffset = Vector2.zero;
        public float OpacityMultiplier = 1f;
        public AnimationCurve EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public bool AutoReverse = true;
        public float DelayBetweenAnimations = 0f;
    }
    
    // Visual Confirmations
    [System.Serializable]
    public class VisualConfirmation
    {
        public Vector3 Position;
        public ConfirmationType Type;
        public string Message;
        public float StartTime;
        public float Duration;
        public Color Color;
        public float Intensity = 1f;
        public ConfirmationStyle Style;
    }
    
    [System.Serializable]
    public class ConfirmationStyle
    {
        public bool ShowIcon = true;
        public bool ShowText = true;
        public bool ShowParticles = false;
        public bool ShowRing = true;
        public float IconSize = 1f;
        public float TextSize = 1f;
        public Vector3 AnimationOffset = Vector3.up;
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve OpacityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
    
    public class ConfirmationRenderer
    {
        private Camera _camera;
        private List<ConfirmationInstance> _activeConfirmations = new List<ConfirmationInstance>();
        
        public ConfirmationRenderer(Camera camera)
        {
            _camera = camera;
        }
        
        public void RenderConfirmation(VisualConfirmation confirmation)
        {
            var instance = new ConfirmationInstance
            {
                Confirmation = confirmation,
                StartTime = Time.time,
                Canvas = CreateConfirmationCanvas(confirmation)
            };
            
            _activeConfirmations.Add(instance);
        }
        
        private Canvas CreateConfirmationCanvas(VisualConfirmation confirmation)
        {
            var canvasGO = new GameObject($"Confirmation_{confirmation.Type}");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = _camera;
            
            // Position the canvas at the confirmation position
            canvasGO.transform.position = confirmation.Position;
            canvasGO.transform.LookAt(_camera.transform);
            
            // Create confirmation content
            CreateConfirmationContent(canvas, confirmation);
            
            return canvas;
        }
        
        private void CreateConfirmationContent(Canvas canvas, VisualConfirmation confirmation)
        {
            // Create icon
            if (confirmation.Style?.ShowIcon == true)
            {
                CreateConfirmationIcon(canvas, confirmation);
            }
            
            // Create text
            if (confirmation.Style?.ShowText == true && !string.IsNullOrEmpty(confirmation.Message))
            {
                CreateConfirmationText(canvas, confirmation);
            }
            
            // Create ring effect
            if (confirmation.Style?.ShowRing == true)
            {
                CreateConfirmationRing(canvas, confirmation);
            }
        }
        
        private void CreateConfirmationIcon(Canvas canvas, VisualConfirmation confirmation)
        {
            var iconGO = new GameObject("ConfirmationIcon");
            iconGO.transform.SetParent(canvas.transform, false);
            
            var image = iconGO.AddComponent<UnityEngine.UI.Image>();
            image.color = confirmation.Color;
            
            // Set icon sprite based on confirmation type
            // This would be populated with actual sprite assets
        }
        
        private void CreateConfirmationText(Canvas canvas, VisualConfirmation confirmation)
        {
            var textGO = new GameObject("ConfirmationText");
            textGO.transform.SetParent(canvas.transform, false);
            
            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = confirmation.Message;
            text.color = confirmation.Color;
            text.fontSize = 24f * (confirmation.Style?.TextSize ?? 1f);
            text.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Position text below icon
            var rectTransform = text.rectTransform;
            rectTransform.anchoredPosition = new Vector2(0f, -50f);
        }
        
        private void CreateConfirmationRing(Canvas canvas, VisualConfirmation confirmation)
        {
            var ringGO = new GameObject("ConfirmationRing");
            ringGO.transform.SetParent(canvas.transform, false);
            
            var image = ringGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(confirmation.Color.r, confirmation.Color.g, confirmation.Color.b, 0.3f);
            
            // Animate ring expansion
            var rectTransform = image.rectTransform;
            rectTransform.sizeDelta = Vector2.zero;
            
            // This would be animated via a coroutine or animation system
        }
        
        public void Update()
        {
            for (int i = _activeConfirmations.Count - 1; i >= 0; i--)
            {
                var confirmation = _activeConfirmations[i];
                
                if (Time.time - confirmation.StartTime >= confirmation.Confirmation.Duration)
                {
                    if (confirmation.Canvas != null)
                    {
                        UnityEngine.Object.DestroyImmediate(confirmation.Canvas.gameObject);
                    }
                    _activeConfirmations.RemoveAt(i);
                }
                else
                {
                    UpdateConfirmationAnimation(confirmation);
                }
            }
        }
        
        private void UpdateConfirmationAnimation(ConfirmationInstance instance)
        {
            float elapsed = Time.time - instance.StartTime;
            float progress = elapsed / instance.Confirmation.Duration;
            
            if (instance.Canvas != null)
            {
                // Update scale animation
                float scale = instance.Confirmation.Style?.ScaleCurve?.Evaluate(progress) ?? 1f;
                instance.Canvas.transform.localScale = Vector3.one * scale;
                
                // Update opacity animation
                float opacity = instance.Confirmation.Style?.OpacityCurve?.Evaluate(progress) ?? 1f;
                var canvasGroup = instance.Canvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = instance.Canvas.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = opacity;
                
                // Update position animation
                if (instance.Confirmation.Style != null)
                {
                    Vector3 offset = instance.Confirmation.Style.AnimationOffset * progress;
                    instance.Canvas.transform.position = instance.Confirmation.Position + offset;
                }
                
                // Keep facing camera
                instance.Canvas.transform.LookAt(_camera.transform);
            }
        }
    }
    
    [System.Serializable]
    public class ConfirmationInstance
    {
        public VisualConfirmation Confirmation;
        public float StartTime;
        public Canvas Canvas;
    }
    
    // Performance Metrics
    [System.Serializable]
    public class FeedbackPerformanceMetrics
    {
        public int ActiveIndicators;
        public int MaxActiveIndicators;
        public int ActiveUIAnimations;
        public int MaxUIAnimations;
        public int ActiveConfirmations;
        public int ScreenEffects;
        public bool UIAnimationsEnabled;
        public float MemoryUsage;
        public float CPUUsage;
        public DateTime LastUpdate;
        public Dictionary<IndicatorType, int> IndicatorTypeCounts = new Dictionary<IndicatorType, int>();
        public Dictionary<UIAnimationType, int> AnimationTypeCounts = new Dictionary<UIAnimationType, int>();
    }
    
    [System.Serializable]
    public class FeedbackSystemReport
    {
        public FeedbackPerformanceMetrics Metrics;
        public Dictionary<IndicatorType, int> ActiveIndicatorTypes;
        public Dictionary<UIAnimationType, int> ActiveAnimationTypes;
        public int PooledIndicators;
        public List<string> ActiveConfirmationMessages;
        public float AverageIndicatorDuration;
        public float AverageAnimationDuration;
    }
    
    // Audio Integration for Feedback
    [System.Serializable]
    public class FeedbackAudioSettings
    {
        public bool EnableAudioFeedback = true;
        public AudioClip SuccessSound;
        public AudioClip WarningSound;
        public AudioClip ErrorSound;
        public AudioClip InfoSound;
        public AudioClip ButtonHoverSound;
        public AudioClip ButtonClickSound;
        public AudioClip NotificationSound;
        public float Volume = 0.5f;
    }
    
    // Accessibility Features
    [System.Serializable]
    public class AccessibilitySettings
    {
        public bool HighContrastMode = false;
        public bool ReducedMotion = false;
        public bool ScreenReaderMode = false;
        public float TextSize = 1f;
        public float AnimationSpeed = 1f;
        public bool ShowTooltips = true;
        public bool UseColorBlindFriendlyPalette = false;
        public float HapticIntensity = 1f;
    }
    
    // Custom Events for Feedback System
    [System.Serializable]
    public class FeedbackEvent
    {
        public string EventId;
        public FeedbackType Type;
        public Vector3 Position;
        public string Message;
        public float Duration = 3f;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public DateTime Timestamp;
    }
    
    [System.Serializable]
    public class FeedbackEventQueue
    {
        private Queue<FeedbackEvent> _events = new Queue<FeedbackEvent>();
        private int _maxQueueSize = 50;
        
        public void EnqueueEvent(FeedbackEvent feedbackEvent)
        {
            if (_events.Count >= _maxQueueSize)
            {
                _events.Dequeue(); // Remove oldest event
            }
            
            _events.Enqueue(feedbackEvent);
        }
        
        public FeedbackEvent DequeueEvent()
        {
            return _events.Count > 0 ? _events.Dequeue() : null;
        }
        
        public int Count => _events.Count;
        
        public void Clear()
        {
            _events.Clear();
        }
    }
    
    // Feedback Templates for Different Game Events
    [CreateAssetMenu(fileName = "New Feedback Template Library", menuName = "Project Chimera/Effects/Feedback Template Library")]
    public class FeedbackTemplateLibrarySO : ScriptableObject
    {
        [SerializeField] private List<FeedbackEventTemplate> _templates = new List<FeedbackEventTemplate>();
        [SerializeField] private FeedbackAudioSettings _audioSettings;
        [SerializeField] private AccessibilitySettings _accessibilitySettings;
        
        private Dictionary<string, FeedbackEventTemplate> _templateLookup;
        
        public void InitializeDefaults()
        {
            if (_templates.Count == 0)
            {
                CreateDefaultTemplates();
            }
            
            BuildLookupTable();
        }
        
        private void CreateDefaultTemplates()
        {
            _templates.AddRange(new[]
            {
                CreateTemplate("plant_added", FeedbackType.Success, "Plant Added", 2f, Color.green),
                CreateTemplate("plant_harvested", FeedbackType.Achievement, "Plant Harvested!", 3f, Color.gold),
                CreateTemplate("plant_low_health", FeedbackType.Warning, "Low Health", 4f, Color.yellow),
                CreateTemplate("construction_started", FeedbackType.Info, "Construction Started", 3f, Color.blue),
                CreateTemplate("construction_completed", FeedbackType.Achievement, "Construction Complete!", 4f, Color.green),
                CreateTemplate("environmental_alert", FeedbackType.Error, "Environmental Alert", 5f, Color.red),
                CreateTemplate("sale_completed", FeedbackType.Success, "Sale Completed", 2f, Color.green),
                CreateTemplate("research_unlocked", FeedbackType.Achievement, "Research Unlocked!", 4f, Color.purple)
            });
        }
        
        private FeedbackEventTemplate CreateTemplate(string id, FeedbackType type, string message, float duration, Color color)
        {
            return new FeedbackEventTemplate
            {
                TemplateId = id,
                Type = type,
                DefaultMessage = message,
                DefaultDuration = duration,
                DefaultColor = color,
                ShowWorldIndicator = true,
                ShowScreenEffect = type == FeedbackType.Achievement || type == FeedbackType.Error,
                PlayAudio = true,
                TriggerHaptic = type == FeedbackType.Achievement || type == FeedbackType.Error
            };
        }
        
        private void BuildLookupTable()
        {
            _templateLookup = new Dictionary<string, FeedbackEventTemplate>();
            foreach (var template in _templates)
            {
                _templateLookup[template.TemplateId] = template;
            }
        }
        
        public FeedbackEventTemplate GetTemplate(string templateId)
        {
            return _templateLookup?.TryGetValue(templateId, out var template) == true ? template : null;
        }
        
        public List<FeedbackEventTemplate> GetTemplatesByType(FeedbackType type)
        {
            return _templates.Where(t => t.Type == type).ToList();
        }
        
        public FeedbackAudioSettings AudioSettings => _audioSettings;
        public AccessibilitySettings AccessibilitySettings => _accessibilitySettings;
    }
    
    [System.Serializable]
    public class FeedbackEventTemplate
    {
        public string TemplateId;
        public FeedbackType Type;
        public string DefaultMessage;
        public float DefaultDuration = 3f;
        public Color DefaultColor = Color.white;
        public bool ShowWorldIndicator = true;
        public bool ShowScreenEffect = false;
        public bool ShowUIAnimation = false;
        public bool PlayAudio = true;
        public bool TriggerHaptic = false;
        public float ScreenShakeIntensity = 0f;
        public float ScreenFlashIntensity = 0f;
        public UIAnimationType UIAnimation = UIAnimationType.Scale;
        public HapticType HapticType = HapticType.Light;
    }
}