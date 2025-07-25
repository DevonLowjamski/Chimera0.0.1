using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Systems.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Onboarding UI controller for Project Chimera.
    /// Manages the user interface during the onboarding sequence.
    /// </summary>
    public class OnboardingUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument _onboardingUIDocument;
        [SerializeField] private VisualTreeAsset _welcomeScreenTemplate;
        [SerializeField] private VisualTreeAsset _progressBarTemplate;
        
        [Header("Onboarding Configuration")]
        [SerializeField] private OnboardingStepDefinitions _stepDefinitions;
        [SerializeField] private float _welcomeScreenDuration = 3f;
        [SerializeField] private bool _showProgressDuringWelcome = true;
        
        // UI Elements
        private VisualElement _rootElement;
        private VisualElement _welcomeScreen;
        private VisualElement _onboardingContainer;
        private VisualElement _progressContainer;
        private VisualElement _skipContainer;
        private Label _phaseLabel;
        private Label _progressLabel;
        private Button _skipButton;
        private Button _continueButton;
        
        // Components
        private TutorialProgressIndicator _progressIndicator;
        private OnboardingSequenceManager _onboardingManager;
        
        // State
        private bool _isInitialized;
        private bool _isWelcomeScreenActive;
        private OnboardingPhase _currentPhase = OnboardingPhase.None;
        private float _welcomeScreenStartTime;
        
        // Events
        public System.Action OnWelcomeScreenCompleted;
        public System.Action OnOnboardingSkipped;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsWelcomeScreenActive => _isWelcomeScreenActive;
        public OnboardingPhase CurrentPhase => _currentPhase;
        
        private void Awake()
        {
            InitializeOnboardingUI();
        }
        
        private void Start()
        {
            // Find onboarding manager
            _onboardingManager = FindObjectOfType<OnboardingSequenceManager>();
            
            if (_onboardingManager != null)
            {
                SubscribeToOnboardingEvents();
                
                // Show welcome screen if onboarding is starting
                if (_onboardingManager.IsOnboardingActive)
                {
                    ShowWelcomeScreen();
                }
            }
        }
        
        /// <summary>
        /// Initialize onboarding UI
        /// </summary>
        private void InitializeOnboardingUI()
        {
            if (_onboardingUIDocument == null)
            {
                Debug.LogError("Onboarding UI Document is not assigned");
                return;
            }
            
            _rootElement = _onboardingUIDocument.rootVisualElement;
            
            CreateOnboardingElements();
            SetupEventHandlers();
            
            _isInitialized = true;
            Debug.Log("Onboarding UI controller initialized");
        }
        
        /// <summary>
        /// Create onboarding UI elements
        /// </summary>
        private void CreateOnboardingElements()
        {
            // Create main onboarding container
            _onboardingContainer = new VisualElement();
            _onboardingContainer.name = "onboarding-container";
            _onboardingContainer.AddToClassList("onboarding-container");
            _onboardingContainer.style.position = Position.Absolute;
            _onboardingContainer.style.left = 0;
            _onboardingContainer.style.top = 0;
            _onboardingContainer.style.right = 0;
            _onboardingContainer.style.bottom = 0;
            _onboardingContainer.style.backgroundColor = new Color(0f, 0f, 0f, 0.9f);
            _onboardingContainer.style.display = DisplayStyle.None;
            
            // Create welcome screen
            CreateWelcomeScreen();
            
            // Create progress container
            CreateProgressContainer();
            
            // Create skip container
            CreateSkipContainer();
            
            _rootElement.Add(_onboardingContainer);
        }
        
        /// <summary>
        /// Create welcome screen
        /// </summary>
        private void CreateWelcomeScreen()
        {
            if (_welcomeScreenTemplate != null)
            {
                _welcomeScreen = _welcomeScreenTemplate.Instantiate();
            }
            else
            {
                _welcomeScreen = CreateDefaultWelcomeScreen();
            }
            
            _welcomeScreen.name = "welcome-screen";
            _welcomeScreen.style.display = DisplayStyle.None;
            
            _onboardingContainer.Add(_welcomeScreen);
        }
        
        /// <summary>
        /// Create default welcome screen
        /// </summary>
        private VisualElement CreateDefaultWelcomeScreen()
        {
            var welcomeScreen = new VisualElement();
            welcomeScreen.AddToClassList("welcome-screen");
            welcomeScreen.style.justifyContent = Justify.Center;
            welcomeScreen.style.alignItems = Align.Center;
            welcomeScreen.style.width = new Length(100, LengthUnit.Percent);
            welcomeScreen.style.height = new Length(100, LengthUnit.Percent);
            
            // Welcome content container
            var contentContainer = new VisualElement();
            contentContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            contentContainer.style.borderTopLeftRadius = 10f;
            contentContainer.style.borderTopRightRadius = 10f;
            contentContainer.style.borderBottomLeftRadius = 10f;
            contentContainer.style.borderBottomRightRadius = 10f;
            contentContainer.style.paddingLeft = 40f;
            contentContainer.style.paddingRight = 40f;
            contentContainer.style.paddingTop = 40f;
            contentContainer.style.paddingBottom = 40f;
            contentContainer.style.maxWidth = 600f;
            contentContainer.style.alignItems = Align.Center;
            
            // Game logo/title
            var titleLabel = new Label("Project Chimera");
            titleLabel.AddToClassList("welcome-title");
            titleLabel.style.fontSize = 36f;
            titleLabel.style.color = new Color(0.8f, 0.6f, 0.2f, 1f);
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.marginBottom = 20f;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            // Subtitle
            var subtitleLabel = new Label("Cannabis Cultivation Simulation");
            subtitleLabel.AddToClassList("welcome-subtitle");
            subtitleLabel.style.fontSize = 18f;
            subtitleLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            subtitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            subtitleLabel.style.marginBottom = 30f;
            
            // Welcome message
            var messageLabel = new Label("Welcome to the most advanced cannabis cultivation simulation. Let's get you started on your journey to becoming a master cultivator.");
            messageLabel.AddToClassList("welcome-message");
            messageLabel.style.fontSize = 14f;
            messageLabel.style.color = Color.white;
            messageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            messageLabel.style.whiteSpace = WhiteSpace.Normal;
            messageLabel.style.maxWidth = 500f;
            messageLabel.style.marginBottom = 30f;
            
            // Continue button
            _continueButton = new Button();
            _continueButton.name = "welcome-continue-button";
            _continueButton.text = "Begin Tutorial";
            _continueButton.AddToClassList("welcome-continue-button");
            _continueButton.style.fontSize = 16f;
            _continueButton.style.paddingLeft = 12f;
            _continueButton.style.paddingRight = 12f;
            _continueButton.style.paddingTop = 12f;
            _continueButton.style.paddingBottom = 12f;
            _continueButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            _continueButton.style.color = Color.white;
            _continueButton.style.borderTopLeftRadius = 6f;
            _continueButton.style.borderTopRightRadius = 6f;
            _continueButton.style.borderBottomLeftRadius = 6f;
            _continueButton.style.borderBottomRightRadius = 6f;
            _continueButton.style.borderTopWidth = 0;
            _continueButton.style.borderRightWidth = 0;
            _continueButton.style.borderBottomWidth = 0;
            _continueButton.style.borderLeftWidth = 0;
            _continueButton.style.minWidth = 150f;
            
            contentContainer.Add(titleLabel);
            contentContainer.Add(subtitleLabel);
            contentContainer.Add(messageLabel);
            contentContainer.Add(_continueButton);
            
            welcomeScreen.Add(contentContainer);
            
            return welcomeScreen;
        }
        
        /// <summary>
        /// Create progress container
        /// </summary>
        private void CreateProgressContainer()
        {
            _progressContainer = new VisualElement();
            _progressContainer.name = "onboarding-progress-container";
            _progressContainer.AddToClassList("onboarding-progress");
            _progressContainer.style.position = Position.Absolute;
            _progressContainer.style.top = 20f;
            _progressContainer.style.left = 20f;
            _progressContainer.style.right = 20f;
            _progressContainer.style.height = 60f;
            _progressContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            _progressContainer.style.borderTopLeftRadius = 8f;
            _progressContainer.style.borderTopRightRadius = 8f;
            _progressContainer.style.borderBottomLeftRadius = 8f;
            _progressContainer.style.borderBottomRightRadius = 8f;
            _progressContainer.style.paddingLeft = 10f;
            _progressContainer.style.paddingRight = 10f;
            _progressContainer.style.paddingTop = 10f;
            _progressContainer.style.paddingBottom = 10f;
            _progressContainer.style.display = DisplayStyle.None;
            
            // Phase label
            _phaseLabel = new Label("Welcome Phase");
            _phaseLabel.name = "phase-label";
            _phaseLabel.style.fontSize = 14f;
            _phaseLabel.style.color = new Color(0.8f, 0.6f, 0.2f, 1f);
            _phaseLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _phaseLabel.style.marginBottom = 5f;
            
            // Progress label
            _progressLabel = new Label("Step 1 of 15");
            _progressLabel.name = "progress-label";
            _progressLabel.style.fontSize = 12f;
            _progressLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            _progressContainer.Add(_phaseLabel);
            _progressContainer.Add(_progressLabel);
            
            // Initialize progress indicator
            _progressIndicator = new TutorialProgressIndicator(_progressContainer);
            
            _onboardingContainer.Add(_progressContainer);
        }
        
        /// <summary>
        /// Create skip container
        /// </summary>
        private void CreateSkipContainer()
        {
            _skipContainer = new VisualElement();
            _skipContainer.name = "onboarding-skip-container";
            _skipContainer.style.position = Position.Absolute;
            _skipContainer.style.bottom = 20f;
            _skipContainer.style.right = 20f;
            _skipContainer.style.display = DisplayStyle.None;
            
            // Skip button
            _skipButton = new Button();
            _skipButton.name = "skip-onboarding-button";
            _skipButton.text = "Skip Tutorial";
            _skipButton.style.fontSize = 12f;
            _skipButton.style.paddingLeft = 8f;
            _skipButton.style.paddingRight = 8f;
            _skipButton.style.paddingTop = 8f;
            _skipButton.style.paddingBottom = 8f;
            _skipButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.8f);
            _skipButton.style.color = Color.white;
            _skipButton.style.borderTopLeftRadius = 4f;
            _skipButton.style.borderTopRightRadius = 4f;
            _skipButton.style.borderBottomLeftRadius = 4f;
            _skipButton.style.borderBottomRightRadius = 4f;
            _skipButton.style.borderTopWidth = 0;
            _skipButton.style.borderRightWidth = 0;
            _skipButton.style.borderBottomWidth = 0;
            _skipButton.style.borderLeftWidth = 0;
            
            _skipContainer.Add(_skipButton);
            _onboardingContainer.Add(_skipContainer);
        }
        
        /// <summary>
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            _continueButton?.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
                HideWelcomeScreen();
            });
            
            _skipButton?.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
                SkipOnboarding();
            });
        }
        
        /// <summary>
        /// Subscribe to onboarding events
        /// </summary>
        private void SubscribeToOnboardingEvents()
        {
            if (_onboardingManager != null)
            {
                // Subscribe to onboarding events if available
                Debug.Log("Subscribed to onboarding manager events");
            }
        }
        
        /// <summary>
        /// Show welcome screen
        /// </summary>
        public void ShowWelcomeScreen()
        {
            if (!_isInitialized)
                return;
            
            _onboardingContainer.style.display = DisplayStyle.Flex;
            _welcomeScreen.style.display = DisplayStyle.Flex;
            _isWelcomeScreenActive = true;
            _welcomeScreenStartTime = Time.time;
            
            // Show progress if enabled
            if (_showProgressDuringWelcome)
            {
                _progressContainer.style.display = DisplayStyle.Flex;
                UpdatePhaseDisplay(OnboardingPhase.Welcome);
            }
            
            // Show skip button
            _skipContainer.style.display = DisplayStyle.Flex;
            
            Debug.Log("Showed onboarding welcome screen");
        }
        
        /// <summary>
        /// Hide welcome screen
        /// </summary>
        public void HideWelcomeScreen()
        {
            if (!_isWelcomeScreenActive)
                return;
            
            _welcomeScreen.style.display = DisplayStyle.None;
            _isWelcomeScreenActive = false;
            
            OnWelcomeScreenCompleted?.Invoke();
            
            Debug.Log("Hid onboarding welcome screen");
        }
        
        /// <summary>
        /// Update phase display
        /// </summary>
        public void UpdatePhaseDisplay(OnboardingPhase phase)
        {
            _currentPhase = phase;
            
            if (_phaseLabel != null)
            {
                switch (phase)
                {
                    case OnboardingPhase.Welcome:
                        _phaseLabel.text = "Welcome Phase";
                        break;
                    case OnboardingPhase.FacilitySetup:
                        _phaseLabel.text = "Facility Setup";
                        break;
                    case OnboardingPhase.FirstPlant:
                        _phaseLabel.text = "First Plant";
                        break;
                    case OnboardingPhase.Completed:
                        _phaseLabel.text = "Tutorial Complete";
                        break;
                    default:
                        _phaseLabel.text = "Onboarding";
                        break;
                }
            }
        }
        
        /// <summary>
        /// Update progress display
        /// </summary>
        public void UpdateProgressDisplay(OnboardingProgress progress)
        {
            if (_progressLabel != null)
            {
                _progressLabel.text = $"Step {progress.CompletedSteps + 1} of {progress.TotalSteps}";
            }
            
            if (_progressIndicator != null)
            {
                _progressIndicator.UpdateProgress(progress.Progress);
            }
            
            UpdatePhaseDisplay(progress.CurrentPhase);
        }
        
        /// <summary>
        /// Skip onboarding
        /// </summary>
        private void SkipOnboarding()
        {
            // Show confirmation dialog
            ShowSkipConfirmation();
        }
        
        /// <summary>
        /// Show skip confirmation dialog
        /// </summary>
        private void ShowSkipConfirmation()
        {
            // In a full implementation, this would show a proper confirmation dialog
            var confirmed = true; // Placeholder
            
            if (confirmed)
            {
                if (_onboardingManager != null)
                {
                    _onboardingManager.SkipOnboarding();
                }
                
                HideOnboardingUI();
                OnOnboardingSkipped?.Invoke();
                
                Debug.Log("Onboarding skipped by user");
            }
        }
        
        /// <summary>
        /// Hide onboarding UI
        /// </summary>
        public void HideOnboardingUI()
        {
            if (_onboardingContainer != null)
            {
                _onboardingContainer.style.display = DisplayStyle.None;
            }
            
            _isWelcomeScreenActive = false;
            _currentPhase = OnboardingPhase.None;
            
            Debug.Log("Hid onboarding UI");
        }
        
        /// <summary>
        /// Show onboarding UI
        /// </summary>
        public void ShowOnboardingUI()
        {
            if (_onboardingContainer != null)
            {
                _onboardingContainer.style.display = DisplayStyle.Flex;
                _progressContainer.style.display = DisplayStyle.Flex;
                _skipContainer.style.display = DisplayStyle.Flex;
            }
        }
        
        private void Update()
        {
            // Update progress indicator animation
            if (_progressIndicator != null)
            {
                _progressIndicator.UpdateAnimation();
            }
            
            // Auto-hide welcome screen after duration
            if (_isWelcomeScreenActive && _welcomeScreenDuration > 0)
            {
                if (Time.time - _welcomeScreenStartTime >= _welcomeScreenDuration)
                {
                    // Could auto-advance here, but better to wait for user input
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_onboardingManager != null)
            {
                // Unsubscribe from onboarding events if subscribed
            }
        }
    }
}