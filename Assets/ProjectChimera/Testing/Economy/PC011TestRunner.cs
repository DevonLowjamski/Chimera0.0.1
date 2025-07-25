using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectChimera.Core;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Systems.Cultivation;

namespace ProjectChimera.Testing.Economy
{
    /// <summary>
    /// PC-011 Test Runner - Executes all PC-011 tests in the Unity scene
    /// This component should be added to a GameObject in the test scene
    /// </summary>
    public class PC011TestRunner : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runTestsOnStart = true;
        [SerializeField] private bool _enableDetailedLogging = true;
        [SerializeField] private float _testInterval = 2f;
        
        [Header("Test Results")]
        [SerializeField] private int _testsRun = 0;
        [SerializeField] private int _testsPassed = 0;
        [SerializeField] private int _testsFailed = 0;
        [SerializeField] private bool _allTestsComplete = false;
        
        [Header("Manager References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private CultivationManager _cultivationManager;
        [SerializeField] private TradingManager _tradingManager;
        [SerializeField] private MarketManager _marketManager;
        
        private List<TestResult> _testResults = new List<TestResult>();
        private bool _testsStarted = false;
        
        private void Start()
        {
            if (_runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        [ContextMenu("Run All PC-011 Tests")]
        public void RunAllTestsManually()
        {
            if (!_testsStarted)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        private IEnumerator RunAllTests()
        {
            _testsStarted = true;
            _testResults.Clear();
            _testsRun = 0;
            _testsPassed = 0;
            _testsFailed = 0;
            
            LogTest("=== PC-011 Economy System Integration Tests Starting ===");
            
            // Test 1: Manager Initialization
            yield return StartCoroutine(TestManagerInitialization());
            yield return new WaitForSeconds(_testInterval);
            
            // Test 2: Basic Workflow
            yield return StartCoroutine(TestBasicWorkflow());
            yield return new WaitForSeconds(_testInterval);
            
            // Test 3: Inventory Operations
            yield return StartCoroutine(TestInventoryOperations());
            yield return new WaitForSeconds(_testInterval);
            
            // Test 4: Market Integration
            yield return StartCoroutine(TestMarketIntegration());
            yield return new WaitForSeconds(_testInterval);
            
            // Test 5: Performance Tests
            yield return StartCoroutine(TestPerformance());
            yield return new WaitForSeconds(_testInterval);
            
            // Generate final report
            GenerateFinalReport();
            
            _allTestsComplete = true;
            _testsStarted = false;
            
            LogTest("=== PC-011 Economy System Integration Tests Complete ===");
        }
        
        private IEnumerator TestManagerInitialization()
        {
            LogTest("Testing Manager Initialization...");
            
            bool passed = true;
            string errorMessage = "";
            
            // Find or create GameManager
            if (_gameManager == null)
            {
                _gameManager = FindObjectOfType<GameManager>();
                if (_gameManager == null)
                {
                    var gameManagerObj = new GameObject("GameManager");
                    _gameManager = gameManagerObj.AddComponent<GameManager>();
                }
            }
            
            // Find or create managers
            if (_cultivationManager == null)
            {
                _cultivationManager = FindObjectOfType<CultivationManager>();
                if (_cultivationManager == null)
                {
                    var cultivationObj = new GameObject("CultivationManager");
                    _cultivationManager = cultivationObj.AddComponent<CultivationManager>();
                }
            }
            
            if (_tradingManager == null)
            {
                _tradingManager = FindObjectOfType<TradingManager>();
                if (_tradingManager == null)
                {
                    var tradingObj = new GameObject("TradingManager");
                    _tradingManager = tradingObj.AddComponent<TradingManager>();
                }
            }
            
            if (_marketManager == null)
            {
                _marketManager = FindObjectOfType<MarketManager>();
                if (_marketManager == null)
                {
                    var marketObj = new GameObject("MarketManager");
                    _marketManager = marketObj.AddComponent<MarketManager>();
                }
            }
            
            // Wait for initialization
            yield return new WaitForSeconds(1f);
            
            // Test if managers are properly initialized
            if (_gameManager == null || _cultivationManager == null || _tradingManager == null || _marketManager == null)
            {
                passed = false;
                errorMessage = "One or more managers failed to initialize";
            }
            else
            {
                LogTest("✓ All managers initialized successfully");
            }
            
            RecordTestResult("Manager Initialization", passed, errorMessage);
        }
        
        private IEnumerator TestBasicWorkflow()
        {
            LogTest("Testing Basic Workflow...");
            
            bool passed = true;
            string errorMessage = "";
            
            // Test cultivation manager basic operations
            if (_cultivationManager != null && _cultivationManager.IsInitialized)
            {
                var stats = _cultivationManager.GetCultivationStats();
                LogTest($"✓ Cultivation stats: {stats.active} active, {stats.grown} grown, {stats.harvested} harvested");
            }
            else
            {
                passed = false;
                errorMessage = "CultivationManager not properly initialized";
            }
            
            // Test trading manager basic operations
            if (_tradingManager != null && _tradingManager.IsInitialized)
            {
                var inventory = _tradingManager.PlayerInventory;
                if (inventory != null)
                {
                    LogTest($"✓ Player inventory accessible: {inventory.InventoryItems.Count} items");
                }
                else
                {
                    passed = false;
                    errorMessage = "PlayerInventory not accessible";
                }
            }
            else
            {
                passed = false;
                errorMessage = "TradingManager not properly initialized";
            }
            
            // Test market manager basic operations
            if (_marketManager != null && _marketManager.IsInitialized)
            {
                var conditions = _marketManager.CurrentMarketConditions;
                if (conditions != null)
                {
                    LogTest($"✓ Market conditions accessible");
                }
                else
                {
                    passed = false;
                    errorMessage = "Market conditions not accessible";
                }
            }
            else
            {
                passed = false;
                errorMessage = "MarketManager not properly initialized";
            }
            
            yield return new WaitForSeconds(0.5f);
            
            RecordTestResult("Basic Workflow", passed, errorMessage);
        }
        
        private IEnumerator TestInventoryOperations()
        {
            LogTest("Testing Inventory Operations...");
            
            bool passed = true;
            string errorMessage = "";
            
            if (_tradingManager != null && _tradingManager.PlayerInventory != null)
            {
                var inventory = _tradingManager.PlayerInventory;
                
                // Test inventory statistics
                inventory.UpdateInventoryStatistics();
                LogTest($"✓ Inventory statistics updated");
                
                // Test inventory capacity
                float capacity = inventory.CurrentCapacity;
                float maxCapacity = inventory.MaxCapacity;
                LogTest($"✓ Inventory capacity: {capacity}/{maxCapacity}");
                
                // Test inventory items
                var items = inventory.InventoryItems;
                LogTest($"✓ Inventory items: {items.Count}");
            }
            else
            {
                passed = false;
                errorMessage = "PlayerInventory not available for testing";
            }
            
            yield return new WaitForSeconds(0.5f);
            
            RecordTestResult("Inventory Operations", passed, errorMessage);
        }
        
        private IEnumerator TestMarketIntegration()
        {
            LogTest("Testing Market Integration...");
            
            bool passed = true;
            string errorMessage = "";
            
            if (_marketManager != null)
            {
                var conditions = _marketManager.CurrentMarketConditions;
                LogTest($"✓ Market conditions retrieved");
                
                // Test market reputation
                var reputation = _marketManager.PlayerReputation;
                LogTest($"✓ Player reputation accessible");
            }
            else
            {
                passed = false;
                errorMessage = "MarketManager not available for testing";
            }
            
            yield return new WaitForSeconds(0.5f);
            
            RecordTestResult("Market Integration", passed, errorMessage);
        }
        
        private IEnumerator TestPerformance()
        {
            LogTest("Testing Performance...");
            
            bool passed = true;
            string errorMessage = "";
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test basic operations performance
            for (int i = 0; i < 100; i++)
            {
                if (_tradingManager != null && _tradingManager.PlayerInventory != null)
                {
                    _tradingManager.PlayerInventory.UpdateInventoryStatistics();
                }
                
                if (i % 10 == 0)
                {
                    yield return null; // Allow Unity to process other tasks
                }
            }
            
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            
            LogTest($"✓ Performance test completed in {elapsedMs}ms");
            
            if (elapsedMs > 5000) // 5 seconds threshold
            {
                passed = false;
                errorMessage = $"Performance test took too long: {elapsedMs}ms";
            }
            
            RecordTestResult("Performance", passed, errorMessage);
        }
        
        private void RecordTestResult(string testName, bool passed, string errorMessage = "")
        {
            _testsRun++;
            
            if (passed)
            {
                _testsPassed++;
                LogTest($"✓ {testName}: PASSED");
            }
            else
            {
                _testsFailed++;
                LogTest($"✗ {testName}: FAILED - {errorMessage}");
            }
            
            _testResults.Add(new TestResult
            {
                TestName = testName,
                Passed = passed,
                ErrorMessage = errorMessage,
                Timestamp = System.DateTime.Now
            });
        }
        
        private void GenerateFinalReport()
        {
            LogTest("=== PC-011 TEST FINAL REPORT ===");
            LogTest($"Total Tests: {_testsRun}");
            LogTest($"Passed: {_testsPassed}");
            LogTest($"Failed: {_testsFailed}");
            LogTest($"Success Rate: {(_testsPassed / (float)_testsRun):P1}");
            
            if (_testsFailed > 0)
            {
                LogTest("FAILED TESTS:");
                foreach (var result in _testResults)
                {
                    if (!result.Passed)
                    {
                        LogTest($"  - {result.TestName}: {result.ErrorMessage}");
                    }
                }
            }
            
            LogTest("=== END REPORT ===");
        }
        
        private void LogTest(string message)
        {
            if (_enableDetailedLogging)
            {
                Debug.Log($"[PC011TestRunner] {message}");
            }
        }
        
        [System.Serializable]
        private class TestResult
        {
            public string TestName;
            public bool Passed;
            public string ErrorMessage;
            public System.DateTime Timestamp;
        }
        
        #region Unity Inspector Methods
        
        [ContextMenu("Clear Test Results")]
        public void ClearTestResults()
        {
            _testResults.Clear();
            _testsRun = 0;
            _testsPassed = 0;
            _testsFailed = 0;
            _allTestsComplete = false;
            LogTest("Test results cleared");
        }
        
        [ContextMenu("Show Test Summary")]
        public void ShowTestSummary()
        {
            if (_testResults.Count > 0)
            {
                GenerateFinalReport();
            }
            else
            {
                LogTest("No test results available. Run tests first.");
            }
        }
        
        #endregion
    }
}