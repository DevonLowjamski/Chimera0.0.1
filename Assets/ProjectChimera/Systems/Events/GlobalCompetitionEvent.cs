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

    /// <summary>
    /// Implementation of global competition events with leaderboards and judging systems.
    /// </summary>
    public class GlobalCompetitionEvent : LiveEventBase, IEducationalEvent, IRewardableEvent
    {
        // IEducationalEvent Implementation
        public List<LearningObjective> LearningObjectives { get; private set; }
        public EducationalContent EducationalMaterials { get; private set; }
        public bool RequiresScientificAccuracy { get; private set; }
        public bool IsScientificallyValidated { get; private set; }

        // IRewardableEvent Implementation
        public RewardStructure RewardStructure { get; private set; }

        // Competition specific properties
        public CompetitionPhase CurrentPhase { get; private set; }
        public Dictionary<string, float> Leaderboard { get; private set; }
        public List<JudgingCriterion> JudgingCriteria { get; private set; }

        private GlobalCompetitionEventSO _competitionEventData;
        private Dictionary<string, CompetitionEntry> _entries;
        private Dictionary<string, EducationalProgress> _learningProgress;

        public GlobalCompetitionEvent(ILiveEventDefinition definition) : base(definition)
        {
            // Initialize competition context
            if (definition is GlobalCompetitionEventSO competitionSO)
            {
                _competitionEventData = competitionSO;
                InitializeCompetitionContext();
            }

            InitializeEducationalContent();
            InitializeRewardStructure();
            
            _entries = new Dictionary<string, CompetitionEntry>();
            _learningProgress = new Dictionary<string, EducationalProgress>();
            Leaderboard = new Dictionary<string, float>();
            JudgingCriteria = new List<JudgingCriterion>();
        }

        private void InitializeCompetitionContext()
        {
            if (_competitionEventData != null)
            {
                CurrentPhase = CompetitionPhase.Registration; // Default initial phase
                
                // Initialize judging criteria with defaults
                JudgingCriteria = new List<JudgingCriterion>
                {
                    new JudgingCriterion { CriterionId = "quality", Name = "Quality", Weight = 0.4f },
                    new JudgingCriterion { CriterionId = "innovation", Name = "Innovation", Weight = 0.3f },
                    new JudgingCriterion { CriterionId = "sustainability", Name = "Sustainability", Weight = 0.3f }
                };
            }
            else
            {
                // Default competition context
                CurrentPhase = CompetitionPhase.Registration;
                JudgingCriteria = new List<JudgingCriterion>
                {
                    new JudgingCriterion
                    {
                        CriterionId = "quality",
                        Name = "Quality",
                        Weight = 0.4f,
                        Description = "Overall quality of submission"
                    },
                    new JudgingCriterion
                    {
                        CriterionId = "innovation",
                        Name = "Innovation",
                        Weight = 0.3f,
                        Description = "Innovative approach and creativity"
                    },
                    new JudgingCriterion
                    {
                        CriterionId = "technical",
                        Name = "Technical Merit",
                        Weight = 0.3f,
                        Description = "Technical execution and expertise"
                    }
                };
            }
        }

        private void InitializeEducationalContent()
        {
            LearningObjectives = new List<LearningObjective>();
            RequiresScientificAccuracy = true;
            IsScientificallyValidated = true;

            // Create educational content about competition
            EducationalMaterials = new EducationalContent
            {
                ContentId = $"competition_education_{EventId}",
                Title = $"Learn About {EventName} Competition",
                Description = $"Educational content about competitive cultivation and judging criteria",
                LearningObjectives = LearningObjectives,
                ResourceLinks = new List<string>(),
                QualityScore = 1.0f,
                IsScientificallyValidated = true
            };
        }

        private void InitializeRewardStructure()
        {
            RewardStructure = new RewardStructure
            {
                RewardStructureId = $"competition_rewards_{EventId}",
                Rewards = new List<EventReward>(),
                DistributionType = RewardDistributionType.Leaderboard
            };
        }

        // Competition-specific methods
        public void SubmitEntry(PlayerProfile player, CompetitionEntry entry)
        {
            if (CurrentPhase != CompetitionPhase.Submission)
            {
                Debug.LogWarning($"[GlobalCompetitionEvent] Cannot submit entry - competition is in {CurrentPhase} phase");
                return;
            }

            entry.PlayerId = player.PlayerId;
            entry.SubmissionTime = DateTime.Now;
            _entries[player.PlayerId] = entry;

            Debug.Log($"[GlobalCompetitionEvent] Entry submitted by player {player.PlayerId}");
        }

        public void AdvancePhase()
        {
            switch (CurrentPhase)
            {
                case CompetitionPhase.Registration:
                    CurrentPhase = CompetitionPhase.Submission;
                    break;
                case CompetitionPhase.Submission:
                    CurrentPhase = CompetitionPhase.Judging;
                    StartJudging();
                    break;
                case CompetitionPhase.Judging:
                    CurrentPhase = CompetitionPhase.Results;
                    FinalizeResults();
                    break;
                case CompetitionPhase.Results:
                    CurrentPhase = CompetitionPhase.Completed;
                    break;
            }

            Debug.Log($"[GlobalCompetitionEvent] Advanced to phase: {CurrentPhase}");
        }

        private void StartJudging()
        {
            Debug.Log($"[GlobalCompetitionEvent] Starting judging phase for {_entries.Count} entries");
            
            // Calculate scores for each entry
            foreach (var entry in _entries.Values)
            {
                float totalScore = CalculateEntryScore(entry);
                Leaderboard[entry.PlayerId] = totalScore;
            }

            // Sort leaderboard
            Leaderboard = Leaderboard.OrderByDescending(kvp => kvp.Value)
                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private float CalculateEntryScore(CompetitionEntry entry)
        {
            float totalScore = 0f;
            
            foreach (var criteria in JudgingCriteria)
            {
                float criteriaScore = entry.GetCriteriaScore(criteria.Name);
                totalScore += criteriaScore * criteria.Weight;
            }

            return totalScore;
        }

        private void FinalizeResults()
        {
            Debug.Log($"[GlobalCompetitionEvent] Finalizing competition results");
            
            // Distribute rewards based on leaderboard position
            DistributeCompetitionRewards();
        }

        private void DistributeCompetitionRewards()
        {
            var sortedPlayers = Leaderboard.Keys.ToList();
            
            for (int i = 0; i < Math.Min(sortedPlayers.Count, 10); i++) // Top 10 get rewards
            {
                var playerId = sortedPlayers[i];
                var player = GetParticipants().FirstOrDefault(p => p.PlayerId == playerId);
                
                if (player != null)
                {
                    var positionRewards = GetPositionRewards(i + 1);
                    foreach (var reward in positionRewards)
                    {
                        GrantReward(player, reward);
                    }
                }
            }
        }

        private List<EventReward> GetPositionRewards(int position)
        {
            // Return rewards based on leaderboard position
            var rewards = new List<EventReward>();
            
            if (position == 1)
            {
                rewards.Add(new EventReward
                {
                    RewardId = $"first_place_{EventId}",
                    RewardType = "Trophy",
                    RewardValue = 1000,
                    Description = "First Place Trophy"
                });
            }
            else if (position <= 3)
            {
                rewards.Add(new EventReward
                {
                    RewardId = $"podium_place_{EventId}",
                    RewardType = "Medal",
                    RewardValue = 500,
                    Description = $"Podium Finish - {position} Place"
                });
            }
            else if (position <= 10)
            {
                rewards.Add(new EventReward
                {
                    RewardId = $"top_ten_{EventId}",
                    RewardType = "Certificate",
                    RewardValue = 100,
                    Description = $"Top 10 Finish - {position} Place"
                });
            }

            return rewards;
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
            if (RequiresScientificAccuracy)
            {
                Debug.Log($"[GlobalCompetitionEvent] Validating educational content for {EventName}");
            }
        }

        // IRewardableEvent Implementation
        public List<EventReward> GetAvailableRewards(PlayerProfile player)
        {
            if (Leaderboard.TryGetValue(player.PlayerId, out var score))
            {
                var position = Leaderboard.Keys.ToList().IndexOf(player.PlayerId) + 1;
                return GetPositionRewards(position);
            }
            
            return new List<EventReward>();
        }

        public void DistributeRewards()
        {
            DistributeCompetitionRewards();
        }

        public bool HasReceivedReward(PlayerProfile player, string rewardId)
        {
            // Check if player has already received this reward
            return false; // Placeholder implementation
        }

        public void GrantReward(PlayerProfile player, EventReward reward)
        {
            Debug.Log($"[GlobalCompetitionEvent] Granting reward {reward.RewardId} to player {player.PlayerId}");
        }

        // Override base class methods for competition specific behavior
        protected override void OnEventStartedInternal()
        {
            base.OnEventStartedInternal();
            CurrentPhase = CompetitionPhase.Registration;
            Debug.Log($"[GlobalCompetitionEvent] Competition {EventName} has started with registration phase!");
        }

        protected override void OnEventEndedInternal(EventResult result)
        {
            base.OnEventEndedInternal(result);
            
            if (CurrentPhase != CompetitionPhase.Completed)
            {
                // Force completion if event ends before all phases complete
                CurrentPhase = CompetitionPhase.Completed;
                FinalizeResults();
            }
            
            Debug.Log($"[GlobalCompetitionEvent] Competition {EventName} has ended!");
        }

        protected virtual void OnPlayerJoinedInternal(PlayerProfile player)
        {
            // Initialize learning progress for new participant
            TrackLearningProgress(player);
            
            Debug.Log($"[GlobalCompetitionEvent] Player {player.PlayerId} registered for competition {EventName}");
        }

        protected override void OnPlayerActionInternal(PlayerAction action)
        {
            base.OnPlayerActionInternal(action);
            
            // Update learning progress based on action
            if (_learningProgress.TryGetValue(action.PlayerId, out var progress))
            {
                progress.LastUpdate = DateTime.Now;
                progress.OverallProgress = Math.Min(1.0f, progress.OverallProgress + 0.05f);
            }
            
            Debug.Log($"[GlobalCompetitionEvent] Player action processed in competition {EventName}");
        }

        public override void Cleanup()
        {
            base.Cleanup();
            
            _entries?.Clear();
            _learningProgress?.Clear();
            Leaderboard?.Clear();
            JudgingCriteria?.Clear();
            
            Debug.Log($"[GlobalCompetitionEvent] Cleaned up competition {EventName}");
        }
    }

    // Supporting data structures for competition events
    [Serializable]
    public class CompetitionEntry
    {
        public string PlayerId;
        public DateTime SubmissionTime;
        public string Title;
        public string Description;
        public Dictionary<string, float> CriteriaScores = new Dictionary<string, float>();
        
        public float GetCriteriaScore(string criteriaName)
        {
            return CriteriaScores.GetValueOrDefault(criteriaName, 0f);
        }
        
        public void SetCriteriaScore(string criteriaName, float score)
        {
            CriteriaScores[criteriaName] = Mathf.Clamp01(score);
        }
    }

}