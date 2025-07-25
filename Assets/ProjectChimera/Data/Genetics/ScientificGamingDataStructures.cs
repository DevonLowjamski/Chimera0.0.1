using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Clean scientific gaming data structures for genetics gaming systems
    /// Restored from ComprehensiveProgressionManager features with verified dependencies
    /// Supports breeding challenges, competitions, and scientific progression
    /// </summary>
    
    #region Scientific Competition Data
    
    [System.Serializable]
    public class CleanScientificCompetition
    {
        public string CompetitionID;
        public string CompetitionName;
        public string Description;
        public ScientificCompetitionType CompetitionType;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public List<CleanCompetitionEntry> Entries = new List<CleanCompetitionEntry>();
        public CleanCompetitionRewards Rewards;
        public CleanCompetitionCriteria JudgingCriteria;
    }
    
    [System.Serializable]
    public class CleanCompetitionEntry
    {
        public string ParticipantID;
        public string ParticipantName;
        public string SubmissionID;
        public float Score;
        public int Rank;
        public DateTime SubmissionDate;
        public CleanGeneticSubmissionData SubmissionData;
        public string Notes;
    }
    
    [System.Serializable]
    public class CleanCompetitionRewards
    {
        public List<string> FirstPlaceRewards = new List<string>();
        public List<string> SecondPlaceRewards = new List<string>();
        public List<string> ThirdPlaceRewards = new List<string>();
        public List<string> ParticipationRewards = new List<string>();
        public float ExperienceBonus;
    }
    
    [System.Serializable]
    public class CleanCompetitionCriteria
    {
        public float GeneticInnovationWeight;
        public float PracticalApplicationWeight;
        public float ScientificRigorWeight;
        public float PresentationQualityWeight;
        public List<string> RequiredMetrics = new List<string>();
    }
    
    #endregion
    
    #region Genetic Research Data
    
    [System.Serializable]
    public class CleanGeneticResearchProject
    {
        public string ProjectID;
        public string ProjectName;
        public string Description;
        public GeneticResearchType ResearchType;
        public DateTime StartDate;
        public float Progress; // 0-1
        public bool IsCompleted;
        public List<CleanResearchPhase> Phases = new List<CleanResearchPhase>();
        public CleanResearchRequirements Requirements;
        public CleanResearchRewards ExpectedRewards;
    }
    
    [System.Serializable]
    public class CleanResearchPhase
    {
        public string PhaseID;
        public string PhaseName;
        public string Description;
        public float Progress; // 0-1
        public bool IsCompleted;
        public List<string> RequiredActions = new List<string>();
        public List<string> CompletedActions = new List<string>();
    }
    
    [System.Serializable]
    public class CleanResearchRequirements
    {
        public int MinSkillLevel;
        public List<string> RequiredEquipment = new List<string>();
        public List<string> RequiredResources = new List<string>();
        public float EstimatedTimeHours;
        public float ResourceCost;
    }
    
    [System.Serializable]
    public class CleanResearchRewards
    {
        public float ExperienceGain;
        public List<string> UnlockedTechniques = new List<string>();
        public List<string> UnlockedEquipment = new List<string>();
        public List<string> UnlockedStrains = new List<string>();
        public float ReputationGain;
    }
    
    #endregion
    
    #region Breeding Challenge Data
    
    [System.Serializable]
    public class CleanBreedingChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string Description;
        public BreedingChallengeType ChallengeType;
        public CleanChallengeObjective Objective;
        public CleanChallengeConstraints Constraints;
        public CleanChallengeRewards Rewards;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public BreedingDifficulty Difficulty;
    }
    
    [System.Serializable]
    public class CleanChallengeObjective
    {
        public string ObjectiveDescription;
        public List<CleanTraitTarget> RequiredTraits = new List<CleanTraitTarget>();
        public float MinimumQualityScore;
        public int RequiredGenerations;
        public bool RequireStability;
    }
    
    [System.Serializable]
    public class CleanTraitTarget
    {
        public string TraitName;
        public float TargetValue;
        public float ToleranceRange;
        public bool IsRequired;
        public float Weight; // Importance in scoring
    }
    
    [System.Serializable]
    public class CleanChallengeConstraints
    {
        public int MaxGenerations;
        public int MaxPlants;
        public List<string> AllowedParentStrains = new List<string>();
        public List<string> ForbiddenTechniques = new List<string>();
        public float TimeLimit; // Hours
        public float BudgetLimit;
    }
    
    [System.Serializable]
    public class CleanChallengeRewards
    {
        public float ExperienceReward;
        public float ReputationReward;
        public List<string> UnlockRewards = new List<string>();
        public string AchievementID;
        public float MonetaryReward;
    }
    
    #endregion
    
    #region Scientific Achievement Data
    
    [System.Serializable]
    public class CleanScientificAchievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public ScientificAchievementType AchievementType;
        public CleanAchievementCriteria Criteria;
        public CleanAchievementRewards Rewards;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public float Progress; // 0-1
        public bool IsHidden;
    }
    
    [System.Serializable]
    public class CleanAchievementCriteria
    {
        public List<CleanAchievementRequirement> Requirements = new List<CleanAchievementRequirement>();
        public bool RequireAllCriteria;
        public float MinimumScore;
    }
    
    [System.Serializable]
    public class CleanAchievementRequirement
    {
        public string RequirementType;
        public string RequirementValue;
        public float TargetValue;
        public bool IsCompleted;
        public float CurrentProgress;
    }
    
    [System.Serializable]
    public class CleanAchievementRewards
    {
        public float ExperienceBonus;
        public float ReputationBonus;
        public List<string> UnlockRewards = new List<string>();
        public string SpecialTitle;
        public string BadgeIcon;
    }
    
    #endregion
    
    #region Genetic Submission Data
    
    [System.Serializable]
    public class CleanGeneticSubmissionData
    {
        public string SubmissionID;
        public string StrainName;
        public CleanGeneticProfile GeneticProfile;
        public CleanPhenotypicData PhenotypicData;
        public CleanBreedingHistory BreedingHistory;
        public DateTime SubmissionDate;
        public string SubmitterNotes;
    }
    
    [System.Serializable]
    public class CleanGeneticProfile
    {
        public List<CleanAlleleExpression> AlleleExpressions = new List<CleanAlleleExpression>();
        public float GeneticDiversity;
        public float StabilityScore;
        public List<string> DominantTraits = new List<string>();
        public List<string> RecessiveTraits = new List<string>();
    }
    
    [System.Serializable]
    public class CleanAlleleExpression
    {
        public string GeneID;
        public string AlleleID;
        public float ExpressionLevel;
        public bool IsDominant;
        public float Contribution;
    }
    
    [System.Serializable]
    public class CleanPhenotypicData
    {
        public float Height;
        public float YieldPotential;
        public float FloweringTime;
        public List<CleanChemicalProfile> ChemicalProfiles = new List<CleanChemicalProfile>();
        public List<CleanPhysicalTrait> PhysicalTraits = new List<CleanPhysicalTrait>();
        public float OverallQuality;
    }
    
    [System.Serializable]
    public class CleanChemicalProfile
    {
        public string CompoundName;
        public float Concentration;
        public string Unit;
        public float Variance;
    }
    
    [System.Serializable]
    public class CleanPhysicalTrait
    {
        public string TraitName;
        public string TraitValue;
        public float NumericValue;
        public string Description;
    }
    
    [System.Serializable]
    public class CleanBreedingHistory
    {
        public List<CleanGenerationRecord> Generations = new List<CleanGenerationRecord>();
        public List<string> ParentStrains = new List<string>();
        public List<string> TechniquesUsed = new List<string>();
        public int TotalGenerations;
        public float TimeInvested;
    }
    
    [System.Serializable]
    public class CleanGenerationRecord
    {
        public int GenerationNumber;
        public List<string> ParentPlants = new List<string>();
        public List<string> OffspringPlants = new List<string>();
        public string SelectionCriteria;
        public DateTime GenerationDate;
        public string Notes;
    }
    
    #endregion
    
    #region Supporting Enums
    
    public enum ScientificCompetitionType
    {
        BreedingChallenge,
        ResearchPresentation,
        InnovationContest,
        QualityCompetition,
        EfficiencyChallenge,
        CollaborativeProject
    }
    
    public enum GeneticResearchType
    {
        TraitMapping,
        BreedingOptimization,
        QualityImprovement,
        YieldEnhancement,
        ResistanceBreeding,
        NovelCompounds
    }
    
    
    public enum ScientificAchievementType
    {
        BreedingMilestone,
        ResearchBreakthrough,
        QualityAchievement,
        InnovationAward,
        CollaborationSuccess,
        TeachingExcellence
    }
    
    #endregion
}