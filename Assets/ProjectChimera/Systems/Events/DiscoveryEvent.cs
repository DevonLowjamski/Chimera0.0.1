using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Systems.Events
{
    using ILiveEventDefinition = ProjectChimera.Core.Events.ILiveEventDefinition;
    using RarityTier = ProjectChimera.Data.Events.RarityTier;

    /// <summary>
    /// Implementation of discovery events with rarity tiers and scientific validation features.
    /// </summary>
    public class DiscoveryEvent : LiveEventBase, IEducationalEvent, IRewardableEvent
    {
        // IEducationalEvent Implementation
        public List<LearningObjective> LearningObjectives { get; private set; }
        public EducationalContent EducationalMaterials { get; private set; }
        public bool RequiresScientificAccuracy { get; private set; }
        public bool IsScientificallyValidated { get; private set; }

        // IRewardableEvent Implementation
        public RewardStructure RewardStructure { get; private set; }

        // Discovery event specific properties
        public RarityTier RarityTier { get; private set; }
        public string DiscoveryCategory { get; private set; }
        public bool IsResearchValidated { get; private set; }
        
        private Dictionary<string, EducationalProgress> _learningProgress;
        private Dictionary<string, DiscoveryProgress> _discoveryProgress;
        private DiscoveryEventSO _discoveryEventData;
        private List<DiscoveryReward> _rarityBasedRewards;

        public DiscoveryEvent(ILiveEventDefinition definition) : base(definition)
        {
            // Initialize discovery context
            if (definition is DiscoveryEventSO discoverySO)
            {
                _discoveryEventData = discoverySO;
                RarityTier = discoverySO.RarityTier;
                DiscoveryCategory = discoverySO.DiscoveryType.ToString();
                IsResearchValidated = discoverySO.IsScientificallyValidated;
            }
            else
            {
                RarityTier = RarityTier.Common;
                DiscoveryCategory = "General";
                IsResearchValidated = false;
            }

            InitializeEducationalContent();
            InitializeRewardStructure();
            
            _learningProgress = new Dictionary<string, EducationalProgress>();
            _discoveryProgress = new Dictionary<string, DiscoveryProgress>();
            _rarityBasedRewards = new List<DiscoveryReward>();
        }

        private void InitializeEducationalContent()
        {
            LearningObjectives = new List<LearningObjective>
            {
                new LearningObjective
                {
                    ObjectiveId = $"discovery_research_{RarityTier}",
                    Description = $"Research and validate {RarityTier} tier discovery",
                    CompletionThreshold = GetRarityCompletionThreshold(),
                    IsRequired = true
                },
                new LearningObjective
                {
                    ObjectiveId = $"discovery_application_{DiscoveryCategory}",
                    Description = $"Apply discovery to {DiscoveryCategory} cultivation",
                    CompletionThreshold = 0.7f,
                    IsRequired = false
                }
            };

            RequiresScientificAccuracy = true;
            IsScientificallyValidated = IsResearchValidated;

            EducationalMaterials = new EducationalContent
            {
                ContentId = $"discovery_education_{EventId}",
                Title = $"{RarityTier} Discovery: {EventName}",
                Description = $"Educational content about {EventName} discovery in {DiscoveryCategory}",
                LearningObjectives = LearningObjectives,
                ResourceLinks = new List<string>(),
                QualityScore = GetRarityQualityScore(),
                IsScientificallyValidated = IsResearchValidated
            };
        }

        private float GetRarityCompletionThreshold()
        {
            return RarityTier switch
            {
                RarityTier.Common => 0.6f,
                RarityTier.Uncommon => 0.7f,
                RarityTier.Rare => 0.8f,
                RarityTier.Epic => 0.85f,
                RarityTier.Legendary => 0.9f,
                RarityTier.Mythic => 0.95f,
                _ => 0.6f
            };
        }

        private float GetRarityQualityScore()
        {
            return RarityTier switch
            {
                RarityTier.Common => 1.0f,
                RarityTier.Uncommon => 1.2f,
                RarityTier.Rare => 1.5f,
                RarityTier.Epic => 1.8f,
                RarityTier.Legendary => 2.0f,
                RarityTier.Mythic => 2.5f,
                _ => 1.0f
            };
        }

        private void InitializeRewardStructure()
        {
            var baseRewards = new List<EventReward>
            {
                new EventReward
                {
                    RewardId = $"discovery_badge_{RarityTier}",
                    RewardType = "Discovery Badge",
                    RewardData = $"{RarityTier} Discovery Badge",
                    IsExclusive = RarityTier >= RarityTier.Epic,
                    ExpirationDate = EndTime.AddDays(GetRarityExpirationDays())
                }
            };

            // Add rarity-specific rewards
            if (RarityTier >= RarityTier.Rare)
            {
                baseRewards.Add(new EventReward
                {
                    RewardId = $"research_credit_{RarityTier}",
                    RewardType = "Research Credit",
                    RewardData = GetRarityResearchCredits(),
                    IsExclusive = false,
                    ExpirationDate = EndTime.AddDays(90)
                });
            }

            if (RarityTier >= RarityTier.Legendary)
            {
                baseRewards.Add(new EventReward
                {
                    RewardId = $"exclusive_access_{EventId}",
                    RewardType = "Exclusive Access",
                    RewardData = "Access to advanced research materials",
                    IsExclusive = true,
                    ExpirationDate = EndTime.AddDays(365)
                });
            }

            RewardStructure = new RewardStructure
            {
                RewardStructureId = $"discovery_rewards_{EventId}",
                Rewards = baseRewards,
                DistributionType = RewardDistributionType.Achievement
            };
        }

        private int GetRarityExpirationDays()
        {
            return RarityTier switch
            {
                RarityTier.Common => 30,
                RarityTier.Uncommon => 45,
                RarityTier.Rare => 60,
                RarityTier.Epic => 90,
                RarityTier.Legendary => 180,
                RarityTier.Mythic => 365,
                _ => 30
            };
        }

        private int GetRarityResearchCredits()
        {
            return RarityTier switch
            {
                RarityTier.Rare => 100,
                RarityTier.Epic => 250,
                RarityTier.Legendary => 500,
                RarityTier.Mythic => 1000,
                _ => 50
            };
        }

        // IEducationalEvent Implementation
        public EducationalProgress TrackLearningProgress(PlayerProfile player)
        {
            if (!_learningProgress.ContainsKey(player.PlayerId))
            {
                _learningProgress[player.PlayerId] = new EducationalProgress
                {
                    PlayerId = player.PlayerId,
                    EventId = EventId,
                    StartTime = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    ObjectiveProgress = new Dictionary<string, float>(),
                    OverallProgress = 0f
                };
            }

            return _learningProgress[player.PlayerId];
        }

        public void ValidateEducationalContent()
        {
            if (RequiresScientificAccuracy && !IsScientificallyValidated)
            {
                Debug.LogWarning($"[DiscoveryEvent] Educational content for {EventName} requires scientific validation");
            }
        }

        // IRewardableEvent Implementation
        public List<EventReward> GetAvailableRewards(PlayerProfile player)
        {
            var rewards = new List<EventReward>(RewardStructure.Rewards);
            
            // Add discovery progress based rewards
            var discoveryProgress = GetDiscoveryProgress(player);
            if (discoveryProgress.ValidationScore >= GetRarityCompletionThreshold())
            {
                rewards.Add(new EventReward
                {
                    RewardId = $"validation_bonus_{RarityTier}_{player.PlayerId}",
                    RewardType = "Validation Bonus",
                    RewardData = $"{RarityTier} validation achievement",
                    IsExclusive = true,
                    ExpirationDate = EndTime.AddDays(30)
                });
            }

            return rewards;
        }

        public void DistributeRewards()
        {
            foreach (var participant in GetParticipants())
            {
                var availableRewards = GetAvailableRewards(participant);
                foreach (var reward in availableRewards)
                {
                    GrantReward(participant, reward);
                }
            }
        }

        public bool HasReceivedReward(PlayerProfile player, string rewardId)
        {
            // Check if player has already received this reward
            return false; // Placeholder implementation
        }

        public void GrantReward(PlayerProfile player, EventReward reward)
        {
            Debug.Log($"[DiscoveryEvent] Granting {RarityTier} reward {reward.RewardId} to player {player.PlayerId}");
        }

        // Discovery event specific methods
        public DiscoveryProgress GetDiscoveryProgress(PlayerProfile player)
        {
            if (!_discoveryProgress.ContainsKey(player.PlayerId))
            {
                _discoveryProgress[player.PlayerId] = new DiscoveryProgress
                {
                    PlayerId = player.PlayerId,
                    EventId = EventId,
                    RarityTier = RarityTier,
                    DiscoveryCategory = DiscoveryCategory,
                    ResearchStartTime = DateTime.Now,
                    ValidationScore = 0f,
                    IsValidated = false
                };
            }

            return _discoveryProgress[player.PlayerId];
        }

        public void ValidateDiscovery(PlayerProfile player, float validationScore)
        {
            var progress = GetDiscoveryProgress(player);
            progress.ValidationScore = validationScore;
            progress.IsValidated = validationScore >= GetRarityCompletionThreshold();
            progress.ValidationTime = DateTime.Now;

            if (progress.IsValidated)
            {
                Debug.Log($"[DiscoveryEvent] Player {player.PlayerId} validated {RarityTier} discovery with score {validationScore}");
            }
        }

        // Override base class methods for discovery event specific behavior
        protected override void OnEventStartedInternal()
        {
            base.OnEventStartedInternal();
            ValidateEducationalContent();
            Debug.Log($"[DiscoveryEvent] {RarityTier} discovery event {EventName} has started!");
        }

        protected override void OnEventEndedInternal(EventResult result)
        {
            base.OnEventEndedInternal(result);
            DistributeRewards();
            Debug.Log($"[DiscoveryEvent] {RarityTier} discovery event {EventName} has ended!");
        }

        protected override void OnPlayerActionInternal(PlayerAction action)
        {
            base.OnPlayerActionInternal(action);
            
            // Track discovery progress for research actions
            if (action.ActionType == "Learn" || action.ActionType == "Submit")
            {
                var player = GetParticipants().FirstOrDefault(p => p.PlayerId == action.PlayerId);
                if (player != null)
                {
                    var progress = TrackLearningProgress(player);
                    var discoveryProgress = GetDiscoveryProgress(player);
                    
                    // Update progress based on action type and rarity
                    float progressIncrement = GetRarityProgressIncrement(action.ActionType);
                    progress.OverallProgress = Math.Min(1.0f, progress.OverallProgress + progressIncrement);
                    
                    // Validate discovery if threshold met
                    if (progress.OverallProgress >= GetRarityCompletionThreshold())
                    {
                        ValidateDiscovery(player, progress.OverallProgress);
                    }
                }
            }
        }

        private float GetRarityProgressIncrement(string actionType)
        {
            float baseIncrement = actionType switch
            {
                "Learn" => 0.1f,
                "Submit" => 0.2f,
                "Share" => 0.05f,
                _ => 0.02f
            };

            // Adjust increment based on rarity (higher rarity = slower progress)
            float rarityMultiplier = RarityTier switch
            {
                RarityTier.Common => 1.0f,
                RarityTier.Uncommon => 0.9f,
                RarityTier.Rare => 0.8f,
                RarityTier.Epic => 0.7f,
                RarityTier.Legendary => 0.6f,
                RarityTier.Mythic => 0.5f,
                _ => 1.0f
            };

            return baseIncrement * rarityMultiplier;
        }

        protected override bool ValidateParticipationRequirements(PlayerProfile player)
        {
            if (!base.ValidateParticipationRequirements(player))
                return false;

            // Additional discovery validation for higher rarity tiers
            if (RarityTier >= RarityTier.Epic)
            {
                // Check if player has prerequisite discoveries
                // This is a placeholder - would check player's discovery history
                Debug.Log($"[DiscoveryEvent] Validating prerequisites for {RarityTier} discovery");
            }

            return true;
        }
    }

    // Discovery event specific data structures
    [Serializable]
    public class DiscoveryProgress
    {
        public string PlayerId;
        public string EventId;
        public RarityTier RarityTier;
        public string DiscoveryCategory;
        public DateTime ResearchStartTime;
        public DateTime ValidationTime;
        public DateTime StartTime;
        public DateTime LastUpdate;
        public float ResearchProgress;
        public float ValidationProgress;
        public float ValidationScore;
        public Dictionary<string, float> CategoryProgress = new Dictionary<string, float>();
        public List<string> CompletedMilestones = new List<string>();
        public bool IsValidated;
        
        public float GetOverallProgress()
        {
            return (ResearchProgress + ValidationProgress) / 2f;
        }
        
        public void UpdateProgress(string category, float progress)
        {
            CategoryProgress[category] = Mathf.Clamp01(progress);
            LastUpdate = DateTime.Now;
            
            // Update overall progress
            if (CategoryProgress.Values.Any())
            {
                ResearchProgress = CategoryProgress.Values.Average();
            }
        }
    }

    [Serializable]
    public class DiscoveryReward : EventReward
    {
        public RarityTier RequiredRarity;
        public string DiscoveryCategory;
    }

}