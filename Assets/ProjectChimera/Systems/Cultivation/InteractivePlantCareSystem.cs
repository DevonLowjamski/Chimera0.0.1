using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Events;
using ProjectChimera.Data.Genetics; // For PlantGrowthStage enum
using ProjectChimera.Events; // For event data classes
// Type aliases to resolve ambiguous references - use consistent naming
using CultivationTaskType = ProjectChimera.Data.Cultivation.CultivationTaskType;
using SkillNodeType = ProjectChimera.Data.Cultivation.SkillNodeType;
using InteractivePlant = ProjectChimera.Data.Cultivation.InteractivePlant;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
// Event system aliases
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;
using SkillProgressionEventData = ProjectChimera.Core.Events.SkillProgressionEventData;
// Local system types
using CareAction = ProjectChimera.Systems.Cultivation.CareAction;
using CareQuality = ProjectChimera.Systems.Cultivation.CareQuality;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Interactive Plant Care System - Hands-on cultivation mechanics with satisfying feedback
    /// Implements direct plant manipulation, tool-based care actions, and skill-based precision
    /// Core component of Enhanced Cultivation Gaming System v2.0
    /// </summary>
    public class InteractivePlantCareSystem : MonoBehaviour
    {
        [Header("Care System Configuration")]
        [SerializeField] private InteractivePlantCareConfigSO _careConfig;
        [SerializeField] private CareToolLibrarySO _toolLibrary;
        
        [Header("Visual Feedback Systems")]
        [SerializeField] private PlantResponseVisualization _responseVisualization;
        [SerializeField] private CareEffectParticleSystem _particleEffects;
        //[SerializeField] private PlantHealthIndicatorSystem _healthIndicators; // Disabled - was using legacy UI
        
        [Header("Audio Feedback Systems")]
        [SerializeField] private AudioSource _careAudioSource;
        [SerializeField] private CareAudioLibrarySO _audioLibrary;
        
        [Header("Care Action Settings")]
        [Range(0.1f, 2.0f)] public float CareActionBaseEfficiency = 1.0f;
        [Range(0.1f, 3.0f)] public float SkillBasedPrecisionMultiplier = 1.5f;
        [Range(0.5f, 2.0f)] public float FeedbackIntensityMultiplier = 1.0f;
        [Range(0.1f, 1.0f)] public float CareSuccessThreshold = 0.6f;
        
        // System State
        private bool _isInitialized = false;
        private Dictionary<int, PlantCareState> _plantCareStates = new Dictionary<int, PlantCareState>();
        private List<CareAction> _activeCareActions = new List<CareAction>();
        private CareToolManager _toolManager;
        private PlantInteractionController _interactionController;
        
        // Care Performance Tracking
        private int _totalCareActionsPerformed = 0;
        private float _averageCareQuality = 0f;
        private Dictionary<CultivationTaskType, int> _careActionCounts = new Dictionary<CultivationTaskType, int>();
        private Dictionary<CultivationTaskType, float> _skillLevels = new Dictionary<CultivationTaskType, float>();
        
        // Events
        private GameEventChannelSO _onPlantCarePerformed;
        private GameEventChannelSO _onCareQualityImproved;
        private GameEventChannelSO _onSkillProgressionTriggered;
        
        #region Initialization
        
        public void Initialize(InteractivePlantCareConfigSO config)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("InteractivePlantCareSystem already initialized", this);
                return;
            }
            
            _careConfig = config ?? _careConfig;
            
            if (_careConfig == null)
            {
                ChimeraLogger.LogError("InteractivePlantCareConfigSO is required for initialization", this);
                return;
            }
            
            InitializeCareSystems();
            InitializeFeedbackSystems();
            InitializeSkillTracking();
            SetupEventChannels();
            
            _isInitialized = true;
            ChimeraLogger.Log("InteractivePlantCareSystem initialized successfully", this);
        }
        
        private void InitializeCareSystems()
        {
            // Initialize care action processing
            _activeCareActions.Clear();
            _plantCareStates.Clear();
            
            // Initialize tool manager if not already assigned
            if (_toolManager == null)
            {
                _toolManager = GetComponentInChildren<CareToolManager>();
                if (_toolManager == null)
                {
                    var toolManagerGO = new GameObject("CareToolManager");
                    toolManagerGO.transform.SetParent(transform);
                    _toolManager = toolManagerGO.AddComponent<CareToolManager>();
                }
            }
            
            // Initialize interaction controller if not already assigned
            if (_interactionController == null)
            {
                _interactionController = GetComponentInChildren<PlantInteractionController>();
                if (_interactionController == null)
                {
                    var controllerGO = new GameObject("PlantInteractionController");
                    controllerGO.transform.SetParent(transform);
                    _interactionController = controllerGO.AddComponent<PlantInteractionController>();
                }
            }
        }
        
        private void InitializeFeedbackSystems()
        {
            // Initialize visual feedback systems
            if (_responseVisualization == null)
            {
                var visualGO = new GameObject("PlantResponseVisualization");
                visualGO.transform.SetParent(transform);
                _responseVisualization = visualGO.AddComponent<PlantResponseVisualization>();
            }
            
            if (_particleEffects == null)
            {
                var particleGO = new GameObject("CareEffectParticleSystem");
                particleGO.transform.SetParent(transform);
                _particleEffects = particleGO.AddComponent<CareEffectParticleSystem>();
            }
            
            // Health indicators system disabled (was using legacy UI)
            // if (_healthIndicators == null)
            // {
            //     var indicatorGO = new GameObject("PlantHealthIndicatorSystem");
            //     indicatorGO.transform.SetParent(transform);
            //     _healthIndicators = indicatorGO.AddComponent<PlantHealthIndicatorSystem>();
            // }
            
            ChimeraLogger.Log("Interactive Plant Care System", "Health indicators system ready (legacy UI disabled)");
            
            // Initialize audio feedback
            if (_careAudioSource == null)
            {
                _careAudioSource = gameObject.AddComponent<AudioSource>();
                _careAudioSource.volume = 0.5f;
                _careAudioSource.spatialBlend = 0.7f; // 3D audio for spatial feedback
            }
        }
        
        private void InitializeSkillTracking()
        {
            // Initialize skill levels for all care types
            foreach (CultivationTaskType taskType in System.Enum.GetValues(typeof(CultivationTaskType)))
            {
                _skillLevels[taskType] = _careConfig.BaseSkillLevel;
                _careActionCounts[taskType] = 0;
            }
        }
        
        private void SetupEventChannels()
        {
            // Get event channels from ChimeraManager or create them
            var cultivationManager = FindObjectOfType<EnhancedCultivationGamingManager>();
            if (cultivationManager != null)
            {
                // Event channels will be connected through the main manager
            }
        }
        
        #endregion
        
        #region Core Care Mechanics
        
        /// <summary>
        /// Process a care action on a plant with specified tool and technique
        /// </summary>
        public PlantCareResult ProcessCareAction(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            if (!_isInitialized || plant == null || action == null)
                return PlantCareResult.Failed;
            
            try
            {
                // Validate care action
                var validation = ValidateCareAction(plant, action);
                if (!validation.IsValid)
                {
                    TriggerFailedCareAudio(action.TaskType, validation.FailureReason);
                    return PlantCareResult.Failed;
                }
                
                // Calculate care quality based on skill, timing, and plant state
                var careQuality = CalculateCareQuality(plant, action);
                
                // Apply care effects to plant
                var careEffects = ApplyCareEffects(plant, action, careQuality);
                
                // Provide immediate feedback
                TriggerImmediateFeedback(plant, action, careQuality);
                
                // Update skill progression
                UpdateSkillProgression(action.TaskType, careQuality);
                
                // Track care action
                TrackCareAction(action, careQuality);
                
                // Raise care performed event
                RaisePlantCarePerformedEvent(plant, action, careQuality, careEffects);
                
                // Determine result based on care quality
                return DetermineCareResult(careQuality);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error processing care action: {ex.Message}", this);
                return PlantCareResult.Failed;
            }
        }
        
        /// <summary>
        /// Perform watering action with specified tool and amount
        /// </summary>
        public PlantCareResult PerformWatering(ProjectChimera.Data.Cultivation.InteractivePlant plant, WateringTool tool, float amount)
        {
            var wateringAction = new CareAction
            {
                TaskType = CultivationTaskType.Watering,
                Tool = tool,
                ActionData = new WateringActionData { Amount = amount, Tool = tool },
                Timestamp = Time.time,
                PlayerSkillLevel = GetCurrentSkillLevel(CultivationTaskType.Watering)
            };
            
            return ProcessCareAction(plant, wateringAction);
        }
        
        /// <summary>
        /// Perform pruning action with specified tool and technique
        /// </summary>
        public PlantCareResult PerformPruning(ProjectChimera.Data.Cultivation.InteractivePlant plant, PruningTool tool, PruningType pruningType)
        {
            var pruningAction = new CareAction
            {
                TaskType = CultivationTaskType.Pruning,
                Tool = tool,
                ActionData = new PruningActionData { PruningType = pruningType, Tool = tool },
                Timestamp = Time.time,
                PlayerSkillLevel = GetCurrentSkillLevel(CultivationTaskType.Pruning)
            };
            
            return ProcessCareAction(plant, pruningAction);
        }
        
        /// <summary>
        /// Perform training action with specified method
        /// </summary>
        public PlantCareResult PerformTraining(ProjectChimera.Data.Cultivation.InteractivePlant plant, TrainingTool tool, TrainingMethod method)
        {
            var trainingAction = new CareAction
            {
                TaskType = CultivationTaskType.Training,
                Tool = tool,
                ActionData = new TrainingActionData { Method = method, Tool = tool },
                Timestamp = Time.time,
                PlayerSkillLevel = GetCurrentSkillLevel(CultivationTaskType.Training)
            };
            
            return ProcessCareAction(plant, trainingAction);
        }
        
        /// <summary>
        /// Perform transplanting action with new container and medium
        /// </summary>
        public PlantCareResult PerformTransplanting(ProjectChimera.Data.Cultivation.InteractivePlant plant, Container newContainer, GrowingMedium medium)
        {
            var transplantAction = new CareAction
            {
                TaskType = CultivationTaskType.Transplanting,
                Tool = null, // Transplanting uses hands
                ActionData = new TransplantActionData { Container = newContainer, Medium = medium },
                Timestamp = Time.time,
                PlayerSkillLevel = GetCurrentSkillLevel(CultivationTaskType.Transplanting)
            };
            
            return ProcessCareAction(plant, transplantAction);
        }
        
        #endregion
        
        #region Care Quality and Skill System
        
        private CareQuality CalculateCareQuality(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            var baseQuality = CalculateBaseQuality(plant, action);
            var skillModifier = CalculateSkillModifier(action);
            var timingModifier = CalculateTimingModifier(plant, action);
            var toolModifier = CalculateToolModifier(action);
            var plantStateModifier = CalculatePlantStateModifier(plant);
            
            var finalQuality = baseQuality * skillModifier * timingModifier * toolModifier * plantStateModifier;
            finalQuality = Mathf.Clamp01(finalQuality);
            
            return ConvertToQualityEnum(finalQuality);
        }
        
        private float CalculateBaseQuality(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            // Base quality depends on action type and plant needs
            var plantNeeds = GetPlantNeeds(plant);
            var actionRelevance = CalculateActionRelevance(action.TaskType, plantNeeds);
            
            return CareActionBaseEfficiency * actionRelevance;
        }
        
        private float CalculateSkillModifier(CareAction action)
        {
            var currentSkill = GetCurrentSkillLevel(action.TaskType);
            var maxSkill = _careConfig.MaxSkillLevel;
            var skillRatio = currentSkill / maxSkill;
            
            return 1.0f + (skillRatio * (SkillBasedPrecisionMultiplier - 1.0f));
        }
        
        private float CalculateTimingModifier(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            // Better timing = better results
            var optimalTiming = GetOptimalCareTime(plant, action.TaskType);
            var currentTime = Time.time;
            var timingError = Mathf.Abs(currentTime - optimalTiming);
            
            return Mathf.Lerp(1.0f, 0.5f, timingError / _careConfig.MaxTimingWindow);
        }
        
        private float CalculateToolModifier(CareAction action)
        {
            if (action.Tool == null) return 1.0f;
            
            // Better tools = better results
            var toolQuality = action.Tool.Quality;
            return 1.0f + (toolQuality * _careConfig.ToolQualityBonus);
        }
        
        private float CalculatePlantStateModifier(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            // Healthier plants respond better to care
            var healthRatio = plant.CurrentHealth / plant.MaxHealth;
            return Mathf.Lerp(0.7f, 1.0f, healthRatio);
        }
        
        private CareQuality ConvertToQualityEnum(float qualityValue)
        {
            return qualityValue switch
            {
                >= 0.95f => CareQuality.Perfect,
                >= 0.85f => CareQuality.Excellent,
                >= 0.70f => CareQuality.Good,
                >= 0.50f => CareQuality.Average,
                >= 0.25f => CareQuality.Poor,
                _ => CareQuality.Failed
            };
        }
        
        #endregion
        
        #region Care Effects Application
        
        private CareEffects ApplyCareEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var effects = new CareEffects();
            
            switch (action.TaskType)
            {
                case CultivationTaskType.Watering:
                    effects = ApplyWateringEffects(plant, action, quality);
                    break;
                    
                case CultivationTaskType.Pruning:
                    effects = ApplyPruningEffects(plant, action, quality);
                    break;
                    
                case CultivationTaskType.Training:
                    effects = ApplyTrainingEffects(plant, action, quality);
                    break;
                    
                case CultivationTaskType.Fertilizing:
                    effects = ApplyFertilizingEffects(plant, action, quality);
                    break;
                    
                case CultivationTaskType.Transplanting:
                    effects = ApplyTransplantingEffects(plant, action, quality);
                    break;
            }
            
            // Apply effects to plant
            var taskType = ConvertCareActionToTaskType(action);
            var qualityFloat = ConvertCareQualityToFloat(quality);
            plant.ApplyCareEffects(taskType, qualityFloat);
            
            return effects;
        }
        
        private CareEffects ApplyWateringEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var wateringData = action.ActionData as WateringActionData;
            var effects = new CareEffects();
            
            // Calculate hydration effect based on quality and amount
            var hydrationEffect = CalculateHydrationEffect(plant, wateringData.Amount, quality);
            effects.HydrationChange = hydrationEffect;
            
            // Quality affects stress reduction
            effects.StressReduction = quality switch
            {
                CareQuality.Perfect => 0.3f,
                CareQuality.Excellent => 0.2f,
                CareQuality.Good => 0.1f,
                _ => 0f
            };
            
            return effects;
        }
        
        private CareEffects ApplyPruningEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var pruningData = action.ActionData as PruningActionData;
            var effects = new CareEffects();
            
            // Pruning affects growth distribution and health
            effects.GrowthRedirection = CalculatePruningGrowthEffect(pruningData.PruningType, quality);
            effects.HealthImprovement = quality == CareQuality.Perfect ? 0.1f : 0f;
            
            // Poor pruning can cause stress
            if (quality == CareQuality.Poor || quality == CareQuality.Failed)
            {
                effects.StressIncrease = 0.2f;
            }
            
            return effects;
        }
        
        private CareEffects ApplyTrainingEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var trainingData = action.ActionData as TrainingActionData;
            var effects = new CareEffects();
            
            // Training affects plant structure and yield potential
            effects.StructuralImprovement = CalculateTrainingStructureEffect(trainingData.Method, quality);
            effects.YieldPotentialIncrease = quality switch
            {
                CareQuality.Perfect => 0.05f,
                CareQuality.Excellent => 0.03f,
                CareQuality.Good => 0.01f,
                _ => 0f
            };
            
            return effects;
        }
        
        private CareEffects ApplyFertilizingEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var effects = new CareEffects();
            
            // Fertilizing affects nutrition and growth rate
            effects.NutritionImprovement = quality switch
            {
                CareQuality.Perfect => 0.4f,
                CareQuality.Excellent => 0.3f,
                CareQuality.Good => 0.2f,
                CareQuality.Average => 0.1f,
                _ => 0f
            };
            
            effects.GrowthRateBoost = effects.NutritionImprovement * 0.5f;
            
            return effects;
        }
        
        private CareEffects ApplyTransplantingEffects(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var transplantData = action.ActionData as TransplantActionData;
            var effects = new CareEffects();
            
            // Transplanting affects root development and growth space
            effects.RootSpaceIncrease = CalculateRootSpaceIncrease(transplantData.Container, quality);
            effects.GrowthPotentialIncrease = effects.RootSpaceIncrease * 0.3f;
            
            // Transplant shock based on quality
            effects.TransplantShock = quality switch
            {
                CareQuality.Perfect => 0f,
                CareQuality.Excellent => 0.05f,
                CareQuality.Good => 0.1f,
                CareQuality.Average => 0.2f,
                _ => 0.4f
            };
            
            return effects;
        }
        
        #endregion
        
        #region Feedback Systems
        
        private void TriggerImmediateFeedback(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            // Visual feedback
            TriggerVisualFeedback(plant, action, quality);
            
            // Audio feedback
            TriggerAudioFeedback(action, quality);
            
            // Haptic feedback (if supported)
            TriggerHapticFeedback(quality);
        }
        
        private void TriggerVisualFeedback(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality)
        {
            var feedbackIntensity = CalculateFeedbackIntensity(quality) * FeedbackIntensityMultiplier;
            
            // Plant response visualization
            _responseVisualization?.ShowPlantResponse(plant, action.TaskType, feedbackIntensity);
            
            // Particle effects
            _particleEffects?.PlayCareEffect(action.TaskType, plant.Position, (float)quality);
            
            // Health indicator update
            // _healthIndicators?.UpdateHealthDisplay(plant, ConvertCareQualityToFloat(quality)); // Disabled - legacy UI
            
            // Simple debug feedback instead of visual health indicators
            float healthValue = ConvertCareQualityToFloat(quality);
            ChimeraLogger.Log("Plant Care", $"Plant health updated: {healthValue:F2} (Care Quality: {quality})");
        }
        
        private void TriggerAudioFeedback(CareAction action, CareQuality quality)
        {
            if (_audioLibrary == null || _careAudioSource == null) return;
            
            // Convert CareAction to CultivationTaskType
            var taskType = ConvertCareActionToTaskType(action);
            var qualityValue = ConvertCareQualityToFloat(quality);
            var audioClip = _audioLibrary.GetCareAudioClip(taskType, qualityValue);
            if (audioClip != null)
            {
                _careAudioSource.clip = audioClip;
                _careAudioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
                _careAudioSource.Play();
            }
        }
        
        private void TriggerHapticFeedback(CareQuality quality)
        {
            // Implement haptic feedback for supported platforms
            // This could integrate with XR controllers or mobile haptics
        }
        
        private void TriggerFailedCareAudio(CultivationTaskType taskType, string failureReason)
        {
            if (_audioLibrary == null || _careAudioSource == null) return;
            
            var failureClip = _audioLibrary.GetFailureAudioClip(taskType);
            if (failureClip != null)
            {
                _careAudioSource.clip = failureClip;
                _careAudioSource.Play();
            }
        }
        
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
        
        #endregion
        
        #region Skill Progression
        
        private void UpdateSkillProgression(CultivationTaskType taskType, CareQuality quality)
        {
            var skillGain = CalculateSkillGain(taskType, quality);
            _skillLevels[taskType] = Mathf.Min(_skillLevels[taskType] + skillGain, _careConfig.MaxSkillLevel);
            
            // Check for skill level milestones
            CheckSkillMilestones(taskType);
        }
        
        private float CalculateSkillGain(CultivationTaskType taskType, CareQuality quality)
        {
            var baseGain = _careConfig.BaseSkillGain;
            var qualityMultiplier = CalculateFeedbackIntensity(quality);
            var currentSkill = _skillLevels[taskType];
            
            // Diminishing returns as skill increases
            var diminishingFactor = 1.0f - (currentSkill / _careConfig.MaxSkillLevel) * 0.8f;
            
            return baseGain * qualityMultiplier * diminishingFactor;
        }
        
        private void CheckSkillMilestones(CultivationTaskType taskType)
        {
            var currentSkill = _skillLevels[taskType];
            var milestones = _careConfig.SkillMilestones;
            
            foreach (var milestone in milestones)
            {
                if (currentSkill >= milestone.RequiredLevel && !milestone.IsUnlocked)
                {
                    milestone.IsUnlocked = true;
                    TriggerSkillMilestoneReached(taskType, milestone);
                }
            }
        }
        
        private void TriggerSkillMilestoneReached(CultivationTaskType taskType, SkillMilestone milestone)
        {
            ChimeraLogger.Log($"Skill milestone reached for {taskType}: {milestone.MilestoneName}", this);
            
            // Trigger milestone celebration effects
            _particleEffects?.PlayMilestoneEffect(transform.position);
            
            // Raise skill progression event
            RaiseSkillProgressionEvent(taskType, milestone);
        }
        
        public float GetCurrentSkillLevel(CultivationTaskType taskType)
        {
            return _skillLevels.ContainsKey(taskType) ? _skillLevels[taskType] : 0f;
        }
        
        #endregion
        
        #region Utility Methods
        
        private CareActionValidation ValidateCareAction(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            var validation = new CareActionValidation { IsValid = true };
            
            // Check if plant can receive this type of care
            if (!CanReceiveCare(plant, action.TaskType))
            {
                validation.IsValid = false;
                validation.FailureReason = $"Plant cannot receive {action.TaskType} care at this time";
                return validation;
            }
            
            // Check if action is appropriate for plant state
            if (!IsActionAppropriate(plant, action))
            {
                validation.IsValid = false;
                validation.FailureReason = "Care action not appropriate for current plant state";
                return validation;
            }
            
            // Check if tool is compatible
            if (!IsToolCompatible(action.Tool, action.TaskType))
            {
                validation.IsValid = false;
                validation.FailureReason = "Tool not compatible with care action";
                return validation;
            }
            
            return validation;
        }
        
        private bool CanReceiveCare(ProjectChimera.Data.Cultivation.InteractivePlant plant, CultivationTaskType taskType)
        {
            // Check plant state and growth stage
            if (plant.CurrentGrowthStage == PlantGrowthStage.Dormant) return false;
            if (plant.CurrentHealth <= 0) return false;
            
            // Some care types have specific requirements
            return taskType switch
            {
                CultivationTaskType.Training => plant.CurrentGrowthStage == PlantGrowthStage.Vegetative,
                CultivationTaskType.Pruning => (int)plant.CurrentGrowthStage >= (int)PlantGrowthStage.Seedling,
                CultivationTaskType.Harvesting => plant.CurrentGrowthStage == PlantGrowthStage.Mature,
                _ => true
            };
        }
        
        private bool IsActionAppropriate(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action)
        {
            var plantNeeds = GetPlantNeeds(plant);
            var actionRelevance = CalculateActionRelevance(action.TaskType, plantNeeds);
            
            return actionRelevance > _careConfig.MinActionRelevanceThreshold;
        }
        
        private bool IsToolCompatible(CareToolBase tool, CultivationTaskType taskType)
        {
            if (tool == null) return true; // Some actions don't require tools
            
            return tool.CompatibleTaskTypes.Contains(taskType);
        }
        
        private CultivationTaskType ConvertCareActionToTaskType(CareAction action)
        {
            // The CareAction class already has TaskType property
            return action.TaskType;
        }
        
        private float ConvertCareQualityToFloat(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Poor => 0.2f,
                CareQuality.Adequate => 0.4f,
                CareQuality.Good => 0.6f,
                CareQuality.Excellent => 0.8f,
                CareQuality.Perfect => 1.0f,
                _ => 0.5f
            };
        }
        
        private PlantNeeds GetPlantNeeds(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            // Calculate current plant needs based on state
            return new PlantNeeds
            {
                WaterNeed = CalculateWaterNeed(plant),
                NutrientNeed = CalculateNutrientNeed(plant),
                LightNeed = CalculateLightNeed(plant),
                CareNeed = CalculateGeneralCareNeed(plant)
            };
        }
        
        private float CalculateActionRelevance(CultivationTaskType taskType, PlantNeeds needs)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => needs.WaterNeed,
                CultivationTaskType.Fertilizing => needs.NutrientNeed,
                CultivationTaskType.EnvironmentalControl => needs.LightNeed,
                CultivationTaskType.Pruning => needs.CareNeed * 0.7f,
                CultivationTaskType.Training => needs.CareNeed * 0.8f,
                _ => needs.CareNeed
            };
        }
        
        private float GetOptimalCareTime(ProjectChimera.Data.Cultivation.InteractivePlant plant, CultivationTaskType taskType)
        {
            // Calculate optimal timing based on plant circadian rhythm and care type
            var plantAgeSeconds = (float)(System.DateTime.Now - plant.PlantedTime).TotalSeconds;
            var plantAge = plantAgeSeconds / 3600f; // Convert to hours
            var circadianCycle = (plantAge % 24f) / 24f; // 24-hour cycle
            
            return taskType switch
            {
                CultivationTaskType.Watering => 6f + (circadianCycle * 24f), // Morning watering
                CultivationTaskType.Fertilizing => 8f + (circadianCycle * 24f), // Morning feeding
                CultivationTaskType.Pruning => 10f + (circadianCycle * 24f), // Late morning pruning
                _ => Time.time
            };
        }
        
        private float CalculateWaterNeed(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            return Mathf.Clamp01(1.0f - plant.CurrentHydration);
        }
        
        private float CalculateNutrientNeed(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            return Mathf.Clamp01(1.0f - plant.CurrentNutrition);
        }
        
        private float CalculateLightNeed(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            return Mathf.Clamp01(1.0f - plant.CurrentLightSatisfaction);
        }
        
        private float CalculateGeneralCareNeed(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            var healthRatio = plant.CurrentHealth / plant.MaxHealth;
            return Mathf.Clamp01(1.0f - healthRatio);
        }
        
        private PlantCareResult DetermineCareResult(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Perfect => PlantCareResult.Perfect,
                CareQuality.Excellent or CareQuality.Good => PlantCareResult.Successful,
                CareQuality.Average => PlantCareResult.Adequate,
                CareQuality.Poor => PlantCareResult.Suboptimal,
                _ => PlantCareResult.Failed
            };
        }
        
        #endregion
        
        #region Event Management
        
        private void RaisePlantCarePerformedEvent(ProjectChimera.Data.Cultivation.InteractivePlant plant, CareAction action, CareQuality quality, CareEffects effects)
        {
            var eventData = new PlantCareEventData
            {
                PlantInstance = plant,
                CareType = action.TaskType.ToString(), // Convert enum to string
                CareQuality = quality.ToString(), // Convert enum to string
                Timestamp = Time.time
            };
            
            _onPlantCarePerformed?.RaiseEvent(eventData);
        }
        
        private void RaiseSkillProgressionEvent(CultivationTaskType taskType, SkillMilestone milestone)
        {
            var eventData = new SkillProgressionEventData
            {
                TaskType = taskType,
                Milestone = milestone,
                CurrentSkillLevel = GetCurrentSkillLevel(taskType),
                Timestamp = Time.time
            };
            
            _onSkillProgressionTriggered?.RaiseEvent(eventData);
        }
        
        #endregion
        
        #region Tracking and Analytics
        
        private void TrackCareAction(CareAction action, CareQuality quality)
        {
            _totalCareActionsPerformed++;
            _careActionCounts[action.TaskType]++;
            
            // Update average care quality
            var qualityValue = CalculateFeedbackIntensity(quality);
            _averageCareQuality = (_averageCareQuality * (_totalCareActionsPerformed - 1) + qualityValue) / _totalCareActionsPerformed;
        }
        
        public CareSystemMetrics GetCareMetrics()
        {
            return new CareSystemMetrics
            {
                TotalCareActions = _totalCareActionsPerformed,
                AverageCareQuality = _averageCareQuality,
                CareActionCounts = new Dictionary<CultivationTaskType, int>(_careActionCounts),
                SkillLevels = new Dictionary<CultivationTaskType, float>(_skillLevels)
            };
        }
        
        #endregion
        
        #region System Updates
        
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update active care actions
            UpdateActiveCareActions(deltaTime);
            
            // Update feedback systems
            UpdateFeedbackSystems(deltaTime);
            
            // Update plant care states
            UpdatePlantCareStates(deltaTime);
        }
        
        private void UpdateActiveCareActions(float deltaTime)
        {
            for (int i = _activeCareActions.Count - 1; i >= 0; i--)
            {
                var action = _activeCareActions[i];
                action.Duration += deltaTime;
                
                // Remove completed actions
                if (action.Duration >= action.MaxDuration)
                {
                    _activeCareActions.RemoveAt(i);
                }
            }
        }
        
        private void UpdateFeedbackSystems(float deltaTime)
        {
            // Feedback systems don't require delta time updates in this implementation
            // Visual feedback is handled through direct method calls when care actions occur
        }
        
        private void UpdatePlantCareStates(float deltaTime)
        {
            var plantsToRemove = new List<int>();
            
            foreach (var kvp in _plantCareStates)
            {
                var plantId = kvp.Key;
                var careState = kvp.Value;
                
                // Update care state timing
                careState.TimeSinceLastCare += deltaTime;
                
                // Remove states for plants that no longer exist
                if (careState.Plant == null)
                {
                    plantsToRemove.Add(plantId);
                }
            }
            
            // Clean up removed plants
            foreach (var plantId in plantsToRemove)
            {
                _plantCareStates.Remove(plantId);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Inspect a plant to get detailed observation data
        /// </summary>
        public PlantObservation InspectPlant(ProjectChimera.Data.Cultivation.InteractivePlant plant, InspectionTool tool = null)
        {
            if (plant == null) return null;
            
            var observation = new PlantObservation
            {
                PlantId = plant.PlantInstanceID,
                VisualHealth = AssessVisualHealth(plant),
                GrowthStage = ConvertToLocalGrowthStage(plant.CurrentGrowthStage),
                EstimatedNeeds = GetPlantNeeds(plant),
                Timestamp = Time.time,
                InspectionQuality = tool?.Quality ?? 1.0f
            };
            
            // Initialize DetailedData dictionary
            observation.DetailedData = new Dictionary<string, object>();
            
            // Tool-specific observations
            if (tool != null)
            {
                
                // Check if tool has detailed inspection capability before using it
                if (tool != null && tool.GetType().GetMethod("GetDetailedInspectionData") != null)
                {
                    try
                    {
                        var detailedData = tool.GetDetailedInspectionData(plant);
                        if (detailedData is Dictionary<string, object> dataDict)
                        {
                            observation.DetailedData = dataDict;
                        }
                        else
                        {
                            observation.DetailedData["InspectionResult"] = detailedData?.ToString() ?? "Basic inspection data";
                        }
                    }
                    catch
                    {
                        observation.DetailedData["InspectionResult"] = "Tool inspection failed";
                    }
                }
                else
                {
                    observation.DetailedData["InspectionResult"] = "Basic visual inspection";
                }
            }
            
            return observation;
        }
        
        /// <summary>
        /// Assess current plant health status
        /// </summary>
        public PlantHealthStatus AssessPlantHealth(ProjectChimera.Data.Cultivation.InteractivePlant plant, DiagnosticTool tool = null)
        {
            if (plant == null) return null;
            
            var healthStatus = new PlantHealthStatus
            {
                OverallHealth = plant.CurrentHealth / plant.MaxHealth,
                Hydration = plant.CurrentHydration,
                Nutrition = plant.CurrentNutrition,
                StressLevel = plant.CurrentStressLevel,
                GrowthRate = plant.CurrentGrowthRate,
                Timestamp = Time.time,
                DiagnosticData = new Dictionary<string, object>(),
                Recommendations = new List<CareRecommendation>()
            };
            
            // Diagnostic tool provides more detailed analysis
            if (tool != null)
            {
                // Check if tool has analysis methods before using them
                try
                {
                    if (tool.GetType().GetMethod("AnalyzePlantHealth") != null)
                    {
                        var analysisResult = tool.AnalyzePlantHealth(plant);
                        if (analysisResult is Dictionary<string, object> analysisDict)
                        {
                            healthStatus.DiagnosticData = analysisDict;
                        }
                        else
                        {
                            healthStatus.DiagnosticData["Analysis"] = analysisResult?.ToString() ?? "No detailed analysis available";
                        }
                    }
                    
                    if (tool.GetType().GetMethod("GenerateRecommendations") != null)
                    {
                        var recommendations = tool.GenerateRecommendations(plant);
                        if (recommendations is List<CareRecommendation> recList)
                        {
                            healthStatus.Recommendations = recList;
                        }
                    }
                }
                catch
                {
                    healthStatus.DiagnosticData["Error"] = "Diagnostic tool analysis failed";
                }
            }
            
            return healthStatus;
        }
        
        /// <summary>
        /// Adjust task complexity based on time acceleration
        /// </summary>
        public void AdjustTaskComplexity(float complexityMultiplier)
        {
            // Adjust care difficulty based on time scale
            CareActionBaseEfficiency = _careConfig.BaseActionEfficiency / complexityMultiplier;
            
            ChimeraLogger.Log($"Care task complexity adjusted by {complexityMultiplier}x", this);
        }
        
        /// <summary>
        /// Apply automation benefits to the plant care system
        /// </summary>
        public void ApplyAutomationBenefits(AutomationBenefits benefits)
        {
            // Apply efficiency gains to care actions
            CareActionBaseEfficiency *= benefits.EfficiencyGains;
            
            // Apply quality optimization to skill-based precision
            SkillBasedPrecisionMultiplier *= benefits.QualityOptimization;
            
            // Log the benefits applied
            ChimeraLogger.Log($"Applied automation benefits: Efficiency {benefits.EfficiencyGains:F2}x, Quality {benefits.QualityOptimization:F2}x", this);
        }
        
        /// <summary>
        /// Convert PlantGrowthStage to local GrowthStage enum
        /// </summary>
        private GrowthStage ConvertToLocalGrowthStage(PlantGrowthStage plantGrowthStage)
        {
            return plantGrowthStage switch
            {
                PlantGrowthStage.Seed => GrowthStage.Seed,
                PlantGrowthStage.Germination => GrowthStage.Germination,
                PlantGrowthStage.Sprout => GrowthStage.Seedling,
                PlantGrowthStage.Seedling => GrowthStage.Seedling,
                PlantGrowthStage.Vegetative => GrowthStage.Vegetative,
                PlantGrowthStage.PreFlowering => GrowthStage.PreFlower,
                PlantGrowthStage.PreFlower => GrowthStage.PreFlower,
                PlantGrowthStage.Flowering => GrowthStage.Flowering,
                PlantGrowthStage.Ripening => GrowthStage.Mature,
                PlantGrowthStage.Mature => GrowthStage.Mature,
                PlantGrowthStage.Harvest => GrowthStage.Mature,
                PlantGrowthStage.Harvestable => GrowthStage.Mature,
                PlantGrowthStage.Harvested => GrowthStage.Dormant,
                PlantGrowthStage.Drying => GrowthStage.Dormant,
                PlantGrowthStage.Curing => GrowthStage.Dormant,
                PlantGrowthStage.Dormant => GrowthStage.Dormant,
                _ => GrowthStage.Seedling
            };
        }
        
        #endregion
        
        #region Calculation Helpers
        
        private float CalculateHydrationEffect(ProjectChimera.Data.Cultivation.InteractivePlant plant, float waterAmount, CareQuality quality)
        {
            var optimalAmount = plant.OptimalWaterAmount;
            var efficiency = CalculateFeedbackIntensity(quality);
            var amountRatio = Mathf.Clamp01(waterAmount / optimalAmount);
            
            // Overwatering reduces efficiency
            if (amountRatio > 1.0f)
            {
                efficiency *= Mathf.Lerp(1.0f, 0.3f, (amountRatio - 1.0f));
            }
            
            return waterAmount * efficiency;
        }
        
        private float CalculatePruningGrowthEffect(PruningType pruningType, CareQuality quality)
        {
            var baseEffect = pruningType switch
            {
                PruningType.Topping => 0.8f,
                PruningType.LST => 0.6f,
                PruningType.Defoliation => 0.4f,
                PruningType.Lollipopping => 0.7f,
                _ => 0.5f
            };
            
            return baseEffect * CalculateFeedbackIntensity(quality);
        }
        
        private float CalculateTrainingStructureEffect(TrainingMethod method, CareQuality quality)
        {
            var baseEffect = method switch
            {
                TrainingMethod.LST => 0.7f,
                TrainingMethod.SCROG => 0.9f,
                TrainingMethod.SOG => 0.6f,
                TrainingMethod.Supercropping => 0.8f,
                _ => 0.5f
            };
            
            return baseEffect * CalculateFeedbackIntensity(quality);
        }
        
        private float CalculateRootSpaceIncrease(Container container, CareQuality quality)
        {
            var spaceIncrease = container.Volume / 1000f; // Convert to normalized value
            return spaceIncrease * CalculateFeedbackIntensity(quality);
        }
        
        private VisualHealthState AssessVisualHealth(ProjectChimera.Data.Cultivation.InteractivePlant plant)
        {
            var healthRatio = plant.CurrentHealth / plant.MaxHealth;
            
            return healthRatio switch
            {
                >= 0.9f => VisualHealthState.Thriving,
                >= 0.7f => VisualHealthState.Healthy,
                >= 0.5f => VisualHealthState.Stressed,
                >= 0.3f => VisualHealthState.Struggling,
                _ => VisualHealthState.Critical
            };
        }
        
        /// <summary>
        /// Unlock new care mechanics based on skill progression
        /// </summary>
        public void UnlockNewMechanics(SkillNodeType nodeType)
        {
            if (!_isInitialized) return;
            
            ChimeraLogger.Log($"Unlocking new care mechanics for skill node: {nodeType}", this);
            
            // This would unlock new care tools, techniques, or interactions
            // based on the skill node that was unlocked
        }
        
        #endregion
    }
    
    #region Data Structures and Enums
    
    [System.Serializable]
    public class CareAction
    {
        public CultivationTaskType TaskType;
        public CareToolBase Tool;
        public ICareActionData ActionData;
        public float Timestamp;
        public float Duration;
        public float MaxDuration = 5.0f;
        public float PlayerSkillLevel;
    }
    
    [System.Serializable]
    public class PlantCareState
    {
        public ProjectChimera.Data.Cultivation.InteractivePlant Plant;
        public float TimeSinceLastCare;
        public Dictionary<CultivationTaskType, float> LastCareTimestamps = new Dictionary<CultivationTaskType, float>();
        public CareQuality LastCareQuality;
    }
    
    [System.Serializable]
    public class CareEffects
    {
        public float HydrationChange;
        public float NutritionImprovement;
        public float HealthImprovement;
        public float StressReduction;
        public float StressIncrease;
        public float GrowthRateBoost;
        public float GrowthRedirection;
        public float StructuralImprovement;
        public float YieldPotentialIncrease;
        public float GrowthPotentialIncrease;
        public float RootSpaceIncrease;
        public float TransplantShock;
    }
    
    [System.Serializable]
    public class PlantNeeds
    {
        public float WaterNeed;
        public float NutrientNeed;
        public float LightNeed;
        public float CareNeed;
    }
    
    [System.Serializable]
    public class CareActionValidation
    {
        public bool IsValid;
        public string FailureReason;
    }
    
    [System.Serializable]
    public class CareSystemMetrics
    {
        public int TotalCareActions;
        public float AverageCareQuality;
        public Dictionary<CultivationTaskType, int> CareActionCounts;
        public Dictionary<CultivationTaskType, float> SkillLevels;
    }
    
    [System.Serializable]
    public class PlantObservation
    {
        public int PlantId;
        public VisualHealthState VisualHealth;
        public GrowthStage GrowthStage;
        public PlantNeeds EstimatedNeeds;
        public float Timestamp;
        public float InspectionQuality;
        public Dictionary<string, object> DetailedData;
    }
    
    [System.Serializable]
    public class PlantHealthStatus
    {
        public float OverallHealth;
        public float Hydration;
        public float Nutrition;
        public float StressLevel;
        public float GrowthRate;
        public float Timestamp;
        public Dictionary<string, object> DiagnosticData;
        public List<CareRecommendation> Recommendations;
    }
    
    // Care Action Data Interfaces and Classes
    public interface ICareActionData { }
    
    [System.Serializable]
    public class WateringActionData : ICareActionData
    {
        public float Amount;
        public WateringTool Tool;
    }
    
    [System.Serializable]
    public class PruningActionData : ICareActionData
    {
        public PruningType PruningType;
        public PruningTool Tool;
    }
    
    [System.Serializable]
    public class TrainingActionData : ICareActionData
    {
        public TrainingMethod Method;
        public TrainingTool Tool;
    }
    
    [System.Serializable]
    public class TransplantActionData : ICareActionData
    {
        public Container Container;
        public GrowingMedium Medium;
    }
    
    // Event Data Classes
    // PlantCareEventData is defined in ProjectChimera.Events.CultivationGamingEventData to avoid CS0101 duplicate definition errors
    // SkillProgressionEventData is defined in ProjectChimera.Events.CultivationGamingEventData to avoid CS0101 duplicate definition errors
    
    // Enums
    public enum VisualHealthState
    {
        Thriving,
        Healthy,
        Stressed,
        Struggling,
        Critical
    }
    
    public enum PruningType
    {
        Topping,
        LST,
        Defoliation,
        Lollipopping,
        Mainlining,
        Supercropping
    }
    
    public enum TrainingMethod
    {
        LST,        // Low Stress Training
        SCROG,      // Screen of Green
        SOG,        // Sea of Green
        Supercropping,
        Manifolding,
        Mainlining
    }
    
    public enum GrowthStage
    {
        Seed,
        Germination,
        Seedling,
        Vegetative,
        PreFlower,
        Flowering,
        Mature,
        Dormant
    }
    
    #endregion
}