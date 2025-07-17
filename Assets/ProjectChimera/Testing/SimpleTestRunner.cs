using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Simple Test Runner that works even with compilation errors in other assemblies
    /// </summary>
    public class SimpleTestRunner : EditorWindow
    {
        [MenuItem("Project Chimera/Testing/Simple Test Runner")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimpleTestRunner>("Simple Test Runner");
            window.Show();
        }

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            GUILayout.Label("Project Chimera - Simple Test Runner", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Basic Information
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Compilation Status", EditorStyles.boldLabel);
            GUILayout.Label("This is a simplified test runner that bypasses compilation errors.");
            GUILayout.Label("To access the full AutomatedTestRunner, fix compilation errors first.");
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            // Basic Actions
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Available Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open Unity Test Runner"))
            {
                EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
            }
            
            if (GUILayout.Button("Open Console"))
            {
                EditorApplication.ExecuteMenuItem("Window/General/Console");
            }
            
            if (GUILayout.Button("Refresh Compilation"))
            {
                AssetDatabase.Refresh();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            // Status Info
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Current Status", EditorStyles.boldLabel);
            GUILayout.Label($"Editor Compiling: {EditorApplication.isCompiling}");
            GUILayout.Label($"Play Mode: {EditorApplication.isPlaying}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
        }
    }
} 