using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Tutorial overlay manager component for Project Chimera.
    /// Manages tutorial overlays, blocking interfaces, and focus areas.
    /// </summary>
    public class TutorialOverlayManager
    {
        private VisualElement _overlayContainer;
        private VisualElement _backgroundOverlay;
        private VisualElement _focusWindow;
        private List<VisualElement> _blockedElements;
        
        // State
        private bool _isInitialized;
        private bool _isOverlayActive;
        private TutorialStepSO _currentStep;
        private Vector2 _focusPosition;
        private Vector2 _focusSize;
        
        // Animation
        private float _fadeInDuration = 0.3f;
        private float _fadeOutDuration = 0.2f;
        private bool _isAnimating;
        private float _animationStartTime;
        private float _targetAlpha;
        private float _currentAlpha;
        
        // Focus area animation
        private Vector2 _targetFocusPosition;
        private Vector2 _targetFocusSize;
        private float _focusAnimationSpeed = 3f;
        
        // Events
        public System.Action OnOverlayShown;
        public System.Action OnOverlayHidden;
        public System.Action<Vector2> OnFocusAreaChanged;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsOverlayActive => _isOverlayActive;
        public TutorialStepSO CurrentStep => _currentStep;
        public Vector2 FocusPosition => _focusPosition;
        public Vector2 FocusSize => _focusSize;
        
        public TutorialOverlayManager(VisualElement overlayContainer)
        {
            _overlayContainer = overlayContainer;
            
            InitializeOverlayManager();
        }
        
        /// <summary>
        /// Initialize overlay manager
        /// </summary>
        private void InitializeOverlayManager()
        {
            if (_overlayContainer == null)
            {
                Debug.LogError("Overlay container is null");
                return;
            }
            
            _blockedElements = new List<VisualElement>();
            
            CreateOverlayElements();
            
            _isInitialized = true;
            Debug.Log("Tutorial overlay manager initialized");
        }
        
        /// <summary>
        /// Create overlay UI elements
        /// </summary>
        private void CreateOverlayElements()
        {
            // Create background overlay
            _backgroundOverlay = new VisualElement();
            _backgroundOverlay.name = "tutorial-background-overlay";
            _backgroundOverlay.AddToClassList("tutorial-background-overlay");
            _backgroundOverlay.style.position = Position.Absolute;
            _backgroundOverlay.style.left = 0;
            _backgroundOverlay.style.top = 0;
            _backgroundOverlay.style.right = 0;
            _backgroundOverlay.style.bottom = 0;
            _backgroundOverlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            _backgroundOverlay.style.display = DisplayStyle.None;
            
            // Create focus window (transparent area)
            _focusWindow = new VisualElement();
            _focusWindow.name = "tutorial-focus-window";
            _focusWindow.AddToClassList("tutorial-focus-window");
            _focusWindow.style.position = Position.Absolute;
            _focusWindow.style.backgroundColor = Color.clear;
            _focusWindow.style.borderTopWidth = 2f;
            _focusWindow.style.borderRightWidth = 2f;
            _focusWindow.style.borderBottomWidth = 2f;
            _focusWindow.style.borderLeftWidth = 2f;
            _focusWindow.style.borderTopColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
            _focusWindow.style.borderRightColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
            _focusWindow.style.borderBottomColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
            _focusWindow.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
            _focusWindow.style.borderTopLeftRadius = 4f;
            _focusWindow.style.borderTopRightRadius = 4f;
            _focusWindow.style.borderBottomLeftRadius = 4f;
            _focusWindow.style.borderBottomRightRadius = 4f;
            _focusWindow.style.display = DisplayStyle.None;
            
            _backgroundOverlay.Add(_focusWindow);
            _overlayContainer.Add(_backgroundOverlay);
        }
        
        /// <summary>
        /// Show overlay for tutorial step
        /// </summary>
        public void ShowOverlay(TutorialStepSO step)
        {
            if (!_isInitialized || step == null)
                return;
            
            _currentStep = step;
            _isOverlayActive = true;
            
            // Show background overlay
            _backgroundOverlay.style.display = DisplayStyle.Flex;
            
            // Setup focus area if specified
            if (step.HighlightSettings.HasHighlight)
            {
                SetupFocusArea(step);
            }
            else
            {
                _focusWindow.style.display = DisplayStyle.None;
            }
            
            // Block interactive elements
            BlockInteractiveElements(step);
            
            // Start fade in animation
            StartFadeAnimation(true);
            
            OnOverlayShown?.Invoke();
            
            Debug.Log($"Showed tutorial overlay for step: {step.StepId}");
        }
        
        /// <summary>
        /// Setup focus area
        /// </summary>
        private void SetupFocusArea(TutorialStepSO step)
        {
            if (step.HighlightSettings.HighlightType == TutorialHighlightType.None)
                return;
            
            // Try to find target element
            var targetBounds = GetTargetElementBounds(step.HighlightSettings.TargetElement);
            
            if (targetBounds.HasValue)
            {
                _targetFocusPosition = new Vector2(targetBounds.Value.x, targetBounds.Value.y);
                _targetFocusSize = new Vector2(targetBounds.Value.width, targetBounds.Value.height);
                
                // Add padding around focus area
                var padding = step.HighlightSettings.HighlightPadding;
                _targetFocusPosition -= Vector2.one * padding;
                _targetFocusSize += Vector2.one * padding * 2f;
            }
            else
            {
                // Default focus area
                _targetFocusPosition = new Vector2(100f, 100f);
                _targetFocusSize = new Vector2(200f, 100f);
            }
            
            // Initialize current position if first time
            if (_focusPosition == Vector2.zero)
            {
                _focusPosition = _targetFocusPosition;
                _focusSize = _targetFocusSize;
            }
            
            _focusWindow.style.display = DisplayStyle.Flex;
            UpdateFocusWindow();
        }
        
        /// <summary>
        /// Get target element bounds
        /// </summary>
        private Rect? GetTargetElementBounds(string elementId)
        {
            if (string.IsNullOrEmpty(elementId))
                return null;
            
            // In a full implementation, this would search for the element
            // For now, return placeholder bounds
            return new Rect(150f, 120f, 180f, 80f);
        }
        
        /// <summary>
        /// Block interactive elements
        /// </summary>
        private void BlockInteractiveElements(TutorialStepSO step)
        {
            _blockedElements.Clear();
            
            // Find elements to block based on step configuration
            if (step.BlockInteraction)
            {
                // Block all interactive elements except tutorial UI
                BlockAllInteractiveElements();
            }
            else if (step.AllowedInteractionElements != null && step.AllowedInteractionElements.Count > 0)
            {
                // Block all except allowed elements
                BlockElementsExcept(step.AllowedInteractionElements);
            }
        }
        
        /// <summary>
        /// Block all interactive elements
        /// </summary>
        private void BlockAllInteractiveElements()
        {
            // In a full implementation, this would find all interactive elements
            // and disable their pointer events
            Debug.Log("Blocking all interactive elements for tutorial");
        }
        
        /// <summary>
        /// Block elements except allowed ones
        /// </summary>
        private void BlockElementsExcept(List<string> allowedElements)
        {
            // In a full implementation, this would selectively block elements
            Debug.Log($"Blocking elements except: {string.Join(", ", allowedElements)}");
        }
        
        /// <summary>
        /// Hide overlay
        /// </summary>
        public void HideOverlay()
        {
            if (!_isInitialized || !_isOverlayActive)
                return;
            
            // Start fade out animation
            StartFadeAnimation(false);
            
            // Unblock elements
            UnblockInteractiveElements();
            
            _isOverlayActive = false;
            _currentStep = null;
            
            OnOverlayHidden?.Invoke();
            
            Debug.Log("Hid tutorial overlay");
        }
        
        /// <summary>
        /// Clear all highlights and hide overlay
        /// </summary>
        public void ClearHighlights()
        {
            HideOverlay();
        }
        
        /// <summary>
        /// Start fade animation
        /// </summary>
        private void StartFadeAnimation(bool fadeIn)
        {
            _isAnimating = true;
            _animationStartTime = Time.time;
            _targetAlpha = fadeIn ? 1f : 0f;
            _currentAlpha = fadeIn ? 0f : 1f;
        }
        
        /// <summary>
        /// Unblock interactive elements
        /// </summary>
        private void UnblockInteractiveElements()
        {
            foreach (var element in _blockedElements)
            {
                if (element != null)
                {
                    element.pickingMode = PickingMode.Position;
                }
            }
            
            _blockedElements.Clear();
            Debug.Log("Unblocked interactive elements");
        }
        
        /// <summary>
        /// Update focus area position
        /// </summary>
        public void UpdateFocusArea(Vector2 position, Vector2 size)
        {
            _targetFocusPosition = position;
            _targetFocusSize = size;
            
            OnFocusAreaChanged?.Invoke(position);
        }
        
        /// <summary>
        /// Update focus window
        /// </summary>
        private void UpdateFocusWindow()
        {
            if (_focusWindow == null)
                return;
            
            _focusWindow.style.left = _focusPosition.x;
            _focusWindow.style.top = _focusPosition.y;
            _focusWindow.style.width = _focusSize.x;
            _focusWindow.style.height = _focusSize.y;
        }
        
        /// <summary>
        /// Update overlay animations
        /// </summary>
        public void UpdateOverlay()
        {
            if (!_isInitialized)
                return;
            
            // Update fade animation
            UpdateFadeAnimation();
            
            // Update focus area animation
            UpdateFocusAreaAnimation();
        }
        
        /// <summary>
        /// Update highlights - compatibility method for tutorial system
        /// </summary>
        public void UpdateHighlights()
        {
            UpdateOverlay();
        }
        
        /// <summary>
        /// Update fade animation
        /// </summary>
        private void UpdateFadeAnimation()
        {
            if (!_isAnimating)
                return;
            
            var elapsed = Time.time - _animationStartTime;
            var duration = _targetAlpha > 0.5f ? _fadeInDuration : _fadeOutDuration;
            var progress = Mathf.Clamp01(elapsed / duration);
            
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, progress);
            
            // Update overlay opacity
            var color = _backgroundOverlay.style.backgroundColor.value;
            color.a = _currentAlpha * 0.7f; // Base alpha is 0.7
            _backgroundOverlay.style.backgroundColor = color;
            
            // Check if animation is complete
            if (progress >= 1f)
            {
                _isAnimating = false;
                
                if (_targetAlpha <= 0f)
                {
                    _backgroundOverlay.style.display = DisplayStyle.None;
                    _focusWindow.style.display = DisplayStyle.None;
                }
            }
        }
        
        /// <summary>
        /// Update focus area animation
        /// </summary>
        private void UpdateFocusAreaAnimation()
        {
            if (!_isOverlayActive || _focusWindow.style.display == DisplayStyle.None)
                return;
            
            // Animate focus position
            if (Vector2.Distance(_focusPosition, _targetFocusPosition) > 1f)
            {
                _focusPosition = Vector2.MoveTowards(_focusPosition, _targetFocusPosition, 
                    Time.deltaTime * _focusAnimationSpeed * 100f);
            }
            
            // Animate focus size
            if (Vector2.Distance(_focusSize, _targetFocusSize) > 1f)
            {
                _focusSize = Vector2.MoveTowards(_focusSize, _targetFocusSize, 
                    Time.deltaTime * _focusAnimationSpeed * 50f);
            }
            
            UpdateFocusWindow();
        }
        
        /// <summary>
        /// Set overlay opacity
        /// </summary>
        public void SetOverlayOpacity(float opacity)
        {
            if (_backgroundOverlay != null)
            {
                var color = _backgroundOverlay.style.backgroundColor.value;
                color.a = Mathf.Clamp01(opacity);
                _backgroundOverlay.style.backgroundColor = color;
            }
        }
        
        /// <summary>
        /// Set focus border color
        /// </summary>
        public void SetFocusBorderColor(Color color)
        {
            if (_focusWindow != null)
            {
                _focusWindow.style.borderTopColor = color;
                _focusWindow.style.borderRightColor = color;
                _focusWindow.style.borderBottomColor = color;
                _focusWindow.style.borderLeftColor = color;
            }
        }
        
        /// <summary>
        /// Set animation speeds
        /// </summary>
        public void SetAnimationSpeeds(float fadeSpeed, float focusSpeed)
        {
            _fadeInDuration = Mathf.Max(0.1f, 1f / fadeSpeed);
            _fadeOutDuration = Mathf.Max(0.1f, 0.5f / fadeSpeed);
            _focusAnimationSpeed = Mathf.Max(0.1f, focusSpeed);
        }
        
        /// <summary>
        /// Check if point is in focus area
        /// </summary>
        public bool IsPointInFocusArea(Vector2 point)
        {
            if (!_isOverlayActive || _focusWindow.style.display == DisplayStyle.None)
                return true;
            
            var focusRect = new Rect(_focusPosition, _focusSize);
            return focusRect.Contains(point);
        }
        
        /// <summary>
        /// Get overlay info
        /// </summary>
        public TutorialOverlayInfo GetOverlayInfo()
        {
            return new TutorialOverlayInfo
            {
                IsActive = _isOverlayActive,
                CurrentStepId = _currentStep?.StepId ?? "",
                FocusPosition = _focusPosition,
                FocusSize = _focusSize,
                CurrentAlpha = _currentAlpha,
                IsAnimating = _isAnimating,
                BlockedElementCount = _blockedElements.Count
            };
        }
        
        /// <summary>
        /// Cleanup overlay manager
        /// </summary>
        public void Cleanup()
        {
            HideOverlay();
            UnblockInteractiveElements();
            
            _currentStep = null;
            _isInitialized = false;
            
            Debug.Log("Tutorial overlay manager cleaned up");
        }
    }
    
    /// <summary>
    /// Tutorial overlay information
    /// </summary>
    public struct TutorialOverlayInfo
    {
        public bool IsActive;
        public string CurrentStepId;
        public Vector2 FocusPosition;
        public Vector2 FocusSize;
        public float CurrentAlpha;
        public bool IsAnimating;
        public int BlockedElementCount;
    }
}