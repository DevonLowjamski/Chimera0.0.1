using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Facilities;

namespace ProjectChimera.Systems.SceneGeneration
{
    /// <summary>
    /// Building Generation Service - Dedicated service for building, room, and architectural structure generation
    /// Extracted from ProceduralSceneGenerator to provide focused building generation functionality
    /// Handles room layouts, structures, specialty buildings, and infrastructure systems
    /// </summary>
    public class BuildingGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Building Configuration")]
        [SerializeField] private bool _enableBuildingGeneration = true;
        [SerializeField] private bool _enableRoomGeneration = true;
        [SerializeField] private bool _enableSpecialtyBuildings = true;
        [SerializeField] private bool _enableInfrastructure = true;

        [Header("Room Configuration")]
        [SerializeField] private int _numberOfRooms = 8;
        [SerializeField] private Vector2Int _roomMinSize = new Vector2Int(4, 4);
        [SerializeField] private Vector2Int _roomMaxSize = new Vector2Int(12, 12);

        [Header("Building Materials")]
        [SerializeField] private Material[] _wallMaterials;
        [SerializeField] private Material[] _floorMaterials;
        [SerializeField] private Material[] _ceilingMaterials;

        [Header("Specialty Buildings")]
        [SerializeField] private bool _includeProcessingArea = true;
        [SerializeField] private bool _includeStorageArea = true;
        [SerializeField] private bool _includeLaboratory = true;
        [SerializeField] private bool _includeOfficeSpace = true;
        [SerializeField] private bool _includeGreenhouse = true;

        [Header("Infrastructure Systems")]
        [SerializeField] private bool _generateElectricalSystems = true;
        [SerializeField] private bool _generatePlumbingSystems = true;
        [SerializeField] private bool _generateHVACSystems = true;
        [SerializeField] private bool _generateSecuritySystems = true;
        [SerializeField] private bool _generateNetworkInfrastructure = true;

        // Service state
        private bool _isInitialized = false;
        private System.Random _random;
        private List<GameObject> _generatedBuildingObjects = new List<GameObject>();
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();
        private Transform _containerTransform;

        // Building generators (simplified for extraction)
        private BuildingLayoutGenerator _layoutGenerator;
        private RoomStructureGenerator _structureGenerator;
        private InfrastructureGenerator _infrastructureGenerator;

        // Events for building generation
        public static event System.Action<BuildingGenerationCompleteArgs> OnBuildingGenerationComplete;
        public static event System.Action<string, float> OnBuildingGenerationProgress;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Building Generation Service";
        public IReadOnlyList<GameObject> GeneratedBuildingObjects => _generatedBuildingObjects;
        public IReadOnlyList<RoomLayout> GeneratedRooms => _generatedRooms;
        public Transform ContainerTransform => _containerTransform;

        public void Initialize(int seed = 0, Transform container = null)
        {
            InitializeService(seed, container);
        }

        // ISceneGenerationService interface implementation
        void ISceneGenerationService.Initialize()
        {
            Initialize(0, null);
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void InitializeDataStructures()
        {
            _generatedBuildingObjects = new List<GameObject>();
            _generatedRooms = new List<RoomLayout>();
            _layoutGenerator = new BuildingLayoutGenerator();
            _structureGenerator = new RoomStructureGenerator();
            _infrastructureGenerator = new InfrastructureGenerator();
        }

        public void InitializeService(int seed = 0, Transform container = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("BuildingGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                InitializeGenerators();
                
                _isInitialized = true;
                ChimeraLogger.Log("BuildingGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize BuildingGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedBuildings();
            _generatedBuildingObjects.Clear();
            _generatedRooms.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("BuildingGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_numberOfRooms <= 0)
            {
                ChimeraLogger.LogWarning("Number of rooms must be positive, using default 8", this);
                _numberOfRooms = 8;
            }

            if (_roomMinSize.x > _roomMaxSize.x || _roomMinSize.y > _roomMaxSize.y)
            {
                ChimeraLogger.LogWarning("Room min size greater than max size, adjusting", this);
                _roomMinSize = Vector2Int.Min(_roomMinSize, _roomMaxSize);
            }
        }

        private void InitializeGenerators()
        {
            _layoutGenerator.Initialize(_random);
            _structureGenerator.Initialize(_random, _wallMaterials, _floorMaterials, _ceilingMaterials);
            _infrastructureGenerator.Initialize(_random);
        }

        #endregion

        #region Building Generation Interface

        /// <summary>
        /// Generate buildings based on facility scene type
        /// </summary>
        public IEnumerator GenerateBuildings(FacilitySceneType sceneType, Vector2Int facilitySize, BuildingGenerationSettings settings = null)
        {
            if (!_isInitialized || !_enableBuildingGeneration)
            {
                ChimeraLogger.LogWarning("BuildingGenerationService not initialized or disabled", this);
                yield break;
            }

            OnBuildingGenerationProgress?.Invoke("Starting building generation", 0f);

            switch (sceneType)
            {
                case FacilitySceneType.IndoorFacility:
                    yield return StartCoroutine(GenerateIndoorBuildings(facilitySize));
                    break;
                    
                case FacilitySceneType.OutdoorFarm:
                    yield return StartCoroutine(GenerateOutdoorBuildings(facilitySize));
                    break;
                    
                case FacilitySceneType.Greenhouse:
                    yield return StartCoroutine(GenerateGreenhouseBuildings(facilitySize));
                    break;
                    
                case FacilitySceneType.MixedFacility:
                    yield return StartCoroutine(GenerateMixedBuildings(facilitySize));
                    break;
                    
                case FacilitySceneType.UrbanRooftop:
                    yield return StartCoroutine(GenerateUrbanBuildings(facilitySize));
                    break;
            }

            OnBuildingGenerationComplete?.Invoke(new BuildingGenerationCompleteArgs
            {
                SceneType = sceneType,
                FacilitySize = facilitySize,
                GeneratedObjects = new List<GameObject>(_generatedBuildingObjects),
                GeneratedRooms = new List<RoomLayout>(_generatedRooms),
                Success = true
            });

            ChimeraLogger.Log($"Building generation completed for {sceneType}", this);
        }

        #endregion

        #region Indoor Building Generation

        /// <summary>
        /// Generate indoor facility buildings
        /// </summary>
        private IEnumerator GenerateIndoorBuildings(Vector2Int facilitySize)
        {
            OnBuildingGenerationProgress?.Invoke("Generating room layout", 0.2f);
            
            // Generate room layout
            yield return StartCoroutine(GenerateRoomLayout(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Building room structures", 0.5f);
            
            // Generate room structures
            yield return StartCoroutine(GenerateRoomStructures());
            
            OnBuildingGenerationProgress?.Invoke("Installing infrastructure", 0.8f);
            
            // Generate infrastructure
            if (_enableInfrastructure)
            {
                yield return StartCoroutine(GenerateInfrastructure());
            }
            
            OnBuildingGenerationProgress?.Invoke("Indoor building complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate room layout using space partitioning
        /// </summary>
        private IEnumerator GenerateRoomLayout(Vector2Int facilitySize)
        {
            if (!_enableRoomGeneration) yield break;

            var roomPlacements = _layoutGenerator.GenerateRoomPlacements(
                facilitySize, 
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

        /// <summary>
        /// Generate room structures
        /// </summary>
        private IEnumerator GenerateRoomStructures()
        {
            foreach (var room in _generatedRooms)
            {
                yield return StartCoroutine(GenerateRoomStructure(room));
            }
        }

        /// <summary>
        /// Generate individual room structure
        /// </summary>
        private IEnumerator GenerateRoomStructure(RoomLayout room)
        {
            // Create room GameObject
            GameObject roomGO = new GameObject(room.RoomName);
            roomGO.transform.SetParent(_containerTransform);
            roomGO.transform.position = room.Position;
            
            // Add room walls
            yield return StartCoroutine(GenerateRoomWalls(roomGO, room));
            
            // Add doors and windows
            yield return StartCoroutine(GenerateDoorsAndWindows(roomGO, room));
            
            // Add room-specific components
            yield return StartCoroutine(AddRoomComponents(roomGO, room));
            
            _generatedBuildingObjects.Add(roomGO);
        }

        /// <summary>
        /// Generate room walls
        /// </summary>
        private IEnumerator GenerateRoomWalls(GameObject roomGO, RoomLayout room)
        {
            var walls = _structureGenerator.GenerateWalls(room.Dimensions);
            
            foreach (var wall in walls)
            {
                wall.transform.SetParent(roomGO.transform);
                yield return new WaitForSeconds(0.005f);
            }
        }

        /// <summary>
        /// Generate doors and windows
        /// </summary>
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

        /// <summary>
        /// Generate door for room
        /// </summary>
        private IEnumerator GenerateDoor(GameObject roomGO, RoomLayout room)
        {
            var door = _structureGenerator.GenerateDoor(room.Dimensions);
            door.transform.SetParent(roomGO.transform);
            
            yield return null;
        }

        /// <summary>
        /// Generate windows for room
        /// </summary>
        private IEnumerator GenerateWindows(GameObject roomGO, RoomLayout room)
        {
            int windowCount = _random.Next(1, 4);
            
            for (int i = 0; i < windowCount; i++)
            {
                var window = _structureGenerator.GenerateWindow(i, windowCount, room.Dimensions);
                window.transform.SetParent(roomGO.transform);
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Add room-specific components
        /// </summary>
        private IEnumerator AddRoomComponents(GameObject roomGO, RoomLayout room)
        {
            // Add grow room controller if applicable (placeholder)
            if (IsGrowRoom(room.RoomType))
            {
                // var growRoomController = roomGO.AddComponent<AdvancedGrowRoomController>();
                // Configure the controller based on room specifications
            }
            
            yield return null;
        }

        #endregion

        #region Specialty Buildings

        /// <summary>
        /// Generate specialty buildings
        /// </summary>
        private IEnumerator GenerateSpecialtyBuildings(Vector2Int facilitySize)
        {
            if (!_enableSpecialtyBuildings) yield break;

            if (_includeProcessingArea)
            {
                yield return StartCoroutine(GenerateProcessingFacility(facilitySize));
            }
            
            if (_includeStorageArea)
            {
                yield return StartCoroutine(GenerateStorageFacility(facilitySize));
            }
            
            if (_includeLaboratory)
            {
                yield return StartCoroutine(GenerateLaboratory(facilitySize));
            }
            
            if (_includeOfficeSpace)
            {
                yield return StartCoroutine(GenerateOfficeBuilding(facilitySize));
            }
        }

        /// <summary>
        /// Generate processing facility
        /// </summary>
        private IEnumerator GenerateProcessingFacility(Vector2Int facilitySize)
        {
            Vector3 processingPosition = new Vector3(facilitySize.x / 3f, 0f, facilitySize.y / 3f);
            
            var processingBuilding = _structureGenerator.GenerateSpecialtyBuilding("Processing Facility", processingPosition, new Vector3(20, 8, 15));
            _generatedBuildingObjects.Add(processingBuilding);
            
            yield return null;
        }

        /// <summary>
        /// Generate storage facility
        /// </summary>
        private IEnumerator GenerateStorageFacility(Vector2Int facilitySize)
        {
            Vector3 storagePosition = new Vector3(-facilitySize.x / 3f, 0f, facilitySize.y / 3f);
            
            var storageBuilding = _structureGenerator.GenerateSpecialtyBuilding("Storage Facility", storagePosition, new Vector3(15, 6, 20));
            _generatedBuildingObjects.Add(storageBuilding);
            
            yield return null;
        }

        /// <summary>
        /// Generate laboratory
        /// </summary>
        private IEnumerator GenerateLaboratory(Vector2Int facilitySize)
        {
            Vector3 labPosition = new Vector3(facilitySize.x / 3f, 0f, -facilitySize.y / 3f);
            
            var laboratory = _structureGenerator.GenerateSpecialtyBuilding("Laboratory", labPosition, new Vector3(12, 5, 12));
            _generatedBuildingObjects.Add(laboratory);
            
            yield return null;
        }

        /// <summary>
        /// Generate office building
        /// </summary>
        private IEnumerator GenerateOfficeBuilding(Vector2Int facilitySize)
        {
            Vector3 officePosition = new Vector3(-facilitySize.x / 3f, 0f, -facilitySize.y / 3f);
            
            var officeBuilding = _structureGenerator.GenerateSpecialtyBuilding("Office Building", officePosition, new Vector3(10, 8, 10));
            _generatedBuildingObjects.Add(officeBuilding);
            
            yield return null;
        }

        #endregion

        #region Outdoor Building Generation

        /// <summary>
        /// Generate outdoor facility buildings
        /// </summary>
        private IEnumerator GenerateOutdoorBuildings(Vector2Int facilitySize)
        {
            OnBuildingGenerationProgress?.Invoke("Generating outdoor structures", 0.3f);
            
            // Generate outdoor cultivation structures
            yield return StartCoroutine(GenerateOutdoorStructures(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Adding support buildings", 0.7f);
            
            // Generate support buildings
            yield return StartCoroutine(GenerateSupportBuildings(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Outdoor buildings complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate outdoor cultivation structures
        /// </summary>
        private IEnumerator GenerateOutdoorStructures(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateOutdoorPlots(facilitySize));
            
            if (_includeGreenhouse)
            {
                yield return StartCoroutine(GenerateGreenhouse(facilitySize));
            }
        }

        /// <summary>
        /// Generate outdoor cultivation plots
        /// </summary>
        private IEnumerator GenerateOutdoorPlots(Vector2Int facilitySize)
        {
            int plotCount = _random.Next(6, 12);
            
            for (int i = 0; i < plotCount; i++)
            {
                Vector3 plotPosition = new Vector3(
                    ((i % 3) - 1f) * 8f,
                    0.2f,
                    ((i / 3) - 1.5f) * 6f
                );
                
                GameObject plot = _structureGenerator.GenerateOutdoorPlot(plotPosition, i + 1);
                _generatedBuildingObjects.Add(plot);
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate greenhouse structure
        /// </summary>
        private IEnumerator GenerateGreenhouse(Vector2Int facilitySize)
        {
            Vector3 greenhousePosition = new Vector3(0f, 0f, facilitySize.y / 2f + 10f);
            
            var greenhouse = _structureGenerator.GenerateGreenhouse(greenhousePosition, new Vector2Int(20, 15));
            _generatedBuildingObjects.Add(greenhouse);
            
            yield return null;
        }

        /// <summary>
        /// Generate support buildings
        /// </summary>
        private IEnumerator GenerateSupportBuildings(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateEquipmentShed(facilitySize));
            yield return StartCoroutine(GenerateStorageBarn(facilitySize));
        }

        /// <summary>
        /// Generate equipment shed
        /// </summary>
        private IEnumerator GenerateEquipmentShed(Vector2Int facilitySize)
        {
            Vector3 shedPosition = new Vector3(facilitySize.x / 2f + 5f, 0f, -facilitySize.y / 4f);
            
            var shed = _structureGenerator.GenerateSpecialtyBuilding("Equipment Shed", shedPosition, new Vector3(8, 4, 6));
            _generatedBuildingObjects.Add(shed);
            
            yield return null;
        }

        /// <summary>
        /// Generate storage barn
        /// </summary>
        private IEnumerator GenerateStorageBarn(Vector2Int facilitySize)
        {
            Vector3 barnPosition = new Vector3(-facilitySize.x / 2f - 5f, 0f, -facilitySize.y / 4f);
            
            var barn = _structureGenerator.GenerateSpecialtyBuilding("Storage Barn", barnPosition, new Vector3(12, 6, 8));
            _generatedBuildingObjects.Add(barn);
            
            yield return null;
        }

        #endregion

        #region Mixed and Urban Buildings

        /// <summary>
        /// Generate mixed facility buildings
        /// </summary>
        private IEnumerator GenerateMixedBuildings(Vector2Int facilitySize)
        {
            OnBuildingGenerationProgress?.Invoke("Generating mixed buildings - indoor", 0.3f);
            
            // Combine indoor and outdoor elements
            yield return StartCoroutine(GenerateIndoorBuildings(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Generating mixed buildings - outdoor", 0.7f);
            
            yield return StartCoroutine(GenerateOutdoorArea(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Mixed buildings complete", 1.0f);
        }

        /// <summary>
        /// Generate greenhouse facility buildings
        /// </summary>
        private IEnumerator GenerateGreenhouseBuildings(Vector2Int facilitySize)
        {
            OnBuildingGenerationProgress?.Invoke("Generating greenhouse structures", 0.5f);
            
            // Generate main greenhouse structures
            yield return StartCoroutine(GenerateMainGreenhouses(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Greenhouse buildings complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate main greenhouse structures
        /// </summary>
        private IEnumerator GenerateMainGreenhouses(Vector2Int facilitySize)
        {
            int greenhouseCount = _random.Next(2, 5);
            
            for (int i = 0; i < greenhouseCount; i++)
            {
                Vector3 position = new Vector3(
                    ((i % 2) - 0.5f) * (facilitySize.x / 2f),
                    0f,
                    ((i / 2) - greenhouseCount / 4f) * (facilitySize.y / 2f)
                );
                
                var greenhouse = _structureGenerator.GenerateGreenhouse(position, new Vector2Int(15, 10));
                _generatedBuildingObjects.Add(greenhouse);
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Generate urban rooftop buildings
        /// </summary>
        private IEnumerator GenerateUrbanBuildings(Vector2Int facilitySize)
        {
            OnBuildingGenerationProgress?.Invoke("Generating urban context", 0.3f);
            
            yield return StartCoroutine(GenerateUrbanContext(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Adding rooftop structures", 0.7f);
            
            yield return StartCoroutine(GenerateRooftopStructures(facilitySize));
            
            OnBuildingGenerationProgress?.Invoke("Urban buildings complete", 1.0f);
        }

        /// <summary>
        /// Generate urban context buildings
        /// </summary>
        private IEnumerator GenerateUrbanContext(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateSurroundingBuildings(facilitySize));
            yield return StartCoroutine(GenerateUrbanElements(facilitySize));
        }

        /// <summary>
        /// Generate surrounding urban buildings
        /// </summary>
        private IEnumerator GenerateSurroundingBuildings(Vector2Int facilitySize)
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
                if (Vector3.Distance(buildingPosition, Vector3.zero) < facilitySize.magnitude / 2f + 20f)
                    continue;
                
                var building = _structureGenerator.GenerateSurroundingBuilding(buildingPosition, i + 1);
                _generatedBuildingObjects.Add(building);
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Generate urban elements
        /// </summary>
        private IEnumerator GenerateUrbanElements(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateRooftopHVAC(facilitySize));
            yield return StartCoroutine(GenerateUtilityInfrastructure(facilitySize));
        }

        /// <summary>
        /// Generate rooftop HVAC units
        /// </summary>
        private IEnumerator GenerateRooftopHVAC(Vector2Int facilitySize)
        {
            int hvacCount = _random.Next(3, 6);
            
            for (int i = 0; i < hvacCount; i++)
            {
                Vector3 hvacPosition = new Vector3(
                    _random.Next(-facilitySize.x / 3, facilitySize.x / 3),
                    2f,
                    _random.Next(-facilitySize.y / 3, facilitySize.y / 3)
                );
                
                var hvacUnit = _infrastructureGenerator.GenerateHVACUnit(hvacPosition, i + 1);
                _generatedBuildingObjects.Add(hvacUnit);
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate rooftop structures
        /// </summary>
        private IEnumerator GenerateRooftopStructures(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateRooftopGreenhouses(facilitySize));
            yield return StartCoroutine(GenerateRooftopUtilities(facilitySize));
        }

        /// <summary>
        /// Generate outdoor area for mixed facilities
        /// </summary>
        private IEnumerator GenerateOutdoorArea(Vector2Int facilitySize)
        {
            Vector3 outdoorOffset = new Vector3(facilitySize.x / 2f + 10f, 0f, 0f);
            yield return StartCoroutine(GenerateOutdoorPlots(facilitySize));
            yield return StartCoroutine(GenerateGreenhouse(facilitySize));
        }

        /// <summary>
        /// Generate rooftop greenhouses
        /// </summary>
        private IEnumerator GenerateRooftopGreenhouses(Vector2Int facilitySize)
        {
            int greenhouseCount = _random.Next(1, 3);
            
            for (int i = 0; i < greenhouseCount; i++)
            {
                Vector3 position = new Vector3(
                    ((i - greenhouseCount / 2f) * 20f),
                    1f,
                    0f
                );
                
                var greenhouse = _structureGenerator.GenerateRooftopGreenhouse(position);
                _generatedBuildingObjects.Add(greenhouse);
                yield return new WaitForSeconds(0.05f);
            }
        }

        /// <summary>
        /// Generate rooftop utilities
        /// </summary>
        private IEnumerator GenerateRooftopUtilities(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateElectricalPanels(facilitySize));
            yield return StartCoroutine(GenerateWaterConnections(facilitySize));
        }

        #endregion

        #region Infrastructure Generation

        /// <summary>
        /// Generate infrastructure systems
        /// </summary>
        private IEnumerator GenerateInfrastructure()
        {
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

        /// <summary>
        /// Generate electrical infrastructure
        /// </summary>
        private IEnumerator GenerateElectricalInfrastructure()
        {
            // Generate main electrical panel
            var mainPanel = _infrastructureGenerator.GenerateElectricalPanel("Main Panel", Vector3.zero);
            _generatedBuildingObjects.Add(mainPanel);
            
            // Generate sub-panels for each room
            foreach (var room in _generatedRooms)
            {
                if (NeedsElectricalPanel(room.RoomType))
                {
                    var subPanel = _infrastructureGenerator.GenerateElectricalPanel($"{room.RoomName} Panel", room.Position);
                    _generatedBuildingObjects.Add(subPanel);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

        /// <summary>
        /// Generate plumbing infrastructure
        /// </summary>
        private IEnumerator GeneratePlumbingInfrastructure()
        {
            yield return StartCoroutine(GenerateMainWaterLine());
            yield return StartCoroutine(GenerateIrrigationSystems());
            yield return StartCoroutine(GenerateDrainageSystems());
        }

        /// <summary>
        /// Generate main water line
        /// </summary>
        private IEnumerator GenerateMainWaterLine()
        {
            var waterLine = _infrastructureGenerator.GenerateMainWaterLine();
            _generatedBuildingObjects.AddRange(waterLine);
            yield return null;
        }

        /// <summary>
        /// Generate irrigation systems
        /// </summary>
        private IEnumerator GenerateIrrigationSystems()
        {
            foreach (var room in _generatedRooms.Where(r => IsGrowRoom(r.RoomType)))
            {
                var irrigationSystem = _infrastructureGenerator.GenerateIrrigationSystem(room.Position, room.Dimensions);
                _generatedBuildingObjects.AddRange(irrigationSystem);
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate drainage systems
        /// </summary>
        private IEnumerator GenerateDrainageSystems()
        {
            var drainageSystem = _infrastructureGenerator.GenerateDrainageSystem(_generatedRooms);
            _generatedBuildingObjects.AddRange(drainageSystem);
            yield return null;
        }

        /// <summary>
        /// Generate HVAC infrastructure
        /// </summary>
        private IEnumerator GenerateHVACInfrastructure()
        {
            var hvacSystem = _infrastructureGenerator.GenerateHVACSystem(_generatedRooms);
            _generatedBuildingObjects.AddRange(hvacSystem);
            yield return null;
        }

        /// <summary>
        /// Generate security infrastructure
        /// </summary>
        private IEnumerator GenerateSecurityInfrastructure()
        {
            var securitySystem = _infrastructureGenerator.GenerateSecuritySystem(_generatedRooms);
            _generatedBuildingObjects.AddRange(securitySystem);
            yield return null;
        }

        /// <summary>
        /// Generate network infrastructure
        /// </summary>
        private IEnumerator GenerateNetworkInfrastructure()
        {
            var networkSystem = _infrastructureGenerator.GenerateNetworkInfrastructure(_generatedRooms);
            _generatedBuildingObjects.AddRange(networkSystem);
            yield return null;
        }

        /// <summary>
        /// Generate electrical panels
        /// </summary>
        private IEnumerator GenerateElectricalPanels(Vector2Int facilitySize)
        {
            int panelCount = _random.Next(2, 4);
            
            for (int i = 0; i < panelCount; i++)
            {
                Vector3 panelPosition = new Vector3(
                    facilitySize.x / 2f - 2f,
                    1.5f,
                    ((i - panelCount / 2f) * 4f)
                );
                
                var panel = _infrastructureGenerator.GenerateElectricalPanel($"Electrical Panel {i + 1}", panelPosition);
                _generatedBuildingObjects.Add(panel);
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Generate water connections
        /// </summary>
        private IEnumerator GenerateWaterConnections(Vector2Int facilitySize)
        {
            Vector3 waterConnection = new Vector3(-facilitySize.x / 2f + 2f, 0.5f, 0f);
            
            var waterAccess = _infrastructureGenerator.GenerateWaterConnection(waterConnection);
            _generatedBuildingObjects.Add(waterAccess);
            yield return null;
        }

        /// <summary>
        /// Generate utility infrastructure
        /// </summary>
        private IEnumerator GenerateUtilityInfrastructure(Vector2Int facilitySize)
        {
            yield return StartCoroutine(GenerateElectricalPanels(facilitySize));
            yield return StartCoroutine(GenerateWaterConnections(facilitySize));
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generate room name based on type
        /// </summary>
        private string GenerateRoomName(FacilityRoomType roomType)
        {
            var roomNumbers = _generatedRooms.Count(r => r.RoomType == roomType.ToString()) + 1;
            return $"{roomType} {roomNumbers:D2}";
        }

        /// <summary>
        /// Check if room type is a grow room
        /// </summary>
        private bool IsGrowRoom(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Vegetative") || 
                   roomType.Contains("Flowering") || roomType.Contains("Nursery");
        }

        /// <summary>
        /// Check if room needs windows
        /// </summary>
        private bool NeedsWindows(string roomType)
        {
            return roomType.Contains("Office") || roomType.Contains("Laboratory") || 
                   roomType.Contains("Break") || _random.NextDouble() > 0.6;
        }

        /// <summary>
        /// Check if room needs electrical panel
        /// </summary>
        private bool NeedsElectricalPanel(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Processing") || 
                   roomType.Contains("HVAC") || roomType.Contains("Server");
        }

        /// <summary>
        /// Clear all generated building objects
        /// </summary>
        public void ClearGeneratedBuildings()
        {
            foreach (var obj in _generatedBuildingObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
        }

        /// <summary>
        /// Get building generation statistics
        /// </summary>
        public BuildingGenerationStats GetGenerationStats()
        {
            return new BuildingGenerationStats
            {
                TotalObjectsGenerated = _generatedBuildingObjects.Count,
                TotalRoomsGenerated = _generatedRooms.Count,
                ServiceInitialized = _isInitialized,
                LastGenerationTime = DateTime.Now
            };
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Building layout generator
    /// </summary>
    public class BuildingLayoutGenerator
    {
        private System.Random _random;

        public void Initialize(System.Random random)
        {
            _random = random;
        }

        public List<RoomPlacement> GenerateRoomPlacements(Vector2Int facilitySize, int roomCount, Vector2Int minSize, Vector2Int maxSize)
        {
            var placements = new List<RoomPlacement>();
            var roomTypes = System.Enum.GetValues(typeof(FacilityRoomType)).Cast<FacilityRoomType>().ToArray();
            
            for (int i = 0; i < roomCount; i++)
            {
                var placement = new RoomPlacement
                {
                    RoomType = roomTypes[_random.Next(roomTypes.Length)],
                    Position = new Vector3(
                        _random.Next(-facilitySize.x / 2, facilitySize.x / 2),
                        0f,
                        _random.Next(-facilitySize.y / 2, facilitySize.y / 2)
                    ),
                    Dimensions = new Vector3(
                        _random.Next(minSize.x, maxSize.x + 1),
                        3f,
                        _random.Next(minSize.y, maxSize.y + 1)
                    )
                };
                
                placements.Add(placement);
            }
            
            return placements;
        }
    }

    /// <summary>
    /// Room structure generator
    /// </summary>
    public class RoomStructureGenerator
    {
        private System.Random _random;
        private Material[] _wallMaterials;
        private Material[] _floorMaterials;
        private Material[] _ceilingMaterials;

        public void Initialize(System.Random random, Material[] wallMaterials, Material[] floorMaterials, Material[] ceilingMaterials)
        {
            _random = random;
            _wallMaterials = wallMaterials;
            _floorMaterials = floorMaterials;
            _ceilingMaterials = ceilingMaterials;
        }

        public List<GameObject> GenerateWalls(Vector3 roomDimensions)
        {
            var walls = new List<GameObject>();
            
            var wallPositions = new Vector3[]
            {
                new Vector3(roomDimensions.x / 2f, 1.5f, 0f),
                new Vector3(-roomDimensions.x / 2f, 1.5f, 0f),
                new Vector3(0f, 1.5f, roomDimensions.z / 2f),
                new Vector3(0f, 1.5f, -roomDimensions.z / 2f),
            };
            
            var wallScales = new Vector3[]
            {
                new Vector3(0.2f, 3f, roomDimensions.z),
                new Vector3(0.2f, 3f, roomDimensions.z),
                new Vector3(roomDimensions.x, 3f, 0.2f),
                new Vector3(roomDimensions.x, 3f, 0.2f),
            };
            
            for (int i = 0; i < wallPositions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall {i + 1}";
                wall.transform.localPosition = wallPositions[i];
                wall.transform.localScale = wallScales[i];
                
                // Apply wall material
                if (_wallMaterials != null && _wallMaterials.Length > 0)
                {
                    var renderer = wall.GetComponent<Renderer>();
                    renderer.material = _wallMaterials[_random.Next(_wallMaterials.Length)];
                }
                
                walls.Add(wall);
            }
            
            return walls;
        }

        public GameObject GenerateDoor(Vector3 roomDimensions)
        {
            Vector3 doorPosition = new Vector3(0f, 0f, -roomDimensions.z / 2f);
            
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.localPosition = doorPosition;
            door.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
            
            // Add door component if available (placeholder)
            // var doorComponent = door.AddComponent<DoorController>();
            
            return door;
        }

        public GameObject GenerateWindow(int index, int totalWindows, Vector3 roomDimensions)
        {
            Vector3 windowPosition = new Vector3(
                ((index - totalWindows / 2f) * 2f),
                1.5f,
                roomDimensions.z / 2f
            );
            
            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = $"Window {index + 1}";
            window.transform.localPosition = windowPosition;
            window.transform.localScale = new Vector3(1f, 1f, 0.05f);
            
            // Make window transparent
            var renderer = window.GetComponent<Renderer>();
            var windowMaterial = new Material(Shader.Find("Standard"));
            windowMaterial.color = new Color(0.8f, 0.9f, 1f, 0.3f);
            windowMaterial.SetFloat("_Mode", 3); // Transparent mode
            renderer.material = windowMaterial;
            
            return window;
        }

        public GameObject GenerateSpecialtyBuilding(string name, Vector3 position, Vector3 dimensions)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = name;
            building.transform.position = position;
            building.transform.localScale = dimensions;
            
            // Apply material
            if (_wallMaterials != null && _wallMaterials.Length > 0)
            {
                var renderer = building.GetComponent<Renderer>();
                renderer.material = _wallMaterials[_random.Next(_wallMaterials.Length)];
            }
            
            return building;
        }

        public GameObject GenerateOutdoorPlot(Vector3 position, int plotNumber)
        {
            GameObject plot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plot.name = $"Outdoor Plot {plotNumber}";
            plot.transform.position = position;
            plot.transform.localScale = new Vector3(6f, 0.4f, 4f);
            
            return plot;
        }

        public GameObject GenerateGreenhouse(Vector3 position, Vector2Int size)
        {
            GameObject greenhouse = new GameObject("Greenhouse");
            greenhouse.transform.position = position;
            
            // Create greenhouse frame
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Greenhouse Frame";
            frame.transform.SetParent(greenhouse.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(size.x, 4f, size.y);
            
            // Create transparent walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Greenhouse Walls";
            walls.transform.SetParent(greenhouse.transform);
            walls.transform.localPosition = Vector3.zero;
            walls.transform.localScale = new Vector3(size.x - 0.5f, 3.5f, size.y - 0.5f);
            
            // Make walls transparent
            var renderer = walls.GetComponent<Renderer>();
            var glassMaterial = new Material(Shader.Find("Standard"));
            glassMaterial.color = new Color(0.9f, 0.9f, 1f, 0.2f);
            glassMaterial.SetFloat("_Mode", 3); // Transparent mode
            renderer.material = glassMaterial;
            
            return greenhouse;
        }

        public GameObject GenerateSurroundingBuilding(Vector3 position, int buildingNumber)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Surrounding Building {buildingNumber}";
            building.transform.position = position;
            building.transform.localScale = new Vector3(
                _random.Next(8, 20),
                _random.Next(20, 60),
                _random.Next(8, 20)
            );
            
            return building;
        }

        public GameObject GenerateRooftopGreenhouse(Vector3 position)
        {
            GameObject greenhouse = new GameObject("Rooftop Greenhouse");
            greenhouse.transform.position = position;
            
            // Smaller rooftop greenhouse
            var structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structure.name = "Greenhouse Structure";
            structure.transform.SetParent(greenhouse.transform);
            structure.transform.localPosition = Vector3.zero;
            structure.transform.localScale = new Vector3(10f, 3f, 8f);
            
            return greenhouse;
        }
    }

    /// <summary>
    /// Infrastructure generator for utilities and systems
    /// </summary>
    public class InfrastructureGenerator
    {
        private System.Random _random;

        public void Initialize(System.Random random)
        {
            _random = random;
        }

        public GameObject GenerateElectricalPanel(string name, Vector3 position)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.position = position + new Vector3(0f, 1.5f, 0f);
            panel.transform.localScale = new Vector3(0.5f, 0.8f, 0.2f);
            
            return panel;
        }

        public GameObject GenerateHVACUnit(Vector3 position, int unitNumber)
        {
            GameObject hvacUnit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hvacUnit.name = $"HVAC Unit {unitNumber}";
            hvacUnit.transform.position = position;
            hvacUnit.transform.localScale = new Vector3(3f, 2f, 2f);
            
            // Add HVAC controller component (placeholder)
            // hvacUnit.AddComponent<HVACController>();
            
            return hvacUnit;
        }

        public GameObject GenerateWaterConnection(Vector3 position)
        {
            GameObject waterAccess = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waterAccess.name = "Water Connection";
            waterAccess.transform.position = position;
            waterAccess.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            return waterAccess;
        }

        public List<GameObject> GenerateMainWaterLine()
        {
            var waterLine = new List<GameObject>();
            
            // Create main water line components
            var mainLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainLine.name = "Main Water Line";
            mainLine.transform.position = new Vector3(0f, -0.5f, 0f);
            mainLine.transform.localScale = new Vector3(0.2f, 10f, 0.2f);
            mainLine.transform.Rotate(0f, 0f, 90f);
            
            waterLine.Add(mainLine);
            return waterLine;
        }

        public List<GameObject> GenerateIrrigationSystem(Vector3 roomPosition, Vector3 roomDimensions)
        {
            var irrigation = new List<GameObject>();
            
            // Create simple irrigation system for grow room
            var irrigationController = new GameObject("Irrigation Controller");
            irrigationController.transform.position = roomPosition + new Vector3(0f, 2f, 0f);
            
            irrigation.Add(irrigationController);
            return irrigation;
        }

        public List<GameObject> GenerateDrainageSystem(List<RoomLayout> rooms)
        {
            var drainage = new List<GameObject>();
            
            // Create central drainage system
            var mainDrain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainDrain.name = "Main Drainage";
            mainDrain.transform.position = new Vector3(0f, -1f, 0f);
            mainDrain.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            
            drainage.Add(mainDrain);
            return drainage;
        }

        public List<GameObject> GenerateHVACSystem(List<RoomLayout> rooms)
        {
            var hvac = new List<GameObject>();
            
            // Create central HVAC system
            var centralHVAC = new GameObject("Central HVAC");
            centralHVAC.transform.position = new Vector3(0f, 3f, 0f);
            // centralHVAC.AddComponent<HVACController>();
            
            hvac.Add(centralHVAC);
            return hvac;
        }

        public List<GameObject> GenerateSecuritySystem(List<RoomLayout> rooms)
        {
            var security = new List<GameObject>();
            
            // Create security cameras for each room
            foreach (var room in rooms)
            {
                var camera = new GameObject($"Security Camera - {room.RoomName}");
                camera.transform.position = room.Position + new Vector3(0f, 2.5f, 0f);
                security.Add(camera);
            }
            
            return security;
        }

        public List<GameObject> GenerateNetworkInfrastructure(List<RoomLayout> rooms)
        {
            var network = new List<GameObject>();
            
            // Create network switch
            var networkSwitch = new GameObject("Network Switch");
            networkSwitch.transform.position = new Vector3(0f, 1.5f, 0f);
            
            network.Add(networkSwitch);
            return network;
        }
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// Room placement data
    /// </summary>
    [System.Serializable]
    public class RoomPlacement
    {
        public FacilityRoomType RoomType;
        public Vector3 Position;
        public Vector3 Dimensions;
    }

    /// <summary>
    /// Room layout configuration
    /// </summary>
    [System.Serializable]
    public class RoomLayout
    {
        public string RoomId;
        public string RoomName;
        public string RoomType;
        public Vector3 Position;
        public Vector3 Dimensions;
        public float Area => Dimensions.x * Dimensions.z;
    }

    /// <summary>
    /// Building generation settings
    /// </summary>
    [System.Serializable]
    public class BuildingGenerationSettings
    {
        public bool EnableSpecialtyBuildings = true;
        public bool EnableInfrastructure = true;
        public int MaxRoomCount = 12;
        public Vector2Int PreferredRoomSize = new Vector2Int(8, 8);
    }

    /// <summary>
    /// Building generation completion arguments
    /// </summary>
    [System.Serializable]
    public class BuildingGenerationCompleteArgs
    {
        public FacilitySceneType SceneType;
        public Vector2Int FacilitySize;
        public List<GameObject> GeneratedObjects;
        public List<RoomLayout> GeneratedRooms;
        public bool Success;
    }

    /// <summary>
    /// Building generation statistics
    /// </summary>
    [System.Serializable]
    public class BuildingGenerationStats
    {
        public int TotalObjectsGenerated;
        public int TotalRoomsGenerated;
        public bool ServiceInitialized;
        public DateTime LastGenerationTime;
    }

    /// <summary>
    /// Facility room types
    /// </summary>
    public enum FacilityRoomType
    {
        Vegetative,
        Flowering,
        Nursery,
        ProcessingRoom,
        StorageRoom,
        Laboratory,
        Office,
        BreakRoom,
        HVACRoom,
        ElectricalRoom,
        ServerRoom
    }

    #endregion
}