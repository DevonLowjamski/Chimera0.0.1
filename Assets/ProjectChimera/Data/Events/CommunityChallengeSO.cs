using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Community challenge ScriptableObject for Project Chimera's collaborative event system.
    /// Defines community-driven challenges with shared goals, collaborative features,
    /// and integration with educational and narrative systems.
    /// </summary>
    [CreateAssetMenu(fileName = "New Community Challenge", menuName = "Project Chimera/Events/Community Challenge", order = 115)]
    public class CommunityChallengeSO : ChimeraDataSO, ILiveEventDefinition
    {
        [Header("Basic Challenge Information")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private ChallengeType _challengeType = ChallengeType.CollectiveGrowing;
        [SerializeField] private EventScope _scope = EventScope.Community;
        
        [Header("Challenge Timing")]
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private TimeSpan _duration;
        [SerializeField] private bool _allowLateJoining = true;
        [SerializeField] private TimeSpan _lateJoiningCutoff;
        
        [Header("Community Goal Configuration")]
        [SerializeField] private string _goalType = "collective_growth";
        [SerializeField] private string _goalDescription;
        [SerializeField] private float _targetAmount = 1000f;
        [SerializeField] private bool _enableSubGoals = true;
        [SerializeField] private List<SubGoalSO> _subGoals = new List<SubGoalSO>();
        
        [Header("Participation Settings")]
        [SerializeField] private int _maxParticipants = 1000;
        [SerializeField] private int _minimumParticipants = 5;
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        [SerializeField] private bool _requiresRegistration = false;
        [SerializeField] private float _minimumContributionForReward = 10f;
        
        [Header("Collaboration Features")]
        [SerializeField] private CollaborationMode _collaborationMode = CollaborationMode.FreeForm;
        [SerializeField] private bool _enableTeams = false;
        [SerializeField] private int _maxTeams = 10;
        [SerializeField] private int _maxTeamSize = 5;
        [SerializeField] private bool _enableSharedResources = true;
        [SerializeField] private List<string> _sharedResourceTypes = new List<string>();
        
        [Header("Social Features")]
        [SerializeField] private bool _enableCommunityChat = true;
        [SerializeField] private string _chatModerationLevel = "moderate";
        [SerializeField] private bool _enableKnowledgeSharing = true;
        [SerializeField] private bool _enableMentorship = true;
        [SerializeField] private bool _enablePeerSupport = true;
        
        [Header("Progress Tracking")]
        [SerializeField] private bool _enableLeaderboard = true;
        [SerializeField] private bool _enableMilestones = true;
        [SerializeField] private List<CommunityMilestoneSO> _milestones = new List<CommunityMilestoneSO>();
        [SerializeField] private bool _enableProgressVisualization = true;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        
        [Header("Rewards and Recognition")]
        [SerializeField] private RewardDistributionType _rewardDistributionType = RewardDistributionType.Community;
        [SerializeField] private List<EventReward> _communityRewards = new List<EventReward>();
        [SerializeField] private List<EventReward> _individualRewards = new List<EventReward>();
        [SerializeField] private bool _enableSpecialRecognition = true;
        [SerializeField] private bool _enableBadges = true;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _hasEducationalContent = true;
        [SerializeField] private bool _requiresScientificAccuracy = true;
        [SerializeField] private bool _isScientificallyValidated = false;
        [SerializeField] private List<string> _learningObjectives = new List<string>();
        [SerializeField] private List<string> _knowledgeCategories = new List<string>();
        [SerializeField] private bool _enableLearningTracking = true;
        
        [Header("Narrative Integration")]
        [SerializeField] private bool _enableNarrativeIntegration = false;
        [SerializeField] private List<StoryEventSO> _storyTriggers = new List<StoryEventSO>();
        [SerializeField] private bool _enableCommunityChoices = false;
        [SerializeField] private List<CommunityChoiceSO> _communityChoices = new List<CommunityChoiceSO>();
        [SerializeField] private bool _affectsMainStory = false;
        
        [Header("Challenge Mechanics")]
        [SerializeField] private bool _enableDynamicDifficulty = true;
        [SerializeField] private float _difficultyScalingFactor = 1.2f;
        [SerializeField] private bool _enableTimeBonus = true;
        [SerializeField] private bool _enableCollaborationBonus = true;
        [SerializeField] private float _collaborationBonusMultiplier = 1.5f;
        
        [Header("Analytics and Metrics")]
        [SerializeField] private bool _enableAdvancedAnalytics = true;
        [SerializeField] private bool _trackIndividualContributions = true;
        [SerializeField] private bool _trackCollaborationPatterns = true;
        [SerializeField] private bool _enableHeatmaps = false;
        
        // ILiveEventDefinition Implementation
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => ProjectChimera.Data.Events.EventType.CommunityChallenge;
        public EventScope Scope => _scope;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Duration => _duration;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => _hasEducationalContent;
        public bool RequiresScientificAccuracy => _requiresScientificAccuracy;
        public bool IsScientificallyValidated => _isScientificallyValidated;
        
        // Community Challenge Properties
        public ChallengeType ChallengeType => _challengeType;
        public CollaborationMode CollaborationMode => _collaborationMode;
        public string GoalType => _goalType;
        public string GoalDescription => _goalDescription;
        public float TargetAmount => _targetAmount;
        public bool EnableSubGoals => _enableSubGoals;
        public List<SubGoalSO> SubGoals => _subGoals;
        
        public int MaxParticipants => _maxParticipants;
        public int MinimumParticipants => _minimumParticipants;
        public bool RequiresRegistration => _requiresRegistration;
        public float MinimumContributionForReward => _minimumContributionForReward;
        public bool AllowLateJoining => _allowLateJoining;
        public TimeSpan LateJoiningCutoff => _lateJoiningCutoff;
        
        public bool EnableTeams => _enableTeams;
        public int MaxTeams => _maxTeams;
        public int MaxTeamSize => _maxTeamSize;
        public bool EnableSharedResources => _enableSharedResources;
        public List<string> SharedResourceTypes => _sharedResourceTypes;
        
        public bool EnableCommunityChat => _enableCommunityChat;
        public string ChatModerationLevel => _chatModerationLevel;
        public bool EnableKnowledgeSharing => _enableKnowledgeSharing;
        public bool EnableMentorship => _enableMentorship;
        public bool EnablePeerSupport => _enablePeerSupport;
        
        public bool EnableLeaderboard => _enableLeaderboard;
        public bool EnableMilestones => _enableMilestones;
        public List<CommunityMilestoneSO> Milestones => _milestones;
        public bool EnableProgressVisualization => _enableProgressVisualization;
        public bool EnableRealTimeUpdates => _enableRealTimeUpdates;
        
        public RewardDistributionType RewardDistributionType => _rewardDistributionType;
        public List<EventReward> CommunityRewards => _communityRewards;
        public List<EventReward> IndividualRewards => _individualRewards;
        public bool EnableSpecialRecognition => _enableSpecialRecognition;
        public bool EnableBadges => _enableBadges;
        
        public List<string> LearningObjectives => _learningObjectives;
        public List<string> KnowledgeCategories => _knowledgeCategories;
        public bool EnableLearningTracking => _enableLearningTracking;
        
        public bool EnableNarrativeIntegration => _enableNarrativeIntegration;
        public List<StoryEventSO> StoryTriggers => _storyTriggers;
        public bool EnableCommunityChoices => _enableCommunityChoices;
        public List<CommunityChoiceSO> CommunityChoices => _communityChoices;
        public bool AffectsMainStory => _affectsMainStory;
        
        public bool EnableDynamicDifficulty => _enableDynamicDifficulty;
        public float DifficultyScalingFactor => _difficultyScalingFactor;
        public bool EnableTimeBonus => _enableTimeBonus;
        public bool EnableCollaborationBonus => _enableCollaborationBonus;
        public float CollaborationBonusMultiplier => _collaborationBonusMultiplier;
        
        public bool EnableAdvancedAnalytics => _enableAdvancedAnalytics;
        public bool TrackIndividualContributions => _trackIndividualContributions;
        public bool TrackCollaborationPatterns => _trackCollaborationPatterns;
        public bool EnableHeatmaps => _enableHeatmaps;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Ensure event ID is set
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = $"community_challenge_{_challengeType}_{Guid.NewGuid():N}";
            }
            
            // Validate timing
            if (_endTime <= _startTime)
            {
                _endTime = _startTime.AddDays(7); // Default 1 week duration
            }
            
            _duration = _endTime - _startTime;
            
            // Validate participation settings
            _maxParticipants = Mathf.Max(1, _maxParticipants);
            _minimumParticipants = Mathf.Max(1, _minimumParticipants);
            _minimumContributionForReward = Mathf.Max(0f, _minimumContributionForReward);
            
            // Validate team settings
            if (_enableTeams)
            {
                _maxTeams = Mathf.Max(1, _maxTeams);
                _maxTeamSize = Mathf.Max(2, _maxTeamSize);
            }
            
            // Validate goal settings
            _targetAmount = Mathf.Max(1f, _targetAmount);
            
            // Validate collaboration settings
            _collaborationBonusMultiplier = Mathf.Max(1f, _collaborationBonusMultiplier);
            _difficultyScalingFactor = Mathf.Max(1f, _difficultyScalingFactor);
            
            // Validate late joining settings
            if (_allowLateJoining && _lateJoiningCutoff.TotalDays <= 0)
            {
                _lateJoiningCutoff = TimeSpan.FromDays(_duration.TotalDays * 0.75); // Allow joining until 75% through
            }
            
            // Initialize default values if needed
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            // Initialize default shared resource types if empty
            if (_enableSharedResources && _sharedResourceTypes.Count == 0)
            {
                _sharedResourceTypes = new List<string>
                {
                    "seeds", "nutrients", "equipment", "knowledge", "experience", "labor"
                };
            }
            
            // Initialize default learning objectives if educational content is enabled
            if (_hasEducationalContent && _learningObjectives.Count == 0)
            {
                _learningObjectives = new List<string>
                {
                    GetDefaultLearningObjective(_challengeType, 0),
                    GetDefaultLearningObjective(_challengeType, 1),
                    GetDefaultLearningObjective(_challengeType, 2)
                };
            }
            
            // Initialize default knowledge categories
            if (_enableKnowledgeSharing && _knowledgeCategories.Count == 0)
            {
                _knowledgeCategories = new List<string>
                {
                    "cultivation_techniques", "genetics", "environmental_control", 
                    "pest_management", "harvesting", "processing"
                };
            }
        }
        
        private string GetDefaultLearningObjective(ChallengeType challengeType, int index)
        {
            return challengeType switch
            {
                ChallengeType.CollectiveGrowing => index switch
                {
                    0 => "Understand collaborative growing techniques and resource sharing",
                    1 => "Learn efficient plant care coordination in community settings",
                    2 => "Master collective decision-making for optimal growing conditions",
                    _ => "Develop community cultivation expertise"
                },
                ChallengeType.KnowledgeSharing => index switch
                {
                    0 => "Effectively share cultivation knowledge with community members",
                    1 => "Learn from experienced growers and mentors",
                    2 => "Contribute to the community knowledge base",
                    _ => "Build teaching and learning skills"
                },
                ChallengeType.ResourceSharing => index switch
                {
                    0 => "Understand sustainable resource management principles",
                    1 => "Learn efficient resource allocation strategies",
                    2 => "Master collaborative resource optimization",
                    _ => "Develop resource sharing expertise"
                },
                ChallengeType.ProblemSolving => index switch
                {
                    0 => "Identify and analyze cultivation challenges collaboratively",
                    1 => "Develop creative solutions through community brainstorming",
                    2 => "Implement and evaluate problem-solving strategies",
                    _ => "Master collaborative problem-solving"
                },
                ChallengeType.Innovation => index switch
                {
                    0 => "Explore innovative cultivation techniques and technologies",
                    1 => "Experiment with new approaches in controlled environments",
                    2 => "Share and validate innovative solutions with the community",
                    _ => "Develop innovation and experimentation skills"
                },
                ChallengeType.Environmental => index switch
                {
                    0 => "Understand environmental sustainability in cannabis cultivation",
                    1 => "Implement eco-friendly growing practices",
                    2 => "Measure and reduce environmental impact",
                    _ => "Master sustainable cultivation practices"
                },
                _ => index switch
                {
                    0 => "Participate effectively in community challenges",
                    1 => "Collaborate and communicate with team members",
                    2 => "Contribute to collective goals and objectives",
                    _ => "Develop community participation skills"
                }
            };
        }
        
        public bool CanPlayerParticipate(PlayerProfile playerProfile)
        {
            // Check minimum participation requirements
            foreach (var requirement in _participationRequirements)
            {
                if (!playerProfile.MeetsRequirement(requirement))
                    return false;
            }
            
            // Check if late joining is allowed
            if (!_allowLateJoining && DateTime.Now > _startTime)
                return false;
            
            // Check late joining cutoff
            if (_allowLateJoining && DateTime.Now > _startTime.Add(_lateJoiningCutoff))
                return false;
            
            return true;
        }
        
        public float GetDifficultyMultiplier(int currentParticipants)
        {
            if (!_enableDynamicDifficulty) return 1f;
            
            var participationRatio = (float)currentParticipants / _maxParticipants;
            return 1f + (participationRatio * (_difficultyScalingFactor - 1f));
        }
        
        public float GetCollaborationBonus(int collaborativeActions)
        {
            if (!_enableCollaborationBonus) return 1f;
            
            var bonusMultiplier = Mathf.Min(_collaborationBonusMultiplier, 1f + (collaborativeActions * 0.1f));
            return bonusMultiplier;
        }
        
        public float GetTimeBonus(DateTime completionTime)
        {
            if (!_enableTimeBonus) return 1f;
            
            var totalDuration = _endTime - _startTime;
            var timeUsed = completionTime - _startTime;
            var timeRatio = (float)(timeUsed.TotalSeconds / totalDuration.TotalSeconds);
            
            // Bonus for early completion (1.5x for completing in first 50% of time)
            return timeRatio <= 0.5f ? 1.5f : Mathf.Lerp(1.5f, 1f, (timeRatio - 0.5f) * 2f);
        }
        
        public CommunityChallengeSummary GetChallengeSummary()
        {
            return new CommunityChallengeSummary
            {
                EventId = _eventId,
                ChallengeType = _challengeType,
                CollaborationMode = _collaborationMode,
                MaxParticipants = _maxParticipants,
                TargetAmount = _targetAmount,
                HasTeams = _enableTeams,
                HasEducationalContent = _hasEducationalContent,
                HasNarrativeIntegration = _enableNarrativeIntegration,
                CreatedAt = DateTime.Now
            };
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class SubGoalSO : ScriptableObject
    {
        [SerializeField] private string _goalId;
        [SerializeField] private string _goalName;
        [SerializeField] private string _goalType;
        [SerializeField] private string _description;
        [SerializeField] private float _targetAmount;
        [SerializeField] private bool _isRequired;
        [SerializeField] private List<EventReward> _rewards = new List<EventReward>();
        
        public string GoalId => _goalId;
        public string GoalName => _goalName;
        public string GoalType => _goalType;
        public string Description => _description;
        public float TargetAmount => _targetAmount;
        public bool IsRequired => _isRequired;
        public List<EventReward> Rewards => _rewards;
    }
    
    [Serializable]
    public class CommunityMilestoneSO : ScriptableObject
    {
        [SerializeField] private string _milestoneId;
        [SerializeField] private string _milestoneName;
        [SerializeField] private string _description;
        [SerializeField] private float _targetValue;
        [SerializeField] private string _progressType;
        [SerializeField] private List<EventReward> _rewards = new List<EventReward>();
        [SerializeField] private bool _isHidden;
        [SerializeField] private bool _triggersSpecialEvent;
        
        public string MilestoneId => _milestoneId;
        public string MilestoneName => _milestoneName;
        public string Description => _description;
        public float TargetValue => _targetValue;
        public string ProgressType => _progressType;
        public List<EventReward> Rewards => _rewards;
        public bool IsHidden => _isHidden;
        public bool TriggersSpecialEvent => _triggersSpecialEvent;
    }
    
    [Serializable]
    public class CommunityChoiceSO : ScriptableObject
    {
        [SerializeField] private string _choiceId;
        [SerializeField] private string _choiceName;
        [SerializeField] private string _description;
        [SerializeField] private List<string> _options = new List<string>();
        [SerializeField] private TimeSpan _votingDuration;
        [SerializeField] private bool _requiresMinimumVotes;
        [SerializeField] private int _minimumVotesRequired;
        [SerializeField] private bool _affectsStory;
        
        public string ChoiceId => _choiceId;
        public string ChoiceName => _choiceName;
        public string Description => _description;
        public List<string> Options => _options;
        public TimeSpan VotingDuration => _votingDuration;
        public bool RequiresMinimumVotes => _requiresMinimumVotes;
        public int MinimumVotesRequired => _minimumVotesRequired;
        public bool AffectsStory => _affectsStory;
    }
    
    [Serializable]
    public class CommunityChallengeSummary
    {
        public string EventId;
        public ChallengeType ChallengeType;
        public CollaborationMode CollaborationMode;
        public int MaxParticipants;
        public float TargetAmount;
        public bool HasTeams;
        public bool HasEducationalContent;
        public bool HasNarrativeIntegration;
        public DateTime CreatedAt;
    }
}