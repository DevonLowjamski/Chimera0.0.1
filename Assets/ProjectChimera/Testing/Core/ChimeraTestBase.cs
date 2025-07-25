using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProjectChimera.Core;

namespace ProjectChimera.Testing.Core
{
    /// <summary>
    /// Base class for all Project Chimera test classes.
    /// Provides common testing functionality and utilities.
    /// </summary>
    public abstract class ChimeraTestBase : MonoBehaviour
    {
        protected GameManager _gameManager;
        protected bool _testInitialized = false;
        
        [SetUp]
        public virtual void SetUp()
        {
            InitializeTest();
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            CleanupTest();
        }
        
        protected virtual void InitializeTest()
        {
            if (!_testInitialized)
            {
                // Find or create GameManager for testing
                _gameManager = FindObjectOfType<GameManager>();
                if (_gameManager == null)
                {
                    var gameManagerGO = new GameObject("TestGameManager");
                    _gameManager = gameManagerGO.AddComponent<GameManager>();
                }
                
                _testInitialized = true;
            }
        }
        
        protected virtual void CleanupTest()
        {
            // Clean up any test-specific resources
        }
        
        protected void LogTestResult(string testName, bool passed, string details = "")
        {
            string result = passed ? "PASSED" : "FAILED";
            Debug.Log($"[TEST {result}] {testName}: {details}");
        }
        
        protected void LogInfo(string message)
        {
            Debug.Log($"[TEST INFO] {message}");
        }
        
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[TEST WARNING] {message}");
        }
        
        protected void LogError(string message)
        {
            Debug.LogError($"[TEST ERROR] {message}");
        }
        
        protected IEnumerator WaitForCondition(System.Func<bool> condition, float timeoutSeconds = 5f)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
            
            if (elapsed >= timeoutSeconds)
            {
                Debug.LogWarning($"Test condition timed out after {timeoutSeconds} seconds");
            }
        }
        
        protected virtual void SetupTestEnvironment()
        {
            // Initialize test environment
            InitializeTest();
        }
        
        protected virtual T CreateTestManager<T>() where T : ChimeraManager
        {
            var gameObject = new GameObject($"Test{typeof(T).Name}");
            var manager = gameObject.AddComponent<T>();
            return manager;
        }
        
        protected virtual void CleanupTestEnvironment()
        {
            // Clean up test environment
            CleanupTest();
        }
        
        // Helper methods for creating test data
        protected virtual ProjectChimera.Data.Genetics.PlantStrainSO CreateTestPlantStrain()
        {
            var strain = ScriptableObject.CreateInstance<ProjectChimera.Data.Genetics.PlantStrainSO>();
            strain.name = "TestStrain";
            strain.StrainName = "Test Cannabis Strain";
            strain.StrainId = "TEST_STRAIN_001";
            return strain;
        }
        
        protected virtual GameObject CreateTestPlantInstance()
        {
            var plantGO = new GameObject("TestPlant");
            var testStrain = CreateTestPlantStrain();
            // Plant instances are now GameObjects with components
            // Add basic components that would be on a real plant
            plantGO.AddComponent<MeshRenderer>();
            plantGO.AddComponent<MeshFilter>();
            return plantGO;
        }
    }
}