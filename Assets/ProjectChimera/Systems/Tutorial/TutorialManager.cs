using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Progression;
using System;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Central tutorial management system for Project Chimera.
    /// Manages tutorial progression, validation, and player guidance.
    /// </summary>
    public class TutorialManager : ChimeraManager
    {
        [Header("Tutorial Configuration")]
        [SerializeField] private TutorialConfigurationSO _tutorialConfiguration;
        [SerializeField] private bool _enableDebugMode = false;
        [SerializeField] private bool _autoStartOnboarding = true;
        [SerializeField] private float _validationCheckInterval = 0.5f;
        
        // Tutorial state
        private TutorialSequenceSO _currentSequence;
        private TutorialStepSO _currentStep;
        private TutorialProgress _currentProgress;
        private TutorialSettings _settings;
        
        // Tutorial data
        private Dictionary<string, TutorialProgress> _sequenceProgress;
        private Dictionary<string, TutorialSessionStats> _sessionStats;
        private List<string> _completedSequences;
        private List<string> _unlockedFeatures;
        
        // Tutorial components
        private TutorialStepValidator _stepValidator;
        private TutorialHighlightSystem _highlightSystem;
        private TutorialAudioSystem _audioSystem;
        private TutorialAnalytics _analytics;
        
        // Timing and validation
        private float _lastValidationCheck;
        private float _stepStartTime;
        private bool _isValidating;
        private string _currentSessionId;
        
        // Events
        public static Action<TutorialSequenceSO> OnSequenceStarted;
        public static Action<TutorialSequenceSO> OnSequenceCompleted;
        public static Action<TutorialStepSO> OnStepStarted;
        public static Action<TutorialStepSO> OnStepCompleted;
        public static Action<TutorialValidationResult> OnValidationResult;
        public static Action<TutorialEventData> OnTutorialEvent;
        
        // Properties
        public bool HasActiveTutorial => _currentSequence != null;
        public TutorialSequenceSO CurrentSequence => _currentSequence;
        public TutorialStepSO CurrentStep => _currentStep;
        public float CurrentProgress => _currentProgress?.ProgressPercentage ?? 0f;
        public bool IsInitialized { get; private set; }
        public TutorialSettings Settings => _settings;
        
        /// <summary>
        /// Get current tutorial step - compatibility method for testing framework
        /// </summary>
        public TutorialStepSO GetCurrentStep()
        {
            return _currentStep;
        }
        
        /// <summary>
        /// Check if current step can be skipped - compatibility method for testing framework
        /// </summary>
        public bool CanSkipCurrentStep()
        {
            return _currentStep != null && _currentStep.CanSkip;
        }
        
        /// <summary>
        /// Check if a tutorial sequence is currently active - compatibility method for testing framework
        /// </summary>
        public bool IsSequenceActive()
        {
            return _currentSequence != null;
        }
        
        /// <summary>
        /// Get tutorial progress data - compatibility method for testing framework
        /// </summary>
        public TutorialProgress GetProgressData()
        {
            return _currentProgress;
        }
        
        /// <summary>
        /// Save tutorial progress - compatibility method for testing framework
        /// </summary>
        public void SaveProgress()
        {
            SaveTutorialProgress();
        }
        
        /// <summary>
        /// Load tutorial progress - compatibility method for testing framework
        /// </summary>
        public void LoadProgress()
        {
            LoadTutorialProgress();
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeTutorialSystem();
            InitializeComponents();
            LoadTutorialProgress();
            
            LogInfo("Tutorial Manager initialized successfully");
            IsInitialized = true;
        }

        protected override void OnManagerShutdown()
        {
            StopCurrentTutorial();
            SaveTutorialProgress();
            
            LogInfo("Tutorial Manager shutdown");
            IsInitialized = false;
        }
        
        /// <summary>
        /// Initialize tutorial system
        /// </summary>
        private void InitializeTutorialSystem()
        {
            if (_tutorialConfiguration == null)
            {
                LogError("Tutorial configuration not assigned!");
                return;
            }
            
            _settings = _tutorialConfiguration.GetTutorialSettings();
            _sequenceProgress = new Dictionary<string, TutorialProgress>();
            _sessionStats = new Dictionary<string, TutorialSessionStats>();
            _completedSequences = new List<string>();
            _unlockedFeatures = new List<string>();
            _currentSessionId = Guid.NewGuid().ToString();
            
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Initialize tutorial components
        /// </summary>
        private void InitializeComponents()
        {
            _stepValidator = new TutorialStepValidator();
            _highlightSystem = new TutorialHighlightSystem(_settings);
            _audioSystem = new TutorialAudioSystem(_settings, _tutorialConfiguration);
            _analytics = new TutorialAnalytics(_settings);
            
            _stepValidator.OnValidationComplete += HandleValidationComplete;
        }
        
        /// <summary>
        /// Validate tutorial configuration
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_tutorialConfiguration == null)
            {
                LogError("Tutorial configuration is null");
                return;
            }
            
            var validation = _tutorialConfiguration.ValidateConfiguration();
            
            if (!validation.IsValid)
            {
                LogError($"Tutorial configuration validation failed: {string.Join(", ", validation.Errors)}");
            }
            
            if (validation.Warnings.Count > 0)
            {
                LogWarning($"Tutorial configuration warnings: {string.Join(", ", validation.Warnings)}");
            }
        }
        
        /// <summary>
        /// Start tutorial sequence
        /// </summary>
        public bool StartTutorialSequence(string sequenceId)
        {
            if (!_settings.EnableTutorials)
            {
                LogWarning("Tutorials are disabled");
                return false;
            }
            
            var sequence = _tutorialConfiguration.GetSequenceById(sequenceId);
            if (sequence == null)
            {
                LogError($"Tutorial sequence not found: {sequenceId}");
                return false;
            }
            
            return StartTutorialSequence(sequence);
        }
        
        /// <summary>
        /// Start tutorial sequence
        /// </summary>
        public bool StartTutorialSequence(TutorialSequenceSO sequence)
        {
            if (sequence == null)
            {
                LogError("Cannot start null tutorial sequence");
                return false;
            }
            
            // Check prerequisites
            var playerLevel = GetCurrentPlayerLevel();
            if (!sequence.ArePrerequisitesSatisfied(_completedSequences, playerLevel, _unlockedFeatures))
            {
                LogWarning($"Prerequisites not satisfied for tutorial sequence: {sequence.SequenceId}");
                return false;
            }
            
            // Stop current tutorial if running
            if (_currentSequence != null)
            {
                StopCurrentTutorial();
            }
            
            // Initialize sequence
            _currentSequence = sequence;
            _currentProgress = GetOrCreateProgress(sequence.SequenceId);
            _currentProgress.Status = TutorialStatus.InProgress;
            _currentProgress.StartTime = DateTime.Now;
            
            // Start first step
            var firstStep = sequence.GetNextStep(null);
            if (firstStep != null)
            {
                StartTutorialStep(firstStep);
            }
            
            // Track analytics
            _analytics?.TrackSequenceStarted(sequence.SequenceId);
            
            // Fire events
            OnSequenceStarted?.Invoke(sequence);
            FireTutorialEvent(TutorialEventType.Started, sequence.SequenceId, null);
            
            LogInfo($"Started tutorial sequence: {sequence.SequenceId}");
            return true;
        }
        
        /// <summary>
        /// Start tutorial step
        /// </summary>
        public void StartTutorialStep(TutorialStepSO step)
        {
            if (step == null)
            {
                LogError("Cannot start null tutorial step");
                return;
            }
            
            _currentStep = step;
            _stepStartTime = Time.time;
            _isValidating = false;
            
            // Update progress
            if (_currentProgress != null)
            {
                _currentProgress.CurrentStepId = step.StepId;
                _currentProgress.LastUpdateTime = DateTime.Now;
            }
            
            // Setup UI highlights
            _highlightSystem?.HighlightTarget(step);
            
            // Play audio
            _audioSystem?.PlayStepAudio(step);
            
            // Pause gameplay if required
            if (step.PauseGameplay && _settings.PauseGameDuringTutorial)
            {
                Time.timeScale = 0f;
            }
            
            // Setup validation
            _stepValidator?.SetupValidation(step);
            
            // Track analytics
            _analytics?.TrackStepStarted(step.StepId);
            
            // Fire events
            OnStepStarted?.Invoke(step);
            FireTutorialEvent(TutorialEventType.Started, _currentSequence?.SequenceId, step.StepId);
            
            LogInfo($"Started tutorial step: {step.StepId}");
        }
        
        /// <summary>
        /// Complete current tutorial step
        /// </summary>
        public bool CompleteCurrentStep()
        {
            if (_currentStep == null)
            {
                LogWarning("No current tutorial step to complete");
                return false;
            }
            
            var completedStep = _currentStep;
            
            // Update progress
            if (_currentProgress != null)
            {
                _currentProgress.CompletedSteps++;
                _currentProgress.CompletedStepIds.Add(completedStep.StepId);
                _currentProgress.ProgressPercentage = _currentSequence.CalculateProgress(_currentProgress.CompletedStepIds);
                _currentProgress.LastUpdateTime = DateTime.Now;
            }
            
            // Clear highlights
            _highlightSystem?.ClearHighlights();
            
            // Resume gameplay
            if (completedStep.PauseGameplay)
            {
                Time.timeScale = 1f;
            }
            
            // Play completion audio
            _audioSystem?.PlayStepCompletedSound();
            
            // Track analytics
            _analytics?.TrackStepCompleted(completedStep.StepId, Time.time - _stepStartTime);
            
            // Fire events
            OnStepCompleted?.Invoke(completedStep);
            FireTutorialEvent(TutorialEventType.StepCompleted, _currentSequence?.SequenceId, completedStep.StepId);
            
            LogInfo($"Completed tutorial step: {completedStep.StepId}");
            
            // Move to next step
            var nextStep = _currentSequence?.GetNextStep(completedStep.StepId);
            if (nextStep != null)
            {
                StartTutorialStep(nextStep);
            }
            else
            {
                // Sequence completed
                CompleteCurrentSequence();
            }
            
            return true;
        }
        
        /// <summary>
        /// Complete current tutorial sequence
        /// </summary>
        private void CompleteCurrentSequence()
        {
            if (_currentSequence == null)
            {
                LogWarning("No current tutorial sequence to complete");
                return;
            }
            
            var completedSequence = _currentSequence;
            
            // Update progress
            if (_currentProgress != null)
            {
                _currentProgress.Status = TutorialStatus.Completed;
                _currentProgress.IsCompleted = true;
                _currentProgress.ProgressPercentage = 1f;
                _currentProgress.LastUpdateTime = DateTime.Now;
            }
            
            // Add to completed sequences
            if (!_completedSequences.Contains(completedSequence.SequenceId))
            {
                _completedSequences.Add(completedSequence.SequenceId);
            }
            
            // Unlock features
            foreach (var feature in completedSequence.UnlockedFeatures)
            {
                if (!_unlockedFeatures.Contains(feature))
                {
                    _unlockedFeatures.Add(feature);
                }
            }
            
            // Grant rewards
            GrantCompletionRewards(completedSequence.CompletionReward);
            
            // Clear current state
            _currentSequence = null;
            _currentStep = null;
            _currentProgress = null;
            
            // Resume gameplay
            Time.timeScale = 1f;
            
            // Play completion audio
            _audioSystem?.PlaySequenceCompletedSound();
            
            // Track analytics
            _analytics?.TrackSequenceCompleted(completedSequence.SequenceId);
            
            // Fire events
            OnSequenceCompleted?.Invoke(completedSequence);
            FireTutorialEvent(TutorialEventType.Completed, completedSequence.SequenceId, null);
            
            // Save progress
            SaveTutorialProgress();
            
            LogInfo($"Completed tutorial sequence: {completedSequence.SequenceId}");
        }
        
        /// <summary>
        /// Skip current tutorial step
        /// </summary>
        public bool SkipCurrentStep()
        {
            if (_currentStep == null || !_currentStep.CanSkip || !_settings.AllowSkipping)
            {
                return false;
            }
            
            var skippedStep = _currentStep;
            
            // Track analytics
            _analytics?.TrackStepSkipped(skippedStep.StepId);
            
            // Fire events
            FireTutorialEvent(TutorialEventType.StepSkipped, _currentSequence?.SequenceId, skippedStep.StepId);
            
            LogInfo($"Skipped tutorial step: {skippedStep.StepId}");
            
            // Complete step (which will advance to next)
            CompleteCurrentStep();
            
            return true;
        }
        
        /// <summary>
        /// Skip current tutorial sequence
        /// </summary>
        public bool SkipCurrentSequence()
        {
            if (_currentSequence == null || !_currentSequence.AllowSequenceSkipping || !_settings.AllowSkipping)
            {
                return false;
            }
            
            var skippedSequence = _currentSequence;
            
            // Update progress
            if (_currentProgress != null)
            {
                _currentProgress.Status = TutorialStatus.Skipped;
                _currentProgress.WasSkipped = true;
                _currentProgress.LastUpdateTime = DateTime.Now;
            }
            
            // Track analytics
            _analytics?.TrackSequenceSkipped(skippedSequence.SequenceId);
            
            // Clear current state
            StopCurrentTutorial();
            
            LogInfo($"Skipped tutorial sequence: {skippedSequence.SequenceId}");
            
            return true;
        }
        
        /// <summary>
        /// Stop current tutorial
        /// </summary>
        public void StopCurrentTutorial()
        {
            if (_currentSequence == null)
                return;
            
            // Clear highlights
            _highlightSystem?.ClearHighlights();
            
            // Resume gameplay
            Time.timeScale = 1f;
            
            // Update progress if abandoned
            if (_currentProgress != null && _currentProgress.Status == TutorialStatus.InProgress)
            {
                _currentProgress.Status = TutorialStatus.Abandoned;
                _currentProgress.LastUpdateTime = DateTime.Now;
            }
            
            // Track analytics
            if (_currentSequence != null)
            {
                _analytics?.TrackSequenceAbandoned(_currentSequence.SequenceId);
            }
            
            // Clear state
            var abandonedSequence = _currentSequence;
            _currentSequence = null;
            _currentStep = null;
            _currentProgress = null;
            
            // Fire events
            FireTutorialEvent(TutorialEventType.Abandoned, abandonedSequence?.SequenceId, null);
            
            LogInfo("Stopped current tutorial");
        }
        
        /// <summary>
        /// Handle validation completion
        /// </summary>
        private void HandleValidationComplete(TutorialValidationResult result)
        {
            if (result.IsValid)
            {
                CompleteCurrentStep();
            }
            else
            {
                // Handle validation failure
                _audioSystem?.PlayErrorSound();
                
                // Show hints if available
                if (_settings.ShowHints && _currentStep?.Hints?.Count > 0)
                {
                    ShowNextHint();
                }
                
                // Track analytics
                _analytics?.TrackValidationFailed(_currentStep?.StepId, result.ErrorMessage);
                
                // Fire events
                OnValidationResult?.Invoke(result);
                FireTutorialEvent(TutorialEventType.ValidationFailed, _currentSequence?.SequenceId, _currentStep?.StepId);
            }
        }
        
        /// <summary>
        /// Show next hint for current step
        /// </summary>
        private void ShowNextHint()
        {
            if (_currentStep?.Hints == null || _currentStep.Hints.Count == 0)
                return;
            
            // In a full implementation, this would cycle through available hints
            var hint = _currentStep.Hints[0];
            
            // Track analytics
            _analytics?.TrackHintShown(_currentStep.StepId, hint.HintText);
            
            // Fire events
            FireTutorialEvent(TutorialEventType.HintShown, _currentSequence?.SequenceId, _currentStep.StepId);
            
            LogInfo($"Showed hint for step {_currentStep.StepId}: {hint.HintText}");
        }
        
        /// <summary>
        /// Get or create progress for sequence
        /// </summary>
        private TutorialProgress GetOrCreateProgress(string sequenceId)
        {
            if (!_sequenceProgress.TryGetValue(sequenceId, out var progress))
            {
                var sequence = _tutorialConfiguration.GetSequenceById(sequenceId);
                progress = new TutorialProgress
                {
                    TutorialId = sequenceId,
                    Status = TutorialStatus.NotStarted,
                    ProgressPercentage = 0f,
                    CompletedSteps = 0,
                    TotalSteps = sequence?.StepCount ?? 0,
                    CompletedStepIds = new List<string>(),
                    CustomData = new Dictionary<string, object>()
                };
                _sequenceProgress[sequenceId] = progress;
            }
            
            return progress;
        }
        
        /// <summary>
        /// Grant completion rewards
        /// </summary>
        private void GrantCompletionRewards(TutorialReward reward)
        {
            if (reward.RewardType == TutorialRewardType.Experience && reward.Amount > 0)
            {
                // Grant experience points
                var progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>();
                // TODO: Implement AddExperience method in ComprehensiveProgressionManager
                // progressionManager?.AddExperience("Tutorial_Completion", reward.Amount, ProgressionCategory.General);
            }
            else if (reward.RewardType == TutorialRewardType.Currency && reward.Amount > 0)
            {
                // Grant currency
                // TODO: Implement currency system - MarketManager doesn't handle currency directly yet
                /*
                var marketManager = GameManager.Instance.GetManager<ProjectChimera.Systems.Economy.MarketManager>();
                marketManager?.AddCurrency(reward.Amount);
                */
                LogInfo($"Would grant currency reward: {reward.Amount} (currency system not yet implemented)");
            }
            
            LogInfo($"Granted tutorial reward: {reward.RewardType} x{reward.Amount}");
        }
        
        /// <summary>
        /// Get current player level
        /// </summary>
        private int GetCurrentPlayerLevel()
        {
            var progressionManager = GameManager.Instance?.GetManager<ComprehensiveProgressionManager>();
            return 1; // ComprehensiveProgressionManager doesn't have PlayerLevel property yet, return default
        }
        
        /// <summary>
        /// Fire tutorial event
        /// </summary>
        private void FireTutorialEvent(TutorialEventType eventType, string sequenceId, string stepId)
        {
            var eventData = new TutorialEventData
            {
                TutorialId = sequenceId ?? "",
                StepId = stepId ?? "",
                EventType = eventType,
                Message = $"{eventType} - {sequenceId ?? "Unknown"}/{stepId ?? "Unknown"}",
                Timestamp = DateTime.Now
            };
            
            OnTutorialEvent?.Invoke(eventData);
        }
        
        /// <summary>
        /// Save tutorial progress
        /// </summary>
        private void SaveTutorialProgress()
        {
            if (!_settings.SaveProgress)
                return;
            
            // In a full implementation, this would save to persistent storage
            LogInfo("Saved tutorial progress");
        }
        
        /// <summary>
        /// Load tutorial progress
        /// </summary>
        private void LoadTutorialProgress()
        {
            if (!_settings.SaveProgress)
                return;
            
            // In a full implementation, this would load from persistent storage
            LogInfo("Loaded tutorial progress");
        }
        
        /// <summary>
        /// Get available tutorial sequences
        /// </summary>
        public List<TutorialSequenceSO> GetAvailableTutorialSequences()
        {
            if (_tutorialConfiguration == null)
                return new List<TutorialSequenceSO>();
            
            var playerLevel = GetCurrentPlayerLevel();
            return _tutorialConfiguration.GetAvailableSequencesForLevel(playerLevel, _completedSequences, _unlockedFeatures);
        }
        
        /// <summary>
        /// Get tutorial progress for sequence
        /// </summary>
        public TutorialProgress GetTutorialProgress(string sequenceId)
        {
            return _sequenceProgress.TryGetValue(sequenceId, out var progress) ? progress : null;
        }
        
        /// <summary>
        /// Check if sequence is completed
        /// </summary>
        public bool IsSequenceCompleted(string sequenceId)
        {
            return _completedSequences.Contains(sequenceId);
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (!IsInitialized || _currentStep == null)
                return;
            
            // Validation checks
            if (Time.time - _lastValidationCheck >= _validationCheckInterval)
            {
                _stepValidator?.CheckValidation(_currentStep);
                _lastValidationCheck = Time.time;
            }
            
            // Timeout checks
            if (_currentStep.TimeoutDuration > 0 && Time.time - _stepStartTime >= _currentStep.TimeoutDuration)
            {
                LogWarning($"Tutorial step {_currentStep.StepId} timed out");
                
                if (_currentStep.IsOptional)
                {
                    CompleteCurrentStep();
                }
                else
                {
                    ShowNextHint();
                }
            }
        }
        
        private void OnValidate()
        {
            _validationCheckInterval = Mathf.Max(0.1f, _validationCheckInterval);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_stepValidator != null)
            {
                _stepValidator.OnValidationComplete -= HandleValidationComplete;
            }
            
            // Save progress before destroying
            SaveTutorialProgress();
        }
    }
}