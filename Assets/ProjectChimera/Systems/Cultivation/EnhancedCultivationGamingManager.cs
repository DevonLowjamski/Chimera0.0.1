using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
// using TimeScaleEventData = ProjectChimera.Events.TimeScaleEventData; // Commented out - namespace doesn't exist
using ProjectChimera.Data.Construction; // For SkillLevel enum
using ProjectChimera.Data.Events; // For event data structures including PlayerChoiceEventData
using ProjectChimera.Core.Events; // For cultivation gaming event data
// using ProjectChimera.Events; // Removed - namespace doesn't exist
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression; // For ExperienceSource enum
// Type aliases to resolve ambiguities
using DataCultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;
using ProgressionExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;
using EventsPlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using EventsChoiceConsequences = ProjectChimera.Events.ChoiceConsequences;
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;
// using EventsPlayerChoice = ProjectChimera.Events.PlayerChoice; // Using local PlayerChoice class instead

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Enhanced Cultivation Gaming Manager - Core gaming system for interactive cultivation mechanics
    /// Implements earned automation progression, skill tree gaming, time acceleration, and player agency
    /// Based on Tech Spec 10 v2.0: Enhanced Cultivation Gaming System
    /// </summary>
    public class EnhancedCultivationGamingManager : ChimeraManager, IChimeraManager
    {
        [Header("Interactive Plant Care Systems")]
        [SerializeField] private InteractivePlantCareSystem _plantCareSystem;
        [SerializeField] private PlantInteractionController _interactionController;
        [SerializeField] private CareToolManager _careToolManager;
        
        [Header("Earned Automation Systems")]
        [SerializeField] private EarnedAutomationProgressionSystem _automationProgressionSystem;
        [SerializeField] private AutomationUnlockManager _automationUnlocks;
        [SerializeField] private ManualTaskBurdenCalculator _burdenCalculator;
        
        [Header("Skill Tree Gaming Systems")]
        [SerializeField] private TreeSkillProgressionSystem _skillTreeSystem;
        [SerializeField] private SkillNodeUnlockManager _nodeUnlockManager;
        [SerializeField] private SkillTreeVisualizationController _treeVisualization;
        
        [Header("Time Acceleration Gaming Systems")]
        [SerializeField] private TimeAccelerationGamingSystem _timeAccelerationSystem;
        [SerializeField] private TimeTransitionManager _timeTransitionManager;
        [SerializeField] private GameSpeedController _gameSpeedController;
        
        [Header("Player Agency Gaming Systems")]
        [SerializeField] private PlayerAgencyGamingSystem _playerAgencySystem;
        [SerializeField] private CultivationPathManager _cultivationPathManager;
        [SerializeField] private FacilityDesignGamingSystem _facilityDesignSystem;
        
        [Header("Configuration")]
        [SerializeField] private EnhancedCultivationGamingConfigSO _cultivationGamingConfig;
        
        [Header("Gaming Event Channels")]
        [SerializeField] private GameEventChannelSO _onPlantCarePerformed;
        [SerializeField] private GameEventChannelSO _onAutomationUnlocked;
        [SerializeField] private GameEventChannelSO _onSkillNodeUnlocked;
        [SerializeField] private GameEventChannelSO _onTimeScaleChanged;
        [SerializeField] private GameEventChannelSO _onPlayerChoiceMade;
        [SerializeField] private GameEventChannelSO _onFacilityDesignCompleted;
        [SerializeField] private GameEventChannelSO _onCultivationPathSelected;
        [SerializeField] private GameEventChannelSO _onManualTaskBurdenIncreased;
        [SerializeField] private GameEventChannelSO _onAutomationBenefitRealized;
        [SerializeField] private GameEventChannelSO _onSkillTreeVisualizationUpdated;
        
        // Manager State
        private bool _isInitialized = false;
        private CultivationGamingState _currentGamingState;
        
        // Gaming Performance Metrics
        private float _sessionStartTime;
        private int _careActionsPerformed = 0;
        private int _automationSystemsUnlocked = 0;
        private int _skillNodesProgressed = 0;
        private int _playerChoicesMade = 0;
        
        #region IChimeraManager Implementation
        
        public bool IsInitialized => _isInitialized;
        public string ManagerName => "Enhanced Cultivation Gaming Manager";
        
        protected override void OnManagerInitialize()
        {
            InitializeManager();
        }
        
        public void Initialize()
        {
            InitializeManager();
        }
        
        public void Shutdown()
        {
            ShutdownManager();
        }
        
        public void InitializeManager()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("EnhancedCultivationGamingManager already initialized", this);
                return;
            }
            
            try
            {
                ValidateConfiguration();
                InitializeInteractivePlantCare();
                InitializeEarnedAutomationProgression();
                InitializeSkillTreeGaming();
                InitializeTimeAccelerationGaming();
                InitializePlayerAgencyGaming();
                InitializeGamingState();
                RegisterEventHandlers();
                
                _sessionStartTime = Time.time;
                _isInitialized = true;
                
                ChimeraLogger.Log("EnhancedCultivationGamingManager initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize EnhancedCultivationGamingManager: {ex.Message}", this);
                throw;
            }
        }
        
        public void ShutdownManager()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            SaveGamingMetrics();
            
            _isInitialized = false;
            ChimeraLogger.Log("EnhancedCultivationGamingManager shutdown completed", this);
        }
        
        #endregion
        
        #region System Initialization
        
        private void ValidateConfiguration()
        {
            if (_cultivationGamingConfig == null)
                throw new System.NullReferenceException("EnhancedCultivationGamingConfigSO is required");
                
            // Validate all required systems are assigned
            if (_plantCareSystem == null) CreateInteractivePlantCareSystem();
            if (_automationProgressionSystem == null) CreateEarnedAutomationProgressionSystem();
            if (_skillTreeSystem == null) CreateTreeSkillProgressionSystem();
            if (_timeAccelerationSystem == null) CreateTimeAccelerationGamingSystem();
            if (_playerAgencySystem == null) CreatePlayerAgencyGamingSystem();
        }
        
        private void InitializeInteractivePlantCare()
        {
            ChimeraLogger.Log("Initializing Interactive Plant Care Systems...", this);
            
            _plantCareSystem?.Initialize(_cultivationGamingConfig?.InteractivePlantCareConfig);
            _interactionController?.Initialize(_cultivationGamingConfig?.PlantInteractionConfig);
            _careToolManager?.Initialize(_cultivationGamingConfig?.CareToolLibrary);
            
            ChimeraLogger.Log("Interactive Plant Care Systems initialized", this);
        }
        
        private void InitializeEarnedAutomationProgression()
        {
            ChimeraLogger.Log("Initializing Earned Automation Progression Systems...", this);
            
            _automationProgressionSystem?.Initialize(_cultivationGamingConfig?.EarnedAutomationConfig);
            _automationUnlocks?.Initialize();
            _burdenCalculator?.Initialize();
            
            ChimeraLogger.Log("Earned Automation Progression Systems initialized", this);
        }
        
        private void InitializeSkillTreeGaming()
        {
            ChimeraLogger.Log("Initializing Skill Tree Gaming Systems...", this);
            
            _skillTreeSystem?.Initialize(_cultivationGamingConfig?.SkillNodeLibrary, _cultivationGamingConfig?.SkillTreeConfigSO);
            _nodeUnlockManager?.Initialize();
            _treeVisualization?.Initialize();
            
            ChimeraLogger.Log("Skill Tree Gaming Systems initialized", this);
        }
        
        private void InitializeTimeAccelerationGaming()
        {
            ChimeraLogger.Log("Initializing Time Acceleration Gaming Systems...", this);
            
            _timeAccelerationSystem?.Initialize(_cultivationGamingConfig?.TimeAccelerationGamingConfig);
            _timeTransitionManager?.Initialize(_cultivationGamingConfig?.TimeTransitionConfig);
            _gameSpeedController?.Initialize(_cultivationGamingConfig?.TimeAccelerationGamingConfig);
            
            ChimeraLogger.Log("Time Acceleration Gaming Systems initialized", this);
        }
        
        private void InitializePlayerAgencyGaming()
        {
            ChimeraLogger.Log("Initializing Player Agency Gaming Systems...", this);
            
            _playerAgencySystem?.Initialize(_cultivationGamingConfig?.PlayerAgencyGamingConfig);
            _cultivationPathManager?.Initialize(_cultivationGamingConfig?.CultivationPathLibrary);
            _facilityDesignSystem?.Initialize(_cultivationGamingConfig?.FacilityDesignLibrary);
            
            ChimeraLogger.Log("Player Agency Gaming Systems initialized", this);
        }
        
        private void InitializeGamingState()
        {
            _currentGamingState = new CultivationGamingState
            {
                SessionStartTime = Time.time,
                CurrentTimeScale = GameTimeScale.Baseline,
                AutomationLevel = AutomationLevel.FullyManual,
                PlayerSkillLevel = SkillLevel.Beginner,
                CultivationApproach = DataCultivationApproach.OrganicTraditional,
                FacilityDesignApproach = FacilityDesignApproach.MinimalistEfficient
            };
        }
        
        #endregion
        
        #region System Creation (Auto-instantiation)
        
        private void CreateInteractivePlantCareSystem()
        {
            var systemGO = new GameObject("InteractivePlantCareSystem");
            systemGO.transform.SetParent(transform);
            _plantCareSystem = systemGO.AddComponent<InteractivePlantCareSystem>();
            
            var controllerGO = new GameObject("PlantInteractionController");
            controllerGO.transform.SetParent(systemGO.transform);
            _interactionController = controllerGO.AddComponent<PlantInteractionController>();
            
            var toolManagerGO = new GameObject("CareToolManager");
            toolManagerGO.transform.SetParent(systemGO.transform);
            _careToolManager = toolManagerGO.AddComponent<CareToolManager>();
        }
        
        private void CreateEarnedAutomationProgressionSystem()
        {
            var systemGO = new GameObject("EarnedAutomationProgressionSystem");
            systemGO.transform.SetParent(transform);
            _automationProgressionSystem = systemGO.AddComponent<EarnedAutomationProgressionSystem>();
            
            var unlockManagerGO = new GameObject("AutomationUnlockManager");
            unlockManagerGO.transform.SetParent(systemGO.transform);
            _automationUnlocks = unlockManagerGO.AddComponent<AutomationUnlockManager>();
            
            var burdenCalculatorGO = new GameObject("ManualTaskBurdenCalculator");
            burdenCalculatorGO.transform.SetParent(systemGO.transform);
            _burdenCalculator = burdenCalculatorGO.AddComponent<ManualTaskBurdenCalculator>();
        }
        
        private void CreateTreeSkillProgressionSystem()
        {
            var systemGO = new GameObject("TreeSkillProgressionSystem");
            systemGO.transform.SetParent(transform);
            _skillTreeSystem = systemGO.AddComponent<TreeSkillProgressionSystem>();
            
            var unlockManagerGO = new GameObject("SkillNodeUnlockManager");
            unlockManagerGO.transform.SetParent(systemGO.transform);
            _nodeUnlockManager = unlockManagerGO.AddComponent<SkillNodeUnlockManager>();
            
            var visualizationGO = new GameObject("SkillTreeVisualizationController");
            visualizationGO.transform.SetParent(systemGO.transform);
            _treeVisualization = visualizationGO.AddComponent<SkillTreeVisualizationController>();
        }
        
        private void CreateTimeAccelerationGamingSystem()
        {
            var systemGO = new GameObject("TimeAccelerationGamingSystem");
            systemGO.transform.SetParent(transform);
            _timeAccelerationSystem = systemGO.AddComponent<TimeAccelerationGamingSystem>();
            
            var transitionManagerGO = new GameObject("TimeTransitionManager");
            transitionManagerGO.transform.SetParent(systemGO.transform);
            _timeTransitionManager = transitionManagerGO.AddComponent<TimeTransitionManager>();
            
            var speedControllerGO = new GameObject("GameSpeedController");
            speedControllerGO.transform.SetParent(systemGO.transform);
            _gameSpeedController = speedControllerGO.AddComponent<GameSpeedController>();
        }
        
        private void CreatePlayerAgencyGamingSystem()
        {
            var systemGO = new GameObject("PlayerAgencyGamingSystem");
            systemGO.transform.SetParent(transform);
            _playerAgencySystem = systemGO.AddComponent<PlayerAgencyGamingSystem>();
            
            var pathManagerGO = new GameObject("CultivationPathManager");
            pathManagerGO.transform.SetParent(systemGO.transform);
            _cultivationPathManager = pathManagerGO.AddComponent<CultivationPathManager>();
            
            var facilityDesignGO = new GameObject("FacilityDesignGamingSystem");
            facilityDesignGO.transform.SetParent(systemGO.transform);
            _facilityDesignSystem = facilityDesignGO.AddComponent<FacilityDesignGamingSystem>();
        }
        
        #endregion
        
        #region Event System
        
        private void RegisterEventHandlers()
        {
            if (_onPlantCarePerformed != null)
                _onPlantCarePerformed.OnEventRaisedWithData.AddListener(OnPlantCarePerformed);
                
            if (_onAutomationUnlocked != null)
                _onAutomationUnlocked.OnEventRaisedWithData.AddListener(OnAutomationUnlocked);
                
            if (_onSkillNodeUnlocked != null)
                _onSkillNodeUnlocked.OnEventRaisedWithData.AddListener(OnSkillNodeUnlocked);
                
            if (_onTimeScaleChanged != null)
                _onTimeScaleChanged.OnEventRaisedWithData.AddListener(OnTimeScaleChanged);
                
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.AddListener(OnPlayerChoiceMade);
        }
        
        private void UnregisterEventHandlers()
        {
            if (_onPlantCarePerformed != null)
                _onPlantCarePerformed.OnEventRaisedWithData.RemoveListener(OnPlantCarePerformed);
                
            if (_onAutomationUnlocked != null)
                _onAutomationUnlocked.OnEventRaisedWithData.RemoveListener(OnAutomationUnlocked);
                
            if (_onSkillNodeUnlocked != null)
                _onSkillNodeUnlocked.OnEventRaisedWithData.RemoveListener(OnSkillNodeUnlocked);
                
            if (_onTimeScaleChanged != null)
                _onTimeScaleChanged.OnEventRaisedWithData.RemoveListener(OnTimeScaleChanged);
                
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.RemoveListener(OnPlayerChoiceMade);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlantCarePerformed(object eventData)
        {
            _careActionsPerformed++;
            
            if (eventData is PlantCareEventData careData)
            {
                ProcessCareActionFeedback(careData);
                UpdateSkillProgression(careData);
                EvaluateAutomationDesire(careData);
            }
        }
        
        private void OnAutomationUnlocked(object eventData)
        {
            _automationSystemsUnlocked++;
            
            if (eventData is AutomationUnlockEventData unlockData)
            {
                UpdateAutomationLevel(unlockData);
                TriggerAutomationBenefitRealization(unlockData);
            }
        }
        
        private void OnSkillNodeUnlocked(object eventData)
        {
            _skillNodesProgressed++;
            
            if (eventData is SkillNodeEventData skillData)
            {
                UpdatePlayerSkillLevel(skillData);
                UnlockNewGameMechanics(skillData);
            }
        }
        
        private void OnTimeScaleChanged(object eventData)
        {
            if (eventData is TimeScaleEventData timeData)
            {
                _currentGamingState.CurrentTimeScale = timeData.NewTimeScale;
                AdjustGameplayComplexity(timeData.NewTimeScale);
            }
        }
        
        private void OnPlayerChoiceMade(object eventData)
        {
            _playerChoicesMade++;
            
            if (eventData is EventsPlayerChoiceEventData choiceData)
            {
                ProcessPlayerChoiceConsequences(choiceData);
                UpdateCultivationPath(choiceData);
            }
        }
        
        #endregion
        
        #region Core Gaming Mechanics
        
        private void ProcessCareActionFeedback(PlantCareEventData careData)
        {
            // Provide immediate visual and audio feedback for care actions
            var careQuality = ParseCareQuality(careData.CareQuality);
            var careType = ParseCultivationTaskType(careData.CareType);
            var feedbackIntensity = CalculateFeedbackIntensity(careQuality);
            
            // Safe cast for PlantInstance
            if (careData.PlantInstance is InteractivePlant plant)
            {
                TriggerVisualFeedback(plant, feedbackIntensity);
            }
            TriggerAudioFeedback(careType, feedbackIntensity);
        }
        
        private void UpdateSkillProgression(PlantCareEventData careData)
        {
            var careQuality = ParseCareQuality(careData.CareQuality);
            var careType = ParseCultivationTaskType(careData.CareType);
            var skillGain = CalculateSkillGain(careType, careQuality);
            _skillTreeSystem?.AddSkillExperience(careData.CareType, skillGain, ProgressionExperienceSource.PlantCare);
        }
        
        private void EvaluateAutomationDesire(PlantCareEventData careData)
        {
            var currentBurden = _burdenCalculator?.CalculateCurrentBurden(careData);
            if (currentBurden == AutomationDesireLevel.High || currentBurden == AutomationDesireLevel.Critical)
            {
                var careType = ParseCultivationTaskType(careData.CareType);
                _automationUnlocks?.EvaluateUnlockEligibility(careType);
            }
        }
        
        private void UpdateAutomationLevel(AutomationUnlockEventData unlockData)
        {
            _currentGamingState.AutomationLevel = CalculateNewAutomationLevel(unlockData);
            _automationProgressionSystem?.ApplyAutomationBenefits(unlockData);
        }
        
        private void AdjustGameplayComplexity(GameTimeScale newTimeScale)
        {
            var complexityMultiplier = CalculateComplexityMultiplier(newTimeScale);
            _plantCareSystem?.AdjustTaskComplexity(complexityMultiplier);
            _burdenCalculator?.UpdateBurdenCalculations(complexityMultiplier);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Perform a care action on a plant with specified tool and technique
        /// </summary>
        public PlantCareResult PerformPlantCare(InteractivePlant plant, CareAction action)
        {
            if (!_isInitialized || plant == null || action == null)
                return PlantCareResult.Failed;
                
            return _plantCareSystem?.ProcessCareAction(plant, action) ?? PlantCareResult.Failed;
        }
        
        /// <summary>
        /// Get current automation desire level for specific task type
        /// </summary>
        public AutomationDesireLevel GetAutomationDesire(CultivationTaskType taskType)
        {
            return _burdenCalculator?.GetAutomationDesire(taskType) ?? AutomationDesireLevel.None;
        }
        
        /// <summary>
        /// Unlock automation system for specified task type
        /// </summary>
        public bool UnlockAutomationSystem(CultivationTaskType taskType, AutomationSystemType systemType)
        {
            return _automationUnlocks?.IsSystemUnlocked(systemType) ?? false;
        }
        
        /// <summary>
        /// Progress skill tree node with specified skill points
        /// </summary>
        public bool ProgressSkillNode(SkillNodeType nodeType, int skillPoints)
        {
            return _skillTreeSystem?.ProgressNode(nodeType, skillPoints) ?? false;
        }
        
        /// <summary>
        /// Change game time scale with transition management
        /// </summary>
        public bool ChangeTimeScale(GameTimeScale newScale)
        {
            return _timeTransitionManager?.RequestTimeScaleChange(newScale) ?? false;
        }
        
        /// <summary>
        /// Make player choice that affects cultivation path
        /// </summary>
        public EventsChoiceConsequences MakePlayerChoice(PlayerChoice choice)
        {
            return _playerAgencySystem?.ProcessPlayerChoice(choice) ?? EventsChoiceConsequences.None;
        }
        
        /// <summary>
        /// Get current cultivation gaming state
        /// </summary>
        public CultivationGamingState GetCurrentGamingState()
        {
            return _currentGamingState;
        }
        
        /// <summary>
        /// Get gaming performance metrics for current session
        /// </summary>
        public CultivationGamingMetrics GetSessionMetrics()
        {
            return new CultivationGamingMetrics
            {
                SessionDuration = Time.time - _sessionStartTime,
                CareActionsPerformed = _careActionsPerformed,
                AutomationSystemsUnlocked = _automationSystemsUnlocked,
                SkillNodesProgressed = _skillNodesProgressed,
                PlayerChoicesMade = _playerChoicesMade,
                CurrentEngagementLevel = CalculateEngagementLevel()
            };
        }
        
        #endregion
        
        #region Utility Methods
        
        private float CalculateFeedbackIntensity(CareQuality quality)
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
        
        private float CalculateSkillGain(CultivationTaskType taskType, CareQuality quality)
        {
            var baseGain = _cultivationGamingConfig.SkillProgressionRate;
            var qualityMultiplier = CalculateFeedbackIntensity(quality);
            return baseGain * qualityMultiplier;
        }
        
        private AutomationLevel CalculateNewAutomationLevel(AutomationUnlockEventData unlockData)
        {
            // Calculate new automation level based on unlocked systems
            var totalSystems = _automationUnlocks?.GetTotalUnlockedSystems() ?? 0;
            return totalSystems switch
            {
                0 => AutomationLevel.FullyManual,
                1 => AutomationLevel.BasicAutomation,
                <= 3 => AutomationLevel.IntermediateAutomation,
                <= 5 => AutomationLevel.AdvancedAutomation,
                _ => AutomationLevel.FullyAutomated
            };
        }
        
        private float CalculateComplexityMultiplier(GameTimeScale timeScale)
        {
            return timeScale switch
            {
                GameTimeScale.SlowMotion => 0.5f,
                GameTimeScale.Baseline => 1.0f,
                GameTimeScale.Standard => 1.2f,
                GameTimeScale.Fast => 1.5f,
                GameTimeScale.VeryFast => 2.0f,
                GameTimeScale.Lightning => 2.5f,
                _ => 1.0f
            };
        }
        
        private float CalculateEngagementLevel()
        {
            var sessionDuration = Time.time - _sessionStartTime;
            var actionsPerMinute = sessionDuration > 0 ? (_careActionsPerformed / (sessionDuration / 60f)) : 0;
            
            return Mathf.Clamp01(actionsPerMinute / 10f); // Normalize to 0-1 based on 10 actions per minute target
        }
        
        private void TriggerVisualFeedback(InteractivePlant plant, float intensity)
        {
            // Implementation for visual feedback system
            // This would integrate with plant visualization and effects systems
        }
        
        private void TriggerAudioFeedback(CultivationTaskType taskType, float intensity)
        {
            // Implementation for audio feedback system
            // This would play appropriate sounds based on care action and quality
        }
        
        private void SaveGamingMetrics()
        {
            var metrics = GetSessionMetrics();
            // Save metrics to persistent storage for analytics
            ChimeraLogger.Log($"Gaming Session Completed - Duration: {metrics.SessionDuration:F1}s, " +
                            $"Care Actions: {metrics.CareActionsPerformed}, " +
                            $"Engagement: {metrics.CurrentEngagementLevel:F2}", this);
        }
        
        private void ProcessPlayerChoiceConsequences(EventsPlayerChoiceEventData choiceData)
        {
            // Process immediate and long-term consequences of player choices
            _playerAgencySystem?.ApplyChoiceConsequences(choiceData);
        }
        
        private void UpdateCultivationPath(EventsPlayerChoiceEventData choiceData)
        {
            // Update player's cultivation path based on their choices
            _cultivationPathManager?.UpdatePath(choiceData);
        }
        
        private void TriggerAutomationBenefitRealization(AutomationUnlockEventData unlockData)
        {
            // Trigger event to show automation benefits being realized
            _onAutomationBenefitRealized?.RaiseEvent(unlockData);
        }
        
        private void UpdatePlayerSkillLevel(SkillNodeEventData skillData)
        {
            // Update overall player skill level based on skill tree progression
            _currentGamingState.PlayerSkillLevel = _skillTreeSystem?.GetOverallSkillLevel() ?? SkillLevel.Beginner;
        }
        
        private void UnlockNewGameMechanics(SkillNodeEventData skillData)
        {
            // Unlock new game mechanics based on skill progression
            _plantCareSystem?.UnlockNewMechanics(skillData.NodeType);
        }
        
        // Helper methods to convert string properties to enums for compatibility
        private CareQuality ParseCareQuality(string careQualityString)
        {
            if (System.Enum.TryParse<CareQuality>(careQualityString, true, out var result))
            {
                return result;
            }
            return CareQuality.Average; // Default fallback
        }
        
        private CultivationTaskType ParseCultivationTaskType(string taskTypeString)
        {
            if (System.Enum.TryParse<CultivationTaskType>(taskTypeString, true, out var result))
            {
                return result;
            }
            return CultivationTaskType.Watering; // Default fallback
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Update()
        {
            if (!_isInitialized) return;
            
            // Update all gaming systems
            _plantCareSystem?.UpdateSystem(Time.deltaTime);
            _automationProgressionSystem?.UpdateSystem(Time.deltaTime);
            _skillTreeSystem?.UpdateSystem(Time.deltaTime);
            _timeAccelerationSystem?.UpdateSystem();
            _playerAgencySystem?.UpdateSystem(Time.deltaTime);
        }
        
        protected override void OnManagerShutdown()
        {
            ShutdownManager();
        }
        
        private void OnDestroy()
        {
            ShutdownManager();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class CultivationGamingState
    {
        public float SessionStartTime;
        public GameTimeScale CurrentTimeScale;
        public AutomationLevel AutomationLevel;
        public SkillLevel PlayerSkillLevel;
        public DataCultivationApproach CultivationApproach;
        public FacilityDesignApproach FacilityDesignApproach;
    }
    
    [System.Serializable]
    public class CultivationGamingMetrics
    {
        public float SessionDuration;
        public int CareActionsPerformed;
        public int AutomationSystemsUnlocked;
        public int SkillNodesProgressed;
        public int PlayerChoicesMade;
        public float CurrentEngagementLevel;
    }
    
    // Gaming Enums
    // GameTimeScale enum is defined in TimeAccelerationGamingConfigSO.cs to avoid duplicates
    // CultivationTaskType enum is defined in CultivationTaskData.cs to avoid duplicates
    
    public enum AutomationLevel
    {
        FullyManual,
        BasicAutomation,
        IntermediateAutomation,
        AdvancedAutomation,
        FullyAutomated
    }
    
    // SkillLevel enum is defined in ProjectChimera.Data.Construction.SkillLevel to avoid duplicates
    
    // All shared enums are defined in CultivationDataStructures.cs, CultivationGamingDataStructures.cs, and SkillTreeDataStructures.cs to avoid duplicates
    // AutomationDesire enum is defined in ManualTaskBurdenCalculator.cs to avoid duplicates
    
    public enum PlantCareResult
    {
        Perfect,
        Successful,
        Adequate,
        Suboptimal,
        Failed
    }
    
    public enum CareQuality
    {
        Failed,
        Poor,
        Adequate,
        Average,
        Good,
        Excellent,
        Perfect
    }
    
    /// <summary>
    /// Local definition of TimeScaleEventData to avoid assembly reference issues
    /// </summary>
    [System.Serializable]
    public class TimeScaleEventData
    {
        public string EventId;
        public GameTimeScale NewTimeScale;
        public GameTimeScale PreviousTimeScale;
        public float Timestamp;
    }

    
    #endregion
}