using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;
using MemoryType = ProjectChimera.Data.Narrative.MemoryType;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Comprehensive character development and dialogue management system for Project Chimera.
    /// Handles character interactions, relationship progression, dialogue trees, and personality-driven
    /// conversations with cannabis cultivation expertise and educational storytelling integration.
    /// </summary>
    public class CharacterDialogueManager : ChimeraManager, IChimeraManager
    {
        [Header("Character Configuration")]
        [SerializeField] private CharacterDatabaseSO _characterDatabase;
        [SerializeField] private DialogueLibrarySO _dialogueLibrary;
        [SerializeField] private NarrativeConfigSO _narrativeConfig;
        [SerializeField] private CharacterDialogueConfigSO _dialogueConfig;
        
        [Header("Character Development")]
        [SerializeField] private bool _enableCharacterDevelopment = true;
        [SerializeField] private bool _enableRelationshipProgression = true;
        [SerializeField] private bool _enablePersonalityEvolution = true;
        [SerializeField] private bool _enableMemorySystem = true;
        [SerializeField] private float _relationshipChangeRate = 1.0f;
        
        [Header("Dialogue System")]
        [SerializeField] private bool _enableDialogueSystem = true;
        [SerializeField] private bool _enableVoiceActing = false;
        [SerializeField] private bool _enableEmotionalResponses = true;
        [SerializeField] private bool _enableContextualDialogue = true;
        [SerializeField] private float _dialogueSpeed = 1.0f;
        [SerializeField] private float _typingSpeed = 50f;
        
        [Header("Conversation Management")]
        [SerializeField] private bool _enableConversationTrees = true;
        [SerializeField] private bool _enableBranchingDialogue = true;
        [SerializeField] private bool _enableChoiceMemory = true;
        [SerializeField] private int _maxActiveConversations = 5;
        [SerializeField] private int _maxConversationHistory = 100;
        
        [Header("Character AI and Behavior")]
        [SerializeField] private bool _enableCharacterAI = true;
        [SerializeField] private bool _enableProactiveDialogue = true;
        [SerializeField] private bool _enableMoodInfluence = true;
        [SerializeField] private bool _enableExpertiseSystem = true;
        [SerializeField] private float _aiUpdateInterval = 2f;
        
        [Header("Event Channels")]
        [SerializeField] private NarrativeEventChannelSO _onCharacterIntroduced;
        [SerializeField] private NarrativeEventChannelSO _onDialogueStarted;
        [SerializeField] private NarrativeEventChannelSO _onDialogueCompleted;
        [SerializeField] private NarrativeEventChannelSO _onRelationshipChanged;
        [SerializeField] private NarrativeEventChannelSO _onCharacterDeveloped;
        
        // Core character systems
        private CharacterInstanceManager _characterInstanceManager;
        private DialogueTreeProcessor _dialogueProcessor;
        private RelationshipManager _relationshipManager;
        private CharacterMemorySystem _memorySystem;
        private PersonalityEngine _personalityEngine;
        
        // Character state management
        private Dictionary<string, ActiveCharacterInstance> _activeCharacters = new Dictionary<string, ActiveCharacterInstance>();
        private Dictionary<string, ActiveConversation> _activeConversations = new Dictionary<string, ActiveConversation>();
        private Dictionary<string, CharacterRelationshipData> _characterRelationships = new Dictionary<string, CharacterRelationshipData>();
        private Dictionary<string, CharacterMemoryProfile> _characterMemories = new Dictionary<string, CharacterMemoryProfile>();
        
        // Dialogue state management
        private Dictionary<string, DialogueState> _dialogueStates = new Dictionary<string, DialogueState>();
        private Queue<DialogueRequest> _dialogueQueue = new Queue<DialogueRequest>();
        private List<CompletedConversation> _conversationHistory = new List<CompletedConversation>();
        
        // Character development tracking
        private Dictionary<string, CharacterDevelopmentProfile> _developmentProfiles = new Dictionary<string, CharacterDevelopmentProfile>();
        private Dictionary<string, PersonalityEvolutionData> _personalityEvolution = new Dictionary<string, PersonalityEvolutionData>();
        
        // Performance and analytics
        private CharacterDialogueMetrics _metrics = new CharacterDialogueMetrics();
        private DialogueAnalyticsEngine _analyticsEngine;
        private ConversationCache _conversationCache;
        
        // System state
        private Coroutine _characterUpdateCoroutine;
        private Coroutine _dialogueProcessingCoroutine;
        private Coroutine _relationshipUpdateCoroutine;
        private DateTime _lastCharacterUpdate;
        private bool _isSystemActive = false;
        
        #region ChimeraManager Implementation
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        protected override void OnManagerInitialize()
        {
            LogInfo("Initializing Character Dialogue Manager...");
            
            if (!ValidateConfiguration())
            {
                LogError("Character Dialogue Manager configuration validation failed");
                return;
            }
            
            InitializeCharacterSystems();
            InitializeDialogueSystems();
            InitializeRelationshipSystem();
            InitializeCharacterDevelopment();
            
            LoadCharacterData();
            StartCharacterSystems();
            
            LogInfo("Character Dialogue Manager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo("Shutting down Character Dialogue Manager...");
            
            StopCharacterSystems();
            SaveCharacterData();
            DisposeCharacterResources();
            
            LogInfo("Character Dialogue Manager shutdown complete");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_isSystemActive)
                return;
            
            // Process immediate dialogue events
            ProcessImmediateDialogueEvents();
            
            // Update character states
            UpdateCharacterStates();
            
            // Update active conversations
            UpdateActiveConversations();
            
            // Update character metrics
            UpdateCharacterMetrics();
        }
        
        private bool ValidateConfiguration()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            if (_characterDatabase == null)
            {
                validationErrors.Add("Character Database SO is not assigned");
                isValid = false;
            }
            
            if (_dialogueLibrary == null)
            {
                validationErrors.Add("Dialogue Library SO is not assigned");
                isValid = false;
            }
            
            if (_narrativeConfig == null)
            {
                validationErrors.Add("Narrative Config SO is not assigned");
                isValid = false;
            }
            
            if (_dialogueConfig == null)
            {
                validationErrors.Add("Character Dialogue Config SO is not assigned");
                isValid = false;
            }
            
            // Validate event channels
            if (_onCharacterIntroduced == null)
            {
                validationErrors.Add("Character Introduced event channel is not assigned");
                isValid = false;
            }
            
            // Validate settings
            if (_relationshipChangeRate <= 0f)
            {
                validationErrors.Add("Relationship change rate must be greater than 0");
                isValid = false;
            }
            
            if (_dialogueSpeed <= 0f)
            {
                validationErrors.Add("Dialogue speed must be greater than 0");
                isValid = false;
            }
            
            if (!isValid)
            {
                LogError($"Character Dialogue Manager validation failed: {string.Join(", ", validationErrors)}");
            }
            else
            {
                LogInfo("Character Dialogue Manager validation passed");
            }
            
            return isValid;
        }
        
        #endregion
        
        #region Initialization Methods
        
        private void InitializeCharacterSystems()
        {
            // Initialize character instance manager
            _characterInstanceManager = new CharacterInstanceManager(_characterDatabase, _narrativeConfig);
            _characterInstanceManager.OnCharacterStateChanged += HandleCharacterStateChanged;
            
            // Initialize personality engine
            if (_enablePersonalityEvolution)
            {
                _personalityEngine = new PersonalityEngine(_narrativeConfig);
                _personalityEngine.OnPersonalityChanged += HandlePersonalityChanged;
            }
            
            // Initialize memory system
            if (_enableMemorySystem)
            {
                _memorySystem = new CharacterMemorySystem(_narrativeConfig);
                _memorySystem.OnMemoryAdded += HandleMemoryAdded;
            }
            
            // Initialize analytics engine
            _analyticsEngine = new DialogueAnalyticsEngine(_narrativeConfig);
            
            LogInfo("Character systems initialized");
        }
        
        private void InitializeDialogueSystems()
        {
            // Initialize dialogue processor
            if (_enableDialogueSystem)
            {
                _dialogueProcessor = new DialogueTreeProcessor(_dialogueLibrary, _dialogueConfig);
                _dialogueProcessor.EnableBranchingDialogue(_enableBranchingDialogue);
                _dialogueProcessor.EnableEmotionalResponses(_enableEmotionalResponses);
                _dialogueProcessor.EnableContextualDialogue(_enableContextualDialogue);
                _dialogueProcessor.SetDialogueSpeed(_dialogueSpeed);
                _dialogueProcessor.OnDialogueCompleted += HandleDialogueCompleted;
                _dialogueProcessor.OnChoiceSelected += HandleDialogueChoiceSelected;
            }
            
            // Initialize conversation cache
            _conversationCache = new ConversationCache(_dialogueConfig);
            
            LogInfo("Dialogue systems initialized");
        }
        
        private void InitializeRelationshipSystem()
        {
            if (_enableRelationshipProgression)
            {
                _relationshipManager = new RelationshipManager(_characterDatabase, _narrativeConfig);
                _relationshipManager.SetChangeRate(_relationshipChangeRate);
                _relationshipManager.OnRelationshipChanged += HandleRelationshipChanged;
                _relationshipManager.OnRelationshipMilestone += HandleRelationshipMilestone;
                
                LogInfo("Relationship system initialized");
            }
        }
        
        private void InitializeCharacterDevelopment()
        {
            if (_enableCharacterDevelopment)
            {
                // Load character development profiles
                foreach (var character in _characterDatabase.Characters)
                {
                    if (character != null)
                    {
                        var developmentProfile = new CharacterDevelopmentProfile
                        {
                            CharacterId = character.CharacterId,
                            InitialPersonalityId = character.PersonalityId,
                            DevelopmentStage = CharacterDevelopmentStage.Initial,
                            ExperiencePoints = 0f,
                            DevelopmentMilestones = new List<string>()
                        };
                        
                        _developmentProfiles[character.CharacterId] = developmentProfile;
                    }
                }
                
                LogInfo("Character development system initialized");
            }
        }
        
        #endregion
        
        #region Character System Management
        
        private void StartCharacterSystems()
        {
            // Start character update loop
            if (_characterUpdateCoroutine == null)
            {
                _characterUpdateCoroutine = StartCoroutine(CharacterUpdateLoop());
            }
            
            // Start dialogue processing loop
            if (_enableDialogueSystem && _dialogueProcessingCoroutine == null)
            {
                _dialogueProcessingCoroutine = StartCoroutine(DialogueProcessingLoop());
            }
            
            // Start relationship update loop
            if (_enableRelationshipProgression && _relationshipUpdateCoroutine == null)
            {
                _relationshipUpdateCoroutine = StartCoroutine(RelationshipUpdateLoop());
            }
            
            _isSystemActive = true;
            LogInfo("Character systems started");
        }
        
        private void StopCharacterSystems()
        {
            _isSystemActive = false;
            
            if (_characterUpdateCoroutine != null)
            {
                StopCoroutine(_characterUpdateCoroutine);
                _characterUpdateCoroutine = null;
            }
            
            if (_dialogueProcessingCoroutine != null)
            {
                StopCoroutine(_dialogueProcessingCoroutine);
                _dialogueProcessingCoroutine = null;
            }
            
            if (_relationshipUpdateCoroutine != null)
            {
                StopCoroutine(_relationshipUpdateCoroutine);
                _relationshipUpdateCoroutine = null;
            }
            
            LogInfo("Character systems stopped");
        }
        
        private IEnumerator CharacterUpdateLoop()
        {
            while (_isSystemActive)
            {
                yield return new WaitForSeconds(_aiUpdateInterval);
                
                try
                {
                    UpdateCharacterAI();
                    ProcessCharacterDevelopment();
                    UpdatePersonalityEvolution();
                    
                    _lastCharacterUpdate = DateTime.Now;
                }
                catch (Exception ex)
                {
                    LogError($"Error in character update loop: {ex.Message}");
                    _metrics.CharacterUpdateErrors++;
                }
            }
        }
        
        private IEnumerator DialogueProcessingLoop()
        {
            while (_isSystemActive)
            {
                yield return new WaitForSeconds(0.1f);
                
                try
                {
                    ProcessDialogueQueue();
                    UpdateDialogueStates();
                    CleanupCompletedDialogues();
                }
                catch (Exception ex)
                {
                    LogError($"Error in dialogue processing loop: {ex.Message}");
                    _metrics.DialogueProcessingErrors++;
                }
            }
        }
        
        private IEnumerator RelationshipUpdateLoop()
        {
            while (_isSystemActive)
            {
                yield return new WaitForSeconds(5f);
                
                try
                {
                    UpdateAllRelationships();
                    ProcessRelationshipEvents();
                }
                catch (Exception ex)
                {
                    LogError($"Error in relationship update loop: {ex.Message}");
                    _metrics.RelationshipUpdateErrors++;
                }
            }
        }
        
        #endregion
        
        #region Character Management
        
        public void IntroduceCharacter(string characterId)
        {
            var character = _characterDatabase.GetCharacter(characterId);
            if (character == null)
            {
                LogWarning($"Character not found: {characterId}");
                return;
            }
            
            // Check if already introduced
            if (_activeCharacters.ContainsKey(characterId))
            {
                LogInfo($"Character already introduced: {character.CharacterName}");
                return;
            }
            
            // Create active character instance
            var characterInstance = new ActiveCharacterInstance
            {
                CharacterId = characterId,
                Character = character,
                IntroductionTime = DateTime.Now,
                IsActive = true,
                CurrentMood = EmotionalState.Neutral,
                InteractionCount = 0,
                LastInteraction = DateTime.MinValue,
                DialogueHistory = new List<string>()
            };
            
            _activeCharacters[characterId] = characterInstance;
            
            // Initialize character memory
            if (_enableMemorySystem)
            {
                _characterMemories[characterId] = new CharacterMemoryProfile
                {
                    CharacterId = characterId,
                    ShortTermMemory = new List<MemoryItem>(),
                    LongTermMemory = new List<MemoryItem>(),
                    PersonalityMemory = new Dictionary<string, float>()
                };
            }
            
            // Trigger character introduction event
            _onCharacterIntroduced?.RaiseEvent(new NarrativeEventMessage
            {
                Type = NarrativeEventType.CharacterIntroduction,
                CharacterId = characterId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "character", character },
                    { "isFirstMeeting", true }
                }
            });
            
            LogInfo($"Introduced character: {character.CharacterName}");
            _metrics.CharactersIntroduced++;
        }
        
        public void StartConversation(string characterId, string dialogueId, Dictionary<string, object> context = null)
        {
            if (!_activeCharacters.TryGetValue(characterId, out var characterInstance))
            {
                LogWarning($"Character not active: {characterId}");
                return;
            }
            
            if (_activeConversations.Count >= _maxActiveConversations)
            {
                LogWarning("Maximum active conversations reached");
                return;
            }
            
            var dialogue = _dialogueLibrary.GetDialogue(dialogueId);
            if (dialogue == null)
            {
                LogWarning($"Dialogue not found: {dialogueId}");
                return;
            }
            
            var conversationId = Guid.NewGuid().ToString("N");
            var conversation = new ActiveConversation
            {
                ConversationId = conversationId,
                CharacterId = characterId,
                DialogueId = dialogueId,
                StartTime = DateTime.Now,
                CurrentNodeId = dialogue.StartNodeId,
                Context = context ?? new Dictionary<string, object>(),
                IsActive = true,
                DialogueHistory = new List<DialogueEntry>()
            };
            
            _activeConversations[conversationId] = conversation;
            
            // Update character state
            characterInstance.LastInteraction = DateTime.Now;
            characterInstance.InteractionCount++;
            
            // Trigger dialogue started event
            _onDialogueStarted?.RaiseEvent(new NarrativeEventMessage
            {
                Type = NarrativeEventType.DialogueStarted,
                CharacterId = characterId,
                DialogueId = dialogueId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "conversationId", conversationId },
                    { "context", context }
                }
            });
            
            LogInfo($"Started conversation with {characterInstance.Character.CharacterName}");
            _metrics.ConversationsStarted++;
        }
        
        #endregion
        
        #region Dialogue Management
        
        public void ProcessDialogueChoice(string conversationId, int choiceIndex)
        {
            if (!_activeConversations.TryGetValue(conversationId, out var conversation))
            {
                LogWarning($"Conversation not found: {conversationId}");
                return;
            }
            
            if (_dialogueProcessor != null)
            {
                _dialogueProcessor.ProcessChoice(conversation, choiceIndex);
                
                // Record choice in character memory
                if (_enableMemorySystem && _characterMemories.TryGetValue(conversation.CharacterId, out var memory))
                {
                    var memoryItem = new MemoryItem
                    {
                        Type = ProjectChimera.Data.Narrative.MemoryType.DialogueChoice,
                        Content = $"Choice {choiceIndex} in dialogue {conversation.DialogueId}",
                        Timestamp = DateTime.Now,
                        EmotionalWeight = 0.5f
                    };
                    
                    _memorySystem.AddMemory(conversation.CharacterId, memoryItem);
                }
                
                _metrics.DialogueChoicesMade++;
            }
        }
        
        private void ProcessDialogueQueue()
        {
            var processedCount = 0;
            var maxProcessPerFrame = 5;
            
            while (_dialogueQueue.Count > 0 && processedCount < maxProcessPerFrame)
            {
                var request = _dialogueQueue.Dequeue();
                ProcessDialogueRequest(request);
                processedCount++;
            }
        }
        
        private void ProcessDialogueRequest(DialogueRequest request)
        {
            switch (request.RequestType)
            {
                case DialogueRequestType.StartConversation:
                    StartConversation(request.CharacterId, request.DialogueId, request.Context);
                    break;
                case DialogueRequestType.ProcessChoice:
                    ProcessDialogueChoice(request.ConversationId, request.ChoiceIndex);
                    break;
                case DialogueRequestType.EndConversation:
                    EndConversation(request.ConversationId);
                    break;
            }
        }
        
        public void EndConversation(string conversationId)
        {
            if (!_activeConversations.TryGetValue(conversationId, out var conversation))
                return;
            
            conversation.IsActive = false;
            conversation.EndTime = DateTime.Now;
            
            // Add to conversation history
            var completedConversation = new CompletedConversation
            {
                ConversationId = conversationId,
                CharacterId = conversation.CharacterId,
                DialogueId = conversation.DialogueId,
                StartTime = conversation.StartTime,
                EndTime = conversation.EndTime,
                TotalDuration = (conversation.EndTime - conversation.StartTime).TotalSeconds,
                DialogueEntries = conversation.DialogueHistory.ToList(),
                Context = conversation.Context
            };
            
            _conversationHistory.Add(completedConversation);
            
            // Maintain history size
            while (_conversationHistory.Count > _maxConversationHistory)
            {
                _conversationHistory.RemoveAt(0);
            }
            
            // Remove from active conversations
            _activeConversations.Remove(conversationId);
            
            // Trigger dialogue completed event
            _onDialogueCompleted?.RaiseEvent(new NarrativeEventMessage
            {
                Type = NarrativeEventType.DialogueCompleted,
                CharacterId = conversation.CharacterId,
                DialogueId = conversation.DialogueId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "conversationId", conversationId },
                    { "duration", completedConversation.TotalDuration }
                }
            });
            
            LogInfo($"Ended conversation: {conversationId}");
            _metrics.ConversationsCompleted++;
        }
        
        #endregion
        
        #region Relationship Management
        
        public void ModifyRelationship(string characterId, float change, string reason = "")
        {
            if (!_enableRelationshipProgression)
                return;
            
            if (_relationshipManager != null)
            {
                _relationshipManager.ModifyRelationship(characterId, change, reason);
                
                // Update character development
                if (_enableCharacterDevelopment && _developmentProfiles.TryGetValue(characterId, out var profile))
                {
                    profile.ExperiencePoints += Mathf.Abs(change) * 0.1f;
                }
                
                _metrics.RelationshipChanges++;
            }
        }
        
        public float GetRelationshipLevel(string characterId)
        {
            return _relationshipManager?.GetRelationshipLevel(characterId) ?? 0f;
        }
        
        private void UpdateAllRelationships()
        {
            if (_relationshipManager == null)
                return;
            
            foreach (var characterId in _activeCharacters.Keys)
            {
                _relationshipManager.UpdateRelationship(characterId);
            }
        }
        
        #endregion
        
        #region Public API
        
        public List<ActiveCharacterInstance> GetActiveCharacters()
        {
            return _activeCharacters.Values.ToList();
        }
        
        public List<ActiveConversation> GetActiveConversations()
        {
            return _activeConversations.Values.ToList();
        }
        
        public CharacterDevelopmentProfile GetCharacterDevelopment(string characterId)
        {
            return _developmentProfiles.GetValueOrDefault(characterId);
        }
        
        public CharacterDialogueMetrics GetMetrics()
        {
            return _metrics;
        }
        
        public bool IsCharacterActive(string characterId)
        {
            return _activeCharacters.ContainsKey(characterId);
        }
        
        public bool IsCharacterInConversation(string characterId)
        {
            return _activeConversations.Values.Any(c => c.CharacterId == characterId && c.IsActive);
        }
        
        #endregion
        
        #region Helper Methods
        
        // Placeholder methods for compilation
        private void LoadCharacterData() { }
        private void SaveCharacterData() { }
        private void DisposeCharacterResources() { }
        private void ProcessImmediateDialogueEvents() { }
        private void UpdateCharacterStates() { }
        private void UpdateActiveConversations() { }
        private void UpdateCharacterMetrics() { }
        private void UpdateCharacterAI() { }
        private void ProcessCharacterDevelopment() { }
        private void UpdatePersonalityEvolution() { }
        private void UpdateDialogueStates() { }
        private void CleanupCompletedDialogues() { }
        private void ProcessRelationshipEvents() { }
        
        // Event handlers
        private void HandleCharacterStateChanged(string characterId, CharacterState newState) { }
        private void HandlePersonalityChanged(string characterId, string newPersonalityId) { }
        private void HandleMemoryAdded(string characterId, MemoryItem memory) { }
        private void HandleDialogueCompleted(string conversationId) { }
        private void HandleDialogueChoiceSelected(string conversationId, int choiceIndex) { }
        private void HandleRelationshipChanged(string characterId, float newLevel, float change) { }
        private void HandleRelationshipMilestone(string characterId, RelationshipMilestone milestone) { }
        
        #endregion
    }
    
    // Supporting data structures and enums
    public enum CharacterDevelopmentStage
    {
        Initial,
        Developing,
        Established,
        Evolved,
        Mastered
    }
    
    public enum DialogueRequestType
    {
        StartConversation,
        ProcessChoice,
        EndConversation,
        UpdateContext
    }

    public enum DialogueStateType
    {
        Active,
        Paused,
        WaitingForChoice,
        Processing,
        Completed,
        Cancelled
    }
    
    // MemoryType enum is defined in NarrativeInterfaces.cs - using that definition
    
    public enum CharacterState
    {
        Idle,
        InConversation,
        Developing,
        Unavailable
    }
    
    public enum RelationshipMilestone
    {
        FirstMeeting,
        Acquaintance,
        Friend,
        CloseFriend,
        Trusted,
        Mentor,
        Rival,
        Enemy
    }
    
    [Serializable]
    public class ActiveCharacterInstance
    {
        public string CharacterId;
        public CharacterProfileSO Character;
        public DateTime IntroductionTime;
        public bool IsActive;
        public EmotionalState CurrentMood;
        public int InteractionCount;
        public DateTime LastInteraction;
        public List<string> DialogueHistory = new List<string>();
    }
    
    [Serializable]
    public class ActiveConversation
    {
        public string ConversationId;
        public string CharacterId;
        public string DialogueId;
        public DateTime StartTime;
        public DateTime EndTime;
        public string CurrentNodeId;
        public Dictionary<string, object> Context = new Dictionary<string, object>();
        public bool IsActive;
        public List<DialogueEntry> DialogueHistory = new List<DialogueEntry>();
    }
    
    [Serializable]
    public class DialogueEntry
    {
        public string NodeId;
        public string SpeakerId;
        public string Text;
        public EmotionalTone Tone;
        public DateTime Timestamp;
        public List<string> ChoiceIds = new List<string>();
        public bool AllowsSkipping = true;
        public float DisplayDuration = 0f;
        public string DialogueText;
        public List<string> Tags = new List<string>();
        
        public bool AllowSkipping() => AllowsSkipping;
    }
    
    [Serializable]
    public class CompletedConversation
    {
        public string ConversationId;
        public string CharacterId;
        public string DialogueId;
        public DateTime StartTime;
        public DateTime EndTime;
        public double TotalDuration;
        public List<DialogueEntry> DialogueEntries = new List<DialogueEntry>();
        public Dictionary<string, object> Context = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CharacterDevelopmentProfile
    {
        public string CharacterId;
        public string InitialPersonalityId;
        public CharacterDevelopmentStage DevelopmentStage;
        public float ExperiencePoints;
        public List<string> DevelopmentMilestones = new List<string>();
        public DateTime LastDevelopmentUpdate;
    }
    
    [Serializable]
    public class CharacterMemoryProfile
    {
        public string CharacterId;
        public List<MemoryItem> ShortTermMemory = new List<MemoryItem>();
        public List<MemoryItem> LongTermMemory = new List<MemoryItem>();
        public Dictionary<string, float> PersonalityMemory = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class MemoryItem
    {
        public MemoryType Type;
        public string Content;
        public DateTime Timestamp;
        public float EmotionalWeight;
        public Dictionary<string, object> AssociatedData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class PersonalityEvolutionData
    {
        public string CharacterId;
        public string OriginalPersonalityId;
        public string CurrentPersonalityId;
        public List<PersonalityShift> EvolutionHistory = new List<PersonalityShift>();
        public float EvolutionRate = 1.0f;
    }
    
    [Serializable]
    public class PersonalityShift
    {
        public string FromPersonalityId;
        public string ToPersonalityId;
        public DateTime ShiftTime;
        public string Reason;
        public float Magnitude;
    }
    
    [Serializable]
    public class DialogueState
    {
        public string StateId;
        public string ConversationId;
        public string CurrentNodeId;
        public DialogueStateType StateType = DialogueStateType.Active;
        public DateTime LastUpdate;
        public Dictionary<string, object> StateData = new Dictionary<string, object>();
        public float Progress;
        public bool IsWaitingForInput;
    }

    [Serializable]
    public class DialogueRequest
    {
        public DialogueRequestType RequestType;
        public string CharacterId;
        public string DialogueId;
        public string ConversationId;
        public int ChoiceIndex;
        public Dictionary<string, object> Context = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CharacterDialogueMetrics
    {
        public int CharactersIntroduced;
        public int ConversationsStarted;
        public int ConversationsCompleted;
        public int DialogueChoicesMade;
        public int RelationshipChanges;
        public int CharacterUpdateErrors;
        public int DialogueProcessingErrors;
        public int RelationshipUpdateErrors;
        public DateTime LastUpdate = DateTime.Now;
    }
    
    // Placeholder classes for compilation
    public class CharacterDialogueConfigSO : ChimeraConfigSO
    {
        public float DefaultTypingSpeed = 50f;
        public int MaxConversationHistory = 100;
        public bool EnableAdvancedFeatures = true;
    }
    
    public class CharacterInstanceManager
    {
        public event Action<string, CharacterState> OnCharacterStateChanged;
        public CharacterInstanceManager(CharacterDatabaseSO database, NarrativeConfigSO config) { }
    }
    
    public class DialogueTreeProcessor
    {
        public event Action<string> OnDialogueCompleted;
        public event Action<string, int> OnChoiceSelected;
        
        public DialogueTreeProcessor(DialogueLibrarySO library, CharacterDialogueConfigSO config) { }
        public void EnableBranchingDialogue(bool enabled) { }
        public void EnableEmotionalResponses(bool enabled) { }
        public void EnableContextualDialogue(bool enabled) { }
        public void SetDialogueSpeed(float speed) { }
        public void ProcessChoice(ActiveConversation conversation, int choiceIndex) { }
    }
    
    public class RelationshipManager
    {
        public event Action<string, float, float> OnRelationshipChanged;
        public event Action<string, RelationshipMilestone> OnRelationshipMilestone;
        
        public RelationshipManager(CharacterDatabaseSO database, NarrativeConfigSO config) { }
        public void SetChangeRate(float rate) { }
        public void ModifyRelationship(string characterId, float change, string reason) { }
        public float GetRelationshipLevel(string characterId) => 50f;
        public void UpdateRelationship(string characterId) { }
    }
    
    public class CharacterMemorySystem
    {
        public event Action<string, MemoryItem> OnMemoryAdded;
        public CharacterMemorySystem(NarrativeConfigSO config) { }
        public void AddMemory(string characterId, MemoryItem memory) { }
    }
    
    public class DialogueAnalyticsEngine
    {
        public DialogueAnalyticsEngine(NarrativeConfigSO config) { }
    }
    
    public class ConversationCache
    {
        public ConversationCache(CharacterDialogueConfigSO config) { }
    }
}