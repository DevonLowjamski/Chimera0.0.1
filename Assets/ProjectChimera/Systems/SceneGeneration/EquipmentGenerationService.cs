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
    /// Equipment Generation Service - Dedicated service for equipment placement and infrastructure systems
    /// Extracted from ProceduralSceneGenerator to provide focused equipment generation functionality
    /// Handles grow room sensors, control panels, processing machinery, maintenance tools, and transport equipment
    /// Includes environmental sensors, nutrient systems, processing equipment, and maintenance infrastructure
    /// </summary>
    public class EquipmentGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Equipment Generation Configuration")]
        [SerializeField] private bool _enableEquipmentGeneration = true;
        [SerializeField] private bool _enableGrowEquipment = true;
        [SerializeField] private bool _enableProcessingEquipment = true;
        [SerializeField] private bool _enableMaintenanceEquipment = true;
        [SerializeField] private bool _enableTransportEquipment = true;

        [Header("Grow Equipment Settings")]
        [SerializeField] private bool _generateEnvironmentalSensors = true;
        [SerializeField] private bool _generateControlPanels = true;
        [SerializeField] private bool _generateNutrientSystems = true;
        [SerializeField] private float _sensorGenerationDelay = 0.01f;

        [Header("Processing Equipment Settings")]
        [SerializeField] private bool _includeProcessingArea = true;
        [SerializeField] private bool _generateTrimmingMachines = true;
        [SerializeField] private bool _generateDryingRacks = true;
        [SerializeField] private bool _generatePackagingStations = true;

        [Header("Maintenance Equipment Settings")]
        [SerializeField] private int _maxCleaningCarts = 4;
        [SerializeField] private bool _generateToolStorage = true;
        [SerializeField] private float _maintenanceEquipmentSpacing = 5f;

        [Header("Transport Equipment Settings")]
        [SerializeField] private GameObject[] _vehiclePrefabs = new GameObject[0];
        [SerializeField] private int _maxVehicles = 3;
        [SerializeField] private int _minTransportCarts = 3;
        [SerializeField] private int _maxTransportCarts = 6;
        [SerializeField] private float _vehicleSpacing = 8f;

        [Header("Equipment Containers")]
        [SerializeField] private Transform _equipmentContainer;
        [SerializeField] private Transform _sensorContainer;
        [SerializeField] private Transform _processingContainer;
        [SerializeField] private Transform _maintenanceContainer;
        [SerializeField] private Transform _transportContainer;

        // Equipment generation state
        private bool _isInitialized = false;
        private System.Random _random;
        private Transform _containerTransform;
        private Vector2Int _facilitySize;
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();
        private List<GameObject> _generatedEquipment = new List<GameObject>();
        private FacilitySceneType _sceneType;

        // Equipment tracking
        private Dictionary<SensorType, List<GameObject>> _generatedSensors = new Dictionary<SensorType, List<GameObject>>();
        private List<GameObject> _controlPanels = new List<GameObject>();
        private List<GameObject> _nutrientSystems = new List<GameObject>();
        private List<GameObject> _processingMachinery = new List<GameObject>();
        private List<GameObject> _maintenanceEquipment = new List<GameObject>();
        private List<GameObject> _transportEquipment = new List<GameObject>();

        // Events for equipment generation progress
        public static event System.Action<EquipmentGenerationPhase> OnEquipmentPhaseStarted;
        public static event System.Action<EquipmentGenerationPhase> OnEquipmentPhaseCompleted;
        public static event System.Action<string, int> OnEquipmentTypeGenerated;
        public static event System.Action<EquipmentGenerationReport> OnEquipmentGenerationCompleted;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Equipment Generation Service";
        public IReadOnlyList<GameObject> GeneratedEquipment => _generatedEquipment;
        public IReadOnlyDictionary<SensorType, List<GameObject>> GeneratedSensors => _generatedSensors;

        public void Initialize()
        {
            InitializeService();
        }

        public void Initialize(int seed = 0, Transform container = null)
        {
            InitializeService(seed, container);
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

        private void Start()
        {
            InitializeService();
        }

        private void InitializeDataStructures()
        {
            _generatedEquipment = new List<GameObject>();
            _generatedSensors = new Dictionary<SensorType, List<GameObject>>();
            _controlPanels = new List<GameObject>();
            _nutrientSystems = new List<GameObject>();
            _processingMachinery = new List<GameObject>();
            _maintenanceEquipment = new List<GameObject>();
            _transportEquipment = new List<GameObject>();

            // Initialize sensor type tracking
            foreach (SensorType sensorType in System.Enum.GetValues(typeof(SensorType)))
            {
                _generatedSensors[sensorType] = new List<GameObject>();
            }
        }

        public void InitializeService()
        {
            InitializeService(0, null);
        }

        public void InitializeService(int seed = 0, Transform container = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("EquipmentGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                SetupContainers();
                
                _isInitialized = true;
                ChimeraLogger.Log("EquipmentGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize EquipmentGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedEquipment();
            _generatedEquipment.Clear();
            _generatedSensors.Clear();
            _controlPanels.Clear();
            _nutrientSystems.Clear();
            _processingMachinery.Clear();
            _maintenanceEquipment.Clear();
            _transportEquipment.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("EquipmentGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_sensorGenerationDelay < 0f)
            {
                ChimeraLogger.LogWarning("Invalid sensor generation delay, using default 0.01s", this);
                _sensorGenerationDelay = 0.01f;
            }

            if (_maxCleaningCarts < 1)
            {
                ChimeraLogger.LogWarning("Invalid cleaning cart count, using default 2", this);
                _maxCleaningCarts = 2;
            }
        }

        private void SetupContainers()
        {
            if (_equipmentContainer == null)
            {
                var containerGO = new GameObject("Equipment Container");
                _equipmentContainer = containerGO.transform;
            }

            // Create specialized containers if not assigned
            if (_sensorContainer == null)
            {
                var sensorGO = new GameObject("Sensor Container");
                sensorGO.transform.SetParent(_equipmentContainer);
                _sensorContainer = sensorGO.transform;
            }

            if (_processingContainer == null)
            {
                var processingGO = new GameObject("Processing Container");
                processingGO.transform.SetParent(_equipmentContainer);
                _processingContainer = processingGO.transform;
            }

            if (_maintenanceContainer == null)
            {
                var maintenanceGO = new GameObject("Maintenance Container");
                maintenanceGO.transform.SetParent(_equipmentContainer);
                _maintenanceContainer = maintenanceGO.transform;
            }

            if (_transportContainer == null)
            {
                var transportGO = new GameObject("Transport Container");
                transportGO.transform.SetParent(_equipmentContainer);
                _transportContainer = transportGO.transform;
            }
        }

        #endregion

        #region Equipment Generation Interface

        /// <summary>
        /// Generate all equipment types for the facility
        /// </summary>
        public IEnumerator GenerateEquipment(FacilitySceneType sceneType, Vector2Int facilitySize, List<RoomLayout> rooms, int randomSeed = 0)
        {
            if (!_isInitialized || !_enableEquipmentGeneration)
            {
                ChimeraLogger.LogWarning("EquipmentGenerationService not initialized or equipment generation disabled", this);
                yield break;
            }

            _sceneType = sceneType;
            _facilitySize = facilitySize;
            _generatedRooms = rooms ?? new List<RoomLayout>();
            _random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            ChimeraLogger.Log($"Starting equipment generation for {sceneType} facility", this);

            yield return StartCoroutine(GenerateGrowEquipment());
            yield return StartCoroutine(GenerateProcessingEquipment());
            yield return StartCoroutine(GenerateMaintenanceEquipment());
            yield return StartCoroutine(GenerateTransportEquipment());

            GenerateCompletionReport();
            ChimeraLogger.Log($"Equipment generation completed: {_generatedEquipment.Count} equipment items generated", this);
        }

        /// <summary>
        /// Generate equipment for specific room
        /// </summary>
        public IEnumerator GenerateRoomEquipment(RoomLayout room)
        {
            if (!_isInitialized || room == null) yield break;

            if (IsGrowRoom(room.RoomType))
            {
                yield return StartCoroutine(GenerateGrowRoomEquipment(room));
            }
            else if (IsProcessingRoom(room.RoomType))
            {
                yield return StartCoroutine(GenerateProcessingRoomEquipment(room));
            }
        }

        /// <summary>
        /// Clear all generated equipment
        /// </summary>
        public void ClearGeneratedEquipment()
        {
            foreach (var equipment in _generatedEquipment)
            {
                if (equipment != null)
                {
                    DestroyImmediate(equipment);
                }
            }

            _generatedEquipment.Clear();
            _generatedSensors.Clear();
            _controlPanels.Clear();
            _nutrientSystems.Clear();
            _processingMachinery.Clear();
            _maintenanceEquipment.Clear();
            _transportEquipment.Clear();
        }

        #endregion

        #region Grow Equipment Generation

        /// <summary>
        /// Generate grow room equipment (sensors, control panels, nutrient systems)
        /// </summary>
        private IEnumerator GenerateGrowEquipment()
        {
            if (!_enableGrowEquipment) yield break;

            OnEquipmentPhaseStarted?.Invoke(EquipmentGenerationPhase.GrowEquipment);
            ChimeraLogger.Log("Generating grow equipment", this);

            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateGrowRoomEquipment(room));
                }
            }

            OnEquipmentPhaseCompleted?.Invoke(EquipmentGenerationPhase.GrowEquipment);
        }

        /// <summary>
        /// Generate equipment for specific grow room
        /// </summary>
        private IEnumerator GenerateGrowRoomEquipment(RoomLayout room)
        {
            if (_generateEnvironmentalSensors)
            {
                yield return StartCoroutine(GenerateEnvironmentalSensors(room));
            }

            if (_generateControlPanels)
            {
                yield return StartCoroutine(GenerateControlPanels(room));
            }

            if (_generateNutrientSystems)
            {
                yield return StartCoroutine(GenerateNutrientSystems(room));
            }
        }

        /// <summary>
        /// Generate environmental sensors throughout the room
        /// </summary>
        private IEnumerator GenerateEnvironmentalSensors(RoomLayout room)
        {
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
                _generatedEquipment.Add(sensor);
                _generatedSensors[sensorType].Add(sensor);

                yield return new WaitForSeconds(_sensorGenerationDelay);
            }

            OnEquipmentTypeGenerated?.Invoke("Environmental Sensors", sensorTypes.Count);
        }

        /// <summary>
        /// Generate individual sensor
        /// </summary>
        private GameObject GenerateSensor(SensorType sensorType, Vector3 position)
        {
            GameObject sensor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sensor.name = $"{sensorType} Sensor";
            sensor.transform.SetParent(_sensorContainer);
            sensor.transform.position = position;
            sensor.transform.localScale = Vector3.one * 0.1f;

            // Add environmental sensor component (placeholder)
            // var sensorComponent = sensor.AddComponent<EnvironmentalSensor>();

            // Set sensor-specific color
            var renderer = sensor.GetComponent<Renderer>();
            renderer.material.color = GetSensorColor(sensorType);

            return sensor;
        }

        /// <summary>
        /// Get color for sensor type
        /// </summary>
        private Color GetSensorColor(SensorType sensorType)
        {
            return sensorType switch
            {
                SensorType.Temperature => Color.red,
                SensorType.Humidity => Color.blue,
                SensorType.LightLevel => Color.yellow,
                SensorType.CO2Level => Color.green,
                SensorType.AirFlow => Color.cyan,
                _ => Color.white
            };
        }

        /// <summary>
        /// Generate control panels for room management
        /// </summary>
        private IEnumerator GenerateControlPanels(RoomLayout room)
        {
            Vector3 panelPosition = room.Position + new Vector3(
                room.Dimensions.x / 2f - 0.5f,
                1.5f,
                0f
            );

            GameObject controlPanel = GenerateControlPanel(panelPosition);
            _generatedEquipment.Add(controlPanel);
            _controlPanels.Add(controlPanel);

            OnEquipmentTypeGenerated?.Invoke("Control Panels", 1);
            yield return null;
        }

        /// <summary>
        /// Generate individual control panel
        /// </summary>
        private GameObject GenerateControlPanel(Vector3 position)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Control Panel";
            panel.transform.SetParent(_equipmentContainer);
            panel.transform.position = position;
            panel.transform.localScale = new Vector3(0.1f, 0.8f, 0.6f);

            // Add visual screen
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Screen";
            screen.transform.SetParent(panel.transform);
            screen.transform.localPosition = new Vector3(0.05f, 0.2f, 0f);
            screen.transform.localScale = new Vector3(0.5f, 0.6f, 0.8f);

            var screenRenderer = screen.GetComponent<Renderer>();
            screenRenderer.material.color = Color.black;

            return panel;
        }

        /// <summary>
        /// Generate nutrient delivery systems
        /// </summary>
        private IEnumerator GenerateNutrientSystems(RoomLayout room)
        {
            Vector3 systemPosition = room.Position + new Vector3(0f, 0f, room.Dimensions.z / 2f - 1f);

            GameObject nutrientSystem = GenerateNutrientSystem(systemPosition);
            _generatedEquipment.Add(nutrientSystem);
            _nutrientSystems.Add(nutrientSystem);

            OnEquipmentTypeGenerated?.Invoke("Nutrient Systems", 1);
            yield return null;
        }

        /// <summary>
        /// Generate individual nutrient system
        /// </summary>
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

        #endregion

        #region Processing Equipment Generation

        /// <summary>
        /// Generate processing equipment (trimming machines, drying racks, packaging stations)
        /// </summary>
        private IEnumerator GenerateProcessingEquipment()
        {
            if (!_enableProcessingEquipment || !_includeProcessingArea) yield break;

            OnEquipmentPhaseStarted?.Invoke(EquipmentGenerationPhase.ProcessingEquipment);
            ChimeraLogger.Log("Generating processing equipment", this);

            var processingRoom = _generatedRooms.FirstOrDefault(r => IsProcessingRoom(r.RoomType));
            if (processingRoom != null)
            {
                yield return StartCoroutine(GenerateProcessingRoomEquipment(processingRoom));
            }

            OnEquipmentPhaseCompleted?.Invoke(EquipmentGenerationPhase.ProcessingEquipment);
        }

        /// <summary>
        /// Generate equipment for processing room
        /// </summary>
        private IEnumerator GenerateProcessingRoomEquipment(RoomLayout room)
        {
            int equipmentCount = 0;

            if (_generateTrimmingMachines)
            {
                GameObject trimmer = GenerateTrimmingMachine(room.Position + new Vector3(-2f, 0f, 0f));
                _generatedEquipment.Add(trimmer);
                _processingMachinery.Add(trimmer);
                equipmentCount++;
            }

            if (_generateDryingRacks)
            {
                GameObject dryingRacks = GenerateDryingRacks(room.Position + new Vector3(2f, 0f, 0f));
                _generatedEquipment.Add(dryingRacks);
                _processingMachinery.Add(dryingRacks);
                equipmentCount++;
            }

            if (_generatePackagingStations)
            {
                GameObject packagingStation = GeneratePackagingStation(room.Position + new Vector3(0f, 0f, 2f));
                _generatedEquipment.Add(packagingStation);
                _processingMachinery.Add(packagingStation);
                equipmentCount++;
            }

            OnEquipmentTypeGenerated?.Invoke("Processing Machinery", equipmentCount);
            yield return null;
        }

        /// <summary>
        /// Generate trimming machine
        /// </summary>
        private GameObject GenerateTrimmingMachine(Vector3 position)
        {
            GameObject machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machine.name = "Trimming Machine";
            machine.transform.SetParent(_processingContainer);
            machine.transform.position = position;
            machine.transform.localScale = new Vector3(2f, 1f, 1.5f);

            var renderer = machine.GetComponent<Renderer>();
            renderer.material.color = new Color(0.8f, 0.8f, 0.9f);

            return machine;
        }

        /// <summary>
        /// Generate drying rack system
        /// </summary>
        private GameObject GenerateDryingRacks(Vector3 position)
        {
            GameObject racks = new GameObject("Drying Racks");
            racks.transform.SetParent(_processingContainer);
            racks.transform.position = position;

            for (int i = 0; i < 4; i++)
            {
                GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rack.name = $"Drying Rack {i + 1}";
                rack.transform.SetParent(racks.transform);
                rack.transform.localPosition = new Vector3(0f, i * 0.5f, 0f);
                rack.transform.localScale = new Vector3(2f, 0.1f, 1f);

                var renderer = rack.GetComponent<Renderer>();
                renderer.material.color = new Color(0.6f, 0.4f, 0.2f); // Wood color
            }

            return racks;
        }

        /// <summary>
        /// Generate packaging station
        /// </summary>
        private GameObject GeneratePackagingStation(Vector3 position)
        {
            GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
            station.name = "Packaging Station";
            station.transform.SetParent(_processingContainer);
            station.transform.position = position;
            station.transform.localScale = new Vector3(1.5f, 0.8f, 1f);

            var renderer = station.GetComponent<Renderer>();
            renderer.material.color = new Color(0.7f, 0.7f, 0.8f);

            return station;
        }

        #endregion

        #region Maintenance Equipment Generation

        /// <summary>
        /// Generate maintenance equipment (cleaning carts, tool storage)
        /// </summary>
        private IEnumerator GenerateMaintenanceEquipment()
        {
            if (!_enableMaintenanceEquipment) yield break;

            OnEquipmentPhaseStarted?.Invoke(EquipmentGenerationPhase.MaintenanceEquipment);
            ChimeraLogger.Log("Generating maintenance equipment", this);

            yield return StartCoroutine(GenerateCleaningEquipment());
            
            if (_generateToolStorage)
            {
                yield return StartCoroutine(GenerateToolStorage());
            }

            OnEquipmentPhaseCompleted?.Invoke(EquipmentGenerationPhase.MaintenanceEquipment);
        }

        /// <summary>
        /// Generate cleaning equipment
        /// </summary>
        private IEnumerator GenerateCleaningEquipment()
        {
            int cleaningCartCount = Mathf.Min(_maxCleaningCarts, 4);
            var cleaningPositions = GenerateCleaningEquipmentPositions(cleaningCartCount);

            foreach (var position in cleaningPositions)
            {
                GameObject cleaningCart = GenerateCleaningCart(position);
                _generatedEquipment.Add(cleaningCart);
                _maintenanceEquipment.Add(cleaningCart);
                yield return new WaitForSeconds(0.01f);
            }

            OnEquipmentTypeGenerated?.Invoke("Cleaning Equipment", cleaningCartCount);
        }

        /// <summary>
        /// Generate positions for cleaning equipment
        /// </summary>
        private Vector3[] GenerateCleaningEquipmentPositions(int count)
        {
            var positions = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                positions[i] = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
            }

            return positions;
        }

        /// <summary>
        /// Generate cleaning cart
        /// </summary>
        private GameObject GenerateCleaningCart(Vector3 position)
        {
            GameObject cart = new GameObject("Cleaning Cart");
            cart.transform.SetParent(_maintenanceContainer);
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

        /// <summary>
        /// Generate tool storage
        /// </summary>
        private IEnumerator GenerateToolStorage()
        {
            GameObject toolCabinet = GenerateToolCabinet(new Vector3(0f, 0f, _facilitySize.y / 2f - 1f));
            _generatedEquipment.Add(toolCabinet);
            _maintenanceEquipment.Add(toolCabinet);

            OnEquipmentTypeGenerated?.Invoke("Tool Storage", 1);
            yield return null;
        }

        /// <summary>
        /// Generate tool cabinet
        /// </summary>
        private GameObject GenerateToolCabinet(Vector3 position)
        {
            GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "Tool Cabinet";
            cabinet.transform.SetParent(_maintenanceContainer);
            cabinet.transform.position = position;
            cabinet.transform.localScale = new Vector3(1f, 2f, 0.5f);

            var renderer = cabinet.GetComponent<Renderer>();
            renderer.material.color = new Color(0.5f, 0.5f, 0.6f);

            return cabinet;
        }

        #endregion

        #region Transport Equipment Generation

        /// <summary>
        /// Generate transport equipment (vehicles, carts)
        /// </summary>
        private IEnumerator GenerateTransportEquipment()
        {
            if (!_enableTransportEquipment) yield break;

            OnEquipmentPhaseStarted?.Invoke(EquipmentGenerationPhase.TransportEquipment);
            ChimeraLogger.Log("Generating transport equipment", this);

            if (_vehiclePrefabs.Length > 0)
            {
                yield return StartCoroutine(GenerateVehicles());
            }

            yield return StartCoroutine(GenerateCartSystem());

            OnEquipmentPhaseCompleted?.Invoke(EquipmentGenerationPhase.TransportEquipment);
        }

        /// <summary>
        /// Generate vehicles for outdoor facilities
        /// </summary>
        private IEnumerator GenerateVehicles()
        {
            if (_sceneType != FacilitySceneType.OutdoorFarm && _sceneType != FacilitySceneType.MixedFacility) 
                yield break;

            int vehicleCount = _random.Next(1, Mathf.Min(_maxVehicles, _vehiclePrefabs.Length) + 1);

            for (int i = 0; i < vehicleCount; i++)
            {
                Vector3 vehiclePosition = new Vector3(
                    _facilitySize.x / 2f + 5f + i * _vehicleSpacing,
                    0f,
                    _facilitySize.y / 2f + 5f
                );

                var vehiclePrefab = _vehiclePrefabs[_random.Next(_vehiclePrefabs.Length)];
                GameObject vehicle = Instantiate(vehiclePrefab, vehiclePosition, Quaternion.identity);
                vehicle.transform.SetParent(_transportContainer);

                _generatedEquipment.Add(vehicle);
                _transportEquipment.Add(vehicle);
                yield return new WaitForSeconds(0.02f);
            }

            OnEquipmentTypeGenerated?.Invoke("Vehicles", vehicleCount);
        }

        /// <summary>
        /// Generate transport cart system
        /// </summary>
        private IEnumerator GenerateCartSystem()
        {
            int cartCount = _random.Next(_minTransportCarts, _maxTransportCarts + 1);

            for (int i = 0; i < cartCount; i++)
            {
                Vector3 cartPosition = new Vector3(
                    ((i % 3) - 1f) * 5f,
                    0f,
                    _facilitySize.y / 2f - 2f
                );

                GameObject cart = GenerateTransportCart(cartPosition);
                _generatedEquipment.Add(cart);
                _transportEquipment.Add(cart);

                yield return new WaitForSeconds(0.01f);
            }

            OnEquipmentTypeGenerated?.Invoke("Transport Carts", cartCount);
        }

        /// <summary>
        /// Generate individual transport cart
        /// </summary>
        private GameObject GenerateTransportCart(Vector3 position)
        {
            GameObject cart = new GameObject("Transport Cart");
            cart.transform.SetParent(_transportContainer);
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

        #region Utility Methods

        /// <summary>
        /// Check if room is a grow room
        /// </summary>
        private bool IsGrowRoom(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Cultivation") || 
                   roomType.Contains("Vegetative") || roomType.Contains("Flowering");
        }

        /// <summary>
        /// Check if room is a processing room
        /// </summary>
        private bool IsProcessingRoom(string roomType)
        {
            return roomType.Contains("Processing") || roomType.Contains("Harvest") || 
                   roomType.Contains("Packaging") || roomType.Contains("Trimming");
        }

        /// <summary>
        /// Generate completion report
        /// </summary>
        private void GenerateCompletionReport()
        {
            var report = new EquipmentGenerationReport
            {
                TotalEquipmentGenerated = _generatedEquipment.Count,
                SensorCount = _generatedSensors.Values.Sum(list => list.Count),
                ControlPanelCount = _controlPanels.Count,
                NutrientSystemCount = _nutrientSystems.Count,
                ProcessingMachineryCount = _processingMachinery.Count,
                MaintenanceEquipmentCount = _maintenanceEquipment.Count,
                TransportEquipmentCount = _transportEquipment.Count,
                GenerationTime = Time.realtimeSinceStartup,
                FacilityType = _sceneType,
                FacilitySize = _facilitySize
            };

            OnEquipmentGenerationCompleted?.Invoke(report);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get equipment count by type
        /// </summary>
        public int GetEquipmentCount(EquipmentType equipmentType)
        {
            return equipmentType switch
            {
                EquipmentType.Sensors => _generatedSensors.Values.Sum(list => list.Count),
                EquipmentType.ControlPanels => _controlPanels.Count,
                EquipmentType.NutrientSystems => _nutrientSystems.Count,
                EquipmentType.ProcessingMachinery => _processingMachinery.Count,
                EquipmentType.MaintenanceEquipment => _maintenanceEquipment.Count,
                EquipmentType.TransportEquipment => _transportEquipment.Count,
                _ => 0
            };
        }

        /// <summary>
        /// Get equipment by type
        /// </summary>
        public List<GameObject> GetEquipmentByType(EquipmentType equipmentType)
        {
            return equipmentType switch
            {
                EquipmentType.ControlPanels => new List<GameObject>(_controlPanels),
                EquipmentType.NutrientSystems => new List<GameObject>(_nutrientSystems),
                EquipmentType.ProcessingMachinery => new List<GameObject>(_processingMachinery),
                EquipmentType.MaintenanceEquipment => new List<GameObject>(_maintenanceEquipment),
                EquipmentType.TransportEquipment => new List<GameObject>(_transportEquipment),
                EquipmentType.Sensors => _generatedSensors.Values.SelectMany(list => list).ToList(),
                _ => new List<GameObject>()
            };
        }

        /// <summary>
        /// Update equipment configuration
        /// </summary>
        public void UpdateEquipmentConfiguration(EquipmentGenerationConfig config)
        {
            _enableGrowEquipment = config.EnableGrowEquipment;
            _enableProcessingEquipment = config.EnableProcessingEquipment;
            _enableMaintenanceEquipment = config.EnableMaintenanceEquipment;
            _enableTransportEquipment = config.EnableTransportEquipment;
            
            ChimeraLogger.Log("Equipment generation configuration updated", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Equipment generation phases
    /// </summary>
    public enum EquipmentGenerationPhase
    {
        GrowEquipment,
        ProcessingEquipment,
        MaintenanceEquipment,
        TransportEquipment
    }

    /// <summary>
    /// Equipment types for categorization
    /// </summary>
    public enum EquipmentType
    {
        Sensors,
        ControlPanels,
        NutrientSystems,
        ProcessingMachinery,
        MaintenanceEquipment,
        TransportEquipment
    }

    /// <summary>
    /// Sensor types for environmental monitoring
    /// </summary>
    public enum SensorType
    {
        Temperature,
        Humidity,
        LightLevel,
        CO2Level,
        AirFlow,
        pH,
        EC,
        Pressure
    }



    /// <summary>
    /// Equipment generation configuration
    /// </summary>
    [System.Serializable]
    public class EquipmentGenerationConfig
    {
        public bool EnableGrowEquipment = true;
        public bool EnableProcessingEquipment = true;
        public bool EnableMaintenanceEquipment = true;
        public bool EnableTransportEquipment = true;
        public int MaxCleaningCarts = 4;
        public int MaxTransportCarts = 6;
        public float SensorGenerationDelay = 0.01f;
    }

    /// <summary>
    /// Equipment generation completion report
    /// </summary>
    [System.Serializable]
    public class EquipmentGenerationReport
    {
        public int TotalEquipmentGenerated;
        public int SensorCount;
        public int ControlPanelCount;
        public int NutrientSystemCount;
        public int ProcessingMachineryCount;
        public int MaintenanceEquipmentCount;
        public int TransportEquipmentCount;
        public float GenerationTime;
        public FacilitySceneType FacilityType;
        public Vector2Int FacilitySize;
    }

    #endregion
}