using UnityEngine;
using UnityEditor;
using ProjectChimera.Data.Tutorial;
using System.Linq;

namespace ProjectChimera.Editor.Tutorial
{
    /// <summary>
    /// Custom inspector for tutorial data asset manager.
    /// Provides tools for managing and validating tutorial assets.
    /// </summary>
    [CustomEditor(typeof(TutorialDataAssetManager))]
    public class TutorialDataInspector : UnityEditor.Editor
    {
        private TutorialDataAssetManager _target;
        private bool _showStatistics = true;
        private bool _showSequences = true;
        private bool _showValidation = false;
        private bool _showAssetCreation = false;
        
        private void OnEnable()
        {
            _target = target as TutorialDataAssetManager;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tutorial Data Asset Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Statistics Section
            DrawStatisticsSection();
            
            // Sequences Section
            DrawSequencesSection();
            
            // Asset Creation Section
            DrawAssetCreationSection();
            
            // Validation Section
            DrawValidationSection();
            
            // Default inspector for remaining properties
            EditorGUILayout.Space();
            DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Draw statistics section
        /// </summary>
        private void DrawStatisticsSection()
        {
            _showStatistics = EditorGUILayout.Foldout(_showStatistics, "Tutorial Statistics", true);
            
            if (_showStatistics)
            {
                EditorGUI.indentLevel++;
                
                var stats = _target.GetTutorialStatistics();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Total Sequences: {stats.TotalSequences}");
                EditorGUILayout.LabelField($"Total Steps: {stats.TotalSteps}");
                EditorGUILayout.LabelField($"Estimated Duration: {stats.TotalEstimatedDuration:F1} minutes");
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Steps by Category", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Onboarding Steps: {stats.OnboardingSteps}");
                EditorGUILayout.LabelField($"Cultivation Steps: {stats.CultivationSteps}");
                EditorGUILayout.LabelField($"Genetics Steps: {stats.GeneticsSteps}");
                EditorGUILayout.LabelField($"Economics Steps: {stats.EconomicsSteps}");
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw sequences section
        /// </summary>
        private void DrawSequencesSection()
        {
            _showSequences = EditorGUILayout.Foldout(_showSequences, "Tutorial Sequences", true);
            
            if (_showSequences)
            {
                EditorGUI.indentLevel++;
                
                if (_target.AvailableSequences != null && _target.AvailableSequences.Count > 0)
                {
                    foreach (var sequence in _target.AvailableSequences)
                    {
                        if (sequence != null)
                        {
                            DrawSequenceInfo(sequence);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No tutorial sequences found. Use 'Create Missing Assets' to generate them.", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw sequence information
        /// </summary>
        private void DrawSequenceInfo(TutorialSequenceSO sequence)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(sequence.SequenceName, EditorStyles.boldLabel);
            
            // Add button to select the sequence asset
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = sequence;
                EditorGUIUtility.PingObject(sequence);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"ID: {sequence.SequenceId}");
            EditorGUILayout.LabelField($"Steps: {sequence.StepCount}");
            EditorGUILayout.LabelField($"Duration: {sequence.EstimatedDuration:F1} minutes");
            EditorGUILayout.LabelField($"Difficulty: {sequence.DifficultyLevel}");
            EditorGUILayout.LabelField($"Required: {(sequence.IsRequired ? "Yes" : "No")}");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draw asset creation section
        /// </summary>
        private void DrawAssetCreationSection()
        {
            _showAssetCreation = EditorGUILayout.Foldout(_showAssetCreation, "Asset Creation Tools", true);
            
            if (_showAssetCreation)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Asset Management", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Missing Assets"))
                {
                    _target.ValidateAndCreateAssets();
                    EditorUtility.DisplayDialog("Asset Creation", "Tutorial assets have been created and validated.", "OK");
                }
                
                if (GUILayout.Button("Refresh Asset References"))
                {
                    RefreshAssetReferences();
                    EditorUtility.DisplayDialog("Refresh Complete", "Asset references have been refreshed.", "OK");
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Asset Folder"))
                {
                    var assetPath = "Assets/ProjectChimera/Data/Tutorial/Generated";
                    var absolutePath = System.IO.Path.Combine(Application.dataPath.Replace("/Assets", ""), assetPath);
                    if (System.IO.Directory.Exists(absolutePath))
                    {
                        EditorUtility.RevealInFinder(absolutePath);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Folder Not Found", $"Asset folder not found at: {assetPath}", "OK");
                    }
                }
                
                if (GUILayout.Button("Reimport Tutorial Assets"))
                {
                    AssetDatabase.ImportAsset("Assets/ProjectChimera/Data/Tutorial", ImportAssetOptions.ImportRecursive);
                    EditorUtility.DisplayDialog("Reimport Complete", "Tutorial assets have been reimported.", "OK");
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Draw validation section
        /// </summary>
        private void DrawValidationSection()
        {
            _showValidation = EditorGUILayout.Foldout(_showValidation, "Validation Tools", true);
            
            if (_showValidation)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Data Validation", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate All Data"))
                {
                    bool isValid = ValidateAllTutorialData();
                    string message = isValid ? "All tutorial data is valid!" : "Validation errors found. Check console for details.";
                    MessageType messageType = isValid ? MessageType.Info : MessageType.Warning;
                    
                    EditorUtility.DisplayDialog("Validation Complete", message, "OK");
                }
                
                if (GUILayout.Button("Check for Duplicates"))
                {
                    CheckForDuplicateSteps();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate Step IDs"))
                {
                    ValidateStepIds();
                }
                
                if (GUILayout.Button("Check Asset References"))
                {
                    CheckAssetReferences();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// Refresh asset references
        /// </summary>
        private void RefreshAssetReferences()
        {
            // Find and refresh all tutorial sequence assets
            var sequenceGuids = AssetDatabase.FindAssets("t:TutorialSequenceSO");
            var sequences = sequenceGuids.Select(guid => AssetDatabase.LoadAssetAtPath<TutorialSequenceSO>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
            
            // Find and refresh all tutorial step assets
            var stepGuids = AssetDatabase.FindAssets("t:TutorialStepSO");
            var steps = stepGuids.Select(guid => AssetDatabase.LoadAssetAtPath<TutorialStepSO>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
            
            Debug.Log($"Found {sequences.Count} sequence assets and {steps.Count} step assets");
            
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// Validate all tutorial data
        /// </summary>
        private bool ValidateAllTutorialData()
        {
            bool isValid = true;
            
            // Validate the main asset
            if (!_target.ValidateData())
            {
                isValid = false;
            }
            
            // Validate all sequences
            foreach (var sequence in _target.AvailableSequences)
            {
                if (sequence != null && !sequence.ValidateData())
                {
                    isValid = false;
                }
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Check for duplicate steps
        /// </summary>
        private void CheckForDuplicateSteps()
        {
            var allStepIds = new System.Collections.Generic.HashSet<string>();
            var duplicates = new System.Collections.Generic.List<string>();
            
            // Get all step IDs from all sequences
            foreach (var sequence in _target.AvailableSequences)
            {
                if (sequence == null) continue;
                
                foreach (var step in sequence.Steps)
                {
                    if (step == null) continue;
                    
                    if (allStepIds.Contains(step.StepId))
                    {
                        duplicates.Add(step.StepId);
                    }
                    else
                    {
                        allStepIds.Add(step.StepId);
                    }
                }
            }
            
            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"Found {duplicates.Count} duplicate step IDs: {string.Join(", ", duplicates)}");
                EditorUtility.DisplayDialog("Duplicates Found", $"Found {duplicates.Count} duplicate step IDs. Check console for details.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Duplicates", "No duplicate step IDs found.", "OK");
            }
        }
        
        /// <summary>
        /// Validate step IDs
        /// </summary>
        private void ValidateStepIds()
        {
            int invalidCount = 0;
            
            foreach (var sequence in _target.AvailableSequences)
            {
                if (sequence == null) continue;
                
                foreach (var step in sequence.Steps)
                {
                    if (step == null)
                    {
                        invalidCount++;
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(step.StepId))
                    {
                        Debug.LogError($"Step '{step.Title}' has empty ID in sequence '{sequence.SequenceName}'");
                        invalidCount++;
                    }
                    else if (!IsValidStepId(step.StepId))
                    {
                        Debug.LogWarning($"Step ID '{step.StepId}' does not follow naming convention");
                    }
                }
            }
            
            string message = invalidCount > 0 ? $"Found {invalidCount} invalid step IDs. Check console for details." : "All step IDs are valid.";
            EditorUtility.DisplayDialog("Validation Complete", message, "OK");
        }
        
        /// <summary>
        /// Check if step ID follows naming convention
        /// </summary>
        private bool IsValidStepId(string stepId)
        {
            // Check if step ID follows the convention: category_module_action
            return stepId.Contains("_") && stepId.Length > 5 && stepId.ToLower() == stepId;
        }
        
        /// <summary>
        /// Check asset references
        /// </summary>
        private void CheckAssetReferences()
        {
            int missingReferences = 0;
            
            // Check sequence references
            foreach (var sequence in _target.AvailableSequences)
            {
                if (sequence == null)
                {
                    missingReferences++;
                    continue;
                }
                
                // Check step references in sequence
                foreach (var step in sequence.Steps)
                {
                    if (step == null)
                    {
                        missingReferences++;
                    }
                }
            }
            
            string message = missingReferences > 0 ? $"Found {missingReferences} missing asset references." : "All asset references are valid.";
            MessageType messageType = missingReferences > 0 ? MessageType.Warning : MessageType.Info;
            EditorUtility.DisplayDialog("Reference Check", message, "OK");
        }
    }
}