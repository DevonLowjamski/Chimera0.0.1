using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Collections = System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;
using ProjectChimera.Events;
// Explicit namespace alias to resolve ambiguous references
using CultivationExpertise = ProjectChimera.Events.CultivationExpertise;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Type-safe event channel for character interaction events in Project Chimera's narrative system.
    /// Features relationship tracking, emotional state monitoring, educational mentorship validation,
    /// and scientific accuracy compliance for cannabis cultivation character interactions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Event Channel", menuName = "Project Chimera/Events/Narrative/Character Event Channel", order = 201)]
    public class CharacterEventChannelSO : GameEventChannelSO<CharacterEventData>
    {
        [Header("Character Event Configuration")]
        [SerializeField] private bool _enableRelationshipTracking = true;
        [SerializeField] private bool _enableEmotionalStateTracking = true;
        [SerializeField] private bool _enableInfluenceTracking = true;
        [SerializeField] private bool _enableEducationalMentorshipTracking = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        
        [Header("Event Filtering")]
        [SerializeField] private List<CharacterEventType> _allowedEventTypes = new List<CharacterEventType>();
        [SerializeField] private List<string> _trackedCharacterIds = new List<string>();
        [SerializeField] private float _minimumRelationshipThreshold = 0.0f;
        [SerializeField] private bool _filterByEducationalRole = false;
        
        [Header("Relationship Monitoring")]
        [SerializeField] private float _significantRelationshipChange = 10.0f;
        [SerializeField] private bool _enableRelationshipAlerts = true;
        [SerializeField] private float _trustDecayRate = 0.001f;
        [SerializeField] private float _respectGrowthMultiplier = 1.2f;
        [SerializeField] private int _maxRelationshipHistory = 50;
        
        [Header("Educational Validation")]
        [SerializeField] private bool _validateEducationalCredentials = true;
        [SerializeField] private bool _trackTeachingEffectiveness = true;
        [SerializeField] private float _minimumCredibilityLevel = 0.5f;
        [SerializeField] private bool _requireExpertiseValidation = true;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableEventAggregation = true;
        [SerializeField] private float _aggregationInterval = 1.0f;
        [SerializeField] private int _maxConcurrentEvents = 20;
        [SerializeField] private bool _enableRealTimeProcessing = true;
        
        // Relationship tracking data
        private Collections.Dictionary<string, CharacterRelationshipTracker> _relationshipTrackers = new Collections.Dictionary<string, CharacterRelationshipTracker>();
        private Collections.Dictionary<string, Collections.List<CharacterEventData>> _characterEventHistory = new Collections.Dictionary<string, Collections.List<CharacterEventData>>();
        private Collections.Dictionary<CharacterEventType, int> _eventTypeCounters = new Collections.Dictionary<CharacterEventType, int>();
        
        // Educational tracking data
        private Collections.Dictionary<string, EducationalInteractionTracker> _educationalTrackers = new Collections.Dictionary<string, EducationalInteractionTracker>();
        private Collections.Dictionary<string, TeachingEffectivenessData> _teachingEffectiveness = new Collections.Dictionary<string, TeachingEffectivenessData>();
        
        // Performance monitoring
        private Collections.Queue<CharacterEventData> _eventQueue = new Collections.Queue<CharacterEventData>();
        private float _lastAggregationTime;
        private int _totalEventsProcessed;
        
        public override void Raise(CharacterEventData eventData)
        {
            if (!ValidateCharacterEvent(eventData))
            {
                Debug.LogWarning($"[CharacterEventChannelSO] Invalid character event rejected: {eventData?.EventType} for character {eventData?.CharacterId}");
                return;
            }
            
            // Add timestamp (ensure it's float)
            if (eventData.Timestamp == 0)
                eventData.Timestamp = Time.time;
            
            // Process or queue the event
            if (_enableEventAggregation)
            {
                QueueEvent(eventData);
            }
            else
            {
                ProcessEventImmediate(eventData);
            }
        }
        
        private bool ValidateCharacterEvent(CharacterEventData eventData)
        {
            if (eventData == null) return false;
            
            // Check event type filtering
            if (_allowedEventTypes.Count > 0 && !_allowedEventTypes.Contains(eventData.EventType))
            {
                return false;
            }
            
            // Check character ID tracking
            if (_trackedCharacterIds.Count > 0 && !_trackedCharacterIds.Contains(eventData.CharacterId))
            {
                return false;
            }
            
            // Validate relationship threshold
            if (_enableRelationshipTracking && eventData.HasRelationshipData)
            {
                var totalRelationshipLevel = eventData.TrustLevel + eventData.RespectLevel + eventData.InfluenceLevel;
                if (totalRelationshipLevel < _minimumRelationshipThreshold)
                {
                    return false;
                }
            }
            
            // Validate educational role if required
            if (_filterByEducationalRole && eventData.HasEducationalContent)
            {
                if (!eventData.IsEducationalMentor)
                {
                    return false;
                }
                
                if (_requireScientificAccuracy && !eventData.IsScientificallyAccurate)
                {
                    return false;
                }
            }
            
            // Validate educational credentials if required
            if (_validateEducationalCredentials && eventData.HasEducationalContent)
            {
                if (eventData.CredibilityLevel < _minimumCredibilityLevel)
                {
                    return false;
                }
                
                if (_requireExpertiseValidation && eventData.ExpertiseAreas.Count == 0)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void QueueEvent(CharacterEventData eventData)
        {
            _eventQueue.Enqueue(eventData);
            
            // Process queue if interval reached or queue is full
            if (Time.time - _lastAggregationTime >= _aggregationInterval || 
                _eventQueue.Count >= _maxConcurrentEvents)
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
            
            _lastAggregationTime = Time.time;
        }
        
        private void ProcessEventImmediate(CharacterEventData eventData)
        {
            // Update relationship tracking
            if (_enableRelationshipTracking)
            {
                UpdateRelationshipTracking(eventData);
            }
            
            // Update emotional state tracking
            if (_enableEmotionalStateTracking)
            {
                UpdateEmotionalStateTracking(eventData);
            }
            
            // Update educational tracking
            if (_enableEducationalMentorshipTracking && eventData.HasEducationalContent)
            {
                UpdateEducationalTracking(eventData);
            }
            
            // Add to history
            AddToCharacterHistory(eventData);
            
            // Update counters
            UpdateEventCounters(eventData);
            
            // Check for significant relationship changes
            if (_enableRelationshipAlerts)
            {
                CheckForSignificantChanges(eventData);
            }
            
            // Raise the actual event
            base.Raise(eventData);
            
            _totalEventsProcessed++;
        }
        
        private void UpdateRelationshipTracking(CharacterEventData eventData)
        {
            if (!eventData.HasRelationshipData) return;
            
            var characterId = eventData.CharacterId;
            
            // Initialize tracker if needed
            if (!_relationshipTrackers.ContainsKey(characterId))
            {
                _relationshipTrackers[characterId] = new CharacterRelationshipTracker
                {
                    CharacterId = characterId,
                    TrustLevel = eventData.TrustLevel,
                    RespectLevel = eventData.RespectLevel,
                    InfluenceLevel = eventData.InfluenceLevel,
                    LastUpdateTime = eventData.Timestamp
                };
            }
            
            var tracker = _relationshipTrackers[characterId];
            
            // Calculate changes
            var trustChange = eventData.TrustLevel - tracker.TrustLevel;
            var respectChange = eventData.RespectLevel - tracker.RespectLevel;
            var influenceChange = eventData.InfluenceLevel - tracker.InfluenceLevel;
            
            // Update tracker
            tracker.TrustLevel = eventData.TrustLevel;
            tracker.RespectLevel = eventData.RespectLevel;
            tracker.InfluenceLevel = eventData.InfluenceLevel;
            tracker.LastUpdateTime = eventData.Timestamp;
            
            // Record relationship change
            if (Mathf.Abs(trustChange) > 0.01f || Mathf.Abs(respectChange) > 0.01f || Mathf.Abs(influenceChange) > 0.01f)
            {
                var relationshipChange = new RelationshipChange
                {
                    CharacterId = characterId,
                    TrustChange = trustChange,
                    RespectChange = respectChange,
                    InfluenceChange = influenceChange,
                    EventType = eventData.EventType.ToString(),
                    Timestamp = eventData.Timestamp,
                    Context = eventData.InteractionContext
                };
                
                tracker.RelationshipHistory.Add(relationshipChange);
                
                // Maintain history size
                if (tracker.RelationshipHistory.Count > _maxRelationshipHistory)
                {
                    tracker.RelationshipHistory.RemoveAt(0);
                }
            }
        }
        
        private void UpdateEmotionalStateTracking(CharacterEventData eventData)
        {
            if (!eventData.HasEmotionalData) return;
            
            var characterId = eventData.CharacterId;
            
            // Initialize tracker if needed
            if (!_relationshipTrackers.ContainsKey(characterId))
            {
                return; // Need relationship tracker first
            }
            
            var tracker = _relationshipTrackers[characterId];
            
            // Update emotional state
            tracker.CurrentEmotionalState = (float)eventData.EmotionalState;
            tracker.EmotionalIntensity = eventData.EmotionalIntensity;
            tracker.LastEmotionalUpdate = eventData.Timestamp;
            
            // Track emotional patterns
            tracker.EmotionalHistory.Add(new EmotionalStateRecord
            {
                State = eventData.EmotionalState.ToString(),
                Intensity = eventData.EmotionalIntensity,
                Trigger = eventData.EventType.ToString(),
                Timestamp = eventData.Timestamp
            });
            
            // Maintain emotional history size
            if (tracker.EmotionalHistory.Count > 20)
            {
                tracker.EmotionalHistory.RemoveAt(0);
            }
        }
        
        private void UpdateEducationalTracking(CharacterEventData eventData)
        {
            var characterId = eventData.CharacterId;
            
            // Initialize educational tracker if needed
            if (!_educationalTrackers.ContainsKey(characterId))
            {
                _educationalTrackers[characterId] = new EducationalInteractionTracker
                {
                    CharacterId = characterId,
                    IsEducationalMentor = eventData.IsEducationalMentor,
                    ExpertiseAreas = new Collections.List<CultivationExpertise>(),
                    CredibilityLevel = eventData.CredibilityLevel
                };
            }
            
            var tracker = _educationalTrackers[characterId];
            
            // Track educational interaction
            var interaction = new EducationalInteraction
            {
                InteractionType = eventData.EventType,
                Topic = eventData.EducationalTopic,
                EffectivenessRating = eventData.TeachingEffectiveness,
                IsScientificallyAccurate = eventData.IsScientificallyAccurate,
                LearningOutcome = eventData.LearningOutcome,
                Timestamp = eventData.Timestamp
            };
            
            tracker.Interactions.Add(interaction);
            tracker.TotalInteractions++;
            tracker.LastInteractionTime = eventData.Timestamp;
            
            // Update teaching effectiveness data
            if (!_teachingEffectiveness.ContainsKey(characterId))
            {
                _teachingEffectiveness[characterId] = new TeachingEffectivenessData
                {
                    CharacterId = characterId
                };
            }
            
            var effectiveness = _teachingEffectiveness[characterId];
            effectiveness.TotalSessions++;
            effectiveness.CumulativeEffectiveness += eventData.TeachingEffectiveness;
            effectiveness.AverageEffectiveness = effectiveness.CumulativeEffectiveness / effectiveness.TotalSessions;
            
            if (eventData.IsScientificallyAccurate)
            {
                effectiveness.ScientificallyAccurateCount++;
            }
            
            effectiveness.AccuracyRate = (float)effectiveness.ScientificallyAccurateCount / effectiveness.TotalSessions;
        }
        
        private void AddToCharacterHistory(CharacterEventData eventData)
        {
            var characterId = eventData.CharacterId;
            
            if (!_characterEventHistory.ContainsKey(characterId))
            {
                _characterEventHistory[characterId] = new Collections.List<CharacterEventData>();
            }
            
            _characterEventHistory[characterId].Add(eventData);
            
            // Maintain history size
            if (_characterEventHistory[characterId].Count > 100)
            {
                _characterEventHistory[characterId].RemoveAt(0);
            }
        }
        
        private void UpdateEventCounters(CharacterEventData eventData)
        {
            if (!_eventTypeCounters.ContainsKey(eventData.EventType))
            {
                _eventTypeCounters[eventData.EventType] = 0;
            }
            
            _eventTypeCounters[eventData.EventType]++;
        }
        
        private void CheckForSignificantChanges(CharacterEventData eventData)
        {
            if (!eventData.HasRelationshipData) return;
            
            var characterId = eventData.CharacterId;
            if (!_relationshipTrackers.ContainsKey(characterId)) return;
            
            var tracker = _relationshipTrackers[characterId];
            var recentHistory = tracker.RelationshipHistory.TakeLast(5).ToList();
            
            if (recentHistory.Count < 2) return;
            
            // Check for significant trust changes
            var totalTrustChange = recentHistory.Sum(change => Mathf.Abs(change.TrustChange));
            if (totalTrustChange >= _significantRelationshipChange)
            {
                // Create alert event
                var alertEvent = new CharacterRelationshipAlert
                {
                    CharacterId = characterId,
                    AlertType = RelationshipAlertType.SignificantTrustChange,
                    ChangeAmount = totalTrustChange,
                    CurrentLevel = tracker.TrustLevel,
                    Timestamp = eventData.Timestamp
                };
                
                // Raise alert (placeholder for actual alert system)
                Debug.Log($"[CharacterEventChannelSO] Significant trust change for {characterId}: {totalTrustChange:F2}");
            }
        }
        
        /// <summary>
        /// Get relationship tracker for specific character
        /// </summary>
        public CharacterRelationshipTracker GetRelationshipTracker(string characterId)
        {
            return _relationshipTrackers.ContainsKey(characterId) ? _relationshipTrackers[characterId] : null;
        }
        
        /// <summary>
        /// Get educational tracker for specific character
        /// </summary>
        public EducationalInteractionTracker GetEducationalTracker(string characterId)
        {
            return _educationalTrackers.ContainsKey(characterId) ? _educationalTrackers[characterId] : null;
        }
        
        /// <summary>
        /// Get teaching effectiveness data for character
        /// </summary>
        public TeachingEffectivenessData GetTeachingEffectiveness(string characterId)
        {
            return _teachingEffectiveness.ContainsKey(characterId) ? _teachingEffectiveness[characterId] : null;
        }
        
        /// <summary>
        /// Get character event statistics
        /// </summary>
        public CharacterEventStatistics GetCharacterEventStatistics()
        {
            return new CharacterEventStatistics
            {
                TotalEventsProcessed = _totalEventsProcessed,
                TrackedCharactersCount = _relationshipTrackers.Count,
                EducationalMentorsCount = _educationalTrackers.Count,
                EventTypeDistribution = new Collections.Dictionary<CharacterEventType, int>(_eventTypeCounters),
                AverageRelationshipLevel = CalculateAverageRelationshipLevel(),
                AverageTeachingEffectiveness = CalculateAverageTeachingEffectiveness()
            };
        }
        
        private float CalculateAverageRelationshipLevel()
        {
            if (_relationshipTrackers.Count == 0) return 0f;
            
            var totalLevel = _relationshipTrackers.Values.Sum(tracker => 
                (tracker.TrustLevel + tracker.RespectLevel + tracker.InfluenceLevel) / 3f);
            
            return totalLevel / _relationshipTrackers.Count;
        }
        
        private float CalculateAverageTeachingEffectiveness()
        {
            if (_teachingEffectiveness.Count == 0) return 0f;
            
            var totalEffectiveness = _teachingEffectiveness.Values.Sum(data => data.AverageEffectiveness);
            return totalEffectiveness / _teachingEffectiveness.Count;
        }
        
        /// <summary>
        /// Force process any queued events
        /// </summary>
        public void FlushEventQueue()
        {
            if (_eventQueue.Count > 0)
            {
                ProcessEventQueue();
            }
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate configuration
            _significantRelationshipChange = Mathf.Max(0f, _significantRelationshipChange);
            _trustDecayRate = Mathf.Clamp01(_trustDecayRate);
            _respectGrowthMultiplier = Mathf.Max(0f, _respectGrowthMultiplier);
            _maxRelationshipHistory = Mathf.Max(1, _maxRelationshipHistory);
            _minimumCredibilityLevel = Mathf.Clamp01(_minimumCredibilityLevel);
            _aggregationInterval = Mathf.Max(0.1f, _aggregationInterval);
            _maxConcurrentEvents = Mathf.Max(1, _maxConcurrentEvents);
        }
    }
    
    // Note: Supporting data structures are defined in ProjectChimera.Events.EventDataStructures
    
    // Local definition of TimeScaleEventData to resolve assembly reference issues
    [System.Serializable]
    public class TimeScaleEventData
    {
        public GameTimeScale NewTimeScale;
        public GameTimeScale PreviousTimeScale;
        public float ScaleMultiplier;
        public float TransitionDuration;
        public string ScaleChangeReason;
        public float Timestamp;
        
        // Additional properties referenced in gaming systems
        public float RealTimeDayDuration;
        public float GameDaysPerRealHour;
        public string Description;
        public float PlayerEngagementOptimal;
        public float RealTimeDayDuration2;
        public float GameDayPerRealHour;
        public float LockInPeriod;
        public bool IsPlayerInitiated;
        public float ChangeTimestamp;
        public Collections.Dictionary<string, object> ScaleParameters = new Collections.Dictionary<string, object>();
    }
}