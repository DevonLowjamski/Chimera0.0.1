using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Equipment;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Linq;
using System;
using SkillCategory = ProjectChimera.Data.Progression.SkillCategory;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Comprehensive progression management system for Project Chimera.
    /// Handles skill trees, experience tracking, research projects, and player advancement.
    /// Integrates with cultivation, genetics, and facility management systems.
    /// </summary>
    public class ProgressionManager : ChimeraManager
    {
        [Header("Progression Configuration")]
        [SerializeField] private bool _enableSkillProgression = true;
        [SerializeField] private bool _enableResearchProgression = true;
        [SerializeField] private bool _enableAchievementSystem = true;
        [SerializeField] private float _experienceGainMultiplier = 1f;
        
        [Header("Player Level Configuration")]
        [SerializeField] private int _maxPlayerLevel = 100;
        [SerializeField] private AnimationCurve _playerLevelCurve;
        [SerializeField] private int _skillPointsPerLevel = 2;
        [SerializeField] private float _baseExperiencePerLevel = 1000f;
        
        [Header("Experience Sources")]
        [SerializeField] private ExperienceRewards _experienceRewards;
        [SerializeField] private float _skillUsageExperienceMultiplier = 0.1f;
        [SerializeField] private bool _enablePassiveExperience = false;
        [SerializeField] private float _passiveExperienceRate = 10f; // per hour
        
        [Header("Research Configuration")]
        [SerializeField] private int _maxSimultaneousResearch = 3;
        [SerializeField] private bool _allowResearchAcceleration = true;
        [SerializeField] private float _researchSpeedMultiplier = 1f;
        [SerializeField] private bool _enableResearchCollaboration = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onPlayerLevelUp;
        [SerializeField] private SimpleGameEventSO _onSkillLearned;
        [SerializeField] private SimpleGameEventSO _onSkillLevelUp;
        [SerializeField] private SimpleGameEventSO _onResearchCompleted;
        [SerializeField] private SimpleGameEventSO _onAchievementUnlocked;
        [SerializeField] private SimpleGameEventSO _onMasteryAchieved;
        
        // Player Progression Data
        private LegacyPlayerProgressionProfile _playerProfile;
        private Dictionary<SkillNodeSO, PlayerSkillData> _playerSkills = new Dictionary<SkillNodeSO, PlayerSkillData>();
        private Dictionary<ResearchProjectSO, ActiveResearchProject> _activeResearch = new Dictionary<ResearchProjectSO, ActiveResearchProject>();
        private List<CompletedResearchProject> _completedResearch = new List<CompletedResearchProject>();
        private Dictionary<string, AchievementProgress> _achievements = new Dictionary<string, AchievementProgress>();
        private Dictionary<SkillCategory, SpecializationProgress> _specializations = new Dictionary<SkillCategory, SpecializationProgress>();
        
        // Experience and Skill Points Management
        private float _totalExperience = 0f;
        private float _currentLevelExperience = 0f;
        private int _availableSkillPoints = 0;
        private float _lastSkillUsageTime;
        private float _lastPassiveExperienceTime;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        // Public Properties
        public int PlayerLevel => _playerProfile?.PlayerLevel ?? 1;
        public float TotalExperience => _totalExperience;
        public float CurrentExperience => _currentLevelExperience;
        public int AvailableSkillPoints => _availableSkillPoints;
        public int ActiveResearchProjects => _activeResearch.Count;
        public int CompletedResearchProjects => _completedResearch.Count;
        public LegacyPlayerProgressionProfile PlayerProfile => _playerProfile;
        public Dictionary<SkillNodeSO, PlayerSkillData> PlayerSkills => _playerSkills;
        
        protected override void OnManagerInitialize()
        {
            InitializePlayerProfile();
            _lastSkillUsageTime = Time.time;
            _lastPassiveExperienceTime = Time.time;
            
            LogInfo("ProgressionManager initialized for advanced player progression tracking");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableSkillProgression && !_enableResearchProgression)
                return;
            
            float currentTime = Time.time;
            
            // Update passive experience
            if (_enablePassiveExperience && currentTime - _lastPassiveExperienceTime >= 3600f) // Every hour
            {
                GainExperience(_passiveExperienceRate, ExperienceSource.Passive_Time);
                _lastPassiveExperienceTime = currentTime;
            }
            
            // Update active research projects
            if (_enableResearchProgression)
            {
                UpdateActiveResearchProjects();
            }
            
            // Update skill-based effects and bonuses
            UpdateSkillEffects();
            
            // Check for achievement progress
            if (_enableAchievementSystem)
            {
                UpdateAchievementProgress();
            }
        }
        
        /// <summary>
        /// Gains experience from various gameplay activities.
        /// </summary>
        public void GainExperience(float amount, ExperienceSource source, SkillNodeSO relatedSkill = null)
        {
            if (!_enableSkillProgression || amount <= 0f)
                return;
            
            float modifiedAmount = amount * _experienceGainMultiplier;
            
            // Apply source-specific modifiers
            modifiedAmount *= GetExperienceSourceMultiplier(source);
            
            // Apply skill-specific bonuses
            if (relatedSkill != null)
            {
                modifiedAmount *= GetSkillExperienceBonus(relatedSkill);
            }
            
            _totalExperience += modifiedAmount;
            _currentLevelExperience += modifiedAmount;
            
            // Check for level up
            CheckForLevelUp();
            
            // Gain skill experience if related to a specific skill
            if (relatedSkill != null)
            {
                GainSkillExperience(relatedSkill, modifiedAmount * _skillUsageExperienceMultiplier);
            }
            
            LogInfo($"Gained {modifiedAmount:F1} experience from {source}");
        }
        
        /// <summary>
        /// Attempts to learn a new skill if prerequisites are met.
        /// </summary>
        public bool LearnSkill(SkillNodeSO skill)
        {
            if (!_enableSkillProgression || skill == null)
                return false;
            
            // Check if already learned
            if (_playerSkills.ContainsKey(skill))
            {
                LogWarning($"Skill {skill.SkillName} is already learned");
                return false;
            }
            
            // Check prerequisites
            if (!skill.CanLearnSkill(GetPlayerSkillProfile()))
            {
                LogWarning($"Prerequisites not met for skill {skill.SkillName}");
                return false;
            }
            
            // Check skill point cost
            int cost = skill.GetSkillPointCost(0);
            if (_availableSkillPoints < cost)
            {
                LogWarning($"Not enough skill points to learn {skill.SkillName}. Required: {cost}, Available: {_availableSkillPoints}");
                return false;
            }
            
            // Learn the skill
            var skillData = new PlayerSkillData
            {
                Skill = skill,
                CurrentLevel = 1,
                CurrentExperience = 0f,
                DateLearned = DateTime.Now,
                TimesUsed = 0,
                Specialization = SpecializationType.Cultivation_Specialist,
                IsMastered = false
            };
            
            _playerSkills[skill] = skillData;
            _availableSkillPoints -= cost;
            
            // Apply skill effects
            ApplySkillEffects(skill, 1);
            
            _onSkillLearned?.Raise();
            LogInfo($"Learned skill: {skill.SkillName}");
            
            return true;
        }
        
        /// <summary>
        /// Levels up a skill using skill points or experience.
        /// </summary>
        public bool LevelUpSkill(SkillNodeSO skill, bool useSkillPoints = true)
        {
            if (!_enableSkillProgression || skill == null || !_playerSkills.ContainsKey(skill))
                return false;
            
            var skillData = _playerSkills[skill];
            
            if (skillData.CurrentLevel >= skill.MaxSkillLevel)
            {
                LogWarning($"Skill {skill.SkillName} is already at maximum level");
                return false;
            }
            
            if (useSkillPoints)
            {
                int cost = skill.GetSkillPointCost(skillData.CurrentLevel);
                if (_availableSkillPoints < cost)
                {
                    LogWarning($"Not enough skill points to level up {skill.SkillName}");
                    return false;
                }
                _availableSkillPoints -= cost;
            }
            else
            {
                // Check if enough experience accumulated
                float requiredExp = skill.GetExperienceRequiredForLevel(skillData.CurrentLevel + 1);
                if (skillData.CurrentExperience < requiredExp)
                {
                    LogWarning($"Not enough experience to level up {skill.SkillName}");
                    return false;
                }
                skillData.CurrentExperience -= requiredExp;
            }
            
            // Level up the skill
            int previousLevel = skillData.CurrentLevel;
            skillData.CurrentLevel++;
            skillData.DateLastLevelUp = DateTime.Now;
            
            // Apply new skill effects
            ApplySkillEffects(skill, skillData.CurrentLevel);
            
            // Check for mastery
            if (skillData.CurrentLevel >= skill.MaxSkillLevel)
            {
                AchieveMastery(skill, skillData);
            }
            
            // Check for specialization unlocks
            CheckSpecializationUnlocks(skill, skillData);
            
            _onSkillLevelUp?.Raise();
            LogInfo($"Leveled up skill {skill.SkillName} from {previousLevel} to {skillData.CurrentLevel}");
            
            return true;
        }
        
        /// <summary>
        /// Starts a research project if requirements are met.
        /// </summary>
        public bool StartResearchProject(ResearchProjectSO project)
        {
            if (!_enableResearchProgression || project == null)
                return false;
            
            // Check if already researching this project
            if (_activeResearch.ContainsKey(project))
            {
                LogWarning($"Research project {project.ProjectName} is already active");
                return false;
            }
            
            // Check maximum simultaneous research limit
            if (_activeResearch.Count >= _maxSimultaneousResearch)
            {
                LogWarning($"Maximum research projects limit reached ({_maxSimultaneousResearch})");
                return false;
            }
            
            // Evaluate feasibility
            var capabilities = GetPlayerResearchCapabilities();
            var feasibility = project.EvaluateResearchFeasibility(capabilities);
            
            if (feasibility.OverallFeasibility < 0.5f)
            {
                LogWarning($"Research project {project.ProjectName} feasibility too low: {feasibility.OverallFeasibility:F2}");
                return false;
            }
            
            // Start the project
            var activeProject = new ActiveResearchProject
            {
                Project = project,
                StartDate = DateTime.Now,
                Progress = 0f,
                QualityScore = 1f,
                CompletedPhases = new List<CompletedPhase>(),
                CompletedMilestones = new List<CompletedMilestone>(),
                TeamExpertise = CalculateTeamExpertise(project),
                HadSetbacks = false
            };
            
            _activeResearch[project] = activeProject;
            
            LogInfo($"Started research project: {project.ProjectName}");
            return true;
        }
        
        /// <summary>
        /// Gets player's specialization bonuses for a specific category.
        /// </summary>
        public float GetSpecializationBonus(SkillCategory category)
        {
            if (!_specializations.ContainsKey(category))
                return 0f;
            
            var specialization = _specializations[category];
            return specialization.SpecializationLevel * 0.05f; // 5% bonus per specialization level
        }
        
        /// <summary>
        /// Gets all unlocked features for the player.
        /// </summary>
        public List<UnlockableFeature> GetUnlockedFeatures()
        {
            var unlockedFeatures = new List<UnlockableFeature>();
            
            foreach (var skillData in _playerSkills.Values)
            {
                var features = skillData.Skill.GetUnlockedFeatures(skillData.CurrentLevel);
                unlockedFeatures.AddRange(features);
            }
            
            return unlockedFeatures;
        }
        
        /// <summary>
        /// Gets all available equipment unlocks for the player.
        /// </summary>
        public List<EquipmentUnlock> GetAvailableEquipmentUnlocks()
        {
            var equipmentUnlocks = new List<EquipmentUnlock>();
            
            foreach (var skillData in _playerSkills.Values)
            {
                foreach (var unlock in skillData.Skill.EquipmentUnlocks)
                {
                    if (skillData.CurrentLevel >= unlock.UnlockLevel)
                        equipmentUnlocks.Add(unlock);
                }
            }
            
            return equipmentUnlocks;
        }
        
        /// <summary>
        /// Calculates skill-based bonuses for specific activities.
        /// </summary>
        public SkillBonusCalculation CalculateSkillBonuses(SkillEffectType effectType)
        {
            var calculation = new SkillBonusCalculation
            {
                EffectType = effectType,
                BaseBonus = 0f,
                SkillBonuses = new List<SkillBonusContribution>(),
                SynergyBonuses = new List<SynergyBonus>(),
                SpecializationBonus = 0f,
                TotalBonus = 0f
            };
            
            // Calculate individual skill bonuses
            foreach (var skillData in _playerSkills.Values)
            {
                float skillBonus = skillData.Skill.CalculateEffectValue(effectType, skillData.CurrentLevel);
                if (skillBonus > 0f)
                {
                    calculation.SkillBonuses.Add(new SkillBonusContribution
                    {
                        Skill = skillData.Skill,
                        BonusValue = skillBonus,
                        SkillLevel = skillData.CurrentLevel
                    });
                    calculation.BaseBonus += skillBonus;
                }
            }
            
            // Calculate synergy bonuses
            calculation.SynergyBonuses = CalculateSynergyBonuses();
            float synergyTotal = calculation.SynergyBonuses.Sum(sb => sb.BonusValue);
            
            // Calculate specialization bonuses
            foreach (var specialization in _specializations.Values)
            {
                if (IsEffectRelevantToSpecialization(effectType, specialization.Category))
                {
                    calculation.SpecializationBonus += specialization.SpecializationLevel * 0.1f;
                }
            }
            
            calculation.TotalBonus = calculation.BaseBonus + synergyTotal + calculation.SpecializationBonus;
            return calculation;
        }
        
        private void InitializePlayerProfile()
        {
            if (_playerProfile == null)
            {
                _playerProfile = new LegacyPlayerProgressionProfile
                {
                    PlayerLevel = 1,
                    TotalExperience = 0f,
                    AvailableSkillPoints = 3, // Starting skill points
                    CreationDate = DateTime.Now,
                    LastPlayDate = DateTime.Now,
                    TotalPlayTime = 0f,
                    AchievementsUnlocked = 0,
                    ResearchProjectsCompleted = 0,
                    SkillsLearned = 0,
                    SkillsMastered = 0
                };
            }
            
            _availableSkillPoints = _playerProfile.AvailableSkillPoints;
            _totalExperience = _playerProfile.TotalExperience;
        }
        
        private void CheckForLevelUp()
        {
            float requiredExp = GetExperienceRequiredForLevel(_playerProfile.PlayerLevel + 1);
            
            while (_currentLevelExperience >= requiredExp && _playerProfile.PlayerLevel < _maxPlayerLevel)
            {
                _playerProfile.PlayerLevel++;
                _currentLevelExperience -= requiredExp;
                _availableSkillPoints += _skillPointsPerLevel;
                _playerProfile.AvailableSkillPoints = _availableSkillPoints;
                
                requiredExp = GetExperienceRequiredForLevel(_playerProfile.PlayerLevel + 1);
                
                _onPlayerLevelUp?.Raise();
                LogInfo($"Player leveled up to level {_playerProfile.PlayerLevel}! Gained {_skillPointsPerLevel} skill points.");
            }
        }
        
        private float GetExperienceRequiredForLevel(int level)
        {
            if (_playerLevelCurve != null && _playerLevelCurve.length > 0)
            {
                float normalizedLevel = (float)level / _maxPlayerLevel;
                return _playerLevelCurve.Evaluate(normalizedLevel) * _baseExperiencePerLevel;
            }
            
            // Default exponential curve
            return Mathf.Pow(level, 1.8f) * _baseExperiencePerLevel;
        }
        
        private float GetExperienceSourceMultiplier(ExperienceSource source)
        {
            return _experienceRewards?.GetExperienceMultiplier(source) ?? 1f;
        }
        
        private float GetSkillExperienceBonus(SkillNodeSO skill)
        {
            if (!_playerSkills.ContainsKey(skill))
                return 1f;
            
            var skillData = _playerSkills[skill];
            float bonus = 1f;
            
            // Learning speed bonuses from other skills
            foreach (var otherSkillData in _playerSkills.Values)
            {
                float learningBonus = otherSkillData.Skill.CalculateEffectValue(SkillEffectType.Learning_Speed, otherSkillData.CurrentLevel);
                bonus += learningBonus * 0.01f; // Convert percentage to multiplier
            }
            
            return bonus;
        }
        
        private void GainSkillExperience(SkillNodeSO skill, float amount)
        {
            if (!_playerSkills.ContainsKey(skill))
                return;
            
            var skillData = _playerSkills[skill];
            skillData.CurrentExperience += amount;
            skillData.TimesUsed++;
            
            // Check for automatic level up if enough experience
            float requiredExp = skill.GetExperienceRequiredForLevel(skillData.CurrentLevel + 1);
            if (skillData.CurrentExperience >= requiredExp && skillData.CurrentLevel < skill.MaxSkillLevel)
            {
                LevelUpSkill(skill, false); // Level up using experience, not skill points
            }
        }
        
        private void ApplySkillEffects(SkillNodeSO skill, int level)
        {
            // Apply passive skill effects to the player profile
            foreach (var effect in skill.SkillEffects)
            {
                if (skill.HasPassiveEffect)
                {
                    ApplyPassiveEffect(effect, level);
                }
            }
        }
        
        private void ApplyPassiveEffect(SkillEffect effect, int level)
        {
            float effectValue = effect.BaseValue + (effect.ValuePerLevel * level);
            
            // Apply effect based on type and scope
            switch (effect.EffectType)
            {
                case SkillEffectType.Learning_Speed:
                    // Applied when calculating experience gains
                    break;
                case SkillEffectType.Research_Speed:
                    // Applied to active research projects
                    break;
                // Add more effect types as needed
            }
        }
        
        private void UpdateActiveResearchProjects()
        {
            var completedProjects = new List<ResearchProjectSO>();
            
            foreach (var kvp in _activeResearch.ToList())
            {
                var project = kvp.Key;
                var activeProject = kvp.Value;
                
                // Update progress based on research speed and team expertise
                float progressRate = CalculateResearchProgressRate(project, activeProject);
                activeProject.Progress += progressRate * Time.deltaTime;
                
                // Check for completion
                if (activeProject.Progress >= 1f)
                {
                    CompleteResearchProject(project, activeProject);
                    completedProjects.Add(project);
                }
            }
            
            // Remove completed projects
            foreach (var project in completedProjects)
            {
                _activeResearch.Remove(project);
            }
        }
        
        private float CalculateResearchProgressRate(ResearchProjectSO project, ActiveResearchProject activeProject)
        {
            float baseRate = 1f / (project.Timeline.EstimatedDurationDays * 86400f); // Per second
            float speedMultiplier = _researchSpeedMultiplier;
            
            // Apply research speed bonuses from skills
            foreach (var skillData in _playerSkills.Values)
            {
                float researchBonus = skillData.Skill.CalculateEffectValue(SkillEffectType.Research_Speed, skillData.CurrentLevel);
                speedMultiplier += researchBonus * 0.01f;
            }
            
            // Apply team expertise multiplier
            speedMultiplier *= activeProject.TeamExpertise;
            
            return baseRate * speedMultiplier;
        }
        
        private void CompleteResearchProject(ResearchProjectSO project, ActiveResearchProject activeProject)
        {
            // Generate research results
            var results = project.GenerateResearchResults(activeProject.QualityScore, activeProject.TeamExpertise, activeProject.HadSetbacks);
            
            // Create completed project record
            var completedProject = new CompletedResearchProject
            {
                Project = project,
                StartDate = activeProject.StartDate,
                CompletionDate = DateTime.Now,
                Results = results,
                QualityScore = activeProject.QualityScore,
                ActualDurationDays = (float)(DateTime.Now - activeProject.StartDate).TotalDays
            };
            
            _completedResearch.Add(completedProject);
            _playerProfile.ResearchProjectsCompleted++;
            
            // Apply research benefits
            ApplyResearchBenefits(results);
            
            _onResearchCompleted?.Raise();
            LogInfo($"Completed research project: {project.ProjectName}");
        }
        
        private void ApplyResearchBenefits(ResearchResults results)
        {
            // Grant experience for completed research
            GainExperience(1000f, ExperienceSource.Research_Completion);
            
            // Unlock technologies and knowledge
            foreach (var tech in results.UnlocksTechnologies)
            {
                UnlockTechnology(tech);
            }
            
            foreach (var knowledge in results.KnowledgeGains)
            {
                UnlockKnowledge(knowledge);
            }
        }
        
        private void UnlockTechnology(TechnologyUnlock tech)
        {
            LogInfo($"Unlocked technology: {tech.TechnologyName}");
            // Implementation depends on how technologies are managed in the game
        }
        
        private void UnlockKnowledge(KnowledgeAdvancement knowledge)
        {
            LogInfo($"Gained knowledge: {knowledge.AdvancementName}");
            // Implementation depends on how knowledge is managed in the game
        }
        
        private void AchieveMastery(SkillNodeSO skill, PlayerSkillData skillData)
        {
            skillData.IsMastered = true;
            skillData.DateMastered = DateTime.Now;
            _playerProfile.SkillsMastered++;
            
            // Apply mastery bonuses
            foreach (var mastery in skill.MasteryBonuses)
            {
                ApplyMasteryBonus(mastery);
            }
            
            _onMasteryAchieved?.Raise();
            LogInfo($"Achieved mastery in skill: {skill.SkillName}");
        }
        
        private void ApplyMasteryBonus(MasteryBonus mastery)
        {
            // Apply permanent mastery bonuses to the player
            LogInfo($"Applied mastery bonus: {mastery.MasteryName} (+{mastery.MasteryBonusValue:P})");
        }
        
        private void CheckSpecializationUnlocks(SkillNodeSO skill, PlayerSkillData skillData)
        {
            if (!skill.AllowsSpecialization || skillData.CurrentLevel < skill.SpecializationUnlockLevel)
                return;
            
            var category = skill.SkillCategory;
            if (!_specializations.ContainsKey(category))
            {
                _specializations[category] = new SpecializationProgress
                {
                    Category = category,
                    SpecializationLevel = 0,
                    UnlockedPaths = new List<SpecializationPath>()
                };
            }
            
            var specialization = _specializations[category];
            
            // Check for new specialization path unlocks
            foreach (var path in skill.SpecializationPaths)
            {
                if (!specialization.UnlockedPaths.Contains(path))
                {
                    bool canUnlock = true;
                    foreach (var requiredSkill in path.RequiredSkills)
                    {
                        if (!_playerSkills.ContainsKey(requiredSkill) || 
                            _playerSkills[requiredSkill].CurrentLevel < skill.SpecializationUnlockLevel)
                        {
                            canUnlock = false;
                            break;
                        }
                    }
                    
                    if (canUnlock)
                    {
                        specialization.UnlockedPaths.Add(path);
                        specialization.SpecializationLevel++;
                        LogInfo($"Unlocked specialization path: {path.SpecializationName}");
                    }
                }
            }
        }
        
        private void UpdateSkillEffects()
        {
            // Update any time-based skill effects or bonuses
            // This could include passive bonuses that change over time
        }
        
        private void UpdateAchievementProgress()
        {
            // Check for achievement progress and unlocks
            // This would integrate with a separate achievement system
        }
        
        private PlayerSkillProfile GetPlayerSkillProfile()
        {
            var profile = new PlayerSkillProfile
            {
                PlayerLevel = _playerProfile.PlayerLevel,
                AvailableSkillPoints = _availableSkillPoints,
                PreferredLearningStyle = LearningType.Hands_On, // Could be player preference
                SkillLevels = _playerSkills.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CurrentLevel),
                AvailableFacilities = GetAvailableFacilities(),
                AvailableResources = GetAvailableResources()
            };
            
            return profile;
        }
        
        private PlayerResearchCapabilities GetPlayerResearchCapabilities()
        {
            var capabilities = new PlayerResearchCapabilities
            {
                AvailableBudget = 100000f, // Would come from player's money
                AvailableResearchTime = 365, // Would come from game time management
                CanManageParallelProjects = _playerProfile.PlayerLevel >= 20,
                SkillLevels = _playerSkills.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CurrentLevel),
                AvailableEquipment = GetAvailableEquipment(),
                AvailableResources = GetAvailableResourceCounts()
            };
            
            return capabilities;
        }
        
        private float CalculateTeamExpertise(ResearchProjectSO project)
        {
            float expertise = 1f;
            
            // Calculate based on relevant skills
            foreach (var skillReq in project.RequiredSkills)
            {
                if (_playerSkills.ContainsKey(skillReq.RequiredSkillNode))
                {
                    var skillData = _playerSkills[skillReq.RequiredSkillNode];
                    float skillContribution = (float)skillData.CurrentLevel / skillReq.RequiredSkillNode.MaxSkillLevel;
                    expertise *= (1f + skillContribution * 0.5f);
                }
            }
            
            return Mathf.Clamp(expertise, 0.5f, 2f);
        }
        
        private List<SynergyBonus> CalculateSynergyBonuses()
        {
            var bonuses = new List<SynergyBonus>();
            var skills = _playerSkills.Keys.ToList();
            
            for (int i = 0; i < skills.Count; i++)
            {
                for (int j = i + 1; j < skills.Count; j++)
                {
                    var skill1 = skills[i];
                    var skill2 = skills[j];
                    var skillData1 = _playerSkills[skill1];
                    var skillData2 = _playerSkills[skill2];
                    
                    float synergyValue = skill1.CalculateSynergyBonus(skill2, skillData1.CurrentLevel, skillData2.CurrentLevel);
                    
                    if (synergyValue > 0f)
                    {
                        bonuses.Add(new SynergyBonus
                        {
                            Skill1 = skill1,
                            Skill2 = skill2,
                            BonusValue = synergyValue,
                            SynergyType = DetermineSynergyType(skill1, skill2)
                        });
                    }
                }
            }
            
            return bonuses;
        }
        
        private SynergyType DetermineSynergyType(SkillNodeSO skill1, SkillNodeSO skill2)
        {
            if (skill1.SkillDomain == skill2.SkillDomain)
                return SynergyType.Domain_Synergy;
            else if (skill1.SkillCategory == skill2.SkillCategory)
                return SynergyType.Category_Synergy;
            else
                return SynergyType.Cross_Domain;
        }
        
        private bool IsEffectRelevantToSpecialization(SkillEffectType effectType, SkillCategory category)
        {
            // Define which effect types are relevant to each specialization category
            var relevantEffects = new Dictionary<SkillCategory, SkillEffectType[]>
            {
                { SkillCategory.Cultivation, new[] { SkillEffectType.Yield_Bonus, SkillEffectType.Growth_Speed, SkillEffectType.Quality_Bonus } },
                { SkillCategory.Genetics, new[] { SkillEffectType.Innovation_Rate, SkillEffectType.Research_Speed } },
                { SkillCategory.Processing, new[] { SkillEffectType.Quality_Bonus, SkillEffectType.Efficiency } },
                { SkillCategory.Business, new[] { SkillEffectType.Cost_Reduction, SkillEffectType.Time_Reduction } }
            };
            
            return relevantEffects.ContainsKey(category) && relevantEffects[category].Contains(effectType);
        }
        
        private List<string> GetAvailableFacilities()
        {
            // Would integrate with facility management system
            return new List<string> { "Basic_Grow_Room", "Laboratory" };
        }
        
        private List<string> GetAvailableResources()
        {
            // Would integrate with inventory/resource management system
            return new List<string> { "Seeds", "Nutrients", "Equipment" };
        }
        
        private List<EquipmentDataSO> GetAvailableEquipment()
        {
            // Would integrate with equipment management system
            return new List<EquipmentDataSO>();
        }
        
        private Dictionary<string, int> GetAvailableResourceCounts()
        {
            // Would integrate with inventory system
            return new Dictionary<string, int>
            {
                { "Seeds", 100 },
                { "Nutrients", 50 },
                { "Test_Samples", 25 }
            };
        }
        
        protected override void OnManagerShutdown()
        {
            // Save progression data
            SaveProgressionData();
            
            _playerSkills.Clear();
            _activeResearch.Clear();
            _completedResearch.Clear();
            _achievements.Clear();
            _specializations.Clear();
            
            LogInfo("ProgressionManager shutdown complete");
        }
        
        private void SaveProgressionData()
        {
            // Would integrate with save system to persist progression data
            LogInfo("Progression data saved");
        }
    }
    
    // Supporting data structures for progression management
    
    [System.Serializable]
    public class LegacyPlayerProgressionProfile
    {
        public int PlayerLevel = 1;
        public float TotalExperience = 0f;
        public int AvailableSkillPoints = 3;
        public DateTime CreationDate;
        public DateTime LastPlayDate;
        public float TotalPlayTime = 0f;
        public int AchievementsUnlocked = 0;
        public int ResearchProjectsCompleted = 0;
        public int SkillsLearned = 0;
        public int SkillsMastered = 0;
    }
    
    [System.Serializable]
    public class PlayerSkillData
    {
        public SkillNodeSO Skill;
        public int CurrentLevel = 1;
        public float CurrentExperience = 0f;
        public DateTime DateLearned;
        public DateTime DateLastLevelUp;
        public DateTime DateMastered;
        public int TimesUsed = 0;
        public SpecializationType Specialization;
        public bool IsMastered = false;
        public List<string> CompletedTraining = new List<string>();
    }
    
    // Note: ActiveResearchProject class is now defined in ResearchDataStructures.cs
    
    [System.Serializable]
    public class CompletedResearchProject
    {
        public ResearchProjectSO Project;
        public DateTime StartDate;
        public DateTime CompletionDate;
        public ResearchResults Results;
        public float QualityScore;
        public float ActualDurationDays;
    }
    
    // AchievementProgress moved to AchievementSystemManager.cs to avoid conflicts
    
    [System.Serializable]
    public class SpecializationProgress
    {
        public SkillCategory Category;
        public int SpecializationLevel = 0;
        public List<SpecializationPath> UnlockedPaths = new List<SpecializationPath>();
    }
    
    [System.Serializable]
    public class ExperienceRewards
    {
        [Range(0.5f, 5f)] public float PlantHarvestMultiplier = 1f;
        [Range(0.5f, 5f)] public float BreedingSuccessMultiplier = 2f;
        [Range(0.5f, 5f)] public float ResearchCompletionMultiplier = 3f;
        [Range(0.5f, 5f)] public float QualityAchievementMultiplier = 1.5f;
        [Range(0.5f, 5f)] public float FacilityCompletionMultiplier = 2.5f;
        [Range(0.5f, 5f)] public float SkillUsageMultiplier = 0.1f;
        
        public float GetExperienceMultiplier(ExperienceSource source)
        {
            return source switch
            {
                ExperienceSource.Plant_Harvest => PlantHarvestMultiplier,
                ExperienceSource.Breeding_Success => BreedingSuccessMultiplier,
                ExperienceSource.Research_Completion => ResearchCompletionMultiplier,
                ExperienceSource.Quality_Achievement => QualityAchievementMultiplier,
                ExperienceSource.Facility_Completion => FacilityCompletionMultiplier,
                ExperienceSource.Skill_Usage => SkillUsageMultiplier,
                _ => 1f
            };
        }
    }
    
    [System.Serializable]
    public class SkillBonusCalculation
    {
        public SkillEffectType EffectType;
        public float BaseBonus;
        public List<SkillBonusContribution> SkillBonuses = new List<SkillBonusContribution>();
        public List<SynergyBonus> SynergyBonuses = new List<SynergyBonus>();
        public float SpecializationBonus;
        public float TotalBonus;
    }
    
    [System.Serializable]
    public class SkillBonusContribution
    {
        public SkillNodeSO Skill;
        public float BonusValue;
        public int SkillLevel;
    }
    
    [System.Serializable]
    public class SynergyBonus
    {
        public SkillNodeSO Skill1;
        public SkillNodeSO Skill2;
        public float BonusValue;
        public SynergyType SynergyType;
    }
    
    // Enums for progression system
    // Note: ExperienceSource enum is now defined in ProgressionDataStructures.cs
}