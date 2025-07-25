using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages player objectives, daily challenges, and goal-driven gameplay.
    /// Provides engaging short-term and long-term goals to enhance player motivation.
    /// Integrates with existing progression and achievement systems.
    /// </summary>
    public class ObjectiveManager : ChimeraManager
    {
        [Header("Objective Configuration")]
        [SerializeField] private bool _enableObjectiveSystem = true;
        [SerializeField] private int _maxActiveObjectives = 5;
        [SerializeField] private int _maxDailyChallenges = 3;
        [SerializeField] private float _objectiveCheckInterval = 30f;
        
        [Header("Daily Challenge Configuration")]
        [SerializeField] private bool _enableDailyChallenges = true;
        [SerializeField] private bool _refreshChallengesAtMidnight = true;
        [SerializeField] private float _challengeDifficultyScaling = 1f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onObjectiveCompleted;
        [SerializeField] private SimpleGameEventSO _onObjectiveFailed;
        [SerializeField] private SimpleGameEventSO _onChallengeCompleted;
        [SerializeField] private SimpleGameEventSO _onNewObjectiveAvailable;
        [SerializeField] private SimpleGameEventSO _onDailyChallengesRefreshed;
        
        // System references - commented out during namespace transition
        // private ProgressionManager _progressionManager;
        // private PlantManager _plantManager;
        // private TimeManager _timeManager;
        
        // Objective tracking
        private List<ActiveObjective> _activeObjectives = new List<ActiveObjective>();
        private List<ActiveChallenge> _dailyChallenges = new List<ActiveChallenge>();
        private List<ObjectiveTemplate> _availableObjectives = new List<ObjectiveTemplate>();
        private Queue<ObjectiveTemplate> _objectiveQueue = new Queue<ObjectiveTemplate>();
        
        // Progress tracking
        private Dictionary<string, float> _progressTracker = new Dictionary<string, float>();
        private DateTime _lastChallengeRefresh;
        private float _lastObjectiveCheck;
        private int _totalObjectivesCompleted = 0;
        private int _totalChallengesCompleted = 0;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        // Public Properties
        public int ActiveObjectiveCount => _activeObjectives.Count;
        public int DailyChallengeCount => _dailyChallenges.Count;
        public int TotalObjectivesCompleted => _totalObjectivesCompleted;
        public List<ActiveObjective> ActiveObjectives => _activeObjectives.ToList();
        public List<ActiveChallenge> DailyChallenges => _dailyChallenges.ToList();
        
        // Events
        public System.Action<ActiveObjective> OnObjectiveCompleted;
        public System.Action<ActiveObjective> OnObjectiveFailed;
        public System.Action<ActiveChallenge> OnChallengeCompleted;
        public System.Action OnNewObjectiveGenerated;
        
        protected override void OnManagerInitialize()
        {
            // InitializeSystemReferences(); // Commented out during namespace transition
            InitializeObjectiveTemplates();
            LoadObjectiveProgress();
            
            if (_enableObjectiveSystem)
            {
                GenerateInitialObjectives();
            }
            
            if (_enableDailyChallenges)
            {
                RefreshDailyChallenges();
            }
            
            _lastChallengeRefresh = DateTime.Now;
            _lastObjectiveCheck = Time.time;
            
            LogInfo("ObjectiveManager initialized with comprehensive goal-driven gameplay");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableObjectiveSystem) return;
            
            float currentTime = Time.time;
            
            // Check objective progress
            if (currentTime - _lastObjectiveCheck >= _objectiveCheckInterval)
            {
                UpdateObjectiveProgress();
                _lastObjectiveCheck = currentTime;
            }
            
            // Check for daily challenge refresh
            if (_enableDailyChallenges && _refreshChallengesAtMidnight)
            {
                CheckDailyChallengeRefresh();
            }
            
            // Generate new objectives if needed
            if (_activeObjectives.Count < _maxActiveObjectives)
            {
                GenerateNewObjective();
            }
        }
        
        /// <summary>
        /// Manually complete an objective (for script-triggered completions)
        /// </summary>
        public void CompleteObjective(string objectiveId)
        {
            var objective = _activeObjectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (objective != null)
            {
                CompleteObjective(objective);
            }
        }
        
        /// <summary>
        /// Update progress on a specific objective
        /// </summary>
        public void UpdateObjectiveProgress(string objectiveId, float progressValue)
        {
            var objective = _activeObjectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (objective != null)
            {
                objective.CurrentProgress = Mathf.Clamp(progressValue, 0f, objective.TargetProgress);
                
                if (objective.CurrentProgress >= objective.TargetProgress)
                {
                    CompleteObjective(objective);
                }
            }
        }
        
        /// <summary>
        /// Add progress to an objective
        /// </summary>
        public void AddObjectiveProgress(string objectiveType, float progressAmount)
        {
            var relevantObjectives = _activeObjectives.Where(o => o.ObjectiveType == objectiveType).ToList();
            
            foreach (var objective in relevantObjectives)
            {
                objective.CurrentProgress = Mathf.Clamp(
                    objective.CurrentProgress + progressAmount, 
                    0f, 
                    objective.TargetProgress
                );
                
                if (objective.CurrentProgress >= objective.TargetProgress)
                {
                    CompleteObjective(objective);
                }
            }
            
            // Also check daily challenges
            var relevantChallenges = _dailyChallenges.Where(c => c.ObjectiveType == objectiveType).ToList();
            
            foreach (var challenge in relevantChallenges)
            {
                challenge.CurrentProgress = Mathf.Clamp(
                    challenge.CurrentProgress + progressAmount,
                    0f,
                    challenge.TargetProgress
                );
                
                if (challenge.CurrentProgress >= challenge.TargetProgress)
                {
                    CompleteChallenge(challenge);
                }
            }
        }
        
        /// <summary>
        /// Get current objective progress for UI display
        /// </summary>
        public List<ObjectiveProgressData> GetObjectiveProgressData()
        {
            var progressData = new List<ObjectiveProgressData>();
            
            foreach (var objective in _activeObjectives)
            {
                progressData.Add(new ObjectiveProgressData
                {
                    ObjectiveId = objective.ObjectiveId,
                    Title = objective.Title,
                    Description = objective.Description,
                    CurrentProgress = objective.CurrentProgress,
                    TargetProgress = objective.TargetProgress,
                    ProgressPercentage = objective.CurrentProgress / objective.TargetProgress,
                    Difficulty = objective.Difficulty,
                    TimeRemaining = objective.ExpirationTime - DateTime.Now,
                    RewardPreview = GetRewardPreview(objective.Rewards)
                });
            }
            
            return progressData;
        }
        
        /// <summary>
        /// Get daily challenge progress for UI display
        /// </summary>
        public List<ChallengeProgressData> GetChallengeProgressData()
        {
            var progressData = new List<ChallengeProgressData>();
            
            foreach (var challenge in _dailyChallenges)
            {
                progressData.Add(new ChallengeProgressData
                {
                    ChallengeId = challenge.ChallengeId,
                    Title = challenge.Title,
                    Description = challenge.Description,
                    CurrentProgress = challenge.CurrentProgress,
                    TargetProgress = challenge.TargetProgress,
                    ProgressPercentage = challenge.CurrentProgress / challenge.TargetProgress,
                    Difficulty = challenge.Difficulty,
                    RewardPreview = GetRewardPreview(challenge.Rewards),
                    IsCompleted = challenge.CurrentProgress >= challenge.TargetProgress
                });
            }
            
            return progressData;
        }
        
        private void InitializeObjectiveTemplates()
        {
            // Generate dynamic objective templates based on current game state
            CreateCultivationObjectives();
            CreateEconomicObjectives();
            CreateResearchObjectives();
            CreateQualityObjectives();
            CreateEfficiencyObjectives();
            
            LogInfo($"Initialized {_availableObjectives.Count} objective templates");
        }
        
        private void CreateCultivationObjectives()
        {
            var cultivationObjectives = new List<ObjectiveTemplate>
            {
                new ObjectiveTemplate
                {
                    Title = "Green Thumb Beginner",
                    Description = "Successfully grow and harvest 3 plants",
                    ObjectiveType = "plant_harvest",
                    TargetProgress = 3f,
                    Difficulty = ObjectiveDifficulty.Easy,
                    EstimatedDuration = TimeSpan.FromDays(7),
                    CategoryIcon = "🌱",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 200f },
                new ObjectiveReward { Type = ObjectiveRewardType.Currency, Value = 1000f }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Strain Collector",
                    Description = "Grow 5 different plant strains",
                    ObjectiveType = "strain_diversity",
                    TargetProgress = 5f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(14),
                    CategoryIcon = "🧬",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 500f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Advanced Genetics Lab" }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Perfect Health Master",
                    Description = "Maintain 10 plants at 95%+ health for 3 days",
                    ObjectiveType = "health_mastery",
                    TargetProgress = 10f,
                    Difficulty = ObjectiveDifficulty.Hard,
                    EstimatedDuration = TimeSpan.FromDays(10),
                    CategoryIcon = "💚",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 750f },
                new ObjectiveReward { Type = ObjectiveRewardType.SkillPoints, Value = 2f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Health Monitoring AI" }
                    }
                }
            };
            
            _availableObjectives.AddRange(cultivationObjectives);
        }
        
        private void CreateEconomicObjectives()
        {
            var economicObjectives = new List<ObjectiveTemplate>
            {
                new ObjectiveTemplate
                {
                    Title = "First Profit",
                    Description = "Generate $5,000 in total revenue",
                    ObjectiveType = "revenue_generation",
                    TargetProgress = 5000f,
                    Difficulty = ObjectiveDifficulty.Easy,
                    EstimatedDuration = TimeSpan.FromDays(5),
                    CategoryIcon = "💰",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 300f },
                new ObjectiveReward { Type = ObjectiveRewardType.Currency, Value = 2000f }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Market Savvy",
                    Description = "Complete 10 successful trades on the market",
                    ObjectiveType = "trading_success",
                    TargetProgress = 10f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(8),
                    CategoryIcon = "📈",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 400f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Market Analytics Dashboard" }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Business Mogul",
                    Description = "Reach $50,000 in cash reserves",
                    ObjectiveType = "cash_accumulation",
                    TargetProgress = 50000f,
                    Difficulty = ObjectiveDifficulty.Hard,
                    EstimatedDuration = TimeSpan.FromDays(21),
                    CategoryIcon = "🏦",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 1000f },
                new ObjectiveReward { Type = ObjectiveRewardType.SkillPoints, Value = 3f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Investment Portfolio" }
                    }
                }
            };
            
            _availableObjectives.AddRange(economicObjectives);
        }
        
        private void CreateResearchObjectives()
        {
            var researchObjectives = new List<ObjectiveTemplate>
            {
                new ObjectiveTemplate
                {
                    Title = "Research Pioneer",
                    Description = "Complete 3 research projects",
                    ObjectiveType = "research_completion",
                    TargetProgress = 3f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(12),
                    CategoryIcon = "🔬",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 600f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Advanced Research Lab" }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Innovation Leader",
                    Description = "Unlock 5 new technologies through research",
                    ObjectiveType = "technology_unlock",
                    TargetProgress = 5f,
                    Difficulty = ObjectiveDifficulty.Hard,
                    EstimatedDuration = TimeSpan.FromDays(18),
                    CategoryIcon = "⚗️",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 800f },
                new ObjectiveReward { Type = ObjectiveRewardType.SkillPoints, Value = 3f },
                new ObjectiveReward { Type = ObjectiveRewardType.Title, Description = "Innovation Pioneer" }
                    }
                }
            };
            
            _availableObjectives.AddRange(researchObjectives);
        }
        
        private void CreateQualityObjectives()
        {
            var qualityObjectives = new List<ObjectiveTemplate>
            {
                new ObjectiveTemplate
                {
                    Title = "Quality Excellence",
                    Description = "Harvest 5 plants with 90%+ quality rating",
                    ObjectiveType = "quality_harvest",
                    TargetProgress = 5f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(10),
                    CategoryIcon = "⭐",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 500f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Quality Assurance System" }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Perfectionist",
                    Description = "Achieve 95%+ quality on any harvest",
                    ObjectiveType = "perfect_quality",
                    TargetProgress = 1f,
                    Difficulty = ObjectiveDifficulty.Expert,
                    EstimatedDuration = TimeSpan.FromDays(15),
                    CategoryIcon = "💎",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 1000f },
                new ObjectiveReward { Type = ObjectiveRewardType.SkillPoints, Value = 2f },
                new ObjectiveReward { Type = ObjectiveRewardType.Title, Description = "Quality Master" }
                    }
                }
            };
            
            _availableObjectives.AddRange(qualityObjectives);
        }
        
        private void CreateEfficiencyObjectives()
        {
            var efficiencyObjectives = new List<ObjectiveTemplate>
            {
                new ObjectiveTemplate
                {
                    Title = "Speed Grower",
                    Description = "Complete a full growth cycle in under 20 days",
                    ObjectiveType = "growth_speed",
                    TargetProgress = 1f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(25),
                    CategoryIcon = "⚡",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 400f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Growth Acceleration Boost" }
                    }
                },
                new ObjectiveTemplate
                {
                    Title = "Automation Expert",
                    Description = "Set up 5 automation rules",
                    ObjectiveType = "automation_setup",
                    TargetProgress = 5f,
                    Difficulty = ObjectiveDifficulty.Medium,
                    EstimatedDuration = TimeSpan.FromDays(7),
                    CategoryIcon = "🤖",
                    Rewards = new List<ObjectiveReward>
                    {
                                        new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 350f },
                new ObjectiveReward { Type = ObjectiveRewardType.UnlockFeature, Description = "Advanced AI Assistant" }
                    }
                }
            };
            
            _availableObjectives.AddRange(efficiencyObjectives);
        }
        
        private void GenerateInitialObjectives()
        {
            // Generate 3-5 starting objectives from different categories
            var beginnerObjectives = _availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Easy).ToList();
            var mediumObjectives = _availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Medium).ToList();
            
            // Start with easy objectives
            for (int i = 0; i < 2 && i < beginnerObjectives.Count; i++)
            {
                CreateActiveObjective(beginnerObjectives[i]);
            }
            
            // Add one medium objective
            if (mediumObjectives.Count > 0)
            {
                CreateActiveObjective(mediumObjectives[UnityEngine.Random.Range(0, mediumObjectives.Count)]);
            }
        }
        
        private void GenerateNewObjective()
        {
            if (_availableObjectives.Count == 0) return;
            
            // Select objective based on player progression level
            var playerLevel = 1; // Assuming a default level
            var appropriateObjectives = GetObjectivesForPlayerLevel(playerLevel);
            
            if (appropriateObjectives.Count > 0)
            {
                var selectedTemplate = appropriateObjectives[UnityEngine.Random.Range(0, appropriateObjectives.Count)];
                CreateActiveObjective(selectedTemplate);
                
                _onNewObjectiveAvailable?.Raise();
                OnNewObjectiveGenerated?.Invoke();
                
                LogInfo($"Generated new objective: {selectedTemplate.Title}");
            }
        }
        
        private List<ObjectiveTemplate> GetObjectivesForPlayerLevel(int playerLevel)
        {
            var availableByDifficulty = new List<ObjectiveTemplate>();
            
            // Add objectives based on player level
            if (playerLevel >= 1)
                availableByDifficulty.AddRange(_availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Easy));
            if (playerLevel >= 5)
                availableByDifficulty.AddRange(_availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Medium));
            if (playerLevel >= 15)
                availableByDifficulty.AddRange(_availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Hard));
            if (playerLevel >= 25)
                availableByDifficulty.AddRange(_availableObjectives.Where(o => o.Difficulty == ObjectiveDifficulty.Expert));
            
            // Filter out objectives that are already active
            return availableByDifficulty.Where(template => 
                !_activeObjectives.Any(active => active.Title == template.Title)
            ).ToList();
        }
        
        private ActiveObjective CreateActiveObjective(ObjectiveTemplate template)
        {
            var objective = new ActiveObjective
            {
                ObjectiveId = Guid.NewGuid().ToString(),
                Title = template.Title,
                Description = template.Description,
                ObjectiveType = template.ObjectiveType,
                CurrentProgress = 0f,
                TargetProgress = template.TargetProgress,
                Difficulty = template.Difficulty,
                CategoryIcon = template.CategoryIcon,
                StartTime = DateTime.Now,
                ExpirationTime = DateTime.Now.Add(template.EstimatedDuration),
                Rewards = template.Rewards
            };
            
            _activeObjectives.Add(objective);
            return objective;
        }
        
        private void CompleteObjective(ActiveObjective objective)
        {
            // Award rewards
            foreach (var reward in objective.Rewards)
            {
                AwardReward(reward);
            }
            
            _activeObjectives.Remove(objective);
            _totalObjectivesCompleted++;
            
            _onObjectiveCompleted?.Raise();
            OnObjectiveCompleted?.Invoke(objective);
            
            LogInfo($"🎯 Objective Completed: {objective.Title}");
            
            // Generate notification for player
            ShowObjectiveCompletionNotification(objective);
        }
        
        private void RefreshDailyChallenges()
        {
            _dailyChallenges.Clear();
            
            // Generate 3 daily challenges of varying difficulty
            var challengeTemplates = GenerateDailyChallengeTemplates();
            
            foreach (var template in challengeTemplates.Take(_maxDailyChallenges))
            {
                var challenge = new ActiveChallenge
                {
                    ChallengeId = Guid.NewGuid().ToString(),
                    Title = template.Title,
                    Description = template.Description,
                    ObjectiveType = template.ObjectiveType,
                    CurrentProgress = 0f,
                    TargetProgress = template.TargetProgress,
                    Difficulty = template.Difficulty,
                    Rewards = template.Rewards
                };
                
                _dailyChallenges.Add(challenge);
            }
            
            _lastChallengeRefresh = DateTime.Now;
            _onDailyChallengesRefreshed?.Raise();
            
            LogInfo($"Daily challenges refreshed: {_dailyChallenges.Count} new challenges available");
        }
        
        private List<ObjectiveTemplate> GenerateDailyChallengeTemplates()
        {
            var challenges = new List<ObjectiveTemplate>();
            
            // Easy daily challenge
            challenges.Add(new ObjectiveTemplate
            {
                Title = "Daily Cultivation",
                Description = "Check on your plants and perform 3 care actions",
                ObjectiveType = "daily_care",
                TargetProgress = 3f,
                Difficulty = ObjectiveDifficulty.Easy,
                CategoryIcon = "🌿",
                Rewards = new List<ObjectiveReward>
                {
                                    new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 100f },
                new ObjectiveReward { Type = ObjectiveRewardType.Currency, Value = 500f }
                }
            });
            
            // Medium daily challenge  
            challenges.Add(new ObjectiveTemplate
            {
                Title = "Market Opportunity",
                Description = "Complete 2 trades at profitable prices",
                ObjectiveType = "daily_trading",
                TargetProgress = 2f,
                Difficulty = ObjectiveDifficulty.Medium,
                CategoryIcon = "💹",
                Rewards = new List<ObjectiveReward>
                {
                                    new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 200f },
                new ObjectiveReward { Type = ObjectiveRewardType.Currency, Value = 1500f }
                }
            });
            
            // Hard daily challenge
            challenges.Add(new ObjectiveTemplate
            {
                Title = "Excellence Standard",
                Description = "Maintain all plants above 85% health",
                ObjectiveType = "daily_excellence",
                TargetProgress = 1f,
                Difficulty = ObjectiveDifficulty.Hard,
                CategoryIcon = "✨",
                Rewards = new List<ObjectiveReward>
                {
                                    new ObjectiveReward { Type = ObjectiveRewardType.Experience, Value = 300f },
                new ObjectiveReward { Type = ObjectiveRewardType.SkillPoints, Value = 1f }
                }
            });
            
            return challenges;
        }
        
        private void UpdateObjectiveProgress()
        {
            // Check for expired objectives
            var expiredObjectives = _activeObjectives.Where(o => DateTime.Now > o.ExpirationTime).ToList();
            foreach (var expired in expiredObjectives)
            {
                FailObjective(expired);
            }
            
            // Update progress from game state
            UpdateProgressFromGameState();
        }
        
        private void UpdateProgressFromGameState()
        {
            // Implementation needed
        }
        
        private void FailObjective(ActiveObjective objective)
        {
            _activeObjectives.Remove(objective);
            _onObjectiveFailed?.Raise();
            OnObjectiveFailed?.Invoke(objective);
            
            LogInfo($"❌ Objective Failed: {objective.Title}");
        }
        
        private void CompleteChallenge(ActiveChallenge challenge)
        {
            foreach (var reward in challenge.Rewards)
            {
                AwardReward(reward);
            }
            
            _totalChallengesCompleted++;
            _onChallengeCompleted?.Raise();
            OnChallengeCompleted?.Invoke(challenge);
            
            LogInfo($"🏆 Daily Challenge Completed: {challenge.Title}");
        }
        
        private void AwardReward(ObjectiveReward reward)
        {
            // Implementation needed
        }
        
        private void CheckDailyChallengeRefresh()
        {
            var now = DateTime.Now;
            var lastRefreshDate = _lastChallengeRefresh.Date;
            var currentDate = now.Date;
            
            if (currentDate > lastRefreshDate)
            {
                RefreshDailyChallenges();
            }
        }
        
        private string GetRewardPreview(List<ObjectiveReward> rewards)
        {
            var previews = new List<string>();
            
            foreach (var reward in rewards)
            {
                switch (reward.Type)
                {
                    case ObjectiveRewardType.Experience:
                        previews.Add($"+{reward.Value:F0} XP");
                        break;
                    case ObjectiveRewardType.Currency:
                        previews.Add($"+${reward.Value:F0}");
                        break;
                    case ObjectiveRewardType.SkillPoints:
                        previews.Add($"+{reward.Value:F0} SP");
                        break;
                    case ObjectiveRewardType.UnlockFeature:
                        previews.Add($"🔓 {reward.Description}");
                        break;
                    case ObjectiveRewardType.Title:
                        previews.Add($"🎖️ {reward.Description}");
                        break;
                }
            }
            
            return string.Join(", ", previews);
        }
        
        private void ShowObjectiveCompletionNotification(ActiveObjective objective)
        {
            // Implementation needed
        }
        
        private void LoadObjectiveProgress()
        {
            // Load saved objective progress from save system
            // For now, start fresh each session
        }
        
        protected override void OnManagerShutdown()
        {
            // Save objective progress
            LogInfo("ObjectiveManager shutdown - objective progress saved");
        }
    }
    
    // Supporting data classes
    [System.Serializable]
    public class ObjectiveTemplate
    {
        public string Title;
        public string Description;
        public string ObjectiveType;
        public float TargetProgress;
        public ObjectiveDifficulty Difficulty;
        public TimeSpan EstimatedDuration;
        public string CategoryIcon;
        public List<ObjectiveReward> Rewards = new List<ObjectiveReward>();
    }
    
    [System.Serializable]
    public class ActiveObjective
    {
        public string ObjectiveId;
        public string Title;
        public string Description;
        public string ObjectiveType;
        public float CurrentProgress;
        public float TargetProgress;
        public ObjectiveDifficulty Difficulty;
        public string CategoryIcon;
        public DateTime StartTime;
        public DateTime ExpirationTime;
        public List<ObjectiveReward> Rewards = new List<ObjectiveReward>();
    }
    
    [System.Serializable]
    public class ActiveChallenge
    {
        public string ChallengeId;
        public string Title;
        public string Description;
        public string ObjectiveType;
        public float CurrentProgress;
        public float TargetProgress;
        public ObjectiveDifficulty Difficulty;
        public List<ObjectiveReward> Rewards = new List<ObjectiveReward>();
    }
    
    [System.Serializable]
    public class ObjectiveReward
    {
        public ObjectiveRewardType Type;
        public float Value;
        public string Description;
    }
    
    [System.Serializable]
    public class ObjectiveProgressData
    {
        public string ObjectiveId;
        public string Title;
        public string Description;
        public float CurrentProgress;
        public float TargetProgress;
        public float ProgressPercentage;
        public ObjectiveDifficulty Difficulty;
        public TimeSpan TimeRemaining;
        public string RewardPreview;
    }
    
    [System.Serializable]
    public class ChallengeProgressData
    {
        public string ChallengeId;
        public string Title;
        public string Description;
        public float CurrentProgress;
        public float TargetProgress;
        public float ProgressPercentage;
        public ObjectiveDifficulty Difficulty;
        public string RewardPreview;
        public bool IsCompleted;
    }
    
    public enum ObjectiveDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Legendary
    }
    
    public enum ObjectiveRewardType
    {
        Experience,
        Currency,
        SkillPoints,
        UnlockFeature,
        Title,
        Item,
        Multiplier
    }
}