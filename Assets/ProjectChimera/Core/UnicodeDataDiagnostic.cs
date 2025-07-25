using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Diagnostic tool for detecting Unicode issues in data structures before JSON serialization.
    /// Helps identify the source of invalid surrogate pair errors.
    /// </summary>
    public static class UnicodeDataDiagnostic
    {
        /// <summary>
        /// Scan an object for potential Unicode issues before JSON serialization
        /// </summary>
        public static UnicodeIssueReport ScanObjectForUnicodeIssues<T>(T obj)
        {
            var report = new UnicodeIssueReport();
            
            if (obj == null)
            {
                report.AddIssue("Object is null");
                return report;
            }

            try
            {
                ScanObjectRecursive(obj, obj.GetType().Name, report, new HashSet<object>(), 0);
            }
            catch (Exception ex)
            {
                report.AddIssue($"Exception during scan: {ex.Message}");
            }

            return report;
        }

        /// <summary>
        /// Quick validation of a string for Unicode issues
        /// </summary>
        public static bool HasUnicodeIssues(string text, out List<UnicodeIssue> issues)
        {
            issues = new List<UnicodeIssue>();
            
            if (string.IsNullOrEmpty(text))
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                
                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]))
                    {
                        issues.Add(new UnicodeIssue
                        {
                            Position = i,
                            Character = c,
                            IssueType = "Invalid high surrogate",
                            Description = "High surrogate not followed by low surrogate"
                        });
                    }
                    else
                    {
                        // Valid surrogate pair - skip the low surrogate in next iteration
                        i++;
                    }
                }
                else if (char.IsLowSurrogate(c))
                {
                    // This is an orphaned low surrogate (not preceded by a high surrogate)
                    issues.Add(new UnicodeIssue
                    {
                        Position = i,
                        Character = c,
                        IssueType = "Orphaned low surrogate",
                        Description = "Low surrogate without preceding high surrogate"
                    });
                }
            }

            return issues.Count > 0;
        }

        /// <summary>
        /// Log a comprehensive Unicode diagnostic for an object
        /// </summary>
        public static void LogUnicodeDiagnostic<T>(T obj, string context = "")
        {
            var report = ScanObjectForUnicodeIssues(obj);
            
            if (report.HasIssues)
            {
                Debug.LogWarning($"Unicode issues detected {context}:\n{report}");
            }
            else
            {
                Debug.Log($"No Unicode issues found {context}");
            }
        }

        private static void ScanObjectRecursive(object obj, string path, UnicodeIssueReport report, HashSet<object> visited, int depth)
        {
            if (obj == null || depth > 5) // Reduced depth limit to prevent stack overflow
                return;

            // Use GetHashCode for reference types to avoid issues with object identity
            if (obj.GetType().IsClass && obj.GetType() != typeof(string) && visited.Contains(obj))
                return;

            if (obj.GetType().IsClass && obj.GetType() != typeof(string))
                visited.Add(obj);

            Type type = obj.GetType();

            // Check strings directly
            if (obj is string str)
            {
                if (HasUnicodeIssues(str, out List<UnicodeIssue> issues))
                {
                    foreach (var issue in issues)
                    {
                        report.AddIssue($"{path}: {issue.IssueType} at position {issue.Position} - {issue.Description}");
                    }
                }
                return;
            }

            // Skip primitive types, Unity objects, and system types
            if (type.IsPrimitive || 
                type == typeof(DateTime) || 
                type == typeof(decimal) ||
                type == typeof(Guid) ||
                typeof(UnityEngine.Object).IsAssignableFrom(type) ||
                type.Namespace?.StartsWith("System") == true ||
                type.Namespace?.StartsWith("UnityEngine") == true)
                return;

            // Check fields (only public and serializable fields for safety)
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    // Skip Unity-specific fields that can cause issues
                    if (field.FieldType.Namespace?.StartsWith("UnityEngine") == true)
                        continue;

                    object value = field.GetValue(obj);
                    if (value != null)
                    {
                        string fieldPath = $"{path}.{field.Name}";
                        ScanObjectRecursive(value, fieldPath, report, visited, depth + 1);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but don't add it as a Unicode issue
                    Debug.LogWarning($"Error accessing field {path}.{field.Name}: {ex.Message}");
                }
            }

            // Check properties (only simple properties for safety)
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                try
                {
                    if (property.CanRead && 
                        property.GetIndexParameters().Length == 0 &&
                        property.PropertyType.Namespace?.StartsWith("UnityEngine") != true)
                    {
                        object value = property.GetValue(obj);
                        if (value != null)
                        {
                            string propPath = $"{path}.{property.Name}";
                            ScanObjectRecursive(value, propPath, report, visited, depth + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but don't add it as a Unicode issue
                    Debug.LogWarning($"Error accessing property {path}.{property.Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Report containing Unicode issues found in an object
    /// </summary>
    public class UnicodeIssueReport
    {
        private List<string> _issues = new List<string>();

        public bool HasIssues => _issues.Count > 0;
        public int IssueCount => _issues.Count;
        public IReadOnlyList<string> Issues => _issues;

        public void AddIssue(string issue)
        {
            _issues.Add(issue);
        }

        public override string ToString()
        {
            if (!HasIssues)
                return "No Unicode issues found";

            var sb = new StringBuilder();
            sb.AppendLine($"Found {IssueCount} Unicode issues:");
            for (int i = 0; i < _issues.Count; i++)
            {
                sb.AppendLine($"  {i + 1}. {_issues[i]}");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Individual Unicode issue details
    /// </summary>
    public struct UnicodeIssue
    {
        public int Position;
        public char Character;
        public string IssueType;
        public string Description;
    }
} 