using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.UI
{
    /// <summary>
    /// ScriptableObject for global UI configuration settings.
    /// Defines UI behavior, performance settings, and feature toggles.
    /// </summary>
    [CreateAssetMenu(fileName = "New UI Configuration", menuName = "Project Chimera/UI/Configuration")]
    public class UIConfigurationSO : ChimeraConfigSO
    {
        [Header("General Settings")]
        [SerializeField] private string _configurationName;
        [SerializeField] private string _description;
        [SerializeField] private UIProfile _targetProfile = UIProfile.Gaming;
        [SerializeField] private bool _enableDebugMode = false;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxUIUpdatesPerFrame = 10;
        [SerializeField] private float _uiUpdateInterval = 0.016f; // 60 FPS
        [SerializeField] private bool _enableUIPooling = true;
        [SerializeField] private int _poolInitialSize = 20;
        [SerializeField] private int _poolMaxSize = 100;
        [SerializeField] private bool _enableLODForUI = true;
        
        [Header("Animation Settings")]
        [SerializeField] private bool _enableAnimations = true;
        [SerializeField] private bool _enableTransitions = true;
        [SerializeField] private float _defaultAnimationDuration = 0.25f;
        [SerializeField] private AnimationCurve _defaultEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool _reduceMotion = false;
        [SerializeField] private float _motionReductionScale = 0.5f;
        
        [Header("Input Settings")]
        [SerializeField] private bool _enableKeyboardNavigation = true;
        [SerializeField] private bool _enableGamepadSupport = true;
        [SerializeField] private bool _enableTouchInput = false;
        [SerializeField] private float _doubleClickThreshold = 0.3f;
        [SerializeField] private float _longPressThreshold = 0.5f;
        [SerializeField] private bool _enableHoverEffects = true;
        
        [Header("Accessibility Settings")]
        [SerializeField] private bool _enableScreenReaderSupport = false;
        [SerializeField] private bool _enableHighContrastMode = false;
        [SerializeField] private bool _enableFocusIndicators = true;
        [SerializeField] private float _textScaleMultiplier = 1.0f;
        [SerializeField] private bool _enableColorBlindSupport = false;
        [SerializeField] private ColorBlindType _colorBlindType = ColorBlindType.None;
        
        [Header("Audio Settings")]
        [SerializeField] private bool _enableUIAudio = true;
        [SerializeField] private float _uiVolumeMultiplier = 1.0f;
        [SerializeField] private bool _enableHoverSounds = true;
        [SerializeField] private bool _enableClickSounds = true;
        [SerializeField] private bool _enableNotificationSounds = true;
        
        [Header("Notification Settings")]
        [SerializeField] private bool _enableNotifications = true;
        [SerializeField] private int _maxNotificationsVisible = 5;
        [SerializeField] private float _defaultNotificationDuration = 5.0f;
        [SerializeField] private bool _enableNotificationQueue = true;
        [SerializeField] private bool _groupSimilarNotifications = true;
        [SerializeField] private Vector2 _notificationPosition = new Vector2(20, 20);
        
        [Header("Modal Settings")]
        [SerializeField] private bool _enableModals = true;
        [SerializeField] private bool _modalBackgroundBlur = true;
        [SerializeField] private bool _closeModalOnBackgroundClick = true;
        [SerializeField] private bool _enableModalAnimations = true;
        [SerializeField] private float _modalFadeInDuration = 0.3f;
        
        [Header("Panel Settings")]
        [SerializeField] private bool _enablePanelCaching = true;
        [SerializeField] private int _maxCachedPanels = 10;
        [SerializeField] private bool _enablePanelPreloading = false;
        [SerializeField] private bool _enablePanelTransitions = true;
        [SerializeField] private PanelTransitionType _defaultTransitionType = PanelTransitionType.Fade;
        
        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugOverlay = false;
        [SerializeField] private bool _logUIEvents = false;
        [SerializeField] private bool _logPerformanceMetrics = false;
        [SerializeField] private bool _enableUIProfiler = false;
        [SerializeField] private KeyCode _debugToggleKey = KeyCode.F12;
        
        [Header("Layout Settings")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private ScreenMatchMode _screenMatchMode = ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] private float _matchWidthOrHeight = 0.5f;
        [SerializeField] private bool _enableResponsiveLayout = true;
        [SerializeField] private List<BreakpointSetting> _responsiveBreakpoints = new List<BreakpointSetting>();
        
        [Header("Localization Settings")]
        [SerializeField] private bool _enableLocalization = false;
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private bool _autoDetectLanguage = true;
        [SerializeField] private bool _enableRTLSupport = false;
        
        // Properties for easy access
        public string ConfigurationName => _configurationName;
        public string Description => _description;
        public UIProfile TargetProfile => _targetProfile;
        public bool EnableDebugMode => _enableDebugMode;
        
        // Performance properties
        public int MaxUIUpdatesPerFrame => _maxUIUpdatesPerFrame;
        public float UIUpdateInterval => _uiUpdateInterval;
        public bool EnableUIPooling => _enableUIPooling;
        public int PoolInitialSize => _poolInitialSize;
        public int PoolMaxSize => _poolMaxSize;
        public bool EnableLODForUI => _enableLODForUI;
        
        // Animation properties
        public bool EnableAnimations => _enableAnimations && !_reduceMotion;
        public bool EnableTransitions => _enableTransitions && !_reduceMotion;
        public float DefaultAnimationDuration => _reduceMotion ? _defaultAnimationDuration * _motionReductionScale : _defaultAnimationDuration;
        public AnimationCurve DefaultEasingCurve => _defaultEasingCurve;
        public bool ReduceMotion => _reduceMotion;
        
        // Input properties
        public bool EnableKeyboardNavigation => _enableKeyboardNavigation;
        public bool EnableGamepadSupport => _enableGamepadSupport;
        public bool EnableTouchInput => _enableTouchInput;
        public float DoubleClickThreshold => _doubleClickThreshold;
        public float LongPressThreshold => _longPressThreshold;
        public bool EnableHoverEffects => _enableHoverEffects;
        
        // Accessibility properties
        public bool EnableScreenReaderSupport => _enableScreenReaderSupport;
        public bool EnableHighContrastMode => _enableHighContrastMode;
        public bool EnableFocusIndicators => _enableFocusIndicators;
        public float TextScaleMultiplier => _textScaleMultiplier;
        public bool EnableColorBlindSupport => _enableColorBlindSupport;
        public ColorBlindType ColorBlindType => _colorBlindType;
        
        // Audio properties
        public bool EnableUIAudio => _enableUIAudio;
        public float UIVolumeMultiplier => _uiVolumeMultiplier;
        public bool EnableHoverSounds => _enableHoverSounds && _enableUIAudio;
        public bool EnableClickSounds => _enableClickSounds && _enableUIAudio;
        public bool EnableNotificationSounds => _enableNotificationSounds && _enableUIAudio;
        
        // Notification properties
        public bool EnableNotifications => _enableNotifications;
        public int MaxNotificationsVisible => _maxNotificationsVisible;
        public float DefaultNotificationDuration => _defaultNotificationDuration;
        public bool EnableNotificationQueue => _enableNotificationQueue;
        public bool GroupSimilarNotifications => _groupSimilarNotifications;
        public Vector2 NotificationPosition => _notificationPosition;
        
        // Modal properties
        public bool EnableModals => _enableModals;
        public bool ModalBackgroundBlur => _modalBackgroundBlur;
        public bool CloseModalOnBackgroundClick => _closeModalOnBackgroundClick;
        public bool EnableModalAnimations => _enableModalAnimations && EnableAnimations;
        public float ModalFadeInDuration => DefaultAnimationDuration;
        
        // Panel properties
        public bool EnablePanelCaching => _enablePanelCaching;
        public int MaxCachedPanels => _maxCachedPanels;
        public bool EnablePanelPreloading => _enablePanelPreloading;
        public bool EnablePanelTransitions => _enablePanelTransitions && EnableTransitions;
        public PanelTransitionType DefaultTransitionType => _defaultTransitionType;
        
        // Debug properties
        public bool ShowDebugOverlay => _showDebugOverlay && _enableDebugMode;
        public bool LogUIEvents => _logUIEvents && _enableDebugMode;
        public bool LogPerformanceMetrics => _logPerformanceMetrics && _enableDebugMode;
        public bool EnableUIProfiler => _enableUIProfiler && _enableDebugMode;
        public KeyCode DebugToggleKey => _debugToggleKey;
        
        // Layout properties
        public Vector2 ReferenceResolution => _referenceResolution;
        public ScreenMatchMode ScreenMatchMode => _screenMatchMode;
        public float MatchWidthOrHeight => _matchWidthOrHeight;
        public bool EnableResponsiveLayout => _enableResponsiveLayout;
        public List<BreakpointSetting> ResponsiveBreakpoints => _responsiveBreakpoints;
        
        // Localization properties
        public bool EnableLocalization => _enableLocalization;
        public string DefaultLanguage => _defaultLanguage;
        public bool AutoDetectLanguage => _autoDetectLanguage;
        public bool EnableRTLSupport => _enableRTLSupport;
        
        /// <summary>
        /// Get breakpoint setting for current screen width
        /// </summary>
        public BreakpointSetting GetCurrentBreakpoint(float screenWidth)
        {
            if (!_enableResponsiveLayout || _responsiveBreakpoints == null)
                return null;
            
            BreakpointSetting currentBreakpoint = null;
            foreach (var breakpoint in _responsiveBreakpoints)
            {
                if (screenWidth >= breakpoint.MinWidth)
                {
                    if (currentBreakpoint == null || breakpoint.MinWidth > currentBreakpoint.MinWidth)
                    {
                        currentBreakpoint = breakpoint;
                    }
                }
            }
            
            return currentBreakpoint;
        }
        
        /// <summary>
        /// Check if UI feature is enabled based on profile
        /// </summary>
        public bool IsFeatureEnabled(UIFeature feature)
        {
            return feature switch
            {
                UIFeature.Animations => EnableAnimations,
                UIFeature.Transitions => EnableTransitions,
                UIFeature.Audio => EnableUIAudio,
                UIFeature.Notifications => EnableNotifications,
                UIFeature.Modals => EnableModals,
                UIFeature.KeyboardNavigation => EnableKeyboardNavigation,
                UIFeature.GamepadSupport => EnableGamepadSupport,
                UIFeature.TouchInput => EnableTouchInput,
                UIFeature.ScreenReader => EnableScreenReaderSupport,
                UIFeature.HighContrast => EnableHighContrastMode,
                UIFeature.ColorBlindSupport => EnableColorBlindSupport,
                UIFeature.Localization => EnableLocalization,
                UIFeature.DebugMode => EnableDebugMode,
                _ => true
            };
        }
        
        /// <summary>
        /// Apply performance optimizations based on current settings
        /// </summary>
        public void ApplyPerformanceSettings()
        {
            if (_enableLODForUI)
            {
                // Apply UI LOD settings
                QualitySettings.pixelLightCount = Mathf.Max(1, QualitySettings.pixelLightCount);
            }
            
            // Configure physics update rate if needed
            Time.fixedDeltaTime = Mathf.Max(_uiUpdateInterval, Time.fixedDeltaTime);
        }
        
        /// <summary>
        /// Get effective animation duration considering motion reduction
        /// </summary>
        public float GetEffectiveAnimationDuration(float duration)
        {
            if (_reduceMotion)
            {
                return duration * _motionReductionScale;
            }
            return duration;
        }
        
        /// <summary>
        /// Create a copy of this configuration with modifications
        /// </summary>
        public UIConfigurationSO CreateVariant(string variantName)
        {
            var variant = CreateInstance<UIConfigurationSO>();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.CopySerializedIfDifferent(this, variant);
#endif
            variant._configurationName = variantName;
            return variant;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_configurationName))
            {
                _configurationName = name;
            }
            
            // Clamp values to reasonable ranges
            _maxUIUpdatesPerFrame = Mathf.Max(1, _maxUIUpdatesPerFrame);
            _uiUpdateInterval = Mathf.Max(0.001f, _uiUpdateInterval);
            _poolInitialSize = Mathf.Max(0, _poolInitialSize);
            _poolMaxSize = Mathf.Max(_poolInitialSize, _poolMaxSize);
            _textScaleMultiplier = Mathf.Clamp(_textScaleMultiplier, 0.5f, 3.0f);
            _uiVolumeMultiplier = Mathf.Clamp01(_uiVolumeMultiplier);
            _maxNotificationsVisible = Mathf.Max(1, _maxNotificationsVisible);
            _defaultNotificationDuration = Mathf.Max(0.1f, _defaultNotificationDuration);
            _maxCachedPanels = Mathf.Max(1, _maxCachedPanels);
        }
        
        public override bool ValidateData()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_configurationName))
            {
                LogError("Configuration name cannot be empty");
                isValid = false;
            }
            
            if (_maxUIUpdatesPerFrame < 1)
            {
                LogError("Max UI updates per frame must be at least 1");
                isValid = false;
            }
            
            if (_uiUpdateInterval <= 0)
            {
                LogError("UI update interval must be positive");
                isValid = false;
            }
            
            if (_poolMaxSize < _poolInitialSize)
            {
                LogError("Pool max size cannot be less than initial size");
                isValid = false;
            }
            
            if (_textScaleMultiplier < 0.5f || _textScaleMultiplier > 3.0f)
            {
                LogWarning("Text scale multiplier outside recommended range (0.5 - 3.0)");
            }
            
            if (_referenceResolution.x <= 0 || _referenceResolution.y <= 0)
            {
                LogError("Reference resolution must have positive dimensions");
                isValid = false;
            }
            
            if (_enableResponsiveLayout && (_responsiveBreakpoints == null || _responsiveBreakpoints.Count == 0))
            {
                LogWarning("Responsive layout enabled but no breakpoints defined");
            }
            
            return isValid;
        }
    }
    
    /// <summary>
    /// UI profile types for different use cases
    /// </summary>
    public enum UIProfile
    {
        Gaming,         // Optimized for gaming with animations and effects
        Professional,   // Clean, minimal interface for productivity
        Accessibility,  // Maximum accessibility features enabled
        Performance,    // Minimal features for best performance
        Mobile,         // Optimized for mobile/touch devices
        Custom          // Custom configuration
    }
    
    /// <summary>
    /// Screen match modes for responsive layout
    /// </summary>
    public enum ScreenMatchMode
    {
        MatchWidthOrHeight,
        MatchWidth,
        MatchHeight,
        Expand,
        Shrink
    }
    
    /// <summary>
    /// Panel transition types
    /// </summary>
    public enum PanelTransitionType
    {
        None,
        Fade,
        Slide,
        Scale,
        Custom
    }
    
    /// <summary>
    /// Color blind support types
    /// </summary>
    public enum ColorBlindType
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia,
        Achromatopsia
    }
    
    /// <summary>
    /// UI features that can be toggled
    /// </summary>
    public enum UIFeature
    {
        Animations,
        Transitions,
        Audio,
        Notifications,
        Modals,
        KeyboardNavigation,
        GamepadSupport,
        TouchInput,
        ScreenReader,
        HighContrast,
        ColorBlindSupport,
        Localization,
        DebugMode
    }
    
    /// <summary>
    /// Responsive breakpoint setting
    /// </summary>
    [System.Serializable]
    public class BreakpointSetting
    {
        [SerializeField] public string Name;
        [SerializeField] public float MinWidth;
        [SerializeField] public float ScaleFactor = 1.0f;
        [SerializeField] public bool EnableFeature = true;
        
        public BreakpointSetting(string name, float minWidth, float scaleFactor = 1.0f)
        {
            Name = name;
            MinWidth = minWidth;
            ScaleFactor = scaleFactor;
        }
    }
}