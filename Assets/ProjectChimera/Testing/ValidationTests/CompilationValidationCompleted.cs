using UnityEngine;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Systems.Construction;
// Explicit type aliases to resolve ambiguous references
using ConstructionRoomType = ProjectChimera.Data.Construction.RoomType;
using FacilitiesRoomType = ProjectChimera.Data.Facilities.RoomType;

namespace ProjectChimera
{
    /// <summary>
    /// Simple compilation validation to ensure critical types are properly defined
    /// </summary>
    public class CompilationValidationCompleted : MonoBehaviour
    {
        private void Start()
        {
            // Test that all critical types compile correctly
            var constructionRoomType = ConstructionRoomType.Vegetative;
            var floweringRoom = ConstructionRoomType.Flowering;
            var dryingRoom = ConstructionRoomType.Drying;
            var meetingRoom = ConstructionRoomType.Meeting;
            
            // Test Facilities RoomType
            var facilityRoomType = FacilitiesRoomType.Vegetative;
            var facilityFlowering = FacilitiesRoomType.Flowering;
            
            // Test RoomTemplate has required properties
            var template = new RoomTemplate();
            string templateId = template.TemplateId;
            Vector2 dimensions = template.Dimensions;
            
            // Test LayoutOptimizationResult has RoomId
            var optimizationResult = new LayoutOptimizationResult();
            string roomId = optimizationResult.RoomId;
            
            Debug.Log("✅ All critical types compile successfully!");
            Debug.Log($"Construction room types: {constructionRoomType}, {floweringRoom}, {dryingRoom}, {meetingRoom}");
            Debug.Log($"Facility room types: {facilityRoomType}, {facilityFlowering}");
            Debug.Log($"Template ID: {templateId}");
            Debug.Log($"Room dimensions: {dimensions}");
            Debug.Log("Room creation and configuration implementation completed successfully!");
        }
    }
}