using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;
using ProjectChimera.Data.Events;
using System.Collections.Generic;
using System.Linq;
using System;
using ProjectChimera.Systems.Narrative;
// Type aliases to resolve ambiguous references
using NarrativeCharacterEventChannelSO = ProjectChimera.Data.Narrative.CharacterEventChannelSO;
using EventsEffectivenessDataPoint = ProjectChimera.Data.Events.EffectivenessDataPoint;
using EventsEffectivenessTrend = ProjectChimera.Data.Events.EffectivenessTrend;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Advanced character relationship management system for Project Chimera's narrative engine.
    /// Features dynamic relationship modeling, emotional state tracking, influence systems,
    /// and educational mentorship validation with scientific accuracy for cannabis cultivation guidance.
    /// </summary>
    public class CharacterRelationshipSystem : MonoBehaviour
    {
        [Header("Relationship Configuration")]
        [SerializeField] private float _relationshipUpdateInterval = 1.0f;
        [SerializeField] private float _trustDecayRate = 0.001f;
        [SerializeField] private float _respectGrowthMultiplier = 1.2f;
        [SerializeField] private bool _enableEmotionalMemory = true;
        [SerializeField] private int _maxRelationshipHistory = 50;
        
        [Header("Educational Validation")]
        [SerializeField] private bool _validateEducationalCredentials = true;
        [SerializeField] private float _minimumCredibilityLevel = 0.5f;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private bool _trackTeachingEffectiveness = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxSimultaneousRelationships = 20;
        [SerializeField] private bool _enableRelationshipCaching = true;
        [SerializeField] private float _cacheUpdateInterval = 5.0f;
        [SerializeField] private bool _enableBatchUpdates = true;
        
        // Core relationship data
        private Dictionary<string, CharacterRelationship> _activeRelationships = new Dictionary<string, CharacterRelationship>();
        private Dictionary<string, RelationshipCache> _relationshipCache = new Dictionary<string, RelationshipCache>();
        private Queue<RelationshipUpdate> _pendingUpdates = new Queue<RelationshipUpdate>();
        
        // Educational tracking
        private Dictionary<string, EducationalMentorData> _educationalMentors = new Dictionary<string, EducationalMentorData>();
        private Dictionary<string, List<LearningInteraction>> _learningHistory = new Dictionary<string, List<LearningInteraction>>();
        
        // Performance monitoring
        private float _lastUpdateTime;
        private float _lastCacheUpdateTime;
        private int _totalRelationshipUpdates;
        private float _averageUpdateTime;
        
        // Event integration
        private NarrativeCharacterEventChannelSO _characterEventChannel;
        
        public void Initialize(CharacterDatabaseSO characterDatabase, CampaignConfigSO campaignConfig, NarrativeCharacterEventChannelSO eventChannel)
        {
            _characterEventChannel = eventChannel;
            
            // Initialize relationships from database
            InitializeRelationshipsFromDatabase(characterDatabase);
            
            // Initialize educational mentors
            InitializeEducationalMentors(characterDatabase);
            
            // Subscribe to events
            if (_characterEventChannel != null)
            {
                _characterEventChannel.OnEventRaised += HandleCharacterEvent;
            }
            
            Debug.Log("[CharacterRelationshipSystem] Initialized with character database");
        }
        
        private void Update()
        {
            if (Time.time - _lastUpdateTime >= _relationshipUpdateInterval)
            {
                UpdateRelationships();
                _lastUpdateTime = Time.time;
            }
            
            if (_enableRelationshipCaching && Time.time - _lastCacheUpdateTime >= _cacheUpdateInterval)
            {
                UpdateRelationshipCache();
                _lastCacheUpdateTime = Time.time;
            }
        }
        
        private void InitializeRelationshipsFromDatabase(CharacterDatabaseSO database)
        {
            // Initialize relationships for all characters
            var allCharacters = database.GetAllCharacters();
            
            foreach (var character in allCharacters)
            {
                var relationship = new CharacterRelationship(character.CharacterId)
                {
                    TrustLevel = character.DefaultTrustLevel,
                    RespectLevel = character.DefaultRespectLevel,
                    InfluenceLevel = character.InfluenceCapacity,
                    Type = RelationshipType.Neutral,
                    CharacterProfile = character
                };
                
                _activeRelationships[character.CharacterId] = relationship;
                
                // Initialize relationship cache
                if (_enableRelationshipCaching)
                {
                    _relationshipCache[character.CharacterId] = new RelationshipCache
                    {
                        CharacterId = character.CharacterId,
                        CachedLevel = relationship.GetOverallRelationshipLevel(),
                        LastCacheTime = Time.time
                    };
                }
            }
            
            // Apply predefined relationships from database
            foreach (var predefinedRelationship in database.PredefinedRelationships)
            {
                ApplyPredefinedRelationship(predefinedRelationship);
            }
        }
        
        private void InitializeEducationalMentors(CharacterDatabaseSO database)
        {
            foreach (var educationalRole in database.EducationalRoles)
            {
                var character = database.GetCharacterById(educationalRole.CharacterId);
                if (character != null && character.CanTeach)
                {
                    var mentorData = new EducationalMentorData
                    {
                        CharacterId = educationalRole.CharacterId,
                        Expertise = educationalRole.Expertise,
                        CredibilityLevel = educationalRole.CredibilityLevel,
                        ValidatedKnowledgeAreas = new List<string>(educationalRole.ValidatedKnowledgeAreas),
                        TeachingEffectiveness = character.TeachingEffectiveness,
                        IsScientificallyAccurate = educationalRole.IsScientificallyAccurate
                    };
                    
                    _educationalMentors[educationalRole.CharacterId] = mentorData;
                    
                    // Initialize learning history
                    _learningHistory[educationalRole.CharacterId] = new List<LearningInteraction>();
                }
            }
        }
        
        private void ApplyPredefinedRelationship(PredefinedRelationship predefinedData)
        {
            if (_activeRelationships.TryGetValue(predefinedData.CharacterAId, out var relationshipA))
            {
                relationshipA.TrustLevel = predefinedData.InitialTrustLevel;
                relationshipA.RespectLevel = predefinedData.InitialRespectLevel;
                relationshipA.InfluenceLevel = predefinedData.InitialInfluenceLevel;
                relationshipA.Type = predefinedData.RelationshipType;
                relationshipA.IsAntagonistic = predefinedData.IsAntagonistic;
                relationshipA.ConflictLevel = predefinedData.ConflictLevel;
                
                // Add shared history
                foreach (var historyItem in predefinedData.SharedHistory)
                {
                    relationshipA.AddSharedMemory(new SharedMemory
                    {
                        MemoryId = Guid.NewGuid().ToString(),
                        Description = historyItem,
                        Timestamp = DateTime.Now.AddDays(-UnityEngine.Random.Range(1, 365)),
                        Type = MemoryType.SharedExperience,
                        EmotionalImpact = 0.5f
                    });
                }
            }
        }
        
        private void UpdateRelationships()
        {
            var startTime = Time.realtimeSinceStartup;
            
            if (_enableBatchUpdates)
            {
                ProcessBatchUpdates();
            }
            else
            {
                ProcessIndividualUpdates();
            }
            
            // Apply natural decay and growth
            ApplyNaturalRelationshipChanges();
            
            // Update performance metrics
            var updateTime = Time.realtimeSinceStartup - startTime;
            UpdatePerformanceMetrics(updateTime);
        }
        
        private void ProcessBatchUpdates()
        {
            var updatesToProcess = new List<RelationshipUpdate>();
            
            // Collect pending updates
            while (_pendingUpdates.Count > 0 && updatesToProcess.Count < 10)
            {
                updatesToProcess.Add(_pendingUpdates.Dequeue());
            }
            
            // Process updates in batch
            foreach (var update in updatesToProcess)
            {
                ApplyRelationshipUpdate(update);
            }
        }
        
        private void ProcessIndividualUpdates()
        {
            if (_pendingUpdates.Count > 0)
            {
                var update = _pendingUpdates.Dequeue();
                ApplyRelationshipUpdate(update);
            }
        }
        
        private void ApplyNaturalRelationshipChanges()
        {
            foreach (var relationship in _activeRelationships.Values)
            {
                // Apply trust decay over time
                if (relationship.TrustLevel > 0)
                {
                    relationship.TrustLevel = Mathf.Max(0, relationship.TrustLevel - _trustDecayRate * Time.deltaTime);
                }
                
                // Apply respect growth for positive relationships
                if (relationship.RespectLevel > 60f)
                {
                    var growthAmount = _respectGrowthMultiplier * Time.deltaTime * 0.01f;
                    relationship.RespectLevel = Mathf.Min(100f, relationship.RespectLevel + growthAmount);
                }
                
                // Update emotional states
                UpdateCharacterEmotionalState(relationship);
            }
        }
        
        private void UpdateCharacterEmotionalState(CharacterRelationship relationship)
        {
            if (!_enableEmotionalMemory) return;
            
            // Calculate emotional state based on recent interactions
            var recentMemories = relationship.GetRecentMemories(TimeSpan.FromHours(24));
            var positiveMemories = recentMemories.Count(m => m.IsPositive);
            var totalMemories = recentMemories.Count;
            
            if (totalMemories > 0)
            {
                var positivityRatio = (float)positiveMemories / totalMemories;
                
                // Update emotional state based on positivity ratio
                relationship.CurrentEmotionalState = positivityRatio switch
                {
                    >= 0.8f => EmotionalState.Happy,
                    >= 0.6f => EmotionalState.Calm,
                    >= 0.4f => EmotionalState.Neutral,
                    >= 0.2f => EmotionalState.Disappointed,
                    _ => EmotionalState.Sad
                };
                
                relationship.EmotionalIntensity = Mathf.Abs(positivityRatio - 0.5f) * 2f;
            }
        }
        
        private void ApplyRelationshipUpdate(RelationshipUpdate update)
        {
            if (!_activeRelationships.TryGetValue(update.CharacterId, out var relationship))
            {
                Debug.LogWarning($"[CharacterRelationshipSystem] Relationship not found for character: {update.CharacterId}");
                return;
            }
            
            // Store previous values
            var previousTrust = relationship.TrustLevel;
            var previousRespect = relationship.RespectLevel;
            var previousInfluence = relationship.InfluenceLevel;
            
            // Apply changes
            relationship.UpdateRelationship(update.Action, update.Context);
            
            // Calculate actual changes
            var trustChange = relationship.TrustLevel - previousTrust;
            var respectChange = relationship.RespectLevel - previousRespect;
            var influenceChange = relationship.InfluenceLevel - previousInfluence;
            
            // Track educational interactions
            if (update.IsEducationalInteraction)
            {
                TrackEducationalInteraction(update, relationship);
            }
            
            // Raise character event
            RaiseCharacterEvent(update, relationship, trustChange, respectChange, influenceChange);
            
            // Update relationship cache if significant change
            if (Mathf.Abs(trustChange) > 5f || Mathf.Abs(respectChange) > 5f || Mathf.Abs(influenceChange) > 5f)
            {
                UpdateRelationshipCacheForCharacter(update.CharacterId);
            }
        }
        
        private void TrackEducationalInteraction(RelationshipUpdate update, CharacterRelationship relationship)
        {
            if (!_educationalMentors.TryGetValue(update.CharacterId, out var mentorData))
            {
                return;
            }
            
            // Validate educational credentials if required
            if (_validateEducationalCredentials)
            {
                if (mentorData.CredibilityLevel < _minimumCredibilityLevel)
                {
                    Debug.LogWarning($"[CharacterRelationshipSystem] Educational interaction with low credibility mentor: {update.CharacterId}");
                    return;
                }
                
                if (_requireScientificAccuracy && !mentorData.IsScientificallyAccurate)
                {
                    Debug.LogWarning($"[CharacterRelationshipSystem] Educational interaction with scientifically inaccurate mentor: {update.CharacterId}");
                    return;
                }
            }
            
            // Create learning interaction record
            var learningInteraction = new LearningInteraction
            {
                InteractionId = Guid.NewGuid().ToString(),
                CharacterId = update.CharacterId,
                Topic = update.EducationalTopic,
                TeachingMethod = update.TeachingMethod,
                Effectiveness = CalculateTeachingEffectiveness(mentorData, update),
                IsScientificallyAccurate = ValidateScientificAccuracy(update.EducationalTopic, mentorData),
                Timestamp = DateTime.Now,
                PlayerComprehension = update.PlayerComprehension,
                LearningOutcome = update.LearningOutcome
            };
            
            // Add to learning history
            if (_learningHistory.TryGetValue(update.CharacterId, out var history))
            {
                history.Add(learningInteraction);
                
                // Maintain history size
                if (history.Count > 100)
                {
                    history.RemoveAt(0);
                }
            }
            
            // Update mentor effectiveness
            UpdateMentorEffectiveness(mentorData, learningInteraction);
            
            // Track teaching effectiveness if enabled
            if (_trackTeachingEffectiveness)
            {
                TrackTeachingEffectivenessMetrics(mentorData, learningInteraction);
            }
        }
        
        private float CalculateTeachingEffectiveness(EducationalMentorData mentorData, RelationshipUpdate update)
        {
            var baseEffectiveness = mentorData.TeachingEffectiveness;
            
            // Adjust for topic expertise
            var topicModifier = 1.0f;
            if (mentorData.ValidatedKnowledgeAreas.Contains(update.EducationalTopic))
            {
                topicModifier = 1.2f; // 20% bonus for validated expertise
            }
            
            // Adjust for relationship level
            if (_activeRelationships.TryGetValue(update.CharacterId, out var relationship))
            {
                var relationshipModifier = Mathf.Lerp(0.8f, 1.2f, relationship.GetOverallRelationshipLevel() / 100f);
                topicModifier *= relationshipModifier;
            }
            
            // Adjust for scientific accuracy
            var accuracyModifier = mentorData.IsScientificallyAccurate ? 1.0f : 0.7f;
            
            return baseEffectiveness * topicModifier * accuracyModifier;
        }
        
        private bool ValidateScientificAccuracy(string topic, EducationalMentorData mentorData)
        {
            if (!_requireScientificAccuracy) return true;
            
            // Check if mentor has validated knowledge in this area
            if (mentorData.ValidatedKnowledgeAreas.Contains(topic))
            {
                return mentorData.IsScientificallyAccurate;
            }
            
            // Check expertise area alignment
            var topicExpertiseMap = new Dictionary<string, CultivationExpertise>
            {
                { "genetics", CultivationExpertise.Genetics },
                { "breeding", CultivationExpertise.Breeding },
                { "nutrition", CultivationExpertise.Nutrition },
                { "lighting", CultivationExpertise.Environmental },
                { "pest_management", CultivationExpertise.IPM },
                { "harvest", CultivationExpertise.PostHarvest },
                { "processing", CultivationExpertise.Processing }
            };
            
            if (topicExpertiseMap.TryGetValue(topic.ToLower(), out var requiredExpertise))
            {
                return mentorData.Expertise.HasFlag(requiredExpertise) && mentorData.IsScientificallyAccurate;
            }
            
            // Default to requiring scientific accuracy
            return mentorData.IsScientificallyAccurate;
        }
        
        private void UpdateMentorEffectiveness(EducationalMentorData mentorData, LearningInteraction interaction)
        {
            // Update cumulative effectiveness
            mentorData.TotalTeachingInteractions++;
            mentorData.CumulativeEffectiveness += interaction.Effectiveness;
            mentorData.AverageEffectiveness = mentorData.CumulativeEffectiveness / mentorData.TotalTeachingInteractions;
            
            // Update topic-specific effectiveness
            if (!mentorData.TopicEffectiveness.ContainsKey(interaction.Topic))
            {
                mentorData.TopicEffectiveness[interaction.Topic] = interaction.Effectiveness;
            }
            else
            {
                // Running average
                var currentAvg = mentorData.TopicEffectiveness[interaction.Topic];
                mentorData.TopicEffectiveness[interaction.Topic] = (currentAvg + interaction.Effectiveness) / 2f;
            }
            
            // Track success/failure
            if (interaction.Effectiveness >= 0.7f && interaction.PlayerComprehension >= 0.7f)
            {
                mentorData.SuccessfulTeachingMoments++;
            }
            else
            {
                mentorData.FailedTeachingMoments++;
            }
        }
        
        private void TrackTeachingEffectivenessMetrics(EducationalMentorData mentorData, LearningInteraction interaction)
        {
            // Add effectiveness data point for trend analysis
            mentorData.EffectivenessHistory.Add(new EventsEffectivenessDataPoint
            {
                EffectivenessValue = interaction.Effectiveness,
                Context = interaction.Topic,
                Timestamp = DateTime.Now
            });
            
            // Maintain history size
            if (mentorData.EffectivenessHistory.Count > 50)
            {
                mentorData.EffectivenessHistory.RemoveAt(0);
            }
            
            // Calculate effectiveness trend
            UpdateEffectivenessTrend(mentorData);
        }
        
        private void UpdateEffectivenessTrend(EducationalMentorData mentorData)
        {
            if (mentorData.EffectivenessHistory.Count < 5) return;
            
            var recentPoints = mentorData.EffectivenessHistory.TakeLast(5).ToList();
            var slope = CalculateEffectivenessSlope(recentPoints);
            
            mentorData.EffectivenessTrend = slope switch
            {
                > 0.1f => EffectivenessTrend.Improving,
                < -0.1f => EffectivenessTrend.Declining,
                _ => EffectivenessTrend.Stable
            };
        }
        
        private float CalculateEffectivenessSlope(List<EventsEffectivenessDataPoint> points)
        {
            if (points.Count < 2) return 0f;
            
            var firstPoint = points.First();
            var lastPoint = points.Last();
            var timeDiff = (lastPoint.Timestamp - firstPoint.Timestamp).TotalMinutes;
            
            if (timeDiff <= 0) return 0f;
            
            return (lastPoint.EffectivenessValue - firstPoint.EffectivenessValue) / (float)timeDiff;
        }
        
        private void RaiseCharacterEvent(RelationshipUpdate update, CharacterRelationship relationship, 
            float trustChange, float respectChange, float influenceChange)
        {
            if (_characterEventChannel == null) return;
            
            var eventData = new CharacterEventData
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = DetermineEventType(update),
                CharacterId = update.CharacterId,
                Timestamp = (float)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds,
                
                HasRelationshipData = true,
                TrustLevel = relationship.TrustLevel,
                RespectLevel = relationship.RespectLevel,
                InfluenceLevel = relationship.InfluenceLevel,
                RelationshipType = relationship.Type,
                InteractionContext = update.Context?.ContextType ?? "general",
                
                HasEmotionalData = _enableEmotionalMemory,
                EmotionalState = relationship.CurrentEmotionalState,
                EmotionalIntensity = relationship.EmotionalIntensity,
                
                HasEducationalContent = update.IsEducationalInteraction,
                IsEducationalMentor = _educationalMentors.ContainsKey(update.CharacterId),
                EducationalTopic = update.EducationalTopic,
                IsScientificallyAccurate = update.IsEducationalInteraction ? 
                    ValidateScientificAccuracy(update.EducationalTopic, _educationalMentors.GetValueOrDefault(update.CharacterId)) : false
            };
            
            if (update.IsEducationalInteraction && _educationalMentors.TryGetValue(update.CharacterId, out var mentorData))
            {
                eventData.ExpertiseAreas = mentorData.Expertise.GetIndividualFlags().ToList();
                eventData.CredibilityLevel = mentorData.CredibilityLevel;
                eventData.TeachingEffectiveness = mentorData.AverageEffectiveness;
            }
            
            _characterEventChannel.Raise(eventData);
        }
        
        private CharacterEventType DetermineEventType(RelationshipUpdate update)
        {
            if (update.IsEducationalInteraction)
            {
                return CharacterEventType.TeachingMoment;
            }
            
            return update.Action.Type switch
            {
                ActionType.Dialogue => CharacterEventType.DialogueCompleted,
                ActionType.Choice => CharacterEventType.ChoiceResponse,
                ActionType.Help => CharacterEventType.TrustGained,
                ActionType.Betray => CharacterEventType.TrustLost,
                ActionType.Support => CharacterEventType.RespectGained,
                _ => CharacterEventType.RelationshipChanged
            };
        }
        
        private void UpdateRelationshipCache()
        {
            foreach (var kvp in _activeRelationships)
            {
                UpdateRelationshipCacheForCharacter(kvp.Key);
            }
        }
        
        private void UpdateRelationshipCacheForCharacter(string characterId)
        {
            if (!_enableRelationshipCaching) return;
            
            if (_activeRelationships.TryGetValue(characterId, out var relationship))
            {
                if (!_relationshipCache.ContainsKey(characterId))
                {
                    _relationshipCache[characterId] = new RelationshipCache
                    {
                        CharacterId = characterId
                    };
                }
                
                var cache = _relationshipCache[characterId];
                cache.CachedLevel = relationship.GetOverallRelationshipLevel();
                cache.CachedStatus = relationship.GetRelationshipStatus();
                cache.LastCacheTime = Time.time;
            }
        }
        
        private void UpdatePerformanceMetrics(float updateTime)
        {
            _totalRelationshipUpdates++;
            
            // Update rolling average
            var weight = 0.1f;
            _averageUpdateTime = (_averageUpdateTime * (1f - weight)) + (updateTime * weight);
        }
        
        private void HandleCharacterEvent(CharacterEventData eventData)
        {
            // Handle incoming character events from other systems
            if (eventData.HasRelationshipData)
            {
                var update = new RelationshipUpdate
                {
                    CharacterId = eventData.CharacterId,
                    Action = new PlayerAction
                    {
                        ActionId = eventData.EventId,
                        Type = ConvertEventTypeToActionType(eventData.EventType),
                        Timestamp = DateTime.Now // Converting from float timestamp
                    },
                    Context = new ActionContext
                    {
                        ContextType = eventData.InteractionContext
                    },
                    IsEducationalInteraction = eventData.HasEducationalContent,
                    EducationalTopic = eventData.EducationalTopic
                };
                
                QueueRelationshipUpdate(update);
            }
        }
        
        private ActionType ConvertEventTypeToActionType(CharacterEventType eventType)
        {
            return eventType switch
            {
                CharacterEventType.DialogueStarted => ActionType.Dialogue,
                CharacterEventType.DialogueCompleted => ActionType.Dialogue,
                CharacterEventType.ChoiceResponse => ActionType.Choice,
                CharacterEventType.TeachingMoment => ActionType.Help,
                CharacterEventType.TrustGained => ActionType.Support,
                CharacterEventType.TrustLost => ActionType.Betray,
                _ => ActionType.Dialogue
            };
        }
        
        #region Public API
        
        /// <summary>
        /// Get relationship data for specific character
        /// </summary>
        public CharacterRelationship GetRelationship(string characterId)
        {
            return _activeRelationships.GetValueOrDefault(characterId);
        }
        
        /// <summary>
        /// Queue a relationship update for processing
        /// </summary>
        public void QueueRelationshipUpdate(RelationshipUpdate update)
        {
            _pendingUpdates.Enqueue(update);
        }
        
        /// <summary>
        /// Get educational mentor data
        /// </summary>
        public EducationalMentorData GetEducationalMentor(string characterId)
        {
            return _educationalMentors.GetValueOrDefault(characterId);
        }
        
        /// <summary>
        /// Get learning history for character
        /// </summary>
        public List<LearningInteraction> GetLearningHistory(string characterId)
        {
            return _learningHistory.GetValueOrDefault(characterId, new List<LearningInteraction>());
        }
        
        /// <summary>
        /// Get relationship statistics
        /// </summary>
        public RelationshipSystemStatistics GetStatistics()
        {
            return new RelationshipSystemStatistics
            {
                TotalActiveRelationships = _activeRelationships.Count,
                EducationalMentorsCount = _educationalMentors.Count,
                TotalUpdatesProcessed = _totalRelationshipUpdates,
                AverageUpdateTime = _averageUpdateTime,
                AverageRelationshipLevel = _activeRelationships.Values.Average(r => r.GetOverallRelationshipLevel()),
                AverageTeachingEffectiveness = _educationalMentors.Values.Average(m => m.AverageEffectiveness)
            };
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_characterEventChannel != null)
            {
                _characterEventChannel.OnEventRaised -= HandleCharacterEvent;
            }
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class RelationshipUpdate
    {
        public string CharacterId;
        public PlayerAction Action;
        public ActionContext Context;
        public bool IsEducationalInteraction;
        public string EducationalTopic;
        public string TeachingMethod;
        public float PlayerComprehension;
        public string LearningOutcome;
    }
    
    [Serializable]
    public class RelationshipCache
    {
        public string CharacterId;
        public float CachedLevel;
        public RelationshipStatus CachedStatus;
        public float LastCacheTime;
    }
    
    [Serializable]
    public class LearningInteraction
    {
        public string InteractionId;
        public string CharacterId;
        public string Topic;
        public string TeachingMethod;
        public float Effectiveness;
        public bool IsScientificallyAccurate;
        public DateTime Timestamp;
        public float PlayerComprehension;
        public string LearningOutcome;
        public float Duration;
    }
    
    [Serializable]
    public class EducationalMentorData
    {
        public string CharacterId;
        public CultivationExpertise Expertise;
        public float CredibilityLevel;
        public List<string> ValidatedKnowledgeAreas = new List<string>();
        public float TeachingEffectiveness;
        public bool IsScientificallyAccurate;
        
        // Teaching statistics
        public int TotalTeachingInteractions;
        public float CumulativeEffectiveness;
        public float AverageEffectiveness;
        public int SuccessfulTeachingMoments;
        public int FailedTeachingMoments;
        
        // Topic-specific data
        public Dictionary<string, float> TopicEffectiveness = new Dictionary<string, float>();
        public List<EventsEffectivenessDataPoint> EffectivenessHistory = new List<EventsEffectivenessDataPoint>();
        public EffectivenessTrend EffectivenessTrend = EffectivenessTrend.Stable;
        
        public float GetTeachingSuccessRate()
        {
            var totalMoments = SuccessfulTeachingMoments + FailedTeachingMoments;
            return totalMoments > 0 ? (float)SuccessfulTeachingMoments / totalMoments : 0f;
        }
    }
    
    [Serializable]
    public class RelationshipSystemStatistics
    {
        public int TotalActiveRelationships;
        public int EducationalMentorsCount;
        public int TotalUpdatesProcessed;
        public float AverageUpdateTime;
        public float AverageRelationshipLevel;
        public float AverageTeachingEffectiveness;
    }
}

// Extension methods for CultivationExpertise enum
public static class CultivationExpertiseExtensions
{
    public static List<CultivationExpertise> GetIndividualFlags(this CultivationExpertise expertise)
    {
        var flags = new List<CultivationExpertise>();
        foreach (CultivationExpertise flag in Enum.GetValues(typeof(CultivationExpertise)))
        {
            if (flag != CultivationExpertise.None && flag != CultivationExpertise.All && expertise.HasFlag(flag))
            {
                flags.Add(flag);
            }
        }
        return flags;
    }
}

// Effectiveness trend enum for educational mentor tracking
public enum EffectivenessTrend
{
    Declining,
    Stable,
    Improving
}