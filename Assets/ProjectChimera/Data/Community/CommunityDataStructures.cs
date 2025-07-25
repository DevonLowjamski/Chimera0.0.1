using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Community
{
    // NOTE: LeaderboardCategory, TimePeriod, and ScoreOrder are now defined in ProjectChimera.Core
    // and should be used from there to avoid type conflicts.

    [System.Serializable]
    public enum ChallengeTimingType
    {
        Daily,
        Weekly,
        Monthly,
        Seasonal,
        Special,
        Community,
        Speedrun,
        Quality,
        Innovation,
        Collaboration,
        Announcements
    }

    [System.Serializable]
    public enum ReputationTier
    {
        Novice,
        Apprentice,
        Cultivator,
        Expert,
        Master,
        Legend
    }

    [System.Serializable]
    public enum ForumCategory
    {
        General,
        Genetics,
        Cultivation,
        Equipment,
        Automation,
        Trading,
        Challenges,
        Showcase,
        Troubleshooting,
        Research,
        Community,
        Announcements
    }

    // Enhanced Forum System Enums
    [System.Serializable]
    public enum TopicType
    {
        Discussion,
        Question,
        Guide,
        Showcase,
        Bug_Report,
        Feature_Request,
        Announcement
    }

    [System.Serializable]
    public enum TopicStatus
    {
        Open,
        Closed,
        Archived,
        Deleted
    }

    [System.Serializable]
    public enum PostStatus
    {
        Published,
        Moderated,
        Hidden,
        Deleted
    }

    [System.Serializable]
    public enum VoteType
    {
        None,
        UpVote,
        DownVote
    }
    
    [System.Serializable]
    public class Vote
    {
        public string VoteId;
        public string UserId;
        public VoteType Type;
        public DateTime VoteDate;
        public string TargetId; // Post ID, Topic ID, etc.
        
        public Vote(string userId, VoteType type)
        {
            VoteId = System.Guid.NewGuid().ToString();
            UserId = userId;
            Type = type;
            VoteDate = DateTime.Now;
        }
    }

    [System.Serializable]
    public enum AttachmentType
    {
        Image,
        Video,
        Document,
        Save_File
    }

    // Reputation System Enums
    [System.Serializable]
    public enum ReputationType
    {
        Helpful_Posts,
        Quality_Content,
        Community_Participation,
        Event_Performance,
        Mentorship,
        Bug_Reports,
        Feedback,
        Moderation_Help,
        Forum_Activity,
        Achievement
    }

    [System.Serializable]
    public enum PrivacyLevel
    {
        Public,
        Friends,
        Private
    }

    [System.Serializable]
    public enum EventStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Finished, // Alias for Completed to match CommunityManager usage
        Ended,
        Cancelled
    }

    // Note: PlayerProfile, PlayerStatus, and LeaderboardType are now defined in SharedDataStructures.cs

    [System.Serializable]
    public class LeaderboardEntry
    {
        [Header("Entry Details")]
        public string PlayerID;
        public string DisplayName;
        public float Score;
        public int Rank;
        public DateTime SubmissionDate;
        public string AdditionalData; // JSON for extra info
        public bool IsVerified;
        public int VoteCount;
        public string ProofImageURL;
        public string ModerationNotes;
    }

    [System.Serializable]
    public class Leaderboard
    {
        [Header("Leaderboard Configuration")]
        public string LeaderboardID;
        public LeaderboardType Type;
        public string Name;
        public string Description;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public int MaxEntries;
        public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
        
        [Header("Requirements")]
        public int MinimumPlayerLevel;
        public List<string> RequiredAchievements = new List<string>();
        public bool RequiresVerification;
    }

    [System.Serializable]
    public class Challenge
    {
        [Header("Challenge Details")]
        public string ChallengeID;
        public string Name;
        public string Description;
        public ChallengeTimingType Type;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        
        [Header("Requirements & Rules")]
        public int MinimumPlayerLevel;
        public List<string> Requirements = new List<string>();
        public string Rules;
        
        [Header("Rewards & Recognition")]
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
        public List<string> ParticipantIDs = new List<string>();
        public List<LeaderboardEntry> Submissions = new List<LeaderboardEntry>();
        
        [Header("Community Features")]
        public int ParticipantCount;
        public float AverageScore;
        public string CreatedBy; // For community challenges
        public bool RequiresApproval;
    }

    [System.Serializable]
    public class ForumPost
    {
        [Header("Post Information")]
        public string PostID;
        public string ThreadID;
        public string AuthorID;
        public string Title;
        public string Content;
        public DateTime CreatedDate;
        public DateTime LastModified;
        
        [Header("Engagement")]
        public int UpVotes;
        public int Likes;
        public int Replies;
        public List<string> Tags = new List<string>();
        public ForumCategory Category;
        public List<Vote> Votes = new List<Vote>();
        
        [Header("Media & Attachments")]
        public List<string> ImageURLs = new List<string>();
        public List<string> AttachmentURLs = new List<string>();
        
        [Header("Moderation")]
        public bool IsApproved;
        public bool IsPinned;
        public bool IsDeleted;
        public string ModerationNotes;
        
        [Header("Helpfulness")]
        public bool IsMarkedSolution;
        public float QualityScore;
        
        // Methods
        public void AddVote(Vote vote)
        {
            // Remove existing vote from this user if any
            Votes.RemoveAll(v => v.UserId == vote.UserId);
            
            // Add new vote
            vote.TargetId = PostID;
            Votes.Add(vote);
            
            // Update vote counts
            UpVotes = Votes.Count(v => v.Type == VoteType.UpVote);
            var downVotes = Votes.Count(v => v.Type == VoteType.DownVote);
        }
    }

    [System.Serializable]
    public class ForumThread
    {
        [Header("Thread Information")]
        public string ThreadID;
        public string OriginalPostID;
        public ForumCategory Category;
        public List<string> PostIDs = new List<string>();
        
        [Header("Activity")]
        public DateTime CreatedDate;
        public int TotalViews;
        public int TotalReplies;
        public DateTime LastReplyDate;
        public string LastReplyAuthor;
        
        [Header("Status")]
        public bool IsActive;
        public bool IsPinned;
        public bool IsSticky;
        public bool HasSolution;
    }

    [System.Serializable]
    public class CommunityEvent
    {
        [Header("Event Details")]
        public string EventID;
        public string Name;
        public string Description;
        public DateTime StartDate;
        public DateTime EndDate;
        public string EventType; // Contest, Collaboration, Research, etc.
        
        [Header("Participation")]
        public List<string> ParticipantIDs = new List<string>();
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
        
        [Header("Progress Tracking")]
        public Dictionary<string, float> ParticipantProgress = new Dictionary<string, float>();
        public Dictionary<string, float> IndividualProgress = new Dictionary<string, float>();
        public bool IsCompleted;
    }

    [System.Serializable]
    public class Badge
    {
        [Header("Badge Information")]
        public string BadgeID;
        public string Name;
        public string Description;
        public string IconURL;
        public string Category;
        
        [Header("Requirements")]
        public List<string> Requirements = new List<string>();
        public bool IsHidden; // Secret badges
        public int Rarity; // 1-5 stars
        public int ReputationValue; // Reputation points awarded for earning this badge
        
        [Header("Statistics")]
        public int TotalAwarded;
        public DateTime FirstAwarded;
        public List<string> HolderIDs = new List<string>();
        
        // Constructor for easy Badge creation
        public Badge(string id, string name, string description, int reputationValue, string iconUrl = "")
        {
            BadgeID = id;
            Name = name;
            Description = description;
            ReputationValue = reputationValue;
            IconURL = iconUrl;
            Requirements = new List<string>();
            HolderIDs = new List<string>();
            TotalAwarded = 0;
        }
        
        // Property alias for compatibility
        public string Id => BadgeID;
    }

    [System.Serializable]
    public class TradeReputation
    {
        [Header("Trading Statistics")]
        public string PlayerID;
        public int TotalTrades;
        public int SuccessfulTrades;
        public int DisputedTrades;
        public float AverageRating;
        
        [Header("Trade Categories")]
        public Dictionary<string, int> TradesByCategory = new Dictionary<string, int>();
        public Dictionary<string, float> CategoryRatings = new Dictionary<string, float>();
        
        [Header("Trust Metrics")]
        public float TrustScore;
        public List<string> TrustedByPlayerIDs = new List<string>();
        public List<string> PositiveFeedback = new List<string>();
        public List<string> NegativeFeedback = new List<string>();
        
        [Header("Recent Activity")]
        public DateTime LastTradeDate;
        public List<string> RecentTradeIDs = new List<string>();
        public float RecentPerformance; // Last 30 days
    }

    [System.Serializable]
    public class CommunityMetrics
    {
        [Header("Overall Statistics")]
        public int TotalPlayers;
        public int ActivePlayers;
        public int TotalPosts;
        public int TotalTopics;
        public int TotalForumPosts;
        public int TotalChallengesCompleted;
        public int TotalTradesCompleted;
        public DateTime LastUpdated;
        
        [Header("Engagement Metrics")]
        public int ActivePlayersDaily;
        public float AverageSessionLength;
        public float CommunityHealthScore;
        
        [Header("Content Statistics")]
        public Dictionary<string, int> PostsByCategory = new Dictionary<string, int>();
        public Dictionary<string, int> ChallengesByType = new Dictionary<string, int>();
        public Dictionary<string, float> LeaderboardDistribution = new Dictionary<string, float>();
        
        [Header("Growth Metrics")]
        public int NewPlayersThisWeek;
        public float RetentionRate;
        public float EngagementGrowth;
    }

    // ENHANCED FORUM SYSTEM CLASSES
    [System.Serializable]
    public class EnhancedForumTopic
    {
        [Header("Topic Information")]
        public string TopicId;
        public string Title;
        public string AuthorId;
        public string AuthorName;
        public DateTime CreatedDate;
        public DateTime LastReplyDate;
        public TopicStatus Status;
        public TopicType Type;
        
        [Header("Engagement")]
        public int ViewCount;
        public int ReplyCount;
        public bool IsSticky;
        public bool IsLocked;
        public List<string> Tags = new List<string>();
        public TopicRating Rating;
        
        [Header("Posts")]
        public List<EnhancedForumPost> Posts = new List<EnhancedForumPost>();
        
        // Constructor for CommunityManager
        public EnhancedForumTopic(string title, string authorId, ForumCategory category, TopicType type)
        {
            TopicId = System.Guid.NewGuid().ToString();
            Title = title;
            AuthorId = authorId;
            CreatedDate = DateTime.Now;
            LastReplyDate = DateTime.Now;
            Status = TopicStatus.Open;
            Type = type;
            ViewCount = 0;
            ReplyCount = 0;
            IsSticky = false;
            IsLocked = false;
            Tags = new List<string>();
            Posts = new List<EnhancedForumPost>();
            Rating = new TopicRating();
        }
        
        // Missing methods for CommunityManager
        public void AddReply(EnhancedForumPost post)
        {
            Posts.Add(post);
            ReplyCount = Posts.Count;
            LastReplyDate = DateTime.Now;
        }
    }

    [System.Serializable]
    public class EnhancedForumPost
    {
        [Header("Post Information")]
        public string PostId;
        public string TopicId;
        public string AuthorId;
        public string AuthorName;
        public string Content;
        public DateTime PostDate;
        public DateTime LastEditDate;
        public string EditedBy;
        
        [Header("Attachments and Media")]
        public List<ForumAttachment> Attachments = new List<ForumAttachment>();
        
        [Header("Voting and Interaction")]
        public PostVoting Voting;
        public List<ForumReply> Replies = new List<ForumReply>();
        
        [Header("Moderation")]
        public PostStatus Status;
        public bool IsDeleted;
        public string DeleteReason;
        
        // Constructor for CommunityManager
        public EnhancedForumPost(string topicId, string authorId, string content)
        {
            PostId = System.Guid.NewGuid().ToString();
            TopicId = topicId;
            AuthorId = authorId;
            Content = content;
            PostDate = DateTime.Now;
            LastEditDate = DateTime.Now;
            Status = PostStatus.Published;
            IsDeleted = false;
            Attachments = new List<ForumAttachment>();
            Voting = new PostVoting();
            Replies = new List<ForumReply>();
        }
        
        // Missing methods for CommunityManager
        public void AddVote(Vote vote)
        {
            if (Voting == null) Voting = new PostVoting();
            
            // Remove existing vote from this user if any
            var existingVote = Voting.VotedUserIds.FirstOrDefault(id => id == vote.UserId);
            if (existingVote != null)
            {
                Voting.VotedUserIds.Remove(existingVote);
                // Adjust vote counts based on previous vote
                if (Voting.UserVoteType == VoteType.UpVote)
                    Voting.UpVotes--;
                else if (Voting.UserVoteType == VoteType.DownVote)
                    Voting.DownVotes--;
            }
            
            // Add new vote
            Voting.VotedUserIds.Add(vote.UserId);
            Voting.UserVoteType = vote.Type;
            Voting.HasUserVoted = true;
            
            if (vote.Type == VoteType.UpVote)
                Voting.UpVotes++;
            else if (vote.Type == VoteType.DownVote)
                Voting.DownVotes++;
                
            Voting.NetScore = Voting.UpVotes - Voting.DownVotes;
        }
    }

    [System.Serializable]
    public class ForumReply
    {
        public string ReplyId;
        public string ParentPostId;
        public string AuthorId;
        public string AuthorName;
        public string Content;
        public DateTime ReplyDate;
        public PostVoting Voting;
        public bool IsDeleted;
    }

    [System.Serializable]
    public class ForumAttachment
    {
        public string AttachmentId;
        public string FileName;
        public string FilePath;
        public AttachmentType Type;
        public long FileSize;
        public DateTime UploadDate;
        public string Description;
    }

    [System.Serializable]
    public class PostVoting
    {
        public int UpVotes;
        public int DownVotes;
        public int NetScore;
        public List<string> VotedUserIds = new List<string>();
        public bool HasUserVoted;
        public VoteType UserVoteType;
    }

    [System.Serializable]
    public class TopicRating
    {
        public float AverageRating;
        public int TotalRatings;
        public Dictionary<int, int> RatingDistribution = new Dictionary<int, int>();
        public bool HasUserRated;
        public int UserRating;
    }

    // ENHANCED LEADERBOARD SYSTEM CLASSES
    [System.Serializable]
    public class LeaderboardSettings : ILeaderboardSettings
    {
        [SerializeField] private ProjectChimera.Core.ScoreOrder _sortOrder;
        [SerializeField] private float _minimumScore;
        [SerializeField] private bool _requireVerification;

        // Constructor for CommunityManager
        public LeaderboardSettings(string name, string description, LeaderboardCategory category, ProjectChimera.Core.ScoreOrder sortOrder, float minimumScore, bool requireVerification)
        {
            _sortOrder = sortOrder;
            _minimumScore = minimumScore;
            _requireVerification = requireVerification;
        }

        public ProjectChimera.Core.ScoreOrder SortOrder => _sortOrder;
        public float MinimumScore => _minimumScore;
        public bool RequireVerification => _requireVerification;
    }

    [System.Serializable]
    public class EnhancedLeaderboard : ILeaderboard
    {
        [Header("Leaderboard Configuration")]
        [SerializeField] private string _leaderboardId;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ProjectChimera.Core.LeaderboardType _type;
        [SerializeField] private ProjectChimera.Core.LeaderboardCategory _category;
        [SerializeField] private ProjectChimera.Core.TimePeriod _period;
        [SerializeField] private bool _isActive;
        [SerializeField] private int _maxEntries;
        [SerializeField] private LeaderboardSettings _settings;

        [Header("State")]
        [SerializeField] private List<EnhancedLeaderboardEntry> _entries = new List<EnhancedLeaderboardEntry>();
        [SerializeField] private DateTime _lastUpdated;
        
        // Constructor for CommunityManager
        public EnhancedLeaderboard(string leaderboardId, ProjectChimera.Core.LeaderboardType type, LeaderboardSettings settings)
        {
            _leaderboardId = leaderboardId;
            _name = type.ToString().Replace("_", " ");
            _description = $"Tracks the top players for {_name}.";
            _type = type;
            _category = ProjectChimera.Core.LeaderboardCategory.General;
            _period = ProjectChimera.Core.TimePeriod.All_Time;
            _isActive = true;
            _maxEntries = 1000;
            _settings = settings;
            _entries = new List<EnhancedLeaderboardEntry>();
            _lastUpdated = DateTime.Now;
        }
        
        // Interface Implementation
        public string Id => _leaderboardId;
        public string DisplayName => _name;
        public string LeaderboardDescription => _description;
        public ProjectChimera.Core.LeaderboardType LeaderboardType => _type;
        public ProjectChimera.Core.LeaderboardCategory LeaderboardCategory => _category;
        public ProjectChimera.Core.TimePeriod TimePeriod => _period;
        public DateTime LastUpdateTime => _lastUpdated;
        public bool Active => _isActive;
        public int MaximumEntries => _maxEntries;
        public ILeaderboardSettings Settings => _settings;
        public IReadOnlyList<ILeaderboardEntry> LeaderboardEntries => _entries;

        // Public Methods
        public EnhancedLeaderboardEntry GetPlayerEntry(string playerId)
        {
            return _entries.FirstOrDefault(e => e.PlayerId == playerId);
        }

        public List<EnhancedLeaderboardEntry> GetTopEntries(int count = 10)
        {
            return _entries.Take(count).ToList();
        }

        public void AddOrUpdateEntry(EnhancedLeaderboardEntry newEntry)
        {
            var existingEntry = _entries.FirstOrDefault(e => e.PlayerId == newEntry.PlayerId);
            if (existingEntry != null)
            {
                bool isBetter = _settings.SortOrder == ProjectChimera.Core.ScoreOrder.Descending
                    ? newEntry.EntryScore > existingEntry.EntryScore
                    : newEntry.EntryScore < existingEntry.EntryScore;

                if (isBetter)
                {
                    _entries.Remove(existingEntry);
                    _entries.Add(newEntry);
                }
            }
            else
            {
                _entries.Add(newEntry);
            }
            SortAndTrimEntries();
        }
        
        private void SortAndTrimEntries()
        {
            if (_settings.SortOrder == ProjectChimera.Core.ScoreOrder.Descending)
            {
                _entries = _entries.OrderByDescending(e => e.EntryScore).ToList();
            }
            else
            {
                _entries = _entries.OrderBy(e => e.EntryScore).ToList();
            }

            // Update ranks
            for (int i = 0; i < _entries.Count; i++)
            {
                _entries[i].Rank = i + 1;
            }

            // Trim to max entries
            if (_entries.Count > _maxEntries)
            {
                _entries = _entries.Take(_maxEntries).ToList();
            }

            _lastUpdated = DateTime.Now;
        }
        
        // Missing method for CommunityManager
        public void UpdateRanks()
        {
            SortAndTrimEntries();
        }
    }

    [System.Serializable]
    public class EnhancedLeaderboardEntry : ILeaderboardEntry
    {
        [SerializeField] private string _entryId;
        [SerializeField] private string _playerId;
        [SerializeField] private string _playerName;
        [SerializeField] private float _score;
        [SerializeField] private int _rank;
        [SerializeField] private DateTime _submissionDate;
        
        public EnhancedLeaderboardEntry(string playerId, string playerName, float score, DateTime submissionDate)
        {
            _entryId = Guid.NewGuid().ToString();
            _playerId = playerId;
            _playerName = playerName;
            _score = score;
            _submissionDate = submissionDate;
            _rank = -1; // Initial rank
        }
        
        // Interface Implementation
        public string Id => _entryId;
        public string PlayerId => _playerId;
        public string DisplayName => _playerName;
        public float EntryScore => _score;
        public int EntryRank => _rank;
        public DateTime LastUpdateTime => _submissionDate;
        
        // Public properties for implementation logic
        public int Rank { get => _rank; set => _rank = value; }
        public string FormattedScore { get; set; }
    }
    
    [System.Serializable]
    public class LeaderboardEntryData
    {
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
        public string ScreenshotPath;
        public string Description;
        public List<string> Tags = new List<string>();
    }

    // REPUTATION SYSTEM CLASSES
    [System.Serializable]
    public class ReputationSystem
    {
        public string PlayerId;
        public int TotalReputation;
        public ReputationLevel Level;
        public List<ReputationSource> Sources = new List<ReputationSource>();
        public ReputationHistory History;
        public List<ReputationBonus> ActiveBonuses = new List<ReputationBonus>();
        public DateTime LastUpdated;
        
        // Property for CommunityManager
        public int ReputationLevel => Level?.Level ?? 1;
        
        // Constructor for CommunityManager
        public ReputationSystem(string playerId)
        {
            PlayerId = playerId;
            TotalReputation = 0;
            Level = new ReputationLevel { Level = 1, Name = "Novice", RequiredReputation = 0 };
            Sources = new List<ReputationSource>();
            History = new ReputationHistory();
            ActiveBonuses = new List<ReputationBonus>();
            LastUpdated = DateTime.Now;
        }

        // Missing methods for CommunityManager
        public void AddReputation(ReputationType type, int points, string reason = "")
        {
            var source = new ReputationSource
            {
                Type = type,
                Points = points,
                EarnedDate = DateTime.Now,
                Description = reason,
                SourceId = System.Guid.NewGuid().ToString()
            };
            
            Sources.Add(source);
            TotalReputation += points;
            
            if (History == null) History = new ReputationHistory();
            var change = new ReputationChange
            {
                Date = DateTime.Now,
                Type = type,
                PointsChanged = points,
                NewTotal = TotalReputation,
                Reason = reason,
                SourceId = source.SourceId
            };
            History.Changes.Add(change);
            
            if (!History.TotalByType.ContainsKey(type))
                History.TotalByType[type] = 0;
            History.TotalByType[type] += points;
            
            LastUpdated = DateTime.Now;
        }

        public int GetTotalReputation()
        {
            return TotalReputation;
        }
        
        public void UpdateReputationLevel()
        {
            int newLevel = 1;
            if (TotalReputation >= 1000) newLevel = 6;
            else if (TotalReputation >= 500) newLevel = 5;
            else if (TotalReputation >= 250) newLevel = 4;
            else if (TotalReputation >= 100) newLevel = 3;
            else if (TotalReputation >= 50) newLevel = 2;
            
            if (Level == null) Level = new ReputationLevel();
            Level.Level = newLevel;
            Level.Name = GetReputationLevelName(newLevel);
            Level.RequiredReputation = GetRequiredReputationForLevel(newLevel);
        }
        
        private string GetReputationLevelName(int level)
        {
            switch (level)
            {
                case 1: return "Novice";
                case 2: return "Apprentice";
                case 3: return "Cultivator";
                case 4: return "Expert";
                case 5: return "Master";
                case 6: return "Legend";
                default: return "Novice";
            }
        }
        
        private int GetRequiredReputationForLevel(int level)
        {
            switch (level)
            {
                case 1: return 0;
                case 2: return 50;
                case 3: return 100;
                case 4: return 250;
                case 5: return 500;
                case 6: return 1000;
                default: return 0;
            }
        }
    }

    [System.Serializable]
    public class ReputationSource
    {
        public ReputationType Type;
        public int Points;
        public DateTime EarnedDate;
        public string Description;
        public string SourceId;
    }

    [System.Serializable]
    public class ReputationHistory
    {
        public List<ReputationChange> Changes = new List<ReputationChange>();
        public Dictionary<ReputationType, int> TotalByType = new Dictionary<ReputationType, int>();
        public int WeeklyGain;
        public int MonthlyGain;
        public float AverageWeeklyGain;
    }

    [System.Serializable]
    public class ReputationChange
    {
        public DateTime Date;
        public ReputationType Type;
        public int PointsChanged;
        public int NewTotal;
        public string Reason;
        public string SourceId;
    }

    [System.Serializable]
    public class ReputationBonus
    {
        public string BonusId;
        public string Name;
        public float Multiplier;
        public DateTime ExpiryDate;
        public ReputationType AppliesTo;
        public string Description;
    }

    [System.Serializable]
    public class ReputationLevel
    {
        public int Level;
        public string Name;
        public string Description;
        public int RequiredReputation;
        public Color LevelColor;
        public string IconPath;
        public List<ReputationPerk> Perks = new List<ReputationPerk>();
    }

    [System.Serializable]
    public class ReputationPerk
    {
        public string Name;
        public string Description;
        public string Type;
        public object PerkData;
        public bool IsActive;
    }

    // ENHANCED COMMUNITY EVENT SYSTEM CLASSES
    [System.Serializable]
    public class EnhancedCommunityEvent : ICommunityEvent
    {
        [SerializeField] private string _eventId;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private DateTime _startDate;
        [SerializeField] private DateTime _endDate;
        [SerializeField] private string _status;
        [SerializeField] private List<EventMilestone> _milestones = new List<EventMilestone>();
        [SerializeField] private List<EventParticipant> _participants = new List<EventParticipant>();
        [SerializeField] private EventRewards _rewards;
        [SerializeField] private EventRequirements _requirements;
        [SerializeField] private EnhancedLeaderboard _leaderboard;
        
        // Constructor for CommunityManager
        public EnhancedCommunityEvent(string eventId, string name, string description, string eventType, DateTime startDate, DateTime endDate)
        {
            _eventId = eventId;
            _name = name;
            _description = description;
            _startDate = startDate;
            _endDate = endDate;
            _status = EventStatus.NotStarted.ToString();
            _milestones = new List<EventMilestone>();
            _participants = new List<EventParticipant>();
            _rewards = new EventRewards();
            _requirements = new EventRequirements();
            
            // Create event leaderboard
            var settings = new LeaderboardSettings(name, description, LeaderboardCategory.Events, ProjectChimera.Core.ScoreOrder.Descending, 0, false);
            _leaderboard = new EnhancedLeaderboard($"event_{eventId}", ProjectChimera.Core.LeaderboardType.Event_Score, settings);
        }

        // Interface Implementation
        public string Id => _eventId;
        public string DisplayName => _name;
        public string EventDescription => _description;
        public DateTime StartDate => _startDate;
        public DateTime EndDate => _endDate;
        public string Status => _status;
        public IReadOnlyList<IEventMilestone> Milestones => _milestones;
        public IReadOnlyList<IEventParticipant> Participants => _participants;
        public IEventRewards Rewards => _rewards;
        public IEventRequirements Requirements => _requirements;
        
        // Additional property for CommunityManager
        public EnhancedLeaderboard Leaderboard => _leaderboard;

        public bool HasParticipant(string playerId)
        {
            return _participants.Any(p => p.PlayerId == playerId);
        }

        public void AddParticipant(EventParticipant participant)
        {
            if (!HasParticipant(participant.PlayerId))
            {
                _participants.Add(participant);
            }
        }
        
        // Missing properties and methods for CommunityManager
        public string Name => _name;
        
        public void UpdateStatus(DateTime currentTime)
        {
            if (currentTime < _startDate)
            {
                _status = "Upcoming";
            }
            else if (currentTime >= _startDate && currentTime <= _endDate)
            {
                _status = "Active";
            }
            else if (currentTime > _endDate)
            {
                _status = "Completed";
            }
        }
    }

    [System.Serializable]
    public class EventParticipant : IEventParticipant
    {
        [SerializeField] private string _participantId;
        [SerializeField] private string _playerName;
        [SerializeField] private DateTime _joinDate;
        [SerializeField] private bool _isActive;
        [SerializeField] private EventParticipationData _data;

        public EventParticipant(string participantId, string playerName, string eventId)
        {
            _participantId = participantId;
            _playerName = playerName;
            _joinDate = DateTime.Now;
            _isActive = true;
            _data = new EventParticipationData { EventId = eventId };
        }
        
        public string Id => _participantId;
        public string PlayerId => _participantId;
        public string DisplayName => _playerName;
        public DateTime ParticipationDate => _joinDate;
        public IEventParticipationData ParticipationData => _data;
        public bool Active => _isActive;
        
        // Missing method for CommunityManager
        public void UpdateScore(string scoreCategory, float score)
        {
            if (_data.ScoresByCategory == null)
                _data.ScoresByCategory = new Dictionary<string, float>();
                
            _data.ScoresByCategory[scoreCategory] = score;
            _data.Score = _data.ScoresByCategory.Values.Sum();
        }
    }

    [System.Serializable]
    public class EventParticipationData : IEventParticipationData
    {
        public string EventId { get; set; }
        public float Progress { get; set; }
        public float Score { get; set; }
        public List<string> Achievements { get; set; } = new List<string>();
        public List<string> CompletedMilestones { get; set; } = new List<string>();
        public Dictionary<string, float> ScoresByCategory { get; set; } = new Dictionary<string, float>();
    }

    [System.Serializable]
    public class EventRewards : IEventRewards
    {
        [SerializeField] private List<EventReward> _rewards;
        public IReadOnlyList<IEventReward> Rewards => _rewards;
    }

    [System.Serializable]
    public class EventReward : IEventReward
    {
        [SerializeField] private string _rewardId;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        
        public string Id => _rewardId;
        public string DisplayName => _name;
        public string RewardDescription => _description;
    }

    [System.Serializable]
    public class EventRequirements : IEventRequirements
    {
        [SerializeField] private int _minimumLevel;
        [SerializeField] private int _minimumReputation;

        public int MinimumLevel => _minimumLevel;
        public int MinimumReputation => _minimumReputation;
    }
    
    [System.Serializable]
    public class EventMilestone : IEventMilestone
    {
        [SerializeField] private string _milestoneId;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private float _requiredProgress;
        [SerializeField] private EventReward _reward;

        public string Id => _milestoneId;
        public string DisplayName => _name;
        public string MilestoneDescription => _description;
        public float RequiredProgress => _requiredProgress;
        public IEventReward Reward => _reward;
    }
}