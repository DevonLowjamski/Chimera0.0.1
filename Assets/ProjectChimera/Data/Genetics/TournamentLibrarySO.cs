using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Tournament Library - Collection of tournaments for scientific competition system
    /// Contains tournament definitions, formats, and competitive structures
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Tournament Library", menuName = "Project Chimera/Gaming/Tournament Library")]
    public class TournamentLibrarySO : ChimeraDataSO
    {
        [Header("Tournament Collection")]
        public List<Tournament> Tournaments = new List<Tournament>();
        
        [Header("Tournament Categories")]
        public List<TournamentCategoryData> Categories = new List<TournamentCategoryData>();
        
        [Header("Tournament Formats")]
        public List<TournamentFormatData> Formats = new List<TournamentFormatData>();
        
        #region Runtime Methods
        
        public Tournament GetTournament(string tournamentID)
        {
            return Tournaments.Find(t => t.TournamentID == tournamentID);
        }
        
        public List<Tournament> GetTournamentsByCategory(CompetitionCategory category)
        {
            return Tournaments.FindAll(t => t.Category == category);
        }
        
        public List<Tournament> GetTournamentsByTier(CompetitionTier tier)
        {
            return Tournaments.FindAll(t => t.Tier == tier);
        }
        
        public TournamentFormatData GetTournamentFormat(TournamentFormat format)
        {
            return Formats.Find(f => f.Format == format);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class Tournament
    {
        public string TournamentID;
        public string TournamentName;
        public CompetitionCategory Category;
        public CompetitionTier Tier;
        public TournamentFormat Format;
        public int MinParticipants;
        public int MaxParticipants;
        public float DurationHours;
        public List<TournamentPhase> Phases = new List<TournamentPhase>();
        public List<TournamentReward> Rewards = new List<TournamentReward>();
        public string Description;
        public Sprite TournamentIcon;
    }
    
    [System.Serializable]
    public class TournamentCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public CompetitionCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class TournamentFormatData
    {
        public string FormatName;
        public TournamentFormat Format;
        public string FormatDescription;
        public List<FormatRule> Rules = new List<FormatRule>();
        public bool AllowsTeams;
        public int MaxRounds;
        public Sprite FormatIcon;
    }
    
    [System.Serializable]
    public class TournamentPhase
    {
        public string PhaseID;
        public string PhaseName;
        public CompetitionPhaseType PhaseType;
        public float DurationHours;
        public float DifficultyMultiplier;
        public List<PhaseObjective> Objectives = new List<PhaseObjective>();
        public bool IsEliminationPhase;
        public float EliminationPercentage;
    }
    
    [System.Serializable]
    public class PhaseObjective
    {
        public string ObjectiveName;
        public PhaseObjectiveType ObjectiveType;
        public float TargetValue;
        public float ScoreWeight;
        public bool IsOptional;
    }
    
    // TournamentReward moved to TournamentDataStructures.cs to resolve namespace conflict
    
    [System.Serializable]
    public class FormatRule
    {
        public string RuleName;
        public TournamentRuleType RuleType;
        public string RuleDescription;
        public bool IsEnforced;
    }
    
    public enum TournamentRewardType
    {
        Experience,
        Reputation,
        Title,
        Item,
        Feature,
        Currency,
        Achievement,
        Recognition
    }
    
    public enum TournamentRuleType
    {
        ParticipantLimit,
        TimeConstraint,
        PerformanceStandard,
        EligibilityRequirement,
        ScoringMethod,
        EliminationCriteria,
        AdvancementRule,
        ConductRule
    }
}