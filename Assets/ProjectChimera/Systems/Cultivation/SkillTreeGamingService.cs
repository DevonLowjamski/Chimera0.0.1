using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Construction; // For SkillLevel enum
using ProjectChimera.Data.Events; // For event data structures
using ProjectChimera.Events.Core; // For cultivation gaming event data
using ProjectChimera.Data.Progression; // For progression data types
using ProjectChimera.Systems.Progression; // For progression managers
using ProgressionExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;
using SkillNodeType = ProjectChimera.Data.Cultivation.SkillNodeType;
using SkillBranch = ProjectChimera.Data.Cultivation.SkillBranch;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Skill Tree Gaming Service - Dedicated service for managing skill tree progression and gaming mechanics
    /// Extracted from EnhancedCultivationGamingManager to provide focused skill tree functionality
    /// Handles skill progression, node unlocks, experience calculation, and skill tree visualization
    /// </summary>
    public class SkillTreeGamingService : MonoBehaviour
    {
        [Header("Skill Tree Gaming Components")]
        [SerializeField] private TreeSkillProgressionSystem _skillTreeSystem;
        [SerializeField] private SkillNodeUnlockManager _nodeUnlockManager;
        [SerializeField] private SkillTreeVisualizationController _treeVisualization;
        
        [Header("Configuration")]
        [SerializeField] private EnhancedCultivationGamingConfigSO _cultivationGamingConfig;
        
        [Header("Skill Tree Event Channels")]
        [SerializeField] private GameEventChannelSO _onSkillNodeUnlocked;
        [SerializeField] private GameEventChannelSO _onSkillTreeVisualizationUpdated;
        [SerializeField] private GameEventChannelSO _onSkillExperienceGained;
        [SerializeField] private GameEventChannelSO _onPlayerSkillLevelChanged;
        
        // Service State
        private bool _isInitialized = false;
        private SkillTreeGamingState _currentSkillState;
        
        // Skill Progression Metrics
        private int _skillNodesProgressed = 0;
        private int _totalExperienceGained = 0;
        private Dictionary<string, float> _skillCategoryExperience;
        private Dictionary<SkillNodeType, bool> _unlockedNodes;
        
        // Dependencies (Constructor Injection Pattern)
        private GameManager _gameManager;
        private TimeManager _timeManager;
        
        #region Service Implementation
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Skill Tree Gaming Service";
        
        public void Initialize()
        {
            InitializeService();
        }
        
        public void Shutdown()
        {
            ShutdownService();
        }
        
        #endregion
        
        #region Service Lifecycle
        
        private void Awake()
        {
            // Initialize dependency references
            InitializeDependencies();
            InitializeDataStructures();
        }
        
        private void Start()
        {
            InitializeService();
        }
        
        private void InitializeDependencies()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _timeManager = FindObjectOfType<TimeManager>();
            
            if (_gameManager == null)
                Debug.LogWarning("GameManager not found - some functionality may be limited");
            if (_timeManager == null)
                Debug.LogWarning("TimeManager not found - time-based mechanics may not work");
        }
        
        private void InitializeDataStructures()
        {
            _skillCategoryExperience = new Dictionary<string, float>();
            _unlockedNodes = new Dictionary<SkillNodeType, bool>();
            
            _currentSkillState = new SkillTreeGamingState
            {
                PlayerSkillLevel = SkillLevel.Beginner,
                TotalExperiencePoints = 0,
                UnlockedNodesCount = 0,
                CurrentSkillCategory = "CultivationBasics"
            };
        }
        
        public void InitializeService()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("SkillTreeGamingService already initialized");
                return;
            }
            
            try
            {
                ValidateConfiguration();
                InitializeSkillTreeSystems();
                InitializeSkillTreeState();
                RegisterEventHandlers();
                
                _isInitialized = true;
                Debug.Log("SkillTreeGamingService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize SkillTreeGamingService: {ex.Message}");
                throw;
            }
        }
        
        public void ShutdownService()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            SaveSkillProgressionMetrics();
            
            _isInitialized = false;
            Debug.Log("SkillTreeGamingService shutdown completed");
        }
        
        #endregion
        
        #region Skill Tree System Initialization
        
        private void ValidateConfiguration()
        {
            if (_cultivationGamingConfig == null)
                throw new System.NullReferenceException("EnhancedCultivationGamingConfigSO is required");
                
            // Create skill tree components if not assigned
            if (_skillTreeSystem == null) CreateTreeSkillProgressionSystem();
            if (_nodeUnlockManager == null) CreateSkillNodeUnlockManager();
            if (_treeVisualization == null) CreateSkillTreeVisualizationController();
        }
        
        private void InitializeSkillTreeSystems()
        {
            Debug.Log("Initializing Skill Tree Gaming Systems...");
            
            // Initialize skill tree progression system
            _skillTreeSystem?.Initialize(_cultivationGamingConfig?.SkillNodeLibrary, _cultivationGamingConfig?.SkillTreeConfigSO);
            
            // Initialize skill node unlock manager
            _nodeUnlockManager?.Initialize();
            
            // Initialize skill tree visualization
            _treeVisualization?.Initialize();
            
            Debug.Log("Skill Tree Gaming Systems initialized");
        }
        
        private void InitializeSkillTreeState()
        {
            // Load any saved skill progression data
            LoadSkillProgressionData();
            
            // Initialize skill categories with starting experience
            InitializeSkillCategories();
            
            // Set up initial skill tree visualization
            RefreshSkillTreeVisualization();
        }
        
        private void InitializeSkillCategories()
        {
            var categories = new string[] 
            { 
                "CultivationBasics", 
                "PlantGenetics", 
                "EnvironmentalControl", 
                "HarvestOptimization", 
                "BusinessManagement",
                "AdvancedTechniques"
            };
            
            foreach (var category in categories)
            {
                if (!_skillCategoryExperience.ContainsKey(category))
                {
                    _skillCategoryExperience[category] = 0f;
                }
            }
        }
        
        #endregion
        
        #region System Creation (Auto-instantiation)
        
        private void CreateTreeSkillProgressionSystem()
        {
            var systemGO = new GameObject("TreeSkillProgressionSystem");
            systemGO.transform.SetParent(transform);
            _skillTreeSystem = systemGO.AddComponent<TreeSkillProgressionSystem>();
        }
        
        private void CreateSkillNodeUnlockManager()
        {
            var unlockManagerGO = new GameObject("SkillNodeUnlockManager");
            unlockManagerGO.transform.SetParent(transform);
            _nodeUnlockManager = unlockManagerGO.AddComponent<SkillNodeUnlockManager>();
        }
        
        private void CreateSkillTreeVisualizationController()
        {
            var visualizationGO = new GameObject("SkillTreeVisualizationController");
            visualizationGO.transform.SetParent(transform);
            _treeVisualization = visualizationGO.AddComponent<SkillTreeVisualizationController>();
        }
        
        #endregion
        
        #region Event System
        
        private void RegisterEventHandlers()
        {
            if (_onSkillNodeUnlocked != null)
                _onSkillNodeUnlocked.OnEventRaisedWithData.AddListener(OnSkillNodeUnlocked);
                
            if (_onSkillTreeVisualizationUpdated != null)
                _onSkillTreeVisualizationUpdated.OnEventRaisedWithData.AddListener(OnSkillTreeVisualizationUpdated);
        }
        
        private void UnregisterEventHandlers()
        {
            if (_onSkillNodeUnlocked != null)
                _onSkillNodeUnlocked.OnEventRaisedWithData.RemoveListener(OnSkillNodeUnlocked);
                
            if (_onSkillTreeVisualizationUpdated != null)
                _onSkillTreeVisualizationUpdated.OnEventRaisedWithData.RemoveListener(OnSkillTreeVisualizationUpdated);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSkillNodeUnlocked(object eventData)
        {
            _skillNodesProgressed++;
            
            if (eventData is SkillNodeEventData skillData)
            {
                ProcessSkillNodeUnlock(skillData);
                UpdatePlayerSkillLevel(skillData);
                UnlockNewGameMechanics(skillData);
                RefreshSkillTreeVisualization();
            }
        }
        
        private void OnSkillTreeVisualizationUpdated(object eventData)
        {
            if (eventData is SkillTreeVisualizationEventData visualData)
            {
                ProcessVisualizationUpdate(visualData);
            }
        }
        
        #endregion
        
        #region Core Skill Tree Mechanics
        
        /// <summary>
        /// Add skill experience for a specific task type and quality
        /// </summary>
        public void AddSkillExperience(string taskType, float experienceGain, ProgressionExperienceSource source)
        {
            if (!_isInitialized || experienceGain <= 0) return;
            
            // Add experience to skill tree system
            _skillTreeSystem?.AddSkillExperience(taskType, experienceGain, source);
            
            // Update local tracking
            var category = GetSkillCategoryForTask(taskType);
            if (_skillCategoryExperience.ContainsKey(category))
            {
                _skillCategoryExperience[category] += experienceGain;
            }
            
            _totalExperienceGained += (int)experienceGain;
            _currentSkillState.TotalExperiencePoints += (int)experienceGain;
            
            // Check for skill level progression
            CheckSkillLevelProgression();
            
            // Trigger experience gained event
            TriggerSkillExperienceGainedEvent(taskType, experienceGain, source);
            
            Debug.Log($"Skill experience gained: {experienceGain} for {taskType} ({source})");
        }
        
        /// <summary>
        /// Progress specific skill node with skill points
        /// </summary>
        public bool ProgressSkillNode(SkillNodeType nodeType, int skillPoints)
        {
            if (!_isInitialized) return false;
            
            var success = _skillTreeSystem?.ProgressNode(nodeType, skillPoints) ?? false;
            
            if (success)
            {
                // Update tracking
                if (!_unlockedNodes.ContainsKey(nodeType))
                {
                    _unlockedNodes[nodeType] = true;
                    _currentSkillState.UnlockedNodesCount++;
                }
                
                // Create skill node event data
                var skillNodeData = new SkillNodeEventData
                {
                    NodeType = nodeType,
                    SkillPointsSpent = skillPoints,
                    Timestamp = System.DateTime.Now
                };
                
                // Trigger skill node unlocked event
                _onSkillNodeUnlocked?.RaiseEvent(skillNodeData);
            }
            
            return success;
        }
        
        /// <summary>
        /// Calculate skill gain based on task type and care quality
        /// </summary>
        public float CalculateSkillGain(string taskType, CareQuality quality)
        {
            var baseGain = _cultivationGamingConfig?.SkillProgressionRate ?? 1.0f;
            var qualityMultiplier = CalculateQualityMultiplier(quality);
            var categoryMultiplier = GetCategoryMultiplier(taskType);
            
            return baseGain * qualityMultiplier * categoryMultiplier;
        }
        
        /// <summary>
        /// Check if a specific skill node is unlocked
        /// </summary>
        public bool IsSkillNodeUnlocked(SkillNodeType nodeType)
        {
            return _unlockedNodes.ContainsKey(nodeType) && _unlockedNodes[nodeType];
        }
        
        /// <summary>
        /// Get current player skill level
        /// </summary>
        public SkillLevel GetCurrentSkillLevel()
        {
            return _currentSkillState.PlayerSkillLevel;
        }
        
        /// <summary>
        /// Get overall skill level from skill tree system
        /// </summary>
        public SkillLevel GetOverallSkillLevel()
        {
            return _skillTreeSystem?.GetOverallSkillLevel() ?? SkillLevel.Beginner;
        }
        
        /// <summary>
        /// Get experience for a specific skill category
        /// </summary>
        public float GetSkillCategoryExperience(string category)
        {
            return _skillCategoryExperience.ContainsKey(category) ? _skillCategoryExperience[category] : 0f;
        }
        
        /// <summary>
        /// Get current skill tree gaming state
        /// </summary>
        public SkillTreeGamingState GetCurrentSkillState()
        {
            return _currentSkillState;
        }
        
        /// <summary>
        /// Get skill progression metrics
        /// </summary>
        public SkillProgressionMetrics GetSkillProgressionMetrics()
        {
            return new SkillProgressionMetrics
            {
                SkillNodesProgressed = _skillNodesProgressed,
                TotalExperienceGained = _totalExperienceGained,
                CurrentSkillLevel = _currentSkillState.PlayerSkillLevel,
                UnlockedNodesCount = _currentSkillState.UnlockedNodesCount,
                SkillCategoryProgression = new Dictionary<string, float>(_skillCategoryExperience)
            };
        }
        
        #endregion
        
        #region Skill Tree Processing
        
        private void ProcessSkillNodeUnlock(SkillNodeEventData skillData)
        {
            // Update skill node unlock tracking
            _unlockedNodes[skillData.NodeType] = true;
            _currentSkillState.UnlockedNodesCount++;
            
            // Log skill node unlock
            Debug.Log($"Skill node unlocked: {skillData.NodeType}");
        }
        
        private void UpdatePlayerSkillLevel(SkillNodeEventData skillData)
        {
            var newSkillLevel = _skillTreeSystem?.GetOverallSkillLevel() ?? SkillLevel.Beginner;
            var previousLevel = _currentSkillState.PlayerSkillLevel;
            
            if (newSkillLevel != previousLevel)
            {
                _currentSkillState.PlayerSkillLevel = newSkillLevel;
                
                // Trigger player skill level changed event
                var levelChangeData = new PlayerSkillLevelEventData
                {
                    PreviousLevel = previousLevel,
                    NewLevel = newSkillLevel,
                    Timestamp = Time.time
                };
                
                _onPlayerSkillLevelChanged?.RaiseEvent(levelChangeData);
                
                Debug.Log($"Player skill level updated: {previousLevel} -> {newSkillLevel}");
            }
        }
        
        private void UnlockNewGameMechanics(SkillNodeEventData skillData)
        {
            // This would integrate with other game systems to unlock new mechanics
            // For now, we'll log the potential unlock
            Debug.Log($"Checking for new game mechanics unlock for node: {skillData.NodeType}");
        }
        
        private void ProcessVisualizationUpdate(SkillTreeVisualizationEventData visualData)
        {
            // Process skill tree visualization updates
            Debug.Log("Skill tree visualization updated");
        }
        
        private void RefreshSkillTreeVisualization()
        {
            _treeVisualization?.RefreshVisualization();
            
            var visualEventData = new SkillTreeVisualizationEventData
            {
                UpdateType = "SkillProgression",
                Timestamp = Time.time
            };
            
            _onSkillTreeVisualizationUpdated?.RaiseEvent(visualEventData);
        }
        
        #endregion
        
        #region Utility Methods
        
        private float CalculateQualityMultiplier(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Perfect => 1.0f,
                CareQuality.Excellent => 0.8f,
                CareQuality.Good => 0.6f,
                CareQuality.Average => 0.4f,
                CareQuality.Poor => 0.2f,
                _ => 0.1f
            };
        }
        
        private string GetSkillCategoryForTask(string taskType)
        {
            return taskType.ToLower() switch
            {
                "watering" => "CultivationBasics",
                "feeding" => "CultivationBasics",
                "pruning" => "AdvancedTechniques",
                "training" => "AdvancedTechniques",
                "breeding" => "PlantGenetics",
                "genetics" => "PlantGenetics",
                "environmental" => "EnvironmentalControl",
                "harvest" => "HarvestOptimization",
                "business" => "BusinessManagement",
                _ => "CultivationBasics"
            };
        }
        
        private float GetCategoryMultiplier(string taskType)
        {
            var category = GetSkillCategoryForTask(taskType);
            var currentExp = GetSkillCategoryExperience(category);
            
            // Provide bonus for developing weaker skill areas
            return currentExp < 100f ? 1.2f : 1.0f;
        }
        
        private void CheckSkillLevelProgression()
        {
            var requiredExp = CalculateRequiredExperienceForLevel(_currentSkillState.PlayerSkillLevel);
            
            if (_currentSkillState.TotalExperiencePoints >= requiredExp)
            {
                var newLevel = CalculateSkillLevelFromExperience(_currentSkillState.TotalExperiencePoints);
                if (newLevel != _currentSkillState.PlayerSkillLevel)
                {
                    var previousLevel = _currentSkillState.PlayerSkillLevel;
                    _currentSkillState.PlayerSkillLevel = newLevel;
                    
                    var levelChangeData = new PlayerSkillLevelEventData
                    {
                        PreviousLevel = previousLevel,
                        NewLevel = newLevel,
                        Timestamp = Time.time
                    };
                    
                    _onPlayerSkillLevelChanged?.RaiseEvent(levelChangeData);
                }
            }
        }
        
        private int CalculateRequiredExperienceForLevel(SkillLevel level)
        {
            return level switch
            {
                SkillLevel.Beginner => 0,
                SkillLevel.Novice => 100,
                SkillLevel.Intermediate => 300,
                SkillLevel.Advanced => 600,
                SkillLevel.Expert => 1000,
                SkillLevel.Master => 1500,
                _ => 0
            };
        }
        
        private SkillLevel CalculateSkillLevelFromExperience(int experience)
        {
            return experience switch
            {
                >= 1500 => SkillLevel.Master,
                >= 1000 => SkillLevel.Expert,
                >= 600 => SkillLevel.Advanced,
                >= 300 => SkillLevel.Intermediate,
                >= 100 => SkillLevel.Novice,
                _ => SkillLevel.Beginner
            };
        }
        
        private void TriggerSkillExperienceGainedEvent(string taskType, float experienceGain, ProgressionExperienceSource source)
        {
            var eventData = new SkillExperienceGainedEventData
            {
                TaskType = taskType,
                ExperienceGained = experienceGain,
                Source = source,
                Timestamp = Time.time
            };
            
            _onSkillExperienceGained?.RaiseEvent(eventData);
        }
        
        private void LoadSkillProgressionData()
        {
            // Implementation for loading saved skill progression data
            // This would integrate with the save/load system
            Debug.Log("Loading skill progression data...");
        }
        
        private void SaveSkillProgressionMetrics()
        {
            var metrics = GetSkillProgressionMetrics();
            // Save metrics to persistent storage
            Debug.Log($"Skill Progression Session Completed - Nodes: {metrics.SkillNodesProgressed}, " +
                            $"Experience: {metrics.TotalExperienceGained}, " +
                            $"Level: {metrics.CurrentSkillLevel}");
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!_isInitialized) return;
            
            // Update skill tree systems
            _skillTreeSystem?.UpdateSystem(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            ShutdownService();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class SkillTreeGamingState
    {
        public SkillLevel PlayerSkillLevel;
        public int TotalExperiencePoints;
        public int UnlockedNodesCount;
        public string CurrentSkillCategory;
    }
    
    [System.Serializable]
    public class SkillProgressionMetrics
    {
        public int SkillNodesProgressed;
        public int TotalExperienceGained;
        public SkillLevel CurrentSkillLevel;
        public int UnlockedNodesCount;
        public Dictionary<string, float> SkillCategoryProgression;
    }
    
    [System.Serializable]
    public class SkillExperienceGainedEventData
    {
        public string TaskType;
        public float ExperienceGained;
        public ProgressionExperienceSource Source;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class PlayerSkillLevelEventData
    {
        public SkillLevel PreviousLevel;
        public SkillLevel NewLevel;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class SkillTreeVisualizationEventData
    {
        public string UpdateType;
        public float Timestamp;
    }
    
    // CareQuality is now defined in PlantCareGamingService.cs to avoid duplicates
    
    #endregion
}