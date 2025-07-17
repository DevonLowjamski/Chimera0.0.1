using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages the skill tree system including skill progression, experience tracking,
    /// skill unlocks, synergies, and player advancement through the cannabis cultivation
    /// expertise progression system.
    /// </summary>
    public class SkillTreeManager : ChimeraManager
    {
        [Header("Skill Tree Configuration")]
        [SerializeField] private List<SkillNodeSO> _allSkillNodes = new List<SkillNodeSO>();
        [SerializeField] private List<SkillTreeBranch> _skillBranches = new List<SkillTreeBranch>();
        [SerializeField] private List<SkillSynergy> _skillSynergies = new List<SkillSynergy>();
        [SerializeField] private SkillTreeSettings _treeSettings;
        
        [Header("Player Progression")]
        [SerializeField] private PlayerProgression _playerProgression;
        [SerializeField] private ExperienceSettings _experienceSettings;
        [SerializeField] private List<ExpertiseArea> _expertiseAreas = new List<ExpertiseArea>();
        
        [Header("Learning Acceleration")]
        [SerializeField] private List<LearningAccelerator> _learningAccelerators = new List<LearningAccelerator>();
        [SerializeField] private List<LearningPath> _learningPaths = new List<LearningPath>();
        
        [Header("Events")]
        [SerializeField] private SimpleGameEventSO _skillUnlockedEvent;
        [SerializeField] private SimpleGameEventSO _levelUpEvent;
        [SerializeField] private SimpleGameEventSO _expertiseAchievedEvent;
        [SerializeField] private SimpleGameEventSO _synergyActivatedEvent;
        
        // Runtime Data
        private Dictionary<SkillNodeSO, SkillState> _skillStates;
        private Dictionary<SkillCategory, float> _categoryExperience;
        private Dictionary<SkillDomain, float> _domainMastery;
        private List<ActiveSynergy> _activeSynergies;
        private List<ExpertiseAchievement> _achievedExpertise;
        private Queue<SkillEvent> _recentSkillEvents;
        private float _timeSinceLastUpdate;
        
        public PlayerProgression PlayerProgression => _playerProgression;
        public List<ExpertiseAchievement> AchievedExpertise => _achievedExpertise;
        public List<ActiveSynergy> ActiveSynergies => _activeSynergies;
        
        // Events
        public System.Action<SkillNodeSO, int> OnSkillLevelChanged; // skill, newLevel
        public System.Action<SkillNodeSO> OnSkillUnlocked;
        public System.Action<int, int> OnPlayerLevelChanged; // oldLevel, newLevel
        public System.Action<ExpertiseArea, float> OnExpertiseAchieved; // area, masteryLevel
        public System.Action<SkillSynergy> OnSynergyActivated;
        
        protected override void OnManagerInitialize()
        {
            _skillStates = new Dictionary<SkillNodeSO, SkillState>();
            _categoryExperience = new Dictionary<SkillCategory, float>();
            _domainMastery = new Dictionary<SkillDomain, float>();
            _activeSynergies = new List<ActiveSynergy>();
            _achievedExpertise = new List<ExpertiseAchievement>();
            _recentSkillEvents = new Queue<SkillEvent>();
            
            InitializeSkillStates();
            InitializeCategoryExperience();
            InitializeDomainMastery();
            InitializePlayerProgression();
            
            Debug.Log("SkillTreeManager initialized successfully");
        }
        
        /// <summary>
        /// Initialize with external configuration (for genetics system integration)
        /// </summary>
        public void Initialize(object config)
        {
            // For genetics system integration - configuration will be handled internally
            if (!IsInitialized)
            {
                OnManagerInitialize();
            }
            
            if (config != null)
            {
                Debug.Log($"SkillTreeManager initialized with external config: {config.GetType().Name}");
            }
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup resources
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastUpdate += Time.deltaTime;
            
            float gameTimeDelta = GameManager.Instance.GetManager<TimeManager>().GetScaledDeltaTime();
            
            if (_timeSinceLastUpdate >= _treeSettings.UpdateInterval * gameTimeDelta)
            {
                UpdateSkillStates();
                CheckSynergyActivations();
                UpdateExpertiseProgress();
                ProcessSkillEvents();
                
                _timeSinceLastUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Adds experience to a specific skill.
        /// </summary>
        public void AddSkillExperience(SkillNodeSO skill, float experience, ExperienceSource source = ExperienceSource.Gameplay)
        {
            if (!_skillStates.ContainsKey(skill))
            {
                Debug.LogWarning($"Skill {skill.name} not found in skill states");
                return;
            }
            
            var skillState = _skillStates[skill];
            
            // Apply learning accelerators
            float modifiedExperience = ApplyLearningModifiers(skill, experience, source);
            
            // Add experience
            float oldExperience = skillState.CurrentExperience;
            skillState.CurrentExperience += modifiedExperience;
            
            // Update category experience
            if (_categoryExperience.ContainsKey(skill.SkillCategory))
            {
                _categoryExperience[skill.SkillCategory] += modifiedExperience;
            }
            
            // Update domain mastery
            if (_domainMastery.ContainsKey(skill.Domain))
            {
                _domainMastery[skill.Domain] += modifiedExperience * 0.1f; // Domains grow slower
            }
            
            // Check for level up
            CheckSkillLevelUp(skill, skillState);
            
            // Update player level
            UpdatePlayerLevel(modifiedExperience);
            
            // Record skill event
            RecordSkillEvent(new SkillEvent
            {
                EventType = SkillEventType.Experience_Gained,
                Skill = skill,
                ExperienceGained = modifiedExperience,
                Source = source,
                Timestamp = System.DateTime.Now
            });
        }
        
        /// <summary>
        /// Attempts to unlock a skill if prerequisites are met.
        /// </summary>
        public bool TryUnlockSkill(SkillNodeSO skill)
        {
            if (!_skillStates.ContainsKey(skill))
                return false;
            
            var skillState = _skillStates[skill];
            
            if (skillState.IsUnlocked)
                return true; // Already unlocked
            
            // Check prerequisites
            if (!ArePrerequisitesMet(skill))
                return false;
            
            // Check skill point cost
            if (_playerProgression.AvailableSkillPoints < skill.SkillPointCost)
                return false;
            
            // Unlock the skill
            skillState.IsUnlocked = true;
            skillState.UnlockDate = System.DateTime.Now;
            _playerProgression.AvailableSkillPoints -= skill.SkillPointCost;
            
            // Record unlock event
            RecordSkillEvent(new SkillEvent
            {
                EventType = SkillEventType.Skill_Unlocked,
                Skill = skill,
                Timestamp = System.DateTime.Now
            });
            
            OnSkillUnlocked?.Invoke(skill);
            _skillUnlockedEvent?.Raise();
            
            return true;
        }
        
        /// <summary>
        /// Gets the current level of a skill.
        /// </summary>
        public int GetSkillLevel(SkillNodeSO skill)
        {
            if (_skillStates.ContainsKey(skill))
            {
                return _skillStates[skill].CurrentLevel;
            }
            return 0;
        }
        
        /// <summary>
        /// Gets the current experience of a skill.
        /// </summary>
        public float GetSkillExperience(SkillNodeSO skill)
        {
            if (_skillStates.ContainsKey(skill))
            {
                return _skillStates[skill].CurrentExperience;
            }
            return 0f;
        }
        
        /// <summary>
        /// Checks if a skill is unlocked.
        /// </summary>
        public bool IsSkillUnlocked(SkillNodeSO skill)
        {
            if (_skillStates.ContainsKey(skill))
            {
                return _skillStates[skill].IsUnlocked;
            }
            return false;
        }
        
        /// <summary>
        /// Gets all unlocked skills in a category.
        /// </summary>
        public List<SkillNodeSO> GetUnlockedSkillsInCategory(SkillCategory category)
        {
            return _allSkillNodes
                .Where(skill => skill.SkillCategory == category && IsSkillUnlocked(skill))
                .ToList();
        }
        
        /// <summary>
        /// Gets the player's mastery level in a domain.
        /// </summary>
        public float GetDomainMastery(SkillDomain domain)
        {
            return _domainMastery.ContainsKey(domain) ? _domainMastery[domain] : 0f;
        }
        
        /// <summary>
        /// Gets the total skill bonus for a specific effect type.
        /// </summary>
        public float GetTotalSkillBonus(SkillEffectType effectType)
        {
            float totalBonus = 0f;
            
            foreach (var kvp in _skillStates)
            {
                var skill = kvp.Key;
                var state = kvp.Value;
                
                if (state.IsUnlocked && state.CurrentLevel > 0)
                {
                    var skillEffect = skill.Effects.Find(e => e.EffectType == effectType);
                    if (skillEffect != null)
                    {
                        float effectValue = skillEffect.BaseValue + (skillEffect.ValuePerLevel * state.CurrentLevel);
                        totalBonus += effectValue;
                    }
                }
            }
            
            // Apply synergy bonuses
            foreach (var synergy in _activeSynergies)
            {
                var synergyEffect = synergy.SynergyDefinition.SynergyEffects.Find(e => e.EffectType == effectType);
                if (synergyEffect != null)
                {
                    totalBonus *= synergyEffect.BonusMultiplier;
                }
            }
            
            return totalBonus;
        }
        
        /// <summary>
        /// Applies a learning accelerator to boost skill progression.
        /// </summary>
        public bool ApplyLearningAccelerator(LearningAccelerator accelerator)
        {
            // Check if requirements are met
            if (!AreLearningAcceleratorRequirementsMet(accelerator))
                return false;
            
            var activeAccelerator = new ActiveLearningAccelerator
            {
                Accelerator = accelerator,
                StartDate = System.DateTime.Now,
                ExpirationDate = System.DateTime.Now.AddHours(accelerator.DurationHours),
                RemainingUses = accelerator.AcceleratorType == AcceleratorType.Time_Acceleration ? 
                    (int)accelerator.DurationDays : -1 // -1 for unlimited uses within duration
            };
            
            _playerProgression.ActiveAccelerators.Add(activeAccelerator);
            
            return true;
        }
        
        /// <summary>
        /// Gets recommended skills for the player based on current progression.
        /// </summary>
        public List<SkillNodeSO> GetRecommendedSkills(int maxRecommendations = 5)
        {
            var recommendations = new List<SkillRecommendation>();
            
            foreach (var skill in _allSkillNodes)
            {
                if (!IsSkillUnlocked(skill) && ArePrerequisitesMet(skill))
                {
                    float score = CalculateRecommendationScore(skill);
                    recommendations.Add(new SkillRecommendation
                    {
                        Skill = skill,
                        Score = score,
                        Reasoning = GenerateRecommendationReasoning(skill, score)
                    });
                }
            }
            
            return recommendations
                .OrderByDescending(r => r.Score)
                .Take(maxRecommendations)
                .Select(r => r.Skill)
                .ToList();
        }
        
        /// <summary>
        /// Gets the optimal learning path for a specific goal.
        /// </summary>
        public LearningPath GetOptimalLearningPath(LearningPathType pathType)
        {
            return _learningPaths.Find(path => path.PathType == pathType);
        }
        
        /// <summary>
        /// Calculates time to master a skill based on current progression rate.
        /// </summary>
        public float CalculateTimeToMaster(SkillNodeSO skill)
        {
            if (!_skillStates.ContainsKey(skill))
                return float.MaxValue;
            
            var skillState = _skillStates[skill];
            
            if (!skillState.IsUnlocked)
                return float.MaxValue;
            
            float currentExperience = skillState.CurrentExperience;
            float experienceToMaster = skill.GetExperienceRequiredForLevel(skill.MaxSkillLevel);
            float remainingExperience = experienceToMaster - currentExperience;
            
            // Calculate average experience gain rate
            float averageGainRate = CalculateAverageExperienceGainRate(skill);
            
            if (averageGainRate <= 0)
                return float.MaxValue;
            
            return remainingExperience / averageGainRate; // Days to master
        }
        
        private void InitializeSkillStates()
        {
            foreach (var skill in _allSkillNodes)
            {
                var skillState = new SkillState
                {
                    Skill = skill,
                    CurrentLevel = 0,
                    CurrentExperience = 0f,
                    IsUnlocked = skill.IsStartingSkill,
                    UnlockDate = skill.IsStartingSkill ? System.DateTime.Now : System.DateTime.MinValue,
                    ExperienceHistory = new List<ExperienceGain>()
                };
                
                _skillStates[skill] = skillState;
            }
        }
        
        private void InitializeCategoryExperience()
        {
            foreach (SkillCategory category in System.Enum.GetValues(typeof(SkillCategory)))
            {
                _categoryExperience[category] = 0f;
            }
        }
        
        private void InitializeDomainMastery()
        {
            foreach (SkillDomain domain in System.Enum.GetValues(typeof(SkillDomain)))
            {
                _domainMastery[domain] = 0f;
            }
        }
        
        private void InitializePlayerProgression()
        {
            if (_playerProgression == null)
            {
                _playerProgression = new PlayerProgression
                {
                    PlayerLevel = 1,
                    TotalExperience = 0f,
                    AvailableSkillPoints = _treeSettings.StartingSkillPoints,
                    ActiveAccelerators = new List<ActiveLearningAccelerator>()
                };
            }
        }
        
        private void UpdateSkillStates()
        {
            // Update active learning accelerators
            for (int i = _playerProgression.ActiveAccelerators.Count - 1; i >= 0; i--)
            {
                var accelerator = _playerProgression.ActiveAccelerators[i];
                
                if (System.DateTime.Now > accelerator.ExpirationDate || accelerator.RemainingUses == 0)
                {
                    _playerProgression.ActiveAccelerators.RemoveAt(i);
                }
            }
        }
        
        private void CheckSynergyActivations()
        {
            foreach (var synergy in _skillSynergies)
            {
                bool isActive = IsSynergyActive(synergy);
                bool wasActive = _activeSynergies.Any(s => s.SynergyDefinition == synergy);
                
                if (isActive && !wasActive)
                {
                    // Activate synergy
                    var activeSynergy = new ActiveSynergy
                    {
                        SynergyDefinition = synergy,
                        ActivationDate = System.DateTime.Now,
                        EffectiveStrength = CalculateSynergyStrength(synergy)
                    };
                    
                    _activeSynergies.Add(activeSynergy);
                    OnSynergyActivated?.Invoke(synergy);
                    _synergyActivatedEvent?.Raise();
                }
                else if (!isActive && wasActive)
                {
                    // Deactivate synergy
                    _activeSynergies.RemoveAll(s => s.SynergyDefinition == synergy);
                }
            }
        }
        
        private void UpdateExpertiseProgress()
        {
            foreach (var expertiseArea in _expertiseAreas)
            {
                float currentMastery = CalculateExpertiseMastery(expertiseArea);
                
                bool wasAchieved = _achievedExpertise.Any(e => e.ExpertiseArea == expertiseArea);
                bool isAchieved = currentMastery >= expertiseArea.ExpertiseThreshold;
                
                if (isAchieved && !wasAchieved)
                {
                    var achievement = new ExpertiseAchievement
                    {
                        ExpertiseArea = expertiseArea,
                        MasteryLevel = currentMastery,
                        AchievementDate = System.DateTime.Now,
                        ActiveBenefits = expertiseArea.Benefits.ToList()
                    };
                    
                    _achievedExpertise.Add(achievement);
                    OnExpertiseAchieved?.Invoke(expertiseArea, currentMastery);
                    _expertiseAchievedEvent?.Raise();
                }
            }
        }
        
        private void ProcessSkillEvents()
        {
            // Clean up old events
            while (_recentSkillEvents.Count > 0 && 
                   (System.DateTime.Now - _recentSkillEvents.Peek().Timestamp).TotalDays > 30)
            {
                _recentSkillEvents.Dequeue();
            }
        }
        
        private void CheckSkillLevelUp(SkillNodeSO skill, SkillState skillState)
        {
            int newLevel = CalculateLevelFromExperience(skill, skillState.CurrentExperience);
            
            if (newLevel > skillState.CurrentLevel)
            {
                int oldLevel = skillState.CurrentLevel;
                skillState.CurrentLevel = newLevel;
                
                OnSkillLevelChanged?.Invoke(skill, newLevel);
                
                // Award skill points for level up
                int skillPointsAwarded = CalculateSkillPointsAwarded(skill, oldLevel, newLevel);
                _playerProgression.AvailableSkillPoints += skillPointsAwarded;
                
                RecordSkillEvent(new SkillEvent
                {
                    EventType = SkillEventType.Level_Up,
                    Skill = skill,
                    NewLevel = newLevel,
                    Timestamp = System.DateTime.Now
                });
            }
        }
        
        private void UpdatePlayerLevel(float experienceGained)
        {
            float oldTotalExperience = _playerProgression.TotalExperience;
            _playerProgression.TotalExperience += experienceGained;
            
            int newPlayerLevel = _experienceSettings.CalculatePlayerLevel(_playerProgression.TotalExperience);
            
            if (newPlayerLevel > _playerProgression.PlayerLevel)
            {
                int oldLevel = _playerProgression.PlayerLevel;
                _playerProgression.PlayerLevel = newPlayerLevel;
                
                // Award skill points for player level up
                int skillPointsAwarded = (newPlayerLevel - oldLevel) * _treeSettings.SkillPointsPerLevel;
                _playerProgression.AvailableSkillPoints += skillPointsAwarded;
                
                OnPlayerLevelChanged?.Invoke(oldLevel, newPlayerLevel);
                _levelUpEvent?.Raise();
            }
        }
        
        private bool ArePrerequisitesMet(SkillNodeSO skill)
        {
            // Check prerequisite skills
            foreach (var prerequisite in skill.PrerequisiteSkills)
            {
                if (!IsSkillUnlocked(prerequisite) || GetSkillLevel(prerequisite) < skill.LearningRequirements.MinimumPrerequisiteLevel)
                {
                    return false;
                }
            }
            
            // Check player level requirement
            if (_playerProgression.PlayerLevel < skill.LearningRequirements.MinimumPlayerLevel)
            {
                return false;
            }
            
            // Check category experience requirement
            if (_categoryExperience.ContainsKey(skill.SkillCategory))
            {
                if (_categoryExperience[skill.SkillCategory] < skill.LearningRequirements.MinimumExperience)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private float ApplyLearningModifiers(SkillNodeSO skill, float baseExperience, ExperienceSource source)
        {
            float modifiedExperience = baseExperience;
            
            // Apply expertise bonuses
            foreach (var achievement in _achievedExpertise)
            {
                if (achievement.ExpertiseArea.RelevantCategories.Contains(skill.SkillCategory))
                {
                    var experienceBenefit = achievement.ActiveBenefits.Find(b => b.BenefitType == ExpertiseBenefitType.Experience_Multiplier);
                    if (experienceBenefit != null)
                    {
                        modifiedExperience *= (1f + experienceBenefit.BenefitValue);
                    }
                }
            }
            
            // Apply learning accelerators
            foreach (var accelerator in _playerProgression.ActiveAccelerators)
            {
                if (accelerator.Accelerator.ApplicableCategories.Contains(skill.SkillCategory))
                {
                    modifiedExperience *= accelerator.Accelerator.SpeedMultiplier;
                    
                    if (accelerator.RemainingUses > 0)
                    {
                        accelerator.RemainingUses--;
                    }
                }
            }
            
            // Apply source multipliers
            switch (source)
            {
                case ExperienceSource.Research:
                    modifiedExperience *= _experienceSettings.ResearchMultiplier;
                    break;
                case ExperienceSource.Teaching:
                    modifiedExperience *= _experienceSettings.TeachingMultiplier;
                    break;
                case ExperienceSource.Collaboration:
                    modifiedExperience *= _experienceSettings.CollaborationMultiplier;
                    break;
            }
            
            return modifiedExperience;
        }
        
        private bool IsSynergyActive(SkillSynergy synergy)
        {
            if (!IsSkillUnlocked(synergy.PrimarySkill) || !IsSkillUnlocked(synergy.SecondarySkill))
                return false;
            
            int primaryLevel = GetSkillLevel(synergy.PrimarySkill);
            int secondaryLevel = GetSkillLevel(synergy.SecondarySkill);
            
            foreach (var effect in synergy.SynergyEffects)
            {
                if (effect.RequiresBothSkills)
                {
                    if (primaryLevel + secondaryLevel < effect.MinimumCombinedLevel)
                        return false;
                }
            }
            
            return true;
        }
        
        private float CalculateSynergyStrength(SkillSynergy synergy)
        {
            int primaryLevel = GetSkillLevel(synergy.PrimarySkill);
            int secondaryLevel = GetSkillLevel(synergy.SecondarySkill);
            
            float strength = synergy.SynergyStrength;
            
            // Synergy strength increases with skill levels
            float levelBonus = (primaryLevel + secondaryLevel) * 0.05f;
            strength += levelBonus;
            
            return Mathf.Clamp01(strength);
        }
        
        private float CalculateExpertiseMastery(ExpertiseArea expertiseArea)
        {
            var relevantSkills = _allSkillNodes
                .Where(skill => expertiseArea.RelevantCategories.Contains(skill.SkillCategory))
                .ToList();
            
            if (relevantSkills.Count == 0)
                return 0f;
            
            int unlockedCount = relevantSkills.Count(skill => IsSkillUnlocked(skill));
            int masteredCount = relevantSkills.Count(skill => GetSkillLevel(skill) >= skill.MaxSkillLevel);
            
            // Check if minimum requirements are met
            if (unlockedCount < expertiseArea.RequiredSkillCount || 
                masteredCount < expertiseArea.RequiredMasteryCount)
            {
                return 0f;
            }
            
            // Calculate mastery based on average skill levels
            float totalLevels = relevantSkills.Sum(skill => GetSkillLevel(skill));
            float maxPossibleLevels = relevantSkills.Sum(skill => skill.MaxSkillLevel);
            
            return maxPossibleLevels > 0 ? totalLevels / maxPossibleLevels : 0f;
        }
        
        private bool AreLearningAcceleratorRequirementsMet(LearningAccelerator accelerator)
        {
            foreach (var requirement in accelerator.Requirements)
            {
                switch (requirement.RequirementType)
                {
                    case AcceleratorRequirementType.Money_Investment:
                        // Check with economy manager if enough money
                        break;
                    case AcceleratorRequirementType.Skill_Level:
                        // Check if player has required skill level
                        break;
                    case AcceleratorRequirementType.Reputation_Level:
                        // Check with relationship manager for reputation
                        break;
                    default:
                        // Other requirements would be checked here
                        break;
                }
            }
            
            return true; // Simplified for now
        }
        
        private float CalculateRecommendationScore(SkillNodeSO skill)
        {
            float score = 0f;
            
            // Base score from skill tier (priority)
            score += skill.SkillPriority switch
            {
                1 => 0.4f,  // Basic tier
                2 => 0.6f,  // Intermediate tier
                3 => 0.8f,  // Advanced tier
                4 => 1f,    // Master tier
                _ => 0.3f
            };
            
            // Bonus for skills in player's strongest categories
            var strongestCategory = _categoryExperience.OrderByDescending(kvp => kvp.Value).First().Key;
            if (skill.SkillCategory == strongestCategory)
                score += 0.3f;
            
            // Bonus for skills that enable synergies
            if (_skillSynergies.Any(s => s.PrimarySkill == skill || s.SecondarySkill == skill))
                score += 0.2f;
            
            // Penalty for expensive skills early on
            if (_playerProgression.PlayerLevel < 10 && skill.SkillPointCost > 3)
                score -= 0.2f;
            
            return Mathf.Clamp01(score);
        }
        
        private string GenerateRecommendationReasoning(SkillNodeSO skill, float score)
        {
            if (score > 0.8f)
                return "Highly recommended based on your current progression and expertise focus.";
            else if (score > 0.6f)
                return "Good choice that fits well with your developing skills.";
            else if (score > 0.4f)
                return "Useful skill that could complement your current abilities.";
            else
                return "Consider this skill for specialized builds or future development.";
        }
        
        private float CalculateAverageExperienceGainRate(SkillNodeSO skill)
        {
            if (!_skillStates.ContainsKey(skill))
                return 0f;
            
            var skillState = _skillStates[skill];
            var recentGains = skillState.ExperienceHistory
                .Where(gain => (System.DateTime.Now - gain.Timestamp).TotalDays <= 7)
                .ToList();
            
            if (recentGains.Count == 0)
                return 0f;
            
            float totalGain = recentGains.Sum(gain => gain.Amount);
            return totalGain / 7f; // Average per day
        }
        
        private int CalculateSkillPointsAwarded(SkillNodeSO skill, int oldLevel, int newLevel)
        {
            int levelsGained = newLevel - oldLevel;
            
            // Award more skill points for higher tier skills
            return skill.SkillTier switch
            {
                1 => levelsGained,     // Novice
                2 => levelsGained * 2, // Apprentice
                3 => levelsGained * 3, // Expert
                4 => levelsGained * 4, // Master
                _ => levelsGained
            };
        }
        
        private void RecordSkillEvent(SkillEvent skillEvent)
        {
            _recentSkillEvents.Enqueue(skillEvent);
            
            // Also record in skill state if applicable
            if (skillEvent.Skill != null && _skillStates.ContainsKey(skillEvent.Skill))
            {
                var skillState = _skillStates[skillEvent.Skill];
                
                if (skillEvent.EventType == SkillEventType.Experience_Gained && skillEvent.ExperienceGained > 0)
                {
                    skillState.ExperienceHistory.Add(new ExperienceGain
                    {
                        Amount = skillEvent.ExperienceGained,
                        Source = skillEvent.Source,
                        Timestamp = skillEvent.Timestamp
                    });
                    
                    // Keep only recent history
                    var cutoffDate = System.DateTime.Now.AddDays(-30);
                    skillState.ExperienceHistory.RemoveAll(gain => gain.Timestamp < cutoffDate);
                }
            }
        }
        
        private int CalculateLevelFromExperience(SkillNodeSO skill, float experience)
        {
            // Calculate which level the current experience corresponds to
            int level = 1;
            
            while (level < skill.MaxSkillLevel)
            {
                float requiredExperience = skill.GetExperienceRequiredForLevel(level + 1);
                if (experience < requiredExperience)
                    break;
                level++;
            }
            
            return level;
        }
    }
    
    [System.Serializable]
    public class SkillTreeSettings
    {
        [Range(0.1f, 5f)] public float UpdateInterval = 1f; // In-game days
        [Range(1, 20)] public int StartingSkillPoints = 5;
        [Range(1, 10)] public int SkillPointsPerLevel = 2;
        [Range(0f, 1f)] public float SynergyDiscoveryChance = 0.1f;
        public bool EnableAutoRecommendations = true;
    }
    
    [System.Serializable]
    public class ExperienceSettings
    {
        [Range(1f, 5f)] public float ResearchMultiplier = 1.5f;
        [Range(1f, 5f)] public float TeachingMultiplier = 2f;
        [Range(1f, 5f)] public float CollaborationMultiplier = 1.3f;
        [Range(1f, 5f)] public float InnovationMultiplier = 2.5f;
        public AnimationCurve PlayerLevelCurve;
        
        public int CalculatePlayerLevel(float totalExperience)
        {
            if (PlayerLevelCurve == null)
            {
                // Default formula: level = sqrt(totalExperience / 1000) + 1
                return Mathf.FloorToInt(Mathf.Sqrt(totalExperience / 1000f)) + 1;
            }
            
            return Mathf.FloorToInt(PlayerLevelCurve.Evaluate(totalExperience / 10000f) * 100f) + 1;
        }
    }
    
    [System.Serializable]
    public class PlayerProgression
    {
        public int PlayerLevel = 1;
        public float TotalExperience = 0f;
        public int AvailableSkillPoints = 5;
        public List<ActiveLearningAccelerator> ActiveAccelerators = new List<ActiveLearningAccelerator>();
        public System.DateTime ProgressionStartDate = System.DateTime.Now;
    }
    
    [System.Serializable]
    public class SkillState
    {
        public SkillNodeSO Skill;
        public int CurrentLevel = 0;
        public float CurrentExperience = 0f;
        public bool IsUnlocked = false;
        public System.DateTime UnlockDate;
        public List<ExperienceGain> ExperienceHistory = new List<ExperienceGain>();
    }
    
    [System.Serializable]
    public class ExperienceGain
    {
        public float Amount;
        public ExperienceSource Source;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class ActiveSynergy
    {
        public SkillSynergy SynergyDefinition;
        public System.DateTime ActivationDate;
        public float EffectiveStrength;
    }
    
    [System.Serializable]
    public class ExpertiseAchievement
    {
        public ExpertiseArea ExpertiseArea;
        public float MasteryLevel;
        public System.DateTime AchievementDate;
        public List<ExpertiseBenefit> ActiveBenefits = new List<ExpertiseBenefit>();
    }
    
    [System.Serializable]
    public class ActiveLearningAccelerator
    {
        public LearningAccelerator Accelerator;
        public System.DateTime StartDate;
        public System.DateTime ExpirationDate;
        public int RemainingUses = -1; // -1 for unlimited
    }
    
    [System.Serializable]
    public class SkillRecommendation
    {
        public SkillNodeSO Skill;
        public float Score;
        public string Reasoning;
    }
    
    [System.Serializable]
    public class SkillEvent
    {
        public SkillEventType EventType;
        public SkillNodeSO Skill;
        public float ExperienceGained;
        public int NewLevel;
        public ExperienceSource Source;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class MentorshipBenefit
    {
        public string BenefitName;
        public MentorshipBenefitType BenefitType;
        public float BenefitValue;
        public string BenefitDescription;
    }
    
    // Note: ExperienceSource enum is now defined in ProgressionDataStructures.cs
    
    public enum SkillEventType
    {
        Experience_Gained,
        Level_Up,
        Skill_Unlocked,
        Synergy_Activated,
        Expertise_Achieved,
        Mastery_Completed
    }
    
    public enum MentorshipBenefitType
    {
        Experience_Boost,
        Skill_Unlock,
        Research_Access,
        Network_Expansion,
        Quality_Improvement,
        Innovation_Support
    }
    

}