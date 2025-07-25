using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectChimera.Editor
{
    /// <summary>
    /// Generates comprehensive assembly dependency reports and validates reference integrity.
    /// </summary>
    public static class AssemblyDependencyReport
    {
        [MenuItem("Project Chimera/Assembly Analysis/Generate Dependency Report")]
        public static void GenerateFullDependencyReport()
        {
            var report = new StringBuilder();
            report.AppendLine("PROJECT CHIMERA ASSEMBLY DEPENDENCY ANALYSIS");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine();
            
            // Find all assembly definitions
            var asmdefPaths = Directory.GetFiles("Assets/ProjectChimera", "*.asmdef", SearchOption.AllDirectories);
            var assemblies = new List<AssemblyInfo>();
            
            // Parse all assemblies
            foreach (var path in asmdefPaths)
            {
                try
                {
                    var content = File.ReadAllText(path);
                    
                    var assemblyInfo = new AssemblyInfo
                    {
                        Name = ExtractJsonString(content, "name") ?? Path.GetFileNameWithoutExtension(path),
                        Path = path,
                        RootNamespace = ExtractJsonString(content, "rootNamespace") ?? "",
                        References = ExtractJsonStringArray(content, "references")
                    };
                    
                    assemblies.Add(assemblyInfo);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse {path}: {e.Message}");
                }
            }
            
            // Generate summary
            report.AppendLine($"SUMMARY");
            report.AppendLine($"Total Assemblies: {assemblies.Count}");
            report.AppendLine($"Total References: {assemblies.Sum(a => a.References.Length)}");
            report.AppendLine();
            
            // Analyze dependency hierarchy
            AnalyzeDependencyHierarchy(assemblies, report);
            
            // Check for issues
            CheckDependencyIssues(assemblies, report);
            
            // Generate detailed assembly list
            GenerateDetailedAssemblyList(assemblies, report);
            
            // Save report
            var reportPath = "Assets/ProjectChimera/Editor/AssemblyDependencyReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"[Assembly Report] Generated dependency report: {reportPath}");
            EditorUtility.RevealInFinder(reportPath);
        }
        
        [MenuItem("Project Chimera/Assembly Analysis/Validate Critical Dependencies")]
        public static void ValidateCriticalDependencies()
        {
            var criticalAssemblies = new[]
            {
                "ProjectChimera.Core",
                "ProjectChimera.Data", 
                "ProjectChimera.Systems",
                "ProjectChimera.Systems.IPM",
                "ProjectChimera.Gaming",
                "ProjectChimera.Testing"
            };
            
            var missingAssemblies = new List<string>();
            var invalidReferences = new List<string>();
            
            foreach (var assemblyName in criticalAssemblies)
            {
                var path = FindAssemblyPath(assemblyName);
                if (string.IsNullOrEmpty(path))
                {
                    missingAssemblies.Add(assemblyName);
                    continue;
                }
                
                // Validate references
                try
                {
                    var content = File.ReadAllText(path);
                    var references = ExtractJsonStringArray(content, "references");
                    
                    foreach (var reference in references)
                    {
                        if (reference.StartsWith("ProjectChimera") && string.IsNullOrEmpty(FindAssemblyPath(reference)))
                        {
                            invalidReferences.Add($"{assemblyName} -> {reference}");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to validate {assemblyName}: {e.Message}");
                }
            }
            
            // Report results
            if (missingAssemblies.Count == 0 && invalidReferences.Count == 0)
            {
                Debug.Log("[Assembly Validation] ✅ All critical assemblies are valid");
                EditorUtility.DisplayDialog("Assembly Validation", 
                    "All critical assemblies and their dependencies are valid!", "OK");
            }
            else
            {
                var message = new StringBuilder();
                message.AppendLine("Assembly validation issues found:");
                
                if (missingAssemblies.Count > 0)
                {
                    message.AppendLine("\nMissing Assemblies:");
                    foreach (var missing in missingAssemblies)
                    {
                        message.AppendLine($"  • {missing}");
                    }
                }
                
                if (invalidReferences.Count > 0)
                {
                    message.AppendLine("\nInvalid References:");
                    foreach (var invalid in invalidReferences)
                    {
                        message.AppendLine($"  • {invalid}");
                    }
                }
                
                Debug.LogError($"[Assembly Validation] ❌ {message.ToString()}");
                EditorUtility.DisplayDialog("Assembly Validation", message.ToString(), "OK");
            }
        }
        
        [MenuItem("Project Chimera/Assembly Analysis/Check Circular Dependencies")]
        public static void CheckCircularDependencies()
        {
            var asmdefPaths = Directory.GetFiles("Assets/ProjectChimera", "*.asmdef", SearchOption.AllDirectories);
            var dependencyGraph = new Dictionary<string, List<string>>();
            
            // Build dependency graph
            foreach (var path in asmdefPaths)
            {
                try
                {
                    var content = File.ReadAllText(path);
                    var name = ExtractJsonString(content, "name") ?? Path.GetFileNameWithoutExtension(path);
                    var references = ExtractJsonStringArray(content, "references");
                    
                    dependencyGraph[name] = references.Where(r => r.StartsWith("ProjectChimera")).ToList();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse {path}: {e.Message}");
                }
            }
            
            // Detect cycles
            var cycles = DetectCycles(dependencyGraph);
            
            if (cycles.Count == 0)
            {
                Debug.Log("[Circular Dependencies] ✅ No circular dependencies detected");
                EditorUtility.DisplayDialog("Circular Dependencies", 
                    "No circular dependencies detected in Project Chimera assemblies!", "OK");
            }
            else
            {
                var message = new StringBuilder();
                message.AppendLine("Circular dependencies detected:");
                
                foreach (var cycle in cycles)
                {
                    message.AppendLine($"  • {string.Join(" -> ", cycle)} -> {cycle[0]}");
                }
                
                Debug.LogError($"[Circular Dependencies] ❌ {message.ToString()}");
                EditorUtility.DisplayDialog("Circular Dependencies", message.ToString(), "OK");
            }
        }
        
        private static void AnalyzeDependencyHierarchy(List<AssemblyInfo> assemblies, StringBuilder report)
        {
            report.AppendLine("DEPENDENCY HIERARCHY");
            report.AppendLine("-".PadRight(40, '-'));
            
            // Find root assemblies (no dependencies on other ProjectChimera assemblies)
            var rootAssemblies = assemblies.Where(a => 
                !a.References.Any(r => r.StartsWith("ProjectChimera"))).ToList();
            
            report.AppendLine($"Root Assemblies ({rootAssemblies.Count}):");
            foreach (var root in rootAssemblies)
            {
                report.AppendLine($"  • {root.Name}");
            }
            report.AppendLine();
            
            // Find leaf assemblies (not referenced by any other ProjectChimera assembly)
            var leafAssemblies = assemblies.Where(a => 
                !assemblies.Any(other => other.References.Contains(a.Name))).ToList();
            
            report.AppendLine($"Leaf Assemblies ({leafAssemblies.Count}):");
            foreach (var leaf in leafAssemblies)
            {
                report.AppendLine($"  • {leaf.Name}");
            }
            report.AppendLine();
            
            // Find most referenced assemblies
            var referenceCounts = assemblies.ToDictionary(a => a.Name, 
                a => assemblies.Count(other => other.References.Contains(a.Name)));
            
            var mostReferenced = referenceCounts.OrderByDescending(kvp => kvp.Value).Take(5);
            
            report.AppendLine("Most Referenced Assemblies:");
            foreach (var referenced in mostReferenced)
            {
                report.AppendLine($"  • {referenced.Key} ({referenced.Value} references)");
            }
            report.AppendLine();
        }
        
        private static void CheckDependencyIssues(List<AssemblyInfo> assemblies, StringBuilder report)
        {
            report.AppendLine("DEPENDENCY ISSUES");
            report.AppendLine("-".PadRight(40, '-'));
            
            var issues = new List<string>();
            
            foreach (var assembly in assemblies)
            {
                // Check for missing references
                foreach (var reference in assembly.References)
                {
                    if (reference.StartsWith("ProjectChimera") && 
                        !assemblies.Any(a => a.Name == reference))
                    {
                        issues.Add($"Missing reference: {assembly.Name} -> {reference}");
                    }
                }
                
                // Check for potential architectural issues
                if (assembly.Name == "ProjectChimera.Core" && 
                    assembly.References.Any(r => r.StartsWith("ProjectChimera.Systems")))
                {
                    issues.Add($"Architectural issue: Core should not reference Systems");
                }
                
                if (assembly.Name.Contains("Editor") && !assembly.Path.Contains("/Editor/"))
                {
                    issues.Add($"Location issue: {assembly.Name} should be in Editor folder");
                }
            }
            
            if (issues.Count == 0)
            {
                report.AppendLine("✅ No dependency issues detected");
            }
            else
            {
                report.AppendLine($"❌ {issues.Count} issues detected:");
                foreach (var issue in issues)
                {
                    report.AppendLine($"  • {issue}");
                }
            }
            report.AppendLine();
        }
        
        private static void GenerateDetailedAssemblyList(List<AssemblyInfo> assemblies, StringBuilder report)
        {
            report.AppendLine("DETAILED ASSEMBLY INFORMATION");
            report.AppendLine("-".PadRight(80, '-'));
            
            foreach (var assembly in assemblies.OrderBy(a => a.Name))
            {
                report.AppendLine($"Assembly: {assembly.Name}");
                report.AppendLine($"Path: {assembly.Path}");
                report.AppendLine($"Root Namespace: {assembly.RootNamespace}");
                report.AppendLine($"References ({assembly.References.Length}):");
                
                if (assembly.References.Length == 0)
                {
                    report.AppendLine("  (none)");
                }
                else
                {
                    foreach (var reference in assembly.References)
                    {
                        var status = reference.StartsWith("ProjectChimera") ? 
                            (assemblies.Any(a => a.Name == reference) ? "✅" : "❌") : "🔗";
                        report.AppendLine($"  {status} {reference}");
                    }
                }
                
                report.AppendLine();
            }
        }
        
        private static string FindAssemblyPath(string assemblyName)
        {
            var asmdefPaths = Directory.GetFiles("Assets/ProjectChimera", "*.asmdef", SearchOption.AllDirectories);
            
            foreach (var path in asmdefPaths)
            {
                try
                {
                    var content = File.ReadAllText(path);
                    var name = ExtractJsonString(content, "name") ?? Path.GetFileNameWithoutExtension(path);
                    
                    if (name == assemblyName)
                    {
                        return path;
                    }
                }
                catch
                {
                    // Continue searching
                }
            }
            
            return null;
        }
        
        private static List<List<string>> DetectCycles(Dictionary<string, List<string>> graph)
        {
            var cycles = new List<List<string>>();
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();
            
            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    DetectCyclesRecursive(node, graph, visited, stack, new List<string>(), cycles);
                }
            }
            
            return cycles;
        }
        
        private static void DetectCyclesRecursive(string node, Dictionary<string, List<string>> graph, 
            HashSet<string> visited, HashSet<string> stack, List<string> path, List<List<string>> cycles)
        {
            visited.Add(node);
            stack.Add(node);
            path.Add(node);
            
            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node])
                {
                    if (stack.Contains(neighbor))
                    {
                        // Found a cycle
                        var cycleStart = path.IndexOf(neighbor);
                        var cycle = path.Skip(cycleStart).ToList();
                        cycles.Add(cycle);
                    }
                    else if (!visited.Contains(neighbor))
                    {
                        DetectCyclesRecursive(neighbor, graph, visited, stack, new List<string>(path), cycles);
                    }
                }
            }
            
            stack.Remove(node);
        }
        
        private static string ExtractJsonString(string json, string key)
        {
            var pattern = $@"""{key}"":\s*""([^""]+)""";
            var match = Regex.Match(json, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }
        
        private static string[] ExtractJsonStringArray(string json, string key)
        {
            var pattern = $@"""{key}"":\s*\[(.*?)\]";
            var match = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (!match.Success) return new string[0];
            
            var arrayContent = match.Groups[1].Value;
            var itemMatches = Regex.Matches(arrayContent, @"""([^""]+)""");
            var items = new List<string>();
            foreach (Match itemMatch in itemMatches)
            {
                items.Add(itemMatch.Groups[1].Value);
            }
            return items.ToArray();
        }
        
        private class AssemblyInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string RootNamespace { get; set; }
            public string[] References { get; set; } = new string[0];
        }
    }
}