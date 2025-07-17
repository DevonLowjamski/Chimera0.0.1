using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Community
{
    /// <summary>
    /// Community gaming data structures for social interaction and collaborative gameplay
    /// Focuses on player-to-player interaction, trading, mentorship, and community competitions
    /// Designed to create engaging social experiences and collaborative cultivation challenges
    /// </summary>
    
    #region Player Social Data
    
    [System.Serializable]
    public class CommunityPlayerProfile
    {
        public string PlayerID;
        public string PlayerName;
        public string DisplayName;
        public int PlayerLevel;
        public float TotalExperience;
        public PlayerReputationLevel ReputationLevel;
        public float ReputationScore;
        public DateTime JoinDate;
        public DateTime LastActivity;
        public List<string> FriendIDs = new List<string>();
        public List<string> BlockedPlayerIDs = new List<string>();
        public List<string> FavoriteStrains = new List<string>();
        public List<string> Specializations = new List<string>();
        public CommunityAchievements Achievements;
        public SocialStatistics SocialStats;
        public PrivacySettings Privacy;
    }
    
    [System.Serializable]
    public class CommunityAchievements
    {
        public int TotalAchievements;
        public int RareAchievements;
        public int LegendaryAchievements;
        public List<string> FeaturedAchievements = new List<string>();
        public DateTime LastAchievementDate;
        public int AchievementStreak;
    }
    
    [System.Serializable]
    public class SocialStatistics
    {
        public int TotalTrades;
        public int SuccessfulTrades;
        public float TradeRating;
        public int MentoringSessions;
        public int StudentsHelped;
        public int CollaborativeProjects;
        public int CommunityContributions;
        public float HelpfulnessRating;
        public int ForumPosts;
        public int KnowledgeShares;
    }
    
    [System.Serializable]
    public class PrivacySettings
    {
        public bool ShowOnlineStatus;
        public bool AllowFriendRequests;
        public bool ShowFacilityTours;
        public bool ShareGrowthData;
        public bool ShowAchievements;
        public bool AllowTradeRequests;
        public bool ShowInLeaderboards;
        public bool AllowMentorRequests;
    }
    
    #endregion
    
    #region Trading System Data
    
    [System.Serializable]
    public class TradeOffer
    {
        public string TradeID;
        public string OfferPlayerID;
        public string RecipientPlayerID;
        public DateTime CreatedDate;
        public DateTime ExpirationDate;
        public TradeStatus Status;
        public List<TradeItem> OfferedItems = new List<TradeItem>();
        public List<TradeItem> RequestedItems = new List<TradeItem>();
        public string Notes;
        public float EstimatedValue;
        public TradeConditions Conditions;
        public bool IsCounterOffer;
        public string OriginalTradeID;
    }
    
    [System.Serializable]
    public class TradeItem
    {
        public string ItemID;
        public string ItemName;
        public TradeItemType ItemType;
        public int Quantity;
        public ItemRarity Rarity;
        public float EstimatedValue;
        public Dictionary<string, object> ItemProperties = new Dictionary<string, object>();
        public string Description;
        public bool IsUnique;
        public DateTime AcquisitionDate;
    }
    
    [System.Serializable]
    public class TradeConditions
    {
        public int MinimumPlayerLevel;
        public float MinimumReputationScore;
        public List<string> RequiredAchievements = new List<string>();
        public bool RequiresFriendship;
        public bool RequiresMutualRating;
        public string SpecialRequirements;
    }
    
    [System.Serializable]
    public class TradeHistory
    {
        public string TradeID;
        public string PartnerPlayerID;
        public DateTime CompletionDate;
        public List<TradeItem> ItemsGiven = new List<TradeItem>();
        public List<TradeItem> ItemsReceived = new List<TradeItem>();
        public float PlayerRating;
        public float PartnerRating;
        public string PlayerFeedback;
        public string PartnerFeedback;
        public bool WasSuccessful;
    }
    
    #endregion
    
    #region Mentorship System Data
    
    [System.Serializable]
    public class MentorshipProgram
    {
        public string ProgramID;
        public string ProgramName;
        public string Description;
        public MentorshipType ProgramType;
        public int MinimumMentorLevel;
        public float MinimumMentorRating;
        public List<string> RequiredSpecializations = new List<string>();
        public int MaxStudentsPerMentor;
        public float DurationWeeks;
        public List<MentorshipGoal> ProgramGoals = new List<MentorshipGoal>();
        public MentorshipRewards Rewards;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class MentorshipRelationship
    {
        public string RelationshipID;
        public string MentorPlayerID;
        public string StudentPlayerID;
        public string ProgramID;
        public DateTime StartDate;
        public DateTime? EndDate;
        public MentorshipStatus Status;
        public List<MentorshipSession> Sessions = new List<MentorshipSession>();
        public List<MentorshipGoal> Goals = new List<MentorshipGoal>();
        public float ProgressScore;
        public MentorshipRatings Ratings;
        public string Notes;
    }
    
    [System.Serializable]
    public class MentorshipSession
    {
        public string SessionID;
        public DateTime SessionDate;
        public float DurationMinutes;
        public string Topic;
        public string SessionNotes;
        public List<string> MaterialsCovered = new List<string>();
        public List<string> NextSteps = new List<string>();
        public float StudentRating;
        public float MentorRating;
        public bool GoalAchieved;
    }
    
    [System.Serializable]
    public class MentorshipGoal
    {
        public string GoalID;
        public string GoalName;
        public string Description;
        public MentorshipGoalType GoalType;
        public float TargetValue;
        public float CurrentProgress;
        public bool IsCompleted;
        public DateTime? CompletionDate;
        public string CompletionNotes;
    }
    
    [System.Serializable]
    public class MentorshipRatings
    {
        public float MentorRating;
        public float StudentRating;
        public string MentorFeedback;
        public string StudentFeedback;
        public DateTime RatingDate;
        public bool WouldRecommend;
    }
    
    [System.Serializable]
    public class MentorshipRewards
    {
        public int ExperienceBonus;
        public int ReputationBonus;
        public List<string> UnlockedContent = new List<string>();
        public List<string> SpecialRecognitions = new List<string>();
        public float CurrencyBonus;
    }
    
    #endregion
    
    #region Collaborative Projects Data
    
    [System.Serializable]
    public class CommunityProject
    {
        public string ProjectID;
        public string ProjectName;
        public string Description;
        public ProjectType ProjectType;
        public string CreatorPlayerID;
        public DateTime CreatedDate;
        public DateTime? CompletionDate;
        public ProjectStatus Status;
        public int MaxParticipants;
        public List<ProjectParticipant> Participants = new List<ProjectParticipant>();
        public List<ProjectMilestone> Milestones = new List<ProjectMilestone>();
        public ProjectGoals Goals;
        public ProjectRewards Rewards;
        public Dictionary<string, object> ProjectData = new Dictionary<string, object>();
        public bool IsPublic;
        public float DifficultyRating;
    }
    
    [System.Serializable]
    public class ProjectParticipant
    {
        public string PlayerID;
        public string PlayerName;
        public DateTime JoinDate;
        public ProjectRole Role;
        public float ContributionScore;
        public List<string> CompletedTasks = new List<string>();
        public List<string> AssignedTasks = new List<string>();
        public bool IsActive;
        public DateTime LastActivity;
    }
    
    [System.Serializable]
    public class ProjectMilestone
    {
        public string MilestoneID;
        public string MilestoneName;
        public string Description;
        public DateTime TargetDate;
        public DateTime? CompletionDate;
        public bool IsCompleted;
        public float ProgressPercentage;
        public List<string> RequiredTasks = new List<string>();
        public ProjectRewards CompletionRewards;
    }
    
    [System.Serializable]
    public class ProjectGoals
    {
        public string PrimaryGoal;
        public List<string> SecondaryGoals = new List<string>();
        public float SuccessCriteria;
        public Dictionary<string, float> ProgressMetrics = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class ProjectRewards
    {
        public int ExperienceReward;
        public int CurrencyReward;
        public List<string> UnlockedStrains = new List<string>();
        public List<string> UnlockedEquipment = new List<string>();
        public List<string> SpecialRecognitions = new List<string>();
        public bool GrantsAchievement;
        public string AchievementID;
    }
    
    #endregion
    
    #region Community Events Data
    
    [System.Serializable]
    public class CommunityGamingEvent
    {
        public string EventID;
        public string EventName;
        public string Description;
        public CommunityGamingEventType EventType;
        public DateTime StartDate;
        public DateTime EndDate;
        public DateTime RegistrationDeadline;
        public int MaxParticipants;
        public List<string> RegisteredPlayerIDs = new List<string>();
        public List<EventChallenge> Challenges = new List<EventChallenge>();
        public EventPrizes Prizes;
        public EventRules Rules;
        public bool IsActive;
        public bool RequiresApplication;
        public int MinimumPlayerLevel;
    }
    
    [System.Serializable]
    public class EventChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string Description;
        public ChallengeType ChallengeType;
        public DateTime StartTime;
        public DateTime EndTime;
        public Dictionary<string, object> ChallengeParameters = new Dictionary<string, object>();
        public List<EventSubmission> Submissions = new List<EventSubmission>();
        public EventScoring ScoringCriteria;
        public bool IsCompleted;
    }
    
    [System.Serializable]
    public class EventSubmission
    {
        public string SubmissionID;
        public string PlayerID;
        public DateTime SubmissionTime;
        public Dictionary<string, object> SubmissionData = new Dictionary<string, object>();
        public float Score;
        public int Rank;
        public bool IsValidated;
        public string JudgeNotes;
    }
    
    [System.Serializable]
    public class EventPrizes
    {
        public List<EventPrize> FirstPlacePrizes = new List<EventPrize>();
        public List<EventPrize> SecondPlacePrizes = new List<EventPrize>();
        public List<EventPrize> ThirdPlacePrizes = new List<EventPrize>();
        public List<EventPrize> ParticipationPrizes = new List<EventPrize>();
        public EventPrize GrandPrize;
    }
    
    [System.Serializable]
    public class EventPrize
    {
        public string PrizeName;
        public string Description;
        public PrizeType PrizeType;
        public object PrizeValue;
        public bool IsUnique;
        public string ImageURL;
    }
    
    [System.Serializable]
    public class EventRules
    {
        public List<string> EligibilityRequirements = new List<string>();
        public List<string> ProhibitedActions = new List<string>();
        public List<string> AllowedTools = new List<string>();
        public bool AllowCollaboration;
        public int MaxSubmissionsPerPlayer;
        public float TimeLimit;
        public string DisqualificationCriteria;
    }
    
    [System.Serializable]
    public class EventScoring
    {
        public Dictionary<string, float> ScoringWeights = new Dictionary<string, float>();
        public float BonusMultiplier;
        public bool UsesPeerReview;
        public bool UsesJudgePanel;
        public float MaxScore;
    }
    
    #endregion
    
    #region Social Features Data
    
    [System.Serializable]
    public class FacilityTour
    {
        public string TourID;
        public string OwnerPlayerID;
        public string TourName;
        public string Description;
        public DateTime CreatedDate;
        public bool IsPublic;
        public List<string> InvitedPlayerIDs = new List<string>();
        public List<TourStop> TourStops = new List<TourStop>();
        public TourRatings Ratings;
        public int VisitorCount;
        public List<TourVisit> RecentVisits = new List<TourVisit>();
        public bool IsActive;
    }
    
    [System.Serializable]
    public class TourStop
    {
        public string StopID;
        public string StopName;
        public string Description;
        public Vector3 Location;
        public string CameraAngle;
        public float Duration;
        public List<string> HighlightedFeatures = new List<string>();
        public string AudioNarration;
        public bool IsInteractive;
    }
    
    [System.Serializable]
    public class TourVisit
    {
        public string VisitID;
        public string VisitorPlayerID;
        public DateTime VisitDate;
        public float DurationMinutes;
        public List<string> StopsVisited = new List<string>();
        public float Rating;
        public string Feedback;
        public bool CompletedTour;
    }
    
    [System.Serializable]
    public class TourRatings
    {
        public float AverageRating;
        public int TotalRatings;
        public Dictionary<int, int> RatingDistribution = new Dictionary<int, int>();
        public List<string> FeaturedComments = new List<string>();
    }
    
    [System.Serializable]
    public class CommunityPost
    {
        public string PostID;
        public string AuthorPlayerID;
        public DateTime PostDate;
        public string Title;
        public string Content;
        public PostType PostType;
        public List<string> Tags = new List<string>();
        public List<PostAttachment> Attachments = new List<PostAttachment>();
        public PostEngagement Engagement;
        public bool IsSticky;
        public bool IsLocked;
        public bool IsFeatured;
        public PostModerationStatus ModerationStatus;
    }
    
    [System.Serializable]
    public class PostAttachment
    {
        public string AttachmentID;
        public string FileName;
        public AttachmentType AttachmentType;
        public string URL;
        public long FileSize;
        public string Description;
    }
    
    [System.Serializable]
    public class PostEngagement
    {
        public int Likes;
        public int Dislikes;
        public int Comments;
        public int Shares;
        public List<string> LikedByPlayerIDs = new List<string>();
        public float EngagementScore;
        public DateTime LastActivity;
    }
    
    #endregion
    
    // Using ReputationSystem from CommunityDataStructures
    
    // Using ReputationEvent from CommunityDataStructures
    
    // Using ReputationBadge from CommunityDataStructures
    
    #region Enums
    
    public enum PlayerReputationLevel
    {
        Newcomer,
        Novice,
        Apprentice,
        Journeyman,
        Expert,
        Master,
        Grandmaster,
        Legend
    }
    
    public enum TradeStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired,
        Completed,
        Cancelled,
        Disputed
    }
    
    public enum TradeItemType
    {
        Seeds,
        Genetics,
        Equipment,
        Nutrients,
        Knowledge,
        Research_Data,
        Artwork,
        Currency,
        Special_Items
    }
    
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical,
        Unique
    }
    
    public enum MentorshipType
    {
        Basic_Cultivation,
        Advanced_Genetics,
        Facility_Management,
        Business_Strategy,
        Research_Methods,
        Competition_Training,
        Specialized_Techniques
    }
    
    public enum MentorshipStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled,
        Pending_Approval
    }
    
    public enum MentorshipGoalType
    {
        Skill_Development,
        Achievement_Unlock,
        Project_Completion,
        Competition_Entry,
        Knowledge_Mastery,
        Facility_Milestone,
        Research_Publication
    }
    
    public enum ProjectType
    {
        Research_Study,
        Breeding_Program,
        Facility_Design,
        Community_Garden,
        Knowledge_Base,
        Competition_Team,
        Innovation_Challenge
    }
    
    public enum ProjectStatus
    {
        Planning,
        Active,
        Completed,
        Paused,
        Cancelled,
        Under_Review
    }
    
    public enum ProjectRole
    {
        Creator,
        Co_Leader,
        Specialist,
        Contributor,
        Observer,
        Mentor,
        Researcher
    }
    
    public enum CommunityEventType
    {
        Competition,
        Workshop,
        Exhibition,
        Collaboration,
        Research_Challenge,
        Social_Gathering,
        Training_Program,
        Innovation_Showcase
    }

    public enum CommunityGamingEventType
    {
        Competition,
        Workshop,
        Exhibition,
        Collaboration,
        Research_Challenge,
        Social_Gathering,
        Training_Program,
        Innovation_Showcase
    }
    
    public enum ChallengeType
    {
        Cultivation_Challenge,
        Genetics_Puzzle,
        Facility_Design,
        Efficiency_Contest,
        Innovation_Challenge,
        Knowledge_Quiz,
        Creative_Challenge,
        Research_Proposal
    }
    
    public enum PrizeType
    {
        Currency,
        Equipment,
        Genetics,
        Recognition,
        Access,
        Achievement,
        Physical_Prize,
        Experience_Bonus
    }
    
    public enum PostType
    {
        Discussion,
        Question,
        Tutorial,
        Showcase,
        News,
        Research_Share,
        Trade_Request,
        Event_Announcement
    }
    
    // Using AttachmentType from ProjectChimera.Data.Community.CommunityDataStructures
    
    public enum PostModerationStatus
    {
        Approved,
        Pending_Review,
        Flagged,
        Removed,
        Restricted
    }
    
    public enum ReputationCategory
    {
        Trading,
        Mentoring,
        Collaboration,
        Knowledge_Sharing,
        Competition,
        Innovation,
        Community_Support,
        Research_Contribution
    }
    
    public enum ReputationEventType
    {
        Successful_Trade,
        Failed_Trade,
        Positive_Feedback,
        Negative_Feedback,
        Helpful_Post,
        Spam_Report,
        Achievement_Unlock,
        Community_Contribution,
        Mentorship_Success,
        Project_Completion
    }
    
    public enum ReputationBadgeType
    {
        Trader,
        Mentor,
        Collaborator,
        Innovator,
        Helper,
        Scholar,
        Leader,
        Pioneer
    }

    public enum BadgeRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    #endregion
}

// Extension methods for DateTime
public static class DateTimeExtensions
{
    public static DateTime AddWeeks(this DateTime dateTime, float weeks)
    {
        return dateTime.AddDays(weeks * 7);
    }
}