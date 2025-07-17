using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Community;

namespace ProjectChimera.Data
{
    /// <summary>
    /// Shared data structures used across multiple systems in Project Chimera.
    /// This centralizes common data types to avoid duplication and ensure consistency.
    /// </summary>

    [Serializable]
    public class PlayerProfile
    {
        [Header("Player Information")]
        public string PlayerId;
        public string PlayerName;
        public string DisplayName;
        public string Bio = "";
        public int Level = 1;
        public int Experience = 0;
        public int ReputationPoints = 0;
        public DateTime LastActive = DateTime.Now;
        public PlayerStatus Status = PlayerStatus.Online;
        
        [Header("Player Stats")]
        public int TotalHarvests = 0;
        public int ForumPosts = 0;
        public int CultivationExperience = 0;
        public bool IsVerified = false;
        public List<string> Badges = new List<string>();
        public List<string> Achievements = new List<string>();
        public List<string> UnlockedContent = new List<string>();
        public Dictionary<string, object> PlayerData = new Dictionary<string, object>();
        
        // Community system properties
        public List<Badge> EarnedBadges = new List<Badge>();

        public PlayerProfile()
        {
            PlayerId = Guid.NewGuid().ToString();
            PlayerName = "Player";
            DisplayName = "Player";
            Badges = new List<string>();
            Achievements = new List<string>();
            UnlockedContent = new List<string>();
            PlayerData = new Dictionary<string, object>();
            EarnedBadges = new List<Badge>();
            LastActive = DateTime.Now;
        }

        public PlayerProfile(string playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            DisplayName = playerName;
            Badges = new List<string>();
            Achievements = new List<string>();
            UnlockedContent = new List<string>();
            PlayerData = new Dictionary<string, object>();
            EarnedBadges = new List<Badge>();
            LastActive = DateTime.Now;
        }

        // Method for achievement checking (from GlobalCompetitionEventSO)
        public bool HasAchievement(string achievement)
        {
            return Achievements.Contains(achievement);
        }

        // Extension methods for requirement checking (from LiveEventInterfaces)
        public bool MeetsRequirement(string requirement)
        {
            // Parse requirement string and check if player meets it
            var parts = requirement.Split(':');
            if (parts.Length < 2) return true;
            
            var type = parts[0];
            var value = parts[1];
            
            return type switch
            {
                "level" => int.TryParse(value, out var requiredLevel) && Level >= requiredLevel,
                "experience" => int.TryParse(value, out var requiredExp) && Experience >= requiredExp,
                "harvest_count" => int.TryParse(value, out var requiredHarvests) && TotalHarvests >= requiredHarvests,
                "cultivation_experience" => int.TryParse(value, out var requiredCultExp) && CultivationExperience >= requiredCultExp,
                "badge" => Badges.Contains(value),
                "achievement" => HasAchievement(value),
                "content_unlocked" => UnlockedContent.Contains(value),
                "verified" => bool.TryParse(value, out var requireVerified) && (!requireVerified || IsVerified),
                _ => true
            };
        }
    }

    public enum PlayerStatus
    {
        Online,
        Away,
        Busy,
        Offline,
        DoNotDisturb
    }

    [Serializable]
    public class PlayerContribution
    {
        [Header("Contribution Information")]
        public PlayerProfile PlayerProfile;
        public string ContributionType;
        public float Amount;
        public DateTime Timestamp = DateTime.Now;
        public Dictionary<string, object> ContributionData = new Dictionary<string, object>();

        public PlayerContribution()
        {
            ContributionData = new Dictionary<string, object>();
        }
    }

    [Serializable]
    public class EventParticipant
    {
        [Header("Participant Information")]
        public string PlayerId;
        public string PlayerName;
        public DateTime JoinedAt = DateTime.Now;
        public bool IsActive = true;
        public Dictionary<string, object> ParticipantData = new Dictionary<string, object>();

        public EventParticipant()
        {
            ParticipantData = new Dictionary<string, object>();
        }

        public EventParticipant(string playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            JoinedAt = DateTime.Now;
            ParticipantData = new Dictionary<string, object>();
        }
        
        // Missing method referenced in CommunityManager.cs line 303
        public void UpdateScore(float score)
        {
            if (ParticipantData.ContainsKey("score"))
            {
                ParticipantData["score"] = score;
            }
            else
            {
                ParticipantData.Add("score", score);
            }
            ParticipantData["lastScoreUpdate"] = DateTime.Now;
        }
    }

    [Serializable]
    public class LeaderboardEntry
    {
        [Header("Leaderboard Entry")]
        public string PlayerId;
        public string PlayerName;
        public float Score;
        public int Rank;
        public DateTime LastUpdated = DateTime.Now;
        public Dictionary<string, object> EntryData = new Dictionary<string, object>();

        public LeaderboardEntry()
        {
            EntryData = new Dictionary<string, object>();
        }
    }

    [Serializable]
    public class EducationalProgress
    {
        [Header("Educational Progress")]
        public string PlayerId;
        public string ObjectiveId;
        public float CompletionPercentage = 0f;
        public DateTime StartTime = DateTime.Now;
        public DateTime? CompletionTime = null;
        public List<string> CompletedSteps = new List<string>();
        public Dictionary<string, object> ProgressData = new Dictionary<string, object>();

        public EducationalProgress()
        {
            CompletedSteps = new List<string>();
            ProgressData = new Dictionary<string, object>();
        }
    }

    [Serializable]
    public enum LeaderboardType
    {
        // Overall Rankings
        Overall,
        Global,
        Regional,
        Friends,
        Guild,
        
        // Time-based Categories
        Weekly,
        Monthly,
        Seasonal,
        
        // Cultivation & Growth
        TotalYield,
        Cultivation,
        
        // Quality & Analysis
        Quality,
        QualityScore,
        THCPotency,
        CBDContent,
        TerpeneProfile,
        
        // Economic & Efficiency
        Economic,
        EconomicSuccess,
        FacilityEfficiency,
        
        // Innovation & Research
        Innovation,
        BreedingInnovation,
        ResearchPoints,
        
        // Performance & Speed
        Speed,
        SpeedRun,
        
        // Community
        CommunityContribution,
        SustainabilityScore
    }
} 