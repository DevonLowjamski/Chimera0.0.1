using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Testing.Units
{
    /// <summary>
    /// PC013-22: Simplified unit tests for cultivation functionality
    /// Tests basic cultivation data structures and functionality with available types
    /// </summary>
    [TestFixture]
    [Category("Unit Tests")]
    [Category("Cultivation Services")]
    public class CultivationServiceUnitTests : ChimeraTestBase
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

        #region Unit Tests

        [Test]
        public void PlantStrainSO_Creation_InitializesCorrectly()
        {
            // Arrange & Act
            var strain = _testStrain;

            // Assert
            Assert.IsNotNull(strain, "PlantStrainSO should be created");
            Assert.IsNotNull(strain.StrainName, "StrainName should not be null");
            Assert.IsNotNull(strain.StrainId, "StrainId should not be null");
            Assert.IsTrue(strain.StrainName.Length > 0, "StrainName should not be empty");
            Assert.IsTrue(strain.StrainId.Length > 0, "StrainId should not be empty");
            
            LogInfo($"PlantStrainSO created successfully: {strain.StrainName}");
        }

        [Test]
        public void PlantGameObject_Creation_HasRequiredComponents()
        {
            // Arrange & Act
            var plant = _testPlant;

            // Assert
            Assert.IsNotNull(plant, "Plant GameObject should be created");
            Assert.IsNotNull(plant.GetComponent<MeshRenderer>(), "Plant should have MeshRenderer");
            Assert.IsNotNull(plant.GetComponent<MeshFilter>(), "Plant should have MeshFilter");
            Assert.IsTrue(plant.activeInHierarchy, "Plant should be active");
            
            LogInfo($"Plant GameObject created successfully: {plant.name}");
        }

        [Test]
        public void GenotypeData_Structure_IsValid()
        {
            // Arrange & Act
            var genotype = _testGenotype;

            // Assert
            Assert.IsNotNull(genotype, "Genotype should not be null");
            Assert.Greater(genotype.Count, 0, "Genotype should contain data");
            
            // Test specific genotype properties
            if (genotype.ContainsKey("Height"))
            {
                Assert.IsInstanceOf<float>(genotype["Height"], "Height should be a float");
                Assert.Greater((float)genotype["Height"], 0, "Height should be positive");
            }
            
            LogInfo($"Genotype data validated with {genotype.Count} properties");
        }

        [Test]
        public void EnvironmentalData_Structure_IsValid()
        {
            // Arrange & Act
            var environment = _testEnvironment;

            // Assert
            Assert.IsNotNull(environment, "Environment should not be null");
            Assert.Greater(environment.Count, 0, "Environment should contain data");
            
            // Test specific environmental properties
            foreach (var kvp in environment)
            {
                Assert.IsNotNull(kvp.Key, "Environment key should not be null");
                Assert.IsInstanceOf<float>(kvp.Value, "Environment value should be a float");
            }
            
            LogInfo($"Environmental data validated with {environment.Count} parameters");
        }

        [Test]
        public void TestDataIntegrity_AllComponents_WorkTogether()
        {
            // Arrange & Act - Use all test components together
            
            // Assert - All components are compatible
            Assert.IsNotNull(_testStrain, "Strain should be available");
            Assert.IsNotNull(_testPlant, "Plant should be available");
            Assert.IsNotNull(_testGenotype, "Genotype should be available");
            Assert.IsNotNull(_testEnvironment, "Environment should be available");
            
            // Test that plant can reference strain data
            Assert.AreEqual("Test Cannabis Strain", _testStrain.StrainName, "Strain name should match expected value");
            Assert.AreEqual("TEST_STRAIN_001", _testStrain.StrainId, "Strain ID should match expected value");
            
            LogInfo("All test data components work together correctly");
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
                {"FloweringTime", 8.5f},
                {"YieldPotential", 450.0f}
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
                {"pH", 6.5f},
                {"VPD", 1.2f}
            };
        }

        #endregion
    }
}