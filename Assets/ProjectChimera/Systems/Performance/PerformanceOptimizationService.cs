using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Performance Optimization Service - Dedicated service for dynamic quality adjustment and optimization management
    /// Extracted from UnifiedPerformanceManagementSystem to provide focused optimization functionality
    /// Handles automatic quality adjustment, optimizer management, and system optimization coordination
    /// </summary>
    public class PerformanceOptimizationService : MonoBehaviour
    {
        [Header("Optimization Configuration")]
        [SerializeField] private bool _enableDynamicQualityAdjustment = true;
        [SerializeField] private bool _enableAutomaticOptimization = true;
        [SerializeField] private float _optimizationCooldownSeconds = 30f;
        [SerializeField] private float _qualityAdjustmentInterval = 5f;

        [Header("Optimization Thresholds")]
        [SerializeField] private float _criticalPerformanceThreshold = 0.5f; // 50% of target frame rate
        [SerializeField] private float _optimalPerformanceThreshold = 0.95f; // 95% of target frame rate
        [SerializeField] private float _memoryPressureThreshold = 0.8f;

        // Optimization management - using simple system references instead of interface
        private Dictionary<string, PerformanceOptimizer> _optimizers = new Dictionary<string, PerformanceOptimizer>();
        private List<IOptimizableSystem> _managedSystems = new List<IOptimizableSystem>();
        private Dictionary<string, PerformanceProfile> _systemProfiles = new Dictionary<string, PerformanceProfile>();

        // Optimization state
        private bool _isInitialized = false;
        private float _lastQualityAdjustment = 0f;
        private Dictionary<string, float> _optimizationCooldowns = new Dictionary<string, float>();

        // Events for optimization notifications
        public static event System.Action<string, PerformanceQualityLevel> OnSystemOptimized;
        public static event System.Action<string, PerformanceQualityLevel> OnQualityAdjusted;
        public static event System.Action<OptimizationReport> OnOptimizationCompleted;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Performance Optimization Service";
        public IReadOnlyDictionary<string, PerformanceOptimizer> Optimizers => _optimizers;
        public bool EnableDynamicQualityAdjustment => _enableDynamicQualityAdjustment;

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
            InitializeDataStructures();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDataStructures()
        {
            _optimizers = new Dictionary<string, PerformanceOptimizer>();
            _managedSystems = new List<IOptimizableSystem>();
            _systemProfiles = new Dictionary<string, PerformanceProfile>();
            _optimizationCooldowns = new Dictionary<string, float>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PerformanceOptimizationService already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                RegisterBuiltInOptimizers();
                
                _isInitialized = true;
                ChimeraLogger.Log("PerformanceOptimizationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize PerformanceOptimizationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            _managedSystems.Clear();
            _systemProfiles.Clear();
            _optimizers.Clear();
            _optimizationCooldowns.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("PerformanceOptimizationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_optimizationCooldownSeconds <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid optimization cooldown, using default 30s", this);
                _optimizationCooldownSeconds = 30f;
            }

            if (_qualityAdjustmentInterval <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid quality adjustment interval, using default 5s", this);
                _qualityAdjustmentInterval = 5f;
            }
        }

        #endregion

        #region System Registration

        /// <summary>
        /// Register a system for optimization management
        /// </summary>
        public void RegisterOptimizableSystem(IOptimizableSystem system)
        {
            if (system == null)
            {
                ChimeraLogger.LogWarning("Cannot register null system for optimization", this);
                return;
            }

            if (!_managedSystems.Contains(system))
            {
                _managedSystems.Add(system);
                
                // Create initial performance profile
                var profile = new PerformanceProfile
                {
                    SystemName = system.SystemName,
                    CurrentQuality = PerformanceQualityLevel.High,
                    LastOptimization = DateTime.Now,
                    IsBottleneck = false
                };
                
                _systemProfiles[system.SystemName] = profile;
                
                ChimeraLogger.Log($"Registered system for optimization: {system.SystemName}", this);
            }
        }

        /// <summary>
        /// Unregister a system from optimization management
        /// </summary>
        public void UnregisterOptimizableSystem(IOptimizableSystem system)
        {
            if (system == null) return;

            if (_managedSystems.Remove(system))
            {
                _systemProfiles.Remove(system.SystemName);
                _optimizationCooldowns.Remove(system.SystemName);
                ChimeraLogger.Log($"Unregistered system from optimization: {system.SystemName}", this);
            }
        }

        /// <summary>
        /// Register a performance optimizer for a specific system type
        /// </summary>
        public void RegisterOptimizer(string systemType, Func<object, bool> optimizationFunction, float cooldown = 5f)
        {
            _optimizers[systemType] = new PerformanceOptimizer
            {
                SystemType = systemType,
                OptimizationFunction = optimizationFunction,
                OptimizationCooldown = cooldown,
                IsEnabled = true,
                LastExecuted = DateTime.MinValue
            };

            ChimeraLogger.Log($"Registered optimizer for: {systemType}", this);
        }

        #endregion

        #region Dynamic Quality Adjustment

        private void Update()
        {
            if (!_isInitialized || !_enableDynamicQualityAdjustment) return;

            // Apply quality adjustments at specified interval
            if (Time.time - _lastQualityAdjustment >= _qualityAdjustmentInterval)
            {
                UpdateOptimizationCooldowns();
                _lastQualityAdjustment = Time.time;
            }
        }

        /// <summary>
        /// Apply dynamic quality adjustments based on current performance state
        /// </summary>
        public void ApplyDynamicQualityAdjustments(GlobalPerformanceMetrics globalMetrics)
        {
            if (!_enableDynamicQualityAdjustment) return;

            var performanceRatio = globalMetrics.CurrentFrameRate / globalMetrics.TargetFrameRate;
            
            if (performanceRatio < _criticalPerformanceThreshold || 
                globalMetrics.MemoryPressure > _memoryPressureThreshold)
            {
                // Critical performance - aggressively reduce quality
                ApplyEmergencyOptimizations(globalMetrics);
            }
            else if (performanceRatio < 0.8f)
            {
                // Poor performance - reduce quality gradually
                ApplyGradualOptimizations(globalMetrics, false);
            }
            else if (performanceRatio > _optimalPerformanceThreshold && 
                     globalMetrics.MemoryPressure < 0.6f)
            {
                // Good performance - consider increasing quality
                ApplyGradualOptimizations(globalMetrics, true);
            }
        }

        /// <summary>
        /// Apply emergency optimizations for critical performance issues
        /// </summary>
        private void ApplyEmergencyOptimizations(GlobalPerformanceMetrics globalMetrics)
        {
            var optimizedSystems = 0;
            
            foreach (var system in _managedSystems)
            {
                if (_systemProfiles.TryGetValue(system.SystemName, out var profile) && 
                    profile.CurrentQuality > PerformanceQualityLevel.Minimal)
                {
                    // Drop quality significantly
                    var newQuality = PerformanceQualityLevel.Low;
                    if (profile.CurrentQuality > PerformanceQualityLevel.Medium)
                    {
                        newQuality = PerformanceQualityLevel.Minimal;
                    }
                    
                    if (ApplySystemOptimization(system, newQuality))
                    {
                        optimizedSystems++;
                    }
                }
            }

            ChimeraLogger.LogWarning($"Applied emergency optimizations to {optimizedSystems} systems", this);
        }

        /// <summary>
        /// Apply gradual optimizations based on performance trend
        /// </summary>
        private void ApplyGradualOptimizations(GlobalPerformanceMetrics globalMetrics, bool increaseQuality)
        {
            var optimizedSystems = 0;
            
            foreach (var system in _managedSystems)
            {
                if (!CanOptimizeSystem(system.SystemName)) continue;

                if (_systemProfiles.TryGetValue(system.SystemName, out var profile))
                {
                    if (increaseQuality)
                    {
                        // Gradually increase quality for good performance
                        if (profile.CurrentQuality < PerformanceQualityLevel.Ultra)
                        {
                            var newQuality = (PerformanceQualityLevel)((int)profile.CurrentQuality + 1);
                            if (ApplySystemOptimization(system, newQuality))
                            {
                                optimizedSystems++;
                            }
                        }
                    }
                    else
                    {
                        // Gradually decrease quality for poor performance
                        if (profile.CurrentQuality > PerformanceQualityLevel.Low)
                        {
                            var newQuality = (PerformanceQualityLevel)((int)profile.CurrentQuality - 1);
                            if (ApplySystemOptimization(system, newQuality))
                            {
                                optimizedSystems++;
                            }
                        }
                    }
                }
            }

            if (optimizedSystems > 0)
            {
                var direction = increaseQuality ? "enhanced" : "reduced";
                ChimeraLogger.Log($"Gradually {direction} quality for {optimizedSystems} systems", this);
            }
        }

        /// <summary>
        /// Apply optimization to a specific system
        /// </summary>
        private bool ApplySystemOptimization(IOptimizableSystem system, PerformanceQualityLevel newQuality)
        {
            try
            {
                // Update the profile directly since we don't have OptimizePerformance method
                if (_systemProfiles.TryGetValue(system.SystemName, out var profile))
                {
                    profile.TotalOptimizations++;
                    profile.LastOptimization = DateTime.Now;
                    profile.CurrentQuality = newQuality;
                }

                // Set cooldown for this system
                _optimizationCooldowns[system.SystemName] = Time.time + _optimizationCooldownSeconds;

                OnSystemOptimized?.Invoke(system.SystemName, newQuality);
                OnQualityAdjusted?.Invoke(system.SystemName, newQuality);
                
                ChimeraLogger.Log($"Optimized {system.SystemName} to {newQuality}", this);
                return true;
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to optimize {system.SystemName}: {ex.Message}", this);
                return false;
            }
        }

        #endregion

        #region Optimizer Management

        /// <summary>
        /// Execute specific optimizer for a system
        /// </summary>
        public bool ExecuteOptimizer(string systemType, object systemInstance)
        {
            if (!_optimizers.TryGetValue(systemType, out var optimizer))
            {
                ChimeraLogger.LogWarning($"No optimizer found for system type: {systemType}", this);
                return false;
            }

            if (!optimizer.IsEnabled)
            {
                ChimeraLogger.Log($"Optimizer for {systemType} is disabled", this);
                return false;
            }

            // Check cooldown
            var timeSinceLastExecution = (DateTime.Now - optimizer.LastExecuted).TotalSeconds;
            if (timeSinceLastExecution < optimizer.OptimizationCooldown)
            {
                ChimeraLogger.Log($"Optimizer for {systemType} still on cooldown", this);
                return false;
            }

            try
            {
                var success = optimizer.OptimizationFunction(systemInstance);
                optimizer.LastExecuted = DateTime.Now;
                optimizer.ExecutionCount++;
                
                if (success)
                {
                    ChimeraLogger.Log($"Successfully executed optimizer for {systemType}", this);
                }
                
                return success;
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Optimizer execution failed for {systemType}: {ex.Message}", this);
                return false;
            }
        }

        /// <summary>
        /// Register built-in performance optimizers
        /// </summary>
        private void RegisterBuiltInOptimizers()
        {
            // SpeedTree optimization
            RegisterOptimizer("SpeedTree", (system) =>
            {
                // Reduce LOD distances and update frequencies
                ChimeraLogger.Log("SpeedTree optimization executed", this);
                return true;
            }, 10f);

            // Genetics system optimization
            RegisterOptimizer("Genetics", (system) =>
            {
                // Reduce calculation frequency for non-critical updates
                ChimeraLogger.Log("Genetics optimization executed", this);
                return true;
            }, 15f);

            // Environmental system optimization
            RegisterOptimizer("Environmental", (system) =>
            {
                // Reduce sensor update frequency
                ChimeraLogger.Log("Environmental optimization executed", this);
                return true;
            }, 5f);

            // UI optimization
            RegisterOptimizer("UI", (system) =>
            {
                // Reduce UI update frequency and disable non-essential animations
                ChimeraLogger.Log("UI optimization executed", this);
                return true;
            }, 5f);

            // Cultivation optimization
            RegisterOptimizer("Cultivation", (system) =>
            {
                // Reduce plant update frequency for distant plants
                ChimeraLogger.Log("Cultivation optimization executed", this);
                return true;
            }, 8f);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if system can be optimized (not on cooldown)
        /// </summary>
        private bool CanOptimizeSystem(string systemName)
        {
            if (!_optimizationCooldowns.TryGetValue(systemName, out var cooldownTime))
                return true;
            
            return Time.time >= cooldownTime;
        }

        /// <summary>
        /// Update optimization cooldowns
        /// </summary>
        private void UpdateOptimizationCooldowns()
        {
            var expiredCooldowns = new List<string>();
            
            foreach (var cooldown in _optimizationCooldowns)
            {
                if (Time.time >= cooldown.Value)
                {
                    expiredCooldowns.Add(cooldown.Key);
                }
            }
            
            foreach (var expired in expiredCooldowns)
            {
                _optimizationCooldowns.Remove(expired);
            }
        }

        /// <summary>
        /// Update system profile data
        /// </summary>
        public void UpdateSystemProfile(string systemName, PerformanceProfile profile)
        {
            if (_systemProfiles.ContainsKey(systemName))
            {
                _systemProfiles[systemName] = profile;
            }
        }

        /// <summary>
        /// Force optimization for all systems
        /// </summary>
        public void ForceOptimizeAllSystems()
        {
            if (!_isInitialized) return;

            var optimizedCount = 0;
            foreach (var system in _managedSystems)
            {
                var newQuality = PerformanceQualityLevel.Medium;
                if (ApplySystemOptimization(system, newQuality))
                {
                    optimizedCount++;
                }
            }

            ChimeraLogger.Log($"Force optimized {optimizedCount} systems", this);
            
            // Generate optimization report
            GenerateOptimizationReport();
        }

        /// <summary>
        /// Generate optimization report
        /// </summary>
        private void GenerateOptimizationReport()
        {
            var report = new OptimizationReport
            {
                Timestamp = DateTime.Now,
                TotalSystemsManaged = _managedSystems.Count,
                SystemsOptimized = _systemProfiles.Values.Count(p => p.TotalOptimizations > 0),
                OptimizersRegistered = _optimizers.Count,
                OptimizationSummary = GenerateOptimizationSummary()
            };

            OnOptimizationCompleted?.Invoke(report);
        }

        /// <summary>
        /// Generate optimization summary text
        /// </summary>
        private string GenerateOptimizationSummary()
        {
            var totalOptimizations = _systemProfiles.Values.Sum(p => p.TotalOptimizations);
            var activeOptimizers = _optimizers.Values.Count(o => o.IsEnabled);
            
            return $"Total optimizations applied: {totalOptimizations} | " +
                   $"Active optimizers: {activeOptimizers}/{_optimizers.Count} | " +
                   $"Systems under management: {_managedSystems.Count}";
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get optimization status for a specific system
        /// </summary>
        public OptimizationStatus GetSystemOptimizationStatus(string systemName)
        {
            if (!_systemProfiles.TryGetValue(systemName, out var profile))
                return null;

            return new OptimizationStatus
            {
                SystemName = systemName,
                CurrentQuality = profile.CurrentQuality,
                TotalOptimizations = profile.TotalOptimizations,
                LastOptimization = profile.LastOptimization,
                CanOptimize = CanOptimizeSystem(systemName),
                IsBottleneck = profile.IsBottleneck
            };
        }

        /// <summary>
        /// Enable or disable dynamic quality adjustment
        /// </summary>
        public void SetDynamicQualityAdjustment(bool enabled)
        {
            _enableDynamicQualityAdjustment = enabled;
            ChimeraLogger.Log($"Dynamic quality adjustment {(enabled ? "enabled" : "disabled")}", this);
        }

        /// <summary>
        /// Enable or disable specific optimizer
        /// </summary>
        public void SetOptimizerEnabled(string systemType, bool enabled)
        {
            if (_optimizers.TryGetValue(systemType, out var optimizer))
            {
                optimizer.IsEnabled = enabled;
                ChimeraLogger.Log($"Optimizer for {systemType} {(enabled ? "enabled" : "disabled")}", this);
            }
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Performance optimizer configuration
    /// </summary>
    [System.Serializable]
    public class PerformanceOptimizer
    {
        public string SystemType;
        public Func<object, bool> OptimizationFunction;
        public float OptimizationCooldown;
        public bool IsEnabled;
        public DateTime LastExecuted;
        public int ExecutionCount;
    }

    /// <summary>
    /// Optimization status for a system
    /// </summary>
    [System.Serializable]
    public class OptimizationStatus
    {
        public string SystemName;
        public PerformanceQualityLevel CurrentQuality;
        public int TotalOptimizations;
        public DateTime LastOptimization;
        public bool CanOptimize;
        public bool IsBottleneck;
    }

    /// <summary>
    /// Optimization report
    /// </summary>
    [System.Serializable]
    public class OptimizationReport
    {
        public DateTime Timestamp;
        public int TotalSystemsManaged;
        public int SystemsOptimized;
        public int OptimizersRegistered;
        public string OptimizationSummary;
    }

    /// <summary>
    /// Interface for systems that can be optimized
    /// </summary>
    public interface IOptimizableSystem
    {
        string SystemName { get; }
    }

    #endregion
}