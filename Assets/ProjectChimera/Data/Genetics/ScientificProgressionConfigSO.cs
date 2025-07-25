using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics.Scientific;
using RewardType = ProjectChimera.Data.Genetics.Scientific.RewardType;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Progression Configuration - Configuration for unified progression and skill advancement
    /// Defines skill trees, achievement systems, and cross-system progression mechanics
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Progression Config", menuName = "Project Chimera/Gaming/Scientific Progression Config")]
    public class ScientificProgressionConfigSO : ChimeraConfigSO
    {
        [Header("Progression Settings")]
        [Range(0.1f, 5.0f)] public float GlobalExperienceMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float SkillSynergyBonus = 1.5f;
        [Range(0.1f, 5.0f)] public float CrossSystemProgressionBonus = 2.0f;
        [Range(1.0f, 20.0f)] public float MasteryUnlockThreshold = 10.0f;
        [Range(0.01f, 1.0f)] public float ReputationDecayRate = 0.02f;
        
        [Header("Skill Tree Configuration")]
        public ScientificSkillTreeLibrarySO SkillTreeLibrary;
        public List<SkillTreeTemplate> SkillTreeTemplates = new List<SkillTreeTemplate>();
        public List<SkillCategoryConfig> SkillCategories = new List<SkillCategoryConfig>();
        public List<SkillSynergyDefinition> SkillSynergies = new List<SkillSynergyDefinition>();
        
        [Header("Achievement System")]
        public ScientificAchievementDatabaseSO AchievementDatabase;
        public List<AchievementCategoryConfig> AchievementCategories = new List<AchievementCategoryConfig>();
        public List<MilestoneDefinition> ProgressionMilestones = new List<MilestoneDefinition>();
        public List<CrossSystemAchievementRule> CrossSystemAchievements = new List<CrossSystemAchievementRule>();
        
        [Header("Experience System")]
        public List<ExperienceTypeConfig> ExperienceTypes = new List<ExperienceTypeConfig>();
        public List<ExperienceLevelConfig> ExperienceLevels = new List<ExperienceLevelConfig>();
        [Range(0.1f, 10.0f)] public float BaseExperienceGain = 1.0f;
        [Range(0.1f, 5.0f)] public float BaseExpertiseGain = 1.0f;
        public ExperienceConfigSO ExpertiseConfig;
        
        [Header("Reputation System")]
        public List<ScientificDisciplineConfig> DisciplineConfigurations = new List<ScientificDisciplineConfig>();
        public List<ReputationSourceConfig> ReputationSources = new List<ReputationSourceConfig>();
        [Range(0f, 1000f)] public float BaseReputation = 0f;
        [Range(0.1f, 5.0f)] public float ReputationGainMultiplier = 1.0f;
        
        [Header("Cross-System Integration")]
        public List<SystemIntegrationRule> IntegrationRules = new List<SystemIntegrationRule>();
        public List<SynergyActivationCondition> SynergyConditions = new List<SynergyActivationCondition>();
        [Range(0.1f, 1.0f)] public float SynergyThreshold = 0.7f;
        public bool EnableIntegrationBonuses = true;
        
        [Header("Progression Analytics")]
        public ProgressionAnalyticsConfigSO AnalyticsConfig;
        public bool EnableProgressionTracking = true;
        public bool EnableEfficiencyMetrics = true;
        [Range(1, 365)] public int AnalyticsRetentionDays = 90;
        
        [Header("Seasonal Progression")]
        public List<SeasonalProgressionEvent> SeasonalEvents = new List<SeasonalProgressionEvent>();
        public bool EnableSeasonalBonuses = true;
        [Range(0.1f, 3.0f)] public float SeasonalBonusMultiplier = 1.3f;
        
        [Header("Legacy System")]
        public List<LegacyAchievementConfig> LegacyAchievements = new List<LegacyAchievementConfig>();
        public List<MasteryRecognitionConfig> MasteryRecognitions = new List<MasteryRecognitionConfig>();
        public bool EnableLegacyTracking = true;
        
        [Header("Performance Settings")]
        [Range(1, 100)] public int MaxConcurrentProgressions = 50;
        [Range(0.1f, 5.0f)] public float ProgressionCalculationOptimization = 1.0f;
        public bool EnableBatchProgressionUpdates = true;
        
        [Header("Sub-System Configurations")]
        public SkillTreeConfigSO SkillTreeConfig;
        public AchievementConfigSO AchievementConfig;
        public ReputationConfigSO ReputationConfig;
        public ExperienceConfigSO ExperienceConfig;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateSkillTreeConfiguration();
            ValidateExperienceConfiguration();
            ValidateReputationConfiguration();
            ValidateIntegrationRules();
        }
        
        private void ValidateSkillTreeConfiguration()
        {
            if (SkillTreeLibrary == null)
            {
                Debug.LogWarning("SkillTreeLibrary is not assigned", this);
            }
            
            if (SkillCategories.Count == 0)
            {
                Debug.LogWarning("No skill categories defined", this);
            }
            
            foreach (var synergy in SkillSynergies)
            {
                if (synergy.RequiredSkills.Count < 2)
                {
                    Debug.LogWarning($"Skill synergy {synergy.SynergyName} requires at least 2 skills", this);
                }
            }
        }
        
        private void ValidateExperienceConfiguration()
        {
            if (ExperienceTypes.Count == 0)
            {
                Debug.LogWarning("No experience types defined", this);
            }
            
            if (ExperienceLevels.Count == 0)
            {
                Debug.LogWarning("No experience levels defined", this);
            }
            
            // Validate experience level progression
            float lastThreshold = 0f;
            foreach (var level in ExperienceLevels)
            {
                if (level.ExperienceThreshold <= lastThreshold)
                {
                    Debug.LogWarning($"Experience level {level.LevelName} has invalid threshold ordering", this);
                }
                lastThreshold = level.ExperienceThreshold;
            }
        }
        
        private void ValidateReputationConfiguration()
        {
            if (DisciplineConfigurations.Count == 0)
            {
                Debug.LogWarning("No scientific disciplines configured", this);
            }
            
            if (ReputationDecayRate > 0.5f)
            {
                Debug.LogWarning("Reputation decay rate is very high - reputation may decay too quickly", this);
            }
        }
        
        private void ValidateIntegrationRules()
        {
            foreach (var rule in IntegrationRules)
            {
                if (rule.RequiredSystems.Count < 2)
                {
                    Debug.LogWarning($"Integration rule {rule.RuleName} requires at least 2 systems", this);
                }
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public SkillCategoryConfig GetSkillCategory(SkillCategory category)
        {
            return SkillCategories.Find(c => c.Category == category);
        }
        
        public float GetCategoryMultiplier(SkillCategory category)
        {
            var categoryConfig = GetSkillCategory(category);
            return categoryConfig?.ExperienceMultiplier ?? 1.0f;
        }
        
        public ScientificDisciplineConfig GetDisciplineConfig(ScientificDiscipline discipline)
        {
            return DisciplineConfigurations.Find(d => d.Discipline == discipline);
        }
        
        public float GetDisciplineMultiplier(ScientificDiscipline discipline)
        {
            var disciplineConfig = GetDisciplineConfig(discipline);
            return disciplineConfig?.ReputationMultiplier ?? 1.0f;
        }
        
        public ExperienceTypeConfig GetExperienceTypeConfig(ExperienceType experienceType)
        {
            return ExperienceTypes.Find(e => e.Type == experienceType);
        }
        
        public ExperienceLevelConfig GetExperienceLevel(float totalExperience)
        {
            ExperienceLevelConfig currentLevel = null;
            foreach (var level in ExperienceLevels)
            {
                if (totalExperience >= level.ExperienceThreshold)
                {
                    currentLevel = level;
                }
                else
                {
                    break;
                }
            }
            return currentLevel;
        }
        
        public List<SkillSynergyDefinition> GetApplicableSynergies(List<string> unlockedSkills)
        {
            return SkillSynergies.FindAll(synergy => 
                synergy.RequiredSkills.TrueForAll(required => unlockedSkills.Contains(required)));
        }
        
        public SystemIntegrationRule GetIntegrationRule(List<ScientificGamingSystem> activeSystems)
        {
            return IntegrationRules.Find(rule => 
                rule.RequiredSystems.TrueForAll(required => activeSystems.Contains(required)));
        }
        
        public float CalculateProgressionEfficiency(ProgressionMetrics metrics)
        {
            var timeSpent = metrics.SessionDuration;
            if (timeSpent <= 0f) return 0f;
            
            var progressionValue = (metrics.SkillsUnlocked * 10f) + 
                                 (metrics.AchievementsEarned * 25f) + 
                                 (metrics.MilestonesReached * 50f);
            
            return progressionValue / timeSpent;
        }
        
        public bool IsSynergyConditionMet(SynergyActivationCondition condition, ProgressionState state)
        {
            return condition.ConditionType switch
            {
                SynergyConditionType.ExperienceThreshold => state.TotalExperience >= condition.RequiredValue,
                SynergyConditionType.SkillCount => state.UnlockedSkillsCount >= condition.RequiredValue,
                SynergyConditionType.ReputationLevel => state.AverageReputation >= condition.RequiredValue,
                SynergyConditionType.SystemBalance => state.SystemBalanceScore >= condition.RequiredValue,
                _ => false
            };
        }
        
        public List<SeasonalProgressionEvent> GetActiveSeasonalEvents()
        {
            var currentSeason = GetCurrentSeason();
            return SeasonalEvents.FindAll(e => e.Season == currentSeason && e.IsActive);
        }
        
        public MasteryRecognitionConfig GetMasteryRecognition(SkillCategory category, float masteryLevel)
        {
            return MasteryRecognitions.Find(m => m.Category == category && masteryLevel >= m.RequiredMasteryLevel);
        }
        
        #endregion
        
        #region Helper Methods
        
        private GameSeason GetCurrentSeason()
        {
            var month = System.DateTime.Now.Month;
            return month switch
            {
                12 or 1 or 2 => GameSeason.Winter,
                3 or 4 or 5 => GameSeason.Spring,
                6 or 7 or 8 => GameSeason.Summer,
                9 or 10 or 11 => GameSeason.Fall,
                _ => GameSeason.Spring
            };
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class SkillTreeTemplate
    {
        public string TemplateName;
        public SkillCategory Category;
        [Range(1, 100)] public int MaxSkillNodes = 50;
        [Range(0.1f, 5.0f)] public float ExperienceMultiplier = 1.0f;
        public List<SkillTreeTier> Tiers = new List<SkillTreeTier>();
        public string Description;
    }
    
    [System.Serializable]
    public class SkillTreeTier
    {
        public string TierName;
        [Range(1, 20)] public int TierLevel = 1;
        [Range(1, 50)] public int NodesInTier = 5;
        [Range(0.1f, 10.0f)] public float UnlockCost = 1.0f;
        public List<string> RequiredPreviousSkills = new List<string>();
    }
    
    [System.Serializable]
    public class SkillCategoryConfig
    {
        public string CategoryName;
        public SkillCategory Category;
        [Range(0.1f, 5.0f)] public float ExperienceMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float SynergyBonus = 1.2f;
        public Color CategoryColor = Color.white;
        public List<string> CoreSkills = new List<string>();
        public string Description;
        public Sprite CategoryIcon;
    }
    
    [System.Serializable]
    public class SkillSynergyDefinition
    {
        public string SynergyName;
        public SkillSynergyType SynergyType;
        public List<string> RequiredSkills = new List<string>();
        [Range(0.1f, 5.0f)] public float SynergyMultiplier = 1.5f;
        public List<SynergyEffect> Effects = new List<SynergyEffect>();
        public string Description;
    }
    
    
    [System.Serializable]
    public class AchievementCategoryConfig
    {
        public string CategoryName;
        public AchievementCategory Category;
        [Range(0.1f, 5.0f)] public float ReputationMultiplier = 1.0f;
        public Color CategoryColor = Color.white;
        public List<string> FeaturedAchievements = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class MilestoneDefinition
    {
        public string MilestoneName;
        public MilestoneType Type;
        public List<MilestoneCriterion> Criteria = new List<MilestoneCriterion>();
        [Range(0.1f, 10.0f)] public float RewardValue = 2.0f;
        public List<string> UnlockedFeatures = new List<string>();
        public bool IsLegacyMilestone = false;
    }
    
    [System.Serializable]
    public class MilestoneCriterion
    {
        public string CriterionName;
        public MilestoneCriterionType CriterionType;
        public float TargetValue;
        public string Description;
    }
    
    [System.Serializable]
    public class CrossSystemAchievementRule
    {
        public string AchievementName;
        public List<ScientificGamingSystem> RequiredSystems = new List<ScientificGamingSystem>();
        public List<CrossSystemRequirement> Requirements = new List<CrossSystemRequirement>();
        [Range(0.1f, 10.0f)] public float CompletionReward = 5.0f;
        public bool IsLegacyAchievement = false;
    }
    
    [System.Serializable]
    public class CrossSystemRequirement
    {
        public string RequirementName;
        public ScientificGamingSystem System;
        public CrossSystemRequirementType RequirementType;
        public float RequiredValue;
    }
    
    [System.Serializable]
    public class ExperienceTypeConfig
    {
        public string TypeName;
        public ExperienceType Type;
        [Range(0.1f, 10.0f)] public float BaseValue = 1.0f;
        [Range(0.1f, 5.0f)] public float GrowthRate = 1.2f;
        public List<ExperienceModifier> Modifiers = new List<ExperienceModifier>();
        public string Description;
    }
    
    [System.Serializable]
    public class ExperienceModifier
    {
        public string ModifierName;
        public ExperienceModifierType ModifierType;
        public float ModifierValue;
        public List<ExperienceCondition> Conditions = new List<ExperienceCondition>();
    }
    
    [System.Serializable]
    public class ExperienceCondition
    {
        public string ConditionName;
        public ExperienceConditionType ConditionType;
        public float ConditionValue;
    }
    
    [System.Serializable]
    public class ExperienceLevelConfig
    {
        public string LevelName;
        public ExperienceLevel Level;
        [Range(0f, 10000f)] public float ExperienceThreshold = 0f;
        [Range(0.1f, 5.0f)] public float LevelMultiplier = 1.0f;
        public List<string> UnlockedFeatures = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class ScientificDisciplineConfig
    {
        public string DisciplineName;
        public ScientificDiscipline Discipline;
        [Range(0.1f, 5.0f)] public float ReputationMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float ExpertiseBonus = 1.2f;
        public Color DisciplineColor = Color.white;
        public List<string> CoreCompetencies = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class ReputationSourceConfig
    {
        public string SourceName;
        public ReputationSource Source;
        [Range(0.1f, 10.0f)] public float BaseReputationValue = 1.0f;
        [Range(0.1f, 5.0f)] public float QualityMultiplier = 1.0f;
        public List<ScientificDiscipline> ApplicableDisciplines = new List<ScientificDiscipline>();
    }
    
    [System.Serializable]
    public class SystemIntegrationRule
    {
        public string RuleName;
        public List<ScientificGamingSystem> RequiredSystems = new List<ScientificGamingSystem>();
        [Range(0.1f, 5.0f)] public float IntegrationBonus = 2.0f;
        public List<IntegrationEffect> Effects = new List<IntegrationEffect>();
        public string Description;
    }
    
    [System.Serializable]
    public class IntegrationEffect
    {
        public string EffectName;
        public IntegrationEffectType EffectType;
        public float EffectValue;
        public List<SkillCategory> AffectedCategories = new List<SkillCategory>();
    }
    
    [System.Serializable]
    public class SynergyActivationCondition
    {
        public string ConditionName;
        public SynergyConditionType ConditionType;
        public float RequiredValue;
        public string Description;
    }
    
    [System.Serializable]
    public class SeasonalProgressionEvent
    {
        public string EventName;
        public GameSeason Season;
        public bool IsActive = true;
        [Range(0.1f, 3.0f)] public float SeasonalBonus = 1.2f;
        public List<SeasonalObjective> Objectives = new List<SeasonalObjective>();
        public List<SeasonalReward> Rewards = new List<SeasonalReward>();
    }
    
    [System.Serializable]
    public class SeasonalObjective
    {
        public string ObjectiveName;
        public SeasonalObjectiveType ObjectiveType;
        public float TargetValue;
        [Range(0.1f, 5.0f)] public float CompletionReward = 2.0f;
    }
    
    [System.Serializable]
    public class SeasonalReward
    {
        public string RewardName;
        public SeasonalRewardType RewardType;
        public float RewardValue;
        public bool IsExclusive = false;
    }
    
    [System.Serializable]
    public class LegacyAchievementConfig
    {
        public string AchievementName;
        public LegacyAchievementType Type;
        public List<LegacyCriterion> Criteria = new List<LegacyCriterion>();
        [Range(1.0f, 20.0f)] public float LegacyValue = 10.0f;
        public List<string> LegacyUnlocks = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class LegacyCriterion
    {
        public string CriterionName;
        public LegacyCriterionType CriterionType;
        public float RequiredValue;
        public string Description;
    }
    
    [System.Serializable]
    public class MasteryRecognitionConfig
    {
        public string RecognitionName;
        public SkillCategory Category;
        [Range(1.0f, 100.0f)] public float RequiredMasteryLevel = 50.0f;
        [Range(0.1f, 10.0f)] public float RecognitionValue = 5.0f;
        public List<string> MasteryUnlocks = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class ProgressionMetrics
    {
        public float SessionDuration;
        public int SkillsUnlocked;
        public int AchievementsEarned;
        public int MilestonesReached;
        public float TotalExperience;
        public float AverageReputation;
    }
    
    [System.Serializable]
    public class ProgressionState
    {
        public float TotalExperience;
        public int UnlockedSkillsCount;
        public float AverageReputation;
        public float SystemBalanceScore;
        public List<ScientificGamingSystem> ActiveSystems = new List<ScientificGamingSystem>();
    }
    
    #endregion
    
    #region Enums
    
    public enum SkillSynergyType
    {
        Multiplicative,
        Additive,
        Conditional,
        Progressive,
        Threshold,
        Compound
    }
    
    public enum ProgressionSynergyEffectType
    {
        ExperienceBonus,
        SkillEfficiency,
        UnlockDiscount,
        ReputationBonus,
        ProgressionSpeed,
        QualityImprovement
    }
    
    
    public enum MilestoneType
    {
        ExperienceThreshold,
        SkillMastery,
        AchievementCount,
        ReputationLevel,
        SystemIntegration,
        CommunityContribution,
        InnovationBreakthrough,
        SeasonalCompletion
    }
    
    public enum MilestoneCriterionType
    {
        TotalExperience,
        SkillsUnlocked,
        AchievementsEarned,
        ReputationPoints,
        SystemsIntegrated,
        InnovationsCreated,
        MentorshipHours,
        CommunityContributions
    }
    
    public enum CrossSystemRequirementType
    {
        ExperienceLevel,
        SkillMastery,
        AchievementCount,
        ReputationThreshold,
        ParticipationLevel,
        ContributionValue,
        InnovationRating,
        CommunityStanding
    }
    
    public enum ExperienceModifierType
    {
        TimeBasedBonus,
        QualityMultiplier,
        StreakBonus,
        SynergyBonus,
        SeasonalBonus,
        CommunityBonus,
        InnovationBonus,
        MasteryBonus
    }
    
    public enum ExperienceConditionType
    {
        TimeOfDay,
        QualityThreshold,
        StreakLength,
        SkillLevel,
        ReputationLevel,
        CommunityRank,
        SeasonalEvent,
        SystemIntegration
    }
    
    public enum ReputationSource
    {
        SkillDemonstration,
        AchievementUnlock,
        CommunityContribution,
        MentorshipActivity,
        InnovationSharing,
        CompetitiveSuccess,
        PeerEndorsement,
        ExpertRecognition
    }
    
    public enum IntegrationEffectType
    {
        CrossSystemBonus,
        SynergyActivation,
        EfficiencyIncrease,
        QualityBonus,
        SpeedBonus,
        AccessUnlock,
        FeatureEnable,
        RewardMultiplier
    }
    
    public enum SynergyConditionType
    {
        ExperienceThreshold,
        SkillCount,
        ReputationLevel,
        SystemBalance,
        AchievementCount,
        CommunityRank,
        MasteryLevel,
        InnovationScore
    }
    
    public enum SeasonalRewardType
    {
        ExperienceBonus,
        SkillUnlock,
        ReputationBoost,
        FeatureAccess,
        CosmeticReward,
        TitleUnlock,
        SpecialRecognition,
        LegacyContribution
    }
    
    public enum LegacyAchievementType
    {
        PioneerStatus,
        MasterMentor,
        InnovationLeader,
        CommunityPillar,
        KnowledgeKeeper,
        SkillLegend,
        InfluentialContributor,
        ScientificBreakthrough
    }
    
    public enum LegacyCriterionType
    {
        YearsOfContribution,
        MentorshipImpact,
        InnovationsCreated,
        CommunityInfluence,
        KnowledgeShared,
        SkillsMastered,
        RecognitionReceived,
        LegacyBuilt
    }
    
    
    #endregion
}