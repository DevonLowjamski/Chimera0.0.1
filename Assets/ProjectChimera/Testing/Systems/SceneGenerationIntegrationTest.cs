using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using ProjectChimera.Systems.SceneGeneration;
using ProjectChimera.Data.Facilities;
using NUnit.Framework;

namespace ProjectChimera.Testing.Systems
{
    /// <summary>
    /// Integration tests for the refactored scene generation system
    /// Tests the ProceduralSceneOrchestrator and all generation services
    /// </summary>
    public class SceneGenerationIntegrationTest
    {
        private GameObject _testGameObject;
        private ProceduralSceneOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _testGameObject = new GameObject("SceneGenerationTest");
            _orchestrator = _testGameObject.AddComponent<ProceduralSceneOrchestrator>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
        }

        [UnityTest]
        public IEnumerator TestOrchestratorInitialization()
        {
            // Test that orchestrator can be initialized
            Assert.IsFalse(_orchestrator.IsInitialized, "Orchestrator should not be initialized initially");
            
            _orchestrator.Initialize();
            yield return null;
            
            Assert.IsTrue(_orchestrator.IsInitialized, "Orchestrator should be initialized after Initialize() call");
            Assert.AreEqual("Procedural Scene Orchestrator", _orchestrator.ServiceName, "Service name should be correct");
        }

        [UnityTest]
        public IEnumerator TestCompleteSceneGeneration()
        {
            // Initialize orchestrator
            _orchestrator.Initialize();
            yield return null;

            // Test complete scene generation
            var facilitySize = new Vector2Int(20, 20);
            var sceneType = FacilitySceneType.IndoorFacility;
            var seed = 12345;

            bool generationStarted = false;
            bool generationCompleted = false;

            // Subscribe to events
            ProceduralSceneOrchestrator.OnGenerationPhaseStarted += (phase) => generationStarted = true;
            ProceduralSceneOrchestrator.OnSceneGenerationCompleted += (report) => generationCompleted = true;

            // Start generation
            yield return _orchestrator.StartCoroutine(_orchestrator.GenerateCompleteScene(sceneType, facilitySize, seed));

            // Verify generation occurred
            Assert.IsTrue(generationStarted, "Generation should have started");
            Assert.IsTrue(generationCompleted, "Generation should have completed");
            Assert.IsFalse(_orchestrator.IsGenerating, "Should not be generating after completion");
        }

        [Test]
        public void TestPhaseEnableDisable()
        {
            _orchestrator.Initialize();
            
            // Test phase enable/disable functionality
            Assert.IsTrue(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Terrain), "Terrain phase should be enabled by default");
            
            _orchestrator.SetPhaseEnabled(SceneGenerationPhase.Terrain, false);
            Assert.IsFalse(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Terrain), "Terrain phase should be disabled after SetPhaseEnabled(false)");
            
            _orchestrator.SetPhaseEnabled(SceneGenerationPhase.Terrain, true);
            Assert.IsTrue(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Terrain), "Terrain phase should be enabled after SetPhaseEnabled(true)");
        }

        [Test]
        public void TestServiceInterface()
        {
            // Test that services implement ISceneGenerationService interface
            var terrainService = new GameObject().AddComponent<TerrainGenerationService>();
            Assert.IsNotNull(terrainService as ISceneGenerationService, "TerrainGenerationService should implement ISceneGenerationService");
            
            var buildingService = new GameObject().AddComponent<BuildingGenerationService>();
            Assert.IsNotNull(buildingService as ISceneGenerationService, "BuildingGenerationService should implement ISceneGenerationService");
            
            var vegetationService = new GameObject().AddComponent<VegetationGenerationService>();
            Assert.IsNotNull(vegetationService as ISceneGenerationService, "VegetationGenerationService should implement ISceneGenerationService");
            
            var equipmentService = new GameObject().AddComponent<EquipmentGenerationService>();
            Assert.IsNotNull(equipmentService as ISceneGenerationService, "EquipmentGenerationService should implement ISceneGenerationService");
            
            var environmentalService = new GameObject().AddComponent<EnvironmentalGenerationService>();
            Assert.IsNotNull(environmentalService as ISceneGenerationService, "EnvironmentalGenerationService should implement ISceneGenerationService");
            
            var detailService = new GameObject().AddComponent<DetailGenerationService>();
            Assert.IsNotNull(detailService as ISceneGenerationService, "DetailGenerationService should implement ISceneGenerationService");
            
            // Clean up test objects
            Object.DestroyImmediate(terrainService.gameObject);
            Object.DestroyImmediate(buildingService.gameObject);
            Object.DestroyImmediate(vegetationService.gameObject);
            Object.DestroyImmediate(equipmentService.gameObject);
            Object.DestroyImmediate(environmentalService.gameObject);
            Object.DestroyImmediate(detailService.gameObject);
        }

        [Test]
        public void TestConfigurationUpdate()
        {
            _orchestrator.Initialize();
            
            var config = new SceneGenerationConfig
            {
                EnableTerrain = false,
                EnableBuildings = false,
                EnableVegetation = true,
                EnableEquipment = true,
                EnableEnvironmental = false,
                EnableDetails = true,
                PhaseTransitionDelay = 0.05f,
                MaxGenerationTime = 30f,
                ShowProgressUpdates = false
            };
            
            _orchestrator.UpdateGenerationConfiguration(config);
            
            // Verify configuration was applied
            Assert.IsFalse(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Terrain), "Terrain should be disabled");
            Assert.IsFalse(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Buildings), "Buildings should be disabled");
            Assert.IsTrue(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Vegetation), "Vegetation should be enabled");
            Assert.IsTrue(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Equipment), "Equipment should be enabled");
            Assert.IsFalse(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Environmental), "Environmental should be disabled");
            Assert.IsTrue(_orchestrator.IsPhaseEnabled(SceneGenerationPhase.Details), "Details should be enabled");
        }
    }
}