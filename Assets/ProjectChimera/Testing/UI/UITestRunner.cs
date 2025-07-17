using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Testing.UI
{
    /// <summary>
    /// Automated test runner for UI systems in Project Chimera.
    /// Coordinates testing framework, validation suite, and performance profiler.
    /// </summary>
    public class UITestRunner : ChimeraMonoBehaviour
    {
        [Header("Test Runner Configuration")]
        [SerializeField] private bool _autoRunOnStart = false;
        [SerializeField] private bool _enableContinuousIntegration = false;
        [SerializeField] private float _ciTestInterval = 300f; // 5 minutes
        [SerializeField] private bool _generateReports = true;
        [SerializeField] private string _reportOutputPath = "Assets/ProjectChimera/Testing/Reports/";
        
        [Header("Test Components")]
        [SerializeField] private UITestFramework _testFramework;
        [SerializeField] private UIValidationSuite _validationSuite;
        [SerializeField] private UIPerformanceProfiler _performanceProfiler;
        
        [Header("Test Configuration")]
        [SerializeField] private UITestConfiguration _testConfig;
        [SerializeField] private bool _runPerformanceTests = true;
        [SerializeField] private bool _runValidationTests = true;
        [SerializeField] private bool _runIntegrationTests = true;
        [SerializeField] private bool _failOnCriticalIssues = true;
        
        [Header("Test Results")]
        [SerializeField] private UITestRunResults _lastRunResults;
        [SerializeField] private List<UITestSession> _testHistory = new List<UITestSession>();
        [SerializeField] private int _maxHistoryEntries = 100;
        
        // Test execution state
        private bool _isTestRunInProgress = false;
        private float _ciTimer = 0f;
        private UITestSession _currentSession;
        
        // Events
        public System.Action<UITestRunResults> OnTestRunCompleted;
        public System.Action<UITestSession> OnTestSessionStarted;
        public System.Action<string> OnTestRunFailed;
        public System.Action<string> OnReportGenerated;
        
        // Properties
        public bool IsTestRunInProgress => _isTestRunInProgress;
        public UITestRunResults LastRunResults => _lastRunResults;
        public int TestHistoryCount => _testHistory.Count;
        public UITestSession CurrentSession => _currentSession;
        
        protected override void Start()
        {
            base.Start();
            
            InitializeTestRunner();
            
            if (_autoRunOnStart)
            {
                RunFullTestSuite();
            }
        }
        
        /// <summary>
        /// Initialize test runner
        /// </summary>
        private void InitializeTestRunner()
        {
            ValidateTestComponents();
            SetupTestEnvironment();
            
            LogInfo("UI Test Runner initialized successfully");
        }
        
        /// <summary>
        /// Validate test components are properly configured
        /// </summary>
        private void ValidateTestComponents()
        {
            if (_testFramework == null)
            {
                LogWarning("UITestFramework not assigned - functional tests will be skipped");
            }
            
            if (_validationSuite == null)
            {
                LogWarning("UIValidationSuite not assigned - validation tests will be skipped");
            }
            
            if (_performanceProfiler == null)
            {
                LogWarning("UIPerformanceProfiler not assigned - performance tests will be skipped");
            }
        }
        
        /// <summary>
        /// Setup test environment
        /// </summary>
        private void SetupTestEnvironment()
        {
            // Ensure report output directory exists
            if (_generateReports && !string.IsNullOrEmpty(_reportOutputPath))
            {
                var fullPath = System.IO.Path.Combine(Application.dataPath, _reportOutputPath.Replace("Assets/", ""));
                if (!System.IO.Directory.Exists(fullPath))
                {
                    System.IO.Directory.CreateDirectory(fullPath);
                }
            }
        }
        
        /// <summary>
        /// Run full test suite
        /// </summary>
        public void RunFullTestSuite()
        {
            if (_isTestRunInProgress)
            {
                LogWarning("Test run already in progress");
                return;
            }
            
            StartCoroutine(ExecuteFullTestSuite());
        }
        
        /// <summary>
        /// Run specific test category
        /// </summary>
        public void RunTestCategory(UITestType testType)
        {
            if (_isTestRunInProgress)
            {
                LogWarning("Test run already in progress");
                return;
            }
            
            StartCoroutine(ExecuteTestCategory(testType));
        }
        
        /// <summary>
        /// Execute full test suite coroutine
        /// </summary>
        private System.Collections.IEnumerator ExecuteFullTestSuite()
        {
            _isTestRunInProgress = true;
            var startTime = System.DateTime.Now;
            
            // Start new test session
            _currentSession = new UITestSession
            {
                SessionId = System.Guid.NewGuid().ToString(),
                StartTime = startTime,
                TestTypes = new List<UITestType>()
            };
            
            OnTestSessionStarted?.Invoke(_currentSession);
            
            LogInfo("Starting full UI test suite");
            
            // Initialize results
            var results = new UITestRunResults
            {
                StartTime = startTime,
                TestFrameworkResults = null,
                ValidationResults = null,
                PerformanceReport = null
            };
            
            // Run functional tests
            if (_testFramework != null)
            {
                LogInfo("Running functional tests...");
                _currentSession.TestTypes.Add(UITestType.Functional);
                
                _testFramework.RunAllTests();
                
                // Wait for tests to complete
                while (_testFramework.IsTestingInProgress)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                results.TestFrameworkResults = _testFramework.LastTestResults;
                LogInfo($"Functional tests completed: {results.TestFrameworkResults.PassedTests}/{results.TestFrameworkResults.TotalTests} passed");
            }
            
            // Run validation tests
            if (_runValidationTests && _validationSuite != null)
            {
                LogInfo("Running validation tests...");
                _currentSession.TestTypes.Add(UITestType.Validation);
                
                _validationSuite.RunFullValidation();
                
                // Wait for validation to complete
                while (_validationSuite.IsValidating)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                results.ValidationResults = _validationSuite.LastValidationResults;
                LogInfo($"Validation tests completed: {results.ValidationResults.PassedRules}/{results.ValidationResults.TotalRules} passed");
            }
            
            // Run performance tests
            if (_runPerformanceTests && _performanceProfiler != null)
            {
                LogInfo("Running performance tests...");
                _currentSession.TestTypes.Add(UITestType.Performance);
                
                if (!_performanceProfiler.IsProfilingActive)
                {
                    _performanceProfiler.StartProfiling();
                }
                
                // Run performance test for a short duration
                yield return new WaitForSeconds(5f);
                
                _performanceProfiler.GeneratePerformanceReport();
                results.PerformanceReport = _performanceProfiler.LastReport;
                LogInfo("Performance tests completed");
            }
            
            // Complete test session
            var endTime = System.DateTime.Now;
            results.EndTime = endTime;
            results.TotalDuration = (float)(endTime - startTime).TotalSeconds;
            results.OverallSuccess = DetermineOverallSuccess(results);
            
            _currentSession.EndTime = endTime;
            _currentSession.Duration = results.TotalDuration;
            _currentSession.Success = results.OverallSuccess;
            
            // Store results
            _lastRunResults = results;
            AddToTestHistory(_currentSession);
            
            // Generate reports
            if (_generateReports)
            {
                GenerateTestReports(results);
            }
            
            // Check for critical failures
            if (_failOnCriticalIssues && !results.OverallSuccess)
            {
                var criticalIssues = GetCriticalIssues(results);
                OnTestRunFailed?.Invoke($"Critical issues found: {string.Join(", ", criticalIssues)}");
            }
            
            OnTestRunCompleted?.Invoke(results);
            
            _isTestRunInProgress = false;
            _currentSession = null;
            
            LogInfo($"Full test suite completed in {results.TotalDuration:F2}s - Success: {results.OverallSuccess}");
        }
        
        /// <summary>
        /// Execute specific test category coroutine
        /// </summary>
        private System.Collections.IEnumerator ExecuteTestCategory(UITestType testType)
        {
            _isTestRunInProgress = true;
            var startTime = System.DateTime.Now;
            
            LogInfo($"Running {testType} tests");
            
            switch (testType)
            {
                case UITestType.Functional:
                    if (_testFramework != null)
                    {
                        _testFramework.RunAllTests();
                        while (_testFramework.IsTestingInProgress)
                        {
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    break;
                    
                case UITestType.Validation:
                    if (_validationSuite != null)
                    {
                        _validationSuite.RunFullValidation();
                        while (_validationSuite.IsValidating)
                        {
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    break;
                    
                case UITestType.Performance:
                    if (_performanceProfiler != null)
                    {
                        if (!_performanceProfiler.IsProfilingActive)
                        {
                            _performanceProfiler.StartProfiling();
                        }
                        yield return new WaitForSeconds(5f);
                        _performanceProfiler.GeneratePerformanceReport();
                    }
                    break;
            }
            
            _isTestRunInProgress = false;
            
            var duration = (System.DateTime.Now - startTime).TotalSeconds;
            LogInfo($"{testType} tests completed in {duration:F2}s");
        }
        
        /// <summary>
        /// Determine overall test success
        /// </summary>
        private bool DetermineOverallSuccess(UITestRunResults results)
        {
            bool success = true;
            
            // Check functional test results
            if (results.TestFrameworkResults != null)
            {
                if (results.TestFrameworkResults.FailedTests > 0)
                {
                    success = false;
                }
            }
            
            // Check validation results
            if (results.ValidationResults != null)
            {
                if (results.ValidationResults.CriticalIssues > 0)
                {
                    success = false;
                }
                
                // High issues might be acceptable depending on configuration
                if (_failOnCriticalIssues && results.ValidationResults.HighIssues > 3)
                {
                    success = false;
                }
            }
            
            // Check performance results
            if (results.PerformanceReport != null)
            {
                if (!results.PerformanceReport.IsHealthy)
                {
                    success = false;
                }
            }
            
            return success;
        }
        
        /// <summary>
        /// Get critical issues from test results
        /// </summary>
        private List<string> GetCriticalIssues(UITestRunResults results)
        {
            var issues = new List<string>();
            
            if (results.TestFrameworkResults != null && results.TestFrameworkResults.FailedTests > 0)
            {
                issues.Add($"{results.TestFrameworkResults.FailedTests} functional test failures");
            }
            
            if (results.ValidationResults != null && results.ValidationResults.CriticalIssues > 0)
            {
                issues.Add($"{results.ValidationResults.CriticalIssues} critical validation issues");
            }
            
            if (results.PerformanceReport != null && !results.PerformanceReport.IsHealthy)
            {
                var criticalAlerts = results.PerformanceReport.ActiveAlerts.Count(a => a.Severity == UIPerformanceAlertSeverity.Critical);
                if (criticalAlerts > 0)
                {
                    issues.Add($"{criticalAlerts} critical performance alerts");
                }
            }
            
            return issues;
        }
        
        /// <summary>
        /// Generate test reports
        /// </summary>
        private void GenerateTestReports(UITestRunResults results)
        {
            if (string.IsNullOrEmpty(_reportOutputPath))
                return;
            
            try
            {
                var timestamp = results.StartTime.ToString("yyyy-MM-dd_HH-mm-ss");
                
                // Generate summary report
                var summaryPath = GenerateSummaryReport(results, timestamp);
                
                // Generate detailed reports
                if (results.TestFrameworkResults != null)
                {
                    GenerateFunctionalTestReport(results.TestFrameworkResults, timestamp);
                }
                
                if (results.ValidationResults != null)
                {
                    GenerateValidationReport(results.ValidationResults, timestamp);
                }
                
                if (results.PerformanceReport != null)
                {
                    GeneratePerformanceReport(results.PerformanceReport, timestamp);
                }
                
                OnReportGenerated?.Invoke(summaryPath);
                LogInfo($"Test reports generated at: {_reportOutputPath}");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to generate test reports: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Generate summary report
        /// </summary>
        private string GenerateSummaryReport(UITestRunResults results, string timestamp)
        {
            var reportPath = System.IO.Path.Combine(_reportOutputPath, $"UI_Test_Summary_{timestamp}.md");
            var content = $@"# UI Test Suite Summary Report

**Test Run Date:** {results.StartTime:yyyy-MM-dd HH:mm:ss}
**Duration:** {results.TotalDuration:F2}s
**Overall Success:** {results.OverallSuccess}

## Test Results Overview

### Functional Tests
{(results.TestFrameworkResults != null ? 
    $@"- **Total Tests:** {results.TestFrameworkResults.TotalTests}
- **Passed:** {results.TestFrameworkResults.PassedTests}
- **Failed:** {results.TestFrameworkResults.FailedTests}
- **Success Rate:** {results.TestFrameworkResults.SuccessRate:P1}" : 
    "- Not executed")}

### Validation Tests
{(results.ValidationResults != null ? 
    $@"- **Total Rules:** {results.ValidationResults.TotalRules}
- **Passed:** {results.ValidationResults.PassedRules}
- **Failed:** {results.ValidationResults.FailedRules}
- **Critical Issues:** {results.ValidationResults.CriticalIssues}
- **High Issues:** {results.ValidationResults.HighIssues}" : 
    "- Not executed")}

### Performance Tests
{(results.PerformanceReport != null ? 
    $@"- **Average Frame Time:** {results.PerformanceReport.AverageFrameTime:F2}ms
- **Average Memory Usage:** {results.PerformanceReport.AverageMemoryUsage:F1}MB
- **Active Alerts:** {results.PerformanceReport.ActiveAlerts.Count}
- **Health Status:** {(results.PerformanceReport.IsHealthy ? "Healthy" : "Issues Detected")}" : 
    "- Not executed")}

## Critical Issues
{(GetCriticalIssues(results).Count > 0 ? 
    string.Join("\n", GetCriticalIssues(results).Select(issue => $"- {issue}")) : 
    "- None detected")}

---
*Report generated by Project Chimera UI Test Runner*
";
            
            System.IO.File.WriteAllText(reportPath, content);
            return reportPath;
        }
        
        /// <summary>
        /// Generate functional test report
        /// </summary>
        private void GenerateFunctionalTestReport(UITestResults results, string timestamp)
        {
            var reportPath = System.IO.Path.Combine(_reportOutputPath, $"UI_Functional_Tests_{timestamp}.json");
            var json = JsonUtility.ToJson(results, true);
            System.IO.File.WriteAllText(reportPath, json);
        }
        
        /// <summary>
        /// Generate validation report
        /// </summary>
        private void GenerateValidationReport(UIValidationResults results, string timestamp)
        {
            var reportPath = System.IO.Path.Combine(_reportOutputPath, $"UI_Validation_{timestamp}.json");
            var json = JsonUtility.ToJson(results, true);
            System.IO.File.WriteAllText(reportPath, json);
        }
        
        /// <summary>
        /// Generate performance report
        /// </summary>
        private void GeneratePerformanceReport(UIPerformanceReport report, string timestamp)
        {
            var reportPath = System.IO.Path.Combine(_reportOutputPath, $"UI_Performance_{timestamp}.json");
            var json = JsonUtility.ToJson(report, true);
            System.IO.File.WriteAllText(reportPath, json);
        }
        
        /// <summary>
        /// Add test session to history
        /// </summary>
        private void AddToTestHistory(UITestSession session)
        {
            _testHistory.Add(session);
            
            // Maintain history size limit
            while (_testHistory.Count > _maxHistoryEntries)
            {
                _testHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Get test history statistics
        /// </summary>
        public UITestHistoryStats GetTestHistoryStats()
        {
            if (_testHistory.Count == 0)
            {
                return new UITestHistoryStats();
            }
            
            var successfulRuns = _testHistory.Count(s => s.Success);
            var recentRuns = _testHistory.TakeLast(10).ToList();
            var recentSuccessRate = recentRuns.Count > 0 ? recentRuns.Count(s => s.Success) / (float)recentRuns.Count : 0f;
            
            return new UITestHistoryStats
            {
                TotalRuns = _testHistory.Count,
                SuccessfulRuns = successfulRuns,
                OverallSuccessRate = successfulRuns / (float)_testHistory.Count,
                RecentSuccessRate = recentSuccessRate,
                AverageDuration = _testHistory.Average(s => s.Duration),
                LastRunTime = _testHistory.Last().StartTime
            };
        }
        
        private void Update()
        {
            // Continuous integration testing
            if (_enableContinuousIntegration && !_isTestRunInProgress)
            {
                _ciTimer += Time.deltaTime;
                
                if (_ciTimer >= _ciTestInterval)
                {
                    LogInfo("Running scheduled CI test");
                    RunFullTestSuite();
                    _ciTimer = 0f;
                }
            }
        }
        
        private void OnValidate()
        {
            _ciTestInterval = Mathf.Max(60f, _ciTestInterval);
            _maxHistoryEntries = Mathf.Max(10, _maxHistoryEntries);
            
            if (string.IsNullOrEmpty(_reportOutputPath))
            {
                _reportOutputPath = "Assets/ProjectChimera/Testing/Reports/";
            }
        }
        
        // Editor methods for manual testing
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void EditorRunTests()
        {
            if (Application.isPlaying)
            {
                RunFullTestSuite();
            }
            else
            {
                LogWarning("Tests can only be run in Play mode");
            }
        }
        
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void EditorGenerateReport()
        {
            if (_lastRunResults != null)
            {
                GenerateTestReports(_lastRunResults);
            }
            else
            {
                LogWarning("No test results available for report generation");
            }
        }
    }
    
    /// <summary>
    /// Test configuration settings
    /// </summary>
    [System.Serializable]
    public class UITestConfiguration
    {
        public bool EnableFunctionalTests = true;
        public bool EnableValidationTests = true;
        public bool EnablePerformanceTests = true;
        public bool FailOnCriticalIssues = true;
        public bool GenerateDetailedReports = true;
        public float PerformanceTestDuration = 5f;
        public List<string> ExcludedTestCategories = new List<string>();
    }
    
    /// <summary>
    /// Complete test run results
    /// </summary>
    [System.Serializable]
    public class UITestRunResults
    {
        public System.DateTime StartTime;
        public System.DateTime EndTime;
        public float TotalDuration;
        public bool OverallSuccess;
        public UITestResults TestFrameworkResults;
        public UIValidationResults ValidationResults;
        public UIPerformanceReport PerformanceReport;
    }
    
    /// <summary>
    /// Test session record
    /// </summary>
    [System.Serializable]
    public class UITestSession
    {
        public string SessionId;
        public System.DateTime StartTime;
        public System.DateTime EndTime;
        public float Duration;
        public bool Success;
        public List<UITestType> TestTypes = new List<UITestType>();
    }
    
    /// <summary>
    /// Test history statistics
    /// </summary>
    public struct UITestHistoryStats
    {
        public int TotalRuns;
        public int SuccessfulRuns;
        public float OverallSuccessRate;
        public float RecentSuccessRate;
        public float AverageDuration;
        public System.DateTime LastRunTime;
    }
    
    /// <summary>
    /// Test types
    /// </summary>
    public enum UITestType
    {
        Functional,
        Validation,
        Performance,
        Integration
    }
}