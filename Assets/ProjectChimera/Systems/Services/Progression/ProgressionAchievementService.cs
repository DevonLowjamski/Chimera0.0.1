using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using ProjectChimera.Systems.Registry;

// Type disambiguation
using ProgressionAchievementReward = ProjectChimera.Data.Progression.AchievementReward;
using AchievementType = ProjectChimera.Data.Achievements.AchievementType;
using AchievementDifficulty = ProjectChimera.Data.Achievements.AchievementDifficulty;

namespace ProjectChimera.Systems.Services.Progression
{
    /// <summary>
    /// PC014-3c: Progression Achievement Service
    /// Milestone tracking and achievement unlock logic
    /// Decomposed from ComprehensiveProgressionManager (350 lines target)
    /// </summary>
    public class ProgressionAchievementService : MonoBehaviour, IProgressionAchievementService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Achievement Configuration")]
        [SerializeField] private bool _enableAchievements = true;
        [SerializeField] private float _achievementCheckInterval = 5.0f;
        [SerializeField] private int _maxPendingRewards = 50;
        
        [Header("Achievement Data")]
        [SerializeField] private List<Achievement> _allAchievements = new List<Achievement>();
        [SerializeField] private List<Milestone> _allMilestones = new List<Milestone>();
        [SerializeField] private Dictionary<string, Dictionary<string, float>> _playerAchievementProgress = new Dictionary<string, Dictionary<string, float>>();
        [SerializeField] private Dictionary<string, List<string>> _playerUnlockedAchievements = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, List<string>> _playerCompletedMilestones = new Dictionary<string, List<string>>();
        
        [Header("Rewards")]
        [SerializeField] private Dictionary<string, List<ProgressionAchievementReward>> _pendingRewards = new Dictionary<string, List<ProgressionAchievementReward>>();
        [SerializeField] private Dictionary<string, MilestoneReward> _milestoneRewards = new Dictionary<string, MilestoneReward>();
        [SerializeField] private List<CleanProgressionReward> _availableRewards = new List<CleanProgressionReward>();
        
        [Header("Statistics")]
        [SerializeField] private int _totalAchievementsUnlocked = 0;
        [SerializeField] private int _totalMilestonesCompleted = 0;
        [SerializeField] private Dictionary<string, int> _achievementUnlockCounts = new Dictionary<string, int>();
        
        private float _lastAchievementCheck = 0f;
        
        #endregion

        #region Events
        
        public event Action<string, string> OnAchievementUnlocked; // playerId, achievementId
        public event Action<string, string> OnMilestoneAchieved; // playerId, milestoneId
        public event Action<string, string> OnAchievementProgressUpdated; // playerId, achievementId
        public event Action<string, ProgressionAchievementReward> OnRewardDistributed; // playerId, reward
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing ProgressionAchievementService...");
            
            // Initialize achievement system
            InitializeAchievementSystem();
            
            // Initialize achievements and milestones
            InitializeAchievements();
            InitializeMilestones();
            
            // Initialize rewards
            InitializeRewards();
            
            // Load existing data
            LoadExistingAchievementData();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IProgressionAchievementService>(this, ServiceDomain.Progression);
            
            IsInitialized = true;
            Debug.Log("ProgressionAchievementService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down ProgressionAchievementService...");
            
            // Save achievement state
            SaveAchievementState();
            
            // Clear collections
            _allAchievements.Clear();
            _allMilestones.Clear();
            _playerAchievementProgress.Clear();
            _playerUnlockedAchievements.Clear();
            _playerCompletedMilestones.Clear();
            _pendingRewards.Clear();
            _milestoneRewards.Clear();
            _achievementUnlockCounts.Clear();
            
            IsInitialized = false;
            Debug.Log("ProgressionAchievementService shutdown complete");
        }
        
        #endregion

        #region Achievement Tracking
        
        public void TrackAchievementProgress(string playerId, string achievementId, float progress)
        {
            if (!_enableAchievements || !IsInitialized)
                return;

            var achievement = _allAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
            if (achievement == null)
            {
                Debug.LogWarning($"Achievement not found: {achievementId}");
                return;
            }

            if (!_playerAchievementProgress.ContainsKey(playerId))
            {
                _playerAchievementProgress[playerId] = new Dictionary<string, float>();
            }

            float oldProgress = _playerAchievementProgress[playerId].GetValueOrDefault(achievementId, 0f);
            float newProgress = Mathf.Max(oldProgress, progress); // Progress can only increase
            
            _playerAchievementProgress[playerId][achievementId] = newProgress;

            OnAchievementProgressUpdated?.Invoke(playerId, achievementId);

            // Check if achievement should be unlocked
            if (newProgress >= 100f && !IsAchievementUnlocked(playerId, achievementId))
            {
                UnlockAchievementInternal(playerId, achievementId);
            }

            Debug.Log($"Updated achievement progress for {playerId}: {achievementId} = {newProgress:F1}%");
        }

        public float GetAchievementProgress(string playerId, string achievementId)
        {
            if (!_playerAchievementProgress.ContainsKey(playerId))
                return 0f;

            return _playerAchievementProgress[playerId].GetValueOrDefault(achievementId, 0f);
        }

        public bool IsAchievementUnlocked(string playerId, string achievementId)
        {
            if (!_playerUnlockedAchievements.ContainsKey(playerId))
                return false;

            return _playerUnlockedAchievements[playerId].Contains(achievementId);
        }

        public List<Achievement> GetUnlockedAchievements(string playerId)
        {
            var unlockedAchievements = new List<Achievement>();

            if (!_playerUnlockedAchievements.ContainsKey(playerId))
                return unlockedAchievements;

            foreach (string achievementId in _playerUnlockedAchievements[playerId])
            {
                var achievement = _allAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
                if (achievement != null)
                {
                    unlockedAchievements.Add(achievement);
                }
            }

            return unlockedAchievements;
        }
        
        #endregion

        #region Milestone System
        
        public void RegisterMilestone(Milestone milestone)
        {
            if (milestone == null)
            {
                Debug.LogError("Cannot register null milestone");
                return;
            }

            var existingMilestone = _allMilestones.FirstOrDefault(m => m.MilestoneID == milestone.MilestoneID);
            if (existingMilestone != null)
            {
                Debug.LogWarning($"Milestone already registered: {milestone.MilestoneID}");
                return;
            }

            _allMilestones.Add(milestone);
            Debug.Log($"Registered milestone: {milestone.MilestoneID}");
        }

        public bool CheckMilestone(string playerId, string milestoneId)
        {
            var milestone = _allMilestones.FirstOrDefault(m => m.MilestoneID == milestoneId);
            if (milestone == null)
            {
                Debug.LogWarning($"Milestone not found: {milestoneId}");
                return false;
            }

            if (_playerCompletedMilestones.ContainsKey(playerId) && 
                _playerCompletedMilestones[playerId].Contains(milestoneId))
            {
                return true; // Already completed
            }

            // Check milestone completion logic
            bool isCompleted = EvaluateMilestoneCompletion(playerId, milestone);
            
            if (isCompleted)
            {
                CompleteMilestone(playerId, milestoneId);
            }

            return isCompleted;
        }

        public List<Milestone> GetAchievedMilestones(string playerId)
        {
            var achievedMilestones = new List<Milestone>();

            if (!_playerCompletedMilestones.ContainsKey(playerId))
                return achievedMilestones;

            foreach (string milestoneId in _playerCompletedMilestones[playerId])
            {
                var milestone = _allMilestones.FirstOrDefault(m => m.MilestoneID == milestoneId);
                if (milestone != null)
                {
                    achievedMilestones.Add(milestone);
                }
            }

            return achievedMilestones;
        }

        public MilestoneReward GetMilestoneReward(string milestoneId)
        {
            return _milestoneRewards.GetValueOrDefault(milestoneId, null);
        }
        
        #endregion

        #region Reward Distribution
        
        public void DistributeAchievementReward(string playerId, string achievementId)
        {
            var achievement = _allAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
            if (achievement == null)
            {
                Debug.LogError($"Achievement not found: {achievementId}");
                return;
            }

            if (!IsAchievementUnlocked(playerId, achievementId))
            {
                Debug.LogError($"Achievement not unlocked: {achievementId}");
                return;
            }

            // Create reward from achievement
            var reward = new ProgressionAchievementReward
            {
                RewardID = $"achievement_{achievementId}_{DateTime.Now.Ticks}",
                AchievementID = achievementId,
                RewardType = AchievementRewardType.Experience_Bonus,
                RewardValue = CalculateAchievementRewardValue(achievement),
                Description = $"Reward for completing {achievement.Name}",
                DateAwarded = DateTime.Now
            };

            // Add to pending rewards
            if (!_pendingRewards.ContainsKey(playerId))
            {
                _pendingRewards[playerId] = new List<ProgressionAchievementReward>();
            }

            _pendingRewards[playerId].Add(reward);

            OnRewardDistributed?.Invoke(playerId, reward);
            Debug.Log($"Distributed achievement reward to {playerId}: {reward.RewardValue} XP");
        }

        public List<ProgressionAchievementReward> GetPendingRewards(string playerId)
        {
            return _pendingRewards.GetValueOrDefault(playerId, new List<ProgressionAchievementReward>());
        }

        public void ClaimReward(string playerId, string rewardId)
        {
            if (!_pendingRewards.ContainsKey(playerId))
            {
                Debug.LogWarning($"No pending rewards for player: {playerId}");
                return;
            }

            var reward = _pendingRewards[playerId].FirstOrDefault(r => r.RewardID == rewardId);
            if (reward == null)
            {
                Debug.LogWarning($"Reward not found: {rewardId}");
                return;
            }

            _pendingRewards[playerId].Remove(reward);
            Debug.Log($"Player {playerId} claimed reward: {reward.Description}");
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeAchievementSystem()
        {
            if (_playerAchievementProgress == null)
                _playerAchievementProgress = new Dictionary<string, Dictionary<string, float>>();
            
            if (_playerUnlockedAchievements == null)
                _playerUnlockedAchievements = new Dictionary<string, List<string>>();
            
            if (_playerCompletedMilestones == null)
                _playerCompletedMilestones = new Dictionary<string, List<string>>();
            
            if (_pendingRewards == null)
                _pendingRewards = new Dictionary<string, List<ProgressionAchievementReward>>();
            
            Debug.Log("Achievement system initialized");
        }

        private void InitializeAchievements()
        {
            if (_allAchievements.Count == 0)
            {
                CreateDefaultAchievements();
            }

            Debug.Log($"Initialized {_allAchievements.Count} achievements");
        }

        private void CreateDefaultAchievements()
        {
            // Progression achievements
            _allAchievements.Add(new Achievement
            {
                AchievementID = "first_level",
                Name = "First Steps",
                Description = "Reach level 2",
                Type = AchievementType.Progression,
                Difficulty = AchievementDifficulty.Easy,
                PointValue = 50
            });

            _allAchievements.Add(new Achievement
            {
                AchievementID = "level_10",
                Name = "Getting Started",
                Description = "Reach level 10",
                Type = AchievementType.Progression,
                Difficulty = AchievementDifficulty.Medium,
                PointValue = 200
            });

            _allAchievements.Add(new Achievement
            {
                AchievementID = "skill_master",
                Name = "Skill Master",
                Description = "Unlock 10 skills",
                Type = AchievementType.Skills,
                Difficulty = AchievementDifficulty.Hard,
                PointValue = 500
            });
        }

        private void InitializeMilestones()
        {
            if (_allMilestones.Count == 0)
            {
                CreateDefaultMilestones();
            }

            InitializeMilestoneRewards();
            Debug.Log($"Initialized {_allMilestones.Count} milestones");
        }

        private void CreateDefaultMilestones()
        {
            _allMilestones.Add(new Milestone
            {
                MilestoneID = "tutorial_complete",
                Name = "Tutorial Master",
                Description = "Complete the tutorial",
                Type = MilestoneType.Tutorial,
                RequiredValue = 1,
                Category = "Learning"
            });

            _allMilestones.Add(new Milestone
            {
                MilestoneID = "first_harvest",
                Name = "First Harvest",
                Description = "Successfully harvest your first plant",
                Type = MilestoneType.Achievement,
                RequiredValue = 1,
                Category = "Cultivation"
            });
        }

        private void InitializeMilestoneRewards()
        {
            foreach (var milestone in _allMilestones)
            {
                _milestoneRewards[milestone.MilestoneID] = new MilestoneReward
                {
                    RewardID = $"milestone_{milestone.MilestoneID}",
                    MilestoneID = milestone.MilestoneID,
                    RewardType = AchievementRewardType.Experience_Bonus,
                    RewardValue = CalculateMilestoneRewardValue(milestone),
                    Description = $"Reward for {milestone.Name}"
                };
            }
        }

        private void InitializeRewards()
        {
            // Initialize available reward types
            _availableRewards.Clear();
            
            _availableRewards.Add(new CleanProgressionReward
            {
                RewardID = "experience",
                RewardName = "Experience Points",
                RewardType = AchievementRewardType.Experience_Bonus,
                Description = "Bonus experience for progression"
            });

            _availableRewards.Add(new CleanProgressionReward
            {
                RewardID = "skill_points",
                RewardName = "Skill Points",
                RewardType = CleanProgressionRewardType.Currency.ToString(),
                Description = "Points to spend on skills"
            });

            Debug.Log($"Initialized {_availableRewards.Count} reward types");
        }

        private void LoadExistingAchievementData()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing achievement data...");
        }

        private void SaveAchievementState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving achievement state...");
        }

        private void UnlockAchievementInternal(string playerId, string achievementId)
        {
            if (!_playerUnlockedAchievements.ContainsKey(playerId))
            {
                _playerUnlockedAchievements[playerId] = new List<string>();
            }

            if (_playerUnlockedAchievements[playerId].Contains(achievementId))
                return;

            _playerUnlockedAchievements[playerId].Add(achievementId);
            _totalAchievementsUnlocked++;

            if (!_achievementUnlockCounts.ContainsKey(achievementId))
            {
                _achievementUnlockCounts[achievementId] = 0;
            }
            _achievementUnlockCounts[achievementId]++;

            OnAchievementUnlocked?.Invoke(playerId, achievementId);
            
            // Automatically distribute reward
            DistributeAchievementReward(playerId, achievementId);

            var achievement = _allAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
            Debug.Log($"Player {playerId} unlocked achievement: {achievement?.Name ?? achievementId}");
        }

        private void CompleteMilestone(string playerId, string milestoneId)
        {
            if (!_playerCompletedMilestones.ContainsKey(playerId))
            {
                _playerCompletedMilestones[playerId] = new List<string>();
            }

            if (_playerCompletedMilestones[playerId].Contains(milestoneId))
                return;

            _playerCompletedMilestones[playerId].Add(milestoneId);
            _totalMilestonesCompleted++;

            OnMilestoneAchieved?.Invoke(playerId, milestoneId);

            var milestone = _allMilestones.FirstOrDefault(m => m.MilestoneID == milestoneId);
            Debug.Log($"Player {playerId} completed milestone: {milestone?.Name ?? milestoneId}");
        }

        private bool EvaluateMilestoneCompletion(string playerId, Milestone milestone)
        {
            // Simple evaluation logic - would be expanded based on milestone type
            switch (milestone.Type)
            {
                case MilestoneType.Tutorial:
                    // Check if tutorial is complete
                    return true; // Placeholder
                
                case MilestoneType.Achievement:
                    // Check if related achievement is unlocked
                    return IsAchievementUnlocked(playerId, milestone.MilestoneID);
                
                case MilestoneType.Level:
                    // Check player level - would need reference to experience service
                    return true; // Placeholder
                
                default:
                    return false;
            }
        }

        private float CalculateAchievementRewardValue(Achievement achievement)
        {
            return achievement.Difficulty switch
            {
                AchievementDifficulty.Easy => 100f,
                AchievementDifficulty.Medium => 250f,
                AchievementDifficulty.Hard => 500f,
                AchievementDifficulty.Expert => 1000f,
                AchievementDifficulty.Legendary => 2500f,
                _ => 100f
            };
        }

        private float CalculateMilestoneRewardValue(Milestone milestone)
        {
            return milestone.Type switch
            {
                MilestoneType.Tutorial => 50f,
                MilestoneType.Level => 200f,
                MilestoneType.Achievement => 300f,
                MilestoneType.System => 150f,
                _ => 100f
            };
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

        private void Update()
        {
            if (!IsInitialized || !_enableAchievements) return;

            // Periodic achievement checking
            if (Time.time - _lastAchievementCheck >= _achievementCheckInterval)
            {
                _lastAchievementCheck = Time.time;
                // Could add automatic progress checking here
            }
        }
        
        #endregion
    }
}