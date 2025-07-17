using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Genetics.Scientific
{
    /// <summary>
    /// Achievement data structures for scientific and breeding achievements.
    /// </summary>
    
    // ScientificAchievement moved to ScientificAchievementDatabaseSO.cs to resolve namespace conflict
    
    [System.Serializable]
    public class ScientificBreakthrough
    {
        public string BreakthroughID;
        public string BreakthroughName;
        public string Description;
        public BreakthroughType Type;
        public TraitType RelatedTrait;
        public float ImpactScore;
        public System.DateTime DiscoveryDate;
        public string ResearchNotes;
        public bool IsPublished;
        
        // Missing properties for Systems layer
        public string DiscoveredBy = "";
        public float ReputationValue = 0f;
        public float PrestigeValue = 0f;
        public string BreakthroughId = "";
        public BreakthroughType BreakthroughType = BreakthroughType.GeneticDiscovery;
        public float SignificanceLevel = 0f;
    }
    
    [System.Serializable]
    public class AchievementCriterion
    {
        public string CriterionID;
        public string Description;
        public AchievementCriterionType Type;
        public float TargetValue;
        public float CurrentValue;
        public bool IsCompleted;
    }
    
    [System.Serializable]
    public class AchievementReward
    {
        public string RewardID;
        public RewardType Type;
        public float Value;
        public string Description;
        public List<string> UnlockedFeatures = new List<string>();
    }
    
    public enum BreakthroughType
    {
        GeneticDiscovery,
        BreedingMethod,
        TraitIdentification,
        EnvironmentalOptimization,
        QualityImprovement,
        EfficiencyGain,
        NovelCombination,
        ScientificInnovation,
        TraitOptimization
    }
    
    public enum AchievementDifficulty
    {
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }
    
    public enum RewardType
    {
        Experience,
        Currency,
        UnlockFeature,
        SpecialAccess,
        Recognition,
        Equipment,
        Knowledge,
        Title
    }
    
    [System.Serializable]
    public class ResearchMilestone
    {
        public string MilestoneID;
        public string MilestoneName;
        public string Description;
        public ResearchMilestoneType Type;
        public float RequiredProgress;
        public float CurrentProgress;
        public bool IsCompleted;
        public AchievementReward Reward;
        public System.DateTime UnlockDate;
        public System.DateTime CompletionDate;
        
        // Missing properties for Systems layer
        public string MilestoneId = "";
        public ResearchMilestoneType MilestoneType = ResearchMilestoneType.BreedingGoalCompletion;
    }
    
    [System.Serializable]
    public class ScientificReputation
    {
        public string PlayerId = "";
        public float ReputationPoints = 0f;
        public int BreakthroughCount = 0;
        public List<string> EarnedTitles = new List<string>();
        public System.DateTime LastUpdated = System.DateTime.Now;
        public int AchievementCount = 0;
        public string CurrentTitle = "";
    }
    
    [System.Serializable]
    public class ScientificRecognition
    {
        public string RecognitionID;
        public string RecognitionName;
        public RecognitionType Type;
        public string Description;
        public string AwardedBy;
        public System.DateTime AwardDate;
        public ScientificBreakthrough RelatedBreakthrough;
        
        // Missing properties for Systems layer
        public string RecognitionId = "";
        public string PlayerId = "";
        public RecognitionType RecognitionType = RecognitionType.ScientificExcellence;
        public string Title = "";
        public System.DateTime AwardedDate = System.DateTime.Now;
        public float PrestigeValue = 0f;
    }
    
    [System.Serializable]
    public class ResearchContribution
    {
        public string ContributionID;
        public string ContributorID;
        public string ContributorName;
        public ContributionType Type;
        public string Description;
        public float ImpactScore;
        public System.DateTime ContributionDate;
        public bool IsPublished;
        
        // Missing properties for Systems layer
        public int DataPointsContributed = 0;
        public ResearchField ResearchField = ResearchField.Genetics;
        public ContributionType ContributionType = ContributionType.DataCollection;
        public string ContributionId = "";
        public string PlayerId = "";
        public float QualityScore = 0f;
    }
    
    [System.Serializable]
    public class ScientificAchievementRequirements
    {
        public List<string> PrerequisiteAchievements = new List<string>();
        public int MinLevel = 1;
        public float MinScore = 0f;
        public List<RequiredResource> RequiredResources = new List<RequiredResource>();
        public bool RequiresSpecialAccess = false;
        public string RequirementId = "";
        public string Description = "";
        public bool IsCompleted = false;
        public int BreedingCrossesCompleted = 0;
        
        // Missing properties for Systems layer
        public string Foundation => "Foundation requirement";
        public int TraitAnalysesPerformed = 0;
        public int DocumentedResearch = 0;
        public int TraitMasteryAchieved = 0;
        public float MinimumTraitExpression = 0f;
        public float PredictedInheritanceAccuracy = 0f;
        public int BreedingGenerationsCompleted = 0;
        public int HybridVigorCrossesAchieved = 0;
        public float MinimumHeterosisEffect = 0f;
        public int ScientificBreakthroughsMade = 0;
        public float ResearchImpactScore = 0f;
        public int DataPointsCollected = 0;
        public int StatisticalAnalysesPerformed = 0;
        public int ResearchProjectsLed = 0;
        public int CollaboratorsWorkedWith = 0;
        public int InnovativeTechniquesCreated = 0;
        public float TechniqueAdoptionRate = 0f;
        public bool ParadigmShiftInitiated = false;
        public float FieldInfluenceScore = 0f;
        public int CollaborativeProjectsCompleted = 0;
        public int KnowledgeSharedInstances = 0;
        public int ResearchersHelped = 0;
        public float CommunityLeadershipScore = 0f;
        public float ResearchersInfluenced = 0f;
    }
    
    [System.Serializable]
    public class RequiredResource
    {
        public string ResourceType;
        public float RequiredAmount;
        public string Description;
    }
    
    [System.Serializable]
    public class PlayerAchievementProgress
    {
        public string PlayerID;
        public string AchievementID;
        public float Progress; // 0-1
        public bool IsUnlocked;
        public bool IsCompleted;
        public System.DateTime StartDate;
        public System.DateTime CompletionDate;
        public List<string> CompletedCriteria = new List<string>();
        
        // Missing properties for Systems layer
        public string PlayerId = ""; // Alias for compatibility with Systems layer
        public int BreedingCrossesCompleted = 0;
        public int TotalBreedingEvents = 0;
        public int DataPointsCollected = 0;
        public int ScientificBreakthroughsMade = 0;
        
        // Additional missing properties for Systems layer
        public int TraitAnalysesPerformed = 0;
        public float OverallQuality = 0f;
        public float HeterosisEffect = 0f;
        public int HybridVigorCrossesAchieved = 0;
        public int DocumentedResearch = 0;
        public int TraitMasteriesAchieved = 0;
        public int CollaborativeProjectsCompleted = 0;
        public float BestTraitExpression = 0f;
        public float BestHeterosisEffect = 0f;
        public float InheritancePredictionAccuracy = 0f;
        public int HighQualityResults = 0;
        public int ReachedMilestonesCount = 0;
        public List<string> ReachedMilestones = new List<string>();
        
        // Additional tournament-related properties for Systems layer
        public int TournamentParticipations = 0;
        public int TournamentWins = 0;
        public int TopFiveTournamentFinishes = 0;
        public System.DateTime FirstActivity = System.DateTime.Now;
        
        // Methods for Systems layer compatibility
        public void AddTraitAnalyses(int count)
        {
            this.TraitAnalysesPerformed += count;
        }
        
        public void UpdateOverallQuality(float quality)
        {
            this.OverallQuality = quality;
        }
        
        public void UpdateHeterosisEffect(float effect)
        {
            this.HeterosisEffect = effect;
        }
        
        public void AddHybridVigorCrosses(int count)
        {
            this.HybridVigorCrossesAchieved += count;
        }
        
        public void AddDocumentedResearch(int count)
        {
            this.DocumentedResearch += count;
        }
    }
    
    [System.Serializable]
    public class ScientificTitleProgression
    {
        public string PlayerID;
        public ScientificTitle CurrentTitle;
        public float ProgressToNext;
        public List<ScientificTitle> EarnedTitles = new List<ScientificTitle>();
        public System.DateTime LastPromotion;
        
        // Missing properties for Systems layer
        public string TitleId = "";
        public string TitleName = "";
        public string Description = "";
        public List<string> RequiredAchievements = new List<string>();
        public int MinimumReputation = 0;
        public Dictionary<ResearchField, float> SpecializationRequirements = new Dictionary<ResearchField, float>();
        public float PrestigeValue = 0f;
    }
    
    [System.Serializable]
    public class ScientificTitle
    {
        public string TitleID;
        public string TitleName;
        public int TitleRank;
        public string Description;
        public List<string> RequiredAchievements = new List<string>();
        public AchievementReward UnlockReward;
    }
    
    public enum ResearchMilestoneType
    {
        BreedingGoalCompletion,
        TournamentVictory,
        ScientificDiscovery,
        InnovationAchievement,
        CollaborationMilestone,
        PublicationMilestone,
        MentorshipMilestone,
        KnowledgeContribution
    }
    
    public enum RecognitionType
    {
        ScientificExcellence,
        InnovationAward,
        BreedingMastery,
        ResearchContribution,
        CommunityLeadership,
        MentorshipRecognition,
        LifetimeAchievement,
        PeerRecognition,
        Breakthrough,
        Title
    }
    
    // Missing enums and classes for Systems layer
    public enum ScientificAchievementCategory
    {
        Foundation,
        Genetics,
        BreedingTechnology,
        Innovation,
        Research,
        Leadership,
        Breeding,
        Collaboration
    }
    
    // Alias for compatibility
    public enum AchievementCategory
    {
        Foundation,
        Genetics,
        BreedingTechnology,
        Innovation,
        Research,
        Leadership,
        Breeding,
        Collaboration
    }
    
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    
    
    public enum ResearchField
    {
        Genetics,
        BreedingTechnology,
        Innovation,
        Research,
        Leadership
    }
    
    public enum ContributionType
    {
        DataCollection,
        ResearchProject,
        Innovation,
        Collaboration,
        Publication,
        Mentorship,
        KnowledgeSharing,
        TechnicalContribution
    }
    
}