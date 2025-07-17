using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// Core tutorial step data structure
    /// </summary>
    [Serializable]
    public class TutorialStepData
    {
        public string StepId;
        public string Title;
        public string Description;
        public string DetailedInstructions;
        public TutorialStepType StepType;
        public TutorialTargetType TargetType;
        public string TargetElementId;
        public Vector2 HighlightOffset;
        public Vector2 HighlightSize;
        public TutorialHighlightShape HighlightShape;
        public TutorialValidationType ValidationType;
        public string ValidationTarget;
        public float TimeoutDuration;
        public bool IsOptional;
        public bool CanSkip;
        public List<string> Prerequisites;
        public List<TutorialHint> Hints;
        public AudioClip NarrationClip;
        public Sprite IllustrationImage;
    }
    
    /// <summary>
    /// Tutorial sequence definition
    /// </summary>
    [Serializable]
    public class TutorialSequenceData
    {
        public string SequenceId;
        public string Name;
        public string Description;
        public TutorialCategory Category;
        public int Priority;
        public bool IsRequired;
        public List<string> StepIds;
        public List<string> UnlockRequirements;
        public TutorialReward CompletionReward;
    }
    
    /// <summary>
    /// Tutorial hint system
    /// </summary>
    [Serializable]
    public class TutorialHint
    {
        public string HintText;
        public float DelayBeforeShow;
        public TutorialHintType HintType;
        public string TargetElement;
        public bool IsContextual;
    }
    
    /// <summary>
    /// Tutorial highlight settings for visual emphasis
    /// </summary>
    [Serializable]
    public class TutorialHighlightSettings
    {
        public bool EnableHighlight = true;
        public Color HighlightColor = Color.yellow;
        public float HighlightIntensity = 1f;
        public TutorialHighlightShape HighlightShape = TutorialHighlightShape.Rectangle;
        public Vector2 HighlightOffset = Vector2.zero;
        public Vector2 HighlightSize = new Vector2(100, 50);
        public float PulseSpeed = 2f;
        public bool EnablePulse = true;
        public float BorderWidth = 2f;
        public bool DimBackground = true;
        public float BackgroundDimAmount = 0.7f;
        public AnimationCurve HighlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Additional properties for TutorialOverlayManager compatibility
        public bool HasHighlight => EnableHighlight; // Compatibility alias
        public TutorialHighlightType HighlightType = TutorialHighlightType.Rectangle; // For overlay manager
        public string TargetElement = ""; // Target element ID for highlighting
        public float HighlightPadding = 10f; // Padding around highlight area
    }
    
    /// <summary>
    /// Tutorial completion reward
    /// </summary>
    [Serializable]
    public class TutorialReward
    {
        public TutorialRewardType RewardType;
        public int Amount;
        public string ItemId;
        public string Description;
    }
    
    /// <summary>
    /// Tutorial progress tracking
    /// </summary>
    [Serializable]
    public class TutorialProgress
    {
        public string TutorialId;
        public string CurrentStepId;
        public TutorialStatus Status;
        public float ProgressPercentage;
        public DateTime StartTime;
        public DateTime LastUpdateTime;
        public int CompletedSteps;
        public int TotalSteps;
        public bool IsCompleted;
        public bool WasSkipped;
        public List<string> CompletedStepIds;
        public Dictionary<string, object> CustomData;
    }
    
    /// <summary>
    /// Tutorial session statistics
    /// </summary>
    [Serializable]
    public class TutorialSessionStats
    {
        public string SessionId;
        public DateTime SessionStart;
        public DateTime SessionEnd;
        public float TotalDuration;
        public int StepsCompleted;
        public int HintsUsed;
        public int StepsSkipped;
        public bool SessionCompleted;
        public TutorialDifficulty PerceivedDifficulty;
        public List<TutorialInteraction> Interactions;
    }
    
    /// <summary>
    /// Tutorial interaction tracking
    /// </summary>
    [Serializable]
    public class TutorialInteraction
    {
        public string StepId;
        public TutorialInteractionType InteractionType;
        public DateTime Timestamp;
        public Vector2 Position;
        public string TargetElement;
        public float Duration;
        public bool WasSuccessful;
    }
    
    /// <summary>
    /// Tutorial validation result
    /// </summary>
    public struct TutorialValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public string NextStepId;
        public TutorialValidationFeedback Feedback;
        public object ValidationData;
    }
    
    /// <summary>
    /// Tutorial event data
    /// </summary>
    [Serializable]
    public struct TutorialEventData
    {
        public string TutorialId;
        public string StepId;
        public TutorialEventType EventType;
        public string Message;
        public DateTime Timestamp;
        public object AdditionalData;
    }
    
    // Enumerations
    
    /// <summary>
    /// Tutorial step types
    /// </summary>
    public enum TutorialStepType
    {
        Introduction,
        Instruction,
        Interaction,
        Validation,
        Information,
        Choice,
        Completion,
        Transition,
        Problem_Solving,
        Assessment,
        Project
    }
    
    /// <summary>
    /// Tutorial target types for highlighting
    /// </summary>
    public enum TutorialTargetType
    {
        None,
        UIElement,
        GameObject,
        ScreenArea,
        Menu,
        Button,
        Panel,
        Input,
        Custom
    }
    
    /// <summary>
    /// Tutorial highlight shapes
    /// </summary>
    public enum TutorialHighlightShape
    {
        Rectangle,
        Circle,
        Oval,
        RoundedRectangle,
        Polygon,
        Custom
    }
    
    /// <summary>
    /// Tutorial highlight types for overlay manager
    /// </summary>
    public enum TutorialHighlightType
    {
        None,
        Rectangle,
        Circle,
        Oval,
        RoundedRectangle,
        Polygon,
        Custom,
        Glow,
        Border,
        Pulse
    }
    
    /// <summary>
    /// Tutorial validation types
    /// </summary>
    public enum TutorialValidationType
    {
        None,
        ButtonClick,
        InputValue,
        StateChange,
        Timer,
        Custom,
        SystemEvent,
        ManagerState,
        UIInteraction,
        UIElementClick,
        GameObjectClick,
        PlayerInput
    }
    
    /// <summary>
    /// Tutorial categories
    /// </summary>
    public enum TutorialCategory
    {
        Onboarding,
        Basic,
        BasicCultivation,
        AdvancedCultivation,
        Cultivation,
        Genetics,
        Economics,
        FacilityManagement,
        Research,
        Advanced,
        Tips
    }
    
    /// <summary>
    /// Tutorial status
    /// </summary>
    public enum TutorialStatus
    {
        NotStarted,
        InProgress,
        Paused,
        Completed,
        Skipped,
        Failed,
        Abandoned
    }
    
    /// <summary>
    /// Tutorial hint types
    /// </summary>
    public enum TutorialHintType
    {
        Text,
        Arrow,
        Highlight,
        Animation,
        Audio,
        Popup
    }
    
    /// <summary>
    /// Tutorial reward types
    /// </summary>
    public enum TutorialRewardType
    {
        Experience,
        Currency,
        Item,
        Unlock,
        Achievement,
        Skill
    }
    
    /// <summary>
    /// Tutorial difficulty levels
    /// </summary>
    public enum TutorialDifficulty
    {
        VeryEasy = 1,
        Easy = 2,
        Normal = 3,
        Hard = 4,
        VeryHard = 5
    }
    
    /// <summary>
    /// Tutorial difficulty levels (alternative naming)
    /// </summary>
    public enum TutorialDifficultyLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }
    
    /// <summary>
    /// Tutorial interaction types
    /// </summary>
    public enum TutorialInteractionType
    {
        Click,
        Hover,
        Drag,
        Input,
        KeyPress,
        Scroll,
        Navigation,
        Custom
    }
    
    /// <summary>
    /// Tutorial validation feedback
    /// </summary>
    public enum TutorialValidationFeedback
    {
        Success,
        Failure,
        Partial,
        Retry,
        Hint,
        Skip
    }
    
    /// <summary>
    /// Tutorial event types for tracking and analytics
    /// </summary>
    public enum TutorialEventType
    {
        Started,
        TutorialStarted,
        StepStarted,
        StepCompleted,
        StepSkipped,
        TutorialSkipped,
        HintShown,
        ValidationFailed,
        Completed,
        TutorialCompleted,
        Abandoned,
        Paused,
        Resumed
    }
    
    /// <summary>
    /// Tutorial configuration settings
    /// </summary>
    [Serializable]
    public class TutorialSettings
    {
        public bool EnableTutorials = true;
        public bool AllowSkipping = true;
        public bool ShowHints = true;
        public float HintDelay = 5f;
        public bool EnableNarration = true;
        public float NarrationVolume = 0.8f;
        public bool EnableSoundEffects = true;
        public float SoundEffectVolume = 0.7f;
        public bool HighlightTargets = true;
        public Color HighlightColor = Color.yellow;
        public float HighlightPulseSpeed = 2f;
        public bool DimBackground = true;
        public float BackgroundDimAmount = 0.7f;
        public bool PauseGameDuringTutorial = false;
        public TutorialSpeed DefaultSpeed = TutorialSpeed.Normal;
        public bool SaveProgress = true;
        public bool TrackAnalytics = true;
    }
    
    /// <summary>
    /// Tutorial speed settings
    /// </summary>
    public enum TutorialSpeed
    {
        Slow,
        Normal,
        Fast,
        Instant
    }

    // Wrapper classes for EnhancedTutorialManager compatibility
    
    /// <summary>
    /// Tutorial sequence wrapper class for manager compatibility
    /// </summary>
    [Serializable]
    public class TutorialSequence
    {
        public string SequenceId;
        public string Name;
        public string Description;
        public TutorialCategory Category;
        public int Priority;
        public bool IsRequired;
        public List<TutorialStep> Steps = new List<TutorialStep>();
        public List<string> UnlockRequirements = new List<string>();
        public List<string> Prerequisites = new List<string>(); // Prerequisites for this sequence
        public TutorialReward CompletionReward;
        public TutorialDifficultyLevel DifficultyLevel;
        public float EstimatedDuration; // Total estimated duration in minutes
        
        public TutorialSequence() { }
        
        public TutorialSequence(TutorialSequenceData data)
        {
            SequenceId = data.SequenceId;
            Name = data.Name;
            Description = data.Description;
            Category = data.Category;
            Priority = data.Priority;
            IsRequired = data.IsRequired;
            UnlockRequirements = new List<string>(data.UnlockRequirements);
            CompletionReward = data.CompletionReward;
            EstimatedDuration = 10f; // Default 10 minutes
        }
    }

    /// <summary>
    /// Tutorial step wrapper class for manager compatibility
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        public string StepId;
        public string Name;
        public string Description;
        public string DetailedInstructions;
        public TutorialStepType StepType;
        public TutorialTargetType TargetType;
        public string TargetElementId;
        public Vector2 HighlightOffset;
        public Vector2 HighlightSize;
        public TutorialHighlightShape HighlightShape;
        public TutorialValidationType ValidationType;
        public string ValidationTarget;
        public float TimeoutDuration;
        public bool IsOptional;
        public bool CanSkip;
        public List<string> Prerequisites = new List<string>();
        public List<TutorialHint> Hints = new List<TutorialHint>();
        public AudioClip NarrationClip;
        public Sprite IllustrationImage;
        public List<TutorialCondition> CompletionConditions = new List<TutorialCondition>();
        
        // Additional properties for manager compatibility
        public float Progress; // Current progress (0-1)
        public bool AutoComplete; // Whether step auto-completes when conditions are met
        public float EstimatedDuration; // Expected completion time in seconds
        public List<ContextualHint> ContextualHints = new List<ContextualHint>();
        public TutorialCategory Category = TutorialCategory.Onboarding; // Category for step
        public int MaxHints = 3; // Maximum hints allowed for this step
        
        public TutorialStep() { }
        
        public TutorialStep(TutorialStepData data)
        {
            StepId = data.StepId;
            Name = data.Title;
            Description = data.Description;
            DetailedInstructions = data.DetailedInstructions;
            StepType = data.StepType;
            TargetType = data.TargetType;
            TargetElementId = data.TargetElementId;
            HighlightOffset = data.HighlightOffset;
            HighlightSize = data.HighlightSize;
            HighlightShape = data.HighlightShape;
            ValidationType = data.ValidationType;
            ValidationTarget = data.ValidationTarget;
            TimeoutDuration = data.TimeoutDuration;
            IsOptional = data.IsOptional;
            CanSkip = data.CanSkip;
            Prerequisites = new List<string>(data.Prerequisites);
            Hints = new List<TutorialHint>(data.Hints);
            NarrationClip = data.NarrationClip;
            IllustrationImage = data.IllustrationImage;
            
            // Set defaults for additional properties
            Progress = 0f;
            AutoComplete = true;
            EstimatedDuration = 30f; // Default 30 seconds
        }
    }

    /// <summary>
    /// Tutorial condition for step completion validation
    /// </summary>
    [Serializable]
    public class TutorialCondition
    {
        public string ConditionId;
        public TutorialValidationType ValidationType;
        public string TargetElement;
        public string ExpectedValue;
        public bool IsRequired = true;
        public string Description;
    }

    /// <summary>
    /// Player tutorial progress tracking
    /// </summary>
    [Serializable]
    public class PlayerTutorialProgress
    {
        public List<string> CompletedSequences = new List<string>();
        public List<string> CompletedSteps = new List<string>();
        public Dictionary<string, TutorialProgress> SequenceProgress = new Dictionary<string, TutorialProgress>();
        public string CurrentSequenceId;
        public string CurrentStepId;
        public System.DateTime LastPlayTime;
        public int TotalTutorialsCompleted;
        public float TotalTimeSpent;
        public float TotalTutorialTime = 0f; // Total time spent in tutorials
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
        public string PlayerId = "";
        public List<string> SkippedTutorials = new List<string>();
        public TutorialDifficulty PreferredDifficulty = TutorialDifficulty.Normal;
    }

    /// <summary>
    /// Learning analytics data
    /// </summary>
    [Serializable]
    public class LearningAnalytics
    {
        public Dictionary<string, float> StepCompletionTimes = new Dictionary<string, float>();
        public Dictionary<string, int> StepAttempts = new Dictionary<string, int>();
        public Dictionary<string, int> HintsUsed = new Dictionary<string, int>();
        public List<PlayerAction> PlayerActions = new List<PlayerAction>();
        public LearningStyle PreferredLearningStyle = LearningStyle.Visual;
        public float OverallSuccessRate;
        public Dictionary<TutorialCategory, float> CategoryPerformance = new Dictionary<TutorialCategory, float>();
        
        // Additional properties for manager compatibility
        public float AverageActionTime;
        public float ErrorRate;
        public float HintUsageRate;
        public System.DateTime SessionStartTime;
        public float TotalSessionTime;
        public List<string> CompletedTutorials = new List<string>();
    }

    /// <summary>
    /// Player action tracking for analytics
    /// </summary>
    [Serializable]
    public class PlayerAction
    {
        public string ActionId;
        public string ActionType;
        public DateTime Timestamp;
        public string Context;
        public Vector2 Position;
        public float Duration;
        public bool WasSuccessful;
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
        
        // Additional properties for manager compatibility
        public bool IsError => !WasSuccessful;
    }

    /// <summary>
    /// Learning style enumeration
    /// </summary>
    public enum LearningStyle
    {
        Visual,
        Auditory,
        Kinesthetic,
        ReadingWriting,
        Mixed
    }

    /// <summary>
    /// Contextual hint data
    /// </summary>
    [Serializable]
    public class ContextualHint
    {
        public string HintId;
        public string HintText;
        public TutorialHintType HintType;
        public string TargetElement;
        public Vector2 Position;
        public float DelayBeforeShow;
        public float DisplayDuration = 5f;
        public bool IsContextual = true;
        public int Priority = 0;
        
        // Additional properties for manager compatibility
        public string Title;
        public string Content;
        public float MinProgress; // Minimum step progress to show this hint
        public float MaxProgress = 1f; // Maximum step progress to show this hint
    }

    /// <summary>
    /// Tutorial metrics tracking
    /// </summary>
    [Serializable]
    public class TutorialMetrics
    {
        public int TotalTutorialsStarted;
        public int TotalTutorialsCompleted;
        public int TotalStepsCompleted;
        public int TotalHintsShown;
        public float AverageCompletionTime;
        public float OverallSuccessRate;
        public Dictionary<string, float> SequenceCompletionRates = new Dictionary<string, float>();
        public Dictionary<string, float> StepDifficultyRatings = new Dictionary<string, float>();
        public System.DateTime LastUpdated;
        public float HintUsageRate = 0f; // Rate at which hints are used
        public int TotalErrors = 0; // Total number of errors encountered
        public float ErrorRate => TotalStepsCompleted > 0 ? (float)TotalErrors / TotalStepsCompleted : 0f;
    }

    /// <summary>
    /// Step performance data
    /// </summary>
    [Serializable]
    public class StepPerformanceData
    {
        public string StepId;
        public float AverageCompletionTime;
        public int TotalAttempts;
        public int SuccessfulCompletions;
        public int HintsUsed;
        public float DifficultyRating;
        public List<string> CommonErrors = new List<string>();
        public TutorialCategory Category = TutorialCategory.Onboarding; // Category for performance tracking
        public float SuccessRate { get; set; } = 0f; // Success rate for this step
        public float ExpectedTime = 30f; // Expected completion time for this step
        public int CompletionCount { get; set; } = 0; // Completion count for this step
        public float TotalCompletionTime = 0f; // Total time spent completing this step
        public System.DateTime LastCompletionTime; // Last time this step was completed
    }
}