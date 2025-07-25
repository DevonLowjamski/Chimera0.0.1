using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data;

namespace ProjectChimera.Systems.Gaming.Genetics
{
    /// <summary>
    /// Comprehensive data structures for Project Chimera's Enhanced Genetics Gaming System.
    /// Provides all necessary types for genetic manipulation and breeding gaming mechanics.
    /// </summary>

    #region Core Genetics Gaming Data

    // GeneticsGamingConfigSO moved to ProjectChimera.Data.Genetics namespace
    // Use the proper ScriptableObject implementation instead

    [Serializable]
    public class CrossSystemSynergy
    {
        public string SynergyId = Guid.NewGuid().ToString();
        public string SynergyName = "";
        public List<string> RequiredSystems = new List<string>();
        public float EfficiencyBonus = 1.2f;
        public Dictionary<string, float> SynergyEffects = new Dictionary<string, float>();
        public bool IsActive = false;
        public DateTime ActivatedAt = DateTime.Now;
    }

    #endregion

    #region Community System Types

    [Serializable]
    public class CommunityStanding
    {
        public string PlayerId = "";
        public float ReputationScore = 50f;
        public int ContributionPoints = 0;
        public CommunityRole Role = CommunityRole.Member;
        public List<string> Achievements = new List<string>();
        public DateTime LastActivity = DateTime.Now;
        public int HelpfulContributions = 0;
        public float TrustRating = 0.5f;
    }

    public enum CommunityRole
    {
        Visitor,
        Member,
        Contributor,
        Expert,
        Moderator,
        Leader,
        Pioneer
    }

    #endregion

    #region Skill and Training Systems

    public enum SensorySkillLevel
    {
        Novice,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }

    public enum BlendingSkillLevel
    {
        Basic,
        Apprentice,
        Journeyman,
        Artisan,
        Master,
        Grandmaster,
        Virtuoso
    }

    public enum SensoryTrainingStatus
    {
        Started,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    [Serializable]
    public class SensoryTrainingResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public SensorySkillLevel SkillLevel = SensorySkillLevel.Novice;
        public float AccuracyScore = 0f;
        public float TimeToComplete = 0f;
        public Dictionary<string, float> SensoryScores = new Dictionary<string, float>();
        public bool Passed = false;
        public string Feedback = "";
        public DateTime CompletedAt = DateTime.Now;
        public SensoryTrainingStatus Status = SensoryTrainingStatus.Started;
        
        // Static instances for compatibility
        public static SensoryTrainingStatus Failed = SensoryTrainingStatus.Failed;
        public static SensoryTrainingStatus Started = SensoryTrainingStatus.Started;
    }

    [Serializable]
    public class SensoryTrainingInfo
    {
        public string TrainingId = Guid.NewGuid().ToString();
        public string TrainingName = "";
        public SensorySkillLevel RequiredLevel = SensorySkillLevel.Novice;
        public List<string> TrainingModules = new List<string>();
        public float EstimatedDuration = 60f; // minutes
        public string TrainingDescription = "";
        public List<string> LearningObjectives = new List<string>();
        public Dictionary<string, object> TrainingData = new Dictionary<string, object>();
    }

    [Serializable]
    public class SensorySkillProgress
    {
        public string ProgressId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public SensorySkillLevel CurrentLevel = SensorySkillLevel.Novice;
        public float ExperiencePoints = 0f;
        public float ProgressToNextLevel = 0f;
        public List<string> CompletedTrainings = new List<string>();
        public Dictionary<string, float> SkillScores = new Dictionary<string, float>();
        public DateTime LastTraining = DateTime.Now;
        public int ConsecutiveSuccesses = 0;
    }

    [Serializable]
    public class SensorySkillEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string EventType = "";
        public SensorySkillLevel SkillLevelBefore = SensorySkillLevel.Novice;
        public SensorySkillLevel SkillLevelAfter = SensorySkillLevel.Novice;
        public float SkillImprovement = 0f;
        public DateTime EventTime = DateTime.Now;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }

    #endregion

    #region Event Data Structures

    [Serializable]
    public class GeneticDiscoveryEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string DiscoveryType = "";
        public string GeneDiscovered = "";
        public string TraitAffected = "";
        public float ImpactMagnitude = 1f;
        public string PlayerId = "";
        public DateTime DiscoveredAt = DateTime.Now;
        public bool IsSignificant = false;
        public Dictionary<string, object> DiscoveryData = new Dictionary<string, object>();
    }

    [Serializable]
    public class BreedingChallengeEventData
    {
        public string ChallengeId = Guid.NewGuid().ToString();
        public string ChallengeName = "";
        public string ChallengeType = "";
        public List<string> TargetTraits = new List<string>();
        public float DifficultyLevel = 0.5f;
        public TimeSpan TimeLimit = TimeSpan.FromHours(24);
        public Dictionary<string, object> ChallengeParameters = new Dictionary<string, object>();
        public List<string> Participants = new List<string>();
        public bool IsActive = true;
        public DateTime StartTime = DateTime.Now;
        public DateTime? EndTime = null;
    }

    [Serializable]
    public class BreedingChallengeInfo
    {
        public string ChallengeId = Guid.NewGuid().ToString();
        public string Title = "";
        public string Description = "";
        public List<string> Requirements = new List<string>();
        public Dictionary<string, float> TargetTraitRanges = new Dictionary<string, float>();
        public float DifficultyRating = 0.5f;
        public string ChallengeCategory = "";
        public float RewardMultiplier = 1.0f;
        public bool IsPublic = true;
    }

    [Serializable]
    public class BreedingChallengeProgress
    {
        public string ProgressId = Guid.NewGuid().ToString();
        public string ChallengeId = "";
        public string PlayerId = "";
        public float CompletionPercentage = 0f;
        public List<string> AchievedTraits = new List<string>();
        public List<string> RemainingTraits = new List<string>();
        public int BreedingAttempts = 0;
        public DateTime LastProgress = DateTime.Now;
        public bool IsCompleted = false;
    }

    [Serializable]
    public class AromaticMasteryEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string TerpeneProfile = "";
        public List<string> TerpenesIdentified = new List<string>();
        public float IdentificationAccuracy = 0f;
        public float BlendingSkillDemonstrated = 0f;
        public SensorySkillLevel SkillLevelAchieved = SensorySkillLevel.Novice;
        public bool MasteryAchieved = false;
        public string PlayerId = "";
        public DateTime EventTime = DateTime.Now;
        public Dictionary<string, float> TerpeneConcentrations = new Dictionary<string, float>();
    }

    [Serializable]
    public class CompetitionEntryEventData
    {
        public string EntryId = Guid.NewGuid().ToString();
        public string CompetitionId = "";
        public string PlayerId = "";
        public string StrainSubmitted = "";
        public Dictionary<string, float> TraitScores = new Dictionary<string, float>();
        public float OverallScore = 0f;
        public int Ranking = 0;
        public bool IsWinner = false;
        public DateTime SubmissionTime = DateTime.Now;
        public List<string> JudgeComments = new List<string>();
    }

    [Serializable]
    public class TournamentVictoryEventData
    {
        public string TournamentId = Guid.NewGuid().ToString();
        public string TournamentName = "";
        public string WinnerId = "";
        public string WinningStrain = "";
        public float WinningScore = 0f;
        public List<string> Participants = new List<string>();
        public Dictionary<string, float> FinalRankings = new Dictionary<string, float>();
        public DateTime TournamentDate = DateTime.Now;
        public string TournamentCategory = "";
        public Dictionary<string, object> TournamentStats = new Dictionary<string, object>();
    }

    #endregion

    #region Genetic Analysis and Breeding

    [Serializable]
    public class GeneticCombination
    {
        public string CombinationId = Guid.NewGuid().ToString();
        public string Parent1Id = "";
        public string Parent2Id = "";
        public List<string> ExpectedTraits = new List<string>();
        public Dictionary<string, float> TraitProbabilities = new Dictionary<string, float>();
        public float SuccessProbability = 0.5f;
        public bool IsViable = true;
        public string CombinationType = "";
        public DateTime CreatedAt = DateTime.Now;
    }

    [Serializable]
    public class BreedingAttemptResult
    {
        public string AttemptId = Guid.NewGuid().ToString();
        public string BreedingPairId = "";
        public bool WasSuccessful = false;
        public bool IsSuccessful = false; // Alias for WasSuccessful for consistency
        public float Score = 0f;
        public string OffspringId = "";
        public List<string> ExpressedTraits = new List<string>();
        public List<string> UnexpectedTraits = new List<string>();
        public float OverallQuality = 0f;
        public DateTime AttemptTime = DateTime.Now;
        public string FailureReason = "";
        public Dictionary<string, object> AttemptData = new Dictionary<string, object>();
    }

    #endregion

    #region Scientific Competition System

    [Serializable]
    public class ScientificCompetitionConfigSO : ScriptableObject
    {
        [Header("Competition Configuration")]
        public string CompetitionName = "";
        public CompetitionType Type = CompetitionType.Breeding;
        public float Duration = 168f; // 1 week in hours
        public int MaxParticipants = 100;
        public List<string> JudgingCriteria = new List<string>();
        public Dictionary<string, float> CriteriaWeights = new Dictionary<string, float>();
        public bool IsPublic = true;
        public float EntryFee = 0f;
        public List<CompetitionReward> Rewards = new List<CompetitionReward>();
    }

    public enum CompetitionType
    {
        Breeding,
        Identification,
        Innovation,
        Collaboration,
        Speed,
        Quality,
        Creativity,
        Research
    }

    [Serializable]
    public class CompetitionReward
    {
        public int Rank = 1;
        public string RewardType = "";
        public float RewardValue = 0f;
        public string RewardDescription = "";
        public bool IsSpecial = false;
    }

    [Serializable]
    public class CompetitionPerformance
    {
        public string PerformanceId = Guid.NewGuid().ToString();
        public string CompetitionId = "";
        public string PlayerId = "";
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public float OverallScore = 0f;
        public int CurrentRank = 0;
        public bool IsFinalized = false;
        public DateTime LastUpdate = DateTime.Now;
        public List<string> JudgeNotes = new List<string>();
    }

    [Serializable]
    public class CompetitionPerformanceResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string PerformanceId = "";
        public float FinalScore = 0f;
        public int FinalRank = 0;
        public bool IsWinner = false;
        public List<CompetitionReward> RewardsEarned = new List<CompetitionReward>();
        public string PerformanceSummary = "";
        public Dictionary<string, object> DetailedResults = new Dictionary<string, object>();
    }

    #endregion

    #region Analytics and Progress Tracking

    [Serializable]
    public class GeneticsProgressionData
    {
        public string PlayerId = "";
        public int SuccessfulBreedings = 0;
        public int DiscoveredTraits = 0;
        public int CompetitionsWon = 0;
        public float OverallSkillRating = 0f;
        public SensorySkillLevel SensoryLevel = SensorySkillLevel.Novice;
        public BlendingSkillLevel BlendingLevel = BlendingSkillLevel.Basic;
        public DateTime LastActivity = DateTime.Now;
        public List<string> UnlockedFeatures = new List<string>();
        public Dictionary<string, float> SkillProgression = new Dictionary<string, float>();
    }

    [Serializable]
    public class GeneticsAnalyticsData
    {
        public string SessionId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public DateTime SessionStart = DateTime.Now;
        public DateTime? SessionEnd = null;
        public int BreedingAttempts = 0;
        public int SuccessfulBreedings = 0;
        public int TraitsDiscovered = 0;
        public float TimeSpentBreeding = 0f;
        public float TimeSpentAnalyzing = 0f;
        public Dictionary<string, int> ActionsPerformed = new Dictionary<string, int>();
        public List<string> AchievementsUnlocked = new List<string>();
    }

    #endregion

    #region Aromatic and Terpene Systems

    [Serializable]
    public class AromaticGamingProfile
    {
        public string ProfileId = Guid.NewGuid().ToString();
        public string ProfileName = "";
        public Dictionary<string, float> TerpeneConcentrations = new Dictionary<string, float>();
        public List<string> DominantTerpenes = new List<string>();
        public string AromaticFamily = "";
        public float IntensityLevel = 0f;
        public float ComplexityScore = 0f;
        public DateTime CreatedAt = DateTime.Now;
        public string CreatedBy = "";
    }

    [Serializable]
    public class TerpeneIdentificationChallenge
    {
        public string ChallengeId = Guid.NewGuid().ToString();
        public List<string> TerpenesToIdentify = new List<string>();
        public List<string> Distractors = new List<string>();
        public float TimeLimit = 300f; // 5 minutes
        public SensorySkillLevel RequiredLevel = SensorySkillLevel.Beginner;
        public float PassingScore = 0.7f;
        public Dictionary<string, string> SensoryDescriptions = new Dictionary<string, string>();
    }

    [Serializable]
    public class TerpeneIdentificationAttempt
    {
        public string AttemptId = Guid.NewGuid().ToString();
        public string ChallengeId = "";
        public string PlayerId = "";
        public List<string> IdentifiedTerpenes = new List<string>();
        public float AccuracyScore = 0f;
        public float TimeUsed = 0f;
        public bool Passed = false;
        public DateTime AttemptTime = DateTime.Now;
        public Dictionary<string, float> ConfidenceLevels = new Dictionary<string, float>();
    }

    [Serializable]
    public class TerpeneIdentificationResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string AttemptId = "";
        public float FinalScore = 0f;
        public SensorySkillLevel SkillLevelAchieved = SensorySkillLevel.Novice;
        public List<string> CorrectIdentifications = new List<string>();
        public List<string> MissedTerpenes = new List<string>();
        public List<string> FalsePositives = new List<string>();
        public string DetailedFeedback = "";
        public bool SkillImproved = false;
        
        // Additional properties for terpene identification
        public bool IsCorrect = false;
        public TerpeneCategory TerpeneCategory = TerpeneCategory.Monoterpenes;
        public float ConfidenceLevel = 0f;
        public string SubmittedTerpene = "";
        public string CorrectTerpene = "";
    }

    [Serializable]
    public class TerpeneComposition
    {
        public string CompositionId = Guid.NewGuid().ToString();
        public string CompositionName = "";
        public Dictionary<string, float> TerpeneRatios = new Dictionary<string, float>();
        public float TotalConcentration = 0f;
        public string DominantTerpene = "";
        public List<string> AromaticNotes = new List<string>();
        public string CompositionType = "";
        public DateTime CreatedAt = DateTime.Now;
    }

    [Serializable]
    public class AromaticObjective
    {
        public string ObjectiveId = Guid.NewGuid().ToString();
        public string ObjectiveName = "";
        public string Description = "";
        public List<string> TargetTerpenes = new List<string>();
        public Dictionary<string, float> TargetRatios = new Dictionary<string, float>();
        public float DifficultyLevel = 0.5f;
        public bool IsCompleted = false;
        public DateTime DueDate = DateTime.Now.AddDays(7);
        public float CompletionReward = 100f;
    }

    [Serializable]
    public class BlendingProjectSpec
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public List<string> RequiredTerpenes = new List<string>();
        public Dictionary<string, float> TargetComposition = new Dictionary<string, float>();
        public float DifficultyRating = 0.5f;
        public TimeSpan TimeLimit = TimeSpan.FromHours(2);
        public string ProjectDescription = "";
        public List<string> QualityCriteria = new List<string>();
    }

    [Serializable]
    public class BlendingProjectResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public string PlayerId = "";
        public Dictionary<string, float> AchievedComposition = new Dictionary<string, float>();
        public float AccuracyScore = 0f;
        public float CreativityScore = 0f;
        public float OverallScore = 0f;
        public bool ProjectCompleted = false;
        public DateTime CompletionTime = DateTime.Now;
        public string JudgeFeedback = "";
    }

    #endregion

    #region Collaboration and Social Features

    [Serializable]
    public class CollaborationProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string ProjectDescription = "";
        public List<string> Participants = new List<string>();
        public string ProjectLeader = "";
        public List<string> Goals = new List<string>();
        public float Progress = 0f;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public bool IsPublic = true;
        public Dictionary<string, object> ProjectData = new Dictionary<string, object>();
    }

    [Serializable]
    public class KnowledgeContribution
    {
        public string ContributionId = Guid.NewGuid().ToString();
        public string ContributorId = "";
        public string ContributionType = "";
        public string Content = "";
        public List<string> Tags = new List<string>();
        public int UpVotes = 0;
        public int DownVotes = 0;
        public bool IsVerified = false;
        public DateTime ContributedAt = DateTime.Now;
        public Dictionary<string, object> ContributionData = new Dictionary<string, object>();
    }

    #endregion

    #region Additional Gaming Types

    [Serializable]
    public class ScientificInnovation
    {
        public string InnovationId = Guid.NewGuid().ToString();
        public string InnovationName = "";
        public string Description = "";
        public string InnovationType = "";
        public string InnovatorId = "";
        public float ImpactScore = 0f;
        public List<string> Applications = new List<string>();
        public DateTime DiscoveredAt = DateTime.Now;
        public bool IsValidated = false;
        public Dictionary<string, object> InnovationData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipCriteria
    {
        public string CriteriaId = Guid.NewGuid().ToString();
        public SensorySkillLevel MinimumSkillLevel = SensorySkillLevel.Advanced;
        public int MinimumExperience = 100;
        public float MinimumRating = 4.0f;
        public List<string> RequiredSkills = new List<string>();
        public List<string> SpecializationAreas = new List<string>();
        public bool RequiresCertification = false;
        public int MaxMentees = 5;
    }

    #endregion

    #region Supporting Enums and Types

    public enum GeneticDiscoveryType
    {
        NewGene,
        GeneInteraction,
        TraitExpression,
        EpigeneticEffect,
        MutationPattern,
        HeritabilityFactor
    }


    public enum TraitCategory
    {
        Morphological,
        Physiological,
        Chemical,
        Resistance,
        Yield,
        Quality,
        Environmental
    }

    public enum TerpeneCategory
    {
        Monoterpenes,
        Sesquiterpenes,
        Diterpenes,
        Triterpenes,
        Esters,
        Aldehydes,
        Alcohols,
        Phenols,
        Ketones,
        Acids
    }

    #endregion

    #region Additional Missing Types - Wave 3

    [Serializable]
    public class NovelAromaticCombination
    {
        public string CombinationId = Guid.NewGuid().ToString();
        public string CombinationName = "";
        public Dictionary<string, float> TerpeneRatios = new Dictionary<string, float>();
        public float NoveltyScore = 0f;
        public string DiscoveredBy = "";
        public DateTime DiscoveryDate = DateTime.Now;
        public bool IsVerified = false;
        public float PopularityScore = 0f;
    }

    [Serializable]
    public class TerpeneSynergy
    {
        public string SynergyId = Guid.NewGuid().ToString();
        public List<string> TerpeneComponents = new List<string>();
        public Dictionary<string, float> SynergyRatios = new Dictionary<string, float>();
        public float SynergyStrength = 0f;
        public string SynergyType = "";
        public string DiscoveredBy = "";
        public DateTime DiscoveredAt = DateTime.Now;
    }

    [Serializable]
    public class MentorshipMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneTitle = "";
        public string MilestoneDescription = "";
        public string MentorId = "";
        public string MenteeId = "";
        public bool IsAchieved = false;
        public DateTime AchievedAt = DateTime.Now;
        public float DifficultyLevel = 0.5f;
        public string Category = "";
    }

    [Serializable]
    public class MentorshipMilestoneResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string MilestoneId = "";
        public bool Success = false;
        public float ProgressScore = 0f;
        public string Feedback = "";
        public DateTime CompletedAt = DateTime.Now;
        public Dictionary<string, object> ResultData = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingObjective
    {
        public string ObjectiveId = Guid.NewGuid().ToString();
        public string ObjectiveName = "";
        public string Description = "";
        public List<string> RequiredTerpenes = new List<string>();
        public Dictionary<string, float> TargetRatios = new Dictionary<string, float>();
        public float DifficultyLevel = 0.5f;
        public float RewardPoints = 100f;
        public DateTime DueDate = DateTime.Now.AddDays(7);
    }


    [Serializable]
    public class BlendingProjectType
    {
        public string TypeId = Guid.NewGuid().ToString();
        public string TypeName = "";
        public string Description = "";
        public float DifficultyMultiplier = 1.0f;
        public List<string> RequiredSkills = new List<string>();
        public Dictionary<string, object> ProjectParameters = new Dictionary<string, object>();
    }

    [Serializable]
    public class CollaborativeProjectSpec
    {
        public string SpecId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string ProjectDescription = "";
        public List<string> RequiredRoles = new List<string>();
        public int MaxParticipants = 5;
        public float EstimatedDuration = 168f; // hours
        public Dictionary<string, object> ProjectGoals = new Dictionary<string, object>();
        public float DifficultyLevel = 0.5f;
    }

    [Serializable]
    public class CollaborativeProjectResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public bool Success = false;
        public float OverallScore = 0f;
        public Dictionary<string, float> ParticipantScores = new Dictionary<string, float>();
        public DateTime CompletedAt = DateTime.Now;
        public string ProjectSummary = "";
        public List<string> Achievements = new List<string>();
    }

    [Serializable]
    public class AromaticDiscovery
    {
        public string DiscoveryId = Guid.NewGuid().ToString();
        public string DiscoveryName = "";
        public string DiscoveryType = "";
        public Dictionary<string, float> TerpeneProfile = new Dictionary<string, float>();
        public string DiscoveredBy = "";
        public DateTime DiscoveryDate = DateTime.Now;
        public float SignificanceScore = 0f;
        public bool IsVerified = false;
    }

    [Serializable]
    public class GeneticInnovation
    {
        public string InnovationId = Guid.NewGuid().ToString();
        public string InnovationName = "";
        public string InnovationType = "";
        public string InnovatorId = "";
        public Dictionary<string, object> InnovationData = new Dictionary<string, object>();
        public float ImpactScore = 0f;
        public DateTime CreatedAt = DateTime.Now;
        public bool IsPublished = false;
        public List<string> Applications = new List<string>();
    }

    [Serializable]
    public class TerpeneIdentificationEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string TerpeneIdentified = "";
        public float IdentificationAccuracy = 0f;
        public float TimeToIdentify = 0f;
        public bool WasCorrect = false;
        public DateTime EventTime = DateTime.Now;
        public string IdentificationMethod = "";
    }

    [Serializable]
    public class InnovationSharingEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string InnovationId = "";
        public string PlayerId = "";
        public string InnovationType = "";
        public float InnovationScore = 0f;
        public bool IsVerified = false;
        public DateTime SharedAt = DateTime.Now;
        public Dictionary<string, object> InnovationData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string MentorId = "";
        public string MenteeId = "";
        public string MentorshipType = "";
        public bool IsEstablished = false;
        public DateTime EventTime = DateTime.Now;
        public Dictionary<string, object> MentorshipDetails = new Dictionary<string, object>();
    }

    [Serializable]
    public class ReputationGainEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float ReputationGain = 0f;
        public string ReputationSource = "";
        public string AchievementId = "";
        public DateTime EventTime = DateTime.Now;
        public Dictionary<string, object> ReputationData = new Dictionary<string, object>();
    }

    #endregion

    #region Additional Missing Types - Wave 4

    [Serializable]
    public class ScientificProgressionConfigSO : ScriptableObject
    {
        [Header("Scientific Progression Configuration")]
        public float ProgressionRate = 1.0f;
        public int MaxSkillLevel = 100;
        public bool EnableCrossSystemProgression = true;
        public List<string> ProgressionCategories = new List<string>();
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class SkillNode
    {
        public string NodeId = Guid.NewGuid().ToString();
        public string NodeName = "";
        public string Description = "";
        public int RequiredLevel = 1;
        public List<string> Prerequisites = new List<string>();
        public Dictionary<string, object> SkillEffects = new Dictionary<string, object>();
        public bool IsUnlocked = false;
        public float ProgressPercentage = 0f;
    }

    [Serializable]
    public class AchievementTriggerData
    {
        public string TriggerId = Guid.NewGuid().ToString();
        public string AchievementId = "";
        public string TriggerType = "";
        public Dictionary<string, object> TriggerConditions = new Dictionary<string, object>();
        public DateTime TriggeredAt = DateTime.Now;
        public string PlayerId = "";
        public bool IsCompleted = false;
    }

    [Serializable]
    public class CrossSystemAchievementData
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public List<string> RequiredSystems = new List<string>();
        public Dictionary<string, object> SystemRequirements = new Dictionary<string, object>();
        public float CompletionPercentage = 0f;
        public bool IsUnlocked = false;
        public DateTime UnlockedAt = DateTime.Now;
    }

    [Serializable]
    public class BreedingChallengeManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public List<BreedingChallenge> ActiveChallenges = new List<BreedingChallenge>();
        public Dictionary<string, float> ChallengeProgress = new Dictionary<string, float>();
        public bool IsInitialized = false;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class ScientificAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public string Description = "";
        public string Category = "";
        public float DifficultyLevel = 0.5f;
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
        public bool IsHidden = false;
        public float PointValue = 100f;
        public DateTime CreatedAt = DateTime.Now;
    }

    [Serializable]
    public class GeneticDiscoveryEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public List<GeneticDiscovery> RecentDiscoveries = new List<GeneticDiscovery>();
        public Dictionary<string, float> DiscoveryProbabilities = new Dictionary<string, float>();
        public bool IsActive = true;
        public DateTime LastDiscovery = DateTime.Now;
        public float DiscoveryRate = 0.1f;
    }

    [Serializable]
    public class TargetTrait
    {
        public string TraitId = Guid.NewGuid().ToString();
        public string TraitName = "";
        public string TraitDescription = "";
        public float TargetValue = 0f;
        public float CurrentValue = 0f;
        public float Tolerance = 0.1f;
        public bool IsAchieved = false;
        public DateTime TargetDate = DateTime.Now.AddDays(30);
    }

    [Serializable]
    public class SensoryTrainingSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public List<SensoryChallenge> AvailableChallenges = new List<SensoryChallenge>();
        public Dictionary<string, SensorySkillProgress> PlayerProgress = new Dictionary<string, SensorySkillProgress>();
        public bool IsInitialized = false;
        public float DifficultyAdjustmentRate = 0.1f;
    }

    [Serializable]
    public class VisualGeneticInterface
    {
        public string InterfaceId = Guid.NewGuid().ToString();
        public bool IsInitialized = false;
        public Dictionary<string, object> VisualizationSettings = new Dictionary<string, object>();
        public List<string> DisplayedTraits = new List<string>();
        public float ZoomLevel = 1.0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class AchievementTriggerData2
    {
        public string TriggerId = Guid.NewGuid().ToString();
        public string EventType = "";
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public string PlayerId = "";
        public DateTime EventTime = DateTime.Now;
        public bool WasProcessed = false;
    }

    #endregion

    #region Additional Missing Types - Wave 5

    [Serializable]
    public class CompetitiveReputation
    {
        public string ReputationId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float OverallRating = 0f;
        public Dictionary<string, float> CategoryRatings = new Dictionary<string, float>();
        public int TotalCompetitions = 0;
        public int WinCount = 0;
        public float WinRate = 0f;
        public DateTime LastUpdate = DateTime.Now;
        public List<string> Achievements = new List<string>();
    }

    [Serializable]
    public class CompetitiveAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public string Description = "";
        public string Category = "";
        public float RequiredRating = 0f;
        public int RequiredWins = 0;
        public bool IsUnlocked = false;
        public DateTime UnlockedAt = DateTime.Now;
        public string IconPath = "";
    }

    [Serializable]
    public class ProjectRole
    {
        public string RoleId = Guid.NewGuid().ToString();
        public string RoleName = "";
        public string Description = "";
        public List<string> Responsibilities = new List<string>();
        public List<string> RequiredSkills = new List<string>();
        public float ResponsibilityWeight = 1.0f;
        public bool IsLeaderRole = false;
    }

    [Serializable]
    public class ProjectJoinResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public string PlayerId = "";
        public bool Success = false;
        public string AssignedRole = "";
        public string Message = "";
        public DateTime JoinedAt = DateTime.Now;
        public int TeamPosition = 0;
    }

    [Serializable]
    public class CompetitiveStatistics
    {
        public string StatsId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, int> CompetitionCounts = new Dictionary<string, int>();
        public Dictionary<string, float> AverageScores = new Dictionary<string, float>();
        public Dictionary<string, DateTime> PersonalBests = new Dictionary<string, DateTime>();
        public float OverallRank = 0f;
        public DateTime LastCompetition = DateTime.Now;
    }

    // AromaticCreationStudio moved to individual MonoBehaviour component file

    [Serializable]
    public class ProjectContribution
    {
        public string ContributionId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public string ContributorId = "";
        public string ContributionType = "";
        public Dictionary<string, object> ContributionData = new Dictionary<string, object>();
        public float ContributionValue = 0f;
        public DateTime ContributedAt = DateTime.Now;
        public bool IsVerified = false;
    }

    [Serializable]
    public class ProjectContributionResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ContributionId = "";
        public bool WasAccepted = false;
        public float QualityScore = 0f;
        public string Feedback = "";
        public float RewardPoints = 0f;
        public DateTime ProcessedAt = DateTime.Now;
    }

    [Serializable]
    public class TerpeneAnalysisGamingSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, TerpeneIdentificationChallenge> ActiveChallenges = new Dictionary<string, TerpeneIdentificationChallenge>();
        public Dictionary<string, AromaticAnalysisResult> RecentAnalyses = new Dictionary<string, AromaticAnalysisResult>();
        public bool IsInitialized = false;
        public float AnalysisAccuracyThreshold = 0.8f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class CollaborativeProjectInfo
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string ProjectDescription = "";
        public List<ProjectRole> AvailableRoles = new List<ProjectRole>();
        public List<string> CurrentParticipants = new List<string>();
        public int MaxParticipants = 5;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public string ProjectStatus = "Open";
    }

    [Serializable]
    public class TournamentEventManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public List<ScheduledTournament> UpcomingTournaments = new List<ScheduledTournament>();
        public Dictionary<string, TournamentBracket> ActiveBrackets = new Dictionary<string, TournamentBracket>();
        public bool IsInitialized = false;
        public DateTime LastTournamentCheck = DateTime.Now;
    }

    [Serializable]
    public class PeerRating
    {
        public string RatingId = Guid.NewGuid().ToString();
        public string RaterId = "";
        public string RatedPlayerId = "";
        public float OverallRating = 0f;
        public Dictionary<string, float> CategoryRatings = new Dictionary<string, float>();
        public string Comments = "";
        public DateTime RatedAt = DateTime.Now;
        public bool IsVerified = false;
    }

    #endregion

    #region Final Missing Types - Wave 6

    [Serializable]
    public class GeneticProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string ProjectDescription = "";
        public List<string> TargetGenes = new List<string>();
        public Dictionary<string, float> ProjectGoals = new Dictionary<string, float>();
        public float CompletionPercentage = 0f;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public string ProjectLeader = "";
        public List<string> Collaborators = new List<string>();
    }

    [Serializable]
    public class TraitExpressionAnalysis
    {
        public string AnalysisId = Guid.NewGuid().ToString();
        public string TraitName = "";
        public Dictionary<string, float> ExpressionLevels = new Dictionary<string, float>();
        public float ConfidenceLevel = 0f;
        public string AnalysisMethod = "";
        public DateTime AnalysisDate = DateTime.Now;
        public string AnalyzedBy = "";
        public bool IsValidated = false;
    }

    [Serializable]
    public class PhenotypePrediction
    {
        public string PredictionId = Guid.NewGuid().ToString();
        public string PredictedTrait = "";
        public float PredictedValue = 0f;
        public float ConfidenceInterval = 0f;
        public Dictionary<string, float> GeneticFactors = new Dictionary<string, float>();
        public DateTime PredictionDate = DateTime.Now;
        public string PredictionModel = "";
        public bool WasAccurate = false;
    }

    [Serializable]
    public class StabilityMarker
    {
        public string MarkerId = Guid.NewGuid().ToString();
        public string MarkerName = "";
        public string ChromosomeLocation = "";
        public float StabilityScore = 0f;
        public List<string> AssociatedTraits = new List<string>();
        public bool IsValidated = false;
        public DateTime DiscoveredAt = DateTime.Now;
        public string DiscoveredBy = "";
    }

    [Serializable]
    public class GeneticInnovationType
    {
        public string TypeId = Guid.NewGuid().ToString();
        public string TypeName = "";
        public string Description = "";
        public float InnovationScore = 0f;
        public List<string> RequiredTechnologies = new List<string>();
        public Dictionary<string, object> TypeProperties = new Dictionary<string, object>();
        public bool IsExperimental = true;
    }

    [Serializable]
    public class GeneticInnovationEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string InnovatorId = "";
        public GeneticInnovationType InnovationType = null;
        public Dictionary<string, object> InnovationDetails = new Dictionary<string, object>();
        public float ImpactScore = 0f;
        public DateTime EventTime = DateTime.Now;
        public bool IsPublished = false;
        public List<string> PeerReviews = new List<string>();
    }

    [Serializable]
    public class AromaticIdentificationEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, float> IdentificationAccuracy = new Dictionary<string, float>();
        public List<TerpeneIdentificationChallenge> ActiveChallenges = new List<TerpeneIdentificationChallenge>();
        public bool IsInitialized = false;
        public float LearningRate = 0.1f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class SensoryMemoryTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, DateTime> LastIdentifications = new Dictionary<string, DateTime>();
        public Dictionary<string, float> IdentificationAccuracies = new Dictionary<string, float>();
        public float MemoryRetentionScore = 0f;
        public bool IsActive = true;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class BreedingChallengeLibrarySO : ScriptableObject
    {
        [Header("Breeding Challenge Library")]
        public List<BreedingChallenge> AvailableChallenges = new List<BreedingChallenge>();
        public List<BreedingChallengeInfo> ChallengeTemplates = new List<BreedingChallengeInfo>();
        public Dictionary<string, float> DifficultyMultipliers = new Dictionary<string, float>();
        public float BaseRewardPoints = 100f;
    }

    [Serializable]
    public class PeerEndorsement
    {
        public string EndorsementId = Guid.NewGuid().ToString();
        public string EndorserId = "";
        public string EndorsedPlayerId = "";
        public string EndorsementType = "";
        public string EndorsementText = "";
        public float EndorsementStrength = 0f;
        public DateTime EndorsementDate = DateTime.Now;
        public bool IsVerified = false;
        public List<string> SupportingEvidence = new List<string>();
    }

    [Serializable]
    public class PeerEndorsementResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string EndorsementId = "";
        public bool WasAccepted = false;
        public float ReputationImpact = 0f;
        public string Feedback = "";
        public DateTime ProcessedAt = DateTime.Now;
        public Dictionary<string, object> ResultMetrics = new Dictionary<string, object>();
    }

    #endregion

    #region Final Complete Types - Wave 7

    [Serializable]
    public class PeerRatingResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string RatingId = "";
        public bool WasAccepted = false;
        public float ReputationChange = 0f;
        public string Feedback = "";
        public DateTime ProcessedAt = DateTime.Now;
        public Dictionary<string, object> ProcessingMetrics = new Dictionary<string, object>();
    }

    [Serializable]
    public class CompetitionEntry
    {
        public string EntryId = Guid.NewGuid().ToString();
        public string CompetitionId = "";
        public string ParticipantId = "";
        public string EntryName = "";
        public Dictionary<string, object> EntryData = new Dictionary<string, object>();
        public float EntryScore = 0f;
        public DateTime SubmittedAt = DateTime.Now;
        public bool IsQualified = true;
        public string EntryCategory = "";
    }

    [Serializable]
    public class CompetitiveMatchmakingSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, float> PlayerRatings = new Dictionary<string, float>();
        public List<string> MatchmakingQueue = new List<string>();
        public bool IsActive = true;
        public float MatchmakingTolerance = 0.2f;
        public DateTime LastMatchUpdate = DateTime.Now;
    }



    [Serializable]
    public class ExpertConsultationRequest
    {
        public string RequestId = Guid.NewGuid().ToString();
        public string RequesterId = "";
        public string ExpertArea = "";
        public string ConsultationTopic = "";
        public string RequestDescription = "";
        public float UrgencyLevel = 0.5f;
        public DateTime RequestedAt = DateTime.Now;
        public string Status = "Pending";
        public List<string> PreferredExperts = new List<string>();
    }

    [Serializable]
    public class ExpertConsultationResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string RequestId = "";
        public string ExpertId = "";
        public string ConsultationNotes = "";
        public List<string> Recommendations = new List<string>();
        public float ConsultationRating = 0f;
        public DateTime CompletedAt = DateTime.Now;
        public bool WasHelpful = false;
    }

    [Serializable]
    public class Tournament
    {
        public string TournamentId = Guid.NewGuid().ToString();
        public string TournamentName = "";
        public CompetitionType Type = CompetitionType.Breeding;
        public List<string> Participants = new List<string>();
        public TournamentBracket Bracket = null;
        public DateTime StartTime = DateTime.Now;
        public DateTime? EndTime = null;
        public string Status = "Registration";
        public Dictionary<string, object> TournamentRules = new Dictionary<string, object>();
    }

    [Serializable]
    public class SocialRecognitionSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, CompetitiveReputation> PlayerReputations = new Dictionary<string, CompetitiveReputation>();
        public List<PeerEndorsement> RecentEndorsements = new List<PeerEndorsement>();
        public bool IsInitialized = false;
        public float ReputationDecayRate = 0.01f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class GeneticDiscoveryDatabaseSO : ScriptableObject
    {
        [Header("Genetic Discovery Database")]
        public List<GeneticDiscovery> KnownDiscoveries = new List<GeneticDiscovery>();
        public List<NovelTrait> DocumentedTraits = new List<NovelTrait>();
        public Dictionary<string, float> DiscoveryProbabilities = new Dictionary<string, float>();
        public float BaseDiscoveryRate = 0.05f;
        public int MaxDiscoveriesPerSession = 3;
    }

    #endregion

    #region Absolute Final Types - Wave 8

    [Serializable]
    public class KnowledgeContributionResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ContributionId = "";
        public bool WasAccepted = false;
        public float QualityScore = 0f;
        public string ReviewerFeedback = "";
        public float ReputationGain = 0f;
        public DateTime ProcessedAt = DateTime.Now;
        public Dictionary<string, object> ProcessingData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipRelationship
    {
        public string RelationshipId = Guid.NewGuid().ToString();
        public string MentorId = "";
        public string MenteeId = "";
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public string RelationshipStatus = "Active";
        public float ProgressScore = 0f;
        public List<MentorshipMilestone> CompletedMilestones = new List<MentorshipMilestone>();
        public Dictionary<string, object> RelationshipData = new Dictionary<string, object>();
    }

    [Serializable]
    public class TournamentEntryRequirements
    {
        public string RequirementsId = Guid.NewGuid().ToString();
        public string TournamentId = "";
        public float MinimumRating = 0f;
        public int MinimumExperience = 0;
        public List<string> RequiredAchievements = new List<string>();
        public List<string> RequiredSkills = new List<string>();
        public float EntryFee = 0f;
        public DateTime RegistrationDeadline = DateTime.Now.AddDays(7);
    }



    [Serializable]
    public class CollaborativeProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string ProjectDescription = "";
        public List<string> TeamMembers = new List<string>();
        public string ProjectLeader = "";
        public CollaborativeProjectSpec ProjectSpec = null;
        public CollaborativeProjectResult ProjectResult = null;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public string ProjectStatus = "Planning";
    }

    [Serializable]
    public class TournamentLibrarySO : ScriptableObject
    {
        [Header("Tournament Library")]
        public List<TournamentSpec> TournamentTemplates = new List<TournamentSpec>();
        public List<CompetitionType> AvailableCompetitionTypes = new List<CompetitionType>();
        public Dictionary<string, float> DifficultyScaling = new Dictionary<string, float>();
        public float BaseTournamentDuration = 168f; // 1 week
        public int MaxParticipantsDefault = 64;
    }

    [Serializable]
    public class CompetitionRewardsLibrarySO : ScriptableObject
    {
        [Header("Competition Rewards Library")]
        public List<CompetitionReward> AvailableRewards = new List<CompetitionReward>();
        public Dictionary<string, float> RewardMultipliers = new Dictionary<string, float>();
        public float BaseRewardValue = 100f;
        public List<string> RewardCategories = new List<string>();
    }

    [Serializable]
    public class TournamentScheduler
    {
        public string SchedulerId = Guid.NewGuid().ToString();
        public List<ScheduledTournament> UpcomingTournaments = new List<ScheduledTournament>();
        public Dictionary<string, DateTime> TournamentCalendar = new Dictionary<string, DateTime>();
        public bool IsActive = true;
        public float SchedulingInterval = 24f; // hours
        public DateTime LastScheduleUpdate = DateTime.Now;
    }

    [Serializable]
    public class KnowledgeContributionResult2
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string ContributionId = "";
        public float ImpactScore = 0f;
        public int CommunityVotes = 0;
        public bool IsVerified = false;
        public string VerificationStatus = "Pending";
        public DateTime LastUpdate = DateTime.Now;
    }

    // Note: BreedingProjectSpec is defined in the Aromatic systems section as BlendingProjectSpec
    // This alias resolves the reference issue
    public class BreedingProjectSpec : BlendingProjectSpec { }
    public class BreedingProjectResult : BlendingProjectResult { }

    #endregion

    #region Additional Missing Gaming System Types

    [Serializable]
    public class AromaticAnalysisResult
    {
        public string AnalysisId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, float> TerpeneConcentrations = new Dictionary<string, float>();
        public float AnalysisAccuracy = 0f;
        public DateTime AnalysisTime = DateTime.Now;
        public bool IsSuccessful = false;
        public string AnalysisMethod = "";
    }

    [Serializable]
    public class BlendingOpportunity
    {
        public string OpportunityId = Guid.NewGuid().ToString();
        public string OpportunityName = "";
        public List<string> RequiredTerpenes = new List<string>();
        public Dictionary<string, float> TargetRatios = new Dictionary<string, float>();
        public float DifficultyLevel = 0.5f;
        public float RewardPoints = 100f;
        public DateTime AvailableUntil = DateTime.Now.AddDays(7);
    }

    [Serializable]
    public class SensoryChallenge
    {
        public string ChallengeId = Guid.NewGuid().ToString();
        public string ChallengeName = "";
        public SensorySkillLevel RequiredLevel = SensorySkillLevel.Novice;
        public List<string> TestSubstances = new List<string>();
        public float TimeLimit = 300f; // 5 minutes
        public float PassingScore = 0.7f;
        public string ChallengeDescription = "";
    }

    [Serializable]
    public class BlendingProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string PlayerId = "";
        public BlendingProjectSpec Specification = null;
        public BlendingProjectResult Result = null;
        public DateTime StartTime = DateTime.Now;
        public DateTime? CompletionTime = null;
        public bool IsCompleted = false;
    }

    [Serializable]
    public class ScheduledTournament
    {
        public string TournamentId = Guid.NewGuid().ToString();
        public string TournamentName = "";
        public CompetitionType Type = CompetitionType.Breeding;
        public DateTime ScheduledStart = DateTime.Now;
        public DateTime ScheduledEnd = DateTime.Now.AddDays(7);
        public int MaxParticipants = 100;
        public bool IsRegistrationOpen = true;
        public List<string> RegisteredParticipants = new List<string>();
    }

    [Serializable]
    public class TournamentSpec
    {
        public string SpecId = Guid.NewGuid().ToString();
        public string TournamentName = "";
        public CompetitionType Type = CompetitionType.Breeding;
        public Dictionary<string, object> Rules = new Dictionary<string, object>();
        public float Duration = 168f; // 1 week in hours
        public List<string> JudgingCriteria = new List<string>();
        public Dictionary<string, float> CriteriaWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class TournamentCreationResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public bool Success = false;
        public string TournamentId = "";
        public string Message = "";
        public DateTime CreatedAt = DateTime.Now;
        public Dictionary<string, object> TournamentDetails = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipGuidance
    {
        public string GuidanceId = Guid.NewGuid().ToString();
        public string MentorId = "";
        public string MenteeId = "";
        public string GuidanceType = "";
        public string GuidanceContent = "";
        public float EffectivenessScore = 0f;
        public DateTime ProvidedAt = DateTime.Now;
    }

    [Serializable]
    public class MentorshipGuidanceResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string GuidanceId = "";
        public bool WasHelpful = false;
        public float ImprovementScore = 0f;
        public string MenteeFeedback = "";
        public DateTime CompletedAt = DateTime.Now;
    }

    #endregion

    #region Additional Missing Types - Wave 2

    [Serializable]
    public class BreedingChallenge
    {
        public string ChallengeId = Guid.NewGuid().ToString();
        public string ChallengeName = "";
        public string ChallengeDescription = "";
        public List<string> TargetTraits = new List<string>();
        public Dictionary<string, float> TargetValues = new Dictionary<string, float>();
        public float DifficultyLevel = 0.5f;
        public DateTime StartDate = DateTime.Now;
        public DateTime EndDate = DateTime.Now.AddDays(30);
        public bool IsActive = true;
        public List<string> Participants = new List<string>();
        public string ChallengeCategory = "";
        public float RewardMultiplier = 1.0f;
    }

    [Serializable]
    public class TournamentJoinResult
    {
        public string ResultId = Guid.NewGuid().ToString();
        public string TournamentId = "";
        public string PlayerId = "";
        public bool Success = false;
        public string Message = "";
        public DateTime JoinedAt = DateTime.Now;
        public int PlayerPosition = 0;
        public bool IsWaitlisted = false;
    }

    [Serializable]
    public class TournamentBracket
    {
        public string BracketId = Guid.NewGuid().ToString();
        public string TournamentId = "";
        public List<string> Participants = new List<string>();
        public Dictionary<string, string> Matchups = new Dictionary<string, string>();
        public Dictionary<string, string> Results = new Dictionary<string, string>();
        public int CurrentRound = 1;
        public int TotalRounds = 4;
        public bool IsCompleted = false;
    }

    [Serializable]
    public class TournamentLeaderboard
    {
        public string LeaderboardId = Guid.NewGuid().ToString();
        public string TournamentId = "";
        public List<TournamentRanking> Rankings = new List<TournamentRanking>();
        public DateTime LastUpdated = DateTime.Now;
        public bool IsFinal = false;
    }

    [Serializable]
    public class TournamentRanking
    {
        public string PlayerId = "";
        public string PlayerName = "";
        public int Rank = 0;
        public float Score = 0f;
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public bool IsQualified = true;
    }

    [Serializable]
    public class NovelTrait
    {
        public string TraitId = Guid.NewGuid().ToString();
        public string TraitName = "";
        public string TraitDescription = "";
        public TraitCategory Category = TraitCategory.Morphological;
        public float Rarity = 0.1f;
        public string DiscoveredBy = "";
        public DateTime DiscoveryDate = DateTime.Now;
        public Dictionary<string, object> TraitProperties = new Dictionary<string, object>();
        public bool IsVerified = false;
        public float ImpactScore = 0f;
    }

    #endregion

    #region Missing Gaming System Types

    [Serializable]
    public class GeneticAnalysisResult
    {
        public string AnalysisId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, float> GeneticTraitScores = new Dictionary<string, float>();
        public List<string> IdentifiedGenes = new List<string>();
        public float AnalysisAccuracy = 0f;
        public DateTime AnalysisTime = DateTime.Now;
        public string AnalysisMethod = "";
        public bool IsSuccessful = false;
    }

    [Serializable]
    public class GeneticDiscovery
    {
        public string DiscoveryId = Guid.NewGuid().ToString();
        public string DiscoveryName = "";
        public string DiscoveryType = "";
        public string DiscoveredBy = "";
        public DateTime DiscoveryDate = DateTime.Now;
        public float SignificanceScore = 0f;
        public Dictionary<string, object> DiscoveryData = new Dictionary<string, object>();
        public bool IsVerified = false;
    }

    [Serializable]
    public class AromaticBlendResult
    {
        public string BlendId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, float> TerpeneComposition = new Dictionary<string, float>();
        public float QualityScore = 0f;
        public float CreativityScore = 0f;
        public DateTime CreatedAt = DateTime.Now;
        public bool MeetsTargetProfile = false;
        public string BlendName = "";
    }

    [Serializable]
    public class CompetitionInfo
    {
        public string CompetitionId = Guid.NewGuid().ToString();
        public string CompetitionName = "";
        public CompetitionType Type = CompetitionType.Breeding;
        public DateTime StartDate = DateTime.Now;
        public DateTime EndDate = DateTime.Now.AddDays(7);
        public List<string> Participants = new List<string>();
        public Dictionary<string, object> Rules = new Dictionary<string, object>();
        public bool IsActive = true;
        public string Description = "";
    }

    [Serializable]
    public class MentorshipRequest
    {
        public string RequestId = Guid.NewGuid().ToString();
        public string MenteeId = "";
        public string PreferredMentorId = "";
        public List<string> LearningGoals = new List<string>();
        public SensorySkillLevel CurrentLevel = SensorySkillLevel.Novice;
        public string RequestMessage = "";
        public DateTime RequestDate = DateTime.Now;
        public MentorshipRequestStatus Status = MentorshipRequestStatus.Pending;
    }





    [Serializable]
    public class CommunityCollaborationConfigSO : ScriptableObject
    {
        [Header("Community Collaboration Configuration")]
        public int MaxCollaborators = 5;
        public float CollaborationRewardMultiplier = 1.5f;
        public bool EnableMentorship = true;
        public bool EnableKnowledgeSharing = true;
        public List<string> AvailableCollaborationTypes = new List<string>();
    }

    public enum MentorshipRequestStatus
    {
        Pending,
        Accepted,
        Rejected,
        InProgress,
        Completed,
        Cancelled
    }

    #endregion

    #region Wave 9 - Additional Missing System Types

    [Serializable]
    public class CompetitionBracketManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, CompetitionBracket> ActiveBrackets = new Dictionary<string, CompetitionBracket>();
        public List<CompetitionMatch> PendingMatches = new List<CompetitionMatch>();
        public BracketFormat BracketType = BracketFormat.SingleElimination;
        public bool IsActive = true;
        public DateTime LastUpdate = DateTime.Now;
        public Dictionary<string, object> BracketSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ReputationManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, PlayerReputation> PlayerReputations = new Dictionary<string, PlayerReputation>();
        public Dictionary<string, ReputationCategory> Categories = new Dictionary<string, ReputationCategory>();
        public float GlobalReputationAverage = 50f;
        public DateTime LastReputationUpdate = DateTime.Now;
        public bool IsTrackingEnabled = true;
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class LegacyTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public List<LegacyAchievement> LegacyAchievements = new List<LegacyAchievement>();
        public Dictionary<string, float> LegacyMetrics = new Dictionary<string, float>();
        public float TotalLegacyScore = 0f;
        public DateTime FirstActivity = DateTime.Now;
        public DateTime LastLegacyUpdate = DateTime.Now;
        public bool IsLegendaryStatus = false;
    }

    [Serializable]
    public class SensoryProgressionManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, SensorySkillProgression> PlayerProgressions = new Dictionary<string, SensorySkillProgression>();
        public List<SensoryMilestone> AvailableMilestones = new List<SensoryMilestone>();
        public SensoryTrainingConfiguration TrainingConfig = new SensoryTrainingConfiguration();
        public bool IsProgressionEnabled = true;
        public DateTime LastProgressionUpdate = DateTime.Now;
    }

    [Serializable]
    public class PeerRatingSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, PeerRating> ActiveRatings = new Dictionary<string, PeerRating>();
        public Dictionary<string, float> PlayerRatings = new Dictionary<string, float>();
        public RatingConfiguration RatingConfig = new RatingConfiguration();
        public bool IsRatingEnabled = true;
        public DateTime LastRatingUpdate = DateTime.Now;
        public float SystemWideAverage = 3.5f;
    }

    [Serializable]
    public class ReputationContribution
    {
        public string ContributionId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string ContributionType = "";
        public float ReputationValue = 0f;
        public string Description = "";
        public DateTime ContributionDate = DateTime.Now;
        public bool IsVerified = false;
        public string VerifiedBy = "";
        public Dictionary<string, object> ContributionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExpertiseRecognitionData
    {
        public string RecognitionId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string ExpertiseArea = "";
        public ExpertiseLevel Level = ExpertiseLevel.Novice;
        public float RecognitionScore = 0f;
        public List<string> RecognizedBy = new List<string>();
        public DateTime RecognitionDate = DateTime.Now;
        public bool IsOfficial = false;
        public Dictionary<string, float> ExpertiseMetrics = new Dictionary<string, float>();
    }

    [Serializable]
    public class CompetitiveAnalyticsEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, CompetitiveMetrics> PlayerMetrics = new Dictionary<string, CompetitiveMetrics>();
        public List<AnalyticsReport> GeneratedReports = new List<AnalyticsReport>();
        public AnalyticsConfiguration Configuration = new AnalyticsConfiguration();
        public bool IsAnalyticsEnabled = true;
        public DateTime LastAnalysisUpdate = DateTime.Now;
        public Dictionary<string, Trend> PerformanceTrends = new Dictionary<string, Trend>();
    }

    [Serializable]
    public class TerpeneBlendingEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, BlendingFormula> SavedFormulas = new Dictionary<string, BlendingFormula>();
        public List<BlendingSession> ActiveSessions = new List<BlendingSession>();
        public BlendingConfiguration BlendingConfig = new BlendingConfiguration();
        public bool IsBlendingEnabled = true;
        public DateTime LastBlendingUpdate = DateTime.Now;
        public Dictionary<string, float> BlendingAccuracy = new Dictionary<string, float>();
    }

    [Serializable]
    public class PerformanceTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, PerformanceMetric> TrackedMetrics = new Dictionary<string, PerformanceMetric>();
        public List<PerformanceSnapshot> PerformanceHistory = new List<PerformanceSnapshot>();
        public float OverallPerformanceScore = 0f;
        public DateTime LastPerformanceUpdate = DateTime.Now;
        public bool IsTrackingActive = true;
        public PerformanceConfiguration TrackingConfig = new PerformanceConfiguration();
    }

    [Serializable]
    public class ExperienceAwardData
    {
        public string AwardId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string AwardType = "";
        public float ExperiencePoints = 0f;
        public string AwardReason = "";
        public DateTime AwardedAt = DateTime.Now;
        public string AwardedBy = "System";
        public bool IsBonus = false;
        public Dictionary<string, object> AwardDetails = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipFacilitationSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, MentorshipPairing> ActivePairings = new Dictionary<string, MentorshipPairing>();
        public List<MentorshipOpportunity> AvailableOpportunities = new List<MentorshipOpportunity>();
        public MentorshipConfiguration Configuration = new MentorshipConfiguration();
        public bool IsFacilitationEnabled = true;
        public DateTime LastFacilitationUpdate = DateTime.Now;
        public Dictionary<string, float> MentorshipSuccess = new Dictionary<string, float>();
    }

    [Serializable]
    public class InnovationShowcaseManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, Innovation> FeaturedInnovations = new Dictionary<string, Innovation>();
        public List<ShowcaseEvent> UpcomingShowcases = new List<ShowcaseEvent>();
        public ShowcaseConfiguration ShowcaseConfig = new ShowcaseConfiguration();
        public bool IsShowcaseEnabled = true;
        public DateTime LastShowcaseUpdate = DateTime.Now;
        public Dictionary<string, float> InnovationRatings = new Dictionary<string, float>();
    }

    [Serializable]
    public class CrossSystemSynergyData
    {
        public string SynergyDataId = Guid.NewGuid().ToString();
        public Dictionary<string, SystemInteraction> SystemInteractions = new Dictionary<string, SystemInteraction>();
        public List<SynergyEffect> ActiveSynergies = new List<SynergyEffect>();
        public float TotalSynergyBonus = 1.0f;
        public DateTime LastSynergyUpdate = DateTime.Now;
        public bool IsSynergyActive = true;
        public Dictionary<string, float> SynergyMetrics = new Dictionary<string, float>();
    }

    #endregion

    #region Supporting Configuration Types

    [Serializable]
    public class SensoryTrainingConfiguration
    {
        public float TrainingDifficulty = 0.5f;
        public int MaxTrainingSessions = 10;
        public float ProgressionRate = 1.0f;
        public bool EnableAdvancedTraining = false;
        public Dictionary<string, object> TrainingParameters = new Dictionary<string, object>();
    }

    [Serializable]
    public class RatingConfiguration
    {
        public int MinRating = 1;
        public int MaxRating = 5;
        public float RatingWeight = 1.0f;
        public bool EnableAnonymousRating = true;
        public Dictionary<string, object> RatingSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class AnalyticsConfiguration
    {
        public bool EnableRealTimeAnalytics = true;
        public float AnalyticsInterval = 60f;
        public int MaxDataPoints = 1000;
        public bool EnablePredictiveAnalytics = false;
        public Dictionary<string, object> AnalyticsSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingConfiguration
    {
        public float BlendingPrecision = 0.1f;
        public int MaxIngredients = 10;
        public bool EnableAutoBlending = false;
        public float BlendingSpeed = 1.0f;
        public Dictionary<string, object> BlendingSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class PerformanceConfiguration
    {
        public float TrackingInterval = 30f;
        public int MaxHistoryEntries = 100;
        public bool EnableDetailedTracking = true;
        public bool EnablePerformanceAlerts = true;
        public Dictionary<string, object> TrackingSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipConfiguration
    {
        public int MaxMenteeCount = 5;
        public float MentorshipDuration = 90f; // days
        public bool EnableAutoMatching = true;
        public bool EnableMentorshipRewards = true;
        public Dictionary<string, object> MentorshipSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ShowcaseConfiguration
    {
        public int MaxFeaturedInnovations = 20;
        public float ShowcaseDuration = 7f; // days
        public bool EnablePublicVoting = true;
        public bool EnableExpertReview = true;
        public Dictionary<string, object> ShowcaseSettings = new Dictionary<string, object>();
    }

    #endregion

    #region Supporting Data Types

    [Serializable]
    public class CompetitionBracket
    {
        public string BracketId = Guid.NewGuid().ToString();
        public string BracketName = "";
        public List<CompetitionMatch> Matches = new List<CompetitionMatch>();
        public BracketFormat Format = BracketFormat.SingleElimination;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public Dictionary<string, object> BracketData = new Dictionary<string, object>();
    }

    [Serializable]
    public class PlayerReputation
    {
        public string PlayerId = "";
        public float OverallReputation = 50f;
        public Dictionary<string, float> CategoryReputations = new Dictionary<string, float>();
        public List<ReputationChange> ReputationHistory = new List<ReputationChange>();
        public DateTime LastReputationChange = DateTime.Now;
    }

    [Serializable]
    public class LegacyAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public string Description = "";
        public DateTime AchievedDate = DateTime.Now;
        public float LegacyValue = 0f;
        public bool IsLegendary = false;
        public Dictionary<string, object> AchievementData = new Dictionary<string, object>();
    }

    [Serializable]
    public class SensorySkillProgression
    {
        public string PlayerId = "";
        public Dictionary<string, float> SkillLevels = new Dictionary<string, float>();
        public List<SensoryMilestone> CompletedMilestones = new List<SensoryMilestone>();
        public float OverallProgress = 0f;
        public DateTime LastProgressUpdate = DateTime.Now;
    }

    #endregion

    #region Wave 10 - Additional Missing Analytics and Competition Types

    [Serializable]
    public class CompetitionMatch
    {
        public string MatchId = Guid.NewGuid().ToString();
        public string MatchName = "";
        public List<string> ParticipantIds = new List<string>();
        public string WinnerId = "";
        public MatchStatus Status = MatchStatus.Scheduled;
        public DateTime ScheduledTime = DateTime.Now;
        public DateTime? StartTime = null;
        public DateTime? EndTime = null;
        public Dictionary<string, float> ParticipantScores = new Dictionary<string, float>();
        public string MatchType = "";
        public Dictionary<string, object> MatchData = new Dictionary<string, object>();
    }

    [Serializable]
    public class SensoryMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public string Description = "";
        public SensorySkillLevel RequiredLevel = SensorySkillLevel.Intermediate;
        public float RequiredAccuracy = 0.8f;
        public int RequiredCompletions = 10;
        public bool IsCompleted = false;
        public DateTime? CompletedDate = null;
        public List<string> Prerequisites = new List<string>();
        public Dictionary<string, object> MilestoneRewards = new Dictionary<string, object>();
    }

    [Serializable]
    public class ReputationCategory
    {
        public string CategoryId = Guid.NewGuid().ToString();
        public string CategoryName = "";
        public string Description = "";
        public float Weight = 1.0f;
        public float MinValue = 0f;
        public float MaxValue = 100f;
        public float DefaultValue = 50f;
        public bool IsVisible = true;
        public Dictionary<string, object> CategorySettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ReputationChange
    {
        public string ChangeId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string CategoryId = "";
        public float OldValue = 0f;
        public float NewValue = 0f;
        public float ChangeAmount = 0f;
        public string ChangeReason = "";
        public DateTime ChangeDate = DateTime.Now;
        public string ChangeSource = "";
        public bool IsVerified = true;
        public Dictionary<string, object> ChangeData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CompetitiveMetrics
    {
        public string PlayerId = "";
        public float OverallRating = 1500f; // ELO-style rating
        public int TotalMatches = 0;
        public int Wins = 0;
        public int Losses = 0;
        public int Draws = 0;
        public float WinRate = 0f;
        public List<string> RecentMatchIds = new List<string>();
        public Dictionary<string, float> SkillRatings = new Dictionary<string, float>();
        public DateTime LastMatchDate = DateTime.Now;
        public float PeakRating = 1500f;
        public int CurrentStreak = 0;
        public Dictionary<string, object> ExtendedMetrics = new Dictionary<string, object>();
    }

    [Serializable]
    public class AnalyticsReport
    {
        public string ReportId = Guid.NewGuid().ToString();
        public string ReportTitle = "";
        public string ReportType = "";
        public DateTime GeneratedDate = DateTime.Now;
        public Dictionary<string, object> ReportData = new Dictionary<string, object>();
        public List<AnalyticsChart> Charts = new List<AnalyticsChart>();
        public List<AnalyticsInsight> Insights = new List<AnalyticsInsight>();
        public string GeneratedBy = "";
        public bool IsPublic = false;
        public string Summary = "";
        public Dictionary<string, float> KeyMetrics = new Dictionary<string, float>();
    }

    [Serializable]
    public class Trend
    {
        public string TrendId = Guid.NewGuid().ToString();
        public string TrendName = "";
        public TrendDirection Direction = TrendDirection.Stable;
        public float TrendStrength = 0f;
        public List<TrendDataPoint> DataPoints = new List<TrendDataPoint>();
        public DateTime StartDate = DateTime.Now.AddDays(-30);
        public DateTime EndDate = DateTime.Now;
        public float ChangeRate = 0f;
        public string MetricName = "";
        public Dictionary<string, object> TrendMetadata = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingFormula
    {
        public string FormulaId = Guid.NewGuid().ToString();
        public string FormulaName = "";
        public Dictionary<string, float> Ingredients = new Dictionary<string, float>();
        public List<BlendingStep> Steps = new List<BlendingStep>();
        public float ExpectedQuality = 0f;
        public string CreatedBy = "";
        public DateTime CreatedDate = DateTime.Now;
        public bool IsPublic = false;
        public int TimesUsed = 0;
        public float AverageRating = 0f;
        public Dictionary<string, object> FormulaMetadata = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingSession
    {
        public string SessionId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string FormulaId = "";
        public DateTime StartTime = DateTime.Now;
        public DateTime? EndTime = null;
        public BlendingSessionStatus Status = BlendingSessionStatus.Active;
        public Dictionary<string, float> CurrentIngredients = new Dictionary<string, float>();
        public List<BlendingAction> Actions = new List<BlendingAction>();
        public float QualityScore = 0f;
        public bool IsCompleted = false;
        public Dictionary<string, object> SessionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class PerformanceMetric
    {
        public string MetricId = Guid.NewGuid().ToString();
        public string MetricName = "";
        public float CurrentValue = 0f;
        public float PreviousValue = 0f;
        public float BestValue = 0f;
        public float WorstValue = 0f;
        public DateTime LastUpdate = DateTime.Now;
        public MetricTrend Trend = MetricTrend.Stable;
        public string Unit = "";
        public bool IsHigherBetter = true;
        public Dictionary<string, object> MetricData = new Dictionary<string, object>();
    }

    [Serializable]
    public class PerformanceSnapshot
    {
        public string SnapshotId = Guid.NewGuid().ToString();
        public DateTime Timestamp = DateTime.Now;
        public Dictionary<string, float> MetricValues = new Dictionary<string, float>();
        public string SessionId = "";
        public string PlayerId = "";
        public float OverallScore = 0f;
        public List<string> Achievements = new List<string>();
        public Dictionary<string, object> ContextData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 10 - Supporting Analytics Types

    [Serializable]
    public class AnalyticsChart
    {
        public string ChartId = Guid.NewGuid().ToString();
        public string ChartTitle = "";
        public ChartType Type = ChartType.Line;
        public List<ChartDataSeries> DataSeries = new List<ChartDataSeries>();
        public Dictionary<string, object> ChartOptions = new Dictionary<string, object>();
    }

    [Serializable]
    public class AnalyticsInsight
    {
        public string InsightId = Guid.NewGuid().ToString();
        public string Title = "";
        public string Description = "";
        public InsightPriority Priority = InsightPriority.Medium;
        public float Confidence = 0.8f;
        public List<string> SupportingData = new List<string>();
        public Dictionary<string, object> InsightData = new Dictionary<string, object>();
    }

    [Serializable]
    public class TrendDataPoint
    {
        public DateTime Timestamp = DateTime.Now;
        public float Value = 0f;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingStep
    {
        public int StepNumber = 1;
        public string StepDescription = "";
        public string IngredientId = "";
        public float Amount = 0f;
        public float Temperature = 20f; // Celsius
        public float Duration = 60f; // seconds
        public Dictionary<string, object> StepParameters = new Dictionary<string, object>();
    }

    [Serializable]
    public class BlendingAction
    {
        public string ActionId = Guid.NewGuid().ToString();
        public DateTime Timestamp = DateTime.Now;
        public BlendingActionType ActionType = BlendingActionType.AddIngredient;
        public string IngredientId = "";
        public float Amount = 0f;
        public float Precision = 0f;
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ChartDataSeries
    {
        public string SeriesName = "";
        public List<ChartDataPoint> DataPoints = new List<ChartDataPoint>();
        public string Color = "#0066CC";
        public Dictionary<string, object> SeriesOptions = new Dictionary<string, object>();
    }

    [Serializable]
    public class ChartDataPoint
    {
        public float X = 0f;
        public float Y = 0f;
        public string Label = "";
        public Dictionary<string, object> PointData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 11 - Final Missing System Interaction Types

    [Serializable]
    public class MentorshipPairing
    {
        public string PairingId = Guid.NewGuid().ToString();
        public string MentorId = "";
        public string MenteeId = "";
        public DateTime PairingDate = DateTime.Now;
        public MentorshipStatus Status = MentorshipStatus.Active;
        public List<MentorshipGoal> Goals = new List<MentorshipGoal>();
        public Dictionary<string, float> ProgressMetrics = new Dictionary<string, float>();
        public float OverallProgress = 0f;
        public DateTime? CompletionDate = null;
        public string Notes = "";
        public Dictionary<string, object> PairingData = new Dictionary<string, object>();
    }

    [Serializable]
    public class Innovation
    {
        public string InnovationId = Guid.NewGuid().ToString();
        public string InnovationTitle = "";
        public string Description = "";
        public string CreatorId = "";
        public DateTime CreatedDate = DateTime.Now;
        public InnovationType Type = InnovationType.Process;
        public float ImpactScore = 0f;
        public float NoveltyScore = 0f;
        public List<string> Tags = new List<string>();
        public bool IsPublic = true;
        public bool IsFeatured = false;
        public int ViewCount = 0;
        public float AverageRating = 0f;
        public int RatingCount = 0;
        public Dictionary<string, object> InnovationData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ShowcaseEvent
    {
        public string EventId = Guid.NewGuid().ToString();
        public string EventTitle = "";
        public string Description = "";
        public DateTime StartDate = DateTime.Now;
        public DateTime EndDate = DateTime.Now.AddDays(7);
        public ShowcaseEventType EventType = ShowcaseEventType.Innovation;
        public List<string> FeaturedInnovationIds = new List<string>();
        public Dictionary<string, float> ParticipantScores = new Dictionary<string, float>();
        public bool IsActive = true;
        public bool IsPublic = true;
        public string OrganizerId = "";
        public Dictionary<string, object> EventSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class SystemInteraction
    {
        public string InteractionId = Guid.NewGuid().ToString();
        public string SourceSystemId = "";
        public string TargetSystemId = "";
        public InteractionType Type = InteractionType.Synergy;
        public float InteractionStrength = 1.0f;
        public Dictionary<string, float> InteractionEffects = new Dictionary<string, float>();
        public bool IsActive = true;
        public DateTime LastInteraction = DateTime.Now;
        public float CumulativeEffect = 0f;
        public Dictionary<string, object> InteractionData = new Dictionary<string, object>();
    }

    [Serializable]
    public class SynergyEffect
    {
        public string EffectId = Guid.NewGuid().ToString();
        public string EffectName = "";
        public string Description = "";
        public SynergyType Type = SynergyType.Multiplicative;
        public float EffectMagnitude = 1.2f;
        public List<string> RequiredSystems = new List<string>();
        public Dictionary<string, float> SystemContributions = new Dictionary<string, float>();
        public bool IsActive = true;
        public DateTime ActivatedAt = DateTime.Now;
        public float Duration = -1f; // -1 for permanent
        public Dictionary<string, object> EffectParameters = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 11 - Supporting Types

    [Serializable]
    public class MentorshipGoal
    {
        public string GoalId = Guid.NewGuid().ToString();
        public string GoalTitle = "";
        public string Description = "";
        public GoalPriority Priority = GoalPriority.Medium;
        public float TargetValue = 100f;
        public float CurrentValue = 0f;
        public bool IsCompleted = false;
        public DateTime? CompletedDate = null;
        public DateTime TargetDate = DateTime.Now.AddDays(30);
        public Dictionary<string, object> GoalData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipOpportunity
    {
        public string OpportunityId = Guid.NewGuid().ToString();
        public string Title = "";
        public string Description = "";
        public string RequiredExpertise = "";
        public ExpertiseLevel MinLevel = ExpertiseLevel.Intermediate;
        public int MaxMentees = 3;
        public int CurrentMentees = 0;
        public bool IsAvailable = true;
        public DateTime CreatedDate = DateTime.Now;
        public string CreatedBy = "";
        public Dictionary<string, object> OpportunityData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 12 - Advanced Aromatic Gaming System Types

    [Serializable]
    public class AromaticCompositionAnalyzer
    {
        public string AnalyzerId = Guid.NewGuid().ToString();
        public Dictionary<string, float> AnalysisCapabilities = new Dictionary<string, float>();
        public float AnalysisPrecision = 0.95f;
        public float AnalysisSpeed = 1.0f;
        public List<string> DetectableCompounds = new List<string>();
        public Dictionary<string, AnalysisMethod> SupportedMethods = new Dictionary<string, AnalysisMethod>();
        public bool IsCalibrated = true;
        public DateTime LastCalibration = DateTime.Now;
        public Dictionary<string, object> AnalyzerSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class FlavorProfileGenerator
    {
        public string GeneratorId = Guid.NewGuid().ToString();
        public Dictionary<string, FlavorProfile> GeneratedProfiles = new Dictionary<string, FlavorProfile>();
        public List<FlavorGenerationRule> GenerationRules = new List<FlavorGenerationRule>();
        public float CreativityLevel = 0.8f;
        public float AccuracyLevel = 0.9f;
        public Dictionary<string, float> FlavorWeights = new Dictionary<string, float>();
        public bool EnableAdvancedGeneration = true;
        public DateTime LastProfileGenerated = DateTime.Now;
        public Dictionary<string, object> GeneratorParameters = new Dictionary<string, object>();
    }

    [Serializable]
    public class AromaticInnovationDetector
    {
        public string DetectorId = Guid.NewGuid().ToString();
        public Dictionary<string, InnovationSignature> KnownInnovations = new Dictionary<string, InnovationSignature>();
        public float NoveltyThreshold = 0.7f;
        public float SimilarityTolerance = 0.3f;
        public List<InnovationPattern> DetectedPatterns = new List<InnovationPattern>();
        public Dictionary<string, float> InnovationScores = new Dictionary<string, float>();
        public bool IsLearningEnabled = true;
        public DateTime LastDetection = DateTime.Now;
        public Dictionary<string, object> DetectorConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class SynergyDiscoveryEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, SynergyPattern> DiscoveredSynergies = new Dictionary<string, SynergyPattern>();
        public List<CompoundInteraction> KnownInteractions = new List<CompoundInteraction>();
        public float DiscoveryThreshold = 0.6f;
        public float ConfidenceLevel = 0.8f;
        public Dictionary<string, float> SynergyStrengths = new Dictionary<string, float>();
        public bool EnablePredictiveDiscovery = true;
        public DateTime LastDiscovery = DateTime.Now;
        public Dictionary<string, object> EngineSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class AromaticQualityAssessment
    {
        public string AssessmentId = Guid.NewGuid().ToString();
        public Dictionary<string, QualityMetric> QualityMetrics = new Dictionary<string, QualityMetric>();
        public float OverallQualityScore = 0f;
        public List<QualityFactor> AssessedFactors = new List<QualityFactor>();
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public QualityGrade Grade = QualityGrade.Good;
        public string AssessmentNotes = "";
        public DateTime AssessmentDate = DateTime.Now;
        public Dictionary<string, object> AssessmentData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 12 - Competition and Community Gaming Types

    [Serializable]
    public class SkillAssessmentSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, SkillAssessment> PlayerAssessments = new Dictionary<string, SkillAssessment>();
        public List<AssessmentCriteria> AssessmentCriteria = new List<AssessmentCriteria>();
        public Dictionary<string, float> SkillWeights = new Dictionary<string, float>();
        public float AssessmentAccuracy = 0.85f;
        public bool IsAdaptiveAssessment = true;
        public DateTime LastAssessmentUpdate = DateTime.Now;
        public Dictionary<string, object> SystemConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class CompetitiveMetricsCollector
    {
        public string CollectorId = Guid.NewGuid().ToString();
        public Dictionary<string, CompetitivePlayerMetrics> PlayerMetrics = new Dictionary<string, CompetitivePlayerMetrics>();
        public List<MetricDefinition> TrackedMetrics = new List<MetricDefinition>();
        public Dictionary<string, float> MetricWeights = new Dictionary<string, float>();
        public float CollectionInterval = 30f;
        public bool IsRealTimeCollection = true;
        public DateTime LastCollection = DateTime.Now;
        public Dictionary<string, object> CollectorSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ScientificSkillTreeManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, ScientificSkillTree> PlayerSkillTrees = new Dictionary<string, ScientificSkillTree>();
        public List<ScientificSkillNode> AvailableSkills = new List<ScientificSkillNode>();
        public Dictionary<string, List<string>> SkillDependencies = new Dictionary<string, List<string>>();
        public float SkillProgressionRate = 1.0f;
        public bool EnableSkillSynergies = true;
        public DateTime LastSkillUpdate = DateTime.Now;
        public Dictionary<string, object> ManagerConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class CompetitiveRecord
    {
        public string RecordId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string CompetitionId = "";
        public CompetitiveRecordType RecordType = CompetitiveRecordType.PersonalBest;
        public float RecordValue = 0f;
        public string RecordCategory = "";
        public DateTime AchievedDate = DateTime.Now;
        public bool IsVerified = false;
        public string VerificationMethod = "";
        public Dictionary<string, object> RecordData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CollaborativeProjectEventData
    {
        public string EventId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public CollaborativeEventType EventType = CollaborativeEventType.ProjectCreated;
        public string EventDescription = "";
        public List<string> ParticipantIds = new List<string>();
        public DateTime EventTimestamp = DateTime.Now;
        public Dictionary<string, object> EventMetadata = new Dictionary<string, object>();
        public bool IsPublic = true;
        public float ImpactScore = 0f;
    }

    [Serializable]
    public class ScientificAchievementTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public Dictionary<string, List<ScientificAchievement>> PlayerAchievements = new Dictionary<string, List<ScientificAchievement>>();
        public List<AchievementDefinition> AvailableAchievements = new List<AchievementDefinition>();
        public Dictionary<string, float> AchievementProgress = new Dictionary<string, float>();
        public bool IsTrackingEnabled = true;
        public DateTime LastAchievementUnlock = DateTime.Now;
        public Dictionary<string, object> TrackerSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorMatchingEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, MentorProfile> AvailableMentors = new Dictionary<string, MentorProfile>();
        public Dictionary<string, MenteeProfile> SeekingMentees = new Dictionary<string, MenteeProfile>();
        public List<MatchingCriteria> MatchingCriteria = new List<MatchingCriteria>();
        public float MatchingAccuracy = 0.8f;
        public bool EnableAutoMatching = true;
        public DateTime LastMatchingRun = DateTime.Now;
        public Dictionary<string, object> EngineConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class LegacyRecognitionSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, LegacyRecord> PlayerLegacies = new Dictionary<string, LegacyRecord>();
        public List<LegacyMilestone> RecognitionMilestones = new List<LegacyMilestone>();
        public Dictionary<string, float> LegacyWeights = new Dictionary<string, float>();
        public float RecognitionThreshold = 100f;
        public bool IsPublicRecognition = true;
        public DateTime LastRecognitionUpdate = DateTime.Now;
        public Dictionary<string, object> SystemSettings = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 12 - Supporting Data Types

    [Serializable]
    public class AnalysisMethod
    {
        public string MethodId = Guid.NewGuid().ToString();
        public string MethodName = "";
        public float Accuracy = 0.9f;
        public float Speed = 1.0f;
        public List<string> SupportedCompounds = new List<string>();
        public Dictionary<string, object> MethodParameters = new Dictionary<string, object>();
    }

    [Serializable]
    public class FlavorProfile
    {
        public string ProfileId = Guid.NewGuid().ToString();
        public string ProfileName = "";
        public Dictionary<string, float> FlavorComponents = new Dictionary<string, float>();
        public float IntensityLevel = 1.0f;
        public string DominantFlavor = "";
        public List<string> SubtleNotes = new List<string>();
        public Dictionary<string, object> ProfileData = new Dictionary<string, object>();
    }

    [Serializable]
    public class FlavorGenerationRule
    {
        public string RuleId = Guid.NewGuid().ToString();
        public string RuleName = "";
        public Dictionary<string, float> InputConditions = new Dictionary<string, float>();
        public Dictionary<string, float> OutputModifiers = new Dictionary<string, float>();
        public float RulePriority = 1.0f;
        public bool IsActive = true;
    }

    [Serializable]
    public class InnovationSignature
    {
        public string SignatureId = Guid.NewGuid().ToString();
        public Dictionary<string, float> CharacteristicFingerprint = new Dictionary<string, float>();
        public float NoveltyScore = 0f;
        public DateTime DiscoveredDate = DateTime.Now;
        public string DiscoveredBy = "";
        public Dictionary<string, object> SignatureData = new Dictionary<string, object>();
    }

    [Serializable]
    public class InnovationPattern
    {
        public string PatternId = Guid.NewGuid().ToString();
        public string PatternName = "";
        public Dictionary<string, float> PatternFeatures = new Dictionary<string, float>();
        public float Confidence = 0.8f;
        public int OccurrenceCount = 1;
        public DateTime FirstDetected = DateTime.Now;
    }

    [Serializable]
    public class SynergyPattern
    {
        public string PatternId = Guid.NewGuid().ToString();
        public List<string> ParticipatingCompounds = new List<string>();
        public Dictionary<string, float> SynergyEffects = new Dictionary<string, float>();
        public float SynergyStrength = 1.0f;
        public SynergyType Type = SynergyType.Multiplicative;
        public DateTime DiscoveredDate = DateTime.Now;
    }

    [Serializable]
    public class CompoundInteraction
    {
        public string InteractionId = Guid.NewGuid().ToString();
        public string CompoundA = "";
        public string CompoundB = "";
        public InteractionType InteractionType = InteractionType.Synergy;
        public float InteractionStrength = 1.0f;
        public Dictionary<string, float> EffectModifiers = new Dictionary<string, float>();
    }

    [Serializable]
    public class QualityMetric
    {
        public string MetricId = Guid.NewGuid().ToString();
        public string MetricName = "";
        public float Value = 0f;
        public float Weight = 1.0f;
        public float MinValue = 0f;
        public float MaxValue = 100f;
        public string Unit = "";
    }

    [Serializable]
    public class QualityFactor
    {
        public string FactorId = Guid.NewGuid().ToString();
        public string FactorName = "";
        public float FactorScore = 0f;
        public float FactorWeight = 1.0f;
        public string FactorDescription = "";
        public Dictionary<string, object> FactorData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 12 - Supporting Enums

    public enum QualityGrade
    {
        Poor,
        Fair,
        Good,
        Excellent,
        Outstanding,
        Legendary
    }

    public enum CompetitiveRecordType
    {
        PersonalBest,
        SeasonRecord,
        AllTimeRecord,
        CategoryRecord,
        TeamRecord,
        GlobalRecord
    }

    public enum CollaborativeEventType
    {
        ProjectCreated,
        MemberJoined,
        MemberLeft,
        MilestoneReached,
        ProjectCompleted,
        ProjectPublished,
        AwardReceived
    }

    #endregion

    #region Wave 13 - Final Supporting Types for Complete Compilation

    [Serializable]
    public class CompetitivePlayerMetrics
    {
        public string PlayerId = "";
        public string PlayerName = "";
        public float OverallCompetitiveRating = 1200f;
        public Dictionary<string, float> SkillRatings = new Dictionary<string, float>();
        public Dictionary<string, int> CompetitionCounts = new Dictionary<string, int>();
        public Dictionary<string, float> WinRates = new Dictionary<string, float>();
        public float AveragePerformanceScore = 0f;
        public int TotalCompetitionsEntered = 0;
        public int TotalWins = 0;
        public DateTime LastCompetitionDate = DateTime.Now;
        public List<string> Achievements = new List<string>();
        public Dictionary<string, object> ExtendedMetrics = new Dictionary<string, object>();
    }

    [Serializable]
    public class AchievementDefinition
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public string Description = "";
        public AchievementCategory Category = AchievementCategory.General;
        public AchievementDifficulty Difficulty = AchievementDifficulty.Normal;
        public List<AchievementCriteria> UnlockCriteria = new List<AchievementCriteria>();
        public Dictionary<string, object> RewardData = new Dictionary<string, object>();
        public bool IsHidden = false;
        public bool IsRepeatable = false;
        public int MaxCompletions = 1;
        public DateTime? AvailableFrom = null;
        public DateTime? AvailableUntil = null;
        public Dictionary<string, object> AchievementMetadata = new Dictionary<string, object>();
    }

    [Serializable]
    public class MetricDefinition
    {
        public string MetricId = Guid.NewGuid().ToString();
        public string MetricName = "";
        public string Description = "";
        public MetricType Type = MetricType.Counter;
        public string Unit = "";
        public float DefaultValue = 0f;
        public float MinValue = float.MinValue;
        public float MaxValue = float.MaxValue;
        public bool IsHigherBetter = true;
        public float UpdateFrequency = 1f; // seconds
        public bool IsRealTime = true;
        public Dictionary<string, object> MetricConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class ScientificSkillTree
    {
        public string SkillTreeId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string SkillTreeName = "";
        public Dictionary<string, ScientificSkillNode> UnlockedSkills = new Dictionary<string, ScientificSkillNode>();
        public Dictionary<string, float> SkillProgress = new Dictionary<string, float>();
        public float TotalSkillPoints = 0f;
        public float AvailableSkillPoints = 0f;
        public DateTime LastSkillUnlock = DateTime.Now;
        public List<string> ActiveSkillEffects = new List<string>();
        public Dictionary<string, object> SkillTreeData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ScientificSkillNode
    {
        public string SkillNodeId = Guid.NewGuid().ToString();
        public string SkillName = "";
        public string Description = "";
        public SkillCategory Category = SkillCategory.Research;
        public int RequiredLevel = 1;
        public float SkillPointCost = 1f;
        public List<string> PrerequisiteSkills = new List<string>();
        public Dictionary<string, float> SkillEffects = new Dictionary<string, float>();
        public bool IsUnlocked = false;
        public float CurrentLevel = 0f;
        public float MaxLevel = 5f;
        public DateTime? UnlockedDate = null;
        public Dictionary<string, object> SkillData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorProfile
    {
        public string MentorId = "";
        public string MentorName = "";
        public List<string> ExpertiseAreas = new List<string>();
        public Dictionary<string, ExpertiseLevel> ExpertiseLevels = new Dictionary<string, ExpertiseLevel>();
        public float MentorRating = 4.0f;
        public int TotalMentees = 0;
        public int CurrentMentees = 0;
        public int MaxMentees = 3;
        public bool IsAvailable = true;
        public List<string> PreferredTopics = new List<string>();
        public string MentorshipStyle = "";
        public DateTime JoinedDate = DateTime.Now;
        public Dictionary<string, object> MentorData = new Dictionary<string, object>();
    }

    [Serializable]
    public class SkillAssessment
    {
        public string AssessmentId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, float> SkillScores = new Dictionary<string, float>();
        public float OverallScore = 0f;
        public AssessmentType Type = AssessmentType.Comprehensive;
        public DateTime AssessmentDate = DateTime.Now;
        public List<string> StrengthAreas = new List<string>();
        public List<string> ImprovementAreas = new List<string>();
        public Dictionary<string, string> Recommendations = new Dictionary<string, string>();
        public bool IsValid = true;
        public float ConfidenceLevel = 0.8f;
        public Dictionary<string, object> AssessmentData = new Dictionary<string, object>();
    }

    [Serializable]
    public class AssessmentCriteria
    {
        public string CriteriaId = Guid.NewGuid().ToString();
        public string CriteriaName = "";
        public string Description = "";
        public float Weight = 1.0f;
        public CriteriaType Type = CriteriaType.Performance;
        public float MinScore = 0f;
        public float MaxScore = 100f;
        public Dictionary<string, float> ThresholdLevels = new Dictionary<string, float>();
        public bool IsRequired = true;
        public Dictionary<string, object> CriteriaSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class MenteeProfile
    {
        public string MenteeId = "";
        public string MenteeName = "";
        public List<string> LearningGoals = new List<string>();
        public Dictionary<string, float> CurrentSkillLevels = new Dictionary<string, float>();
        public List<string> InterestedTopics = new List<string>();
        public string LearningStyle = "";
        public float AvailableTime = 5f; // hours per week
        public string ExperienceLevel = "Beginner";
        public DateTime JoinedDate = DateTime.Now;
        public bool IsSeekingMentor = true;
        public string PreferredMentorType = "";
        public Dictionary<string, object> MenteeData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MatchingCriteria
    {
        public string CriteriaId = Guid.NewGuid().ToString();
        public string CriteriaName = "";
        public MatchingType Type = MatchingType.SkillLevel;
        public float Weight = 1.0f;
        public float CompatibilityThreshold = 0.7f;
        public Dictionary<string, object> MatchingParameters = new Dictionary<string, object>();
        public bool IsRequired = false;
        public bool IsFlexible = true;
        public Dictionary<string, float> PreferenceWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class LegacyRecord
    {
        public string RecordId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string PlayerName = "";
        public float TotalLegacyScore = 0f;
        public Dictionary<string, float> LegacyCategories = new Dictionary<string, float>();
        public List<LegacyContribution> Contributions = new List<LegacyContribution>();
        public List<string> RecognizedAchievements = new List<string>();
        public DateTime FirstContribution = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
        public LegacyStatus Status = LegacyStatus.Building;
        public bool IsPublic = true;
        public Dictionary<string, object> LegacyData = new Dictionary<string, object>();
    }

    [Serializable]
    public class LegacyMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public string Description = "";
        public float RequiredLegacyScore = 100f;
        public LegacyCategory Category = LegacyCategory.Achievement;
        public Dictionary<string, object> UnlockRewards = new Dictionary<string, object>();
        public bool IsRepeatable = false;
        public string RecognitionLevel = "Bronze";
        public DateTime? FirstAchievedDate = null;
        public int TimesAchieved = 0;
        public Dictionary<string, object> MilestoneData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 13 - Final Supporting Data Types

    [Serializable]
    public class AchievementCriteria
    {
        public string CriteriaId = Guid.NewGuid().ToString();
        public string CriteriaType = "";
        public string TargetMetric = "";
        public float RequiredValue = 0f;
        public ComparisonOperator Operator = ComparisonOperator.GreaterThanOrEqual;
        public bool IsRequired = true;
        public Dictionary<string, object> CriteriaData = new Dictionary<string, object>();
    }

    [Serializable]
    public class LegacyContribution
    {
        public string ContributionId = Guid.NewGuid().ToString();
        public string ContributionType = "";
        public string Description = "";
        public float ImpactScore = 0f;
        public DateTime ContributionDate = DateTime.Now;
        public bool IsVerified = false;
        public string VerifiedBy = "";
        public Dictionary<string, object> ContributionDetails = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 13 - Final Supporting Enums

    public enum AchievementCategory
    {
        General,
        Competition,
        Research,
        Collaboration,
        Innovation,
        Mentorship,
        Community,
        Legacy
    }

    public enum AchievementDifficulty
    {
        Trivial,
        Easy,
        Normal,
        Hard,
        Extreme,
        Legendary,
        Impossible
    }

    public enum MetricType
    {
        Counter,
        Gauge,
        Timer,
        Percentage,
        Rating,
        Score,
        Level,
        Currency
    }

    public enum SkillCategory
    {
        Research,
        Analysis,
        Cultivation,
        Genetics,
        Chemistry,
        Competition,
        Mentorship,
        Innovation
    }

    public enum AssessmentType
    {
        Initial,
        Periodic,
        Comprehensive,
        Specialized,
        Competitive,
        Certification,
        Progress
    }

    public enum CriteriaType
    {
        Performance,
        Knowledge,
        Skill,
        Behavior,
        Achievement,
        Time,
        Quality
    }

    public enum MatchingType
    {
        SkillLevel,
        Interest,
        Experience,
        Availability,
        Personality,
        Goals,
        Style
    }

    public enum LegacyStatus
    {
        Building,
        Established,
        Notable,
        Distinguished,
        Legendary,
        Immortal
    }

    public enum LegacyCategory
    {
        Achievement,
        Innovation,
        Mentorship,
        Research,
        Community,
        Competition,
        Discovery
    }

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        DoesNotContain
    }

    #endregion

    #region Wave 14 - ScriptableObject Configurations and Manager Types

    [Serializable]
    public class EnhancedScientificGamingConfigSO : ScriptableObject
    {
        [Header("General Gaming Configuration")]
        public bool EnableAdvancedGenetics = true;
        public bool EnableAromaticGaming = true;
        public bool EnableCompetitiveMode = true;
        public bool EnableCommunityFeatures = true;
        public float DifficultyMultiplier = 1.0f;
        
        [Header("Progression Settings")]
        public float ExperienceMultiplier = 1.0f;
        public float SkillProgressionRate = 1.0f;
        public int MaxSkillLevel = 100;
        public bool EnableLegacySystem = true;
        
        [Header("Competition Configuration")]
        public int MaxCompetitionParticipants = 100;
        public float CompetitionDuration = 3600f; // 1 hour
        public bool EnableRealTimeCompetitions = true;
        public float TournamentFrequency = 7f; // days
        
        [Header("Community Settings")]
        public int MaxMentorshipConnections = 10;
        public bool EnablePeerReviews = true;
        public bool EnableCollaborativeProjects = true;
        public float ReputationDecayRate = 0.01f;
    }

    [Serializable]
    public class MentorshipProgramConfigSO : ScriptableObject
    {
        [Header("Program Configuration")]
        public string ProgramName = "Scientific Mentorship";
        public string ProgramDescription = "";
        public int MaxMentorCapacity = 5;
        public int MaxMenteeCapacity = 20;
        public float ProgramDuration = 90f; // days
        
        [Header("Matching Settings")]
        public bool EnableAutoMatching = true;
        public float MatchingThreshold = 0.7f;
        public List<string> RequiredSkills = new List<string>();
        public Dictionary<string, float> SkillWeights = new Dictionary<string, float>();
        
        [Header("Reward Configuration")]
        public bool EnableMentorRewards = true;
        public bool EnableMenteeRewards = true;
        public Dictionary<string, float> RewardMultipliers = new Dictionary<string, float>();
        
        [Header("Progress Tracking")]
        public bool EnableProgressTracking = true;
        public float ProgressUpdateInterval = 7f; // days
        public List<string> MilestoneDefinitions = new List<string>();
    }

    [Serializable]
    public class CollaborativeProjectLibrarySO : ScriptableObject
    {
        [Header("Project Library Configuration")]
        public List<CollaborativeProjectTemplate> ProjectTemplates = new List<CollaborativeProjectTemplate>();
        public Dictionary<string, ProjectCategory> Categories = new Dictionary<string, ProjectCategory>();
        public int MaxActiveProjects = 50;
        public float ProjectDuration = 30f; // days
        
        [Header("Collaboration Settings")]
        public int MaxCollaborators = 10;
        public bool EnablePublicProjects = true;
        public bool EnablePrivateProjects = true;
        public float ContributionThreshold = 0.1f;
        
        [Header("Resource Requirements")]
        public Dictionary<string, int> ResourceLimits = new Dictionary<string, int>();
        public bool EnableResourceSharing = true;
        public float ResourceAllocationMultiplier = 1.0f;
    }

    [Serializable]
    public class MentorshipProgressTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string MentorshipId = "";
        public Dictionary<string, float> ProgressMetrics = new Dictionary<string, float>();
        public List<ProgressMilestone> CompletedMilestones = new List<ProgressMilestone>();
        public float OverallProgress = 0f;
        public DateTime LastProgressUpdate = DateTime.Now;
        public List<string> LearningObjectives = new List<string>();
        public Dictionary<string, bool> ObjectiveCompletion = new Dictionary<string, bool>();
        public string CurrentPhase = "Introduction";
        public Dictionary<string, object> ProgressData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MentorshipRewardsManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, RewardDefinition> AvailableRewards = new Dictionary<string, RewardDefinition>();
        public Dictionary<string, List<string>> PlayerRewards = new Dictionary<string, List<string>>();
        public float RewardMultiplier = 1.0f;
        public bool IsRewardSystemEnabled = true;
        public DateTime LastRewardDistribution = DateTime.Now;
        public Dictionary<string, float> RewardThresholds = new Dictionary<string, float>();
        public List<string> RewardCategories = new List<string>();
        public Dictionary<string, object> RewardConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class PeerEndorsementManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, List<PeerEndorsement>> PlayerEndorsements = new Dictionary<string, List<PeerEndorsement>>();
        public Dictionary<string, float> EndorsementWeights = new Dictionary<string, float>();
        public float EndorsementThreshold = 3;
        public bool EnablePeerEndorsements = true;
        public DateTime LastEndorsementUpdate = DateTime.Now;
        public List<string> EndorsementCategories = new List<string>();
        public Dictionary<string, int> EndorsementLimits = new Dictionary<string, int>();
        public Dictionary<string, object> EndorsementSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class GeneticPuzzleRenderer
    {
        public string RendererInfo = Guid.NewGuid().ToString();
        public Dictionary<string, PuzzleVisualization> ActivePuzzles = new Dictionary<string, PuzzleVisualization>();
        public List<RenderingMode> SupportedModes = new List<RenderingMode>();
        public float RenderingQuality = 1.0f;
        public bool EnableInteractiveRendering = true;
        public DateTime LastRenderUpdate = DateTime.Now;
        public Dictionary<string, RenderingSettings> ModeSettings = new Dictionary<string, RenderingSettings>();
        public bool IsGPUAccelerated = true;
        public Dictionary<string, object> RendererConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class CommunityReputationTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public Dictionary<string, CommunityReputation> PlayerReputations = new Dictionary<string, CommunityReputation>();
        public Dictionary<string, float> ReputationWeights = new Dictionary<string, float>();
        public float ReputationDecayRate = 0.01f;
        public bool EnableReputationTracking = true;
        public DateTime LastReputationUpdate = DateTime.Now;
        public List<string> ReputationCategories = new List<string>();
        public Dictionary<string, float> CategoryThresholds = new Dictionary<string, float>();
        public Dictionary<string, object> TrackerSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class BreedingProgressVisualizer
    {
        public string VisualizerId = Guid.NewGuid().ToString();
        public Dictionary<string, BreedingVisualization> ActiveVisualizations = new Dictionary<string, BreedingVisualization>();
        public List<VisualizationType> SupportedTypes = new List<VisualizationType>();
        public float VisualizationQuality = 1.0f;
        public bool EnableRealTimeVisualization = true;
        public DateTime LastVisualizationUpdate = DateTime.Now;
        public Dictionary<string, VisualizationSettings> TypeSettings = new Dictionary<string, VisualizationSettings>();
        public bool EnableAnimations = true;
        public Dictionary<string, object> VisualizerConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class SocialAchievementSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, SocialAchievement> AvailableAchievements = new Dictionary<string, SocialAchievement>();
        public Dictionary<string, List<string>> PlayerAchievements = new Dictionary<string, List<string>>();
        public bool EnableSocialAchievements = true;
        public DateTime LastAchievementCheck = DateTime.Now;
        public List<string> AchievementCategories = new List<string>();
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
        public float AchievementMultiplier = 1.0f;
        public Dictionary<string, object> SystemConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class GeneticObjectiveTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public Dictionary<string, GeneticObjective> ActiveObjectives = new Dictionary<string, GeneticObjective>();
        public Dictionary<string, float> ObjectiveProgress = new Dictionary<string, float>();
        public bool EnableObjectiveTracking = true;
        public DateTime LastObjectiveUpdate = DateTime.Now;
        public List<string> ObjectiveCategories = new List<string>();
        public Dictionary<string, int> ObjectiveLimits = new Dictionary<string, int>();
        public float ProgressUpdateInterval = 60f; // seconds
        public Dictionary<string, object> TrackerConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class CollaborativeResearchManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, ResearchProject> ActiveProjects = new Dictionary<string, ResearchProject>();
        public Dictionary<string, List<string>> ProjectCollaborators = new Dictionary<string, List<string>>();
        public bool EnableCollaborativeResearch = true;
        public DateTime LastProjectUpdate = DateTime.Now;
        public int MaxActiveProjects = 20;
        public float ProjectDuration = 14f; // days
        public Dictionary<string, float> ContributionWeights = new Dictionary<string, float>();
        public Dictionary<string, object> ManagerConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class BreedingProjectManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, BreedingProject> ActiveProjects = new Dictionary<string, BreedingProject>();
        public Dictionary<string, float> ProjectProgress = new Dictionary<string, float>();
        public bool EnableBreedingProjects = true;
        public DateTime LastProjectUpdate = DateTime.Now;
        public int MaxActiveProjects = 10;
        public float ProjectDuration = 21f; // days
        public Dictionary<string, ProjectTemplate> ProjectTemplates = new Dictionary<string, ProjectTemplate>();
        public Dictionary<string, object> ManagerConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class ProjectCoordinationEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, ProjectCoordination> ActiveCoordinations = new Dictionary<string, ProjectCoordination>();
        public List<CoordinationRule> CoordinationRules = new List<CoordinationRule>();
        public bool EnableProjectCoordination = true;
        public DateTime LastCoordinationUpdate = DateTime.Now;
        public float CoordinationEfficiency = 0.8f;
        public Dictionary<string, float> CoordinationMetrics = new Dictionary<string, float>();
        public bool EnableAutomaticCoordination = true;
        public Dictionary<string, object> EngineConfiguration = new Dictionary<string, object>();
    }

    [Serializable]
    public class NovelTraitDetector
    {
        public string DetectorId = Guid.NewGuid().ToString();
        public Dictionary<string, TraitSignature> KnownTraits = new Dictionary<string, TraitSignature>();
        public List<NoveltyPattern> DetectionPatterns = new List<NoveltyPattern>();
        public float NoveltyThreshold = 0.8f;
        public bool EnableNoveltyDetection = true;
        public DateTime LastDetectionRun = DateTime.Now;
        public Dictionary<string, float> TraitWeights = new Dictionary<string, float>();
        public float DetectionSensitivity = 0.7f;
        public Dictionary<string, object> DetectorConfiguration = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 14 - Supporting Data Types

    [Serializable]
    public class CollaborativeProjectTemplate
    {
        public string TemplateId = Guid.NewGuid().ToString();
        public string TemplateName = "";
        public string Description = "";
        public ProjectCategory Category = ProjectCategory.Research;
        public int RequiredCollaborators = 3;
        public int MaxCollaborators = 10;
        public float EstimatedDuration = 30f; // days
        public List<string> RequiredSkills = new List<string>();
        public Dictionary<string, object> TemplateData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ProgressMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public string Description = "";
        public float RequiredProgress = 0.25f;
        public bool IsCompleted = false;
        public DateTime? CompletionDate = null;
        public Dictionary<string, object> MilestoneRewards = new Dictionary<string, object>();
    }

    [Serializable]
    public class RewardDefinition
    {
        public string RewardId = Guid.NewGuid().ToString();
        public string RewardName = "";
        public RewardType Type = RewardType.Experience;
        public float Value = 100f;
        public string Description = "";
        public Dictionary<string, object> RewardData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 14 - Supporting Enums

    public enum ProjectCategory
    {
        Research,
        Breeding,
        Analysis,
        Innovation,
        Community,
        Competition,
        Education
    }

    public enum RewardType
    {
        Experience,
        Currency,
        Item,
        Title,
        Badge,
        Access,
        Boost
    }

    public enum RenderingMode
    {
        Standard,
        HighQuality,
        Interactive,
        Simplified,
        Animation,
        VR,
        AR
    }

    public enum VisualizationType
    {
        Timeline,
        TreeView,
        Network,
        Chart,
        Heatmap,
        Animation,
        Interactive3D
    }

    #endregion

    #region Wave 11 - Supporting Enums

    public enum MentorshipStatus
    {
        Pending,
        Active,
        OnHold,
        Completed,
        Cancelled,
        Graduated
    }

    public enum InnovationType
    {
        Process,
        Product,
        Method,
        Theory,
        Application,
        Hybrid,
        Breakthrough
    }

    public enum ShowcaseEventType
    {
        Innovation,
        Competition,
        Collaboration,
        Discovery,
        Achievement,
        Community,
        Research
    }

    public enum InteractionType
    {
        Synergy,
        Conflict,
        Dependency,
        Enhancement,
        Inhibition,
        Neutral
    }

    public enum SynergyType
    {
        Additive,
        Multiplicative,
        Exponential,
        Threshold,
        Conditional,
        Dynamic
    }

    public enum GoalPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Optional
    }

    #endregion

    #region Wave 10 - Supporting Enums

    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Paused,
        Completed,
        Cancelled,
        Postponed
    }

    public enum TrendDirection
    {
        Rising,
        Falling,
        Stable,
        Volatile,
        Unknown
    }

    public enum BlendingSessionStatus
    {
        Active,
        Paused,
        Completed,
        Failed,
        Cancelled
    }

    public enum MetricTrend
    {
        Improving,
        Declining,
        Stable,
        Volatile
    }

    public enum ChartType
    {
        Line,
        Bar,
        Pie,
        Scatter,
        Histogram,
        Heatmap
    }

    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum BlendingActionType
    {
        AddIngredient,
        RemoveIngredient,
        Mix,
        Heat,
        Cool,
        Wait,
        Measure
    }

    #endregion

    #region Supporting Enums

    public enum BracketFormat
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Swiss,
        Ladder,
        Custom
    }

    public enum ExpertiseLevel
    {
        Novice,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Grandmaster
    }

    #endregion

    #region Wave 15 - Final Missing Types for Complete Compilation

    [Serializable]
    public class PuzzleVisualization
    {
        public string VisualizationId = Guid.NewGuid().ToString();
        public string PuzzleName = "";
        public PuzzleType Type = PuzzleType.Genetic;
        public Dictionary<string, object> VisualizationData = new Dictionary<string, object>();
        public RenderingSettings RenderSettings = new RenderingSettings();
        public List<string> InteractiveElements = new List<string>();
        public bool IsAnimated = false;
        public float AnimationSpeed = 1.0f;
        public DateTime CreatedAt = DateTime.Now;
    }

    [Serializable]
    public class RenderingSettings
    {
        public string SettingsId = Guid.NewGuid().ToString();
        public float RenderScale = 1.0f;
        public bool EnableAntiAliasing = true;
        public bool EnableShadows = true;
        public RenderQuality Quality = RenderQuality.High;
        public Dictionary<string, float> ShaderParameters = new Dictionary<string, float>();
        public Color BackgroundColor = Color.white;
        public bool EnablePostProcessing = true;
        public float Brightness = 1.0f;
        public float Contrast = 1.0f;
    }

    public enum PuzzleType
    {
        Genetic,
        Aromatic,
        Breeding,
        Environmental,
        Chemical,
        Competition
    }

    public enum RenderQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    [Serializable]
    public class CommunityReputation
    {
        public string ReputationId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float OverallScore = 50.0f;
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public List<string> Achievements = new List<string>();
        public int HelpfulContributions = 0;
        public int QualitySubmissions = 0;
        public float TrustRating = 0.5f;
        public DateTime LastUpdate = DateTime.Now;
        public CommunityRank Rank = CommunityRank.Member;
        public List<string> Endorsements = new List<string>();
    }

    public enum CommunityRank
    {
        Newcomer,
        Member,
        Contributor,
        Expert,
        Specialist,
        Master,
        Legend
    }

    [Serializable]
    public class BreedingVisualization
    {
        public string VisualizationId = Guid.NewGuid().ToString();
        public string BreedingPairId = "";
        public VisualizationSettings Settings = new VisualizationSettings();
        public Dictionary<string, object> GeneticVisualization = new Dictionary<string, object>();
        public Dictionary<string, object> PhenotypePreview = new Dictionary<string, object>();
        public List<string> VisualEffects = new List<string>();
        public bool ShowProbabilities = true;
        public bool ShowDominance = true;
        public DateTime GeneratedAt = DateTime.Now;
    }

    [Serializable]
    public class VisualizationSettings
    {
        public string SettingsId = Guid.NewGuid().ToString();
        public float ZoomLevel = 1.0f;
        public Vector3 CameraPosition = Vector3.zero;
        public Vector3 CameraRotation = Vector3.zero;
        public bool ShowGrid = true;
        public bool ShowLabels = true;
        public Color PrimaryColor = Color.blue;
        public Color SecondaryColor = Color.green;
        public float AnimationSpeed = 1.0f;
        public Dictionary<string, bool> FeatureToggles = new Dictionary<string, bool>();
    }

    [Serializable]
    public class SocialAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public string Description = "";
        public SocialAchievementType Type = SocialAchievementType.Collaboration;
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
        public bool IsUnlocked = false;
        public DateTime UnlockedAt = DateTime.MinValue;
        public float ProgressPercentage = 0.0f;
        public List<string> ParticipatingPlayers = new List<string>();
        public bool IsTeamAchievement = false;
    }

    public enum SocialAchievementType
    {
        Collaboration,
        Mentorship,
        Community,
        Leadership,
        Innovation,
        Support
    }

    [Serializable]
    public class GeneticObjective
    {
        public string ObjectiveId = Guid.NewGuid().ToString();
        public string ObjectiveName = "";
        public string Description = "";
        public GeneticObjectiveType Type = GeneticObjectiveType.TraitExpression;
        public Dictionary<string, object> TargetCriteria = new Dictionary<string, object>();
        public Dictionary<string, float> SuccessThresholds = new Dictionary<string, float>();
        public float CompletionPercentage = 0.0f;
        public bool IsCompleted = false;
        public DateTime CreatedAt = DateTime.Now;
        public DateTime? CompletedAt = null;
        public List<string> RequiredGenes = new List<string>();
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
    }

    public enum GeneticObjectiveType
    {
        TraitExpression,
        AlleleFrequency,
        PhenotypeStability,
        NovelCombination,
        ResistanceDevelopment,
        YieldOptimization
    }

    [Serializable]
    public class ResearchProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string Description = "";
        public ResearchProjectType Type = ResearchProjectType.Genetics;
        public ResearchStatus Status = ResearchStatus.Planning;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public float CompletionPercentage = 0.0f;
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
        public Dictionary<string, object> Results = new Dictionary<string, object>();
        public List<string> CollaboratorIds = new List<string>();
        public float FundingRequired = 1000.0f;
        public float FundingSecured = 0.0f;
        public List<string> Milestones = new List<string>();
    }

    public enum ResearchProjectType
    {
        Genetics,
        Aromatics,
        Environmental,
        Breeding,
        Innovation,
        Optimization
    }

    public enum ResearchStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled,
        UnderReview
    }

    [Serializable]
    public class BreedingProject
    {
        public string ProjectId = Guid.NewGuid().ToString();
        public string ProjectName = "";
        public string Description = "";
        public BreedingProjectType Type = BreedingProjectType.TraitSelection;
        public BreedingStatus Status = BreedingStatus.Planning;
        public DateTime StartDate = DateTime.Now;
        public DateTime? EndDate = null;
        public List<string> ParentStrainIds = new List<string>();
        public List<string> TargetTraits = new List<string>();
        public Dictionary<string, object> BreedingParameters = new Dictionary<string, object>();
        public float SuccessProbability = 0.5f;
        public List<string> GeneratedOffspring = new List<string>();
        public Dictionary<string, float> TraitExpressionResults = new Dictionary<string, float>();
    }

    public enum BreedingProjectType
    {
        TraitSelection,
        HybridCreation,
        StabilityTesting,
        NovelCombination,
        ResistanceBreeding,
        YieldEnhancement
    }

    public enum BreedingStatus
    {
        Planning,
        CrossPollination,
        Germination,
        Growing,
        Testing,
        Evaluation,
        Completed,
        Failed
    }

    [Serializable]
    public class ProjectTemplate
    {
        public string TemplateId = Guid.NewGuid().ToString();
        public string TemplateName = "";
        public string Description = "";
        public ProjectTemplateType Type = ProjectTemplateType.Research;
        public Dictionary<string, object> DefaultParameters = new Dictionary<string, object>();
        public List<string> RequiredResources = new List<string>();
        public List<string> RecommendedSkills = new List<string>();
        public float EstimatedDuration = 30.0f; // days
        public float DifficultyRating = 0.5f;
        public Dictionary<string, object> SuccessCriteria = new Dictionary<string, object>();
        public bool IsPublic = true;
        public DateTime CreatedAt = DateTime.Now;
        public string CreatorId = "";
    }

    public enum ProjectTemplateType
    {
        Research,
        Breeding,
        Competition,
        Collaboration,
        Education,
        Innovation
    }

    [Serializable]
    public class ProjectCoordination
    {
        public string CoordinationId = Guid.NewGuid().ToString();
        public string ProjectId = "";
        public string CoordinatorId = "";
        public List<string> TeamMemberIds = new List<string>();
        public Dictionary<string, string> RoleAssignments = new Dictionary<string, string>();
        public List<string> TaskList = new List<string>();
        public Dictionary<string, float> TaskProgress = new Dictionary<string, float>();
        public CoordinationRule Rules = new CoordinationRule();
        public DateTime LastUpdate = DateTime.Now;
        public bool IsActive = true;
        public Dictionary<string, object> CommunicationChannels = new Dictionary<string, object>();
    }

    [Serializable]
    public class CoordinationRule
    {
        public string RuleId = Guid.NewGuid().ToString();
        public string RuleName = "";
        public string Description = "";
        public RuleType Type = RuleType.Permission;
        public Dictionary<string, object> Conditions = new Dictionary<string, object>();
        public Dictionary<string, object> Actions = new Dictionary<string, object>();
        public bool IsActive = true;
        public int Priority = 1;
        public DateTime CreatedAt = DateTime.Now;
        public string CreatedBy = "";
    }

    public enum RuleType
    {
        Permission,
        Restriction,
        Automation,
        Notification,
        Validation,
        Trigger
    }

    [Serializable]
    public class TraitSignature
    {
        public string SignatureId = Guid.NewGuid().ToString();
        public string TraitName = "";
        public Dictionary<string, float> GeneticMarkers = new Dictionary<string, float>();
        public Dictionary<string, object> PhenotypicIndicators = new Dictionary<string, object>();
        public float SignatureStrength = 1.0f;
        public float Reliability = 0.95f;
        public List<string> AssociatedGenes = new List<string>();
        public Dictionary<string, float> ExpressionPattern = new Dictionary<string, float>();
        public DateTime DiscoveredAt = DateTime.Now;
        public string DiscoveredBy = "";
        public bool IsValidated = false;
        public float ValidationScore = 0.0f;
    }

    [Serializable]
    public class NoveltyPattern
    {
        public string PatternId = Guid.NewGuid().ToString();
        public string PatternName = "";
        public string Description = "";
        public NoveltyPatternType Type = NoveltyPatternType.TraitCombination;
        public Dictionary<string, object> PatternData = new Dictionary<string, object>();
        public float NoveltyScore = 0.0f;
        public float RarityScore = 0.0f;
        public Dictionary<string, float> ComponentWeights = new Dictionary<string, float>();
        public DateTime DiscoveredAt = DateTime.Now;
        public string DiscoveredBy = "";
        public bool IsVerified = false;
        public List<string> SimilarPatterns = new List<string>();
        public Dictionary<string, object> ValidationData = new Dictionary<string, object>();
    }

    public enum NoveltyPatternType
    {
        TraitCombination,
        GeneticExpression,
        AromaticProfile,
        MorphologyVariation,
        ResistancePattern,
        YieldCharacteristic
    }

    #endregion

    #region Wave 19 - Missing Genetics Gaming System Types

    [Serializable]
    public class GeneticInnovationTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public List<string> InnovationHistory = new List<string>();
        public Dictionary<string, float> InnovationScores = new Dictionary<string, float>();
        public DateTime LastInnovation = DateTime.Now;
        public float InnovationRate = 0.1f;
        public List<string> UnlockedTechniques = new List<string>();
        public Dictionary<string, object> InnovationData = new Dictionary<string, object>();
    }

    [Serializable]
    public class KnowledgeExchangeHub
    {
        public string HubId = Guid.NewGuid().ToString();
        public string HubName = "";
        public List<string> ParticipantIds = new List<string>();
        public Dictionary<string, KnowledgeEntry> KnowledgeBase = new Dictionary<string, KnowledgeEntry>();
        public List<string> ActiveDiscussions = new List<string>();
        public float ActivityLevel = 0.5f;
        public DateTime LastActivity = DateTime.Now;
        public bool IsPublic = true;
        public Dictionary<string, float> ExpertiseAreas = new Dictionary<string, float>();
    }

    [Serializable]
    public class KnowledgeEntry
    {
        public string EntryId = Guid.NewGuid().ToString();
        public string Title = "";
        public string Content = "";
        public string AuthorId = "";
        public DateTime CreatedAt = DateTime.Now;
        public List<string> Tags = new List<string>();
        public float Quality = 0.5f;
        public int Views = 0;
        public List<string> Contributors = new List<string>();
    }

    [Serializable]
    public class CommunityInnovationTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string CommunityId = "";
        public Dictionary<string, float> CollectiveInnovations = new Dictionary<string, float>();
        public List<string> BreakthroughHistory = new List<string>();
        public float CommunityInnovationScore = 0.5f;
        public DateTime LastBreakthrough = DateTime.Now;
        public Dictionary<string, List<string>> CollaborativeProjects = new Dictionary<string, List<string>>();
        public List<string> SharedTechniques = new List<string>();
    }

    [Serializable]
    public class KnowledgeDocumentationSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, DocumentationEntry> Documentation = new Dictionary<string, DocumentationEntry>();
        public List<string> Categories = new List<string>();
        public Dictionary<string, float> CategoryPopularity = new Dictionary<string, float>();
        public bool AutoGenerateDocumentation = true;
        public DateTime LastUpdate = DateTime.Now;
        public Dictionary<string, object> SystemSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class DocumentationEntry
    {
        public string EntryId = Guid.NewGuid().ToString();
        public string Title = "";
        public string Description = "";
        public string Category = "";
        public DateTime CreatedAt = DateTime.Now;
        public DateTime LastModified = DateTime.Now;
        public string AuthorId = "";
        public List<string> Collaborators = new List<string>();
        public Dictionary<string, object> Content = new Dictionary<string, object>();
    }

    [Serializable]
    public class CommunityLearningPlatform
    {
        public string PlatformId = Guid.NewGuid().ToString();
        public string PlatformName = "";
        public List<string> LearningModules = new List<string>();
        public Dictionary<string, float> LearningProgress = new Dictionary<string, float>();
        public List<string> AvailableCourses = new List<string>();
        public bool EnableCertification = true;
        public DateTime LastActivity = DateTime.Now;
        public Dictionary<string, object> PlatformSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExpertConsultationSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public List<string> AvailableExperts = new List<string>();
        public Dictionary<string, ExpertProfile> ExpertProfiles = new Dictionary<string, ExpertProfile>();
        public List<string> ActiveConsultations = new List<string>();
        public float SystemEfficiency = 0.8f;
        public bool AutoMatchExperts = true;
        public DateTime LastConsultation = DateTime.Now;
        public Dictionary<string, object> ConsultationHistory = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExpertProfile
    {
        public string ExpertId = Guid.NewGuid().ToString();
        public string ExpertName = "";
        public List<string> ExpertiseAreas = new List<string>();
        public float ExpertiseLevel = 0.8f;
        public int ConsultationsCompleted = 0;
        public float SuccessRate = 0.9f;
        public bool IsAvailable = true;
        public Dictionary<string, float> Ratings = new Dictionary<string, float>();
        public DateTime LastActivity = DateTime.Now;
    }

    [Serializable]
    public class CommunityContribution
    {
        public string ContributionId = Guid.NewGuid().ToString();
        public string ContributorId = "";
        public ContributionType Type = ContributionType.Knowledge;
        public string Title = "";
        public string Description = "";
        public DateTime ContributedAt = DateTime.Now;
        public float Value = 1.0f;
        public List<string> Beneficiaries = new List<string>();
        public bool IsVerified = false;
        public string VerifiedBy = "";
        public Dictionary<string, object> ContributionDetails = new Dictionary<string, object>();
    }

    public enum ContributionType
    {
        Knowledge,
        Research,
        Technique,
        Innovation,
        Teaching,
        Mentoring,
        Resource
    }

    [Serializable]
    public class SkillTreeData
    {
        public string SkillTreeId = Guid.NewGuid().ToString();
        public string TreeName = "";
        public Dictionary<string, SkillNode> Nodes = new Dictionary<string, SkillNode>();
        public string PlayerId = "";
        public float CompletionPercentage = 0.0f;
        public DateTime LastUpdate = DateTime.Now;
        public List<string> UnlockedPaths = new List<string>();
        public Dictionary<string, float> SkillProgress = new Dictionary<string, float>();
    }

    [Serializable]
    public class SkillTree
    {
        public string TreeId = Guid.NewGuid().ToString();
        public string TreeName = "";
        public List<SkillNode> Nodes = new List<SkillNode>();
        public Dictionary<string, List<string>> NodeConnections = new Dictionary<string, List<string>>();
        public SkillTreeType TreeType = SkillTreeType.Linear;
        public bool IsActive = true;
        public DateTime CreatedAt = DateTime.Now;
    }

    public enum SkillTreeType
    {
        Linear,
        Branching,
        Web,
        Hierarchical
    }

    [Serializable]
    public class SkillUnlockedData
    {
        public string UnlockId = Guid.NewGuid().ToString();
        public string SkillId = "";
        public string PlayerId = "";
        public DateTime UnlockedAt = DateTime.Now;
        public string UnlockMethod = "";
        public Dictionary<string, object> UnlockContext = new Dictionary<string, object>();
        public bool IsMastered = false;
        public float MasteryLevel = 0.0f;
    }

    [Serializable]
    public class AchievementEarnedData
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public string AchievementName = "";
        public DateTime EarnedAt = DateTime.Now;
        public float Difficulty = 0.5f;
        public Dictionary<string, object> EarnConditions = new Dictionary<string, object>();
        public bool IsRare = false;
        public List<string> Witnesses = new List<string>();
    }

    [Serializable]
    public class MilestoneData
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public string Description = "";
        public float RequiredProgress = 100f;
        public float CurrentProgress = 0f;
        public bool IsCompleted = false;
        public DateTime? CompletedAt = null;
        public Dictionary<string, object> Rewards = new Dictionary<string, object>();
        public MilestoneType Type = MilestoneType.Progress;
    }

    public enum MilestoneType
    {
        Progress,
        Achievement,
        Discovery,
        Collaboration,
        Innovation
    }

    [Serializable]
    public class SkillSynergy
    {
        public string SynergyId = Guid.NewGuid().ToString();
        public List<string> RequiredSkills = new List<string>();
        public string SynergyName = "";
        public float SynergyBonus = 1.2f;
        public bool IsActive = false;
        public DateTime ActivatedAt = DateTime.MinValue;
        public Dictionary<string, float> BonusEffects = new Dictionary<string, float>();
    }

    [Serializable]
    public class CrossSystemProgress
    {
        public string ProgressId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, float> SystemProgress = new Dictionary<string, float>();
        public List<string> CompletedSystems = new List<string>();
        public float OverallProgress = 0.0f;
        public DateTime LastUpdate = DateTime.Now;
        public Dictionary<string, object> CrossSystemBonuses = new Dictionary<string, object>();
    }

    [Serializable]
    public class ProgressionMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public string SystemName = "";
        public float RequiredValue = 100f;
        public bool IsCompleted = false;
        public DateTime? CompletedAt = null;
        public Dictionary<string, object> CompletionRewards = new Dictionary<string, object>();
        public MilestoneCategory Category = MilestoneCategory.Individual;
    }

    public enum MilestoneCategory
    {
        Individual,
        Team,
        Community,
        Global
    }

    #endregion

    #region Wave 16 - Scientific Progression System Types

    [Serializable]
    public class ScientificSkillTreeLibrarySO : ScriptableObject
    {
        [Header("Skill Tree Library Configuration")]
        public string LibraryName = "Scientific Skill Trees";
        public List<ScientificSkillTree> SkillTrees = new List<ScientificSkillTree>();
        public Dictionary<string, SkillTreeTemplate> Templates = new Dictionary<string, SkillTreeTemplate>();
        public ScientificSkillTreeSettings DefaultSettings = new ScientificSkillTreeSettings();
    }

    [Serializable]
    public class ScientificAchievementDatabaseSO : ScriptableObject
    {
        [Header("Achievement Database")]
        public string DatabaseName = "Scientific Achievements";
        public List<ScientificAchievement> Achievements = new List<ScientificAchievement>();
        public Dictionary<string, AchievementCategory> Categories = new Dictionary<string, AchievementCategory>();
        public AchievementDatabaseSettings Settings = new AchievementDatabaseSettings();
    }

    [Serializable]
    public class SkillNodeProgressionTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, SkillNodeProgress> NodeProgress = new Dictionary<string, SkillNodeProgress>();
        public float OverallProgress = 0f;
        public DateTime LastUpdate = DateTime.Now;
        public List<string> CompletedPaths = new List<string>();
        public Dictionary<string, float> CategoryProgress = new Dictionary<string, float>();
    }

    [Serializable]
    public class SkillSynergyEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, SkillSynergy> ActiveSynergies = new Dictionary<string, SkillSynergy>();
        public float SynergyMultiplier = 1.2f;
        public bool IsEnabled = true;
        public DateTime LastUpdate = DateTime.Now;
        public Dictionary<string, float> SynergyBonuses = new Dictionary<string, float>();
    }

    [Serializable]
    public class SkillUnlockManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, SkillUnlockCondition> UnlockConditions = new Dictionary<string, SkillUnlockCondition>();
        public List<string> AvailableSkills = new List<string>();
        public float UnlockDifficulty = 1.0f;
        public bool AutoUnlockEnabled = false;
        public DateTime LastUnlock = DateTime.Now;
    }

    [Serializable]
    public class CrossSystemAchievementProcessor
    {
        public string ProcessorId = Guid.NewGuid().ToString();
        public Dictionary<string, SystemIntegration> SystemConnections = new Dictionary<string, SystemIntegration>();
        public List<CrossSystemAchievement> CrossSystemAchievements = new List<CrossSystemAchievement>();
        public bool IsProcessing = false;
        public float ProcessingInterval = 30f;
        public DateTime LastProcessed = DateTime.Now;
    }

    [Serializable]
    public class MilestoneRecognitionSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public List<ResearchMilestone> Milestones = new List<ResearchMilestone>();
        public Dictionary<string, MilestoneTracker> MilestoneTrackers = new Dictionary<string, MilestoneTracker>();
        public MilestoneRecognitionSettings Settings = new MilestoneRecognitionSettings();
        public bool IsActive = true;
        public DateTime LastRecognition = DateTime.Now;
    }

    [Serializable]
    public class ProgressionRewardsManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, ProgressionReward> AvailableRewards = new Dictionary<string, ProgressionReward>();
        public RewardDistributionSettings DistributionSettings = new RewardDistributionSettings();
        public List<string> PendingRewards = new List<string>();
        public float RewardMultiplier = 1.0f;
        public DateTime LastRewardDistribution = DateTime.Now;
    }

    [Serializable]
    public class ScientificReputationManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, GamingScientificReputation> PlayerReputations = new Dictionary<string, GamingScientificReputation>();
        public ReputationCalculationSettings CalculationSettings = new ReputationCalculationSettings();
        public List<ReputationEvent> RecentEvents = new List<ReputationEvent>();
        public float BaseReputationScore = 50f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class ExpertiseRecognitionSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, ExpertiseLevel> PlayerExpertise = new Dictionary<string, ExpertiseLevel>();
        public ExpertiseRecognitionSettings Settings = new ExpertiseRecognitionSettings();
        public List<ExpertiseAchievement> ExpertiseAchievements = new List<ExpertiseAchievement>();
        public bool IsActive = true;
        public DateTime LastEvaluation = DateTime.Now;
    }

    [Serializable]
    public class LegacyProgressionTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public LegacyProgressionData ProgressionData = new LegacyProgressionData();
        public List<LegacyMilestone> LegacyMilestones = new List<LegacyMilestone>();
        public float LegacyScore = 0f;
        public DateTime StartDate = DateTime.Now;
        public Dictionary<string, object> LegacyMetrics = new Dictionary<string, object>();
    }

    [Serializable]
    public class ScientificRankingSystem
    {
        public string SystemId = Guid.NewGuid().ToString();
        public Dictionary<string, ScientificRanking> PlayerRankings = new Dictionary<string, ScientificRanking>();
        public RankingCalculationSettings CalculationSettings = new RankingCalculationSettings();
        public List<RankingCategory> RankingCategories = new List<RankingCategory>();
        public DateTime LastRankingUpdate = DateTime.Now;
        public bool IsActive = true;
    }

    [Serializable]
    public class UnifiedExperienceManager
    {
        public string ManagerId = Guid.NewGuid().ToString();
        public Dictionary<string, UnifiedExperience> PlayerExperiences = new Dictionary<string, UnifiedExperience>();
        public ExperienceCalculationSettings CalculationSettings = new ExperienceCalculationSettings();
        public List<ExperienceSource> ExperienceSources = new List<ExperienceSource>();
        public float ExperienceMultiplier = 1.0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class ExperienceMultiplierEngine
    {
        public string EngineId = Guid.NewGuid().ToString();
        public Dictionary<string, ExperienceMultiplier> ActiveMultipliers = new Dictionary<string, ExperienceMultiplier>();
        public float BaseMultiplier = 1.0f;
        public float MaxMultiplier = 5.0f;
        public bool IsEnabled = true;
        public DateTime LastUpdate = DateTime.Now;
        public List<MultiplierCondition> MultiplierConditions = new List<MultiplierCondition>();
    }

    [Serializable]
    public class SeasonalProgressionBonus
    {
        public string BonusId = Guid.NewGuid().ToString();
        public string SeasonName = "";
        public float BonusMultiplier = 1.5f;
        public DateTime StartDate = DateTime.Now;
        public DateTime EndDate = DateTime.Now.AddDays(30);
        public List<string> ApplicableCategories = new List<string>();
        public bool IsActive = false;
        public Dictionary<string, object> BonusConditions = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 16 - Supporting Data Structures

    [Serializable]
    public class SkillTreeTemplate
    {
        public string TemplateId = Guid.NewGuid().ToString();
        public string TemplateName = "";
        public ScientificTreeType TreeType = ScientificTreeType.Research;
        public Dictionary<string, object> TemplateSettings = new Dictionary<string, object>();
        public List<SkillNodeTemplate> NodeTemplates = new List<SkillNodeTemplate>();
    }

    [Serializable]
    public class ScientificSkillTreeSettings
    {
        public float DefaultUnlockCost = 100f;
        public float DifficultyScaling = 1.2f;
        public bool EnableSynergies = true;
        public bool EnableSeasonalBonuses = true;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class AchievementDatabaseSettings
    {
        public bool EnableCategories = true;
        public bool EnableTiers = true;
        public bool EnableSeasonalAchievements = true;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class SkillNodeProgress
    {
        public string NodeId = "";
        public float Progress = 0f;
        public bool IsCompleted = false;
        public DateTime StartDate = DateTime.Now;
        public DateTime? CompletionDate = null;
        public Dictionary<string, float> SubProgress = new Dictionary<string, float>();
    }

    [Serializable]
    public class SkillUnlockCondition
    {
        public string ConditionId = Guid.NewGuid().ToString();
        public UnlockConditionType Type = UnlockConditionType.Prerequisites;
        public Dictionary<string, object> ConditionData = new Dictionary<string, object>();
        public bool IsMet = false;
        public float Progress = 0f;
    }

    [Serializable]
    public class SystemIntegration
    {
        public string IntegrationId = Guid.NewGuid().ToString();
        public string SystemName = "";
        public bool IsConnected = false;
        public float IntegrationStrength = 1.0f;
        public Dictionary<string, object> IntegrationData = new Dictionary<string, object>();
    }

    [Serializable]
    public class CrossSystemAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public List<string> RequiredSystems = new List<string>();
        public Dictionary<string, object> SystemRequirements = new Dictionary<string, object>();
        public bool IsUnlocked = false;
        public float Progress = 0f;
    }

    [Serializable]
    public class ResearchMilestone
    {
        public string MilestoneId = Guid.NewGuid().ToString();
        public string MilestoneName = "";
        public ResearchMilestoneType Type = ResearchMilestoneType.Discovery;
        public bool IsAchieved = false;
        public DateTime AchievedAt = DateTime.Now;
        public Dictionary<string, object> MilestoneData = new Dictionary<string, object>();
    }

    [Serializable]
    public class MilestoneTracker
    {
        public string TrackerId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public Dictionary<string, MilestoneProgress> MilestoneProgress = new Dictionary<string, MilestoneProgress>();
        public float OverallProgress = 0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class MilestoneRecognitionSettings
    {
        public bool EnableAutoRecognition = true;
        public float RecognitionThreshold = 0.8f;
        public bool EnableNotifications = true;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }

    [Serializable]
    public class ProgressionReward
    {
        public string RewardId = Guid.NewGuid().ToString();
        public string RewardName = "";
        public RewardType Type = RewardType.Experience;
        public float RewardValue = 100f;
        public Dictionary<string, object> RewardData = new Dictionary<string, object>();
        public bool IsClaimable = false;
    }

    [Serializable]
    public class RewardDistributionSettings
    {
        public float BaseRewardMultiplier = 1.0f;
        public bool EnableBonusRewards = true;
        public bool EnableSeasonalRewards = true;
        public Dictionary<string, object> DistributionRules = new Dictionary<string, object>();
    }

    [Serializable]
    public class GamingScientificReputation
    {
        public string ReputationId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float ReputationScore = 50f;
        public ScientificReputationLevel Level = ScientificReputationLevel.Novice;
        public Dictionary<string, float> CategoryScores = new Dictionary<string, float>();
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class ReputationCalculationSettings
    {
        public float BaseReputationScore = 50f;
        public float MaxReputationScore = 1000f;
        public float DecayRate = 0.01f;
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class ReputationEvent
    {
        public string EventId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public ReputationEventType Type = ReputationEventType.Discovery;
        public float ReputationImpact = 10f;
        public DateTime Timestamp = DateTime.Now;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExpertiseRecognitionSettings
    {
        public float ExpertiseThreshold = 80f;
        public bool EnableAutoRecognition = true;
        public bool EnablePeerReview = true;
        public Dictionary<string, object> RecognitionCriteria = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExpertiseAchievement
    {
        public string AchievementId = Guid.NewGuid().ToString();
        public string AchievementName = "";
        public ExpertiseCategory Category = ExpertiseCategory.Research;
        public ExpertiseTier RequiredTier = ExpertiseTier.Expert;
        public bool IsUnlocked = false;
        public Dictionary<string, object> Requirements = new Dictionary<string, object>();
    }

    [Serializable]
    public class LegacyProgressionData
    {
        public string ProgressionId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float LegacyScore = 0f;
        public Dictionary<string, float> LegacyMetrics = new Dictionary<string, float>();
        public List<string> LegacyAchievements = new List<string>();
        public DateTime StartDate = DateTime.Now;
    }

    [Serializable]
    public class ScientificRanking
    {
        public string RankingId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public int OverallRank = 0;
        public Dictionary<string, int> CategoryRanks = new Dictionary<string, int>();
        public float RankingScore = 0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class RankingCalculationSettings
    {
        public RankingMethod Method = RankingMethod.OverallScore;
        public bool EnableCategoryRankings = true;
        public float RankingUpdateInterval = 3600f; // 1 hour
        public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>();
    }

    [Serializable]
    public class RankingCategory
    {
        public string CategoryId = Guid.NewGuid().ToString();
        public string CategoryName = "";
        public RankingCategoryType Type = RankingCategoryType.Research;
        public float CategoryWeight = 1.0f;
        public bool IsActive = true;
    }

    [Serializable]
    public class UnifiedExperience
    {
        public string ExperienceId = Guid.NewGuid().ToString();
        public string PlayerId = "";
        public float TotalExperience = 0f;
        public Dictionary<string, float> CategoryExperience = new Dictionary<string, float>();
        public int OverallLevel = 1;
        public DateTime LastUpdate = DateTime.Now;
    }

    [Serializable]
    public class ExperienceCalculationSettings
    {
        public float BaseExperienceRate = 1.0f;
        public float LevelingCurve = 1.2f;
        public bool EnableCategoryLeveling = true;
        public Dictionary<string, float> CategoryMultipliers = new Dictionary<string, float>();
    }

    [Serializable]
    public class ExperienceSource
    {
        public string SourceId = Guid.NewGuid().ToString();
        public string SourceName = "";
        public ExperienceSourceType Type = ExperienceSourceType.Research;
        public float ExperienceValue = 10f;
        public bool IsActive = true;
        public Dictionary<string, object> SourceData = new Dictionary<string, object>();
    }

    [Serializable]
    public class ExperienceMultiplier
    {
        public string MultiplierId = Guid.NewGuid().ToString();
        public string MultiplierName = "";
        public float MultiplierValue = 1.2f;
        public DateTime StartTime = DateTime.Now;
        public DateTime? EndTime = null;
        public bool IsActive = true;
        public List<string> ApplicableCategories = new List<string>();
    }

    [Serializable]
    public class MultiplierCondition
    {
        public string ConditionId = Guid.NewGuid().ToString();
        public MultiplierConditionType Type = MultiplierConditionType.Achievement;
        public Dictionary<string, object> ConditionData = new Dictionary<string, object>();
        public bool IsMet = false;
        public float MultiplierBonus = 1.1f;
    }

    [Serializable]
    public class MilestoneProgress
    {
        public string ProgressId = Guid.NewGuid().ToString();
        public string MilestoneId = "";
        public float Progress = 0f;
        public bool IsCompleted = false;
        public DateTime StartDate = DateTime.Now;
        public DateTime? CompletionDate = null;
    }

    [Serializable]
    public class SkillNodeTemplate
    {
        public string TemplateId = Guid.NewGuid().ToString();
        public string TemplateName = "";
        public ScientificSkillType SkillType = ScientificSkillType.Research;
        public Dictionary<string, object> TemplateData = new Dictionary<string, object>();
    }

    #endregion

    #region Wave 16 - Supporting Enums

    public enum ScientificTreeType
    {
        Research,
        Development,
        Innovation,
        Collaboration,
        Leadership,
        Legacy
    }

    public enum ScientificSkillType
    {
        Research,
        Analysis,
        Innovation,
        Collaboration,
        Teaching,
        Leadership,
        Discovery,
        Publication
    }

    public enum ScientificAchievementType
    {
        Research,
        Discovery,
        Innovation,
        Collaboration,
        Teaching,
        Leadership,
        Publication,
        Impact
    }

    public enum AchievementTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Legendary
    }

    public enum AchievementCategoryType
    {
        Research,
        Innovation,
        Collaboration,
        Teaching,
        Leadership,
        Impact
    }

    public enum UnlockConditionType
    {
        Prerequisites,
        Experience,
        Achievement,
        TimeGated,
        ResourceBased,
        PeerReview
    }

    public enum ResearchMilestoneType
    {
        Discovery,
        Innovation,
        Publication,
        Collaboration,
        Impact,
        Recognition
    }

    public enum ScientificReputationLevel
    {
        Novice,
        Apprentice,
        Practitioner,
        Expert,
        Authority,
        Legend
    }

    public enum ReputationEventType
    {
        Discovery,
        Publication,
        Collaboration,
        Teaching,
        Innovation,
        Recognition
    }

    public enum ExpertiseCategory
    {
        Research,
        Innovation,
        Analysis,
        Collaboration,
        Teaching,
        Leadership
    }

    public enum ExpertiseTier
    {
        Novice,
        Apprentice,
        Practitioner,
        Expert,
        Master,
        Grandmaster
    }

    public enum LegacyMilestoneType
    {
        Career,
        Impact,
        Innovation,
        Mentorship,
        Leadership,
        Recognition
    }

    public enum RankingMethod
    {
        OverallScore,
        CategoryWeighted,
        PeerRated,
        ImpactBased,
        Hybrid
    }

    public enum RankingCategoryType
    {
        Research,
        Innovation,
        Collaboration,
        Teaching,
        Leadership,
        Impact
    }

    public enum ExperienceSourceType
    {
        Research,
        Discovery,
        Innovation,
        Collaboration,
        Teaching,
        Achievement,
        Milestone,
        Recognition
    }

    public enum MultiplierConditionType
    {
        Achievement,
        TimeOfDay,
        Seasonal,
        Collaborative,
        Streak,
        Performance
    }

    #endregion

    #region Wave 18 - Final Missing System Types

    [Serializable]
    public class ProgressionAnalyticsCollector
    {
        public string CollectorId = Guid.NewGuid().ToString();
        public Dictionary<string, object> CollectedData = new Dictionary<string, object>();
        public DateTime LastCollection = DateTime.Now;
        public bool IsActive = true;
        public float CollectionInterval = 60f;
        public List<string> DataSources = new List<string>();
    }

    #endregion
}