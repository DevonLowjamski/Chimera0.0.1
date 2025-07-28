using UnityEngine;
using System.Linq;

namespace ProjectChimera.Testing.ValidationTests
{
    /// <summary>
    /// Debug script to identify specific architectural violations
    /// </summary>
    public class ArchitecturalViolationDebugger : MonoBehaviour
    {
        [ContextMenu("Debug ScriptableObject Violations")]
        public void DebugScriptableObjectViolations()
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
        
        [ContextMenu("Debug Manager Inheritance Violations")]
        public void DebugManagerInheritanceViolations()
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
        
        [ContextMenu("Debug Namespace Violations")]
        public void DebugNamespaceViolations()
        {
            Debug.Log("=== Namespace Consistency Violations ===");
            
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
            
            int violationCount = 0;
            foreach (var assembly in chimeraAssemblies)
            {
                var assemblyName = assembly.GetName().Name;
                var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToArray();
                
                foreach (var type in types)
                {
                    var expectedNamespaceStart = assemblyName;
                    if (!type.Namespace.StartsWith(expectedNamespaceStart))
                    {
                        // Allow some flexibility for Core and Data assemblies
                        if (!assemblyName.Contains("Core") && !assemblyName.Contains("Data") && !assemblyName.Contains("Testing"))
                        {
                            Debug.LogError($"VIOLATION: {type.FullName} in assembly {assemblyName} (expected namespace to start with {expectedNamespaceStart})");
                            violationCount++;
                        }
                    }
                }
            }
            
            Debug.Log($"Total Namespace consistency violations: {violationCount}");
        }
    }
}