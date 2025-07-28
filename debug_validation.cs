using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class DebugValidation : MonoBehaviour
{
    [ContextMenu("Debug ScriptableObject Issues")]
    public void DebugScriptableObjectIssues()
    {
        Debug.Log("=== DEBUGGING SCRIPTABLEOBJECT NAMING ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        Debug.Log($"Found {chimeraAssemblies.Length} ProjectChimera assemblies");
        
        foreach (var assembly in chimeraAssemblies)
        {
            Debug.Log($"Checking assembly: {assembly.FullName}");
            
            try
            {
                var scriptableObjectTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                    .ToArray();
                
                Debug.Log($"  Found {scriptableObjectTypes.Length} ScriptableObject types");
                
                foreach (var type in scriptableObjectTypes)
                {
                    if (!type.Name.EndsWith("SO"))
                    {
                        Debug.LogError($"  ❌ VIOLATION: {type.FullName} should end with 'SO'");
                    }
                    else
                    {
                        Debug.Log($"  ✅ {type.Name}");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogError($"  ❌ ReflectionTypeLoadException in {assembly.FullName}: {ex.Message}");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Debug.LogError($"    - {loaderException?.Message}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"  ❌ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
    }
    
    [ContextMenu("Debug Manager Issues")]
    public void DebugManagerIssues()
    {
        Debug.Log("=== DEBUGGING MANAGER INHERITANCE ===");
        
        var chimeraManagerType = System.Type.GetType("ProjectChimera.Core.ChimeraManager, ProjectChimera.Core");
        if (chimeraManagerType == null)
        {
            Debug.LogError("❌ ChimeraManager type not found!");
            return;
        }
        
        Debug.Log($"✅ Found ChimeraManager: {chimeraManagerType.FullName}");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var systemsAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray(); // Check all assemblies, not just Systems
        
        Debug.Log($"Checking {systemsAssemblies.Length} assemblies for Manager types");
        
        foreach (var assembly in systemsAssemblies)
        {
            Debug.Log($"Checking assembly: {assembly.FullName}");
            
            try
            {
                var managerTypes = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour)))
                    .ToArray();
                
                Debug.Log($"  Found {managerTypes.Length} Manager types inheriting from MonoBehaviour");
                
                foreach (var type in managerTypes)
                {
                    if (!type.IsSubclassOf(chimeraManagerType))
                    {
                        Debug.LogError($"  ❌ VIOLATION: {type.FullName} should inherit from ChimeraManager");
                    }
                    else
                    {
                        Debug.Log($"  ✅ {type.Name} inherits from ChimeraManager");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogError($"  ❌ ReflectionTypeLoadException in {assembly.FullName}: {ex.Message}");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Debug.LogError($"    - {loaderException?.Message}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"  ❌ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
    }
    
    [ContextMenu("Debug Namespace Issues")]
    public void DebugNamespaceIssues()
    {
        Debug.Log("=== DEBUGGING NAMESPACE CONSISTENCY ===");
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
        
        foreach (var assembly in chimeraAssemblies)
        {
            var assemblyName = assembly.GetName().Name;
            Debug.Log($"Checking assembly: {assemblyName}");
            
            try
            {
                var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToArray();
                
                foreach (var type in types)
                {
                    var expectedNamespaceStart = assemblyName;
                    if (!type.Namespace.StartsWith(expectedNamespaceStart))
                    {
                        // Allow some flexibility for Core and Data assemblies
                        if (!assemblyName.Contains("Core") && !assemblyName.Contains("Data") && !assemblyName.Contains("Testing"))
                        {
                            Debug.LogError($"  ❌ VIOLATION: {type.FullName} in assembly {assemblyName} (expected namespace to start with {expectedNamespaceStart})");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"  ❌ Exception in {assembly.FullName}: {ex.Message}");
            }
        }
    }
}