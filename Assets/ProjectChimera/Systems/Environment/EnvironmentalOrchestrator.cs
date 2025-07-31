using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Automation;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using LightSpectrumData = ProjectChimera.Data.Environment.LightSpectrumData;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
using SensorReading = ProjectChimera.Data.Automation.SensorReading;
using SensorType = ProjectChimera.Data.Automation.SensorType;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// PC013-6e: Main coordinator class for environmental operations
    /// Orchestrates specialized environmental services to provide a unified interface
    /// while maintaining separation of concerns and single responsibility principle.
    /// Replaces the monolithic EnvironmentalManager.cs.
    /// </summary>
    public class EnvironmentalOrchestrator : ChimeraManager
    {
        [Header("Orchestrator Configuration")]
        [SerializeField] private bool _enableAutoEnvironmentalControl = true;
        [SerializeField] private float _environmentalUpdateInterval = 30f; // 30 seconds
        [SerializeField] private float _coordinationSensitivity = 1f;
        [SerializeField] private int _maxEnvironmentalZones = 25;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<EnvironmentalConditions> _onEnvironmentalOptimization;
        [SerializeField] private GameEventSO<string> _onEnvironmentalAlert;
        [SerializeField] private GameEventSO<EnvironmentalConditions> _onConditionsChanged;
        
        // Specialized service components
        private ClimateControlManager _climateControlManager;
        private HVACSystemManager _hvacSystemManager;
        private LightingSystemManager _lightingSystemManager;
        private SensorNetworkManager _sensorNetworkManager;
        
        // Dependencies
        // Note: PlantManager dependency removed to prevent circular assembly references
        
        // Timing
        private float _lastEnvironmentalUpdate = 0f;
        
        public string ManagerName => "Environmental Orchestrator";
        
        // Unified Properties (delegating to specialized services)
        public int ActiveClimateZones => _climateControlManager?.ActiveClimateZones ?? 0;
        public int ActiveHVACZones => _hvacSystemManager?.ActiveHVACZones ?? 0;
        public int ActiveLightingZones => _lightingSystemManager?.ActiveLightingZones ?? 0;
        public int TotalSensors => _sensorNetworkManager?.TotalSensors ?? 0;
        public int TotalActiveAlerts => 
            (_climateControlManager?.ActiveAlerts ?? 0) +
            (_hvacSystemManager?.ActiveAlerts ?? 0) +
            (_lightingSystemManager?.ActiveAlerts ?? 0) +
            (_sensorNetworkManager?.ActiveAlerts ?? 0);
        
        public bool EnableAutoEnvironmentalControl 
        { 
            get => _enableAutoEnvironmentalControl; 
            set => _enableAutoEnvironmentalControl = value; 
        }
        public float CoordinationSensitivity 
        { 
            get => _coordinationSensitivity; 
            set => _coordinationSensitivity = Mathf.Clamp(value, 0.1f, 3f); 
        }
        
        // Service Access Properties
        public ClimateControlManager ClimateControl => _climateControlManager;
        public HVACSystemManager HVACSystem => _hvacSystemManager;
        public LightingSystemManager LightingSystem => _lightingSystemManager;
        public SensorNetworkManager SensorNetwork => _sensorNetworkManager;
        
        protected override void OnManagerInitialize()
        {
            Debug.Log("[EnvironmentalOrchestrator] Initializing environmental orchestration system...");
            
            InitializeServices();
            ConnectEventHandlers();
            
            _lastEnvironmentalUpdate = Time.time;
            
            Debug.Log($"[EnvironmentalOrchestrator] Initialized. Auto-control: {_enableAutoEnvironmentalControl}, Max zones: {_maxEnvironmentalZones}");
        }
        
        protected override void OnManagerShutdown()
        {
            Debug.Log("[EnvironmentalOrchestrator] Shutting down environmental orchestration...");
            
            DisconnectEventHandlers();
            ShutdownServices();
        }
        
        protected override void Update()
        {
            if (!IsInitialized || !_enableAutoEnvironmentalControl) return;
            
            // Handle environmental coordination updates
            if (Time.time - _lastEnvironmentalUpdate >= _environmentalUpdateInterval)
            {
                CoordinateEnvironmentalSystems();
                _lastEnvironmentalUpdate = Time.time;
            }
        }
        
        #region Service Initialization
        
        private void InitializeServices()
        {
            // Get or create service components
            _climateControlManager = GetOrCreateComponent<ClimateControlManager>();
            _hvacSystemManager = GetOrCreateComponent<HVACSystemManager>();
            _lightingSystemManager = GetOrCreateComponent<LightingSystemManager>();
            _sensorNetworkManager = GetOrCreateComponent<SensorNetworkManager>();
            
            // Note: PlantManager dependency removed to prevent circular assembly references
            // Environmental systems operate independently and communicate via events
            
            // Initialize all services
            _climateControlManager?.Initialize();
            _hvacSystemManager?.Initialize();
            _lightingSystemManager?.Initialize();
            _sensorNetworkManager?.Initialize();
        }
        
        private void ShutdownServices()
        {
            _climateControlManager?.Shutdown();
            _hvacSystemManager?.Shutdown();
            _lightingSystemManager?.Shutdown();
            _sensorNetworkManager?.Shutdown();
        }
        
        private T GetOrCreateComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        
        #endregion
        
        #region Event Handling
        
        private void ConnectEventHandlers()
        {
            if (_climateControlManager != null)
            {
                _climateControlManager.OnClimateOptimized += OnClimateOptimized;
                _climateControlManager.OnClimateAlert += OnClimateAlert;
                _climateControlManager.OnTemperatureChanged += OnTemperatureChanged;
                _climateControlManager.OnHumidityChanged += OnHumidityChanged;
            }
            
            if (_hvacSystemManager != null)
            {
                _hvacSystemManager.OnEnvironmentControlled += OnHVACEnvironmentControlled;
                _hvacSystemManager.OnHVACAlert += OnHVACAlert;
                _hvacSystemManager.OnPowerConsumptionChanged += OnPowerConsumptionChanged;
            }
            
            if (_lightingSystemManager != null)
            {
                _lightingSystemManager.OnPPFDChanged += OnPPFDChanged;
                _lightingSystemManager.OnLightingAlert += OnLightingAlert;
                _lightingSystemManager.OnPhotoperiodChanged += OnPhotoperiodChanged;
            }
            
            if (_sensorNetworkManager != null)
            {
                _sensorNetworkManager.OnSensorDataReceived += OnSensorDataReceived;
                _sensorNetworkManager.OnSensorAlert += OnSensorAlert;
                _sensorNetworkManager.OnSensorStatusChanged += OnSensorStatusChanged;
            }
        }
        
        private void DisconnectEventHandlers()
        {
            if (_climateControlManager != null)
            {
                _climateControlManager.OnClimateOptimized -= OnClimateOptimized;
                _climateControlManager.OnClimateAlert -= OnClimateAlert;
                _climateControlManager.OnTemperatureChanged -= OnTemperatureChanged;
                _climateControlManager.OnHumidityChanged -= OnHumidityChanged;
            }
            
            if (_hvacSystemManager != null)
            {
                _hvacSystemManager.OnEnvironmentControlled -= OnHVACEnvironmentControlled;
                _hvacSystemManager.OnHVACAlert -= OnHVACAlert;
                _hvacSystemManager.OnPowerConsumptionChanged -= OnPowerConsumptionChanged;
            }
            
            if (_lightingSystemManager != null)
            {
                _lightingSystemManager.OnPPFDChanged -= OnPPFDChanged;
                _lightingSystemManager.OnLightingAlert -= OnLightingAlert;
                _lightingSystemManager.OnPhotoperiodChanged -= OnPhotoperiodChanged;
            }
            
            if (_sensorNetworkManager != null)
            {
                _sensorNetworkManager.OnSensorDataReceived -= OnSensorDataReceived;
                _sensorNetworkManager.OnSensorAlert -= OnSensorAlert;
                _sensorNetworkManager.OnSensorStatusChanged -= OnSensorStatusChanged;
            }
        }
        
        #endregion
        
        #region Public API - Environmental Operations
        
        /// <summary>
        /// Creates a comprehensive environmental zone with all systems
        /// </summary>
        public string CreateEnvironmentalZone(string zoneName, float zoneArea, PlantGrowthStage growthStage, EnvironmentalConditions targetConditions)
        {
            if (ActiveClimateZones + ActiveHVACZones + ActiveLightingZones >= _maxEnvironmentalZones)
            {
                Debug.LogWarning($"[EnvironmentalOrchestrator] Maximum environmental zone limit ({_maxEnvironmentalZones}) reached");
                return null;
            }
            
            string zoneId = System.Guid.NewGuid().ToString();
            
            // Create zone in all relevant services
            bool climateCreated = _climateControlManager?.CreateClimateZone(zoneId, targetConditions) ?? false;
            bool hvacCreated = _hvacSystemManager?.CreateHVACZone(zoneId, zoneArea, targetConditions) ?? false;
            bool lightingCreated = _lightingSystemManager?.CreateLightingZone(zoneId, zoneArea, growthStage) ?? false;
            
            // Deploy basic sensors
            DeployBasicSensors(zoneId);
            
            if (climateCreated || hvacCreated || lightingCreated)
            {
                Debug.Log($"[EnvironmentalOrchestrator] Created environmental zone '{zoneName}' (ID: {zoneId})");
                return zoneId;
            }
            
            Debug.LogError($"[EnvironmentalOrchestrator] Failed to create environmental zone '{zoneName}'");
            return null;
        }
        
        /// <summary>
        /// Optimizes environmental conditions for a specific growth stage
        /// </summary>
        public void OptimizeEnvironmentForGrowthStage(string zoneId, PlantGrowthStage growthStage)
        {
            if (string.IsNullOrEmpty(zoneId))
            {
                Debug.LogWarning("[EnvironmentalOrchestrator] Cannot optimize - invalid zone ID");
                return;
            }
            
            // Get current sensor readings
            var currentConditions = _sensorNetworkManager?.ReadZoneConditions(zoneId) ?? EnvironmentalConditions.CreateIndoorDefault();
            
            // Calculate optimal conditions for growth stage
            var optimalConditions = CalculateOptimalConditionsForStage(growthStage);
            
            // Coordinate optimization across all systems
            _climateControlManager?.OptimizeClimateForGrowth(zoneId, growthStage);
            _hvacSystemManager?.ControlEnvironment(zoneId, optimalConditions);
            _lightingSystemManager?.SetupPhotoperiodSchedule(zoneId, growthStage);
            
            _onEnvironmentalOptimization?.Raise(optimalConditions);
            
            Debug.Log($"[EnvironmentalOrchestrator] Optimized environment for zone '{zoneId}' (Stage: {growthStage})");
        }
        
        /// <summary>
        /// Gets comprehensive environmental conditions for a zone
        /// </summary>
        public EnvironmentalConditions GetZoneConditions(string zoneId)
        {
            // Primary source: sensor readings
            if (_sensorNetworkManager != null)
            {
                return _sensorNetworkManager.ReadZoneConditions(zoneId);
            }
            
            // Fallback to climate control manager
            if (_climateControlManager != null)
            {
                return _climateControlManager.GetZoneClimate(zoneId);
            }
            
            // Final fallback
            return EnvironmentalConditions.CreateIndoorDefault();
        }
        
        /// <summary>
        /// Updates environmental conditions for a zone across all systems
        /// </summary>
        public void SetZoneConditions(string zoneId, EnvironmentalConditions conditions)
        {
            _climateControlManager?.SetZoneClimate(zoneId, conditions);
            _hvacSystemManager?.ControlEnvironment(zoneId, conditions);
            
            _onConditionsChanged?.Raise(conditions);
            
            Debug.Log($"[EnvironmentalOrchestrator] Updated conditions for zone '{zoneId}'");
        }
        
        /// <summary>
        /// Performs comprehensive environmental analysis for a zone
        /// </summary>
        public EnvironmentalAnalysisResult AnalyzeZoneEnvironment(string zoneId)
        {
            var result = new EnvironmentalAnalysisResult
            {
                ZoneId = zoneId,
                AnalysisTimestamp = System.DateTime.Now
            };
            
            // Get current conditions
            result.CurrentConditions = GetZoneConditions(zoneId);
            
            // Analyze climate performance
            if (_climateControlManager != null)
            {
                var climateHistory = _climateControlManager.GetClimateHistory(zoneId);
                result.TemperatureStability = CalculateStability(climateHistory.GetAverageTemperature());
                result.HumidityStability = CalculateStability(climateHistory.GetAverageHumidity());
            }
            
            // Analyze HVAC performance
            if (_hvacSystemManager != null)
            {
                var hvacPerformance = _hvacSystemManager.GetPerformanceData(zoneId);
                result.PowerEfficiency = hvacPerformance.AverageEfficiency;
                result.PowerConsumption = hvacPerformance.TotalPowerConsumption;
            }
            
            // Analyze lighting performance
            if (_lightingSystemManager != null)
            {
                var lightingZone = _lightingSystemManager.GetLightingZone(zoneId);
                var lightingPerformance = _lightingSystemManager.GetPerformanceData(zoneId);
                result.LightingEfficiency = lightingPerformance.AverageEfficiency;
                result.PPFDAccuracy = CalculatePPFDAccuracy(lightingZone.TargetDLI, lightingZone.CurrentDLI);
            }
            
            // Analyze sensor network health
            if (_sensorNetworkManager != null)
            {
                var networkStatus = _sensorNetworkManager.GetNetworkStatus(zoneId);
                result.SensorNetworkHealth = networkStatus.NetworkHealth;
                result.DataReliability = CalculateDataReliability(networkStatus);
            }
            
            return result;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Coordinates environmental systems for optimal performance
        /// </summary>
        private void CoordinateEnvironmentalSystems()
        {
            // Identify all active zones across services
            var activeZones = GetAllActiveZones();
            
            foreach (string zoneId in activeZones)
            {
                CoordinateZoneSystems(zoneId);
            }
        }
        
        /// <summary>
        /// Coordinates systems for a specific zone
        /// </summary>
        private void CoordinateZoneSystems(string zoneId)
        {
            // Get current sensor readings
            var sensorConditions = _sensorNetworkManager?.ReadZoneConditions(zoneId);
            if (sensorConditions == null) return;
            
            // Check if HVAC and climate systems are aligned
            var climateConditions = _climateControlManager?.GetZoneClimate(zoneId);
            var hvacZone = _hvacSystemManager?.GetHVACZone(zoneId);
            
            if (climateConditions != null && hvacZone.HasValue)
            {
                // Coordinate HVAC to support climate targets
                if (!ConditionsAreAligned(climateConditions, hvacZone.Value.CurrentConditions))
                {
                    _hvacSystemManager?.ControlEnvironment(zoneId, climateConditions);
                }
            }
            
            // Coordinate lighting with growth requirements
            var lightingZone = _lightingSystemManager?.GetLightingZone(zoneId);
            if (lightingZone != null)
            {
                var optimalSpectrum = CalculateOptimalSpectrum(lightingZone.GrowthStage);
                _lightingSystemManager?.ControlLighting(zoneId, lightingZone.TargetDLI, optimalSpectrum);
            }
        }
        
        /// <summary>
        /// Deploys basic sensor suite for a new zone
        /// </summary>
        private void DeployBasicSensors(string zoneId)
        {
            if (_sensorNetworkManager == null) return;
            
            // Deploy essential sensors
            _sensorNetworkManager.DeploySensor(zoneId, SensorType.TempHumidity, Vector3.zero);
            _sensorNetworkManager.DeploySensor(zoneId, SensorType.CO2, Vector3.up);
            _sensorNetworkManager.DeploySensor(zoneId, SensorType.Light, Vector3.up * 2f);
        }
        
        /// <summary>
        /// Gets all active zones from all services
        /// </summary>
        private HashSet<string> GetAllActiveZones()
        {
            var zones = new HashSet<string>();
            
            // This would require adding zone enumeration methods to each service
            // For now, we'll use a placeholder approach
            
            return zones;
        }
        
        /// <summary>
        /// Calculates optimal conditions for a growth stage
        /// </summary>
        private EnvironmentalConditions CalculateOptimalConditionsForStage(PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case PlantGrowthStage.Seed:
                case PlantGrowthStage.Germination:
                    return new EnvironmentalConditions
                    {
                        Temperature = 24f,
                        Humidity = 65f,
                        CO2Level = 800f,
                        DailyLightIntegral = 15f,
                        AirflowRate = 0.3f
                    };
                    
                case PlantGrowthStage.Seedling:
                    return new EnvironmentalConditions
                    {
                        Temperature = 23f,
                        Humidity = 60f,
                        CO2Level = 900f,
                        DailyLightIntegral = 20f,
                        AirflowRate = 0.5f
                    };
                    
                case PlantGrowthStage.Vegetative:
                    return new EnvironmentalConditions
                    {
                        Temperature = 25f,
                        Humidity = 55f,
                        CO2Level = 1200f,
                        DailyLightIntegral = 35f,
                        AirflowRate = 0.8f
                    };
                    
                case PlantGrowthStage.Flowering:
                    return new EnvironmentalConditions
                    {
                        Temperature = 23f,
                        Humidity = 45f,
                        CO2Level = 1000f,
                        DailyLightIntegral = 40f,
                        AirflowRate = 1f
                    };
                    
                default:
                    return EnvironmentalConditions.CreateIndoorDefault();
            }
        }
        
        /// <summary>
        /// Calculates optimal light spectrum for growth stage
        /// </summary>
        private LightSpectrumData CalculateOptimalSpectrum(PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case PlantGrowthStage.Seedling:
                case PlantGrowthStage.Vegetative:
                    return new LightSpectrumData
                    {
                        RedPercent = 30f,
                        BluePercent = 40f,
                        GreenPercent = 20f,
                        FarRedPercent = 5f,
                        UVPercent = 5f
                    };
                    
                case PlantGrowthStage.Flowering:
                    return new LightSpectrumData
                    {
                        RedPercent = 50f,
                        BluePercent = 25f,
                        GreenPercent = 15f,
                        FarRedPercent = 8f,
                        UVPercent = 2f
                    };
                    
                default:
                    return new LightSpectrumData
                    {
                        RedPercent = 40f,
                        BluePercent = 30f,
                        GreenPercent = 20f,
                        FarRedPercent = 8f,
                        UVPercent = 2f
                    };
            }
        }
        
        /// <summary>
        /// Checks if environmental conditions are aligned
        /// </summary>
        private bool ConditionsAreAligned(EnvironmentalConditions a, EnvironmentalConditions b)
        {
            return Mathf.Abs(a.Temperature - b.Temperature) < 1f &&
                   Mathf.Abs(a.Humidity - b.Humidity) < 5f &&
                   Mathf.Abs(a.CO2Level - b.CO2Level) < 100f;
        }
        
        /// <summary>
        /// Calculates environmental stability metric
        /// </summary>
        private float CalculateStability(float averageValue)
        {
            // Simplified stability calculation - would be more complex in reality
            return Mathf.Clamp01(1f - (Random.Range(0f, 0.2f))); // 80-100% stability
        }
        
        /// <summary>
        /// Calculates PPFD accuracy
        /// </summary>
        private float CalculatePPFDAccuracy(float target, float actual)
        {
            if (target <= 0f) return 0f;
            float accuracy = 1f - (Mathf.Abs(target - actual) / target);
            return Mathf.Clamp01(accuracy);
        }
        
        /// <summary>
        /// Calculates data reliability from sensor network
        /// </summary>
        private float CalculateDataReliability(SensorNetworkStatus networkStatus)
        {
            if (networkStatus.TotalSensors == 0) return 0f;
            
            float activeRatio = (float)networkStatus.ActiveSensors / networkStatus.TotalSensors;
            float reliabilityPenalty = (float)networkStatus.MalfunctioningSensors / networkStatus.TotalSensors;
            
            return Mathf.Clamp01(activeRatio - reliabilityPenalty);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnClimateOptimized(string zoneId, EnvironmentalConditions conditions)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Climate optimized for zone '{zoneId}'");
        }
        
        private void OnClimateAlert(string zoneId, ClimateAlert alert)
        {
            _onEnvironmentalAlert?.Raise($"Climate alert in zone {zoneId}: {alert.Severity}");
            Debug.LogWarning($"[EnvironmentalOrchestrator] Climate alert in zone '{zoneId}': {alert.Severity}");
        }
        
        private void OnHVACEnvironmentControlled(string zoneId, EnvironmentalConditions conditions)
        {
            Debug.Log($"[EnvironmentalOrchestrator] HVAC controlled environment for zone '{zoneId}'");
        }
        
        private void OnHVACAlert(string zoneId, HVACAlert alert)
        {
            _onEnvironmentalAlert?.Raise($"HVAC alert in zone {zoneId}: {alert.Severity}");
            Debug.LogWarning($"[EnvironmentalOrchestrator] HVAC alert in zone '{zoneId}': {alert.Severity}");
        }
        
        private void OnLightingAlert(string zoneId, LightingAlert alert)
        {
            _onEnvironmentalAlert?.Raise($"Lighting alert in zone {zoneId}: {alert.Severity}");
            Debug.LogWarning($"[EnvironmentalOrchestrator] Lighting alert in zone '{zoneId}': {alert.Severity}");
        }
        
        private void OnSensorAlert(string zoneId, SensorAlert alert)
        {
            _onEnvironmentalAlert?.Raise($"Sensor alert in zone {zoneId}: {alert.Severity}");
            Debug.LogWarning($"[EnvironmentalOrchestrator] Sensor alert in zone '{zoneId}': {alert.Severity}");
        }
        
        private void OnTemperatureChanged(string zoneId, float temperature)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Temperature changed in zone '{zoneId}': {temperature:F1}°C");
        }
        
        private void OnHumidityChanged(string zoneId, float humidity)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Humidity changed in zone '{zoneId}': {humidity:F1}%");
        }
        
        private void OnPowerConsumptionChanged(string zoneId, float powerConsumption)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Power consumption in zone '{zoneId}': {powerConsumption:F0}W");
        }
        
        private void OnPPFDChanged(string zoneId, float ppfd)
        {
            Debug.Log($"[EnvironmentalOrchestrator] PPFD changed in zone '{zoneId}': {ppfd:F0} µmol/m²/s");
        }
        
        private void OnPhotoperiodChanged(string zoneId, PhotoperiodPhase phase)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Photoperiod phase in zone '{zoneId}': {phase}");
        }
        
        private void OnSensorDataReceived(string sensorId, EnvironmentalSensorReading reading)
        {
            // PC014-FIX-25: Fixed delegate signature to match SensorNetworkManager expectation
            // Could aggregate and process sensor data here
        }
        
        private void OnSensorStatusChanged(string sensorId, SensorStatus status)
        {
            Debug.Log($"[EnvironmentalOrchestrator] Sensor '{sensorId}' status: {status}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Comprehensive environmental analysis result
    /// </summary>
    [System.Serializable]
    public struct EnvironmentalAnalysisResult
    {
        public string ZoneId;
        public System.DateTime AnalysisTimestamp;
        public EnvironmentalConditions CurrentConditions;
        public float TemperatureStability;
        public float HumidityStability;
        public float PowerEfficiency;
        public float PowerConsumption;
        public float LightingEfficiency;
        public float PPFDAccuracy;
        public float SensorNetworkHealth;
        public float DataReliability;
        
        public float OverallScore => (TemperatureStability + HumidityStability + PowerEfficiency + 
                                     LightingEfficiency + PPFDAccuracy + SensorNetworkHealth + DataReliability) / 7f;
    }
}