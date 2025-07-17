using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectChimera.Core;
using ProjectChimera.Data.UI;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Editor
{
    /// <summary>
    /// Editor utility to create properly configured ScriptableObject assets
    /// and prevent Unity Editor configuration errors.
    /// </summary>
    public static class AssetConfigurationHelper
    {
        private const string UI_DATA_BINDING_PATH = "Assets/ProjectChimera/Data/UI/Bindings/";
        private const string EVENT_CHANNEL_PATH = "Assets/ProjectChimera/Data/Events/Channels/";
        
        [MenuItem("Project Chimera/Create Example UI Data Bindings")]
        public static void CreateExampleUIDataBindings()
        {
            // Ensure directory exists
            if (!Directory.Exists(UI_DATA_BINDING_PATH))
            {
                Directory.CreateDirectory(UI_DATA_BINDING_PATH);
            }
            
            // Create Plant Health Binding
            var plantHealthBinding = UIDataBindingExamples.CreatePlantHealthBinding();
            CreateAndSaveAsset(plantHealthBinding, UI_DATA_BINDING_PATH + "PlantHealthBinding.asset");
            
            // Create Temperature Binding
            var temperatureBinding = UIDataBindingExamples.CreateTemperatureBinding();
            CreateAndSaveAsset(temperatureBinding, UI_DATA_BINDING_PATH + "TemperatureBinding.asset");
            
            // Create Currency Binding
            var currencyBinding = UIDataBindingExamples.CreateCurrencyBinding();
            CreateAndSaveAsset(currencyBinding, UI_DATA_BINDING_PATH + "CurrencyBinding.asset");
            
            AssetDatabase.Refresh();
            Debug.Log("[Chimera] Created example UI Data Binding assets");
        }
        
        [MenuItem("Project Chimera/Create Example Event Channels")]
        public static void CreateExampleEventChannels()
        {
            // Ensure directory exists
            if (!Directory.Exists(EVENT_CHANNEL_PATH))
            {
                Directory.CreateDirectory(EVENT_CHANNEL_PATH);
            }
            
            // Create Plant Harvested Event
            var plantHarvestedEvent = EventChannelExamples.CreatePlantHarvestedEvent();
            CreateAndSaveAsset(plantHarvestedEvent, EVENT_CHANNEL_PATH + "PlantHarvestedEvent.asset");
            
            // Create Environmental Alert Event
            var environmentalAlertEvent = EventChannelExamples.CreateEnvironmentalAlertEvent();
            CreateAndSaveAsset(environmentalAlertEvent, EVENT_CHANNEL_PATH + "EnvironmentalAlertEvent.asset");
            
            // Create Currency Changed Event
            var currencyChangedEvent = EventChannelExamples.CreateCurrencyChangedEvent();
            CreateAndSaveAsset(currencyChangedEvent, EVENT_CHANNEL_PATH + "CurrencyChangedEvent.asset");
            
            // Create Equipment Malfunction Event
            var equipmentMalfunctionEvent = EventChannelExamples.CreateEquipmentMalfunctionEvent();
            CreateAndSaveAsset(equipmentMalfunctionEvent, EVENT_CHANNEL_PATH + "EquipmentMalfunctionEvent.asset");
            
            // Create Game State Changed Event
            var gameStateChangedEvent = EventChannelExamples.CreateGameStateChangedEvent();
            CreateAndSaveAsset(gameStateChangedEvent, EVENT_CHANNEL_PATH + "GameStateChangedEvent.asset");
            
            // Create Research Completed Event
            var researchCompletedEvent = EventChannelExamples.CreateResearchCompletedEvent();
            CreateAndSaveAsset(researchCompletedEvent, EVENT_CHANNEL_PATH + "ResearchCompletedEvent.asset");
            
            AssetDatabase.Refresh();
            Debug.Log("[Chimera] Created example Event Channel assets");
        }
        
        [MenuItem("Project Chimera/Fix All Asset Configuration Issues")]
        public static void FixAllAssetConfigurationIssues()
        {
            CreateExampleUIDataBindings();
            CreateExampleEventChannels();
            ValidateExistingAssets();
            
            Debug.Log("[Chimera] Fixed all asset configuration issues");
        }
        
        /// <summary>
        /// Creates and saves a ScriptableObject asset at the specified path
        /// </summary>
        private static void CreateAndSaveAsset(ScriptableObject asset, string path)
        {
            // Check if asset already exists
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
            {
                Debug.LogWarning($"[Chimera] Asset already exists at {path}, skipping creation");
                return;
            }
            
            // Create the asset
            AssetDatabase.CreateAsset(asset, path);
            
            // Mark as dirty and save
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[Chimera] Created asset: {path}");
        }
        
        /// <summary>
        /// Validates existing assets and fixes common configuration issues
        /// </summary>
        private static void ValidateExistingAssets()
        {
            // Find all UIDataBindingSO assets
            string[] bindingGuids = AssetDatabase.FindAssets("t:UIDataBindingSO");
            foreach (string guid in bindingGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var binding = AssetDatabase.LoadAssetAtPath<UIDataBindingSO>(path);
                
                if (binding != null)
                {
                    ValidateUIDataBinding(binding, path);
                }
            }
            
            // Find all GameEventSO assets
            string[] eventGuids = AssetDatabase.FindAssets("t:ChimeraEventSO");
            foreach (string guid in eventGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var eventChannel = AssetDatabase.LoadAssetAtPath<ChimeraEventSO>(path);
                
                if (eventChannel != null)
                {
                    ValidateEventChannel(eventChannel, path);
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// Validates a UI Data Binding asset and fixes common issues
        /// </summary>
        private static void ValidateUIDataBinding(UIDataBindingSO binding, string path)
        {
            bool needsUpdate = false;
            
            // Check if binding name is empty
            if (string.IsNullOrEmpty(binding.BindingName))
            {
                // Set binding name to asset name
                var bindingNameField = typeof(UIDataBindingSO).GetField("_bindingName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bindingNameField?.SetValue(binding, binding.name);
                needsUpdate = true;
            }
            
            // Check if source manager type is empty
            if (string.IsNullOrEmpty(binding.SourceManagerType))
            {
                var sourceManagerField = typeof(UIDataBindingSO).GetField("_sourceManagerType", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                sourceManagerField?.SetValue(binding, "ExampleManager");
                needsUpdate = true;
            }
            
            // Check if target UI element is empty
            if (string.IsNullOrEmpty(binding.TargetUIElement))
            {
                var targetElementField = typeof(UIDataBindingSO).GetField("_targetUIElement", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                targetElementField?.SetValue(binding, "ExampleUIElement");
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                EditorUtility.SetDirty(binding);
                Debug.Log($"[Chimera] Fixed UI Data Binding configuration: {path}");
            }
        }
        
        /// <summary>
        /// Validates an Event Channel asset and fixes common issues
        /// </summary>
        private static void ValidateEventChannel(ChimeraEventSO eventChannel, string path)
        {
            bool needsUpdate = false;
            
            // Check if display name is empty
            if (string.IsNullOrEmpty(eventChannel.DisplayName))
            {
                var displayNameField = typeof(ChimeraScriptableObject).GetField("_displayName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                displayNameField?.SetValue(eventChannel, eventChannel.name);
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                EditorUtility.SetDirty(eventChannel);
                Debug.Log($"[Chimera] Fixed Event Channel configuration: {path}");
            }
        }
        
        /// <summary>
        /// Cleans up corrupted asset files that cause XML parsing errors
        /// </summary>
        [MenuItem("Project Chimera/Clean Up Corrupted Assets")]
        public static void CleanUpCorruptedAssets()
        {
            // Find all .asset files in the project
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            
            foreach (string path in assetPaths)
            {
                if (path.EndsWith(".asset"))
                {
                    // Try to load the asset
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    
                    if (asset == null)
                    {
                        // Asset file exists but can't be loaded - likely corrupted
                        Debug.LogWarning($"[Chimera] Found potentially corrupted asset: {path}");
                        
                        // You can uncomment the following line to delete corrupted assets
                        // AssetDatabase.DeleteAsset(path);
                    }
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("[Chimera] Cleaned up corrupted assets");
        }
    }
}