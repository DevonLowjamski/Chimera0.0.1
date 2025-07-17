using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Controls environmental response visual effects for cannabis plants.
    /// Manages sophisticated VFX systems that visualize how plants respond to
    /// wind, light, water, temperature, humidity, and other environmental factors.
    /// </summary>
    public class EnvironmentalResponseVFXController : ChimeraManager, IVFXCompatibleSystem
    {
        [Header("Environmental VFX Configuration")]
        [SerializeField] private bool _enableEnvironmentalVFX = true;
        [SerializeField] private bool _enableRealtimeResponse = true;
        [SerializeField] private float _responseUpdateInterval = 0.1f;
        
        [Header("Wind Response Settings")]
        [SerializeField] private bool _enableWindEffects = true;
        [SerializeField] private Vector3 _windDirection = new Vector3(1f, 0f, 0.3f);
        [SerializeField, Range(0f, 10f)] private float _windStrength = 2.0f;
        [SerializeField, Range(0f, 5f)] private float _windTurbulence = 1.0f;
        [SerializeField] private AnimationCurve _windResponseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Light Response Settings")]
        [SerializeField] private bool _enableLightResponse = true;
        [SerializeField, Range(0f, 1000f)] private float _lightIntensity = 600f;
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField] private Vector3 _lightDirection = new Vector3(0f, -1f, 0.2f);
        [SerializeField] private AnimationCurve _phototropismCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Water Response Settings")]
        [SerializeField] private bool _enableWaterEffects = true;
        [SerializeField, Range(0f, 1f)] private float _soilMoisture = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _humidity = 0.55f;
        [SerializeField] private Color _waterVaporColor = new Color(0.7f, 0.9f, 1f, 0.3f);
        [SerializeField] private AnimationCurve _transpirationCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f);
        
        [Header("Temperature Response Settings")]
        [SerializeField] private bool _enableTemperatureEffects = true;
        [SerializeField, Range(15f, 35f)] private float _temperature = 24f;
        [SerializeField] private Color _heatDistortionColor = new Color(1f, 0.8f, 0.6f, 0.2f);
        [SerializeField] private Color _coldResponseColor = new Color(0.6f, 0.8f, 1f, 0.3f);
        [SerializeField] private AnimationCurve _thermalResponseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Atmospheric Response Settings")]
        [SerializeField] private bool _enableAtmosphericEffects = true;
        [SerializeField, Range(300f, 1500f)] private float _co2Level = 400f;
        [SerializeField, Range(0f, 2f)] private float _airflow = 1.0f;
        [SerializeField] private Color _co2VisualizationColor = new Color(0.8f, 1f, 0.8f, 0.2f);
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentResponses = 30;
        [SerializeField] private float _responseCullingDistance = 25f;
        [SerializeField] private bool _enableLODResponse = true;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Environmental Response State
        private Dictionary<string, EnvironmentalResponseInstance> _activeResponses = new Dictionary<string, EnvironmentalResponseInstance>();
        private Queue<string> _responseUpdateQueue = new Queue<string>();
        private List<EnvironmentalChangeEvent> _pendingEnvironmentalChanges = new List<EnvironmentalChangeEvent>();
        
        // VFX Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private PlantHealthVFXController _healthVFXController;
        private VFXCompatibilityLayer _vfxCompatibilityLayer;
        
        // Environmental Monitoring
        private EnvironmentalConditions _currentEnvironment;
        private EnvironmentalConditions _previousEnvironment;
        private Dictionary<EnvironmentalFactor, EnvironmentalFactorData> _environmentalFactors;
        private EnvironmentalVFXPerformanceMetrics _performanceMetrics;
        private float _lastEnvironmentalUpdate = 0f;
        private int _responsesProcessedThisFrame = 0;
        
        // Wind Simulation
        private WindSimulationData _windSimulation;
        private List<WindZone> _windZones = new List<WindZone>();
        
        // Events
        public System.Action<string, EnvironmentalFactor, float> OnEnvironmentalResponse;
        public System.Action<EnvironmentalConditions> OnEnvironmentalChange;
        public System.Action<string, EnvironmentalResponseInstance> OnResponseInstanceCreated;
        public System.Action<EnvironmentalVFXPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public EnvironmentalConditions CurrentEnvironment => _currentEnvironment;
        public Vector3 WindDirection => _windDirection;
        public float WindStrength => _windStrength;
        public float LightIntensity => _lightIntensity;
        public float Temperature => _temperature;
        public int ActiveResponses => _activeResponses.Count;
        public EnvironmentalVFXPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeEnvironmentalVFXSystem();
            InitializeEnvironmentalFactors();
            InitializeWindSimulation();
            InitializePerformanceTracking();
            ConnectToVFXManagers();
            StartEnvironmentalMonitoring();
            LogInfo("Environmental Response VFX Controller initialized");
        }
        
        private void Update()
        {
            if (_enableEnvironmentalVFX && Time.time - _lastEnvironmentalUpdate >= _responseUpdateInterval)
            {
                UpdateEnvironmentalConditions();
                ProcessResponseUpdateQueue();
                UpdateEnvironmentalEffects();
                UpdatePerformanceMetrics();
                _lastEnvironmentalUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeEnvironmentalVFXSystem()
        {
            LogInfo("=== INITIALIZING ENVIRONMENTAL RESPONSE VFX SYSTEM ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph available - environmental response effects enabled");
            #else
            LogWarning("⚠️ VFX Graph not available - using fallback environmental effects");
            #endif
            
            // Initialize current environment
            _currentEnvironment = new EnvironmentalConditions
            {
                Temperature = _temperature,
                Humidity = _humidity,
                LightIntensity = _lightIntensity,
                CO2Level = _co2Level,
                AirFlow = _airflow,
                BarometricPressure = 1013.25f, // Standard atmospheric pressure
                LightDirection = _lightDirection.y, // Use Y component for light direction angle
                WindDirection = _windDirection.y, // Use Y component for wind direction angle
                WindSpeed = _windStrength
            };
            
            _previousEnvironment = _currentEnvironment.Clone();
            
            // Initialize performance metrics
            _performanceMetrics = new EnvironmentalVFXPerformanceMetrics
            {
                ActiveResponses = 0,
                ResponseUpdatesPerSecond = 0f,
                AverageResponseTime = 0f,
                EnvironmentalChangeRate = 0f,
                LastUpdate = DateTime.Now
            };
            
            LogInfo("✅ Environmental VFX system initialized");
        }
        
        private void InitializeEnvironmentalFactors()
        {
            LogInfo("Setting up environmental factors...");
            
            _environmentalFactors = new Dictionary<EnvironmentalFactor, EnvironmentalFactorData>
            {
                [EnvironmentalFactor.Wind] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.Wind,
                    FactorName = "Wind",
                    CurrentValue = _windStrength,
                    OptimalRange = new Vector2(0.5f, 2.5f),
                    ResponseIntensity = 1.0f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "Wind movement and plant sway response"
                },
                
                [EnvironmentalFactor.Light] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.Light,
                    FactorName = "Light",
                    CurrentValue = _lightIntensity,
                    OptimalRange = new Vector2(400f, 800f),
                    ResponseIntensity = 0.8f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "Light intensity and phototropism response"
                },
                
                [EnvironmentalFactor.Water] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.Water,
                    FactorName = "Water",
                    CurrentValue = _soilMoisture,
                    OptimalRange = new Vector2(0.4f, 0.8f),
                    ResponseIntensity = 0.9f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "Water availability and transpiration response"
                },
                
                [EnvironmentalFactor.Temperature] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.Temperature,
                    FactorName = "Temperature",
                    CurrentValue = _temperature,
                    OptimalRange = new Vector2(20f, 28f),
                    ResponseIntensity = 0.7f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "Temperature stress and thermal response"
                },
                
                [EnvironmentalFactor.Humidity] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.Humidity,
                    FactorName = "Humidity",
                    CurrentValue = _humidity,
                    OptimalRange = new Vector2(0.45f, 0.65f),
                    ResponseIntensity = 0.6f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "Humidity levels and vapor exchange"
                },
                
                [EnvironmentalFactor.CO2] = new EnvironmentalFactorData
                {
                    Factor = EnvironmentalFactor.CO2,
                    FactorName = "CO2",
                    CurrentValue = _co2Level,
                    OptimalRange = new Vector2(600f, 1200f),
                    ResponseIntensity = 0.5f,
                    VFXType = CannabisVFXType.EnvironmentalResponse,
                    Description = "CO2 concentration and photosynthesis enhancement"
                }
            };
            
            LogInfo($"✅ Configured {_environmentalFactors.Count} environmental factors");
        }
        
        private void InitializeWindSimulation()
        {
            LogInfo("Setting up wind simulation system...");
            
            _windSimulation = new WindSimulationData
            {
                BaseWindDirection = _windDirection.normalized,
                BaseWindStrength = _windStrength,
                Turbulence = _windTurbulence,
                LastUpdate = Time.time,
                WindVariation = Vector3.zero,
                GustIntensity = 0f,
                GustDuration = 0f
            };
            
            // Find existing wind zones in the scene
            _windZones.AddRange(FindObjectsOfType<WindZone>());
            
            LogInfo($"✅ Wind simulation initialized with {_windZones.Count} wind zones");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        private void ConnectToVFXManagers()
        {
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            _healthVFXController = FindObjectOfType<PlantHealthVFXController>();
            _vfxCompatibilityLayer = FindObjectOfType<VFXCompatibilityLayer>();
            
            if (_vfxTemplateManager == null)
            {
                LogWarning("CannabisVFXTemplateManager not found");
            }
            
            if (_speedTreeIntegration == null)
            {
                LogWarning("SpeedTreeVFXIntegrationManager not found");
            }
            
            if (_healthVFXController == null)
            {
                LogWarning("PlantHealthVFXController not found");
            }
            
            if (_vfxCompatibilityLayer != null)
            {
                InitializeCompatibility(_vfxCompatibilityLayer);
                LogInfo("✅ VFX Compatibility Layer connected successfully");
            }
            else
            {
                LogWarning("⚠️ VFXCompatibilityLayer not found - environmental VFX may have compatibility issues");
            }
            
            LogInfo("✅ Connected to VFX management systems");
        }
        
        private void StartEnvironmentalMonitoring()
        {
            StartCoroutine(ContinuousEnvironmentalMonitoring());
            StartCoroutine(WindSimulationLoop());
            LogInfo("✅ Environmental monitoring started");
        }
        
        #endregion
        
        #region Environmental Response Instance Management
        
        public string CreateEnvironmentalResponseInstance(Transform plantTransform, PlantStrainSO strainData = null)
        {
            string instanceId = Guid.NewGuid().ToString();
            
            var responseInstance = new EnvironmentalResponseInstance
            {
                InstanceId = instanceId,
                PlantTransform = plantTransform,
                StrainData = strainData,
                IsActive = true,
                CreationTime = Time.time,
                LastUpdateTime = 0f,
                EnvironmentalSensitivity = new Dictionary<EnvironmentalFactor, float>(),
                VFXComponents = new Dictionary<EnvironmentalFactor, string>(),
                ResponseHistory = new List<EnvironmentalResponseEvent>()
            };
            
            // Initialize environmental sensitivity
            InitializeEnvironmentalSensitivity(responseInstance);
            
            // Create VFX components for environmental responses
            CreateEnvironmentalVFXComponents(responseInstance);
            
            // Apply strain-specific environmental traits
            if (strainData != null)
            {
                ApplyStrainEnvironmentalTraits(responseInstance, strainData);
            }
            
            _activeResponses[instanceId] = responseInstance;
            _responseUpdateQueue.Enqueue(instanceId);
            
            LogInfo($"Environmental response instance created: {instanceId.Substring(0, Math.Min(8, instanceId.Length))} for plant {plantTransform.name}");
            OnResponseInstanceCreated?.Invoke(instanceId, responseInstance);
            
            return instanceId;
        }
        
        private void InitializeEnvironmentalSensitivity(EnvironmentalResponseInstance instance)
        {
            // Initialize sensitivity to each environmental factor
            foreach (var factor in _environmentalFactors.Keys)
            {
                instance.EnvironmentalSensitivity[factor] = GetBaseSensitivity(factor);
            }
        }
        
        private float GetBaseSensitivity(EnvironmentalFactor factor)
        {
            return factor switch
            {
                EnvironmentalFactor.Wind => 0.8f,
                EnvironmentalFactor.Light => 1.0f,
                EnvironmentalFactor.Water => 0.9f,
                EnvironmentalFactor.Temperature => 0.7f,
                EnvironmentalFactor.Humidity => 0.6f,
                EnvironmentalFactor.CO2 => 0.5f,
                _ => 0.7f
            };
        }
        
        private void CreateEnvironmentalVFXComponents(EnvironmentalResponseInstance instance)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                foreach (var kvp in _environmentalFactors)
                {
                    var factor = kvp.Key;
                    var factorData = kvp.Value;
                    
                    // Create VFX instance for this environmental factor
                    string vfxInstanceId = _vfxTemplateManager.CreateVFXInstance(
                        factorData.VFXType,
                        instance.PlantTransform,
                        instance.PlantTransform.GetComponent<MonoBehaviour>()
                    );
                    
                    if (vfxInstanceId != null)
                    {
                        instance.VFXComponents[factor] = vfxInstanceId;
                        
                        // Configure VFX for this specific environmental factor
                        ConfigureEnvironmentalFactorVFX(vfxInstanceId, factor, factorData);
                    }
                }
            }
            #endif
        }
        
        private void ConfigureEnvironmentalFactorVFX(string vfxInstanceId, EnvironmentalFactor factor, EnvironmentalFactorData factorData)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                // Set factor-specific VFX parameters
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "EnvironmentalFactor", (float)factor);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ResponseIntensity", factorData.ResponseIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CurrentValue", factorData.CurrentValue);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "OptimalMin", factorData.OptimalRange.x);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "OptimalMax", factorData.OptimalRange.y);
            }
            #endif
        }
        
        private void ApplyStrainEnvironmentalTraits(EnvironmentalResponseInstance instance, PlantStrainSO strainData)
        {
            // Apply strain-specific environmental sensitivity
            float strainWindTolerance = 0.8f; // Would get from strain genetics
            float strainHeatTolerance = 0.7f; // Would get from strain genetics
            float strainDroughtTolerance = 0.6f; // Would get from strain genetics
            
            // Adjust sensitivity based on genetics
            instance.EnvironmentalSensitivity[EnvironmentalFactor.Wind] *= strainWindTolerance;
            instance.EnvironmentalSensitivity[EnvironmentalFactor.Temperature] *= strainHeatTolerance;
            instance.EnvironmentalSensitivity[EnvironmentalFactor.Water] *= (2f - strainDroughtTolerance); // Inverse for drought tolerance
            
            LogInfo($"Applied strain environmental traits to instance {instance.InstanceId.Substring(0, Math.Min(8, instance.InstanceId.Length))}: Wind={strainWindTolerance:F2}, Heat={strainHeatTolerance:F2}");
        }
        
        #endregion
        
        #region Environmental Response Processing
        
        private void UpdateEnvironmentalConditions()
        {
            // Store previous environment for change detection
            _previousEnvironment = _currentEnvironment.Clone();
            
            // Update current environmental conditions
            _currentEnvironment.Temperature = _temperature;
            _currentEnvironment.Humidity = _humidity;
            _currentEnvironment.LightIntensity = _lightIntensity;
            _currentEnvironment.CO2Level = _co2Level;
            _currentEnvironment.AirFlow = _airflow;
            _currentEnvironment.WindSpeed = _windStrength;
            _currentEnvironment.WindDirection = _windDirection.y; // Use Y component for wind direction angle
            _currentEnvironment.LightDirection = _lightDirection.y; // Use Y component for light direction angle
            
            // Update environmental factors
            _environmentalFactors[EnvironmentalFactor.Wind].CurrentValue = _windStrength;
            _environmentalFactors[EnvironmentalFactor.Light].CurrentValue = _lightIntensity;
            _environmentalFactors[EnvironmentalFactor.Water].CurrentValue = _soilMoisture;
            _environmentalFactors[EnvironmentalFactor.Temperature].CurrentValue = _temperature;
            _environmentalFactors[EnvironmentalFactor.Humidity].CurrentValue = _humidity;
            _environmentalFactors[EnvironmentalFactor.CO2].CurrentValue = _co2Level;
            
            // Detect significant changes
            DetectEnvironmentalChanges();
        }
        
        private void DetectEnvironmentalChanges()
        {
            bool significantChange = false;
            
            // Check for significant changes in each factor
            if (Mathf.Abs(_currentEnvironment.Temperature - _previousEnvironment.Temperature) > 1f)
            {
                TriggerEnvironmentalChangeEvent(EnvironmentalFactor.Temperature, _previousEnvironment.Temperature, _currentEnvironment.Temperature);
                significantChange = true;
            }
            
            if (Mathf.Abs(_currentEnvironment.LightIntensity - _previousEnvironment.LightIntensity) > 50f)
            {
                TriggerEnvironmentalChangeEvent(EnvironmentalFactor.Light, _previousEnvironment.LightIntensity, _currentEnvironment.LightIntensity);
                significantChange = true;
            }
            
            if (Mathf.Abs(_currentEnvironment.WindDirection - _previousEnvironment.WindDirection) > 0.2f ||
                Mathf.Abs(_currentEnvironment.WindSpeed - _previousEnvironment.WindSpeed) > 0.5f)
            {
                TriggerEnvironmentalChangeEvent(EnvironmentalFactor.Wind, _previousEnvironment.WindSpeed, _currentEnvironment.WindSpeed);
                significantChange = true;
            }
            
            if (significantChange)
            {
                OnEnvironmentalChange?.Invoke(_currentEnvironment);
            }
        }
        
        private void TriggerEnvironmentalChangeEvent(EnvironmentalFactor factor, float previousValue, float newValue)
        {
            var changeEvent = new EnvironmentalChangeEvent
            {
                Factor = factor,
                PreviousValue = previousValue,
                NewValue = newValue,
                ChangeTime = Time.time,
                ChangeRate = Mathf.Abs(newValue - previousValue) / Time.deltaTime
            };
            
            _pendingEnvironmentalChanges.Add(changeEvent);
            
            LogInfo($"Environmental change detected: {factor} changed from {previousValue:F2} to {newValue:F2}");
        }
        
        private void ProcessResponseUpdateQueue()
        {
            _responsesProcessedThisFrame = 0;
            
            // Process pending environmental changes
            ProcessPendingEnvironmentalChanges();
            
            // Update response instances in queue
            while (_responseUpdateQueue.Count > 0 && _responsesProcessedThisFrame < _maxConcurrentResponses)
            {
                string instanceId = _responseUpdateQueue.Dequeue();
                
                if (_activeResponses.ContainsKey(instanceId))
                {
                    UpdateResponseInstance(instanceId);
                    _responsesProcessedThisFrame++;
                    
                    // Re-queue for next update cycle
                    _responseUpdateQueue.Enqueue(instanceId);
                }
            }
        }
        
        private void ProcessPendingEnvironmentalChanges()
        {
            foreach (var changeEvent in _pendingEnvironmentalChanges)
            {
                ApplyEnvironmentalChangeToAllInstances(changeEvent);
            }
            
            _pendingEnvironmentalChanges.Clear();
        }
        
        private void ApplyEnvironmentalChangeToAllInstances(EnvironmentalChangeEvent changeEvent)
        {
            foreach (var instance in _activeResponses.Values)
            {
                ApplyEnvironmentalChangeToInstance(instance, changeEvent);
            }
        }
        
        private void ApplyEnvironmentalChangeToInstance(EnvironmentalResponseInstance instance, EnvironmentalChangeEvent changeEvent)
        {
            float sensitivity = instance.EnvironmentalSensitivity[changeEvent.Factor];
            float responseIntensity = changeEvent.ChangeRate * sensitivity;
            
            // Record response event
            var responseEvent = new EnvironmentalResponseEvent
            {
                Factor = changeEvent.Factor,
                ResponseTime = Time.time,
                ResponseIntensity = responseIntensity,
                EnvironmentalValue = changeEvent.NewValue,
                PlantResponse = CalculatePlantResponse(changeEvent.Factor, changeEvent.NewValue, sensitivity)
            };
            
            instance.ResponseHistory.Add(responseEvent);
            
            // Limit history size
            if (instance.ResponseHistory.Count > 50)
            {
                instance.ResponseHistory.RemoveAt(0);
            }
            
            // Update VFX based on response
            UpdateEnvironmentalVFX(instance, changeEvent.Factor, responseEvent);
            
            OnEnvironmentalResponse?.Invoke(instance.InstanceId, changeEvent.Factor, responseIntensity);
        }
        
        private float CalculatePlantResponse(EnvironmentalFactor factor, float environmentalValue, float sensitivity)
        {
            var factorData = _environmentalFactors[factor];
            float optimalRange = factorData.OptimalRange.y - factorData.OptimalRange.x;
            float optimalCenter = (factorData.OptimalRange.x + factorData.OptimalRange.y) * 0.5f;
            
            float deviation = Mathf.Abs(environmentalValue - optimalCenter) / (optimalRange * 0.5f);
            return Mathf.Clamp01(deviation * sensitivity);
        }
        
        private void UpdateResponseInstance(string instanceId)
        {
            var instance = _activeResponses[instanceId];
            
            // Skip if plant is too far away
            if (_enableLODResponse && IsInstanceCulled(instance))
            {
                SetInstanceVFXActive(instance, false);
                return;
            }
            
            SetInstanceVFXActive(instance, true);
            
            // Update all environmental factor responses
            foreach (var factor in _environmentalFactors.Keys)
            {
                UpdateFactorResponse(instance, factor);
            }
            
            instance.LastUpdateTime = Time.time;
        }
        
        private bool IsInstanceCulled(EnvironmentalResponseInstance instance)
        {
            if (Camera.main == null) return false;
            
            float distance = Vector3.Distance(Camera.main.transform.position, instance.PlantTransform.position);
            return distance > _responseCullingDistance;
        }
        
        private void SetInstanceVFXActive(EnvironmentalResponseInstance instance, bool active)
        {
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXComponents.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.SetVFXActive(vfxInstanceId, active);
                }
            }
            #endif
        }
        
        private void UpdateFactorResponse(EnvironmentalResponseInstance instance, EnvironmentalFactor factor)
        {
            if (!instance.VFXComponents.ContainsKey(factor)) return;
            
            string vfxInstanceId = instance.VFXComponents[factor];
            var factorData = _environmentalFactors[factor];
            float sensitivity = instance.EnvironmentalSensitivity[factor];
            
            switch (factor)
            {
                case EnvironmentalFactor.Wind:
                    UpdateWindResponse(vfxInstanceId, instance, sensitivity);
                    break;
                    
                case EnvironmentalFactor.Light:
                    UpdateLightResponse(vfxInstanceId, instance, sensitivity);
                    break;
                    
                case EnvironmentalFactor.Water:
                    UpdateWaterResponse(vfxInstanceId, instance, sensitivity);
                    break;
                    
                case EnvironmentalFactor.Temperature:
                    UpdateTemperatureResponse(vfxInstanceId, instance, sensitivity);
                    break;
                    
                case EnvironmentalFactor.Humidity:
                    UpdateHumidityResponse(vfxInstanceId, instance, sensitivity);
                    break;
                    
                case EnvironmentalFactor.CO2:
                    UpdateCO2Response(vfxInstanceId, instance, sensitivity);
                    break;
            }
        }
        
        #endregion
        
        #region Specific Environmental Factor Updates
        
        private void UpdateWindResponse(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_enableWindEffects && _vfxTemplateManager != null)
            {
                // Calculate wind effect on plant
                float windIntensity = _windStrength * sensitivity;
                Vector3 windDirectionWithVariation = _windDirection + _windSimulation.WindVariation;
                
                // Wind sway response
                float swayIntensity = _windResponseCurve.Evaluate(windIntensity / 5f);
                Vector3 swayDirection = windDirectionWithVariation.normalized;
                
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WindStrength", windIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WindDirection", windDirectionWithVariation);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "SwayIntensity", swayIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "Turbulence", _windTurbulence);
                
                // Leaf flutter and movement
                float leafFlutter = windIntensity * 0.5f + _windSimulation.GustIntensity;
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "LeafFlutter", leafFlutter);
                
                // Wind damage visualization at high intensities
                if (windIntensity > 4f)
                {
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WindStress", windIntensity - 4f);
                }
            }
            #endif
        }
        
        private void UpdateLightResponse(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_enableLightResponse && _vfxTemplateManager != null)
            {
                float lightResponse = _lightIntensity * sensitivity / 1000f;
                
                // Phototropism - plant orientation toward light
                Vector3 lightToPlant = (instance.PlantTransform.position - (_lightDirection * 10f)).normalized;
                float phototropismStrength = _phototropismCurve.Evaluate(lightResponse);
                
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "LightIntensity", _lightIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "LightDirection", _lightDirection);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "PhototropismStrength", phototropismStrength);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "LightColor", _lightColor);
                
                // Photosynthesis visualization
                float photosynthesisRate = Mathf.Clamp01(_lightIntensity / 800f) * sensitivity;
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "PhotosynthesisRate", photosynthesisRate);
                
                // Light stress at extreme intensities
                if (_lightIntensity > 900f)
                {
                    float lightStress = (_lightIntensity - 900f) / 100f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "LightStress", lightStress);
                }
            }
            #endif
        }
        
        private void UpdateWaterResponse(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_enableWaterEffects && _vfxTemplateManager != null)
            {
                // Transpiration effects
                float transpirationRate = _transpirationCurve.Evaluate(_soilMoisture) * sensitivity;
                float waterVaporIntensity = transpirationRate * (_temperature / 25f) * (1f - _humidity);
                
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "SoilMoisture", _soilMoisture);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TranspirationRate", transpirationRate);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WaterVaporIntensity", waterVaporIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WaterVaporColor", _waterVaporColor);
                
                // Drought stress visualization
                if (_soilMoisture < 0.3f)
                {
                    float droughtStress = (0.3f - _soilMoisture) / 0.3f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "DroughtStress", droughtStress);
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "WiltingEffect", droughtStress);
                }
                
                // Overwatering effects
                if (_soilMoisture > 0.9f)
                {
                    float overwaterStress = (_soilMoisture - 0.9f) / 0.1f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "OverwaterStress", overwaterStress);
                }
            }
            #endif
        }
        
        private void UpdateTemperatureResponse(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_enableTemperatureEffects && _vfxTemplateManager != null)
            {
                float thermalResponse = _thermalResponseCurve.Evaluate((_temperature - 20f) / 15f);
                
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "Temperature", _temperature);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ThermalResponse", thermalResponse * sensitivity);
                
                // Heat stress effects
                if (_temperature > 30f)
                {
                    float heatStress = (_temperature - 30f) / 5f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HeatStress", heatStress);
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HeatDistortionColor", _heatDistortionColor);
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HeatShimmer", heatStress * 0.5f);
                }
                
                // Cold stress effects
                if (_temperature < 18f)
                {
                    float coldStress = (18f - _temperature) / 8f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ColdStress", coldStress);
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ColdResponseColor", _coldResponseColor);
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "MetabolicSlowdown", coldStress);
                }
            }
            #endif
        }
        
        private void UpdateHumidityResponse(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "Humidity", _humidity);
                
                // VPD (Vapor Pressure Deficit) calculation for realistic transpiration
                float vpd = CalculateVPD(_temperature, _humidity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "VPD", vpd);
                
                // Humidity stress
                if (_humidity > 0.8f || _humidity < 0.3f)
                {
                    float humidityStress = _humidity > 0.8f ? 
                        (_humidity - 0.8f) / 0.2f : 
                        (0.3f - _humidity) / 0.3f;
                    humidityStress *= sensitivity;
                    
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HumidityStress", humidityStress);
                }
            }
            #endif
        }
        
        private void UpdateCO2Response(string vfxInstanceId, EnvironmentalResponseInstance instance, float sensitivity)
        {
            #if UNITY_VFX_GRAPH
            if (_enableAtmosphericEffects && _vfxTemplateManager != null)
            {
                float co2Enhancement = Mathf.Clamp01((_co2Level - 400f) / 800f);
                
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CO2Level", _co2Level);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CO2Enhancement", co2Enhancement * sensitivity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CO2VisualizationColor", _co2VisualizationColor);
                
                // Enhanced photosynthesis with elevated CO2
                if (_co2Level > 600f)
                {
                    float co2Boost = (_co2Level - 600f) / 600f * sensitivity;
                    _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CO2PhotosynthesisBoost", co2Boost);
                }
                
                // Air circulation effects
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "AirFlow", _airflow);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "CO2Distribution", _airflow * 0.5f);
            }
            #endif
        }
        
        private float CalculateVPD(float temperature, float humidity)
        {
            // Simplified VPD calculation
            float saturationVaporPressure = 610.7f * Mathf.Pow(10f, (7.5f * temperature) / (237.3f + temperature));
            float actualVaporPressure = saturationVaporPressure * humidity;
            return (saturationVaporPressure - actualVaporPressure) / 1000f; // Convert to kPa
        }
        
        #endregion
        
        #region Environmental Monitoring and Wind Simulation
        
        private IEnumerator ContinuousEnvironmentalMonitoring()
        {
            while (_enableRealtimeResponse)
            {
                yield return new WaitForSeconds(_responseUpdateInterval);
                
                // Simulate environmental variations
                SimulateEnvironmentalVariations();
                
                // Update all response instances
                foreach (var instance in _activeResponses.Values)
                {
                    MonitorInstanceEnvironmentalResponse(instance);
                }
            }
        }
        
        private IEnumerator WindSimulationLoop()
        {
            while (_enableWindEffects)
            {
                yield return new WaitForSeconds(0.05f); // 20Hz update rate
                
                UpdateWindSimulation();
            }
        }
        
        private void SimulateEnvironmentalVariations()
        {
            // Add natural variations to environmental factors
            float time = Time.time;
            
            // Wind variations
            _windSimulation.WindVariation = new Vector3(
                Mathf.Sin(time * 0.3f) * 0.2f,
                0f,
                Mathf.Cos(time * 0.4f) * 0.15f
            );
            
            // Random gusts
            if (UnityEngine.Random.Range(0f, 1f) < 0.05f) // 5% chance per update
            {
                _windSimulation.GustIntensity = UnityEngine.Random.Range(0.5f, 2f);
                _windSimulation.GustDuration = UnityEngine.Random.Range(1f, 3f);
            }
            
            // Light variations (clouds, etc.)
            float lightVariation = Mathf.Sin(time * 0.1f) * 50f;
            _lightIntensity = Mathf.Clamp(_lightIntensity + lightVariation * Time.deltaTime, 200f, 1000f);
            
            // Temperature variations
            float temperatureVariation = Mathf.Sin(time * 0.05f) * 0.5f;
            _temperature += temperatureVariation * Time.deltaTime;
            _temperature = Mathf.Clamp(_temperature, 18f, 32f);
        }
        
        private void UpdateWindSimulation()
        {
            float deltaTime = Time.time - _windSimulation.LastUpdate;
            
            // Update gust effects
            if (_windSimulation.GustDuration > 0f)
            {
                _windSimulation.GustDuration -= deltaTime;
                float gustProgress = 1f - (_windSimulation.GustDuration / 3f);
                _windSimulation.GustIntensity *= Mathf.Lerp(1f, 0f, gustProgress);
            }
            else
            {
                _windSimulation.GustIntensity = 0f;
            }
            
            // Apply wind to Unity wind zones
            foreach (var windZone in _windZones)
            {
                if (windZone != null)
                {
                    windZone.windMain = _windStrength + _windSimulation.GustIntensity;
                    windZone.windTurbulence = _windTurbulence;
                    windZone.transform.rotation = Quaternion.LookRotation(_windDirection + _windSimulation.WindVariation);
                }
            }
            
            _windSimulation.LastUpdate = Time.time;
        }
        
        private void MonitorInstanceEnvironmentalResponse(EnvironmentalResponseInstance instance)
        {
            // Check for extreme environmental conditions that require immediate response
            CheckForExtremeConditions(instance);
            
            // Update environmental stress indicators
            UpdateEnvironmentalStressIndicators(instance);
        }
        
        private void CheckForExtremeConditions(EnvironmentalResponseInstance instance)
        {
            // Check for conditions that could damage the plant
            if (_temperature > 35f || _temperature < 10f)
            {
                TriggerExtremeTemperatureResponse(instance);
            }
            
            if (_windStrength > 8f)
            {
                TriggerExtremeWindResponse(instance);
            }
            
            if (_soilMoisture < 0.1f)
            {
                TriggerExtremeWaterStressResponse(instance);
            }
        }
        
        private void TriggerExtremeTemperatureResponse(EnvironmentalResponseInstance instance)
        {
            LogWarning($"EXTREME TEMPERATURE: Plant {instance.InstanceId.Substring(0, Math.Min(8, instance.InstanceId.Length))} exposed to {_temperature:F1}°C");
            
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXComponents.Values)
            {
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ExtremeTemperatureAlert", 1f);
            }
            #endif
        }
        
        private void TriggerExtremeWindResponse(EnvironmentalResponseInstance instance)
        {
            LogWarning($"EXTREME WIND: Plant {instance.InstanceId.Substring(0, Math.Min(8, instance.InstanceId.Length))} exposed to {_windStrength:F1} wind strength");
            
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponents.ContainsKey(EnvironmentalFactor.Wind))
            {
                string vfxInstanceId = instance.VFXComponents[EnvironmentalFactor.Wind];
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ExtremeWindAlert", 1f);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "WindDamageRisk", 1f);
            }
            #endif
        }
        
        private void TriggerExtremeWaterStressResponse(EnvironmentalResponseInstance instance)
        {
            LogWarning($"EXTREME WATER STRESS: Plant {instance.InstanceId.Substring(0, Math.Min(8, instance.InstanceId.Length))} soil moisture at {_soilMoisture:F2}");
            
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponents.ContainsKey(EnvironmentalFactor.Water))
            {
                string vfxInstanceId = instance.VFXComponents[EnvironmentalFactor.Water];
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "ExtremeWaterStress", 1f);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "CriticalWilting", 1f);
            }
            #endif
        }
        
        private void UpdateEnvironmentalStressIndicators(EnvironmentalResponseInstance instance)
        {
            // Calculate overall environmental stress
            float overallStress = 0f;
            int stressFactors = 0;
            
            foreach (var factor in _environmentalFactors.Keys)
            {
                float factorStress = CalculateFactorStress(factor);
                if (factorStress > 0.1f)
                {
                    overallStress += factorStress;
                    stressFactors++;
                }
            }
            
            if (stressFactors > 0)
            {
                overallStress /= stressFactors;
                
                // Update health controller if available
                if (_healthVFXController != null)
                {
                    _healthVFXController.UpdatePlantHealth(instance.InstanceId, PlantHealthCategory.EnvironmentalStress, 1f - overallStress);
                }
            }
        }
        
        private float CalculateFactorStress(EnvironmentalFactor factor)
        {
            var factorData = _environmentalFactors[factor];
            float currentValue = factorData.CurrentValue;
            Vector2 optimalRange = factorData.OptimalRange;
            
            if (currentValue >= optimalRange.x && currentValue <= optimalRange.y)
            {
                return 0f; // No stress in optimal range
            }
            
            float stress = currentValue < optimalRange.x ?
                (optimalRange.x - currentValue) / optimalRange.x :
                (currentValue - optimalRange.y) / optimalRange.y;
            
            return Mathf.Clamp01(stress);
        }
        
        #endregion
        
        #region Performance and Effects Management
        
        private void UpdateEnvironmentalEffects()
        {
            // Update global environmental effects
            UpdateGlobalEnvironmentalIndicators();
            
            // Update adaptive quality if enabled
            if (_enableAdaptiveQuality)
            {
                UpdateAdaptiveQuality();
            }
        }
        
        private void UpdateGlobalEnvironmentalIndicators()
        {
            // Update global environmental visualization effects
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                // Global atmospheric effects
                _vfxTemplateManager.UpdateGlobalVFXParameter("GlobalWindStrength", _windStrength);
                _vfxTemplateManager.UpdateGlobalVFXParameter("GlobalLightIntensity", _lightIntensity);
                _vfxTemplateManager.UpdateGlobalVFXParameter("GlobalTemperature", _temperature);
                _vfxTemplateManager.UpdateGlobalVFXParameter("GlobalHumidity", _humidity);
            }
            #endif
        }
        
        private void UpdateAdaptiveQuality()
        {
            // Adjust VFX quality based on performance
            float currentFrameRate = 1f / Time.deltaTime;
            float targetFrameRate = 60f;
            
            if (currentFrameRate < targetFrameRate * 0.8f)
            {
                // Reduce quality
                ReduceEnvironmentalVFXQuality();
            }
            else if (currentFrameRate > targetFrameRate * 1.2f)
            {
                // Increase quality
                IncreaseEnvironmentalVFXQuality();
            }
        }
        
        private void ReduceEnvironmentalVFXQuality()
        {
            _maxConcurrentResponses = Mathf.Max(15, _maxConcurrentResponses - 2);
            _responseUpdateInterval = Mathf.Min(0.3f, _responseUpdateInterval + 0.05f);
        }
        
        private void IncreaseEnvironmentalVFXQuality()
        {
            _maxConcurrentResponses = Mathf.Min(50, _maxConcurrentResponses + 1);
            _responseUpdateInterval = Mathf.Max(0.05f, _responseUpdateInterval - 0.02f);
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveResponses = _activeResponses.Count;
            _performanceMetrics.ResponseUpdatesPerSecond = _responsesProcessedThisFrame / Time.deltaTime;
            _performanceMetrics.EnvironmentalChangeRate = _pendingEnvironmentalChanges.Count / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Environmental VFX Updates
        
        private void UpdateEnvironmentalVFX(EnvironmentalResponseInstance instance, EnvironmentalFactor factor, EnvironmentalResponseEvent responseEvent)
        {
            if (!instance.VFXComponents.ContainsKey(factor)) return;
            
            string vfxInstanceId = instance.VFXComponents[factor];
            
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                // Update VFX based on environmental response
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ResponseIntensity", responseEvent.ResponseIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "PlantResponse", responseEvent.PlantResponse);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "EnvironmentalValue", responseEvent.EnvironmentalValue);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "ResponseTime", responseEvent.ResponseTime);
            }
            #endif
        }
        
        #endregion
        
        #region Public Interface
        
        public EnvironmentalResponseInstance GetResponseInstance(string instanceId)
        {
            return _activeResponses.ContainsKey(instanceId) ? _activeResponses[instanceId] : null;
        }
        
        public List<EnvironmentalResponseInstance> GetAllResponseInstances()
        {
            return new List<EnvironmentalResponseInstance>(_activeResponses.Values);
        }
        
        public void SetWindConditions(Vector3 direction, float strength, float turbulence)
        {
            _windDirection = direction.normalized;
            _windStrength = strength;
            _windTurbulence = turbulence;
            
            LogInfo($"Wind conditions updated: Direction={direction}, Strength={strength:F2}, Turbulence={turbulence:F2}");
        }
        
        public void SetLightConditions(float intensity, Color color, Vector3 direction)
        {
            _lightIntensity = intensity;
            _lightColor = color;
            _lightDirection = direction.normalized;
            
            LogInfo($"Light conditions updated: Intensity={intensity:F2}, Direction={direction}");
        }
        
        public void SetAtmosphericConditions(float temperature, float humidity, float co2Level)
        {
            _temperature = temperature;
            _humidity = humidity;
            _co2Level = co2Level;
            
            LogInfo($"Atmospheric conditions updated: T={temperature:F1}°C, RH={humidity:F2}, CO2={co2Level:F0}ppm");
        }
        
        public void SetWaterConditions(float soilMoisture)
        {
            _soilMoisture = soilMoisture;
            
            LogInfo($"Water conditions updated: Soil Moisture={soilMoisture:F2}");
        }
        
        public void DestroyResponseInstance(string instanceId)
        {
            if (_activeResponses.ContainsKey(instanceId))
            {
                var instance = _activeResponses[instanceId];
                
                // Cleanup VFX instances
                foreach (string vfxInstanceId in instance.VFXComponents.Values)
                {
                    _vfxTemplateManager?.DestroyVFXInstance(vfxInstanceId);
                }
                
                _activeResponses.Remove(instanceId);
                LogInfo($"Environmental response instance destroyed: {instanceId.Substring(0, Math.Min(8, instanceId.Length))}");
            }
        }
        
        public EnvironmentalResponseReport GetEnvironmentalReport()
        {
            return new EnvironmentalResponseReport
            {
                ActiveResponses = _activeResponses.Count,
                CurrentEnvironment = _currentEnvironment,
                EnvironmentalFactors = new Dictionary<EnvironmentalFactor, float>(_environmentalFactors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CurrentValue)),
                WindSimulation = _windSimulation,
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Show Environmental Status")]
        public void ShowEnvironmentalStatus()
        {
            LogInfo("=== ENVIRONMENTAL RESPONSE VFX STATUS ===");
            LogInfo($"Active Response Instances: {_activeResponses.Count}");
            LogInfo($"Wind: {_windStrength:F2} strength, {_windDirection} direction");
            LogInfo($"Light: {_lightIntensity:F2} PPFD, {_lightDirection} direction");
            LogInfo($"Temperature: {_temperature:F1}°C");
            LogInfo($"Humidity: {_humidity:F2} RH");
            LogInfo($"CO2: {_co2Level:F0} ppm");
            LogInfo($"Soil Moisture: {_soilMoisture:F2}");
        }
        
        [ContextMenu("Simulate Weather Change")]
        public void SimulateWeatherChange()
        {
            // Simulate a weather front passing through
            _windStrength = UnityEngine.Random.Range(0.5f, 4f);
            _windDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            _temperature = UnityEngine.Random.Range(18f, 30f);
            _humidity = UnityEngine.Random.Range(0.3f, 0.8f);
            _lightIntensity = UnityEngine.Random.Range(200f, 900f);
            
            LogInfo("Weather change simulated");
        }
        
        [ContextMenu("Create Wind Storm")]
        public void CreateWindStorm()
        {
            _windStrength = 6f;
            _windTurbulence = 3f;
            _windDirection = new Vector3(1f, 0f, 0.5f).normalized;
            
            LogInfo("Wind storm created");
        }
        
        [ContextMenu("Reset Environmental Conditions")]
        public void ResetEnvironmentalConditions()
        {
            _windStrength = 2f;
            _windTurbulence = 1f;
            _windDirection = new Vector3(1f, 0f, 0.3f).normalized;
            _temperature = 24f;
            _humidity = 0.55f;
            _lightIntensity = 600f;
            _co2Level = 400f;
            _soilMoisture = 0.6f;
            
            LogInfo("Environmental conditions reset to defaults");
        }
        
        #endregion
        
        #region IVFXCompatibleSystem Implementation
        
        /// <summary>
        /// Initializes VFX compatibility layer integration for environmental response system.
        /// </summary>
        public void InitializeCompatibility(VFXCompatibilityLayer compatibilityLayer)
        {
            _vfxCompatibilityLayer = compatibilityLayer;
            
            // Register this system with the compatibility layer
            if (_vfxCompatibilityLayer != null)
            {
                LogInfo("🔌 Environmental Response VFX System connected to compatibility layer");
            }
        }
        
        /// <summary>
        /// Cleans up VFX compatibility layer integration.
        /// </summary>
        public void CleanupCompatibility()
        {
            _vfxCompatibilityLayer = null;
            LogInfo("🔌 Environmental Response VFX System disconnected from compatibility layer");
        }
        
        /// <summary>
        /// Updates environmental response based on new environmental conditions.
        /// </summary>
        public void UpdateEnvironmentalResponse(EnvironmentalConditions conditions)
        {
            if (conditions == null) return;
            
            // Update internal environmental state
            _currentEnvironment = conditions.Clone();
            
            // Process environmental updates through compatibility layer
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(conditions);
            }
            
            // Update all active response instances
            foreach (var kvp in _activeResponses)
            {
                var instance = kvp.Value;
                UpdateInstanceEnvironmentalResponse(instance, conditions);
            }
            
            LogInfo($"🌿 Updated environmental response for {_activeResponses.Count} plant instances");
        }
        
        /// <summary>
        /// Updates growth animation based on growth data.
        /// </summary>
        public void UpdateGrowthAnimation(GrowthAnimationData growthData)
        {
            if (growthData == null) return;
            
            // Find the corresponding plant instance
            if (_activeResponses.TryGetValue(growthData.PlantInstanceId, out var instance))
            {
                // Update environmental sensitivity based on growth stage
                UpdateGrowthStageEnvironmentalSensitivity(instance, growthData.GrowthStage, growthData.GrowthProgress);
                
                LogInfo($"🌱 Updated growth-based environmental response for plant {growthData.PlantInstanceId.Substring(0, Math.Min(8, growthData.PlantInstanceId.Length))}");
            }
        }
        
        /// <summary>
        /// Updates environmental response for a specific plant instance.
        /// </summary>
        private void UpdateInstanceEnvironmentalResponse(EnvironmentalResponseInstance instance, EnvironmentalConditions conditions)
        {
            // Initialize LastEnvironmentalValues if null
            if (instance.LastEnvironmentalValues == null)
            {
                instance.LastEnvironmentalValues = new EnvironmentalConditions();
            }
            
            // Update wind response
            if (_enableWindEffects && conditions.WindSpeed != instance.LastEnvironmentalValues.WindSpeed)
            {
                UpdateInstanceWindResponse(instance, conditions.WindSpeed, conditions.WindDirection);
            }
            
            // Update light response
            if (_enableLightResponse && conditions.LightIntensity != instance.LastEnvironmentalValues.LightIntensity)
            {
                UpdateInstanceLightResponse(instance, conditions.LightIntensity, conditions.LightDirection);
            }
            
            // Update water/humidity response
            if (_enableWaterEffects && conditions.Humidity != instance.LastEnvironmentalValues.Humidity)
            {
                UpdateInstanceWaterResponse(instance, conditions.Humidity);
            }
            
            // Update temperature response
            if (_enableTemperatureEffects && conditions.Temperature != instance.LastEnvironmentalValues.Temperature)
            {
                UpdateInstanceTemperatureResponse(instance, conditions.Temperature);
            }
            
            // Store updated environmental values
            instance.LastEnvironmentalValues = conditions.Clone();
        }
        
        /// <summary>
        /// Updates environmental sensitivity based on plant growth stage.
        /// </summary>
        private void UpdateGrowthStageEnvironmentalSensitivity(EnvironmentalResponseInstance instance, ProjectChimera.Data.Genetics.PlantGrowthStage growthStage, float growthProgress)
        {
            // Adjust environmental sensitivity based on growth stage
            switch (growthStage)
            {
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling:
                    // Seedlings are more sensitive to environmental changes
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Temperature] = 1.5f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Wind] = 2.0f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Water] = 1.8f;
                    break;
                    
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative:
                    // Vegetative plants are moderately sensitive
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Temperature] = 1.0f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Wind] = 1.0f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Light] = 1.2f;
                    break;
                    
                case ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering:
                    // Flowering plants are sensitive to environmental stress
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Humidity] = 1.5f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.Temperature] = 1.3f;
                    instance.EnvironmentalSensitivity[EnvironmentalFactor.CO2] = 1.2f;
                    break;
                    
                default:
                    // Default sensitivity values
                    foreach (var factor in System.Enum.GetValues(typeof(EnvironmentalFactor)).Cast<EnvironmentalFactor>())
                    {
                        instance.EnvironmentalSensitivity[factor] = 1.0f;
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Updates wind response for a specific plant instance.
        /// </summary>
        private void UpdateInstanceWindResponse(EnvironmentalResponseInstance instance, float windSpeed, float windDirection)
        {
            var windFactor = _environmentalFactors[EnvironmentalFactor.Wind];
            float sensitivity = instance.EnvironmentalSensitivity[EnvironmentalFactor.Wind];
            float responseIntensity = windFactor.ResponseIntensity * sensitivity;
            
            // Calculate wind response based on wind speed and direction
            float windEffect = Mathf.Clamp01(windSpeed / windFactor.OptimalRange.y) * responseIntensity;
            
            // Apply wind response through VFX compatibility layer if available
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(windEffect);
            }
        }
        
        /// <summary>
        /// Updates light response for a specific plant instance.
        /// </summary>
        private void UpdateInstanceLightResponse(EnvironmentalResponseInstance instance, float lightIntensity, float lightDirection)
        {
            var lightFactor = _environmentalFactors[EnvironmentalFactor.Light];
            float sensitivity = instance.EnvironmentalSensitivity[EnvironmentalFactor.Light];
            float responseIntensity = lightFactor.ResponseIntensity * sensitivity;
            
            // Calculate light response based on intensity and direction
            float lightEffect = Mathf.Clamp01(lightIntensity / lightFactor.OptimalRange.y) * responseIntensity;
            
            // Apply light response through VFX compatibility layer if available
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(lightEffect);
            }
        }
        
        /// <summary>
        /// Updates water/humidity response for a specific plant instance.
        /// </summary>
        private void UpdateInstanceWaterResponse(EnvironmentalResponseInstance instance, float humidity)
        {
            var waterFactor = _environmentalFactors[EnvironmentalFactor.Water];
            float sensitivity = instance.EnvironmentalSensitivity[EnvironmentalFactor.Water];
            float responseIntensity = waterFactor.ResponseIntensity * sensitivity;
            
            // Calculate water stress response
            float waterEffect = Mathf.Clamp01(Mathf.Abs(humidity - 0.6f) / 0.4f) * responseIntensity;
            
            // Apply water response through VFX compatibility layer if available
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(waterEffect);
            }
        }
        
        /// <summary>
        /// Updates temperature response for a specific plant instance.
        /// </summary>
        private void UpdateInstanceTemperatureResponse(EnvironmentalResponseInstance instance, float temperature)
        {
            var tempFactor = _environmentalFactors[EnvironmentalFactor.Temperature];
            float sensitivity = instance.EnvironmentalSensitivity[EnvironmentalFactor.Temperature];
            float responseIntensity = tempFactor.ResponseIntensity * sensitivity;
            
            // Calculate temperature stress response (optimal range is typically 20-26°C for cannabis)
            float optimalTemp = 23f; // Optimal temperature
            float tempStress = Mathf.Abs(temperature - optimalTemp) / 10f; // Normalize stress
            float tempEffect = Mathf.Clamp01(tempStress) * responseIntensity;
            
            // Apply temperature response through VFX compatibility layer if available
            if (_vfxCompatibilityLayer != null)
            {
                _vfxCompatibilityLayer.ProcessEnvironmentalResponse(tempEffect);
            }
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            
            // Cleanup all response instances
            var instanceIds = new List<string>(_activeResponses.Keys);
            foreach (string instanceId in instanceIds)
            {
                DestroyResponseInstance(instanceId);
            }
            
            CancelInvoke();
            LogInfo("Environmental Response VFX Controller shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum EnvironmentalFactor
    {
        Wind = 0,
        Light = 1,
        Water = 2,
        Temperature = 3,
        Humidity = 4,
        CO2 = 5
    }
    
    [System.Serializable]
    public class EnvironmentalResponseInstance
    {
        public string InstanceId;
        public Transform PlantTransform;
        public PlantStrainSO StrainData;
        public bool IsActive;
        public float CreationTime;
        public float LastUpdateTime;
        public Dictionary<EnvironmentalFactor, float> EnvironmentalSensitivity;
        public Dictionary<EnvironmentalFactor, string> VFXComponents;
        public List<EnvironmentalResponseEvent> ResponseHistory;
        public EnvironmentalConditions LastEnvironmentalValues;
    }
    
    [System.Serializable]
    public class EnvironmentalFactorData
    {
        public EnvironmentalFactor Factor;
        public string FactorName;
        public float CurrentValue;
        public Vector2 OptimalRange;
        public float ResponseIntensity;
        public CannabisVFXType VFXType;
        public string Description;
    }
    
    [System.Serializable]
    public class EnvironmentalChangeEvent
    {
        public EnvironmentalFactor Factor;
        public float PreviousValue;
        public float NewValue;
        public float ChangeTime;
        public float ChangeRate;
    }
    
    [System.Serializable]
    public class EnvironmentalResponseEvent
    {
        public EnvironmentalFactor Factor;
        public float ResponseTime;
        public float ResponseIntensity;
        public float EnvironmentalValue;
        public float PlantResponse;
    }
    
    [System.Serializable]
    public class WindSimulationData
    {
        public Vector3 BaseWindDirection;
        public float BaseWindStrength;
        public float Turbulence;
        public Vector3 WindVariation;
        public float GustIntensity;
        public float GustDuration;
        public float LastUpdate;
    }
    
    [System.Serializable]
    public class EnvironmentalVFXPerformanceMetrics
    {
        public int ActiveResponses;
        public float ResponseUpdatesPerSecond;
        public float AverageResponseTime;
        public float EnvironmentalChangeRate;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class EnvironmentalResponseReport
    {
        public int ActiveResponses;
        public EnvironmentalConditions CurrentEnvironment;
        public Dictionary<EnvironmentalFactor, float> EnvironmentalFactors;
        public WindSimulationData WindSimulation;
        public EnvironmentalVFXPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}