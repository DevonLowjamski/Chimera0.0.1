using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using AchievementCategory = ProjectChimera.Data.Progression.AchievementCategory;
using AchievementRarity = ProjectChimera.Data.Progression.AchievementRarity;
using AchievementProgress = ProjectChimera.Data.Achievements.AchievementProgress;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// PC-012-2b: Achievement Reward Service - Specialized reward calculation and distribution
    /// Handles achievement reward processing, currency distribution, XP awards, and item rewards
    /// Part of the decomposed AchievementSystemManager (500/1903 lines)
    /// Integrates with player progression, inventory, and economy systems
    /// </summary>
    public class AchievementRewardService : MonoBehaviour, IAchievementRewardService
    {
        [Header("Reward Configuration")]
        [SerializeField] private bool _enableRewards = true;
        [SerializeField] private bool _enableRewardLogging = true;
        [SerializeField] private float _rewardMultiplier = 1.0f;
        [SerializeField] private bool _enableBonusRewards = true;
        [SerializeField] private float _bonusRewardChance = 0.15f;

        [Header("Currency Rewards")]
        [SerializeField] private bool _enableCurrencyRewards = true;
        [SerializeField] private int _baseCurrencyReward = 100;
        [SerializeField] private float _currencyMultiplier = 1.2f;
        [SerializeField] private int _maxCurrencyReward = 10000;

        [Header("Experience Rewards")]
        [SerializeField] private bool _enableXpRewards = true;
        [SerializeField] private int _baseXpReward = 50;
        [SerializeField] private float _xpMultiplier = 1.5f;
        [SerializeField] private int _maxXpReward = 5000;

        [Header("Item Rewards")]
        [SerializeField] private bool _enableItemRewards = true;
        [SerializeField] private float _itemRewardChance = 0.25f;
        [SerializeField] private int _maxItemsPerReward = 3;

        // Service state
        private bool _isInitialized = false;
        private Dictionary<AchievementRarity, RewardMultipliers> _rarityMultipliers = new Dictionary<AchievementRarity, RewardMultipliers>();
        private Dictionary<AchievementCategory, CategoryRewardBonus> _categoryBonuses = new Dictionary<AchievementCategory, CategoryRewardBonus>();
        private List<RewardTransaction> _rewardHistory = new List<RewardTransaction>();
        private Dictionary<string, float> _playerTotalRewards = new Dictionary<string, float>();

        // Events for reward distribution
        public event Action<string, RewardBundle> OnRewardCalculated;
        public event Action<string, RewardBundle> OnRewardDistributed;
        public event Action<string, string, float> OnCurrencyRewarded;
        public event Action<string, int> OnExperienceRewarded;
        public event Action<string, List<ItemReward>> OnItemsRewarded;
        public event Action<RewardTransaction> OnRewardTransactionCompleted;

        // Legacy events for backward compatibility
        public static event Action<string, float> OnPlayerCurrencyUpdated;
        public static event Action<string, int> OnPlayerExperienceUpdated;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Achievement Reward Service";
        public IReadOnlyList<RewardTransaction> RewardHistory => _rewardHistory;
        public float TotalRewardsDistributed => _playerTotalRewards.Values.Sum();

        public void Initialize()
        {
            InitializeService();
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeRewardSystem();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeRewardSystem()
        {
            _rarityMultipliers = new Dictionary<AchievementRarity, RewardMultipliers>();
            _categoryBonuses = new Dictionary<AchievementCategory, CategoryRewardBonus>();
            _rewardHistory = new List<RewardTransaction>();
            _playerTotalRewards = new Dictionary<string, float>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("AchievementRewardService already initialized", this);
                return;
            }

            try
            {
                SetupRarityMultipliers();
                SetupCategoryBonuses();
                
                _isInitialized = true;
                ChimeraLogger.Log("AchievementRewardService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize AchievementRewardService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            _rarityMultipliers.Clear();
            _categoryBonuses.Clear();
            _rewardHistory.Clear();
            _playerTotalRewards.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("AchievementRewardService shutdown completed", this);
        }

        #endregion

        #region Reward System Setup

        private void SetupRarityMultipliers()
        {
            _rarityMultipliers[AchievementRarity.Common] = new RewardMultipliers
            {
                CurrencyMultiplier = 1.0f,
                ExperienceMultiplier = 1.0f,
                ItemChanceMultiplier = 1.0f,
                BonusChanceMultiplier = 1.0f
            };

            _rarityMultipliers[AchievementRarity.Uncommon] = new RewardMultipliers
            {
                CurrencyMultiplier = 1.5f,
                ExperienceMultiplier = 1.3f,
                ItemChanceMultiplier = 1.2f,
                BonusChanceMultiplier = 1.1f
            };

            _rarityMultipliers[AchievementRarity.Rare] = new RewardMultipliers
            {
                CurrencyMultiplier = 2.0f,
                ExperienceMultiplier = 1.8f,
                ItemChanceMultiplier = 1.5f,
                BonusChanceMultiplier = 1.3f
            };

            _rarityMultipliers[AchievementRarity.Epic] = new RewardMultipliers
            {
                CurrencyMultiplier = 3.0f,
                ExperienceMultiplier = 2.5f,
                ItemChanceMultiplier = 2.0f,
                BonusChanceMultiplier = 1.6f
            };

            _rarityMultipliers[AchievementRarity.Legendary] = new RewardMultipliers
            {
                CurrencyMultiplier = 5.0f,
                ExperienceMultiplier = 4.0f,
                ItemChanceMultiplier = 3.0f,
                BonusChanceMultiplier = 2.0f
            };
        }

        private void SetupCategoryBonuses()
        {
            _categoryBonuses[AchievementCategory.Cultivation_Mastery] = new CategoryRewardBonus
            {
                BonusMultiplier = 1.2f,
                SpecialItemChance = 0.1f,
                CategorySpecificRewards = new List<string> { "advanced_nutrients", "rare_seeds", "cultivation_tools" }
            };

            _categoryBonuses[AchievementCategory.Genetics_Innovation] = new CategoryRewardBonus
            {
                BonusMultiplier = 1.3f,
                SpecialItemChance = 0.15f,
                CategorySpecificRewards = new List<string> { "genetic_markers", "breeding_equipment", "lab_tools" }
            };

            _categoryBonuses[AchievementCategory.Research_Excellence] = new CategoryRewardBonus
            {
                BonusMultiplier = 1.25f,
                SpecialItemChance = 0.12f,
                CategorySpecificRewards = new List<string> { "research_books", "lab_equipment", "data_analyzers" }
            };

            _categoryBonuses[AchievementCategory.Business_Success] = new CategoryRewardBonus
            {
                BonusMultiplier = 1.4f,
                SpecialItemChance = 0.08f,
                CategorySpecificRewards = new List<string> { "business_contracts", "market_data", "investment_opportunities" }
            };

            _categoryBonuses[AchievementCategory.Social] = new CategoryRewardBonus
            {
                BonusMultiplier = 1.1f,
                SpecialItemChance = 0.2f,
                CategorySpecificRewards = new List<string> { "social_tokens", "reputation_boosts", "community_access" }
            };
        }

        #endregion

        #region Reward Calculation

        public RewardBundle CalculateRewards(Achievement achievement, string playerId = "current_player")
        {
            if (!_isInitialized || !_enableRewards)
            {
                return new RewardBundle();
            }

            var rewardBundle = new RewardBundle
            {
                AchievementId = achievement.AchievementID,
                PlayerId = playerId,
                CalculatedAt = DateTime.Now
            };

            // Get multipliers for this achievement
            var rarityMultiplier = GetRarityMultiplier(achievement.Rarity);
            var categoryBonus = GetCategoryBonus(achievement.Category);

            // Calculate currency rewards
            if (_enableCurrencyRewards)
            {
                rewardBundle.CurrencyReward = CalculateCurrencyReward(achievement, rarityMultiplier, categoryBonus);
            }

            // Calculate experience rewards
            if (_enableXpRewards)
            {
                rewardBundle.ExperienceReward = CalculateExperienceReward(achievement, rarityMultiplier, categoryBonus);
            }

            // Calculate item rewards
            if (_enableItemRewards)
            {
                rewardBundle.ItemRewards = CalculateItemRewards(achievement, rarityMultiplier, categoryBonus);
            }

            // Apply bonus rewards if enabled
            if (_enableBonusRewards && ShouldApplyBonusReward(rarityMultiplier, categoryBonus))
            {
                ApplyBonusRewards(rewardBundle, rarityMultiplier, categoryBonus);
            }

            // Calculate total reward value
            rewardBundle.TotalValue = CalculateTotalRewardValue(rewardBundle);

            OnRewardCalculated?.Invoke(playerId, rewardBundle);

            if (_enableRewardLogging)
            {
                ChimeraLogger.Log($"Calculated rewards for {achievement.AchievementName}: {rewardBundle.TotalValue} total value", this);
            }

            return rewardBundle;
        }

        private int CalculateCurrencyReward(Achievement achievement, RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            float baseReward = _baseCurrencyReward;
            
            // Apply achievement points multiplier
            baseReward *= (achievement.Points / 100f);
            
            // Apply rarity multiplier
            baseReward *= rarityMultiplier.CurrencyMultiplier;
            
            // Apply category bonus
            baseReward *= categoryBonus.BonusMultiplier;
            
            // Apply global multiplier
            baseReward *= _rewardMultiplier;
            
            // Apply currency-specific multiplier
            baseReward *= _currencyMultiplier;

            int finalReward = Mathf.RoundToInt(baseReward);
            return Mathf.Clamp(finalReward, 0, _maxCurrencyReward);
        }

        private int CalculateExperienceReward(Achievement achievement, RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            float baseReward = _baseXpReward;
            
            // Apply achievement points multiplier (XP is typically lower than currency)
            baseReward *= (achievement.Points / 150f);
            
            // Apply rarity multiplier
            baseReward *= rarityMultiplier.ExperienceMultiplier;
            
            // Apply category bonus
            baseReward *= categoryBonus.BonusMultiplier;
            
            // Apply global multiplier
            baseReward *= _rewardMultiplier;
            
            // Apply XP-specific multiplier
            baseReward *= _xpMultiplier;

            int finalReward = Mathf.RoundToInt(baseReward);
            return Mathf.Clamp(finalReward, 0, _maxXpReward);
        }

        private List<ItemReward> CalculateItemRewards(Achievement achievement, RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            var itemRewards = new List<ItemReward>();
            
            // Calculate base item chance
            float itemChance = _itemRewardChance * rarityMultiplier.ItemChanceMultiplier;
            
            // Add category-specific special item chance
            if (UnityEngine.Random.Range(0f, 1f) <= categoryBonus.SpecialItemChance)
            {
                itemChance += 0.3f; // Boost chance for category-specific items
            }

            // Determine number of items to award
            int itemCount = 0;
            for (int i = 0; i < _maxItemsPerReward; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) <= itemChance)
                {
                    itemCount++;
                    itemChance *= 0.7f; // Reduce chance for additional items
                }
            }

            // Generate item rewards
            for (int i = 0; i < itemCount; i++)
            {
                var itemReward = GenerateItemReward(achievement, rarityMultiplier, categoryBonus);
                if (itemReward != null)
                {
                    itemRewards.Add(itemReward);
                }
            }

            return itemRewards;
        }

        private ItemReward GenerateItemReward(Achievement achievement, RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            // Determine if this should be a category-specific item
            bool isCategorySpecific = UnityEngine.Random.Range(0f, 1f) <= categoryBonus.SpecialItemChance;
            
            string itemId;
            ItemRarity itemRarity;
            int quantity;

            if (isCategorySpecific && categoryBonus.CategorySpecificRewards.Count > 0)
            {
                // Select category-specific item
                itemId = categoryBonus.CategorySpecificRewards[UnityEngine.Random.Range(0, categoryBonus.CategorySpecificRewards.Count)];
                itemRarity = DetermineItemRarity(achievement.Rarity, true);
                quantity = UnityEngine.Random.Range(1, 3);
            }
            else
            {
                // Select general item
                itemId = GenerateGeneralItemId(achievement);
                itemRarity = DetermineItemRarity(achievement.Rarity, false);
                quantity = UnityEngine.Random.Range(1, 4);
            }

            return new ItemReward
            {
                ItemId = itemId,
                Quantity = quantity,
                Rarity = itemRarity,
                Source = $"Achievement: {achievement.AchievementName}",
                AwardedAt = DateTime.Now
            };
        }

        private string GenerateGeneralItemId(Achievement achievement)
        {
            var generalItems = new List<string>
            {
                "growth_booster", "nutrient_supplement", "pest_control", "ph_tester",
                "thermometer", "humidity_meter", "light_meter", "watering_can",
                "pruning_shears", "plant_stakes", "fertilizer_basic", "soil_amendment",
                "seed_starter", "clone_gel", "harvest_bag", "drying_rack"
            };

            return generalItems[UnityEngine.Random.Range(0, generalItems.Count)];
        }

        private ItemRarity DetermineItemRarity(AchievementRarity achievementRarity, bool isCategorySpecific)
        {
            float rarityBoost = isCategorySpecific ? 0.2f : 0f;
            float randomValue = UnityEngine.Random.Range(0f, 1f) + rarityBoost;

            return achievementRarity switch
            {
                AchievementRarity.Common => randomValue > 0.8f ? ItemRarity.Uncommon : ItemRarity.Common,
                AchievementRarity.Uncommon => randomValue > 0.7f ? ItemRarity.Rare : (randomValue > 0.3f ? ItemRarity.Uncommon : ItemRarity.Common),
                AchievementRarity.Rare => randomValue > 0.6f ? ItemRarity.Epic : (randomValue > 0.2f ? ItemRarity.Rare : ItemRarity.Uncommon),
                AchievementRarity.Epic => randomValue > 0.4f ? ItemRarity.Legendary : (randomValue > 0.1f ? ItemRarity.Epic : ItemRarity.Rare),
                AchievementRarity.Legendary => randomValue > 0.3f ? ItemRarity.Legendary : ItemRarity.Epic,
                _ => ItemRarity.Common
            };
        }

        private bool ShouldApplyBonusReward(RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            float bonusChance = _bonusRewardChance * rarityMultiplier.BonusChanceMultiplier * categoryBonus.BonusMultiplier;
            return UnityEngine.Random.Range(0f, 1f) <= bonusChance;
        }

        private void ApplyBonusRewards(RewardBundle rewardBundle, RewardMultipliers rarityMultiplier, CategoryRewardBonus categoryBonus)
        {
            // Apply bonus multipliers
            rewardBundle.CurrencyReward = Mathf.RoundToInt(rewardBundle.CurrencyReward * 1.5f);
            rewardBundle.ExperienceReward = Mathf.RoundToInt(rewardBundle.ExperienceReward * 1.3f);
            
            // Add bonus item
            var bonusItem = new ItemReward
            {
                ItemId = "bonus_reward_token",
                Quantity = 1,
                Rarity = ItemRarity.Epic,
                Source = "Bonus Reward",
                AwardedAt = DateTime.Now
            };
            
            rewardBundle.ItemRewards.Add(bonusItem);
            rewardBundle.HasBonusReward = true;

            if (_enableRewardLogging)
            {
                ChimeraLogger.Log($"Applied bonus rewards to {rewardBundle.AchievementId}", this);
            }
        }

        private float CalculateTotalRewardValue(RewardBundle rewardBundle)
        {
            float totalValue = 0f;
            
            // Currency value (1:1 ratio)
            totalValue += rewardBundle.CurrencyReward;
            
            // Experience value (convert to currency equivalent)
            totalValue += rewardBundle.ExperienceReward * 0.5f;
            
            // Item values (estimated based on rarity)
            foreach (var item in rewardBundle.ItemRewards)
            {
                float itemValue = item.Rarity switch
                {
                    ItemRarity.Common => 50f,
                    ItemRarity.Uncommon => 100f,
                    ItemRarity.Rare => 250f,
                    ItemRarity.Epic => 500f,
                    ItemRarity.Legendary => 1000f,
                    _ => 25f
                };
                totalValue += itemValue * item.Quantity;
            }

            return totalValue;
        }

        #endregion

        #region Reward Distribution

        public bool DistributeRewards(RewardBundle rewardBundle)
        {
            if (!_isInitialized || rewardBundle == null)
            {
                return false;
            }

            try
            {
                var transaction = new RewardTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    PlayerId = rewardBundle.PlayerId,
                    AchievementId = rewardBundle.AchievementId,
                    RewardBundle = rewardBundle,
                    ProcessedAt = DateTime.Now,
                    Success = false
                };

                // Distribute currency
                if (rewardBundle.CurrencyReward > 0)
                {
                    DistributeCurrency(rewardBundle.PlayerId, rewardBundle.CurrencyReward);
                }

                // Distribute experience
                if (rewardBundle.ExperienceReward > 0)
                {
                    DistributeExperience(rewardBundle.PlayerId, rewardBundle.ExperienceReward);
                }

                // Distribute items
                if (rewardBundle.ItemRewards.Count > 0)
                {
                    DistributeItems(rewardBundle.PlayerId, rewardBundle.ItemRewards);
                }

                // Update player total rewards
                UpdatePlayerTotalRewards(rewardBundle.PlayerId, rewardBundle.TotalValue);

                // Record successful transaction
                transaction.Success = true;
                _rewardHistory.Add(transaction);

                OnRewardDistributed?.Invoke(rewardBundle.PlayerId, rewardBundle);
                OnRewardTransactionCompleted?.Invoke(transaction);

                if (_enableRewardLogging)
                {
                    ChimeraLogger.Log($"Successfully distributed rewards to {rewardBundle.PlayerId}: {rewardBundle.TotalValue} total value", this);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to distribute rewards: {ex.Message}", this);
                return false;
            }
        }

        private void DistributeCurrency(string playerId, int amount)
        {
            // TODO: Integrate with actual currency system
            // For now, just fire events
            OnCurrencyRewarded?.Invoke(playerId, "coins", amount);
            OnPlayerCurrencyUpdated?.Invoke(playerId, amount);

            if (_enableRewardLogging)
            {
                ChimeraLogger.Log($"Awarded {amount} coins to {playerId}", this);
            }
        }

        private void DistributeExperience(string playerId, int amount)
        {
            // TODO: Integrate with actual progression system
            // For now, just fire events
            OnExperienceRewarded?.Invoke(playerId, amount);
            OnPlayerExperienceUpdated?.Invoke(playerId, amount);

            if (_enableRewardLogging)
            {
                ChimeraLogger.Log($"Awarded {amount} XP to {playerId}", this);
            }
        }

        private void DistributeItems(string playerId, List<ItemReward> items)
        {
            // TODO: Integrate with actual inventory system
            // For now, just fire events
            OnItemsRewarded?.Invoke(playerId, items);

            if (_enableRewardLogging)
            {
                ChimeraLogger.Log($"Awarded {items.Count} items to {playerId}", this);
            }
        }

        private void UpdatePlayerTotalRewards(string playerId, float value)
        {
            if (!_playerTotalRewards.ContainsKey(playerId))
            {
                _playerTotalRewards[playerId] = 0f;
            }
            
            _playerTotalRewards[playerId] += value;
        }

        #endregion

        #region Helper Methods

        private RewardMultipliers GetRarityMultiplier(AchievementRarity rarity)
        {
            return _rarityMultipliers.TryGetValue(rarity, out var multiplier) ? multiplier : _rarityMultipliers[AchievementRarity.Common];
        }

        private CategoryRewardBonus GetCategoryBonus(AchievementCategory category)
        {
            return _categoryBonuses.TryGetValue(category, out var bonus) ? bonus : new CategoryRewardBonus
            {
                BonusMultiplier = 1.0f,
                SpecialItemChance = 0.05f,
                CategorySpecificRewards = new List<string>()
            };
        }

        public float GetPlayerTotalRewards(string playerId)
        {
            return _playerTotalRewards.TryGetValue(playerId, out var total) ? total : 0f;
        }

        public List<RewardTransaction> GetPlayerRewardHistory(string playerId)
        {
            return _rewardHistory.Where(t => t.PlayerId == playerId).ToList();
        }

        public RewardStatistics GetRewardStatistics()
        {
            return new RewardStatistics
            {
                TotalTransactions = _rewardHistory.Count,
                TotalValueDistributed = _rewardHistory.Sum(t => t.RewardBundle.TotalValue),
                TotalPlayersRewarded = _playerTotalRewards.Count,
                AverageLengthPerTransaction = _rewardHistory.Count > 0 ? _rewardHistory.Average(t => t.RewardBundle.TotalValue) : 0f,
                MostRewardedPlayer = _playerTotalRewards.Count > 0
                    ? _playerTotalRewards.OrderByDescending(kvp => kvp.Value).First().Key 
                    : "None"
            };
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class RewardMultipliers
    {
        public float CurrencyMultiplier = 1.0f;
        public float ExperienceMultiplier = 1.0f;
        public float ItemChanceMultiplier = 1.0f;
        public float BonusChanceMultiplier = 1.0f;
    }

    [System.Serializable]
    public class CategoryRewardBonus
    {
        public float BonusMultiplier = 1.0f;
        public float SpecialItemChance = 0.1f;
        public List<string> CategorySpecificRewards = new List<string>();
    }

    [System.Serializable]
    public class RewardBundle
    {
        public string AchievementId = "";
        public string PlayerId = "";
        public int CurrencyReward = 0;
        public int ExperienceReward = 0;
        public List<ItemReward> ItemRewards = new List<ItemReward>();
        public bool HasBonusReward = false;
        public float TotalValue = 0f;
        public DateTime CalculatedAt = DateTime.Now;
    }

    [System.Serializable]
    public class ItemReward
    {
        public string ItemId = "";
        public int Quantity = 1;
        public ItemRarity Rarity = ItemRarity.Common;
        public string Source = "";
        public DateTime AwardedAt = DateTime.Now;
    }

    [System.Serializable]
    public class RewardTransaction
    {
        public string TransactionId = "";
        public string PlayerId = "";
        public string AchievementId = "";
        public RewardBundle RewardBundle;
        public bool Success = false;
        public DateTime ProcessedAt = DateTime.Now;
    }

    [System.Serializable]
    public class RewardStatistics
    {
        public int TotalTransactions = 0;
        public float TotalValueDistributed = 0f;
        public int TotalPlayersRewarded = 0;
        public float AverageLengthPerTransaction = 0f;
        public string MostRewardedPlayer = "";
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    #endregion
}