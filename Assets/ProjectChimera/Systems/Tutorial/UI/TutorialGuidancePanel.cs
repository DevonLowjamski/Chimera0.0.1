using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Tutorial guidance panel component for Project Chimera.
    /// Manages the main tutorial instruction panel with step content and controls.
    /// </summary>
    public class TutorialGuidancePanel
    {
        private VisualElement _panelContainer;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Label _instructionsLabel;
        private VisualElement _imageContainer;
        private VisualElement _buttonContainer;
        private Button _nextButton;
        private Button _skipButton;
        private Button _closeButton;
        private VisualElement _errorPanel;
        private Label _errorLabel;
        
        // State
        private TutorialStepSO _currentStep;
        private bool _isInitialized;
        
        // Events
        public System.Action<string> OnButtonClicked;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public TutorialStepSO CurrentStep => _currentStep;
        
        public TutorialGuidancePanel(VisualElement panelContainer)
        {
            _panelContainer = panelContainer;
            
            InitializePanel();
        }
        
        /// <summary>
        /// Initialize guidance panel
        /// </summary>
        private void InitializePanel()
        {
            if (_panelContainer == null)
            {
                Debug.LogError("Panel container is null");
                return;
            }
            
            CreatePanelElements();
            SetupEventHandlers();
            
            _isInitialized = true;
            Debug.Log("Tutorial guidance panel initialized");
        }
        
        /// <summary>
        /// Create panel UI elements
        /// </summary>
        private void CreatePanelElements()
        {
            // Clear existing content
            _panelContainer.Clear();
            
            // Create header
            CreateHeader();
            
            // Create content area
            CreateContentArea();
            
            // Create button area
            CreateButtonArea();
            
            // Create error panel (initially hidden)
            CreateErrorPanel();
        }
        
        /// <summary>
        /// Create header section
        /// </summary>
        private void CreateHeader()
        {
            var header = new VisualElement();
            header.name = "tutorial-header";
            header.AddToClassList("tutorial-header");
            header.style.marginBottom = 15f;
            
            _titleLabel = new Label();
            _titleLabel.name = "tutorial-title";
            _titleLabel.AddToClassList("tutorial-title");
            _titleLabel.style.fontSize = 18f;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.marginBottom = 5f;
            
            header.Add(_titleLabel);
            _panelContainer.Add(header);
        }
        
        /// <summary>
        /// Create content area
        /// </summary>
        private void CreateContentArea()
        {
            var contentArea = new VisualElement();
            contentArea.name = "tutorial-content";
            contentArea.AddToClassList("tutorial-content");
            contentArea.style.marginBottom = 20f;
            
            // Description label
            _descriptionLabel = new Label();
            _descriptionLabel.name = "tutorial-description";
            _descriptionLabel.AddToClassList("tutorial-description");
            _descriptionLabel.style.fontSize = 14f;
            _descriptionLabel.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            _descriptionLabel.style.marginBottom = 10f;
            _descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            
            // Instructions label
            _instructionsLabel = new Label();
            _instructionsLabel.name = "tutorial-instructions";
            _instructionsLabel.AddToClassList("tutorial-instructions");
            _instructionsLabel.style.fontSize = 12f;
            _instructionsLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            _instructionsLabel.style.marginBottom = 15f;
            _instructionsLabel.style.whiteSpace = WhiteSpace.Normal;
            
            // Image container
            _imageContainer = new VisualElement();
            _imageContainer.name = "tutorial-image";
            _imageContainer.AddToClassList("tutorial-image");
            _imageContainer.style.height = 100f;
            _imageContainer.style.marginBottom = 15f;
            _imageContainer.style.display = DisplayStyle.None; // Hidden by default
            
            contentArea.Add(_descriptionLabel);
            contentArea.Add(_instructionsLabel);
            contentArea.Add(_imageContainer);
            
            _panelContainer.Add(contentArea);
        }
        
        /// <summary>
        /// Create button area
        /// </summary>
        private void CreateButtonArea()
        {
            _buttonContainer = new VisualElement();
            _buttonContainer.name = "tutorial-buttons";
            _buttonContainer.AddToClassList("tutorial-buttons");
            _buttonContainer.style.flexDirection = FlexDirection.Row;
            _buttonContainer.style.justifyContent = Justify.SpaceBetween;
            _buttonContainer.style.marginTop = 10f;
            
            // Next/Continue button
            _nextButton = new Button();
            _nextButton.name = "tutorial-next";
            _nextButton.text = "Continue";
            _nextButton.AddToClassList("tutorial-button");
            _nextButton.AddToClassList("tutorial-button-primary");
            _nextButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            _nextButton.style.color = Color.white;
            _nextButton.style.borderTopWidth = 0;
            _nextButton.style.borderRightWidth = 0;
            _nextButton.style.borderBottomWidth = 0;
            _nextButton.style.borderLeftWidth = 0;
            _nextButton.style.borderTopLeftRadius = 4f;
            _nextButton.style.borderTopRightRadius = 4f;
            _nextButton.style.borderBottomLeftRadius = 4f;
            _nextButton.style.borderBottomRightRadius = 4f;
            _nextButton.style.paddingLeft = 15f;
            _nextButton.style.paddingRight = 15f;
            _nextButton.style.paddingTop = 8f;
            _nextButton.style.paddingBottom = 8f;
            
            // Skip button
            _skipButton = new Button();
            _skipButton.name = "tutorial-skip";
            _skipButton.text = "Skip";
            _skipButton.AddToClassList("tutorial-button");
            _skipButton.AddToClassList("tutorial-button-secondary");
            _skipButton.style.backgroundColor = new Color(0.6f, 0.6f, 0.2f, 1f);
            _skipButton.style.color = Color.white;
            _skipButton.style.borderTopWidth = 0;
            _skipButton.style.borderRightWidth = 0;
            _skipButton.style.borderBottomWidth = 0;
            _skipButton.style.borderLeftWidth = 0;
            _skipButton.style.borderTopLeftRadius = 4f;
            _skipButton.style.borderTopRightRadius = 4f;
            _skipButton.style.borderBottomLeftRadius = 4f;
            _skipButton.style.borderBottomRightRadius = 4f;
            _skipButton.style.paddingLeft = 15f;
            _skipButton.style.paddingRight = 15f;
            _skipButton.style.paddingTop = 8f;
            _skipButton.style.paddingBottom = 8f;
            
            // Close button
            _closeButton = new Button();
            _closeButton.name = "tutorial-close";
            _closeButton.text = "Close";
            _closeButton.AddToClassList("tutorial-button");
            _closeButton.AddToClassList("tutorial-button-danger");
            _closeButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 1f);
            _closeButton.style.color = Color.white;
            _closeButton.style.borderTopWidth = 0;
            _closeButton.style.borderRightWidth = 0;
            _closeButton.style.borderBottomWidth = 0;
            _closeButton.style.borderLeftWidth = 0;
            _closeButton.style.borderTopLeftRadius = 4f;
            _closeButton.style.borderTopRightRadius = 4f;
            _closeButton.style.borderBottomLeftRadius = 4f;
            _closeButton.style.borderBottomRightRadius = 4f;
            _closeButton.style.paddingLeft = 15f;
            _closeButton.style.paddingRight = 15f;
            _closeButton.style.paddingTop = 8f;
            _closeButton.style.paddingBottom = 8f;
            
            var buttonGroup = new VisualElement();
            buttonGroup.style.flexDirection = FlexDirection.Row;
            buttonGroup.style.justifyContent = Justify.FlexEnd;
            
            buttonGroup.Add(_nextButton);
            buttonGroup.Add(_skipButton);
            buttonGroup.Add(_closeButton);
            
            _buttonContainer.Add(buttonGroup);
            _panelContainer.Add(_buttonContainer);
        }
        
        /// <summary>
        /// Create error panel
        /// </summary>
        private void CreateErrorPanel()
        {
            _errorPanel = new VisualElement();
            _errorPanel.name = "tutorial-error";
            _errorPanel.AddToClassList("tutorial-error");
            _errorPanel.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.8f);
            _errorPanel.style.borderTopLeftRadius = 4f;
            _errorPanel.style.borderTopRightRadius = 4f;
            _errorPanel.style.borderBottomLeftRadius = 4f;
            _errorPanel.style.borderBottomRightRadius = 4f;
            _errorPanel.style.paddingLeft = 10f;
            _errorPanel.style.paddingRight = 10f;
            _errorPanel.style.paddingTop = 10f;
            _errorPanel.style.paddingBottom = 10f;
            _errorPanel.style.marginBottom = 10f;
            _errorPanel.style.display = DisplayStyle.None;
            
            _errorLabel = new Label();
            _errorLabel.name = "tutorial-error-text";
            _errorLabel.style.color = Color.white;
            _errorLabel.style.fontSize = 12f;
            _errorLabel.style.whiteSpace = WhiteSpace.Normal;
            
            _errorPanel.Add(_errorLabel);
            _panelContainer.Insert(0, _errorPanel);
        }
        
        /// <summary>
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            _nextButton?.RegisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("continue"));
            _skipButton?.RegisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("skip"));
            _closeButton?.RegisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("close"));
        }
        
        /// <summary>
        /// Show tutorial step
        /// </summary>
        public void ShowStep(TutorialStepSO step)
        {
            if (!_isInitialized || step == null)
                return;
            
            _currentStep = step;
            
            // Update content
            _titleLabel.text = step.Title;
            _descriptionLabel.text = step.ShortDescription;
            _instructionsLabel.text = step.DetailedInstructions;
            
            // Show/hide image
            if (step.IllustrationImage != null)
            {
                _imageContainer.style.display = DisplayStyle.Flex;
                _imageContainer.style.backgroundImage = new StyleBackground(step.IllustrationImage);
            }
            else
            {
                _imageContainer.style.display = DisplayStyle.None;
            }
            
            // Update button states
            _skipButton.SetEnabled(step.CanSkip);
            _skipButton.style.display = step.CanSkip ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Update button text based on step type
            switch (step.StepType)
            {
                case TutorialStepType.Introduction:
                    _nextButton.text = "Start";
                    break;
                case TutorialStepType.Completion:
                    _nextButton.text = "Finish";
                    break;
                default:
                    _nextButton.text = "Continue";
                    break;
            }
            
            // Hide error panel
            HideError();
            
            Debug.Log($"Showing tutorial step: {step.StepId}");
        }
        
        /// <summary>
        /// Show completion message
        /// </summary>
        public void ShowCompletion(TutorialSequenceSO sequence)
        {
            if (!_isInitialized || sequence == null)
                return;
            
            _titleLabel.text = "Tutorial Complete!";
            _descriptionLabel.text = $"You have successfully completed the {sequence.SequenceName} tutorial.";
            _instructionsLabel.text = "You can now explore the features you've learned about.";
            
            // Hide image
            _imageContainer.style.display = DisplayStyle.None;
            
            // Update buttons for completion
            _nextButton.text = "Close";
            _skipButton.style.display = DisplayStyle.None;
            _closeButton.style.display = DisplayStyle.None;
            
            HideError();
            
            Debug.Log($"Showing tutorial completion: {sequence.SequenceId}");
        }
        
        /// <summary>
        /// Show validation error
        /// </summary>
        public void ShowValidationError(string errorMessage)
        {
            if (!_isInitialized || string.IsNullOrEmpty(errorMessage))
                return;
            
            _errorLabel.text = errorMessage;
            _errorPanel.style.display = DisplayStyle.Flex;
            
            Debug.Log($"Showing validation error: {errorMessage}");
        }
        
        /// <summary>
        /// Hide error panel
        /// </summary>
        public void HideError()
        {
            if (_errorPanel != null)
            {
                _errorPanel.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Set panel visibility
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_panelContainer != null)
            {
                _panelContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Update panel position
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            if (_panelContainer != null)
            {
                _panelContainer.style.left = position.x;
                _panelContainer.style.top = position.y;
            }
        }
        
        /// <summary>
        /// Enable/disable interaction
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _nextButton?.SetEnabled(interactable);
            _skipButton?.SetEnabled(interactable && (_currentStep?.CanSkip ?? false));
            _closeButton?.SetEnabled(interactable);
        }
        
        /// <summary>
        /// Cleanup guidance panel
        /// </summary>
        public void Cleanup()
        {
            // Unregister event handlers
            _nextButton?.UnregisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("continue"));
            _skipButton?.UnregisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("skip"));
            _closeButton?.UnregisterCallback<ClickEvent>(evt => OnButtonClicked?.Invoke("close"));
            
            _currentStep = null;
            _isInitialized = false;
            
            Debug.Log("Tutorial guidance panel cleaned up");
        }
    }
}