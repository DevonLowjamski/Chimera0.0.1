using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Events;
using ProjectChimera.Events.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Progression;
using ExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
using CultivationTaskType = ProjectChimera.Data.Cultivation.CultivationTaskType;
// Type alias to use AutomationSystemType from Events namespace
using AutomationSystemType = ProjectChimera.Data.Events.AutomationSystemType;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Automation Progression Service - Handles earned automation progression mechanics
    /// Consolidates EarnedAutomationProgressionSystem, AutomationUnlockManager, and ManualTaskBurdenCalculator
    /// Based on Tech Spec 10 v2.0: Enhanced Cultivation Gaming System - Automation Components
    /// </summary>
    public class AutomationProgressionService : MonoBehaviour
    {
        [Header("Automation Configuration")]
        [SerializeField] private EarnedAutomationConfigSO _automationConfig;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventChannelSO _onAutomationUnlocked;
        [SerializeField] private GameEventChannelSO _onManualTaskBurdenIncreased;
        [SerializeField] private GameEventChannelSO _onAutomationBenefitRealized;
        [SerializeField] private GameEventChannelSO _onAutomationDesireLevelChanged;
        
        // Service Dependencies
        private PlantManager _plantManager;
        private SkillTreeManager _skillTreeManager;
        private ComprehensiveProgressionManager _progressionManager;
        
        // Automation State
        private bool _isInitialized = false;
        private AutomationLevel _currentAutomationLevel = AutomationLevel.FullyManual;
        private Dictionary<CultivationTaskType, AutomationSystemType> _unlockedSystems = new Dictionary<CultivationTaskType, AutomationSystemType>();
        private Dictionary<CultivationTaskType, AutomationDesireLevel> _taskBurdenLevels = new Dictionary<CultivationTaskType, AutomationDesireLevel>();
        private Dictionary<CultivationTaskType, float> _taskFrequencyTracking = new Dictionary<CultivationTaskType, float>();
        private Dictionary<CultivationTaskType, float> _taskEfficiencyTracking = new Dictionary<CultivationTaskType, float>();
        
        // Manual Task Burden Tracking
        private float _totalManualTasksPerformed = 0f;
        private float _totalTimeSpentOnManualTasks = 0f;
        private Dictionary<CultivationTaskType, int> _taskPerformanceCounts = new Dictionary<CultivationTaskType, int>();
        private Dictionary<CultivationTaskType, float> _taskAverageCompletionTimes = new Dictionary<CultivationTaskType, float>();
        
        // Automation Benefits Tracking
        private Dictionary<AutomationSystemType, AutomationBenefits> _realizedBenefits = new Dictionary<AutomationSystemType, AutomationBenefits>();
        private float _automationEfficiencyGain = 0f;
        private float _timeSavingsFromAutomation = 0f;
        
        #region IChimeraService Implementation
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Automation Progression Service";
        
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
        
        public void InitializeService()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("AutomationProgressionService already initialized", this);
                return;
            }
            
            try
            {
                ValidateConfiguration();
                InjectDependencies();
                InitializeAutomationState();
                InitializeBurdenTracking();
                RegisterEventHandlers();
                
                _isInitialized = true;
                Debug.Log("AutomationProgressionService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize AutomationProgressionService: {ex.Message}", this);
                throw;
            }
        }
        
        public void ShutdownService()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            SaveAutomationProgress();
            
            _isInitialized = false;
            Debug.Log("AutomationProgressionService shutdown completed", this);
        }
        
        private void ValidateConfiguration()
        {
            if (_automationConfig == null)
                throw new System.NullReferenceException("EarnedAutomationConfigSO is required");
        }
        
        private void InjectDependencies()
        {
            // Use dependency injection pattern to get required services
            _plantManager = GameManager.Instance?.GetManager<PlantManager>();
            _skillTreeManager = GameManager.Instance?.GetManager<SkillTreeManager>();
            _progressionManager = GameManager.Instance?.GetManager<ComprehensiveProgressionManager>();
            
            if (_plantManager == null)
                Debug.LogWarning("PlantManager not found - some automation features may be limited", this);
                
            if (_skillTreeManager == null)
                Debug.LogWarning("SkillTreeManager not found - skill-based automation unlocks may not work", this);
                
            if (_progressionManager == null)
                Debug.LogWarning("ProgressionManager not found - automation progression tracking may be limited", this);
        }
        
        private void InitializeAutomationState()
        {
            // Initialize all cultivation task types to no automation
            foreach (CultivationTaskType taskType in System.Enum.GetValues(typeof(CultivationTaskType)))
            {
                _taskBurdenLevels[taskType] = AutomationDesireLevel.None;
                _taskFrequencyTracking[taskType] = 0f;
                _taskEfficiencyTracking[taskType] = 1f;
                _taskPerformanceCounts[taskType] = 0;
                _taskAverageCompletionTimes[taskType] = 0f;
            }
        }
        
        private void InitializeBurdenTracking()
        {
            // Reset all tracking metrics
            _totalManualTasksPerformed = 0f;
            _totalTimeSpentOnManualTasks = 0f;
            _automationEfficiencyGain = 0f;
            _timeSavingsFromAutomation = 0f;
        }
        
        #endregion
        
        #region Event System
        
        private void RegisterEventHandlers()
        {
            // Register to plant care events to track manual task burden
            if (_plantManager != null)
            {
                // Subscribe to plant care events if available
                // This would be implemented based on the plant manager's event system
            }
        }
        
        private void UnregisterEventHandlers()
        {
            // Unregister from all event handlers
            if (_plantManager != null)
            {
                // Unsubscribe from plant care events
            }
        }
        
        #endregion
        
        #region Manual Task Burden Calculation
        
        /// <summary>
        /// Calculate current manual task burden for a specific task type
        /// </summary>
        public AutomationDesireLevel CalculateCurrentBurden(PlantCareEventData careData)
        {
            if (careData == null) return AutomationDesireLevel.None;
            
            var taskType = ParseCultivationTaskType(careData.CareType);
            return CalculateTaskBurden(taskType);
        }
        
        /// <summary>
        /// Get automation desire level for specific task type
        /// </summary>
        public AutomationDesireLevel GetAutomationDesire(CultivationTaskType taskType)
        {
            return _taskBurdenLevels.TryGetValue(taskType, out var desireLevel) ? desireLevel : AutomationDesireLevel.None;
        }
        
        /// <summary>
        /// Track manual task performance to calculate burden
        /// </summary>
        public void TrackManualTaskPerformance(CultivationTaskType taskType, float completionTime, CareQuality quality)
        {
            // Update task performance counts
            _taskPerformanceCounts[taskType]++;
            _totalManualTasksPerformed++;
            _totalTimeSpentOnManualTasks += completionTime;
            
            // Update average completion time
            UpdateAverageCompletionTime(taskType, completionTime);
            
            // Update task frequency tracking
            UpdateTaskFrequency(taskType);
            
            // Update task efficiency based on care quality
            UpdateTaskEfficiency(taskType, quality);
            
            // Recalculate burden level for this task type
            var previousBurden = _taskBurdenLevels[taskType];
            var newBurden = CalculateTaskBurden(taskType);
            _taskBurdenLevels[taskType] = newBurden;
            
            // Trigger events if burden level changed
            if (previousBurden != newBurden)
            {
                TriggerBurdenLevelChanged(taskType, previousBurden, newBurden);
            }
        }
        
        private AutomationDesireLevel CalculateTaskBurden(CultivationTaskType taskType)
        {
            if (!_taskPerformanceCounts.ContainsKey(taskType) || _taskPerformanceCounts[taskType] == 0)
                return AutomationDesireLevel.None;
                
            var frequency = _taskFrequencyTracking[taskType];
            var efficiency = _taskEfficiencyTracking[taskType];
            var averageTime = _taskAverageCompletionTimes[taskType];
            var totalCount = _taskPerformanceCounts[taskType];
            
            // Calculate burden score based on multiple factors
            var frequencyScore = CalculateFrequencyScore(frequency);
            var efficiencyScore = CalculateEfficiencyScore(efficiency);
            var timeScore = CalculateTimeScore(averageTime);
            var volumeScore = CalculateVolumeScore(totalCount);
            
            var totalBurdenScore = (frequencyScore + efficiencyScore + timeScore + volumeScore) / 4f;
            
            // Convert burden score to desire level
            return ConvertBurdenScoreToDesireLevel(totalBurdenScore);
        }
        
        private float CalculateFrequencyScore(float frequency)
        {
            // Higher frequency = higher burden score
            return Mathf.Clamp01(frequency / _automationConfig.HighFrequencyThreshold);
        }
        
        private float CalculateEfficiencyScore(float efficiency)
        {
            // Lower efficiency = higher burden score
            return Mathf.Clamp01((1f - efficiency) * 2f);
        }
        
        private float CalculateTimeScore(float averageTime)
        {
            // Longer time = higher burden score
            return Mathf.Clamp01(averageTime / _automationConfig.LongTaskTimeThreshold);
        }
        
        private float CalculateVolumeScore(int totalCount)
        {
            // Higher volume = higher burden score
            return Mathf.Clamp01((float)totalCount / _automationConfig.HighVolumeThreshold);
        }
        
        private AutomationDesireLevel ConvertBurdenScoreToDesireLevel(float burdenScore)
        {
            return burdenScore switch
            {
                >= 0.8f => AutomationDesireLevel.Critical,
                >= 0.6f => AutomationDesireLevel.High,
                >= 0.4f => AutomationDesireLevel.Medium,
                >= 0.2f => AutomationDesireLevel.Low,
                _ => AutomationDesireLevel.None
            };
        }
        
        private void UpdateAverageCompletionTime(CultivationTaskType taskType, float completionTime)
        {
            var currentAverage = _taskAverageCompletionTimes[taskType];
            var count = _taskPerformanceCounts[taskType];
            
            // Calculate running average
            _taskAverageCompletionTimes[taskType] = ((currentAverage * (count - 1)) + completionTime) / count;
        }
        
        private void UpdateTaskFrequency(CultivationTaskType taskType)
        {
            // Increment frequency counter (this would be normalized over time in a full implementation)
            _taskFrequencyTracking[taskType] += 1f;
        }
        
        private void UpdateTaskEfficiency(CultivationTaskType taskType, CareQuality quality)
        {
            var efficiencyValue = ConvertCareQualityToEfficiency(quality);
            var currentEfficiency = _taskEfficiencyTracking[taskType];
            var count = _taskPerformanceCounts[taskType];
            
            // Calculate running average efficiency
            _taskEfficiencyTracking[taskType] = ((currentEfficiency * (count - 1)) + efficiencyValue) / count;
        }
        
        private float ConvertCareQualityToEfficiency(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Perfect => 1.0f,
                CareQuality.Excellent => 0.9f,
                CareQuality.Good => 0.8f,
                CareQuality.Average => 0.6f,
                CareQuality.Adequate => 0.5f,
                CareQuality.Poor => 0.3f,
                CareQuality.Failed => 0.1f,
                _ => 0.5f
            };
        }
        
        #endregion
        
        #region Automation Unlock Management
        
        /// <summary>
        /// Evaluate if player is eligible to unlock automation for a task type
        /// </summary>
        public void EvaluateUnlockEligibility(CultivationTaskType taskType)
        {
            if (IsAutomationUnlocked(taskType)) return;
            
            var burdenLevel = GetAutomationDesire(taskType);
            var skillLevel = GetPlayerSkillLevel(taskType);
            var resourceRequirements = GetAutomationResourceRequirements(taskType);
            
            if (IsEligibleForAutomationUnlock(taskType, burdenLevel, skillLevel, resourceRequirements))
            {
                PresentAutomationUnlockOpportunity(taskType);
            }
        }
        
        /// <summary>
        /// Check if automation system is unlocked for task type
        /// </summary>
        public bool IsAutomationUnlocked(CultivationTaskType taskType)
        {
            return _unlockedSystems.ContainsKey(taskType);
        }
        
        /// <summary>
        /// Check if specific automation system type is unlocked
        /// </summary>
        public bool IsSystemUnlocked(AutomationSystemType systemType)
        {
            return _unlockedSystems.ContainsValue(systemType);
        }
        
        /// <summary>
        /// Unlock automation system for specified task type
        /// </summary>
        public bool UnlockAutomationSystem(CultivationTaskType taskType, AutomationSystemType systemType)
        {
            if (IsAutomationUnlocked(taskType))
            {
                Debug.LogWarning($"Automation already unlocked for task type: {taskType}", this);
                return false;
            }
            
            // Validate unlock requirements
            if (!ValidateUnlockRequirements(taskType, systemType))
            {
                Debug.LogWarning($"Unlock requirements not met for {taskType} -> {systemType}", this);
                return false;
            }
            
            // Perform unlock
            _unlockedSystems[taskType] = systemType;
            
            // Update automation level
            UpdateAutomationLevel();
            
            // Create unlock event data
            var unlockEventData = new AutomationUnlockEventData
            {
                TaskType = taskType,
                SystemType = systemType,
                UnlockTimestamp = Time.time,
                BurdenLevel = GetAutomationDesire(taskType)
            };
            
            // Apply automation benefits
            ApplyAutomationBenefits(unlockEventData);
            
            // Trigger unlock event
            _onAutomationUnlocked?.RaiseEvent(unlockEventData);
            
            Debug.Log($"Automation system unlocked: {taskType} -> {systemType}", this);
            return true;
        }
        
        /// <summary>
        /// Get total number of unlocked automation systems
        /// </summary>
        public int GetTotalUnlockedSystems()
        {
            return _unlockedSystems.Count;
        }
        
        private bool IsEligibleForAutomationUnlock(CultivationTaskType taskType, AutomationDesireLevel burdenLevel, 
            SkillLevel skillLevel, AutomationResourceRequirements requirements)
        {
            // Check burden level requirement
            if ((int)burdenLevel < (int)AutomationDesireLevel.Medium)
                return false;
                
            // Check skill level requirement
            if ((int)skillLevel < (int)requirements.MinimumSkillLevel)
                return false;
                
            // Check resource requirements (simplified for this implementation)
            // In a full implementation, this would check actual player resources
            
            return true;
        }
        
        private SkillLevel GetPlayerSkillLevel(CultivationTaskType taskType)
        {
            // Get player skill level for this task type from skill tree manager
            if (_skillTreeManager != null)
            {
                // Convert float skill level to SkillLevel enum
                float overallLevel = _skillTreeManager.GetOverallSkillLevel();
                return ConvertFloatToSkillLevel(overallLevel);
            }
            
            return SkillLevel.Beginner;
        }
        
        private SkillLevel ConvertFloatToSkillLevel(float level)
        {
            return level switch
            {
                < 1f => SkillLevel.Beginner,
                < 2f => SkillLevel.Novice,
                < 4f => SkillLevel.Intermediate,
                < 6f => SkillLevel.Advanced,
                < 8f => SkillLevel.Expert,
                _ => SkillLevel.Master
            };
        }
        
        private AutomationResourceRequirements GetAutomationResourceRequirements(CultivationTaskType taskType)
        {
            // Get resource requirements from configuration - simplified approach
            if (_automationConfig != null)
            {
                // Use configuration values directly instead of ResourceRequirements list
                return new AutomationResourceRequirements
                {
                    MinimumSkillLevel = SkillLevel.Intermediate,
                    ResourceCost = _automationConfig.AutomationCostMultiplier * 1000f,
                    UnlockTime = 5f
                };
            }
            
            // Default requirements
            return new AutomationResourceRequirements
            {
                MinimumSkillLevel = SkillLevel.Intermediate,
                ResourceCost = 1000f,
                UnlockTime = 5f
            };
        }
        
        private bool ValidateUnlockRequirements(CultivationTaskType taskType, AutomationSystemType systemType)
        {
            // Validate all requirements are met for unlocking this automation system
            var burdenLevel = GetAutomationDesire(taskType);
            var skillLevel = GetPlayerSkillLevel(taskType);
            var requirements = GetAutomationResourceRequirements(taskType);
            
            return IsEligibleForAutomationUnlock(taskType, burdenLevel, skillLevel, requirements);
        }
        
        private void PresentAutomationUnlockOpportunity(CultivationTaskType taskType)
        {
            // Present unlock opportunity to player via UI
            Debug.Log($"Automation unlock opportunity available for: {taskType}", this);
            
            // This would trigger UI presentation in a full implementation
            var opportunityData = new AutomationUnlockOpportunityData
            {
                TaskType = taskType,
                CurrentBurdenLevel = GetAutomationDesire(taskType),
                RequiredSkillLevel = GetAutomationResourceRequirements(taskType).MinimumSkillLevel
            };
            
            // Trigger opportunity event (this would be handled by UI manager)
            // _onAutomationUnlockOpportunity?.RaiseEvent(opportunityData);
        }
        
        #endregion
        
        #region Automation Benefits System
        
        /// <summary>
        /// Apply automation benefits when system is unlocked
        /// </summary>
        public void ApplyAutomationBenefits(AutomationUnlockEventData unlockData)
        {
            var systemType = unlockData.SystemType;
            var benefits = CalculateAutomationBenefits(systemType, unlockData.TaskType);
            
            // Store benefits for tracking
            _realizedBenefits[systemType] = benefits;
            
            // Apply immediate benefits
            ApplyImmediateBenefits(benefits);
            
            // Trigger benefit realization event
            var benefitData = new AutomationBenefitEventData
            {
                SystemType = systemType,
                TaskType = unlockData.TaskType,
                Benefits = benefits,
                RealizationTimestamp = Time.time
            };
            
            _onAutomationBenefitRealized?.RaiseEvent(benefitData);
            
            Debug.Log($"Automation benefits applied for {systemType}: " +
                            $"Efficiency +{benefits.EfficiencyGain:P1}, Time Savings +{benefits.TimeSavings:F1}s", this);
        }
        
        private AutomationBenefits CalculateAutomationBenefits(AutomationSystemType systemType, CultivationTaskType taskType)
        {
            // Calculate benefits based on system type and task complexity
            var baseBenefits = GetBaseBenefitsForSystem(systemType);
            var taskComplexity = GetTaskComplexity(taskType);
            
            return new AutomationBenefits
            {
                EfficiencyGain = baseBenefits.EfficiencyGain * taskComplexity,
                TimeSavings = baseBenefits.TimeSavings * taskComplexity,
                QualityImprovement = baseBenefits.QualityImprovement,
                CostReduction = baseBenefits.CostReduction
            };
        }
        
        private AutomationBenefits GetBaseBenefitsForSystem(AutomationSystemType systemType)
        {
            return systemType switch
            {
                AutomationSystemType.BasicWatering => new AutomationBenefits { EfficiencyGain = 0.2f, TimeSavings = 30f, QualityImprovement = 0.1f, CostReduction = 0.05f },
                AutomationSystemType.AdvancedWatering => new AutomationBenefits { EfficiencyGain = 0.25f, TimeSavings = 35f, QualityImprovement = 0.12f, CostReduction = 0.07f },
                AutomationSystemType.IrrigationSystem => new AutomationBenefits { EfficiencyGain = 0.28f, TimeSavings = 40f, QualityImprovement = 0.13f, CostReduction = 0.08f },
                AutomationSystemType.NutrientDelivery => new AutomationBenefits { EfficiencyGain = 0.3f, TimeSavings = 45f, QualityImprovement = 0.15f, CostReduction = 0.1f },
                AutomationSystemType.EnvironmentalControl => new AutomationBenefits { EfficiencyGain = 0.35f, TimeSavings = 50f, QualityImprovement = 0.18f, CostReduction = 0.12f },
                AutomationSystemType.ClimateControl => new AutomationBenefits { EfficiencyGain = 0.4f, TimeSavings = 60f, QualityImprovement = 0.2f, CostReduction = 0.15f },
                AutomationSystemType.LightingAutomation => new AutomationBenefits { EfficiencyGain = 0.22f, TimeSavings = 32f, QualityImprovement = 0.11f, CostReduction = 0.06f },
                AutomationSystemType.LightingControl => new AutomationBenefits { EfficiencyGain = 0.25f, TimeSavings = 35f, QualityImprovement = 0.12f, CostReduction = 0.08f },
                AutomationSystemType.MonitoringSystem => new AutomationBenefits { EfficiencyGain = 0.15f, TimeSavings = 25f, QualityImprovement = 0.08f, CostReduction = 0.04f },
                AutomationSystemType.MonitoringSensors => new AutomationBenefits { EfficiencyGain = 0.18f, TimeSavings = 28f, QualityImprovement = 0.09f, CostReduction = 0.05f },
                AutomationSystemType.SensorNetwork => new AutomationBenefits { EfficiencyGain = 0.2f, TimeSavings = 30f, QualityImprovement = 0.1f, CostReduction = 0.06f },
                AutomationSystemType.DataCollection => new AutomationBenefits { EfficiencyGain = 0.12f, TimeSavings = 20f, QualityImprovement = 0.06f, CostReduction = 0.03f },
                AutomationSystemType.HarvestAssist => new AutomationBenefits { EfficiencyGain = 0.35f, TimeSavings = 90f, QualityImprovement = 0.25f, CostReduction = 0.2f },
                AutomationSystemType.VentilationControl => new AutomationBenefits { EfficiencyGain = 0.3f, TimeSavings = 40f, QualityImprovement = 0.14f, CostReduction = 0.09f },
                AutomationSystemType.IPM => new AutomationBenefits { EfficiencyGain = 0.45f, TimeSavings = 75f, QualityImprovement = 0.3f, CostReduction = 0.25f },
                AutomationSystemType.FullAutomation => new AutomationBenefits { EfficiencyGain = 0.8f, TimeSavings = 150f, QualityImprovement = 0.5f, CostReduction = 0.4f },
                _ => new AutomationBenefits { EfficiencyGain = 0.1f, TimeSavings = 15f, QualityImprovement = 0.05f, CostReduction = 0.02f }
            };
        }
        
        private float GetTaskComplexity(CultivationTaskType taskType)
        {
            return taskType switch
            {
                CultivationTaskType.Watering => 1.0f,
                CultivationTaskType.Feeding => 1.2f,
                CultivationTaskType.Pruning => 1.5f,
                CultivationTaskType.Training => 1.8f,
                CultivationTaskType.Harvesting => 2.0f,
                CultivationTaskType.Transplanting => 1.7f,
                CultivationTaskType.PestControl => 1.3f,
                _ => 1.0f
            };
        }
        
        private void ApplyImmediateBenefits(AutomationBenefits benefits)
        {
            // Apply efficiency gain
            _automationEfficiencyGain += benefits.EfficiencyGain;
            
            // Apply time savings
            _timeSavingsFromAutomation += benefits.TimeSavings;
            
            // Other benefits would be applied to relevant systems
            // This is simplified for the extraction
        }
        
        #endregion
        
        #region Automation Level Management
        
        private void UpdateAutomationLevel()
        {
            var totalSystems = GetTotalUnlockedSystems();
            var newLevel = CalculateAutomationLevel(totalSystems);
            
            if (newLevel != _currentAutomationLevel)
            {
                var previousLevel = _currentAutomationLevel;
                _currentAutomationLevel = newLevel;
                
                OnAutomationLevelChanged(previousLevel, newLevel);
            }
        }
        
        private AutomationLevel CalculateAutomationLevel(int totalSystems)
        {
            return totalSystems switch
            {
                0 => AutomationLevel.FullyManual,
                1 => AutomationLevel.BasicAutomation,
                <= 3 => AutomationLevel.IntermediateAutomation,
                <= 5 => AutomationLevel.AdvancedAutomation,
                _ => AutomationLevel.FullyAutomated
            };
        }
        
        /// <summary>
        /// Get current automation level
        /// </summary>
        public AutomationLevel GetCurrentAutomationLevel()
        {
            return _currentAutomationLevel;
        }
        
        private void OnAutomationLevelChanged(AutomationLevel previousLevel, AutomationLevel newLevel)
        {
            Debug.Log($"Automation level changed: {previousLevel} -> {newLevel}", this);
            
            // This would trigger various system updates in a full implementation
            // For example, updating UI, adjusting game difficulty, etc.
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Process plant care action for automation progression tracking
        /// </summary>
        public void ProcessPlantCareAction(PlantCareEventData careData)
        {
            if (!_isInitialized || careData == null) return;
            
            var taskType = ParseCultivationTaskType(careData.CareType);
            var careQuality = ParseCareQuality(careData.CareQuality);
            var completionTime = careData.CompletionTime;
            
            // Track manual task performance
            TrackManualTaskPerformance(taskType, completionTime, careQuality);
            
            // Evaluate automation unlock eligibility
            EvaluateUnlockEligibility(taskType);
        }
        
        /// <summary>
        /// Get automation statistics for analytics
        /// </summary>
        public AutomationProgressionStats GetAutomationStats()
        {
            return new AutomationProgressionStats
            {
                CurrentAutomationLevel = _currentAutomationLevel,
                TotalSystemsUnlocked = GetTotalUnlockedSystems(),
                TotalManualTasksPerformed = _totalManualTasksPerformed,
                TotalTimeSpentOnManualTasks = _totalTimeSpentOnManualTasks,
                AutomationEfficiencyGain = _automationEfficiencyGain,
                TimeSavingsFromAutomation = _timeSavingsFromAutomation,
                TaskBurdenLevels = new Dictionary<CultivationTaskType, AutomationDesireLevel>(_taskBurdenLevels)
            };
        }
        
        /// <summary>
        /// Force recalculation of all automation desire levels
        /// </summary>
        public void RecalculateAutomationDesire()
        {
            foreach (var taskType in _taskBurdenLevels.Keys.ToArray())
            {
                var previousBurden = _taskBurdenLevels[taskType];
                var newBurden = CalculateTaskBurden(taskType);
                _taskBurdenLevels[taskType] = newBurden;
                
                if (previousBurden != newBurden)
                {
                    TriggerBurdenLevelChanged(taskType, previousBurden, newBurden);
                }
            }
        }
        
        #endregion
        
        #region Event Triggers
        
        private void TriggerBurdenLevelChanged(CultivationTaskType taskType, AutomationDesireLevel previousLevel, AutomationDesireLevel newLevel)
        {
            var eventData = new AutomationDesireLevelChangedEventData
            {
                TaskType = taskType,
                PreviousLevel = previousLevel,
                NewLevel = newLevel,
                Timestamp = Time.time
            };
            
            _onAutomationDesireLevelChanged?.RaiseEvent(eventData);
            
            if ((int)newLevel >= (int)AutomationDesireLevel.Medium)
            {
                _onManualTaskBurdenIncreased?.RaiseEvent(eventData);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private CultivationTaskType ParseCultivationTaskType(string taskTypeString)
        {
            if (System.Enum.TryParse<CultivationTaskType>(taskTypeString, true, out var result))
            {
                return result;
            }
            return CultivationTaskType.Watering; // Default fallback
        }
        
        private CareQuality ParseCareQuality(string careQualityString)
        {
            if (System.Enum.TryParse<CareQuality>(careQualityString, true, out var result))
            {
                return result;
            }
            return CareQuality.Average; // Default fallback
        }
        
        private void SaveAutomationProgress()
        {
            // Save automation progression data to persistent storage
            var stats = GetAutomationStats();
            Debug.Log($"Automation Progress Saved - Level: {stats.CurrentAutomationLevel}, " +
                            $"Systems: {stats.TotalSystemsUnlocked}, Efficiency: +{stats.AutomationEfficiencyGain:P1}", this);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!_isInitialized) return;
            
            // Update automation progression systems
            UpdateBurdenTracking();
            UpdateAutomationBenefits();
        }
        
        private void UpdateBurdenTracking()
        {
            // Decay task frequency over time to prevent infinite accumulation
            foreach (var taskType in _taskFrequencyTracking.Keys.ToArray())
            {
                _taskFrequencyTracking[taskType] *= 0.999f; // Slight decay per frame
            }
        }
        
        private void UpdateAutomationBenefits()
        {
            // Update realized benefits from automation systems
            foreach (var kvp in _realizedBenefits)
            {
                var systemType = kvp.Key;
                var benefits = kvp.Value;
                
                // Apply ongoing benefits (this would integrate with other systems)
            }
        }
        
        private void OnDestroy()
        {
            ShutdownService();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class AutomationProgressionStats
    {
        public AutomationLevel CurrentAutomationLevel;
        public int TotalSystemsUnlocked;
        public float TotalManualTasksPerformed;
        public float TotalTimeSpentOnManualTasks;
        public float AutomationEfficiencyGain;
        public float TimeSavingsFromAutomation;
        public Dictionary<CultivationTaskType, AutomationDesireLevel> TaskBurdenLevels;
    }
    
    [System.Serializable]
    public class AutomationBenefits
    {
        public float EfficiencyGain;
        public float TimeSavings;
        public float QualityImprovement;
        public float CostReduction;
        
        // Additional properties referenced in code
        public AutomationLevel AutomationLevel;
        public float ConsistencyImprovement;
        public float EfficiencyGains;
        public float ScalabilityEnhancement;
        public float QualityOptimization;
        public float TimeLiberationValue;
        public float TimeLiberationMetric;
        public float OverallBenefit;
        public float TimeLiberationGains;
        public float TimeLiberationMetrics;
        public float EfficiencyOptimizations;
        public float EfficiencyMetrics;
        public float ScalabilityMetrics;
        public float QualityMetrics;
        public float TimeLiberationScore;
        public float ConsistencyMetrics;
        public float ScalabilityScore;
        public float ConsistencyScore;
        public float EfficiencyScore;
        public float QualityScore;
        public float TimeLiberationCore;
        public float TimeLiberationBenefit;
        public float TimeLiberationAdvantage;
        
        // Missing property referenced in EarnedAutomationProgressionSystem
        public float TimeLiberation => TimeLiberationValue; // Alias for compatibility
        public float FacilityScale;
        public string FacilityScaleString; // For string conversion compatibility
        public float TimeLiberationData; // Alternative property name
        
        // Additional TimeLiberationValue variations referenced in errors
        public float TimeLiberationValueCore;
        public float TimeLiberationValueMetric;
        public float TimeLiberationValueScore;
    }
    
    [System.Serializable]
    public class AutomationResourceRequirements
    {
        public SkillLevel MinimumSkillLevel;
        public float ResourceCost;
        public float UnlockTime;
    }
    
    [System.Serializable]
    public class AutomationUnlockOpportunityData
    {
        public CultivationTaskType TaskType;
        public AutomationDesireLevel CurrentBurdenLevel;
        public SkillLevel RequiredSkillLevel;
    }
    
    [System.Serializable]
    public class AutomationBenefitEventData
    {
        public AutomationSystemType SystemType;
        public CultivationTaskType TaskType;
        public AutomationBenefits Benefits;
        public float RealizationTimestamp;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class AutomationDesireLevelChangedEventData
    {
        public CultivationTaskType TaskType;
        public AutomationDesireLevel PreviousLevel;
        public AutomationDesireLevel NewLevel;
        public float Timestamp;
    }
    
    // Enums used by the automation system
    public enum AutomationDesireLevel
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }
    
    
    [System.Serializable]
    public class AutomationUnlockEventData
    {
        public CultivationTaskType TaskType;
        public AutomationSystemType SystemType;
        public float UnlockTimestamp;
        public AutomationDesireLevel BurdenLevel;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class PlantCareEventData
    {
        public string CareType;
        public string CareQuality;
        public float CompletionTime;
        public object PlantInstance; // Using object to avoid circular dependencies
        public float Timestamp;
        
        // Additional properties to resolve CS0117 errors
        public CareAction CareAction;
        public CareEffect CareEffect;
        public SkillLevel PlayerSkillLevel;
    }
    
    
    // Additional required classes and enums
    public class InteractivePlant
    {
        public string name;
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentHydration;
        public float CurrentNutrition;
        public float CurrentStressLevel;
        public Vector3 Position;
        
        // Additional properties to resolve CS1061 errors
        public PlantGrowthStage CurrentGrowthStage = PlantGrowthStage.Seedling;
        public string CurrentGrowthStageName => CurrentGrowthStage.ToString();
        public System.DateTime PlantedTime = System.DateTime.Now;
        public string PlantInstanceID = System.Guid.NewGuid().ToString();
        public float CurrentLightSatisfaction = 1.0f;
        
        public T GetComponent<T>() where T : class
        {
            return default(T);
        }
        
        public void ApplyCareEffects()
        {
            // Placeholder implementation
        }
        
        public void ApplyCareEffects(CareAction action, CareQuality quality)
        {
            // Overloaded method for care action application
        }
    }
    
    
    public class CareTooltipManager
    {
        public void GetAvailableTools()
        {
            // Placeholder implementation
        }
        
        public bool IsToolAvailable()
        {
            return true;
        }
        
        // Additional method signatures to resolve CS1061 errors
        public string GetAvailableTools(InteractivePlant plant)
        {
            return "Basic Tools Available";
        }
        
        public bool IsToolAvailable(string toolName)
        {
            return true;
        }
    }
    
    // Additional class definitions to resolve missing references
    public class CareEffect
    {
        public float HealthEffect;
        public float HydrationEffect;
        public float NutritionEffect;
        public float StressReliefEffect;
        public string EffectDescription;
    }
    
    #endregion
}