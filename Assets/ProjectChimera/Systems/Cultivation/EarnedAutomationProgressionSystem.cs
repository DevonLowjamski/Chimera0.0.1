using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Events;

// Type aliases to resolve ambiguities
using DataAutomationDesireLevel = ProjectChimera.Data.Cultivation.AutomationDesireLevel;
using NarrativeTaskType = ProjectChimera.Data.Narrative.TaskType;
using DataCultivationTaskType = ProjectChimera.Data.Cultivation.CultivationTaskType;
using DataTaskType = ProjectChimera.Data.Cultivation.TaskType;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Earned Automation Progression System - Implements "burden of consistency" gaming mechanics
    /// Players experience manual task burden before earning automation relief
    /// Core component of Enhanced Cultivation Gaming System v2.0
    /// </summary>
    public class EarnedAutomationProgressionSystem : MonoBehaviour
    {
        [Header("Automation Configuration")]
        [SerializeField] private EarnedAutomationConfigSO _automationConfig;
        [SerializeField] private AutomationUnlockLibrarySO _unlockLibrary;
        
        [Header("Burden Calculation Settings")]
        [Range(0.1f, 3.0f)] public float BurdenAccumulationRate = 1.0f;
        [Range(0.1f, 2.0f)] public float ScalePressureMultiplier = 1.2f;
        [Range(0.1f, 2.0f)] public float TimeAccelerationBurdenMultiplier = 1.5f;
        [Range(0.1f, 1.0f)] public float QualityRiskTolerance = 0.7f;
        
        [Header("Automation Benefits")]
        [Range(1.1f, 3.0f)] public float ConsistencyImprovementMultiplier = 1.8f;
        [Range(1.1f, 2.5f)] public float EfficiencyGainMultiplier = 1.5f;
        [Range(1.1f, 3.0f)] public float ScalabilityBonusMultiplier = 2.0f;
        [Range(0.05f, 0.3f)] public float QualityOptimizationBonus = 0.15f;
        
        // System State
        private bool _isInitialized = false;
        private Dictionary<DataCultivationTaskType, AutomationProgress> _automationProgress = new Dictionary<DataCultivationTaskType, AutomationProgress>();
        private Dictionary<DataCultivationTaskType, TaskBurdenState> _taskBurdenStates = new Dictionary<DataCultivationTaskType, TaskBurdenState>();
        private Dictionary<AutomationSystemType, AutomationSystemState> _automationSystems = new Dictionary<AutomationSystemType, AutomationSystemState>();
        
        // Dependencies
        private ManualTaskBurdenCalculator _burdenCalculator;
        private AutomationUnlockManager _unlockManager;
        private InteractivePlantCareSystem _plantCareSystem;
        
        // Events
        private GameEventChannelSO _onAutomationDesireIncreased;
        private GameEventChannelSO _onAutomationUnlocked;
        private GameEventChannelSO _onBurdenThresholdReached;
        private GameEventChannelSO _onAutomationBenefitRealized;
        
        // Performance Metrics
        private float _totalBurdenReduction = 0f;
        private int _automationSystemsUnlocked = 0;
        private float _automationEfficiencyGained = 0f;
        
        #region Initialization
        
        public void Initialize(EarnedAutomationConfigSO config)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("EarnedAutomationProgressionSystem already initialized", this);
                return;
            }
            
            _automationConfig = config ?? _automationConfig;
            
            if (_automationConfig == null)
            {
                ChimeraLogger.LogError("EarnedAutomationConfigSO is required for initialization", this);
                return;
            }
            
            InitializeAutomationProgress();
            InitializeBurdenStates();
            InitializeAutomationSystems();
            SetupDependencies();
            SetupEventChannels();
            
            _isInitialized = true;
            ChimeraLogger.Log("EarnedAutomationProgressionSystem initialized successfully", this);
        }
        
        private void InitializeAutomationProgress()
        {
            // Initialize automation progress for all task types
            foreach (DataCultivationTaskType taskType in System.Enum.GetValues(typeof(DataCultivationTaskType)))
            {
                _automationProgress[taskType] = new AutomationProgress
                {
                    TaskType = taskType,
                    CurrentBurden = 0f,
                    BurdenThreshold = _automationConfig.GetBurdenThreshold(taskType),
                    AutomationDesireLevel = DataAutomationDesireLevel.None,
                    IsAutomationAvailable = false,
                    IsAutomationUnlocked = false,
                    TimeSinceLastBurdenIncrease = 0f
                };
            }
        }
        
        private void InitializeBurdenStates()
        {
            // Initialize burden tracking for all task types
            foreach (DataCultivationTaskType taskType in System.Enum.GetValues(typeof(DataCultivationTaskType)))
            {
                _taskBurdenStates[taskType] = new TaskBurdenState
                {
                    TaskType = taskType,
                    CognitiveLoad = 0f,
                    TimeInvestment = 0f,
                    ConsistencyChallenge = 0f,
                    ScalePressure = 0f,
                    QualityRisk = 0f,
                    TotalTasksPerformed = 0,
                    AverageTaskDuration = 0f,
                    ConsistencyRating = 1.0f
                };
            }
        }
        
        private void InitializeAutomationSystems()
        {
            // Initialize all automation system types
            foreach (AutomationSystemType systemType in System.Enum.GetValues(typeof(AutomationSystemType)))
            {
                _automationSystems[systemType] = new AutomationSystemState
                {
                    SystemType = systemType,
                    IsUnlocked = false,
                    IsActive = false,
                    EfficiencyLevel = 0f,
                    MaintenanceRequired = false,
                    LastMaintenanceTime = 0f,
                    OperationalHours = 0f
                };
            }
        }
        
        private void SetupDependencies()
        {
            // Get or create burden calculator
            _burdenCalculator = GetComponentInChildren<ManualTaskBurdenCalculator>();
            if (_burdenCalculator == null)
            {
                var calculatorGO = new GameObject("ManualTaskBurdenCalculator");
                calculatorGO.transform.SetParent(transform);
                _burdenCalculator = calculatorGO.AddComponent<ManualTaskBurdenCalculator>();
            }
            
            // Get or create unlock manager
            _unlockManager = GetComponentInChildren<AutomationUnlockManager>();
            if (_unlockManager == null)
            {
                var unlockGO = new GameObject("AutomationUnlockManager");
                unlockGO.transform.SetParent(transform);
                _unlockManager = unlockGO.AddComponent<AutomationUnlockManager>();
            }
            
            // Find plant care system
            _plantCareSystem = FindObjectOfType<InteractivePlantCareSystem>();
        }
        
        private void SetupEventChannels()
        {
            // Event channels will be connected through the main cultivation gaming manager
        }
        
        #endregion
        
        #region Burden Calculation and Tracking
        
        /// <summary>
        /// Calculate automation desire for a specific task based on current burden
        /// </summary>
        public AutomationDesire CalculateAutomationDesire(ManualCareTask task, PlayerProficiency proficiency)
        {
            if (!_isInitialized || task == null || proficiency == null)
                return new AutomationDesire { DesireLevel = DataAutomationDesireLevel.None };
            
            var taskType = task.TaskType;
            var burdenState = _taskBurdenStates[taskType];
            
            var desire = new AutomationDesire
            {
                TaskType = taskType,
                CognitiveLoad = CalculateCognitiveLoad(task, proficiency),
                TimeInvestment = CalculateTimeInvestment(task),
                ConsistencyChallenge = CalculateConsistencyChallenge(task, proficiency),
                ScalePressure = CalculateScalePressure(task),
                QualityRisk = CalculateQualityRisk(task, proficiency)
            };
            
            // Calculate overall desire level
            desire.OverallBurden = CalculateOverallBurden(desire);
            desire.DesireLevel = DetermineBurdenLevel(desire.OverallBurden);
            
            // Update burden state
            UpdateBurdenState(taskType, desire);
            
            return desire;
        }
        
        private float CalculateCognitiveLoad(ManualCareTask task, PlayerProficiency proficiency)
        {
            var baseComplexity = _automationConfig.GetTaskComplexity(task.TaskType);
            var skillModifier = 1.0f - (proficiency.SkillLevel / _automationConfig.MaxSkillLevel);
            var frequencyModifier = Mathf.Log(1 + task.Frequency) / Mathf.Log(2); // Logarithmic scaling
            
            return baseComplexity * skillModifier * frequencyModifier * BurdenAccumulationRate;
        }
        
        private float CalculateTimeInvestment(ManualCareTask task)
        {
            var baseDuration = task.AverageDuration;
            var plantCountModifier = Mathf.Sqrt(task.PlantCount); // Square root scaling
            var urgencyModifier = task.IsUrgent ? 1.5f : 1.0f;
            
            return baseDuration * plantCountModifier * urgencyModifier;
        }
        
        private float CalculateConsistencyChallenge(ManualCareTask task, PlayerProficiency proficiency)
        {
            var precisionRequired = _automationConfig.GetPrecisionRequirement(task.TaskType);
            var skillGap = Mathf.Max(0, precisionRequired - proficiency.SkillLevel);
            var consistencyHistory = proficiency.ConsistencyRating;
            
            return skillGap * (2.0f - consistencyHistory); // Higher burden for inconsistent players
        }
        
        private float CalculateScalePressure(ManualCareTask task)
        {
            var plantCount = task.PlantCount;
            var facilitySize = task.FacilitySize;
            var scaleFactor = (plantCount * facilitySize) / _automationConfig.BaseScaleReference;
            
            return Mathf.Pow(scaleFactor, ScalePressureMultiplier);
        }
        
        private float CalculateQualityRisk(ManualCareTask task, PlayerProficiency proficiency)
        {
            var errorTolerance = _automationConfig.GetErrorTolerance(task.TaskType);
            var skillReliability = proficiency.ConsistencyRating;
            var qualityStandard = task.RequiredQualityLevel;
            
            var riskFactor = (qualityStandard - skillReliability) / errorTolerance;
            return Mathf.Max(0, riskFactor);
        }
        
        private float CalculateOverallBurden(AutomationDesire desire)
        {
            var weights = _automationConfig.BurdenWeights;
            
            return (desire.CognitiveLoad * weights.CognitiveLoadWeight) +
                   (desire.TimeInvestment * weights.TimeInvestmentWeight) +
                   (desire.ConsistencyChallenge * weights.ConsistencyChallengeWeight) +
                   (desire.ScalePressure * weights.ScalePressureWeight) +
                   (desire.QualityRisk * weights.QualityRiskWeight);
        }
        
        private DataAutomationDesireLevel DetermineBurdenLevel(float overallBurden)
        {
            var thresholds = _automationConfig.BurdenThresholds;
            
            if (overallBurden >= thresholds.CriticalThreshold)
                return DataAutomationDesireLevel.Critical;
            else if (overallBurden >= thresholds.HighThreshold)
                return DataAutomationDesireLevel.High;
            else if (overallBurden >= thresholds.MediumThreshold)
                return DataAutomationDesireLevel.Medium;
            else if (overallBurden >= thresholds.LowThreshold)
                return DataAutomationDesireLevel.Low;
            else
                return DataAutomationDesireLevel.None;
        }
        
        private void UpdateBurdenState(DataCultivationTaskType taskType, AutomationDesire desire)
        {
            var burdenState = _taskBurdenStates[taskType];
            var progress = _automationProgress[taskType];
            
            // Update burden state values
            burdenState.CognitiveLoad = desire.CognitiveLoad;
            burdenState.TimeInvestment = desire.TimeInvestment;
            burdenState.ConsistencyChallenge = desire.ConsistencyChallenge;
            burdenState.ScalePressure = desire.ScalePressure;
            burdenState.QualityRisk = desire.QualityRisk;
            
            // Update automation progress
            progress.CurrentBurden = desire.OverallBurden;
            progress.AutomationDesireLevel = (DataAutomationDesireLevel)desire.DesireLevel;
            progress.TimeSinceLastBurdenIncrease = 0f;
            
            // Check if automation should become available
            CheckAutomationAvailability(taskType, progress);
            
            // Raise events if thresholds are reached
            if (desire.DesireLevel >= AutomationDesireLevel.High && !progress.HasRaisedHighBurdenEvent)
            {
                RaiseBurdenThresholdReached(taskType, desire.DesireLevel);
                progress.HasRaisedHighBurdenEvent = true;
            }
        }
        
        #endregion
        
        #region Automation Unlock System
        
        /// <summary>
        /// Unlock automation system for specified task type
        /// </summary>
        public AutomationUnlock UnlockAutomationSystem(DataCultivationTaskType taskType, PlayerProgress progress)
        {
            if (!_isInitialized)
                return new AutomationUnlock { Success = false, Reason = "System not initialized" };
            
            var automationProgress = _automationProgress[taskType];
            
            // Check if automation is available for unlock
            if (!automationProgress.IsAutomationAvailable)
            {
                return new AutomationUnlock 
                { 
                    Success = false, 
                    Reason = "Automation not yet available - more manual experience required" 
                };
            }
            
            // Check if already unlocked
            if (automationProgress.IsAutomationUnlocked)
            {
                return new AutomationUnlock 
                { 
                    Success = false, 
                    Reason = "Automation already unlocked for this task type" 
                };
            }
            
            // Determine which automation systems to unlock
            var unlock = new AutomationUnlock
            {
                Success = true,
                TaskType = taskType,
                IrrigationAutomation = UnlockWateringSystem(automationProgress, progress),
                EnvironmentalAutomation = UnlockClimateControl(automationProgress, progress),
                NutrientAutomation = UnlockFertigation(automationProgress, progress),
                MonitoringAutomation = UnlockSensorSystems(automationProgress, progress),
                LightingAutomation = UnlockLightScheduling(automationProgress, progress)
            };
            
            // Apply automation unlock
            ApplyAutomationUnlock(taskType, unlock);
            
            return unlock;
        }
        
        private AutomationSystemUnlock UnlockWateringSystem(AutomationProgress progress, PlayerProgress playerProgress)
        {
            if (progress.TaskType != DataCultivationTaskType.Watering) 
                return new AutomationSystemUnlock { IsUnlocked = false };
            
            var irrigationMastery = playerProgress.IrrigationMastery;
            var requiredMastery = _automationConfig.GetRequiredMastery(AutomationSystemType.IrrigationSystem);
            
            return new AutomationSystemUnlock
            {
                IsUnlocked = irrigationMastery >= requiredMastery,
                SystemType = AutomationSystemType.IrrigationSystem,
                EfficiencyLevel = CalculateSystemEfficiency(irrigationMastery, requiredMastery),
                MaintenanceRequirement = CalculateMaintenanceRequirement(AutomationSystemType.IrrigationSystem)
            };
        }
        
        private AutomationSystemUnlock UnlockClimateControl(AutomationProgress progress, PlayerProgress playerProgress)
        {
            if (progress.TaskType != DataCultivationTaskType.EnvironmentalControl) 
                return new AutomationSystemUnlock { IsUnlocked = false };
            
            var environmentalSkill = playerProgress.EnvironmentalSkill;
            var requiredSkill = _automationConfig.GetRequiredMastery(AutomationSystemType.ClimateControl);
            
            return new AutomationSystemUnlock
            {
                IsUnlocked = environmentalSkill >= requiredSkill,
                SystemType = AutomationSystemType.ClimateControl,
                EfficiencyLevel = CalculateSystemEfficiency(environmentalSkill, requiredSkill),
                MaintenanceRequirement = CalculateMaintenanceRequirement(AutomationSystemType.ClimateControl)
            };
        }
        
        private AutomationSystemUnlock UnlockFertigation(AutomationProgress progress, PlayerProgress playerProgress)
        {
            if (progress.TaskType != DataCultivationTaskType.Fertilizing) 
                return new AutomationSystemUnlock { IsUnlocked = false };
            
            var nutritionExpertise = playerProgress.NutritionExpertise;
            var requiredExpertise = _automationConfig.GetRequiredMastery(AutomationSystemType.NutrientDelivery);
            
            return new AutomationSystemUnlock
            {
                IsUnlocked = nutritionExpertise >= requiredExpertise,
                SystemType = AutomationSystemType.NutrientDelivery,
                EfficiencyLevel = CalculateSystemEfficiency(nutritionExpertise, requiredExpertise),
                MaintenanceRequirement = CalculateMaintenanceRequirement(AutomationSystemType.NutrientDelivery)
            };
        }
        
        private AutomationSystemUnlock UnlockSensorSystems(AutomationProgress progress, PlayerProgress playerProgress)
        {
            if (progress.TaskType != DataCultivationTaskType.DataCollection) 
                return new AutomationSystemUnlock { IsUnlocked = false };
            
            var observationSkill = playerProgress.ObservationSkill;
            var requiredSkill = _automationConfig.GetRequiredMastery(AutomationSystemType.SensorNetwork);
            
            return new AutomationSystemUnlock
            {
                IsUnlocked = observationSkill >= requiredSkill,
                SystemType = AutomationSystemType.SensorNetwork,
                EfficiencyLevel = CalculateSystemEfficiency(observationSkill, requiredSkill),
                MaintenanceRequirement = CalculateMaintenanceRequirement(AutomationSystemType.SensorNetwork)
            };
        }
        
        private AutomationSystemUnlock UnlockLightScheduling(AutomationProgress progress, PlayerProgress playerProgress)
        {
            if (progress.TaskType != DataCultivationTaskType.EnvironmentalControl) 
                return new AutomationSystemUnlock { IsUnlocked = false };
            
            var photoperiodKnowledge = playerProgress.PhotoperiodKnowledge;
            var requiredKnowledge = _automationConfig.GetRequiredMastery(AutomationSystemType.LightingSchedule);
            
            return new AutomationSystemUnlock
            {
                IsUnlocked = photoperiodKnowledge >= requiredKnowledge,
                SystemType = AutomationSystemType.LightingSchedule,
                EfficiencyLevel = CalculateSystemEfficiency(photoperiodKnowledge, requiredKnowledge),
                MaintenanceRequirement = CalculateMaintenanceRequirement(AutomationSystemType.LightingSchedule)
            };
        }
        
        private void ApplyAutomationUnlock(DataCultivationTaskType taskType, AutomationUnlock unlock)
        {
            var progress = _automationProgress[taskType];
            progress.IsAutomationUnlocked = true;
            progress.UnlockTimestamp = Time.time;
            
            // Update automation system states
            UpdateAutomationSystemState(unlock.IrrigationAutomation);
            UpdateAutomationSystemState(unlock.EnvironmentalAutomation);
            UpdateAutomationSystemState(unlock.NutrientAutomation);
            UpdateAutomationSystemState(unlock.MonitoringAutomation);
            UpdateAutomationSystemState(unlock.LightingAutomation);
            
            // Track metrics
            _automationSystemsUnlocked++;
            
            // Raise automation unlocked event
            RaiseAutomationUnlockedEvent(unlock);
            
            ChimeraLogger.Log($"Automation unlocked for {taskType} with {GetUnlockedSystemsCount(unlock)} systems", this);
        }
        
        private void UpdateAutomationSystemState(AutomationSystemUnlock systemUnlock)
        {
            if (!systemUnlock.IsUnlocked) return;
            
            var systemState = _automationSystems[systemUnlock.SystemType];
            systemState.IsUnlocked = true;
            systemState.IsActive = true;
            systemState.EfficiencyLevel = systemUnlock.EfficiencyLevel;
            systemState.MaintenanceRequired = false;
            systemState.LastMaintenanceTime = Time.time;
        }
        
        #endregion
        
        #region Automation Benefits Calculation
        
        /// <summary>
        /// Calculate automation benefits for current automation level and facility scale
        /// </summary>
        public AutomationBenefits CalculateAutomationImpact(AutomationLevel level, FacilityScale scale)
        {
            if (!_isInitialized)
                return new AutomationBenefits();
            
            var benefits = new AutomationBenefits
            {
                AutomationLevel = level,
                FacilityScale = scale,
                ConsistencyImprovement = CalculateConsistencyBonus(level),
                EfficiencyGains = CalculateEfficiencyBonus(level),
                ScalabilityEnhancement = CalculateScalingBonus(level, scale),
                QualityOptimization = CalculateQualityBonus(level),
                TimeLiberation = CalculateTimeBonus(level, scale)
            };
            
            // Calculate overall benefit score
            benefits.OverallBenefit = CalculateOverallBenefit(benefits);
            
            // Apply benefits to relevant systems
            ApplyAutomationBenefits(benefits);
            
            return benefits;
        }
        
        private float CalculateConsistencyBonus(AutomationLevel level)
        {
            var baseConsistency = 1.0f;
            var automationBonus = level switch
            {
                AutomationLevel.FullyManual => 0f,
                AutomationLevel.BasicAutomation => 0.2f,
                AutomationLevel.IntermediateAutomation => 0.5f,
                AutomationLevel.AdvancedAutomation => 0.8f,
                AutomationLevel.FullyAutomated => 1.0f,
                _ => 0f
            };
            
            return baseConsistency + (automationBonus * (ConsistencyImprovementMultiplier - 1.0f));
        }
        
        private float CalculateEfficiencyBonus(AutomationLevel level)
        {
            var automationCoverage = GetAutomationCoverage(level);
            return 1.0f + (automationCoverage * (EfficiencyGainMultiplier - 1.0f));
        }
        
        private float CalculateScalingBonus(AutomationLevel level, FacilityScale scale)
        {
            var automationLevel = GetAutomationCoverage(level);
            var scaleComplexity = CalculateScaleComplexity(scale);
            
            return 1.0f + (automationLevel * scaleComplexity * (ScalabilityBonusMultiplier - 1.0f));
        }
        
        private float CalculateQualityBonus(AutomationLevel level)
        {
            var precisionControl = GetPrecisionControl(level);
            return precisionControl * QualityOptimizationBonus;
        }
        
        private float CalculateTimeBonus(AutomationLevel level, FacilityScale scale)
        {
            var automationCoverage = GetAutomationCoverage(level);
            var taskComplexity = CalculateTaskComplexity(scale);
            
            return automationCoverage * taskComplexity;
        }
        
        private float GetAutomationCoverage(AutomationLevel level)
        {
            return level switch
            {
                AutomationLevel.FullyManual => 0.0f,
                AutomationLevel.BasicAutomation => 0.2f,
                AutomationLevel.IntermediateAutomation => 0.5f,
                AutomationLevel.AdvancedAutomation => 0.8f,
                AutomationLevel.FullyAutomated => 1.0f,
                _ => 0.0f
            };
        }
        
        private float GetPrecisionControl(AutomationLevel level)
        {
            return level switch
            {
                AutomationLevel.FullyManual => 0.7f, // Human precision varies
                AutomationLevel.BasicAutomation => 0.8f,
                AutomationLevel.IntermediateAutomation => 0.9f,
                AutomationLevel.AdvancedAutomation => 0.95f,
                AutomationLevel.FullyAutomated => 0.98f,
                _ => 0.7f
            };
        }
        
        private float CalculateScaleComplexity(FacilityScale scale)
        {
            return (scale.PlantCount * scale.RoomCount * scale.SystemComplexity) / 1000f;
        }
        
        private float CalculateTaskComplexity(FacilityScale scale)
        {
            return Mathf.Log(1 + scale.PlantCount) * scale.RoomCount;
        }
        
        private float CalculateOverallBenefit(AutomationBenefits benefits)
        {
            var weights = _automationConfig.BenefitWeights;
            
            return (benefits.ConsistencyImprovement * weights.ConsistencyWeight) +
                   (benefits.EfficiencyGains * weights.EfficiencyWeight) +
                   (benefits.ScalabilityEnhancement * weights.ScalabilityWeight) +
                   (benefits.QualityOptimization * weights.QualityWeight) +
                   (benefits.TimeLiberation * weights.TimeWeight);
        }
        
        private void ApplyAutomationBenefits(AutomationBenefits benefits)
        {
            // Apply benefits to plant care system
            if (_plantCareSystem != null)
            {
                _plantCareSystem.ApplyAutomationBenefits(benefits);
            }
            
            // Track metrics
            _automationEfficiencyGained += benefits.EfficiencyGains - 1.0f;
            _totalBurdenReduction += benefits.TimeLiberation;
            
            // Raise automation benefit realized event
            RaiseAutomationBenefitRealizedEvent(benefits);
        }
        
        #endregion
        
        #region System State Management
        
        private void CheckAutomationAvailability(DataCultivationTaskType taskType, AutomationProgress progress)
        {
            if (progress.IsAutomationAvailable) return;
            
            var burdenThreshold = _automationConfig.GetBurdenThreshold(taskType);
            var experienceThreshold = _automationConfig.GetExperienceThreshold(taskType);
            
            var burdenMet = progress.CurrentBurden >= burdenThreshold;
            var experienceMet = GetTaskExperience(taskType) >= experienceThreshold;
            
            if (burdenMet && experienceMet)
            {
                progress.IsAutomationAvailable = true;
                progress.AvailabilityTimestamp = Time.time;
                
                RaiseAutomationAvailableEvent(taskType);
                
                ChimeraLogger.Log($"Automation now available for {taskType}", this);
            }
        }
        
        private int GetTaskExperience(DataCultivationTaskType taskType)
        {
            return _taskBurdenStates[taskType].TotalTasksPerformed;
        }
        
        #endregion
        
        #region Event Management
        
        private void RaiseBurdenThresholdReached(DataCultivationTaskType taskType, AutomationDesireLevel desireLevel)
        {
            var eventData = new BurdenThresholdEventData
            {
                TaskType = taskType,
                DesireLevel = desireLevel,
                CurrentBurden = _automationProgress[taskType].CurrentBurden,
                Timestamp = Time.time
            };
            
            _onBurdenThresholdReached?.RaiseEvent(eventData);
        }
        
        private void RaiseAutomationAvailableEvent(DataCultivationTaskType taskType)
        {
            var eventData = new AutomationAvailableEventData
            {
                TaskType = taskType,
                Timestamp = Time.time
            };
            
            _onAutomationDesireIncreased?.RaiseEvent(eventData);
        }
        
        private void RaiseAutomationUnlockedEvent(AutomationUnlock unlock)
        {
            var eventData = new AutomationUnlockEventData
            {
                TaskType = unlock.TaskType.ToString(), // Convert to string
                SystemType = ConvertToUnlockedSystemsString(unlock), // Convert to string
                Timestamp = (float)(System.DateTime.Now - System.DateTime.UnixEpoch).TotalSeconds // Convert to float timestamp
            };
            
            _onAutomationUnlocked?.RaiseEvent(eventData);
        }
        
        private void RaiseAutomationBenefitRealizedEvent(AutomationBenefits benefits)
        {
            var eventData = new AutomationBenefitEventData
            {
                Benefits = benefits,
                Timestamp = Time.time
            };
            
            _onAutomationBenefitRealized?.RaiseEvent(eventData);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get current automation desire level for specific task type
        /// </summary>
        public AutomationDesire GetAutomationDesire(DataCultivationTaskType taskType)
        {
            if (!_isInitialized || !_automationProgress.ContainsKey(taskType))
                return new AutomationDesire { DesireLevel = DataAutomationDesireLevel.None };
            
            var progress = _automationProgress[taskType];
            var burdenState = _taskBurdenStates[taskType];
            
            return new AutomationDesire
            {
                TaskType = taskType,
                DesireLevel = progress.AutomationDesireLevel,
                OverallBurden = progress.CurrentBurden,
                CognitiveLoad = burdenState.CognitiveLoad,
                TimeInvestment = burdenState.TimeInvestment,
                ConsistencyChallenge = burdenState.ConsistencyChallenge,
                ScalePressure = burdenState.ScalePressure,
                QualityRisk = burdenState.QualityRisk
            };
        }
        
        /// <summary>
        /// Check if automation is available for unlock for specific task type
        /// </summary>
        public bool IsAutomationAvailable(DataCultivationTaskType taskType)
        {
            return _isInitialized && 
                   _automationProgress.ContainsKey(taskType) && 
                   _automationProgress[taskType].IsAutomationAvailable;
        }
        
        /// <summary>
        /// Check if automation is already unlocked for specific task type
        /// </summary>
        public bool IsAutomationUnlocked(DataCultivationTaskType taskType)
        {
            return _isInitialized && 
                   _automationProgress.ContainsKey(taskType) && 
                   _automationProgress[taskType].IsAutomationUnlocked;
        }
        
        /// <summary>
        /// Get total number of unlocked automation systems
        /// </summary>
        public int GetTotalUnlockedSystems()
        {
            return _automationSystems.Values.Count(system => system.IsUnlocked);
        }
        
        /// <summary>
        /// Get automation system state for specific system type
        /// </summary>
        public AutomationSystemState GetSystemState(AutomationSystemType systemType)
        {
            return _automationSystems.ContainsKey(systemType) ? 
                   _automationSystems[systemType] : 
                   new AutomationSystemState { SystemType = systemType, IsUnlocked = false };
        }
        
        /// <summary>
        /// Get automation progression metrics
        /// </summary>
        public AutomationProgressionMetrics GetProgressionMetrics()
        {
            return new AutomationProgressionMetrics
            {
                TotalBurdenReduction = _totalBurdenReduction,
                AutomationSystemsUnlocked = _automationSystemsUnlocked,
                AutomationEfficiencyGained = _automationEfficiencyGained,
                AvailableAutomationTasks = _automationProgress.Values.Count(p => p.IsAutomationAvailable),
                UnlockedAutomationTasks = _automationProgress.Values.Count(p => p.IsAutomationUnlocked)
            };
        }
        
        /// <summary>
        /// Record manual task performance for burden calculation
        /// </summary>
        public void RecordManualTaskPerformance(DataCultivationTaskType taskType, float duration, CareQuality quality)
        {
            if (!_isInitialized || !_taskBurdenStates.ContainsKey(taskType))
                return;
            
            var burdenState = _taskBurdenStates[taskType];
            burdenState.TotalTasksPerformed++;
            
            // Update average duration
            burdenState.AverageTaskDuration = 
                (burdenState.AverageTaskDuration * (burdenState.TotalTasksPerformed - 1) + duration) / 
                burdenState.TotalTasksPerformed;
            
            // Update consistency rating
            var qualityValue = quality switch
            {
                CareQuality.Perfect => 1.0f,
                CareQuality.Excellent => 0.9f,
                CareQuality.Good => 0.7f,
                CareQuality.Average => 0.5f,
                CareQuality.Poor => 0.3f,
                _ => 0.1f
            };
            
            burdenState.ConsistencyRating = 
                (burdenState.ConsistencyRating * 0.9f) + (qualityValue * 0.1f);
        }
        
        #endregion
        
        #region System Updates
        
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            UpdateBurdenDecay(deltaTime);
            UpdateAutomationSystems(deltaTime);
            UpdateProgressionTracking(deltaTime);
        }
        
        private void UpdateBurdenDecay(float deltaTime)
        {
            // Gradually reduce burden over time if no new burden is added
            foreach (var progress in _automationProgress.Values)
            {
                progress.TimeSinceLastBurdenIncrease += deltaTime;
                
                if (progress.TimeSinceLastBurdenIncrease > _automationConfig.BurdenDecayDelay)
                {
                    var decayRate = _automationConfig.BurdenDecayRate;
                    progress.CurrentBurden = Mathf.Max(0, progress.CurrentBurden - (decayRate * deltaTime));
                    
                    // Update desire level based on new burden
                    progress.AutomationDesireLevel = DetermineBurdenLevel(progress.CurrentBurden);
                }
            }
        }
        
        private void UpdateAutomationSystems(float deltaTime)
        {
            foreach (var systemState in _automationSystems.Values)
            {
                if (!systemState.IsActive) continue;
                
                systemState.OperationalHours += deltaTime / 3600f; // Convert to hours
                
                // Check for maintenance requirements
                var maintenanceInterval = _automationConfig.GetMaintenanceInterval(systemState.SystemType);
                if (systemState.OperationalHours - systemState.LastMaintenanceTime > maintenanceInterval)
                {
                    systemState.MaintenanceRequired = true;
                    systemState.EfficiencyLevel *= 0.9f; // Reduce efficiency when maintenance is due
                }
            }
        }
        
        private void UpdateProgressionTracking(float deltaTime)
        {
            // Update any progression-related tracking here
        }
        
        #endregion
        
        #region Automation Benefits
        
        public void ApplyAutomationBenefits(AutomationUnlockEventData unlockData)
        {
            // Apply benefits of automation unlock
            var systemType = ParseAutomationSystemType(unlockData.SystemType);
            var taskType = ParseDataCultivationTaskType(unlockData.TaskType);
            
            if (_automationSystems.ContainsKey(systemType))
            {
                var systemState = _automationSystems[systemType];
                systemState.IsActive = true;
                systemState.EfficiencyLevel = CalculateInitialEfficiency(systemType);
                
                // Reduce burden for the associated task
                if (_automationProgress.ContainsKey(taskType))
                {
                    var progress = _automationProgress[taskType];
                    progress.CurrentBurden *= 0.3f; // Reduce burden by 70%
                    progress.IsAutomationUnlocked = true;
                }
            }
        }
        
        private float CalculateInitialEfficiency(AutomationSystemType systemType)
        {
            // Calculate initial efficiency based on system type
            return systemType switch
            {
                AutomationSystemType.IrrigationSystem => 0.85f,
                AutomationSystemType.ClimateControl => 0.9f,
                AutomationSystemType.MonitoringSensors => 0.95f,
                AutomationSystemType.DataCollection => 0.98f,
                _ => 0.8f
            };
        }
        
        #endregion
        
        #region Utility Methods
        
        private float CalculateSystemEfficiency(float playerSkill, float requiredSkill)
        {
            var skillRatio = playerSkill / requiredSkill;
            return Mathf.Clamp01(0.7f + (skillRatio * 0.3f)); // 70% to 100% efficiency range
        }
        
        private float CalculateMaintenanceRequirement(AutomationSystemType systemType)
        {
            return _automationConfig.GetMaintenanceInterval(systemType);
        }
        
        private List<AutomationSystemType> GetUnlockedSystems(AutomationUnlock unlock)
        {
            var systems = new List<AutomationSystemType>();
            
            if (unlock.IrrigationAutomation.IsUnlocked) systems.Add(AutomationSystemType.IrrigationSystem);
            if (unlock.EnvironmentalAutomation.IsUnlocked) systems.Add(AutomationSystemType.ClimateControl);
            if (unlock.NutrientAutomation.IsUnlocked) systems.Add(AutomationSystemType.NutrientDelivery);
            if (unlock.MonitoringAutomation.IsUnlocked) systems.Add(AutomationSystemType.SensorNetwork);
            if (unlock.LightingAutomation.IsUnlocked) systems.Add(AutomationSystemType.LightingSchedule);
            
            return systems;
        }
        
        private int GetUnlockedSystemsCount(AutomationUnlock unlock)
        {
            return GetUnlockedSystems(unlock).Count;
        }
        
        /// <summary>
        /// Helper method to convert string to DataCultivationTaskType enum
        /// </summary>
        private DataCultivationTaskType ParseDataCultivationTaskType(string taskTypeString)
        {
            if (System.Enum.TryParse<DataCultivationTaskType>(taskTypeString, true, out var result))
            {
                return result;
            }
            return DataCultivationTaskType.Watering; // Default fallback
        }
        
        /// <summary>
        /// Helper method to convert string to AutomationSystemType enum
        /// </summary>
        private AutomationSystemType ParseAutomationSystemType(string systemTypeString)
        {
            if (System.Enum.TryParse<AutomationSystemType>(systemTypeString, true, out var result))
            {
                return result;
            }
            return AutomationSystemType.IrrigationSystem; // Default fallback
        }
        
        private UnlockedSystems ConvertToUnlockedSystemsEnum(AutomationUnlock unlock)
        {
            // Return the primary unlocked system - prioritize the most significant one
            if (unlock.IrrigationAutomation.IsUnlocked) return UnlockedSystems.AutoWatering;
            if (unlock.EnvironmentalAutomation.IsUnlocked) return UnlockedSystems.ClimateControl;
            if (unlock.NutrientAutomation.IsUnlocked) return UnlockedSystems.NutrientInjection;
            if (unlock.MonitoringAutomation.IsUnlocked) return UnlockedSystems.MonitoringSystem;
            if (unlock.LightingAutomation.IsUnlocked) return UnlockedSystems.LightingAutomation;
            
            // Default fallback
            return UnlockedSystems.MonitoringSystem;
        }
        
        private string ConvertToUnlockedSystemsString(AutomationUnlock unlock)
        {
            // Return the primary unlocked system as string
            if (unlock.IrrigationAutomation.IsUnlocked) return "AutoWatering";
            if (unlock.EnvironmentalAutomation.IsUnlocked) return "ClimateControl";
            if (unlock.NutrientAutomation.IsUnlocked) return "NutrientInjection";
            if (unlock.MonitoringAutomation.IsUnlocked) return "MonitoringSystem";
            if (unlock.LightingAutomation.IsUnlocked) return "LightingAutomation";
            
            // Default fallback
            return "MonitoringSystem";
        }
        
        private NarrativeTaskType ConvertToNarrativeTaskType(DataCultivationTaskType cultivationTaskType)
        {
            return cultivationTaskType switch
            {
                DataCultivationTaskType.Watering => NarrativeTaskType.Watering,
                DataCultivationTaskType.Fertilizing => NarrativeTaskType.Fertilizing,
                DataCultivationTaskType.Pruning => NarrativeTaskType.Pruning,
                DataCultivationTaskType.Training => NarrativeTaskType.Training,
                DataCultivationTaskType.Harvesting => NarrativeTaskType.Harvesting,
                DataCultivationTaskType.Transplanting => NarrativeTaskType.Transplanting,
                DataCultivationTaskType.PestControl => NarrativeTaskType.PestControl,
                DataCultivationTaskType.EnvironmentalAdjustment => NarrativeTaskType.EnvironmentalAdjustment,
                DataCultivationTaskType.Monitoring => NarrativeTaskType.Monitoring,
                DataCultivationTaskType.Maintenance => NarrativeTaskType.Maintenance,
                DataCultivationTaskType.Breeding => NarrativeTaskType.Breeding,
                DataCultivationTaskType.Processing => NarrativeTaskType.Processing,
                DataCultivationTaskType.Research => NarrativeTaskType.Research,
                DataCultivationTaskType.Construction => NarrativeTaskType.Construction,
                DataCultivationTaskType.Automation => NarrativeTaskType.Automation,
                _ => NarrativeTaskType.Monitoring // Default fallback
            };
        }
        
        #endregion
    }
    
    #region Data Structures and Enums
    
    [System.Serializable]
    public class AutomationProgress
    {
        public DataCultivationTaskType TaskType;
        public float CurrentBurden;
        public float BurdenThreshold;
        public DataAutomationDesireLevel AutomationDesireLevel;
        public bool IsAutomationAvailable;
        public bool IsAutomationUnlocked;
        public float TimeSinceLastBurdenIncrease;
        public float AvailabilityTimestamp;
        public float UnlockTimestamp;
        public bool HasRaisedHighBurdenEvent;
    }
    
    [System.Serializable]
    public class TaskBurdenState
    {
        public DataCultivationTaskType TaskType;
        public float CognitiveLoad;
        public float TimeInvestment;
        public float ConsistencyChallenge;
        public float ScalePressure;
        public float QualityRisk;
        public int TotalTasksPerformed;
        public float AverageTaskDuration;
        public float ConsistencyRating;
    }
    
    [System.Serializable]
    public class AutomationSystemState
    {
        public AutomationSystemType SystemType;
        public bool IsUnlocked;
        public bool IsActive;
        public float EfficiencyLevel;
        public bool MaintenanceRequired;
        public float LastMaintenanceTime;
        public float OperationalHours;
    }
    
    [System.Serializable]
    public class AutomationDesire
    {
        public DataCultivationTaskType TaskType;
        public DataAutomationDesireLevel DesireLevel;
        public float OverallBurden;
        public float CognitiveLoad;
        public float TimeInvestment;
        public float ConsistencyChallenge;
        public float ScalePressure;
        public float QualityRisk;
    }
    
    [System.Serializable]
    public class AutomationUnlock
    {
        public bool Success;
        public string Reason;
        public DataCultivationTaskType TaskType;
        public AutomationSystemUnlock IrrigationAutomation;
        public AutomationSystemUnlock EnvironmentalAutomation;
        public AutomationSystemUnlock NutrientAutomation;
        public AutomationSystemUnlock MonitoringAutomation;
        public AutomationSystemUnlock LightingAutomation;
    }
    
    [System.Serializable]
    public class AutomationSystemUnlock
    {
        public bool IsUnlocked;
        public AutomationSystemType SystemType;
        public float EfficiencyLevel;
        public float MaintenanceRequirement;
    }
    
    [System.Serializable]
    public class AutomationBenefits
    {
        public AutomationLevel AutomationLevel;
        public FacilityScale FacilityScale;
        public float ConsistencyImprovement;
        public float EfficiencyGains;
        public float ScalabilityEnhancement;
        public float QualityOptimization;
        public float TimeLiberation;
        public float OverallBenefit;
    }
    
    [System.Serializable]
    public class ManualCareTask
    {
        public DataCultivationTaskType TaskType;
        public float AverageDuration;
        public int PlantCount;
        public int FacilitySize;
        public float Frequency;
        public float RequiredQualityLevel;
        public bool IsUrgent;
    }
    
    [System.Serializable]
    public class PlayerProficiency
    {
        public float SkillLevel;
        public float ConsistencyRating;
    }
    
    [System.Serializable]
    public class PlayerProgress
    {
        public float IrrigationMastery;
        public float EnvironmentalSkill;
        public float NutritionExpertise;
        public float ObservationSkill;
        public float PhotoperiodKnowledge;
    }
    
    [System.Serializable]
    public class FacilityScale
    {
        public int PlantCount;
        public int RoomCount;
        public float SystemComplexity;
        public float TaskComplexity;
    }
    
    [System.Serializable]
    public class AutomationProgressionMetrics
    {
        public float TotalBurdenReduction;
        public int AutomationSystemsUnlocked;
        public float AutomationEfficiencyGained;
        public int AvailableAutomationTasks;
        public int UnlockedAutomationTasks;
    }
    
    // Event Data Classes
    [System.Serializable]
    public class BurdenThresholdEventData
    {
        public DataCultivationTaskType TaskType;
        public AutomationDesireLevel DesireLevel;
        public float CurrentBurden;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class AutomationAvailableEventData
    {
        public DataCultivationTaskType TaskType;
        public float Timestamp;
    }
    
    // AutomationUnlockEventData is defined in ProjectChimera.Data.Cultivation.SkillNodeEventData to avoid CS0101 duplicate definition errors
    
    [System.Serializable]
    public class AutomationBenefitEventData
    {
        public AutomationBenefits Benefits;
        public float Timestamp;
    }
    
    // Note: AutomationDesireLevel enum is defined in ProjectChimera.Data.Cultivation namespace
    
    #endregion
}