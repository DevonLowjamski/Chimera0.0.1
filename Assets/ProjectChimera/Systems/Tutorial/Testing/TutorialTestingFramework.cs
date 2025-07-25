using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.UI.Tutorial;

namespace ProjectChimera.Systems.Tutorial.Testing
{
    /// <summary>
    /// Comprehensive testing framework for the tutorial system.
    /// Provides automated validation and testing tools for tutorial functionality.
    /// </summary>
    public class TutorialTestingFramework : ChimeraManager
    {
        [Header("Testing Configuration")]
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private bool _detailedLogging = true;
        [SerializeField] private float _testTimeout = 30f;
        [SerializeField] private bool _includePerformanceTests = true;
        
        [Header("Test Assets")]
        [SerializeField] private TutorialDataAssetManager _tutorialDataManager;
        [SerializeField] private List<TutorialSequenceSO> _testSequences;
        [SerializeField] private List<TutorialStepSO> _testSteps;
        
        [Header("Test Results")]
        [SerializeField] private TutorialTestResults _lastTestResults;
        
        // Dependencies
        private TutorialManager _tutorialManager;
        private TutorialUIController _tutorialUIController;
        
        // Test state
        private bool _isRunningTests = false;
        private List<TutorialTestCase> _testCases;
        private int _currentTestIndex = 0;
        private float _testStartTime;
        
        protected override void OnManagerInitialize()
        {
            _tutorialManager = GameManager.Instance.GetManager<TutorialManager>();
            
            // Find tutorial UI controller
            _tutorialUIController = FindFirstObjectByType<TutorialUIController>();
            
            InitializeTestCases();
            
            if (_runTestsOnStart)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }
        
        protected override void OnManagerShutdown()
        {
            // Stop any running tests
            _isRunningTests = false;
            
            // Clear test data
            _testCases?.Clear();
            _currentTestIndex = 0;
            
            LogInfo("Tutorial Testing Framework shutdown");
        }
        
        /// <summary>
        /// Initialize test cases
        /// </summary>
        private void InitializeTestCases()
        {
            _testCases = new List<TutorialTestCase>
            {
                new TutorialTestCase
                {
                    TestName = "Tutorial Manager Initialization",
                    TestMethod = TestTutorialManagerInitialization,
                    Category = TutorialTestCategory.Initialization,
                    Priority = TutorialTestPriority.Critical
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Data Asset Validation",
                    TestMethod = TestTutorialDataAssetValidation,
                    Category = TutorialTestCategory.DataValidation,
                    Priority = TutorialTestPriority.Critical
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Step Progression",
                    TestMethod = TestTutorialStepProgression,
                    Category = TutorialTestCategory.Functionality,
                    Priority = TutorialTestPriority.High
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Validation System",
                    TestMethod = TestTutorialValidationSystem,
                    Category = TutorialTestCategory.Validation,
                    Priority = TutorialTestPriority.High
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Progress Persistence",
                    TestMethod = TestTutorialProgressPersistence,
                    Category = TutorialTestCategory.Persistence,
                    Priority = TutorialTestPriority.Medium
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial UI Integration",
                    TestMethod = TestTutorialUIIntegration,
                    Category = TutorialTestCategory.UI,
                    Priority = TutorialTestPriority.High
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Skip and Navigation",
                    TestMethod = TestTutorialSkipAndNavigation,
                    Category = TutorialTestCategory.Navigation,
                    Priority = TutorialTestPriority.Medium
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Error Handling",
                    TestMethod = TestTutorialErrorHandling,
                    Category = TutorialTestCategory.ErrorHandling,
                    Priority = TutorialTestPriority.High
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Performance Validation",
                    TestMethod = TestTutorialPerformance,
                    Category = TutorialTestCategory.Performance,
                    Priority = TutorialTestPriority.Low
                },
                
                new TutorialTestCase
                {
                    TestName = "Tutorial Sequence Completion",
                    TestMethod = TestTutorialSequenceCompletion,
                    Category = TutorialTestCategory.Functionality,
                    Priority = TutorialTestPriority.High
                }
            };
        }
        
        /// <summary>
        /// Run all tests
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            if (_isRunningTests)
            {
                LogWarning("Tests are already running");
                return;
            }
            
            StartCoroutine(RunAllTestsCoroutine());
        }
        
        /// <summary>
        /// Run all tests coroutine
        /// </summary>
        private IEnumerator RunAllTestsCoroutine()
        {
            _isRunningTests = true;
            _testStartTime = Time.realtimeSinceStartup;
            
            var results = new TutorialTestResults
            {
                TestDate = System.DateTime.Now,
                TotalTests = _testCases.Count,
                PassedTests = 0,
                FailedTests = 0,
                SkippedTests = 0,
                TestDetails = new List<TutorialTestDetail>()
            };
            
            LogInfo($"Starting tutorial testing framework - {_testCases.Count} tests to run");
            
            foreach (var testCase in _testCases)
            {
                _currentTestIndex = _testCases.IndexOf(testCase);
                
                var testDetail = new TutorialTestDetail
                {
                    TestName = testCase.TestName,
                    Category = testCase.Category,
                    Priority = testCase.Priority,
                    StartTime = Time.realtimeSinceStartup
                };
                
                LogInfo($"Running test: {testCase.TestName}");
                
                // Reset test case state
                testCase.TestResult = false;
                testCase.ErrorMessage = "";
                testCase.IsCompleted = false;
                
                bool testPassed = false;
                string errorMessage = "";
                
                // Run test with timeout - moved outside try-catch to avoid CS1626
                var testCoroutine = StartCoroutine(RunTestWithTimeout(testCase, _testTimeout));
                yield return testCoroutine;
                
                // Handle test results after coroutine completes
                try
                {
                    testPassed = testCase.TestResult;
                    errorMessage = testCase.ErrorMessage;
                }
                catch (System.Exception ex)
                {
                    testPassed = false;
                    errorMessage = $"Test exception: {ex.Message}";
                    LogError($"Test {testCase.TestName} threw exception: {ex.Message}");
                }
                
                testDetail.EndTime = Time.realtimeSinceStartup;
                testDetail.Duration = testDetail.EndTime - testDetail.StartTime;
                testDetail.Passed = testPassed;
                testDetail.ErrorMessage = errorMessage;
                
                results.TestDetails.Add(testDetail);
                
                if (testPassed)
                {
                    results.PassedTests++;
                    LogInfo($"✓ Test passed: {testCase.TestName} ({testDetail.Duration:F2}s)");
                }
                else
                {
                    results.FailedTests++;
                    LogError($"✗ Test failed: {testCase.TestName} - {errorMessage}");
                }
                
                // Small delay between tests
                yield return new WaitForSeconds(0.1f);
            }
            
            results.TotalDuration = Time.realtimeSinceStartup - _testStartTime;
            results.SuccessRate = (float)results.PassedTests / results.TotalTests * 100f;
            
            _lastTestResults = results;
            
            LogTestResults(results);
            
            _isRunningTests = false;
        }
        
        /// <summary>
        /// Run test with timeout
        /// </summary>
        private IEnumerator RunTestWithTimeout(TutorialTestCase testCase, float timeout)
        {
            var testCoroutine = StartCoroutine(testCase.TestMethod());
            float startTime = Time.realtimeSinceStartup;
            
            while (!testCase.IsCompleted && (Time.realtimeSinceStartup - startTime) < timeout)
            {
                yield return null;
            }
            
            if (!testCase.IsCompleted)
            {
                StopCoroutine(testCoroutine);
                testCase.TestResult = false;
                testCase.ErrorMessage = $"Test timed out after {timeout} seconds";
                testCase.IsCompleted = true;
            }
        }
        
        /// <summary>
        /// Test tutorial manager initialization
        /// </summary>
        private IEnumerator TestTutorialManagerInitialization()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Verify tutorial manager is initialized
                if (_tutorialManager == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial manager is null";
                    testCase.IsCompleted = true;
                    yield break;
                }
                
                // Verify manager is active and enabled
                if (!_tutorialManager.IsInitialized)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial manager is not initialized";
                    testCase.IsCompleted = true;
                    yield break;
                }
                
                // Verify essential components
                if (_tutorialDataManager == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial data manager reference is missing";
                    testCase.IsCompleted = true;
                    yield break;
                }
                
                testCase.TestResult = true;
                testCase.IsCompleted = true;
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
        }
        
        /// <summary>
        /// Test tutorial data asset validation
        /// </summary>
        private IEnumerator TestTutorialDataAssetValidation()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Validate tutorial data manager
                if (!_tutorialDataManager.ValidateData())
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial data manager validation failed";
                    testCase.IsCompleted = true;
                    yield break;
                }
                
                // Check sequence availability
                var sequences = _tutorialDataManager.AvailableSequences;
                if (sequences == null || sequences.Count == 0)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "No tutorial sequences available";
                    testCase.IsCompleted = true;
                    yield break;
                }
                
                // Validate each sequence
                foreach (var sequence in sequences)
                {
                    if (sequence == null)
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = "Found null sequence in available sequences";
                        testCase.IsCompleted = true;
                        yield break;
                    }
                    
                    if (!sequence.ValidateData())
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = $"Sequence validation failed: {sequence.SequenceName}";
                        testCase.IsCompleted = true;
                        yield break;
                    }
                }
                
                testCase.TestResult = true;
                testCase.IsCompleted = true;
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial step progression
        /// </summary>
        private IEnumerator TestTutorialStepProgression()
        {
            var testCase = _testCases[_currentTestIndex];
            bool shouldExit = false;
            
            try
            {
                // Get first available sequence for testing
                var sequence = _tutorialDataManager.AvailableSequences.FirstOrDefault();
                if (sequence == null || sequence.Steps.Count == 0)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "No sequences available for testing";
                    testCase.IsCompleted = true;
                    shouldExit = true;
                }
                else
                {
                    // Start tutorial sequence
                    bool started = _tutorialManager.StartTutorialSequence((TutorialSequenceSO)sequence);
                    if (!started)
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = "Failed to start tutorial sequence";
                        testCase.IsCompleted = true;
                        shouldExit = true;
                    }
                    else
                    {
                        // Check if first step is active
                        var currentStep = _tutorialManager.GetCurrentStep();
                        if (currentStep == null)
                        {
                            testCase.TestResult = false;
                            testCase.ErrorMessage = "No current step after starting sequence";
                            testCase.IsCompleted = true;
                            shouldExit = true;
                        }
                        else
                        {
                            // Try to progress to next step
                            bool progressed = _tutorialManager.CompleteCurrentStep();
                            if (!progressed && sequence.Steps.Count > 1)
                            {
                                testCase.TestResult = false;
                                testCase.ErrorMessage = "Failed to progress to next step";
                                testCase.IsCompleted = true;
                                shouldExit = true;
                            }
                            else
                            {
                                // Stop tutorial
                                _tutorialManager.StopCurrentTutorial();
                                
                                testCase.TestResult = true;
                                testCase.IsCompleted = true;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial validation system
        /// </summary>
        private IEnumerator TestTutorialValidationSystem()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Find a step with validation for testing
                TutorialStepSO validationStep = null;
                foreach (var sequence in _tutorialDataManager.AvailableSequences)
                {
                    validationStep = sequence.Steps.FirstOrDefault(s => 
                        s.ValidationType != TutorialValidationType.None && 
                        s.ValidationType != TutorialValidationType.Timer);
                    
                    if (validationStep != null) break;
                }
                
                if (validationStep == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "No validation steps found for testing";
                    testCase.IsCompleted = true;
                }
                else
                {
                    // Test validation logic (basic validation check)
                    bool isValidTarget = !string.IsNullOrEmpty(validationStep.ValidationTarget);
                    if (!isValidTarget && validationStep.ValidationType != TutorialValidationType.Timer)
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = $"Validation step missing target: {validationStep.StepId}";
                        testCase.IsCompleted = true;
                    }
                    else
                    {
                        testCase.TestResult = true;
                        testCase.IsCompleted = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial progress persistence
        /// </summary>
        private IEnumerator TestTutorialProgressPersistence()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Test progress tracking
                var progress = _tutorialManager.GetProgressData();
                if (progress == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Progress data is null";
                    testCase.IsCompleted = true;
                }
                else
                {
                    // Test save/load functionality
                    _tutorialManager.SaveProgress();
                    _tutorialManager.LoadProgress();
                    
                    testCase.TestResult = true;
                    testCase.IsCompleted = true;
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial UI integration
        /// </summary>
        private IEnumerator TestTutorialUIIntegration()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                if (_tutorialUIController == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial UI controller not found";
                    testCase.IsCompleted = true;
                }
                else if (!_tutorialUIController.gameObject.activeInHierarchy)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial UI controller is not active";
                    testCase.IsCompleted = true;
                }
                else
                {
                    testCase.TestResult = true;
                    testCase.IsCompleted = true;
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial skip and navigation
        /// </summary>
        private IEnumerator TestTutorialSkipAndNavigation()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Find a skippable sequence
                var skippableSequence = _tutorialDataManager.AvailableSequences.FirstOrDefault(s => 
                    s.Steps.Any(step => step.CanSkip));
                
                if (skippableSequence == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "No skippable sequences found";
                    testCase.IsCompleted = true;
                }
                else
                {
                    // Test skip functionality
                    _tutorialManager.StartTutorialSequence((TutorialSequenceSO)skippableSequence);
                    
                    bool canSkip = _tutorialManager.CanSkipCurrentStep();
                    if (canSkip)
                    {
                        bool skipped = _tutorialManager.SkipCurrentStep();
                        if (!skipped)
                        {
                            testCase.TestResult = false;
                            testCase.ErrorMessage = "Failed to skip step when skip was allowed";
                            testCase.IsCompleted = true;
                        }
                        else
                        {
                            testCase.TestResult = true;
                            testCase.IsCompleted = true;
                        }
                    }
                    else
                    {
                        testCase.TestResult = true;
                        testCase.IsCompleted = true;
                    }
                    
                    _tutorialManager.StopCurrentTutorial();
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial error handling
        /// </summary>
        private IEnumerator TestTutorialErrorHandling()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Test null sequence handling
                bool startedNull = _tutorialManager.StartTutorialSequence((TutorialSequenceSO)null);
                if (startedNull)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "Tutorial manager accepted null sequence";
                    testCase.IsCompleted = true;
                }
                else
                {
                    // Test invalid step completion
                    bool completedInvalid = _tutorialManager.CompleteCurrentStep();
                    if (completedInvalid && _tutorialManager.GetCurrentStep() == null)
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = "Completed step when no tutorial was active";
                        testCase.IsCompleted = true;
                    }
                    else
                    {
                        testCase.TestResult = true;
                        testCase.IsCompleted = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial performance
        /// </summary>
        private IEnumerator TestTutorialPerformance()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                float startTime = Time.realtimeSinceStartup;
                
                // Rapid sequence start/stop test
                var sequence = _tutorialDataManager.AvailableSequences.FirstOrDefault();
                if (sequence != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        _tutorialManager.StartTutorialSequence((TutorialSequenceSO)sequence);
                        _tutorialManager.StopCurrentTutorial();
                    }
                }
                
                float duration = Time.realtimeSinceStartup - startTime;
                
                // Performance should be reasonable (under 1 second for 10 cycles)
                if (duration > 1.0f)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = $"Performance test failed - took {duration:F2}s for basic operations";
                    testCase.IsCompleted = true;
                }
                else
                {
                    testCase.TestResult = true;
                    testCase.IsCompleted = true;
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test tutorial sequence completion
        /// </summary>
        private IEnumerator TestTutorialSequenceCompletion()
        {
            var testCase = _testCases[_currentTestIndex];
            
            try
            {
                // Find a short sequence for testing
                var shortSequence = _tutorialDataManager.AvailableSequences
                    .Where(s => s.Steps.Count <= 3)
                    .FirstOrDefault();
                
                if (shortSequence == null)
                {
                    testCase.TestResult = false;
                    testCase.ErrorMessage = "No short sequences found for completion testing";
                    testCase.IsCompleted = true;
                }
                else
                {
                    // Start sequence and complete all steps
                    _tutorialManager.StartTutorialSequence((TutorialSequenceSO)shortSequence);
                    
                    int completedSteps = 0;
                    int maxSteps = shortSequence.Steps.Count;
                    
                    while (_tutorialManager.GetCurrentStep() != null && completedSteps < maxSteps)
                    {
                        _tutorialManager.CompleteCurrentStep();
                        completedSteps++;
                        
                        // Safety check to prevent infinite loop
                        if (completedSteps > maxSteps)
                        {
                            break;
                        }
                    }
                    
                    // Check if sequence was properly completed
                    bool isActive = _tutorialManager.IsSequenceActive();
                    if (isActive)
                    {
                        testCase.TestResult = false;
                        testCase.ErrorMessage = "Sequence still active after completing all steps";
                        testCase.IsCompleted = true;
                    }
                    else
                    {
                        testCase.TestResult = true;
                        testCase.IsCompleted = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                testCase.TestResult = false;
                testCase.ErrorMessage = ex.Message;
                testCase.IsCompleted = true;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Log test results
        /// </summary>
        private void LogTestResults(TutorialTestResults results)
        {
            LogInfo("═══════════════════════════════════════");
            LogInfo("TUTORIAL TESTING FRAMEWORK RESULTS");
            LogInfo("═══════════════════════════════════════");
            LogInfo($"Total Tests: {results.TotalTests}");
            LogInfo($"Passed: {results.PassedTests} ✓");
            LogInfo($"Failed: {results.FailedTests} ✗");
            LogInfo($"Skipped: {results.SkippedTests} -");
            LogInfo($"Success Rate: {results.SuccessRate:F1}%");
            LogInfo($"Total Duration: {results.TotalDuration:F2} seconds");
            LogInfo("═══════════════════════════════════════");
            
            if (_detailedLogging && results.TestDetails.Count > 0)
            {
                LogInfo("DETAILED TEST RESULTS:");
                
                foreach (var detail in results.TestDetails)
                {
                    string status = detail.Passed ? "✓ PASS" : "✗ FAIL";
                    string message = detail.Passed ? "" : $" - {detail.ErrorMessage}";
                    
                    LogInfo($"[{detail.Category}] {status} {detail.TestName} ({detail.Duration:F2}s){message}");
                }
                
                LogInfo("═══════════════════════════════════════");
            }
            
            // Summary based on results
            if (results.FailedTests == 0)
            {
                LogInfo("🎉 ALL TUTORIAL TESTS PASSED!");
            }
            else if (results.SuccessRate >= 80f)
            {
                LogWarning($"⚠️  Most tests passed but {results.FailedTests} failed. Review failed tests.");
            }
            else
            {
                LogError($"❌ Tutorial testing failed - {results.FailedTests} failures detected!");
            }
        }
        
        /// <summary>
        /// Get test results
        /// </summary>
        public TutorialTestResults GetLastTestResults()
        {
            return _lastTestResults;
        }
        
        /// <summary>
        /// Check if tests are currently running
        /// </summary>
        public bool IsRunningTests()
        {
            return _isRunningTests;
        }
    }
    
    /// <summary>
    /// Tutorial test case structure
    /// </summary>
    [System.Serializable]
    public class TutorialTestCase
    {
        public string TestName;
        public System.Func<IEnumerator> TestMethod;
        public TutorialTestCategory Category;
        public TutorialTestPriority Priority;
        public bool TestResult;
        public string ErrorMessage;
        public bool IsCompleted;
    }
    
    /// <summary>
    /// Tutorial test results
    /// </summary>
    [System.Serializable]
    public class TutorialTestResults
    {
        public System.DateTime TestDate;
        public int TotalTests;
        public int PassedTests;
        public int FailedTests;
        public int SkippedTests;
        public float SuccessRate;
        public float TotalDuration;
        public List<TutorialTestDetail> TestDetails;
    }
    
    /// <summary>
    /// Individual test detail
    /// </summary>
    [System.Serializable]
    public class TutorialTestDetail
    {
        public string TestName;
        public TutorialTestCategory Category;
        public TutorialTestPriority Priority;
        public float StartTime;
        public float EndTime;
        public float Duration;
        public bool Passed;
        public string ErrorMessage;
    }
    
    /// <summary>
    /// Tutorial test categories
    /// </summary>
    public enum TutorialTestCategory
    {
        Initialization,
        DataValidation,
        Functionality,
        Validation,
        Persistence,
        UI,
        Navigation,
        ErrorHandling,
        Performance
    }
    
    /// <summary>
    /// Tutorial test priorities
    /// </summary>
    public enum TutorialTestPriority
    {
        Critical,
        High,
        Medium,
        Low
    }
}