using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.UI.Tutorial
{
    /// <summary>
    /// Tutorial progress indicator component for Project Chimera.
    /// Shows visual progress through tutorial sequences and steps.
    /// </summary>
    public class TutorialProgressIndicator
    {
        private VisualElement _progressContainer;
        private VisualElement _progressBar;
        private VisualElement _progressFill;
        private Label _progressLabel;
        private VisualElement _stepIndicators;
        
        // State
        private TutorialSequenceSO _currentSequence;
        private float _currentProgress;
        private int _totalSteps;
        private int _completedSteps;
        private bool _isInitialized;
        
        // Animation
        private float _animationSpeed = 2f;
        private float _targetProgress;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public float CurrentProgress => _currentProgress;
        public TutorialSequenceSO CurrentSequence => _currentSequence;
        
        public TutorialProgressIndicator(VisualElement progressContainer)
        {
            _progressContainer = progressContainer;
            
            InitializeProgressIndicator();
        }
        
        /// <summary>
        /// Initialize progress indicator
        /// </summary>
        private void InitializeProgressIndicator()
        {
            if (_progressContainer == null)
            {
                Debug.LogError("Progress container is null");
                return;
            }
            
            CreateProgressElements();
            
            _isInitialized = true;
            Debug.Log("Tutorial progress indicator initialized");
        }
        
        /// <summary>
        /// Create progress UI elements
        /// </summary>
        private void CreateProgressElements()
        {
            // Clear existing content
            _progressContainer.Clear();
            
            // Create progress bar
            _progressBar = new VisualElement();
            _progressBar.name = "progress-bar";
            _progressBar.AddToClassList("tutorial-progress-bar");
            _progressBar.style.height = 6f;
            _progressBar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            _progressBar.style.borderTopLeftRadius = 3f;
            _progressBar.style.borderTopRightRadius = 3f;
            _progressBar.style.borderBottomLeftRadius = 3f;
            _progressBar.style.borderBottomRightRadius = 3f;
            _progressBar.style.overflow = Overflow.Hidden;
            
            // Create progress fill
            _progressFill = new VisualElement();
            _progressFill.name = "progress-fill";
            _progressFill.AddToClassList("tutorial-progress-fill");
            _progressFill.style.height = new Length(100, LengthUnit.Percent);
            _progressFill.style.width = 0f;
            _progressFill.style.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 1f);
            _progressFill.style.borderTopLeftRadius = 3f;
            _progressFill.style.borderTopRightRadius = 0f;
            _progressFill.style.borderBottomLeftRadius = 3f;
            _progressFill.style.borderBottomRightRadius = 0f;
            
            _progressBar.Add(_progressFill);
            
            // Create progress label
            _progressLabel = new Label();
            _progressLabel.name = "progress-label";
            _progressLabel.AddToClassList("tutorial-progress-label");
            _progressLabel.style.fontSize = 10f;
            _progressLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            _progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _progressLabel.style.marginTop = 4f;
            _progressLabel.text = "Tutorial Progress";
            
            // Create step indicators container
            _stepIndicators = new VisualElement();
            _stepIndicators.name = "step-indicators";
            _stepIndicators.AddToClassList("tutorial-step-indicators");
            _stepIndicators.style.flexDirection = FlexDirection.Row;
            _stepIndicators.style.justifyContent = Justify.SpaceBetween;
            _stepIndicators.style.marginTop = 8f;
            _stepIndicators.style.display = DisplayStyle.None; // Hidden by default
            
            // Add elements to container
            _progressContainer.Add(_progressBar);
            _progressContainer.Add(_progressLabel);
            _progressContainer.Add(_stepIndicators);
        }
        
        /// <summary>
        /// Set current tutorial sequence
        /// </summary>
        public void SetSequence(TutorialSequenceSO sequence)
        {
            if (!_isInitialized || sequence == null)
                return;
            
            _currentSequence = sequence;
            _totalSteps = sequence.StepCount;
            _completedSteps = 0;
            _currentProgress = 0f;
            _targetProgress = 0f;
            
            // Update label
            _progressLabel.text = $"{sequence.SequenceName} - 0/{_totalSteps}";
            
            // Create step indicators
            CreateStepIndicators();
            
            // Reset progress fill
            _progressFill.style.width = 0f;
            
            Debug.Log($"Set tutorial sequence: {sequence.SequenceId} ({_totalSteps} steps)");
        }
        
        /// <summary>
        /// Create step indicators
        /// </summary>
        private void CreateStepIndicators()
        {
            if (_stepIndicators == null || _currentSequence == null)
                return;
            
            _stepIndicators.Clear();
            
            // Only show step indicators if we have a reasonable number of steps
            if (_totalSteps > 10)
            {
                _stepIndicators.style.display = DisplayStyle.None;
                return;
            }
            
            _stepIndicators.style.display = DisplayStyle.Flex;
            
            for (int i = 0; i < _totalSteps; i++)
            {
                var stepIndicator = new VisualElement();
                stepIndicator.name = $"step-indicator-{i}";
                stepIndicator.AddToClassList("tutorial-step-indicator");
                stepIndicator.style.width = 8f;
                stepIndicator.style.height = 8f;
                stepIndicator.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                stepIndicator.style.borderTopLeftRadius = 4f;
                stepIndicator.style.borderTopRightRadius = 4f;
                stepIndicator.style.borderBottomLeftRadius = 4f;
                stepIndicator.style.borderBottomRightRadius = 4f;
                stepIndicator.style.marginLeft = 2f;
                stepIndicator.style.marginRight = 2f;
                
                _stepIndicators.Add(stepIndicator);
            }
        }
        
        /// <summary>
        /// Update progress
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (!_isInitialized)
                return;
            
            _targetProgress = Mathf.Clamp01(progress);
            _completedSteps = Mathf.RoundToInt(_targetProgress * _totalSteps);
            
            // Update label
            if (_currentSequence != null)
            {
                _progressLabel.text = $"{_currentSequence.SequenceName} - {_completedSteps}/{_totalSteps}";
            }
            else
            {
                _progressLabel.text = $"Progress - {(_targetProgress * 100f):F0}%";
            }
            
            // Update step indicators
            UpdateStepIndicators();
            
            Debug.Log($"Updated tutorial progress: {_targetProgress:F2} ({_completedSteps}/{_totalSteps})");
        }
        
        /// <summary>
        /// Update step indicators
        /// </summary>
        private void UpdateStepIndicators()
        {
            if (_stepIndicators == null || _stepIndicators.childCount == 0)
                return;
            
            for (int i = 0; i < _stepIndicators.childCount; i++)
            {
                var indicator = _stepIndicators[i];
                
                if (i < _completedSteps)
                {
                    // Completed step
                    indicator.style.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 1f);
                }
                else if (i == _completedSteps)
                {
                    // Current step
                    indicator.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 1f);
                }
                else
                {
                    // Future step
                    indicator.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                }
            }
        }
        
        /// <summary>
        /// Complete sequence
        /// </summary>
        public void CompleteSequence()
        {
            if (!_isInitialized)
                return;
            
            _targetProgress = 1f;
            _completedSteps = _totalSteps;
            
            // Update label
            if (_currentSequence != null)
            {
                _progressLabel.text = $"{_currentSequence.SequenceName} - Complete!";
            }
            else
            {
                _progressLabel.text = "Tutorial Complete!";
            }
            
            // Set progress fill to completed color
            _progressFill.style.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 1f);
            
            UpdateStepIndicators();
            
            Debug.Log("Completed tutorial sequence progress");
        }
        
        /// <summary>
        /// Reset progress
        /// </summary>
        public void ResetProgress()
        {
            if (!_isInitialized)
                return;
            
            _currentProgress = 0f;
            _targetProgress = 0f;
            _completedSteps = 0;
            
            _progressFill.style.width = 0f;
            _progressLabel.text = "Tutorial Progress";
            
            // Reset step indicators
            if (_stepIndicators != null)
            {
                for (int i = 0; i < _stepIndicators.childCount; i++)
                {
                    var indicator = _stepIndicators[i];
                    indicator.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                }
            }
            
            Debug.Log("Reset tutorial progress");
        }
        
        /// <summary>
        /// Update progress animation
        /// </summary>
        public void UpdateAnimation()
        {
            if (!_isInitialized)
                return;
            
            // Animate progress bar
            if (Mathf.Abs(_currentProgress - _targetProgress) > 0.01f)
            {
                _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress, Time.deltaTime * _animationSpeed);
                
                // Update progress fill width
                _progressFill.style.width = new Length(_currentProgress * 100f, LengthUnit.Percent);
                
                // Update border radius based on progress
                if (_currentProgress >= 1f)
                {
                    _progressFill.style.borderTopRightRadius = 3f;
                    _progressFill.style.borderBottomRightRadius = 3f;
                }
                else
                {
                    _progressFill.style.borderTopRightRadius = 0f;
                    _progressFill.style.borderBottomRightRadius = 0f;
                }
            }
        }
        
        /// <summary>
        /// Set progress bar color
        /// </summary>
        public void SetProgressColor(Color color)
        {
            if (_progressFill != null)
            {
                _progressFill.style.backgroundColor = color;
            }
        }
        
        /// <summary>
        /// Set background color
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            if (_progressBar != null)
            {
                _progressBar.style.backgroundColor = color;
            }
        }
        
        /// <summary>
        /// Set visibility
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_progressContainer != null)
            {
                _progressContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Set animation speed
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = Mathf.Max(0.1f, speed);
        }
        
        /// <summary>
        /// Get progress info
        /// </summary>
        public TutorialProgressInfo GetProgressInfo()
        {
            return new TutorialProgressInfo
            {
                CurrentProgress = _currentProgress,
                TargetProgress = _targetProgress,
                CompletedSteps = _completedSteps,
                TotalSteps = _totalSteps,
                SequenceId = _currentSequence?.SequenceId ?? "",
                IsComplete = _currentProgress >= 1f
            };
        }
        
        /// <summary>
        /// Cleanup progress indicator
        /// </summary>
        public void Cleanup()
        {
            _currentSequence = null;
            _isInitialized = false;
            
            Debug.Log("Tutorial progress indicator cleaned up");
        }
    }
    
    /// <summary>
    /// Tutorial progress information
    /// </summary>
    public struct TutorialProgressInfo
    {
        public float CurrentProgress;
        public float TargetProgress;
        public int CompletedSteps;
        public int TotalSteps;
        public string SequenceId;
        public bool IsComplete;
    }
}