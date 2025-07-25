using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectChimera.Editor
{
    /// <summary>
    /// Final assembly validation summary for Project Chimera after major system changes.
    /// Provides a comprehensive overview of assembly health and reference integrity.
    /// </summary>
    public static class AssemblyValidationSummary
    {
        [MenuItem("Project Chimera/Assembly Validation/Complete System Validation")]
        public static void CompleteSystemValidation()
        {
            var summary = new StringBuilder();
            summary.AppendLine("PROJECT CHIMERA COMPLETE SYSTEM VALIDATION");
            summary.AppendLine($"Validation Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            summary.AppendLine($"Unity Version: {Application.unityVersion}");
            summary.AppendLine("=".PadRight(80, '='));
            summary.AppendLine();
            
            bool allValidationsPassed = true;
            
            // 1. Assembly Definition Validation
            allValidationsPassed &= ValidateAssemblyDefinitions(summary);
            
            // 2. Compilation Status Check
            allValidationsPassed &= ValidateCompilationStatus(summary);
            
            // 3. Critical Dependencies Check
            allValidationsPassed &= ValidateCriticalDependencies(summary);
            
            // 4. Namespace Usage Validation
            allValidationsPassed &= ValidateNamespaceUsage(summary);
            
            // 5. IPM Gaming System Integration Check
            allValidationsPassed &= ValidateIPMGamingSystemIntegration(summary);
            
            // 6. File Structure Validation
            allValidationsPassed &= ValidateFileStructure(summary);
            
            // Generate final status
            GenerateFinalStatus(summary, allValidationsPassed);
            
            // Save validation report
            var reportPath = "Assets/ProjectChimera/Editor/CompleteSystemValidationReport.txt";
            File.WriteAllText(reportPath, summary.ToString());
            AssetDatabase.Refresh();
            
            // Display results
            var statusMessage = allValidationsPassed ? 
                "✅ Complete system validation PASSED" : 
                "❌ Complete system validation FAILED";
            
            Debug.Log($"[System Validation] {statusMessage}");
            
            EditorUtility.DisplayDialog("Complete System Validation", 
                $"{statusMessage}\n\nDetailed report saved to:\n{reportPath}", "OK");
        }
        
        private static bool ValidateAssemblyDefinitions(StringBuilder summary)
        {
            summary.AppendLine("1. ASSEMBLY DEFINITION VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var asmdefPaths = Directory.GetFiles("Assets/ProjectChimera", "*.asmdef", SearchOption.AllDirectories);
            var validAssemblies = 0;
            var totalAssemblies = asmdefPaths.Length;
            
            foreach (var path in asmdefPaths)
            {
                try
                {
                    var content = File.ReadAllText(path);
                    if (content.Contains("\"name\"") && content.Contains("\"rootNamespace\""))
                    {
                        validAssemblies++;
                    }
                }
                catch
                {
                    summary.AppendLine($"  ❌ Invalid assembly definition: {path}");
                }
            }
            
            var success = validAssemblies == totalAssemblies;
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Valid Assemblies: {validAssemblies}/{totalAssemblies}");
            summary.AppendLine();
            
            return success;
        }
        
        private static bool ValidateCompilationStatus(StringBuilder summary)
        {
            summary.AppendLine("2. COMPILATION STATUS VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var hasCompilationErrors = EditorUtility.scriptCompilationFailed;
            var success = !hasCompilationErrors;
            
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Compilation Errors: {(hasCompilationErrors ? "Present" : "None")}");
            summary.AppendLine();
            
            return success;
        }
        
        private static bool ValidateCriticalDependencies(StringBuilder summary)
        {
            summary.AppendLine("3. CRITICAL DEPENDENCIES VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var criticalAssemblies = new[]
            {
                "ProjectChimera.Core",
                "ProjectChimera.Data",
                "ProjectChimera.Systems.IPM",
                "ProjectChimera.Gaming"
            };
            
            var foundAssemblies = 0;
            var missingAssemblies = new StringBuilder();
            
            foreach (var assembly in criticalAssemblies)
            {
                var path = FindAssemblyDefinition(assembly);
                if (!string.IsNullOrEmpty(path))
                {
                    foundAssemblies++;
                    summary.AppendLine($"  ✅ {assembly}");
                }
                else
                {
                    summary.AppendLine($"  ❌ {assembly} (MISSING)");
                    missingAssemblies.AppendLine($"    - {assembly}");
                }
            }
            
            var success = foundAssemblies == criticalAssemblies.Length;
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Found: {foundAssemblies}/{criticalAssemblies.Length}");
            summary.AppendLine();
            
            return success;
        }
        
        private static bool ValidateNamespaceUsage(StringBuilder summary)
        {
            summary.AppendLine("4. NAMESPACE USAGE VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var csFiles = Directory.GetFiles("Assets/ProjectChimera", "*.cs", SearchOption.AllDirectories);
            var validNamespaces = 0;
            var totalFiles = 0;
            
            foreach (var file in csFiles.Take(100)) // Sample validation
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("namespace ProjectChimera") || 
                        content.Contains("using ProjectChimera"))
                    {
                        validNamespaces++;
                    }
                    totalFiles++;
                }
                catch
                {
                    // Skip problematic files
                }
            }
            
            var success = (float)validNamespaces / totalFiles > 0.8f; // 80% threshold
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Valid Namespace Usage: {validNamespaces}/{totalFiles} ({(float)validNamespaces/totalFiles*100:F1}%)");
            summary.AppendLine();
            
            return success;
        }
        
        private static bool ValidateIPMGamingSystemIntegration(StringBuilder summary)
        {
            summary.AppendLine("5. IPM GAMING SYSTEM INTEGRATION VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var requiredFiles = new[]
            {
                "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs",
                "Assets/ProjectChimera/Systems/IPM/IPMGamingArchitectureTypes.cs",
                "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs"
            };
            
            var foundFiles = 0;
            foreach (var file in requiredFiles)
            {
                if (File.Exists(file))
                {
                    foundFiles++;
                    summary.AppendLine($"  ✅ {Path.GetFileName(file)}");
                }
                else
                {
                    summary.AppendLine($"  ❌ {Path.GetFileName(file)} (MISSING)");
                }
            }
            
            var success = foundFiles == requiredFiles.Length;
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Required Files: {foundFiles}/{requiredFiles.Length}");
            summary.AppendLine();
            
            return success;
        }
        
        private static bool ValidateFileStructure(StringBuilder summary)
        {
            summary.AppendLine("6. FILE STRUCTURE VALIDATION");
            summary.AppendLine("-".PadRight(50, '-'));
            
            var requiredDirectories = new[]
            {
                "Assets/ProjectChimera/Core",
                "Assets/ProjectChimera/Data",
                "Assets/ProjectChimera/Systems",
                "Assets/ProjectChimera/Systems/IPM",
                "Assets/ProjectChimera/Systems/Gaming",
                "Assets/ProjectChimera/Testing",
                "Assets/ProjectChimera/Editor"
            };
            
            var foundDirectories = 0;
            foreach (var dir in requiredDirectories)
            {
                if (Directory.Exists(dir))
                {
                    foundDirectories++;
                    summary.AppendLine($"  ✅ {Path.GetFileName(dir)}");
                }
                else
                {
                    summary.AppendLine($"  ❌ {Path.GetFileName(dir)} (MISSING)");
                }
            }
            
            var success = foundDirectories == requiredDirectories.Length;
            summary.AppendLine($"  Status: {(success ? "✅ PASSED" : "❌ FAILED")}");
            summary.AppendLine($"  Required Directories: {foundDirectories}/{requiredDirectories.Length}");
            summary.AppendLine();
            
            return success;
        }
        
        private static void GenerateFinalStatus(StringBuilder summary, bool allValidationsPassed)
        {
            summary.AppendLine("FINAL VALIDATION STATUS");
            summary.AppendLine("=".PadRight(80, '='));
            
            if (allValidationsPassed)
            {
                summary.AppendLine("✅ ALL VALIDATIONS PASSED");
                summary.AppendLine();
                summary.AppendLine("Project Chimera assembly references are fully validated and ready for:");
                summary.AppendLine("  • Continued development");
                summary.AppendLine("  • Production builds");
                summary.AppendLine("  • Advanced system integration");
                summary.AppendLine("  • Performance optimization");
                summary.AppendLine();
                summary.AppendLine("The Enhanced IPM Gaming System with clean architecture patterns");
                summary.AppendLine("has been successfully integrated and all dependencies are resolved.");
            }
            else
            {
                summary.AppendLine("❌ VALIDATION FAILED");
                summary.AppendLine();
                summary.AppendLine("One or more validation checks failed. Please review the issues above");
                summary.AppendLine("and use the provided assembly validation tools to resolve them:");
                summary.AppendLine("  • Assembly Reference Validator");
                summary.AppendLine("  • Assembly Dependency Report");
                summary.AppendLine("  • Unity Editor Diagnostics");
                summary.AppendLine();
                summary.AppendLine("All tools are available in the Project Chimera menu.");
            }
            
            summary.AppendLine();
            summary.AppendLine($"Validation completed at: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        
        private static string FindAssemblyDefinition(string assemblyName)
        {
            var asmdefPaths = Directory.GetFiles("Assets/ProjectChimera", "*.asmdef", SearchOption.AllDirectories);
            
            foreach (var path in asmdefPaths)
            {
                try
                {
                    var content = File.ReadAllText(path);
                    if (content.Contains($"\"name\": \"{assemblyName}\""))
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
    }
}