using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;
using ProjectChimera.Data.Events;
using ProjectChimera.Events;
// Type alias to resolve ambiguity
using ConsequenceType = ProjectChimera.Events.ConsequenceType;
using ConsequenceSeverity = ProjectChimera.Events.ConsequenceSeverity;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Type-safe event channel for consequence tracking events in Project Chimera's narrative system.
    /// Features consequence validation, delayed effect processing, ripple effect monitoring,
    /// and educational impact tracking with scientific accuracy enforcement.
    /// </summary>
    [CreateAssetMenu(fileName = "New Consequence Event Channel", menuName = "Project Chimera/Events/Narrative/Consequence Event Channel", order = 203)]
    public class ConsequenceEventChannelSO : GameEventChannelSO<NarrativeConsequenceEventData>
    {
        [Header("Consequence Event Configuration")]
        [SerializeField] private bool _enableConsequenceValidation = true;
        [SerializeField] private bool _enableDelayedConsequenceTracking = true;
        [SerializeField] private bool _enableRippleEffectMonitoring = true;
        [SerializeField] private bool _enableEducationalImpactTracking = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        
        [Header("Event Filtering")]
        [SerializeField] private List<ConsequenceType> _allowedConsequenceTypes = new List<ConsequenceType>();
        [SerializeField] private List<ConsequenceSeverity> _allowedSeverityLevels = new List<ConsequenceSeverity>();
        [SerializeField] private float _minimumImpactThreshold = 0.1f;
        [SerializeField] private bool _filterByEducationalContent = false;
        
        [Header("Processing Settings")]
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private float _batchProcessingInterval = 1.0f;
        [SerializeField] private int _maxConsequencesPerBatch = 10;
        [SerializeField] private bool _enableRealTimeProcessing = true;
        
        [Header("Educational Validation")]
        [SerializeField] private bool _validateEducationalConsequences = true;
        [SerializeField] private float _minimumEducationalValue = 0.3f;
        [SerializeField] private bool _requireLearningObjectives = true;
        [SerializeField] private bool _trackLearningOutcomes = true;
        
        // Consequence tracking data
        private Dictionary<string, NarrativeConsequenceEventData> _activeConsequences = new Dictionary<string, NarrativeConsequenceEventData>();
        private Queue<NarrativeConsequenceEventData> _delayedConsequences = new Queue<NarrativeConsequenceEventData>();
        private Dictionary<ConsequenceType, int> _consequenceTypeCounters = new Dictionary<ConsequenceType, int>();
        private Dictionary<string, List<NarrativeConsequenceEventData>> _rippleEffectChains = new Dictionary<string, List<NarrativeConsequenceEventData>>();
        
        // Educational tracking data
        private Dictionary<string, EducationalConsequenceData> _educationalConsequences = new Dictionary<string, EducationalConsequenceData>();
        private Dictionary<string, LearningOutcomeData> _learningOutcomes = new Dictionary<string, LearningOutcomeData>();
        
        // Performance monitoring
        private Queue<NarrativeConsequenceEventData> _eventQueue = new Queue<NarrativeConsequenceEventData>();
        private float _lastBatchProcessingTime;
        private int _totalConsequencesProcessed;
        
        public override void Raise(NarrativeConsequenceEventData eventData)
        {
            if (!ValidateConsequenceEvent(eventData))
            {
                Debug.LogWarning($"[ConsequenceEventChannelSO] Invalid consequence event rejected: {eventData?.ConsequenceType} with severity {eventData?.Severity}");
                return;
            }
            
            // Add timestamp
            eventData.Timestamp = DateTime.Now;
            
            // Process or queue the event
            if (_enableBatchProcessing)
            {
                QueueEvent(eventData);
            }
            else
            {
                ProcessEventImmediate(eventData);
            }
        }
        
        private bool ValidateConsequenceEvent(NarrativeConsequenceEventData eventData)
        {
            if (eventData == null) return false;
            
            // Check consequence type filtering
            if (_allowedConsequenceTypes.Count > 0 && !_allowedConsequenceTypes.Contains(eventData.ConsequenceType))
            {
                return false;
            }
            
            // Check severity level filtering
            if (_allowedSeverityLevels.Count > 0 && !_allowedSeverityLevels.Contains(eventData.Severity))
            {
                return false;
            }
            
            // Check impact threshold
            if (eventData.ImpactValue < _minimumImpactThreshold)
            {
                return false;
            }
            
            // Validate educational content if required
            if (_filterByEducationalContent && eventData.HasEducationalContent)
            {
                if (_requireScientificAccuracy && !eventData.IsScientificallyAccurate)
                {
                    return false;
                }
                
                if (_requireLearningObjectives && eventData.LearningObjectives.Count == 0)
                {
                    return false;
                }
                
                if (eventData.EducationalValue < _minimumEducationalValue)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void QueueEvent(NarrativeConsequenceEventData eventData)
        {
            _eventQueue.Enqueue(eventData);
            
            // Process queue if interval reached or queue is full
            if (Time.time - _lastBatchProcessingTime >= _batchProcessingInterval || 
                _eventQueue.Count >= _maxConsequencesPerBatch)
            {
                ProcessEventQueue();
            }
        }
        
        private void ProcessEventQueue()
        {
            while (_eventQueue.Count > 0)
            {
                var eventData = _eventQueue.Dequeue();
                ProcessEventImmediate(eventData);
            }
            
            _lastBatchProcessingTime = Time.time;
        }
        
        private void ProcessEventImmediate(NarrativeConsequenceEventData eventData)
        {
            // Update consequence tracking
            UpdateConsequenceTracking(eventData);
            
            // Update educational tracking if applicable
            if (_enableEducationalImpactTracking && eventData.HasEducationalContent)
            {
                UpdateEducationalTracking(eventData);
            }
            
            // Update ripple effect monitoring
            if (_enableRippleEffectMonitoring)
            {
                UpdateRippleEffectTracking(eventData);
            }
            
            // Update counters
            UpdateConsequenceCounters(eventData);
            
            // Raise the actual event
            base.Raise(eventData);
            
            _totalConsequencesProcessed++;
        }
        
        private void UpdateConsequenceTracking(NarrativeConsequenceEventData eventData)
        {
            _activeConsequences[eventData.ConsequenceId] = eventData;
            
            // Handle delayed consequences
            if (_enableDelayedConsequenceTracking && eventData.IsDelayed)
            {
                _delayedConsequences.Enqueue(eventData);
            }
        }
        
        private void UpdateEducationalTracking(NarrativeConsequenceEventData eventData)
        {
            if (!eventData.HasEducationalContent) return;
            
            var educationalData = new EducationalConsequenceData
            {
                ConsequenceId = eventData.ConsequenceId,
                Topic = eventData.EducationalTopic,
                LearningValue = eventData.EducationalValue,
                IsScientificallyAccurate = eventData.IsScientificallyAccurate,
                LearningObjectives = new List<string>(eventData.LearningObjectives),
                Timestamp = eventData.Timestamp
            };
            
            _educationalConsequences[eventData.ConsequenceId] = educationalData;
            
            // Track learning outcomes if enabled
            if (_trackLearningOutcomes)
            {
                TrackLearningOutcome(eventData);
            }
        }
        
        private void TrackLearningOutcome(NarrativeConsequenceEventData eventData)
        {
            var outcomeData = new LearningOutcomeData
            {
                ConsequenceId = eventData.ConsequenceId,
                Topic = eventData.EducationalTopic,
                OutcomeDescription = eventData.LearningOutcome,
                EffectivenessScore = eventData.EducationalValue,
                ComprehensionLevel = eventData.PlayerComprehension,
                Timestamp = eventData.Timestamp
            };
            
            _learningOutcomes[eventData.ConsequenceId] = outcomeData;
        }
        
        private void UpdateRippleEffectTracking(NarrativeConsequenceEventData eventData)
        {
            if (string.IsNullOrEmpty(eventData.SourceConsequenceId)) return;
            
            // Add to ripple effect chain
            if (!_rippleEffectChains.ContainsKey(eventData.SourceConsequenceId))
            {
                _rippleEffectChains[eventData.SourceConsequenceId] = new List<NarrativeConsequenceEventData>();
            }
            
            _rippleEffectChains[eventData.SourceConsequenceId].Add(eventData);
        }
        
        private void UpdateConsequenceCounters(NarrativeConsequenceEventData eventData)
        {
            if (!_consequenceTypeCounters.ContainsKey(eventData.ConsequenceType))
            {
                _consequenceTypeCounters[eventData.ConsequenceType] = 0;
            }
            
            _consequenceTypeCounters[eventData.ConsequenceType]++;
        }
        
        public NarrativeConsequenceEventData GetActiveConsequence(string consequenceId)
        {
            return _activeConsequences.TryGetValue(consequenceId, out var consequence) ? consequence : null;
        }
        
        public List<NarrativeConsequenceEventData> GetConsequencesByType(ConsequenceType type)
        {
            return _activeConsequences.Values.Where(c => c.ConsequenceType == type).ToList();
        }
        
        public EducationalConsequenceData GetEducationalConsequence(string consequenceId)
        {
            return _educationalConsequences.TryGetValue(consequenceId, out var educationalData) ? educationalData : null;
        }
        
        public List<NarrativeConsequenceEventData> GetRippleEffectChain(string sourceConsequenceId)
        {
            return _rippleEffectChains.TryGetValue(sourceConsequenceId, out var chain) ? chain : new List<NarrativeConsequenceEventData>();
        }
        
        public ConsequenceChannelStatistics GetStatistics()
        {
            return new ConsequenceChannelStatistics
            {
                TotalConsequencesProcessed = _totalConsequencesProcessed,
                ActiveConsequenceCount = _activeConsequences.Count,
                DelayedConsequenceCount = _delayedConsequences.Count,
                EducationalConsequenceCount = _educationalConsequences.Count,
                RippleEffectChainCount = _rippleEffectChains.Count,
                ConsequenceTypeBreakdown = new Dictionary<ConsequenceType, int>(_consequenceTypeCounters)
            };
        }
        
        public void FlushEventQueue()
        {
            ProcessEventQueue();
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Ensure minimum thresholds are valid
            _minimumImpactThreshold = Mathf.Clamp(_minimumImpactThreshold, 0f, 1f);
            _minimumEducationalValue = Mathf.Clamp(_minimumEducationalValue, 0f, 1f);
            _batchProcessingInterval = Mathf.Max(_batchProcessingInterval, 0.1f);
            _maxConsequencesPerBatch = Mathf.Max(_maxConsequencesPerBatch, 1);
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class NarrativeConsequenceEventData
    {
        public string ConsequenceId;
        public ConsequenceType ConsequenceType;
        public ConsequenceSeverity Severity;
        public float ImpactValue = 0.5f;
        public bool IsDelayed = false;
        public float DelayTime = 0f;
        public string SourceConsequenceId;
        public DateTime Timestamp;
        
        // Educational content
        public bool HasEducationalContent = false;
        public string EducationalTopic;
        public float EducationalValue = 0f;
        public bool IsScientificallyAccurate = true;
        public List<string> LearningObjectives = new List<string>();
        public string LearningOutcome;
        public float PlayerComprehension = 0.5f;
        
        // Relationship impacts
        public Dictionary<string, float> RelationshipImpacts = new Dictionary<string, float>();
        
        // Narrative flags
        public List<string> NarrativeFlags = new List<string>();
    }
    
    [Serializable]
    public class EducationalConsequenceData
    {
        public string ConsequenceId;
        public string Topic;
        public float LearningValue;
        public bool IsScientificallyAccurate;
        public List<string> LearningObjectives = new List<string>();
        public DateTime Timestamp;
    }
    
    [Serializable]
    public class LearningOutcomeData
    {
        public string ConsequenceId;
        public string Topic;
        public string OutcomeDescription;
        public float EffectivenessScore;
        public float ComprehensionLevel;
        public DateTime Timestamp;
    }
    
    [Serializable]
    public class ConsequenceChannelStatistics
    {
        public int TotalConsequencesProcessed;
        public int ActiveConsequenceCount;
        public int DelayedConsequenceCount;
        public int EducationalConsequenceCount;
        public int RippleEffectChainCount;
        public Dictionary<ConsequenceType, int> ConsequenceTypeBreakdown = new Dictionary<ConsequenceType, int>();
    }
} 