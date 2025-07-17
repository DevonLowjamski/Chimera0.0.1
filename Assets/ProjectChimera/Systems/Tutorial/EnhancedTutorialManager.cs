using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Enhanced tutorial system for Project Chimera providing comprehensive
    /// new player guidance through interactive tutorials, progressive learning,
    /// contextual hints, and adaptive difficulty based on player performance.
    /// </summary>
    public class EnhancedTutorialManager : ChimeraManager
    {
        [Header("Tutorial Configuration")]
        [SerializeField] private bool _enableTutorials = true;
        [SerializeField] private bool _enableAdaptiveDifficulty = true;
        [SerializeField] private bool _enableContextualHints = true;
        [SerializeField] private bool _enableProgressTracking = true;
        [SerializeField] private float _hintDelaySeconds = 30f;
        [SerializeField] private int _maxHintsPerSession = 5;
        
        [Header("Learning Analytics")]
        [SerializeField] private bool _enableLearningAnalytics = true;
        [SerializeField] private bool _trackPlayerPerformance = true;
        [SerializeField] private bool _enablePersonalization = true;
        [SerializeField] private float _difficultyAdjustmentThreshold = 0.7f;
        
        [Header("UI Integration")]
        [SerializeField] private bool _enableHighlighting = true;
        [SerializeField] private bool _enableAnimations = true;
        [SerializeField] private bool _enableVoiceOver = false;
        [SerializeField] private float _highlightFadeTime = 0.5f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onTutorialStarted;
        [SerializeField] private SimpleGameEventSO _onTutorialCompleted;
        [SerializeField] private SimpleGameEventSO _onStepCompleted;
        [SerializeField] private SimpleGameEventSO _onHintShown;
        [SerializeField] private SimpleGameEventSO _onPlayerStuck;
        
        // Core Tutorial Data
        private Dictionary<string, TutorialSequence> _availableSequences = new Dictionary<string, TutorialSequence>();
        private TutorialSequence _currentSequence;
        private TutorialStep _currentStep;
        private PlayerTutorialProgress _playerProgress = new PlayerTutorialProgress();
        
        // Learning Analytics
        private LearningAnalytics _learningAnalytics = new LearningAnalytics();
        private List<PlayerAction> _recentActions = new List<PlayerAction>();
        private Dictionary<string, float> _stepDifficultyRatings = new Dictionary<string, float>();
        
        // Tutorial State
        private bool _isTutorialActive = false;
        private float _stepStartTime = 0f;
        private int _hintsShownThisSession = 0;
        private float _lastHintTime = 0f;
        private Queue<ContextualHint> _pendingHints = new Queue<ContextualHint>();
        
        // Performance Tracking
        private TutorialMetrics _tutorialMetrics = new TutorialMetrics();
        private Dictionary<string, StepPerformanceData> _stepPerformance = new Dictionary<string, StepPerformanceData>();
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        // Public Properties
        public bool IsTutorialActive => _isTutorialActive;
        public TutorialSequence CurrentSequence => _currentSequence;
        public TutorialStep CurrentStep => _currentStep;
        public PlayerTutorialProgress PlayerProgress => _playerProgress;
        public LearningAnalytics Analytics => _learningAnalytics;
        public TutorialMetrics Metrics => _tutorialMetrics;
        public float OverallProgress => CalculateOverallProgress();
        
        // Events
        public System.Action<TutorialSequence> OnTutorialStarted;
        public System.Action<TutorialSequence> OnTutorialCompleted;
        public System.Action<TutorialStep> OnStepStarted;
        public System.Action<TutorialStep> OnStepCompleted;
        public System.Action<ContextualHint> OnHintShown;
        public System.Action<string> OnPlayerStuck; // area where player is stuck
        
        protected override void OnManagerInitialize()
        {
            InitializeTutorialSequences();
            InitializeLearningAnalytics();
            LoadPlayerProgress();
            InitializeAdaptiveDifficulty();
            
            LogInfo("EnhancedTutorialManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableTutorials) return;
            
            // Update active tutorial
            if (_isTutorialActive && _currentStep != null)
            {
                UpdateCurrentStep();
                CheckStepCompletion();
                ProcessContextualHints();
                DetectPlayerStuck();
            }
            
            // Process learning analytics
            if (_enableLearningAnalytics)
            {
                ProcessLearningAnalytics();
            }
            
            // Update tutorial metrics
            UpdateTutorialMetrics();
        }
        
        #region Tutorial Management
        
        /// <summary>
        /// Start a tutorial sequence
        /// </summary>
        public bool StartTutorial(string sequenceId, bool forceRestart = false)
        {
            if (!_enableTutorials) return false;
            
            if (!_availableSequences.TryGetValue(sequenceId, out var sequence))
            {
                LogWarning($"Tutorial sequence not found: {sequenceId}");
                return false;
            }
            
            // Check if already completed (unless forcing restart)
            if (!forceRestart && _playerProgress.CompletedSequences.Contains(sequenceId))
            {
                LogInfo($"Tutorial already completed: {sequence.Name}");
                return false;
            }
            
            // Check prerequisites
            if (!CheckTutorialPrerequisites(sequence))
            {
                LogWarning($"Tutorial prerequisites not met: {sequence.Name}");
                return false;
            }
            
            _currentSequence = sequence;
            _isTutorialActive = true;
            _hintsShownThisSession = 0;
            
            // Start first step
            if (sequence.Steps.Count > 0)
            {
                StartTutorialStep(sequence.Steps[0]);
            }
            
            // Record tutorial start
            RecordTutorialEvent(TutorialEventType.TutorialStarted, sequenceId);
            
            _onTutorialStarted?.Raise();
            OnTutorialStarted?.Invoke(sequence);
            
            LogInfo($"Started tutorial: {sequence.Name}");
            return true;
        }
        
        /// <summary>
        /// Complete current tutorial step
        /// </summary>
        public bool CompleteCurrentStep()
        {
            if (!_isTutorialActive || _currentStep == null) return false;
            
            float completionTime = Time.time - _stepStartTime;
            
            // Record step completion
            RecordStepCompletion(_currentStep, completionTime);
            
            _onStepCompleted?.Raise();
            OnStepCompleted?.Invoke(_currentStep);
            
            // Move to next step or complete tutorial
            var nextStep = GetNextStep(_currentStep);
            if (nextStep != null)
            {
                StartTutorialStep(nextStep);
            }
            else
            {
                CompleteTutorial();
            }
            
            LogInfo($"Completed tutorial step: {_currentStep.Name}");
            return true;
        }
        
        /// <summary>
        /// Skip current tutorial
        /// </summary>
        public void SkipTutorial()
        {
            if (!_isTutorialActive) return;
            
            string sequenceId = _currentSequence?.SequenceId;
            
            _isTutorialActive = false;
            _currentSequence = null;
            _currentStep = null;
            
            // Record skip event
            if (!string.IsNullOrEmpty(sequenceId))
            {
                RecordTutorialEvent(TutorialEventType.TutorialSkipped, sequenceId);
            }
            
            LogInfo("Tutorial skipped by player");
        }
        
        /// <summary>
        /// Show hint for current step
        /// </summary>
        public bool ShowHint()
        {
            if (!_isTutorialActive || _currentStep == null) return false;
            if (_hintsShownThisSession >= _maxHintsPerSession) return false;
            
            var hint = GetContextualHint(_currentStep);
            if (hint == null) return false;
            
            DisplayHint(hint);
            _hintsShownThisSession++;
            _lastHintTime = Time.time;
            
            // Record hint usage
            RecordTutorialEvent(TutorialEventType.HintShown, _currentStep.StepId);
            
            _onHintShown?.Raise();
            OnHintShown?.Invoke(hint);
            
            LogInfo($"Showed hint: {hint.Title}");
            return true;
        }
        
        /// <summary>
        /// Check if player is eligible for specific tutorial
        /// </summary>
        public bool IsEligibleForTutorial(string sequenceId)
        {
            if (!_availableSequences.TryGetValue(sequenceId, out var sequence))
                return false;
            
            // Check if already completed
            if (_playerProgress.CompletedSequences.Contains(sequenceId))
                return false;
            
            // Check prerequisites
            return CheckTutorialPrerequisites(sequence);
        }
        
        /// <summary>
        /// Get recommended next tutorial
        /// </summary>
        public TutorialSequence GetRecommendedTutorial()
        {
            var availableSequences = _availableSequences.Values
                .Where(s => IsEligibleForTutorial(s.SequenceId))
                .OrderBy(s => s.Priority)
                .ThenBy(s => s.EstimatedDuration);
            
            // Use learning analytics to recommend best tutorial
            if (_enablePersonalization)
            {
                return GetPersonalizedRecommendation(availableSequences.ToList());
            }
            
            return availableSequences.FirstOrDefault();
        }
        
        #endregion
        
        #region Step Management
        
        private void StartTutorialStep(TutorialStep step)
        {
            _currentStep = step;
            _stepStartTime = Time.time;
            
            // Adjust difficulty if enabled
            if (_enableAdaptiveDifficulty)
            {
                AdjustStepDifficulty(step);
            }
            
            // Set up step conditions
            SetupStepConditions(step);
            
            // Show step UI
            DisplayStepInstructions(step);
            
            // Record step start
            RecordTutorialEvent(TutorialEventType.StepStarted, step.StepId);
            
            OnStepStarted?.Invoke(step);
            
            LogInfo($"Started tutorial step: {step.Name}");
        }
        
        private void UpdateCurrentStep()
        {
            if (_currentStep == null) return;
            
            // Update step progress
            _currentStep.Progress = CalculateStepProgress(_currentStep);
            
            // Check for automatic completion
            if (_currentStep.AutoComplete && _currentStep.Progress >= 1.0f)
            {
                CompleteCurrentStep();
            }
        }
        
        private void CheckStepCompletion()
        {
            if (_currentStep == null) return;
            
            // Check completion conditions
            foreach (var condition in _currentStep.CompletionConditions)
            {
                if (!EvaluateCondition(condition))
                    return;
            }
            
            // All conditions met - complete step
            if (!_currentStep.AutoComplete)
            {
                CompleteCurrentStep();
            }
        }
        
        private float CalculateStepProgress(TutorialStep step)
        {
            if (step.CompletionConditions.Count == 0) return 0f;
            
            int completedConditions = 0;
            foreach (var condition in step.CompletionConditions)
            {
                if (EvaluateCondition(condition))
                    completedConditions++;
            }
            
            return (float)completedConditions / step.CompletionConditions.Count;
        }
        
        private bool EvaluateCondition(TutorialCondition condition)
        {
            // Simplified condition evaluation
            // In real implementation, this would check game state
            return UnityEngine.Random.value > 0.3f; // Simulated condition check
        }
        
        #endregion
        
        #region Contextual Hints
        
        private void ProcessContextualHints()
        {
            if (!_enableContextualHints) return;
            if (Time.time - _lastHintTime < _hintDelaySeconds) return;
            if (_hintsShownThisSession >= _maxHintsPerSession) return;
            
            // Check if player might need a hint
            if (ShouldShowContextualHint())
            {
                ShowHint();
            }
        }
        
        private bool ShouldShowContextualHint()
        {
            if (_currentStep == null) return false;
            
            float timeOnStep = Time.time - _stepStartTime;
            float expectedTime = _currentStep.EstimatedDuration * 1.5f; // 50% longer than expected
            
            return timeOnStep > expectedTime && _currentStep.Progress < 0.5f;
        }
        
        private ContextualHint GetContextualHint(TutorialStep step)
        {
            if (step.ContextualHints.Count == 0) return null;
            
            // Get appropriate hint based on current progress
            var availableHints = step.ContextualHints.Where(h => 
                h.MinProgress <= step.Progress && h.MaxProgress >= step.Progress).ToList();
            
            if (availableHints.Count == 0)
                availableHints = step.ContextualHints;
            
            return availableHints[UnityEngine.Random.Range(0, availableHints.Count)];
        }
        
        private void DisplayHint(ContextualHint hint)
        {
            // In real implementation, this would show the hint in UI
            LogInfo($"Displaying hint: {hint.Title} - {hint.Content}");
        }
        
        #endregion
        
        #region Learning Analytics
        
        private void ProcessLearningAnalytics()
        {
            // Analyze player behavior patterns
            AnalyzePlayerBehavior();
            
            // Update difficulty ratings
            UpdateDifficultyRatings();
            
            // Detect learning preferences
            DetectLearningPreferences();
            
            // Update performance metrics
            UpdatePerformanceMetrics();
        }
        
        private void AnalyzePlayerBehavior()
        {
            // Analyze recent actions for patterns
            var recentActions = _recentActions.TakeLast(50).ToList();
            
            _learningAnalytics.AverageActionTime = recentActions.Average(a => a.Duration);
            _learningAnalytics.ErrorRate = recentActions.Count(a => a.IsError) / (float)recentActions.Count;
            _learningAnalytics.HintUsageRate = (float)_hintsShownThisSession / _maxHintsPerSession;
            
            // Update learning style analysis
            AnalyzeLearningStyle(recentActions);
        }
        
        private void AnalyzeLearningStyle(List<PlayerAction> actions)
        {
            // Simplified learning style analysis
            float visualActions = actions.Count(a => a.ActionType == "Visual") / (float)actions.Count;
            float auditoryActions = actions.Count(a => a.ActionType == "Auditory") / (float)actions.Count;
            float kinestheticActions = actions.Count(a => a.ActionType == "Kinesthetic") / (float)actions.Count;
            
            if (visualActions > 0.5f)
                _learningAnalytics.PreferredLearningStyle = LearningStyle.Visual;
            else if (auditoryActions > 0.4f)
                _learningAnalytics.PreferredLearningStyle = LearningStyle.Auditory;
            else
                _learningAnalytics.PreferredLearningStyle = LearningStyle.Kinesthetic;
        }
        
        private void DetectPlayerStuck()
        {
            if (_currentStep == null) return;
            
            float timeOnStep = Time.time - _stepStartTime;
            float progressRate = _currentStep.Progress / timeOnStep;
            
            // Player might be stuck if they're taking much longer than expected
            if (timeOnStep > _currentStep.EstimatedDuration * 3f && progressRate < 0.1f)
            {
                _onPlayerStuck?.Raise();
                OnPlayerStuck?.Invoke(_currentStep.Name);
                
                // Automatically show hint or offer assistance
                if (_enableContextualHints)
                {
                    ShowHint();
                }
            }
        }
        
        #endregion
        
        #region Adaptive Difficulty
        
        private void AdjustStepDifficulty(TutorialStep step)
        {
            if (!_enableAdaptiveDifficulty) return;
            
            // Get player's performance on similar steps
            var stepCategory = step.Category;
            var similarSteps = _stepPerformance.Values
                .Where(p => p.Category == stepCategory)
                .ToList();
            
            if (similarSteps.Count == 0) return;
            
            float averagePerformance = similarSteps.Average(p => p.SuccessRate);
            
            // Adjust difficulty based on performance
            if (averagePerformance < _difficultyAdjustmentThreshold)
            {
                // Make step easier
                step.EstimatedDuration *= 1.5f;
                step.MaxHints += 2;
                LogInfo($"Reduced difficulty for step: {step.Name}");
            }
            else if (averagePerformance > 0.9f)
            {
                // Make step more challenging
                step.EstimatedDuration *= 0.8f;
                step.MaxHints = Mathf.Max(1, step.MaxHints - 1);
                LogInfo($"Increased difficulty for step: {step.Name}");
            }
        }
        
        private void UpdateDifficultyRatings()
        {
            foreach (var step in _stepPerformance)
            {
                var performance = step.Value;
                float difficultyRating = 1.0f - performance.SuccessRate;
                difficultyRating += performance.AverageCompletionTime / performance.ExpectedTime;
                difficultyRating += performance.HintsUsed / 5f; // Normalize to 0-1
                
                _stepDifficultyRatings[step.Key] = Mathf.Clamp01(difficultyRating);
            }
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeTutorialSequences()
        {
            // Create sample tutorial sequences
            CreateBasicTutorialSequence();
            CreateCultivationTutorialSequence();
            CreateGeneticsTutorialSequence();
            CreateEconomicsTutorialSequence();
            CreateAdvancedTutorialSequence();
        }
        
        private void CreateBasicTutorialSequence()
        {
            var sequence = new TutorialSequence
            {
                SequenceId = "basic_tutorial",
                Name = "Getting Started",
                Description = "Learn the basics of Project Chimera",
                Category = TutorialCategory.Basic,
                Priority = 1,
                EstimatedDuration = 300f, // 5 minutes
                Prerequisites = new List<string>(),
                Steps = new List<TutorialStep>()
            };
            
            // Add tutorial steps
            sequence.Steps.Add(CreateTutorialStep(
                "welcome",
                "Welcome to Project Chimera",
                "Welcome to the world of cannabis cultivation simulation!",
                60f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "interface_overview",
                "Interface Overview",
                "Let's explore the main interface and controls",
                120f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "first_plant",
                "Your First Plant",
                "Plant your first cannabis seed and start growing",
                120f
            ));
            
            _availableSequences[sequence.SequenceId] = sequence;
        }
        
        private void CreateCultivationTutorialSequence()
        {
            var sequence = new TutorialSequence
            {
                SequenceId = "cultivation_tutorial",
                Name = "Cultivation Basics",
                Description = "Learn fundamental cultivation techniques",
                Category = TutorialCategory.Cultivation,
                Priority = 2,
                EstimatedDuration = 600f, // 10 minutes
                Prerequisites = new List<string> { "basic_tutorial" },
                Steps = new List<TutorialStep>()
            };
            
            sequence.Steps.Add(CreateTutorialStep(
                "environmental_controls",
                "Environmental Controls",
                "Learn to manage temperature, humidity, and lighting",
                180f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "watering_nutrients",
                "Watering and Nutrients",
                "Understand plant nutrition and watering schedules",
                180f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "harvest_basics",
                "Harvest Basics",
                "Learn when and how to harvest your plants",
                240f
            ));
            
            _availableSequences[sequence.SequenceId] = sequence;
        }
        
        private void CreateGeneticsTutorialSequence()
        {
            var sequence = new TutorialSequence
            {
                SequenceId = "genetics_tutorial",
                Name = "Plant Genetics",
                Description = "Understand breeding and genetic inheritance",
                Category = TutorialCategory.Genetics,
                Priority = 3,
                EstimatedDuration = 900f, // 15 minutes
                Prerequisites = new List<string> { "cultivation_tutorial" },
                Steps = new List<TutorialStep>()
            };
            
            sequence.Steps.Add(CreateTutorialStep(
                "strain_selection",
                "Strain Selection",
                "Choose the right strains for your goals",
                300f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "breeding_basics",
                "Breeding Basics",
                "Learn how to cross different strains",
                300f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "trait_inheritance",
                "Trait Inheritance",
                "Understand how traits are passed to offspring",
                300f
            ));
            
            _availableSequences[sequence.SequenceId] = sequence;
        }
        
        private void CreateEconomicsTutorialSequence()
        {
            var sequence = new TutorialSequence
            {
                SequenceId = "economics_tutorial",
                Name = "Business Management",
                Description = "Learn to manage your cannabis business",
                Category = TutorialCategory.Economics,
                Priority = 4,
                EstimatedDuration = 720f, // 12 minutes
                Prerequisites = new List<string> { "cultivation_tutorial" },
                Steps = new List<TutorialStep>()
            };
            
            sequence.Steps.Add(CreateTutorialStep(
                "market_analysis",
                "Market Analysis",
                "Understand market prices and demand",
                240f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "financial_planning",
                "Financial Planning",
                "Create budgets and manage cash flow",
                240f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "scaling_operations",
                "Scaling Operations",
                "Learn to expand your cultivation business",
                240f
            ));
            
            _availableSequences[sequence.SequenceId] = sequence;
        }
        
        private void CreateAdvancedTutorialSequence()
        {
            var sequence = new TutorialSequence
            {
                SequenceId = "advanced_tutorial",
                Name = "Advanced Techniques",
                Description = "Master advanced cultivation and business strategies",
                Category = TutorialCategory.Advanced,
                Priority = 5,
                EstimatedDuration = 1200f, // 20 minutes
                Prerequisites = new List<string> { "genetics_tutorial", "economics_tutorial" },
                Steps = new List<TutorialStep>()
            };
            
            sequence.Steps.Add(CreateTutorialStep(
                "automation_systems",
                "Automation Systems",
                "Set up automated cultivation systems",
                400f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "quality_optimization",
                "Quality Optimization",
                "Maximize product quality and potency",
                400f
            ));
            
            sequence.Steps.Add(CreateTutorialStep(
                "competitive_strategy",
                "Competitive Strategy",
                "Develop strategies for market competition",
                400f
            ));
            
            _availableSequences[sequence.SequenceId] = sequence;
        }
        
        private TutorialStep CreateTutorialStep(string stepId, string name, string description, float duration)
        {
            return new TutorialStep
            {
                StepId = stepId,
                Name = name,
                Description = description,
                EstimatedDuration = duration,
                CompletionConditions = new List<TutorialCondition>(),
                ContextualHints = new List<ContextualHint>(),
                AutoComplete = false,
                MaxHints = 3,
                Progress = 0f
            };
        }
        
        private void InitializeLearningAnalytics()
        {
            _learningAnalytics = new LearningAnalytics
            {
                SessionStartTime = DateTime.Now,
                TotalSessionTime = 0f,
                AverageActionTime = 0f,
                ErrorRate = 0f,
                HintUsageRate = 0f,
                PreferredLearningStyle = LearningStyle.Visual,
                CompletedTutorials = new List<string>()
            };
        }
        
        private void LoadPlayerProgress()
        {
            // In real implementation, load from save data
            _playerProgress = new PlayerTutorialProgress
            {
                PlayerId = "player_001",
                CompletedSequences = new List<string>(),
                CompletedSteps = new List<string>(),
                TotalTutorialTime = 0f,
                SkippedTutorials = new List<string>(),
                PreferredDifficulty = TutorialDifficulty.Normal
            };
        }
        
        private void InitializeAdaptiveDifficulty()
        {
            // Initialize difficulty system
        }
        
        private bool CheckTutorialPrerequisites(TutorialSequence sequence)
        {
            return sequence.Prerequisites.All(prereq => 
                _playerProgress.CompletedSequences.Contains(prereq));
        }
        
        private TutorialStep GetNextStep(TutorialStep currentStep)
        {
            if (_currentSequence == null) return null;
            
            int currentIndex = _currentSequence.Steps.IndexOf(currentStep);
            if (currentIndex >= 0 && currentIndex < _currentSequence.Steps.Count - 1)
            {
                return _currentSequence.Steps[currentIndex + 1];
            }
            
            return null; // No more steps
        }
        
        private void CompleteTutorial()
        {
            if (_currentSequence == null) return;
            
            string sequenceId = _currentSequence.SequenceId;
            
            // Record completion
            _playerProgress.CompletedSequences.Add(sequenceId);
            RecordTutorialEvent(TutorialEventType.TutorialCompleted, sequenceId);
            
            _isTutorialActive = false;
            _currentSequence = null;
            _currentStep = null;
            
            _onTutorialCompleted?.Raise();
            OnTutorialCompleted?.Invoke(_currentSequence);
            
            LogInfo($"Tutorial completed: {_currentSequence.Name}");
        }
        
        private void SetupStepConditions(TutorialStep step)
        {
            // Set up conditions for step completion
        }
        
        private void DisplayStepInstructions(TutorialStep step)
        {
            // Display step instructions in UI
        }
        
        private void RecordTutorialEvent(TutorialEventType eventType, string targetId)
        {
            // Record event for analytics
        }
        
        private void RecordStepCompletion(TutorialStep step, float completionTime)
        {
            string stepId = step.StepId;
            
            if (!_stepPerformance.ContainsKey(stepId))
            {
                _stepPerformance[stepId] = new StepPerformanceData
                {
                    StepId = stepId,
                    Category = step.Category,
                    ExpectedTime = step.EstimatedDuration
                };
            }
            
            var performance = _stepPerformance[stepId];
            performance.CompletionCount++;
            performance.TotalCompletionTime += completionTime;
            performance.AverageCompletionTime = performance.TotalCompletionTime / performance.CompletionCount;
            performance.SuccessRate = 1.0f; // Simplified - assume success if completed
            performance.HintsUsed = _hintsShownThisSession;
            
            _playerProgress.CompletedSteps.Add(stepId);
        }
        
        private float CalculateOverallProgress()
        {
            if (_availableSequences.Count == 0) return 0f;
            
            return (float)_playerProgress.CompletedSequences.Count / _availableSequences.Count;
        }
        
        private TutorialSequence GetPersonalizedRecommendation(List<TutorialSequence> availableSequences)
        {
            // Use learning analytics to provide personalized recommendations
            var learningStyle = _learningAnalytics.PreferredLearningStyle;
            var errorRate = _learningAnalytics.ErrorRate;
            
            // Recommend easier tutorials if error rate is high
            if (errorRate > 0.3f)
            {
                return availableSequences
                    .Where(s => s.Category == TutorialCategory.Basic)
                    .FirstOrDefault() ?? availableSequences.FirstOrDefault();
            }
            
            // Recommend based on learning style preferences
            return availableSequences.FirstOrDefault();
        }
        
        private void UpdateTutorialMetrics()
        {
            _tutorialMetrics.TotalTutorialsCompleted = _playerProgress.CompletedSequences.Count;
            _tutorialMetrics.TotalStepsCompleted = _playerProgress.CompletedSteps.Count;
            _tutorialMetrics.AverageCompletionTime = _stepPerformance.Values.Average(p => p.AverageCompletionTime);
            _tutorialMetrics.OverallSuccessRate = CalculateOverallSuccessRate();
            _tutorialMetrics.HintUsageRate = _learningAnalytics.HintUsageRate;
        }
        
        private float CalculateOverallSuccessRate()
        {
            if (_stepPerformance.Count == 0) return 1.0f;
            return _stepPerformance.Values.Average(p => p.SuccessRate);
        }
        
        private void UpdatePerformanceMetrics()
        {
            // Update various performance metrics
        }
        
        private void DetectLearningPreferences()
        {
            // Analyze and detect learning preferences
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            LogInfo($"EnhancedTutorialManager shutdown - Completed tutorials: {_playerProgress.CompletedSequences.Count}");
        }
    }
}