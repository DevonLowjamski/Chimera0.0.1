using UnityEngine;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using CultivationSystems = ProjectChimera.Systems.Cultivation;
using ProjectChimera.Scripts.Cultivation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Core;
using System.Collections.Generic;
using System.Linq;
// Explicit alias to resolve EnvironmentalConditions ambiguity
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
// Explicit alias to resolve LightSpectrum ambiguity - use enum from Cultivation namespace  
using LightSpectrum = ProjectChimera.Data.Cultivation.LightSpectrum;
// Explicit aliases to resolve controller ambiguity - use MonoBehaviour classes from Scripts namespace
using HVACController = ProjectChimera.Scripts.Environment.HVACController;
using LightingController = ProjectChimera.Scripts.Environment.LightingController;
using VentilationController = ProjectChimera.Scripts.Environment.VentilationController;
using IrrigationController = ProjectChimera.Scripts.Environment.IrrigationController;

namespace ProjectChimera.Scripts.Facilities
{
    /// <summary>
    /// Controls a grow room facility with environmental systems and plant management.
    /// Integrates with EnvironmentalManager and PlantManager for automated facility operations.
    /// </summary>
    public class GrowRoomController : MonoBehaviour
    {
        [Header("Room Configuration")]
        [SerializeField] private string _roomId;
        [SerializeField] private string _roomName = "Grow Room";
        [SerializeField] private Vector3 _roomDimensions = new Vector3(12f, 3f, 8f);
        [SerializeField] private int _maxPlants = 16;
        [SerializeField] private float _plantSpacing = 1.5f;
        
        [Header("Environmental Controls")]
        [SerializeField] private Transform _hvacSystem;
        [SerializeField] private Transform _lightingSystem;
        [SerializeField] private Transform _ventilationSystem;
        [SerializeField] private Transform _irrigationSystem;
        
        [Header("Plant Management")]
        [SerializeField] private Transform _plantContainer;
        [SerializeField] private GameObject _plantPrefab;
        [SerializeField] private Transform[] _plantPositions;
        
        [Header("Monitoring")]
        [SerializeField] private bool _enableAutomation = true;
        [SerializeField] private bool _showDebugInfo = true;
        [SerializeField] private float _updateInterval = 5f;
        
        // Environmental Data
        private EnvironmentalConditions _currentConditions;
        private EnvironmentSystems.EnvironmentalManager _environmentalManager;
        private CultivationSystems.PlantManager _plantManager;
        
        // Plant Management
        private List<PlantInstanceComponent> _plantsInRoom = new List<PlantInstanceComponent>();
        private Dictionary<Vector3, PlantInstanceComponent> _plantGrid = new Dictionary<Vector3, PlantInstanceComponent>();
        
        // Environmental Controls
        private HVACController _hvacController;
        private LightingController _lightingController;
        private VentilationController _ventilationController;
        private IrrigationController _irrigationController;
        
        // Room State
        private bool _isOperational = true;
        private float _lastUpdateTime;
        private RoomStatus _currentStatus = RoomStatus.Idle;
        
        // Events
        public System.Action<GrowRoomController> OnRoomStatusChanged;
        public System.Action<GrowRoomController, EnvironmentalConditions> OnEnvironmentUpdated;
        public System.Action<GrowRoomController, PlantInstanceComponent> OnPlantAdded;
        public System.Action<GrowRoomController, PlantInstanceComponent> OnPlantRemoved;
        
        // Properties
        public string RoomId => string.IsNullOrEmpty(_roomId) ? name : _roomId;
        public string RoomName => _roomName;
        public Vector3 RoomDimensions => _roomDimensions;
        public int CurrentPlantCount => _plantsInRoom.Count;
        public int MaxPlants => _maxPlants;
        public float OccupancyRate => (float)CurrentPlantCount / _maxPlants;
        public EnvironmentalConditions CurrentConditions => _currentConditions;
        public RoomStatus Status => _currentStatus;
        public bool IsOperational => _isOperational;
        public bool HasAvailableSpace => CurrentPlantCount < _maxPlants;
        
        private void Awake()
        {
            if (string.IsNullOrEmpty(_roomId))
                _roomId = System.Guid.NewGuid().ToString();
                
            InitializeComponents();
        }
        
        private void Start()
        {
            InitializeManagers();
            InitializePlantPositions();
            InitializeEnvironmentalSystems();
            
            if (_enableAutomation)
            {
                StartAutomation();
            }
        }
        
        private void Update()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateRoomSystems();
                UpdateEnvironmentalConditions();
                MonitorPlantHealth();
                _lastUpdateTime = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Find or create plant container
            if (_plantContainer == null)
            {
                var container = transform.Find("Plants");
                if (container == null)
                {
                    var plantContainer = new GameObject("Plants");
                    plantContainer.transform.SetParent(transform);
                    _plantContainer = plantContainer.transform;
                }
                else
                {
                    _plantContainer = container;
                }
            }
            
            // Initialize environmental controls
            _hvacController = _hvacSystem?.GetComponent<HVACController>();
            _lightingController = _lightingSystem?.GetComponent<LightingController>();
            _ventilationController = _ventilationSystem?.GetComponent<VentilationController>();
            _irrigationController = _irrigationSystem?.GetComponent<IrrigationController>();
        }
        
        private void InitializeManagers()
        {
            _environmentalManager = GameManager.Instance?.GetManager<EnvironmentSystems.EnvironmentalManager>();
            _plantManager = GameManager.Instance?.GetManager<CultivationSystems.PlantManager>();
            
            if (_environmentalManager == null)
                Debug.LogError("EnvironmentalManager not found");
            if (_plantManager == null)
                Debug.LogError("PlantManager not found");
        }
        
        private void InitializePlantPositions()
        {
            if (_plantPositions == null || _plantPositions.Length == 0)
            {
                GenerateGridPositions();
            }
        }
        
        private void GenerateGridPositions()
        {
            var positions = new List<Transform>();
            
            int plantsPerRow = Mathf.FloorToInt(_roomDimensions.x / _plantSpacing);
            int rows = Mathf.FloorToInt(_roomDimensions.z / _plantSpacing);
            int totalPositions = Mathf.Min(plantsPerRow * rows, _maxPlants);
            
            float startX = -(_roomDimensions.x * 0.5f) + (_plantSpacing * 0.5f);
            float startZ = -(_roomDimensions.z * 0.5f) + (_plantSpacing * 0.5f);
            
            for (int i = 0; i < totalPositions; i++)
            {
                int row = i / plantsPerRow;
                int col = i % plantsPerRow;
                
                Vector3 position = new Vector3(
                    startX + col * _plantSpacing,
                    0f,
                    startZ + row * _plantSpacing
                );
                
                GameObject positionMarker = new GameObject($"PlantPosition_{i}");
                positionMarker.transform.SetParent(_plantContainer);
                positionMarker.transform.localPosition = position;
                
                positions.Add(positionMarker.transform);
            }
            
            _plantPositions = positions.ToArray();
        }
        
        private void InitializeEnvironmentalSystems()
        {
            // Set up environmental monitoring
            _currentConditions = new EnvironmentalConditions
            {
                Temperature = 22f, // 22°C default
                Humidity = 50f,    // 50% default
                CO2Level = 400f,   // 400 ppm default
                LightIntensity = 0f,
                AirCirculation = 1f
            };
        }
        
        private void StartAutomation()
        {
            _currentStatus = RoomStatus.Automated;
            OnRoomStatusChanged?.Invoke(this);
            
            if (_lightingController != null)
                _lightingController.SetAutomationMode(true);
            if (_hvacController != null)
                _hvacController.SetAutomationMode(true);
            if (_ventilationController != null)
                _ventilationController.SetAutomationMode(true);
        }
        
        #endregion
        
        #region Plant Management
        
        /// <summary>
        /// Add a plant to the grow room
        /// </summary>
        public bool AddPlant(GameObject plantPrefab, Vector3? preferredPosition = null)
        {
            if (!HasAvailableSpace)
            {
                Debug.LogWarning($"Grow room {_roomName} is at capacity");
                return false;
            }
            
            // Find available position
            Transform plantPosition = FindAvailablePosition(preferredPosition);
            if (plantPosition == null)
            {
                Debug.LogWarning($"No available positions in grow room {_roomName}");
                return false;
            }
            
            // Instantiate plant
            GameObject plantGO = Instantiate(plantPrefab, plantPosition);
            plantGO.transform.localPosition = Vector3.zero;
            
            var plantComponent = plantGO.GetComponent<PlantInstanceComponent>();
            if (plantComponent == null)
            {
                Debug.LogError("Plant prefab missing PlantInstanceComponent");
                Destroy(plantGO);
                return false;
            }
            
            // Add to room tracking
            _plantsInRoom.Add(plantComponent);
            _plantGrid[plantPosition.localPosition] = plantComponent;
            
            // Set up plant events
            plantComponent.OnPlantGrowthStageChanged += OnPlantStageChanged;
            plantComponent.OnPlantHealthChanged += OnPlantHealthChanged;
            
            OnPlantAdded?.Invoke(this, plantComponent);
            UpdateRoomStatus();
            
            Debug.Log($"Added plant to {_roomName} at position {plantPosition.localPosition}");
            return true;
        }
        
        /// <summary>
        /// Remove a plant from the grow room
        /// </summary>
        public bool RemovePlant(PlantInstanceComponent plant)
        {
            if (!_plantsInRoom.Contains(plant))
                return false;
            
            _plantsInRoom.Remove(plant);
            
            // Find and clear grid position
            var gridEntry = _plantGrid.FirstOrDefault(kvp => kvp.Value == plant);
            if (!gridEntry.Equals(default(KeyValuePair<Vector3, PlantInstanceComponent>)))
            {
                _plantGrid.Remove(gridEntry.Key);
            }
            
            // Clean up events
            plant.OnPlantGrowthStageChanged -= OnPlantStageChanged;
            plant.OnPlantHealthChanged -= OnPlantHealthChanged;
            
            OnPlantRemoved?.Invoke(this, plant);
            UpdateRoomStatus();
            
            return true;
        }
        
        /// <summary>
        /// Water all plants in the room
        /// </summary>
        public void WaterAllPlants(float amount = 25f)
        {
            foreach (var plant in _plantsInRoom)
            {
                plant.WaterPlant(amount);
            }
            
            Debug.Log($"Watered all plants in {_roomName}");
        }
        
        /// <summary>
        /// Feed all plants in the room
        /// </summary>
        public void FeedAllPlants(float amount = 25f, string nutrientType = "General")
        {
            foreach (var plant in _plantsInRoom)
            {
                plant.AddNutrients(amount, nutrientType);
            }
            
            Debug.Log($"Fed all plants in {_roomName} with {nutrientType}");
        }
        
        private Transform FindAvailablePosition(Vector3? preferredPosition = null)
        {
            // Try preferred position first
            if (preferredPosition.HasValue)
            {
                var closest = _plantPositions
                    .Where(p => !_plantGrid.ContainsKey(p.localPosition))
                    .OrderBy(p => Vector3.Distance(p.localPosition, preferredPosition.Value))
                    .FirstOrDefault();
                
                if (closest != null)
                    return closest;
            }
            
            // Find any available position
            return _plantPositions.FirstOrDefault(p => !_plantGrid.ContainsKey(p.localPosition));
        }
        
        #endregion
        
        #region Environmental Control
        
        /// <summary>
        /// Set target environmental conditions
        /// </summary>
        public void SetTargetConditions(float temperature, float humidity, float co2Level = -1)
        {
            if (_hvacController != null)
            {
                _hvacController.SetTargetTemperature(temperature);
                _hvacController.SetTargetHumidity(humidity);
            }
            
            if (co2Level > 0 && _ventilationController != null)
            {
                _ventilationController.SetTargetCO2(co2Level);
            }
            
            Debug.Log($"Set target conditions for {_roomName}: {temperature}°C, {humidity}% RH");
        }
        
        /// <summary>
        /// Set lighting parameters for the room
        /// </summary>
        public void SetLighting(bool enabled, float intensity = 1f, LightSpectrum? spectrum = null)
        {
            if (_lightingController != null)
            {
                _lightingController.SetLightingEnabled(enabled);
                _lightingController.SetIntensity(intensity);
                
                if (spectrum.HasValue)
                {
                    // Convert Cultivation.LightSpectrum enum to Environment.LightSpectrum class
                    var environmentSpectrum = ConvertToEnvironmentSpectrum(spectrum.Value);
                    _lightingController.SetSpectrum(environmentSpectrum);
                }
            }
        }
        
        /// <summary>
        /// Convert Cultivation LightSpectrum enum to Environment LightSpectrum class
        /// </summary>
        private ProjectChimera.Data.Environment.LightSpectrum ConvertToEnvironmentSpectrum(ProjectChimera.Data.Cultivation.LightSpectrum cultivationSpectrum)
        {
            var environmentSpectrum = new ProjectChimera.Data.Environment.LightSpectrum();
            
            // Set spectrum values based on enum type
            switch (cultivationSpectrum)
            {
                case ProjectChimera.Data.Cultivation.LightSpectrum.Red:
                    environmentSpectrum.Red = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.Blue:
                    environmentSpectrum.Blue = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.Green:
                    environmentSpectrum.Green = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.FarRed:
                    environmentSpectrum.FarRed = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.UVA:
                    environmentSpectrum.UVA = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.UVB:
                    environmentSpectrum.UVB = 1f;
                    break;
                case ProjectChimera.Data.Cultivation.LightSpectrum.WhiteBalance:
                    environmentSpectrum.WhiteBalance = 1f;
                    break;
                default:
                    // Full spectrum as default
                    environmentSpectrum.Red = 0.3f;
                    environmentSpectrum.Blue = 0.3f;
                    environmentSpectrum.Green = 0.2f;
                    environmentSpectrum.FarRed = 0.1f;
                    environmentSpectrum.UVA = 0.05f;
                    environmentSpectrum.WhiteBalance = 0.05f;
                    break;
            }
            
            return environmentSpectrum;
        }
        
        /// <summary>
        /// Start automated environmental control
        /// </summary>
        public void StartEnvironmentalAutomation()
        {
            if (!_enableAutomation) return;
            
            // Analyze plant needs and adjust environment accordingly
            OptimizeEnvironmentForPlants();
            
            _currentStatus = RoomStatus.Automated;
            OnRoomStatusChanged?.Invoke(this);
        }
        
        private void OptimizeEnvironmentForPlants()
        {
            if (_plantsInRoom.Count == 0) return;
            
            // Get plant growth stages
            var stages = _plantsInRoom.Select(p => p.CurrentStage).ToList();
            var dominantStage = stages.GroupBy(s => s).OrderByDescending(g => g.Count()).First().Key;
            
            // Set optimal conditions based on dominant growth stage
            switch (dominantStage)
            {
                case PlantGrowthStage.Germination:
                case PlantGrowthStage.Seedling:
                    SetTargetConditions(24f, 70f, 800f); // Warm, humid for seedlings
                    SetLighting(true, 0.6f);
                    break;
                    
                case PlantGrowthStage.Vegetative:
                    SetTargetConditions(26f, 60f, 1000f); // Optimal for vegetative growth
                    SetLighting(true, 1f);
                    break;
                    
                case PlantGrowthStage.Flowering:
                    SetTargetConditions(22f, 50f, 1200f); // Cooler, drier for flowering
                    SetLighting(true, 0.8f);
                    break;
                    
                default:
                    SetTargetConditions(23f, 55f, 900f); // General optimal conditions
                    SetLighting(true, 0.8f);
                    break;
            }
        }
        
        #endregion
        
        #region System Updates
        
        private void UpdateRoomSystems()
        {
            if (!_isOperational) return;
            
            // Update environmental controls
            _hvacController?.UpdateSystem();
            _lightingController?.UpdateSystem();
            _ventilationController?.UpdateSystem();
            _irrigationController?.UpdateSystem();
            
            // Update automation if enabled
            if (_enableAutomation && _currentStatus == RoomStatus.Automated)
            {
                OptimizeEnvironmentForPlants();
            }
        }
        
        private void UpdateEnvironmentalConditions()
        {
            // Simulate environmental readings (in real implementation, these would come from sensors)
            if (_environmentalManager != null)
            {
                _currentConditions = _environmentalManager.GetRoomConditions(RoomId);
            }
            else
            {
                // Fallback simulation
                _currentConditions.Temperature += Random.Range(-0.5f, 0.5f);
                _currentConditions.Humidity += Random.Range(-2f, 2f);
                _currentConditions.CO2Level += Random.Range(-10f, 10f);
                _currentConditions.LightIntensity = _lightingController?.CurrentIntensity ?? 0f;
            }
            
            OnEnvironmentUpdated?.Invoke(this, _currentConditions);
        }
        
        private void MonitorPlantHealth()
        {
            bool hasUnhealthyPlants = _plantsInRoom.Any(p => p.PlantData?.CurrentHealth < 50f);
            
            if (hasUnhealthyPlants && _currentStatus != RoomStatus.Alert)
            {
                _currentStatus = RoomStatus.Alert;
                OnRoomStatusChanged?.Invoke(this);
                Debug.LogWarning($"Unhealthy plants detected in {_roomName}");
            }
            else if (!hasUnhealthyPlants && _currentStatus == RoomStatus.Alert)
            {
                _currentStatus = _enableAutomation ? RoomStatus.Automated : RoomStatus.Manual;
                OnRoomStatusChanged?.Invoke(this);
            }
        }
        
        private void UpdateRoomStatus()
        {
            var previousStatus = _currentStatus;
            
            if (!_isOperational)
            {
                _currentStatus = RoomStatus.Offline;
            }
            else if (CurrentPlantCount == 0)
            {
                _currentStatus = RoomStatus.Idle;
            }
            else if (_enableAutomation)
            {
                _currentStatus = RoomStatus.Automated;
            }
            else
            {
                _currentStatus = RoomStatus.Manual;
            }
            
            if (previousStatus != _currentStatus)
            {
                OnRoomStatusChanged?.Invoke(this);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlantStageChanged(PlantInstanceComponent plant)
        {
            Debug.Log($"Plant in {_roomName} advanced to {plant.CurrentStage}");
            
            // Trigger environmental optimization when plants change stages
            if (_enableAutomation)
            {
                OptimizeEnvironmentForPlants();
            }
        }
        
        private void OnPlantHealthChanged(PlantInstanceComponent plant)
        {
            Debug.Log($"Plant health changed in {_roomName}: {plant.PlantData?.CurrentHealth:F1}%");
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Get room status summary
        /// </summary>
        public GrowRoomStatus GetRoomStatus()
        {
            return new GrowRoomStatus
            {
                RoomId = RoomId,
                RoomName = _roomName,
                Status = _currentStatus,
                PlantCount = CurrentPlantCount,
                MaxPlants = _maxPlants,
                OccupancyRate = OccupancyRate,
                AverageHealth = _plantsInRoom.Count > 0 ? 
                    _plantsInRoom.Average(p => p.PlantData?.CurrentHealth ?? 0f) : 0f,
                Conditions = _currentConditions,
                IsOperational = _isOperational
            };
        }
        
        /// <summary>
        /// Emergency shutdown of all systems
        /// </summary>
        public void EmergencyShutdown()
        {
            _isOperational = false;
            _currentStatus = RoomStatus.Offline;
            
            _hvacController?.Shutdown();
            _lightingController?.Shutdown();
            _ventilationController?.Shutdown();
            _irrigationController?.Shutdown();
            
            OnRoomStatusChanged?.Invoke(this);
            Debug.LogWarning($"Emergency shutdown activated for {_roomName}");
        }
        
        /// <summary>
        /// Restart all systems
        /// </summary>
        public void RestartSystems()
        {
            _isOperational = true;
            
            _hvacController?.Startup();
            _lightingController?.Startup();
            _ventilationController?.Startup();
            _irrigationController?.Startup();
            
            UpdateRoomStatus();
            Debug.Log($"Systems restarted for {_roomName}");
        }
        
        #endregion
        
        private void OnDrawGizmosSelected()
        {
            if (_showDebugInfo)
            {
                // Draw room bounds
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, _roomDimensions);
                
                // Draw plant positions
                if (_plantPositions != null)
                {
                    Gizmos.color = Color.green;
                    foreach (var position in _plantPositions)
                    {
                        if (position != null)
                        {
                            Vector3 worldPos = transform.TransformPoint(position.localPosition);
                            bool occupied = _plantGrid.ContainsKey(position.localPosition);
                            Gizmos.color = occupied ? Color.red : Color.green;
                            Gizmos.DrawWireSphere(worldPos, 0.2f);
                        }
                    }
                }
            }
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public enum RoomStatus
    {
        Idle,
        Manual,
        Automated,
        Alert,
        Offline
    }
    
    [System.Serializable]
    public class GrowRoomStatus
    {
        public string RoomId;
        public string RoomName;
        public RoomStatus Status;
        public int PlantCount;
        public int MaxPlants;
        public float OccupancyRate;
        public float AverageHealth;
        public EnvironmentalConditions Conditions;
        public bool IsOperational;
    }
}