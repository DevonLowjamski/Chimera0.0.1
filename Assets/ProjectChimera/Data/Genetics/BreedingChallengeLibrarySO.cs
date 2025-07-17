using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Breeding Challenge Library - Collection of breeding challenges for genetics gaming system
    /// Contains challenge definitions, objectives, and progression parameters
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Breeding Challenge Library", menuName = "Project Chimera/Gaming/Breeding Challenge Library")]
    public class BreedingChallengeLibrarySO : ChimeraDataSO
    {
        [Header("Challenge Collection")]
        public List<BreedingChallenge> Challenges = new List<BreedingChallenge>();
        
        [Header("Challenge Categories")]
        public List<ChallengeCategoryData> Categories = new List<ChallengeCategoryData>();
        
        [Header("Difficulty Progression")]
        public List<DifficultyTierData> DifficultyTiers = new List<DifficultyTierData>();
        
        #region Runtime Methods
        
        public BreedingChallenge GetChallenge(string challengeID)
        {
            return Challenges.Find(c => c.ChallengeID == challengeID);
        }
        
        public List<BreedingChallenge> GetChallengesByType(BreedingChallengeType challengeType)
        {
            return Challenges.FindAll(c => c.ChallengeType == challengeType);
        }
        
        public List<BreedingChallenge> GetChallengesByDifficulty(int difficultyLevel)
        {
            return Challenges.FindAll(c => c.DifficultyLevel == difficultyLevel);
        }
        
        public DifficultyTierData GetDifficultyTier(int difficultyLevel)
        {
            return DifficultyTiers.Find(d => d.DifficultyLevel == difficultyLevel);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class BreedingChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public BreedingChallengeType ChallengeType;
        public int DifficultyLevel;
        public List<BreedingObjective> Objectives = new List<BreedingObjective>();
        public List<string> RequiredParentStrains = new List<string>();
        public int MaxGenerations;
        public float TimeLimit;
        public float RewardMultiplier;
        public string Description;
        public Sprite ChallengeIcon;
        
        // Runtime properties for challenge progress
        public int AttemptsUsed = 0;
        public int MaxAttempts = 10;
        public float BestScore = 0f;
        public float FinalScore = 0f;
        public bool IsCompleted = false;
        public BreedingObjective Objective;
    }
    
    // BreedingObjective moved to BreedingDataStructures.cs to resolve namespace conflict
    
    // TargetTrait moved to BreedingDataStructures.cs to resolve namespace conflict
    
    [System.Serializable]
    public class ChallengeCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public BreedingChallengeType ChallengeType;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class DifficultyTierData
    {
        public string TierName;
        public int DifficultyLevel;
        public float DifficultyMultiplier;
        public float RewardMultiplier;
        public Color TierColor = Color.white;
        public List<string> UnlockedFeatures = new List<string>();
        public string Description;
    }
    
    public enum TraitImportance
    {
        Low,
        Medium,
        High,
        Critical
    }
}