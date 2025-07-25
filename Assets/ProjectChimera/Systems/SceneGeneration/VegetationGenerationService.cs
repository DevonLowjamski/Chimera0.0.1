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
    /// Vegetation Generation Service - Dedicated service for plants, landscaping, and organic elements generation
    /// Extracted from ProceduralSceneGenerator to provide focused vegetation generation functionality
    /// Handles indoor/outdoor plants, wild vegetation, trees, grass, landscaping, and decorative elements
    /// </summary>
    public class VegetationGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Vegetation Configuration")]
        [SerializeField] private bool _enableVegetationGeneration = true;
        [SerializeField] private bool _enableIndoorPlants = true;
        [SerializeField] private bool _enableWildVegetation = true;
        [SerializeField] private bool _enableLandscaping = true;

        [Header("Plant Configuration")]
        [SerializeField] private int _plantsPerRoom = 20;
        [SerializeField] private float _vegetationDensity = 1.5f;
        [SerializeField] private GameObject[] _plantPrefabs;

        [Header("Wild Vegetation")]
        [SerializeField] private bool _enableWildGrass = true;
        [SerializeField] private bool _enableWildPlants = true;
        [SerializeField] private bool _enableTrees = true;
        [SerializeField] private float _grassDensity = 2.0f;
        [SerializeField] private int _minWildPlants = 20;
        [SerializeField] private int _maxWildPlants = 50;

        [Header("Landscaping")]
        [SerializeField] private bool _enableFlowerBeds = true;
        [SerializeField] private bool _enableHedges = true;
        [SerializeField] private bool _enableDecorations = true;
        [SerializeField] private int _minFlowerBeds = 3;
        [SerializeField] private int _maxFlowerBeds = 8;

        [Header("Tree Configuration")]
        [SerializeField] private int _minTrees = 5;
        [SerializeField] private int _maxTrees = 15;
        [SerializeField] private float _treeSafeDistance = 8f;

        // Service state
        private bool _isInitialized = false;
        private System.Random _random;
        private List<GameObject> _generatedVegetationObjects = new List<GameObject>();
        private List<RoomLayout> _availableRooms = new List<RoomLayout>();
        private Transform _containerTransform;
        private Vector2Int _facilitySize;

        // Vegetation generators
        private PlantGenerator _plantGenerator;
        private WildVegetationGenerator _wildVegetationGenerator;
        private LandscapingGenerator _landscapingGenerator;

        // Events for vegetation generation
        public static event System.Action<VegetationGenerationCompleteArgs> OnVegetationGenerationComplete;
        public static event System.Action<string, float> OnVegetationGenerationProgress;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Vegetation Generation Service";
        public IReadOnlyList<GameObject> GeneratedVegetationObjects => _generatedVegetationObjects;
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
            _generatedVegetationObjects = new List<GameObject>();
            _availableRooms = new List<RoomLayout>();
            _plantGenerator = new PlantGenerator();
            _wildVegetationGenerator = new WildVegetationGenerator();
            _landscapingGenerator = new LandscapingGenerator();
        }

        public void InitializeService(int seed = 0, Transform container = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("VegetationGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                InitializeGenerators();
                
                _isInitialized = true;
                ChimeraLogger.Log("VegetationGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize VegetationGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedVegetation();
            _generatedVegetationObjects.Clear();
            _availableRooms.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("VegetationGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_plantsPerRoom <= 0)
            {
                ChimeraLogger.LogWarning("Plants per room must be positive, using default 20", this);
                _plantsPerRoom = 20;
            }

            if (_vegetationDensity <= 0f)
            {
                ChimeraLogger.LogWarning("Vegetation density must be positive, using default 1.5", this);
                _vegetationDensity = 1.5f;
            }

            if (_minWildPlants > _maxWildPlants)
            {
                ChimeraLogger.LogWarning("Min wild plants greater than max, adjusting", this);
                _minWildPlants = Mathf.Min(_minWildPlants, _maxWildPlants);
            }
        }

        private void InitializeGenerators()
        {
            _plantGenerator.Initialize(_random, _plantPrefabs);
            _wildVegetationGenerator.Initialize(_random, _vegetationDensity, _grassDensity);
            _landscapingGenerator.Initialize(_random);
        }

        #endregion

        #region Vegetation Generation Interface

        /// <summary>
        /// Generate vegetation based on facility scene type and available rooms
        /// </summary>
        public IEnumerator GenerateVegetation(FacilitySceneType sceneType, Vector2Int facilitySize, List<RoomLayout> rooms = null, VegetationGenerationSettings settings = null)
        {
            if (!_isInitialized || !_enableVegetationGeneration)
            {
                ChimeraLogger.LogWarning("VegetationGenerationService not initialized or disabled", this);
                yield break;
            }

            _facilitySize = facilitySize;
            _availableRooms = rooms ?? new List<RoomLayout>();

            OnVegetationGenerationProgress?.Invoke("Starting vegetation generation", 0f);

            switch (sceneType)
            {
                case FacilitySceneType.IndoorFacility:
                    yield return StartCoroutine(GenerateIndoorVegetation());
                    break;
                    
                case FacilitySceneType.OutdoorFarm:
                    yield return StartCoroutine(GenerateOutdoorVegetation());
                    break;
                    
                case FacilitySceneType.Greenhouse:
                    yield return StartCoroutine(GenerateGreenhouseVegetation());
                    break;
                    
                case FacilitySceneType.MixedFacility:
                    yield return StartCoroutine(GenerateMixedVegetation());
                    break;
                    
                case FacilitySceneType.UrbanRooftop:
                    yield return StartCoroutine(GenerateUrbanVegetation());
                    break;
            }

            OnVegetationGenerationComplete?.Invoke(new VegetationGenerationCompleteArgs
            {
                SceneType = sceneType,
                FacilitySize = facilitySize,
                GeneratedObjects = new List<GameObject>(_generatedVegetationObjects),
                Success = true
            });

            ChimeraLogger.Log($"Vegetation generation completed for {sceneType}", this);
        }

        #endregion

        #region Indoor Vegetation Generation

        /// <summary>
        /// Generate indoor facility vegetation
        /// </summary>
        private IEnumerator GenerateIndoorVegetation()
        {
            OnVegetationGenerationProgress?.Invoke("Generating indoor plants", 0.3f);
            
            if (_enableIndoorPlants)
            {
                yield return StartCoroutine(GenerateIndoorPlants());
            }
            
            OnVegetationGenerationProgress?.Invoke("Indoor vegetation complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate plants in indoor rooms
        /// </summary>
        private IEnumerator GenerateIndoorPlants()
        {
            foreach (var room in _availableRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomPlants(room));
                }
            }
        }

        /// <summary>
        /// Generate plants for specific room
        /// </summary>
        private IEnumerator GenerateRoomPlants(RoomLayout room)
        {
            int plantsToGenerate = Mathf.Min(_plantsPerRoom, Mathf.RoundToInt((room.Dimensions.x * room.Dimensions.z) / 2f));
            
            for (int i = 0; i < plantsToGenerate; i++)
            {
                Vector3 plantPosition = GeneratePlantPosition(room, i, plantsToGenerate);
                GameObject plant = _plantGenerator.GeneratePlant(plantPosition, room);
                
                if (plant != null)
                {
                    plant.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(plant);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate plant position within room using grid layout
        /// </summary>
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

        #endregion

        #region Outdoor Vegetation Generation

        /// <summary>
        /// Generate outdoor facility vegetation
        /// </summary>
        private IEnumerator GenerateOutdoorVegetation()
        {
            OnVegetationGenerationProgress?.Invoke("Generating wild grass", 0.2f);
            
            if (_enableWildVegetation)
            {
                if (_enableWildGrass)
                {
                    yield return StartCoroutine(GenerateWildGrass());
                }
                
                OnVegetationGenerationProgress?.Invoke("Generating wild plants", 0.5f);
                
                if (_enableWildPlants)
                {
                    yield return StartCoroutine(GenerateWildPlants());
                }
                
                OnVegetationGenerationProgress?.Invoke("Generating trees", 0.7f);
                
                if (_enableTrees)
                {
                    yield return StartCoroutine(GenerateTrees());
                }
            }
            
            OnVegetationGenerationProgress?.Invoke("Adding landscaping", 0.9f);
            
            if (_enableLandscaping)
            {
                yield return StartCoroutine(GenerateLandscaping());
            }
            
            OnVegetationGenerationProgress?.Invoke("Outdoor vegetation complete", 1.0f);
        }

        /// <summary>
        /// Generate wild grass across facility
        /// </summary>
        private IEnumerator GenerateWildGrass()
        {
            int grassCount = Mathf.RoundToInt(_facilitySize.magnitude * _grassDensity);
            
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
                
                GameObject grass = _wildVegetationGenerator.GenerateGrass(grassPosition);
                if (grass != null)
                {
                    grass.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(grass);
                }
                
                if (i % 10 == 0)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

        /// <summary>
        /// Generate wild plants scattered around facility
        /// </summary>
        private IEnumerator GenerateWildPlants()
        {
            int plantCount = _random.Next(_minWildPlants, _maxWildPlants + 1);
            
            for (int i = 0; i < plantCount; i++)
            {
                Vector3 plantPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                if (IsNearBuilding(plantPosition, 5f))
                    continue;
                
                GameObject wildPlant = _wildVegetationGenerator.GenerateWildPlant(plantPosition);
                if (wildPlant != null)
                {
                    wildPlant.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(wildPlant);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate trees around facility
        /// </summary>
        private IEnumerator GenerateTrees()
        {
            int treeCount = _random.Next(_minTrees, _maxTrees + 1);
            
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 treePosition = new Vector3(
                    _random.Next(-_facilitySize.x / 2, _facilitySize.x / 2),
                    0f,
                    _random.Next(-_facilitySize.y / 2, _facilitySize.y / 2)
                );
                
                if (IsNearBuilding(treePosition, _treeSafeDistance))
                    continue;
                
                GameObject tree = _wildVegetationGenerator.GenerateTree(treePosition);
                if (tree != null)
                {
                    tree.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(tree);
                }
                
                yield return new WaitForSeconds(0.05f);
            }
        }

        #endregion

        #region Landscaping Generation

        /// <summary>
        /// Generate landscaping elements
        /// </summary>
        private IEnumerator GenerateLandscaping()
        {
            if (_enableFlowerBeds)
            {
                yield return StartCoroutine(GenerateFlowerBeds());
            }
            
            if (_enableHedges)
            {
                yield return StartCoroutine(GenerateHedges());
            }
            
            if (_enableDecorations)
            {
                yield return StartCoroutine(GenerateDecorations());
            }
        }

        /// <summary>
        /// Generate flower beds
        /// </summary>
        private IEnumerator GenerateFlowerBeds()
        {
            int bedCount = _random.Next(_minFlowerBeds, _maxFlowerBeds + 1);
            
            for (int i = 0; i < bedCount; i++)
            {
                Vector3 bedPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    0.1f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject flowerBed = _landscapingGenerator.GenerateFlowerBed(bedPosition);
                if (flowerBed != null)
                {
                    flowerBed.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(flowerBed);
                }
                
                yield return new WaitForSeconds(0.03f);
            }
        }

        /// <summary>
        /// Generate hedges
        /// </summary>
        private IEnumerator GenerateHedges()
        {
            yield return StartCoroutine(GeneratePerimeterHedges());
        }

        /// <summary>
        /// Generate perimeter hedges
        /// </summary>
        private IEnumerator GeneratePerimeterHedges()
        {
            var hedgePositions = new Vector3[]
            {
                new Vector3(_facilitySize.x / 2f + 5f, 0.5f, 0f),
                new Vector3(-_facilitySize.x / 2f - 5f, 0.5f, 0f),
            };
            
            foreach (var position in hedgePositions)
            {
                GameObject hedge = _landscapingGenerator.GenerateHedge(position, _facilitySize.y);
                if (hedge != null)
                {
                    hedge.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(hedge);
                }
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate decorative elements
        /// </summary>
        private IEnumerator GenerateDecorations()
        {
            yield return StartCoroutine(GenerateBenches());
            yield return StartCoroutine(GenerateSigns());
        }

        /// <summary>
        /// Generate benches
        /// </summary>
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
                
                GameObject bench = _landscapingGenerator.GenerateBench(benchPosition);
                if (bench != null)
                {
                    bench.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(bench);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate signs
        /// </summary>
        private IEnumerator GenerateSigns()
        {
            Vector3 signPosition = new Vector3(0f, 2f, -_facilitySize.y / 2f - 2f);
            GameObject facilitySign = _landscapingGenerator.GenerateSign("Cannabis Cultivation Facility", signPosition);
            
            if (facilitySign != null)
            {
                facilitySign.transform.SetParent(_containerTransform);
                _generatedVegetationObjects.Add(facilitySign);
            }
            
            yield return null;
        }

        #endregion

        #region Specialized Vegetation Generation

        /// <summary>
        /// Generate greenhouse vegetation
        /// </summary>
        private IEnumerator GenerateGreenhouseVegetation()
        {
            OnVegetationGenerationProgress?.Invoke("Generating greenhouse plants", 0.5f);
            
            // Focus on controlled indoor growing
            if (_enableIndoorPlants)
            {
                yield return StartCoroutine(GenerateIndoorPlants());
            }
            
            // Add some decorative landscaping around greenhouse
            if (_enableLandscaping)
            {
                yield return StartCoroutine(GenerateGreenhouseLandscaping());
            }
            
            OnVegetationGenerationProgress?.Invoke("Greenhouse vegetation complete", 1.0f);
        }

        /// <summary>
        /// Generate mixed facility vegetation
        /// </summary>
        private IEnumerator GenerateMixedVegetation()
        {
            OnVegetationGenerationProgress?.Invoke("Generating mixed vegetation - indoor", 0.3f);
            
            // Indoor plants
            if (_enableIndoorPlants)
            {
                yield return StartCoroutine(GenerateIndoorPlants());
            }
            
            OnVegetationGenerationProgress?.Invoke("Generating mixed vegetation - outdoor", 0.7f);
            
            // Outdoor vegetation
            if (_enableWildVegetation)
            {
                yield return StartCoroutine(GenerateWildGrass());
                yield return StartCoroutine(GenerateWildPlants());
            }
            
            if (_enableLandscaping)
            {
                yield return StartCoroutine(GenerateLandscaping());
            }
            
            OnVegetationGenerationProgress?.Invoke("Mixed vegetation complete", 1.0f);
        }

        /// <summary>
        /// Generate urban rooftop vegetation
        /// </summary>
        private IEnumerator GenerateUrbanVegetation()
        {
            OnVegetationGenerationProgress?.Invoke("Generating rooftop plants", 0.4f);
            
            // Container plants and rooftop gardens
            yield return StartCoroutine(GenerateRooftopContainers());
            
            OnVegetationGenerationProgress?.Invoke("Adding urban landscaping", 0.8f);
            
            yield return StartCoroutine(GenerateUrbanLandscaping());
            
            OnVegetationGenerationProgress?.Invoke("Urban vegetation complete", 1.0f);
        }

        /// <summary>
        /// Generate greenhouse-specific landscaping
        /// </summary>
        private IEnumerator GenerateGreenhouseLandscaping()
        {
            // Add paths and decorative elements around greenhouse structures
            int decorativeCount = _random.Next(3, 6);
            
            for (int i = 0; i < decorativeCount; i++)
            {
                Vector3 position = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    0.1f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject decorative = _landscapingGenerator.GenerateGreenhouseDecoration(position, i);
                if (decorative != null)
                {
                    decorative.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(decorative);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate rooftop container plants
        /// </summary>
        private IEnumerator GenerateRooftopContainers()
        {
            int containerCount = _random.Next(8, 16);
            
            for (int i = 0; i < containerCount; i++)
            {
                Vector3 containerPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 3, _facilitySize.x / 3),
                    1f,
                    _random.Next(-_facilitySize.y / 3, _facilitySize.y / 3)
                );
                
                GameObject container = _landscapingGenerator.GenerateRooftopContainer(containerPosition, i);
                if (container != null)
                {
                    container.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(container);
                }
                
                yield return new WaitForSeconds(0.03f);
            }
        }

        /// <summary>
        /// Generate urban landscaping
        /// </summary>
        private IEnumerator GenerateUrbanLandscaping()
        {
            // Urban-specific decorative elements
            yield return StartCoroutine(GenerateUrbanPlanters());
            yield return StartCoroutine(GenerateUrbanSeating());
        }

        /// <summary>
        /// Generate urban planters
        /// </summary>
        private IEnumerator GenerateUrbanPlanters()
        {
            int planterCount = _random.Next(4, 8);
            
            for (int i = 0; i < planterCount; i++)
            {
                Vector3 planterPosition = new Vector3(
                    ((i % 2) - 0.5f) * _facilitySize.x / 2f,
                    0.5f,
                    ((i / 2) - planterCount / 4f) * _facilitySize.y / 2f
                );
                
                GameObject planter = _landscapingGenerator.GenerateUrbanPlanter(planterPosition);
                if (planter != null)
                {
                    planter.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(planter);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate urban seating
        /// </summary>
        private IEnumerator GenerateUrbanSeating()
        {
            int seatingCount = _random.Next(2, 4);
            
            for (int i = 0; i < seatingCount; i++)
            {
                Vector3 seatingPosition = new Vector3(
                    _random.Next(-_facilitySize.x / 4, _facilitySize.x / 4),
                    1f,
                    _random.Next(-_facilitySize.y / 4, _facilitySize.y / 4)
                );
                
                GameObject seating = _landscapingGenerator.GenerateUrbanSeating(seatingPosition);
                if (seating != null)
                {
                    seating.transform.SetParent(_containerTransform);
                    _generatedVegetationObjects.Add(seating);
                }
                
                yield return new WaitForSeconds(0.02f);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if room type is a grow room
        /// </summary>
        private bool IsGrowRoom(string roomType)
        {
            return roomType.Contains("Grow") || roomType.Contains("Vegetative") || 
                   roomType.Contains("Flowering") || roomType.Contains("Nursery");
        }

        /// <summary>
        /// Check if position is too close to existing buildings
        /// </summary>
        private bool IsNearBuilding(Vector3 position, float minDistance)
        {
            foreach (var room in _availableRooms)
            {
                if (Vector3.Distance(position, room.Position) < minDistance)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clear all generated vegetation objects
        /// </summary>
        public void ClearGeneratedVegetation()
        {
            foreach (var obj in _generatedVegetationObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
        }

        /// <summary>
        /// Get vegetation generation statistics
        /// </summary>
        public VegetationGenerationStats GetGenerationStats()
        {
            return new VegetationGenerationStats
            {
                TotalObjectsGenerated = _generatedVegetationObjects.Count,
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
    /// Plant generator for cultivation plants
    /// </summary>
    public class PlantGenerator
    {
        private System.Random _random;
        private GameObject[] _plantPrefabs;

        public void Initialize(System.Random random, GameObject[] plantPrefabs)
        {
            _random = random;
            _plantPrefabs = plantPrefabs;
        }

        public GameObject GeneratePlant(Vector3 position, RoomLayout room)
        {
            GameObject plant;
            
            if (_plantPrefabs != null && _plantPrefabs.Length > 0)
            {
                var prefab = _plantPrefabs[_random.Next(_plantPrefabs.Length)];
                plant = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                plant = GenerateProceduralPlant(position);
            }
            
            // Add interactive plant component if not already present (placeholder)
            // if (plant.GetComponent<InteractivePlantComponent>() == null)
            // {
            //     plant.AddComponent<InteractivePlantComponent>();
            // }
            
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
    }

    /// <summary>
    /// Wild vegetation generator for outdoor elements
    /// </summary>
    public class WildVegetationGenerator
    {
        private System.Random _random;
        private float _vegetationDensity;
        private float _grassDensity;

        public void Initialize(System.Random random, float vegetationDensity, float grassDensity)
        {
            _random = random;
            _vegetationDensity = vegetationDensity;
            _grassDensity = grassDensity;
        }

        public GameObject GenerateGrass(Vector3 position)
        {
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grass.name = "Wild Grass";
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

        public GameObject GenerateWildPlant(Vector3 position)
        {
            GameObject plant = new GameObject("Wild Plant");
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

        public GameObject GenerateTree(Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
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
    }

    /// <summary>
    /// Landscaping generator for decorative elements
    /// </summary>
    public class LandscapingGenerator
    {
        private System.Random _random;

        public void Initialize(System.Random random)
        {
            _random = random;
        }

        public GameObject GenerateFlowerBed(Vector3 position)
        {
            GameObject bed = new GameObject("Flower Bed");
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

        public GameObject GenerateHedge(Vector3 position, float length)
        {
            GameObject hedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hedge.name = "Hedge";
            hedge.transform.position = position;
            hedge.transform.localScale = new Vector3(1f, 1f, length);
            
            var renderer = hedge.GetComponent<Renderer>();
            renderer.material.color = new Color(0.2f, 0.5f, 0.2f);
            
            return hedge;
        }

        public GameObject GenerateBench(Vector3 position)
        {
            GameObject bench = new GameObject("Bench");
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

        public GameObject GenerateSign(string text, Vector3 position)
        {
            GameObject sign = new GameObject("Facility Sign");
            sign.transform.position = position;
            
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

        public GameObject GenerateGreenhouseDecoration(Vector3 position, int index)
        {
            GameObject decoration = new GameObject($"Greenhouse Decoration {index + 1}");
            decoration.transform.position = position;
            
            // Create small decorative planter
            GameObject planter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            planter.name = "Decorative Planter";
            planter.transform.SetParent(decoration.transform);
            planter.transform.localPosition = Vector3.zero;
            planter.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
            
            return decoration;
        }

        public GameObject GenerateRooftopContainer(Vector3 position, int index)
        {
            GameObject container = new GameObject($"Rooftop Container {index + 1}");
            container.transform.position = position;
            
            // Container base
            GameObject containerBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            containerBase.name = "Container Base";
            containerBase.transform.SetParent(container.transform);
            containerBase.transform.localPosition = Vector3.zero;
            containerBase.transform.localScale = new Vector3(1.5f, 0.5f, 1f);
            
            // Plant in container
            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Container Plant";
            plant.transform.SetParent(container.transform);
            plant.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            plant.transform.localScale = Vector3.one * 0.3f;
            
            var plantRenderer = plant.GetComponent<Renderer>();
            plantRenderer.material.color = new Color(0.2f, 0.7f, 0.2f);
            
            return container;
        }

        public GameObject GenerateUrbanPlanter(Vector3 position)
        {
            GameObject planter = new GameObject("Urban Planter");
            planter.transform.position = position;
            
            // Planter box
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Planter Box";
            box.transform.SetParent(planter.transform);
            box.transform.localPosition = Vector3.zero;
            box.transform.localScale = new Vector3(2f, 0.8f, 0.8f);
            
            // Urban plants
            int plantCount = _random.Next(2, 5);
            for (int i = 0; i < plantCount; i++)
            {
                GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                plant.name = $"Urban Plant {i + 1}";
                plant.transform.SetParent(planter.transform);
                plant.transform.localPosition = new Vector3(
                    ((i - plantCount / 2f) * 0.4f),
                    0.6f,
                    0f
                );
                plant.transform.localScale = Vector3.one * 0.2f;
                
                var renderer = plant.GetComponent<Renderer>();
                renderer.material.color = new Color(0.1f, _random.Next(50, 90) / 100f, 0.1f);
            }
            
            return planter;
        }

        public GameObject GenerateUrbanSeating(Vector3 position)
        {
            GameObject seating = new GameObject("Urban Seating");
            seating.transform.position = position;
            
            // Modern bench design
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Urban Bench";
            bench.transform.SetParent(seating.transform);
            bench.transform.localPosition = Vector3.zero;
            bench.transform.localScale = new Vector3(2.5f, 0.1f, 0.6f);
            
            // Support legs
            for (int i = 0; i < 2; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = $"Bench Leg {i + 1}";
                leg.transform.SetParent(seating.transform);
                leg.transform.localPosition = new Vector3(((i * 2) - 1f) * 0.8f, -0.3f, 0f);
                leg.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
            }
            
            return seating;
        }
    }

    #endregion

    #region Data Structures

    // RoomLayout class is defined in BuildingGenerationService.cs

    /// <summary>
    /// Vegetation generation settings
    /// </summary>
    [System.Serializable]
    public class VegetationGenerationSettings
    {
        public bool EnableWildVegetation = true;
        public bool EnableLandscaping = true;
        public float CustomVegetationDensity = 1.5f;
        public int MaxPlantsPerRoom = 25;
    }

    /// <summary>
    /// Vegetation generation completion arguments
    /// </summary>
    [System.Serializable]
    public class VegetationGenerationCompleteArgs
    {
        public FacilitySceneType SceneType;
        public Vector2Int FacilitySize;
        public List<GameObject> GeneratedObjects;
        public bool Success;
    }

    /// <summary>
    /// Vegetation generation statistics
    /// </summary>
    [System.Serializable]
    public class VegetationGenerationStats
    {
        public int TotalObjectsGenerated;
        public bool ServiceInitialized;
        public DateTime LastGenerationTime;
    }

    #endregion
}