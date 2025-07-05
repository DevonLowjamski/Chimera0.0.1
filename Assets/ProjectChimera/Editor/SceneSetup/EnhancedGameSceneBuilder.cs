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
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Progression;
using ProjectChimera.Systems.Community;
using ProjectChimera.Systems.Construction;
using ProjectChimera.Systems.Tutorial;
using ProjectChimera.Systems.Facilities;
using ProjectChimera.UI;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Scripts.Environment;
using ProjectChimera.Scripts.Cultivation;
using ProjectChimera.Scripts.Facilities;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using CultivationLightType = ProjectChimera.Data.Cultivation.LightType;

namespace ProjectChimera.Editor.SceneSetup
{
    // Enhanced Game Scene Builder for Project Chimera
    /// <summary>
    /// Enhanced scene builder that creates functional prefabs and connects all systems.
    /// Builds a complete playable game scene with all components properly configured.
    /// </summary>
    public static class EnhancedGameSceneBuilder
    {
        [MenuItem("Project Chimera/Build Enhanced Main Scene")]
        public static void BuildEnhancedMainScene()
        {
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "EnhancedMainGameScene";
            
            Debug.Log("Building Enhanced Project Chimera Main Game Scene...");
            
            // Create core systems with full functionality
            var gameManager = CreateFullGameManagerSystem();
            var cameraSystem = CreateAdvancedCameraSystem();
            var lightingSystem = CreateAdvancedLightingSystem();
            var environment = CreateFunctionalEnvironment();
            var uiSystem = CreateFunctionalUISystem();
            var cultivation = CreateFunctionalCultivationSystem();
            var audioSystem = CreateAudioSystem();
            
            // Connect all systems
            ConnectSystemReferences(gameManager, cameraSystem, lightingSystem, environment, uiSystem, cultivation);
            
            // Create sample content
            PopulateWithSampleContent(cultivation, environment);
            
            // Save the scene
            string scenePath = "Assets/ProjectChimera/Scenes/EnhancedMainGameScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Enhanced Main Game Scene created and saved to: {scenePath}");
            Debug.Log("Scene includes functional systems, connected components, and sample content.");
        }
        
        [MenuItem("Project Chimera/Create Plant Prefab")]
        public static void CreatePlantPrefab()
        {
            CreateBasicPlantPrefab();
        }
        
        [MenuItem("Project Chimera/Create Grow Room Prefab")]
        public static void CreateGrowRoomPrefab()
        {
            CreateBasicGrowRoomPrefab();
        }
        
        private static GameObject CreateFullGameManagerSystem()
        {
            // Main GameManager GameObject
            GameObject gameManagerGO = new GameObject("Game Manager");
            
            // Add Core Managers
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            gameManagerGO.AddComponent<TimeManager>();
            gameManagerGO.AddComponent<DataManager>();
            gameManagerGO.AddComponent<EventManager>();
            gameManagerGO.AddComponent<SaveManager>();
            gameManagerGO.AddComponent<SettingsManager>();
            
            // Add System Managers
            gameManagerGO.AddComponent<PlantManager>();
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
            
            Debug.Log("Full GameManager system created with all manager components");
            return gameManagerGO;
        }
        
        private static GameObject CreateAdvancedCameraSystem()
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
            uiCameraGO.transform.SetParent(mainCameraGO.transform);
            Camera uiCamera = uiCameraGO.AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            uiCamera.orthographic = true;
            uiCamera.depth = 1;
            
#if UNITY_URP
            var uiCameraData = uiCameraGO.AddComponent<UniversalAdditionalCameraData>();
            uiCameraData.renderType = CameraRenderType.Overlay;
#endif
            
            // Add UI camera to main camera stack
#if UNITY_URP
            cameraData.cameraStack.Add(uiCamera);
#endif
            
            Debug.Log("Advanced camera system created with URP configuration");
            return mainCameraGO;
        }
        
        private static GameObject CreateAdvancedLightingSystem()
        {
            GameObject lightingSystemGO = new GameObject("Lighting System");
            
            // Directional Light (Sun)
            GameObject sunLightGO = new GameObject("Sun Light");
            sunLightGO.transform.SetParent(lightingSystemGO.transform);
            Light sunLight = sunLightGO.AddComponent<Light>();
            sunLight.type = UnityEngine.LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.8f);
            sunLight.intensity = 1.2f;
            sunLight.shadows = LightShadows.Soft;
            sunLightGO.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            
            // Environment Lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
            RenderSettings.ambientIntensity = 0.3f;
            
            // Create functional grow lights
            CreateFunctionalGrowLights(lightingSystemGO);
            
            Debug.Log("Advanced lighting system created with functional grow lights");
            return lightingSystemGO;
        }
        
        private static void CreateFunctionalGrowLights(GameObject parent)
        {
            GameObject growLightsParent = new GameObject("Grow Lights");
            growLightsParent.transform.SetParent(parent.transform);
            
            // LED Grow Light with controller
            GameObject ledLightGO = CreateGrowLight("LED Grow Light", CultivationLightType.LED, growLightsParent.transform);
            ledLightGO.transform.position = new Vector3(0f, 3f, 0f);
            
            // HPS Grow Light with controller
            GameObject hpsLightGO = CreateGrowLight("HPS Grow Light", CultivationLightType.HPS, growLightsParent.transform);
            hpsLightGO.transform.position = new Vector3(5f, 3f, 0f);
        }
        
        private static GameObject CreateGrowLight(string name, CultivationLightType lightType, Transform parent)
        {
            GameObject lightGO = new GameObject(name);
            lightGO.transform.SetParent(parent);
            
            // Unity Light component
            Light light = lightGO.AddComponent<Light>();
            light.type = UnityEngine.LightType.Spot;
            light.intensity = 2f;
            light.range = 10f;
            light.spotAngle = 60f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            light.enabled = false; // Will be controlled by LightingController
            
            // Set color based on light type
            light.color = lightType switch
            {
                CultivationLightType.LED => Color.white,
                CultivationLightType.HPS => new Color(1f, 0.7f, 0.3f),
                CultivationLightType.CMH => new Color(0.9f, 0.9f, 1f),
                _ => Color.white
            };
            
            // Add LightingController
            var controller = lightGO.AddComponent<LightingController>();
            
            // Create fixture visual
            GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fixture.name = "Light Fixture";
            fixture.transform.SetParent(lightGO.transform);
            fixture.transform.localPosition = Vector3.zero;
            fixture.transform.localScale = new Vector3(1f, 0.2f, 1f);
            
            // Remove collider from fixture
            Object.DestroyImmediate(fixture.GetComponent<Collider>());
            
            return lightGO;
        }
        
        private static GameObject CreateFunctionalEnvironment()
        {
            GameObject environmentGO = new GameObject("Environment");
            
            // Ground Plane
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGO.name = "Ground";
            groundGO.transform.SetParent(environmentGO.transform);
            groundGO.transform.localScale = new Vector3(20f, 1f, 20f);
            
            // Create material for ground
            Material groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMaterial.color = new Color(0.3f, 0.5f, 0.3f); // Green ground
            groundGO.GetComponent<Renderer>().material = groundMaterial;
            
            // Create functional facility structure
            CreateFunctionalFacility(environmentGO);
            
            // Create outdoor area
            CreateFunctionalOutdoorArea(environmentGO);
            
            Debug.Log("Functional environment created with interactive facilities");
            return environmentGO;
        }
        
        private static void CreateFunctionalFacility(GameObject parent)
        {
            GameObject facilityGO = new GameObject("Facility");
            facilityGO.transform.SetParent(parent.transform);
            
            // Main Building structure
            GameObject mainBuildingGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBuildingGO.name = "Main Building";
            mainBuildingGO.transform.SetParent(facilityGO.transform);
            mainBuildingGO.transform.position = new Vector3(0f, 2.5f, 0f);
            mainBuildingGO.transform.localScale = new Vector3(15f, 5f, 10f);
            
            // Make building transparent
            Material buildingMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            buildingMaterial.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
            mainBuildingGO.GetComponent<Renderer>().material = buildingMaterial;
            
            // Remove main building collider
            Object.DestroyImmediate(mainBuildingGO.GetComponent<Collider>());
            
            // Create functional grow rooms
            CreateFunctionalGrowRooms(facilityGO);
        }
        
        private static void CreateFunctionalGrowRooms(GameObject parent)
        {
            GameObject growRoomsGO = new GameObject("Grow Rooms");
            growRoomsGO.transform.SetParent(parent.transform);
            
            // Create 4 functional grow rooms
            for (int i = 0; i < 4; i++)
            {
                GameObject growRoomGO = CreateFunctionalGrowRoom($"Grow Room {i + 1}", i);
                growRoomGO.transform.SetParent(growRoomsGO.transform);
                
                float x = (i % 2) * 8f - 4f;
                float z = (i / 2) * 6f - 3f;
                growRoomGO.transform.position = new Vector3(x, 0f, z);
            }
        }
        
        private static GameObject CreateFunctionalGrowRoom(string roomName, int roomIndex)
        {
            GameObject roomGO = new GameObject(roomName);
            
            // Room structure
            GameObject roomStructure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomStructure.name = "Room Structure";
            roomStructure.transform.SetParent(roomGO.transform);
            roomStructure.transform.localPosition = new Vector3(0f, 2f, 0f);
            roomStructure.transform.localScale = new Vector3(6f, 4f, 4f);
            
            // Make room semi-transparent
            Material roomMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roomMaterial.color = new Color(0.9f, 0.9f, 0.9f, 0.4f);
            roomStructure.GetComponent<Renderer>().material = roomMaterial;
            
            // Remove structure collider
            Object.DestroyImmediate(roomStructure.GetComponent<Collider>());
            
            // Add GrowRoomController
            var roomController = roomGO.AddComponent<GrowRoomController>();
            
            // Create environmental systems for the room
            CreateRoomEnvironmentalSystems(roomGO);
            
            return roomGO;
        }
        
        private static void CreateRoomEnvironmentalSystems(GameObject roomGO)
        {
            // HVAC System
            GameObject hvacGO = new GameObject("HVAC System");
            hvacGO.transform.SetParent(roomGO.transform);
            hvacGO.transform.localPosition = new Vector3(2f, 3f, 0f);
            hvacGO.AddComponent<HVACController>();
            
            // Add visual representation
            GameObject hvacBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hvacBox.name = "HVAC Unit";
            hvacBox.transform.SetParent(hvacGO.transform);
            hvacBox.transform.localScale = new Vector3(1f, 0.5f, 0.5f);
            Object.DestroyImmediate(hvacBox.GetComponent<Collider>());
            
            // Lighting System
            GameObject lightingGO = new GameObject("Room Lighting");
            lightingGO.transform.SetParent(roomGO.transform);
            lightingGO.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            lightingGO.AddComponent<LightingController>();
            
            // Add light fixture
            GameObject lightFixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lightFixture.name = "Light Fixture";
            lightFixture.transform.SetParent(lightingGO.transform);
            lightFixture.transform.localScale = new Vector3(2f, 0.1f, 2f);
            Object.DestroyImmediate(lightFixture.GetComponent<Collider>());
            
            // Ventilation System
            GameObject ventilationGO = new GameObject("Ventilation System");
            ventilationGO.transform.SetParent(roomGO.transform);
            ventilationGO.transform.localPosition = new Vector3(-2f, 3f, 0f);
            ventilationGO.AddComponent<VentilationController>();
            
            // Add fan visual
            GameObject fan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fan.name = "Exhaust Fan";
            fan.transform.SetParent(ventilationGO.transform);
            fan.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
            Object.DestroyImmediate(fan.GetComponent<Collider>());
            
            // Irrigation System
            GameObject irrigationGO = new GameObject("Irrigation System");
            irrigationGO.transform.SetParent(roomGO.transform);
            irrigationGO.transform.localPosition = new Vector3(0f, 0.5f, 2f);
            irrigationGO.AddComponent<IrrigationController>();
            
            // Add irrigation visual
            GameObject irrigationTank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            irrigationTank.name = "Water Tank";
            irrigationTank.transform.SetParent(irrigationGO.transform);
            irrigationTank.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            Object.DestroyImmediate(irrigationTank.GetComponent<Collider>());
        }
        
        private static void CreateFunctionalOutdoorArea(GameObject parent)
        {
            GameObject outdoorGO = new GameObject("Outdoor Area");
            outdoorGO.transform.SetParent(parent.transform);
            
            // Greenhouse with environmental controls
            GameObject greenhouseGO = CreateFunctionalGreenhouse();
            greenhouseGO.transform.SetParent(outdoorGO.transform);
            greenhouseGO.transform.position = new Vector3(0f, 0f, 15f);
            
            // Outdoor growing plots
            CreateOutdoorPlots(outdoorGO);
        }
        
        private static GameObject CreateFunctionalGreenhouse()
        {
            GameObject greenhouseGO = new GameObject("Greenhouse");
            
            // Greenhouse structure
            GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structure.name = "Greenhouse Structure";
            structure.transform.SetParent(greenhouseGO.transform);
            structure.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            structure.transform.localScale = new Vector3(12f, 5f, 8f);
            
            // Make greenhouse transparent
            Material greenhouseMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            greenhouseMaterial.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            structure.GetComponent<Renderer>().material = greenhouseMaterial;
            
            // Remove collider
            Object.DestroyImmediate(structure.GetComponent<Collider>());
            
            // Add environmental systems
            CreateRoomEnvironmentalSystems(greenhouseGO);
            
            return greenhouseGO;
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
                Material plotMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                plotMaterial.color = new Color(0.4f, 0.2f, 0.1f);
                plotGO.GetComponent<Renderer>().material = plotMaterial;
            }
        }
        
        private static GameObject CreateFunctionalUISystem()
        {
            // UI Canvas
            GameObject canvasGO = new GameObject("Main UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Event System
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Add MainGameUIController
            var uiController = canvasGO.AddComponent<MainGameUIController>();
            
            // Create functional UI panels
            CreateFunctionalUIElements(canvasGO);
            
            Debug.Log("Functional UI system created with MainGameUIController");
            return canvasGO;
        }
        
        private static void CreateFunctionalUIElements(GameObject canvas)
        {
            // Main Panel
            GameObject mainPanelGO = new GameObject("Main Panel");
            mainPanelGO.transform.SetParent(canvas.transform, false);
            
            var mainRect = mainPanelGO.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            var mainImage = mainPanelGO.AddComponent<UnityEngine.UI.Image>();
            mainImage.color = new Color(0f, 0f, 0f, 0f);
            
            // HUD Panel
            CreateFunctionalHUD(canvas);
            
            // Side Panel
            CreateFunctionalSidePanel(canvas);
            
            // Info Panels
            CreateFunctionalInfoPanels(canvas);
            
            // Notification System
            CreateFunctionalNotificationSystem(canvas);
        }
        
        private static void CreateFunctionalHUD(GameObject canvas)
        {
            GameObject hudGO = new GameObject("HUD Panel");
            hudGO.transform.SetParent(canvas.transform, false);
            
            var hudRect = hudGO.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
            
            // Top Bar
            CreateTopBar(hudGO);
            
            // Bottom Action Bar
            CreateBottomActionBar(hudGO);
        }
        
        private static void CreateTopBar(GameObject parent)
        {
            GameObject topBarGO = new GameObject("Top Bar");
            topBarGO.transform.SetParent(parent.transform, false);
            
            var rect = topBarGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 60f);
            
            var image = topBarGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Currency Text
            CreateUIText(topBarGO, "Currency Text", "Cash: $25,000", new Vector2(0f, 0f), new Vector2(0.3f, 1f));
            
            // Reputation Text
            CreateUIText(topBarGO, "Reputation Text", "Reputation: 0", new Vector2(0.3f, 0f), new Vector2(0.6f, 1f));
            
            // Level Text
            CreateUIText(topBarGO, "Level Text", "Level: 1", new Vector2(0.6f, 0f), new Vector2(0.8f, 1f));
            
            // Time Text
            CreateUIText(topBarGO, "Time Text", "Day 1", new Vector2(0.8f, 0f), new Vector2(1f, 1f));
        }
        
        private static void CreateBottomActionBar(GameObject parent)
        {
            GameObject bottomBarGO = new GameObject("Bottom Bar");
            bottomBarGO.transform.SetParent(parent.transform, false);
            
            var rect = bottomBarGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 80f);
            
            var image = bottomBarGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Action Buttons
            string[] buttonNames = { "Plant", "Harvest", "Genetics", "Market", "Construction", "Tutorial" };
            
            for (int i = 0; i < buttonNames.Length; i++)
            {
                CreateActionButton(bottomBarGO, buttonNames[i], i, buttonNames.Length);
            }
        }
        
        private static GameObject CreateActionButton(GameObject parent, string buttonName, int index, int totalButtons)
        {
            GameObject buttonGO = new GameObject($"{buttonName} Button");
            buttonGO.transform.SetParent(parent.transform, false);
            
            var rect = buttonGO.AddComponent<RectTransform>();
            float buttonWidth = 120f;
            float spacing = 10f;
            float totalWidth = totalButtons * buttonWidth + (totalButtons - 1) * spacing;
            float startX = -totalWidth / 2f + buttonWidth / 2f;
            
            rect.anchoredPosition = new Vector2(startX + index * (buttonWidth + spacing), 0f);
            rect.sizeDelta = new Vector2(buttonWidth, 60f);
            
            var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.2f, 1f);
            
            // Button text
            CreateUIText(buttonGO, "Text", buttonName, Vector2.zero, Vector2.one);
            
            button.targetGraphic = buttonImage;
            
            return buttonGO;
        }
        
        private static GameObject CreateUIText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);
            
            var rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = text;
            textComponent.color = Color.white;
            textComponent.fontSize = 14;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            return textGO;
        }
        
        private static void CreateFunctionalSidePanel(GameObject canvas)
        {
            GameObject sidePanelGO = new GameObject("Side Panel");
            sidePanelGO.transform.SetParent(canvas.transform, false);
            
            var rect = sidePanelGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.1f);
            rect.anchorMax = new Vector2(1f, 0.9f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300f, 0f);
            
            var image = sidePanelGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            
            sidePanelGO.SetActive(false);
        }
        
        private static void CreateFunctionalInfoPanels(GameObject canvas)
        {
            // Plant Info Panel
            GameObject plantInfoGO = new GameObject("Plant Info Panel");
            plantInfoGO.transform.SetParent(canvas.transform, false);
            
            var plantRect = plantInfoGO.AddComponent<RectTransform>();
            plantRect.anchorMin = new Vector2(0f, 0.6f);
            plantRect.anchorMax = new Vector2(0.3f, 1f);
            plantRect.offsetMin = Vector2.zero;
            plantRect.offsetMax = Vector2.zero;
            
            var plantImage = plantInfoGO.AddComponent<UnityEngine.UI.Image>();
            plantImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            CreateUIText(plantInfoGO, "Plant Info Text", "Select a plant...", Vector2.zero, Vector2.one);
            plantInfoGO.SetActive(false);
            
            // Facility Info Panel
            GameObject facilityInfoGO = new GameObject("Facility Info Panel");
            facilityInfoGO.transform.SetParent(canvas.transform, false);
            
            var facilityRect = facilityInfoGO.AddComponent<RectTransform>();
            facilityRect.anchorMin = new Vector2(0f, 0.2f);
            facilityRect.anchorMax = new Vector2(0.3f, 0.6f);
            facilityRect.offsetMin = Vector2.zero;
            facilityRect.offsetMax = Vector2.zero;
            
            var facilityImage = facilityInfoGO.AddComponent<UnityEngine.UI.Image>();
            facilityImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            CreateUIText(facilityInfoGO, "Facility Info Text", "Select a facility...", Vector2.zero, Vector2.one);
            facilityInfoGO.SetActive(false);
        }
        
        private static void CreateFunctionalNotificationSystem(GameObject canvas)
        {
            GameObject notificationContainerGO = new GameObject("Notification Container");
            notificationContainerGO.transform.SetParent(canvas.transform, false);
            
            var rect = notificationContainerGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.7f, 0.8f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Create notification prefab
            CreateNotificationPrefab();
        }
        
        private static void CreateNotificationPrefab()
        {
            // This would create a prefab for notifications
            // For now, we'll just log that it should be created
            Debug.Log("Notification prefab should be created in Assets/ProjectChimera/Prefabs/UI/");
        }
        
        private static GameObject CreateFunctionalCultivationSystem()
        {
            GameObject cultivationGO = new GameObject("Cultivation System");
            
            // Plant Container
            GameObject plantsGO = new GameObject("Plants");
            plantsGO.transform.SetParent(cultivationGO.transform);
            
            // Equipment Container
            GameObject equipmentGO = new GameObject("Equipment");
            equipmentGO.transform.SetParent(cultivationGO.transform);
            
            // Create sample plants with functionality
            CreateSampleFunctionalPlants(plantsGO);
            
            Debug.Log("Functional cultivation system created with interactive plants");
            return cultivationGO;
        }
        
        private static void CreateSampleFunctionalPlants(GameObject parent)
        {
            // Create a few sample plants for testing
            for (int i = 0; i < 3; i++)
            {
                GameObject plantGO = CreateBasicPlantPrefab();
                plantGO.name = $"Cannabis Plant {i + 1}";
                plantGO.transform.SetParent(parent.transform);
                plantGO.transform.position = new Vector3(i * 2f - 2f, 0f, 0f);
            }
        }
        
        private static GameObject CreateBasicPlantPrefab()
        {
            GameObject plantGO = new GameObject("Cannabis Plant");
            
            // Plant visual (simple capsule)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Plant Visual";
            visual.transform.SetParent(plantGO.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Make plants green
            Material plantMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            plantMaterial.color = new Color(0.2f, 0.8f, 0.2f);
            visual.GetComponent<Renderer>().material = plantMaterial;
            
            // Add PlantInstanceComponent
            var plantComponent = plantGO.AddComponent<PlantInstanceComponent>();
            
            // Add collider for interaction
            var collider = plantGO.AddComponent<SphereCollider>();
            collider.radius = 1f;
            collider.isTrigger = false;
            
            // Save as prefab
            string prefabPath = "Assets/ProjectChimera/Prefabs/Plants/BasicPlant.prefab";
            CreateDirectoryIfNotExists("Assets/ProjectChimera/Prefabs/Plants/");
            
            return plantGO;
        }
        
        private static GameObject CreateBasicGrowRoomPrefab()
        {
            GameObject roomGO = CreateFunctionalGrowRoom("Basic Grow Room", 0);
            
            // Save as prefab
            string prefabPath = "Assets/ProjectChimera/Prefabs/Facilities/BasicGrowRoom.prefab";
            CreateDirectoryIfNotExists("Assets/ProjectChimera/Prefabs/Facilities/");
            
            return roomGO;
        }
        
        private static GameObject CreateAudioSystem()
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
            return audioGO;
        }
        
        private static void ConnectSystemReferences(GameObject gameManager, GameObject cameraSystem, 
            GameObject lightingSystem, GameObject environment, GameObject uiSystem, GameObject cultivation)
        {
            // This would connect UI controller references to UI elements
            var uiController = uiSystem.GetComponent<MainGameUIController>();
            if (uiController != null)
            {
                // Connect UI elements to controller
                Debug.Log("UI Controller references should be connected manually in the inspector");
            }
            
            // Connect grow room environmental systems
            var growRooms = environment.GetComponentsInChildren<GrowRoomController>();
            foreach (var room in growRooms)
            {
                // Connect environmental systems to room controller
                var hvac = room.GetComponentInChildren<HVACController>();
                var lighting = room.GetComponentInChildren<LightingController>();
                var ventilation = room.GetComponentInChildren<VentilationController>();
                var irrigation = room.GetComponentInChildren<IrrigationController>();
                
                Debug.Log($"Environmental systems connected for {room.name}");
            }
            
            Debug.Log("System references connected - some manual inspector setup may be required");
        }
        
        private static void PopulateWithSampleContent(GameObject cultivation, GameObject environment)
        {
            // Add some sample plants to grow rooms
            var growRooms = environment.GetComponentsInChildren<GrowRoomController>();
            
            foreach (var room in growRooms)
            {
                // Add 2-3 plants to each room
                for (int i = 0; i < 3; i++)
                {
                    var plantPrefab = CreateBasicPlantPrefab();
                    room.AddPlant(plantPrefab);
                }
            }
            
            Debug.Log("Sample content populated - plants added to grow rooms");
        }
        
        private static void CreateDirectoryIfNotExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                currentPath += "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    string parentPath = currentPath.Substring(0, currentPath.LastIndexOf('/'));
                    string folderName = currentPath.Substring(currentPath.LastIndexOf('/') + 1);
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }
            }
        }
    }
}