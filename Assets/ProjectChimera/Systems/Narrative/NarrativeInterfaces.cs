using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Narrative;
using ProjectChimera.Core;
// Namespace aliases to resolve ambiguous references
using CoreGameState = ProjectChimera.Core.GameState;
using NarrativeGameState = ProjectChimera.Data.Narrative.GameState;
using DataEducationalProgressTracker = ProjectChimera.Data.Narrative.EducationalProgressTracker;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Core interface for story arc implementations in Project Chimera's narrative system
    /// </summary>
    public interface IStoryArc
    {
        string ArcId { get; }
        string ArcName { get; }
        StoryArcCategory Category { get; }
        bool IsActive { get; }
        bool IsCompleted { get; }
        float Progress { get; }
        
        // Lifecycle management
        void Initialize(NarrativeContext context);
        void StartArc();
        void UpdateArc(float deltaTime);
        void PauseArc();
        void ResumeArc();
        void CompleteArc();
        void Cleanup();
        
        // Story beat management
        StoryBeatSO GetCurrentBeat();
        StoryBeatSO GetNextBeat();
        bool CanProgressToNextBeat();
        void ProgressToNextBeat();
        void JumpToBeat(string beatId);
        
        // Educational integration
        List<LearningObjective> GetLearningObjectives();
        float GetEducationalProgress();
        bool ValidateEducationalContent();
        
        // Events
        event Action<string> OnBeatChanged;
        event Action<float> OnProgressChanged;
        event Action OnArcCompleted;
        event Action<string> OnEducationalMilestone;
    }
    
    /// <summary>
    /// Interface for character relationship management
    /// </summary>
    public interface ICharacterRelationship
    {
        string CharacterId { get; }
        RelationshipType Type { get; }
        float TrustLevel { get; }
        float RespectLevel { get; }
        float InfluenceLevel { get; }
        
        // Relationship dynamics
        void UpdateRelationship(PlayerAction action, ActionContext context);
        void ModifyTrust(float change);
        void ModifyRespect(float change);
        void ModifyInfluence(float change);
        
        // Memory and history
        void AddSharedMemory(SharedMemory memory);
        List<SharedMemory> GetSharedMemories();
        List<RelationshipEvent> GetRelationshipHistory();
        
        // Relationship queries
        bool CanInfluencePlayer();
        float GetOverallRelationshipLevel();
        RelationshipStatus GetRelationshipStatus();
        
        // Events
        event Action<float> OnTrustChanged;
        event Action<float> OnRespectChanged;
        event Action<RelationshipStatus> OnStatusChanged;
    }
    
    /// <summary>
    /// Interface for dialogue processing and management
    /// </summary>
    public interface IDialogueProcessor
    {
        bool IsProcessing { get; }
        DialogueEntry CurrentDialogue { get; }
        
        // Dialogue lifecycle
        void StartDialogue(DialogueSequence sequence);
        void ProcessDialogueEntry(DialogueEntry entry);
        void DisplayChoices(List<PlayerChoice> choices);
        void ProcessPlayerChoice(PlayerChoice choice);
        void EndDialogue();
        
        // Context and state
        void SetContext(DialogueContext context);
        void UpdateSpeakerMood(string speakerId, EmotionalState mood);
        void ApplyRelationshipInfluence(ICharacterRelationship relationship);
        
        // Validation and filtering
        bool ValidateDialogueContent(DialogueEntry entry);
        List<PlayerChoice> FilterAvailableChoices(List<PlayerChoice> choices, NarrativeGameState gameState);
        
        // Events
        event Action<DialogueEntry> OnDialogueStarted;
        event Action<PlayerChoice> OnChoiceSelected;
        event Action<DialogueResult> OnDialogueCompleted;
    }
    
    /// <summary>
    /// Interface for branching narrative engine
    /// </summary>
    public interface IBranchingNarrativeEngine
    {
        NarrativeGraph CurrentGraph { get; }
        List<NarrativePath> ActivePaths { get; }
        
        // Path calculation and management
        NarrativePath CalculateNextPath(PlayerDecision decision);
        List<NarrativePath> GetAvailablePaths(NarrativeState currentState);
        void UpdateNarrativeState(NarrativeState newState);
        
        // Decision processing
        void ProcessPlayerDecision(PlayerDecision decision);
        List<Consequence> CalculateConsequences(PlayerDecision decision);
        void ApplyConsequences(List<Consequence> consequences);
        
        // Dynamic generation
        NarrativeEvent GenerateContextualEvent(NarrativeContext context);
        bool ValidateNarrativeCoherence(NarrativeEvent narrativeEvent);
        
        // Events
        event Action<NarrativePath> OnPathChanged;
        event Action<PlayerDecision> OnDecisionProcessed;
        event Action<List<Consequence>> OnConsequencesApplied;
    }
    
    /// <summary>
    /// Interface for consequence tracking and management
    /// </summary>
    public interface IConsequenceTracker
    {
        Dictionary<string, List<Consequence>> TrackedConsequences { get; }
        
        // Consequence processing
        void ProcessChoice(PlayerChoice choice);
        void ApplyConsequence(Consequence consequence);
        void ScheduleDelayedConsequence(DelayedConsequence consequence);
        
        // Consequence queries
        List<Consequence> GetConsequencesForChoice(string choiceId);
        List<Consequence> GetActiveConsequences();
        bool HasConsequence(string consequenceId);
        
        // Timeline management
        void UpdateDelayedConsequences(float deltaTime);
        void CancelDelayedConsequence(string consequenceId);
        
        // History and tracking
        void RecordChoiceInHistory(PlayerChoice choice, List<Consequence> consequences);
        ConsequenceHistory GetConsequenceHistory();
        
        // Events
        event Action<Consequence> OnConsequenceApplied;
        event Action<DelayedConsequence> OnDelayedConsequenceTriggered;
        event Action<PlayerChoice> OnChoiceRecorded;
    }
    
    /// <summary>
    /// Interface for emotional engagement analysis
    /// </summary>
    public interface IEmotionalEngagementAnalyzer
    {
        EmotionalProfile CurrentProfile { get; }
        float OverallEngagement { get; }
        
        // Emotion tracking
        void TrackEmotionalResponse(NarrativeEvent narrativeEvent, PlayerResponse response);
        void UpdateCharacterAttachment(string characterId, float attachmentChange);
        void UpdateStoryInvestment(string storyId, float investmentChange);
        
        // Analysis and prediction
        EmotionalData AnalyzeResponse(PlayerResponse response);
        float PredictEngagementLevel(NarrativeContext context);
        void AdaptNarrativeStyle(EmotionalData emotionalData);
        
        // Reporting
        EmotionalReport GenerateEmotionalReport();
        Dictionary<string, float> GetCharacterAttachmentLevels();
        Dictionary<string, float> GetStoryInvestmentLevels();
        
        // Events
        event Action<EmotionalData> OnEmotionalResponseTracked;
        event Action<float> OnEngagementChanged;
        event Action<string> OnNarrativeStyleAdapted;
    }
    
    /// <summary>
    /// Interface for narrative analytics and tracking
    /// </summary>
    public interface INarrativeAnalytics
    {
        bool IsTracking { get; }
        AnalyticsProfile CurrentProfile { get; }
        
        // Data collection
        void TrackPlayerAction(PlayerAction action);
        void TrackDialogueInteraction(DialogueInteraction interaction);
        void TrackStoryProgression(StoryProgressionEvent progressionEvent);
        void TrackEducationalOutcome(EducationalOutcome outcome);
        
        // Performance monitoring
        void TrackPerformanceMetrics(PerformanceMetrics metrics);
        void TrackLoadTime(float loadTime);
        void TrackMemoryUsage(float memoryUsage);
        
        // Reporting and analysis
        AnalyticsReport GenerateReport(ReportType reportType);
        void ExportData(string format, string destination);
        void ProcessAnalyticsData();
        
        // Configuration
        void UpdateAnalyticsConfiguration(NarrativeAnalyticsConfigSO config);
        void EnableTracking(bool enable);
        
        // Events
        event Action<PlayerAction> OnPlayerActionTracked;
        event Action<AnalyticsReport> OnReportGenerated;
        event Action<string> OnDataExported;
    }
    
    /// <summary>
    /// Interface for narrative event processing
    /// </summary>
    public interface INarrativeEventProcessor
    {
        Queue<NarrativeEvent> PendingEvents { get; }
        
        // Event processing
        void ProcessEvent(NarrativeEvent narrativeEvent);
        void QueueEvent(NarrativeEvent narrativeEvent);
        void ProcessEventQueue();
        
        // Event creation
        NarrativeEvent CreateStoryEvent(string eventType, object eventData);
        NarrativeEvent CreateCharacterEvent(string characterId, string eventType, object eventData);
        NarrativeEvent CreateChoiceEvent(PlayerChoice choice, object context);
        
        // Event filtering and validation
        bool ValidateEvent(NarrativeEvent narrativeEvent);
        List<NarrativeEvent> FilterEventsByType(string eventType);
        
        // Events
        event Action<NarrativeEvent> OnEventProcessed;
        event Action<NarrativeEvent> OnEventQueued;
        event Action OnEventQueueEmpty;
    }
    
    /// <summary>
    /// Interface for educational content validation
    /// </summary>
    public interface IEducationalContentValidator
    {
        ValidationProfile CurrentProfile { get; }
        
        // Content validation
        ValidationResult ValidateContent(EducationalContent content);
        ValidationResult ValidateCultivationFact(CultivationFact fact);
        ValidationResult ValidateLearningObjective(LearningObjective objective);
        
        // Scientific accuracy
        bool VerifyScientificAccuracy(string content, CultivationTopic topic);
        ScientificValidation GetValidationData(string contentId);
        void UpdateValidationDatabase(ValidationDatabase database);
        
        // Compliance checking
        bool CheckEducationalCompliance(EducationalContent content);
        ComplianceReport GenerateComplianceReport();
        
        // Events
        event Action<ValidationResult> OnContentValidated;
        event Action<ScientificValidation> OnScientificValidationUpdated;
        event Action<ComplianceReport> OnComplianceReportGenerated;
    }
    
    // Supporting data structures and enums
    
    /// <summary>
    /// Context for narrative operations
    /// </summary>
    [Serializable]
    public class NarrativeContext
    {
        public GameObject PlayerReference;
        public Transform UIParent;
        public Dictionary<string, object> GlobalContext = new Dictionary<string, object>();
        public NarrativeGameState CurrentGameState;
        public List<ICharacterRelationship> ActiveRelationships = new List<ICharacterRelationship>();
        public bool EnableTutorials = true;
        public float UIScale = 1.0f;
        
        // Methods for accessing context data
        public Dictionary<string, object> AddionalData() => GlobalContext;
        public Dictionary<string, object> Data() => GlobalContext;
    }
    
    /// <summary>
    /// Dialogue context and state
    /// </summary>
    [Serializable]
    public class DialogueContext
    {
        public string CurrentSpeakerId;
        public Dictionary<string, EmotionalState> CharacterMoods = new Dictionary<string, EmotionalState>();
        public List<ICharacterRelationship> RelevantRelationships = new List<ICharacterRelationship>();
        public NarrativeGameState GameState;
        public float TimeSinceLastDialogue;
        public DialogueLocation Location;
    }
    
    /// <summary>
    /// Player action data
    /// </summary>
    [Serializable]
    public class PlayerAction
    {
        public string ActionId;
        public ActionType Type;
        public string TargetId;
        public float Value;
        public DateTime Timestamp;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Action context for relationship updates
    /// </summary>
    [Serializable]
    public class ActionContext
    {
        public string ContextType;
        public string Location;
        public List<string> Witnesses = new List<string>();
        public float Severity = 1.0f;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Shared memory between player and character
    /// </summary>
    [Serializable]
    public class SharedMemory
    {
        public string MemoryId;
        public string Description;
        public DateTime Timestamp;
        public float EmotionalImpact = 0.5f;
        public MemoryType Type;
        public bool IsPositive = true;
    }
    
    /// <summary>
    /// Relationship event record
    /// </summary>
    [Serializable]
    public class RelationshipEvent
    {
        public PlayerAction Action;
        public ActionContext Context;
        public float TrustChange;
        public float RespectChange;
        public float InfluenceChange;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Dialogue sequence container
    /// </summary>
    [Serializable]
    public class DialogueSequence
    {
        public string SequenceId;
        public List<DialogueEntry> Entries = new List<DialogueEntry>();
        public List<PlayerChoice> Choices = new List<PlayerChoice>();
        public DialogueContext Context;
    }
    
    /// <summary>
    /// Dialogue result data
    /// </summary>
    [Serializable]
    public class DialogueResult
    {
        public string DialogueId;
        public PlayerChoice SelectedChoice;
        public float TimeSpent;
        public List<Consequence> Consequences = new List<Consequence>();
        public EmotionalResponse PlayerResponse;
    }
    
    /// <summary>
    /// Player decision data
    /// </summary>
    [Serializable]
    public class PlayerDecision
    {
        public string DecisionId;
        public PlayerChoice Choice;
        public NarrativeContext Context;
        public float DecisionTime;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Narrative path information
    /// </summary>
    [Serializable]
    public class NarrativePath
    {
        public string PathId;
        public List<string> BeatIds = new List<string>();
        public float PathWeight = 1.0f;
        public Dictionary<string, object> PathData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Narrative state tracking
    /// </summary>
    [Serializable]
    public class NarrativeState
    {
        public string CurrentArcId;
        public string CurrentBeatId;
        public Dictionary<string, float> StateValues = new Dictionary<string, float>();
        public List<string> CompletedBeats = new List<string>();
        public List<string> AvailableChoices = new List<string>();
        
        // Additional properties for compatibility with Data namespace version
        public string CurrentMainStory;
        public List<string> ActiveStoryArcs = new List<string>();
        public List<string> RecentEvents = new List<string>();
        public DateTime StateTimestamp;
        public HashSet<string> NarrativeFlagsSet = new HashSet<string>();
        public DataEducationalProgressTracker EducationalProgress;
        
        // Property accessor for NarrativeFlags compatibility
        public HashSet<string> NarrativeFlags 
        { 
            get => NarrativeFlagsSet ?? (NarrativeFlagsSet = new HashSet<string>());
            set => NarrativeFlagsSet = value ?? new HashSet<string>();
        }
    }
    
    /// <summary>
    /// Extension methods for cross-namespace conversions
    /// </summary>
    public static class NarrativeStateExtensions
    {
        /// <summary>
        /// Convert Data namespace NarrativeState to Systems namespace NarrativeState
        /// </summary>
        public static ProjectChimera.Systems.Narrative.NarrativeState ToSystemsNarrativeState(this ProjectChimera.Data.Narrative.NarrativeState dataState)
        {
            if (dataState == null) return null;
            
            return new ProjectChimera.Systems.Narrative.NarrativeState
            {
                CurrentMainStory = dataState.CurrentMainStory,
                ActiveStoryArcs = new List<string>(dataState.ActiveStoryArcs ?? new List<string>()),
                RecentEvents = new List<string>(dataState.RecentEvents ?? new List<string>()),
                StateTimestamp = dataState.StateTimestamp,
                NarrativeFlagsSet = new HashSet<string>(dataState.NarrativeFlagsSet ?? new HashSet<string>()),
                EducationalProgress = dataState.EducationalProgress,
                CurrentArcId = dataState.CurrentArcId,
                CurrentBeatId = dataState.CurrentBeatId,
                AvailableChoices = new List<string>(dataState.AvailableChoices ?? new List<string>())
            };
        }
        
        /// <summary>
        /// Convert Systems namespace NarrativeState to Data namespace NarrativeState
        /// </summary>
        public static ProjectChimera.Data.Narrative.NarrativeState ToDataNarrativeState(this ProjectChimera.Systems.Narrative.NarrativeState systemsState)
        {
            if (systemsState == null) return null;
            
            return new ProjectChimera.Data.Narrative.NarrativeState
            {
                CurrentMainStory = systemsState.CurrentMainStory,
                ActiveStoryArcs = new List<string>(systemsState.ActiveStoryArcs ?? new List<string>()),
                RecentEvents = new List<string>(systemsState.RecentEvents ?? new List<string>()),
                StateTimestamp = systemsState.StateTimestamp,
                NarrativeFlagsSet = new HashSet<string>(systemsState.NarrativeFlagsSet ?? new HashSet<string>()),
                EducationalProgress = systemsState.EducationalProgress,
                CurrentArcId = systemsState.CurrentArcId,
                CurrentBeatId = systemsState.CurrentBeatId,
                AvailableChoices = new List<string>(systemsState.AvailableChoices ?? new List<string>())
            };
        }
    }
    
    /// <summary>
    /// Narrative graph structure
    /// </summary>
    [Serializable]
    public class NarrativeGraph
    {
        public Dictionary<string, List<string>> BeatConnections = new Dictionary<string, List<string>>();
        public Dictionary<string, NarrativeNode> Nodes = new Dictionary<string, NarrativeNode>();
    }
    
    /// <summary>
    /// Narrative node data
    /// </summary>
    [Serializable]
    public class NarrativeNode
    {
        public string NodeId;
        public StoryBeatSO Beat;
        public List<string> ConnectedNodes = new List<string>();
        public Dictionary<string, object> NodeData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Narrative event data
    /// </summary>
    [Serializable]
    public class NarrativeEvent
    {
        public string EventId;
        public NarrativeEventType EventType;
        public object EventData;
        public DateTime Timestamp;
        public float Priority = 1.0f;
    }
    
    /// <summary>
    /// Emotional profile tracking
    /// </summary>
    [Serializable]
    public class EmotionalProfile
    {
        public Dictionary<string, float> CharacterAttachments = new Dictionary<string, float>();
        public Dictionary<string, float> StoryInvestments = new Dictionary<string, float>();
        public float OverallEngagement = 0.5f;
        public EmotionalTrend Trend = EmotionalTrend.Stable;
    }
    
    /// <summary>
    /// Player emotional response
    /// </summary>
    [Serializable]
    public class EmotionalResponse
    {
        public float Engagement = 0.5f;
        public float Satisfaction = 0.5f;
        public float Interest = 0.5f;
        public float Confusion = 0f;
        public EmotionalState PrimaryEmotion = EmotionalState.Neutral;
    }
    
    /// <summary>
    /// Emotional data analysis
    /// </summary>
    [Serializable]
    public class EmotionalData
    {
        public float EngagementLevel = 0.5f;
        public Dictionary<string, float> EmotionalMetrics = new Dictionary<string, float>();
        public List<EmotionalTrigger> Triggers = new List<EmotionalTrigger>();
        public EmotionalTrend Trend = EmotionalTrend.Stable;
    }
    
    /// <summary>
    /// Emotional trigger identification
    /// </summary>
    [Serializable]
    public class EmotionalTrigger
    {
        public string TriggerId;
        public TriggerType Type;
        public float Impact = 0.5f;
        public string Description;
    }
    
    // Enums
    public enum RelationshipStatus
    {
        Hostile,
        Unfriendly,
        Neutral,
        Friendly,
        Close,
        Intimate
    }
    
    public enum ActionType
    {
        Dialogue,
        Choice,
        SkillUse,
        ItemGive,
        ItemTake,
        Help,
        Ignore,
        Betray,
        Support
    }
    
    // MemoryType enum is defined in ProjectChimera.Data.Narrative.NarrativeDataStructures.cs - removed duplicate
    
    public enum DialogueLocation
    {
        Indoor,
        Outdoor,
        Greenhouse,
        Laboratory,
        Office,
        Home,
        Public,
        Private
    }
    
    public enum EmotionalTrend
    {
        Declining,
        Stable,
        Improving,
        Volatile
    }
    
    public enum TriggerType
    {
        Character,
        Story,
        Choice,
        Outcome,
        Music,
        Visual,
        Educational
    }
    
    // Placeholder classes for compilation
    public class PlayerResponse { }
    public class StoryProgressionEvent { }
    public class EducationalOutcome { }
    public class AnalyticsProfile { }
    public class AnalyticsReport { }
    public class DialogueInteraction { }
    public class ValidationProfile { }
    public class ScientificValidation { }
    public class ValidationDatabase { }
    public class ComplianceReport { }
    public class ConsequenceHistory { }
    public class EmotionalReport { }
    
    [System.Serializable]
    public class PerformanceMetrics
    {
        public float FrameRate;
        public float MemoryUsage;
        public float LoadTime;
        public float ProcessingTime;
        public int ActiveObjects;
        public float GPUUsage;
        public float CPUUsage;
        public System.DateTime Timestamp;
        public float AverageUpdateTime;
    }
}