using UnityEngine;
using UnityEngine.UI;
// using TMPro;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Automation;
using ProjectChimera.SceneGeneration;
using ProjectChimera.Scripts.Environment;
using ProjectChimera.Scripts.Facilities;
using CultivationSystems = ProjectChimera.Systems.Cultivation;
// Explicit aliases to resolve controller ambiguity
using HVACController = ProjectChimera.Scripts.Environment.HVACController;
using LightingController = ProjectChimera.Scripts.Environment.LightingController;
using VentilationController = ProjectChimera.Scripts.Environment.VentilationController;
using IrrigationController = ProjectChimera.Scripts.Environment.IrrigationController;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using InteractivePlantComponent = ProjectChimera.Systems.Cultivation.InteractivePlantComponent;
using EnvironmentalSensor = ProjectChimera.Environment.EnvironmentalSensor;
using SensorType = ProjectChimera.Environment.SensorType;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace ProjectChimera.Cultivation
{
    /// <summary>
    /// Advanced grow room controller with comprehensive environmental management,
    /// automation systems, and intelligent plant monitoring.
    /// </summary>
    public class AdvancedGrowRoomController : MonoBehaviour
    {
        [Header("Room Configuration")]
        [SerializeField] private string _roomName = "Grow Room";
        [SerializeField] private FacilityRoomType _roomType = FacilityRoomType.Vegetative;
        [SerializeField] private Vector3 _roomDimensions = new Vector3(6f, 3f, 4f);
        [SerializeField] private int _maxPlantCapacity = 12;
        [SerializeField] private float _plantSpacing = 1f;
        
        [Header("Environmental Systems")]
        [SerializeField] private HVACController _hvacController;
        [SerializeField] private LightingController _lightingController;
        [SerializeField] private VentilationController _ventilationController;
        [SerializeField] private IrrigationController _irrigationController;
        [SerializeField] private CO2Controller _co2Controller;
        
        [Header("Monitoring Systems")]
        [SerializeField] private Transform _sensorContainer;
        [SerializeField] private GameObject _temperatureSensorPrefab;
        [SerializeField] private GameObject _humiditySensorPrefab;
        [SerializeField] private GameObject _lightSensorPrefab;
        [SerializeField] private GameObject _co2SensorPrefab;
        
        [Header("Plant Management")]
        [SerializeField] private Transform _plantContainer;
        [SerializeField] private Transform[] _plantPositions;
        [SerializeField] private LayerMask _plantLayerMask = 1 << 8;
        
        [Header("Automation")]
        [SerializeField] private bool _enableAutomation = true;
        [SerializeField] private bool _enableIntelligentControl = true;
        [SerializeField] private bool _enablePredictiveManagement = true;
        [SerializeField] private float _automationUpdateInterval = 5f;
        
        [Header("Environmental Targets")]
        [SerializeField] private float _targetTemperature = 24f;
        [SerializeField] private float _targetHumidity = 60f;
        [SerializeField] private float _targetCO2Level = 1200f;
        [SerializeField] private float _targetLightIntensity = 600f;
        [SerializeField] private float _targetAirFlow = 0.8f;
        
        [Header("Visual Indicators")]
        [SerializeField] private Renderer _roomStatusIndicator;
        [SerializeField] private Light _roomStatusLight;
        [SerializeField] private ParticleSystem _airflowParticles;
        [SerializeField] private Canvas _roomInfoCanvas;
        // [SerializeField] private TMPro.TextMeshProUGUI _roomInfoText;
        [SerializeField] private UnityEngine.UI.Text _roomInfoText; // Fallback to standard Text component
        
        // Room State
        private List<InteractivePlantComponent> _activePlants = new List<InteractivePlantComponent>();
        private List<EnvironmentalSensor> _sensors = new List<EnvironmentalSensor>();
        private RoomStatus _currentStatus = RoomStatus.Empty;
        private EnvironmentalConditions _currentConditions;
        private EnvironmentalConditions _targetConditions;
        
        // Automation System
        private float _lastAutomationUpdate = 0f;
        private AutomationProfile _automationProfile;
        private bool _automationActive = false;
        
        // Monitoring and Analytics
        private Queue<EnvironmentalReading> _environmentalHistory = new Queue<EnvironmentalReading>();
        private const int MAX_HISTORY_SIZE = 1440; // 24 hours at 1-minute intervals
        private float _lastDataCollection = 0f;
        private float _dataCollectionInterval = 60f; // 1 minute
        
        // Plant Analytics
        private PlantHealthAnalytics _plantAnalytics;
        private float _lastPlantAnalysis = 0f;
        private float _plantAnalysisInterval = 300f; // 5 minutes
        
        // Resource Consumption
        private ResourceConsumption _resourceConsumption;
        private float _lastResourceUpdate = 0f;
        private float _resourceUpdateInterval = 60f;
        
        // Events
        public System.Action<AdvancedGrowRoomController> OnRoomStatusChanged;
        public System.Action<AdvancedGrowRoomController, InteractivePlantComponent> OnPlantAdded;
        public System.Action<AdvancedGrowRoomController, InteractivePlantComponent> OnPlantRemoved;
        public System.Action<AdvancedGrowRoomController, EnvironmentalAlert> OnEnvironmentalAlert;
        public System.Action<AdvancedGrowRoomController, AutomationEvent> OnAutomationEvent;
        
        // Properties
        public string RoomName => _roomName;
        public FacilityRoomType RoomType => _roomType;
        public RoomStatus CurrentStatus => _currentStatus;
        public EnvironmentalConditions CurrentConditions => _currentConditions;
        public EnvironmentalConditions TargetConditions => _targetConditions;
        public List<InteractivePlantComponent> ActivePlants => _activePlants;
        public int PlantCount => _activePlants.Count;
        public int MaxCapacity => _maxPlantCapacity;
        public float OccupancyRate => (float)_activePlants.Count / _maxPlantCapacity;
        public bool HasAvailableSpace => _activePlants.Count < _maxPlantCapacity;
        public bool IsAutomationEnabled => _enableAutomation && _automationActive;
        public PlantHealthAnalytics PlantAnalytics => _plantAnalytics;
        public ResourceConsumption ResourceConsumption => _resourceConsumption;
        
        private void Awake()
        {
            InitializeRoom();
        }
        
        private void Start()
        {
            SetupEnvironmentalSystems();
            SetupSensors();
            SetupPlantPositions();
            InitializeAutomation();
            StartMonitoring();
        }
        
        private void Update()
        {
            float currentTime = Time.time;
            
            // Update environmental conditions
            UpdateEnvironmentalConditions();
            
            // Process automation
            if (_enableAutomation && currentTime - _lastAutomationUpdate >= _automationUpdateInterval)
            {
                ProcessAutomation();
                _lastAutomationUpdate = currentTime;
            }
            
            // Collect environmental data
            if (currentTime - _lastDataCollection >= _dataCollectionInterval)
            {
                CollectEnvironmentalData();
                _lastDataCollection = currentTime;
            }
            
            // Analyze plant health
            if (currentTime - _lastPlantAnalysis >= _plantAnalysisInterval)
            {
                AnalyzePlantHealth();
                _lastPlantAnalysis = currentTime;
            }
            
            // Update resource consumption
            if (currentTime - _lastResourceUpdate >= _resourceUpdateInterval)
            {
                UpdateResourceConsumption();
                _lastResourceUpdate = currentTime;
            }
            
            // Update visual indicators
            UpdateVisualIndicators();
        }
        
        #region Initialization
        
        private void InitializeRoom()
        {
            // Initialize environmental conditions
            _currentConditions = new EnvironmentalConditions
            {
                Temperature = 22f,
                Humidity = 55f,
                CO2Level = 400f,
                LightIntensity = 0f,
                AirFlow = 0.3f
            };
            
            _targetConditions = new EnvironmentalConditions
            {
                Temperature = _targetTemperature,
                Humidity = _targetHumidity,
                CO2Level = _targetCO2Level,
                LightIntensity = _targetLightIntensity,
                AirFlow = _targetAirFlow
            };
            
            // Initialize analytics
            _plantAnalytics = new PlantHealthAnalytics();
            _resourceConsumption = new ResourceConsumption();
            
            // Set initial status
            SetRoomStatus(RoomStatus.Initializing);
        }
        
        private void SetupEnvironmentalSystems()
        {
            // Auto-find environmental systems if not assigned
            if (_hvacController == null)
                _hvacController = GetComponentInChildren<HVACController>();
            
            if (_lightingController == null)
                _lightingController = GetComponentInChildren<LightingController>();
            
            if (_ventilationController == null)
                _ventilationController = GetComponentInChildren<VentilationController>();
            
            if (_irrigationController == null)
                _irrigationController = GetComponentInChildren<IrrigationController>();
            
            if (_co2Controller == null)
                _co2Controller = GetComponentInChildren<CO2Controller>();
            
            // Configure systems with target parameters
            ConfigureEnvironmentalSystems();
        }
        
        private void ConfigureEnvironmentalSystems()
        {
            if (_hvacController != null)
            {
                _hvacController.SetTargetTemperature(_targetTemperature);
                _hvacController.SetTargetHumidity(_targetHumidity);
            }
            
            if (_lightingController != null)
            {
                _lightingController.SetTargetIntensity(_targetLightIntensity);
                _lightingController.SetLightSchedule(CreateDefaultLightSchedule());
            }
            
            if (_ventilationController != null)
            {
                _ventilationController.SetTargetAirFlow(_targetAirFlow);
            }
            
            if (_co2Controller != null)
            {
                _co2Controller.SetTargetCO2Level(_targetCO2Level);
            }
        }
        
        private LightSchedule CreateDefaultLightSchedule()
        {
            return new LightSchedule
            {
                ScheduleName = "Default 18/6",
                LightPeriodHours = 18,
                DarkPeriodHours = 6,
                LightStartTime = new TimeSpan(6, 0, 0),
                IntensityRampTime = 30, // 30 minutes
                IsActive = true
            };
        }
        
        private void SetupSensors()
        {
            if (_sensorContainer == null)
            {
                _sensorContainer = new GameObject("Sensors").transform;
                _sensorContainer.SetParent(transform);
            }
            
            // Create environmental sensors at strategic positions
            CreateSensor(SensorType.Temperature, new Vector3(-1f, 1.5f, -1f));
            CreateSensor(SensorType.Humidity, new Vector3(1f, 1.5f, -1f));
            CreateSensor(SensorType.CO2Level, new Vector3(0f, 1.5f, 1f));
            CreateSensor(SensorType.LightLevel, new Vector3(0f, 2f, 0f));
            CreateSensor(SensorType.AirFlow, new Vector3(2f, 2f, 0f));
        }
        
        private void CreateSensor(SensorType sensorType, Vector3 localPosition)
        {
            GameObject sensorGO = new GameObject($"{sensorType} Sensor");
            sensorGO.transform.SetParent(_sensorContainer);
            sensorGO.transform.localPosition = localPosition;
            
            var sensor = sensorGO.AddComponent<EnvironmentalSensor>();
            sensor.Initialize(sensorType, this);
            
            _sensors.Add(sensor);
            
            // Add visual representation
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Sensor Visual";
            visual.transform.SetParent(sensorGO.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 0.1f;
            
            // Remove collider
            Destroy(visual.GetComponent<Collider>());
            
            // Set sensor color
            var renderer = visual.GetComponent<Renderer>();
            renderer.material.color = GetSensorColor(sensorType);
        }
        
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
        
        private void SetupPlantPositions()
        {
            if (_plantContainer == null)
            {
                _plantContainer = new GameObject("Plants").transform;
                _plantContainer.SetParent(transform);
            }
            
            if (_plantPositions == null || _plantPositions.Length == 0)
            {
                CreatePlantPositions();
            }
        }
        
        private void CreatePlantPositions()
        {
            _plantPositions = new Transform[_maxPlantCapacity];
            
            int rows = Mathf.CeilToInt(Mathf.Sqrt(_maxPlantCapacity));
            int cols = Mathf.CeilToInt((float)_maxPlantCapacity / rows);
            
            for (int i = 0; i < _maxPlantCapacity; i++)
            {
                int row = i / cols;
                int col = i % cols;
                
                float x = (col - cols * 0.5f + 0.5f) * _plantSpacing;
                float z = (row - rows * 0.5f + 0.5f) * _plantSpacing;
                
                GameObject positionGO = new GameObject($"Plant Position {i + 1}");
                positionGO.transform.SetParent(_plantContainer);
                positionGO.transform.localPosition = new Vector3(x, 0f, z);
                
                _plantPositions[i] = positionGO.transform;
                
                // Add visual marker
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Position Marker";
                marker.transform.SetParent(positionGO.transform);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);
                
                var markerRenderer = marker.GetComponent<Renderer>();
                markerRenderer.material.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                
                // Remove collider
                Destroy(marker.GetComponent<Collider>());
            }
        }
        
        #endregion
        
        #region Environmental Management
        
        private void UpdateEnvironmentalConditions()
        {
            // Collect readings from all sensors
            UpdateSensorReadings();
            
            // Calculate average conditions
            CalculateAverageConditions();
            
            // Check for environmental alerts
            CheckEnvironmentalAlerts();
        }
        
        private void UpdateSensorReadings()
        {
            foreach (var sensor in _sensors)
            {
                sensor.UpdateReading(_currentConditions);
            }
        }
        
        private void CalculateAverageConditions()
        {
            if (_sensors.Count == 0) return;
            
            var tempSensors = _sensors.Where(s => s.SensorType == SensorType.Temperature).ToList();
            var humiditySensors = _sensors.Where(s => s.SensorType == SensorType.Humidity).ToList();
            var lightSensors = _sensors.Where(s => s.SensorType == SensorType.LightLevel).ToList();
            var co2Sensors = _sensors.Where(s => s.SensorType == SensorType.CO2Level).ToList();
            var airflowSensors = _sensors.Where(s => s.SensorType == SensorType.AirFlow).ToList();
            
            if (tempSensors.Count > 0)
                _currentConditions.Temperature = tempSensors.Average(s => s.CurrentReading);
            
            if (humiditySensors.Count > 0)
                _currentConditions.Humidity = humiditySensors.Average(s => s.CurrentReading);
            
            if (lightSensors.Count > 0)
                _currentConditions.LightIntensity = lightSensors.Average(s => s.CurrentReading);
            
            if (co2Sensors.Count > 0)
                _currentConditions.CO2Level = co2Sensors.Average(s => s.CurrentReading);
            
            if (airflowSensors.Count > 0)
                _currentConditions.AirFlow = airflowSensors.Average(s => s.CurrentReading);
            
            // Update conditions from environmental systems
            UpdateConditionsFromSystems();
        }
        
        private void UpdateConditionsFromSystems()
        {
            if (_hvacController != null)
            {
                _currentConditions.Temperature = _hvacController.CurrentTemperature;
                _currentConditions.Humidity = _hvacController.CurrentHumidity;
            }
            
            if (_lightingController != null)
            {
                _currentConditions.LightIntensity = _lightingController.CurrentIntensity;
            }
            
            if (_ventilationController != null)
            {
                _currentConditions.AirFlow = _ventilationController.AirFlowRate;
            }
            
            if (_co2Controller != null)
            {
                _currentConditions.CO2Level = _co2Controller.CurrentCO2Level;
            }
        }
        
        private void CheckEnvironmentalAlerts()
        {
            // Temperature alerts
            CheckTemperatureAlerts();
            
            // Humidity alerts
            CheckHumidityAlerts();
            
            // Light alerts
            CheckLightAlerts();
            
            // CO2 alerts
            CheckCO2Alerts();
            
            // Air flow alerts
            CheckAirFlowAlerts();
        }
        
        private void CheckTemperatureAlerts()
        {
            float tempDifference = Mathf.Abs(_currentConditions.Temperature - _targetConditions.Temperature);
            
            if (tempDifference > 3f)
            {
                var alert = new EnvironmentalAlert
                {
                    AlertType = AlertType.Temperature,
                    Severity = tempDifference > 5f ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Message = $"Temperature deviation: {tempDifference:F1}°C from target",
                    CurrentValue = _currentConditions.Temperature,
                    TargetValue = _targetConditions.Temperature,
                    Timestamp = DateTime.Now
                };
                
                OnEnvironmentalAlert?.Invoke(this, alert);
            }
        }
        
        private void CheckHumidityAlerts()
        {
            float humidityDifference = Mathf.Abs(_currentConditions.Humidity - _targetConditions.Humidity);
            
            if (humidityDifference > 10f)
            {
                var alert = new EnvironmentalAlert
                {
                    AlertType = AlertType.Humidity,
                    Severity = humidityDifference > 20f ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Message = $"Humidity deviation: {humidityDifference:F1}% from target",
                    CurrentValue = _currentConditions.Humidity,
                    TargetValue = _targetConditions.Humidity,
                    Timestamp = DateTime.Now
                };
                
                OnEnvironmentalAlert?.Invoke(this, alert);
            }
        }
        
        private void CheckLightAlerts()
        {
            if (_lightingController != null && _lightingController.IsLightPeriod)
            {
                float lightDifference = Mathf.Abs(_currentConditions.LightIntensity - _targetConditions.LightIntensity);
                
                if (lightDifference > 100f)
                {
                    var alert = new EnvironmentalAlert
                    {
                        AlertType = AlertType.Light,
                        Severity = lightDifference > 200f ? AlertSeverity.Critical : AlertSeverity.Warning,
                        Message = $"Light intensity deviation: {lightDifference:F0} PPFD from target",
                        CurrentValue = _currentConditions.LightIntensity,
                        TargetValue = _targetConditions.LightIntensity,
                        Timestamp = DateTime.Now
                    };
                    
                    OnEnvironmentalAlert?.Invoke(this, alert);
                }
            }
        }
        
        private void CheckCO2Alerts()
        {
            float co2Difference = Mathf.Abs(_currentConditions.CO2Level - _targetConditions.CO2Level);
            
            if (co2Difference > 200f)
            {
                var alert = new EnvironmentalAlert
                {
                    AlertType = AlertType.CO2,
                    Severity = co2Difference > 400f ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Message = $"CO2 level deviation: {co2Difference:F0} ppm from target",
                    CurrentValue = _currentConditions.CO2Level,
                    TargetValue = _targetConditions.CO2Level,
                    Timestamp = DateTime.Now
                };
                
                OnEnvironmentalAlert?.Invoke(this, alert);
            }
        }
        
        private void CheckAirFlowAlerts()
        {
            if (_currentConditions.AirFlow < 0.2f)
            {
                var alert = new EnvironmentalAlert
                {
                    AlertType = AlertType.AirFlow,
                    Severity = AlertSeverity.Warning,
                    Message = "Poor air circulation detected",
                    CurrentValue = _currentConditions.AirFlow,
                    TargetValue = _targetConditions.AirFlow,
                    Timestamp = DateTime.Now
                };
                
                OnEnvironmentalAlert?.Invoke(this, alert);
            }
        }
        
        #endregion
        
        #region Automation System
        
        private void InitializeAutomation()
        {
            _automationProfile = CreateDefaultAutomationProfile();
            _automationActive = _enableAutomation;
        }
        
        private AutomationProfile CreateDefaultAutomationProfile()
        {
            return new AutomationProfile
            {
                ProfileName = "Standard Cannabis",
                TemperatureControl = new TemperatureAutomation
                {
                    Enabled = true,
                    TargetTemperature = _targetTemperature,
                    ToleranceDegrees = 1f,
                    ResponseSpeed = AutomationSpeed.Medium
                },
                HumidityControl = new HumidityAutomation
                {
                    Enabled = true,
                    TargetHumidity = _targetHumidity,
                    TolerancePercent = 5f,
                    ResponseSpeed = AutomationSpeed.Medium
                },
                LightingControl = new LightingAutomation
                {
                    Enabled = true,
                    Schedule = CreateDefaultLightSchedule(),
                    IntensityControl = true,
                    SpectrumControl = false
                },
                VentilationControl = new VentilationAutomation
                {
                    Enabled = true,
                    TargetAirFlow = _targetAirFlow,
                    CO2Triggered = true,
                    TemperatureTriggered = true
                },
                IrrigationControl = new IrrigationAutomation
                {
                    Enabled = true,
                    AutoWatering = true,
                    SoilMoistureThreshold = 30f,
                    Schedule = CreateDefaultWateringSchedule()
                }
            };
        }
        
        private WateringSchedule CreateDefaultWateringSchedule()
        {
            return new WateringSchedule
            {
                ScheduleName = "Daily Watering",
                WateringTimes = new List<TimeSpan> { new TimeSpan(8, 0, 0), new TimeSpan(20, 0, 0) },
                WateringDuration = 300, // 5 minutes
                IsActive = true
            };
        }
        
        private void ProcessAutomation()
        {
            if (!_automationActive) return;
            
            // Process each automation system
            ProcessTemperatureAutomation();
            ProcessHumidityAutomation();
            ProcessLightingAutomation();
            ProcessVentilationAutomation();
            ProcessIrrigationAutomation();
            
            // Intelligent optimization
            if (_enableIntelligentControl)
            {
                ProcessIntelligentOptimization();
            }
            
            // Predictive management
            if (_enablePredictiveManagement)
            {
                ProcessPredictiveManagement();
            }
        }
        
        private void ProcessTemperatureAutomation()
        {
            if (!_automationProfile.TemperatureControl.Enabled || _hvacController == null) return;
            
            float tempDifference = _currentConditions.Temperature - _automationProfile.TemperatureControl.TargetTemperature;
            float tolerance = _automationProfile.TemperatureControl.ToleranceDegrees;
            
            if (Mathf.Abs(tempDifference) > tolerance)
            {
                if (tempDifference > 0f)
                {
                    // Too hot - increase cooling
                    _hvacController.SetCoolingPower(Mathf.Clamp01(tempDifference / 5f));
                    _hvacController.SetHeatingPower(0f);
                }
                else
                {
                    // Too cold - increase heating
                    _hvacController.SetHeatingPower(Mathf.Clamp01(-tempDifference / 5f));
                    _hvacController.SetCoolingPower(0f);
                }
                
                LogAutomationEvent($"Temperature adjustment: {tempDifference:F1}°C difference");
            }
        }
        
        private void ProcessHumidityAutomation()
        {
            if (!_automationProfile.HumidityControl.Enabled || _hvacController == null) return;
            
            float humidityDifference = _currentConditions.Humidity - _automationProfile.HumidityControl.TargetHumidity;
            float tolerance = _automationProfile.HumidityControl.TolerancePercent;
            
            if (Mathf.Abs(humidityDifference) > tolerance)
            {
                if (humidityDifference > 0f)
                {
                    // Too humid - increase dehumidification
                    _hvacController.SetDehumidifierPower(Mathf.Clamp01(humidityDifference / 20f));
                    _hvacController.SetHumidifierPower(0f);
                }
                else
                {
                    // Too dry - increase humidification
                    _hvacController.SetHumidifierPower(Mathf.Clamp01(-humidityDifference / 20f));
                    _hvacController.SetDehumidifierPower(0f);
                }
                
                LogAutomationEvent($"Humidity adjustment: {humidityDifference:F1}% difference");
            }
        }
        
        private void ProcessLightingAutomation()
        {
            if (!_automationProfile.LightingControl.Enabled || _lightingController == null) return;
            
            var schedule = _automationProfile.LightingControl.Schedule;
            if (schedule != null && schedule.IsActive)
            {
                bool shouldBeOn = _lightingController.ShouldLightBeOn(schedule);
                
                if (shouldBeOn && !_lightingController.IsLightOn)
                {
                    _lightingController.TurnOnLights();
                    LogAutomationEvent("Lights turned on by schedule");
                }
                else if (!shouldBeOn && _lightingController.IsLightOn)
                {
                    _lightingController.TurnOffLights();
                    LogAutomationEvent("Lights turned off by schedule");
                }
                
                // Adjust intensity if needed
                if (_automationProfile.LightingControl.IntensityControl && _lightingController.IsLightOn)
                {
                    float targetIntensity = _targetConditions.LightIntensity;
                    float currentIntensity = _currentConditions.LightIntensity;
                    
                    if (Mathf.Abs(currentIntensity - targetIntensity) > 50f)
                    {
                        _lightingController.SetTargetIntensity(targetIntensity);
                        LogAutomationEvent($"Light intensity adjusted to {targetIntensity:F0} PPFD");
                    }
                }
            }
        }
        
        private void ProcessVentilationAutomation()
        {
            if (!_automationProfile.VentilationControl.Enabled || _ventilationController == null) return;
            
            float targetAirFlow = _automationProfile.VentilationControl.TargetAirFlow;
            
            // Adjust based on temperature
            if (_automationProfile.VentilationControl.TemperatureTriggered)
            {
                if (_currentConditions.Temperature > _targetConditions.Temperature + 2f)
                {
                    targetAirFlow = Mathf.Max(targetAirFlow, 1.2f);
                }
            }
            
            // Adjust based on CO2
            if (_automationProfile.VentilationControl.CO2Triggered)
            {
                if (_currentConditions.CO2Level > _targetConditions.CO2Level + 300f)
                {
                    targetAirFlow = Mathf.Max(targetAirFlow, 1.0f);
                }
            }
            
            if (Mathf.Abs(_currentConditions.AirFlow - targetAirFlow) > 0.1f)
            {
                _ventilationController.SetTargetAirFlow(targetAirFlow);
                LogAutomationEvent($"Ventilation adjusted to {targetAirFlow:F1} m/s");
            }
        }
        
        private void ProcessIrrigationAutomation()
        {
            if (!_automationProfile.IrrigationControl.Enabled || _irrigationController == null) return;
            
            if (_automationProfile.IrrigationControl.AutoWatering)
            {
                // Check if plants need watering
                var thirstyPlants = _activePlants.Where(p => p.WaterLevel < _automationProfile.IrrigationControl.SoilMoistureThreshold).ToList();
                
                if (thirstyPlants.Count > 0)
                {
                    StartCoroutine(AutoWaterPlants(thirstyPlants));
                    LogAutomationEvent($"Auto-watering {thirstyPlants.Count} plants");
                }
            }
        }
        
        private IEnumerator AutoWaterPlants(List<InteractivePlantComponent> plants)
        {
            foreach (var plant in plants)
            {
                if (plant != null)
                {
                    plant.WaterPlant(25f);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        
        private void ProcessIntelligentOptimization()
        {
            // Analyze plant performance and adjust environmental parameters
            AnalyzePlantPerformance();
            
            // Optimize energy consumption
            OptimizeEnergyConsumption();
            
            // Dynamic parameter adjustment based on plant feedback
            AdjustParametersBasedOnPlantHealth();
        }
        
        private void AnalyzePlantPerformance()
        {
            if (_activePlants.Count == 0) return;
            
            float averageHealth = _activePlants.Average(p => p.Health);
            float averageGrowthRate = _activePlants.Average(p => p.GrowthProgress);
            
            // Adjust targets based on plant performance
            if (averageHealth < 70f)
            {
                // Plants are struggling - be more conservative
                AdjustTargetsForStrugglingPlants();
            }
            else if (averageHealth > 90f && averageGrowthRate > 0.8f)
            {
                // Plants are thriving - optimize for growth
                AdjustTargetsForThrivingPlants();
            }
        }
        
        private void AdjustTargetsForStrugglingPlants()
        {
            // More stable temperature
            _targetConditions.Temperature = 23f;
            
            // Lower humidity to prevent mold
            _targetConditions.Humidity = 50f;
            
            // Increase air flow
            _targetConditions.AirFlow = 1.0f;
            
            LogAutomationEvent("Adjusted targets for struggling plants");
        }
        
        private void AdjustTargetsForThrivingPlants()
        {
            // Slightly higher temperature for faster growth
            _targetConditions.Temperature = 25f;
            
            // Optimal humidity
            _targetConditions.Humidity = 65f;
            
            // Higher CO2 for photosynthesis
            _targetConditions.CO2Level = 1400f;
            
            LogAutomationEvent("Adjusted targets for thriving plants");
        }
        
        private void OptimizeEnergyConsumption()
        {
            // Reduce unnecessary systems operation
            float currentOccupancy = OccupancyRate;
            
            if (currentOccupancy < 0.5f)
            {
                // Reduce power for low occupancy
                if (_lightingController != null)
                {
                    _lightingController.SetPowerEfficiencyMode(true);
                }
                
                if (_hvacController != null)
                {
                    _hvacController.SetEcoMode(true);
                }
                
                LogAutomationEvent("Energy optimization: Eco mode enabled");
            }
        }
        
        private void AdjustParametersBasedOnPlantHealth()
        {
            var unhealthyPlants = _activePlants.Where(p => p.Health < 60f).ToList();
            
            if (unhealthyPlants.Count > _activePlants.Count * 0.3f)
            {
                // More than 30% of plants are unhealthy
                TriggerEmergencyOptimization();
            }
        }
        
        private void TriggerEmergencyOptimization()
        {
            var alert = new EnvironmentalAlert
            {
                AlertType = AlertType.PlantHealth,
                Severity = AlertSeverity.Critical,
                Message = "Multiple plants showing poor health - emergency optimization activated",
                Timestamp = DateTime.Now
            };
            
            OnEnvironmentalAlert?.Invoke(this, alert);
            
            // Reset to safe defaults
            _targetConditions = new EnvironmentalConditions
            {
                Temperature = 23f,
                Humidity = 55f,
                CO2Level = 1000f,
                LightIntensity = 500f,
                AirFlow = 0.8f
            };
            
            LogAutomationEvent("Emergency optimization activated");
        }
        
        private void ProcessPredictiveManagement()
        {
            // Predict future environmental conditions
            PredictEnvironmentalChanges();
            
            // Predict plant needs
            PredictPlantNeeds();
            
            // Preemptive adjustments
            MakePreemptiveAdjustments();
        }
        
        private void PredictEnvironmentalChanges()
        {
            // Simple trend analysis
            if (_environmentalHistory.Count >= 60) // At least 1 hour of data
            {
                var recentData = _environmentalHistory.TakeLast(60).ToList();
                
                // Temperature trend
                float tempTrend = CalculateTrend(recentData.Select(r => r.Temperature).ToList());
                
                if (Math.Abs(tempTrend) > 0.1f)
                {
                    // Preemptively adjust HVAC
                    if (_hvacController != null)
                    {
                        _hvacController.MakePreemptiveAdjustment(tempTrend);
                    }
                }
            }
        }
        
        private float CalculateTrend(List<float> values)
        {
            if (values.Count < 2) return 0f;
            
            // Simple linear regression slope
            float n = values.Count;
            float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f;
            
            for (int i = 0; i < values.Count; i++)
            {
                sumX += i;
                sumY += values[i];
                sumXY += i * values[i];
                sumX2 += i * i;
            }
            
            return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        }
        
        private void PredictPlantNeeds()
        {
            foreach (var plant in _activePlants)
            {
                // Predict when plant will need watering
                if (plant.WaterLevel > 20f)
                {
                    float waterConsumptionRate = CalculateWaterConsumptionRate(plant);
                    float timeToWatering = plant.WaterLevel / waterConsumptionRate;
                    
                    if (timeToWatering < 2f) // Less than 2 hours
                    {
                        // Schedule preemptive watering
                        StartCoroutine(SchedulePreemptiveWatering(plant, timeToWatering * 0.8f));
                    }
                }
            }
        }
        
        private float CalculateWaterConsumptionRate(InteractivePlantComponent plant)
        {
            // Estimate based on stage, environmental conditions, and plant health
            float baseRate = plant.CurrentStage switch
            {
                PlantGrowthStage.Seed => 0.5f,
                PlantGrowthStage.Sprout => 1f,
                PlantGrowthStage.Vegetative => 2f,
                PlantGrowthStage.Flowering => 3f,
                PlantGrowthStage.Harvest => 1f,
                _ => 1f
            };
            
            // Environmental modifiers
            if (_currentConditions.Temperature > 26f)
                baseRate *= 1.5f;
            
            if (_currentConditions.Humidity < 50f)
                baseRate *= 1.3f;
            
            return baseRate; // Per hour
        }
        
        private IEnumerator SchedulePreemptiveWatering(InteractivePlantComponent plant, float delayHours)
        {
            yield return new WaitForSeconds(delayHours * 3600f); // Convert to seconds
            
            if (plant != null && plant.WaterLevel < 30f)
            {
                plant.WaterPlant(20f);
                LogAutomationEvent($"Preemptive watering for {plant.PlantData.StrainName}");
            }
        }
        
        private void MakePreemptiveAdjustments()
        {
            // Make small adjustments to prevent larger deviations
            PreemptiveTemperatureAdjustment();
            PreemptiveHumidityAdjustment();
            PreemptiveLightAdjustment();
        }
        
        private void PreemptiveTemperatureAdjustment()
        {
            float tempDifference = _currentConditions.Temperature - _targetConditions.Temperature;
            
            if (Mathf.Abs(tempDifference) > 0.5f && _hvacController != null)
            {
                // Small preemptive adjustment
                float adjustment = tempDifference * 0.1f;
                _hvacController.MakePreemptiveAdjustment(-adjustment);
            }
        }
        
        private void PreemptiveHumidityAdjustment()
        {
            float humidityDifference = _currentConditions.Humidity - _targetConditions.Humidity;
            
            if (Mathf.Abs(humidityDifference) > 3f && _hvacController != null)
            {
                // Small preemptive adjustment
                float adjustment = humidityDifference * 0.1f;
                _hvacController.MakePreemptiveHumidityAdjustment(-adjustment);
            }
        }
        
        private void PreemptiveLightAdjustment()
        {
            if (_lightingController != null && _lightingController.IsLightOn)
            {
                float lightDifference = _currentConditions.LightIntensity - _targetConditions.LightIntensity;
                
                if (Mathf.Abs(lightDifference) > 25f)
                {
                    float adjustment = lightDifference * 0.1f;
                    _lightingController.MakePreemptiveIntensityAdjustment(-adjustment);
                }
            }
        }
        
        private void LogAutomationEvent(string message)
        {
            var automationEvent = new AutomationEvent
            {
                Timestamp = DateTime.Now,
                EventType = AutomationEventType.SystemAdjustment,
                Message = message,
                RoomName = _roomName
            };
            
            OnAutomationEvent?.Invoke(this, automationEvent);
            Debug.Log($"[{_roomName}] Automation: {message}");
        }
        
        #endregion
        
        #region Plant Management
        
        public bool AddPlant(GameObject plantPrefab)
        {
            if (_activePlants.Count >= _maxPlantCapacity)
                return false;
            
            // Find available position
            Transform availablePosition = FindAvailablePosition();
            if (availablePosition == null)
                return false;
            
            // Instantiate plant
            GameObject plantGO = Instantiate(plantPrefab, availablePosition.position, availablePosition.rotation);
            plantGO.transform.SetParent(_plantContainer);
            
            // Get plant component
            var plantComponent = plantGO.GetComponent<InteractivePlantComponent>();
            if (plantComponent == null)
            {
                plantComponent = plantGO.AddComponent<InteractivePlantComponent>();
            }
            
            // Add to active plants
            _activePlants.Add(plantComponent);
            
            // Subscribe to plant events
            plantComponent.OnPlantDied += OnPlantDied;
            plantComponent.OnPlantHarvestReady += OnPlantHarvestReady;
            plantComponent.OnHealthChanged += OnPlantHealthChanged;
            
            OnPlantAdded?.Invoke(this, plantComponent);
            
            // Update room status
            UpdateRoomStatus();
            
            Debug.Log($"Added plant to {_roomName}. Current count: {_activePlants.Count}/{_maxPlantCapacity}");
            return true;
        }
        
        private Transform FindAvailablePosition()
        {
            for (int i = 0; i < _plantPositions.Length; i++)
            {
                if (IsPositionAvailable(_plantPositions[i]))
                {
                    return _plantPositions[i];
                }
            }
            return null;
        }
        
        private bool IsPositionAvailable(Transform position)
        {
            Collider[] overlapping = Physics.OverlapSphere(position.position, 0.3f, _plantLayerMask);
            return overlapping.Length == 0;
        }
        
        public bool RemovePlant(InteractivePlantComponent plant)
        {
            if (!_activePlants.Contains(plant))
                return false;
            
            // Unsubscribe from events
            plant.OnPlantDied -= OnPlantDied;
            plant.OnPlantHarvestReady -= OnPlantHarvestReady;
            plant.OnHealthChanged -= OnPlantHealthChanged;
            
            _activePlants.Remove(plant);
            OnPlantRemoved?.Invoke(this, plant);
            
            // Update room status
            UpdateRoomStatus();
            
            Debug.Log($"Removed plant from {_roomName}. Current count: {_activePlants.Count}/{_maxPlantCapacity}");
            return true;
        }
        
        private void OnPlantDied(InteractivePlantComponent plant)
        {
            RemovePlant(plant);
            
            var alert = new EnvironmentalAlert
            {
                AlertType = AlertType.PlantHealth,
                Severity = AlertSeverity.Warning,
                Message = $"Plant died in {_roomName}: {plant.PlantData.StrainName}",
                Timestamp = DateTime.Now
            };
            
            OnEnvironmentalAlert?.Invoke(this, alert);
        }
        
        private void OnPlantHarvestReady(InteractivePlantComponent plant)
        {
            Debug.Log($"Plant ready for harvest in {_roomName}: {plant.PlantData.StrainName}");
        }
        
        private void OnPlantHealthChanged(InteractivePlantComponent plant)
        {
            float newHealth = plant.Health * 100f; // Convert to percentage
            if (newHealth < 30f)
            {
                var alert = new EnvironmentalAlert
                {
                    AlertType = AlertType.PlantHealth,
                    Severity = AlertSeverity.Warning,
                    Message = $"Plant health critical in {_roomName}: {plant.PlantData.StrainName} ({newHealth:F0}%)",
                    Timestamp = DateTime.Now
                };
                
                OnEnvironmentalAlert?.Invoke(this, alert);
            }
        }
        
        #endregion
        
        #region Monitoring and Analytics
        
        private void StartMonitoring()
        {
            InvokeRepeating(nameof(CollectEnvironmentalData), 0f, _dataCollectionInterval);
        }
        
        private void CollectEnvironmentalData()
        {
            var reading = new EnvironmentalReading
            {
                Timestamp = DateTime.Now,
                Temperature = _currentConditions.Temperature,
                Humidity = _currentConditions.Humidity,
                LightIntensity = _currentConditions.LightIntensity,
                CO2Level = _currentConditions.CO2Level,
                AirFlow = _currentConditions.AirFlow
            };
            
            _environmentalHistory.Enqueue(reading);
            
            // Maintain maximum history size
            while (_environmentalHistory.Count > MAX_HISTORY_SIZE)
            {
                _environmentalHistory.Dequeue();
            }
        }
        
        private void AnalyzePlantHealth()
        {
            if (_activePlants.Count == 0)
            {
                _plantAnalytics = new PlantHealthAnalytics();
                return;
            }
            
            _plantAnalytics.TotalPlants = _activePlants.Count;
            _plantAnalytics.AverageHealth = _activePlants.Average(p => p.Health);
            _plantAnalytics.AverageGrowthRate = _activePlants.Average(p => p.GrowthProgress);
            _plantAnalytics.HealthyPlants = _activePlants.Count(p => p.Health >= 70f);
            _plantAnalytics.UnhealthyPlants = _activePlants.Count(p => p.Health < 50f);
            _plantAnalytics.HarvestablePlants = _activePlants.Count(p => p.IsHarvestable);
            
            // Calculate growth stage distribution
            _plantAnalytics.StageDistribution = new Dictionary<PlantGrowthStage, int>();
            foreach (PlantGrowthStage stage in Enum.GetValues(typeof(PlantGrowthStage)))
            {
                _plantAnalytics.StageDistribution[stage] = _activePlants.Count(p => p.CurrentStage == stage);
            }
            
            _plantAnalytics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateResourceConsumption()
        {
            _resourceConsumption.PowerConsumption = CalculatePowerConsumption();
            _resourceConsumption.WaterConsumption = CalculateWaterConsumption();
            _resourceConsumption.CO2Consumption = CalculateCO2Consumption();
            _resourceConsumption.NutrientConsumption = CalculateNutrientConsumption();
            _resourceConsumption.LastUpdate = DateTime.Now;
        }
        
        private float CalculatePowerConsumption()
        {
            float power = 0f;
            
            if (_lightingController != null)
                power += _lightingController.PowerConsumption;
            
            if (_hvacController != null)
                power += _hvacController.PowerConsumption;
            
            if (_ventilationController != null)
                power += _ventilationController.PowerConsumption;
            
            if (_co2Controller != null)
                power += _co2Controller.PowerConsumption;
            
            return power;
        }
        
        private float CalculateWaterConsumption()
        {
            return _activePlants.Sum(p => CalculateWaterConsumptionRate(p));
        }
        
        private float CalculateCO2Consumption()
        {
            // Plants consume CO2 during photosynthesis
            float plantConsumption = _activePlants.Count * 0.5f; // kg/hour per plant
            
            // CO2 injection system
            float injectionRate = _co2Controller?.InjectionRate ?? 0f;
            
            return injectionRate - plantConsumption;
        }
        
        private float CalculateNutrientConsumption()
        {
            return _activePlants.Sum(p => p.CurrentStage switch
            {
                PlantGrowthStage.Vegetative => 0.2f,
                PlantGrowthStage.Flowering => 0.3f,
                _ => 0.1f
            });
        }
        
        #endregion
        
        #region Room Status Management
        
        private void UpdateRoomStatus()
        {
            RoomStatus newStatus = DetermineRoomStatus();
            
            if (newStatus != _currentStatus)
            {
                SetRoomStatus(newStatus);
            }
        }
        
        private RoomStatus DetermineRoomStatus()
        {
            if (_activePlants.Count == 0)
                return RoomStatus.Empty;
            
            if (_activePlants.Count == _maxPlantCapacity)
                return RoomStatus.Full;
            
            // Check environmental conditions
            bool environmentalOK = IsEnvironmentalConditionsOK();
            if (!environmentalOK)
                return RoomStatus.Alert;
            
            // Check plant health
            float averageHealth = _activePlants.Average(p => p.Health);
            if (averageHealth < 50f)
                return RoomStatus.Alert;
            
            if (averageHealth > 80f)
                return RoomStatus.Optimal;
            
            return RoomStatus.Active;
        }
        
        private bool IsEnvironmentalConditionsOK()
        {
            float tempDiff = Mathf.Abs(_currentConditions.Temperature - _targetConditions.Temperature);
            float humidityDiff = Mathf.Abs(_currentConditions.Humidity - _targetConditions.Humidity);
            
            return tempDiff < 3f && humidityDiff < 15f;
        }
        
        private void SetRoomStatus(RoomStatus newStatus)
        {
            _currentStatus = newStatus;
            OnRoomStatusChanged?.Invoke(this);
            
            Debug.Log($"Room {_roomName} status changed to: {newStatus}");
        }
        
        #endregion
        
        #region Visual Feedback
        
        private void UpdateVisualIndicators()
        {
            UpdateStatusIndicator();
            UpdateStatusLight();
            UpdateAirflowParticles();
            UpdateInfoDisplay();
        }
        
        private void UpdateStatusIndicator()
        {
            if (_roomStatusIndicator == null) return;
            
            Color statusColor = _currentStatus switch
            {
                RoomStatus.Empty => Color.gray,
                RoomStatus.Initializing => Color.yellow,
                RoomStatus.Active => Color.green,
                RoomStatus.Optimal => Color.cyan,
                RoomStatus.Full => Color.blue,
                RoomStatus.Alert => Color.red,
                RoomStatus.Maintenance => Color.orange,
                _ => Color.white
            };
            
            _roomStatusIndicator.material.color = statusColor;
        }
        
        private void UpdateStatusLight()
        {
            if (_roomStatusLight == null) return;
            
            Color lightColor = _currentStatus switch
            {
                RoomStatus.Optimal => Color.green,
                RoomStatus.Active => Color.blue,
                RoomStatus.Alert => Color.red,
                RoomStatus.Empty => Color.gray,
                _ => Color.white
            };
            
            _roomStatusLight.color = lightColor;
            _roomStatusLight.enabled = _currentStatus != RoomStatus.Empty;
        }
        
        private void UpdateAirflowParticles()
        {
            if (_airflowParticles == null) return;
            
            var emission = _airflowParticles.emission;
            emission.rateOverTime = _currentConditions.AirFlow * 20f;
            
            var velocityOverLifetime = _airflowParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(_currentConditions.AirFlow);
        }
        
        private void UpdateInfoDisplay()
        {
            if (_roomInfoText == null || _roomInfoCanvas == null) return;
            
            string infoText = $"<b>{_roomName}</b>\n" +
                             $"Status: {_currentStatus}\n" +
                             $"Plants: {_activePlants.Count}/{_maxPlantCapacity}\n" +
                             $"Temp: {_currentConditions.Temperature:F1}°C\n" +
                             $"Humidity: {_currentConditions.Humidity:F0}%\n" +
                             $"Light: {_currentConditions.LightIntensity:F0} PPFD\n" +
                             $"CO2: {_currentConditions.CO2Level:F0} ppm";
            
            if (_plantAnalytics.TotalPlants > 0)
            {
                infoText += $"\nAvg Health: {_plantAnalytics.AverageHealth:F0}%";
            }
            
            _roomInfoText.text = infoText;
            
            // Show info when room is selected or has alerts
            bool showInfo = _currentStatus == RoomStatus.Alert || Input.GetKey(KeyCode.LeftShift);
            _roomInfoCanvas.gameObject.SetActive(showInfo);
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Get comprehensive room status information
        /// </summary>
        public RoomStatusInfo GetRoomStatus()
        {
            return new RoomStatusInfo
            {
                RoomName = _roomName,
                RoomType = _roomType,
                Status = _currentStatus,
                PlantCount = _activePlants.Count,
                MaxPlants = _maxPlantCapacity,
                OccupancyRate = OccupancyRate,
                AverageHealth = _plantAnalytics.AverageHealth,
                Conditions = _currentConditions,
                TargetConditions = _targetConditions,
                AutomationEnabled = _automationActive,
                ResourceConsumption = _resourceConsumption,
                LastUpdate = DateTime.Now
            };
        }
        
        /// <summary>
        /// Get environmental history for analytics
        /// </summary>
        public List<EnvironmentalReading> GetEnvironmentalHistory(int hours = 24)
        {
            int recordCount = Mathf.Min(hours * 60, _environmentalHistory.Count);
            return _environmentalHistory.TakeLast(recordCount).ToList();
        }
        
        /// <summary>
        /// Enable or disable automation
        /// </summary>
        public void SetAutomationEnabled(bool enabled)
        {
            _automationActive = enabled;
            LogAutomationEvent($"Automation {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Update target environmental conditions
        /// </summary>
        public void UpdateTargetConditions(EnvironmentalConditions newTargets)
        {
            _targetConditions = newTargets;
            ConfigureEnvironmentalSystems();
            LogAutomationEvent("Target conditions updated");
        }
        
        /// <summary>
        /// Get list of harvestable plants
        /// </summary>
        public List<InteractivePlantComponent> GetHarvestablePlants()
        {
            return _activePlants.Where(p => p.IsHarvestable).ToList();
        }
        
        /// <summary>
        /// Emergency shutdown of all systems
        /// </summary>
        public void EmergencyShutdown()
        {
            SetRoomStatus(RoomStatus.Maintenance);
            _automationActive = false;
            
            // Turn off all systems
            if (_lightingController != null)
                _lightingController.TurnOffLights();
            
            if (_hvacController != null)
                _hvacController.SetEmergencyMode(true);
            
            if (_co2Controller != null)
                _co2Controller.StopInjection();
            
            LogAutomationEvent("Emergency shutdown activated");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up event listeners
            foreach (var plant in _activePlants)
            {
                if (plant != null)
                {
                    plant.OnPlantDied -= OnPlantDied;
                    plant.OnPlantHarvestReady -= OnPlantHarvestReady;
                    plant.OnHealthChanged -= OnPlantHealthChanged;
                }
            }
            
            StopAllCoroutines();
            CancelInvoke();
        }
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            // Draw room bounds
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, _roomDimensions);
            
            // Draw plant positions
            if (_plantPositions != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var position in _plantPositions)
                {
                    if (position != null)
                    {
                        Gizmos.DrawWireSphere(position.position, 0.2f);
                    }
                }
            }
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class RoomStatusInfo
    {
        public string RoomName;
        public FacilityRoomType RoomType;
        public RoomStatus Status;
        public int PlantCount;
        public int MaxPlants;
        public float OccupancyRate;
        public float AverageHealth;
        public EnvironmentalConditions Conditions;
        public EnvironmentalConditions TargetConditions;
        public bool AutomationEnabled;
        public ResourceConsumption ResourceConsumption;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class EnvironmentalReading
    {
        public DateTime Timestamp;
        public float Temperature;
        public float Humidity;
        public float LightIntensity;
        public float CO2Level;
        public float AirFlow;
    }
    
    [System.Serializable]
    public class PlantHealthAnalytics
    {
        public int TotalPlants;
        public float AverageHealth;
        public float AverageGrowthRate;
        public int HealthyPlants;
        public int UnhealthyPlants;
        public int HarvestablePlants;
        public Dictionary<PlantGrowthStage, int> StageDistribution = new Dictionary<PlantGrowthStage, int>();
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class ResourceConsumption
    {
        public float PowerConsumption; // kW
        public float WaterConsumption; // L/hour
        public float CO2Consumption; // kg/hour
        public float NutrientConsumption; // L/hour
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class EnvironmentalAlert
    {
        public AlertType AlertType;
        public AlertSeverity Severity;
        public string Message;
        public float CurrentValue;
        public float TargetValue;
        public DateTime Timestamp;
    }
    
    [System.Serializable]
    public class AutomationEvent
    {
        public DateTime Timestamp;
        public AutomationEventType EventType;
        public string Message;
        public string RoomName;
    }
    
    [System.Serializable]
    public enum RoomStatus
    {
        Empty,
        Initializing,
        Active,
        Optimal,
        Full,
        Alert,
        Maintenance
    }
    
    [System.Serializable]
    public enum AlertType
    {
        Temperature,
        Humidity,
        Light,
        CO2,
        AirFlow,
        PlantHealth,
        System
    }
    
    [System.Serializable]
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
    
    [System.Serializable]
    public enum AutomationEventType
    {
        SystemAdjustment,
        ScheduledAction,
        EmergencyResponse,
        OptimizationChange
    }

    [System.Serializable]
    public class AutomationProfile
    {
        public string ProfileName;
        public TemperatureAutomation TemperatureControl;
        public HumidityAutomation HumidityControl;
        public LightingAutomation LightingControl;
        public VentilationAutomation VentilationControl;
        public IrrigationAutomation IrrigationControl;
    }

    [System.Serializable]
    public class TemperatureAutomation
    {
        public bool Enabled;
        public float TargetTemperature;
        public float ToleranceDegrees;
        public AutomationSpeed ResponseSpeed;
    }

    [System.Serializable]
    public class HumidityAutomation
    {
        public bool Enabled;
        public float TargetHumidity;
        public float TolerancePercent;
        public AutomationSpeed ResponseSpeed;
    }

    [System.Serializable]
    public class LightingAutomation
    {
        public bool Enabled;
        public float TargetIntensity;
        public AutomationSpeed ResponseSpeed;
        public LightSchedule Schedule;
        public bool IntensityControl;
        public bool SpectrumControl;
    }

    [System.Serializable]
    public class VentilationAutomation
    {
        public bool Enabled;
        public float TargetAirFlow;
        public AutomationSpeed ResponseSpeed;
        public bool CO2Triggered;
        public bool TemperatureTriggered;
    }

    [System.Serializable]
    public class IrrigationAutomation
    {
        public bool Enabled;
        public WateringSchedule Schedule;
        public AutomationSpeed ResponseSpeed;
        public bool AutoWatering;
        public float SoilMoistureThreshold;
    }

    [System.Serializable]
    public class WateringSchedule
    {
        public string ScheduleName;
        public List<TimeSpan> WateringTimes;
        public int WateringDuration; // in seconds
        public bool IsActive;
    }

    [System.Serializable]
    public enum AutomationSpeed
    {
        Slow,
        Medium,
        Fast
    }
    
    // Plant data class for proper property access
    [System.Serializable]
    public class PlantDataInfo
    {
        public float WaterLevel;
        public float Health;
        public float GrowthProgress;
        public PlantGrowthStage CurrentStage;
        public string StrainName;
        public string Id;
        
        public PlantDataInfo(float waterLevel, float health, float growthProgress, PlantGrowthStage currentStage, string strainName, string id)
        {
            WaterLevel = waterLevel;
            Health = health;
            GrowthProgress = growthProgress;
            CurrentStage = currentStage;
            StrainName = strainName;
            Id = id;
        }
    }

    // Stub class for InteractivePlantComponent (deleted file)
    public class InteractivePlantComponent : MonoBehaviour
    {
        [Header("Plant Properties")]
        [SerializeField] private float _waterLevel = 0.5f;
        [SerializeField] private float _health = 1.0f;
        [SerializeField] private float _growthProgress = 0.0f;
        [SerializeField] private PlantGrowthStage _currentStage = PlantGrowthStage.Seedling;
        [SerializeField] private string _strainName = "Unknown Strain";
        
        public event System.Action<InteractivePlantComponent> OnPlantDied;
        public event System.Action<InteractivePlantComponent> OnPlantHarvestReady;
        public event System.Action<InteractivePlantComponent> OnHealthChanged;
        
        // Properties
        public float WaterLevel 
        { 
            get => _waterLevel; 
            set => _waterLevel = Mathf.Clamp01(value); 
        }
        
        public float Health 
        { 
            get => _health; 
            set 
            { 
                _health = Mathf.Clamp01(value);
                OnHealthChanged?.Invoke(this);
            } 
        }
        
        public float GrowthProgress 
        { 
            get => _growthProgress; 
            set => _growthProgress = Mathf.Clamp01(value); 
        }
        
        public PlantGrowthStage CurrentStage 
        { 
            get => _currentStage; 
            set => _currentStage = value; 
        }
        
        public string StrainName 
        { 
            get => _strainName; 
            set => _strainName = value; 
        }
        
        public bool IsHarvestable => _currentStage == PlantGrowthStage.Harvest || _currentStage == PlantGrowthStage.Harvestable;
        
        public PlantDataInfo PlantData => new PlantDataInfo(
            _waterLevel, 
            _health, 
            _growthProgress, 
            _currentStage,
            _strainName,
            GetInstanceID().ToString()
        );
        
        // Methods
        public void WaterPlant(float amount = 0.2f)
        {
            WaterLevel = Mathf.Min(1.0f, WaterLevel + amount);
        }
        
        public void TriggerPlantDied()
        {
            OnPlantDied?.Invoke(this);
        }
        
        public void TriggerPlantHarvestReady()
        {
            OnPlantHarvestReady?.Invoke(this);
        }
        
        public void TriggerHealthChanged()
        {
            OnHealthChanged?.Invoke(this);
        }
    }
}