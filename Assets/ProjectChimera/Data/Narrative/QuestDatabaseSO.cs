using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Comprehensive quest database ScriptableObject for Project Chimera's quest and mission system.
    /// Contains all quests, missions, objectives, and progression content with cannabis cultivation
    /// integration and educational learning pathways.
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest Database", menuName = "Project Chimera/Narrative/Quest Database", order = 103)]
    public class QuestDatabaseSO : ChimeraDataSO
    {
        [Header("Quest Collection")]
        [SerializeField] private List<QuestSO> _quests = new List<QuestSO>();
        [SerializeField] private List<MissionSO> _missions = new List<MissionSO>();
        [SerializeField] private List<ObjectiveSO> _objectives = new List<ObjectiveSO>();
        [SerializeField] private List<QuestChainSO> _questChains = new List<QuestChainSO>();
        
        [Header("Quest Organization")]
        [SerializeField] private List<string> _mainQuestIds = new List<string>();
        [SerializeField] private List<string> _sideQuestIds = new List<string>();
        [SerializeField] private List<string> _dailyQuestIds = new List<string>();
        [SerializeField] private List<string> _educationalQuestIds = new List<string>();
        [SerializeField] private List<string> _cultivationQuestIds = new List<string>();
        [SerializeField] private List<string> _communityQuestIds = new List<string>();
        
        [Header("Quest Categories")]
        [SerializeField] private List<QuestCategorySO> _questCategories = new List<QuestCategorySO>();
        [SerializeField] private List<QuestTagSO> _questTags = new List<QuestTagSO>();
        [SerializeField] private List<QuestDifficultySO> _difficultyLevels = new List<QuestDifficultySO>();
        
        [Header("Progression and Dependencies")]
        [SerializeField] private Dictionary<string, List<string>> _questDependencies = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, List<string>> _unlockRequirements = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, QuestRewardStructure> _questRewards = new Dictionary<string, QuestRewardStructure>();
        
        [Header("Integration Settings")]
        [SerializeField] private List<CultivationQuestMapping> _cultivationMappings = new List<CultivationQuestMapping>();
        [SerializeField] private List<EducationalQuestMapping> _educationalMappings = new List<EducationalQuestMapping>();
        [SerializeField] private List<EventQuestMapping> _eventMappings = new List<EventQuestMapping>();
        [SerializeField] private List<CharacterQuestMapping> _characterMappings = new List<CharacterQuestMapping>();
        
        [Header("Dynamic Quest Generation")]
        [SerializeField] private bool _enableDynamicQuests = false;
        [SerializeField] private List<QuestTemplateSO> _questTemplates = new List<QuestTemplateSO>();
        [SerializeField] private List<ObjectiveTemplateSO> _objectiveTemplates = new List<ObjectiveTemplateSO>();
        [SerializeField] private QuestGenerationConfigSO _generationConfig;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableQuestCaching = true;
        [SerializeField] private int _maxCachedQuests = 200;
        [SerializeField] private bool _enableLazyLoading = true;
        [SerializeField] private bool _preloadActiveQuests = true;
        
        // Runtime caches
        private Dictionary<string, QuestSO> _questCache = new Dictionary<string, QuestSO>();
        private Dictionary<string, MissionSO> _missionCache = new Dictionary<string, MissionSO>();
        private Dictionary<string, ObjectiveSO> _objectiveCache = new Dictionary<string, ObjectiveSO>();
        private Dictionary<string, QuestChainSO> _questChainCache = new Dictionary<string, QuestChainSO>();
        private Dictionary<QuestType, List<QuestSO>> _questsByType = new Dictionary<QuestType, List<QuestSO>>();
        
        // Properties
        public List<QuestSO> Quests => _quests;
        public List<MissionSO> Missions => _missions;
        public List<ObjectiveSO> Objectives => _objectives;
        public List<QuestChainSO> QuestChains => _questChains;
        public List<QuestCategorySO> QuestCategories => _questCategories;
        public List<QuestTagSO> QuestTags => _questTags;
        public List<QuestDifficultySO> DifficultyLevels => _difficultyLevels;
        public bool EnableDynamicQuests => _enableDynamicQuests;
        public List<string> MainQuestIds => _mainQuestIds;
        public List<string> SideQuestIds => _sideQuestIds;
        public List<string> DailyQuestIds => _dailyQuestIds;
        public List<string> EducationalQuestIds => _educationalQuestIds;
        public List<string> CultivationQuestIds => _cultivationQuestIds;
        public List<string> CommunityQuestIds => _communityQuestIds;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Rebuild caches after validation
            RebuildCaches();
            
            // Validate quest organization
            ValidateQuestOrganization();
            
            // Validate dependencies
            ValidateQuestDependencies();
        }
        
        private void OnEnable()
        {
            RebuildCaches();
        }
        
        #region Cache Management
        
        private void RebuildCaches()
        {
            // Clear existing caches
            _questCache.Clear();
            _missionCache.Clear();
            _objectiveCache.Clear();
            _questChainCache.Clear();
            _questsByType.Clear();
            
            // Rebuild quest cache
            foreach (var quest in _quests)
            {
                if (quest != null && !string.IsNullOrEmpty(quest.QuestId))
                {
                    _questCache[quest.QuestId] = quest;
                    
                    // Organize by type
                    if (!_questsByType.ContainsKey(quest.QuestType))
                    {
                        _questsByType[quest.QuestType] = new List<QuestSO>();
                    }
                    _questsByType[quest.QuestType].Add(quest);
                }
            }
            
            // Rebuild mission cache
            foreach (var mission in _missions)
            {
                if (mission != null && !string.IsNullOrEmpty(mission.MissionId))
                {
                    _missionCache[mission.MissionId] = mission;
                }
            }
            
            // Rebuild objective cache
            foreach (var objective in _objectives)
            {
                if (objective != null && !string.IsNullOrEmpty(objective.ObjectiveId))
                {
                    _objectiveCache[objective.ObjectiveId] = objective;
                }
            }
            
            // Rebuild quest chain cache
            foreach (var questChain in _questChains)
            {
                if (questChain != null && !string.IsNullOrEmpty(questChain.ChainId))
                {
                    _questChainCache[questChain.ChainId] = questChain;
                }
            }
        }
        
        #endregion
        
        #region Quest Retrieval
        
        public QuestSO GetQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return null;
            
            return _questCache.GetValueOrDefault(questId);
        }
        
        public List<QuestSO> GetQuestsByType(QuestType questType)
        {
            return _questsByType.GetValueOrDefault(questType, new List<QuestSO>());
        }
        
        public List<QuestSO> GetMainQuests()
        {
            return _mainQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public List<QuestSO> GetSideQuests()
        {
            return _sideQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public List<QuestSO> GetDailyQuests()
        {
            return _dailyQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public List<QuestSO> GetEducationalQuests()
        {
            return _educationalQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public List<QuestSO> GetCultivationQuests()
        {
            return _cultivationQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public List<QuestSO> GetCommunityQuests()
        {
            return _communityQuestIds.Select(GetQuest).Where(quest => quest != null).ToList();
        }
        
        public MissionSO GetMission(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
                return null;
            
            return _missionCache.GetValueOrDefault(missionId);
        }
        
        public ObjectiveSO GetObjective(string objectiveId)
        {
            if (string.IsNullOrEmpty(objectiveId))
                return null;
            
            return _objectiveCache.GetValueOrDefault(objectiveId);
        }
        
        public QuestChainSO GetQuestChain(string chainId)
        {
            if (string.IsNullOrEmpty(chainId))
                return null;
            
            return _questChainCache.GetValueOrDefault(chainId);
        }
        
        #endregion
        
        #region Quest Querying and Filtering
        
        public List<QuestSO> GetQuestsByCategory(string categoryId)
        {
            return _quests.Where(quest => quest != null && quest.CategoryId == categoryId).ToList();
        }
        
        public List<QuestSO> GetQuestsByTag(string tagId)
        {
            return _quests.Where(quest => quest != null && quest.TagIds.Contains(tagId)).ToList();
        }
        
        public List<QuestSO> GetQuestsByDifficulty(QuestDifficulty difficulty)
        {
            return _quests.Where(quest => quest != null && quest.Difficulty == difficulty).ToList();
        }
        
        public List<QuestSO> GetQuestsByCharacter(string characterId)
        {
            return _quests.Where(quest => quest != null && quest.QuestGiverId == characterId).ToList();
        }
        
        public List<QuestSO> GetAvailableQuests(PlayerQuestProfile playerProfile)
        {
            var availableQuests = new List<QuestSO>();
            
            foreach (var quest in _quests)
            {
                if (quest == null) continue;
                
                if (IsQuestAvailable(quest.QuestId, playerProfile))
                {
                    availableQuests.Add(quest);
                }
            }
            
            return availableQuests;
        }
        
        public bool IsQuestAvailable(string questId, PlayerQuestProfile playerProfile)
        {
            var quest = GetQuest(questId);
            if (quest == null) return false;
            
            // Check if already completed
            if (playerProfile.CompletedQuestIds.Contains(questId))
                return false;
            
            // Check if already active
            if (playerProfile.ActiveQuestIds.Contains(questId))
                return false;
            
            // Check level requirements
            if (quest.MinimumLevel > playerProfile.PlayerLevel)
                return false;
            
            // Check dependencies
            if (_questDependencies.TryGetValue(questId, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (!playerProfile.CompletedQuestIds.Contains(dependency))
                        return false;
                }
            }
            
            // Check unlock requirements
            if (_unlockRequirements.TryGetValue(questId, out var requirements))
            {
                foreach (var requirement in requirements)
                {
                    if (!playerProfile.MeetsRequirement(requirement))
                        return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Integration Mappings
        
        public List<QuestSO> GetQuestsForCultivationActivity(string activityType)
        {
            var mappings = _cultivationMappings.Where(m => m.CultivationActivity == activityType);
            return mappings.SelectMany(m => m.QuestIds).Select(GetQuest).Where(q => q != null).ToList();
        }
        
        public List<QuestSO> GetQuestsForEducationalTopic(string topicId)
        {
            var mappings = _educationalMappings.Where(m => m.EducationalTopic == topicId);
            return mappings.SelectMany(m => m.QuestIds).Select(GetQuest).Where(q => q != null).ToList();
        }
        
        public List<QuestSO> GetQuestsForEvent(string eventType)
        {
            var mappings = _eventMappings.Where(m => m.EventType == eventType);
            return mappings.SelectMany(m => m.QuestIds).Select(GetQuest).Where(q => q != null).ToList();
        }
        
        public List<QuestSO> GetQuestsForCharacter(string characterId, string interactionType)
        {
            var mappings = _characterMappings.Where(m => m.CharacterId == characterId && m.InteractionType == interactionType);
            return mappings.SelectMany(m => m.QuestIds).Select(GetQuest).Where(q => q != null).ToList();
        }
        
        #endregion
        
        #region Dynamic Quest Generation
        
        public QuestSO GenerateDynamicQuest(QuestGenerationParameters parameters)
        {
            if (!_enableDynamicQuests || _generationConfig == null)
                return null;
            
            var template = GetBestQuestTemplate(parameters);
            if (template == null)
                return null;
            
            return GenerateQuestFromTemplate(template, parameters);
        }
        
        private QuestTemplateSO GetBestQuestTemplate(QuestGenerationParameters parameters)
        {
            return _questTemplates.FirstOrDefault(template => 
                template.QuestType == parameters.QuestType && 
                template.Difficulty == parameters.Difficulty &&
                template.ContextTags.Any(tag => parameters.ContextTags.Contains(tag)));
        }
        
        private QuestSO GenerateQuestFromTemplate(QuestTemplateSO template, QuestGenerationParameters parameters)
        {
            var generatedQuest = new QuestSO
            {
                QuestId = $"generated_{Guid.NewGuid():N}",
                QuestName = ProcessTemplate(template.QuestNameTemplate, parameters.Variables),
                Description = ProcessTemplate(template.DescriptionTemplate, parameters.Variables),
                QuestType = template.QuestType,
                Difficulty = template.Difficulty,
                CategoryId = template.CategoryId,
                IsGenerated = true,
                ExpirationTime = DateTime.Now.AddDays(parameters.ExpirationDays)
            };
            
            // Generate objectives
            generatedQuest.ObjectiveIds = GenerateObjectivesFromTemplate(template, parameters);
            
            // Set rewards
            generatedQuest.RewardIds = GenerateRewardsFromTemplate(template, parameters);
            
            return generatedQuest;
        }
        
        private List<string> GenerateObjectivesFromTemplate(QuestTemplateSO template, QuestGenerationParameters parameters)
        {
            var objectiveIds = new List<string>();
            
            foreach (var objectiveTemplate in template.ObjectiveTemplates)
            {
                var objective = GenerateObjectiveFromTemplate(objectiveTemplate, parameters);
                if (objective != null)
                {
                    _objectives.Add(objective);
                    _objectiveCache[objective.ObjectiveId] = objective;
                    objectiveIds.Add(objective.ObjectiveId);
                }
            }
            
            return objectiveIds;
        }
        
        private ObjectiveSO GenerateObjectiveFromTemplate(ObjectiveTemplateSO template, QuestGenerationParameters parameters)
        {
            return new ObjectiveSO
            {
                ObjectiveId = $"generated_obj_{Guid.NewGuid():N}",
                Description = ProcessTemplate(template.DescriptionTemplate, parameters.Variables),
                ObjectiveType = template.ObjectiveType,
                TargetValue = template.BaseTargetValue * parameters.DifficultyMultiplier,
                IsGenerated = true
            };
        }
        
        private List<string> GenerateRewardsFromTemplate(QuestTemplateSO template, QuestGenerationParameters parameters)
        {
            // Generate dynamic rewards based on template and parameters
            return template.BaseRewardIds.ToList();
        }
        
        private string ProcessTemplate(string templateText, Dictionary<string, object> variables)
        {
            var processedText = templateText;
            
            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                if (processedText.Contains(placeholder))
                {
                    processedText = processedText.Replace(placeholder, variable.Value?.ToString() ?? "");
                }
            }
            
            return processedText;
        }
        
        #endregion
        
        #region Quest Management
        
        public void AddQuest(QuestSO quest)
        {
            if (quest == null || _quests.Contains(quest))
                return;
            
            _quests.Add(quest);
            _questCache[quest.QuestId] = quest;
            
            // Update type cache
            if (!_questsByType.ContainsKey(quest.QuestType))
            {
                _questsByType[quest.QuestType] = new List<QuestSO>();
            }
            _questsByType[quest.QuestType].Add(quest);
        }
        
        public void RemoveQuest(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null)
            {
                _quests.Remove(quest);
                _questCache.Remove(questId);
                
                // Update type cache
                if (_questsByType.ContainsKey(quest.QuestType))
                {
                    _questsByType[quest.QuestType].Remove(quest);
                }
            }
        }
        
        public void AddQuestChain(QuestChainSO questChain)
        {
            if (questChain == null || _questChains.Contains(questChain))
                return;
            
            _questChains.Add(questChain);
            _questChainCache[questChain.ChainId] = questChain;
        }
        
        #endregion
        
        #region Validation
        
        private void ValidateQuestOrganization()
        {
            // Validate main quests exist
            _mainQuestIds.RemoveAll(id => GetQuest(id) == null);
            
            // Validate side quests exist
            _sideQuestIds.RemoveAll(id => GetQuest(id) == null);
            
            // Validate daily quests exist
            _dailyQuestIds.RemoveAll(id => GetQuest(id) == null);
            
            // Validate educational quests exist
            _educationalQuestIds.RemoveAll(id => GetQuest(id) == null);
            
            // Validate cultivation quests exist
            _cultivationQuestIds.RemoveAll(id => GetQuest(id) == null);
            
            // Validate community quests exist
            _communityQuestIds.RemoveAll(id => GetQuest(id) == null);
        }
        
        private void ValidateQuestDependencies()
        {
            var validQuestIds = _quests.Where(q => q != null).Select(q => q.QuestId).ToHashSet();
            
            // Remove invalid dependencies
            var keysToRemove = new List<string>();
            foreach (var kvp in _questDependencies)
            {
                if (!validQuestIds.Contains(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }
                
                // Remove invalid dependency references
                kvp.Value.RemoveAll(dep => !validQuestIds.Contains(dep));
            }
            
            // Remove entries with invalid keys
            foreach (var key in keysToRemove)
            {
                _questDependencies.Remove(key);
            }
        }
        
        #endregion
        
        #region Statistics and Analytics
        
        public QuestDatabaseStatistics GetStatistics()
        {
            return new QuestDatabaseStatistics
            {
                TotalQuests = _quests.Count,
                TotalMissions = _missions.Count,
                TotalObjectives = _objectives.Count,
                TotalQuestChains = _questChains.Count,
                MainQuests = _mainQuestIds.Count,
                SideQuests = _sideQuestIds.Count,
                DailyQuests = _dailyQuestIds.Count,
                EducationalQuests = _educationalQuestIds.Count,
                CultivationQuests = _cultivationQuestIds.Count,
                CommunityQuests = _communityQuestIds.Count,
                Categories = _questCategories.Count,
                Tags = _questTags.Count,
                DifficultyLevels = _difficultyLevels.Count,
                QuestTemplates = _questTemplates.Count,
                ObjectiveTemplates = _objectiveTemplates.Count
            };
        }
        
        #endregion
    }
    
    // Supporting data structures and enums
    public enum QuestType
    {
        Main,
        Side,
        Daily,
        Weekly,
        Educational,
        Cultivation,
        Community,
        Event,
        Challenge,
        Achievement
    }
    
    public enum QuestDifficulty
    {
        Tutorial,
        Easy,
        Normal,
        Hard,
        Expert,
        Master
    }
    
    public enum ObjectiveType
    {
        Collect,
        Craft,
        Grow,
        Harvest,
        Sell,
        Research,
        Learn,
        Interact,
        Explore,
        Complete
    }
    
    [Serializable]
    public class QuestGenerationParameters
    {
        public QuestType QuestType;
        public QuestDifficulty Difficulty;
        public List<string> ContextTags = new List<string>();
        public Dictionary<string, object> Variables = new Dictionary<string, object>();
        public float DifficultyMultiplier = 1.0f;
        public int ExpirationDays = 7;
    }
    
    [Serializable]
    public class PlayerQuestProfile
    {
        public string PlayerId;
        public int PlayerLevel;
        public List<string> ActiveQuestIds = new List<string>();
        public List<string> CompletedQuestIds = new List<string>();
        public Dictionary<string, float> Skills = new Dictionary<string, float>();
        public List<string> UnlockedAchievements = new List<string>();
        
        public bool MeetsRequirement(string requirement)
        {
            // Parse requirement string and check if player meets it
            var parts = requirement.Split(':');
            if (parts.Length < 2) return true;
            
            var type = parts[0];
            var value = parts[1];
            
            return type switch
            {
                "level" => int.TryParse(value, out var requiredLevel) && PlayerLevel >= requiredLevel,
                "skill" => parts.Length >= 3 && Skills.ContainsKey(value) && float.TryParse(parts[2], out var requiredSkill) && Skills[value] >= requiredSkill,
                "achievement" => UnlockedAchievements.Contains(value),
                "quest_completed" => CompletedQuestIds.Contains(value),
                _ => true
            };
        }
    }
    
    [Serializable]
    public class QuestRewardStructure
    {
        public List<string> ItemRewards = new List<string>();
        public int ExperienceReward;
        public int CurrencyReward;
        public List<string> SkillRewards = new List<string>();
        public List<string> UnlockRewards = new List<string>();
    }
    
    [Serializable]
    public class CultivationQuestMapping
    {
        public string CultivationActivity;
        public List<string> QuestIds = new List<string>();
        public float TriggerThreshold = 1.0f;
        public bool IsRepeatable = false;
    }
    
    [Serializable]
    public class EducationalQuestMapping
    {
        public string EducationalTopic;
        public List<string> QuestIds = new List<string>();
        public string SkillLevel = "beginner";
        public bool RequiresCompletion = false;
    }
    
    [Serializable]
    public class EventQuestMapping
    {
        public string EventType;
        public List<string> QuestIds = new List<string>();
        public string EventTrigger = "start";
        public bool IsOptional = true;
    }
    
    [Serializable]
    public class CharacterQuestMapping
    {
        public string CharacterId;
        public string InteractionType;
        public List<string> QuestIds = new List<string>();
        public float RelationshipThreshold = 0.0f;
    }
    
    [Serializable]
    public class QuestDatabaseStatistics
    {
        public int TotalQuests;
        public int TotalMissions;
        public int TotalObjectives;
        public int TotalQuestChains;
        public int MainQuests;
        public int SideQuests;
        public int DailyQuests;
        public int EducationalQuests;
        public int CultivationQuests;
        public int CommunityQuests;
        public int Categories;
        public int Tags;
        public int DifficultyLevels;
        public int QuestTemplates;
        public int ObjectiveTemplates;
    }
    
    // Placeholder classes for compilation
    public class QuestSO : ChimeraDataSO
    {
        public string QuestId;
        public string QuestName;
        public string Description;
        public QuestType QuestType;
        public QuestDifficulty Difficulty;
        public string CategoryId;
        public List<string> TagIds = new List<string>();
        public string QuestGiverId;
        public int MinimumLevel = 1;
        public List<string> ObjectiveIds = new List<string>();
        public List<string> RewardIds = new List<string>();
        public bool IsGenerated = false;
        public DateTime ExpirationTime;
    }
    
    public class MissionSO : ChimeraDataSO
    {
        public string MissionId;
        public string MissionName;
        public List<string> QuestIds = new List<string>();
    }
    
    public class ObjectiveSO : ChimeraDataSO
    {
        public string ObjectiveId;
        public string Description;
        public ObjectiveType ObjectiveType;
        public float TargetValue;
        public bool IsGenerated = false;
    }
    
    public class QuestChainSO : ChimeraDataSO
    {
        public string ChainId;
        public string ChainName;
        public List<string> QuestIds = new List<string>();
    }
    
    public class QuestCategorySO : ChimeraDataSO
    {
        public string CategoryId;
        public string CategoryName;
    }
    
    public class QuestTagSO : ChimeraDataSO
    {
        public string TagId;
        public string TagName;
    }
    
    public class QuestDifficultySO : ChimeraDataSO
    {
        public QuestDifficulty Difficulty;
        public string DifficultyName;
        public float DifficultyMultiplier = 1.0f;
    }
    
    public class QuestTemplateSO : ChimeraDataSO
    {
        public string TemplateId;
        public QuestType QuestType;
        public QuestDifficulty Difficulty;
        public string CategoryId;
        public string QuestNameTemplate;
        public string DescriptionTemplate;
        public List<string> ContextTags = new List<string>();
        public List<ObjectiveTemplateSO> ObjectiveTemplates = new List<ObjectiveTemplateSO>();
        public List<string> BaseRewardIds = new List<string>();
    }
    
    public class ObjectiveTemplateSO : ChimeraDataSO
    {
        public string TemplateId;
        public ObjectiveType ObjectiveType;
        public string DescriptionTemplate;
        public float BaseTargetValue = 1.0f;
    }
    
    public class QuestGenerationConfigSO : ChimeraDataSO
    {
        public float DefaultDifficultyMultiplier = 1.0f;
        public int DefaultExpirationDays = 7;
        public bool EnableRandomization = true;
    }
}