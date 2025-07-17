using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Global competition event ScriptableObject defining large-scale competitive events
    /// that bring together the cannabis cultivation community for friendly competition.
    /// </summary>
    [CreateAssetMenu(fileName = "New Global Competition Event", menuName = "Project Chimera/Events/Global Competition Event", order = 110)]
    public class GlobalCompetitionEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [Header("Basic Event Information")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private CompetitionType _competitionType;
        [SerializeField] private EventScope _scope = EventScope.Global;
        
        [Header("Event Timing")]
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private TimeSpan _registrationPeriod;
        [SerializeField] private TimeSpan _submissionPeriod;
        [SerializeField] private TimeSpan _judgingPeriod;
        
        [Header("Competition Structure")]
        [SerializeField] private List<CompetitionCategorySO> _categories = new List<CompetitionCategorySO>();
        [SerializeField] private JudgingCriteriaSO _judgingCriteria;
        [SerializeField] private LeaderboardConfigSO _leaderboardConfig;
        [SerializeField] private bool _allowTeamParticipation = false;
        [SerializeField] private int _maxTeamSize = 1;
        
        [Header("Participation Requirements")]
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        [SerializeField] private int _minimumPlayerLevel = 1;
        [SerializeField] private int _minimumCultivationExperience = 0;
        [SerializeField] private bool _requiresVerifiedAccount = false;
        [SerializeField] private List<string> _requiredAchievements = new List<string>();
        
        [Header("Rewards and Recognition")]
        [SerializeField] private TieredRewardStructureSO _rewardStructure;
        [SerializeField] private List<ParticipationRewardSO> _participationRewards = new List<ParticipationRewardSO>();
        [SerializeField] private List<SpecialRecognitionSO> _specialRecognitions = new List<SpecialRecognitionSO>();
        [SerializeField] private bool _enableExclusiveTitles = true;
        [SerializeField] private bool _enableLimitedEditionRewards = true;
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalContent = true;
        [SerializeField] private bool _requiresScientificAccuracy = true;
        [SerializeField] private bool _isScientificallyValidated = false;
        [SerializeField] private EducationalContentSO _educationalMaterials;
        [SerializeField] private List<string> _learningObjectives = new List<string>();
        
        [Header("Competition Mechanics")]
        [SerializeField] private CompetitionFormat _format = CompetitionFormat.OpenSubmission;
        [SerializeField] private bool _enableRealTimeTracking = true;
        [SerializeField] private bool _enablePublicVoting = false;
        [SerializeField] private float _publicVotingWeight = 0.2f;
        [SerializeField] private bool _enablePeerReview = true;
        
        [Header("Community Features")]
        [SerializeField] private bool _enableSocialSharing = true;
        [SerializeField] private bool _enableShowcase = true;
        [SerializeField] private bool _enableMentorship = true;
        [SerializeField] private bool _enableCollaboration = false;
        [SerializeField] private CommunityFeaturesSO _communityFeatures;
        
        [Header("Technical Specifications")]
        [SerializeField] private int _maxParticipants = 100000;
        [SerializeField] private int _maxSubmissionsPerParticipant = 1;
        [SerializeField] private bool _enableAntiCheat = true;
        [SerializeField] private bool _enableSubmissionValidation = true;
        [SerializeField] private SubmissionRequirementsSO _submissionRequirements;
        
        // ILiveEventDefinition Implementation
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => ProjectChimera.Data.Events.EventType.GlobalCompetition;
        public EventScope Scope => _scope;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Duration => _endTime - _startTime;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => _hasEducationalContent;
        public bool RequiresScientificAccuracy => _requiresScientificAccuracy;
        public bool IsScientificallyValidated => _isScientificallyValidated;
        
        // Competition-specific Properties
        public CompetitionType CompetitionType => _competitionType;
        public List<CompetitionCategorySO> Categories => _categories;
        public JudgingCriteriaSO JudgingCriteria => _judgingCriteria;
        public LeaderboardConfigSO LeaderboardConfig => _leaderboardConfig;
        public bool AllowTeamParticipation => _allowTeamParticipation;
        public int MaxTeamSize => _maxTeamSize;
        
        public TimeSpan RegistrationPeriod => _registrationPeriod;
        public TimeSpan SubmissionPeriod => _submissionPeriod;
        public TimeSpan JudgingPeriod => _judgingPeriod;
        
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        public int MinimumCultivationExperience => _minimumCultivationExperience;
        public bool RequiresVerifiedAccount => _requiresVerifiedAccount;
        public List<string> RequiredAchievements => _requiredAchievements;
        
        public TieredRewardStructureSO RewardStructure => _rewardStructure;
        public List<ParticipationRewardSO> ParticipationRewards => _participationRewards;
        public List<SpecialRecognitionSO> SpecialRecognitions => _specialRecognitions;
        public bool EnableExclusiveTitles => _enableExclusiveTitles;
        public bool EnableLimitedEditionRewards => _enableLimitedEditionRewards;
        
        public EducationalContentSO EducationalMaterials => _educationalMaterials;
        public List<string> LearningObjectives => _learningObjectives;
        
        public CompetitionFormat Format => _format;
        public bool EnableRealTimeTracking => _enableRealTimeTracking;
        public bool EnablePublicVoting => _enablePublicVoting;
        public float PublicVotingWeight => _publicVotingWeight;
        public bool EnablePeerReview => _enablePeerReview;
        
        public bool EnableSocialSharing => _enableSocialSharing;
        public bool EnableShowcase => _enableShowcase;
        public bool EnableMentorship => _enableMentorship;
        public bool EnableCollaboration => _enableCollaboration;
        public CommunityFeaturesSO CommunityFeatures => _communityFeatures;
        
        public int MaxParticipants => _maxParticipants;
        public int MaxSubmissionsPerParticipant => _maxSubmissionsPerParticipant;
        public bool EnableAntiCheat => _enableAntiCheat;
        public bool EnableSubmissionValidation => _enableSubmissionValidation;
        public SubmissionRequirementsSO SubmissionRequirements => _submissionRequirements;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Ensure event ID is set
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = $"competition_{_competitionType}_{Guid.NewGuid():N}";
            }
            
            // Validate timing
            if (_endTime <= _startTime)
            {
                _endTime = _startTime.AddDays(7); // Default 1 week duration
            }
            
            // Validate participation limits
            _maxParticipants = Mathf.Max(1, _maxParticipants);
            _maxSubmissionsPerParticipant = Mathf.Max(1, _maxSubmissionsPerParticipant);
            _minimumPlayerLevel = Mathf.Max(1, _minimumPlayerLevel);
            _minimumCultivationExperience = Mathf.Max(0, _minimumCultivationExperience);
            
            // Validate team settings
            if (_allowTeamParticipation)
            {
                _maxTeamSize = Mathf.Max(2, _maxTeamSize);
            }
            else
            {
                _maxTeamSize = 1;
            }
            
            // Validate voting settings
            _publicVotingWeight = Mathf.Clamp01(_publicVotingWeight);
            
            // Validate periods
            if (_registrationPeriod.TotalDays <= 0)
                _registrationPeriod = TimeSpan.FromDays(7); // Default 1 week registration
            
            if (_submissionPeriod.TotalDays <= 0)
                _submissionPeriod = TimeSpan.FromDays(14); // Default 2 weeks submission
            
            if (_judgingPeriod.TotalDays <= 0)
                _judgingPeriod = TimeSpan.FromDays(7); // Default 1 week judging
        }
        
        public bool CanPlayerParticipate(PlayerProfile playerProfile)
        {
            // Check minimum level requirement
            if (playerProfile.Level < _minimumPlayerLevel)
                return false;
            
            // Check cultivation experience
            if (playerProfile.CultivationExperience < _minimumCultivationExperience)
                return false;
            
            // Check account verification
            if (_requiresVerifiedAccount && !playerProfile.IsVerified)
                return false;
            
            // Check required achievements
            foreach (var achievement in _requiredAchievements)
            {
                if (!playerProfile.HasAchievement(achievement))
                    return false;
            }
            
            // Check additional participation requirements
            foreach (var requirement in _participationRequirements)
            {
                if (!playerProfile.MeetsRequirement(requirement))
                    return false;
            }
            
            return true;
        }
        
        public DateTime GetRegistrationDeadline()
        {
            return _startTime.Subtract(_registrationPeriod);
        }
        
        public DateTime GetSubmissionDeadline()
        {
            return _startTime.Add(_submissionPeriod);
        }
        
        public DateTime GetJudgingCompletionDate()
        {
            return GetSubmissionDeadline().Add(_judgingPeriod);
        }
        
        public CompetitionPhase GetCurrentPhase(DateTime currentTime)
        {
            var registrationDeadline = GetRegistrationDeadline();
            var submissionDeadline = GetSubmissionDeadline();
            var judgingCompletion = GetJudgingCompletionDate();
            
            if (currentTime < registrationDeadline)
                return CompetitionPhase.PreRegistration;
            
            if (currentTime < _startTime)
                return CompetitionPhase.Registration;
            
            if (currentTime < submissionDeadline)
                return CompetitionPhase.Submission;
            
            if (currentTime < judgingCompletion)
                return CompetitionPhase.Judging;
            
            if (currentTime <= _endTime)
                return CompetitionPhase.Results;
            
            return CompetitionPhase.Completed;
        }
        
        public bool IsInPhase(CompetitionPhase phase, DateTime currentTime)
        {
            return GetCurrentPhase(currentTime) == phase;
        }
        
        public TimeSpan GetTimeRemainingInPhase(DateTime currentTime)
        {
            var currentPhase = GetCurrentPhase(currentTime);
            
            return currentPhase switch
            {
                CompetitionPhase.PreRegistration => GetRegistrationDeadline() - currentTime,
                CompetitionPhase.Registration => _startTime - currentTime,
                CompetitionPhase.Submission => GetSubmissionDeadline() - currentTime,
                CompetitionPhase.Judging => GetJudgingCompletionDate() - currentTime,
                CompetitionPhase.Results => _endTime - currentTime,
                CompetitionPhase.Completed => TimeSpan.Zero,
                _ => TimeSpan.Zero
            };
        }
        
        public CompetitionEventStatistics GetEventStatistics()
        {
            return new CompetitionEventStatistics
            {
                EventId = _eventId,
                CompetitionType = _competitionType,
                MaxParticipants = _maxParticipants,
                CategoryCount = _categories.Count,
                HasEducationalContent = _hasEducationalContent,
                AllowsTeams = _allowTeamParticipation,
                MaxTeamSize = _maxTeamSize,
                EnablesPublicVoting = _enablePublicVoting,
                CreatedAt = DateTime.Now
            };
        }
    }
    
    // Supporting enums and data structures
    public enum CompetitionFormat
    {
        OpenSubmission,
        InvitationOnly,
        Qualification,
        Tournament,
        League,
        Championship
    }
    
    public enum CompetitionPhase
    {
        PreRegistration,
        Registration,
        Submission,
        Judging,
        Results,
        Completed
    }
    
    [Serializable]
    public class CompetitionEventStatistics
    {
        public string EventId;
        public CompetitionType CompetitionType;
        public int MaxParticipants;
        public int CategoryCount;
        public bool HasEducationalContent;
        public bool AllowsTeams;
        public int MaxTeamSize;
        public bool EnablesPublicVoting;
        public DateTime CreatedAt;
    }
    
    // Note: PlayerProfile is now defined in SharedDataStructures.cs
}