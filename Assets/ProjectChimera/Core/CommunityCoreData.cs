using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Core
{
    [System.Serializable]
    public enum LeaderboardType
    {
        TotalYield,
        Quality,
        Economic,
        Efficiency,
        Innovation,
        CommunityContribution,
        BreedingInnovation,
        SpeedRun,
        THCPotency,
        CBDContent,
        TerpeneProfile,
        Cultivation,
        EconomicSuccess,
        FacilityEfficiency,
        ResearchPoints,
        Seasonal,
        Global,
        Regional,
        Friends,
        Guild,
        Speed,
        Event_Score,
        Highest_Quality_Strain,
        Most_Unique_Genotypes,
        Highest_Profit,
        Most_Contracts_Completed
    }

    [System.Serializable]
    public enum LeaderboardCategory
    {
        Cultivation,
        Genetics,
        Economics,
        Efficiency,
        Innovation,
        Community,
        Competition,
        Events,
        General
    }

    [System.Serializable]
    public enum TimePeriod
    {
        All_Time,
        AllTime, // Keep for backward compatibility
        Monthly,
        Weekly,
        Daily,
        Seasonal
    }

    [System.Serializable]
    public enum ScoreOrder
    {
        Descending,
        Ascending
    }

    [System.Serializable]
    public class EventParticipationData
    {
        public string EventId;
        public float Progress;
        public float Score;
        public Dictionary<string, float> Scores = new Dictionary<string, float>();
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
        public int Rank;
        public float TotalScore;
        public List<string> CompletedChallenges = new List<string>();
        public List<string> CompletedMilestones = new List<string>();
        public List<string> Achievements = new List<string>();
    }
} 