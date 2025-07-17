using UnityEngine;
using UnityEditor;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Systems.Tutorial.Testing;
// Explicit alias for Systems layer TutorialValidationResult to resolve ambiguity
using SystemsTutorialValidationResult = ProjectChimera.Systems.Tutorial.Testing.TutorialValidationResult;
using System.Collections.Generic;

namespace ProjectChimera.Editor.Tutorial
{
    /// <summary>
    /// Editor window for tutorial validation and testing tools.
    /// Provides comprehensive validation interface for tutorial content.
    /// </summary>
    public class TutorialValidationWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private TutorialDataAssetManager _selectedDataManager;
        private SystemsTutorialValidationResult _lastValidationResult;
        private string _validationReport = "";
        private bool _showDetailedResults = true;
        private bool _autoRefresh = false;
        
        // GUI styles
        private GUIStyle _headerStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _successStyle;
        private bool _stylesInitialized = false;
        
        [MenuItem("Project Chimera/Tutorial/Validation Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<TutorialValidationWindow>("Tutorial Validation");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            // Find tutorial data manager if not selected
            if (_selectedDataManager == null)
            {
                var guids = AssetDatabase.FindAssets("t:TutorialDataAssetManager");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _selectedDataManager = AssetDatabase.LoadAssetAtPath<TutorialDataAssetManager>(path);
                }
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            DrawManagerSelection();
            DrawValidationControls();
            DrawValidationResults();
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// Initialize GUI styles
        /// </summary>
        private void InitializeStyles()
        {
            if (_stylesInitialized) return;
            
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            _errorStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = Color.red }
            };
            
            _warningStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = new Color(1f, 0.6f, 0f) }
            };
            
            _successStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = Color.green }
            };
            
            _stylesInitialized = true;
        }
        
        /// <summary>
        /// Draw window header
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Validation & Testing Tools", _headerStyle);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Comprehensive validation tools for Project Chimera tutorial system", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// Draw data manager selection
        /// </summary>
        private void DrawManagerSelection()
        {
            EditorGUILayout.LabelField("Data Manager Selection", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var newManager = EditorGUILayout.ObjectField("Tutorial Data Manager", 
                _selectedDataManager, typeof(TutorialDataAssetManager), false) as TutorialDataAssetManager;
            
            if (newManager != _selectedDataManager)
            {
                _selectedDataManager = newManager;
                _lastValidationResult = null;
                _validationReport = "";
            }
            
            if (_selectedDataManager == null)
            {
                EditorGUILayout.HelpBox("Please select a Tutorial Data Asset Manager to begin validation.", MessageType.Info);
                
                // Auto-find button
                if (GUILayout.Button("Find Tutorial Data Manager"))
                {
                    FindTutorialDataManager();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Selected: {_selectedDataManager.name}");
                
                // Show basic info
                var stats = _selectedDataManager.GetTutorialStatistics();
                EditorGUILayout.LabelField($"Sequences: {stats.TotalSequences}, Steps: {stats.TotalSteps}");
                EditorGUILayout.LabelField($"Estimated Duration: {stats.TotalEstimatedDuration:F1} minutes");
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// Draw validation controls
        /// </summary>
        private void DrawValidationControls()
        {
            EditorGUILayout.LabelField("Validation Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Settings
            EditorGUILayout.BeginHorizontal();
            _showDetailedResults = EditorGUILayout.Toggle("Show Detailed Results", _showDetailedResults);
            _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Validation buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = _selectedDataManager != null;
            
            if (GUILayout.Button("Validate All", GUILayout.Height(30)))
            {
                RunFullValidation();
            }
            
            if (GUILayout.Button("Quick Check", GUILayout.Height(30)))
            {
                RunQuickValidation();
            }
            
            if (GUILayout.Button("Generate Report", GUILayout.Height(30)))
            {
                GenerateDetailedReport();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Additional tools
            EditorGUILayout.LabelField("Additional Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Find Missing Assets"))
            {
                FindMissingAssets();
            }
            
            if (GUILayout.Button("Check Duplicates"))
            {
                CheckForDuplicates();
            }
            
            if (GUILayout.Button("Validate Step IDs"))
            {
                ValidateStepIds();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// Draw validation results
        /// </summary>
        private void DrawValidationResults()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            
            if (_lastValidationResult != null)
            {
                DrawValidationSummary();
                
                if (_showDetailedResults)
                {
                    DrawDetailedResults();
                }
            }
            else if (!string.IsNullOrEmpty(_validationReport))
            {
                DrawValidationReport();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("No validation results available. Run validation to see results.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Draw validation summary
        /// </summary>
        private void DrawValidationSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Summary header
            if (_lastValidationResult.IsValid)
            {
                EditorGUILayout.LabelField("✅ Validation Passed", _successStyle);
            }
            else
            {
                EditorGUILayout.LabelField("❌ Validation Failed", _errorStyle);
            }
            
            // Counts
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Errors: {_lastValidationResult.ErrorCount}");
            EditorGUILayout.LabelField($"Warnings: {_lastValidationResult.WarningCount}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draw detailed results
        /// </summary>
        private void DrawDetailedResults()
        {
            if (_lastValidationResult.ErrorCount > 0)
            {
                EditorGUILayout.LabelField("Errors", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(_errorStyle);
                
                foreach (var error in _lastValidationResult.Errors)
                {
                    EditorGUILayout.LabelField($"❌ {error}");
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            if (_lastValidationResult.WarningCount > 0)
            {
                EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(_warningStyle);
                
                foreach (var warning in _lastValidationResult.Warnings)
                {
                    EditorGUILayout.LabelField($"⚠️  {warning}");
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// Draw validation report
        /// </summary>
        private void DrawValidationReport()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Validation Report", EditorStyles.boldLabel);
            
            var reportScrollPosition = Vector2.zero;
            reportScrollPosition = EditorGUILayout.BeginScrollView(reportScrollPosition, GUILayout.Height(200));
            
            EditorGUILayout.TextArea(_validationReport, GUILayout.ExpandHeight(true));
            
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Copy Report to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = _validationReport;
                Debug.Log("Validation report copied to clipboard");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Find tutorial data manager automatically
        /// </summary>
        private void FindTutorialDataManager()
        {
            var guids = AssetDatabase.FindAssets("t:TutorialDataAssetManager");
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Not Found", "No Tutorial Data Asset Manager found in project.", "OK");
                return;
            }
            
            if (guids.Length == 1)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _selectedDataManager = AssetDatabase.LoadAssetAtPath<TutorialDataAssetManager>(path);
                Debug.Log($"Found and selected Tutorial Data Manager: {_selectedDataManager.name}");
            }
            else
            {
                // Multiple found - show selection dialog
                var options = new List<string>();
                var managers = new List<TutorialDataAssetManager>();
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var manager = AssetDatabase.LoadAssetAtPath<TutorialDataAssetManager>(path);
                    options.Add(manager.name);
                    managers.Add(manager);
                }
                
                var selection = EditorUtility.DisplayDialogComplex("Multiple Found", 
                    $"Found {guids.Length} Tutorial Data Asset Managers. Select one:", 
                    options[0], options.Count > 1 ? options[1] : "Cancel", "Cancel");
                
                if (selection < managers.Count)
                {
                    _selectedDataManager = managers[selection];
                }
            }
        }
        
        /// <summary>
        /// Run full validation
        /// </summary>
        private void RunFullValidation()
        {
            if (_selectedDataManager == null) return;
            
            _lastValidationResult = TutorialValidationUtilities.ValidateDataAssetManager(_selectedDataManager);
            _validationReport = "";
            
            string resultText = _lastValidationResult.IsValid ? "Validation passed!" : "Validation failed!";
            Debug.Log($"Tutorial validation complete: {resultText} ({_lastValidationResult.ErrorCount} errors, {_lastValidationResult.WarningCount} warnings)");
        }
        
        /// <summary>
        /// Run quick validation
        /// </summary>
        private void RunQuickValidation()
        {
            if (_selectedDataManager == null) return;
            
            bool isValid = _selectedDataManager.ValidateData();
            _lastValidationResult = new SystemsTutorialValidationResult();
            
            if (!isValid)
            {
                _lastValidationResult.AddError("Data manager validation failed - check console for details");
            }
            
            _validationReport = "";
            
            Debug.Log($"Quick validation complete: {(isValid ? "Passed" : "Failed")}");
        }
        
        /// <summary>
        /// Generate detailed report
        /// </summary>
        private void GenerateDetailedReport()
        {
            if (_selectedDataManager == null) return;
            
            _validationReport = TutorialValidationUtilities.GenerateValidationReport(_selectedDataManager);
            _lastValidationResult = null;
            
            Debug.Log("Detailed validation report generated");
        }
        
        /// <summary>
        /// Find missing assets
        /// </summary>
        private void FindMissingAssets()
        {
            if (_selectedDataManager == null) return;
            
            var missingCount = 0;
            var sequences = _selectedDataManager.AvailableSequences;
            
            foreach (var sequence in sequences)
            {
                if (sequence == null)
                {
                    missingCount++;
                    continue;
                }
                
                foreach (var step in sequence.Steps)
                {
                    if (step == null)
                    {
                        missingCount++;
                    }
                }
            }
            
            EditorUtility.DisplayDialog("Missing Assets Check", 
                $"Found {missingCount} missing asset references.", "OK");
        }
        
        /// <summary>
        /// Check for duplicates
        /// </summary>
        private void CheckForDuplicates()
        {
            if (_selectedDataManager == null) return;
            
            var stepIds = new HashSet<string>();
            var duplicates = new List<string>();
            
            foreach (var sequence in _selectedDataManager.AvailableSequences)
            {
                if (sequence == null) continue;
                
                foreach (var step in sequence.Steps)
                {
                    if (step == null) continue;
                    
                    if (stepIds.Contains(step.StepId))
                    {
                        duplicates.Add(step.StepId);
                    }
                    else
                    {
                        stepIds.Add(step.StepId);
                    }
                }
            }
            
            EditorUtility.DisplayDialog("Duplicate Check", 
                $"Found {duplicates.Count} duplicate step IDs.", "OK");
        }
        
        /// <summary>
        /// Validate step IDs
        /// </summary>
        private void ValidateStepIds()
        {
            if (_selectedDataManager == null) return;
            
            var invalidIds = new List<string>();
            
            foreach (var sequence in _selectedDataManager.AvailableSequences)
            {
                if (sequence == null) continue;
                
                foreach (var step in sequence.Steps)
                {
                    if (step == null) continue;
                    
                    if (string.IsNullOrEmpty(step.StepId) || !IsValidStepId(step.StepId))
                    {
                        invalidIds.Add(step.StepId ?? "Empty ID");
                    }
                }
            }
            
            EditorUtility.DisplayDialog("Step ID Validation", 
                $"Found {invalidIds.Count} invalid step IDs.", "OK");
        }
        
        /// <summary>
        /// Check if step ID is valid
        /// </summary>
        private bool IsValidStepId(string stepId)
        {
            return !string.IsNullOrEmpty(stepId) && 
                   stepId == stepId.ToLower() && 
                   stepId.Contains("_") && 
                   !stepId.StartsWith("_") && 
                   !stepId.EndsWith("_");
        }
        
        private void Update()
        {
            if (_autoRefresh && _selectedDataManager != null && _lastValidationResult != null)
            {
                // Auto-refresh every few seconds
                if (EditorApplication.timeSinceStartup % 5 < 0.1f)
                {
                    RunQuickValidation();
                    Repaint();
                }
            }
        }
    }
}