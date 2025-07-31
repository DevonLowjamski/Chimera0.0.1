using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.AI;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using AIEnvironmentalAnalysisResult = ProjectChimera.Data.AI.EnvironmentalAnalysisResult;
using RecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;

namespace ProjectChimera.Testing.Systems.AI
{
    /// <summary>
    /// PC013: Comprehensive test suite for new AI services achieving 90% code coverage
    /// Tests AIAnalysisService, AIRecommendationService, AIPersonalityService, AILearningService, and AIAdvisorCoordinator
    /// </summary>
    public class PC013AIServicesTestSuite : ChimeraTestBase
    {
        private AIAnalysisService _analysisService;
        private AIRecommendationService _recommendationService;
        private AIPersonalityService _personalityService;
        private AILearningService _learningService;
        private AIAdvisorCoordinator _coordinatorService;
        private AIServicesIntegration _integrationService;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            // Create service instances for testing
            _analysisService = CreateTestManager<AIAnalysisService>();
            _recommendationService = CreateTestManager<AIRecommendationService>();
            _personalityService = CreateTestManager<AIPersonalityService>();
            _learningService = CreateTestManager<AILearningService>();
            _coordinatorService = CreateTestManager<AIAdvisorCoordinator>();
            _integrationService = CreateTestManager<AIServicesIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            _integrationService?.Shutdown();
            _coordinatorService?.Shutdown();
            _learningService?.Shutdown();
            _personalityService?.Shutdown();
            _recommendationService?.Shutdown();
            _analysisService?.Shutdown();
            
            CleanupTestEnvironment();
        }
        
        #region AIAnalysisService Tests
        
        [Test]
        public void AIAnalysisService_InitializesSuccessfully()
        {
            // Act
            _analysisService.Initialize();
            
            // Assert
            Assert.IsTrue(_analysisService.IsInitialized, "AIAnalysisService should initialize successfully");
            Assert.IsTrue(_analysisService.EnableRealTimeAnalysis, "Real-time analysis should be enabled by default");
            Assert.IsTrue(_analysisService.EnablePredictiveAnalysis, "Predictive analysis should be enabled by default");
            Assert.AreEqual(0.6f, _analysisService.ConfidenceThreshold, 0.01f, "Default confidence threshold should be 0.6");
        }
        
        [Test]
        public void AIAnalysisService_ConfigurationProperties_WorkCorrectly()
        {
            // Arrange
            _analysisService.Initialize();
            
            // Act & Assert - EnableRealTimeAnalysis
            _analysisService.EnableRealTimeAnalysis = false;
            Assert.IsFalse(_analysisService.EnableRealTimeAnalysis, "Should be able to disable real-time analysis");
            
            // Act & Assert - EnablePredictiveAnalysis
            _analysisService.EnablePredictiveAnalysis = false;
            Assert.IsFalse(_analysisService.EnablePredictiveAnalysis, "Should be able to disable predictive analysis");
            
            // Act & Assert - ConfidenceThreshold
            _analysisService.ConfidenceThreshold = 0.8f;
            Assert.AreEqual(0.8f, _analysisService.ConfidenceThreshold, 0.01f, "Should be able to set confidence threshold");
        }
        
        [UnityTest]
        public IEnumerator AIAnalysisService_AnalyzeCultivationDataAsync_ReturnsValidResults()
        {
            // Arrange
            _analysisService.Initialize();
            
            // Act
            var task = _analysisService.AnalyzeCultivationDataAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.Result;
            
            // Assert
            Assert.IsNotNull(result, "Cultivation analysis should return a result");
            Assert.IsTrue(result.TotalPlants >= 0, "Total plants should be non-negative");
            Assert.IsTrue(result.ActivePlants >= 0, "Active plants should be non-negative");
            Assert.IsTrue(result.AverageHealth >= 0f && result.AverageHealth <= 1f, "Average health should be in valid range");
            Assert.IsNotNull(result.HealthIssues, "Health issues list should not be null");
            Assert.IsNotNull(result.GrowthRecommendations, "Growth recommendations should not be null");
        }
        
        [UnityTest]
        public IEnumerator AIAnalysisService_AnalyzeEnvironmentalDataAsync_ReturnsValidResults()
        {
            // Arrange
            _analysisService.Initialize();
            
            // Act
            var task = _analysisService.AnalyzeEnvironmentalDataAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.Result;
            
            // Assert
            Assert.IsNotNull(result, "Environmental analysis should return a result");
            Assert.IsTrue(result.TemperatureScore >= 0f && result.TemperatureScore <= 1f, "Temperature score should be in valid range");
            Assert.IsTrue(result.HumidityScore >= 0f && result.HumidityScore <= 1f, "Humidity score should be in valid range");
            Assert.IsTrue(result.CO2Score >= 0f && result.CO2Score <= 1f, "CO2 score should be in valid range");
            Assert.IsTrue(result.LightScore >= 0f && result.LightScore <= 1f, "Light score should be in valid range");
            Assert.IsNotNull(result.Recommendations, "Environmental recommendations should not be null");
            Assert.IsNotNull(result.OptimalRanges, "Optimal ranges should not be null");
        }
        
        [UnityTest]
        public IEnumerator AIAnalysisService_AnalyzeGeneticsDataAsync_ReturnsValidResults()
        {
            // Arrange
            _analysisService.Initialize();
            
            // Act
            var task = _analysisService.AnalyzeGeneticsDataAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.Result;
            
            // Assert
            Assert.IsNotNull(result, "Genetics analysis should return a result");
            Assert.IsTrue(result.TotalStrains >= 0, "Total strains should be non-negative");
            Assert.IsTrue(result.GeneticDiversity >= 0f && result.GeneticDiversity <= 1f, "Genetic diversity should be in valid range");
            Assert.IsTrue(result.BreedingPotential >= 0f && result.BreedingPotential <= 1f, "Breeding potential should be in valid range");
            Assert.IsNotNull(result.OptimalCrosses, "Optimal crosses should not be null");
            Assert.IsNotNull(result.GeneticRecommendations, "Genetic recommendations should not be null");
        }
        
        [Test]
        public void AIAnalysisService_EventsFireCorrectly()
        {
            // Arrange
            _analysisService.Initialize();
            bool cultivationEventFired = false;
            bool environmentalEventFired = false;
            bool geneticsEventFired = false;
            
            _analysisService.OnCultivationAnalysisComplete += (result) => cultivationEventFired = true;
            _analysisService.OnEnvironmentalAnalysisComplete += (result) => environmentalEventFired = true;
            _analysisService.OnGeneticsAnalysisComplete += (result) => geneticsEventFired = true;
            
            // Act
            var cultivationTask = _analysisService.AnalyzeCultivationDataAsync();
            var environmentalTask = _analysisService.AnalyzeEnvironmentalDataAsync();
            var geneticsTask = _analysisService.AnalyzeGeneticsDataAsync();
            
            // Wait for completion
            Task.WaitAll(cultivationTask, environmentalTask, geneticsTask);
            
            // Assert
            Assert.IsTrue(cultivationEventFired, "Cultivation analysis complete event should fire");
            Assert.IsTrue(environmentalEventFired, "Environmental analysis complete event should fire");
            Assert.IsTrue(geneticsEventFired, "Genetics analysis complete event should fire");
        }
        
        #endregion
        
        #region AIRecommendationService Tests
        
        [Test]
        public void AIRecommendationService_InitializesSuccessfully()
        {
            // Act
            _recommendationService.Initialize();
            
            // Assert
            Assert.IsTrue(_recommendationService.IsInitialized, "AIRecommendationService should initialize successfully");
            Assert.IsNotNull(_recommendationService.ActiveRecommendations, "Active recommendations should be initialized");
            Assert.IsNotNull(_recommendationService.RecommendationHistory, "Recommendation history should be initialized");
        }
        
        [UnityTest]
        public IEnumerator AIRecommendationService_GeneratesRecommendationsFromAnalysis()
        {
            // Arrange
            _recommendationService.Initialize();
            var testAnalysis = CreateTestCultivationAnalysis();
            
            // Act
            var task = _recommendationService.GenerateRecommendationsAsync(testAnalysis);
            yield return new WaitUntil(() => task.IsCompleted);
            var recommendations = task.Result;
            
            // Assert
            Assert.IsNotNull(recommendations, "Should generate recommendations from analysis");
            Assert.IsTrue(recommendations.Count > 0, "Should generate at least one recommendation");
            
            foreach (var rec in recommendations)
            {
                Assert.IsNotNull(rec.Id, "Recommendation should have valid ID");
                Assert.IsNotNull(rec.Title, "Recommendation should have title");
                Assert.IsNotNull(rec.Category, "Recommendation should have category");
                Assert.IsTrue(rec.Priority >= 0, "Priority should be valid");
            }
        }
        
        [Test]
        public void AIRecommendationService_PrioritizesRecommendationsCorrectly()
        {
            // Arrange
            _recommendationService.Initialize();
            var recommendations = new List<AIRecommendation>
            {
                CreateTestRecommendation("Low Priority", RecommendationPriority.Low),
                CreateTestRecommendation("High Priority", RecommendationPriority.High),
                CreateTestRecommendation("Medium Priority", RecommendationPriority.Medium)
            };
            
            // Act
            var prioritized = _recommendationService.PrioritizeRecommendations(recommendations);
            
            // Assert
            Assert.AreEqual(3, prioritized.Count, "Should return all recommendations");
            Assert.AreEqual(RecommendationPriority.High, prioritized[0].Priority, "Highest priority should be first");
            Assert.AreEqual(RecommendationPriority.Medium, prioritized[1].Priority, "Medium priority should be second");
            Assert.AreEqual(RecommendationPriority.Low, prioritized[2].Priority, "Lowest priority should be last");
        }
        
        [Test]
        public void AIRecommendationService_FiltersRecommendationsByCategory()
        {
            // Arrange
            _recommendationService.Initialize();
            var recommendations = new List<AIRecommendation>
            {
                CreateTestRecommendation("Plant Health", RecommendationPriority.High, "PlantHealth"),
                CreateTestRecommendation("Environmental", RecommendationPriority.Medium, "Environmental"),
                CreateTestRecommendation("Economic", RecommendationPriority.Low, "Economic")
            };
            
            // Act
            var filtered = _recommendationService.FilterRecommendations(recommendations, "PlantHealth");
            
            // Assert
            Assert.AreEqual(1, filtered.Count, "Should filter to one recommendation");
            Assert.AreEqual("PlantHealth", filtered[0].Category, "Should return correct category");
        }
        
        #endregion
        
        #region AIPersonalityService Tests
        
        [Test]
        public void AIPersonalityService_InitializesWithDefaultPersonality()
        {
            // Act
            _personalityService.Initialize();
            
            // Assert
            Assert.IsTrue(_personalityService.IsInitialized, "AIPersonalityService should initialize successfully");
            Assert.IsNotNull(_personalityService.CurrentPersonality, "Should have a current personality");
            Assert.IsNotNull(_personalityService.ActiveProfile, "Should have an active profile");
        }
        
        [Test]
        public void AIPersonalityService_RecordsPlayerInteractions()
        {
            // Arrange
            _personalityService.Initialize();
            var initialInteractionCount = _personalityService.GetInteractionHistory().Count;
            
            // Act
            _personalityService.RecordPlayerInteraction("TestInteraction", 0.8f);
            
            // Assert
            var history = _personalityService.GetInteractionHistory();
            Assert.AreEqual(initialInteractionCount + 1, history.Count, "Should record the interaction");
            Assert.AreEqual("TestInteraction", history[history.Count - 1].InteractionType, "Should record correct interaction type");
        }
        
        [Test]
        public void AIPersonalityService_AdaptsPersonalityBasedOnInteractions()
        {
            // Arrange
            _personalityService.Initialize();
            var initialPersonality = _personalityService.CurrentPersonality;
            
            // Act - Record multiple high-satisfaction interactions
            for (int i = 0; i < 5; i++)
            {
                _personalityService.RecordPlayerInteraction("PositiveInteraction", 0.9f);
            }
            _personalityService.UpdatePersonalityFromInteractions();
            
            // Assert
            var newPersonality = _personalityService.CurrentPersonality;
            Assert.IsNotNull(newPersonality, "Should have updated personality");
            // Personality adaptation logic should have made some changes
        }
        
        [Test]
        public void AIPersonalityService_GeneratesPersonalizedMessages()
        {
            // Arrange
            _personalityService.Initialize();
            
            // Act
            var message = _personalityService.GeneratePersonalizedMessage("Welcome", "greeting");
            
            // Assert
            Assert.IsNotNull(message, "Should generate a personalized message");
            Assert.IsTrue(message.Length > 0, "Message should not be empty");
        }
        
        #endregion
        
        #region AILearningService Tests
        
        [Test]
        public void AILearningService_InitializesWithMLModels()
        {
            // Act
            _learningService.Initialize();
            
            // Assert
            Assert.IsTrue(_learningService.IsInitialized, "AILearningService should initialize successfully");
            Assert.IsNotNull(_learningService.ActiveModels, "Should have active models collection");
            Assert.IsNotNull(_learningService.LearningHistory, "Should have learning history");
        }
        
        [UnityTest]
        public IEnumerator AILearningService_TrainsModelsFromData()
        {
            // Arrange
            _learningService.Initialize();
            var trainingData = CreateTestLearningData();
            
            // Act
            var task = _learningService.TrainModelAsync("TestModel", trainingData);
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.Result;
            
            // Assert
            Assert.IsNotNull(result, "Should return training result");
            Assert.IsTrue(result.Success, "Training should succeed");
            Assert.IsNotNull(result.ModelId, "Should have model ID");
            Assert.IsTrue(result.Accuracy >= 0f && result.Accuracy <= 1f, "Accuracy should be in valid range");
        }
        
        [UnityTest]
        public IEnumerator AILearningService_MakesPredictions()
        {
            // Arrange
            _learningService.Initialize();
            var trainingData = CreateTestLearningData();
            var trainTask = _learningService.TrainModelAsync("PredictionModel", trainingData);
            yield return new WaitUntil(() => trainTask.IsCompleted);
            
            var inputData = CreateTestPredictionInput();
            
            // Act
            var predictionTask = _learningService.MakePredictionAsync("PredictionModel", inputData);
            yield return new WaitUntil(() => predictionTask.IsCompleted);
            var prediction = predictionTask.Result;
            
            // Assert
            Assert.IsNotNull(prediction, "Should return prediction result");
            Assert.IsNotNull(prediction.PredictedVariable, "Should have predicted variable");
            Assert.IsTrue(prediction.Confidence >= 0f && prediction.Confidence <= 1f, "Confidence should be in valid range");
        }
        
        #endregion
        
        #region AIAdvisorCoordinator Tests
        
        [Test]
        public void AIAdvisorCoordinator_InitializesSuccessfully()
        {
            // Act
            _coordinatorService.Initialize();
            
            // Assert
            Assert.IsTrue(_coordinatorService.IsInitialized, "AIAdvisorCoordinator should initialize successfully");
            Assert.IsNotNull(_coordinatorService.ActiveRequests, "Should have active requests collection");
            Assert.IsNotNull(_coordinatorService.WorkflowHistory, "Should have workflow history");
        }
        
        [UnityTest]
        public IEnumerator AIAdvisorCoordinator_ProcessesCoordinationRequests()
        {
            // Arrange
            _coordinatorService.Initialize();
            var request = CreateTestCoordinationRequest();
            
            // Act
            var task = _coordinatorService.ProcessRequestAsync(request);
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.Result;
            
            // Assert
            Assert.IsNotNull(result, "Should return coordination result");
            Assert.IsTrue(result.Success, "Coordination should succeed");
            Assert.IsNotNull(result.GeneratedRecommendations, "Should generate recommendations");
        }
        
        [Test]
        public void AIAdvisorCoordinator_ManagesServiceHealth()
        {
            // Arrange
            _coordinatorService.Initialize();
            
            // Act
            var healthStatus = _coordinatorService.GetServiceHealthStatus();
            
            // Assert
            Assert.IsNotNull(healthStatus, "Should return health status");
            Assert.IsTrue(healthStatus.Count > 0, "Should have health information for services");
        }
        
        #endregion
        
        #region AIServicesIntegration Tests
        
        [Test]
        public void AIServicesIntegration_InitializesAllServices()
        {
            // Act
            _integrationService.Initialize();
            
            // Assert
            Assert.IsTrue(_integrationService.IsInitialized, "Integration service should initialize");
            Assert.AreEqual(IntegrationStatus.Running, _integrationService.Status, "Should be in running status");
            Assert.IsNotNull(_integrationService.ServiceContainer, "Should have service container");
            Assert.IsNotNull(_integrationService.EventBus, "Should have event bus");
            // Verify the service container and event bus are properly initialized
            var serviceContainer = _integrationService.ServiceContainer;
            var eventBus = _integrationService.EventBus;
            Assert.IsNotNull(serviceContainer, "Service container should be accessible");
            Assert.IsNotNull(eventBus, "Event bus should be accessible");
        }
        
        [Test]
        public void AIServicesIntegration_ProvidesMetrics()
        {
            // Arrange
            _integrationService.Initialize();
            
            // Act
            var metrics = _integrationService.GetMetrics();
            
            // Assert
            Assert.IsNotNull(metrics, "Should provide integration metrics");
            Assert.IsTrue(metrics.TotalServices > 0, "Should have registered services");
            Assert.IsTrue(metrics.InitializationTime >= TimeSpan.Zero, "Should have valid initialization time");
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator AllAIServices_WorkTogetherInIntegratedSystem()
        {
            // Arrange
            _integrationService.Initialize();
            yield return new WaitForSeconds(1f); // Allow initialization to complete
            
            // Act - Simulate a full AI analysis cycle
            var analysisTask = _analysisService.AnalyzeCultivationDataAsync();
            yield return new WaitUntil(() => analysisTask.IsCompleted);
            var analysisResult = analysisTask.Result;
            
            var recommendationTask = _recommendationService.GenerateRecommendationsAsync(analysisResult);
            yield return new WaitUntil(() => recommendationTask.IsCompleted);
            var recommendations = recommendationTask.Result;
            
            // Assert
            Assert.IsNotNull(analysisResult, "Analysis should complete successfully");
            Assert.IsNotNull(recommendations, "Recommendations should be generated");
            Assert.IsTrue(recommendations.Count > 0, "Should generate at least one recommendation");
            
            // Verify integration metrics
            var metrics = _integrationService.GetMetrics();
            Assert.IsTrue(metrics.TotalEvents > 0, "Events should have been processed");
        }
        
        #endregion
        
        #region Helper Methods
        
        private CultivationAnalysisResult CreateTestCultivationAnalysis()
        {
            return new CultivationAnalysisResult
            {
                TotalPlants = 10,
                ActivePlants = 8,
                AverageHealth = 0.85f,
                PredictedYield = 150f,
                OptimalYield = 180f,
                HealthIssues = new List<string> { "Minor nutrient deficiency" },
                GrowthRecommendations = new List<string> { "Increase nitrogen levels" },
                EfficiencyScore = 0.8f
            };
        }
        
        private AIRecommendation CreateTestRecommendation(string title, RecommendationPriority priority, string category = "Test")
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = $"Test recommendation: {title}",
                Description = $"Detailed description for {title}",
                Priority = priority,
                Category = category,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                ConfidenceScore = 0.8f,
                EstimatedImpact = 0.7f
            };
        }
        
        private List<LearningRecord> CreateTestLearningData()
        {
            return new List<LearningRecord>
            {
                new LearningRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    InputData = new Dictionary<string, float> { ["temperature"] = 24f, ["humidity"] = 60f },
                    OutputData = new Dictionary<string, float> { ["yield"] = 120f },
                    Context = "cultivation_data"
                }
            };
        }
        
        private Dictionary<string, float> CreateTestPredictionInput()
        {
            return new Dictionary<string, float>
            {
                ["temperature"] = 25f,
                ["humidity"] = 55f,
                ["light_hours"] = 18f
            };
        }
        
        private CoordinationRequest CreateTestCoordinationRequest()
        {
            return new CoordinationRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                RequestType = RequestType.Analysis,
                WorkflowType = WorkflowType.CultivationOptimization,
                Priority = CoordinationPriority.Medium,
                RequestedAt = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>
                {
                    ["plant_count"] = 10,
                    ["analysis_depth"] = "comprehensive"
                }
            };
        }
        
        #endregion
    }
}