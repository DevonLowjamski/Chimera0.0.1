using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics.Scientific;
using ScientificResearchField = ProjectChimera.Data.Genetics.Scientific.ResearchField;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific system data structures for research, reputation, and statistics.
    /// </summary>
    
    // SimpleTraitData moved to ScientificAchievementDataStructures.cs to resolve namespace conflict
    
    [System.Serializable]
    public class SpecializationData
    {
        public string SpecializationID;
        public string SpecializationName;
        public ResearchField PrimaryField;
        public List<ResearchField> SecondaryFields = new List<ResearchField>();
        public float ExpertiseLevel;
        public float ProgressToNext;
        public List<string> SpecializedSkills = new List<string>();
        public List<string> UnlockedCapabilities = new List<string>();
        public System.DateTime LastImprovement;
        
        // Missing properties for Systems layer
        public string PlayerId = "";
        public Dictionary<ScientificResearchField, float> FieldExpertise = new Dictionary<ScientificResearchField, float>();
        public Dictionary<string, float> TraitExpertise = new Dictionary<string, float>();
        public List<string> MasteredTraits = new List<string>();
        public System.DateTime LastResearchDate = System.DateTime.Now;
        public System.DateTime LastUpdateTime = System.DateTime.Now;
    }
    
    [System.Serializable]
    public class PlayerReputationProfile
    {
        public string PlayerID;
        public float OverallRating;
        public float BreedingReputation;
        public float ResearchReputation;
        public float TeachingReputation;
        public float CommunityReputation;
        public int PublishedPapers;
        public int Citations;
        public List<string> AwardsReceived = new List<string>();
        public List<string> RecognitionBadges = new List<string>();
        public System.DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class ScientificStatistics
    {
        public int TotalBreakthroughs;
        public int TotalPublications;
        public int TotalCitations;
        public float AverageBreakthroughImpact;
        public int PlayersWithPublications;
        public int ActiveResearchers;
        public float GlobalKnowledgeScore;
        public Dictionary<ScientificResearchField, int> BreakthroughsByField = new Dictionary<ScientificResearchField, int>();
        public System.DateTime LastUpdate;
        
        // Missing properties for Systems layer
        public int TournamentsCompleted = 0;
        public int BreedingGoalsCompleted = 0;
        public int AchievementsUnlocked = 0;
        public int TitlesAwarded = 0;
        public int CollaborativeProjectsActive = 0;
        public int ResearchContributionsSubmitted = 0;
        
        public void UpdateStatistics()
        {
            LastUpdate = System.DateTime.Now;
        }
    }
    
    // ResearchField moved to AchievementDataStructures.cs to resolve namespace conflict
}