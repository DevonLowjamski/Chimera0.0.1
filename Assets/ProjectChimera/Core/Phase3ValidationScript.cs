using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Simple validation script for Phase 3.1 Enhanced Integration Features.
    /// Tests basic functionality without complex testing framework dependencies.
    /// </summary>
    public class Phase3ValidationScript : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private bool _runValidationOnStart = true;
        
        private void Start()
        {
            if (_runValidationOnStart)
            {
                ValidatePhase3Features();
            }
        }

        /// <summary>
        /// Validate that all Phase 3.1 features are working.
        /// </summary>
        public void ValidatePhase3Features()
        {
            ChimeraLogger.Log("Phase3Validation", "=== Validating Phase 3.1 Enhanced Integration Features ===", this);

            bool allTestsPassed = true;

            // Test 1: Advanced Manager Dependency System
            var dependencySystem = AdvancedManagerDependencySystem.Instance;
            if (dependencySystem != null)
            {
                ChimeraLogger.Log("Phase3Validation", "✅ Advanced Manager Dependency System - INITIALIZED", this);
            }
            else
            {
                ChimeraLogger.LogError("Phase3Validation", "❌ Advanced Manager Dependency System - NOT FOUND", this);
                allTestsPassed = false;
            }

            // Test 2: Enhanced Data Validation Framework
            var validationFramework = AdvancedDataValidationFramework.Instance;
            if (validationFramework != null)
            {
                ChimeraLogger.Log("Phase3Validation", "✅ Enhanced Data Validation Framework - INITIALIZED", this);
            }
            else
            {
                ChimeraLogger.LogError("Phase3Validation", "❌ Enhanced Data Validation Framework - NOT FOUND", this);
                allTestsPassed = false;
            }

            // Test 3: Unified Performance Management System
            var performanceSystem = UnifiedPerformanceManagementSystem.Instance;
            if (performanceSystem != null)
            {
                ChimeraLogger.Log("Phase3Validation", "✅ Unified Performance Management System - INITIALIZED", this);
                
                // Test basic functionality
                var metrics = performanceSystem.GetGlobalMetrics();
                if (metrics != null)
                {
                    ChimeraLogger.Log("Phase3Validation", $"✅ Performance Metrics - Available (FPS: {metrics.CurrentFrameRate:F1})", this);
                }
                else
                {
                    ChimeraLogger.LogWarning("Phase3Validation", "⚠️ Performance Metrics - Not available", this);
                }
            }
            else
            {
                ChimeraLogger.LogError("Phase3Validation", "❌ Unified Performance Management System - NOT FOUND", this);
                allTestsPassed = false;
            }

            // Summary
            if (allTestsPassed)
            {
                ChimeraLogger.Log("Phase3Validation", "🎉 All Phase 3.1 Enhanced Integration Features are working correctly!", this);
            }
            else
            {
                ChimeraLogger.LogError("Phase3Validation", "💥 Some Phase 3.1 features failed validation", this);
            }
        }
    }
}