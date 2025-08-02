using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Registry;

namespace ProjectChimera.Systems.Services.Progression
{
    /// <summary>
    /// PC014-3a: Experience Management Service
    /// XP calculation, distribution, and level progression
    /// Decomposed from ComprehensiveProgressionManager (360 lines target)
    /// </summary>
    public class ExperienceManagementService : MonoBehaviour, IExperienceManagementService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Experience Configuration")]
        [SerializeField] private bool _enableExperienceSystem = true;
        [SerializeField] private float _baseExperienceRequirement = 1000f;
        [SerializeField] private float _experienceScalingFactor = 1.5f;
        [SerializeField] private int _maxPlayerLevel = 100;
        
        [Header("Experience Multipliers")]
        [SerializeField] private Dictionary<string, float> _systemExperienceRates = new Dictionary<string, float>();
        [SerializeField] private List<CrossSystemBonus> _crossSystemBonuses = new List<CrossSystemBonus>();
        [SerializeField] private float _qualityBonusMultiplier = 0.5f;
        [SerializeField] private float _difficultyBonusMultiplier = 0.3f;
        
        [Header("Player Profiles")]
        [SerializeField] private List<PlayerProgressionProfile> _playerProfiles = new List<PlayerProgressionProfile>();
        [SerializeField] private Dictionary<string, DateTime> _lastExperienceUpdate = new Dictionary<string, DateTime>();
        
        [Header("Experience Statistics")]
        [SerializeField] private float _totalExperienceAwarded = 0f;
        [SerializeField] private int _totalLevelsGained = 0;
        [SerializeField] private Dictionary<string, float> _systemExperienceStats = new Dictionary<string, float>();
        
        #endregion

        #region Events
        
        public event Action<string, float> OnExperienceAwarded; // playerId, amount
        public event Action<string, int> OnLevelUp; // playerId, newLevel
        public event Action<string, string, float> OnSystemExperienceGained; // playerId, system, amount
        public event Action<string, ExperienceCalculationResult> OnExperienceCalculated; // playerId, result
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing ExperienceManagementService...");
            
            // Initialize experience system
            InitializeExperienceSystem();
            
            // Initialize system rates
            InitializeSystemExperienceRates();
            
            // Initialize cross-system bonuses
            InitializeCrossSystemBonuses();
            
            // Load existing player profiles
            LoadExistingProfiles();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IExperienceManagementService>(this, ServiceDomain.Progression);
            
            IsInitialized = true;
            Debug.Log("ExperienceManagementService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down ExperienceManagementService...");
            
            // Save experience state
            SaveExperienceState();
            
            // Clear collections
            _playerProfiles.Clear();
            _systemExperienceRates.Clear();
            _crossSystemBonuses.Clear();
            _lastExperienceUpdate.Clear();
            _systemExperienceStats.Clear();
            
            IsInitialized = false;
            Debug.Log("ExperienceManagementService shutdown complete");
        }
        
        #endregion

        #region Experience System
        
        public void AwardExperience(string playerId, ExperienceSourceData source, float amount)
        {
            if (!_enableExperienceSystem || !IsInitialized)
                return;

            var profile = GetOrCreatePlayerProfile(playerId);
            var calculationResult = CalculateExperience(source, amount, profile);
            
            // Apply experience
            float oldExperience = profile.TotalExperience;
            int oldLevel = profile.CurrentLevel;
            
            profile.TotalExperience += calculationResult.FinalExperience;
            
            // Update system experience
            string systemName = source.SystemName;
            if (!profile.SystemExperience.ContainsKey(systemName))
            {
                profile.SystemExperience[systemName] = 0f;
            }
            profile.SystemExperience[systemName] += calculationResult.FinalExperience;
            
            // Track statistics
            _totalExperienceAwarded += calculationResult.FinalExperience;
            if (!_systemExperienceStats.ContainsKey(systemName))
            {
                _systemExperienceStats[systemName] = 0f;
            }
            _systemExperienceStats[systemName] += calculationResult.FinalExperience;
            
            // Check for level up
            int newLevel = GetLevelFromExperience(profile.TotalExperience);
            if (newLevel > oldLevel)
            {
                profile.CurrentLevel = newLevel;
                _totalLevelsGained++;
                ProcessLevelUp(playerId, oldLevel, newLevel);
            }
            
            // Update activity tracking
            profile.RecentSystemActivity[systemName] = DateTime.Now;
            profile.LastActivityDate = DateTime.Now;
            _lastExperienceUpdate[playerId] = DateTime.Now;
            
            // Fire events
            OnExperienceAwarded?.Invoke(playerId, calculationResult.FinalExperience);
            OnSystemExperienceGained?.Invoke(playerId, systemName, calculationResult.FinalExperience);
            OnExperienceCalculated?.Invoke(playerId, calculationResult);
            
            Debug.Log($"Awarded {calculationResult.FinalExperience:F1} XP to {playerId} from {systemName}");
        }

        public float GetExperience(string playerId)
        {
            var profile = GetPlayerProfile(playerId);
            return profile?.TotalExperience ?? 0f;
        }

        public int GetLevel(string playerId)
        {
            var profile = GetPlayerProfile(playerId);
            return profile?.CurrentLevel ?? 1;
        }

        public int GetLevelFromExperience(float experience)
        {
            if (experience <= 0) return 1;
            
            float totalRequired = 0f;
            for (int level = 1; level < _maxPlayerLevel; level++)
            {
                float requiredForThisLevel = _baseExperienceRequirement * Mathf.Pow(_experienceScalingFactor, level - 1);
                totalRequired += requiredForThisLevel;
                
                if (experience < totalRequired)
                {
                    return level;
                }
            }
            
            return _maxPlayerLevel;
        }

        public float GetExperienceForLevel(int level)
        {
            if (level <= 1) return 0f;
            
            float totalRequired = 0f;
            for (int i = 1; i < level; i++)
            {
                totalRequired += _baseExperienceRequirement * Mathf.Pow(_experienceScalingFactor, i - 1);
            }
            
            return totalRequired;
        }

        public float GetExperienceToNextLevel(string playerId)
        {
            var profile = GetPlayerProfile(playerId);
            if (profile == null) return _baseExperienceRequirement;
            
            int currentLevel = profile.CurrentLevel;
            if (currentLevel >= _maxPlayerLevel) return 0f;
            
            float experienceForNextLevel = GetExperienceForLevel(currentLevel + 1);
            return experienceForNextLevel - profile.TotalExperience;
        }
        
        #endregion

        #region Level Progression
        
        public bool CheckLevelUp(string playerId)
        {
            var profile = GetPlayerProfile(playerId);
            if (profile == null) return false;
            
            int levelFromExperience = GetLevelFromExperience(profile.TotalExperience);
            return levelFromExperience > profile.CurrentLevel;
        }

        public LevelUpResult ProcessLevelUp(string playerId)
        {
            return ProcessLevelUp(playerId, GetLevel(playerId), GetLevelFromExperience(GetExperience(playerId)));
        }

        public List<LevelReward> GetLevelRewards(int level)
        {
            var rewards = new List<LevelReward>();
            
            // Base skill points for each level
            rewards.Add(new LevelReward
            {
                RewardId = $"level_{level}_skillpoints",
                RewardName = "Skill Points",
                Type = CleanProgressionRewardType.Currency,
                Value = CalculateSkillPointsForLevel(level),
                Description = $"Skill points awarded for reaching level {level}"
            });
            
            // Special rewards for milestone levels
            if (level % 5 == 0)
            {
                rewards.Add(new LevelReward
                {
                    RewardId = $"level_{level}_bonus",
                    RewardName = "Milestone Bonus",
                    Type = CleanProgressionRewardType.Experience,
                    Value = level * 50f,
                    Description = $"Bonus experience for milestone level {level}"
                });
            }
            
            // Major milestone rewards
            if (level % 10 == 0)
            {
                rewards.Add(new LevelReward
                {
                    RewardId = $"level_{level}_unlock",
                    RewardName = "Content Unlock",
                    Type = CleanProgressionRewardType.Unlock,
                    Value = 1f,
                    Description = $"Special content unlocked at level {level}"
                });
            }
            
            return rewards;
        }
        
        #endregion

        #region Experience Sources
        
        public void RegisterExperienceSource(ExperienceSourceData source)
        {
            if (!_systemExperienceRates.ContainsKey(source.SystemName))
            {
                _systemExperienceRates[source.SystemName] = source.BaseMultiplier;
                Debug.Log($"Registered experience source: {source.SystemName} (multiplier: {source.BaseMultiplier})");
            }
        }

        public List<ExperienceSourceData> GetExperienceSources()
        {
            return _systemExperienceRates.Select(kvp => new ExperienceSourceData
            {
                SystemName = kvp.Key,
                BaseMultiplier = kvp.Value,
                IsActive = true
            }).ToList();
        }

        public ExperienceMultiplier GetExperienceMultiplier(string playerId)
        {
            var profile = GetPlayerProfile(playerId);
            if (profile == null)
            {
                return new ExperienceMultiplier { BaseMultiplier = 1.0f, TotalMultiplier = 1.0f };
            }
            
            float crossSystemMultiplier = CalculateCrossSystemMultiplier(profile);
            float levelBonusMultiplier = 1.0f + (profile.CurrentLevel * 0.01f); // 1% per level
            
            return new ExperienceMultiplier
            {
                BaseMultiplier = 1.0f,
                CrossSystemMultiplier = crossSystemMultiplier,
                LevelBonusMultiplier = levelBonusMultiplier,
                TotalMultiplier = crossSystemMultiplier * levelBonusMultiplier
            };
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeExperienceSystem()
        {
            if (_playerProfiles == null)
                _playerProfiles = new List<PlayerProgressionProfile>();
            
            if (_systemExperienceStats == null)
                _systemExperienceStats = new Dictionary<string, float>();
            
            Debug.Log("Experience system initialized");
        }

        private void InitializeSystemExperienceRates()
        {
            var defaultRates = new Dictionary<string, float>
            {
                {"Cultivation", 1.0f},
                {"Genetics", 1.2f},
                {"Business", 0.8f},
                {"Research", 1.5f},
                {"Competition", 2.0f},
                {"Achievement", 1.3f},
                {"Social", 0.9f}
            };
            
            foreach (var kvp in defaultRates)
            {
                if (!_systemExperienceRates.ContainsKey(kvp.Key))
                {
                    _systemExperienceRates[kvp.Key] = kvp.Value;
                }
            }
            
            Debug.Log($"Initialized {_systemExperienceRates.Count} system experience rates");
        }

        private void InitializeCrossSystemBonuses()
        {
            _crossSystemBonuses.Clear();
            
            _crossSystemBonuses.Add(new CrossSystemBonus
            {
                BonusID = "cultivation_genetics",
                BonusName = "Science Synergy",
                RequiredSystems = new List<string> {"Cultivation", "Genetics"},
                ExperienceMultiplier = 1.25f,
                Description = "25% bonus when using cultivation and genetics together"
            });
            
            _crossSystemBonuses.Add(new CrossSystemBonus
            {
                BonusID = "research_competition",
                BonusName = "Competitive Edge",
                RequiredSystems = new List<string> {"Research", "Competition"},
                ExperienceMultiplier = 1.30f,
                Description = "30% bonus for research-driven competition"
            });
            
            Debug.Log($"Initialized {_crossSystemBonuses.Count} cross-system bonuses");
        }

        private void LoadExistingProfiles()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing player profiles...");
        }

        private void SaveExperienceState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving experience state...");
        }

        private PlayerProgressionProfile GetOrCreatePlayerProfile(string playerId)
        {
            var profile = _playerProfiles.FirstOrDefault(p => p.PlayerId == playerId);
            if (profile == null)
            {
                profile = new PlayerProgressionProfile
                {
                    PlayerId = playerId,
                    PlayerName = playerId,
                    CurrentLevel = 1,
                    TotalExperience = 0f,
                    SystemExperience = new Dictionary<string, float>(),
                    CompletedMilestones = new List<string>(),
                    UnlockedContent = new List<string>(),
                    RecentSystemActivity = new Dictionary<string, DateTime>(),
                    ProfileCreatedDate = DateTime.Now,
                    LastActivityDate = DateTime.Now,
                    Stats = new ProgressionStats()
                };
                
                _playerProfiles.Add(profile);
                Debug.Log($"Created new player profile for {playerId}");
            }
            
            return profile;
        }

        private PlayerProgressionProfile GetPlayerProfile(string playerId)
        {
            return _playerProfiles.FirstOrDefault(p => p.PlayerId == playerId);
        }

        private ExperienceCalculationResult CalculateExperience(ExperienceSourceData source, float baseAmount, PlayerProgressionProfile profile)
        {
            var result = new ExperienceCalculationResult
            {
                BaseExperience = baseAmount
            };
            
            // Apply system multiplier
            float systemMultiplier = _systemExperienceRates.GetValueOrDefault(source.SystemName, 1.0f);
            
            // Apply quality bonus
            result.QualityBonus = baseAmount * source.QualityMultiplier * _qualityBonusMultiplier;
            
            // Apply difficulty bonus
            result.DifficultyBonus = baseAmount * source.DifficultyMultiplier * _difficultyBonusMultiplier;
            
            // Apply cross-system multiplier
            result.CrossSystemMultiplier = CalculateCrossSystemMultiplier(profile);
            
            // Calculate final experience
            float total = (baseAmount + result.QualityBonus + result.DifficultyBonus) * systemMultiplier * result.CrossSystemMultiplier;
            result.FinalExperience = total;
            
            // Create breakdown
            result.CalculationBreakdown = $"Base: {baseAmount:F1}, Quality: +{result.QualityBonus:F1}, " +
                                        $"Difficulty: +{result.DifficultyBonus:F1}, System: x{systemMultiplier:F2}, " +
                                        $"Cross-System: x{result.CrossSystemMultiplier:F2} = {total:F1} XP";
            
            return result;
        }

        private float CalculateCrossSystemMultiplier(PlayerProgressionProfile profile)
        {
            float multiplier = 1.0f;
            var recentActivity = profile.RecentSystemActivity.Where(kvp => 
                (DateTime.Now - kvp.Value).TotalHours <= 24).Select(kvp => kvp.Key).ToList();
            
            foreach (var bonus in _crossSystemBonuses)
            {
                if (bonus.RequiredSystems.All(system => recentActivity.Contains(system)))
                {
                    multiplier = Mathf.Max(multiplier, bonus.ExperienceMultiplier);
                    bonus.IsActive = true;
                    bonus.ActivationCount++;
                    bonus.LastActivation = DateTime.Now;
                }
                else
                {
                    bonus.IsActive = false;
                }
            }
            
            return multiplier;
        }

        private LevelUpResult ProcessLevelUp(string playerId, int oldLevel, int newLevel)
        {
            var result = new LevelUpResult
            {
                OldLevel = oldLevel,
                NewLevel = newLevel,
                SkillPointsAwarded = CalculateSkillPointsForLevel(newLevel),
                LevelRewards = GetLevelRewards(newLevel).Select(lr => new CleanProgressionReward
                {
                    RewardID = lr.RewardId,
                    RewardName = lr.RewardName,
                    RewardType = lr.Type.ToString(),
                    Description = lr.Description
                }).ToList()
            };
            
            OnLevelUp?.Invoke(playerId, newLevel);
            Debug.Log($"Player {playerId} leveled up: {oldLevel} → {newLevel} (+{result.SkillPointsAwarded} skill points)");
            
            return result;
        }

        private int CalculateSkillPointsForLevel(int level)
        {
            // Award more skill points for higher levels
            if (level <= 10) return 1;
            if (level <= 25) return 2;
            if (level <= 50) return 3;
            if (level <= 75) return 4;
            return 5;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
    }
}