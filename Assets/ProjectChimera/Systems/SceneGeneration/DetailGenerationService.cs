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
    /// Detail Generation Service - Dedicated service for decorations, finishing touches, and polish elements
    /// Extracted from ProceduralSceneGenerator to provide focused detail generation functionality
    /// Handles environmental details (rocks, debris), furniture placement, and decorative elements
    /// Includes office furniture, break room setups, and atmospheric details for facility polish
    /// </summary>
    public class DetailGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Detail Generation Configuration")]
        [SerializeField] private bool _enableDetailGeneration = true;
        [SerializeField] private bool _enableEnvironmentalDetails = true;
        [SerializeField] private bool _enableFurniture = true;
        [SerializeField] private bool _enableDecorations = true;

        [Header("Environmental Detail Settings")]
        [SerializeField] private bool _generateRocks = true;
        [SerializeField] private bool _generateDebris = true;
        [SerializeField] private int _minRockCount = 10;
        [SerializeField] private int _maxRockCount = 25;
        [SerializeField] private int _minDebrisCount = 5;
        [SerializeField] private int _maxDebrisCount = 15;
        [SerializeField] private float _rockMinScale = 0.2f;
        [SerializeField] private float _rockMaxScale = 0.8f;
        [SerializeField] private float _debrisMinScale = 0.1f;
        [SerializeField] private float _debrisMaxScale = 0.3f;

        [Header("Furniture Settings")]
        [SerializeField] private bool _generateOfficeFurniture = true;
        [SerializeField] private bool _generateBreakRoomFurniture = true;
        [SerializeField] private bool _generateStorageFurniture = true;
        [SerializeField] private int _chairsPerTable = 4;

        [Header("Generation Delays")]
        [SerializeField] private float _rockGenerationDelay = 0.01f;
        [SerializeField] private float _debrisGenerationDelay = 0.005f;
        [SerializeField] private float _furnitureGenerationDelay = 0.02f;

        [Header("Detail Containers")]
        [SerializeField] private Transform _detailContainer;
        [SerializeField] private Transform _decorationContainer;
        [SerializeField] private Transform _furnitureContainer;
        [SerializeField] private Transform _environmentalDetailContainer;

        // Detail generation state
        private bool _isInitialized = false;
        private System.Random _random;
        private Transform _containerTransform;
        private Vector2Int _facilitySize;
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();
        private List<GameObject> _generatedDetails = new List<GameObject>();
        private FacilitySceneType _sceneType;

        // Detail tracking
        private List<GameObject> _environmentalDetails = new List<GameObject>();
        private List<GameObject> _furniture = new List<GameObject>();
        private List<GameObject> _decorations = new List<GameObject>();
        private Dictionary<string, List<GameObject>> _roomFurniture = new Dictionary<string, List<GameObject>>();

        // Events for detail generation progress
        public static event System.Action<DetailGenerationPhase> OnDetailPhaseStarted;
        public static event System.Action<DetailGenerationPhase> OnDetailPhaseCompleted;
        public static event System.Action<string, int> OnDetailTypeGenerated;
        public static event System.Action<DetailGenerationReport> OnDetailGenerationCompleted;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Detail Generation Service";
        public IReadOnlyList<GameObject> GeneratedDetails => _generatedDetails;
        public IReadOnlyList<GameObject> EnvironmentalDetails => _environmentalDetails;
        public IReadOnlyList<GameObject> Furniture => _furniture;

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
            _generatedDetails = new List<GameObject>();
            _environmentalDetails = new List<GameObject>();
            _furniture = new List<GameObject>();
            _decorations = new List<GameObject>();
            _roomFurniture = new Dictionary<string, List<GameObject>>();
        }

        public void InitializeService()
        {
            InitializeService(0, null);
        }

        public void InitializeService(int seed = 0, Transform container = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("DetailGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                SetupContainers();
                
                _isInitialized = true;
                ChimeraLogger.Log("DetailGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize DetailGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedDetails();
            _generatedDetails.Clear();
            _environmentalDetails.Clear();
            _furniture.Clear();
            _decorations.Clear();
            _roomFurniture.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("DetailGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_rockGenerationDelay < 0f)
            {
                ChimeraLogger.LogWarning("Invalid rock generation delay, using default 0.01s", this);
                _rockGenerationDelay = 0.01f;
            }

            if (_minRockCount < 0)
            {
                ChimeraLogger.LogWarning("Invalid minimum rock count, using default 10", this);
                _minRockCount = 10;
            }

            if (_maxRockCount < _minRockCount)
            {
                ChimeraLogger.LogWarning("Max rock count less than min, adjusting", this);
                _maxRockCount = _minRockCount + 10;
            }
        }

        private void SetupContainers()
        {
            if (_detailContainer == null)
            {
                var containerGO = new GameObject("Detail Container");
                _detailContainer = containerGO.transform;
            }

            // Create specialized containers if not assigned
            if (_decorationContainer == null)
            {
                var decorationGO = new GameObject("Decoration Container");
                decorationGO.transform.SetParent(_detailContainer);
                _decorationContainer = decorationGO.transform;
            }

            if (_furnitureContainer == null)
            {
                var furnitureGO = new GameObject("Furniture Container");
                furnitureGO.transform.SetParent(_detailContainer);
                _furnitureContainer = furnitureGO.transform;
            }

            if (_environmentalDetailContainer == null)
            {
                var envDetailGO = new GameObject("Environmental Detail Container");
                envDetailGO.transform.SetParent(_detailContainer);
                _environmentalDetailContainer = envDetailGO.transform;
            }
        }

        #endregion

        #region Detail Generation Interface

        /// <summary>
        /// Generate all detail elements for the facility
        /// </summary>
        public IEnumerator GenerateDetails(FacilitySceneType sceneType, Vector2Int facilitySize, List<RoomLayout> rooms, int randomSeed = 0)
        {
            if (!_isInitialized || !_enableDetailGeneration)
            {
                ChimeraLogger.LogWarning("DetailGenerationService not initialized or detail generation disabled", this);
                yield break;
            }

            _sceneType = sceneType;
            _facilitySize = facilitySize;
            _generatedRooms = rooms ?? new List<RoomLayout>();
            _random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            ChimeraLogger.Log($"Starting detail generation for {sceneType} facility", this);

            if (_enableEnvironmentalDetails)
            {
                yield return StartCoroutine(GenerateEnvironmentalDetails());
            }

            if (_enableFurniture)
            {
                yield return StartCoroutine(GenerateFurniture());
            }

            if (_enableDecorations)
            {
                yield return StartCoroutine(GenerateDecorations());
            }

            GenerateCompletionReport();
            ChimeraLogger.Log($"Detail generation completed: {_generatedDetails.Count} detail objects generated", this);
        }

        /// <summary>
        /// Clear all generated detail objects
        /// </summary>
        public void ClearGeneratedDetails()
        {
            foreach (var detail in _generatedDetails)
            {
                if (detail != null)
                {
                    DestroyImmediate(detail);
                }
            }

            _generatedDetails.Clear();
            _environmentalDetails.Clear();
            _furniture.Clear();
            _decorations.Clear();
            _roomFurniture.Clear();
        }

        #endregion

        #region Environmental Details Generation

        /// <summary>
        /// Generate environmental details (rocks, debris, natural elements)
        /// </summary>
        private IEnumerator GenerateEnvironmentalDetails()
        {
            OnDetailPhaseStarted?.Invoke(DetailGenerationPhase.EnvironmentalDetails);
            ChimeraLogger.Log("Generating environmental details", this);

            if (_generateRocks)
            {
                yield return StartCoroutine(GenerateRocks());
            }

            if (_generateDebris)
            {
                yield return StartCoroutine(GenerateDebris());
            }

            OnDetailPhaseCompleted?.Invoke(DetailGenerationPhase.EnvironmentalDetails);
        }

        /// <summary>
        /// Generate random rocks for outdoor facilities
        /// </summary>
        private IEnumerator GenerateRocks()
        {
            if (_sceneType != FacilitySceneType.OutdoorFarm && _sceneType != FacilitySceneType.MixedFacility)
                yield break;

            int rockCount = _random.Next(_minRockCount, _maxRockCount + 1);
            int rocksGenerated = 0;

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
                _generatedDetails.Add(rock);
                _environmentalDetails.Add(rock);
                rocksGenerated++;

                if (i % 5 == 0)
                    yield return new WaitForSeconds(_rockGenerationDelay);
            }

            OnDetailTypeGenerated?.Invoke("Rocks", rocksGenerated);
        }

        /// <summary>
        /// Generate individual rock
        /// </summary>
        private GameObject GenerateRock(Vector3 position)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(_environmentalDetailContainer);
            rock.transform.position = position;
            
            float scale = _random.Next((int)(_rockMinScale * 100), (int)(_rockMaxScale * 100)) / 100f;
            rock.transform.localScale = Vector3.one * scale;

            // Random rotation for natural look
            rock.transform.rotation = Quaternion.Euler(
                _random.Next(0, 360),
                _random.Next(0, 360),
                _random.Next(0, 360)
            );

            var renderer = rock.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.4f, 0.4f);

            return rock;
        }

        /// <summary>
        /// Generate debris items for realism
        /// </summary>
        private IEnumerator GenerateDebris()
        {
            int debrisCount = _random.Next(_minDebrisCount, _maxDebrisCount + 1);

            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 debrisPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    0f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );

                GameObject debris = GenerateDebrisItem(debrisPosition);
                _generatedDetails.Add(debris);
                _environmentalDetails.Add(debris);

                yield return new WaitForSeconds(_debrisGenerationDelay);
            }

            OnDetailTypeGenerated?.Invoke("Debris", debrisCount);
        }

        /// <summary>
        /// Generate individual debris item
        /// </summary>
        private GameObject GenerateDebrisItem(Vector3 position)
        {
            var primitives = new PrimitiveType[] { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere };
            var primitive = primitives[_random.Next(primitives.Length)];

            GameObject debris = GameObject.CreatePrimitive(primitive);
            debris.name = "Debris";
            debris.transform.SetParent(_environmentalDetailContainer);
            debris.transform.position = position;
            
            float scale = _random.Next((int)(_debrisMinScale * 100), (int)(_debrisMaxScale * 100)) / 100f;
            debris.transform.localScale = Vector3.one * scale;

            var renderer = debris.GetComponent<Renderer>();
            renderer.material.color = new Color(
                _random.Next(20, 60) / 100f,
                _random.Next(20, 60) / 100f,
                _random.Next(20, 60) / 100f
            );

            return debris;
        }

        #endregion

        #region Furniture Generation

        /// <summary>
        /// Generate furniture for different room types
        /// </summary>
        private IEnumerator GenerateFurniture()
        {
            OnDetailPhaseStarted?.Invoke(DetailGenerationPhase.Furniture);
            ChimeraLogger.Log("Generating furniture", this);

            if (_generateOfficeFurniture)
            {
                yield return StartCoroutine(GenerateOfficeFurniture());
            }

            if (_generateBreakRoomFurniture)
            {
                yield return StartCoroutine(GenerateBreakRoomFurniture());
            }

            if (_generateStorageFurniture)
            {
                yield return StartCoroutine(GenerateStorageFurniture());
            }

            OnDetailPhaseCompleted?.Invoke(DetailGenerationPhase.Furniture);
        }

        /// <summary>
        /// Generate office furniture (desk, chair, etc.)
        /// </summary>
        private IEnumerator GenerateOfficeFurniture()
        {
            var officeRooms = _generatedRooms.Where(r => IsOfficeRoom(r.RoomType)).ToList();
            int furnitureGenerated = 0;

            foreach (var officeRoom in officeRooms)
            {
                var roomFurniture = new List<GameObject>();

                // Generate desk
                GameObject desk = GenerateDesk(officeRoom.Position);
                _generatedDetails.Add(desk);
                _furniture.Add(desk);
                roomFurniture.Add(desk);

                // Generate chair
                GameObject chair = GenerateChair(officeRoom.Position + new Vector3(0f, 0f, -1f));
                _generatedDetails.Add(chair);
                _furniture.Add(chair);
                roomFurniture.Add(chair);

                // Generate filing cabinet
                GameObject cabinet = GenerateFilingCabinet(officeRoom.Position + new Vector3(2f, 0f, 0f));
                _generatedDetails.Add(cabinet);
                _furniture.Add(cabinet);
                roomFurniture.Add(cabinet);

                _roomFurniture[officeRoom.RoomName] = roomFurniture;
                furnitureGenerated += roomFurniture.Count;

                yield return new WaitForSeconds(_furnitureGenerationDelay);
            }

            OnDetailTypeGenerated?.Invoke("Office Furniture", furnitureGenerated);
        }

        /// <summary>
        /// Generate desk
        /// </summary>
        private GameObject GenerateDesk(Vector3 position)
        {
            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(_furnitureContainer);
            desk.transform.position = position + new Vector3(0f, 0.4f, 0f);
            desk.transform.localScale = new Vector3(2f, 0.8f, 1f);

            var renderer = desk.GetComponent<Renderer>();
            renderer.material.color = new Color(0.6f, 0.4f, 0.2f); // Wood color

            return desk;
        }

        /// <summary>
        /// Generate chair
        /// </summary>
        private GameObject GenerateChair(Vector3 position)
        {
            GameObject chair = new GameObject("Chair");
            chair.transform.SetParent(_furnitureContainer);
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

            // Set colors
            var seatRenderer = seat.GetComponent<Renderer>();
            var backRenderer = back.GetComponent<Renderer>();
            Color chairColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
            seatRenderer.material.color = chairColor;
            backRenderer.material.color = chairColor;

            return chair;
        }

        /// <summary>
        /// Generate filing cabinet
        /// </summary>
        private GameObject GenerateFilingCabinet(Vector3 position)
        {
            GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "Filing Cabinet";
            cabinet.transform.SetParent(_furnitureContainer);
            cabinet.transform.position = position + new Vector3(0f, 0.6f, 0f);
            cabinet.transform.localScale = new Vector3(0.6f, 1.2f, 0.4f);

            var renderer = cabinet.GetComponent<Renderer>();
            renderer.material.color = new Color(0.8f, 0.8f, 0.8f); // Light gray

            return cabinet;
        }

        /// <summary>
        /// Generate break room furniture
        /// </summary>
        private IEnumerator GenerateBreakRoomFurniture()
        {
            var breakRooms = _generatedRooms.Where(r => IsBreakRoom(r.RoomType)).ToList();
            int furnitureGenerated = 0;

            foreach (var breakRoom in breakRooms)
            {
                var roomFurniture = new List<GameObject>();

                // Generate table
                GameObject table = GenerateTable(breakRoom.Position);
                _generatedDetails.Add(table);
                _furniture.Add(table);
                roomFurniture.Add(table);

                // Generate chairs around table
                for (int i = 0; i < _chairsPerTable; i++)
                {
                    Vector3 chairPos = breakRoom.Position + new Vector3(
                        Mathf.Sin(i * 90f * Mathf.Deg2Rad) * 1.5f,
                        0f,
                        Mathf.Cos(i * 90f * Mathf.Deg2Rad) * 1.5f
                    );

                    GameObject chair = GenerateChair(chairPos);
                    _generatedDetails.Add(chair);
                    _furniture.Add(chair);
                    roomFurniture.Add(chair);
                }

                _roomFurniture[breakRoom.RoomName] = roomFurniture;
                furnitureGenerated += roomFurniture.Count;

                yield return new WaitForSeconds(_furnitureGenerationDelay);
            }

            OnDetailTypeGenerated?.Invoke("Break Room Furniture", furnitureGenerated);
        }

        /// <summary>
        /// Generate table
        /// </summary>
        private GameObject GenerateTable(Vector3 position)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "Table";
            table.transform.SetParent(_furnitureContainer);
            table.transform.position = position + new Vector3(0f, 0.4f, 0f);
            table.transform.localScale = new Vector3(2f, 0.1f, 2f);

            var renderer = table.GetComponent<Renderer>();
            renderer.material.color = new Color(0.7f, 0.5f, 0.3f); // Wood color

            return table;
        }

        /// <summary>
        /// Generate storage furniture for various rooms
        /// </summary>
        private IEnumerator GenerateStorageFurniture()
        {
            var storageRooms = _generatedRooms.Where(r => IsStorageRoom(r.RoomType)).ToList();
            int furnitureGenerated = 0;

            foreach (var storageRoom in storageRooms)
            {
                var roomFurniture = new List<GameObject>();

                // Generate shelving units
                for (int i = 0; i < 3; i++)
                {
                    Vector3 shelfPos = storageRoom.Position + new Vector3(
                        (i - 1) * 2f,
                        0f,
                        -storageRoom.Dimensions.z / 2f + 0.5f
                    );

                    GameObject shelf = GenerateShelf(shelfPos);
                    _generatedDetails.Add(shelf);
                    _furniture.Add(shelf);
                    roomFurniture.Add(shelf);
                }

                _roomFurniture[storageRoom.RoomName] = roomFurniture;
                furnitureGenerated += roomFurniture.Count;

                yield return new WaitForSeconds(_furnitureGenerationDelay);
            }

            OnDetailTypeGenerated?.Invoke("Storage Furniture", furnitureGenerated);
        }

        /// <summary>
        /// Generate shelving unit
        /// </summary>
        private GameObject GenerateShelf(Vector3 position)
        {
            GameObject shelf = new GameObject("Shelf Unit");
            shelf.transform.SetParent(_furnitureContainer);
            shelf.transform.position = position;

            // Create multiple shelves
            for (int i = 0; i < 4; i++)
            {
                GameObject shelfLevel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelfLevel.name = $"Shelf Level {i + 1}";
                shelfLevel.transform.SetParent(shelf.transform);
                shelfLevel.transform.localPosition = new Vector3(0f, i * 0.5f + 0.1f, 0f);
                shelfLevel.transform.localScale = new Vector3(1.8f, 0.05f, 0.4f);

                var renderer = shelfLevel.GetComponent<Renderer>();
                renderer.material.color = new Color(0.5f, 0.3f, 0.1f); // Dark wood
            }

            return shelf;
        }

        #endregion

        #region Decorations Generation

        /// <summary>
        /// Generate decorative elements
        /// </summary>
        private IEnumerator GenerateDecorations()
        {
            OnDetailPhaseStarted?.Invoke(DetailGenerationPhase.Decorations);
            ChimeraLogger.Log("Generating decorations", this);

            yield return StartCoroutine(GenerateWallDecorations());
            yield return StartCoroutine(GeneratePlants());

            OnDetailPhaseCompleted?.Invoke(DetailGenerationPhase.Decorations);
        }

        /// <summary>
        /// Generate wall decorations
        /// </summary>
        private IEnumerator GenerateWallDecorations()
        {
            // Simple wall decorations for office rooms
            var officeRooms = _generatedRooms.Where(r => IsOfficeRoom(r.RoomType));
            int decorationsGenerated = 0;

            foreach (var room in officeRooms)
            {
                GameObject wallArt = GenerateWallArt(room.Position + new Vector3(0f, 1.5f, room.Dimensions.z / 2f - 0.1f));
                _generatedDetails.Add(wallArt);
                _decorations.Add(wallArt);
                decorationsGenerated++;

                yield return new WaitForSeconds(0.01f);
            }

            OnDetailTypeGenerated?.Invoke("Wall Decorations", decorationsGenerated);
        }

        /// <summary>
        /// Generate wall art
        /// </summary>
        private GameObject GenerateWallArt(Vector3 position)
        {
            GameObject wallArt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallArt.name = "Wall Art";
            wallArt.transform.SetParent(_decorationContainer);
            wallArt.transform.position = position;
            wallArt.transform.localScale = new Vector3(0.8f, 0.6f, 0.02f);

            var renderer = wallArt.GetComponent<Renderer>();
            renderer.material.color = new Color(_random.Next(0, 100) / 100f, _random.Next(0, 100) / 100f, _random.Next(0, 100) / 100f);

            return wallArt;
        }

        /// <summary>
        /// Generate decorative plants
        /// </summary>
        private IEnumerator GeneratePlants()
        {
            var commonRooms = _generatedRooms.Where(r => IsCommonRoom(r.RoomType));
            int plantsGenerated = 0;

            foreach (var room in commonRooms)
            {
                GameObject plant = GenerateDecorativePlant(room.Position + new Vector3(room.Dimensions.x / 2f - 0.5f, 0f, room.Dimensions.z / 2f - 0.5f));
                _generatedDetails.Add(plant);
                _decorations.Add(plant);
                plantsGenerated++;

                yield return new WaitForSeconds(0.01f);
            }

            OnDetailTypeGenerated?.Invoke("Decorative Plants", plantsGenerated);
        }

        /// <summary>
        /// Generate decorative plant
        /// </summary>
        private GameObject GenerateDecorativePlant(Vector3 position)
        {
            GameObject plant = new GameObject("Decorative Plant");
            plant.transform.SetParent(_decorationContainer);
            plant.transform.position = position;

            // Pot
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Pot";
            pot.transform.SetParent(plant.transform);
            pot.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            pot.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
            pot.GetComponent<Renderer>().material.color = new Color(0.4f, 0.2f, 0.1f);

            // Plant
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.SetParent(plant.transform);
            foliage.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            foliage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            foliage.GetComponent<Renderer>().material.color = new Color(0.2f, 0.6f, 0.2f);

            return plant;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if position is near a building
        /// </summary>
        private bool IsNearBuilding(Vector3 position, float minDistance)
        {
            // Simple distance check - can be expanded with actual building collision detection
            return _generatedRooms.Any(room => Vector3.Distance(position, room.Position) < minDistance);
        }

        /// <summary>
        /// Check if room is an office room
        /// </summary>
        private bool IsOfficeRoom(string roomType)
        {
            return roomType.Contains("Office") || roomType.Contains("Admin") || roomType.Contains("Manager");
        }

        /// <summary>
        /// Check if room is a break room
        /// </summary>
        private bool IsBreakRoom(string roomType)
        {
            return roomType.Contains("Break") || roomType.Contains("Lunch") || roomType.Contains("Rest");
        }

        /// <summary>
        /// Check if room is a storage room
        /// </summary>
        private bool IsStorageRoom(string roomType)
        {
            return roomType.Contains("Storage") || roomType.Contains("Supply") || roomType.Contains("Warehouse");
        }

        /// <summary>
        /// Check if room is a common area
        /// </summary>
        private bool IsCommonRoom(string roomType)
        {
            return roomType.Contains("Lobby") || roomType.Contains("Reception") || roomType.Contains("Common") || IsBreakRoom(roomType);
        }

        /// <summary>
        /// Generate completion report
        /// </summary>
        private void GenerateCompletionReport()
        {
            var report = new DetailGenerationReport
            {
                TotalDetailsGenerated = _generatedDetails.Count,
                EnvironmentalDetailsCount = _environmentalDetails.Count,
                FurnitureCount = _furniture.Count,
                DecorationsCount = _decorations.Count,
                RoomsFurnished = _roomFurniture.Count,
                GenerationTime = Time.realtimeSinceStartup,
                FacilityType = _sceneType,
                FacilitySize = _facilitySize
            };

            OnDetailGenerationCompleted?.Invoke(report);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get detail count by type
        /// </summary>
        public int GetDetailCount(DetailType detailType)
        {
            return detailType switch
            {
                DetailType.EnvironmentalDetails => _environmentalDetails.Count,
                DetailType.Furniture => _furniture.Count,
                DetailType.Decorations => _decorations.Count,
                _ => 0
            };
        }

        /// <summary>
        /// Get furniture for specific room
        /// </summary>
        public List<GameObject> GetRoomFurniture(string roomName)
        {
            return _roomFurniture.ContainsKey(roomName) ? 
                new List<GameObject>(_roomFurniture[roomName]) : 
                new List<GameObject>();
        }

        /// <summary>
        /// Update detail configuration
        /// </summary>
        public void UpdateDetailConfiguration(DetailGenerationConfig config)
        {
            _enableEnvironmentalDetails = config.EnableEnvironmentalDetails;
            _enableFurniture = config.EnableFurniture;
            _enableDecorations = config.EnableDecorations;
            _minRockCount = config.MinRockCount;
            _maxRockCount = config.MaxRockCount;
            
            ChimeraLogger.Log("Detail generation configuration updated", this);
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
    /// Detail generation phases
    /// </summary>
    public enum DetailGenerationPhase
    {
        EnvironmentalDetails,
        Furniture,
        Decorations
    }

    /// <summary>
    /// Detail types for categorization
    /// </summary>
    public enum DetailType
    {
        EnvironmentalDetails,
        Furniture,
        Decorations
    }

    /// <summary>
    /// Detail generation configuration
    /// </summary>
    [System.Serializable]
    public class DetailGenerationConfig
    {
        public bool EnableEnvironmentalDetails = true;
        public bool EnableFurniture = true;
        public bool EnableDecorations = true;
        public int MinRockCount = 10;
        public int MaxRockCount = 25;
        public int MinDebrisCount = 5;
        public int MaxDebrisCount = 15;
    }

    /// <summary>
    /// Detail generation completion report
    /// </summary>
    [System.Serializable]
    public class DetailGenerationReport
    {
        public int TotalDetailsGenerated;
        public int EnvironmentalDetailsCount;
        public int FurnitureCount;
        public int DecorationsCount;
        public int RoomsFurnished;
        public float GenerationTime;
        public FacilitySceneType FacilityType;
        public Vector2Int FacilitySize;
    }

    #endregion
}