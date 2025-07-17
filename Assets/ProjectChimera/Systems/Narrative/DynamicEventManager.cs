using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Phase 4.2.a: Dynamic Event System for Narrative Progression
    /// Manages story events, character interactions, and world-building moments
    /// Creates emergent storytelling through player actions and world state
    /// </summary>
    public class DynamicEventManager : ChimeraManager
    {
        [Header("Phase 4.2 Configuration")]
        public bool EnableDynamicEvents = true;
        public bool EnablePlayerChoiceEvents = true;
        public bool EnableWorldStateEvents = true;
        public bool EnableCharacterDrivenEvents = true;
        
        [Header("Event Generation Settings")]
        [Range(0f, 1f)] public float BaseEventChance = 0.1f;
        [Range(0f, 5f)] public float PlayerActionMultiplier = 2f;
        [Range(1, 20)] public int MaxConcurrentEvents = 8;
        public float EventCooldownPeriod = 300f; // 5 minutes
        
        [Header("Active Events")]
        [SerializeField] private List<NarrativeEvent> activeEvents = new List<NarrativeEvent>();
        [SerializeField] private List<NarrativeEvent> completedEvents = new List<NarrativeEvent>();
        [SerializeField] private List<NarrativeEvent> pendingEvents = new List<NarrativeEvent>();
        [SerializeField] private Dictionary<string, EventChain> eventChains = new Dictionary<string, EventChain>();
        
        [Header("World State Tracking")]
        [SerializeField] private WorldStateSnapshot currentWorldState = new WorldStateSnapshot();
        [SerializeField] private List<WorldStateChange> recentChanges = new List<WorldStateChange>();
        [SerializeField] private Dictionary<string, float> playerReputation = new Dictionary<string, float>();
        
        [Header("Character Relationships")]
        [SerializeField] private Dictionary<string, CharacterRelationship> characterRelationships = new Dictionary<string, CharacterRelationship>();
        [SerializeField] private List<CharacterInteraction> recentInteractions = new List<CharacterInteraction>();
        
        // Phase 4.2 Data Structures
        [System.Serializable]
        public class NarrativeEvent
        {
            public string EventId;
            public EventType Type;
            public EventPriority Priority;
            public string Title;
            public string Description;
            public DateTime TriggerTime;
            public float Duration;
            public List<string> RequiredConditions;
            public List<string> Characters;
            public List<EventChoice> AvailableChoices;
            public EventOutcome SelectedOutcome;
            public bool IsCompleted;
            public bool HasPlayerChoice;
            public Dictionary<string, object> EventData;
            public List<string> ConsequenceEvents;
        }
        
        [System.Serializable]
        public class EventChoice
        {
            public string ChoiceId;
            public string ChoiceText;
            public string Description;
            public List<string> Requirements;
            public List<EventConsequence> Consequences;
            public float ReputationChange;
            public Dictionary<string, float> SkillRequirements;
            public bool IsAvailable;
            public string UnavailableReason;
        }
        
        [System.Serializable]
        public class EventConsequence
        {
            public ConsequenceType Type;
            public string Target;
            public float Value;
            public string Description;
            public bool IsImmediate;
            public float DelayTime;
        }
        
        [System.Serializable]
        public class EventChain
        {
            public string ChainId;
            public string ChainName;
            public List<string> EventSequence;
            public int CurrentEventIndex;
            public Dictionary<string, object> ChainVariables;
            public bool IsActive;
            public float ProgressPercentage;
            public List<string> UnlockedBranches;
        }
        
        [System.Serializable]
        public class WorldStateSnapshot
        {
            public DateTime SnapshotTime;
            public int PlayerLevel;
            public float TotalWealth;
            public int PlantsGrown;
            public int HarvestsCompleted;
            public float AverageQuality;
            public List<string> UnlockedTechnologies;
            public List<string> CompletedResearch;
            public Dictionary<string, float> MarketReputation;
            public Dictionary<string, int> FacilityStats;
            public List<string> ActiveChallenges;
        }
        
        [System.Serializable]
        public class WorldStateChange
        {
            public DateTime ChangeTime;
            public ChangeType Type;
            public string PropertyName;
            public object OldValue;
            public object NewValue;
            public float ImpactMagnitude;
            public string CauseDescription;
        }
        
        [System.Serializable]
        public class CharacterRelationship
        {
            public string CharacterId;
            public string CharacterName;
            public RelationshipType Type;
            public float TrustLevel;
            public float RespectLevel;
            public float FriendshipLevel;
            public float BusinessLevel;
            public DateTime LastInteraction;
            public List<string> SharedHistory;
            public Dictionary<string, float> TopicAffinities;
            public bool IsAvailableForEvents;
        }
        
        [System.Serializable]
        public class CharacterInteraction
        {
            public string InteractionId;
            public string CharacterId;
            public DateTime InteractionTime;
            public InteractionType Type;
            public string Context;
            public List<string> TopicsDiscussed;
            public float RelationshipChange;
            public EventOutcome Outcome;
            public Dictionary<string, object> InteractionData;
        }
        
        [System.Serializable]
        public class EventOutcome
        {
            public string OutcomeId;
            public string Description;
            public bool WasSuccessful;
            public List<EventConsequence> AppliedConsequences;
            public Dictionary<string, float> ReputationChanges;
            public List<string> UnlockedContent;
            public List<string> TriggeredEvents;
        }
        
        public enum EventType
        {
            StoryProgression,
            CharacterEncounter,
            BusinessOpportunity,
            CrisisEvent,
            Discovery,
            Relationship,
            MarketChange,
            TechnicalChallenge,
            CommunityEvent,
            PersonalMilestone
        }
        
        public enum EventPriority
        {
            Low,
            Medium,
            High,
            Critical,
            MainStory
        }
        
        public enum ConsequenceType
        {
            ResourceChange,
            ReputationChange,
            SkillUnlock,
            TechnologyUnlock,
            RelationshipChange,
            EventTrigger,
            WorldStateChange,
            CharacterAvailabilityChange
        }
        
        public enum ChangeType
        {
            Resource,
            Skill,
            Technology,
            Reputation,
            Relationship,
            Achievement,
            Facility,
            Market
        }
        
        public enum RelationshipType
        {
            Stranger,
            Acquaintance,
            Friend,
            BusinessPartner,
            Mentor,
            Rival,
            Enemy,
            Family
        }
        
        public enum InteractionType
        {
            Conversation,
            Business,
            Collaboration,
            Conflict,
            Teaching,
            Learning,
            Favor,
            Trade
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeEventSystem();
            LoadEventTemplates();
            SetupWorldStateTracking();
            RegisterEventListeners();
            
            Debug.Log($"[Phase 4.2] DynamicEventManager initialized - Events system active");
        }
        
        protected override void OnManagerShutdown()
        {
            UnregisterEventListeners();
            SaveEventHistory();
            SaveWorldState();
            
            Debug.Log($"[Phase 4.2] DynamicEventManager shutdown - {completedEvents.Count} events completed");
        }
        
        private void InitializeEventSystem()
        {
            // Phase 4.2.a: Initialize event generation system
            InvokeRepeating(nameof(ProcessEventGeneration), 60f, 180f); // Every 3 minutes
            InvokeRepeating(nameof(UpdateActiveEvents), 30f, 60f); // Every minute
            InvokeRepeating(nameof(UpdateWorldState), 300f, 600f); // Every 10 minutes
            
            // Initialize character relationships
            InitializeCharacterRelationships();
            
            // Set up event chains
            InitializeEventChains();
        }
        
        private void LoadEventTemplates()
        {
            // Phase 4.2.b: Load pre-defined event templates
            // This would typically load from ScriptableObjects, but for now we'll create some examples
            
            var welcomeEvent = new NarrativeEvent
            {
                EventId = "welcome_001",
                Type = EventType.StoryProgression,
                Priority = EventPriority.MainStory,
                Title = "Welcome to the Industry",
                Description = "You've just acquired your first cultivation facility. An experienced grower offers to show you the ropes.",
                Duration = 600f, // 10 minutes
                RequiredConditions = new List<string> { "FirstLogin", "FacilityAcquired" },
                Characters = new List<string> { "mentor_sam" },
                HasPlayerChoice = true,
                AvailableChoices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        ChoiceId = "accept_help",
                        ChoiceText = "Accept the mentorship",
                        Description = "Learn the basics from an experienced cultivator",
                        Consequences = new List<EventConsequence>
                        {
                            new EventConsequence
                            {
                                Type = ConsequenceType.RelationshipChange,
                                Target = "mentor_sam",
                                Value = 0.3f,
                                Description = "Sam appreciates your willingness to learn"
                            }
                        }
                    },
                    new EventChoice
                    {
                        ChoiceId = "decline_help",
                        ChoiceText = "Politely decline",
                        Description = "Try to figure things out on your own",
                        Consequences = new List<EventConsequence>
                        {
                            new EventConsequence
                            {
                                Type = ConsequenceType.RelationshipChange,
                                Target = "mentor_sam",
                                Value = -0.1f,
                                Description = "Sam respects your independence but feels slightly rebuffed"
                            }
                        }
                    }
                },
                EventData = new Dictionary<string, object>()
            };
            
            pendingEvents.Add(welcomeEvent);
        }
        
        private void InitializeCharacterRelationships()
        {
            // Phase 4.2.c: Initialize key characters
            characterRelationships["mentor_sam"] = new CharacterRelationship
            {
                CharacterId = "mentor_sam",
                CharacterName = "Sam Rodriguez",
                Type = RelationshipType.Stranger,
                TrustLevel = 0.5f,
                RespectLevel = 0.6f,
                FriendshipLevel = 0.2f,
                BusinessLevel = 0.3f,
                LastInteraction = DateTime.MinValue,
                SharedHistory = new List<string>(),
                TopicAffinities = new Dictionary<string, float>
                {
                    ["Cultivation"] = 0.9f,
                    ["Business"] = 0.7f,
                    ["Technology"] = 0.6f,
                    ["PersonalLife"] = 0.4f
                },
                IsAvailableForEvents = true
            };
            
            characterRelationships["supplier_maria"] = new CharacterRelationship
            {
                CharacterId = "supplier_maria",
                CharacterName = "Maria Santos",
                Type = RelationshipType.Stranger,
                TrustLevel = 0.6f,
                RespectLevel = 0.5f,
                FriendshipLevel = 0.1f,
                BusinessLevel = 0.8f,
                LastInteraction = DateTime.MinValue,
                SharedHistory = new List<string>(),
                TopicAffinities = new Dictionary<string, float>
                {
                    ["Business"] = 0.9f,
                    ["Equipment"] = 0.8f,
                    ["Market"] = 0.7f,
                    ["PersonalLife"] = 0.3f
                },
                IsAvailableForEvents = true
            };
        }
        
        private void InitializeEventChains()
        {
            // Phase 4.2.d: Initialize narrative event chains
            eventChains["mentor_storyline"] = new EventChain
            {
                ChainId = "mentor_storyline",
                ChainName = "The Mentor's Guidance",
                EventSequence = new List<string> 
                { 
                    "welcome_001", 
                    "first_lesson_002", 
                    "advanced_techniques_003",
                    "mentor_revelation_004"
                },
                CurrentEventIndex = 0,
                ChainVariables = new Dictionary<string, object>(),
                IsActive = true,
                ProgressPercentage = 0f,
                UnlockedBranches = new List<string>()
            };
        }
        
        private void SetupWorldStateTracking()
        {
            // Phase 4.2.e: Begin tracking world state changes
            currentWorldState = new WorldStateSnapshot
            {
                SnapshotTime = DateTime.Now,
                PlayerLevel = 1,
                TotalWealth = 1000f,
                PlantsGrown = 0,
                HarvestsCompleted = 0,
                AverageQuality = 0f,
                UnlockedTechnologies = new List<string> { "BasicCultivation" },
                CompletedResearch = new List<string>(),
                MarketReputation = new Dictionary<string, float>(),
                FacilityStats = new Dictionary<string, int>(),
                ActiveChallenges = new List<string>()
            };
        }
        
        private void ProcessEventGeneration()
        {
            // Phase 4.2.f: Dynamic event generation based on world state
            if (!EnableDynamicEvents || activeEvents.Count >= MaxConcurrentEvents)
                return;
            
            float eventChance = CalculateEventGenerationChance();
            
            if (UnityEngine.Random.Range(0f, 1f) < eventChance)
            {
                GenerateEvent();
            }
            
            ProcessPendingEvents();
        }
        
        private float CalculateEventGenerationChance()
        {
            // Phase 4.2.g: Calculate event generation probability
            float baseChance = BaseEventChance;
            float playerActionBonus = CalculatePlayerActionBonus();
            float worldStateBonus = CalculateWorldStateBonus();
            float relationshipBonus = CalculateRelationshipBonus();
            
            return Mathf.Clamp01(baseChance + playerActionBonus + worldStateBonus + relationshipBonus);
        }
        
        private float CalculatePlayerActionBonus()
        {
            // Recent player actions increase event chance
            float recentChangeCount = recentChanges.Count(c => (DateTime.Now - c.ChangeTime).TotalMinutes < 30);
            return (recentChangeCount / 10f) * PlayerActionMultiplier * 0.1f;
        }
        
        private float CalculateWorldStateBonus()
        {
            // World state milestones trigger events
            float bonus = 0f;
            
            if (currentWorldState.PlantsGrown % 10 == 0 && currentWorldState.PlantsGrown > 0)
                bonus += 0.2f; // Milestone events
            
            if (currentWorldState.TotalWealth > 10000f)
                bonus += 0.1f; // Wealth-based events
                
            return bonus;
        }
        
        private float CalculateRelationshipBonus()
        {
            // Strong relationships enable more events
            float avgRelationship = characterRelationships.Values.Average(r => 
                (r.TrustLevel + r.RespectLevel + r.FriendshipLevel + r.BusinessLevel) / 4f);
            
            return avgRelationship * 0.1f;
        }
        
        private void GenerateEvent()
        {
            // Phase 4.2.h: Generate appropriate event based on current context
            EventType eventType = SelectEventType();
            string availableCharacter = SelectAvailableCharacter(eventType);
            
            var generatedEvent = CreateContextualEvent(eventType, availableCharacter);
            
            if (generatedEvent != null)
            {
                activeEvents.Add(generatedEvent);
                Debug.Log($"[Phase 4.2] Generated event: {generatedEvent.Title} ({generatedEvent.Type})");
                
                // Fire event notification
                GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("NarrativeEventGenerated", generatedEvent);
            }
        }
        
        private EventType SelectEventType()
        {
            // Weighted selection based on current world state
            var weights = new Dictionary<EventType, float>
            {
                [EventType.CharacterEncounter] = 0.3f,
                [EventType.BusinessOpportunity] = 0.2f,
                [EventType.Discovery] = 0.15f,
                [EventType.CrisisEvent] = 0.1f,
                [EventType.MarketChange] = 0.1f,
                [EventType.TechnicalChallenge] = 0.1f,
                [EventType.PersonalMilestone] = 0.05f
            };
            
            float totalWeight = weights.Values.Sum();
            float random = UnityEngine.Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var kvp in weights)
            {
                currentWeight += kvp.Value;
                if (random <= currentWeight)
                {
                    return kvp.Key;
                }
            }
            
            return EventType.CharacterEncounter;
        }
        
        private string SelectAvailableCharacter(EventType eventType)
        {
            var availableCharacters = characterRelationships.Values
                .Where(r => r.IsAvailableForEvents && 
                           (DateTime.Now - r.LastInteraction).TotalMinutes > EventCooldownPeriod)
                .ToList();
            
            if (availableCharacters.Count == 0)
                return null;
            
            // Select character based on event type affinity
            var bestMatch = availableCharacters
                .OrderByDescending(c => GetCharacterEventAffinity(c, eventType))
                .First();
            
            return bestMatch.CharacterId;
        }
        
        private float GetCharacterEventAffinity(CharacterRelationship character, EventType eventType)
        {
            return eventType switch
            {
                EventType.BusinessOpportunity => character.BusinessLevel,
                EventType.TechnicalChallenge => character.TopicAffinities.GetValueOrDefault("Technology", 0.5f),
                EventType.CharacterEncounter => character.FriendshipLevel,
                _ => 0.5f
            };
        }
        
        private NarrativeEvent CreateContextualEvent(EventType eventType, string characterId)
        {
            // Phase 4.2.i: Create events based on context
            if (string.IsNullOrEmpty(characterId))
                return null;
            
            var character = characterRelationships[characterId];
            var eventId = Guid.NewGuid().ToString("N")[..8];
            
            return eventType switch
            {
                EventType.CharacterEncounter => CreateCharacterEncounterEvent(eventId, character),
                EventType.BusinessOpportunity => CreateBusinessOpportunityEvent(eventId, character),
                EventType.TechnicalChallenge => CreateTechnicalChallengeEvent(eventId, character),
                _ => CreateGenericEvent(eventId, character, eventType)
            };
        }
        
        private NarrativeEvent CreateCharacterEncounterEvent(string eventId, CharacterRelationship character)
        {
            return new NarrativeEvent
            {
                EventId = eventId,
                Type = EventType.CharacterEncounter,
                Priority = EventPriority.Medium,
                Title = $"Encounter with {character.CharacterName}",
                Description = $"You run into {character.CharacterName} while checking on your facility.",
                TriggerTime = DateTime.Now,
                Duration = 300f, // 5 minutes
                Characters = new List<string> { character.CharacterId },
                HasPlayerChoice = true,
                AvailableChoices = GenerateEncounterChoices(character),
                EventData = new Dictionary<string, object>
                {
                    ["characterMood"] = UnityEngine.Random.Range(0.3f, 1f),
                    ["encounterLocation"] = "facility_main"
                }
            };
        }
        
        private NarrativeEvent CreateBusinessOpportunityEvent(string eventId, CharacterRelationship character)
        {
            return new NarrativeEvent
            {
                EventId = eventId,
                Type = EventType.BusinessOpportunity,
                Priority = EventPriority.High,
                Title = $"Business Proposal from {character.CharacterName}",
                Description = $"{character.CharacterName} has a business opportunity they'd like to discuss with you.",
                TriggerTime = DateTime.Now,
                Duration = 600f, // 10 minutes
                Characters = new List<string> { character.CharacterId },
                HasPlayerChoice = true,
                AvailableChoices = GenerateBusinessChoices(character),
                EventData = new Dictionary<string, object>
                {
                    ["proposalValue"] = UnityEngine.Random.Range(1000f, 10000f),
                    ["riskLevel"] = UnityEngine.Random.Range(0.1f, 0.8f)
                }
            };
        }
        
        private NarrativeEvent CreateTechnicalChallengeEvent(string eventId, CharacterRelationship character)
        {
            return new NarrativeEvent
            {
                EventId = eventId,
                Type = EventType.TechnicalChallenge,
                Priority = EventPriority.Medium,
                Title = "Technical Problem",
                Description = $"You're facing a technical issue. {character.CharacterName} might be able to help.",
                TriggerTime = DateTime.Now,
                Duration = 450f, // 7.5 minutes
                Characters = new List<string> { character.CharacterId },
                HasPlayerChoice = true,
                AvailableChoices = GenerateTechnicalChoices(character),
                EventData = new Dictionary<string, object>
                {
                    ["problemComplexity"] = UnityEngine.Random.Range(0.3f, 0.9f),
                    ["urgency"] = UnityEngine.Random.Range(0.2f, 0.8f)
                }
            };
        }
        
        private List<EventChoice> GenerateEncounterChoices(CharacterRelationship character)
        {
            return new List<EventChoice>
            {
                new EventChoice
                {
                    ChoiceId = "friendly_chat",
                    ChoiceText = "Have a friendly chat",
                    Description = "Spend some time catching up",
                    Consequences = new List<EventConsequence>
                    {
                        new EventConsequence
                        {
                            Type = ConsequenceType.RelationshipChange,
                            Target = character.CharacterId,
                            Value = 0.1f,
                            Description = "Improved friendship"
                        }
                    }
                },
                new EventChoice
                {
                    ChoiceId = "keep_brief",
                    ChoiceText = "Keep the conversation brief",
                    Description = "Politely acknowledge them but stay focused on work",
                    Consequences = new List<EventConsequence>()
                }
            };
        }
        
        private List<EventChoice> GenerateBusinessChoices(CharacterRelationship character)
        {
            return new List<EventChoice>
            {
                new EventChoice
                {
                    ChoiceId = "accept_proposal",
                    ChoiceText = "Accept the proposal",
                    Description = "Agree to the business opportunity",
                    Requirements = new List<string> { "MinWealth:5000" },
                    Consequences = new List<EventConsequence>
                    {
                        new EventConsequence
                        {
                            Type = ConsequenceType.ResourceChange,
                            Target = "wealth",
                            Value = -5000f,
                            Description = "Investment cost"
                        },
                        new EventConsequence
                        {
                            Type = ConsequenceType.RelationshipChange,
                            Target = character.CharacterId,
                            Value = 0.3f,
                            Description = "Business partnership strengthened"
                        }
                    }
                },
                new EventChoice
                {
                    ChoiceId = "decline_proposal",
                    ChoiceText = "Decline the proposal",
                    Description = "Politely turn down the opportunity",
                    Consequences = new List<EventConsequence>
                    {
                        new EventConsequence
                        {
                            Type = ConsequenceType.RelationshipChange,
                            Target = character.CharacterId,
                            Value = -0.1f,
                            Description = "Slight disappointment"
                        }
                    }
                }
            };
        }
        
        private List<EventChoice> GenerateTechnicalChoices(CharacterRelationship character)
        {
            return new List<EventChoice>
            {
                new EventChoice
                {
                    ChoiceId = "ask_for_help",
                    ChoiceText = "Ask for their expertise",
                    Description = "Request technical assistance",
                    Consequences = new List<EventConsequence>
                    {
                        new EventConsequence
                        {
                            Type = ConsequenceType.RelationshipChange,
                            Target = character.CharacterId,
                            Value = 0.2f,
                            Description = "They appreciate being asked for help"
                        }
                    }
                },
                new EventChoice
                {
                    ChoiceId = "solve_alone",
                    ChoiceText = "Handle it yourself",
                    Description = "Try to solve the problem independently",
                    Consequences = new List<EventConsequence>()
                }
            };
        }
        
        private void UpdateActiveEvents()
        {
            // Phase 4.2.j: Update and expire active events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                var activeEvent = activeEvents[i];
                
                // Check if event has expired
                if ((DateTime.Now - activeEvent.TriggerTime).TotalSeconds > activeEvent.Duration)
                {
                    ExpireEvent(activeEvent);
                    activeEvents.RemoveAt(i);
                }
            }
        }
        
        private void ExpireEvent(NarrativeEvent narrativeEvent)
        {
            // Handle event expiration
            if (!narrativeEvent.IsCompleted)
            {
                narrativeEvent.IsCompleted = true;
                completedEvents.Add(narrativeEvent);
                
                Debug.Log($"[Phase 4.2] Event expired: {narrativeEvent.Title}");
                
                // Apply default consequences for expired events
                ApplyDefaultConsequences(narrativeEvent);
            }
        }
        
        private void ProcessPendingEvents()
        {
            // Process events that are ready to trigger
            for (int i = pendingEvents.Count - 1; i >= 0; i--)
            {
                var pendingEvent = pendingEvents[i];
                
                if (AreConditionsMet(pendingEvent.RequiredConditions))
                {
                    activeEvents.Add(pendingEvent);
                    pendingEvents.RemoveAt(i);
                    
                    Debug.Log($"[Phase 4.2] Triggered pending event: {pendingEvent.Title}");
                    
                    GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("NarrativeEventTriggered", pendingEvent);
                }
            }
        }
        
        private bool AreConditionsMet(List<string> conditions)
        {
            // Check if all conditions are met for event triggering
            if (conditions == null || conditions.Count == 0)
                return true;
            
            foreach (var condition in conditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            return true;
        }
        
        private bool EvaluateCondition(string condition)
        {
            // Simple condition evaluation - would be expanded in full implementation
            return condition switch
            {
                "FirstLogin" => true, // Assume this is always true for now
                "FacilityAcquired" => true, // Assume this is always true for now
                _ => false
            };
        }
        
        private void UpdateWorldState()
        {
            // Phase 4.2.k: Update world state snapshot
            var newSnapshot = new WorldStateSnapshot
            {
                SnapshotTime = DateTime.Now,
                PlayerLevel = currentWorldState.PlayerLevel,
                TotalWealth = currentWorldState.TotalWealth,
                PlantsGrown = currentWorldState.PlantsGrown,
                HarvestsCompleted = currentWorldState.HarvestsCompleted,
                AverageQuality = currentWorldState.AverageQuality,
                UnlockedTechnologies = new List<string>(currentWorldState.UnlockedTechnologies),
                CompletedResearch = new List<string>(currentWorldState.CompletedResearch),
                MarketReputation = new Dictionary<string, float>(currentWorldState.MarketReputation),
                FacilityStats = new Dictionary<string, int>(currentWorldState.FacilityStats),
                ActiveChallenges = new List<string>(currentWorldState.ActiveChallenges)
            };
            
            // Detect changes and log them
            DetectWorldStateChanges(currentWorldState, newSnapshot);
            currentWorldState = newSnapshot;
        }
        
        private void DetectWorldStateChanges(WorldStateSnapshot oldState, WorldStateSnapshot newState)
        {
            // Compare states and log significant changes
            if (oldState.PlayerLevel != newState.PlayerLevel)
            {
                LogWorldStateChange(ChangeType.Skill, "PlayerLevel", oldState.PlayerLevel, newState.PlayerLevel, "Level progression");
            }
            
            if (Math.Abs(oldState.TotalWealth - newState.TotalWealth) > 100f)
            {
                LogWorldStateChange(ChangeType.Resource, "TotalWealth", oldState.TotalWealth, newState.TotalWealth, "Wealth change");
            }
        }
        
        private void LogWorldStateChange(ChangeType type, string property, object oldValue, object newValue, string description)
        {
            var change = new WorldStateChange
            {
                ChangeTime = DateTime.Now,
                Type = type,
                PropertyName = property,
                OldValue = oldValue,
                NewValue = newValue,
                ImpactMagnitude = CalculateChangeMagnitude(oldValue, newValue),
                CauseDescription = description
            };
            
            recentChanges.Add(change);
            
            // Keep only recent changes (last 24 hours)
            recentChanges.RemoveAll(c => (DateTime.Now - c.ChangeTime).TotalHours > 24);
        }
        
        private float CalculateChangeMagnitude(object oldValue, object newValue)
        {
            if (oldValue is float oldFloat && newValue is float newFloat)
            {
                return Math.Abs(newFloat - oldFloat) / Math.Max(Math.Abs(oldFloat), 1f);
            }
            if (oldValue is int oldInt && newValue is int newInt)
            {
                return Math.Abs(newInt - oldInt) / Math.Max(Math.Abs(oldInt), 1f);
            }
            return 1f; // Default magnitude for non-numeric changes
        }
        
        // Public API methods
        public List<NarrativeEvent> GetActiveEvents() => new List<NarrativeEvent>(activeEvents);
        public List<NarrativeEvent> GetCompletedEvents() => new List<NarrativeEvent>(completedEvents);
        public WorldStateSnapshot GetCurrentWorldState() => currentWorldState;
        public Dictionary<string, CharacterRelationship> GetCharacterRelationships() => new Dictionary<string, CharacterRelationship>(characterRelationships);
        
        public bool MakeEventChoice(string eventId, string choiceId)
        {
            var activeEvent = activeEvents.FirstOrDefault(e => e.EventId == eventId);
            if (activeEvent == null) return false;
            
            var choice = activeEvent.AvailableChoices.FirstOrDefault(c => c.ChoiceId == choiceId);
            if (choice == null || !choice.IsAvailable) return false;
            
            // Apply choice consequences
            ApplyChoiceConsequences(choice, activeEvent);
            
            // Mark event as completed
            activeEvent.IsCompleted = true;
            activeEvent.SelectedOutcome = new EventOutcome
            {
                OutcomeId = choiceId,
                Description = choice.Description,
                WasSuccessful = true,
                AppliedConsequences = choice.Consequences
            };
            
            // Move to completed events
            activeEvents.Remove(activeEvent);
            completedEvents.Add(activeEvent);
            
            Debug.Log($"[Phase 4.2] Player chose '{choice.ChoiceText}' for event '{activeEvent.Title}'");
            
            // Check for event chain progression
            ProcessEventChainProgression(activeEvent);
            
            return true;
        }
        
        private void ApplyChoiceConsequences(EventChoice choice, NarrativeEvent activeEvent)
        {
            foreach (var consequence in choice.Consequences)
            {
                ApplyConsequence(consequence, activeEvent);
            }
        }
        
        private void ApplyConsequence(EventConsequence consequence, NarrativeEvent sourceEvent)
        {
            switch (consequence.Type)
            {
                case ConsequenceType.RelationshipChange:
                    if (characterRelationships.ContainsKey(consequence.Target))
                    {
                        var relationship = characterRelationships[consequence.Target];
                        relationship.TrustLevel = Mathf.Clamp01(relationship.TrustLevel + consequence.Value);
                        relationship.LastInteraction = DateTime.Now;
                    }
                    break;
                    
                case ConsequenceType.ResourceChange:
                    // Would integrate with resource management system
                    Debug.Log($"[Phase 4.2] Resource change: {consequence.Target} by {consequence.Value}");
                    break;
                    
                case ConsequenceType.EventTrigger:
                    // Trigger follow-up events
                    TriggerFollowUpEvent(consequence.Target, sourceEvent);
                    break;
            }
        }
        
        private void TriggerFollowUpEvent(string eventId, NarrativeEvent sourceEvent)
        {
            // Create and trigger follow-up events
            Debug.Log($"[Phase 4.2] Triggering follow-up event: {eventId}");
        }
        
        private void ProcessEventChainProgression(NarrativeEvent completedEvent)
        {
            // Check if this event is part of any chains and advance them
            foreach (var chain in eventChains.Values)
            {
                if (chain.IsActive && chain.CurrentEventIndex < chain.EventSequence.Count)
                {
                    var currentEventId = chain.EventSequence[chain.CurrentEventIndex];
                    if (currentEventId == completedEvent.EventId)
                    {
                        chain.CurrentEventIndex++;
                        chain.ProgressPercentage = (float)chain.CurrentEventIndex / chain.EventSequence.Count;
                        
                        Debug.Log($"[Phase 4.2] Advanced event chain '{chain.ChainName}' to {chain.ProgressPercentage:P0}");
                        
                        // Trigger next event in chain if available
                        if (chain.CurrentEventIndex < chain.EventSequence.Count)
                        {
                            var nextEventId = chain.EventSequence[chain.CurrentEventIndex];
                            ScheduleChainEvent(nextEventId, chain);
                        }
                        else
                        {
                            // Chain completed
                            chain.IsActive = false;
                            Debug.Log($"[Phase 4.2] Completed event chain: {chain.ChainName}");
                        }
                    }
                }
            }
        }
        
        private void ScheduleChainEvent(string eventId, EventChain chain)
        {
            // Schedule the next event in the chain
            Debug.Log($"[Phase 4.2] Scheduling next event in chain: {eventId}");
        }
        
        private void ApplyDefaultConsequences(NarrativeEvent expiredEvent)
        {
            // Apply consequences for events that expire without player action
            foreach (var character in expiredEvent.Characters)
            {
                if (characterRelationships.ContainsKey(character))
                {
                    var relationship = characterRelationships[character];
                    relationship.TrustLevel = Mathf.Clamp01(relationship.TrustLevel - 0.05f); // Small penalty for ignoring
                }
            }
        }
        
        private NarrativeEvent CreateGenericEvent(string eventId, CharacterRelationship character, EventType eventType)
        {
            return new NarrativeEvent
            {
                EventId = eventId,
                Type = eventType,
                Priority = EventPriority.Low,
                Title = $"Event with {character.CharacterName}",
                Description = "A general interaction opportunity.",
                TriggerTime = DateTime.Now,
                Duration = 300f,
                Characters = new List<string> { character.CharacterId },
                HasPlayerChoice = false,
                AvailableChoices = new List<EventChoice>(),
                EventData = new Dictionary<string, object>()
            };
        }
        
        private void RegisterEventListeners()
        {
            // Register for game events that might trigger narrative events
        }
        
        private void UnregisterEventListeners()
        {
            // Unregister event listeners
        }
        
        private void SaveEventHistory()
        {
            // Save event history to persistent storage
            Debug.Log($"[Phase 4.2] Saving event history: {completedEvents.Count} completed events");
        }
        
        private void SaveWorldState()
        {
            // Save current world state
            Debug.Log($"[Phase 4.2] Saving world state snapshot");
        }
        
        public void TriggerManualEvent(string eventId)
        {
            // Allow manual triggering of specific events for testing
            Debug.Log($"[Phase 4.2] Manually triggering event: {eventId}");
        }
    }
}