using System;
using System.Collections.Generic;

namespace ProjectChimera.Core
{
    public interface ILeaderboard
    {
        string Id { get; }
        string DisplayName { get; }
        string LeaderboardDescription { get; }
        LeaderboardType LeaderboardType { get; }
        LeaderboardCategory LeaderboardCategory { get; }
        TimePeriod TimePeriod { get; }
        DateTime LastUpdateTime { get; }
        bool Active { get; }
        int MaximumEntries { get; }
        IReadOnlyList<ILeaderboardEntry> LeaderboardEntries { get; }
        ILeaderboardSettings Settings { get; }
    }

    public interface ILeaderboardEntry
    {
        string Id { get; }
        string DisplayName { get; }
        float EntryScore { get; }
        int EntryRank { get; }
        DateTime LastUpdateTime { get; }
    }

    public interface IEventParticipant
    {
        string Id { get; }
        string PlayerId { get; }
        string DisplayName { get; }
        DateTime ParticipationDate { get; }
        IEventParticipationData ParticipationData { get; }
        bool Active { get; }
        void UpdateScore(string scoreCategory, float score);
    }

    public interface ILeaderboardSettings
    {
        ScoreOrder SortOrder { get; }
        float MinimumScore { get; }
        bool RequireVerification { get; }
    }

    public interface ICommunityEvent
    {
        string Id { get; }
        string DisplayName { get; }
        string EventDescription { get; }
        DateTime StartDate { get; }
        DateTime EndDate { get; }
        string Status { get; }
        IReadOnlyList<IEventMilestone> Milestones { get; }
        IReadOnlyList<IEventParticipant> Participants { get; }
        IEventRewards Rewards { get; }
        IEventRequirements Requirements { get; }
    }

    public interface IEventParticipationData
    {
        string EventId { get; }
        float Progress { get; }
        float Score { get; }
        List<string> Achievements { get; }
        List<string> CompletedMilestones { get; }
        Dictionary<string, float> ScoresByCategory { get; }
    }

    public interface IEventRewards
    {
        IReadOnlyList<IEventReward> Rewards { get; }
    }

    public interface IEventReward
    {
        string Id { get; }
        string DisplayName { get; }
        string RewardDescription { get; }
    }

    public interface IEventRequirements
    {
        int MinimumLevel { get; }
        int MinimumReputation { get; }
    }

    public interface IEventMilestone
    {
        string Id { get; }
        string DisplayName { get; }
        string MilestoneDescription { get; }
        float RequiredProgress { get; }
        IEventReward Reward { get; }
    }
} 