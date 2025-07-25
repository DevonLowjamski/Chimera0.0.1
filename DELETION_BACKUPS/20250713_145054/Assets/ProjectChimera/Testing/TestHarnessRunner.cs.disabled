
using UnityEngine;
using System.Collections;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Orchestrates integration tests within the Test Harness scene.
    /// This class is responsible for setting up test conditions, running the simulation,
    /// and reporting the results.
    /// </summary>
    public class TestHarnessRunner : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private int _simulationDays = 20;
        [SerializeField] private float _timeScale = 100f; // Run simulation quickly

        [Header("Test Assets")]
        [SerializeField] private PlantStrainSO _testStrain;

        private CultivationManager _cultivationManager;

        void Start()
        {
            // Automatically start the test when the scene loads in a development build
            #if DEVELOPMENT_BUILD
            StartCoroutine(RunTestRoutine());
            #endif
        }

        [ContextMenu("Run Integration Test")]
        public void RunTest()
        {
            StartCoroutine(RunTestRoutine());
        }

        private IEnumerator RunTestRoutine()
        {
            Debug.Log("--- TEST HARNESS: INITIALIZING ---");

            // 1. Setup Test Environment
            if (!SetupEnvironment())
            {
                Debug.LogError("--- TEST HARNESS: FAILED TO SETUP ENVIRONMENT. ABORTING. ---");
                yield break;
            }

            Debug.Log("--- TEST HARNESS: SETUP COMPLETE. STARTING SIMULATION. ---");

            // 2. Run Simulation
            yield return RunSimulation();

            Debug.Log("--- TEST HARNESS: SIMULATION COMPLETE. ANALYZING RESULTS. ---");

            // 3. Analyze Results
            AnalyzeResults();

            Debug.Log("--- TEST HARNESS: TEST COMPLETE. ---");
        }

        private bool SetupEnvironment()
        {
            // Find the CultivationManager
            _cultivationManager = FindObjectOfType<CultivationManager>();
            if (_cultivationManager == null)
            {
                Debug.LogError("CultivationManager not found in the scene. Please add it.");
                return false;
            }

            // Set a high time scale for fast testing
            _cultivationManager.TimeAcceleration = _timeScale;

            // TODO: Clear any existing plants from previous test runs

            // TODO: Instantiate test plants based on test configuration

            return true;
        }

        private IEnumerator RunSimulation()
        {
            float simulationSeconds = _simulationDays * (86400f / _cultivationManager.TimeAcceleration);
            float startTime = Time.time;
            float elapsedTime = 0f;

            while (elapsedTime < simulationSeconds)
            {
                elapsedTime = Time.time - startTime;
                // The CultivationManager's Update loop will handle the growth simulation
                yield return null;
            }
        }

        private void AnalyzeResults()
        {
            Debug.Log("--- Analyzing Test Results ---");
            // TODO: Implement result analysis logic
            // Example:
            // var plants = _cultivationManager.GetAllPlants();
            // foreach(var plant in plants)
            // {
            //     Debug.Log($"Plant {plant.PlantID}: Height = {plant.CurrentHeight}cm, Health = {plant.OverallHealth}");
            // }
        }
    }
}
