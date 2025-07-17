using UnityEngine;
using System.Reflection;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Testing;
using ProjectChimera.Systems.Progression;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Diagnostic tool to check assembly references and identify compilation issues
    /// </summary>
    public class AssemblyDiagnostic : MonoBehaviour
    {
        [ContextMenu("Check Assembly References")]
        public void CheckAssemblyReferences()
        {
            Debug.Log("=== Assembly Diagnostic Report ===");
            
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            
            // Check for ProjectChimera assemblies
            var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
            
            Debug.Log($"Found {chimeraAssemblies.Length} ProjectChimera assemblies:");
            
            foreach (var assembly in chimeraAssemblies)
            {
                Debug.Log($"- {assembly.FullName}");
                
                // Check for specific types
                if (assembly.FullName.Contains("Progression"))
                {
                    CheckProgressionTypes(assembly);
                }
                
                if (assembly.FullName.Contains("Cultivation"))
                {
                    CheckCultivationTypes(assembly);
                }
            }
            
            Debug.Log("=== End Assembly Diagnostic ===");
        }
        
        private void CheckProgressionTypes(Assembly assembly)
        {
            Debug.Log("Checking Progression assembly types:");
            
            try
            {
                var progressionManagerType = assembly.GetType("ProjectChimera.Systems.Progression.ProgressionManager");
                Debug.Log($"ProgressionManager found: {progressionManagerType != null}");
                
                var objectiveManagerType = assembly.GetType("ProjectChimera.Systems.Progression.ObjectiveManager");
                Debug.Log($"ObjectiveManager found: {objectiveManagerType != null}");
                
                var competitiveManagerType = assembly.GetType("ProjectChimera.Systems.Progression.CompetitiveManager");
                Debug.Log($"CompetitiveManager found: {competitiveManagerType != null}");
                
                var competitiveAchievementType = assembly.GetType("ProjectChimera.Systems.Progression.CompetitiveAchievementType");
                Debug.Log($"CompetitiveAchievementType found: {competitiveAchievementType != null}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking Progression types: {e.Message}");
            }
        }
        
        private void CheckCultivationTypes(Assembly assembly)
        {
            Debug.Log("Checking Cultivation assembly types:");
            
            try
            {
                var plantManagerType = assembly.GetType("ProjectChimera.Systems.Cultivation.PlantManager");
                Debug.Log($"PlantManager found: {plantManagerType != null}");
                
                // Check references to Progression types
                if (plantManagerType != null)
                {
                    var fields = plantManagerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    var progressionManagerField = fields.FirstOrDefault(f => f.Name.Contains("progressionManager"));
                    var objectiveManagerField = fields.FirstOrDefault(f => f.Name.Contains("objectiveManager"));
                    var competitiveManagerField = fields.FirstOrDefault(f => f.Name.Contains("competitiveManager"));
                    
                    Debug.Log($"PlantManager._progressionManager field found: {progressionManagerField != null}");
                    Debug.Log($"PlantManager._objectiveManager field found: {objectiveManagerField != null}");
                    Debug.Log($"PlantManager._competitiveManager field found: {competitiveManagerField != null}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking Cultivation types: {e.Message}");
            }
        }
    }
} 