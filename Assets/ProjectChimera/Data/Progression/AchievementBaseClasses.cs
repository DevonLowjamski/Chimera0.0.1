using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;

// Resolve RewardType namespace conflicts
using ProgressionRewardType = ProjectChimera.Data.Progression.RewardType;
using EventRewardType = ProjectChimera.Data.Events.RewardType;

// Force recompilation to recognize aliases

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// Base implementation for achievement trigger systems.
    /// Provides common functionality for all trigger types while allowing
    /// specialized implementations for cultivation, environmental, facility,
    /// economic, and educational achievement detection.
    /// </summary>
    [Serializable]
    public abstract class BaseAchievementTrigger : IAchievementTrigger
    {
        [SerializeField] protected string _triggerId;
        [SerializeField] protected AchievementCategory _category;
        [SerializeField] protected AchievementTriggerType _triggerType;
        [SerializeField] protected int _priority;
        [SerializeField] protected List<string> _targetAchievements = new List<string>();
        
        public string TriggerId => _triggerId;
        public AchievementCategory Category => _category;
        public AchievementTriggerType TriggerType => _triggerType;
        
        protected BaseAchievementTrigger(string triggerId, AchievementCategory category, AchievementTriggerType triggerType, int priority = 0)
        {
            _triggerId = triggerId;
            _category = category;
            _triggerType = triggerType;
            _priority = priority;
        }
        
        public virtual List<string> ProcessTrigger(AchievementContext context, PlayerAchievementProfile playerProfile)
        {
            var validAchievements = new List<string>();
            
            foreach (var achievementId in _targetAchievements)
            {
                if (ShouldTriggerAchievement(achievementId, context, playerProfile))
                {
                    validAchievements.Add(achievementId);
                }
            }
            
            return validAchievements;
        }
        
        public abstract bool CanProcessTrigger(object triggerData);
        
        public virtual int GetPriority() => _priority;
        
        protected abstract bool ShouldTriggerAchievement(string achievementId, AchievementContext context, PlayerAchievementProfile playerProfile);
        
        protected virtual bool ValidateBaseConditions(AchievementContext context, PlayerAchievementProfile playerProfile)
        {
            // Basic validation logic that can be overridden
            return context != null && playerProfile != null;
        }
    }
    
    /// <summary>
    /// Base implementation for achievement progress tracking.
    /// Handles common progress calculation patterns while allowing
    /// specialized implementations for different achievement types.
    /// </summary>
    [Serializable]
    public abstract class BaseAchievementProgress : IAchievementProgress
    {
        [SerializeField] protected string _achievementId;
        [SerializeField] protected float _currentProgress;
        [SerializeField] protected float _requiredValue;
        [SerializeField] protected ProgressCalculationType _calculationType;
        [SerializeField] protected DateTime _lastUpdate;
        [SerializeField] protected bool _isComplete;
        
        public string AchievementId => _achievementId;
        public float CurrentProgress 
        { 
            get => _currentProgress; 
            set 
            {
                _currentProgress = Mathf.Clamp01(value);
                _lastUpdate = DateTime.Now;
                _isComplete = _currentProgress >= 1f;
            }
        }
        
        protected BaseAchievementProgress(string achievementId, float requiredValue, ProgressCalculationType calculationType = ProgressCalculationType.Linear)
        {
            _achievementId = achievementId;
            _requiredValue = requiredValue;
            _calculationType = calculationType;
            _currentProgress = 0f;
            _lastUpdate = DateTime.Now;
            _isComplete = false;
        }
        
        public virtual float CalculateProgress(object progressData, AchievementContext context)
        {
            try
            {
                var rawProgress = ExtractProgressValue(progressData, context);
                var calculatedProgress = ApplyCalculationType(rawProgress);
                
                CurrentProgress = calculatedProgress;
                return CurrentProgress;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error calculating progress for achievement {_achievementId}: {ex.Message}");
                return CurrentProgress; // Return existing progress on error
            }
        }
        
        public virtual bool IsComplete() => _isComplete;
        
        public virtual void ResetProgress()
        {
            _currentProgress = 0f;
            _lastUpdate = DateTime.Now;
            _isComplete = false;
        }
        
        public virtual AchievementProgressInfo GetProgressInfo()
        {
            return new AchievementProgressInfo
            {
                AchievementId = _achievementId,
                CurrentProgress = _currentProgress,
                RequiredValue = _requiredValue,
                CalculationType = _calculationType,
                LastUpdate = _lastUpdate,
                IsComplete = _isComplete,
                DisplayText = GenerateDisplayText(),
                ProgressPercentage = _currentProgress * 100f
            };
        }
        
        protected abstract float ExtractProgressValue(object progressData, AchievementContext context);
        
        protected virtual float ApplyCalculationType(float rawProgress)
        {
            return _calculationType switch
            {
                ProgressCalculationType.Linear => rawProgress / _requiredValue,
                ProgressCalculationType.Logarithmic => Mathf.Log(rawProgress + 1) / Mathf.Log(_requiredValue + 1),
                ProgressCalculationType.Exponential => Mathf.Pow(rawProgress / _requiredValue, 0.5f),
                ProgressCalculationType.Threshold => rawProgress >= _requiredValue ? 1f : 0f,
                _ => rawProgress / _requiredValue
            };
        }
        
        protected virtual string GenerateDisplayText()
        {
            return _calculationType switch
            {
                ProgressCalculationType.Threshold => _isComplete ? "Complete" : "In Progress",
                _ => $"{(_currentProgress * _requiredValue):F0} / {_requiredValue:F0}"
            };
        }
    }
    
    /// <summary>
    /// Base implementation for milestone tracking.
    /// Provides framework for tracking multi-achievement milestones
    /// with complex dependency and synergy systems.
    /// </summary>
    [Serializable]
    public abstract class BaseMilestoneTracker : IMilestoneTracker
    {
        [SerializeField] protected string _milestoneId;
        [SerializeField] protected List<MilestoneRequirement> _requirements = new List<MilestoneRequirement>();
        [SerializeField] protected MilestoneCalculationType _calculationType;
        [SerializeField] protected float _currentProgress;
        [SerializeField] protected bool _isComplete;
        
        public string MilestoneId => _milestoneId;
        
        protected BaseMilestoneTracker(string milestoneId, List<MilestoneRequirement> requirements, MilestoneCalculationType calculationType = MilestoneCalculationType.AllRequired)
        {
            _milestoneId = milestoneId;
            _requirements = requirements ?? new List<MilestoneRequirement>();
            _calculationType = calculationType;
            _currentProgress = 0f;
            _isComplete = false;
        }
        
        public virtual float UpdateMilestoneProgress(UnlockedAchievement unlockedAchievement, PlayerAchievementProfile playerProfile)
        {
            try
            {
                var previousProgress = _currentProgress;
                _currentProgress = CalculateMilestoneProgress(playerProfile);
                
                var progressDelta = _currentProgress - previousProgress;
                if (progressDelta > 0)
                {
                    OnProgressUpdated(progressDelta, unlockedAchievement, playerProfile);
                }
                
                if (!_isComplete && _currentProgress >= 1f)
                {
                    _isComplete = true;
                    OnMilestoneCompleted(playerProfile);
                }
                
                return _currentProgress;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating milestone progress for {_milestoneId}: {ex.Message}");
                return _currentProgress;
            }
        }
        
        public virtual bool IsMilestoneComplete(PlayerAchievementProfile playerProfile)
        {
            return _isComplete || CalculateMilestoneProgress(playerProfile) >= 1f;
        }
        
        public virtual List<MilestoneRequirement> GetRemainingRequirements(PlayerAchievementProfile playerProfile)
        {
            var remainingRequirements = new List<MilestoneRequirement>();
            
            foreach (var requirement in _requirements)
            {
                if (!IsRequirementMet(requirement, playerProfile))
                {
                    remainingRequirements.Add(requirement);
                }
            }
            
            return remainingRequirements;
        }
        
        protected virtual float CalculateMilestoneProgress(PlayerAchievementProfile playerProfile)
        {
            var metRequirements = _requirements.Count(req => IsRequirementMet(req, playerProfile));
            
            return _calculationType switch
            {
                MilestoneCalculationType.AllRequired => metRequirements == _requirements.Count ? 1f : (float)metRequirements / _requirements.Count,
                MilestoneCalculationType.AnyRequired => metRequirements > 0 ? 1f : 0f,
                MilestoneCalculationType.Percentage => (float)metRequirements / _requirements.Count,
                MilestoneCalculationType.Weighted => CalculateWeightedProgress(playerProfile),
                _ => (float)metRequirements / _requirements.Count
            };
        }
        
        protected virtual bool IsRequirementMet(MilestoneRequirement requirement, PlayerAchievementProfile playerProfile)
        {
            return requirement.RequirementType switch
            {
                MilestoneRequirementType.Achievement => playerProfile.UnlockedAchievements.Contains(requirement.TargetId),
                MilestoneRequirementType.Category => GetCategoryCount(requirement.CategoryTarget, playerProfile) >= requirement.RequiredCount,
                MilestoneRequirementType.Points => playerProfile.AchievementPoints >= requirement.RequiredCount,
                MilestoneRequirementType.Level => playerProfile.Level >= requirement.RequiredCount,
                MilestoneRequirementType.Custom => EvaluateCustomRequirement(requirement, playerProfile),
                _ => false
            };
        }
        
        protected virtual int GetCategoryCount(AchievementCategory category, PlayerAchievementProfile playerProfile)
        {
            return playerProfile.CategoryProgress.GetValueOrDefault(category, 0);
        }
        
        protected virtual float CalculateWeightedProgress(PlayerAchievementProfile playerProfile)
        {
            var totalWeight = _requirements.Sum(req => req.Weight);
            var achievedWeight = _requirements.Where(req => IsRequirementMet(req, playerProfile)).Sum(req => req.Weight);
            
            return totalWeight > 0 ? achievedWeight / totalWeight : 0f;
        }
        
        protected abstract bool EvaluateCustomRequirement(MilestoneRequirement requirement, PlayerAchievementProfile playerProfile);
        
        protected virtual void OnProgressUpdated(float progressDelta, UnlockedAchievement unlockedAchievement, PlayerAchievementProfile playerProfile)
        {
            // Override in derived classes for custom progress update logic
        }
        
        protected virtual void OnMilestoneCompleted(PlayerAchievementProfile playerProfile)
        {
            // Override in derived classes for custom completion logic
        }
    }
    
    /// <summary>
    /// Base implementation for reward calculation systems.
    /// Provides standardized reward calculation with scaling,
    /// multipliers, and bonus systems.
    /// </summary>
    [Serializable]
    public abstract class BaseRewardCalculator : IRewardCalculator
    {
        [SerializeField] protected RewardCalculationConfig _config;
        [SerializeField] protected Dictionary<AchievementRarity, float> _rarityMultipliers = new Dictionary<AchievementRarity, float>();
        [SerializeField] protected Dictionary<AchievementDifficulty, float> _difficultyMultipliers = new Dictionary<AchievementDifficulty, float>();
        
        protected BaseRewardCalculator(RewardCalculationConfig config)
        {
            _config = config;
            InitializeDefaultMultipliers();
        }
        
        public virtual List<AchievementReward> CalculateAchievementRewards(AchievementSO achievement, AchievementContext context, PlayerAchievementProfile playerProfile)
        {
            try
            {
                var baseRewards = GetBaseAchievementRewards(achievement);
                var scaledRewards = ApplyScaling(baseRewards, achievement, context, playerProfile);
                var multipliedRewards = ApplyRewardMultipliers(scaledRewards, playerProfile);
                var bonusRewards = CalculateBonusRewards(achievement, context, playerProfile);
                
                var finalRewards = new List<AchievementReward>();
                finalRewards.AddRange(multipliedRewards);
                finalRewards.AddRange(bonusRewards);
                
                return ValidateAndClampRewards(finalRewards);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error calculating achievement rewards for {achievement.AchievementId}: {ex.Message}");
                return new List<AchievementReward>();
            }
        }
        
        public virtual List<AchievementReward> CalculateMilestoneRewards(MilestoneSO milestone, MilestoneContext context, PlayerAchievementProfile playerProfile)
        {
            try
            {
                var baseRewards = GetBaseMilestoneRewards(milestone);
                var scaledRewards = ApplyMilestoneScaling(baseRewards, milestone, context, playerProfile);
                var multipliedRewards = ApplyRewardMultipliers(scaledRewards, playerProfile);
                
                return ValidateAndClampRewards(multipliedRewards);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error calculating milestone rewards for {milestone.MilestoneId}: {ex.Message}");
                return new List<AchievementReward>();
            }
        }
        
        public virtual List<AchievementReward> ApplyRewardMultipliers(List<AchievementReward> baseRewards, PlayerAchievementProfile playerProfile)
        {
            var multipliedRewards = new List<AchievementReward>();
            
            foreach (var reward in baseRewards)
            {
                var multipliedReward = new AchievementReward(reward);
                
                // Apply player level multiplier
                var levelMultiplier = CalculateLevelMultiplier(playerProfile.Level);
                multipliedReward.Amount = (int)(multipliedReward.Amount * levelMultiplier);
                
                // Apply achievement streak multiplier
                var streakMultiplier = CalculateStreakMultiplier(playerProfile);
                multipliedReward.Amount = (int)(multipliedReward.Amount * streakMultiplier);
                
                // Apply premium status multiplier
                var premiumMultiplier = CalculatePremiumMultiplier(playerProfile);
                multipliedReward.Amount = (int)(multipliedReward.Amount * premiumMultiplier);
                
                multipliedRewards.Add(multipliedReward);
            }
            
            return multipliedRewards;
        }
        
        protected virtual List<AchievementReward> GetBaseAchievementRewards(AchievementSO achievement)
        {
            var rewards = new List<AchievementReward>();
            
            if (achievement.Rewards != null)
            {
                rewards.AddRange(achievement.Rewards);
            }
            
            // Add default rewards based on rarity and difficulty
            if (achievement.ExperienceReward > 0)
            {
                rewards.Add(new AchievementReward
                {
                    Type = RewardType.Experience,
                    Amount = achievement.ExperienceReward,
                    Description = $"{achievement.ExperienceReward} XP"
                });
            }
            
            if (achievement.PointValue > 0)
            {
                rewards.Add(new AchievementReward
                {
                    Type = RewardType.AchievementPoints,
                    Amount = achievement.PointValue,
                    Description = $"{achievement.PointValue} Achievement Points"
                });
            }
            
            return rewards;
        }
        
        protected virtual List<AchievementReward> GetBaseMilestoneRewards(MilestoneSO milestone)
        {
            var rewards = new List<AchievementReward>();
            
            if (milestone.Rewards != null)
            {
                rewards.AddRange(milestone.Rewards);
            }
            
            return rewards;
        }
        
        protected virtual List<AchievementReward> ApplyScaling(List<AchievementReward> baseRewards, AchievementSO achievement, AchievementContext context, PlayerAchievementProfile playerProfile)
        {
            var scaledRewards = new List<AchievementReward>();
            
            foreach (var reward in baseRewards)
            {
                var scaledReward = new AchievementReward(reward);
                
                // Apply rarity scaling
                if (_rarityMultipliers.TryGetValue(achievement.Rarity, out var rarityMultiplier))
                {
                    scaledReward.Amount = (int)(scaledReward.Amount * rarityMultiplier);
                }
                
                // Apply difficulty scaling
                if (_difficultyMultipliers.TryGetValue(achievement.Difficulty, out var difficultyMultiplier))
                {
                    scaledReward.Amount = (int)(scaledReward.Amount * difficultyMultiplier);
                }
                
                scaledRewards.Add(scaledReward);
            }
            
            return scaledRewards;
        }
        
        protected virtual List<AchievementReward> ApplyMilestoneScaling(List<AchievementReward> baseRewards, MilestoneSO milestone, MilestoneContext context, PlayerAchievementProfile playerProfile)
        {
            // Milestones typically have higher reward multipliers
            var scaledRewards = new List<AchievementReward>();
            var milestoneMultiplier = _config.MilestoneRewardMultiplier;
            
            foreach (var reward in baseRewards)
            {
                var scaledReward = new AchievementReward(reward);
                scaledReward.Amount = (int)(scaledReward.Amount * milestoneMultiplier);
                scaledRewards.Add(scaledReward);
            }
            
            return scaledRewards;
        }
        
        protected abstract List<AchievementReward> CalculateBonusRewards(AchievementSO achievement, AchievementContext context, PlayerAchievementProfile playerProfile);
        
        protected virtual float CalculateLevelMultiplier(int playerLevel)
        {
            return 1f + (playerLevel - 1) * _config.LevelMultiplierIncrement;
        }
        
        protected virtual float CalculateStreakMultiplier(PlayerAchievementProfile playerProfile)
        {
            // Calculate based on recent achievement unlocks
            var recentAchievements = GetRecentAchievementCount(playerProfile, TimeSpan.FromDays(7));
            return 1f + (recentAchievements * _config.StreakMultiplierIncrement);
        }
        
        protected virtual float CalculatePremiumMultiplier(PlayerAchievementProfile playerProfile)
        {
            return playerProfile.HasPremiumStatus ? _config.PremiumStatusMultiplier : 1f;
        }
        
        protected virtual int GetRecentAchievementCount(PlayerAchievementProfile playerProfile, TimeSpan timespan)
        {
            // This would typically be implemented with actual achievement unlock timestamps
            return 0; // Placeholder implementation
        }
        
        protected virtual List<AchievementReward> ValidateAndClampRewards(List<AchievementReward> rewards)
        {
            var validatedRewards = new List<AchievementReward>();
            
            foreach (var reward in rewards)
            {
                if (reward.Amount > 0)
                {
                    // Apply maximum limits
                    var maxAmount = GetMaxRewardAmount(reward.Type);
                    reward.Amount = Mathf.Min(reward.Amount, maxAmount);
                    
                    validatedRewards.Add(reward);
                }
            }
            
            return validatedRewards;
        }
        
        protected virtual int GetMaxRewardAmount(RewardType rewardType)
        {
            return rewardType switch
            {
                RewardType.Experience => _config.MaxExperienceReward,
                RewardType.AchievementPoints => _config.MaxAchievementPointsReward,
                RewardType.Currency => _config.MaxCurrencyReward,
                RewardType.Items => _config.MaxItemReward,
                _ => int.MaxValue
            };
        }
        
        private void InitializeDefaultMultipliers()
        {
            // Initialize default rarity multipliers
            _rarityMultipliers[AchievementRarity.Common] = 1f;
            _rarityMultipliers[AchievementRarity.Uncommon] = 1.25f;
            _rarityMultipliers[AchievementRarity.Rare] = 1.5f;
            _rarityMultipliers[AchievementRarity.Epic] = 2f;
            _rarityMultipliers[AchievementRarity.Legendary] = 3f;
            _rarityMultipliers[AchievementRarity.Mythic] = 5f;
            
            // Initialize default difficulty multipliers
            _difficultyMultipliers[AchievementDifficulty.Trivial] = 0.5f;
            _difficultyMultipliers[AchievementDifficulty.Easy] = 0.75f;
            _difficultyMultipliers[AchievementDifficulty.Medium] = 1f;
            _difficultyMultipliers[AchievementDifficulty.Hard] = 1.5f;
            _difficultyMultipliers[AchievementDifficulty.Expert] = 2f;
            _difficultyMultipliers[AchievementDifficulty.Master] = 3f;
            _difficultyMultipliers[AchievementDifficulty.Grandmaster] = 5f;
        }
    }
    
    /// <summary>
    /// Base implementation for progressive achievements.
    /// Handles multi-tier achievement systems with escalating requirements
    /// and rewards.
    /// </summary>
    [Serializable]
    public abstract class BaseProgressiveAchievement : BaseAchievementProgress, IProgressiveAchievement
    {
        [SerializeField] protected List<AchievementTier> _tiers;
        [SerializeField] protected int _currentTier;
        [SerializeField] protected float _tierProgress;
        
        public List<AchievementTier> Tiers => _tiers;
        public int CurrentTier 
        { 
            get => _currentTier; 
            set => _currentTier = Mathf.Clamp(value, 0, _tiers.Count - 1); 
        }
        
        protected BaseProgressiveAchievement(string achievementId, List<AchievementTier> tiers) 
            : base(achievementId, tiers?.LastOrDefault()?.RequiredValue ?? 1f)
        {
            _tiers = tiers ?? new List<AchievementTier>();
            _currentTier = 0;
            _tierProgress = 0f;
        }
        
        public virtual bool CheckTierUnlock(object progressData)
        {
            if (_currentTier >= _tiers.Count - 1) return false;
            
            var nextTier = _tiers[_currentTier + 1];
            var currentValue = ExtractProgressValue(progressData, null);
            
            return currentValue >= nextTier.RequiredValue;
        }
        
        public virtual AchievementTierRequirements GetNextTierRequirements()
        {
            if (_currentTier >= _tiers.Count - 1) return null;
            
            var nextTier = _tiers[_currentTier + 1];
            var currentValue = GetCurrentValue();
            
            return new AchievementTierRequirements
            {
                TierIndex = _currentTier + 1,
                TierName = nextTier.TierName,
                RequiredValue = nextTier.RequiredValue,
                CurrentValue = currentValue,
                RemainingValue = nextTier.RequiredValue - currentValue,
                ProgressPercentage = (currentValue / nextTier.RequiredValue) * 100f
            };
        }
        
        public virtual AchievementTier UnlockNextTier()
        {
            if (_currentTier >= _tiers.Count - 1) return null;
            
            _currentTier++;
            var unlockedTier = _tiers[_currentTier];
            
            OnTierUnlocked(unlockedTier);
            
            return unlockedTier;
        }
        
        public override float CalculateProgress(object progressData, AchievementContext context)
        {
            var currentValue = ExtractProgressValue(progressData, context);
            
            // Check for tier unlocks
            while (_currentTier < _tiers.Count - 1 && currentValue >= _tiers[_currentTier + 1].RequiredValue)
            {
                UnlockNextTier();
            }
            
            // Calculate progress within current tier
            if (_currentTier < _tiers.Count)
            {
                var currentTierData = _tiers[_currentTier];
                var previousTierValue = _currentTier > 0 ? _tiers[_currentTier - 1].RequiredValue : 0f;
                var tierRange = currentTierData.RequiredValue - previousTierValue;
                var tierProgress = (currentValue - previousTierValue) / tierRange;
                
                _tierProgress = Mathf.Clamp01(tierProgress);
            }
            
            // Overall progress is based on final tier
            var finalTier = _tiers.LastOrDefault();
            if (finalTier != null)
            {
                CurrentProgress = currentValue / finalTier.RequiredValue;
            }
            
            return CurrentProgress;
        }
        
        public override AchievementProgressInfo GetProgressInfo()
        {
            var baseInfo = base.GetProgressInfo();
            
            baseInfo.TierInfo = new AchievementTierInfo
            {
                CurrentTier = _currentTier,
                TotalTiers = _tiers.Count,
                TierName = _currentTier < _tiers.Count ? _tiers[_currentTier].TierName : "Complete",
                TierProgress = _tierProgress,
                NextTierRequirements = GetNextTierRequirements()
            };
            
            return baseInfo;
        }
        
        protected abstract float GetCurrentValue();
        
        protected virtual void OnTierUnlocked(AchievementTier unlockedTier)
        {
            // Override in derived classes for custom tier unlock logic
            Debug.Log($"Tier unlocked: {unlockedTier.TierName} for achievement {_achievementId}");
        }
    }
}