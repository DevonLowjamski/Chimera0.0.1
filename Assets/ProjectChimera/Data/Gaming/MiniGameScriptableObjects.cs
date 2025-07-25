using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Gaming;

namespace ProjectChimera.Data.Gaming
{
    /// <summary>
    /// Database of all available mini-games in Project Chimera
    /// </summary>
    [CreateAssetMenu(fileName = "MiniGameDatabase", menuName = "Project Chimera/Gaming/Mini Game Database")]
    public class MiniGameDatabaseSO : ChimeraConfigSO
    {
        [Header("Mini Game Registry")]
        [SerializeField] private List<MiniGameDefinition> _availableGames = new List<MiniGameDefinition>();
        [SerializeField] private List<string> _enabledGameIds = new List<string>();
        
        [Header("Game Categories")]
        [SerializeField] private List<GameCategoryConfig> _categories = new List<GameCategoryConfig>();
        
        public List<MiniGameDefinition> AvailableGames => _availableGames;
        public List<string> EnabledGameIds => _enabledGameIds;
        public List<GameCategoryConfig> Categories => _categories;
        
        public MiniGameDefinition GetGameById(string gameId)
        {
            return _availableGames.Find(game => game.GameId == gameId);
        }
        
        public bool IsGameEnabled(string gameId)
        {
            return _enabledGameIds.Contains(gameId);
        }
    }
    
    /// <summary>
    /// Configuration for difficulty scaling in mini-games
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyScalingConfig", menuName = "Project Chimera/Gaming/Difficulty Scaling Config")]
    public class DifficultyScalingConfigSO : ChimeraConfigSO
    {
        [Header("Adaptive Difficulty Settings")]
        [SerializeField] private float _baseSuccessRate = 0.7f;
        [SerializeField] private float _difficultyAdjustmentRate = 0.1f;
        [SerializeField] private int _performanceWindowSize = 10;
        
        [Header("Difficulty Curves")]
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.Linear(0, 1, 1, 2);
        [SerializeField] private AnimationCurve _complexityCurve = AnimationCurve.Linear(0, 1, 1, 1.5f);
        [SerializeField] private AnimationCurve _precisionCurve = AnimationCurve.Linear(0, 0.9f, 1, 0.95f);
        
        public float BaseSuccessRate => _baseSuccessRate;
        public float DifficultyAdjustmentRate => _difficultyAdjustmentRate;
        public int PerformanceWindowSize => _performanceWindowSize;
        public AnimationCurve SpeedCurve => _speedCurve;
        public AnimationCurve ComplexityCurve => _complexityCurve;
        public AnimationCurve PrecisionCurve => _precisionCurve;
    }
    
    /// <summary>
    /// Configuration for mini-game rewards and progression
    /// </summary>
    [CreateAssetMenu(fileName = "MiniGameRewardConfig", menuName = "Project Chimera/Gaming/Reward Config")]
    public class MiniGameRewardConfigSO : ChimeraConfigSO
    {
        [Header("Base Rewards")]
        [SerializeField] private int _baseCurrencyReward = 100;
        [SerializeField] private float _baseExperienceReward = 50f;
        
        [Header("Multipliers")]
        [SerializeField] private float _perfectBonusMultiplier = 1.5f;
        [SerializeField] private float _speedBonusMultiplier = 1.2f;
        [SerializeField] private float _streakMultiplier = 1.1f;
        [SerializeField] private float _difficultyMultiplier = 1.3f;
        
        [Header("Progression Rewards")]
        [SerializeField] private List<ProgressionReward> _progressionRewards = new List<ProgressionReward>();
        
        public int BaseCurrencyReward => _baseCurrencyReward;
        public float BaseExperienceReward => _baseExperienceReward;
        public float PerfectBonusMultiplier => _perfectBonusMultiplier;
        public float SpeedBonusMultiplier => _speedBonusMultiplier;
        public float StreakMultiplier => _streakMultiplier;
        public float DifficultyMultiplier => _difficultyMultiplier;
        public List<ProgressionReward> ProgressionRewards => _progressionRewards;
    }
    
    /// <summary>
    /// Definition of a mini-game
    /// </summary>
    [System.Serializable]
    public class MiniGameDefinition
    {
        public string GameId;
        public string GameName;
        public string Description;
        public string Category;
        public GameObject GamePrefab;
        public Sprite GameIcon;
        public bool IsUnlocked;
        public int RequiredLevel;
        public List<string> Prerequisites = new List<string>();
    }
    
    /// <summary>
    /// Configuration for game categories
    /// </summary>
    [System.Serializable]
    public class GameCategoryConfig
    {
        public string CategoryId;
        public string CategoryName;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public bool IsEnabled = true;
    }
    
    /// <summary>
    /// Progression reward configuration
    /// </summary>
    [System.Serializable]
    public class ProgressionReward
    {
        public string RewardId;
        public string RewardName;
        public string RewardType; // "currency", "experience", "unlock", "item"
        public float RewardValue;
        public int RequiredScore;
        public string UnlockContent;
    }
} 