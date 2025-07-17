using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Genetics.Scientific;
using ResearchField = ProjectChimera.Data.Genetics.Scientific.ResearchField;
using AchievementCategory = ProjectChimera.Data.Genetics.Scientific.ScientificAchievementCategory;
using ScientificReputation = ProjectChimera.Data.Genetics.Scientific.ScientificReputation;
using ContributionType = ProjectChimera.Data.Genetics.Scientific.ContributionType;
using System.Collections.Generic;
using System.Linq;
using System;
using PlantTrait = ProjectChimera.Data.Genetics.TraitType;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Phase 4.2: Scientific Achievement and Recognition System
    /// Tracks scientific breakthroughs, breeding accomplishments, and research contributions.
    /// Provides recognition, titles, and progression for players' scientific work.
    /// </summary>
    public class ScientificAchievementManager : ChimeraManager
    {
        [Header("Achievement Configuration")]
        [SerializeField] private bool _enableAchievements = true;
        [SerializeField] private float _achievementCheckInterval = 5f;
        [SerializeField] private bool _enableRealTimeTracking = true;
        [SerializeField] private bool _enableProgressiveAchievements = true;
        
        [Header("Recognition Settings")]
        [SerializeField] private bool _enableTitleProgression = true;
        [SerializeField] private bool _enableSpecializationTracking = true;
        [SerializeField] private float _masteryThreshold = 0.9f;
        [SerializeField] private int _breakthroughRequirement = 5;
        
        [Header("Research Tracking")]
        [SerializeField] private bool _enableDiscoveryLogging = true;
        [SerializeField] private bool _enableInnovationDetection = true;
        [SerializeField] private float _innovationThreshold = 0.95f;
        [SerializeField] private bool _enableCollaborativeResearch = true;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<ScientificAchievement> _onAchievementUnlocked;
        [SerializeField] private GameEventSO<ScientificBreakthrough> _onBreakthroughMade;
        [SerializeField] private GameEventSO<ResearchMilestone> _onMilestoneReached;
        [SerializeField] private GameEventSO<ScientificRecognition> _onRecognitionEarned;
        
        // Achievement tracking
        private Dictionary<string, ScientificAchievement> _availableAchievements = new Dictionary<string, ScientificAchievement>();
        private Dictionary<string, PlayerAchievementProgress> _playerProgress = new Dictionary<string, PlayerAchievementProgress>();
        private Dictionary<string, List<ScientificAchievement>> _unlockedAchievements = new Dictionary<string, List<ScientificAchievement>>();
        
        // Research tracking
        private List<ScientificBreakthrough> _globalBreakthroughs = new List<ScientificBreakthrough>();
        private Dictionary<string, List<ResearchContribution>> _playerContributions = new Dictionary<string, List<ResearchContribution>>();
        private Dictionary<string, SpecializationData> _playerSpecializations = new Dictionary<string, SpecializationData>();
        
        // Recognition system
        private Dictionary<string, ScientificReputation> _playerReputations = new Dictionary<string, ScientificReputation>();
        private List<ScientificTitleProgression> _titleProgressions = new List<ScientificTitleProgression>();
        private Dictionary<string, List<ScientificRecognition>> _playerRecognitions = new Dictionary<string, List<ScientificRecognition>>();
        
        // Performance tracking
        private float _lastAchievementCheck;
        private ScientificStatistics _globalStatistics = new ScientificStatistics();
        
        // Dependencies
        private BreedingSimulator _breedingSimulator;
        private ProjectChimera.Systems.Genetics.TraitExpressionEngine _traitEngine;
        private BreedingGoalManager _breedingGoalManager;
        private BreedingTournamentManager _tournamentManager;
        
        // Events
        public event Action<ScientificAchievement, string> OnAchievementUnlocked;
        public event Action<ScientificBreakthrough> OnBreakthroughMade;
        public event Action<ResearchMilestone, string> OnMilestoneReached;
        public event Action<ScientificRecognition, string> OnRecognitionEarned;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        protected override void OnManagerInitialize()
        {
            InitializeAchievementSystem();
            LoadAchievementDefinitions();
            LoadTitleProgressions();
            
            // Initialize dependencies directly instead of FindObjectOfType
            _breedingSimulator = new ProjectChimera.Systems.Genetics.BreedingSimulator(true, 0.1f);
            _traitEngine = new ProjectChimera.Systems.Genetics.TraitExpressionEngine(true, true);
            _breedingGoalManager = FindObjectOfType<BreedingGoalManager>();
            _tournamentManager = FindObjectOfType<BreedingTournamentManager>();
            
            // Subscribe to events
            SetupEventListeners();
            
            _lastAchievementCheck = Time.time;
            
            LogInfo("Scientific Achievement Manager initialized with research tracking");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableAchievements) return;
            
            if (Time.time - _lastAchievementCheck >= _achievementCheckInterval)
            {
                CheckProgressiveAchievements();
                UpdateSpecializationTracking();
                ProcessMilestoneChecks();
                _lastAchievementCheck = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize the achievement system.
        /// </summary>
        private void InitializeAchievementSystem()
        {
            CreateFoundationalAchievements();
            CreateBreedingAchievements();
            CreateScientificAchievements();
            CreateInnovationAchievements();
            CreateCollaborationAchievements();
            
            LogInfo("Achievement system initialized with scientific focus");
        }
        
        /// <summary>
        /// Set up event listeners for automatic achievement tracking.
        /// </summary>
        private void SetupEventListeners()
        {
            // Subscribe to breeding events
            if (_breedingSimulator != null)
            {
                // Would subscribe to breeding completion events
            }
            
            // Subscribe to tournament events
            if (_tournamentManager != null)
            {
                _tournamentManager.OnTournamentCompleted += OnTournamentCompleted;
                _tournamentManager.OnResultsAnnounced += OnTournamentResultsAnnounced;
            }
            
            // Subscribe to breeding goal events
            if (_breedingGoalManager != null)
            {
                _breedingGoalManager.OnGoalCompleted += OnBreedingGoalCompleted;
            }
        }
        
        /// <summary>
        /// Load achievement definitions from configuration.
        /// </summary>
        private void LoadAchievementDefinitions()
        {
            // This would load from ScriptableObject assets
            LogInfo("Achievement definitions loaded");
        }
        
        /// <summary>
        /// Load title progression configurations.
        /// </summary>
        private void LoadTitleProgressions()
        {
            _titleProgressions.Add(new ScientificTitleProgression
            {
                TitleId = "novice-breeder",
                TitleName = "Novice Breeder",
                Description = "Beginning your journey in genetic research",
                RequiredAchievements = new List<string> { "first-breeding" },
                MinimumReputation = 0,
                SpecializationRequirements = new Dictionary<ResearchField, float>()
            });
            
            _titleProgressions.Add(new ScientificTitleProgression
            {
                TitleId = "genetic-researcher",
                TitleName = "Genetic Researcher",
                Description = "Dedicated to understanding plant genetics",
                RequiredAchievements = new List<string> { "trait-specialist", "inheritance-master" },
                MinimumReputation = 1000,
                SpecializationRequirements = new Dictionary<ResearchField, float> { { ResearchField.Genetics, 5.0f } }
            });
            
            _titleProgressions.Add(new ScientificTitleProgression
            {
                TitleId = "breeding-innovator",
                TitleName = "Breeding Innovator",
                Description = "Pioneer in breeding methodology",
                RequiredAchievements = new List<string> { "innovation-pioneer", "breakthrough-maker" },
                MinimumReputation = 2500,
                SpecializationRequirements = new Dictionary<ResearchField, float> { { ResearchField.BreedingTechnology, 8.0f }, { ResearchField.Innovation, 6.0f } }
            });
            
            _titleProgressions.Add(new ScientificTitleProgression
            {
                TitleId = "master-geneticist",
                TitleName = "Master Geneticist",
                Description = "Master of genetic principles and applications",
                RequiredAchievements = new List<string> { "genetic-mastery", "research-leader" },
                MinimumReputation = 5000,
                SpecializationRequirements = new Dictionary<ResearchField, float> { { ResearchField.Genetics, 10.0f }, { ResearchField.Research, 8.0f } }
            });
            
            _titleProgressions.Add(new ScientificTitleProgression
            {
                TitleId = "scientific-legend",
                TitleName = "Scientific Legend",
                Description = "Legendary contributor to cannabis science",
                RequiredAchievements = new List<string> { "paradigm-shifter", "field-pioneer" },
                MinimumReputation = 10000,
                SpecializationRequirements = new Dictionary<ResearchField, float> { { ResearchField.Genetics, 15.0f }, { ResearchField.Innovation, 12.0f }, { ResearchField.Leadership, 10.0f } }
            });
            
            LogInfo($"Loaded {_titleProgressions.Count} title progressions");
        }
        
        /// <summary>
        /// Create foundational achievements.
        /// </summary>
        private void CreateFoundationalAchievements()
        {
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "first-breeding",
                Name = "First Steps",
                Description = "Complete your first breeding cross",
                Category = AchievementCategory.Foundation,
                Rarity = AchievementRarity.Common,
                ReputationReward = 100,
                UnlockRequirements = new List<string> { "BreedingCrossesCompleted: 1" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "genetic-observer",
                Name = "Genetic Observer",
                Description = "Analyze trait expression in 10 different plants",
                Category = AchievementCategory.Foundation,
                Rarity = AchievementRarity.Common,
                ReputationReward = 200,
                UnlockRequirements = new List<string> { "TraitAnalysesPerformed: 10" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "systematic-breeder",
                Name = "Systematic Breeder",
                Description = "Complete 50 breeding crosses with detailed documentation",
                Category = AchievementCategory.Foundation,
                Rarity = AchievementRarity.Uncommon,
                ReputationReward = 500,
                UnlockRequirements = new List<string> { "BreedingCrossesCompleted: 50", "DocumentedResearch: 25" }
            });
        }
        
        /// <summary>
        /// Create breeding-focused achievements.
        /// </summary>
        private void CreateBreedingAchievements()
        {
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "trait-specialist",
                Name = "Trait Specialist",
                Description = "Achieve mastery in optimizing a specific trait",
                Category = AchievementCategory.Breeding,
                Rarity = AchievementRarity.Rare,
                ReputationReward = 1000,
                UnlockRequirements = new List<string> { "TraitMasteryAchieved: 1", "MinimumTraitExpression: 0.95" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "inheritance-master",
                Name = "Inheritance Master",
                Description = "Demonstrate deep understanding of Mendelian genetics",
                Category = AchievementCategory.Breeding,
                Rarity = AchievementRarity.Rare,
                ReputationReward = 1500,
                UnlockRequirements = new List<string> { "PredictedInheritanceAccuracy: 0.9", "BreedingGenerationsCompleted: 5" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "hybrid-vigor-expert",
                Name = "Hybrid Vigor Expert",
                Description = "Consistently achieve significant heterosis effects",
                Category = AchievementCategory.Breeding,
                Rarity = AchievementRarity.Epic,
                ReputationReward = 2000,
                UnlockRequirements = new List<string> { "HybridVigorCrossesAchieved: 10", "MinimumHeterosisEffect: 1.2" }
            });
        }
        
        /// <summary>
        /// Create scientific research achievements.
        /// </summary>
        private void CreateScientificAchievements()
        {
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "breakthrough-maker",
                Name = "Breakthrough Maker",
                Description = "Make a significant scientific breakthrough",
                Category = AchievementCategory.Research,
                Rarity = AchievementRarity.Legendary,
                ReputationReward = 5000,
                UnlockRequirements = new List<string> { "ScientificBreakthroughsMade: 1", "ResearchImpactScore: 0.8" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "data-scientist",
                Name = "Data Scientist",
                Description = "Collect and analyze extensive breeding data",
                Category = AchievementCategory.Research,
                Rarity = AchievementRarity.Rare,
                ReputationReward = 1200,
                UnlockRequirements = new List<string> { "DataPointsCollected: 1000", "StatisticalAnalysesPerformed: 50" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "research-leader",
                Name = "Research Leader",
                Description = "Lead collaborative research projects",
                Category = AchievementCategory.Research,
                Rarity = AchievementRarity.Epic,
                ReputationReward = 3000,
                UnlockRequirements = new List<string> { "ResearchProjectsLed: 5", "CollaboratorsWorkedWith: 10" }
            });
        }
        
        /// <summary>
        /// Create innovation-focused achievements.
        /// </summary>
        private void CreateInnovationAchievements()
        {
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "innovation-pioneer",
                Name = "Innovation Pioneer",
                Description = "Pioneer new breeding techniques",
                Category = AchievementCategory.Innovation,
                Rarity = AchievementRarity.Epic,
                ReputationReward = 2500,
                UnlockRequirements = new List<string> { "InnovativeTechniquesCreated: 3", "TechniqueAdoptionRate: 0.25" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "paradigm-shifter",
                Name = "Paradigm Shifter",
                Description = "Fundamentally change understanding in the field",
                Category = AchievementCategory.Innovation,
                Rarity = AchievementRarity.Mythical,
                ReputationReward = 10000,
                UnlockRequirements = new List<string> { "ParadigmShiftInitiated: true", "FieldInfluenceScore: 0.9" }
            });
        }
        
        /// <summary>
        /// Create collaboration achievements.
        /// </summary>
        private void CreateCollaborationAchievements()
        {
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "team-player",
                Name = "Team Player",
                Description = "Successfully collaborate on breeding projects",
                Category = AchievementCategory.Collaboration,
                Rarity = AchievementRarity.Common,
                ReputationReward = 300,
                UnlockRequirements = new List<string> { "CollaborativeProjectsCompleted: 5" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "knowledge-sharer",
                Name = "Knowledge Sharer",
                Description = "Share knowledge and help other researchers",
                Category = AchievementCategory.Collaboration,
                Rarity = AchievementRarity.Uncommon,
                ReputationReward = 600,
                UnlockRequirements = new List<string> { "KnowledgeSharedInstances: 25", "ResearchersHelped: 10" }
            });
            
            CreateAchievement(new ScientificAchievement
            {
                AchievementId = "community-leader",
                Name = "Community Leader",
                Description = "Lead and inspire the research community",
                Category = AchievementCategory.Collaboration,
                Rarity = AchievementRarity.Epic,
                ReputationReward = 4000,
                UnlockRequirements = new List<string> { "CommunityLeadershipScore: 0.8", "ResearchersInfluenced: 50" }
            });
        }
        
        /// <summary>
        /// Create an achievement and add it to the system.
        /// </summary>
        private void CreateAchievement(ScientificAchievement achievement)
        {
            achievement.CreatedAt = DateTime.Now;
            _availableAchievements[achievement.AchievementId] = achievement;
        }
        
        /// <summary>
        /// Record a breeding event for achievement tracking.
        /// </summary>
        public void RecordBreedingEvent(string playerId, BreedingEventData eventData)
        {
            if (!_enableAchievements) return;
            
            var progress = GetOrCreatePlayerProgress(playerId);
            
            // Update relevant counters
            progress.BreedingCrossesCompleted++;
            progress.TotalBreedingEvents++;
            
            // Record trait achievements
            if (eventData.TraitExpression != null)
            {
                UpdateTraitSpecialization(playerId, eventData.TraitExpression);
                progress.TraitAnalysesPerformed++;
            }
            
            // Check for quality achievements
            if (eventData.OverallQuality >= _innovationThreshold)
            {
                progress.HighQualityResults++;
                RecordPotentialBreakthrough(playerId, eventData);
            }
            
            // Check for hybrid vigor
            if (eventData.HeterosisEffect >= 1.2f)
            {
                progress.HybridVigorCrossesAchieved++;
            }
            
            CheckAchievementUnlocks(playerId);
            
            if (_enableDiscoveryLogging)
            {
                LogBreedingDiscovery(playerId, eventData);
            }
        }
        
        /// <summary>
        /// Record a research contribution.
        /// </summary>
        public void RecordResearchContribution(string playerId, ResearchContribution contribution)
        {
            if (!_playerContributions.ContainsKey(playerId))
            {
                _playerContributions[playerId] = new List<ResearchContribution>();
            }
            
            _playerContributions[playerId].Add(contribution);
            
            var progress = GetOrCreatePlayerProgress(playerId);
            progress.DocumentedResearch++;
            progress.DataPointsCollected += contribution.DataPointsContributed;
            
            // Update specialization
            UpdateResearchSpecialization(playerId, contribution.ResearchField);
            
            CheckAchievementUnlocks(playerId);
            
            LogInfo($"Research contribution recorded for player {playerId}: {contribution.ContributionType}");
        }
        
        /// <summary>
        /// Record a scientific breakthrough.
        /// </summary>
        public void RecordScientificBreakthrough(string playerId, ScientificBreakthrough breakthrough)
        {
            breakthrough.DiscoveredBy = playerId;
            breakthrough.DiscoveryDate = DateTime.Now;
            
            _globalBreakthroughs.Add(breakthrough);
            
            var progress = GetOrCreatePlayerProgress(playerId);
            progress.ScientificBreakthroughsMade++;
            
            // Update reputation
            var reputation = GetOrCreatePlayerReputation(playerId);
            reputation.ReputationPoints += breakthrough.ReputationValue;
            reputation.BreakthroughCount++;
            
            // Create recognition
            var recognition = new ScientificRecognition
            {
                RecognitionId = Guid.NewGuid().ToString(),
                PlayerId = playerId,
                RecognitionType = RecognitionType.Breakthrough,
                Title = $"Breakthrough: {breakthrough.BreakthroughName}",
                Description = breakthrough.Description,
                AwardedDate = DateTime.Now,
                PrestigeValue = breakthrough.PrestigeValue
            };
            
            AwardRecognition(playerId, recognition);
            
            _onBreakthroughMade?.Raise(breakthrough);
            OnBreakthroughMade?.Invoke(breakthrough);
            
            CheckAchievementUnlocks(playerId);
            
            LogInfo($"Scientific breakthrough recorded: {breakthrough.BreakthroughName} by player {playerId}");
        }
        
        /// <summary>
        /// Check and unlock achievements for a player.
        /// </summary>
        private void CheckAchievementUnlocks(string playerId)
        {
            var progress = GetOrCreatePlayerProgress(playerId);
            var unlockedList = GetOrCreateUnlockedList(playerId);
            
            foreach (var achievement in _availableAchievements.Values)
            {
                // Skip if already unlocked
                if (unlockedList.Any(a => a.AchievementId == achievement.AchievementId))
                    continue;
                
                // Check if requirements are met
                if (AreStringRequirementsMet(achievement.UnlockRequirements, progress))
                {
                    UnlockAchievement(playerId, achievement);
                }
            }
        }
        
        /// <summary>
        /// Check if achievement requirements are met.
        /// </summary>
        private bool AreRequirementsMet(ScientificAchievementRequirements requirements, PlayerAchievementProgress progress)
        {
            if (requirements.BreedingCrossesCompleted > 0 && progress.BreedingCrossesCompleted < requirements.BreedingCrossesCompleted)
                return false;
            
            if (requirements.TraitAnalysesPerformed > 0 && progress.TraitAnalysesPerformed < requirements.TraitAnalysesPerformed)
                return false;
            
            if (requirements.DocumentedResearch > 0 && progress.DocumentedResearch < requirements.DocumentedResearch)
                return false;
            
            if (requirements.ScientificBreakthroughsMade > 0 && progress.ScientificBreakthroughsMade < requirements.ScientificBreakthroughsMade)
                return false;
            
            if (requirements.TraitMasteryAchieved > 0 && progress.TraitMasteriesAchieved < requirements.TraitMasteryAchieved)
                return false;
            
            if (requirements.HybridVigorCrossesAchieved > 0 && progress.HybridVigorCrossesAchieved < requirements.HybridVigorCrossesAchieved)
                return false;
            
            if (requirements.CollaborativeProjectsCompleted > 0 && progress.CollaborativeProjectsCompleted < requirements.CollaborativeProjectsCompleted)
                return false;
            
            if (requirements.DataPointsCollected > 0 && progress.DataPointsCollected < requirements.DataPointsCollected)
                return false;
            
            // Check minimum thresholds
            if (requirements.MinimumTraitExpression > 0f && progress.BestTraitExpression < requirements.MinimumTraitExpression)
                return false;
            
            if (requirements.MinimumHeterosisEffect > 0f && progress.BestHeterosisEffect < requirements.MinimumHeterosisEffect)
                return false;
            
            if (requirements.PredictedInheritanceAccuracy > 0f && progress.InheritancePredictionAccuracy < requirements.PredictedInheritanceAccuracy)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Check if string-based achievement requirements are met.
        /// </summary>
        private bool AreStringRequirementsMet(List<string> requirements, PlayerAchievementProgress progress)
        {
            foreach (var requirement in requirements)
            {
                if (!IsStringRequirementMet(requirement, progress))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Check if a single string requirement is met.
        /// </summary>
        private bool IsStringRequirementMet(string requirement, PlayerAchievementProgress progress)
        {
            if (string.IsNullOrEmpty(requirement))
                return true;
            
            var parts = requirement.Split(':');
            if (parts.Length != 2)
                return true; // Invalid format, consider as met
            
            var criterionName = parts[0].Trim();
            if (!int.TryParse(parts[1].Trim(), out var requiredValue))
                return true; // Invalid value, consider as met
            
            return criterionName switch
            {
                "BreedingCrossesCompleted" => progress.BreedingCrossesCompleted >= requiredValue,
                "TraitAnalysesPerformed" => progress.TraitAnalysesPerformed >= requiredValue,
                "DocumentedResearch" => progress.DocumentedResearch >= requiredValue,
                "ScientificBreakthroughsMade" => progress.ScientificBreakthroughsMade >= requiredValue,
                "TraitMasteriesAchieved" => progress.TraitMasteriesAchieved >= requiredValue,
                "HybridVigorCrossesAchieved" => progress.HybridVigorCrossesAchieved >= requiredValue,
                "CollaborativeProjectsCompleted" => progress.CollaborativeProjectsCompleted >= requiredValue,
                "DataPointsCollected" => progress.DataPointsCollected >= requiredValue,
                "HighQualityResults" => progress.HighQualityResults >= requiredValue,
                "ReachedMilestones" => progress.ReachedMilestonesCount >= requiredValue,
                _ => true // Unknown criterion, consider as met
            };
        }
        
        /// <summary>
        /// Unlock an achievement for a player.
        /// </summary>
        private void UnlockAchievement(string playerId, ScientificAchievement achievement)
        {
            var unlockedList = GetOrCreateUnlockedList(playerId);
            var unlockedAchievement = new ScientificAchievement(achievement)
            {
                UnlockedDate = DateTime.Now
            };
            
            unlockedList.Add(unlockedAchievement);
            
            // Award reputation
            var reputation = GetOrCreatePlayerReputation(playerId);
            reputation.ReputationPoints += achievement.ReputationReward;
            reputation.AchievementCount++;
            
            _onAchievementUnlocked?.Raise(unlockedAchievement);
            OnAchievementUnlocked?.Invoke(unlockedAchievement, playerId);
            
            // Check for title progression
            CheckTitleProgression(playerId);
            
            LogInfo($"Achievement unlocked for player {playerId}: {achievement.Name}");
        }
        
        /// <summary>
        /// Check and update title progression for a player.
        /// </summary>
        private void CheckTitleProgression(string playerId)
        {
            if (!_enableTitleProgression) return;
            
            var reputation = GetOrCreatePlayerReputation(playerId);
            var unlockedAchievements = GetOrCreateUnlockedList(playerId);
            var specializations = GetOrCreatePlayerSpecialization(playerId);
            
            foreach (var titleProgression in _titleProgressions)
            {
                // Skip if already achieved
                if (reputation.EarnedTitles.Contains(titleProgression.TitleId))
                    continue;
                
                // Check reputation requirement
                if (reputation.ReputationPoints < titleProgression.MinimumReputation)
                    continue;
                
                // Check achievement requirements
                bool hasRequiredAchievements = true;
                foreach (var requiredAchievement in titleProgression.RequiredAchievements)
                {
                    if (!unlockedAchievements.Any(a => a.AchievementId == requiredAchievement))
                    {
                        hasRequiredAchievements = false;
                        break;
                    }
                }
                
                if (!hasRequiredAchievements)
                    continue;
                
                // Check specialization requirements
                bool hasRequiredSpecializations = true;
                foreach (var specialization in titleProgression.SpecializationRequirements)
                {
                    if (!specializations.FieldExpertise.TryGetValue(specialization.Key, out var level) || 
                        level < specialization.Value)
                    {
                        hasRequiredSpecializations = false;
                        break;
                    }
                }
                
                if (!hasRequiredSpecializations)
                    continue;
                
                // Award title
                AwardTitle(playerId, titleProgression);
            }
        }
        
        /// <summary>
        /// Award a title to a player.
        /// </summary>
        private void AwardTitle(string playerId, ScientificTitleProgression titleProgression)
        {
            var reputation = GetOrCreatePlayerReputation(playerId);
            reputation.EarnedTitles.Add(titleProgression.TitleId);
            reputation.CurrentTitle = titleProgression.TitleName;
            
            var recognition = new ScientificRecognition
            {
                RecognitionId = Guid.NewGuid().ToString(),
                PlayerId = playerId,
                RecognitionType = RecognitionType.Title,
                Title = titleProgression.TitleName,
                Description = titleProgression.Description,
                AwardedDate = DateTime.Now,
                PrestigeValue = titleProgression.PrestigeValue
            };
            
            AwardRecognition(playerId, recognition);
            
            LogInfo($"Title awarded to player {playerId}: {titleProgression.TitleName}");
        }
        
        /// <summary>
        /// Award recognition to a player.
        /// </summary>
        private void AwardRecognition(string playerId, ScientificRecognition recognition)
        {
            if (!_playerRecognitions.ContainsKey(playerId))
            {
                _playerRecognitions[playerId] = new List<ScientificRecognition>();
            }
            
            _playerRecognitions[playerId].Add(recognition);
            
            _onRecognitionEarned?.Raise(recognition);
            OnRecognitionEarned?.Invoke(recognition, playerId);
        }
        
        /// <summary>
        /// Update trait specialization for a player using trait expression data.
        /// </summary>
        private void UpdateTraitSpecialization(string playerId, string traitExpression)
        {
            var specialization = GetOrCreatePlayerSpecialization(playerId);
            
            // Update trait expertise (simplified parsing from string)
            var traits = new Dictionary<PlantTrait, float>
            {
                { PlantTrait.THCContent, ParseTraitValue(traitExpression, "THC") },
                { PlantTrait.CBDContent, ParseTraitValue(traitExpression, "CBD") },
                { PlantTrait.YieldPotential, ParseTraitValue(traitExpression, "Yield") },
                { PlantTrait.PlantHeight, ParseTraitValue(traitExpression, "Height") }
            };
            
            UpdateTraitSpecializationCore(playerId, specialization, traits);
        }
        
        /// <summary>
        /// Update trait specialization for a player using SimpleTraitData.
        /// </summary>
        private void UpdateTraitSpecialization(string playerId, SimpleTraitData traitExpression)
        {
            var specialization = GetOrCreatePlayerSpecialization(playerId);
            
            // Update trait expertise (simplified parsing from string)
            var traits = new Dictionary<PlantTrait, float>
            {
                { PlantTrait.THCContent, ParseTraitValue(traitExpression, "THC") },
                { PlantTrait.CBDContent, ParseTraitValue(traitExpression, "CBD") },
                { PlantTrait.YieldPotential, ParseTraitValue(traitExpression, "Yield") },
                { PlantTrait.PlantHeight, ParseTraitValue(traitExpression, "Height") }
            };
            
            UpdateTraitSpecializationCore(playerId, specialization, traits);
        }
        
        /// <summary>
        /// Core logic for updating trait specialization.
        /// </summary>
        private void UpdateTraitSpecializationCore(string playerId, SpecializationData specialization, Dictionary<PlantTrait, float> traits)
        {
            foreach (var trait in traits)
            {
                var traitKey = trait.Key.ToString();
                if (!specialization.TraitExpertise.ContainsKey(traitKey))
                {
                    specialization.TraitExpertise[traitKey] = 0f;
                }
                
                // Increase expertise based on trait quality
                var expertiseGain = trait.Value * 0.1f; // 10% of trait value
                specialization.TraitExpertise[traitKey] += expertiseGain;
                
                // Check for mastery
                if (specialization.TraitExpertise[traitKey] >= _masteryThreshold && 
                    !specialization.MasteredTraits.Contains(traitKey))
                {
                    specialization.MasteredTraits.Add(traitKey);
                    
                    var progress = GetOrCreatePlayerProgress(playerId);
                    progress.TraitMasteriesAchieved++;
                    
                    LogInfo($"Player {playerId} achieved mastery in {traitKey}");
                }
            }
        }
        
        /// <summary>
        /// Update research specialization for a player.
        /// </summary>
        private void UpdateResearchSpecialization(string playerId, ProjectChimera.Data.Genetics.Scientific.ResearchField field)
        {
            var specialization = GetOrCreatePlayerSpecialization(playerId);
            
            if (!specialization.FieldExpertise.ContainsKey(field))
            {
                specialization.FieldExpertise[field] = 0;
            }
            
            specialization.FieldExpertise[field]++;
            specialization.LastResearchDate = DateTime.Now;
        }
        
        /// <summary>
        /// Record a potential breakthrough.
        /// </summary>
        private void RecordPotentialBreakthrough(string playerId, BreedingEventData eventData)
        {
            if (eventData.OverallQuality >= _innovationThreshold)
            {
                var breakthrough = new ScientificBreakthrough
                {
                    BreakthroughId = Guid.NewGuid().ToString(),
                    BreakthroughName = "Exceptional Trait Expression",
                    Description = $"Achieved {eventData.OverallQuality:P1} trait expression quality",
                    BreakthroughType = BreakthroughType.TraitOptimization,
                    SignificanceLevel = eventData.OverallQuality,
                    ReputationValue = (int)(1000 * eventData.OverallQuality),
                    PrestigeValue = (int)(500 * eventData.OverallQuality)
                };
                
                RecordScientificBreakthrough(playerId, breakthrough);
            }
        }
        
        /// <summary>
        /// Log breeding discovery for scientific tracking.
        /// </summary>
        private void LogBreedingDiscovery(string playerId, BreedingEventData eventData)
        {
            var discovery = new ResearchContribution
            {
                ContributionId = Guid.NewGuid().ToString(),
                PlayerId = playerId,
                ContributionType = ContributionType.DataCollection,
                ResearchField = ResearchField.Genetics,
                DataPointsContributed = 1,
                QualityScore = eventData.OverallQuality,
                ContributionDate = DateTime.Now
            };
            
            RecordResearchContribution(playerId, discovery);
        }
        
        /// <summary>
        /// Check progressive achievements that update over time.
        /// </summary>
        private void CheckProgressiveAchievements()
        {
            if (!_enableProgressiveAchievements) return;
            
            foreach (var playerId in _playerProgress.Keys.ToList())
            {
                CheckAchievementUnlocks(playerId);
            }
        }
        
        /// <summary>
        /// Update specialization tracking for all players.
        /// </summary>
        private void UpdateSpecializationTracking()
        {
            if (!_enableSpecializationTracking) return;
            
            foreach (var playerId in _playerSpecializations.Keys.ToList())
            {
                var specialization = _playerSpecializations[playerId];
                specialization.LastUpdateTime = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Process milestone checks for all players.
        /// </summary>
        private void ProcessMilestoneChecks()
        {
            foreach (var playerId in _playerProgress.Keys.ToList())
            {
                CheckResearchMilestones(playerId);
            }
        }
        
        /// <summary>
        /// Check research milestones for a player.
        /// </summary>
        private void CheckResearchMilestones(string playerId)
        {
            var progress = _playerProgress[playerId];
            
            // Check breeding milestones
            var breedingMilestones = new[] { 10, 25, 50, 100, 250, 500, 1000 };
            foreach (var milestone in breedingMilestones)
            {
                if (progress.BreedingCrossesCompleted >= milestone && 
                    !progress.ReachedMilestones.Contains($"breeding-{milestone}"))
                {
                    var milestoneData = new ResearchMilestone
                    {
                        MilestoneId = $"breeding-{milestone}",
                        MilestoneName = $"{milestone} Breeding Crosses",
                        Description = $"Completed {milestone} breeding crosses",
                        MilestoneType = ResearchMilestoneType.BreedingGoalCompletion,
                        RequiredProgress = milestone,
                        CurrentProgress = milestone,
                        IsCompleted = true,
                        CompletionDate = DateTime.Now
                    };
                    
                    progress.ReachedMilestones.Add(milestoneData.MilestoneId);
                    progress.ReachedMilestonesCount++;
                    _onMilestoneReached?.Raise(milestoneData);
                    OnMilestoneReached?.Invoke(milestoneData, playerId);
                }
            }
        }
        
        /// <summary>
        /// Handle tournament completion events.
        /// </summary>
        private void OnTournamentCompleted(TournamentData tournament)
        {
            // Record tournament achievements
            _globalStatistics.TournamentsCompleted++;
        }
        
        /// <summary>
        /// Handle tournament results announcements.
        /// </summary>
        private void OnTournamentResultsAnnounced(TournamentResult results)
        {
            foreach (var ranking in results.Rankings)
            {
                var progress = GetOrCreatePlayerProgress(ranking.PlayerId);
                progress.TournamentParticipations++;
                
                if (ranking.Rank == 1)
                {
                    progress.TournamentWins++;
                }
                
                if (ranking.Rank <= 5)
                {
                    progress.TopFiveTournamentFinishes++;
                }
                
                CheckAchievementUnlocks(ranking.PlayerId);
            }
        }
        
        /// <summary>
        /// Handle breeding goal completion events.
        /// </summary>
        private void OnBreedingGoalCompleted(BreedingGoal goal)
        {
            // Would record goal achievements if we had player context
            _globalStatistics.BreedingGoalsCompleted++;
        }
        
        /// <summary>
        /// Get or create player achievement progress.
        /// </summary>
        private PlayerAchievementProgress GetOrCreatePlayerProgress(string playerId)
        {
            if (!_playerProgress.ContainsKey(playerId))
            {
                _playerProgress[playerId] = new PlayerAchievementProgress
                {
                    PlayerId = playerId,
                    FirstActivity = DateTime.Now
                };
            }
            
            return _playerProgress[playerId];
        }
        
        /// <summary>
        /// Get or create player unlocked achievements list.
        /// </summary>
        private List<ScientificAchievement> GetOrCreateUnlockedList(string playerId)
        {
            if (!_unlockedAchievements.ContainsKey(playerId))
            {
                _unlockedAchievements[playerId] = new List<ScientificAchievement>();
            }
            
            return _unlockedAchievements[playerId];
        }
        
        /// <summary>
        /// Get or create player reputation data.
        /// </summary>
        private ScientificReputation GetOrCreatePlayerReputation(string playerId)
        {
            if (!_playerReputations.ContainsKey(playerId))
            {
                _playerReputations[playerId] = new ScientificReputation
                {
                    PlayerId = playerId,
                    ReputationPoints = 0,
                    CurrentTitle = "Novice Researcher"
                };
            }
            
            return _playerReputations[playerId];
        }
        
        /// <summary>
        /// Get or create player specialization data.
        /// </summary>
        private SpecializationData GetOrCreatePlayerSpecialization(string playerId)
        {
            if (!_playerSpecializations.ContainsKey(playerId))
            {
                _playerSpecializations[playerId] = new SpecializationData
                {
                    PlayerId = playerId
                };
            }
            
            return _playerSpecializations[playerId];
        }
        
        /// <summary>
        /// Get player achievements.
        /// </summary>
        public List<ScientificAchievement> GetPlayerAchievements(string playerId)
        {
            return GetOrCreateUnlockedList(playerId);
        }
        
        /// <summary>
        /// Get player reputation.
        /// </summary>
        public ScientificReputation GetPlayerReputation(string playerId)
        {
            return GetOrCreatePlayerReputation(playerId);
        }
        
        /// <summary>
        /// Get player specializations.
        /// </summary>
        public SpecializationData GetPlayerSpecializations(string playerId)
        {
            return GetOrCreatePlayerSpecialization(playerId);
        }
        
        /// <summary>
        /// Get global breakthroughs.
        /// </summary>
        public List<ScientificBreakthrough> GetGlobalBreakthroughs()
        {
            return new List<ScientificBreakthrough>(_globalBreakthroughs);
        }
        
        /// <summary>
        /// Get achievement statistics.
        /// </summary>
        public ScientificStatistics GetStatistics()
        {
            return _globalStatistics;
        }
        
        /// <summary>
        /// Parse trait value from expression string.
        /// </summary>
        private float ParseTraitValue(string expression, string traitName)
        {
            // Simple parsing - look for trait name followed by a value
            if (string.IsNullOrEmpty(expression)) return 0f;
            
            // For now, return random values based on trait name
            // In a real implementation, this would parse the actual expression
            return traitName switch
            {
                "THC" => UnityEngine.Random.Range(0.1f, 0.9f),
                "CBD" => UnityEngine.Random.Range(0.1f, 0.8f),
                "Yield" => UnityEngine.Random.Range(0.3f, 1.0f),
                "Height" => UnityEngine.Random.Range(0.5f, 1.2f),
                _ => 0.5f
            };
        }
        
        /// <summary>
        /// Parse trait value from SimpleTraitData based on trait name.
        /// </summary>
        private float ParseTraitValue(SimpleTraitData traitData, string traitName)
        {
            if (traitData == null) return 0f;
            
            return traitName switch
            {
                "THC" => traitData.THCExpression,
                "CBD" => traitData.CBDExpression,
                "Yield" => traitData.YieldExpression,
                "Height" => traitData.HeightExpression,
                _ => float.TryParse(traitData.Value, out float result) ? result : 0f
            };
        }
        
        protected override void OnManagerShutdown()
        {
            _availableAchievements.Clear();
            _playerProgress.Clear();
            _unlockedAchievements.Clear();
            _globalBreakthroughs.Clear();
            _playerContributions.Clear();
            _playerSpecializations.Clear();
            _playerReputations.Clear();
            _playerRecognitions.Clear();
            
            LogInfo("Scientific Achievement Manager shutdown complete");
        }
    }
}