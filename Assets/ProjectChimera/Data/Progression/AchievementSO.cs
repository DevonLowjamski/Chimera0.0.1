using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject for defining individual achievements in the progression system
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Project Chimera/Progression/Achievement")]
    public class AchievementSO : ChimeraDataSO
    {
        [Header("Achievement Identity")]
        [SerializeField] private string _achievementId;
        [SerializeField] private string _achievementName;
        [SerializeField] private string _description;
        [SerializeField] private string _iconPath;
        [SerializeField] private Color _achievementColor = Color.gold;
        
        [Header("Achievement Configuration")]
        [SerializeField] private AchievementCategory _category;
        [SerializeField] private AchievementType _type;
        [SerializeField] private bool _isHidden = false;
        [SerializeField] private bool _isRepeatable = false;
        [SerializeField] private int _sortOrder = 0;
        
        [Header("Requirements")]
        [SerializeField] private List<AchievementRequirements> _requirements = new List<AchievementRequirements>();
        [SerializeField] private int _minimumPlayerLevel = 1;
        [SerializeField] private List<string> _prerequisiteAchievements = new List<string>();
        
        [Header("Rewards")]
        [SerializeField] private List<AchievementReward> _rewards = new List<AchievementReward>();
        [SerializeField] private float _experienceReward = 100f;
        [SerializeField] private int _skillPointReward = 1;
        [SerializeField] private int _pointValue = 10; // Achievement points value
        
        [Header("Difficulty and Rarity")]
        [SerializeField] private AchievementRarity _rarity = AchievementRarity.Common;
        [SerializeField] private AchievementDifficulty _difficulty = AchievementDifficulty.Normal;
        
        [Header("Progress Tracking")]
        [SerializeField] private bool _trackProgress = true;
        [SerializeField] private float _targetValue = 1f;
        [SerializeField] private string _progressFormat = "{0}/{1}";
        
        [Header("Tiered Achievement System")]
        [SerializeField] private List<AchievementTier> _tiers = new List<AchievementTier>();
        [SerializeField] private int _currentTier = 0;
        
        [Header("Validation")]
        [SerializeField] private bool _requiresValidation = false;
        [SerializeField] private string _validationMethod;
        
        // Public Properties
        public string AchievementId => _achievementId;
        public string AchievementName => _achievementName;
        public string Description => _description;
        public string IconPath => _iconPath;
        public Color AchievementColor => _achievementColor;
        public AchievementCategory Category => _category;
        public AchievementType Type => _type;
        public bool IsHidden => _isHidden;
        public bool IsRepeatable => _isRepeatable;
        public int SortOrder => _sortOrder;
        public List<AchievementRequirements> Requirements => _requirements;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        public List<string> PrerequisiteAchievements => _prerequisiteAchievements;
        public List<AchievementReward> Rewards => _rewards;
        public float ExperienceReward => _experienceReward;
        public int SkillPointReward => _skillPointReward;
        public int PointValue => _pointValue;
        public AchievementRarity Rarity => _rarity;
        public AchievementDifficulty Difficulty => _difficulty;
        public bool TrackProgress => _trackProgress;
        public float TargetValue => _targetValue;
        public string ProgressFormat => _progressFormat;
        public bool RequiresValidation => _requiresValidation;
        public string ValidationMethod => _validationMethod;
        
        // Tiered Achievement Properties for BaseAchievementTrigger compatibility
        public int TotalTiers => _tiers?.Count ?? 1; // Total number of tiers for this achievement
        public AchievementTierRequirements NextTierRequirements => GetNextTierRequirements(); // Requirements for next tier
        
        /// <summary>
        /// Validates if the achievement requirements are met
        /// </summary>
        public bool ValidateRequirements(PlayerProgressionData playerData)
        {
            if (playerData.PlayerLevel < _minimumPlayerLevel)
                return false;
                
            foreach (var prerequisite in _prerequisiteAchievements)
            {
                if (!playerData.CompletedAchievements.Contains(prerequisite))
                    return false;
            }
            
            foreach (var requirement in _requirements)
            {
                // Validate each requirement based on its type
                // This would be implemented based on the specific requirement validation logic
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculates the current progress towards this achievement
        /// </summary>
        public float CalculateProgress(PlayerProgressionData playerData)
        {
            if (!_trackProgress)
                return 0f;
                
            // Implementation would depend on the specific achievement type and requirements
            // This is a placeholder that would be expanded based on actual achievement logic
            return 0f;
        }
        
        /// <summary>
        /// Awards the achievement rewards to the player
        /// </summary>
        public void AwardRewards(PlayerProgressionData playerData)
        {
            // Add experience
            playerData.TotalExperience += _experienceReward;
            
            // Add skill points
            playerData.AvailableSkillPoints += _skillPointReward;
            
            // Process additional rewards
            foreach (var reward in _rewards)
            {
                // Process each reward based on its type
                // This would be implemented based on the specific reward processing logic
            }
            
            // Mark achievement as completed
            if (!playerData.CompletedAchievements.Contains(_achievementId))
            {
                playerData.CompletedAchievements.Add(_achievementId);
            }
        }
        
        /// <summary>
        /// Gets the requirements for the next tier of this achievement
        /// </summary>
        public AchievementTierRequirements GetNextTierRequirements()
        {
            if (_tiers == null || _tiers.Count == 0)
            {
                // Return default requirements for non-tiered achievements
                return new AchievementTierRequirements
                {
                    NextTierLevel = 1,
                    TierIndex = 0,
                    TierName = "Complete",
                    RequiredProgress = _targetValue,
                    RequiredValue = _targetValue,
                    CurrentValue = 0f,
                    RemainingValue = _targetValue,
                    ProgressPercentage = 0f
                };
            }
            
            int nextTierIndex = _currentTier;
            if (nextTierIndex >= _tiers.Count)
            {
                // Already at max tier
                return null;
            }
            
            var nextTier = _tiers[nextTierIndex];
            return new AchievementTierRequirements
            {
                NextTierLevel = nextTier.TierLevel,
                TierIndex = nextTierIndex,
                TierName = nextTier.TierName,
                RequiredProgress = nextTier.RequiredProgress,
                RequiredValue = nextTier.RequiredProgress,
                CurrentValue = 0f, // Would be calculated based on current progress
                RemainingValue = nextTier.RequiredProgress,
                ProgressPercentage = 0f,
                RequirementDescription = nextTier.TierDescription
            };
        }
    }
} 