using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Phase 4.2.b: NPC Interaction and Relationship Management System
    /// Manages complex NPC personalities, dialogue systems, and evolving relationships
    /// Creates meaningful character interactions that impact gameplay and story
    /// </summary>
    public class NPCInteractionManager : ChimeraManager
    {
        [Header("Phase 4.2.b Configuration")]
        public bool EnableNPCInteractions = true;
        public bool EnableRelationshipTracking = true;
        public bool EnableDynamicDialogue = true;
        public bool EnablePersonalitySystem = true;
        
        [Header("Interaction Settings")]
        [Range(0f, 1f)] public float BaseInteractionChance = 0.2f;
        [Range(0f, 5f)] public float RelationshipInfluence = 3f;
        [Range(1, 10)] public int MaxDailyInteractions = 5;
        public float ConversationCooldown = 1800f; // 30 minutes
        
        [Header("Active NPCs")]
        [SerializeField] private List<NPCCharacter> activeNPCs = new List<NPCCharacter>();
        [SerializeField] private List<OngoingConversation> activeConversations = new List<OngoingConversation>();
        [SerializeField] private List<InteractionHistory> interactionHistory = new List<InteractionHistory>();
        [SerializeField] private Dictionary<string, NPCRelationshipState> relationshipStates = new Dictionary<string, NPCRelationshipState>();
        
        [Header("Dialogue System")]
        [SerializeField] private DialogueDatabase dialogueDatabase = new DialogueDatabase();
        [SerializeField] private List<DialogueContext> availableContexts = new List<DialogueContext>();
        [SerializeField] private Dictionary<string, NPCMood> currentMoods = new Dictionary<string, NPCMood>();
        
        // Phase 4.2.b Data Structures
        [System.Serializable]
        public class NPCCharacter
        {
            public string NPCId;
            public string Name;
            public string Role;
            public string Location;
            public NPCPersonality Personality;
            public NPCBackground Background;
            public List<string> Expertise;
            public List<string> Interests;
            public Vector3 Position;
            public bool IsAvailable;
            public DateTime LastInteraction;
            public float BaseOpinion;
            public Dictionary<string, float> TopicKnowledge;
            public List<string> ActiveQuests;
            public NPCBehaviorPattern BehaviorPattern;
        }
        
        [System.Serializable]
        public class NPCPersonality
        {
            public float Extroversion;        // 0-1: Shy to Outgoing
            public float Agreeableness;      // 0-1: Competitive to Cooperative
            public float Conscientiousness;  // 0-1: Spontaneous to Organized
            public float Neuroticism;        // 0-1: Calm to Anxious
            public float Openness;           // 0-1: Traditional to Innovative
            public float Helpfulness;        // 0-1: Self-focused to Helpful
            public float Trustworthiness;    // 0-1: Skeptical to Trusting
            public List<PersonalityTrait> DominantTraits;
        }
        
        [System.Serializable]
        public class NPCBackground
        {
            public int Age;
            public string Education;
            public List<string> PreviousJobs;
            public string OriginLocation;
            public List<string> LifeEvents;
            public Dictionary<string, string> Relationships;
            public List<string> Achievements;
            public List<string> Regrets;
            public string CurrentGoals;
        }
        
        [System.Serializable]
        public class NPCBehaviorPattern
        {
            public List<string> PreferredTopics;
            public List<string> AvoidedTopics;
            public float ConversationLength;
            public ConversationStyle Style;
            public List<string> CommonPhrases;
            public float OpinionChangeRate;
            public bool RemembersDetails;
            public int AttentionSpan;
        }
        
        [System.Serializable]
        public class OngoingConversation
        {
            public string ConversationId;
            public string NPCId;
            public DateTime StartTime;
            public ConversationType Type;
            public List<DialogueExchange> Exchanges;
            public ConversationContext Context;
            public float PlayerSatisfaction;
            public float NPCSatisfaction;
            public Dictionary<string, object> ConversationData;
            public bool IsActive;
            public string CurrentTopic;
        }
        
        [System.Serializable]
        public class DialogueExchange
        {
            public DateTime Timestamp;
            public DialogueSpeaker Speaker;
            public string Message;
            public DialogueTone Tone;
            public List<string> AvailableResponses;
            public string SelectedResponse;
            public float EmotionalImpact;
            public Dictionary<string, object> ExchangeData;
        }
        
        [System.Serializable]
        public class NPCRelationshipState
        {
            public string NPCId;
            public RelationshipLevel Level;
            public float TrustScore;
            public float RespectScore;
            public float FriendshipScore;
            public float RomanceScore;
            public float BusinessScore;
            public DateTime RelationshipStarted;
            public List<string> SharedExperiences;
            public List<string> Conflicts;
            public List<string> FavorsDone;
            public List<string> FavorsReceived;
            public float ReputationWithTheirContacts;
            public RelationshipTrend CurrentTrend;
        }
        
        [System.Serializable]
        public class DialogueContext
        {
            public string ContextId;
            public string Name;
            public List<string> RequiredConditions;
            public List<DialogueNode> DialogueNodes;
            public Dictionary<string, string> ContextVariables;
            public bool IsRepeatable;
            public float Priority;
        }
        
        [System.Serializable]
        public class DialogueNode
        {
            public string NodeId;
            public string NPCLine;
            public List<DialogueOption> PlayerOptions;
            public List<string> Conditions;
            public List<DialogueConsequence> Consequences;
            public string NextNodeId;
            public bool EndsConversation;
        }
        
        [System.Serializable]
        public class DialogueOption
        {
            public string OptionId;
            public string OptionText;
            public List<string> Requirements;
            public DialogueTone Tone;
            public float RelationshipImpact;
            public List<DialogueConsequence> Consequences;
            public string LeadsToNodeId;
            public bool IsAvailable;
        }
        
        [System.Serializable]
        public class DialogueConsequence
        {
            public ConsequenceType Type;
            public string Target;
            public float Value;
            public string Description;
            public bool IsImmediate;
        }
        
        [System.Serializable]
        public class DialogueDatabase
        {
            public Dictionary<string, List<DialogueContext>> NPCDialogues;
            public Dictionary<string, DialogueTemplate> Templates;
            public List<string> CommonGreetings;
            public List<string> CommonFarewells;
            public Dictionary<string, List<string>> TopicResponses;
        }
        
        [System.Serializable]
        public class DialogueTemplate
        {
            public string TemplateId;
            public string TemplateName;
            public List<string> VariableSlots;
            public string BaseStructure;
            public List<DialogueTone> SupportedTones;
        }
        
        [System.Serializable]
        public class NPCMood
        {
            public string NPCId;
            public MoodState CurrentMood;
            public float Intensity;
            public List<string> MoodCauses;
            public DateTime MoodStartTime;
            public float MoodDuration;
            public Dictionary<string, float> TopicModifiers;
        }
        
        [System.Serializable]
        public class InteractionHistory
        {
            public string InteractionId;
            public string NPCId;
            public DateTime InteractionTime;
            public InteractionType Type;
            public float Duration;
            public List<string> TopicsDiscussed;
            public InteractionOutcome Outcome;
            public float PlayerSatisfaction;
            public float NPCSatisfaction;
            public Dictionary<string, float> RelationshipChanges;
            public string Summary;
        }
        
        [System.Serializable]
        public class ConversationContext
        {
            public string Location;
            public List<string> NearbyNPCs;
            public string CurrentActivity;
            public Dictionary<string, object> EnvironmentalFactors;
            public List<string> RecentEvents;
            public TimeOfDay TimeOfDay;
            public WeatherCondition Weather;
        }
        
        public enum ConversationType
        {
            Casual,
            Business,
            Tutorial,
            Quest,
            Romance,
            Conflict,
            Gossip,
            Technical,
            Philosophical
        }
        
        public enum DialogueSpeaker
        {
            Player,
            NPC,
            System
        }
        
        public enum DialogueTone
        {
            Friendly,
            Professional,
            Flirty,
            Aggressive,
            Respectful,
            Dismissive,
            Curious,
            Sympathetic,
            Humorous,
            Serious
        }
        
        public enum RelationshipLevel
        {
            Stranger,
            Acquaintance,
            Friend,
            GoodFriend,
            BestFriend,
            RomanticInterest,
            Partner,
            BusinessPartner,
            Rival,
            Enemy
        }
        
        public enum PersonalityTrait
        {
            Optimistic,
            Pessimistic,
            Analytical,
            Creative,
            Practical,
            Idealistic,
            Cautious,
            Adventurous,
            Independent,
            Collaborative
        }
        
        public enum ConversationStyle
        {
            DirectAndBrief,
            DetailedAndThorough,
            CasualAndRambling,
            FormalAndStructured,
            EmotionalAndExpressive,
            LogicalAndFactual
        }
        
        public enum MoodState
        {
            Happy,
            Sad,
            Angry,
            Excited,
            Worried,
            Confident,
            Frustrated,
            Content,
            Suspicious,
            Grateful
        }
        
        public enum RelationshipTrend
        {
            Improving,
            Declining,
            Stable,
            Volatile,
            NewRelationship
        }
        
        public enum InteractionType
        {
            Greeting,
            Conversation,
            Business,
            Favor,
            Conflict,
            Romance,
            Teaching,
            Learning,
            Gossip,
            Goodbye
        }
        
        public enum InteractionOutcome
        {
            Positive,
            Negative,
            Neutral,
            Mixed,
            Romantic,
            Business,
            Educational,
            Conflicting
        }
        
        public enum TimeOfDay
        {
            EarlyMorning,
            Morning,
            Afternoon,
            Evening,
            Night,
            LateNight
        }
        
        public enum WeatherCondition
        {
            Sunny,
            Cloudy,
            Rainy,
            Stormy,
            Foggy
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeNPCSystem();
            LoadNPCData();
            SetupDialogueSystem();
            RegisterEventListeners();
            
            Debug.Log($"[Phase 4.2.b] NPCInteractionManager initialized - {activeNPCs.Count} NPCs active");
        }
        
        protected override void OnManagerShutdown()
        {
            SaveInteractionHistory();
            SaveRelationshipStates();
            UnregisterEventListeners();
            
            Debug.Log($"[Phase 4.2.b] NPCInteractionManager shutdown - {interactionHistory.Count} interactions recorded");
        }
        
        private void InitializeNPCSystem()
        {
            // Phase 4.2.b: Initialize NPC interaction system
            InvokeRepeating(nameof(UpdateNPCStates), 30f, 120f); // Every 2 minutes
            InvokeRepeating(nameof(ProcessRandomInteractions), 180f, 300f); // Every 5 minutes
            InvokeRepeating(nameof(UpdateMoods), 600f, 900f); // Every 15 minutes
            
            InitializeDialogueDatabase();
        }
        
        private void LoadNPCData()
        {
            // Phase 4.2.b: Load NPC character data
            CreateDefaultNPCs();
            InitializeRelationshipStates();
        }
        
        private void CreateDefaultNPCs()
        {
            // Create Sam Rodriguez - The Mentor
            var sam = new NPCCharacter
            {
                NPCId = "mentor_sam",
                Name = "Sam Rodriguez",
                Role = "Experienced Cultivator",
                Location = "Local Greenhouse",
                Personality = new NPCPersonality
                {
                    Extroversion = 0.7f,
                    Agreeableness = 0.8f,
                    Conscientiousness = 0.9f,
                    Neuroticism = 0.2f,
                    Openness = 0.6f,
                    Helpfulness = 0.9f,
                    Trustworthiness = 0.8f,
                    DominantTraits = new List<PersonalityTrait> { PersonalityTrait.Practical, PersonalityTrait.Collaborative }
                },
                Background = new NPCBackground
                {
                    Age = 52,
                    Education = "Agricultural Science Degree",
                    PreviousJobs = new List<string> { "Farm Hand", "Greenhouse Manager", "Agricultural Consultant" },
                    OriginLocation = "Rural California",
                    CurrentGoals = "Share knowledge with new cultivators"
                },
                Expertise = new List<string> { "Cannabis Cultivation", "Organic Growing", "Pest Management" },
                Interests = new List<string> { "Sustainable Agriculture", "Teaching", "Local Community" },
                BaseOpinion = 0.6f,
                IsAvailable = true,
                TopicKnowledge = new Dictionary<string, float>
                {
                    ["Cultivation"] = 0.95f,
                    ["Business"] = 0.7f,
                    ["Technology"] = 0.5f,
                    ["PersonalLife"] = 0.6f
                },
                BehaviorPattern = new NPCBehaviorPattern
                {
                    PreferredTopics = new List<string> { "Growing techniques", "Plant health", "Industry history" },
                    AvoidedTopics = new List<string> { "Personal finances", "Family drama" },
                    ConversationLength = 0.7f, // Tends to have longer conversations
                    Style = ConversationStyle.DetailedAndThorough,
                    OpinionChangeRate = 0.3f,
                    RemembersDetails = true,
                    AttentionSpan = 8
                }
            };
            activeNPCs.Add(sam);
            
            // Create Maria Santos - The Supplier
            var maria = new NPCCharacter
            {
                NPCId = "supplier_maria",
                Name = "Maria Santos",
                Role = "Equipment Supplier",
                Location = "Supply Store",
                Personality = new NPCPersonality
                {
                    Extroversion = 0.8f,
                    Agreeableness = 0.6f,
                    Conscientiousness = 0.8f,
                    Neuroticism = 0.3f,
                    Openness = 0.7f,
                    Helpfulness = 0.7f,
                    Trustworthiness = 0.8f,
                    DominantTraits = new List<PersonalityTrait> { PersonalityTrait.Practical, PersonalityTrait.Independent }
                },
                Background = new NPCBackground
                {
                    Age = 38,
                    Education = "Business Administration",
                    PreviousJobs = new List<string> { "Sales Representative", "Small Business Owner" },
                    OriginLocation = "Urban Area",
                    CurrentGoals = "Build successful equipment business"
                },
                Expertise = new List<string> { "Equipment Sales", "Business Operations", "Customer Service" },
                Interests = new List<string> { "Business Growth", "Technology", "Customer Success" },
                BaseOpinion = 0.5f,
                IsAvailable = true,
                TopicKnowledge = new Dictionary<string, float>
                {
                    ["Business"] = 0.9f,
                    ["Equipment"] = 0.85f,
                    ["Cultivation"] = 0.4f,
                    ["Technology"] = 0.7f
                },
                BehaviorPattern = new NPCBehaviorPattern
                {
                    PreferredTopics = new List<string> { "Equipment", "Business opportunities", "Market trends" },
                    AvoidedTopics = new List<string> { "Personal relationships", "Politics" },
                    ConversationLength = 0.5f, // More efficient conversations
                    Style = ConversationStyle.DirectAndBrief,
                    OpinionChangeRate = 0.5f,
                    RemembersDetails = true,
                    AttentionSpan = 6
                }
            };
            activeNPCs.Add(maria);
            
            // Create Alex Kim - The Researcher
            var alex = new NPCCharacter
            {
                NPCId = "researcher_alex",
                Name = "Dr. Alex Kim",
                Role = "Cannabis Researcher",
                Location = "Research Lab",
                Personality = new NPCPersonality
                {
                    Extroversion = 0.4f,
                    Agreeableness = 0.7f,
                    Conscientiousness = 0.9f,
                    Neuroticism = 0.4f,
                    Openness = 0.95f,
                    Helpfulness = 0.8f,
                    Trustworthiness = 0.9f,
                    DominantTraits = new List<PersonalityTrait> { PersonalityTrait.Analytical, PersonalityTrait.Idealistic }
                },
                Background = new NPCBackground
                {
                    Age = 31,
                    Education = "PhD in Plant Biology",
                    PreviousJobs = new List<string> { "Graduate Researcher", "Lab Assistant" },
                    OriginLocation = "University Town",
                    CurrentGoals = "Advance cannabis science and genetics"
                },
                Expertise = new List<string> { "Genetics", "Plant Biology", "Research Methods", "Data Analysis" },
                Interests = new List<string> { "Scientific Discovery", "Genetics", "Environmental Impact" },
                BaseOpinion = 0.6f,
                IsAvailable = true,
                TopicKnowledge = new Dictionary<string, float>
                {
                    ["Science"] = 0.95f,
                    ["Genetics"] = 0.9f,
                    ["Technology"] = 0.8f,
                    ["Business"] = 0.3f
                },
                BehaviorPattern = new NPCBehaviorPattern
                {
                    PreferredTopics = new List<string> { "Research", "Genetics", "Scientific methods", "Data" },
                    AvoidedTopics = new List<string> { "Business politics", "Non-scientific claims" },
                    ConversationLength = 0.8f, // Loves detailed discussions
                    Style = ConversationStyle.LogicalAndFactual,
                    OpinionChangeRate = 0.2f, // Changes opinion slowly, based on evidence
                    RemembersDetails = true,
                    AttentionSpan = 10
                }
            };
            activeNPCs.Add(alex);
        }
        
        private void InitializeRelationshipStates()
        {
            foreach (var npc in activeNPCs)
            {
                relationshipStates[npc.NPCId] = new NPCRelationshipState
                {
                    NPCId = npc.NPCId,
                    Level = RelationshipLevel.Stranger,
                    TrustScore = npc.BaseOpinion,
                    RespectScore = 0.5f,
                    FriendshipScore = 0.1f,
                    RomanceScore = 0f,
                    BusinessScore = 0.3f,
                    RelationshipStarted = DateTime.Now,
                    SharedExperiences = new List<string>(),
                    Conflicts = new List<string>(),
                    FavorsDone = new List<string>(),
                    FavorsReceived = new List<string>(),
                    ReputationWithTheirContacts = 0.5f,
                    CurrentTrend = RelationshipTrend.NewRelationship
                };
                
                // Set initial mood
                currentMoods[npc.NPCId] = new NPCMood
                {
                    NPCId = npc.NPCId,
                    CurrentMood = MoodState.Content,
                    Intensity = 0.5f,
                    MoodCauses = new List<string> { "Normal day" },
                    MoodStartTime = DateTime.Now,
                    MoodDuration = 3600f, // 1 hour
                    TopicModifiers = new Dictionary<string, float>()
                };
            }
        }
        
        private void SetupDialogueSystem()
        {
            InitializeDialogueDatabase();
            CreateDialogueContexts();
        }
        
        private void InitializeDialogueDatabase()
        {
            dialogueDatabase = new DialogueDatabase
            {
                NPCDialogues = new Dictionary<string, List<DialogueContext>>(),
                Templates = new Dictionary<string, DialogueTemplate>(),
                CommonGreetings = new List<string>
                {
                    "Hello there!",
                    "Good to see you!",
                    "Hey, how's it going?",
                    "Nice to meet you.",
                    "Welcome!"
                },
                CommonFarewells = new List<string>
                {
                    "See you later!",
                    "Take care!",
                    "Until next time.",
                    "Good luck!",
                    "Bye for now."
                },
                TopicResponses = new Dictionary<string, List<string>>
                {
                    ["Weather"] = new List<string>
                    {
                        "Nice day today, isn't it?",
                        "Perfect growing weather!",
                        "Hope this weather holds up."
                    },
                    ["Business"] = new List<string>
                    {
                        "How's business treating you?",
                        "The market's been interesting lately.",
                        "Always looking for good opportunities."
                    }
                }
            };
        }
        
        private void CreateDialogueContexts()
        {
            // Create basic dialogue contexts for different NPCs
            foreach (var npc in activeNPCs)
            {
                var contexts = new List<DialogueContext>();
                
                // First meeting context
                var firstMeeting = new DialogueContext
                {
                    ContextId = $"{npc.NPCId}_first_meeting",
                    Name = "First Meeting",
                    RequiredConditions = new List<string> { "FirstMeeting" },
                    DialogueNodes = CreateFirstMeetingDialogue(npc),
                    IsRepeatable = false,
                    Priority = 1f
                };
                contexts.Add(firstMeeting);
                
                // General conversation context
                var general = new DialogueContext
                {
                    ContextId = $"{npc.NPCId}_general",
                    Name = "General Conversation",
                    RequiredConditions = new List<string>(),
                    DialogueNodes = CreateGeneralDialogue(npc),
                    IsRepeatable = true,
                    Priority = 0.5f
                };
                contexts.Add(general);
                
                dialogueDatabase.NPCDialogues[npc.NPCId] = contexts;
            }
        }
        
        private List<DialogueNode> CreateFirstMeetingDialogue(NPCCharacter npc)
        {
            var nodes = new List<DialogueNode>();
            
            var greeting = new DialogueNode
            {
                NodeId = "greeting",
                NPCLine = GetPersonalizedGreeting(npc),
                PlayerOptions = new List<DialogueOption>
                {
                    new DialogueOption
                    {
                        OptionId = "friendly_response",
                        OptionText = "Nice to meet you too!",
                        Tone = DialogueTone.Friendly,
                        RelationshipImpact = 0.1f,
                        LeadsToNodeId = "introduction",
                        IsAvailable = true
                    },
                    new DialogueOption
                    {
                        OptionId = "professional_response",
                        OptionText = "Thank you. I'm here about business.",
                        Tone = DialogueTone.Professional,
                        RelationshipImpact = 0.05f,
                        LeadsToNodeId = "business_intro",
                        IsAvailable = true
                    }
                },
                NextNodeId = "introduction"
            };
            nodes.Add(greeting);
            
            return nodes;
        }
        
        private List<DialogueNode> CreateGeneralDialogue(NPCCharacter npc)
        {
            var nodes = new List<DialogueNode>();
            
            var mainNode = new DialogueNode
            {
                NodeId = "main",
                NPCLine = GetPersonalizedGeneralLine(npc),
                PlayerOptions = GenerateTopicOptions(npc),
                NextNodeId = "end"
            };
            nodes.Add(mainNode);
            
            return nodes;
        }
        
        private string GetPersonalizedGreeting(NPCCharacter npc)
        {
            var personality = npc.Personality;
            
            if (personality.Extroversion > 0.7f)
            {
                return $"Hey there! I'm {npc.Name}. Great to meet a new face around here!";
            }
            else if (personality.Extroversion < 0.3f)
            {
                return $"Oh, hello. I'm {npc.Name}. Nice to meet you.";
            }
            else
            {
                return $"Hi, I'm {npc.Name}. Welcome to the area.";
            }
        }
        
        private string GetPersonalizedGeneralLine(NPCCharacter npc)
        {
            var mood = currentMoods[npc.NPCId];
            var relationship = relationshipStates[npc.NPCId];
            
            if (mood.CurrentMood == MoodState.Happy && relationship.FriendshipScore > 0.6f)
            {
                return "Good to see you again! What brings you by today?";
            }
            else if (mood.CurrentMood == MoodState.Worried)
            {
                return "Oh, hello. I've got a lot on my mind, but what can I do for you?";
            }
            else
            {
                return "Hello again. What's on your mind?";
            }
        }
        
        private List<DialogueOption> GenerateTopicOptions(NPCCharacter npc)
        {
            var options = new List<DialogueOption>();
            
            // Add topic options based on NPC expertise and interests
            foreach (var topic in npc.Expertise.Take(3))
            {
                options.Add(new DialogueOption
                {
                    OptionId = $"ask_about_{topic.ToLower()}",
                    OptionText = $"I'd like to know about {topic.ToLower()}.",
                    Tone = DialogueTone.Curious,
                    RelationshipImpact = 0.05f,
                    IsAvailable = true
                });
            }
            
            // Add general options
            options.Add(new DialogueOption
            {
                OptionId = "how_are_you",
                OptionText = "How are you doing?",
                Tone = DialogueTone.Friendly,
                RelationshipImpact = 0.02f,
                IsAvailable = true
            });
            
            return options;
        }
        
        private void UpdateNPCStates()
        {
            // Phase 4.2.c: Update NPC availability and states
            foreach (var npc in activeNPCs)
            {
                UpdateNPCAvailability(npc);
                UpdateNPCLocation(npc);
            }
        }
        
        private void UpdateNPCAvailability(NPCCharacter npc)
        {
            // Check if enough time has passed since last interaction
            var timeSinceLastInteraction = DateTime.Now - npc.LastInteraction;
            npc.IsAvailable = timeSinceLastInteraction.TotalSeconds > ConversationCooldown;
            
            // Factor in NPC's behavior pattern
            if (npc.BehaviorPattern.Style == ConversationStyle.DirectAndBrief)
            {
                npc.IsAvailable = npc.IsAvailable && timeSinceLastInteraction.TotalSeconds > ConversationCooldown * 0.7f;
            }
        }
        
        private void UpdateNPCLocation(NPCCharacter npc)
        {
            // Simple location updates - in full implementation, this would be more sophisticated
            // For now, NPCs stay in their designated locations
        }
        
        private void ProcessRandomInteractions()
        {
            // Phase 4.2.d: Generate random interaction opportunities
            if (!EnableNPCInteractions) return;
            
            var availableNPCs = activeNPCs.Where(npc => npc.IsAvailable).ToList();
            if (availableNPCs.Count == 0) return;
            
            foreach (var npc in availableNPCs.Take(2)) // Limit to 2 random interactions
            {
                float interactionChance = CalculateInteractionChance(npc);
                
                if (UnityEngine.Random.Range(0f, 1f) < interactionChance)
                {
                    TriggerRandomInteraction(npc);
                }
            }
        }
        
        private float CalculateInteractionChance(NPCCharacter npc)
        {
            float baseChance = BaseInteractionChance;
            var relationship = relationshipStates[npc.NPCId];
            var mood = currentMoods[npc.NPCId];
            
            // Relationship influence
            float relationshipBonus = (relationship.FriendshipScore + relationship.TrustScore) * 0.1f;
            
            // Personality influence
            float personalityBonus = npc.Personality.Extroversion * 0.1f;
            
            // Mood influence
            float moodMultiplier = mood.CurrentMood switch
            {
                MoodState.Happy => 1.2f,
                MoodState.Excited => 1.3f,
                MoodState.Sad => 0.7f,
                MoodState.Angry => 0.5f,
                _ => 1f
            };
            
            return (baseChance + relationshipBonus + personalityBonus) * moodMultiplier;
        }
        
        private void TriggerRandomInteraction(NPCCharacter npc)
        {
            var interactionType = DetermineInteractionType(npc);
            var context = CreateInteractionContext(npc);
            
            var interaction = new InteractionHistory
            {
                InteractionId = Guid.NewGuid().ToString("N")[..8],
                NPCId = npc.NPCId,
                InteractionTime = DateTime.Now,
                Type = interactionType,
                Duration = 0f, // Will be updated when interaction ends
                TopicsDiscussed = new List<string>(),
                RelationshipChanges = new Dictionary<string, float>()
            };
            
            interactionHistory.Add(interaction);
            npc.LastInteraction = DateTime.Now;
            
            Debug.Log($"[Phase 4.2.b] Random interaction triggered with {npc.Name}: {interactionType}");
            
            // Fire interaction event
            GameManager.Instance.GetManager<EventManager>()?.TriggerEvent("NPCInteractionTriggered", new { NPCId = npc.NPCId, Type = interactionType });
        }
        
        private InteractionType DetermineInteractionType(NPCCharacter npc)
        {
            var relationship = relationshipStates[npc.NPCId];
            var weights = new Dictionary<InteractionType, float>();
            
            // Base weights
            weights[InteractionType.Greeting] = 0.3f;
            weights[InteractionType.Conversation] = 0.4f;
            weights[InteractionType.Business] = npc.TopicKnowledge.GetValueOrDefault("Business", 0.1f) * 0.3f;
            
            // Relationship-based weights
            if (relationship.FriendshipScore > 0.6f)
            {
                weights[InteractionType.Favor] = 0.2f;
                weights[InteractionType.Gossip] = 0.1f;
            }
            
            if (relationship.BusinessScore > 0.7f)
            {
                weights[InteractionType.Business] += 0.2f;
            }
            
            // Select weighted random
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
            
            return InteractionType.Greeting;
        }
        
        private ConversationContext CreateInteractionContext(NPCCharacter npc)
        {
            return new ConversationContext
            {
                Location = npc.Location,
                NearbyNPCs = activeNPCs.Where(n => n.Location == npc.Location && n.NPCId != npc.NPCId).Select(n => n.NPCId).ToList(),
                CurrentActivity = "General",
                EnvironmentalFactors = new Dictionary<string, object>(),
                RecentEvents = new List<string>(),
                TimeOfDay = GetCurrentTimeOfDay(),
                Weather = WeatherCondition.Sunny // Default
            };
        }
        
        private TimeOfDay GetCurrentTimeOfDay()
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                >= 5 and < 8 => TimeOfDay.EarlyMorning,
                >= 8 and < 12 => TimeOfDay.Morning,
                >= 12 and < 17 => TimeOfDay.Afternoon,
                >= 17 and < 20 => TimeOfDay.Evening,
                >= 20 and < 23 => TimeOfDay.Night,
                _ => TimeOfDay.LateNight
            };
        }
        
        private void UpdateMoods()
        {
            // Phase 4.2.e: Update NPC moods based on recent events and personality
            foreach (var npc in activeNPCs)
            {
                UpdateNPCMood(npc);
            }
        }
        
        private void UpdateNPCMood(NPCCharacter npc)
        {
            var currentMood = currentMoods[npc.NPCId];
            var relationship = relationshipStates[npc.NPCId];
            
            // Check if mood should change based on time
            if ((DateTime.Now - currentMood.MoodStartTime).TotalSeconds > currentMood.MoodDuration)
            {
                // Generate new mood based on personality and recent interactions
                var newMoodState = GenerateNewMood(npc, relationship);
                
                currentMood.CurrentMood = newMoodState;
                currentMood.Intensity = UnityEngine.Random.Range(0.3f, 0.8f);
                currentMood.MoodStartTime = DateTime.Now;
                currentMood.MoodDuration = UnityEngine.Random.Range(1800f, 7200f); // 30 minutes to 2 hours
                
                Debug.Log($"[Phase 4.2.b] {npc.Name} mood changed to {newMoodState}");
            }
        }
        
        private MoodState GenerateNewMood(NPCCharacter npc, NPCRelationshipState relationship)
        {
            // Generate mood based on personality traits
            var personality = npc.Personality;
            
            if (personality.Neuroticism > 0.7f && UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                return UnityEngine.Random.Range(0f, 1f) < 0.5f ? MoodState.Worried : MoodState.Frustrated;
            }
            
            if (relationship.CurrentTrend == RelationshipTrend.Improving)
            {
                return UnityEngine.Random.Range(0f, 1f) < 0.7f ? MoodState.Happy : MoodState.Content;
            }
            
            if (relationship.CurrentTrend == RelationshipTrend.Declining)
            {
                return UnityEngine.Random.Range(0f, 1f) < 0.6f ? MoodState.Sad : MoodState.Frustrated;
            }
            
            // Default to content with some variation
            var moodOptions = new[] { MoodState.Content, MoodState.Happy, MoodState.Confident };
            return moodOptions[UnityEngine.Random.Range(0, moodOptions.Length)];
        }
        
        // Public API Methods
        public List<NPCCharacter> GetAvailableNPCs()
        {
            return activeNPCs.Where(npc => npc.IsAvailable).ToList();
        }
        
        public NPCCharacter GetNPC(string npcId)
        {
            return activeNPCs.FirstOrDefault(npc => npc.NPCId == npcId);
        }
        
        public NPCRelationshipState GetRelationshipState(string npcId)
        {
            return relationshipStates.GetValueOrDefault(npcId);
        }
        
        public bool StartConversation(string npcId, ConversationType type = ConversationType.Casual)
        {
            var npc = GetNPC(npcId);
            if (npc == null || !npc.IsAvailable) return false;
            
            var conversation = new OngoingConversation
            {
                ConversationId = Guid.NewGuid().ToString("N")[..8],
                NPCId = npcId,
                StartTime = DateTime.Now,
                Type = type,
                Exchanges = new List<DialogueExchange>(),
                Context = CreateInteractionContext(npc),
                IsActive = true,
                ConversationData = new Dictionary<string, object>()
            };
            
            activeConversations.Add(conversation);
            npc.IsAvailable = false;
            
            Debug.Log($"[Phase 4.2.b] Started {type} conversation with {npc.Name}");
            
            return true;
        }
        
        public bool ContinueConversation(string conversationId, string playerResponse)
        {
            var conversation = activeConversations.FirstOrDefault(c => c.ConversationId == conversationId && c.IsActive);
            if (conversation == null) return false;
            
            var npc = GetNPC(conversation.NPCId);
            if (npc == null) return false;
            
            // Process player response
            var exchange = new DialogueExchange
            {
                Timestamp = DateTime.Now,
                Speaker = DialogueSpeaker.Player,
                Message = playerResponse,
                Tone = DialogueTone.Friendly, // Would be determined by response analysis
                EmotionalImpact = CalculateEmotionalImpact(playerResponse, npc),
                ExchangeData = new Dictionary<string, object>()
            };
            
            conversation.Exchanges.Add(exchange);
            
            // Generate NPC response
            var npcResponse = GenerateNPCResponse(conversation, exchange, npc);
            conversation.Exchanges.Add(npcResponse);
            
            return true;
        }
        
        public void EndConversation(string conversationId)
        {
            var conversation = activeConversations.FirstOrDefault(c => c.ConversationId == conversationId);
            if (conversation == null) return;
            
            var npc = GetNPC(conversation.NPCId);
            if (npc != null)
            {
                npc.IsAvailable = true;
                npc.LastInteraction = DateTime.Now;
            }
            
            // Process conversation outcomes
            ProcessConversationOutcome(conversation);
            
            conversation.IsActive = false;
            activeConversations.Remove(conversation);
            
            Debug.Log($"[Phase 4.2.b] Ended conversation with {npc?.Name} after {conversation.Exchanges.Count} exchanges");
        }
        
        private float CalculateEmotionalImpact(string playerResponse, NPCCharacter npc)
        {
            // Simple emotional impact calculation - would be more sophisticated in full implementation
            var positiveWords = new[] { "thank", "please", "appreciate", "great", "wonderful", "help" };
            var negativeWords = new[] { "no", "can't", "won't", "hate", "terrible", "stupid" };
            
            var lowerResponse = playerResponse.ToLower();
            
            float impact = 0f;
            foreach (var word in positiveWords)
            {
                if (lowerResponse.Contains(word))
                    impact += 0.1f;
            }
            
            foreach (var word in negativeWords)
            {
                if (lowerResponse.Contains(word))
                    impact -= 0.1f;
            }
            
            return Mathf.Clamp(impact, -1f, 1f);
        }
        
        private DialogueExchange GenerateNPCResponse(OngoingConversation conversation, DialogueExchange playerExchange, NPCCharacter npc)
        {
            var mood = currentMoods[npc.NPCId];
            var relationship = relationshipStates[npc.NPCId];
            
            // Generate contextual response based on NPC personality and mood
            string responseText = GenerateContextualResponse(playerExchange.Message, npc, mood, relationship);
            
            return new DialogueExchange
            {
                Timestamp = DateTime.Now,
                Speaker = DialogueSpeaker.NPC,
                Message = responseText,
                Tone = DetermineNPCTone(npc, mood, playerExchange.EmotionalImpact),
                EmotionalImpact = playerExchange.EmotionalImpact * 0.5f, // NPC responds to player emotion
                ExchangeData = new Dictionary<string, object>()
            };
        }
        
        private string GenerateContextualResponse(string playerMessage, NPCCharacter npc, NPCMood mood, NPCRelationshipState relationship)
        {
            // Simple response generation - would use more sophisticated NLP in full implementation
            var lowerMessage = playerMessage.ToLower();
            
            // Check for specific topics
            foreach (var expertise in npc.Expertise)
            {
                if (lowerMessage.Contains(expertise.ToLower()))
                {
                    return $"Oh, you're interested in {expertise.ToLower()}! I'd be happy to share what I know about that.";
                }
            }
            
            // Mood-based responses
            if (mood.CurrentMood == MoodState.Happy)
            {
                return "That sounds great! I'm always excited to talk about these things.";
            }
            else if (mood.CurrentMood == MoodState.Worried)
            {
                return "I appreciate you asking, though I have to admit I'm a bit preoccupied today.";
            }
            
            // Default responses based on personality
            if (npc.Personality.Extroversion > 0.7f)
            {
                return "Absolutely! I love talking about this stuff. What specifically interests you?";
            }
            else
            {
                return "Sure, I can help with that. What would you like to know?";
            }
        }
        
        private DialogueTone DetermineNPCTone(NPCCharacter npc, NPCMood mood, float playerEmotionalImpact)
        {
            // Determine tone based on personality, mood, and player's emotional impact
            if (playerEmotionalImpact > 0.3f)
            {
                return npc.Personality.Agreeableness > 0.6f ? DialogueTone.Friendly : DialogueTone.Respectful;
            }
            else if (playerEmotionalImpact < -0.3f)
            {
                return npc.Personality.Agreeableness > 0.6f ? DialogueTone.Sympathetic : DialogueTone.Dismissive;
            }
            
            return mood.CurrentMood switch
            {
                MoodState.Happy => DialogueTone.Friendly,
                MoodState.Excited => DialogueTone.Friendly,
                MoodState.Sad => DialogueTone.Sympathetic,
                MoodState.Angry => DialogueTone.Aggressive,
                MoodState.Confident => DialogueTone.Professional,
                _ => DialogueTone.Respectful
            };
        }
        
        private void ProcessConversationOutcome(OngoingConversation conversation)
        {
            var npc = GetNPC(conversation.NPCId);
            var relationship = relationshipStates[conversation.NPCId];
            
            // Calculate conversation satisfaction and relationship changes
            float overallImpact = conversation.Exchanges
                .Where(e => e.Speaker == DialogueSpeaker.Player)
                .Average(e => e.EmotionalImpact);
            
            // Update relationship based on conversation
            if (overallImpact > 0.2f)
            {
                relationship.FriendshipScore = Mathf.Clamp01(relationship.FriendshipScore + 0.05f);
                relationship.TrustScore = Mathf.Clamp01(relationship.TrustScore + 0.02f);
                relationship.CurrentTrend = RelationshipTrend.Improving;
            }
            else if (overallImpact < -0.2f)
            {
                relationship.FriendshipScore = Mathf.Clamp01(relationship.FriendshipScore - 0.03f);
                relationship.TrustScore = Mathf.Clamp01(relationship.TrustScore - 0.05f);
                relationship.CurrentTrend = RelationshipTrend.Declining;
            }
            
            // Update relationship level based on scores
            UpdateRelationshipLevel(relationship);
            
            // Record interaction in history
            var interaction = new InteractionHistory
            {
                InteractionId = conversation.ConversationId,
                NPCId = conversation.NPCId,
                InteractionTime = conversation.StartTime,
                Type = InteractionType.Conversation,
                Duration = (float)(DateTime.Now - conversation.StartTime).TotalSeconds,
                Outcome = overallImpact > 0 ? InteractionOutcome.Positive : 
                         overallImpact < 0 ? InteractionOutcome.Negative : InteractionOutcome.Neutral,
                PlayerSatisfaction = Mathf.Clamp01(0.5f + overallImpact),
                NPCSatisfaction = Mathf.Clamp01(0.5f + overallImpact * 0.8f),
                RelationshipChanges = new Dictionary<string, float>
                {
                    ["Friendship"] = relationship.FriendshipScore,
                    ["Trust"] = relationship.TrustScore
                },
                Summary = $"Conversation with {npc?.Name}: {conversation.Exchanges.Count} exchanges"
            };
            
            interactionHistory.Add(interaction);
        }
        
        private void UpdateRelationshipLevel(NPCRelationshipState relationship)
        {
            float totalScore = (relationship.TrustScore + relationship.FriendshipScore + relationship.RespectScore) / 3f;
            
            var newLevel = totalScore switch
            {
                >= 0.9f => RelationshipLevel.BestFriend,
                >= 0.7f => RelationshipLevel.GoodFriend,
                >= 0.5f => RelationshipLevel.Friend,
                >= 0.3f => RelationshipLevel.Acquaintance,
                _ => RelationshipLevel.Stranger
            };
            
            if (newLevel != relationship.Level)
            {
                Debug.Log($"[Phase 4.2.b] Relationship with {relationship.NPCId} upgraded to {newLevel}");
                relationship.Level = newLevel;
            }
        }
        
        private void RegisterEventListeners()
        {
            // Register for game events that might affect NPC relationships
        }
        
        private void UnregisterEventListeners()
        {
            // Unregister event listeners
        }
        
        private void SaveInteractionHistory()
        {
            Debug.Log($"[Phase 4.2.b] Saving interaction history: {interactionHistory.Count} interactions");
        }
        
        private void SaveRelationshipStates()
        {
            Debug.Log($"[Phase 4.2.b] Saving relationship states for {relationshipStates.Count} NPCs");
        }
    }
}