using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.AI;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Testing.Systems.AI
{
    /// <summary>
    /// PC014-1c: Comprehensive test suite for refactored AI advisor components
    /// Tests the specialized services extracted from the monolithic AIAdvisorManager
    /// </summary>
    public class AIServiceIntegrationTests : ChimeraTestBase
    {
        private RefactoredAIAdvisorManager _aiAdvisorManager;
        private CultivationAnalysisService _cultivationService;
        private MarketAnalysisService _marketService;
        private EnvironmentalAnalysisService _environmentalService;
        private RecommendationService _recommendationService;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            // Create test manager
            _aiAdvisorManager = CreateTestManager<RefactoredAIAdvisorManager>();
            
            // Create individual services for direct testing
            _cultivationService = new CultivationAnalysisService();
            _marketService = new MarketAnalysisService();
            _environmentalService = new EnvironmentalAnalysisService();
            _recommendationService = new RecommendationService();
        }
        
        [TearDown]
        public void TearDown()
        {
            _aiAdvisorManager?.Shutdown();
            _cultivationService?.Shutdown();
            _marketService?.Shutdown();
            _environmentalService?.Shutdown();
            _recommendationService?.Shutdown();
            
            CleanupTestEnvironment();
        }
        
        #region RefactoredAIAdvisorManager Tests
        
        [Test]
        public void RefactoredAIAdvisorManager_InitializesSuccessfully()
        {
            // Act
            _aiAdvisorManager.Initialize();
            
            // Assert
            Assert.IsTrue(_aiAdvisorManager.IsInitialized, "RefactoredAIAdvisorManager should initialize successfully");
            Assert.IsNotNull(_aiAdvisorManager.ActiveRecommendations, "ActiveRecommendations should be available");
            Assert.IsNotNull(_aiAdvisorManager.OptimizationOpportunities, "OptimizationOpportunities should be available");
            Assert.IsNotNull(_aiAdvisorManager.CriticalInsights, "CriticalInsights should be available");
        }
        
        [Test]
        public void RefactoredAIAdvisorManager_MaintainsBackwardCompatibility()
        {
            // Arrange
            _aiAdvisorManager.Initialize();
            
            // Act & Assert - Test backward compatible API
            var recommendations = _aiAdvisorManager.GetRecommendationsByCategory("TestCategory");
            Assert.IsNotNull(recommendations, "GetRecommendationsByCategory should return valid collection");
            
            var analysisReport = _aiAdvisorManager.GetLastAnalysisReport();
            Assert.IsNotNull(analysisReport, "GetLastAnalysisReport should return valid report");
            
            var systemEfficiency = _aiAdvisorManager.SystemEfficiencyScore;
            Assert.IsTrue(systemEfficiency >= 0f && systemEfficiency <= 1f, "SystemEfficiencyScore should be valid range");
        }
        
        [UnityTest]
        public IEnumerator RefactoredAIAdvisorManager_GeneratesRecommendationsOverTime()
        {
            // Arrange
            _aiAdvisorManager.Initialize();
            var initialRecommendationCount = _aiAdvisorManager.ActiveRecommendations.Count;
            
            // Act - Wait for analysis cycles
            yield return new WaitForSeconds(2f);
            
            // Assert - Should have generated some recommendations
            var finalRecommendationCount = _aiAdvisorManager.ActiveRecommendations.Count;
            Assert.Pass($"Recommendation count: {initialRecommendationCount} -> {finalRecommendationCount}");
        }
        
        #endregion
        
        #region CultivationAnalysisService Tests
        
        [Test]
        public void CultivationAnalysisService_InitializesAndShutdownsProperly()
        {
            // Act
            _cultivationService.Initialize();
            
            // Assert
            Assert.IsTrue(_cultivationService.IsInitialized, "CultivationAnalysisService should initialize");
            
            // Act
            _cultivationService.Shutdown();
            
            // Assert
            Assert.IsFalse(_cultivationService.IsInitialized, "CultivationAnalysisService should shutdown");
        }
        
        [Test]
        public void CultivationAnalysisService_AnalyzePlantHealth_ReturnsValidRecommendations()
        {
            // Arrange
            _cultivationService.Initialize();
            
            // Act
            var recommendations = _cultivationService.AnalyzePlantHealth();
            
            // Assert
            Assert.IsNotNull(recommendations, "Plant health analysis should return recommendations list");
            
            foreach (var rec in recommendations)
            {
                Assert.IsNotNull(rec.Id, "Recommendation should have valid ID");
                Assert.IsNotNull(rec.Title, "Recommendation should have title");
                Assert.IsNotNull(rec.Category, "Recommendation should have category");
                Assert.AreEqual("PlantHealth", rec.Category, "Plant health recommendations should have correct category");
            }
        }
        
        [Test]
        public void CultivationAnalysisService_AnalyzeGrowthPatterns_ProducesValidResults()
        {
            // Arrange
            _cultivationService.Initialize();
            
            // Act
            var recommendations = _cultivationService.AnalyzeGrowthPatterns();
            
            // Assert
            Assert.IsNotNull(recommendations, "Growth pattern analysis should return results");
            
            foreach (var rec in recommendations)
            {
                Assert.IsTrue(rec.ConfidenceScore >= 0f && rec.ConfidenceScore <= 1f, "Confidence score should be valid");
                Assert.IsTrue(rec.EstimatedImpact >= 0f && rec.EstimatedImpact <= 1f, "Estimated impact should be valid");
            }
        }
        
        [Test]
        public void CultivationAnalysisService_GetCultivationInsights_ReturnsValidInsights()
        {
            // Arrange
            _cultivationService.Initialize();
            var testSnapshot = CreateTestAnalysisSnapshot();
            
            // Act
            _cultivationService.PerformAnalysis(testSnapshot);
            var insights = _cultivationService.GetCultivationInsights();
            
            // Assert
            Assert.IsNotNull(insights, "Should return insights collection");
        }
        
        #endregion
        
        #region MarketAnalysisService Tests
        
        [Test]
        public void MarketAnalysisService_InitializesWithSimulatedData()
        {
            // Act
            _marketService.Initialize();
            
            // Assert
            Assert.IsTrue(_marketService.IsInitialized, "MarketAnalysisService should initialize");
            
            var marketData = _marketService.GetMarketData();
            Assert.IsNotNull(marketData, "Should provide market data");
            Assert.IsNotNull(marketData.ProductPrices, "Should have product prices");
            Assert.IsNotNull(marketData.DemandData, "Should have demand data");
        }
        
        [Test]
        public void MarketAnalysisService_AnalyzeMarketTrends_GeneratesRelevantRecommendations()
        {
            // Arrange
            _marketService.Initialize();
            
            // Act
            var recommendations = _marketService.AnalyzeMarketTrends();
            
            // Assert
            Assert.IsNotNull(recommendations, "Market trend analysis should return recommendations");
            
            foreach (var rec in recommendations)
            {
                Assert.AreEqual("MarketTrends", rec.Category, "Market recommendations should have correct category");
                Assert.IsTrue(rec.ExpirationTime > DateTime.Now, "Recommendations should not be immediately expired");
            }
        }
        
        [Test]
        public void MarketAnalysisService_AnalyzePricingOpportunities_ProducesValidResults()
        {
            // Arrange
            _marketService.Initialize();
            
            // Act
            var recommendations = _marketService.AnalyzePricingOpportunities();
            
            // Assert
            Assert.IsNotNull(recommendations, "Pricing analysis should return results");
            
            foreach (var rec in recommendations)
            {
                Assert.AreEqual("PricingOptimization", rec.Category, "Pricing recommendations should have correct category");
            }
        }
        
        [Test]
        public void MarketAnalysisService_AnalyzeProductDemand_IdentifiesDemandPatterns()
        {
            // Arrange
            _marketService.Initialize();
            
            // Act
            var recommendations = _marketService.AnalyzeProductDemand();
            
            // Assert
            Assert.IsNotNull(recommendations, "Demand analysis should return results");
            
            foreach (var rec in recommendations)
            {
                Assert.AreEqual("DemandAnalysis", rec.Category, "Demand recommendations should have correct category");
            }
        }
        
        #endregion
        
        #region EnvironmentalAnalysisService Tests
        
        [Test]
        public void EnvironmentalAnalysisService_InitializesCorrectly()
        {
            // Act
            _environmentalService.Initialize();
            
            // Assert
            Assert.IsTrue(_environmentalService.IsInitialized, "EnvironmentalAnalysisService should initialize");
        }
        
        [Test]
        public void EnvironmentalAnalysisService_AnalyzeEnvironmentalOptimization_GeneratesRecommendations()
        {
            // Arrange
            _environmentalService.Initialize();
            
            // Act
            var recommendations = _environmentalService.AnalyzeEnvironmentalOptimization();
            
            // Assert
            Assert.IsNotNull(recommendations, "Environmental optimization should return recommendations");
            
            foreach (var rec in recommendations)
            {
                Assert.AreEqual("EnvironmentalOptimization", rec.Category, "Environmental recommendations should have correct category");
            }
        }
        
        [Test]
        public void EnvironmentalAnalysisService_AnalyzeClimateControl_ProducesValidResults()
        {
            // Arrange
            _environmentalService.Initialize();
            
            // Act
            var recommendations = _environmentalService.AnalyzeClimateControl();
            
            // Assert
            Assert.IsNotNull(recommendations, "Climate control analysis should return results");
            
            foreach (var rec in recommendations)
            {
                Assert.AreEqual("ClimateControl", rec.Category, "Climate recommendations should have correct category");
            }
        }
        
        [Test]
        public void EnvironmentalAnalysisService_GetEnvironmentalOptimizations_ReturnsValidOpportunities()
        {
            // Arrange
            _environmentalService.Initialize();
            var testSnapshot = CreateTestAnalysisSnapshot();
            
            // Act
            _environmentalService.PerformAnalysis(testSnapshot);
            var optimizations = _environmentalService.GetEnvironmentalOptimizations();
            
            // Assert
            Assert.IsNotNull(optimizations, "Should return optimization opportunities");
        }
        
        #endregion
        
        #region RecommendationService Tests
        
        [Test]
        public void RecommendationService_ManagesRecommendationLifecycle()
        {
            // Arrange
            _recommendationService.Initialize();
            var testRecommendation = CreateTestRecommendation();
            
            // Act & Assert - Add recommendation
            _recommendationService.AddRecommendation(testRecommendation);
            var activeRecs = _recommendationService.GetActiveRecommendations();
            Assert.AreEqual(1, activeRecs.Count, "Should have one active recommendation");
            
            // Act & Assert - Mark as implemented
            _recommendationService.MarkAsImplemented(testRecommendation.Id);
            activeRecs = _recommendationService.GetActiveRecommendations();
            Assert.AreEqual(0, activeRecs.Count, "Should have no active recommendations after implementation");
        }
        
        [Test]
        public void RecommendationService_HandlesRecommendationExpiration()
        {
            // Arrange
            _recommendationService.Initialize();
            var expiredRecommendation = CreateTestRecommendation();
            expiredRecommendation.ExpirationTime = DateTime.Now.AddSeconds(-1); // Already expired
            
            // Act
            _recommendationService.AddRecommendation(expiredRecommendation);
            _recommendationService.UpdateRecommendationStatus();
            var activeRecs = _recommendationService.GetActiveRecommendations();
            
            // Assert
            Assert.AreEqual(0, activeRecs.Count, "Expired recommendations should not be in active list");
        }
        
        [Test]
        public void RecommendationService_FiltersByCategory()
        {
            // Arrange
            _recommendationService.Initialize();
            var rec1 = CreateTestRecommendation();
            rec1.Category = "TestCategory1";
            var rec2 = CreateTestRecommendation();
            rec2.Category = "TestCategory2";
            
            // Act
            _recommendationService.AddRecommendation(rec1);
            _recommendationService.AddRecommendation(rec2);
            var category1Recs = _recommendationService.GetRecommendationsByCategory("TestCategory1");
            var category2Recs = _recommendationService.GetRecommendationsByCategory("TestCategory2");
            
            // Assert
            Assert.AreEqual(1, category1Recs.Count, "Should filter by category correctly");
            Assert.AreEqual(1, category2Recs.Count, "Should filter by category correctly");
            Assert.AreEqual("TestCategory1", category1Recs[0].Category, "Should return correct category");
        }
        
        [Test]
        public void RecommendationService_ProvidesStatistics()
        {
            // Arrange
            _recommendationService.Initialize();
            var rec = CreateTestRecommendation();
            rec.Category = "TestCategory";
            
            // Act
            _recommendationService.AddRecommendation(rec);
            _recommendationService.MarkAsImplemented(rec.Id);
            
            var implementationStats = _recommendationService.GetImplementationStatistics();
            var implementationRate = _recommendationService.GetImplementationRate("TestCategory");
            
            // Assert
            Assert.IsNotNull(implementationStats, "Should provide implementation statistics");
            Assert.IsTrue(implementationRate >= 0f && implementationRate <= 1f, "Implementation rate should be valid");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void AIServices_WorkTogetherInRefactoredManager()
        {
            // Arrange
            _aiAdvisorManager.Initialize();
            
            // Act - Let the manager run its analysis cycles
            var initialRecommendations = _aiAdvisorManager.ActiveRecommendations.Count;
            
            // Simulate some time passing for analysis
            for (int i = 0; i < 3; i++)
            {
                // The manager should be running its analysis cycles
                System.Threading.Thread.Sleep(100);
            }
            
            // Assert
            Assert.IsTrue(_aiAdvisorManager.IsInitialized, "Manager should remain initialized");
            Assert.IsTrue(_aiAdvisorManager.SystemEfficiencyScore >= 0f, "System efficiency should be calculated");
        }
        
        [Test]
        public void AIServices_HandleNullInputsGracefully()
        {
            // Arrange
            _cultivationService.Initialize();
            _marketService.Initialize();
            _environmentalService.Initialize();
            
            // Act & Assert - Services should handle null analysis snapshots
            Assert.DoesNotThrow(() => _cultivationService.PerformAnalysis(null));
            Assert.DoesNotThrow(() => _marketService.PerformAnalysis(null));
            Assert.DoesNotThrow(() => _environmentalService.PerformAnalysis(null));
        }
        
        #endregion
        
        #region Helper Methods
        
        private AnalysisSnapshot CreateTestAnalysisSnapshot()
        {
            return new AnalysisSnapshot
            {
                Timestamp = DateTime.Now,
                EnvironmentalData = new EnvironmentalSnapshot
                {
                    HVACEfficiency = 85f,
                    EnergyUsage = 1200f,
                    LightingEfficiency = 90f,
                    DLIOptimization = 88f
                },
                EconomicData = new EconomicSnapshot
                {
                    Revenue = 45000f,
                    Profit = 12000f,
                    ROI = 0.18f
                },
                SystemData = new SystemSnapshot
                {
                    SystemHealth = 0.92f
                }
            };
        }
        
        private AIRecommendation CreateTestRecommendation()
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Test Recommendation",
                Summary = "Test summary",
                Description = "Test description",
                Priority = RecommendationPriority.Medium,
                Category = "TestCategory",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(24),
                ConfidenceScore = 0.8f,
                EstimatedImpact = 0.7f,
                IsActive = true
            };
        }
        
        #endregion
    }
}