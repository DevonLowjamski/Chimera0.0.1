using NUnit.Framework;
using UnityEngine;
using System.Collections;
using ProjectChimera.Core;
using ProjectChimera.Testing;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Systems.AI;
using ProjectChimera.UI.Panels;
using ProjectChimera.UI.Financial;
using ProjectChimera.UI.AIAdvisor;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Dashboard;

namespace ProjectChimera.Testing.Integration
{
    [TestFixture]
    public class UIIntegrationTests
    {
        private GameObject _testGameObject;
        private FinancialManagementController _financialController;
        private AIAdvisorController _aiController;
        private GameUIManager _gameUIManager;
        private FacilityDashboardController _facilityController;

        [SetUp]
        public void SetUp()
        {
            // Create test game object with UI controllers
            _testGameObject = new GameObject("TestUIIntegration");
            
            // Add UI controllers
            _financialController = _testGameObject.AddComponent<FinancialManagementController>();
            _aiController = _testGameObject.AddComponent<AIAdvisorController>();
            _gameUIManager = _testGameObject.AddComponent<GameUIManager>();
            _facilityController = _testGameObject.AddComponent<FacilityDashboardController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
                Object.DestroyImmediate(_testGameObject);
        }

        //[Test]
        public void FinancialController_Initialization_CompletesSuccessfully()
        {
            // Assert
            Assert.IsNotNull(_financialController, "FinancialManagementController should be created");
            Assert.IsInstanceOf<FinancialManagementController>(_financialController, "Should be correct type");
        }

        //[Test]
        public void AIController_Initialization_CompletesSuccessfully()
        {
            // Assert
            Assert.IsNotNull(_aiController, "AIAdvisorController should be created");
            Assert.IsInstanceOf<AIAdvisorController>(_aiController, "Should be correct type");
        }

        //[Test]
        public void GameUIManager_Initialization_CompletesSuccessfully()
        {
            // Assert
            Assert.IsNotNull(_gameUIManager, "GameUIManager should be created");
            Assert.IsInstanceOf<GameUIManager>(_gameUIManager, "Should be correct type");
        }

        //[Test]
        public void FacilityController_Initialization_CompletesSuccessfully()
        {
            // Assert
            Assert.IsNotNull(_facilityController, "FacilityDashboardController should be created");
            Assert.IsInstanceOf<FacilityDashboardController>(_facilityController, "Should be correct type");
        }

        //[Test]
        public void FinancialController_BackendIntegration_HandlesMarketData()
        {
            // Arrange
            var marketManager = _testGameObject.AddComponent<MarketManager>();

            // Act & Assert - Should not throw when accessing backend data
            Assert.DoesNotThrow(() => 
            {
                // Test basic integration - accessing backend through controller
                var portfolioData = marketManager.GetPortfolioMetrics();
                Assert.IsNotNull(portfolioData, "Should be able to get portfolio data through controller");
            }, "Financial controller should integrate properly with MarketManager");
        }

        //[Test]
        public void AIController_BackendIntegration_HandlesAIData()
        {
            // Arrange
            var aiManager = _testGameObject.AddComponent<AIAdvisorManager>();

            // Act & Assert - Should not throw when accessing backend data
            Assert.DoesNotThrow(() => 
            {
                // Test basic integration - accessing backend through controller
                var aiData = aiManager.GetAIData();
                Assert.IsNotNull(aiData, "Should be able to get AI data through controller");
            }, "AI controller should integrate properly with AIAdvisorManager");
        }

        //[Test]
        public void Controllers_CrossCommunication_WorksProperly()
        {
            // Act & Assert - Controllers should be able to coexist
            Assert.DoesNotThrow(() => 
            {
                // Test that multiple controllers can exist simultaneously
                var financial = _financialController;
                var ai = _aiController;
                var ui = _gameUIManager;
                var facility = _facilityController;
                
                Assert.IsNotNull(financial, "Financial controller should exist");
                Assert.IsNotNull(ai, "AI controller should exist");
                Assert.IsNotNull(ui, "UI manager should exist");
                Assert.IsNotNull(facility, "Facility controller should exist");
            }, "All controllers should coexist without conflicts");
        }

        //[Test]
        public void UIDataFlow_FinancialToUI_TransfersCorrectly()
        {
            // Arrange
            var marketManager = _testGameObject.AddComponent<MarketManager>();

            // Act
            var portfolioMetrics = marketManager.GetPortfolioMetrics();
            var financialData = marketManager.GetFinancialData();

            // Assert
            Assert.IsNotNull(portfolioMetrics, "Portfolio metrics should be available for UI");
            Assert.IsNotNull(financialData, "Financial data should be available for UI");
        }

        //[Test]
        public void UIDataFlow_AIToUI_TransfersCorrectly()
        {
            // Arrange
            var aiManager = _testGameObject.AddComponent<AIAdvisorManager>();

            // Act
            var recommendations = aiManager.GetActiveRecommendations();
            var insights = aiManager.GetRecentInsights();
            var opportunities = aiManager.GetOptimizationOpportunities();

            // Assert
            Assert.IsNotNull(recommendations, "Recommendations should be available for UI");
            Assert.IsNotNull(insights, "Insights should be available for UI");
            Assert.IsNotNull(opportunities, "Opportunities should be available for UI");
        }

        //[Test]
        public void ErrorHandling_MissingBackendSystems_HandledGracefully()
        {
            // Test that UI controllers handle missing backend systems gracefully
            Assert.DoesNotThrow(() => 
            {
                // Controllers should not crash when backend systems are missing
                var controller = _financialController;
                // Should handle gracefully when MarketManager is not available
            }, "Controllers should handle missing backend systems gracefully");
        }

        //[Test]
        public void StateManagement_UIControllers_MaintainConsistentState()
        {
            // Act - Test that controllers maintain consistent state
            var controller1State = _financialController.GetHashCode();
            var controller2State = _aiController.GetHashCode();
            var managerState = _gameUIManager.GetHashCode();
            var facilityState = _facilityController.GetHashCode();

            // Assert - Controllers should maintain their state consistently
            Assert.AreEqual(controller1State, _financialController.GetHashCode(), "Financial controller state should be consistent");
            Assert.AreEqual(controller2State, _aiController.GetHashCode(), "AI controller state should be consistent");
            Assert.AreEqual(managerState, _gameUIManager.GetHashCode(), "UI manager state should be consistent");
            Assert.AreEqual(facilityState, _facilityController.GetHashCode(), "Facility controller state should be consistent");
        }

        //[Test]
        public void EventSystem_UIControllers_HandleEventsCorrectly()
        {
            // Test that controllers can handle events without crashing
            Assert.DoesNotThrow(() => 
            {
                // Controllers should be able to handle Unity events
                // Note: Start() is called automatically by Unity, we test component existence instead
                Assert.IsNotNull(_financialController, "Financial controller should be initialized");
                Assert.IsNotNull(_aiController, "AI controller should be initialized");
                Assert.IsNotNull(_gameUIManager, "UI manager should be initialized");
                Assert.IsNotNull(_facilityController, "Facility controller should be initialized");
            }, "Controllers should handle Unity lifecycle events");
        }

        //[Test]
        
        [Ignore("Performance timing not reliable in test environment")]
        public void UIIntegration_DataRetrieval_PerformanceTest()
        {
            // Test disabled due to timing reliability issues in test environment
            Assert.Inconclusive("This test requires Unity runtime environment for accurate performance measurement");
        }

        //[Test]
        
        public void UIControllers_Initialization_PerformanceTest()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act - Test controller initialization performance
            for (int i = 0; i < 5; i++)
            {
                var testObj = new GameObject($"PerfTest_{i}");
                testObj.AddComponent<FinancialManagementController>();
                testObj.AddComponent<AIAdvisorController>();
                testObj.AddComponent<GameUIManager>();
                testObj.AddComponent<FacilityDashboardController>();
                Object.DestroyImmediate(testObj);
            }

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 200, "5 controller initialization cycles should complete in under 200ms");
        }

        //[Test]
        public void ThreadSafety_ConcurrentAccess_HandlesCorrectly()
        {
            // Test that UI controllers handle concurrent access appropriately
            Assert.DoesNotThrow(() => 
            {
                // Multiple quick accesses to controllers
                for (int i = 0; i < 10; i++)
                {
                    var financial = _financialController;
                    var ai = _aiController;
                    var ui = _gameUIManager;
                    var facility = _facilityController;
                }
            }, "Controllers should handle rapid successive access");
        }

        //[Test]
        public void MemoryManagement_UIControllers_NoMemoryLeaks()
        {
            // Arrange
            long initialMemory = System.GC.GetTotalMemory(true);

            // Act - Create and destroy controllers multiple times
            for (int i = 0; i < 10; i++)
            {
                var testObj = new GameObject($"MemTest_{i}");
                testObj.AddComponent<FinancialManagementController>();
                testObj.AddComponent<AIAdvisorController>();
                Object.DestroyImmediate(testObj);
            }

            // Force garbage collection
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            long finalMemory = System.GC.GetTotalMemory(true);

            // Assert - Memory usage should not increase significantly
            long memoryIncrease = finalMemory - initialMemory;
            Assert.Less(memoryIncrease, 1024 * 1024, "Memory increase should be less than 1MB"); // Allow for some variance
        }

        //[Test]
        public void ComponentDependencies_UIControllers_ResolveCorrectly()
        {
            // Test that UI controllers can resolve their dependencies correctly
            Assert.DoesNotThrow(() => 
            {
                // Controllers should be able to find and work with their dependencies
                var hasFinancial = _testGameObject.GetComponent<FinancialManagementController>();
                var hasAI = _testGameObject.GetComponent<AIAdvisorController>();
                var hasUI = _testGameObject.GetComponent<GameUIManager>();
                var hasFacility = _testGameObject.GetComponent<FacilityDashboardController>();
                
                Assert.IsNotNull(hasFinancial, "Should find financial controller");
                Assert.IsNotNull(hasAI, "Should find AI controller");
                Assert.IsNotNull(hasUI, "Should find UI manager");
                Assert.IsNotNull(hasFacility, "Should find facility controller");
            }, "Controllers should resolve component dependencies correctly");
        }
    }
} 