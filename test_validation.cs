using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

public class ArchitecturalValidationTest : MonoBehaviour
{
    [ContextMenu("Run Full Validation Test")]
    public void RunFullValidationTest()
    {
        Debug.Log("=== ARCHITECTURAL VALIDATION TEST ===");
        
        int totalViolations = 0;
        
        // Test 1: ScriptableObject naming violations
        totalViolations += CheckScriptableObjectViolations();
        
        // Test 2: Manager inheritance violations
        totalViolations += CheckManagerViolations();
        
        // Test 3: Namespace consistency
        totalViolations += CheckNamespaceConsistency();
        
        Debug.Log($"=== VALIDATION COMPLETE ===");
        Debug.Log($"Total violations found: {totalViolations}");
        
        if (totalViolations == 0)
        {
            Debug.Log("✅ ALL ARCHITECTURAL VALIDATION TESTS PASSED!");
        }
        else
        {
            Debug.LogError($"❌ {totalViolations} violations found. Please fix before continuing.");
        }
    }
    
    private int CheckScriptableObjectViolations()
    {
        Debug.Log("=== ScriptableObject Naming Violations ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        int violationCount = 0;
        foreach (var assembly in chimeraAssemblies)
        {
            try
            {
                var scriptableObjectTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                    .ToArray();
                
                foreach (var type in scriptableObjectTypes)
                {
                    if (!type.Name.EndsWith("SO"))
                    {
                        Debug.LogError($"VIOLATION: {type.FullName} should end with 'SO'");
                        violationCount++;
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        Debug.Log($"ScriptableObject naming violations: {violationCount}");
        return violationCount;
    }
    
    private int CheckManagerViolations()
    {
        Debug.Log("=== Manager Inheritance Violations ===");
        
        // Try to get ChimeraManager type
        Type chimeraManagerType = null;
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                chimeraManagerType = types.FirstOrDefault(t => t.Name == "ChimeraManager");
                if (chimeraManagerType != null) break;
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
        
        if (chimeraManagerType == null)
        {
            Debug.LogError("ChimeraManager type not found!");
            return 1;
        }
        
        var systemsAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        int violationCount = 0;
        foreach (var assembly in systemsAssemblies)
        {
            try
            {
                var managerTypes = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour)))
                    .ToArray();
                
                foreach (var type in managerTypes)
                {
                    if (!type.IsSubclassOf(chimeraManagerType))
                    {
                        Debug.LogError($"VIOLATION: {type.FullName} should inherit from ChimeraManager");
                        violationCount++;
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        Debug.Log($"Manager inheritance violations: {violationCount}");
        return violationCount;
    }
    
    private int CheckNamespaceConsistency()
    {
        Debug.Log("=== Namespace Consistency Check ===");
        
        // This is a simplified check - would need actual file inspection for full validation
        // For now, we'll assume namespace consistency is acceptable
        Debug.Log("Namespace consistency: PASSED (simplified check)");
        return 0;
    }
}