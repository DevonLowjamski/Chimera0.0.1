using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;
using ProjectChimera.Data.Events;
using ProjectChimera.Data.Narrative;

// Type aliases to resolve ambiguous references
using DataPlayerContribution = ProjectChimera.Data.PlayerContribution;
using DataCollaborationMode = ProjectChimera.Data.Events.CollaborationMode;
using DataRewardDistributionType = ProjectChimera.Data.Events.RewardDistributionType;
using ChallengeType = ProjectChimera.Data.Events.ChallengeType;

namespace ProjectChimera.Systems.Events
{
    // Extension method for Dictionary to support GetValueOrDefault
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// Community challenge event implementation for Project Chimera's live events system.
    /// Handles collaborative community goals, multi-player objectives, and shared rewards
    /// with integration to the narrative system for story-driven community events.
    /// </summary>
    public class CommunityChallenge : CommunityEventBase, ICommunityEvent, IRewardableEvent, IEducationalEvent
    {
        // Community challenge properties
        public CommunityGoal CommunityGoal { get; private set; }
        public ChallengeType ChallengeType { get; private set; }
        public DataCollaborationMode CollaborationMode { get; private set; }
        public CommunityGoalTracker GoalTracker { get; private set; }
        
        // IRewardableEvent implementation
        public RewardStructure RewardStructure { get; private set; }
        
        // IEducationalEvent implementation
        public List<LearningObjective> LearningObjectives { get; private set; }
        public EducationalContent EducationalMaterials { get; private set; }
        public bool RequiresScientificAccuracy { get; private set; }
        public bool IsScientificallyValidated { get; private set; }
        
        // Challenge configuration
        private CommunityChallengeSO _challengeConfig;
        private Dictionary<string, CommunityContribution> _memberContributions = new Dictionary<string, CommunityContribution>();
        private List<CommunityMilestone> _milestones = new List<CommunityMilestone>();
        private CommunityLeaderboard _leaderboard;
        
        // Public properties for external access
        public bool IsCompleted => CommunityGoal?.IsCompleted ?? false;
        public string GoalId => CommunityGoal?.GoalId ?? EventId;
        
        // Collaboration and social features
        private Dictionary<string, CommunityTeam> _teams = new Dictionary<string, CommunityTeam>();
        private List<CommunityCollaborator> _collaborators = new List<CommunityCollaborator>();
        private CommunityChat _communityChat;
        private SharedResourcePool _sharedResources;
        
        // Progress tracking
        private Dictionary<string, float> _subGoalProgress = new Dictionary<string, float>();
        private List<CompletedMilestone> _completedMilestones = new List<CompletedMilestone>();
        private CommunityMetrics _communityMetrics;
        
        // Narrative integration
        private NarrativeEventTrigger _narrativeIntegration;
        private List<StoryEventSO> _storyTriggers = new List<StoryEventSO>();
        private Dictionary<string, NarrativeChoice> _communityChoices = new Dictionary<string, NarrativeChoice>();
        
        // Educational progress tracking
        private Dictionary<string, EducationalProgress> _learningProgress = new Dictionary<string, EducationalProgress>();
        private CommunityKnowledgeBase _knowledgeBase;
        
        public CommunityChallenge(ILiveEventDefinition definition) 
            : base(definition, new CommunityGoal())
        {
            // Cast to community challenge-specific config
            if (definition is CommunityChallengeSO challengeConfig)
            {
                _challengeConfig = challengeConfig;
                InitializeCommunityChallenge(challengeConfig);
            }
            else
            {
                Debug.LogError($"[CommunityChallenge] Invalid event definition type for community challenge: {definition.GetType()}");
            }
        }
        
        private void InitializeCommunityChallenge(CommunityChallengeSO config)
        {
            ChallengeType = config.ChallengeType;
            CollaborationMode = (DataCollaborationMode)config.CollaborationMode;
            MaxParticipants = config.MaxParticipants;
            
            // Initialize community goal
            CommunityGoal = new CommunityGoal
            {
                GoalId = $"goal_{config.EventId}",
                GoalType = config.GoalType,
                TargetAmount = config.TargetAmount,
                Description = config.GoalDescription,
                TimeLimit = config.Duration
            };
            
            // Initialize goal tracker
            GoalTracker = new CommunityGoalTracker();
            
            // Initialize reward structure
            RewardStructure = new RewardStructure
            {
                RewardStructureId = $"rewards_{EventId}",
                DistributionType = (RewardDistributionType)config.RewardDistributionType,
                Rewards = new List<EventReward>() // Initialize with empty list for now
            };
            
            // Initialize educational content if present
            if (config.HasEducationalContent)
            {
                RequiresScientificAccuracy = config.RequiresScientificAccuracy;
                IsScientificallyValidated = config.IsScientificallyValidated;
                
                EducationalMaterials = new EducationalContent
                {
                    ContentId = $"edu_{EventId}",
                    Title = $"Educational Content for {config.ChallengeType}",
                    Description = $"Learn about {config.ChallengeType} challenge: {GetEducationalTopic(config.ChallengeType)}",
                    LearningObjectives = new List<LearningObjective>(),
                    IsScientificallyValidated = config.IsScientificallyValidated
                };
                
                LoadLearningObjectives(config.LearningObjectives?.ToList() ?? new List<string>());
            }
            
            // Initialize systems
            _leaderboard = new CommunityLeaderboard(EventId, config.EnableLeaderboard);
            _communityMetrics = new CommunityMetrics(EventId);
            
            // Initialize collaboration features
            if (config.EnableTeams)
            {
                InitializeTeamCollaboration(config);
            }
            
            if (config.EnableSharedResources)
            {
                _sharedResources = new SharedResourcePool(EventId, config.SharedResourceTypes);
            }
            
            if (config.EnableCommunityChat)
            {
                _communityChat = new CommunityChat(EventId, config.ChatModerationLevel);
            }
            
            // Initialize milestones
            InitializeMilestones(config.Milestones);
            
            // Initialize narrative integration
            if (config.EnableNarrativeIntegration)
            {
                InitializeNarrativeIntegration(config);
            }
            
            // Initialize knowledge base
            if (config.HasEducationalContent)
            {
                _knowledgeBase = new CommunityKnowledgeBase(EventId, config.KnowledgeCategories);
            }
        }
        
        #region Event Lifecycle Override
        
        protected override void OnEventStartedInternal()
        {
            base.OnEventStartedInternal();
            
            // Initialize community systems
            InitializeCommunityParticipation();
            
            // Start collaborative features
            if (_sharedResources != null)
            {
                _sharedResources.Initialize();
            }
            
            if (_communityChat != null)
            {
                _communityChat.StartChat();
            }
            
            // Trigger narrative integration
            if (_narrativeIntegration != null)
            {
                _narrativeIntegration.TriggerCommunityEventStart(this);
            }
            
            Debug.Log($"[CommunityChallenge] Started community challenge: {EventId}");
        }
        
        protected override void OnUpdateInternal(float deltaTime)
        {
            base.OnUpdateInternal(deltaTime);
            
            // Update community goal progress
            UpdateCommunityGoalProgress();
            
            // Update milestone progress
            CheckMilestoneCompletion();
            
            // Update leaderboard
            if (_leaderboard != null)
            {
                _leaderboard.UpdateLeaderboard(_memberContributions);
            }
            
            // Update collaborative systems
            UpdateCollaborativeSystems(deltaTime);
            
            // Update narrative integration
            if (_narrativeIntegration != null)
            {
                _narrativeIntegration.UpdateCommunityProgress(this);
            }
            
            // Update metrics
            _communityMetrics.UpdateMetrics(DateTime.Now);
        }
        
        protected override void OnPlayerActionInternal(PlayerAction action)
        {
            base.OnPlayerActionInternal(action);
            
            switch (action.Type)
            {
                case ActionType.Contribute:
                    HandlePlayerContribution(action);
                    break;
                case ActionType.Share:
                    HandleResourceSharing(action);
                    break;
                case ActionType.Help:
                    HandlePlayerHelp(action);
                    break;
                case ActionType.Learn:
                    HandleLearningActivity(action);
                    break;
                case ActionType.Teach:
                    HandleTeachingActivity(action);
                    break;
                case ActionType.Challenge:
                    HandleSubChallenge(action);
                    break;
                case ActionType.Celebrate:
                    HandleCelebration(action);
                    break;
            }
        }
        
        protected override void OnCommunityGoalReached()
        {
            base.OnCommunityGoalReached();
            
            // Distribute community rewards
            DistributeCommunityRewards();
            
            // Complete educational objectives
            CompleteCommunityLearningObjectives();
            
            // Trigger narrative completion
            if (_narrativeIntegration != null)
            {
                _narrativeIntegration.TriggerCommunityGoalComplete(this);
            }
            
            // Archive community achievements
            ArchiveCommunityAchievements();
            
            Debug.Log($"[CommunityChallenge] Community goal reached for challenge: {EventId}");
        }
        
        #endregion
        
        #region Player Action Handlers
        
        private void HandlePlayerContribution(PlayerAction action)
        {
            var player = action.Player;
            var contributionType = action.Parameters.GetValueOrDefault("contributionType") as string;
            var contributionAmount = (float)(action.Parameters.GetValueOrDefault("amount") ?? 0f);
            var contributionData = action.Parameters.GetValueOrDefault("data");
            
            if (string.IsNullOrEmpty(contributionType) || contributionAmount <= 0f)
            {
                Debug.LogWarning($"[CommunityChallenge] Invalid contribution from player: {player.PlayerId}");
                return;
            }
            
            // Record contribution
            RecordPlayerContribution(player, contributionType, contributionAmount, contributionData);
            
            // Update community progress  
            var contribution = new PlayerContribution
            {
                PlayerId = player.PlayerId,
                TotalContribution = contributionAmount,
                LastContribution = DateTime.Now
            };
            
            UpdateCommunityProgress(contribution);
            
            // Update leaderboard
            if (_leaderboard != null)
            {
                _leaderboard.UpdatePlayerScore(player.PlayerId, contributionAmount);
            }
            
            // Track for narrative integration
            if (_narrativeIntegration != null)
            {
                _narrativeIntegration.TrackPlayerContribution(player, contributionType, contributionAmount);
            }
            
            Debug.Log($"[CommunityChallenge] Player {player.PlayerId} contributed {contributionAmount} {contributionType}");
        }
        
        private void HandleResourceSharing(PlayerAction action)
        {
            if (_sharedResources == null || !_challengeConfig.EnableSharedResources)
                return;
            
            var player = action.Player;
            var resourceType = action.Parameters.GetValueOrDefault("resourceType") as string;
            var resourceAmount = (float)(action.Parameters.GetValueOrDefault("amount") ?? 0f);
            
            if (_sharedResources.ShareResource(player.PlayerId, resourceType, resourceAmount))
            {
                // Track sharing for community metrics
                _communityMetrics.TrackResourceSharing(player.PlayerId, resourceType, resourceAmount);
                
                Debug.Log($"[CommunityChallenge] Player {player.PlayerId} shared {resourceAmount} {resourceType}");
            }
        }
        
        private void HandlePlayerHelp(PlayerAction action)
        {
            var helper = action.Player;
            var helpeeId = action.Parameters.GetValueOrDefault("helpeeId") as string;
            var helpType = action.Parameters.GetValueOrDefault("helpType") as string;
            
            if (!string.IsNullOrEmpty(helpeeId) && !string.IsNullOrEmpty(helpType))
            {
                RecordPlayerHelp(helper.PlayerId, helpeeId, helpType);
                
                // Track collaboration
                _communityMetrics.TrackCollaboration(helper.PlayerId, helpeeId, helpType);
                
                Debug.Log($"[CommunityChallenge] Player {helper.PlayerId} helped {helpeeId} with {helpType}");
            }
        }
        
        private void HandleLearningActivity(PlayerAction action)
        {
            if (!_challengeConfig.HasEducationalContent)
                return;
            
            var player = action.Player;
            var objectiveId = action.Parameters.GetValueOrDefault("objectiveId") as string;
            var progress = (float)(action.Parameters.GetValueOrDefault("progress") ?? 0f);
            
            if (!string.IsNullOrEmpty(objectiveId))
            {
                UpdateLearningProgress(player, objectiveId, progress);
                
                // Update community knowledge base
                if (_knowledgeBase != null)
                {
                    _knowledgeBase.RecordLearningActivity(player.PlayerId, objectiveId, progress);
                }
            }
        }
        
        private void HandleTeachingActivity(PlayerAction action)
        {
            if (!_challengeConfig.HasEducationalContent || !_challengeConfig.EnableKnowledgeSharing)
                return;
            
            var teacher = action.Player;
            var studentId = action.Parameters.GetValueOrDefault("studentId") as string;
            var knowledge = action.Parameters.GetValueOrDefault("knowledge") as string;
            
            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(knowledge))
            {
                RecordTeachingActivity(teacher.PlayerId, studentId, knowledge);
                
                // Update knowledge base
                if (_knowledgeBase != null)
                {
                    _knowledgeBase.RecordKnowledgeSharing(teacher.PlayerId, studentId, knowledge);
                }
                
                Debug.Log($"[CommunityChallenge] Player {teacher.PlayerId} taught {knowledge} to {studentId}");
            }
        }
        
        private void HandleSubChallenge(PlayerAction action)
        {
            var player = action.Player;
            var challengeId = action.Parameters.GetValueOrDefault("challengeId") as string;
            var challengeData = action.Parameters.GetValueOrDefault("challengeData");
            
            if (!string.IsNullOrEmpty(challengeId))
            {
                CreateSubChallenge(player, challengeId, challengeData);
            }
        }
        
        private void HandleCelebration(PlayerAction action)
        {
            var player = action.Player;
            var celebrationType = action.Parameters.GetValueOrDefault("type") as string ?? "milestone";
            var celebrationData = action.Parameters.GetValueOrDefault("data");
            
            TriggerCommunityCelebration(player, celebrationType, celebrationData);
        }
        
        #endregion
        
        #region Community Features
        
        private void InitializeCommunityParticipation()
        {
            foreach (var participant in _participants)
            {
                // Initialize member contribution tracking
                _memberContributions[participant.PlayerId] = new CommunityContribution
                {
                    PlayerId = participant.PlayerId,
                    PlayerProfile = participant,
                    JoinTime = DateTime.Now,
                    TotalContribution = 0f,
                    ContributionBreakdown = new Dictionary<string, float>()
                };
                
                // Add to team if team mode is enabled
                if (_challengeConfig.EnableTeams)
                {
                    AssignPlayerToTeam(participant);
                }
                
                // Initialize learning progress
                if (_challengeConfig.HasEducationalContent)
                {
                    _learningProgress[participant.PlayerId] = new EducationalProgress
                    {
                        PlayerId = participant.PlayerId,
                        EventId = EventId,
                        StartTime = DateTime.Now
                    };
                }
            }
        }
        
        private void InitializeTeamCollaboration(CommunityChallengeSO config)
        {
            var teamCount = Mathf.Min(config.MaxTeams, Mathf.CeilToInt((float)MaxParticipants / config.MaxTeamSize));
            
            for (int i = 0; i < teamCount; i++)
            {
                var teamId = $"team_{i + 1}";
                _teams[teamId] = new CommunityTeam
                {
                    TeamId = teamId,
                    TeamName = $"Team {i + 1}",
                    MaxMembers = config.MaxTeamSize,
                    Members = new List<string>(),
                    TeamScore = 0f,
                    IsActive = true
                };
            }
        }
        
        private void InitializeMilestones(List<CommunityMilestoneSO> milestoneConfigs)
        {
            if (milestoneConfigs == null) return;
            
            foreach (var milestoneConfig in milestoneConfigs)
            {
                _milestones.Add(new CommunityMilestone
                {
                    MilestoneId = milestoneConfig.MilestoneId,
                    Name = milestoneConfig.MilestoneName,
                    Description = milestoneConfig.Description,
                    TargetValue = milestoneConfig.TargetValue,
                    CurrentValue = 0f,
                    IsCompleted = false,
                    Rewards = new List<EventReward>() // Initialize with empty list for now
                });
            }
        }
        
        private void InitializeNarrativeIntegration(CommunityChallengeSO config)
        {
            _narrativeIntegration = new NarrativeEventTrigger(EventId);
            
            // Load story triggers
            if (config.StoryTriggers != null)
            {
                _storyTriggers = config.StoryTriggers.ToList();
                foreach (var storyTrigger in _storyTriggers)
                {
                    _narrativeIntegration.RegisterStoryTrigger(storyTrigger);
                }
            }
            
            // Initialize community choices for narrative branching
            if (config.EnableCommunityChoices)
            {
                InitializeCommunityChoices(config.CommunityChoices);
            }
        }
        
        private void InitializeCommunityChoices(List<CommunityChoiceSO> choiceConfigs)
        {
            if (choiceConfigs == null) return;
            
            foreach (var choiceConfig in choiceConfigs)
            {
                _communityChoices[choiceConfig.ChoiceId] = new NarrativeChoice
                {
                    ChoiceId = choiceConfig.ChoiceId,
                    Description = choiceConfig.Description,
                    Options = choiceConfig.Options != null ? choiceConfig.Options.ToList() : new List<string>(),
                    VotingDeadline = DateTime.Now.Add(choiceConfig.VotingDuration),
                    Votes = new Dictionary<string, int>()
                };
            }
        }
        
        private void UpdateCommunityGoalProgress()
        {
            var totalContribution = _memberContributions.Values.Sum(c => c.TotalContribution);
            CommunityProgress = totalContribution;
            
            // Update sub-goal progress
            foreach (var subGoal in _challengeConfig.SubGoals ?? new List<SubGoalSO>())
            {
                var subGoalContribution = _memberContributions.Values
                    .Where(c => c.ContributionBreakdown.ContainsKey(subGoal.GoalType))
                    .Sum(c => c.ContributionBreakdown[subGoal.GoalType]);
                
                _subGoalProgress[subGoal.GoalId] = subGoalContribution;
            }
        }
        
        private void CheckMilestoneCompletion()
        {
            foreach (var milestone in _milestones.Where(m => !m.IsCompleted))
            {
                milestone.CurrentValue = GetMilestoneProgress(milestone.MilestoneId);
                
                if (milestone.CurrentValue >= milestone.TargetValue)
                {
                    CompleteMilestone(milestone);
                }
            }
        }
        
        private void CompleteMilestone(CommunityMilestone milestone)
        {
            milestone.IsCompleted = true;
            milestone.CompletionTime = DateTime.Now;
            
            _completedMilestones.Add(new CompletedMilestone
            {
                Milestone = milestone,
                CompletionTime = DateTime.Now,
                ParticipantCount = CurrentParticipants
            });
            
            // Distribute milestone rewards
            DistributeMilestoneRewards(milestone);
            
            // Trigger narrative milestone event
            if (_narrativeIntegration != null)
            {
                _narrativeIntegration.TriggerMilestoneComplete(milestone);
            }
            
            // Trigger celebration
            TriggerCommunityCelebration(null, "milestone", milestone);
            
            Debug.Log($"[CommunityChallenge] Milestone completed: {milestone.Name}");
        }
        
        #endregion
        
        #region IEducationalEvent Implementation
        
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
        
        public void ValidateEducationalContent()
        {
            if (!_challengeConfig.HasEducationalContent)
                return;
            
            // Validate educational materials
            if (EducationalMaterials != null)
            {
                var validator = new EducationalContentValidator();
                var result = validator.ValidateContent(EducationalMaterials);
                IsScientificallyValidated = result.IsValid;
                
                if (!IsScientificallyValidated && RequiresScientificAccuracy)
                {
                    Debug.LogError($"[CommunityChallenge] Educational content validation failed for challenge: {EventId}");
                }
            }
        }
        
        private void UpdateLearningProgress(PlayerProfile player, string objectiveId, float progress)
        {
            var learningProgress = TrackLearningProgress(player);
            learningProgress.ObjectiveProgress[objectiveId] = progress;
            learningProgress.LastUpdate = DateTime.Now;
            
            // Update overall progress
            learningProgress.OverallProgress = learningProgress.ObjectiveProgress.Values.Average();
        }
        
        private void LoadLearningObjectives(List<string> objectiveDescriptions)
        {
            LearningObjectives = new List<LearningObjective>();
            
            for (int i = 0; i < objectiveDescriptions.Count; i++)
            {
                LearningObjectives.Add(new LearningObjective
                {
                    ObjectiveId = $"obj_{i}",
                    Description = objectiveDescriptions[i],
                    CompletionThreshold = 0.8f,
                    IsRequired = i < 3 // First 3 are required
                });
            }
        }
        
        private void CompleteCommunityLearningObjectives()
        {
            foreach (var objective in LearningObjectives)
            {
                var completionRate = GetObjectiveCompletionRate(objective.ObjectiveId);
                if (completionRate >= objective.CompletionThreshold)
                {
                    // Award community learning achievement
                    AwardCommunityLearningAchievement(objective);
                }
            }
        }
        
        #endregion
        
        #region IRewardableEvent Implementation
        
        public List<EventReward> GetAvailableRewards(PlayerProfile player)
        {
            var rewards = new List<EventReward>();
            
            // Individual contribution rewards
            if (_memberContributions.TryGetValue(player.PlayerId, out var contribution))
            {
                if (contribution.TotalContribution >= _challengeConfig.MinimumContributionForReward)
                {
                    rewards.AddRange(RewardStructure.Rewards.Where(r => r.RewardType == "participation"));
                }
                
                // Performance-based rewards
                var leaderboardPosition = _leaderboard?.GetPlayerPosition(player.PlayerId) ?? -1;
                if (leaderboardPosition > 0 && leaderboardPosition <= 10)
                {
                    rewards.AddRange(RewardStructure.Rewards.Where(r => r.RewardType == "performance"));
                }
            }
            
            // Educational rewards
            if (_challengeConfig.HasEducationalContent)
            {
                var learningProgress = _learningProgress.GetValueOrDefault(player.PlayerId);
                if (learningProgress?.OverallProgress >= 0.8f)
                {
                    rewards.AddRange(RewardStructure.Rewards.Where(r => r.RewardType == "education"));
                }
            }
            
            return rewards;
        }
        
        public void DistributeRewards()
        {
            foreach (var contribution in _memberContributions.Values)
            {
                var rewards = GetAvailableRewards(contribution.PlayerProfile);
                foreach (var reward in rewards)
                {
                    GrantReward(contribution.PlayerProfile, reward);
                }
            }
        }
        
        public bool HasReceivedReward(PlayerProfile player, string rewardId)
        {
            // Check reward history
            return false; // Placeholder
        }
        
        public void GrantReward(PlayerProfile player, EventReward reward)
        {
            // Grant reward to player
            Debug.Log($"[CommunityChallenge] Granted reward {reward.RewardId} to player {player.PlayerId}");
        }
        
        private void DistributeCommunityRewards()
        {
            if (IsCommunityGoalReached)
            {
                // Distribute community-wide rewards
                foreach (var participant in _participants)
                {
                    var communityRewards = RewardStructure.Rewards.Where(r => r.RewardType == "community").ToList();
                    foreach (var reward in communityRewards)
                    {
                        GrantReward(participant, reward);
                    }
                }
            }
        }
        
        private void DistributeMilestoneRewards(CommunityMilestone milestone)
        {
            foreach (var participant in _participants)
            {
                foreach (var reward in milestone.Rewards)
                {
                    GrantReward(participant, reward);
                }
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GetEducationalTopic(ChallengeType challengeType)
        {
            return challengeType switch
            {
                ChallengeType.CollectiveGrowing => "Collaborative Cannabis Cultivation Techniques",
                ChallengeType.KnowledgeSharing => "Cannabis Science and Community Education",
                ChallengeType.ResourceSharing => "Sustainable Resource Management in Cannabis Cultivation",
                ChallengeType.ProblemSolving => "Community Problem-Solving in Cannabis Growing",
                ChallengeType.Innovation => "Innovation and Experimentation in Cannabis Science",
                ChallengeType.Environmental => "Environmental Stewardship in Cannabis Cultivation",
                _ => "Community Cannabis Cultivation"
            };
        }
        
        private void RecordPlayerContribution(PlayerProfile player, string contributionType, float amount, object data)
        {
            if (_memberContributions.TryGetValue(player.PlayerId, out var contribution))
            {
                contribution.TotalContribution += amount;
                contribution.LastContribution = DateTime.Now;
                
                if (!contribution.ContributionBreakdown.ContainsKey(contributionType))
                {
                    contribution.ContributionBreakdown[contributionType] = 0f;
                }
                contribution.ContributionBreakdown[contributionType] += amount;
            }
        }
        
        private void AssignPlayerToTeam(PlayerProfile player)
        {
            // Find team with least members
            var availableTeam = _teams.Values
                .Where(team => team.Members.Count < team.MaxMembers)
                .OrderBy(team => team.Members.Count)
                .FirstOrDefault();
            
            if (availableTeam != null)
            {
                availableTeam.Members.Add(player.PlayerId);
                Debug.Log($"[CommunityChallenge] Player {player.PlayerId} assigned to {availableTeam.TeamId}");
            }
        }
        
        private float GetMilestoneProgress(string milestoneId)
        {
            // Calculate milestone progress based on milestone type and community contributions
            return _memberContributions.Values.Sum(c => c.TotalContribution);
        }
        
        private float GetObjectiveCompletionRate(string objectiveId)
        {
            if (_learningProgress.Count == 0) return 0f;
            
            var completedCount = _learningProgress.Values
                .Count(progress => progress.ObjectiveProgress.GetValueOrDefault(objectiveId, 0f) >= 0.8f);
            
            return (float)completedCount / _learningProgress.Count;
        }
        
        private void UpdateCollaborativeSystems(float deltaTime)
        {
            // Update shared resources
            _sharedResources?.Update(deltaTime);
            
            // Update community chat
            _communityChat?.Update(deltaTime);
            
            // Update teams
            UpdateTeamProgress();
        }
        
        private void UpdateTeamProgress()
        {
            foreach (var team in _teams.Values)
            {
                team.TeamScore = team.Members
                    .Where(memberId => _memberContributions.ContainsKey(memberId))
                    .Sum(memberId => _memberContributions[memberId].TotalContribution);
            }
        }
        
        // Placeholder methods
        private void RecordPlayerHelp(string helperId, string helpeeId, string helpType) { }
        private void RecordTeachingActivity(string teacherId, string studentId, string knowledge) { }
        private void CreateSubChallenge(PlayerProfile player, string challengeId, object data) { }
        private void TriggerCommunityCelebration(PlayerProfile player, string type, object data) { }
        private void AwardCommunityLearningAchievement(LearningObjective objective) { }
        private void ArchiveCommunityAchievements() { }
        
        #endregion
    }
    
    // Supporting data structures and enums
    // CollaborationMode enum is defined in LiveEventInterfaces.cs - removed duplicate
    
    [Serializable]
    public class CommunityContribution
    {
        public string PlayerId;
        public PlayerProfile PlayerProfile;
        public DateTime JoinTime;
        public DateTime LastContribution;
        public float TotalContribution;
        public Dictionary<string, float> ContributionBreakdown = new Dictionary<string, float>();
        public List<string> Achievements = new List<string>();
    }
    
    [Serializable]
    public class CommunityMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public float TargetValue;
        public float CurrentValue;
        public bool IsCompleted;
        public DateTime CompletionTime;
        public List<EventReward> Rewards = new List<EventReward>();
    }
    
    [Serializable]
    public class CompletedMilestone
    {
        public CommunityMilestone Milestone;
        public DateTime CompletionTime;
        public int ParticipantCount;
    }
    
    [Serializable]
    public class CommunityTeam
    {
        public string TeamId;
        public string TeamName;
        public int MaxMembers;
        public List<string> Members = new List<string>();
        public float TeamScore;
        public bool IsActive;
        public Dictionary<string, object> TeamData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CommunityCollaborator
    {
        public string CollaboratorId;
        public string Role;
        public List<string> Specializations = new List<string>();
        public float CollaborationScore;
        public bool IsActive;
    }
    
    [Serializable]
    public class NarrativeChoice
    {
        public string ChoiceId;
        public string Description;
        public List<string> Options = new List<string>();
        public DateTime VotingDeadline;
        public Dictionary<string, int> Votes = new Dictionary<string, int>();
        public string WinningOption;
        public bool IsResolved;
    }
    
    // CommunityGoalTracker is defined in LiveEventsManager.cs
    
    public class CommunityLeaderboard
    {
        public CommunityLeaderboard(string eventId, bool enabled) { }
        public void UpdateLeaderboard(Dictionary<string, CommunityContribution> contributions) { }
        public void UpdatePlayerScore(string playerId, float score) { }
        public int GetPlayerPosition(string playerId) => -1;
    }
    
    public class CommunityMetrics
    {
        public CommunityMetrics(string eventId) { }
        public void UpdateMetrics(DateTime currentTime) { }
        public void TrackResourceSharing(string playerId, string resourceType, float amount) { }
        public void TrackCollaboration(string helperId, string helpeeId, string helpType) { }
    }
    
    public class CommunityChat
    {
        public CommunityChat(string eventId, string moderationLevel) { }
        public void StartChat() { }
        public void Update(float deltaTime) { }
    }
    
    public class SharedResourcePool
    {
        public SharedResourcePool(string eventId, List<string> resourceTypes) { }
        public void Initialize() { }
        public void Update(float deltaTime) { }
        public bool ShareResource(string playerId, string resourceType, float amount) => true;
    }
    
    public class CommunityKnowledgeBase
    {
        public CommunityKnowledgeBase(string eventId, List<string> categories) { }
        public void RecordLearningActivity(string playerId, string objectiveId, float progress) { }
        public void RecordKnowledgeSharing(string teacherId, string studentId, string knowledge) { }
    }
    
    public class NarrativeEventTrigger
    {
        public NarrativeEventTrigger(string eventId) { }
        public void RegisterStoryTrigger(StoryEventSO storyTrigger) { }
        public void TriggerCommunityEventStart(CommunityChallenge challenge) { }
        public void UpdateCommunityProgress(CommunityChallenge challenge) { }
        public void TriggerCommunityGoalComplete(CommunityChallenge challenge) { }
        public void TrackPlayerContribution(PlayerProfile player, string type, float amount) { }
        public void TriggerMilestoneComplete(CommunityMilestone milestone) { }
    }
}