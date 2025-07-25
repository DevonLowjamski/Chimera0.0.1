using UnityEngine;
using ProjectChimera.Systems.Cultivation;

namespace ProjectChimera
{
    /// <summary>
    /// Compilation test specifically for PlayerAgencyGamingSystem to verify type resolution
    /// </summary>
    public class PlayerAgencyCompilationTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("PlayerAgency compilation test - checking PlayerAgencyGamingSystem compiles");
            
            // Test that PlayerAgencyGamingSystem can be accessed without compilation errors
            var agencySystem = GetComponent<PlayerAgencyGamingSystem>();
            if (agencySystem != null)
            {
                // Test that we can access the system's public API without errors
                var state = agencySystem.GetCurrentAgencyState();
                var approaches = agencySystem.GetAvailableCultivationApproaches();
                var designs = agencySystem.GetAvailableFacilityDesigns();
                var metrics = agencySystem.GetAgencyMetrics();
                
                Debug.Log($"PlayerAgencyGamingSystem compilation successful - {approaches.Count} cultivation approaches, {designs.Count} facility designs available");
            }
            else
            {
                Debug.Log("PlayerAgencyGamingSystem component not found, but compilation successful");
            }
        }
    }
}