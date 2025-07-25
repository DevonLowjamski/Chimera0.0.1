using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// ScriptableObject definition for tutorial sequences in Project Chimera.
    /// Defines complete tutorial flows with multiple steps and progression logic.
    /// </summary>
    [CreateAssetMenu(fileName = "New Tutorial Sequence", menuName = "Project Chimera/Tutorial/Tutorial Sequence")]
    public class TutorialSequenceSO : ChimeraDataSO
    {
        [Header("Sequence Identification")]
        [SerializeField] private string _sequenceId;
        [SerializeField] private string _sequenceName;
        [SerializeField] private string _sequenceDescription;
        [SerializeField] private TutorialCategory _category = TutorialCategory.Onboarding;
        
        [Header("Configuration")]
        [SerializeField] private int _priority = 0;
        [SerializeField] private bool _isRequired = true;
        [SerializeField] private bool _canRestart = true;
        [SerializeField] private bool _saveProgress = true;
        
        [Header("Steps")]
        [SerializeField] private List<TutorialStepSO> _tutorialSteps = new List<TutorialStepSO>();
        [SerializeField] private bool _allowStepSkipping = true;
        [SerializeField] private bool _allowSequenceSkipping = false;
        
        [Header("Prerequisites")]
        [SerializeField] private List<string> _unlockRequirements = new List<string>();
        [SerializeField] private List<TutorialSequenceSO> _prerequisiteSequences = new List<TutorialSequenceSO>();
        [SerializeField] private int _minimumPlayerLevel = 0;
        
        [Header("Completion Rewards")]
        [SerializeField] private TutorialReward _completionReward;
        [SerializeField] private List<string> _unlockedFeatures = new List<string>();
        [SerializeField] private List<string> _achievementIds = new List<string>();
        
        [Header("Visual & Audio")]
        [SerializeField] private Sprite _sequenceIcon;
        [SerializeField] private AudioClip _introductionClip;
        [SerializeField] private AudioClip _completionClip;
        
        [Header("Advanced Settings")]
        [SerializeField] private TutorialSpeed _defaultSpeed = TutorialSpeed.Normal;
        [SerializeField] private TutorialDifficultyLevel _difficultyLevel = TutorialDifficultyLevel.Beginner;
        [SerializeField] private bool _resetOnFailure = false;
        [SerializeField] private int _maxRetries = 3;
        [SerializeField] private float _sequenceTimeout = 600f; // 10 minutes
        
        // Properties
        public string SequenceId => _sequenceId;
        public string SequenceName => _sequenceName;
        public string SequenceDescription => _sequenceDescription;
        public TutorialCategory Category => _category;
        public int Priority => _priority;
        public bool IsRequired => _isRequired;
        public bool CanRestart => _canRestart;
        public bool SaveProgress => _saveProgress;
        
        public List<TutorialStepSO> TutorialSteps => _tutorialSteps;
        public List<TutorialStepSO> Steps => _tutorialSteps; // Compatibility alias for testing framework
        public bool AllowStepSkipping => _allowStepSkipping;
        public bool AllowSequenceSkipping => _allowSequenceSkipping;
        public int StepCount => _tutorialSteps.Count;
        
        /// <summary>
        /// Calculate estimated duration based on steps - compatibility property for testing framework
        /// </summary>
        public float EstimatedDuration
        {
            get
            {
                float totalDuration = 0f;
                foreach (var step in _tutorialSteps)
                {
                    if (step != null)
                    {
                        // Use TimeoutDuration as an approximation for estimated step duration
                        totalDuration += step.TimeoutDuration;
                    }
                }
                return totalDuration;
            }
        }
        
        public List<string> UnlockRequirements => _unlockRequirements;
        public List<TutorialSequenceSO> PrerequisiteSequences => _prerequisiteSequences;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        
        public TutorialReward CompletionReward => _completionReward;
        public List<string> UnlockedFeatures => _unlockedFeatures;
        public List<string> AchievementIds => _achievementIds;
        
        public Sprite SequenceIcon => _sequenceIcon;
        public AudioClip IntroductionClip => _introductionClip;
        public AudioClip CompletionClip => _completionClip;
        
        public TutorialSpeed DefaultSpeed => _defaultSpeed;
        public TutorialDifficultyLevel DifficultyLevel => _difficultyLevel;
        public bool ResetOnFailure => _resetOnFailure;
        public int MaxRetries => _maxRetries;
        public float SequenceTimeout => _sequenceTimeout;
        
        #region Runtime Setters
        /// <summary>
        /// Runtime setter methods for TutorialDataAssetManager
        /// </summary>
        public void SetSequenceId(string sequenceId) => _sequenceId = sequenceId;
        public void SetSequenceName(string sequenceName) => _sequenceName = sequenceName;
        public void SetDescription(string description) => _sequenceDescription = description;
        public void SetDifficultyLevel(TutorialDifficultyLevel difficultyLevel) => _difficultyLevel = difficultyLevel;
        public void SetIsRequired(bool isRequired) => _isRequired = isRequired;
        public void SetSteps(List<TutorialStepSO> steps) => _tutorialSteps = steps;
        public void SetStepCount(int stepCount) { /* Read-only, calculated from steps list */ }
        public void SetEstimatedDuration(float duration) { /* Read-only, calculated from steps */ }
        public void SetCategory(TutorialCategory category) => _category = category;
        public void SetPriority(int priority) => _priority = priority;
        #endregion
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_sequenceId))
            {
                _sequenceId = name.Replace(" ", "_").ToLowerInvariant();
            }
            
            if (string.IsNullOrEmpty(_sequenceName))
            {
                _sequenceName = name;
            }
            
            _priority = Mathf.Max(0, _priority);
            _minimumPlayerLevel = Mathf.Max(0, _minimumPlayerLevel);
            _maxRetries = Mathf.Max(1, _maxRetries);
            _sequenceTimeout = Mathf.Max(60f, _sequenceTimeout);
            
            // Remove null steps
            _tutorialSteps.RemoveAll(step => step == null);
            
            // Validate completion reward
            if (_completionReward.RewardType == TutorialRewardType.Currency ||
                _completionReward.RewardType == TutorialRewardType.Experience)
            {
                _completionReward.Amount = Mathf.Max(0, _completionReward.Amount);
            }
            
            // Add validation checks
            if (string.IsNullOrEmpty(_sequenceId))
            {
                LogError("Sequence ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_sequenceName))
            {
                LogError("Sequence name cannot be empty");
                isValid = false;
            }
            
            if (_tutorialSteps.Count == 0)
            {
                LogError("Sequence must contain at least one tutorial step");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get sequence data for runtime use
        /// </summary>
        public TutorialSequenceData GetSequenceData()
        {
            var stepIds = _tutorialSteps.Where(step => step != null).Select(step => step.StepId).ToList();
            
            return new TutorialSequenceData
            {
                SequenceId = _sequenceId,
                Name = _sequenceName,
                Description = _sequenceDescription,
                Category = _category,
                Priority = _priority,
                IsRequired = _isRequired,
                StepIds = stepIds,
                UnlockRequirements = new List<string>(_unlockRequirements),
                CompletionReward = _completionReward
            };
        }
        
        /// <summary>
        /// Check if sequence prerequisites are satisfied
        /// </summary>
        public bool ArePrerequisitesSatisfied(List<string> completedSequences, int playerLevel, List<string> unlockedFeatures)
        {
            // Check player level
            if (playerLevel < _minimumPlayerLevel)
                return false;
            
            // Check prerequisite sequences
            foreach (var prereq in _prerequisiteSequences)
            {
                if (prereq != null && !completedSequences.Contains(prereq.SequenceId))
                    return false;
            }
            
            // Check unlock requirements
            foreach (var requirement in _unlockRequirements)
            {
                if (!unlockedFeatures.Contains(requirement))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get next step in sequence
        /// </summary>
        public TutorialStepSO GetNextStep(string currentStepId)
        {
            if (string.IsNullOrEmpty(currentStepId))
                return _tutorialSteps.FirstOrDefault();
            
            var currentIndex = _tutorialSteps.FindIndex(step => step != null && step.StepId == currentStepId);
            if (currentIndex >= 0 && currentIndex < _tutorialSteps.Count - 1)
            {
                return _tutorialSteps[currentIndex + 1];
            }
            
            return null; // Sequence completed
        }
        
        /// <summary>
        /// Get previous step in sequence
        /// </summary>
        public TutorialStepSO GetPreviousStep(string currentStepId)
        {
            if (string.IsNullOrEmpty(currentStepId))
                return null;
            
            var currentIndex = _tutorialSteps.FindIndex(step => step != null && step.StepId == currentStepId);
            if (currentIndex > 0)
            {
                return _tutorialSteps[currentIndex - 1];
            }
            
            return null;
        }
        
        /// <summary>
        /// Get step by ID
        /// </summary>
        public TutorialStepSO GetStepById(string stepId)
        {
            return _tutorialSteps.FirstOrDefault(step => step != null && step.StepId == stepId);
        }
        
        /// <summary>
        /// Calculate sequence progress
        /// </summary>
        public float CalculateProgress(List<string> completedStepIds)
        {
            if (_tutorialSteps.Count == 0)
                return 1f;
            
            var completedCount = _tutorialSteps.Count(step => 
                step != null && completedStepIds.Contains(step.StepId));
            
            return (float)completedCount / _tutorialSteps.Count;
        }
        
        /// <summary>
        /// Get sequence statistics
        /// </summary>
        public TutorialSequenceStats GetSequenceStats()
        {
            var requiredSteps = _tutorialSteps.Count(step => step != null && !step.IsOptional);
            var optionalSteps = _tutorialSteps.Count(step => step != null && step.IsOptional);
            var estimatedDuration = _tutorialSteps.Sum(step => step != null ? step.TimeoutDuration * 0.5f : 0f);
            
            return new TutorialSequenceStats
            {
                SequenceId = _sequenceId,
                TotalSteps = _tutorialSteps.Count,
                RequiredSteps = requiredSteps,
                OptionalSteps = optionalSteps,
                EstimatedDuration = estimatedDuration,
                Category = _category,
                Priority = _priority,
                IsRequired = _isRequired
            };
        }
        
        /// <summary>
        /// Validate sequence integrity
        /// </summary>
        public TutorialSequenceValidation ValidateSequence()
        {
            var validation = new TutorialSequenceValidation
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };
            
            // Check for null steps
            if (_tutorialSteps.Any(step => step == null))
            {
                validation.Warnings.Add("Sequence contains null tutorial steps");
            }
            
            // Check for empty sequence
            if (_tutorialSteps.Count == 0)
            {
                validation.IsValid = false;
                validation.Errors.Add("Sequence contains no tutorial steps");
            }
            
            // Check for duplicate step IDs
            var stepIds = _tutorialSteps.Where(step => step != null).Select(step => step.StepId);
            var duplicateIds = stepIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
            
            foreach (var duplicateId in duplicateIds)
            {
                validation.IsValid = false;
                validation.Errors.Add($"Duplicate step ID found: {duplicateId}");
            }
            
            // Check prerequisite chains
            foreach (var step in _tutorialSteps.Where(step => step != null))
            {
                foreach (var prereq in step.PrerequisiteSteps)
                {
                    if (prereq != null && !_tutorialSteps.Contains(prereq))
                    {
                        validation.Warnings.Add($"Step {step.StepId} has prerequisite {prereq.StepId} not in this sequence");
                    }
                }
            }
            
            return validation;
        }
    }
    
    /// <summary>
    /// Tutorial sequence statistics
    /// </summary>
    public struct TutorialSequenceStats
    {
        public string SequenceId;
        public int TotalSteps;
        public int RequiredSteps;
        public int OptionalSteps;
        public float EstimatedDuration;
        public TutorialCategory Category;
        public int Priority;
        public bool IsRequired;
    }
    
    /// <summary>
    /// Tutorial sequence validation result
    /// </summary>
    public struct TutorialSequenceValidation
    {
        public bool IsValid;
        public List<string> Errors;
        public List<string> Warnings;
    }
}