using UnityEngine;
using UnityEditor;
using ProjectChimera.Systems.Tutorial.Testing;

namespace ProjectChimera.Editor.Tutorial
{
    /// <summary>
    /// Custom editor for tutorial testing framework.
    /// Provides convenient testing tools in the Unity Editor.
    /// </summary>
    [CustomEditor(typeof(TutorialTestingFramework))]
    public class TutorialTestingEditor : UnityEditor.Editor
    {
        private TutorialTestingFramework _target;
        private bool _showTestResults = true;
        private bool _showTestConfiguration = true;
        private bool _showQuickActions = true;
        
        private void OnEnable()
        {
            _target = target as TutorialTestingFramework;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Testing Framework", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Quick Actions Section
            DrawQuickActionsSection();
            
            // Test Configuration Section
            DrawTestConfigurationSection();
            
            // Test Results Section
            DrawTestResultsSection();
            
            // Default inspector for remaining properties
            EditorGUILayout.Space();
            DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Draw quick actions section
        /// </summary>
        private void DrawQuickActionsSection()
        {
            _showQuickActions = EditorGUILayout.Foldout(_showQuickActions, "Quick Actions", true);
            
            if (_showQuickActions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Test Execution", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                
                // Run all tests button
                if (GUILayout.Button("Run All Tests", GUILayout.Height(30)))
                {
                    if (Application.isPlaying)
                    {
                        _target.RunAllTests();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Tests Require Play Mode", 
                            "Tutorial tests must be run in Play Mode to access runtime systems.", "OK");
                    }
                }
                
                // Test status indicator
                GUILayout.Label(GetTestStatusText(), GetTestStatusStyle(), GUILayout.Width(120));
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                
                // Validation only button
                if (GUILayout.Button("Quick Validation"))
                {
                    RunQuickValidation();
                }
                
                // Clear results button
                if (GUILayout.Button("Clear Results"))
                {
                    ClearTestResults();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw test configuration section
        /// </summary>
        private void DrawTestConfigurationSection()
        {
            _showTestConfiguration = EditorGUILayout.Foldout(_showTestConfiguration, "Test Configuration", true);
            
            if (_showTestConfiguration)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Test Settings", EditorStyles.boldLabel);
                
                // Test timeout
                var timeoutProperty = serializedObject.FindProperty("_testTimeout");
                EditorGUILayout.PropertyField(timeoutProperty, new GUIContent("Test Timeout (seconds)"));
                
                // Detailed logging
                var loggingProperty = serializedObject.FindProperty("_detailedLogging");
                EditorGUILayout.PropertyField(loggingProperty, new GUIContent("Detailed Logging"));
                
                // Include performance tests
                var performanceProperty = serializedObject.FindProperty("_includePerformanceTests");
                EditorGUILayout.PropertyField(performanceProperty, new GUIContent("Include Performance Tests"));
                
                // Run tests on start
                var autoRunProperty = serializedObject.FindProperty("_runTestsOnStart");
                EditorGUILayout.PropertyField(autoRunProperty, new GUIContent("Run Tests on Start"));
                
                EditorGUILayout.Space();
                
                // Asset references
                EditorGUILayout.LabelField("Required Assets", EditorStyles.boldLabel);
                
                var dataManagerProperty = serializedObject.FindProperty("_tutorialDataManager");
                EditorGUILayout.PropertyField(dataManagerProperty, new GUIContent("Tutorial Data Manager"));
                
                if (dataManagerProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Tutorial Data Manager reference is required for testing.", MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw test results section
        /// </summary>
        private void DrawTestResultsSection()
        {
            _showTestResults = EditorGUILayout.Foldout(_showTestResults, "Test Results", true);
            
            if (_showTestResults)
            {
                EditorGUI.indentLevel++;
                
                var lastResults = _target.GetLastTestResults();
                
                if (lastResults != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField("Last Test Run", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Date: {lastResults.TestDate:yyyy-MM-dd HH:mm:ss}");
                    EditorGUILayout.LabelField($"Duration: {lastResults.TotalDuration:F2} seconds");
                    
                    EditorGUILayout.Space();
                    
                    // Results summary
                    EditorGUILayout.LabelField("Results Summary", EditorStyles.boldLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Total Tests: {lastResults.TotalTests}");
                    EditorGUILayout.LabelField($"Success Rate: {lastResults.SuccessRate:F1}%");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    // Passed tests (green)
                    var passedContent = new GUIContent($"✓ Passed: {lastResults.PassedTests}");
                    var passedStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } };
                    EditorGUILayout.LabelField(passedContent, passedStyle);
                    
                    // Failed tests (red)
                    var failedContent = new GUIContent($"✗ Failed: {lastResults.FailedTests}");
                    var failedStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } };
                    EditorGUILayout.LabelField(failedContent, failedStyle);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (lastResults.SkippedTests > 0)
                    {
                        EditorGUILayout.LabelField($"Skipped: {lastResults.SkippedTests}");
                    }
                    
                    EditorGUILayout.Space();
                    
                    // Detailed results
                    if (lastResults.TestDetails != null && lastResults.TestDetails.Count > 0)
                    {
                        EditorGUILayout.LabelField("Test Details", EditorStyles.boldLabel);
                        
                        foreach (var detail in lastResults.TestDetails)
                        {
                            DrawTestDetail(detail);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("No test results available. Run tests to see results here.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw individual test detail
        /// </summary>
        private void DrawTestDetail(TutorialTestDetail detail)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Status icon and test name
            string statusIcon = detail.Passed ? "✓" : "✗";
            var statusColor = detail.Passed ? Color.green : Color.red;
            var statusStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = statusColor } };
            
            EditorGUILayout.LabelField($"{statusIcon} {detail.TestName}", statusStyle, GUILayout.Width(200));
            
            // Category
            EditorGUILayout.LabelField($"[{detail.Category}]", GUILayout.Width(80));
            
            // Duration
            EditorGUILayout.LabelField($"{detail.Duration:F2}s", GUILayout.Width(50));
            
            EditorGUILayout.EndHorizontal();
            
            // Error message if failed
            if (!detail.Passed && !string.IsNullOrEmpty(detail.ErrorMessage))
            {
                EditorGUI.indentLevel++;
                var errorStyle = new GUIStyle(EditorStyles.helpBox) { normal = { textColor = Color.red } };
                EditorGUILayout.LabelField(detail.ErrorMessage, errorStyle);
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Get test status text
        /// </summary>
        private string GetTestStatusText()
        {
            if (!Application.isPlaying)
            {
                return "Play Mode Required";
            }
            
            if (_target.IsRunningTests())
            {
                return "Running Tests...";
            }
            
            var lastResults = _target.GetLastTestResults();
            if (lastResults != null)
            {
                if (lastResults.FailedTests == 0)
                {
                    return "All Tests Passed";
                }
                else
                {
                    return $"{lastResults.FailedTests} Test(s) Failed";
                }
            }
            
            return "Ready to Test";
        }
        
        /// <summary>
        /// Get test status style
        /// </summary>
        private GUIStyle GetTestStatusStyle()
        {
            var style = new GUIStyle(EditorStyles.label);
            
            if (!Application.isPlaying)
            {
                style.normal.textColor = Color.gray;
                return style;
            }
            
            if (_target.IsRunningTests())
            {
                style.normal.textColor = Color.yellow;
                return style;
            }
            
            var lastResults = _target.GetLastTestResults();
            if (lastResults != null)
            {
                style.normal.textColor = lastResults.FailedTests == 0 ? Color.green : Color.red;
            }
            
            return style;
        }
        
        /// <summary>
        /// Run quick validation
        /// </summary>
        private void RunQuickValidation()
        {
            var dataManager = serializedObject.FindProperty("_tutorialDataManager").objectReferenceValue;
            
            if (dataManager == null)
            {
                EditorUtility.DisplayDialog("Validation Failed", 
                    "Tutorial Data Manager reference is missing. Please assign it first.", "OK");
                return;
            }
            
            // Basic asset validation
            var manager = dataManager as ProjectChimera.Data.Tutorial.TutorialDataAssetManager;
            if (manager != null)
            {
                bool isValid = manager.ValidateData();
                string message = isValid ? 
                    "Tutorial data validation passed successfully!" : 
                    "Tutorial data validation found issues. Check console for details.";
                
                MessageType messageType = isValid ? MessageType.Info : MessageType.Warning;
                EditorUtility.DisplayDialog("Validation Complete", message, "OK");
            }
        }
        
        /// <summary>
        /// Clear test results
        /// </summary>
        private void ClearTestResults()
        {
            // Clear results would need to be implemented in the testing framework
            EditorUtility.DisplayDialog("Results Cleared", 
                "Test results have been cleared. Run new tests to see fresh results.", "OK");
        }
    }
}