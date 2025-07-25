using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Simple UI for running PC013 tests within Unity
    /// Provides easy access to test framework functionality
    /// </summary>
    public class PC013TestRunnerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _runAllTestsButton;
        [SerializeField] private Button _runDITestsButton;
        [SerializeField] private Button _runOptimizationTestsButton;
        [SerializeField] private Button _runPoolingTestsButton;
        [SerializeField] private Button _runValidationButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _resultsText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private GameObject _progressPanel;
        
        [Header("Test Configuration")]
        [SerializeField] private bool _autoCreateTestFramework = true;
        [SerializeField] private bool _autoCreateValidationSuite = true;
        
        // Component references
        private PC013RefactoringTestFramework _testFramework;
        private PC013ValidationSuite _validationSuite;
        
        // UI State
        private bool _testsRunning = false;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeUI();
            
            if (_autoCreateTestFramework)
            {
                CreateTestFramework();
            }
            
            if (_autoCreateValidationSuite)
            {
                CreateValidationSuite();
            }
        }
        
        #endregion
        
        #region UI Initialization
        
        private void InitializeUI()
        {
            // Setup button listeners
            if (_runAllTestsButton != null)
                _runAllTestsButton.onClick.AddListener(RunAllTests);
                
            if (_runDITestsButton != null)
                _runDITestsButton.onClick.AddListener(RunDITests);
                
            if (_runOptimizationTestsButton != null)
                _runOptimizationTestsButton.onClick.AddListener(RunOptimizationTests);
                
            if (_runPoolingTestsButton != null)
                _runPoolingTestsButton.onClick.AddListener(RunPoolingTests);
                
            if (_runValidationButton != null)
                _runValidationButton.onClick.AddListener(RunValidation);
            
            // Initialize UI state
            UpdateStatusText("PC013 Test Runner Ready");
            if (_progressPanel != null)
                _progressPanel.SetActive(false);
                
            if (_resultsText != null)
                _resultsText.text = "No tests run yet";
        }
        
        #endregion
        
        #region Component Creation
        
        private void CreateTestFramework()
        {
            if (_testFramework == null)
            {
                _testFramework = PC013RefactoringTestFramework.Instance;
                
                if (_testFramework == null)
                {
                    var testGO = new GameObject("PC013TestFramework");
                    testGO.transform.SetParent(transform);
                    _testFramework = testGO.AddComponent<PC013RefactoringTestFramework>();
                }
            }
        }
        
        private void CreateValidationSuite()
        {
            if (_validationSuite == null)
            {
                _validationSuite = PC013ValidationSuite.Instance;
                
                if (_validationSuite == null)
                {
                    var validationGO = new GameObject("PC013ValidationSuite");
                    validationGO.transform.SetParent(transform);
                    _validationSuite = validationGO.AddComponent<PC013ValidationSuite>();
                }
            }
        }
        
        #endregion
        
        #region Test Execution
        
        public void RunAllTests()
        {
            if (_testsRunning)
            {
                UpdateStatusText("Tests already running...");
                return;
            }
            
            if (_testFramework == null)
            {
                CreateTestFramework();
            }
            
            StartCoroutine(ExecuteAllTests());
        }
        
        public void RunDITests()
        {
            if (_testsRunning) return;
            
            if (_testFramework == null)
            {
                CreateTestFramework();
            }
            
            StartCoroutine(ExecuteDITests());
        }
        
        public void RunOptimizationTests()
        {
            if (_testsRunning) return;
            
            if (_testFramework == null)
            {
                CreateTestFramework();
            }
            
            StartCoroutine(ExecuteOptimizationTests());
        }
        
        public void RunPoolingTests()
        {
            if (_testsRunning) return;
            
            if (_testFramework == null)
            {
                CreateTestFramework();
            }
            
            StartCoroutine(ExecutePoolingTests());
        }
        
        public void RunValidation()
        {
            if (_testsRunning) return;
            
            if (_validationSuite == null)
            {
                CreateValidationSuite();
            }
            
            StartCoroutine(ExecuteValidation());
        }
        
        #endregion
        
        #region Test Coroutines
        
        private IEnumerator ExecuteAllTests()
        {
            _testsRunning = true;
            ShowProgress(true);
            UpdateStatusText("Running comprehensive PC013 tests...");
            UpdateProgress(0f);
            
            // Configure for all tests
            _testFramework._enableDependencyInjectionTests = true;
            _testFramework._enableOptimizationTests = true;
            _testFramework._enablePoolingTests = true;
            _testFramework._enablePerformanceTests = true;
            
            yield return StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            DisplayTestResults(results);
            
            UpdateProgress(1f);
            UpdateStatusText("All tests completed");
            ShowProgress(false);
            _testsRunning = false;
        }
        
        private IEnumerator ExecuteDITests()
        {
            _testsRunning = true;
            ShowProgress(true);
            UpdateStatusText("Running Dependency Injection tests...");
            
            // Configure for DI tests only
            _testFramework._enableDependencyInjectionTests = true;
            _testFramework._enableOptimizationTests = false;
            _testFramework._enablePoolingTests = false;
            _testFramework._enablePerformanceTests = false;
            
            yield return StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            DisplayDIResults(results);
            
            UpdateStatusText("DI tests completed");
            ShowProgress(false);
            _testsRunning = false;
        }
        
        private IEnumerator ExecuteOptimizationTests()
        {
            _testsRunning = true;
            ShowProgress(true);
            UpdateStatusText("Running Optimization tests...");
            
            // Configure for optimization tests only
            _testFramework._enableDependencyInjectionTests = false;
            _testFramework._enableOptimizationTests = true;
            _testFramework._enablePoolingTests = false;
            _testFramework._enablePerformanceTests = false;
            
            yield return StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            DisplayOptimizationResults(results);
            
            UpdateStatusText("Optimization tests completed");
            ShowProgress(false);
            _testsRunning = false;
        }
        
        private IEnumerator ExecutePoolingTests()
        {
            _testsRunning = true;
            ShowProgress(true);
            UpdateStatusText("Running Pooling tests...");
            
            // Configure for pooling tests only
            _testFramework._enableDependencyInjectionTests = false;
            _testFramework._enableOptimizationTests = false;
            _testFramework._enablePoolingTests = true;
            _testFramework._enablePerformanceTests = false;
            
            yield return StartCoroutine(_testFramework.RunAllTests());
            
            var results = _testFramework.GetTestResults();
            DisplayPoolingResults(results);
            
            UpdateStatusText("Pooling tests completed");
            ShowProgress(false);
            _testsRunning = false;
        }
        
        private IEnumerator ExecuteValidation()
        {
            _testsRunning = true;
            ShowProgress(true);
            UpdateStatusText("Running system validation...");
            
            yield return StartCoroutine(_validationSuite.RunFullValidation());
            
            var results = _validationSuite.GetLatestValidationResults();
            DisplayValidationResults(results);
            
            UpdateStatusText("Validation completed");
            ShowProgress(false);
            _testsRunning = false;
        }
        
        #endregion
        
        #region Result Display
        
        private void DisplayTestResults(PC013TestResults results)
        {
            if (_resultsText == null) return;
            
            var resultText = $"PC013 TEST RESULTS ({results.TotalTestDuration:F2}s)\n\n";
            
            if (results.DependencyInjectionResults != null)
            {
                var di = results.DependencyInjectionResults;
                resultText += $"Dependency Injection: {di.TestsPassed}/{di.TotalTests} passed\n";
            }
            
            if (results.OptimizationResults != null)
            {
                var opt = results.OptimizationResults;
                resultText += $"Optimization: {opt.TestsPassed}/{opt.TotalTests} passed\n";
            }
            
            if (results.PoolingResults != null)
            {
                var pool = results.PoolingResults;
                resultText += $"Pooling: {pool.TestsPassed}/{pool.TotalTests} passed\n";
            }
            
            if (results.PerformanceResults != null)
            {
                var perf = results.PerformanceResults;
                resultText += $"Performance: {perf.TestsPassed}/{perf.TotalTests} passed\n";
                resultText += $"Memory Reduction: {perf.AllocationReduction:F1}KB\n";
                resultText += $"Batch Time: {perf.BatchProcessingTime:F2}ms\n";
            }
            
            _resultsText.text = resultText;
        }
        
        private void DisplayDIResults(PC013TestResults results)
        {
            if (_resultsText == null || results.DependencyInjectionResults == null) return;
            
            var di = results.DependencyInjectionResults;
            var resultText = $"DEPENDENCY INJECTION RESULTS\n\n";
            resultText += $"Tests Passed: {di.TestsPassed}/{di.TotalTests}\n";
            resultText += $"Service Container: {(di.ServiceContainerCreated ? "✓" : "✗")}\n";
            resultText += $"Service Registration: {(di.ServiceRegistrationWorking ? "✓" : "✗")}\n";
            resultText += $"Service Resolution: {(di.ServiceResolutionWorking ? "✓" : "✗")}\n";
            resultText += $"Manager Integration: {(di.ManagerIntegrationWorking ? "✓" : "✗")}\n";
            
            _resultsText.text = resultText;
        }
        
        private void DisplayOptimizationResults(PC013TestResults results)
        {
            if (_resultsText == null || results.OptimizationResults == null) return;
            
            var opt = results.OptimizationResults;
            var resultText = $"OPTIMIZATION RESULTS\n\n";
            resultText += $"Tests Passed: {opt.TestsPassed}/{opt.TotalTests}\n";
            resultText += $"Purge Manager: {(opt.PurgeManagerWorking ? "✓" : "✗")}\n";
            resultText += $"Update Optimizer: {(opt.UpdateOptimizerWorking ? "✓" : "✗")}\n";
            resultText += $"Batch Processor: {(opt.BatchProcessorWorking ? "✓" : "✗")}\n";
            
            _resultsText.text = resultText;
        }
        
        private void DisplayPoolingResults(PC013TestResults results)
        {
            if (_resultsText == null || results.PoolingResults == null) return;
            
            var pool = results.PoolingResults;
            var resultText = $"POOLING RESULTS\n\n";
            resultText += $"Tests Passed: {pool.TestsPassed}/{pool.TotalTests}\n";
            resultText += $"Generic Pool: {(pool.GenericPoolWorking ? "✓" : "✗")}\n";
            resultText += $"Object Manager: {(pool.PooledObjectManagerWorking ? "✓" : "✗")}\n";
            resultText += $"Collection Purgers: {(pool.CollectionPurgersWorking ? "✓" : "✗")}\n";
            
            _resultsText.text = resultText;
        }
        
        private void DisplayValidationResults(PC013ValidationResults results)
        {
            if (_resultsText == null) return;
            
            var resultText = $"VALIDATION RESULTS ({results.TotalValidationTime:F2}s)\n\n";
            resultText += $"Overall Status: {(results.AllSystemsValid ? "✓ PASS" : "✗ FAIL")}\n\n";
            
            if (results.DependencyInjectionValidation != null)
            {
                resultText += $"DI Health: {results.DependencyInjectionValidation.OverallDIHealth:P1}\n";
            }
            
            if (results.OptimizationValidation != null)
            {
                resultText += $"Optimization Health: {results.OptimizationValidation.OverallOptimizationHealth:P1}\n";
            }
            
            if (results.PoolingValidation != null)
            {
                resultText += $"Pooling Health: {results.PoolingValidation.OverallPoolingHealth:P1}\n";
            }
            
            if (results.PerformanceValidation != null)
            {
                var perf = results.PerformanceValidation;
                resultText += $"Performance Health: {perf.OverallPerformanceHealth:P1}\n";
                resultText += $"Memory: {perf.CurrentMemoryUsageMB:F1}MB\n";
                resultText += $"FPS: {perf.CurrentFrameRate:F1}\n";
            }
            
            _resultsText.text = resultText;
        }
        
        #endregion
        
        #region UI Helpers
        
        private void UpdateStatusText(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
            
            Debug.Log($"[PC013TestRunnerUI] {status}");
        }
        
        private void ShowProgress(bool show)
        {
            if (_progressPanel != null)
            {
                _progressPanel.SetActive(show);
            }
        }
        
        private void UpdateProgress(float progress)
        {
            if (_progressSlider != null)
            {
                _progressSlider.value = progress;
            }
        }
        
        #endregion
    }
}