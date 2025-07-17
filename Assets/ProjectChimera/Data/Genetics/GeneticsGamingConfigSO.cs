using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Genetics Gaming Configuration - Configuration for breeding challenges and genetic discovery gameplay
    /// Defines breeding challenge parameters, genetic puzzle complexity, and discovery mechanics
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Genetics Gaming Config", menuName = "Project Chimera/Gaming/Genetics Gaming Config")]
    public class GeneticsGamingConfigSO : ChimeraConfigSO
    {
        [Header("Breeding Challenge Settings")]
        [Range(0.1f, 3.0f)] public float ChallengeProgressionRate = 1.0f;
        [Range(0.1f, 5.0f)] public float DiscoveryRewardMultiplier = 2.0f;
        [Range(0.1f, 1.0f)] public float PuzzleDifficultyScaling = 0.7f;
        [Range(1.0f, 10.0f)] public float InnovationDetectionThreshold = 4.0f;
        
        [Header("Challenge Configuration")]
        public List<BreedingChallengeTemplate> ChallengeTemplates = new List<BreedingChallengeTemplate>();
        public List<GeneticPuzzleConfig> PuzzleConfigurations = new List<GeneticPuzzleConfig>();
        public List<BreedingObjectiveConfig> ObjectiveConfigurations = new List<BreedingObjectiveConfig>();
        
        [Header("Discovery System Settings")]
        [Range(0.1f, 1.0f)] public float NovelTraitDetectionSensitivity = 0.8f;
        [Range(1, 10)] public int MinimumGenerationsForDiscovery = 3;
        [Range(0.1f, 5.0f)] public float GeneticInnovationReward = 3.0f;
        public bool EnableAutoDiscoveryDetection = true;
        
        [Header("Visual Interface Settings")]
        public GeneticVisualizationConfigSO VisualizationConfig;
        public bool EnableRealTimeGenetics = true;
        public bool EnablePuzzleAnimations = true;
        public bool EnableProgressVisualization = true;
        
        [Header("Progression Integration")]
        [Range(0.1f, 3.0f)] public float ExperiencePerChallenge = 1.5f;
        [Range(0.1f, 5.0f)] public float DiscoveryExperienceBonus = 2.5f;
        [Range(1, 100)] public int ChallengeCountForMilestone = 25;
        public bool EnableCrossBreedingUnlocks = true;
        
        [Header("Difficulty Scaling")]
        public AnimationCurve DifficultyProgression;
        public AnimationCurve RewardProgression;
        [Range(1, 50)] public int MaxDifficultyLevel = 20;
        [Range(0.1f, 2.0f)] public float PlayerSkillAdaptation = 1.0f;
        
        [Header("Innovation Tracking")]
        public List<GeneticInnovationType> TrackedInnovations = new List<GeneticInnovationType>();
        [Range(0.1f, 1.0f)] public float InnovationRarityThreshold = 0.3f;
        public bool EnableInnovationSharing = true;
        public bool TrackCommunityInnovations = true;
        
        [Header("Challenge Types")]
        public List<BreedingChallengeType> AvailableChallengeTypes = new List<BreedingChallengeType>();
        public List<GeneticPuzzleType> AvailablePuzzleTypes = new List<GeneticPuzzleType>();
        
        [Header("Performance Settings")]
        [Range(1, 50)] public int MaxConcurrentChallenges = 10;
        [Range(0.1f, 5.0f)] public float GeneticCalculationOptimization = 1.0f;
        public bool EnableBatchProcessing = true;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateChallengeTemplates();
            ValidateDifficultyCurves();
            ValidateInnovationSettings();
        }
        
        private void ValidateChallengeTemplates()
        {
            if (ChallengeTemplates.Count == 0)
            {
                Debug.LogWarning("No breeding challenge templates defined", this);
            }
            
            foreach (var template in ChallengeTemplates)
            {
                if (template.DifficultyLevel < 1 || template.DifficultyLevel > MaxDifficultyLevel)
                {
                    Debug.LogWarning($"Challenge template {template.ChallengeName} has invalid difficulty level", this);
                }
            }
        }
        
        private void ValidateDifficultyCurves()
        {
            if (DifficultyProgression == null || DifficultyProgression.keys.Length == 0)
            {
                Debug.LogWarning("Difficulty progression curve not configured", this);
            }
            
            if (RewardProgression == null || RewardProgression.keys.Length == 0)
            {
                Debug.LogWarning("Reward progression curve not configured", this);
            }
        }
        
        private void ValidateInnovationSettings()
        {
            if (InnovationRarityThreshold > 0.9f)
            {
                Debug.LogWarning("Innovation rarity threshold is very high - innovations may be too rare", this);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public BreedingChallengeTemplate GetChallengeTemplate(BreedingChallengeType challengeType, int difficultyLevel)
        {
            return ChallengeTemplates.Find(t => t.ChallengeType == challengeType && t.DifficultyLevel == difficultyLevel);
        }
        
        public float GetDifficultyMultiplier(int playerLevel)
        {
            if (DifficultyProgression == null) return 1.0f;
            
            var normalizedLevel = (float)playerLevel / MaxDifficultyLevel;
            return DifficultyProgression.Evaluate(normalizedLevel);
        }
        
        public float GetRewardMultiplier(int challengeLevel)
        {
            if (RewardProgression == null) return 1.0f;
            
            var normalizedLevel = (float)challengeLevel / MaxDifficultyLevel;
            return RewardProgression.Evaluate(normalizedLevel);
        }
        
        public bool IsInnovationType(GeneticInnovationType innovationType)
        {
            return TrackedInnovations.Contains(innovationType);
        }
        
        public GeneticPuzzleConfig GetPuzzleConfig(GeneticPuzzleType puzzleType)
        {
            return PuzzleConfigurations.Find(p => p.PuzzleType == puzzleType);
        }
        
        public BreedingObjectiveConfig GetObjectiveConfig(BreedingObjectiveType objectiveType)
        {
            return ObjectiveConfigurations.Find(o => o.ObjectiveType == objectiveType);
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class BreedingChallengeTemplate
    {
        public string ChallengeName;
        public BreedingChallengeType ChallengeType;
        [Range(1, 50)] public int DifficultyLevel = 1;
        [Range(0.1f, 5.0f)] public float RewardMultiplier = 1.0f;
        public List<BreedingObjectiveType> RequiredObjectives = new List<BreedingObjectiveType>();
        public List<string> RequiredParentStrains = new List<string>();
        [Range(1, 20)] public int MaxGenerations = 5;
        public string Description;
        public Sprite ChallengeIcon;
    }
    
    [System.Serializable]
    public class GeneticPuzzleConfig
    {
        public string PuzzleName;
        public GeneticPuzzleType PuzzleType;
        [Range(1, 10)] public int ComplexityLevel = 1;
        [Range(1, 50)] public int NumberOfGenes = 5;
        [Range(2, 10)] public int NumberOfAlleles = 4;
        public bool RequiresEpistasis = false;
        public bool RequiresLinkage = false;
        [Range(0.1f, 3.0f)] public float TimeLimit = 2.0f;
        public List<GeneticConstraint> Constraints = new List<GeneticConstraint>();
    }
    
    [System.Serializable]
    public class BreedingObjectiveConfig
    {
        public string ObjectiveName;
        public BreedingObjectiveType ObjectiveType;
        public List<TargetTrait> TargetTraits = new List<TargetTrait>();
        [Range(0.1f, 1.0f)] public float RequiredAccuracy = 0.8f;
        [Range(0.1f, 5.0f)] public float CompletionReward = 1.5f;
        public bool IsOptional = false;
        public string Description;
    }
    
    
    [System.Serializable]
    public class GeneticConstraint
    {
        public string ConstraintName;
        public GeneticConstraintType ConstraintType;
        public List<string> AffectedGenes = new List<string>();
        public float ConstraintValue;
        public string Description;
    }
    
    [System.Serializable]
    public class GeneticInnovationType
    {
        public string InnovationName;
        public InnovationCategory Category;
        [Range(0.01f, 1.0f)] public float RarityThreshold = 0.1f;
        [Range(0.1f, 10.0f)] public float InnovationValue = 2.0f;
        public bool RequiresCommunityValidation = false;
        public List<string> RequiredTraitCombinations = new List<string>();
    }
    
    #endregion
}