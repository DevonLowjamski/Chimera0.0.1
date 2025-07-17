using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Cultivation;
using ProjectChimera.Environment;
using ProjectChimera.Scripts.Environment;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Automation;
using ProjectChimera.Data.Facilities;
// Explicit aliases to resolve ambiguity
using SensorType = ProjectChimera.Data.Automation.SensorType;
using RoomLayout = ProjectChimera.Data.Facilities.RoomLayout;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace ProjectChimera.SceneGeneration
{
    /// <summary>
    /// Advanced procedural scene generator that creates diverse cultivation environments
    /// with realistic layouts, environmental systems, and interactive elements.
    /// </summary>
    public class ProceduralSceneGenerator : MonoBehaviour
    {
        [Header("Generation Parameters")]
        [SerializeField] private FacilitySceneType _sceneType = FacilitySceneType.IndoorFacility;
        [SerializeField] private Vector2Int _facilitySize = new Vector2Int(50, 30);
        [SerializeField] private int _numberOfRooms = 8;
        [SerializeField] private float _roomMinSize = 8f;
        [SerializeField] private float _roomMaxSize = 15f;
        [SerializeField] private bool _generateRandomSeed = true;
        [SerializeField] private int _generationSeed = 12345;
        
        [Header("Environmental Settings")]
        [SerializeField] private bool _includeOutdoorArea = true;
        [SerializeField] private bool _includeGreenhouse = true;
        [SerializeField] private bool _includeProcessingArea = true;
        [SerializeField] private bool _includeStorageArea = true;
        [SerializeField] private bool _includeLaboratory = true;
        [SerializeField] private bool _includeOfficeSpace = true;
        
        [Header("Vegetation Settings")]
        [SerializeField] private int _plantsPerRoom = 12;
        [SerializeField] private float _vegetationDensity = 0.7f;
        [SerializeField] private bool _includeWildVegetation = true;
        [SerializeField] private bool _includeLandscaping = true;
        
        [Header("Infrastructure")]
        [SerializeField] private bool _generateElectricalSystems = true;
        [SerializeField] private bool _generatePlumbingSystems = true;
        [SerializeField] private bool _generateHVACSystems = true;
        [SerializeField] private bool _generateSecuritySystems = true;
        [SerializeField] private bool _generateNetworkInfrastructure = true;
        
        [Header("Prefab Libraries")]
        [SerializeField] private GameObject[] _roomPrefabs;
        [SerializeField] private GameObject[] _plantPrefabs;
        [SerializeField] private GameObject[] _equipmentPrefabs;
        [SerializeField] private GameObject[] _furniturePrefabs;
        [SerializeField] private GameObject[] _decorationPrefabs;
        [SerializeField] private GameObject[] _vehiclePrefabs;
        
        [Header("Materials and Textures")]
        [SerializeField] private Material[] _floorMaterials;
        [SerializeField] private Material[] _wallMaterials;
        [SerializeField] private Material[] _roofMaterials;
        [SerializeField] private Material[] _groundMaterials;
        
        [Header("Lighting Configuration")]
        [SerializeField] private bool _generateDynamicLighting = true;
        [SerializeField] private bool _generateNaturalLighting = true;
        [SerializeField] private bool _generateGrowLights = true;
        [SerializeField] private int _lightsPerRoom = 4;
        [SerializeField] private float _ambientLightIntensity = 0.3f;
        
        [Header("Weather and Time")]
        [SerializeField] private bool _includeWeatherSystem = true;
        [SerializeField] private bool _includeDayNightCycle = true;
        [SerializeField] private bool _includeSeasonalChanges = true;
        [SerializeField] private WeatherType _initialWeather = WeatherType.Clear;
        [SerializeField] private TimeOfDay _initialTimeOfDay = TimeOfDay.Morning;
        
        // Generation State
        private System.Random _random;
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();
        private List<GameObject> _generatedObjects = new List<GameObject>();
        private Dictionary<string, Transform> _systemContainers = new Dictionary<string, Transform>();
        private TerrainGenerator _terrainGenerator;
        private BuildingGenerator _buildingGenerator;
        private VegetationGenerator _vegetationGenerator;
        
        // Scene Containers
        private Transform _buildingsContainer;
        private Transform _vegetationContainer;
        private Transform _equipmentContainer;
        private Transform _lightingContainer;
        private Transform _environmentalContainer;
        private Transform _decorationContainer;
        
        // Properties
        public FacilitySceneType CurrentFacilitySceneType => _sceneType;
        public List<RoomLayout> GeneratedRooms => _generatedRooms;
        public bool IsGenerating { get; private set; }
        
        // Events
        public System.Action<ProceduralSceneGenerator> OnGenerationStarted;
        public System.Action<ProceduralSceneGenerator> OnGenerationCompleted;
        public System.Action<ProceduralSceneGenerator, float> OnGenerationProgress;
        
        private void Awake()
        {
            InitializeGenerator();
        }
        
        private void Start()
        {
            if (_generateRandomSeed)
            {
                _generationSeed = UnityEngine.Random.Range(1, 999999);
            }
            
            StartCoroutine(GenerateScene());
        }
        
        #region Initialization
        
        private void InitializeGenerator()
        {
            _random = new System.Random(_generationSeed);
            
            CreateSystemContainers();
            InitializeSubGenerators();
        }
        
        private void CreateSystemContainers()
        {
            _buildingsContainer = CreateContainer("Buildings");
            _vegetationContainer = CreateContainer("Vegetation");
            _equipmentContainer = CreateContainer("Equipment");
            _lightingContainer = CreateContainer("Lighting");
            _environmentalContainer = CreateContainer("Environmental Systems");
            _decorationContainer = CreateContainer("Decorations");
            
            _systemContainers["Buildings"] = _buildingsContainer;
            _systemContainers["Vegetation"] = _vegetationContainer;
            _systemContainers["Equipment"] = _equipmentContainer;
            _systemContainers["Lighting"] = _lightingContainer;
            _systemContainers["Environmental"] = _environmentalContainer;
            _systemContainers["Decorations"] = _decorationContainer;
        }
        
        private Transform CreateContainer(string name)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(transform);
            return container.transform;
        }
        
        private void InitializeSubGenerators()
        {
            _terrainGenerator = gameObject.AddComponent<TerrainGenerator>();
            _buildingGenerator = gameObject.AddComponent<BuildingGenerator>();
            _vegetationGenerator = gameObject.AddComponent<VegetationGenerator>();
            
            // Configure sub-generators
            _terrainGenerator.Initialize(_random, _facilitySize);
            _buildingGenerator.Initialize(_random, _systemContainers);
            _vegetationGenerator.Initialize(_random, _vegetationContainer);
        }
        
        #endregion
        
        #region Scene Generation
        
        public IEnumerator GenerateScene()
        {
            IsGenerating = true;
            OnGenerationStarted?.Invoke(this);
            
            Debug.Log($"Starting scene generation - Type: {_sceneType}, Seed: {_generationSeed}");
            
            // Clear existing content
            ClearExistingContent();
            
            // Generate in phases
            yield return StartCoroutine(GeneratePhase1_Terrain());
            UpdateProgress(0.2f);
            
            yield return StartCoroutine(GeneratePhase2_Buildings());
            UpdateProgress(0.4f);
            
            yield return StartCoroutine(GeneratePhase3_Infrastructure());
            UpdateProgress(0.6f);
            
            yield return StartCoroutine(GeneratePhase4_Vegetation());
            UpdateProgress(0.75f);
            
            yield return StartCoroutine(GeneratePhase5_Equipment());
            UpdateProgress(0.85f);
            
            yield return StartCoroutine(GeneratePhase6_Lighting());
            UpdateProgress(0.95f);
            
            yield return StartCoroutine(GeneratePhase7_Details());
            UpdateProgress(1f);
            
            // Final optimizations
            yield return StartCoroutine(OptimizeGeneration());
            
            IsGenerating = false;
            OnGenerationCompleted?.Invoke(this);
            
            Debug.Log($"Scene generation completed - Generated {_generatedObjects.Count} objects");
        }
        
        private void ClearExistingContent()
        {
            foreach (var obj in _generatedObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            _generatedObjects.Clear();
            _generatedRooms.Clear();
        }
        
        private void UpdateProgress(float progress)
        {
            OnGenerationProgress?.Invoke(this, progress);
        }
        
        #endregion
        
        #region Phase 1: Terrain Generation
        
        private IEnumerator GeneratePhase1_Terrain()
        {
            Debug.Log("Phase 1: Generating terrain...");
            
            switch (_sceneType)
            {
                case FacilitySceneType.IndoorFacility:
                    yield return StartCoroutine(GenerateIndoorTerrain());
                    break;
                    
                case FacilitySceneType.OutdoorFarm:
                    yield return StartCoroutine(GenerateOutdoorTerrain());
                    break;
                    
                case FacilitySceneType.Greenhouse:
                    yield return StartCoroutine(GenerateGreenhouseTerrain());
                    break;
                    
                case FacilitySceneType.MixedFacility:
                    yield return StartCoroutine(GenerateMixedTerrain());
                    break;
                    
                case FacilitySceneType.UrbanRooftop:
                    yield return StartCoroutine(GenerateUrbanTerrain());
                    break;
            }
        }
        
        private IEnumerator GenerateIndoorTerrain()
        {
            // Generate base floor
            GameObject floor = CreateFloor(_facilitySize);
            _generatedObjects.Add(floor);
            
            // Generate surrounding walls if needed
            if (_sceneType == FacilitySceneType.IndoorFacility)
            {
                yield return StartCoroutine(GenerateExteriorWalls());
            }
            
            yield return null;
        }
        
        private IEnumerator GenerateOutdoorTerrain()
        {
            // Generate terrain with height variations
            var terrain = _terrainGenerator.GenerateOutdoorTerrain(_facilitySize, _groundMaterials);
            _generatedObjects.Add(terrain);
            
            // Add natural features
            yield return StartCoroutine(GenerateNaturalFeatures());
            
            yield return null;
        }
        
        private IEnumerator GenerateGreenhouseTerrain()
        {
            // Generate raised growing beds
            yield return StartCoroutine(GenerateGrowingBeds());
            
            // Generate greenhouse structure
            var greenhouse = _buildingGenerator.GenerateGreenhouse(_facilitySize);
            _generatedObjects.Add(greenhouse);
            
            yield return null;
        }
        
        private IEnumerator GenerateMixedTerrain()
        {
            // Combine indoor and outdoor elements
            yield return StartCoroutine(GenerateIndoorTerrain());
            yield return StartCoroutine(GenerateOutdoorArea());
        }
        
        private IEnumerator GenerateUrbanTerrain()
        {
            // Generate rooftop base
            GameObject rooftop = CreateRooftop(_facilitySize);
            _generatedObjects.Add(rooftop);
            
            // Add urban context
            yield return StartCoroutine(GenerateUrbanContext());
        }
        
        private GameObject CreateFloor(Vector2Int size)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Facility Floor";
            floor.transform.SetParent(_buildingsContainer);
            floor.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
            floor.transform.position = Vector3.zero;
            
            // Apply material
            if (_floorMaterials.Length > 0)
            {
                var renderer = floor.GetComponent<Renderer>();
                renderer.material = _floorMaterials[_random.Next(_floorMaterials.Length)];
            }
            
            return floor;
        }
        
        private GameObject CreateRooftop(Vector2Int size)
        {
            GameObject rooftop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rooftop.name = "Rooftop Base";
            rooftop.transform.SetParent(_buildingsContainer);
            rooftop.transform.localScale = new Vector3(size.x, 2f, size.y);
            rooftop.transform.position = new Vector3(0f, -1f, 0f);
            
            // Apply material
            if (_roofMaterials.Length > 0)
            {
                var renderer = rooftop.GetComponent<Renderer>();
                renderer.material = _roofMaterials[_random.Next(_roofMaterials.Length)];
            }
            
            return rooftop;
        }
        
        private IEnumerator GenerateExteriorWalls()
        {
            // Generate perimeter walls
            var wallPositions = new Vector3[]
            {
                new Vector3(_facilitySize.x / 2f, 2.5f, 0f), // Right wall
                new Vector3(-_facilitySize.x / 2f, 2.5f, 0f), // Left wall
                new Vector3(0f, 2.5f, _facilitySize.y / 2f), // Front wall
                new Vector3(0f, 2.5f, -_facilitySize.y / 2f), // Back wall
            };
            
            var wallScales = new Vector3[]
            {
                new Vector3(1f, 5f, _facilitySize.y),
                new Vector3(1f, 5f, _facilitySize.y),
                new Vector3(_facilitySize.x, 5f, 1f),
                new Vector3(_facilitySize.x, 5f, 1f),
            };
            
            for (int i = 0; i < wallPositions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Exterior Wall {i + 1}";
                wall.transform.SetParent(_buildingsContainer);
                wall.transform.position = wallPositions[i];
                wall.transform.localScale = wallScales[i];
                
                // Apply material
                if (_wallMaterials.Length > 0)
                {
                    var renderer = wall.GetComponent<Renderer>();
                    renderer.material = _wallMaterials[_random.Next(_wallMaterials.Length)];
                }
                
                _generatedObjects.Add(wall);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator GenerateNaturalFeatures()
        {
            // Generate hills, paths, water features, etc.
            yield return StartCoroutine(GenerateHills());
            yield return StartCoroutine(GeneratePaths());
            yield return StartCoroutine(GenerateWaterFeatures());
        }
        
        private IEnumerator GenerateHills()
        {
            int hillCount = _random.Next(2, 5);
            
            for (int i = 0; i < hillCount; i++)
            {
                Vector3 position = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                var hill = _terrainGenerator.GenerateHill(position, _random.Next(5, 15));
                _generatedObjects.Add(hill);
                
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        private IEnumerator GeneratePaths()
        {
            // Generate main pathways connecting different areas
            var pathPoints = GeneratePathNetwork();
            
            foreach (var path in pathPoints)
            {
                var pathObject = _terrainGenerator.GeneratePath(path);
                _generatedObjects.Add(pathObject);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private List<List<Vector3>> GeneratePathNetwork()
        {
            var pathNetwork = new List<List<Vector3>>();
            
            // Main central path
            var mainPath = new List<Vector3>
            {
                new Vector3(-_facilitySize.x / 2f, 0.1f, 0f),
                new Vector3(0f, 0.1f, 0f),
                new Vector3(_facilitySize.x / 2f, 0.1f, 0f)
            };
            pathNetwork.Add(mainPath);
            
            // Cross path
            var crossPath = new List<Vector3>
            {
                new Vector3(0f, 0.1f, -_facilitySize.y / 2f),
                new Vector3(0f, 0.1f, 0f),
                new Vector3(0f, 0.1f, _facilitySize.y / 2f)
            };
            pathNetwork.Add(crossPath);
            
            return pathNetwork;
        }
        
        private IEnumerator GenerateWaterFeatures()
        {
            if (_random.NextDouble() > 0.7) // 30% chance of water feature
            {
                Vector3 pondPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    -0.2f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                var pond = _terrainGenerator.GeneratePond(pondPosition);
                _generatedObjects.Add(pond);
            }
            
            yield return null;
        }
        
        private IEnumerator GenerateGrowingBeds()
        {
            int bedCount = _random.Next(8, 16);
            
            for (int i = 0; i < bedCount; i++)
            {
                Vector3 bedPosition = new Vector3(
                    ((i % 4) - 1.5f) * 6f,
                    0.3f,
                    ((i / 4) - 1.5f) * 6f
                );
                
                GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bed.name = $"Growing Bed {i + 1}";
                bed.transform.SetParent(_buildingsContainer);
                bed.transform.position = bedPosition;
                bed.transform.localScale = new Vector3(4f, 0.6f, 2f);
                
                // Apply soil material
                if (_groundMaterials.Length > 0)
                {
                    var renderer = bed.GetComponent<Renderer>();
                    renderer.material = _groundMaterials[0]; // Assume first is soil
                }
                
                _generatedObjects.Add(bed);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private IEnumerator GenerateOutdoorArea()
        {
            // Generate outdoor cultivation area adjacent to indoor facility
            Vector3 outdoorOffset = new Vector3(_facilitySize.x / 2f + 10f, 0f, 0f);
            
            yield return StartCoroutine(GenerateOutdoorPlots(outdoorOffset));
            yield return StartCoroutine(GenerateGreenhouse(outdoorOffset));
        }
        
        private IEnumerator GenerateOutdoorPlots(Vector3 offset)
        {
            int plotCount = _random.Next(6, 12);
            
            for (int i = 0; i < plotCount; i++)
            {
                Vector3 plotPosition = offset + new Vector3(
                    ((i % 3) - 1f) * 8f,
                    0.2f,
                    ((i / 3) - 1.5f) * 6f
                );
                
                GameObject plot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plot.name = $"Outdoor Plot {i + 1}";
                plot.transform.SetParent(_buildingsContainer);
                plot.transform.position = plotPosition;
                plot.transform.localScale = new Vector3(6f, 0.4f, 4f);
                
                _generatedObjects.Add(plot);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private IEnumerator GenerateGreenhouse(Vector3 offset)
        {
            if (_includeGreenhouse)
            {
                var greenhouse = _buildingGenerator.GenerateGreenhouse(new Vector2Int(20, 15));
                greenhouse.transform.position = offset + new Vector3(0f, 0f, 20f);
                _generatedObjects.Add(greenhouse);
            }
            
            yield return null;
        }
        
        private IEnumerator GenerateUrbanContext()
        {
            // Generate surrounding city buildings (simplified)
            yield return StartCoroutine(GenerateSurroundingBuildings());
            
            // Add urban elements
            yield return StartCoroutine(GenerateUrbanElements());
        }
        
        private IEnumerator GenerateSurroundingBuildings()
        {
            int buildingCount = _random.Next(5, 10);
            
            for (int i = 0; i < buildingCount; i++)
            {
                Vector3 buildingPosition = new Vector3(
                    _random.Next(-80, 80),
                    _random.Next(10, 30),
                    _random.Next(-80, 80)
                );
                
                // Ensure buildings are outside our facility area
                if (Vector3.Distance(buildingPosition, Vector3.zero) < _facilitySize.magnitude / 2f + 20f)
                    continue;
                
                GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Surrounding Building {i + 1}";
                building.transform.position = buildingPosition;
                building.transform.localScale = new Vector3(
                    _random.Next(8, 20),
                    _random.Next(20, 60),
                    _random.Next(8, 20)
                );
                
                _generatedObjects.Add(building);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator GenerateUrbanElements()
        {
            // Add HVAC units, pipes, etc. typical of rooftop environments
            yield return StartCoroutine(GenerateRooftopHVAC());
            yield return StartCoroutine(GenerateUtilityInfrastructure());
        }
        
        private IEnumerator GenerateRooftopHVAC()
        {
            int hvacCount = _random.Next(3, 6);
            
            for (int i = 0; i < hvacCount; i++)
            {
                Vector3 hvacPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    2f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject hvacUnit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hvacUnit.name = $"HVAC Unit {i + 1}";
                hvacUnit.transform.position = hvacPosition;
                hvacUnit.transform.localScale = new Vector3(3f, 2f, 2f);
                
                // Add HVAC controller component
                hvacUnit.AddComponent<HVACController>();
                
                _generatedObjects.Add(hvacUnit);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private IEnumerator GenerateUtilityInfrastructure()
        {
            // Generate electrical panels, water access, etc.
            yield return StartCoroutine(GenerateElectricalPanels());
            yield return StartCoroutine(GenerateWaterConnections());
        }
        
        private IEnumerator GenerateElectricalPanels()
        {
            int panelCount = _random.Next(2, 4);
            
            for (int i = 0; i < panelCount; i++)
            {
                Vector3 panelPosition = new Vector3(
                    _facilitySize.x / 2f - 2f,
                    1.5f,
                    ((i - panelCount / 2f) * 4f)
                );
                
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = $"Electrical Panel {i + 1}";
                panel.transform.position = panelPosition;
                panel.transform.localScale = new Vector3(0.3f, 1f, 0.6f);
                
                _generatedObjects.Add(panel);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator GenerateWaterConnections()
        {
            // Generate water access points
            Vector3 waterConnection = new Vector3(-_facilitySize.x / 2f + 2f, 0.5f, 0f);
            
            GameObject waterAccess = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waterAccess.name = "Water Connection";
            waterAccess.transform.position = waterConnection;
            waterAccess.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            _generatedObjects.Add(waterAccess);
            yield return null;
        }
        
        #endregion
        
        #region Phase 2: Building Generation
        
        private IEnumerator GeneratePhase2_Buildings()
        {
            Debug.Log("Phase 2: Generating buildings and rooms...");
            
            yield return StartCoroutine(GenerateRoomLayout());
            yield return StartCoroutine(GenerateRoomStructures());
            yield return StartCoroutine(GenerateSpecialtyBuildings());
        }
        
        private IEnumerator GenerateRoomLayout()
        {
            // Generate room placement using space partitioning
            var roomPlacements = _buildingGenerator.GenerateRoomPlacements(
                _facilitySize, 
                _numberOfRooms, 
                _roomMinSize, 
                _roomMaxSize
            );
            
            foreach (var placement in roomPlacements)
            {
                var roomLayout = new RoomLayout
                {
                    RoomId = Guid.NewGuid().ToString(),
                    RoomName = GenerateRoomName(placement.RoomType),
                    RoomType = placement.RoomType.ToString(),
                    Position = placement.Position,
                    Dimensions = placement.Dimensions
                };
                
                _generatedRooms.Add(roomLayout);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private string GenerateRoomName(FacilityRoomType roomType)
        {
            var roomNumbers = _generatedRooms.Count(r => r.RoomType == roomType.ToString()) + 1;
            return $"{roomType} {roomNumbers:D2}";
        }
        
        private IEnumerator GenerateRoomStructures()
        {
            foreach (var room in _generatedRooms)
            {
                yield return StartCoroutine(GenerateRoomStructure(room));
            }
        }
        
        private IEnumerator GenerateRoomStructure(RoomLayout room)
        {
            // Create room GameObject
            GameObject roomGO = new GameObject(room.RoomName);
            roomGO.transform.SetParent(_buildingsContainer);
            roomGO.transform.position = room.Position;
            
            // Add room walls
            yield return StartCoroutine(GenerateRoomWalls(roomGO, room));
            
            // Add doors and windows
            yield return StartCoroutine(GenerateDoorsAndWindows(roomGO, room));
            
            // Add grow room controller if applicable
            if (IsGrowRoom(room.RoomType))
            {
                var growRoomController = roomGO.AddComponent<AdvancedGrowRoomController>();
                // Configure the controller based on room specifications
            }
            
            _generatedObjects.Add(roomGO);
        }
        
        private bool IsGrowRoom(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Vegetative") || 
                   roomType.Contains("Flowering") || roomType.Contains("Nursery");
        }
        
        private IEnumerator GenerateRoomWalls(GameObject roomGO, RoomLayout room)
        {
            // Generate 4 walls for the room
            var wallPositions = new Vector3[]
            {
                new Vector3(room.Dimensions.x / 2f, 1.5f, 0f),
                new Vector3(-room.Dimensions.x / 2f, 1.5f, 0f),
                new Vector3(0f, 1.5f, room.Dimensions.z / 2f),
                new Vector3(0f, 1.5f, -room.Dimensions.z / 2f),
            };
            
            var wallScales = new Vector3[]
            {
                new Vector3(0.2f, 3f, room.Dimensions.z),
                new Vector3(0.2f, 3f, room.Dimensions.z),
                new Vector3(room.Dimensions.x, 3f, 0.2f),
                new Vector3(room.Dimensions.x, 3f, 0.2f),
            };
            
            for (int i = 0; i < wallPositions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall {i + 1}";
                wall.transform.SetParent(roomGO.transform);
                wall.transform.localPosition = wallPositions[i];
                wall.transform.localScale = wallScales[i];
                
                // Apply wall material
                if (_wallMaterials.Length > 0)
                {
                    var renderer = wall.GetComponent<Renderer>();
                    renderer.material = _wallMaterials[_random.Next(_wallMaterials.Length)];
                }
                
                yield return new WaitForSeconds(0.005f);
            }
        }
        
        private IEnumerator GenerateDoorsAndWindows(GameObject roomGO, RoomLayout room)
        {
            // Generate at least one door per room
            yield return StartCoroutine(GenerateDoor(roomGO, room));
            
            // Generate windows based on room type
            if (NeedsWindows(room.RoomType))
            {
                yield return StartCoroutine(GenerateWindows(roomGO, room));
            }
        }
        
        private IEnumerator GenerateDoor(GameObject roomGO, RoomLayout room)
        {
            // Place door on one of the walls
            Vector3 doorPosition = new Vector3(0f, 0f, -room.Dimensions.z / 2f);
            
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(roomGO.transform);
            door.transform.localPosition = doorPosition;
            door.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
            
            // Add door component if available
            var doorComponent = door.AddComponent<DoorController>();
            
            yield return null;
        }
        
        private bool NeedsWindows(string roomType)
        {
            // Office spaces and some grow rooms need windows
            return roomType.Contains("Office") || roomType.Contains("Laboratory") || 
                   roomType.Contains("Break") || _random.NextDouble() > 0.6;
        }
        
        private IEnumerator GenerateWindows(GameObject roomGO, RoomLayout room)
        {
            int windowCount = _random.Next(1, 4);
            
            for (int i = 0; i < windowCount; i++)
            {
                Vector3 windowPosition = new Vector3(
                    ((i - windowCount / 2f) * 2f),
                    1.5f,
                    room.Dimensions.z / 2f
                );
                
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = $"Window {i + 1}";
                window.transform.SetParent(roomGO.transform);
                window.transform.localPosition = windowPosition;
                window.transform.localScale = new Vector3(1f, 1f, 0.05f);
                
                // Make window transparent
                var renderer = window.GetComponent<Renderer>();
                var windowMaterial = new Material(Shader.Find("Standard"));
                windowMaterial.color = new Color(0.8f, 0.9f, 1f, 0.3f);
                windowMaterial.SetFloat("_Mode", 3); // Transparent mode
                renderer.material = windowMaterial;
                
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator GenerateSpecialtyBuildings()
        {
            if (_includeProcessingArea)
            {
                yield return StartCoroutine(GenerateProcessingFacility());
            }
            
            if (_includeStorageArea)
            {
                yield return StartCoroutine(GenerateStorageFacility());
            }
            
            if (_includeLaboratory)
            {
                yield return StartCoroutine(GenerateLaboratory());
            }
            
            if (_includeOfficeSpace)
            {
                yield return StartCoroutine(GenerateOfficeBuilding());
            }
        }
        
        private IEnumerator GenerateProcessingFacility()
        {
            Vector3 processingPosition = new Vector3(_facilitySize.x / 3f, 0f, _facilitySize.y / 3f);
            
            var processingBuilding = _buildingGenerator.GenerateProcessingFacility(processingPosition);
            _generatedObjects.Add(processingBuilding);
            
            yield return null;
        }
        
        private IEnumerator GenerateStorageFacility()
        {
            Vector3 storagePosition = new Vector3(-_facilitySize.x / 3f, 0f, _facilitySize.y / 3f);
            
            var storageBuilding = _buildingGenerator.GenerateStorageFacility(storagePosition);
            _generatedObjects.Add(storageBuilding);
            
            yield return null;
        }
        
        private IEnumerator GenerateLaboratory()
        {
            Vector3 labPosition = new Vector3(_facilitySize.x / 3f, 0f, -_facilitySize.y / 3f);
            
            var laboratory = _buildingGenerator.GenerateLaboratory(labPosition);
            _generatedObjects.Add(laboratory);
            
            yield return null;
        }
        
        private IEnumerator GenerateOfficeBuilding()
        {
            Vector3 officePosition = new Vector3(-_facilitySize.x / 3f, 0f, -_facilitySize.y / 3f);
            
            var officeBuilding = _buildingGenerator.GenerateOfficeBuilding(officePosition);
            _generatedObjects.Add(officeBuilding);
            
            yield return null;
        }
        
        #endregion
        
        #region Phase 3: Infrastructure
        
        private IEnumerator GeneratePhase3_Infrastructure()
        {
            Debug.Log("Phase 3: Generating infrastructure...");
            
            if (_generateElectricalSystems)
            {
                yield return StartCoroutine(GenerateElectricalInfrastructure());
            }
            
            if (_generatePlumbingSystems)
            {
                yield return StartCoroutine(GeneratePlumbingInfrastructure());
            }
            
            if (_generateHVACSystems)
            {
                yield return StartCoroutine(GenerateHVACInfrastructure());
            }
            
            if (_generateSecuritySystems)
            {
                yield return StartCoroutine(GenerateSecurityInfrastructure());
            }
            
            if (_generateNetworkInfrastructure)
            {
                yield return StartCoroutine(GenerateNetworkInfrastructure());
            }
        }
        
        private IEnumerator GenerateElectricalInfrastructure()
        {
            // Generate main electrical panel
            var mainPanel = CreateElectricalPanel("Main Panel", Vector3.zero);
            _generatedObjects.Add(mainPanel);
            
            // Generate sub-panels for each room
            foreach (var room in _generatedRooms)
            {
                if (NeedsElectricalPanel(room.RoomType))
                {
                    var subPanel = CreateElectricalPanel($"{room.RoomName} Panel", room.Position);
                    _generatedObjects.Add(subPanel);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        private GameObject CreateElectricalPanel(string name, Vector3 position)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(_environmentalContainer);
            panel.transform.position = position + new Vector3(0f, 1.5f, 0f);
            panel.transform.localScale = new Vector3(0.5f, 0.8f, 0.2f);
            
            return panel;
        }
        
        private bool NeedsElectricalPanel(string roomType)
        {
            // High-power rooms need dedicated panels
            return roomType.Contains("Grow") || roomType.Contains("Processing") || 
                   roomType.Contains("HVAC") || roomType.Contains("Server");
        }
        
        private IEnumerator GeneratePlumbingInfrastructure()
        {
            // Generate main water line
            yield return StartCoroutine(GenerateMainWaterLine());
            
            // Generate irrigation systems for grow rooms
            yield return StartCoroutine(GenerateIrrigationSystems());
            
            // Generate drainage systems
            yield return StartCoroutine(GenerateDrainageSystems());
        }
        
        private IEnumerator GenerateMainWaterLine()
        {
            // Create visual representation of main water line
            GameObject waterLine = new GameObject("Main Water Line");
            waterLine.transform.SetParent(_environmentalContainer);
            
            // Add pipe segments
            for (int i = 0; i < 10; i++)
            {
                GameObject pipeSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipeSegment.name = $"Pipe Segment {i + 1}";
                pipeSegment.transform.SetParent(waterLine.transform);
                pipeSegment.transform.position = new Vector3(i * 5f - 22.5f, -0.5f, 0f);
                pipeSegment.transform.localScale = new Vector3(0.2f, 2.5f, 0.2f);
                pipeSegment.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                
                yield return new WaitForSeconds(0.005f);
            }
            
            _generatedObjects.Add(waterLine);
        }
        
        private IEnumerator GenerateIrrigationSystems()
        {
            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomIrrigation(room));
                }
            }
        }
        
        private IEnumerator GenerateRoomIrrigation(RoomLayout room)
        {
            GameObject irrigationSystem = new GameObject($"{room.RoomName} Irrigation");
            irrigationSystem.transform.SetParent(_environmentalContainer);
            irrigationSystem.transform.position = room.Position;
            
            // Add irrigation controller
            var irrigationController = irrigationSystem.AddComponent<IrrigationController>();
            
            // Generate irrigation lines
            int lineCount = Mathf.RoundToInt(room.Dimensions.x / 3f);
            
            for (int i = 0; i < lineCount; i++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                line.name = $"Irrigation Line {i + 1}";
                line.transform.SetParent(irrigationSystem.transform);
                line.transform.localPosition = new Vector3((i - lineCount / 2f) * 3f, 0.1f, 0f);
                line.transform.localScale = new Vector3(0.1f, room.Dimensions.z / 2f, 0.1f);
                line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                
                yield return new WaitForSeconds(0.01f);
            }
            
            _generatedObjects.Add(irrigationSystem);
        }
        
        private IEnumerator GenerateDrainageSystems()
        {
            // Generate floor drains for each room
            foreach (var room in _generatedRooms)
            {
                if (NeedsDrainage(room.RoomType))
                {
                    GameObject drain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    drain.name = $"{room.RoomName} Drain";
                    drain.transform.SetParent(_environmentalContainer);
                    drain.transform.position = room.Position + new Vector3(0f, -0.1f, 0f);
                    drain.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
                    
                    _generatedObjects.Add(drain);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        private bool NeedsDrainage(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Processing") || 
                   roomType.Contains("Wash") || roomType.Contains("Bathroom");
        }
        
        private IEnumerator GenerateHVACInfrastructure()
        {
            // Generate main HVAC unit
            GameObject mainHVAC = CreateHVACUnit("Main HVAC", Vector3.zero + new Vector3(0f, 4f, 0f));
            _generatedObjects.Add(mainHVAC);
            
            // Generate HVAC for each room that needs it
            foreach (var room in _generatedRooms)
            {
                if (NeedsHVAC(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomHVAC(room));
                }
            }
        }
        
        private GameObject CreateHVACUnit(string name, Vector3 position)
        {
            GameObject hvacUnit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hvacUnit.name = name;
            hvacUnit.transform.SetParent(_environmentalContainer);
            hvacUnit.transform.position = position;
            hvacUnit.transform.localScale = new Vector3(3f, 1.5f, 2f);
            
            // Add HVAC controller
            hvacUnit.AddComponent<HVACController>();
            
            return hvacUnit;
        }
        
        private bool NeedsHVAC(string roomType)
        {
            // Most rooms need HVAC, but some more than others
            return true; // For simplicity, all rooms get HVAC
        }
        
        private IEnumerator GenerateRoomHVAC(RoomLayout room)
        {
            // Generate HVAC unit for the room
            Vector3 hvacPosition = room.Position + new Vector3(0f, 3.5f, 0f);
            GameObject roomHVAC = CreateHVACUnit($"{room.RoomName} HVAC", hvacPosition);
            _generatedObjects.Add(roomHVAC);
            
            // Generate air ducts
            yield return StartCoroutine(GenerateAirDucts(room));
        }
        
        private IEnumerator GenerateAirDucts(RoomLayout room)
        {
            GameObject ductSystem = new GameObject($"{room.RoomName} Ducts");
            ductSystem.transform.SetParent(_environmentalContainer);
            ductSystem.transform.position = room.Position;
            
            // Generate supply and return ducts
            int ductCount = Mathf.RoundToInt(room.Area / 20f); // One duct per 20 sqm
            
            for (int i = 0; i < ductCount; i++)
            {
                GameObject duct = GameObject.CreatePrimitive(PrimitiveType.Cube);
                duct.name = $"Air Duct {i + 1}";
                duct.transform.SetParent(ductSystem.transform);
                duct.transform.localPosition = new Vector3(
                    ((i % 2) - 0.5f) * room.Dimensions.x * 0.8f,
                    2.8f,
                    ((i / 2) - ductCount / 4f) * room.Dimensions.z * 0.6f
                );
                duct.transform.localScale = new Vector3(0.4f, 0.3f, 2f);
                
                yield return new WaitForSeconds(0.005f);
            }
            
            _generatedObjects.Add(ductSystem);
        }
        
        private IEnumerator GenerateSecurityInfrastructure()
        {
            // Generate security cameras
            yield return StartCoroutine(GenerateSecurityCameras());
            
            // Generate access control systems
            yield return StartCoroutine(GenerateAccessControl());
            
            // Generate alarm systems
            yield return StartCoroutine(GenerateAlarmSystems());
        }
        
        private IEnumerator GenerateSecurityCameras()
        {
            // Place cameras at strategic locations
            var cameraLocations = new Vector3[]
            {
                new Vector3(0f, 3f, 0f), // Central
                new Vector3(_facilitySize.x / 2f, 3f, _facilitySize.y / 2f), // Corner 1
                new Vector3(-_facilitySize.x / 2f, 3f, _facilitySize.y / 2f), // Corner 2
                new Vector3(_facilitySize.x / 2f, 3f, -_facilitySize.y / 2f), // Corner 3
                new Vector3(-_facilitySize.x / 2f, 3f, -_facilitySize.y / 2f), // Corner 4
            };
            
            foreach (var location in cameraLocations)
            {
                GameObject camera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                camera.name = "Security Camera";
                camera.transform.SetParent(_environmentalContainer);
                camera.transform.position = location;
                camera.transform.localScale = new Vector3(0.3f, 0.3f, 0.5f);
                
                // Add security camera component
                camera.AddComponent<SecurityCamera>();
                
                _generatedObjects.Add(camera);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private IEnumerator GenerateAccessControl()
        {
            // Generate card readers at entrances
            foreach (var room in _generatedRooms)
            {
                if (NeedsAccessControl(room.RoomType))
                {
                    GameObject cardReader = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cardReader.name = $"{room.RoomName} Card Reader";
                    cardReader.transform.SetParent(_environmentalContainer);
                    cardReader.transform.position = room.Position + new Vector3(1f, 1.5f, -room.Dimensions.z / 2f);
                    cardReader.transform.localScale = new Vector3(0.2f, 0.3f, 0.1f);
                    
                    _generatedObjects.Add(cardReader);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        private bool NeedsAccessControl(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Processing") || 
                   roomType.Contains("Storage") || roomType.Contains("Laboratory");
        }
        
        private IEnumerator GenerateAlarmSystems()
        {
            // Generate motion sensors and alarm panels
            foreach (var room in _generatedRooms)
            {
                GameObject motionSensor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                motionSensor.name = $"{room.RoomName} Motion Sensor";
                motionSensor.transform.SetParent(_environmentalContainer);
                motionSensor.transform.position = room.Position + new Vector3(0f, 2.5f, 0f);
                motionSensor.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
                _generatedObjects.Add(motionSensor);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator GenerateNetworkInfrastructure()
        {
            // Generate network equipment
            yield return StartCoroutine(GenerateNetworkEquipment());
            
            // Generate wireless access points
            yield return StartCoroutine(GenerateWirelessInfrastructure());
        }
        
        private IEnumerator GenerateNetworkEquipment()
        {
            // Main server rack
            GameObject serverRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            serverRack.name = "Server Rack";
            serverRack.transform.SetParent(_environmentalContainer);
            serverRack.transform.position = new Vector3(0f, 1f, _facilitySize.y / 2f - 2f);
            serverRack.transform.localScale = new Vector3(1f, 2f, 0.8f);
            
            _generatedObjects.Add(serverRack);
            yield return null;
        }
        
        private IEnumerator GenerateWirelessInfrastructure()
        {
            // Place wireless access points
            int apCount = Mathf.RoundToInt(_facilitySize.magnitude / 15f);
            
            for (int i = 0; i < apCount; i++)
            {
                Vector3 apPosition = new Vector3(
                    ((i % 3) - 1f) * _facilitySize.x / 3f,
                    2.8f,
                    ((i / 3) - 1f) * _facilitySize.y / 3f
                );
                
                GameObject accessPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                accessPoint.name = $"Wireless AP {i + 1}";
                accessPoint.transform.SetParent(_environmentalContainer);
                accessPoint.transform.position = apPosition;
                accessPoint.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
                
                _generatedObjects.Add(accessPoint);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        #endregion
        
        #region Phase 4: Vegetation
        
        private IEnumerator GeneratePhase4_Vegetation()
        {
            Debug.Log("Phase 4: Generating vegetation...");
            
            yield return StartCoroutine(GenerateIndoorPlants());
            
            if (_includeWildVegetation)
            {
                yield return StartCoroutine(GenerateOutdoorVegetation());
            }
            
            if (_includeLandscaping)
            {
                yield return StartCoroutine(GenerateLandscaping());
            }
        }
        
        private IEnumerator GenerateIndoorPlants()
        {
            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomPlants(room));
                }
            }
        }
        
        private IEnumerator GenerateRoomPlants(RoomLayout room)
        {
            int plantsToGenerate = Mathf.Min(_plantsPerRoom, Mathf.RoundToInt(room.Area / 2f));
            
            for (int i = 0; i < plantsToGenerate; i++)
            {
                Vector3 plantPosition = GeneratePlantPosition(room, i, plantsToGenerate);
                GameObject plant = GeneratePlant(plantPosition, room);
                _generatedObjects.Add(plant);
                
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private Vector3 GeneratePlantPosition(RoomLayout room, int index, int totalPlants)
        {
            // Generate grid-based positioning with some randomization
            int plantsPerRow = Mathf.CeilToInt(Mathf.Sqrt(totalPlants));
            int row = index / plantsPerRow;
            int col = index % plantsPerRow;
            
            float spacing = Mathf.Min(room.Dimensions.x, room.Dimensions.z) / (plantsPerRow + 1);
            
            Vector3 basePosition = room.Position + new Vector3(
                (col - plantsPerRow / 2f + 0.5f) * spacing,
                0f,
                (row - plantsPerRow / 2f + 0.5f) * spacing
            );
            
            // Add some randomization
            basePosition += new Vector3(
                _random.Next(-50, 50) / 100f,
                0f,
                _random.Next(-50, 50) / 100f
            );
            
            return basePosition;
        }
        
        private GameObject GeneratePlant(Vector3 position, RoomLayout room)
        {
            GameObject plant;
            
            if (_plantPrefabs.Length > 0)
            {
                var prefab = _plantPrefabs[_random.Next(_plantPrefabs.Length)];
                plant = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Generate procedural plant
                plant = GenerateProceduralPlant(position);
            }
            
            plant.transform.SetParent(_vegetationContainer);
            
            // Add interactive plant component if not already present
            if (plant.GetComponent<InteractivePlantComponent>() == null)
            {
                plant.AddComponent<InteractivePlantComponent>();
            }
            
            return plant;
        }
        
        private GameObject GenerateProceduralPlant(Vector3 position)
        {
            GameObject plant = new GameObject("Cannabis Plant");
            plant.transform.position = position;
            
            // Create plant structure
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(plant.transform);
            stem.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            stem.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            
            // Add leaves
            int leafCount = _random.Next(3, 7);
            for (int i = 0; i < leafCount; i++)
            {
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.name = $"Leaf {i + 1}";
                leaf.transform.SetParent(plant.transform);
                
                float angle = (360f / leafCount) * i;
                float height = 0.2f + i * 0.3f;
                
                leaf.transform.localPosition = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * 0.5f,
                    height,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * 0.5f
                );
                leaf.transform.localScale = new Vector3(0.3f, 0.1f, 0.6f);
                
                // Make leaves green
                var renderer = leaf.GetComponent<Renderer>();
                renderer.material.color = new Color(0.2f, 0.8f, 0.2f);
            }
            
            return plant;
        }
        
        private IEnumerator GenerateOutdoorVegetation()
        {
            if (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility)
            {
                yield return StartCoroutine(GenerateWildGrass());
                yield return StartCoroutine(GenerateWildPlants());
                yield return StartCoroutine(GenerateTrees());
            }
        }
        
        private IEnumerator GenerateWildGrass()
        {
            int grassCount = Mathf.RoundToInt(_facilitySize.magnitude * _vegetationDensity);
            
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 grassPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                // Skip if too close to buildings
                if (IsNearBuilding(grassPosition, 3f))
                    continue;
                
                GameObject grass = GenerateGrass(grassPosition);
                _generatedObjects.Add(grass);
                
                if (i % 10 == 0)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        private bool IsNearBuilding(Vector3 position, float minDistance)
        {
            foreach (var room in _generatedRooms)
            {
                if (Vector3.Distance(position, room.Position) < minDistance)
                {
                    return true;
                }
            }
            return false;
        }
        
        private GameObject GenerateGrass(Vector3 position)
        {
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grass.name = "Wild Grass";
            grass.transform.SetParent(_vegetationContainer);
            grass.transform.position = position;
            grass.transform.localScale = new Vector3(0.1f, _random.Next(10, 30) / 100f, 0.1f);
            
            // Random rotation
            grass.transform.rotation = Quaternion.Euler(0f, _random.Next(0, 360), 0f);
            
            // Green color with variation
            var renderer = grass.GetComponent<Renderer>();
            renderer.material.color = new Color(
                _random.Next(10, 30) / 100f,
                _random.Next(60, 90) / 100f,
                _random.Next(10, 30) / 100f
            );
            
            return grass;
        }
        
        private IEnumerator GenerateWildPlants()
        {
            int plantCount = _random.Next(20, 50);
            
            for (int i = 0; i < plantCount; i++)
            {
                Vector3 plantPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                if (IsNearBuilding(plantPosition, 5f))
                    continue;
                
                GameObject wildPlant = GenerateWildPlant(plantPosition);
                _generatedObjects.Add(wildPlant);
                
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private GameObject GenerateWildPlant(Vector3 position)
        {
            GameObject plant = new GameObject("Wild Plant");
            plant.transform.SetParent(_vegetationContainer);
            plant.transform.position = position;
            
            // Generate random bush-like structure
            int branchCount = _random.Next(3, 8);
            for (int i = 0; i < branchCount; i++)
            {
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                branch.name = $"Branch {i + 1}";
                branch.transform.SetParent(plant.transform);
                
                branch.transform.localPosition = new Vector3(
                    _random.Next(-100, 100) / 100f,
                    _random.Next(10, 200) / 100f,
                    _random.Next(-100, 100) / 100f
                );
                branch.transform.localScale = Vector3.one * _random.Next(20, 60) / 100f;
                
                var renderer = branch.GetComponent<Renderer>();
                renderer.material.color = new Color(
                    _random.Next(20, 40) / 100f,
                    _random.Next(50, 80) / 100f,
                    _random.Next(20, 40) / 100f
                );
            }
            
            return plant;
        }
        
        private IEnumerator GenerateTrees()
        {
            int treeCount = _random.Next(5, 15);
            
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 treePosition = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                if (IsNearBuilding(treePosition, 8f))
                    continue;
                
                GameObject tree = GenerateTree(treePosition);
                _generatedObjects.Add(tree);
                
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        private GameObject GenerateTree(Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(_vegetationContainer);
            tree.transform.position = position;
            
            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            trunk.transform.localScale = new Vector3(0.5f, 5f, 0.5f);
            
            var trunkRenderer = trunk.GetComponent<Renderer>();
            trunkRenderer.material.color = new Color(0.4f, 0.2f, 0.1f);
            
            // Foliage
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.SetParent(tree.transform);
            foliage.transform.localPosition = new Vector3(0f, 6f, 0f);
            foliage.transform.localScale = Vector3.one * _random.Next(300, 600) / 100f;
            
            var foliageRenderer = foliage.GetComponent<Renderer>();
            foliageRenderer.material.color = new Color(0.1f, 0.6f, 0.1f);
            
            return tree;
        }
        
        private IEnumerator GenerateLandscaping()
        {
            yield return StartCoroutine(GenerateFlowerBeds());
            yield return StartCoroutine(GenerateHedges());
            yield return StartCoroutine(GenerateDecorations());
        }
        
        private IEnumerator GenerateFlowerBeds()
        {
            int bedCount = _random.Next(3, 8);
            
            for (int i = 0; i < bedCount; i++)
            {
                Vector3 bedPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    0.1f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject flowerBed = GenerateFlowerBed(bedPosition);
                _generatedObjects.Add(flowerBed);
                
                yield return new WaitForSeconds(0.03f);
            }
        }
        
        private GameObject GenerateFlowerBed(Vector3 position)
        {
            GameObject bed = new GameObject("Flower Bed");
            bed.transform.SetParent(_decorationContainer);
            bed.transform.position = position;
            
            // Bed base
            GameObject bedBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bedBase.name = "Bed Base";
            bedBase.transform.SetParent(bed.transform);
            bedBase.transform.localPosition = Vector3.zero;
            bedBase.transform.localScale = new Vector3(3f, 0.2f, 2f);
            
            var baseRenderer = bedBase.GetComponent<Renderer>();
            baseRenderer.material.color = new Color(0.3f, 0.2f, 0.1f);
            
            // Flowers
            int flowerCount = _random.Next(5, 12);
            for (int i = 0; i < flowerCount; i++)
            {
                GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = $"Flower {i + 1}";
                flower.transform.SetParent(bed.transform);
                flower.transform.localPosition = new Vector3(
                    _random.Next(-120, 120) / 100f,
                    0.15f,
                    _random.Next(-80, 80) / 100f
                );
                flower.transform.localScale = Vector3.one * 0.1f;
                
                var flowerRenderer = flower.GetComponent<Renderer>();
                flowerRenderer.material.color = new Color(
                    _random.Next(50, 100) / 100f,
                    _random.Next(20, 80) / 100f,
                    _random.Next(50, 100) / 100f
                );
            }
            
            return bed;
        }
        
        private IEnumerator GenerateHedges()
        {
            // Generate perimeter hedges
            yield return StartCoroutine(GeneratePerimeterHedges());
        }
        
        private IEnumerator GeneratePerimeterHedges()
        {
            var hedgePositions = new Vector3[]
            {
                new Vector3(_facilitySize.x / 2f + 5f, 0.5f, 0f),
                new Vector3(-_facilitySize.x / 2f - 5f, 0.5f, 0f),
            };
            
            foreach (var position in hedgePositions)
            {
                GameObject hedge = GenerateHedge(position);
                _generatedObjects.Add(hedge);
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private GameObject GenerateHedge(Vector3 position)
        {
            GameObject hedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hedge.name = "Hedge";
            hedge.transform.SetParent(_decorationContainer);
            hedge.transform.position = position;
            hedge.transform.localScale = new Vector3(1f, 1f, _facilitySize.y);
            
            var renderer = hedge.GetComponent<Renderer>();
            renderer.material.color = new Color(0.2f, 0.5f, 0.2f);
            
            return hedge;
        }
        
        private IEnumerator GenerateDecorations()
        {
            // Generate benches, signs, etc.
            yield return StartCoroutine(GenerateBenches());
            yield return StartCoroutine(GenerateSigns());
        }
        
        private IEnumerator GenerateBenches()
        {
            int benchCount = _random.Next(2, 5);
            
            for (int i = 0; i < benchCount; i++)
            {
                Vector3 benchPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 4, _facilitySize.x / 4),
                    0.3f,
                    _random.Next(-_facilitySize.y / 4, _facilitySize.y / 4)
                );
                
                GameObject bench = GenerateBench(benchPosition);
                _generatedObjects.Add(bench);
                
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private GameObject GenerateBench(Vector3 position)
        {
            GameObject bench = new GameObject("Bench");
            bench.transform.SetParent(_decorationContainer);
            bench.transform.position = position;
            
            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(bench.transform);
            seat.transform.localPosition = Vector3.zero;
            seat.transform.localScale = new Vector3(2f, 0.1f, 0.5f);
            
            // Back
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back";
            back.transform.SetParent(bench.transform);
            back.transform.localPosition = new Vector3(0f, 0.5f, -0.2f);
            back.transform.localScale = new Vector3(2f, 1f, 0.1f);
            
            return bench;
        }
        
        private IEnumerator GenerateSigns()
        {
            // Generate facility signs
            GameObject facilitySign = GenerateSign("Cannabis Cultivation Facility", Vector3.zero);
            _generatedObjects.Add(facilitySign);
            
            yield return null;
        }
        
        private GameObject GenerateSign(string text, Vector3 position)
        {
            GameObject sign = new GameObject("Facility Sign");
            sign.transform.SetParent(_decorationContainer);
            sign.transform.position = position + new Vector3(0f, 2f, -_facilitySize.y / 2f - 2f);
            
            // Sign post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Sign Post";
            post.transform.SetParent(sign.transform);
            post.transform.localPosition = Vector3.zero;
            post.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
            
            // Sign board
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Sign Board";
            board.transform.SetParent(sign.transform);
            board.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            board.transform.localScale = new Vector3(4f, 1f, 0.1f);
            
            return sign;
        }
        
        #endregion
        
        #region Phase 5: Equipment
        
        private IEnumerator GeneratePhase5_Equipment()
        {
            Debug.Log("Phase 5: Generating equipment...");
            
            yield return StartCoroutine(GenerateGrowEquipment());
            yield return StartCoroutine(GenerateProcessingEquipment());
            yield return StartCoroutine(GenerateMaintenanceEquipment());
            yield return StartCoroutine(GenerateTransportEquipment());
        }
        
        private IEnumerator GenerateGrowEquipment()
        {
            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomEquipment(room));
                }
            }
        }
        
        private IEnumerator GenerateRoomEquipment(RoomLayout room)
        {
            // Generate equipment based on room type and size
            yield return StartCoroutine(GenerateEnvironmentalSensors(room));
            yield return StartCoroutine(GenerateControlPanels(room));
            yield return StartCoroutine(GenerateNutrientSystems(room));
        }
        
        private IEnumerator GenerateEnvironmentalSensors(RoomLayout room)
        {
            // Generate various sensors throughout the room
            var sensorTypes = new List<SensorType>
            {
                SensorType.Temperature,
                SensorType.Humidity,
                SensorType.LightLevel,
                SensorType.CO2Level,
                SensorType.AirFlow
            };
            
            foreach (var sensorType in sensorTypes)
            {
                Vector3 sensorPosition = room.Position + new Vector3(
                    _random.Next(-200, 200) / 100f,
                    1.5f,
                    _random.Next(-200, 200) / 100f
                );
                
                GameObject sensor = GenerateSensor(sensorType, sensorPosition);
                _generatedObjects.Add(sensor);
                
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private GameObject GenerateSensor(SensorType sensorType, Vector3 position)
        {
            GameObject sensor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sensor.name = $"{sensorType} Sensor";
            sensor.transform.SetParent(_equipmentContainer);
            sensor.transform.position = position;
            sensor.transform.localScale = Vector3.one * 0.1f;
            
            // Add environmental sensor component
            var sensorComponent = sensor.AddComponent<EnvironmentalSensor>();
            
            return sensor;
        }
        
        private IEnumerator GenerateControlPanels(RoomLayout room)
        {
            Vector3 panelPosition = room.Position + new Vector3(
                room.Dimensions.x / 2f - 0.5f,
                1.5f,
                0f
            );
            
            GameObject controlPanel = GenerateControlPanel(panelPosition);
            _generatedObjects.Add(controlPanel);
            
            yield return null;
        }
        
        private GameObject GenerateControlPanel(Vector3 position)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Control Panel";
            panel.transform.SetParent(_equipmentContainer);
            panel.transform.position = position;
            panel.transform.localScale = new Vector3(0.1f, 0.8f, 0.6f);
            
            // Add some visual elements
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Screen";
            screen.transform.SetParent(panel.transform);
            screen.transform.localPosition = new Vector3(0.05f, 0.2f, 0f);
            screen.transform.localScale = new Vector3(0.5f, 0.6f, 0.8f);
            
            var screenRenderer = screen.GetComponent<Renderer>();
            screenRenderer.material.color = Color.black;
            
            return panel;
        }
        
        private IEnumerator GenerateNutrientSystems(RoomLayout room)
        {
            Vector3 systemPosition = room.Position + new Vector3(0f, 0f, room.Dimensions.z / 2f - 1f);
            
            GameObject nutrientSystem = GenerateNutrientSystem(systemPosition);
            _generatedObjects.Add(nutrientSystem);
            
            yield return null;
        }
        
        private GameObject GenerateNutrientSystem(Vector3 position)
        {
            GameObject system = new GameObject("Nutrient System");
            system.transform.SetParent(_equipmentContainer);
            system.transform.position = position;
            
            // Reservoir
            GameObject reservoir = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reservoir.name = "Nutrient Reservoir";
            reservoir.transform.SetParent(system.transform);
            reservoir.transform.localPosition = Vector3.zero;
            reservoir.transform.localScale = new Vector3(1f, 0.8f, 1f);
            
            // Pump
            GameObject pump = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pump.name = "Pump";
            pump.transform.SetParent(system.transform);
            pump.transform.localPosition = new Vector3(1.5f, 0f, 0f);
            pump.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            return system;
        }
        
        private IEnumerator GenerateProcessingEquipment()
        {
            if (_includeProcessingArea)
            {
                var processingRoom = _generatedRooms.FirstOrDefault(r => r.RoomType.Contains("Processing"));
                if (processingRoom != null)
                {
                    yield return StartCoroutine(GenerateProcessingMachinery(processingRoom));
                }
            }
        }
        
        private IEnumerator GenerateProcessingMachinery(RoomLayout room)
        {
            // Trimming machines
            GameObject trimmer = GenerateTrimmingMachine(room.Position + new Vector3(-2f, 0f, 0f));
            _generatedObjects.Add(trimmer);
            
            // Drying racks
            GameObject dryingRacks = GenerateDryingRacks(room.Position + new Vector3(2f, 0f, 0f));
            _generatedObjects.Add(dryingRacks);
            
            // Packaging station
            GameObject packagingStation = GeneratePackagingStation(room.Position + new Vector3(0f, 0f, 2f));
            _generatedObjects.Add(packagingStation);
            
            yield return null;
        }
        
        private GameObject GenerateTrimmingMachine(Vector3 position)
        {
            GameObject machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machine.name = "Trimming Machine";
            machine.transform.SetParent(_equipmentContainer);
            machine.transform.position = position;
            machine.transform.localScale = new Vector3(2f, 1f, 1.5f);
            
            return machine;
        }
        
        private GameObject GenerateDryingRacks(Vector3 position)
        {
            GameObject racks = new GameObject("Drying Racks");
            racks.transform.SetParent(_equipmentContainer);
            racks.transform.position = position;
            
            for (int i = 0; i < 4; i++)
            {
                GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rack.name = $"Drying Rack {i + 1}";
                rack.transform.SetParent(racks.transform);
                rack.transform.localPosition = new Vector3(0f, i * 0.5f, 0f);
                rack.transform.localScale = new Vector3(2f, 0.1f, 1f);
            }
            
            return racks;
        }
        
        private GameObject GeneratePackagingStation(Vector3 position)
        {
            GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
            station.name = "Packaging Station";
            station.transform.SetParent(_equipmentContainer);
            station.transform.position = position;
            station.transform.localScale = new Vector3(1.5f, 0.8f, 1f);
            
            return station;
        }
        
        private IEnumerator GenerateMaintenanceEquipment()
        {
            // Generate cleaning equipment, tools, etc.
            yield return StartCoroutine(GenerateCleaningEquipment());
            yield return StartCoroutine(GenerateToolStorage());
        }
        
        private IEnumerator GenerateCleaningEquipment()
        {
            var cleaningPositions = new Vector3[]
            {
                new Vector3(_facilitySize.x / 4f, 0f, _facilitySize.y / 4f),
                new Vector3(-_facilitySize.x / 4f, 0f, -_facilitySize.y / 4f),
            };
            
            foreach (var position in cleaningPositions)
            {
                GameObject cleaningCart = GenerateCleaningCart(position);
                _generatedObjects.Add(cleaningCart);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private GameObject GenerateCleaningCart(Vector3 position)
        {
            GameObject cart = new GameObject("Cleaning Cart");
            cart.transform.SetParent(_equipmentContainer);
            cart.transform.position = position;
            
            // Cart body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Cart Body";
            body.transform.SetParent(cart.transform);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1f, 1.2f);
            
            // Wheels
            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel {i + 1}";
                wheel.transform.SetParent(cart.transform);
                wheel.transform.localPosition = new Vector3(
                    ((i % 2) - 0.5f) * 0.6f,
                    0.1f,
                    ((i / 2) - 0.5f) * 1f
                );
                wheel.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
                wheel.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            
            return cart;
        }
        
        private IEnumerator GenerateToolStorage()
        {
            GameObject toolCabinet = GenerateToolCabinet(new Vector3(0f, 0f, _facilitySize.y / 2f - 1f));
            _generatedObjects.Add(toolCabinet);
            
            yield return null;
        }
        
        private GameObject GenerateToolCabinet(Vector3 position)
        {
            GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "Tool Cabinet";
            cabinet.transform.SetParent(_equipmentContainer);
            cabinet.transform.position = position;
            cabinet.transform.localScale = new Vector3(1f, 2f, 0.5f);
            
            return cabinet;
        }
        
        private IEnumerator GenerateTransportEquipment()
        {
            if (_vehiclePrefabs.Length > 0)
            {
                yield return StartCoroutine(GenerateVehicles());
            }
            
            yield return StartCoroutine(GenerateCartSystem());
        }
        
        private IEnumerator GenerateVehicles()
        {
            if (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility)
            {
                for (int i = 0; i < _random.Next(1, 3); i++)
                {
                    Vector3 vehiclePosition = new Vector3(
                        _facilitySize.x / 2f + 5f + i * 8f,
                        0f,
                        _facilitySize.y / 2f + 5f
                    );
                    
                    var vehiclePrefab = _vehiclePrefabs[_random.Next(_vehiclePrefabs.Length)];
                    GameObject vehicle = Instantiate(vehiclePrefab, vehiclePosition, Quaternion.identity);
                    vehicle.transform.SetParent(_equipmentContainer);
                    
                    _generatedObjects.Add(vehicle);
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        
        private IEnumerator GenerateCartSystem()
        {
            // Generate transport carts for moving plants and equipment
            int cartCount = _random.Next(3, 6);
            
            for (int i = 0; i < cartCount; i++)
            {
                Vector3 cartPosition = new Vector3(
                    ((i % 3) - 1f) * 5f,
                    0f,
                    _facilitySize.y / 2f - 2f
                );
                
                GameObject cart = GenerateTransportCart(cartPosition);
                _generatedObjects.Add(cart);
                
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private GameObject GenerateTransportCart(Vector3 position)
        {
            GameObject cart = new GameObject("Transport Cart");
            cart.transform.SetParent(_equipmentContainer);
            cart.transform.position = position;
            
            // Platform
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(cart.transform);
            platform.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            platform.transform.localScale = new Vector3(1.5f, 0.1f, 2f);
            
            // Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(cart.transform);
            handle.transform.localPosition = new Vector3(0f, 1f, -1f);
            handle.transform.localScale = new Vector3(0.05f, 0.8f, 0.05f);
            
            // Wheels
            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel {i + 1}";
                wheel.transform.SetParent(cart.transform);
                wheel.transform.localPosition = new Vector3(
                    ((i % 2) - 0.5f) * 1.2f,
                    0.1f,
                    ((i / 2) - 0.5f) * 1.6f
                );
                wheel.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
                wheel.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            
            return cart;
        }
        
        #endregion
        
        #region Phase 6: Lighting
        
        private IEnumerator GeneratePhase6_Lighting()
        {
            Debug.Log("Phase 6: Generating lighting systems...");
            
            if (_generateNaturalLighting)
            {
                yield return StartCoroutine(GenerateNaturalLighting());
            }
            
            if (_generateGrowLights)
            {
                yield return StartCoroutine(GenerateGrowLighting());
            }
            
            if (_generateDynamicLighting)
            {
                yield return StartCoroutine(GenerateDynamicLighting());
            }
            
            yield return StartCoroutine(GenerateGeneralLighting());
        }
        
        private IEnumerator GenerateNaturalLighting()
        {
            // Generate sun light
            GameObject sunLight = new GameObject("Sun Light");
            sunLight.transform.SetParent(_lightingContainer);
            sunLight.transform.position = new Vector3(0f, 50f, 0f);
            sunLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            
            Light sun = sunLight.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.8f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            
            _generatedObjects.Add(sunLight);
            
            // Configure ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
            RenderSettings.ambientIntensity = _ambientLightIntensity;
            
            yield return null;
        }
        
        private IEnumerator GenerateGrowLighting()
        {
            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomGrowLights(room));
                }
            }
        }
        
        private IEnumerator GenerateRoomGrowLights(RoomLayout room)
        {
            int lightsInRoom = Mathf.Max(_lightsPerRoom, Mathf.RoundToInt(room.Area / 10f));
            
            for (int i = 0; i < lightsInRoom; i++)
            {
                Vector3 lightPosition = GenerateLightPosition(room, i, lightsInRoom);
                GameObject growLight = GenerateGrowLight(lightPosition, room);
                _generatedObjects.Add(growLight);
                
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private Vector3 GenerateLightPosition(RoomLayout room, int index, int totalLights)
        {
            int lightsPerRow = Mathf.CeilToInt(Mathf.Sqrt(totalLights));
            int row = index / lightsPerRow;
            int col = index % lightsPerRow;
            
            float spacing = Mathf.Min(room.Dimensions.x, room.Dimensions.z) / (lightsPerRow + 1);
            
            return room.Position + new Vector3(
                (col - lightsPerRow / 2f + 0.5f) * spacing,
                3f, // Height above plants
                (row - lightsPerRow / 2f + 0.5f) * spacing
            );
        }
        
        private GameObject GenerateGrowLight(Vector3 position, RoomLayout room)
        {
            GameObject lightGO = new GameObject($"{room.RoomName} Grow Light");
            lightGO.transform.SetParent(_lightingContainer);
            lightGO.transform.position = position;
            
            // Light component
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.9f, 0.8f); // Warm white
            light.intensity = 2f;
            light.range = 8f;
            light.spotAngle = 60f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // Add lighting controller
            var controller = lightGO.AddComponent<LightingController>();
            
            // Visual fixture
            GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fixture.name = "Light Fixture";
            fixture.transform.SetParent(lightGO.transform);
            fixture.transform.localPosition = Vector3.zero;
            fixture.transform.localScale = new Vector3(1f, 0.2f, 1f);
            
            // Remove collider
            Destroy(fixture.GetComponent<Collider>());
            
            return lightGO;
        }
        
        private IEnumerator GenerateDynamicLighting()
        {
            // Generate dynamic lighting effects for special areas
            yield return StartCoroutine(GenerateAccentLighting());
            yield return StartCoroutine(GenerateSecurityLighting());
        }
        
        private IEnumerator GenerateAccentLighting()
        {
            // Generate decorative lighting
            var accentPositions = new Vector3[]
            {
                new Vector3(0f, 1.5f, -_facilitySize.y / 2f), // Entrance
                new Vector3(_facilitySize.x / 2f, 1.5f, 0f), // Side
                new Vector3(-_facilitySize.x / 2f, 1.5f, 0f), // Other side
            };
            
            foreach (var position in accentPositions)
            {
                GameObject accentLight = GenerateAccentLight(position);
                _generatedObjects.Add(accentLight);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private GameObject GenerateAccentLight(Vector3 position)
        {
            GameObject lightGO = new GameObject("Accent Light");
            lightGO.transform.SetParent(_lightingContainer);
            lightGO.transform.position = position;
            
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 1f, 0.8f); // Soft green
            light.intensity = 1f;
            light.range = 5f;
            
            return lightGO;
        }
        
        private IEnumerator GenerateSecurityLighting()
        {
            // Generate perimeter security lighting
            var securityPositions = new Vector3[]
            {
                new Vector3(_facilitySize.x / 2f, 4f, _facilitySize.y / 2f),
                new Vector3(-_facilitySize.x / 2f, 4f, _facilitySize.y / 2f),
                new Vector3(_facilitySize.x / 2f, 4f, -_facilitySize.y / 2f),
                new Vector3(-_facilitySize.x / 2f, 4f, -_facilitySize.y / 2f),
            };
            
            foreach (var position in securityPositions)
            {
                GameObject securityLight = GenerateSecurityLight(position);
                _generatedObjects.Add(securityLight);
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private GameObject GenerateSecurityLight(Vector3 position)
        {
            GameObject lightGO = new GameObject("Security Light");
            lightGO.transform.SetParent(_lightingContainer);
            lightGO.transform.position = position;
            
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = Color.white;
            light.intensity = 3f;
            light.range = 15f;
            light.spotAngle = 90f;
            light.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            
            return lightGO;
        }
        
        private IEnumerator GenerateGeneralLighting()
        {
            // Generate general facility lighting
            foreach (var room in _generatedRooms)
            {
                if (!IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomGeneralLighting(room));
                }
            }
        }
        
        private IEnumerator GenerateRoomGeneralLighting(RoomLayout room)
        {
            Vector3 lightPosition = room.Position + new Vector3(0f, 2.5f, 0f);
            
            GameObject roomLight = GenerateGeneralLight(lightPosition, room.RoomName);
            _generatedObjects.Add(roomLight);
            
            yield return null;
        }
        
        private GameObject GenerateGeneralLight(Vector3 position, string roomName)
        {
            GameObject lightGO = new GameObject($"{roomName} Light");
            lightGO.transform.SetParent(_lightingContainer);
            lightGO.transform.position = position;
            
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 1f, 0.9f); // Warm white
            light.intensity = 1.5f;
            light.range = 6f;
            
            return lightGO;
        }
        
        #endregion
        
        #region Phase 7: Details and Polish
        
        private IEnumerator GeneratePhase7_Details()
        {
            Debug.Log("Phase 7: Adding details and polish...");
            
            yield return StartCoroutine(GenerateEnvironmentalDetails());
            yield return StartCoroutine(GenerateFurniture());
            yield return StartCoroutine(GenerateWeatherEffects());
            yield return StartCoroutine(GenerateParticleEffects());
        }
        
        private IEnumerator GenerateEnvironmentalDetails()
        {
            // Add small environmental details
            yield return StartCoroutine(GenerateRocks());
            yield return StartCoroutine(GenerateDebris());
        }
        
        private IEnumerator GenerateRocks()
        {
            if (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility)
            {
                int rockCount = _random.Next(10, 25);
                
                for (int i = 0; i < rockCount; i++)
                {
                    Vector3 rockPosition = new Vector3(
                        _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                        0f,
                        _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                    );
                    
                    if (IsNearBuilding(rockPosition, 2f))
                        continue;
                    
                    GameObject rock = GenerateRock(rockPosition);
                    _generatedObjects.Add(rock);
                    
                    if (i % 5 == 0)
                        yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        private GameObject GenerateRock(Vector3 position)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(_decorationContainer);
            rock.transform.position = position;
            rock.transform.localScale = Vector3.one * _random.Next(20, 80) / 100f;
            
            // Random rotation
            rock.transform.rotation = Quaternion.Euler(
                _random.Next(0, 360),
                _random.Next(0, 360),
                _random.Next(0, 360)
            );
            
            var renderer = rock.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.4f, 0.4f);
            
            return rock;
        }
        
        private IEnumerator GenerateDebris()
        {
            // Generate small debris items
            int debrisCount = _random.Next(5, 15);
            
            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 debrisPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    0f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject debris = GenerateDebrisItem(debrisPosition);
                _generatedObjects.Add(debris);
                
                yield return new WaitForSeconds(0.005f);
            }
        }
        
        private GameObject GenerateDebrisItem(Vector3 position)
        {
            var primitives = new PrimitiveType[] { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere };
            var primitive = primitives[_random.Next(primitives.Length)];
            
            GameObject debris = GameObject.CreatePrimitive(primitive);
            debris.name = "Debris";
            debris.transform.SetParent(_decorationContainer);
            debris.transform.position = position;
            debris.transform.localScale = Vector3.one * _random.Next(10, 30) / 100f;
            
            var renderer = debris.GetComponent<Renderer>();
            renderer.material.color = new Color(
                _random.Next(20, 60) / 100f,
                _random.Next(20, 60) / 100f,
                _random.Next(20, 60) / 100f
            );
            
            return debris;
        }
        
        private IEnumerator GenerateFurniture()
        {
            yield return StartCoroutine(GenerateOfficeFurniture());
            yield return StartCoroutine(GenerateBreakRoomFurniture());
        }
        
        private IEnumerator GenerateOfficeFurniture()
        {
            var officeRoom = _generatedRooms.FirstOrDefault(r => r.RoomType.Contains("Office"));
            if (officeRoom != null)
            {
                // Generate desk
                GameObject desk = GenerateDesk(officeRoom.Position);
                _generatedObjects.Add(desk);
                
                // Generate chair
                GameObject chair = GenerateChair(officeRoom.Position + new Vector3(0f, 0f, -1f));
                _generatedObjects.Add(chair);
                
                yield return null;
            }
        }
        
        private GameObject GenerateDesk(Vector3 position)
        {
            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(_decorationContainer);
            desk.transform.position = position + new Vector3(0f, 0.4f, 0f);
            desk.transform.localScale = new Vector3(2f, 0.8f, 1f);
            
            return desk;
        }
        
        private GameObject GenerateChair(Vector3 position)
        {
            GameObject chair = new GameObject("Chair");
            chair.transform.SetParent(_decorationContainer);
            chair.transform.position = position;
            
            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(chair.transform);
            seat.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            seat.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            
            // Back
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back";
            back.transform.SetParent(chair.transform);
            back.transform.localPosition = new Vector3(0f, 0.6f, -0.2f);
            back.transform.localScale = new Vector3(0.5f, 0.8f, 0.1f);
            
            return chair;
        }
        
        private IEnumerator GenerateBreakRoomFurniture()
        {
            var breakRoom = _generatedRooms.FirstOrDefault(r => r.RoomType.Contains("Break"));
            if (breakRoom != null)
            {
                // Generate table
                GameObject table = GenerateTable(breakRoom.Position);
                _generatedObjects.Add(table);
                
                // Generate chairs around table
                for (int i = 0; i < 4; i++)
                {
                    Vector3 chairPos = breakRoom.Position + new Vector3(
                        Mathf.Sin(i * 90f * Mathf.Deg2Rad) * 1.5f,
                        0f,
                        Mathf.Cos(i * 90f * Mathf.Deg2Rad) * 1.5f
                    );
                    
                    GameObject chair = GenerateChair(chairPos);
                    _generatedObjects.Add(chair);
                }
                
                yield return null;
            }
        }
        
        private GameObject GenerateTable(Vector3 position)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "Table";
            table.transform.SetParent(_decorationContainer);
            table.transform.position = position + new Vector3(0f, 0.4f, 0f);
            table.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            return table;
        }
        
        private IEnumerator GenerateWeatherEffects()
        {
            if (_includeWeatherSystem && (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility))
            {
                yield return StartCoroutine(CreateWeatherSystem());
            }
        }
        
        private IEnumerator CreateWeatherSystem()
        {
            GameObject weatherSystem = new GameObject("Weather System");
            weatherSystem.transform.SetParent(_environmentalContainer);
            
            // Add weather controller component
            var weatherController = weatherSystem.AddComponent<WeatherController>();
            
            _generatedObjects.Add(weatherSystem);
            yield return null;
        }
        
        private IEnumerator GenerateParticleEffects()
        {
            yield return StartCoroutine(GenerateAmbientParticles());
            yield return StartCoroutine(GenerateEnvironmentalParticles());
        }
        
        private IEnumerator GenerateAmbientParticles()
        {
            // Generate ambient dust particles
            GameObject dustSystem = new GameObject("Ambient Dust");
            dustSystem.transform.SetParent(_environmentalContainer);
            dustSystem.transform.position = new Vector3(0f, 2f, 0f);
            
            var particles = dustSystem.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 10f;
            main.startSpeed = 0.5f;
            main.startSize = 0.01f;
            main.startColor = new Color(1f, 1f, 1f, 0.1f);
            main.maxParticles = 100;
            
            var emission = particles.emission;
            emission.rateOverTime = 5f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_facilitySize.x, 5f, _facilitySize.y);
            
            _generatedObjects.Add(dustSystem);
            yield return null;
        }
        
        private IEnumerator GenerateEnvironmentalParticles()
        {
            if (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility)
            {
                // Generate pollen particles
                GameObject pollenSystem = new GameObject("Pollen Particles");
                pollenSystem.transform.SetParent(_environmentalContainer);
                pollenSystem.transform.position = new Vector3(0f, 3f, 0f);
                
                var particles = pollenSystem.AddComponent<ParticleSystem>();
                var main = particles.main;
                main.startLifetime = 20f;
                main.startSpeed = 1f;
                main.startSize = 0.02f;
                main.startColor = new Color(1f, 1f, 0.5f, 0.3f);
                main.maxParticles = 50;
                
                var emission = particles.emission;
                emission.rateOverTime = 2f;
                
                var velocityOverLifetime = particles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                
                _generatedObjects.Add(pollenSystem);
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Optimization
        
        private IEnumerator OptimizeGeneration()
        {
            Debug.Log("Optimizing generated scene...");
            
            yield return StartCoroutine(OptimizeMeshes());
            yield return StartCoroutine(OptimizeLighting());
            yield return StartCoroutine(OptimizeColliders());
            yield return StartCoroutine(SetupLOD());
        }
        
        private IEnumerator OptimizeMeshes()
        {
            // Combine similar meshes where possible
            var renderers = _generatedObjects
                .Where(obj => obj != null && obj.GetComponent<Renderer>() != null)
                .Select(obj => obj.GetComponent<Renderer>())
                .ToArray();
            
            // Group by material
            var materialGroups = renderers
                .GroupBy(r => r.material.name)
                .Where(g => g.Count() > 10); // Only combine if there are many instances
            
            foreach (var group in materialGroups)
            {
                // Could implement mesh combining here for performance
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator OptimizeLighting()
        {
            // Set up light culling and optimization
            var lights = _generatedObjects
                .Where(obj => obj != null && obj.GetComponent<Light>() != null)
                .Select(obj => obj.GetComponent<Light>())
                .ToArray();
            
            foreach (var light in lights)
            {
                // Optimize light settings
                if (light.type == LightType.Point && light.range > 10f)
                {
                    light.range = 10f; // Limit range for performance
                }
                
                if (light.shadows == LightShadows.Hard)
                {
                    light.shadows = LightShadows.Soft; // Use soft shadows for better quality
                }
                
                yield return new WaitForSeconds(0.001f);
            }
        }
        
        private IEnumerator OptimizeColliders()
        {
            // Optimize colliders for performance
            var colliders = _generatedObjects
                .Where(obj => obj != null && obj.GetComponent<Collider>() != null)
                .Select(obj => obj.GetComponent<Collider>())
                .ToArray();
            
            foreach (var collider in colliders)
            {
                // Convert complex colliders to simpler ones where appropriate
                if (collider is MeshCollider meshCollider && !meshCollider.convex)
                {
                    // For static objects, this is fine
                    meshCollider.convex = false;
                }
                
                yield return new WaitForSeconds(0.001f);
            }
        }
        
        private IEnumerator SetupLOD()
        {
            // Set up Level of Detail for complex objects
            var complexObjects = _generatedObjects
                .Where(obj => obj != null && obj.transform.childCount > 3)
                .ToArray();
            
            foreach (var obj in complexObjects)
            {
                // Could implement LOD groups here
                yield return new WaitForSeconds(0.001f);
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Regenerate the scene with new parameters
        /// </summary>
        public void RegenerateScene()
        {
            if (!IsGenerating)
            {
                StartCoroutine(GenerateScene());
            }
        }
        
        /// <summary>
        /// Change scene type and regenerate
        /// </summary>
        public void ChangeFacilitySceneType(FacilitySceneType newType)
        {
            _sceneType = newType;
            RegenerateScene();
        }
        
        /// <summary>
        /// Get generation statistics
        /// </summary>
        public SceneGenerationStats GetGenerationStats()
        {
            return new SceneGenerationStats
            {
                FacilitySceneType = _sceneType,
                TotalObjects = _generatedObjects.Count,
                RoomCount = _generatedRooms.Count,
                PlantCount = _generatedObjects.Count(obj => obj != null && obj.GetComponent<InteractivePlantComponent>() != null),
                LightCount = _generatedObjects.Count(obj => obj != null && obj.GetComponent<Light>() != null),
                GenerationSeed = _generationSeed,
                FacilitySize = _facilitySize,
                GenerationTime = Time.realtimeSinceStartup
            };
        }
        
        /// <summary>
        /// Save scene configuration for later reuse
        /// </summary>
        public SceneConfiguration SaveConfiguration()
        {
            return new SceneConfiguration
            {
                FacilitySceneType = _sceneType,
                FacilitySize = _facilitySize,
                NumberOfRooms = _numberOfRooms,
                GenerationSeed = _generationSeed,
                VegetationDensity = _vegetationDensity,
                PlantsPerRoom = _plantsPerRoom,
                IncludeOutdoorArea = _includeOutdoorArea,
                IncludeGreenhouse = _includeGreenhouse,
                LightsPerRoom = _lightsPerRoom,
                GenerateDynamicLighting = _generateDynamicLighting
            };
        }
        
        /// <summary>
        /// Load scene configuration
        /// </summary>
        public void LoadConfiguration(SceneConfiguration config)
        {
            _sceneType = config.FacilitySceneType;
            _facilitySize = config.FacilitySize;
            _numberOfRooms = config.NumberOfRooms;
            _generationSeed = config.GenerationSeed;
            _vegetationDensity = config.VegetationDensity;
            _plantsPerRoom = config.PlantsPerRoom;
            _includeOutdoorArea = config.IncludeOutdoorArea;
            _includeGreenhouse = config.IncludeGreenhouse;
            _lightsPerRoom = config.LightsPerRoom;
            _generateDynamicLighting = config.GenerateDynamicLighting;
            
            RegenerateScene();
        }
        
        #endregion
        
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
    
    // Supporting data structures and enums
    [System.Serializable]
    public enum FacilityFacilitySceneType
    {
        IndoorFacility,
        OutdoorFarm,
        Greenhouse,
        MixedFacility,
        UrbanRooftop
    }
    
    [System.Serializable]
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rainy,
        Windy,
        Stormy
    }
    
    [System.Serializable]
    public enum TimeOfDay
    {
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Evening,
        Night
    }
    
    [System.Serializable]
    public class RoomPlacement
    {
        public Vector3 Position;
        public Vector3 Dimensions;
        public FacilityRoomType RoomType;
    }
    
    [System.Serializable]
    public class SceneGenerationStats
    {
        public FacilitySceneType FacilitySceneType;
        public int TotalObjects;
        public int RoomCount;
        public int PlantCount;
        public int LightCount;
        public int GenerationSeed;
        public Vector2Int FacilitySize;
        public float GenerationTime;
    }
    
    [System.Serializable]
    public class SceneConfiguration
    {
        public FacilitySceneType FacilitySceneType;
        public Vector2Int FacilitySize;
        public int NumberOfRooms;
        public int GenerationSeed;
        public float VegetationDensity;
        public int PlantsPerRoom;
        public bool IncludeOutdoorArea;
        public bool IncludeGreenhouse;
        public int LightsPerRoom;
        public bool GenerateDynamicLighting;
    }
    
    // Placeholder components that would be implemented elsewhere
    public class DoorController : MonoBehaviour { }
    public class SecurityCamera : MonoBehaviour { }
    public class WeatherController : MonoBehaviour { }
    
    // Stub classes for missing generators
    public class TerrainGenerator : MonoBehaviour 
    { 
        public void Initialize(System.Random random, Vector2Int facilitySize) { }
        
        // Missing methods that are called in the main generator
        public void GenerateOutdoorTerrain() { }
        public GameObject GenerateOutdoorTerrain(Vector2Int facilitySize, Material[] groundMaterials) { return new GameObject("OutdoorTerrain"); }
        public void GenerateHill() { }
        public GameObject GenerateHill(Vector3 position) { return new GameObject("Hill"); }
        public GameObject GenerateHill(Vector3 position, int size) { return new GameObject("Hill"); }
        public void GeneratePath() { }
        public GameObject GeneratePath(List<Vector3> pathPoints) { return new GameObject("Path"); }
        public GameObject GeneratePond(Vector3 position) { return new GameObject("Pond"); }
    }
    
    public class BuildingGenerator : MonoBehaviour 
    { 
        private System.Random _random;
        
        public void Initialize(System.Random random, Dictionary<string, Transform> systemContainers) 
        { 
            _random = random;
        }
        
        public List<RoomPlacement> GenerateRoomPlacements(Vector2Int facilitySize, int numberOfRooms, float roomMinSize, float roomMaxSize)
        {
            var placements = new List<RoomPlacement>();
            var roomTypes = System.Enum.GetValues(typeof(FacilityRoomType)).Cast<FacilityRoomType>().ToArray();
            
            for (int i = 0; i < numberOfRooms; i++)
            {
                var placement = new RoomPlacement
                {
                    Position = new Vector3(
                        _random.Next(-facilitySize.x / 2, facilitySize.x / 2),
                        0f,
                        _random.Next(-facilitySize.y / 2, facilitySize.y / 2)
                    ),
                    Dimensions = new Vector3(
                        _random.Next((int)roomMinSize, (int)roomMaxSize),
                        3f, // Standard room height
                        _random.Next((int)roomMinSize, (int)roomMaxSize)
                    ),
                    RoomType = roomTypes[_random.Next(roomTypes.Length)]
                };
                placements.Add(placement);
            }
            
            return placements;
        }
        
        public GameObject GenerateProcessingFacility(Vector3 position) { return new GameObject("ProcessingFacility"); }
        public GameObject GenerateStorageFacility(Vector3 position) { return new GameObject("StorageFacility"); }
        public GameObject GenerateLaboratory(Vector3 position) { return new GameObject("Laboratory"); }
        public GameObject GenerateOfficeBuilding(Vector3 position) { return new GameObject("OfficeBuilding"); }
        
        // Missing methods that are called in the main generator
        public void GenerateGreenhouse() { }
        public GameObject GenerateGreenhouse(Vector2Int facilitySize) { return new GameObject("Greenhouse"); }
    }
    
    public class VegetationGenerator : MonoBehaviour 
    { 
        public void Initialize(System.Random random, Transform vegetationContainer) { }
    }
}