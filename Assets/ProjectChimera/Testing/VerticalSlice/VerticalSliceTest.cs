using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System;
using System.Linq;

namespace ProjectChimera.Testing.VerticalSlice
{
    /// <summary>
    /// PC-009: Vertical Slice Implementation Test
    /// Tests the complete Plant → Harvest → Sell → Progress gameplay loop
    /// This validates the core integration between Cultivation, Economy, and Progression systems.
    /// </summary>
    public class VerticalSliceTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private PlantStrainSO _testStrain;
        [SerializeField] private Vector3 _testPosition = Vector3.zero;
        [SerializeField] private bool _runTestOnStart = false;
        [SerializeField] private bool _enableDetailedLogging = true;
        
        [Header("Test Results")]
        [SerializeField] private string _lastTestResult = "Not Run";
        [SerializeField] private float _testExecutionTime = 0f;
        
        // System References
        private CultivationManager _cultivationManager;
        private TradingManager _tradingManager;
        
        // Test Data
        private string _testPlantId;
        private PlantInstanceSO _testPlant;
        
        private void Start()
        {
            if (_runTestOnStart)
            {
                StartVerticalSliceTest();
            }
        }
        
        /// <summary>
        /// Initiates the complete vertical slice test
        /// </summary>
        [ContextMenu("Run Vertical Slice Test")]
        public void StartVerticalSliceTest()
        {
            float startTime = Time.time;
            LogTest("=== STARTING PC-009 VERTICAL SLICE TEST ===");
            
            try
            {
                // Step 1: Validate Prerequisites
                if (!ValidatePrerequisites())
                {
                    _lastTestResult = "FAILED: Prerequisites not met";
                    return;
                }
                
                // Step 2: Test Single Seed Planting
                if (!TestSeedPlanting())
                {
                    _lastTestResult = "FAILED: Seed planting failed";
                    return;
                }
                
                LogTest("✅ PHASE 2 TASK PC-009-1: Single seed planting via CultivationManager - SUCCESSFUL");
                
                // Step 3: Test PlayerInventory Integration (PC-009-2)
                if (!TestInventoryIntegration())
                {
                    _lastTestResult = "FAILED: Inventory integration failed";
                    return;
                }
                
                LogTest("✅ PHASE 2 TASK PC-009-2: PlayerInventory system integration - SUCCESSFUL");
                
                // Step 4: Test MarketManager Integration (placeholder for PC-009-3)
                LogTest("🔄 PC-009-3: Market sales integration - PENDING (Next Phase 2 task)");
                
                // Step 5: Test ProgressionManager Integration (placeholder for PC-009-4)
                LogTest("🔄 PC-009-4: Progression XP rewards - PENDING (Next Phase 2 task)");
                
                _testExecutionTime = Time.time - startTime;
                _lastTestResult = $"PARTIAL SUCCESS: Step 2/4 complete in {_testExecutionTime:F2}s";
                
                LogTest($"=== PC-009 VERTICAL SLICE TEST RESULT: {_lastTestResult} ===");
            }
            catch (System.Exception ex)
            {
                _testExecutionTime = Time.time - startTime;
                _lastTestResult = $"ERROR: {ex.Message}";
                LogTest($"❌ VERTICAL SLICE TEST ERROR: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validates that all required systems and data are available
        /// </summary>
        private bool ValidatePrerequisites()
        {
            LogTest("Validating prerequisites...");
            
            // Check CultivationManager
            _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            if (_cultivationManager == null)
            {
                LogTest("❌ CultivationManager not found - ensure GameManager is initialized");
                return false;
            }
            LogTest("✅ CultivationManager found");
            
            // Check test strain
            if (_testStrain == null)
            {
                LogTest("❌ Test strain not assigned - please assign a PlantStrainSO in inspector");
                return false;
            }
            LogTest($"✅ Test strain assigned: {_testStrain.DisplayName}");
            
            LogTest("✅ All prerequisites validated");
            return true;
        }
        
        /// <summary>
        /// Tests the single seed planting functionality (PC-009-1)
        /// </summary>
        private bool TestSeedPlanting()
        {
            LogTest("Testing single seed planting...");
            
            try
            {
                // Generate unique test plant name
                string testPlantName = $"VerticalSlice_Plant_{System.DateTime.Now:HHmmss}";
                
                // Attempt to plant seed using CultivationManager
                _testPlant = _cultivationManager.PlantSeed(
                    plantName: testPlantName,
                    strain: _testStrain,
                    genotype: null, // Using default genetics for now
                    position: _testPosition,
                    zoneId: "test_zone"
                );
                
                if (_testPlant == null)
                {
                    LogTest("❌ PlantSeed() returned null - planting failed");
                    return false;
                }
                
                // Store plant ID for future operations
                _testPlantId = _testPlant.PlantID;
                
                // Validate plant was properly created
                if (string.IsNullOrEmpty(_testPlantId))
                {
                    LogTest("❌ Plant created but has no ID");
                    return false;
                }
                
                // Verify plant can be retrieved from cultivation system
                var retrievedPlant = _cultivationManager.GetPlant(_testPlantId);
                if (retrievedPlant == null)
                {
                    LogTest($"❌ Plant {_testPlantId} not found in cultivation system");
                    return false;
                }
                
                LogTest($"✅ Successfully planted seed '{testPlantName}' with ID: {_testPlantId}");
                LogTest($"   - Strain: {_testStrain.DisplayName}");
                LogTest($"   - Position: {_testPosition}");
                LogTest($"   - Zone: test_zone");
                LogTest($"   - Growth Stage: {_testPlant.CurrentGrowthStage}");
                LogTest($"   - Health: {_testPlant.OverallHealth:F1}%");
                
                return true;
            }
            catch (System.Exception ex)
            {
                LogTest($"❌ Exception during seed planting: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// PC-009-2: Test PlayerInventory integration with harvested products
        /// </summary>
        private bool TestInventoryIntegration()
        {
            try
            {
                LogTest("Testing PlayerInventory integration...");
                
                // Get TradingManager for inventory access
                var tradingManager = FindObjectOfType<ProjectChimera.Systems.Economy.TradingManager>();
                if (tradingManager?.PlayerInventory == null)
                {
                    LogTest("❌ TradingManager or PlayerInventory not available");
                    return false;
                }
                
                var inventory = tradingManager.PlayerInventory;
                var initialCount = inventory.InventoryItems.Count;
                var initialCapacity = inventory.CurrentCapacity;
                
                // Test adding a harvested product
                float testQuantity = 50f;
                float testQuality = 0.85f;
                string testBatchId = "TEST_BATCH_001";
                
                bool addResult = inventory.AddHarvestedProduct(null, testQuantity, testQuality, testBatchId);
                if (!addResult)
                {
                    LogTest("❌ Failed to add harvested product to inventory");
                    return false;
                }
                
                // Verify inventory was updated
                if (inventory.InventoryItems.Count != initialCount + 1)
                {
                    LogTest($"❌ Inventory count mismatch. Expected: {initialCount + 1}, Actual: {inventory.InventoryItems.Count}");
                    return false;
                }
                
                if (Mathf.Abs(inventory.CurrentCapacity - (initialCapacity + testQuantity)) > 0.01f)
                {
                    LogTest($"❌ Capacity mismatch. Expected: {initialCapacity + testQuantity:F1}, Actual: {inventory.CurrentCapacity:F1}");
                    return false;
                }
                
                // Test inventory statistics update
                inventory.UpdateInventoryStatistics();
                
                var addedItem = inventory.InventoryItems.LastOrDefault();
                if (addedItem == null || addedItem.BatchId != testBatchId)
                {
                    LogTest("❌ Added item not found or batch ID mismatch");
                    return false;
                }
                
                LogTest($"✅ Successfully added {testQuantity}g to inventory (Quality: {testQuality:P1})");
                LogTest($"   - Batch ID: {testBatchId}");
                LogTest($"   - Total Items: {inventory.InventoryItems.Count}");
                LogTest($"   - Total Capacity: {inventory.CurrentCapacity:F1}g");
                
                return true;
            }
            catch (System.Exception ex)
            {
                LogTest($"❌ Exception during inventory integration test: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Utility method for consistent test logging
        /// </summary>
        private void LogTest(string message)
        {
            if (_enableDetailedLogging)
            {
                Debug.Log($"[VerticalSliceTest] {message}");
            }
        }
        
        /// <summary>
        /// Cleans up test data when component is destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Clean up test plant if it exists
            if (!string.IsNullOrEmpty(_testPlantId) && _cultivationManager != null)
            {
                _cultivationManager.RemovePlant(_testPlantId, false);
                LogTest($"Cleaned up test plant: {_testPlantId}");
            }
        }
        
        /// <summary>
        /// Manual cleanup method for inspector use
        /// </summary>
        [ContextMenu("Clean Up Test Plant")]
        public void CleanUpTestPlant()
        {
            if (!string.IsNullOrEmpty(_testPlantId) && _cultivationManager != null)
            {
                bool removed = _cultivationManager.RemovePlant(_testPlantId, false);
                if (removed)
                {
                    LogTest($"✅ Manually cleaned up test plant: {_testPlantId}");
                    _testPlantId = null;
                    _testPlant = null;
                }
                else
                {
                    LogTest($"❌ Failed to remove test plant: {_testPlantId}");
                }
            }
            else
            {
                LogTest("No test plant to clean up");
            }
        }
    }
}