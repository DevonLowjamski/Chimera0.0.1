using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Tutorial hint system component for Project Chimera.
    /// Manages contextual hints and help tooltips during tutorials.
    /// </summary>
    public class TutorialHintSystem
    {
        private VisualElement _hintContainer;
        private VisualTreeAsset _hintPanelTemplate;
        private Dictionary<string, TutorialHintPanel> _activeHints;
        private List<TutorialHintPanel> _hintPool;
        
        // State
        private bool _isInitialized;
        private float _hintDisplayDuration = 5f;
        private int _maxActiveHints = 3;
        
        // Animation
        private float _fadeInDuration = 0.3f;
        private float _fadeOutDuration = 0.2f;
        
        // Events
        public System.Action OnHintDismissed;
        public System.Action<string> OnHintClicked;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public int ActiveHintCount => _activeHints?.Count ?? 0;
        public bool HasActiveHints => ActiveHintCount > 0;
        
        public TutorialHintSystem(VisualElement hintContainer, VisualTreeAsset hintPanelTemplate = null)
        {
            _hintContainer = hintContainer;
            _hintPanelTemplate = hintPanelTemplate;
            
            InitializeHintSystem();
        }
        
        /// <summary>
        /// Initialize hint system
        /// </summary>
        private void InitializeHintSystem()
        {
            if (_hintContainer == null)
            {
                Debug.LogError("Hint container is null");
                return;
            }
            
            _activeHints = new Dictionary<string, TutorialHintPanel>();
            _hintPool = new List<TutorialHintPanel>();
            
            CreateHintContainer();
            
            _isInitialized = true;
            Debug.Log("Tutorial hint system initialized");
        }
        
        /// <summary>
        /// Create hint container
        /// </summary>
        private void CreateHintContainer()
        {
            // Create hint overlay if it doesn't exist
            var hintOverlay = _hintContainer.Q("hint-overlay");
            if (hintOverlay == null)
            {
                hintOverlay = new VisualElement();
                hintOverlay.name = "hint-overlay";
                hintOverlay.AddToClassList("tutorial-hint-overlay");
                hintOverlay.style.position = Position.Absolute;
                hintOverlay.style.left = 0;
                hintOverlay.style.top = 0;
                hintOverlay.style.right = 0;
                hintOverlay.style.bottom = 0;
                hintOverlay.pickingMode = PickingMode.Ignore; // Allow clicks through
                
                _hintContainer.Add(hintOverlay);
            }
        }
        
        /// <summary>
        /// Show hint
        /// </summary>
        public void ShowHint(TutorialHint hint, TutorialStepSO step = null)
        {
            if (!_isInitialized || hint == null)
                return;
            
            // Check if we already have this hint
            var hintId = GenerateHintId(hint, step);
            if (_activeHints.ContainsKey(hintId))
                return;
            
            // Remove oldest hint if we're at max capacity
            if (_activeHints.Count >= _maxActiveHints)
            {
                RemoveOldestHint();
            }
            
            // Create hint panel
            var hintPanel = GetOrCreateHintPanel();
            hintPanel.ShowHint(hint, step);
            
            // Position hint panel
            PositionHintPanel(hintPanel, hint, step);
            
            // Add to active hints
            _activeHints[hintId] = hintPanel;
            
            // Setup auto-dismiss timer
            if (hint.DelayBeforeShow > 0)
            {
                hintPanel.SetupAutoDismiss(hint.DelayBeforeShow + _hintDisplayDuration);
            }
            else
            {
                hintPanel.SetupAutoDismiss(_hintDisplayDuration);
            }
            
            Debug.Log($"Showed tutorial hint: {hint.HintText}");
        }
        
        /// <summary>
        /// Show contextual hint at position
        /// </summary>
        public void ShowHintAtPosition(string hintText, Vector2 position, TutorialHintType hintType = TutorialHintType.Text)
        {
            var hint = new TutorialHint
            {
                HintText = hintText,
                HintType = hintType,
                DelayBeforeShow = 0f,
                IsContextual = true
            };
            
            var hintPanel = GetOrCreateHintPanel();
            hintPanel.ShowHint(hint);
            hintPanel.SetPosition(position);
            
            var hintId = $"contextual_{position.x}_{position.y}_{Time.time}";
            _activeHints[hintId] = hintPanel;
            
            hintPanel.SetupAutoDismiss(_hintDisplayDuration);
        }
        
        /// <summary>
        /// Get or create hint panel
        /// </summary>
        private TutorialHintPanel GetOrCreateHintPanel()
        {
            // Try to get from pool
            TutorialHintPanel hintPanel = null;
            
            for (int i = _hintPool.Count - 1; i >= 0; i--)
            {
                if (!_hintPool[i].IsActive)
                {
                    hintPanel = _hintPool[i];
                    _hintPool.RemoveAt(i);
                    break;
                }
            }
            
            // Create new if none available
            if (hintPanel == null)
            {
                hintPanel = CreateHintPanel();
            }
            
            return hintPanel;
        }
        
        /// <summary>
        /// Create hint panel
        /// </summary>
        private TutorialHintPanel CreateHintPanel()
        {
            VisualElement panelElement;
            
            if (_hintPanelTemplate != null)
            {
                panelElement = _hintPanelTemplate.Instantiate();
            }
            else
            {
                panelElement = CreateDefaultHintPanel();
            }
            
            var hintOverlay = _hintContainer.Q("hint-overlay");
            hintOverlay.Add(panelElement);
            
            var hintPanel = new TutorialHintPanel(panelElement);
            hintPanel.OnDismissed += HandleHintDismissed;
            hintPanel.OnClicked += HandleHintClicked;
            
            return hintPanel;
        }
        
        /// <summary>
        /// Create default hint panel
        /// </summary>
        private VisualElement CreateDefaultHintPanel()
        {
            var panel = new VisualElement();
            panel.name = "tutorial-hint-panel";
            panel.AddToClassList("tutorial-hint-panel");
            
            // Styling
            panel.style.position = Position.Absolute;
            panel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            panel.style.borderTopWidth = 1f;
            panel.style.borderRightWidth = 1f;
            panel.style.borderBottomWidth = 1f;
            panel.style.borderLeftWidth = 1f;
            panel.style.borderTopColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            panel.style.borderRightColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            panel.style.borderBottomColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            panel.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            panel.style.borderTopLeftRadius = 6f;
            panel.style.borderTopRightRadius = 6f;
            panel.style.borderBottomLeftRadius = 6f;
            panel.style.borderBottomRightRadius = 6f;
            panel.style.paddingLeft = 10f;
            panel.style.paddingRight = 10f;
            panel.style.paddingTop = 10f;
            panel.style.paddingBottom = 10f;
            panel.style.maxWidth = 250f;
            panel.style.minWidth = 100f;
            
            // Create hint text
            var hintLabel = new Label();
            hintLabel.name = "hint-text";
            hintLabel.AddToClassList("tutorial-hint-text");
            hintLabel.style.fontSize = 11f;
            hintLabel.style.color = Color.white;
            hintLabel.style.whiteSpace = WhiteSpace.Normal;
            hintLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            
            // Create close button
            var closeButton = new Button();
            closeButton.name = "hint-close";
            closeButton.text = "×";
            closeButton.AddToClassList("tutorial-hint-close");
            closeButton.style.position = Position.Absolute;
            closeButton.style.top = 2f;
            closeButton.style.right = 2f;
            closeButton.style.width = 16f;
            closeButton.style.height = 16f;
            closeButton.style.fontSize = 12f;
            closeButton.style.backgroundColor = Color.clear;
            closeButton.style.borderTopWidth = 0;
            closeButton.style.borderRightWidth = 0;
            closeButton.style.borderBottomWidth = 0;
            closeButton.style.borderLeftWidth = 0;
            closeButton.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            panel.Add(hintLabel);
            panel.Add(closeButton);
            
            return panel;
        }
        
        /// <summary>
        /// Position hint panel
        /// </summary>
        private void PositionHintPanel(TutorialHintPanel hintPanel, TutorialHint hint, TutorialStepSO step)
        {
            Vector2 position = Vector2.zero;
            
            if (hint.IsContextual)
            {
                // Use default positioning for contextual hints
                position = new Vector2(20f, 100f);
            }
            else if (step != null && !string.IsNullOrEmpty(hint.TargetElement))
            {
                // Try to position near target element
                var targetBounds = GetTargetElementBounds(hint.TargetElement);
                if (targetBounds.HasValue)
                {
                    position = new Vector2(
                        targetBounds.Value.xMax + 10f,
                        targetBounds.Value.y
                    );
                }
                else
                {
                    // Fallback positioning
                    position = GetDefaultHintPosition();
                }
            }
            else
            {
                // Default positioning
                position = GetDefaultHintPosition();
            }
            
            // Ensure hint stays on screen
            position = ClampToScreen(position, new Vector2(250f, 100f));
            
            hintPanel.SetPosition(position);
        }
        
        /// <summary>
        /// Get default hint position
        /// </summary>
        private Vector2 GetDefaultHintPosition()
        {
            // Position hints in a staggered pattern
            var hintIndex = _activeHints.Count;
            var offsetX = 20f + (hintIndex * 10f);
            var offsetY = 100f + (hintIndex * 30f);
            
            return new Vector2(offsetX, offsetY);
        }
        
        /// <summary>
        /// Clamp position to screen bounds
        /// </summary>
        private Vector2 ClampToScreen(Vector2 position, Vector2 panelSize)
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            
            position.x = Mathf.Clamp(position.x, 10f, screenWidth - panelSize.x - 10f);
            position.y = Mathf.Clamp(position.y, 10f, screenHeight - panelSize.y - 10f);
            
            return position;
        }
        
        /// <summary>
        /// Get target element bounds
        /// </summary>
        private Rect? GetTargetElementBounds(string elementId)
        {
            // In a full implementation, this would find the target element
            // For now, return null as placeholder
            return null;
        }
        
        /// <summary>
        /// Generate hint ID
        /// </summary>
        private string GenerateHintId(TutorialHint hint, TutorialStepSO step)
        {
            if (step != null)
            {
                return $"{step.StepId}_{hint.HintText.GetHashCode()}";
            }
            
            return $"hint_{hint.HintText.GetHashCode()}_{Time.time}";
        }
        
        /// <summary>
        /// Remove oldest hint
        /// </summary>
        private void RemoveOldestHint()
        {
            if (_activeHints.Count == 0)
                return;
            
            TutorialHintPanel oldestHint = null;
            string oldestHintId = null;
            float oldestTime = float.MaxValue;
            
            foreach (var kvp in _activeHints)
            {
                if (kvp.Value.DisplayTime < oldestTime)
                {
                    oldestTime = kvp.Value.DisplayTime;
                    oldestHint = kvp.Value;
                    oldestHintId = kvp.Key;
                }
            }
            
            if (oldestHint != null)
            {
                DismissHint(oldestHintId);
            }
        }
        
        /// <summary>
        /// Dismiss hint
        /// </summary>
        public void DismissHint(string hintId)
        {
            if (!_activeHints.TryGetValue(hintId, out var hintPanel))
                return;
            
            hintPanel.Dismiss();
            _activeHints.Remove(hintId);
            
            // Return to pool
            _hintPool.Add(hintPanel);
        }
        
        /// <summary>
        /// Clear all hints
        /// </summary>
        public void ClearHints()
        {
            if (!_isInitialized)
                return;
            
            var hintIds = new List<string>(_activeHints.Keys);
            
            foreach (var hintId in hintIds)
            {
                DismissHint(hintId);
            }
            
            Debug.Log("Cleared all tutorial hints");
        }
        
        /// <summary>
        /// Handle hint dismissed
        /// </summary>
        private void HandleHintDismissed(TutorialHintPanel hintPanel)
        {
            // Find and remove hint from active list
            string hintIdToRemove = null;
            foreach (var kvp in _activeHints)
            {
                if (kvp.Value == hintPanel)
                {
                    hintIdToRemove = kvp.Key;
                    break;
                }
            }
            
            if (hintIdToRemove != null)
            {
                _activeHints.Remove(hintIdToRemove);
                _hintPool.Add(hintPanel);
            }
            
            OnHintDismissed?.Invoke();
        }
        
        /// <summary>
        /// Handle hint clicked
        /// </summary>
        private void HandleHintClicked(string hintText)
        {
            OnHintClicked?.Invoke(hintText);
        }
        
        /// <summary>
        /// Update hint system
        /// </summary>
        public void UpdateHints()
        {
            if (!_isInitialized)
                return;
            
            // Update all active hints
            foreach (var hintPanel in _activeHints.Values)
            {
                hintPanel.UpdateHint();
            }
        }
        
        /// <summary>
        /// Set hint display duration
        /// </summary>
        public void SetHintDisplayDuration(float duration)
        {
            _hintDisplayDuration = Mathf.Max(1f, duration);
        }
        
        /// <summary>
        /// Set max active hints
        /// </summary>
        public void SetMaxActiveHints(int maxHints)
        {
            _maxActiveHints = Mathf.Max(1, maxHints);
        }
        
        /// <summary>
        /// Cleanup hint system
        /// </summary>
        public void Cleanup()
        {
            ClearHints();
            
            // Cleanup pooled hints
            foreach (var hintPanel in _hintPool)
            {
                hintPanel.OnDismissed -= HandleHintDismissed;
                hintPanel.OnClicked -= HandleHintClicked;
                hintPanel.Cleanup();
            }
            
            _hintPool.Clear();
            _isInitialized = false;
            
            Debug.Log("Tutorial hint system cleaned up");
        }
    }
}