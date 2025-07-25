using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Facilities;

namespace ProjectChimera.Systems.SceneGeneration
{
    /// <summary>
    /// Terrain Generation Service - Dedicated service for terrain, ground, and natural features generation
    /// Extracted from ProceduralSceneGenerator to provide focused terrain generation functionality
    /// Handles floor creation, terrain sculpting, natural features, and outdoor environments
    /// </summary>
    public class TerrainGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Terrain Configuration")]
        [SerializeField] private bool _enableTerrainGeneration = true;
        [SerializeField] private bool _enableNaturalFeatures = true;
        [SerializeField] private bool _enablePathGeneration = true;
        [SerializeField] private bool _enableWaterFeatures = true;

        [Header("Terrain Materials")]
        [SerializeField] private Material[] _floorMaterials;
        [SerializeField] private Material[] _wallMaterials;
        [SerializeField] private Material[] _roofMaterials;
        [SerializeField] private Material[] _groundMaterials;

        [Header("Natural Features")]
        [SerializeField] private int _minHillCount = 2;
        [SerializeField] private int _maxHillCount = 5;
        [SerializeField] private int _minHillSize = 5;
        [SerializeField] private int _maxHillSize = 15;
        [SerializeField] private float _waterFeatureChance = 0.3f;

        [Header("Path Configuration")]
        [SerializeField] private float _pathWidth = 2f;
        [SerializeField] private float _pathHeight = 0.1f;
        [SerializeField] private Material _pathMaterial;

        // Service state
        private bool _isInitialized = false;
        private System.Random _random;
        private List<GameObject> _generatedTerrainObjects = new List<GameObject>();
        private Transform _containerTransform;

        // Terrain generators (simplified for extraction)
        private TerrainHeightmapGenerator _heightmapGenerator;
        private NaturalFeatureGenerator _featureGenerator;

        // Events for terrain generation
        public static event System.Action<TerrainGenerationCompleteArgs> OnTerrainGenerationComplete;
        public static event System.Action<string, float> OnTerrainGenerationProgress;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Terrain Generation Service";
        public IReadOnlyList<GameObject> GeneratedTerrainObjects => _generatedTerrainObjects;
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
            _generatedTerrainObjects = new List<GameObject>();
            _heightmapGenerator = new TerrainHeightmapGenerator();
            _featureGenerator = new NaturalFeatureGenerator();
        }

        public void InitializeService(int seed = 0, Transform container = null)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("TerrainGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                InitializeGenerators();
                
                _isInitialized = true;
                ChimeraLogger.Log("TerrainGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize TerrainGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedTerrain();
            _generatedTerrainObjects.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("TerrainGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_floorMaterials == null || _floorMaterials.Length == 0)
            {
                ChimeraLogger.LogWarning("No floor materials assigned, using default materials", this);
            }

            if (_minHillCount > _maxHillCount)
            {
                ChimeraLogger.LogWarning("Min hill count greater than max, adjusting", this);
                _minHillCount = Mathf.Min(_minHillCount, _maxHillCount);
            }
        }

        private void InitializeGenerators()
        {
            _heightmapGenerator.Initialize(_random);
            _featureGenerator.Initialize(_random, _groundMaterials);
        }

        #endregion

        #region Terrain Generation Interface

        /// <summary>
        /// Generate terrain based on facility scene type
        /// </summary>
        public IEnumerator GenerateTerrain(FacilitySceneType sceneType, Vector2Int facilitySize, TerrainGenerationSettings settings = null)
        {
            return GenerateTerrainInternal(sceneType, facilitySize, settings);
        }

        /// <summary>
        /// Generate terrain with random seed (overload for orchestrator)
        /// </summary>
        public IEnumerator GenerateTerrain(FacilitySceneType sceneType, Vector2Int facilitySize, int randomSeed)
        {
            // Update random seed if provided
            if (randomSeed > 0)
            {
                _random = new System.Random(randomSeed);
                InitializeGenerators();
            }
            return GenerateTerrainInternal(sceneType, facilitySize, null);
        }

        /// <summary>
        /// Internal terrain generation implementation
        /// </summary>
        private IEnumerator GenerateTerrainInternal(FacilitySceneType sceneType, Vector2Int facilitySize, TerrainGenerationSettings settings = null)
        {
            if (!_isInitialized || !_enableTerrainGeneration)
            {
                ChimeraLogger.LogWarning("TerrainGenerationService not initialized or disabled", this);
                yield break;
            }

            OnTerrainGenerationProgress?.Invoke("Starting terrain generation", 0f);

            switch (sceneType)
            {
                case FacilitySceneType.IndoorFacility:
                    yield return StartCoroutine(GenerateIndoorTerrain(facilitySize));
                    break;
                    
                case FacilitySceneType.OutdoorFarm:
                    yield return StartCoroutine(GenerateOutdoorTerrain(facilitySize));
                    break;
                    
                case FacilitySceneType.Greenhouse:
                    yield return StartCoroutine(GenerateGreenhouseTerrain(facilitySize));
                    break;
                    
                case FacilitySceneType.MixedFacility:
                    yield return StartCoroutine(GenerateMixedTerrain(facilitySize));
                    break;
                    
                case FacilitySceneType.UrbanRooftop:
                    yield return StartCoroutine(GenerateUrbanTerrain(facilitySize));
                    break;
            }

            OnTerrainGenerationComplete?.Invoke(new TerrainGenerationCompleteArgs
            {
                SceneType = sceneType,
                FacilitySize = facilitySize,
                GeneratedObjects = new List<GameObject>(_generatedTerrainObjects),
                Success = true
            });

            ChimeraLogger.Log($"Terrain generation completed for {sceneType}", this);
        }

        #endregion

        #region Indoor Terrain Generation

        /// <summary>
        /// Generate indoor facility terrain
        /// </summary>
        private IEnumerator GenerateIndoorTerrain(Vector2Int facilitySize)
        {
            OnTerrainGenerationProgress?.Invoke("Generating indoor floor", 0.2f);
            
            // Generate base floor
            GameObject floor = CreateFloor(facilitySize);
            _generatedTerrainObjects.Add(floor);
            
            OnTerrainGenerationProgress?.Invoke("Generating exterior walls", 0.6f);
            
            // Generate surrounding walls
            yield return StartCoroutine(GenerateExteriorWalls(facilitySize));
            
            OnTerrainGenerationProgress?.Invoke("Indoor terrain complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Create facility floor
        /// </summary>
        private GameObject CreateFloor(Vector2Int size)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Facility Floor";
            floor.transform.SetParent(_containerTransform);
            floor.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
            floor.transform.position = Vector3.zero;
            
            // Apply material
            if (_floorMaterials != null && _floorMaterials.Length > 0)
            {
                var renderer = floor.GetComponent<Renderer>();
                renderer.material = _floorMaterials[_random.Next(_floorMaterials.Length)];
            }
            
            return floor;
        }

        /// <summary>
        /// Generate exterior walls for indoor facilities
        /// </summary>
        private IEnumerator GenerateExteriorWalls(Vector2Int facilitySize)
        {
            // Generate perimeter walls
            var wallPositions = new Vector3[]
            {
                new Vector3(facilitySize.x / 2f, 2.5f, 0f), // Right wall
                new Vector3(-facilitySize.x / 2f, 2.5f, 0f), // Left wall
                new Vector3(0f, 2.5f, facilitySize.y / 2f), // Front wall
                new Vector3(0f, 2.5f, -facilitySize.y / 2f), // Back wall
            };
            
            var wallScales = new Vector3[]
            {
                new Vector3(1f, 5f, facilitySize.y),
                new Vector3(1f, 5f, facilitySize.y),
                new Vector3(facilitySize.x, 5f, 1f),
                new Vector3(facilitySize.x, 5f, 1f),
            };
            
            for (int i = 0; i < wallPositions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Exterior Wall {i + 1}";
                wall.transform.SetParent(_containerTransform);
                wall.transform.position = wallPositions[i];
                wall.transform.localScale = wallScales[i];
                
                // Apply material
                if (_wallMaterials != null && _wallMaterials.Length > 0)
                {
                    var renderer = wall.GetComponent<Renderer>();
                    renderer.material = _wallMaterials[_random.Next(_wallMaterials.Length)];
                }
                
                _generatedTerrainObjects.Add(wall);
                yield return new WaitForSeconds(0.01f);
            }
        }

        #endregion

        #region Outdoor Terrain Generation

        /// <summary>
        /// Generate outdoor terrain with natural features
        /// </summary>
        private IEnumerator GenerateOutdoorTerrain(Vector2Int facilitySize)
        {
            OnTerrainGenerationProgress?.Invoke("Generating outdoor terrain base", 0.1f);
            
            // Generate terrain with height variations
            var terrain = _heightmapGenerator.GenerateOutdoorTerrain(facilitySize, _groundMaterials);
            _generatedTerrainObjects.Add(terrain);
            
            OnTerrainGenerationProgress?.Invoke("Adding natural features", 0.5f);
            
            // Add natural features
            if (_enableNaturalFeatures)
            {
                yield return StartCoroutine(GenerateNaturalFeatures(facilitySize));
            }
            
            OnTerrainGenerationProgress?.Invoke("Outdoor terrain complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate natural features for outdoor terrain
        /// </summary>
        private IEnumerator GenerateNaturalFeatures(Vector2Int facilitySize)
        {
            // Generate hills
            yield return StartCoroutine(GenerateHills(facilitySize));
            
            // Generate paths
            if (_enablePathGeneration)
            {
                yield return StartCoroutine(GeneratePaths(facilitySize));
            }
            
            // Generate water features
            if (_enableWaterFeatures)
            {
                yield return StartCoroutine(GenerateWaterFeatures(facilitySize));
            }
        }

        /// <summary>
        /// Generate hills across the terrain
        /// </summary>
        private IEnumerator GenerateHills(Vector2Int facilitySize)
        {
            int hillCount = _random.Next(_minHillCount, _maxHillCount + 1);
            
            for (int i = 0; i < hillCount; i++)
            {
                Vector3 position = new Vector3(
                    _random.Next(-facilitySize.x / 2, facilitySize.x / 2),
                    0f,
                    _random.Next(-facilitySize.y / 2, facilitySize.y / 2)
                );
                
                var hill = _featureGenerator.GenerateHill(position, _random.Next(_minHillSize, _maxHillSize + 1));
                _generatedTerrainObjects.Add(hill);
                
                yield return new WaitForSeconds(0.05f);
            }
        }

        /// <summary>
        /// Generate path network
        /// </summary>
        private IEnumerator GeneratePaths(Vector2Int facilitySize)
        {
            var pathPoints = GeneratePathNetwork(facilitySize);
            
            foreach (var path in pathPoints)
            {
                var pathObject = _featureGenerator.GeneratePath(path, _pathWidth, _pathHeight, _pathMaterial);
                _generatedTerrainObjects.Add(pathObject);
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate path network layout
        /// </summary>
        private List<List<Vector3>> GeneratePathNetwork(Vector2Int facilitySize)
        {
            var pathNetwork = new List<List<Vector3>>();
            
            // Main central path
            var mainPath = new List<Vector3>
            {
                new Vector3(-facilitySize.x / 2f, _pathHeight, 0f),
                new Vector3(0f, _pathHeight, 0f),
                new Vector3(facilitySize.x / 2f, _pathHeight, 0f)
            };
            pathNetwork.Add(mainPath);
            
            // Cross path
            var crossPath = new List<Vector3>
            {
                new Vector3(0f, _pathHeight, -facilitySize.y / 2f),
                new Vector3(0f, _pathHeight, 0f),
                new Vector3(0f, _pathHeight, facilitySize.y / 2f)
            };
            pathNetwork.Add(crossPath);
            
            return pathNetwork;
        }

        /// <summary>
        /// Generate water features like ponds
        /// </summary>
        private IEnumerator GenerateWaterFeatures(Vector2Int facilitySize)
        {
            if (_random.NextDouble() < _waterFeatureChance)
            {
                Vector3 pondPosition = new Vector3(
                    _random.Next(-facilitySize.x / 3, facilitySize.x / 3),
                    -0.2f,
                    _random.Next(-facilitySize.y / 3, facilitySize.y / 3)
                );
                
                var pond = _featureGenerator.GeneratePond(pondPosition);
                _generatedTerrainObjects.Add(pond);
            }
            
            yield return null;
        }

        #endregion

        #region Greenhouse Terrain Generation

        /// <summary>
        /// Generate greenhouse terrain with growing beds
        /// </summary>
        private IEnumerator GenerateGreenhouseTerrain(Vector2Int facilitySize)
        {
            OnTerrainGenerationProgress?.Invoke("Generating growing beds", 0.3f);
            
            // Generate raised growing beds
            yield return StartCoroutine(GenerateGrowingBeds(facilitySize));
            
            OnTerrainGenerationProgress?.Invoke("Greenhouse terrain complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Generate raised growing beds for greenhouse
        /// </summary>
        private IEnumerator GenerateGrowingBeds(Vector2Int facilitySize)
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
                bed.transform.SetParent(_containerTransform);
                bed.transform.position = bedPosition;
                bed.transform.localScale = new Vector3(4f, 0.6f, 2f);
                
                // Apply soil material
                if (_groundMaterials != null && _groundMaterials.Length > 0)
                {
                    var renderer = bed.GetComponent<Renderer>();
                    renderer.material = _groundMaterials[0]; // Assume first is soil
                }
                
                _generatedTerrainObjects.Add(bed);
                yield return new WaitForSeconds(0.02f);
            }
        }

        #endregion

        #region Mixed and Urban Terrain

        /// <summary>
        /// Generate mixed indoor/outdoor terrain
        /// </summary>
        private IEnumerator GenerateMixedTerrain(Vector2Int facilitySize)
        {
            OnTerrainGenerationProgress?.Invoke("Generating mixed terrain - indoor", 0.3f);
            
            // Combine indoor and outdoor elements
            yield return StartCoroutine(GenerateIndoorTerrain(facilitySize));
            
            OnTerrainGenerationProgress?.Invoke("Generating mixed terrain - outdoor", 0.7f);
            
            yield return StartCoroutine(GenerateOutdoorArea(facilitySize));
            
            OnTerrainGenerationProgress?.Invoke("Mixed terrain complete", 1.0f);
        }

        /// <summary>
        /// Generate outdoor area for mixed facilities
        /// </summary>
        private IEnumerator GenerateOutdoorArea(Vector2Int facilitySize)
        {
            Vector3 outdoorOffset = new Vector3(facilitySize.x / 2f + 10f, 0f, 0f);
            yield return StartCoroutine(GenerateOutdoorPlots(outdoorOffset));
        }

        /// <summary>
        /// Generate outdoor cultivation plots
        /// </summary>
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
                plot.transform.SetParent(_containerTransform);
                plot.transform.position = plotPosition;
                plot.transform.localScale = new Vector3(6f, 0.4f, 4f);
                
                _generatedTerrainObjects.Add(plot);
                yield return new WaitForSeconds(0.02f);
            }
        }

        /// <summary>
        /// Generate urban rooftop terrain
        /// </summary>
        private IEnumerator GenerateUrbanTerrain(Vector2Int facilitySize)
        {
            OnTerrainGenerationProgress?.Invoke("Generating rooftop base", 0.5f);
            
            // Generate rooftop base
            GameObject rooftop = CreateRooftop(facilitySize);
            _generatedTerrainObjects.Add(rooftop);
            
            OnTerrainGenerationProgress?.Invoke("Urban terrain complete", 1.0f);
            
            yield return null;
        }

        /// <summary>
        /// Create rooftop base for urban facilities
        /// </summary>
        private GameObject CreateRooftop(Vector2Int size)
        {
            GameObject rooftop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rooftop.name = "Rooftop Base";
            rooftop.transform.SetParent(_containerTransform);
            rooftop.transform.localScale = new Vector3(size.x, 2f, size.y);
            rooftop.transform.position = new Vector3(0f, -1f, 0f);
            
            // Apply material
            if (_roofMaterials != null && _roofMaterials.Length > 0)
            {
                var renderer = rooftop.GetComponent<Renderer>();
                renderer.material = _roofMaterials[_random.Next(_roofMaterials.Length)];
            }
            
            return rooftop;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all generated terrain objects
        /// </summary>
        public void ClearGeneratedTerrain()
        {
            foreach (var obj in _generatedTerrainObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
        }

        /// <summary>
        /// Get terrain generation statistics
        /// </summary>
        public TerrainGenerationStats GetGenerationStats()
        {
            return new TerrainGenerationStats
            {
                TotalObjectsGenerated = _generatedTerrainObjects.Count,
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
    /// Simplified terrain heightmap generator
    /// </summary>
    public class TerrainHeightmapGenerator
    {
        private System.Random _random;

        public void Initialize(System.Random random)
        {
            _random = random;
        }

        public GameObject GenerateOutdoorTerrain(Vector2Int size, Material[] groundMaterials)
        {
            // Create terrain plane with height variation
            GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Outdoor Terrain";
            terrain.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
            
            // Apply ground material
            if (groundMaterials != null && groundMaterials.Length > 0)
            {
                var renderer = terrain.GetComponent<Renderer>();
                renderer.material = groundMaterials[_random.Next(groundMaterials.Length)];
            }
            
            return terrain;
        }
    }

    /// <summary>
    /// Natural feature generator for terrain elements
    /// </summary>
    public class NaturalFeatureGenerator
    {
        private System.Random _random;
        private Material[] _groundMaterials;

        public void Initialize(System.Random random, Material[] groundMaterials)
        {
            _random = random;
            _groundMaterials = groundMaterials;
        }

        public GameObject GenerateHill(Vector3 position, int size)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = $"Hill_{position.x:F0}_{position.z:F0}";
            hill.transform.position = position + Vector3.up * (size / 4f);
            hill.transform.localScale = Vector3.one * size;
            
            // Apply ground material
            if (_groundMaterials != null && _groundMaterials.Length > 0)
            {
                var renderer = hill.GetComponent<Renderer>();
                renderer.material = _groundMaterials[_random.Next(_groundMaterials.Length)];
            }
            
            return hill;
        }

        public GameObject GeneratePath(List<Vector3> pathPoints, float width, float height, Material pathMaterial)
        {
            GameObject pathContainer = new GameObject("Path");
            
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var start = pathPoints[i];
                var end = pathPoints[i + 1];
                var center = (start + end) / 2f;
                var distance = Vector3.Distance(start, end);
                
                GameObject pathSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pathSegment.name = $"Path Segment {i}";
                pathSegment.transform.SetParent(pathContainer.transform);
                pathSegment.transform.position = center;
                pathSegment.transform.LookAt(end);
                pathSegment.transform.localScale = new Vector3(width, height, distance);
                
                if (pathMaterial != null)
                {
                    var renderer = pathSegment.GetComponent<Renderer>();
                    renderer.material = pathMaterial;
                }
            }
            
            return pathContainer;
        }

        public GameObject GeneratePond(Vector3 position)
        {
            GameObject pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond.name = "Pond";
            pond.transform.position = position;
            pond.transform.localScale = new Vector3(6f, 0.2f, 6f);
            
            // Apply water-like material if available
            if (_groundMaterials != null && _groundMaterials.Length > 1)
            {
                var renderer = pond.GetComponent<Renderer>();
                renderer.material = _groundMaterials[1]; // Assume second material is water-like
            }
            
            return pond;
        }
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// Terrain generation settings
    /// </summary>
    [System.Serializable]
    public class TerrainGenerationSettings
    {
        public bool EnableNaturalFeatures = true;
        public bool EnablePathGeneration = true;
        public bool EnableWaterFeatures = true;
        public int HillDensity = 3;
        public float WaterFeatureChance = 0.3f;
    }

    /// <summary>
    /// Terrain generation completion arguments
    /// </summary>
    [System.Serializable]
    public class TerrainGenerationCompleteArgs
    {
        public FacilitySceneType SceneType;
        public Vector2Int FacilitySize;
        public List<GameObject> GeneratedObjects;
        public bool Success;
    }

    /// <summary>
    /// Terrain generation statistics
    /// </summary>
    [System.Serializable]
    public class TerrainGenerationStats
    {
        public int TotalObjectsGenerated;
        public bool ServiceInitialized;
        public DateTime LastGenerationTime;
    }

    #endregion
}