using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Seasonal event and breeding event data structures.
    /// </summary>
    
    [System.Serializable]
    public class SeasonData
    {
        public string SeasonID;
        public string SeasonName;
        public SeasonType Type;
        public System.DateTime StartDate;
        public System.DateTime EndDate;
        public List<SeasonalEvent> Events = new List<SeasonalEvent>();
        public List<SeasonalModifier> Modifiers = new List<SeasonalModifier>();
        public SeasonalRewards Rewards;
        
        // Missing properties for Systems layer
        public string SeasonId { get; set; } = ""; // Changed to settable property
        public bool IsActive = true;
    }
    
    [System.Serializable]
    public class SeasonalEvent
    {
        public string EventID;
        public string EventName;
        public string Description;
        public SeasonalEventType EventType;
        public System.DateTime StartTime;
        public System.DateTime EndTime;
        public List<EventReward> Rewards = new List<EventReward>();
        public EventRequirements Requirements;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class BreedingEventData
    {
        public string EventID;
        public string EventName;
        public BreedingEventType EventType;
        public string Description;
        public System.DateTime StartDate;
        public System.DateTime EndDate;
        public List<BreedingChallenge> Challenges = new List<BreedingChallenge>();
        public EventRewards Rewards;
        public bool IsGlobalEvent;
        
        // Missing properties for Systems layer
        public string TraitExpression = "";
        public float OverallQuality = 0f;
        public float HeterosisEffect = 0f;
    }
    
    // BreedingChallenge moved to BreedingChallengeLibrarySO.cs to resolve namespace conflict
    
    [System.Serializable]
    public class SeasonalModifier
    {
        public string ModifierName;
        public ModifierType Type;
        public float Value;
        public string Description;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class SeasonalRewards
    {
        public List<EventReward> ParticipationRewards = new List<EventReward>();
        public List<EventReward> CompletionRewards = new List<EventReward>();
        public List<EventReward> LeaderboardRewards = new List<EventReward>();
    }
    
    [System.Serializable]
    public class EventReward
    {
        public string RewardID;
        public string RewardName;
        public RewardType Type;
        public float Value;
        public string Description;
        public bool IsExclusive;
    }
    
    [System.Serializable]
    public class EventRewards
    {
        public List<EventReward> Rewards = new List<EventReward>();
        public EventReward GrandPrize;
        public List<EventReward> ParticipationRewards = new List<EventReward>();
    }
    
    [System.Serializable]
    public class EventRequirements
    {
        public int MinLevel = 1;
        public List<string> RequiredAchievements = new List<string>();
        public float EntryFee = 0f;
        public bool RequiresOriginalWork = false;
    }
    
    public enum SeasonType
    {
        Spring,
        Summer,
        Fall,
        Winter,
        Special,
        Championship,
        Anniversary,
        Holiday
    }
    
    public enum BreedingEventType
    {
        CommunityChallenge,
        CompetitiveBreeding,
        InnovationContest,
        SpeedBreeding,
        QualityChallenge,
        CollaborativeProject,
        EducationalEvent,
        ResearchInitiative
    }
    
    public enum ModifierType
    {
        ExperienceMultiplier,
        ResourceBonus,
        TimeReduction,
        QualityBonus,
        SuccessRateIncrease,
        CostReduction,
        AccessBonus,
        SpecialUnlock
    }
}