using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Aromatic Gaming Configuration - Configuration for sensory training and creative blending gameplay
    /// Defines sensory challenge parameters, blending mechanics, and aromatic discovery systems
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Aromatic Gaming Config", menuName = "Project Chimera/Gaming/Aromatic Gaming Config")]
    public class AromaticGamingConfigSO : ChimeraConfigSO
    {
        [Header("Sensory Training Settings")]
        [Range(0.1f, 3.0f)] public float SensorySkillProgressionRate = 1.0f;
        [Range(0.1f, 5.0f)] public float CreativityRewardMultiplier = 2.0f;
        [Range(0.1f, 1.0f)] public float SensoryChallengeDifficulty = 0.7f;
        [Range(1.0f, 10.0f)] public float InnovationThreshold = 4.0f;
        [Range(0.1f, 2.0f)] public float BlendingPrecisionRequired = 0.8f;
        
        [Header("Sensory Challenge Configuration")]
        public List<SensoryTrainingTemplate> TrainingTemplates = new List<SensoryTrainingTemplate>();
        public List<AromaticIdentificationChallenge> IdentificationChallenges = new List<AromaticIdentificationChallenge>();
        public List<SensoryMemoryExercise> MemoryExercises = new List<SensoryMemoryExercise>();
        
        [Header("Creative Blending Settings")]
        [Range(2, 20)] public int MaxTerpenesPerBlend = 10;
        [Range(0.01f, 1.0f)] public float MinimumTerpeneConcentration = 0.05f;
        [Range(0.1f, 5.0f)] public float BlendingCreativityBonus = 1.8f;
        public bool EnableRealTimeBlending = true;
        public bool EnableBlendPreview = true;
        
        [Header("Terpene Analysis Settings")]
        public TerpeneLibrarySO TerpeneLibrary;
        public AromaticProfileDatabaseSO ProfileDatabase;
        [Range(0.1f, 1.0f)] public float AnalysisAccuracyThreshold = 0.85f;
        [Range(1, 100)] public int MaxAnalysisComplexity = 50;
        
        [Header("Innovation Detection")]
        [Range(0.1f, 1.0f)] public float NovelBlendDetectionSensitivity = 0.7f;
        [Range(0.1f, 1.0f)] public float SynergyDetectionThreshold = 0.6f;
        public bool EnableInnovationSharing = true;
        public bool TrackCommunityInnovations = true;
        
        [Header("Difficulty Progression")]
        public AnimationCurve SensoryDifficultyProgression;
        public AnimationCurve BlendingComplexityProgression;
        public AnimationCurve RewardMultiplierProgression;
        [Range(1, 50)] public int MaxSensoryLevel = 25;
        [Range(0.1f, 2.0f)] public float PlayerAdaptationRate = 1.0f;
        
        [Header("Aromatic Categories")]
        public List<TerpeneCategoryConfig> TerpeneCategories = new List<TerpeneCategoryConfig>();
        public List<FlavorProfileConfig> FlavorProfiles = new List<FlavorProfileConfig>();
        public List<AromaticSynergyRule> SynergyRules = new List<AromaticSynergyRule>();
        
        [Header("Sensory Training Types")]
        public List<SensoryTrainingType> AvailableTrainingTypes = new List<SensoryTrainingType>();
        public List<BlendingChallengeType> AvailableBlendingChallenges = new List<BlendingChallengeType>();
        
        [Header("UI and Visualization")]
        public AromaticVisualizationConfigSO VisualizationConfig;
        public bool EnableAromaticWheel = true;
        public bool EnableRealTimeFeedback = true;
        public bool EnableProgressVisualization = true;
        
        [Header("Performance Settings")]
        [Range(1, 20)] public int MaxConcurrentSensoryChallenges = 5;
        [Range(1, 50)] public int MaxConcurrentBlendingProjects = 15;
        [Range(0.1f, 5.0f)] public float CalculationOptimization = 1.0f;
        public bool EnableBatchBlending = true;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateTrainingTemplates();
            ValidateTerpeneSettings();
            ValidateProgressionCurves();
            ValidateSynergyRules();
        }
        
        private void ValidateTrainingTemplates()
        {
            if (TrainingTemplates.Count == 0)
            {
                Debug.LogWarning("No sensory training templates defined", this);
            }
            
            foreach (var template in TrainingTemplates)
            {
                if (template.DifficultyLevel < 1 || template.DifficultyLevel > MaxSensoryLevel)
                {
                    Debug.LogWarning($"Training template {template.TemplateName} has invalid difficulty level", this);
                }
            }
        }
        
        private void ValidateTerpeneSettings()
        {
            if (TerpeneLibrary == null)
            {
                Debug.LogWarning("TerpeneLibrary is not assigned", this);
            }
            
            if (ProfileDatabase == null)
            {
                Debug.LogWarning("ProfileDatabase is not assigned", this);
            }
            
            if (MinimumTerpeneConcentration >= 1.0f)
            {
                Debug.LogError("MinimumTerpeneConcentration must be less than 1.0", this);
            }
        }
        
        private void ValidateProgressionCurves()
        {
            if (SensoryDifficultyProgression == null || SensoryDifficultyProgression.keys.Length == 0)
            {
                Debug.LogWarning("Sensory difficulty progression curve not configured", this);
            }
            
            if (BlendingComplexityProgression == null || BlendingComplexityProgression.keys.Length == 0)
            {
                Debug.LogWarning("Blending complexity progression curve not configured", this);
            }
        }
        
        private void ValidateSynergyRules()
        {
            foreach (var rule in SynergyRules)
            {
                if (rule.RequiredTerpenes.Count < 2)
                {
                    Debug.LogWarning($"Synergy rule {rule.SynergyName} requires at least 2 terpenes", this);
                }
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public SensoryTrainingTemplate GetTrainingTemplate(SensoryTrainingType trainingType, SensoryDifficulty difficulty)
        {
            return TrainingTemplates.Find(t => t.TrainingType == trainingType && t.Difficulty == difficulty);
        }
        
        public float GetSensoryDifficultyMultiplier(int playerLevel)
        {
            if (SensoryDifficultyProgression == null) return 1.0f;
            
            var normalizedLevel = (float)playerLevel / MaxSensoryLevel;
            return SensoryDifficultyProgression.Evaluate(normalizedLevel);
        }
        
        public float GetBlendingComplexity(int blendingLevel)
        {
            if (BlendingComplexityProgression == null) return 1.0f;
            
            var normalizedLevel = (float)blendingLevel / MaxSensoryLevel;
            return BlendingComplexityProgression.Evaluate(normalizedLevel);
        }
        
        public float GetRewardMultiplier(int achievementLevel)
        {
            if (RewardMultiplierProgression == null) return 1.0f;
            
            var normalizedLevel = (float)achievementLevel / MaxSensoryLevel;
            return RewardMultiplierProgression.Evaluate(normalizedLevel);
        }
        
        public TerpeneCategoryConfig GetTerpeneCategory(TerpeneCategory category)
        {
            return TerpeneCategories.Find(c => c.Category == category);
        }
        
        public List<AromaticSynergyRule> GetApplicableSynergyRules(List<string> terpeneNames)
        {
            return SynergyRules.FindAll(rule => 
                rule.RequiredTerpenes.TrueForAll(required => terpeneNames.Contains(required)));
        }
        
        public FlavorProfileConfig GetFlavorProfile(FlavorProfileType profileType)
        {
            return FlavorProfiles.Find(p => p.ProfileType == profileType);
        }
        
        public bool IsValidBlendConcentration(float concentration)
        {
            return concentration >= MinimumTerpeneConcentration && concentration <= 1.0f;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class SensoryTrainingTemplate
    {
        public string TemplateName;
        public SensoryTrainingType TrainingType;
        public SensoryDifficulty Difficulty;
        [Range(1, 50)] public int DifficultyLevel = 1;
        [Range(0.1f, 5.0f)] public float RewardMultiplier = 1.0f;
        [Range(5, 300)] public int TimeLimit = 60;
        public List<TerpeneCategory> FocusCategories = new List<TerpeneCategory>();
        public List<string> RequiredTerpenes = new List<string>();
        public string Description;
        public Sprite TrainingIcon;
    }
    
    [System.Serializable]
    public class AromaticIdentificationChallenge
    {
        public string ChallengeName;
        public IdentificationChallengeType ChallengeType;
        [Range(1, 20)] public int NumberOfSamples = 5;
        [Range(0.1f, 1.0f)] public float RequiredAccuracy = 0.8f;
        public List<TerpeneCategory> ChallengeCategories = new List<TerpeneCategory>();
        public bool IncludeDistractors = true;
        [Range(1, 10)] public int NumberOfDistractors = 3;
        [Range(0.1f, 3.0f)] public float CompletionReward = 1.5f;
    }
    
    [System.Serializable]
    public class SensoryMemoryExercise
    {
        public string ExerciseName;
        public MemoryExerciseType ExerciseType;
        [Range(2, 20)] public int SequenceLength = 5;
        [Range(1, 60)] public int MemoryTime = 15;
        [Range(0.1f, 1.0f)] public float RequiredAccuracy = 0.7f;
        public List<SensoryModality> TestedModalities = new List<SensoryModality>();
        [Range(0.1f, 3.0f)] public float MemoryReward = 1.8f;
    }
    
    [System.Serializable]
    public class TerpeneCategoryConfig
    {
        public string CategoryName;
        public TerpeneCategory Category;
        public Color CategoryColor = Color.white;
        public List<string> RepresentativeTerpenes = new List<string>();
        [Range(0.1f, 2.0f)] public float DifficultyModifier = 1.0f;
        [Range(0.1f, 3.0f)] public float MasteryReward = 1.5f;
        public string Description;
        public Sprite CategoryIcon;
    }
    
    [System.Serializable]
    public class FlavorProfileConfig
    {
        public string ProfileName;
        public FlavorProfileType ProfileType;
        public List<TerpeneComponent> TerpeneComponents = new List<TerpeneComponent>();
        [Range(0.1f, 2.0f)] public float ComplexityFactor = 1.0f;
        [Range(0.1f, 5.0f)] public float InnovationPotential = 1.0f;
        public string FlavorDescription;
        public List<string> CommonCombinations = new List<string>();
    }
    
    [System.Serializable]
    public class TerpeneComponent
    {
        public string TerpeneName;
        [Range(0.01f, 1.0f)] public float MinConcentration = 0.1f;
        [Range(0.01f, 1.0f)] public float MaxConcentration = 0.8f;
        [Range(0.01f, 1.0f)] public float OptimalConcentration = 0.3f;
        public TerpeneRole Role = TerpeneRole.Primary;
        public bool IsEssential = false;
    }
    
    [System.Serializable]
    public class AromaticSynergyRule
    {
        public string SynergyName;
        public AromaticSynergyType SynergyType;
        public List<string> RequiredTerpenes = new List<string>();
        [Range(0.1f, 5.0f)] public float SynergyMultiplier = 1.5f;
        [Range(0.01f, 1.0f)] public float ActivationThreshold = 0.3f;
        public string EffectDescription;
        public bool IsRareInteraction = false;
    }
    
    #endregion
}