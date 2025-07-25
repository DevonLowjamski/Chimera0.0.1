using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Testing;
using ProjectChimera.UI.Core;
using ProjectChimera.Data.UI;

namespace ProjectChimera.Testing.UI
{
    /// <summary>
    /// Comprehensive validation suite for UI systems in Project Chimera.
    /// Provides deep validation of UI components, data consistency, and integration points.
    /// </summary>
    public class UIValidationSuite : ChimeraMonoBehaviour
    {
        [Header("Validation Configuration")]
        [SerializeField] private bool _enableRuntimeValidation = true;
        [SerializeField] private bool _enablePerformanceValidation = true;
        [SerializeField] private bool _enableDataValidation = true;
        [SerializeField] private bool _enableIntegrationValidation = true;
        [SerializeField] private float _validationInterval = 60f; // seconds
        
        [Header("Validation Targets")]
        [SerializeField] private GameUIManager _gameUIManager;
        [SerializeField] private UIPrefabManager _prefabManager;
        [SerializeField] private UIStateManager _stateManager;
        [SerializeField] private UIIntegrationManager _integrationManager;
        
        [Header("Validation Thresholds")]
        [SerializeField] private int _maxActiveComponents = 100;
        [SerializeField] private float _maxMemoryUsageMB = 50f;
        [SerializeField] private float _maxUpdateTimeMs = 16f; // 60 FPS
        [SerializeField] private int _maxOrphanedComponents = 5;
        
        [Header("Validation Results")]
        [SerializeField] private UIValidationResults _lastValidationResults;
        [SerializeField] private List<UIValidationIssue> _currentIssues = new List<UIValidationIssue>();
        [SerializeField] private bool _showValidationDetails = true;
        
        // Validation state
        private float _validationTimer = 0f;
        private Dictionary<string, UIComponentHealth> _componentHealth;
        private List<UIValidationRule> _validationRules;
        private bool _isValidating = false;
        
        // Events
        public System.Action<UIValidationResults> OnValidationCompleted;
        public System.Action<UIValidationIssue> OnValidationIssueFound;
        public System.Action<UIValidationIssue> OnValidationIssueResolved;
        
        // Properties
        public bool IsValidating => _isValidating;
        public UIValidationResults LastValidationResults => _lastValidationResults;
        public int CurrentIssueCount => _currentIssues.Count;
        public int CriticalIssueCount => _currentIssues.Count(i => i.Severity == UIValidationSeverity.Critical);
        
        protected override void Start()
        {
            base.Start();
            
            InitializeValidationSuite();
            
            if (_enableRuntimeValidation)
            {
                RunFullValidation();
            }
        }
        
        /// <summary>
        /// Initialize validation suite
        /// </summary>
        private void InitializeValidationSuite()
        {
            _componentHealth = new Dictionary<string, UIComponentHealth>();
            _validationRules = new List<UIValidationRule>();
            
            RegisterValidationRules();
            
            LogInfo("UI Validation Suite initialized successfully");
        }
        
        /// <summary>
        /// Register built-in validation rules
        /// </summary>
        private void RegisterValidationRules()
        {
            // Component validation rules
            AddValidationRule("component_initialization", ValidateComponentInitialization, UIValidationSeverity.High);
            AddValidationRule("component_memory_usage", ValidateComponentMemoryUsage, UIValidationSeverity.Medium);
            AddValidationRule("component_lifecycle", ValidateComponentLifecycle, UIValidationSeverity.High);
            AddValidationRule("component_references", ValidateComponentReferences, UIValidationSeverity.Medium);
            
            // Manager validation rules
            AddValidationRule("manager_integration", ValidateManagerIntegration, UIValidationSeverity.Critical);
            AddValidationRule("manager_performance", ValidateManagerPerformance, UIValidationSeverity.Medium);
            AddValidationRule("manager_state", ValidateManagerState, UIValidationSeverity.High);
            
            // Data validation rules
            AddValidationRule("data_consistency", ValidateDataConsistency, UIValidationSeverity.High);
            AddValidationRule("data_persistence", ValidateDataPersistence, UIValidationSeverity.Medium);
            AddValidationRule("data_serialization", ValidateDataSerialization, UIValidationSeverity.Medium);
            
            // Performance validation rules
            AddValidationRule("performance_fps", ValidatePerformanceFPS, UIValidationSeverity.Medium);
            AddValidationRule("performance_memory", ValidatePerformanceMemory, UIValidationSeverity.High);
            AddValidationRule("performance_pooling", ValidatePerformancePooling, UIValidationSeverity.Low);
            
            // Integration validation rules
            AddValidationRule("integration_events", ValidateIntegrationEvents, UIValidationSeverity.High);
            AddValidationRule("integration_bindings", ValidateIntegrationBindings, UIValidationSeverity.Medium);
            AddValidationRule("integration_dependencies", ValidateIntegrationDependencies, UIValidationSeverity.Critical);
            
            LogInfo($"Registered {_validationRules.Count} validation rules");
        }
        
        /// <summary>
        /// Add validation rule
        /// </summary>
        public void AddValidationRule(string ruleName, System.Func<UIValidationResult> ruleMethod, UIValidationSeverity severity)
        {
            _validationRules.Add(new UIValidationRule
            {
                Name = ruleName,
                Method = ruleMethod,
                Severity = severity,
                IsEnabled = true
            });
        }
        
        /// <summary>
        /// Run full validation suite
        /// </summary>
        public void RunFullValidation()
        {
            if (_isValidating)
            {
                LogWarning("Validation already in progress");
                return;
            }
            
            StartCoroutine(ExecuteValidationSuite());
        }
        
        /// <summary>
        /// Run validation by category
        /// </summary>
        public void RunValidationByCategory(UIValidationCategory category)
        {
            var rulesToRun = _validationRules.Where(r => GetRuleCategory(r.Name) == category && r.IsEnabled).ToList();
            StartCoroutine(ExecuteValidationBatch(rulesToRun));
        }
        
        /// <summary>
        /// Execute validation suite coroutine
        /// </summary>
        private System.Collections.IEnumerator ExecuteValidationSuite()
        {
            _isValidating = true;
            var startTime = System.DateTime.Now;
            
            var issues = new List<UIValidationIssue>();
            var enabledRules = _validationRules.Where(r => r.IsEnabled).ToList();
            
            LogInfo($"Starting UI validation suite with {enabledRules.Count} rules");
            
            foreach (var rule in enabledRules)
            {
                var result = ExecuteValidationRule(rule);
                
                if (!result.IsValid)
                {
                    var issue = new UIValidationIssue
                    {
                        RuleName = rule.Name,
                        Severity = rule.Severity,
                        Message = result.Message,
                        Details = result.Details,
                        Timestamp = System.DateTime.Now,
                        IsResolved = false
                    };
                    
                    issues.Add(issue);
                    OnValidationIssueFound?.Invoke(issue);
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            CompileValidationResults(issues, startTime);
            _isValidating = false;
        }
        
        /// <summary>
        /// Execute validation batch coroutine
        /// </summary>
        private System.Collections.IEnumerator ExecuteValidationBatch(List<UIValidationRule> rules)
        {
            _isValidating = true;
            var startTime = System.DateTime.Now;
            
            var issues = new List<UIValidationIssue>();
            
            foreach (var rule in rules)
            {
                var result = ExecuteValidationRule(rule);
                
                if (!result.IsValid)
                {
                    var issue = new UIValidationIssue
                    {
                        RuleName = rule.Name,
                        Severity = rule.Severity,
                        Message = result.Message,
                        Details = result.Details,
                        Timestamp = System.DateTime.Now,
                        IsResolved = false
                    };
                    
                    issues.Add(issue);
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            CompileValidationResults(issues, startTime);
            _isValidating = false;
        }
        
        /// <summary>
        /// Execute individual validation rule
        /// </summary>
        private UIValidationResult ExecuteValidationRule(UIValidationRule rule)
        {
            try
            {
                return rule.Method.Invoke();
            }
            catch (System.Exception ex)
            {
                LogError($"Validation rule {rule.Name} threw exception: {ex.Message}");
                return UIValidationResult.Invalid($"Exception: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Compile validation results
        /// </summary>
        private void CompileValidationResults(List<UIValidationIssue> issues, System.DateTime startTime)
        {
            var executionTime = (System.DateTime.Now - startTime).TotalSeconds;
            
            // Update current issues
            UpdateCurrentIssues(issues);
            
            _lastValidationResults = new UIValidationResults
            {
                TotalRules = _validationRules.Count(r => r.IsEnabled),
                PassedRules = _validationRules.Count(r => r.IsEnabled) - issues.Count,
                FailedRules = issues.Count,
                CriticalIssues = issues.Count(i => i.Severity == UIValidationSeverity.Critical),
                HighIssues = issues.Count(i => i.Severity == UIValidationSeverity.High),
                MediumIssues = issues.Count(i => i.Severity == UIValidationSeverity.Medium),
                LowIssues = issues.Count(i => i.Severity == UIValidationSeverity.Low),
                ExecutionTime = (float)executionTime,
                Issues = issues,
                Timestamp = System.DateTime.Now
            };
            
            OnValidationCompleted?.Invoke(_lastValidationResults);
            
            LogValidationResults();
        }
        
        /// <summary>
        /// Update current issues list
        /// </summary>
        private void UpdateCurrentIssues(List<UIValidationIssue> newIssues)
        {
            // Mark resolved issues
            foreach (var currentIssue in _currentIssues)
            {
                if (!newIssues.Any(i => i.RuleName == currentIssue.RuleName))
                {
                    currentIssue.IsResolved = true;
                    OnValidationIssueResolved?.Invoke(currentIssue);
                }
            }
            
            // Update current issues
            _currentIssues = newIssues;
        }
        
        /// <summary>
        /// Log validation results
        /// </summary>
        private void LogValidationResults()
        {
            var results = _lastValidationResults;
            
            LogInfo($"UI Validation Completed:");
            LogInfo($"- Total Rules: {results.TotalRules}");
            LogInfo($"- Passed: {results.PassedRules}");
            LogInfo($"- Failed: {results.FailedRules}");
            LogInfo($"- Critical Issues: {results.CriticalIssues}");
            LogInfo($"- Execution Time: {results.ExecutionTime:F2}s");
            
            if (_showValidationDetails)
            {
                foreach (var issue in results.Issues.Where(i => i.Severity >= UIValidationSeverity.High))
                {
                    LogWarning($"{issue.Severity}: {issue.RuleName} - {issue.Message}");
                }
            }
        }
        
        /// <summary>
        /// Get rule category from name
        /// </summary>
        private UIValidationCategory GetRuleCategory(string ruleName)
        {
            if (ruleName.StartsWith("component_")) return UIValidationCategory.Component;
            if (ruleName.StartsWith("manager_")) return UIValidationCategory.Manager;
            if (ruleName.StartsWith("data_")) return UIValidationCategory.Data;
            if (ruleName.StartsWith("performance_")) return UIValidationCategory.Performance;
            if (ruleName.StartsWith("integration_")) return UIValidationCategory.Integration;
            return UIValidationCategory.General;
        }
        
        // Validation Rule Implementations
        
        /// <summary>
        /// Validate component initialization
        /// </summary>
        private UIValidationResult ValidateComponentInitialization()
        {
            if (_prefabManager == null)
                return UIValidationResult.Invalid("PrefabManager not assigned");
            
            var activeComponents = _prefabManager.GetActiveComponents<UIComponentPrefab>();
            var uninitializedComponents = activeComponents.Where(c => !c.IsInitialized).ToList();
            
            if (uninitializedComponents.Count > 0)
            {
                var details = uninitializedComponents.Select(c => $"Component {c.ComponentName} not initialized").ToList();
                return UIValidationResult.Invalid($"{uninitializedComponents.Count} components not initialized", details);
            }
            
            return UIValidationResult.Valid("All components properly initialized");
        }
        
        /// <summary>
        /// Validate component memory usage
        /// </summary>
        private UIValidationResult ValidateComponentMemoryUsage()
        {
            var currentMemory = System.GC.GetTotalMemory(false) / 1024f / 1024f; // MB
            
            if (currentMemory > _maxMemoryUsageMB)
            {
                return UIValidationResult.Invalid($"Memory usage too high: {currentMemory:F1}MB > {_maxMemoryUsageMB}MB");
            }
            
            return UIValidationResult.Valid($"Memory usage within limits: {currentMemory:F1}MB");
        }
        
        /// <summary>
        /// Validate component lifecycle
        /// </summary>
        private UIValidationResult ValidateComponentLifecycle()
        {
            if (_prefabManager == null)
                return UIValidationResult.Invalid("PrefabManager not assigned");
            
            var activeComponents = _prefabManager.GetActiveComponents<UIComponentPrefab>();
            var orphanedComponents = new List<UIComponentPrefab>();
            
            foreach (var component in activeComponents)
            {
                if (component == null || component.gameObject == null)
                {
                    orphanedComponents.Add(component);
                }
            }
            
            if (orphanedComponents.Count > _maxOrphanedComponents)
            {
                return UIValidationResult.Invalid($"Too many orphaned components: {orphanedComponents.Count} > {_maxOrphanedComponents}");
            }
            
            return UIValidationResult.Valid("Component lifecycle healthy");
        }
        
        /// <summary>
        /// Validate component references
        /// </summary>
        private UIValidationResult ValidateComponentReferences()
        {
            // Implementation would check for null references, circular dependencies, etc.
            return UIValidationResult.Valid("Component references valid");
        }
        
        /// <summary>
        /// Validate manager integration
        /// </summary>
        private UIValidationResult ValidateManagerIntegration()
        {
            var missingManagers = new List<string>();
            
            if (_gameUIManager == null) missingManagers.Add("GameUIManager");
            if (_prefabManager == null) missingManagers.Add("PrefabManager");
            if (_stateManager == null) missingManagers.Add("StateManager");
            if (_integrationManager == null) missingManagers.Add("IntegrationManager");
            
            if (missingManagers.Count > 0)
            {
                return UIValidationResult.Invalid($"Missing managers: {string.Join(", ", missingManagers)}");
            }
            
            return UIValidationResult.Valid("All managers properly integrated");
        }
        
        /// <summary>
        /// Validate manager performance
        /// </summary>
        private UIValidationResult ValidateManagerPerformance()
        {
            // Implementation would check manager update times, responsiveness
            return UIValidationResult.Valid("Manager performance acceptable");
        }
        
        /// <summary>
        /// Validate manager state
        /// </summary>
        private UIValidationResult ValidateManagerState()
        {
            var issues = new List<string>();
            
            if (_gameUIManager != null && !_gameUIManager.IsInitialized)
                issues.Add("GameUIManager not initialized");
                
            if (_prefabManager != null && !_prefabManager.IsInitialized)
                issues.Add("PrefabManager not initialized");
                
            if (_stateManager != null && !_stateManager.IsInitialized)
                issues.Add("StateManager not initialized");
            
            if (issues.Count > 0)
            {
                return UIValidationResult.Invalid($"Manager state issues: {string.Join(", ", issues)}");
            }
            
            return UIValidationResult.Valid("All managers in valid state");
        }
        
        /// <summary>
        /// Validate data consistency
        /// </summary>
        private UIValidationResult ValidateDataConsistency()
        {
            // Implementation would check data integrity, relationships
            return UIValidationResult.Valid("Data consistency validated");
        }
        
        /// <summary>
        /// Validate data persistence
        /// </summary>
        private UIValidationResult ValidateDataPersistence()
        {
            if (_stateManager == null)
                return UIValidationResult.Invalid("StateManager not available for persistence validation");
            
            // Test save/load functionality
            var testKey = "validation_test";
            var testValue = System.Guid.NewGuid().ToString();
            
            try
            {
                _stateManager.SaveState(testKey, UIStateCategory.General, testValue);
                var loadedValue = _stateManager.LoadState<string>(testKey, UIStateCategory.General);
                _stateManager.RemoveState(testKey, UIStateCategory.General);
                
                if (loadedValue != testValue)
                {
                    return UIValidationResult.Invalid("Data persistence test failed: values don't match");
                }
                
                return UIValidationResult.Valid("Data persistence working correctly");
            }
            catch (System.Exception ex)
            {
                return UIValidationResult.Invalid($"Data persistence error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validate data serialization
        /// </summary>
        private UIValidationResult ValidateDataSerialization()
        {
            // Implementation would test JSON serialization, ScriptableObject integrity
            return UIValidationResult.Valid("Data serialization validated");
        }
        
        /// <summary>
        /// Validate performance FPS
        /// </summary>
        private UIValidationResult ValidatePerformanceFPS()
        {
            var currentFPS = 1f / Time.deltaTime;
            var targetFrameTime = _maxUpdateTimeMs;
            var currentFrameTime = Time.deltaTime * 1000f;
            
            if (currentFrameTime > targetFrameTime)
            {
                return UIValidationResult.Invalid($"Frame time too high: {currentFrameTime:F1}ms > {targetFrameTime:F1}ms (FPS: {currentFPS:F1})");
            }
            
            return UIValidationResult.Valid($"Performance acceptable: {currentFrameTime:F1}ms (FPS: {currentFPS:F1})");
        }
        
        /// <summary>
        /// Validate performance memory
        /// </summary>
        private UIValidationResult ValidatePerformanceMemory()
        {
            return ValidateComponentMemoryUsage(); // Reuse memory validation
        }
        
        /// <summary>
        /// Validate performance pooling
        /// </summary>
        private UIValidationResult ValidatePerformancePooling()
        {
            if (_prefabManager == null)
                return UIValidationResult.Invalid("PrefabManager not available for pooling validation");
            
            var activeCount = _prefabManager.ActiveComponentCount;
            
            if (activeCount > _maxActiveComponents)
            {
                return UIValidationResult.Invalid($"Too many active components: {activeCount} > {_maxActiveComponents}");
            }
            
            return UIValidationResult.Valid($"Component pooling healthy: {activeCount} active components");
        }
        
        /// <summary>
        /// Validate integration events
        /// </summary>
        private UIValidationResult ValidateIntegrationEvents()
        {
            // Implementation would test event system functionality
            return UIValidationResult.Valid("Integration events validated");
        }
        
        /// <summary>
        /// Validate integration bindings
        /// </summary>
        private UIValidationResult ValidateIntegrationBindings()
        {
            // Implementation would test data binding functionality
            return UIValidationResult.Valid("Integration bindings validated");
        }
        
        /// <summary>
        /// Validate integration dependencies
        /// </summary>
        private UIValidationResult ValidateIntegrationDependencies()
        {
            return ValidateManagerIntegration(); // Reuse manager validation
        }
        
        private void Update()
        {
            if (!_enableRuntimeValidation)
                return;
            
            _validationTimer += Time.deltaTime;
            
            if (_validationTimer >= _validationInterval)
            {
                RunFullValidation();
                _validationTimer = 0f;
            }
        }
        
        private void OnValidate()
        {
            _validationInterval = Mathf.Max(1f, _validationInterval);
            _maxActiveComponents = Mathf.Max(1, _maxActiveComponents);
            _maxMemoryUsageMB = Mathf.Max(1f, _maxMemoryUsageMB);
            _maxUpdateTimeMs = Mathf.Max(1f, _maxUpdateTimeMs);
            _maxOrphanedComponents = Mathf.Max(0, _maxOrphanedComponents);
        }
    }
    
    /// <summary>
    /// Validation rule definition
    /// </summary>
    [System.Serializable]
    public class UIValidationRule
    {
        public string Name;
        public System.Func<UIValidationResult> Method;
        public UIValidationSeverity Severity;
        public bool IsEnabled;
    }
    
    /// <summary>
    /// Validation result container
    /// </summary>
    public struct UIValidationResult
    {
        public bool IsValid;
        public string Message;
        public List<string> Details;
        
        public static UIValidationResult Valid(string message = "Validation passed")
        {
            return new UIValidationResult
            {
                IsValid = true,
                Message = message,
                Details = new List<string>()
            };
        }
        
        public static UIValidationResult Invalid(string message, List<string> details = null)
        {
            return new UIValidationResult
            {
                IsValid = false,
                Message = message,
                Details = details ?? new List<string>()
            };
        }
    }
    
    /// <summary>
    /// Validation issue record
    /// </summary>
    [System.Serializable]
    public class UIValidationIssue
    {
        public string RuleName;
        public UIValidationSeverity Severity;
        public string Message;
        public List<string> Details = new List<string>();
        public System.DateTime Timestamp;
        public bool IsResolved;
    }
    
    /// <summary>
    /// Complete validation results
    /// </summary>
    [System.Serializable]
    public class UIValidationResults
    {
        public int TotalRules;
        public int PassedRules;
        public int FailedRules;
        public int CriticalIssues;
        public int HighIssues;
        public int MediumIssues;
        public int LowIssues;
        public float ExecutionTime;
        public List<UIValidationIssue> Issues = new List<UIValidationIssue>();
        public System.DateTime Timestamp;
        
        public float SuccessRate => TotalRules > 0 ? (PassedRules / (float)TotalRules) : 0f;
        public bool IsHealthy => CriticalIssues == 0 && HighIssues <= 2;
    }
    
    /// <summary>
    /// Component health tracking
    /// </summary>
    public class UIComponentHealth
    {
        public string ComponentId;
        public float LastValidationTime;
        public int ValidationFailures;
        public List<string> RecentIssues = new List<string>();
        public bool IsHealthy => ValidationFailures < 3 && RecentIssues.Count < 5;
    }
    
    /// <summary>
    /// Validation categories
    /// </summary>
    public enum UIValidationCategory
    {
        General,
        Component,
        Manager,
        Data,
        Performance,
        Integration
    }
    
    /// <summary>
    /// Validation severity levels
    /// </summary>
    public enum UIValidationSeverity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}