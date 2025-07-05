using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Economy;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Systems.Community;
using ProjectChimera.Systems.Construction;
using ProjectChimera.Systems.Tutorial;

namespace ProjectChimera.Editor.SceneSetup
{
    /// <summary>
    /// Automated scene builder for Project Chimera main game scene.
    /// Creates all necessary GameObjects, cameras, lighting, and manager components.
    /// </summary>
    public static class GameSceneBuilder
    {
        [MenuItem("Project Chimera/Build Main Game Scene")]
        public static void BuildMainGameScene()
        {
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "MainGameScene";
            
            Debug.Log("Building Project Chimera Main Game Scene...");
            
            // Create core systems
            CreateCameraSystem();
            CreateLightingSystem();
            CreateGameManagerSystem();
            CreateEnvironmentSystem();
            CreateUISystem();
            CreateCultivationSystem();
            CreateAudioSystem();
            
            // Save the scene
            string scenePath = "Assets/ProjectChimera/Scenes/MainGameScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Main Game Scene created and saved to: {scenePath}");
        }
        
        [MenuItem("Project Chimera/Build Tutorial Scene")]
        public static void BuildTutorialScene()
        {
            // Create tutorial scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "TutorialScene";
            
            Debug.Log("Building Project Chimera Tutorial Scene...");
            
            // Create simplified systems for tutorial
            CreateCameraSystem();
            CreateLightingSystem();
            CreateGameManagerSystem();
            CreateSimplifiedEnvironment();
            CreateTutorialUI();
            CreateBasicCultivationSetup();
            
            // Save the scene
            string scenePath = "Assets/ProjectChimera/Scenes/TutorialScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Tutorial Scene created and saved to: {scenePath}");
        }
        
        private static void CreateCameraSystem()
        {
            // Main Camera
            GameObject mainCameraGO = new GameObject("Main Camera");
            Camera mainCamera = mainCameraGO.AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = new Vector3(0f, 10f, -10f);
            mainCamera.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            
            // Configure camera for URP
#if UNITY_URP
            var cameraData = mainCameraGO.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderType = CameraRenderType.Base;
            cameraData.cameraStack.Clear();
#endif
            
            // Add audio listener
            mainCameraGO.AddComponent<AudioListener>();
            
            // UI Camera (overlay)
            GameObject uiCameraGO = new GameObject("UI Camera");
            Camera uiCamera = uiCameraGO.AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            uiCamera.orthographic = true;
            uiCamera.depth = 1; // Higher depth than main camera
            
#if UNITY_URP
            var uiCameraData = uiCameraGO.AddComponent<UniversalAdditionalCameraData>();
            uiCameraData.renderType = CameraRenderType.Overlay;
            
            // Add UI camera to main camera stack
            cameraData.cameraStack.Add(uiCamera);
#endif
            
            Debug.Log("Camera system created");
        }
        
        private static void CreateLightingSystem()
        {
            // Directional Light (Sun)
            GameObject sunLightGO = new GameObject("Sun Light");
            Light sunLight = sunLightGO.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.8f); // Warm sunlight
            sunLight.intensity = 1.2f;
            sunLight.shadows = LightShadows.Soft;
            sunLightGO.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            
            // Environment Lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
            RenderSettings.ambientIntensity = 0.3f;
            
            // Indoor Grow Lights (for cultivation areas)
            CreateGrowLightSystem();
            
            Debug.Log("Lighting system created");
        }
        
        private static void CreateGrowLightSystem()
        {
            GameObject growLightsParent = new GameObject("Grow Lights");
            
            // LED Grow Light
            GameObject ledLightGO = new GameObject("LED Grow Light");
            ledLightGO.transform.SetParent(growLightsParent.transform);
            ledLightGO.transform.position = new Vector3(0f, 3f, 0f);
            
            Light ledLight = ledLightGO.AddComponent<Light>();
            ledLight.type = LightType.Spot;
            ledLight.color = new Color(1f, 0.8f, 1f); // Purple-ish grow light
            ledLight.intensity = 2f;
            ledLight.range = 10f;
            ledLight.spotAngle = 60f;
            ledLight.shadows = LightShadows.Soft;
            ledLightGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // HPS Grow Light (backup/supplemental)
            GameObject hpsLightGO = new GameObject("HPS Grow Light");
            hpsLightGO.transform.SetParent(growLightsParent.transform);
            hpsLightGO.transform.position = new Vector3(5f, 3f, 0f);
            
            Light hpsLight = hpsLightGO.AddComponent<Light>();
            hpsLight.type = LightType.Spot;
            hpsLight.color = new Color(1f, 0.7f, 0.3f); // Orange HPS color
            hpsLight.intensity = 1.5f;
            hpsLight.range = 8f;
            hpsLight.spotAngle = 45f;
            hpsLight.shadows = LightShadows.Soft;
            hpsLightGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // Initially disable grow lights (will be controlled by systems)
            ledLight.enabled = false;
            hpsLight.enabled = false;
        }
        
        private static void CreateGameManagerSystem()
        {
            // Main GameManager GameObject
            GameObject gameManagerGO = new GameObject("Game Manager");
            
            // Add Core Managers
            gameManagerGO.AddComponent<GameManager>();
            gameManagerGO.AddComponent<TimeManager>();
            gameManagerGO.AddComponent<DataManager>();
            gameManagerGO.AddComponent<EventManager>();
            gameManagerGO.AddComponent<SaveManager>();
            gameManagerGO.AddComponent<SettingsManager>();
            
            // Add System Managers
            gameManagerGO.AddComponent<CultivationManager>();
            gameManagerGO.AddComponent<GeneticsManager>();
            gameManagerGO.AddComponent<EnvironmentSystems.EnvironmentalManager>();
            gameManagerGO.AddComponent<CurrencyManager>();
            gameManagerGO.AddComponent<ProgressionManager>();
            gameManagerGO.AddComponent<CommunityManager>();
            gameManagerGO.AddComponent<ConstructionManager>();
            gameManagerGO.AddComponent<EnhancedTutorialManager>();
            
            // Create System References GameObject
            GameObject systemRefsGO = new GameObject("System References");
            systemRefsGO.transform.SetParent(gameManagerGO.transform);
            
            Debug.Log("GameManager system created with all manager components");
        }
        
        private static void CreateEnvironmentSystem()
        {
            // Environment Container
            GameObject environmentGO = new GameObject("Environment");
            
            // Ground Plane
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGO.name = "Ground";
            groundGO.transform.SetParent(environmentGO.transform);
            groundGO.transform.localScale = new Vector3(20f, 1f, 20f); // 200x200 units
            
            // Create facility structure
            CreateFacilityStructure(environmentGO);
            
            // Create outdoor area
            CreateOutdoorArea(environmentGO);
            
            Debug.Log("Environment system created");
        }
        
        private static void CreateFacilityStructure(GameObject parent)
        {
            GameObject facilityGO = new GameObject("Facility");
            facilityGO.transform.SetParent(parent.transform);
            
            // Main Building
            GameObject mainBuildingGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBuildingGO.name = "Main Building";
            mainBuildingGO.transform.SetParent(facilityGO.transform);
            mainBuildingGO.transform.position = new Vector3(0f, 2.5f, 0f);
            mainBuildingGO.transform.localScale = new Vector3(15f, 5f, 10f);
            
            // Remove collider from building (we'll add custom ones)
            Object.DestroyImmediate(mainBuildingGO.GetComponent<Collider>());
            
            // Grow Rooms
            CreateGrowRooms(facilityGO);
            
            // Processing Area
            CreateProcessingArea(facilityGO);
            
            // Storage Area
            CreateStorageArea(facilityGO);
        }
        
        private static void CreateGrowRooms(GameObject parent)
        {
            GameObject growRoomsGO = new GameObject("Grow Rooms");
            growRoomsGO.transform.SetParent(parent.transform);
            
            // Create 4 grow rooms
            for (int i = 0; i < 4; i++)
            {
                GameObject growRoomGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                growRoomGO.name = $"Grow Room {i + 1}";
                growRoomGO.transform.SetParent(growRoomsGO.transform);
                
                float x = (i % 2) * 8f - 4f; // Alternate x position
                float z = (i / 2) * 6f - 3f; // Alternate z position
                growRoomGO.transform.position = new Vector3(x, 2f, z);
                growRoomGO.transform.localScale = new Vector3(3f, 4f, 2.5f);
                
                // Add grow room component (will create later)
                // growRoomGO.AddComponent<GrowRoom>();
            }
        }
        
        private static void CreateProcessingArea(GameObject parent)
        {
            GameObject processingGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            processingGO.name = "Processing Area";
            processingGO.transform.SetParent(parent.transform);
            processingGO.transform.position = new Vector3(8f, 2f, 0f);
            processingGO.transform.localScale = new Vector3(4f, 4f, 6f);
        }
        
        private static void CreateStorageArea(GameObject parent)
        {
            GameObject storageGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            storageGO.name = "Storage Area";
            storageGO.transform.SetParent(parent.transform);
            storageGO.transform.position = new Vector3(-8f, 2f, 0f);
            storageGO.transform.localScale = new Vector3(4f, 4f, 6f);
        }
        
        private static void CreateOutdoorArea(GameObject parent)
        {
            GameObject outdoorGO = new GameObject("Outdoor Area");
            outdoorGO.transform.SetParent(parent.transform);
            
            // Greenhouse
            GameObject greenhouseGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            greenhouseGO.name = "Greenhouse";
            greenhouseGO.transform.SetParent(outdoorGO.transform);
            greenhouseGO.transform.position = new Vector3(0f, 2.5f, 15f);
            greenhouseGO.transform.localScale = new Vector3(12f, 5f, 8f);
            
            // Make greenhouse transparent
            var renderer = greenhouseGO.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            material.SetFloat("_Surface", 1); // Transparent
            material.SetFloat("_Blend", 0); // Alpha blend
            renderer.material = material;
            
            // Outdoor growing plots
            CreateOutdoorPlots(outdoorGO);
        }
        
        private static void CreateOutdoorPlots(GameObject parent)
        {
            GameObject plotsGO = new GameObject("Growing Plots");
            plotsGO.transform.SetParent(parent.transform);
            
            for (int i = 0; i < 6; i++)
            {
                GameObject plotGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plotGO.name = $"Plot {i + 1}";
                plotGO.transform.SetParent(plotsGO.transform);
                
                float x = (i % 3) * 4f - 4f;
                float z = (i / 3) * 4f + 20f;
                plotGO.transform.position = new Vector3(x, 0.2f, z);
                plotGO.transform.localScale = new Vector3(3f, 0.4f, 3f);
                
                // Make plots brown (soil)
                var renderer = plotGO.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.4f, 0.2f, 0.1f); // Brown soil
                renderer.material = material;
            }
        }
        
        private static void CreateUISystem()
        {
            // UI Canvas
            GameObject canvasGO = new GameObject("Main UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Event System
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Create UI panels
            CreateMainUIPanel(canvasGO);
            CreateHUDPanel(canvasGO);
            CreateMenuPanels(canvasGO);
            
            Debug.Log("UI system created");
        }
        
        private static void CreateMainUIPanel(GameObject canvas)
        {
            GameObject mainPanelGO = new GameObject("Main Panel");
            mainPanelGO.transform.SetParent(canvas.transform, false);
            
            var rectTransform = mainPanelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            var image = mainPanelGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0f, 0f, 0f, 0f); // Transparent background
        }
        
        private static void CreateHUDPanel(GameObject canvas)
        {
            GameObject hudGO = new GameObject("HUD Panel");
            hudGO.transform.SetParent(canvas.transform, false);
            
            var rectTransform = hudGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Top bar for currency and stats
            CreateTopBar(hudGO);
            
            // Bottom bar for actions
            CreateBottomBar(hudGO);
            
            // Side panel for information
            CreateSidePanel(hudGO);
        }
        
        private static void CreateTopBar(GameObject parent)
        {
            GameObject topBarGO = new GameObject("Top Bar");
            topBarGO.transform.SetParent(parent.transform, false);
            
            var rectTransform = topBarGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 60f);
            
            var image = topBarGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Currency display
            CreateCurrencyDisplay(topBarGO);
        }
        
        private static void CreateBottomBar(GameObject parent)
        {
            GameObject bottomBarGO = new GameObject("Bottom Bar");
            bottomBarGO.transform.SetParent(parent.transform, false);
            
            var rectTransform = bottomBarGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 80f);
            
            var image = bottomBarGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Action buttons
            CreateActionButtons(bottomBarGO);
        }
        
        private static void CreateSidePanel(GameObject parent)
        {
            GameObject sidePanelGO = new GameObject("Side Panel");
            sidePanelGO.transform.SetParent(parent.transform, false);
            
            var rectTransform = sidePanelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 0.1f);
            rectTransform.anchorMax = new Vector2(1f, 0.9f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(300f, 0f);
            
            var image = sidePanelGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            
            // Initially hidden
            sidePanelGO.SetActive(false);
        }
        
        private static void CreateCurrencyDisplay(GameObject parent)
        {
            GameObject currencyGO = new GameObject("Currency Display");
            currencyGO.transform.SetParent(parent.transform, false);
            
            var rectTransform = currencyGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0.3f, 1f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            var text = currencyGO.AddComponent<UnityEngine.UI.Text>();
            text.text = "Cash: $25,000";
            text.color = Color.white;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleLeft;
            
            // Set default font
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        private static void CreateActionButtons(GameObject parent)
        {
            string[] buttonNames = { "Plant", "Harvest", "Genetics", "Market", "Construction", "Tutorial" };
            
            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject buttonGO = new GameObject($"{buttonNames[i]} Button");
                buttonGO.transform.SetParent(parent.transform, false);
                
                var rectTransform = buttonGO.AddComponent<RectTransform>();
                float buttonWidth = 120f;
                float spacing = 10f;
                float totalWidth = buttonNames.Length * buttonWidth + (buttonNames.Length - 1) * spacing;
                float startX = -totalWidth / 2f + buttonWidth / 2f;
                
                rectTransform.anchoredPosition = new Vector2(startX + i * (buttonWidth + spacing), 0f);
                rectTransform.sizeDelta = new Vector2(buttonWidth, 60f);
                
                var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
                var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
                buttonImage.color = new Color(0.2f, 0.4f, 0.2f, 1f);
                
                // Button text
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);
                
                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                var text = textGO.AddComponent<UnityEngine.UI.Text>();
                text.text = buttonNames[i];
                text.color = Color.white;
                text.fontSize = 14;
                text.alignment = TextAnchor.MiddleCenter;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                
                button.targetGraphic = buttonImage;
            }
        }
        
        private static void CreateMenuPanels(GameObject canvas)
        {
            // These will be created later as needed
            Debug.Log("Menu panels placeholder created");
        }
        
        private static void CreateCultivationSystem()
        {
            GameObject cultivationGO = new GameObject("Cultivation System");
            
            // Plant Container
            GameObject plantsGO = new GameObject("Plants");
            plantsGO.transform.SetParent(cultivationGO.transform);
            
            // Equipment Container
            GameObject equipmentGO = new GameObject("Equipment");
            equipmentGO.transform.SetParent(cultivationGO.transform);
            
            // Create sample plants
            CreateSamplePlants(plantsGO);
            
            Debug.Log("Cultivation system created");
        }
        
        private static void CreateSamplePlants(GameObject parent)
        {
            // Create a few sample plants for testing
            for (int i = 0; i < 3; i++)
            {
                GameObject plantGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                plantGO.name = $"Cannabis Plant {i + 1}";
                plantGO.transform.SetParent(parent.transform);
                plantGO.transform.position = new Vector3(i * 2f - 2f, 1f, 0f);
                plantGO.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                
                // Make plants green
                var renderer = plantGO.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.2f, 0.8f, 0.2f); // Green
                renderer.material = material;
                
                // Add plant component (will create later)
                // plantGO.AddComponent<PlantInstance>();
            }
        }
        
        private static void CreateAudioSystem()
        {
            GameObject audioGO = new GameObject("Audio System");
            
            // Background music
            GameObject musicGO = new GameObject("Background Music");
            musicGO.transform.SetParent(audioGO.transform);
            var musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.3f;
            musicSource.playOnAwake = false;
            
            // SFX
            GameObject sfxGO = new GameObject("SFX");
            sfxGO.transform.SetParent(audioGO.transform);
            var sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            
            Debug.Log("Audio system created");
        }
        
        private static void CreateSimplifiedEnvironment()
        {
            // Simplified environment for tutorial
            GameObject environmentGO = new GameObject("Tutorial Environment");
            
            // Simple ground
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGO.name = "Ground";
            groundGO.transform.SetParent(environmentGO.transform);
            groundGO.transform.localScale = new Vector3(5f, 1f, 5f);
            
            // Simple grow area
            GameObject growAreaGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            growAreaGO.name = "Tutorial Grow Area";
            growAreaGO.transform.SetParent(environmentGO.transform);
            growAreaGO.transform.position = new Vector3(0f, 1f, 0f);
            growAreaGO.transform.localScale = new Vector3(4f, 2f, 4f);
            
            Debug.Log("Simplified tutorial environment created");
        }
        
        private static void CreateTutorialUI()
        {
            // Simplified UI for tutorial
            CreateUISystem(); // Reuse main UI system
            
            // Add tutorial-specific elements
            GameObject tutorialPanelGO = new GameObject("Tutorial Panel");
            // Will implement tutorial UI components later
            
            Debug.Log("Tutorial UI created");
        }
        
        private static void CreateBasicCultivationSetup()
        {
            // Basic cultivation setup for tutorial
            GameObject cultivationGO = new GameObject("Tutorial Cultivation");
            
            // Single plant for tutorial
            GameObject plantGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            plantGO.name = "Tutorial Plant";
            plantGO.transform.SetParent(cultivationGO.transform);
            plantGO.transform.position = new Vector3(0f, 1f, 0f);
            plantGO.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
            
            // Make it green
            var renderer = plantGO.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.3f, 0.9f, 0.3f);
            renderer.material = material;
            
            Debug.Log("Basic cultivation setup created for tutorial");
        }
        
        [MenuItem("Project Chimera/Setup Project Folders")]
        public static void SetupProjectFolders()
        {
            // Create necessary folder structure
            string[] folders = {
                "Assets/ProjectChimera/Scenes",
                "Assets/ProjectChimera/Prefabs",
                "Assets/ProjectChimera/Prefabs/Plants",
                "Assets/ProjectChimera/Prefabs/Equipment",
                "Assets/ProjectChimera/Prefabs/UI",
                "Assets/ProjectChimera/Materials",
                "Assets/ProjectChimera/Textures",
                "Assets/ProjectChimera/Audio",
                "Assets/ProjectChimera/Audio/Music",
                "Assets/ProjectChimera/Audio/SFX",
                "Assets/ProjectChimera/Models",
                "Assets/ProjectChimera/Animations",
                "Assets/ProjectChimera/Resources"
            };
            
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parentFolder = System.IO.Path.GetDirectoryName(folder);
                    string folderName = System.IO.Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Project folder structure created");
        }
    }
}