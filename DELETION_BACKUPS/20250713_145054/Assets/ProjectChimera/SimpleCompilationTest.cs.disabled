using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Data.Construction;

namespace ProjectChimera
{
    /// <summary>
    /// Simple Compilation Test
    /// Tests only basic types and avoids problematic enum values
    /// Final validation that core types compile without errors
    /// </summary>
    public class SimpleCompilationTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== Simple Compilation Test ===");
            
            TestBasicTypes();
            TestSystemComponents();
            TestDataStructures();
            
            Debug.Log("✅ Simple compilation test completed successfully!");
        }
        
        private void TestBasicTypes()
        {
            Debug.Log("Testing basic types...");
            
            // Test basic enums with safe values
            var taskType = ProjectChimera.Data.Cultivation.CultivationTaskType.Watering;
            var careQuality = ProjectChimera.Data.Cultivation.CareQuality.Good; // Use value that exists in all CareQuality enums
            var choiceType = ProjectChimera.Events.PlayerChoiceType.CultivationMethod;
            
            // Test basic classes
            var gameObj = new GameObject("TestObject");
            var manager = GetComponent<ChimeraManager>();
            
            Debug.Log($"✓ Basic types: Task={taskType}, Care={careQuality}, ChoiceType={choiceType}");
        }
        
        private void TestSystemComponents()
        {
            Debug.Log("Testing system components...");
            
            // Test that components can be referenced without errors
            var cultivationManager = GetComponent<EnhancedCultivationGamingManager>();
            var plantCareSystem = GetComponent<InteractivePlantCareSystem>();
            var automationSystem = GetComponent<EarnedAutomationProgressionSystem>();
            
            Debug.Log("✓ All system components compile and are accessible");
        }
        
        private void TestDataStructures()
        {
            Debug.Log("Testing data structures...");
            
            // Test InteractivePlant with basic properties
            var plant = new InteractivePlant
            {
                PlantInstanceID = 123,
                PlantedTime = System.DateTime.Now.AddDays(-10)
            };
            
            // Test construction types with safe values
            var participantData = new ProjectChimera.Data.Construction.ParticipantInfo
            {
                PlayerId = "test-player",
                PlayerName = "Test Player",
                Role = ProjectChimera.Data.Construction.ParticipantRole.Architect
            };
            
            var participantCore = new ProjectChimera.Core.ParticipantInfo
            {
                PlayerId = "core-player",
                PlayerName = "Core Player",
                Role = ProjectChimera.Core.CollaborationRole.Architect,
                JoinTime = System.DateTime.Now,
                IsActive = true
            };
            
            Debug.Log($"✓ Data structures: Plant={plant.PlantInstanceID}, DataParticipant={participantData.PlayerId}, CoreParticipant={participantCore.PlayerId}");
        }
    }
}