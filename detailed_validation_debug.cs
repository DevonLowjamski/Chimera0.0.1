using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class DetailedValidationDebug : MonoBehaviour
{
    [ContextMenu("Run Detailed ScriptableObject Debug")]
    public void RunDetailedScriptableObjectDebug()
    {
        Debug.Log("=== DETAILED SCRIPTABLEOBJECT NAMING DEBUG ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        Debug.Log($"Checking {chimeraAssemblies.Length} ProjectChimera assemblies...");
        
        int totalViolations = 0;
        int totalSOs = 0;
        
        foreach (var assembly in chimeraAssemblies)
        {
            Debug.Log($"\n--- Checking Assembly: {assembly.FullName} ---");
            
            try
            {
                var scriptableObjectTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                    .ToArray();
                
                totalSOs += scriptableObjectTypes.Length;
                Debug.Log($"Found {scriptableObjectTypes.Length} ScriptableObject types in this assembly");
                
                foreach (var type in scriptableObjectTypes)
                {
                    if (!type.Name.EndsWith("SO"))
                    {
                        Debug.LogError($"❌ VIOLATION: {type.FullName} should end with 'SO'");
                        totalViolations++;
                    }
                    else
                    {
                        Debug.Log($"✅ Valid: {type.Name}");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"⚠️ ReflectionTypeLoadException in {assembly.FullName}: {ex.Message}");
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        if (loaderEx != null)
                            Debug.LogWarning($"  Loader Exception: {loaderEx.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
        
        Debug.Log($"\n=== SCRIPTABLEOBJECT SUMMARY ===");
        Debug.Log($"Total ScriptableObjects found: {totalSOs}");
        Debug.Log($"Total violations: {totalViolations}");
        Debug.Log($"Compliance rate: {((totalSOs - totalViolations) / (float)totalSOs * 100):F1}%");
    }
    
    [ContextMenu("Run Detailed Manager Debug")]
    public void RunDetailedManagerDebug()
    {
        Debug.Log("=== DETAILED MANAGER INHERITANCE DEBUG ===");
        
        var chimeraManagerType = System.Type.GetType("ProjectChimera.Core.ChimeraManager, ProjectChimera.Core");
        if (chimeraManagerType == null)
        {
            Debug.LogError("❌ ChimeraManager type not found!");
            return;
        }
        
        Debug.Log($"✅ Found ChimeraManager: {chimeraManagerType.FullName}");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        int totalManagers = 0;
        int totalViolations = 0;
        
        foreach (var assembly in chimeraAssemblies)
        {
            Debug.Log($"\n--- Checking Assembly: {assembly.FullName} ---");
            
            try
            {
                var managerTypes = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour)))
                    .ToArray();
                
                totalManagers += managerTypes.Length;
                Debug.Log($"Found {managerTypes.Length} Manager types in this assembly");
                
                foreach (var type in managerTypes)
                {
                    if (!type.IsSubclassOf(chimeraManagerType))
                    {
                        Debug.LogError($"❌ VIOLATION: {type.FullName} should inherit from ChimeraManager");
                        totalViolations++;
                    }
                    else
                    {
                        Debug.Log($"✅ Valid: {type.Name} inherits from ChimeraManager");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"⚠️ ReflectionTypeLoadException in {assembly.FullName}: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
        
        Debug.Log($"\n=== MANAGER INHERITANCE SUMMARY ===");
        Debug.Log($"Total Managers found: {totalManagers}");
        Debug.Log($"Total violations: {totalViolations}");
        Debug.Log($"Compliance rate: {((totalManagers - totalViolations) / (float)totalManagers * 100):F1}%");
    }
    
    [ContextMenu("Run Detailed Namespace Debug")]
    public void RunDetailedNamespaceDebug()
    {
        Debug.Log("=== DETAILED NAMESPACE CONSISTENCY DEBUG ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        int totalViolations = 0;
        int totalTypes = 0;
        
        foreach (var assembly in chimeraAssemblies)
        {
            var assemblyName = assembly.GetName().Name;
            Debug.Log($"\n--- Checking Assembly: {assemblyName} ---");
            
            try
            {
                var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToArray();
                totalTypes += types.Length;
                
                Debug.Log($"Found {types.Length} types with namespaces in this assembly");
                
                foreach (var type in types)
                {
                    var expectedNamespaceStart = assemblyName;
                    if (!type.Namespace.StartsWith(expectedNamespaceStart))
                    {
                        // Allow some flexibility for Core and Data assemblies, and specific cases
                        if (!assemblyName.Contains("Core") && !assemblyName.Contains("Data") && !assemblyName.Contains("Testing") && !assemblyName.Contains("Editor"))
                        {
                            Debug.LogError($"❌ NAMESPACE VIOLATION: {type.FullName} in assembly {assemblyName} (expected namespace to start with {expectedNamespaceStart})");
                            totalViolations++;
                        }
                    }
                }
                
                // Show expected vs allowed namespaces for this assembly
                var uniqueNamespaces = types.Select(t => t.Namespace).Distinct().OrderBy(n => n).ToArray();
                Debug.Log($"Namespaces found in {assemblyName}: {string.Join(", ", uniqueNamespaces)}");
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"⚠️ ReflectionTypeLoadException in {assembly.FullName}: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
        
        Debug.Log($"\n=== NAMESPACE CONSISTENCY SUMMARY ===");
        Debug.Log($"Total types checked: {totalTypes}");
        Debug.Log($"Total violations: {totalViolations}");
        Debug.Log($"Compliance rate: {((totalTypes - totalViolations) / (float)totalTypes * 100):F1}%");
    }
}