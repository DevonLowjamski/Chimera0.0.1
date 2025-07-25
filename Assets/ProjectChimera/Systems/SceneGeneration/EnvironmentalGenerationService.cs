using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Facilities;

namespace ProjectChimera.Systems.SceneGeneration
{
    /// <summary>
    /// Environmental Generation Service - Dedicated service for lighting, weather, and atmospheric systems
    /// Extracted from ProceduralSceneGenerator to provide focused environmental generation functionality
    /// Handles natural lighting, grow lights, dynamic lighting, weather effects, and particle systems
    /// Includes ambient lighting configuration, security lighting, and environmental optimization
    /// </summary>
    public class EnvironmentalGenerationService : MonoBehaviour, ISceneGenerationService
    {
        [Header("Environmental Generation Configuration")]
        [SerializeField] private bool _enableEnvironmentalGeneration = true;
        [SerializeField] private bool _enableLightingGeneration = true;
        [SerializeField] private bool _enableWeatherGeneration = true;
        [SerializeField] private bool _enableParticleGeneration = true;
        [SerializeField] private bool _enableOptimization = true;

        [Header("Lighting Settings")]
        [SerializeField] private bool _generateNaturalLighting = true;
        [SerializeField] private bool _generateGrowLights = true;
        [SerializeField] private bool _generateDynamicLighting = true;
        [SerializeField] private bool _generateGeneralLighting = true;
        [SerializeField] private int _lightsPerRoom = 4;
        [SerializeField] private float _ambientLightIntensity = 0.3f;
        [SerializeField] private float _lightGenerationDelay = 0.02f;

        [Header("Weather Settings")]
        [SerializeField] private bool _includeWeatherSystem = true;
        [SerializeField] private bool _generateAmbientParticles = true;
        [SerializeField] private bool _generateEnvironmentalParticles = true;

        [Header("Optimization Settings")]
        [SerializeField] private bool _optimizeLighting = true;
        [SerializeField] private bool _cullDistantLights = true;
        [SerializeField] private float _maxLightRange = 10f;
        [SerializeField] private bool _useSoftShadows = true;

        [Header("Environmental Containers")]
        [SerializeField] private Transform _environmentalContainer;
        [SerializeField] private Transform _lightingContainer;
        [SerializeField] private Transform _weatherContainer;
        [SerializeField] private Transform _particleContainer;

        // Environmental generation state
        private bool _isInitialized = false;
        private System.Random _random;
        private Transform _containerTransform;
        private Vector2Int _facilitySize;
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();
        private List<GameObject> _generatedEnvironmentalObjects = new List<GameObject>();
        private FacilitySceneType _sceneType;

        // Environmental tracking
        private Dictionary<LightType, List<GameObject>> _generatedLights = new Dictionary<LightType, List<GameObject>>();
        private List<GameObject> _weatherSystems = new List<GameObject>();
        private List<GameObject> _particleSystems = new List<GameObject>();
        private Dictionary<string, GameObject> _roomLights = new Dictionary<string, GameObject>();

        // Events for environmental generation progress
        public static event System.Action<EnvironmentalGenerationPhase> OnEnvironmentalPhaseStarted;
        public static event System.Action<EnvironmentalGenerationPhase> OnEnvironmentalPhaseCompleted;
        public static event System.Action<string, int> OnLightingSystemGenerated;
        public static event System.Action<EnvironmentalGenerationReport> OnEnvironmentalGenerationCompleted;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Environmental Generation Service";
        public IReadOnlyList<GameObject> GeneratedEnvironmentalObjects => _generatedEnvironmentalObjects;
        public IReadOnlyDictionary<LightType, List<GameObject>> GeneratedLights => _generatedLights;

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
            _generatedEnvironmentalObjects = new List<GameObject>();
            _generatedLights = new Dictionary<LightType, List<GameObject>>();
            _weatherSystems = new List<GameObject>();
            _particleSystems = new List<GameObject>();
            _roomLights = new Dictionary<string, GameObject>();

            // Initialize light type tracking
            foreach (LightType lightType in System.Enum.GetValues(typeof(LightType)))
            {
                _generatedLights[lightType] = new List<GameObject>();
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
                ChimeraLogger.LogWarning("EnvironmentalGenerationService already initialized", this);
                return;
            }

            try
            {
                _random = seed > 0 ? new System.Random(seed) : new System.Random();
                _containerTransform = container ?? transform;
                
                ValidateConfiguration();
                SetupContainers();
                
                _isInitialized = true;
                ChimeraLogger.Log("EnvironmentalGenerationService initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize EnvironmentalGenerationService: {ex.Message}", this);
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            ClearGeneratedEnvironmentalObjects();
            _generatedEnvironmentalObjects.Clear();
            _generatedLights.Clear();
            _weatherSystems.Clear();
            _particleSystems.Clear();
            _roomLights.Clear();
            
            _isInitialized = false;
            ChimeraLogger.Log("EnvironmentalGenerationService shutdown completed", this);
        }

        private void ValidateConfiguration()
        {
            if (_lightGenerationDelay < 0f)
            {
                ChimeraLogger.LogWarning("Invalid light generation delay, using default 0.02s", this);
                _lightGenerationDelay = 0.02f;
            }

            if (_ambientLightIntensity < 0f)
            {
                ChimeraLogger.LogWarning("Invalid ambient light intensity, using default 0.3", this);
                _ambientLightIntensity = 0.3f;
            }

            if (_lightsPerRoom < 1)
            {
                ChimeraLogger.LogWarning("Invalid lights per room count, using default 4", this);
                _lightsPerRoom = 4;
            }
        }

        private void SetupContainers()
        {
            if (_environmentalContainer == null)
            {
                var containerGO = new GameObject("Environmental Container");
                _environmentalContainer = containerGO.transform;
            }

            // Create specialized containers if not assigned
            if (_lightingContainer == null)
            {
                var lightingGO = new GameObject("Lighting Container");
                lightingGO.transform.SetParent(_environmentalContainer);
                _lightingContainer = lightingGO.transform;
            }

            if (_weatherContainer == null)
            {
                var weatherGO = new GameObject("Weather Container");
                weatherGO.transform.SetParent(_environmentalContainer);
                _weatherContainer = weatherGO.transform;
            }

            if (_particleContainer == null)
            {
                var particleGO = new GameObject("Particle Container");
                particleGO.transform.SetParent(_environmentalContainer);
                _particleContainer = particleGO.transform;
            }
        }

        #endregion

        #region Environmental Generation Interface

        /// <summary>
        /// Generate all environmental systems for the facility
        /// </summary>
        public IEnumerator GenerateEnvironmentalSystems(FacilitySceneType sceneType, Vector2Int facilitySize, List<RoomLayout> rooms, int randomSeed = 0)
        {
            if (!_isInitialized || !_enableEnvironmentalGeneration)
            {
                ChimeraLogger.LogWarning("EnvironmentalGenerationService not initialized or environmental generation disabled", this);
                yield break;
            }

            _sceneType = sceneType;
            _facilitySize = facilitySize;
            _generatedRooms = rooms ?? new List<RoomLayout>();
            _random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            ChimeraLogger.Log($"Starting environmental generation for {sceneType} facility", this);

            if (_enableLightingGeneration)
            {
                yield return StartCoroutine(GenerateLightingSystems());
            }

            if (_enableWeatherGeneration)
            {
                yield return StartCoroutine(GenerateWeatherSystems());
            }

            if (_enableParticleGeneration)
            {
                yield return StartCoroutine(GenerateParticleSystems());
            }

            if (_enableOptimization)
            {
                yield return StartCoroutine(OptimizeEnvironmentalSystems());
            }

            GenerateCompletionReport();
            ChimeraLogger.Log($"Environmental generation completed: {_generatedEnvironmentalObjects.Count} environmental objects generated", this);
        }

        /// <summary>
        /// Clear all generated environmental objects
        /// </summary>
        public void ClearGeneratedEnvironmentalObjects()
        {
            foreach (var envObject in _generatedEnvironmentalObjects)
            {
                if (envObject != null)
                {
                    DestroyImmediate(envObject);
                }
            }

            _generatedEnvironmentalObjects.Clear();
            _generatedLights.Clear();
            _weatherSystems.Clear();
            _particleSystems.Clear();
            _roomLights.Clear();
        }

        #endregion

        #region Lighting Systems Generation

        /// <summary>
        /// Generate comprehensive lighting systems
        /// </summary>
        private IEnumerator GenerateLightingSystems()
        {
            OnEnvironmentalPhaseStarted?.Invoke(EnvironmentalGenerationPhase.Lighting);
            ChimeraLogger.Log("Generating lighting systems", this);

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

            if (_generateGeneralLighting)
            {
                yield return StartCoroutine(GenerateGeneralLighting());
            }

            OnEnvironmentalPhaseCompleted?.Invoke(EnvironmentalGenerationPhase.Lighting);
        }

        /// <summary>
        /// Generate natural lighting (sun, ambient)
        /// </summary>
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
            sun.shadows = _useSoftShadows ? LightShadows.Soft : LightShadows.Hard;

            _generatedEnvironmentalObjects.Add(sunLight);
            _generatedLights[LightType.Directional].Add(sunLight);

            // Configure ambient lighting
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
            RenderSettings.ambientIntensity = _ambientLightIntensity;

            OnLightingSystemGenerated?.Invoke("Natural Lighting", 1);
            yield return null;
        }

        /// <summary>
        /// Generate grow lights for cultivation rooms
        /// </summary>
        private IEnumerator GenerateGrowLighting()
        {
            int totalGrowLights = 0;

            foreach (var room in _generatedRooms)
            {
                if (IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomGrowLights(room));
                    totalGrowLights += Mathf.Max(_lightsPerRoom, Mathf.RoundToInt(room.Area / 10f));
                }
            }

            OnLightingSystemGenerated?.Invoke("Grow Lighting", totalGrowLights);
        }

        /// <summary>
        /// Generate grow lights for specific room
        /// </summary>
        private IEnumerator GenerateRoomGrowLights(RoomLayout room)
        {
            int lightsInRoom = Mathf.Max(_lightsPerRoom, Mathf.RoundToInt(room.Area / 10f));

            for (int i = 0; i < lightsInRoom; i++)
            {
                Vector3 lightPosition = GenerateLightPosition(room, i, lightsInRoom);
                GameObject growLight = GenerateGrowLight(lightPosition, room);
                _generatedEnvironmentalObjects.Add(growLight);
                _generatedLights[LightType.Spot].Add(growLight);

                yield return new WaitForSeconds(_lightGenerationDelay);
            }
        }

        /// <summary>
        /// Generate optimal light position in room
        /// </summary>
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

        /// <summary>
        /// Generate individual grow light
        /// </summary>
        private GameObject GenerateGrowLight(Vector3 position, RoomLayout room)
        {
            GameObject lightGO = new GameObject($"{room.RoomName} Grow Light");
            lightGO.transform.SetParent(_lightingContainer);
            lightGO.transform.position = position;

            // Light component
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.9f, 0.8f); // Warm white for plant growth
            light.intensity = 2f;
            light.range = 8f;
            light.spotAngle = 60f;
            light.shadows = _useSoftShadows ? LightShadows.Soft : LightShadows.None;
            light.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Add lighting controller (placeholder)
            // var controller = lightGO.AddComponent<LightingController>();

            // Visual fixture
            GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fixture.name = "Light Fixture";
            fixture.transform.SetParent(lightGO.transform);
            fixture.transform.localPosition = Vector3.zero;
            fixture.transform.localScale = new Vector3(1f, 0.2f, 1f);

            // Remove collider from fixture
            Destroy(fixture.GetComponent<Collider>());

            return lightGO;
        }

        /// <summary>
        /// Generate dynamic lighting effects
        /// </summary>
        private IEnumerator GenerateDynamicLighting()
        {
            yield return StartCoroutine(GenerateAccentLighting());
            yield return StartCoroutine(GenerateSecurityLighting());
        }

        /// <summary>
        /// Generate accent lighting for atmosphere
        /// </summary>
        private IEnumerator GenerateAccentLighting()
        {
            var accentPositions = new Vector3[]
            {
                new Vector3(0f, 1.5f, -_facilitySize.y / 2f), // Entrance
                new Vector3(_facilitySize.x / 2f, 1.5f, 0f), // Side
                new Vector3(-_facilitySize.x / 2f, 1.5f, 0f), // Other side
            };

            foreach (var position in accentPositions)
            {
                GameObject accentLight = GenerateAccentLight(position);
                _generatedEnvironmentalObjects.Add(accentLight);
                _generatedLights[LightType.Point].Add(accentLight);
                yield return new WaitForSeconds(0.01f);
            }

            OnLightingSystemGenerated?.Invoke("Accent Lighting", accentPositions.Length);
        }

        /// <summary>
        /// Generate individual accent light
        /// </summary>
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

        /// <summary>
        /// Generate security lighting for perimeter
        /// </summary>
        private IEnumerator GenerateSecurityLighting()
        {
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
                _generatedEnvironmentalObjects.Add(securityLight);
                _generatedLights[LightType.Spot].Add(securityLight);
                yield return new WaitForSeconds(0.01f);
            }

            OnLightingSystemGenerated?.Invoke("Security Lighting", securityPositions.Length);
        }

        /// <summary>
        /// Generate individual security light
        /// </summary>
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

        /// <summary>
        /// Generate general facility lighting
        /// </summary>
        private IEnumerator GenerateGeneralLighting()
        {
            int generalLights = 0;

            foreach (var room in _generatedRooms)
            {
                if (!IsGrowRoom(room.RoomType))
                {
                    yield return StartCoroutine(GenerateRoomGeneralLighting(room));
                    generalLights++;
                }
            }

            OnLightingSystemGenerated?.Invoke("General Lighting", generalLights);
        }

        /// <summary>
        /// Generate general lighting for specific room
        /// </summary>
        private IEnumerator GenerateRoomGeneralLighting(RoomLayout room)
        {
            Vector3 lightPosition = room.Position + new Vector3(0f, 2.5f, 0f);

            GameObject roomLight = GenerateGeneralLight(lightPosition, room.RoomName);
            _generatedEnvironmentalObjects.Add(roomLight);
            _generatedLights[LightType.Point].Add(roomLight);
            _roomLights[room.RoomName] = roomLight;

            yield return null;
        }

        /// <summary>
        /// Generate individual general light
        /// </summary>
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

        #region Weather Systems Generation

        /// <summary>
        /// Generate weather systems for outdoor facilities
        /// </summary>
        private IEnumerator GenerateWeatherSystems()
        {
            if (!_includeWeatherSystem) yield break;
            if (_sceneType != FacilitySceneType.OutdoorFarm && _sceneType != FacilitySceneType.MixedFacility) yield break;

            OnEnvironmentalPhaseStarted?.Invoke(EnvironmentalGenerationPhase.Weather);
            ChimeraLogger.Log("Generating weather systems", this);

            yield return StartCoroutine(CreateWeatherSystem());

            OnEnvironmentalPhaseCompleted?.Invoke(EnvironmentalGenerationPhase.Weather);
        }

        /// <summary>
        /// Create main weather control system
        /// </summary>
        private IEnumerator CreateWeatherSystem()
        {
            GameObject weatherSystem = new GameObject("Weather System");
            weatherSystem.transform.SetParent(_weatherContainer);

            // Add weather controller component (placeholder)
            // var weatherController = weatherSystem.AddComponent<WeatherController>();

            _generatedEnvironmentalObjects.Add(weatherSystem);
            _weatherSystems.Add(weatherSystem);

            yield return null;
        }

        #endregion

        #region Particle Systems Generation

        /// <summary>
        /// Generate atmospheric particle effects
        /// </summary>
        private IEnumerator GenerateParticleSystems()
        {
            OnEnvironmentalPhaseStarted?.Invoke(EnvironmentalGenerationPhase.Particles);
            ChimeraLogger.Log("Generating particle systems", this);

            if (_generateAmbientParticles)
            {
                yield return StartCoroutine(GenerateAmbientParticles());
            }

            if (_generateEnvironmentalParticles)
            {
                yield return StartCoroutine(GenerateEnvironmentalParticles());
            }

            OnEnvironmentalPhaseCompleted?.Invoke(EnvironmentalGenerationPhase.Particles);
        }

        /// <summary>
        /// Generate ambient dust particle system
        /// </summary>
        private IEnumerator GenerateAmbientParticles()
        {
            GameObject dustSystem = new GameObject("Ambient Dust");
            dustSystem.transform.SetParent(_particleContainer);
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

            _generatedEnvironmentalObjects.Add(dustSystem);
            _particleSystems.Add(dustSystem);

            yield return null;
        }

        /// <summary>
        /// Generate environmental particle effects (pollen, etc.)
        /// </summary>
        private IEnumerator GenerateEnvironmentalParticles()
        {
            if (_sceneType == FacilitySceneType.OutdoorFarm || _sceneType == FacilitySceneType.MixedFacility)
            {
                // Generate pollen particles
                GameObject pollenSystem = new GameObject("Pollen Particles");
                pollenSystem.transform.SetParent(_particleContainer);
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

                _generatedEnvironmentalObjects.Add(pollenSystem);
                _particleSystems.Add(pollenSystem);
            }

            yield return null;
        }

        #endregion

        #region Environmental Optimization

        /// <summary>
        /// Optimize environmental systems for performance
        /// </summary>
        private IEnumerator OptimizeEnvironmentalSystems()
        {
            OnEnvironmentalPhaseStarted?.Invoke(EnvironmentalGenerationPhase.Optimization);
            ChimeraLogger.Log("Optimizing environmental systems", this);

            if (_optimizeLighting)
            {
                yield return StartCoroutine(OptimizeLighting());
            }

            OnEnvironmentalPhaseCompleted?.Invoke(EnvironmentalGenerationPhase.Optimization);
        }

        /// <summary>
        /// Optimize lighting for performance
        /// </summary>
        private IEnumerator OptimizeLighting()
        {
            var allLights = _generatedLights.Values.SelectMany(list => list)
                .Where(lightObj => lightObj != null)
                .Select(lightObj => lightObj.GetComponent<Light>())
                .Where(light => light != null)
                .ToArray();

            foreach (var light in allLights)
            {
                // Limit light range for performance
                if (_cullDistantLights && light.type == LightType.Point && light.range > _maxLightRange)
                {
                    light.range = _maxLightRange;
                }

                // Optimize shadow settings
                if (light.shadows == LightShadows.Hard && _useSoftShadows)
                {
                    light.shadows = LightShadows.Soft;
                }

                yield return new WaitForSeconds(0.001f);
            }

            ChimeraLogger.Log($"Optimized {allLights.Length} lights for performance", this);
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
        /// Generate completion report
        /// </summary>
        private void GenerateCompletionReport()
        {
            var report = new EnvironmentalGenerationReport
            {
                TotalEnvironmentalObjects = _generatedEnvironmentalObjects.Count,
                LightingSystemsCount = _generatedLights.Values.Sum(list => list.Count),
                WeatherSystemsCount = _weatherSystems.Count,
                ParticleSystemsCount = _particleSystems.Count,
                NaturalLightingEnabled = _generateNaturalLighting,
                GrowLightingEnabled = _generateGrowLights,
                WeatherSystemEnabled = _includeWeatherSystem,
                GenerationTime = Time.realtimeSinceStartup,
                FacilityType = _sceneType,
                FacilitySize = _facilitySize
            };

            OnEnvironmentalGenerationCompleted?.Invoke(report);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get environmental object count by type
        /// </summary>
        public int GetEnvironmentalObjectCount(EnvironmentalObjectType objectType)
        {
            return objectType switch
            {
                EnvironmentalObjectType.Lights => _generatedLights.Values.Sum(list => list.Count),
                EnvironmentalObjectType.WeatherSystems => _weatherSystems.Count,
                EnvironmentalObjectType.ParticleSystems => _particleSystems.Count,
                _ => 0
            };
        }

        /// <summary>
        /// Get lights by type
        /// </summary>
        public List<GameObject> GetLightsByType(LightType lightType)
        {
            return _generatedLights.ContainsKey(lightType) ? 
                new List<GameObject>(_generatedLights[lightType]) : 
                new List<GameObject>();
        }

        /// <summary>
        /// Update environmental configuration
        /// </summary>
        public void UpdateEnvironmentalConfiguration(EnvironmentalGenerationConfig config)
        {
            _enableLightingGeneration = config.EnableLighting;
            _enableWeatherGeneration = config.EnableWeather;
            _enableParticleGeneration = config.EnableParticles;
            _ambientLightIntensity = config.AmbientLightIntensity;
            _lightsPerRoom = config.LightsPerRoom;
            
            ChimeraLogger.Log("Environmental generation configuration updated", this);
        }

        /// <summary>
        /// Toggle specific lighting system
        /// </summary>
        public void SetLightingSystemActive(LightType lightType, bool active)
        {
            if (_generatedLights.ContainsKey(lightType))
            {
                foreach (var lightObj in _generatedLights[lightType])
                {
                    if (lightObj != null)
                    {
                        lightObj.SetActive(active);
                    }
                }
            }
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
    /// Environmental generation phases
    /// </summary>
    public enum EnvironmentalGenerationPhase
    {
        Lighting,
        Weather,
        Particles,
        Optimization
    }

    /// <summary>
    /// Environmental object types for categorization
    /// </summary>
    public enum EnvironmentalObjectType
    {
        Lights,
        WeatherSystems,
        ParticleSystems
    }

    /// <summary>
    /// Environmental generation configuration
    /// </summary>
    [System.Serializable]
    public class EnvironmentalGenerationConfig
    {
        public bool EnableLighting = true;
        public bool EnableWeather = true;
        public bool EnableParticles = true;
        public float AmbientLightIntensity = 0.3f;
        public int LightsPerRoom = 4;
        public bool OptimizeLighting = true;
        public float MaxLightRange = 10f;
    }

    /// <summary>
    /// Environmental generation completion report
    /// </summary>
    [System.Serializable]
    public class EnvironmentalGenerationReport
    {
        public int TotalEnvironmentalObjects;
        public int LightingSystemsCount;
        public int WeatherSystemsCount;
        public int ParticleSystemsCount;
        public bool NaturalLightingEnabled;
        public bool GrowLightingEnabled;
        public bool WeatherSystemEnabled;
        public float GenerationTime;
        public FacilitySceneType FacilityType;
        public Vector2Int FacilitySize;
    }

    #endregion
}