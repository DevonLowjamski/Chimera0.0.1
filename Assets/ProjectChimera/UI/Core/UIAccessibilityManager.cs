using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Comprehensive accessibility manager for UI systems in Project Chimera.
    /// Implements screen reader support, keyboard navigation, and accessibility enhancements.
    /// </summary>
    public class UIAccessibilityManager : ChimeraManager
    {
        [Header("Accessibility Configuration")]
        [SerializeField] private bool _enableAccessibilityFeatures = true;
        [SerializeField] private bool _enableScreenReaderSupport = true;
        [SerializeField] private bool _enableKeyboardNavigation = true;
        [SerializeField] private bool _enableHighContrastMode = false;
        [SerializeField] private bool _enableFocusIndicators = true;
        
        [Header("Screen Reader Settings")]
        [SerializeField] private bool _enableLiveRegions = true;
        [SerializeField] private bool _enableAriaLabels = true;
        [SerializeField] private float _announceDelay = 0.5f;
        [SerializeField] private int _maxAnnouncementLength = 200;
        
        [Header("Keyboard Navigation")]
        [SerializeField] private bool _enableTabNavigation = true;
        [SerializeField] private bool _enableArrowKeyNavigation = true;
        [SerializeField] private bool _enableEscapeKeyHandling = true;
        [SerializeField] private bool _enableEnterKeyActivation = true;
        [SerializeField] private float _navigationRepeatDelay = 0.5f;
        
        [Header("Visual Accessibility")]
        [SerializeField] private float _textScaleMultiplier = 1f;
        [SerializeField] private bool _enableColorBlindSupport = false;
        [SerializeField] private ColorBlindnessType _colorBlindnessType = ColorBlindnessType.None;
        [SerializeField] private UIAccessibilityThemeSO _accessibilityTheme;
        
        [Header("Audio Accessibility")]
        [SerializeField] private bool _enableAudioCues = true;
        [SerializeField] private float _audioVolume = 0.7f;
        [SerializeField] private AudioClip _focusSound;
        [SerializeField] private AudioClip _selectSound;
        [SerializeField] private AudioClip _errorSound;
        
        // Screen reader system
        private UIScreenReader _screenReader;
        private Queue<UIAnnouncement> _announcementQueue;
        private float _lastAnnouncementTime;
        
        // Keyboard navigation
        private UIKeyboardNavigator _keyboardNavigator;
        private VisualElement _currentFocusedElement;
        private List<VisualElement> _focusableElements;
        private Dictionary<VisualElement, UIAccessibilityInfo> _accessibilityInfo;
        
        // Audio system
        private AudioSource _audioSource;
        
        // Visual accessibility
        private Dictionary<VisualElement, UIColorAdjustment> _colorAdjustments;
        private Dictionary<VisualElement, UITextScaling> _textScaling;
        
        // Live regions
        private Dictionary<string, UILiveRegion> _liveRegions;
        
        // Events
        public System.Action<string> OnScreenReaderAnnouncement;
        public System.Action<VisualElement> OnFocusChanged;
        public System.Action<UIAccessibilityEvent> OnAccessibilityEvent;
        public System.Action<bool> OnHighContrastModeChanged;
        
        // Properties
        public bool IsAccessibilityEnabled => _enableAccessibilityFeatures;
        public bool IsScreenReaderActive => _enableScreenReaderSupport && _screenReader != null;
        public bool IsScreenReaderEnabled => _enableScreenReaderSupport;
        public bool IsKeyboardNavigationEnabled => _enableKeyboardNavigation;
        public bool IsHighContrastEnabled => _enableHighContrastMode;
        public VisualElement CurrentFocusedElement => _currentFocusedElement;
        public int FocusableElementCount => _focusableElements?.Count ?? 0;
        public float TextScaleMultiplier => _textScaleMultiplier;
        
        protected override void OnManagerInitialize()
        {
            InitializeAccessibilitySystem();
            InitializeScreenReader();
            InitializeKeyboardNavigation();
            InitializeAudioSystem();
            InitializeVisualAccessibility();
            
            // LogInfo("UI Accessibility Manager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up screen reader
            if (_screenReader != null)
            {
                _screenReader.OnAnnouncementRequested -= HandleAnnouncementRequest;
                _screenReader.Shutdown();
                _screenReader = null;
            }
            
            // Clean up keyboard navigator
            if (_keyboardNavigator != null)
            {
                _keyboardNavigator.OnNavigationRequested -= HandleNavigationRequest;
                _keyboardNavigator.OnActivationRequested -= HandleActivationRequest;
                _keyboardNavigator.Shutdown();
                _keyboardNavigator = null;
            }
            
            // Clear collections
            _announcementQueue?.Clear();
            _focusableElements?.Clear();
            _accessibilityInfo?.Clear();
            _colorAdjustments?.Clear();
            _textScaling?.Clear();
            _liveRegions?.Clear();
            
            // LogInfo("UI Accessibility Manager shutdown complete");
        }
        
        /// <summary>
        /// Initialize accessibility system
        /// </summary>
        private void InitializeAccessibilitySystem()
        {
            _announcementQueue = new Queue<UIAnnouncement>();
            _focusableElements = new List<VisualElement>();
            _accessibilityInfo = new Dictionary<VisualElement, UIAccessibilityInfo>();
            _colorAdjustments = new Dictionary<VisualElement, UIColorAdjustment>();
            _textScaling = new Dictionary<VisualElement, UITextScaling>();
            _liveRegions = new Dictionary<string, UILiveRegion>();
            
            // Apply accessibility theme if available
            if (_accessibilityTheme != null)
            {
                ApplyAccessibilityTheme();
            }
        }
        
        /// <summary>
        /// Initialize screen reader system
        /// </summary>
        private void InitializeScreenReader()
        {
            if (!_enableScreenReaderSupport)
                return;
            
            _screenReader = new UIScreenReader();
            _screenReader.Initialize();
            
            _screenReader.OnAnnouncementRequested += HandleAnnouncementRequest;
            
            LogInfo("Screen reader system initialized");
        }
        
        /// <summary>
        /// Initialize keyboard navigation
        /// </summary>
        private void InitializeKeyboardNavigation()
        {
            if (!_enableKeyboardNavigation)
                return;
            
            _keyboardNavigator = new UIKeyboardNavigator();
            _keyboardNavigator.Initialize();
            
            _keyboardNavigator.OnNavigationRequested += HandleNavigationRequest;
            _keyboardNavigator.OnActivationRequested += HandleActivationRequest;
            
            LogInfo("Keyboard navigation initialized");
        }
        
        /// <summary>
        /// Initialize audio system
        /// </summary>
        private void InitializeAudioSystem()
        {
            if (!_enableAudioCues)
                return;
            
            var audioGO = new GameObject("AccessibilityAudio");
            audioGO.transform.SetParent(transform);
            
            _audioSource = audioGO.AddComponent<AudioSource>();
            _audioSource.volume = _audioVolume;
            _audioSource.playOnAwake = false;
            
            LogInfo("Audio accessibility system initialized");
        }
        
        /// <summary>
        /// Initialize visual accessibility features
        /// </summary>
        private void InitializeVisualAccessibility()
        {
            if (_enableHighContrastMode)
            {
                EnableHighContrastMode(true);
            }
            
            if (_enableColorBlindSupport && _colorBlindnessType != ColorBlindnessType.None)
            {
                ApplyColorBlindnessFilter(_colorBlindnessType);
            }
            
            ApplyTextScaling(_textScaleMultiplier);
        }
        
        /// <summary>
        /// Register element for accessibility
        /// </summary>
        public void RegisterElement(VisualElement element, UIAccessibilityInfo accessibilityInfo = null)
        {
            if (element == null || _accessibilityInfo.ContainsKey(element))
                return;
            
            var info = accessibilityInfo ?? CreateDefaultAccessibilityInfo(element);
            _accessibilityInfo[element] = info;
            
            // Add to focusable elements if appropriate
            if (info.IsFocusable)
            {
                _focusableElements.Add(element);
                SortFocusableElements();
            }
            
            // Set up ARIA attributes
            SetupAriaAttributes(element, info);
            
            // Set up keyboard event handlers
            SetupKeyboardHandlers(element, info);
            
            // Apply visual accessibility
            ApplyVisualAccessibility(element, info);
            
            LogInfo($"Registered element for accessibility: {element.name}");
        }
        
        /// <summary>
        /// Unregister element from accessibility
        /// </summary>
        public void UnregisterElement(VisualElement element)
        {
            if (element == null)
                return;
            
            _accessibilityInfo.Remove(element);
            _focusableElements.Remove(element);
            _colorAdjustments.Remove(element);
            _textScaling.Remove(element);
            
            if (_currentFocusedElement == element)
            {
                MoveFocusToNextElement();
            }
            
            LogInfo($"Unregistered element from accessibility: {element.name}");
        }
        
        /// <summary>
        /// Create default accessibility info for element
        /// </summary>
        private UIAccessibilityInfo CreateDefaultAccessibilityInfo(VisualElement element)
        {
            var info = new UIAccessibilityInfo
            {
                Role = DetermineElementRole(element),
                Label = GetElementLabel(element),
                Description = GetElementDescription(element),
                IsFocusable = IsElementFocusable(element),
                TabIndex = 0,
                LiveRegion = UILiveRegionType.None,
                KeyboardShortcut = KeyCode.None
            };
            
            return info;
        }
        
        /// <summary>
        /// Determine ARIA role for element
        /// </summary>
        private UIAriaRole DetermineElementRole(VisualElement element)
        {
            return element switch
            {
                Button => UIAriaRole.Button,
                Label => UIAriaRole.Text,
                TextField => UIAriaRole.TextBox,
                Toggle => UIAriaRole.CheckBox,
                DropdownField => UIAriaRole.ComboBox,
                Slider => UIAriaRole.Slider,
                ProgressBar => UIAriaRole.ProgressBar,
                ScrollView => UIAriaRole.ScrollBar,
                ListView => UIAriaRole.List,
                _ => UIAriaRole.Generic
            };
        }
        
        /// <summary>
        /// Get accessible label for element
        /// </summary>
        private string GetElementLabel(VisualElement element)
        {
            // Try to get label from various sources
            // Check for common text-containing elements
            if (element is Label label && !string.IsNullOrEmpty(label.text))
            {
                return label.text;
            }
            
            if (element is Button button && !string.IsNullOrEmpty(button.text))
            {
                return button.text;
            }
            
            if (element is TextField textField && !string.IsNullOrEmpty(textField.value))
            {
                return textField.value;
            }
            
            if (!string.IsNullOrEmpty(element.tooltip))
            {
                return element.tooltip;
            }
            
            if (!string.IsNullOrEmpty(element.name))
            {
                return element.name.Replace("-", " ").Replace("_", " ");
            }
            
            return element.GetType().Name;
        }
        
        /// <summary>
        /// Get accessible description for element
        /// </summary>
        private string GetElementDescription(VisualElement element)
        {
            return element.tooltip ?? "";
        }
        
        /// <summary>
        /// Check if element should be focusable
        /// </summary>
        private bool IsElementFocusable(VisualElement element)
        {
            return element.focusable || element is Button || element is TextField || element is Toggle;
        }
        
        /// <summary>
        /// Setup ARIA attributes for element
        /// </summary>
        private void SetupAriaAttributes(VisualElement element, UIAccessibilityInfo info)
        {
            if (!_enableAriaLabels)
                return;
            
            // Set ARIA role using userData instead of SetProperty
            element.userData = element.userData ?? new Dictionary<string, object>();
            var userData = element.userData as Dictionary<string, object>;
            userData["aria-role"] = info.Role.ToString().ToLower();
            
            // Set ARIA label
            if (!string.IsNullOrEmpty(info.Label))
            {
                userData["aria-label"] = info.Label;
            }
            
            // Set ARIA description
            if (!string.IsNullOrEmpty(info.Description))
            {
                userData["aria-describedby"] = info.Description;
            }
            
            // Set tab index
            if (info.IsFocusable)
            {
                element.tabIndex = info.TabIndex;
            }
            
            // Set live region
            if (info.LiveRegion != UILiveRegionType.None)
            {
                userData["aria-live"] = info.LiveRegion.ToString().ToLower();
            }
        }
        
        /// <summary>
        /// Setup keyboard event handlers
        /// </summary>
        private void SetupKeyboardHandlers(VisualElement element, UIAccessibilityInfo info)
        {
            if (!_enableKeyboardNavigation)
                return;
            
            element.RegisterCallback<KeyDownEvent>(OnElementKeyDown);
            element.RegisterCallback<FocusInEvent>(OnElementFocusIn);
            element.RegisterCallback<FocusOutEvent>(OnElementFocusOut);
        }
        
        /// <summary>
        /// Apply visual accessibility to element
        /// </summary>
        private void ApplyVisualAccessibility(VisualElement element, UIAccessibilityInfo info)
        {
            // Apply text scaling to text-containing elements
            if (element is Label || element is Button || element is TextField)
            {
                ApplyTextScalingToElement(element);
            }
            
            // Apply color adjustments
            if (_enableColorBlindSupport)
            {
                ApplyColorAdjustmentToElement(element);
            }
            
            // Apply focus indicators
            if (_enableFocusIndicators && info.IsFocusable)
            {
                SetupFocusIndicator(element);
            }
        }
        
        /// <summary>
        /// Handle keyboard input on element
        /// </summary>
        private void OnElementKeyDown(KeyDownEvent evt)
        {
            var element = evt.target as VisualElement;
            
            switch (evt.keyCode)
            {
                case KeyCode.Tab:
                    HandleTabNavigation(evt.shiftKey);
                    evt.StopPropagation();
                    evt.PreventDefault();
                    break;
                    
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.LeftArrow:
                case KeyCode.RightArrow:
                    if (_enableArrowKeyNavigation)
                    {
                        HandleArrowNavigation(evt.keyCode);
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                    break;
                    
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_enableEnterKeyActivation)
                    {
                        HandleElementActivation(element);
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                    break;
                    
                case KeyCode.Escape:
                    if (_enableEscapeKeyHandling)
                    {
                        HandleEscapeKey(element);
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                    break;
            }
            
            // Handle custom keyboard shortcuts
            if (_accessibilityInfo.TryGetValue(element, out var info) && info.KeyboardShortcut != KeyCode.None)
            {
                if (evt.keyCode == info.KeyboardShortcut)
                {
                    HandleElementActivation(element);
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }
        }
        
        /// <summary>
        /// Handle element focus in
        /// </summary>
        private void OnElementFocusIn(FocusInEvent evt)
        {
            var element = evt.target as VisualElement;
            SetCurrentFocus(element);
        }
        
        /// <summary>
        /// Handle element focus out
        /// </summary>
        private void OnElementFocusOut(FocusOutEvent evt)
        {
            var element = evt.target as VisualElement;
            
            if (_currentFocusedElement == element)
            {
                _currentFocusedElement = null;
            }
        }
        
        /// <summary>
        /// Set current focused element
        /// </summary>
        public void SetCurrentFocus(VisualElement element)
        {
            if (_currentFocusedElement == element)
                return;
            
            var previousElement = _currentFocusedElement;
            _currentFocusedElement = element;
            
            // Update focus indicators
            if (previousElement != null)
            {
                RemoveFocusIndicator(previousElement);
            }
            
            if (element != null)
            {
                AddFocusIndicator(element);
                
                // Announce focus change to screen reader
                if (_enableScreenReaderSupport)
                {
                    AnnounceFocusChange(element);
                }
                
                // Play focus sound
                if (_enableAudioCues && _focusSound != null)
                {
                    PlayAccessibilitySound(_focusSound);
                }
            }
            
            OnFocusChanged?.Invoke(element);
        }
        
        /// <summary>
        /// Handle tab navigation
        /// </summary>
        private void HandleTabNavigation(bool reverse)
        {
            if (_focusableElements.Count == 0)
                return;
            
            var currentIndex = _currentFocusedElement != null ? 
                _focusableElements.IndexOf(_currentFocusedElement) : -1;
            
            int nextIndex;
            if (reverse)
            {
                nextIndex = currentIndex <= 0 ? _focusableElements.Count - 1 : currentIndex - 1;
            }
            // else
            // {
                nextIndex = currentIndex >= _focusableElements.Count - 1 ? 0 : currentIndex + 1;
            // }
            
            var nextElement = _focusableElements[nextIndex];
            nextElement.Focus();
        }
        
        /// <summary>
        /// Handle arrow key navigation
        /// </summary>
        private void HandleArrowNavigation(KeyCode keyCode)
        {
            if (_currentFocusedElement == null)
                return;
            
            VisualElement targetElement = null;
            
            switch (keyCode)
            {
                case KeyCode.UpArrow:
                    targetElement = FindElementInDirection(_currentFocusedElement, Vector2.up);
                    break;
                case KeyCode.DownArrow:
                    targetElement = FindElementInDirection(_currentFocusedElement, Vector2.down);
                    break;
                case KeyCode.LeftArrow:
                    targetElement = FindElementInDirection(_currentFocusedElement, Vector2.left);
                    break;
                case KeyCode.RightArrow:
                    targetElement = FindElementInDirection(_currentFocusedElement, Vector2.right);
                    break;
            }
            
            if (targetElement != null)
            {
                targetElement.Focus();
            }
        }
        
        /// <summary>
        /// Find element in specified direction
        /// </summary>
        private VisualElement FindElementInDirection(VisualElement fromElement, Vector2 direction)
        {
            var fromBounds = fromElement.worldBound;
            var fromCenter = fromBounds.center;
            
            VisualElement bestElement = null;
            float bestDistance = float.MaxValue;
            
            foreach (var element in _focusableElements)
            {
                if (element == fromElement || !element.enabledInHierarchy)
                    continue;
                
                var elementBounds = element.worldBound;
                var elementCenter = elementBounds.center;
                var toVector = elementCenter - fromCenter;
                
                // Check if element is in the right direction
                var dot = Vector2.Dot(direction.normalized, toVector.normalized);
                if (dot < 0.5f) // Must be at least 60 degrees in the right direction
                    continue;
                
                var distance = toVector.magnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestElement = element;
                }
            }
            
            return bestElement;
        }
        
        /// <summary>
        /// Handle element activation
        /// </summary>
        private void HandleElementActivation(VisualElement element)
        {
            if (element == null)
                return;
            
            // Trigger click event
            using (var clickEvent = ClickEvent.GetPooled())
            {
                clickEvent.target = element;
                element.SendEvent(clickEvent);
            }
            
            // Play selection sound
            if (_enableAudioCues && _selectSound != null)
            {
                PlayAccessibilitySound(_selectSound);
            }
            
            // Announce activation
            if (_enableScreenReaderSupport)
            {
                var info = _accessibilityInfo.TryGetValue(element, out var accessInfo) ? accessInfo : null;
                var announcement = $"{info?.Label ?? element.name} activated";
                AnnounceToScreenReader(announcement);
            }
        }
        
        /// <summary>
        /// Handle escape key
        /// </summary>
        private void HandleEscapeKey(VisualElement element)
        {
            // Close modal or return to parent
            var modal = FindParentModal(element);
            if (modal != null)
            {
                // Close modal
                modal.style.display = DisplayStyle.None;
                return;
            }
            
            // Move focus to parent or main menu
            var parent = element.parent;
            while (parent != null)
            {
                if (_focusableElements.Contains(parent))
                {
                    parent.Focus();
                    return;
                }
                parent = parent.parent;
            }
        }
        
        /// <summary>
        /// Find parent modal element
        /// </summary>
        private VisualElement FindParentModal(VisualElement element)
        {
            var parent = element.parent;
            while (parent != null)
            {
                if (parent.ClassListContains("modal") || parent.ClassListContains("modal-container"))
                {
                    return parent;
                }
                parent = parent.parent;
            }
            return null;
        }
        
        /// <summary>
        /// Announce to screen reader
        /// </summary>
        public void AnnounceToScreenReader(string message, UIAnnouncementPriority priority = UIAnnouncementPriority.Normal)
        {
            if (!_enableScreenReaderSupport || string.IsNullOrEmpty(message))
                return;
            
            // Limit message length
            if (message.Length > _maxAnnouncementLength)
            {
                message = message.Substring(0, _maxAnnouncementLength - 3) + "...";
            }
            
            var announcement = new UIAnnouncement
            {
                Message = message,
                Priority = priority,
                Timestamp = Time.time
            };
            
            _announcementQueue.Enqueue(announcement);
            
            OnScreenReaderAnnouncement?.Invoke(message);
        }
        
        /// <summary>
        /// Announce focus change
        /// </summary>
        private void AnnounceFocusChange(VisualElement element)
        {
            if (!_accessibilityInfo.TryGetValue(element, out var info))
                return;
            
            var message = $"{info.Role} {info.Label}";
            if (!string.IsNullOrEmpty(info.Description))
            {
                message += $", {info.Description}";
            }
            
            AnnounceToScreenReader(message, UIAnnouncementPriority.High);
        }
        
        /// <summary>
        /// Process screen reader announcements
        /// </summary>
        private void ProcessScreenReaderAnnouncements()
        {
            if (_announcementQueue.Count == 0)
                return;
            
            var currentTime = Time.time;
            if (currentTime - _lastAnnouncementTime < _announceDelay)
                return;
            
            var announcement = _announcementQueue.Dequeue();
            
            if (_screenReader != null)
            {
                _screenReader.Announce(announcement.Message, announcement.Priority);
            }
            
            _lastAnnouncementTime = currentTime;
            
            LogInfo($"Screen reader announcement: {announcement.Message}");
        }
        
        /// <summary>
        /// Apply accessibility theme
        /// </summary>
        private void ApplyAccessibilityTheme()
        {
            if (_accessibilityTheme == null)
                return;
            
            // Apply theme settings
            _textScaleMultiplier = _accessibilityTheme.TextScaleMultiplier;
            _enableHighContrastMode = _accessibilityTheme.EnableHighContrast;
            _enableColorBlindSupport = _accessibilityTheme.EnableColorBlindSupport;
            _colorBlindnessType = _accessibilityTheme.ColorBlindnessType;
            
            ApplyTextScaling(_textScaleMultiplier);
            
            if (_enableHighContrastMode)
            {
                EnableHighContrastMode(true);
            }
            
            if (_enableColorBlindSupport)
            {
                ApplyColorBlindnessFilter(_colorBlindnessType);
            }
            
            LogInfo("Applied accessibility theme");
        }
        
        /// <summary>
        /// Apply text scaling
        /// </summary>
        public void ApplyTextScaling(float multiplier)
        {
            _textScaleMultiplier = Mathf.Clamp(multiplier, 0.5f, 3f);
            
            foreach (var element in _accessibilityInfo.Keys)
            {
                ApplyTextScalingToElement(element);
            }
        }
        
        /// <summary>
        /// Apply text scaling to specific element
        /// </summary>
        private void ApplyTextScalingToElement(VisualElement element)
        {
            if (element is Label || element is TextField || element is Button)
            {
                var currentSize = element.resolvedStyle.fontSize;
                var newSize = currentSize * _textScaleMultiplier;
                element.style.fontSize = newSize;
                
                _textScaling[element] = new UITextScaling
                {
                    OriginalSize = currentSize,
                    ScaledSize = newSize,
                    ScaleMultiplier = _textScaleMultiplier
                };
            }
        }
        
        /// <summary>
        /// Enable/disable high contrast mode
        /// </summary>
        public void EnableHighContrastMode(bool enable)
        {
            _enableHighContrastMode = enable;
            
            foreach (var element in _accessibilityInfo.Keys)
            {
                ApplyHighContrastToElement(element, enable);
            }
            
            OnHighContrastModeChanged?.Invoke(enable);
            
            LogInfo($"High contrast mode {(enable ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Apply high contrast to element
        /// </summary>
        private void ApplyHighContrastToElement(VisualElement element, bool enable)
        {
            if (enable)
            {
                element.AddToClassList("high-contrast");
                element.style.borderTopColor = Color.white;
                element.style.borderRightColor = Color.white;
                element.style.borderBottomColor = Color.white;
                element.style.borderLeftColor = Color.white;
                
                if (element is Label || element is TextField || element is Button)
                {
                    element.style.color = Color.white;
                }
            }
            // else
            // {
                element.RemoveFromClassList("high-contrast");
                // Reset to original colors
            // }
        }
        
        /// <summary>
        /// Apply color blindness filter
        /// </summary>
        public void ApplyColorBlindnessFilter(ColorBlindnessType type)
        {
            _colorBlindnessType = type;
            
            foreach (var element in _accessibilityInfo.Keys)
            {
                ApplyColorAdjustmentToElement(element);
            }
            
            LogInfo($"Applied color blindness filter: {type}");
        }
        
        /// <summary>
        /// Apply color adjustment to element
        /// </summary>
        private void ApplyColorAdjustmentToElement(VisualElement element)
        {
            if (_colorBlindnessType == ColorBlindnessType.None)
                return;
            
            // Get current colors
            var textColor = element.resolvedStyle.color;
            var backgroundColor = element.resolvedStyle.backgroundColor;
            
            // Apply color blindness simulation/correction
            var adjustedTextColor = AdjustColorForColorBlindness(textColor, _colorBlindnessType);
            var adjustedBackgroundColor = AdjustColorForColorBlindness(backgroundColor, _colorBlindnessType);
            
            // Apply adjusted colors
            element.style.color = adjustedTextColor;
            element.style.backgroundColor = adjustedBackgroundColor;
            
            _colorAdjustments[element] = new UIColorAdjustment
            {
                OriginalTextColor = textColor,
                OriginalBackgroundColor = backgroundColor,
                AdjustedTextColor = adjustedTextColor,
                AdjustedBackgroundColor = adjustedBackgroundColor,
                ColorBlindnessType = _colorBlindnessType
            };
        }
        
        /// <summary>
        /// Adjust color for color blindness
        /// </summary>
        private Color AdjustColorForColorBlindness(Color originalColor, ColorBlindnessType type)
        {
            switch (type)
            {
                case ColorBlindnessType.Protanopia:
                    // Remove red component
                    return new Color(0, originalColor.g, originalColor.b, originalColor.a);
                    
                case ColorBlindnessType.Deuteranopia:
                    // Adjust green component
                    return new Color(originalColor.r, originalColor.g * 0.5f, originalColor.b, originalColor.a);
                    
                case ColorBlindnessType.Tritanopia:
                    // Adjust blue component
                    return new Color(originalColor.r, originalColor.g, originalColor.b * 0.5f, originalColor.a);
                    
                case ColorBlindnessType.Monochromacy:
                    // Convert to grayscale
                    var gray = originalColor.r * 0.299f + originalColor.g * 0.587f + originalColor.b * 0.114f;
                    return new Color(gray, gray, gray, originalColor.a);
                    
                default:
                    return originalColor;
            }
        }
        
        /// <summary>
        /// Setup focus indicator for element
        /// </summary>
        private void SetupFocusIndicator(VisualElement element)
        {
            if (!_enableFocusIndicators)
                return;
            
            element.AddToClassList("focusable");
        }
        
        /// <summary>
        /// Add focus indicator to element
        /// </summary>
        private void AddFocusIndicator(VisualElement element)
        {
            if (!_enableFocusIndicators)
                return;
            
            element.AddToClassList("focused");
            element.style.borderTopColor = Color.yellow;
            element.style.borderRightColor = Color.yellow;
            element.style.borderBottomColor = Color.yellow;
            element.style.borderLeftColor = Color.yellow;
            element.style.borderTopWidth = 3f;
            element.style.borderRightWidth = 3f;
            element.style.borderBottomWidth = 3f;
            element.style.borderLeftWidth = 3f;
        }
        
        /// <summary>
        /// Remove focus indicator from element
        /// </summary>
        private void RemoveFocusIndicator(VisualElement element)
        {
            if (!_enableFocusIndicators)
                return;
            
            element.RemoveFromClassList("focused");
            // Reset border to original
        }
        
        /// <summary>
        /// Play accessibility sound
        /// </summary>
        private void PlayAccessibilitySound(AudioClip clip)
        {
            if (!_enableAudioCues || _audioSource == null || clip == null)
                return;
            
            _audioSource.PlayOneShot(clip, _audioVolume);
        }
        
        /// <summary>
        /// Sort focusable elements by tab index and position
        /// </summary>
        private void SortFocusableElements()
        {
            _focusableElements.Sort((a, b) =>
            {
                var aInfo = _accessibilityInfo.TryGetValue(a, out var infoA) ? infoA : null;
                var bInfo = _accessibilityInfo.TryGetValue(b, out var infoB) ? infoB : null;
                
                var aTabIndex = aInfo?.TabIndex ?? 0;
                var bTabIndex = bInfo?.TabIndex ?? 0;
                
                if (aTabIndex != bTabIndex)
                    return aTabIndex.CompareTo(bTabIndex);
                
                // Sort by position (top to bottom, left to right)
                var aBounds = a.worldBound;
                var bBounds = b.worldBound;
                
                if (Mathf.Abs(aBounds.y - bBounds.y) > 10f)
                    return aBounds.y.CompareTo(bBounds.y);
                
                return aBounds.x.CompareTo(bBounds.x);
            });
        }
        
        /// <summary>
        /// Handle navigation request from keyboard navigator
        /// </summary>
        private void HandleNavigationRequest(UINavigationDirection direction)
        {
            switch (direction)
            {
                case UINavigationDirection.Next:
                    HandleTabNavigation(false);
                    break;
                case UINavigationDirection.Previous:
                    HandleTabNavigation(true);
                    break;
                case UINavigationDirection.Up:
                    HandleArrowNavigation(KeyCode.UpArrow);
                    break;
                case UINavigationDirection.Down:
                    HandleArrowNavigation(KeyCode.DownArrow);
                    break;
                case UINavigationDirection.Left:
                    HandleArrowNavigation(KeyCode.LeftArrow);
                    break;
                case UINavigationDirection.Right:
                    HandleArrowNavigation(KeyCode.RightArrow);
                    break;
            }
        }
        
        /// <summary>
        /// Handle activation request from keyboard navigator
        /// </summary>
        private void HandleActivationRequest()
        {
            if (_currentFocusedElement != null)
            {
                HandleElementActivation(_currentFocusedElement);
            }
        }
        
        /// <summary>
        /// Handle announcement request from screen reader
        /// </summary>
        private void HandleAnnouncementRequest(string message, UIAnnouncementPriority priority)
        {
            AnnounceToScreenReader(message, priority);
        }
        
        /// <summary>
        /// Move focus to next available element
        /// </summary>
        public void MoveFocusToNextElement()
        {
            if (_focusableElements.Count > 0)
            {
                var nextElement = _focusableElements.FirstOrDefault(e => e.enabledInHierarchy);
                nextElement?.Focus();
            }
        }
        
        /// <summary>
        /// Get accessibility statistics
        /// </summary>
        public UIAccessibilityStats GetAccessibilityStats()
        {
            return new UIAccessibilityStats
            {
                RegisteredElements = _accessibilityInfo.Count,
                FocusableElements = _focusableElements.Count,
                ScreenReaderEnabled = _enableScreenReaderSupport,
                KeyboardNavigationEnabled = _enableKeyboardNavigation,
                HighContrastEnabled = _enableHighContrastMode,
                ColorBlindSupportEnabled = _enableColorBlindSupport,
                TextScaleMultiplier = _textScaleMultiplier,
                PendingAnnouncements = _announcementQueue.Count
            };
        }
        
        /// <summary>
        /// Dispose of accessibility manager resources
        /// </summary>
        public void Dispose()
        {
            // Clear event registrations
            OnScreenReaderAnnouncement = null;
            OnFocusChanged = null;
            OnAccessibilityEvent = null;
            OnHighContrastModeChanged = null;
            
            // Clean up collections
            _accessibilityInfo?.Clear();
            _focusableElements?.Clear();
            _colorAdjustments?.Clear();
            _textScaling?.Clear();
            _liveRegions?.Clear();
            _announcementQueue?.Clear();
            
            // Clean up components
            _screenReader = null;
            _keyboardNavigator = null;
            _currentFocusedElement = null;
            
            LogInfo("UIAccessibilityManager disposed");
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (_enableScreenReaderSupport)
            {
                ProcessScreenReaderAnnouncements();
            }
        }
        
        protected void OnValidate()
        {
            _announceDelay = Mathf.Max(0.1f, _announceDelay);
            _maxAnnouncementLength = Mathf.Max(50, _maxAnnouncementLength);
            _navigationRepeatDelay = Mathf.Max(0.1f, _navigationRepeatDelay);
            _textScaleMultiplier = Mathf.Clamp(_textScaleMultiplier, 0.5f, 3f);
            _audioVolume = Mathf.Clamp01(_audioVolume);
        }
    }
}