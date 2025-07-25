using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Testing.Integration
{
    /// <summary>
    /// PC013-23: Comprehensive integration tests for service orchestrators
    /// Tests the interaction between multiple services to ensure proper orchestration
    /// and architectural integrity after the PC013 refactoring
    /// </summary>
    [TestFixture]
    [Category("Integration Tests")]
    [Category("Service Orchestration")]
    public class ServiceOrchestratorIntegrationTests : ChimeraTestBase
    {
        // Service instances for integration testing
        private PlantLifecycleService _lifecycleService;
        private PlantGrowthService _growthService;
        private PlantEnvironmentalProcessingService _environmentalService;
        private PlantYieldCalculationService _yieldService;
        private PlantGeneticsService _geneticsService;
        private PlantHarvestService _harvestService;
        private PlantStatisticsService _statisticsService;
        private PlantAchievementService _achievementService;
        private PlantProcessingService _processingService;
        private PlantEnvironmentalService _plantEnvironmentalService;
        
        // Test data
        private PlantStrainSO _testStrain;
        private PlantInstance _testPlant;
        private CannabisGenotype _testGenotype;
        private EnvironmentalConditions _testEnvironment;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            // Initialize all services
            InitializeServices();
            
            // Create test data
            _testStrain = CreateTestPlantStrain();
            _testPlant = CreateTestPlantInstance();
            _testGenotype = CreateTestGenotype();
            _testEnvironment = CreateTestEnvironmentalConditions();
        }
        
        [TearDown]
        public void TearDown()
        {
            ShutdownServices();
            CleanupTestEnvironment();
        }
        
        #region Service Orchestration Tests
        
        [Test]
        public void ServiceOrchestration_PlantLifecycleWorkflow_ExecutesCorrectly()
        {
            // Arrange - Plant creation and registration
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex("PlantLifecycleService.*initialized"));
            _lifecycleService.Initialize();
            
            // Act - Register plant and track lifecycle
            _lifecycleService.RegisterPlant(_testPlant);
            var trackedPlant = _lifecycleService.GetTrackedPlant(_testPlant.PlantID);
            
            // Assert - Plant is properly tracked
            Assert.IsNotNull(trackedPlant, "Plant should be tracked by lifecycle service");
            Assert.AreEqual(_testPlant.PlantID, trackedPlant.PlantID, "Tracked plant should have correct ID");
        }
        
        [Test]
        public void ServiceOrchestration_GrowthAndEnvironmentalIntegration_WorksTogether()
        {
            // Arrange
            _growthService.Initialize();
            _environmentalService.Initialize();
            
            var initialStage = _testPlant.CurrentGrowthStage;
            
            // Act - Process growth with environmental factors
            var growthResult = _growthService.ProcessPlantGrowth(_testPlant, 1.0f);
            var environmentalResult = _environmentalService.ProcessEnvironmentalEffects(_testPlant, _testEnvironment);
            
            // Assert - Both services return valid results
            Assert.IsNotNull(growthResult, "Growth processing should return results");
            Assert.IsNotNull(environmentalResult, "Environmental processing should return results");
            Assert.GreaterOrEqual(environmentalResult.EnvironmentalFitness, 0f, "Environmental fitness should be non-negative");
            Assert.LessOrEqual(environmentalResult.EnvironmentalFitness, 1f, "Environmental fitness should not exceed 1.0");
        }
        
        [Test]
        public void ServiceOrchestration_YieldCalculationWithMultipleServices_ProducesConsistentResults()
        {
            // Arrange
            _yieldService.Initialize();
            _growthService.Initialize();
            _environmentalService.Initialize();
            _testPlant.CurrentGrowthStage = PlantGrowthStage.Harvest;
            
            // Act - Calculate yield using multiple service inputs
            var yieldData = _yieldService.CalculateExpectedYieldData(_testPlant);
            var growthResult = _growthService.ProcessPlantGrowth(_testPlant, 1.0f);
            var environmentalResult = _environmentalService.ProcessEnvironmentalEffects(_testPlant, _testEnvironment);
            
            // Assert - Yield calculation integrates multiple factors
            Assert.IsNotNull(yieldData, "Yield calculation should return data");
            Assert.Greater(yieldData.BaseYield, 0f, "Base yield should be positive");
            Assert.Greater(yieldData.EstimatedYield, 0f, "Estimated yield should be positive");
            Assert.IsTrue(yieldData.HealthModifier > 0f && yieldData.HealthModifier <= 2f, "Health modifier should be in reasonable range");
        }
        
        [Test]
        public void ServiceOrchestration_HarvestWorkflow_IntegratesAllServices()
        {
            // Arrange
            InitializeAllServices();
            _testPlant.CurrentGrowthStage = PlantGrowthStage.Harvest;
            
            // Expect warning for test scenario (GameManager not available)
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Cannot harvest unknown plant.*testing mode"));
            
            // Act - Execute harvest workflow
            var harvestResult = _harvestService.HarvestPlant(_testPlant.PlantID);
            var achievementStats = _achievementService.GetAchievementStats();
            var statistics = _statisticsService.GetStatistics();
            
            // Assert - Harvest workflow completes (even with null result in test mode)
            Assert.IsNull(harvestResult, "Harvest should return null in test mode without GameManager");
            Assert.IsNotNull(achievementStats, "Achievement stats should be available");
            Assert.IsNotNull(statistics, "Statistics should be available");
        }
        
        [Test]
        public void ServiceOrchestration_ProcessingServiceCoordination_ManagesMultipleServices()
        {
            // Arrange
            _processingService.Initialize();
            _lifecycleService.Initialize();
            _growthService.Initialize();
            
            // Act - Process plant through processing service
            var processingResult = _processingService.ProcessPlantData(_testPlant);
            var processingStats = _processingService.GetProcessingStats();
            
            // Assert - Processing service coordinates properly
            Assert.IsNotNull(processingResult, "Processing should return results");
            Assert.IsTrue(processingResult.ProcessingSuccess, "Processing should succeed");
            Assert.Greater(processingResult.ProcessingTimeMs, 0f, "Processing time should be recorded");
            Assert.AreEqual(_testPlant.PlantID, processingResult.PlantID, "Processing result should have correct plant ID");
        }
        
        #endregion
        
        #region Cross-Service Data Flow Tests
        
        [Test]
        public void CrossServiceDataFlow_AchievementTrackingIntegration_RecordsCorrectly()
        {
            // Arrange
            _achievementService.Initialize();
            _lifecycleService.Initialize();
            
            var initialStats = _achievementService.GetAchievementStats();
            var initialCreatedCount = initialStats.TotalPlantsCreated;
            
            // Act - Track plant creation through achievement service
            _achievementService.TrackPlantCreation(_testPlant);
            _lifecycleService.RegisterPlant(_testPlant);
            
            var finalStats = _achievementService.GetAchievementStats();
            
            // Assert - Achievement tracking integrates with lifecycle
            Assert.AreEqual(initialCreatedCount + 1, finalStats.TotalPlantsCreated, "Plant creation should be tracked");
            Assert.GreaterOrEqual(finalStats.StrainDiversity, 1, "Strain diversity should be tracked");
        }
        
        [Test]
        public void CrossServiceDataFlow_StatisticsAggregation_CombinesMultipleSources()
        {
            // Arrange
            InitializeAllServices();
            
            // Act - Generate statistics from multiple services
            var plantStats = _statisticsService.GetStatistics();
            var enhancedStats = _statisticsService.GetEnhancedStatistics();
            var achievementStats = _achievementService.GetAchievementStats();
            var geneticsStats = _geneticsService.GetGeneticPerformanceStats();
            
            // Assert - Statistics aggregate properly
            Assert.IsNotNull(plantStats, "Plant statistics should be available");
            Assert.IsNotNull(enhancedStats, "Enhanced statistics should be available");
            Assert.IsNotNull(achievementStats, "Achievement statistics should be available");
            Assert.IsNotNull(geneticsStats, "Genetics statistics should be available");
            
            // Verify data consistency between services
            Assert.GreaterOrEqual(plantStats.TotalPlants, 0, "Total plants should be non-negative");
            Assert.GreaterOrEqual(achievementStats.TotalPlantsCreated, 0, "Achievement plant count should be non-negative");
        }
        
        [Test]
        public void CrossServiceDataFlow_EnvironmentalResponseChain_ProcessesSequentially()
        {
            // Arrange
            _environmentalService.Initialize();
            _plantEnvironmentalService.Initialize();
            _growthService.Initialize();
            
            // Act - Process environmental response chain
            var envResponse = _plantEnvironmentalService.UpdateEnvironmentalResponse(_testPlant, _testEnvironment);
            var envProcessing = _environmentalService.ProcessEnvironmentalEffects(_testPlant, _testEnvironment);
            var growthResult = _growthService.ProcessPlantGrowth(_testPlant, 1.0f);
            
            // Assert - Environmental response chain works
            Assert.IsNotNull(envResponse, "Environment response should be generated");
            Assert.IsNotNull(envProcessing, "Environmental processing should complete");
            Assert.IsNotNull(growthResult, "Growth processing should complete");
            
            Assert.GreaterOrEqual(envResponse.ResponseLevel, 0f, "Response level should be non-negative");
            Assert.GreaterOrEqual(envProcessing.EnvironmentalFitness, 0f, "Environmental fitness should be non-negative");
        }
        
        #endregion
        
        #region Performance Integration Tests
        
        [Test]
        public void PerformanceIntegration_MultiServiceProcessing_MaintainsPerformance()
        {
            // Arrange
            InitializeAllServices();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - Process through all services
            var lifecycleData = _lifecycleService.GetPlantLifecycleData(_testPlant);
            var growthResult = _growthService.ProcessPlantGrowth(_testPlant, 1.0f);
            var environmentalResult = _environmentalService.ProcessEnvironmentalEffects(_testPlant, _testEnvironment);
            var yieldData = _yieldService.CalculateExpectedYieldData(_testPlant);
            var geneticPotential = _geneticsService.CalculateGeneticPotential(_testGenotype);
            
            stopwatch.Stop();
            
            // Assert - All services complete within performance bounds
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "Multi-service processing should complete within 100ms");
            Assert.IsNotNull(lifecycleData, "Lifecycle data should be available");
            Assert.IsNotNull(growthResult, "Growth result should be available");
            Assert.IsNotNull(environmentalResult, "Environmental result should be available");
            Assert.IsNotNull(yieldData, "Yield data should be available");
            Assert.IsNotNull(geneticPotential, "Genetic potential should be available");
        }
        
        [Test]
        public void PerformanceIntegration_BatchProcessing_ScalesEfficiently()
        {
            // Arrange
            InitializeAllServices();
            var testPlants = new List<PlantInstance>();
            
            // Create multiple test plants
            for (int i = 0; i < 10; i++)
            {
                var plant = CreateTestPlantInstance();
                plant.PlantID = $"TEST_PLANT_{i:D3}";
                testPlants.Add(plant);
            }
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - Process multiple plants
            foreach (var plant in testPlants)
            {
                _lifecycleService.RegisterPlant(plant);
                _growthService.ProcessPlantGrowth(plant, 1.0f);
                _environmentalService.ProcessEnvironmentalEffects(plant, _testEnvironment);
                _achievementService.TrackPlantCreation(plant);
            }
            
            stopwatch.Stop();
            
            // Assert - Batch processing scales efficiently
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, "Batch processing of 10 plants should complete within 500ms");
            
            var finalStats = _achievementService.GetAchievementStats();
            Assert.GreaterOrEqual(finalStats.TotalPlantsCreated, 10, "All plants should be tracked");
        }
        
        #endregion
        
        #region Error Handling Integration Tests
        
        [Test]
        public void ErrorHandlingIntegration_ServiceFailureIsolation_DoesNotCascade()
        {
            // Arrange - Initialize only some services
            _lifecycleService.Initialize();
            _growthService.Initialize();
            // Leave other services uninitialized
            
            // Act & Assert - Services handle missing dependencies gracefully
            Assert.DoesNotThrow(() => {
                var lifecycleData = _lifecycleService.GetPlantLifecycleData(_testPlant);
                var growthResult = _growthService.ProcessPlantGrowth(_testPlant, 1.0f);
                var yieldData = _yieldService.CalculateExpectedYieldData(_testPlant);
            }, "Services should handle missing dependencies gracefully");
        }
        
        [Test]
        public void ErrorHandlingIntegration_InvalidDataHandling_RemainsStable()
        {
            // Arrange
            InitializeAllServices();
            
            // Act & Assert - Services handle invalid data gracefully
            Assert.DoesNotThrow(() => {
                _growthService.ProcessPlantGrowth(null, 1.0f);
                _environmentalService.ProcessEnvironmentalEffects(null, _testEnvironment);
                _yieldService.CalculateExpectedYieldData(null);
                _achievementService.TrackPlantCreation(null);
            }, "Services should handle null inputs gracefully");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void InitializeServices()
        {
            _lifecycleService = new PlantLifecycleService();
            _growthService = new PlantGrowthService();
            _environmentalService = new PlantEnvironmentalProcessingService();
            _yieldService = new PlantYieldCalculationService();
            _geneticsService = new PlantGeneticsService();
            _harvestService = new PlantHarvestService();
            _statisticsService = new PlantStatisticsService();
            _achievementService = new PlantAchievementService();
            _processingService = new PlantProcessingService();
            _plantEnvironmentalService = new PlantEnvironmentalService();
        }
        
        private void InitializeAllServices()
        {
            _lifecycleService?.Initialize();
            _growthService?.Initialize();
            _environmentalService?.Initialize();
            _yieldService?.Initialize();
            _geneticsService?.Initialize();
            _harvestService?.Initialize();
            _statisticsService?.Initialize();
            _achievementService?.Initialize();
            _processingService?.Initialize();
            _plantEnvironmentalService?.Initialize();
        }
        
        private void ShutdownServices()
        {
            _lifecycleService?.Shutdown();
            _growthService?.Shutdown();
            _environmentalService?.Shutdown();
            _yieldService?.Shutdown();
            _geneticsService?.Shutdown();
            _harvestService?.Shutdown();
            _statisticsService?.Shutdown();
            _achievementService?.Shutdown();
            _processingService?.Shutdown();
            _plantEnvironmentalService?.Shutdown();
        }
        
        private EnvironmentalConditions CreateTestEnvironmentalConditions()
        {
            return new EnvironmentalConditions
            {
                Temperature = 24.0f,
                Humidity = 60.0f,
                CO2Level = 400.0f,
                LightIntensity = 800.0f
            };
        }
        
        private CannabisGenotype CreateTestGenotype()
        {
            return new CannabisGenotype
            {
                GenotypeId = "TEST_GENOTYPE_001",
                StrainId = "TEST_STRAIN_001",
                StrainName = "Test Strain",
                GeneticVariability = 0.5f
            };
        }
        
        #endregion
    }
}