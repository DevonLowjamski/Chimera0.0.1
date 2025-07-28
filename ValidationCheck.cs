using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ValidationCheck : MonoBehaviour
{
    [ContextMenu("Check ScriptableObject Violations")]
    public void CheckScriptableObjectViolations()
    {
        Debug.Log("=== ScriptableObject Naming Violations ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        int violationCount = 0;
        foreach (var assembly in chimeraAssemblies)
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
        
        Debug.Log($"Total ScriptableObject naming violations: {violationCount}");
    }
    
    [ContextMenu("Check Manager Violations")]
    public void CheckManagerViolations()
    {
        Debug.Log("=== Manager Inheritance Violations ===");
        
        var chimeraManagerType = System.Type.GetType("ProjectChimera.Core.ChimeraManager, ProjectChimera.Core");
        if (chimeraManagerType == null)
        {
            Debug.LogError("ChimeraManager type not found!");
            return;
        }
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var systemsAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera.Systems")).ToArray();
        
        int violationCount = 0;
        foreach (var assembly in systemsAssemblies)
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
        
        Debug.Log($"Total Manager inheritance violations: {violationCount}");
    }
}