using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Narrative;

namespace ProjectChimera
{
    /// <summary>
    /// Clean compilation test without ambiguous type references
    /// </summary>
    public class CleanCompilationTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Clean compilation test - Project Chimera compiles successfully");
            
            // Basic Unity and Core types work
            var gameObject = new GameObject("TestObject");
            var manager = GetComponent<ChimeraManager>();
            
            // Test core systems compile
            var cultivationManager = GetComponent<EnhancedCultivationGamingManager>();
            var storyManager = GetComponent<StoryCampaignManager>();
            var automationSystem = GetComponent<EarnedAutomationProgressionSystem>();
            var agencySystem = GetComponent<PlayerAgencyGamingSystem>();
            var careSystem = GetComponent<InteractivePlantCareSystem>();
            
            Debug.Log("Project Chimera compilation successful - all major systems compile without errors!");
        }
    }
}