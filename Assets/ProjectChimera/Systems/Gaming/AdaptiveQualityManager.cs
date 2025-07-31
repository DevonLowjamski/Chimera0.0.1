using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Adaptive Quality Manager - Dynamic quality adjustment for gameplay
    /// Part of Module 1: Gaming Experience Core - Deliverable 2.2
    /// 
    /// Intelligently adjusts graphics quality in real-time based on performance feedback
    /// to maintain optimal gaming experience while preserving visual fidelity.
    /// </summary>
    public class AdaptiveQualityManager : ChimeraManager, IAdaptiveQualityManager
    {
        [Header("Adaptive Quality Configuration")]
        [SerializeField] private bool _enableAdaptiveQuality = true;
        [SerializeField] private GamingPerformanceState _targetPerformanceLevel = GamingPerformanceState.Good;
        [SerializeField] private float _aggressivenessLevel = 0.5f; // 0.0-1.0
        [SerializeField] private QualityLevel _minimumQualityLevel = QualityLevel.Low;
        [SerializeField] private QualityLevel _maximumQualityLevel = QualityLevel.Ultra;
        
        [Header("Adjustment Timing")]
        [SerializeField] private float _adjustmentCooldown = 3.0f; // Seconds between adjustments
        [SerializeField] private float _emergencyAdjustmentDelay = 0.5f; // Faster adjustments for critical performance
        [SerializeField] private int _performanceFramesRequired = 60; // Frames to confirm performance state
        
        [Header("Quality Settings")]
        [SerializeField] private QualityPerformanceImpacts _performanceImpacts;
        [SerializeField] private QualityVisualImportance _visualImportance;
        
        [Header("Debug & Logging")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private bool _enableAdjustmentLogging = true;
        
        // Current state
        private QualityLevel _currentQualityLevel = QualityLevel.High;
        private QualityLevel _targetQualityLevel = QualityLevel.High;
        private QualityAdjustmentMode _currentAdjustmentMode = QualityAdjustmentMode.Stable;
        private bool _isAdjustingQuality = false;
        private float _timeSinceLastAdjustment = 0f;
        
        // Performance monitoring integration
        private IGamingPerformanceMonitor _performanceMonitor;
        private GamingPerformanceState _lastPerformanceState = GamingPerformanceState.Optimal;
        private int _performanceStateFrameCount = 0;
        
        // Quality adjustment tracking
        private readonly List<QualityAdjustmentRecord> _adjustmentHistory = new List<QualityAdjustmentRecord>();
        
        // Analysis data
        private QualityAnalysisData _lastAnalysis;
        private bool _manualOverride = false;
        private float _manualOverrideTime = 0f;
        private const float MANUAL_OVERRIDE_DURATION = 30f; // 30 seconds
        
        #region Properties
        
        public bool EnableAdaptiveQuality
        {
            get => _enableAdaptiveQuality;
            set => _enableAdaptiveQuality = value;
        }
        
        public GamingPerformanceState TargetPerformanceLevel
        {
            get => _targetPerformanceLevel;
            set => _targetPerformanceLevel = value;
        }
        
        public float AggressivenessLevel
        {
            get => _aggressivenessLevel;
            set => _aggressivenessLevel = Mathf.Clamp01(value);
        }
        
        public QualityLevel MinimumQualityLevel
        {
            get => _minimumQualityLevel;
            set => _minimumQualityLevel = value;
        }
        
        public QualityLevel MaximumQualityLevel
        {
            get => _maximumQualityLevel;
            set => _maximumQualityLevel = value;
        }
        
        public QualityLevel CurrentQualityLevel => _currentQualityLevel;
        public QualityLevel TargetQualityLevel => _targetQualityLevel;
        public QualityAdjustmentMode CurrentAdjustmentMode => _currentAdjustmentMode;
        public bool IsAdjustingQuality => _isAdjustingQuality;
        public float TimeSinceLastAdjustment => _timeSinceLastAdjustment;
        
        // Quality settings are managed through Unity's built-in system
        
        public QualityPerformanceImpacts PerformanceImpacts
        {
            get => _performanceImpacts;
            set => _performanceImpacts = value;
        }
        
        public QualityVisualImportance VisualImportance
        {
            get => _visualImportance;
            set => _visualImportance = value;
        }
        
        #endregion
        
        #region Events
        
        public event Action<QualityChangeEventData> OnQualityChanged;
        public event Action<QualityAdjustmentEventData> OnQualityAdjusted;
        public event Action<QualityLimitEventData> OnQualityLimitReached;
        
        #endregion
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeAdaptiveQuality();
            FindPerformanceMonitor();
            
            Debug.Log("[AdaptiveQualityManager] Dynamic quality adjustment system initialized");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastAdjustment += Time.unscaledDeltaTime;
            
            // Handle manual override timeout
            if (_manualOverride)
            {
                _manualOverrideTime += Time.unscaledDeltaTime;
                if (_manualOverrideTime >= MANUAL_OVERRIDE_DURATION)
                {
                    _manualOverride = false;
                    Debug.Log("[AdaptiveQualityManager] Manual override expired, resuming adaptive quality");
                }
            }
            
            // Perform adaptive quality adjustments
            if (_enableAdaptiveQuality && !_manualOverride)
            {
                UpdateAdaptiveQuality();
            }
        }
        
        protected override void OnManagerShutdown()
        {
            if (_performanceMonitor != null)
            {
                _performanceMonitor.OnPerformanceDegraded -= OnPerformanceDegraded;
                _performanceMonitor.OnPerformanceRecovered -= OnPerformanceRecovered;
                _performanceMonitor.OnCriticalPerformance -= OnCriticalPerformance;
            }
            
            Debug.Log("[AdaptiveQualityManager] Dynamic quality adjustment shutdown complete");
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeAdaptiveQuality()
        {
            // Initialize default quality settings if not set
            if (_performanceImpacts == null)
            {
                _performanceImpacts = CreateDefaultPerformanceImpacts();
            }
            
            if (_visualImportance == null)
            {
                _visualImportance = CreateDefaultVisualImportance();
            }
            
            // Get current Unity quality level
            _currentQualityLevel = (QualityLevel)QualitySettings.GetQualityLevel();
            _targetQualityLevel = _currentQualityLevel;
            
            // Apply current settings
            ApplyQualitySettingsToUnity();
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[AdaptiveQualityManager] Initialized at quality level: {_currentQualityLevel}");
            }
        }
        
        private void FindPerformanceMonitor()
        {
            // Try to get performance monitor from service registry
            var serviceRegistry = GamingServiceRegistry.Instance;
            if (serviceRegistry != null)
            {
                _performanceMonitor = serviceRegistry.GetService<IGamingPerformanceMonitor>();
            }
            
            // Fallback to finding in scene
            if (_performanceMonitor == null)
            {
                _performanceMonitor = FindObjectOfType<GamingPerformanceMonitor>();
            }
            
            if (_performanceMonitor != null)
            {
                // Subscribe to performance events
                _performanceMonitor.OnPerformanceDegraded += OnPerformanceDegraded;
                _performanceMonitor.OnPerformanceRecovered += OnPerformanceRecovered;
                _performanceMonitor.OnCriticalPerformance += OnCriticalPerformance;
                
                Debug.Log("[AdaptiveQualityManager] Connected to Gaming Performance Monitor");
            }
            else
            {
                Debug.LogWarning("[AdaptiveQualityManager] Gaming Performance Monitor not found - operating in standalone mode");
            }
        }
        
        #endregion
        
        #region Adaptive Quality Logic
        
        private void UpdateAdaptiveQuality()
        {
            if (_performanceMonitor == null) return;
            
            var currentPerformance = _performanceMonitor.CurrentPerformanceState;
            
            // Track performance state consistency
            if (currentPerformance == _lastPerformanceState)
            {
                _performanceStateFrameCount++;
            }
            else
            {
                _performanceStateFrameCount = 1;
                _lastPerformanceState = currentPerformance;
            }
            
            // Determine if adjustment is needed
            var shouldAdjust = ShouldAdjustQuality(currentPerformance);
            var adjustmentCooldown = GetAdjustmentCooldown(currentPerformance);
            
            if (shouldAdjust && _timeSinceLastAdjustment >= adjustmentCooldown)
            {
                PerformQualityAdjustment(currentPerformance);
            }
        }
        
        private bool ShouldAdjustQuality(GamingPerformanceState currentPerformance)
        {
            // Check if we need enough consistent frames to confirm performance state
            var requiredFrames = GetRequiredFramesForAdjustment(currentPerformance);
            if (_performanceStateFrameCount < requiredFrames)
                return false;
            
            // Check if performance is below target
            if (currentPerformance < _targetPerformanceLevel && _currentQualityLevel > _minimumQualityLevel)
            {
                return true; // Need to reduce quality
            }
            
            // Check if performance is above target and we can increase quality
            if (currentPerformance > _targetPerformanceLevel && _currentQualityLevel < _maximumQualityLevel)
            {
                return true; // Can increase quality
            }
            
            return false;
        }
        
        private void PerformQualityAdjustment(GamingPerformanceState currentPerformance)
        {
            _isAdjustingQuality = true;
            
            var previousLevel = _currentQualityLevel;
            var newLevel = DetermineTargetQualityLevel(currentPerformance);
            
            if (newLevel != _currentQualityLevel)
            {
                ApplyQualityLevel(newLevel, currentPerformance);
                
                // Record adjustment
                RecordQualityAdjustment(previousLevel, newLevel, currentPerformance);
                
                // Fire events
                FireQualityChangeEvent(previousLevel, newLevel, currentPerformance);
            }
            
            _timeSinceLastAdjustment = 0f;
            _isAdjustingQuality = false;
        }
        
        private QualityLevel DetermineTargetQualityLevel(GamingPerformanceState currentPerformance)
        {
            var currentLevel = (int)_currentQualityLevel;
            var adjustmentStep = CalculateAdjustmentStep(currentPerformance);
            
            if (currentPerformance < _targetPerformanceLevel)
            {
                // Reduce quality
                currentLevel += adjustmentStep;
            }
            else if (currentPerformance > _targetPerformanceLevel)
            {
                // Increase quality
                currentLevel -= adjustmentStep;
            }
            
            // Clamp to valid range
            currentLevel = Mathf.Clamp(currentLevel, (int)_maximumQualityLevel, (int)_minimumQualityLevel);
            
            return (QualityLevel)currentLevel;
        }
        
        private int CalculateAdjustmentStep(GamingPerformanceState currentPerformance)
        {
            // Base step size
            var step = 1;
            
            // Adjust based on aggressiveness
            if (_aggressivenessLevel > 0.7f)
            {
                step = 2; // More aggressive adjustments
            }
            
            // Emergency adjustments for critical performance
            if (currentPerformance == GamingPerformanceState.Critical)
            {
                step = Mathf.Max(2, (int)(_aggressivenessLevel * 3));
            }
            
            return step;
        }
        
        #endregion
        
        #region Performance Event Handlers
        
        private void OnPerformanceDegraded(GamingPerformanceData performanceData)
        {
            if (!_enableAdaptiveQuality || _manualOverride) return;
            
            if (_enableAdjustmentLogging)
            {
                Debug.Log($"[AdaptiveQualityManager] Performance degraded to {performanceData.PerformanceState} - considering quality reduction");
            }
            
            // Immediate adjustment for severe degradation
            if (performanceData.PerformanceState <= GamingPerformanceState.Poor)
            {
                _timeSinceLastAdjustment = _adjustmentCooldown; // Allow immediate adjustment
            }
        }
        
        private void OnPerformanceRecovered(GamingPerformanceData performanceData)
        {
            if (!_enableAdaptiveQuality || _manualOverride) return;
            
            if (_enableAdjustmentLogging)
            {
                Debug.Log($"[AdaptiveQualityManager] Performance recovered to {performanceData.PerformanceState} - considering quality increase");
            }
        }
        
        private void OnCriticalPerformance(GamingPerformanceData performanceData)
        {
            if (!_enableAdaptiveQuality || _manualOverride) return;
            
            // Emergency quality reduction
            if (_currentQualityLevel > _minimumQualityLevel)
            {
                var emergencyLevel = (QualityLevel)Mathf.Max((int)_minimumQualityLevel, (int)_currentQualityLevel + 2);
                
                Debug.LogWarning($"[AdaptiveQualityManager] Critical performance detected - emergency quality reduction to {emergencyLevel}");
                
                ApplyQualityLevel(emergencyLevel, performanceData.PerformanceState);
                _timeSinceLastAdjustment = 0f;
                
                // Fire emergency adjustment event
                var adjustmentData = new QualityAdjustmentEventData
                {
                    SettingName = "Emergency Quality Reduction",
                    PreviousValue = _currentQualityLevel,
                    NewValue = emergencyLevel,
                    EstimatedPerformanceGain = EstimatePerformanceImpact(_currentQualityLevel, emergencyLevel),
                    TriggeringPerformanceState = performanceData.PerformanceState,
                    Timestamp = DateTime.UtcNow
                };
                
                OnQualityAdjusted?.Invoke(adjustmentData);
            }
            else
            {
                // Already at minimum quality - fire limit reached event
                var limitEvent = new QualityLimitEventData
                {
                    LimitReached = _minimumQualityLevel,
                    IsMinimumLimit = true,
                    CurrentPerformance = performanceData.PerformanceState,
                    RecommendedAction = "Consider reducing game complexity or upgrading hardware",
                    Timestamp = DateTime.UtcNow
                };
                
                OnQualityLimitReached?.Invoke(limitEvent);
            }
        }
        
        #endregion
        
        #region Quality Application
        
        private void ApplyQualityLevel(QualityLevel level, GamingPerformanceState triggeringPerformance)
        {
            var previousLevel = _currentQualityLevel;
            _currentQualityLevel = level;
            
            // Apply the quality settings
            ApplyQualitySettingsToUnity();
            
            // Update adjustment mode
            if (level > previousLevel)
            {
                _currentAdjustmentMode = QualityAdjustmentMode.Upgrading;
            }
            else if (level < previousLevel)
            {
                _currentAdjustmentMode = triggeringPerformance == GamingPerformanceState.Critical 
                    ? QualityAdjustmentMode.Emergency 
                    : QualityAdjustmentMode.Downgrading;
            }
            else
            {
                _currentAdjustmentMode = QualityAdjustmentMode.Stable;
            }
            
            if (_enableAdjustmentLogging)
            {
                Debug.Log($"[AdaptiveQualityManager] Applied quality level: {level} (was {previousLevel})");
            }
        }
        
        private void ApplyQualitySettingsToUnity()
        {
            // Apply Unity quality settings - use the built-in quality level system
            QualitySettings.SetQualityLevel((int)_currentQualityLevel, true);
            
            // Apply additional quality adjustments based on level
            ApplyLevelSpecificSettings(_currentQualityLevel);
        }
        
        private void ApplyLevelSpecificSettings(QualityLevel level)
        {
            // Apply settings based on quality level using Unity's built-in properties
            switch (level)
            {
                case QualityLevel.Ultra:
                    QualitySettings.globalTextureMipmapLimit = 0;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                    QualitySettings.antiAliasing = 8;
                    QualitySettings.shadowDistance = 200f;
                    QualitySettings.lodBias = 2.0f;
                    Application.targetFrameRate = 60;
                    break;
                    
                case QualityLevel.High:
                    QualitySettings.globalTextureMipmapLimit = 0;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.shadowDistance = 150f;
                    QualitySettings.lodBias = 2.0f;
                    Application.targetFrameRate = 60;
                    break;
                    
                case QualityLevel.Medium:
                    QualitySettings.globalTextureMipmapLimit = 1;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.shadowDistance = 100f;
                    QualitySettings.lodBias = 1.5f;
                    Application.targetFrameRate = 60;
                    break;
                    
                case QualityLevel.Low:
                    QualitySettings.globalTextureMipmapLimit = 2;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.lodBias = 1.0f;
                    Application.targetFrameRate = 60;
                    break;
                    
                case QualityLevel.Minimum:
                    QualitySettings.globalTextureMipmapLimit = 3;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.shadowDistance = 25f;
                    QualitySettings.lodBias = 0.5f;
                    Application.targetFrameRate = 30;
                    break;
            }
            
            // Apply VSync setting (disable for better performance monitoring)
            QualitySettings.vSyncCount = 0;
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetQualityLevel(QualityLevel level)
        {
            // Enable manual override
            _manualOverride = true;
            _manualOverrideTime = 0f;
            
            ApplyQualityLevel(level, GamingPerformanceState.Optimal);
            
            Debug.Log($"[AdaptiveQualityManager] Manual quality override to {level} for {MANUAL_OVERRIDE_DURATION} seconds");
        }
        
        public void ForceQualityAdjustment()
        {
            if (_performanceMonitor != null)
            {
                var currentPerformance = _performanceMonitor.CurrentPerformanceState;
                PerformQualityAdjustment(currentPerformance);
                
                Debug.Log("[AdaptiveQualityManager] Forced quality adjustment completed");
            }
        }
        
        public void ResetToDefaults()
        {
            _currentQualityLevel = QualityLevel.High;
            ApplyQualitySettingsToUnity();
            _manualOverride = false;
            
            Debug.Log("[AdaptiveQualityManager] Reset to default quality settings");
        }
        
        public void ApplyQualityPreset(QualityPreset preset)
        {
            var level = preset switch
            {
                QualityPreset.Ultra => QualityLevel.Ultra,
                QualityPreset.High => QualityLevel.High,
                QualityPreset.Medium => QualityLevel.Medium,
                QualityPreset.Low => QualityLevel.Low,
                _ => QualityLevel.High
            };
            
            SetQualityLevel(level);
        }
        
        public QualityAnalysisData GetQualityAnalysis()
        {
            var analysis = new QualityAnalysisData
            {
                CurrentLevel = _currentQualityLevel,
                RecommendedLevel = GetRecommendedQualityLevel(),
                OverallQualityScore = CalculateQualityScore(),
                PerformanceEfficiency = CalculatePerformanceEfficiency(),
                IsOptimalForHardware = IsOptimalForCurrentHardware(),
                CanIncreaseQuality = _currentQualityLevel < _maximumQualityLevel,
                ShouldDecreaseQuality = ShouldDecreaseQualityBasedOnPerformance(),
                BottleneckComponent = IdentifyBottleneckComponent(),
                TotalAdjustments = _adjustmentHistory.Count,
                AdjustmentsLastMinute = _adjustmentHistory.Count(r => (DateTime.UtcNow - r.Timestamp).TotalMinutes <= 1),
                RecommendedChanges = GenerateRecommendations(),
                PerformanceWarnings = GeneratePerformanceWarnings()
            };
            
            _lastAnalysis = analysis;
            return analysis;
        }
        
        public List<QualityAdjustmentRecord> GetAdjustmentHistory(int count = 50)
        {
            return _adjustmentHistory.TakeLast(count).ToList();
        }
        
        public float EstimatePerformanceImpact(QualityLevel fromLevel, QualityLevel toLevel)
        {
            // Simplified performance impact calculation
            var levelDifference = (int)toLevel - (int)fromLevel;
            var baseImpact = levelDifference * 15f; // ~15% per quality level
            
            // Adjust based on aggressiveness and hardware capability
            return baseImpact * (1f + _aggressivenessLevel * 0.5f);
        }
        
        public QualityLevel GetRecommendedQualityLevel()
        {
            if (_performanceMonitor == null)
                return QualityLevel.Medium;
            
            var currentPerformance = _performanceMonitor.CurrentPerformanceState;
            
            return currentPerformance switch
            {
                GamingPerformanceState.Optimal => QualityLevel.Ultra,
                GamingPerformanceState.Good => QualityLevel.High,
                GamingPerformanceState.Degraded => QualityLevel.Medium,
                GamingPerformanceState.Poor => QualityLevel.Low,
                GamingPerformanceState.Critical => QualityLevel.Minimum,
                _ => QualityLevel.Medium
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        private float GetAdjustmentCooldown(GamingPerformanceState currentPerformance)
        {
            return currentPerformance == GamingPerformanceState.Critical 
                ? _emergencyAdjustmentDelay 
                : _adjustmentCooldown;
        }
        
        private int GetRequiredFramesForAdjustment(GamingPerformanceState currentPerformance)
        {
            return currentPerformance == GamingPerformanceState.Critical 
                ? _performanceFramesRequired / 4 
                : _performanceFramesRequired;
        }
        
        private void RecordQualityAdjustment(QualityLevel fromLevel, QualityLevel toLevel, GamingPerformanceState triggeringPerformance)
        {
            var record = new QualityAdjustmentRecord
            {
                Timestamp = DateTime.UtcNow,
                FromLevel = fromLevel,
                ToLevel = toLevel,
                TriggeringPerformance = triggeringPerformance,
                FPSBefore = _performanceMonitor?.CurrentFPS ?? 0f,
                FPSAfter = 0f, // Will be updated in next frame
                AdjustmentDetails = $"Performance: {triggeringPerformance}, Mode: {_currentAdjustmentMode}",
                WasSuccessful = true
            };
            
            _adjustmentHistory.Add(record);
            
            // Keep history manageable
            if (_adjustmentHistory.Count > 1000)
            {
                _adjustmentHistory.RemoveRange(0, 500);
            }
        }
        
        private void FireQualityChangeEvent(QualityLevel previousLevel, QualityLevel newLevel, GamingPerformanceState triggeringPerformance)
        {
            var changeEvent = new QualityChangeEventData
            {
                PreviousLevel = previousLevel,
                NewLevel = newLevel,
                AdjustmentMode = _currentAdjustmentMode,
                Reason = $"Performance state: {triggeringPerformance}",
                PerformanceGain = EstimatePerformanceImpact(previousLevel, newLevel),
                VisualImpact = CalculateVisualImpact(previousLevel, newLevel),
                Timestamp = DateTime.UtcNow
            };
            
            OnQualityChanged?.Invoke(changeEvent);
        }
        
        private float CalculateQualityScore()
        {
            // Calculate overall quality score (0-100)
            var levelScore = (5 - (int)_currentQualityLevel) * 20f; // 20 points per level above minimum
            return Mathf.Clamp(levelScore, 0f, 100f);
        }
        
        private float CalculatePerformanceEfficiency()
        {
            if (_performanceMonitor == null) return 50f;
            
            var targetFPS = _performanceMonitor.TargetFrameRate;
            var currentFPS = _performanceMonitor.CurrentFPS;
            
            return Mathf.Clamp((currentFPS / targetFPS) * 100f, 0f, 100f);
        }
        
        private bool IsOptimalForCurrentHardware()
        {
            // Simplified hardware optimization check
            return _performanceMonitor?.IsPerformingOptimally ?? false;
        }
        
        private bool ShouldDecreaseQualityBasedOnPerformance()
        {
            return _performanceMonitor?.IsUnderPerformanceStress ?? false;
        }
        
        private string IdentifyBottleneckComponent()
        {
            // Simplified bottleneck identification
            if (_performanceMonitor == null) return "Unknown";
            
            var memoryUsage = _performanceMonitor.CurrentMemoryUsage;
            if (memoryUsage > 2000f) return "Memory";
            
            var currentFPS = _performanceMonitor.CurrentFPS;
            if (currentFPS < 30f) return "GPU";
            
            return "Balanced";
        }
        
        private List<string> GenerateRecommendations()
        {
            var recommendations = new List<string>();
            
            if (_currentQualityLevel > QualityLevel.Medium && _performanceMonitor?.IsUnderPerformanceStress == true)
            {
                recommendations.Add("Consider reducing quality level for better performance");
            }
            
            if (_currentQualityLevel < QualityLevel.High && _performanceMonitor?.IsPerformingOptimally == true)
            {
                recommendations.Add("Performance headroom available - can increase quality");
            }
            
            return recommendations;
        }
        
        private List<string> GeneratePerformanceWarnings()
        {
            var warnings = new List<string>();
            
            if (_adjustmentHistory.Count(r => (DateTime.UtcNow - r.Timestamp).TotalMinutes <= 1) > 5)
            {
                warnings.Add("Frequent quality adjustments - system may be unstable");
            }
            
            if (_currentQualityLevel == _minimumQualityLevel && _performanceMonitor?.IsUnderPerformanceStress == true)
            {
                warnings.Add("Already at minimum quality but performance issues persist");
            }
            
            return warnings;
        }
        
        private float CalculateVisualImpact(QualityLevel fromLevel, QualityLevel toLevel)
        {
            // Simplified visual impact calculation
            var levelDifference = (int)fromLevel - (int)toLevel;
            return levelDifference * 20f; // 20% visual impact per level
        }
        
        #endregion
        
        #region Helper Methods
        
        private QualityPerformanceImpacts CreateDefaultPerformanceImpacts()
        {
            return new QualityPerformanceImpacts();
        }
        
        private QualityVisualImportance CreateDefaultVisualImportance()
        {
            return new QualityVisualImportance();
        }
        
        #endregion
    }
}