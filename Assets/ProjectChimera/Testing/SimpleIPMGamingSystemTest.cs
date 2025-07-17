using UnityEngine;
using ProjectChimera.Systems.IPM;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Simple test for Enhanced IPM Gaming System that avoids complex async/coroutine patterns.
    /// This test verifies basic system functionality without try-catch blocks in coroutines.
    /// </summary>
    public class SimpleIPMGamingSystemTest : MonoBehaviour
    {
        [Header("Simple IPM Gaming Test")]
        [SerializeField] private bool _runTestOnStart = false;
        
        private EnhancedIPMGamingSystem _gamingSystem;
        private int _testsPassed = 0;
        private int _totalTests = 0;
        
        private void Start()
        {
            if (_runTestOnStart)
            {
                RunSimpleTests();
            }
        }
        
        /// <summary>
        /// Run basic tests for the Enhanced IPM Gaming System.
        /// </summary>
        [ContextMenu("Run Simple IPM Gaming Tests")]
        public void RunSimpleTests()
        {
            ChimeraLogger.Log("IPMTest", "=== Running Simple Enhanced IPM Gaming System Tests ===", this);
            
            _testsPassed = 0;
            _totalTests = 0;
            
            TestSystemCreation();
            TestSystemBasicProperties();
            TestSystemHealthCheck();
            
            // Log final results
            float successRate = _totalTests > 0 ? (_testsPassed * 100f) / _totalTests : 0f;
            ChimeraLogger.Log("IPMTest", $"Simple Tests Complete: {_testsPassed}/{_totalTests} passed ({successRate:F1}%)", this);
        }
        
        private void TestSystemCreation()
        {
            ChimeraLogger.Log("IPMTest", "Testing system creation...", this);
            
            // Test 1: Find or create gaming system
            _gamingSystem = FindObjectOfType<EnhancedIPMGamingSystem>();
            if (_gamingSystem == null)
            {
                var gameObject = new GameObject("Enhanced IPM Gaming System");
                _gamingSystem = gameObject.AddComponent<EnhancedIPMGamingSystem>();
            }
            
            LogTestResult("Gaming System Creation", _gamingSystem != null);
        }
        
        private void TestSystemBasicProperties()
        {
            ChimeraLogger.Log("IPMTest", "Testing system basic properties...", this);
            
            if (_gamingSystem != null)
            {
                // Test 2: Manager Name
                string managerName = _gamingSystem.ManagerName;
                LogTestResult("Manager Name", !string.IsNullOrEmpty(managerName));
                
                // Test 3: System Metrics
                var metrics = _gamingSystem.SystemMetrics;
                LogTestResult("System Metrics", metrics != null);
                
                // Test 4: Active Battles Count
                int activeBattles = _gamingSystem.ActiveBattlesCount;
                LogTestResult("Active Battles Count", activeBattles >= 0);
                
                // Test 5: Total Players Count
                int totalPlayers = _gamingSystem.TotalPlayersCount;
                LogTestResult("Total Players Count", totalPlayers >= 0);
            }
            else
            {
                LogTestResult("System Basic Properties", false);
            }
        }
        
        private void TestSystemHealthCheck()
        {
            ChimeraLogger.Log("IPMTest", "Testing system health check...", this);
            
            if (_gamingSystem != null)
            {
                // Test 6: Health Check
                var healthCheck = _gamingSystem.PerformSystemHealthCheck();
                LogTestResult("System Health Check", healthCheck != null);
                
                if (healthCheck != null)
                {
                    // Test 7: Health Status
                    LogTestResult("Health Status Valid", healthCheck.OverallHealth != ProjectChimera.Data.IPM.IPMSystemHealth.Unknown);
                    
                    // Test 8: Component Statuses
                    LogTestResult("Component Statuses", healthCheck.ComponentStatuses != null && healthCheck.ComponentStatuses.Count > 0);
                }
            }
            else
            {
                LogTestResult("System Health Check", false);
            }
        }
        
        private void LogTestResult(string testName, bool passed)
        {
            _totalTests++;
            if (passed)
            {
                _testsPassed++;
                ChimeraLogger.Log("IPMTest", $"✓ {testName}: PASSED", this);
            }
            else
            {
                ChimeraLogger.LogError("IPMTest", $"✗ {testName}: FAILED", this);
            }
        }
    }
}