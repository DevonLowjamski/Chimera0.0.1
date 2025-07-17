using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// ScriptableObject definition for individual tutorial steps in Project Chimera.
    /// Defines step-by-step instructions and validation for player guidance.
    /// </summary>
    [CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Project Chimera/Tutorial/Tutorial Step")]
    public class TutorialStepSO : ChimeraDataSO
    {
        [Header("Step Identification")]
        [SerializeField] private string _stepId;
        [SerializeField] private string _title;
        [SerializeField] private string _shortDescription;
        
        [Header("Content")]
        [SerializeField] private string _detailedInstructions;
        [SerializeField] private TutorialStepType _stepType = TutorialStepType.Instruction;
        [SerializeField] private Sprite _illustrationImage;
        [SerializeField] private AudioClip _narrationClip;
        
        [Header("Target Configuration")]
        [SerializeField] private TutorialTargetType _targetType = TutorialTargetType.UIElement;
        [SerializeField] private string _targetElementId;
        [SerializeField] private Vector2 _highlightOffset = Vector2.zero;
        [SerializeField] private Vector2 _highlightSize = new Vector2(100, 50);
        [SerializeField] private TutorialHighlightShape _highlightShape = TutorialHighlightShape.Rectangle;
        
        [Header("Validation")]
        [SerializeField] private TutorialValidationType _validationType = TutorialValidationType.ButtonClick;
        [SerializeField] private string _validationTarget;
        [SerializeField] private float _timeoutDuration = 30f;
        [SerializeField] private bool _isOptional = false;
        [SerializeField] private bool _canSkip = true;
        
        [Header("Prerequisites")]
        [SerializeField] private List<TutorialStepSO> _prerequisiteSteps = new List<TutorialStepSO>();
        [SerializeField] private List<string> _requiredUnlocks = new List<string>();
        
        [Header("Hints")]
        [SerializeField] private List<TutorialHint> _hints = new List<TutorialHint>();
        [SerializeField] private float _hintDelay = 5f;
        
        [Header("Advanced Settings")]
        [SerializeField] private bool _pauseGameplay = false;
        [SerializeField] private bool _dimBackground = true;
        [SerializeField] private float _backgroundDimAmount = 0.7f;
        [SerializeField] private bool _allowInterruption = false;
        [SerializeField] private bool _blockInteraction = true;
        [SerializeField] private List<string> _allowedInteractionElements = new List<string>();
        
        [Header("Highlight Settings")]
        [SerializeField] private TutorialHighlightSettings _highlightSettings = new TutorialHighlightSettings();
        
        // Properties
        public string StepId => _stepId;
        public string Title => _title;
        public string ShortDescription => _shortDescription;
        public string DetailedInstructions => _detailedInstructions;
        public string InstructionText { get; set; } // Dynamic instruction text that can be set at runtime
        public TutorialStepType StepType => _stepType;
        public Sprite IllustrationImage => _illustrationImage;
        public AudioClip NarrationClip => _narrationClip;
        
        public TutorialTargetType TargetType => _targetType;
        public string TargetElementId => _targetElementId;
        public Vector2 HighlightOffset => _highlightOffset;
        public Vector2 HighlightSize => _highlightSize;
        public TutorialHighlightShape HighlightShape => _highlightShape;
        
        public TutorialValidationType ValidationType => _validationType;
        public string ValidationTarget => _validationTarget;
        public float TimeoutDuration => _timeoutDuration;
        public bool IsOptional => _isOptional;
        public bool CanSkip => _canSkip;
        
        public List<TutorialStepSO> PrerequisiteSteps => _prerequisiteSteps;
        public List<string> RequiredUnlocks => _requiredUnlocks;
        public List<TutorialHint> Hints => _hints;
        public float HintDelay => _hintDelay;
        
        public bool PauseGameplay => _pauseGameplay;
        public bool DimBackground => _dimBackground;
        public float BackgroundDimAmount => _backgroundDimAmount;
        public bool AllowInterruption => _allowInterruption;
        public bool BlockInteraction => _blockInteraction;
        public List<string> AllowedInteractionElements => _allowedInteractionElements;
        public TutorialHighlightSettings HighlightSettings => _highlightSettings;
        
        // Runtime configuration methods
        public void SetStepId(string stepId) => _stepId = stepId;
        public void SetTitle(string title) => _title = title;
        public void SetShortDescription(string description) => _shortDescription = description;
        public void SetDetailedInstructions(string instructions) => _detailedInstructions = instructions;
        public void SetStepType(TutorialStepType stepType) => _stepType = stepType;
        public void SetValidationType(TutorialValidationType validationType) => _validationType = validationType;
        public void SetValidationTarget(string validationTarget) => _validationTarget = validationTarget;
        public void SetTimeoutDuration(float duration) => _timeoutDuration = duration;
        public void SetCanSkip(bool canSkip) => _canSkip = canSkip;
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_stepId))
            {
                _stepId = name.Replace(" ", "_").ToLowerInvariant();
            }
            
            if (string.IsNullOrEmpty(_title))
            {
                _title = name;
            }
            
            _timeoutDuration = Mathf.Max(5f, _timeoutDuration);
            _hintDelay = Mathf.Max(1f, _hintDelay);
            _backgroundDimAmount = Mathf.Clamp01(_backgroundDimAmount);
            
            // Validate highlight size
            _highlightSize.x = Mathf.Max(10f, _highlightSize.x);
            _highlightSize.y = Mathf.Max(10f, _highlightSize.y);
            
            // Add validation checks
            if (string.IsNullOrEmpty(_stepId))
            {
                LogError("Step ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_title))
            {
                LogError("Step title cannot be empty");
                isValid = false;
            }
            
            if (_validationType != TutorialValidationType.Timer && string.IsNullOrEmpty(_validationTarget))
            {
                LogWarning("Validation target should be specified for non-timer validations");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get step data for runtime use
        /// </summary>
        public TutorialStepData GetStepData()
        {
            var prerequisiteIds = new List<string>();
            foreach (var prereq in _prerequisiteSteps)
            {
                if (prereq != null)
                    prerequisiteIds.Add(prereq.StepId);
            }
            
            return new TutorialStepData
            {
                StepId = _stepId,
                Title = _title,
                Description = _shortDescription,
                DetailedInstructions = _detailedInstructions,
                StepType = _stepType,
                TargetType = _targetType,
                TargetElementId = _targetElementId,
                HighlightOffset = _highlightOffset,
                HighlightSize = _highlightSize,
                HighlightShape = _highlightShape,
                ValidationType = _validationType,
                ValidationTarget = _validationTarget,
                TimeoutDuration = _timeoutDuration,
                IsOptional = _isOptional,
                CanSkip = _canSkip,
                Prerequisites = prerequisiteIds,
                Hints = new List<TutorialHint>(_hints),
                NarrationClip = _narrationClip,
                IllustrationImage = _illustrationImage
            };
        }
        
        /// <summary>
        /// Check if this step's prerequisites are satisfied
        /// </summary>
        public bool ArePrerequisitesSatisfied(List<string> completedSteps)
        {
            foreach (var prereq in _prerequisiteSteps)
            {
                if (prereq != null && !completedSteps.Contains(prereq.StepId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get localized content for step
        /// </summary>
        public TutorialStepContent GetLocalizedContent(string languageCode = "en")
        {
            // In a full implementation, this would load localized content
            return new TutorialStepContent
            {
                Title = _title,
                Description = _shortDescription,
                DetailedInstructions = _detailedInstructions,
                HintTexts = _hints.ConvertAll(h => h.HintText)
            };
        }
        
        /// <summary>
        /// Create validation context for this step
        /// </summary>
        public TutorialValidationContext CreateValidationContext()
        {
            return new TutorialValidationContext
            {
                StepId = _stepId,
                ValidationType = _validationType,
                ValidationTarget = _validationTarget,
                TimeoutDuration = _timeoutDuration,
                IsOptional = _isOptional
            };
        }
    }
    
    /// <summary>
    /// Localized tutorial step content
    /// </summary>
    [System.Serializable]
    public struct TutorialStepContent
    {
        public string Title;
        public string Description;
        public string DetailedInstructions;
        public List<string> HintTexts;
    }
    
    /// <summary>
    /// Tutorial validation context
    /// </summary>
    [System.Serializable]
    public struct TutorialValidationContext
    {
        public string StepId;
        public TutorialValidationType ValidationType;
        public string ValidationTarget;
        public float TimeoutDuration;
        public bool IsOptional;
    }
}