using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectChimera.Data.Events;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;
// Explicit namespace aliases to resolve ambiguous references
using CompetitionPhase = ProjectChimera.Data.Events.CompetitionPhase;
using EventPriority = ProjectChimera.Core.Events.EventPriority;
using ILiveEventDefinition = ProjectChimera.Core.Events.ILiveEventDefinition;
using LiveEventMessageType = ProjectChimera.Core.Events.LiveEventMessageType;
using LiveEventMessage = ProjectChimera.Core.Events.LiveEventMessage;
using GlobalEventState = ProjectChimera.Core.Events.GlobalEventState;
using UrgencyLevel = ProjectChimera.Data.Events.UrgencyLevel;

namespace ProjectChimera.Systems.Events
{
    /// <summary>
    /// Core interfaces and base classes for Project Chimera's live event system.
    /// Provides the fundamental contracts for all live event implementations including
    /// global competitions, seasonal events, community challenges, and cultural celebrations.
    /// </summary>
    
    // Import key enums and types from Data.Events namespace for convenience
    using EventType = ProjectChimera.Data.Events.EventType;
    using EventSeason = ProjectChimera.Data.Events.Season;
    using CompetitionType = ProjectChimera.Data.Events.CompetitionType;
    using CrisisType = ProjectChimera.Data.Events.CrisisType;
    using RarityTier = ProjectChimera.Data.Events.RarityTier;
    
    #region Core Event Interfaces
    
    /// <summary>
    /// Primary interface for all live event instances in the system.
    /// </summary>
    public interface ILiveEvent
    {
        string EventId { get; }
        string EventName { get; }
        string Description { get; }
        EventType Type { get; }
        EventScope Scope { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        TimeSpan Duration { get; }
        EventStatus Status { get; }
        
        bool CanParticipate(PlayerProfile player);
        void StartEvent(EventContext context);
        void UpdateEvent(float deltaTime);
        void EndEvent(EventResult result);
        void ProcessPlayerAction(PlayerAction action);
        void Cleanup();
        
        event Action<ILiveEvent> OnEventStarted;
        event Action<ILiveEvent> OnEventEnded;
        event Action<ILiveEvent, PlayerAction> OnPlayerAction;
    }
    
    /// <summary>
    /// Interface for events that support real-time participation tracking.
    /// </summary>
    public interface IParticipationTracking
    {
        int CurrentParticipants { get; }
        int MaxParticipants { get; }
        List<PlayerProfile> GetParticipants();
        bool AddParticipant(PlayerProfile player);
        bool RemoveParticipant(PlayerProfile player);
        ParticipationData GetParticipationData();
    }
    
    /// <summary>
    /// Interface for events with community goals and collaborative features.
    /// </summary>
    public interface ICommunityEvent
    {
        CommunityGoal CommunityGoal { get; }
        float CommunityProgress { get; }
        bool IsCommunityGoalReached { get; }
        Dictionary<string, float> GetIndividualContributions();
        void UpdateCommunityProgress(PlayerContribution contribution);
        CommunityEventStatistics GetCommunityStatistics();
    }
    
    /// <summary>
    /// Interface for events with educational content and learning objectives.
    /// </summary>
    public interface IEducationalEvent
    {
        List<LearningObjective> LearningObjectives { get; }
        EducationalContent EducationalMaterials { get; }
        bool RequiresScientificAccuracy { get; }
        bool IsScientificallyValidated { get; }
        EducationalProgress TrackLearningProgress(PlayerProfile player);
        void ValidateEducationalContent();
    }
    
    /// <summary>
    /// Interface for events with reward systems and recognition.
    /// </summary>
    public interface IRewardableEvent
    {
        RewardStructure RewardStructure { get; }
        List<EventReward> GetAvailableRewards(PlayerProfile player);
        void DistributeRewards();
        bool HasReceivedReward(PlayerProfile player, string rewardId);
        void GrantReward(PlayerProfile player, EventReward reward);
    }
    
    /// <summary>
    /// Interface for events that can be synchronized globally.
    /// </summary>
    public interface IGlobalEvent
    {
        string GlobalEventId { get; }
        bool RequiresGlobalSynchronization { get; }
        GlobalEventState GlobalState { get; }
        void SynchronizeWithGlobalState(GlobalEventUpdate update);
        GlobalEventUpdate CreateGlobalUpdate();
        void HandleGlobalStateConflict(GlobalEventState conflictingState);
    }
    
    /// <summary>
    /// Interface for seasonal and time-sensitive events.
    /// </summary>
    public interface ISeasonalEvent
    {
        EventSeason AssociatedSeason { get; }
        bool IsSeasonallyActive { get; }
        SeasonalModifiers GetSeasonalModifiers();
        void ApplySeasonalChanges(EventSeason newSeason);
        bool IsValidForSeason(EventSeason season);
    }
    
    /// <summary>
    /// Interface for cultural events with regional variations.
    /// </summary>
    public interface ICulturalEvent
    {
        CulturalContext CulturalContext { get; }
        List<string> CulturalTags { get; }
        bool RequiresCulturalAuthenticity { get; }
        bool IsCulturallySensitive { get; }
        CulturalVariation GetRegionalVariation(string regionId);
        void ValidateCulturalSensitivity();
    }
    
    /// <summary>
    /// Interface for crisis response and emergency events.
    /// </summary>
    public interface ICrisisResponseEvent
    {
        CrisisType CrisisType { get; }
        UrgencyLevel UrgencyLevel { get; }
        CrisisContext CrisisContext { get; }
        EmergencyResponse EmergencyResponse { get; }
        void ActivateEmergencyResponse();
        void MobilizeCommunityResponse();
        bool IsEmergencyActive { get; }
    }
    
    #endregion
    
    #region Base Event Classes
    
    /// <summary>
    /// Abstract base class for all live event implementations.
    /// Provides common functionality and ensures consistent behavior across event types.
    /// </summary>
    public abstract class LiveEventBase : ILiveEvent, IParticipationTracking
    {
        // Core event properties
        public string EventId { get; protected set; }
        public string EventName { get; protected set; }
        public string Description { get; protected set; }
        public EventType Type { get; protected set; }
        public EventScope Scope { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        public TimeSpan Duration => EndTime - StartTime;
        public EventStatus Status { get; protected set; } = EventStatus.NotStarted;
        
        // Participation tracking
        public int CurrentParticipants => _participants.Count;
        public int MaxParticipants { get; protected set; }
        
        // Event state
        protected List<PlayerProfile> _participants = new List<PlayerProfile>();
        protected Dictionary<string, PlayerContribution> _playerContributions = new Dictionary<string, PlayerContribution>();
        protected EventContext _eventContext;
        protected DateTime _lastUpdate;
        protected bool _isCleanedUp = false;
        
        // Events
        public event Action<ILiveEvent> OnEventStarted;
        public event Action<ILiveEvent> OnEventEnded;
        public event Action<ILiveEvent, PlayerAction> OnPlayerAction;
        
        protected LiveEventBase(ILiveEventDefinition definition)
        {
            EventId = definition.EventId;
            EventName = definition.EventName;
            Description = definition.Description;
            Type = (EventType)definition.EventType;
            Scope = definition.Scope;
            StartTime = definition.StartTime;
            EndTime = definition.EndTime;
            MaxParticipants = 1000; // Default, can be overridden
        }
        
        public virtual bool CanParticipate(PlayerProfile player)
        {
            if (player == null) return false;
            if (Status != EventStatus.Active) return false;
            if (_participants.Count >= MaxParticipants) return false;
            if (_participants.Contains(player)) return false;
            
            return ValidateParticipationRequirements(player);
        }
        
        public virtual void StartEvent(EventContext context)
        {
            if (Status != EventStatus.NotStarted)
            {
                Debug.LogWarning($"[LiveEventBase] Event {EventId} cannot be started - current status: {Status}");
                return;
            }
            
            _eventContext = context;
            Status = EventStatus.Active;
            _lastUpdate = DateTime.Now;
            
            OnEventStarted?.Invoke(this);
            OnEventStartedInternal();
            
            Debug.Log($"[LiveEventBase] Started event: {EventId}");
        }
        
        public virtual void UpdateEvent(float deltaTime)
        {
            if (Status != EventStatus.Active) return;
            
            _lastUpdate = DateTime.Now;
            
            // Check for automatic end conditions
            if (DateTime.Now >= EndTime)
            {
                Status = EventStatus.Completed;
                return;
            }
            
            OnUpdateInternal(deltaTime);
        }
        
        public virtual void EndEvent(EventResult result)
        {
            if (Status == EventStatus.Ended || Status == EventStatus.NotStarted)
            {
                Debug.LogWarning($"[LiveEventBase] Event {EventId} cannot be ended - current status: {Status}");
                return;
            }
            
            Status = EventStatus.Ended;
            
            OnEventEndedInternal(result);
            OnEventEnded?.Invoke(this);
            
            Debug.Log($"[LiveEventBase] Ended event: {EventId}");
        }
        
        public virtual void ProcessPlayerAction(PlayerAction action)
        {
            if (Status != EventStatus.Active) return;
            if (action?.Player == null) return;
            
            // Ensure player is participating
            if (!_participants.Contains(action.Player))
            {
                if (!AddParticipant(action.Player))
                {
                    Debug.LogWarning($"[LiveEventBase] Player {action.Player} cannot join event {EventId}");
                    return;
                }
            }
            
            OnPlayerActionInternal(action);
            OnPlayerAction?.Invoke(this, action);
        }
        
        public virtual void Cleanup()
        {
            if (_isCleanedUp) return;
            
            _participants.Clear();
            _playerContributions.Clear();
            
            OnCleanupInternal();
            _isCleanedUp = true;
            
            Debug.Log($"[LiveEventBase] Cleaned up event: {EventId}");
        }
        
        #region IParticipationTracking Implementation
        
        public List<PlayerProfile> GetParticipants()
        {
            return new List<PlayerProfile>(_participants);
        }
        
        public virtual bool AddParticipant(PlayerProfile player)
        {
            if (!CanParticipate(player)) return false;
            
            _participants.Add(player);
            _playerContributions[player.PlayerId] = new PlayerContribution
            {
                PlayerId = player.PlayerId,
                JoinTime = DateTime.Now,
                TotalContribution = 0f
            };
            
            OnParticipantAdded(player);
            return true;
        }
        
        public virtual bool RemoveParticipant(PlayerProfile player)
        {
            if (!_participants.Contains(player)) return false;
            
            _participants.Remove(player);
            _playerContributions.Remove(player.PlayerId);
            
            OnParticipantRemoved(player);
            return true;
        }
        
        public ParticipationData GetParticipationData()
        {
            return new ParticipationData
            {
                EventId = EventId,
                CurrentParticipants = CurrentParticipants,
                MaxParticipants = MaxParticipants,
                ParticipationRate = (float)CurrentParticipants / MaxParticipants,
                TotalContributions = _playerContributions.Values.Sum(c => c.TotalContribution),
                AverageContribution = _playerContributions.Count > 0 ? 
                    _playerContributions.Values.Average(c => c.TotalContribution) : 0f
            };
        }
        
        #endregion
        
        #region Protected Virtual Methods for Subclass Implementation
        
        protected virtual bool ValidateParticipationRequirements(PlayerProfile player)
        {
            return true;
        }
        
        protected virtual void OnEventStartedInternal() { }
        protected virtual void OnUpdateInternal(float deltaTime) { }
        protected virtual void OnEventEndedInternal(EventResult result) { }
        protected virtual void OnPlayerActionInternal(PlayerAction action) { }
        protected virtual void OnParticipantAdded(PlayerProfile player) { }
        protected virtual void OnParticipantRemoved(PlayerProfile player) { }
        protected virtual void OnCleanupInternal() { }
        
        #endregion
        
        #region Utility Methods
        
        protected PlayerContribution GetPlayerContribution(PlayerProfile player)
        {
            return _playerContributions.GetValueOrDefault(player.PlayerId);
        }
        
        protected void UpdatePlayerContribution(PlayerProfile player, float contributionAmount)
        {
            if (_playerContributions.TryGetValue(player.PlayerId, out var contribution))
            {
                contribution.TotalContribution += contributionAmount;
                contribution.LastContribution = DateTime.Now;
            }
        }
        
        protected bool IsEventExpired()
        {
            return DateTime.Now > EndTime;
        }
        
        protected TimeSpan GetRemainingTime()
        {
            var remaining = EndTime - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Base class for community events with collaborative goals.
    /// </summary>
    public abstract class CommunityEventBase : LiveEventBase, ICommunityEvent
    {
        public CommunityGoal CommunityGoal { get; protected set; }
        public float CommunityProgress { get; protected set; }
        public bool IsCommunityGoalReached => CommunityProgress >= CommunityGoal.TargetAmount;
        
        protected float _progressMultiplier = 1.0f;
        protected Dictionary<string, float> _individualContributions = new Dictionary<string, float>();
        
        protected CommunityEventBase(ILiveEventDefinition definition, CommunityGoal communityGoal) 
            : base(definition)
        {
            CommunityGoal = communityGoal;
        }
        
        public Dictionary<string, float> GetIndividualContributions()
        {
            return new Dictionary<string, float>(_individualContributions);
        }
        
        public virtual void UpdateCommunityProgress(PlayerContribution contribution)
        {
            var contributionAmount = contribution.TotalContribution * _progressMultiplier;
            
            // Update individual contribution tracking
            if (!_individualContributions.ContainsKey(contribution.PlayerId))
            {
                _individualContributions[contribution.PlayerId] = 0f;
            }
            _individualContributions[contribution.PlayerId] += contributionAmount;
            
            // Update community progress
            CommunityProgress += contributionAmount;
            
            // Check for goal completion
            if (IsCommunityGoalReached && Status == EventStatus.Active)
            {
                OnCommunityGoalReached();
            }
        }
        
        public CommunityEventStatistics GetCommunityStatistics()
        {
            return new CommunityEventStatistics
            {
                EventId = EventId,
                GoalType = CommunityGoal.GoalType,
                TargetAmount = CommunityGoal.TargetAmount,
                CurrentProgress = CommunityProgress,
                ProgressPercentage = (CommunityProgress / CommunityGoal.TargetAmount) * 100f,
                ParticipantCount = CurrentParticipants,
                TopContributors = GetTopContributors(5),
                AverageContribution = _individualContributions.Count > 0 ? 
                    _individualContributions.Values.Average() : 0f
            };
        }
        
        protected virtual void OnCommunityGoalReached()
        {
            Debug.Log($"[CommunityEventBase] Community goal reached for event: {EventId}");
        }
        
        private List<ContributorInfo> GetTopContributors(int count)
        {
            return _individualContributions
                .OrderByDescending(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => new ContributorInfo
                {
                    PlayerId = kvp.Key,
                    Contribution = kvp.Value
                })
                .ToList();
        }
    }
    
    /// <summary>
    /// Base class for educational events with learning objectives and content validation.
    /// </summary>
    public abstract class EducationalEventBase : LiveEventBase, IEducationalEvent
    {
        public List<LearningObjective> LearningObjectives { get; protected set; }
        public EducationalContent EducationalMaterials { get; protected set; }
        public bool RequiresScientificAccuracy { get; protected set; }
        public bool IsScientificallyValidated { get; protected set; }
        
        protected Dictionary<string, EducationalProgress> _learningProgress = new Dictionary<string, EducationalProgress>();
        protected EducationalContentValidator _contentValidator;
        
        protected EducationalEventBase(ILiveEventDefinition definition, EducationalContent materials) 
            : base(definition)
        {
            EducationalMaterials = materials;
            LearningObjectives = new List<LearningObjective>();
            RequiresScientificAccuracy = definition.RequiresScientificAccuracy;
            IsScientificallyValidated = definition.IsScientificallyValidated;
        }
        
        public EducationalProgress TrackLearningProgress(PlayerProfile player)
        {
            if (!_learningProgress.ContainsKey(player.PlayerId))
            {
                _learningProgress[player.PlayerId] = new EducationalProgress
                {
                    PlayerId = player.PlayerId,
                    EventId = EventId,
                    StartTime = DateTime.Now
                };
            }
            
            return _learningProgress[player.PlayerId];
        }
        
        public virtual void ValidateEducationalContent()
        {
            if (_contentValidator == null)
            {
                _contentValidator = new EducationalContentValidator();
            }
            
            var validationResult = _contentValidator.ValidateContent(EducationalMaterials);
            IsScientificallyValidated = validationResult.IsValid;
            
            if (!IsScientificallyValidated && RequiresScientificAccuracy)
            {
                Debug.LogError($"[EducationalEventBase] Educational content validation failed for event: {EventId}");
            }
        }
        
        protected void UpdateLearningProgress(PlayerProfile player, string objectiveId, float progress)
        {
            var learningProgress = TrackLearningProgress(player);
            learningProgress.ObjectiveProgress[objectiveId] = progress;
            learningProgress.LastUpdate = DateTime.Now;
        }
        
        protected float GetObjectiveCompletion(PlayerProfile player, string objectiveId)
        {
            var learningProgress = TrackLearningProgress(player);
            return learningProgress.ObjectiveProgress.GetValueOrDefault(objectiveId, 0f);
        }
    }
    
    #endregion
    
    #region Supporting Data Structures
    
    public enum EventStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Ended,
        Cancelled
    }
    
    
    public enum CulturalPeriodType
    {
        General,
        Traditional,
        Modern,
        Contemporary,
        Seasonal,
        Festival,
        Holiday,
        Historical,
        Regional
    }
    
    [Serializable]
    public class CulturalPeriod
    {
        public string PeriodName;
        public CulturalPeriodType PeriodType;
        public DateTime StartDate;
        public DateTime EndDate;
        public string Description;
        public string CulturalContext;
        
        public CulturalPeriod()
        {
            PeriodName = "Default Period";
            PeriodType = CulturalPeriodType.General;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(30); // Default 30-day period
        }
        
        public CulturalPeriod(string periodName, CulturalPeriodType periodType)
        {
            PeriodName = periodName;
            PeriodType = periodType;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(30); // Default 30-day period
        }
    }
    
    public enum EventResult
    {
        Success,
        Failed,
        Cancelled,
        Expired,
        Completed,
        Abandoned,
        Error
    }
    
    [Serializable]
    public class EventResultData
    {
        public string EventId;
        public EventResult Result;
        public DateTime EndTime;
        public bool WasSuccessful;
        public string ResultMessage;
        public Dictionary<string, object> ResultData = new Dictionary<string, object>();
        
        public EventResultData(string eventId, EventResult result)
        {
            EventId = eventId;
            Result = result;
            EndTime = DateTime.Now;
            WasSuccessful = result == EventResult.Success || result == EventResult.Completed;
            ResultMessage = result.ToString();
        }
    }
    
    public enum EventPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }
    
    public enum ActionType
    {
        Learn,
        Submit,
        Share,
        Participate,
        Contribute,
        Collaborate,
        Validate,
        Research,
        Discover,
        Create,
        Complete,
        Progress,
        Help,
        Compete,
        Educate,
        Support,
        Teach,
        Challenge,
        Celebrate
    }
    
    // Season enum is defined in ProjectChimera.Data.Events namespace - use EventSeason alias
    
    public enum CompetitionPhase
    {
        Registration,
        Submission,
        Judging,
        Results,
        Completed
    }
    
    public enum RewardDistributionType
    {
        Participation,
        Achievement,
        Leaderboard,
        Milestone,
        Completion,
        Contribution,
        Equal,
        Weighted,
        Ranking,
        Community,
        Random
    }
    
    public enum CollaborationMode
    {
        Individual,
        Team,
        Teams,
        Community,
        Global,
        Cooperative,
        Competitive,
        Mixed,
        FreeForm,
        Mentorship,
        PeerToPeer
    }

    // ActionType enum is already defined above - removed duplicate
    
    [Serializable]
    public class PlayerAction
    {
        public string PlayerId;
        public string ActionType;
        public ActionType Type; // ActionType enum property
        public PlayerProfile Player; // Player profile property
        public DateTime Timestamp;
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
        public Dictionary<string, object> Parameters => ActionData; // Alias for ActionData
        
        public PlayerAction(string playerId, string actionType)
        {
            PlayerId = playerId;
            ActionType = actionType;
            Timestamp = DateTime.Now;
        }
        
        public PlayerAction(PlayerProfile player, ActionType actionType)
        {
            Player = player;
            PlayerId = player?.PlayerId;
            Type = actionType;
            ActionType = actionType.ToString();
            Timestamp = DateTime.Now;
        }
    }
    
    [Serializable]
    public class EventContext
    {
        public string EventId;
        public DateTime EventStartTime;
        public DateTime Timestamp;
        public EventSeason CurrentSeason;
        public CulturalPeriod CurrentCulturalPeriod;
        public Dictionary<string, object> ContextData = new Dictionary<string, object>();
        public List<string> ActivePlayerIds = new List<string>();
        
        public EventContext(string eventId)
        {
            EventId = eventId;
            EventStartTime = DateTime.Now;
            Timestamp = DateTime.Now;
            CurrentSeason = EventSeason.Spring; // Default
            CurrentCulturalPeriod = new CulturalPeriod("Modern", CulturalPeriodType.Modern); // Default
        }
    }
    

    
    [Serializable]
    public class PlayerContribution
    {
        public string PlayerId;
        public DateTime JoinTime;
        public DateTime LastContribution;
        public float TotalContribution;
        public Dictionary<string, float> ContributionBreakdown = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class ParticipationData
    {
        public string EventId;
        public int CurrentParticipants;
        public int MaxParticipants;
        public float ParticipationRate;
        public float TotalContributions;
        public float AverageContribution;
    }
    
    [Serializable]
    public class CommunityGoal
    {
        public string GoalId;
        public string GoalType;
        public float TargetAmount;
        public string Description;
        public TimeSpan TimeLimit;
        public bool IsCompleted;
    }
    
    [Serializable]
    public class CommunityEventStatistics
    {
        public string EventId;
        public string GoalType;
        public float TargetAmount;
        public float CurrentProgress;
        public float ProgressPercentage;
        public int ParticipantCount;
        public List<ContributorInfo> TopContributors;
        public float AverageContribution;
    }
    
    [Serializable]
    public class ContributorInfo
    {
        public string PlayerId;
        public float Contribution;
    }
    
    [Serializable]
    public class LearningObjective
    {
        public string ObjectiveId;
        public string Description;
        public float CompletionThreshold;
        public bool IsRequired;
    }
    
    [Serializable]
    public class EducationalProgress
    {
        public string PlayerId;
        public string EventId;
        public DateTime StartTime;
        public DateTime LastUpdate;
        public Dictionary<string, float> ObjectiveProgress = new Dictionary<string, float>();
        public float OverallProgress;
    }
    
    // GlobalEventState now imported from ProjectChimera.Core.Events
    
    [Serializable]
    public class GlobalEventUpdate
    {
        public string EventId;
        public string UpdateType;
        public Dictionary<string, object> UpdateData = new Dictionary<string, object>();
        public DateTime Timestamp;
        public EventPriority Priority = EventPriority.Medium;
    }
    
    [Serializable]
    public class CulturalContext
    {
        public string CultureName;
        public string Region;
        public string Description;
        public List<string> CulturalTags = new List<string>();
        public bool RequiresAuthenticity;
    }
    
    [Serializable]
    public class CulturalVariation
    {
        public string RegionId;
        public string VariationName;
        public Dictionary<string, object> RegionalModifications = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CrisisContext
    {
        public CrisisType CrisisType;
        public UrgencyLevel UrgencyLevel;
        public string Description;
        public DateTime CrisisStartTime;
        public TimeSpan ExpectedDuration;
    }
    
    [Serializable]
    public class EmergencyResponse
    {
        public string ResponseId;
        public List<string> ResponseActions = new List<string>();
        public Dictionary<string, object> ResponseResources = new Dictionary<string, object>();
        public bool IsActive;
    }
    
    [Serializable]
    public class SeasonalModifiers
    {
        public EventSeason Season;
        public Dictionary<string, float> EnvironmentalModifiers = new Dictionary<string, float>();
        public Dictionary<string, float> GameplayModifiers = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class RewardStructure
    {
        public string RewardStructureId;
        public List<EventReward> Rewards = new List<EventReward>();
        public RewardDistributionType DistributionType;
    }
    
    [Serializable]
    public class EventReward
    {
        public string RewardId;
        public string RewardType;
        public object RewardData;
        public bool IsExclusive;
        public DateTime ExpirationDate;
        public float RewardValue;
        public string Description;
    }
    
    // RewardDistributionType enum is already defined above - removed duplicate
    
    // Placeholder classes for compilation
    public class EducationalContentValidator
    {
        public ValidationResult ValidateContent(EducationalContent content)
        {
            return new ValidationResult { IsValid = true };
        }
    }
    
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
    }
    
    #endregion
    
    /// <summary>
    /// Core interfaces and data structures for Project Chimera's live event system.
    /// Provides type-safe event messaging and subscription management.
    /// </summary>
    
    // Core event types now imported from ProjectChimera.Core.Events
    // LiveEventMessageType, EventPriority, EventScope, CompetitionPhase, and LiveEventMessage
    // are defined in Core.Events to avoid circular dependencies
    
    // Specialized Event Messages
    [Serializable]
    public class CompetitionEventMessage : LiveEventMessage
    {
        public string CompetitionId;
        public CompetitionPhase Phase;
        public CompetitionPhase PreviousPhase;
        public Dictionary<string, float> Scores = new Dictionary<string, float>();
        public int ParticipantCount;
        
        public CompetitionEventMessage(string competitionId, CompetitionPhase phase) 
            : base(LiveEventMessageType.CompetitionUpdate, $"Competition {phase}")
        {
            CompetitionId = competitionId;
            Phase = phase;
            SetData("CompetitionId", competitionId);
            SetData("Phase", phase);
        }
    }
    
    [Serializable]
    public class CommunityGoalEventMessage : LiveEventMessage
    {
        public string GoalId;
        public float Progress;
        public float TargetAmount;
        public int ParticipantCount;
        public bool IsCompleted;
        
        public CommunityGoalEventMessage(string goalId, float progress, float targetAmount) 
            : base(LiveEventMessageType.CommunityGoalProgress, "Community Goal Progress")
        {
            GoalId = goalId;
            Progress = progress;
            TargetAmount = targetAmount;
            IsCompleted = progress >= targetAmount;
            SetData("GoalId", goalId);
            SetData("Progress", progress);
            SetData("TargetAmount", targetAmount);
        }
    }
    
    [Serializable]
    public class SeasonalEventMessage : LiveEventMessage
    {
        public string Season;
        public string PreviousSeason;
        public Dictionary<string, float> SeasonalModifiers = new Dictionary<string, float>();
        
        public SeasonalEventMessage(string season, string previousSeason) 
            : base(LiveEventMessageType.SeasonalTransition, "Seasonal Transition")
        {
            Season = season;
            PreviousSeason = previousSeason;
            SetData("Season", season);
            SetData("PreviousSeason", previousSeason);
        }
    }
    
    [Serializable]
    public class EducationalEventMessage : LiveEventMessage
    {
        public string PlayerId;
        public string ObjectiveId;
        public float CompletionScore;
        public List<string> LearningCategories = new List<string>();
        
        public EducationalEventMessage(string playerId, string objectiveId, float completionScore) 
            : base(LiveEventMessageType.EducationalContent, "Learning Objective Completed")
        {
            PlayerId = playerId;
            ObjectiveId = objectiveId;
            CompletionScore = completionScore;
            SetData("PlayerId", playerId);
            SetData("ObjectiveId", objectiveId);
            SetData("CompletionScore", completionScore);
        }
    }
    
    [Serializable]
    public class GlobalSyncEventMessage : LiveEventMessage
    {
        public string EventId;
        public Dictionary<string, object> SyncData = new Dictionary<string, object>();
        
        public GlobalSyncEventMessage(string eventId, Dictionary<string, object> syncData) 
            : base(LiveEventMessageType.SystemNotification, "Global Synchronization")
        {
            EventId = eventId;
            SyncData = syncData ?? new Dictionary<string, object>();
            SetData("EventId", eventId);
            SetData("SyncData", SyncData);
        }
    }
    
    // Additional data structures for complete event system integration
    

    
    [Serializable]
    public class EducationalContent
    {
        public string ContentId;
        public string Title;
        public string Description;
        public List<LearningObjective> LearningObjectives = new List<LearningObjective>();
        public List<string> ResourceLinks = new List<string>();
        public float QualityScore = 1.0f;
        public bool IsScientificallyValidated = false;
    }
    
    // Note: PlayerProfile is now defined in SharedDataStructures.cs
}