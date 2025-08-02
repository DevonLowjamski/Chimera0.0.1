using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Competition;
using ProjectChimera.Systems.Registry;
using ProjectChimera.Systems.Progression;
using PlacementPosition = ProjectChimera.Data.Competition.PlacementPosition;
using PrizeRarity = ProjectChimera.Data.Competition.PrizeRarity;
using Prize = ProjectChimera.Data.Competition.Prize;
using WinnerProfile = ProjectChimera.Data.Competition.WinnerProfile;
using RewardHistory = ProjectChimera.Data.Competition.RewardHistory;
using RewardStatistics = ProjectChimera.Data.Competition.RewardStatistics;
using CompetitionResults = ProjectChimera.Data.Competition.CompetitionResults;
using CompetitionType = ProjectChimera.Data.Competition.CompetitionType;
using PrizeStructure = ProjectChimera.Data.Competition.PrizeStructure;
using DistributedPrize = ProjectChimera.Data.Competition.DistributedPrize;
using PlantRanking = ProjectChimera.Data.Competition.PlantRanking;

namespace ProjectChimera.Systems.Services.Competition
{
    /// <summary>
    /// PC014-1d: Competition Rewards Service
    /// Manages prize distribution and achievement integration
    /// Decomposed from CannabisCupManager (473 lines target)
    /// </summary>
    public class CompetitionRewardsService : MonoBehaviour, ICompetitionRewardsService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Rewards Configuration")]
        [SerializeField] private bool _enableAutomaticDistribution = true;
        [SerializeField] private bool _enableAchievementIntegration = true;
        [SerializeField] private float _prizeMultiplier = 1.0f;
        [Range(1, 10)] [SerializeField] private int _hallOfFameSize = 100;
        
        [Header("Prize Pool")]
        [SerializeField] private List<Prize> _availablePrizes = new List<Prize>();
        [SerializeField] private Dictionary<CompetitionType, PrizeStructure> _prizeStructures = new Dictionary<CompetitionType, PrizeStructure>();
        [SerializeField] private List<ProjectChimera.Data.Achievements.Achievement> _competitionAchievements = new List<ProjectChimera.Data.Achievements.Achievement>();
        
        [Header("Distributed Rewards")]
        [SerializeField] private List<DistributedPrize> _distributedPrizes = new List<DistributedPrize>();
        [SerializeField] private List<WinnerProfile> _hallOfFame = new List<WinnerProfile>();
        [SerializeField] private Dictionary<string, List<RewardHistory>> _playerRewardHistory = new Dictionary<string, List<RewardHistory>>();
        [SerializeField] private Dictionary<string, RewardStatistics> _playerRewardStats = new Dictionary<string, RewardStatistics>();
        
        private IProgressionAchievementService _achievementService;
        
        #endregion

        #region Events
        
        public event Action<string, string> OnPrizeDistributed; // winnerId, prizeId
        public event Action<string, string> OnAchievementUnlocked; // playerId, achievementId
        public event Action<string> OnWinnerRecognized; // winnerId
        public event Action<string, float> OnPrizeValueAwarded; // winnerId, value
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing CompetitionRewardsService...");
            
            // Get achievement service reference
            _achievementService = ServiceRegistry.Instance.GetService<IProgressionAchievementService>();
            
            // Initialize prize structures
            InitializePrizeStructures();
            
            // Initialize competition achievements
            InitializeCompetitionAchievements();
            
            // Load existing rewards data
            LoadExistingRewardsData();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<ICompetitionRewardsService>(this, ServiceDomain.Competition);
            
            IsInitialized = true;
            Debug.Log("CompetitionRewardsService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down CompetitionRewardsService...");
            
            // Save rewards state
            SaveRewardsState();
            
            // Clear collections
            _availablePrizes.Clear();
            _prizeStructures.Clear();
            _competitionAchievements.Clear();
            _distributedPrizes.Clear();
            _hallOfFame.Clear();
            _playerRewardHistory.Clear();
            _playerRewardStats.Clear();
            
            _achievementService = null;
            
            IsInitialized = false;
            Debug.Log("CompetitionRewardsService shutdown complete");
        }
        
        #endregion

        #region Prize System
        
        public void DistributePrizes(string competitionId, CompetitionResults results)
        {
            if (!IsInitialized)
            {
                Debug.LogError("CompetitionRewardsService not initialized");
                return;
            }

            if (results == null || !results.Rankings.Any())
            {
                Debug.LogWarning($"No results to distribute prizes for competition {competitionId}");
                return;
            }

            var competitionType = GetCompetitionType(competitionId);
            if (!_prizeStructures.ContainsKey(competitionType))
            {
                Debug.LogError($"No prize structure defined for competition type {competitionType}");
                return;
            }

            var prizeStructure = _prizeStructures[competitionType];
            
            // Distribute placement prizes
            DistributePlacementPrizes(competitionId, results, prizeStructure);
            
            // Distribute category prizes
            DistributeCategoryPrizes(competitionId, results, prizeStructure);
            
            // Process special awards
            ProcessSpecialAwards(competitionId, results);
            
            Debug.Log($"Prize distribution completed for competition {competitionId}");
        }

        public Prize GetPrize(PlacementPosition position, CompetitionType type)
        {
            if (!_prizeStructures.ContainsKey(type))
                return null;

            var prizeStructure = _prizeStructures[type];
            
            return position switch
            {
                PlacementPosition.First => prizeStructure.FirstPlacePrize,
                PlacementPosition.Second => prizeStructure.SecondPlacePrize,
                PlacementPosition.Third => prizeStructure.ThirdPlacePrize,
                PlacementPosition.HonorableMention => prizeStructure.HonorableMentionPrize,
                _ => null
            };
        }

        public List<Prize> GetAvailablePrizes(string competitionId)
        {
            var competitionType = GetCompetitionType(competitionId);
            if (!_prizeStructures.ContainsKey(competitionType))
                return new List<Prize>();

            var prizeStructure = _prizeStructures[competitionType];
            var prizes = new List<Prize>();
            
            if (prizeStructure.FirstPlacePrize != null)
                prizes.Add(prizeStructure.FirstPlacePrize);
            if (prizeStructure.SecondPlacePrize != null)
                prizes.Add(prizeStructure.SecondPlacePrize);
            if (prizeStructure.ThirdPlacePrize != null)
                prizes.Add(prizeStructure.ThirdPlacePrize);
            if (prizeStructure.HonorableMentionPrize != null)
                prizes.Add(prizeStructure.HonorableMentionPrize);

            return prizes;
        }

        public bool ClaimPrize(string winnerId, string prizeId)
        {
            var distributedPrize = _distributedPrizes.FirstOrDefault(p => p.PrizeId == prizeId && p.WinnerId == winnerId);
            if (distributedPrize == null)
            {
                Debug.LogError($"Prize {prizeId} not found for winner {winnerId}");
                return false;
            }

            if (distributedPrize.IsClaimed)
            {
                Debug.LogWarning($"Prize {prizeId} already claimed by {winnerId}");
                return true;
            }

            distributedPrize.IsClaimed = true;
            distributedPrize.ClaimDate = DateTime.Now;
            
            // Award the prize to the player
            AwardPrizeToPlayer(winnerId, distributedPrize.Prize);
            
            Debug.Log($"Prize {prizeId} claimed by winner {winnerId}");
            return true;
        }
        
        #endregion

        #region Achievement Integration
        
        public void ProcessCompetitionAchievements(string competitionId, CompetitionResults results)
        {
            if (!_enableAchievementIntegration || _achievementService == null)
                return;

            foreach (var ranking in results.Rankings)
            {
                ProcessPlayerAchievements(ranking.PlantId, competitionId, ranking);
            }
            
            // Process special achievements
            ProcessSpecialCompetitionAchievements(competitionId, results);
        }

        public List<ProjectChimera.Data.Achievements.Achievement> GetCompetitionAchievements()
        {
            return new List<ProjectChimera.Data.Achievements.Achievement>(_competitionAchievements);
        }

        public bool UnlockAchievement(string playerId, string achievementId)
        {
            if (_achievementService == null)
            {
                Debug.LogError("Achievement service not available");
                return false;
            }

            var achievement = _competitionAchievements.FirstOrDefault(a => a.AchievementID == achievementId);
            if (achievement == null)
            {
                Debug.LogError($"Competition achievement not found: {achievementId}");
                return false;
            }

            // Use achievement service to unlock
            _achievementService.TrackAchievementProgress(playerId, $"competition_achievement_{achievementId}", 1f);
            
            OnAchievementUnlocked?.Invoke(playerId, achievementId);
            Debug.Log($"Achievement {achievementId} unlocked for player {playerId}");
            
            return true;
        }
        
        #endregion

        #region Winner Recognition
        
        public void RecognizeWinner(string winnerId, string competitionId, PlacementPosition position)
        {
            var winnerProfile = CreateWinnerProfile(winnerId, competitionId, position);
            
            // Add to hall of fame
            AddToHallOfFame(winnerProfile);
            
            // Update player statistics
            UpdatePlayerStatistics(winnerId, position);
            
            // Process recognition achievements
            ProcessRecognitionAchievements(winnerId, position);
            
            OnWinnerRecognized?.Invoke(winnerId);
            Debug.Log($"Recognized winner {winnerId} for {position} place in competition {competitionId}");
        }

        public WinnerProfile CreateWinnerProfile(string winnerId, CompetitionResults results)
        {
            var topRanking = results.Rankings.FirstOrDefault(r => r.PlantId == winnerId);
            if (topRanking == null)
                return null;

            return new WinnerProfile
            {
                WinnerId = winnerId,
                CompetitionId = results.CompetitionId,
                PlacementPosition = DeterminePlacementFromRank(topRanking.Rank),
                Score = topRanking.Score,
                WinDate = results.CalculationDate,
                CompetitionName = GetCompetitionName(results.CompetitionId),
                Category = GetWinningCategory(winnerId, results)
            };
        }

        public List<WinnerProfile> GetHallOfFame()
        {
            return _hallOfFame.OrderByDescending(w => w.WinDate).Take(_hallOfFameSize).ToList();
        }
        
        #endregion

        #region Reward History
        
        public List<RewardHistory> GetPlayerRewards(string playerId)
        {
            if (_playerRewardHistory.ContainsKey(playerId))
            {
                return new List<RewardHistory>(_playerRewardHistory[playerId]);
            }
            
            return new List<RewardHistory>();
        }

        public RewardStatistics GetRewardStatistics(string playerId)
        {
            if (_playerRewardStats.ContainsKey(playerId))
            {
                return _playerRewardStats[playerId];
            }
            
            return new RewardStatistics
            {
                PlayerId = playerId,
                TotalPrizesWon = 0,
                TotalValueAwarded = 0f,
                AchievementsUnlocked = 0,
                FirstPlaceWins = 0,
                SecondPlaceWins = 0,
                ThirdPlaceWins = 0,
                CompetitionsParticipated = 0
            };
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializePrizeStructures()
        {
            // Championship competition prizes
            _prizeStructures[CompetitionType.Championship] = new PrizeStructure
            {
                FirstPlacePrize = new Prize
                {
                    PrizeId = "championship_first",
                    Name = "Championship Cup Winner",
                    Description = "Grand Championship Trophy and Prize Package",
                    MonetaryValue = 10000f,
                    Items = new List<string> { "Golden Trophy", "Cash Prize", "Exclusive Seeds" },
                    Rarity = PrizeRarity.Legendary
                },
                SecondPlacePrize = new Prize
                {
                    PrizeId = "championship_second",
                    Name = "Championship Runner-up",
                    Description = "Silver Championship Trophy and Prize Package",
                    MonetaryValue = 5000f,
                    Items = new List<string> { "Silver Trophy", "Cash Prize", "Premium Seeds" },
                    Rarity = PrizeRarity.Epic
                },
                ThirdPlacePrize = new Prize
                {
                    PrizeId = "championship_third",
                    Name = "Championship Third Place",
                    Description = "Bronze Championship Trophy and Prize Package",
                    MonetaryValue = 2500f,
                    Items = new List<string> { "Bronze Trophy", "Cash Prize", "Quality Seeds" },
                    Rarity = PrizeRarity.Rare
                }
            };
            
            // Regional competition prizes
            _prizeStructures[CompetitionType.Regional] = new PrizeStructure
            {
                FirstPlacePrize = new Prize
                {
                    PrizeId = "regional_first",
                    Name = "Regional Champion",
                    Description = "Regional Championship Trophy",
                    MonetaryValue = 2500f,
                    Items = new List<string> { "Regional Trophy", "Cash Prize" },
                    Rarity = PrizeRarity.Epic
                },
                SecondPlacePrize = new Prize
                {
                    PrizeId = "regional_second",
                    Name = "Regional Runner-up",
                    Description = "Regional Silver Trophy",
                    MonetaryValue = 1000f,
                    Items = new List<string> { "Silver Trophy", "Cash Prize" },
                    Rarity = PrizeRarity.Rare
                },
                ThirdPlacePrize = new Prize
                {
                    PrizeId = "regional_third",
                    Name = "Regional Third Place",
                    Description = "Regional Bronze Trophy",
                    MonetaryValue = 500f,
                    Items = new List<string> { "Bronze Trophy", "Seeds Package" },
                    Rarity = PrizeRarity.Uncommon
                }
            };
        }

        private void InitializeCompetitionAchievements()
        {
            _competitionAchievements.Add(new ProjectChimera.Data.Achievements.Achievement
            {
                AchievementID = "first_competition",
                AchievementName = "First Timer",
                Description = "Participate in your first competition",
                Category = ProjectChimera.Data.Achievements.AchievementCategory.Competition,
                Rarity = ProjectChimera.Data.Achievements.AchievementRarity.Common,
                Points = 50f,
                TriggerEvent = "competition_participation"
            });
            
            _competitionAchievements.Add(new ProjectChimera.Data.Achievements.Achievement
            {
                AchievementID = "competition_winner",
                AchievementName = "Champion",
                Description = "Win first place in any competition",
                Category = ProjectChimera.Data.Achievements.AchievementCategory.Competition,
                Rarity = ProjectChimera.Data.Achievements.AchievementRarity.Epic,
                Points = 500f,
                TriggerEvent = "first_place_win"
            });
        }

        private void LoadExistingRewardsData()
        {
            Debug.Log("Loading existing rewards data...");
        }

        private void SaveRewardsState()
        {
            Debug.Log("Saving rewards state...");
        }

        private CompetitionType GetCompetitionType(string competitionId)
        {
            // TODO: Get actual competition type from CompetitionManagementService
            return CompetitionType.Regional;
        }

        private void DistributePlacementPrizes(string competitionId, CompetitionResults results, PrizeStructure prizeStructure)
        {
            var rankings = results.Rankings.OrderBy(r => r.Rank).ToList();
            
            for (int i = 0; i < Math.Min(3, rankings.Count); i++)
            {
                var ranking = rankings[i];
                Prize prize = null;
                
                switch (i)
                {
                    case 0:
                        prize = prizeStructure.FirstPlacePrize;
                        RecognizeWinner(ranking.PlantId, competitionId, PlacementPosition.First);
                        break;
                    case 1:
                        prize = prizeStructure.SecondPlacePrize;
                        RecognizeWinner(ranking.PlantId, competitionId, PlacementPosition.Second);
                        break;
                    case 2:
                        prize = prizeStructure.ThirdPlacePrize;
                        RecognizeWinner(ranking.PlantId, competitionId, PlacementPosition.Third);
                        break;
                }
                
                if (prize != null)
                {
                    DistributePrize(ranking.PlantId, prize, competitionId);
                }
            }
        }

        private void DistributeCategoryPrizes(string competitionId, CompetitionResults results, PrizeStructure prizeStructure)
        {
            foreach (var categoryWinner in results.CategoryWinners)
            {
                var categoryPrize = CreateCategoryPrize(categoryWinner.Key);
                if (categoryPrize != null)
                {
                    DistributePrize(categoryWinner.Value, categoryPrize, competitionId);
                }
            }
        }

        private void ProcessSpecialAwards(string competitionId, CompetitionResults results)
        {
            // Process special awards like "Best Newcomer", "People's Choice", etc.
            // This would integrate with additional judging criteria
        }

        private void DistributePrize(string winnerId, Prize prize, string competitionId)
        {
            var distributedPrize = new DistributedPrize
            {
                PrizeId = prize.PrizeId,
                WinnerId = winnerId,
                CompetitionId = competitionId,
                Prize = prize,
                DistributionDate = DateTime.Now,
                IsClaimed = false
            };
            
            _distributedPrizes.Add(distributedPrize);
            
            // Add to player reward history
            AddToPlayerRewardHistory(winnerId, distributedPrize);
            
            OnPrizeDistributed?.Invoke(winnerId, prize.PrizeId);
            OnPrizeValueAwarded?.Invoke(winnerId, prize.MonetaryValue * _prizeMultiplier);
            
            Debug.Log($"Distributed prize {prize.Name} to winner {winnerId}");
        }

        private void AwardPrizeToPlayer(string playerId, Prize prize)
        {
            // TODO: Integrate with player inventory/currency system
            Debug.Log($"Awarded prize to player {playerId}: {prize.Name} (Value: ${prize.MonetaryValue})");
        }

        private void ProcessPlayerAchievements(string plantId, string competitionId, PlantRanking ranking)
        {
            // Process rank-based achievements
            switch (ranking.Rank)
            {
                case 1:
                    UnlockAchievement(plantId, "competition_winner");
                    break;
                case 2:
                case 3:
                    UnlockAchievement(plantId, "competition_podium");
                    break;
            }
            
            // Process participation achievement
            UnlockAchievement(plantId, "first_competition");
        }

        private void ProcessSpecialCompetitionAchievements(string competitionId, CompetitionResults results)
        {
            // Process achievements for special circumstances
            // e.g., perfect scores, close competitions, etc.
        }

        private WinnerProfile CreateWinnerProfile(string winnerId, string competitionId, PlacementPosition position)
        {
            return new WinnerProfile
            {
                WinnerId = winnerId,
                CompetitionId = competitionId,
                PlacementPosition = position,
                WinDate = DateTime.Now,
                CompetitionName = GetCompetitionName(competitionId),
                Category = "Overall" // Default category
            };
        }

        private void AddToHallOfFame(WinnerProfile winnerProfile)
        {
            _hallOfFame.Add(winnerProfile);
            
            // Keep hall of fame size manageable
            if (_hallOfFame.Count > _hallOfFameSize)
            {
                _hallOfFame = _hallOfFame.OrderByDescending(w => w.WinDate).Take(_hallOfFameSize).ToList();
            }
        }

        private void UpdatePlayerStatistics(string winnerId, PlacementPosition position)
        {
            if (!_playerRewardStats.ContainsKey(winnerId))
            {
                _playerRewardStats[winnerId] = new RewardStatistics { PlayerId = winnerId };
            }

            var stats = _playerRewardStats[winnerId];
            stats.CompetitionsParticipated++;
            
            switch (position)
            {
                case PlacementPosition.First:
                    stats.FirstPlaceWins++;
                    break;
                case PlacementPosition.Second:
                    stats.SecondPlaceWins++;
                    break;
                case PlacementPosition.Third:
                    stats.ThirdPlaceWins++;
                    break;
            }
        }

        private void ProcessRecognitionAchievements(string winnerId, PlacementPosition position)
        {
            // Process achievements based on winning position
            if (position == PlacementPosition.First)
            {
                UnlockAchievement(winnerId, "competition_winner");
            }
        }

        private void AddToPlayerRewardHistory(string playerId, DistributedPrize distributedPrize)
        {
            if (!_playerRewardHistory.ContainsKey(playerId))
            {
                _playerRewardHistory[playerId] = new List<RewardHistory>();
            }

            var rewardHistory = new RewardHistory
            {
                PlayerId = playerId,
                PrizeName = distributedPrize.Prize.Name,
                PrizeValue = distributedPrize.Prize.MonetaryValue,
                CompetitionId = distributedPrize.CompetitionId,
                AwardDate = distributedPrize.DistributionDate
            };
            
            _playerRewardHistory[playerId].Add(rewardHistory);
            
            // Update statistics
            if (!_playerRewardStats.ContainsKey(playerId))
            {
                _playerRewardStats[playerId] = new RewardStatistics { PlayerId = playerId };
            }
            
            var stats = _playerRewardStats[playerId];
            stats.TotalPrizesWon++;
            stats.TotalValueAwarded += distributedPrize.Prize.MonetaryValue;
        }

        private Prize CreateCategoryPrize(string categoryId)
        {
            return new Prize
            {
                PrizeId = $"category_{categoryId}",
                Name = $"Best {categoryId}",
                Description = $"Category winner for {categoryId}",
                MonetaryValue = 500f,
                Items = new List<string> { "Category Trophy" },
                Rarity = PrizeRarity.Rare
            };
        }

        private PlacementPosition DeterminePlacementFromRank(int rank)
        {
            return rank switch
            {
                1 => PlacementPosition.First,
                2 => PlacementPosition.Second,
                3 => PlacementPosition.Third,
                _ => PlacementPosition.HonorableMention
            };
        }

        private string GetCompetitionName(string competitionId)
        {
            // TODO: Get actual competition name from CompetitionManagementService
            return $"Competition {competitionId}";
        }

        private string GetWinningCategory(string winnerId, CompetitionResults results)
        {
            var categoryWinner = results.CategoryWinners.FirstOrDefault(kvp => kvp.Value == winnerId);
            return categoryWinner.Key ?? "Overall";
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

    // Data structures moved to ProjectChimera.Data.Competition.CompetitionStructures
}