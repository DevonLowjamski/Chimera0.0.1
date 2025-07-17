using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutomationUnlockEventData = ProjectChimera.Core.Events.AutomationUnlockEventData;

namespace ProjectChimera.Data.Narrative
{
    // Enums required by the Narrative System

    public enum NarrativeEventType
    {
        CharacterEncounter,
        DialogueStarted,
        DialogueCompleted,
        ChoiceMade,
        ConsequenceTriggered,
        QuestStarted,
        QuestProgress,
        QuestCompleted,
        BranchUnlocked,
        EducationalMoment,
        StoryCompleted,
        CharacterIntroduction,
        CharacterIntroduced,
        ChoicePresented,
        StoryStarted
    }
    
    // Extension methods for NarrativeEventType
    public static class NarrativeEventTypeExtensions
    {
        public static string ToStringValue(this NarrativeEventType eventType)
        {
            return eventType.ToString();
        }
    }
    
    // Wrapper struct for NarrativeEventType that supports implicit conversion to string
    [Serializable]
    public struct EventTypeWrapper
    {
        public NarrativeEventType Value;
        
        public EventTypeWrapper(NarrativeEventType value)
        {
            Value = value;
        }
        
        public static implicit operator string(EventTypeWrapper wrapper)
        {
            return wrapper.Value.ToString();
        }
        
        public static implicit operator EventTypeWrapper(NarrativeEventType eventType)
        {
            return new EventTypeWrapper(eventType);
        }
        
        public static implicit operator NarrativeEventType(EventTypeWrapper wrapper)
        {
            return wrapper.Value;
        }
        
        public override string ToString() => Value.ToString();
    }

    public enum MemoryType
    {
        DialogueChoice,
        PlayerAction,
        KeyObservation,
        FirstMeeting,
        Conversation,
        SharedExperience,
        Conflict,
        Achievement,
        Betrayal,
        Support,
        Learning
    }

    public enum ConsequenceType
    {
        TrustChange,
        RespectChange,
        InfluenceChange,
        RelationshipStatus,
        EconomicImpact,
        EnvironmentalChange,
        CultivationEffect,
        ProgressionUnlock,
        GradualRelationshipChange
    }

    public enum ConsequenceSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical
    }

    public enum RelationshipType
    {
        Neutral,
        Stranger,
        Acquaintance,
        Friend,
        Mentor,
        Rival,
        Business,
        Romantic,
        Family,
        Enemy,
        Ally,
        BestFriend,
        Friendly,
        Unfriendly,
        Hostile
    }

    public enum EmotionalState
    {
        Neutral,
        Happy,
        Excited,
        Calm,
        Anxious,
        Frustrated,
        Angry,
        Sad,
        Curious,
        Impressed,
        Disappointed,
        Confident,
        Worried,
        Optimistic,
        Skeptical,
        Fearful
    }

    public enum CultivationExpertise
    {
        None,
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Genetics,
        Breeding,
        Hydroponics,
        OrganicSoil,
        Lighting,
        Environmental,
        Harvesting,
        Processing,
        Business,
        Compliance,
        Nutrition,
        IPM,
        PostHarvest,
        Technology,
        Legal,
        All
    }

    public enum CultivationTopic
    {
        GrowthStages,
        Genetics,
        Breeding,
        Nutrients,
        Lighting,
        Temperature,
        Humidity,
        pH,
        Watering,
        Pruning,
        Training,
        Harvesting,
        Drying,
        Curing,
        PestControl,
        DiseaseManagement,
        EnvironmentalControl,
        Equipment,
        Business,
        Compliance,
        Safety,
        Quality,
        Testing,
        Processing,
        Extraction
    }

    public enum StoryArcCategory
    {
        NoviceGrower,
        Entrepreneur,
        MasterBreeder,
        CommunityLeader,
        InnovationPioneer,
        Sidestory,
        Event
    }

    public enum DependencyType
    {
        Sequential,
        Parallel,
        Optional,
        Exclusive
    }

    public enum BranchingType
    {
        PlayerChoice,
        Conditional,
        Random,
        Timed,
        SkillCheck,
        RandomEvent,
        CharacterRelationship,
        GameState
    }

    public enum ConditionType
    {
        PlantsHarvested,
        BusinessRevenue,
        StrainsBred,
        CommunityReputation,
        TechnologicalAdvancement,
        TimeSpent,
        CompletedArcs,
        SkillLevel,
        Achievement,
        Custom
    }

    public enum ComparisonType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        NotContains
    }

    public enum TaskType
    {
        Watering,
        Fertilizing,
        Pruning,
        Training,
        Harvesting,
        Transplanting,
        PestControl,
        EnvironmentalAdjustment,
        Monitoring,
        Maintenance,
        Breeding,
        Processing,
        Research,
        Construction,
        Automation
    }

    public enum UnlockedSystems
    {
        BasicCultivation,
        AdvancedNutrients,
        Hydroponics,
        Breeding,
        Automation,
        Processing,
        BusinessManagement,
        Research,
        CommunityFeatures,
        AdvancedEnvironmental,
        Genetics,
        Extraction,
        Compliance,
        Analytics
    }

    public enum EmotionalTone
    {
        Neutral,
        Uplifting,
        Inspiring,
        Calming,
        Exciting,
        Mysterious,
        Dramatic,
        Humorous,
        Serious,
        Contemplative,
        Nostalgic,
        Hopeful,
        Melancholic,
        Triumphant,
        Suspenseful,
        Romantic,
        Adventurous,
        Peaceful,
        Intense,
        Whimsical
    }

    // Data Structures required by the Narrative System

    [Serializable]
    public class NarrativeEventMessage
    {
        public string CharacterId;
        public string DialogueId;
        public string QuestId;
        public string EventType;
        public NarrativeEventType Type;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public string StoryId;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public DateTime Timestamp;
        public string ChoiceId;
    }

    [Serializable]
    public class Consequence
    {
        public string ConsequenceId;
        public ConsequenceType Type;
        public string Description;
        public ConsequenceSeverity Severity;
        public bool IsImmediate;
        public float DelayTime;
        public float Duration;
        public List<string> CharacterTargets = new List<string>();
        public List<string> NarrativeFlags = new List<string>();
        public float Desirability;
        public float EducationalImpact;
        public string TriggeringChoiceId;
        public DateTime Timestamp;
        public Dictionary<string, float> RelationshipImpacts = new Dictionary<string, float>();
        public EducationalConsequence EducationalContent;
    }

    [Serializable]
    public class ConsequenceTemplate
    {
        public string TemplateId;
        public ConsequenceType Type;
        public string Description;
        public ConsequenceSeverity Severity;
        public bool IsImmediate;
        public float DelayTime;
        public float Duration;
        public List<string> CharacterTargets = new List<string>();
        public List<string> NarrativeFlags = new List<string>();
        public float Desirability;
        public float EducationalImpact;
        public List<string> TriggeredByChoices = new List<string>();
        public List<RelationshipModifier> RelationshipModifiers = new List<RelationshipModifier>();
        public bool HasEducationalContent;
        public string EducationalTopic;
        public bool IsScientificallyAccurate;
        public float LearningReinforcementLevel;
    }

    [Serializable]
    public class RelationshipModifier
    {
        public string CharacterId;
        public float ImpactValue;
    }

    [Serializable]
    public class LearningMoment
    {
        public string MomentId;
        public string Topic;
        public string SpeakerId;
        public string Content;
        public float EducationalValue;
        public DateTime Timestamp;
        [SerializeField] private bool _isReinforcement;
        public string BranchId;
        
        public bool IsReinforcement 
        { 
            get => _isReinforcement; 
            set => _isReinforcement = value; 
        }
        
        // Method for compatibility
        public bool GetIsReinforcement() { return _isReinforcement; }
    }

    [Serializable]
    public class EducationalProgressTracker
    {
        public List<LearningMoment> LearningMoments = new List<LearningMoment>();
        public List<EducationalMilestone> Milestones = new List<EducationalMilestone>();
        
        public void AddLearningMoment(LearningMoment moment) => LearningMoments.Add(moment);
        public float GetTotalEducationalValue() => LearningMoments.Sum(lm => lm.EducationalValue);
        public bool HasMilestone(string milestoneId) => Milestones.Any(m => m.MilestoneId == milestoneId);
        public void AddMilestone(EducationalMilestone milestone) => Milestones.Add(milestone);
        public int GetMilestoneCount() => Milestones.Count;
    }

    [Serializable]
    public class NarrativeState
    {
        public string CurrentMainStory;
        public List<string> ActiveStoryArcs = new List<string>();
        public List<string> RecentEvents = new List<string>();
        public DateTime StateTimestamp;
        public HashSet<string> NarrativeFlagsSet = new HashSet<string>();
        public Dictionary<string, CharacterRelationship> CharacterRelationshipStates = new Dictionary<string, CharacterRelationship>();
        public List<PlayerChoice> PlayerChoiceHistory = new List<PlayerChoice>();
        public EducationalProgressTracker EducationalProgress = new EducationalProgressTracker();
        public string CurrentBeatId;
        public string PreviousBeatId;
        public DateTime LastDecisionTime;
        public int DecisionCount;
        public List<string> AvailableChoices = new List<string>();
        public List<StoryChoice> AvailableChoicesDetailed = new List<StoryChoice>();
        public List<string> AvailableChoiceIds = new List<string>();
        public string StateId;
        public string CurrentArcId;
        public DateTime Timestamp;
        public NarrativeFlags NarrativeFlagsObject; // NarrativeFlags object for compatibility
        
        // Additional properties for NarrativeManager compatibility
        public string CurrentMainStory2;
        public List<StoryArcSO> ActiveStoryArcs2 = new List<StoryArcSO>();
        public DateTime StateTimestamp2;
        public string IsMainStory;
        public string CurrentMainStory3;
        
        // Missing properties from error messages  
        public string CurrentMainStory4;
        public List<string> ActiveStoryArcs3 = new List<string>();
        public string RecentEvents2;
        public string StateTimestamp3;
        
        // Property to provide access to NarrativeFlagsSet for compatibility
        public HashSet<string> NarrativeFlags 
        { 
            get => NarrativeFlagsSet ?? (NarrativeFlagsSet = new HashSet<string>());
            set => NarrativeFlagsSet = value ?? new HashSet<string>();
        }
        
    }

    [Serializable]
    public class PlayerChoice
    {
        public string ChoiceId;
        public string ChoiceText;
        public string DialogueId;
        public DateTime ChoiceTimestamp;
        public List<string> ConsequenceIds = new List<string>();
        public float ChoiceWeight;
        public Dictionary<string, object> ChoiceData = new Dictionary<string, object>();
        public string ResolutionNodeId;
        public string ResultingNodeId;
    }

    [Serializable]
    public class CharacterRelationship
    {
        public string CharacterId;
        public RelationshipType Type;
        public float TrustLevel;
        public float RespectLevel;
        public float InfluenceLevel;
        public float RelationshipLevel;
        public EmotionalState CurrentEmotion;
        public DateTime LastInteraction;
        public List<string> SharedHistory = new List<string>();
        public Dictionary<string, float> AttributeModifiers = new Dictionary<string, float>();
        
        // Default constructor
        public CharacterRelationship() { }
        
        // Constructor with characterId
        public CharacterRelationship(string characterId)
        {
            CharacterId = characterId;
            Type = RelationshipType.Neutral;
            TrustLevel = 50f;
            RespectLevel = 50f;
            InfluenceLevel = 50f;
            RelationshipLevel = 50f;
            CurrentEmotion = EmotionalState.Neutral;
            LastInteraction = DateTime.Now;
            SharedHistory = new List<string>();
            AttributeModifiers = new Dictionary<string, float>();
        }
        
        public float GetOverallRelationshipLevel()
        {
            return (TrustLevel + RespectLevel + InfluenceLevel) / 3f;
        }
        
        public void ModifyTrust(float change)
        {
            TrustLevel = Mathf.Clamp(TrustLevel + change, 0f, 100f);
            UpdateOverallRelationship();
        }
        
        public void ModifyRespect(float change)
        {
            RespectLevel = Mathf.Clamp(RespectLevel + change, 0f, 100f);
            UpdateOverallRelationship();
        }
        
        public void ModifyInfluence(float change)
        {
            InfluenceLevel = Mathf.Clamp(InfluenceLevel + change, 0f, 100f);
            UpdateOverallRelationship();
        }
        
        private void UpdateOverallRelationship()
        {
            RelationshipLevel = GetOverallRelationshipLevel();
        }
    }

    [Serializable]
    public class ConsequenceData
    {
        public string ConsequenceId;
        public string TargetId;
        public ConsequenceType Type;
        public string Description;
        public ConsequenceSeverity Severity;
        public float Magnitude;
        public bool IsProcessed;
        public DateTime Timestamp;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }

    [Serializable]
    public class PlayerProgressData
    {
        public int PlayerLevel;
        public Dictionary<string, float> SkillLevels = new Dictionary<string, float>();
        public List<string> UnlockedAchievements = new List<string>();
        public List<string> CompletedArcs = new List<string>();
        public List<string> ActiveArcs = new List<string>();
        public List<string> RecentlyCompletedArcs = new List<string>();
        public float TotalPlantsHarvested;
        public float TotalRevenue;
        public float StrainsBred;
        public float CommunityReputation;
        public float TechnologyLevel;
        public float TotalPlayTime;
    }

    [Serializable]
    public class PlayerPreferences
    {
        public float EducationalContentPreference = 0.7f;
        public float PreferredDifficulty = 0.5f;
        public Dictionary<StoryArcCategory, float> StoryTypePreferences = new Dictionary<StoryArcCategory, float>();
        
        public float GetStoryTypePreference(StoryArcCategory category)
        {
            return StoryTypePreferences.TryGetValue(category, out float preference) ? preference : 0.5f;
        }
    }

    [Serializable]
    public class PlayerChoiceData
    {
        public string ChoiceId;
        public string StoryId;
        public string ChoiceText;
        public DateTime ChoiceTime;
        public DateTime Timestamp;
        public int SelectedOptionIndex;
        public string ResultingNodeId;
        public List<string> ConsequenceIds = new List<string>();
        public Dictionary<string, object> ChoiceMetadata = new Dictionary<string, object>();
        
        // Conversion constructor from StoryChoice
        public PlayerChoiceData(StoryChoice storyChoice)
        {
            ChoiceId = storyChoice.ChoiceId;
            StoryId = storyChoice.StoryId;
            ChoiceText = storyChoice.ChoiceText;
            ChoiceTime = storyChoice.Timestamp;
            ConsequenceIds = storyChoice.Consequences?.Select(c => c.ConsequenceId).ToList() ?? new List<string>();
            ChoiceMetadata = new Dictionary<string, object>();
        }
        
        // Default constructor
        public PlayerChoiceData() { }
    }

    [Serializable]
    public class BranchingNarrativeState
    {
        public List<string> ActiveBranches = new List<string>();
        public Dictionary<string, float> BranchProbabilities = new Dictionary<string, float>();
        public List<string> AvailableBranches = new List<string>();
        public string CurrentBranch;
        public DateTime LastBranchTime;
        public int BranchDepth;
        public string StateId;
        public string CurrentArcId;
        public string CurrentBeatId;
        public List<PlayerChoiceData> PlayerChoiceHistory = new List<PlayerChoiceData>();
        public Dictionary<string, CharacterRelationship> CharacterRelationshipStates = new Dictionary<string, CharacterRelationship>();
        public Dictionary<string, float> CharacterRelationshipValues = new Dictionary<string, float>();
        public EducationalProgressTracker EducationalProgress = new EducationalProgressTracker();
        public HashSet<string> NarrativeFlagsSet = new HashSet<string>();
        public DateTime Timestamp;
        public DateTime LastDecisionTime;
        public int DecisionCount;
    }

    [Serializable]
    public class GameState
    {
        public string CurrentScene;
        public float GameTime;
        public int PlayerLevel;
        public Dictionary<string, object> GameVariables = new Dictionary<string, object>();
        public List<string> ActiveFlags = new List<string>();
        public DateTime SaveTime;
    }

    [Serializable]
    public class StoryInstance
    {
        public string StoryId;
        public StoryArcSO StoryArc;
        public float Progress;
        public bool IsActive;
        public DateTime StartTime;
        public DateTime? CompletionTime;
        public List<string> CompletedEvents = new List<string>();
        public List<PlayerChoiceData> PlayerChoices = new List<PlayerChoiceData>();
        public Dictionary<string, object> StoryData = new Dictionary<string, object>();
        public int CurrentChapter;
    }

    [Serializable]
    public class StoryChoice
    {
        public string ChoiceId;
        public string ChoiceText;
        public List<string> RequiredFlags = new List<string>();
        public List<Consequence> Consequences = new List<Consequence>();
        public Dictionary<int, List<string>> ConsequencesByIndex = new Dictionary<int, List<string>>();
        public bool IsAvailable = true;
        public float Weight = 1.0f;
        public string StoryId;
        public ChoiceOption SelectedOption;
        public DateTime Timestamp;

        // Conversion constructor from ChoiceOption
        public StoryChoice(ProjectChimera.Data.Narrative.ChoiceOption option)
        {
            ChoiceId = option.OptionId;
            ChoiceText = option.OptionText;
            RequiredFlags = new List<string>(option.RequiredFlags);
            Consequences = new List<Consequence>(option.Consequences);
            IsAvailable = option.IsAvailable;
            Weight = 1.0f;
        }

        // Default constructor
        public StoryChoice() { }
    }

    [Serializable]
    public class CharacterInstance
    {
        public string CharacterId;
        public string CharacterName;
        public RelationshipType RelationshipType;
        public float TrustLevel;
        public float RespectLevel;
        public EmotionalState CurrentEmotion;
        public List<string> InteractionHistory = new List<string>();
        public Dictionary<string, object> CharacterData = new Dictionary<string, object>();
        public CharacterProfileSO CharacterData2;
        public DateTime IntroductionTime;
        public bool IsActive;
        public EmotionalState CurrentMood;
        public List<string> DialogueHistory = new List<string>();
    }

    [Serializable]
    public class CommunityStoryEvent
    {
        public string EventId;
        public string EventName;
        public string Description;
        public DateTime EventTime;
        public List<string> ParticipantIds = new List<string>();
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public bool IsCompleted;
    }


    [Serializable]
    public class CultivationStoryEvent
    {
        public string EventId;
        public string CultivationActivity;
        public string StoryContext;
        public List<string> RelatedStoryIds = new List<string>();
        public DateTime EventTime;
        public Dictionary<string, object> CultivationData = new Dictionary<string, object>();
        public bool IsEducational;
    }

    [Serializable]
    public class CompletedStoryArc
    {
        public string StoryId;
        public DateTime CompletionTime;
        public List<PlayerChoiceData> ChoicesMade = new List<PlayerChoiceData>();
        public float FinalProgress;
        public Dictionary<string, object> CompletionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class LiveEventStoryTrigger
    {
        public string TriggerId;
        public string EventType;
        public string StoryId;
        public List<string> TriggerConditions = new List<string>();
        public bool IsActive = true;
        public DateTime CreatedTime;
        public Dictionary<string, object> TriggerData = new Dictionary<string, object>();
    }

    [Serializable]
    public class EducationalStoryEvent
    {
        public string EventId;
        public string EducationalTopic;
        public string LearningObjective;
        public bool IsScientificallyAccurate = true;
        public List<string> LearningOutcomes = new List<string>();
        public string Difficulty = "beginner";
        public Dictionary<string, object> EducationalData = new Dictionary<string, object>();
    }

    [Serializable]
    public class QuestInstance
    {
        public string QuestId;
        public string QuestName;
        public string Description;
        public bool IsActive;
        public bool IsCompleted;
        public float Progress;
        public DateTime StartTime;
        public DateTime? CompletionTime;
        public List<string> Objectives = new List<string>();
        public Dictionary<string, object> QuestData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CharacterRelationshipData
    {
        public string CharacterId;
        public string CharacterName;
        public RelationshipType RelationshipType;
        public float TrustLevel;
        public float RespectLevel;
        public float InfluenceLevel;
        public EmotionalState CurrentEmotion;
        public List<string> SharedExperiences = new List<string>();
        public Dictionary<string, float> AttributeModifiers = new Dictionary<string, float>();
        public DateTime LastInteraction;
        public List<string> SharedHistory = new List<string>();
        
        // Additional relationship properties
        public string CharacterAId;
        public float InitialTrustLevel;
        public float InitialRespectLevel;
        public float InitialInfluenceLevel;
        public bool IsAntagonistic;
        public float ConflictLevel;
    }

    [Serializable]
    public class RelationshipState
    {
        public Dictionary<string, CharacterRelationshipData> Characters = new Dictionary<string, CharacterRelationshipData>();
        public List<string> RecentInteractions = new List<string>();
        public float OverallSocialStanding;
        public DateTime LastUpdate;
        public string CharacterId;
        public float RelationshipLevel;
        public RelationshipType RelationshipType;
        public DateTime FirstMeeting;
        public DateTime LastInteraction;
    }

    [Serializable]
    public class CultivationFact
    {
        public string FactId;
        public string Topic;
        public string FactText;
        public bool IsScientificallyVerified = true;
        public string SourceReference;
        public string DifficultyLevel = "beginner";
        public List<string> RelatedTopics = new List<string>();
        public Dictionary<string, object> FactData = new Dictionary<string, object>();
    }

    [Serializable]
    public class LearningObjective
    {
        public string ObjectiveId;
        public string ObjectiveText;
        public string Category;
        public string DifficultyLevel = "beginner";
        public bool IsCompleted;
        public float Progress;
        public List<string> PrerequisiteObjectives = new List<string>();
        public List<string> AssociatedFacts = new List<string>();
        public Dictionary<string, object> ObjectiveData = new Dictionary<string, object>();
    }


    // Supporting configuration classes for narrative systems
    [Serializable]
    public class NarrativeConfig
    {
        public bool EnableBranchingNarratives = true;
        public bool EnableCultivationIntegration = true;
        public bool EnableEducationalContent = true;
        public float UpdateInterval = 1.0f;
    }

    [Serializable]
    public class ChoiceManagerConfig
    {
        public bool EnableBranchingNarratives = true;
        public int MaxChoicesPerEvent = 5;
        public float ChoiceTimeoutDuration = 30.0f;
        public bool AllowChoiceUndo = false;
    }

    [Serializable]
    public class CultivationStoryTrigger
    {
        public string TriggerId;
        public string CultivationEvent;
        public string StoryId;
        public List<string> TriggerConditions = new List<string>();
        public bool IsActive = true;
        public Dictionary<string, object> TriggerData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CultivationNarrativeConfig
    {
        public bool EnableCultivationStoryTriggers = true;
        public List<CultivationStoryTrigger> StoryTriggers = new List<CultivationStoryTrigger>();
        public float TriggerCheckInterval = 2.0f;
    }

    // Alias for compatibility
    [Serializable]
    public class NarrativeRelationship
    {
        public string CharacterAId;
        public string CharacterBId;
        public RelationshipType RelationshipType;
        public float InitialTrustLevel = 0.5f;
        public float InitialRespectLevel = 0.5f;
        public float InitialInfluenceLevel = 0.5f;

        // Conversion constructor from PredefinedRelationship
        public NarrativeRelationship(ProjectChimera.Data.Narrative.PredefinedRelationship predefined)
        {
            CharacterAId = predefined.CharacterAId;
            CharacterBId = predefined.CharacterBId;
            RelationshipType = predefined.RelationshipType;
            InitialTrustLevel = predefined.InitialTrustLevel;
            InitialRespectLevel = predefined.InitialRespectLevel;
            InitialInfluenceLevel = predefined.InitialInfluenceLevel;
        }

        // Default constructor
        public NarrativeRelationship() { }
    }

    // Educational consequence tracking
    [Serializable]
    public class EducationalConsequence
    {
        public string ConsequenceId;
        public string EducationalTopic;
        public List<string> LearningOutcomes = new List<string>();
        public float EducationalValue = 0.7f;
        public bool IsScientificallyAccurate = true;
        public string ValidatedBy;
        public DateTime ValidationDate;
        public string Topic;
        public float LearningValue;
        public float ReinforcementLevel;
        public List<string> KeyConcepts = new List<string>();
        public string LearningOutcome;
    }

    // Supporting classes for BranchingNarrativeEngine
    [Serializable]
    public class NarrativeBranch
    {
        public string BranchId;
        public string BranchName;
        public List<string> RequiredConditions = new List<string>();
        public float Probability = 1.0f;
        public bool IsActive = true;
        public DateTime CreatedTime;
        public Dictionary<string, object> BranchData = new Dictionary<string, object>();
    }

    [Serializable]
    public class BranchNode
    {
        public string NodeId;
        public string NodeContent;
        public List<string> ConnectedNodes = new List<string>();
        public Dictionary<string, object> NodeData = new Dictionary<string, object>();
    }

    [Serializable]
    public class NarrativeEvent
    {
        public string EventId;
        public NarrativeEventType EventType;
        public string Description;
        public DateTime EventTime;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public object Data; // Additional data object for specific event types
    }

    [Serializable]
    public class CachedBranch
    {
        public string BranchId;
        public NarrativeBranch Branch;
        public DateTime CacheTime;
        public bool IsValid = true;
    }

    [Serializable]
    public class PlayerDecision
    {
        public string DecisionId;
        public string ChoiceText;
        public DateTime DecisionTime;
        public DateTime Timestamp;
        public Dictionary<string, object> DecisionData = new Dictionary<string, object>();
        public StoryChoice Choice;
        public DecisionContext Context;
    }

    [Serializable]
    public class DecisionContext
    {
        public string ContextId;
        public Dictionary<string, object> ContextData = new Dictionary<string, object>();
        
        public Dictionary<string, object> AdditionalData()
        {
            return ContextData;
        }
    }

    [Serializable]
    public class EducationalMilestone
    {
        public string MilestoneId;
        public string MilestoneName;
        public string EducationalTopic;
        public DateTime AchievedTime;
        public Dictionary<string, object> MilestoneData = new Dictionary<string, object>();
        public string Description;
        public float EducationalValue;
    }

    [Serializable]
    public class NarrativePerformanceMetrics
    {
        public int TotalBranchesGenerated;
        public float AverageBranchGenerationTime;
        public int ActiveBranches;
        public DateTime LastUpdate;
        public float AverageUpdateTime;
    }

    // Supporting utility classes for narrative systems
    public class BranchProbabilityCalculator
    {
        public void Initialize(CampaignConfigSO config) { }
        public float CalculateBranchProbability(string branchId, Dictionary<string, object> context) { return 1.0f; }
        public float CalculateBranchProbability(PlayerDecision decision, NarrativeState state) { return 1.0f; }
    }

    public class ConsequencePredictor
    {
        public void Initialize() { }
        public List<Consequence> PredictConsequences(string choiceId, NarrativeState state) { return new List<Consequence>(); }
        public List<Consequence> PredictConsequences(PlayerDecision decision, BranchNode targetNode, NarrativeState state) { return new List<Consequence>(); }
    }

    public class EducationalFlowValidator
    {
        public void Initialize() { }
        public bool ValidateEducationalContent(string content) { return true; }
    }

    // These system classes are defined as MonoBehaviour components in the Systems folder - removed duplicates

    // ChoiceOption class for StoryChoiceSO compatibility
    [Serializable]
    public class ChoiceOption
    {
        public string OptionId;
        public string OptionText;
        public string Description;
        public List<string> RequiredFlags = new List<string>();
        public List<Consequence> Consequences = new List<Consequence>();
        public bool IsAvailable = true;
    }

    // Additional manager classes for NarrativeManager
    public class ChoiceManager
    {
        public ChoiceManager() { }
        public ChoiceManager(NarrativeConfig narrativeConfig) { }
        public ChoiceManager(StoryEventConfigSO storyEventConfig, NarrativeConfigSO narrativeConfig) { }
        public ChoiceManager(NarrativeConfigSO narrativeConfig) { }
        public ChoiceManager(object config, NarrativeConfigSO narrativeConfig) { }
        public void EnableBranchingNarratives(bool enable) { }
        public void ChoiceManagerProcessChoice(string choiceId) { }
    }

    public class CultivationNarrativeIntegrator
    {
        public event System.Action<CultivationStoryTrigger> OnCultivationStoryTrigger;
        
        public CultivationNarrativeIntegrator() { }
        public CultivationNarrativeIntegrator(NarrativeConfig config) { }
        public CultivationNarrativeIntegrator(CultivationNarrativeConfig config) { }
        public CultivationNarrativeIntegrator(NarrativeConfigSO config) { }
        public void Initialize() { }
        public void TriggerEvent(CultivationStoryTrigger trigger) 
        { 
            OnCultivationStoryTrigger?.Invoke(trigger); 
        }
        public void HandleCultivationStoryTrigger(CultivationStoryTrigger trigger)
        {
            TriggerEvent(trigger);
        }
    }

    // Additional missing data structures
    [Serializable]
    public class DialogueChoice
    {
        public string ChoiceId;
        public string ChoiceText;
        public List<string> RequiredFlags = new List<string>();
        public bool IsAvailable = true;
    }

    [Serializable]
    public class CharacterPersonality
    {
        public string PersonalityId;
        public string PersonalityName;
        public Dictionary<string, float> Traits = new Dictionary<string, float>();
        
        // Specific personality traits for CharacterRelationshipSystem compatibility
        public float Loyalty;
        public float Strictness;
        public float Compassion;
        public float Independence;
        public float Leadership;
        public float Skepticism;
    }

    [Serializable]
    public class CharacterMemory
    {
        public string MemoryId;
        public MemoryType Type;
        public string Content;
        public DateTime CreatedTime;
        public float Importance = 1.0f;
    }

    [Serializable]
    public class NarrativeFlags
    {
        public HashSet<string> Flags = new HashSet<string>();
        public void AddFlag(string flag) => Flags.Add(flag);
        public void Add(string flag) => Flags.Add(flag);
        public bool HasFlag(string flag) => Flags.Contains(flag);
    }

    // Additional missing classes for NarrativeManager
    [Serializable]
    public class NarrativePreferences
    {
        public float EducationalContentPreference = 0.7f;
        public float PreferredDifficulty = 0.5f;
        public Dictionary<StoryArcCategory, float> StoryTypePreferences = new Dictionary<StoryArcCategory, float>();
    }

    [Serializable]
    public class PlayerNarrativeProfile
    {
        public string PlayerId;
        public Dictionary<string, float> StoryProgression = new Dictionary<string, float>();
        public List<string> CompletedStories = new List<string>();
        public List<PlayerChoiceData> ChoiceHistory = new List<PlayerChoiceData>();
        public PlayerPreferences Preferences = new PlayerPreferences();
        public Dictionary<string, CharacterRelationship> CharacterRelationships = new Dictionary<string, CharacterRelationship>();
        public DateTime LastPlayTime;
        public PlayerPreferences NarrativePreferences = new PlayerPreferences();
        
        // Additional properties for NarrativeManager compatibility
        public string UnlockedContent;
        public List<string> CharacterMeetings = new List<string>();
        public List<string> CompletedChoices = new List<string>();
    }

    [Serializable] 
    public class NarrativeMetrics
    {
        public int StoryProgressUpdates;
        public int ChoicesMade;
        public int ConsequencesTriggered;
        public int CharacterInteractions;
        public float TotalPlayTime;
        public Dictionary<string, int> StoryCompletions = new Dictionary<string, int>();
        public Dictionary<string, float> EngagementScores = new Dictionary<string, float>();
        
        // Additional properties for NarrativeManager compatibility
        public int UpdateErrors;
        public int IntegrationErrors;
        public int CharacterUpdateErrors;
        public int StoriesStarted;
        public int StoriesCompleted;
        public int CharactersIntroduced;
        
        public void UpdateError(string error) { UpdateErrors++; }
        public void CharacterUpdateError(string error) { CharacterUpdateErrors++; }
    }

    // Missing ScriptableObject references
    [Serializable]
    public class StoryEventConfigSO
    {
        public string ConfigId;
        public string ConfigName;
        public bool EnableEventLogging = true;
        public float EventTimeoutDuration = 30.0f;
        public int MaxConcurrentEvents = 5;
    }

    // Additional event data types for StoryCampaignManager
    [Serializable]
    public class DecisionEventData
    {
        public string DecisionId;
        public string DecisionDescription;
        public DateTime DecisionTime;
        public Dictionary<string, object> DecisionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CampaignEventData
    {
        public string CampaignId;
        public string EventType;
        public DateTime EventTime;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }

    // CharacterDataSO for character data management
    [System.Serializable]
    public class CharacterDataSO
    {
        public string CharacterName;
        public string CharacterDescription;
        public bool IsCompanion;
        public CultivationExpertise Expertise;
        public CharacterPersonality Personality;
        public EmotionalState DefaultEmotion;
        public Dictionary<string, object> CharacterData = new Dictionary<string, object>();
    }

    // Missing types for StoryCampaignManager
    [Serializable]
    public static class StoryEventTypeConstants
    {
        public const string ArcStarted = "ArcStarted";
        public const string ArcCompleted = "ArcCompleted";
        public const string ChapterStarted = "ChapterStarted";
        public const string ChapterCompleted = "ChapterCompleted";
    }

    [Serializable]
    public class NarrativeStoryEventData
    {
        public string EventId;
        public string EventType;  // Changed to string to use constants from StoryEventTypeConstants
        public string ArcId;
        public DateTime Timestamp;
        
        // Constructor that accepts NarrativeEventType and converts to string
        public NarrativeStoryEventData(string eventId, NarrativeEventType eventType, string arcId)
        {
            EventId = eventId;
            EventType = eventType.ToString();
            ArcId = arcId;
            Timestamp = DateTime.Now;
        }
        
        // Default constructor
        public NarrativeStoryEventData() { }
    }

    // CharacterEventData is defined in CharacterEventData.cs - removed duplicate

    // Missing types for narrative system managers
    [Serializable]
    public class ValidationResult
    {
        public bool IsValid;
        public string Message;
        public List<string> Errors = new List<string>();
    }

    [Serializable]
    public class EducationalContent
    {
        public string ContentId;
        public string Topic;
        public string Content;
        public bool IsScientificallyAccurate = true;
        public List<string> References = new List<string>();
    }

    // SimpleGameEventSO is defined in ProjectChimera.Core.GameEventSO.cs - removed duplicate

    // StoryArcLibrarySO is defined in StoryArcLibrarySO.cs - removed duplicate

    // StoryEventSO is defined in StoryEventSO.cs - removed duplicate

    // ReportType is defined as an enum in NarrativeAnalyticsConfigSO.cs - removed duplicate class

    // ScientificValidation is defined in StoryArcLibrarySO.cs - removed duplicate

    [System.Serializable]
    public class ComplianceReport
    {
        public bool IsCompliant;
        public List<string> Issues = new List<string>();
        public DateTime GeneratedDate;
    }

    // Missing types from StoryCampaignManager and other files
    [System.Serializable]
    public class BranchingNarrativeEngine
    {
        public void Initialize(CampaignConfigSO config, StoryArcLibrarySO storyLibrary) { }
        public void ProcessPlayerDecision(PlayerDecision decision) { }
        public void Update(float deltaTime) { }
        public NarrativeState GetCurrentNarrativeState() { return new NarrativeState(); }
    }

    [System.Serializable]
    public class CharacterRelationshipManager
    {
        public void Initialize(CharacterDatabaseSO database, CampaignConfigSO config) { }
        public void Update(float deltaTime) { }
    }

    // DialogueProcessingEngine and ConsequenceTrackingSystem are defined as MonoBehaviour classes in Systems folder - removed duplicates

    // Fix type mismatch errors for StoryArcSO.IsMainStory property
    public static class StoryArcSOExtensions
    {
        public static bool IsMainStory(this StoryArcSO storyArc)
        {
            return storyArc?.Category == StoryArcCategory.NoviceGrower || 
                   storyArc?.Category == StoryArcCategory.Entrepreneur ||
                   storyArc?.Category == StoryArcCategory.MasterBreeder;
        }
        
        public static string StartNodeId(this StoryArcSO storyArc)
        {
            return storyArc?.StoryBeats?.FirstOrDefault()?.BeatId ?? "";
        }
    }

    // Extension methods for type conversions
    public static class NarrativeDataExtensions
    {
        public static List<PlayerChoiceData> ToPlayerChoiceDataList(this List<StoryChoice> storyChoices)
        {
            return storyChoices?.Select(sc => new PlayerChoiceData(sc)).ToList() ?? new List<PlayerChoiceData>();
        }
        
        public static List<StoryChoice> ToStoryChoiceList(this List<PlayerChoiceData> playerChoices)
        {
            return playerChoices?.Select(pc => new StoryChoice
            {
                ChoiceId = pc.ChoiceId,
                StoryId = pc.StoryId,
                ChoiceText = pc.ChoiceText,
                Timestamp = pc.ChoiceTime
            }).ToList() ?? new List<StoryChoice>();
        }
        
        // Note: Cross-namespace conversion methods moved to Systems assembly due to assembly reference constraints
        
        // Extension method for List<Consequence> to provide GetValueOrDefault functionality
        public static T GetValueOrDefault<T>(this List<T> list, int index, T defaultValue = default(T))
        {
            return index >= 0 && index < list.Count ? list[index] : defaultValue;
        }
    }

    // Supporting event data classes for Systems.Narrative compatibility
    [Serializable]
    public class CharacterEncounterData
    {
        public string CharacterId;
        public float InitialRelationshipLevel = 50f;
        public string EncounterType;
        public DateTime Timestamp;
    }
    
    [Serializable]
    public class ConsequenceEventData
    {
        public string ConsequenceId;
        public string ConsequenceType;
        public float Impact;
        public string TargetId;
        public DateTime Timestamp;
        public Consequence Consequence;
    }
    
    [Serializable]
    public class EducationalEventData
    {
        public LearningMoment LearningMoment;
        public string EventType;
        public DateTime Timestamp;
        
        // Additional properties for compatibility with TaskDataStructures version
        public string EventId;
        public string Title;
        public string Content;
        public string Subject;
        public int DifficultyLevel;
        public List<string> LearningObjectives = new List<string>();
        public Dictionary<string, object> InteractiveElements = new Dictionary<string, object>();
        public bool IsCompleted;
        public float ComprehensionScore;
    }

}

