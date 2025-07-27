using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Example configurations for Event Channel ScriptableObjects.
    /// This file demonstrates proper configuration patterns to prevent Unity Editor errors.
    /// </summary>
    public static class EventChannelExamples
    {
        /// <summary>
        /// Creates a properly configured Plant Harvested Event Channel
        /// </summary>
        public static StringGameEventSO CreatePlantHarvestedEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<StringGameEventSO>();
            
            eventChannel.name = "PlantHarvestedEvent";
            
            #if UNITY_EDITOR
            // Set display name and description using reflection
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Plant Harvested");
            descriptionField?.SetValue(eventChannel, "Raised when a plant is harvested, contains plant ID");
            #endif
            
            return eventChannel;
        }
        
        /// <summary>
        /// Creates a properly configured Environmental Alert Event Channel
        /// </summary>
        public static StringGameEventSO CreateEnvironmentalAlertEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<StringGameEventSO>();
            
            eventChannel.name = "EnvironmentalAlertEvent";
            
            #if UNITY_EDITOR
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Environmental Alert");
            descriptionField?.SetValue(eventChannel, "Raised when environmental conditions require attention");
            #endif
            
            return eventChannel;
        }
        
        /// <summary>
        /// Creates a properly configured Currency Changed Event Channel
        /// </summary>
        public static FloatGameEventSO CreateCurrencyChangedEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<FloatGameEventSO>();
            
            eventChannel.name = "CurrencyChangedEvent";
            
            #if UNITY_EDITOR
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Currency Changed");
            descriptionField?.SetValue(eventChannel, "Raised when player currency changes, contains new amount");
            #endif
            
            return eventChannel;
        }
        
        /// <summary>
        /// Creates a properly configured Equipment Malfunction Event Channel
        /// </summary>
        public static StringGameEventSO CreateEquipmentMalfunctionEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<StringGameEventSO>();
            
            eventChannel.name = "EquipmentMalfunctionEvent";
            
            #if UNITY_EDITOR
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Equipment Malfunction");
            descriptionField?.SetValue(eventChannel, "Raised when equipment malfunctions, contains equipment ID");
            #endif
            
            return eventChannel;
        }
        
        /// <summary>
        /// Creates a properly configured Game State Changed Event Channel
        /// </summary>
        public static SimpleGameEventSO CreateGameStateChangedEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<SimpleGameEventSO>();
            
            eventChannel.name = "GameStateChangedEvent";
            
            #if UNITY_EDITOR
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Game State Changed");
            descriptionField?.SetValue(eventChannel, "Raised when game state changes (pause, resume, etc.)");
            #endif
            
            return eventChannel;
        }
        
        /// <summary>
        /// Creates a properly configured Research Completed Event Channel
        /// </summary>
        public static IntGameEventSO CreateResearchCompletedEvent()
        {
            var eventChannel = ScriptableObject.CreateInstance<IntGameEventSO>();
            
            eventChannel.name = "ResearchCompletedEvent";
            
            #if UNITY_EDITOR
            var displayNameField = typeof(ChimeraScriptableObjectSO).GetField("_displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ChimeraScriptableObjectSO).GetField("_description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            displayNameField?.SetValue(eventChannel, "Research Completed");
            descriptionField?.SetValue(eventChannel, "Raised when research is completed, contains research ID");
            #endif
            
            return eventChannel;
        }
    }
}