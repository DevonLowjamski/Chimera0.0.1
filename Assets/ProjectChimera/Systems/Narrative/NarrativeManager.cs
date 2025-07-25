using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Comprehensive narrative management system for Project Chimera's story building and progression.
    /// Handles dynamic storytelling, character development, branching narratives, player choices,
    /// and integration with cultivation activities for an immersive cannabis growing journey.
    /// </summary>
    public class NarrativeManager : ChimeraManager, IChimeraManager
    {
        [Header("Narrative Configuration")]
        [SerializeField] private NarrativeConfigSO _narrativeConfig;
        [SerializeField] private StoryDatabaseSO _storyDatabase;
        [SerializeField] private CharacterDatabaseSO _characterDatabase;
        [SerializeField] private DialogueLibrarySO _dialogueLibrary;
        [SerializeField] private QuestDatabaseSO _questDatabase;
        
        [Header("Story Progression")]
        [SerializeField] private bool _enableDynamicStorytelling = true;
        [SerializeField] private bool _enablePlayerChoices = true;
        [SerializeField] private bool _enableCharacterDevelopment = true;
        [SerializeField] private bool _enableBranchingNarratives = true;
        [SerializeField] private float _storyProgressionSpeed = 1.0f;
        
        [Header("Integration Settings")]
        [SerializeField] private bool _enableCultivationIntegration = true;
        [SerializeField] private bool _enableLiveEventIntegration = true;
        [SerializeField] private bool _enableEducationalIntegration = true;
        [SerializeField] private bool _enableCommunityStories = true;
        [SerializeField] private float _integrationUpdateInterval = 60f;
        
        [Header("Character Management")]
        [SerializeField] private bool _enableCompanionSystem = true;
        [SerializeField] private bool _enableMentorSystem = true;
        [SerializeField] private bool _enableRivalSystem = true;
        [SerializeField] private int _maxActiveCharacters = 10;
        [SerializeField] private bool _enableCharacterRelationships = true;
        
        [Header("Quest and Mission System")]
        [SerializeField] private bool _enableQuestSystem = true;
        [SerializeField] private bool _enableDynamicQuests = true;
        [SerializeField] private int _maxActiveQuests = 5;
        [SerializeField] private bool _enableQuestChaining = true;
        [SerializeField] private bool _enableCommunityQuests = true;
        
        [Header("Dialogue and Interaction")]
        [SerializeField] private bool _enableDialogueSystem = true;
        [SerializeField] private bool _enableVoiceActing = false;
        [SerializeField] private bool _enableEmotionalResponses = true;
        [SerializeField] private bool _enableContextualDialogue = true;
        [SerializeField] private float _dialogueSpeed = 1.0f;
        
        [Header("Event Channels - Narrative System")]
        [SerializeField] private NarrativeEventChannelSO _onStoryProgressed;
        [SerializeField] private NarrativeEventChannelSO _onChoiceMade;
        [SerializeField] private NarrativeEventChannelSO _onCharacterMet;
        [SerializeField] private NarrativeEventChannelSO _onQuestCompleted;
        [SerializeField] private NarrativeEventChannelSO _onDialogueStarted;
        [SerializeField] private NarrativeEventChannelSO _onStoryBranchUnlocked;
        
        // Core narrative systems
        private StoryProgressionManager _storyManager;
        private CharacterManager _characterManager;
        private DialogueManager _dialogueManager;
        private QuestManager _questManager;
        private ChoiceManager _choiceManager;
        
        // Story state management
        private NarrativeState _currentNarrativeState;
        private Dictionary<string, StoryInstance> _activeStories = new Dictionary<string, StoryInstance>();
        private Dictionary<string, CharacterInstance> _activeCharacters = new Dictionary<string, CharacterInstance>();
        private Dictionary<string, QuestInstance> _activeQuests = new Dictionary<string, QuestInstance>();
        
        // Player progression tracking
        private PlayerNarrativeProfile _playerProfile;
        private List<StoryChoice> _playerChoices = new List<StoryChoice>();
        private Dictionary<string, RelationshipState> _characterRelationships = new Dictionary<string, RelationshipState>();
        private List<CompletedStoryArc> _completedArcs = new List<CompletedStoryArc>();
        
        // Integration systems
        private CultivationNarrativeIntegrator _cultivationIntegrator;
        private LiveEventNarrativeIntegrator _liveEventIntegrator;
        private EducationalNarrativeIntegrator _educationalIntegrator;
        private CommunityStoryManager _communityStoryManager;
        
        // Dynamic content generation
        private DynamicStoryGenerator _storyGenerator;
        private PersonalityEngine _personalityEngine;
        private EmotionalResponseSystem _emotionalSystem;
        private ContextualDialogueGenerator _contextualDialogue;
        
        // Performance and analytics
        private NarrativeMetrics _metrics = new NarrativeMetrics();
        private StoryAnalyticsEngine _analyticsEngine;
        private NarrativeContentCache _contentCache;
        
        // System state
        private Coroutine _narrativeUpdateCoroutine;
        private Coroutine _integrationUpdateCoroutine;
        private Coroutine _characterUpdateCoroutine;
        private DateTime _lastStoryUpdate;
        private bool _isNarrativeActive = false;
        
        #region ChimeraManager Implementation
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        protected override void OnManagerInitialize()
        {
            LogInfo("Initializing Narrative Manager...");
            
            if (!ValidateConfiguration())
            {
                LogError("Narrative Manager configuration validation failed");
                return;
            }
            
            InitializeNarrativeSystems();
            InitializeIntegrationSystems();
            InitializePlayerProfile();
            LoadNarrativeState();
            
            StartNarrativeSystems();
            
            LogInfo("Narrative Manager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo("Shutting down Narrative Manager...");
            
            StopNarrativeSystems();
            SaveNarrativeState();
            DisposeNarrativeResources();
            
            LogInfo("Narrative Manager shutdown complete");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_isNarrativeActive)
                return;
            
            // Handle immediate narrative events
            ProcessImmediateNarrativeEvents();
            
            // Update character interactions
            UpdateCharacterInteractions();
            
            // Update quest progress
            UpdateQuestProgress();
            
            // Update metrics
            UpdateNarrativeMetrics();
        }
        
        private bool ValidateConfiguration()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            if (_narrativeConfig == null)
            {
                validationErrors.Add("Narrative Config SO is not assigned");
                isValid = false;
            }
            
            if (_storyDatabase == null)
            {
                validationErrors.Add("Story Database SO is not assigned");
                isValid = false;
            }
            
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
            
            // Validate event channels
            if (_onStoryProgressed == null)
            {
                validationErrors.Add("Story Progressed event channel is not assigned");
                isValid = false;
            }
            
            // Validate settings
            if (_storyProgressionSpeed <= 0f)
            {
                validationErrors.Add("Story progression speed must be greater than 0");
                isValid = false;
            }
            
            if (_maxActiveCharacters <= 0)
            {
                validationErrors.Add("Max active characters must be greater than 0");
                isValid = false;
            }
            
            if (!isValid)
            {
                LogError($"Narrative Manager validation failed: {string.Join(", ", validationErrors)}");
            }
            else
            {
                LogInfo("Narrative Manager validation passed");
            }
            
            return isValid;
        }
        
        #endregion
        
        #region Initialization Methods
        
        private void InitializeNarrativeSystems()
        {
            // Initialize story progression manager
            _storyManager = new StoryProgressionManager(_storyDatabase, _narrativeConfig);
            _storyManager.SetProgressionSpeed(_storyProgressionSpeed);
            
            // Initialize character manager
            _characterManager = new CharacterManager(_characterDatabase, _narrativeConfig);
            _characterManager.SetMaxActiveCharacters(_maxActiveCharacters);
            _characterManager.EnableRelationships(_enableCharacterRelationships);
            
            // Initialize dialogue manager
            if (_enableDialogueSystem)
            {
                _dialogueManager = new DialogueManager(_dialogueLibrary, _narrativeConfig);
                _dialogueManager.SetDialogueSpeed(_dialogueSpeed);
                _dialogueManager.EnableEmotionalResponses(_enableEmotionalResponses);
                _dialogueManager.EnableContextualDialogue(_enableContextualDialogue);
            }
            
            // Initialize quest manager
            if (_enableQuestSystem)
            {
                _questManager = new QuestManager(_questDatabase, _narrativeConfig);
                _questManager.SetMaxActiveQuests(_maxActiveQuests);
                _questManager.EnableDynamicQuests(_enableDynamicQuests);
                _questManager.EnableQuestChaining(_enableQuestChaining);
            }
            
            // Initialize choice manager
            if (_enablePlayerChoices)
            {
                _choiceManager = new ChoiceManager(null, _narrativeConfig);
                _choiceManager.EnableBranchingNarratives(_enableBranchingNarratives);
            }
            
            // Initialize content cache
            _contentCache = new NarrativeContentCache(_narrativeConfig);
            
            LogInfo("Core narrative systems initialized");
        }
        
        private void InitializeIntegrationSystems()
        {
            // Initialize cultivation integration
            if (_enableCultivationIntegration)
            {
                _cultivationIntegrator = new CultivationNarrativeIntegrator(_narrativeConfig);
                _cultivationIntegrator.OnCultivationStoryTrigger += HandleCultivationStoryTrigger;
            }
            
            // Initialize live event integration
            if (_enableLiveEventIntegration)
            {
                _liveEventIntegrator = new LiveEventNarrativeIntegrator(_narrativeConfig);
                _liveEventIntegrator.OnEventStoryTrigger += HandleLiveEventStoryTrigger;
            }
            
            // Initialize educational integration
            if (_enableEducationalIntegration)
            {
                _educationalIntegrator = new EducationalNarrativeIntegrator(_narrativeConfig);
                _educationalIntegrator.OnLearningStoryTrigger += HandleEducationalStoryTrigger;
            }
            
            // Initialize community stories
            if (_enableCommunityStories)
            {
                _communityStoryManager = new CommunityStoryManager(_narrativeConfig);
                _communityStoryManager.OnCommunityStoryEvent += HandleCommunityStoryEvent;
            }
            
            LogInfo("Integration systems initialized");
        }
        
        private void InitializePlayerProfile()
        {
            _playerProfile = new PlayerNarrativeProfile
            {
                PlayerId = SystemInfo.deviceUniqueIdentifier, // Placeholder
                NarrativePreferences = new PlayerPreferences(),
                StoryProgression = new Dictionary<string, float>(),
                UnlockedContent = "",
                CharacterMeetings = new List<string>(),
                CompletedChoices = new List<string>()
            };
            
            // Initialize current narrative state
            _currentNarrativeState = new NarrativeState
            {
                CurrentMainStory = null,
                ActiveStoryArcs = new List<string>(),
                AvailableChoices = new List<string>(),
                RecentEvents = new List<string>(),
                StateTimestamp = DateTime.Now
            };
        }
        
        private void LoadNarrativeState()
        {
            // Load saved narrative state from persistent storage
            // This would integrate with Project Chimera's save system
            
            // For now, initialize with beginning state
            InitializeBeginningState();
        }
        
        private void InitializeBeginningState()
        {
            // Start with the beginning story arc
            var beginningStory = _storyDatabase.GetBeginningStory();
            if (beginningStory != null)
            {
                StartStoryArc(beginningStory);
            }
            
            // Introduce initial characters
            IntroduceInitialCharacters();
            
            // Create initial quests
            CreateInitialQuests();
        }
        
        #endregion
        
        #region Narrative System Management
        
        private void StartNarrativeSystems()
        {
            // Start main narrative update loop
            if (_narrativeUpdateCoroutine == null)
            {
                _narrativeUpdateCoroutine = StartCoroutine(NarrativeUpdateLoop());
            }
            
            // Start integration update loop
            if (_integrationUpdateCoroutine == null)
            {
                _integrationUpdateCoroutine = StartCoroutine(IntegrationUpdateLoop());
            }
            
            // Start character update loop
            if (_characterUpdateCoroutine == null)
            {
                _characterUpdateCoroutine = StartCoroutine(CharacterUpdateLoop());
            }
            
            _isNarrativeActive = true;
            LogInfo("Narrative systems started");
        }
        
        private void StopNarrativeSystems()
        {
            _isNarrativeActive = false;
            
            if (_narrativeUpdateCoroutine != null)
            {
                StopCoroutine(_narrativeUpdateCoroutine);
                _narrativeUpdateCoroutine = null;
            }
            
            if (_integrationUpdateCoroutine != null)
            {
                StopCoroutine(_integrationUpdateCoroutine);
                _integrationUpdateCoroutine = null;
            }
            
            if (_characterUpdateCoroutine != null)
            {
                StopCoroutine(_characterUpdateCoroutine);
                _characterUpdateCoroutine = null;
            }
            
            LogInfo("Narrative systems stopped");
        }
        
        private IEnumerator NarrativeUpdateLoop()
        {
            while (_isNarrativeActive)
            {
                yield return new WaitForSeconds(10f); // Update every 10 seconds
                
                try
                {
                    UpdateActiveStories();
                    CheckStoryTriggers();
                    ProcessPendingChoices();
                    UpdateStoryProgression();
                    
                    _lastStoryUpdate = DateTime.Now;
                }
                catch (Exception ex)
                {
                    LogError($"Error in narrative update loop: {ex.Message}");
                    _metrics.UpdateErrors++;
                }
            }
        }
        
        private IEnumerator IntegrationUpdateLoop()
        {
            while (_isNarrativeActive)
            {
                yield return new WaitForSeconds(_integrationUpdateInterval);
                
                try
                {
                    UpdateCultivationIntegration();
                    UpdateLiveEventIntegration();
                    UpdateEducationalIntegration();
                    UpdateCommunityStories();
                }
                catch (Exception ex)
                {
                    LogError($"Error in integration update loop: {ex.Message}");
                    _metrics.IntegrationErrors++;
                }
            }
        }
        
        private IEnumerator CharacterUpdateLoop()
        {
            while (_isNarrativeActive)
            {
                yield return new WaitForSeconds(30f); // Update every 30 seconds
                
                try
                {
                    UpdateCharacterStates();
                    ProcessCharacterInteractions();
                    UpdateRelationships();
                    CheckCharacterEvents();
                }
                catch (Exception ex)
                {
                    LogError($"Error in character update loop: {ex.Message}");
                    _metrics.CharacterUpdateErrors++;
                }
            }
        }
        
        #endregion
        
        #region Story Management
        
        public void StartStoryArc(StoryArcSO storyArc)
        {
            if (storyArc == null) return;
            
            var storyInstance = new StoryInstance
            {
                StoryId = storyArc.StoryId,
                StoryArc = storyArc,
                StartTime = DateTime.Now,
                CurrentChapter = 0,
                IsActive = true,
                Progress = 0f
            };
            
            _activeStories[storyArc.StoryId] = storyInstance;
            _currentNarrativeState.ActiveStoryArcs.Add(storyArc.StoryId);
            
            // Set as main story if it's a main arc
            if (storyArc.IsMainStory() && _currentNarrativeState.CurrentMainStory == null)
            {
                _currentNarrativeState.CurrentMainStory = storyArc.StoryId;
            }
            
            // Trigger story started event
            _onStoryProgressed?.RaiseEvent(new NarrativeEventMessage
            {
                EventType = NarrativeEventType.StoryStarted.ToString(),
                StoryId = storyArc.StoryId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "storyArc", storyArc },
                    { "isMainStory", storyArc.IsMainStory() }
                }
            });
            
            LogInfo($"Started story arc: {storyArc.StoryName}");
            _metrics.StoriesStarted++;
        }
        
        public void ProgressStory(string storyId, float progressAmount)
        {
            if (!_activeStories.TryGetValue(storyId, out var storyInstance))
                return;
            
            storyInstance.Progress += progressAmount * _storyProgressionSpeed;
            storyInstance.Progress = Mathf.Clamp01(storyInstance.Progress);
            
            // Check for chapter progression
            var totalChapters = storyInstance.StoryArc.Chapters.Count;
            var newChapter = Mathf.FloorToInt(storyInstance.Progress * totalChapters);
            
            if (newChapter > storyInstance.CurrentChapter && newChapter < totalChapters)
            {
                storyInstance.CurrentChapter = newChapter;
                TriggerChapterTransition(storyInstance, newChapter);
            }
            
            // Check for story completion
            if (storyInstance.Progress >= 1f && storyInstance.IsActive)
            {
                CompleteStoryArc(storyInstance);
            }
            
            // Update player profile
            _playerProfile.StoryProgression[storyId] = storyInstance.Progress;
            
            _metrics.StoryProgressUpdates++;
        }
        
        private void CompleteStoryArc(StoryInstance storyInstance)
        {
            storyInstance.IsActive = false;
            storyInstance.CompletionTime = DateTime.Now;
            
            // Add to completed arcs
            _completedArcs.Add(new CompletedStoryArc
            {
                StoryId = storyInstance.StoryId,
                CompletionTime = DateTime.Now,
                ChoicesMade = _playerChoices.Where(c => c.StoryId == storyInstance.StoryId)
                    .Select(sc => new PlayerChoiceData(sc)).ToList(),
                FinalProgress = storyInstance.Progress
            });
            
            // Remove from active stories
            _activeStories.Remove(storyInstance.StoryId);
            _currentNarrativeState.ActiveStoryArcs.Remove(storyInstance.StoryId);
            
            // Check for follow-up stories
            CheckFollowUpStories(storyInstance.StoryArc);
            
            // Trigger story completed event
            _onStoryProgressed?.RaiseEvent(new NarrativeEventMessage
            {
                EventType = NarrativeEventType.StoryCompleted.ToString(),
                StoryId = storyInstance.StoryId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "storyInstance", storyInstance },
                    { "completionTime", storyInstance.CompletionTime }
                }
            });
            
            LogInfo($"Completed story arc: {storyInstance.StoryArc.StoryName}");
            _metrics.StoriesCompleted++;
        }
        
        private void TriggerChapterTransition(StoryInstance storyInstance, int newChapter)
        {
            var chapter = storyInstance.StoryArc.Chapters[newChapter];
            
            // Trigger chapter events
            foreach (var storyEvent in chapter.ChapterEvents)
            {
                TriggerStoryEvent(storyEvent, storyInstance);
            }
            
            // Introduce new characters if any
            foreach (var characterId in chapter.IntroducedCharacters)
            {
                IntroduceCharacter(characterId);
            }
            
            // Create chapter quests
            foreach (var questId in chapter.AvailableQuests)
            {
                CreateQuest(questId);
            }
            
            LogInfo($"Progressed to chapter {newChapter + 1} in story: {storyInstance.StoryArc.StoryName}");
        }
        
        #endregion
        
        #region Choice Management
        
        public void PresentChoice(StoryChoiceSO choice)
        {
            if (choice == null || !_enablePlayerChoices) return;
            
            // Add to available choices
            _currentNarrativeState.AvailableChoices.Add(choice.ChoiceId);
            
            // Trigger choice presentation event
            _onChoiceMade?.RaiseEvent(new NarrativeEventMessage
            {
                EventType = NarrativeEventType.ChoicePresented.ToString(),
                ChoiceId = choice.ChoiceId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "choice", choice },
                    { "options", choice.Options }
                }
            });
            
            LogInfo($"Presented choice: {choice.ChoiceText}");
        }
        
        public void MakeChoice(string choiceId, int optionIndex)
        {
            var choice = _storyDatabase.GetChoice(choiceId);
            if (choice == null || optionIndex < 0 || optionIndex >= choice.Options.Count)
                return;
            
            var selectedOption = choice.Options[optionIndex];
            
            // Record choice
            var storyChoice = new StoryChoice
            {
                ChoiceId = choiceId,
                StoryId = GetCurrentStoryId(),
                SelectedOption = selectedOption,
                Timestamp = DateTime.Now,
                Consequences = new List<Consequence>(), // Initialize as empty list, will be populated from consequence IDs
                ConsequencesByIndex = choice.ConsequencesByOption.ContainsKey(optionIndex) ? 
                    new Dictionary<int, List<string>> {{ optionIndex, choice.ConsequencesByOption[optionIndex] }} : 
                    new Dictionary<int, List<string>>()
            };
            
            _playerChoices.Add(storyChoice);
            _playerProfile.CompletedChoices.Add(choiceId);
            
            // Remove from available choices
            _currentNarrativeState.AvailableChoices.Remove(choiceId);
            
            // Apply choice consequences
            ApplyChoiceConsequences(storyChoice);
            
            // Trigger choice made event
            _onChoiceMade?.RaiseEvent(new NarrativeEventMessage
            {
                EventType = NarrativeEventType.ChoiceMade.ToString(),
                ChoiceId = choiceId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "choice", storyChoice },
                    { "selectedOption", selectedOption }
                }
            });
            
            LogInfo($"Made choice: {choice.ChoiceText} -> {selectedOption.OptionText}");
            _metrics.ChoicesMade++;
        }
        
        private void ApplyChoiceConsequences(StoryChoice storyChoice)
        {
            // Apply consequences from the selected option
            foreach (var consequenceDict in storyChoice.ConsequencesByIndex)
            {
                foreach (var consequenceId in consequenceDict.Value)
                {
                    ApplyConsequence(consequenceId, storyChoice);
                }
            }
        }
        
        private void ApplyConsequence(string consequence, StoryChoice choice)
        {
            // Parse and apply consequence
            // Format: "type:target:value" (e.g., "relationship:mentor:10", "story:unlock_branch", "item:seeds:5")
            var parts = consequence.Split(':');
            if (parts.Length < 2) return;
            
            var type = parts[0];
            var target = parts[1];
            
            switch (type)
            {
                case "relationship":
                    if (parts.Length >= 3 && float.TryParse(parts[2], out var relationshipChange))
                    {
                        ModifyRelationship(target, relationshipChange);
                    }
                    break;
                case "story":
                    if (target == "unlock_branch" && parts.Length >= 3)
                    {
                        UnlockStoryBranch(parts[2]);
                    }
                    break;
                case "character":
                    if (target == "introduce" && parts.Length >= 3)
                    {
                        IntroduceCharacter(parts[2]);
                    }
                    break;
                case "quest":
                    if (target == "unlock" && parts.Length >= 3)
                    {
                        CreateQuest(parts[2]);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Character Management
        
        public void IntroduceCharacter(string characterId)
        {
            var characterData = _characterDatabase.GetCharacter(characterId);
            if (characterData == null) return;
            
            // Check if already introduced
            if (_activeCharacters.ContainsKey(characterId))
                return;
            
            var characterInstance = new CharacterInstance
            {
                CharacterId = characterId,
                CharacterData2 = characterData,
                IntroductionTime = DateTime.Now,
                IsActive = true,
                CurrentMood = EmotionalState.Neutral,
                DialogueHistory = new List<string>()
            };
            
            _activeCharacters[characterId] = characterInstance;
            _playerProfile.CharacterMeetings.Add(characterId);
            
            // Initialize relationship
            _characterRelationships[characterId] = new RelationshipState
            {
                CharacterId = characterId,
                RelationshipLevel = 0f,
                RelationshipType = RelationshipType.Neutral,
                FirstMeeting = DateTime.Now,
                LastInteraction = DateTime.Now
            };
            
            // Trigger character introduction event
            _onCharacterMet?.RaiseEvent(new NarrativeEventMessage
            {
                EventType = NarrativeEventType.CharacterIntroduced.ToString(),
                CharacterId = characterId,
                Timestamp = DateTime.Now,
                Data = new Dictionary<string, object>
                {
                    { "character", characterData },
                    { "isCompanion", characterData.IsCompanion }
                }
            });
            
            LogInfo($"Introduced character: {characterData.CharacterName}");
            _metrics.CharactersIntroduced++;
        }
        
        private void ModifyRelationship(string characterId, float change)
        {
            if (!_characterRelationships.TryGetValue(characterId, out var relationship))
                return;
            
            relationship.RelationshipLevel += change;
            relationship.RelationshipLevel = Mathf.Clamp(relationship.RelationshipLevel, -100f, 100f);
            relationship.LastInteraction = DateTime.Now;
            
            // Update relationship type based on level
            relationship.RelationshipType = GetRelationshipType(relationship.RelationshipLevel);
            
            LogInfo($"Modified relationship with {characterId}: {change:+0;-0} (Total: {relationship.RelationshipLevel})");
        }
        
        private RelationshipType GetRelationshipType(float level)
        {
            return level switch
            {
                >= 75f => RelationshipType.BestFriend,
                >= 50f => RelationshipType.Friend,
                >= 25f => RelationshipType.Friendly,
                >= -25f => RelationshipType.Neutral,
                >= -50f => RelationshipType.Unfriendly,
                >= -75f => RelationshipType.Enemy,
                _ => RelationshipType.Hostile
            };
        }
        
        #endregion
        
        #region Public API
        
        public NarrativeState GetCurrentNarrativeState()
        {
            return _currentNarrativeState;
        }
        
        public List<StoryInstance> GetActiveStories()
        {
            return _activeStories.Values.ToList();
        }
        
        public List<CharacterInstance> GetActiveCharacters()
        {
            return _activeCharacters.Values.ToList();
        }
        
        public PlayerNarrativeProfile GetPlayerProfile()
        {
            return _playerProfile;
        }
        
        public void TriggerStoryEvent(string eventId)
        {
            var storyEvent = _storyDatabase.GetStoryEvent(eventId);
            if (storyEvent != null)
            {
                TriggerStoryEvent(storyEvent, null);
            }
        }
        
        public NarrativeMetrics GetMetrics()
        {
            return _metrics;
        }
        
        #endregion
        
        #region Helper Methods
        
        // Placeholder methods for compilation
        private void UpdateActiveStories() { }
        private void CheckStoryTriggers() { }
        private void ProcessPendingChoices() { }
        private void UpdateStoryProgression() { }
        private void UpdateCultivationIntegration() { }
        private void UpdateLiveEventIntegration() { }
        private void UpdateEducationalIntegration() { }
        private void UpdateCommunityStories() { }
        private void UpdateCharacterStates() { }
        private void ProcessCharacterInteractions() { }
        private void UpdateRelationships() { }
        private void CheckCharacterEvents() { }
        private void ProcessImmediateNarrativeEvents() { }
        private void UpdateCharacterInteractions() { }
        private void UpdateQuestProgress() { }
        private void UpdateNarrativeMetrics() { }
        private void IntroduceInitialCharacters() { }
        private void CreateInitialQuests() { }
        private void CheckFollowUpStories(StoryArcSO storyArc) { }
        private void TriggerStoryEvent(StoryEventSO storyEvent, StoryInstance storyInstance) { }
        private void CreateQuest(string questId) { }
        private void UnlockStoryBranch(string branchId) { }
        private string GetCurrentStoryId() => _currentNarrativeState.CurrentMainStory ?? "";
        private void SaveNarrativeState() { }
        private void DisposeNarrativeResources() { }
        
        // Event handlers
        private void HandleCultivationStoryTrigger(CultivationStoryTrigger eventData) { }
        private void HandleLiveEventStoryTrigger(LiveEventStoryTrigger eventData) { }
        private void HandleEducationalStoryTrigger(EducationalStoryEvent eventData) { }
        private void HandleCommunityStoryEvent(CommunityStoryEvent eventData) { }
        
        #endregion
    }
    
    // Supporting systems and helper classes for NarrativeManager
    public class StoryProgressionManager
    {
        public StoryProgressionManager(StoryDatabaseSO database, NarrativeConfigSO config) { }
        public void SetProgressionSpeed(float speed) { }
    }
    
    public class CharacterManager
    {
        public CharacterManager(CharacterDatabaseSO database, NarrativeConfigSO config) { }
        public void SetMaxActiveCharacters(int maxCharacters) { }
        public void EnableRelationships(bool enabled) { }
    }
    
    public class DialogueManager
    {
        public DialogueManager(DialogueLibrarySO library, NarrativeConfigSO config) { }
        public void SetDialogueSpeed(float speed) { }
        public void EnableEmotionalResponses(bool enabled) { }
        public void EnableContextualDialogue(bool enabled) { }
    }
    
    public class QuestManager
    {
        public QuestManager(QuestDatabaseSO database, NarrativeConfigSO config) { }
        public void SetMaxActiveQuests(int maxQuests) { }
        public void EnableDynamicQuests(bool enabled) { }
        public void EnableQuestChaining(bool enabled) { }
    }
    
    // Note: ChoiceManager is defined in StoryEventSystem.cs to avoid namespace conflicts
    
    public class LiveEventNarrativeIntegrator
    {
        public event Action<LiveEventStoryTrigger> OnEventStoryTrigger;
        public LiveEventNarrativeIntegrator(NarrativeConfigSO config) { }
    }
    
    public class EducationalNarrativeIntegrator
    {
        public event Action<EducationalStoryEvent> OnLearningStoryTrigger;
        public EducationalNarrativeIntegrator(NarrativeConfigSO config) { }
    }
    
    public class CommunityStoryManager
    {
        public event Action<CommunityStoryEvent> OnCommunityStoryEvent;
        public CommunityStoryManager(NarrativeConfigSO config) { }
    }
    
    public class DynamicStoryGenerator
    {
        public DynamicStoryGenerator(NarrativeConfigSO config) { }
    }
    
    public class PersonalityEngine
    {
        public event System.Action<string, string> OnPersonalityChanged;
        
        public PersonalityEngine(NarrativeConfigSO config) { }
        
        public void UpdatePersonality(string characterId, string newPersonalityId)
        {
            OnPersonalityChanged?.Invoke(characterId, newPersonalityId);
        }
    }
    
    public class EmotionalResponseSystem
    {
        public EmotionalResponseSystem(NarrativeConfigSO config) { }
    }
    
    public class ContextualDialogueGenerator
    {
        public ContextualDialogueGenerator(NarrativeConfigSO config) { }
    }
    
    public class StoryAnalyticsEngine
    {
        public StoryAnalyticsEngine(NarrativeConfigSO config) { }
    }
    
    public class NarrativeContentCache
    {
        public NarrativeContentCache(NarrativeConfigSO config) { }
    }
}