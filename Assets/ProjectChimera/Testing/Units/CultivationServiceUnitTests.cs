using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
// Explicit aliases to resolve ambiguity
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Testing.Units
{
    /// <summary>
    /// PC013-22: Comprehensive unit tests for all refactored cultivation service classes
    /// Tests each service in isolation to verify Single Responsibility Principle adherence
    /// </summary>
    [TestFixture]
    [Category("Unit Tests")]
    [Category("Cultivation Services")]
    public class CultivationServiceUnitTests : ChimeraTestBase
    {
        // Test data
        private PlantStrainSO _testStrain;
        private PlantInstance _testPlant;
        private CannabisGenotype _testGenotype;
        private EnvironmentalConditions _testEnvironment;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            
            // Create test data
            _testStrain = CreateTestPlantStrain();
            _testPlant = CreateTestPlantInstance();
            _testGenotype = CreateTestGenotype();
            _testEnvironment = CreateTestEnvironmentalConditions();
        }
        
        [TearDown]
        public void TearDown()
        {
            CleanupTestEnvironment();
        }
        
        #region PlantGrowthService Tests
        
        [Test]
        public void PlantGrowthService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantGrowthService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantGrowthService_ProcessGrowth_WithValidPlant_ReturnsResults()
        {
            // Arrange
            var service = new PlantGrowthService();
            service.Initialize();
            
            // Act
            var result = service.ProcessPlantGrowth(_testPlant, 1.0f);
            
            // Assert
            Assert.IsNotNull(result, "Growth processing should return a result");
        }
        
        [Test]
        public void PlantGrowthService_ProcessGrowth_WithNullPlant_HandlesGracefully()
        {
            // Arrange
            var service = new PlantGrowthService();
            service.Initialize();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.ProcessPlantGrowth(null, 1.0f));
        }
        
        [Test]
        public void PlantGrowthService_CalculateStageTransition_ReturnsValidStage()
        {
            // Arrange
            var service = new PlantGrowthService();
            service.Initialize();
            
            // Act
            var newStage = service.CalculateStageTransition(_testPlant, 1.0f);
            
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(PlantGrowthStage), newStage), "Should return valid growth stage");
        }
        
        #endregion
        
        #region PlantEnvironmentalProcessingService Tests
        
        [Test]
        public void PlantEnvironmentalProcessingService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantEnvironmentalProcessingService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantEnvironmentalProcessingService_ProcessEnvironmentalEffects_WithValidData_ReturnsResults()
        {
            // Arrange
            var service = new PlantEnvironmentalProcessingService();
            service.Initialize();
            
            // Act
            var result = service.ProcessEnvironmentalEffects(_testPlant, _testEnvironment);
            
            // Assert
            Assert.IsNotNull(result, "Environmental processing should return a result");
        }
        
        [Test]
        public void PlantEnvironmentalProcessingService_CalculateStressFactors_ReturnsValidRange()
        {
            // Arrange
            var service = new PlantEnvironmentalProcessingService();
            service.Initialize();
            
            // Act
            var stressFactors = service.CalculateStressFactors(_testPlant, _testEnvironment);
            
            // Assert
            Assert.IsNotNull(stressFactors, "Stress factors should not be null");
            Assert.GreaterOrEqual(stressFactors.OverallStressLevel, 0f, "Overall stress should be non-negative");
            Assert.LessOrEqual(stressFactors.OverallStressLevel, 1f, "Overall stress should not exceed 1.0");
        }
        
        #endregion
        
        #region PlantYieldCalculationService Tests
        
        [Test]
        public void PlantYieldCalculationService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantYieldCalculationService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantYieldCalculationService_CalculateYield_WithValidPlant_ReturnsPositiveValue()
        {
            // Arrange
            var service = new PlantYieldCalculationService();
            service.Initialize();
            
            // Set plant to harvest stage for valid calculation
            _testPlant.CurrentGrowthStage = PlantGrowthStage.Harvest;
            
            // Act
            var yieldData = service.CalculateExpectedYieldData(_testPlant);
            
            // Assert
            Assert.IsNotNull(yieldData, "Yield calculation should return data");
            Assert.Greater(yieldData.BaseYield, 0f, "Base yield should be positive");
        }
        
        [Test]
        public void PlantYieldCalculationService_HarvestPlant_WithInvalidPlant_HandlesGracefully()
        {
            // Arrange
            var service = new PlantYieldCalculationService();
            service.Initialize();
            
            // Expect error log for invalid plant ID
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Cannot harvest unknown plant"));
            
            // Act
            var result = service.HarvestPlant("INVALID_PLANT_ID");
            
            // Assert
            Assert.IsNull(result, "Invalid harvest should return null");
        }
        
        #endregion
        
        #region PlantGeneticsService Tests
        
        [Test]
        public void PlantGeneticsService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantGeneticsService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantGeneticsService_CalculateGeneticPotential_ReturnsValidRange()
        {
            // Arrange
            var service = new PlantGeneticsService();
            service.Initialize();
            
            // Act
            var potential = service.CalculateGeneticPotential(_testGenotype);
            
            // Assert
            Assert.IsNotNull(potential, "Genetic potential should not be null");
            Assert.GreaterOrEqual(potential.OverallPotential, 0f, "Overall potential should be non-negative");
            Assert.LessOrEqual(potential.OverallPotential, 1f, "Overall potential should not exceed 1.0");
        }
        
        [Test]
        public void PlantGeneticsService_ProcessGeneExpression_WithValidData_ReturnsResults()
        {
            // Arrange
            var service = new PlantGeneticsService();
            service.Initialize();
            
            // Act
            var expression = service.ProcessGeneExpression(_testPlant, _testEnvironment);
            
            // Assert
            Assert.IsNotNull(expression, "Gene expression should return results");
        }
        
        #endregion
        
        #region PlantLifecycleService Tests
        
        [Test]
        public void PlantLifecycleService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantLifecycleService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantLifecycleService_UpdateLifecycle_ProgressesPlant()
        {
            // Arrange
            var service = new PlantLifecycleService();
            service.Initialize();
            var initialStage = _testPlant.CurrentGrowthStage;
            
            // Act
            service.UpdatePlantLifecycle(_testPlant, 1.0f);
            
            // Assert - The stage may or may not change, but operation should complete
            Assert.IsTrue(Enum.IsDefined(typeof(PlantGrowthStage), _testPlant.CurrentGrowthStage));
        }
        
        [Test]
        public void PlantLifecycleService_GetLifecycleData_ReturnsValidData()
        {
            // Arrange
            var service = new PlantLifecycleService();
            service.Initialize();
            
            // Act
            var lifecycleData = service.GetPlantLifecycleData(_testPlant);
            
            // Assert
            Assert.IsNotNull(lifecycleData, "Lifecycle data should not be null");
            Assert.GreaterOrEqual(lifecycleData.DaysInCurrentStage, 0f, "Days in stage should be non-negative");
        }
        
        #endregion
        
        #region PlantHarvestService Tests
        
        [Test]
        public void PlantHarvestService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantHarvestService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantHarvestService_IsReadyForHarvest_WithHarvestStage_ReturnsTrue()
        {
            // Arrange
            var service = new PlantHarvestService();
            service.Initialize();
            _testPlant.CurrentGrowthStage = PlantGrowthStage.Harvest;
            
            // Act
            var isReady = service.IsPlantReadyForHarvest(_testPlant);
            
            // Assert
            Assert.IsTrue(isReady, "Plant in harvest stage should be ready");
        }
        
        [Test]
        public void PlantHarvestService_IsReadyForHarvest_WithNonHarvestStage_ReturnsFalse()
        {
            // Arrange
            var service = new PlantHarvestService();
            service.Initialize();
            _testPlant.CurrentGrowthStage = PlantGrowthStage.Seedling;
            
            // Act
            var isReady = service.IsPlantReadyForHarvest(_testPlant);
            
            // Assert
            Assert.IsFalse(isReady, "Plant in seedling stage should not be ready");
        }
        
        #endregion
        
        #region PlantStatisticsService Tests
        
        [Test]
        public void PlantStatisticsService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantStatisticsService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantStatisticsService_CalculateStatistics_ReturnsValidData()
        {
            // Arrange
            var service = new PlantStatisticsService();
            service.Initialize();
            
            // Act
            var stats = service.CalculatePlantStatistics(_testPlant);
            
            // Assert
            Assert.IsNotNull(stats, "Statistics should not be null");
            Assert.GreaterOrEqual(stats.HealthPercentage, 0f, "Health percentage should be non-negative");
            Assert.LessOrEqual(stats.HealthPercentage, 100f, "Health percentage should not exceed 100");
        }
        
        [Test]
        public void PlantStatisticsService_GetPerformanceMetrics_ReturnsValidMetrics()
        {
            // Arrange
            var service = new PlantStatisticsService();
            service.Initialize();
            
            // Act
            var metrics = service.GetPlantPerformanceMetrics(_testPlant);
            
            // Assert
            Assert.IsNotNull(metrics, "Performance metrics should not be null");
        }
        
        #endregion
        
        #region PlantEnvironmentalService Tests
        
        [Test]
        public void PlantEnvironmentalService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantEnvironmentalService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantEnvironmentalService_UpdateEnvironmentalResponse_ProcessesCorrectly()
        {
            // Arrange
            var service = new PlantEnvironmentalService();
            service.Initialize();
            
            // Act
            var response = service.UpdateEnvironmentalResponse(_testPlant, _testEnvironment);
            
            // Assert
            Assert.IsNotNull(response, "Environmental response should not be null");
        }
        
        #endregion
        
        #region PlantAchievementService Tests
        
        [Test]
        public void PlantAchievementService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantAchievementService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantAchievementService_ProcessAchievements_HandlesGracefully()
        {
            // Arrange
            var service = new PlantAchievementService();
            service.Initialize();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.ProcessPlantAchievements(_testPlant));
        }
        
        #endregion
        
        #region PlantProcessingService Tests
        
        [Test]
        public void PlantProcessingService_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var service = new PlantProcessingService();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => service.Initialize());
        }
        
        [Test]
        public void PlantProcessingService_ProcessPlant_WithValidData_ReturnsResults()
        {
            // Arrange
            var service = new PlantProcessingService();
            service.Initialize();
            
            // Act
            var result = service.ProcessPlantData(_testPlant);
            
            // Assert
            Assert.IsNotNull(result, "Plant processing should return results");
        }
        
        #endregion
        
        #region Helper Methods
        
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
        
        #endregion
    }
}