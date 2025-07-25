using UnityEngine;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Gaming Enums - Shared enum definitions for Enhanced Scientific Gaming System v2.0
    /// Contains all common enums used across genetics, aromatics, competition, and community systems
    /// Single source of truth for enum definitions to prevent duplication
    /// </summary>
    
    #region Core Gaming Enums
    
    public enum GameSeason
    {
        Spring,
        Summer,
        Fall,
        Winter
    }
    
    public enum ScientificGamingSystem
    {
        Genetics,
        Aromatics,
        Competition,
        Community
    }
    
    public enum ExperienceType
    {
        GeneticDiscovery,
        BreedingSuccess,
        AromaticMastery,
        CompetitiveAchievement,
        CommunityContribution,
        InnovationCreation,
        MentorshipActivity,
        ResearchAdvancement
    }
    
    public enum ExperienceLevel
    {
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Grandmaster,
        Legend
    }
    
    public enum DifficultyLevel
    {
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Grandmaster
    }
    
    public enum SkillCategory
    {
        GeneticAnalysis,
        BreedingMastery,
        AromaticIdentification,
        TerpeneBlending,
        CompetitivePlanning,
        SocialCollaboration,
        ResearchMethodology,
        InnovationCreation
    }
    
    public enum ScientificDiscipline
    {
        Genetics,
        Chemistry,
        Biology,
        Aromatics,
        Analytics,
        Innovation,
        Community,
        Competition
    }
    
    #endregion
    
    #region Breeding & Genetics Enums
    
    public enum BreedingChallengeType
    {
        BasicCross,
        TraitOptimization,
        MultiGenerationBreeding,
        NovelCombination,
        PrecisionBreeding,
        InnovationChallenge,
        CommunityCollaboration,
        CompetitiveBreeding,
        QualityMaximization,
        YieldChallenge
    }
    
    public enum BreedingObjectiveType
    {
        YieldMaximization,
        PotencyOptimization,
        FlavorEnhancement,
        DiseaseResistance,
        GrowthOptimization,
        NovelPhenotype,
        TraitBalance,
        InnovativeBreeding
    }
    
    public enum GeneticPuzzleType
    {
        Dominance,
        Epistasis,
        Linkage,
        Polygenic,
        QuantitativeTrait,
        GeneInteraction,
        PopulationGenetics,
        BreedingStrategy
    }
    
    #endregion
    
    #region Aromatic & Terpene Enums
    
    public enum TerpeneCategory
    {
        Monoterpenes,
        Sesquiterpenes,
        Diterpenes,
        Alcohols,
        Esters,
        Aldehydes,
        Ketones,
        Phenols,
        Thiols,
        Oxides
    }
    
    public enum FlavorProfileType
    {
        Citrus,
        Floral,
        Woody,
        Herbal,
        Spicy,
        Sweet,
        Earthy,
        Fruity,
        Pine,
        Diesel,
        Skunky,
        Minty
    }
    
    public enum TerpeneRole
    {
        Primary,
        Secondary,
        Accent,
        Modifier,
        Synergist
    }
    
    public enum SensoryTrainingType
    {
        TerpeneIdentification,
        ConcentrationDetection,
        BlendAnalysis,
        QualityAssessment,
        SynergyRecognition,
        FlavorProfiling,
        AromaticMemory,
        SensoryDiscrimination
    }
    
    public enum SensoryDifficulty
    {
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum BlendingChallengeType
    {
        RecipeRecreation,
        CreativeBlending,
        FlavorMatching,
        SynergyOptimization,
        InnovativeCreation,
        ThemeBasedBlending,
        CompetitiveBlending,
        CollaborativeBlending
    }
    
    #endregion
    
    #region Competition Enums
    
    public enum CompetitionCategory
    {
        GeneticsBreeding,
        AromaticMastery,
        InnovationChallenge,
        CrossDisciplinary,
        CommunityCollaboration,
        SpeedBreeding,
        PrecisionAnalysis,
        CreativeBlending
    }
    
    public enum CompetitionTier
    {
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Elite,
        Champion,
        Legend
    }
    
    public enum CompetitionResult
    {
        Victory,
        RunnerUp,
        SemiFinalElimination,
        QuarterFinalElimination,
        EarlyElimination,
        Participation,
        Disqualification,
        NoShow
    }
    
    public enum TournamentFormat
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Swiss,
        Ladder,
        KingOfTheHill,
        TeamCompetition,
        Mixed
    }
    
    public enum CompetitionPhaseType
    {
        Qualification,
        Preliminary,
        Quarterfinal,
        Semifinal,
        Final,
        Championship,
        Playoff,
        Challenge
    }
    
    public enum PhaseObjectiveType
    {
        YieldTarget,
        QualityScore,
        InnovationRating,
        TimeCompletion,
        EfficiencyScore,
        AccuracyRating,
        CreativityScore,
        CollaborationRating
    }
    
    #endregion
    
    #region Community & Project Enums
    
    public enum ProjectType
    {
        GeneticsResearch,
        AromaticInnovation,
        CommunityInitiative,
        KnowledgeDocumentation,
        ToolDevelopment,
        EducationalContent,
        CompetitiveTeaming,
        CrossDisciplinary
    }
    
    public enum ProjectComplexity
    {
        Simple,
        Moderate,
        Complex,
        Advanced,
        Experimental
    }
    
    public enum ProjectRole
    {
        ProjectLeader,
        ResearchSpecialist,
        DataAnalyst,
        CommunityLiaison,
        QualityAssurance,
        Documentation,
        Innovation,
        Mentorship
    }
    
    public enum ProjectObjectiveType
    {
        ResearchTarget,
        CommunityEngagement,
        KnowledgeContribution,
        InnovationGoal,
        QualityMetric,
        TimelineMilestone,
        CollaborationMetric,
        ImpactMeasurement
    }
    
    public enum SeasonalObjectiveType
    {
        CommunityGrowth,
        KnowledgeSharing,
        CollaborationIncrease,
        InnovationBoost,
        MentorshipExpansion,
        SkillDevelopment,
        RecognitionProgram,
        EventParticipation
    }
    
    #endregion
    
    #region Achievement & Progression Enums
    
    // AchievementCategory moved to AchievementDataStructures.cs to resolve namespace conflict
    
    public enum AchievementCriterionType
    {
        TournamentWins,
        ConsecutiveWins,
        ScoreThreshold,
        RatingThreshold,
        ParticipationCount,
        InnovationCount,
        MentorshipHours,
        CommunityContribution
    }
    
    public enum RewardComponentType
    {
        Experience,
        Reputation,
        UnlockFeature,
        SpecialTitle,
        CosmeticItem,
        AccessToken,
        MentorshipCredit,
        InnovationBonus
    }
    
    public enum CommunityRank
    {
        Newcomer,
        Member,
        Contributor,
        Collaborator,
        Expert,
        Mentor,
        Leader,
        Elder
    }
    
    public enum EndorsementType
    {
        SkillRecognition,
        QualityWork,
        Mentorship,
        Innovation,
        Collaboration,
        Leadership,
        Knowledge,
        Community
    }
    
    public enum CompetitiveAchievementType
    {
        TournamentWin,
        ConsecutiveWins,
        RankingAchievement,
        SkillMastery,
        InnovationRecognition,
        CommunityLeadership,
        LegacyContribution,
        EliteStatus
    }
    
    public enum ExpertiseArea
    {
        GeneticAnalysis,
        BreedingTechniques,
        AromaticIdentification,
        TerpeneBlending,
        QualityAssessment,
        ResearchMethodology,
        DataAnalysis,
        CommunityBuilding
    }
    
    // ContributionType moved to AchievementDataStructures.cs to resolve namespace conflict
    
    public enum CompetitionRank
    {
        Unranked,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        Grandmaster
    }
    
    public enum LegacyMilestoneType
    {
        FoundingContributor,
        KnowledgePioneer,
        CommunityBuilder,
        InnovationLeader,
        MentorshipMaster,
        SkillLegend,
        ResearchPioneer,
        LegacyBuilder
    }
    
    public enum CommunityEventType
    {
        KnowledgeSharing,
        CollaborativeProject,
        MentorshipEvent,
        InnovationShowcase,
        SkillWorkshop,
        NetworkingSession,
        CommunityChallenge,
        RecognitionCeremony
    }
    
    public enum InnovationShowcaseType
    {
        GeneticInnovation,
        AromaticCreation,
        MethodologyAdvancement,
        ToolDevelopment,
        ResearchBreakthrough,
        CommunityContribution,
        CrossDisciplinary,
        LegacyProject
    }
    
    public enum ConsultationType
    {
        TechnicalGuidance,
        ResearchReview,
        MethodologyConsultation,
        CareerMentorship,
        ProjectPlanning,
        InnovationStrategy,
        QualityAssessment,
        CommunityBuilding
    }
    
    public enum MentorshipActivityType
    {
        OneOnOneSession,
        GroupWorkshop,
        ProjectReview,
        SkillAssessment,
        CareerGuidance,
        ResearchDiscussion,
        PracticalTraining,
        ProgressEvaluation
    }
    
    public enum RoleResponsibilityType
    {
        PrimaryTask,
        SecondaryTask,
        Oversight,
        Quality,
        Documentation,
        Communication,
        Innovation,
        Mentorship
    }
    
    public enum MentorshipProgramType
    {
        BasicGuidance,
        AdvancedMentorship,
        SpecializedTraining,
        LeadershipDevelopment,
        InnovationMentorship,
        CommunityBuilding,
        LegacyProgram,
        CrossDisciplinary
    }
    
    public enum CrossSystemSynergyType
    {
        GeneticsAromatics,
        CompetitionCommunity,
        InnovationCollaboration,
        SkillIntegration,
        KnowledgeSynergy,
        MethodologyFusion,
        ExperienceAmplification,
        LegacySynergy
    }
    
    public enum ScientificGamingSystemType
    {
        Genetics,
        Aromatics,
        Competition,
        Community,
        Innovation,
        Analytics,
        Collaboration,
        Legacy
    }
    
    public enum ScientificGamingEventType
    {
        SystemActivation,
        AchievementUnlock,
        SynergyActivation,
        ProgressionMilestone,
        CommunityEvent,
        InnovationCreated,
        CompetitionResult,
        LegacyContribution
    }
    
    public enum SeasonalEventType
    {
        SpringAwakening,
        SummerGrowth,
        FallHarvest,
        WinterReflection,
        SpecialEvent,
        CommunityGathering,
        InnovationFair,
        LegacyCelebration
    }
    
    public enum GeneticConstraintType
    {
        DominancePattern,
        AlleleInteraction,
        GeneExpression,
        TraitLimitation,
        BreedingRestriction,
        PhenotypeBoundary,
        InheritanceRule,
        EnvironmentalSensitivity
    }
    
    public enum ContributionQuality
    {
        Basic,
        Good,
        Excellent,
        Outstanding,
        Groundbreaking
    }
    
    public enum InnovationCategory
    {
        GeneticInnovation,
        AromaticDiscovery,
        MethodologyAdvancement,
        ToolDevelopment,
        ProcessOptimization,
        QualityImprovement,
        EfficiencyGain,
        NovelApproach
    }
    
    public enum MentorshipMilestone
    {
        FirstMentee,
        SuccessfulGuidance,
        SkillTransfer,
        KnowledgeSharing,
        CommunityImpact,
        LegacyBuilding,
        ExpertRecognition,
        MasterMentor
    }
    
    public enum SocialAchievementType
    {
        CommunityContribution,
        MentorshipExcellence,
        CollaborativeLeadership,
        KnowledgeSharing,
        PeerRecognition,
        InnovationSupport,
        CommunityBuilding,
        LegacyContribution
    }
    
    public enum SocialCriterionType
    {
        CommunityEngagement,
        MentorshipHours,
        CollaborationCount,
        KnowledgeContributions,
        PeerEndorsements,
        ProjectLeadership,
        InnovationSupport,
        LegacyImpact
    }
    
    public enum AchievementRequirementType
    {
        ExperienceLevel,
        SkillMastery,
        CommunityRank,
        ProjectCompletion,
        InnovationCount,
        MentorshipHours,
        CollaborationScore,
        ReputationThreshold
    }
    
    public enum AromaticSynergyType
    {
        TerpeneBlending,
        FlavorEnhancement,
        AromaOptimization,
        SensoryBalance,
        ComplexityCreation,
        NovelCombination,
        QualityAmplification,
        ExperienceEnhancement
    }
    
    public enum CompetitionType
    {
        Individual,
        Team,
        Collaborative,
        Innovation,
        Speed,
        Quality,
        Precision,
        Creative
    }
    
    public enum IdentificationChallengeType
    {
        BasicAroma,
        ComplexBlend,
        BlindTasting,
        TerpeneProfile,
        QualityAssessment,
        ComparativeAnalysis,
        ExpertValidation,
        AdvancedSensory
    }
    
    public enum MemoryExerciseType
    {
        AromaticRecall,
        SequenceMemory,
        PatternRecognition,
        SensoryMapping,
        FlavorAssociation,
        ComplexityBuilding,
        RetentionTraining,
        ExpertChallenge
    }
    
    public enum SensoryModality
    {
        Olfactory,
        Gustatory,
        Visual,
        Tactile,
        Auditory,
        Multimodal,
        Synesthetic,
        Enhanced
    }
    
    public enum GeopoliticalEvent
    {
        LegalizationChange,
        RegulationUpdate,
        MarketShift,
        PolicyChange,
        InternationalTrade,
        ComplianceUpdate,
        TaxationChange,
        LicensingUpdate
    }
    
    public enum RegulatoryChange
    {
        QualityStandards,
        TestingRequirements,
        LabelingRules,
        PackagingRegulations,
        DistributionLaws,
        ConsumptionLimits,
        LicenseRequirements,
        ComplianceAudits
    }
    
    public enum CreditRating
    {
        Poor,
        Fair,
        Good,
        VeryGood,
        Excellent,
        Outstanding
    }
    
    public enum BusinessEducationEnrollment
    {
        Success,
        Failure,
        InProgress,
        Completed,
        Cancelled,
        Pending,
        Reason
    }
    
    public enum BusinessCertificationResult
    {
        Success,
        Failure,
        InProgress,
        Expired,
        Pending,
        Reason
    }
    
    public enum IndustryConnectionResult
    {
        Success,
        Failure,
        Pending,
        Established,
        Lost,
        Strengthened,
        Reason
    }
    
    public enum BusinessConsortium
    {
        FoundingDate,
        MemberCount,
        IndustryFocus,
        GeographicScope,
        AnnualRevenue,
        MarketShare,
        Reputation,
        Partnerships
    }
    
    #endregion
}