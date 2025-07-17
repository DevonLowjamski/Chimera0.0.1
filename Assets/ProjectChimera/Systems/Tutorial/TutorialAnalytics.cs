using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Tutorial analytics system for Project Chimera.
    /// Tracks tutorial usage, performance, and player behavior.
    /// </summary>
    public class TutorialAnalytics
    {
        private TutorialSettings _settings;
        private Dictionary<string, TutorialAnalyticsData> _sequenceAnalytics;
        private Dictionary<string, TutorialStepAnalytics> _stepAnalytics;
        private List<TutorialInteraction> _interactions;
        private bool _isInitialized;
        
        // Session tracking
        private string _currentSessionId;
        private DateTime _sessionStartTime;
        private TutorialSessionStats _currentSession;
        
        // Performance metrics
        private int _totalSequencesStarted;
        private int _totalSequencesCompleted;
        private int _totalStepsCompleted;
        private int _totalHintsShown;
        private float _totalTutorialTime;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public string CurrentSessionId => _currentSessionId;
        public int TotalSequencesStarted => _totalSequencesStarted;
        public int TotalSequencesCompleted => _totalSequencesCompleted;
        public float CompletionRate => _totalSequencesStarted > 0 ? (float)_totalSequencesCompleted / _totalSequencesStarted : 0f;
        
        public TutorialAnalytics(TutorialSettings settings)
        {
            _settings = settings;
            
            InitializeAnalytics();
        }
        
        /// <summary>
        /// Initialize analytics system
        /// </summary>
        private void InitializeAnalytics()
        {
            if (!_settings.TrackAnalytics)
            {
                _isInitialized = true;
                return;
            }
            
            _sequenceAnalytics = new Dictionary<string, TutorialAnalyticsData>();
            _stepAnalytics = new Dictionary<string, TutorialStepAnalytics>();
            _interactions = new List<TutorialInteraction>();
            
            StartNewSession();
            
            _isInitialized = true;
            Debug.Log("Tutorial analytics system initialized");
        }
        
        /// <summary>
        /// Start new analytics session
        /// </summary>
        private void StartNewSession()
        {
            _currentSessionId = Guid.NewGuid().ToString();
            _sessionStartTime = DateTime.Now;
            
            _currentSession = new TutorialSessionStats
            {
                SessionId = _currentSessionId,
                SessionStart = _sessionStartTime,
                StepsCompleted = 0,
                HintsUsed = 0,
                StepsSkipped = 0,
                SessionCompleted = false,
                Interactions = new List<TutorialInteraction>()
            };
            
            Debug.Log($"Started new tutorial analytics session: {_currentSessionId}");
        }
        
        /// <summary>
        /// Track sequence started
        /// </summary>
        public void TrackSequenceStarted(string sequenceId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            _totalSequencesStarted++;
            
            var analyticsData = GetOrCreateSequenceAnalytics(sequenceId);
            analyticsData.StartCount++;
            analyticsData.LastStartTime = DateTime.Now;
            
            Debug.Log($"Tracked sequence started: {sequenceId}");
        }
        
        /// <summary>
        /// Track sequence completed
        /// </summary>
        public void TrackSequenceCompleted(string sequenceId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            _totalSequencesCompleted++;
            
            var analyticsData = GetOrCreateSequenceAnalytics(sequenceId);
            analyticsData.CompletionCount++;
            analyticsData.LastCompletionTime = DateTime.Now;
            
            if (analyticsData.LastStartTime.HasValue)
            {
                var duration = (DateTime.Now - analyticsData.LastStartTime.Value).TotalSeconds;
                analyticsData.TotalDuration += (float)duration;
                analyticsData.AverageDuration = analyticsData.TotalDuration / analyticsData.CompletionCount;
            }
            
            _currentSession.SessionCompleted = true;
            
            Debug.Log($"Tracked sequence completed: {sequenceId}");
        }
        
        /// <summary>
        /// Track sequence skipped
        /// </summary>
        public void TrackSequenceSkipped(string sequenceId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            var analyticsData = GetOrCreateSequenceAnalytics(sequenceId);
            analyticsData.SkipCount++;
            
            Debug.Log($"Tracked sequence skipped: {sequenceId}");
        }
        
        /// <summary>
        /// Track sequence abandoned
        /// </summary>
        public void TrackSequenceAbandoned(string sequenceId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            var analyticsData = GetOrCreateSequenceAnalytics(sequenceId);
            analyticsData.AbandonCount++;
            
            Debug.Log($"Tracked sequence abandoned: {sequenceId}");
        }
        
        /// <summary>
        /// Track step started
        /// </summary>
        public void TrackStepStarted(string stepId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            var stepAnalytics = GetOrCreateStepAnalytics(stepId);
            stepAnalytics.StartCount++;
            stepAnalytics.LastStartTime = DateTime.Now;
            
            Debug.Log($"Tracked step started: {stepId}");
        }
        
        /// <summary>
        /// Track step completed
        /// </summary>
        public void TrackStepCompleted(string stepId, float duration)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            _totalStepsCompleted++;
            _currentSession.StepsCompleted++;
            
            var stepAnalytics = GetOrCreateStepAnalytics(stepId);
            stepAnalytics.CompletionCount++;
            stepAnalytics.TotalDuration += duration;
            stepAnalytics.AverageDuration = stepAnalytics.TotalDuration / stepAnalytics.CompletionCount;
            
            Debug.Log($"Tracked step completed: {stepId} (duration: {duration:F2}s)");
        }
        
        /// <summary>
        /// Track step skipped
        /// </summary>
        public void TrackStepSkipped(string stepId)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            _currentSession.StepsSkipped++;
            
            var stepAnalytics = GetOrCreateStepAnalytics(stepId);
            stepAnalytics.SkipCount++;
            
            Debug.Log($"Tracked step skipped: {stepId}");
        }
        
        /// <summary>
        /// Track hint shown
        /// </summary>
        public void TrackHintShown(string stepId, string hintText)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            _totalHintsShown++;
            _currentSession.HintsUsed++;
            
            var stepAnalytics = GetOrCreateStepAnalytics(stepId);
            stepAnalytics.HintCount++;
            
            Debug.Log($"Tracked hint shown for step: {stepId}");
        }
        
        /// <summary>
        /// Track validation failed
        /// </summary>
        public void TrackValidationFailed(string stepId, string errorMessage)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            var stepAnalytics = GetOrCreateStepAnalytics(stepId);
            stepAnalytics.FailureCount++;
            
            // Track interaction
            var interaction = new TutorialInteraction
            {
                StepId = stepId,
                InteractionType = TutorialInteractionType.Custom,
                Timestamp = DateTime.Now,
                WasSuccessful = false
            };
            
            _interactions.Add(interaction);
            _currentSession.Interactions.Add(interaction);
            
            Debug.Log($"Tracked validation failed for step: {stepId} - {errorMessage}");
        }
        
        /// <summary>
        /// Track player interaction
        /// </summary>
        public void TrackInteraction(string stepId, TutorialInteractionType interactionType, Vector2 position, bool wasSuccessful)
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return;
            
            var interaction = new TutorialInteraction
            {
                StepId = stepId,
                InteractionType = interactionType,
                Timestamp = DateTime.Now,
                Position = position,
                WasSuccessful = wasSuccessful
            };
            
            _interactions.Add(interaction);
            _currentSession.Interactions.Add(interaction);
            
            Debug.Log($"Tracked interaction: {interactionType} for step {stepId} (successful: {wasSuccessful})");
        }
        
        /// <summary>
        /// Get or create sequence analytics
        /// </summary>
        private TutorialAnalyticsData GetOrCreateSequenceAnalytics(string sequenceId)
        {
            if (!_sequenceAnalytics.TryGetValue(sequenceId, out var analyticsData))
            {
                analyticsData = new TutorialAnalyticsData
                {
                    SequenceId = sequenceId,
                    StartCount = 0,
                    CompletionCount = 0,
                    SkipCount = 0,
                    AbandonCount = 0,
                    TotalDuration = 0f,
                    AverageDuration = 0f
                };
                _sequenceAnalytics[sequenceId] = analyticsData;
            }
            
            return analyticsData;
        }
        
        /// <summary>
        /// Get or create step analytics
        /// </summary>
        private TutorialStepAnalytics GetOrCreateStepAnalytics(string stepId)
        {
            if (!_stepAnalytics.TryGetValue(stepId, out var stepAnalytics))
            {
                stepAnalytics = new TutorialStepAnalytics
                {
                    StepId = stepId,
                    StartCount = 0,
                    CompletionCount = 0,
                    SkipCount = 0,
                    FailureCount = 0,
                    HintCount = 0,
                    TotalDuration = 0f,
                    AverageDuration = 0f
                };
                _stepAnalytics[stepId] = stepAnalytics;
            }
            
            return stepAnalytics;
        }
        
        /// <summary>
        /// Get sequence analytics
        /// </summary>
        public TutorialAnalyticsData GetSequenceAnalytics(string sequenceId)
        {
            return _sequenceAnalytics.TryGetValue(sequenceId, out var data) ? data : null;
        }
        
        /// <summary>
        /// Get step analytics
        /// </summary>
        public TutorialStepAnalytics GetStepAnalytics(string stepId)
        {
            return _stepAnalytics.TryGetValue(stepId, out var data) ? data : null;
        }
        
        /// <summary>
        /// Get overall analytics summary
        /// </summary>
        public TutorialAnalyticsSummary GetAnalyticsSummary()
        {
            var totalInteractions = _interactions.Count;
            var successfulInteractions = _interactions.Count(i => i.WasSuccessful);
            
            return new TutorialAnalyticsSummary
            {
                TotalSequencesStarted = _totalSequencesStarted,
                TotalSequencesCompleted = _totalSequencesCompleted,
                TotalStepsCompleted = _totalStepsCompleted,
                TotalHintsShown = _totalHintsShown,
                CompletionRate = CompletionRate,
                SuccessRate = totalInteractions > 0 ? (float)successfulInteractions / totalInteractions : 0f,
                AverageStepsPerSequence = _totalSequencesCompleted > 0 ? (float)_totalStepsCompleted / _totalSequencesCompleted : 0f,
                AverageHintsPerStep = _totalStepsCompleted > 0 ? (float)_totalHintsShown / _totalStepsCompleted : 0f,
                TotalSessions = 1, // Current session
                ActiveSequences = _sequenceAnalytics.Count,
                TotalInteractions = totalInteractions
            };
        }
        
        /// <summary>
        /// Export analytics data
        /// </summary>
        public string ExportAnalyticsData()
        {
            if (!_isInitialized || !_settings.TrackAnalytics)
                return "Analytics tracking disabled";
            
            var summary = GetAnalyticsSummary();
            
            var export = $"Tutorial Analytics Export\n";
            export += $"Generated: {DateTime.Now}\n";
            export += $"Session ID: {_currentSessionId}\n\n";
            
            export += $"Summary:\n";
            export += $"- Total Sequences Started: {summary.TotalSequencesStarted}\n";
            export += $"- Total Sequences Completed: {summary.TotalSequencesCompleted}\n";
            export += $"- Completion Rate: {summary.CompletionRate:P2}\n";
            export += $"- Total Steps Completed: {summary.TotalStepsCompleted}\n";
            export += $"- Total Hints Shown: {summary.TotalHintsShown}\n";
            export += $"- Success Rate: {summary.SuccessRate:P2}\n\n";
            
            export += $"Sequence Analytics:\n";
            foreach (var sequence in _sequenceAnalytics.Values)
            {
                export += $"- {sequence.SequenceId}: {sequence.CompletionCount}/{sequence.StartCount} completed, avg {sequence.AverageDuration:F1}s\n";
            }
            
            return export;
        }
        
        /// <summary>
        /// Reset analytics data
        /// </summary>
        public void ResetAnalytics()
        {
            _sequenceAnalytics.Clear();
            _stepAnalytics.Clear();
            _interactions.Clear();
            
            _totalSequencesStarted = 0;
            _totalSequencesCompleted = 0;
            _totalStepsCompleted = 0;
            _totalHintsShown = 0;
            _totalTutorialTime = 0f;
            
            StartNewSession();
            
            Debug.Log("Reset tutorial analytics data");
        }
        
        /// <summary>
        /// Cleanup analytics system
        /// </summary>
        public void Cleanup()
        {
            if (_currentSession != null)
            {
                _currentSession.SessionEnd = DateTime.Now;
                _currentSession.TotalDuration = (float)(DateTime.Now - _sessionStartTime).TotalSeconds;
            }
            
            _isInitialized = false;
            Debug.Log("Tutorial analytics system cleaned up");
        }
    }
    
    /// <summary>
    /// Tutorial analytics data for sequences
    /// </summary>
    public class TutorialAnalyticsData
    {
        public string SequenceId;
        public int StartCount;
        public int CompletionCount;
        public int SkipCount;
        public int AbandonCount;
        public float TotalDuration;
        public float AverageDuration;
        public DateTime? LastStartTime;
        public DateTime? LastCompletionTime;
    }
    
    /// <summary>
    /// Tutorial step analytics data
    /// </summary>
    public class TutorialStepAnalytics
    {
        public string StepId;
        public int StartCount;
        public int CompletionCount;
        public int SkipCount;
        public int FailureCount;
        public int HintCount;
        public float TotalDuration;
        public float AverageDuration;
        public DateTime? LastStartTime;
    }
    
    /// <summary>
    /// Tutorial analytics summary
    /// </summary>
    public struct TutorialAnalyticsSummary
    {
        public int TotalSequencesStarted;
        public int TotalSequencesCompleted;
        public int TotalStepsCompleted;
        public int TotalHintsShown;
        public float CompletionRate;
        public float SuccessRate;
        public float AverageStepsPerSequence;
        public float AverageHintsPerStep;
        public int TotalSessions;
        public int ActiveSequences;
        public int TotalInteractions;
    }
}