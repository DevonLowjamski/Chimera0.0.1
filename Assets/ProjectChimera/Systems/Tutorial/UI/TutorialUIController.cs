using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Systems.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Tutorial UI controller for Project Chimera.
    /// Manages tutorial overlay, guidance panels, and interactive UI elements.
    /// </summary>
    public class TutorialUIController : ChimeraMonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField] private UIDocument _tutorialUIDocument;
        [SerializeField] private string _overlayContainerName = "tutorial-overlay";
        [SerializeField] private string _guidancePanelName = "tutorial-guidance";
        [SerializeField] private string _progressBarName = "tutorial-progress";
        
        [Header("Tutorial Components")]
        [SerializeField] private VisualTreeAsset _guidancePanelTemplate;
        [SerializeField] private VisualTreeAsset _highlightOverlayTemplate;
        [SerializeField] private VisualTreeAsset _hintPanelTemplate;
        [SerializeField] private VisualTreeAsset _progressIndicatorTemplate;
        
        [Header("Animation Settings")]
        [SerializeField] private float _panelFadeInDuration = 0.3f;
        [SerializeField] private float _panelFadeOutDuration = 0.2f;
        [SerializeField] private float _highlightPulseDuration = 1.5f;
        [SerializeField] private AnimationCurve _fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // UI Elements
        private VisualElement _rootElement;
        private VisualElement _overlayContainer;
        private VisualElement _guidancePanel;
        private VisualElement _progressBar;
        private VisualElement _currentHighlight;
        private VisualElement _hintPanel;
        
        // UI Components
        private TutorialGuidancePanel _guidancePanelController;
        private TutorialProgressIndicator _progressIndicator;
        private TutorialHintSystem _hintSystem;
        private TutorialOverlayManager _overlayManager;
        
        // State
        private TutorialStepSO _currentStep;
        private TutorialSequenceSO _currentSequence;
        private bool _isUIInitialized;
        private bool _isVisible;
        
        // Animation
        private Dictionary<VisualElement, TutorialUIAnimation> _activeAnimations;
        
        // Events
        public System.Action<string> OnStepButtonClicked;
        public System.Action OnTutorialSkipped;
        public System.Action OnTutorialClosed;
        public System.Action<bool> OnTutorialVisibilityChanged;
        
        // Properties
        public bool IsUIInitialized => _isUIInitialized;
        public bool IsVisible => _isVisible;
        public TutorialStepSO CurrentStep => _currentStep;
        public TutorialSequenceSO CurrentSequence => _currentSequence;
        
        protected override void Start()
        {
            base.Start();
            
            InitializeTutorialUI();
        }
        
        /// <summary>
        /// Initialize tutorial UI system
        /// </summary>
        private void InitializeTutorialUI()
        {
            if (_tutorialUIDocument == null)
            {
                LogError("Tutorial UI Document not assigned!");
                return;
            }
            
            _rootElement = _tutorialUIDocument.rootVisualElement;
            _activeAnimations = new Dictionary<VisualElement, TutorialUIAnimation>();
            
            CreateUIStructure();
            InitializeComponents();
            SetupEventHandlers();
            
            // Initially hide tutorial UI
            SetUIVisibility(false);
            
            _isUIInitialized = true;
            LogInfo("Tutorial UI Controller initialized");
        }
        
        /// <summary>
        /// Create UI structure
        /// </summary>
        private void CreateUIStructure()
        {
            // Create overlay container
            _overlayContainer = new VisualElement();
            _overlayContainer.name = _overlayContainerName;
            _overlayContainer.AddToClassList("tutorial-overlay");
            _overlayContainer.style.position = Position.Absolute;
            _overlayContainer.style.left = 0;
            _overlayContainer.style.top = 0;
            _overlayContainer.style.right = 0;
            _overlayContainer.style.bottom = 0;
            _overlayContainer.style.display = DisplayStyle.None;
            
            _rootElement.Add(_overlayContainer);
            
            // Create guidance panel from template
            if (_guidancePanelTemplate != null)
            {
                _guidancePanel = _guidancePanelTemplate.Instantiate();
                _guidancePanel.name = _guidancePanelName;
                _guidancePanel.AddToClassList("tutorial-guidance-panel");
                _overlayContainer.Add(_guidancePanel);
            }
            else
            {
                CreateDefaultGuidancePanel();
            }
            
            // Create progress bar
            CreateProgressBar();
        }
        
        /// <summary>
        /// Create default guidance panel
        /// </summary>
        private void CreateDefaultGuidancePanel()
        {
            _guidancePanel = new VisualElement();
            _guidancePanel.name = _guidancePanelName;
            _guidancePanel.AddToClassList("tutorial-guidance-panel");
            
            // Panel styling
            _guidancePanel.style.position = Position.Absolute;
            _guidancePanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            _guidancePanel.style.borderTopWidth = 2f;
            _guidancePanel.style.borderRightWidth = 2f;
            _guidancePanel.style.borderBottomWidth = 2f;
            _guidancePanel.style.borderLeftWidth = 2f;
            _guidancePanel.style.borderTopColor = Color.white;
            _guidancePanel.style.borderRightColor = Color.white;
            _guidancePanel.style.borderBottomColor = Color.white;
            _guidancePanel.style.borderLeftColor = Color.white;
            _guidancePanel.style.borderTopLeftRadius = 8f;
            _guidancePanel.style.borderTopRightRadius = 8f;
            _guidancePanel.style.borderBottomLeftRadius = 8f;
            _guidancePanel.style.borderBottomRightRadius = 8f;
            _guidancePanel.style.paddingLeft = 20f;
            _guidancePanel.style.paddingRight = 20f;
            _guidancePanel.style.paddingTop = 20f;
            _guidancePanel.style.paddingBottom = 20f;
            _guidancePanel.style.minWidth = 300f;
            _guidancePanel.style.maxWidth = 500f;
            
            // Position panel
            _guidancePanel.style.top = new Length(20, LengthUnit.Percent);
            _guidancePanel.style.left = new Length(20, LengthUnit.Percent);
            
            _overlayContainer.Add(_guidancePanel);
        }
        
        /// <summary>
        /// Create progress bar
        /// </summary>
        private void CreateProgressBar()
        {
            _progressBar = new VisualElement();
            _progressBar.name = _progressBarName;
            _progressBar.AddToClassList("tutorial-progress-bar");
            
            // Progress bar styling
            _progressBar.style.position = Position.Absolute;
            _progressBar.style.top = 10f;
            _progressBar.style.left = new Length(20, LengthUnit.Percent);
            _progressBar.style.right = new Length(20, LengthUnit.Percent);
            _progressBar.style.height = 4f;
            _progressBar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            _progressBar.style.borderTopLeftRadius = 2f;
            _progressBar.style.borderTopRightRadius = 2f;
            _progressBar.style.borderBottomLeftRadius = 2f;
            _progressBar.style.borderBottomRightRadius = 2f;
            
            _overlayContainer.Add(_progressBar);
        }
        
        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeComponents()
        {
            _guidancePanelController = new TutorialGuidancePanel(_guidancePanel);
            _progressIndicator = new TutorialProgressIndicator(_progressBar);
            _hintSystem = new TutorialHintSystem(_overlayContainer, _hintPanelTemplate);
            _overlayManager = new TutorialOverlayManager(_overlayContainer);
            
            _guidancePanelController.OnButtonClicked += HandleGuidancePanelButton;
            _hintSystem.OnHintDismissed += HandleHintDismissed;
        }
        
        /// <summary>
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            // Subscribe to tutorial manager events
            TutorialManager.OnSequenceStarted += HandleSequenceStarted;
            TutorialManager.OnSequenceCompleted += HandleSequenceCompleted;
            TutorialManager.OnStepStarted += HandleStepStarted;
            TutorialManager.OnStepCompleted += HandleStepCompleted;
            TutorialManager.OnValidationResult += HandleValidationResult;
        }
        
        /// <summary>
        /// Show tutorial step
        /// </summary>
        public void ShowTutorialStep(TutorialStepSO step, TutorialSequenceSO sequence = null)
        {
            if (!_isUIInitialized || step == null)
                return;
            
            _currentStep = step;
            _currentSequence = sequence;
            
            // Show overlay
            SetUIVisibility(true);
            
            // Update guidance panel
            _guidancePanelController?.ShowStep(step);
            
            // Update progress if we have sequence
            if (sequence != null)
            {
                var progress = CalculateSequenceProgress(sequence, step);
                _progressIndicator?.UpdateProgress(progress);
            }
            
            // Position guidance panel based on step target
            PositionGuidancePanel(step);
            
            // Show highlight overlay
            _overlayManager?.ShowOverlay(step);
            
            // Animate panel in
            AnimatePanelIn(_guidancePanel);
            
            LogInfo($"Showed tutorial step: {step.StepId}");
        }
        
        /// <summary>
        /// Hide tutorial UI
        /// </summary>
        public void HideTutorialUI()
        {
            if (!_isUIInitialized)
                return;
            
            // Animate out and then hide
            AnimatePanelOut(_guidancePanel, () => {
                SetUIVisibility(false);
                _overlayManager?.ClearHighlights();
                _hintSystem?.ClearHints();
            });
            
            _currentStep = null;
            _currentSequence = null;
            
            LogInfo("Hid tutorial UI");
        }
        
        /// <summary>
        /// Show hint for current step
        /// </summary>
        public void ShowHint(TutorialHint hint)
        {
            if (!_isUIInitialized || hint == null)
                return;
            
            _hintSystem?.ShowHint(hint, _currentStep);
            
            LogInfo($"Showed tutorial hint: {hint.HintText}");
        }
        
        /// <summary>
        /// Update tutorial progress
        /// </summary>
        public void UpdateProgress(float progress)
        {
            _progressIndicator?.UpdateProgress(progress);
        }
        
        /// <summary>
        /// Set UI visibility
        /// </summary>
        private void SetUIVisibility(bool visible)
        {
            if (_overlayContainer != null)
            {
                _overlayContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            _isVisible = visible;
            OnTutorialVisibilityChanged?.Invoke(visible);
        }
        
        /// <summary>
        /// Position guidance panel based on step target
        /// </summary>
        private void PositionGuidancePanel(TutorialStepSO step)
        {
            if (_guidancePanel == null || step == null)
                return;
            
            // Default positioning
            var panelLeft = 20f;
            var panelTop = 20f;
            
            // Try to position panel relative to target
            switch (step.TargetType)
            {
                case TutorialTargetType.UIElement:
                case TutorialTargetType.Button:
                case TutorialTargetType.Panel:
                    // Position near target element
                    var targetBounds = GetTargetElementBounds(step.TargetElementId);
                    if (targetBounds.HasValue)
                    {
                        panelLeft = targetBounds.Value.xMax + 20f;
                        panelTop = targetBounds.Value.y;
                        
                        // Keep panel on screen
                        var screenWidth = Screen.width;
                        var screenHeight = Screen.height;
                        
                        if (panelLeft + 300f > screenWidth)
                        {
                            panelLeft = targetBounds.Value.x - 320f;
                        }
                        
                        if (panelTop + 200f > screenHeight)
                        {
                            panelTop = screenHeight - 220f;
                        }
                        
                        panelLeft = Mathf.Max(20f, panelLeft);
                        panelTop = Mathf.Max(20f, panelTop);
                    }
                    break;
                    
                case TutorialTargetType.ScreenArea:
                    // Position based on highlight offset
                    panelLeft = step.HighlightOffset.x + step.HighlightSize.x + 20f;
                    panelTop = step.HighlightOffset.y;
                    break;
                    
                default:
                    // Use default positioning
                    break;
            }
            
            _guidancePanel.style.left = panelLeft;
            _guidancePanel.style.top = panelTop;
        }
        
        /// <summary>
        /// Get target element bounds
        /// </summary>
        private Rect? GetTargetElementBounds(string elementId)
        {
            // In a full implementation, this would find the target element and return its bounds
            // For now, return null as placeholder
            return null;
        }
        
        /// <summary>
        /// Calculate sequence progress
        /// </summary>
        private float CalculateSequenceProgress(TutorialSequenceSO sequence, TutorialStepSO currentStep)
        {
            if (sequence == null || currentStep == null)
                return 0f;
            
            var stepIndex = sequence.TutorialSteps.IndexOf(currentStep);
            if (stepIndex < 0)
                return 0f;
            
            return (float)stepIndex / sequence.StepCount;
        }
        
        /// <summary>
        /// Animate panel in
        /// </summary>
        private void AnimatePanelIn(VisualElement panel)
        {
            if (panel == null)
                return;
            
            var animation = new TutorialUIAnimation
            {
                Element = panel,
                Duration = _panelFadeInDuration,
                StartTime = Time.time,
                AnimationType = TutorialAnimationType.FadeIn,
                Curve = _fadeInCurve
            };
            
            _activeAnimations[panel] = animation;
            
            // Set initial state
            panel.style.opacity = 0f;
            panel.style.scale = new Scale(Vector3.one * 0.8f);
        }
        
        /// <summary>
        /// Animate panel out
        /// </summary>
        private void AnimatePanelOut(VisualElement panel, System.Action onComplete = null)
        {
            if (panel == null)
                return;
            
            var animation = new TutorialUIAnimation
            {
                Element = panel,
                Duration = _panelFadeOutDuration,
                StartTime = Time.time,
                AnimationType = TutorialAnimationType.FadeOut,
                Curve = _fadeInCurve,
                OnComplete = onComplete
            };
            
            _activeAnimations[panel] = animation;
        }
        
        /// <summary>
        /// Update animations
        /// </summary>
        private void UpdateAnimations()
        {
            var completedAnimations = new List<VisualElement>();
            
            foreach (var kvp in _activeAnimations)
            {
                var element = kvp.Key;
                var animation = kvp.Value;
                
                var elapsed = Time.time - animation.StartTime;
                var progress = Mathf.Clamp01(elapsed / animation.Duration);
                var curveValue = animation.Curve.Evaluate(progress);
                
                switch (animation.AnimationType)
                {
                    case TutorialAnimationType.FadeIn:
                        element.style.opacity = curveValue;
                        element.style.scale = new Scale(Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, curveValue));
                        break;
                        
                    case TutorialAnimationType.FadeOut:
                        element.style.opacity = 1f - curveValue;
                        element.style.scale = new Scale(Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, curveValue));
                        break;
                        
                    case TutorialAnimationType.Pulse:
                        var pulseScale = 1f + Mathf.Sin(elapsed * Mathf.PI * 2f / _highlightPulseDuration) * 0.05f;
                        element.style.scale = new Scale(Vector3.one * pulseScale);
                        break;
                }
                
                if (progress >= 1f)
                {
                    completedAnimations.Add(element);
                    animation.OnComplete?.Invoke();
                }
            }
            
            // Remove completed animations
            foreach (var element in completedAnimations)
            {
                _activeAnimations.Remove(element);
            }
        }
        
        /// <summary>
        /// Handle sequence started
        /// </summary>
        private void HandleSequenceStarted(TutorialSequenceSO sequence)
        {
            _currentSequence = sequence;
            _progressIndicator?.SetSequence(sequence);
        }
        
        /// <summary>
        /// Handle sequence completed
        /// </summary>
        private void HandleSequenceCompleted(TutorialSequenceSO sequence)
        {
            _progressIndicator?.CompleteSequence();
            
            // Show completion feedback
            _guidancePanelController?.ShowCompletion(sequence);
        }
        
        /// <summary>
        /// Handle step started
        /// </summary>
        private void HandleStepStarted(TutorialStepSO step)
        {
            ShowTutorialStep(step, _currentSequence);
        }
        
        /// <summary>
        /// Handle step completed
        /// </summary>
        private void HandleStepCompleted(TutorialStepSO step)
        {
            _hintSystem?.ClearHints();
            
            if (_currentSequence != null)
            {
                var progress = CalculateSequenceProgress(_currentSequence, step);
                _progressIndicator?.UpdateProgress(progress);
            }
        }
        
        /// <summary>
        /// Handle validation result
        /// </summary>
        private void HandleValidationResult(TutorialValidationResult result)
        {
            if (!result.IsValid)
            {
                _guidancePanelController?.ShowValidationError(result.ErrorMessage);
                
                // Show hint if available
                if (result.Feedback == TutorialValidationFeedback.Hint && _currentStep?.Hints?.Count > 0)
                {
                    ShowHint(_currentStep.Hints[0]);
                }
            }
        }
        
        /// <summary>
        /// Handle guidance panel button click
        /// </summary>
        private void HandleGuidancePanelButton(string buttonId)
        {
            switch (buttonId)
            {
                case "next":
                case "continue":
                    OnStepButtonClicked?.Invoke("continue");
                    break;
                    
                case "skip":
                    OnTutorialSkipped?.Invoke();
                    break;
                    
                case "close":
                    OnTutorialClosed?.Invoke();
                    HideTutorialUI();
                    break;
                    
                default:
                    OnStepButtonClicked?.Invoke(buttonId);
                    break;
            }
        }
        
        /// <summary>
        /// Handle hint dismissed
        /// </summary>
        private void HandleHintDismissed()
        {
            LogInfo("Tutorial hint dismissed");
        }
        
        private void Update()
        {
            if (_isUIInitialized)
            {
                UpdateAnimations();
                _overlayManager?.UpdateHighlights();
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Unsubscribe from events
            TutorialManager.OnSequenceStarted -= HandleSequenceStarted;
            TutorialManager.OnSequenceCompleted -= HandleSequenceCompleted;
            TutorialManager.OnStepStarted -= HandleStepStarted;
            TutorialManager.OnStepCompleted -= HandleStepCompleted;
            TutorialManager.OnValidationResult -= HandleValidationResult;
            
            if (_guidancePanelController != null)
            {
                _guidancePanelController.OnButtonClicked -= HandleGuidancePanelButton;
            }
            
            if (_hintSystem != null)
            {
                _hintSystem.OnHintDismissed -= HandleHintDismissed;
            }
        }
    }
    
    /// <summary>
    /// Tutorial UI animation data
    /// </summary>
    public class TutorialUIAnimation
    {
        public VisualElement Element;
        public float Duration;
        public float StartTime;
        public TutorialAnimationType AnimationType;
        public AnimationCurve Curve;
        public System.Action OnComplete;
    }
    
    /// <summary>
    /// Tutorial animation types
    /// </summary>
    public enum TutorialAnimationType
    {
        FadeIn,
        FadeOut,
        Pulse,
        Slide,
        Scale
    }
}