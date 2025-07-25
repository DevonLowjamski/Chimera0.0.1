using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Systems.AI;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Testing.AI
{
    [TestFixture]
    public class AIAdvisorManagerTests
    {
        private GameObject _testGameObject;
        private AIAdvisorManager _aiAdvisorManager;

        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("TestAIAdvisorManager");
            _aiAdvisorManager = _testGameObject.AddComponent<AIAdvisorManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
                Object.DestroyImmediate(_testGameObject);
        }

        //[Test]
        public void AIAdvisorManager_Initialization_SetsUpCorrectly()
        {
            // Assert
            Assert.IsNotNull(_aiAdvisorManager, "AIAdvisorManager should be created");
            Assert.IsInstanceOf<AIAdvisorManager>(_aiAdvisorManager, "Should be AIAdvisorManager type");
        }

        //[Test]
        public void GetActiveRecommendations_ReturnsValidList()
        {
            // Act
            var recommendations = _aiAdvisorManager.GetActiveRecommendations();

            // Assert
            Assert.IsNotNull(recommendations, "Active recommendations should not be null");
            Assert.IsInstanceOf<List<AIRecommendation>>(recommendations, "Should return List<AIRecommendation>");
        }

        //[Test]
        public void GetOptimizationOpportunities_ReturnsValidList()
        {
            // Act
            var opportunities = _aiAdvisorManager.GetOptimizationOpportunities();

            // Assert
            Assert.IsNotNull(opportunities, "Optimization opportunities should not be null");
            Assert.IsInstanceOf<List<OptimizationOpportunity>>(opportunities, "Should return List<OptimizationOpportunity>");
        }

        //[Test]
        public void GetRecentInsights_ReturnsValidList()
        {
            // Arrange
            int requestedCount = 5;

            // Act
            var insights = _aiAdvisorManager.GetRecentInsights(requestedCount);

            // Assert
            Assert.IsNotNull(insights, "Recent insights should not be null");
            Assert.IsInstanceOf<List<DataInsight>>(insights, "Should return List<DataInsight>");
            Assert.LessOrEqual(insights.Count, requestedCount, "Should not return more than requested count");
        }

        //[Test]
        public void GeneratePerformanceReport_ReturnsValidReport()
        {
            // Act
            var report = _aiAdvisorManager.GeneratePerformanceReport();

            // Assert
            Assert.IsNotNull(report, "Performance report should not be null");
            Assert.IsInstanceOf<AIPerformanceReport>(report, "Should return AIPerformanceReport type");
        }

        //[Test]
        public void ProcessUserQuery_WithValidQuery_ExecutesCallback()
        {
            // Arrange
            string testQuery = "What is the optimal temperature for flowering?";
            string receivedResponse = null;
            bool callbackExecuted = false;

            // Act
            _aiAdvisorManager.ProcessUserQuery(testQuery, response => 
            {
                receivedResponse = response;
                callbackExecuted = true;
            });

            // Wait briefly for async operation
            var timeout = Time.time + 2f;
            while (!callbackExecuted && Time.time < timeout)
            {
                // Wait for callback
            }

            // Assert
            Assert.IsTrue(callbackExecuted, "Callback should have been executed");
            Assert.IsNotNull(receivedResponse, "Response should not be null");
            Assert.IsNotEmpty(receivedResponse, "Response should not be empty");
        }

        //[Test]
        public void ProcessUserQuery_WithEmptyQuery_HandlesGracefully()
        {
            // Arrange
            string emptyQuery = "";
            bool callbackExecuted = false;

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                _aiAdvisorManager.ProcessUserQuery(emptyQuery, response => 
                {
                    callbackExecuted = true;
                });
            }, "Should handle empty query gracefully");
        }

        //[Test]
        public void ProcessUserQuery_WithNullCallback_HandlesGracefully()
        {
            // Arrange
            string testQuery = "Test query";

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                _aiAdvisorManager.ProcessUserQuery(testQuery, null);
            }, "Should handle null callback gracefully");
        }

        //[Test]
        public void AnalyzeFacilityState_ReturnsValidData()
        {
            // Act
            var facilityState = _aiAdvisorManager.AnalyzeFacilityState();

            // Assert
            Assert.IsNotNull(facilityState, "Facility state should not be null");
            // The method returns object, so we verify it's not null
        }

        //[Test]
        public void GeneratePredictions_ReturnsValidData()
        {
            // Act
            var predictions = _aiAdvisorManager.GeneratePredictions();

            // Assert
            Assert.IsNotNull(predictions, "Predictions should not be null");
            // The method returns object, so we verify it's not null
        }

        //[Test]
        public void GetAIData_ReturnsValidData()
        {
            // Act
            var aiData = _aiAdvisorManager.GetAIData();

            // Assert
            Assert.IsNotNull(aiData, "AI data should not be null");
            // The method returns object, so we verify it's not null
        }

        //[Test]
        public void ActiveRecommendations_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                int count = _aiAdvisorManager.ActiveRecommendations;
                Assert.GreaterOrEqual(count, 0, "Active recommendations count should be non-negative");
            }, "ActiveRecommendations property should be accessible");
        }

        //[Test]
        public void CriticalInsights_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                int count = _aiAdvisorManager.CriticalInsights;
                Assert.GreaterOrEqual(count, 0, "Critical insights count should be non-negative");
            }, "CriticalInsights property should be accessible");
        }

        //[Test]
        public void OptimizationOpportunities_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                int count = _aiAdvisorManager.OptimizationOpportunities;
                Assert.GreaterOrEqual(count, 0, "Optimization opportunities count should be non-negative");
            }, "OptimizationOpportunities property should be accessible");
        }

        //[Test]
        public void SystemEfficiencyScore_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                float score = _aiAdvisorManager.SystemEfficiencyScore;
                Assert.GreaterOrEqual(score, 0f, "System efficiency score should be non-negative");
                Assert.LessOrEqual(score, 1f, "System efficiency score should not exceed 1");
            }, "SystemEfficiencyScore property should be accessible");
        }

        //[Test]
        public void GetRecommendationsByCategory_WithValidCategory_ReturnsFilteredList()
        {
            // Arrange
            string category = "Environmental";

            // Act
            var recommendations = _aiAdvisorManager.GetRecommendationsByCategory(category);

            // Assert
            Assert.IsNotNull(recommendations, "Filtered recommendations should not be null");
            Assert.IsInstanceOf<List<AIRecommendation>>(recommendations, "Should return List<AIRecommendation>");
        }

        //[Test]
        public void ImplementRecommendation_WithValidId_ExecutesSuccessfully()
        {
            // Arrange
            string testRecommendationId = "test_recommendation_id";

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                bool result = _aiAdvisorManager.ImplementRecommendation(testRecommendationId);
                // Result may be false if recommendation doesn't exist, but should not throw
            }, "ImplementRecommendation should execute without error");
        }

        //[Test]
        public void DismissRecommendation_WithValidId_ExecutesSuccessfully()
        {
            // Arrange
            string testRecommendationId = "test_recommendation_id";
            string dismissalReason = "User preference";

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                bool result = _aiAdvisorManager.DismissRecommendation(testRecommendationId, dismissalReason);
                // Result may be false if recommendation doesn't exist, but should not throw
            }, "DismissRecommendation should execute without error");
        }

        //[Test]
        
        [Ignore("Coroutine execution not supported in test environment")]
        public void ProcessUserQuery_PerformanceTest()
        {
            // Test disabled due to coroutine execution issues in test environment
            Assert.Inconclusive("This test requires Unity runtime environment to execute properly");
        }

        //[Test]
        
        public void AnalyzeFacilityState_PerformanceTest()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                _aiAdvisorManager.AnalyzeFacilityState();
            }

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "100 AnalyzeFacilityState calls should complete in under 1 second");
        }

        //[Test]
        
        public void GeneratePredictions_PerformanceTest()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 50; i++)
            {
                _aiAdvisorManager.GeneratePredictions();
            }

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1500, "50 GeneratePredictions calls should complete in under 1.5 seconds");
        }

        //[Test]
        public void GetActiveRecommendations_ConsistentResults()
        {
            // Act
            var recommendations1 = _aiAdvisorManager.GetActiveRecommendations();
            var recommendations2 = _aiAdvisorManager.GetActiveRecommendations();

            // Assert
            Assert.AreEqual(recommendations1.Count, recommendations2.Count, "Consecutive calls should return consistent results");
        }

        //[Test]
        public void Settings_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                var settings = _aiAdvisorManager.Settings;
                // Settings might be null if not configured, but property should be accessible
            }, "Settings property should be accessible");
        }
    }
} 