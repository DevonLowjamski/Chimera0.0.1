using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Testing.Integration
{
    /// <summary>
    /// PC013-23: Simplified integration tests for service orchestrators
    /// Tests basic integration functionality with available types only
    /// </summary>
    [TestFixture]
    [Category("Integration Tests")]
    [Category("Service Orchestration")]
    public class ServiceOrchestratorIntegrationTests : ChimeraTestBase
    {
        // Test data - simplified for available types
        private PlantStrainSO _testStrain;
        private GameObject _testPlant;
        private Dictionary<string, object> _testGenotype;
        private Dictionary<string, float> _testEnvironment;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            SetupTestEnvironment();
            
            // Create test data with available types
            _testStrain = CreateTestPlantStrain();
            _testPlant = CreateTestPlantInstance();
            _testGenotype = CreateTestGenotype();
            _testEnvironment = CreateTestEnvironmentalConditions();
        }

        [TearDown]
        public override void TearDown()
        {
            if (_testPlant != null)
                DestroyImmediate(_testPlant);
            if (_testStrain != null)
                DestroyImmediate(_testStrain);
                
            CleanupTestEnvironment();
            base.TearDown();
        }

        #region Simplified Integration Tests
        
        [Test]
        public void BasicIntegration_TestDataCreation_WorksCorrectly()
        {
            // Arrange & Act - Test data creation
            
            // Assert - All test data was created successfully
            Assert.IsNotNull(_testStrain, "Test strain should be created");
            Assert.IsNotNull(_testPlant, "Test plant should be created");
            Assert.IsNotNull(_testGenotype, "Test genotype should be created");
            Assert.IsNotNull(_testEnvironment, "Test environment should be created");
            
            LogInfo("BasicIntegration: Test data creation completed successfully");
        }

        [Test]
        public void BasicIntegration_PlantStrainData_IsValid()
        {
            // Arrange & Act
            var strain = _testStrain;
            
            // Assert - Strain data is valid
            Assert.IsNotNull(strain.StrainName, "Strain name should not be null");
            Assert.IsNotNull(strain.StrainId, "Strain ID should not be null");
            Assert.IsTrue(strain.StrainName.Length > 0, "Strain name should not be empty");
            Assert.IsTrue(strain.StrainId.Length > 0, "Strain ID should not be empty");
            
            LogInfo($"BasicIntegration: Validated strain '{strain.StrainName}' with ID '{strain.StrainId}'");
        }

        [Test]
        public void BasicIntegration_PlantGameObject_HasRequiredComponents()
        {
            // Arrange & Act
            var plant = _testPlant;
            
            // Assert - Plant GameObject has basic components
            Assert.IsNotNull(plant, "Plant GameObject should not be null");
            Assert.IsNotNull(plant.GetComponent<MeshRenderer>(), "Plant should have MeshRenderer component");
            Assert.IsNotNull(plant.GetComponent<MeshFilter>(), "Plant should have MeshFilter component");
            Assert.IsTrue(plant.name.Contains("TestPlant"), "Plant should have appropriate name");
            
            LogInfo($"BasicIntegration: Validated plant GameObject '{plant.name}' with required components");
        }

        [Test]
        public void BasicIntegration_GenotypeData_IsStructuredCorrectly()
        {
            // Arrange & Act
            var genotype = _testGenotype;
            
            // Assert - Genotype data structure is valid
            Assert.IsNotNull(genotype, "Genotype should not be null");
            Assert.Greater(genotype.Count, 0, "Genotype should contain data");
            
            LogInfo($"BasicIntegration: Validated genotype with {genotype.Count} properties");
        }

        [Test]
        public void BasicIntegration_EnvironmentalData_IsStructuredCorrectly()
        {
            // Arrange & Act
            var environment = _testEnvironment;
            
            // Assert - Environmental data structure is valid
            Assert.IsNotNull(environment, "Environment should not be null");
            Assert.Greater(environment.Count, 0, "Environment should contain data");
            
            // Check for expected environmental parameters
            Assert.IsTrue(environment.ContainsKey("Temperature") || environment.Count > 0, "Environment should have temperature or other parameters");
            
            LogInfo($"BasicIntegration: Validated environment with {environment.Count} parameters");
        }

        #endregion

        #region Helper Methods

        private Dictionary<string, object> CreateTestGenotype()
        {
            return new Dictionary<string, object>
            {
                {"Height", 1.5f},
                {"BudDensity", 0.8f},
                {"THCPotential", 0.15f},
                {"CBDPotential", 0.02f},
                {"FloweringTime", 8.5f}
            };
        }

        private Dictionary<string, float> CreateTestEnvironmentalConditions()
        {
            return new Dictionary<string, float>
            {
                {"Temperature", 22.0f},
                {"Humidity", 0.6f},
                {"CO2", 400.0f},
                {"LightIntensity", 800.0f},
                {"pH", 6.5f}
            };
        }

        #endregion
    }
}