using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Competition Configuration - Configuration for tournament and recognition systems
    /// Defines competition parameters, matchmaking rules, and achievement recognition mechanics
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Competition Config", menuName = "Project Chimera/Gaming/Scientific Competition Config")]
    public class ScientificCompetitionConfigSO : ChimeraConfigSO
    {
        [Header("Competition Settings")]
        [Range(0.1f, 3.0f)] public float CompetitiveProgressionRate = 1.2f;
        [Range(0.1f, 5.0f)] public float ReputationGainMultiplier = 1.8f;
        [Range(1.0f, 10.0f)] public float EliteTierThreshold = 6.0f;
        [Range(0.1f, 1.0f)] public float MatchmakingTolerance = 0.3f;
        
        [Header("Tournament Configuration")]
        public List<TournamentTemplate> TournamentTemplates = new List<TournamentTemplate>();
        public List<CompetitionCategory> AvailableCategories = new List<CompetitionCategory>();
        public List<CompetitionTier> AvailableTiers = new List<CompetitionTier>();
        
        [Header("Matchmaking System")]
        [Range(0.1f, 2.0f)] public float SkillBasedMatchmakingWeight = 1.0f;
        [Range(0.1f, 2.0f)] public float ExperienceBasedWeight = 0.8f;
        [Range(0.1f, 2.0f)] public float ReputationBasedWeight = 1.2f;
        [Range(1, 60)] public int MaxMatchmakingTime = 30;
        public bool EnableCrossRegionMatching = true;
        
        [Header("Recognition System")]
        public List<CompetitiveAchievementConfig> AchievementConfigurations = new List<CompetitiveAchievementConfig>();
        public List<ReputationTierConfig> ReputationTiers = new List<ReputationTierConfig>();
        public List<LegacyMilestoneConfig> LegacyMilestones = new List<LegacyMilestoneConfig>();
        
        [Header("Ranking System")]
        [Range(1, 100)] public int MaxRankingTier = 50;
        [Range(0.1f, 1.0f)] public float RankingDecayRate = 0.1f;
        [Range(1, 365)] public int SeasonLength = 90;
        public bool EnableSeasonalReset = true;
        
        [Header("Competition Analytics")]
        public bool EnablePerformanceTracking = true;
        public bool EnableSkillAssessment = true;
        public bool EnableCompetitiveMetrics = true;
        [Range(1, 100)] public int MetricsRetentionDays = 30;
        
        [Header("Reward System")]
        public List<CompetitionRewardConfig> RewardConfigurations = new List<CompetitionRewardConfig>();
        [Range(0.1f, 5.0f)] public float WinStreakMultiplier = 1.5f;
        [Range(0.1f, 3.0f)] public float TournamentWinBonus = 2.0f;
        public bool EnableEliteRewards = true;
        
        [Header("Competition Types")]
        public List<CompetitionType> AvailableCompetitionTypes = new List<CompetitionType>();
        public List<TournamentFormat> AvailableTournamentFormats = new List<TournamentFormat>();
        
        [Header("Entry Requirements")]
        public List<CompetitionEntryRequirement> EntryRequirements = new List<CompetitionEntryRequirement>();
        public bool RequireQualification = true;
        public bool EnableSkillGating = true;
        
        [Header("Performance Settings")]
        [Range(1, 100)] public int MaxConcurrentTournaments = 20;
        [Range(1, 1000)] public int MaxParticipantsPerTournament = 64;
        [Range(0.1f, 5.0f)] public float CompetitionProcessingOptimization = 1.0f;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateTournamentTemplates();
            ValidateReputationTiers();
            ValidateRewardConfigurations();
            ValidateCompetitionSettings();
        }
        
        private void ValidateTournamentTemplates()
        {
            if (TournamentTemplates.Count == 0)
            {
                Debug.LogWarning("No tournament templates defined", this);
            }
            
            foreach (var template in TournamentTemplates)
            {
                if (template.MinParticipants > template.MaxParticipants)
                {
                    Debug.LogError($"Tournament template {template.TemplateName} has invalid participant range", this);
                }
            }
        }
        
        private void ValidateReputationTiers()
        {
            if (ReputationTiers.Count == 0)
            {
                Debug.LogWarning("No reputation tiers defined", this);
                return;
            }
            
            float lastThreshold = 0f;
            foreach (var tier in ReputationTiers)
            {
                if (tier.ReputationThreshold <= lastThreshold)
                {
                    Debug.LogWarning($"Reputation tier {tier.TierName} has invalid threshold ordering", this);
                }
                lastThreshold = tier.ReputationThreshold;
            }
        }
        
        private void ValidateRewardConfigurations()
        {
            foreach (var reward in RewardConfigurations)
            {
                if (reward.RewardValue <= 0f)
                {
                    Debug.LogWarning($"Reward configuration {reward.RewardName} has invalid reward value", this);
                }
            }
        }
        
        private void ValidateCompetitionSettings()
        {
            if (MatchmakingTolerance > 0.8f)
            {
                Debug.LogWarning("Matchmaking tolerance is very high - may result in poor matches", this);
            }
            
            if (EliteTierThreshold < 2.0f)
            {
                Debug.LogWarning("Elite tier threshold is very low - may be too easy to achieve", this);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public TournamentTemplate GetTournamentTemplate(CompetitionCategory category, CompetitionTier tier)
        {
            return TournamentTemplates.Find(t => t.Category == category && t.Tier == tier);
        }
        
        public ReputationTierConfig GetReputationTier(float currentReputation)
        {
            ReputationTierConfig currentTier = null;
            foreach (var tier in ReputationTiers)
            {
                if (currentReputation >= tier.ReputationThreshold)
                {
                    currentTier = tier;
                }
                else
                {
                    break;
                }
            }
            return currentTier;
        }
        
        public CompetitionRewardConfig GetRewardConfiguration(CompetitionResult result, CompetitionTier tier)
        {
            return RewardConfigurations.Find(r => r.ResultType == result && r.ApplicableTier == tier);
        }
        
        public bool CanEnterCompetition(CompetitionCategory category, CompetitionTier tier, CompetitorProfile profile)
        {
            var requirement = GetEntryRequirement(category, tier);
            if (requirement == null) return true;
            
            return EvaluateEntryRequirement(requirement, profile);
        }
        
        public CompetitionEntryRequirement GetEntryRequirement(CompetitionCategory category, CompetitionTier tier)
        {
            return EntryRequirements.Find(r => r.Category == category && r.Tier == tier);
        }
        
        public float CalculateMatchmakingScore(CompetitorProfile profileA, CompetitorProfile profileB)
        {
            var skillDifference = Mathf.Abs(profileA.SkillRating - profileB.SkillRating);
            var experienceDifference = Mathf.Abs(profileA.ExperienceLevel - profileB.ExperienceLevel);
            var reputationDifference = Mathf.Abs(profileA.Reputation - profileB.Reputation);
            
            var normalizedSkillDiff = skillDifference / 100f;
            var normalizedExpDiff = experienceDifference / 10f;
            var normalizedRepDiff = reputationDifference / 100f;
            
            var weightedScore = (normalizedSkillDiff * SkillBasedMatchmakingWeight) +
                               (normalizedExpDiff * ExperienceBasedWeight) +
                               (normalizedRepDiff * ReputationBasedWeight);
            
            return 1f - Mathf.Clamp01(weightedScore / 3f);
        }
        
        public List<LegacyMilestoneConfig> GetAvailableLegacyMilestones(CompetitorProfile profile)
        {
            return LegacyMilestones.FindAll(m => EvaluateLegacyRequirements(m, profile));
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool EvaluateEntryRequirement(CompetitionEntryRequirement requirement, CompetitorProfile profile)
        {
            if (profile.SkillRating < requirement.MinimumSkillRating) return false;
            if (profile.ExperienceLevel < requirement.MinimumExperienceLevel) return false;
            if (profile.Reputation < requirement.MinimumReputation) return false;
            if (profile.TournamentWins < requirement.MinimumTournamentWins) return false;
            
            foreach (var requiredAchievement in requirement.RequiredAchievements)
            {
                if (!profile.UnlockedAchievements.Contains(requiredAchievement))
                    return false;
            }
            
            return true;
        }
        
        private bool EvaluateLegacyRequirements(LegacyMilestoneConfig milestone, CompetitorProfile profile)
        {
            return profile.TournamentWins >= milestone.RequiredTournamentWins &&
                   profile.Reputation >= milestone.RequiredReputation &&
                   profile.ConsecutiveSeasonRanking >= milestone.RequiredSeasonRanking;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class TournamentTemplate
    {
        public string TemplateName;
        public CompetitionCategory Category;
        public CompetitionTier Tier;
        public TournamentFormat Format;
        [Range(2, 1000)] public int MinParticipants = 8;
        [Range(2, 1000)] public int MaxParticipants = 64;
        [Range(1, 168)] public int DurationHours = 24;
        [Range(0.1f, 5.0f)] public float RewardMultiplier = 1.0f;
        public List<CompetitionPhase> Phases = new List<CompetitionPhase>();
        public string Description;
        public Sprite TournamentIcon;
    }
    
    [System.Serializable]
    public class CompetitionPhase
    {
        public string PhaseName;
        public CompetitionPhaseType PhaseType;
        [Range(1, 168)] public int DurationHours = 6;
        [Range(0.1f, 2.0f)] public float DifficultyMultiplier = 1.0f;
        public List<PhaseObjective> Objectives = new List<PhaseObjective>();
        public bool IsEliminationPhase = false;
        [Range(0.1f, 0.9f)] public float EliminationPercentage = 0.5f;
    }
    
    
    [System.Serializable]
    public class CompetitiveAchievementConfig
    {
        public string AchievementName;
        public CompetitiveAchievementType AchievementType;
        public List<AchievementCriterion> Criteria = new List<AchievementCriterion>();
        [Range(0.1f, 10.0f)] public float ReputationReward = 2.0f;
        public bool IsLegacyAchievement = false;
        public string Description;
        public Sprite AchievementIcon;
    }
    
    
    [System.Serializable]
    public class ReputationTierConfig
    {
        public string TierName;
        public CompetitionRank Rank;
        [Range(0f, 1000f)] public float ReputationThreshold = 0f;
        [Range(0.1f, 5.0f)] public float TierMultiplier = 1.0f;
        public Color TierColor = Color.white;
        public List<string> UnlockedFeatures = new List<string>();
        public string Description;
        public Sprite TierIcon;
    }
    
    [System.Serializable]
    public class LegacyMilestoneConfig
    {
        public string MilestoneName;
        public LegacyMilestoneType MilestoneType;
        [Range(1, 100)] public int RequiredTournamentWins = 10;
        [Range(0f, 1000f)] public float RequiredReputation = 100f;
        [Range(1, 10)] public int RequiredSeasonRanking = 3;
        [Range(0.1f, 10.0f)] public float LegacyReward = 5.0f;
        public List<string> LegacyUnlocks = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class CompetitionRewardConfig
    {
        public string RewardName;
        public CompetitionResult ResultType;
        public CompetitionTier ApplicableTier;
        [Range(0.1f, 10.0f)] public float RewardValue = 1.0f;
        public List<RewardComponent> RewardComponents = new List<RewardComponent>();
        public bool IsSeasonalReward = false;
    }
    
    
    [System.Serializable]
    public class CompetitionEntryRequirement
    {
        public CompetitionCategory Category;
        public CompetitionTier Tier;
        [Range(0f, 100f)] public float MinimumSkillRating = 0f;
        [Range(0, 50)] public int MinimumExperienceLevel = 0;
        [Range(0f, 1000f)] public float MinimumReputation = 0f;
        [Range(0, 100)] public int MinimumTournamentWins = 0;
        public List<string> RequiredAchievements = new List<string>();
        public bool RequiresQualification = false;
    }
    
    [System.Serializable]
    public class CompetitorProfile
    {
        public string CompetitorID;
        public string CompetitorName;
        public float SkillRating;
        public int ExperienceLevel;
        public float Reputation;
        public int TournamentWins;
        public int ConsecutiveSeasonRanking;
        public List<string> UnlockedAchievements = new List<string>();
        public CompetitionRank CurrentRank;
    }
    
    #endregion
}
