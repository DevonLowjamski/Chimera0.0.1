using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Data.Community;
using CommunityData = ProjectChimera.Data.Community;
using System.Collections.Generic;
using System.Linq;
using System;
using ProjectChimera.Data.UI;

namespace ProjectChimera.Systems.Community
{
    public class CommunityManager : ChimeraManager
    {
        [Header("Community Configuration")]
        [SerializeField] private bool _enableLeaderboards = true;
        [SerializeField] private bool _enableForum = true;
        [SerializeField] private bool _enableReputationSystem = true;
        [SerializeField] private bool _enableCommunityEvents = true;
        [SerializeField] private int _maxLeaderboardEntries = 1000;
        [SerializeField] private float _leaderboardUpdateInterval = 300f; // 5 minutes
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onReputationChanged;
        [SerializeField] private SimpleGameEventSO _onLeaderboardUpdated;
        [SerializeField] private SimpleGameEventSO _onForumPostCreated;
        [SerializeField] private SimpleGameEventSO _onCommunityEventStarted;
        [SerializeField] private SimpleGameEventSO _onAchievementUnlocked;
        
        private PlayerProfile _currentPlayerProfile;
        private Dictionary<string, CommunityData.EnhancedLeaderboard> _leaderboards = new Dictionary<string, CommunityData.EnhancedLeaderboard>();
        private List<CommunityData.EnhancedForumTopic> _forumTopics = new List<CommunityData.EnhancedForumTopic>();
        private Dictionary<string, CommunityData.ReputationSystem> _playerReputations = new Dictionary<string, CommunityData.ReputationSystem>();
        private List<CommunityData.EnhancedCommunityEvent> _activeEvents = new List<CommunityData.EnhancedCommunityEvent>();
        private Dictionary<string, CommunityData.Badge> _availableBadges = new Dictionary<string, CommunityData.Badge>();
        private Dictionary<string, CommunityData.EnhancedForumPost> _forumPosts = new Dictionary<string, CommunityData.EnhancedForumPost>();
        private Dictionary<CommunityData.ForumCategory, List<CommunityData.EnhancedForumTopic>> _topicsByCategory = new Dictionary<CommunityData.ForumCategory, List<CommunityData.EnhancedForumTopic>>();
        private List<string> _moderatorIds = new List<string>();
        private float _lastLeaderboardUpdate = 0f;
        private Dictionary<string, DateTime> _lastPlayerActivity = new Dictionary<string, DateTime>();
        private CommunityMetrics _communityMetrics = new CommunityMetrics();
        private Dictionary<string, CommunityData.EventParticipant> _activeParticipants = new Dictionary<string, CommunityData.EventParticipant>();
        private Dictionary<string, ProjectChimera.Core.EventParticipationData> _participationData = new Dictionary<string, ProjectChimera.Core.EventParticipationData>();
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        public PlayerProfile CurrentPlayer => _currentPlayerProfile;
        public List<CommunityData.EnhancedLeaderboard> ActiveLeaderboards => _leaderboards.Values.Where(l => l.Active).ToList();
        public List<CommunityData.EnhancedForumTopic> RecentTopics => _forumTopics.OrderByDescending(t => t.LastReplyDate).Take(10).ToList();
        public List<CommunityData.EnhancedCommunityEvent> UpcomingEvents => _activeEvents.Where(e => e.StartDate > DateTime.Now).ToList();
        public CommunityMetrics Metrics => _communityMetrics;
        public bool IsPlayerOnline => _currentPlayerProfile?.Status == PlayerStatus.Online;
        
        public Action<PlayerProfile> OnPlayerProfileUpdated;
        public Action<CommunityData.EnhancedLeaderboard> OnLeaderboardUpdated;
        public Action<CommunityData.EnhancedForumPost> OnForumPostCreated;
        public Action<int> OnReputationChanged;
        public Action<CommunityData.EnhancedCommunityEvent> OnEventStarted;
        public Action<CommunityData.Badge> OnBadgeEarned;
        public Action<Data.LeaderboardType, int> OnRankingChanged;
        public Action<CompetitionEvent> OnCompetitionStarted;
        public Action<CompetitionEvent> OnCompetitionEnded;
        public Action<RecordType, float> OnPersonalRecordSet;
        
        protected override void OnManagerInitialize()
        {
            InitializePlayerProfile();
            InitializeLeaderboards();
            InitializeForumSystem();
            InitializeReputationSystem();
            InitializeCommunityEvents();
            InitializeBadgeSystem();
            LogInfo("CommunityManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            if (_enableLeaderboards && currentTime - _lastLeaderboardUpdate >= _leaderboardUpdateInterval)
            {
                UpdateAllLeaderboards();
                _lastLeaderboardUpdate = currentTime;
            }
            if (_enableCommunityEvents) { UpdateCommunityEvents(); }
            UpdatePlayerActivity();
            UpdateCommunityMetrics();
        }
        
        public void UpdatePlayerProfile(string displayName, string bio = "", PlayerStatus status = PlayerStatus.Online)
        {
            if (_currentPlayerProfile == null) return;
            _currentPlayerProfile.DisplayName = displayName;
            _currentPlayerProfile.Bio = bio;
            _currentPlayerProfile.Status = status;
            _currentPlayerProfile.LastActive = DateTime.Now;
            OnPlayerProfileUpdated?.Invoke(_currentPlayerProfile);
            LogInfo($"Player profile updated: {displayName}");
        }
        
        public void AddExperience(int experiencePoints, string reason = "")
        {
            if (_currentPlayerProfile == null) return;
            int oldLevel = _currentPlayerProfile.Level;
            _currentPlayerProfile.Experience += experiencePoints;
            int newLevel = Mathf.FloorToInt(_currentPlayerProfile.Experience / 1000f) + 1;
            if (newLevel > oldLevel)
            {
                _currentPlayerProfile.Level = newLevel;
                LogInfo($"Player leveled up to {newLevel}!");
                AddReputation(CommunityData.ReputationType.Community_Participation, 50, $"Reached Level {newLevel}");
            }
            LogInfo($"Added {experiencePoints} experience - {reason}");
        }
        
        public void SetPlayerStatus(PlayerStatus status)
        {
            if (_currentPlayerProfile == null) return;
            _currentPlayerProfile.Status = status;
            _currentPlayerProfile.LastActive = DateTime.Now;
            if (status == PlayerStatus.Online)
            {
                _lastPlayerActivity[_currentPlayerProfile.PlayerId] = DateTime.Now;
            }
            LogInfo($"Player status set to: {status}");
        }
        
        public bool SubmitScore(ProjectChimera.Core.LeaderboardType leaderboardType, float score, string additionalData = "")
        {
            if (!_enableLeaderboards || _currentPlayerProfile == null) return false;
            string leaderboardId = leaderboardType.ToString();
            if (!_leaderboards.TryGetValue(leaderboardId, out var leaderboard))
            {
                LogWarning($"Leaderboard not found: {leaderboardType}");
                return false;
            }
            if (score < leaderboard.Settings.MinimumScore)
            {
                LogWarning($"Score {score} below minimum required: {leaderboard.Settings.MinimumScore}");
                return false;
            }
            var newEntry = new CommunityData.EnhancedLeaderboardEntry(
                _currentPlayerProfile.PlayerId, _currentPlayerProfile.DisplayName, score, DateTime.Now
            );
            newEntry.FormattedScore = FormatScore(score, leaderboardType);
            leaderboard.AddOrUpdateEntry(newEntry);
            OnLeaderboardUpdated?.Invoke(leaderboard);
            _onLeaderboardUpdated?.Raise();
            LogInfo($"Submitted score for {leaderboardType}: {score}");
            return true;
        }
        
        public CommunityData.EnhancedLeaderboard GetLeaderboard(ProjectChimera.Core.LeaderboardType type)
        {
            return _leaderboards.TryGetValue(type.ToString(), out var leaderboard) ? leaderboard : null;
        }
        
        public int GetPlayerRank(ProjectChimera.Core.LeaderboardType type, string playerId = null)
        {
            var leaderboard = GetLeaderboard(type);
            if (leaderboard == null) return -1;
            playerId ??= _currentPlayerProfile?.PlayerId;
            if (string.IsNullOrEmpty(playerId)) return -1;
            var entry = leaderboard.GetPlayerEntry(playerId);
            return entry?.Rank ?? -1;
        }
        
        public List<CommunityData.EnhancedLeaderboardEntry> GetTopEntries(ProjectChimera.Core.LeaderboardType type, int count = 10)
        {
            var leaderboard = GetLeaderboard(type);
            return leaderboard?.GetTopEntries(count) ?? new List<CommunityData.EnhancedLeaderboardEntry>();
        }
        
        public string CreateForumTopic(string title, string initialContent, CommunityData.ForumCategory category, CommunityData.TopicType type = CommunityData.TopicType.Discussion)
        {
            if (!_enableForum || _currentPlayerProfile == null) return null;
            var newTopic = new CommunityData.EnhancedForumTopic(title, _currentPlayerProfile.PlayerId, category, type);
            _forumTopics.Add(newTopic);
            if (!_topicsByCategory.ContainsKey(category))
            {
                _topicsByCategory[category] = new List<CommunityData.EnhancedForumTopic>();
            }
            _topicsByCategory[category].Add(newTopic);
            ReplyToTopic(newTopic.TopicId, initialContent);
            _onForumPostCreated?.Raise();
            LogInfo($"New forum topic created: {title}");
            return newTopic.TopicId;
        }
        
        public string ReplyToTopic(string topicId, string content)
        {
            if (!_enableForum || _currentPlayerProfile == null) return null;
            var topic = _forumTopics.FirstOrDefault(t => t.TopicId == topicId);
            if (topic == null)
            {
                LogWarning($"Topic not found: {topicId}");
                return null;
            }
            var newPost = new CommunityData.EnhancedForumPost(topicId, _currentPlayerProfile.PlayerId, content);
            _forumPosts.Add(newPost.PostId, newPost);
            topic.AddReply(newPost);
            OnForumPostCreated?.Invoke(newPost);
            LogInfo($"Replied to topic: {topic.Title}");
            return newPost.PostId;
        }
        
        public bool VoteOnPost(string postId, CommunityData.VoteType voteType)
        {
            if (!_enableForum || _currentPlayerProfile == null) return false;
            if (!_forumPosts.TryGetValue(postId, out var post))
            {
                LogWarning($"Post not found: {postId}");
                return false;
            }
            post.AddVote(new CommunityData.Vote(_currentPlayerProfile.PlayerId, voteType));
            AddReputation(CommunityData.ReputationType.Forum_Activity, 1, $"Voted on post {postId}");
            return true;
        }
        
        public List<CommunityData.EnhancedForumTopic> GetTopicsByCategory(CommunityData.ForumCategory category, int limit = 20)
        {
            if (!_topicsByCategory.TryGetValue(category, out var topics))
            {
                return new List<CommunityData.EnhancedForumTopic>();
            }
            return topics.OrderByDescending(t => t.LastReplyDate).Take(limit).ToList();
        }
        
        public void AddReputation(CommunityData.ReputationType type, int points, string reason = "")
        {
            AddReputationToPlayer(_currentPlayerProfile?.PlayerId, type, points, reason);
        }
        
        public void AddReputationToPlayer(string playerId, CommunityData.ReputationType type, int points, string reason = "")
        {
            if (!_enableReputationSystem || string.IsNullOrEmpty(playerId)) return;
            if (!_playerReputations.TryGetValue(playerId, out var reputation))
            {
                reputation = new CommunityData.ReputationSystem(playerId);
                _playerReputations[playerId] = reputation;
            }
            reputation.AddReputation(type, points, reason);
            CheckReputationLevelUp(reputation);
            if (playerId == _currentPlayerProfile?.PlayerId)
            {
                OnReputationChanged?.Invoke(reputation.GetTotalReputation());
                _onReputationChanged?.Raise();
            }
            LogInfo($"Added {points} reputation to {playerId} for {type} ({reason})");
        }
        
        public CommunityData.ReputationSystem GetPlayerReputation(string playerId = null)
        {
            playerId ??= _currentPlayerProfile?.PlayerId;
            if (string.IsNullOrEmpty(playerId)) return null;
            return _playerReputations.TryGetValue(playerId, out var reputation) ? reputation : null;
        }
        
        public string CreateCommunityEvent(string name, string description, DateTime startDate, DateTime endDate, string eventType = "Challenge")
        {
            if (!_enableCommunityEvents) return null;
            var newEvent = new CommunityData.EnhancedCommunityEvent(
                Guid.NewGuid().ToString(), name, description, eventType, startDate, endDate
            );
            _activeEvents.Add(newEvent);
            OnEventStarted?.Invoke(newEvent);
            _onCommunityEventStarted?.Raise();
            LogInfo($"Community event created: {name}");
            return newEvent.Id;
        }
        
        public bool JoinEvent(string eventId)
        {
            if (!_enableCommunityEvents || _currentPlayerProfile == null) return false;
            var communityEvent = _activeEvents.FirstOrDefault(e => e.Id == eventId);
            if (communityEvent == null)
            {
                LogWarning($"Event not found: {eventId}");
                return false;
            }
            if (!MeetsEventRequirements(communityEvent.Requirements))
            {
                LogWarning("Player does not meet event requirements.");
                return false;
            }
            var participant = new CommunityData.EventParticipant(_currentPlayerProfile.PlayerId, _currentPlayerProfile.DisplayName, eventId);
            communityEvent.AddParticipant(participant);
            _activeParticipants[participant.PlayerId] = participant;
            LogInfo($"Player {_currentPlayerProfile.DisplayName} joined event {communityEvent.Name}");
            return true;
        }
        
        public bool SubmitEventScore(string eventId, string scoreCategory, float score)
        {
            if (!_enableCommunityEvents || _currentPlayerProfile == null) return false;
            var communityEvent = _activeEvents.FirstOrDefault(e => e.Id == eventId);
            if (communityEvent == null) return false;
            var participant = communityEvent.Participants.FirstOrDefault(p => p.PlayerId == _currentPlayerProfile.PlayerId);
            if (participant == null)
            {
                LogWarning("Player not participating in this event.");
                return false;
            }
            participant.UpdateScore(scoreCategory, score);
            UpdateEventLeaderboard(communityEvent);
            LogInfo($"Submitted score {score} for event {communityEvent.Name} in category {scoreCategory}");
            return true;
        }
        
        public void AwardBadge(string badgeId, string reason = "")
        {
            if (_currentPlayerProfile == null) return;
            if (!_availableBadges.TryGetValue(badgeId, out var badge))
            {
                LogWarning($"Badge not found: {badgeId}");
                return;
            }
            if (_currentPlayerProfile.EarnedBadges.Any(b => b.Id == badgeId))
            {
                LogInfo($"Player already has badge: {badge.Name}");
                return;
            }
            _currentPlayerProfile.EarnedBadges.Add(badge);
            AddReputation(CommunityData.ReputationType.Achievement, badge.ReputationValue, $"Earned Badge: {badge.Name}");
            OnBadgeEarned?.Invoke(badge);
            LogInfo($"Awarded badge '{badge.Name}' to player. Reason: {reason}");
        }
        
        public void CheckAutomaticBadges() {}
        
        private void InitializePlayerProfile()
        {
            _currentPlayerProfile = new PlayerProfile
            {
                PlayerId = "player_001", DisplayName = "Chimera_Master", Level = 1, Experience = 0,
                Status = PlayerStatus.Online, LastActive = DateTime.Now,
                EarnedBadges = new List<CommunityData.Badge>()
            };
            _playerReputations[_currentPlayerProfile.PlayerId] = new CommunityData.ReputationSystem(_currentPlayerProfile.PlayerId);
            _lastPlayerActivity[_currentPlayerProfile.PlayerId] = DateTime.Now;
        }
        
        private void InitializeLeaderboards()
        {
            // Leaderboards are always enabled in this implementation
            foreach (ProjectChimera.Core.LeaderboardType type in Enum.GetValues(typeof(ProjectChimera.Core.LeaderboardType)))
            {
                var settings = new LeaderboardSettings(
                    GetLeaderboardDisplayName(type), GetLeaderboardDescription(type), GetLeaderboardCategory(type),
                    GetLeaderboardSortOrder(type), 0, true
                );
                var leaderboard = new CommunityData.EnhancedLeaderboard(type.ToString(), type, settings);
                _leaderboards.Add(leaderboard.Id, leaderboard);
            }
        }
        
        private void InitializeForumSystem()
        {
            if (!_enableForum) return;
            foreach (CommunityData.ForumCategory category in Enum.GetValues(typeof(CommunityData.ForumCategory)))
            {
                _topicsByCategory[category] = new List<CommunityData.EnhancedForumTopic>();
            }
            CreateForumTopic("Welcome to the Chimera Forums!", "This is the place to discuss all things Project Chimera.", CommunityData.ForumCategory.General);
            CreateForumTopic("Show off your best strains!", "Post your greatest genetic creations here.", CommunityData.ForumCategory.Genetics);
        }
        
        private void InitializeReputationSystem()
        {
            if (!_enableReputationSystem) return;
            if (_currentPlayerProfile != null && !_playerReputations.ContainsKey(_currentPlayerProfile.PlayerId))
            {
                _playerReputations[_currentPlayerProfile.PlayerId] = new CommunityData.ReputationSystem(_currentPlayerProfile.PlayerId);
            }
        }
        
        private void InitializeCommunityEvents() {}
        
        private void InitializeBadgeSystem()
        {
            if (_currentPlayerProfile == null) return;
            var badge1 = new CommunityData.Badge("badge_first_post", "First Post", "Made your first post on the forums.", 10, "icon_first_post");
            var badge2 = new CommunityData.Badge("badge_top_10", "Top 10", "Reached the top 10 on any leaderboard.", 100, "icon_top_10");
            var badge3 = new CommunityData.Badge("badge_master_breeder", "Master Breeder", "Created a strain with 3+ maxed out traits.", 250, "icon_master_breeder");
            _availableBadges.Add(badge1.Id, badge1);
            _availableBadges.Add(badge2.Id, badge2);
            _availableBadges.Add(badge3.Id, badge3);
        }
        
        private void UpdateAllLeaderboards()
        {
            foreach (var leaderboard in _leaderboards.Values)
            {
                leaderboard.UpdateRanks();
                OnLeaderboardUpdated?.Invoke(leaderboard);
            }
            _onLeaderboardUpdated?.Raise();
            LogInfo("All leaderboards updated.");
        }
        
        private void UpdateCommunityEvents()
        {
            var now = DateTime.Now;
            var eventsToRemove = new List<CommunityData.EnhancedCommunityEvent>();
            foreach (var communityEvent in _activeEvents)
            {
                communityEvent.UpdateStatus(now);
                if (communityEvent.Status == CommunityData.EventStatus.Active.ToString()) { UpdateEventProgress(communityEvent); }
                else if (communityEvent.Status == CommunityData.EventStatus.Finished.ToString()) { eventsToRemove.Add(communityEvent); }
            }
            foreach (var toRemove in eventsToRemove) { _activeEvents.Remove(toRemove); }
        }
        
        private void UpdateEventProgress(CommunityData.EnhancedCommunityEvent communityEvent) {}

        private void UpdateEventLeaderboard(CommunityData.EnhancedCommunityEvent communityEvent)
        {
            if (communityEvent.Leaderboard == null) return;
            communityEvent.Leaderboard.UpdateRanks();
        }
        
        private void UpdatePlayerActivity()
        {
            if (_currentPlayerProfile != null && _currentPlayerProfile.Status == PlayerStatus.Online)
            {
                _lastPlayerActivity[_currentPlayerProfile.PlayerId] = DateTime.Now;
            }
        }
        
        private void UpdateCommunityMetrics()
        {
            _communityMetrics.ActivePlayers = _lastPlayerActivity.Count(p => (DateTime.Now - p.Value).TotalMinutes < 15);
            _communityMetrics.TotalPosts = _forumPosts.Count;
            _communityMetrics.TotalTopics = _forumTopics.Count;
        }
        
        private bool MeetsEventRequirements(IEventRequirements requirements) { return true; }
        
        private void CheckReputationLevelUp(CommunityData.ReputationSystem reputation)
        {
            int oldLevel = reputation.ReputationLevel;
            reputation.UpdateReputationLevel();
            int newLevel = reputation.ReputationLevel;
            if (newLevel > oldLevel)
            {
                LogInfo($"Player {reputation.PlayerId} reached reputation level {newLevel}!");
                AwardBadge($"badge_rep_level_{newLevel}", $"Reached reputation level {newLevel}");
            }
        }
        
        private float CalculateCommunityHealthScore()
        {
            float score = 0;
            score += _communityMetrics.ActivePlayers * 0.5f;
            score += _communityMetrics.TotalPosts * 0.2f;
            score += _activeEvents.Count * 1.5f;
            return score;
        }
        
        private string GetLeaderboardDisplayName(ProjectChimera.Core.LeaderboardType type)
        {
            return type.ToString().Replace("_", " ");
        }
        
        private string GetLeaderboardDescription(ProjectChimera.Core.LeaderboardType type)
        {
            return $"Tracks the top players for {GetLeaderboardDisplayName(type)}.";
        }
        
        private LeaderboardCategory GetLeaderboardCategory(ProjectChimera.Core.LeaderboardType type)
        {
            switch (type)
            {
                case ProjectChimera.Core.LeaderboardType.Highest_Quality_Strain:
                case ProjectChimera.Core.LeaderboardType.Most_Unique_Genotypes:
                    return LeaderboardCategory.Cultivation;
                case ProjectChimera.Core.LeaderboardType.Highest_Profit:
                case ProjectChimera.Core.LeaderboardType.Most_Contracts_Completed:
                    return LeaderboardCategory.Economics;
                default:
                    return LeaderboardCategory.General;
            }
        }
        
        private ScoreOrder GetLeaderboardSortOrder(ProjectChimera.Core.LeaderboardType type)
        {
            return ScoreOrder.Descending;
        }
        
        private string FormatScore(float score, ProjectChimera.Core.LeaderboardType type)
        {
            switch (type)
            {
                case ProjectChimera.Core.LeaderboardType.Highest_Profit: return $"${score:N2}";
                case ProjectChimera.Core.LeaderboardType.Highest_Quality_Strain: return $"{score:F2}%";
                default: return score.ToString("N0");
            }
        }
        
        private string GetReputationLevelName(int level)
        {
            if (level >= 10) return "Community Pillar";
            if (level >= 5) return "Valued Member";
            if (level >= 2) return "Contributor";
            return "Newcomer";
        }
        
        private Color GetReputationLevelColor(int level)
        {
            if (level >= 10) return Color.cyan;
            if (level >= 5) return Color.green;
            if (level >= 2) return Color.yellow;
            return Color.white;
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo("CommunityManager shutting down.");
        }
    }
}