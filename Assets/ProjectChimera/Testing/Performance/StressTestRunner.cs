using UnityEngine;
using System.Collections;
using ProjectChimera.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// Simple runner script to execute plant stress tests programmatically
    /// Attach this to a GameObject in your scene to run stress tests
    /// Updated to fix compilation errors
    /// </summary>
    public class StressTestRunner : MonoBehaviour
    {
        [Header("Stress Test Configuration")]
        [SerializeField] private bool _runOnStart = false;
        [SerializeField] private bool _runAllTests = true;
        [SerializeField] private bool _runGradualLoad = true;
        [SerializeField] private bool _runSuddenLoad = true;
        [SerializeField] private bool _runMemoryTest = true;
        [SerializeField] private bool _runSustainedLoad = true;
        
        [Header("Runtime Controls")]
        [SerializeField] private KeyCode _startTestsKey = KeyCode.F1;
        [SerializeField] private KeyCode _stopTestsKey = KeyCode.F2;
        
        private PlantStressTestingFramework _stressFramework;
        private bool _testsRunning = false;
        
        private void Start()
        {
            InitializeStressFramework();
            
            if (_runOnStart)
            {
                StartCoroutine(RunStressTests());
            }
        }
        
        private void Update()
        {
            // Keyboard controls for runtime testing
            if (Input.GetKeyDown(_startTestsKey) && !_testsRunning)
            {
                StartCoroutine(RunStressTests());
            }
            
            if (Input.GetKeyDown(_stopTestsKey) && _testsRunning)
            {
                StopStressTests();
            }
        }
        
        private void InitializeStressFramework()
        {
            // Create stress testing framework if it doesn't exist
            var existingFramework = FindObjectOfType<PlantStressTestingFramework>();
            
            if (existingFramework == null)
            {
                var frameworkGO = new GameObject("PlantStressTestingFramework");
                _stressFramework = frameworkGO.AddComponent<PlantStressTestingFramework>();
            }
            else
            {
                _stressFramework = existingFramework;
            }
            
            Debug.Log($"[StressTestRunner] Framework initialized. Press {_startTestsKey} to start tests.");
        }
        
        /// <summary>
        /// Run stress tests based on configuration
        /// </summary>
        public IEnumerator RunStressTests()
        {
            if (_testsRunning) yield break;
            
            _testsRunning = true;
            Debug.Log("[StressTestRunner] Starting stress tests...");
            
            if (_stressFramework == null)
            {
                Debug.LogError("[StressTestRunner] Stress framework not initialized!");
                _testsRunning = false;
                yield break;
            }
            
            if (_runAllTests)
            {
                // Run the comprehensive test suite
                yield return _stressFramework.RunAllStressTests();
            }
            else
            {
                // Run individual tests based on configuration
                if (_runGradualLoad)
                {
                    Debug.Log("[StressTestRunner] Running Gradual Load Test...");
                    yield return _stressFramework.GradualLoadStressTest();
                    yield return new WaitForSeconds(2f);
                }
                
                if (_runSuddenLoad)
                {
                    Debug.Log("[StressTestRunner] Running Sudden Load Test...");
                    yield return _stressFramework.SuddenLoadStressTest();
                    yield return new WaitForSeconds(2f);
                }
                
                if (_runMemoryTest)
                {
                    Debug.Log("[StressTestRunner] Running Memory Stress Test...");
                    yield return _stressFramework.MemoryStressTest();
                    yield return new WaitForSeconds(2f);
                }
                
                if (_runSustainedLoad)
                {
                    Debug.Log("[StressTestRunner] Running Sustained Load Test...");
                    yield return _stressFramework.SustainedLoadStressTest();
                }
                
                // Generate final report
                _stressFramework.GenerateStressTestReport();
            }
            
            Debug.Log("[StressTestRunner] All stress tests completed!");
            _testsRunning = false;
        }
        
        /// <summary>
        /// Stop running stress tests
        /// </summary>
        public void StopStressTests()
        {
            if (_testsRunning)
            {
                StopAllCoroutines();
                _testsRunning = false;
                Debug.Log("[StressTestRunner] Stress tests stopped.");
            }
        }
        
        /// <summary>
        /// Public method to start tests from UI or other scripts
        /// </summary>
        [ContextMenu("Start Stress Tests")]
        public void StartStressTestsManual()
        {
            if (!_testsRunning)
            {
                StartCoroutine(RunStressTests());
            }
        }
        
        /// <summary>
        /// Run a specific stress test scenario
        /// </summary>
        public void RunSpecificTest(string testName)
        {
            if (_testsRunning) return;
            
            StartCoroutine(RunSpecificTestCoroutine(testName));
        }
        
        private IEnumerator RunSpecificTestCoroutine(string testName)
        {
            _testsRunning = true;
            Debug.Log($"[StressTestRunner] Running specific test: {testName}");
            
            switch (testName.ToLower())
            {
                case "gradual":
                case "gradualload":
                    yield return _stressFramework.GradualLoadStressTest();
                    break;
                    
                case "sudden":
                case "suddenload":
                    yield return _stressFramework.SuddenLoadStressTest();
                    break;
                    
                case "memory":
                case "memorytest":
                    yield return _stressFramework.MemoryStressTest();
                    break;
                    
                case "sustained":
                case "sustainedload":
                    yield return _stressFramework.SustainedLoadStressTest();
                    break;
                    
                default:
                    Debug.LogWarning($"[StressTestRunner] Unknown test name: {testName}");
                    break;
            }
            
            _stressFramework.GenerateStressTestReport();
            _testsRunning = false;
        }
        
        private void OnGUI()
        {
            // Simple runtime UI for testing
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Plant Stress Testing Framework");
            
            if (!_testsRunning)
            {
                if (GUILayout.Button("Run All Stress Tests"))
                {
                    StartStressTestsManual();
                }
                
                GUILayout.Space(10);
                GUILayout.Label("Individual Tests:");
                
                if (GUILayout.Button("Gradual Load Test"))
                    RunSpecificTest("gradual");
                if (GUILayout.Button("Sudden Load Test"))
                    RunSpecificTest("sudden");
                if (GUILayout.Button("Memory Stress Test"))
                    RunSpecificTest("memory");
                if (GUILayout.Button("Sustained Load Test"))
                    RunSpecificTest("sustained");
            }
            else
            {
                GUILayout.Label("Tests Running...");
                if (GUILayout.Button("Stop Tests"))
                {
                    StopStressTests();
                }
            }
            
            GUILayout.EndArea();
        }
    }
}