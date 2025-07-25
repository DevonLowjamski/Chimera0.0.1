using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Individual tutorial hint panel for Project Chimera.
    /// Represents a single hint display with interaction and animation.
    /// </summary>
    public class TutorialHintPanel
    {
        private VisualElement _panelElement;
        private Label _hintTextLabel;
        private Button _closeButton;
        
        // State
        private TutorialHint _currentHint;
        private TutorialStepSO _associatedStep;
        private bool _isActive;
        private float _displayStartTime;
        private float _autoDismissTime;
        private bool _hasAutoDismiss;
        
        // Animation
        private bool _isAnimating;
        private float _animationStartTime;
        private float _animationDuration = 0.3f;
        
        // Events
        public System.Action<TutorialHintPanel> OnDismissed;
        public System.Action<string> OnClicked;
        
        // Properties
        public bool IsActive => _isActive;
        public float DisplayTime => Time.time - _displayStartTime;
        public TutorialHint CurrentHint => _currentHint;
        public TutorialStepSO AssociatedStep => _associatedStep;
        
        public TutorialHintPanel(VisualElement panelElement)
        {
            _panelElement = panelElement;
            
            InitializeHintPanel();
        }
        
        /// <summary>
        /// Initialize hint panel
        /// </summary>
        private void InitializeHintPanel()
        {
            if (_panelElement == null)
            {
                Debug.LogError("Panel element is null");
                return;
            }
            
            FindUIElements();
            SetupEventHandlers();
            
            // Initially hide panel
            _panelElement.style.display = DisplayStyle.None;
            
            Debug.Log("Tutorial hint panel initialized");
        }
        
        /// <summary>
        /// Find UI elements
        /// </summary>
        private void FindUIElements()
        {
            _hintTextLabel = _panelElement.Q<Label>("hint-text");
            _closeButton = _panelElement.Q<Button>("hint-close");
            
            // Create elements if they don't exist
            if (_hintTextLabel == null)
            {
                _hintTextLabel = new Label();
                _hintTextLabel.name = "hint-text";
                _hintTextLabel.AddToClassList("tutorial-hint-text");
                _panelElement.Insert(0, _hintTextLabel);
            }
            
            if (_closeButton == null)
            {
                _closeButton = new Button();
                _closeButton.name = "hint-close";
                _closeButton.text = "×";
                _closeButton.AddToClassList("tutorial-hint-close");
                _panelElement.Add(_closeButton);
            }
        }
        
        /// <summary>
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            _closeButton?.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
                Dismiss();
            });
            
            _panelElement?.RegisterCallback<ClickEvent>(evt => {
                if (evt.target == _panelElement)
                {
                    OnClicked?.Invoke(_currentHint?.HintText ?? "");
                }
            });
        }
        
        /// <summary>
        /// Show hint
        /// </summary>
        public void ShowHint(TutorialHint hint, TutorialStepSO step = null)
        {
            if (hint == null)
                return;
            
            _currentHint = hint;
            _associatedStep = step;
            _isActive = true;
            _displayStartTime = Time.time;
            _hasAutoDismiss = false;
            
            // Update content
            if (_hintTextLabel != null)
            {
                _hintTextLabel.text = hint.HintText;
            }
            
            // Apply hint type styling
            ApplyHintTypeStyling(hint.HintType);
            
            // Show panel with animation
            ShowWithAnimation();
            
            Debug.Log($"Showing hint: {hint.HintText}");
        }
        
        /// <summary>
        /// Apply hint type styling
        /// </summary>
        private void ApplyHintTypeStyling(TutorialHintType hintType)
        {
            // Reset classes
            _panelElement.RemoveFromClassList("hint-text");
            _panelElement.RemoveFromClassList("hint-arrow");
            _panelElement.RemoveFromClassList("hint-highlight");
            _panelElement.RemoveFromClassList("hint-popup");
            
            // Apply type-specific styling
            switch (hintType)
            {
                case TutorialHintType.Text:
                    _panelElement.AddToClassList("hint-text");
                    _panelElement.style.borderTopColor = new Color(0.8f, 0.6f, 0.2f, 1f);
                    _panelElement.style.borderRightColor = new Color(0.8f, 0.6f, 0.2f, 1f);
                    _panelElement.style.borderBottomColor = new Color(0.8f, 0.6f, 0.2f, 1f);
                    _panelElement.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f, 1f);
                    break;
                    
                case TutorialHintType.Arrow:
                    _panelElement.AddToClassList("hint-arrow");
                    _panelElement.style.borderTopColor = new Color(0.2f, 0.6f, 0.8f, 1f);
                    _panelElement.style.borderRightColor = new Color(0.2f, 0.6f, 0.8f, 1f);
                    _panelElement.style.borderBottomColor = new Color(0.2f, 0.6f, 0.8f, 1f);
                    _panelElement.style.borderLeftColor = new Color(0.2f, 0.6f, 0.8f, 1f);
                    break;
                    
                case TutorialHintType.Highlight:
                    _panelElement.AddToClassList("hint-highlight");
                    _panelElement.style.borderTopColor = new Color(0.8f, 0.2f, 0.6f, 1f);
                    _panelElement.style.borderRightColor = new Color(0.8f, 0.2f, 0.6f, 1f);
                    _panelElement.style.borderBottomColor = new Color(0.8f, 0.2f, 0.6f, 1f);
                    _panelElement.style.borderLeftColor = new Color(0.8f, 0.2f, 0.6f, 1f);
                    break;
                    
                case TutorialHintType.Popup:
                    _panelElement.AddToClassList("hint-popup");
                    _panelElement.style.borderTopColor = new Color(0.6f, 0.8f, 0.2f, 1f);
                    _panelElement.style.borderRightColor = new Color(0.6f, 0.8f, 0.2f, 1f);
                    _panelElement.style.borderBottomColor = new Color(0.6f, 0.8f, 0.2f, 1f);
                    _panelElement.style.borderLeftColor = new Color(0.6f, 0.8f, 0.2f, 1f);
                    break;
                    
                default:
                    // Default styling
                    break;
            }
        }
        
        /// <summary>
        /// Show panel with animation
        /// </summary>
        private void ShowWithAnimation()
        {
            _panelElement.style.display = DisplayStyle.Flex;
            
            // Set initial animation state
            _panelElement.style.opacity = 0f;
            _panelElement.style.scale = new Scale(Vector3.one * 0.8f);
            
            // Start animation
            _isAnimating = true;
            _animationStartTime = Time.time;
        }
        
        /// <summary>
        /// Hide panel with animation
        /// </summary>
        private void HideWithAnimation()
        {
            _isAnimating = true;
            _animationStartTime = Time.time;
        }
        
        /// <summary>
        /// Set panel position
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            if (_panelElement != null)
            {
                _panelElement.style.left = position.x;
                _panelElement.style.top = position.y;
            }
        }
        
        /// <summary>
        /// Setup auto-dismiss timer
        /// </summary>
        public void SetupAutoDismiss(float dismissTime)
        {
            _autoDismissTime = dismissTime;
            _hasAutoDismiss = true;
        }
        
        /// <summary>
        /// Dismiss hint
        /// </summary>
        public void Dismiss()
        {
            if (!_isActive)
                return;
            
            HideWithAnimation();
            _isActive = false;
            
            OnDismissed?.Invoke(this);
            
            Debug.Log($"Dismissed hint: {_currentHint?.HintText}");
        }
        
        /// <summary>
        /// Update hint panel
        /// </summary>
        public void UpdateHint()
        {
            if (!_isActive)
                return;
            
            // Update animation
            UpdateAnimation();
            
            // Check auto-dismiss
            if (_hasAutoDismiss && DisplayTime >= _autoDismissTime)
            {
                Dismiss();
            }
        }
        
        /// <summary>
        /// Update animation
        /// </summary>
        private void UpdateAnimation()
        {
            if (!_isAnimating)
                return;
            
            var elapsed = Time.time - _animationStartTime;
            var progress = Mathf.Clamp01(elapsed / _animationDuration);
            
            if (_isActive)
            {
                // Fade in animation
                _panelElement.style.opacity = progress;
                _panelElement.style.scale = new Scale(Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, progress));
                
                if (progress >= 1f)
                {
                    _isAnimating = false;
                }
            }
            else
            {
                // Fade out animation
                _panelElement.style.opacity = 1f - progress;
                _panelElement.style.scale = new Scale(Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, progress));
                
                if (progress >= 1f)
                {
                    _isAnimating = false;
                    _panelElement.style.display = DisplayStyle.None;
                }
            }
        }
        
        /// <summary>
        /// Set hint text
        /// </summary>
        public void SetHintText(string text)
        {
            if (_hintTextLabel != null)
            {
                _hintTextLabel.text = text;
            }
        }
        
        /// <summary>
        /// Set panel size
        /// </summary>
        public void SetSize(Vector2 size)
        {
            if (_panelElement != null)
            {
                _panelElement.style.width = size.x;
                _panelElement.style.height = size.y;
            }
        }
        
        /// <summary>
        /// Set panel style
        /// </summary>
        public void SetStyle(Color backgroundColor, Color borderColor, float borderWidth = 1f)
        {
            if (_panelElement != null)
            {
                _panelElement.style.backgroundColor = backgroundColor;
                _panelElement.style.borderTopColor = borderColor;
                _panelElement.style.borderRightColor = borderColor;
                _panelElement.style.borderBottomColor = borderColor;
                _panelElement.style.borderLeftColor = borderColor;
                _panelElement.style.borderTopWidth = borderWidth;
                _panelElement.style.borderRightWidth = borderWidth;
                _panelElement.style.borderBottomWidth = borderWidth;
                _panelElement.style.borderLeftWidth = borderWidth;
            }
        }
        
        /// <summary>
        /// Set text color
        /// </summary>
        public void SetTextColor(Color color)
        {
            if (_hintTextLabel != null)
            {
                _hintTextLabel.style.color = color;
            }
        }
        
        /// <summary>
        /// Enable/disable close button
        /// </summary>
        public void SetCloseButtonVisible(bool visible)
        {
            if (_closeButton != null)
            {
                _closeButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Check if point is inside panel
        /// </summary>
        public bool ContainsPoint(Vector2 point)
        {
            if (_panelElement == null)
                return false;
            
            var bounds = _panelElement.worldBound;
            return bounds.Contains(point);
        }
        
        /// <summary>
        /// Get panel info
        /// </summary>
        public TutorialHintPanelInfo GetPanelInfo()
        {
            return new TutorialHintPanelInfo
            {
                IsActive = _isActive,
                DisplayTime = DisplayTime,
                HintText = _currentHint?.HintText ?? "",
                HintType = _currentHint?.HintType ?? TutorialHintType.Text,
                Position = new Vector2(_panelElement.style.left.value.value, _panelElement.style.top.value.value),
                HasAutoDismiss = _hasAutoDismiss,
                AutoDismissTime = _autoDismissTime,
                AssociatedStepId = _associatedStep?.StepId ?? ""
            };
        }
        
        /// <summary>
        /// Cleanup hint panel
        /// </summary>
        public void Cleanup()
        {
            // Unregister event handlers
            _closeButton?.UnregisterCallback<ClickEvent>(evt => Dismiss());
            _panelElement?.UnregisterCallback<ClickEvent>(evt => OnClicked?.Invoke(_currentHint?.HintText ?? ""));
            
            // Hide panel
            if (_panelElement != null)
            {
                _panelElement.style.display = DisplayStyle.None;
            }
            
            _isActive = false;
            _currentHint = null;
            _associatedStep = null;
            
            Debug.Log("Tutorial hint panel cleaned up");
        }
    }
    
    /// <summary>
    /// Tutorial hint panel information
    /// </summary>
    public struct TutorialHintPanelInfo
    {
        public bool IsActive;
        public float DisplayTime;
        public string HintText;
        public TutorialHintType HintType;
        public Vector2 Position;
        public bool HasAutoDismiss;
        public float AutoDismissTime;
        public string AssociatedStepId;
    }
}