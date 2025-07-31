using System;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Interface for Gaming Performance Monitor - Real-time 60fps gaming performance management
    /// Part of Module 1: Gaming Experience Core - Deliverable 2.1
    /// 
    /// Provides comprehensive performance monitoring specifically designed for gaming environments,
    /// ensuring smooth 60fps gameplay through real-time monitoring and adaptive optimizations.
    /// </summary>
    public interface IGamingPerformanceMonitor : IGamingService
    {
        #region Performance Targets
        
        /// <summary>
        /// Target frame rate for optimal gaming experience (default: 60fps)
        /// </summary>
        float TargetFrameRate { get; set; }
        
        /// <summary>
        /// Maximum acceptable frame time in milliseconds (16.67ms for 60fps)
        /// </summary>
        float MaxAcceptableFrameTime { get; set; }
        
        /// <summary>
        /// Critical performance threshold - triggers immediate optimization
        /// </summary>
        float CriticalPerformanceThreshold { get; set; }
        
        #endregion
        
        #region Real-Time Metrics
        
        /// <summary>
        /// Current frames per second
        /// </summary>
        float CurrentFPS { get; }
        
        /// <summary>
        /// Current frame time in milliseconds
        /// </summary>
        float CurrentFrameTime { get; }
        
        /// <summary>
        /// Average FPS over the last second
        /// </summary>
        float AverageFPS { get; }
        
        /// <summary>
        /// Minimum FPS recorded in the current monitoring window
        /// </summary>
        float MinFPS { get; }
        
        /// <summary>
        /// Maximum FPS recorded in the current monitoring window
        /// </summary>
        float MaxFPS { get; }
        
        /// <summary>
        /// Current memory usage in MB
        /// </summary>
        float CurrentMemoryUsage { get; }
        
        /// <summary>
        /// GPU memory usage in MB (if available)
        /// </summary>
        float GPUMemoryUsage { get; }
        
        #endregion
        
        #region Performance States
        
        /// <summary>
        /// Current overall performance state
        /// </summary>
        GamingPerformanceState CurrentPerformanceState { get; }
        
        /// <summary>
        /// Is the system currently performing optimally for gaming?
        /// </summary>
        bool IsPerformingOptimally { get; }
        
        /// <summary>
        /// Is the system under performance stress?
        /// </summary>
        bool IsUnderPerformanceStress { get; }
        
        /// <summary>
        /// Are we hitting target frame rate consistently?
        /// </summary>
        bool IsHittingTargetFrameRate { get; }
        
        #endregion
        
        #region Performance Events
        
        /// <summary>
        /// Event fired when performance drops below acceptable levels
        /// </summary>
        event Action<GamingPerformanceData> OnPerformanceDegraded;
        
        /// <summary>
        /// Event fired when performance recovers to acceptable levels
        /// </summary>
        event Action<GamingPerformanceData> OnPerformanceRecovered;
        
        /// <summary>
        /// Event fired when critical performance threshold is reached
        /// </summary>
        event Action<GamingPerformanceData> OnCriticalPerformance;
        
        /// <summary>
        /// Event fired every second with updated performance metrics
        /// </summary>
        event Action<GamingPerformanceData> OnPerformanceUpdate;
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// Enable/disable automatic performance optimization
        /// </summary>
        bool EnableAutomaticOptimization { get; set; }
        
        /// <summary>
        /// Performance monitoring update frequency (updates per second)
        /// </summary>
        float MonitoringFrequency { get; set; }
        
        /// <summary>
        /// Number of frames to average for FPS calculations
        /// </summary>
        int FrameAveragingWindow { get; set; }
        
        /// <summary>
        /// Enable detailed performance profiling (may impact performance)
        /// </summary>
        bool EnableDetailedProfiling { get; set; }
        
        #endregion
        
        #region Performance Data
        
        /// <summary>
        /// Get comprehensive performance data snapshot
        /// </summary>
        GamingPerformanceData GetCurrentPerformanceData();
        
        /// <summary>
        /// Get historical performance data for the last N seconds
        /// </summary>
        GamingPerformanceHistory GetPerformanceHistory(float durationSeconds = 60f);
        
        /// <summary>
        /// Get performance statistics for the current session
        /// </summary>
        GamingPerformanceStatistics GetSessionStatistics();
        
        #endregion
        
        #region Manual Controls
        
        /// <summary>
        /// Force a performance check and optimization pass
        /// </summary>
        void ForcePerformanceCheck();
        
        /// <summary>
        /// Reset performance statistics and history
        /// </summary>
        void ResetStatistics();
        
        /// <summary>
        /// Start intensive performance monitoring for debugging
        /// </summary>
        void StartIntensiveMonitoring();
        
        /// <summary>
        /// Stop intensive performance monitoring
        /// </summary>
        void StopIntensiveMonitoring();
        
        #endregion
    }
    
    /// <summary>
    /// Gaming performance states
    /// </summary>
    public enum GamingPerformanceState
    {
        Optimal,        // 60+ FPS, smooth gameplay
        Good,           // 45-60 FPS, acceptable gameplay
        Degraded,       // 30-45 FPS, noticeable impact
        Poor,           // 15-30 FPS, significant impact
        Critical        // <15 FPS, unplayable
    }
    
    /// <summary>
    /// Comprehensive gaming performance data snapshot
    /// </summary>
    [Serializable]
    public struct GamingPerformanceData
    {
        [Header("Frame Rate Metrics")]
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float CurrentFrameTime;
        public float AverageFrameTime;
        
        [Header("Memory Metrics")]
        public float TotalMemoryUsage;
        public float MonoMemoryUsage;
        public float GPUMemoryUsage;
        public float TempAllocatorSize;
        
        [Header("Performance State")]
        public GamingPerformanceState PerformanceState;
        public float PerformanceScore;
        public bool IsStable;
        public bool NeedsOptimization;
        
        [Header("Timing")]
        public DateTime Timestamp;
        public float SessionTime;
        
        [Header("System Info")]
        public float CPUUsage;
        public float GPUUsage;
        public int ActiveGameObjects;
        public int DrawCalls;
        public int SetPassCalls;
        public int Triangles;
        public int Vertices;
        
        public override string ToString()
        {
            return $"FPS: {CurrentFPS:F1} ({PerformanceState}) | Frame: {CurrentFrameTime:F2}ms | Mem: {TotalMemoryUsage:F0}MB | Score: {PerformanceScore:F2}";
        }
    }
    
    /// <summary>
    /// Historical performance data collection
    /// </summary>
    [Serializable]
    public class GamingPerformanceHistory
    {
        public GamingPerformanceData[] DataPoints;
        public float DurationSeconds;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float FPSStandardDeviation;
        public int PerformanceDropCount;
        public float TotalTimeInOptimalState;
        public float TotalTimeInDegradedState;
        
        public override string ToString()
        {
            return $"History ({DurationSeconds}s): Avg {AverageFPS:F1} FPS, Range {MinFPS:F1}-{MaxFPS:F1}, Drops: {PerformanceDropCount}";
        }
    }
    
    /// <summary>
    /// Session-wide performance statistics
    /// </summary>
    [Serializable]
    public class GamingPerformanceStatistics
    {
        [Header("Session Overview")]
        public DateTime SessionStartTime;
        public float TotalSessionTime;
        public float TotalFrames;
        
        [Header("FPS Statistics")]
        public float SessionAverageFPS;
        public float SessionMinFPS;
        public float SessionMaxFPS;
        public float SessionFPSStandardDeviation;
        
        [Header("Performance Distribution")]
        public float TimeInOptimalState;     // Percentage
        public float TimeInGoodState;        // Percentage
        public float TimeInDegradedState;    // Percentage
        public float TimeInPoorState;        // Percentage
        public float TimeInCriticalState;    // Percentage
        
        [Header("Performance Events")]
        public int TotalPerformanceDrops;
        public int CriticalPerformanceEvents;
        public int AutoOptimizationTriggered;
        
        [Header("Memory Statistics")]
        public float PeakMemoryUsage;
        public float AverageMemoryUsage;
        public int MemoryGCCollections;
        
        public override string ToString()
        {
            return $"Session: {TotalSessionTime:F0}s, Avg FPS: {SessionAverageFPS:F1}, Optimal: {TimeInOptimalState:F1}%, Drops: {TotalPerformanceDrops}";
        }
    }
}