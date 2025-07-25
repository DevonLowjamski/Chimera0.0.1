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
    /// Procedural Scene Orchestrator - Central coordination service for all scene generation phases
    /// Replaces the monolithic ProceduralSceneGenerator by coordinating specialized services
    /// Manages multi-phase generation workflow with progress tracking and error handling
    /// Provides unified interface for facility scene generation across all facility types
    /// </summary>
    public class ProceduralSceneOrchestrator : MonoBehaviour
    {
        [Header("Generation Services Configuration")]
        [SerializeField] private bool _enableSceneGeneration = true;
        [SerializeField] private bool _enableTerrainGeneration = true;
        [SerializeField] private bool _enableBuildingGeneration = true;
        [SerializeField] private bool _enableVegetationGeneration = true;
        [SerializeField] private bool _enableEquipmentGeneration = true;
        [SerializeField] private bool _enableEnvironmentalGeneration = true;
        [SerializeField] private bool _enableDetailGeneration = true;

        [Header("Generation Timing Configuration")]
        [SerializeField] private float _phaseTransitionDelay = 0.1f;
        [SerializeField] private float _maxGenerationTime = 60f;
        [SerializeField] private bool _generateInBackground = true;
        [SerializeField] private bool _showProgressUpdates = true;

        [Header("Service References")]
        [SerializeField] private TerrainGenerationService _terrainService;
        [SerializeField] private BuildingGenerationService _buildingService;
        [SerializeField] private VegetationGenerationService _vegetationService;
        [SerializeField] private EquipmentGenerationService _equipmentService;
        [SerializeField] private EnvironmentalGenerationService _environmentalService;
        [SerializeField] private DetailGenerationService _detailService;

        [Header("Generation Containers")]
        [SerializeField] private Transform _sceneContainer;
        [SerializeField] private Transform _generatedObjectsContainer;

        // Orchestration state
        private bool _isInitialized = false;
        private bool _isGenerating = false;
        private Vector2Int _facilitySize;
        private FacilitySceneType _sceneType;
        private int _randomSeed;
        private float _generationStartTime;

        // Service coordination
        private List<ISceneGenerationService> _generationServices = new List<ISceneGenerationService>();
        private Dictionary<Type, bool> _serviceStatus = new Dictionary<Type, bool>();
        private GenerationPhaseProgress _currentProgress = new GenerationPhaseProgress();
        private List<RoomLayout> _generatedRooms = new List<RoomLayout>();

        // Events for generation coordination and progress
        public static event System.Action<SceneGenerationPhase> OnGenerationPhaseStarted;
        public static event System.Action<SceneGenerationPhase> OnGenerationPhaseCompleted;
        public static event System.Action<GenerationProgressUpdate> OnGenerationProgressUpdate;
        public static event System.Action<SceneGenerationReport> OnSceneGenerationCompleted;
        public static event System.Action<string> OnGenerationError;

        #region Service Implementation

        public bool IsInitialized => _isInitialized;
        public bool IsGenerating => _isGenerating;
        public string ServiceName => "Procedural Scene Orchestrator";
        public GenerationPhaseProgress CurrentProgress => _currentProgress;
        public IReadOnlyList<RoomLayout> GeneratedRooms => _generatedRooms;

        public void Initialize()
        {
            InitializeService();
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
            _generationServices = new List<ISceneGenerationService>();
            _serviceStatus = new Dictionary<Type, bool>();
            _currentProgress = new GenerationPhaseProgress();
            _generatedRooms = new List<RoomLayout>();
        }

        public void InitializeService()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("ProceduralSceneOrchestrator already initialized", this);
                return;
            }

            try
            {
                ValidateConfiguration();
                SetupContainers();
                RegisterGenerationServices();
                InitializeServices();
                
                _isInitialized = true;
                ChimeraLogger.Log("ProceduralSceneOrchestrator initialized successfully", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Failed to initialize ProceduralSceneOrchestrator: {ex.Message}", this);
                OnGenerationError?.Invoke($"Orchestrator initialization failed: {ex.Message}");
                throw;
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                if (_isGenerating)
                {
                    StopAllCoroutines();
                    _isGenerating = false;
                }

                ShutdownServices();
                ClearAllGeneratedContent();
                
                _generationServices.Clear();
                _serviceStatus.Clear();
                _generatedRooms.Clear();
                
                _isInitialized = false;
                ChimeraLogger.Log("ProceduralSceneOrchestrator shutdown completed", this);
            }
            catch (System.Exception ex)
            {
                ChimeraLogger.LogError($"Error during ProceduralSceneOrchestrator shutdown: {ex.Message}", this);
            }
        }

        private void ValidateConfiguration()
        {
            if (_phaseTransitionDelay < 0f)
            {
                ChimeraLogger.LogWarning("Invalid phase transition delay, using default 0.1s", this);
                _phaseTransitionDelay = 0.1f;
            }

            if (_maxGenerationTime <= 0f)
            {
                ChimeraLogger.LogWarning("Invalid max generation time, using default 60s", this);
                _maxGenerationTime = 60f;
            }
        }

        private void SetupContainers()
        {
            if (_sceneContainer == null)
            {
                var containerGO = new GameObject("Scene Container");
                _sceneContainer = containerGO.transform;
            }

            if (_generatedObjectsContainer == null)
            {
                var objectsGO = new GameObject("Generated Objects Container");
                objectsGO.transform.SetParent(_sceneContainer);
                _generatedObjectsContainer = objectsGO.transform;
            }
        }

        private void RegisterGenerationServices()
        {
            _generationServices.Clear();
            _serviceStatus.Clear();

            // Auto-discover services if not manually assigned
            if (_terrainService == null) _terrainService = FindObjectOfType<TerrainGenerationService>();
            if (_buildingService == null) _buildingService = FindObjectOfType<BuildingGenerationService>();
            if (_vegetationService == null) _vegetationService = FindObjectOfType<VegetationGenerationService>();
            if (_equipmentService == null) _equipmentService = FindObjectOfType<EquipmentGenerationService>();
            if (_environmentalService == null) _environmentalService = FindObjectOfType<EnvironmentalGenerationService>();
            if (_detailService == null) _detailService = FindObjectOfType<DetailGenerationService>();

            // Register available services
            RegisterService(_terrainService, _enableTerrainGeneration);
            RegisterService(_buildingService, _enableBuildingGeneration);
            RegisterService(_vegetationService, _enableVegetationGeneration);
            RegisterService(_equipmentService, _enableEquipmentGeneration);
            RegisterService(_environmentalService, _enableEnvironmentalGeneration);
            RegisterService(_detailService, _enableDetailGeneration);
        }

        private void RegisterService<T>(T service, bool enabled) where T : MonoBehaviour, ISceneGenerationService
        {
            if (service != null && enabled)
            {
                _generationServices.Add(service);
                _serviceStatus[typeof(T)] = true;
                ChimeraLogger.Log($"Registered generation service: {service.ServiceName}", this);
            }
            else
            {
                _serviceStatus[typeof(T)] = false;
                if (service == null && enabled)
                {
                    ChimeraLogger.LogWarning($"Service {typeof(T).Name} is enabled but not found", this);
                }
            }
        }

        private void InitializeServices()
        {
            foreach (var service in _generationServices)
            {
                try
                {
                    if (!service.IsInitialized)
                    {
                        service.Initialize();
                    }
                }
                catch (System.Exception ex)
                {
                    ChimeraLogger.LogError($"Failed to initialize service {service.ServiceName}: {ex.Message}", this);
                    OnGenerationError?.Invoke($"Service initialization failed: {service.ServiceName}");
                }
            }
        }

        private void ShutdownServices()
        {
            foreach (var service in _generationServices)
            {
                try
                {
                    if (service != null && service.IsInitialized)
                    {
                        service.Shutdown();
                    }
                }
                catch (System.Exception ex)
                {
                    ChimeraLogger.LogError($"Error shutting down service {service?.ServiceName}: {ex.Message}", this);
                }
            }
        }

        #endregion

        #region Scene Generation Interface

        /// <summary>
        /// Generate complete facility scene with all phases
        /// </summary>
        public IEnumerator GenerateCompleteScene(FacilitySceneType sceneType, Vector2Int facilitySize, int randomSeed = 0)
        {
            if (!_isInitialized || !_enableSceneGeneration)
            {
                ChimeraLogger.LogWarning("ProceduralSceneOrchestrator not initialized or scene generation disabled", this);
                OnGenerationError?.Invoke("Scene generation not available");
                yield break;
            }

            if (_isGenerating)
            {
                ChimeraLogger.LogWarning("Scene generation already in progress", this);
                yield break;
            }

            _isGenerating = true;
            _sceneType = sceneType;
            _facilitySize = facilitySize;
            _randomSeed = randomSeed != 0 ? randomSeed : UnityEngine.Random.Range(1, 100000);
            _generationStartTime = Time.realtimeSinceStartup;

            ChimeraLogger.Log($"Starting complete scene generation for {sceneType} facility (Size: {facilitySize}, Seed: {_randomSeed})", this);

            // Clear any existing content
            ClearAllGeneratedContent();
            ResetProgress();

            // Execute generation phases in sequence
            yield return StartCoroutine(ExecuteGenerationPhasesWithErrorHandling());

            // Generate completion report
            GenerateCompletionReport();

            ChimeraLogger.Log($"Complete scene generation finished in {Time.realtimeSinceStartup - _generationStartTime:F2}s", this);
            _isGenerating = false;
        }

        /// <summary>
        /// Generate specific phase only
        /// </summary>
        public IEnumerator GenerateSpecificPhase(SceneGenerationPhase phase, FacilitySceneType sceneType, Vector2Int facilitySize, int randomSeed = 0)
        {
            if (!_isInitialized || !_enableSceneGeneration)
            {
                ChimeraLogger.LogWarning("ProceduralSceneOrchestrator not initialized", this);
                yield break;
            }

            _sceneType = sceneType;
            _facilitySize = facilitySize;
            _randomSeed = randomSeed != 0 ? randomSeed : UnityEngine.Random.Range(1, 100000);

            yield return StartCoroutine(ExecuteSinglePhase(phase));
        }

        /// <summary>
        /// Clear all generated scene content
        /// </summary>
        public void ClearAllGeneratedContent()
        {
            // Clear content from all services
            _terrainService?.ClearGeneratedTerrain();
            _buildingService?.ClearGeneratedBuildings();
            _vegetationService?.ClearGeneratedVegetation();
            _equipmentService?.ClearGeneratedEquipment();
            _environmentalService?.ClearGeneratedEnvironmentalObjects();
            _detailService?.ClearGeneratedDetails();

            // Clear containers
            if (_generatedObjectsContainer != null)
            {
                for (int i = _generatedObjectsContainer.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(_generatedObjectsContainer.GetChild(i).gameObject);
                }
            }

            _generatedRooms.Clear();
        }

        #endregion

        #region Generation Phase Execution

        /// <summary>
        /// Execute generation phases with error handling wrapper
        /// </summary>
        private IEnumerator ExecuteGenerationPhasesWithErrorHandling()
        {
            // Cannot use try-catch with yield return, so we'll handle errors in individual phases
            yield return StartCoroutine(ExecuteGenerationPhases());
        }

        /// <summary>
        /// Execute all generation phases in sequence
        /// </summary>
        private IEnumerator ExecuteGenerationPhases()
        {
            var phases = new SceneGenerationPhase[]
            {
                SceneGenerationPhase.Terrain,
                SceneGenerationPhase.Buildings,
                SceneGenerationPhase.Vegetation,
                SceneGenerationPhase.Equipment,
                SceneGenerationPhase.Environmental,
                SceneGenerationPhase.Details
            };

            for (int i = 0; i < phases.Length; i++)
            {
                var phase = phases[i];
                
                // Check timeout
                if (Time.realtimeSinceStartup - _generationStartTime > _maxGenerationTime)
                {
                    ChimeraLogger.LogWarning($"Scene generation timeout after {_maxGenerationTime}s", this);
                    OnGenerationError?.Invoke("Generation timeout");
                    yield break;
                }

                yield return StartCoroutine(ExecuteSinglePhase(phase));

                // Phase transition delay
                if (i < phases.Length - 1)
                {
                    yield return new WaitForSeconds(_phaseTransitionDelay);
                }

                // Update overall progress
                UpdateOverallProgress(i + 1, phases.Length);
            }
        }

        /// <summary>
        /// Execute single generation phase
        /// </summary>
        private IEnumerator ExecuteSinglePhase(SceneGenerationPhase phase)
        {
            OnGenerationPhaseStarted?.Invoke(phase);
            
            if (_showProgressUpdates)
            {
                ChimeraLogger.Log($"Starting generation phase: {phase}", this);
            }

            float phaseStartTime = Time.realtimeSinceStartup;

            // Execute the phase without try-catch since yields are not allowed
            switch (phase)
            {
                case SceneGenerationPhase.Terrain:
                    if (_enableTerrainGeneration && _terrainService != null)
                    {
                        // Initialize service with seed before generation
                        _terrainService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_terrainService.GenerateTerrain(_sceneType, _facilitySize));
                    }
                    break;

                case SceneGenerationPhase.Buildings:
                    if (_enableBuildingGeneration && _buildingService != null)
                    {
                        // Initialize service with seed before generation
                        _buildingService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_buildingService.GenerateBuildings(_sceneType, _facilitySize));
                        // Capture generated rooms for later phases
                        if (_buildingService.GeneratedRooms != null)
                        {
                            _generatedRooms = _buildingService.GeneratedRooms.ToList();
                        }
                    }
                    break;

                case SceneGenerationPhase.Vegetation:
                    if (_enableVegetationGeneration && _vegetationService != null)
                    {
                        // Initialize service with seed before generation  
                        _vegetationService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_vegetationService.GenerateVegetation(_sceneType, _facilitySize, _generatedRooms));
                    }
                    break;

                case SceneGenerationPhase.Equipment:
                    if (_enableEquipmentGeneration && _equipmentService != null)
                    {
                        // Initialize service before generation
                        _equipmentService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_equipmentService.GenerateEquipment(_sceneType, _facilitySize, _generatedRooms, _randomSeed));
                    }
                    break;

                case SceneGenerationPhase.Environmental:
                    if (_enableEnvironmentalGeneration && _environmentalService != null)
                    {
                        // Initialize service before generation
                        _environmentalService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_environmentalService.GenerateEnvironmentalSystems(_sceneType, _facilitySize, _generatedRooms, _randomSeed));
                    }
                    break;

                case SceneGenerationPhase.Details:
                    if (_enableDetailGeneration && _detailService != null)
                    {
                        // Initialize service before generation
                        _detailService.Initialize(_randomSeed, _generatedObjectsContainer);
                        yield return StartCoroutine(_detailService.GenerateDetails(_sceneType, _facilitySize, _generatedRooms, _randomSeed));
                    }
                    break;
            }

            float phaseTime = Time.realtimeSinceStartup - phaseStartTime;
            if (_showProgressUpdates)
            {
                ChimeraLogger.Log($"Completed generation phase: {phase} in {phaseTime:F2}s", this);
            }

            OnGenerationPhaseCompleted?.Invoke(phase);
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Reset generation progress
        /// </summary>
        private void ResetProgress()
        {
            _currentProgress = new GenerationPhaseProgress
            {
                CurrentPhase = SceneGenerationPhase.Terrain,
                PhaseProgress = 0f,
                OverallProgress = 0f,
                StartTime = Time.realtimeSinceStartup,
                EstimatedTimeRemaining = 0f
            };
        }

        /// <summary>
        /// Update overall generation progress
        /// </summary>
        private void UpdateOverallProgress(int completedPhases, int totalPhases)
        {
            _currentProgress.OverallProgress = (float)completedPhases / totalPhases;
            _currentProgress.ElapsedTime = Time.realtimeSinceStartup - _currentProgress.StartTime;
            
            if (completedPhases > 0)
            {
                float avgTimePerPhase = _currentProgress.ElapsedTime / completedPhases;
                _currentProgress.EstimatedTimeRemaining = avgTimePerPhase * (totalPhases - completedPhases);
            }

            var progressUpdate = new GenerationProgressUpdate
            {
                Phase = _currentProgress.CurrentPhase,
                PhaseProgress = _currentProgress.PhaseProgress,
                OverallProgress = _currentProgress.OverallProgress,
                ElapsedTime = _currentProgress.ElapsedTime,
                EstimatedTimeRemaining = _currentProgress.EstimatedTimeRemaining
            };

            OnGenerationProgressUpdate?.Invoke(progressUpdate);
        }

        #endregion

        #region Completion Reporting

        /// <summary>
        /// Generate comprehensive completion report
        /// </summary>
        private void GenerateCompletionReport()
        {
            var report = new SceneGenerationReport
            {
                FacilityType = _sceneType,
                FacilitySize = _facilitySize,
                RandomSeed = _randomSeed,
                GenerationTime = Time.realtimeSinceStartup - _generationStartTime,
                
                // Phase execution status
                TerrainGenerated = _enableTerrainGeneration && _terrainService != null,
                BuildingsGenerated = _enableBuildingGeneration && _buildingService != null,
                VegetationGenerated = _enableVegetationGeneration && _vegetationService != null,
                EquipmentGenerated = _enableEquipmentGeneration && _equipmentService != null,
                EnvironmentalGenerated = _enableEnvironmentalGeneration && _environmentalService != null,
                DetailsGenerated = _enableDetailGeneration && _detailService != null,

                // Detailed counts from services
                TerrainObjectCount = _terrainService?.GeneratedTerrainObjects?.Count ?? 0,
                BuildingCount = _buildingService?.GeneratedBuildingObjects?.Count ?? 0,
                RoomCount = _generatedRooms.Count,
                VegetationObjectCount = _vegetationService?.GeneratedVegetationObjects?.Count ?? 0,
                EquipmentCount = _equipmentService?.GeneratedEquipment?.Count ?? 0,
                EnvironmentalObjectCount = _environmentalService?.GeneratedEnvironmentalObjects?.Count ?? 0,
                DetailObjectCount = _detailService?.GeneratedDetails?.Count ?? 0,

                // Total object counts
                TotalObjectsGenerated = CalculateTotalObjectsGenerated(),
                
                // Performance metrics
                MemoryUsed = GC.GetTotalMemory(false),
                GenerationSuccess = true
            };

            OnSceneGenerationCompleted?.Invoke(report);
            ChimeraLogger.Log($"Scene generation report: {report.TotalObjectsGenerated} total objects generated", this);
        }

        /// <summary>
        /// Calculate total generated objects across all services
        /// </summary>
        private int CalculateTotalObjectsGenerated()
        {
            int total = 0;
            total += _terrainService?.GeneratedTerrainObjects?.Count ?? 0;
            total += _buildingService?.GeneratedBuildingObjects?.Count ?? 0;
            total += _vegetationService?.GeneratedVegetationObjects?.Count ?? 0;
            total += _equipmentService?.GeneratedEquipment?.Count ?? 0;
            total += _environmentalService?.GeneratedEnvironmentalObjects?.Count ?? 0;
            total += _detailService?.GeneratedDetails?.Count ?? 0;
            return total;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get generation status for specific phase
        /// </summary>
        public bool IsPhaseEnabled(SceneGenerationPhase phase)
        {
            return phase switch
            {
                SceneGenerationPhase.Terrain => _enableTerrainGeneration,
                SceneGenerationPhase.Buildings => _enableBuildingGeneration,
                SceneGenerationPhase.Vegetation => _enableVegetationGeneration,
                SceneGenerationPhase.Equipment => _enableEquipmentGeneration,
                SceneGenerationPhase.Environmental => _enableEnvironmentalGeneration,
                SceneGenerationPhase.Details => _enableDetailGeneration,
                _ => false
            };
        }

        /// <summary>
        /// Enable/disable specific generation phase
        /// </summary>
        public void SetPhaseEnabled(SceneGenerationPhase phase, bool enabled)
        {
            switch (phase)
            {
                case SceneGenerationPhase.Terrain:
                    _enableTerrainGeneration = enabled;
                    break;
                case SceneGenerationPhase.Buildings:
                    _enableBuildingGeneration = enabled;
                    break;
                case SceneGenerationPhase.Vegetation:
                    _enableVegetationGeneration = enabled;
                    break;
                case SceneGenerationPhase.Equipment:
                    _enableEquipmentGeneration = enabled;
                    break;
                case SceneGenerationPhase.Environmental:
                    _enableEnvironmentalGeneration = enabled;
                    break;
                case SceneGenerationPhase.Details:
                    _enableDetailGeneration = enabled;
                    break;
            }

            ChimeraLogger.Log($"Phase {phase} {(enabled ? "enabled" : "disabled")}", this);
        }

        /// <summary>
        /// Get service status for specific service type
        /// </summary>
        public bool IsServiceAvailable<T>() where T : MonoBehaviour, ISceneGenerationService
        {
            return _serviceStatus.ContainsKey(typeof(T)) && _serviceStatus[typeof(T)];
        }

        /// <summary>
        /// Get service instance by type
        /// </summary>
        public T GetService<T>() where T : MonoBehaviour, ISceneGenerationService
        {
            return _generationServices.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Update generation configuration
        /// </summary>
        public void UpdateGenerationConfiguration(SceneGenerationConfig config)
        {
            _enableTerrainGeneration = config.EnableTerrain;
            _enableBuildingGeneration = config.EnableBuildings;
            _enableVegetationGeneration = config.EnableVegetation;
            _enableEquipmentGeneration = config.EnableEquipment;
            _enableEnvironmentalGeneration = config.EnableEnvironmental;
            _enableDetailGeneration = config.EnableDetails;
            _phaseTransitionDelay = config.PhaseTransitionDelay;
            _maxGenerationTime = config.MaxGenerationTime;
            _showProgressUpdates = config.ShowProgressUpdates;

            ChimeraLogger.Log("Scene generation configuration updated", this);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    #region Data Structures and Interfaces

    /// <summary>
    /// Interface for scene generation services
    /// </summary>
    public interface ISceneGenerationService
    {
        bool IsInitialized { get; }
        string ServiceName { get; }
        void Initialize();
        void Shutdown();
    }

    /// <summary>
    /// Scene generation phases
    /// </summary>
    public enum SceneGenerationPhase
    {
        Terrain,
        Buildings,
        Vegetation,
        Equipment,
        Environmental,
        Details
    }

    /// <summary>
    /// Generation phase progress tracking
    /// </summary>
    [System.Serializable]
    public class GenerationPhaseProgress
    {
        public SceneGenerationPhase CurrentPhase;
        public float PhaseProgress;
        public float OverallProgress;
        public float StartTime;
        public float ElapsedTime;
        public float EstimatedTimeRemaining;
    }

    /// <summary>
    /// Progress update event data
    /// </summary>
    [System.Serializable]
    public class GenerationProgressUpdate
    {
        public SceneGenerationPhase Phase;
        public float PhaseProgress;
        public float OverallProgress;
        public float ElapsedTime;
        public float EstimatedTimeRemaining;
    }

    /// <summary>
    /// Scene generation configuration
    /// </summary>
    [System.Serializable]
    public class SceneGenerationConfig
    {
        public bool EnableTerrain = true;
        public bool EnableBuildings = true;
        public bool EnableVegetation = true;
        public bool EnableEquipment = true;
        public bool EnableEnvironmental = true;
        public bool EnableDetails = true;
        public float PhaseTransitionDelay = 0.1f;
        public float MaxGenerationTime = 60f;
        public bool ShowProgressUpdates = true;
    }

    /// <summary>
    /// Complete scene generation report
    /// </summary>
    [System.Serializable]
    public class SceneGenerationReport
    {
        public FacilitySceneType FacilityType;
        public Vector2Int FacilitySize;
        public int RandomSeed;
        public float GenerationTime;
        
        // Phase execution status
        public bool TerrainGenerated;
        public bool BuildingsGenerated;
        public bool VegetationGenerated;
        public bool EquipmentGenerated;
        public bool EnvironmentalGenerated;
        public bool DetailsGenerated;
        
        // Object counts
        public int TerrainObjectCount;
        public int BuildingCount;
        public int RoomCount;
        public int VegetationObjectCount;
        public int EquipmentCount;
        public int EnvironmentalObjectCount;
        public int DetailObjectCount;
        public int TotalObjectsGenerated;
        
        // Performance metrics
        public long MemoryUsed;
        public bool GenerationSuccess;
    }

    #endregion
}